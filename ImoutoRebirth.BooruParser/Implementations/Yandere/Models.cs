using System.Text.Json.Serialization;

namespace ImoutoRebirth.BooruParser.Implementations.Yandere;

public record YandereChild(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string Md5
);

public record YandereMediaMetadata(
    [property: JsonPropertyName("metadata")] YandereMetadata Metadata
);

public record YandereMetadata(
    [property: JsonPropertyName("Ugoira:FrameDelays")] IReadOnlyCollection<int>? UgoiraFrameDelays
);

public record YandereNote(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("body")] string Body
);

public record YandereParent(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string Md5
);

/// <summary>
/// only=id,md5,tag_string,is_banned,is_deleted
/// </summary>
public record YanderePostPreview(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string? Md5,
    [property: JsonPropertyName("tag_string")] string TagString,
    [property: JsonPropertyName("is_banned")] bool IsBanned,
    [property: JsonPropertyName("is_deleted")] bool IsDeleted
);

public record YanderePost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("tags")] string Tags,
    [property: JsonPropertyName("created_at")] int CreatedAt,
    [property: JsonPropertyName("updated_at")] int UpdatedAt,
    [property: JsonPropertyName("creator_id")] int CreatorId,
    [property: JsonPropertyName("approver_id")] object ApproverId,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("change")] int Change,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("file_size")] int FileSize,
    [property: JsonPropertyName("file_ext")] string FileExt,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("is_shown_in_index")] bool IsShownInIndex,
    [property: JsonPropertyName("preview_url")] string PreviewUrl,
    [property: JsonPropertyName("preview_width")] int PreviewWidth,
    [property: JsonPropertyName("preview_height")] int PreviewHeight,
    [property: JsonPropertyName("actual_preview_width")] int ActualPreviewWidth,
    [property: JsonPropertyName("actual_preview_height")] int ActualPreviewHeight,
    [property: JsonPropertyName("sample_url")] string? SampleUrl,
    [property: JsonPropertyName("sample_width")] int SampleWidth,
    [property: JsonPropertyName("sample_height")] int SampleHeight,
    [property: JsonPropertyName("sample_file_size")] int SampleFileSize,
    [property: JsonPropertyName("jpeg_url")] string JpegUrl,
    [property: JsonPropertyName("jpeg_width")] int JpegWidth,
    [property: JsonPropertyName("jpeg_height")] int JpegHeight,
    [property: JsonPropertyName("jpeg_file_size")] int JpegFileSize,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("is_rating_locked")] bool IsRatingLocked,
    [property: JsonPropertyName("has_children")] bool HasChildren,
    [property: JsonPropertyName("parent_id")] int? ParentId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("is_pending")] bool IsPending,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("is_held")] bool IsHeld,
    [property: JsonPropertyName("frames_pending_string")] string FramesPendingString,
    [property: JsonPropertyName("frames_pending")] IReadOnlyList<object> FramesPending,
    [property: JsonPropertyName("frames_string")] string FramesString,
    [property: JsonPropertyName("frames")] IReadOnlyList<object> Frames,
    [property: JsonPropertyName("is_note_locked")] bool IsNoteLocked,
    [property: JsonPropertyName("last_noted_at")] int LastNotedAt,
    [property: JsonPropertyName("last_commented_at")] int LastCommentedAt
);

public record YandereUploader(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);

public record YanderePool(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("posts")] YanderePoolPost[] Posts
);

public record YanderePoolPost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("md5")] string Md5
);

public record YandereTagsHistoryEntry(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("post_id")] int PostId,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("parent_id")] int? ParentId,
    [property: JsonPropertyName("parent_changed")] bool ParentChanged
);

public record YandereNotesHistoryEntry(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("post_id")] int PostId,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt
);
