using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Sankaku;
using NLog;

namespace Imouto.BooruParser.Controllers
{
    public class SankakuLoader : AbstractBooruLoader, IBooruAsyncLoader
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
        
        #region Constructors

        public SankakuLoader(string login, string passHash, int loadDelay, HttpClient httpClient = null) 
            : base(httpClient, loadDelay)
        {
            _login = login;
            _passwordhash = passHash;
            LoginCookie = $"login={_login};pass_hash={_passwordhash}";
        }

        #endregion Constructors
        
        protected override string RootUrl => SANKAKU_ROOT_URL;

        protected override string AddAuth(string url) => url;

        #region Methods

        private async Task<List<PostUpdateEntry>> LoadTagHistoryPageAsync(int? beforeId = null)
        {
            var url = (beforeId == null)
                      ? SANKAKU_POSTHISTORY_URL
                      : SANKAKU_POSTHISTORY_BEFORE_URL + (beforeId.Value);
            var pageHtml = await LoadPageAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var updates = SankakuPostUpdateEntry.Parse(htmlDoc);

            return updates;
        }

        #endregion Methods

        #region IBooruLoader members

        public async Task<Post> LoadPostAsync(int postId)
        {
            var pageHtml = await LoadPageAsync(SANKAKU_POST_URL + postId);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageHtml);

            var post = new SankakuPost(postId, htmlDoc.DocumentNode);
            return post;
        }

        public async Task<SearchResult> LoadSearchResultAsync(string tagsString)
        {
            var pageHtml = await LoadPageAsync(SANKAKU_SEARCH_URL + WebUtility.UrlEncode(tagsString));

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
            var pageHtml = await LoadPageAsync(url);

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
