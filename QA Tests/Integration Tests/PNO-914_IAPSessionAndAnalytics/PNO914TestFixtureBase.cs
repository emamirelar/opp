/**
 * @fileoverview PNO-914 test fixture base — IAP Session Refresh & Analytics improvements.
 * Provides in-memory UNOPSAppDbContext, seeded reference data (OrgUnit, Partner, Interaction,
 * Document, User), UNOPSOpportunityManager, and mock helpers for AI, PDF, and analytics services.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914;

/// <summary>
/// Base fixture for PNO-914 tests.
/// PNO-914: IAP Session Refresh, Analytics, Create Opportunity from Interactions, AI Integration, PDF Generation.
/// Tests UNOPSOpportunityManager.CreateOpportunityFromProposalAsync and related flows.
/// </summary>
public abstract class PNO914TestFixtureBase : IDisposable
{
    protected readonly UNOPSAppDbContext DbContext;
    protected readonly UNOPSOpportunityManager Manager;
    protected readonly int CurrentUserId = 1;
    private bool _disposed;

    protected PNO914TestFixtureBase()
    {
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, CurrentUserId.ToString()),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@unops.org")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new UNOPSAppDbContext(options, userResolverService, mockSchema.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(UNOPSOpportunityManager).Assembly);
            cfg.CreateMap<Country, UNOPS.PAO.Models.Locations.CountryModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        var configValues = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DbSchema"] = "public",
            ["ASPNETCORE_ENVIRONMENT"] = "Testing",
            ["AISettings:DisableExternalCalls"] = "true"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new UNOPSAppDbContext(options, userResolverService, mockSchema.Object));
        mockDbContextFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new UNOPSAppDbContext(options, userResolverService, mockSchema.Object));

        var mockExchangeRate = new Mock<IExchangeRateService>();
        mockExchangeRate.Setup(x => x.ConvertToUSDAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
            .ReturnsAsync((decimal amount, string _, DateTime? _) => new ExchangeRateResult
            {
                AmountUSD = amount,
                ExchangeRate = 1.0m,
                ExchangeRateDate = DateTime.UtcNow,
                ExchangeRateId = 0
            });
        mockExchangeRate.Setup(x => x.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(1.0m);

        Manager = new UNOPSOpportunityManager(
            mapper,
            DbContext,
            configuration,
            mockDbContextFactory.Object,
            mockExchangeRate.Object,
            null,
            mockHttpContextAccessor.Object,
            null);
    }

    // ──────────────────────────────────────────────────────────────
    // Seed helpers
    // ──────────────────────────────────────────────────────────────

    protected async Task<OrganizationHierarchy> SeedOrgUnitAsync(
        int id,
        string name = "Test OrgUnit",
        OrganizationUnitType type = OrganizationUnitType.OrgUnit)
    {
        var existing = await DbContext.Set<OrganizationHierarchy>().FindAsync(id);
        if (existing != null) return existing;

        var orgUnit = new OrganizationHierarchy
        {
            Id = id,
            Name = name,
            Code = $"OU{id}",
            Description = $"Org unit {id}",
            Type = type,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        DbContext.Set<OrganizationHierarchy>().Add(orgUnit);
        await DbContext.SaveChangesAsync();
        return orgUnit;
    }

    protected async Task<Opportunity> SeedOpportunityAsync(
        int id,
        string name,
        int? responsibleOrgUnitId = null,
        string stage = "IDENTIFY & PROFILE")
    {
        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null) return existing;

        var opportunity = new Opportunity
        {
            Id = id,
            Name = name,
            Description = "Test opportunity",
            Stage = stage,
            Status = EntityStatus.Active,
            IsDeleted = false,
            ResponsibleOrgUnitId = responsibleOrgUnitId,
            InitiativeBudgetUSD = 50000m,
            BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true
        };
        DbContext.Opportunities.Add(opportunity);
        await DbContext.SaveChangesAsync();
        return opportunity;
    }

    protected async Task<Interaction> SeedInteractionAsync(int id, string subject, int? partnerId = null, bool isDeleted = false)
    {
        var existing = await DbContext.Interactions.FindAsync(id);
        if (existing != null) return existing;

        var interaction = new UNOPSInteraction
        {
            Id = id,
            Name = subject,
            Subject = subject,
            Type = InteractionType.VirtualMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            IsDeleted = isDeleted
        };
        DbContext.Interactions.Add(interaction);
        await DbContext.SaveChangesAsync();
        return interaction;
    }

    protected async Task<Document> SeedDocumentAsync(int id, string storagePath = "gs://bucket/folder/doc.pdf", int? interactionId = null)
    {
        var existing = await DbContext.Documents.FindAsync(id);
        if (existing != null) return existing;

        var document = new UNOPSDocument(false)
        {
            Id = id,
            Name = $"Document-{id}",
            StoragePath = storagePath,
            Type = "application/pdf",
            Status = EntityStatus.Active,
            IsDeleted = false,
            InteractionId = interactionId
        };
        DbContext.Documents.Add(document);
        await DbContext.SaveChangesAsync();
        return document;
    }

    protected async Task<PAOUser> SeedUserAsync(int id, string email = "user@unops.org")
    {
        var existing = await DbContext.PAOUsers.FindAsync(id);
        if (existing != null) return existing;

        var user = new PAOUser
        {
            Id = id,
            Email = email
        };
        DbContext.PAOUsers.Add(user);
        await DbContext.SaveChangesAsync();
        return user;
    }

    protected async Task<Partner> SeedPartnerAsync(int id, string name = "Test Partner")
    {
        var existing = await DbContext.Partners.FindAsync(id);
        if (existing != null) return existing;

        var partner = new UNOPSPartner
        {
            Id = id,
            Name = name,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        DbContext.Partners.Add(partner);
        await DbContext.SaveChangesAsync();
        return partner;
    }

    protected async Task EnsureReferenceDataAsync()
    {
        if (!await DbContext.Currencies.AnyAsync(c => c.Code == "USD"))
        {
            DbContext.Currencies.Add(new Currency { Code = "USD", Name = "US Dollar", IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }

        if (!await DbContext.EntityRoles.AnyAsync(r => r.Code == "Opportunity_Manager_Opportunity"))
        {
            DbContext.EntityRoles.Add(new EntityRole
            {
                Id = 100,
                Name = "Opportunity Manager",
                EntityType = "Opportunity",
                Code = "Opportunity_Manager_Opportunity",
                IsDeleted = false,
                Status = EntityStatus.Active
            });
            await DbContext.SaveChangesAsync();
        }

        if (!await DbContext.ProposedInitiativeTypes.AnyAsync())
        {
            DbContext.ProposedInitiativeTypes.Add(new ProposedInitiativeType
            {
                Id = 1,
                Name = "Project",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
            await DbContext.SaveChangesAsync();
        }

        if (!await DbContext.Countries.AnyAsync())
        {
            DbContext.Countries.Add(new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" });
            await DbContext.SaveChangesAsync();
        }

        if (!await DbContext.SDGs.AnyAsync())
        {
            DbContext.SDGs.Add(new SDG { Id = 1, Name = "No Poverty", SDGNumber = "1", IsDeleted = false });
            await DbContext.SaveChangesAsync();
        }

        if (!await DbContext.Outputs.AnyAsync())
        {
            DbContext.Outputs.Add(new Output
            {
                Id = 1,
                Name = "Test Output",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
            await DbContext.SaveChangesAsync();
        }
    }

    protected CreateOpportunityFromInteractionsRequest BuildRequest(
        string name = "PNO-914 Test Opportunity",
        int? responsibleOrgUnitId = null,
        int? partnerId = null,
        bool isFundingPartner = false,
        bool isClientPartner = false,
        List<int>? sourceInteractionIds = null,
        List<NewDocumentRequest>? documents = null)
    {
        return new CreateOpportunityFromInteractionsRequest
        {
            Name = name,
            Description = "Test description",
            PartnerId = partnerId,
            IsFundingPartner = isFundingPartner,
            IsClientPartner = isClientPartner,
            ResponsibleOrgUnitId = responsibleOrgUnitId,
            SourceInteractionIds = sourceInteractionIds,
            Documents = documents
        };
    }

    protected async Task<Opportunity?> GetOpportunityFromDbAsync(int opportunityId)
    {
        return await DbContext.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);
    }

    // ──────────────────────────────────────────────────────────────
    // IDisposable
    // ──────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DbContext.Dispose();
    }
}
