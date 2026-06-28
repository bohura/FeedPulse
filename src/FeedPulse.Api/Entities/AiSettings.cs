namespace FeedPulse.Api.Entities
{
    public class AiSettings
    {
        public int Id { get; set; }

        public bool IsEnabled {  get; set; }

        public string ApiKey { get; set; } = string.Empty;

        public string Model {  get; set; }= string.Empty;

        public bool EnableTitleTranslation {  get; set; }

        public bool EnableSummaryTranslation {  get; set; }

        public bool EnableSummaryGeneration {  get; set; }

        public string TargetLanguage { get; set; } = string.Empty;

        public int? TitleTranslationProfileId { get; set; }

        public int? SummaryTranslationProfileId { get; set; }

        public int? SummaryGenerationProfileId { get; set; }

        public string TitleTranslationPrompt { get; set; } = string.Empty;

        public string SummaryTranslationPrompt { get; set; } = string.Empty;

        public string SummaryGenerationPrompt { get; set; } = string.Empty;
    }
}
