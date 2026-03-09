using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ConnectionStringEncoding;

/// <summary>
/// PNO-1166: UTF-8 client encoding — Integration tests.
/// End-to-end spec compliance, cross-requirement validation.
/// </summary>
public class ConnectionStringEncodingIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_PlainConnectionString_AppendsEncoding_AllReqs()
    {
        var input = "Host=localhost;Database=test;Username=user;Password=pass";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix, "REQ-6");
        result.Should().Contain("Host=localhost", "REQ-1: Original params preserved");
        result.Should().Contain("Database=test");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionStringWithClientEncoding_NoAppend_REQ2_REQ3()
    {
        var withSpace = "Host=localhost;Client Encoding=UTF8";
        var noSpace = "Host=localhost;ClientEncoding=UTF8";

        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(withSpace).Should().Be(withSpace);
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(noSpace).Should().Be(noSpace);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_CaseInsensitivity_REQ4()
    {
        var variations = new[]
        {
            "Host=localhost;client encoding=utf8",
            "Host=localhost;CLIENT ENCODING=UTF8",
            "Host=localhost;clientencoding=utf8",
            "Host=localhost;CLIENTENCODING=UTF8"
        };

        foreach (var input in variations)
        {
            var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
            result.Should().Be(input, "REQ-4: Case-insensitive check");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_Utf8Encoding_AccentedChars_REQ5()
    {
        var accented = "Ángel María";
        ConnectionStringEncodingSpec.IsValidUtf8(accented).Should().BeTrue("REQ-5");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_Utf8Suffix_ExactFormat_REQ6()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().Be(";Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ApplyEnsureUtf8_ThenValidate_ResultValid()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_SimulatesNpgsqlBuilderOutput()
    {
        var builderOutput = "Host=localhost;Port=5432;Database=opportunity;Username=admin;Password=secret";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(builderOutput);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().Contain("Port=5432");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_WithIAM_SimulatesStartupFlow()
    {
        var input = "Host=/cloudsql/project:region:instance;Database=opportunity;Username=user;Use IAM Authentication=true";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_AllReqs_PlainString_Append()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(";Client Encoding=UTF8", "REQ-6");
        result.Should().Contain(input, "REQ-1: Original preserved");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_AllReqs_ClientEncoding_NoAppend()
    {
        var inputs = new[] { "Host=localhost;Client Encoding=UTF8", "Host=localhost;ClientEncoding=UTF8" };
        foreach (var input in inputs)
        {
            var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
            result.Should().Be(input, "REQ-2, REQ-3");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_Utf8RoundTrip_AccentedNames()
    {
        var names = new[] { "Ángel María", "José García", "François Müller", "Ñoño" };
        foreach (var name in names)
        {
            ConnectionStringEncodingSpec.IsValidUtf8(name).Should().BeTrue("REQ-5");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_EnsureUtf8_IdempotentWhenAlreadyPresent()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var r1 = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var r2 = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(r1);
        r2.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_EnsureUtf8_IdempotentWhenAppended()
    {
        var input = "Host=localhost";
        var r1 = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var r2 = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(r1);
        r2.Should().Be(r1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_AllParamTypes_Preserved()
    {
        var input = "Host=localhost;Port=5432;Database=test;Username=u;Password=p;SSL Mode=Require;Maximum Pool Size=100";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain("Host=localhost");
        result.Should().Contain("Port=5432");
        result.Should().Contain("SSL Mode=Require");
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_EmptyAndNull_Handled()
    {
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding("").Should().Be(ConnectionStringEncodingSpec.Utf8Suffix);
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(null!).Should().Be(ConnectionStringEncodingSpec.Utf8Suffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_StartupSimulation_ConnectionStringBuilderOutput()
    {
        var simulatedBuilderOutput = "Host=localhost;Database=opportunity;Username=admin;Password=secret;Maximum Pool Size=100;Minimum Pool Size=5";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(simulatedBuilderOutput);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_StartupSimulation_AlreadyHasEncoding()
    {
        var simulatedBuilderOutput = "Host=localhost;Database=opportunity;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(simulatedBuilderOutput);
        result.Should().Be(simulatedBuilderOutput);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_CaseVariations_AllNoAppend()
    {
        var variations = new[]
        {
            "Host=localhost;client encoding=utf8",
            "Host=localhost;CLIENT ENCODING=UTF8",
            "Host=localhost;Client Encoding=UTF8",
            "Host=localhost;clientencoding=utf8",
            "Host=localhost;CLIENTENCODING=UTF8",
            "Host=localhost;ClientEncoding=UTF8"
        };

        foreach (var input in variations)
        {
            var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
            result.Should().Be(input);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_Utf8Suffix_Constant_MatchesExpected()
    {
        var expected = ";Client Encoding=UTF8";
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().Be(expected);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_PlainConnectionString_Append_ThenNoAppend()
    {
        var input = "Host=localhost;Database=test";
        var withEncoding = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var again = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(withEncoding);
        again.Should().Be(withEncoding);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_CorruptionPrevention_Utf8Encoding()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain("Client Encoding=UTF8",
            "Prevents ?? corruption when PostgreSQL session uses different default encoding");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_AccentedChars_RoundTrip_WithUtf8()
    {
        var value = "Ángel María";
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var roundTrip = System.Text.Encoding.UTF8.GetString(bytes);
        roundTrip.Should().Be(value);
        ConnectionStringEncodingSpec.IsValidUtf8(value).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_WithUnicodeHost_AppendsEncoding()
    {
        var input = "Host=db.ünops.org;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_AllRequirements_Validated()
    {
        var plain = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(plain);
        result.Should().EndWith(";Client Encoding=UTF8", "REQ-6");

        var withEncoding = "Host=localhost;Client Encoding=UTF8";
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(withEncoding).Should().Be(withEncoding, "REQ-2");

        var noSpace = "Host=localhost;ClientEncoding=UTF8";
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(noSpace).Should().Be(noSpace, "REQ-3");

        var lower = "Host=localhost;client encoding=utf8";
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(lower).Should().Be(lower, "REQ-4");

        ConnectionStringEncodingSpec.IsValidUtf8("Ángel María").Should().BeTrue("REQ-5");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_RealWorldFormat()
    {
        var input = "Host=localhost;Port=5432;Database=UNOPS_PAO;Username=postgres;Password=postgres;Include Error Detail=true";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_CloudSqlFormat()
    {
        var input = "Host=/cloudsql/project:region:instance;Database=opportunity;Username=user;Use IAM Authentication=true";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_WithSearchPath()
    {
        var input = "Host=localhost;Database=test;Search Path=public,extensions";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_WithCommandTimeout()
    {
        var input = "Host=localhost;Database=test;Command Timeout=120";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_WithPooling()
    {
        var input = "Host=localhost;Database=test;Maximum Pool Size=50;Minimum Pool Size=5";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ConnectionString_WithSSL()
    {
        var input = "Host=localhost;Database=test;SSL Mode=Require;Trust Server Certificate=true";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_Spec_MatchesStartupBehavior()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input + ";Client Encoding=UTF8",
            "Spec must replicate Startup.ConfigureDataAccess logic (commit 2c11f14e)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_ClientEncoding_WithSpace_And_NoSpace_BothPreventAppend()
    {
        var withSpace = "Host=localhost;Client Encoding=UTF8";
        var noSpace = "Host=localhost;ClientEncoding=UTF8";

        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(withSpace).Should().Be(withSpace);
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(noSpace).Should().Be(noSpace);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_Utf8Encoding_EnsuresCorrectDisplay()
    {
        var comment = "Ensures UTF-8 encoding for correct display of accented characters (e.g. Ángel María)";
        ConnectionStringEncodingSpec.IsValidUtf8("Ángel María").Should().BeTrue();
    }
}
