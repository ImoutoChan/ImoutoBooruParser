namespace Imouto.BooruParser.Implementations.Sankaku;

public record SankakuSettings
{
    public string? AccessToken { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public int PauseBetweenRequestsInMs { get; set; } = 1;

    public TimeSpan PauseBetweenRequests => TimeSpan.FromMilliseconds(PauseBetweenRequestsInMs);

    public Func<Tokens, Task> SaveTokensCallbackAsync { get; set; } = _ => Task.CompletedTask;
}

public record Tokens(string? AccessToken, string? RefreshToken);
