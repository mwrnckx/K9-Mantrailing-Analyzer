Imports System.IO
'Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging

' Aliasy zabraňují konfliktu s System.Drawing.Color, Font, Table atd.
Imports OXW = DocumentFormat.OpenXml.Wordprocessing
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

        If language <> "cs" AndAlso language <> "en" Then
            language = "en" ' fallback to English if unsupported language code is given
        End If

        Using dlg As New SaveFileDialog()
            dlg.Title = If(language = "cs", "Uložit diplom", "Save diploma")
            dlg.Filter = "Word Document (*.docx)|*.docx"
            Dim _fileName As String = $"diploma_{category}_{dogName}_{eventDate:yyyy-MM-dd}.docx"
            ' odstraní nepovolené znaky z názvu souboru, které by mohly způsobit chybu při ukládání
            ' Příkaz pro odstranění nepovolených znaků:
            dlg.FileName = String.Join("_", _fileName.Split(Path.GetInvalidFileNameChars()))
            ' Složka Downloads — čteme z registrů, fallback na UserProfile\Downloads
            Dim downloads As String = Convert.ToString(
                My.Computer.Registry.GetValue(
                    "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders",
                    "{374DE290-123F-4565-9164-39C4925E467B}",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")))
            dlg.InitialDirectory = downloads

            If dlg.ShowDialog() <> DialogResult.OK Then Return

            Dim outputPath As String = dlg.FileName
            BuildDocx(outputPath, category, dogName, handlerName, totalScore, bonusScore,
                      eventDate, placement, workingDirectory, language)

            Dim msg As String = If(language = "cs",
                $"Diplom byl uložen:{Environment.NewLine}{outputPath}{Environment.NewLine}{Environment.NewLine}Otevřít nyní?",
                $"Diploma saved:{Environment.NewLine}{outputPath}{Environment.NewLine}{Environment.NewLine}Open now?")
            Dim title As String = If(language = "cs", "Diplom vytvořen", "Diploma created")
            If MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information) = DialogResult.Yes Then
                Process.Start(New ProcessStartInfo(outputPath) With {.UseShellExecute = True})
            End If
        End Using
    End Sub

    ' =========================================================================
    ' Internal document builder
    ' =========================================================================
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
        Dim dateStr As String = eventDate.ToString(
            If(language = "cs", "d. MMMM yyyy", "MMMM d, yyyy"),
            New Globalization.CultureInfo(If(language = "cs", "cs-CZ", "en-GB")))

        Const gold As String = "C8960C"
        Const darkGray As String = "2C2C2C"
        Const lightGold As String = "FFF3CC"

        ' A4 landscape (DXA: 1440 = 1 inch)
        Const pageW As UInt32 = 16838UI
        Const pageH As UInt32 = 11906UI
        Const marginLR As UInt32 = 1080UI
        Const marginTB As UInt32 = 720UI
        Dim contentW As Integer = CInt(pageW) - CInt(marginLR) * 2

        Using doc As WordprocessingDocument =
                WordprocessingDocument.Create(outputPath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document)

            Dim mainPart As MainDocumentPart = doc.AddMainDocumentPart()
            mainPart.Document = New OXW.Document()
            Dim body As New OXW.Body()

            Dim sectPr As New OXW.SectionProperties(
                New OXW.PageSize() With {
                    .Width = pageW,
                    .Height = pageH,
                    .Orient = OXW.PageOrientationValues.Landscape},
                New OXW.PageMargin() With {
                    .Top = CInt(marginTB),
                    .Bottom = CInt(marginTB),
                    .Left = marginLR,
                    .Right = marginLR})

            body.AppendChild(BorderParagraph(gold))
            body.AppendChild(BuildHeaderTable(mainPart, FindLogo(workingDirectory),
                                              lblTitle, lblSubtitle, gold, darkGray, contentW))
            body.AppendChild(SpacerParagraph(160))
            body.AppendChild(BannerParagraph(lblPlacement, gold, 52))
            body.AppendChild(SpacerParagraph(120))
            body.AppendChild(BuildInfoTable(contentW, gold, lightGold, darkGray,
                                            lblCategory, category,
                                            lblDog, dogName,
                                            lblHandler, handlerName,
                                            lblScore, totalScore.ToString(),
                                            lblBonus, bonusScore.ToString(),
                                            lblDate, dateStr))
            body.AppendChild(SpacerParagraph(400))
            body.AppendChild(SignatureParagraph(lblSignature, darkGray))
            body.AppendChild(BorderParagraph(gold))
            body.AppendChild(sectPr)

            mainPart.Document.AppendChild(body)
            mainPart.Document.Save()
        End Using
    End Sub

    ' ── Logo lookup ───────────────────────────────────────────────────────────
    Private Shared Function FindLogo(workingDirectory As String) As String
        For Each ext In {"png", "jpg", "jpeg"}
            Dim p As String = Path.Combine(workingDirectory, "Resources", "images", $"logo.{ext}")
            If File.Exists(p) Then Return p
        Next
        Return Nothing
    End Function

    ' ── Ordinals ─────────────────────────────────────────────────────────────
    Private Shared Function OrdinalCzech(n As Integer) As String
        Return $"{n}. místo"
    End Function

    Private Shared Function OrdinalEnglish(n As Integer) As String
        Dim suffix As String = If(n = 1, "st", If(n = 2, "nd", If(n = 3, "rd", "th")))
        Return $"{n}{suffix} place"
    End Function

    ' ── Gold border paragraph ─────────────────────────────────────────────────
    Private Shared Function BorderParagraph(color As String) As OXW.Paragraph
        Dim p As New OXW.Paragraph()
        Dim pPr As New OXW.ParagraphProperties()
        pPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "0"})
        pPr.AppendChild(New OXW.ParagraphBorders(
            New OXW.BottomBorder() With {
                .Val = OXW.BorderValues.Single,
                .Size = 12UI,
                .Color = color,
                .Space = 1UI}))
        p.AppendChild(pPr)
        Return p
    End Function

    ' ── Spacer paragraph ──────────────────────────────────────────────────────
    Private Shared Function SpacerParagraph(twips As Integer) As OXW.Paragraph
        Dim p As New OXW.Paragraph()
        Dim pPr As New OXW.ParagraphProperties()
        pPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = twips.ToString(), .After = "0"})
        p.AppendChild(pPr)
        Return p
    End Function

    ' ── Banner paragraph (placement) ──────────────────────────────────────────
    Private Shared Function BannerParagraph(text As String, color As String, fontSize As Integer) As OXW.Paragraph
        Dim p As New OXW.Paragraph()
        Dim pPr As New OXW.ParagraphProperties()
        pPr.AppendChild(New OXW.Justification() With {.Val = OXW.JustificationValues.Center})
        pPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "0"})
        p.AppendChild(pPr)
        Dim r As New OXW.Run()
        Dim rPr As New OXW.RunProperties()
        rPr.AppendChild(New OXW.Bold())
        rPr.AppendChild(New OXW.Color() With {.Val = color})
        rPr.AppendChild(New OXW.FontSize() With {.Val = (fontSize * 2).ToString()})
        rPr.AppendChild(New OXW.RunFonts() With {.Ascii = "Georgia", .HighAnsi = "Georgia"})
        r.AppendChild(rPr)
        r.AppendChild(New OXW.Text(text))
        p.AppendChild(r)
        Return p
    End Function

    ' ── Header table (logo left, title right) ─────────────────────────────────
    Private Shared Function BuildHeaderTable(
            mainPart As MainDocumentPart,
            logoPath As String,
            title As String,
            subtitle As String,
            gold As String,
            darkGray As String,
            contentW As Integer) As OXW.Table

        Dim tbl As New OXW.Table()
        Dim tblPr As New OXW.TableProperties()
        tblPr.AppendChild(New OXW.TableWidth() With {
            .Width = contentW.ToString(), .Type = OXW.TableWidthUnitValues.Dxa})
        tblPr.AppendChild(New OXW.TableBorders(
            New OXW.TopBorder() With {.Val = OXW.BorderValues.None},
            New OXW.BottomBorder() With {.Val = OXW.BorderValues.None},
            New OXW.LeftBorder() With {.Val = OXW.BorderValues.None},
            New OXW.RightBorder() With {.Val = OXW.BorderValues.None},
            New OXW.InsideHorizontalBorder() With {.Val = OXW.BorderValues.None},
            New OXW.InsideVerticalBorder() With {.Val = OXW.BorderValues.None}))
        tbl.AppendChild(tblPr)
        tbl.AppendChild(New OXW.TableGrid(
            New OXW.GridColumn() With {.Width = "1800"},
            New OXW.GridColumn() With {.Width = (contentW - 1800).ToString()}))

        Dim row As New OXW.TableRow()

        ' Left cell: logo
        Dim logoCell As New OXW.TableCell()
        Dim logoCellPr As New OXW.TableCellProperties()
        logoCellPr.AppendChild(New OXW.TableCellWidth() With {
            .Width = "1800", .Type = OXW.TableWidthUnitValues.Dxa})
        logoCellPr.AppendChild(New OXW.TableCellVerticalAlignment() With {
            .Val = OXW.TableVerticalAlignmentValues.Center})
        logoCell.AppendChild(logoCellPr)
        If logoPath IsNot Nothing Then
            logoCell.AppendChild(BuildLogoParagraph(mainPart, logoPath))
        Else
            logoCell.AppendChild(New OXW.Paragraph())
        End If

        ' Right cell: title text
        Dim titleCell As New OXW.TableCell()
        Dim titleCellPr As New OXW.TableCellProperties()
        titleCellPr.AppendChild(New OXW.TableCellWidth() With {
            .Width = (contentW - 1800).ToString(), .Type = OXW.TableWidthUnitValues.Dxa})
        titleCellPr.AppendChild(New OXW.TableCellVerticalAlignment() With {
            .Val = OXW.TableVerticalAlignmentValues.Center})
        titleCell.AppendChild(titleCellPr)

        ' Title
        Dim pTitle As New OXW.Paragraph()
        Dim pTitlePr As New OXW.ParagraphProperties()
        pTitlePr.AppendChild(New OXW.Justification() With {.Val = OXW.JustificationValues.Center})
        pTitlePr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "60"})
        pTitle.AppendChild(pTitlePr)
        Dim rTitle As New OXW.Run()
        Dim rTitlePr As New OXW.RunProperties()
        rTitlePr.AppendChild(New OXW.Bold())
        rTitlePr.AppendChild(New OXW.Color() With {.Val = gold})
        rTitlePr.AppendChild(New OXW.FontSize() With {.Val = "80"})
        rTitlePr.AppendChild(New OXW.RunFonts() With {.Ascii = "Georgia", .HighAnsi = "Georgia"})
        rTitle.AppendChild(rTitlePr)
        rTitle.AppendChild(New OXW.Text(title))
        pTitle.AppendChild(rTitle)
        titleCell.AppendChild(pTitle)

        ' Subtitle
        Dim pSub As New OXW.Paragraph()
        Dim pSubPr As New OXW.ParagraphProperties()
        pSubPr.AppendChild(New OXW.Justification() With {.Val = OXW.JustificationValues.Center})
        pSubPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "0"})
        pSub.AppendChild(pSubPr)
        Dim rSub As New OXW.Run()
        Dim rSubPr As New OXW.RunProperties()
        rSubPr.AppendChild(New OXW.Color() With {.Val = darkGray})
        rSubPr.AppendChild(New OXW.FontSize() With {.Val = "28"})
        rSubPr.AppendChild(New OXW.Italic())
        rSubPr.AppendChild(New OXW.RunFonts() With {.Ascii = "Georgia", .HighAnsi = "Georgia"})
        rSub.AppendChild(rSubPr)
        rSub.AppendChild(New OXW.Text(subtitle))
        pSub.AppendChild(rSub)
        titleCell.AppendChild(pSub)

        row.AppendChild(logoCell)
        row.AppendChild(titleCell)
        tbl.AppendChild(row)
        Return tbl
    End Function

    ' ── Logo image paragraph ──────────────────────────────────────────────────
    Private Shared Function BuildLogoParagraph(mainPart As MainDocumentPart, logoPath As String) As OXW.Paragraph
        Dim ext As String = Path.GetExtension(logoPath).ToLower()
        Dim contentType As String = If(ext = ".png", "image/png", "image/jpeg")
        Dim imgPart As ImagePart = mainPart.AddImagePart(contentType)
        Using fs As New FileStream(logoPath, FileMode.Open, FileAccess.Read)
            imgPart.FeedData(fs)
        End Using
        Dim relId As String = mainPart.GetIdOfPart(imgPart)
        Dim emuW As Long = 1200000L
        Dim emuH As Long = 1200000L

        Dim p As New OXW.Paragraph()
        Dim pPr As New OXW.ParagraphProperties()
        pPr.AppendChild(New OXW.Justification() With {.Val = OXW.JustificationValues.Center})
        pPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "0"})
        p.AppendChild(pPr)

        Dim r As New OXW.Run()
        r.AppendChild(New OXW.RunProperties())
        r.AppendChild(New OXW.Drawing(
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
                                    New A.AdjustValueList()) With {
                                    .Preset = A.ShapeTypeValues.Rectangle})
                        )
                    ) With {.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"}
                )
            ) With {.DistanceFromTop = 0UI, .DistanceFromBottom = 0UI,
                    .DistanceFromLeft = 0UI, .DistanceFromRight = 0UI}
        ))
        p.AppendChild(r)
        Return p
    End Function

    ' ── Info table (category, dog, handler, score, bonus, date) ──────────────
    Private Shared Function BuildInfoTable(
            contentW As Integer,
            gold As String,
            lightGold As String,
            darkGray As String,
            ParamArray pairs() As String) As OXW.Table

        Dim colLabel As Integer = CInt(contentW * 0.35)
        Dim colValue As Integer = contentW - colLabel

        Dim tbl As New OXW.Table()
        Dim tblPr As New OXW.TableProperties()
        tblPr.AppendChild(New OXW.TableWidth() With {
            .Width = contentW.ToString(), .Type = OXW.TableWidthUnitValues.Dxa})
        tblPr.AppendChild(New OXW.TableBorders(
            New OXW.TopBorder() With {.Val = OXW.BorderValues.None},
            New OXW.BottomBorder() With {.Val = OXW.BorderValues.None},
            New OXW.LeftBorder() With {.Val = OXW.BorderValues.None},
            New OXW.RightBorder() With {.Val = OXW.BorderValues.None},
            New OXW.InsideHorizontalBorder() With {.Val = OXW.BorderValues.Single, .Size = 4UI, .Color = gold},
            New OXW.InsideVerticalBorder() With {.Val = OXW.BorderValues.None}))
        tbl.AppendChild(tblPr)
        tbl.AppendChild(New OXW.TableGrid(
            New OXW.GridColumn() With {.Width = colLabel.ToString()},
            New OXW.GridColumn() With {.Width = colValue.ToString()}))

        Dim i As Integer = 0
        While i < pairs.Length - 1
            Dim lbl As String = pairs(i)
            Dim val As String = pairs(i + 1)
            Dim isAlt As Boolean = (i \ 2) Mod 2 = 1

            Dim row As New OXW.TableRow()

            ' Label cell
            Dim lblCell As New OXW.TableCell()
            Dim lblCellPr As New OXW.TableCellProperties()
            lblCellPr.AppendChild(New OXW.TableCellWidth() With {
                .Width = colLabel.ToString(), .Type = OXW.TableWidthUnitValues.Dxa})
            lblCellPr.AppendChild(New OXW.TableCellVerticalAlignment() With {
                .Val = OXW.TableVerticalAlignmentValues.Center})
            If isAlt Then
                lblCellPr.AppendChild(New OXW.Shading() With {
                    .Fill = lightGold, .Val = OXW.ShadingPatternValues.Clear})
            End If
            lblCellPr.AppendChild(New OXW.TableCellMargin() With {
                .TopMargin = New OXW.TopMargin() With {.Width = "80", .Type = OXW.TableWidthUnitValues.Dxa},
                .BottomMargin = New OXW.BottomMargin() With {.Width = "80", .Type = OXW.TableWidthUnitValues.Dxa},
                .LeftMargin = New OXW.LeftMargin() With {.Width = "160", .Type = OXW.TableWidthUnitValues.Dxa},
                .RightMargin = New OXW.RightMargin() With {.Width = "160", .Type = OXW.TableWidthUnitValues.Dxa}})
            lblCell.AppendChild(lblCellPr)
            Dim pLbl As New OXW.Paragraph()
            Dim pLblPr As New OXW.ParagraphProperties()
            pLblPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "0"})
            pLbl.AppendChild(pLblPr)
            Dim rLbl As New OXW.Run()
            Dim rLblPr As New OXW.RunProperties()
            rLblPr.AppendChild(New OXW.Bold())
            rLblPr.AppendChild(New OXW.Color() With {.Val = gold})
            rLblPr.AppendChild(New OXW.FontSize() With {.Val = "28"})
            rLblPr.AppendChild(New OXW.RunFonts() With {.Ascii = "Arial", .HighAnsi = "Arial"})
            rLbl.AppendChild(rLblPr)
            rLbl.AppendChild(New OXW.Text(lbl))
            pLbl.AppendChild(rLbl)
            lblCell.AppendChild(pLbl)

            ' Value cell
            Dim valCell As New OXW.TableCell()
            Dim valCellPr As New OXW.TableCellProperties()
            valCellPr.AppendChild(New OXW.TableCellWidth() With {
                .Width = colValue.ToString(), .Type = OXW.TableWidthUnitValues.Dxa})
            valCellPr.AppendChild(New OXW.TableCellVerticalAlignment() With {
                .Val = OXW.TableVerticalAlignmentValues.Center})
            If isAlt Then
                valCellPr.AppendChild(New OXW.Shading() With {
                    .Fill = lightGold, .Val = OXW.ShadingPatternValues.Clear})
            End If
            valCellPr.AppendChild(New OXW.TableCellMargin() With {
                .TopMargin = New OXW.TopMargin() With {.Width = "80", .Type = OXW.TableWidthUnitValues.Dxa},
                .BottomMargin = New OXW.BottomMargin() With {.Width = "80", .Type = OXW.TableWidthUnitValues.Dxa},
                .LeftMargin = New OXW.LeftMargin() With {.Width = "160", .Type = OXW.TableWidthUnitValues.Dxa},
                .RightMargin = New OXW.RightMargin() With {.Width = "160", .Type = OXW.TableWidthUnitValues.Dxa}})
            valCell.AppendChild(valCellPr)
            Dim pVal As New OXW.Paragraph()
            Dim pValPr As New OXW.ParagraphProperties()
            pValPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "0"})
            pVal.AppendChild(pValPr)
            Dim rVal As New OXW.Run()
            Dim rValPr As New OXW.RunProperties()
            rValPr.AppendChild(New OXW.Color() With {.Val = darkGray})
            rValPr.AppendChild(New OXW.FontSize() With {.Val = "28"})
            rValPr.AppendChild(New OXW.RunFonts() With {.Ascii = "Arial", .HighAnsi = "Arial"})
            rVal.AppendChild(rValPr)
            rVal.AppendChild(New OXW.Text(val))
            pVal.AppendChild(rVal)
            valCell.AppendChild(pVal)

            row.AppendChild(lblCell)
            row.AppendChild(valCell)
            tbl.AppendChild(row)
            i += 2
        End While

        Return tbl
    End Function

    ' ── Signature line ────────────────────────────────────────────────────────
    Private Shared Function SignatureParagraph(label As String, darkGray As String) As OXW.Paragraph
        Dim p As New OXW.Paragraph()
        Dim pPr As New OXW.ParagraphProperties()
        pPr.AppendChild(New OXW.Justification() With {.Val = OXW.JustificationValues.Center})
        pPr.AppendChild(New OXW.SpacingBetweenLines() With {.Before = "0", .After = "60"})
        pPr.AppendChild(New OXW.ParagraphBorders(
            New OXW.BottomBorder() With {
                .Val = OXW.BorderValues.Single,
                .Size = 6UI,
                .Color = darkGray,
                .Space = 1UI}))
        p.AppendChild(pPr)
        p.AppendChild(New OXW.Run(New OXW.RunProperties(
            New OXW.FontSize() With {.Val = "48"})))
        Return p
    End Function

End Class
