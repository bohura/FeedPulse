using FeedPulse.Api.Contracts.Feeds;

namespace FeedPulse.Api.Services
{
    public interface IFeedSyncService
    {
        Task<FeedSyncResult> SyncAsync(int feedId);
    }
}
