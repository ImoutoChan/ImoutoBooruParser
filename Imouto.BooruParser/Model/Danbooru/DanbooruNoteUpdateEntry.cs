using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Danbooru
{
    public abstract class DanbooruNoteUpdateEntry : NoteUpdateEntry
    {
        internal static List<NoteUpdateEntry> Parse(HtmlDocument htmlDoc)
        {
            var result = new List<NoteUpdateEntry>();

            var notesNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='a-index']/table/tbody/tr");

            if (notesNodes == null)
            {
                return result;
            }

            foreach (var item in notesNodes)
            {
                var noteUpdateEntry = new NoteUpdateEntry();

                noteUpdateEntry.PostId = int.Parse(item.SelectNodes("td")[1].SelectSingleNode("a").InnerHtml);
                var dateString = item.SelectNodes("td")[6].SelectSingleNode("time").Attributes["datetime"].Value;
                noteUpdateEntry.Date = DateTime.Parse(dateString);

                result.Add(noteUpdateEntry);
            }

            return result;
        }
    }
}