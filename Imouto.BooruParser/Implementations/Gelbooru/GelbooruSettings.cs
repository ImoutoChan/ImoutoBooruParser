namespace Imouto.BooruParser.Implementations.Gelbooru;

public record GelbooruSettings
{
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);

    public required string ApiKey { get; set; }

    public int UserId { get; set; }
}
