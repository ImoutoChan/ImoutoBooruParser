using System.Collections.Concurrent;

namespace Imouto.BooruParser.Implementations;

public class Throttler
{
    private static readonly ConcurrentDictionary<string, Throttler> Throttlers = new();

    public static Throttler Get(string key) => Throttlers.GetOrAdd(key, _ => new Throttler());

    private readonly SemaphoreSlim _locker = new(1);
    private DateTimeOffset _lastAccess = DateTimeOffset.MinValue;
    
    public async ValueTask UseAsync(TimeSpan delay)
    {
        await _locker.WaitAsync();

        var now = DateTimeOffset.UtcNow;
        
        var timePassedSinceLastCall = now - _lastAccess;
        
        if (timePassedSinceLastCall < delay)
            await Task.Delay(delay - timePassedSinceLastCall);

        _lastAccess = now;
    }
    
    public void Release() => _locker.Release();
}
