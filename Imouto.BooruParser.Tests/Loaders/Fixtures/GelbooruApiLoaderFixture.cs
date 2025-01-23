using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Gelbooru;
using Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures;

public class GelbooruApiLoaderFixture
{
    private IBooruApiLoader? _loader;
    private readonly bool _enableCache = true;
    
    private readonly IOptions<GelbooruSettings> _options 
        = Options.Create(new GelbooruSettings { PauseBetweenRequestsInMs = 0 });

    private IFlurlClientCache Factory =>
        _enableCache
            ? new FlurlClientCache().WithDefaults(x => x.AddMiddleware(() => new HardCachingHttpMessageHandler()))
            : new FlurlClientCache();

    public IBooruApiLoader GetLoader() => _loader ??= new GelbooruApiLoader(Factory, _options);
}
