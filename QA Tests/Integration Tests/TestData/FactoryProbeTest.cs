using Xunit;
using Xunit.Abstractions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;

namespace UNOPS.PAO.IntegrationTests.TestData;

[Collection("Integration Tests")]
public class FactoryProbeTest
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public FactoryProbeTest(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public void FactoryReportsPostgresStatus()
    {
        // Force factory initialization by accessing Services
        var _ = _factory.Services;
        _output.WriteLine($"IsUsingPostgres: {_factory.IsUsingPostgres}");

        // Reproduce the factory probe logic with logging
        var tokenFile = Path.Combine(Path.GetTempPath(), "gcloud_token.txt");
        _output.WriteLine($"Token file path: {tokenFile}");
        _output.WriteLine($"Token file exists: {File.Exists(tokenFile)}");

        if (File.Exists(tokenFile))
        {
            var token = File.ReadAllText(tokenFile).Trim();
            _output.WriteLine($"Token length: {token.Length}");

            // Try connecting with this token
            var cs = $"Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Password={token};Timeout=15";
            try
            {
                using var conn = new Npgsql.NpgsqlConnection(cs);
                conn.Open();
                _output.WriteLine("Direct connection: SUCCESS");

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM \"UserProfile\" LIMIT 1";
                cmd.ExecuteNonQuery();
                _output.WriteLine("UserProfile SELECT: SUCCESS");

                conn.Close();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Direct connection FAILED: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    _output.WriteLine($"  Inner: {ex.InnerException.Message}");
            }
        }

        // Try via NpgsqlDataSource (like the factory does)
        _output.WriteLine("\n--- NpgsqlDataSource path ---");
        try
        {
            var connBuilder = new Npgsql.NpgsqlConnectionStringBuilder(
                "Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Timeout=15")
            {
                MinPoolSize = 2,
                MaxPoolSize = 20
            };

            var fileToken = File.Exists(tokenFile) ? File.ReadAllText(tokenFile).Trim() : null;
            if (!string.IsNullOrEmpty(fileToken) && fileToken.Length > 50)
            {
                connBuilder.Password = fileToken;
                _output.WriteLine($"Injected token as password ({fileToken.Length} chars)");
            }

            var dsBuilder = new Npgsql.NpgsqlDataSourceBuilder(connBuilder.ConnectionString);
            using var dataSource = dsBuilder.Build();

            using var probe = dataSource.OpenConnection();
            _output.WriteLine("DataSource.OpenConnection: SUCCESS");

            using var cmd = probe.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM \"UserProfile\" LIMIT 1";
            cmd.ExecuteNonQuery();
            _output.WriteLine("UserProfile SELECT via DataSource: SUCCESS");
            probe.Close();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"DataSource probe FAILED: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                _output.WriteLine($"  Inner: {ex.InnerException.Message}");
        }
    }
}
