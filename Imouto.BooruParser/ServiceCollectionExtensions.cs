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
        services.AddSingleton<IFlurlClientCache>(_ => new FlurlClientCache());
        
        services.AddTransient<IBooruApiLoader<int>, DanbooruApiLoader>();
        services.AddTransient<IBooruApiLoader<int>, YandereApiLoader>();
        services.AddTransient<IBooruApiLoader<string>, SankakuApiLoader>();
        services.AddTransient<IBooruApiLoader<int>, GelbooruApiLoader>();
        services.AddTransient<IBooruApiLoader<int>, Rule34ApiLoader>();
        
        services.AddTransient<DanbooruApiLoader>();
        services.AddTransient<YandereApiLoader>();
        services.AddTransient<SankakuApiLoader>();
        services.AddTransient<GelbooruApiLoader>();
        services.AddTransient<Rule34ApiLoader>();
        
        services.AddTransient<IBooruApiAccessor<int>, DanbooruApiLoader>();
        services.AddTransient<IBooruApiAccessor<int>, YandereApiLoader>();
        services.AddTransient<IBooruApiAccessor<string>, SankakuApiLoader>();
        
        services.AddTransient<ISankakuAuthManager, SankakuAuthManager>();

        return services;
    }
}
