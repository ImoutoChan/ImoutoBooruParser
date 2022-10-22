using Flurl;
using Flurl.Http;

namespace ImoutoRebirth.BooruParser.Implementations;

// check on loli posts
// check on banned posts
public class DanbooruApiLoader : IBooruApiLoader
{
    private const string BaseUrl = "https://danbooru.donmai.us";

    public async Task<Post> GetPostAsync(int postId)
    {
        var post = await BaseUrl
            .AppendPathSegments("posts", postId + ".json")
            .GetJsonAsync<DanbooruPost>();

        return new Post(
            new PostIdentity(postId, post.Md5),
            post.FileUrl,
            post.LargeFileUrl ?? post.PreviewFileUrl,
            post.IsBanned || post.IsDeleted ? ExistState.MarkDeleted : ExistState.Exist,
            post.CreatedAt,
            new Uploader(post.UploaderId, post.Uploader.Name),
            post.Source,
            new Size(post.ImageWidth, post.ImageHeight),
            post.FileSize,
            GetRating(post.Rating),
            GetRatingSafeLevel(post.Rating),
            GetUgoiraMetadata(post),
            GetParent(post),
            GetChildren(post),
            await GetPoolsAsync(postId),
            GetTags(post),
            GetNotes(post));
    }

    public Task<SearchResult> SearchAsync(string tags)
    {
        throw new NotImplementedException();
    }

    public Task<SearchResult> GetPopularPostsAsync(PopularType type)
    {
        throw new NotImplementedException();
    }

    public Task<HistorySearchResult<TagsHistoryEntry>> LoadTagHistoryPageAsync(SearchToken? token)
    {
        throw new NotImplementedException();
    }

    public Task<HistorySearchResult<NoteHistoryEntry>> LoadNotesHistoryPageAsync(SearchToken? token)
    {
        throw new NotImplementedException();
    }

    private static PostIdentity? GetParent(DanbooruPost post)
        => post.Parent != null ? new PostIdentity(post.Parent.Id, post.Parent.Md5) : null;

    private static IReadOnlyCollection<PostIdentity> GetChildren(DanbooruPost post)
        => post.Children.Select(x => new PostIdentity(x.Id, x.Md5)).ToList();


    private static IReadOnlyCollection<int> GetUgoiraMetadata(DanbooruPost post)
    {
        var isUgoira = post.FileExt == "zip";
        if (!isUgoira)
            return Array.Empty<int>();

        return post.MediaMetadata.Metadata.UgoiraFrameDelays ?? Array.Empty<int>();
    }


    private static async Task<IReadOnlyCollection<Pool>> GetPoolsAsync(int postId)
    {
        var pools = await BaseUrl
            .AppendPathSegment("pools.json")
            .SetQueryParam("search[post_tags_match]", $"id:{postId}")
            .SetQueryParam("only", $"id,name,post_ids")
            .GetJsonAsync<IReadOnlyCollection<DanbooruPool>>();

        return pools
            .Select(x => new Pool(x.Id, x.Name, Array.IndexOf(x.PostIds, postId)))
            .ToList();
    }

    private static IReadOnlyCollection<Note> GetNotes(DanbooruPost post)
    {
        if (post.LastNotedAt == null)
            return Array.Empty<Note>();

        return post.Notes
            .Select(x => new Note(x.Id, x.Body, new Position(x.Y, x.X), new Size(x.Width, x.Height)))
            .ToList();
    }

    private static Rating GetRating(string postRating) => GetRatingFromChar(postRating).Item1;

    private static RatingSafeLevel GetRatingSafeLevel(string postRating) => GetRatingFromChar(postRating).Item2;

    private static (Rating, RatingSafeLevel) GetRatingFromChar(string rating)
        => rating switch
        {
            "q" => (Rating.Questionable, RatingSafeLevel.None),
            "s" => (Rating.Safe, RatingSafeLevel.Sensitive),
            "g" => (Rating.Safe, RatingSafeLevel.General),
            "e" => (Rating.Explicit, RatingSafeLevel.None),
            _ => (Rating.Questionable, RatingSafeLevel.None)
        };

    private static IReadOnlyCollection<Tag> GetTags(DanbooruPost post)
        => post.TagStringArtist.Split(' ').Select(x => (Type: "artist", Tag: x))
            .Union(post.TagStringCharacter.Split(' ').Select(x => (Type: "character", Tag: x)))
            .Union(post.TagStringCopyright.Split(' ').Select(x => (Type: "copyright", Tag: x)))
            .Union(post.TagStringGeneral.Split(' ').Select(x => (Type: "general", Tag: x)))
            .Union(post.TagStringMeta.Split(' ').Select(x => (Type: "meta", Tag: x)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Tag))
            .Select(x => new Tag(x.Type, x.Tag.Replace('_', ' ')))
            .ToList();
}
    
