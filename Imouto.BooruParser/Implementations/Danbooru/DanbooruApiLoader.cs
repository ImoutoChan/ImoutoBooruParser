using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Danbooru;

public class DanbooruApiLoader : IBooruApiLoader, IBooruApiAccessor
{
    private const string BaseUrl = "https://danbooru.donmai.us";
    private readonly IFlurlClient _flurlClient;

    public DanbooruApiLoader(IFlurlClientFactory factory, IOptions<DanbooruSettings> options)
    {
        _flurlClient = factory.Get(new Url(BaseUrl)).Configure(x => SetAuthParameters(x, options));
    }

    public async Task<Post> GetPostAsync(int postId)
    {
        var post = await _flurlClient.Request("posts", $"{postId}.json")
            .SetQueryParam("only", "id,tag_string_artist,tag_string_character,tag_string_copyright,pools,tag_string_general,tag_string_meta,parent_id,md5,file_url,large_file_url,preview_file_url,file_ext,last_noted_at,is_banned,is_deleted,created_at,uploader_id,source,image_width,image_height,file_size,rating,media_metadata[metadata],parent[id,md5],children[id,md5],notes[id,x,y,width,height,body],uploader[id,name]")
            .GetJsonAsync<DanbooruPost>();

        return new Post(
            new PostIdentity(postId, post.Md5),
            post.FileUrl,
            post.LargeFileUrl ?? post.PreviewFileUrl,
            post.IsBanned || post.IsDeleted ? ExistState.MarkDeleted : ExistState.Exist,
            post.CreatedAt.ToUniversalTime(),
            new Uploader(post.UploaderId, post.Uploader.Name.Replace('_', ' ')),
            post.Source,
            new Size(post.ImageWidth, post.ImageHeight),
            post.FileSize,
            GetRating(post.Rating),
            GetRatingSafeLevel(post.Rating),
            GetUgoiraMetadata(post),
            GetParent(post),
            GetChildren(post),
            await GetPoolsAsync(postId),
            GetTags(post),
            GetNotes(post));
    }

    public async Task<Post?> GetPostByMd5Async(string md5)
    {
        var posts = await _flurlClient.Request("posts.json")
            .SetQueryParam("only", "id,tag_string_artist,tag_string_character,tag_string_copyright,pools,tag_string_general,tag_string_meta,parent_id,md5,file_url,large_file_url,preview_file_url,file_ext,last_noted_at,is_banned,is_deleted,created_at,uploader_id,source,image_width,image_height,file_size,rating,media_metadata[metadata],parent[id,md5],children[id,md5],notes[id,x,y,width,height,body],uploader[id,name]")
            .SetQueryParam("tags", $"md5:{md5}")
            .GetJsonAsync<IReadOnlyCollection<DanbooruPost>>();

        if (!posts.Any())
            return null;

        var post = posts.First();
        return new Post(
            new PostIdentity(post.Id, post.Md5),
            post.FileUrl,
            post.LargeFileUrl ?? post.PreviewFileUrl,
            post.IsBanned || post.IsDeleted ? ExistState.MarkDeleted : ExistState.Exist,
            post.CreatedAt,
            new Uploader(post.UploaderId, post.Uploader.Name.Replace('_', ' ')),
            post.Source,
            new Size(post.ImageWidth, post.ImageHeight),
            post.FileSize,
            GetRating(post.Rating),
            GetRatingSafeLevel(post.Rating),
            GetUgoiraMetadata(post),
            GetParent(post),
            GetChildren(post),
            await GetPoolsAsync(post.Id),
            GetTags(post),
            GetNotes(post));
    }

    public async Task<SearchResult> SearchAsync(string tags)
    {
        var posts = await _flurlClient.Request("posts.json")
            .SetQueryParam("tags", tags)
            .SetQueryParam("only", "id,md5,tag_string,is_banned,is_deleted")
            .GetJsonAsync<IReadOnlyCollection<DanbooruPostPreview>>();

        return new SearchResult(posts
            .Select(x => new PostPreview(x.Id, x.Md5, x.TagString, x.IsBanned, x.IsDeleted))
            .ToList());
    }

