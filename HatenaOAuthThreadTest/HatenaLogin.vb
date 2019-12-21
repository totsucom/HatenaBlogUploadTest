

Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Threading
Imports AsyncOAuth

'最初タスクでトライしたが、一連の作業をスレッドに置き換えたらかなりすっきりした

Public Class HatenaLogin

    Private Const URL_LOGIN_PAGE = "https://www.hatena.ne.jp/login"

    '必要な情報はここがすべて。扱い注意
    Private Const HATENA_USERNAME = "はてなブログにログインするユーザー名。メールアドレスなど" '秘密
    Private Const HATENA_PASSWORD = "はてなブログにログインするパスワード" '絶対秘密
    Private Const HATENA_ID As String = "はてなブログのID。ニックネーム後の括弧内のid"
    Private Const HATENA_BLOG_ID As String = "ブログID。例えば XXXX.hatenablog.com"
    Private Const CONSUMER_KEY As String = "HATENA DEVELOPER CENTER のアプリケーション登録で取得" '秘密
    Private Const CONSUMER_SECRET As String = "HATENA DEVELOPER CENTER のアプリケーション登録で取得" '絶対秘密

    '親フォーム
    Private Shared parentForm As Form = Nothing

    'ステップ実行する場合のボタン
    Private Shared stepButton As Button = Nothing

    'こっそり作成するウェブブラウザ
    Private Shared wb As WebBrowser = Nothing
    Private Shared wbPreparedByParent As Boolean = False

    Public Enum LOGIN_STATUS
        NONE
        GETTING_LOGIN_PAGE
        POSTING_PASSWORD
        GETTING_PERMISSION_PAGE
        GETTING_PINCODE
        GOT_CLIENT          '成功
        LOGIN_ERROR         '失敗
        PERMISSION_ERROR    '失敗
    End Enum

    Private Enum THREAD_STATUS
        NONE
        RUNNNING
        SUCCESS_EXIT    '成功
        ERROR_EXIT      '失敗
    End Enum

    Private Class ShareClass
        Public status As LOGIN_STATUS = LOGIN_STATUS.NONE
        Public bWebBrowserDocumentCompleted As Boolean = False 'ブラウザがhtmlを読み込んだ
        Public bStepButtonClicked As Boolean = False 'ステップ実行ボタンがクリックされた
        Public errorMessage As String = ""
        Public threadStatus As THREAD_STATUS = THREAD_STATUS.NONE
        Public client As HatenaClient = Nothing 'スレッドの目的
    End Class
    Private Shared shareData As ShareClass = Nothing

    Public Shared ReadOnly Property LoginStatus As LOGIN_STATUS
        Get
            SyncLock shareData
                Return shareData.status
            End SyncLock
        End Get
    End Property

    Public Shared ReadOnly Property ThreadStatus As LOGIN_STATUS
        Get
            SyncLock shareData
                Return shareData.threadStatus
            End SyncLock
        End Get
    End Property

    Public Shared ReadOnly Property ErrorMessage As String
        Get
            SyncLock shareData
                Return shareData.errorMessage
            End SyncLock
        End Get
    End Property

    Public Shared ReadOnly Property Client As HatenaClient
        Get
            SyncLock shareData
                Return shareData.client
            End SyncLock
        End Get
    End Property

    Private Class ThreadParam
        Public scope As String
    End Class

    'ログインを開始する
    'ログインの流れを確認したいときはフォームにWebBrowserコントロール配置して渡す
    'ウェブブラウザの動作をステップで見たいときはフォームにボタンを配置して渡す
    'StatusプロパティがGOT_CLIENT(成功)になったらClientプロパティでHatenaClientを取得して、自由に操作できる
    Public Shared Sub Open(form As Form, scope As String, Optional webBrowser As WebBrowser = Nothing, Optional stepTestButton As Button = Nothing)

        '初期化
        shareData = New ShareClass

        '親フォームを記憶
        parentForm = form

        'ボタンを記憶
        stepButton = stepTestButton
        If stepButton IsNot Nothing Then
            AddHandler stepButton.Click, AddressOf stepButton_click
        End If

        If webBrowser IsNot Nothing Then
            'ウェブブラウザが用意されていた
            wbPreparedByParent = True
            wb = webBrowser
        Else
            'ウェブブラウザを作成
            wbPreparedByParent = False
            wb = New WebBrowser
            parentForm.Controls.Add(wb)
        End If
        AddHandler wb.DocumentCompleted, AddressOf wb_documentCompleted

        Dim param As New ThreadParam
        param.scope = scope

        'スレッドを開始
        Dim t As Thread = New Thread(New ParameterizedThreadStart(AddressOf loginThread))
        t.Start(param)
    End Sub


    'ウェブブラウザがhtmlを読み終えたら呼び出される
    Private Shared Sub wb_documentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
        SyncLock shareData
            shareData.bWebBrowserDocumentCompleted = True 'フラグを立てる
        End SyncLock
    End Sub

    'ステップ実行ボタンが押された
    Private Shared Sub stepButton_click(sender As Object, e As EventArgs)
        SyncLock shareData
            shareData.bStepButtonClicked = True 'フラグを立てる
        End SyncLock
        DirectCast(sender, Button).Enabled = False '無効にする
    End Sub

    'ステップ実行ボタンが押されるのを待つ
    Private Shared Sub waitStepButton()
        If stepButton Is Nothing Then Return

        'ボタン有効
        SyncLock shareData
            shareData.bStepButtonClicked = False
        End SyncLock
        parentForm.Invoke(EnableStepButtonDelegate, New Object() {True})

        Do 'クリック待ち
            SyncLock shareData
                If shareData.bStepButtonClicked Then Exit Do
            End SyncLock
            Threading.Thread.Sleep(100)
        Loop

        'ボタン無効はクリックイベントハンドラで実行済
    End Sub

    'ウェブサイトが開くまで待つ
    Private Shared Sub waitWebSiteLoading()
        Do '開くのを待つ。コールバックの完了を待つのに他に方法ないか？
            SyncLock shareData
                If shareData.bWebBrowserDocumentCompleted Then Exit Do
            End SyncLock
            Threading.Thread.Sleep(100)
        Loop
        Threading.Thread.Sleep(300) '待ち時間が少ないと失敗しやすい（経験）
    End Sub

    'ログイン処理を進めるスレッド
    Private Shared Async Sub loginThread(threadParam As Object)
        Dim param As ThreadParam = DirectCast(threadParam, ThreadParam)

        'ボタン無効
        If stepButton IsNot Nothing Then parentForm.Invoke(EnableStepButtonDelegate, New Object() {False})

        'ステータスを設定
        SyncLock shareData
            shareData.threadStatus = THREAD_STATUS.RUNNNING
        End SyncLock

        'ログインページを開く
        SyncLock shareData
            shareData.status = LOGIN_STATUS.GETTING_LOGIN_PAGE 'ログインページ取得中
            shareData.bWebBrowserDocumentCompleted = False
        End SyncLock
        parentForm.Invoke(WebBrowserUrlDelegate, New Object() {URL_LOGIN_PAGE})

        waitWebSiteLoading()

        Dim msg As String = ""

        If Not parentForm.Invoke(IsAlreadyLoginDelegate) Then
            'ログインしていない
            Debug.Print("状態：ログインしていない")

            waitStepButton()

            'ユーザー、パスワードを入力
            msg = parentForm.Invoke(LoginHatena1Delegate)
            If msg.Length > 0 Then
                'うまくログイン入力できなかった
                SyncLock shareData
                    shareData.status = LOGIN_STATUS.LOGIN_ERROR 'ログインエラー
                    shareData.errorMessage = msg
                    shareData.threadStatus = THREAD_STATUS.ERROR_EXIT 'エラー終了
                End SyncLock
                parentForm.Invoke(ReleaseResourcesDelegate)
                Exit Sub
            End If

            waitStepButton()

            '送信をクリック
            msg = parentForm.Invoke(LoginHatena2Delegate)
            If msg.Length > 0 Then
                'うまくログイン入力できなかった
                SyncLock shareData
                    shareData.status = LOGIN_STATUS.LOGIN_ERROR 'ログインエラー
                    shareData.errorMessage = msg
                    shareData.threadStatus = THREAD_STATUS.ERROR_EXIT 'エラー終了
                End SyncLock
                parentForm.Invoke(ReleaseResourcesDelegate)
                Exit Sub
            End If

            SyncLock shareData
                shareData.status = LOGIN_STATUS.POSTING_PASSWORD 'ユーザー、パスワードを送信中
                shareData.bWebBrowserDocumentCompleted = False
            End SyncLock

            waitWebSiteLoading()

            msg = parentForm.Invoke(GetLoginErrorDelegate)
            If msg.Length > 0 Then
                'ログイン失敗
                SyncLock shareData
                    shareData.status = LOGIN_STATUS.LOGIN_ERROR 'ログインエラー
                    shareData.errorMessage = msg
                    shareData.threadStatus = THREAD_STATUS.ERROR_EXIT 'エラー終了
                End SyncLock
                parentForm.Invoke(ReleaseResourcesDelegate)
                Exit Sub
            End If
        Else
            Debug.Print("状態：ログインしている")

            waitStepButton()
        End If

        'ここからOAuth認証らしい（...）

        'ハッシュ計算関数
        OAuthUtility.ComputeHash = Function(key, buffer)
                                       Using hmac = New HMACSHA1(key)
                                           Return hmac.ComputeHash(buffer)
                                       End Using
                                   End Function

        Dim authorizer = New OAuthAuthorizer(CONSUMER_KEY, CONSUMER_SECRET)

        'リクエストトークンを取得
        Dim tokenResponse = Await authorizer.GetRequestToken(
            "https://www.hatena.com/oauth/initiate",
            New KeyValuePair(Of String, String)() {New KeyValuePair(Of String, String)("oauth_callback", "oob")},
            New FormUrlEncodedContent(New KeyValuePair(Of String, String)() {New KeyValuePair(Of String, String)("scope", param.scope)}))
        Dim requestToken As RequestToken = tokenResponse.Token()
        Dim pinRequestUrl As String = authorizer.BuildAuthorizeUrl("https://www.hatena.ne.jp/oauth/authorize", requestToken)

        '認証ページを開く
        SyncLock shareData
            shareData.status = LOGIN_STATUS.GETTING_PERMISSION_PAGE 'ログインページ取得中
            shareData.bWebBrowserDocumentCompleted = False
        End SyncLock
        parentForm.Invoke(WebBrowserUrlDelegate, New Object() {pinRequestUrl})

        waitWebSiteLoading()

        waitStepButton()

        '許可するボタンをクリック
        msg = parentForm.Invoke(GivePermissionDelegate)
        If msg.Length > 0 Then
            'うまくクリックできなかった
            SyncLock shareData
                shareData.status = LOGIN_STATUS.PERMISSION_ERROR '認証エラー
                shareData.errorMessage = msg
                shareData.threadStatus = THREAD_STATUS.ERROR_EXIT 'エラー終了
            End SyncLock
            parentForm.Invoke(ReleaseResourcesDelegate)
            Exit Sub
        End If
        SyncLock shareData
            shareData.status = LOGIN_STATUS.GETTING_PINCODE 'ピンコードページを取得中
            shareData.bWebBrowserDocumentCompleted = False
        End SyncLock

        waitWebSiteLoading()

        waitStepButton()

        'ピンコードを取得する
        Dim pinCode As PinCodeResult = parentForm.Invoke(GetPinCodeDelegate)
        If pinCode.pinCode.Length = 0 Then
            'うまく取得できなかった
            SyncLock shareData
                shareData.status = LOGIN_STATUS.PERMISSION_ERROR '認証エラー
                shareData.errorMessage = msg
                shareData.threadStatus = THREAD_STATUS.ERROR_EXIT 'エラー終了
            End SyncLock
            parentForm.Invoke(ReleaseResourcesDelegate)
            Exit Sub
        End If

        'アクセストークンを取得する
        Dim accessTokenResponse = Await authorizer.GetAccessToken(
                "https://www.hatena.com/oauth/token", requestToken, pinCode.pinCode)
        Dim accessToken As AccessToken = accessTokenResponse.Token

        '成功終了
        SyncLock shareData
            shareData.client = New HatenaClient(CONSUMER_KEY, CONSUMER_SECRET, accessToken, HATENA_ID, HATENA_BLOG_ID)
            shareData.status = LOGIN_STATUS.GOT_CLIENT 'クライアントを取得した
            shareData.errorMessage = ""
            shareData.threadStatus = THREAD_STATUS.SUCCESS_EXIT '成功終了
        End SyncLock
        parentForm.Invoke(ReleaseResourcesDelegate)
    End Sub


    '###########################
    'ブラウザ操作はデリゲートで行う
    '以下デリゲート関数
    '###########################

    'ウェブブラウザでURLを開く
    Private Delegate Sub DelegateSetWebBrowserUrl(url As String)
    Private Shared WebBrowserUrlDelegate As New DelegateSetWebBrowserUrl(AddressOf SetWebBrowserUrl)
    Private Shared Sub SetWebBrowserUrl(url As String)
        wb.Url = New Uri(url)
    End Sub

    'はてなログインページを見てすでにログインしているか調べる
    'Booleanで返す
    Private Delegate Function DelegateIsAlreadyLogin() As Boolean
    Private Shared IsAlreadyLoginDelegate As New DelegateIsAlreadyLogin(AddressOf isAlreadyLogin)
    Private Shared Function isAlreadyLogin() As Boolean
        For Each elem As HtmlElement In wb.Document.GetElementsByTagName("p")
            Dim html As String = elem.InnerHtml()
            If html.IndexOf("(id:" & HATENA_ID & ")") >= 0 AndAlso html.IndexOf("ログインしています") >= 0 Then
                Return True
            End If
        Next
        Return False
    End Function

    'はてなログインページに入力を実行
    '返り値が空文字なら成功
    Private Delegate Function DelegateLoginHatena1() As Boolean
    Private Shared LoginHatena1Delegate As New DelegateLoginHatena1(AddressOf loginHatena1)
    Private Shared Function loginHatena1() As String

        'ユーザー名を入力
        Dim elem As HtmlElement = wb.Document.GetElementById("login-name")
        If elem Is Nothing Then Return "ユーザー名の場所が見つかりません"
        elem.SetAttribute("value", HATENA_USERNAME)

        'パスワードを入力
        Dim bFound As Boolean = False
        For Each elem In wb.Document.GetElementsByTagName("input")
            If elem.GetAttribute("type") = "password" And elem.GetAttribute("name") = "password" Then
                elem.SetAttribute("value", HATENA_PASSWORD)
                bFound = True
                Exit For
            End If
        Next
        If Not bFound Then Return "パスワードの場所が見つかりません"

        Return ""
    End Function

    'はてなログインページの送信を実行
    '返り値が空文字なら成功
    Private Delegate Function DelegateLoginHatena2() As Boolean
    Private Shared LoginHatena2Delegate As New DelegateLoginHatena2(AddressOf loginHatena2)
    Private Shared Function loginHatena2() As String

        '送信ボタンを押す
        For Each elem As HtmlElement In wb.Document.GetElementsByTagName("input")
            If elem.GetAttribute("type") = "submit" And elem.GetAttribute("value") = "送信する" Then
                elem.InvokeMember("Click")
                Return "" '成功
            End If
        Next
        Return "送信ボタンが見つかりません"
    End Function


    'はてなログインページ後にエラーがあるとメッセージを返す
    '返り値が空文字なら成功
    Private Delegate Function DelegateGetLoginError() As Boolean
    Private Shared GetLoginErrorDelegate As New DelegateGetLoginError(AddressOf getLoginError)
    Private Shared Function getLoginError() As String
        For Each elem As HtmlElement In wb.Document.GetElementsByTagName("div")
            If elem.GetAttribute("className") = "error-message" Then
                Return elem.InnerHtml()
            End If
        Next
        Return "" '成功
    End Function


    'はてな認証ページの許可するボタンをクリックする
    '返り値が空文字なら成功
    Private Delegate Function DelegateGivePermission() As String
    Private Shared GivePermissionDelegate As New DelegateGivePermission(AddressOf givePermission)
    Private Shared Function givePermission() As String
        For Each elem As HtmlElement In wb.Document.GetElementsByTagName("input")
            If elem.GetAttribute("type") = "submit" And elem.GetAttribute("value") = "許可する" Then
                elem.InvokeMember("click")
                Return "" '成功
            End If
        Next
        Return "許可するボタンが見つかりません"
    End Function

    Private Class PinCodeResult
        Public pinCode As String = ""
        Public errorMessage As String = ""
    End Class

    'はてな認証後のピンコードを取得する
    '返り値のクラスのerrorMessageが空文字なら成功
    Private Delegate Function DelegateGetPinCode() As PinCodeResult
    Private Shared GetPinCodeDelegate As New DelegateGetPinCode(AddressOf getPinCode)
    Private Shared Function getPinCode() As PinCodeResult
        Dim r As New PinCodeResult

        Dim ec As HtmlElementCollection = wb.Document.GetElementsByTagName("pre")
        If ec.Count = 1 Then
            r.pinCode = ec(0).InnerHtml()
            Return r '取得した
        End If

        For Each elem As HtmlElement In wb.Document.GetElementsByTagName("div")
            If elem.GetAttribute("className") = "Error-Message" Then
                r.errorMessage = elem.InnerHtml()
                Return r
            End If
        Next

        r.errorMessage = "ピンコードをうまく読めない"
        Return r
    End Function

    'ステップ実行ボタンを有効にする
    Private Delegate Sub DelegateEnableStepButton(enable As Boolean)
    Private Shared EnableStepButtonDelegate As New DelegateEnableStepButton(AddressOf enableStepButton)
    Private Shared Sub enableStepButton(enable As Boolean)
        If stepButton IsNot Nothing Then
            stepButton.Enabled = enable
        End If
    End Sub

    'リソースをリリースする
    Private Delegate Sub DelegateReleaseResources()
    Private Shared ReleaseResourcesDelegate As New DelegateReleaseResources(AddressOf releaseResources)
    Private Shared Sub releaseResources()

        'イベントハンドラを削除
        If stepButton IsNot Nothing Then
            RemoveHandler stepButton.Click, AddressOf stepButton_click
            stepButton = Nothing
        End If
        RemoveHandler wb.DocumentCompleted, AddressOf wb_documentCompleted

        'ウェブブラウザを削除
        If Not wbPreparedByParent Then
            parentForm.Controls.Remove(wb)
            wb.Dispose()
            wb = Nothing
        End If
    End Sub


End Class
