using FeedPulse.Api.Contracts.Feeds;
using FeedPulse.Api.Data;
using FeedPulse.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace FeedPulse.Api.Services
{
    public class FeedSyncService : IFeedSyncService
    {
        private readonly AppDbContext appDbContext;

        public FeedSyncService(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public async Task<FeedSyncResult> SyncAsync(int feedId)
        {
            var feedsyncResult = new FeedSyncResult();
            feedsyncResult.Success = false;
            var feed = await appDbContext.Feeds.FindAsync(feedId);
            if (feed is null)
            {
                return feedsyncResult;
            }

            feedsyncResult.FeedId = feed.Id;
            feedsyncResult.FeedTitle = feed.Title;
            XNamespace contentNs = "http://purl.org/rss/1.0/modules/content/";
            using var client = new HttpClient();
            string xml;
            try
            {
                xml = await client.GetStringAsync(feed.Url);
            }
            catch (Exception ex)
            {
                feedsyncResult.ErrorMessage = $"Failed to download feed: {ex.Message}";
                return feedsyncResult;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(xml))
                {
                    feedsyncResult.ErrorMessage = "返回内容为空";
                    return feedsyncResult;
                }
                var document = XDocument.Parse(xml);
                var rootName = document.Root?.Name.LocalName;
                if (string.Equals(rootName, "rss", StringComparison.OrdinalIgnoreCase))
                {
                    var items = document.Root?
                        .Element("channel")?
                        .Elements("item")
                        .ToList() ?? new List<XElement>();

                    var insertedCount = 0;

                    foreach (var item in items)
                    {
                        var title = item.Element("title")?.Value?.Trim();
                        var link = item.Element("link")?.Value?.Trim();
                        var externalId = item.Element("guid")?.Value?.Trim() ?? link;

                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
                            continue;

                        var exists = await appDbContext.FeedItems.AnyAsync(x =>
                            x.FeedId == feed.Id && x.ExternalId == externalId
                        );

                        if (exists)
                            continue;

                        DateTimeOffset? publishedAt = null;
                        var pubDateText = item.Element("pubDate")?.Value;
                        if (DateTimeOffset.TryParse(pubDateText, out var parsedPubDate))
                        {
                            publishedAt = parsedPubDate.ToUniversalTime();
                        }

                        appDbContext.FeedItems.Add(new FeedItem
                        {
                            FeedId = feed.Id,
                            ExternalId = externalId,
                            Title = title,
                            Link = link,
                            Summary = item.Element(contentNs + "encoded")?.Value?.Trim()
                                ?? item.Element("description")?.Value?.Trim(),
                            PublishedAt = publishedAt,
                            CreatedAt = DateTimeOffset.UtcNow
                        });

                        insertedCount++;
                    }
                    try
                    {
                        await appDbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        feedsyncResult.ErrorMessage = $"Failed to save feed items: {ex.InnerException?.Message ?? ex.Message}";
                        return feedsyncResult;
                    }

                    feedsyncResult.FetchedCount = items.Count;
                    feedsyncResult.AddedCount = insertedCount;
                    feedsyncResult.Success = true;

                    return feedsyncResult;
                }
                if (string.Equals(rootName, "feed", StringComparison.OrdinalIgnoreCase))
                {
                    XNamespace atom = "http://www.w3.org/2005/Atom";

                    var insertedCount = 0;

                    var entries = document.Root?
                        .Elements(atom + "entry")
                        .ToList() ?? new List<XElement>();

                    foreach (var entry in entries)
                    {
                        var title = entry.Element(atom + "title")?.Value?.Trim();
                        var link = entry.Elements(atom + "link")
                            .Select(x => x.Attribute("href")?.Value?.Trim())
                            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
                        {
                            continue;
                        }
                        var externalId = entry.Element(atom + "id")?.Value?.Trim() ?? link;
                        var content = entry.Element(atom + "content")?.Value?.Trim();
                        var summary = content
                            ?? entry.Element(atom + "summary")?.Value?.Trim();

                        var exists = await appDbContext.FeedItems.AnyAsync(x =>
                            x.FeedId == feed.Id && x.ExternalId == externalId
                        );

                        if (exists)
                            continue;

                        DateTimeOffset? publishedAt = null;
                        var pubDateText = entry.Element("pubDate")?.Value
                            ?? entry.Element(atom + "updated")?.Value;
                        if (DateTimeOffset.TryParse(pubDateText, out var parsedPubDate))
                        {
                            publishedAt = parsedPubDate.ToUniversalTime();
                        }

                        appDbContext.FeedItems.Add(new FeedItem
                        {
                            FeedId = feed.Id,
                            ExternalId = externalId,
                            Title = title,
                            Link = link,
                            Summary = summary,
                            PublishedAt = publishedAt,
                            CreatedAt = DateTimeOffset.UtcNow
                        });

                        insertedCount++;
                    }
                    try
                    {
                        await appDbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        feedsyncResult.ErrorMessage = $"Failed to save feed items: {ex.InnerException?.Message ?? ex.Message}";
                        return feedsyncResult;
                    }

                    feedsyncResult.FetchedCount = entries.Count;
                    feedsyncResult.AddedCount = insertedCount;
                    feedsyncResult.Success = true;
                    return feedsyncResult;

                }
                feedsyncResult.ErrorMessage = "Unsupported feed format.";
                return feedsyncResult;

            }
            catch (Exception ex)
            {
                feedsyncResult.ErrorMessage = $"Failed to parse feed XML: {ex.Message}";
                return feedsyncResult;
            }
        }
    }
}
