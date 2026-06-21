using FeedPulse.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> 
            options) : base(options)
        {

        }
        public DbSet<Feed> Feeds =>Set<Feed>();
        public DbSet<FeedItem> FeedItems => Set<FeedItem>();
    }
}
