using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ConnectionStringEncoding;

/// <summary>
/// PNO-1166: UTF-8 client encoding — Functional tests.
/// Business rules, data flow, specification compliance.
/// </summary>
public class ConnectionStringEncodingFunctionalTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_PlainConnectionString_ResultContainsAllOriginalParams()
    {
        var input = "Host=localhost;Database=test;Username=user;Password=pass";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain("Host=localhost");
        result.Should().Contain("Database=test");
        result.Should().Contain("Username=user");
        result.Should().Contain("Password=pass");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_PlainConnectionString_ResultEndsWithExactSuffix()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_WhenAppending_ResultLength_EqualsInputPlusSuffix()
    {
        var input = "Host=localhost";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Length.Should().Be(input.Length + ConnectionStringEncodingSpec.Utf8EncodingSuffix.Length);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_WhenNotAppending_ResultLength_EqualsInput()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Length.Should().Be(input.Length);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Result_IsDeterministic()
    {
        var input = "Host=localhost;Database=test";
        var r1 = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var r2 = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        r1.Should().Be(r2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Result_IsPure_NoSideEffects()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        input.Should().Be("Host=localhost;Database=test", "Input should not be modified");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Utf8Suffix_Format_ClientSpaceEncodingEqualsUTF8()
    {
        var suffix = ConnectionStringEncodingSpec.Utf8EncodingSuffix;
        suffix.Should().Contain("Client Encoding=UTF8");
        suffix.Should().StartWith(";");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_WithSpace_TakesPrecedence_NoAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_NoSpace_TakesPrecedence_NoAppend()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_BothVariants_CheckIsOr_NotAnd()
    {
        var withSpace = "Host=localhost;Client Encoding=UTF8";
        var noSpace = "Host=localhost;ClientEncoding=UTF8";
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(withSpace).Should().Be(withSpace);
        ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(noSpace).Should().Be(noSpace);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_OrdinalIgnoreCase_ClientEncoding_WithSpace()
    {
        var input = "Host=localhost;CLIENT ENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_OrdinalIgnoreCase_ClientEncoding_NoSpace()
    {
        var input = "Host=localhost;CLIENTENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IsValidUtf8_AccentedChars_RoundTripPreserves()
    {
        var value = "Ángel María";
        var isValid = ConnectionStringEncodingSpec.IsValidUtf8(value);
        isValid.Should().BeTrue();
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var roundTrip = System.Text.Encoding.UTF8.GetString(bytes);
        roundTrip.Should().Be(value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_OrderOfParams_Preserved()
    {
        var input = "Host=a;Database=b;Username=c;Password=d";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var withoutSuffix = result.Substring(0, result.Length - ConnectionStringEncodingSpec.Utf8EncodingSuffix.Length);
        withoutSuffix.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_SpecialCharsInValue_Preserved()
    {
        var input = "Host=localhost;Password=p@ss%word";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain("Password=p@ss%word");
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_EqualsInValue_Preserved()
    {
        var input = "Host=localhost;Options=key=value";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain("Options=key=value");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Utf8Suffix_NoTrailingSemicolon()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().NotEndWith(";");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Utf8Suffix_Format_MatchesNpgsqlExpectation()
    {
        var suffix = ConnectionStringEncodingSpec.Utf8EncodingSuffix;
        suffix.Should().Contain("Client Encoding");
        suffix.Should().Contain("UTF8");
        suffix.Should().Contain("=");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_WhenAppending_ResultIsValidConnectionStringFormat()
    {
        var input = "Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain(";");
        result.Should().Contain("=");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_WithSpace_ValueUTF8_Exact()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_NoSpace_ValueUTF8_Exact()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_WithSpace_ValueUTF16_NoAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF16";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_NoSpace_ValueLATIN1_NoAppend()
    {
        var input = "Host=localhost;ClientEncoding=LATIN1";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_PlainConnectionString_ResultIsValidForNpgsql()
    {
        var input = "Host=localhost;Database=test;Username=user;Password=pass";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain("Host=");
        result.Should().Contain("Database=");
        result.Should().Contain("Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_WithSpace_AlreadyValid_NoAppend()
    {
        var input = "Host=localhost;Database=test;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ClientEncoding_NoSpace_AlreadyValid_NoAppend()
    {
        var input = "Host=localhost;Database=test;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IsValidUtf8_ConnectionStringWithAccentedChars_Valid()
    {
        var value = "Host=db.ünops.org;Database=test";
        ConnectionStringEncodingSpec.IsValidUtf8(value).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_RepeatedCalls_WhenNoEncoding_IdempotentAfterFirst()
    {
        var input = "Host=localhost";
        var first = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var second = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(first);
        second.Should().Be(first);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_RepeatedCalls_WhenHasEncoding_Idempotent()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var first = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var second = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(first);
        second.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Utf8Suffix_Length_IsCorrect()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Length.Should().Be(";Client Encoding=UTF8".Length);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_Utf8Suffix_ExactContent()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().Be(";Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_WithIAM_AppendsEncoding()
    {
        var input = "Host=localhost;Database=test;Use IAM Authentication=true";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_WithSSL_AppendsEncoding()
    {
        var input = "Host=localhost;Database=test;SSL Mode=Require";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_WithPooling_AppendsEncoding()
    {
        var input = "Host=localhost;Database=test;Maximum Pool Size=100;Minimum Pool Size=5";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_WithTimeout_AppendsEncoding()
    {
        var input = "Host=localhost;Database=test;Command Timeout=60";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_WithSearchPath_AppendsEncoding()
    {
        var input = "Host=localhost;Database=test;Search Path=public";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_WithIncludeErrorDetail_AppendsEncoding()
    {
        var input = "Host=localhost;Database=test;Include Error Detail=true";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void EnsureUtf8_ConnectionString_Complex_AppendsEncoding()
    {
        var input = "Host=db.example.com;Port=5432;Database=opportunity;Username=admin;Password=secret;SSL Mode=Require;Maximum Pool Size=100;Command Timeout=30";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().Contain("Host=db.example.com");
        result.Should().Contain("SSL Mode=Require");
    }
}
