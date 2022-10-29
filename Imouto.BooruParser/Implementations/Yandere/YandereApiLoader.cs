using System.Globalization;
using System.Text.RegularExpressions;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;
using ImoutoRebirth.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Implementations.Yandere;

// test 
/// correct time
/// parent md5 1032014
/// children md5s 1032017
/// pools 1032020
/// tags with type 729673
/// notes 729673
/// extensions for tags and notes
public class YandereApiLoader : IBooruApiLoader, IBooruApiAccessor
{
    private static readonly Regex NotePositionRegex = new(
            "width[:\\s]*(?<width>\\d+.{0,1}\\d*)px.*height[:\\s]*(?<height>\\d+.{0,1}\\d*)px.*top[:\\s]*(?<top>\\d+.{0,1}\\d*)px.*left[:\\s]*(?<left>\\d+.{0,1}\\d*)px",
            RegexOptions.Compiled);
    
    private const string BaseUrl = "https://yande.re";

    private readonly IFlurlClient _flurlClient;

    public YandereApiLoader(IFlurlClientFactory factory, IOptions<YandereSettings> options)
    {
        _flurlClient = factory.Get(new Url(BaseUrl)).Configure(x => SetAuthParameters(x, options));
    }

    public async Task<Post> GetPostAsync(int postId)
    {
        var posts = await _flurlClient.Request("post", "index.json")
            .SetQueryParam("tags", $"id:{postId}")
            .GetJsonAsync<YanderePost[]>();
        var post = posts.First();

        var postHtml = await _flurlClient
            .Request("post", "show", postId)
            .GetHtmlDocumentAsync();

        return new Post(
            new PostIdentity(postId, post.Md5),
            post.FileUrl,
            post.SampleUrl ?? post.JpegUrl,
            GetExistState(postHtml),
            DateTimeOffset.FromUnixTimeSeconds(post.CreatedAt),
            new Uploader(post.CreatorId, post.Author),
            post.Source,
            new Size(post.Width, post.Height),
            post.FileSize,
            GetRatingFromChar(post.Rating),
            RatingSafeLevel.None,
            Array.Empty<int>(),
            await GetPostIdentityAsync(post.ParentId),
            await GetChildrenAsync(postHtml),
            await GetPoolsAsync(postId, postHtml),
            GetTags(postHtml),
            GetNotes(post, postHtml));
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        // https://yande.re/post.json?tags=md5%3Ae6500b62d4003a5f4ba226d3a665c25a
        var posts = await _flurlClient.Request("post.json")
            .SetQueryParam("tags", $"md5:{md5} holds:all")
            .GetJsonAsync<IReadOnlyCollection<YanderePost>>();

        if (!posts.Any())
            return null;
        var post = posts.First();

        var postHtml = await _flurlClient
            .Request("post", "show", post.Id)
            .GetHtmlDocumentAsync();

        return new Post(
            new PostIdentity(post.Id, post.Md5),
            post.FileUrl,
            post.SampleUrl ?? post.JpegUrl,
            GetExistState(postHtml),
            DateTimeOffset.FromUnixTimeSeconds(post.CreatedAt),
            new Uploader(post.CreatorId, post.Author),
            post.Source,
            new Size(post.Width, post.Height),
            post.FileSize,
            GetRatingFromChar(post.Rating),
            RatingSafeLevel.None,
            Array.Empty<int>(),
            await GetPostIdentityAsync(post.ParentId),
            await GetChildrenAsync(postHtml),
            await GetPoolsAsync(post.Id, postHtml),
            GetTags(postHtml),
            GetNotes(post, postHtml));
    }

    /// <summary>
    /// Remember to include "holds:all" if you want to see all posts.
    /// </summary>
    public async Task<SearchResult> SearchAsync(string tags)
    {
        var posts = await _flurlClient.Request("post.json")
            .SetQueryParam("tags", tags)
            .GetJsonAsync<IReadOnlyCollection<YanderePost>>();

        return new SearchResult(posts
            .Select(x => new PostPreview(x.Id, x.Md5, x.Tags, false, false))
            .ToList());
    }

