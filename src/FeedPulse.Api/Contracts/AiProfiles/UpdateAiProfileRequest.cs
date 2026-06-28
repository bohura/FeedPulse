using FeedPulse.Api.Entities;

namespace FeedPulse.Api.Contracts.AiProfiles
{
    public class UpdateAiProfileRequest
    {
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
    }
}
