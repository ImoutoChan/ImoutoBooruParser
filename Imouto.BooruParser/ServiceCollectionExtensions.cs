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
    public static IServiceCollection AddBooruParsers(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IFlurlClientCache>(_ => new FlurlClientCache());

        services.AddSingleton<ISankakuAuthManager, SankakuAuthManager>();

        services.AddSingleton<DanbooruApiLoader>();
        services.AddSingleton<YandereApiLoader>();
        services.AddSingleton<SankakuApiLoader>();
        services.AddSingleton<GelbooruApiLoader>();
        services.AddSingleton<Rule34ApiLoader>();

        services.AddSingleton<IBooruApiLoader>(x => x.GetRequiredService<DanbooruApiLoader>());
        services.AddSingleton<IBooruApiLoader>(x => x.GetRequiredService<YandereApiLoader>());
        services.AddSingleton<IBooruApiLoader>(x => x.GetRequiredService<SankakuApiLoader>());
        services.AddSingleton<IBooruApiLoader>(x => x.GetRequiredService<GelbooruApiLoader>());
        services.AddSingleton<IBooruApiLoader>(x => x.GetRequiredService<Rule34ApiLoader>());

        services.AddSingleton<IBooruApiAccessor>(x => x.GetRequiredService<DanbooruApiLoader>());
        services.AddSingleton<IBooruApiAccessor>(x => x.GetRequiredService<YandereApiLoader>());
        services.AddSingleton<IBooruApiAccessor>(x => x.GetRequiredService<SankakuApiLoader>());

        return services;
    }
}
