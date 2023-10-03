namespace Imouto.BooruParser.Implementations.Sankaku;

public record SankakuSettings
{
    public string? Login { get; set; }
    
    public string? Password { get; set; }
    
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);
}

public record Tokens(string? AccessToken, string? RefreshToken);
