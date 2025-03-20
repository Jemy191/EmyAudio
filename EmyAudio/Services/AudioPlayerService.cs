using EmyAudio.AudioPlayer;
using JetBrains.Annotations;
using Silk.NET.OpenAL;

namespace EmyAudio.Services;

public unsafe class AudioPlayerService : IDisposable
{
    readonly ALContext alc;
    readonly Device* device;
    readonly Context* context;
    readonly AL al;
    readonly uint source;

    public readonly PlaybackMonitor PlaybackMonitor;
    
    [PublicAPI]
    public float Position
    {
        get
        {
            al.GetSourceProperty(source, SourceFloat.SecOffset, out var value);
            return value;
        }
        set => al.SetSourceProperty(source, SourceFloat.SecOffset, value);
    }


    int volume = 100;
    [PublicAPI]
    public int Volume
    {
        get => volume;
        set
        {
            volume = Math.Clamp(value, 0, 100);
            al.SetSourceProperty(source, SourceFloat.Gain, volume / 100.0f);
        }
    }
    
    bool isLooping;
    [PublicAPI]
    public bool IsLooping
    {
        get => isLooping;
        set
        {
            isLooping = value;
            al.SetSourceProperty(source, SourceBoolean.Looping, value);
        }
    }

    [PublicAPI]
    public bool IsPlaying { get; private set; }
    
    uint? currentAudioBuffer;

    [PublicAPI]
    public AudioPlayerService()
    {
        // Create an OpenAL context
        alc = ALContext.GetApi();
        device = alc.OpenDevice("Emy audio player");
        context = alc.CreateContext(device, null);
        alc.MakeContextCurrent(context);

        al = AL.GetApi();

        source = al.GenSource();
        al.SetSourceProperty(source, SourceFloat.Pitch, 1.0f);
        al.SetSourceProperty(source, SourceFloat.Gain, 1.0f);
        al.SetSourceProperty(source, SourceBoolean.Looping, false);

        PlaybackMonitor = new PlaybackMonitor(al, source);
        PlaybackMonitor.OnAudioEnd += () => IsPlaying = false;
    }
    
    [PublicAPI]
    public void Play(Stream audioStream, int frequency)
    {
        Stop();
        
        if(currentAudioBuffer is not null)
            al.DeleteBuffer(currentAudioBuffer.Value);
        
        currentAudioBuffer = al.GenBuffer();
        var memoryStream = new MemoryStream();
        audioStream.CopyTo(memoryStream);
        al.BufferData(currentAudioBuffer.Value, BufferFormat.Stereo16, memoryStream.ToArray(), frequency);

        al.SetSourceProperty(source, SourceInteger.Buffer, currentAudioBuffer.Value);
        
        Resume();

        PlaybackMonitor.Start();
    }

    [PublicAPI]
    public void Pause()
    {
        if (!IsPlaying)
            return;

        IsPlaying = false;
        al.SourcePause(source);
        PlaybackMonitor.Stop();
    }

    [PublicAPI]
    public void Resume()
    {
        if (IsPlaying)
            return;

        IsPlaying = true;
        al.SourcePlay(source);
        PlaybackMonitor.Start();
    }

    [PublicAPI]
    public void Reset()
    {
        al.SourceRewind(source);
        al.SourcePlay(source);
        PlaybackMonitor.Start();
    }

    [PublicAPI]
    public void Stop()
    {
        al.SourceStop(source);
        PlaybackMonitor.Stop();
    }

    public void Dispose()
    {
        Stop();

        if (currentAudioBuffer is not null)
            al.DeleteBuffer(currentAudioBuffer.Value);
        
        PlaybackMonitor.Dispose();
        al.DeleteSource(source);
        alc.DestroyContext(context);
        alc.CloseDevice(device);
    }
}