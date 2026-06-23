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
        public FeedItemsController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeedItem>>> GetAll(int feedId)
        {
            var feedExists = await appDbContext.Feeds.AnyAsync(feed => feed.Id == feedId);
            if (!feedExists)
            {
                return NotFound();
            }
            var feedItem = await appDbContext.FeedItems
                .AsNoTracking()
                .Where(item => item.FeedId == feedId)
                .OrderByDescending(item => item.PublishedAt ?? item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            return Ok(feedItem);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FeedItem>> GetById(int feedId, int id)
        {
            var feeditem = await appDbContext.FeedItems
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.FeedId == feedId && item.Id == id);
            if (feeditem == null)
            {
                return NotFound();
            }
            return Ok(feeditem);
        }


    }
}
