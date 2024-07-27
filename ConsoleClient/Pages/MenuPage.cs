using Core.Services;
using Spectre.Console;

namespace ConsoleClient.Pages;

public class MenuPage : IPage
{
    readonly PageNavigationService pageNavigationService;
    readonly SettingsService settingsService;
    public string Name => "Menu";

    public MenuPage(PageNavigationService pageNavigationService,
                    SettingsService settingsService)
    {
        this.pageNavigationService = pageNavigationService;
        this.settingsService = settingsService;
    }

    public async Task Show()
    {
        if (!settingsService.Setting.FirstTime)
        {
            var @continue = AnsiConsole.Confirm("Continue last listening session?");
            if(@continue)
                await pageNavigationService.Open<PlayerPage>();
        }

        while (!pageNavigationService.ExitRequested)
        {
            settingsService.Setting.FirstTime = false;
            await settingsService.Save();

            List<(string name, Type page)> pages =
            [
                ("Player", typeof(PlayerPage)),
                ("Setting", typeof(SettingPage)),
                ("Importer", typeof(ImporterPage)),
                ("Playlist downloader", typeof(PlaylistDownloader)),
                ("License", typeof(LicensesPage))
            ];
            var choice = AnsiConsole.Prompt(new SelectionPrompt<(string name, Type page)>()
                .Title("Menu")
                .AddChoices(pages)
                .UseConverter(c => c.name));

            await pageNavigationService.Open(choice.page);
        }
    }
}