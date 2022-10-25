﻿using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations.Yandere;
using ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures.HttpCache;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;

public class YandereApiLoaderFixture
{
    private IBooruApiLoader? _loader;
    private IBooruApiAccessor? _apiAccessor;
    private readonly bool _enableCache = false;

    private readonly IOptions<YandereSettings> _authorizedOptions = Options.Create(
        new YandereSettings()
        {
            PasswordHash = "5eedf880498cac52bbfc8386150682d54174fab0",
            Login = "testuser1",
            PauseBetweenRequestsInMs = 1
        });
    
    private readonly IOptions<YandereSettings> _options 
        = Options.Create(new YandereSettings { PauseBetweenRequestsInMs = 0 });

    private IFlurlClientFactory Factory =>
        _enableCache ? new HardCachePerBaseUrlFlurlClientFactory() : new PerBaseUrlFlurlClientFactory();

    public IBooruApiLoader GetLoader() => _loader ??= new YandereApiLoader(Factory, _options);

    public IBooruApiAccessor GetApiAccessorWithAuth()
        => _apiAccessor ??= new YandereApiLoader(Factory, _authorizedOptions);
}
