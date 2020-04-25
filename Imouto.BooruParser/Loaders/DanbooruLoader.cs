using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Danbooru;
using Imouto.BooruParser.Model.Danbooru.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Post = Imouto.BooruParser.Model.Base.Post;

namespace Imouto.BooruParser.Loaders
{
    public class DanbooruLoader: IBooruLoader, IBooruAsyncLoader
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger<DanbooruLoader>();

        #region Consts

        private const string HOST = "danbooru.donmai.us";
        private const string ROOT_URL = "https://danbooru.donmai.us";
        private const string POST_URL = ROOT_URL + "/posts/";
        private const string POST_JSON = ROOT_URL + "/posts/{0}.json";
        private const string SEARCH_JSON = ROOT_URL + "/posts.json?utf8=✓&tags=";
        private const string POPULAR_JSON = ROOT_URL + "/explore/posts/popular.json?date=";

        private const string POSTHISTORY_URL = ROOT_URL + "/post_versions";
        private const string POSTHISTORY_JSON_URL = ROOT_URL + "/post_versions.json";
        private const string POSTHISTORY_PAGE_URL = POSTHISTORY_URL + "?page=";
        private const string POSTHISTORY_PAGE_JSON_URL = POSTHISTORY_JSON_URL + "?page=";
        private const string POSTHISTORY_AFTER_URL = POSTHISTORY_URL + "?page=a";
        private const string POSTHISTORY_AFTER_JSON_URL = POSTHISTORY_JSON_URL + "?page=a";

        private const string NOTEHISTORY_URL = ROOT_URL + "/note_versions";
        private const string NOTEHISTORY_PAGE_URL = NOTEHISTORY_URL + "?page=";
        private const string NOTEHISTORY_PAGE_JSON_URL = NOTEHISTORY_URL + ".json?page=";

        #endregion Consts
        
        private readonly string _login;
        private readonly string _apiKey;
        private readonly BooruLoader _booruLoader;
        private const string BooruName = "Danbooru";

        #region Constructors

        public DanbooruLoader(string login, string apiKey, int loadDelay, HttpClient httpClient = null, BooruLoader booruLoader = null)
        {
            _login = login;
            _apiKey = apiKey;
            _booruLoader = booruLoader ?? new BooruLoader(httpClient, loadDelay, customUrlTramsform: AddAuth);
        }

        #endregion Constructors
        

        #region IBooruLoader members

        public Post LoadPost(int postId) 
            => Task.Run(async () => await LoadPostAsync(postId)).Result;

