/**
 * @fileoverview PNO-1196 shared test fixture base.
 * Opportunity status set to Closed after rejection (NO GO decision).
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// Shared fixture base for PNO-1196: Opportunity EntityStatus→Closed after Reject.
/// Provides InMemory DB, mocked WorkflowController dependencies, and seed helpers.
/// </summary>
public abstract class PNO1196TestFixtureBase : IDisposable
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
    protected readonly UserResolverService<int> UserResolverService;
    protected readonly DefaultHttpContext HttpContext;
    protected readonly DbContextOptions<AppDbContext> DbOptions;

    protected PNO1196TestFixtureBase()
    {
        DbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@unops.org")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        HttpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(HttpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        UserResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new AppDbContext(DbOptions, UserResolverService, mockDbContextSchema.Object);

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
            .ReturnsAsync("Generated statement text");

        // Default: allow approval for all tests (individual tests can override)
        MockApproverProvider.Setup(x => x.CanUserApproveAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        MockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("GO");
        MockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Test Opportunity");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x["AppBaseUrl"]).Returns("https://test.pao.unops.org");

        var mockNotificationLogger = new Mock<ILogger<PaoWorkflowNotificationService>>();
        var mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        mockContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(DbOptions, UserResolverService, mockDbContextSchema.Object));
        mockContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(DbOptions, UserResolverService, mockDbContextSchema.Object));

        var mockNotificationManager = new Mock<NotificationManager>(
            new AppDbContext(DbOptions, UserResolverService, mockDbContextSchema.Object),
            UserResolverService);
        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
        NotificationService = new PaoWorkflowNotificationService(
            MockEmailSender.Object,
            mockContextFactory.Object,
            mockServiceScopeFactory.Object,
            mockNotificationLogger.Object,
            mockConfiguration.Object,
            mockNotificationManager.Object);

        Controller = new WorkflowController(
            MockLogger.Object,
            MockAuthService.Object,
            UserResolverService,
            MockWorkflowManager.Object,
            MockEntityStageProvider.Object,
            MockApproverProvider.Object,
            new[] { MockRequirementsProvider.Object },
            MockManagerWrapper.Object,
            DbContext,
            NotificationService);

        Controller.ControllerContext = new ControllerContext { HttpContext = HttpContext };
    }

    /// <summary>Seeds an Opportunity with specified stage and status.</summary>
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
                Description = "PNO-1196 test opportunity",
                Stage = stage,
                Status = status,
                IsDeleted = false,
                InitiativeBudgetUSD = 500000m,
                Challenges = "Test challenges",
                ExpectedImpact = "Test expected impact",
                ExpectedOutcomes = "Test expected outcomes",
                BeneficiariesToBeDetermined = true,
                UNOPSMissionsNotApplicable = true,
                TargetSigningDate = DateTime.UtcNow.AddMonths(2),
                ImplementationStartDate = DateTime.UtcNow.AddMonths(3),
                TargetDeliveryDate = DateTime.UtcNow.AddMonths(14),
                OpportunityStatementMarkdown = "## Opportunity Statement",
                ResponsibleOrgUnitId = 1,
                ProposedInitiativeTypeId = 1
            });
        }
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 1))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = 1,
                Name = "UNOPS HQ",
                Code = "HQ",
                Description = "UNOPS Headquarters",
                IsDeleted = false
            });
        }
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a pending workflow task.
    /// NOTE: WorkflowLog is managed by IWorkflowManager (fully mocked in tests).
    /// AppDbContext does not include WorkflowLog entities, so no DB insertion is needed.
    /// MockWorkflowManager.Setup(x => x.PendingTask(...)).Returns(workflowLog) handles the mock.
    /// </summary>
    protected Task SeedPendingWorkflowTaskAsync(int opportunityId, int taskId = 100)
    {
        // No-op: IWorkflowManager.PendingTask() is mocked in each test to return the expected WorkflowLog.
        return Task.CompletedTask;
    }

    /// <summary>Builds a valid RejectWorkflowRequest.</summary>
    protected static RejectWorkflowRequest BuildRejectRequest(
        int entityId = 1,
        string entityName = "Opportunity",
        string rationale = "Project no longer aligned with UNOPS mandate",
        bool confirm = true) => new()
    {
        EntityId = entityId,
        EntityName = entityName,
        Rationale = rationale,
        ConfirmationAcknowledged = confirm
    };

    public virtual void Dispose()
    {
        DbContext.Dispose();
    }
}
