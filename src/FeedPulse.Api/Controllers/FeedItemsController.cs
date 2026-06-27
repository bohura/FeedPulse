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
