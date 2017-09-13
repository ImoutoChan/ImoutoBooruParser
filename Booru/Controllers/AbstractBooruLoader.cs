using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Imouto.BooruParser.Helpers;
using NLog;

namespace Imouto.BooruParser.Controllers
{
    public abstract class AbstractBooruLoader
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _httpClientSemaphoreSlim = new SemaphoreSlim(1);
        private readonly int _waitMilliseconds;
        private DateTimeOffset _lastRequestTime = DateTimeOffset.Now.AddDays(-1);
        private string _lastRequestCookie;

        protected AbstractBooruLoader(HttpClient httpClient, int loadDelay)
        {
            _httpClient = httpClient ?? new HttpClient();
            _waitMilliseconds = loadDelay;
        }

        protected abstract string RootUrl { get; }

        protected string LoginCookie { get; set; }

        protected async Task<T> UseClient<T>(Func<HttpClient, CancellationToken, Task<T>> action, 
                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            await _httpClientSemaphoreSlim.WaitAsync(cancellationToken);

            var timePassed = _lastRequestTime - DateTimeOffset.UtcNow;
            var timePassedMilliseconds = timePassed.Duration().TotalMilliseconds;
            var delayDuration = _waitMilliseconds - Convert.ToInt32(timePassedMilliseconds);

            if (delayDuration > 0)
            {
                await Task.Delay(delayDuration, cancellationToken);
            }

            try
            {
                return await action(_httpClient, cancellationToken);
            }
            finally
            {
                _lastRequestTime = DateTimeOffset.UtcNow;
                _httpClientSemaphoreSlim.Release();
            }
        }

        protected async Task<string> LoadPageAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            var authorizedUrl = AddAuth(url);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, authorizedUrl);
                requestMessage.Version = Version.Parse("1.1");
                requestMessage.Headers.Set("Connection: keep-alive");
                requestMessage.Headers.Set("Cache-Control: max-age=0");
                requestMessage.Headers.Set("Upgrade-Insecure-Requests: 1");
                requestMessage.Headers.Set("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36");
                requestMessage.Headers.Set("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                requestMessage.Headers.Set("DNT: 1");
                requestMessage.Headers.Set($"Referer: {RootUrl}");
                requestMessage.Headers.Set("Accept-Encoding: gzip, deflate");
                requestMessage.Headers.Set("Accept-Language: en-US,en;q=0.8,ru;q=0.6");

                var cookieString = (_lastRequestCookie ?? String.Empty) 
                    + ';' 
                    + (LoginCookie ?? String.Empty);
                requestMessage.Headers.Set("Cookie", cookieString);

                var httpResponse = await UseClient(async (httpClient, cT) 
                    => await httpClient.GetAsync(authorizedUrl, cT), cancellationToken);

                await httpResponse.EnsureSuccessStatusCodeWithResponce();

                var cookie = httpResponse.Headers.FirstOrDefault(x => x.Key == "Cookie");

                _lastRequestCookie = String.Join(";", cookie.Value);

                var html = await httpResponse.Content.ReadAsStringAsync();

                return html;
            }
            catch (HttpException he)
            {
                if (he.HttpStatusCode == (HttpStatusCode)421 
                    || he.HttpStatusCode == (HttpStatusCode)429)
                {
                    _lastRequestTime = DateTimeOffset.Now.AddSeconds(30);
                }

                Logger.Error(he, $"Load page '{url}' threw HttpException ({he.HttpStatusCode})");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Load page '{url}' threw exception");
                throw;
            }

        }

        protected abstract string AddAuth(string url);
    }
}
