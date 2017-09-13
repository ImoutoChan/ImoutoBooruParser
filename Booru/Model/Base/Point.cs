using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("({Left}; {Top})")]
    public struct Point
    {
        public int Top { get; set; }
        public int Left { get; set; }
    }
}
