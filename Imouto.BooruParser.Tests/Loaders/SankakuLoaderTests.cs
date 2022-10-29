using FluentAssertions;
using Imouto.BooruParser.Extensions;
using Imouto.BooruParser.Implementations;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders;

// This line will skip all tests in file
// xUnit doesn't support skipping all tests in class
// Comment this line to enable tests
// using FactAttribute = System.Runtime.CompilerServices.CompilerGeneratedAttribute;

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

            var post = await loader.GetPostAsync(5735331);

            post.Should().NotBe(null);
            post.OriginalUrl.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ShouldGetPostAsync()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(6541010);

            post.Should().NotBeNull();
            post.OriginalUrl.Should().StartWith("https://v.sankakucomplex.com/data/de/aa/deaac52a6b001b6953db90a09f7629f7.jpg");
            post.Id.Id.Should().Be(6541010);
            post.Id.Md5Hash.Should().Be("deaac52a6b001b6953db90a09f7629f7");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(113);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf(new[] { "meta", "general", "copyright", "character", "circle", "artist", "medium", "genre" });
            }
            
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().Be(6541009);
            post.Parent!.Md5Hash.Should().Be("8f37e824ec321d96f0e149d77ee5d21d");
            post.Pools.Should().HaveCount(4);
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be("https://www.pixiv.net/member_illust.php?mode=medium&illust_id=66351185");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Width.Should().Be(756);
            post.FileResolution.Height.Should().Be(1052);
            post.PostedAt.Should().Be(new DateTimeOffset(2017, 12, 18, 21, 22, 21, 0, TimeSpan.Zero));
            post.SampleUrl.Should().StartWith("https://v.sankakucomplex.com/data/sample/de/aa/sample-deaac52a6b001b6953db90a09f7629f7.jpg");
            post.UploaderId.Id.Should().Be(231462);
            post.UploaderId.Name.Should().Be("Domestikun");
            post.FileSizeInBytes.Should().Be(617163);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldGetPostByMd5Async()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostByMd5Async("deaac52a6b001b6953db90a09f7629f7");

            post.Should().NotBeNull();
            post!.OriginalUrl.Should().StartWith("https://v.sankakucomplex.com/data/de/aa/deaac52a6b001b6953db90a09f7629f7.jpg");
            post.Id.Id.Should().Be(6541010);
            post.Id.Md5Hash.Should().Be("deaac52a6b001b6953db90a09f7629f7");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(113);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf(new[] { "meta", "general", "copyright", "character", "circle", "artist", "medium", "genre" });
            }
            
            post.Parent.Should().NotBeNull();
            post.Parent!.Id.Should().Be(6541009);
            post.Parent!.Md5Hash.Should().Be("8f37e824ec321d96f0e149d77ee5d21d");
            post.Pools.Should().HaveCount(4);
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be("https://www.pixiv.net/member_illust.php?mode=medium&illust_id=66351185");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(756, 1052));
            post.PostedAt.Should().Be(new DateTimeOffset(2017, 12, 18, 21, 22, 21, 0, TimeSpan.Zero));
            post.SampleUrl.Should().StartWith("https://v.sankakucomplex.com/data/sample/de/aa/sample-deaac52a6b001b6953db90a09f7629f7.jpg");
            post.UploaderId.Id.Should().Be(231462);
            post.UploaderId.Name.Should().Be("Domestikun");
            post.FileSizeInBytes.Should().Be(617163);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldContainLinkWithoutAmp()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostAsync(5735331);

            post.OriginalUrl.Should().NotContain("&amp;");
        }
    }

    public class LoadFirstTagHistoryPageAsyncMethod : SankakuLoaderTests
    {
        public LoadFirstTagHistoryPageAsyncMethod(SankakuLoaderFixture loaderFixture) 
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

        [Fact]
        public async Task ShouldFindWithMultipleTags()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var result = await loader.SearchAsync("1girl long_hair");
            result.Results.Should().NotBeEmpty();
            result.Results.Should().HaveCountGreaterThan(1);
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
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var notesHistory = await loader.GetNoteHistoryToDateTimeAsync(DateTime.Now.AddHours(-1)).ToListAsync();
            notesHistory.Should().NotBeEmpty();
        }
    }

    public class LoadTagHistoryUpToAsyncMethod : SankakuLoaderTests
    {
        public LoadTagHistoryUpToAsyncMethod(SankakuLoaderFixture loaderFixture) 
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

    public class LoadTagHistoryFromAsyncMethod : SankakuLoaderTests
    {
        public LoadTagHistoryFromAsyncMethod(SankakuLoaderFixture loaderFixture) 
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
            tagsHistory.Count.Should().BeGreaterOrEqualTo(firstTagHistoryPage.Count + 100);
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
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(31143268);

            post.Tags.Count.Should().BeGreaterThan(30);
            post.ChildrenIds.Count.Should().NotBe(0);
            post.Parent!.Id.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task ShouldLoadChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(31729492);

            post.ChildrenIds.Should().HaveCount(8);
            post.ChildrenIds.Distinct().Should().HaveCount(post.ChildrenIds.Count);
            post.ChildrenIds.First().Should().Be(new PostIdentity(31729784, "c47bcc8a56dfdf0267c788860ed81c3e"));

            foreach (var postChildrenId in post.ChildrenIds)
            {
                postChildrenId.Id.Should().BeGreaterThan(1);
                postChildrenId.Md5Hash.Should().HaveLength(32);
            }
        }

        [Fact]
        public async Task ShouldLoadPostWithEmptyChildren()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();
            var post = await loader.GetPostByMd5Async("692078f4b19d8a7992bc361baac39650");

            post.Should().NotBeNull();
            post!.Tags.Count.Should().BeGreaterThan(30);

            // no longer empty, but inaccessible
            post.ChildrenIds.Count.Should().Be(2);
        }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(30879033);

            post.Notes.Count.Should().Be(4);
            
            post.Notes.ElementAt(0).Id.Should().Be(1838620);
            post.Notes.ElementAt(0).Text.Should().Be("Wishes to be a slave");
            post.Notes.ElementAt(0).Size.Should().Be(new Size(620, 280));
            post.Notes.ElementAt(0).Point.Should().Be(new Position(646, 92));
            
            post.Notes.ElementAt(1).Id.Should().Be(1838619);
            post.Notes.ElementAt(1).Text.Should().Be("H cup\nHuge breasts <3");
            post.Notes.ElementAt(1).Size.Should().Be(new Size(574, 434));
            post.Notes.ElementAt(1).Point.Should().Be(new Position(475, 2229));
            
            post.Notes.ElementAt(2).Id.Should().Be(1838618);
            post.Notes.ElementAt(2).Text.Should().Be("Slutty voice <3");
            post.Notes.ElementAt(2).Size.Should().Be(new Size(550, 224));
            post.Notes.ElementAt(2).Point.Should().Be(new Position(84, 2171));
            
            post.Notes.ElementAt(3).Id.Should().Be(1838617);
            post.Notes.ElementAt(3).Text.Should().Be("Vulgar\nOrgasm face <3");
            post.Notes.ElementAt(3).Size.Should().Be(new Size(563, 311));
            post.Notes.ElementAt(3).Point.Should().Be(new Position(60, 118));
        }
            
        [Fact]
        public async Task ShouldLoadSampleUrl()
        {
            var loader = _loaderFixture.GetLoaderWithoutAuth();

            var post = await loader.GetPostAsync(17649920);

            post.Should().NotBeNull();
            post.SampleUrl.Should().Contain("sample");
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
            await api.FavoritePostAsync(30879033);
        }
    }
}
