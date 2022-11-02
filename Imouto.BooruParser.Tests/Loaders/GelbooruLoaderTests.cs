using FluentAssertions;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders;

public class GelbooruLoaderTests : IClassFixture<GelbooruApiLoaderFixture>
{
    private readonly GelbooruApiLoaderFixture _loaderFixture;

    public GelbooruLoaderTests(GelbooruApiLoaderFixture loaderFixture) => _loaderFixture = loaderFixture;

    public class GetPostAsyncMethod : GelbooruLoaderTests
    {
        public GetPostAsyncMethod(GelbooruApiLoaderFixture loaderFixture) : base(loaderFixture) { }

        [Fact]
        public async Task ShouldReturnPost()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(7837194);

            post.Should().NotBeNull();
            post.OriginalUrl.Should().Be("https://img3.gelbooru.com/images/89/e4/89e42789d4ef991e25dab050627c9ef2.jpeg");
            post.Id.Id.Should().Be(7837194);
            post.Id.Md5Hash.Should().Be("89e42789d4ef991e25dab050627c9ef2");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(18);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf("general", "copyright", "character", "circle", "artist", "metadata");
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Safe);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.Sensitive);
            post.Source.Should().Be("https://twitter.com/sian_sasaland/status/1583461715777585154");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1003, 1416));
            post.PostedAt.Should().Be(new DateTimeOffset(2022, 10, 22, 7, 3, 36, TimeSpan.Zero));
            post.SampleUrl.Should().Be("https://img3.gelbooru.com/images/89/e4/89e42789d4ef991e25dab050627c9ef2.jpeg");
            post.UploaderId.Id.Should().Be(44282);
            post.UploaderId.Name.Should().Be("jojosstand");
            
            // isn't supported in gelbooru
            post.FileSizeInBytes.Should().Be(-1);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnPostByMd5()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostByMd5Async("89e42789d4ef991e25dab050627c9ef2");

            post.Should().NotBeNull();
            post!.OriginalUrl.Should().Be("https://img3.gelbooru.com/images/89/e4/89e42789d4ef991e25dab050627c9ef2.jpeg");
            post.Id.Id.Should().Be(7837194);
            post.Id.Md5Hash.Should().Be("89e42789d4ef991e25dab050627c9ef2");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(18);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf("general", "copyright", "character", "circle", "artist", "metadata");
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Safe);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.Sensitive);
            post.Source.Should().Be("https://twitter.com/sian_sasaland/status/1583461715777585154");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1003, 1416));
            post.PostedAt.Should().Be(new DateTimeOffset(2022, 10, 22, 7, 3, 36, TimeSpan.Zero));
            post.SampleUrl.Should().Be("https://img3.gelbooru.com/images/89/e4/89e42789d4ef991e25dab050627c9ef2.jpeg");
            post.UploaderId.Id.Should().Be(44282);
            post.UploaderId.Name.Should().Be("jojosstand");
            
            // isn't supported in gelbooru
            post.FileSizeInBytes.Should().Be(-1);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldReturnPostByMd5For0825()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostByMd5Async("6adc36db78ca238576fd45995f3e0825");

            post.Should().BeNull();
        }

        [Fact]
        public async Task ShouldReturnPostByMd5For26f7()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostByMd5Async("9fdf36606d2d321b28f78252817226f7");

            post.Should().NotBeNull();
        }
    }

    public class SearchAsyncMethod : GelbooruLoaderTests
    {
        public SearchAsyncMethod(GelbooruApiLoaderFixture loaderFixture) : base(loaderFixture) { }

        [Fact]
        public async Task SearchAsyncShouldFind()
        {
            var loader = _loaderFixture.GetLoader();

            var result = await loader.SearchAsync("no_bra");
            result.Results.Should().HaveCount(20);
            result.Results.ToList().ForEach(x => x.IsDeleted.Should().BeFalse());
            result.Results.ToList().ForEach(x => x.IsBanned.Should().BeFalse());

            foreach (var preview in result.Results)
            {
                var post = await loader.GetPostAsync(preview.Id);
                post.Tags.Select(x => x.Name).Should().Contain("no bra");
            }
        }
    }

    public class GetPostMetadataMethod : GelbooruLoaderTests
    {
        public GetPostMetadataMethod(GelbooruApiLoaderFixture fixture) : base(fixture) { }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(7858922);

            post.Notes.Should().HaveCount(4);
            
            post.Notes.First().Id.Should().Be(588208);
            post.Notes.First().Text.Should().Be("Copied 3 notes from post #5780559.");
            post.Notes.First().Point.Should().Be(new Position(0, 0));
            post.Notes.First().Size.Should().Be(new Size(0, 0));
            
            post.Notes.ElementAt(1).Id.Should().Be(588209);
            post.Notes.ElementAt(1).Text.Should().Be("Vaginal opening");
            post.Notes.ElementAt(1).Point.Should().Be(new Position(897, 1613));
            post.Notes.ElementAt(1).Size.Should().Be(new Size(102, 219));
            
            post.Notes.ElementAt(2).Id.Should().Be(588210);
            post.Notes.ElementAt(2).Text.Should().Be("Urethal opening");
            post.Notes.ElementAt(2).Point.Should().Be(new Position(240, 2086));
            post.Notes.ElementAt(2).Size.Should().Be(new Size(108, 308));
            
            post.Notes.ElementAt(3).Id.Should().Be(588211);
            post.Notes.ElementAt(3).Text.Should().Be("Clitoris");
            post.Notes.ElementAt(3).Point.Should().Be(new Position(256, 1540));
            post.Notes.ElementAt(3).Size.Should().Be(new Size(105, 257));
        }
            
        [Fact]
        public async Task ShouldLoadSampleUrl()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(7859608);

            post.Should().NotBeNull();
            post.SampleUrl.Should().Contain("sample");
        }
        
        [Fact]
        public async Task ShouldGetBannedPostWithSomeData()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(7841638);

            post.Should().NotBeNull();
            post.OriginalUrl.Should().Be("https://img3.gelbooru.com//images/b9/b9/b9b933c1835d043ec38cbefbe78554eb.png");
            post.Id.Id.Should().Be(7841638);
            post.Id.Md5Hash.Should().Be("b9b933c1835d043ec38cbefbe78554eb");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(34);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf("general", "copyright", "character", "circle", "artist", "metadata", "deprecated");
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().BeNull();
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.MarkDeleted);
            post.FileResolution.Should().Be(new Size(2244, 3541));
            post.PostedAt.Should().Be(new DateTimeOffset(2022, 10, 23, 7, 20, 31, TimeSpan.FromHours(-5)));
            post.SampleUrl.Should().Be("https://img3.gelbooru.com//images/b9/b9/b9b933c1835d043ec38cbefbe78554eb.png");
            post.UploaderId.Id.Should().Be(-1);
            post.UploaderId.Name.Should().Be("oniii-chan");
            
            // isn't supported in gelbooru
            post.FileSizeInBytes.Should().Be(-1);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ShouldGetBannedPostWithSomeDataByMd5()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostByMd5Async("b9b933c1835d043ec38cbefbe78554eb");

            post.Should().NotBeNull();
            post!.OriginalUrl.Should().Be("https://img3.gelbooru.com//images/b9/b9/b9b933c1835d043ec38cbefbe78554eb.png");
            post.Id.Id.Should().Be(7841638);
            post.Id.Md5Hash.Should().Be("b9b933c1835d043ec38cbefbe78554eb");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(34);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf("general", "copyright", "character", "circle", "artist", "metadata", "deprecated");
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().BeNull();
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.MarkDeleted);
            post.FileResolution.Should().Be(new Size(2244, 3541));
            post.PostedAt.Should().Be(new DateTimeOffset(2022, 10, 23, 7, 20, 31, TimeSpan.FromHours(-5)));
            post.SampleUrl.Should().Be("https://img3.gelbooru.com//images/b9/b9/b9b933c1835d043ec38cbefbe78554eb.png");
            post.UploaderId.Id.Should().Be(-1);
            post.UploaderId.Name.Should().Be("oniii-chan");
            
            // isn't supported in gelbooru
            post.FileSizeInBytes.Should().Be(-1);
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
    }
}
