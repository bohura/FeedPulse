namespace FeedPulse.Api.Contracts.FeedItems
{
    public class FeedItemContentResponse
    {
        public int FeedItemId { get; set; }

        public string Link { get; set; } = string.Empty;

        public string ContentSource { get; set; } = string.Empty;

        public bool HasCachedFullContent { get; set; }

        public DateTimeOffset? FetchedAt { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
