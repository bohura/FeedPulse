using FeedPulse.Api.Entities;

namespace FeedPulse.Api.Contracts.AiProfiles
{
    public class GetAiProfileResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Vendor { get; set; } = string.Empty;

        public AiProvider Provider { get; set; }

        public string BaseUrl { get; set; } = string.Empty;

        public bool HasApiKey { get; set; }

        public string Model { get; set; } = string.Empty;

        public string TargetLanguage { get; set; } = string.Empty;

        public bool IsEnabled { get; set; }

        public bool IsDefault { get; set; }

        public bool EnableTitleTranslation { get; set; }

        public bool EnableSummaryTranslation { get; set; }

        public bool EnableSummaryGeneration { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
