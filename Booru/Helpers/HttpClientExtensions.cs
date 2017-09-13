using System.Net.Http.Headers;

namespace Imouto.BooruParser.Helpers
{
    static class HttpClientExtensions
    {
        public static void Set(this HttpRequestHeaders headers, string name, string value)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, value);
        }

        public static void Set(this HttpRequestHeaders headers, string header)
        {
            var index = header.IndexOf(':');

            if (index == -1)
                return;

            var name = header.Substring(0, index);
            var value = header.Substring(index + 2);

            headers.Set(name, value);
        }
    }
}
