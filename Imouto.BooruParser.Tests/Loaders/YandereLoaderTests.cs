using System;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders
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

                var serachResult = await ibal.LoadSearchResultAsync("1girl");
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
    }
}