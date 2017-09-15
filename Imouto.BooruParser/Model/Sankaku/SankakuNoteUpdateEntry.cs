using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Sankaku
{
    public abstract class SankakuNoteUpdateEntry : NoteUpdateEntry
    {
        internal static List<NoteUpdateEntry> Parse(HtmlDocument htmlDoc)
        {
            var result = new List<NoteUpdateEntry>();

            var notesNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id='content']/table/tbody/tr");
            foreach (var item in notesNodes)
            {
                var noteUpdateEntry = new NoteUpdateEntry();

                noteUpdateEntry.PostId = int.Parse(item.SelectNodes("td")[1].SelectSingleNode("a").InnerHtml);
                var dateString = item.SelectNodes("td")[5].InnerHtml;
                noteUpdateEntry.Date = DateTime.Parse(dateString);

                result.Add(noteUpdateEntry);
            }

            return result;
        }
    }
}