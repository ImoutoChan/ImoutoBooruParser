using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{Id} — {Md5}")]
    public class PreviewEntry
    {
        public int Id { get; set; }

        public string Md5 { get; set; }

        public string Title { get; set; }
        
        public bool IsBanned { get; set; }
    }
}
