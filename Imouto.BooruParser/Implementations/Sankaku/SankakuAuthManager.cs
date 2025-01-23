using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Sankaku;

public class SankakuAuthManager : ISankakuAuthManager
{
    private const string TokensKey = "sankaku_complex_tokens";
    private const string SessionKey = "sankaku_complex_session";
    private const string BaseUrl = "https://capi-v2.sankakucomplex.com/";

    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<SankakuSettings> _options;
    private readonly IFlurlClientCache _factory;
    private readonly IFlurlClient _flurlClient;

    public SankakuAuthManager(
        IMemoryCache memoryCache,
        IOptions<SankakuSettings> options,
        IFlurlClientCache factory)
    {
        _memoryCache = memoryCache;
        _options = options;
        _factory = factory;
        _flurlClient = factory.GetOrAdd(new Url(BaseUrl), new Url(BaseUrl));
    }

    public async ValueTask<string?> GetTokenAsync()
    {
        var tokens = await GetTokensAsync();

        if (tokens?.AccessToken is null)
            return null;
        
        if (!IsExpired(tokens.AccessToken))
            return tokens.AccessToken;

        if (tokens.RefreshToken == null)
            return null;
        
        var (accessToken, refreshToken) = await RefreshTokenAsync(tokens.RefreshToken);
        _memoryCache.Set(TokensKey, new Tokens(accessToken, refreshToken));

        return accessToken;
    }

