using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.ConnectionStringEncoding;

/// <summary>
/// PNO-1166: UTF-8 client encoding — Negative tests.
///
/// Requirements validated:
/// - REQ-2: When "Client Encoding" present (any case), do NOT append
/// - REQ-3: When "ClientEncoding" present (any case), do NOT append
/// - REQ-4: Check must be case-insensitive
/// </summary>
public class ConnectionStringEncodingNegativeTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_DoesNotAppend_REQ2()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
        result.Should().NotContain(";Client Encoding=UTF8;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_Lowercase_DoesNotAppend_REQ2_REQ4()
    {
        var input = "Host=localhost;client encoding=utf8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input, "REQ-4: Case-insensitive check");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_Uppercase_DoesNotAppend_REQ2_REQ4()
    {
        var input = "Host=localhost;CLIENT ENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_MixedCase_DoesNotAppend_REQ2_REQ4()
    {
        var input = "Host=localhost;Client ENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_WithSpace_DoesNotAppend_REQ2()
    {
        var input = "Host=localhost;Database=test;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_AtStart_DoesNotAppend_REQ2()
    {
        var input = "Client Encoding=UTF8;Host=localhost;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_InMiddle_DoesNotAppend_REQ2()
    {
        var input = "Host=localhost;Client Encoding=UTF8;Database=test";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_WithDifferentValue_DoesNotAppend_REQ2()
    {
        var input = "Host=localhost;Client Encoding=LATIN1";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_DoesNotDuplicateSuffix()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        var count = (result.Length - result.Replace("Client Encoding=UTF8", "").Length) / "Client Encoding=UTF8".Length;
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_DoesNotAppend_REQ3()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_Lowercase_DoesNotAppend_REQ3_REQ4()
    {
        var input = "Host=localhost;clientencoding=utf8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input, "REQ-4: Case-insensitive check for ClientEncoding");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_Uppercase_DoesNotAppend_REQ3_REQ4()
    {
        var input = "Host=localhost;CLIENTENCODING=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_MixedCase_DoesNotAppend_REQ3()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_AtEnd_DoesNotAppend_REQ3()
    {
        var input = "Host=localhost;Database=test;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_WithDifferentValue_DoesNotAppend_REQ3()
    {
        var input = "Host=localhost;ClientEncoding=LATIN1";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_Substring_DoesNotMatch()
    {
        var input = "Host=localhost;SomeClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input, "SomeClientEncoding is not ClientEncoding");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_Substring_ClientEncodingX()
    {
        var input = "Host=localhost;ClientEncodingX=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input, "ClientEncodingX contains ClientEncoding substring - spec uses Contains, so no append");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_Substring_XClientEncoding()
    {
        var input = "Host=localhost;XClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input, "XClientEncoding contains ClientEncoding substring - spec uses Contains, so no append");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_PartialMatch_ClientEnc()
    {
        var input = "Host=localhost;ClientEnc=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix, "ClientEnc is not Client Encoding or ClientEncoding");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_PartialMatch_Encoding()
    {
        var input = "Host=localhost;Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_PartialMatch_ClientEncod()
    {
        var input = "Host=localhost;ClientEncod=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().EndWith(ConnectionStringEncodingSpec.Utf8EncodingSuffix);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_WithSpace_DoesNotAppendTwice()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().NotContain(";Client Encoding=UTF8;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_DoesNotAppendTwice()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().NotContain(";Client Encoding=UTF8;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_ReturnsSameReferenceWhenNoAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().BeSameAs(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_WithSpace_DoesNotAppendWrongSuffix()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().NotContain(";ClientEncoding=UTF8", "Client Encoding (with space) is the correct format");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_EmptyString_DoesNotThrow()
    {
        var act = () => ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding("");
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_Null_DoesNotThrow()
    {
        var act = () => ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(null!);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_Latin1_DoesNotAppend()
    {
        var input = "Host=localhost;Client Encoding=LATIN1";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_UTF8_DoesNotAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_UTF16_DoesNotAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF16";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_MultipleParams_DoesNotAppend()
    {
        var input = "Host=localhost;Port=5432;Database=test;Client Encoding=UTF8;SSL Mode=Require";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_WithSpace_DoesNotAppend_ClientEncoding()
    {
        var input = "Host=localhost;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().NotContain(";Client Encoding=UTF8;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_NoSpace_DoesNotAppend_ClientEncoding()
    {
        var input = "Host=localhost;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().NotContain(";Client Encoding=UTF8;Client Encoding=UTF8");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_WhitespaceAround_DoesNotAppend()
    {
        var input = "Host=localhost; Client Encoding=UTF8 ";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsClientEncoding_MultipleOccurrences_DoesNotAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF8;Database=test;Client Encoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void EnsureUtf8_ContainsBoth_ClientEncoding_And_ClientEncoding_DoesNotAppend()
    {
        var input = "Host=localhost;Client Encoding=UTF8;ClientEncoding=UTF8";
        var result = ConnectionStringEncodingSpec.EnsureUtf8ClientEncoding(input);
        result.Should().Be(input);
    }
}
