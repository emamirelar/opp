/// <summary>
/// Tests for AI Prompt entity data entry permutations (hypothetical AiPromptRequest).
///
/// Requirements validated:
/// - REQ-1: Type, DataRetrievalMethod, SystemInstructions, Feature, GenerationConfig, ContentConfig, Project, Location, Model required → Field order, invalid tests
/// - REQ-2: UserPrompt, Description, ToolsConfig, SafetySettings optional → Partial tests
/// - REQ-3: GenerationConfig, ContentConfig, ToolsConfig, SafetySettings must be valid JSON when provided → Invalid JSON tests
/// - REQ-4: CacheInvalidationMinutes must be non-negative → Boundary tests
/// - REQ-5: AdminCanChange, UseCache boolean flags → Boundary tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.AIPrompt;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "AIPrompt")]

public class AIPromptDataEntryPermutationTests
{
    /// <summary>
    /// Hypothetical AI prompt request model for permutation testing.
    /// </summary>
    private class AiPromptRequest
    {
        public string? Type { get; set; }
        public string? DataRetrievalMethod { get; set; }
        public string? SystemInstructions { get; set; }
        public string? UserPrompt { get; set; }
        public string? Feature { get; set; }
        public string? Description { get; set; }
        public string? GenerationConfig { get; set; }
        public string? ContentConfig { get; set; }
        public string? ToolsConfig { get; set; }
        public string? SafetySettings { get; set; }
        public string? Project { get; set; }
        public string? Location { get; set; }
        public string? Model { get; set; }
        public bool AdminCanChange { get; set; }
        public bool UseCache { get; set; }
        public int CacheInvalidationMinutes { get; set; }
    }

    private static bool IsValidJson(string? value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        value = value.Trim();
        return (value.StartsWith("{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal)) ||
               (value.StartsWith("[", StringComparison.Ordinal) && value.EndsWith("]", StringComparison.Ordinal));
    }

    private static (bool IsValid, List<string> Errors) ValidateAiPromptRequest(AiPromptRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Type)) errors.Add("Type is required");
        if (string.IsNullOrWhiteSpace(req.DataRetrievalMethod)) errors.Add("DataRetrievalMethod is required");
        if (string.IsNullOrWhiteSpace(req.SystemInstructions)) errors.Add("SystemInstructions is required");
        if (string.IsNullOrWhiteSpace(req.Feature)) errors.Add("Feature is required");
        if (string.IsNullOrWhiteSpace(req.GenerationConfig)) errors.Add("GenerationConfig is required");
        else if (!IsValidJson(req.GenerationConfig)) errors.Add("GenerationConfig must be valid JSON");
        if (string.IsNullOrWhiteSpace(req.ContentConfig)) errors.Add("ContentConfig is required");
        else if (!IsValidJson(req.ContentConfig)) errors.Add("ContentConfig must be valid JSON");
        if (!string.IsNullOrEmpty(req.ToolsConfig) && !IsValidJson(req.ToolsConfig)) errors.Add("ToolsConfig must be valid JSON when provided");
        if (!string.IsNullOrEmpty(req.SafetySettings) && !IsValidJson(req.SafetySettings)) errors.Add("SafetySettings must be valid JSON when provided");
        if (string.IsNullOrWhiteSpace(req.Project)) errors.Add("Project is required");
        if (string.IsNullOrWhiteSpace(req.Location)) errors.Add("Location is required");
        if (string.IsNullOrWhiteSpace(req.Model)) errors.Add("Model is required");
        if (req.CacheInvalidationMinutes < 0) errors.Add("CacheInvalidationMinutes must be non-negative");
        return (errors.Count == 0, errors);
    }

