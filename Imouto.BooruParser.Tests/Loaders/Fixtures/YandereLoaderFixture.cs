using Imouto.BooruParser.Loaders;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures
{
    public class YandereLoaderFixture
    {
        private IBooruAsyncLoader _loader;

        public IBooruAsyncLoader GetLoader() => _loader ?? (_loader = new YandereLoader());
    }
}