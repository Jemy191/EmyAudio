using ConsoleClient.Pages;
using Microsoft.Extensions.Hosting;

namespace ConsoleClient;

public class AppHost : IHostedService
{
    readonly PageNavigationService pageNavigationService;
    readonly IHostApplicationLifetime applicationLifetime;
    public AppHost(PageNavigationService pageNavigationService, IHostApplicationLifetime applicationLifetime)
    {
        this.pageNavigationService = pageNavigationService;
        this.applicationLifetime = applicationLifetime;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await pageNavigationService.Open<MenuPage>();
        applicationLifetime.StopApplication();
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}