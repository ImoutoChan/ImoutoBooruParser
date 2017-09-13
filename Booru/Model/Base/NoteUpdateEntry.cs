using System;
using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{PostId} : {Date}")]
    public class NoteUpdateEntry
    {
        public int PostId { get; set; }

        public DateTime Date { get; set; }
    }
}