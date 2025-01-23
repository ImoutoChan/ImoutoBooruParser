namespace Imouto.BooruParser;

public interface IBooruApiLoader : IBooruApiLoader<int>
{
}

public interface IBooruApiLoader<TId>
{
    Task<Post<TId>> GetPostAsync(TId postId);
    
    Task<Post<TId>?> GetPostByMd5Async(string md5);

    Task<SearchResult<TId>> SearchAsync(string tags);

    Task<SearchResult<TId>> GetPopularPostsAsync(PopularType type);

    Task<HistorySearchResult<TagHistoryEntry<TId>>> GetTagHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default);

    Task<HistorySearchResult<NoteHistoryEntry<TId>>> GetNoteHistoryPageAsync(
        SearchToken? token,
        int limit = 100,
        CancellationToken ct = default);
}

public interface IBooruApiAccessor : IBooruApiAccessor<int>
{
}

public interface IBooruApiAccessor<in TId>
{
    Task FavoritePostAsync(TId postId);
}

/// <param name="Page">For danbooru: b{lowest-history-id-on-current-page}</param>
public record SearchToken(string Page);

public record SearchResult<TId>(IReadOnlyCollection<PostPreview<TId>> Results);

public record HistorySearchResult<T>(
    IReadOnlyCollection<T> Results,
    SearchToken? NextToken);

public record PostPreview<TId>(TId Id, string? Md5Hash, string Title, bool IsBanned, bool IsDeleted);

/// <summary>
/// OriginalUrl, SampleUrl and PostIdentity.Md5 are nulls when post is banned
/// </summary>
public record Post<TId>(
    PostIdentity<TId> Id,
    string? OriginalUrl,
    string? SampleUrl,
    string? PreviewUrl,
    ExistState ExistState,
    DateTimeOffset PostedAt,
    Uploader<TId> UploaderId,
    string? Source,
    Size FileResolution,
    int FileSizeInBytes,
    Rating Rating,
    RatingSafeLevel RatingSafeLevel,
    IReadOnlyCollection<int> UgoiraFrameDelays,
    PostIdentity<TId>? Parent,
    IReadOnlyCollection<PostIdentity<TId>> ChildrenIds,
    IReadOnlyCollection<Pool<TId>> Pools,
    IReadOnlyCollection<Tag> Tags,
    IReadOnlyCollection<Note<TId>> Notes);

public record Post(
    PostIdentity<int> Id,
    string? OriginalUrl,
    string? SampleUrl,
    string? PreviewUrl,
    ExistState ExistState,
    DateTimeOffset PostedAt,
    Uploader<int> UploaderId,
    string? Source,
    Size FileResolution,
    int FileSizeInBytes,
    Rating Rating,
    RatingSafeLevel RatingSafeLevel,
    IReadOnlyCollection<int> UgoiraFrameDelays,
    PostIdentity<int>? Parent,
    IReadOnlyCollection<PostIdentity<int>> ChildrenIds,
    IReadOnlyCollection<Pool<int>> Pools,
    IReadOnlyCollection<Tag> Tags,
    IReadOnlyCollection<Note<int>> Notes) : Post<int>(Id, OriginalUrl, SampleUrl, PreviewUrl, ExistState, PostedAt,
    UploaderId, Source, FileResolution, FileSizeInBytes, Rating, RatingSafeLevel, UgoiraFrameDelays, Parent,
    ChildrenIds, Pools, Tags, Notes);


public enum ExistState { Exist, MarkDeleted, Deleted }

public enum PopularType { Day, Week, Month }

public enum Rating { Safe, Questionable, Explicit }

public enum RatingSafeLevel { None, Sensitive, General }

public record Pool<TId>(TId Id, string Name, int Position);

public record Note<TId>(TId Id, string Text, Position Point, Size Size);

public record Tag(string Type, string Name);

public record PostIdentity<TId>(TId Id, string Md5Hash);

public record Uploader<TId>(TId Id, string Name);

public record struct Position(int Top, int Left);

public record struct Size(int Width, int Height);

public record TagHistoryEntry<TId>(
    int HistoryId,
    DateTimeOffset UpdatedAt,
    TId PostId,
    string? ParentId,
    bool ParentChanged);

public record NoteHistoryEntry<TId>(int HistoryId, TId PostId, DateTimeOffset UpdatedAt);
