using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Sankaku;
using Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures;

public class SankakuLoaderFixture
{
    private IBooruApiLoader<string>? _withAuth;
    private IBooruApiLoader<string>? _withoutAuth;
    private IBooruApiAccessor<string>? _apiAccessor;
    private readonly bool _enableCache = false;

    private IFlurlClientCache Factory =>
        _enableCache
            ? new FlurlClientCache().WithDefaults(x => x.AddMiddleware(() => new HardCachingHttpMessageHandler()))
            : new FlurlClientCache();
    
    private readonly IOptions<SankakuSettings> _authorizedOptions = Options.Create(
        new SankakuSettings
        {
            PauseBetweenRequestsInMs = 1,
            Login = "testuser159",
            Password = "testuser159"
        });
    
    private readonly IOptions<SankakuSettings> _options 
        = Options.Create(new SankakuSettings { PauseBetweenRequestsInMs = 0 });

    public IBooruApiLoader<string> GetLoaderWithAuth()
        => _withAuth ??= new SankakuApiLoader(
            Factory, 
            _authorizedOptions,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), _authorizedOptions, Factory));

    public IBooruApiAccessor<string> GetAccessorWithAuth()
        => _apiAccessor ??= new SankakuApiLoader(
            Factory, 
            _authorizedOptions,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), _authorizedOptions, Factory));

    public IBooruApiLoader<string> GetLoaderWithoutAuth()
        => _withoutAuth ??= new SankakuApiLoader(
            Factory, 
            _options,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), _options, Factory));
}
