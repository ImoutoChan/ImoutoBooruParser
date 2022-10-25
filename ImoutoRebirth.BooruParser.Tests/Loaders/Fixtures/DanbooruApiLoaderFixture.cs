using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations.Danbooru;
using ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;

public class DanbooruApiLoaderFixture
{
    private IBooruApiLoader? _danbooruWithAuth;
    private IBooruApiLoader? _danbooruWithoutAuth;
    private IBooruApiAccessor? _danbooruApiAccessor;
    private readonly bool _enableCache = true;

    private readonly IOptions<DanbooruSettings> _options 
        = Options.Create(new DanbooruSettings { PauseBetweenRequestsInMs = 0 });
    
    private readonly IOptions<DanbooruSettings> _authorizedOptions = Options.Create(
        new DanbooruSettings()
        {
            ApiKey = "t77cOKpOMV5I4HN3r3gfOooG5hrh3sAqgsD_YDQCZGc",
            Login = "testuser159",
            PauseBetweenRequestsInMs = 1
        });

    private IFlurlClientFactory Factory =>
        _enableCache ? new HardCachePerBaseUrlFlurlClientFactory() : new PerBaseUrlFlurlClientFactory();

    public IBooruApiLoader GetLoaderWithAuth()
        => _danbooruWithAuth ??= new DanbooruApiLoader(Factory, _authorizedOptions);

    public IBooruApiLoader GetLoaderWithoutAuth()
        => _danbooruWithoutAuth ??= new DanbooruApiLoader(Factory, _options);

    public IBooruApiAccessor GetApiAccessorWithAuth()
        => _danbooruApiAccessor ??= new DanbooruApiLoader(Factory, _authorizedOptions);
}
