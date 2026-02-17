using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Xunit;
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1197;

/// <summary>
/// Shared test fixture base for PNO-1197 DoA Level 3 Fallback tests.
/// Provides InMemory DB (AppDbContext), mocks, and seed helpers.
/// </summary>
public abstract class PNO1197TestFixtureBase : IDisposable
{
    protected readonly Mock<ILogger<WorkflowController>> MockLogger;
    protected readonly Mock<IAuthorizationService> MockAuthService;
    protected readonly Mock<IWorkflowManager> MockWorkflowManager;
    protected readonly Mock<IEntityStageProvider> MockEntityStageProvider;
    protected readonly Mock<IPaoWorkflowApproverProvider> MockApproverProvider;
    protected readonly Mock<IStageRequirementsProvider> MockRequirementsProvider;
    protected readonly Mock<IManagerWrapper> MockManagerWrapper;
    protected readonly Mock<IGeminiManager> MockGeminiManager;
    protected readonly Mock<IEmailSender> MockEmailSender;
    protected readonly PaoWorkflowNotificationService NotificationService;
    protected readonly AppDbContext DbContext;
    protected readonly WorkflowController Controller;
    protected readonly DefaultHttpContext HttpContext;

    protected PNO1197TestFixtureBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        HttpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(HttpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);

        MockLogger = new Mock<ILogger<WorkflowController>>();
        MockAuthService = new Mock<IAuthorizationService>();
        MockWorkflowManager = new Mock<IWorkflowManager>();
        MockEntityStageProvider = new Mock<IEntityStageProvider>();
        MockApproverProvider = new Mock<IPaoWorkflowApproverProvider>();
        MockRequirementsProvider = new Mock<IStageRequirementsProvider>();
        MockManagerWrapper = new Mock<IManagerWrapper>();
        MockGeminiManager = new Mock<IGeminiManager>();
        MockEmailSender = new Mock<IEmailSender>();

