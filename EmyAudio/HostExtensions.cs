using EmyAudio.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YoutubeExplode;

namespace EmyAudio;

public static class HostExtensions
{
    public static IHostApplicationBuilder UseEmyAudio(this IHostApplicationBuilder builder, UserCredential cred)
    {
        var youtubeApi = new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = cred,
            ApplicationName = "Emy audio"
        });
        
        builder.Services
            .AddDbContext<AppDbContext>()
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

        return builder;
    }

    public static async Task<IHost> InitEmyAudio(this IHost host)
    {
        await host.Services.GetService<SettingsService>()!.Load();
        return host;
    }
}