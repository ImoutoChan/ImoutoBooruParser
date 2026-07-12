using System.Runtime.CompilerServices;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Implementations.Yandere;

namespace Imouto.BooruParser.Implementations;

public static class BooruApiLoaderNoteHistoryExtensions
{
    public static async Task<IReadOnlyCollection<NoteHistoryEntry>> GetNoteHistoryFirstPageAsync(
        this IBooruApiLoader loader,
        int limit = 100,
        CancellationToken ct = default)
    {
        var page = await loader.GetNoteHistoryPageAsync(null, limit, ct);
        return page.Results;
    }

    public static async IAsyncEnumerable<NoteHistoryEntry> GetNoteHistoryFromIdToPresentAsync(
        this IBooruApiLoader loader, 
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
            throw new NotSupportedException("Yande.re note history does not expose stable history ids");
        }
    }

    public static async IAsyncEnumerable<NoteHistoryEntry> GetNoteHistoryToDateTimeAsync(
        this IBooruApiLoader loader, 
        DateTimeOffset upToDateTime,
        int limit = 100,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        SearchToken? searchToken = null;
        while (true)
        {
            var page = await loader.GetNoteHistoryPageAsync(searchToken, limit, ct);
            searchToken = page.NextToken;

            if (!page.Results.Any())
                yield break;

            foreach (var historyEntry in page.Results)
                yield return historyEntry;

            if (searchToken == null || page.Results.Min(x => x.UpdatedAt) < upToDateTime)
                yield break;
        }
    }
}
