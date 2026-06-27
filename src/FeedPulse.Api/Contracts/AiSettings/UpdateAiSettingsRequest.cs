namespace FeedPulse.Api.Contracts.AiSettings
{
    public class UpdateAiSettingsRequest
    {
        public bool IsEnabled { get; set; }

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public bool EnableTitleTranslation { get; set; }

        public bool EnableSummaryTranslation { get; set; }

        public bool EnableSummaryGeneration { get; set; }
    }
}
