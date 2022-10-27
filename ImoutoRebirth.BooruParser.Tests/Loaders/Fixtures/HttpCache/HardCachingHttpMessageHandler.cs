using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures.HttpCache;

public class HardCachingHttpMessageHandler : DelegatingHandler
{
    private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
    private static readonly object CacheLocker = new();

    static HardCachingHttpMessageHandler()
    {
        var now = DateTimeOffset.Now;
        var fileName = $"{now:yy-MM-dd}-http-cache.json";
        if (!File.Exists(fileName))
            return;
        
        var fileContent = File.ReadAllText(fileName);
        Cache = JsonSerializer.Deserialize<ConcurrentDictionary<string, CacheEntry>>(fileContent)!;
    }

    private static void SaveCache()
    {
        lock (CacheLocker)
        {
            var now = DateTimeOffset.Now;
            File.WriteAllText($"{now:yy-MM-dd}-http-cache.json", JsonSerializer.Serialize(Cache));
        }
    }

    public HardCachingHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Method != HttpMethod.Get && !request.RequestUri!.ToString().Contains("/graphql"))
        {
            return await base.SendAsync(request, ct);
        }
        
        var key = await GetKeyAsync(request);
        if (Cache.TryGetValue(key, out var value))
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(value.Response),
                StatusCode = value.Code,
                RequestMessage = request,
                Headers = {  },
                Version = Version.Parse("1.1")
            };
        }

        var result = await base.SendAsync(request, ct);
        var code = result.StatusCode;

        if ((int)code > 200)
            return result;
        
        var content = await result.Content!.ReadAsStringAsync(ct);

        Cache.TryAdd(key, new CacheEntry(content, code));
        SaveCache();
        
        return new HttpResponseMessage
        {
            Content = new StringContent(content),
            StatusCode = code,
            RequestMessage = request,
            Headers = {  },
            Version = Version.Parse("1.1")
        };
    }

    private static async Task<string> GetKeyAsync(HttpRequestMessage request)
    {
        if (request.Method == HttpMethod.Get)
            return request.RequestUri!.ToString();

        var content = await request.Content!.ReadAsStringAsync();
        return request.RequestUri + content;
    }

    private record CacheEntry(string Response, HttpStatusCode Code);
}
