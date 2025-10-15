namespace RateLimiter.Core
{
    public interface IRateLimitStore
    {
        /// <summary>
        ///  returns true if the request is allowed, false if rate limit exceeded
        /// </summary>
        bool TryAcquire(string key, int windowMs, int maxCount, long nowTicks);
    }
}
