using FluentAssertions;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using ImoutoRebirth.BooruParser.Implementations.Sankaku;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace ImoutoRebirth.BooruParser.Tests;

public class SankakuAuthManagerTests
{
    private const string ExpiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxNTE2MjM5MDIyfQ.E9bQ6QAil4HpH825QC5PtjNGEDQTtMpcj0SO2W8vmag";
    private const string NonExpiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjQ3NzE1OCwic3ViTHZsIjowLCJpc3MiOiJodHRwczovL2NhcGktdjIuc2Fua2FrdWNvbXBsZXguY29tIiwidHlwZSI6IkJlYXJlciIsImF1ZCI6ImNvbXBsZXgiLCJzY29wZSI6ImNvbXBsZXgiLCJpYXQiOjE2NjY3NTE0NDcsImV4cCI6MTk3NjkzNDI0N30.WvPanPsDYpteA2yy_tzluKKCTEe1CBd8CYXvbKOUWeA";

    [Fact]
    public async Task ShouldReturnRefreshedToken()
    {
        // arrange
        var newAccessToken = Guid.NewGuid().ToString();
        var newRefreshToken = Guid.NewGuid().ToString();
        
        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { access_token = newAccessToken, refresh_token = newRefreshToken });
        
        var cacheMock = new MemoryCache(new MemoryCacheOptions());

        var simpleOptions = Options.Create(
            new SankakuSettings()
            {
                AccessToken = ExpiredToken,
                RefreshToken = "refresh-token"
            });
        
        var manager = new SankakuAuthManager(
            cacheMock, 
            simpleOptions, 
            new PerBaseUrlFlurlClientFactory());

        // act
        var token = await manager.GetTokenAsync();
        
        // assert
        token.Should().Be(newAccessToken);
    }

    [Fact]
    public async Task ShouldReturnNonExpiredToken()
    {
        // arrange
        var cacheMock = new MemoryCache(new MemoryCacheOptions());
        var simpleOptions = Options.Create(
            new SankakuSettings
            {
                AccessToken = NonExpiredToken,
                RefreshToken = "refresh-token"
            });
        
        var manager = new SankakuAuthManager(
            cacheMock, 
            simpleOptions, 
            new PerBaseUrlFlurlClientFactory());

        // act
        var token = await manager.GetTokenAsync();
        
        // assert
        token.Should().Be(NonExpiredToken);
    }

    [Fact]
    public async Task ShouldReturnNonExpiredTokenFromMemoryCache()
    {
        const string refreshToken = "refresh-token";
        
        // arrange
        var cacheMock = new MemoryCache(new MemoryCacheOptions());
        cacheMock.Set("sankaku_complex_tokens", new Tokens(NonExpiredToken, refreshToken));
        
        var simpleOptions = Options.Create(
            new SankakuSettings()
            {
                AccessToken = "1",
                RefreshToken = "2"
            });
        
        var manager = new SankakuAuthManager(
            cacheMock, 
            simpleOptions, 
            new PerBaseUrlFlurlClientFactory());

        // act
        var token = await manager.GetTokenAsync();
        
        // assert
        token.Should().Be(NonExpiredToken);
    }

    [Fact]
    public async Task ShouldCallSaveTokensCallback()
    {
        // arrange
        var newAccessToken = Guid.NewGuid().ToString();
        var newRefreshToken = Guid.NewGuid().ToString();
        var savedAccessToken = "";
        var savedRefreshToken = "";
        
        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(new { access_token = newAccessToken, refresh_token = newRefreshToken });
        
        var cacheMock = new MemoryCache(new MemoryCacheOptions());

        var simpleOptions = Options.Create(
            new SankakuSettings()
            {
                AccessToken = ExpiredToken,
                RefreshToken = "refresh-token",
                SaveTokensCallbackAsync = tokens =>
                {
                    savedAccessToken = tokens.AccessToken;
                    savedRefreshToken = tokens.RefreshToken;
                    return Task.CompletedTask;
                }
            });
        
        var manager = new SankakuAuthManager(
            cacheMock, 
            simpleOptions, 
            new PerBaseUrlFlurlClientFactory());

        // act
        await manager.GetTokenAsync();
        
        // assert
        savedAccessToken.Should().Be(newAccessToken);
        savedRefreshToken.Should().Be(newRefreshToken);
    }
}
