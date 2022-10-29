using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations.Danbooru;
using ImoutoRebirth.BooruParser.Implementations.Gelbooru;
using ImoutoRebirth.BooruParser.Implementations.Sankaku;
using ImoutoRebirth.BooruParser.Implementations.Yandere;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.BooruParser;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Should:
    ///     Add memory cache
    ///     Configure SankakuSettings, YandereSettings, DanbooruSettings
    /// </summary>
    public static IServiceCollection AddBooruParsers(this IServiceCollection services)
    {
        services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
        
        services.AddTransient<IBooruApiLoader, DanbooruApiLoader>();
        services.AddTransient<IBooruApiLoader, YandereApiLoader>();
        services.AddTransient<IBooruApiLoader, SankakuApiLoader>();
        services.AddTransient<IBooruApiLoader, GelbooruApiLoader>();
        
        services.AddTransient<DanbooruApiLoader>();
        services.AddTransient<YandereApiLoader>();
        services.AddTransient<SankakuApiLoader>();
        services.AddTransient<GelbooruApiLoader>();
        
        services.AddTransient<IBooruApiAccessor, DanbooruApiLoader>();
        services.AddTransient<IBooruApiAccessor, YandereApiLoader>();
        services.AddTransient<IBooruApiAccessor, SankakuApiLoader>();
        
        services.AddTransient<ISankakuAuthManager, SankakuAuthManager>();

        return services;
    }
}
