Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Threading
Imports AsyncOAuth

'参考
'https://elve.hateblo.jp/entry/2017/04/18/210709


Public Class Form1
    ' set your token
    Private Const ConsumerKey As String = "C/eEl0qILbDQGQ=="
    Private Const ConsumerSecret As String = "tpWXScLJHHmeRKeOZcX3RG7th0k="
    Private Const HatenaId As String = "mae8bit"
    Private Const HatenaBlogId As String = "mae8bit.hatenablog.com" 'mae8bit.hatenablog.com または mae8bit-twe.hateblo.jp
    Private Const HatenaUserName As String = "maeoka8bit@gmail.com"
    Private Const HatenaPassword As String = "ia16777216"

    Private authorizer As OAuthAuthorizer = Nothing
    Private client As HatenaClient = Nothing

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'ハッシュを計算してるらしい
        OAuthUtility.ComputeHash = Function(key, buffer)
                                       Using hmac = New HMACSHA1(key)
                                           Return hmac.ComputeHash(buffer)
                                       End Using
                                   End Function

        HatenaLogin.Open(WebBrowser1, HatenaUserName, HatenaPassword)

        Timer1.Start()
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) _
        Handles WebBrowser1.DocumentCompleted

        With WebBrowser1
            TextBoxHtmlSource.Text = .DocumentText
        End With
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Static stp As Integer = 0

        If stp = 0 Then
            Select Case HatenaLogin.Status
                Case HatenaLogin.LOGIN_STATUS.LOGIN_ERROR
                    Timer1.Stop()
                    MsgBox("ログイン失敗 LOGIN_ERROR")
                Case HatenaLogin.LOGIN_STATUS.WEBPAGE_PARSE_ERROR
                    Timer1.Stop()
                    MsgBox("ログイン失敗 WEBPAGE_PARSE_ERROR")
                Case HatenaLogin.LOGIN_STATUS.LOGIN_SUCCESS
                    Task.Run(
                        Sub()
                            HatenaPermission.OpenRequestUrl(WebBrowser1, ConsumerKey, ConsumerSecret,
                                                            "read_public,read_private,write_private")
                        End Sub)
                    stp = 1
            End Select

        ElseIf stp = 1 Then
            Select Case HatenaPermission.Status
                Case HatenaPermission.PERMISSION_STATUS.MISS_PERMISSION
                    Timer1.Stop()
                    MsgBox("許可取得できず MISS_PERMISSION")
                Case HatenaPermission.PERMISSION_STATUS.WEBPAGE_PARSE_ERROR
                    Timer1.Stop()
                    MsgBox("許可取得できず WEBPAGE_PARSE_ERROR")
                Case HatenaPermission.PERMISSION_STATUS.GOT_PERMISSION
                    authorizer = New OAuthAuthorizer(ConsumerKey, ConsumerSecret)
                    Task.Run(
                        Sub()
                            HatenaPermission.GetAccessToken(authorizer)
                        End Sub)
                    stp = 2
            End Select

        ElseIf stp = 2 Then
            If HatenaPermission.Status = HatenaPermission.PERMISSION_STATUS.GOT_ACCESS_TOKEN Then
                client = New HatenaClient(ConsumerKey, ConsumerSecret, HatenaPermission.AccessToken, HatenaId, HatenaBlogId)
                Debug.Print("はてなclientの準備ができた")
                stp = 3
            End If
        End If
    End Sub

    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim r As String = Await client.GetMy()
        Debug.Print(r)
    End Sub

    Private Async Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        'Dim r As HatenaClient.HttpResponseUrl = Await client.UploadArticle("TEST2", "*これはテスト" & vbNewLine & "+あ" & vbNewLine & "+い" & vbNewLine & "+う", {"TWELITE"}, True)
        'Debug.Print(r.code)
        'Debug.Print(r.url)

        Dim r As HatenaClient.HttpResponseArticle = Await client.DownloadArticle("26006613486496807")
        Debug.Print("================")
        Debug.Print(r.code)
        Debug.Print(r.title)
        Debug.Print(r.editMode.ToString)
        Debug.Print("================")
        Debug.Print(r.summary)
        Debug.Print("================")
        Debug.Print(r.body)
        Debug.Print("================")
        Debug.Print(r.errorMessage)

        If r.errorMessage.Length = 0 Then
            Dim s As HatenaClient.HttpResponseUrl = Await client.UpdateArticle("26006613486496807",
                   r.title & "+", r.body & vbNewLine & "+だ", Nothing, False)
            Debug.Print(s.code)
            Debug.Print(s.url)
        End If

    End Sub
End Class

