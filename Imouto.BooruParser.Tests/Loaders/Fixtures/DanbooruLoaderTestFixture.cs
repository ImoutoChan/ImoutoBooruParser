using Imouto.BooruParser.Loaders;

namespace Imouto.BooruParser.Tests.Loaders.Fixtures
{
    public class DanbooruLoaderFixture
    {
        private IBooruAsyncLoader _danbooruWithAuth;
        private IBooruAsyncLoader _danbooruWithoutAuth;

        public IBooruAsyncLoader GetLoaderWithAuth() 
            => _danbooruWithAuth
                ?? (_danbooruWithAuth = new DanbooruLoader("testuser159", "t77cOKpOMV5I4HN3r3gfOooG5hrh3sAqgsD_YDQCZGc", 1240));

        public IBooruAsyncLoader GetLoaderWithoutAuth()
            => _danbooruWithoutAuth
               ?? (_danbooruWithoutAuth = new DanbooruLoader(null, null, 5000));
    }
}
