namespace FeedPulse.Api.Entities
{
    public class Feed
    {
         public int Id { get; set; }

        public required string Title { get; set; }

        public required string Url { get; set; }

        public DateTimeOffset CreatedAt { get; set; } =  DateTimeOffset.UtcNow;

        public bool IsActive {  get; set; }

        public ICollection<FeedItem> FeedItems { get; set; } = new List<FeedItem>();

    }
}
