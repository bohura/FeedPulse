using System.ComponentModel.DataAnnotations;

namespace FeedPulse.Api.Contracts.Feeds;

public class UpdateFeedRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}