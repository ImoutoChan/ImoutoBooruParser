using System.Collections.Concurrent;

namespace Imouto.BooruParser.Implementations;

public class Throttler
{
    private static readonly ConcurrentDictionary<string, Throttler> Throttlers = new();

    public static Throttler Get(string key) => Throttlers.GetOrAdd(key, _ => new Throttler());

    private readonly SemaphoreSlim _locker = new(1);
    private DateTimeOffset _lastAccess = DateTimeOffset.MinValue;
    
    public async ValueTask UseAsync(TimeSpan delay, CancellationToken ct = default)
    {
        await _locker.WaitAsync(ct);
        try
        {
            var now = DateTimeOffset.UtcNow;
            var timePassedSinceLastCall = now - _lastAccess;

            if (timePassedSinceLastCall < delay)
                await Task.Delay(delay - timePassedSinceLastCall, ct);

            _lastAccess = DateTimeOffset.UtcNow;
        }
        finally
        {
            _locker.Release();
        }
    }
}
