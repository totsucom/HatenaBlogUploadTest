Imports System.Net.Http
Imports AsyncOAuth

Public Class HatenaPermission
    Public Enum PERMISSION_STATUS
        NONE
        GETTING_WEBPAGE
        SUBMITTING_PERMISSION
        GOT_PERMISSION
        GOT_ACCESS_TOKEN
        MISS_PERMISSION
        WEBPAGE_PARSE_ERROR
    End Enum
    Private Shared _status As PERMISSION_STATUS = PERMISSION_STATUS.NONE
    Private Shared _pinCode As String
    Private Shared _requestToken As RequestToken
    Private Shared _accessToken As AccessToken

    'OAuth状態を取得する
    Public Shared ReadOnly Property Status As PERMISSION_STATUS
        Get
            Return _status
        End Get
    End Property

    Public Shared ReadOnly Property AccessToken As AccessToken
        Get
            Return _accessToken
        End Get
    End Property

    '許可ページを開く
    'ウェブブラウザはHatenaLoginを成功させておくこと
    Public Shared Async Sub OpenRequestUrl(wb As WebBrowser, consumerkey As String, consumerSecret As String, scope As String)
        Dim pinRequestUrl As String = Await GetRequestUrl(consumerkey, consumerSecret, scope)
        wb.Url = New Uri(pinRequestUrl)
        Debug.Print("pinRequestUrl=" & pinRequestUrl)
        _status = PERMISSION_STATUS.GETTING_WEBPAGE

        AddHandler wb.DocumentCompleted, AddressOf OpenRequestUrl_DocumentCompleted
    End Sub

    Private Shared Sub OpenRequestUrl_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
        Dim wb = DirectCast(sender, WebBrowser)

        If _status = PERMISSION_STATUS.GETTING_WEBPAGE Then
            '許可するボタンをクリック
            Dim bFound As Boolean = False
            For Each elem As HtmlElement In wb.Document.GetElementsByTagName("input")
                If elem.GetAttribute("type") = "submit" And elem.GetAttribute("value") = "許可する" Then
                    elem.InvokeMember("Click")
                    bFound = True
                    Exit For
                End If
            Next
            If Not bFound Then
                Debug.Print("許可するボタンが見つかりません")
                _status = PERMISSION_STATUS.WEBPAGE_PARSE_ERROR
                RemoveHandler wb.DocumentCompleted, AddressOf OpenRequestUrl_DocumentCompleted
                Return
            End If

            _status = PERMISSION_STATUS.SUBMITTING_PERMISSION

        ElseIf _status = PERMISSION_STATUS.SUBMITTING_PERMISSION Then
            'pinCodeを取得する
            Dim ec As HtmlElementCollection = wb.Document.GetElementsByTagName("pre")
            If ec.Count = 1 Then
                _pinCode = ec(0).InnerHtml()
                _status = PERMISSION_STATUS.GOT_PERMISSION
            ElseIf ec.Count > 1 Then
                _status = PERMISSION_STATUS.WEBPAGE_PARSE_ERROR
            Else
                Dim bFound As Boolean = False
                For Each elem As HtmlElement In wb.Document.GetElementsByTagName("div")
                    If elem.GetAttribute("class") = "Error-Message" Then
                        Debug.Print(elem.InnerHtml())
                        bFound = True
                        Exit For
                    End If
                Next
                If bFound Then
                    _status = PERMISSION_STATUS.MISS_PERMISSION
                Else
                    _status = PERMISSION_STATUS.WEBPAGE_PARSE_ERROR
                End If
            End If

            Debug.Print(_status)

            RemoveHandler wb.DocumentCompleted, AddressOf OpenRequestUrl_DocumentCompleted
        End If
    End Sub

    Private Shared Async Function GetRequestUrl(consumerkey As String, consumerSecret As String, Optional scope As String = "read_public") As Task(Of String)
        ' create authorizer
        Dim authorizer = New OAuthAuthorizer(consumerkey, consumerSecret)

        ' get request token
        Dim tokenResponse = Await authorizer.GetRequestToken(
            "https://www.hatena.com/oauth/initiate",
            New KeyValuePair(Of String, String)() {New KeyValuePair(Of String, String)("oauth_callback", "oob")},
            New FormUrlEncodedContent(New KeyValuePair(Of String, String)() {New KeyValuePair(Of String, String)("scope", scope)}))

        _requestToken = tokenResponse.Token()
        Return authorizer.BuildAuthorizeUrl("https://www.hatena.ne.jp/oauth/authorize", _requestToken)
    End Function


    Public Shared Async Sub GetAccessToken(authorizer As OAuthAuthorizer)

        ' get access token
        Dim accessTokenResponse = Await authorizer.GetAccessToken("https://www.hatena.com/oauth/token", _requestToken, _pinCode)

        ' save access token.
        _accessToken = accessTokenResponse.Token

        _status = PERMISSION_STATUS.GOT_ACCESS_TOKEN
    End Sub

End Class
