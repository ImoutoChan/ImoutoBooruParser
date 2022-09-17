using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Danbooru.Json;

namespace Imouto.BooruParser.Model.Danbooru
{
    [DebuggerDisplay("{UpdateId} : {PostId} : {UpdateDateTime}")]
    public class DanbooruPostUpdateEntry : PostUpdateEntry
    {
        internal static List<PostUpdateEntry> GetFromJson(IEnumerable<PostVersion> versions)
        {
            return versions
                .Select(x => new DanbooruPostUpdateEntry
                {
                    UpdateId = x.Id,
                    PostId = x.PostId,
                    UpdateDateTime = x.UpdatedAt.DateTime,
                    ParentId = x.ParentId,
                    ParentChanged = x.ParentChanged
                })
                .Cast<PostUpdateEntry>()
                .ToList();
        }
    }
}
