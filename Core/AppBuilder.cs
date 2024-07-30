using Core.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;

namespace Core;

public class AppBuilder(UserCredential cred, string postgresConnectionString) : IAsyncDisposable
{
    readonly ServiceCollection services = [];
    
    ServiceProvider? serviceProvider;
    AsyncServiceScope? scope;
    
    public IServiceProvider Build()
    {
        var youtubeApi = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = cred,
            ApplicationName = "Emy audio"
        });
        
        services
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(postgresConnectionString))
            .AddSingleton(youtubeApi)
            .AddTransient<YoutubeStreamingService>()
            .AddTransient<YoutubeClient>()
            .AddTransient<PlayerService>()
            .AddTransient<GoogleAuthService>()
            .AddTransient<DownloadedAudioService>()
            .AddTransient<AudioService>()
            .AddTransient<TagService>()
            .AddSingleton<SettingsService>()
            .AddSingleton<VlcService>();
        
        serviceProvider = services.BuildServiceProvider(true);

        scope = serviceProvider.CreateAsyncScope();
        var context = scope.Value.ServiceProvider.GetRequiredService<AppDbContext>();
        
        context.Database.Migrate();
            
        if(!context.Database.CanConnect())
            throw new ApplicationException("Database do net exist");
        
        return serviceProvider;
    }

    public AppBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        configureServices.Invoke(services);
        return this;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (serviceProvider is not null)
            await serviceProvider.DisposeAsync();
        if (scope is not null)
            await scope.Value.DisposeAsync();
    }
}