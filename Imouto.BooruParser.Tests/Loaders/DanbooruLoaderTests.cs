using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using Imouto.BooruParser.Model.Danbooru;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;
using Xunit.Sdk;

namespace Imouto.BooruParser.Tests.Loaders.DanbooruLoaderTests
{
    public class DanbooruLoaderTests : IClassFixture<DanbooruLoaderFixture>
    {
        private readonly DanbooruLoaderFixture _loaderFixture;

        public DanbooruLoaderTests(DanbooruLoaderFixture loaderFixture)
        {
            _loaderFixture = loaderFixture;
        }

        public class LoadPostAsyncMethod : DanbooruLoaderTests
        {
            public LoadPostAsyncMethod(DanbooruLoaderFixture loaderFixture) : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnPostWithoutCredentials()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var post = await loader.LoadPostAsync(1);

                post.Should().NotBe(null);
                post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
            }
        }

        public class LoadFirstTagHistoryPageAsyncMethod : DanbooruLoaderTests
        {
            public LoadFirstTagHistoryPageAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldReturnWithCredentials()
            {
                var loader = _loaderFixture.GetLoaderWithAuth();

                var firstPage = await loader.LoadFirstTagHistoryPageAsync();

                firstPage.Should().NotBeEmpty();
            }
        }

        public class LoadSearchResultAsyncMethod : DanbooruLoaderTests
        {
            public LoadSearchResultAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldFind()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("1girl");
                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldFindMd5OfDeletedPost()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:746310ab23d72e075755fd426469e31c");
                
                
                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().Be(1);
                
                searchResult.Results.First().Id.Should().Be(0);
            }

            [Fact]
            public async Task ShouldFindMd5OfRegularPost()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:4ff6bfa1745692b8eaf4ba2d2208c207");
                
                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().Be(1);
                
