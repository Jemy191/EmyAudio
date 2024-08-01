using EmyAudio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EmyAudio;

public class AppDbContext : DbContext
{
    public DbSet<AudioInfo> AudioInfos { get; init; }
    public DbSet<AudioSkip> AudioSkips { get; init; }
    public DbSet<Tag> Tags { get; init; }

    // Required to enable migration directly on the class lib proj
    public AppDbContext()
    {
        
    }
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql();
        }
    }

    public async Task Init()
    {
        if (!await Database.CanConnectAsync())
            throw new InvalidOperationException($"Cannot connect to PostgresSQL Server. Please check your appsettings.json.");

        await Database.GetInfrastructure().GetRequiredService<IMigrator>().MigrateAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AudioInfo>()
            .HasMany(a => a.Skips)
            .WithOne(s => s.AudioInfo)
            .HasForeignKey(s => s.AudioInfoId);

        modelBuilder.Entity<AudioInfo>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.AudioInfos)
            .UsingEntity<Dictionary<string, object>>(
                "AudioInfoToTagJoin",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagName"),
                j => j.HasOne<AudioInfo>().WithMany().HasForeignKey("AudioInfoId")
            );
    }
}