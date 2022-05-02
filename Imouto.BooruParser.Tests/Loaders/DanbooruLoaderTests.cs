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
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("1girl");
                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldFindMd5()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:746310ab23d72e075755fd426469e31c");
                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().Be(1);
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

            /// <summary>
            /// Bug with post 5032478
            /// </summary>
            [Fact]
            public async Task ShouldLoadChildren()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var post = await loader.LoadPostAsync(5032478);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().Be(2);
                post.ParentId.Should().BeNull();
            }

            /// <summary>
            /// Bug with post 5314036
            /// </summary>
            [Fact]
            public async Task ShouldLoadChildrenFor5314036()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var post = await loader.LoadPostAsync(5314036);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().Be(1);
                post.ChildrenIds.First().Should().Be("5318896:46dda085dc9c60dd4380ed7b4433aa41");
                post.ParentId.Should().BeNull();
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
            public async Task ShouldLoadMetaTags()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();
                var searchResult = await loader.LoadSearchResultAsync("md5:43d3f7154d9612aaaf7ce0fa585887b2");
                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Tags.Should().Contain(x => x.Name == "paid reward");
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

            [Fact]
            public async Task ShouldLoadUgoiraMetadata()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:0802b6180ff110aa1055a5b9ef0d8b0a");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.UgoiraFrameData.Should().NotBeNull();
                post.UgoiraFrameData.ContentType.Should().Be("image/jpeg");
                post.UgoiraFrameData.Data.Should().HaveCount(411);
                post.UgoiraFrameData.Data.Last().Delay.Should().Be(2800);
                post.UgoiraFrameData.Data.Last().File.Should().Be("000410.jpg");
            }
        }

        public class FavoritePostAsyncMethod : DanbooruLoaderTests
        {
            public FavoritePostAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture)
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldFavoritePost()
            {
                var api = _danbooruLoaderFixture.GetApiAccessorWithAuth();
                await api.FavoritePostAsync(5004994);
            }
        }
    }
}
