using System.Text.Json.Serialization;

namespace Imouto.BooruParser.Implementations.Yandere;

public record YanderePost(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("tags")] string Tags,
    [property: JsonPropertyName("created_at")] int CreatedAt,
    [property: JsonPropertyName("creator_id")] int? CreatorId,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("file_size")] int FileSize,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("sample_url")] string? SampleUrl,
    [property: JsonPropertyName("jpeg_url")] string JpegUrl,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("parent_id")] int? ParentId,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("last_noted_at")] int LastNotedAt
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
