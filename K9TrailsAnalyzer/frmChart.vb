
Imports System.Globalization
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting
Imports TrackVideoExporter
Imports Chart = System.Windows.Forms.DataVisualization.Charting.Chart
Imports Legend = System.Windows.Forms.DataVisualization.Charting.Legend
Imports GPXTrailAnalyzer.My.Resources

Partial Class frmChart
    Inherits System.Windows.Forms.Form

    ' Vlastnosti pro data
    Private X_Data As DateTime()
    Private X_DataString As String()
    Private Y_Data As Double()
    Private yAxisLabel As String
    Private startDate As Date
    Private endDate As Date
    Private isIntercept As Boolean 'typ proložené přímky
    Private chartType As SeriesChartType 'Typ grafu
    Private dogName As String 'Text pro název grafu
    Private Text As String 'Text pro název grafu
    Private scatterMode As ScatterModeEnum ' nový enum pro scatter plot
    Private trailData As List(Of (Time As DateTime, Age As Double, Length As Double, TotalScore As Integer, Deviation As Double, Speed As Double, Distancekm As Double, Blinding As LevelOfBlindingType))
    Private isScatterMode As Boolean ' nový příznak
    Dim blindingColors As New Dictionary(Of LevelOfBlindingType, Color) From {
        {LevelOfBlindingType.Unknown, Color.LightGray},
        {LevelOfBlindingType.Open, Color.Green},
        {LevelOfBlindingType.KnownTrack, Color.Blue},
        {LevelOfBlindingType.SingleBlind, Color.Orange},
        {LevelOfBlindingType.DoubleBlindAssisted, Color.OrangeRed},
        {LevelOfBlindingType.DoubleBlindSolo, Color.Red}
    }

    Dim blindingLabels As New Dictionary(Of LevelOfBlindingType, String) From {
        {LevelOfBlindingType.Unknown, "Uknowv"},
        {LevelOfBlindingType.Open, "Open"},
        {LevelOfBlindingType.KnownTrack, "Known Track"},
        {LevelOfBlindingType.SingleBlind, "Single Blind"},
        {LevelOfBlindingType.DoubleBlindAssisted, "Double Blind (assisted)"},
        {LevelOfBlindingType.DoubleBlindSolo, "Double Blind (solo)"}
    }
    Public Enum ScatterModeEnum
        ScoreVsAge
        ScoreVsTime
    End Enum
    ' Konstruktor, který přijme data
    Public Sub New(dogName As String, _X_data As DateTime(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _meText As String, _isIntercept As Boolean, _chartType As SeriesChartType, _CultureInfo As CultureInfo)
        Me.X_Data = _X_data
        Me.Y_Data = _Y_data
        Me.yAxisLabel = yAxisLabel
        Me.startDate = _startDate
        Me.endDate = _endDate
        Me.Text = _meText
        Me.isIntercept = _isIntercept
        Me.chartType = _chartType
        Me.dogName = dogName
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()
    End Sub

    ' Konstruktor, který přijme data
    Public Sub New(dogname As String, data As List(Of (X As Date, Y As Double)), yAxisLabel As String, _startDate As Date, _endDate As Date, _meText As String, _isIntercept As Boolean, _chartType As SeriesChartType, _CultureInfo As CultureInfo)
        ' Rozdělení na X a Y osy
        Me.X_Data = data.Select(Function(p) p.X).ToArray()
        Me.Y_Data = data.Select(Function(p) p.Y).ToArray()
        Me.yAxisLabel = yAxisLabel
        Me.startDate = _startDate
        Me.endDate = _endDate
        Me.Text = _meText
        Me.isIntercept = _isIntercept
        Me.chartType = _chartType
        Me.dogName = dogname
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()

    End Sub

    ' Konstruktor, který přijme data
    Public Sub New(dogName As String, _X_data As String(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _meText As String, _isIntercept As Boolean, _chartType As SeriesChartType, _CultureInfo As CultureInfo)
        Me.X_DataString = _X_data
        Me.Y_Data = _Y_data
        Me.yAxisLabel = yAxisLabel
        Me.startDate = _startDate
        Me.endDate = _endDate
        Me.Text = _meText
        Me.isIntercept = _isIntercept
        Me.chartType = _chartType
        Me.dogName = dogName
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()

    End Sub

    ' Nový konstruktor pro scatter plot
    Public Sub New(dogname As String,
               _trailData As List(Of (Time As DateTime, Age As Double, Length As Double, TotalScore As Integer, Deviation As Double, Speed As Double, Distancekm As Double, Blinding As LevelOfBlindingType)),
               YAxisLabel As String, _meText As String,
               _CultureInfo As CultureInfo, _mode As ScatterModeEnum)
        Me.dogName = dogname
        Me.yAxisLabel = YAxisLabel
        Me.Text = _meText
        Me.trailData = _trailData  ' nová private proměnná
        Me.X_Data = _trailData.Select(Function(t) t.Time).ToArray() ' pro regresi
        Me.Y_Data = _trailData.Select(Function(t) CDbl(t.TotalScore)).ToArray() ' pro regresi
        Me.scatterMode = _mode ' nový enum pro scatter plot
        Me.isScatterMode = True    ' nový příznak
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()
    End Sub

    ' Metoda pro výpočet směrnice přímky procházející bodem [X_Data.First().ToOADate(), 0]
    Private Function CalculateLinearRegression(_X_Data() As Date, _Y_data() As Double, _IsIntercept As Boolean) As (slope As Double, intercept As Double)
        Dim n As Integer = _X_Data.Length
        If n < 2 Then Return (0.0, 0.0)

        ' STABILIZACE: Použijeme první bod jako "časovou nulu"
        Dim firstX As Double = _X_Data(0).ToOADate()

        Dim sumX As Double = 0, sumY As Double = 0
        Dim sumXY As Double = 0, sumX2 As Double = 0

        For i As Integer = 0 To n - 1
            ' x je nyní relativní čas (např. 0, 1.5, 2.0 dní od začátku)
            Dim x As Double = _X_Data(i).ToOADate() - firstX
            Dim y As Double = _Y_data(i)

            sumX += x
            sumY += y
            sumXY += x * y
            sumX2 += x * x
        Next

        Dim slope As Double
        Dim localIntercept As Double

        If _IsIntercept Then
            ' Klasická regrese y = ax + b
            Dim denominator As Double = (n * sumX2 - sumX * sumX)
            If Math.Abs(denominator) < 0.0000000001 Then Return (0.0, 0.0) ' Ochrana před svislicí

            slope = (n * sumXY - sumX * sumY) / denominator
            localIntercept = (sumY - slope * sumX) / n
        Else
            ' Regrese procházející počátkem [0,0] v relativním čase
            If sumX2 = 0 Then Return (0.0, 0.0)
            slope = sumXY / sumX2
            localIntercept = 0
        End If

        ' PŘEVOD ZPĚT: Musíme upravit intercept, aby odpovídal původním OADate
        ' Původní rovnice: y = slope * (x - firstX) + localIntercept
        ' Roznásobeno: y = slope * x - (slope * firstX) + localIntercept
        Dim finalIntercept As Double = localIntercept - (slope * firstX)

        Return (slope, finalIntercept)
    End Function


    ' Polynomická regrese 2. stupně: y = a*x² + b*x + c
    Private Function CalculatePolynomialRegression(data As List(Of (Age As Double, Score As Integer))) As (c As Double, b As Double, a As Double)
        Dim n As Integer = data.Count
        If n < 3 Then Return (0, 0, 0) ' Nebo vyhodit výjimku

        Dim sumX As Double = 0, sumX2 As Double = 0, sumX3 As Double = 0, sumX4 As Double = 0
        Dim sumY As Double = 0, sumXY As Double = 0, sumX2Y As Double = 0

        For Each t In data
            Dim x As Double = t.Age
            Dim y As Double = t.Score
            Dim x2 As Double = x * x
            sumX += x
            sumX2 += x2
            sumX3 += x2 * x
            sumX4 += x2 * x2
            sumY += y
            sumXY += x * y
            sumX2Y += x2 * y
        Next

        ' Matice soustavy (Augmented Matrix)
        Dim G(2, 3) As Double
        G(0, 0) = n : G(0, 1) = sumX : G(0, 2) = sumX2 : G(0, 3) = sumY
        G(1, 0) = sumX : G(1, 1) = sumX2 : G(1, 2) = sumX3 : G(1, 3) = sumXY
        G(2, 0) = sumX2 : G(2, 1) = sumX3 : G(2, 2) = sumX4 : G(2, 3) = sumX2Y

        ' Gaussova eliminace s velmi jednoduchou kontrolou stability
        For col = 0 To 1
            ' Pokud je diagonální prvek příliš malý, soustava je špatně podmíněná
            If Math.Abs(G(col, col)) < 0.0000000001 Then Return (0, 0, 0)

            For row = col + 1 To 2
                Dim factor As Double = G(row, col) / G(col, col)
                For j = col To 3
                    G(row, j) -= factor * G(col, j)
                Next
            Next
        Next

        ' Zpětná substituce s kontrolou posledního prvku
        If Math.Abs(G(2, 2)) < 0.0000000001 Then Return (0, 0, 0)

        Dim resC As Double = G(2, 3) / G(2, 2)
        Dim resB As Double = (G(1, 3) - G(1, 2) * resC) / G(1, 1)
        Dim resA As Double = (G(0, 3) - G(0, 2) * resC - G(0, 1) * resB) / G(0, 0)

        Return (resA, resB, resC)
    End Function

    Private Sub AddTrendLine(data As List(Of (Age As Double, Score As Integer)), color As Color, label As String)
        If data.Count < 3 Then Return ' polynomická regrese potřebuje aspoň 3 body
        Try
            Dim coef = CalculatePolynomialRegression(data)

            Dim trendSeries As New Series(label & " trend") With {
            .ChartType = SeriesChartType.Line,
            .Color = color,
            .BorderWidth = 2,
            .BorderDashStyle = ChartDashStyle.Dash
        }

            Dim xMin As Double = data.Min(Function(t) t.Age)
            Dim xMax As Double = data.Max(Function(t) t.Age)

            For i As Integer = 0 To 100
                Dim x As Double = xMin + (xMax - xMin) * i / 100
                Dim y As Double = coef.a * x * x + coef.b * x + coef.c
                trendSeries.Points.AddXY(x, y)
            Next

            chart1.Series.Add(trendSeries)
        Catch ex As Exception
            Debug.WriteLine($"Trend line failed for {label}: " & ex.Message)
        End Try
    End Sub
    Private Sub Chart_Load(sender As Object, e As EventArgs) Handles Me.Load
        If isScatterMode Then
            If Me.scatterMode = ScatterModeEnum.ScoreVsAge Then
                PlotScoreVsAge(chart1)
            ElseIf Me.scatterMode = ScatterModeEnum.ScoreVsTime Then
                PlotScoreVsTime(chart1)
            End If
            Return
        End If
        ' ... stávající kód zůstává beze změny
        ' Nastavení rozsahu osy X na základě data
        ' Získání rozměrů obrazovky
        Dim screenBounds As Rectangle = Screen.PrimaryScreen.Bounds
        Me.Size = New Drawing.Size(screenBounds.Height * 0.8 / 3 * 4, screenBounds.Height * 0.8)
        Me.chart1.ChartAreas(0).AxisX.IsStartedFromZero = False
        ' Formátování popisků osy X (ŠIKMÉ POPISKY)
        chart1.ChartAreas(0).AxisX.LabelStyle.IsStaggered = True
        chart1.ChartAreas(0).AxisX.LabelStyle.Angle = -45 ' Nastavení úhlu



        ' Nastavení vlastností pro osu Y
        Me.chart1.ChartAreas(0).AxisY.Title = yAxisLabel
        ' Pokud chceme zobrazit mřížku
        chart1.ChartAreas(0).AxisX.MajorGrid.Enabled = True
        chart1.ChartAreas(0).AxisY.MajorGrid.Enabled = True

        'Styl mřížky
        chart1.ChartAreas(0).AxisX.MajorGrid.LineColor = Color.LightGray
        chart1.ChartAreas(0).AxisY.MajorGrid.LineColor = Color.LightGray
        chart1.ChartAreas(0).AxisX.MajorGrid.LineWidth = 1
        chart1.ChartAreas(0).AxisY.MajorGrid.LineWidth = 1
        chart1.ChartAreas(0).AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash 'Tečkovaná čára
        chart1.ChartAreas(0).AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash
        chart1.Titles(0).Text = Me.dogName & " - " & Me.Text

        'chart1.Titles(0).Alignment = ContentAlignment.TopCenter

        Dim series1 As New Series() With {
            .Name = "Series1",
            .ChartType = Me.chartType}

        ' Přidání dat do série
        If Me.chartType = SeriesChartType.Point Then
            chart1.ChartAreas(0).AxisX.LabelStyle.Format = "MMMM yy"
            With series1
                .MarkerSize = 10 ' Nastaví velikost bodů na 10 pixelů
                .MarkerStyle = MarkerStyle.Circle
                .MarkerColor = Color.Chocolate
                .XValueType = ChartValueType.DateTime
            End With
            Me.chart1.ChartAreas(0).AxisX.Minimum = startDate.ToOADate()
            Me.chart1.ChartAreas(0).AxisX.Maximum = endDate.ToOADate()
            For i As Integer = 0 To Y_Data.Length - 1
                series1.Points.AddXY(X_Data(i), Y_Data(i))
            Next



            ' Vytvoření nové série pro proloženou přímku
            Dim regressionSeries As New Series() With {
                .Name = "Trend Line",
                .ChartType = SeriesChartType.Line,
                .XValueType = ChartValueType.DateTime,
                .Color = System.Drawing.Color.Red,
                .BorderWidth = 2
            }
            Try
                ' Výpočet lineární regrese
                Dim regression = CalculateLinearRegression(X_Data, Y_Data, isIntercept)
                Dim slope = regression.slope
                Dim intercept = regression.intercept
                ' Přidání dvou bodů do série, které reprezentují přímku
                Dim xStart As Double = X_Data.First().ToOADate()
                Dim xEnd As Double = X_Data.Last().ToOADate()
                Dim yStart As Double = slope * xStart + Intercept
                Dim yEnd As Double = slope * xEnd + Intercept

                regressionSeries.Points.AddXY(DateTime.FromOADate(xStart), yStart)
                regressionSeries.Points.AddXY(DateTime.FromOADate(xEnd), yEnd)

                ' Přidání regresní série do grafu
                chart1.Series.Add(regressionSeries)
            Catch ex As Exception
                Debug.WriteLine("Failed to interlace a straight line")
            End Try


        ElseIf Me.chartType = SeriesChartType.Column Then


            series1.Color = Color.Chocolate
            series1.IsValueShownAsLabel = True
            series1.LabelFormat = "N2"
            series1.XValueType = ChartValueType.String
            series1.IsXValueIndexed = True
            series1.XValueType = ChartValueType.String



            For i As Integer = 0 To Y_Data.Length - 1
                series1.Points.AddXY(X_DataString(i), Y_Data(i))
            Next

        End If



        ' Přidání série do grafu
        chart1.Series.Add(series1)
        Debug.WriteLine($"Počet bodů: {series1.Points.Count}")
        Debug.WriteLine($"ChartAreas: {chart1.ChartAreas.Count}, Series: {chart1.Series.Count}")
        Debug.WriteLine($"Nakonec: chart.Series.Count={chart1.Series.Count}, Body={series1.Points.Count}")

    End Sub

    Public Sub PlotScoreVsAge(chart As Chart)

        Debug.WriteLine($"TrailData count: {trailData?.Count}")
        Debug.WriteLine($"Series count před vykreslením: {chart1.Series.Count}")

        If trailData Is Nothing OrElse trailData.Count = 0 Then
            Debug.WriteLine("TrailData je prázdný - končím")
            Return
        End If
        chart.Series.Clear()
        'Me.chart1.ChartAreas(0).AxisY.Maximum = 500.0
        chart.ChartAreas(0).AxisX.Maximum = 4.0
        chart.Titles(0).Text = Me.dogName & " - " & Me.Text
        ' Vytvoř samostatnou sérii pro každou kategorii
        Dim seriesDict As New Dictionary(Of LevelOfBlindingType, Series)
        For Each kvp In blindingColors
            Dim s As New Series(blindingLabels(kvp.Key))
            s.ChartType = SeriesChartType.Point
            s.Color = kvp.Value
            s.MarkerStyle = MarkerStyle.Circle
            s.MarkerSize = 10
            chart.Series.Add(s)
            seriesDict(kvp.Key) = s
        Next

        ' Naplň daty
        For Each t In trailData
            Dim s As Series = seriesDict(t.Blinding)
            Dim idx As Integer = s.Points.AddXY(t.Age, t.TotalScore)
            s.Points(idx).ToolTip = $"{t.Time:dd.MM.yyyy} | {t.Age:F1}h | {t.Length:F1}km | {t.TotalScore}b | {blindingLabels(t.Blinding)}"
        Next

        ' Popisky os
        With chart.ChartAreas(0)
            .AxisX.Title = $"{Resource1.outAge}"
            .AxisX.Minimum = 0
            .AxisY.Title = Me.yAxisLabel
            .AxisY.Minimum = 0
        End With

        For Each kvp In blindingColors
            Dim blinding = kvp.Key
            Dim filtered = trailData.
        Where(Function(t) t.Blinding = blinding).
        Select(Function(t) (t.Age, t.TotalScore)).
        ToList()
            If filtered.Count >= 3 Then
                AddTrendLine(filtered, kvp.Value, blindingLabels(blinding))
            End If
        Next

        ' Legenda
        chart.Legends.Clear()
        chart.Legends.Add(New Legend(Resource1.outBlinding) With {
    .BackColor = Color.Transparent
})

    End Sub

    Private Sub PlotScoreVsTime(chart As Chart)
        chart.Titles(0).Text = Me.dogName & " - " & Me.Text
        chart.ChartAreas(0).AxisX.LabelStyle.Format = "MMMM yy"
        chart.ChartAreas(0).AxisX.LabelStyle.Angle = -45
        chart.ChartAreas(0).AxisX.Minimum = trailData.Min(Function(t) t.Time).ToOADate()
        chart.ChartAreas(0).AxisX.Maximum = trailData.Max(Function(t) t.Time).ToOADate()
        chart.ChartAreas(0).AxisY.Title = Me.yAxisLabel
        chart.ChartAreas(0).AxisX.MajorGrid.LineColor = Color.LightGray
        chart.ChartAreas(0).AxisY.MajorGrid.LineColor = Color.LightGray
        chart.ChartAreas(0).AxisX.MajorGrid.LineWidth = 1
        chart.ChartAreas(0).AxisY.MajorGrid.LineWidth = 1
        chart.ChartAreas(0).AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash
        chart.ChartAreas(0).AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash

        ' Série bodů podle zaslepení
        For Each kvp In blindingColors
            Dim blinding = kvp.Key
            Dim filtered = trailData.Where(Function(t) t.Blinding = blinding).ToList()
            If filtered.Count = 0 Then Continue For

            Dim s As New Series(blindingLabels(blinding)) With {
            .ChartType = SeriesChartType.Point,
            .Color = kvp.Value,
            .MarkerStyle = MarkerStyle.Circle,
            .MarkerSize = 8,
            .XValueType = ChartValueType.DateTime
        }

            For Each t In filtered
                Dim idx As Integer = s.Points.AddXY(t.Time, t.TotalScore)
                s.Points(idx).ToolTip = $"{t.Time:dd.MM.yyyy} | {t.Age:F1}h | {t.Length:F1}km | {t.TotalScore}b | {blindingLabels(blinding)}"
            Next

            chart.Series.Add(s)

            ' Lineární trend
            If filtered.Count >= 2 Then
                Dim xVals = filtered.Select(Function(t) t.Time).ToArray()
                Dim yVals = filtered.Select(Function(t) CDbl(t.TotalScore)).ToArray()

                Dim regressionSeries As New Series(blindingLabels(blinding) & " trend") With {
                .ChartType = SeriesChartType.Line,
                .Color = kvp.Value,
                .BorderWidth = 2,
                .BorderDashStyle = ChartDashStyle.Dash,
                .XValueType = ChartValueType.DateTime
            }
                ' Výpočet lineární regrese
                Dim regression = CalculateLinearRegression(xVals, yVals, True)
                Dim slope = regression.slope
                Dim intercept = regression.intercept
                ' Přidání dvou bodů do série, které reprezentují přímku
                Dim xStart As Double = xVals.First().ToOADate()
                Dim xEnd As Double = xVals.Last().ToOADate()
                Dim yStart As Double = slope * xStart + intercept
                Dim yEnd As Double = slope * xEnd + intercept

                regressionSeries.Points.AddXY(DateTime.FromOADate(xStart), yStart)
                regressionSeries.Points.AddXY(DateTime.FromOADate(xEnd), yEnd)



                chart1.Series.Add(regressionSeries)
            End If
        Next

        ' Legenda
        chart.Legends.Clear()
        chart.Legends.Add(New Legend(Resource1.outBlinding) With {
    .BackColor = Color.Transparent
})
    End Sub
    Private Sub SaveAs(sender As Object, e As EventArgs) Handles SaveAsToolStripMenuItem.Click
        Using dialog As New SaveFileDialog()
            dialog.Filter = "PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg"
            'dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = IO.Directory.GetParent(Application.StartupPath).ToString
            dialog.Title = "Save as"
            dialog.FileName = Me.Text.Replace("/", " per ")

            If dialog.ShowDialog() = DialogResult.OK Then

                Debug.WriteLine($"Selected file: {dialog.FileName}")
                'Ulož upravený RTF text zpět do souboru

                Dim format As ChartImageFormat
                Try
                    Select Case dialog.FilterIndex
                        Case 1
                            format = ChartImageFormat.Png
                        Case 2
                            format = ChartImageFormat.Jpeg
                    End Select
                    Me.chart1.SaveImage(dialog.FileName, format)

                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {dialog.FileName} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub
End Class

