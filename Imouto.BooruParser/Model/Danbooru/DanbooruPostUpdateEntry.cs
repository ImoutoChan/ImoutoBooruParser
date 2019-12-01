using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Danbooru
{
    [DebuggerDisplay("{UpdateId} : {PostId} : {UpdateDateTime}")]
    public class DanbooruPostUpdateEntry : PostUpdateEntry
    {
        internal static List<PostUpdateEntry> Parse(HtmlDocument htmlDoc)
        {
            var result = new List<PostUpdateEntry>();

            var historyNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='a-index']/div[1]/table/tbody/tr");

            if (historyNodes == null)
            {
                return result;
            }
            
            foreach (var node in historyNodes)
            {
                var id = int.Parse(node.Attributes["id"].Value.Split('-').Last());
                
                var postIdString = node.SelectSingleNode("td[2]/a").InnerHtml.Split('.').First();
                var dateString = node.SelectSingleNode("td[4]/div/time").Attributes["datetime"].Value;
                var userString = node.SelectSingleNode("td[4]/a").InnerHtml;

                var postId = int.Parse(postIdString);
                var date = DateTime.Parse(dateString);
                var user = WebUtility.HtmlDecode(userString);

                // Not implemented
                //var tagNodes = data[5].SelectNodes("span");
                //var addedTags = SankakuTag.GetTagsFromList(tagNodes[0]);
                //var removedTags = SankakuTag.GetTagsFromList(tagNodes[1]);
                //var unchangedTags = SankakuTag.GetTagsFromList(tagNodes[2]);

                result.Add(new DanbooruPostUpdateEntry
                {
                    UpdateId = id,
                    PostId = postId,
                    UpdateDateTime = date,
                    User = user
                });
            }

            return result;
        }
    }
}