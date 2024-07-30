using Core.Models;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode.Videos;

namespace Core.Services;

public class AudioService
{
    readonly AppDbContext dbContext;
    public AudioService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Add(AudioInfo audioInfo)
    {
        await dbContext.AudioInfos.AddAsync(audioInfo);
        await dbContext.SaveChangesAsync();
    }

    public async Task Add(List<AudioInfo> audios)
    {
        await dbContext.AudioInfos.AddRangeAsync(audios);
        await dbContext.SaveChangesAsync();
    }

    public async Task<AudioInfo?> TryGet(VideoId audioId) =>
        await dbContext.AudioInfos
            .SingleOrDefaultAsync(a => a.Id == audioId.Value);
    
    public async Task<AudioInfo?> TryGetNoTracking(VideoId audioId) =>
        await dbContext.AudioInfos
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == audioId.Value);

    public async Task Update(AudioInfo audioInfo)
    {
        dbContext.AudioInfos.Update(audioInfo);
        await dbContext.SaveChangesAsync();
    }
    public async Task Save() => await dbContext.SaveChangesAsync();
}