namespace FeedPulse.Api.Entities
{
    public class FeedItemContent
    {
        public int Id { get; set; }

        public int FeedItemId { get; set; }

        public FeedItem FeedItem { get; set; } = null!;

        public string Content { get; set; } = string.Empty;

        public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
