Imports TrackVideoExporter.TrackVideoExporter

Public Class frmVideoSettings
    Public videoSettings As VideoSettingsConfig
    Public Sub New(videoSetingst As VideoSettingsConfig)

        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()
        Me.videoSettings = videoSetingst
        ' Přidejte libovolnou inicializaci po volání InitializeComponent().

    End Sub

    Private Sub cbVideoSize_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbVideoSize.SelectedIndexChanged
        Select Case cbVideoSize.Text

            Case "3840x2160 (4K Ultra HD)"
                numWidth.Value = 3840
                numHeight.Value = 2160
            Case "1280x720 (HD)"
                numWidth.Value = 1280
                numHeight.Value = 720
            Case "1080x1080 (Instagram Square)"
                numWidth.Value = 1080
                numHeight.Value = 1080
            Case "Vlastní...", ""

            Case "1920x1080 (Full HD)"
                numWidth.Value = 1920
                numHeight.Value = 1080
            Case "1920x1440"
                numWidth.Value = 1920
                numHeight.Value = 1440
            Case "1024x768 (XGA)"
                numWidth.Value = 1024
                numHeight.Value = 768
            Case "1080x1920 (Vertical - TikTok/Reels)"
                numWidth.Value = 1080
                numWidth.Value = 1920
            Case Else


        End Select

    End Sub

    Private Sub frmVideoSettings_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.numWidth.Value = Me.videoSettings.VideoWidth
        Me.numHeight.Value = Me.videoSettings.VideoHeight
        Me.numTrailWidth.Value = Me.videoSettings.TrailWidth

        If Me.videoSettings.VideoWidth = 3840 AndAlso Me.videoSettings.VideoHeight = 2160 Then
            cbVideoSize.Text = "3840x2160 (4K Ultra HD)"
        End If
        If Me.videoSettings.VideoWidth = 1280 AndAlso Me.videoSettings.VideoHeight = 720 Then
            cbVideoSize.Text = "1280x720 (HD)"
        End If
        If Me.videoSettings.VideoWidth = 1080 AndAlso Me.videoSettings.VideoHeight = 1080 Then
            cbVideoSize.Text = "1080x1080 (Instagram Square)"
        End If
        If Me.videoSettings.VideoWidth = 1920 AndAlso Me.videoSettings.VideoHeight = 1080 Then
            cbVideoSize.Text = "1920x1080 (Full HD)"
        End If
        If Me.videoSettings.VideoWidth = 1920 AndAlso Me.videoSettings.VideoHeight = 1440 Then
            cbVideoSize.Text = "1920x1440"
        End If
        If Me.videoSettings.VideoWidth = 1024 AndAlso Me.videoSettings.VideoHeight = 768 Then
            cbVideoSize.Text = "1024x768 (XGA)"
        End If
        If Me.videoSettings.VideoWidth = 1080 AndAlso Me.videoSettings.VideoHeight = 1920 Then
            cbVideoSize.Text = "1080x1920 (Vertical - TikTok/Reels)"
        End If



    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        ' Tady proběhne uložení do vlastnosti videoSettings
        Me.videoSettings.VideoWidth = CInt(numWidth.Value)
        Me.videoSettings.VideoHeight = CInt(numHeight.Value)
        Me.videoSettings.TrailWidth = CInt(numTrailWidth.Value)
        Me.DialogResult = DialogResult.OK ' Tímto se okno samo zavře a vrátí OK
    End Sub

End Class