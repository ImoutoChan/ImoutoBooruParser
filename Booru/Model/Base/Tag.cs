using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{Name} ({JapName}) : {Type}")]
    public class Tag
    {
        public static List<Tag> Tags = new List<Tag>();
        public static readonly SemaphoreSlim CreatingTagSemaphore = new SemaphoreSlim(1, 1);
        
        public static Tag CreateOrGetTag(TagType type, string name, string japName)
        {
            CreatingTagSemaphore.Wait();

            try
            {
                var tag = Tags.FirstOrDefault(x => x.JapName == japName && x.Name == name && x.Type == type);

                if (tag == null)
                {
                    var newTag = new Tag(type, name, japName);
                    Tags.Add(newTag);
                    return newTag;
                }
                else
                {
                    return tag;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CreatingTagSemaphore.Release();
            }
        }

        protected Tag(TagType type, string name, string japName)
        {
            Type = type;
            Name = name;
            JapName = japName;
        }

        public TagType Type { get; }

        public string Name { get; }

        public string JapName { get; }
    }
}
