using System.IdentityModel.Tokens.Jwt;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Imouto.BooruParser.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Imouto.BooruParser.Implementations.Sankaku;

public class SankakuAuthManager : ISankakuAuthManager
{
    private const string Key = "sankaku_complex_tokens";
    private const string BaseUrl = "https://capi-v2.sankakucomplex.com/";

    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<SankakuSettings> _options;
    private readonly IFlurlClientFactory _factory;
    private readonly IFlurlClient _flurlClient;

    public SankakuAuthManager(
        IMemoryCache memoryCache,
        IOptions<SankakuSettings> options,
        IFlurlClientFactory factory)
    {
        _memoryCache = memoryCache;
        _options = options;
        _factory = factory;
        _flurlClient = factory.Get(new Url(BaseUrl));
    }

    public async ValueTask<string?> GetTokenAsync()
    {
        var tokens = GetTokens();

        if (tokens.AccessToken is null)
            return null;
        
        if (!IsExpired(tokens.AccessToken))
            return tokens.AccessToken;

        if (tokens.RefreshToken == null)
            return null;
        
        var (accessToken, refreshToken) = await RefreshTokenAsync(tokens.RefreshToken);
        await SaveTokensAsync(new Tokens(accessToken, refreshToken));

        return accessToken;
    }

    public async Task<IReadOnlyCollection<FlurlCookie>> GetSankakuChannelSessionAsync()
    {
        if (_options.Value.Login is null || _options.Value.Password is null)
            return Array.Empty<FlurlCookie>();
        
        var client = _factory.Get(new Url("https://chan.sankakucomplex.com"))
            .WithHeader("Connection", "keep-alive")
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
            .WithHeader("Accept-Encoding", "gzip, deflate, br")
            .WithHeader("Accept-Language", "en");

        var doc = await client
            .Request("user", "login")
            .GetHtmlDocumentAsync();

        var authenticityToken = doc.DocumentNode.SelectNodes("//form")
            .First(x => x.Attributes["action"].Value == "/en/user/authenticate")
            .SelectSingleNode("input[@name='authenticity_token']")
            .Attributes["value"].Value;

        IReadOnlyList<FlurlCookie>? cookies = null;
        var response = await client
            .Request("en", "user", "authenticate")
            .OnRedirect(x => cookies = x.Response.Cookies)
            .PostUrlEncodedAsync(new Dictionary<string, string>()
            {
                { "authenticity_token", authenticityToken },
                { "url", "" },
                { "user[name]", _options.Value.Login! },
                { "user[password]", _options.Value.Password! },
                { "commit", "Login" }
            });

        return cookies!;
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

    private Tokens GetTokens()
    {
        var cached = _memoryCache.Get<Tokens?>(Key);

        var access = cached != null ? cached.AccessToken : _options.Value.AccessToken;
        var refresh = cached != null ? cached.RefreshToken : _options.Value.RefreshToken;
        
        return new(access, refresh);
    }

    private async Task SaveTokensAsync(Tokens tokens)
    {
        _memoryCache.Set(Key, tokens);
        var task = _options.Value.SaveTokensCallbackAsync?.Invoke(tokens);
        if (task != null)
            await task;
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
}
