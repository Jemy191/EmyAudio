using EmyAudio.Models;
using Microsoft.EntityFrameworkCore;

namespace EmyAudio;

public class AppDbContext : DbContext
{
    public DbSet<AudioInfo> AudioInfos { get; init; }
    public DbSet<AudioSkip> AudioSkips { get; init; }
    public DbSet<Tag> Tags { get; init; }

    protected AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if(!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Dev_EmyAudioDatabase;Username=Admin;Password=8888z4");
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