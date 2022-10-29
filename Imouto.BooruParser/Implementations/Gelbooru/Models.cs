using System.Text.Json.Serialization;

namespace Imouto.BooruParser.Implementations.Gelbooru;

public record GelbooruPost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("creator_id")] int CreatorId,
    [property: JsonPropertyName("parent_id")] int ParentId,
    [property: JsonPropertyName("tags")] string Tags,
    [property: JsonPropertyName("has_notes")] string HasNotes,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("sample_url")] string? SampleUrl
);

public record GelbooruPostPage(
    [property: JsonPropertyName("post")] IReadOnlyCollection<GelbooruPost>? Posts
);
