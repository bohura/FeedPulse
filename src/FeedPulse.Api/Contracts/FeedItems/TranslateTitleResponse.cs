namespace FeedPulse.Api.Contracts.FeedItems
{
    public class TranslateTitleResponse
    {
        public int FeedItemId { get; set; }

        public string OriginalTitle { get; set; } = string.Empty;

        public string TranslatedTitle { get; set; }= string.Empty;
    }
}
