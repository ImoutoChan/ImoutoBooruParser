# Imouto.BooruParser

[![NuGet](https://img.shields.io/nuget/v/Imouto.BooruParser.svg?style=flat-square)](https://www.nuget.org/packages/Imouto.BooruParser/)
[![license](https://img.shields.io/github/license/ImoutoChan/ImoutoBooruParser.svg?style=flat-square)](https://github.com/ImoutoChan/Imouto.BooruParser)

The dotnet library for retrieving info from booru sites ([chan.sankakucomplex.com](https://chan.sankakucomplex.com), [danbooru.donmai.us](https://danbooru.donmai.us), [yande.re](https://yande.re), [gelbooru.com](https://gelbooru.com), [rule34.xxx](https://rule34.xxx)). You can get individual posts, tags/notes history, and search by tags. Install as [NuGet package](https://www.nuget.org/packages/Imouto.BooruParser/):

```powershell
Install-Package Imouto.BooruParser
```
```xml
<PackageReference Include="Imouto.BooruParser" Version="4.*" />
```

## Quick start (examples)

```csharp
// Danbooru requires BotUserAgent
var danbooru = new DanbooruApiLoader(
    new FlurlClientCache(),
    Options.Create(new DanbooruSettings { BotUserAgent = "SampleUserAgent/v1" }));
var dPost = await danbooru.GetPostAsync(5628013);
Console.WriteLine(dPost.Id.Md5Hash);

// Yandere requires BotUserAgent; optional login/password hash
var yandere = new YandereApiLoader(
    new FlurlClientCache(),
    Options.Create(new YandereSettings { BotUserAgent = "SampleUserAgent/v1" }));
var yPost = await yandere.GetPostAsync(408517);

// Gelbooru requires UserId + ApiKey
var gelbooru = new GelbooruApiLoader(
    new FlurlClientCache(),
    Options.Create(new GelbooruSettings { UserId = 123456, ApiKey = "<your_api_key>", PauseBetweenRequestsInMs = 0 }));
var gPost = await gelbooru.GetPostAsync(7837194);

// Rule34 requires UserId + ApiKey
var rule34 = new Rule34ApiLoader(
    new FlurlClientCache(),
    Options.Create(new Rule34Settings { UserId = 123456, ApiKey = "<your_api_key>", PauseBetweenRequestsInMs = 1000 }));
var rPost = await rule34.GetPostAsync(8548333);

// Sankaku supports string post ids; auth is optional but recommended
// Requires ISankakuAuthManager via DI if you use AddBooruParsers; for manual usage see tests
// Here is a minimal sample without DI (unauthorized):
var sankaku = new SankakuApiLoader(
    new FlurlClientCache(),
    Options.Create(new SankakuSettings { PauseBetweenRequestsInMs = 0 }),
    new SankakuAuthManager(new Microsoft.Extensions.Caching.Memory.MemoryCache(new()), Options.Create(new SankakuSettings()), new FlurlClientCache()));
var sPost = await sankaku.GetPostAsync("jXajkOWmor2");

SearchResult page = await danbooru.SearchAsync("tag1 tag2");
page = await danbooru.GetNextPageAsync(page);
var tagHistory = await danbooru.GetTagHistoryPageAsync(null, limit: 100);
```

## DI registration

```csharp
services.AddBooruParsers();
// For Sankaku auth management add IMemoryCache
services.AddMemoryCache();

// Optional: bind settings from configuration
services.Configure<DanbooruSettings>(Configuration.GetSection("Danbooru"));
services.Configure<GelbooruSettings>(Configuration.GetSection("Gelbooru"));
services.Configure<SankakuSettings>(Configuration.GetSection("Sankaku"));
services.Configure<YandereSettings>(Configuration.GetSection("Yandere"));
services.Configure<Rule34Settings>(Configuration.GetSection("Rule34"));
```

### Settings overview

- DanbooruSettings: 
  - Login, 
  - ApiKey, 
  - BotUserAgent (required), 
  - PauseBetweenRequestsInMs
- YandereSettings: 
  - Login, 
  - PasswordHash, 
  - BotUserAgent (required), 
  - PauseBetweenRequestsInMs
- GelbooruSettings: 
  - UserId (required), 
  - ApiKey (required), 
  - PauseBetweenRequestsInMs
- Rule34Settings: 
  - UserId (required), 
  - ApiKey (required), 
  - PauseBetweenRequestsInMs
- SankakuSettings: 
  - Login, 
  - Password, 
  - PauseBetweenRequestsInMs

`PauseBetweenRequestsInMs`: 
  - 0 — requests can run in parallel; 
  - `>0` — adds a delay between requests to help avoid bans/rate limits.

### Public API surface

Main interface: `IBooruApiLoader`

- GetPostAsync(string postId) / extension GetPostAsync(int) for non-Sankaku loaders
- GetPostByMd5Async(string md5)
- SearchAsync(string tags), GetNextPageAsync, GetPreviousPageAsync
- GetPopularPostsAsync(PopularType)
- GetTagHistoryPageAsync(SearchToken? token, int limit)
- GetNoteHistoryPageAsync(SearchToken? token, int limit)

Additionally, `IBooruApiAccessor` (where supported): 
- FavoritePostAsync

Note: Sankaku uses string post identifiers; int helpers are provided as extensions in `Api.cs`.

## Changelog

### Version 4.2.4
Yandere settings now require your own user agent for your bot

### Version 4.2.3
Rule34 now requires user id and api key, you can get them in settings in your profile page

### Version 4.2.2
Gelbooru now requires user id and api key, you can get them in settings in your profile page

### Version 4.1.1
Fix sankaku api url, update dependencies

### Version 4.1.0
* Search now has a basic navigation function (next/prev pages)

### Version 4.0.0
* I'm bumping the major version due to changes in the public API. Sankaku now uses string Ids for various entities, 
and we have to adjust for it. Most public API types now have a string Id parameter.
* Technical: FluentAssertion has been replaced with AwesomeAssertions, you know why.
* Technical: Flurl has been upgraded to version 4, it has some breaking changes, but I've adjusted for them, we'll see 
how it goes.
* Technical: Upgraded to net9.0

### Version 3.3.0
* Sankaku is child of devil and was broken once again. Removed old auth chan. part as now all logins goes throw beta.

### Version 3.2.0
* Added **Rule34**, they have a lot of tagged pay-walled staff and I will be using them as tag source for this stuff. No auth is required for now.
* Sankaku was broken once again. This time they changed their auth flow on their old site. PassHash no longer works. Without auth you also can't get all of post tags (only 20 general tags). I fixed it (only god know for how long). You have to provide login and password and it should work.

### Version 3.1
Once again sankaku broke everything. It's now recommended to provide login and pass_hash for sankaku in order to receive all tags from posts. Without it you can only get 20 general tags.

### Version 3.0
Have to increment major version due to breaking change in danbooru api usage. Now you have to provide your own user agent for your bot in danbooru settings, otherwise danbooru requests would fail with 403 (recent change on their side).

### Version 2.0 released!

The new version of this library has been released. It's a complete rewrite and has a lot of **breaking changes**. Please **don't update** unless you're ready to spent some time in refactoring and reading.

The new version has gelbooru as partly supported booru. It doesn't provide a way to get tags or notes history, but you can search and retrieve posts by id or md5. To consume this library you should use `IBooruApiLoader` interface.
