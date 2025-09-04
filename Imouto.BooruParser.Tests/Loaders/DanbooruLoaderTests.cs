using System.Net;
using AwesomeAssertions;
using Imouto.BooruParser.Implementations;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders;

public class DanbooruLoaderTests : IClassFixture<DanbooruApiLoaderFixture>, IClassFixture<GelbooruApiLoaderFixture>
{
    private readonly DanbooruApiLoaderFixture _loaderFixture;
    private readonly GelbooruApiLoaderFixture? _gelbooruApiLoaderFixture;

    private DanbooruLoaderTests(
        DanbooruApiLoaderFixture loaderFixture, 
        GelbooruApiLoaderFixture? gelbooruApiLoaderFixture = null)
    {
        _loaderFixture = loaderFixture;
        _gelbooruApiLoaderFixture = gelbooruApiLoaderFixture;
    }

    public class GetPostAsyncMethod : DanbooruLoaderTests
    {
        public GetPostAsyncMethod(
            DanbooruApiLoaderFixture loaderFixture,
            GelbooruApiLoaderFixture gelbooruApiLoaderFixture) : base(loaderFixture, gelbooruApiLoaderFixture)
        {
        }

        [Fact]
        public async Task ShouldDownloadPostMedia()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync(5773061);
            var mediaUrl = post.OriginalUrl;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "UnitTestBot/1.0");
            var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, mediaUrl));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ShouldDownloadPostMediaThroughGelbooru()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync(5773061);

            var gelbooruLoader = _gelbooruApiLoaderFixture!.GetLoader();
            var gelbooruPost = await gelbooruLoader.GetPostByMd5Async(post.Id.Md5Hash);
            var mediaUrl = gelbooruPost!.OriginalUrl;
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "UnitTestBot/1.0");
            var result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, mediaUrl));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ShouldReturnPostWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5773061);

            await Verify(post);
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
        public async Task ShouldNavigateSearch()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var searchResult = await loader.SearchAsync("1girl");
            searchResult.Results.Should().NotBeEmpty();
            searchResult.PageNumber.Should().Be(1);

            var searchResultNext = await loader.GetNextPageAsync(searchResult);
            searchResultNext.Results.Should().NotBeEmpty();
            searchResultNext.PageNumber.Should().Be(2);

            var searchResultPrev = await loader.GetPreviousPageAsync(searchResultNext);
            searchResultPrev.Results.Should().NotBeEmpty();
            searchResultPrev.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task ShouldFindMd5OfBannedPost()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var searchResult = await loader.SearchAsync("md5:746310ab23d72e075755fd426469e31c");

            await Verify(searchResult);
        }
    
        [Fact]
        public async Task ShouldFindMd5OfRegularPost()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var searchResult = await loader.SearchAsync("md5:4ff6bfa1745692b8eaf4ba2d2208c207");

            await Verify(searchResult);
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
    
            result.Should().HaveCountGreaterThanOrEqualTo(299);
            result.DistinctBy(x => x.HistoryId).Should().HaveCount(result.Count);
        }
    
        [Fact]
        public async Task ShouldLoadTagsHistoryWithParentChanges()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var page = await loader.GetTagHistoryPageAsync(new SearchToken("a43125946"));

            await Verify(page);
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
            result.Results.First().Id.Should().NotBeEmpty();
            result.Results.First().Title.Should().NotBeNullOrWhiteSpace();
            result.Results.First().Md5Hash.Should().NotBeNullOrWhiteSpace();
        }
    
        [Fact]
        public async Task ShouldLoadPopularForWeek()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var result = await loader.GetPopularPostsAsync(PopularType.Week);
    
            result.Results.Should().NotBeEmpty();
            result.Results.First().Id.Should().NotBeEmpty();
            result.Results.First().Title.Should().NotBeNullOrWhiteSpace();
            result.Results.First().Md5Hash.Should().NotBeNullOrWhiteSpace();
        }
    
        [Fact]
        public async Task ShouldLoadPopularForMonth()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
    
            var result = await loader.GetPopularPostsAsync(PopularType.Month);
    
            result.Results.Should().NotBeEmpty();
            result.Results.First().Id.Should().NotBeEmpty();
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

            await Verify(post);
        }

        /// <summary>
        /// Bug with post 5032478
        /// </summary>
        [Fact]
        public async Task ShouldLoadChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5032478);

            await Verify(post);
        }

        /// <summary>
        /// Bug with post 5314036
        /// </summary>
        [Fact]
        public async Task ShouldLoadChildrenFor5314036()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5314036);

            await Verify(post);
        }
            
        /// <summary>
        /// Bug with post 5666656
        /// </summary>
        [Fact]
        public async Task ShouldLoadSampleUrlFor5666656()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(5666656);

            await Verify(post);
        }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(5176287);

            await Verify(post);
        }
            
        [Fact]
        public async Task ShouldLoadMetaTags()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync(3811474);

            await Verify(post);
        }
            
        [Fact]
        public async Task ShouldLoadPools()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(5246157);

            await Verify(post);
        }

        [Fact]
        public async Task ShouldLoadPoolWithPosition()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostByMd5Async("7fb1c60a41e2f71684835e9c9bdaa2d9");

            await Verify(post);
        }
            
        [Fact]
        public async Task ShouldLoadManyPools()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(599785);

            await Verify(post);
        }
            
        [Fact]
        public async Task ShouldLoadUgoiraMetadata()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            
            var post = await loader.GetPostAsync(4278890);

            await Verify(post);
        }

        [Fact]
        public async Task ShouldSafeRatingLevelAsGeneralMetadata()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var generalPost = await loader.GetPostAsync(5765281);
            var sensitivePost = await loader.GetPostAsync(5372463);
            var questionablePost = await loader.GetPostAsync(5026269);
            var explicitPost = await loader.GetPostAsync(236059);

            await Verify(new [] {generalPost, sensitivePost, questionablePost, explicitPost});
        }

        [Fact]
        public async Task ShouldGetRestrictedWithSomeData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostAsync(5387246);

            await Verify(restrictedPost);
        }

        [Fact] 
        public async Task ShouldGetDeletedWithFullData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostAsync(5707498);

            await Verify(restrictedPost);
        }

        [Fact(Skip = "Works only with gold accounts")] 
        public async Task ShouldGetRestrictedByTagsWithFullData()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var restrictedPost = await loader.GetPostAsync(5387246);

            await Verify(restrictedPost);
        }

        [Theory]
        [InlineData("5069825")]
        [InlineData("5767795")]
        public async Task ShouldGetBannedPostWithSomeData(string id)
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

            await Verify(post);
        }

        [Fact]
        public async Task ShouldGetBannedPostByMd5WithSomeData()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var restrictedPost = await loader.GetPostByMd5Async("b9b933c1835d043ec38cbefbe78554eb");

            await Verify(restrictedPost);
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
