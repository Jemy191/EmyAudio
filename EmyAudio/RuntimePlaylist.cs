using System.Collections.ObjectModel;
using EmyAudio.Models;

namespace EmyAudio;

public class RuntimePlaylist
{
    readonly AudioInfo[] audioInfos;

    public required string Name { get; init; }
    public int CurrentIndex { get; private set; }
    public IEnumerable<AudioInfo> AudioInfos => audioInfos.AsReadOnly();

    public RuntimePlaylist(AudioInfo[] audioInfos)
    {
        this.audioInfos = audioInfos;
    }
    
    public AudioInfo? Current
    {
        get
        {
            if (CurrentIndex < 0 || CurrentIndex >= audioInfos.Length)
                return null;

            return audioInfos[CurrentIndex];
        }
    }
    public int Count => audioInfos.Length;

    public void SetIndex(int newIndex)
    {
        CurrentIndex = newIndex;
    }
    
    public void Next(bool loop)
    {
        CurrentIndex++;
        
        if(CurrentIndex >= audioInfos.Length)
            CurrentIndex = loop ? 0 : audioInfos.Length-1;
    }

    public void Previous()
    {
        CurrentIndex--;
        
        if (CurrentIndex <= 0)
            CurrentIndex = 0;
    }

    public void Reset()
    {
        CurrentIndex = 0;
    }
}