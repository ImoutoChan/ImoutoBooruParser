using System.Text.Json;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;
using Imouto.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Rule34;

public class Rule34ApiLoader : IBooruApiLoader
{
    private const string HtmlBaseUrl = "https://rule34.xxx";
    private const string JsonBaseUrl = "https://api.rule34.xxx";
    private readonly IFlurlClient _flurlHtmlClient;
    private readonly IFlurlClient _flurlJsonClient;

    public Rule34ApiLoader(IFlurlClientFactory factory, IOptions<Rule34Settings> options)
    {
        _flurlHtmlClient = factory.Get(new Url(HtmlBaseUrl))
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
            .Configure(x => SetAuthParameters(x, options));

        _flurlJsonClient = factory.Get(new Url(JsonBaseUrl))
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
            .Configure(x => SetAuthParameters(x, options));
    }

    public async Task<Post> GetPostAsync(int postId)
    {
        // https://rule34.xxx/index.php?page=post&s=view&id=
        var postHtml = await _flurlHtmlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "post",
                s = "view",
                id = postId
            })
            .GetHtmlDocumentAsync();
        
        // https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&id=
        var postJson = await _flurlJsonClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, id = postId
            })
            .GetJsonAsync<Rule34Post[]>();
        var post = postJson?.FirstOrDefault();
        
        return post != null 
            ? CreatePost(post, postHtml) 
            : null!;
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        // https://gelbooru.com/index.php?page=post&s=list&md5=
        var postHtml = await _flurlHtmlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "post",
                s = "list",
                md5 = md5
            })
            .WithAutoRedirect(true)
            .GetHtmlDocumentAsync();
        
        // https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=md5:
        var postJson = await _flurlJsonClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, tags = $"md5:{md5}"
            })
            .GetJsonAsync<Rule34Post[]>();
        
        var post = postJson?.FirstOrDefault();
        
        return post != null
            ? CreatePost(post, postHtml)
            : null;
    }

    public async Task<SearchResult> SearchAsync(string tags)
    {
        // https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=1girl
        var postJson = await _flurlJsonClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 20, tags = tags
            })
            .GetJsonAsync<Rule34Post[]>();

        return new SearchResult(postJson?
            .Select(x => new PostPreview(x.Id, x.Hash, x.Tags, false, false))
            .ToArray() ?? Array.Empty<PostPreview>());
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
        => post.ParentId != 0 ? new PostIdentity(post.ParentId, string.Empty) : null;

    /// <remarks>
    /// Haven't found any post with them
    /// </remarks>
    private static IReadOnlyCollection<PostIdentity> GetChildren() => Array.Empty<PostIdentity>();

    /// <remarks>
    /// Sample: https://rule34.xxx/index.php?page=post&amp;s=view&amp;id=6204314
    /// </remarks>
    private static IReadOnlyCollection<Note> GetNotes(Rule34Post? post, HtmlDocument postHtml)
    {
        if (post?.HasNotes != true)
            return Array.Empty<Note>();

        
        var boxes = postHtml.DocumentNode.SelectNodes(@"//*[@id='note-container']/*[@class='note-box']");
        var bodies = postHtml.DocumentNode.SelectNodes(@"//*[@id='note-container']/*[@class='note-body']");
        var notes = boxes != null && bodies != null
            ? boxes.Zip(bodies)
                .Select(x =>
                {
                    var box = x.First;
                    var body = x.Second;

                    var boxData = box.Attributes["style"].Value.Split(';')
                        .ToDictionary(x => x.Split(':').First().Trim(), x => x.Split(':').Last().Trim());

                    var height = boxData["height"].Trim('p', 'x');
                    var width = boxData["width"].Trim('p', 'x');
                    var top = boxData["top"].Trim('p', 'x');
                    var left = boxData["left"].Trim('p', 'x');

                    var size = new Size(GetSizeInt(width), GetSizeInt(height));
                    var point = new Position(GetPositionInt(top), GetPositionInt(left));

                    var id = Convert.ToInt32(body.Attributes["id"].Value.Split('-').Last());
                    var text = body.InnerText;

                    return new Note(id, text, point, size);
                })
            : Enumerable.Empty<Note>();

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
            .SelectSingleNode(@"//*[@id='tag-sidebar']")
            .SelectNodes(@"li")
            .Where(x => x.Attributes["class"]?.Value.StartsWith("tag-type-") == true)
            .Select(x =>
            {
                var type = x.Attributes["class"].Value.Split(' ').First().Split('-').Last();
                var name = x.SelectNodes("a")[1].InnerText;

                return new Tag(type, name);
            })
            .ToList();

    /// <summary>
    /// Auth isn't supported right now.
    /// </summary>
    private static void SetAuthParameters(FlurlHttpSettings settings, IOptions<Rule34Settings> options)
    {
        var delay = options.Value.PauseBetweenRequests;
        
        settings.BeforeCallAsync = async call =>
        {
            if (delay > TimeSpan.Zero)
                await Throttler.Get("rule34").UseAsync(delay);
        };
    }

    private static Post CreatePost(Rule34Post post, HtmlDocument postHtml) 
        => new(
            new PostIdentity(post.Id, post.Hash),
            post.FileUrl,
            !string.IsNullOrWhiteSpace(post.SampleUrl) ? post.SampleUrl : post.FileUrl,
            post.PreviewUrl,
            ExistState.Exist,
            DateTimeOffset.FromUnixTimeSeconds(post.Change),
            new Uploader(-1, post.Owner.Replace('_', ' ')),
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
}
