using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{Id} : {Name}")]
    public class Pool
    {
        public Pool(int id, string name)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }

        public int Id { get; }
    }
}
