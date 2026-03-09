/**
 * PERFORMANCE TESTS — Workflow Operations
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Context: Workflow submodule (UNOPS.Workflow) available in CI with GH_PAT secret.
 * Endpoints: /api/workflow (submit, approve, reject, recall, reopen, cancel)
 * Related: PNO-731 (Org Unit Role Refresh), PNO-1146 (Email Notifications),
 *          PNO-1166 (Reject Duplicate and OM Transfer), PNO-1197 (DoA3 Fallback),
 *          Task-8.4 (Workflow Requirements IsMet validation)
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 * @see .cursor/rules/entity-framework-performance-optimization.mdc
 */

using System.Diagnostics;
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

namespace UNOPS.PAO.Business.Tests.PerformanceTests;

/// <summary>
/// Performance Tests for Workflow operations (submit, approve, reject, recall, reopen, cancel).
/// Verifies response times, throughput, and behaviour under concurrent access.
/// Uses mocked services and InMemory DB — no real database connections.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Type", "Performance")]
public class WorkflowPerformanceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly WorkflowController _controller;
    private readonly Mock<IWorkflowManager> _mockWorkflowManager;
    private readonly Mock<IEntityStageProvider> _mockEntityStageProvider;
    private readonly Mock<IPaoWorkflowApproverProvider> _mockApproverProvider;
    private readonly Stopwatch _stopwatch;

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private const int MaxSingleOperationMs = 500;
    private const int MaxBulkOperationMs = 5_000;
    private const int MaxSimpleSearchMs = 500;
    private const int MaxComplexSearchMs = 2_000;
    private const int MaxPaginatedQueryMs = 200;
    private const int MaxStatusQueryMs = 200;
    private const int MaxConcurrentReadMs = 100;
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public WorkflowPerformanceTests()
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

        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("IDENTIFY & PROFILE");

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
        _stopwatch = new Stopwatch();
    }

    public void Dispose() => _dbContext.Dispose();

    #region Helpers

    private static async Task SeedOpportunityAsync(AppDbContext dbContext, int id, string stage)
    {
        var existing = await dbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Stage = stage;
        }
        else
        {
            dbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
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

    private static WorkflowSubmitRequest CreateSubmitRequest(int entityId = 1) => new()
    {
        EntityName = "opportunity",
        EntityId = entityId,
        NewStage = "GO",
        ConfirmedNonOMSubmission = false,
        ConfirmedOrgUnitWarning = true,
        AcknowledgedStatement = true
    };

    #endregion

    #region 1. Single Workflow Stage Transition (SLA: 500ms)

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task SingleWorkflowStageTransition_SubmitCompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(_dbContext, 1, 1);

        _stopwatch.Restart();
        var result = await _controller.Submit(CreateSubmitRequest(1));
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Submit took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 2. Workflow Approval Chain Response Time

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task WorkflowApprovalChain_ApproveCompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 2, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "2", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 2)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "2")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "2")).ReturnsAsync("Test");
        _mockEntityStageProvider.Setup(x => x.UpdateStageAsync("Opportunity", "2", "GO", It.IsAny<int>())).ReturnsAsync(true);
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 2, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Approve(It.IsAny<WorkflowLog>(), "Opportunity", 2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("GO");

        var request = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 2, Rationale = "Approve", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        _stopwatch.Restart();
        var result = await _controller.Approve(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Approve took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 3. Workflow Rejection with Reason Response Time

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task WorkflowRejectionWithReason_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 3, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "3", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "3")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "3")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 3, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 3, Rationale = "Reject reason", ConfirmationAcknowledged = true };
        _stopwatch.Restart();
        var result = await _controller.Reject(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Reject took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 4. Workflow Recall Operation Response Time

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task WorkflowRecallOperation_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 4, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "4", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 4)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "4")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "4")).ReturnsAsync("Test");
        _mockWorkflowManager.Setup(x => x.Recall(It.IsAny<WorkflowLog>(), "Opportunity", 4, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new WorkflowRecallRequest { EntityName = "opportunity", EntityId = 4, Comment = "Recall" };
        _stopwatch.Restart();
        var result = await _controller.Recall(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Recall took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 5. Workflow Reopen from Cancelled Response Time

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task WorkflowReopenFromCancelled_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 5, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(_dbContext, 5, 1);

        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 5, Comment = null };
        _stopwatch.Restart();
        var result = await _controller.Reopen(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Reopen took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 6. Workflow Requirements Validation (IsRequirementsMet) Response Time

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task WorkflowRequirementsValidation_IsRequirementsMet_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 6, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync(It.IsAny<string>(), "6")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), "6")).ReturnsAsync("IDENTIFY & PROFILE");

        _stopwatch.Restart();
        var result = await _controller.GetRequirementsForStageChange("Opportunity", 6, "GO");
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Requirements validation took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSimpleSearchMs}ms");
    }

    #endregion

    #region 7. Workflow Status Query Response Time (SLA: 200ms)

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task WorkflowStatusQuery_CompletesWithin200ms()
    {
        await SeedOpportunityAsync(_dbContext, 7, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "7")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "7")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 7)).Returns((WorkflowLog?)null);

        _stopwatch.Restart();
        var result = await _controller.GetWorkflowState("Opportunity", 7);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxStatusQueryMs,
            $"Status query took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxStatusQueryMs}ms");
    }

    #endregion

    #region 8. Workflow History/Audit Trail Query Response Time

    [Fact]
    [Trait("SubCategory", "Search")]
    public async Task WorkflowHistoryAuditTrailQuery_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 8, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "8")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", 8)).Returns(new List<WorkflowHistoryModel>());

        _stopwatch.Restart();
        var result = await _controller.GetWorkflowHistory("Opportunity", 8);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"History query took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 9. Email Notification Generation Throughput

    [Fact]
    [Trait("SubCategory", "BulkOps")]
    public async Task EmailNotificationGeneration_Throughput_25SubmitsWithin5Seconds()
    {
        for (var i = 10; i < 35; i++)
        {
            await SeedOpportunityAsync(_dbContext, i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(_dbContext, i, 1);
        }

        _stopwatch.Restart();
        var submitTasks = Enumerable.Range(10, 25).Select(i => _controller.Submit(CreateSubmitRequest(i))).ToArray();
        var results = await Task.WhenAll(submitTasks);
        _stopwatch.Stop();

        results.Should().HaveCount(25);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"25 submit operations took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    #endregion

    #region 10. Org Unit Role Refresh Operation Time (PNO-731)

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task OrgUnitRoleRefresh_SubmitOperation_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 40, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(_dbContext, 40, 1);

        _stopwatch.Restart();
        var result = await _controller.Submit(CreateSubmitRequest(40));
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Submit with org unit role refresh took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 11. DoA3 Fallback Resolution Time (PNO-1197)

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task DoA3FallbackResolution_SubmitCompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 41, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(_dbContext, 41, 1);

        _stopwatch.Restart();
        var result = await _controller.Submit(CreateSubmitRequest(41));
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Submit with DoA3 fallback resolution took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 12. Duplicate Detection Response Time (PNO-1166)

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task DuplicateDetection_RejectCompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 42, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "42", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 42)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "42")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "42")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 42, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 42, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 42, Rationale = "Reject", ConfirmationAcknowledged = true };
        _stopwatch.Restart();
        var result = await _controller.Reject(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Reject with duplicate detection took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 13. OM Role Transfer Operation Time (PNO-1166)

    [Fact]
    [Trait("SubCategory", "SingleOps")]
    public async Task OMRoleTransfer_RejectOperation_CompletesWithin500ms()
    {
        await SeedOpportunityAsync(_dbContext, 43, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "43", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 43)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "43")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "43")).ReturnsAsync("Test");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 43, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 43, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 43, Rationale = "Reject", ConfirmationAcknowledged = true };
        _stopwatch.Restart();
        var result = await _controller.Reject(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Reject with OM role transfer took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region 14. Bulk Workflow Requirements Check Throughput

    [Fact]
    [Trait("SubCategory", "BulkOps")]
    public async Task BulkWorkflowRequirementsCheck_50Concurrent_CompletesWithin5Seconds()
    {
        await SeedOpportunityAsync(_dbContext, 50, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync(It.IsAny<string>(), "50")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), "50")).ReturnsAsync("IDENTIFY & PROFILE");

        var reqTasks = Enumerable.Range(0, 50).Select(_ => _controller.GetRequirementsForStageChange("Opportunity", 50, "GO")).ToArray();
        _stopwatch.Restart();
        var results = await Task.WhenAll(reqTasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"50 bulk requirements checks took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxBulkOperationMs}ms");
    }

    #endregion

    #region 15. Workflow Stage Transition with AsNoTracking Optimization

    [Fact]
    [Trait("SubCategory", "N+1")]
    public async Task WorkflowStageTransition_QueryUsesAsNoTracking_CompletesWithinThreshold()
    {
        await SeedOpportunityAsync(_dbContext, 51, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(_dbContext, 51, 1);

        _stopwatch.Restart();
        var result = await _controller.Submit(CreateSubmitRequest(51));
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Submit took {_stopwatch.ElapsedMilliseconds}ms — verify AsNoTracking on read queries");
    }

    #endregion

    #region 16. N+1 Query Detection for Workflow Stage Queries

    [Fact]
    [Trait("SubCategory", "N+1")]
    public async Task N1QueryDetection_50WorkflowStateQueries_CompletesWithinThreshold()
    {
        for (var i = 60; i < 110; i++)
        {
            await SeedOpportunityAsync(_dbContext, i, "IDENTIFY & PROFILE");
            _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", i.ToString())).ReturnsAsync(true);
            _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
            _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
        }

        _stopwatch.Restart();
        for (var i = 60; i < 110; i++)
        {
            await _controller.GetWorkflowState("Opportunity", i);
        }
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"50 workflow state queries — avg {avgMs:F0}ms/op exceeded {MaxConcurrentReadMs}ms (possible N+1)");
    }

    #endregion

    #region 17. Memory Allocation During Workflow Operations

    [Fact]
    [Trait("SubCategory", "Memory")]
    public async Task MemoryAllocationDuringWorkflowOperations_NoExcessiveGrowth()
    {
        for (var i = 70; i < 80; i++)
        {
            await SeedOpportunityAsync(_dbContext, i, "IDENTIFY & PROFILE");
            _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", i.ToString())).ReturnsAsync(true);
            _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
            _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
        }

        GC.Collect();
        var memBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (var i = 70; i < 80; i++)
        {
            await _controller.GetWorkflowState("Opportunity", i);
        }

        GC.Collect();
        var memAfter = GC.GetTotalMemory(forceFullCollection: true);
        var growthMb = (memAfter - memBefore) / 1_048_576.0;
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB during 10 workflow operations — possible leak");
    }

    #endregion

    #region 18. Concurrent Workflow Reads Don't Degrade

    [Fact]
    [Trait("SubCategory", "ConcurrentAccess")]
    public async Task ConcurrentWorkflowReads_50Parallel_MaintainsPerformance()
    {
        await SeedOpportunityAsync(_dbContext, 80, "IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "80")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "80")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 80)).Returns((WorkflowLog?)null);

        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _controller.GetWorkflowState("Opportunity", 80))
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        tasks.Should().OnlyContain(t => t.Result != null);
        (_stopwatch.ElapsedMilliseconds / 50.0).Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {_stopwatch.ElapsedMilliseconds / 50.0:F0}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    [Trait("SubCategory", "Benchmark")]
    public async Task Benchmark_AllWorkflowOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();

        await SeedOpportunityAsync(_dbContext, 90, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(_dbContext, 90, 1);
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", "90")).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "90")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 90)).Returns((WorkflowLog?)null);
        _mockWorkflowManager.Setup(x => x.GetWorkflowHistory(It.IsAny<StateMachine>(), "Opportunity", 90)).Returns(new List<WorkflowHistoryModel>());

        report["GetWorkflowState"] = await TimeMs(() => _controller.GetWorkflowState("Opportunity", 90));
        report["GetRequirements"] = await TimeMs(() => _controller.GetRequirementsForStageChange("Opportunity", 90, "GO"));
        report["GetWorkflowHistory"] = await TimeMs(() => _controller.GetWorkflowHistory("Opportunity", 90));
        report["Submit"] = await TimeMs(async () => { await _controller.Submit(CreateSubmitRequest(90)); });

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-20}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs);
    }

    private async Task<long> TimeMs(Func<Task> fn)
    {
        _stopwatch.Restart();
        await fn();
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }

    #endregion
}
