using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Danbooru;
using Newtonsoft.Json;

namespace Imouto.BooruParser.Controllers
{
    public class DanbooruLoader : AbstractBooruLoader, IBooruLoader, IBooruAsyncLoader
    {
        #region Consts

        private const string HOST = "danbooru.donmai.us";
        private const string ROOT_URL = "https://danbooru.donmai.us";
        private const string POST_URL = ROOT_URL + "/posts/";
        private const string POST_JSON = ROOT_URL + "/posts/{0}.json";
        private const string SEARCH_JSON = ROOT_URL + "/posts.json?utf8=✓&tags=";

        private const string POSTHISTORY_URL = ROOT_URL + "/post_versions";
        private const string POSTHISTORY_PAGE_URL = POSTHISTORY_URL + "?page=";
        private const string POSTHISTORY_AFTER_URL = POSTHISTORY_URL + "?page=a";

        private const string NOTEHISTORY_URL = ROOT_URL + "/note_versions";
        private const string NOTEHISTORY_PAGE_URL = NOTEHISTORY_URL + "?page=";
        private const string NOTEHISTORY_PAGE_JSON_URL = NOTEHISTORY_URL + ".json?page=";

        #endregion Consts
        
        private readonly string _login;
        private readonly string _apiKey;

        #region Constructors

        public DanbooruLoader(string login, string apiKey, int loadDelay, HttpClient httpClient = null) : base(httpClient, loadDelay)
        {
            _login = login;
            _apiKey = apiKey;
        }

        #endregion Constructors

        protected override string RootUrl => ROOT_URL;

        #region Methods

        protected override string AddAuth(string url)
        {
            if (!url.Contains("?"))
            {
                url = url + "?";
            }

            return url + $"&login={_login}&api_key={_apiKey}";
        }
        
        private async Task<SearchResult> LoadSearchResultAsync(string tagsString, int? limit)
        {
            var pageHtml = await LoadPageAsync(SEARCH_JSON 
                + WebUtility.UrlEncode(tagsString) 
                + (limit.HasValue? $"&limit={limit.Value}" : string.Empty));

            var results = JsonConvert.DeserializeObject<List<Model.Danbooru.Json.Post>>(pageHtml);

            var searchResult = new DanbooruSearchResult(results);

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

            var updates = DanbooruPostUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        private async Task<List<PostUpdateEntry>> LoadTagHistoryAfterAsync(int id)
        {
            var url = POSTHISTORY_AFTER_URL + id;
            var pageHtml = await LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = DanbooruPostUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        private async Task<List<NoteUpdateEntry>> LoadNoteHistoryPageAsync(string page)
        {
            var url = NOTEHISTORY_PAGE_JSON_URL + page;
            var pageHtml = await LoadPageAsync(url).ConfigureAwait(false);

            var updates = JsonConvert.DeserializeObject<List<Model.Danbooru.Json.Version>>(pageHtml);

            return updates.Select(x => new NoteUpdateEntry { Date = DateTime.Parse(x.created_at), PostId = x.id }).ToList();
        }

        #endregion Methods

        #region IBooruLoader members

        public Post LoadPost(int postId) 
            => Task.Run(async () => await LoadPostAsync(postId)).Result;

        public async Task<Post> LoadPostAsync(int postId)
        {
            var postHtml = await LoadPageAsync(POST_URL + postId).ConfigureAwait(false);
            var postJson = JsonConvert.DeserializeObject<Model.Danbooru.Json.Post>(await LoadPageAsync(string.Format(POST_JSON, postId)).ConfigureAwait(false));

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(postHtml);

            var post = new DanbooruPost(postId, htmlDoc.DocumentNode, postJson);
            return post;
        }

        public SearchResult LoadSearchResult(string tagsString) 
            => LoadSearchResultAsync(tagsString).Result;

        public async Task<SearchResult> LoadSearchResultAsync(string tagsString)
        {
            return await LoadSearchResultAsync(tagsString, null).ConfigureAwait(false);
        }
        
        public List<NoteUpdateEntry> LoadNotesHistory(DateTime lastUpdateTime) 
            => LoadNotesHistoryAsync(lastUpdateTime).Result;

        public async Task<List<NoteUpdateEntry>> LoadNotesHistoryAsync(DateTime lastUpdateTime)
        {
            var booruName = "Danbooru";

            var result = new List<NoteUpdateEntry>();

            int failedCounter = 0;
            int lastId = Int32.MaxValue;

            do
            {
                try
                {
                    var historyPack = await LoadNoteHistoryPageAsync("b" + lastId).ConfigureAwait(false);
                    result.AddRange(historyPack);
                    Logger.Info($"{booruName} | Parsing notes history | Status: PARSING | History page parsed before #{lastId}");
                    lastId = result.Last().PostId;
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

        public List<PostUpdateEntry> LoadTagHistoryUpTo(DateTime toDate) 
            => LoadTagHistoryUpToAsync(toDate).Result;

        public async Task<List<PostUpdateEntry>> LoadTagHistoryUpToAsync(DateTime toDate)
        {
            var result = new List<PostUpdateEntry>();

            var nextLoadPage = 1;
            var failedCounter = 0;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextLoadPage).ConfigureAwait(false);
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

        public List<PostUpdateEntry> LoadTagHistoryFrom(int fromId) 
            => LoadTagHistoryFromAsync(fromId).Result;

        public async Task<List<PostUpdateEntry>> LoadTagHistoryFromAsync(int fromId)
        {
            var booruName = "Danbooru";

            var lastUpdateId = fromId;
            var failedCounter = 0;
            var result = new List<PostUpdateEntry>();
            var continueFlag = true;

            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryAfterAsync(lastUpdateId).ConfigureAwait(false);
                    result.InsertRange(0, historyPack);

                    Logger.Info($"{booruName} | Parsing tags history | Status: PARSING | History page parsed after #{lastUpdateId}");

                    var firstElement = result.FirstOrDefault();

                    if (firstElement == null)
                    {
                        break;
                    }

                    lastUpdateId = firstElement.UpdateId;
                    failedCounter = 0;
                    continueFlag = historyPack.Count >= 20;
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
            while (continueFlag);
            return result;
        }

        public List<PostUpdateEntry> LoadFirstTagHistoryPage() 
            => LoadFirstTagHistoryPageAsync().Result;

        public async Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync()
        {
            return await LoadTagHistoryPageAsync().ConfigureAwait(false);
        }

        #endregion IBooruLoader members
    }
}