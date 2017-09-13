using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            foreach (var node in historyNodes)
            {
                var id = Int32.Parse(node.Attributes["id"].Value.Substring(1));

                var data = node.SelectNodes("td");

                var historyType = data[0].InnerHtml;
                if (historyType != "Post")
                {
                    break;
                }

                var postid = Int32.Parse(data[2].ChildNodes[0].InnerHtml);
                var date = DateTime.Parse(data[3].InnerHtml);
                var user = data[4].ChildNodes[0].InnerHtml;
                
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
                    User = user
                });
            }

            return result;
        }
    }
}