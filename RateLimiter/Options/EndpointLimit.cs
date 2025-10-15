namespace RateLimiter.Options
{
    public class EndpointLimit
    {
        public string Endpoint { get; set; } = "/";
        public int RequestLimitMs { get; set; }
        public int RequestLimitCount { get; set; }
    }
}