    public async Task<SearchResult> GetPopularPostsAsync(PopularType type)
    {
        var scale = type switch
        {
            PopularType.Day => "day",
            PopularType.Week => "week",
            PopularType.Month => "month",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        var posts = await _flurlClient.Request("explore", "posts", "popular.json")
            .SetQueryParam("date", $"{DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ss.fffzzz}")
            .SetQueryParam("scale", scale)
            .SetQueryParam("only", "id,md5,tag_string,is_banned,is_deleted")
            .GetJsonAsync<IReadOnlyCollection<DanbooruPostPreview>>();

        return new SearchResult(posts
            .Select(x => new PostPreview(x.Id, x.Md5, x.TagString, x.IsBanned, x.IsDeleted))
            .ToList());
    }

    public async Task<HistorySearchResult<TagHistoryEntry>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default)
    {
        var request = _flurlClient.Request("post_versions.json")
            .SetQueryParam("limit", limit);
        
        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var found = await request.GetJsonAsync<IReadOnlyCollection<DanbooruTagsHistoryEntry>>(cancellationToken: ct);

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
        var request = _flurlClient.Request("note_versions.json")
            .SetQueryParam("limit", limit);
        
        if (token != null)
            request = request.SetQueryParam("page", token.Page);

        var found = await request.GetJsonAsync<IReadOnlyCollection<DanbooruNotesHistoryEntry>>(cancellationToken: ct);

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

    private static PostIdentity? GetParent(DanbooruPost post)
        => post.Parent != null ? new PostIdentity(post.Parent.Id, post.Parent.Md5) : null;

    private static IReadOnlyCollection<PostIdentity> GetChildren(DanbooruPost post)
        => post.Children.Select(x => new PostIdentity(x.Id, x.Md5)).ToList();

    private static IReadOnlyCollection<int> GetUgoiraMetadata(DanbooruPost post)
    {
        var isUgoira = post.FileExt == "zip";
        if (!isUgoira)
            return Array.Empty<int>();

        return post.MediaMetadata.Metadata.UgoiraFrameDelays ?? Array.Empty<int>();
    }

    private async Task<IReadOnlyCollection<Pool>> GetPoolsAsync(int postId)
    {
        var pools = await _flurlClient.Request("pools.json")
            .SetQueryParam("search[post_tags_match]", $"id:{postId}")
            .SetQueryParam("only", $"id,name,post_ids")
            .GetJsonAsync<IReadOnlyCollection<DanbooruPool>>();

        return pools
            .Select(x => new Pool(x.Id, x.Name, Array.IndexOf(x.PostIds, postId)))
            .ToList();
    }

    private static IReadOnlyCollection<Note> GetNotes(DanbooruPost post)
    {
        if (post.LastNotedAt == null)
            return Array.Empty<Note>();

        return post.Notes
            .Select(x => new Note(x.Id, x.Body, new Position(x.Y, x.X), new Size(x.Width, x.Height)))
            .ToList();
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

    private static IReadOnlyCollection<Tag> GetTags(DanbooruPost post)
        => post.TagStringArtist.Split(' ').Select(x => (Type: "artist", Tag: x))
            .Union(post.TagStringCharacter.Split(' ').Select(x => (Type: "character", Tag: x)))
            .Union(post.TagStringCopyright.Split(' ').Select(x => (Type: "copyright", Tag: x)))
            .Union(post.TagStringGeneral.Split(' ').Select(x => (Type: "general", Tag: x)))
            .Union(post.TagStringMeta.Split(' ').Select(x => (Type: "meta", Tag: x)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Tag))
            .Select(x => new Tag(x.Type, x.Tag.Replace('_', ' ')))
            .ToList();

    private static void SetAuthParameters(FlurlHttpSettings settings, IOptions<DanbooruSettings> options)
    {
        var login = options.Value.Login;
        var apiKey = options.Value.ApiKey;
        var delay = options.Value.PauseBetweenRequests;

        settings.BeforeCallAsync = async call =>
        {
            if (options.Value.Login != null && options.Value.ApiKey != null)
                call.Request.SetQueryParam("login", login).SetQueryParam("api_key", apiKey);

            if (delay > TimeSpan.Zero)
                await Throttler.Get("danbooru").UseAsync(delay);
        };
        
        settings.AfterCall = _ => Throttler.Get("danbooru").Release();
    }
}
