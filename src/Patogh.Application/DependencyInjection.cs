using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Patogh.Application.Behaviors;
using Patogh.Application.Configurations;
using System.Reflection;

namespace Patogh.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register JwtSettings in Application to avoid circular dependency.
        // Infrastructure reads the same options via the same key.
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}