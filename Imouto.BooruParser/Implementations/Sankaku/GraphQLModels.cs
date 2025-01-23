using System.Text.Json.Serialization;

namespace Imouto.BooruParser.Implementations.Sankaku;

public record Data(
    [property: JsonPropertyName("postTagHistoryConnection")] PostTagHistoryConnection? PostTagHistoryConnection
);

public record Edge(
    [property: JsonPropertyName("node")] Node Node
);

public record Node(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("post")] SankakuTagHistoryDocumentPost Post,
    [property: JsonPropertyName("parent")] string? Parent,
    [property: JsonPropertyName("createdAt")] string CreatedAt
);

public record PageInfo(
    [property: JsonPropertyName("hasNextPage")] bool HasNextPage,
    [property: JsonPropertyName("endCursor")] string EndCursor
);

public record SankakuTagHistoryDocumentPost(
    [property: JsonPropertyName("id")] string Id
);

public record PostTagHistoryConnection(
    [property: JsonPropertyName("pageInfo")] PageInfo PageInfo,
    [property: JsonPropertyName("edges")] IReadOnlyList<Edge> Edges
);

public record SankakuTagHistoryDocument(
    [property: JsonPropertyName("data")] Data Data
);
