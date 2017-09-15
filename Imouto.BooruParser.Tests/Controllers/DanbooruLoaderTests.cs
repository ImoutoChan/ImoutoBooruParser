using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Controllers;
using Imouto.BooruParser.Tests.Controllers.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Controllers
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
            public LoadFirstTagHistoryPageAsyncMethod(DanbooruLoaderFixture danbooruLoaderFixture) : base(danbooruLoaderFixture)
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
    }
}
