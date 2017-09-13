using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{Id} — {Name}")]
    public class User
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }
}