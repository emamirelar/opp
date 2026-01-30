namespace UNOPS.PAO.UNOPSDataAccess.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;

/// <summary>
/// Design-time factory for UNOPSAppDbContext to enable EF Core migrations
/// </summary>
public class UNOPSAppDbContextFactory : IDesignTimeDbContextFactory<UNOPSAppDbContext>
{
    public UNOPSAppDbContext CreateDbContext(string[] args)
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
        var optionsBuilder = new DbContextOptionsBuilder<UNOPSAppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("UNOPS.PAO.UNOPSDataAccess");
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });

        // Create minimal dependencies for design-time context creation
        // These are only used during migrations, not at runtime
        var userService = new UserResolverService<int>(null); // No HttpContext at design-time
        var schema = new DefaultDbContextSchema(); // Use default schema

        return new UNOPSAppDbContext(optionsBuilder.Options, userService, schema);
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

    /// <summary>
    /// Default schema implementation for design-time context creation
    /// </summary>
    private class DefaultDbContextSchema : IDbContextSchema
    {
        public string Schema => "public";
    }
}
