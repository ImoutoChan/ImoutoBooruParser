using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{Id} — {Label}")]
    public class Note
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger<Note>();

        /// <summary>
        ///     Work for both: sankaku and yandere
        /// </summary>
        public static List<Note> GetNotes(HtmlNode tagsNode)
        {
            var resultCollection = new List<Note>();

            if (tagsNode == null)
            {
                return resultCollection;
            }


            var noteDivNodes = tagsNode.SelectNodes(@"div");
            if (noteDivNodes == null)
            {
                return resultCollection;
            }

            for (var i = 0; i < noteDivNodes.Count(); i += 2)
            {
                try
                {
                    var containerNode = noteDivNodes[i];
                    var stylesStrings = containerNode.Attributes["style"].Value.Split(new[]
                    {
                        ';'
                    },
                                                                                      StringSplitOptions
                                                                                          .RemoveEmptyEntries).ToList();

                    var size = new Size();
                    var point = new Point();

                    foreach (var stylesString in stylesStrings)
                    {
                        var elements = stylesString.Trim().Split(new[]
                        {
                            ": "
                        },
                                                                 StringSplitOptions.RemoveEmptyEntries);
                        var type = elements.First();
                        var value = elements.Last();

                        switch (type)
                        {
                            case "width":
                                size.Width = (int) (Convert.ToDouble(value.Substring(0, value.Length - 2)) + 0.5);
                                break;
                            case "height":
                                size.Height = (int) (Convert.ToDouble(value.Substring(0, value.Length - 2)) + 0.5);
                                break;
                            case "top":
                                point.Top =
                                    (int) Math.Ceiling(Convert.ToDouble(value.Substring(0, value.Length - 2)) - 0.5);
                                break;
                            case "left":
                                point.Left =
                                    (int) Math.Ceiling(Convert.ToDouble(value.Substring(0, value.Length - 2)) - 0.5);
                                break;
                        }
                    }

                    var textDivNode = noteDivNodes[i + 1];
                    var id = Convert.ToInt32(textDivNode.Attributes["id"].Value.Split('-').Last());
                    var text = textDivNode.InnerHtml;

                    resultCollection.Add(new Note(id, text, point, size));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in parsing note:\n" + ex.Message);
                    Logger.LogError(ex, "Error in parsing note");
                }
            }

            return resultCollection;
        }

        protected Note(int id, string text, Point point, Size size)
        {
            Id = id;
            Label = text;
            NotePoint = point;
            NoteSize = size;
        }

        public int Id { get; }

        public string Label { get; }

        public Point NotePoint { get; }

        public Size NoteSize { get; }
    }
}
