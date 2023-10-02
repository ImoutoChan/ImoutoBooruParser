using System.Text.Json.Serialization;

namespace Imouto.BooruParser.Implementations.Rule34;

public record Rule34Post(
    [property: JsonPropertyName("preview_url")] string PreviewUrl,
    [property: JsonPropertyName("sample_url")] string SampleUrl,
    [property: JsonPropertyName("file_url")] string FileUrl,
    [property: JsonPropertyName("directory")] int Directory,
    [property: JsonPropertyName("hash")] string Hash,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("change")] int Change,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("parent_id")] int ParentId,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("sample")] bool Sample,
    [property: JsonPropertyName("sample_height")] int SampleHeight,
    [property: JsonPropertyName("sample_width")] int SampleWidth,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("tags")] string Tags,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("has_notes")] bool? HasNotes,
    [property: JsonPropertyName("comment_count")] int CommentCount
);
