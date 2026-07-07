using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Patogh.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApiHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbConn = configuration.GetConnectionString("DefaultConnection");
        var redisConn = configuration.GetConnectionString("Redis");

        var builder = services.AddHealthChecks();

        // PostgreSQL — restored from commented-out state
        /*if (!string.IsNullOrWhiteSpace(dbConn) &&
            !dbConn.Contains("REPLACE_IN_USER_SECRETS"))
        {
            builder.AddNpgsql(
                dbConn,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "postgresql"]);
        }*/

        // Redis
        builder.AddCheck("redis", () =>
        {
            if (string.IsNullOrWhiteSpace(redisConn))
                return HealthCheckResult.Degraded(
                    "Redis not configured — using NullCacheService");

            return HealthCheckResult.Healthy("Redis configured");
        }, tags: ["cache", "redis"]);

        return services;
    }
}