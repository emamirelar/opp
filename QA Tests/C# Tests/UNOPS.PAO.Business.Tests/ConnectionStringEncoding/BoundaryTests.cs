using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ConnectionStringEncoding;

/// <summary>
/// PNO-1166: UTF-8 client encoding — Boundary tests.
/// Edge values, case boundaries, substring boundaries.
/// </summary>
public class ConnectionStringEncodingBoundaryTests
{
    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_EmptyString_AppendsSuffix()
    {
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding("");
        result.Should().Be(ConnectionStringEncodingSpec.Utf8Suffix, "Spec appends suffix to empty string");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_Null_AppendsSuffix()
    {
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(null!);
        result.Should().Be(ConnectionStringEncodingSpec.Utf8Suffix, "Spec appends suffix to null (null + suffix = suffix)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_SingleParam_AppendsEncoding()
    {
        var input = "Host=localhost";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be("Host=localhost;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_ExactMatch_WithSpace_AtEnd()
    {
        var input = "Host=localhost;Database=test;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_ExactMatch_NoSpace_AtEnd()
    {
        var input = "Host=localhost;Database=test;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_AtVeryStart()
    {
        var input = "Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_AtVeryStart()
    {
        var input = "ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_ImmediatelyAfterSemicolon()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ConnectionString_EndsWithSemicolon_AppendsCorrectly()
    {
        var input = "Host=localhost;";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be("Host=localhost;;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ConnectionString_NoSemicolon_AppendsWithSemicolon()
    {
        var input = "Host=localhost";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Contain(";Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_Lowercase_ExactBoundary()
    {
        var input = "Host=localhost;client encoding=utf8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_Uppercase_ExactBoundary()
    {
        var input = "Host=localhost;CLIENT ENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_Lowercase_ExactBoundary()
    {
        var input = "Host=localhost;clientencoding=utf8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_Uppercase_ExactBoundary()
    {
        var input = "Host=localhost;CLIENTENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_OneCharDifferent_ClientEncodin_Appends()
    {
        var input = "Host=localhost;ClientEncodin=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_OneCharDifferent_ClientEncodings_DoesNotAppend()
    {
        var input = "Host=localhost;ClientEncodings=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input, "ClientEncodings contains ClientEncoding substring - spec uses Contains, so no append");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_OneCharDifferent_ClientEncodin_Appends()
    {
        var input = "Host=localhost;Client Encodin=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_VeryLongConnectionString_AppendsCorrectly()
    {
        var baseInput = "Host=localhost;Database=test";
        var longParam = new string('x', 1000);
        var input = $"{baseInput};{longParam}=value";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
        result.Should().StartWith(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_UnicodeInValue()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ConnectionString_WithUnicodeInHost_AppendsEncoding()
    {
        var input = "Host=db.ünops.org;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_Utf8Suffix_StartsWithSemicolon()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().StartWith(";");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_Utf8Suffix_EndsWithUTF8()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().EndWith("UTF8");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_Utf8Suffix_ContainsSpace()
    {
        ConnectionStringEncodingSpec.Utf8EncodingSuffix.Should().Contain(" ");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_ValueUTF8_ExactMatch()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_ValueUTF8_ExactMatch()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IsValidUtf8_EmptyString_ReturnsTrue()
    {
        ConnectionStringEncodingSpec.IsValidUtf8("").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IsValidUtf8_Null_ReturnsTrue()
    {
        ConnectionStringEncodingSpec.IsValidUtf8(null!).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IsValidUtf8_SingleAccentedChar()
    {
        ConnectionStringEncodingSpec.IsValidUtf8("á").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IsValidUtf8_ASCIIOnly_ReturnsTrue()
    {
        ConnectionStringEncodingSpec.IsValidUtf8("Hello World").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_CaseVariations_AllMatch()
    {
        var variations = new[] { "client encoding", "CLIENT ENCODING", "Client Encoding", "ClIeNt EnCoDiNg" };
        foreach (var v in variations)
        {
            var input = $"Host=localhost;{v}=UTF8";
            var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
            result.Should().Be(input, $"{v} should match case-insensitively");
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_CaseVariations_AllMatch()
    {
        var variations = new[] { "clientencoding", "CLIENTENCODING", "ClientEncoding", "ClIeNtEnCoDiNg" };
        foreach (var v in variations)
        {
            var input = $"Host=localhost;{v}=UTF8";
            var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
            result.Should().Be(input, $"{v} should match case-insensitively");
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ConnectionString_Minimal_AppendsOnce()
    {
        var input = "Host=localhost";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var suffixCount = (result.Length - result.Replace(";Client Encoding=UTF8", "").Length) / ";Client Encoding=UTF8".Length;
        suffixCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_AsSubstringOfValue_DoesNotAppend()
    {
        var input = "Host=localhost;SomeParam=Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input,
            "Contains matches Client Encoding anywhere - even as value of another param");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_AsSubstringOfValue_DoesNotAppend()
    {
        var input = "Host=localhost;SomeParam=ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_WithSpace_ExactKeyMatch()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ClientEncoding_NoSpace_ExactKeyMatch()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ApplyTwice_WhenNoEncoding_AppendsOnce()
    {
        var input = "Host=localhost";
        var first = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var second = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(first);
        second.Should().Be(first, "Second application should not append again");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void EnsureUtf8_ApplyTwice_WhenHasEncoding_Unchanged()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var first = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var second = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(first);
        second.Should().Be(input);
    }
}
