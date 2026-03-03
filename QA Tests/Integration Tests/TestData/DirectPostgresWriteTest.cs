using Xunit;
using Xunit.Abstractions;
using Npgsql;

namespace UNOPS.PAO.IntegrationTests.TestData;

/// <summary>
/// Directly connects to PostgreSQL (bypassing factory) and verifies read/write.
/// Proves the real database is accessible and writable from the test process.
/// </summary>
public class DirectPostgresWriteTest
{
    private readonly ITestOutputHelper _output;

    public DirectPostgresWriteTest(ITestOutputHelper output)
    {
        _output = output;
    }

    private string? GetToken()
    {
        var tokenFile = Path.Combine(Path.GetTempPath(), "gcloud_token.txt");
        if (!File.Exists(tokenFile)) return null;
        var t = File.ReadAllText(tokenFile).Trim();
        return t.Length > 50 ? t : null;
    }

    [Fact]
    public void ReadFromRealPostgres()
    {
        var token = GetToken();
        if (token == null) return; // Skip when not using PostgreSQL (no gcloud IAM token)
        _output.WriteLine($"Token: {token.Length} chars");

        var cs = $"Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Password={token};Timeout=15";

        using var conn = new NpgsqlConnection(cs);
        conn.Open();
        _output.WriteLine("Connected to PostgreSQL!");

        // First discover which schemas and tables exist
        using var schemaCmd = conn.CreateCommand();
        schemaCmd.CommandText = @"
            SELECT table_schema, table_name 
            FROM information_schema.tables 
            WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')
            ORDER BY table_schema, table_name
            LIMIT 100";
        using var reader = schemaCmd.ExecuteReader();
        _output.WriteLine("\n=== Tables in database ===");
        while (reader.Read())
        {
            _output.WriteLine($"  {reader.GetString(0)}.{reader.GetString(1)}");
        }
        reader.Close();

        // Count entities (EF uses plural table names)
        var tables = new[] { "Opportunities", "Partners", "Contacts", "Interactions", "UserProfile" };
        foreach (var table in tables)
        {
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM public.\"{table}\" WHERE \"IsDeleted\" = false";
                var count = cmd.ExecuteScalar();
                _output.WriteLine($"  {table}: {count} active rows");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"  {table}: {ex.Message}");
            }
        }
    }

    [Fact]
    public void WriteAndReadBackFromPostgres()
    {
        var token = GetToken();
        if (token == null) return; // Skip when not using PostgreSQL (no gcloud IAM token)

        var cs = $"Host=127.0.0.1;Port=5432;Database=unops-opportunityplus-dev-db-leonardc;Username=leonardc@unops.org;Password={token};Timeout=15";

        using var conn = new NpgsqlConnection(cs);
        conn.Open();
        _output.WriteLine("Connected to PostgreSQL!");

        // Find a real user ID for FK constraints
        int userId;
        using (var userCmd = conn.CreateCommand())
        {
            userCmd.CommandText = @"SELECT ""Id"" FROM public.""AspNetUsers"" LIMIT 1";
            var result = userCmd.ExecuteScalar();
            Assert.NotNull(result);
            userId = (int)result;
            _output.WriteLine($"Using user Id={userId} for FK constraints");
        }

        // Write a new Opportunity directly via SQL
        var uniqueName = $"IntegrationTest_{Guid.NewGuid():N}";
        int insertedId;

        using (var insertCmd = conn.CreateCommand())
        {
            insertCmd.CommandText = @"
                INSERT INTO public.""Opportunities"" (""Name"", ""Description"", ""Status"", ""IsDeleted"", ""DeletedBy"", ""CreatedDate"", ""CreatedBy"", ""LastModifiedDate"", ""LastModifiedBy"", ""WorkflowStatus"")
                VALUES (@name, @desc, 1, false, 0, NOW(), @uid, NOW(), @uid, 0)
                RETURNING ""Id""";
            insertCmd.Parameters.AddWithValue("name", uniqueName);
            insertCmd.Parameters.AddWithValue("desc", "Written by integration test - will be cleaned up");
            insertCmd.Parameters.AddWithValue("uid", userId);
            insertedId = (int)insertCmd.ExecuteScalar()!;
        }

        _output.WriteLine($"INSERTED Opportunity Id={insertedId}, Name='{uniqueName}'");

        // Read it back
        using (var readCmd = conn.CreateCommand())
        {
            readCmd.CommandText = @"SELECT ""Name"", ""Description"" FROM public.""Opportunities"" WHERE ""Id"" = @id";
            readCmd.Parameters.AddWithValue("id", insertedId);
            using var reader = readCmd.ExecuteReader();
            Assert.True(reader.Read());
            var readName = reader.GetString(0);
            var readDesc = reader.GetString(1);
            _output.WriteLine($"READ BACK: Name='{readName}', Description='{readDesc}'");
            Assert.Equal(uniqueName, readName);
        }

        // Clean up — soft delete
        using (var delCmd = conn.CreateCommand())
        {
            delCmd.CommandText = @"UPDATE public.""Opportunities"" SET ""IsDeleted"" = true, ""DeletedDate"" = NOW() WHERE ""Id"" = @id";
            delCmd.Parameters.AddWithValue("id", insertedId);
            var affected = delCmd.ExecuteNonQuery();
            _output.WriteLine($"SOFT DELETED: {affected} row(s) affected");
            Assert.Equal(1, affected);
        }

        // Verify soft-deleted record is excluded
        using (var verifyCmd = conn.CreateCommand())
        {
            verifyCmd.CommandText = @"SELECT COUNT(*) FROM public.""Opportunities"" WHERE ""Id"" = @id AND ""IsDeleted"" = false";
            verifyCmd.Parameters.AddWithValue("id", insertedId);
            var remaining = (long)verifyCmd.ExecuteScalar()!;
            _output.WriteLine($"VERIFY: Active rows with Id={insertedId}: {remaining} (expected 0)");
            Assert.Equal(0, remaining);
        }

        // Hard delete test data
        using (var hardDelCmd = conn.CreateCommand())
        {
            hardDelCmd.CommandText = @"DELETE FROM public.""Opportunities"" WHERE ""Id"" = @id";
            hardDelCmd.Parameters.AddWithValue("id", insertedId);
            hardDelCmd.ExecuteNonQuery();
            _output.WriteLine("HARD DELETED test data — cleanup complete");
        }

        _output.WriteLine("\n=== POSTGRESQL WRITE TEST PASSED ===");
    }
}
