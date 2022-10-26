using System.Text.Json.Serialization;

namespace ImoutoRebirth.BooruParser.Implementations.Sankaku;

public record SankakuChild(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string Md5
);

public record SankakuMediaMetadata(
    [property: JsonPropertyName("metadata")] SankakuMetadata Metadata
);

public record SankakuMetadata(
    [property: JsonPropertyName("Ugoira:FrameDelays")] IReadOnlyCollection<int>? UgoiraFrameDelays
);

public record SankakuNote(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("created_at")] SankakuDateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] SankakuDateTime UpdatedAt,
    [property: JsonPropertyName("creator_id")] int CreatorId,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("post_id")] int PostId,
    [property: JsonPropertyName("body")] string Body
);

public record SankakuParent(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string Md5
);

/// <summary>
/// only=id,md5,tag_string,is_banned,is_deleted
/// </summary>
public record SankakuPostPreview(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string? Md5,
    [property: JsonPropertyName("tag_string")] string TagString,
    [property: JsonPropertyName("is_banned")] bool IsBanned,
    [property: JsonPropertyName("is_deleted")] bool IsDeleted
);

public record SankakuPost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("author")] SankakuAuthor Author,
    [property: JsonPropertyName("sample_url")] string SampleUrl,
    [property: JsonPropertyName("sample_width")] int SampleWidth,
    [property: JsonPropertyName("sample_height")] int SampleHeight,
    [property: JsonPropertyName("preview_url")] string? PreviewUrl,
    [property: JsonPropertyName("preview_width")] int PreviewWidth,
    [property: JsonPropertyName("preview_height")] int PreviewHeight,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("file_size")] int FileSize,
    [property: JsonPropertyName("file_type")] string FileType,
    [property: JsonPropertyName("created_at")] SankakuDateTime CreatedAt,
    [property: JsonPropertyName("has_children")] bool HasChildren,
    [property: JsonPropertyName("has_comments")] bool HasComments,
    [property: JsonPropertyName("has_notes")] bool HasNotes,
    [property: JsonPropertyName("is_favorited")] bool IsFavorited,
    [property: JsonPropertyName("user_vote")] object UserVote,
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("parent_id")] int? ParentId,
    [property: JsonPropertyName("change")] int Change,
    [property: JsonPropertyName("fav_count")] int FavCount,
    [property: JsonPropertyName("recommended_posts")] int RecommendedPosts,
    [property: JsonPropertyName("recommended_score")] int RecommendedScore,
    [property: JsonPropertyName("vote_count")] int VoteCount,
    [property: JsonPropertyName("total_score")] int TotalScore,
    [property: JsonPropertyName("comment_count")] int CommentCount,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("in_visible_pool")] bool InVisiblePool,
    [property: JsonPropertyName("is_premium")] bool IsPremium,
    [property: JsonPropertyName("is_rating_locked")] bool IsRatingLocked,
    [property: JsonPropertyName("is_note_locked")] bool IsNoteLocked,
    [property: JsonPropertyName("is_status_locked")] bool IsStatusLocked,
    [property: JsonPropertyName("redirect_to_signup")] bool RedirectToSignup,
    [property: JsonPropertyName("sequence")] object Sequence,
    [property: JsonPropertyName("tags")] IReadOnlyList<SankakuTag> Tags,
    [property: JsonPropertyName("video_duration")] object VideoDuration
);

public record SankakuDateTime(
    [property: JsonPropertyName("json_class")] string JsonClass,
    [property: JsonPropertyName("s")] int S,
    [property: JsonPropertyName("n")] int N
);

public record SankakuAuthor(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("avatar")] string Avatar,
    [property: JsonPropertyName("avatar_rating")] string AvatarRating
);

public record SankakuTag(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name_en")] string NameEn,
    [property: JsonPropertyName("name_ja")] string NameJa,
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("post_count")] int PostCount,
    [property: JsonPropertyName("pool_count")] int PoolCount,
    [property: JsonPropertyName("locale")] string Locale,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("version")] int? Version,
    [property: JsonPropertyName("tagName")] string TagName,
    [property: JsonPropertyName("total_post_count")] int TotalPostCount,
    [property: JsonPropertyName("total_pool_count")] int TotalPoolCount,
    [property: JsonPropertyName("name")] string Name
);

public record SankakuUploader(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);

public record SankakuPostPool(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);
public record SankakuPool(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("posts")] SankakuPoolPost[] Posts
);

public record SankakuPoolPost(
    [property: JsonPropertyName("id")] int Id
);

public record SankakuTagsHistoryEntry(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("post_id")] int PostId,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("parent_id")] int? ParentId,
    [property: JsonPropertyName("parent_changed")] bool ParentChanged
);

public record SankakuNotesHistoryEntry(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("post_id")] int PostId,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt
);

public record SankakuRefreshResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
