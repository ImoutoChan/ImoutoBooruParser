using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Controllers;
using Xunit;

namespace Imouto.BooruParser.Tests.Controllers
{
    public class DanbooruLoaderTests
    {
        public class LoadPostAsyncMethod : DanbooruLoaderTests
        {
            [Fact]
            public async Task ShouldReturnPostWithoutCreditnails()
            {
                IBooruAsyncLoader ibal = new DanbooruLoader(null, null, 5000);

                var post = await ibal.LoadPostAsync(1);

                post.Should().NotBe(null);
            }
        }
    }

    public class SankakuLoaderTests
    {
        public class LoadPostAsyncMethod : SankakuLoaderTests
        {
            [Fact]
            public async Task ShouldReturnPostWithoutCreditnails()
            {
                IBooruAsyncLoader ibal = new SankakuLoader(null, null, 5000);

                var post = await ibal.LoadPostAsync(5735331);

                post.Should().NotBe(null);
            }
        }
    }

    public class YandereLoaderTests
    {
        public class LoadPostAsyncMethod : YandereLoaderTests
        {
            [Fact]
            public async Task ShouldReturnPostWithoutCreditnails()
            {
                IBooruAsyncLoader ibal = new YandereLoader();

                var post = await ibal.LoadPostAsync(408517);

                post.Should().NotBe(null);
            }
        }
    }
}
