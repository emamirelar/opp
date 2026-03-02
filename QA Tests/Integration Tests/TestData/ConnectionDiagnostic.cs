using Xunit;
using Xunit.Abstractions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;

namespace UNOPS.PAO.IntegrationTests.TestData;

[Collection("Integration Tests")]
public class ConnectionDiagnostic
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public ConnectionDiagnostic(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task DiagnosePostgresConnection()
    {
        _output.WriteLine($"IsUsingPostgres from factory: {_factory.IsUsingPostgres}");

        // Read pre-generated token from temp file (gcloud may not be in PATH for test process)
        var tokenFile = Path.Combine(Path.GetTempPath(), "gcloud_token.txt");
        string? fileToken = null;
        if (File.Exists(tokenFile))
        {
            fileToken = File.ReadAllText(tokenFile).Trim();
            _output.WriteLine($"Token loaded from file, length: {fileToken.Length}");
        }
        else
        {
            _output.WriteLine("No token file found at " + tokenFile);
        }

        // Also try getting token via Google Application Default Credentials
        string? adcToken = null;
        try
        {
            var credential = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();
            adcToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            _output.WriteLine($"ADC token obtained, length: {adcToken?.Length}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ADC token failed: {ex.GetType().Name}: {ex.Message}");
        }

        // Raw TCP probe: can the proxy even respond at the PostgreSQL protocol level?
        _output.WriteLine("\n--- Raw TCP probe on 127.0.0.1:5432 ---");
        try
        {
            using var tcp = new System.Net.Sockets.TcpClient();
            tcp.Connect("127.0.0.1", 5432);
            _output.WriteLine($"  TCP connected: {tcp.Connected}");

            // Send a PostgreSQL SSLRequest message
            var sslRequest = new byte[] { 0, 0, 0, 8, 4, 210, 22, 47 };
            var stream = tcp.GetStream();
            stream.Write(sslRequest, 0, sslRequest.Length);
            stream.ReadTimeout = 5000;

            var buffer = new byte[1];
            var bytesRead = stream.Read(buffer, 0, 1);
            if (bytesRead > 0)
            {
                var ch = (char)buffer[0];
                _output.WriteLine($"  SSL probe response: '{ch}' (0x{buffer[0]:X2})");
                _output.WriteLine($"  Interpretation: {(ch == 'S' ? "SSL supported" : ch == 'N' ? "SSL not supported (use plain)" : "Unknown")}");
            }
            else
            {
                _output.WriteLine("  No bytes received from proxy");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"  TCP FAILED: {ex.GetType().Name}: {ex.Message}");
        }

        // Try connection variants
        var variants = new List<(string Label, string ConnStr)>();

        if (!string.IsNullOrEmpty(fileToken))
            variants.Add(("gcloud token", $"Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Password={fileToken};Timeout=15"));

        if (!string.IsNullOrEmpty(adcToken))
            variants.Add(("ADC token", $"Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Password={adcToken};Timeout=15"));

        variants.Add(("No password", "Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Timeout=15"));

        foreach (var (label, cs) in variants)
        {
            _output.WriteLine($"\n--- {label} ---");
            try
            {
                using var c = new Npgsql.NpgsqlConnection(cs);
                c.Open();
                _output.WriteLine("  SUCCESS!");
                using var cmd = c.CreateCommand();
                cmd.CommandText = "SELECT current_user, current_database()";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    _output.WriteLine($"  current_user: {reader.GetString(0)}");
                    _output.WriteLine($"  current_database: {reader.GetString(1)}");
                }

                c.Close();
                _output.WriteLine("  CONNECTION VERIFIED - PostgreSQL is reachable!");
                return;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"  FAILED: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                    _output.WriteLine($"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
        }

        _output.WriteLine("\n--- PeriodicPasswordProvider (Startup.cs pattern) ---");
        try
        {
            var dsBuilder = new Npgsql.NpgsqlDataSourceBuilder(
                "Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Timeout=15");
            dsBuilder.UsePeriodicPasswordProvider(
                async (settings, ct) =>
                {
                    var cred = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync(ct);
                    return await cred.UnderlyingCredential.GetAccessTokenForRequestAsync(cancellationToken: ct);
                },
                TimeSpan.FromMinutes(55),
                TimeSpan.FromSeconds(5));
            await using var dataSource = dsBuilder.Build();
            await using var conn = await dataSource.OpenConnectionAsync();
            _output.WriteLine("  SUCCESS with PeriodicPasswordProvider!");
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT current_user, current_database()";
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _output.WriteLine($"  current_user: {reader.GetString(0)}");
                _output.WriteLine($"  current_database: {reader.GetString(1)}");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"  FAILED: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                _output.WriteLine($"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
        }
    }
}
