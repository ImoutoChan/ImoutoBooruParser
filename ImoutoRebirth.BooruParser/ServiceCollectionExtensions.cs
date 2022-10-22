using Flurl.Http.Configuration;
using ImoutoRebirth.BooruParser.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.BooruParser;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBooruParser(this IServiceCollection services)
    {
        services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
        services.AddTransient<IBooruApiLoader, DanbooruApiLoader>();

        return services;
    }
}
