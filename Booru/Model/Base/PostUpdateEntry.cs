using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Imouto.BooruParser.Model.Base
{
    [DebuggerDisplay("{UpdateId} : {PostId} : {UpdateDateTime}")]
    public abstract class PostUpdateEntry
    {
        public int UpdateId { get; protected set; }

        public DateTime UpdateDateTime { get; protected set; }

        public int PostId { get; protected set; }

        public string User { get; protected set; }

        public Rating Rating { get; protected set; }

        public int? Parent { get; protected set; }

        public List<Tag> AddedTags { get; protected set; }

        public List<Tag> RemovedTags { get; protected set; }

        public List<Tag> UnchangedTags { get; protected set; }
    }
}
