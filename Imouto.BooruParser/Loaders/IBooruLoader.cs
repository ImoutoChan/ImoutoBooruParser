using System;
using System.Collections.Generic;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Loaders
{
    /// <summary>
    /// Interface provide booru parce logic.
    /// </summary>
    public interface IBooruLoader
    {
        Post LoadPost(int postId);

        SearchResult LoadSearchResult(string tagsString);
        
        List<NoteUpdateEntry> LoadNotesHistory(DateTime lastUpdateTime);

        List<PostUpdateEntry> LoadTagHistoryUpTo(DateTime toDate);

        List<PostUpdateEntry> LoadTagHistoryFrom(int fromId);

        List<PostUpdateEntry> LoadFirstTagHistoryPage();
    }
}