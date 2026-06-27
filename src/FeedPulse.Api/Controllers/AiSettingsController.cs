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
                    IsEnabled=false,
                    HasApiKey=false,
                    Model=string.Empty,
                    EnableSummaryGeneration=false,
                    EnableSummaryTranslation=false,
                    EnableTitleTranslation=false,
                });
            }

            return Ok(new GetAiSettingsResponse
            {
                IsEnabled = settings.IsEnabled,
                HasApiKey = !string.IsNullOrWhiteSpace(settings.ApiKey),
                Model = settings.Model,
                EnableTitleTranslation = settings.EnableTitleTranslation,
                EnableSummaryTranslation = settings.EnableSummaryTranslation,
                EnableSummaryGeneration = settings.EnableSummaryGeneration
            });
        }
        [HttpPut]
        public async Task<ActionResult<GetAiSettingsResponse>> Update(UpdateAiSettingsRequest request)
        {
            var settings = await appDbContext.AiSettings
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if(settings is null)
            {
                settings = new FeedPulse.Api.Entities.AiSettings();
                appDbContext.AiSettings.Add(settings);
            }

            settings.IsEnabled = request.IsEnabled;
            settings.ApiKey = request.ApiKey;
            settings.Model = request.Model;
            settings.EnableTitleTranslation = request.EnableTitleTranslation;
            settings.EnableSummaryTranslation = request.EnableSummaryTranslation;
            settings.EnableSummaryGeneration = request.EnableSummaryGeneration;

            await appDbContext.SaveChangesAsync();

            return Ok(new GetAiSettingsResponse
            {
                IsEnabled = settings.IsEnabled,
                HasApiKey = !string.IsNullOrWhiteSpace(settings.ApiKey),
                Model = settings.Model,
                EnableTitleTranslation = settings.EnableTitleTranslation,
                EnableSummaryTranslation = settings.EnableSummaryTranslation,
                EnableSummaryGeneration = settings.EnableSummaryGeneration
            });
        }
    }
}
