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
        = Options.Create(new Rule34Settings
        {
            PauseBetweenRequestsInMs = 1000,
            ApiKey = "de5e28a70f698d321917df00addd4d00e60df240cf85bbc32b0cd7d49fd6853adc66d61e1fac398e04f5e55eb957cfaf142a7bd9df636854255fd0f68592406e",
            UserId = 5270091
        });

    private IFlurlClientCache Factory =>
        _enableCache
            ? new FlurlClientCache().WithDefaults(x => x.AddMiddleware(() => new HardCachingHttpMessageHandler()))
            : new FlurlClientCache();

    public IBooruApiLoader GetLoader() => _loader ??= new Rule34ApiLoader(Factory, _options);
}
