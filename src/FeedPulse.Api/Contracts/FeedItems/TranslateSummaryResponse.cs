namespace FeedPulse.Api.Contracts.FeedItems
{
    public class TranslateSummaryResponse
    {
        public int FeedItemId {  get; set; }

        public string OriginalSummary { get; set; } = string.Empty;

        public string TranslatedSummary {  get; set; } = string.Empty;
    }
}
