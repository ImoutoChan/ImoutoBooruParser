using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Implementations.Gelbooru;
using Imouto.BooruParser.Implementations.Rule34;
using Imouto.BooruParser.Implementations.Sankaku;
using Imouto.BooruParser.Implementations.Yandere;
using Microsoft.Extensions.DependencyInjection;

namespace Imouto.BooruParser;

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
        services.AddTransient<IBooruApiLoader, Rule34ApiLoader>();
        
        services.AddTransient<DanbooruApiLoader>();
        services.AddTransient<YandereApiLoader>();
        services.AddTransient<SankakuApiLoader>();
        services.AddTransient<GelbooruApiLoader>();
        services.AddTransient<Rule34ApiLoader>();
        
        services.AddTransient<IBooruApiAccessor, DanbooruApiLoader>();
        services.AddTransient<IBooruApiAccessor, YandereApiLoader>();
        services.AddTransient<IBooruApiAccessor, SankakuApiLoader>();
        
        services.AddTransient<ISankakuAuthManager, SankakuAuthManager>();

        return services;
    }
}
