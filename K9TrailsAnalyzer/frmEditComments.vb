Imports System.ComponentModel
Imports System.Globalization
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports TrackVideoExporter

Public Class frmEditComments
    'Public Property Category As String
    Public Property TrailDescription As TrailReport
    Public Property GoalPart As String
    Public Property TrailPart As String
    Public Property DogPart As String
    Public Property Language As String = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant()
    Public Property GpxFileName As String
    Public Property HandlerName As String



    Dim LanguageAbbreviations As New List(Of String) From {
    "cs", ' čeština
    "en", ' angličtina
    "de", ' němčina
    "fr", ' francouzština
    "es", ' španělština
    "it", ' italština
    "pl", ' polština
    "ru", ' ruština
    "uk", ' ukrajinština
    "sk", ' slovenština
    "hu", ' maďarština
    "pt", ' portugalština
    "nl", ' nizozemština
    "sv", ' švédština
    "da", ' dánština
    "fi", ' finština
    "no", ' norština
    "tr", ' turečtina
    "zh", ' čínština
    "ja", ' japonština
    "ko", ' korejština
    "ar", ' arabština
    "he", ' hebrejština
    "el"  ' řečtina
}




    Public Sub New()

        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()

        ' Přidejte libovolnou inicializaci po volání InitializeComponent().
        LanguageAbbreviations.Sort()
        cbLanguage.Items.AddRange(LanguageAbbreviations.ToArray())
        'cbTrackType.Items.AddRange([Enum].GetNames(GetType(TrailReport.TrackTypeEnum)))

    End Sub

    Private Sub Me_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' 1. Naplnění textových polí
        txtGoal.Text = Me.TrailDescription.Goal.Text
        txtTrail.Text = Me.TrailDescription.Trail.Text
        txtPerformance.Text = Me.TrailDescription.Performance.Text
        txtDogName.Text = Me.TrailDescription.DogName.Text
        txtHandlerName.Text = Me.HandlerName

        ' 2. Nastavení výběru jazyka (cbLanguage)
        ' Nejdříve naplníme seznam zkratek (pokud už není naplněn v konstruktoru)
        If cbLanguage.Items.Count = 0 Then
            cbLanguage.Items.AddRange(LanguageAbbreviations.ToArray())
        End If

        If Language IsNot Nothing Then
            ' Nastavíme vybraný jazyk a zamkneme combo, pokud je jazyk pevně dán
            Me.cbLanguage.SelectedItem = Language
            Me.cbLanguage.Enabled = False
        End If

        ' 3. Naplnění cbLevelOfBlinding (využijeme pomocnou metodu)
        ' Určíme, jaký kód jazyka použít (buď ten z proměnné Language, nebo aktuální v systému)
        Dim langToUse As String = If(Language IsNot Nothing, Language.ToString(),
                                System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)

        UpdateBlindingComboBox(langToUse)

        ' Nastavení vybrané hodnoty z popisu trailu (musí být až po UpdateBlindingComboBox!)
        cbLevelOfBlinding.SelectedValue = Me.TrailDescription.LevelOfBlinding

        cbNumberOfArticlesFound.SelectedItem = Me.TrailDescription.NumberOfArticlesFound
        ' 4. Vizuální úpravy a popisky
        lblInfo.Text = $"{Form1.mnuFile.Text} {GpxFileName}" & vbCrLf & lblInfo.Text

        txtPerformance.AllowDrop = True
        txtGoal.AllowDrop = True
        txtTrail.AllowDrop = True

        Me.lblGoal.Text = TrailReport.goalLabel & Me.lblGoal.Text
        Me.lblTrail.Text = TrailReport.trailLabel & Me.lblTrail.Text
        Me.lblPerformance.Text = TrailReport.performanceLabel & Me.lblPerformance.Text

        Me.btnOK.Focus()
    End Sub

    Private Sub UpdateBlindingComboBox(languageCode As String)
        ' 1. Vytvoření CultureInfo na základě vybraného jazyka
        Dim culture As New System.Globalization.CultureInfo(languageCode)
        Dim resources As New System.ComponentModel.ComponentResourceManager(GetType(frmEditComments))

        ' Uložení aktuální vybrané hodnoty, abychom ji po změně jazyka mohli vrátit zpět
        Dim selectedVal As LevelOfBlindingType = LevelOfBlindingType.Unknown
        If cbLevelOfBlinding.SelectedValue IsNot Nothing Then
            selectedVal = DirectCast(cbLevelOfBlinding.SelectedValue, LevelOfBlindingType)
        End If

        ' 2. Vytvoření nového seznamu položek s překlady pro danou kulturu
        ' Použijeme přetížení GetString(key, culture)
        Dim displayItems As New List(Of LevelOfBlindingDisplayItem) From {
        New LevelOfBlindingDisplayItem(LevelOfBlindingType.Unknown, resources.GetString("cbLevelOfBlinding.Items", culture)),
        New LevelOfBlindingDisplayItem(LevelOfBlindingType.Open, resources.GetString("cbLevelOfBlinding.Items1", culture)),
        New LevelOfBlindingDisplayItem(LevelOfBlindingType.KnownTrack, resources.GetString("cbLevelOfBlinding.Items2", culture)),
        New LevelOfBlindingDisplayItem(LevelOfBlindingType.SingleBlind, resources.GetString("cbLevelOfBlinding.Items3", culture)),
        New LevelOfBlindingDisplayItem(LevelOfBlindingType.DoubleBlindAssisted, resources.GetString("cbLevelOfBlinding.Items4", culture)),
                New LevelOfBlindingDisplayItem(LevelOfBlindingType.DoubleBlindSolo, resources.GetString("cbLevelOfBlinding.Items5", culture))
    }

        ' 3. Re-binding dat
        ' Tip: Aby se UI "necukalo", je dobré DataSource na chvíli vynulovat
        cbLevelOfBlinding.DataSource = Nothing
        cbLevelOfBlinding.DisplayMember = "Text"
        cbLevelOfBlinding.ValueMember = "Value"
        cbLevelOfBlinding.DataSource = displayItems

        ' 4. Vrácení původní vybrané hodnoty
        cbLevelOfBlinding.SelectedValue = selectedVal
    End Sub

    Private Sub SaveFormData()
        ' Získáte přímo Enum

        If cbLevelOfBlinding.SelectedItem IsNot Nothing Then
            'Dim selectedLevelOfBlindingType As LevelOfBlindingType = DirectCast(cbLevelOfBlinding.SelectedValue, LevelOfBlindingType)
            Me.TrailDescription.LevelOfBlinding = cbLevelOfBlinding.SelectedValue
            Me.TrailDescription.LevelOfBlindingText = cbLevelOfBlinding.SelectedItem.Text
        End If
        If cbNumberOfArticlesFound.SelectedItem IsNot Nothing Then
            Me.TrailDescription.NumberOfArticlesFound = cbNumberOfArticlesFound.SelectedItem.ToString
        End If

        Me.TrailDescription.GoalText = txtGoal.Text
        Me.TrailDescription.TrailText = txtTrail.Text
        Me.TrailDescription.PerformanceText = txtPerformance.Text
        Me.TrailDescription.DogNameText = txtDogName.Text ' 
        Me.TrailDescription.HandlerNameText = txtHandlerName.Text '
        Me.TrailDescription.DogName.Text = txtDogName.Text
        Me.HandlerName = txtHandlerName.Text
        If Me.Language Is Nothing Then
            Me.Language = cbLanguage.SelectedItem?.ToString()?.ToLowerInvariant() ' Uloží vybraný jazyk
        ElseIf Me.Language <> cbLanguage.SelectedItem?.ToString()?.ToLowerInvariant() Then
            ' Pokud se jazyk změnil, aktualizujeme ho
            mboxEx("The language of the existing description cannot be changed!")

        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        ' Po kliknutí na OK tlačítko se data uloží a formulář se zavře s výsledkem OK
        SaveFormData()
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub MyForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Tato událost se spustí, když se formulář zavírá (včetně křížku)
        ' e.CloseReason vám řekne, proč se formulář zavírá.
        ' Pokud se zavírá křížkem (UserClosing) A ještě nebylo nastaveno DialogResult na OK (např. přes tlačítko OK),
        ' pak to ošetříme jako OK.


        If Me.DialogResult = DialogResult.OK Then
            ' Pokud je DialogResult již OK (nastaveno tlačítkem OK),
            ' znamená to, že data jsou již uložena (nebo se budou ukládat jinde),
            ' a nemusíme zde nic dalšího dělat, kromě toho, že umožníme zavření.
        ElseIf Me.DialogResult = DialogResult.Retry Then
            ' Pokud byste chtěli, aby křížek fungoval jako Cancel, udělali byste:
            ' If e.CloseReason = CloseReason.UserClosing AndAlso Me.DialogResult <> DialogResult.OK Then
            '     Me.DialogResult = DialogResult.Cancel
        Else
            SaveFormData()
            Me.DialogResult = DialogResult.OK
        End If

    End Sub

    Private Sub btnAnotherLang_Click(sender As Object, e As EventArgs) Handles btnAnotherLang.Click
        ' Po kliknutí na  tlačítko se data uloží a formulář se zavře s výsledkem retry
        SaveFormData()
        DialogResult = DialogResult.Retry
        Close()
    End Sub

    Private Sub frmEditComments_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Me.cbLanguage.SelectedItem = Nothing Then
            Select Case Me.DialogResult
                Case DialogResult.OK, DialogResult.Retry
                    e.Cancel = True
                    MessageBox.Show("Please select a language for the description.", "Language not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Select
        End If
    End Sub

    Private Sub cbLanguage_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbLanguage.SelectedIndexChanged
        Dim selectedLang As String = cbLanguage.SelectedItem.ToString()

        ' Předpokládáme, že selectedLang je např. "en" nebo "cs"
        ' Pokud máte v LanguageAbbreviations celé názvy, budete potřebovat mapování
        UpdateBlindingComboBox(selectedLang)
    End Sub
End Class

