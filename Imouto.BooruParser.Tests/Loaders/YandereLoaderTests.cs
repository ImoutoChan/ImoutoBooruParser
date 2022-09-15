using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders.YandereLoaderTests
{
    public class YandereLoaderTests : IClassFixture<YandereLoaderFixture>
    {
        private readonly YandereLoaderFixture _yandereLoaderFixture;

        public YandereLoaderTests(YandereLoaderFixture yandereLoaderFixture)
        {
            _yandereLoaderFixture = yandereLoaderFixture;
        }

        public class LoadPostAsyncMethod : YandereLoaderTests
        {
            public LoadPostAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnPost()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var post = await ibal.LoadPostAsync(408517);

                post.Should().NotBe(null);
                post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
            }
        }

        public class LoadFirstTagHistoryPageAsyncMethod : YandereLoaderTests
        {
            public LoadFirstTagHistoryPageAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnHistory()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var firstPage = await ibal.LoadFirstTagHistoryPageAsync();

                firstPage.Should().NotBeEmpty();
            }
        }

        public class LoadSearchResultAsyncMethod : YandereLoaderTests
        {
            public LoadSearchResultAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldFind()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var serachResult = await ibal.LoadSearchResultAsync("no_bra");
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }
        }

        public class LoadNotesHistoryAsyncMethod : YandereLoaderTests
        {
            public LoadNotesHistoryAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadNotesHistory()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var notesHistory = await ibal.LoadNotesHistoryAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadTagHistoryUpToAsyncMethod : YandereLoaderTests
        {
            public LoadTagHistoryUpToAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryToDate()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var notesHistory = await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadTagHistoryFromAsyncMethod : YandereLoaderTests
        {
            public LoadTagHistoryFromAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryFromId()
            {
                var ibal = _yandereLoaderFixture.GetLoader();
                var firstTagHistoryPage = await ibal.LoadFirstTagHistoryPageAsync();

                var notesHistory = await ibal.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId);

                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadPopularAsyncMethod : YandereLoaderTests
        {
            public LoadPopularAsyncMethod(YandereLoaderFixture yandereLoaderFixture)
                : base(yandereLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadPopularForDay()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var serachResult = await ibal.LoadPopularAsync(PopularType.Day);
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForWeek()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var serachResult = await ibal.LoadPopularAsync(PopularType.Week);
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForMonth()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var serachResult = await ibal.LoadPopularAsync(PopularType.Month);
                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }
        }

        public class LoadPostMetadataAsyncMethod : YandereLoaderTests
        {
            public LoadPostMetadataAsyncMethod(YandereLoaderFixture fixture)
                : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldLoadParentsAndChildren()
            {
                var loader = _yandereLoaderFixture.GetLoader();

                var searchResult = await loader.LoadSearchResultAsync("md5:17a281b47c5baf18e4c9f6d85cc83285");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.ChildrenIds.Count.Should().NotBe(0);
                post.ParentId.Should().NotBeNullOrWhiteSpace();
            }
            
            [Fact]
            public async Task ShouldLoadChildrenOf801490()
            {
                var loader = _yandereLoaderFixture.GetLoader();

                var post = await loader.LoadPostAsync(801490);

                foreach (var childrenId in post.ChildrenIds)
                {
                    var cpost = await loader.LoadPostAsync(Int32.Parse(childrenId.Split(':').First()));
                    var md5 = cpost.Md5;

                    md5.Should().NotBeEmpty();
                }

                post.ChildrenIds.Count.Should().Be(3);
            }

            [Fact]
            public async Task ShouldLoadNotes()
            {
                var loader = _yandereLoaderFixture.GetLoader();

                var searchResult = await loader.LoadSearchResultAsync("md5:caedf1c6957a956fcbd1f8bb17effb73");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Notes.Count.Should().BeGreaterThan(0);
            }

            [Fact]
            public async Task ShouldLoadPools()
            {
                var loader = _yandereLoaderFixture.GetLoader();

                var searchResult = await loader.LoadSearchResultAsync("md5:2bb1833a5a12852d186d4fcf86bf9020");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Pools.Count.Should().BeGreaterOrEqualTo(1);
            }
            
            [Fact]
            public async Task ShouldLoadSampleUrl()
            {
                var loader = _yandereLoaderFixture.GetLoader();

                var post = await loader.LoadPostAsync(1021031);

                post.Should().NotBeNull();
                post.SampleUrl.Should().Contain("sample");
            }
        }

        public class FavoritePostAsyncMethod : YandereLoaderTests
        {
            public FavoritePostAsyncMethod(YandereLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldFavoritePost()
            {
                var api = _yandereLoaderFixture.GetApiAccessorWithAuth();
                await api.FavoritePostAsync(883843);
            }
        }
    }
}
