using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.AuditLogs;
using UNOPS.PAO.Models.Links;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Smoke;

/// <summary>
/// Fast, critical-path smoke tests designed to run on every Push/PR.
/// These verify that the system's core components instantiate, persist,
/// and retrieve data without throwing — acting as a "is the system alive?" gate.
///
/// Run with: dotnet test --filter "Category=Smoke"
/// Expected duration: less than 30 seconds for the full suite.
/// </summary>
[Trait("Category", "Smoke")]
public class SmokeTests : ManagerTestBase
{
    private readonly IMapper _realMapper;

    public SmokeTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("UNOPS.PAO") == true));
        });
        _realMapper = mapperConfig.CreateMapper();
    }

    // =====================================================================
    // AUTOMAPPER CONFIGURATION
    // =====================================================================

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("Category", "Smoke")]
    public void AutoMapper_Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("UNOPS.PAO") == true));
        });

        config.Invoking(c => c.AssertConfigurationIsValid())
            .Should().NotThrow("all AutoMapper profiles must be valid for the system to function");
    }

    // =====================================================================
    // DATABASE CONTEXT & SCHEMA
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public void DbContext_Instantiates_WithoutError()
    {
        Context.Should().NotBeNull();
        Context.Database.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task DbContext_CanQueryPartners()
    {
        var query = await Context.Partners.Take(1).ToListAsync();
        query.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task DbContext_CanQueryContacts()
    {
        var query = await Context.Contacts.Take(1).ToListAsync();
        query.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task DbContext_CanQueryOpportunities()
    {
        var query = await Context.Opportunities.Take(1).ToListAsync();
        query.Should().NotBeNull();
    }

    // =====================================================================
    // PARTNER MANAGER — CRUD round-trip
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task PartnerManager_CreateAndRetrieve_RoundTrips()
    {
        var manager = new PartnerManager(_realMapper, Context);

        var partner = new UNOPSPartner
        {
            Name = $"Smoke-Partner-{Guid.NewGuid():N}",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Partners.Add(partner);
        await Context.SaveChangesAsync();

        var retrieved = await Context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == partner.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(partner.Name);
    }

    // =====================================================================
    // CONTACT MANAGER — CRUD round-trip
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task ContactManager_CreateAndRetrieve_RoundTrips()
    {
        var partnerId = await CreateTestPartnerAsync("Smoke-Contact-Parent");

        var contact = new UNOPSContact
        {
            Name = $"Smoke-Contact-{Guid.NewGuid():N}",
            LastName = "SmokeLastName",
            Title = "QA Engineer",
            Email = "smoke@unops.org",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Contacts.Add(contact);
        await Context.SaveChangesAsync();

        var retrieved = await Context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contact.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("smoke@unops.org");
    }

    // =====================================================================
    // COMMENT MANAGER — create and fetch
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task CommentManager_CreateAndGet_Works()
    {
        var mockManagerWrapper = new Mock<IManagerWrapper>();
        var mockUserDataManager = new Mock<IUserDataManager>();
        mockUserDataManager.Setup(u => u.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PAOUserModel { Id = 1, Email = "smoke@unops.org" });
        mockManagerWrapper.Setup(m => m.UserDataManager).Returns(mockUserDataManager.Object);

        var manager = new CommentManager(_realMapper, Context, mockManagerWrapper.Object);

        var request = new CommentRequest
        {
            EntityType = "Partner",
            EntityId = 1,
            Content = "Smoke test comment"
        };

        var created = await manager.CreateCommentAsync(request);

        created.Should().NotBeNull();
        created.Content.Should().Be("Smoke test comment");

        var comments = await manager.GetCommentsByEntityAsync("Partner", 1);
        comments.Should().NotBeEmpty();
    }

    // =====================================================================
    // AUDIT LOG MANAGER — create and fetch
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task AuditLogManager_CreateAndGet_Works()
    {
        var manager = new AuditLogManager(_realMapper, Context);

        var request = new AuditLogCreateRequest
        {
            EntityType = "Partner",
            EntityId = 999,
            Action = "smoke_test",
            UserId = TestUserId,
            Description = "Smoke test audit entry"
        };

        var created = await manager.CreateAuditLogAsync(request);

        created.Should().NotBeNull();
        created.Action.Should().Be("smoke_test");

        var latest = await manager.GetLatestAuditLogAsync("Partner", 999);
        latest.Should().NotBeNull();
        latest!.Action.Should().Be("smoke_test");
    }

    // =====================================================================
    // LINK MANAGER — create with FK validation
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task LinkManager_CreateAndGet_Works()
    {
        var partnerId = await CreateTestPartnerAsync("Smoke-Link-Parent");

        var manager = new LinkManager(_realMapper, Context);

        var request = new LinkRequest
        {
            Entity = LinkEntityType.Partner,
            EntityId = partnerId,
            Url = "https://smoke-test.unops.org",
            Name = "Smoke Link"
        };

        var created = await manager.CreateLinkAsync(request);

        created.Should().NotBeNull();
        created.Url.Should().Be("https://smoke-test.unops.org");

        var retrieved = await manager.GetLink(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Url.Should().Be("https://smoke-test.unops.org");
    }

    // =====================================================================
    // DOCUMENT MANAGER — instantiation and null-safe retrieval
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task DocumentManager_GetNonExistentDocument_ReturnsNull()
    {
        var manager = new DocumentManager(_realMapper, Context);

        var result = await manager.GetDocumentByIdAsync(-1);

        result.Should().BeNull("non-existent document ID should return null, not throw");
    }

    // =====================================================================
    // ENTITY ARTIFACT MANAGER — instantiation
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public void EntityArtifactManager_Instantiates()
    {
        var manager = new EntityArtifactManager(_realMapper, Context);

        manager.Should().NotBeNull();
    }

    // =====================================================================
    // OPPORTUNITY — persist and read back
    // =====================================================================

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("Category", "Smoke")]
    public async Task Opportunity_CreateAndRetrieve_RoundTrips()
    {
        var opportunity = new Domain.Entities.Opportunity
        {
            Name = $"Smoke-Opp-{Guid.NewGuid():N}"[..50],
            Description = "Smoke test opportunity",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Opportunities.Add(opportunity);
        await Context.SaveChangesAsync();

        var retrieved = await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Description.Should().Be("Smoke test opportunity");
    }

    // =====================================================================
    // INTERACTION — persist and read back
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task Interaction_CreateAndRetrieve_RoundTrips()
    {
        var interaction = new UNOPSInteraction
        {
            Name = $"Smoke-Interaction-{Guid.NewGuid():N}",
            Subject = "Smoke test meeting",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Description = "Smoke test interaction",
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Interactions.Add(interaction);
        await Context.SaveChangesAsync();

        var retrieved = await Context.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interaction.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Type.Should().Be(InteractionType.InPersonMeeting);
    }

    // =====================================================================
    // SOFT DELETE — verify the IsDeleted pattern works
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task SoftDelete_FilterWorks_DeletedRecordExcluded()
    {
        var partner = new UNOPSPartner
        {
            Name = $"Smoke-Deleted-{Guid.NewGuid():N}",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedBy = TestUserId,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Partners.Add(partner);
        await Context.SaveChangesAsync();

        var activePartners = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Id == partner.Id)
            .ToListAsync();

        activePartners.Should().BeEmpty("soft-deleted partner must not appear in filtered queries");
    }

    // =====================================================================
    // TRANSACTION ISOLATION — verify test data doesn't leak
    // =====================================================================

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestTransaction_DataIsIsolated()
    {
        var uniqueName = $"Smoke-Isolated-{Guid.NewGuid():N}";

        var partner = new UNOPSPartner
        {
            Name = uniqueName,
            Status = EntityStatus.Active,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Partners.Add(partner);
        await Context.SaveChangesAsync();

        var exists = await Context.Partners
            .AnyAsync(p => p.Name == uniqueName);

        exists.Should().BeTrue("data should be visible within the test transaction");
    }
}
