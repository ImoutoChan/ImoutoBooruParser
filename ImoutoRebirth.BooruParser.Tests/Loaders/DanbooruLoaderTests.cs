using FluentAssertions;
using ImoutoRebirth.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace ImoutoRebirth.BooruParser.Tests.Loaders;

public class DanbooruLoaderTests : IClassFixture<DanbooruApiLoaderFixture>
{
    private readonly DanbooruApiLoaderFixture _loaderFixture;

    private DanbooruLoaderTests(DanbooruApiLoaderFixture loaderFixture)
    {
        _loaderFixture = loaderFixture;
    }

    public class GetPostAsyncMethod : DanbooruLoaderTests
    {
        public GetPostAsyncMethod(DanbooruApiLoaderFixture loaderFixture) : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldReturnPostWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(1);

            post.Should().NotBe(null);
            post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }
    }

    // public class LoadFirstTagHistoryPageAsyncMethod : DanbooruLoaderTests
    // {
    //     public LoadFirstTagHistoryPageAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldReturnWithCredentials()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithAuth();
    //
    //         var firstPage = await loader.LoadFirstTagHistoryPageAsync();
    //
    //         firstPage.Should().NotBeEmpty();
    //     }
    // }
    //
    // public class LoadSearchResultAsyncMethod : DanbooruLoaderTests
    // {
    //     public LoadSearchResultAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldFind()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var searchResult = await loader.LoadSearchResultAsync("1girl");
    //         searchResult.Results.Should().NotBeEmpty();
    //         searchResult.NotEmpty.Should().BeTrue();
    //         searchResult.SearchCount.Should().BeGreaterThan(1);
    //     }
    //
    //     [Fact]
    //     public async Task ShouldFindMd5OfDeletedPost()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var searchResult = await loader.LoadSearchResultAsync("md5:746310ab23d72e075755fd426469e31c");
    //         
    //         
    //         searchResult.Results.Should().NotBeEmpty();
    //         searchResult.NotEmpty.Should().BeTrue();
    //         searchResult.SearchCount.Should().Be(1);
    //         
    //         searchResult.Results.First().Id.Should().Be(0);
    //     }
    //
    //     [Fact]
    //     public async Task ShouldFindMd5OfRegularPost()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var searchResult = await loader.LoadSearchResultAsync("md5:4ff6bfa1745692b8eaf4ba2d2208c207");
    //         
    //         searchResult.Results.Should().NotBeEmpty();
    //         searchResult.NotEmpty.Should().BeTrue();
    //         searchResult.SearchCount.Should().Be(1);
    //         
    //         searchResult.Results.First().Id.Should().Be(5031817);
    //         searchResult.Results.First().Md5.Should().Be("4ff6bfa1745692b8eaf4ba2d2208c207");
    //     }
    // }

    // public class LoadNotesHistoryAsyncMethod : DanbooruLoaderTests
    // {
    //     public LoadNotesHistoryAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadNotesHistory()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var notesHistory = await loader.LoadNotesHistoryAsync(DateTime.Now.AddHours(-1));
    //         notesHistory.Should().NotBeEmpty();
    //     }
    // }
    // public class LoadTagHistoryUpToAsyncMethod : DanbooruLoaderTests
    // {
    //     public LoadTagHistoryUpToAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadWithAuth()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithAuth();
    //
    //         var notesHistory = await loader.LoadTagHistoryUpToAsync(DateTime.Now.AddHours(-1));
    //         notesHistory.Should().NotBeEmpty();
    //     }
    // }
    //
    // public class LoadTagHistoryFromAsyncMethod : DanbooruLoaderTests
    // {
    //     public LoadTagHistoryFromAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadTagsHistoryFromId()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithAuth();
    //         var firstTagHistoryPage = await loader.LoadFirstTagHistoryPageAsync();
    //
    //         var tagHistory = await loader.LoadTagHistoryFromAsync(firstTagHistoryPage.Last().UpdateId);
    //
    //         tagHistory.Should().NotBeEmpty();
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadTagsHistoryWithParentChanges()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithAuth();
    //
    //         var tagsHistory = await loader.LoadTagHistoryFromAsync(43125946);
    //
    //         tagsHistory.Should().NotBeEmpty();
    //         tagsHistory.First(x => x.UpdateId == 43125965).ParentChanged.Should().BeTrue();
    //         tagsHistory.First(x => x.UpdateId == 43125965).Parent.Should().BeNull();
    //         
    //         tagsHistory.First(x => x.UpdateId == 43125951).ParentChanged.Should().BeFalse();
    //         tagsHistory.First(x => x.UpdateId == 43125951).Parent.Should().BeNull();
    //         
    //         tagsHistory.First(x => x.UpdateId == 43125948).ParentChanged.Should().BeFalse();
    //         tagsHistory.First(x => x.UpdateId == 43125948).Parent?.Id.Should().Be(4978487);
    //     }
    // }
    //
    // public class LoadPopularAsyncMethod : DanbooruLoaderTests
    // {
    //     public LoadPopularAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadPopularForDay()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var searchResult = await loader.LoadPopularAsync(PopularType.Day);
    //
    //         searchResult.Results.Should().NotBeEmpty();
    //         searchResult.NotEmpty.Should().BeTrue();
    //         searchResult.SearchCount.Should().BeGreaterThan(1);
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadPopularForWeek()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var searchResult = await loader.LoadPopularAsync(PopularType.Week);
    //
    //         searchResult.Results.Should().NotBeEmpty();
    //         searchResult.NotEmpty.Should().BeTrue();
    //         searchResult.SearchCount.Should().BeGreaterThan(1);
    //     }
    //
    //     [Fact]
    //     public async Task ShouldLoadPopularForMonth()
    //     {
    //         var loader = _loaderFixture.GetLoaderWithoutAuth();
    //
    //         var serachResult = await loader.LoadPopularAsync(PopularType.Month);
    //
    //         serachResult.Results.Should().NotBeEmpty();
    //         serachResult.NotEmpty.Should().BeTrue();
    //         serachResult.SearchCount.Should().BeGreaterThan(1);
    //     }
    // }

    public class LoadPostMetadataAsyncMethod : DanbooruLoaderTests
    {
        public LoadPostMetadataAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadParentsAndChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(2392459);
            
            post.Tags.Count.Should().BeGreaterThan(30);
            post.ChildrenIds.Count.Should().NotBe(0);
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().BeGreaterThan(0);
            post.Parent!.Md5Hash.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Bug with post 5032478
        /// </summary>
        [Fact]
        public async Task ShouldLoadChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5032478);

            post.Tags.Count.Should().BeGreaterThan(30);
            post.ChildrenIds.Count.Should().Be(2);
            post.Parent.Should().BeNull();
        }

        /// <summary>
        /// Bug with post 5314036
        /// </summary>
        [Fact]
        public async Task ShouldLoadChildrenFor5314036()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5314036);

            post.Tags.Count.Should().BeGreaterThan(30);
            post.ChildrenIds.Count.Should().Be(1);
            post.ChildrenIds.First().Should().Be(new PostIdentity(5318896, "46dda085dc9c60dd4380ed7b4433aa41"));
            post.Parent.Should().BeNull();
        }
            
        /// <summary>
        /// Bug with post 5666656
        /// </summary>
        [Fact]
        public async Task ShouldLoadSampleUrlFor5666656()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5666656);

            post.Should().NotBeNull();
            post.SampleUrl.Should()
                .Be("https://cdn.donmai.us/sample/4a/8b/sample-4a8b6ecee31d9e66e5532f22b19ab736.webm");
        }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(5176287);
            
            post.Notes.Count.Should().Be(6);
        }
            
        [Fact]
        public async Task ShouldLoadMetaTags()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync(3811474);
            
            post.Tags.Should().Contain(x => x.Name == "paid reward");
        }
            
        [Fact]
        public async Task ShouldLoadPools()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(5246157);
            
            post.Pools.Count.Should().BeGreaterOrEqualTo(1);
        }
            
        [Fact]
        public async Task ShouldLoadManyPools()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(599785);
            
            post.Pools.Count.Should().BeGreaterOrEqualTo(3);
        }
            
        [Fact]
        public async Task ShouldLoadUgoiraMetadata()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(4278890);
            
            post.UgoiraFrameDelays.Should().NotBeNull();
            post.UgoiraFrameDelays.Should().HaveCount(411);
            post.UgoiraFrameDelays.Last().Should().Be(2800);
        }

        [Fact]
        public async Task ShouldSafeRatingLevelAsGeneralMetadata()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var generalPost = await loader.GetPostAsync(5765281);
            var sensitivePost = await loader.GetPostAsync(5372463);
            var questionablePost = await loader.GetPostAsync(5026269);
            var explicitPost = await loader.GetPostAsync(236059);

            generalPost.Rating.Should().Be(Rating.Safe);
            generalPost.RatingSafeLevel.Should().Be(RatingSafeLevel.General);
                
            sensitivePost.Rating.Should().Be(Rating.Safe);
            sensitivePost.RatingSafeLevel.Should().Be(RatingSafeLevel.Sensitive);
                
            questionablePost.Rating.Should().Be(Rating.Questionable);
            questionablePost.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
                
            explicitPost.Rating.Should().Be(Rating.Explicit);
            explicitPost.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
        }

        [Fact]
        public async Task ShouldGetRestrictedWithSomeData()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var restrictedPost = await loader.GetPostAsync(5387246);

            restrictedPost.Tags.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldGetBannedPostWithSomeData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostAsync(5069825);

            restrictedPost.OriginalUrl.Should().BeNullOrEmpty();
            restrictedPost.Tags.Should().NotBeEmpty();
        }
    }

    // public class FavoritePostAsyncMethod : DanbooruLoaderTests
    // {
    //     public FavoritePostAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
    //         : base(loaderFixture)
    //     {
    //     }
    //
    //     [Fact]
    //     public async Task ShouldFavoritePost()
    //     {
    //         var api = _loaderFixture.GetApiAccessorWithAuth();
    //         await api.FavoritePostAsync(5004994);
    //     }
    // }
}
