using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace RateLimiter.Extensions
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection AddRateLimiter(this IServiceCollection services, IConfiguration config, string sectionName = "RateLimiter")
        {
            services.Configure<Options.RateLimiterOptions>(config.GetSection(sectionName));
            services.AddSingleton<Core.IRateLimitStore, Core.InMemorySlidingWindowStore>();
            services.AddTransient<Middleware.RateLimiterMiddleware>();
            return services;
        }

        public static IApplicationBuilder UseRateLimiterMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Middleware.RateLimiterMiddleware>();
        }
    }
}
