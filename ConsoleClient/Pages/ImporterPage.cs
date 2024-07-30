using System.Xml;
using Core.Models;
using Core.Services;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using LibVLCSharp.Shared;
using Spectre.Console;

namespace ConsoleClient.Pages;

public class ImporterPage : IPage
{
    readonly YoutubeStreamingService youtubeStreamingService;
    readonly YouTubeService youtubeApi;
    readonly AudioService audioService;
    readonly TagService tagService;
    public string Name => "Importer";

    public ImporterPage(YoutubeStreamingService youtubeStreamingService,
                        YouTubeService youtubeApi,
                        AudioService audioService,
                        TagService tagService)
    {
        this.youtubeStreamingService = youtubeStreamingService;
        this.youtubeApi = youtubeApi;
        this.audioService = audioService;
        this.tagService = tagService;
    }

    public async Task Show()
    {
        if (!AnsiConsole.Confirm("Selected playlist will be imported as tag.\r\nDo you want to continue?"))
            return;

        var tagTypeEnums = Enum.GetValues<TagType>();
        TagType? tagType = null;
        if (AnsiConsole.Confirm("Will all tag will be of the same type"))
            tagType = AnsiConsole.Prompt(new SelectionPrompt<TagType>()
                .Title("Which type of tag will playlists be imported as?")
                .AddChoices(tagTypeEnums)
            );

        var audioTypeEnums = Enum.GetValues<AudioType>();
        var audioType = AnsiConsole.Prompt(new SelectionPrompt<AudioType>()
            .Title("Which type of audio will the videos be imported as?")
            .AddChoices(audioTypeEnums)
        );

        var addAdditionalTags = AnsiConsole.Confirm("Do you want to add additional tags on playlist?", false);
        var willDownload = AnsiConsole.Confirm("Do you want to download the selected playlist?");
        var usePlaylistNameAsTag = AnsiConsole.Confirm("Do you want to use the playlist name as the tag name?", false);

        var playlists = (await youtubeStreamingService.GetMyPlaylists()).ToList();

        var selectedPlayLists = AnsiConsole.Prompt(
            new MultiSelectionPrompt<(string, string)>()
                .Title("Playlists")
                .PageSize(30)
                .MoreChoicesText("[grey](Move up and down to reveal more playlists)[/]")
                .AddChoices(playlists)
                .UseConverter(c => Markup.Escape(c.Item1))
        );

        foreach (var playList in selectedPlayLists)
        {
            var (name, id) = playList;
            var audios = (await youtubeStreamingService.GetPlaylistAudios(id)).ToList();

            var audioListRequest = youtubeApi.Videos.List("snippet,contentDetails");
            audioListRequest.Id = new Repeatable<string>(audios.Select(a => a.Id));
            audioListRequest.MaxResults = 10000;

            var audioInfosTask = audioListRequest.ExecuteAsync();

            var tags = await Tag(name, willDownload, id, addAdditionalTags, tagType, usePlaylistNameAsTag);

            await Audio(audioInfosTask, tags, audioType);
            
            AnsiConsole.MarkupLine("[green]Success![/]");
            Console.Read();
        }
    }
    async Task Audio(Task<VideoListResponse> audioInfosTask, List<Tag> tags, AudioType audioType)
    {
        List<AudioInfo> importedAudios = [];
        foreach (var audio in (await audioInfosTask).Items)
        {
            var existingAudio = await audioService.TryGet(audio.Id);
            if (existingAudio is not null)
            {
                existingAudio.Tags.UnionWith(tags);
                await audioService.Save();
                continue;
            }

            var audioInfo = new AudioInfo
            {
                Id = audio.Id,
                Type = audioType,
                Name = audio.Snippet.Title,
                Duration = XmlConvert.ToTimeSpan(audio.ContentDetails.Duration),
                Tags = [..tags]
            };

            importedAudios.Add(audioInfo);
        }

        await audioService.Add(importedAudios);
    }
    async Task<List<Tag>> Tag(string name, bool willDownload, string id, bool addAdditionalTags, TagType? tagType, bool usePlaylistNameAsTag)
    {
        AnsiConsole.MarkupLineInterpolated($"{name}");
        tagType ??= AnsiConsole.Ask<TagType>("Which type of tag will this playlist be imported as?");

        if (!usePlaylistNameAsTag)
            name = AnsiConsole.Ask<string>("What is the name of the tag?");
        
        List<Tag> tags =
        [
            new Tag
            {
                Name = name,
                Downloaded = willDownload,
                Type = tagType.Value,
                OriginalPlaylistId = id
            }
        ];

        if (addAdditionalTags && AnsiConsole.Confirm("Do you want to add tag for this playlist?", false))
            tags.Add(new Tag
            {
                Name = AnsiConsole.Ask<string>("Tag name"),
                Downloaded = willDownload,
                Type = tagType.Value,
                OriginalPlaylistId = id
            });

        foreach (var tag in tags)
        {
            var added = await tagService.Add(tag);

            if (!added)
            {
                AnsiConsole.MarkupLine("A tag with the same name already exists.");

                var existingTag = await tagService.TryGetNoTracking(tag.Name);

                if (tag.OriginalPlaylistId != existingTag?.OriginalPlaylistId)
                    AnsiConsole.MarkupLine($"Original playlist id mismatch New:{tag.OriginalPlaylistId} vs Old:{existingTag?.OriginalPlaylistId}");

                if (tag.Type != existingTag?.Type)
                    AnsiConsole.MarkupLine($"Tag type mismatch New:{tag.Type} vs Old:{existingTag?.Type}");

                var overrideOld = AnsiConsole.Confirm("Do you want to override the old tag?");

                if (overrideOld)
                    await tagService.Update(tag);
            }
        }
        return tags;
    }
}