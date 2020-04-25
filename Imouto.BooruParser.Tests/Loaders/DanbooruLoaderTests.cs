using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Danbooru;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;
using Xunit.Sdk;

namespace Imouto.BooruParser.Tests.Loaders.DanbooruLoaderTests
{
    public class DanbooruLoaderTests : IClassFixture<DanbooruLoaderFixture>
    {
        private readonly DanbooruLoaderFixture _danbooruLoaderFixture;

        public DanbooruLoaderTests(DanbooruLoaderFixture danbooruLoaderFixture)
        {
            _danbooruLoaderFixture = danbooruLoaderFixture;
        }

        public class LoadPostAsyncMethod : DanbooruLoaderTests
        {
            public LoadPostAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnPostWithoutCredentials()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var post = await ibal.LoadPostAsync(1);

                post.Should().NotBe(null);
                post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
            }
        }

        public class LoadFirstTagHistoryPageAsyncMethod : DanbooruLoaderTests
        {
            public LoadFirstTagHistoryPageAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) 
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnWithCredentials()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithAuth();

                var firstPage = await ibal.LoadFirstTagHistoryPageAsync();

                firstPage.Should().NotBeEmpty();
            }
        }
        
        public class LoadSearchResultAsyncMethod : DanbooruLoaderTests
        {
            public LoadSearchResultAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) 
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldFind()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var serachResult = await ibal.LoadSearchResultAsync("1girl");
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }
        }
        
        public class LoadNotesHistoryAsyncMethod : DanbooruLoaderTests
        {
            public LoadNotesHistoryAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) 
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadNotesHistory()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var notesHistory = await ibal.LoadNotesHistoryAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }
        public class LoadTagHistoryUpToAsyncMethod : DanbooruLoaderTests
        {
            public LoadTagHistoryUpToAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) 
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadWithAuth()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithAuth();

                var notesHistory = await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadTagHistoryFromAsyncMethod : DanbooruLoaderTests
        {
            public LoadTagHistoryFromAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) 
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryFromId()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithAuth();
                var firstTagHistoryPage = await ibal.LoadFirstTagHistoryPageAsync();

                var notesHistory = await ibal.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId);

                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadPopularAsyncMethod : DanbooruLoaderTests
        {
            public LoadPopularAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture)
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadPopularForDay()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadPopularAsync(PopularType.Day);

                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForWeek()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadPopularAsync(PopularType.Week);

                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForMonth()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var serachResult = await loader.LoadPopularAsync(PopularType.Month);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }
        }

        public class LoadPostMetadataAsyncMethod : DanbooruLoaderTests
        {
            public LoadPostMetadataAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture)
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadParentsAndChildren()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:46cce564e9b43a4c69c132840dca1252");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().NotBe(0);
                post.ParentId.Should().NotBeNullOrWhiteSpace();
            }

            [Fact]
            public async Task ShouldLoadNotes()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:59b8ac9d3fe23a315f4468623ea7609a");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Notes.Count.Should().BeGreaterThan(6);
            }

            [Fact]
            public async Task ShouldLoadPools()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:e9964274b9d09fbd365268a71ef35713");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Pools.Count.Should().BeGreaterOrEqualTo(2);
            }
        }
    }
}
