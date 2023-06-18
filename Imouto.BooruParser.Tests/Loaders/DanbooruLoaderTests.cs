using FluentAssertions;
using Imouto.BooruParser.Implementations;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders;

public class DanbooruLoaderTests : IClassFixture<DanbooruApiLoaderFixture>
{
    private readonly DanbooruApiLoaderFixture _loaderFixture;

    private DanbooruLoaderTests(DanbooruApiLoaderFixture loaderFixture) => _loaderFixture = loaderFixture;

    public class GetPostAsyncMethod : DanbooruLoaderTests
    {
        public GetPostAsyncMethod(DanbooruApiLoaderFixture loaderFixture) : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldReturnPostWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5773061);

            post.Should().NotBeNull();
            post.OriginalUrl.Should().Be("https://cdn.donmai.us/original/54/3f/543f49b2d9fd4e31d8cb10ceaff6cad7.jpg");
            post.Id.Id.Should().Be(5773061);
            post.Id.Md5Hash.Should().Be("543f49b2d9fd4e31d8cb10ceaff6cad7");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(35);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf(new[] { "meta", "general", "copyright", "character", "circle", "artist" });
            }
            
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().Be(5775694);
            post.Parent!.Md5Hash.Should().Be("886823ace72390fe7a8926e2ffe3299d");
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Safe);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.Sensitive);
            post.Source.Should().Be("https://twitter.com/jewel_milk/status/1584877432959541250");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1000, 1414));
            post.PostedAt.Should().Be(new DateTimeOffset(2022, 10, 25, 12, 01, 24, 980, TimeSpan.Zero));
            post.SampleUrl.Should().Be("https://cdn.donmai.us/sample/54/3f/sample-543f49b2d9fd4e31d8cb10ceaff6cad7.jpg");
            post.UploaderId.Id.Should().Be(508969);
            post.UploaderId.Name.Should().Be("Topsy Krett");
            post.FileSizeInBytes.Should().Be(151135);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
    }
    
    public class LoadSearchResultAsyncMethod : DanbooruLoaderTests
    {
        public LoadSearchResultAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }
    
        [Fact]
        public async Task ShouldFind()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var searchResult = await loader.SearchAsync("1girl");
            searchResult.Results.Should().NotBeEmpty();
        }
    
        [Fact]
        public async Task ShouldFindMd5OfBannedPost()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var searchResult = await loader.SearchAsync("md5:746310ab23d72e075755fd426469e31c");
            
            searchResult.Results.Should().NotBeEmpty();
            
            searchResult.Results.First().Id.Should().Be(3630304);
            searchResult.Results.First().Md5Hash.Should().BeNull();
        }
    
        [Fact]
        public async Task ShouldFindMd5OfRegularPost()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var searchResult = await loader.SearchAsync("md5:4ff6bfa1745692b8eaf4ba2d2208c207");
            
            searchResult.Results.Should().NotBeEmpty();
            
            searchResult.Results.First().Id.Should().Be(5031817);
            searchResult.Results.First().Md5Hash.Should().Be("4ff6bfa1745692b8eaf4ba2d2208c207");
        }
    }

    public class LoadNotesHistoryAsyncMethod : DanbooruLoaderTests
    {
        public LoadNotesHistoryAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }
    
        [Fact]
        public async Task ShouldGetUpToDateTime()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var result = new List<NoteHistoryEntry>();
            await foreach (var item in loader.GetNoteHistoryToDateTimeAsync(DateTime.Now.AddHours(-1))) 
                result.Add(item);

            result.Should().NotBeEmpty();
            result.DistinctBy(x => x.HistoryId).Should().HaveCount(result.Count);
        }
        
        [Fact]
        public async Task ShouldGetFirstPage()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var firstPage = await loader.GetNoteHistoryFirstPageAsync();
    
            firstPage.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task ShouldLoadNotesHistoryFromId()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var page = await loader.GetNoteHistoryPageAsync(new SearchToken("3"));
    
            var result = new List<NoteHistoryEntry>();
            await foreach (var item in loader.GetNoteHistoryFromIdToPresentAsync(page.Results.Last().HistoryId))
                result.Add(item);
    
            result.Should().HaveCountGreaterThanOrEqualTo(299);
            result.DistinctBy(x => x.HistoryId).Should().HaveCount(result.Count);
        }
    }
    public class LoadTagHistory : DanbooruLoaderTests
    {
        public LoadTagHistory(DanbooruApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }
    
        [Fact]
        public async Task ShouldGetUpToDateTime()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = new List<TagHistoryEntry>();
            await foreach (var item in loader.GetTagHistoryToDateTimeAsync(DateTime.Now.AddHours(-1))) 
                result.Add(item);

            result.Should().NotBeEmpty();
            result.DistinctBy(x => x.HistoryId).Should().HaveCount(result.Count);
        }
    
        [Fact]
        public async Task ShouldGetFirstPage()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var firstPage = await loader.GetTagHistoryFirstPageAsync();
    
            firstPage.Should().NotBeEmpty();
            firstPage.Should().HaveCount(100);
        }
        
        [Fact]
        public async Task ShouldGetFirstPageWith1000Results()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var firstPage = await loader.GetTagHistoryFirstPageAsync(1000);
    
            firstPage.Should().NotBeEmpty();
            firstPage.Should().HaveCount(1000);
        }
    
        [Fact]
        public async Task ShouldLoadTagsHistoryFromId()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var page = await loader.GetTagHistoryPageAsync(new SearchToken("3"));
    
            var result = new List<TagHistoryEntry>();
            await foreach (var item in loader.GetTagHistoryFromIdToPresentAsync(page.Results.Last().HistoryId))
                result.Add(item);
    
            result.Should().HaveCountGreaterOrEqualTo(299);
            result.DistinctBy(x => x.HistoryId).Should().HaveCount(result.Count);
        }
    
        [Fact]
        public async Task ShouldLoadTagsHistoryWithParentChanges()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var page = await loader.GetTagHistoryPageAsync(new SearchToken("a43125946"));
            var tagsHistory = page.Results;
    
            tagsHistory.Should().NotBeEmpty();
            tagsHistory.First(x => x.HistoryId == 43125965).ParentChanged.Should().BeTrue();
            tagsHistory.First(x => x.HistoryId == 43125965).ParentId.Should().BeNull();
            
            tagsHistory.First(x => x.HistoryId == 43125951).ParentChanged.Should().BeFalse();
            tagsHistory.First(x => x.HistoryId == 43125951).ParentId.Should().BeNull();
            
            tagsHistory.First(x => x.HistoryId == 43125948).ParentChanged.Should().BeFalse();
            tagsHistory.First(x => x.HistoryId == 43125948).ParentId.Should().Be(4978487);
        }
        
        [Fact]
        public async Task ShouldReturnNextNumberToken()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var firstPage = await loader.GetTagHistoryPageAsync(null);

            firstPage.NextToken?.Page.Should().StartWith("b");
        }
        
        [Fact]
        public async Task ShouldReturnNextNumberTokenFor2()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var firstPage = await loader.GetTagHistoryPageAsync(new SearchToken("2"));

            firstPage.NextToken?.Page.Should().Be("3");
        }
        
        [Fact]
        public async Task ShouldReturnNextNumberTokenForA()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var firstPage = await loader.GetTagHistoryPageAsync(new SearchToken("a100"));

            firstPage.NextToken?.Page.Should().StartWith("a");
        }
        
        [Fact]
        public async Task ShouldReturnNextNumberTokenForB()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var firstPage = await loader.GetTagHistoryPageAsync(new SearchToken("b10000"));

            firstPage.NextToken?.Page.Should().StartWith("b");
        }
    }
    
    public class LoadPopularAsyncMethod : DanbooruLoaderTests
    {
        public LoadPopularAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }
    
        [Fact]
        public async Task ShouldLoadPopularForDay()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var result = await loader.GetPopularPostsAsync(PopularType.Day);
    
            result.Results.Should().NotBeEmpty();
            result.Results.First().Id.Should().BeGreaterThan(0);
            result.Results.First().Title.Should().NotBeNullOrWhiteSpace();
            result.Results.First().Md5Hash.Should().NotBeNullOrWhiteSpace();
        }
    
        [Fact]
        public async Task ShouldLoadPopularForWeek()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var result = await loader.GetPopularPostsAsync(PopularType.Week);
    
            result.Results.Should().NotBeEmpty();
            result.Results.First().Id.Should().BeGreaterThan(0);
            result.Results.First().Title.Should().NotBeNullOrWhiteSpace();
            result.Results.First().Md5Hash.Should().NotBeNullOrWhiteSpace();
        }
    
        [Fact]
        public async Task ShouldLoadPopularForMonth()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var result = await loader.GetPopularPostsAsync(PopularType.Month);
    
            result.Results.Should().NotBeEmpty();
            result.Results.First().Id.Should().BeGreaterThan(0);
            result.Results.First().Title.Should().NotBeNullOrWhiteSpace();
            result.Results.First().Md5Hash.Should().NotBeNullOrWhiteSpace();
        }
    }

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
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostAsync(5387246);

            restrictedPost.Tags.Should().NotBeEmpty();
        }

        [Fact] 
        public async Task ShouldGetDeletedWithFullData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostAsync(5707498);

            restrictedPost.Tags.Should().NotBeEmpty();
            restrictedPost.Id.Md5Hash.Should().NotBeNullOrWhiteSpace();
            restrictedPost.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact(Skip = "Works only with gold accounts")] 
        public async Task ShouldGetRestrictedByTagsWithFullData()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var restrictedPost = await loader.GetPostAsync(5387246);

            restrictedPost.Tags.Should().NotBeEmpty();
            restrictedPost.Id.Md5Hash.Should().NotBeNullOrWhiteSpace();
            restrictedPost.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(5069825)]
        [InlineData(5767795)]
        public async Task ShouldGetBannedPostWithSomeData(int id)
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostAsync(id);

            restrictedPost.OriginalUrl.Should().BeNullOrEmpty();
            restrictedPost.Tags.Should().NotBeEmpty();
        }

        [Fact] 
        public async Task ShouldGetByMd5WithFullData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostByMd5Async("fcf0c189e898edcb316ea0b61096c622");
            
            post.Should().NotBeNull();
            post!.Id.Id.Should().Be(5766237);
            post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ShouldGetBannedPostByMd5WithSomeData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostByMd5Async("b9b933c1835d043ec38cbefbe78554eb");

            restrictedPost.Should().NotBeNull();
            restrictedPost!.Id.Id.Should().Be(5767795);
            restrictedPost.OriginalUrl.Should().BeNull();
            restrictedPost.Tags.Should().NotBeEmpty();
        }
    }

    public class FavoritePostAsyncMethod : DanbooruLoaderTests
    {
        public FavoritePostAsyncMethod(DanbooruApiLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }
    
        [Fact]
        public async Task ShouldFavoritePost()
        {
            var api = _loaderFixture.GetApiAccessorWithAuth();
            await api.FavoritePostAsync(5768298);
        }
    }
}
