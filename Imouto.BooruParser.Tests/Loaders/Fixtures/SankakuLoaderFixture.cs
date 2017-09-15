using Imouto.BooruParser.Loaders;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures
{
    public class SankakuLoaderFixture
    {
        private IBooruAsyncLoader _withAuth;
        private IBooruAsyncLoader _withoutAuth;

        public IBooruAsyncLoader GetLoaderWithAuth()
            => _withAuth
               ?? (_withAuth = new SankakuLoader("testuser159", "69f56a924a71774358c31e9233fc8e3c9a1b7d55", 4761));

        public IBooruAsyncLoader GetLoaderWithoutAuth()
            => _withoutAuth
               ?? (_withoutAuth = new SankakuLoader(null, null, 5000));
    }
}