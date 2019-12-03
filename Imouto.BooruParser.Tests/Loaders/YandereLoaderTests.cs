using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders.YandereLoaderTests
{
    // This line will skip all tests in file
    // xUnit doesn't support skipping all tests in class
    // Comment this line to enable tests
    using FactAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;

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
    }
}