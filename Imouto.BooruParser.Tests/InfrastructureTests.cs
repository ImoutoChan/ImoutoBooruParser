using System.Diagnostics;
using AwesomeAssertions;
using Imouto.BooruParser.Implementations;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Implementations.Gelbooru;
using Imouto.BooruParser.Implementations.Rule34;
using Imouto.BooruParser.Implementations.Sankaku;
using Imouto.BooruParser.Implementations.Yandere;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Imouto.BooruParser.Tests;

public class InfrastructureTests
{
    [Fact]
    public async Task ThrottlerSpacesEveryCall()
    {
        var throttler = Throttler.Get(Guid.NewGuid().ToString());
        var delay = TimeSpan.FromMilliseconds(50);
        var stopwatch = Stopwatch.StartNew();

        await throttler.UseAsync(delay);
        await throttler.UseAsync(delay);
        await throttler.UseAsync(delay);

        stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(80));
    }

    [Fact]
    public void DependencyInjectionReusesLoaderInstancesAcrossInterfaces()
    {
        var services = new ServiceCollection();
        services.Configure<DanbooruSettings>(x => x.BotUserAgent = "UnitTestBot/1.0");
        services.Configure<YandereSettings>(x => x.BotUserAgent = "UnitTestBot/1.0");
        services.Configure<GelbooruSettings>(x =>
        {
            x.ApiKey = "test";
            x.UserId = 1;
        });
        services.Configure<Rule34Settings>(x =>
        {
            x.ApiKey = "test";
            x.UserId = 1;
            x.BotUserAgent = "UnitTestBot/1.0";
        });
        services.Configure<SankakuSettings>(_ => { });
        services.AddBooruParsers();

        using var provider = services.BuildServiceProvider();
        var loaders = provider.GetServices<IBooruApiLoader>().ToArray();
        var accessors = provider.GetServices<IBooruApiAccessor>().ToArray();

        loaders.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<DanbooruApiLoader>()));
        loaders.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<YandereApiLoader>()));
        loaders.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<SankakuApiLoader>()));
        loaders.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<GelbooruApiLoader>()));
        loaders.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<Rule34ApiLoader>()));
        accessors.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<DanbooruApiLoader>()));
        accessors.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<YandereApiLoader>()));
        accessors.Should().ContainSingle(x => ReferenceEquals(x, provider.GetRequiredService<SankakuApiLoader>()));
    }

    [Fact]
    public async Task HistoryEnumeratorsStopOnEmptyPage()
    {
        var loader = new Mock<IBooruApiLoader>();
        loader.Setup(x => x.GetTagHistoryPageAsync(null, 100, default))
            .ReturnsAsync(new HistorySearchResult<TagHistoryEntry>([], null));
        loader.Setup(x => x.GetNoteHistoryPageAsync(null, 100, default))
            .ReturnsAsync(new HistorySearchResult<NoteHistoryEntry>([], null));

        var tags = await loader.Object
            .GetTagHistoryToDateTimeAsync(DateTimeOffset.UtcNow)
            .ToListAsync();
        var notes = await loader.Object
            .GetNoteHistoryToDateTimeAsync(DateTimeOffset.UtcNow)
            .ToListAsync();

        tags.Should().BeEmpty();
        notes.Should().BeEmpty();
    }
}
