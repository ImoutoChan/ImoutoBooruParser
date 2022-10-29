using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Implementations.Sankaku;

public class SankakuApiLoader : IBooruApiLoader, IBooruApiAccessor
{
    private const string ApiBaseUrl = "https://capi-v2.sankakucomplex.com/";
    private const string HtmlBaseUrl = "https://chan.sankakucomplex.com/";

    private readonly IFlurlClient _flurlClient;
    private readonly IFlurlClient _htmlFlurlClient;
    private readonly ISankakuAuthManager _sankakuAuthManager;

    public SankakuApiLoader(
        IFlurlClientFactory factory, 
        IOptions<SankakuSettings> options,
        ISankakuAuthManager sankakuAuthManager)
    {
        _sankakuAuthManager = sankakuAuthManager;
        _flurlClient = factory.Get(new Url(ApiBaseUrl))
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
            .WithHeader("Accept-Encoding", "gzip, deflate, br")
            .WithHeader("Accept-Language", "en")
            .Configure(x => SetAuthParameters(x, options));
        
        _htmlFlurlClient = factory.Get(new Url(HtmlBaseUrl))
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
            .WithHeader("Accept-Encoding", "gzip, deflate, br")
            .WithHeader("Accept-Language", "en")
            .Configure(x => SetAuthParameters(x, options));
    }

    public async Task<Post> GetPostAsync(int postId)
    {
        var post = await _flurlClient.Request("posts", postId).GetJsonAsync<SankakuPost>();

        return new Post(
            new PostIdentity(postId, post.Md5),
            post.FileUrl,
            post.SampleUrl ?? post.FileUrl,
            ExistState.Exist,
            DateTimeOffset.FromUnixTimeSeconds(post.CreatedAt.S),
            new Uploader(post.Author.Id, post.Author.Name),
            post.Source,
            new Size(post.Width, post.Height),
            post.FileSize,
            GetRating(post.Rating),
            RatingSafeLevel.None,
            Array.Empty<int>(),
            await GetPostIdentityAsync(post.ParentId),
            await GetChildrenAsync(post),
            await GetPoolsAsync(postId).ToListAsync(),
            GetTags(post),
            await GetNotesAsync(post));
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        // https://capi-v2.sankakucomplex.com/posts?tags=md5:123e273a06a85f7a897ec1561b26911a
        var posts = await _flurlClient.Request("posts")
            .SetQueryParam("tags", $"md5:{md5}")
            .GetJsonAsync<IReadOnlyCollection<SankakuPost>>();

        if (!posts.Any())
            return null;

        var post = posts.First();

        return new Post(
            new PostIdentity(post.Id, post.Md5),
            post.FileUrl,
            post.SampleUrl ?? post.FileUrl,
            ExistState.Exist,
            DateTimeOffset.FromUnixTimeSeconds(post.CreatedAt.S),
            new Uploader(post.Author.Id, post.Author.Name),
            post.Source,
            new Size(post.Width, post.Height),
            post.FileSize,
            GetRating(post.Rating),
            RatingSafeLevel.None,
            Array.Empty<int>(),
            await GetPostIdentityAsync(post.ParentId),
            await GetChildrenAsync(post),
            await GetPoolsAsync(post.Id).ToListAsync(),
            GetTags(post),
            await GetNotesAsync(post));
    }

    public async Task<SearchResult> SearchAsync(string tags)
    {
        var posts = await _flurlClient.Request("posts")
            .SetQueryParam("tags", tags)
            .GetJsonAsync<IReadOnlyCollection<SankakuPost>>();

        return new SearchResult(posts
            .Select(x => new PostPreview(
                x.Id, 
                x.Md5, 
                string.Join(" ", x.Tags.Select(y => y.TagName)), 
                false,
                false))
            .ToList());
    }

