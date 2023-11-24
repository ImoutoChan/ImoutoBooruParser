using System.Runtime.CompilerServices;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Implementations.Sankaku;
using Imouto.BooruParser.Implementations.Yandere;

namespace Imouto.BooruParser.Implementations;

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
        if (loader is DanbooruApiLoader)
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
        else if (loader is YandereApiLoader)
        {
            var firstPage = await loader.GetTagHistoryFirstPageAsync(ct: ct);

            // prediction
            var currentId = firstPage.Max(x => x.HistoryId);
            var predictedPage = (currentId - afterHistoryId) / 20 + 2;
            
            // validation
            HistorySearchResult<TagHistoryEntry> page;
            var tries = 10;
            do
            {
                page = await loader.GetTagHistoryPageAsync(new SearchToken($"{predictedPage++}"), limit, ct);
                tries--;
            } while (!page.Results.Any() && tries > 0);
            
            if (page.Results.All(x => x.HistoryId > afterHistoryId))
                throw new Exception("Prediction failed");

            // execution
            var searchToken = new SearchToken($"{predictedPage--}");
            do
            {
                page = await loader.GetTagHistoryPageAsync(searchToken, limit, ct);
                searchToken = new SearchToken($"{predictedPage--}");

                foreach (var tagsHistoryEntry in page.Results)
                    yield return tagsHistoryEntry;

            } while (searchToken.Page != "0");
        }
        else if (loader is SankakuApiLoader)
        {
            
            HistorySearchResult<TagHistoryEntry> page;
            var cont = false;
            SearchToken? searchToken = null;
            do
            {
                page = await loader.GetTagHistoryPageAsync(searchToken, limit, ct);
                searchToken = page.NextToken;

                foreach (var tagsHistoryEntry in page.Results)
                    yield return tagsHistoryEntry;

                cont = !page.Results.Any(x => x.HistoryId < afterHistoryId);
            } while (cont);
        }
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

            if (!page.Results.Any())
                break;

        } while (page.Results.Last().UpdatedAt >= upToDateTime);
    }
}
