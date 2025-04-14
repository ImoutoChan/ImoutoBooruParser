using FluentAssertions;
using Imouto.BooruParser.Extensions;
using Imouto.BooruParser.Implementations;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders;

// This line will skip all tests in file
// xUnit doesn't support skipping all tests in class
// Comment this line to enable tests
using FactAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;

public class SankakuLoaderTests : IClassFixture<SankakuLoaderFixture>
{
    private readonly SankakuLoaderFixture _loaderFixture;

    public SankakuLoaderTests(SankakuLoaderFixture loaderFixture) => _loaderFixture = loaderFixture;

    public class GetPostAsyncMethod : SankakuLoaderTests
    {
        public GetPostAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldReturnPostWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync("jXajkOWmor2");

            post.Should().NotBe(null);
            post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ShouldGetPostAsync()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var post = await loader.GetPostAsync("YoMBDZgQJrO");

            post.Should().NotBeNull();
            post.OriginalUrl.Should().StartWith("https://v.sankakucomplex.com/data/de/aa/deaac52a6b001b6953db90a09f7629f7.jpg");
            post.Id.Id.Should().Be("YoMBDZgQJrO");
            post.Id.Md5Hash.Should().Be("deaac52a6b001b6953db90a09f7629f7");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(123);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf(SankakuTagTypes());
            }
            
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().Be("PVaD8oPQ7ab");
            post.Parent!.Md5Hash.Should().Be("8f37e824ec321d96f0e149d77ee5d21d");
            post.Pools.Should().HaveCount(2);
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be(null);
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Width.Should().Be(756);
            post.FileResolution.Height.Should().Be(1052);
            post.PostedAt.Should().Be(new DateTimeOffset(2017, 12, 18, 21, 22, 21, 0, TimeSpan.Zero));
            post.SampleUrl.Should().StartWith("https://v.sankakucomplex.com/data/de/aa/deaac52a6b001b6953db90a09f7629f7.jpg");
            post.UploaderId.Id.Should().Be("8yrx2mEgME6");
            post.UploaderId.Name.Should().Be("Domestikun");
            post.FileSizeInBytes.Should().Be(617163);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldGetPostByMd5Async()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var post = await loader.GetPostByMd5Async("deaac52a6b001b6953db90a09f7629f7");

