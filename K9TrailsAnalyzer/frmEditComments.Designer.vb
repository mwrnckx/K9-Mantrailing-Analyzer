<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmEditComments
    Inherits System.Windows.Forms.Form

    'Formulář přepisuje metodu Dispose, aby vyčistil seznam součástí.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Vyžadováno Návrhářem Windows Form
    Private components As System.ComponentModel.IContainer

    'POZNÁMKA: Následující procedura je vyžadována Návrhářem Windows Form
    'Může být upraveno pomocí Návrháře Windows Form.  
    'Neupravovat pomocí editoru kódu
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEditComments))
        lblInfo = New Label()
        lblPerformance = New Label()
        lblTrail = New Label()
        lblGoal = New Label()
        btnOK = New Button()
        btnAnotherLang = New Button()
        lblLanguage = New Label()
        cbLanguage = New ComboBox()
        txtGoal = New TextBox()
        txtPerformance = New TextBox()
        txtTrail = New TextBox()
        lblDogName = New Label()
        lblHandlerName = New Label()
        txtDogName = New TextBox()
        txtHandlerName = New TextBox()
        lblLevelOfBlinding = New Label()
        cbLevelOfBlinding = New ComboBox()
        lblNoOFArticles = New Label()
        cbNumberOfArticlesFound = New ComboBox()
        SuspendLayout()
        ' 
        ' lblInfo
        ' 
        lblInfo.BackColor = Color.LightYellow
        resources.ApplyResources(lblInfo, "lblInfo")
        lblInfo.Name = "lblInfo"
        ' 
        ' lblPerformance
        ' 
        resources.ApplyResources(lblPerformance, "lblPerformance")
        lblPerformance.Name = "lblPerformance"
        ' 
        ' lblTrail
        ' 
        resources.ApplyResources(lblTrail, "lblTrail")
        lblTrail.Name = "lblTrail"
        ' 
        ' lblGoal
        ' 
        resources.ApplyResources(lblGoal, "lblGoal")
        lblGoal.Name = "lblGoal"
        ' 
        ' btnOK
        ' 
        resources.ApplyResources(btnOK, "btnOK")
        btnOK.BackColor = Color.Goldenrod
        btnOK.Name = "btnOK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' btnAnotherLang
        ' 
        resources.ApplyResources(btnAnotherLang, "btnAnotherLang")
        btnAnotherLang.BackColor = Color.Salmon
        btnAnotherLang.Name = "btnAnotherLang"
        btnAnotherLang.UseVisualStyleBackColor = False
        ' 
        ' lblLanguage
        ' 
        resources.ApplyResources(lblLanguage, "lblLanguage")
        lblLanguage.Name = "lblLanguage"
        ' 
        ' cbLanguage
        ' 
        cbLanguage.FormattingEnabled = True
        resources.ApplyResources(cbLanguage, "cbLanguage")
        cbLanguage.Name = "cbLanguage"
        ' 
        ' txtGoal
        ' 
        txtGoal.AllowDrop = True
        resources.ApplyResources(txtGoal, "txtGoal")
        txtGoal.Name = "txtGoal"
        ' 
        ' txtPerformance
        ' 
        txtPerformance.AllowDrop = True
        resources.ApplyResources(txtPerformance, "txtPerformance")
        txtPerformance.Name = "txtPerformance"
        ' 
        ' txtTrail
        ' 
        txtTrail.AllowDrop = True
        resources.ApplyResources(txtTrail, "txtTrail")
        txtTrail.Name = "txtTrail"
        ' 
        ' lblDogName
        ' 
        resources.ApplyResources(lblDogName, "lblDogName")
        lblDogName.Name = "lblDogName"
        ' 
        ' lblHandlerName
        ' 
        resources.ApplyResources(lblHandlerName, "lblHandlerName")
        lblHandlerName.Name = "lblHandlerName"
        ' 
        ' txtDogName
        ' 
        resources.ApplyResources(txtDogName, "txtDogName")
        txtDogName.Name = "txtDogName"
        ' 
        ' txtHandlerName
        ' 
        resources.ApplyResources(txtHandlerName, "txtHandlerName")
        txtHandlerName.Name = "txtHandlerName"
        ' 
        ' lblLevelOfBlinding
        ' 
        resources.ApplyResources(lblLevelOfBlinding, "lblLevelOfBlinding")
        lblLevelOfBlinding.Name = "lblLevelOfBlinding"
        ' 
        ' cbLevelOfBlinding
        ' 
        cbLevelOfBlinding.DropDownStyle = ComboBoxStyle.DropDownList
        cbLevelOfBlinding.DropDownWidth = 800
        cbLevelOfBlinding.FormattingEnabled = True
        cbLevelOfBlinding.Items.AddRange(New Object() {resources.GetString("cbLevelOfBlinding.Items"), resources.GetString("cbLevelOfBlinding.Items1"), resources.GetString("cbLevelOfBlinding.Items2"), resources.GetString("cbLevelOfBlinding.Items3"), resources.GetString("cbLevelOfBlinding.Items4"), resources.GetString("cbLevelOfBlinding.Items5")})
        resources.ApplyResources(cbLevelOfBlinding, "cbLevelOfBlinding")
        cbLevelOfBlinding.Name = "cbLevelOfBlinding"
        ' 
        ' lblNoOFArticles
        ' 
        resources.ApplyResources(lblNoOFArticles, "lblNoOFArticles")
        lblNoOFArticles.Name = "lblNoOFArticles"
        ' 
        ' cbNumberOfArticlesFound
        ' 
        cbNumberOfArticlesFound.DropDownStyle = ComboBoxStyle.DropDownList
        cbNumberOfArticlesFound.FormattingEnabled = True
        cbNumberOfArticlesFound.Items.AddRange(New Object() {resources.GetString("cbNumberOfArticlesFound.Items"), resources.GetString("cbNumberOfArticlesFound.Items1"), resources.GetString("cbNumberOfArticlesFound.Items2"), resources.GetString("cbNumberOfArticlesFound.Items3"), resources.GetString("cbNumberOfArticlesFound.Items4"), resources.GetString("cbNumberOfArticlesFound.Items5"), resources.GetString("cbNumberOfArticlesFound.Items6"), resources.GetString("cbNumberOfArticlesFound.Items7"), resources.GetString("cbNumberOfArticlesFound.Items8"), resources.GetString("cbNumberOfArticlesFound.Items9"), resources.GetString("cbNumberOfArticlesFound.Items10")})
        resources.ApplyResources(cbNumberOfArticlesFound, "cbNumberOfArticlesFound")
        cbNumberOfArticlesFound.Name = "cbNumberOfArticlesFound"
        ' 
        ' frmEditComments
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.LightYellow
        Controls.Add(cbNumberOfArticlesFound)
        Controls.Add(cbLevelOfBlinding)
        Controls.Add(lblLevelOfBlinding)
        Controls.Add(txtHandlerName)
        Controls.Add(txtDogName)
        Controls.Add(lblHandlerName)
        Controls.Add(lblDogName)
        Controls.Add(txtTrail)
        Controls.Add(txtPerformance)
        Controls.Add(txtGoal)
        Controls.Add(cbLanguage)
        Controls.Add(btnAnotherLang)
        Controls.Add(btnOK)
        Controls.Add(lblNoOFArticles)
        Controls.Add(lblGoal)
        Controls.Add(lblTrail)
        Controls.Add(lblLanguage)
        Controls.Add(lblPerformance)
        Controls.Add(lblInfo)
        Name = "frmEditComments"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblInfo As Label
    Friend WithEvents lblPerformance As Label
    Friend WithEvents lblTrail As Label
    Friend WithEvents lblGoal As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnotherLang As Button
    Friend WithEvents lblLanguage As Label
    Friend WithEvents cbLanguage As ComboBox
    Friend WithEvents txtGoal As TextBox
    Friend WithEvents txtPerformance As TextBox
    Friend WithEvents txtTrail As TextBox
    Friend WithEvents lblDogName As Label
    Friend WithEvents lblHandlerName As Label
    Friend WithEvents txtDogName As TextBox
    Friend WithEvents txtHandlerName As TextBox
    Friend WithEvents lblLevelOfBlinding As Label
    Friend WithEvents cbLevelOfBlinding As ComboBox
    Friend WithEvents lblNoOFArticles As Label
    Friend WithEvents cbNumberOfArticlesFound As ComboBox
End Class
