// See https://aka.ms/new-console-template for more information

using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Danbooru;
using Microsoft.Extensions.Options;

Console.WriteLine("Hello, World!");

// var services = new ServiceCollection();
// services.AddBooruParsers();

var loader = new DanbooruApiLoader(
    new FlurlClientCache(),
    Options.Create(new DanbooruSettings()
    {
        BotUserAgent = "SampleUserAgent/v1"
    }));

var post = await loader.GetPostAsync(5628013);
Console.WriteLine(post.Id.Md5Hash);