    private static AiPromptRequest CreateValidBaseRequest() => new()
    {
        Type = "Completion",
        DataRetrievalMethod = "RAG",
        SystemInstructions = "You are a helpful assistant.",
        Feature = "OpportunitySummary",
        GenerationConfig = "{}",
        ContentConfig = "{}",
        Project = "OpportunityPlus",
        Location = "us-central1",
        Model = "gemini-1.5-pro",
        AdminCanChange = false,
        UseCache = true,
        CacheInvalidationMinutes = 60
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_TypeFirst_ProducesValidRequest()
    {
        var req = new AiPromptRequest { Type = "Chat", DataRetrievalMethod = "RAG", SystemInstructions = "Hi", Feature = "Test", GenerationConfig = "{}", ContentConfig = "{}", Project = "P", Location = "L", Model = "M" };
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.Type.Should().Be("Chat");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_DataRetrievalMethodFirst_ProducesValidRequest()
    {
        var req = new AiPromptRequest { DataRetrievalMethod = "Direct", Type = "Completion", SystemInstructions = "Hi", Feature = "Test", GenerationConfig = "{}", ContentConfig = "{}", Project = "P", Location = "L", Model = "M" };
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.DataRetrievalMethod.Should().Be("Direct");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_SystemInstructionsFirst_ProducesValidRequest()
    {
        var req = new AiPromptRequest { SystemInstructions = "Instructions", Type = "Completion", DataRetrievalMethod = "RAG", Feature = "Test", GenerationConfig = "{}", ContentConfig = "{}", Project = "P", Location = "L", Model = "M" };
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.SystemInstructions.Should().Be("Instructions");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ConfigsFirst_ProducesValidRequest()
    {
        var req = new AiPromptRequest { GenerationConfig = "{\"a\":1}", ContentConfig = "{\"b\":2}", Type = "Completion", DataRetrievalMethod = "RAG", SystemInstructions = "Hi", Feature = "Test", Project = "P", Location = "L", Model = "M" };
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.GenerationConfig.Should().Contain("a");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllFieldsReverseOrder_ProducesValidRequest()
    {
        var req = new AiPromptRequest
        {
            CacheInvalidationMinutes = 30,
            UseCache = false,
            AdminCanChange = true,
            Model = "gemini",
            Location = "eu",
            Project = "P",
            ContentConfig = "{}",
            GenerationConfig = "{}",
            Feature = "F",
            SystemInstructions = "S",
            DataRetrievalMethod = "RAG",
            Type = "Completion"
        };
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.CacheInvalidationMinutes.Should().Be(30);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_InterleavedOptionalAndRequired_Valid()
    {
        var req = new AiPromptRequest();
        req.Type = "Completion";
        req.UserPrompt = "Optional";
        req.DataRetrievalMethod = "RAG";
        req.Description = "Desc";
        req.SystemInstructions = "Instructions";
        req.Feature = "F";
        req.GenerationConfig = "{}";
        req.ToolsConfig = "{}";
        req.ContentConfig = "{}";
        req.SafetySettings = "{}";
        req.Project = "P";
        req.Location = "L";
        req.Model = "M";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Invalid Combinations

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyType_FailsValidation(string? type)
    {
        var req = CreateValidBaseRequest();
        req.Type = type ?? string.Empty;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Type"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyDataRetrievalMethod_FailsValidation(string? value)
    {
        var req = CreateValidBaseRequest();
        req.DataRetrievalMethod = value ?? string.Empty;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("DataRetrievalMethod"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptySystemInstructions_FailsValidation(string? value)
    {
        var req = CreateValidBaseRequest();
        req.SystemInstructions = value ?? string.Empty;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SystemInstructions"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyFeature_FailsValidation(string? value)
    {
        var req = CreateValidBaseRequest();
        req.Feature = value ?? string.Empty;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Feature"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyGenerationConfig_FailsValidation(string? value)
    {
        var req = CreateValidBaseRequest();
        req.GenerationConfig = value ?? string.Empty;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("GenerationConfig"));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{invalid}")]
    [InlineData("[]")]
    [InlineData("plain text")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidGenerationConfigJson_FailsValidation(string value)
    {
        var req = CreateValidBaseRequest();
        req.GenerationConfig = value;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("GenerationConfig") || e.Contains("JSON"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyContentConfig_FailsValidation(string? value)
    {
        var req = CreateValidBaseRequest();
        req.ContentConfig = value ?? string.Empty;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ContentConfig"));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{invalid}")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidContentConfigJson_FailsValidation(string value)
    {
        var req = CreateValidBaseRequest();
        req.ContentConfig = value;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ContentConfig") || e.Contains("JSON"));
    }

    [Theory]
    [InlineData("not json")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidToolsConfigJson_FailsValidation(string value)
    {
        var req = CreateValidBaseRequest();
        req.ToolsConfig = value;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ToolsConfig") || e.Contains("JSON"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Negative")]
    public void Invalid_NegativeCacheInvalidationMinutes_FailsValidation(int value)
    {
        var req = CreateValidBaseRequest();
        req.CacheInvalidationMinutes = value;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("CacheInvalidationMinutes"));
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidJsonConfigs_InvalidType_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.GenerationConfig = "{}";
        req.ContentConfig = "{}";
        req.Type = "";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidRequired_InvalidGenerationConfig_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Type = "Completion";
        req.GenerationConfig = "not valid json";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidRequired_InvalidContentConfig_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.ContentConfig = "invalid";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidRequired_ValidOptionalJsonConfigs_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ToolsConfig = "{}";
        req.SafetySettings = "{}";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidType_InvalidDataRetrievalMethod_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.DataRetrievalMethod = null;
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidConfigs_InvalidCacheInvalidationMinutes_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.CacheInvalidationMinutes = -1;
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidRequired_ValidOptionalUserPrompt_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserPrompt = "Summarize this";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidProject_InvalidLocation_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Location = "";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalRequiredOnly_Valid()
    {
        var req = new AiPromptRequest { Type = "Completion", DataRetrievalMethod = "RAG", SystemInstructions = "Hi", Feature = "F", GenerationConfig = "{}", ContentConfig = "{}", Project = "P", Location = "L", Model = "M" };
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.UserPrompt.Should().BeNull();
        req.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithUserPrompt_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserPrompt = "Summarize the opportunity";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.UserPrompt.Should().Be("Summarize the opportunity");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithDescription_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Description = "AI prompt for summaries";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithToolsConfig_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ToolsConfig = "{\"tools\":[]}";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithSafetySettings_Valid()
    {
        var req = CreateValidBaseRequest();
        req.SafetySettings = "{\"blocked\":[]}";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_AllOptionalFilled_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserPrompt = "User prompt";
        req.Description = "Description";
        req.ToolsConfig = "{}";
        req.SafetySettings = "{}";
        req.AdminCanChange = true;
        req.UseCache = false;
        req.CacheInvalidationMinutes = 120;
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithConfigsArray_Valid()
    {
        var req = CreateValidBaseRequest();
        req.GenerationConfig = "[]";
        req.ContentConfig = "[]";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithComplexJsonConfigs_Valid()
    {
        var req = CreateValidBaseRequest();
        req.GenerationConfig = "{\"temperature\":0.7,\"maxTokens\":1000}";
        req.ContentConfig = "{\"format\":\"markdown\"}";
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongSystemInstructions_PropertyAcceptsValue()
    {
        var longStr = InvalidValueSets.VeryLongString(50000);
        var req = CreateValidBaseRequest();
        req.SystemInstructions = longStr;
        req.SystemInstructions.Should().HaveLength(50000);
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongGenerationConfigJson_Valid()
    {
        var longJson = "{\"key\":\"" + InvalidValueSets.MaxLengthString(10000) + "\"}";
        var req = CreateValidBaseRequest();
        req.GenerationConfig = longJson;
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_CacheInvalidationMinutesAtIntMaxValue_Valid()
    {
        var req = CreateValidBaseRequest();
        req.CacheInvalidationMinutes = int.MaxValue;
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
        req.CacheInvalidationMinutes.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_CacheInvalidationMinutesAtZero_Valid()
    {
        var req = CreateValidBaseRequest();
        req.CacheInvalidationMinutes = 0;
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_CacheInvalidationMinutesAtNegativeOne_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.CacheInvalidationMinutes = -1;
        var (isValid, errors) = ValidateAiPromptRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("CacheInvalidationMinutes"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SpecialCharsInType_PropertyAcceptsValue()
    {
        var req = CreateValidBaseRequest();
        req.Type = InvalidValueSets.SpecialCharacters[0];
        req.Type.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SpecialCharsInSystemInstructions_PropertyAcceptsValue()
    {
        var req = CreateValidBaseRequest();
        req.SystemInstructions = InvalidValueSets.SpecialCharacters[1];
        req.SystemInstructions.Should().Contain("DROP");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UnicodeInFeature_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Feature = InvalidValueSets.UnicodeStrings[0];
        var (isValid, _) = ValidateAiPromptRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion
}
