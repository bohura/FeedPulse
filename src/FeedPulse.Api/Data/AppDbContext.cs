using FeedPulse.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>
            options) : base(options)
        {

        }
        public DbSet<Feed> Feeds => Set<Feed>();
        public DbSet<FeedItem> FeedItems => Set<FeedItem>();
        public DbSet<AiSettings> AiSettings => Set<AiSettings>();
        public DbSet<FeedItemContent> FeedItemContents => Set<FeedItemContent>();
        public DbSet<AiProfile> AiProfiles => Set<AiProfile>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FeedItem>()
                .HasIndex(item => new { item.FeedId, item.ExternalId })
                .IsUnique();

            modelBuilder.Entity<FeedItem>()
                .HasOne(item => item.FullContent)
                .WithOne(content => content.FeedItem)
                .HasForeignKey<FeedItemContent>(content => content.FeedItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeedItemContent>()
                .HasIndex(content => content.FeedItemId)
                .IsUnique();

            modelBuilder.Entity<AiProfile>()
                .HasIndex(profile => profile.Name)
                .IsUnique();

            modelBuilder.Entity<AiProfile>()
                .Property(profile => profile.Provider)
                .HasConversion<string>();
        }
    }
}
