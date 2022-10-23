namespace ImoutoRebirth.BooruParser;

public interface IBooruApiLoader
{
    Task<Post> GetPostAsync(int postId);
    
    Task<Post?> GetPostByMd5Async(string md5);

    Task<SearchResult> SearchAsync(string tags);

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
    Task FavoritePostAsync(int postId);
}

/// <param name="Page">For danbooru: b{lowest-history-id-on-current-page}</param>
public record SearchToken(string Page);

public record SearchResult(IReadOnlyCollection<PostPreview> Results)
{
    public bool IsFound => Results.Any();
}

public record HistorySearchResult<T>(
    IReadOnlyCollection<T> Results, 
    SearchToken? NextToken)
{
    public bool IsFound => Results.Any();
}

public record PostPreview(int Id, string? Md5Hash, string Title, bool IsBanned, bool IsDeleted);

/// <summary>
/// OriginalUrl, SampleUrl and PostIdentity.Md5 are nulls when post is banned
/// </summary>
public record Post(
    PostIdentity Id,
    string? OriginalUrl,
    string? SampleUrl,
    ExistState ExistState,
    DateTimeOffset PostedAt,
    Uploader UploaderId,
    string Source,
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

public record Pool(int Id, string Name, int Position);

public record Note(int Id, string Text, Position Point, Size Size);

public record Tag(string Type, string Name);

public record PostIdentity(int Id, string Md5Hash);

public record Uploader(int Id, string Name);

public record struct Position(int Top, int Left);

public record struct Size(int Width, int Height);

public record TagHistoryEntry(
    int HistoryId,
    DateTimeOffset UpdatedAt,
    int PostId,
    int? ParentId,
    bool ParentChanged);

public record NoteHistoryEntry(int HistoryId, int PostId, DateTimeOffset UpdatedAt);
