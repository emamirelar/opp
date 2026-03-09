using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.Unit.SoftDeleteAuditTests;

/// <summary>
/// Shared fixture for SoftDeleteAuditTests.
/// Seeds both active and soft-deleted records for each UNOPS entity type,
/// then tests verify the exact query patterns used in production managers
/// correctly exclude soft-deleted records.
///
/// Uses UNOPS entity types (UNOPSPartner, UNOPSContact, etc.) because
/// UNOPSAppDbContext overrides base DbSets with UNOPS-typed DbSets.
/// </summary>
public class SoftDeleteAuditTestFixture : IDisposable
{
    public UNOPSAppDbContext Context { get; }

    public int ActivePartnerId { get; private set; }
    public int DeletedPartnerId { get; private set; }
    public int ActiveContactId { get; private set; }
    public int DeletedContactId { get; private set; }
    public int ActiveOpportunityId { get; private set; }
    public int DeletedOpportunityId { get; private set; }
    public int ActiveInteractionId { get; private set; }
    public int DeletedInteractionId { get; private set; }
    public int ActiveDocumentId { get; private set; }
    public int DeletedDocumentId { get; private set; }

    public SoftDeleteAuditTestFixture()
    {
        var dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"SoftDel_{Guid.NewGuid():N}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");

        var mockHttpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") });
        mockHttpContext.Setup(c => c.User).Returns(new ClaimsPrincipal(identity));
        var mockRequest = new Mock<Microsoft.AspNetCore.Http.HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(new Microsoft.AspNetCore.Http.HeaderDictionary());
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        var mockAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var userResolver = new UserResolverService<int>(mockAccessor.Object);
        Context = new UNOPSAppDbContext(dbContextOptions, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(Context);

        SeedAllEntities().GetAwaiter().GetResult();
    }

    private async Task SeedAllEntities()
    {
        var activePartner = new UNOPSPartner
        {
            Name = "Active Partner",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deletedPartner = new UNOPSPartner
        {
            Name = "DELETED Partner - Should Not Appear",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow.AddDays(-1)
        };
        Context.Partners.AddRange(activePartner, deletedPartner);
        await Context.SaveChangesAsync();
        ActivePartnerId = activePartner.Id;
        DeletedPartnerId = deletedPartner.Id;

        var activeContact = new UNOPSContact
        {
            Name = "Active Contact",
            LastName = "Active",
            Title = "Mr",
            Email = "active@example.com",
            PartnerId = ActivePartnerId,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deletedContact = new UNOPSContact
        {
            Name = "DELETED Contact - Should Not Appear",
            LastName = "Deleted",
            Title = "Mr",
            Email = "deleted@example.com",
            PartnerId = ActivePartnerId,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow.AddDays(-1)
        };
        Context.Contacts.AddRange(activeContact, deletedContact);
        await Context.SaveChangesAsync();
        ActiveContactId = activeContact.Id;
        DeletedContactId = deletedContact.Id;

        var activeOpp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = "Active Opportunity",
            Description = "Active opp description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deletedOpp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = "DELETED Opportunity - Should Not Appear",
            Description = "Deleted opp description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow.AddDays(-1)
        };
        Context.Opportunities.AddRange(activeOpp, deletedOpp);
        await Context.SaveChangesAsync();
        ActiveOpportunityId = activeOpp.Id;
        DeletedOpportunityId = deletedOpp.Id;

        var activeInteraction = new UNOPSInteraction
        {
            Name = "Active Interaction",
            Subject = "Active interaction subject",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deletedInteraction = new UNOPSInteraction
        {
            Name = "DELETED Interaction - Should Not Appear",
            Subject = "Deleted interaction subject",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow.AddDays(-1)
        };
        Context.Interactions.AddRange(activeInteraction, deletedInteraction);
        await Context.SaveChangesAsync();
        ActiveInteractionId = activeInteraction.Id;
        DeletedInteractionId = deletedInteraction.Id;

        var activeDocument = new UNOPSDocument
        {
            Name = "Active Document",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deletedDocument = new UNOPSDocument
        {
            Name = "DELETED Document - Should Not Appear",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow.AddDays(-1)
        };
        Context.Documents.AddRange(activeDocument, deletedDocument);
        await Context.SaveChangesAsync();
        ActiveDocumentId = activeDocument.Id;
        DeletedDocumentId = deletedDocument.Id;
    }

    public void Dispose() => Context.Dispose();
}
