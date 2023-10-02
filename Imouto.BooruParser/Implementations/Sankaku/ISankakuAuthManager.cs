using Flurl.Http;

namespace Imouto.BooruParser.Implementations.Sankaku;

public interface ISankakuAuthManager
{
    ValueTask<string?> GetTokenAsync();

    Task<IReadOnlyCollection<FlurlCookie>> GetSankakuChannelSessionAsync();
}
