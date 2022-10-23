using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;

public class DanbooruApiLoaderFixture
{
    private IBooruApiLoader? _danbooruWithAuth;
    private IBooruApiLoader? _danbooruWithoutAuth;
    private IBooruApiAccessor? _danbooruApiAccessor;

    public IBooruApiLoader GetLoaderWithAuth()
        => _danbooruWithAuth ??=
            new DanbooruApiLoader(new PerBaseUrlFlurlClientFactory(), Options.Create(new DanbooruSettings()
            {
                ApiKey = "t77cOKpOMV5I4HN3r3gfOooG5hrh3sAqgsD_YDQCZGc",
                Login = "testuser159",
                PauseBetweenRequestsInMs = 1240
            }));

    public IBooruApiLoader GetLoaderWithoutAuth()
        => _danbooruWithoutAuth ??=
            new DanbooruApiLoader(new PerBaseUrlFlurlClientFactory(), Options.Create(new DanbooruSettings()
            {
                PauseBetweenRequestsInMs = 0
            }));

    public IBooruApiAccessor GetApiAccessorWithAuth()
        => _danbooruApiAccessor ??= new DanbooruApiLoader(new PerBaseUrlFlurlClientFactory(), Options.Create(
            new DanbooruSettings()
            {
                ApiKey = "t77cOKpOMV5I4HN3r3gfOooG5hrh3sAqgsD_YDQCZGc",
                Login = "testuser159",
                PauseBetweenRequestsInMs = 1240
            }));
}
