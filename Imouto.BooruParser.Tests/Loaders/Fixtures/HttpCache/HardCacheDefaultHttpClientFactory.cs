using Flurl.Http.Configuration;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures.HttpCache;

public class HardCacheDefaultHttpClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler() 
        => new HardCachingHttpMessageHandler(base.CreateMessageHandler());
}
