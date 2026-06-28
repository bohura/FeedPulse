namespace FeedPulse.Api.Entities
{
    public class AiProfile
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Vendor { get; set; } = string.Empty;

        public AiProvider Provider { get; set; } = AiProvider.OpenAiCompatible;

        public string BaseUrl { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string TargetLanguage { get; set; } = "Simplified Chinese";

        public bool IsEnabled { get; set; }

        public bool IsDefault { get; set; }

        public bool EnableTitleTranslation { get; set; }

        public bool EnableSummaryTranslation { get; set; }

        public bool EnableSummaryGeneration { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
