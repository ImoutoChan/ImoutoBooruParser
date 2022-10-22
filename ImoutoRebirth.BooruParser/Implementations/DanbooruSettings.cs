namespace ImoutoRebirth.BooruParser.Implementations;

public record DanbooruSettings
{
    public string? Login { get; set; }
    
    public string? ApiKey { get; set; }
    
    public int PauseBetweenRequestsInMs { get; set; } = 0;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);
}
