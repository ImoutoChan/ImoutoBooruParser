using System.Collections.Generic;
using System.Diagnostics;
using Imouto.BooruParser.Model.Base;
using Post = Imouto.BooruParser.Model.Danbooru.Json.Post;

namespace Imouto.BooruParser.Model.Danbooru
{
    [DebuggerDisplay("Results count: {SearchCount}")]
    public class DanbooruSearchResult : SearchResult
    {
        public DanbooruSearchResult(List<Post> jsonPosts)
        {
            Parse(jsonPosts);
        }

        private void Parse(List<Post> jsonPosts)
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
                    Title = jsonPost.tag_string
                });
            }
        }
    }
}