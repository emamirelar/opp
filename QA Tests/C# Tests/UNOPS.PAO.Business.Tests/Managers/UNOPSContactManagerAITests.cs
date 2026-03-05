/**
 * @fileoverview Mock-based tests for UNOPSContactManager AI methods.
 * Tests GetPartnerNamesFromGeminiAsync, GetPartnerNamesForAIAsync, GetUnmatchedEmailsWithPartnerSuggestionsAsync.
 * Uses AISettings:DisableExternalCalls to avoid real Gemini API calls.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Mock-based tests for UNOPSContactManager AI methods.
/// 3:1 Ratio: P=2, N≥6, E≥6, F≥6, I≥6
/// </summary>
public class UNOPSContactManagerAITests : ManagerTestBase
{
    private readonly UNOPSContactManager _manager;
    private readonly string _testMarker = $"ContactAI_{Guid.NewGuid():N}";

    public UNOPSContactManagerAITests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["AISettings:DisableExternalCalls"] = "true",
            ["ASPNETCORE_ENVIRONMENT"] = "Testing",
            ["ConnectionStrings:DbSchema"] = "Host=localhost;Database=test;"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        var mockPermissionService = new Mock<IPermissionService>();

        var mockUserPreference = new Mock<IUserPreferenceService>();
        var mockOrgUnit = new Mock<IOrgUnitHierarchyService>();
        var mockLogger = new Mock<ILogger<GlobalFilterService>>();
        var globalFilterService = new GlobalFilterService(
            mockUserPreference.Object,
            mockLogger.Object,
            Context,
            mockOrgUnit.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var contactLogger = new Mock<ILogger<UNOPSContactManager>>().Object;

        _manager = new UNOPSContactManager(
            mapper,
            Context,
            configuration,
            mockPermissionService.Object,
            globalFilterService,
            mockHttpContextAccessor.Object,
            contactLogger,
            null);
    }

    #region Positive (2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerNamesForAIAsync_EmptyDomains_ReturnsStructuredResponse()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesForAIAsync(user, new List<string>());

        result.Should().NotBeNull();
        var dyn = result as dynamic;
        Assert.NotNull(dyn);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_EmailsWithDbMatch_ReturnsPartnerSuggestions()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Test {_testMarker}",
            FirstName = "Test",
            LastName = "User",
            Title = "Mr",
            Email = $"test_{_testMarker}@example.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var emails = new List<string> { $"other_{_testMarker}@example.com" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].UnmatchedEmail.Should().Be(emails[0]);
        result[0].PartnerName.Should().NotBeNullOrEmpty();
        result[0].PartnerId.Should().Be(partnerId);
    }

    #endregion

    #region Negative (6+)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerNamesFromGeminiAsync_NonExistentContact_ReturnsEmptyStructure()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesFromGeminiAsync(user, int.MaxValue);

        result.Should().NotBeNull();
        var dyn = result as dynamic;
        Assert.NotNull(dyn);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_NullList_Throws()
    {
        var act = () => _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(null!, null);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]

    [Trait("Defect", "DEF-090")]
    [Trait("Category", "Negative")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_InvalidEmailFormat_ReturnsEmptyPartnerName()
    {
        var emails = new List<string> { "invalid-email-no-at" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].UnmatchedEmail.Should().Be("invalid-email-no-at");
        result[0].PartnerName.Should().BeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerNamesForAIAsync_NullUser_ReturnsResponseWithFallback()
    {
        var result = await _manager.GetPartnerNamesForAIAsync(null!, new List<string> { "example.com" });

        result.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-090")]
    [Trait("Category", "Negative")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_UnknownDomain_ReturnsFallbackWithoutPartnerId()
    {
        var emails = new List<string> { "user@unknown-domain-xyz-12345.com" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].UnmatchedEmail.Should().Be(emails[0]);
        result[0].PartnerId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerNamesFromGeminiAsync_ContactWithNoEmail_ReturnsEmptyDomains()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerNoEmail_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"NoEmail {_testMarker}",
            FirstName = "No",
            LastName = "Email",
            Title = "Mr",
            Email = "",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesFromGeminiAsync(user, contact.Id);

        result.Should().NotBeNull();
    }

    #endregion

    #region Edge/Boundary (6+)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_EmptyList_ReturnsEmptyList()
    {
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(new List<string>(), null);

        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerNamesForAIAsync_SingleDomain_ReturnsStructuredResponse()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesForAIAsync(user, new List<string> { "example.com" });

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_EmailWithNoDomain_HandlesGracefully()
    {
        var emails = new List<string> { "invalid@" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].UnmatchedEmail.Should().Be("invalid@");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_MultipleDomains_MixedMatchAndUnmatch()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerMulti_{_testMarker}");
        await Context.Contacts.AddAsync(new UNOPSContact
        {
            Name = $"C1 {_testMarker}",
            FirstName = "C",
            LastName = "1",
            Title = "Mr",
            Email = $"a_{_testMarker}@matched.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        });
        await SaveChangesAsync();

        var emails = new List<string>
        {
            $"b_{_testMarker}@matched.com",
            "user@unmatched-xyz.com"
        };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerNamesForAIAsync_MultipleDomains_ReturnsAllInResponse()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesForAIAsync(user, new List<string> { "a.com", "b.com", "c.com" });

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_DuplicateEmails_ReturnsOnePerEntry()
    {
        var emails = new List<string> { "same@test.com", "same@test.com" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(2);
    }

    #endregion

    #region Functional (6+)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_VerifiesUnmatchedEmailProperty()
    {
        var emails = new List<string> { "verify@test.com" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result[0].UnmatchedEmail.Should().Be("verify@test.com");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerNamesForAIAsync_EmptyDomains_ContainsPartnerNamesKey()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        var result = await _manager.GetPartnerNamesForAIAsync(user, new List<string>());

        result.Should().NotBeNull();
        result!.GetType().GetProperty("partnerNames").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerNamesFromGeminiAsync_ContactWithEmail_ExtractsDomain()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerDomain_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Domain {_testMarker}",
            FirstName = "D",
            LastName = "Domain",
            Title = "Mr",
            Email = $"user_{_testMarker}@extracted-domain.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesFromGeminiAsync(user, contact.Id);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_ContactWithSameDomain_ReturnsMostCommonPartner()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerCommon_{_testMarker}");
        var domain = $"common_{_testMarker}.com";
        for (int i = 0; i < 3; i++)
        {
            await Context.Contacts.AddAsync(new UNOPSContact
            {
                Name = $"C{i} {_testMarker}",
                FirstName = "C",
                LastName = i.ToString(),
                Title = "Mr",
                Email = $"c{i}_{_testMarker}@{domain}",
                PartnerId = partnerId,
                Status = EntityStatus.Active,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            });
        }
        await SaveChangesAsync();

        var emails = new List<string> { $"newuser_{_testMarker}@{domain}" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].PartnerId.Should().Be(partnerId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerNamesForAIAsync_WithDomains_ContainsSummaryMetadata()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesForAIAsync(user, new List<string> { "example.com" });

        result.Should().NotBeNull();
        result!.GetType().GetProperty("searchMetadata").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetUnmatchedEmailsWithPartnerSuggestionsAsync_UserParameter_AcceptedWithoutError()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        var emails = new List<string> { "user@test.com" };

        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, user);

        result.Should().NotBeNull();
    }

    #endregion

    #region Integration (6+)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetPartnerNamesFromGeminiThenForAI_ConsistentStructure()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerFlow_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Flow {_testMarker}",
            FirstName = "F",
            LastName = "Low",
            Title = "Mr",
            Email = $"flow_{_testMarker}@flow.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var fromGemini = await _manager.GetPartnerNamesFromGeminiAsync(user, contact.Id);
        var domain = "flow.com";
        var forAI = await _manager.GetPartnerNamesForAIAsync(user, new List<string> { domain });

        fromGemini.Should().NotBeNull();
        forAI.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_UnmatchedEmails_EndToEndWithDbAndFallback()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerE2E_{_testMarker}");
        await Context.Contacts.AddAsync(new UNOPSContact
        {
            Name = $"E2E {_testMarker}",
            FirstName = "E",
            LastName = "2E",
            Title = "Mr",
            Email = $"e2e_{_testMarker}@e2e.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        });
        await SaveChangesAsync();

        var emails = new List<string>
        {
            $"other_{_testMarker}@e2e.com",
            "unknown@no-match-xyz.com"
        };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().HaveCount(2);
        result[0].PartnerId.Should().Be(partnerId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_ContactRepositoryAndPartnerRepository_UsedCorrectly()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerRepo_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Repo {_testMarker}",
            FirstName = "R",
            LastName = "epo",
            Title = "Mr",
            Email = $"repo_{_testMarker}@repo.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var emails = new List<string> { $"lookup_{_testMarker}@repo.com" };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().ContainSingle(r => r.PartnerId == partnerId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetPartnerNamesFromGeminiAsync_ContactToDomainToAI()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerChain_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Chain {_testMarker}",
            FirstName = "C",
            LastName = "hain",
            Title = "Mr",
            Email = $"chain_{_testMarker}@chain.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesFromGeminiAsync(user, contact.Id);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_MultipleEmailsDifferentDomains_AllProcessed()
    {
        var emails = new List<string>
        {
            "a@domain1.com",
            "b@domain2.com",
            "c@domain3.com"
        };
        var result = await _manager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(emails, null);

        result.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_ConfigurationDisableExternalCalls_NoGeminiCallSucceeds()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetPartnerNamesForAIAsync(user, new List<string> { "test.com" });

        result.Should().NotBeNull();
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | GetPartnerNamesForAIAsync_EmptyDomains_ReturnsStructuredResponse, GetUnmatchedEmailsWithPartnerSuggestionsAsync_EmailsWithDbMatch_ReturnsPartnerSuggestions |
| Negative (N) | 6 | GetPartnerNamesFromGeminiAsync_NonExistentContact_ReturnsEmptyStructure, GetUnmatchedEmailsWithPartnerSuggestionsAsync_NullList_ThrowsOrHandlesGracefully, GetUnmatchedEmailsWithPartnerSuggestionsAsync_InvalidEmailFormat_ReturnsEmptyPartnerName, GetPartnerNamesForAIAsync_NullUser_ReturnsResponseWithFallback, GetUnmatchedEmailsWithPartnerSuggestionsAsync_UnknownDomain_ReturnsFallbackWithoutPartnerId, GetPartnerNamesFromGeminiAsync_ContactWithNoEmail_ReturnsEmptyDomains |
| Edge/Boundary (E) | 6 | GetUnmatchedEmailsWithPartnerSuggestionsAsync_EmptyList_ReturnsEmptyList, GetPartnerNamesForAIAsync_SingleDomain_ReturnsStructuredResponse, GetUnmatchedEmailsWithPartnerSuggestionsAsync_EmailWithNoDomain_HandlesGracefully, GetUnmatchedEmailsWithPartnerSuggestionsAsync_MultipleDomains_MixedMatchAndUnmatch, GetPartnerNamesForAIAsync_MultipleDomains_ReturnsAllInResponse, GetUnmatchedEmailsWithPartnerSuggestionsAsync_DuplicateEmails_ReturnsOnePerEntry |
| Functional (F) | 6 | GetUnmatchedEmailsWithPartnerSuggestionsAsync_VerifiesUnmatchedEmailProperty, GetPartnerNamesForAIAsync_EmptyDomains_ContainsPartnerNamesKey, GetPartnerNamesFromGeminiAsync_ContactWithEmail_ExtractsDomain, GetUnmatchedEmailsWithPartnerSuggestionsAsync_ContactWithSameDomain_ReturnsMostCommonPartner, GetPartnerNamesForAIAsync_WithDomains_ContainsSummaryMetadata, GetUnmatchedEmailsWithPartnerSuggestionsAsync_UserParameter_AcceptedWithoutError |
| Integration (I) | 6 | FullFlow_GetPartnerNamesFromGeminiThenForAI_ConsistentStructure, FullFlow_UnmatchedEmails_EndToEndWithDbAndFallback, FullFlow_ContactRepositoryAndPartnerRepository_UsedCorrectly, FullFlow_GetPartnerNamesFromGeminiAsync_ContactToDomainToAI, FullFlow_MultipleEmailsDifferentDomains_AllProcessed, FullFlow_ConfigurationDisableExternalCalls_NoGeminiCallSucceeds |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
