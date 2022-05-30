using System;
using System.Collections.Generic;
using System.Diagnostics;
using Imouto.BooruParser.Model.Danbooru.Json;

namespace Imouto.BooruParser.Model.Base
{
    /// <summary>
    /// +PostId
    /// +IsExist
    /// Parent
    /// Children
    /// Pools
    /// Tags :
    ///     Translating
    ///     Source
    /// MD5
    /// SizeH
    /// SizeW
    /// Rating
    /// PostedDate
    /// PostedUser
    /// </summary>
    [DebuggerDisplay("{PostId} — {Md5}")]
    public abstract class Post
    {
        #region Constructor

        public Post(int postId, string md5)
        {
            if (postId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(postId));
            }

            PostId = postId;
            Md5 = md5;
        }

        #endregion Constructor

        #region Properties

        public string OriginalUrl { get; protected set; }

        public int PostId { get; }

        public ExistState PostExistState { get; protected set; }

        public DateTime PostedDateTime { get; protected set; }

        public User PostedUser { get; protected set; }

        public string Source { get; protected set; }

        public string Md5 { get; protected set; }

        public Size ImageSize { get; protected set; }

        public int ByteSize { get; protected set; }

        public Rating ImageRating { get; protected set; }
        
        public RatingSafeLevel RatingSafeLevel { get; protected set; }

        public UgoiraFrameData UgoiraFrameData { get; set; }

        /// <summary>
        /// String format: "{postId:md5}", sample: "4323700:974e0e7a11c5a713834e61cf6a33efcf"
        /// </summary>
        public string ParentId { get; protected set; }

        /// <summary>
        /// String format: "{postId:md5}", sample: "4323700:974e0e7a11c5a713834e61cf6a33efcf"
        /// </summary>
        public List<string> ChildrenIds { get; } = new List<string>();

        public List<Pool> Pools { get; } = new List<Pool>();

        public List<Tag> Tags { get; } = new List<Tag>();

        public List<Note> Notes { get; } = new List<Note>();

        public DateTime ActualDateTime { get; } = DateTime.Now;

        #endregion //Properties
    }
}
