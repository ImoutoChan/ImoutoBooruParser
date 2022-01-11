using Imouto.BooruParser.Loaders;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures
{
    public class YandereLoaderFixture
    {
        private IBooruAsyncLoader _loader;
        private IBooruApiAccessor _apiAccessor;

        public IBooruAsyncLoader GetLoader() => _loader ??= new YandereLoader();

        public IBooruApiAccessor GetApiAccessorWithAuth()
            => _apiAccessor ??= new YandereLoader(login: "testuser1", passwordHash: "5eedf880498cac52bbfc8386150682d54174fab0");
    }
}
