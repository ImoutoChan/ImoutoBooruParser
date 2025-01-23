using System.Text.Json.Serialization;

namespace Imouto.BooruParser.Implementations.Sankaku;

public record SankakuNote(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("created_at")] SankakuDateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] SankakuDateTime UpdatedAt,
    [property: JsonPropertyName("creator_id")] string CreatorId,
    [property: JsonPropertyName("x")] int X,
    [property: JsonPropertyName("y")] int Y,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("post_id")] string PostId,
    [property: JsonPropertyName("body")] string Body
);

public record SankakuPost(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("author")] SankakuAuthor Author,
    [property: JsonPropertyName("preview_url")] string? PreviewUrl,
    [property: JsonPropertyName("sample_url")] string? SampleUrl,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("file_size")] int FileSize,
    [property: JsonPropertyName("created_at")] SankakuDateTime CreatedAt,
    [property: JsonPropertyName("has_children")] bool HasChildren,
    [property: JsonPropertyName("has_notes")] bool HasNotes,
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("parent_id")] string? ParentId,
    [property: JsonPropertyName("tags")] IReadOnlyList<SankakuTag> Tags
);

public record SankakuDateTime(
    [property: JsonPropertyName("s")] int S
);

public record SankakuAuthor(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

public record SankakuTag(
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("tagName")] string TagName
);

public record SankakuPostPool(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);
public record SankakuPool(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("posts")] SankakuPoolPost[] Posts
);

public record SankakuPoolPost(
    [property: JsonPropertyName("id")] string Id
);

public record SankakuRefreshResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
