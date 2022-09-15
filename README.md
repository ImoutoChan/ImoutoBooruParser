# Imouto.BooruParser

[![AppVeyor](https://img.shields.io/appveyor/ci/ImoutoChan/imoutobooruparser.svg?style=flat-square)](https://ci.appveyor.com/project/ImoutoChan/imoutobooruparser)
[![NuGet](https://img.shields.io/nuget/v/Imouto.BooruParser.svg?style=flat-square)](https://www.nuget.org/packages/Imouto.BooruParser/)
[![license](https://img.shields.io/github/license/ImoutoChan/ImoutoBooruParser.svg?style=flat-square)](https://github.com/ImoutoChan/Imouto.BooruParser)

.NET library for working with posts from booru sites ([chan.sankakucomplex.com](https://chan.sankakucomplex.com), [danbooru.donmai.us](https://danbooru.donmai.us), [yande.re](https://yande.re)). You can retrieve individual posts, tags/notes history, and search by tags.

## Usage

```C#
var username = "user";
var apiKey = "HBArbAk4WcKTSeAfsyBO";
var delayBetweenRequestsInMs = 1240;

var danbooruLoader = new DanbooruLoader(username, apiKey, delayBetweenRequestsInMs);
var post = await danbooruLoader.LoadPostAsync(5628013);
Console.WriteLine(post.Md5);
```

## Installation

Install as [NuGet package](https://www.nuget.org/packages/Imouto.BooruParser/):

```powershell
Install-Package Imouto.BooruParser
```

or

```xml
<PackageReference Include="Imouto.BooruParser" Version="1.17.4" />
```

