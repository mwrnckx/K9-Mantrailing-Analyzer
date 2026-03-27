Imports System.Drawing
Imports System.Reflection
Imports System.Resources.ResXFileRef
Imports System.Runtime.CompilerServices.RuntimeHelpers
Imports System.Security.Cryptography
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
Imports System.Xml
Imports Microsoft.VisualBasic.Logging

''' <summary>
''' Provides conversion of GPX data between various internal formats used for video rendering.
''' </summary>
Public Class TrackConverter
    ''' <summary>
    ''' Raised when a non-critical warning occurs during processing.
    ''' </summary>
    Public Event WarningOccurred(message As String, _color As Color)
    ''' <summary>
    ''' Minimum latitude found in all tracks.
    ''' </summary>
    Public minLat As Double = Double.MaxValue

    ''' <summary>
    ''' Maximum latitude found in all tracks.
    ''' </summary>
    Public maxLat As Double = Double.MinValue

    ''' <summary>
    ''' Minimum longitude found in all tracks.
    ''' </summary>
    Public minLon As Double = Double.MaxValue

    ''' <summary>
    ''' Maximum longitude found in all tracks.
    ''' </summary>
    Public maxLon As Double = Double.MinValue

    ''' <summary>
    ''' Initializes a new instance of the <see cref="TrackConverter"/> class.
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Converts a list of GPX track nodes to a list of track points with XML nodes.
    ''' </summary>
    ''' <param name="_tracksAsTrkNode">List of track nodes containing raw GPX XML data.</param>
    ''' <returns>List of <see cref="TrackAsTrkPts"/> representing extracted track points.</returns>
    Public Function ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode As List(Of TrackAsTrkNode)) As List(Of TrackAsTrkPts)
        Dim tracksAsTrkPts As New List(Of TrackAsTrkPts)
        For Each track In _tracksAsTrkNode
            Dim _TrackAsTrkPts As TrackAsTrkPts = ConvertTrackAsTrkNodeToTrkPts(track)
            tracksAsTrkPts.Add(_TrackAsTrkPts)
        Next
        Return tracksAsTrkPts
    End Function

    Public Function ConvertTrackAsTrkNodeToTrkPts(track As TrackAsTrkNode) As TrackAsTrkPts
        Dim trkptNodes As XmlNodeList = SelectTrkptNodes(track.TrkNode)
        Dim _TrackAsTrkPts As New TrackAsTrkPts(track.TrackType, trkptNodes)
        Return _TrackAsTrkPts
    End Function

    ''' <summary>
    ''' Converts XML track points to geographical points with timestamps.
    ''' </summary>
    ''' <param name="_tracksAsTrkPts">List of tracks containing XML track point nodes.</param>
    ''' <returns>List of <see cref="TrackAsGeoPoints"/> with lat/lon and time.</returns>
    Public Function ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts As List(Of TrackAsTrkPts)) As List(Of TrackAsGeoPoints)
        If _tracksAsTrkPts Is Nothing Then Return Nothing
        Dim tracksAsGeoPoints As New List(Of TrackAsGeoPoints)
        For Each track In _tracksAsTrkPts

            Dim _TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(track)

            tracksAsGeoPoints.Add(_TrackAsGeoPoints)
        Next
        Return tracksAsGeoPoints
    End Function

    Public Function ConvertTrackTrkPtsToGeoPoints(track As TrackAsTrkPts) As TrackAsGeoPoints
        If track Is Nothing Then Return Nothing
        Dim trackGeoPoints As New List(Of TrackGeoPoint)
        For Each trkptnode As XmlNode In track.TrackPoints
            Dim lat = Double.Parse(trkptnode.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon = Double.Parse(trkptnode.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Dim timenode = SelectSingleChildNode("time", trkptnode)
            Dim time As DateTime
            If timenode IsNot Nothing Then
                time = DateTime.Parse(timenode.InnerText, Nothing, Globalization.DateTimeStyles.AssumeUniversal)
            Else
                Debug.WriteLine("Time node not found in trkpt.")
                time = DateTime.MinValue
            End If
            Dim nameNode As XmlNode = SelectSingleChildNode("name", trkptnode)
            Dim name
            If nameNode IsNot Nothing Then
                name = nameNode.InnerText
            Else name = ""
            End If


            Dim geopoint As New TrackGeoPoint With {
                .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                .Time = time,
                .name = name
            }
            trackGeoPoints.Add(geopoint)
        Next
        Dim _TrackAsGeoPoints As New TrackAsGeoPoints(track.TrackType, trackGeoPoints)
        Return _TrackAsGeoPoints

    End Function

    Public Shared Function PurifyTrackAsGeoPoints(rawPoints As TrackAsGeoPoints, maxSpeedKmh As Double) As TrackAsGeoPoints
        ' 0. KROK: Sanitace a základní kontrola
        If rawPoints Is Nothing OrElse rawPoints.TrackGeoPoints.Count < 2 Then
            Return rawPoints
        End If
        ' 1. KROK: Tvůj rychlostní a distanční filtr (Sanitace dat)
        Dim cleanedPoints As New List(Of TrackGeoPoint)
        If rawPoints.TrackGeoPoints.Count = 0 Then Return New TrackAsGeoPoints(rawPoints.TrackType, cleanedPoints)

        ' Přidáme první dva body
        cleanedPoints.Add(rawPoints.TrackGeoPoints(0))
        cleanedPoints.Add(rawPoints.TrackGeoPoints(1))

        Dim lastValidPoint = rawPoints.TrackGeoPoints(1)
        ' Projdeme zbytek a vyhážeme "teleportace"
        For i As Integer = 2 To rawPoints.TrackGeoPoints.Count - 1
            Dim current = rawPoints.TrackGeoPoints(i)
            Dim timeDiffHours As Double = (current.Time - lastValidPoint.Time).TotalSeconds / 3600.0
            ' Ošetření bodů ve stejném čase (ignore) nebo příliš blízko u sebe
            If timeDiffHours <= 0 Then Continue For
            Dim distKm As Double = TrackConverter.HaversineDistance(lastValidPoint.Location.Lat, lastValidPoint.Location.Lon, current.Location.Lat, current.Location.Lon, "km")
            Dim speed As Double = If(timeDiffHours > 0, distKm / timeDiffHours, 0)

            ' Filtrujeme jen nesmysly (např. víc než 40 km/h) a příliš krátké pohyby (drift)
            If distKm > 0.001 Then ' 1 metr práh (pro psy stačí méně než 2m)
                If speed <= maxSpeedKmh Then
                    cleanedPoints.Add(current)
                    lastValidPoint = current
                End If
            End If
        Next
        ' Kontrola, zda nám po čištění něco zbylo (aspoň 2 body pro trasu)
        If cleanedPoints.Count < 2 Then Return New TrackAsGeoPoints(rawPoints.TrackType, cleanedPoints)

        ' 2. KROK: Douglas-Peucker (Simplifikace pro zachování lomů a odstranění šumu)
        ' Tolerance v metrech (např. 0.002 km = 2 metry). 
        ' Čím větší číslo, tím "rovnější" čáry, ale lomy zůstanou.
        Dim simplifiedPoints = DouglasPeucker(cleanedPoints, 0.002)
        Dim finalPoints = RedensifyTrackAsGeopoints(simplifiedPoints, 2) ' Redensifikace pro vložení bodů každé 2 metry
        Return New TrackAsGeoPoints(rawPoints.TrackType, finalPoints)
    End Function

    ' Pomocná funkce pro Douglas-Peucker algoritmus
    Private Shared Function DouglasPeucker(points As List(Of TrackGeoPoint), epsilonkm As Double) As List(Of TrackGeoPoint)
        If points.Count < 3 Then Return points

        Dim maxDistkm As Double = 0
        Dim index As Integer = 0
        Dim lastIdx As Integer = points.Count - 1

        ' Najdeme bod s největší odchylkou od spojnice prvního a posledního bodu
        For i As Integer = 1 To lastIdx - 1
            Dim distkm As Double = PerpendicularDistancekm(points(i), points(0), points(lastIdx))
            If distkm > maxDistkm Then
                maxDistkm = distkm
                index = i
            End If
        Next

        Dim result As New List(Of TrackGeoPoint)

        ' Pokud je odchylka větší než epsilon, bod si zaslouží zůstat a dělíme trasu na dvě části
        If maxDistkm > epsilonkm Then
            Dim leftRecursive = DouglasPeucker(points.GetRange(0, index + 1), epsilonkm)
            Dim rightRecursive = DouglasPeucker(points.GetRange(index, points.Count - index), epsilonkm)

            result.AddRange(leftRecursive)
            result.RemoveAt(result.Count - 1) ' Odstraníme duplicitu na spoji
            result.AddRange(rightRecursive)
        Else
            ' Žádný bod nevybočuje dostatečně, stačí nám jen začátek a konec úseku
            result.Add(points(0))
            result.Add(points(lastIdx))
        End If

        Return result
    End Function

    Private Shared Function RedensifyTrackAsGeopoints(points As List(Of TrackGeoPoint), maxDistMeters As Double) As List(Of TrackGeoPoint)
        If points.Count < 2 Then Return points

        Dim result As New List(Of TrackGeoPoint)
        Dim maxDistKm As Double = maxDistMeters / 1000.0 ' Převod na km pro Haversine

        For i As Integer = 0 To points.Count - 2
            Dim p1 = points(i)
            Dim p2 = points(i + 1)

            ' Přidáme startovní bod úseku
            result.Add(p1)

            ' Spočítáme skutečnou vzdálenost mezi body
            Dim realDistKm As Double = TrackConverter.HaversineDistance(p1.Location.Lat, p1.Location.Lon, p2.Location.Lat, p2.Location.Lon, "km")

            ' Pokud je úsek delší než povolený limit, vložíme body
            If realDistKm > maxDistKm Then
                Dim pocetNoveVlozenychBodů As Integer = CInt(Math.Floor(realDistKm / maxDistKm))

                For j As Integer = 1 To pocetNoveVlozenychBodů
                    ' Lineární interpolace (vypočítáme, kde v rámci úseku bod leží - hodnota 0 až 1)
                    Dim f As Double = j / (pocetNoveVlozenychBodů + 1)

                    ' Výpočet nových souřadnic
                    Dim newLat = p1.Location.Lat + (p2.Location.Lat - p1.Location.Lat) * f
                    Dim newLon = p1.Location.Lon + (p2.Location.Lon - p1.Location.Lon) * f

                    ' Interpolace času (aby i časová osa byla plynulá)
                    Dim ticksDiff = p2.Time.Ticks - p1.Time.Ticks
                    Dim newTime = New DateTime(p1.Time.Ticks + CLng(ticksDiff * f))

                    ' Vytvoření nového bodu (předpokládám tvůj konstruktor)
                    Dim interpolatedPoint As New TrackGeoPoint With {
                .Location = New Coordinates With {.Lat = newLat, .Lon = newLon},
                .Time = newTime,
                .name = "interpolated point"
            }
                    result.Add(interpolatedPoint)
                Next
            End If
        Next

        ' Nakonec přidáme úplně poslední bod celé trasy
        result.Add(points.Last())

        Return result
    End Function

    ' Výpočet kolmé vzdálenosti bodu od úsečky (zjednodušeně pro malé vzdálenosti)
    Private Shared Function PerpendicularDistancekm(p As TrackGeoPoint, lineStart As TrackGeoPoint, lineEnd As TrackGeoPoint) As Double
        ' Použijeme Haversine pro převod na km, aby epsilon odpovídalo metrům
        Dim l2 As Double = TrackConverter.HaversineDistance(lineStart.Location.Lat, lineStart.Location.Lon, lineEnd.Location.Lat, lineEnd.Location.Lon, "km")
        If l2 = 0 Then Return TrackConverter.HaversineDistance(p.Location.Lat, p.Location.Lon, lineStart.Location.Lat, lineStart.Location.Lon, "km")

        ' Standardní vzorec pro vzdálenost bodu od přímky v rovině (pro GPS na malé ploše stačí)
        ' Pro kynologii na ploše pár set metrů je tato aproximace naprosto v pořádku
        Return Math.Abs((lineEnd.Location.Lat - lineStart.Location.Lat) * (lineStart.Location.Lon - p.Location.Lon) - (lineStart.Location.Lat - p.Location.Lat) * (lineEnd.Location.Lon - lineStart.Location.Lon)) /
           Math.Sqrt(Math.Pow(lineEnd.Location.Lat - lineStart.Location.Lat, 2) + Math.Pow(lineEnd.Location.Lon - lineStart.Location.Lon, 2)) * 111.32 ' Převod stupňů na km cca
    End Function

    ''' <summary>
    ''' Calculates bounding box (min/max lat/lon) from a list of geographical tracks.
    ''' </summary>
    ''' <param name="_tracksAsGeoPoints">List of tracks containing geographical points.</param>
    Public Sub SetCoordinatesBounds(_tracksAsGeoPoints As List(Of TrackAsGeoPoints))
        For Each Track In _tracksAsGeoPoints
            For Each geoPoint As TrackGeoPoint In Track.TrackGeoPoints
                minLat = Math.Min(minLat, geoPoint.Location.Lat)
                maxLat = Math.Max(maxLat, geoPoint.Location.Lat)
                minLon = Math.Min(minLon, geoPoint.Location.Lon)
                maxLon = Math.Max(maxLon, geoPoint.Location.Lon)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Converts geographical points to pixel coordinates for drawing on a map image.
    ''' </summary>
    ''' <param name="_tracksAsGeoPoints">List of geographical tracks.</param>
    ''' <param name="minTileX">X index of the top-left map tile.</param>
    ''' <param name="minTileY">Y index of the top-left map tile.</param>
    ''' <param name="zoom">Zoom level of the tile map.</param>
    ''' <returns>List of <see cref="TrackAsPointsF"/> with 2D screen coordinates and timestamps.</returns>
    Public Function ConvertTracksGeoPointsToPointsF(_tracksAsGeoPoints As List(Of TrackAsGeoPoints), minTileX As Single, minTileY As Single, zoom As Integer) As List(Of TrackAsPointsF)
        'Dim latDistancePerDegree As Double = 111_320.0
        'Dim centerLat As Double = (minLat + maxLat) / 2
        'Dim lonDistancePerDegree As Double = Math.Cos(centerLat * Math.PI / 180) * latDistancePerDegree
        'Dim widthInMeters As Double = (maxLon - minLon) * lonDistancePerDegree
        'Dim heightInMeters As Double = (maxLat - minLat) * latDistancePerDegree

        Dim _tracksAsPointsF As New List(Of TrackAsPointsF)
        For Each Track In _tracksAsGeoPoints
            Dim _TrackAsPointsF = ConvertTrackGeoPointsToPointsF(Track, minTileX, minTileY, zoom)
            _tracksAsPointsF.Add(_TrackAsPointsF)
        Next
        Return _tracksAsPointsF
    End Function

    Public Function ConvertTrackGeoPointsToPointsF(track As TrackAsGeoPoints, minTileX As Single, minTileY As Single, zoom As Integer) As TrackAsPointsF
        If track Is Nothing Then Return New TrackAsPointsF(TrackType.Unknown, New List(Of TrackPointF))
        Dim _TrackAsPointsF As New TrackAsPointsF(track.TrackType, New List(Of TrackPointF))
        For Each geoPoint As TrackGeoPoint In track.TrackGeoPoints
            Dim pt = LatLonToPixel(geoPoint.Location.Lat, geoPoint.Location.Lon, zoom, minTileX, minTileY)
            Dim _trackpointF As New TrackPointF With {
                .Location = New PointF With {.X = pt.X, .Y = pt.Y},
                .Time = geoPoint.Time,
                .Name = geoPoint.name
            }
            _TrackAsPointsF.TrackPointsF.Add(_trackpointF)
        Next
        Return _TrackAsPointsF
    End Function

    ''' <summary>
    ''' Converts geographical coordinates (lat, lon) to pixel coordinates within a composite tile image.
    ''' </summary>
    ''' <param name="lat">Latitude in decimal degrees.</param>
    ''' <param name="lon">Longitude in decimal degrees.</param>
    ''' <param name="zoom">Zoom level of the tile map.</param>
    ''' <param name="minTileX">X index of the top-left tile.</param>
    ''' <param name="minTileY">Y index of the top-left tile.</param>
    ''' <returns>PointF with X and Y pixel positions in the tile image.</returns>
    Function LatLonToPixel(lat As Double, lon As Double, zoom As Integer, minTileX As Integer, minTileY As Integer) As PointF
        Dim n = Math.Pow(2, zoom)
        Dim tileX = (lon + 180.0) / 360.0 * n
        Dim tileY = (1 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2 * n
        Dim pixelX = CSng((tileX - minTileX) * 256)
        Dim pixelY = CSng((tileY - minTileY) * 256)
        Return New PointF(pixelX, pixelY)
    End Function

    ''' <summary>
    ''' Selects all &lt;trkpt&gt; nodes from a GPX track node.
    ''' </summary>
    ''' <param name="trkNode">The GPX &lt;trk&gt; node.</param>
    ''' <returns>XmlNodeList containing all &lt;trkpt&gt; elements.</returns>
    Function SelectTrkptNodes(trkNode As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(trkNode.OwnerDocument.NameTable)
        Dim ns As String = trkNode.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return trkNode.SelectNodes(".//gpx:trkpt", nsmgr)
    End Function

    ''' <summary>
    ''' Selects a single child node from a parent node, using the GPX namespace.
    ''' </summary>
    ''' <param name="childName">Name of the child element to select (e.g., "time").</param>
    ''' <param name="parent">The parent XmlNode (e.g., trkpt).</param>
    ''' <returns>The selected XmlNode, or Nothing if not found.</returns>
    Public Shared Function SelectSingleChildNode(childName As String, parent As XmlNode) As XmlNode
        Dim nsmgr As New XmlNamespaceManager(parent.OwnerDocument.NameTable)
        Dim ns As String = parent.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return parent.SelectSingleNode($"gpx:{childName}", nsmgr)
    End Function

    ' Metoda pro výběr poduzlů z uzlu Node

    Public Function SelectChildNodes(childName As String, parent As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(parent.OwnerDocument.NameTable)
        Dim ns As String = parent.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return parent.SelectNodes($"gpx:{childName}", nsmgr)
    End Function

    Public Shared Function CreateAndAddElement(parentNode As XmlElement,
                                XpathchildNodeName As String,
                                value As String,
                                insertAfter As Boolean,
                                Optional attName As String = "",
                                Optional attValue As String = ""
                               ) As XmlNode



        Dim childNodes As XmlNodeList = SelectAllChildNodes(XpathchildNodeName, parentNode)

        ' Kontrola duplicity
        For Each node As XmlNode In childNodes
            If (node.Attributes(attName)?.Value = attValue) Then ' zkontroluje zda node s atributem attvalue už neexistuje:
                'node.RemoveAll() ' odstraní všechny podřízené uzly, pokud existují
                node.InnerText = value ' nastaví text na nový
                'If node IsNot Nothing AndAlso node.ParentNode IsNot Nothing Then
                '    node.ParentNode.RemoveChild(node)
                'End If
                Return node ' nalezen existující uzel, končíme
            End If
        Next

        ' Pokud jsme žádný nenalezli, tak ho přidáme
        Dim insertedNode As XmlNode = Nothing
        Dim childNode As XmlElement = CreateElement(XpathchildNodeName, parentNode)
        childNode.InnerText = value
        If attValue <> "" Then childNode.SetAttribute(attName, attValue)
        Debug.WriteLine($"Přidávám nový uzel {XpathchildNodeName} s atributem {attName}={attValue} a textem '{value}'.")

        If childNodes.Count = 0 OrElse insertAfter Then
            insertedNode = parentNode.AppendChild(childNode)
        Else
            insertedNode = parentNode.InsertBefore(childNode, childNodes(0))
        End If

        Return insertedNode
    End Function

    ' Metoda pro rekurentní výběr všech poduzlů z uzlu Node
    Public Shared Function SelectAllChildNodes(XpathChildName As String, node As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(node.OwnerDocument.NameTable)
        Dim ns As String = node.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return node.SelectNodes(".//" & XpathChildName, nsmgr)
    End Function

    Public Shared Function CreateElement(nodename As String, parent As XmlNode, Optional _namespaceUri As String = Nothing) As XmlNode
        Dim xmlDoc As XmlDocument = parent.OwnerDocument
        If _namespaceUri IsNot Nothing Then
            ' Pokud je zadán jmenný prostor, použijeme ho

            Return xmlDoc.CreateElement(nodename, _namespaceUri)
        End If
        Return xmlDoc.CreateElement(nodename, xmlDoc.DocumentElement.NamespaceURI)
    End Function





    ''' <summary>
    ''' Approximate lat/long conversion to local coordinates in metres
    ''' </summary>
    ''' <param name="lat"></param>
    ''' <param name="lon"></param>
    ''' <param name="lat0"></param>
    ''' <param name="lon0"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    Public Sub LatLonToXY(lat As Double, lon As Double, lat0 As Double, lon0 As Double, ByRef x As Double, ByRef y As Double)
        Dim R As Double = 6371000.0 ' poloměr Země v m
        Dim dLat As Double = (lat - lat0) * Math.PI / 180.0
        Dim dLon As Double = (lon - lon0) * Math.PI / 180.0
        Dim meanLat As Double = (lat + lat0) / 2.0 * Math.PI / 180.0
        x = R * dLon * Math.Cos(meanLat)
        y = R * dLat
    End Sub



    ' Function to calculate the distance in km between two GPS points using the Haversine formula
    Public Shared Function HaversineDistance(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double, units As String) As Double
        Dim dLat As Double = DegToRad(lat2 - lat1)
        Dim dLon As Double = DegToRad(lon2 - lon1)
        ' Constants for converting degrees to radians and Earth's radius
        Const EARTH_RADIUS As Double = 6371 ' Earth's radius in kilometers

        Dim a As Double = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
        Dim c As Double = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))

        If units = "km" Then
            Return EARTH_RADIUS * c ' Result in kilometers
        ElseIf units = "m" Then
            Return EARTH_RADIUS * c * 1000 'result in metres
        Else
            Return EARTH_RADIUS * c ' Result in kilometers
        End If
    End Function
    ' Function to convert degrees to radians
    Private Shared Function DegToRad(degrees As Double) As Double
        Const PI As Double = 3.14159265358979
        Return degrees * PI / 180
    End Function

    Public Function PromptForStartTime(trackName As String, start_end As String, Optional maxTries As Integer = 3) As DateTime?
        Dim input As String
        Dim parsedDate As DateTime
        Dim attempt As Integer = 0

        While attempt < maxTries
            input = InputBox($"There is a missing {start_end} time in the {trackName} track." & vbCrLf &
                         "Enter the time in the format: yyyy-MM-ddTHH:mm:ss",
                         "Fill in the time",
                         Now.ToString("yyyy-MM-ddTHH:mm:ss"), MessageBoxIcon.Warning)

            ' Uživatel kliknul "Zrušit" nebo nechal prázdné → přerušit
            If String.IsNullOrWhiteSpace(input) Then Return Nothing

            If DateTime.TryParse(input, parsedDate) Then
                Return parsedDate
            Else
                MessageBox.Show("Invalid date/time format. Try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                attempt += 1
            End If
        End While

        ' Pokud se nepodařilo ani na třetí pokus, návrat Nothing
        Return Nothing
    End Function



End Class