            post.Should().NotBeNull();
            post!.OriginalUrl.Should().StartWith("https://v.sankakucomplex.com/data/de/aa/deaac52a6b001b6953db90a09f7629f7.jpg");
            post.Id.Id.Should().Be("YoMBDZgQJrO");
            post.Id.Md5Hash.Should().Be("deaac52a6b001b6953db90a09f7629f7");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(123);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf(SankakuTagTypes());
            }
            
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().Be("PVaD8oPQ7ab");
            post.Parent!.Md5Hash.Should().Be("8f37e824ec321d96f0e149d77ee5d21d");
            post.Pools.Should().HaveCount(2);
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be(null);
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(756, 1052));
            post.PostedAt.Should().Be(new DateTimeOffset(2017, 12, 18, 21, 22, 21, 0, TimeSpan.Zero));
            post.SampleUrl.Should().StartWith("https://v.sankakucomplex.com/data/de/aa/deaac52a6b001b6953db90a09f7629f7.jpg");
            post.UploaderId.Id.Should().Be("8yrx2mEgME6");
            post.UploaderId.Name.Should().Be("Domestikun");
            post.FileSizeInBytes.Should().Be(617163);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldGetPostByMd5Async_dc9da74597ecd47b2848fb4d68fce77a()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var post = await loader.GetPostByMd5Async("dc9da74597ecd47b2848fb4d68fce77a");

            post.Should().NotBeNull();
            post!.OriginalUrl.Should().StartWith("https://v.sankakucomplex.com/data/dc/9d/dc9da74597ecd47b2848fb4d68fce77a.mp4");
            post.Id.Id.Should().Be("P7RLK8e90r6");
            post.Id.Md5Hash.Should().Be("dc9da74597ecd47b2848fb4d68fce77a");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(101);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf(SankakuTagTypes());
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().HaveCount(0);
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be(null);
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1644, 1862));
            post.PostedAt.Should().Be(new DateTimeOffset(2023, 09, 29, 5, 54, 09, 0, TimeSpan.Zero));
            post.SampleUrl.Should().StartWith("https://v.sankakucomplex.com/data/dc/9d/dc9da74597ecd47b2848fb4d68fce77a.mp4");
            post.UploaderId.Id.Should().Be("YoMB0X4BrOv");
            post.UploaderId.Name.Should().Be("Just_some_guy");
            post.FileSizeInBytes.Should().Be(22152413);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldGetPostByMd5Async_d62ed6aebd2b75aa9661795b54a957d7()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var post = await loader.GetPostByMd5Async("d62ed6aebd2b75aa9661795b54a957d7");

            post.Should().NotBeNull();
            post!.Tags.Should().HaveCount(66);
        }

        [Fact]
        public async Task ShouldContainLinkWithoutAmp()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync("qEMAek6EWMJ");

            post.OriginalUrl.Should().NotContain("&amp;");
        }

        [Fact]
        public async Task ShouldContainTagsWithoutAmp()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();
            var post = await loader.GetPostAsync("1QaE6NQQyr9");

            post.Tags.Should().AllSatisfy(x => (x.Name + x.Type)
                .Should()
                .NotContain("&#"));
        }
    }
        
    public class SearchAsyncMethod : SankakuLoaderTests
    {
        public SearchAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldFind()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.SearchAsync("1girl");
            result.Results.Should().NotBeEmpty();
            result.Results.Should().HaveCountGreaterThan(1);
        }

        //[Fact(Skip = "Clown Sankaku only allow now 1 tag for free accounts")]
        public async Task ShouldFindWithMultipleTags()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.SearchAsync("1girl long_hair");
            result.Results.Should().NotBeEmpty();
            result.Results.Should().HaveCountGreaterThan(1);
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
    }

    public class LoadNotesHistoryAsyncMethod : SankakuLoaderTests
    {
        public LoadNotesHistoryAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadNotesHistory()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var notesHistory = await loader.GetNoteHistoryToDateTimeAsync(DateTime.Now.AddHours(-1)).ToListAsync();
            notesHistory.Should().NotBeEmpty();
        }
    }

    public class GetTagHistoryFirstPageAsyncMethod : SankakuLoaderTests
    {
        public GetTagHistoryFirstPageAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public void ShouldThrowWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            Func<Task> action = async () => await loader.GetTagHistoryFirstPageAsync();

            action.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task ShouldReturnWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var firstPage = await loader.GetTagHistoryFirstPageAsync();

            firstPage.Should().NotBeEmpty();
        }
    }

    public class GetTagHistoryToDateTimeAsyncMethod : SankakuLoaderTests
    {
        public GetTagHistoryToDateTimeAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public void ShouldNotLoadTagsHistoryToDateWithoutCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            Func<Task> action = async ()
                => await loader.GetTagHistoryToDateTimeAsync(DateTime.Now.AddMinutes(-5)).ToListAsync();

            action.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryToDateWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var history = await loader.GetTagHistoryToDateTimeAsync(DateTime.Now.AddMinutes(-5)).ToListAsync();
            history.Should().NotBeEmpty();
        }
    }

    public class GetTagHistoryFromIdToPresentAsyncMethod : SankakuLoaderTests
    {
        public GetTagHistoryFromIdToPresentAsyncMethod(SankakuLoaderFixture loaderFixture) 
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryFromIdWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();
            var firstTagHistoryPage = await loader.GetTagHistoryFirstPageAsync();
            firstTagHistoryPage.Should().NotBeEmpty();

            var notesHistory = await loader.GetTagHistoryFromIdToPresentAsync(firstTagHistoryPage.Last().HistoryId).ToListAsync();

            notesHistory.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadTagsHistoryFromIdAndHaveAllDataWithCredentials()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();
            var firstTagHistoryPage = await loader.GetTagHistoryFirstPageAsync();
            firstTagHistoryPage.Should().NotBeEmpty();

            var tagsHistory = await loader
                .GetTagHistoryFromIdToPresentAsync(firstTagHistoryPage.Last().HistoryId - 100)
                .ToListAsync();

            tagsHistory.Should().NotBeEmpty();
            tagsHistory.Count.Should().BeGreaterThanOrEqualTo(firstTagHistoryPage.Count + 100);
            tagsHistory.Select(x => x.PostId).Should().Contain(firstTagHistoryPage.Select(x => x.PostId));
        }
    }

    public class GetPopularPostsAsyncMethod : SankakuLoaderTests
    {
        public GetPopularPostsAsyncMethod(SankakuLoaderFixture sankakuLoaderFixture)
            : base(sankakuLoaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadPopularForDay()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.GetPopularPostsAsync(PopularType.Day);

            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadPopularForWeek()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.GetPopularPostsAsync(PopularType.Week);

            result.Results.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadPopularForMonth()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.GetPopularPostsAsync(PopularType.Month);

            result.Results.Should().NotBeEmpty();
        }
    }

    public class LoadPostMetadataAsyncMethod : SankakuLoaderTests
    {
        public LoadPostMetadataAsyncMethod(SankakuLoaderFixture sankakuLoaderFixture)
            : base(sankakuLoaderFixture)
        {
        }

        [Fact]
        public async Task ShouldLoadParentsAndChildren()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();

            var post = await loader.GetPostAsync("8JaGzJm6oML");

            post.Tags.Count.Should().BeGreaterThan(30);
            post.ChildrenIds.Count.Should().NotBe(0);
            post.Parent!.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldLoadChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync("1QaE73DqNR9");

            post.ChildrenIds.Should().HaveCount(1);
            post.ChildrenIds.Distinct().Should().HaveCount(post.ChildrenIds.Count);
            post.ChildrenIds.First().Should().Be(new PostIdentity("1QaE73DqBR9", "25d539de97741a801d64f9158d3581a9"));
        }

        [Fact]
        public async Task ShouldLoadPostWithEmptyChildren()
        {
            var loader = _loaderFixture.GetLoaderWithAuth();
            var post = await loader.GetPostByMd5Async("692078f4b19d8a7992bc361baac39650");

            post.Should().NotBeNull();
            post!.Tags.Count.Should().BeGreaterThan(30);

            // no longer empty, but inaccessible
            // and now empty again
            post.ChildrenIds.Count.Should().Be(0);
        }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync("6ea46lYb5R3");

            post.Notes.Count.Should().Be(4);
            
            post.Notes.ElementAt(0).Id.Should().Be("9krZZB36rg8");
            post.Notes.ElementAt(0).Text.Should().Be("Wishes to be a slave");
            post.Notes.ElementAt(0).Size.Should().Be(new Size(620, 280));
            post.Notes.ElementAt(0).Point.Should().Be(new Position(646, 92));
            
            post.Notes.ElementAt(1).Id.Should().Be("b8aJE5AlM2L");
            post.Notes.ElementAt(1).Text.Should().Be("H cup\nHuge breasts <3");
            post.Notes.ElementAt(1).Size.Should().Be(new Size(574, 434));
            post.Notes.ElementAt(1).Point.Should().Be(new Position(475, 2229));
            
            post.Notes.ElementAt(2).Id.Should().Be("8yrxeXqvRE6");
            post.Notes.ElementAt(2).Text.Should().Be("Slutty voice <3");
            post.Notes.ElementAt(2).Size.Should().Be(new Size(550, 224));
            post.Notes.ElementAt(2).Point.Should().Be(new Position(84, 2171));
            
            post.Notes.ElementAt(3).Id.Should().Be("6Qa8bwAjR9A");
            post.Notes.ElementAt(3).Text.Should().Be("Vulgar\nOrgasm face <3");
            post.Notes.ElementAt(3).Size.Should().Be(new Size(563, 311));
            post.Notes.ElementAt(3).Point.Should().Be(new Position(60, 118));
        }
            
        [Fact]
        public async Task ShouldLoadSampleUrl()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync("qXMOAG3y2rl");

            post.Should().NotBeNull();

            // no longer true, sankaku just returns the usual url here
            // post.SampleUrl.Should().Contain("sample");

            post.PreviewUrl.Should().Contain("preview");
        }
    }
    
    public class FavoritePostAsyncMethod : SankakuLoaderTests
    {
        public FavoritePostAsyncMethod(SankakuLoaderFixture loaderFixture)
            : base(loaderFixture)
        {
        }

        [Fact]
        public async Task ShouldFavoritePost()
        {
            var api = _loaderFixture.GetAccessorWithAuth();
            await api.FavoritePostAsync("YoMBDZgQJrO");
        }
    }

    private static string[] SankakuTagTypes()
    {
        return new [] {"meta", "general", "copyright", "character", "circle", "artist", "medium", "genre",

            // new
            "activity", "anatomy", "fashion", "pose", "role", "object", "substance", "setting", "automatic" };
    }
}
