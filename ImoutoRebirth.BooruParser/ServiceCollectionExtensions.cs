using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations.Danbooru;
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
    public static IServiceCollection AddBooruParser(this IServiceCollection services)
    {
        services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
        
        services.AddTransient<IBooruApiLoader, DanbooruApiLoader>();
        services.AddTransient<IBooruApiLoader, YandereApiLoader>();
        services.AddTransient<IBooruApiLoader, SankakuApiLoader>();
        services.AddTransient<IBooruApiAccessor, DanbooruApiLoader>();
        services.AddTransient<IBooruApiAccessor, YandereApiLoader>();
        services.AddTransient<IBooruApiAccessor, SankakuApiLoader>();
        
        services.AddTransient<ISankakuAuthManager, SankakuAuthManager>();

        return services;
    }
}
