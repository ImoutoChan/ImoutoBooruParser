using System.Collections.Generic;
using System.Diagnostics;
using Imouto.BooruParser.Model.Base;
using Post = Imouto.BooruParser.Model.Yandere.Json.Post;

namespace Imouto.BooruParser.Model.Yandere
{
    [DebuggerDisplay("Results count: {SearchCount}")]
    public class YandereSearchResult : SearchResult
    {
        public YandereSearchResult(List<Post> jsonPosts)
        {
            Parse(jsonPosts);
        }

        public void Parse(List<Post> jsonPosts)
        {
            SearchCount = jsonPosts.Count;
            ParseResults(jsonPosts);
        }

        private void ParseResults(List<Post> jsonPosts)
        {
            foreach (var jsonPost in jsonPosts)
            {
                Results.Add(new PreviewEntry
                {
                    Id = jsonPost.id,
                    Md5 = jsonPost.md5,
                    Title = jsonPost.tags
                });
            }
        }
    }
}