'HatenaLoginで権限を取得
'HatenaClientで記事の操作

'ここではHatenaLoginを行い、HatenaClientが使用できる段階までをテスト

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        '権限取得
        'WebBrowser1を渡さない場合は裏で実行してくれる（といってもForm上に無残に表示されるので工夫は必要）
        'ButtonStepを渡しすとウェブブラウザを使用する部分をステップで進められる
        HatenaLogin.Open(Me, "read_public,read_private,write_private", WebBrowser1, ButtonStep)

        '完了するまでタイマーで監視
        Timer1.Start()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Static lastStatus As HatenaLogin.LOGIN_STATUS = HatenaLogin.LOGIN_STATUS.NONE

        Dim status As HatenaLogin.LOGIN_STATUS
        status = HatenaLogin.LoginStatus
        If status <> lastStatus Then
            lastStatus = status
            LabelLoginStatus.Text = status.ToString

            Select Case status
                Case HatenaLogin.LOGIN_STATUS.GOT_CLIENT
                    '目的達成 HatenaLogin.Client を使って記事の投稿とかできる
                    Debug.Print("成功した")
                    Timer1.Stop()
                    LoginDone()
                Case HatenaLogin.LOGIN_STATUS.LOGIN_ERROR
                    'ログイン失敗
                    Debug.Print("ログイン失敗")
                    Debug.Print(HatenaLogin.ErrorMessage)
                    Timer1.Stop()
                Case HatenaLogin.LOGIN_STATUS.PERMISSION_ERROR
                    '認証失敗
                    Debug.Print("認証失敗")
                    Debug.Print(HatenaLogin.ErrorMessage)
                    Timer1.Stop()
            End Select
        End If
    End Sub

    'ログインできたので記事UIを有効にする
    Private Sub LoginDone()
        ButtonStep.Enabled = False
        EnableArticleButtons(True)
        ComboBoxDraft.SelectedIndex = 0

        '確認用に投稿時のHTTPレスポンスをテキストボックスに出力する
        HatenaLogin.Client.Textbox = TextBoxHttpResponse
    End Sub

    '記事関連のボタンの有効化
    Private Sub EnableArticleButtons(enable As Boolean)
        ButtonNewArticle.Enabled = enable
        ButtonLoadArticle.Enabled = enable
        ButtonUploadArticle.Enabled = enable
    End Sub

    '新規記事
    Private Sub ButtonNewArticle_Click(sender As Object, e As EventArgs) Handles ButtonNewArticle.Click
        TextBoxEntry.Text = "" '記事エントリ番号を空にする
        TextBoxTitle.Text = ""
        TextBoxBody.Text = ""
    End Sub

    '読み込み
    Private Async Sub ButtonLoadArticle_Click(sender As Object, e As EventArgs) Handles ButtonLoadArticle.Click
        Dim entryId As String = InputBox("記事のエントリIDを入力してください")
        If entryId.Length = 0 Then Return

        TextBoxHttpResponse.Text = ""

        '記事の読み込みは非同期で行われるので、その間にボタンが押されないように無効化
        EnableArticleButtons(False)

        '記事の読み込み
        Dim r As HatenaClient.HttpResponseArticle = Await HatenaLogin.Client.DownloadArticle(entryId)

        EnableArticleButtons(True)

        If r.errorMessage.Length > 0 Then
            MsgBox("読み込めませんでした" & vbNewLine & r.errorMessage)
            Return
        End If

        'UIに反映

        'エントリID
        TextBoxEntry.Text = entryId

        'タイトル
        TextBoxTitle.Text = r.title

        'ドラフト
        ComboBoxDraft.SelectedIndex = IIf(r.bDraft, 1, 0)

        'カテゴリをカンマ区切りに
        Dim bFirst As Boolean = True
        Dim s As String = ""
        For Each cat As String In r.categories
            If bFirst Then
                bFirst = False
            Else
                s &= ","
            End If
            s &= cat
        Next
        TextBoxCategories.Text = s

        '本文
        TextBoxBody.Text = r.body
    End Sub

    '投稿
    Private Async Sub ButtonUploadArticle_Click(sender As Object, e As EventArgs) Handles ButtonUploadArticle.Click

        TextBoxHttpResponse.Text = ""

        'カテゴリを配列に
        Dim a As New List(Of String)
        For Each cat As String In TextBoxCategories.Text.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
            a.Add(cat)
        Next

        '下書きかどうか
        Dim bDraft As Boolean = (ComboBoxDraft.SelectedItem.ToString() = "下書き")

        If TextBoxEntry.Text.Length = 0 Then
            '新規投稿

            '記事の読み込みは非同期で行われるので、その間にボタンが押されないように無効化
            EnableArticleButtons(False)

            Dim r As HatenaClient.HttpResponseUrl = Await HatenaLogin.Client.UploadArticle(
                TextBoxTitle.Text, TextBoxBody.Text, a.ToArray(), bDraft)

            EnableArticleButtons(True)

            If r.errorMessage.Length > 0 Then
                MsgBox("投稿できませんでした" & vbNewLine & r.errorMessage)
                Return
            End If

            TextBoxEntry.Text = r.entry

            Debug.Print("編集アドレス：" & r.editUrl)
            If Not bDraft Then Debug.Print("公開アドレス：" & r.alternateUrl)

            MsgBox("投稿しました")
        Else
            '更新

            Dim r As HatenaClient.HttpResponseUrl = Await HatenaLogin.Client.UpdateArticle(
                TextBoxEntry.Text, TextBoxTitle.Text, TextBoxBody.Text, a.ToArray(), bDraft)

            EnableArticleButtons(True)

            If r.errorMessage.Length > 0 Then
                MsgBox("更新できませんでした" & vbNewLine & r.errorMessage)
                Return
            End If

            Debug.Print("編集アドレス：" & r.editUrl)
            If Not bDraft Then Debug.Print("公開アドレス：" & r.alternateUrl)

            MsgBox("更新しました")
        End If
    End Sub

End Class
