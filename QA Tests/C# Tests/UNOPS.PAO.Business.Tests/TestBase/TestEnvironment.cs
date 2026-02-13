using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Centralized test environment configuration.
/// 
/// DEFAULT: Uses real PostgreSQL database (reads connection string from appsettings.Testing.json).
/// This is the standard workflow — Cloud SQL Proxy must be running.
/// IAM authentication is configured automatically when UseIamAuthentication=true in appsettings.
/// 
/// To fall back to InMemory (e.g., CI without a database, or proxy not running):
///   $env:USE_INMEMORY_DB = "true"
///   dotnet test ...
/// 
/// To override the connection string (e.g., different database):
///   $env:TEST_DB_CONNECTION_STRING = "Host=...;Port=5432;Database=...;Username=..."
///   dotnet test ...
/// </summary>
public static class TestEnvironment
{
    /// <summary>
    /// Environment variable: set to "true" to fall back to InMemory database
    /// </summary>
    public const string UseInMemoryEnvVar = "USE_INMEMORY_DB";

    /// <summary>
    /// Environment variable: explicit connection string (overrides appsettings)
    /// </summary>
    public const string ConnectionStringEnvVar = "TEST_DB_CONNECTION_STRING";

    /// <summary>
    /// Cached connection string (resolved once per test run)
    /// </summary>
    private static readonly string? _connectionString;

    /// <summary>
    /// Whether IAM authentication is enabled (read from appsettings.Testing.json)
    /// </summary>
    private static readonly bool _useIamAuth;

    /// <summary>
    /// Shared NpgsqlDataSource for IAM-authenticated connections.
    /// Must be shared because it manages the periodic password refresh.
    /// </summary>
    private static readonly NpgsqlDataSource? _dataSource;

    /// <summary>
    /// Whether we are using a real PostgreSQL database (DEFAULT)
    /// </summary>
    public static bool UsePostgreSQL { get; }

    /// <summary>
    /// Whether we are using InMemory database (opt-in via USE_INMEMORY_DB=true)
    /// </summary>
    public static bool UseInMemory => !UsePostgreSQL;

    /// <summary>
    /// The connection string for PostgreSQL (null if using InMemory)
    /// </summary>
    public static string? ConnectionString => _connectionString;

    /// <summary>
    /// Whether IAM authentication is enabled for the test database connection
    /// </summary>
    public static bool UseIamAuthentication => _useIamAuth;

    /// <summary>
    /// The shared NpgsqlDataSource configured with IAM auth (null if InMemory or no IAM)
    /// </summary>
    public static NpgsqlDataSource? DataSource => _dataSource;

    /// <summary>
    /// Skip reason for tests that require a relational database but InMemory is active
    /// </summary>
    public const string RequiresRelationalDb = "Requires relational database (PostgreSQL). Running in InMemory mode (USE_INMEMORY_DB=true).";

    static TestEnvironment()
    {
        // Check if InMemory mode is explicitly requested
        var useInMemory = Environment.GetEnvironmentVariable(UseInMemoryEnvVar);
        if (string.Equals(useInMemory, "true", StringComparison.OrdinalIgnoreCase))
        {
            _connectionString = null;
            _useIamAuth = false;
            _dataSource = null;
            UsePostgreSQL = false;
            return;
        }

        // Priority 1: Explicit connection string env var
        var explicitConnStr = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
        if (!string.IsNullOrWhiteSpace(explicitConnStr))
        {
            _connectionString = explicitConnStr;
            _useIamAuth = false; // Explicit connection strings should include their own auth
            UsePostgreSQL = true;
            _dataSource = BuildDataSource(_connectionString, _useIamAuth);
            return;
        }

        // Priority 2 (DEFAULT): Read connection string from appsettings.Testing.json
        var (connStr, iamAuth) = LoadConnectionSettingsFromAppSettings();
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            _connectionString = connStr;
            _useIamAuth = iamAuth;
            UsePostgreSQL = true;
            _dataSource = BuildDataSource(_connectionString, _useIamAuth);
            return;
        }

