namespace Imouto.BooruParser.Implementations.Gelbooru;

public record GelbooruSettings
{
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);
}
