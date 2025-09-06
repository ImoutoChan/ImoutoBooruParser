namespace Imouto.BooruParser.Implementations.Danbooru;

public record DanbooruSettings
{
    public string? Login { get; set; }
    
    public string? ApiKey { get; set; }
    
    public int PauseBetweenRequestsInMs { get; set; } = 1;
    
    public required string? BotUserAgent { get; set; }

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);
}
