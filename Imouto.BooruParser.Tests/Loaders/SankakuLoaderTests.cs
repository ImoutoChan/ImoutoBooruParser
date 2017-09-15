using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders
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
            public LoadPostAsyncMethod(SankakuLoaderFixture loaderFixture) : base(loaderFixture)
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
            public LoadFirstTagHistoryPageAsyncMethod(SankakuLoaderFixture loaderFixture) : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldThrowWithoutCreditnails()
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
    }
}