using Flurl.Http;

namespace Imouto.BooruParser.Implementations.Danbooru;

public static class DanbooruRequestEnricher
{
    public static IFlurlRequest WithUserAgent(this IFlurlRequest request, string botUserAgent)
    {
        request.WithHeader("User-Agent", botUserAgent);
        return request;
    }
}