                searchResult.Results.First().Id.Should().Be(5031817);
                searchResult.Results.First().Md5.Should().Be("4ff6bfa1745692b8eaf4ba2d2208c207");
            }
        }

        public class LoadNotesHistoryAsyncMethod : DanbooruLoaderTests
        {
            public LoadNotesHistoryAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadNotesHistory()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var notesHistory = await loader.LoadNotesHistoryAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }
        public class LoadTagHistoryUpToAsyncMethod : DanbooruLoaderTests
        {
            public LoadTagHistoryUpToAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadWithAuth()
            {
                var loader = _loaderFixture.GetLoaderWithAuth();

                var notesHistory = await loader.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
                notesHistory.Should().NotBeEmpty();
            }
        }

        public class LoadTagHistoryFromAsyncMethod : DanbooruLoaderTests
        {
            public LoadTagHistoryFromAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadTagsHistoryFromId()
            {
                var loader = _loaderFixture.GetLoaderWithAuth();
                var firstTagHistoryPage = await loader.LoadFirstTagHistoryPageAsync();

                var tagHistory = await loader.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId);

                tagHistory.Should().NotBeEmpty();
            }

            [Fact(Skip = "Can't load exact page")]
            public async Task ShouldLoadTagsHistoryWithParentChanges()
            {
                var loader = _loaderFixture.GetLoaderWithAuth();

                var tagsHistory = await loader.LoadTagHistoryFromAsync(43125946);

                tagsHistory.Should().NotBeEmpty();
                tagsHistory.First(x => x.UpdateId == 43125965).ParentChanged.Should().BeTrue();
                tagsHistory.First(x => x.UpdateId == 43125965).ParentId.Should().BeNull();
                
                tagsHistory.First(x => x.UpdateId == 43125951).ParentChanged.Should().BeFalse();
                tagsHistory.First(x => x.UpdateId == 43125951).ParentId.Should().BeNull();
                
                tagsHistory.First(x => x.UpdateId == 43125948).ParentChanged.Should().BeFalse();
                tagsHistory.First(x => x.UpdateId == 43125948).ParentId.Should().Be(4978487);
            }
        }

        public class LoadPopularAsyncMethod : DanbooruLoaderTests
        {
            public LoadPopularAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadPopularForDay()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadPopularAsync(PopularType.Day);

                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForWeek()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadPopularAsync(PopularType.Week);

                searchResult.Results.Should().NotBeEmpty();
                searchResult.NotEmpty.Should().BeTrue();
                searchResult.SearchCount.Should().BeGreaterThan(1);
            }

            [Fact]
            public async Task ShouldLoadPopularForMonth()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var serachResult = await loader.LoadPopularAsync(PopularType.Month);

                serachResult.Results.Should().NotBeEmpty();
                serachResult.NotEmpty.Should().BeTrue();
                serachResult.SearchCount.Should().BeGreaterThan(1);
            }
        }

        public class LoadPostMetadataAsyncMethod : DanbooruLoaderTests
        {
            public LoadPostMetadataAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldLoadParentsAndChildren()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:46cce564e9b43a4c69c132840dca1252");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().NotBe(0);
                post.ParentId.Should().NotBeNullOrWhiteSpace();
            }

            /// <summary>
            /// Bug with post 5032478
            /// </summary>
            [Fact]
            public async Task ShouldLoadChildren()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var post = await loader.LoadPostAsync(5032478);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().Be(2);
                post.ParentId.Should().BeNull();
            }

            /// <summary>
            /// Bug with post 5314036
            /// </summary>
            [Fact]
            public async Task ShouldLoadChildrenFor5314036()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var post = await loader.LoadPostAsync(5314036);

                post.Tags.Count.Should().BeGreaterThan(30);
                post.ChildrenIds.Count.Should().Be(1);
                post.ChildrenIds.First().Should().Be("5318896:46dda085dc9c60dd4380ed7b4433aa41");
                post.ParentId.Should().BeNull();
            }
            
            /// <summary>
            /// Bug with post 5666656
            /// </summary>
            [Fact]
            public async Task ShouldLoadSampleUrlFor5666656()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var post = await loader.LoadPostAsync(5666656);

                post.Should().NotBeNull();
                post.SampleUrl.Should()
                    .Be("https://cdn.donmai.us/sample/4a/8b/sample-4a8b6ecee31d9e66e5532f22b19ab736.webm");
            }

            [Fact]
            public async Task ShouldLoadNotes()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:59b8ac9d3fe23a315f4468623ea7609a");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Notes.Count.Should().BeGreaterThan(6);
            }

            [Fact]
            public async Task ShouldLoadMetaTags()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();
                var searchResult = await loader.LoadSearchResultAsync("md5:43d3f7154d9612aaaf7ce0fa585887b2");
                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Tags.Should().Contain(x => x.Name == "paid reward");
            }

            [Fact]
            public async Task ShouldLoadPools()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:37493f99a0e45a35f5b69f2c90b2ad39");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.Pools.Count.Should().BeGreaterOrEqualTo(1);
            }

            [Fact]
            public async Task ShouldLoadUgoiraMetadata()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var searchResult = await loader.LoadSearchResultAsync("md5:0802b6180ff110aa1055a5b9ef0d8b0a");

                searchResult.NotEmpty.Should().BeTrue();
                var result = searchResult.Results.First();

                var post = await loader.LoadPostAsync(result.Id);

                post.UgoiraFrameData.Should().NotBeNull();
                post.UgoiraFrameData.ContentType.Should().Be("image/jpeg");
                post.UgoiraFrameData.Data.Should().HaveCount(411);
                post.UgoiraFrameData.Data.Last().Delay.Should().Be(2800);
                post.UgoiraFrameData.Data.Last().File.Should().Be("000410.jpg");
            }

            [Fact]
            public async Task ShouldSafeRatingLevelAsGeneralMetadata()
            {
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var generalPost = await loader.LoadPostAsync(5576979);
                var sensitivePost = await loader.LoadPostAsync(5372463);
                var questionablePost = await loader.LoadPostAsync(5026269);
                var explicitPost = await loader.LoadPostAsync(236059);

                generalPost.ImageRating.Should().Be(Rating.Safe);
                generalPost.RatingSafeLevel.Should().Be(RatingSafeLevel.General);
                
                sensitivePost.ImageRating.Should().Be(Rating.Safe);
                sensitivePost.RatingSafeLevel.Should().Be(RatingSafeLevel.Sensitive);
                
                questionablePost.ImageRating.Should().Be(Rating.Questionable);
                questionablePost.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
                
                explicitPost.ImageRating.Should().Be(Rating.Explicit);
                explicitPost.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            }

            [Fact]
            public async Task ShouldGetTagsFromBannedPostMetadata()
            {
                var md5 = "6ff425beb52e662827e962fa82f96580";
                var loader = _loaderFixture.GetLoaderWithoutAuth();

                var foundBannedPosts = await loader.LoadSearchResultAsync($"md5:{md5}");

                var foundBannedPost = foundBannedPosts.Results.FirstOrDefault();
                foundBannedPost.Should().NotBeNull();
                foundBannedPost!.IsBanned.Should().BeTrue();

                var post = await (loader as IBooruAsyncBannedLoader).LoadBannedPostAsync(md5);

                post.Should().NotBeNull();
                post.Md5.Should().Be(md5);
                post.Tags.Count.Should().BeGreaterThan(30);
            }
        }

        public class FavoritePostAsyncMethod : DanbooruLoaderTests
        {
            public FavoritePostAsyncMethod(DanbooruLoaderFixture loaderFixture)
                : base(loaderFixture)
            {
            }

            [Fact]
            public async Task ShouldFavoritePost()
            {
                var api = _loaderFixture.GetApiAccessorWithAuth();
                await api.FavoritePostAsync(5004994);
            }
        }
    }
}
