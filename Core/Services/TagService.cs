using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class TagService
{
    readonly AppDbContext dbContext;
    public TagService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> Add(Tag tag)
    {
        if (dbContext.Tags.Any(t => t.Name == tag.Name))
            return false;
        await dbContext.Tags.AddAsync(tag);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Tag?> TryGet(string name) =>
        await dbContext.Tags
            .SingleOrDefaultAsync(t => t.Name == name);
    
    public async Task<Tag?> TryGetNoTracking(string name) =>
        await dbContext.Tags
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Name == name);

    public async Task Update(Tag tag)
    {
        dbContext.Tags.Update(tag);
        await dbContext.SaveChangesAsync();
    }

    public async Task Hide(string name)
    {
        var tag = await TryGet(name);
        
        if (tag == null)
            return;
        
        tag.Hidden = true;
        await dbContext.SaveChangesAsync();
    }
}