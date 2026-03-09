/// <summary>
/// Specification for UTF-8 Client Encoding in PostgreSQL connection strings.
/// Replicates the logic from Startup.ConfigureDataAccess (commit 2c11f14e).
///
/// Requirements:
///   REQ-1: Append ";Client Encoding=UTF8" when not present
///   REQ-2: Do not append when "Client Encoding" exists (case-insensitive)
///   REQ-3: Do not append when "ClientEncoding" exists (case-insensitive)
///   REQ-4: Case-insensitive matching (OrdinalIgnoreCase)
///   REQ-5: Accented characters survive round-trip with UTF-8
///   REQ-6: Appended value is exactly ";Client Encoding=UTF8"
/// </summary>

namespace UNOPS.PAO.Business.Tests.ConnectionStringEncoding;

public static class ConnectionStringEncodingSpec
{
    public const string Utf8Suffix = ";Client Encoding=UTF8";
    public const string Utf8EncodingSuffix = Utf8Suffix;

    /// <summary>
    /// Replicates the encoding logic from Startup.cs line 584-590.
    /// </summary>
    public static string EnsureUtf8ClientEncoding(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return connectionString + Utf8Suffix;

        if (!connectionString.Contains("Client Encoding", StringComparison.OrdinalIgnoreCase)
            && !connectionString.Contains("ClientEncoding", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString + Utf8Suffix;
        }

        return connectionString;
    }

    /// <summary>
    /// Validates that a string contains valid UTF-8 characters (accented, CJK, emoji, etc.)
    /// </summary>
    public static bool IsValidUtf8(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var roundTrip = System.Text.Encoding.UTF8.GetString(bytes);
        return string.Equals(text, roundTrip, StringComparison.Ordinal);
    }

    public const string SampleConnectionString = "Host=localhost;Database=opportunityplus;Port=5432";
    public const string WithEncodingAlready = "Host=localhost;Database=test;Client Encoding=UTF8";
    public const string WithEncodingNoSpace = "Host=localhost;Database=test;ClientEncoding=UTF8";
}
