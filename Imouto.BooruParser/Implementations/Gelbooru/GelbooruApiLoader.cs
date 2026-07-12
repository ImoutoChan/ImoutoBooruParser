using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;
using Imouto.BooruParser.Extensions;
using Imouto.BooruParser.Implementations.Danbooru;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Gelbooru;

public class GelbooruApiLoader : IBooruApiLoader
{
    private static readonly Regex DateTimeRegex = new(
        ".*(?<month>[A-Za-z]{3}).*(?<date>\\d{2}).*(?<hours>\\d{2})\\:(?<minutes>\\d{2})\\:(?<seconds>\\d{2}).*(?<tzhours>[+\\-]\\d{2})(?<tzminutes>\\d{2}).*(?<year>\\d{4})",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));
    
    private const string BaseUrl = "https://gelbooru.com/";

    private readonly IFlurlClient _flurlClient;
    private readonly IOptions<GelbooruSettings> _options;

    public GelbooruApiLoader(IFlurlClientCache factory, IOptions<GelbooruSettings> options)
    {
        _options = options;
        _flurlClient = factory
            .GetForDomain(new Url(BaseUrl))
            .BeforeCall(_ => DelayWithThrottler(options));
    }

    public async Task<Post> GetPostAsync(string postId)
    {
        // https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=1&id=
        var postJson = await Request()
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, id = postId
            })
            .GetJsonAsync<GelbooruPostPage>();

        var post = postJson.Posts?.FirstOrDefault();
        if (post != null)
            return await CreatePostAsync(post);

        // Deleted posts are not returned by DAPI, but can still have a public HTML page.
        HtmlDocument postHtml;
        try
        {
            postHtml = await Request()
                .SetQueryParams(new
                {
                    page = "post",
                    s = "view",
                    id = postId
                })
                .GetHtmlDocumentAsync();
        }
        catch (FlurlHttpException exception) when (exception.Call.Response?.StatusCode == 404)
        {
            throw new PostNotFoundException("Gelbooru", postId, exception);
        }

        return CreatePost(postHtml)
            ?? throw new PostNotFoundException("Gelbooru", postId);
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        // https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=1&md5=
        var postJson = await Request()
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, tags = $"md5:{md5}"
            })
            .GetJsonAsync<GelbooruPostPage>();
        
        var post = postJson.Posts?.FirstOrDefault();
        if (post != null)
            return await CreatePostAsync(post);

        var postHtml = await Request()
            .SetQueryParams(new
            {
                page = "post",
                s = "list",
                md5 = md5
            })
            .WithAutoRedirect(true)
            .GetHtmlDocumentAsync();

        return CreatePost(postHtml);
    }

    public async Task<SearchResult> SearchAsync(string tags)
    {
        // https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=20&tags=1girl
        var postJson = await Request()
            .SetQueryParam("page", "dapi")
            .SetQueryParam("s", "post")
            .SetQueryParam("q", "index")
            .SetQueryParam("json", 1)
            .SetQueryParam("limit", 20)
            .SetQueryParam("tags", tags)
            .SetQueryParam("pid", 0)
            .GetJsonAsync<GelbooruPostPage>();

        return new SearchResult(postJson.Posts?
            .Select(x => new PostPreview(x.Id.ToString(), x.Md5, x.Tags, false, false))
            .ToArray() ?? [], tags, 0);
    }

    public async Task<SearchResult> GetNextPageAsync(SearchResult results)
    {
        var nextPage = results.PageNumber + 1;

        var postJson = await Request()
            .SetQueryParam("page", "dapi")
            .SetQueryParam("s", "post")
            .SetQueryParam("q", "index")
            .SetQueryParam("json", 1)
            .SetQueryParam("limit", 20)
            .SetQueryParam("tags", results.SearchTags)
            .SetQueryParam("pid", nextPage)
            .GetJsonAsync<GelbooruPostPage>();

        return new SearchResult(postJson.Posts?
            .Select(x => new PostPreview(
                x.Id.ToString(),
                x.Md5,
                x.Tags,
                false,
                false))
            .ToArray() ?? [], results.SearchTags, nextPage);
    }

    public async Task<SearchResult> GetPreviousPageAsync(SearchResult results)
    {
        if (results.PageNumber <= 0)
            throw new ArgumentOutOfRangeException("PageNumber", results.PageNumber, null);

        var nextPage = results.PageNumber - 1;

        var postJson = await Request()
            .SetQueryParam("page", "dapi")
            .SetQueryParam("s", "post")
            .SetQueryParam("q", "index")
            .SetQueryParam("json", 1)
            .SetQueryParam("limit", 20)
            .SetQueryParam("tags", results.SearchTags)
            .SetQueryParam("pid", nextPage)
            .GetJsonAsync<GelbooruPostPage>();

        return new SearchResult(postJson.Posts?
            .Select(x => new PostPreview(
                x.Id.ToString(),
                x.Md5,
                x.Tags,
                false,
                false))
            .ToArray() ?? [], results.SearchTags, nextPage);
    }

    private IFlurlRequest Request()
        => _flurlClient.Request("index.php")
            .AppendQueryParam("api_key", _options.Value.ApiKey)
            .AppendQueryParam("user_id", _options.Value.UserId);

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
    private static IReadOnlyCollection<PostIdentity> GetChildren() => [];

    private async Task<IReadOnlyCollection<Note>> GetNotesAsync(GelbooruPost post)
    {
        if (!string.Equals(post.HasNotes, "true", StringComparison.OrdinalIgnoreCase))
            return [];

        var notesXml = await Request()
            .SetQueryParams(new
            {
                page = "dapi", s = "note", q = "index", post_id = post.Id
            })
            .GetStringAsync();

        return XDocument.Parse(notesXml).Root?
            .Elements("note")
            .Where(x => !string.Equals(
                (string?)x.Attribute("is_active"),
                "false",
                StringComparison.OrdinalIgnoreCase))
            .Select(x => new Note(
                ((int)x.Attribute("id")!).ToString(),
                GetPlainText((string?)x.Attribute("body") ?? string.Empty),
                new Position((int)x.Attribute("y")!, (int)x.Attribute("x")!),
                new Size((int)x.Attribute("width")!, (int)x.Attribute("height")!)))
            .OrderBy(x => int.Parse(x.Id, CultureInfo.InvariantCulture))
            .ToArray() ?? [];
    }

    private static IReadOnlyCollection<Note> GetNotes(HtmlDocument postHtml)
    {
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
            }) ?? [];

        return notes.ToList();
        
        static int GetSizeInt(string number)
            => (int)(double.Parse(number, CultureInfo.InvariantCulture) + 0.5);
        
        static int GetPositionInt(string number)
            => (int)Math.Ceiling(double.Parse(number, CultureInfo.InvariantCulture) - 0.5);
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
            .SelectSingleNode(@"//*[@id='tag-list']")!
            .SelectNodes(@"li")!
            .Where(x => x.Attributes["class"]?.Value.StartsWith("tag-type-") == true)
            .Select(x =>
            {
                var type = x.Attributes["class"].Value.Split('-').Last();
                var name = x.SelectSingleNode(@"a")!.InnerHtml;

                return new Tag(type, name);
            })
            .ToList();

    private async Task<IReadOnlyCollection<Tag>> GetTagsAsync(GelbooruPost post)
    {
        var names = post.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var found = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var chunk in names.Chunk(100))
        {
            var response = await Request()
                .SetQueryParams(new
                {
                    page = "dapi",
                    s = "tag",
                    q = "index",
                    json = 1,
                    limit = chunk.Length,
                    names = string.Join(' ', chunk)
                })
                .GetJsonAsync<GelbooruTagPage>();

            foreach (var tag in response.Tags ?? [])
                found[tag.Name] = tag.Type;
        }

        return names
            .Select(x => new Tag(
                found.TryGetValue(x, out var type) ? GetTagType(type) : "general",
                x.Replace('_', ' ')))
            .ToArray();
    }

    private static string GetTagType(int type)
        => type switch
        {
            0 => "general",
            1 => "artist",
            2 => "deprecated",
            3 => "copyright",
            4 => "character",
            5 => "metadata",
            _ => "general"
        };

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
        var month = DateTime.ParseExact(monthString, "MMM", CultureInfo.InvariantCulture).Month;
        var offsetMinutes = tzHours < 0 ? -tzMinutes : tzMinutes;
        
        return new DateTimeOffset(year, month, date, hours, minutes, seconds,
            new TimeSpan(tzHours, offsetMinutes, 0));
    }

    private async Task<Post> CreatePostAsync(GelbooruPost post)
    {
        var tagsTask = GetTagsAsync(post);
        var notesTask = GetNotesAsync(post);
        await Task.WhenAll(tagsTask, notesTask);

        return new(
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
            [],
            GetParent(post),
            GetChildren(),
            [],
            await tagsTask,
            await notesTask);
    }

    private static Post? CreatePost(HtmlDocument postHtml)
    {
        var idString = postHtml.DocumentNode.SelectSingleNode("//head/link[@rel='canonical']")
            ?.Attributes["href"]?.Value?.Split('=')?.Last();

        if (idString == null)
            return null;
        
        var id = int.Parse(idString);
        
        var url = postHtml.DocumentNode.SelectSingleNode("//head/meta[@property='og:image']")!.Attributes["content"].Value;
        var md5 = url.Split('/').Last().Split('.').First();
        
        var dateString = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Posted: ')]/text()[1]")!.InnerText[8..];
        var date = new DateTimeOffset(DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), TimeSpan.FromHours(-5));
        
        var uploader = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Posted: ')]/a/text()")?.InnerText ?? "Anonymous";
        
        var source = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Source: ')]/a[1]")?.Attributes["href"].Value;
        
        var sizeString = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Size: ')]/text()")!.InnerText;
        var size = sizeString.Split(':').Last().Trim().Split('x').Select(int.Parse).ToList();
        
        var rating = postHtml.DocumentNode.SelectSingleNode("//li[contains (., 'Rating: ')]/text()")!.InnerText.Split(' ').Last().ToLowerInvariant();
        
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
            [],
            null,
            GetChildren(),
            [],
            GetTags(postHtml),
            GetNotes(postHtml));
    }

    private static string GetPlainText(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return WebUtility.HtmlDecode(document.DocumentNode.InnerText);
    }
}
