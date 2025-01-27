using Imouto.BooruParser.Implementations.Sankaku;

namespace Imouto.BooruParser;

public static class BooruApiLoaderExtensions
{
    public static Task<Post> GetPostAsync(this IBooruApiLoader loader, int postId)
    {
        if (loader is SankakuApiLoader)
            throw new InvalidOperationException("Sankaku doesn't support getting post by int id");

        return loader.GetPostAsync(postId.ToString());
    }

    public static Task FavoritePostAsync(this IBooruApiAccessor loader, int postId)
    {
        if (loader is SankakuApiLoader)
            throw new InvalidOperationException("Sankaku doesn't support fav post by int id");

        return loader.FavoritePostAsync(postId.ToString());
    }

    public static int GetIntId(this PostIdentity postIdentity)
    {
        if (int.TryParse(postIdentity.Id, out var intId))
            return intId;

        throw new InvalidOperationException("This is probably is sankaku post, which doesn't support int id");
    }
}

public interface IBooruApiLoader
{
    Task<Post> GetPostAsync(string postId);
    
    Task<Post?> GetPostByMd5Async(string md5);

    Task<SearchResult> SearchAsync(string tags);
    Task<SearchResult> GetNextPageAsync(SearchResult results);
    Task<SearchResult> GetPreviousPageAsync(SearchResult results);

    Task<SearchResult> GetPopularPostsAsync(PopularType type);

    Task<HistorySearchResult<TagHistoryEntry>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default);

    Task<HistorySearchResult<NoteHistoryEntry>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default);
}

public interface IBooruApiAccessor
{
    Task FavoritePostAsync(string postId);
}

/// <param name="Page">For danbooru: b{lowest-history-id-on-current-page}</param>
public record SearchToken(string Page);

public record SearchResult(IReadOnlyCollection<PostPreview> Results, string SearchTags, int PageNumber);

public record HistorySearchResult<T>(
    IReadOnlyCollection<T> Results,
    SearchToken? NextToken);

public record PostPreview(string Id, string? Md5Hash, string Title, bool IsBanned, bool IsDeleted);

/// <summary>
/// OriginalUrl, SampleUrl and PostIdentity.Md5 are nulls when post is banned
/// </summary>
public record Post(
    PostIdentity Id,
    string? OriginalUrl,
    string? SampleUrl,
    string? PreviewUrl,
    ExistState ExistState,
    DateTimeOffset PostedAt,
    Uploader UploaderId,
    string? Source,
    Size FileResolution,
    int FileSizeInBytes,
    Rating Rating,
    RatingSafeLevel RatingSafeLevel,
    IReadOnlyCollection<int> UgoiraFrameDelays,
    PostIdentity? Parent,
    IReadOnlyCollection<PostIdentity> ChildrenIds,
    IReadOnlyCollection<Pool> Pools,
    IReadOnlyCollection<Tag> Tags,
    IReadOnlyCollection<Note> Notes);


public enum ExistState { Exist, MarkDeleted, Deleted }

public enum PopularType { Day, Week, Month }

public enum Rating { Safe, Questionable, Explicit }

public enum RatingSafeLevel { None, Sensitive, General }

public record Pool(string Id, string Name, int Position);

public record Note(string Id, string Text, Position Point, Size Size);

public record Tag(string Type, string Name);

public record PostIdentity(string Id, string Md5Hash);

public record Uploader(string Id, string Name);

public record struct Position(int Top, int Left);

public record struct Size(int Width, int Height);

public record TagHistoryEntry(
    int HistoryId,
    DateTimeOffset UpdatedAt,
    string PostId,
    string? ParentId,
    bool ParentChanged);

public record NoteHistoryEntry(int HistoryId, string PostId, DateTimeOffset UpdatedAt);