        // Fallback: If no config file found, use InMemory
        _connectionString = null;
        _useIamAuth = false;
        _dataSource = null;
        UsePostgreSQL = false;
    }

    /// <summary>
    /// Loads the connection string and IAM auth setting from appsettings.Testing.json
    /// </summary>
    private static (string? connectionString, bool useIamAuth) LoadConnectionSettingsFromAppSettings()
    {
        try
        {
            // Look for appsettings.Testing.json relative to the test assembly location
            var assemblyDir = Path.GetDirectoryName(typeof(TestEnvironment).Assembly.Location);
            
            // Try multiple possible locations
            var searchPaths = new[]
            {
                // Alongside the test DLL (copied to output via CopyToOutputDirectory)
                Path.Combine(assemblyDir ?? ".", "appsettings.Testing.json"),
                // In the test project source directory (3 levels up from bin/Debug/net9.0)
                Path.Combine(assemblyDir ?? ".", "..", "..", "..", "appsettings.Testing.json"),
            };

            foreach (var path in searchPaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonFile(fullPath, optional: false)
                        .Build();

                    // Try DbContext key first (matches app convention), then DefaultConnection
                    var connStr = config.GetConnectionString("DbContext")
                        ?? config.GetConnectionString("DefaultConnection");

                    // Read IAM authentication setting
                    var iamAuth = config.GetSection("ConnectionStrings")
                        .GetValue<bool>("UseIamAuthentication", false);

                    return (connStr, iamAuth);
                }
            }
        }
        catch
        {
            // If anything goes wrong reading config, fall back to InMemory
        }

        return (null, false);
    }

    /// <summary>
    /// Builds an NpgsqlDataSource with optional IAM authentication.
    /// Mirrors the configuration from Startup.ConfigureDataAccess().
    /// </summary>
    private static NpgsqlDataSource BuildDataSource(string connectionString, bool useIamAuth)
    {
        // Configure connection pool settings (matching main app pattern)
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MinPoolSize = 2,
            MaxPoolSize = 20,
            ConnectionLifetime = 300,
            ConnectionIdleLifetime = 60,
            Timeout = 30,
        };

        // When using IAM auth, password must be null (provided dynamically by the callback)
        if (useIamAuth)
        {
            connectionStringBuilder.Password = null;
        }

        var optimizedConnectionString = connectionStringBuilder.ToString();

        // Build the data source with optional IAM auth
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(optimizedConnectionString);
        if (useIamAuth)
        {
            // Enable IAM authentication on the CloudSqlIamAuthProvider
            CloudSqlIamAuthProvider.IsEnabled = true;

            // Use periodic password provider — generates OAuth2 tokens via Application Default Credentials
            dataSourceBuilder.UsePeriodicPasswordProvider(
                async (connStringBuilder, ct) =>
                {
                    var password = await CloudSqlIamAuthProvider.ProvidePasswordAsync(
                        connStringBuilder.Host ?? "",
                        connStringBuilder.Port,
                        connStringBuilder.Database ?? "",
                        connStringBuilder.Username ?? "",
                        ct);
                    return password ?? "";
                },
                TimeSpan.FromMinutes(55),   // Token refresh interval (tokens expire after 60 min)
                TimeSpan.FromSeconds(5)      // Refresh failure retry interval
            );
        }

        return dataSourceBuilder.Build();
    }

    /// <summary>
    /// Creates DbContextOptions for AppDbContext based on the test environment.
    /// Default: PostgreSQL (with optional IAM auth). Fallback: InMemory (when USE_INMEMORY_DB=true).
    /// </summary>
    public static DbContextOptions<AppDbContext> CreateAppDbContextOptions(string? databaseName = null)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        if (UsePostgreSQL)
        {
            if (_dataSource != null)
            {
                // Use the pre-built data source (handles IAM auth and connection pooling)
                builder.UseNpgsql(_dataSource, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                });
            }
            else
            {
                // Fallback to raw connection string (no IAM)
                builder.UseNpgsql(_connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                });
            }
            builder.EnableSensitiveDataLogging();
        }
        else
        {
            builder.UseInMemoryDatabase(databaseName: databaseName ?? $"TestDb_{Guid.NewGuid()}");
            builder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
        }

        return builder.Options;
    }

    /// <summary>
    /// Creates DbContextOptions for UNOPSAppDbContext based on the test environment.
    /// Default: PostgreSQL (with optional IAM auth). Fallback: InMemory (when USE_INMEMORY_DB=true).
    /// </summary>
    public static DbContextOptions<UNOPSAppDbContext> CreateUNOPSDbContextOptions(string? databaseName = null)
    {
        var builder = new DbContextOptionsBuilder<UNOPSAppDbContext>();

        if (UsePostgreSQL)
        {
            if (_dataSource != null)
            {
                builder.UseNpgsql(_dataSource, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                });
            }
            else
            {
                builder.UseNpgsql(_connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                });
            }
            builder.EnableSensitiveDataLogging();
        }
        else
        {
            builder.UseInMemoryDatabase(databaseName: databaseName ?? $"TestDb_{Guid.NewGuid()}");
            builder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
        }

        return builder.Options;
    }

    /// <summary>
    /// Helper for IConfiguration in tests — includes connection string when available
    /// </summary>
    public static IConfiguration CreateTestConfiguration()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["IsUNOPSOverride"] = "true",
            ["ExchangeRate:ApiKey"] = "test-key",
            ["ExchangeRate:BaseUrl"] = "https://test-api.example.com",
            ["ConnectionStrings:DbSchema"] = "public",
            ["AISettings:DisableExternalCalls"] = "true",
            ["AISettings:ModelName"] = "gemini-pro",
            ["AISettings:ProjectId"] = "test-project",
            ["AISettings:Location"] = "us-central1",
            ["GoogleCloud:ProjectId"] = "test-project",
            ["GoogleCloud:PubSubTopic"] = "test-topic",
            ["GoogleCloud:BucketName"] = "test-bucket",
            ["GoogleCloud:UseMockServices"] = "true"
        };

        if (UsePostgreSQL)
        {
            configValues["ConnectionStrings:DefaultConnection"] = _connectionString;
            configValues["ConnectionStrings:DbContext"] = _connectionString;
        }
        else
        {
            configValues["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test_db;";
            configValues["ConnectionStrings:DbContext"] = "Host=localhost;Database=test_db;";
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    /// <summary>
    /// Ensures the test database schema exists.
    /// For InMemory: creates the schema (each test gets a fresh DB by default).
    /// For PostgreSQL: no-op — the real database schema is managed by EF migrations.
    /// NEVER call EnsureCreated/EnsureDeleted on a real PostgreSQL database.
    /// </summary>
    public static void EnsureCleanDatabase(DbContext context)
    {
        if (UseInMemory)
        {
            // InMemory databases need EnsureCreated to build the schema
            context.Database.EnsureCreated();
        }
        // PostgreSQL: schema already exists from migrations — do NOT call EnsureCreated/EnsureDeleted
    }
}
