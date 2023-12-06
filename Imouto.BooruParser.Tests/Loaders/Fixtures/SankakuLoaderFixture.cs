using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Sankaku;
using Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures;

public class SankakuLoaderFixture
{
    private IBooruApiLoader? _withAuth;
    private IBooruApiLoader? _withoutAuth;
    private IBooruApiAccessor? _apiAccessor;
    private readonly bool _enableCache = false;

    private IFlurlClientFactory Factory =>
        _enableCache ? new HardCachePerBaseUrlFlurlClientFactory() : new PerBaseUrlFlurlClientFactory();
    
    private readonly IOptions<SankakuSettings> _authorizedOptions = Options.Create(
        new SankakuSettings
        {
            PauseBetweenRequestsInMs = 1,
            Login = "testuser159",
            Password = "testuser159"
        });
    
    private readonly IOptions<SankakuSettings> _options 
        = Options.Create(new SankakuSettings { PauseBetweenRequestsInMs = 0 });

    public IBooruApiLoader GetLoaderWithAuth()
        => _withAuth ??= new SankakuApiLoader(
            Factory, 
            _authorizedOptions,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), _authorizedOptions, Factory));

    public IBooruApiAccessor GetAccessorWithAuth()
        => _apiAccessor ??= new SankakuApiLoader(
            Factory, 
            _authorizedOptions,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), _authorizedOptions, Factory));

    public IBooruApiLoader GetLoaderWithoutAuth()
        => _withoutAuth ??= new SankakuApiLoader(
            Factory, 
            _options,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), _options, Factory));
}