        MockRequirementsProvider.Setup(x => x.EntityNames).Returns(new[] { "Opportunity" });
        MockManagerWrapper.Setup(x => x.GeminiManager).Returns(MockGeminiManager.Object);
        MockGeminiManager.Setup(x => x.GenerateOpportunityStatementAsync(
            It.IsAny<int>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
            .ReturnsAsync("Generated statement");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x["AppBaseUrl"]).Returns("https://test.pao.unops.org");

        var mockNotificationLogger = new Mock<ILogger<PaoWorkflowNotificationService>>();
        var mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        mockContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(options, userResolverService, mockDbContextSchema.Object));
        mockContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(options, userResolverService, mockDbContextSchema.Object));
        var mockNotificationManager = new Mock<NotificationManager>(
            new AppDbContext(options, userResolverService, mockDbContextSchema.Object),
            userResolverService);
        NotificationService = new PaoWorkflowNotificationService(
            MockEmailSender.Object,
            mockContextFactory.Object,
            mockNotificationLogger.Object,
            mockConfiguration.Object,
            mockNotificationManager.Object);

        Controller = new WorkflowController(
            MockLogger.Object,
            MockAuthService.Object,
            userResolverService,
            MockWorkflowManager.Object,
            MockEntityStageProvider.Object,
            MockApproverProvider.Object,
            new[] { MockRequirementsProvider.Object },
            MockManagerWrapper.Object,
            DbContext,
            NotificationService);

        Controller.ControllerContext = new ControllerContext { HttpContext = HttpContext };
    }

    /// <summary>
    /// Seeds an Opportunity that satisfies ALL 21 ValidateOpportunityRequirementsAsync checks.
    /// Includes DoA2 holder by default. Use SeedDoAHolderAsync to add/override DoA holders.
    /// </summary>
    protected async Task SeedOpportunityAsync(int id, string stage, EntityStatus status = EntityStatus.Active)
    {
        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Stage = stage;
            existing.Status = status;
        }
        else
        {
            DbContext.Opportunities.Add(new Opportunity
            {
                Id = id,
                Name = $"Test Opportunity {id}",
                Description = "Full test opportunity for workflow testing",
                Stage = stage,
                Status = status,
                IsDeleted = false,
                InitiativeBudgetUSD = 100000m,
                Challenges = "Test challenges description",
                ExpectedImpact = "Test expected impact",
                ExpectedOutcomes = "Test expected outcomes",
                BeneficiariesToBeDetermined = true,
                UNOPSMissionsNotApplicable = true,
                TargetSigningDate = DateTime.UtcNow.AddMonths(1),
                ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
                TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
                OpportunityStatementMarkdown = "## Opportunity Statement\nThis is a test statement.",
                ResponsibleOrgUnitId = 1,
                ProposedInitiativeTypeId = 1
            });
        }
        await DbContext.SaveChangesAsync();

        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 1))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = 1,
                Name = "Test Org Unit",
                Code = "TOU",
                Description = "Test org unit",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        if (!await DbContext.Set<ProposedInitiativeType>().AnyAsync(p => p.Id == 1))
        {
            DbContext.Set<ProposedInitiativeType>().Add(new ProposedInitiativeType
            {
                Id = 1,
                Name = "Test Initiative Type",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await DbContext.SaveChangesAsync();

        if (!await DbContext.Set<OpportunityDeliverable>().AnyAsync(d => d.OpportunityId == id))
        {
            DbContext.Set<OpportunityDeliverable>().Add(new OpportunityDeliverable
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                Name = "Test Deliverable"
            });
        }
        if (!await DbContext.Set<OpportunitySDG>().AnyAsync(s => s.OpportunityId == id))
        {
            DbContext.Set<OpportunitySDG>().Add(new OpportunitySDG
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                SDGId = 1,
                Name = "SDG 1"
            });
        }
        if (!await DbContext.Set<OpportunityFundingPartner>().AnyAsync(fp => fp.OpportunityId == id))
        {
            DbContext.Set<OpportunityFundingPartner>().Add(new OpportunityFundingPartner
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                PartnerId = 1,
                Name = "Funding Partner"
            });
        }
        if (!await DbContext.Set<OpportunityClientPartner>().AnyAsync(cp => cp.OpportunityId == id))
        {
            DbContext.Set<OpportunityClientPartner>().Add(new OpportunityClientPartner
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                PartnerId = 2,
                Name = "Client Partner"
            });
        }
        if (!await DbContext.Set<OpportunityCountry>().AnyAsync(oc => oc.OpportunityId == id))
        {
            if (!await DbContext.Set<Country>().AnyAsync(c => c.Id == 1))
            {
                DbContext.Set<Country>().Add(new Country
                {
                    Id = 1,
                    Name = "Test Country",
                    Iso2Code = "TC",
                    Status = EntityStatus.Active,
                    IsDeleted = false
                });
                await DbContext.SaveChangesAsync();
            }
            DbContext.Set<OpportunityCountry>().Add(new OpportunityCountry
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                CountryId = 1,
                Name = "Test Country"
            });
        }

        if (!await DbContext.EntityUserRoles.AnyAsync(eur =>
                eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1 &&
                eur.EntityRole != null && eur.EntityRole.Code == "DoA2_OrganizationHierarchy" && !eur.IsDeleted))
        {
            await SeedDoAHolderAsync(1, 2);
        }
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Creates EntityUserRole with EntityRole.Code = "DoA{level}_OrganizationHierarchy" for the given org unit.
    /// Explicitly sets EntityRole navigation property for InMemory DB.
    /// </summary>
    protected async Task SeedDoAHolderAsync(int orgUnitId, int doaLevel)
    {
        var code = $"DoA{doaLevel}_OrganizationHierarchy";
        var entityRole = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Code == code);
        if (entityRole == null)
        {
            entityRole = new EntityRole
            {
                Id = 200 + doaLevel,
                Name = $"DoA Level {doaLevel} Holder",
                Code = code,
                EntityType = "OrganizationHierarchy",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            DbContext.EntityRoles.Add(entityRole);
            await DbContext.SaveChangesAsync();
        }

        var nextId = await DbContext.EntityUserRoles.AnyAsync()
            ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) + 1
            : 1;
        DbContext.EntityUserRoles.Add(new EntityUserRole
        {
            Id = nextId,
            UserId = 1,
            EntityRoleId = entityRole.Id,
            EntityRole = entityRole,
            EntityId = orgUnitId,
            EntityType = "OrganizationHierarchy",
            Name = $"DoA{doaLevel} Holder",
            IsDeleted = false
        });
        await DbContext.SaveChangesAsync();
    }

    protected async Task SeedOpportunityManagerStakeholderAsync(int opportunityId, int userId)
    {
        var omRole = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Name == "Opportunity Manager");
        if (omRole == null)
        {
            omRole = new EntityRole
            {
                Id = 100,
                Name = "Opportunity Manager",
                Code = "OPP_MANAGER",
                EntityType = "Opportunity",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            DbContext.EntityRoles.Add(omRole);
            await DbContext.SaveChangesAsync();
        }
        DbContext.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            Id = opportunityId * 1000 + userId,
            OpportunityId = opportunityId,
            UserId = userId,
            EntityRoleId = omRole.Id,
            EntityRole = omRole,
            IsInternal = true
        });
        await DbContext.SaveChangesAsync();
    }

    protected void SetupStandardSubmitMocks()
    {
        MockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", It.IsAny<string>())).ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", It.IsAny<string>()))
            .ReturnsAsync("IDENTIFY & PROFILE");
        MockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", It.IsAny<string>()))
            .ReturnsAsync("Test Opportunity");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);
        MockWorkflowManager.Setup(x => x.WorkflowStateByStage(
                It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal))
            .Returns(new State { StageCode = "IDENTIFY & PROFILE" });
        MockWorkflowManager.Setup(x => x.NextActions("Opportunity", It.IsAny<State>(), Facing.Internal))
            .Returns(new[] { new WorkflowStateActionModel { NewStage = "GO", Comment = "optional" } });
        MockWorkflowManager.Setup(x => x.ApprovalNeeded("Opportunity", "IDENTIFY & PROFILE", "GO")).Returns(true);
        MockWorkflowManager.Setup(x => x.AddLog(It.IsAny<WorkflowLogModel>())).Returns(Task.CompletedTask);
        MockWorkflowManager.Setup(x => x.Initiate(
            It.IsAny<UNOPS.Workflow.Models.WorkflowActionModel>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
    }

    protected async Task RemoveDoAHoldersForOrgUnitAsync(int orgUnitId)
    {
        var holders = await DbContext.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == orgUnitId)
            .ToListAsync();
        DbContext.EntityUserRoles.RemoveRange(holders);
        await DbContext.SaveChangesAsync();
    }

    public virtual void Dispose()
    {
        DbContext.Dispose();
    }
}
