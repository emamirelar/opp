using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;

namespace UNOPS.PAO.UNOPSDataAccess.Context;

/// <summary>
/// Design-time factory for creating UNOPSAppDbContext instances during EF Core migrations.
/// This factory is used by Entity Framework tools (like dotnet ef migrations) to instantiate
/// the DbContext when the application is not running.
/// </summary>
public class UNOPSAppDbContextFactory : IDesignTimeDbContextFactory<UNOPSAppDbContext>
{
    /// <summary>
    /// Creates a new instance of UNOPSAppDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the EF tools</param>
    /// <returns>A configured UNOPSAppDbContext instance</returns>
    public UNOPSAppDbContext CreateDbContext(string[] args)
    {
        // Find the Server project directory
        var currentDir = Directory.GetCurrentDirectory();
        var serverPath = FindServerProjectPath(currentDir);
        
        if (string.IsNullOrEmpty(serverPath))
        {
            throw new InvalidOperationException(
                $"Could not find UNOPS.PAO.Server directory. Current directory: {currentDir}. " +
                "Please ensure you're running the command from the solution root directory.");
        }

        // Determine environment (with fallback priority)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                          ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? "Local"; // Default to Local for development machines
        
        // Build configuration from appsettings.json in the Server project
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(serverPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        // Try to load environment-specific file
        var envConfigFile = $"appsettings.{environment}.json";
        var envConfigPath = Path.Combine(serverPath, envConfigFile);
        if (File.Exists(envConfigPath))
        {
            configBuilder.AddJsonFile(envConfigFile, optional: true, reloadOnChange: true);
            Console.WriteLine($"[Design-Time Factory] Loaded environment config: {envConfigFile}");
        }
        else
        {
            // Fallback: try appsettings.Local.json if environment file doesn't exist
            if (File.Exists(Path.Combine(serverPath, "appsettings.Local.json")))
            {
                configBuilder.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
                Console.WriteLine("[Design-Time Factory] Loaded fallback config: appsettings.Local.json");
            }
            else
            {
                Console.WriteLine($"[Design-Time Factory] No environment-specific config found. Using base appsettings.json");
            }
        }
        
        var configuration = configBuilder.Build();

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("DbContext");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string 'DbContext' not found in configuration. " +
                $"Environment: {environment}. " +
                $"Checked files: appsettings.json, appsettings.{environment}.json, appsettings.Local.json. " +
                $"Server path: {serverPath}");
        }
        
        Console.WriteLine($"[Design-Time Factory] Using connection string from configuration (Environment: {environment})");

        // Get schema from configuration (defaults to "public")
        var schema = configuration.GetConnectionString("DbSchema") ?? "public";

        // Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<UNOPSAppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // Create a mock IHttpContextAccessor that returns null for HttpContext
        // This is needed because the DbContext constructor calls GetCurrentUserId()
        // which safely handles null HttpContext by returning default(int)
        var mockHttpContextAccessor = new DesignTimeHttpContextAccessor();
        
        // Create user resolver service with mock accessor
        var dummyUserResolver = new UserResolverService<int>(mockHttpContextAccessor, null);
        
        // Create schema service with configured schema
        var dbContextSchema = new DbContextSchema(schema);

        // Return configured DbContext
        return new UNOPSAppDbContext(optionsBuilder.Options, dummyUserResolver, dbContextSchema);
    }

    /// <summary>
    /// Finds the UNOPS.PAO.Server project directory by searching upward from the current directory.
    /// </summary>
    /// <param name="startPath">The starting directory path</param>
    /// <returns>The full path to the Server project directory, or null if not found</returns>
    private string? FindServerProjectPath(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        
        // Search up to 5 levels up from current directory
        for (int i = 0; i < 5 && directory != null; i++)
        {
            // Check if UNOPS.PAO.Server exists in current directory
            var serverPath = Path.Combine(directory.FullName, "UNOPS.PAO.Server");
            if (Directory.Exists(serverPath))
            {
                return serverPath;
            }
            
            // Move up one directory
            directory = directory.Parent;
        }
        
        return null;
    }
}

/// <summary>
/// Mock HttpContextAccessor for design-time operations.
/// Returns null for HttpContext, which is safely handled by UserResolverService.
/// </summary>
internal class DesignTimeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = null;
}

