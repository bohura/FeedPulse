using FeedPulse.Api.Entities;

namespace FeedPulse.Api.Services
{
    public class AiTextRequest
    {
        public AiProfile Profile { get; set; } = null!;

        public string InputText { get; set; } = string.Empty;

        public string Prompt { get; set; } = string.Empty;

        public string TargetLanguage { get; set; } = "Simplified Chinese";
    }
}