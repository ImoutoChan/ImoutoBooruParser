# Imouto.BooruParser

[![AppVeyor](https://img.shields.io/appveyor/ci/ImoutoChan/imoutobooruparser.svg?style=flat-square)](https://ci.appveyor.com/project/ImoutoChan/imoutobooruparser)
[![NuGet](https://img.shields.io/nuget/v/Imouto.BooruParser.svg?style=flat-square)](https://www.nuget.org/packages/Imouto.BooruParser/)
[![license](https://img.shields.io/github/license/ImoutoChan/ImoutoBooruParser.svg?style=flat-square)](https://github.com/ImoutoChan/Imouto.BooruParser)

The .net standart library for parsing data from booru sites ([chan.sankakucomplex.com](https://chan.sankakucomplex.com), [danbooru.donmai.us](https://danbooru.donmai.us), [yande.re](https://yande.re)).
The library can parse information about individual posts, tags and notes history and can perform a search on this sites.

## Usage

```C#
static async void Run()
{
    var danbooruLoader = new DanbooruLoader("username", "apikey", 1240);
    var post = await danbooruLoader.LoadPostAsync(1);
    Console.WriteLine(post.Md5);
}
```

## Installation

Install as [NuGet package](https://www.nuget.org/packages/Imouto.BooruParser/):

```powershell
Install-Package Imouto.BooruParser
```
