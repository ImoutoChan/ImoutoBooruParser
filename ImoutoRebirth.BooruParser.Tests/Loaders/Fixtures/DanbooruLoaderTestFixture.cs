using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using ImoutoRebirth.BooruParser.Implementations;
using ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;

public class DanbooruApiLoaderFixture
{
    private IBooruApiLoader? _danbooruWithAuth;
    private IBooruApiLoader? _danbooruWithoutAuth;
    private IBooruApiAccessor? _danbooruApiAccessor;

    private readonly IOptions<DanbooruSettings> _options 
        = Options.Create(new DanbooruSettings { PauseBetweenRequestsInMs = 0 });
    
    private readonly IOptions<DanbooruSettings> _authorizedOptions = Options.Create(
        new DanbooruSettings()
        {
            ApiKey = "t77cOKpOMV5I4HN3r3gfOooG5hrh3sAqgsD_YDQCZGc",
            Login = "testuser159",
            PauseBetweenRequestsInMs = 1
        });

    private readonly IFlurlClientFactory _factory = new HardCachePerBaseUrlFlurlClientFactory();

    public IBooruApiLoader GetLoaderWithAuth()
        => _danbooruWithAuth ??= new DanbooruApiLoader(_factory, _authorizedOptions);

    public IBooruApiLoader GetLoaderWithoutAuth()
        => _danbooruWithoutAuth ??= new DanbooruApiLoader(_factory, _options);

    public IBooruApiAccessor GetApiAccessorWithAuth()
        => _danbooruApiAccessor ??= new DanbooruApiLoader(_factory, _authorizedOptions);
}
