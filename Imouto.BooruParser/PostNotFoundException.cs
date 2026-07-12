namespace Imouto.BooruParser;

public sealed class PostNotFoundException : Exception
{
    public PostNotFoundException(string booru, string postId)
        : base($"Post '{postId}' was not found on {booru}.")
    {
        Booru = booru;
        PostId = postId;
    }

    public PostNotFoundException(string booru, string postId, Exception innerException)
        : base($"Post '{postId}' was not found on {booru}.", innerException)
    {
        Booru = booru;
        PostId = postId;
    }

    public string Booru { get; }

    public string PostId { get; }
}
