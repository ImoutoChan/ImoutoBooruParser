using System.Runtime.CompilerServices;

namespace ImoutoRebirth.BooruParser.Implementations;

public static class BooruApiLoaderTagHistoryExtensions
{
    public static async Task<IReadOnlyCollection<TagHistoryEntry>> GetTagHistoryFirstPageAsync(
        this IBooruApiLoader loader,
        int limit = 100,
        CancellationToken ct = default)
    {
        var page = await loader.GetTagHistoryPageAsync(null, limit, ct);
        return page.Results;
    }

    public static async IAsyncEnumerable<TagHistoryEntry> GetTagHistoryFromIdToPresentAsync(
        this IBooruApiLoader loader, 
        int afterHistoryId,
        int limit = 100,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var searchToken = new SearchToken($"a{afterHistoryId}");
        do
        {
            var page = await loader.GetTagHistoryPageAsync(searchToken, limit, ct);
            searchToken = page.NextToken;

            foreach (var tagsHistoryEntry in page.Results)
                yield return tagsHistoryEntry;

        } while (searchToken != null);
    }

    public static async IAsyncEnumerable<TagHistoryEntry> GetTagHistoryToDateTimeAsync(
        this IBooruApiLoader loader, 
        DateTimeOffset upToDateTime,
        int limit = 100,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        SearchToken? searchToken = null;
        HistorySearchResult<TagHistoryEntry> page;
        do
        {
            page = await loader.GetTagHistoryPageAsync(searchToken, limit, ct);
            searchToken = page.NextToken;

            foreach (var tagsHistoryEntry in page.Results)
                yield return tagsHistoryEntry;

        } while (page.Results.Last().UpdatedAt >= upToDateTime);
    }
}
