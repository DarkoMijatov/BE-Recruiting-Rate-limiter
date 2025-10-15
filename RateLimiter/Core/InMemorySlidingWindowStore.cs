using System.Collections.Concurrent;

namespace RateLimiter.Core
{
    /// <summary>
    /// a simple in-memory sliding window rate limit store
    /// </summary>
    public sealed class InMemorySlidingWindowStore : IRateLimitStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _buckets = new();
        public bool TryAcquire(string key, int windowMs, int maxCount, long nowTicks)
        {
            var windowTicks = TimeSpan.FromMilliseconds(windowMs).Ticks;
            var cutoff = nowTicks - windowTicks;

            var q = _buckets.GetOrAdd(key, _ => new ConcurrentQueue<long>());

            // clean old entries
            while (q.TryPeek(out var oldest) && oldest < cutoff)
            {
                q.TryDequeue(out _);
            }

            // check if we are within limit
            if (q.Count >= maxCount)
                return false;

            // insert the new entry
            q.Enqueue(nowTicks);
            return true;
        }

    }
}