    public async Task<SearchResult> GetPopularPostsAsync(PopularType type)
    {
        var period = type switch
        {
            PopularType.Day => "1d",
            PopularType.Week => "1w",
            PopularType.Month => "1m",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        // https://yande.re/post/popular_recent.json?period=1w
        var posts = await _flurlClient.Request("post", "popular_recent.json")
            .SetQueryParam("period", period)
            .GetJsonAsync<IReadOnlyCollection<YanderePost>>();

        return new SearchResult(posts
            .Select(x => new PostPreview(x.Id, x.Md5, x.Tags, false, false))
            .ToList());
    }

    public async Task<HistorySearchResult<TagHistoryEntry>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
    {
        var request = _flurlClient.Request("history");

        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var pageHtml = await request.GetHtmlDocumentAsync(cancellationToken: ct);

        var entries = pageHtml.DocumentNode
            .SelectNodes("//*[@id='history']/tbody/tr")
            ?.Select(x =>
            {
                var id = int.Parse(x.Attributes["id"].Value[1..]);
                var data = x.SelectNodes("td");
                return (id, data);
            })
            .Where(x => x.data[0].InnerHtml == "Post")
            .Select(x =>
            {
                var data = x.data;
                var postId = int.Parse(data[2].ChildNodes[0].InnerHtml);
                var date = DateTime.Parse(data[3].InnerHtml);

                int? parentId = null;
                var parentChanged = false;
                var parentNodes = data[5].SelectNodes("span/a");
                if (parentNodes?.Count == 1)
                {
                    parentId = int.Parse(parentNodes.First().InnerText);
                    parentChanged = true;
                }

                return new TagHistoryEntry(
                    x.id,
                    new DateTimeOffset(date, TimeSpan.Zero),
                    postId,
                    parentId,
                    parentChanged);
            }) ?? Enumerable.Empty<TagHistoryEntry>();

        var nextPage = token?.Page switch
        {
            var x when int.TryParse(x, out var page) => (page + 1).ToString(),
            _ => "2"
        };

        return new HistorySearchResult<TagHistoryEntry>(entries.ToList(), new SearchToken(nextPage));
    }

    public async Task<HistorySearchResult<NoteHistoryEntry>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
    {
        var request = _flurlClient.Request("note", "history");

        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var pageHtml = await request.GetHtmlDocumentAsync(cancellationToken: ct);

        var entries = pageHtml.DocumentNode
            .SelectNodes("//*[@id='content']/table/tbody/tr")
            .Select(x =>
            {
                var postId = int.Parse(x.SelectNodes("td")[1].SelectSingleNode("a").InnerHtml);
                var dateString = x.SelectNodes("td")[5].InnerHtml;
                var date = DateTime.ParseExact(dateString, "MM/dd/yy", CultureInfo.InvariantCulture);

                return new NoteHistoryEntry(-1, postId, date);
            })
            .ToList();

        var nextPage = token?.Page switch
        {
            var x when int.TryParse(x, out var page) => (page + 1).ToString(),
            _ => "2"
        };

        return new HistorySearchResult<NoteHistoryEntry>(entries, new SearchToken(nextPage));
    }

    public async Task FavoritePostAsync(int postId)
    {
        await _flurlClient.Request("post", "vote.json")
            .PostMultipartAsync(content => content
                .Add("id", new StringContent(postId.ToString()))
                .Add("score", new StringContent("3")));
    }

    private async Task<PostIdentity?> GetPostIdentityAsync(int? postId)
    {
        if (postId == null)
            return null;

        return await GetPostIdentityAsync(postId.Value);
    }
    
    private async Task<PostIdentity> GetPostIdentityAsync(int postId)
    {
        var posts = await _flurlClient.Request("post", "index.json")
            .SetQueryParam("tags", $"id:{postId} holds:all")
            .GetJsonAsync<YanderePost[]>();

        var post = posts.First();

        return new PostIdentity(post.Id, post.Md5);
    }

    private async Task<IReadOnlyCollection<PostIdentity>> GetChildrenAsync(
        HtmlDocument postHtml)
    {
        var childrenIds = postHtml.DocumentNode
            .SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")
            ?.FirstOrDefault(x => x.InnerHtml.Contains("child post"))
            ?.SelectNodes(@"a").Where(x => x.Attributes["href"]?.Value.Contains("/post/show/") ?? false)
            .Select(x => int.Parse(x.InnerHtml))
            .ToArray() ?? Array.Empty<int>();

        if (!childrenIds.Any())
            return Array.Empty<PostIdentity>();

        var childrenTasks = childrenIds.Select(GetPostIdentityAsync).ToList();

        await Task.WhenAll(childrenTasks);

        return childrenTasks.Select(x => x.Result).ToList();
    }
    
    private static ExistState GetExistState(HtmlDocument postHtml)
    {
        var isDeleted = postHtml.DocumentNode
            .SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")
            ?.Any(x => x.InnerHtml.Contains("This post was deleted.")) ?? false;

        return isDeleted ? ExistState.MarkDeleted : ExistState.Exist;
    }

    private async Task<IReadOnlyCollection<Pool>> GetPoolsAsync(int postId, HtmlDocument postHtml)
    {
        var pools = postHtml.DocumentNode
            .SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")
            ?.Where(x => (x.Attributes["id"]?.Value)?[..4] == "pool")
            .Select(x =>
            {
                var id = int.Parse(x.Attributes["id"].Value[4..]);
                var aNodes = x.SelectNodes("div/p/a");
                var poolNode = aNodes.Last(y => y.Attributes["href"].Value[..5] == "/pool");
                var  name = poolNode.InnerHtml;

                return (id, name);
            }) ?? Enumerable.Empty<(int, string)>();
        
        var tasks = pools
            .Select(x => GetPoolForPostAsync(x.id, postId))
            .ToList();
        await Task.WhenAll(tasks);
        return tasks.Select(x => x.Result).ToList();
    }

    private async Task<Pool> GetPoolForPostAsync(int poolId, int postId)
    {
        // https://yande.re/pool/show.json?id={id}
        var pool = await _flurlClient.Request("pool", "show.json")
            .SetQueryParam("id", poolId)
            .GetJsonAsync<YanderePool>();

        return new Pool(pool.Id, pool.Name.Replace('_', ' '), Array.IndexOf(pool.Posts.Select(x => x.Id).ToArray(), postId));
    }

    private static IReadOnlyCollection<Note> GetNotes(YanderePost post, HtmlDocument postHtml)
    {
        if (post.LastNotedAt == 0)
            return Array.Empty<Note>();

        var notes = postHtml.DocumentNode
            .SelectSingleNode(@"//*[@id='note-container']")
            ?.SelectNodes(@"div")
            ?.SelectPairs((styleNode, textNode) =>
            {
                var stylesStrings = styleNode.Attributes["style"].Value;
                var match = NotePositionRegex.Match(stylesStrings);

                var height = match.Groups["height"].Value;
                var width = match.Groups["width"].Value;
                var top = match.Groups["top"].Value;
                var left = match.Groups["left"].Value;

                var size = new Size(GetSizeInt(width), GetSizeInt(height));
                var point = new Position(GetPositionInt(top), GetPositionInt(left));

                var id = Convert.ToInt32(textNode.Attributes["id"].Value.Split('-').Last());
                var text = textNode.InnerHtml;

                return new Note(id, text, point, size);
            }) ?? Enumerable.Empty<Note>();

        return notes.ToList();
        
        static int GetSizeInt(string number) => (int)(Convert.ToDouble(number) + 0.5);
        
        static int GetPositionInt(string number) => (int)Math.Ceiling(Convert.ToDouble(number) - 0.5);
    }

    private static Rating GetRatingFromChar(string rating)
        => rating switch
        {
            "q" => Rating.Questionable,
            "s" => Rating.Safe,
            "e" => Rating.Explicit,
            _ => throw new ArgumentOutOfRangeException(nameof(rating))
        };

    private static IReadOnlyCollection<Tag> GetTags(HtmlDocument postHtml)
    {
        return postHtml.DocumentNode
            .SelectSingleNode(@"//*[@id='tag-sidebar']")
            .SelectNodes(@"li")
            .Select(x =>
            {
                var type = x.Attributes["class"].Value.Split('-').Last();
                var aNode = x.SelectSingleNode(@"a[2]");
                var name = aNode.InnerHtml;

                return new Tag(type, name);
            })
            .ToList();
    }

    private static void SetAuthParameters(FlurlHttpSettings settings, IOptions<YandereSettings> options)
    {
        var login = options.Value.Login;
        var passwordHash = options.Value.PasswordHash;
        var delay = options.Value.PauseBetweenRequests;

        settings.BeforeCallAsync = async call =>
        {
            if (login != null && passwordHash != null)
                call.Request.SetQueryParam("login", login).SetQueryParam("password_hash", passwordHash);

            if (delay > TimeSpan.Zero)
                await Throttler.Get("Yandere").UseAsync(delay);
        };

        settings.AfterCall = _ => Throttler.Get("Yandere").Release();
    }
}
