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

    'Public Async Function GetFromUrl(url As String) As Task(Of String)
    '    Dim client = CreateOAuthClient()
    '    Dim response = Await client.PostAsync(url, New StringContent("", Encoding.UTF8))
    '    Return Await response.Content.ReadAsStringAsync()
    'End Function

    Public Class HttpResponseUrl
        Public code As Integer = 0
        Public url As String = ""
        Public entry As String = ""
        Public errorMessage As String = ""
    End Class

    Public Class HttpResponseArticle
        Public code As Integer = 0
        Public summary As String = ""
        Public title As String = ""
        Public body As String = ""
        Public editMode As ContentEditMode = ContentEditMode.UNKNOWN
        Public errorMessage As String = ""
    End Class

    'すでに公開しているものはdraftできない
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

        Dim client = CreateOAuthClient()
        Dim response = Await client.PutAsync("https://blog.hatena.ne.jp/" & _hatenaId & "/" & _blogId & "/atom/entry/" & entryId,
                                                  New StringContent(sb.ToString, Encoding.UTF8))
        Dim text As String = Await response.Content.ReadAsStringAsync()

        Dim r As New HttpResponseUrl
        r.code = response.StatusCode
        If r.code = 200 Then
            'Created
            If Not getAlternateUrl(text, r.url, r.entry) Then
                r.errorMessage = "URLを取得できない"
                Debug.Print("Can not get url at UpdateArticle()")
            End If
        Else
            r.errorMessage = "レスポンスコードエラー " & r.code
            Debug.Print("Response error at UpdateArticle() " & r.code)
        End If
        Return r
    End Function

    Public Async Function DownloadArticle(entryId As String) As Task(Of HttpResponseArticle)
        Dim client = CreateOAuthClient()
        Dim response = Await client.GetAsync("https://blog.hatena.ne.jp/" &
                                             _hatenaId & "/" & _blogId & "/atom/entry/" & entryId)
        Dim text As String = Await response.Content.ReadAsStringAsync()

        Dim r As New HttpResponseArticle
        r.code = response.StatusCode
        If r.code <> 200 Then
            r.errorMessage = "コードエラー"
            Return r
        End If

        If Not getSummary(text, r.summary) Then
            r.errorMessage = "サマリー取得できない"
            Return r
        End If

        If Not getTitle(text, r.title) Then
            r.errorMessage = "タイトル取得できない"
            Return r
        End If

        If Not getBody(text, r.body, r.editMode) Then
            r.errorMessage = "本文を取得できない"
            Return r
        End If

        Return r
    End Function

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

        Dim r As New HttpResponseUrl
        r.code = response.StatusCode
        If r.code = 201 Then
            'Created
            If Not getAlternateUrl(text, r.url, r.entry) Then
                Debug.Print("Can not get url at UploadArticle()")
            End If
        Else
            Debug.Print("Response error at UploadArticle() " & r.code)
        End If
        Return r
    End Function

    Private Shared Function getAlternateUrl(ByRef text As String, ByRef url As String, ByRef entry As String) As Boolean
        Dim i As Integer = text.IndexOf("<link rel=""alternate""")
        If i < 0 Then Return False
        i = text.IndexOf(" href=""", i)
        Dim j As Integer
        If i < 0 Then Return False
        i += 7
        j = text.IndexOf("""", i)
        If j < 0 Then Return False
        url = text.Substring(i, j - i)
        i = url.LastIndexOf("/")
        If i < 0 Then Return False
        entry = url.Substring(i + 1)
        Return True
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

    Public Enum ContentEditMode
        UNKNOWN
        MITAMAMA
        HATENA
        MARKDOWN
    End Enum

    Private Shared Function getBody(ByRef text As String,
                                       ByRef content As String, ByRef editMode As ContentEditMode) As Boolean
        Dim i As Integer = text.IndexOf("<content type=""text/")
        If i < 0 Then Return False
        i += 20
        If text.Substring(i, 6) = "html"">" Then
            i += 5
            editMode = ContentEditMode.MITAMAMA
        ElseIf text.Substring(i, 17) = "x-hatena-syntax"">" Then
            i += 16
            editMode = ContentEditMode.HATENA
        ElseIf text.Substring(i, 12) = "x-markdown"">" Then
            i += 11
            editMode = ContentEditMode.MARKDOWN
        Else
            Return False
        End If
        Dim j As Integer
        j = text.IndexOf("</content>", i)
        If j < 0 Then Return False
        content = System.Net.WebUtility.HtmlDecode(text.Substring(i, j - i))
        Return True
    End Function

End Class


