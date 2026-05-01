<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmVideoSettings
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
        cbVideoSize = New ComboBox()
        numWidth = New NumericUpDown()
        numHeight = New NumericUpDown()
        btnOK = New Button()
        lblResolution = New Label()
        lblWidth = New Label()
        lblHeight = New Label()
        Label1 = New Label()
        numTrailWidth = New NumericUpDown()
        lblTrailSpeedColor = New Label()
        cbTrailSpeedColor = New ComboBox()
        CType(numWidth, ComponentModel.ISupportInitialize).BeginInit()
        CType(numHeight, ComponentModel.ISupportInitialize).BeginInit()
        CType(numTrailWidth, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' cbVideoSize
        ' 
        cbVideoSize.BackColor = Color.Goldenrod
        cbVideoSize.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        cbVideoSize.FormattingEnabled = True
        cbVideoSize.Items.AddRange(New Object() {"3840x2160 (4K Ultra HD)", "2560x1440 (QHD - 2K)", "1920x1080 (Full HD)", "1280x720 (HD)", "1920x1440", "1024x768 (XGA)", "1080x1920 (Vertical - TikTok/Reels)", "1080x1080 (Instagram Square)", "Vlastní..."})
        cbVideoSize.Location = New Point(196, 39)
        cbVideoSize.Name = "cbVideoSize"
        cbVideoSize.Size = New Size(282, 29)
        cbVideoSize.TabIndex = 0
        ' 
        ' numWidth
        ' 
        numWidth.BackColor = Color.LightYellow
        numWidth.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        numWidth.Location = New Point(196, 90)
        numWidth.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        numWidth.Minimum = New Decimal(New Integer() {360, 0, 0, 0})
        numWidth.Name = "numWidth"
        numWidth.Size = New Size(120, 29)
        numWidth.TabIndex = 1
        numWidth.Value = New Decimal(New Integer() {360, 0, 0, 0})
        ' 
        ' numHeight
        ' 
        numHeight.BackColor = Color.LightYellow
        numHeight.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        numHeight.Location = New Point(196, 133)
        numHeight.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        numHeight.Minimum = New Decimal(New Integer() {360, 0, 0, 0})
        numHeight.Name = "numHeight"
        numHeight.Size = New Size(120, 29)
        numHeight.TabIndex = 2
        numHeight.Value = New Decimal(New Integer() {360, 0, 0, 0})
        ' 
        ' btnOK
        ' 
        btnOK.BackColor = Color.Salmon
        btnOK.FlatStyle = FlatStyle.Flat
        btnOK.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        btnOK.Location = New Point(455, 283)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(75, 29)
        btnOK.TabIndex = 3
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' lblResolution
        ' 
        lblResolution.AutoSize = True
        lblResolution.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        lblResolution.ForeColor = Color.Maroon
        lblResolution.Location = New Point(33, 42)
        lblResolution.Name = "lblResolution"
        lblResolution.Size = New Size(145, 21)
        lblResolution.TabIndex = 4
        lblResolution.Text = "Video Resolution:"
        ' 
        ' lblWidth
        ' 
        lblWidth.AutoSize = True
        lblWidth.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        lblWidth.ForeColor = Color.Maroon
        lblWidth.Location = New Point(33, 92)
        lblWidth.Name = "lblWidth"
        lblWidth.Size = New Size(61, 21)
        lblWidth.TabIndex = 5
        lblWidth.Text = "Width:"
        ' 
        ' lblHeight
        ' 
        lblHeight.AutoSize = True
        lblHeight.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        lblHeight.ForeColor = Color.Maroon
        lblHeight.Location = New Point(33, 135)
        lblHeight.Name = "lblHeight"
        lblHeight.Size = New Size(66, 21)
        lblHeight.TabIndex = 6
        lblHeight.Text = "Height:"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        Label1.ForeColor = Color.Maroon
        Label1.Location = New Point(33, 181)
        Label1.Name = "Label1"
        Label1.Size = New Size(220, 21)
        Label1.TabIndex = 8
        Label1.Text = "Width of the trails (metres):"
        ' 
        ' numTrailWidth
        ' 
        numTrailWidth.BackColor = Color.LightYellow
        numTrailWidth.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        numTrailWidth.Location = New Point(259, 179)
        numTrailWidth.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        numTrailWidth.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numTrailWidth.Name = "numTrailWidth"
        numTrailWidth.Size = New Size(120, 29)
        numTrailWidth.TabIndex = 7
        numTrailWidth.Value = New Decimal(New Integer() {10, 0, 0, 0})
        ' 
        ' lblTrailSpeedColor
        ' 
        lblTrailSpeedColor.AutoSize = True
        lblTrailSpeedColor.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        lblTrailSpeedColor.ForeColor = Color.Maroon
        lblTrailSpeedColor.Location = New Point(33, 233)
        lblTrailSpeedColor.Name = "lblTrailSpeedColor"
        lblTrailSpeedColor.Size = New Size(342, 21)
        lblTrailSpeedColor.TabIndex = 9
        lblTrailSpeedColor.Text = "Render the dog's trail color based on speed:"
        ' 
        ' cbTrailSpeedColor
        ' 
        cbTrailSpeedColor.BackColor = Color.Goldenrod
        cbTrailSpeedColor.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        cbTrailSpeedColor.FormattingEnabled = True
        cbTrailSpeedColor.Items.AddRange(New Object() {"Yes", "No"})
        cbTrailSpeedColor.Location = New Point(381, 230)
        cbTrailSpeedColor.Name = "cbTrailSpeedColor"
        cbTrailSpeedColor.Size = New Size(63, 29)
        cbTrailSpeedColor.TabIndex = 10
        ' 
        ' frmVideoSettings
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.DarkSeaGreen
        ClientSize = New Size(542, 324)
        Controls.Add(cbTrailSpeedColor)
        Controls.Add(lblTrailSpeedColor)
        Controls.Add(Label1)
        Controls.Add(numTrailWidth)
        Controls.Add(lblHeight)
        Controls.Add(lblWidth)
        Controls.Add(lblResolution)
        Controls.Add(btnOK)
        Controls.Add(numHeight)
        Controls.Add(numWidth)
        Controls.Add(cbVideoSize)
        Name = "frmVideoSettings"
        Text = "Video Settings"
        CType(numWidth, ComponentModel.ISupportInitialize).EndInit()
        CType(numHeight, ComponentModel.ISupportInitialize).EndInit()
        CType(numTrailWidth, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents cbVideoSize As ComboBox
    Friend WithEvents numWidth As NumericUpDown
    Friend WithEvents numHeight As NumericUpDown
    Friend WithEvents btnOK As Button
    Friend WithEvents lblResolution As Label
    Friend WithEvents lblWidth As Label
    Friend WithEvents lblHeight As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents numTrailWidth As NumericUpDown
    Friend WithEvents lblTrailSpeedColor As Label
    Friend WithEvents cbTrailSpeedColor As ComboBox
End Class
