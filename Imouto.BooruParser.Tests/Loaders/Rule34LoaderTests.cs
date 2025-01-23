using FluentAssertions;
using Imouto.BooruParser.Tests.Loaders.Fixtures;
using Xunit;

namespace Imouto.BooruParser.Tests.Loaders;

public class Rule34LoaderTests : IClassFixture<Rule34ApiLoaderFixture>
{
    private readonly Rule34ApiLoaderFixture _loaderFixture;

    public Rule34LoaderTests(Rule34ApiLoaderFixture loaderFixture) => _loaderFixture = loaderFixture;

    public class GetPostAsyncMethod : Rule34LoaderTests
    {
        public GetPostAsyncMethod(Rule34ApiLoaderFixture loaderFixture) : base(loaderFixture) { }

        [Fact]
        public async Task ShouldReturnPost()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(8548333);

            post.Should().NotBeNull();
            post.OriginalUrl.Should().Be("https://api-cdn-mp4.rule34.xxx/images/7492/42936037bc650b4d38bc9f6df355b0f1.mp4");
            post.Id.GetIntId().Should().Be(8548333);
            post.Id.Md5Hash.Should().Be("42936037bc650b4d38bc9f6df355b0f1");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(110);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf("general", "copyright", "character", "circle", "artist", "metadata");
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be("");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1920, 1440));
            post.PostedAt.Should().Be(new DateTimeOffset(2025, 1, 4, 15, 32, 54, TimeSpan.Zero));
            post.SampleUrl.Should().Be("https://api-cdn.rule34.xxx/images/7492/42936037bc650b4d38bc9f6df355b0f1.jpg");
            post.UploaderId.Name.Should().Be("nebushad");
            
            // isn't supported in gelbooru
            post.FileSizeInBytes.Should().Be(-1);
            post.UploaderId.Id.Should().Be("-1");
            post.UgoiraFrameDelays.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldReturnPostByMd5()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostByMd5Async("42936037bc650b4d38bc9f6df355b0f1");

            post.Should().NotBeNull();
            post!.OriginalUrl.Should().Be("https://api-cdn-mp4.rule34.xxx/images/7492/42936037bc650b4d38bc9f6df355b0f1.mp4");
            post.Id.GetIntId().Should().Be(8548333);
            post.Id.Md5Hash.Should().Be("42936037bc650b4d38bc9f6df355b0f1");
            post.Notes.Should().BeEmpty();
            post.Tags.Should().HaveCount(110);

            foreach (var postTag in post.Tags)
            {
                postTag.Name.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().NotBeNullOrWhiteSpace();
                postTag.Type.Should().BeOneOf("general", "copyright", "character", "circle", "artist", "metadata");
            }
            
            post.Parent.Should().BeNull();
            post.Pools.Should().BeEmpty();
            post.Rating.Should().Be(Rating.Explicit);
            post.RatingSafeLevel.Should().Be(RatingSafeLevel.None);
            post.Source.Should().Be("");
            post.ChildrenIds.Should().BeEmpty();
            post.ExistState.Should().Be(ExistState.Exist);
            post.FileResolution.Should().Be(new Size(1920, 1440));
            post.PostedAt.Should().Be(new DateTimeOffset(2025, 1, 4, 15, 32, 54, TimeSpan.Zero));
            post.SampleUrl.Should().Be("https://api-cdn.rule34.xxx/images/7492/42936037bc650b4d38bc9f6df355b0f1.jpg");
            post.UploaderId.Name.Should().Be("nebushad");
            
            // isn't supported in gelbooru
            post.FileSizeInBytes.Should().Be(-1);
            post.UploaderId.Id.Should().Be("-1");
            post.UgoiraFrameDelays.Should().BeEmpty();
        }
    }

    public class SearchAsyncMethod : Rule34LoaderTests
    {
        public SearchAsyncMethod(Rule34ApiLoaderFixture loaderFixture) : base(loaderFixture) { }

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

    public class GetPostMetadataMethod : Rule34LoaderTests
    {
        public GetPostMetadataMethod(Rule34ApiLoaderFixture fixture) : base(fixture) { }

        [Fact]
        public async Task ShouldLoadNotes()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(6204314);

            post.Notes.Should().HaveCount(2);
            
            post.Notes.First().Id.Should().Be("93525");
            post.Notes.First().Text.Should().Be("Slap");
            post.Notes.First().Point.Should().Be(new Position(8, 77));
            post.Notes.First().Size.Should().Be(new Size(257, 378));
            
            post.Notes.ElementAt(1).Id.Should().Be("93526");
            post.Notes.ElementAt(1).Text.Should().Be("Slap");
            post.Notes.ElementAt(1).Point.Should().Be(new Position(33, 1131));
            post.Notes.ElementAt(1).Size.Should().Be(new Size(178, 524));
        }
            
        [Fact]
        public async Task ShouldLoadSampleUrl()
        {
            var loader = _loaderFixture.GetLoader();

            var post = await loader.GetPostAsync(8548333);

            post.Should().NotBeNull();
            post.SampleUrl.Should().NotBeNull();
        }
    }
}
