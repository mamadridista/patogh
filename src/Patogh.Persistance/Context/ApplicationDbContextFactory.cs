using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Patogh.Persistence.Context;

namespace Patogh.Persistence.Context;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // ⚠️ Change the connection string or read from appsettings if needed
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Patogh_DB;Username=postgres;Password=20042040");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}