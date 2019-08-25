using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Imouto.BooruParser.Helpers;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Loaders
{
    public class BooruLoader
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger<BooruLoader>();

        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _httpClientSemaphoreSlim = new SemaphoreSlim(1);
        private readonly int _waitMilliseconds;
        private readonly string _loginCookie;
        private readonly Func<string, HttpRequestMessage> _customMessageBuilder;
        private readonly Func<string, string> _customUrlTramsform;
        private DateTimeOffset _lastRequestTime = DateTimeOffset.Now.AddDays(-1);
        private string _lastRequestCookie;

        public BooruLoader(HttpClient httpClient,
                                int loadDelay,
                                string loginCookie = null,
                                Func<string, HttpRequestMessage> customMessageBuilder = null,
                                Func<string, string> customUrlTramsform = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _waitMilliseconds = loadDelay;
            _loginCookie = loginCookie;
            _customMessageBuilder = customMessageBuilder;
            _customUrlTramsform = customUrlTramsform;
        }

        private async Task<T> UseClient<T>(Func<HttpClient, CancellationToken, Task<T>> action, 
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

        public async Task<string> LoadPageAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                url = _customUrlTramsform?.Invoke(url) ?? url;

                var requestMessage = _customMessageBuilder?.Invoke(url) ?? new HttpRequestMessage(HttpMethod.Get, url);
                SetCookie(requestMessage);

                var httpResponse = await UseClient(async (httpClient, cT)
                    => await httpClient.SendAsync(requestMessage, cT), cancellationToken);

                await httpResponse.EnsureSuccessStatusCodeWithResponce();

                _lastRequestCookie = GetCookie(httpResponse);

                var html = await httpResponse.Content.ReadAsStringAsync();

                return html;
            }
            catch (HttpException httpException)
            {
                if (httpException.HttpStatusCode == (HttpStatusCode)421 
                    || httpException.HttpStatusCode == (HttpStatusCode)429)
                {
                    _lastRequestTime = DateTimeOffset.Now.AddSeconds(30);
                }

                Logger.LogError(httpException, "Loading page '{Url}' threw HttpException {HttpStatusCode}", url, httpException.HttpStatusCode);
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Load page '{Url}' threw exception", url);
                throw;
            }
        }

        private string GetCookie(HttpResponseMessage httpResponse)
        {
            var cookie = httpResponse.Headers.FirstOrDefault(x => x.Key == "Cookie");

            if (cookie.Key != null && cookie.Value != null)
            {
                return String.Join(";", cookie.Value) + ';';
            }

            return null;
        }

        private void SetCookie(HttpRequestMessage requestMessage)
        {
            var cookieString = GetCookieString();
            if (cookieString != null)
            {
                requestMessage.Headers.Set("Cookie", cookieString);
            }
        }

        private string GetCookieString()
        {
            var sb = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(_lastRequestCookie))
            {
                sb.Append(_lastRequestCookie);

                if (!_lastRequestCookie.EndsWith(";"))
                {
                    sb.Append(';');
                }
            }
            if (!String.IsNullOrWhiteSpace(_loginCookie))
            {
                sb.Append(_loginCookie);
            }


            return sb.Length > 0 ? sb.ToString() : null;
        }
    }
}
