namespace FeedPulse.Api.Entities
{
    public class FeedItem
    {
        public int Id { get; set; }

        public int FeedId { get; set; }

        public Guid ExternalId { get; set; }

        public string Title { get; set; }

        public string Link { get; set; }

        public string Summary { get; set; }

        public DateTimeOffset PublishedAt { get; set; }

        public DateTimeOffset CreateAt { get; set; }
    }
}
