Imports System.IO
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports A = DocumentFormat.OpenXml.Drawing
Imports DW = DocumentFormat.OpenXml.Drawing.Wordprocessing
Imports PIC = DocumentFormat.OpenXml.Drawing.Pictures

''' <summary>
''' Generates a printable DOCX diploma for the winner of a mantrailing competition.
''' Call GenerateDiploma() with the relevant data and a Save dialog will appear.
''' </summary>
Public Class DiplomaGenerator

    ''' <summary>
    ''' Main entry point — call this from your button/menu handler.
    ''' </summary>
    ''' <param name="category">Category name, e.g. "Advanced"</param>
    ''' <param name="dogName">Name of the dog</param>
    ''' <param name="handlerName">Name of the handler</param>
    ''' <param name="totalScore">Total score (points)</param>
    ''' <param name="bonusScore">Bonus points total</param>
    ''' <param name="eventDate">Date of the event</param>
    ''' <param name="placement">Placement, e.g. 1</param>
    ''' <param name="workingDirectory">Base working directory of the application (for logo lookup)</param>
    ''' <param name="language">Language code: "cs" for Czech, "en" for English</param>
    Public Shared Sub GenerateDiploma(
            category As String,
            dogName As String,
            handlerName As String,
            totalScore As Integer,
            bonusScore As Integer,
            eventDate As DateTime,
            placement As Integer,
            workingDirectory As String,
            language As String)

        ' --- Save dialog ---
        Using dlg As New SaveFileDialog()
            dlg.Title = If(language = "cs", "Uložit diplom", "Save diploma")
            dlg.Filter = "Word Document (*.docx)|*.docx"
            dlg.FileName = $"diploma_{category}_{dogName}_{eventDate:yyyy-MM-dd}.docx"
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) &
                                   Path.DirectorySeparatorChar & "Downloads"

            If dlg.ShowDialog() <> DialogResult.OK Then Return

            Dim outputPath As String = dlg.FileName
            BuildDocx(outputPath, category, dogName, handlerName, totalScore, bonusScore,
                      eventDate, placement, workingDirectory, language)

            ' Offer to open the file
            Dim msg As String = If(language = "cs",
                $"Diplom byl uložen:{Environment.NewLine}{outputPath}{Environment.NewLine}{Environment.NewLine}Otevřít nyní?",
                $"Diploma saved:{Environment.NewLine}{outputPath}{Environment.NewLine}{Environment.NewLine}Open now?")
            Dim title As String = If(language = "cs", "Diplom vytvořen", "Diploma created")
            If MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information) = DialogResult.Yes Then
                Process.Start(New ProcessStartInfo(outputPath) With {.UseShellExecute = True})
            End If
        End Using
    End Sub

    ' -------------------------------------------------------------------------
    ' Internal document builder
    ' -------------------------------------------------------------------------
    Private Shared Sub BuildDocx(
            outputPath As String,
            category As String,
            dogName As String,
            handlerName As String,
            totalScore As Integer,
            bonusScore As Integer,
            eventDate As DateTime,
            placement As Integer,
            workingDirectory As String,
            language As String)

        ' Localised strings
        Dim lblTitle As String = If(language = "cs", "DIPLOM", "DIPLOMA")
        Dim lblSubtitle As String = If(language = "cs", "Mantrailing – soutěžní výsledky", "Mantrailing – Competition Results")
        Dim lblPlacement As String = If(language = "cs", OrdinalCzech(placement), OrdinalEnglish(placement))
        Dim lblCategory As String = If(language = "cs", "Kategorie", "Category")
        Dim lblDog As String = If(language = "cs", "Pes", "Dog")
        Dim lblHandler As String = If(language = "cs", "Psovod", "Handler")
        Dim lblScore As String = If(language = "cs", "Body celkem", "Total score")
        Dim lblBonus As String = If(language = "cs", "Bonusové body", "Bonus points")
        Dim lblDate As String = If(language = "cs", "Datum konání", "Event date")
        Dim lblSignature As String = If(language = "cs", "Podpis organizátora", "Organiser signature")
        Dim dateStr As String = eventDate.ToString(If(language = "cs", "d. MMMM yyyy",
                                                            "MMMM d, yyyy"),
                                                         New Globalization.CultureInfo(
                                                             If(language = "cs", "cs-CZ", "en-GB")))

        ' Gold accent colour (hex without #)
        Const gold As String = "C8960C"
        Const darkGray As String = "2C2C2C"
        Const lightGold As String = "FFF3CC"

        ' A4 landscape in DXA  (1 inch = 1440 DXA)
        ' Short edge = 11906, long edge = 16838
        Const pageW As UInt32 = 16838UI   ' long edge becomes width in landscape
        Const pageH As UInt32 = 11906UI
        Const marginLR As UInt32 = 1080UI  ' ~1.9 cm margins left/right
        Const marginTB As UInt32 = 720UI   ' ~1.3 cm margins top/bottom
        Dim contentW As Integer = CInt(pageW) - CInt(marginLR) * 2  ' ≈ 14678 DXA

        Using doc As WordprocessingDocument =
                WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document)

            Dim mainPart As MainDocumentPart = doc.AddMainDocumentPart()
            mainPart.Document = New Document()
            Dim body As New Body()

            ' ── Page layout (landscape A4) ──────────────────────────────────
            Dim sectPr As New SectionProperties(
                New PageSize() With {
                    .Width = pageW,
                    .Height = pageH,
                    .Orient = PageOrientationValues.Landscape
                },
                New PageMargin() With {
                    .Top = CInt(marginTB),
                    .Bottom = CInt(marginTB),
                    .Left = marginLR,
                    .Right = marginLR
                }
            )

            ' ── Helper: create a styled paragraph ───────────────────────────
            ' (defined as local lambdas for conciseness)

            ' Gold top border line
            body.AppendChild(BorderParagraph(gold, contentW))

            ' ── Logo + Title row (table with 2 columns) ─────────────────────
            Dim logoPath As String = FindLogo(workingDirectory)
            Dim headerTable As Table = BuildHeaderTable(mainPart, logoPath, lblTitle,
                                                        lblSubtitle, gold, darkGray, contentW)
            body.AppendChild(headerTable)

            ' Spacing
            body.AppendChild(SpacerParagraph(160))

            ' ── Gold banner: placement ───────────────────────────────────────
            body.AppendChild(BannerParagraph(lblPlacement, gold, 52))

            ' Spacing
            body.AppendChild(SpacerParagraph(120))

            ' ── Info table ──────────────────────────────────────────────────
            Dim infoTable As Table = BuildInfoTable(
                contentW, gold, lightGold, darkGray, language,
                lblCategory, category,
                lblDog, dogName,
                lblHandler, handlerName,
                lblScore, totalScore.ToString(),
                lblBonus, bonusScore.ToString(),
                lblDate, dateStr)
            body.AppendChild(infoTable)

            ' Spacing
            body.AppendChild(SpacerParagraph(400))

            ' ── Signature line ──────────────────────────────────────────────
            body.AppendChild(SignatureParagraph(lblSignature, darkGray, contentW))

            ' Gold bottom border line
            body.AppendChild(BorderParagraph(gold, contentW))

            ' Attach section properties
            body.AppendChild(sectPr)
            mainPart.Document.AppendChild(body)
            mainPart.Document.Save()
        End Using
    End Sub

    ' ── Logo lookup ──────────────────────────────────────────────────────────
    Private Shared Function FindLogo(workingDirectory As String) As String
        For Each ext In {"png", "jpg", "jpeg"}
            Dim p As String = Path.Combine(workingDirectory, "Resources", "images", $"logo.{ext}")
            If File.Exists(p) Then Return p
        Next
        Return Nothing
    End Function

    ' ── Ordinal helpers ──────────────────────────────────────────────────────
    Private Shared Function OrdinalCzech(n As Integer) As String
        Return $"{n}. místo"
    End Function

    Private Shared Function OrdinalEnglish(n As Integer) As String
        Dim suffix As String = If(n = 1, "st", If(n = 2, "nd", If(n = 3, "rd", "th")))
        Return $"{n}{suffix} place"
    End Function

    ' ── Gold border paragraph ────────────────────────────────────────────────
    Private Shared Function BorderParagraph(color As String, contentW As Integer) As Paragraph
        Dim p As New Paragraph()
        Dim pPr As New ParagraphProperties()
        pPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "0"})
        pPr.AppendChild(New ParagraphBorders(
            New BottomBorder() With {
                .Val = BorderValues.Single,
                .Size = 12UI,
                .Color = color,
                .Space = 1UI
            }))
        p.AppendChild(pPr)
        Return p
    End Function

    ' ── Spacer paragraph ────────────────────────────────────────────────────
    Private Shared Function SpacerParagraph(twips As Integer) As Paragraph
        Dim p As New Paragraph()
        Dim pPr As New ParagraphProperties()
        pPr.AppendChild(New SpacingBetweenLines() With {
            .Before = twips.ToString(), .After = "0"})
        p.AppendChild(pPr)
        Return p
    End Function

    ' ── Banner paragraph (placement) ────────────────────────────────────────
    Private Shared Function BannerParagraph(text As String, color As String, fontSize As Integer) As Paragraph
        Dim p As New Paragraph()
        Dim pPr As New ParagraphProperties()
        pPr.AppendChild(New Justification() With {.Val = JustificationValues.Center})
        pPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "0"})
        p.AppendChild(pPr)
        Dim r As New Run()
        Dim rPr As New RunProperties()
        rPr.AppendChild(New Bold())
        rPr.AppendChild(New Color() With {.Val = color})
        rPr.AppendChild(New FontSize() With {.Val = (fontSize * 2).ToString()})
        rPr.AppendChild(New RunFonts() With {.Ascii = "Georgia", .HighAnsi = "Georgia"})
        r.AppendChild(rPr)
        r.AppendChild(New Text(text))
        p.AppendChild(r)
        Return p
    End Function

    ' ── Header table (logo left, title right) ────────────────────────────────
    Private Shared Function BuildHeaderTable(
            mainPart As MainDocumentPart,
            logoPath As String,
            title As String,
            subtitle As String,
            gold As String,
            darkGray As String,
            contentW As Integer) As Table

        Dim tbl As New Table()
        Dim tblPr As New TableProperties()
        tblPr.AppendChild(New TableStyle() With {.Val = "TableGrid"})
        tblPr.AppendChild(New TableWidth() With {
            .Width = contentW.ToString(), .Type = TableWidthUnitValues.Dxa})
        tblPr.AppendChild(New TableBorders(
            New TopBorder() With {.Val = BorderValues.None},
            New BottomBorder() With {.Val = BorderValues.None},
            New LeftBorder() With {.Val = BorderValues.None},
            New RightBorder() With {.Val = BorderValues.None},
            New InsideHorizontalBorder() With {.Val = BorderValues.None},
            New InsideVerticalBorder() With {.Val = BorderValues.None}))
        tbl.AppendChild(tblPr)
        tbl.AppendChild(New TableGrid(
            New GridColumn() With {.Width = "1800"},
            New GridColumn() With {.Width = (contentW - 1800).ToString()}))

        Dim row As New TableRow()

        ' Left cell: logo
        Dim logoCell As New TableCell()
        Dim logoCellPr As New TableCellProperties()
        logoCellPr.AppendChild(New TableCellWidth() With {.Width = "1800", .Type = TableWidthUnitValues.Dxa})
        logoCellPr.AppendChild(New TableCellVerticalAlignment() With {.Val = TableVerticalAlignmentValues.Center})
        logoCell.AppendChild(logoCellPr)

        If logoPath IsNot Nothing Then
            logoCell.AppendChild(BuildLogoParagraph(mainPart, logoPath))
        Else
            logoCell.AppendChild(New Paragraph())
        End If

        ' Right cell: title text
        Dim titleCell As New TableCell()
        Dim titleCellPr As New TableCellProperties()
        titleCellPr.AppendChild(New TableCellWidth() With {
            .Width = (contentW - 1800).ToString(), .Type = TableWidthUnitValues.Dxa})
        titleCellPr.AppendChild(New TableCellVerticalAlignment() With {.Val = TableVerticalAlignmentValues.Center})
        titleCell.AppendChild(titleCellPr)

        ' Title
        Dim pTitle As New Paragraph()
        Dim pTitlePr As New ParagraphProperties()
        pTitlePr.AppendChild(New Justification() With {.Val = JustificationValues.Center})
        pTitlePr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "60"})
        pTitle.AppendChild(pTitlePr)
        Dim rTitle As New Run()
        Dim rTitlePr As New RunProperties()
        rTitlePr.AppendChild(New Bold())
        rTitlePr.AppendChild(New Color() With {.Val = gold})
        rTitlePr.AppendChild(New FontSize() With {.Val = "80"})  ' 40pt
        rTitlePr.AppendChild(New RunFonts() With {.Ascii = "Georgia", .HighAnsi = "Georgia"})
        rTitle.AppendChild(rTitlePr)
        rTitle.AppendChild(New Text(title))
        pTitle.AppendChild(rTitle)
        titleCell.AppendChild(pTitle)

        ' Subtitle
        Dim pSub As New Paragraph()
        Dim pSubPr As New ParagraphProperties()
        pSubPr.AppendChild(New Justification() With {.Val = JustificationValues.Center})
        pSubPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "0"})
        pSub.AppendChild(pSubPr)
        Dim rSub As New Run()
        Dim rSubPr As New RunProperties()
        rSubPr.AppendChild(New Color() With {.Val = darkGray})
        rSubPr.AppendChild(New FontSize() With {.Val = "28"})  ' 14pt
        rSubPr.AppendChild(New Italic())
        rSubPr.AppendChild(New RunFonts() With {.Ascii = "Georgia", .HighAnsi = "Georgia"})
        rSub.AppendChild(rSubPr)
        rSub.AppendChild(New Text(subtitle))
        pSub.AppendChild(rSub)
        titleCell.AppendChild(pSub)

        row.AppendChild(logoCell)
        row.AppendChild(titleCell)
        tbl.AppendChild(row)
        Return tbl
    End Function

    ' ── Logo image paragraph ─────────────────────────────────────────────────
    Private Shared Function BuildLogoParagraph(mainPart As MainDocumentPart, logoPath As String) As Paragraph
        Dim imgPart As ImagePart
        Dim ext As String = Path.GetExtension(logoPath).ToLower()
        Dim contentType As String = If(ext = ".png", "image/png", "image/jpeg")
        imgPart = mainPart.AddImagePart(contentType)
        Using fs As New FileStream(logoPath, FileMode.Open, FileAccess.Read)
            imgPart.FeedData(fs)
        End Using
        Dim relId As String = mainPart.GetIdOfPart(imgPart)

        ' 1700 DXA wide ≈ 3 cm, preserve aspect ratio approximately
        Dim emuW As Long = 1200000  ' ~1.3 cm in EMU
        Dim emuH As Long = 1200000

        Dim p As New Paragraph()
        Dim pPr As New ParagraphProperties()
        pPr.AppendChild(New Justification() With {.Val = JustificationValues.Center})
        pPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "0"})
        p.AppendChild(pPr)

        Dim r As New Run()
        r.AppendChild(New RunProperties())
        r.AppendChild(New Drawing(
            New DW.Inline(
                New DW.Extent() With {.Cx = emuW, .Cy = emuH},
                New DW.EffectExtent() With {.LeftEdge = 0, .TopEdge = 0, .RightEdge = 0, .BottomEdge = 0},
                New DW.DocProperties() With {.Id = 1UI, .Name = "Logo"},
                New DW.NonVisualGraphicFrameDrawingProperties(
                    New A.GraphicFrameLocks() With {.NoChangeAspect = True}),
                New A.Graphic(
                    New A.GraphicData(
                        New PIC.Picture(
                            New PIC.NonVisualPictureProperties(
                                New PIC.NonVisualDrawingProperties() With {.Id = 0UI, .Name = "logo"},
                                New PIC.NonVisualPictureDrawingProperties()),
                            New PIC.BlipFill(
                                New A.Blip() With {.Embed = relId},
                                New A.Stretch(New A.FillRectangle())),
                            New PIC.ShapeProperties(
                                New A.Transform2D(
                                    New A.Offset() With {.X = 0, .Y = 0},
                                    New A.Extents() With {.Cx = emuW, .Cy = emuH}),
                                New A.PresetGeometry(
                                    New A.AdjustValueList()) With {.Preset = A.ShapeTypeValues.Rectangle})
                        )
                    ) With {.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"}
                )
            ) With {.DistanceFromTop = 0UI, .DistanceFromBottom = 0UI,
                    .DistanceFromLeft = 0UI, .DistanceFromRight = 0UI}
        ))
        p.AppendChild(r)
        Return p
    End Function

    ' ── Info table (category, dog, handler, score, bonus, date) ─────────────
    Private Shared Function BuildInfoTable(
            contentW As Integer,
            gold As String,
            lightGold As String,
            darkGray As String,
            language As String,
            ParamArray pairs() As String) As Table

        ' pairs: label0, value0, label1, value1, ...
        Dim colLabel As Integer = CInt(contentW * 0.35)
        Dim colValue As Integer = contentW - colLabel

        Dim tbl As New Table()
        Dim tblPr As New TableProperties()
        tblPr.AppendChild(New TableWidth() With {
            .Width = contentW.ToString(), .Type = TableWidthUnitValues.Dxa})
        tblPr.AppendChild(New TableBorders(
            New TopBorder() With {.Val = BorderValues.None},
            New BottomBorder() With {.Val = BorderValues.None},
            New LeftBorder() With {.Val = BorderValues.None},
            New RightBorder() With {.Val = BorderValues.None},
            New InsideHorizontalBorder() With {.Val = BorderValues.Single, .Size = 4UI, .Color = gold},
            New InsideVerticalBorder() With {.Val = BorderValues.None}))
        tbl.AppendChild(tblPr)
        tbl.AppendChild(New TableGrid(
            New GridColumn() With {.Width = colLabel.ToString()},
            New GridColumn() With {.Width = colValue.ToString()}))

        Dim i As Integer = 0
        While i < pairs.Length - 1
            Dim lbl As String = pairs(i)
            Dim val As String = pairs(i + 1)
            Dim isAlt As Boolean = (i \ 2) Mod 2 = 1

            Dim row As New TableRow()

            ' Label cell
            Dim lblCell As New TableCell()
            Dim lblCellPr As New TableCellProperties()
            lblCellPr.AppendChild(New TableCellWidth() With {
                .Width = colLabel.ToString(), .Type = TableWidthUnitValues.Dxa})
            lblCellPr.AppendChild(New TableCellVerticalAlignment() With {
                .Val = TableVerticalAlignmentValues.Center})
            If isAlt Then
                lblCellPr.AppendChild(New Shading() With {
                    .Fill = lightGold, .Val = ShadingPatternValues.Clear})
            End If
            lblCellPr.AppendChild(New TableCellMargin() With {
                .TopMargin = New TopMargin() With {.Width = "80", .Type = TableWidthUnitValues.Dxa},
                .BottomMargin = New BottomMargin() With {.Width = "80", .Type = TableWidthUnitValues.Dxa},
                .LeftMargin = New LeftMargin() With {.Width = "160", .Type = TableWidthUnitValues.Dxa},
                .RightMargin = New RightMargin() With {.Width = "160", .Type = TableWidthUnitValues.Dxa}})
            lblCell.AppendChild(lblCellPr)
            Dim pLbl As New Paragraph()
            Dim pLblPr As New ParagraphProperties()
            pLblPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "0"})
            pLbl.AppendChild(pLblPr)
            Dim rLbl As New Run()
            Dim rLblPr As New RunProperties()
            rLblPr.AppendChild(New Bold())
            rLblPr.AppendChild(New Color() With {.Val = gold})
            rLblPr.AppendChild(New FontSize() With {.Val = "28"})
            rLblPr.AppendChild(New RunFonts() With {.Ascii = "Arial", .HighAnsi = "Arial"})
            rLbl.AppendChild(rLblPr)
            rLbl.AppendChild(New Text(lbl))
            pLbl.AppendChild(rLbl)
            lblCell.AppendChild(pLbl)

            ' Value cell
            Dim valCell As New TableCell()
            Dim valCellPr As New TableCellProperties()
            valCellPr.AppendChild(New TableCellWidth() With {
                .Width = colValue.ToString(), .Type = TableWidthUnitValues.Dxa})
            valCellPr.AppendChild(New TableCellVerticalAlignment() With {
                .Val = TableVerticalAlignmentValues.Center})
            If isAlt Then
                valCellPr.AppendChild(New Shading() With {
                    .Fill = lightGold, .Val = ShadingPatternValues.Clear})
            End If
            valCellPr.AppendChild(New TableCellMargin() With {
                .TopMargin = New TopMargin() With {.Width = "80", .Type = TableWidthUnitValues.Dxa},
                .BottomMargin = New BottomMargin() With {.Width = "80", .Type = TableWidthUnitValues.Dxa},
                .LeftMargin = New LeftMargin() With {.Width = "160", .Type = TableWidthUnitValues.Dxa},
                .RightMargin = New RightMargin() With {.Width = "160", .Type = TableWidthUnitValues.Dxa}})
            valCell.AppendChild(valCellPr)
            Dim pVal As New Paragraph()
            Dim pValPr As New ParagraphProperties()
            pValPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "0"})
            pVal.AppendChild(pValPr)
            Dim rVal As New Run()
            Dim rValPr As New RunProperties()
            rValPr.AppendChild(New Color() With {.Val = darkGray})
            rValPr.AppendChild(New FontSize() With {.Val = "28"})
            rValPr.AppendChild(New RunFonts() With {.Ascii = "Arial", .HighAnsi = "Arial"})
            rVal.AppendChild(rValPr)
            rVal.AppendChild(New Text(val))
            pVal.AppendChild(rVal)
            valCell.AppendChild(pVal)

            row.AppendChild(lblCell)
            row.AppendChild(valCell)
            tbl.AppendChild(row)
            i += 2
        End While

        Return tbl
    End Function

    ' ── Signature line ───────────────────────────────────────────────────────
    Private Shared Function SignatureParagraph(label As String, darkGray As String, contentW As Integer) As Paragraph
        ' A centred line with underscores and the label below
        Dim p As New Paragraph()
        Dim pPr As New ParagraphProperties()
        pPr.AppendChild(New Justification() With {.Val = JustificationValues.Center})
        pPr.AppendChild(New SpacingBetweenLines() With {.Before = "0", .After = "60"})
        ' Bottom border as signature line
        pPr.AppendChild(New ParagraphBorders(
            New BottomBorder() With {
                .Val = BorderValues.Single,
                .Size = 6UI,
                .Color = darkGray,
                .Space = 1UI
            }))
        p.AppendChild(pPr)
        ' Empty run — the border acts as the line
        p.AppendChild(New Run(New RunProperties(
            New FontSize() With {.Val = "48"})))   ' tall enough for the border to show

        Return p
    End Function

End Class
