using FluentAssertions;
using ImoutoRebirth.BooruParser.Extensions;
using ImoutoRebirth.BooruParser.Implementations;
using ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace ImoutoRebirth.BooruParser.Tests.Loaders;

// This line will skip all tests in file
// xUnit doesn't support skipping all tests in class
// Comment this line to enable tests
// using FactAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;

public class SankakuLoaderTests : IClassFixture<SankakuLoaderFixture>
{
    private readonly SankakuLoaderFixture _loaderFixture;

    public SankakuLoaderTests(SankakuLoaderFixture loaderFixture) => _loaderFixture = loaderFixture;

    public class GetPostAsyncMethod : SankakuLoaderTests
    {
        public GetPostAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldReturnPostWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5735331);

            post.Should().NotBe(null);
            post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ShouldContainLinkWithoutAmp()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync(5735331);

            post.OriginalUrl.Should().NotContain("&amp;");
        }
    }

    public class LoadFirstTagHistoryPageAsyncMethod : SankakuLoaderTests
    {
        public LoadFirstTagHistoryPageAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public void ShouldThrowWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            Func<Task> action = async () => await loader.GetTagHistoryFirstPageAsync();

            action.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task ShouldReturnWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var firstPage = await loader.GetTagHistoryFirstPageAsync();

            firstPage.Should().NotBeEmpty();
        }
    }
        
    public class LoadSearchResultAsyncMethod : SankakuLoaderTests
    {
        public LoadSearchResultAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldFind()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.SearchAsync("1girl");
            result.Results.Should().NotBeEmpty();
            result.Results.Should().HaveCountGreaterThan(1);
        }

        [Fact]
        public async Task ShouldFindWithMultipleTags()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.SearchAsync("1girl long_hair");
            result.Results.Should().NotBeEmpty();
            result.Results.Should().HaveCountGreaterThan(1);
        }
    }

    public class LoadNotesHistoryAsyncMethod : SankakuLoaderTests
    {
        public LoadNotesHistoryAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadNotesHistory()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var notesHistory = await loader.GetNoteHistoryToDateTimeAsync(DateTime.Now.AddHours(-1)).ToListAsync();
            notesHistory.Should().NotBeEmpty();
        }
    }

    public class LoadTagHistoryUpToAsyncMethod : SankakuLoaderTests
    {
        public LoadTagHistoryUpToAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public void ShouldNotLoadTagsHistoryToDateWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            Func<Task> action = async ()
                => await loader.GetTagHistoryToDateTimeAsync(DateTime.Now.AddMinutes(-5)).ToListAsync();

            action.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryToDateWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var notesHistory = await loader.GetTagHistoryToDateTimeAsync(DateTime.Now.AddMinutes(-5)).ToListAsync();
            notesHistory.Should().NotBeEmpty();
        }
    }

    public class LoadTagHistoryFromAsyncMethod : SankakuLoaderTests
    {
        public LoadTagHistoryFromAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryFromIdWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();
            var firstTagHistoryPage = await loader.GetTagHistoryFirstPageAsync();

            var notesHistory = await loader.GetTagHistoryFromIdToPresentAsync(firstTagHistoryPage.Last().HistoryId).ToListAsync();

            notesHistory.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryFromIdAndHaveAllDataWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();
            var firstTagHistoryPage = await loader.GetTagHistoryFirstPageAsync();

            var tagsHistory = await loader
                .GetTagHistoryFromIdToPresentAsync(firstTagHistoryPage.Last().HistoryId - 100)
                .ToListAsync();

            tagsHistory.Should().NotBeEmpty();
            tagsHistory.Count.Should().BeGreaterOrEqualTo(firstTagHistoryPage.Count + 100);
            tagsHistory.Select(x => x.PostId).Should().Contain(firstTagHistoryPage.Select(x => x.PostId));
        }
    }

    public class LoadPopularAsyncMethod : SankakuLoaderTests
    {
        public LoadPopularAsyncMethod(SankakuLoaderFixture sankakuLoaderFixture)
            : base(sankakuLoaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadPopularForDay()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.GetPopularPostsAsync(PopularType.Day);

            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadPopularForWeek()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.GetPopularPostsAsync(PopularType.Week);

            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadPopularForMonth()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.GetPopularPostsAsync(PopularType.Month);

            result.Results.Should().NotBeEmpty();
        }
    }

    public class LoadPostMetadataAsyncMethod : SankakuLoaderTests
    {
        public LoadPostMetadataAsyncMethod(SankakuLoaderFixture sankakuLoaderFixture)
            : base(sankakuLoaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadParentsAndChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(31143268);

            post.Tags.Count.Should().BeGreaterThan(30);
            post.ChildrenIds.Count.Should().NotBe(0);
            post.Parent!.Id.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task ShouldLoadPostWithEmptyChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostByMd5Async("692078f4b19d8a7992bc361baac39650");

            post.Should().NotBeNull();
            post!.Tags.Count.Should().BeGreaterThan(30);

            // no longer empty, but inaccessible
            post.ChildrenIds.Count.Should().Be(2);
        }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostByMd5Async("acea0eadcc5e8cc64100dc3bde45720c");

            post.Should().NotBeNull();
            post!.Notes.Count.Should().Be(4);
        }
            
        [Fact]
        public async Task ShouldLoadSampleUrl()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(17649920);

            post.Should().NotBeNull();
            post.SampleUrl.Should().Contain("sample");
        }
    }
}
