using ConsoleClient;
using ConsoleClient.Pages;
using Core;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

try
{
    AnsiConsole.Clear();
    AnsiConsole.Write(
        new FigletText("EmyMusic")
            .Centered()
            .Color(Color.Pink1));

    var cred = await GoogleAuthService.Connect();
    
    var settingService = new SettingsService();
    await settingService.Load();

    var postgresConnectionString = settingService.Setting.PostgresConnectionString;
    
    if(string.IsNullOrWhiteSpace(postgresConnectionString))
    {
        postgresConnectionString = AnsiConsole.Ask<string>("Please provide you postgres connection string");
        settingService.Setting.PostgresConnectionString = postgresConnectionString;
        await settingService.Save();
    }

    await using var builder = new AppBuilder(cred, postgresConnectionString)
        .ConfigureServices(service => service
            .AddSingleton<PageNavigationService>());

    var serviceProvider = builder.Build();

    await serviceProvider.GetService<SettingsService>()!.Load();

    await serviceProvider.GetService<PageNavigationService>()!.Open<MenuPage>();
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.Read();
}