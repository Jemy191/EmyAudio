using LibVLCSharp.Shared;

namespace Core.Services;

public class VlcService : IDisposable
{
    readonly string logFilePath = Path.Combine("Log", "Vlc.log");
    
    readonly LibVLC libVlc;
    public readonly MediaPlayer mediaPlayer;
    public VlcService()
    {
        LibVLCSharp.Shared.Core.Initialize();
        libVlc = new LibVLC();

        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        File.WriteAllText(logFilePath, "Empty log");
        libVlc.SetLogFile(logFilePath);
        
        mediaPlayer = new MediaPlayer(libVlc);
        
    }

    public async ValueTask Play(Stream stream)
    {
        var streamMediaInput = new StreamMediaInput(stream);
        var media = new Media(libVlc, streamMediaInput);
        await media.Parse(MediaParseOptions.ParseNetwork);

        mediaPlayer.Play(media);
    }
    public void Dispose()
    {
        libVlc.CloseLogFile();
    }
}
