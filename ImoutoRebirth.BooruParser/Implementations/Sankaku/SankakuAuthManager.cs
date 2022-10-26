using System.IdentityModel.Tokens.Jwt;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.BooruParser.Implementations.Sankaku;

public class SankakuAuthManager : ISankakuAuthManager
{
    private const string Key = "sankaku_complex_tokens";
    private const string BaseUrl = "https://capi-v2.sankakucomplex.com/";

    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<SankakuSettings> _options;
    private readonly IFlurlClient _flurlClient;

    public SankakuAuthManager(
        IMemoryCache memoryCache,
        IOptions<SankakuSettings> options,
        IFlurlClientFactory factory)
    {
        _memoryCache = memoryCache;
        _options = options;
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
        await _options.Value.SaveTokensCallbackAsync(tokens);
    }

    private Task<(string newAccessToken, string newRefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
        // https://capi-v2.sankakucomplex.com/auth/token?lang=en
        // var response = await _flurlClient.Request("auth", "token")
        //     .PostJsonAsync(new { refresh_token = refreshToken })
        //     .ReceiveJson<SankakuRefreshResponse>();
        //
        // return (response.AccessToken, response.RefreshToken);
    }
}
