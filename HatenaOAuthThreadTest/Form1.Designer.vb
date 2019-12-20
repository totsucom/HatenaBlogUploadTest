<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.LabelLoginStatus = New System.Windows.Forms.Label()
        Me.WebBrowser1 = New System.Windows.Forms.WebBrowser()
        Me.ButtonStep = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TextBoxTitle = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TextBoxBody = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TextBoxCategories = New System.Windows.Forms.TextBox()
        Me.ButtonUploadArticle = New System.Windows.Forms.Button()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.ButtonNewArticle = New System.Windows.Forms.Button()
        Me.ButtonLoadArticle = New System.Windows.Forms.Button()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.TextBoxHttpResponse = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ComboBoxDraft = New System.Windows.Forms.ComboBox()
        Me.TextBoxEntry = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'Timer1
        '
        '
        'LabelLoginStatus
        '
        Me.LabelLoginStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LabelLoginStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.LabelLoginStatus.Location = New System.Drawing.Point(104, 155)
        Me.LabelLoginStatus.Name = "LabelLoginStatus"
        Me.LabelLoginStatus.Size = New System.Drawing.Size(199, 23)
        Me.LabelLoginStatus.TabIndex = 0
        Me.LabelLoginStatus.Text = "Label1"
        Me.LabelLoginStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'WebBrowser1
        '
        Me.WebBrowser1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.WebBrowser1.Location = New System.Drawing.Point(12, 34)
        Me.WebBrowser1.MinimumSize = New System.Drawing.Size(20, 20)
        Me.WebBrowser1.Name = "WebBrowser1"
        Me.WebBrowser1.Size = New System.Drawing.Size(487, 118)
        Me.WebBrowser1.TabIndex = 1
        '
        'ButtonStep
        '
        Me.ButtonStep.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonStep.Location = New System.Drawing.Point(373, 158)
        Me.ButtonStep.Name = "ButtonStep"
        Me.ButtonStep.Size = New System.Drawing.Size(126, 24)
        Me.ButtonStep.TabIndex = 2
        Me.ButtonStep.Text = "ログインステップ実行"
        Me.ButtonStep.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 160)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(86, 12)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "ログインステータス"
        '
        'TextBoxTitle
        '
        Me.TextBoxTitle.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxTitle.Location = New System.Drawing.Point(67, 238)
        Me.TextBoxTitle.Name = "TextBoxTitle"
        Me.TextBoxTitle.Size = New System.Drawing.Size(350, 19)
        Me.TextBoxTitle.TabIndex = 4
        '
        'Label3
        '
        Me.Label3.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 241)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(40, 12)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "タイトル"
        '
        'Label4
        '
        Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 311)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(354, 12)
        Me.Label4.TabIndex = 6
        Me.Label4.Text = "本文（書式[見たまま|はてな記法|Markdown]はブログの設定に依存します）"
        '
        'TextBoxBody
        '
        Me.TextBoxBody.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxBody.Location = New System.Drawing.Point(32, 326)
        Me.TextBoxBody.Multiline = True
        Me.TextBoxBody.Name = "TextBoxBody"
        Me.TextBoxBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextBoxBody.Size = New System.Drawing.Size(385, 93)
        Me.TextBoxBody.TabIndex = 7
        '
        'Label5
        '
        Me.Label5.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 266)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(116, 12)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "カテゴリー(カンマ区切り)"
        '
        'TextBoxCategories
        '
        Me.TextBoxCategories.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxCategories.Location = New System.Drawing.Point(67, 280)
        Me.TextBoxCategories.Name = "TextBoxCategories"
        Me.TextBoxCategories.Size = New System.Drawing.Size(350, 19)
        Me.TextBoxCategories.TabIndex = 8
        '
        'ButtonUploadArticle
        '
        Me.ButtonUploadArticle.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonUploadArticle.Enabled = False
        Me.ButtonUploadArticle.Location = New System.Drawing.Point(423, 379)
        Me.ButtonUploadArticle.Name = "ButtonUploadArticle"
        Me.ButtonUploadArticle.Size = New System.Drawing.Size(76, 40)
        Me.ButtonUploadArticle.TabIndex = 10
        Me.ButtonUploadArticle.Text = "投稿"
        Me.ButtonUploadArticle.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 9)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(154, 12)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "ログインに使用するウェブブラウザ"
        '
        'ButtonNewArticle
        '
        Me.ButtonNewArticle.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonNewArticle.Enabled = False
        Me.ButtonNewArticle.Location = New System.Drawing.Point(12, 209)
        Me.ButtonNewArticle.Name = "ButtonNewArticle"
        Me.ButtonNewArticle.Size = New System.Drawing.Size(75, 23)
        Me.ButtonNewArticle.TabIndex = 12
        Me.ButtonNewArticle.Text = "新規記事"
        Me.ButtonNewArticle.UseVisualStyleBackColor = True
        '
        'ButtonLoadArticle
        '
        Me.ButtonLoadArticle.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButtonLoadArticle.Enabled = False
        Me.ButtonLoadArticle.Location = New System.Drawing.Point(106, 209)
        Me.ButtonLoadArticle.Name = "ButtonLoadArticle"
        Me.ButtonLoadArticle.Size = New System.Drawing.Size(75, 23)
        Me.ButtonLoadArticle.TabIndex = 13
        Me.ButtonLoadArticle.Text = "読み込み"
        Me.ButtonLoadArticle.UseVisualStyleBackColor = True
        '
        'Label7
        '
        Me.Label7.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(197, 214)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(49, 12)
        Me.Label7.TabIndex = 14
        Me.Label7.Text = "エントリID"
        '
        'TextBoxHttpResponse
        '
        Me.TextBoxHttpResponse.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxHttpResponse.Location = New System.Drawing.Point(12, 450)
        Me.TextBoxHttpResponse.Multiline = True
        Me.TextBoxHttpResponse.Name = "TextBoxHttpResponse"
        Me.TextBoxHttpResponse.ReadOnly = True
        Me.TextBoxHttpResponse.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextBoxHttpResponse.Size = New System.Drawing.Size(487, 106)
        Me.TextBoxHttpResponse.TabIndex = 16
        '
        'Label1
        '
        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 435)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(126, 12)
        Me.Label1.TabIndex = 17
        Me.Label1.Text = "投稿時のHTTPレスポンス"
        '
        'ComboBoxDraft
        '
        Me.ComboBoxDraft.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBoxDraft.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxDraft.FormattingEnabled = True
        Me.ComboBoxDraft.Items.AddRange(New Object() {"公開", "下書き"})
        Me.ComboBoxDraft.Location = New System.Drawing.Point(423, 326)
        Me.ComboBoxDraft.Name = "ComboBoxDraft"
        Me.ComboBoxDraft.Size = New System.Drawing.Size(76, 20)
        Me.ComboBoxDraft.TabIndex = 18
        '
        'TextBoxEntry
        '
        Me.TextBoxEntry.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.TextBoxEntry.Location = New System.Drawing.Point(252, 211)
        Me.TextBoxEntry.Name = "TextBoxEntry"
        Me.TextBoxEntry.ReadOnly = True
        Me.TextBoxEntry.Size = New System.Drawing.Size(167, 19)
        Me.TextBoxEntry.TabIndex = 19
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(511, 563)
        Me.Controls.Add(Me.TextBoxEntry)
        Me.Controls.Add(Me.ComboBoxDraft)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBoxHttpResponse)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.ButtonLoadArticle)
        Me.Controls.Add(Me.ButtonNewArticle)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.ButtonUploadArticle)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.TextBoxCategories)
        Me.Controls.Add(Me.TextBoxBody)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.TextBoxTitle)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ButtonStep)
        Me.Controls.Add(Me.WebBrowser1)
        Me.Controls.Add(Me.LabelLoginStatus)
        Me.Name = "Form1"
        Me.Text = "はてなブログに記事を投稿するよ"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Timer1 As Timer
    Friend WithEvents LabelLoginStatus As Label
    Friend WithEvents WebBrowser1 As WebBrowser
    Friend WithEvents ButtonStep As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents TextBoxTitle As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents TextBoxBody As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents TextBoxCategories As TextBox
    Friend WithEvents ButtonUploadArticle As Button
    Friend WithEvents Label6 As Label
    Friend WithEvents ButtonNewArticle As Button
    Friend WithEvents ButtonLoadArticle As Button
    Friend WithEvents Label7 As Label
    Friend WithEvents TextBoxHttpResponse As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents ComboBoxDraft As ComboBox
    Friend WithEvents TextBoxEntry As TextBox
End Class
