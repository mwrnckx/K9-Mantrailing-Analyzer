
Imports System.Drawing
Imports System.IO
Imports System.Text.Json.Serialization
Imports System.Windows.Forms

Namespace TrackVideoExporter


    ''' <summary>
    ''' Class responsible for creating overlay videos from GPS tracks.
    ''' </summary>
    Public Class VideoExportManager

        Private converter As TrackConverter
        Private encoder As FfmpegVideoEncoder
        Private FFMpegPath As String
        ''' <summary>
        ''' Directory where output images and video will be saved.
        ''' </summary>
        Private outputDir As DirectoryInfo
        Private windDirection As Double?
        Private windSpeed As Double?
        Private videoSettings As VideoSettingsConfig
        Private backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single) = (Nothing, 0, 0)
        Private LocalisedReports As New Dictionary(Of String, TrailReport)
        'Private textParts As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))
        'Private textPartsEng As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))

        ''' <summary>
        ''' Raised when a non-critical warning occurs during processing.
        ''' </summary>
        Public Event WarningOccurred(message As String, _color As Color)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="VideoExportManager"/> class.
        ''' </summary>
        ''' <param name="FFMpegPath">Path to the FFMpeg executable.</param>
        ''' <param name="outputDir">Output directory for generated images and video.</param>
        ''' <param name="windDir">Optional wind direction in degrees.</param>
        ''' <param name="windSpeed">Optional wind speed.</param>
        ''' <param name="LocalisedReports">Optional dictionary of localised trail reports.</param>
        Public Sub New(FFMpegPath As String, outputDir As DirectoryInfo,
                       Optional windDir As Double? = Nothing,
                       Optional windSpeed As Double? = Nothing,
                         Optional LocalisedReports As Dictionary(Of String, TrailReport) = Nothing, Optional videoSettings As VideoSettingsConfig = Nothing)
            Me.FFMpegPath = FFMpegPath
            Me.outputDir = outputDir
            Me.windDirection = windDir

            Me.windSpeed = windSpeed
            Me.LocalisedReports = LocalisedReports
            ' Zde nastavíme vaši požadovanou "konstantní" výchozí hodnotu
            If videoSettings Is Nothing Then
                Me.videoSettings = New VideoSettingsConfig
            Else
                Me.videoSettings = videoSettings
            End If

            converter = New TrackConverter()

        End Sub

        ''' <summary>
        ''' Converts TRK nodes to geo points and generates an overlay video.
        ''' </summary>
        '''<param name="localisedReports"></param>
        '''<param name="_tracksAsTrkNode"> </param>
        Public Async Function CreateVideoFromTrkNodes(_tracksAsTrkNode As List(Of TrackAsTrkNode),
                                                      Optional maxDeviationPoints As TrackAsGeoPoints = Nothing,
                                                      Optional waypoints As TrackAsTrkPts = Nothing,
                                                      Optional LocalisedReports As Dictionary(Of String, TrailReport) = Nothing,
                                                      Optional bestCheckPointIndex As Integer = 0) As Task(Of Boolean)
            Dim tracksAsTrkPts = converter.ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode)
            Me.LocalisedReports = LocalisedReports
            Return Await CreateVideoFromTrkPts(tracksAsTrkPts,
                                               maxDeviationPoints,
                                               waypoints, Me.LocalisedReports,
                                               bestCheckPointIndex)
        End Function

        ''' <summary>
        ''' Converts TRK points to geo points and creates a video.
        ''' </summary>
        ''' <param name="_tracksAsTrkPts">List of tracks in TRK point format.</param>
        ''' <param name="LocalisedReports">Dictionary of localised trail reports.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromTrkPts(
            _tracksAsTrkPts As List(Of TrackAsTrkPts),
                maxDevPointsAsGeoPoints As TrackAsGeoPoints,
            waypoints As TrackAsTrkPts,
               LocalisedReports As Dictionary(Of String, TrailReport),
            Optional bestCheckPointIndex As Integer = 0) As Task(Of Boolean)

            Dim wayPointsAsGeoPoints As TrackAsGeoPoints = converter.ConvertTrackTrkPtsToGeoPoints(waypoints)
            Dim tracksAsGeoPoints As List(Of TrackAsGeoPoints) = converter.ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts)

            'vyhlazení GPS šumu!!!
            Dim purifiedTracksAsGeoPoints As New List(Of TrackAsGeoPoints)
            For Each _track As TrackAsGeoPoints In tracksAsGeoPoints

                If _track.TrackGeoPoints.Count > 30 Then ' vyhlazují se jen delší trasy 
                    Dim purifiedTrack As TrackAsGeoPoints
                    purifiedTrack = TrackConverter.PurifyTrackAsGeoPoints(_track, 10) ' Filtr pro maximální rychlost 10 km/h
                    '_track = purifiedTrack
                    purifiedTracksAsGeoPoints.Add(purifiedTrack)
                End If

            Next

            'Dim maxDevPointsAsGeoPoints As TrackAsGeoPoints = converter.ConvertTrackTrkPtsToGeoPoints(maxDeviation)
            Me.LocalisedReports = LocalisedReports
            Return Await CreateVideoFromGeoPoints(purifiedTracksAsGeoPoints, maxDevPointsAsGeoPoints, wayPointsAsGeoPoints, bestCheckPointIndex)
        End Function

        ''' <summary>
        ''' Creates a video from tracks represented as geo points (latitude/longitude).
        ''' </summary>
        ''' <param name="_tracksAsGeoPoints">List of tracks with geographic coordinates.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromGeoPoints(
            _tracksAsGeoPoints As List(Of TrackAsGeoPoints),
               Optional maxDeviationAsGeoPoints As TrackAsGeoPoints = Nothing,
               Optional waypointsAsGeoPoints As TrackAsGeoPoints = Nothing, Optional bestCheckPointIndex As Integer = 0) As Task(Of Boolean)


            converter.SetCoordinatesBounds(_tracksAsGeoPoints)
            Dim downloader As New OsmTileDownloader()
            backgroundTiles = Await downloader.GetMapBitmap(
                converter.minLat, converter.maxLat,
                converter.minLon, converter.maxLon)


            Dim _TracksAsPointsF As List(Of TrackAsPointsF) =
                converter.ConvertTracksGeoPointsToPointsF(
                     _tracksAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, OsmTileDownloader.zoom)
            Dim wayPointsAsPointsF As TrackAsPointsF =
                converter.ConvertTrackGeoPointsToPointsF(
                    waypointsAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, OsmTileDownloader.zoom)

            Dim maxDeviationPointsAsPointsF As TrackAsPointsF =
                converter.ConvertTrackGeoPointsToPointsF(
                    maxDeviationAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, OsmTileDownloader.zoom)
            Dim maxDeviationMetres As Double = 0
            If maxDeviationAsGeoPoints IsNot Nothing Then
                maxDeviationMetres = TrackConverter.HaversineDistance(maxDeviationAsGeoPoints.TrackGeoPoints(0).Location.Lat, maxDeviationAsGeoPoints.TrackGeoPoints(0).Location.Lon, maxDeviationAsGeoPoints.TrackGeoPoints(1).Location.Lat, maxDeviationAsGeoPoints.TrackGeoPoints(1).Location.Lon, "m")
            End If
            Dim latitude As Double = _tracksAsGeoPoints(0).TrackGeoPoints(0).Location.Lat
            Return Await CreateVideoFromPointsF(_TracksAsPointsF, maxDeviationPointsAsPointsF, wayPointsAsPointsF, maxDeviationMetres, bestCheckPointIndex, latitude)

        End Function





        ''' <summary>
        ''' Creates a video from 2D screen points (with timestamps).
        ''' </summary>
        ''' <param name="_tracksAsPointsF">List of 2D track points with timing information.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromPointsF(
            _tracksAsPointsF As List(Of TrackAsPointsF),
            Optional maxDeviationAsPointsF As TrackAsPointsF = Nothing,
            Optional waypointsAsPointsF As TrackAsPointsF = Nothing, Optional maxDeviationMetres As Double = 0, Optional bestCheckPointIndex As Integer = 0, Optional latitude As Double = 50) As Task(Of Boolean)

            Dim pngDir As DirectoryInfo = Nothing
            Dim pngCreator As PngSequenceCreator = Nothing

            Await Task.Run(Sub()

                               Dim renderer As New PngRenderer(windDirection, windSpeed, Me.backgroundTiles, Me.videoSettings, latitude)
                               renderer.CreateWindArrowBitmap(outputDir)
                               Dim staticBgTransparent = renderer.RenderStaticTransparentBackground(_tracksAsPointsF, backgroundTiles, waypointsAsPointsF, bestCheckPointIndex)
                               Dim staticBgMap = renderer.RenderStaticMap(_tracksAsPointsF, backgroundTiles, maxDeviationAsPointsF, waypointsAsPointsF, maxDeviationMetres, bestCheckPointIndex)


                               pngCreator = New PngSequenceCreator(renderer, videoSettings)

                               Dim pngTimes = pngCreator.GetPngTimes(_tracksAsPointsF)
                               pngCreator.CreateReports(outputDir, Me.LocalisedReports)
                               pngCreator.CreateFrames(_tracksAsPointsF,
                                        staticBgTransparent, staticBgMap, backgroundTiles.bgmap,
                                        outputDir, pngTimes)

                           End Sub)

            Dim outputFile = IO.Path.Combine(outputDir.FullName, "overlay")
            encoder = New FfmpegVideoEncoder()
            Return Await encoder.EncodeFromPngs(FFMpegPath, outputDir, outputFile)

        End Function

    End Class

    Public Class VideoSettingsConfig
        <JsonPropertyName("videoWidth")>
        Public Property VideoWidth As Integer = 1920

        <JsonPropertyName("videoHeight")>
        Public Property VideoHeight As Integer = 1080

        <JsonPropertyName("trailWidth")>
        Public Property TrailWidth_m As Integer = 2 'trail width in metres

        <JsonPropertyName("dogTrailSpeedColor")>
        Public Property DogTrailSpeedColor As Boolean = False 'dog trail color according to speed

        <JsonPropertyName("videoMode")>
        Public Property VideoMode As VideoModeEnum = VideoModeEnum.light

        Public Enum VideoModeEnum
            light
            dark
        End Enum

    End Class

End Namespace

