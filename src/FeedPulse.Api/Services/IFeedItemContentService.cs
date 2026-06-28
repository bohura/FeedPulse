using FeedPulse.Api.Entities;

namespace FeedPulse.Api.Services
{
    public interface IFeedItemContentService
    {
        Task<FeedItemContentResult> GetBestContentAsync(
            FeedItem feedItem,
            CancellationToken cancellationToken = default);

        Task<FeedItemContentResult> FetchAndCacheFullContentAsync(
            FeedItem feedItem,
            CancellationToken cancellationToken = default);
    }
}
