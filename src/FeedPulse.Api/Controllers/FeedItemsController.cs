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
        private readonly IFeedSyncService feedSync;
        public FeedItemsController(AppDbContext appDbContext, IFeedSyncService feedSync)
        {
            this.appDbContext = appDbContext;
            this.feedSync = feedSync;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeedItem>>> GetAll(int feedId)
        {
            var feddExists = await appDbContext.Feeds.AnyAsync(feed => feed.Id == feedId);
            if (!feddExists)
            {
                return NotFound();
            }
            var feedItems = await appDbContext.FeedItems
                .AsNoTracking()
                .Where(item => item.FeedId == feedId)
                .OrderByDescending(item => item.PublishedAt ?? item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

            return Ok(feedItems);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FeedItem>> GetById(int feedId, int id)
        {
            var feeditems = await appDbContext.FeedItems
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.FeedId == feedId && item.Id == id);
            if (feeditems == null)
            {
                return NotFound();
            }
            return Ok(feeditems);
        }


    }
}
