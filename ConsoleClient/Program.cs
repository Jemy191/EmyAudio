using ConsoleClient;
using ConsoleClient.Pages;
using Core.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using YoutubeExplode;

AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("EmyMusic")
        .Centered()
        .Color(Color.Pink1));

var cred = await GoogleAuthService.Connect();
var youtubeApi = new YouTubeService(new BaseClientService.Initializer
{
    HttpClientInitializer = cred,
    ApplicationName = "Emy audio"
});
    
var services = new ServiceCollection()
    .AddSingleton(youtubeApi)
    .AddTransient<YoutubeStreamingService>()
    .AddTransient<YoutubeClient>()
    .AddTransient<GoogleAuthService>()
    .AddSingleton<PageNavigationService>()
    .AddSingleton<SettingsService>()
    .AddSingleton<VlcService>();

var serviceProvider = services.BuildServiceProvider(true);

var settingService = serviceProvider.GetService<SettingsService>()!;

await settingService.Load();

var pageNavigationService = serviceProvider.GetService<PageNavigationService>()!;

if (!settingService.Setting.FirstTime)
{
    await pageNavigationService.Open<PlayerPage>();
    return;
}

settingService.Setting.FirstTime = false;
await settingService.Save();

await pageNavigationService.Open<MenuPage>();