    public async Task<IReadOnlyCollection<FlurlCookie>> GetSankakuChannelSessionAsync()
    {
        if (_options.Value.Login is null || _options.Value.Password is null)
            return Array.Empty<FlurlCookie>();

        var found = _memoryCache.Get<IReadOnlyCollection<FlurlCookie>>(SessionKey);
        if (found != null)
            return found;
        
        var factory = _factory;
        var login = _options.Value.Login;
        var password = _options.Value.Password;
        
        var cookieJar = new CookieJar();
        
        var loginClient = factory.GetOrAdd("https://login.sankakucomplex.com", "https://login.sankakucomplex.com")
            .WithHeader("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"")
            .WithHeader("sec-ch-ua-mobile", "?0")
            .WithHeader("sec-ch-ua-platform", "\"Windows\"")
            .WithHeader("DNT", "1")
            .WithHeader("Upgrade-Insecure-Requests", "1")
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36")
            .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
            .WithHeader("Sec-Fetch-Site", "none")
            .WithHeader("Sec-Fetch-Mode", "navigate")
            .WithHeader("Sec-Fetch-User", "?1")
            .WithHeader("Sec-Fetch-Dest", "document")
            .WithHeader("Accept-Language", "en");

        var responses = new List<IFlurlResponse>();
        
        // https://login.sankakucomplex.com/oidc/auth?response_type=code&scope=openid&client_id=sankaku-web-app&redirect_uri=https://sankaku.app/sso/callback&state=return_uri=https://sankaku.app/&theme=black&route=login&lang=en
        var result = await loginClient
            .Request("oidc", "auth")
            .WithCookies(cookieJar)
            .SetQueryParam("response_type", "code")
            .SetQueryParam("scope", "openid")
            .SetQueryParam("client_id", "sankaku-channel-legacy")
            .SetQueryParam("redirect_uri", "https://chan.sankakucomplex.com/sso/callback")
            .SetQueryParam("route", "login")
            .OnRedirect(x => responses.Add(x.Response))
            .GetAsync();
        
        // https://login.sankakucomplex.com/user/mfa-state
        var result1 = await loginClient
            .Request("user", "mfa-state")
            .WithCookies(cookieJar)
            .PostJsonAsync(new
            {
                login = login,
                password = password,
                browserInfo = "Chrome"
            });
        
        // https://login.sankakucomplex.com/auth/token
        var result2 = await loginClient
            .Request("auth", "token")
            .WithCookies(cookieJar)
            .PostJsonAsync(new
            {
                login = login,
                password = password,
            })
            .ReceiveJson<SankakuTokenResponse>();
        

        var interactionId = responses.First().Headers
            .First(x => x.Name == "Location").Value.ToString()!
            .Split('?')[0]
            .Split('/').Last();

        
        var interactionResponses = new List<IFlurlResponse>();
        // https://login.sankakucomplex.com/oidc/interaction/<>/login
        var result3 = await loginClient
            .Request("oidc", "interaction", interactionId, "login")
            .WithCookies(cookieJar)
            .OnRedirect(x => interactionResponses.Add(x.Response))
            .PostUrlEncodedAsync(new
            {
                access_token = result2.AccessToken,
                state = "lang=en&theme=black",
            });

        _memoryCache.Set(SessionKey, interactionResponses[2].Cookies as IReadOnlyCollection<FlurlCookie>);
        return interactionResponses[2].Cookies;
    }

    private static bool IsExpired(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(accessToken);
        var claims = jwtSecurityToken.Claims.ToList();
        var expired = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims.First(x => x.Type == "exp").Value));
        var isExpired = expired - DateTimeOffset.UtcNow < TimeSpan.FromHours(1);
        return isExpired;
    }

    private async ValueTask<Tokens?> GetTokensAsync()
    {
        var cached = _memoryCache.Get<Tokens?>(TokensKey);

        if (cached != null)
            return new(cached.AccessToken!, cached.RefreshToken!);
        
        if (_options.Value.Login is null || _options.Value.Password is null)
            return null;
        
        var newTokens = await SankakuFullLoginAsync();
        _memoryCache.Set(TokensKey, newTokens);
        
        return newTokens;
    }

    private async Task<(string newAccessToken, string newRefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        // throw new NotImplementedException();
        // https://capi-v2.sankakucomplex.com/auth/token?lang=en
        var response = await _flurlClient.Request("auth", "token")
            .PostJsonAsync(new { refresh_token = refreshToken })
            .ReceiveJson<SankakuRefreshResponse>();
        
        return (response.AccessToken, response.RefreshToken);
    }

    private async Task<Tokens> SankakuFullLoginAsync()
    {
        var factory = _factory;
        var login = _options.Value.Login;
        var password = _options.Value.Password;
        
        var cookieJar = new CookieJar();
        
        var loginClient = factory.GetOrAdd("https://login.sankakucomplex.com", "https://login.sankakucomplex.com")
            .WithHeader("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"")
            .WithHeader("sec-ch-ua-mobile", "?0")
            .WithHeader("sec-ch-ua-platform", "\"Windows\"")
            .WithHeader("DNT", "1")
            .WithHeader("Upgrade-Insecure-Requests", "1")
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36")
            .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
            .WithHeader("Sec-Fetch-Site", "none")
            .WithHeader("Sec-Fetch-Mode", "navigate")
            .WithHeader("Sec-Fetch-User", "?1")
            .WithHeader("Sec-Fetch-Dest", "document")
            .WithHeader("Accept-Language", "en");
        
        var capiClient = factory.GetOrAdd("https://capi-v2.sankakucomplex.com", "https://capi-v2.sankakucomplex.com")
            .WithHeader("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"")
            .WithHeader("sec-ch-ua-mobile", "?0")
            .WithHeader("sec-ch-ua-platform", "\"Windows\"")
            .WithHeader("DNT", "1")
            .WithHeader("Upgrade-Insecure-Requests", "1")
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36")
            .WithHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
            .WithHeader("Sec-Fetch-Site", "none")
            .WithHeader("Sec-Fetch-Mode", "navigate")
            .WithHeader("Sec-Fetch-User", "?1")
            .WithHeader("Sec-Fetch-Dest", "document")
            .WithHeader("Accept-Language", "en");

        var responses = new List<IFlurlResponse>();
        
        // https://login.sankakucomplex.com/oidc/auth?response_type=code&scope=openid&client_id=sankaku-web-app&redirect_uri=https://sankaku.app/sso/callback&state=return_uri=https://sankaku.app/&theme=black&route=login&lang=en
        var result = await loginClient
            .Request("oidc", "auth")
            .WithCookies(cookieJar)
            .SetQueryParam("response_type", "code")
            .SetQueryParam("scope", "openid")
            .SetQueryParam("client_id", "sankaku-web-app")
            .SetQueryParam("redirect_uri", "https://sankaku.app/sso/callback")
            .SetQueryParam("state", "return_uri=https://sankaku.app/")
            .SetQueryParam("theme", "black")
            .SetQueryParam("route", "login")
            .SetQueryParam("lang", "en")
            .OnRedirect(x => responses.Add(x.Response))
            .GetAsync();
        
        // https://login.sankakucomplex.com/user/mfa-state
        var result1 = await loginClient
            .Request("user", "mfa-state")
            .WithCookies(cookieJar)
            .PostJsonAsync(new
            {
                login = login,
                password = password,
                browserInfo = "Chrome"
            });
        
        // https://login.sankakucomplex.com/auth/token
        var result2 = await loginClient
            .Request("auth", "token")
            .WithCookies(cookieJar)
            .PostJsonAsync(new
            {
                login = login,
                password = password,
            })
            .ReceiveJson<SankakuTokenResponse>();
        

        var interactionId = responses.First().Headers
            .First(x => x.Name == "Location").Value.ToString()!
            .Split('?')[0]
            .Split('/').Last();

        
        var interactionResponses = new List<IFlurlResponse>();
        // https://login.sankakucomplex.com/oidc/interaction/<>/login
        var result3 = await loginClient
            .Request("oidc", "interaction", interactionId, "login")
            .WithCookies(cookieJar)
            .OnRedirect(x => interactionResponses.Add(x.Response))
            .PostUrlEncodedAsync(new
            {
                access_token = result2.AccessToken,
                state = "lang=en&theme=black&return_uri=https://sankaku.app/",
            });
        
        var code = interactionResponses.Last().Headers
            .First(x => x.Name == "Location").Value.ToString()!
            .Split('?')[1]
            .Split('&')[0]
            .Split('=')[1];
        
        // https://capi-v2.sankakucomplex.com/sso/finalize?lang=en
        var result4 = await capiClient
            .Request("sso", "finalize")
            .WithCookies(cookieJar)
            .SetQueryParam("lang", "en")
            .PostJsonAsync(new
            {
                code = code,
                client_id = "sankaku-web-app",
                redirect_uri = "https://sankaku.app/sso/callback",
            })
            .ReceiveJson<SankakuTokenResponse>();

        return new(result4.AccessToken, result4.RefreshToken);
    }
    
    private record SankakuTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken, 
        [property: JsonPropertyName("refresh_token")] string RefreshToken);
}
