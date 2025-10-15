namespace RateLimiter.Options
{
    public class RateLimiterOptions
    {
        public bool ReuqestLimiterEnabled { get; set; } = true;

        // global defaults
        public int DefaultRequestLimitMs { get; set; } = 1000; // 1 second
        public int DefaultRequestLimitCount { get; set; } = 10; // 10 requests

        // optional per-endpoint limits
        public List<EndpointLimit>? EndpointLimits { get; set; }

        // optional to allow X-Forwarded-For header
        public bool RespectXForwardedFor { get; set; } = false;
        public string XForwardedForHeaderName { get; set; } = "X-Forwarded-For";
    }
}
