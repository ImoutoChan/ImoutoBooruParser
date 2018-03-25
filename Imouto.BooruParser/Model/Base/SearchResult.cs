using System.Collections.Generic;

namespace Imouto.BooruParser.Model.Base
{
    public abstract class SearchResult
    {
        public List<PreviewEntry> Results { get; } = new List<PreviewEntry>();

        public bool NotEmpty => Results.Count > 0;

        public int? SearchCount { get; protected set; }
    }
}
