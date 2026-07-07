using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Patogh.API.Extensions;
using Patogh.API.Middlewares;
using Patogh.Infrastructure.BackgroundJobs;
using Patogh.Persistence.Context;
using Patogh.Persistence.Seed;
using Serilog;
using System.Text.Json;

// ── Serilog bootstrap logger ──────────────────────────────────────────────────
// Active from application start — catches startup exceptions before DI is built.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Patogh API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // ── Core ──────────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // ── Feature Services ──────────────────────────────────────────────────────
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddProjectServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddApiRateLimiting();
    builder.Services.AddApiHealthChecks(builder.Configuration);

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.AddDirectoryBrowser();

    var app = builder.Build();

    // ── Validate critical configuration in ALL environments ───────────────────
    // In Production: throws and stops startup on misconfiguration.
    // In Development: logs warnings but continues.
    app.ValidateConfiguration();

    // ── Database migration ────────────────────────────────────────────────────
    // Run migrations automatically on startup.
    // In Production, consider running migrations as a separate pre-deploy step
    // (e.g., a Kubernetes init container) for zero-downtime deployments.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        if (app.Environment.IsDevelopment())
            await DataSeeder.SeedAsync(db);
    }

    // ── Middleware Pipeline ───────────────────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserId",
                httpContext.User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous");
        };
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patogh API v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseStaticFiles();
    app.UseCors("FrontendPolicy");
    app.UseRateLimiter();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── Hangfire ──────────────────────────────────────────────────────────────
    // TODO: Secure the Hangfire dashboard with proper authorization in production.
    // See: https://docs.hangfire.io/en/latest/configuration/using-dashboard.html
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // Lock down in production by replacing [] with an auth filter, e.g.:
        // Authorization = [new HangfireRoleAuthorizationFilter("Admin")]
        Authorization = app.Environment.IsDevelopment()
            ? []
            : [new LocalRequestsOnlyAuthorizationFilter()]
    });

    RecurringJob.AddOrUpdate<ReservationReminderJob>(
        "reservation-reminder",
        job => job.ExecuteAsync(),
        "*/30 * * * *");

    RecurringJob.AddOrUpdate<AutoCancelJob>(
        "auto-cancel-pending",
        job => job.ExecuteAsync(),
        "0 * * * *");

    // ── Health Checks ─────────────────────────────────────────────────────────
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                duration = report.TotalDuration,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration
                })
            }));
        }
    });

    app.MapHealthChecks("/health/live",
        new HealthCheckOptions { Predicate = _ => false });

    app.MapHealthChecks("/health/ready",
        new HealthCheckOptions
        {
            Predicate = c => c.Tags.Contains("db") || c.Tags.Contains("cache")
        });

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Patogh API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
