
Imports TweliteZoneArticle

Public Class HatenaLogin
    Public Enum LOGIN_STATUS
        NONE
        GETTING_WEBPAGE
        POSTING_PASSWORD
        LOGIN_SUCCESS
        WEBPAGE_PARSE_ERROR
        LOGIN_ERROR
    End Enum
    Private Shared _status As LOGIN_STATUS = LOGIN_STATUS.NONE

    Private Shared _hatenaUsername As String
    Private Shared _hatenaPassword As String

    'ログイン状態を取得する
    Public Shared ReadOnly Property Status As LOGIN_STATUS
        Get
            Return _status
        End Get
    End Property

    'ブラウザにはてなブログにログインさせる
    Public Shared Sub Open(wb As WebBrowser, hatenaUsername As String, hatenaPassword As String)
        wb.Url = New Uri("https://www.hatena.ne.jp/login")
        _hatenaUsername = hatenaUsername
        _hatenaPassword = hatenaPassword
        _status = LOGIN_STATUS.GETTING_WEBPAGE

        AddHandler wb.DocumentCompleted, AddressOf OpenHatenaLogin_DocumentCompleted
    End Sub


    'ブラウザにはてなブログにログインさせる
    Private Shared Sub OpenHatenaLogin_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
        Dim wb = DirectCast(sender, WebBrowser)

        If _status = LOGIN_STATUS.GETTING_WEBPAGE Then
            Try
                'ユーザー名を入力
                wb.Document.GetElementById("login-name").SetAttribute("Value", _hatenaUsername)

                'パスワードを入力
                Dim bFound As Boolean = False
                For Each elem As HtmlElement In wb.Document.GetElementsByTagName("input")
                    If elem.GetAttribute("type") = "password" And elem.GetAttribute("name") = "password" Then
                        elem.SetAttribute("Value", _hatenaPassword)
                        bFound = True
                        Exit For
                    End If
                Next
                If Not bFound Then
                    Debug.Print("パスワード入力場所が見つかりません")
                    _status = LOGIN_STATUS.WEBPAGE_PARSE_ERROR
                    RemoveHandler wb.DocumentCompleted, AddressOf OpenHatenaLogin_DocumentCompleted
                    Return
                End If

                '送信ボタンを押す
                bFound = False
                For Each elem As HtmlElement In wb.Document.GetElementsByTagName("input")
                    If elem.GetAttribute("type") = "submit" And elem.GetAttribute("value") = "送信する" Then
                        elem.InvokeMember("Click")
                        bFound = True
                        Exit For
                    End If
                Next
                If Not bFound Then
                    Debug.Print("送信ボタンが見つかりません")
                    _status = LOGIN_STATUS.WEBPAGE_PARSE_ERROR
                    RemoveHandler wb.DocumentCompleted, AddressOf OpenHatenaLogin_DocumentCompleted
                    Return
                End If
            Catch ex As Exception
                Debug.Print(ex.Message)
                _status = LOGIN_STATUS.WEBPAGE_PARSE_ERROR
                RemoveHandler wb.DocumentCompleted, AddressOf OpenHatenaLogin_DocumentCompleted
                Return
            End Try

            _status = LOGIN_STATUS.POSTING_PASSWORD

        ElseIf _status = LOGIN_STATUS.POSTING_PASSWORD Then

            Dim bFound As Boolean = False
            For Each elem As HtmlElement In wb.Document.GetElementsByTagName("div")
                If elem.GetAttribute("className") = "error-message" Then
                    Dim p As HtmlElement = elem.GetElementsByTagName("p")(0)
                    Debug.Print(p.InnerHtml())
                    bFound = True
                    Exit For
                End If
            Next
            If bFound Then
                _status = LOGIN_STATUS.LOGIN_ERROR
            Else
                _status = LOGIN_STATUS.LOGIN_SUCCESS
            End If

            RemoveHandler wb.DocumentCompleted, AddressOf OpenHatenaLogin_DocumentCompleted
        End If

    End Sub
End Class
