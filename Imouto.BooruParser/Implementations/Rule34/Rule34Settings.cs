namespace Imouto.BooruParser.Implementations.Rule34;

public record Rule34Settings
{
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);
}
