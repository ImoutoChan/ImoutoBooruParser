using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Model.Danbooru
{
    [DebuggerDisplay("{Id} — {Label}")]
    class DanbooruNote : Note
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger<DanbooruNote>();

        public new static List<Note> GetNotes(HtmlNode rootNode)
        {
            var resultCollection = new List<Note>();

            var tagsNode = rootNode.SelectSingleNode(@"//*[@id='notes']");

            if (tagsNode == null)
            {
                return resultCollection;
            }


            var noteDivNodes = tagsNode.SelectNodes(@"article");
            if (noteDivNodes == null)
            {
                return resultCollection;
            }

            for (var i = 0; i < noteDivNodes.Count(); i++)
            {
                try
                {
                    var containerNode = noteDivNodes[i];

                    var widthString = containerNode.Attributes["data-width"]?.Value;
                    var heightString = containerNode.Attributes["data-height"]?.Value;
                    var xString = containerNode.Attributes["data-x"]?.Value;
                    var yString = containerNode.Attributes["data-y"]?.Value;
                    var idString = containerNode.Attributes["data-id"]?.Value;
                    var textString = containerNode.InnerHtml;

                    if (widthString == null
                        || heightString == null
                        || xString == null
                        || yString == null
                        || textString == null)
                    {
                        throw new Exception("Error while parsing note parametrs");
                    }

                    var width = int.Parse(widthString);
                    var height = int.Parse(heightString);
                    var x = int.Parse(xString);
                    var y = int.Parse(yString);
                    var id = int.Parse(idString);

                    var size = new Size
                    {
                        Height = height,
                        Width = width
                    };

                    var point = new Point
                    {
                        Left = x,
                        Top = y
                    };


                    resultCollection.Add(new DanbooruNote(id, textString, point, size));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in parsing note:\n" + ex.Message);
                    Logger.LogError(ex, "Error in parsing note");
                }
            }

            return resultCollection;
        }

        private DanbooruNote(int id, string text, Point point, Size size) : base(id, text, point, size)
        {
        }
    }
}