using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Controllers
{
    public interface IBooruAsyncLoader
    {
        Task<Post> LoadPostAsync(int postId);

        Task<SearchResult> LoadSearchResultAsync(string tagsString);

        Task<List<NoteUpdateEntry>> LoadNotesHistoryAsync(DateTime lastUpdateTime);

        Task<List<PostUpdateEntry>> LoadTagHistoryUpToAsync(DateTime toDate);

        Task<List<PostUpdateEntry>> LoadTagHistoryFromAsync(int fromId);

        Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync();
    }
}