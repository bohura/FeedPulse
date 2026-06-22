namespace FeedPulse.Api.Contracts.Feeds
{
    public class FeedSyncResult
    {
        public int FeedId { get; set; }

        public string FeedTitle { get; set; } = string.Empty;

        public int FetchedCount { get; set; }

        public int AddedCount { get; set; }

        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
