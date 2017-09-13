using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{Width}x{Height}")]
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
