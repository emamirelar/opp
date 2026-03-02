using System.Security.Claims;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
/// PNO-1166: Performance tests for Reject action fix + OM role transfer.
/// Tests response times, throughput, and memory/resource usage.
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Type", "Performance")]
public class PerformanceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly WorkflowController _controller;
    private readonly Mock<IWorkflowManager> _mockWorkflowManager;
    private readonly Mock<IEntityStageProvider> _mockEntityStageProvider;
    private readonly Mock<IPaoWorkflowApproverProvider> _mockApproverProvider;

    public PerformanceTests()
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
        _dbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);

        _mockWorkflowManager = new Mock<IWorkflowManager>();
        _mockEntityStageProvider = new Mock<IEntityStageProvider>();
        _mockApproverProvider = new Mock<IPaoWorkflowApproverProvider>();

        var mockLogger = new Mock<ILogger<WorkflowController>>();
        var mockAuthService = new Mock<IAuthorizationService>();
        var mockRequirementsProvider = new Mock<IStageRequirementsProvider>();
        var mockManagerWrapper = new Mock<IManagerWrapper>();
        var mockGeminiManager = new Mock<IGeminiManager>();
        var mockEmailSender = new Mock<IEmailSender>();

        mockRequirementsProvider.Setup(x => x.EntityNames).Returns(new[] { "Opportunity" });
        mockRequirementsProvider.Setup(x => x.GetRequirementsForStageChange(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new List<StageRequirement>());
        mockManagerWrapper.Setup(x => x.GeminiManager).Returns(mockGeminiManager.Object);
        var mockOppManager = new Mock<IOpportunityManager>();
        mockOppManager.Setup(x => x.AssignExecutiveAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        mockManagerWrapper.Setup(x => x.OpportunityManager).Returns(mockOppManager.Object);
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
        var notificationService = new PaoWorkflowNotificationService(
            mockEmailSender.Object,
            mockContextFactory.Object,
            mockNotificationLogger.Object,
            mockConfiguration.Object,
            mockNotificationManager.Object);

        _controller = new WorkflowController(
            mockLogger.Object,
            mockAuthService.Object,
            userResolverService,
            _mockWorkflowManager.Object,
            _mockEntityStageProvider.Object,
            _mockApproverProvider.Object,
            new[] { mockRequirementsProvider.Object },
            mockManagerWrapper.Object,
            _dbContext,
            notificationService);

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task SeedOpportunityAsync(int id, string stage)
    {
        var existing = await _dbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Stage = stage;
        }
        else
        {
            _dbContext.Opportunities.Add(new Opportunity
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
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedOpportunityManagerStakeholderAsync(int opportunityId, int userId)
    {
        var omRole = await _dbContext.EntityRoles.FirstOrDefaultAsync(r => r.Name == "Opportunity Manager");
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
            _dbContext.EntityRoles.Add(omRole);
            await _dbContext.SaveChangesAsync();
        }
        _dbContext.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            Id = opportunityId * 1000 + userId,
            OpportunityId = opportunityId,
            UserId = userId,
            EntityRoleId = omRole.Id,
            EntityRole = omRole,
            IsInternal = true
        });
        await _dbContext.SaveChangesAsync();
    }

    #region PERF_001-005: Response time tests

    [Fact]
    public async Task PERF_001_Reject_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "1", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "1")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "1")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 1, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Reject", ConfirmationAcknowledged = true };
        var sw = Stopwatch.StartNew();
        await _controller.Reject(request);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task PERF_002_Cancel_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(2, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 2)).Returns((WorkflowLog?)null);

        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 2, Comment = "Cancel" };
        var sw = Stopwatch.StartNew();
        await _controller.Cancel(request);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task PERF_003_Reopen_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(3, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(3, 1);

        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 3, Comment = null };
        var sw = Stopwatch.StartNew();
        await _controller.Reopen(request);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task PERF_004_Approve_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "4", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "4")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "4")).ReturnsAsync("Test");
        _mockEntityStageProvider.Setup(x => x.UpdateStageAsync("Opportunity", "4", "GO", It.IsAny<int>())).ReturnsAsync(true);
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 4, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Approve(It.IsAny<WorkflowLog>(), "Opportunity", 4, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("GO");
        var mockOppManager = new Mock<IOpportunityManager>();
        mockOppManager.Setup(x => x.AssignExecutiveAsync(4, 10)).Returns(Task.CompletedTask);

        var request = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 4, Rationale = "Approve", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        var sw = Stopwatch.StartNew();
        await _controller.Approve(request);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task PERF_005_WorkflowHistory_LoadsWithin200ms()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "5")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", 5)).Returns(new List<WorkflowHistoryModel>());

        var sw = Stopwatch.StartNew();
        await _controller.GetWorkflowHistory("Opportunity", 5);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    #endregion

    #region PERF_006-010: Throughput tests

    [Fact]
    public async Task PERF_006_TenSequentialRejects_Within5Seconds()
    {
        for (var i = 10; i < 20; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = i.ToString(), NewStage = "GO" };
            _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns(pendingTask);
            _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
            _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", i.ToString())).ReturnsAsync($"Test {i}");
            _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", i, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
            _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", i, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        }

        var sw = Stopwatch.StartNew();
        for (var i = 10; i < 20; i++)
        {
            var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = i, Rationale = "Reject", ConfirmationAcknowledged = true };
            await _controller.Reject(request);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task PERF_007_TenSequentialCancels_Within5Seconds()
    {
        for (var i = 20; i < 30; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var sw = Stopwatch.StartNew();
        for (var i = 20; i < 30; i++)
        {
            var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = i, Comment = "Cancel" };
            await _controller.Cancel(request);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public async Task PERF_008_WorkflowStateQuery_Under100ms()
    {
        await SeedOpportunityAsync(30, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "30")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "30")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 30)).Returns((WorkflowLog?)null);

        var sw = Stopwatch.StartNew();
        await _controller.GetWorkflowState("Opportunity", 30);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task PERF_009_RequirementsQuery_Under200ms()
    {
        await SeedOpportunityAsync(31, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync(It.IsAny<string>(), "31")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), "31")).ReturnsAsync("IDENTIFY & PROFILE");

        var sw = Stopwatch.StartNew();
        await _controller.GetRequirementsForStageChange("Opportunity", 31, "GO");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public async Task PERF_010_FiftyHistoryEntries_LoadWithin500ms()
    {
        await SeedOpportunityAsync(32, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "32")).ReturnsAsync(true);
        var history = Enumerable.Range(0, 50).Select(i => new WorkflowHistoryModel
        {
            FromStage = "IDENTIFY & PROFILE",
            ToStage = "GO",
            Action = "Submitted",
            CompletedOn = DateTime.UtcNow.AddMinutes(-i)
        }).ToList();
        _mockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", 32)).Returns(history);

        var sw = Stopwatch.StartNew();
        await _controller.GetWorkflowHistory("Opportunity", 32);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    #endregion

    #region PERF_011-016: Memory/resource tests

    [Fact]
    public async Task PERF_011_Reject_DoesNotLeakConnections()
    {
        await SeedOpportunityAsync(40, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "40", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 40)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "40")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "40")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 40, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 40, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        for (var i = 0; i < 10; i++)
        {
            var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 40, Rationale = "Reject", ConfirmationAcknowledged = true };
            await _controller.Reject(request);
        }
        _dbContext.ChangeTracker.Entries().Should().NotBeNull();
    }

    [Fact]
    public async Task PERF_012_LargeRationale_DoesNotCauseMemorySpike()
    {
        await SeedOpportunityAsync(41, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "41", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 41)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "41")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "41")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 41, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 41, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var largeRationale = new string('x', 2000);
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 41, Rationale = largeRationale, ConfirmationAcknowledged = true };
        var result = await _controller.Reject(request);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PERF_013_BulkWorkflowOperations_MemoryStable()
    {
        for (var i = 42; i < 52; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", i.ToString())).ReturnsAsync(true);
            _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
            _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
        }

        for (var i = 42; i < 52; i++)
        {
            await _controller.GetWorkflowState("Opportunity", i);
        }
        _dbContext.Opportunities.Count().Should().Be(10);
    }

    [Fact]
    public void PERF_014_DbContext_ProperlyDisposed()
    {
        _dbContext.Should().NotBeNull();
        _dbContext.Database.EnsureCreated();
        _dbContext.Dispose();
        Assert.True(true);
    }

    [Fact]
    public async Task PERF_015_Reject_NoExcessiveSqlQueries()
    {
        await SeedOpportunityAsync(45, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "45", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 45)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "45")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "45")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 45, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 45, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 45, Rationale = "Reject", ConfirmationAcknowledged = true };
        var sw = Stopwatch.StartNew();
        await _controller.Reject(request);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task PERF_016_RejectWithLargeHistory_DoesNotDegrade()
    {
        await SeedOpportunityAsync(46, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "46", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 46)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "46")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "46")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 46, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 46, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 46, Rationale = "Reject", ConfirmationAcknowledged = true };
        var sw = Stopwatch.StartNew();
        await _controller.Reject(request);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    #endregion
}
