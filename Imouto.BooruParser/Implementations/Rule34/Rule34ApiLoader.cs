using System.Xml.Linq;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Imouto.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Rule34;

public class Rule34ApiLoader : IBooruApiLoader
{
    private readonly IOptions<Rule34Settings> _options;
    private const string JsonBaseUrl = "https://api.rule34.xxx";
    private readonly IFlurlClient _flurlJsonClient;

    public Rule34ApiLoader(IFlurlClientCache factory, IOptions<Rule34Settings> options)
    {
        _options = options;
        _flurlJsonClient = factory.GetForDomain(new Url(JsonBaseUrl))
            .WithHeader("Connection", "keep-alive")
            .WithHeader("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"")
            .WithHeader("sec-ch-ua-mobile", "?0")
            .WithHeader("sec-ch-ua-platform", "\"Windows\"")
            .WithHeader("DNT", "1")
            .WithHeader("Upgrade-Insecure-Requests", "1")
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36")
            .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
            .WithHeader("Sec-Fetch-Site", "none")
            .WithHeader("Sec-Fetch-Mode", "navigate")
            .WithHeader("Sec-Fetch-User", "?1")
            .WithHeader("Sec-Fetch-Dest", "document")
            .WithHeader("Accept-Language", "en")
            .BeforeCall(_ => DelayWithThrottler(options));
    }

    public async Task<Post> GetPostAsync(string postId)
    {
        // https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&id=
        var postJson = await Request()
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, id = postId,
                fields = "tag_info"
            })
            .GetJsonAsync<Rule34Post[]>();
        var post = postJson?.FirstOrDefault();
        
        return post != null
            ? CreatePost(post, await GetNotesAsync(post))
            : null!;
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        // https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=md5:
        var postJson = await Request()
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, tags = $"md5:{md5}",
                fields = "tag_info"
            })
            .GetJsonAsync<Rule34Post[]>();
        
        var post = postJson?.FirstOrDefault();
        
        return post != null
            ? CreatePost(post, await GetNotesAsync(post))
            : null;
    }

    public async Task<SearchResult> SearchAsync(string tags)
    {
        // https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=1girl
        var postJson = await Request()
            .SetQueryParam("page", "dapi")
            .SetQueryParam("s", "post")
            .SetQueryParam("q", "index")
            .SetQueryParam("json", 1)
            .SetQueryParam("limit", 20)
            .SetQueryParam("tags", tags)
            .SetQueryParam("pid", 0)
            .GetJsonAsync<Rule34Post[]>();

        return new SearchResult(postJson?
            .Select(x => new PostPreview(x.Id.ToString(), x.Hash, x.Tags, false, false))
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
            .GetJsonAsync<Rule34Post[]>();

        return new SearchResult(postJson?
            .Select(x => new PostPreview(
                x.Id.ToString(),
                x.Hash,
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
            .GetJsonAsync<Rule34Post[]>();

        return new SearchResult(postJson?
            .Select(x => new PostPreview(
                x.Id.ToString(),
                x.Hash,
                x.Tags,
                false,
                false))
            .ToArray() ?? [], results.SearchTags, nextPage);
    }

    public Task<SearchResult> GetPopularPostsAsync(PopularType type)
        => throw new NotSupportedException("Rule34 does not support popularity charts");

    public Task<HistorySearchResult<TagHistoryEntry>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
        => throw new NotSupportedException("Rule34 does not support history");

    public Task<HistorySearchResult<NoteHistoryEntry>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
        => throw new NotSupportedException("Rule34 does not support history");

    private static PostIdentity? GetParent(Rule34Post post)
        => post.ParentId != 0 ? new PostIdentity(post.ParentId.ToString(), string.Empty) : null;

    private IFlurlRequest Request()
        => _flurlJsonClient.Request("index.php")
            .AppendQueryParam("api_key", _options.Value.ApiKey)
            .AppendQueryParam("user_id", _options.Value.UserId);

    /// <remarks>
    /// Haven't found any post with them
    /// </remarks>
    private static IReadOnlyCollection<PostIdentity> GetChildren() => Array.Empty<PostIdentity>();

    private async Task<IReadOnlyCollection<Note>> GetNotesAsync(Rule34Post post)
    {
        if (post.HasNotes != true)
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
            .Select(x => new
            {
                Id = (int)x.Attribute("id")!,
                Text = (string?)x.Attribute("body") ?? string.Empty,
                X = (int)x.Attribute("x")!,
                Y = (int)x.Attribute("y")!,
                Width = (int)x.Attribute("width")!,
                Height = (int)x.Attribute("height")!
            })
            .OrderBy(x => x.Id)
            .Select(x => new Note(
                x.Id.ToString(),
                x.Text,
                new Position(x.Y, x.X),
                new Size(x.Width, x.Height)))
            .ToArray() ?? [];
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

    private static IReadOnlyCollection<Tag> GetTags(Rule34Post post)
        => post.TagInfo?
            .Select(x => new Tag(
                x.Type == "tag" ? "general" : x.Type,
                x.Name.Replace('_', ' ')))
            .ToArray()
            ?? post.Tags
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new Tag("general", x.Replace('_', ' ')))
                .ToArray();

    private static async Task DelayWithThrottler(IOptions<Rule34Settings> options)
    {
        var delay = options.Value.PauseBetweenRequests;
        if (delay > TimeSpan.Zero)
            await Throttler.Get("rule34").UseAsync(delay);
    }


    private static Post CreatePost(Rule34Post post, IReadOnlyCollection<Note> notes)
        => new(
            new PostIdentity(post.Id.ToString(), post.Hash),
            post.FileUrl,
            !string.IsNullOrWhiteSpace(post.SampleUrl) ? post.SampleUrl : post.FileUrl,
            post.PreviewUrl,
            ExistState.Exist,
            DateTimeOffset.FromUnixTimeSeconds(post.Change),
            new Uploader("-1", post.Owner.Replace('_', ' ')),
            post.Source,
            new Size(post.Width, post.Height),
            -1,
            GetRating(post.Rating),
            GetRatingSafeLevel(post.Rating),
            Array.Empty<int>(),
            GetParent(post),
            GetChildren(),
            Array.Empty<Pool>(),
            GetTags(post),
            notes);
}
