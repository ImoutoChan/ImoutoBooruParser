using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures;

public class DanbooruApiLoaderFixture
{
    private IBooruApiLoader<int>? _danbooruWithAuth;
    private IBooruApiLoader<int>? _danbooruWithoutAuth;
    private IBooruApiAccessor<int>? _danbooruApiAccessor;
    private readonly bool _enableCache = true;

    private readonly IOptions<DanbooruSettings> _options 
        = Options.Create(new DanbooruSettings 
        { 
            PauseBetweenRequestsInMs = 0,
            BotUserAgent = "UnitTestBot/1.0"
        });
    
    private readonly IOptions<DanbooruSettings> _authorizedOptions = Options.Create(
        new DanbooruSettings
        {
            ApiKey = "t77cOKpOMV5I4HN3r3gfOooG5hrh3sAqgsD_YDQCZGc",
            Login = "testuser159",
            PauseBetweenRequestsInMs = 1,
            BotUserAgent = "UnitTestBot/1.0"
        });

    private IFlurlClientCache Factory =>
        _enableCache
            ? new FlurlClientCache().WithDefaults(x => x.AddMiddleware(() => new HardCachingHttpMessageHandler()))
            : new FlurlClientCache();

    public IBooruApiLoader<int> GetLoaderWithAuth()
        => _danbooruWithAuth ??= new DanbooruApiLoader(Factory, _authorizedOptions);

    public IBooruApiLoader<int> GetLoaderWithoutAuth()
        => _danbooruWithoutAuth ??= new DanbooruApiLoader(Factory, _options);

    public IBooruApiAccessor<int> GetApiAccessorWithAuth()
        => _danbooruApiAccessor ??= new DanbooruApiLoader(Factory, _authorizedOptions);
}
