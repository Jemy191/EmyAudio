using ConsoleClient;
using ConsoleClient.Pages;
using Core.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using YoutubeExplode;

try
{
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
        .AddTransient<PlayerService>()
        .AddTransient<GoogleAuthService>()
        .AddTransient<DownloadedAudioService>()
        .AddSingleton<PageNavigationService>()
        .AddSingleton<SettingsService>()
        .AddSingleton<VlcService>();

    await using var serviceProvider = services.BuildServiceProvider(true);

    await serviceProvider.GetService<SettingsService>()!.Load();

    await serviceProvider.GetService<PageNavigationService>()!.Open<MenuPage>();
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.Read();
}