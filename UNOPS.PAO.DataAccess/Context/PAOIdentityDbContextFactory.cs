namespace UNOPS.PAO.DataAccess.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.IO;

/// <summary>
/// Design-time factory for PAOIdentityDbContext to enable EF Core migrations
/// </summary>
public class PAOIdentityDbContextFactory : IDesignTimeDbContextFactory<PAOIdentityDbContext>
{
    public PAOIdentityDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string from environment variable (for CI/CD) or configuration
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=PAO;Username=postgres;Password=postgres";

        Console.WriteLine($"[Design-Time Factory] Using connection string: {MaskPassword(connectionString)}");

        // Build DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<PAOIdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("UNOPS.PAO.DataAccess");
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });

        // Create a minimal service provider for the context
        // This is only used during migrations, not at runtime
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new PAOIdentityDbContext(optionsBuilder.Options, serviceProvider);
    }

    private static string MaskPassword(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder.Password))
            {
                builder.Password = "***";
            }
            return builder.ConnectionString;
        }
        catch
        {
            return "[connection string parse error]";
        }
    }
}
