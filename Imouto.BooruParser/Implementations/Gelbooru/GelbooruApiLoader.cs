using System.Globalization;
using System.Text.RegularExpressions;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;
using Imouto.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Gelbooru;

public class GelbooruApiLoader : IBooruApiLoader
{
    private static readonly Regex DateTimeRegex = new(
        ".*(?<month>\\w{3}).*(?<date>\\d{2}).*(?<hours>\\d{2})\\:(?<minutes>\\d{2})\\:(?<seconds>\\d{2}).*(?<tzhours>[+\\-]\\d{2})(?<tzminutes>\\d{2}).*(?<year>\\d{4})", RegexOptions.Compiled);
    
    private const string BaseUrl = "https://gelbooru.com/";
    private readonly IFlurlClient _flurlClient;

    public GelbooruApiLoader(IFlurlClientCache factory, IOptions<GelbooruSettings> options)
        => _flurlClient = factory
            .GetForDomain(new Url(BaseUrl))
            .BeforeCall(_ => DelayWithThrottler(options));

    public async Task<Post> GetPostAsync(string postId)
    {
        // https://gelbooru.com/index.php?page=post&s=view&id=
        var postHtml = await _flurlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "post",
                s = "view",
                id = postId
            })
            .GetHtmlDocumentAsync();
        
        // https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=1&id=
        var postJson = await _flurlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, id = postId
            })
            .GetJsonAsync<GelbooruPostPage>();

        var post = postJson.Posts?.FirstOrDefault();
        
        return post != null 
            ? CreatePost(post, postHtml) 
            : CreatePost(postHtml)!;
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        // https://gelbooru.com/index.php?page=post&s=list&md5=
        var postHtml = await _flurlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "post",
                s = "list",
                md5 = md5
            })
            .WithAutoRedirect(true)
            .GetHtmlDocumentAsync();
        
        // https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=1&md5=
        var postJson = await _flurlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, tags = $"md5:{md5}"
            })
            .GetJsonAsync<GelbooruPostPage>();
        
        var post = postJson.Posts?.FirstOrDefault();
        
        return post != null
            ? CreatePost(post, postHtml)
            : CreatePost(postHtml);
    }

    public async Task<SearchResult> SearchAsync(string tags)
    {
        // https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=20&tags=1girl
        var postJson = await _flurlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 20, tags = tags
            })
            .GetJsonAsync<GelbooruPostPage>();

        return new SearchResult(postJson.Posts?
            .Select(x => new PostPreview(x.Id.ToString(), x.Md5, x.Tags, false, false))
            .ToArray() ?? Array.Empty<PostPreview>());
    }

    public Task<SearchResult> GetPopularPostsAsync(PopularType type)
        => throw new NotSupportedException("Gelbooru does not support popularity charts");

    public Task<HistorySearchResult<TagHistoryEntry>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
        => throw new NotSupportedException("Gelbooru does not support history");

    public Task<HistorySearchResult<NoteHistoryEntry>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
        => throw new NotSupportedException("Gelbooru does not support history");

    /// <remarks>
    /// Parent is always 0.
    /// </remarks>
    private static PostIdentity? GetParent(GelbooruPost post)
        => post.ParentId != 0 ? new PostIdentity(post.ParentId.ToString(), string.Empty) : null;

    /// <remarks>
    /// Haven't found any post with them
    /// </remarks>
    private static IReadOnlyCollection<PostIdentity> GetChildren() => Array.Empty<PostIdentity>();

    private static IReadOnlyCollection<Note> GetNotes(GelbooruPost? post, HtmlDocument postHtml)
    {
        if (post?.HasNotes == "false")
            return Array.Empty<Note>();

        var notes = postHtml.DocumentNode
            .SelectNodes(@"//*[@id='notes']/article")
            ?.Select(note =>
            {
                var height = note.Attributes["data-height"].Value;
                var width = note.Attributes["data-width"].Value;
                var top = note.Attributes["data-y"].Value;
                var left = note.Attributes["data-x"].Value;

                var size = new Size(GetSizeInt(width), GetSizeInt(height));
                var point = new Position(GetPositionInt(top), GetPositionInt(left));

                var id = Convert.ToInt32(note.Attributes["data-id"].Value);
                var text = note.InnerText;

                return new Note(id.ToString(), text, point, size);
            }) ?? Enumerable.Empty<Note>();

        return notes.ToList();
        
        static int GetSizeInt(string number) => (int)(Convert.ToDouble(number) + 0.5);
        
        static int GetPositionInt(string number) => (int)Math.Ceiling(Convert.ToDouble(number) - 0.5);
    }

    private static Rating GetRating(string postRating) => GetRatingFromChar(postRating).Item1;

    private static RatingSafeLevel GetRatingSafeLevel(string postRating) => GetRatingFromChar(postRating).Item2;

    private static (Rating, RatingSafeLevel) GetRatingFromChar(string rating)
        => rating[0] switch
        {
            'q' => (Rating.Questionable, RatingSafeLevel.None),
            's' => (Rating.Safe, RatingSafeLevel.Sensitive),
            'g' => (Rating.Safe, RatingSafeLevel.General),
            'e' => (Rating.Explicit, RatingSafeLevel.None),
            _ => (Rating.Questionable, RatingSafeLevel.None)
        };

    private static IReadOnlyCollection<Tag> GetTags(HtmlDocument post) 
        => post.DocumentNode
            .SelectSingleNode(@"//*[@id='tag-list']")
            .SelectNodes(@"li")
            .Where(x => x.Attributes["class"]?.Value.StartsWith("tag-type-") == true)
            .Select(x =>
            {
                var type = x.Attributes["class"].Value.Split('-').Last();
                var name = x.SelectSingleNode(@"a").InnerHtml;

                return new Tag(type, name);
            })
            .ToList();

    /// <summary>
    /// Auth isn't supported right now.
    /// </summary>
    private static async Task DelayWithThrottler(IOptions<GelbooruSettings> options)
    {
        var delay = options.Value.PauseBetweenRequests;
        if (delay > TimeSpan.Zero)
            await Throttler.Get("gelbooru").UseAsync(delay);
    }

    private static DateTimeOffset ExtractDate(GelbooruPost post)
    {
        var datetime = DateTimeRegex.Match(post.CreatedAt);
        
        var year = int.Parse(datetime.Groups["year"].Value);
        var monthString = datetime.Groups["month"].Value;
        var date = int.Parse(datetime.Groups["date"].Value);
        var hours = int.Parse(datetime.Groups["hours"].Value);
        var minutes = int.Parse(datetime.Groups["minutes"].Value);
        var seconds = int.Parse(datetime.Groups["seconds"].Value);
        var tzHours = int.Parse(datetime.Groups["tzhours"].Value);
        var tzMinutes = int.Parse(datetime.Groups["tzminutes"].Value);
        var month = DateTime.Parse($"01 {monthString} 2020").Month;
        
        return new DateTimeOffset(year, month, date, hours, minutes, seconds,
            new TimeSpan(tzHours, tzMinutes, 0));
    }

    private static Post CreatePost(GelbooruPost post, HtmlDocument postHtml) 
        => new(
            new PostIdentity(post.Id.ToString(), post.Md5),
            post.FileUrl,
            !string.IsNullOrWhiteSpace(post.SampleUrl) ? post.SampleUrl : post.FileUrl,
            post.PreviewUrl,
            ExistState.Exist,
            ExtractDate(post),
            new Uploader(post.CreatorId.ToString(), post.Owner.Replace('_', ' ')),
            post.Source,
            new Size(post.Width, post.Height),
            -1,
            GetRating(post.Rating),
            GetRatingSafeLevel(post.Rating),
            Array.Empty<int>(),
            GetParent(post),
            GetChildren(),
            Array.Empty<Pool>(),
            GetTags(postHtml),
            GetNotes(post, postHtml));

    private static Post? CreatePost(HtmlDocument postHtml)
    {
        var idString = postHtml.DocumentNode.SelectSingleNode("//head/link[@rel='canonical']")
            ?.Attributes["href"]?.Value?.Split('=')?.Last();

        if (idString == null)
            return null;
        
        var id = int.Parse(idString);
        
        var url = postHtml.DocumentNode.SelectSingleNode("//head/meta[@property='og:image']").Attributes["content"].Value;
        var md5 = url.Split('/').Last().Split('.').First();
        
        var dateString = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Posted: ')]/text()[1]").InnerText[8..];
        var date = new DateTimeOffset(DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TimeSpan.FromHours(-5));
        
        var uploader = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Posted: ')]/a/text()")?.InnerText ?? "Anonymous";
        
        var source = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Source: ')]/a[1]")?.Attributes["href"].Value;
        
        var sizeString = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Size: ')]/text()").InnerText;
        var size = sizeString.Split(':').Last().Trim().Split('x').Select(int.Parse).ToList();
        
        var rating = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Rating: ')]/text()").InnerText.Split(' ').Last().ToLower();
        
        return new(
            new PostIdentity(id.ToString(), md5),
            url,
            url,
            url,
            ExistState.MarkDeleted,
            date,
            new Uploader("-1", uploader.Replace('_', ' ')),
            source,
            new Size(size[0], size[1]),
            -1,
            GetRating(rating),
            GetRatingSafeLevel(rating),
            Array.Empty<int>(),
            null,
            GetChildren(),
            Array.Empty<Pool>(),
            GetTags(postHtml),
            GetNotes(null, postHtml));
    }
}
