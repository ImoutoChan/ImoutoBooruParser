using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using NLog;

namespace Imouto.BooruParser.Model.Sankaku
{
    [DebuggerDisplay("{PostId} — {Md5}")]
    public class SankakuPost : Post
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Constructor

        public SankakuPost(int postId, HtmlNode postNode, string md5 = null) : base(postId, md5)
        {
            if (postNode == null)
            {
                throw new ArgumentNullException(nameof(postNode));
            }

            ParsePost(postNode);
        }

        #endregion Constructor
        
        #region Methods

        private void ParsePost(HtmlNode postNode)
        {
            ParseTags(postNode);
            ParseStats(postNode);

            ParseIsDeleted(postNode);

            if (PostExistState == ExistState.Exist)
            {
                ParseNotes(postNode);
                ParsePools(postNode);
            }

            ParseRelations(postNode);
        }

        private void ParseIsDeleted(HtmlNode postNode)
        {
            var deletedNotifNode = postNode.SelectSingleNode(@"//*[@id='right-col']/div[@class='status-notice deleted']");

            this.PostExistState = deletedNotifNode != null ? ExistState.MarkDeleled : ExistState.Exist;
        }

        private void ParsePools(HtmlNode postNode)
        {
            this.Pools.AddRange(SankakuPool.GetPools(postNode));
        }

        private void ParseNotes(HtmlNode postNode)
        {
            this.Notes.AddRange(Note.GetNotes(postNode.SelectSingleNode(@"//*[@id='note-container']")));
        }

        private void ParseTags(HtmlNode postNode)
        {
            this.Tags.AddRange(SankakuTag.GetTags(postNode.SelectSingleNode(@"//*[@id='tag-sidebar']")));
        }

        private void ParseRelations(HtmlNode postNode)
        {
            var childrenRootNode = postNode.SelectNodes(@"//*[@id='right-col']/div[@class='status-notice']/div")?.FirstOrDefault(x => x.Attributes["id"]?.Value.Substring(0, 4) == "chil");
            if (childrenRootNode != null)
            {
                var childrenNodes = childrenRootNode.SelectNodes(@"span");
                foreach (var child in childrenNodes)
                {
                    var childId = child.Attributes["id"].Value;
                    var childPostId = Int32.Parse(childId.Substring(1));
                    var link = child.FirstChild.FirstChild.Attributes["src"].Value;
                    var md5 = (new Regex(@"([abcdef\d]){32}")).Match(link).Value;

                    this.ChildrenIds.Add(String.Format("{0}:{1}", childPostId, md5));
                }
            }


            var parentRootNode = postNode.SelectNodes(@"//*[@id='right-col']/div[@class='status-notice']/div")?.FirstOrDefault(x => x.Attributes["id"]?.Value.Substring(0, 4) == "pare");
            if (parentRootNode != null)
            {
                var parent = parentRootNode.SelectSingleNode(@"span");
                var parentId = parent.Attributes["id"].Value;
                var parentPostId = Int32.Parse(parentId.Substring(1));
                var link = parent.FirstChild.FirstChild.Attributes["src"].Value;
                var md5 = (new Regex(@"([abcdef\d]){32}")).Match(link).Value;

                this.ParentId = String.Format("{0}:{1}", parentPostId, md5);
            }
        }

        #region Parse Stats Logic

        private void ParseStats(HtmlNode postNode)
        {
            var statsLiNodes = postNode.SelectNodes(@"//*[@id='stats']/ul/li");
            foreach (var liNode in statsLiNodes)
            {
                var str = liNode.InnerHtml.Substring(0, 3);
                switch (str)
                {
                    case "\nPo":
                        ParsePostedInfo(liNode);
                        break;
                    case "Ori":
                        ParseOriginalInfo(liNode);
                        break;
                    case "Sou":
                        ParseSourceInfo(liNode);
                        break;
                    case "Rat":
                        ParseRatingInfo(liNode);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ParseRatingInfo(HtmlNode liNode)
        {
            try
            {
                this.ImageRating = (Rating)Enum.Parse(typeof(Rating), GetStatsSeparatedValue(liNode));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in parsing 'rating' info:\n" + ex.Message);
                Logger.Error($"Error in parsing 'rating' info: {ex.Message}");
            }
        }

        private void ParseSourceInfo(HtmlNode liNode)
        {
            try
            {
                var aSourceNode = liNode.SelectSingleNode(@"a");
                string sourceString;

                // Text source
                if (aSourceNode == null)
                {
                    sourceString = GetStatsSeparatedValue(liNode);
                }
                // Link source
                else
                {
                    sourceString = aSourceNode.Attributes["href"].Value;
                }

                this.Source = sourceString;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in parsing 'source' info:\n" + ex.Message);
                Logger.Error($"Error in parsing 'source' info: {ex.Message}");
            }
        }

        private void ParseOriginalInfo(HtmlNode liNode)
        {
            try
            {
                //Parsing orig image info (hash, size, wxh)
                var aOriginalNode = liNode.SelectSingleNode(@"a");

                var hash = aOriginalNode.Attributes["href"].Value.Split('/').Last().Split('.').First();

                var byteSizeString = aOriginalNode.Attributes["title"].Value.Split(' ').First().Replace(",", "");
                var byteSize = Convert.ToInt32(byteSizeString);

                var sizes = aOriginalNode.InnerHtml.Split(' ').First().Split('x');
                var size = new Size
                {
                    Height = Convert.ToInt32(sizes.Last()),
                    Width = Convert.ToInt32(sizes.First()),
                };

                this.Md5 = hash;
                this.ByteSize = byteSize;
                this.ImageSize = size;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in parsing 'original' info:\n" + ex.Message);
                Logger.Error($"Error in parsing 'original' info: {ex.Message}");
            }
        }

        private void ParsePostedInfo(HtmlNode liNode)
        {
            try
            {
                //Parsing posted info (date/user)
                var aNodes = liNode.SelectNodes(@"a");

                // 0 - date
                var postedDateString = aNodes[0].Attributes["title"].Value;
                var postedDateTime = Convert.ToDateTime(postedDateString);

                this.PostedDateTime = postedDateTime;

                if (aNodes.Count > 1)
                {
                    // 1 - user 
                    var userString = aNodes[1].Attributes["href"].Value;
                    var idString = userString.Split('/').Last();
                    var userName = aNodes[1].InnerHtml;
                    var user = new User { Id = Convert.ToInt32(idString), Name = userName };
                    this.PostedUser = user;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in parsing 'posted' info:\n" + ex.Message);
                Logger.Error($"Error in parsing 'posted' info: {ex.Message}");
            }
        }

        private string GetStatsSeparatedValue(HtmlNode liNode)
        {
            string sourceString;
            var entries = liNode.InnerHtml.Split(new[]
                                                 {
                                                     ": "
                                                 },
                                                 StringSplitOptions.RemoveEmptyEntries).ToList();

            if (entries.Count() > 2)
            {
                entries.Remove(entries.First());
                sourceString = String.Join(": ", entries);
            }
            else
            {
                sourceString = entries[1];
            }
            return sourceString;
        }

        #endregion Parse Stats Logic

        #endregion Methods
    }
}
