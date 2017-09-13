using System.Collections.Generic;
using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    public abstract class SearchResult
    {
        [DebuggerDisplay("{Id} — {Md5}")]
        public class PreviewEntry
        {
            public int Id { get; set; }

            public string Md5 { get; set; }

            public string Title { get; set; }
        }

        public List<PreviewEntry> Results { get; } = new List<PreviewEntry>();

        public bool NotEmpty => Results.Count > 0;

        public int? SearchCount { get; protected set; }
    }
}
