using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;
using ImoutoRebirth.BooruParser.Extensions;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Implementations.Gelbooru;

// notes 7858922
public class GelbooruApiLoader : IBooruApiLoader, IBooruApiAccessor
{
    private const string BaseUrl = "https://gelbooru.com/";
    private readonly IFlurlClient _flurlClient;

    public GelbooruApiLoader(IFlurlClientFactory factory, IOptions<GelbooruSettings> options) 
        => _flurlClient = factory.Get(new Url(BaseUrl)).Configure(x => SetAuthParameters(x, options));

    public async Task<Post> GetPostAsync(int postId)
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

        var post = postJson.Posts.First();
        
        return new Post(
            new PostIdentity(postId, post.Md5),
            post.FileUrl,
            post.PreviewUrl,
            ExistState.Exist,
            DateTimeOffset.Parse(post.CreatedAt),
            new Uploader(post.CreatorId, post.Owner.Replace('_', ' ')),
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
                page = "dapi", s = "post", q = "index", json = 1, limit = 1, md5 = md5
            })
            .GetJsonAsync<GelbooruPostPage>();
        
        
        var post = postJson.Posts.First();
        
        return new Post(
            new PostIdentity(post.Id, post.Md5),
            post.FileUrl,
            post.PreviewUrl,
            ExistState.Exist,
            DateTimeOffset.Parse(post.CreatedAt),
            new Uploader(post.CreatorId, post.Owner.Replace('_', ' ')),
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

    public async Task<SearchResult> SearchAsync(string tags)
    {
        //https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&limit=20&tags=1girl
        var postJson = await _flurlClient.Request("index.php")
            .SetQueryParams(new
            {
                page = "dapi", s = "post", q = "index", json = 1, limit = 100, tags = tags
            })
            .GetJsonAsync<GelbooruPostPage>();

        return new SearchResult(postJson.Posts
            .Select(x => new PostPreview(x.Id, x.Md5, x.Tags, false, false))
            .ToList());
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

    public Task FavoritePostAsync(int postId)
        => throw new NotSupportedException("It should be possible but isn't implemented right now");

    /// <remarks>
    /// Parent is always 0.
    /// </remarks>>
    private static PostIdentity? GetParent(GelbooruPost post)
        => post.ParentId != 0 ? new PostIdentity(post.ParentId, string.Empty) : null;

    /// <remarks>
    /// Haven't found any post with them
    /// </remarks>>
    private static IReadOnlyCollection<PostIdentity> GetChildren() => Array.Empty<PostIdentity>();

    private static IReadOnlyCollection<Note> GetNotes(GelbooruPost post, HtmlDocument postHtml)
    {
        if (post.HasNotes != "true")
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

                return new Note(id, text, point, size);
            }) ?? Enumerable.Empty<Note>();

        return notes.ToList();
        
        static int GetSizeInt(string number) => (int)(Convert.ToDouble(number) + 0.5);
        
        static int GetPositionInt(string number) => (int)Math.Ceiling(Convert.ToDouble(number) - 0.5);
    }

    private static Rating GetRating(string postRating) => GetRatingFromChar(postRating).Item1;

    private static RatingSafeLevel GetRatingSafeLevel(string postRating) => GetRatingFromChar(postRating).Item2;

    private static (Rating, RatingSafeLevel) GetRatingFromChar(string rating)
        => rating switch
        {
            "q" => (Rating.Questionable, RatingSafeLevel.None),
            "s" => (Rating.Safe, RatingSafeLevel.Sensitive),
            "g" => (Rating.Safe, RatingSafeLevel.General),
            "e" => (Rating.Explicit, RatingSafeLevel.None),
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
    private static void SetAuthParameters(FlurlHttpSettings settings, IOptions<GelbooruSettings> options)
    {
        var delay = options.Value.PauseBetweenRequests;
        
        settings.BeforeCallAsync = async call =>
        {
            if (delay > TimeSpan.Zero)
                await Throttler.Get("gelbooru").UseAsync(delay);
        };
        
        settings.AfterCall = _ => Throttler.Get("gelbooru").Release();
    }
}
