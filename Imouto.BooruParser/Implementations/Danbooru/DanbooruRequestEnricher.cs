using Flurl.Http;

namespace Imouto.BooruParser.Implementations.Danbooru;

public static class DanbooruRequestEnricher
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36";
    
    public static IFlurlRequest WithUserAgent(this IFlurlRequest request)
    {
        request.WithHeader("User-Agent", UserAgent);
        return request;
    }
}
