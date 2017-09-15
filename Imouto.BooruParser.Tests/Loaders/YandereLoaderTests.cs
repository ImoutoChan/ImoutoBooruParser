using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Tests.Controllers.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Controllers
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
            public async Task ShouldReturnPostWithoutCreditnails()
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
            public async Task ShouldReturnHistoryWithoutCreditnails()
            {
                var ibal = _yandereLoaderFixture.GetLoader();

                var firstPage = await ibal.LoadFirstTagHistoryPageAsync();

                firstPage.Should().NotBeEmpty();
            }
        }
    }
}