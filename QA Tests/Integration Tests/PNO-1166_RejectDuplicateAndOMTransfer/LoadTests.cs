using System.Security.Claims;
using System.Diagnostics;
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
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
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

namespace UNOPS.PAO.IntegrationTests.PNO1166;

/// <summary>
/// PNO-1166: Load tests for Reject action fix + OM role transfer.
/// Tests sustained load, spike load, and stress scenarios.
/// Each load test uses its own InMemory DB instance to avoid state conflicts.
/// </summary>
[Collection("Load")]
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class LoadTests : IDisposable
{
    public void Dispose()
    {
        // No shared DB - each test creates its own
    }

    private static (WorkflowController Controller, AppDbContext DbContext, Mock<IWorkflowManager> MockWorkflowManager, Mock<IEntityStageProvider> MockEntityStageProvider, Mock<IPaoWorkflowApproverProvider> MockApproverProvider) CreateControllerWithDb()
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
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        var dbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);

        var mockLogger = new Mock<ILogger<WorkflowController>>();
        var mockAuthService = new Mock<IAuthorizationService>();
        var mockWorkflowManager = new Mock<IWorkflowManager>();
        var mockEntityStageProvider = new Mock<IEntityStageProvider>();
        var mockApproverProvider = new Mock<IPaoWorkflowApproverProvider>();

        mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);
        mockEntityStageProvider.Setup(x => x.IsEntityValidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("IDENTIFY & PROFILE");
        var mockRequirementsProvider = new Mock<IStageRequirementsProvider>();
        var mockManagerWrapper = new Mock<IManagerWrapper>();
        var mockGeminiManager = new Mock<IGeminiManager>();
        var mockEmailSender = new Mock<IEmailSender>();

        mockRequirementsProvider.Setup(x => x.EntityNames).Returns(new[] { "Opportunity" });
        mockRequirementsProvider.Setup(x => x.GetRequirementsForStageChange(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<StageRequirement>());
        mockManagerWrapper.Setup(x => x.GeminiManager).Returns(mockGeminiManager.Object);
        mockGeminiManager.Setup(x => x.GenerateOpportunityStatementAsync(
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
        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
        var notificationService = new PaoWorkflowNotificationService(
            mockEmailSender.Object,
            mockContextFactory.Object,
            mockServiceScopeFactory.Object,
            mockNotificationLogger.Object,
            mockConfiguration.Object,
            mockNotificationManager.Object);

        var controller = new WorkflowController(
            mockLogger.Object,
            mockAuthService.Object,
            userResolverService,
            mockWorkflowManager.Object,
            mockEntityStageProvider.Object,
            mockApproverProvider.Object,
            new[] { mockRequirementsProvider.Object },
            mockManagerWrapper.Object,
            dbContext,
            notificationService);

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider);
    }

    private static async Task SeedOpportunityAsync(AppDbContext dbContext, int id, string stage)
    {
        var existing = await dbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Stage = stage;
        }
        else
        {
            dbContext.Opportunities.Add(new Opportunity
            {
                Id = id,
                Name = $"Test Opportunity {id}",
                Description = "Test",
                Stage = stage,
                Status = EntityStatus.Active,
                IsDeleted = false,
                InitiativeBudgetUSD = 100000m,
                Challenges = "Test",
                ExpectedImpact = "Test",
                ExpectedOutcomes = "Test",
                BeneficiariesToBeDetermined = true,
                UNOPSMissionsNotApplicable = true,
                TargetSigningDate = DateTime.UtcNow.AddMonths(1),
                ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
                TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
                OpportunityStatementMarkdown = "##",
                ResponsibleOrgUnitId = 1,
                ProposedInitiativeTypeId = 1
            });
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedOpportunityManagerStakeholderAsync(AppDbContext dbContext, int opportunityId, int userId)
    {
        var omRole = await dbContext.EntityRoles.FirstOrDefaultAsync(r => r.Name == "Opportunity Manager");
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
            dbContext.EntityRoles.Add(omRole);
            await dbContext.SaveChangesAsync();
        }
        dbContext.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            Id = opportunityId * 1000 + userId,
            OpportunityId = opportunityId,
            UserId = userId,
            EntityRoleId = omRole.Id,
            EntityRole = omRole,
            IsInternal = true
        });
        await dbContext.SaveChangesAsync();
    }

    #region LOAD_001-003: Sustained load

    [Fact]
    public async Task LOAD_001_TenConsecutiveWorkflowOperations_Within10Seconds()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 10; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var sw = Stopwatch.StartNew();
            for (var i = 1; i <= 10; i++)
            {
                var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = i, Comment = "Cancel" };
                await controller.Cancel(request);
            }
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(10000);
        }
    }

    [Fact]
    public async Task LOAD_002_TwentyConsecutiveRejectOperations_Within20Seconds()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 20; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = i.ToString(), NewStage = "GO" };
                mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns(pendingTask);
                mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
                mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", i.ToString())).ReturnsAsync($"Test {i}");
                mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", i, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
                mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", i, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            }

            var sw = Stopwatch.StartNew();
            for (var i = 1; i <= 20; i++)
            {
                var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = i, Rationale = "Reject", ConfirmationAcknowledged = true };
                await controller.Reject(request);
            }
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(20000);
        }
    }

    [Fact]
    public async Task LOAD_003_FiftyHistoryQueries_Within10Seconds()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, _) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 50; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
            }
            mockEntityStageProvider.Setup(x => x.IsEntityValidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            mockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", It.IsAny<int>())).Returns(new List<WorkflowHistoryModel>());

            var sw = Stopwatch.StartNew();
            var tasks = Enumerable.Range(1, 50).Select(i => controller.GetWorkflowHistory("Opportunity", i)).ToArray();
            await Task.WhenAll(tasks);
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(10000);
        }
    }

    #endregion

    #region LOAD_004-006: Spike load

    [Fact]
    public async Task LOAD_004_BurstOfFiveSimultaneousRejects()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 5; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = i.ToString(), NewStage = "GO" };
                mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns(pendingTask);
                mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
                mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", i.ToString())).ReturnsAsync($"Test {i}");
                mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", i, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
                mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", i, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            }

            var tasks = Enumerable.Range(1, 5).Select(i =>
                controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = i, Rationale = "Reject", ConfirmationAcknowledged = true })).ToArray();
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(5);
        }
    }

    [Fact]
    public async Task LOAD_005_BurstOfFiveSimultaneousCancels()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 5; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var tasks = Enumerable.Range(1, 5).Select(i =>
                controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = i, Comment = "Cancel" })).ToArray();
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(5);
        }
    }

    [Fact]
    public async Task LOAD_006_BurstOfTenWorkflowStateQueries()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithDb();
        await using (dbContext)
        {
            await SeedOpportunityAsync(dbContext, 1, "IDENTIFY & PROFILE");

            var tasks = Enumerable.Range(0, 10).Select(_ => controller.GetWorkflowState("Opportunity", 1)).ToArray();
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(10);
        }
    }

    #endregion

    #region LOAD_007-010: Stress tests

    [Fact]
    public async Task LOAD_007_HundredSequentialWorkflowOperations()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 100; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var successCount = 0;
            for (var i = 1; i <= 100; i++)
            {
                var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = i, Comment = "Cancel" };
                var result = await controller.Cancel(request);
                if (result.Result is OkObjectResult) successCount++;
            }
            successCount.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task LOAD_008_WorkflowOperations_WithLargeDataPayloads()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithDb();
        await using (dbContext)
        {
            await SeedOpportunityAsync(dbContext, 1, "IDENTIFY & PROFILE");
            var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "1", NewStage = "GO" };
            mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1)).Returns(pendingTask);
            mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "1")).ReturnsAsync("IDENTIFY & PROFILE");
            mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "1")).ReturnsAsync("Test");
            mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 1, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
            mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var largeRationale = new string('x', 2000);
            var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = largeRationale, ConfirmationAcknowledged = true };
            var result = await controller.Reject(request);
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task LOAD_009_Operations_WithIncreasingRationaleSizes()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var size = 100; size <= 500; size += 100)
            {
                await SeedOpportunityAsync(dbContext, size, "IDENTIFY & PROFILE");
                var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = size.ToString(), NewStage = "GO" };
                mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", size)).Returns(pendingTask);
                mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", size.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
                mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", size.ToString())).ReturnsAsync($"Test {size}");
                mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", size, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
                mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", size, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

                var rationale = new string('a', size);
                var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = size, Rationale = rationale, ConfirmationAcknowledged = true };
                var result = await controller.Reject(request);
                result.Should().NotBeNull();
            }
        }
    }

    [Fact]
    public async Task LOAD_010_SystemRecovery_AfterHeavyLoad()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithDb();
        await using (dbContext)
        {
            for (var i = 1; i <= 20; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
            }

            for (var round = 0; round < 3; round++)
            {
                var tasks = Enumerable.Range(1, 10).Select(i => controller.GetWorkflowState("Opportunity", i)).ToArray();
                await Task.WhenAll(tasks);
            }

            var finalResult = await controller.GetWorkflowState("Opportunity", 1);
            finalResult.Result.Should().NotBeNull();
        }
    }

    #endregion
}
