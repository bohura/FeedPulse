namespace FeedPulse.Api.Entities
{
    public class FeedItem
    {
        public int Id { get; set; }

        public int FeedId { get; set; }

        public Feed Feed { get; set; } = null!;

        public string? ExternalId { get; set; }

        public required string Title { get; set; }

        public required string Link { get; set; }

        public string? Summary { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
