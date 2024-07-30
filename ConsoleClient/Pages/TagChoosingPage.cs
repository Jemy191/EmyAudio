using EmyAudio;
using EmyAudio.Models;
using EmyAudio.Services;
using Spectre.Console;

namespace ConsoleClient.Pages;

public class TagChoosingPage : IPage<RuntimePlaylist>
{
    public string Name => "Tag choosing";

    readonly YoutubeStreamingService youtube;
    public TagChoosingPage(YoutubeStreamingService youtube)
    {
        this.youtube = youtube;
    }
    
    public async Task<RuntimePlaylist> ShowAsync()
    {
        var playlists = (await youtube.GetMyPlaylists()).ToList();

        var selectedPlayList = AnsiConsole.Prompt(
            new SelectionPrompt<(string, string)>()
                .Title("Playlists")
                .PageSize(30)
                .MoreChoicesText("[grey](Move up and down to reveal more playlists)[/]")
                .AddChoices(playlists)
                .UseConverter(c => Markup.Escape(c.Item1))
                .EnableSearch()
                .SearchPlaceholderText("Search...")
        );

        var audios = await youtube.GetPlaylistAudios(selectedPlayList.Item2);
        
        var shuffle = AnsiConsole.Confirm("Shuffle playlist");

        var playlist = audios.Select(a => new AudioInfo
        {
            Id = a.Item2,
            Name = a.Item1,
            Type = AudioType.Music,
            Duration = TimeSpan.FromMinutes(1),
            Score = 0
        }).ToArray();
        
        if(shuffle)
            Random.Shared.Shuffle(playlist);
            
        return new RuntimePlaylist(playlist)
        {
            Name = selectedPlayList.Item1
        };
    }
}