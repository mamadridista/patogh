using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Patogh.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Auth endpoints: max 5 requests per minute per IP.
            // Prevents OTP brute-force and credential stuffing attacks.
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0; // No queue — reject immediately
            });

            // General API endpoints: 100 requests per minute per IP.
            // Prevents scraping and abuse of search/listing endpoints.
            options.AddFixedWindowLimiter("general", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // When rate limit is exceeded, return 429 Too Many Requests
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Message = "تعداد درخواست‌های شما از حد مجاز بیشتر شده است. لطفاً کمی صبر کنید.",
                    RetryAfter = "60 seconds"
                }, cancellationToken);
            };
        });

        return services;
    }
}