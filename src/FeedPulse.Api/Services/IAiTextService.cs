namespace FeedPulse.Api.Services
{
    public interface IAiTextService
    {
        Task<string> GenerateTextAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default);
    }
}