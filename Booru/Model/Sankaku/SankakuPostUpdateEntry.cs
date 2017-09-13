using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                var id = Int32.Parse(node.Attributes["id"].Value.Substring(1));

                var data = node.SelectNodes("td");

                var postid = Int32.Parse(data[1].ChildNodes[0].InnerHtml);
                var date = DateTime.Parse(data[2].InnerHtml);
                var user = data[3].ChildNodes[1].InnerHtml;


                Rating rating;
                switch (data[5].InnerHtml)
                {
                    case "e":
                        rating = Rating.Explicit;
                        break;
                    case "s":
                        rating = Rating.Safe;
                        break;
                    default:
                    case "q":
                        rating = Rating.Questionable;
                        break;
                }

                int? parent = null;
                var parentString = data[6].InnerHtml;
                if (!String.IsNullOrWhiteSpace(parentString))
                {
                    parent = Int32.Parse(parentString);
                }

                var tagNodes = data[7].SelectNodes("span");
                var addedTags = SankakuTag.GetTagsFromList(tagNodes[0]);
                var removedTags = SankakuTag.GetTagsFromList(tagNodes[1]);
                var unchangedTags = SankakuTag.GetTagsFromList(tagNodes[2]);

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
                    UnchangedTags = unchangedTags
                });
            }

            return result;
        }
    }
}