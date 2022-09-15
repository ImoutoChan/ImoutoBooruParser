using System;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Yandere
{
    [DebuggerDisplay("{PostId} — {Md5}")]
    public class YanderePost : Post
    {
        #region Constructor

        public YanderePost(int postId, HtmlNode postNode, Json.Post postJson, string md5 = null) : base(postId, md5)
        {
            if (postNode == null)
            {
                throw new ArgumentNullException(nameof(postNode));
            }
            if (postJson == null)
            {
                throw new ArgumentNullException(nameof(postJson));
            }

            ParsePost(postNode, postJson);
        }

        #endregion Constructor
        
        #region Methods

        private void ParsePost(HtmlNode postNode, Json.Post postJson)
        {
            ParseTags(postNode);
            ParseStats(postJson);
            
            ParseIsDeleted(postNode);
            
            if (PostExistState == ExistState.Exist)
            {
                ParseNotes(postNode);
                ParsePools(postNode);
            }

            ParseRelations(postNode);
        }
        private void ParseTags(HtmlNode postNode)
        {
            this.Tags.AddRange(YandereTag.GetTags(postNode.SelectSingleNode(@"//*[@id='tag-sidebar']")));
        }

        private void ParseStats(Json.Post postJson)
        {
            this.PostedDateTime = DateTimeOffset.FromUnixTimeSeconds(postJson.created_at).DateTime;
            this.PostedUser = new User
            {
                Id = postJson.creator_id,
                Name = postJson.author
            };

            this.Md5 = postJson.md5;
            this.ByteSize = postJson.file_size;
            this.ImageSize = new Size
            {
                Height = postJson.height,
                Width = postJson.width
            };

            this.Source = postJson.source;

            ImageRating = GetRatingFromChar(postJson.rating);
            RatingSafeLevel = RatingSafeLevel.None;

            OriginalUrl = postJson.file_url;
            SampleUrl = postJson.sample_url;
        }

        private Rating GetRatingFromChar(string rating)
        {
            switch (rating)
            {
                default:
                case "q":
                    return Rating.Questionable;
                    break;
                case "s":
                    return Rating.Safe;
                    break;
                case "e":
                    return Rating.Explicit;
            }
        }
        
        private void ParseNotes(HtmlNode postNode)
        {
            this.Notes.AddRange(Note.GetNotes(postNode.SelectSingleNode(@"//*[@id='note-container']")));
        }

        private void ParsePools(HtmlNode postNode)
        {
            this.Pools.AddRange(YanderePool.GetPools(postNode));
        }

        private void ParseIsDeleted(HtmlNode postNode)
        {
            var isDeleted = postNode.SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")?.Any(x => x.InnerHtml.Contains("This post was deleted.")) ?? false;

            this.PostExistState = isDeleted ? ExistState.MarkDeleled : ExistState.Exist;
        }

        private void ParseRelations(HtmlNode postNode)
        {
            var childrenRootNode = postNode.SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")?.FirstOrDefault(x => x.InnerHtml.Contains("child post"));
            if (childrenRootNode != null)
            {
                var childrenNodes = childrenRootNode.SelectNodes(@"a").Where(x => x.Attributes["href"]?.Value.Contains("/post/show/") ?? false);
                foreach (var child in childrenNodes)
                {
                    var childId = Int32.Parse(child.InnerHtml);
                    this.ChildrenIds.Add($"{childId}:{String.Empty}");
                }
            }


            var parentRootNode = postNode.SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")?.FirstOrDefault(x => x.InnerHtml.Contains("parent post"));
            if (parentRootNode != null)
            {
                var parent = parentRootNode.SelectSingleNode(@"a");
                var parentHref = parent.Attributes["href"].Value;
                var parentPostId = Int32.Parse(parentHref.Split('/').Last());

                this.ParentId = $"{parentPostId}:{String.Empty}";
            }
        }


        #endregion Methods
    }
}
