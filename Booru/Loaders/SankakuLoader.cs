using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Imouto.BooruParser.Helpers;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Sankaku;
using NLog;

namespace Imouto.BooruParser.Controllers
{
    public class SankakuLoader : IBooruAsyncLoader
    {
        #region Consts

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string SANKAKU_HOST = "chan.sankakucomplex.com";
        private const string SANKAKU_ROOT_URL = "https://chan.sankakucomplex.com";
        private const string SANKAKU_POST_URL = SANKAKU_ROOT_URL + "/post/show/";
        private const string SANKAKU_SEARCH_URL = SANKAKU_ROOT_URL + "/?tags=";
        private const string SANKAKU_POSTHISTORY_URL = SANKAKU_ROOT_URL + "/post_tag_history";
        private const string SANKAKU_POSTHISTORY_BEFORE_URL = SANKAKU_POSTHISTORY_URL + "?before_id=";
        private const string SANKAKU_NOTEHISTORY_URL = SANKAKU_ROOT_URL + "/note/history";
        private const string SANKAKU_NOTEHISTORY_PAGE_URL = SANKAKU_NOTEHISTORY_URL + "?page=";

        #endregion Consts
        
        private readonly string _login;
        private readonly string _passwordhash;
        private readonly BooruLoader _booruLoader;

        #region Constructors

        public SankakuLoader(string login, string passHash, int loadDelay, HttpClient httpClient = null, BooruLoader booruLoader = null) 
        {
            _login = login;
            _passwordhash = passHash;
            _booruLoader = booruLoader 
                ?? new BooruLoader(httpClient, 
                                    loadDelay, 
                                    loginCookie: $"login={_login};pass_hash={_passwordhash};", 
                                    customMessageBuilder: BuildRequestMessage);
        }

        #endregion Constructors

        #region Methods

        private HttpRequestMessage BuildRequestMessage(string url)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            requestMessage.Version = Version.Parse("1.1");
            requestMessage.Headers.Set("Connection: keep-alive");
            requestMessage.Headers.Set("Cache-Control: max-age=0");
            requestMessage.Headers.Set("Upgrade-Insecure-Requests: 1");
            requestMessage.Headers.Set("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
            requestMessage.Headers.Set("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            requestMessage.Headers.Set("DNT: 1");
            requestMessage.Headers.Set($"Referer: {SANKAKU_HOST}");
            requestMessage.Headers.Set("Accept-Encoding: gzip, deflate");
            requestMessage.Headers.Set("Accept-Language: en-US,en;q=0.8,ru;q=0.6");

            return requestMessage;
        }
        private async Task<List<PostUpdateEntry>> LoadTagHistoryPageAsync(int? beforeId = null)
        {
            var url = (beforeId == null)
                      ? SANKAKU_POSTHISTORY_URL
                      : SANKAKU_POSTHISTORY_BEFORE_URL + (beforeId.Value);
            var pageHtml = await _booruLoader.LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = SankakuPostUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        #endregion Methods

        #region IBooruLoader members

        public async Task<Post> LoadPostAsync(int postId)
        {
            var pageHtml = await _booruLoader.LoadPageAsync(SANKAKU_POST_URL + postId);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var post = new SankakuPost(postId, htmlDoc.DocumentNode);
            return post;
        }

        public async Task<SearchResult> LoadSearchResultAsync(string tagsString)
        {
            var pageHtml = await _booruLoader.LoadPageAsync(SANKAKU_SEARCH_URL + WebUtility.UrlEncode(tagsString));

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var searchResult = new SankakuSearchResult(htmlDoc.DocumentNode);

            return searchResult;
        }

        public async Task<List<PostUpdateEntry>> LoadTagHistoryFromAsync(int fromId)
        {
            var booruName = "Sankaku";

            var firstHistoryPage = await LoadFirstTagHistoryPageAsync();
            var currentLast = firstHistoryPage.FirstOrDefault()?.UpdateId;

            var result = new List<PostUpdateEntry>();
            var failedCounter = 0;

            int? nextLoad = fromId + 101;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextLoad);
                    result.InsertRange(0, historyPack);
                    
                    Logger.Info($"{booruName} | Parsing tags history | Status: PARSING | History page parsed before #{nextLoad}");

                    nextLoad = result.First().UpdateId + 101;
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
            while (nextLoad < currentLast + 101);
            return result;
        }

        public Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync()
        {
            return LoadTagHistoryPageAsync();
        }

        private async Task<List<NoteUpdateEntry>> LoadNoteHistoryPageAsync(int page = 1)
        {
            var url = SANKAKU_NOTEHISTORY_PAGE_URL + page;
            var pageHtml = await _booruLoader.LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = SankakuNoteUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        public async Task<List<NoteUpdateEntry>> LoadNotesHistoryAsync(DateTime lastUpdateTime)
        {
            var booruName = "Sankaku";

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

            int? nextLoad = null;
            var failedCounter = 0;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextLoad);
                    result.AddRange(historyPack);

                    nextLoad = result.Last().UpdateId;
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

        #endregion IBooruLoader members
    }
}
