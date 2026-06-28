namespace FeedPulse.Api.Services
{
    public class FeedItemContentResult
    {
        public int FeedItemId { get; init; }

        public string Link { get; init; } = string.Empty;

        public string Content { get; init; } = string.Empty;

        public string ContentSource { get; init; } = string.Empty;

        public bool HasCachedFullContent { get; init; }

        public DateTimeOffset? FetchedAt { get; init; }
    }
}