        public async Task<Post> LoadPostAsync(int postId)
        {
            var postHtml = await _booruLoader.LoadPageAsync(POST_URL + postId).ConfigureAwait(false);
            var postJsonString = await _booruLoader.LoadPageAsync(string.Format(POST_JSON, postId)).ConfigureAwait(false);
            var postJson = JsonConvert.DeserializeObject<Model.Danbooru.Json.Post>(postJsonString);

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
            var result = new List<NoteUpdateEntry>();

            int failedCounter = 0;
            int lastId = Int32.MaxValue;

            using (Logger.BeginScope("Loading notes history for {BooruName}", BooruName))
            {
                do
                {
                    try
                    {
                        var historyPack = await LoadNoteHistoryPageAsync("b" + lastId).ConfigureAwait(false);
                        result.AddRange(historyPack);
                        Logger.LogTrace("Status: LOADING | Notes page parsed before #{LastId}", lastId);
                        lastId = result.Last().PostId;
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
                } while (result.LastOrDefault()?.Date > lastUpdateTime);
            }

            return result;
        }

        public List<PostUpdateEntry> LoadTagHistoryUpTo(DateTime toDate) 
            => LoadTagHistoryUpToAsync(toDate).Result;

        public async Task<List<PostUpdateEntry>> LoadTagHistoryUpToAsync(DateTime toDate)
        {
            var result = new List<PostUpdateEntry>();

            var nextLoadPage = 1;
            var failedCounter = 0;
            var lastResultUpdateDateTime = DateTime.MaxValue;
            do
            {
                try
                {
                    var historyPack = await LoadTagHistoryPageAsync(nextLoadPage).ConfigureAwait(false);

                    if (!historyPack.Any())
                    {
                        throw new Exception("Empty results were received");
                    }

                    result.AddRange(historyPack);

                    nextLoadPage++;

                    lastResultUpdateDateTime = result.Last().UpdateDateTime;
                }
                catch (Exception e)
                {
                    failedCounter++;

                    if (failedCounter > 5)
                    {
                        Logger.LogError(e, $"Tag history loading failed after {failedCounter} tries.");
                        throw;
                    }
                }
            }
            while (lastResultUpdateDateTime > toDate);

            return result;
        }

        public List<PostUpdateEntry> LoadTagHistoryFrom(int fromId) 
            => LoadTagHistoryFromAsync(fromId).Result;

        public async Task<List<PostUpdateEntry>> LoadTagHistoryFromAsync(int fromId)
        {
            var lastUpdateId = fromId;
            var failedCounter = 0;
            var result = new List<PostUpdateEntry>();
            var continueFlag = true;

            using (Logger.BeginScope("Loading tags history for {BooruName}", BooruName))
            {
                do
                {
                    try
                    {
                        var historyPack = await LoadTagHistoryAfterAsync(lastUpdateId).ConfigureAwait(false);
                        result.InsertRange(0, historyPack);

                        Logger.LogTrace("Status: LOADING | History page parsed after #{LastId}", lastUpdateId);

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
                        Logger.LogWarning(e, "Status: ERROR | Exceptions count: #{FailedCounter}", failedCounter);

                        if (failedCounter > 3)
                        {
                            Logger.LogError(e, "Status: TERMINATED");
                            break;
                        }
                    }
                }
                while (continueFlag);
            }
            return result;
        }

        public List<PostUpdateEntry> LoadFirstTagHistoryPage() 
            => LoadFirstTagHistoryPageAsync().Result;

        public async Task<List<PostUpdateEntry>> LoadFirstTagHistoryPageAsync()
        {
            return await LoadTagHistoryPageAsync().ConfigureAwait(false);
        }

        public async Task<SearchResult> LoadPopularAsync(PopularType type)
        {
            var popularString = GetPopularString(type);
            var pageHtml = await _booruLoader.LoadPageAsync(POPULAR_JSON + popularString);

            var results = JsonConvert.DeserializeObject<List<Model.Danbooru.Json.Post>>(pageHtml);

            var searchResult = new DanbooruSearchResult(results);

            return searchResult;
        }

        #endregion IBooruLoader members

        private string GetPopularString(PopularType type)
        {
            string scale;
            switch (type)
            {
                case PopularType.Day:
                    scale = "day";
                    break;
                case PopularType.Week:
                    scale = "week";
                    break;
                case PopularType.Month:
                    scale = "month";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return WebUtility.UrlEncode($"{DateTimeOffset.Now.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffzzz}") 
                   + $"&scale={scale}";
        }

        private string AddAuth(string url)
        {
            if (!url.Contains("?"))
            {
                url = url + "?";
            }

            return url + $"&login={_login}&api_key={_apiKey}";
        }

        private async Task<SearchResult> LoadSearchResultAsync(string tagsString, int? limit)
        {
            var pageHtml = await _booruLoader.LoadPageAsync(SEARCH_JSON
                + WebUtility.UrlEncode(tagsString)
                + (limit.HasValue ? $"&limit={limit.Value}" : string.Empty));

            var results = JsonConvert.DeserializeObject<List<Model.Danbooru.Json.Post>>(pageHtml);

            var searchResult = new DanbooruSearchResult(results);

            return searchResult;
        }

        private async Task<List<PostUpdateEntry>> LoadTagHistoryPageAsync(int? page = null)
        {
            var url = (page == null)
                      ? POSTHISTORY_JSON_URL
                      : POSTHISTORY_PAGE_JSON_URL + (page.Value);
            var json = await _booruLoader.LoadPageAsync(url);

            var versions = JsonConvert.DeserializeObject<IReadOnlyCollection<PostVersion>>(json);
            
            return DanbooruPostUpdateEntry.GetFromJson(versions);
        }

        private async Task<List<PostUpdateEntry>> LoadTagHistoryAfterAsync(int id)
        {
            var url = POSTHISTORY_AFTER_JSON_URL + id;
            var json = await _booruLoader.LoadPageAsync(url);

            var versions = JsonConvert.DeserializeObject<IReadOnlyCollection<PostVersion>>(json);
            
            return DanbooruPostUpdateEntry.GetFromJson(versions);
        }

        private async Task<List<NoteUpdateEntry>> LoadNoteHistoryPageAsync(string page)
        {
            var url = NOTEHISTORY_PAGE_JSON_URL + page;
            var pageHtml = await _booruLoader.LoadPageAsync(url).ConfigureAwait(false);

            var updates = JsonConvert.DeserializeObject<List<Model.Danbooru.Json.Version>>(pageHtml);

            return updates.Select(x => new NoteUpdateEntry
            {
                Date = DateTime.Parse(x.created_at), 
                PostId = x.id
            }).ToList();
        }
    }
}