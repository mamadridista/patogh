using Patogh.Application;
using Patogh.Infrastructure;
using Patogh.Persistence;

namespace Patogh.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);
        services.AddPersistence(configuration);

        return services;
    }
}