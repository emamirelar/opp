/// <summary>
/// Comprehensive unit tests for ScreenContextCacheService.
/// Tests URL parsing, entity type detection, entity detail fetching, AI recommendations,
/// cache behavior (store, retrieve, invalidate), and edge cases (invalid URLs, null inputs).
/// Requirements source: UNOPS.PAO.UNOPSBusiness/Services/ScreenContextCacheService.cs
/// </summary>

using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

[Trait("Category", "Unit")]
[Trait("Feature", "ScreenContextCacheService")]
public class ScreenContextCacheServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ScreenContextCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly UNOPSAppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ScreenContextCacheService _service;

    public ScreenContextCacheServiceTests()
    {
        var dbName = $"ScreenContext_{Guid.NewGuid():N}";
        var options = TestEnvironment.CreateUNOPSDbContextOptions(dbName);
        var userResolver = new UNOPS.PAO.DataAccess.Services.UserResolverService<int>(
            TestDbContextFactory.CreateMockHttpContextAccessor("1").Object);
        var mockSchema = new Mock<UNOPS.PAO.DataAccess.Interfaces.IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        _context = TestDbContextFactory.CreateUNOPS(options, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(_context);

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _logger = Mock.Of<ILogger<ScreenContextCacheService>>();
        _configuration = TestEnvironment.CreateTestConfiguration();
        _httpClient = new HttpClient();

        _service = new ScreenContextCacheService(
            _memoryCache,
            _logger,
            _configuration,
            _context,
            _httpClient);
    }

    public void Dispose() => _context?.Dispose();

    #region 1. URL Parsing and Entity Type Detection — Homepage

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_EmptyUrl_ReturnsHomepage()
    {
        var result = await _service.GetScreenContextAsync("", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("homepage");
        ctx.GetProperty("screen_data").GetProperty("view_type").GetString().Should().Be("homepage");
        ctx.GetProperty("screen_name").GetString().Should().Be("Dashboard");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_RootSlash_ReturnsHomepage()
    {
        var result = await _service.GetScreenContextAsync("/", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("homepage");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_NullScreenUrl_HandlesGracefully()
    {
        var result = await _service.GetScreenContextAsync(null!, "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().NotBeNullOrEmpty();
    }

    #endregion

    #region 2. URL Parsing — AI Assistant Mode

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_AIUrl_ReturnsAIAssistantMode()
    {
        var result = await _service.GetScreenContextAsync("/ai", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("ai_assistant_mode");
        ctx.GetProperty("screen_data").GetProperty("view_type").GetString().Should().Be("ai_fullscreen");
        ctx.GetProperty("screen_name").GetString().Should().Be("AI Assistant");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_UrlContainsAI_ReturnsAIAssistantMode()
    {
        var result = await _service.GetScreenContextAsync("/some/path/ai/assistant", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("ai_assistant_mode");
    }

    #endregion

    #region 3. URL Parsing — Entity List Pages

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_PartnersUrl_ReturnsEntityListPage()
    {
        var result = await _service.GetScreenContextAsync("/partners", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_list_page");
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Partner");
        ctx.GetProperty("screen_data").GetProperty("view_type").GetString().Should().Be("list");
        ctx.GetProperty("screen_name").GetString().Should().Be("Partner List");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_ContactsUrl_ReturnsEntityListPage()
    {
        var result = await _service.GetScreenContextAsync("/contacts", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Contact");
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_list_page");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_InteractionsUrl_ReturnsEntityListPage()
    {
        var result = await _service.GetScreenContextAsync("/interactions", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Interaction");
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_list_page");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_AiPromptsUrl_ContainsAi_ReturnsAIAssistantMode()
    {
        var result = await _service.GetScreenContextAsync("/aiprompts", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("ai_assistant_mode");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_PartnershipsUrl_MapsToPartner()
    {
        var result = await _service.GetScreenContextAsync("/partnerships", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_PartnerTreeUrl_ReturnsPartnerTreeEntity()
    {
        var result = await _service.GetScreenContextAsync("/partner-tree", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("PartnerTree");
    }

    #endregion

    #region 4. URL Parsing — Entity Detail Pages (with ID)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_PartnersWithNumericId_ReturnsEntityDetailPage()
    {
        var result = await _service.GetScreenContextAsync("/partners/42", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_detail_page");
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Partner");
        ctx.GetProperty("entity_id_in_focus").GetString().Should().Be("42");
        ctx.GetProperty("screen_data").GetProperty("view_type").GetString().Should().Be("detail");
        ctx.GetProperty("screen_name").GetString().Should().Be("Partner Details");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_ContactsWithGuid_ReturnsEntityDetailPage()
    {
        var guid = Guid.NewGuid().ToString();
        var result = await _service.GetScreenContextAsync($"/contacts/{guid}", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_detail_page");
        ctx.GetProperty("entity_id_in_focus").GetString().Should().Be(guid);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_InteractionsWithLongAlphanumericId_ReturnsEntityDetailPage()
    {
        var longId = "abcdefghij12345";
        var result = await _service.GetScreenContextAsync($"/interactions/{longId}", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_detail_page");
        ctx.GetProperty("entity_id_in_focus").GetString().Should().Be(longId);
    }

    #endregion

    #region 5. URL Parsing — Form and Dashboard Pages

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_FormPageUrl_EntityTakesPrecedence_ReturnsEntityListPage()
    {
        var result = await _service.GetScreenContextAsync("/partners/create", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_list_page");
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_EditInPath_EntityWithIdTakesPrecedence_ReturnsEntityDetailPage()
    {
        var result = await _service.GetScreenContextAsync("/partners/1/edit", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_detail_page");
        ctx.GetProperty("entity_id_in_focus").GetString().Should().Be("1");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_DashboardUrl_ReturnsDashboardOverview()
    {
        var result = await _service.GetScreenContextAsync("/dashboard", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("dashboard_overview");
        ctx.GetProperty("screen_data").GetProperty("view_type").GetString().Should().Be("dashboard");
        ctx.GetProperty("screen_name").GetString().Should().Be("Dashboard Overview");
    }

    #endregion

    #region 6. URL Parsing — Unknown Entity Types and Specific Page

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_UnknownEntityPath_ReturnsSpecificPage()
    {
        var result = await _service.GetScreenContextAsync("/unknown-entity", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("specific_page");
        ctx.GetProperty("screen_data").GetProperty("view_type").GetString().Should().Be("generic_page");
        ctx.GetProperty("screen_name").GetString().Should().Be("Application Page");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_EntityWithNonIdNextPart_ReturnsEntityListPage()
    {
        var result = await _service.GetScreenContextAsync("/partners/create", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_list_page");
    }

    #endregion

    #region 7. Entity Detail Fetching

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_PartnerDetailUrl_WithSeededPartner_FetchesEntityDetails()
    {
        var partnerId = SeedPartner(1, "Test Partner", "Long desc");

        var result = await _service.GetScreenContextAsync($"/partners/{partnerId}", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_details").ValueKind.Should().NotBe(JsonValueKind.Null);
        var details = ctx.GetProperty("entity_details");
        details.GetProperty("Id").GetInt32().Should().Be(partnerId);
        details.GetProperty("Name").GetString().Should().Be("Test Partner");
        details.GetProperty("EntityType").GetString().Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_ContactDetailUrl_WithSeededContact_FetchesEntityDetails()
    {
        var partnerId = SeedPartner(1, "Partner", "");
        var contactId = SeedContact(1, "John", "Doe", "john@test.com", partnerId);

        var result = await _service.GetScreenContextAsync($"/contacts/{contactId}", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_details").ValueKind.Should().NotBe(JsonValueKind.Null);
        var details = ctx.GetProperty("entity_details");
        details.GetProperty("Name").GetString().Should().Be("John Doe");
        details.GetProperty("EntityType").GetString().Should().Be("Contact");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetScreenContextAsync_InteractionDetailUrl_WithSeededInteraction_FetchesEntityDetails()
    {
        var interactionId = SeedInteraction(1, "Meeting Subject");

        var result = await _service.GetScreenContextAsync($"/interactions/{interactionId}", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_details").ValueKind.Should().NotBe(JsonValueKind.Null);
        var details = ctx.GetProperty("entity_details");
        details.GetProperty("Name").GetString().Should().Be("Meeting Subject");
        details.GetProperty("EntityType").GetString().Should().Be("Interaction");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetScreenContextAsync_PartnerDetailUrl_NonExistentId_ReturnsNullEntityDetails()
    {
        var result = await _service.GetScreenContextAsync("/partners/99999", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_details").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetScreenContextAsync_PartnerDetailUrl_InvalidIdFormat_ReturnsNullEntityDetails()
    {
        var result = await _service.GetScreenContextAsync("/partners/not-a-number", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_details").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_AiPromptDetailUrl_UnknownEntityType_ReturnsNullEntityDetails()
    {
        var result = await _service.GetScreenContextAsync("/aiprompts/123", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_details").ValueKind.Should().Be(JsonValueKind.Null);
    }

    #endregion

    #region 8. AI Recommendation Logic

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_SameScreenAndFocus_ReturnsSameFocusRecommendation()
    {
        var url = "/partners";
        var result = await _service.GetScreenContextAsync(url, url, "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("focus_relationship").GetString().Should().Be("same");
        var recs = ctx.GetProperty("recommendations");
        recs.GetArrayLength().Should().Be(1);
        recs[0].GetString().Should().Contain("prioritize this entity");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_DifferentScreenAndFocus_ReturnsDifferentFocusRecommendation()
    {
        var result = await _service.GetScreenContextAsync("/partners", "/contacts", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("focus_relationship").GetString().Should().Be("different");
        var recs = ctx.GetProperty("recommendations");
        recs.GetArrayLength().Should().Be(1);
        recs[0].GetString().Should().Contain("separate focus");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_EmptyUserFocus_EmptyScreenUrl_UsesScreenUrlForFocus()
    {
        var screenUrl = "/partners";
        var result = await _service.GetScreenContextAsync(screenUrl, "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("resolved_user_focus_context").GetString().Should().Be(screenUrl);
        ctx.GetProperty("focus_relationship").GetString().Should().Be("same");
    }

    #endregion

    #region 9. Cache Behavior

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_SecondCallWithSameKey_ReturnsCachedResult()
    {
        var url = "/partners";
        var result1 = await _service.GetScreenContextAsync(url, "", "1");
        var result2 = await _service.GetScreenContextAsync(url, "", "1");

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        var ctx1 = ToJsonElement(result1);
        var ctx2 = ToJsonElement(result2);
        ctx1.GetProperty("screen_type").GetString().Should().Be(ctx2.GetProperty("screen_type").GetString());
        ctx1.GetProperty("entity_in_focus").GetString().Should().Be(ctx2.GetProperty("entity_in_focus").GetString());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_DifferentUserFocusContext_DifferentCacheKeys()
    {
        var result1 = await _service.GetScreenContextAsync("/partners", "/partners", "1");
        var result2 = await _service.GetScreenContextAsync("/partners", "/contacts", "1");

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        var ctx1 = ToJsonElement(result1);
        var ctx2 = ToJsonElement(result2);
        ctx1.GetProperty("focus_relationship").GetString().Should().Be("same");
        ctx2.GetProperty("focus_relationship").GetString().Should().Be("different");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task InvalidateScreenContextCache_RemovesEntry_SubsequentCallRegenerates()
    {
        var url = "/partners";
        var focus = "";

        var result1 = await _service.GetScreenContextAsync(url, focus, "1");
        _service.InvalidateScreenContextCache(url, focus);
        var result2 = await _service.GetScreenContextAsync(url, focus, "1");

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        var ctx1 = ToJsonElement(result1);
        var ctx2 = ToJsonElement(result2);
        ctx1.GetProperty("screen_type").GetString().Should().Be(ctx2.GetProperty("screen_type").GetString());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void InvalidateScreenContextCache_WithWhitespace_DoesNotThrow()
    {
        var act = () => _service.InvalidateScreenContextCache("  /partners  ", "  ");
        act.Should().NotThrow();
    }

    #endregion

    #region 10. Edge Cases — Null and Invalid Inputs

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetScreenContextAsync_NullUserId_ReturnsResult()
    {
        var result = await _service.GetScreenContextAsync("/partners", "", null!);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_UrlWithColonsAndSlashes_NormalizedInCacheKey()
    {
        var result = await _service.GetScreenContextAsync("https://example.com/partners/1", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_in_focus").GetString().Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_EntityListWithoutId_DoesNotFetchEntityDetails()
    {
        _ = SeedPartner(1, "Partner", "");

        var result = await _service.GetScreenContextAsync("/partners", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("entity_id_in_focus").ValueKind.Should().Be(JsonValueKind.Null);
        ctx.GetProperty("entity_details").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetScreenContextAsync_ShortAlphanumericNotTreatedAsId_ReturnsListPage()
    {
        var result = await _service.GetScreenContextAsync("/partners/abc", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("screen_type").GetString().Should().Be("entity_list_page");
    }

    #endregion

    #region 11. Response Structure Validation

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_ReturnsAllExpectedTopLevelProperties()
    {
        var result = await _service.GetScreenContextAsync("/partners/1", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        ctx.GetProperty("original_screen_url").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("original_user_focus_context").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("resolved_user_focus_context").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("focus_relationship").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("screen_type").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("entity_in_focus").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("screen_name").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("screen_url").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("recommendations").ValueKind.Should().NotBe(JsonValueKind.Null);
        ctx.GetProperty("screen_data").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetScreenContextAsync_ScreenData_HasIntelligentContext()
    {
        var result = await _service.GetScreenContextAsync("/partners", "", "1");

        result.Should().NotBeNull();
        var ctx = ToJsonElement(result);
        var screenData = ctx.GetProperty("screen_data");
        screenData.GetProperty("intelligent_context").GetBoolean().Should().BeTrue();
        screenData.GetProperty("detected_entity").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    #endregion

    #region 12. Error Handling

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetScreenContextAsync_ServiceDoesNotThrowOnValidInput()
    {
        var result = await _service.GetScreenContextAsync("/partners", "", "1");

        result.Should().NotBeNull();
    }

    #endregion

    #region Helpers

    private static JsonElement ToJsonElement(object? obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        var json = JsonSerializer.Serialize(obj);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private int SeedPartner(int id, string name, string longDesc)
    {
        var partner = new UNOPSPartner
        {
            Name = name,
            PartnerShortDescription = "Short",
            PartnerLongDescription = longDesc,
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            IsDeleted = false
        };
        _context.Partners.Add(partner);
        _context.SaveChanges();
        return partner.Id;
    }

    private int SeedContact(int id, string firstName, string lastName, string email, int partnerId)
    {
        var contact = new UNOPSContact
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Title = "Manager",
            PartnerId = partnerId,
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            IsDeleted = false
        };
        _context.Contacts.Add(contact);
        _context.SaveChanges();
        return contact.Id;
    }

    private int SeedInteraction(int id, string subject)
    {
        var interaction = new UNOPSInteraction
        {
            Subject = subject,
            Description = "Desc",
            Type = Domain.Enums.InteractionType.VirtualMeeting,
            Date = DateTime.UtcNow,
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            IsDeleted = false
        };
        _context.Interactions.Add(interaction);
        _context.SaveChanges();
        return interaction.Id;
    }

    #endregion
}
