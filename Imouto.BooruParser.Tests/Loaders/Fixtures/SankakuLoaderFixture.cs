﻿using Flurl.Http.Configuration;
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
    private readonly bool _enableCache = true;

    private IFlurlClientFactory Factory =>
        _enableCache ? new HardCachePerBaseUrlFlurlClientFactory() : new PerBaseUrlFlurlClientFactory();
    
    private readonly IOptions<SankakuSettings> _authorizedOptions = Options.Create(
        new SankakuSettings()
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjQ3NzE1OCwic3ViTHZsIjowLCJpc3MiOiJodHRwczovL2NhcGktdjIuc2Fua2FrdWNvbXBsZXguY29tIiwidHlwZSI6IkJlYXJlciIsImF1ZCI6ImNvbXBsZXgiLCJzY29wZSI6ImNvbXBsZXgiLCJpYXQiOjE2NzIyNTk4NjQsImV4cCI6MTY3MjQzMjY2NH0.T2rf_f2CKwZizE4HHsE_zROIuiFMUGdTGmCp2pH80mA",
            RefreshToken = "AczkCpDoFoLR1_PcasAInc9Ewcd",
            SaveTokensCallbackAsync = tokens =>
            {
                Console.WriteLine($"new token: {tokens.AccessToken}, {tokens.RefreshToken}");
                return Task.CompletedTask;
            },
            PauseBetweenRequestsInMs = 1
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
