Public NotInheritable Class SplashScreen1

    'TODO: Tento formulář lze jednoduše zvolit jako úvodní obrazovku aplikace na kartě "Aplikace"
    '  Návrháře projektu ("Vlastnosti" v nabídce "Projekt").


    Private Sub SplashScreen1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Za běhu programu nastaví text dialogu podle informací o sestavení aplikace.  

        'TODO: Upravte informace o sestavení aplikace v projektovém podokně "Aplikace" 
        '  vlastnostech projektu (v nabídce "Projekt").

        ''Název aplikace
        'If My.Application.Info.Title <> "" Then
        '    ApplicationTitle.Text = My.Application.Info.Title
        'Else
        '    'Pokud chybí název aplikace, použije se jméno aplikace bez přípony
        '    ApplicationTitle.Text = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        'End If

        'Zformátuje informace o verzi použitím textu zapsaného do Správce verzí při návrhu jako
        '  formátovací řetězec. Toto dovoluje efektivní lokalizaci.
        '  Informace o sestavení a revizi mohou být zahrnuty použitím následujícího kódu a změnou 
        '  doby návrhu ve Správě verzí na "Version {0}.{1:00}.{2}.{3}" nebo něco podobného.  Další informace
        '  naleznete v Nápovědě pod heslem String.Format().
        '
        '    Version.Text = System.String.Format(Version.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, My.Application.Info.Version.Revision)

        'Version.Text = System.String.Format(Version.Text, My.Application.Info.Version.Major, My.Application.Info.Version.Minor)

        ''Informace o autorských právech
        'Copyright.Text = My.Application.Info.Copyright
    End Sub

End Class
