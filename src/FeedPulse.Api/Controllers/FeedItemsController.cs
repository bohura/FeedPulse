using FeedPulse.Api.Contracts.FeedItems;
using FeedPulse.Api.Data;
using FeedPulse.Api.Entities;
using FeedPulse.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Controllers
{
    [Route("feeds/{feedId:int}/items")]
    [ApiController]
    public class FeedItemsController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly IFeedItemContentService feedItemContentService;
        private readonly IAiTextService aiTextService;

        public FeedItemsController(
            AppDbContext appDbContext,
            IFeedItemContentService feedItemContentService,
            IAiTextService aiTextService)
        {
            this.appDbContext = appDbContext;
            this.feedItemContentService = feedItemContentService;
            this.aiTextService = aiTextService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(int feedId,int page=1,int pageSize=20)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 20;
            }else if (pageSize > 100)
            {
                pageSize = 100;
            }
            var feedExists = await appDbContext.Feeds.AnyAsync(feed => feed.Id == feedId);
            if (!feedExists)
            {
                return NotFound();
            }
            var query = appDbContext.FeedItems
                .AsNoTracking()
                .Where(item => item.FeedId == feedId);

            var totalCount = await query.CountAsync();

            var feeditems = await query
                .OrderByDescending(item => item.PublishedAt ?? item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new {
                page,
                pageSize,
                totalCount,
                items = feeditems
            });
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FeedItem>> GetById(int feedId, int id)
        {
            var feeditem = await FindFeedItemAsync(feedId, id);
            if (feeditem == null)
            {
                return NotFound();
            }

            
            return Ok(feeditem);
        }

        [HttpPost("{id:int}/translate-title")]
        public async Task<ActionResult<TranslateTitleResponse>> TranslateTitle(int feedId, int id)
        {
            var feedItem = await FindFeedItemAsync(feedId, id);

            if (feedItem is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(feedItem.Title))
            {
                return BadRequest("Feed item title is empty.");
            }

            var settings = await GetAiSettingsAsync();

            var validationResult = ValidateAiSettings(
                settings,
                settings?.EnableTitleTranslation ?? false,
                "Title translation is disabled.");

            if (validationResult is not null)
            {
                return validationResult;
            }

            var aiSettings = settings!;

            if (!aiSettings.TitleTranslationProfileId.HasValue)
            {
                return BadRequest("Title translation profile is not configured.");
            }

            var profile = await FindAiProfileAsync(aiSettings.TitleTranslationProfileId.Value);

            if (profile is null)
            {
                return BadRequest("Title translation profile does not exist.");
            }

            if (!profile.IsEnabled)
            {
                return BadRequest("Title translation profile is disabled.");
            }

            try
            {
                var translatedTitle = await aiTextService.GenerateTextAsync(
                    new AiTextRequest
                    {
                        Profile = profile,
                        InputText = feedItem.Title,
                        Prompt = aiSettings.TitleTranslationPrompt,
                        TargetLanguage = aiSettings.TargetLanguage
                    });

                return Ok(new TranslateTitleResponse
                {
                    FeedItemId = feedItem.Id,
                    OriginalTitle = feedItem.Title,
                    TranslatedTitle = translatedTitle
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:int}/translate-summary")]
        public async Task<ActionResult<TranslateSummaryResponse>> TranslateSummary(int feedId, int id)
        {
            var feedItem = await FindFeedItemAsync(feedId, id);

            if (feedItem is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(feedItem.Summary))
            {
                return BadRequest("Feed item summary is empty.");
            }

            var settings = await GetAiSettingsAsync();

            var validationResult = ValidateAiSettings(
                settings,
                settings?.EnableSummaryTranslation ?? false,
                "Summary translation is disabled.");

            if (validationResult is not null)
            {
                return validationResult;
            }

            return Ok(new TranslateSummaryResponse
            {
                FeedItemId = feedItem.Id,
                OriginalSummary = feedItem.Summary,
                TranslatedSummary = "[placeholder] " + feedItem.Summary
            });
        }

        [HttpPost("{id:int}/summarize")]
        public async Task<ActionResult<SummarizeFeedItemResponse>> Summarize(int feedId, int id)
        {
            var feedItem = await FindFeedItemAsync(feedId, id);

            if (feedItem is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(feedItem.Summary))
            {
                return BadRequest("Feed item summary is empty.");
            }

            var settings = await GetAiSettingsAsync();

            var validationResult = ValidateAiSettings(
                settings,
                settings?.EnableSummaryGeneration ?? false,
                "Summary generation is disabled.");

            if (validationResult is not null)
            {
                return validationResult;
            }

            return Ok(new SummarizeFeedItemResponse
            {
                FeedItemId = feedItem.Id,
                OriginalSummary = feedItem.Summary,
                AiSummary = "[placeholder] " + feedItem.Summary
            });
        }

        private Task<FeedItem?> FindFeedItemAsync(int feedId, int id)
        {
            return appDbContext.FeedItems
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.FeedId == feedId && item.Id == id);
        }

        private Task<AiSettings?> GetAiSettingsAsync()
        {
            return appDbContext.AiSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
        }

        private ActionResult? ValidateAiSettings(
            AiSettings? settings,
            bool featureEnabled,
            string featureDisabledMessage)
        {
            if (settings is null || !settings.IsEnabled)
            {
                return BadRequest("AI is disabled.");
            }

            if (!featureEnabled)
            {
                return BadRequest(featureDisabledMessage);
            }

            return null;
        }

        [HttpGet("{id:int}/content")]
        public async Task<ActionResult<FeedItemContentResponse>> GetContent(int feedId, int id)
        {
            var feedItem = await FindFeedItemAsync(feedId, id);

            if (feedItem is null)
            {
                return NotFound();
            }

            var result = await feedItemContentService.GetBestContentAsync(feedItem);

            return Ok(new FeedItemContentResponse
            {
                FeedItemId = result.FeedItemId,
                Link = result.Link,
                Content = result.Content,
                ContentSource = result.ContentSource,
                HasCachedFullContent = result.HasCachedFullContent,
                FetchedAt = result.FetchedAt
            });
        }

        [HttpPost("{id:int}/fetch-full-content")]
        public async Task<ActionResult<FeedItemContentResponse>> FetchFullContent(int feedId, int id)
        {
            var feedItem = await FindFeedItemAsync(feedId, id);

            if (feedItem is null)
            {
                return NotFound();
            }

            var result = await feedItemContentService.FetchAndCacheFullContentAsync(feedItem);

            return Ok(new FeedItemContentResponse
            {
                FeedItemId = result.FeedItemId,
                Link = result.Link,
                Content = result.Content,
                ContentSource = result.ContentSource,
                HasCachedFullContent = result.HasCachedFullContent,
                FetchedAt = result.FetchedAt
            });
        }

        private Task<AiProfile?> FindAiProfileAsync(int id)
        {
            return appDbContext.AiProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.Id == id);
        }
    }
}
