using Imouto.BooruParser.Controllers;

namespace Imouto.BooruParser.Tests.Controllers.Fixtures
{
    public class YandereLoaderFixture
    {
        private IBooruAsyncLoader _loader;

        public IBooruAsyncLoader GetLoader() => _loader ?? (_loader = new YandereLoader());
    }
}