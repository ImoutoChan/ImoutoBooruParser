namespace Imouto.BooruParser.Implementations.Yandere;

public record YandereSettings
{
    public string? Login { get; set; }
    
    public string? PasswordHash { get; set; }
    
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);

    public string? BotUserAgent { get; set; }
}
