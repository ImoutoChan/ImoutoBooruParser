using System.Drawing;
using FluentAssertions;
using ImoutoRebirth.BooruParser.Extensions;
using ImoutoRebirth.BooruParser.Implementations;
using ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace ImoutoRebirth.BooruParser.Tests.Loaders;

public class YandereLoaderTests : IClassFixture<YandereApiLoaderFixture>
{
    private readonly YandereApiLoaderFixture _loaderFixture;

    public YandereLoaderTests(YandereApiLoaderFixture loaderFixture) => _loaderFixture = loaderFixture;

    public class GetPostAsyncMethod : YandereLoaderTests
    {
        public GetPostAsyncMethod(YandereApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldReturnPost()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(408517);

            post.Should().NotBe(null);
            post.OriginalUrl.Should().Be("https://files.yande.re/image/5569d245d4c85921a0da173d87391862/yande.re%20408517%20cleavage%20dakimakura%20fate_grand_order%20kimono%20mash_kyrielight%20no_bra%20nopan%20open_shirt%20yuran.png");
            post.Id.Id.Should().Be(408517);
            post.Id.Md5Hash.Should().Be("5569d245d4c85921a0da173d87391862");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(9);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Questionable);
            post.Source.Should().Be("https://yurang92.booth.pm/items/621246");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1383, 4393));
            post.PostedAt.Should().Be(new DateTimeOffset(2017, 09, 06, 5, 38, 17, TimeSpan.Zero));
            post.SampleUrl.Should().Be("https://files.yande.re/sample/5569d245d4c85921a0da173d87391862/yande.re%20408517%20sample%20cleavage%20dakimakura%20fate_grand_order%20kimono%20mash_kyrielight%20no_bra%20nopan%20open_shirt%20yuran.jpg");
            post.UploaderId.Id.Should().Be(25882);
            post.UploaderId.Name.Should().Be("Mr_GT");
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.FileSizeInBytes.Should().Be(5455985);
            post.UgoiraFrameDelays.Should().BeEmpty();

        }
    }

    public class LoadSearchResultAsyncMethod : YandereLoaderTests
    {
        public LoadSearchResultAsyncMethod(YandereApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldFind()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.SearchAsync("no_bra");
            result.Results.Should().HaveCountGreaterThan(1);
        }
    }

    public class LoadNotesHistoryAsyncMethod : YandereLoaderTests
    {
        public LoadNotesHistoryAsyncMethod(YandereApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadNotesHistory()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.GetNoteHistoryToDateTimeAsync(DateTime.Now.AddHours(-5)).ToListAsync();

            result.Should().NotBeEmpty();
            result.Should().HaveCountGreaterOrEqualTo(25);
        }
    }

    public class LoadTagHistoryUpToAsyncMethod : YandereLoaderTests
    {
        public LoadTagHistoryUpToAsyncMethod(YandereApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryToDate()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.GetTagHistoryToDateTimeAsync(DateTime.Now.AddHours(-1)).ToListAsync();

            result.Should().NotBeEmpty();
            result.First().HistoryId.Should().BeGreaterThan(0);
            result.First().PostId.Should().BeGreaterThan(0);
            result.First().UpdatedAt.Should().BeAfter(DateTime.Now.AddHours(-2));
        }

        [Fact]
        public async Task ShouldGetTagHistoryFirstPage()
        {
            var loader = _loaderFixture.GetLoader();

            var firstPage = await loader.GetTagHistoryFirstPageAsync();

            firstPage.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldGetTagHistoryFromIdToPresent()
        {
            var loader = _loaderFixture.GetLoader();
            var firstTagHistoryPage = await loader.GetTagHistoryFirstPageAsync();

            var result = await loader.GetTagHistoryFromIdToPresentAsync(firstTagHistoryPage.Last().HistoryId)
                .ToListAsync();

            result.Should().NotBeEmpty();
            result.DistinctBy(x => x.HistoryId).Should().HaveCount(result.Count);
        }
    }

    public class LoadPopularAsyncMethod : YandereLoaderTests
    {
        public LoadPopularAsyncMethod(YandereApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadPopularForDay()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.GetPopularPostsAsync(PopularType.Day);
            result.Results.Should().HaveCountGreaterThan(1);
        }

        [Fact]
        public async Task ShouldLoadPopularForWeek()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.GetPopularPostsAsync(PopularType.Week);
            result.Results.Should().HaveCountGreaterThan(1);
        }

        [Fact]
        public async Task ShouldLoadPopularForMonth()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.GetPopularPostsAsync(PopularType.Month);
            result.Results.Should().HaveCountGreaterThan(1);
        }
    }

    public class LoadPostMetadataAsyncMethod : YandereLoaderTests
    {
        public LoadPostMetadataAsyncMethod(YandereApiLoaderFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task ShouldLoadParentsAndChildren()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(507625);

            post.ChildrenIds.Count.Should().NotBe(0);
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().BeGreaterThan(1);
        }
            
        [Fact]
        public async Task ShouldLoadChildrenOf801490()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(801490);

            foreach (var childrenId in post.ChildrenIds)
            {
                var childPost = await loader.GetPostAsync(childrenId.Id);
                var md5 = childPost.Id.Md5Hash;

                md5.Should().NotBeEmpty();
            }

            post.ChildrenIds.Count.Should().Be(3);
        }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(321139);

            post.Notes.Should().HaveCount(2);
            post.Notes.First().Id.Should().Be(4625);
            post.Notes.First().Text.Should().Be("Hmm.....!");
            post.Notes.First().Point.Should().Be(new Position(72, 824));
            post.Notes.First().Size.Should().Be(new Size(109, 274));
        }

        [Fact]
        public async Task ShouldLoadPools()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(572039);

            post.Pools.Count.Should().BeGreaterOrEqualTo(1);
        }
            
        [Fact]
        public async Task ShouldLoadSampleUrl()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(1021031);

            post.Should().NotBeNull();
            post.SampleUrl.Should().Contain("sample");
        }
    }

    public class FavoritePostAsyncMethod : YandereLoaderTests
    {
        public FavoritePostAsyncMethod(YandereApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldFavoritePost()
        {
            var api = _loaderFixture.GetApiAccessorWithAuth();
            await api.FavoritePostAsync(883843);
        }
    }
}
