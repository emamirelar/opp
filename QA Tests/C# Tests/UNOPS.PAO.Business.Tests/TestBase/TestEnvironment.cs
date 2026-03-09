using System.Net.Sockets;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
/// FALLBACK: When USE_INMEMORY_DB=true, uses SQLite in-memory instead of InMemory provider.
/// SQLite supports relational model features (Z.EntityFramework.Extensions, raw SQL, etc.)
/// that the EF Core InMemory provider does not, enabling ~118 previously-skipped tests.
/// 
/// To fall back to SQLite in-memory (e.g., CI without a database, or proxy not running):
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
    /// Stores the proxy connectivity error message when PostgreSQL is configured
    /// but the Cloud SQL Proxy is unreachable.
    /// </summary>
    private static readonly string? _proxyError;

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
    /// Whether we are using SQLite in-memory (same as UseInMemory — SQLite replaced InMemory provider)
    /// </summary>
    public static bool UseSQLite => !UsePostgreSQL;

    /// <summary>
    /// Skip reason for tests that require PostgreSQL-specific features (similarity(), pg_trgm, etc.)
    /// </summary>
    public const string RequiresPostgreSQL = "Requires PostgreSQL-specific features (similarity, pg_trgm). Running in SQLite mode (USE_INMEMORY_DB=true).";

    /// <summary>
    /// Skip reason for tests that require a relational database but InMemory is active.
    /// NOTE: Most tests previously skipped with this reason now run under SQLite.
    /// </summary>
    public const string RequiresRelationalDb = "Requires relational database (PostgreSQL). Running in SQLite mode (USE_INMEMORY_DB=true).";

    /// <summary>
    /// Tracks open SQLite connections to prevent GC from closing them
    /// (in-memory databases are destroyed when their connection closes).
    /// Connections are cleaned up at process exit.
    /// </summary>
    private static readonly List<SqliteConnection> _sqliteConnections = new();

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
            _proxyError = VerifyProxyConnectivity(_connectionString);
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
            _proxyError = VerifyProxyConnectivity(_connectionString);
            return;
        }

        // Fallback: If no config file found, use InMemory
        _connectionString = null;
        _useIamAuth = false;
        _dataSource = null;
        UsePostgreSQL = false;
    }

    /// <summary>
    /// Attempts a TCP connection to the database host:port to verify the Cloud SQL Proxy
    /// (or database server) is reachable. Returns null on success, or an error message on failure.
    /// Called once during static initialization so every test gets a fast, clear failure
    /// instead of waiting for Npgsql timeouts.
    /// </summary>
    private static string? VerifyProxyConnectivity(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var host = builder.Host ?? "127.0.0.1";
            var port = builder.Port > 0 ? builder.Port : 5432;

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            if (connectTask.Wait(TimeSpan.FromSeconds(5)))
            {
                return null; // connected successfully
            }

            return BuildProxyErrorMessage(host, port, "Connection timed out after 5 seconds");
        }
        catch (AggregateException ex) when (ex.InnerException is SocketException sockEx)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return BuildProxyErrorMessage(
                builder.Host ?? "127.0.0.1",
                builder.Port > 0 ? builder.Port : 5432,
                sockEx.Message);
        }
        catch (SocketException ex)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return BuildProxyErrorMessage(
                builder.Host ?? "127.0.0.1",
                builder.Port > 0 ? builder.Port : 5432,
                ex.Message);
        }
        catch
        {
            // If we can't even parse the connection string, let the test run normally
            // and Npgsql will report the real error
            return null;
        }
    }

    private static string BuildProxyErrorMessage(string host, int port, string detail)
    {
        return $"""

            ╔══════════════════════════════════════════════════════════════════════╗
            ║                    DATABASE PROXY NOT RUNNING                       ║
            ╠══════════════════════════════════════════════════════════════════════╣
            ║                                                                    ║
            ║  Cannot connect to {host}:{port,-5}                                ║
            ║  Detail: {detail,-53} ║
            ║                                                                    ║
            ║  PostgreSQL tests require the Cloud SQL Proxy.                      ║
            ║                                                                    ║
            ║  To fix:                                                           ║
            ║    1. Start the Cloud SQL Proxy:                                   ║
            ║       cloud-sql-proxy --port {port} <instance-connection-name>     ║
            ║                                                                    ║
            ║    2. OR run tests with in-memory DB (no proxy needed):            ║
            ║       $env:USE_INMEMORY_DB = "true"                                ║
            ║       dotnet test ...                                              ║
            ║                                                                    ║
            ╚══════════════════════════════════════════════════════════════════════╝
            """;
    }

    /// <summary>
    /// Throws immediately with a clear error if PostgreSQL was configured but
    /// the Cloud SQL Proxy is not reachable. Call sites: CreateAppDbContextOptions
    /// and CreateUNOPSDbContextOptions.
    /// </summary>
    private static void ThrowIfProxyUnavailable()
    {
        if (_proxyError is not null)
        {
            throw new InvalidOperationException(_proxyError);
        }
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
            Pooling = false,
            Timeout = 60,
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
    /// Default: PostgreSQL (with optional IAM auth). Fallback: SQLite in-memory (when USE_INMEMORY_DB=true).
    /// </summary>
    public static DbContextOptions<AppDbContext> CreateAppDbContextOptions(string? databaseName = null)
    {
        ThrowIfProxyUnavailable();

        var builder = new DbContextOptionsBuilder<AppDbContext>();

        if (UsePostgreSQL)
        {
            if (_dataSource != null)
            {
                builder.UseNpgsql(_dataSource, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                    // NOTE: Do NOT use EnableRetryOnFailure — incompatible with
                    // user-initiated transactions (BeginTransaction) used for test isolation.
                });
            }
            else
            {
                builder.UseNpgsql(_connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                    // NOTE: Do NOT use EnableRetryOnFailure — incompatible with
                    // user-initiated transactions (BeginTransaction) used for test isolation.
                });
            }
            builder.EnableSensitiveDataLogging();
        }
        else
        {
            // Use SQLite in-memory instead of InMemory provider.
            // SQLite supports relational model features needed by Z.EntityFramework.Extensions.
            var connection = CreateSqliteConnection();
            builder.UseSqlite(connection);
            // SQLite model may lack navigation properties that exist only in UNOPSAppDbContext.
            // Suppress InvalidIncludePathError so string-based includes degrade gracefully.
            builder.ConfigureWarnings(w => w.Ignore(CoreEventId.InvalidIncludePathError));
        }

        return builder.Options;
    }

    /// <summary>
    /// Creates DbContextOptions for UNOPSAppDbContext based on the test environment.
    /// Default: PostgreSQL (with optional IAM auth). Fallback: SQLite in-memory (when USE_INMEMORY_DB=true).
    /// </summary>
    public static DbContextOptions<UNOPSAppDbContext> CreateUNOPSDbContextOptions(string? databaseName = null)
    {
        ThrowIfProxyUnavailable();

        var builder = new DbContextOptionsBuilder<UNOPSAppDbContext>();

        if (UsePostgreSQL)
        {
            if (_dataSource != null)
            {
                builder.UseNpgsql(_dataSource, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                    // NOTE: Do NOT use EnableRetryOnFailure — incompatible with
                    // user-initiated transactions (BeginTransaction) used for test isolation.
                });
            }
            else
            {
                builder.UseNpgsql(_connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(60);
                    // NOTE: Do NOT use EnableRetryOnFailure — incompatible with
                    // user-initiated transactions (BeginTransaction) used for test isolation.
                });
            }
            builder.EnableSensitiveDataLogging();
        }
        else
        {
            // Use SQLite in-memory instead of InMemory provider.
            // SQLite supports relational model features needed by Z.EntityFramework.Extensions.
            var connection = CreateSqliteConnection();
            builder.UseSqlite(connection);
            builder.ConfigureWarnings(w => w.Ignore(CoreEventId.InvalidIncludePathError));
        }

        return builder.Options;
    }

    /// <summary>
    /// Environment variable: set to "true" to enable SQLite foreign key enforcement.
    /// When enabled, tests that insert data with non-existent FK references will fail,
    /// catching data integrity issues that would fail on PostgreSQL.
    /// </summary>
    public const string EnableForeignKeysEnvVar = "SQLITE_ENABLE_FK";

    /// <summary>
    /// Whether SQLite foreign key constraints are enforced.
    /// Default: false (matches legacy InMemory provider behavior).
    /// Set SQLITE_ENABLE_FK=true to enable strict FK validation.
    /// </summary>
    public static bool EnableForeignKeys { get; } =
        string.Equals(
            Environment.GetEnvironmentVariable(EnableForeignKeysEnvVar),
            "true",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new SQLite in-memory connection, opens it, and tracks it to prevent GC.
    /// Each call returns a fresh connection with its own isolated in-memory database.
    /// Foreign key enforcement is controlled by the SQLITE_ENABLE_FK environment variable.
    /// Default: OFF (matching legacy InMemory provider behavior).
    /// </summary>
    private static SqliteConnection CreateSqliteConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = EnableForeignKeys
            ? "PRAGMA foreign_keys = ON;"
            : "PRAGMA foreign_keys = OFF;";
        cmd.ExecuteNonQuery();

        lock (_sqliteConnections)
        {
            _sqliteConnections.Add(connection);
        }
        return connection;
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
    /// For SQLite/InMemory: creates the schema (each test gets a fresh DB by default).
    /// For PostgreSQL: no-op — the real database schema is managed by EF migrations.
    /// NEVER call EnsureCreated/EnsureDeleted on a real PostgreSQL database.
    /// </summary>
    public static void EnsureCleanDatabase(DbContext context)
    {
        if (UseInMemory)
        {
            context.Database.EnsureCreated();

            // Tables marked ExcludeFromMigrations() (e.g. AspNetUsers, AspNetUserRoles)
            // are NOT created by EnsureCreated(). Create minimal stubs so that
            // string-based includes referencing these tables don't fail with "no such table".
            CreateExcludedTables(context);
        }
    }

    /// <summary>
    /// Creates minimal table stubs for entities marked with ExcludeFromMigrations().
    /// These tables exist in the production PostgreSQL database (managed by Identity)
    /// but are skipped by EnsureCreated() / EF migrations.
    /// </summary>
    private static void CreateExcludedTables(DbContext context)
    {
        try
        {
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUsers"" (
                    ""Id"" INTEGER PRIMARY KEY,
                    ""Email"" TEXT,
                    ""IsInternal"" INTEGER NOT NULL DEFAULT 0,
                    ""ActiveUser"" INTEGER NOT NULL DEFAULT 1,
                    ""UserName"" TEXT,
                    ""NormalizedUserName"" TEXT,
                    ""NormalizedEmail"" TEXT,
                    ""EmailConfirmed"" INTEGER NOT NULL DEFAULT 0,
                    ""PasswordHash"" TEXT,
                    ""SecurityStamp"" TEXT,
                    ""ConcurrencyStamp"" TEXT,
                    ""PhoneNumber"" TEXT,
                    ""PhoneNumberConfirmed"" INTEGER NOT NULL DEFAULT 0,
                    ""TwoFactorEnabled"" INTEGER NOT NULL DEFAULT 0,
                    ""LockoutEnd"" TEXT,
                    ""LockoutEnabled"" INTEGER NOT NULL DEFAULT 0,
                    ""AccessFailedCount"" INTEGER NOT NULL DEFAULT 0
                );
            ");
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""AspNetUserRoles"" (
                    ""UserId"" INTEGER NOT NULL,
                    ""RoleId"" INTEGER NOT NULL,
                    PRIMARY KEY (""UserId"", ""RoleId"")
                );
            ");
        }
        catch
        {
            // Tables may already exist from a previous EnsureCreated call
        }
    }
}
