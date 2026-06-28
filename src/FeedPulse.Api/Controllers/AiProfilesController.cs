using FeedPulse.Api.Contracts.AiProfiles;
using FeedPulse.Api.Data;
using FeedPulse.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Controllers
{
    [Route("ai-profiles")]
    [ApiController]
    public class AiProfilesController : ControllerBase
    {
        private readonly AppDbContext appDbContext;

        public AiProfilesController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAiProfileResponse>>> GetAll()
        {
            var profiles = await appDbContext.AiProfiles
                .AsNoTracking()
                .OrderByDescending(profile => profile.IsDefault)
                .ThenBy(profile => profile.Name)
                .ToListAsync();

            return Ok(profiles.Select(MapToResponse));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetAiProfileResponse>> GetById(int id)
        {
            var profile = await appDbContext.AiProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.Id == id);

            if (profile is null)
            {
                return NotFound();
            }

            return Ok(MapToResponse(profile));
        }

        [HttpPost]
        public async Task<ActionResult<GetAiProfileResponse>> Create(CreateAiProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Profile name is required.");
            }

            var normalizedName = request.Name.Trim();

            var nameExists = await appDbContext.AiProfiles
                .AnyAsync(profile => profile.Name == normalizedName);

            if (nameExists)
            {
                return BadRequest("Profile name already exists.");
            }

            if (request.IsDefault)
            {
                var defaultProfiles = await appDbContext.AiProfiles
                    .Where(profile => profile.IsDefault)
                    .ToListAsync();

                foreach (var item in defaultProfiles)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            var now = DateTimeOffset.UtcNow;

            var profile = new AiProfile
            {
                Name = normalizedName,
                Vendor = string.IsNullOrWhiteSpace(request.Vendor)
                    ? request.Provider.ToString()
                    : request.Vendor.Trim(),
                Provider = request.Provider,
                BaseUrl = request.BaseUrl.Trim(),
                ApiKey = request.ApiKey.Trim(),
                Model = request.Model.Trim(),
                TargetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
                    ? "Simplified Chinese"
                    : request.TargetLanguage.Trim(),
                IsEnabled = request.IsEnabled,
                IsDefault = request.IsDefault,
                EnableTitleTranslation = request.EnableTitleTranslation,
                EnableSummaryTranslation = request.EnableSummaryTranslation,
                EnableSummaryGeneration = request.EnableSummaryGeneration,
                CreatedAt = now,
                UpdatedAt = now
            };

            appDbContext.AiProfiles.Add(profile);
            await appDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = profile.Id }, MapToResponse(profile));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<GetAiProfileResponse>> Update(int id, UpdateAiProfileRequest request)
        {
            var profile = await appDbContext.AiProfiles
                .SingleOrDefaultAsync(item => item.Id == id);

            if (profile is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Profile name is required.");
            }

            var normalizedName = request.Name.Trim();

            var nameExists = await appDbContext.AiProfiles
                .AnyAsync(item => item.Id != id && item.Name == normalizedName);

            if (nameExists)
            {
                return BadRequest("Profile name already exists.");
            }

            if (request.IsDefault)
            {
                var defaultProfiles = await appDbContext.AiProfiles
                    .Where(item => item.Id != id && item.IsDefault)
                    .ToListAsync();

                foreach (var item in defaultProfiles)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            profile.Name = normalizedName;
            profile.Vendor = string.IsNullOrWhiteSpace(request.Vendor)
                ? request.Provider.ToString()
                : request.Vendor.Trim();
            profile.Provider = request.Provider;
            profile.BaseUrl = request.BaseUrl.Trim();
            profile.ApiKey = request.ApiKey.Trim();
            profile.Model = request.Model.Trim();
            profile.TargetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
                ? "Simplified Chinese"
                : request.TargetLanguage.Trim();
            profile.IsEnabled = request.IsEnabled;
            profile.IsDefault = request.IsDefault;
            profile.EnableTitleTranslation = request.EnableTitleTranslation;
            profile.EnableSummaryTranslation = request.EnableSummaryTranslation;
            profile.EnableSummaryGeneration = request.EnableSummaryGeneration;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            await appDbContext.SaveChangesAsync();

            return Ok(MapToResponse(profile));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var profile = await appDbContext.AiProfiles
                .SingleOrDefaultAsync(item => item.Id == id);

            if (profile is null)
            {
                return NotFound();
            }

            if (profile.IsDefault)
            {
                return BadRequest("Default profile cannot be deleted. Set another default profile first.");
            }

            appDbContext.AiProfiles.Remove(profile);
            await appDbContext.SaveChangesAsync();

            return NoContent();
        }

        private static GetAiProfileResponse MapToResponse(AiProfile profile)
        {
            return new GetAiProfileResponse
            {
                Id = profile.Id,
                Name = profile.Name,
                Vendor = profile.Vendor,
                Provider = profile.Provider,
                BaseUrl = profile.BaseUrl,
                HasApiKey = !string.IsNullOrWhiteSpace(profile.ApiKey),
                Model = profile.Model,
                TargetLanguage = profile.TargetLanguage,
                IsEnabled = profile.IsEnabled,
                IsDefault = profile.IsDefault,
                EnableTitleTranslation = profile.EnableTitleTranslation,
                EnableSummaryTranslation = profile.EnableSummaryTranslation,
                EnableSummaryGeneration = profile.EnableSummaryGeneration,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}