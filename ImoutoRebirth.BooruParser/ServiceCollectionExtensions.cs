using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations.Danbooru;
using ImoutoRebirth.BooruParser.Implementations.Yandere;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.BooruParser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBooruParser(this IServiceCollection services)
    {
        services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
        services.AddTransient<IBooruApiLoader, DanbooruApiLoader>();
        services.AddTransient<IBooruApiLoader, YandereApiLoader>();
        services.AddTransient<IBooruApiAccessor, DanbooruApiLoader>();
        services.AddTransient<IBooruApiAccessor, YandereApiLoader>();

        return services;
    }
}
