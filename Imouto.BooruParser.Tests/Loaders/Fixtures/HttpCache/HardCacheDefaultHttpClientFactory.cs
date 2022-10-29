using Flurl.Http.Configuration;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;

public class HardCacheDefaultHttpClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler() 
        => new HardCachingHttpMessageHandler(base.CreateMessageHandler());
}
