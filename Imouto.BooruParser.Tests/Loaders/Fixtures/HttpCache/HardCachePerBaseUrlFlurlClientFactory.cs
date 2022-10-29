using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures.HttpCache;

public class HardCachePerBaseUrlFlurlClientFactory : PerBaseUrlFlurlClientFactory
{
    protected override IFlurlClient Create(Url url)
    {
        var client = base.Create(url);
        client.Settings.HttpClientFactory = new HardCacheDefaultHttpClientFactory();
        return client;
    }
}
