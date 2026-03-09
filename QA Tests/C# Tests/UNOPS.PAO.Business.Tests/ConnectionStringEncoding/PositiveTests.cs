using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ConnectionStringEncoding;

/// <summary>
/// PNO-1166: UTF-8 client encoding — Positive (happy-path) tests.
///
/// Requirements validated:
/// - REQ-1: When connection string does NOT contain encoding, append ";Client Encoding=UTF8"
/// - REQ-5: Accented characters survive when UTF8 encoding is active
/// - REQ-6: The appended value must be exactly ";Client Encoding=UTF8"
/// </summary>
public class ConnectionStringEncodingPositiveTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_PlainConnectionString_AppendsEncoding_REQ1_REQ6()
    {
        var input = "Host=localhost;Database=test;Username=user;Password=pass";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix, "REQ-6: Must append exactly ;Client Encoding=UTF8");
        result.Should().Be(input + ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_MinimalConnectionString_AppendsEncoding_REQ1()
    {
        var input = "Host=localhost;Database=db";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().Contain("Host=localhost");
        result.Should().Contain("Database=db");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_ConnectionStringWithOtherParams_AppendsEncoding_REQ1()
    {
        var input = "Host=db.example.com;Port=5432;Database=opportunity;Username=admin;Password=secret;SSL Mode=Require";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().Contain("Host=db.example.com");
        result.Should().Contain("SSL Mode=Require");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_AppendedValue_IsExactSuffix_REQ6()
    {
        var input = "Host=localhost";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        var suffix = result.Substring(input.Length);
        suffix.Should().Be(";Client Encoding=UTF8", "REQ-6: Appended value must be exactly ;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_Utf8SuffixConstant_MatchesExpected_REQ6()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().Be(";Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void IsValidUtf8_AccentedCharacters_AngelMaria_REQ5()
    {
        var value = "Ángel María";
        ConnectionStringEncodingSpec.IsValidUtf8(value).Should().BeTrue(
            "REQ-5: Accented characters like Ángel María must survive when UTF8 encoding is active");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void IsValidUtf8_AccentedCharacters_RoundTrip_REQ5()
    {
        var value = "José García-Núñez";
        var isValid = ConnectionStringEncodingSpec.IsValidUtf8(value);
        isValid.Should().BeTrue("UTF-8 round-trip preserves accented characters");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void IsValidUtf8_MixedUnicode_PreservesCorrectly_REQ5()
    {
        var value = "Café résumé naïve 日本語";
        ConnectionStringEncodingSpec.IsValidUtf8(value).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_ConnectionStringWithSemicolon_AppendsCorrectly_REQ1()
    {
        var input = "Host=localhost;Database=test;";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().Be(input + ConnectionStringEncodingSpec.Utf8Suffix, "Spec appends suffix; trailing semicolon yields ;;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_ConnectionStringWithPooling_AppendsEncoding_REQ1()
    {
        var input = "Host=localhost;Database=test;Maximum Pool Size=100";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().Contain("Maximum Pool Size=100");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void EnsureUtf8_ConnectionStringWithTimeout_AppendsEncoding_REQ1()
    {
        var input = "Host=localhost;Database=test;Command Timeout=30";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);

        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }
}
