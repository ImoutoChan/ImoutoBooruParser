using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Rule34;
using Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures;

public class Rule34ApiLoaderFixture
{
    private IBooruApiLoader? _loader;
    private readonly bool _enableCache = false;
    
    private readonly IOptions<Rule34Settings> _options 
        = Options.Create(new Rule34Settings { PauseBetweenRequestsInMs = 0 });

    private IFlurlClientFactory Factory =>
        _enableCache ? new HardCachePerBaseUrlFlurlClientFactory() : new PerBaseUrlFlurlClientFactory();

    public IBooruApiLoader GetLoader() => _loader ??= new Rule34ApiLoader(Factory, _options);
}
