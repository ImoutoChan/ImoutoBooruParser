using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Implementations.Sankaku;

// test
// children: 31729492
// for history:
// auth with bearer, update it with refresh
// use graphql for history

public class SankakuApiLoader : IBooruApiLoader, IBooruApiAccessor
{
    private const string BaseUrl = "https://capi-v2.sankakucomplex.com/";
    private readonly IFlurlClient _flurlClient;
    private readonly ISankakuAuthManager _sankakuAuthManager;

    public SankakuApiLoader(
        IFlurlClientFactory factory, 
        IOptions<SankakuSettings> options,
        ISankakuAuthManager sankakuAuthManager)
    {
        _sankakuAuthManager = sankakuAuthManager;
        _flurlClient = factory.Get(new Url(BaseUrl)).Configure(x => SetAuthParameters(x, options));
    }

    public async Task<Post> GetPostAsync(int postId)
    {
        var post = await _flurlClient.Request("posts", postId).GetJsonAsync<SankakuPost>();

        return new Post(
            new PostIdentity(postId, post.Md5),
            post.FileUrl,
            post.PreviewUrl ?? post.FileUrl,
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
            post.PreviewUrl ?? post.FileUrl,
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
        ???
        var request = _flurlClient.Request("post_versions.json")
            .SetQueryParam("limit", limit);
        
        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var found = await request.GetJsonAsync<IReadOnlyCollection<SankakuTagsHistoryEntry>>(cancellationToken: ct);

        if (!found.Any())
            return new(Array.Empty<TagHistoryEntry>(), null);
        
        var entries = found
            .Select(x => new TagHistoryEntry(x.Id, x.UpdatedAt, x.PostId, x.ParentId, x.ParentChanged))
            .ToList();

        var nextPage = token?.Page[0] switch
        {
            null => $"b{found.Min(x => x.Id)}",
            'b' => $"b{found.Min(x => x.Id)}",
            'a' => $"a{found.Max(x => x.Id)}",
            var x when int.TryParse(x.ToString(), out var page) => (page + 1).ToString(),
            _ => "2"
        };

        return new(entries, new SearchToken(nextPage));
    }

    public async Task<HistorySearchResult<NoteHistoryEntry>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
    {
         ???
        var request = _flurlClient.Request("note_versions.json")
            .SetQueryParam("limit", limit);
        
        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var found = await request.GetJsonAsync<IReadOnlyCollection<SankakuNotesHistoryEntry>>(cancellationToken: ct);

        if (!found.Any())
            return new(Array.Empty<NoteHistoryEntry>(), null);
        
        var entries = found
            .Select(x => new NoteHistoryEntry(x.Id, x.PostId, x.UpdatedAt))
            .ToList();

        var nextPage = token?.Page[0] switch
        {
            null => $"b{found.Min(x => x.Id)}",
            'b' => $"b{found.Min(x => x.Id)}",
            'a' => $"a{found.Max(x => x.Id)}",
            var _ when int.TryParse(token.Page, out var page) => (page + 1).ToString(),
            _ => "2"
        };

        return new(entries, new SearchToken(nextPage));
    }

    public async Task FavoritePostAsync(int postId)
    {
        await _flurlClient.Request("favorites.json")
            .SetQueryParam("post_id", postId)
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

        return posts.Select(x => new PostIdentity(post.Id, post.Md5)).ToList();
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

    private static Rating GetRating(string postRating) => GetRatingFromChar(postRating).Item1;

    private static (Rating, RatingSafeLevel) GetRatingFromChar(string rating)
        => rating switch
        {
            "q" => (Rating.Questionable, RatingSafeLevel.None),
            "s" => (Rating.Safe, RatingSafeLevel.Sensitive),
            "e" => (Rating.Explicit, RatingSafeLevel.None),
            _ => (Rating.Questionable, RatingSafeLevel.None)
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
            3 => "copyright",
            4 => "character",
            8 => "metadata",
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
