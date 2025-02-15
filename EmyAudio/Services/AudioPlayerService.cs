using Silk.NET.OpenAL;

namespace EmyAudio.Services;

public unsafe class AudioPlayerService : IDisposable
{
    readonly ALContext alc;
    readonly Device* device;
    readonly Context* context;
    readonly AL al;
    
    public int Position { get; set; }
    public int Volume { get; set; }
    
    public AudioPlayerService()
    {
        // Create an OpenAL context
        alc = ALContext.GetApi();
        device = alc.OpenDevice("Emy audio player");
        context = alc.CreateContext(device, null);
        alc.MakeContextCurrent(context);

        al = AL.GetApi();
    }


    public void Play(Stream audioStream, int frequency)
    {
        var buffer = al.GenBuffer();
        var memoryStream = new MemoryStream();
        audioStream.CopyTo(memoryStream);
        al.BufferData(buffer, BufferFormat.Stereo16, memoryStream.ToArray(), );
    }
    
    public void TogglePause()
    {
    }
    
    public void Reset()
    {
    }
    
    public void Stop()
    {
    }
    
    public void Dispose()
    {
        alc.DestroyContext(context);
        alc.CloseDevice(device);
    }
}