using FeedPulse.Api.Contracts.Feeds;
using FeedPulse.Api.Data;
using FeedPulse.Api.Entities;
using FeedPulse.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Controllers;

[Route("feeds")]
[ApiController]
public class FeedsController : ControllerBase
{
    private readonly AppDbContext appDbContext;
    private readonly IFeedSyncService feedSync;
    public FeedsController(AppDbContext appContext, IFeedSyncService feedSync)
    {
        this.appDbContext = appContext;
        this.feedSync = feedSync;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Feed>>> GetAll()
    {
        var feeds = await appDbContext.Feeds
            .AsNoTracking()
            .OrderByDescending(feed=>feed.CreatedAt)
            .ThenByDescending(feed=>feed.Id)
            .ToListAsync();
        return Ok(feeds);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Feed>> GetById(int id)
    {
        var feed = await appDbContext.Feeds
            .AsNoTracking()
             .SingleOrDefaultAsync(x=>x.Id==id);
        if (feed is null)
        {
            return NotFound();
        }

        return Ok(feed);
    }

    [HttpPost]
    public async Task<ActionResult<Feed>> Create(CreateFeedRequest request)
    {
        var feed = new Feed
        {
            Title = request.Title,
            Url = request.Url,
            IsActive = true
        };

        appDbContext.Feeds.Add(feed);
        await appDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = feed.Id }, feed);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Feed>> Update(int id, UpdateFeedRequest request)
    {
        var feed = await appDbContext.Feeds.FindAsync(id);
        if (feed is null)
        {
            return NotFound();
        }

        feed.Title = request.Title;
        feed.Url = request.Url;
        feed.IsActive = request.IsActive;

        await appDbContext.SaveChangesAsync();

        return Ok(feed);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var feed = await appDbContext.Feeds.FindAsync(id);
        if (feed is null)
        {
            return NotFound();
        }

        appDbContext.Feeds.Remove(feed);
        await appDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/sync")]
    public async Task<IActionResult> Sync(int id)
    {
        var importedCount = await feedSync.SyncAsync(id);
        return Ok(importedCount);
    }
}
