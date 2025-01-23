using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Yandere;
using Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures;

public class YandereApiLoaderFixture
{
    private IBooruApiLoader<int>? _loader;
    private IBooruApiAccessor<int>? _apiAccessor;
    private readonly bool _enableCache = true;

    private readonly IOptions<YandereSettings> _authorizedOptions = Options.Create(
        new YandereSettings()
        {
            PasswordHash = "5eedf880498cac52bbfc8386150682d54174fab0",
            Login = "testuser1",
            PauseBetweenRequestsInMs = 0
        });
    
    private readonly IOptions<YandereSettings> _options 
        = Options.Create(new YandereSettings { PauseBetweenRequestsInMs = 0 });

    private IFlurlClientCache Factory =>
        _enableCache
            ? new FlurlClientCache().WithDefaults(x => x.AddMiddleware(() => new HardCachingHttpMessageHandler()))
            : new FlurlClientCache();

    public IBooruApiLoader<int> GetLoader() => _loader ??= new YandereApiLoader(Factory, _options);

    public IBooruApiAccessor<int> GetApiAccessorWithAuth()
        => _apiAccessor ??= new YandereApiLoader(Factory, _authorizedOptions);
}
