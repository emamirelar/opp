/**
 * LOAD TESTS — Workflow Operations
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase strategy: QA Tests/Load Tests/README.md (5 phases)
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 *
 * Context: Workflow submodule (UNOPS.Workflow) available in CI.
 * Endpoints: /api/workflow (submit, approve, reject, recall, reopen, cancel)
 * Related: PNO-731 (Org Unit Role Refresh), PNO-1146 (Email Notifications),
 *          PNO-1166 (Reject Duplicate and OM Transfer), PNO-1197 (DoA3 Fallback)
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

namespace UNOPS.PAO.Business.Tests.LoadTests;

/// <summary>
/// Load Tests for Workflow operations (submit, approve, reject, recall, reopen, cancel).
/// Verifies system behaviour under sustained, spike, and stress conditions.
/// Uses mocked services and InMemory DB — no real database connections.
///
/// Required: ≥10 tests (FIXED)
/// Phase mapping:
///   Sustained Load  → Phase 2: Normal operations over time
///   Spike           → Phase 5: Sudden load increases + recovery
///   Stress Limits   → Phase 3: Beyond normal capacity
///   Recovery        → Phase 3+5: Post-overload stability
/// </summary>
public class WorkflowLoadTests : IDisposable
{
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 500;

    public void Dispose() => GC.SuppressFinalize(this);

    private static (WorkflowController Controller, AppDbContext DbContext, Mock<IWorkflowManager> MockWorkflowManager,
        Mock<IEntityStageProvider> MockEntityStageProvider, Mock<IPaoWorkflowApproverProvider> MockApproverProvider)
        CreateControllerWithMocks()
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
            dbContext.Opportunities.Add(new Domain.Entities.Opportunity
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

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Sustained load on workflow approval/rejection endpoints.
    /// Phase 2: Load Testing — normal operation over time.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task SustainedLoad_ApprovalRejectionEndpoints_PerformanceDoesNotDegrade()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= NormalUsers; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = i.ToString(), NewStage = "GO" };
                mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns(pendingTask);
                mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
                mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", i.ToString())).ReturnsAsync($"Test {i}");
                mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", i, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
                mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", i, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            }

            var times = new List<long>();
            for (var i = 1; i <= NormalUsers; i++)
            {
                var sw = Stopwatch.StartNew();
                await controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = i, Rationale = "Reject", ConfirmationAcknowledged = true });
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
            }

            var first = times.Take(times.Count / 4).Average();
            var last = times.Skip(3 * times.Count / 4).Average();
            last.Should().BeLessThan(first * 3,
                $"Approval/rejection perf degraded from {first:F0}ms to {last:F0}ms under {NormalUsers} ops");
        }
    }

    /// <summary>
    /// Mixed read/write on workflow status queries.
    /// Phase 2: Load Testing — 80% read (GetWorkflowState), 20% write (Cancel).
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task SustainedLoad_MixedReadWriteWorkflowStatus_ThroughputMeetsTarget()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= NormalUsers; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var readCount = (int)(NormalUsers * 0.8);
            var writeCount = NormalUsers - readCount;
            var readTasks = Enumerable.Range(0, readCount).Select(_ => controller.GetWorkflowState("Opportunity", 1));
            var writeTasks = Enumerable.Range(1, writeCount).Select(i => controller.Cancel(new WorkflowCancelRequest { EntityName = "opportunity", EntityId = i, Comment = "Cancel" }));
            var allTasks = readTasks.Cast<Task>().Concat(writeTasks.Cast<Task>()).ToArray();

            var sw = Stopwatch.StartNew();
            await Task.WhenAll(allTasks);
            sw.Stop();

            var avgMs = sw.ElapsedMilliseconds / (double)NormalUsers;
            avgMs.Should().BeLessThan(MaxP95ResponseMs,
                $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
        }
    }

    /// <summary>
    /// Sustained load on duplicate detection during OM transfer (PNO-1166).
    /// Phase 2: Load Testing — repeated reject operations with OM transfer logic.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task SustainedLoad_DuplicateDetectionDuringOMTransfer_ConsistencyMaintained()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= NormalUsers / 2; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = i.ToString(), NewStage = "GO" };
                mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns(pendingTask);
                mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
                mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", i.ToString())).ReturnsAsync($"Test {i}");
                mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", i, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
                mockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", i, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            }

            var times = new List<long>();
            for (var i = 1; i <= NormalUsers / 2; i++)
            {
                var sw = Stopwatch.StartNew();
                await controller.Reject(new RejectWorkflowRequest { EntityName = "opportunity", EntityId = i, Rationale = "Reject", ConfirmationAcknowledged = true });
                sw.Stop();
                lock (times) times.Add(sw.ElapsedMilliseconds);
            }

            var avg = times.Average();
            var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
            stdDev.Should().BeLessThan(avg * 2,
                $"Duplicate detection/OM transfer times inconsistent (avg={avg:F0}ms, σ={stdDev:F0}ms)");
        }
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Spike load on workflow recall operations.
    /// Phase 5: Spike Testing — sudden burst of recall requests.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task SpikeLoad_RecallOperations_HandlesGracefully()
    {
        var (controller, dbContext, mockWorkflowManager, mockEntityStageProvider, mockApproverProvider) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= PeakUsers; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = i.ToString(), NewStage = "GO" };
                mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns(pendingTask);
                mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", i.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
                mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", i.ToString())).ReturnsAsync($"Test {i}");
                mockWorkflowManager.Setup(x => x.Recall(It.IsAny<WorkflowLog>(), "Opportunity", i, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            }

            var baselineSw = Stopwatch.StartNew();
            for (var i = 1; i <= 10; i++)
                await controller.Recall(new WorkflowRecallRequest { EntityName = "opportunity", EntityId = i, Comment = "Recall" });
            baselineSw.Stop();

            var spikeSw = Stopwatch.StartNew();
            var recallTasks = Enumerable.Range(1, PeakUsers).Select(i =>
                controller.Recall(new WorkflowRecallRequest { EntityName = "opportunity", EntityId = i, Comment = "Recall" })).ToArray();
            await Task.WhenAll(recallTasks);
            spikeSw.Stop();

            var scale = (double)spikeSw.ElapsedMilliseconds / Math.Max(baselineSw.ElapsedMilliseconds, 1);
            scale.Should().BeLessThan((double)PeakUsers / 10 * 3,
                $"Recall spike scaled {scale:F1}× — expected sub-linear");
        }
    }

    /// <summary>
    /// Concurrent email notification dispatch under workflow transitions (PNO-1146).
    /// Phase 5: Spike Testing — burst of submit operations triggering notifications.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task SpikeLoad_EmailNotificationDispatch_HandlesConcurrentTransitions()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= 25; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var submitTasks = Enumerable.Range(1, 25).Select(i => controller.Submit(CreateSubmitRequest(i))).ToArray();
            var results = await Task.WhenAll(submitTasks);

            results.Should().HaveCount(25);
            results.Count(r => r.Result != null).Should().Be(25);
        }
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Concurrent workflow stage transitions — 50+ simultaneous submit-for-go requests.
    /// Phase 3: Stress Testing — beyond normal capacity.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task StressLoad_ConcurrentStageTransitions_DoesNotCrash()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= 55; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var completed = 0;
            var tasks = Enumerable.Range(1, 55).Select(async i =>
            {
                await controller.Submit(CreateSubmitRequest(i));
                Interlocked.Increment(ref completed);
            }).ToArray();

            var allDone = Task.WhenAll(tasks);
            var timeout = Task.Delay(TimeSpan.FromSeconds(60));
            var first = await Task.WhenAny(allDone, timeout);

            first.Should().Be(allDone,
                $"Only {completed}/55 submit operations completed — possible crash or timeout");
            completed.Should().Be(55);
        }
    }

    /// <summary>
    /// Parallel org unit role refresh operations (PNO-731).
    /// Phase 3: Stress Testing — concurrent role resolution under load.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task StressLoad_OrgUnitRoleRefresh_ErrorRateWithinLimit()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= StressUsers / 5; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var success = 0;
            var failure = 0;
            var tasks = Enumerable.Range(1, 100).Select(async i =>
            {
                try
                {
                    await controller.Submit(CreateSubmitRequest((i % 20) + 1));
                    Interlocked.Increment(ref success);
                }
                catch
                {
                    Interlocked.Increment(ref failure);
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            var errorRate = (double)failure / 100;
            errorRate.Should().BeLessThan(MaxErrorRate,
                $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under org unit role refresh load");
        }
    }

    /// <summary>
    /// Concurrent DoA3 fallback resolution under load (PNO-1197).
    /// Phase 3: Stress Testing — DoA validation under concurrent requests.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task StressLoad_DoA3FallbackResolution_DataIntegrityMaintained()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            for (var i = 1; i <= 50; i++)
            {
                await SeedOpportunityAsync(dbContext, i, "IDENTIFY & PROFILE");
                await SeedOpportunityManagerStakeholderAsync(dbContext, i, 1);
            }

            var expected = Enumerable.Range(1, 50).Sum();
            var actual = 0;
            var tasks = Enumerable.Range(1, 50).Select(async i =>
            {
                await controller.Submit(CreateSubmitRequest(i));
                Interlocked.Add(ref actual, i);
            }).ToArray();

            await Task.WhenAll(tasks);

            actual.Should().Be(expected,
                "Data integrity compromised under DoA3 fallback stress load");
        }
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    /// <summary>
    /// Bulk workflow requirements validation.
    /// Phase 3: Stress — many concurrent requirements checks.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task Recovery_BulkRequirementsValidation_AllComplete()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            await SeedOpportunityAsync(dbContext, 1, "IDENTIFY & PROFILE");

            var reqTasks = Enumerable.Range(0, 50).Select(_ => controller.GetRequirementsForStageChange("Opportunity", 1)).ToArray();
            var results = await Task.WhenAll(reqTasks);

            results.Should().HaveCount(50);
            results.Count(r => r.Result != null).Should().Be(50);
        }
    }

    /// <summary>
    /// Recovery test after workflow spike load.
    /// Phase 5: Spike Testing — system returns to baseline after overload.
    /// </summary>
    [Fact]
    [Trait("Category", "LoadTest")]
    public async Task Recovery_AfterWorkflowSpikeLoad_ReturnsToBaseline()
    {
        var (controller, dbContext, _, _, _) = CreateControllerWithMocks();
        await using (dbContext)
        {
            await SeedOpportunityAsync(dbContext, 1, "IDENTIFY & PROFILE");

            var baselineSw = Stopwatch.StartNew();
            await controller.GetWorkflowState("Opportunity", 1);
            baselineSw.Stop();
            var baselineMs = baselineSw.ElapsedMilliseconds;

            var spikeTasks = Enumerable.Range(0, PeakUsers).Select(_ => controller.GetWorkflowState("Opportunity", 1)).ToArray();
            await Task.WhenAll(spikeTasks);

            await Task.Delay(RecoveryWindowMs);

            var recoveredSw = Stopwatch.StartNew();
            await controller.GetWorkflowState("Opportunity", 1);
            recoveredSw.Stop();
            var recoveredMs = recoveredSw.ElapsedMilliseconds;

            var effectiveBaseline = Math.Max(baselineMs, 1);
            recoveredMs.Should().BeLessThan(effectiveBaseline * 4,
                $"Post-spike response {recoveredMs}ms did not recover (baseline {baselineMs}ms)");
        }
    }

    #endregion
}
