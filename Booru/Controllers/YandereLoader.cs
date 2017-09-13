using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Yandere;
using Newtonsoft.Json;
using NLog;

namespace Imouto.BooruParser.Controllers
{
    public class YandereLoader : AbstractBooruLoader, IBooruAsyncLoader
    {
        #region Consts

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string HOST = "yande.re";
        private const string ROOT_URL = "https://yande.re";
        private const string POST_URL = ROOT_URL + "/post/show/";
        private const string POST_JSON = ROOT_URL + "/post.json?tags=id:";
        private const string SEARCH_JSON = ROOT_URL + "/post.json?tags=";
        private const string POSTHISTORY_URL = ROOT_URL + "/history";
        private const string POSTHISTORY_PAGE_URL = POSTHISTORY_URL + "?page=";
        private const string NOTEHISTORY_URL = ROOT_URL + "/note/history";
        private const string NOTEHISTORY_PAGE_URL = NOTEHISTORY_URL + "?page=";

        #endregion Consts

        public YandereLoader(HttpClient httpClient, int loadDelay) : base(httpClient, loadDelay)
        {
        }

        #region Methods

        private async Task<SearchResult> LoadSearchResultAsync(string tagsString, int? limit)
        {
            var pageHtml = await LoadPageAsync(SEARCH_JSON + WebUtility.UrlEncode(tagsString) + (limit.HasValue
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
            var pageHtml = await LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = YanderePostUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        #endregion Methods

        #region IBooruLoader members

        public async Task<Post> LoadPostAsync(int postId)
        {
            var postHtml = await LoadPageAsync(POST_URL + postId);
            var postJson = JsonConvert.DeserializeObject<List<Model.Yandere.Json.Post>>(await LoadPageAsync(POST_JSON + postId)).FirstOrDefault();

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
            var pageHtml = await LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = YandereNoteUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        public async Task<List<NoteUpdateEntry>> LoadNotesHistoryAsync(DateTime lastUpdateTime)
        {
            var booruName = "Yandere";
            
            var result = new List<NoteUpdateEntry>();
            int failedCounter = 0;
            int nextPage = 1;
            do
            {
                try
                {
                    var historyPack = await LoadNoteHistoryPageAsync(nextPage);
                    result.AddRange(historyPack);
                    Logger.Info($"{booruName} | Parsing notes history | Status: PARSING | History page parsed #{nextPage}");
                    nextPage++;
                    failedCounter = 0;
                }
                catch (Exception e)
                {
                    failedCounter++;
                    Logger.Error($"{booruName} | Parsing notes history | Status: ERROR | Exception #{failedCounter} : {e.Message}");

                    if (failedCounter > 3)
                    {
                        Logger.Error($"{booruName} | Parsing notes history | Status: TERMINATE");
                        break;
                    }
                }
            }
            while (result.LastOrDefault()?.Date > lastUpdateTime);

            return result;
        }

        public async Task<List<PostUpdateEntry>> LoadTagHistoryUpToAsync(DateTime toDate)
        {
            var result = new List<PostUpdateEntry>();

            var nextLoadPageAsync = 1;
            var failedCounter = 0;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextLoadPageAsync);
                    result.AddRange(historyPack);

                    nextLoadPageAsync++;
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
            var booruName = "Yandere";

            var currentLast = (await LoadFirstTagHistoryPageAsync()).FirstOrDefault()?.UpdateId;

            var result = new List<PostUpdateEntry>();
            var failedCounter = 0;

            var nextPage = (currentLast - fromId) / 20 + 1;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextPage);
                    result.InsertRange(0, historyPack);

                    Logger.Info($"{booruName} | Parsing tags history | Status: PARSING | History page parsed #{nextPage}");

                    nextPage--;
                    failedCounter = 0;
                }
                catch (Exception e)
                {
                    failedCounter++;
                    Logger.Error($"{booruName} | Parsing tags history | Status: ERROR | Exception #{failedCounter} : {e.Message}");

                    if (failedCounter > 3)
                    {
                        Logger.Error($"{booruName} | Parsing tags history | Status: TERMINATE");
                        break;
                    }
                }
            }
            while (nextPage > 0);
            return result;
        }

        public Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync()
        {
            return LoadTagHistoryPageAsync();
        }

        #endregion IBooruLoader members


        protected override string RootUrl => "yande.re";

        protected override string AddAuth(string url) => url;
    }
}
