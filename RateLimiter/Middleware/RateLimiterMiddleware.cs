using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RateLimiter.Core;
using RateLimiter.Options;

namespace RateLimiter.Middleware
{
    public class RateLimiterMiddleware : IMiddleware
    {
        private readonly IRateLimitStore _store;
        private readonly IOptions<Options.RateLimiterOptions> _options;
        public RateLimiterMiddleware(IRateLimitStore store, IOptions<Options.RateLimiterOptions> options)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var opt = _options.Value;

            if (!opt.ReuqestLimiterEnabled)
            {
                await next(context);
                return;
            }

            // get the client IP (by default RemoteIpAddress)
            var clientIp = GetClientIp(context, opt) ?? "unknown";

            // get the endpoint path (for per-endpoint limit if exists)
            var path = context.Request.Path.HasValue ? context.Request.Path.Value! : "/";

            var nowTicks = DateTime.UtcNow.Ticks;

            // if there is a specific limit for this endpoint, use it; otherwise use global default
            if (TryGetEndpointLimit(opt, path, out var endpointLimit))
            {
                if (!AllowRequest(clientIp, path, endpointLimit!.RequestLimitMs, endpointLimit!.RequestLimitCount, nowTicks))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return;
                }
            }
            else
            {
                if (!AllowRequest(clientIp, "__GLOBAL__", opt.DefaultRequestLimitMs, opt.DefaultRequestLimitCount, nowTicks))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return;
                }
            }
            await next(context);
        }

        private bool AllowRequest(string clientIp, string path, int requestLimitMs, int requestLimitCount, long nowTicks)
        {
            var key = $"{clientIp}|{path}";
            return _store.TryAcquire(key, requestLimitMs, requestLimitCount, nowTicks);
        }

        private bool TryGetEndpointLimit(Options.RateLimiterOptions opt, string path, out EndpointLimit endpointLimit)
        {
            endpointLimit = null!;
            if (opt.EndpointLimits == null || opt.EndpointLimits.Count == 0)
            {
                return false;
            }

            // the simplest matching logic: exact match ignoring case
            endpointLimit = opt.EndpointLimits.FirstOrDefault(e => string.Equals(NormalizePath(e.Endpoint), NormalizePath(path), StringComparison.OrdinalIgnoreCase));

            return endpointLimit != null;

            static string NormalizePath(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return "/";
                }
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                if (path.Length > 1 && path.EndsWith("/"))
                {
                    path = path.TrimEnd('/');
                }
                return path;
            }
        }

        private string? GetClientIp(HttpContext context, Options.RateLimiterOptions opt)
        {
            if (opt.RespectXForwardedFor &&
                    context.Request.Headers.TryGetValue(opt.XForwardedForHeaderName, out var headerValue) &&
                    !string.IsNullOrWhiteSpace(headerValue.ToString()))
            {
                // take the first IP in the X-Forwarded-For header
                var first = headerValue.ToString().Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(first))
                {
                    return first;
                }
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
