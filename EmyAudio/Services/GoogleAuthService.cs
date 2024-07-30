using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

namespace EmyAudio.Services;

public class GoogleAuthService
{
    public static async Task<UserCredential> Connect()
    {
        const string credentialsPath = @"YoutubeOAuthCred.json";

        await using var credStream = File.OpenRead(credentialsPath);
        
        string[] scopes = [YouTubeService.Scope.YoutubeReadonly];
        
        var secret = await GoogleClientSecrets.FromStreamAsync(credStream);
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secret.Secrets,
            scopes,
            "user",
            CancellationToken.None, new FileDataStore("YoutubeAuth.Store"));
        
        return credential;
    }
}