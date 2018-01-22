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

                var data = node.SelectNodes("td");

                var postid = int.Parse(data[0].SelectNodes("a").First().InnerHtml.Split('.').First());
                var date = DateTime.Parse(data[1].ChildNodes[0].Attributes["datetime"].Value);
                var user = WebUtility.HtmlDecode(data[2].SelectSingleNode("a").InnerHtml);

                // NonImplement
                //var tagNodes = data[5].SelectNodes("span");
                //var addedTags = SankakuTag.GetTagsFromList(tagNodes[0]);
                //var removedTags = SankakuTag.GetTagsFromList(tagNodes[1]);
                //var unchangedTags = SankakuTag.GetTagsFromList(tagNodes[2]);

                result.Add(new DanbooruPostUpdateEntry
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