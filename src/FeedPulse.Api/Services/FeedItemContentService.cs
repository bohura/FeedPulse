using FeedPulse.Api.Data;
using FeedPulse.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;

namespace FeedPulse.Api.Services
{
    public class FeedItemContentService : IFeedItemContentService
    {
        private readonly AppDbContext appDbContext;
        private readonly HttpClient httpClient;

        private const int MinFullContentLength = 200;
        private const int MaxStoredContentLength = 40000;

        private static readonly Regex ScriptRegex = new(
            "<(script|style|noscript|iframe|svg)[^>]*?>.*?</\\1>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex ArticleRegex = new(
            "<article\\b[^>]*>(.*?)</article>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex MainRegex = new(
            "<main\\b[^>]*>(.*?)</main>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex BodyRegex = new(
            "<body\\b[^>]*>(.*?)</body>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex BreakRegex = new(
            "<br\\s*/?>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ClosingBlockRegex = new(
            "</(p|div|section|article|main|header|footer|aside|li|ul|ol|h1|h2|h3|h4|h5|h6|tr|table|blockquote)>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex OpeningListItemRegex = new(
            "<li\\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TagRegex = new(
            "<[^>]+>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex WhitespaceRegex = new(
            "[ \\t\\f\\v]+",
            RegexOptions.Compiled);

        private static readonly Regex EmptyLineRegex = new(
            "\\n\\s*\\n+",
            RegexOptions.Compiled);

        public FeedItemContentService(HttpClient httpClient, AppDbContext appDbContext)
        {
            this.httpClient = httpClient;
            this.appDbContext = appDbContext;
        }

        public async Task<FeedItemContentResult> GetBestContentAsync(
            FeedItem feedItem,
            CancellationToken cancellationToken = default)
        {
            var cachedContent = await appDbContext.FeedItemContents
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    content => content.FeedItemId == feedItem.Id,
                    cancellationToken);

            return BuildBestAvailableResult(feedItem, cachedContent);
        }

        public async Task<FeedItemContentResult> FetchAndCacheFullContentAsync(
            FeedItem feedItem,
            CancellationToken cancellationToken = default)
        {
            var cachedContent = await appDbContext.FeedItemContents
                .SingleOrDefaultAsync(
                    content => content.FeedItemId == feedItem.Id,
                    cancellationToken);

            var extractedContent = await TryFetchFullContentAsync(feedItem.Link, cancellationToken);

            if (string.IsNullOrWhiteSpace(extractedContent))
            {
                return BuildBestAvailableResult(feedItem, cachedContent);
            }

            var normalizedContent = LimitLength(extractedContent, MaxStoredContentLength);

            if (normalizedContent.Length < MinFullContentLength)
            {
                return BuildBestAvailableResult(feedItem, cachedContent);
            }

            var now = DateTimeOffset.UtcNow;

            if (cachedContent is null)
            {
                cachedContent = new FeedItemContent
                {
                    FeedItemId = feedItem.Id,
                    CreatedAt = now
                };

                appDbContext.FeedItemContents.Add(cachedContent);
            }

            cachedContent.Content = normalizedContent;
            cachedContent.FetchedAt = now;
            cachedContent.UpdatedAt = now;

            await appDbContext.SaveChangesAsync(cancellationToken);

            return new FeedItemContentResult
            {
                FeedItemId = feedItem.Id,
                Link = feedItem.Link,
                Content = cachedContent.Content,
                ContentSource = "fetched-full-content",
                HasCachedFullContent = true,
                FetchedAt = cachedContent.FetchedAt
            };
        }

        private static FeedItemContentResult BuildBestAvailableResult(
            FeedItem feedItem,
            FeedItemContent? cachedContent)
        {
            if (cachedContent is not null && !string.IsNullOrWhiteSpace(cachedContent.Content))
            {
                return new FeedItemContentResult
                {
                    FeedItemId = feedItem.Id,
                    Link = feedItem.Link,
                    Content = cachedContent.Content,
                    ContentSource = "cached-full-content",
                    HasCachedFullContent = true,
                    FetchedAt = cachedContent.FetchedAt
                };
            }

            if (!string.IsNullOrWhiteSpace(feedItem.Summary))
            {
                return new FeedItemContentResult
                {
                    FeedItemId = feedItem.Id,
                    Link = feedItem.Link,
                    Content = feedItem.Summary,
                    ContentSource = "feed-summary",
                    HasCachedFullContent = false,
                    FetchedAt = null
                };
            }

            if (!string.IsNullOrWhiteSpace(feedItem.Title))
            {
                return new FeedItemContentResult
                {
                    FeedItemId = feedItem.Id,
                    Link = feedItem.Link,
                    Content = feedItem.Title,
                    ContentSource = "feed-title",
                    HasCachedFullContent = false,
                    FetchedAt = null
                };
            }



            return new FeedItemContentResult
            {
                FeedItemId = feedItem.Id,
                Link = feedItem.Link,
                Content = string.Empty,
                ContentSource = "unavailable",
                HasCachedFullContent = false,
                FetchedAt = null
            };
        }

        private async Task<string?> TryFetchFullContentAsync(
            string link,
            CancellationToken cancellationToken)
        {
            if (!Uri.TryCreate(link, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,text/plain;q=0.8,*/*;q=0.7");

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(rawContent))
            {
                return null;
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType;

            if (string.Equals(mediaType, "text/plain", StringComparison.OrdinalIgnoreCase))
            {
                return NormalizeText(rawContent);
            }

            return ExtractMainText(rawContent);
        }

        private static string LimitLength(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }

            return text[..maxLength].Trim();
        }

        private static string NormalizeText(string text)
        {
            var normalized = text
                .Replace("\r", string.Empty)
                .Replace('\u00A0', ' ')
                .Trim();

            normalized = WhitespaceRegex.Replace(normalized, " ");
            normalized = Regex.Replace(normalized, " *\\n *", "\n");
            normalized = EmptyLineRegex.Replace(normalized, "\n\n");

            return normalized.Trim();
        }

        private static string? ExtractMainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var sanitizedHtml = ScriptRegex.Replace(html, " ");

            var candidates = new List<string>();
            candidates.AddRange(GetCandidateBlocks(ArticleRegex, sanitizedHtml));
            candidates.AddRange(GetCandidateBlocks(MainRegex, sanitizedHtml));
            candidates.AddRange(GetCandidateBlocks(BodyRegex, sanitizedHtml));

            if (candidates.Count == 0)
            {
                candidates.Add(sanitizedHtml);
            }

            var bestText = candidates
                .Select(HtmlToText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .OrderByDescending(text => text.Length)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(bestText)
                ? null
                : LimitLength(bestText, MaxStoredContentLength);
        }

        private static IEnumerable<string> GetCandidateBlocks(Regex regex, string html)
        {
            foreach (Match match in regex.Matches(html))
            {
                if (match.Groups.Count > 1)
                {
                    yield return match.Groups[1].Value;
                }
            }
        }

        private static string HtmlToText(string html)
        {
            var text = BreakRegex.Replace(html, "\n");
            text = ClosingBlockRegex.Replace(text, "\n");
            text = OpeningListItemRegex.Replace(text, "- ");
            text = TagRegex.Replace(text, " ");
            text = WebUtility.HtmlDecode(text);

            return NormalizeText(text);
        }
    }
}