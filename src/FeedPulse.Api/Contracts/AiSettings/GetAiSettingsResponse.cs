namespace FeedPulse.Api.Contracts.AiSettings
{
    public class GetAiSettingsResponse
    {
        public bool IsEnabled { get; set; }

        public bool HasApiKey { get; set; }

        public string Model { get; set; } = string.Empty;

        public bool EnableTitleTranslation { get; set; }

        public bool EnableSummaryTranslation { get; set; }

        public bool EnableSummaryGeneration { get; set; }
    }
}
