using System;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Model.Sankaku
{
    [DebuggerDisplay("Results count: {" + nameof(SearchCount) + "}")]
    public class SankakuSearchResult : SearchResult
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger<SankakuSearchResult>();

        public SankakuSearchResult(HtmlNode documentNode)
        {
            Parse(documentNode);
        }

        public void Parse(HtmlNode node)
        {
            ParseCount(node);
            ParseResults(node);

        }

        private void ParseResults(HtmlNode node)
        {
            var previewNodes = node.SelectNodes("//*[@id='post-list']/div/div/span");
            if (previewNodes == null)
            {
                return;
            }

            foreach (var spanNode in previewNodes)
            {
                var idStr = spanNode.ChildNodes[0].Attributes["href"].Value.Split('/').Last();
                var id = Int32.Parse(idStr);

                var md5 = spanNode.ChildNodes[0]?.ChildNodes[0]?.Attributes["src"]?.Value.Split('/')?.LastOrDefault()?.Split('.')?.FirstOrDefault();
                var title = spanNode.ChildNodes[0]?.ChildNodes[0]?.Attributes["title"]?.Value;

                Results.Add(new PreviewEntry { Id = id, Md5 = md5, Title = title });
            }
        }

        private void ParseCount(HtmlNode node)
        {
            var countNode = node.SelectSingleNode("//*[@class='tag-count']");
            try
            {
                var countString = countNode?.InnerHtml?.Replace(",", String.Empty);
                if (countString != null)
                {
                    SearchCount = Int32.Parse(countString);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while search result loading: {ex.Message}");
                Logger.LogError(ex, "Exception while search result loading");
            }
        }
    }
}