    public Task<SearchResult> GetPopularPostsAsync(PopularType type)
    {
        var end = DateTime.Now.Date;
        var start = type switch
        {
            PopularType.Day => end.AddDays(-1),
            PopularType.Week => end.AddDays(-7),
            PopularType.Month => end.AddMonths(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        const string dateFormat = "yyyy-MM-dd";
        var search = $"date:{start.ToString(dateFormat)}..{end.ToString(dateFormat)} order:quality";

        return SearchAsync(search);
    }

    public async Task<HistorySearchResult<TagHistoryEntry>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
    {
        var response = await _flurlClient.Request("graphql")
            .WithHeader("content-type", "application/json")
            .PostAsync(new StringContent(
                "{\"operationName\":\"PostTagHistoryConnection\",\"variables\":{},\"query\":\"query PostTagHistoryConnection {\\n  postTagHistoryConnection(\\n    first: 100\\n    after: \\\""
                + $"{token?.Page}"
                +
                "\\\"\\n    before: \\\"\\\"\\n    lang: \\\"en\\\"\\n    tagNames: []\\n    userNames: []\\n    postIds: []\\n    addedTags: []\\n    removedTags: []\\n    isRatingChanged: null\\n    isSourceChanged: null\\n    isParentChanged: null\\n    negativeScoreOnly: null\\n    ipAddresses: []\\n    excludeSystemUser: null\\n    order: \\\"\\\"\\n    limit: 40\\n    sortBy: \\\"\\\"\\n    sortDirection: null\\n  ) {\\n    totalCount\\n    pageInfo {\\n      hasNextPage\\n      hasPreviousPage\\n      startCursor\\n      endCursor\\n    }\\n    edges {\\n      node {\\n        id\\n        post {\\n          id\\n        }\\n        parent\\n        createdAt\\n      }\\n    }\\n  }\\n}\\n\"}"
                ), cancellationToken: ct)
            .ReceiveJson<SankakuTagHistoryDocument>();

        var entries = response.Data.PostTagHistoryConnection.Edges.Select(x => x.Node)
            .Select(x => new TagHistoryEntry(
                x.Id, 
                DateTimeOffset.FromUnixTimeSeconds(long.Parse(x.CreatedAt)), 
                x.Post.Id, 
                x.Parent != null ? int.Parse(x.Parent) : null, 
                true))
            .ToList();

        var nextPage = response.Data.PostTagHistoryConnection.PageInfo.HasNextPage
            ? new SearchToken(response.Data.PostTagHistoryConnection.PageInfo.EndCursor)
            : null;

        return new(entries, nextPage);
    }

    public async Task<HistorySearchResult<NoteHistoryEntry>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
    {
        var request = _htmlFlurlClient.Request("note", "history");
        
        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var document = await request.GetHtmlDocumentAsync(cancellationToken: ct);

        var entries = document.DocumentNode.SelectNodes("//*[@id='content']/table/tbody/tr")
            .Select(x =>
            {
                var postId = int.Parse(x.SelectNodes("td")[1].SelectSingleNode("a").InnerHtml);
                var dateString = x.SelectNodes("td")[5].InnerHtml;
                var date = DateTime.Parse(dateString);

                return new NoteHistoryEntry(-1, postId, new DateTimeOffset(date, TimeSpan.Zero));
            })
            .ToList();

        var nextPage = token?.Page[0] switch
        {
            null => "2",
            _ when int.TryParse(token.Page, out var page) => (page + 1).ToString(),
            _ => "2"
        };

        return new(entries, new SearchToken(nextPage));
    }

    public async Task FavoritePostAsync(int postId)
    {
        // https://capi-v2.sankakucomplex.com/posts/30879033/favorite?lang=en
        await _flurlClient.Request("posts", postId, "favorite")
            .SetQueryParam("lang", "en")
            .PostAsync();
    }

    private async Task<PostIdentity?> GetPostIdentityAsync(int? postId)
    {
        if (postId == null)
            return null;

        return await GetPostIdentityAsync(postId.Value);
    }
    
    private async Task<PostIdentity> GetPostIdentityAsync(int postId)
    {
        var post = await _flurlClient.Request("posts", postId)
            .GetJsonAsync<SankakuPost>();

        return new PostIdentity(post.Id, post.Md5);
    }

    private async Task<IReadOnlyCollection<PostIdentity>> GetChildrenAsync(SankakuPost post)
    {
        if (!post.HasChildren)
            return Array.Empty<PostIdentity>();
        
        // https://capi-v2.sankakucomplex.com/posts?tags=parent:31729492
        var posts = await _flurlClient.Request("posts")
            .SetQueryParam("tags", $"parent:{post.Id}")
            .GetJsonAsync<SankakuPost[]>();

        return posts.Select(x => new PostIdentity(x.Id, x.Md5)).ToList();
    }

    private async IAsyncEnumerable<Pool> GetPoolsAsync(int postId)
    {
        // https://capi-v2.sankakucomplex.com/post/31236940/pools
        var pools = await _flurlClient.Request("post", postId, "pools")
            .GetJsonAsync<IReadOnlyCollection<SankakuPostPool>>();

        foreach (var poolInfo in pools)
        {
            // https://capi-v2.sankakucomplex.com/pools/451910
            var pool = await _flurlClient.Request("pools", poolInfo.Id)
                .GetJsonAsync<SankakuPool>();

            var poolPosts = pool.Posts.Select(x => x.Id).ToArray();

            yield return new Pool(poolInfo.Id, poolInfo.Name, Array.IndexOf(poolPosts, postId));
        }
    }

    private async Task<IReadOnlyCollection<Note>> GetNotesAsync(SankakuPost post)
    {
        if (!post.HasNotes)
            return Array.Empty<Note>();

        //https://capi-v2.sankakucomplex.com/posts/31930965/notes
        var notes = await _flurlClient.Request("posts", post.Id, "notes")
            .GetJsonAsync<IReadOnlyCollection<SankakuNote>>();
        
        return notes
            .Select(x => new Note(x.Id, x.Body, new Position(x.Y, x.X), new Size(x.Width, x.Height)))
            .ToList();
    }

    private static Rating GetRating(string rating)
        => rating switch
        {
            "q" => Rating.Questionable,
            "s" => Rating.Safe,
            "e" => Rating.Explicit,
            _ => Rating.Questionable
        };

    private static IReadOnlyCollection<Tag> GetTags(SankakuPost post)
        => post.Tags
            .Select(x => new Tag(GetTagType(x.Type), x.TagName.Replace('_', ' ')))
            .ToList();
    
    private static string GetTagType(int type) 
        => type switch
        {
            0 => "general",
            1 => "artist",
            2 => "studio",
            3 => "copyright",
            4 => "character",
            5 => "genre",
            8 => "medium",
            9 => "meta",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

    private void SetAuthParameters(FlurlHttpSettings settings, IOptions<SankakuSettings> options)
    {
        settings.BeforeCallAsync = async call =>
        {
            var accessToken = await _sankakuAuthManager.GetTokenAsync();
            var delay = options.Value.PauseBetweenRequests;
            
            if (accessToken != null)
                call.Request.WithHeader("Authorization", $"Bearer {accessToken}");

            if (delay > TimeSpan.Zero)
                await Throttler.Get("Sankaku").UseAsync(delay);
        };
        
        settings.AfterCall = _ => Throttler.Get("Sankaku").Release();
    }
}
