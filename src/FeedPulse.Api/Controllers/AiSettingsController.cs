using FeedPulse.Api.Contracts.AiSettings;
using FeedPulse.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Controllers
{
    [Route("ai-settings")]
    [ApiController]
    public class AiSettingsController:ControllerBase
    {
        private readonly AppDbContext appDbContext;

        private const string DefaultTitleTranslationPrompt =
    "Translate the title into the target language. Return only the translated title without explanations or markdown.";

        private const string DefaultSummaryTranslationPrompt =
            "Translate the text into the target language. Return only the translated text without explanations or markdown.";

        private const string DefaultSummaryGenerationPrompt =
            "Summarize the content in the target language. Return only the summary without explanations or markdown.";

        public AiSettingsController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<GetAiSettingsResponse>> Get()
        {
            var settings = await appDbContext.AiSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if(settings is null)
            {
                return Ok(new GetAiSettingsResponse
                {
                    IsEnabled = false,
                    HasApiKey = false,
                    Model = string.Empty,
                    TargetLanguage = "Simplified Chinese",
                    EnableSummaryGeneration = false,
                    EnableSummaryTranslation = false,
                    EnableTitleTranslation = false,
                    TitleTranslationProfileId = null,
                    SummaryTranslationProfileId = null,
                    SummaryGenerationProfileId = null,
                    TitleTranslationPrompt = DefaultTitleTranslationPrompt,
                    SummaryTranslationPrompt = DefaultSummaryTranslationPrompt,
                    SummaryGenerationPrompt = DefaultSummaryGenerationPrompt
                });
            }

            return Ok(new GetAiSettingsResponse
            {
                IsEnabled = settings.IsEnabled,
                HasApiKey = !string.IsNullOrWhiteSpace(settings.ApiKey),
                Model = settings.Model,
                TargetLanguage = NormalizeTargetLanguage(settings.TargetLanguage),
                EnableTitleTranslation = settings.EnableTitleTranslation,
                EnableSummaryTranslation = settings.EnableSummaryTranslation,
                EnableSummaryGeneration = settings.EnableSummaryGeneration,
                TitleTranslationProfileId = settings.TitleTranslationProfileId,
                SummaryTranslationProfileId = settings.SummaryTranslationProfileId,
                SummaryGenerationProfileId = settings.SummaryGenerationProfileId,
                TitleTranslationPrompt = NormalizePrompt(settings.TitleTranslationPrompt, DefaultTitleTranslationPrompt),
                SummaryTranslationPrompt = NormalizePrompt(settings.SummaryTranslationPrompt, DefaultSummaryTranslationPrompt),
                SummaryGenerationPrompt = NormalizePrompt(settings.SummaryGenerationPrompt, DefaultSummaryGenerationPrompt)
            });
        }
        [HttpPut]
        public async Task<ActionResult<GetAiSettingsResponse>> Update(UpdateAiSettingsRequest request)
        {
            var settings = await appDbContext.AiSettings
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (!await AiProfileExistsAsync(request.TitleTranslationProfileId))
            {
                return BadRequest("Title translation profile does not exist.");
            }

            if (!await AiProfileExistsAsync(request.SummaryTranslationProfileId))
            {
                return BadRequest("Summary translation profile does not exist.");
            }

            if (!await AiProfileExistsAsync(request.SummaryGenerationProfileId))
            {
                return BadRequest("Summary generation profile does not exist.");
            }

            if (settings is null)
            {
                settings = new FeedPulse.Api.Entities.AiSettings();
                appDbContext.AiSettings.Add(settings);
            }

            settings.IsEnabled = request.IsEnabled;
            settings.ApiKey = request.ApiKey;
            settings.Model = request.Model;
            settings.TargetLanguage = NormalizeTargetLanguage(request.TargetLanguage);
            settings.EnableTitleTranslation = request.EnableTitleTranslation;
            settings.EnableSummaryTranslation = request.EnableSummaryTranslation;
            settings.EnableSummaryGeneration = request.EnableSummaryGeneration;
            settings.TitleTranslationProfileId = request.TitleTranslationProfileId;
            settings.SummaryTranslationProfileId = request.SummaryTranslationProfileId;
            settings.SummaryGenerationProfileId = request.SummaryGenerationProfileId;
            settings.TitleTranslationPrompt = NormalizePrompt(request.TitleTranslationPrompt, DefaultTitleTranslationPrompt);
            settings.SummaryTranslationPrompt = NormalizePrompt(request.SummaryTranslationPrompt, DefaultSummaryTranslationPrompt);
            settings.SummaryGenerationPrompt = NormalizePrompt(request.SummaryGenerationPrompt, DefaultSummaryGenerationPrompt);

            await appDbContext.SaveChangesAsync();

            return Ok(new GetAiSettingsResponse
            {
                IsEnabled = settings.IsEnabled,
                HasApiKey = !string.IsNullOrWhiteSpace(settings.ApiKey),
                Model = settings.Model,
                TargetLanguage = NormalizeTargetLanguage(settings.TargetLanguage),
                EnableTitleTranslation = settings.EnableTitleTranslation,
                EnableSummaryTranslation = settings.EnableSummaryTranslation,
                EnableSummaryGeneration = settings.EnableSummaryGeneration,
                TitleTranslationProfileId = settings.TitleTranslationProfileId,
                SummaryTranslationProfileId = settings.SummaryTranslationProfileId,
                SummaryGenerationProfileId = settings.SummaryGenerationProfileId,
                TitleTranslationPrompt = settings.TitleTranslationPrompt,
                SummaryTranslationPrompt = settings.SummaryTranslationPrompt,
                SummaryGenerationPrompt = settings.SummaryGenerationPrompt
            });
        }

        private static string NormalizePrompt(string prompt, string defaultPrompt)
        {
            return string.IsNullOrWhiteSpace(prompt)
                ? defaultPrompt
                : prompt.Trim();
        }

        private Task<bool> AiProfileExistsAsync(int? profileId)
        {
            if (!profileId.HasValue)
            {
                return Task.FromResult(true);
            }

            return appDbContext.AiProfiles.AnyAsync(profile => profile.Id == profileId.Value);
        }

        private static string NormalizeTargetLanguage(string? targetLanguage)
        {
            return string.IsNullOrWhiteSpace(targetLanguage)
                ? "Simplified Chinese"
                : targetLanguage.Trim();
        }
    }
}
