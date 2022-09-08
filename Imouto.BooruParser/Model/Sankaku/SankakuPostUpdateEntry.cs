using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Sankaku
{
    //[DebuggerDisplay("{UpdateId} : {PostId} : (A/R/U) {AddedTags.Count}/{RemovedTags.Count}/{UnchangedTags.Count}")]
    [DebuggerDisplay("{UpdateId} : {PostId} : {UpdateDateTime}")]
    public class SankakuPostUpdateEntry : PostUpdateEntry
    {
        internal static List<PostUpdateEntry> Parse(HtmlDocument htmlDoc)
        {
            var result = new List<PostUpdateEntry>();

            var historyNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='history']/tbody/tr");
            foreach (var node in historyNodes)
            {
                var id = int.Parse(node.Attributes["id"].Value.Substring(1));

                var data = node.SelectNodes("td");

                var postid = int.Parse(data[1].SelectNodes("a")[0].InnerHtml);
                var date = DateTime.Parse(data[1].InnerText.Split(
                    new [] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries).Last());
                var user = data[2].SelectNodes("a")[1].InnerHtml;


                Rating rating;
                switch (data[3].SelectSingleNode("span/a").InnerText)
                {
                    case "Explicit":
                        rating = Rating.Explicit;
                        break;
                    case "Safe":
                        rating = Rating.Safe;
                        break;
                    case "Questionable":
                        rating = Rating.Questionable;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(rating));
                }

                int? parent = null;
                var parentStrings = data[3].SelectNodes("span/a");
                if (parentStrings.Count > 1 && !string.IsNullOrWhiteSpace(parentStrings[1].InnerText))
                {
                    parent = int.Parse(parentStrings[1].InnerText);
                }

                var tagNodes = data[4].SelectNodes("span");
                var addedTags = SankakuTag.GetTagsFromList(tagNodes[0]);
                var removedTags = SankakuTag.GetTagsFromList(tagNodes[1]);

                result.Add(new SankakuPostUpdateEntry
                {
                    UpdateId = id,
                    PostId = postid,
                    UpdateDateTime = date,
                    User = user,
                    Rating = rating,
                    Parent = parent,
                    AddedTags = addedTags,
                    RemovedTags = removedTags,
                    UnchangedTags = Array.Empty<Tag>().ToList()
                });
            }

            return result;
        }
    }
}
