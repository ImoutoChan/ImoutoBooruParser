using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders.SankakuLoaderTests
{
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
            public async Task ShouldReturnPostWithoutCreditnails()
            {
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                var post = await ibal.LoadPostAsync(5735331);

                post.Should().NotBe(null);
            }
        }

        public class LoadFirstTagHistoryPageAsyncMethod : SankakuLoaderTests
        {
            public LoadFirstTagHistoryPageAsyncMethod(SankakuLoaderFixture loaderFixture) 
                : base(loaderFixture)
            {
            }

            [Fact]
            public void ShouldThrowWithoutCreditnails()
            {
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                Func<Task> action = async () => await ibal.LoadFirstTagHistoryPageAsync();

                action.ShouldThrow<HttpRequestException>();
            }

            [Fact]
            public async Task ShouldReturnWithCreditnails()
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
            public void ShouldNotLoadTagsHistoryToDateWithoutCreditnails()
            {
                var ibal = _loaderFixture.GetLoaderWithoutAuth();

                Func<Task> action = async ()
                    => await ibal.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));

                action.ShouldThrow<HttpRequestException>();
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryToDateWithCreditnails()
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
            public async Task ShouldLoadTagsHistoryFromIdWithCreditnails()
            {
                var ibal = _loaderFixture.GetLoaderWithAuth();
                var firstTagHistoryPage = await ibal.LoadFirstTagHistoryPageAsync();

                var notesHistory = await ibal.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId);

                notesHistory.Should().NotBeEmpty();
            }
        }
    }
}