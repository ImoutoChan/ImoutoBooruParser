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
        = Options.Create(new GelbooruSettings
        {
            PauseBetweenRequestsInMs = 0,
            UserId = 1740518,
            ApiKey = "e975ef828d4789449b469a36fabd60afe981a4de010fa40922b4e42adbdf22d052a4ef41fc4d767a59bb04121c79b411eea020707084433bf2b05c23597c97c2"
        });

    private IFlurlClientCache Factory =>
        _enableCache
            ? new FlurlClientCache().WithDefaults(x => x.AddMiddleware(() => new HardCachingHttpMessageHandler()))
            : new FlurlClientCache();

    public IBooruApiLoader GetLoader() => _loader ??= new GelbooruApiLoader(Factory, _options);
}
