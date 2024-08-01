using ConsoleClient;
using EmyAudio;
using EmyAudio.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

try
{
    AnsiConsole.Clear();
    AnsiConsole.Write(
        new FigletText("EmyMusic")
            .Centered()
            .Color(Color.Pink1));

    var cred = await GoogleAuthService.Connect();

    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.AddUserSecrets(typeof(AppDbContext).Assembly);

    builder.UseEmyAudio(cred);

    builder.Services
        .AddHostedService<AppHost>()
        .AddSingleton<PageNavigationService>();
    
    using var app = builder.Build();

    await app.InitEmyAudio();
    await app.RunAsync();
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.Read();
}