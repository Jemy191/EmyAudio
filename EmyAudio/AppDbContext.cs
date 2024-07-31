using EmyAudio.Models;
using EmyAudio.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmyAudio;

public class AppDbContext : DbContext
{
    readonly IConfiguration configuration;
    public DbSet<AudioInfo> AudioInfos { get; init; }
    public DbSet<AudioSkip> AudioSkips { get; init; }
    public DbSet<Tag> Tags { get; init; }

    protected AppDbContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = configuration["PostgresConnectionString"];
            optionsBuilder.UseNpgsql(connectionString);
        }
        
        Database.Migrate();
        
        if(!Database.CanConnect())
            throw new Exception($"Cannot connect to PostgresSQL Server. Please check your appsettings.json.");
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