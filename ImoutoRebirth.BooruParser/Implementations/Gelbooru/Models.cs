using System.Text.Json.Serialization;

namespace ImoutoRebirth.BooruParser.Implementations.Gelbooru;

public record GelbooruPost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("change")] int Change,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("creator_id")] int CreatorId,
    [property: JsonPropertyName("parent_id")] int ParentId,
    [property: JsonPropertyName("sample")] int Sample,
    [property: JsonPropertyName("preview_height")] int PreviewHeight,
    [property: JsonPropertyName("preview_width")] int PreviewWidth,
    [property: JsonPropertyName("tags")] string Tags,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("has_notes")] string HasNotes,
    [property: JsonPropertyName("has_comments")] string HasComments,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("preview_url")] string PreviewUrl,
    [property: JsonPropertyName("sample_url")] string SampleUrl,
    [property: JsonPropertyName("sample_height")] int SampleHeight,
    [property: JsonPropertyName("sample_width")] int SampleWidth,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("post_locked")] int PostLocked,
    [property: JsonPropertyName("has_children")] string HasChildren
);

public record GelbooruPostPage(
    [property: JsonPropertyName("post")] IReadOnlyCollection<GelbooruPost> Posts
);
