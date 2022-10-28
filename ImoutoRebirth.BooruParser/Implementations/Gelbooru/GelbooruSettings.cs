namespace ImoutoRebirth.BooruParser.Implementations.Gelbooru;

public record GelbooruSettings
{
    public int? UserId { get; set; }
    
    public string? ApiKey { get; set; }
    
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);
}
