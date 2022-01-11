using System.Threading.Tasks;

namespace Imouto.BooruParser.Loaders
{
    public interface IBooruApiAccessor
    {
        Task FavoritePostAsync(int postId);
    }
}