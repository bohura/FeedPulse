using System.ComponentModel.DataAnnotations;

namespace FeedPulse.Api.Contracts.Feeds;

public class CreateFeedRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
}