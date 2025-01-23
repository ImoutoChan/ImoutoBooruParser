using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using HtmlAgilityPack;

namespace Imouto.BooruParser.Extensions;

public static class FlurlExtensions
{
    public static async Task<HtmlDocument> GetHtmlDocumentAsync(
        this IFlurlRequest request,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var str = await request.SendAsync(HttpMethod.Get, null, completionOption, cancellationToken).ReceiveString();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(str);
        return doc;
    }

    public static IFlurlClient GetForDomain(this IFlurlClientCache cache, Url baseUrl)
        => cache.GetOrAdd(baseUrl, baseUrl);
}
