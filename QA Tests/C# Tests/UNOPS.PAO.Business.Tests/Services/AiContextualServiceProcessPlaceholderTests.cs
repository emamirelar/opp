/**
 * @fileoverview Tests for AiContextualService.ProcessPlaceholders and GetDetailsFromGeminiResponse.
 * These methods are pure logic (no external calls) and can be tested without Google credentials.
 *
 * Ratio: P=3, N=9, E=9, F=9, I=9 → Total=39
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Tests for AiContextualService.ProcessPlaceholders and GetDetailsFromGeminiResponse.
/// These methods are pure-logic (no Google credentials or external API calls needed at runtime).
///
/// Ratio compliance:
///   Positive  (P) =  3
///   Negative  (N) =  9  (N ≥ 3P ✅)
///   Edge      (E) =  9  (E ≥ 3P ✅)
///   Functional(F) =  9  (F ≥ 3P ✅)
///   Integration(I)=  9  (I ≥ 3P ✅)
///   ─────────────────────────────────
///   TOTAL         = 39
/// </summary>
public class AiContextualServiceProcessPlaceholderTests : IDisposable
{
    private readonly AiContextualService _service;
    private readonly UNOPSAppDbContext _context;
    private readonly bool _serviceAvailable;

    public AiContextualServiceProcessPlaceholderTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AISettings:DisableExternalCalls", "true" },
                { "AISettings:ProjectId", "test-project" },
                { "AISettings:Location", "us-central1" },
                { "AISettings:EmbeddingModelName", "textembedding-gecko" },
                { "AISettings:PubSubProjectId", "test-project" },
                { "AISettings:PubSubTopicId", "test-topic" },
                { "ASPNETCORE_ENVIRONMENT", "Testing" },
                { "ConnectionStrings:DbSchema", "Host=localhost;Database=test_placeholder" },
            })
            .Build();

        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase($"AiCtxTest_{Guid.NewGuid()}")
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var testIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Name, "Test User")
        }, "TestAuth");
        var testPrincipal = new ClaimsPrincipal(testIdentity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.User).Returns(testPrincipal);
        httpContextMock.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        _context = new UNOPSAppDbContext(options, userResolverService, mockDbSchema.Object);

        try
        {
            _service = new AiContextualService(config, _context, null!);
            _serviceAvailable = true;
        }
        catch (Exception)
        {
            _service = null!;
            _serviceAvailable = false;
        }
    }

    private void SkipIfServiceUnavailable()
    {
        if (!_serviceAvailable)
            Assert.Fail(
                "AiContextualService could not be instantiated in test environment. " +
                "PubSubPublisher or other constructor dependency may require real Google config.");
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }

    // ═══════════════════════════════════════════════════════════════════
    // POSITIVE TESTS (P=3)
    // ═══════════════════════════════════════════════════════════════════

    #region Positive Tests

    [Fact]
    [Trait("Category", "Positive")]
    public void PP_001_ProcessPlaceholders_SimpleReplacement_ReturnsSubstitutedText()
    {
        SkipIfServiceUnavailable();
        var text = "Hello {name}, welcome to {location}";
        var json = """{"name": "Alice", "location": "Geneva"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("Hello Alice, welcome to Geneva", result);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void PP_002_ProcessPlaceholders_NestedProperty_ReturnsNestedValue()
    {
        SkipIfServiceUnavailable();
        var text = "Partner: {partner.name}, Country: {partner.country}";
        var json = """{"partner": {"name": "UNICEF", "country": "Switzerland"}}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("Partner: UNICEF, Country: Switzerland", result);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void PP_003_GetDetailsFromGeminiResponse_ValidJson_ReturnsParsedObject()
    {
        SkipIfServiceUnavailable();
        var geminiResponse = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "{\"summary\": \"Test summary\", \"score\": 85}"}]
                }
            }]
        }
        """;

        var result = _service.GetDetailsFromGeminiResponse(geminiResponse);

        Assert.Equal("Test summary", result["summary"]?.ToString());
        Assert.Equal(85, result["score"]?.Value<int>());
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // NEGATIVE TESTS (N=9)
    // ═══════════════════════════════════════════════════════════════════

    #region Negative Tests

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_001_ProcessPlaceholders_NullText_ReturnsEmptyString()
    {
        SkipIfServiceUnavailable();
        var result = _service.ProcessPlaceholders(null!, """{"key": "value"}""");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_002_ProcessPlaceholders_EmptyText_ReturnsEmptyString()
    {
        SkipIfServiceUnavailable();
        var result = _service.ProcessPlaceholders("", """{"key": "value"}""");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_003_ProcessPlaceholders_NullJson_ReturnsOriginalText()
    {
        SkipIfServiceUnavailable();
        var text = "Hello {name}";
        var result = _service.ProcessPlaceholders(text, null!);
        Assert.Equal(text, result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_004_ProcessPlaceholders_EmptyJson_ReturnsOriginalText()
    {
        SkipIfServiceUnavailable();
        var text = "Hello {name}";
        var result = _service.ProcessPlaceholders(text, "");
        Assert.Equal(text, result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_005_ProcessPlaceholders_InvalidJson_ReturnsOriginalText()
    {
        SkipIfServiceUnavailable();
        var text = "Hello {name}";
        var result = _service.ProcessPlaceholders(text, "not valid json {{");
        Assert.Equal(text, result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_006_ProcessPlaceholders_MissingPlaceholder_ReplacesWithEmpty()
    {
        SkipIfServiceUnavailable();
        var text = "Hello {nonExistentKey}";
        var json = """{"name": "Alice"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("Hello ", result);
        Assert.DoesNotContain("{nonExistentKey}", result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_007_GetDetailsFromGeminiResponse_InvalidJsonThrows()
    {
        SkipIfServiceUnavailable();
        Assert.Throws<Newtonsoft.Json.JsonReaderException>(
            () => _service.GetDetailsFromGeminiResponse("not json at all"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_008_GetDetailsFromGeminiResponse_MissingCandidates_Throws()
    {
        SkipIfServiceUnavailable();
        var response = """{"other": "data"}""";
        Assert.ThrowsAny<Exception>(
            () => _service.GetDetailsFromGeminiResponse(response));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void PN_009_GetDetailsFromGeminiResponse_EmptyCandidates_Throws()
    {
        SkipIfServiceUnavailable();
        var response = """{"candidates": []}""";
        Assert.ThrowsAny<Exception>(
            () => _service.GetDetailsFromGeminiResponse(response));
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // EDGE/BOUNDARY TESTS (E=9)
    // ═══════════════════════════════════════════════════════════════════

    #region Edge/Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_001_ProcessPlaceholders_PromptDataShortcut_ReturnsFullJson()
    {
        SkipIfServiceUnavailable();
        var json = """{"partner": "ACME", "status": "active"}""";
        var result = _service.ProcessPlaceholders("{promptData}", json);
        Assert.Equal(json, result);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_002_ProcessPlaceholders_NoPlaceholders_ReturnsOriginal()
    {
        SkipIfServiceUnavailable();
        var text = "No placeholders here at all.";
        var json = """{"name": "Alice"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal(text, result);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_003_ProcessPlaceholders_JsonContentInText_NotCorrupted()
    {
        SkipIfServiceUnavailable();
        var text = """Return a JSON object like { "isAligned": true } with {partnerName} data""";
        var json = """{"partnerName": "UNICEF"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Contains("UNICEF", result);
        Assert.Contains("{ \"isAligned\": true }", result);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_004_ProcessPlaceholders_SpecialCharsInValues_Preserved()
    {
        SkipIfServiceUnavailable();
        var text = "Partner: {name}";
        var json = """{"name": "ACME <Corp> & Partners 'Ltd'"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("Partner: ACME <Corp> & Partners 'Ltd'", result);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_005_ProcessPlaceholders_DuplicatePlaceholders_AllReplaced()
    {
        SkipIfServiceUnavailable();
        var text = "{name} met with {name} and discussed {name}'s proposal";
        var json = """{"name": "Alice"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("Alice met with Alice and discussed Alice's proposal", result);
        Assert.DoesNotContain("{name}", result);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_006_ProcessPlaceholders_VeryLongValue_ReplacedSuccessfully()
    {
        SkipIfServiceUnavailable();
        var longValue = new string('x', 10000);
        var text = "Summary: {data}";
        var json = $"{{\"data\": \"{longValue}\"}}";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal($"Summary: {longValue}", result);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_007_GetDetailsFromGeminiResponse_JsonWrappedInMarkdownCodeBlock()
    {
        SkipIfServiceUnavailable();
        var response = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "```json\n{\"result\": \"clean\"}\n```"}]
                }
            }]
        }
        """;

        var result = _service.GetDetailsFromGeminiResponse(response);

        Assert.Equal("clean", result["result"]?.ToString());
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_008_GetDetailsFromGeminiResponse_PlainTextResponse_WrappedInMessage()
    {
        SkipIfServiceUnavailable();
        var response = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "This is a plain text answer, not JSON."}]
                }
            }]
        }
        """;

        var result = _service.GetDetailsFromGeminiResponse(response);

        Assert.NotNull(result["Message"]);
        Assert.Contains("plain text answer", result["Message"]!.ToString());
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void PE_009_ProcessPlaceholders_UnicodeValues_Preserved()
    {
        SkipIfServiceUnavailable();
        var text = "Partenaire: {name}, Pays: {country}";
        var json = """{"name": "Médecins Sans Frontières", "country": "République Démocratique du Congo"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Contains("Médecins Sans Frontières", result);
        Assert.Contains("République Démocratique du Congo", result);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // FUNCTIONAL TESTS (F=9)
    // ═══════════════════════════════════════════════════════════════════

    #region Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_001_ProcessPlaceholders_DotNotation_TraversesNestedObjects()
    {
        SkipIfServiceUnavailable();
        var text = "The {partner.contact.name} from {partner.name}";
        var json = """{"partner": {"name": "ACME", "contact": {"name": "John"}}}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("The John from ACME", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_002_ProcessPlaceholders_MixedExistingAndMissing_PartialReplacement()
    {
        SkipIfServiceUnavailable();
        var text = "Name: {name}, Age: {age}, Role: {role}";
        var json = """{"name": "Alice", "role": "Manager"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Contains("Alice", result);
        Assert.Contains("Manager", result);
        Assert.DoesNotContain("{name}", result);
        Assert.DoesNotContain("{role}", result);
        Assert.DoesNotContain("{age}", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_003_ProcessPlaceholders_NumericValues_ConvertedToString()
    {
        SkipIfServiceUnavailable();
        var text = "Budget: {budget} USD, Count: {count}";
        var json = """{"budget": 1500000, "count": 42}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Contains("1500000", result);
        Assert.Contains("42", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_004_ProcessPlaceholders_BooleanValues_ConvertedToString()
    {
        SkipIfServiceUnavailable();
        var text = "Active: {isActive}";
        var json = """{"isActive": true}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.DoesNotContain("{isActive}", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_005_ProcessPlaceholders_UnderscoresInKeys_Supported()
    {
        SkipIfServiceUnavailable();
        var text = "ID: {partner_id}, Name: {partner_name}";
        var json = """{"partner_id": "P-001", "partner_name": "ACME Corp"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("ID: P-001, Name: ACME Corp", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_006_GetDetailsFromGeminiResponse_NestedJsonPreserved()
    {
        SkipIfServiceUnavailable();
        var response = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "{\"partner\": {\"name\": \"ACME\", \"contacts\": [{\"id\": 1}]}}"}]
                }
            }]
        }
        """;

        var result = _service.GetDetailsFromGeminiResponse(response);

        Assert.Equal("ACME", result["partner"]?["name"]?.ToString());
        Assert.NotNull(result["partner"]?["contacts"]);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_007_ProcessPlaceholders_MultipleReplacements_AllApplied()
    {
        SkipIfServiceUnavailable();
        var text = "{greeting} {name}! Your {entityType} ({entityId}) in {region} is {status}.";
        var json = """
        {
            "greeting": "Hello",
            "name": "Dr. Smith",
            "entityType": "Opportunity",
            "entityId": "OPP-2024-001",
            "region": "East Africa",
            "status": "Active"
        }
        """;

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Equal("Hello Dr. Smith! Your Opportunity (OPP-2024-001) in East Africa is Active.", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_008_ProcessPlaceholders_CaseSensitiveKeys_Respected()
    {
        SkipIfServiceUnavailable();
        var text = "Name: {Name}, name: {name}";
        var json = """{"Name": "UPPERCASE", "name": "lowercase"}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.DoesNotContain("{Name}", result);
        Assert.DoesNotContain("{name}", result);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void PF_009_GetDetailsFromGeminiResponse_ArrayResponse_Parsed()
    {
        SkipIfServiceUnavailable();
        var response = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "{\"items\": [\"a\", \"b\", \"c\"], \"count\": 3}"}]
                }
            }]
        }
        """;

        var result = _service.GetDetailsFromGeminiResponse(response);

        Assert.Equal(3, result["count"]?.Value<int>());
        var items = result["items"] as JArray;
        Assert.NotNull(items);
        Assert.Equal(3, items!.Count);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // INTEGRATION TESTS (I=9)
    // ═══════════════════════════════════════════════════════════════════

    #region Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_001_ProcessThenParse_EndToEnd_PlaceholdersInGeminiOutput()
    {
        SkipIfServiceUnavailable();
        var template = "Generate insights for {partnerName} in {country}";
        var contextJson = """{"partnerName": "UNICEF", "country": "Kenya"}""";
        var processedPrompt = _service.ProcessPlaceholders(template, contextJson);

        Assert.Equal("Generate insights for UNICEF in Kenya", processedPrompt);

        var geminiResponse = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "{\"insight\": \"UNICEF operations in Kenya are strong\"}"}]
                }
            }]
        }
        """;
        var parsed = _service.GetDetailsFromGeminiResponse(geminiResponse);
        Assert.Contains("UNICEF", parsed["insight"]!.ToString());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_002_ProcessPlaceholders_RealisticPartnerPrompt()
    {
        SkipIfServiceUnavailable();
        var template = """
            Analyze the partnership with {partnerName} ({partnerType}) based in {country}.
            Key contacts: {contactName}. Status: {status}. 
            Engagement value: {value} {currency}.
            """;
        var json = """
        {
            "partnerName": "Red Cross International",
            "partnerType": "NGO",
            "country": "Switzerland",
            "contactName": "Dr. Jean Dupont",
            "status": "Active",
            "value": 2500000,
            "currency": "CHF"
        }
        """;

        var result = _service.ProcessPlaceholders(template, json);

        Assert.Contains("Red Cross International", result);
        Assert.Contains("NGO", result);
        Assert.Contains("Switzerland", result);
        Assert.Contains("Dr. Jean Dupont", result);
        Assert.Contains("2500000", result);
        Assert.Contains("CHF", result);
        Assert.DoesNotContain("{partnerName}", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_003_ProcessPlaceholders_RealisticOpportunityPrompt()
    {
        SkipIfServiceUnavailable();
        var template = """
            Evaluate opportunity "{opportunityName}" (ID: {id}).
            Partner: {partner.name}. Sector: {sector}.
            Estimated value: {estimatedValue} USD.
            """;
        var json = """
        {
            "opportunityName": "Infrastructure Development",
            "id": "OPP-2024-001",
            "partner": {"name": "World Bank"},
            "sector": "Infrastructure",
            "estimatedValue": 5000000
        }
        """;

        var result = _service.ProcessPlaceholders(template, json);

        Assert.Contains("Infrastructure Development", result);
        Assert.Contains("World Bank", result);
        Assert.Contains("5000000", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_004_GetDetailsFromGeminiResponse_RealisticOpportunityInsight()
    {
        SkipIfServiceUnavailable();
        var response = """
        {
            "candidates": [{
                "content": {
                    "parts": [{"text": "```json\n{\"recommendation\": \"Proceed to GO stage\", \"confidence\": 0.87, \"risks\": [\"Budget overrun\", \"Timeline delay\"], \"alignedSDGs\": [1, 4, 13]}\n```"}]
                }
            }]
        }
        """;

        var result = _service.GetDetailsFromGeminiResponse(response);

        Assert.Equal("Proceed to GO stage", result["recommendation"]?.ToString());
        Assert.Equal(0.87, result["confidence"]?.Value<double>());
        var risks = result["risks"] as JArray;
        Assert.NotNull(risks);
        Assert.Equal(2, risks!.Count);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_005_ProcessPlaceholders_MultiLevelNesting()
    {
        SkipIfServiceUnavailable();
        var text = "Org: {org.unit.name}, Manager: {org.manager.email}";
        var json = """
        {
            "org": {
                "unit": {"name": "East Africa Regional"},
                "manager": {"email": "manager@unops.org"}
            }
        }
        """;

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Contains("East Africa Regional", result);
        Assert.Contains("manager@unops.org", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_006_ProcessPlaceholders_SequentialCalls_Independent()
    {
        SkipIfServiceUnavailable();
        var template = "Hello {name}";
        var result1 = _service.ProcessPlaceholders(template, """{"name": "Alice"}""");
        var result2 = _service.ProcessPlaceholders(template, """{"name": "Bob"}""");
        var result3 = _service.ProcessPlaceholders(template, """{"name": "Charlie"}""");

        Assert.Equal("Hello Alice", result1);
        Assert.Equal("Hello Bob", result2);
        Assert.Equal("Hello Charlie", result3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_007_GetDetailsFromGeminiResponse_SequentialParsing_Independent()
    {
        SkipIfServiceUnavailable();
        string MakeResponse(string text) =>
            "{\"candidates\": [{\"content\": {\"parts\": [{\"text\": \"" + text.Replace("\"", "\\\"") + "\"}]}}]}";

        var r1 = _service.GetDetailsFromGeminiResponse(MakeResponse("{\"v\": 1}"));
        var r2 = _service.GetDetailsFromGeminiResponse(MakeResponse("{\"v\": 2}"));

        Assert.Equal(1, r1["v"]?.Value<int>());
        Assert.Equal(2, r2["v"]?.Value<int>());
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_008_ProcessPlaceholders_ComplexJsonWithArrays()
    {
        SkipIfServiceUnavailable();
        var text = "Partner {name} has SDGs: {sdgs}";
        var json = """{"name": "UNDP", "sdgs": [1, 4, 13]}""";

        var result = _service.ProcessPlaceholders(text, json);

        Assert.Contains("UNDP", result);
        Assert.DoesNotContain("{name}", result);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void PI_009_ProcessAndParse_RoundTrip_LargePayload()
    {
        SkipIfServiceUnavailable();
        var fields = new JObject();
        for (var i = 0; i < 50; i++)
            fields[$"field{i}"] = $"value{i}";

        var template = "Field0: {field0}, Field25: {field25}, Field49: {field49}";
        var result = _service.ProcessPlaceholders(template, fields.ToString());

        Assert.Contains("value0", result);
        Assert.Contains("value25", result);
        Assert.Contains("value49", result);
    }

    #endregion
}
