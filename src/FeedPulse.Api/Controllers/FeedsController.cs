using FeedPulse.Api.Contracts.Feeds;
using FeedPulse.Api.Data;
using FeedPulse.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedPulse.Api.Controllers;

[Route("feeds")]
[ApiController]
public class FeedsController : ControllerBase
{
    private readonly AppDbContext appDbContext;

    public FeedsController(AppDbContext appContext)
    {
        this.appDbContext = appContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Feed>>> GetAll()
    {
        var feeds = await appDbContext.Feeds.ToListAsync();
        return Ok(feeds);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Feed>> GetById(int id)
    {
        var feed = await appDbContext.Feeds.FindAsync(id);
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
}