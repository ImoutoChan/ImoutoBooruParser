using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Model.Sankaku
{
    public static class SankakuTag
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger(nameof(SankakuTag));

        public static List<Tag> GetTags(HtmlNode tagsNode)
        {
            var resultCollection = new List<Tag>();

            var tagLiNodes = tagsNode.SelectNodes(@"li");
            foreach (var liNode in tagLiNodes)
            {
                try
                {
                    var tagTypeString = liNode.Attributes["class"].Value.Split('-').Last().UpperCaseFirstChar();
                    var type = (TagType)Enum.Parse(typeof(TagType), tagTypeString);

                    var aNode = liNode.SelectSingleNode(@"a");

                    var name = aNode.InnerHtml;

                    var japName = aNode.Attributes["title"]?.Value;

                    resultCollection.Add(Tag.CreateOrGetTag(type, name, japName));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in parsing tag:\n" + ex.Message);
                    Logger.LogError(ex, "Error in parsing tag");
                }
            }

            return resultCollection;
        }

        public static List<Tag> GetTagsFromList(HtmlNode tagNode)
        {
            var resultCollection = new List<Tag>();

            var tagSpanNodes = tagNode.SelectNodes(@"span");

            if (tagSpanNodes != null)
            {
                foreach (var liNode in tagSpanNodes)
                {
                    try
                    {
                        var tagTypeString = liNode.Attributes["class"]?.Value.Split(' ').FirstOrDefault()?.Split('-').Last().UpperCaseFirstChar();
                        var type = (TagType)Enum.Parse(typeof(TagType), tagTypeString);

                        var aNode = liNode.SelectSingleNode(@"a");

                        var name = aNode.InnerHtml;

                        var japName = aNode.Attributes["title"]?.Value;

                        resultCollection.Add(Tag.CreateOrGetTag(type, name, japName));

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in parsing tag:\n" + ex.Message);
                        Logger.LogError(ex, "Error in parsing tag");
                    }
                }
            }

            return resultCollection;
        }
    }
}
