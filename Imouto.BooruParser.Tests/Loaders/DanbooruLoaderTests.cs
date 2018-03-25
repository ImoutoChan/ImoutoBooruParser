using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

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
            public async Task ShouldReturnPostWithoutCreditnails()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var post = await ibal.LoadPostAsync(1);

                post.Should().NotBe(null);
            }
        }

        public class LoadFirstTagHistoryPageAsyncMethod : DanbooruLoaderTests
        {
            public LoadFirstTagHistoryPageAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) 
                : base(danbooruLoaderFixture)
            {
            }

            [Fact]
            public async Task ShouldNotReturnWithoutCreditnails()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var firstPage = await ibal.LoadFirstTagHistoryPageAsync();

                firstPage.Should().BeEmpty();
            }


            [Fact]
            public async Task ShouldReturnWithCreditnails()
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
            public void ShouldThrowWithoutAuth()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                Func<Task> action = async () => await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
                action.ShouldThrow<Exception>();
            }

            [Fact]
            public async Task ShouldLoadWithAuth()
            {
                var ibal = _danbooruLoaderFixture.GetLoaderWithAuth();

                var notesHistory = await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadTagHistoryFromAsynMethodc : DanbooruLoaderTests
        {
            public LoadTagHistoryFromAsynMethodc(DanbooruLoaderFixture danbooruLoaderFixture) 
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

                var serachResult = await loader.LoadPopularAsync(PopularType.Day);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForWeek()
            {
                var loader = _danbooruLoaderFixture.GetLoaderWithoutAuth();

                var serachResult = await loader.LoadPopularAsync(PopularType.Week);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
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
    }
}
