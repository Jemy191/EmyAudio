using Spectre.Console;

namespace ConsoleClient.Pages;

public class MenuPage : IPage
{
    readonly PageNavigationService pageNavigationService;
    public string Name => "Menu";

    public MenuPage(PageNavigationService pageNavigationService)
    {
        this.pageNavigationService = pageNavigationService;
    }

    public async Task Show()
    {
        List<(string name, Type page)> pages =
        [
            ("Player", typeof(PlayerPage)),
            ("Setting", typeof(SettingPage)),
            ("Importer", typeof(ImporterPage))
        ];
        var choice = AnsiConsole.Prompt(new SelectionPrompt<(string name, Type page)>()
            .Title("Menu")
            .AddChoices(pages)
            .UseConverter(c => c.name));

        await pageNavigationService.Open(choice.page);
    }
}