Imports System.Net.Http
Imports System.Text
Imports System.Xml
Imports AsyncOAuth

Public Class HatenaClient
    Private ReadOnly _consumerKey As String
    Private ReadOnly _consumerSecret As String
    Private ReadOnly _accessToken As AccessToken
    Private ReadOnly _hatenaId As String
    Private ReadOnly _blogId As String

    Private _textbox As TextBox = Nothing

    'HTTPレスポンスを見るためにテキストボックスを設定できる
    Public Property Textbox As TextBox
        Get
            Return _textbox
        End Get
        Set(value As TextBox)
            _textbox = value
        End Set
    End Property

    Public Sub New(consumerKey As String, consumerSecret As String, accesstoken As AccessToken, hatenaId As String, blogId As String)
        _consumerKey = consumerKey
        _consumerSecret = consumerSecret
        _accessToken = accesstoken
        _hatenaId = hatenaId
        _blogId = blogId
    End Sub

    Private Function CreateOAuthClient() As HttpClient
        Return OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken)
    End Function

    'scope は "read_public" で使える
    Public Async Function GetMy() As Task(Of String)
        Dim client = CreateOAuthClient()
        Dim json = Await client.GetStringAsync("http://n.hatena.com/applications/my.json")
        Return json
    End Function

    'いつ使う？
    Public Async Function ApplicationStart() As Task(Of String)
        Dim client = CreateOAuthClient()
        Dim response = Await client.PostAsync("http://n.hatena.com/applications/start", New StringContent("", Encoding.UTF8))
        Return Await response.Content.ReadAsStringAsync()
    End Function

    '返り値用クラス(記事の投稿)
    Public Class HttpResponseUrl
        Public code As Integer = 0 'http response code
        Public alternateUrl As String = ""
        Public editUrl As String = ""
        Public entry As String = ""
        Public bDraft As Boolean = False
        Public errorMessage As String = ""
    End Class

    '返り値用クラス(記事の読み込み)
    Public Class HttpResponseArticle
        Public code As Integer = 0      'http response code
        Public summary As String = ""   'summary of article max 140 chars
        Public categories As String()
        Public title As String = ""
        Public body As String = ""
        Public editMode As ContentEditMode = ContentEditMode.UNKNOWN
        Public bDraft As Boolean = False
        Public errorMessage As String = ""
    End Class

    '記事を更新する
    '公開している記事を更新で下書きには戻せない。HTTPレスポンスコード400
    Public Async Function UpdateArticle(entryId As String, title As String, body As String,
                                        categories As String(), draft As Boolean) As Task(Of HttpResponseUrl)
        Dim sb As New Text.StringBuilder
        sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>")
        sb.Append("<entry xmlns=""http://www.w3.org/2005/Atom"" xmlns:app=""http://www.w3.org/2007/app""><title>")
        sb.Append(System.Net.WebUtility.HtmlEncode(title))
        sb.Append("</title><author><name>")
        sb.Append(_hatenaId)
        sb.Append("</name></author><content type=""text/plain"">")
        sb.Append(System.Net.WebUtility.HtmlEncode(body))
        sb.Append("</content><updated>")
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd"))
        sb.Append("T")
        sb.Append(DateTime.Now.ToString("HH:mm:ss"))
        sb.Append("</updated>")
        If categories IsNot Nothing Then
            For Each cat As String In categories
                sb.Append("<category term=""")
                sb.Append(cat)
                sb.Append(""" />")
            Next
        End If
        sb.Append("<app:control><app:draft>")
        sb.Append(IIf(draft, "yes", "no"))
        sb.Append("</app:draft></app:control></entry>")

        '送信
        Dim client = CreateOAuthClient()
        Dim response = Await client.PutAsync("https://blog.hatena.ne.jp/" & _hatenaId & "/" & _blogId & "/atom/entry/" & entryId,
                                                  New StringContent(sb.ToString, Encoding.UTF8))
        Dim text As String = Await response.Content.ReadAsStringAsync()
        If _textbox IsNot Nothing Then _textbox.AppendText(text)

        'レスポンスを読み取る
        Dim r As New HttpResponseUrl
        r.code = response.StatusCode
        If r.code = 200 Then
            '成功
            If getUrl(text, r.editUrl, r.alternateUrl) Then
                r.entry = getEntryIdFromUrl(r.editUrl)
                r.bDraft = isDraft(text)
            Else
                r.errorMessage = "更新はできたけどURLを取得できない"
                Debug.Print("Can not get url at UpdateArticle()")
            End If
        Else
            r.errorMessage = "更新失敗 レスポンスコードエラー " & r.code
            Debug.Print("Response error at UpdateArticle() " & r.code)
        End If
        Return r
    End Function

    '記事を取得する
    Public Async Function DownloadArticle(entryId As String) As Task(Of HttpResponseArticle)
        Dim client = CreateOAuthClient()
        Dim response = Await client.GetAsync("https://blog.hatena.ne.jp/" &
                                             _hatenaId & "/" & _blogId & "/atom/entry/" & entryId)
        Dim text As String = Await response.Content.ReadAsStringAsync()
        If _textbox IsNot Nothing Then _textbox.AppendText(text)

        Dim r As New HttpResponseArticle
        r.code = response.StatusCode
        If r.code <> 200 Then
            r.errorMessage = "取得失敗。レスポンスコードエラー " & r.code
            Return r
        End If

        If Not getSummary(text, r.summary) Then
            r.errorMessage = "取得したが、サマリーを読めない"
            Return r
        End If

        'カテゴリー取得
        r.categories = getCategories(text)

        If Not getTitle(text, r.title) Then
            r.errorMessage = "取得したが、タイトルを読めない"
            Return r
        End If

        If Not getBody(text, r.body, r.editMode) Then
            r.errorMessage = "取得したが、本文を読めない"
            Return r
        End If

        'ドラフト
        r.bDraft = isDraft(text)

        Return r
    End Function

    '記事を投稿する
    Public Async Function UploadArticle(title As String, body As String,
                                        categories As String(), draft As Boolean) As Task(Of HttpResponseUrl)
        Dim sb As New Text.StringBuilder

        sb.Append("<?xml version=""1.0"" encoding=""utf-8""?>")
        sb.Append("<entry xmlns=""http://www.w3.org/2005/Atom"" xmlns:app=""http://www.w3.org/2007/app""><title>")
        sb.Append(System.Net.WebUtility.HtmlEncode(title))
        sb.Append("</title><author><name>")
        sb.Append(_hatenaId)
        sb.Append("</name></author><content type=""text/plain"">")
        sb.Append(System.Net.WebUtility.HtmlEncode(body))
        sb.Append("</content><updated>")
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd"))
        sb.Append("T")
        sb.Append(DateTime.Now.ToString("HH:mm:ss"))
        sb.Append("</updated>")
        If categories IsNot Nothing Then
            For Each cat As String In categories
                sb.Append("<category term=""")
                sb.Append(cat)
                sb.Append(""" />")
            Next
        End If
        sb.Append("<app:control><app:draft>")
        sb.Append(IIf(draft, "yes", "no"))
        sb.Append("</app:draft></app:control></entry>")

        Dim client = CreateOAuthClient()
        Dim response = Await client.PostAsync("https://blog.hatena.ne.jp/" & _hatenaId & "/" & _blogId & "/atom/entry",
                                                  New StringContent(sb.ToString, Encoding.UTF8))
        Dim text As String = Await response.Content.ReadAsStringAsync()
        If _textbox IsNot Nothing Then _textbox.AppendText(text)

        Dim r As New HttpResponseUrl
        r.code = response.StatusCode
        If r.code = 201 Then
            '成功
            If getUrl(text, r.editUrl, r.alternateUrl) Then
                r.entry = getEntryIdFromUrl(r.editUrl)
                r.bDraft = isDraft(text)
            Else
                r.errorMessage = "投稿はできたけどURLを取得できない"
                Debug.Print("Can not get url at UploadArticle()")
            End If
        Else
            r.errorMessage = "投稿失敗 レスポンスコードエラー " & r.code
            Debug.Print(r.errorMessage)
        End If
        Return r
    End Function

    'editUrl 編集用のURL。末尾の番号はエントリID
    'alternateUrl 公開用のURL。ドラフト保存時は見えない。末尾の番号はエントリID
    Private Shared Function getUrl(ByRef text As String, ByRef editUrl As String, ByRef alternateUrl As String) As Boolean
        Dim i As Integer = text.IndexOf("<link rel=""edit""")
        If i < 0 Then Return False
        i = text.IndexOf(" href=""", i)
        Dim j As Integer
        If i < 0 Then Return False
        i += 7
        j = text.IndexOf("""", i)
        If j < 0 Then Return False
        editUrl = text.Substring(i, j - i)

        i = text.IndexOf("<link rel=""alternate""")
        If i < 0 Then Return False
        i = text.IndexOf(" href=""", i)
        If i < 0 Then Return False
        i += 7
        j = text.IndexOf("""", i)
        If j < 0 Then Return False
        alternateUrl = text.Substring(i, j - i)
        Return True
    End Function

    Private Shared Function getEntryIdFromUrl(url As String) As String
        Dim i As Integer = url.LastIndexOf("/")
        If i < 0 Then Return ""
        Return url.Substring(i + 1)
    End Function

    Private Shared Function getTitle(ByRef text As String, ByRef title As String) As Boolean
        Dim i As Integer = text.IndexOf("<title>")
        If i < 0 Then Return False
        Dim j As Integer
        i += 7
        j = text.IndexOf("</title>", i)
        If j < 0 Then Return False
        title = System.Net.WebUtility.HtmlDecode(text.Substring(i, j - i))
        Return True
    End Function

    Private Shared Function getSummary(ByRef text As String, ByRef summary As String) As Boolean
        Dim i As Integer = text.IndexOf("<summary type=""text"">")
        If i < 0 Then Return False
        Dim j As Integer
        i += 21
        j = text.IndexOf("</summary>", i)
        If j < 0 Then Return False
        summary = System.Net.WebUtility.HtmlDecode(text.Substring(i, j - i))
        Return True
    End Function

    Private Shared Function getCategories(ByRef text As String) As String()
        Dim ar As New List(Of String)
        Dim j As Integer = 0
        Do
            Dim i As Integer = text.IndexOf("<category term=""", j)
            If i < 0 Then Exit Do

            i += 16

            j = text.IndexOf("""", i)
            If j < 0 Then Exit Do

            ar.Add(text.Substring(i, j - i))
            j += 1
        Loop
        Return ar.ToArray()
    End Function

    Public Enum ContentEditMode
        UNKNOWN
        MITAMAMA
        HATENA
        MARKDOWN
    End Enum

    Private Shared Function getBody(ByRef text As String,
                                       ByRef content As String, ByRef editMode As ContentEditMode) As Boolean

        Static tags As String() = {"<content type=""text/html"">", "<content type=""text/x-hatena-syntax"">", "<content type=""text/x-markdown"">"}
        Static types As ContentEditMode() = {ContentEditMode.MITAMAMA, ContentEditMode.HATENA, ContentEditMode.MARKDOWN}

        Dim i As Integer
        For k As Integer = 0 To UBound(tags)
            i = text.IndexOf(tags(k))
            If i >= 0 Then
                i += tags(k).Length
                editMode = types(k)
                Dim j As Integer
                j = text.IndexOf("</content>", i)
                If j < 0 Then Return False
                content = System.Net.WebUtility.HtmlDecode(text.Substring(i, j - i))
                Return True
            End If
        Next
        Return False
    End Function

    Private Shared Function isDraft(ByRef text As String) As Boolean
        Return (text.IndexOf("<app:draft>yes</app:draft>") >= 0)
    End Function

End Class


