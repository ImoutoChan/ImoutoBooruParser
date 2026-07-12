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
            var pages = new Dictionary<int, HistorySearchResult<TagHistoryEntry>>();

            async Task<HistorySearchResult<TagHistoryEntry>> LoadPage(int pageNumber)
            {
                if (pages.TryGetValue(pageNumber, out var cached))
                    return cached;

                var token = pageNumber == 1 ? null : new SearchToken(pageNumber.ToString());
                var page = await loader.GetTagHistoryPageAsync(token, limit, ct);
                pages[pageNumber] = page;
                return page;
            }

            static bool IsAtOrPastBoundary(
                HistorySearchResult<TagHistoryEntry> page,
                int historyId)
                => page.OldestHistoryId == null || page.OldestHistoryId <= historyId;

            var low = 1;
            var high = 1;
            var highPage = await LoadPage(high);
            while (!IsAtOrPastBoundary(highPage, afterHistoryId) && high < int.MaxValue)
            {
                low = high + 1;
                high = high > int.MaxValue / 2 ? int.MaxValue : high * 2;
                highPage = await LoadPage(high);
            }

            while (low < high)
            {
                var middle = low + (high - low) / 2;
                var middlePage = await LoadPage(middle);
                if (IsAtOrPastBoundary(middlePage, afterHistoryId))
                    high = middle;
                else
                    low = middle + 1;
            }

            var boundaryPage = low;
            if ((await LoadPage(boundaryPage)).OldestHistoryId == null && boundaryPage > 1)
                boundaryPage--;

            for (var pageNumber = boundaryPage; pageNumber >= 1; pageNumber--)
            {
                var page = await LoadPage(pageNumber);
                foreach (var tagsHistoryEntry in page.Results
                             .Where(x => x.HistoryId > afterHistoryId)
                             .Reverse())
                    yield return tagsHistoryEntry;
            }
        }
        else if (loader is SankakuApiLoader)
        {
            
            SearchToken? searchToken = null;
            while (true)
            {
                var page = await loader.GetTagHistoryPageAsync(searchToken, limit, ct);

                foreach (var tagsHistoryEntry in page.Results.Where(x => x.HistoryId > afterHistoryId))
                    yield return tagsHistoryEntry;

                if (page.Results.Any(x => x.HistoryId <= afterHistoryId) || page.NextToken == null)
                    break;

                searchToken = page.NextToken;
            }
        }
    }

    public static async IAsyncEnumerable<TagHistoryEntry> GetTagHistoryToDateTimeAsync(
        this IBooruApiLoader loader, 
        DateTimeOffset upToDateTime,
        int limit = 100,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        SearchToken? searchToken = null;
        while (true)
        {
            var page = await loader.GetTagHistoryPageAsync(searchToken, limit, ct);
            searchToken = page.NextToken;

            if (!page.Results.Any())
                yield break;

            foreach (var tagsHistoryEntry in page.Results)
                yield return tagsHistoryEntry;

            if (searchToken == null || page.Results.Min(x => x.UpdatedAt) < upToDateTime)
                yield break;
        }
    }
}
