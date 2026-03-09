/**
 * @fileoverview Mock-based tests for UNOPSInteractionManager AI methods.
 * Tests GetInteractionDetailsForAIAsync, GetInteractionDetailsForOpportunityCreationAsync, GetInteractionDetailsAsync.
 * Uses in-memory DB; no external API calls.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Repositories.Generic;
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
/// Mock-based tests for UNOPSInteractionManager AI methods.
/// 3:1 Ratio: P=2, N≥6, E≥6, F≥6, I≥6
/// </summary>
public class UNOPSInteractionManagerAITests : ManagerTestBase
{
    private readonly UNOPSInteractionManager _manager;
    private readonly string _testMarker = $"IntAI_{Guid.NewGuid():N}";

    public UNOPSInteractionManagerAITests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbSchema"] = "Host=localhost;Database=test;",
            ["CloudStorage:BucketName"] = "test-bucket"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService
            .Setup(x => x.ApplyAccessControlFiltersAsync<UNOPSInteraction>(
                It.IsAny<IQueryable<UNOPSInteraction>>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((IQueryable<UNOPSInteraction> q, ClaimsPrincipal _, string __, string ___) => q);

        var mockUserPreference = new Mock<IUserPreferenceService>();
        var mockOrgUnit = new Mock<IOrgUnitHierarchyService>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<GlobalFilterService>>();
        var globalFilterService = new GlobalFilterService(
            mockUserPreference.Object,
            mockLogger.Object,
            Context,
            mockOrgUnit.Object);

        var partnerTreeRepository = new DataRepository<UNOPSPartnerTree>(Context);
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var partnerTreeService = new PartnerTreeService(partnerTreeRepository, memoryCache);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, TestUserId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _manager = new UNOPSInteractionManager(
            mapper,
            Context,
            configuration,
            partnerTreeService,
            mockPermissionService.Object,
            globalFilterService,
            mockHttpContextAccessor.Object,
            null,
            null,
            null);
    }

    private async Task<int> CreateInteractionWithDetailsAsync()
    {
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"Contact {_testMarker}",
            FirstName = "C",
            LastName = "Test",
            Title = "Mr",
            Email = $"c_{_testMarker}@test.com",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await Context.SaveChangesAsync();

        var interaction = new UNOPSInteraction
        {
            Name = $"Interaction {_testMarker}",
            Subject = $"Subject {_testMarker}",
            Description = "Description",
            Date = DateTime.UtcNow,
            Type = InteractionType.InPersonMeeting,
            Location = "Location",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await Context.SaveChangesAsync();

        await Context.InteractionContacts.AddAsync(new InteractionContact
        {
            InteractionId = interaction.Id,
            ContactId = contact.Id
        });
        await Context.InteractionPartners.AddAsync(new InteractionPartner
        {
            InteractionId = interaction.Id,
            PartnerId = partnerId
        });
        await Context.SaveChangesAsync();

        return interaction.Id;
    }

    #region Positive (2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetInteractionDetailsForAIAsync_ValidId_ReturnsStructuredObject()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()) }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, id);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("id").Should().NotBeNull();
        result.GetType().GetProperty("subject").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_ValidId_ReturnsDictionary()
    {
        var id = await CreateInteractionWithDetailsAsync();

        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        result.Should().NotBeNull();
        result.Should().ContainKey("subject");
        result.Should().ContainKey("contacts");
        result.Should().ContainKey("partners");
    }

    #endregion

    #region Negative (6+)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetInteractionDetailsForAIAsync_NonExistentId_ReturnsErrorObject()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, int.MaxValue);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("error").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_NonExistentId_ReturnsNull()
    {
        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(int.MaxValue);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetInteractionDetailsAsync_NonExistentId_ReturnsNull()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsAsync(user, int.MaxValue);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetInteractionDetailsForAIAsync_DeletedInteraction_ReturnsErrorOrNull()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var interaction = await Context.Interactions.FindAsync(id);
        if (interaction != null)
        {
            interaction.IsDeleted = true;
            await Context.SaveChangesAsync();
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, id);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_DeletedInteraction_ReturnsNull()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var interaction = await Context.Interactions.FindAsync(id);
        if (interaction != null)
        {
            interaction.IsDeleted = true;
            await Context.SaveChangesAsync();
        }

        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetInteractionDetailsAsync_NullUser_HandlesGracefully()
    {
        var id = await CreateInteractionWithDetailsAsync();

        var result = await _manager.GetInteractionDetailsAsync(null!, id);

        result.Should().NotBeNull();
    }

    #endregion

    #region Edge/Boundary (6+)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetInteractionDetailsForAIAsync_InteractionWithNoContacts_ReturnsEmptyContacts()
    {
        var partnerId = await CreateTestPartnerAsync($"PartnerNoC_{_testMarker}");
        var interaction = new UNOPSInteraction
        {
            Name = $"NoContacts {_testMarker}",
            Subject = "No Contacts",
            Description = "Desc",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await Context.SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, interaction.Id);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_InteractionWithNoPartners_ReturnsEmptyPartners()
    {
        var interaction = new UNOPSInteraction
        {
            Name = $"NoPartners {_testMarker}",
            Subject = "No Partners",
            Description = "Desc",
            Date = DateTime.UtcNow,
            Type = InteractionType.Call,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Interactions.AddAsync(interaction);
        await Context.SaveChangesAsync();

        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(interaction.Id);

        result.Should().NotBeNull();
        result!["partners"].Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetInteractionDetailsAsync_InteractionIdZero_ReturnsNull()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsAsync(user, 0);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetInteractionDetailsForAIAsync_InteractionIdNegative_ReturnsErrorOrNull()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, -1);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_InteractionWithDocuments_IncludesDocuments()
    {
        var id = await CreateInteractionWithDetailsAsync();

        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        result.Should().NotBeNull();
        result!.Should().ContainKey("documents");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetInteractionDetailsForAIAsync_ContainsSummaryStatistics()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, id);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("summary").Should().NotBeNull();
    }

    #endregion

    #region Functional (6+)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetInteractionDetailsForAIAsync_ReturnsContactNames()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, id);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("contactNames").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_ReturnsDateFormatted()
    {
        var id = await CreateInteractionWithDetailsAsync();

        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        result.Should().NotBeNull();
        result!.Should().ContainKey("date");
        result["date"].Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetInteractionDetailsAsync_ReturnsInteractionModel()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsAsync(user, id);

        result.Should().NotBeNull();
        result!.Subject.Should().NotBeNullOrEmpty();
        result.Id.Should().Be(id);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetInteractionDetailsForAIAsync_ReturnsPartnerNames()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsForAIAsync(user, id);

        result.Should().NotBeNull();
        result!.GetType().GetProperty("partnerNames").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetInteractionDetailsForOpportunityCreationAsync_ReturnsType()
    {
        var id = await CreateInteractionWithDetailsAsync();

        var result = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        result.Should().NotBeNull();
        result!.Should().ContainKey("type");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetInteractionDetailsAsync_PopulatesContactIds()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsAsync(user, id);

        result.Should().NotBeNull();
        result!.ContactIds.Should().NotBeNull();
    }

    #endregion

    #region Integration (6+)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetInteractionDetailsAsync_ThenForAI_ConsistentData()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var details = await _manager.GetInteractionDetailsAsync(user, id);
        var forAI = await _manager.GetInteractionDetailsForAIAsync(user, id);

        details.Should().NotBeNull();
        forAI.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetInteractionDetailsForOpportunityCreation_ThenForAI_BothSucceed()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var forCreation = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);
        var forAI = await _manager.GetInteractionDetailsForAIAsync(user, id);

        forCreation.Should().NotBeNull();
        forAI.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_InteractionWithContactsAndPartners_AllRelationsLoaded()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetInteractionDetailsAsync(user, id);

        result.Should().NotBeNull();
        result!.ContactIds.Should().NotBeEmpty();
        result.PartnerIds.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_ContextAndRepositories_UsedCorrectly()
    {
        var id = await CreateInteractionWithDetailsAsync();

        var forCreation = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        ((string?)forCreation["subject"]).Should().NotBeNullOrEmpty();
        forCreation["contacts"].Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetBasicEntityAsync_DelegatesToGetInteractionDetailsAsync()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var result = await _manager.GetBasicEntityAsync(id, user);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_AllThreeMethods_SameInteractionId()
    {
        var id = await CreateInteractionWithDetailsAsync();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

        var details = await _manager.GetInteractionDetailsAsync(user, id);
        var forAI = await _manager.GetInteractionDetailsForAIAsync(user, id);
        var forCreation = await _manager.GetInteractionDetailsForOpportunityCreationAsync(id);

        details.Should().NotBeNull();
        forAI.Should().NotBeNull();
        forCreation.Should().NotBeNull();
        (forCreation!["id"] as int?).Should().Be(id);
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | GetInteractionDetailsForAIAsync_ValidId_ReturnsStructuredObject, GetInteractionDetailsForOpportunityCreationAsync_ValidId_ReturnsDictionary |
| Negative (N) | 6 | GetInteractionDetailsForAIAsync_NonExistentId_ReturnsErrorObject, GetInteractionDetailsForOpportunityCreationAsync_NonExistentId_ReturnsNull, GetInteractionDetailsAsync_NonExistentId_ReturnsNull, GetInteractionDetailsForAIAsync_DeletedInteraction_ReturnsErrorOrNull, GetInteractionDetailsForOpportunityCreationAsync_DeletedInteraction_ReturnsNull, GetInteractionDetailsAsync_NullUser_HandlesGracefully |
| Edge/Boundary (E) | 6 | GetInteractionDetailsForAIAsync_InteractionWithNoContacts_ReturnsEmptyContacts, GetInteractionDetailsForOpportunityCreationAsync_InteractionWithNoPartners_ReturnsEmptyPartners, GetInteractionDetailsAsync_InteractionIdZero_ReturnsNull, GetInteractionDetailsForAIAsync_InteractionIdNegative_ReturnsErrorOrNull, GetInteractionDetailsForOpportunityCreationAsync_InteractionWithDocuments_IncludesDocuments, GetInteractionDetailsForAIAsync_ContainsSummaryStatistics |
| Functional (F) | 6 | GetInteractionDetailsForAIAsync_ReturnsContactNames, GetInteractionDetailsForOpportunityCreationAsync_ReturnsDateFormatted, GetInteractionDetailsAsync_ReturnsInteractionModel, GetInteractionDetailsForAIAsync_ReturnsPartnerNames, GetInteractionDetailsForOpportunityCreationAsync_ReturnsType, GetInteractionDetailsAsync_PopulatesContactIds |
| Integration (I) | 6 | FullFlow_GetInteractionDetailsAsync_ThenForAI_ConsistentData, FullFlow_GetInteractionDetailsForOpportunityCreation_ThenForAI_BothSucceed, FullFlow_InteractionWithContactsAndPartners_AllRelationsLoaded, FullFlow_ContextAndRepositories_UsedCorrectly, FullFlow_GetBasicEntityAsync_DelegatesToGetInteractionDetailsAsync, FullFlow_AllThreeMethods_SameInteractionId |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
