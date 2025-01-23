using System.Runtime.CompilerServices;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Implementations.Yandere;

namespace Imouto.BooruParser.Implementations;

public static class BooruApiLoaderNoteHistoryExtensions
{
    public static async Task<IReadOnlyCollection<NoteHistoryEntry<TId>>> GetNoteHistoryFirstPageAsync<TId>(
        this IBooruApiLoader<TId> loader,
        int limit = 100,
        CancellationToken ct = default)
    {
        var page = await loader.GetNoteHistoryPageAsync(null, limit, ct);
        return page.Results;
    }

    public static async IAsyncEnumerable<NoteHistoryEntry<TId>> GetNoteHistoryFromIdToPresentAsync<TId>(
        this IBooruApiLoader<TId> loader,
        int afterHistoryId,
        int limit = 100,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (loader is DanbooruApiLoader)
        {
            var searchToken = new SearchToken($"a{afterHistoryId}");
            do
            {
                var page = await loader.GetNoteHistoryPageAsync(searchToken, limit, ct);
                searchToken = page.NextToken;

                foreach (var historyEntry in page.Results)
                    yield return historyEntry;

            } while (searchToken != null);
        }
        else if (loader is YandereApiLoader)
        {
            throw new NotImplementedException();
        }
    }

    public static async IAsyncEnumerable<NoteHistoryEntry<TId>> GetNoteHistoryToDateTimeAsync<TId>(
        this IBooruApiLoader<TId> loader,
        DateTimeOffset upToDateTime,
        int limit = 100,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        SearchToken? searchToken = null;
        HistorySearchResult<NoteHistoryEntry<TId>> page;
        do
        {
            page = await loader.GetNoteHistoryPageAsync(searchToken, limit, ct);
            searchToken = page.NextToken;

            foreach (var historyEntry in page.Results)
                yield return historyEntry;

        } while (page.Results.Last().UpdatedAt >= upToDateTime);
    }
}
