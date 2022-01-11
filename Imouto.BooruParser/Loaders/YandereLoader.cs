using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Yandere;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Imouto.BooruParser.Loaders
{
    public class YandereLoader : IBooruAsyncLoader, IBooruApiAccessor
    {
        private readonly BooruLoader _booruLoader;
        private readonly string _booruName = "Yandere";
        private readonly string _login;
        private readonly string _passwordHash;

        #region Consts

        private static readonly ILogger Logger = LoggerAccessor.GetLogger<DanbooruLoader>();

        private const string HOST = "yande.re";
        private const string ROOT_URL = "https://yande.re";
        private const string POST_URL = ROOT_URL + "/post/show/";
        private const string POST_JSON = ROOT_URL + "/post.json?tags=id:";
        private const string SEARCH_JSON = ROOT_URL + "/post.json?tags=";
        private const string POPULAR_JSON = ROOT_URL + "/post/popular_recent.json?period=";
        private const string POSTHISTORY_URL = ROOT_URL + "/history";
        private const string POSTHISTORY_PAGE_URL = POSTHISTORY_URL + "?page=";
        private const string NOTEHISTORY_URL = ROOT_URL + "/note/history";
        private const string NOTEHISTORY_PAGE_URL = NOTEHISTORY_URL + "?page=";
        private const string FAVORITE_POST_JSON_URL = ROOT_URL + "/post/vote.json";

        #endregion Consts

        public YandereLoader(
            HttpClient httpClient = null,
            BooruLoader booruLoader = null,
            string login = null,
            string passwordHash = null)
        {
            _login = login;
            _passwordHash = passwordHash;
            _booruLoader = booruLoader ?? new BooruLoader(httpClient, 0);
        }

        #region IBooruLoader members

        public async Task<Post> LoadPostAsync(int postId)
        {
            var postHtml = await _booruLoader.LoadPageAsync(POST_URL + postId);
            var postJson = JsonConvert.DeserializeObject<List<Model.Yandere.Json.Post>>(await _booruLoader.LoadPageAsync(POST_JSON + postId)).FirstOrDefault();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(postHtml);

            var post = new YanderePost(postId, htmlDoc.DocumentNode, postJson);
            return post;
        }

        public Task<SearchResult> LoadSearchResultAsync(string tagsString)
        {
            return LoadSearchResultAsync(tagsString, null);
        }

        private async Task<List<NoteUpdateEntry>> LoadNoteHistoryPageAsync(int page = 1)
        {
            var url = NOTEHISTORY_PAGE_URL + page;
            var pageHtml = await _booruLoader.LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = YandereNoteUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        public async Task<List<NoteUpdateEntry>> LoadNotesHistoryAsync(DateTime lastUpdateTime)
        {
            var result = new List<NoteUpdateEntry>();
            int failedCounter = 0;
            int nextPage = 1;

            using (Logger.BeginScope("Loading notes history for {BooruName}", _booruName))
            {
                do
                {
                    try
                    {
                        var historyPack = await LoadNoteHistoryPageAsync(nextPage);
                        result.AddRange(historyPack);
                        Logger.LogTrace("Status: LOADING | Notes page parsed #{PageNumber}", nextPage);
                        nextPage++;
                        failedCounter = 0;
                    }
                    catch (Exception e)
                    {
                        failedCounter++;
                        Logger.LogWarning(e, "Status: ERROR | Exceptions count: #{FailedCounter}", failedCounter);

                        if (failedCounter > 3)
                        {
                            Logger.LogError(e, "Status: TERMINATED");
                            break;
                        }
                    }
                }
                while (result.LastOrDefault()?.Date > lastUpdateTime);
            }

            return result;
        }

        public async Task<List<PostUpdateEntry>> LoadTagHistoryUpToAsync(DateTime toDate)
        {
            var result = new List<PostUpdateEntry>();

            var nextLoadPage = 1;
            var failedCounter = 0;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextLoadPage);
                    result.AddRange(historyPack);

                    nextLoadPage++;
                }
                catch (Exception e)
                {
                    failedCounter++;

                    if (failedCounter > 5)
                    {
                        throw new Exception("LoadFailed: " + e.Message, e);
                    }
                }
            }
            while (result.Last().UpdateDateTime > toDate);

            return result;
        }

        public async Task<List<PostUpdateEntry>> LoadTagHistoryFromAsync(int fromId)
        {
            var currentLast = (await LoadFirstTagHistoryPageAsync()).FirstOrDefault()?.UpdateId;

            var result = new List<PostUpdateEntry>();
            var failedCounter = 0;

            var nextPage = (currentLast - fromId) / 20 + 1;

            using (Logger.BeginScope("Loading tags history for {BooruName}", _booruName))
            {
                do
                {
                    try
                    {
                        var historyPack = await LoadTagHistoryPageAsync(nextPage);
                        result.InsertRange(0, historyPack);

                        Logger.LogTrace("Status: LOADING | Tags page parsed #{PageNumber}", nextPage);

                        nextPage--;
                        failedCounter = 0;
                    }
                    catch (Exception e)
                    {
                        failedCounter++;
                        Logger.LogWarning(e, "Status: ERROR | Exceptions count: #{FailedCounter}", failedCounter);

                        if (failedCounter > 3)
                        {
                            Logger.LogError(e, "Status: TERMINATED");
                            break;
                        }
                    }
                }
                while (nextPage > 0);
            }
            return result;
        }

        public Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync()
        {
            return LoadTagHistoryPageAsync();
        }

        public async Task<SearchResult> LoadPopularAsync(PopularType type)
        {
            var param = GetPopularTypeParam(type);

            var pageHtml = await _booruLoader.LoadPageAsync(POPULAR_JSON + param);

            var results = JsonConvert.DeserializeObject<List<Model.Yandere.Json.Post>>(pageHtml);

            var searchResult = new YandereSearchResult(results);

            return searchResult;
        }

        #endregion IBooruLoader members

        private string GetPopularTypeParam(PopularType type)
        {
            switch (type)
            {
                case PopularType.Day:
                    return "1d";
                case PopularType.Week:
                    return "1w";
                case PopularType.Month:
                    return "1m";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private async Task<SearchResult> LoadSearchResultAsync(string tagsString, int? limit)
        {
            var pageHtml = await _booruLoader.LoadPageAsync(SEARCH_JSON + WebUtility.UrlEncode(tagsString) + (limit.HasValue
                                                                ? $"&limit={limit.Value}"
                                                                : string.Empty));

            var results = JsonConvert.DeserializeObject<List<Model.Yandere.Json.Post>>(pageHtml);

            var searchResult = new YandereSearchResult(results);

            return searchResult;
        }

        private async Task<List<PostUpdateEntry>> LoadTagHistoryPageAsync(int? page = null)
        {
            var url = (page == null)
                ? POSTHISTORY_URL
                : POSTHISTORY_PAGE_URL + (page.Value);
            var pageHtml = await _booruLoader.LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = YanderePostUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        private string AddAuth(string url)
        {
            if (!url.Contains("?"))
            {
                url += "?";
            }

            return url + $"&login={_login}&password_hash={_passwordHash}";
        }

        public async Task FavoritePostAsync(int postId)
        {
            var url = AddAuth(FAVORITE_POST_JSON_URL);

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(postId.ToString()), "id");
            content.Add(new StringContent("3"), "score");

            await _booruLoader.PostAsync(url, content);
        }
    }
}
