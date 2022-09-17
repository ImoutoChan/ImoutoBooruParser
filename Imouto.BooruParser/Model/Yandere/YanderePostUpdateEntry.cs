using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Yandere
{
    //[DebuggerDisplay("{UpdateId} : {PostId} : (A/R/U) {AddedTags.Count}/{RemovedTags.Count}/{UnchangedTags.Count}")]
    [DebuggerDisplay("{UpdateId} : {PostId} : {UpdateDateTime}")]
    public class YanderePostUpdateEntry : PostUpdateEntry
    {
        internal static List<PostUpdateEntry> Parse(HtmlDocument htmlDoc)
        {
            var result = new List<PostUpdateEntry>();

            var historyNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='history']/tbody/tr");
            foreach (var trNode in historyNodes)
            {
                var id = Int32.Parse(trNode.Attributes["id"].Value.Substring(1));

                var data = trNode.SelectNodes("td");

                var historyType = data[0].InnerHtml;
                if (historyType != "Post")
                {
                    continue;
                }

                var postid = Int32.Parse(data[2].ChildNodes[0].InnerHtml);
                var date = DateTime.Parse(data[3].InnerHtml);
                var user = data[4].ChildNodes[0].InnerHtml;

                int? parentId = null;
                var parentChanged = false;
                var parentNodes = data[5].SelectNodes("span/a");
                if (parentNodes?.Count == 1)
                {
                    parentId = Int32.Parse(parentNodes.First().InnerText);
                    parentChanged = true;
                }
                
                // NonImplement
                //var tagNodes = data[5].SelectNodes("span");
                //var addedTags = SankakuTag.GetTagsFromList(tagNodes[0]);
                //var removedTags = SankakuTag.GetTagsFromList(tagNodes[1]);
                //var unchangedTags = SankakuTag.GetTagsFromList(tagNodes[2]);

                result.Add(new YanderePostUpdateEntry
                {
                    UpdateId = id,
                    PostId = postid,
                    UpdateDateTime = date,
                    User = user,
                    ParentId = parentId,
                    ParentChanged = parentChanged
                });
            }

            return result;
        }
    }
}
