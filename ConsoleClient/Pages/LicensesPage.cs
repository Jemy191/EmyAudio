using Spectre.Console;

namespace ConsoleClient.Pages;

public class LicensesPage : IPage
{
    public string Name => "Licenses";
    public async Task Show()
    {
        var license =
            """
                This app use these open source libraries with divers license:
                https://www.svgrepo.com/svg/495926/audio-square
            """; 
        AnsiConsole.MarkupLine(Markup.Escape(license));

        Console.ReadKey();
    }
}