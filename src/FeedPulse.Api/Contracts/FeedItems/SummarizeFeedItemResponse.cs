namespace FeedPulse.Api.Contracts.FeedItems
{
    public class SummarizeFeedItemResponse
    {
        public int FeedItemId { get; set; }

        public string OriginalSummary { get; set; } = string.Empty;

        public string AiSummary { get; set; } = string.Empty;
    }
}
