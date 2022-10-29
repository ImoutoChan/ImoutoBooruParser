namespace ImoutoRebirth.BooruParser.Implementations.Sankaku;

public interface ISankakuAuthManager
{
    ValueTask<string?> GetTokenAsync();
}