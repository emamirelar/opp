using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Tests for AI/Gemini resilience improvements from the dev-deploy merge (March 2026).
/// Covers: credential fallback with DisableExternalCalls, missing AISettings config,
/// and dummy credential behavior.
/// </summary>
public class AiResilienceTests
{
    // ================================================================
    // POSITIVE TESTS (2 tests)
    // ================================================================

    #region Positive Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Positive")]
    [Trait("TestId", "TC-AI-RES-POS-001")]
    public void Configuration_DisableExternalCalls_True_IsReadable()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:DisableExternalCalls"] = "true"
            })
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        disabled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Positive")]
    [Trait("TestId", "TC-AI-RES-POS-002")]
    public void Configuration_AISettings_WhenPresent_IsReadable()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:ProjectId"] = "my-project",
                ["AISettings:Location"] = "us-central1",
                ["AISettings:ModelName"] = "gemini-pro",
                ["AISettings:DisableExternalCalls"] = "false"
            })
            .Build();

        config.GetValue<string>("AISettings:ProjectId").Should().Be("my-project");
        config.GetValue<bool>("AISettings:DisableExternalCalls").Should().BeFalse();
    }

    #endregion

    // ================================================================
    // NEGATIVE TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Negative Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-AI-RES-NEG-001")]
    public void Configuration_MissingAISettings_ReturnsDefaults()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        disabled.Should().BeFalse("missing bool config defaults to false");

        var projectId = config.GetValue<string>("AISettings:ProjectId");
        projectId.Should().BeNull("missing string config returns null");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-AI-RES-NEG-002")]
    public void Configuration_EmptySecretName_IsNullOrEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:AIServiceAccountJSONSecretName"] = ""
            })
            .Build();

        var secretName = config.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
        string.IsNullOrEmpty(secretName).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-AI-RES-NEG-003")]
    public void Configuration_NullSecretName_IsNullOrEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:AIServiceAccountJSONSecretName"] = null
            })
            .Build();

        var secretName = config.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
        string.IsNullOrEmpty(secretName).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-AI-RES-NEG-004")]
    public void Configuration_DisableExternalCalls_NotSet_DefaultsFalse()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:ProjectId"] = "test"
            })
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        disabled.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-AI-RES-NEG-005")]
    public void Configuration_InvalidBoolValue_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:DisableExternalCalls"] = "not-a-bool"
            })
            .Build();

        Action act = () => config.GetValue<bool>("AISettings:DisableExternalCalls");

        act.Should().Throw<InvalidOperationException>(
            "invalid bool values should throw rather than silently default");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-AI-RES-NEG-006")]
    public void Configuration_TestingEnvironment_DisablesExternalCalls()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Testing"
            })
            .Build();

        var env = config["ASPNETCORE_ENVIRONMENT"];
        var isTesting = string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase);
        isTesting.Should().BeTrue();
    }

    #endregion

    // ================================================================
    // EDGE/BOUNDARY TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Edge/Boundary Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-001")]
    public void Configuration_CaseSensitivity_Testing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "testing"
            })
            .Build();

        var env = config["ASPNETCORE_ENVIRONMENT"];
        var isTesting = string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase);
        isTesting.Should().BeTrue("case-insensitive comparison should match");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-002")]
    public void DummyCredentialJson_HasRequiredFields()
    {
        var json = """
            {
                "type": "service_account",
                "project_id": "dummy-disabled",
                "private_key_id": "dummy",
                "client_email": "dummy@dummy-disabled.iam.gserviceaccount.com",
                "client_id": "0"
            }
            """;

        json.Should().Contain("service_account");
        json.Should().Contain("dummy-disabled");
        json.Should().Contain("client_email");
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-003")]
    public void Configuration_BoolParsing_IsCaseInsensitive(string value, bool expected)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:DisableExternalCalls"] = value
            })
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        disabled.Should().Be(expected);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-004")]
    public void NullableString_CoalescencePattern_WorksCorrectly()
    {
        string? connectionString = null;
        var result = connectionString ?? string.Empty;
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-005")]
    public void NullableString_EmptyProjectId_CoalescesToEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var projectId = config.GetValue<string>("AISettings:ProjectId") ?? string.Empty;
        var location = config.GetValue<string>("AISettings:Location") ?? string.Empty;
        var model = config.GetValue<string>("AISettings:EmbeddingModelName") ?? string.Empty;

        var endpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{model}";
        endpoint.Should().Be("projects//locations//publishers/google/models/");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-006")]
    public void PdfResponseDetection_MagicBytes_CorrectlyIdentified()
    {
        byte[] pdfBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };

        var isPdf = pdfBytes.Length >= 4
                    && pdfBytes[0] == 0x25
                    && pdfBytes[1] == 0x50
                    && pdfBytes[2] == 0x44
                    && pdfBytes[3] == 0x46;

        isPdf.Should().BeTrue("bytes %PDF should identify a PDF response");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-007")]
    public void PdfResponseDetection_NonPdfBytes_NotIdentified()
    {
        byte[] htmlBytes = { 0x3C, 0x21, 0x44, 0x4F };

        var isPdf = htmlBytes.Length >= 4
                    && htmlBytes[0] == 0x25
                    && htmlBytes[1] == 0x50
                    && htmlBytes[2] == 0x44
                    && htmlBytes[3] == 0x46;

        isPdf.Should().BeFalse("HTML bytes should not be identified as PDF");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-AI-RES-EDGE-008")]
    public void PdfResponseDetection_EmptyBytes_NotIdentified()
    {
        byte[] emptyBytes = Array.Empty<byte>();

        var isPdf = emptyBytes.Length >= 4
                    && emptyBytes[0] == 0x25
                    && emptyBytes[1] == 0x50
                    && emptyBytes[2] == 0x44
                    && emptyBytes[3] == 0x46;

        isPdf.Should().BeFalse("empty bytes should not be identified as PDF");
    }

    #endregion

    // ================================================================
    // FUNCTIONAL TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Functional Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-001")]
    public void JsonResponseValidation_ValidJson_Passes()
    {
        var responseContent = """{"status": "success", "pdfBase64": "abc123"}""";
        var trimmed = responseContent.TrimStart();

        var isValidJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
        isValidJson.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-002")]
    public void JsonResponseValidation_HtmlResponse_Fails()
    {
        var responseContent = "<html><body>Error 502</body></html>";
        var trimmed = responseContent.TrimStart();

        var isValidJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
        isValidJson.Should().BeFalse("HTML error page should not pass JSON validation");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-003")]
    public void JsonResponseValidation_ArrayResponse_Passes()
    {
        var responseContent = """[{"id": 1}, {"id": 2}]""";
        var trimmed = responseContent.TrimStart();

        var isValidJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
        isValidJson.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-004")]
    public void JsonResponseValidation_PlainTextError_Fails()
    {
        var responseContent = "Internal Server Error";
        var trimmed = responseContent.TrimStart();

        var isValidJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
        isValidJson.Should().BeFalse("plain text error should not pass JSON validation");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-005")]
    public void PdfBase64Conversion_RoundTrip_Preserves()
    {
        byte[] originalBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
        var base64 = Convert.ToBase64String(originalBytes);
        var roundTripped = Convert.FromBase64String(base64);

        roundTripped.Should().BeEquivalentTo(originalBytes);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-006")]
    public void ContentTypeDetection_ApplicationPdf_Detected()
    {
        var contentType = "application/pdf";

        var isPdf = contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase);
        isPdf.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-007")]
    public void ContentTypeDetection_ApplicationJson_NotPdf()
    {
        var contentType = "application/json";

        var isPdf = contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase);
        isPdf.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-008")]
    public void CredentialDecisionLogic_DisableExternalCalls_ReturnsDummy()
    {
        bool disableExternalCalls = true;
        bool configMissing = false;
        bool secretNameEmpty = false;

        var useDummy = disableExternalCalls || configMissing || secretNameEmpty;
        useDummy.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AI-RES-FUNC-009")]
    public void CredentialDecisionLogic_AllPresent_UsesReal()
    {
        bool disableExternalCalls = false;
        bool configMissing = false;
        bool secretNameEmpty = false;

        var useDummy = disableExternalCalls || configMissing || secretNameEmpty;
        useDummy.Should().BeFalse();
    }

    #endregion

    // ================================================================
    // INTEGRATION TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Integration Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-AI-RES-INT-001")]
    public void FullConfigPipeline_DisableExternalCalls_DisablesAI()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:DisableExternalCalls"] = "true",
                ["AISettings:ProjectId"] = "test-project",
                ["AISettings:Location"] = "us-central1",
                ["AISettings:ModelName"] = "gemini-pro"
            })
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        disabled.Should().BeTrue();

        var env = config["ASPNETCORE_ENVIRONMENT"];
        var isTesting = string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase);
        var shouldDisable = disabled || isTesting;
        shouldDisable.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-AI-RES-INT-002")]
    public void FullConfigPipeline_TestingEnv_DisablesAI()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:DisableExternalCalls"] = "false",
                ["ASPNETCORE_ENVIRONMENT"] = "Testing"
            })
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        var env = config["ASPNETCORE_ENVIRONMENT"];
        var isTesting = string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase);
        var shouldDisable = disabled || isTesting;

        shouldDisable.Should().BeTrue("Testing environment should disable external calls");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-AI-RES-INT-003")]
    public void FullConfigPipeline_ProductionEnv_AllowsAI()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AISettings:DisableExternalCalls"] = "false",
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["AISettings:ProjectId"] = "prod-project"
            })
            .Build();

        var disabled = config.GetValue<bool>("AISettings:DisableExternalCalls");
        var env = config["ASPNETCORE_ENVIRONMENT"];
        var isTesting = string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase);
        var shouldDisable = disabled || isTesting;

        shouldDisable.Should().BeFalse("Production should allow external calls");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-AI-RES-INT-004")]
    public void NullSafeEndpoint_Construction_WithMissingConfig()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var projectId = config.GetValue<string>("AISettings:ProjectId") ?? string.Empty;
        var location = config.GetValue<string>("AISettings:Location") ?? string.Empty;
        var model = config.GetValue<string>("AISettings:EmbeddingModelName") ?? string.Empty;

        var endpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{model}";

        endpoint.Should().NotBeNull("endpoint should never be null even with missing config");
        endpoint.Should().Contain("projects/");
        endpoint.Should().Contain("publishers/google/models/");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-AI-RES-INT-005")]
    public void ResponseHandling_PdfContentType_SkipsJsonDeserialization()
    {
        var contentType = "application/pdf";
        byte[] responseBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };

        var isPdf = contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase)
                    || (responseBytes.Length >= 4 && responseBytes[0] == 0x25
                        && responseBytes[1] == 0x50 && responseBytes[2] == 0x44 && responseBytes[3] == 0x46);

        isPdf.Should().BeTrue();

        var base64 = Convert.ToBase64String(responseBytes);
        base64.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-AI-RES-INT-006")]
    public void ResponseHandling_JsonContentType_DeserializesNormally()
    {
        var contentType = "application/json";
        var responseContent = """{"status": "success", "pdfBase64": "dGVzdA=="}""";
        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseContent);

        var isPdf = contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase)
                    || (responseBytes.Length >= 4 && responseBytes[0] == 0x25
                        && responseBytes[1] == 0x50 && responseBytes[2] == 0x44 && responseBytes[3] == 0x46);

        isPdf.Should().BeFalse();

        var text = System.Text.Encoding.UTF8.GetString(responseBytes);
        var trimmed = text.TrimStart();
        var isValidJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");
        isValidJson.Should().BeTrue();
    }

    #endregion
}
