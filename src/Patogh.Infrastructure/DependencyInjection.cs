using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Patogh.Application.Interfaces;
using Patogh.Infrastructure.Auth;
using Patogh.Infrastructure.Authentication;
using Patogh.Infrastructure.BackgroundJobs;
using Patogh.Infrastructure.Configurations;
using Patogh.Infrastructure.Security;
using Patogh.Infrastructure.Services.Cache;
using Patogh.Infrastructure.Services.Otp;
using Patogh.Infrastructure.Services.Sms;
using Patogh.Infrastructure.Storage;
using StackExchange.Redis;

namespace Patogh.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Strongly-typed options ────────────────────────────────────────────
        services.Configure<OtpSettings>(
            configuration.GetSection("OtpSettings"));
        services.Configure<ReservationSettings>(
            configuration.GetSection("ReservationSettings"));

        // ── JWT & Auth ────────────────────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // ── File Storage ──────────────────────────────────────────────────────
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // ── Redis (with graceful fallback) ────────────────────────────────────
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                try
                {
                    var configOpts = ConfigurationOptions.Parse(redisConnection);
                    configOpts.ConnectTimeout = 5000;
                    configOpts.AbortOnConnectFail = false;

                    var multiplexer = ConnectionMultiplexer.Connect(configOpts);

                    multiplexer.ConnectionFailed += (_, e) =>
                        logger.LogWarning(
                            "Redis connection failed: {FailureType} — cache operations will be no-ops.",
                            e.FailureType);

                    multiplexer.ConnectionRestored += (_, _) =>
                        logger.LogInformation("Redis connection restored.");

                    logger.LogInformation("Redis connected successfully to {Endpoint}.",
                        redisConnection);
                    return multiplexer;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Could not connect to Redis at startup. " +
                        "Cache and OTP will use fallback implementations. " +
                        "Resolve Redis connectivity before going to production.");
                    var opts = ConfigurationOptions.Parse(redisConnection);
                    opts.AbortOnConnectFail = false;
                    return ConnectionMultiplexer.Connect(opts);
                }
            });

            services.AddScoped<ICacheService, ResilientRedisCacheService>();
            services.AddScoped<IOtpService, RedisOtpService>();
        }
        else
        {
            services.AddScoped<ICacheService, NullCacheService>();
            services.AddScoped<IOtpService, MockOtpService>();
        }

        // ── SMS Sender ────────────────────────────────────────────────────────
        // ConsoleSmsSender is used by default (dev + staging).
        // To add a real SMS provider, inject it here conditionally:
        //   var twilioSid = configuration["Twilio:AccountSid"];
        //   if (!string.IsNullOrWhiteSpace(twilioSid)) { register TwilioSmsSender }
        //   else { register ConsoleSmsSender }
        services.AddScoped<ISmsSender, ConsoleSmsSender>();

        // ── Hangfire ──────────────────────────────────────────────────────────
        var hangfireConn = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(hangfireConn)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2;
            options.Queues = ["default", "critical"];
        });

        services.AddTransient<ReservationReminderJob>();
        services.AddTransient<AutoCancelJob>();

        return services;
    }
}
