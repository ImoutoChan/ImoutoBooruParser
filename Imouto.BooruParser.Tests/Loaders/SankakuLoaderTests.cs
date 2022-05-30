using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders.SankakuLoaderTests
{
    // This line will skip all tests in file
    // xUnit doesn't support skipping all tests in class
    // Comment this line to enable tests
    using FactAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;

    public class SankakuLoaderTests : IClassFixture<SankakuLoaderFixture>
    {
        private readonly SankakuLoaderFixture _loaderFixture;

        public SankakuLoaderTests(SankakuLoaderFixture loaderFixture)
        {
            _loaderFixture = loaderFixture;
        }

        public class LoadPostAsyncMethod : SankakuLoaderTests
        {
            public LoadPostAsyncMethod(SankakuLoaderFixture loaderFixture) 
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnPostWithoutCredentials()
            {
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                var post = await ibal.LoadPostAsync(5735331);

                post.Should().NotBe(null);
                post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
            }

            [Fact]
            public async Task ShouldContainLinkWithoutAmp()
            {
                var ibal = _loaderFixture.GetLoaderWithoutAuth();
                var post = await ibal.LoadPostAsync(5735331);

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
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                Func<Task> action = async () => await ibal.LoadFirstTagHistoryPageAsync();

                action.Should().Throw<HttpRequestException>();
            }

            [Fact]
            public async Task ShouldReturnWithCredentials()
            {
                var ibal = _loaderFixture.GetLoaderWithAuth();

                var firstPage = await ibal.LoadFirstTagHistoryPageAsync();

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
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                var serachResult = await ibal.LoadSearchResultAsync("1girl");
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldFindWithMultipleTags()
            {
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                var serachResult = await ibal.LoadSearchResultAsync("1girl long_hair");
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
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
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                var notesHistory = await ibal.LoadNotesHistoryAsync(DateTime.Now.AddHours(-1));
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
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                Func<Task> action = async ()
                    => await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));

                action.Should().Throw<HttpRequestException>();
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryToDateWithCredentials()
            {
                var ibal = _loaderFixture.GetLoaderWithAuth();

                var notesHistory = await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
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
                var ibal = _loaderFixture.GetLoaderWithAuth();
                var firstTagHistoryPage = await ibal.LoadFirstTagHistoryPageAsync();

                var notesHistory = await ibal.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId);

                notesHistory.Should().NotBeEmpty();
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryFromIdAndHaveAllDataWithCredentials()
            {
                var ibal = _loaderFixture.GetLoaderWithAuth();
                var firstTagHistoryPage = await ibal.LoadFirstTagHistoryPageAsync();

                var tagsHistory = await ibal.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId - 100);

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

                var serachResult = await loader.LoadPopularAsync(PopularType.Day);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
            }

            [Fact]
            public async Task ShouldLoadPopularForWeek()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var serachResult = await loader.LoadPopularAsync(PopularType.Week);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
            }

            [Fact]
            public async Task ShouldLoadPopularForMonth()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var serachResult = await loader.LoadPopularAsync(PopularType.Month);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
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

                var post = await loader.LoadPostAsync(31143268);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().NotBe(0);
                post.ParentId.Should().NotBeNullOrWhiteSpace();
            }

            [Fact]
            public async Task ShouldLoadPostWithEmptyChildren()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();
                var searchResult = await loader.LoadSearchResultAsync("md5:692078f4b19d8a7992bc361baac39650");
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Tags.Count.Should().BeGreaterThan(30);

                // no longer empty, but inaccessible
                post.ChildrenIds.Count.Should().Be(2);
            }

            [Fact]
            public async Task ShouldLoadNotes()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:acea0eadcc5e8cc64100dc3bde45720c");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Notes.Count.Should().Be(4);
            }
        }
    }
}
