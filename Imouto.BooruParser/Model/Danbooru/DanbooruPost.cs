using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;

namespace Imouto.BooruParser.Model.Danbooru
{
    [DebuggerDisplay("{PostId} — {Md5}")]
    public class DanbooruPost : Post
    {
        #region Constructor

        public DanbooruPost(int postId, HtmlNode documentNode, Json.Post postJson, string md5 = null)
            : base(postId, md5)
        {
            if (postJson == null)
            {
                throw new ArgumentNullException(nameof(postJson));
            }
            if (documentNode == null)
            {
                throw new ArgumentNullException(nameof(documentNode));
            }

            ParsePost(postJson, documentNode);
        }

        #endregion Constructor

        public bool IsUgoira => OriginalUrl?.EndsWith(".zip") ?? false;

        #region Methods

        private void ParsePost(Json.Post postJson, HtmlNode documentNode)
        {
            ParseTags(postJson);
            ParseStats(postJson);
            ParseIsDeleted(postJson);

            if (PostExistState == ExistState.Exist)
            {
                ParsePools(documentNode);
                ParseNotes(documentNode);
            }

            ParseRelations(documentNode);


        }

        private void ParseTags(Json.Post postJson)
        {
            foreach (var tag in postJson.tag_string_artist.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                Tags.Add(Tag.CreateOrGetTag(TagType.Artist, tag.Replace('_', ' '), null));
            }
            foreach (var tag in postJson.tag_string_character.Split(new [] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                Tags.Add(Tag.CreateOrGetTag(TagType.Character, tag.Replace('_', ' '), null));
            }
            foreach (var tag in postJson.tag_string_copyright.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                Tags.Add(Tag.CreateOrGetTag(TagType.Copyright, tag.Replace('_', ' '), null));
            }
            foreach (var tag in postJson.tag_string_general.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                Tags.Add(Tag.CreateOrGetTag(TagType.General, tag.Replace('_', ' '), null));
            }
            foreach (var tag in postJson.tag_string_meta.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                Tags.Add(Tag.CreateOrGetTag(TagType.Meta, tag.Replace('_', ' '), null));
            }
        }

        private void ParseStats(Json.Post postJson)
        {
            PostedDateTime = DateTime.Parse(postJson.created_at);
            PostedUser = new User
            {
                Id = postJson.uploader_id,
                Name = postJson.uploader_name
            };


            Md5 = postJson.md5;
            ByteSize = postJson.file_size;
            ImageSize = new Size
            {
                Height = postJson.image_height,
                Width = postJson.image_width
            };

            Source = postJson.source;

            ImageRating = GetRatingFromChar(postJson.rating);

            OriginalUrl = postJson.file_url;
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
            Notes.AddRange(DanbooruNote.GetNotes(postNode));
        }

        private void ParsePools(HtmlNode postNode)
        {
            Pools.AddRange(DanbooruPool.GetPools(postNode));
        }

        private void ParseIsDeleted(Json.Post postJson)
        {
            PostExistState = (postJson.is_banned || postJson.is_deleted) ? ExistState.MarkDeleled : ExistState.Exist;
        }

        private void ParseRelations(HtmlNode postNode)
        {
            var childrenNodes = postNode.SelectNodes(@"//div[@id='has-children-relationship-preview']//article");

            if (childrenNodes != null)
            {
                foreach (var child in childrenNodes)
                {
                    var childId = int.Parse(child.Attributes["id"].Value.Substring(5));
                    if (childId == PostId)
                    {
                        continue;
                    }

                    var url = child.SelectSingleNode("*//img").Attributes["src"].Value;

                    var md5 = new Regex("[a-fA-F0-9]{32}").Match(url).Value;
                    ChildrenIds.Add($"{childId}:{md5}");
                }
            }


            var parentNodes = postNode.SelectNodes(@"//div[@id='has-parent-relationship-preview']//article");
            if (parentNodes != null)
            {
                var parent = parentNodes[0];
                var parentId = int.Parse(parent.Attributes["id"].Value.Substring(5));

                var url = parent.SelectSingleNode("*//img").Attributes["src"].Value;

                var md5 = new Regex("[a-fA-F0-9]{32}").Match(url).Value;

                ParentId = $"{parentId}:{md5}";
            }
        }

        #endregion Methods
    }
}
