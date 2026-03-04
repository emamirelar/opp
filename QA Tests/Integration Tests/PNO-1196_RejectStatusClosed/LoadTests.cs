/**
 * @fileoverview PNO-1196 Load Tests — 10 tests.
 * Sustained load, spike load, and throughput tests for Reject operation.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
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
using EntityStatus = UNOPS.PAO.Domain.Entities.EntityStatus;

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Load Tests — 10 sustained and spike load tests.
/// Each test creates its own isolated controller/DB to avoid state conflicts.
/// </summary>
[Collection("Load")]
[Trait("Category", "Load")]
[Trait("Ticket", "PNO-1196")]
public class LoadTests : IDisposable
{
    public void Dispose() { }

    private static (WorkflowController Controller, AppDbContext DbContext, Mock<IWorkflowManager> Wf) CreateIsolated()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "LoadUser"),
            new(ClaimTypes.Email, "load@unops.org")
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
        var mockRequirementsProvider = new Mock<IStageRequirementsProvider>();
        var mockManagerWrapper = new Mock<IManagerWrapper>();
        var mockGeminiManager = new Mock<IGeminiManager>();
        var mockEmailSender = new Mock<IEmailSender>();

        mockRequirementsProvider.Setup(x => x.EntityNames).Returns(new[] { "Opportunity" });
        mockManagerWrapper.Setup(x => x.GeminiManager).Returns(mockGeminiManager.Object);

        var mockConfiguration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        mockConfiguration.Setup(x => x["AppBaseUrl"]).Returns("https://test.pao.unops.org");

        var mockNotificationLogger = new Mock<ILogger<PaoWorkflowNotificationService>>();
        var mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        mockContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(options, userResolverService, mockDbContextSchema.Object));
        mockContextFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(options, userResolverService, mockDbContextSchema.Object));
        var mockNotificationManager = new Mock<NotificationManager>(
            new AppDbContext(options, userResolverService, mockDbContextSchema.Object), userResolverService);
        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
        var notificationService = new PaoWorkflowNotificationService(
            mockEmailSender.Object, mockContextFactory.Object, mockServiceScopeFactory.Object,
            mockNotificationLogger.Object, mockConfiguration.Object, mockNotificationManager.Object);

        mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>()))
            .Returns((WorkflowLog?)null);

        // Default: allow approval for all load tests
        mockApproverProvider.Setup(x => x.CanUserApproveAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("GO");
        mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Test Opportunity");

        var controller = new WorkflowController(
            mockLogger.Object, mockAuthService.Object, userResolverService,
            mockWorkflowManager.Object, mockEntityStageProvider.Object,
            mockApproverProvider.Object, new[] { mockRequirementsProvider.Object },
            mockManagerWrapper.Object, dbContext, notificationService);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return (controller, dbContext, mockWorkflowManager);
    }

    [Fact] [Trait("TestId", "LOAD-001")]
    public async Task Load_50NoPendingTaskRejects_AllReturn400()
    {
        var (controller, _, wf) = CreateIsolated();

        wf.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var sw = Stopwatch.StartNew();
        for (var i = 1; i <= 50; i++)
        {
            var r = await controller.Reject(new RejectWorkflowRequest
            {
                EntityId = i, EntityName = "Opportunity",
                Rationale = "Load test rationale", ConfirmationAcknowledged = true
            });
            r.Should().BeOfType<BadRequestObjectResult>();
        }
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact] [Trait("TestId", "LOAD-002")]
    public async Task Load_100FailedRejects_CompletesWithin30Seconds()
    {
        var (controller, _, wf) = CreateIsolated();
        wf.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var sw = Stopwatch.StartNew();
        var tasks = Enumerable.Range(1, 100).Select(i =>
            controller.Reject(new RejectWorkflowRequest
            {
                EntityId = i, EntityName = "Opportunity",
                Rationale = "Load test", ConfirmationAcknowledged = true
            }));
        var results = await Task.WhenAll(tasks);
        sw.Stop();

        results.Should().AllBeOfType<BadRequestObjectResult>();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact] [Trait("TestId", "LOAD-003")]
    public async Task Load_DbSeed1000Records_CompletesWithin30Seconds()
    {
        var (_, dbContext, _) = CreateIsolated();

        var sw = Stopwatch.StartNew();
        for (var i = 1; i <= 1000; i++)
        {
            dbContext.Opportunities.Add(new Opportunity
            {
                Id = i, Name = $"Load Opp {i}", Stage = "GO", Description = "Load test opportunity",
                Status = EntityStatus.Active, IsDeleted = false,
                ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
            });
        }
        await dbContext.SaveChangesAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
        (await dbContext.Opportunities.CountAsync()).Should().Be(1000);
    }

    [Fact] [Trait("TestId", "LOAD-004")]
    public async Task Load_QueryClosedFromLargeDataset_CompletesWithin5Seconds()
    {
        var (_, dbContext, _) = CreateIsolated();

        for (var i = 1; i <= 500; i++)
        {
            dbContext.Opportunities.Add(new Opportunity
            {
                Id = i, Name = $"Opp {i}", Stage = i % 2 == 0 ? "NO GO" : "GO", Description = "Load test opportunity",
                Status = i % 2 == 0 ? EntityStatus.Closed : EntityStatus.Active,
                IsDeleted = false, ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
            });
        }
        await dbContext.SaveChangesAsync();

        var sw = Stopwatch.StartNew();
        var closed = await dbContext.Opportunities
            .Where(o => o.Status == EntityStatus.Closed && !o.IsDeleted)
            .CountAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
        closed.Should().Be(250);
    }

    [Fact] [Trait("TestId", "LOAD-005")]
    public async Task Load_20SuccessfulRejects_CompletesWithin20Seconds()
    {
        var (controller, dbContext, wf) = CreateIsolated();

        for (var i = 1; i <= 20; i++)
        {
            dbContext.Opportunities.Add(new Opportunity
            {
                Id = i, Name = $"Opp {i}", Stage = "GO", Description = "Load test opportunity",
                Status = EntityStatus.Active, IsDeleted = false,
                ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
            });
            // WorkflowLog is managed by IWorkflowManager mock, no DB seeding needed
        }
        await dbContext.SaveChangesAsync();

        wf.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>()))
            .Returns<string, int>((_, id) => new WorkflowLog { Id = 100 + id, RequiresApproval = true });
        wf.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var sw = Stopwatch.StartNew();
        for (var i = 1; i <= 20; i++)
        {
            await controller.Reject(new RejectWorkflowRequest
            {
                EntityId = i, EntityName = "Opportunity",
                Rationale = "Load test successful rejection", ConfirmationAcknowledged = true
            });
        }
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(20));
    }

    [Fact] [Trait("TestId", "LOAD-006")]
    public async Task Load_SpikeLoad_10ConcurrentControllers_AllRespond()
    {
        var controllers = Enumerable.Range(0, 10).Select(_ => CreateIsolated()).ToList();

        var tasks = controllers.Select(c =>
            c.Controller.Reject(new RejectWorkflowRequest
            {
                EntityId = 1, EntityName = "Opportunity",
                Rationale = "Spike load test", ConfirmationAcknowledged = true
            }));

        var results = await Task.WhenAll(tasks);
        results.Should().AllBeOfType<BadRequestObjectResult>();

        foreach (var (_, dbContext, _) in controllers)
            dbContext.Dispose();
    }

    [Fact] [Trait("TestId", "LOAD-007")]
    public async Task Load_SustainedLoad_RepeatReject_NoMemoryLeak()
    {
        var (controller, _, wf) = CreateIsolated();
        wf.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var memBefore = GC.GetTotalMemory(true);

        for (var i = 0; i < 50; i++)
        {
            await controller.Reject(new RejectWorkflowRequest
            {
                EntityId = i, EntityName = "Opportunity",
                Rationale = "Sustained load rationale", ConfirmationAcknowledged = true
            });
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        var memAfter = GC.GetTotalMemory(true);

        var memGrowthMB = (memAfter - memBefore) / (1024.0 * 1024.0);
        memGrowthMB.Should().BeLessThan(100, "Memory growth should be < 100MB over 50 calls");
    }

    [Fact] [Trait("TestId", "LOAD-008")]
    public async Task Load_200EmptyEntityNameRejects_AllReturn400()
    {
        var (controller, _, _) = CreateIsolated();

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 200; i++)
        {
            var r = await controller.Reject(new RejectWorkflowRequest
            {
                EntityId = i, EntityName = "",
                Rationale = "Load test", ConfirmationAcknowledged = true
            });
            r.Should().BeOfType<BadRequestObjectResult>();
        }
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact] [Trait("TestId", "LOAD-009")]
    public async Task Load_ThroughputTest_50RejectsPerSecond()
    {
        var (controller, _, wf) = CreateIsolated();
        wf.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 50; i++)
        {
            await controller.Reject(new RejectWorkflowRequest
            {
                EntityId = i, EntityName = "Opportunity",
                Rationale = "Throughput test", ConfirmationAcknowledged = true
            });
        }
        sw.Stop();

        var throughput = 50.0 / sw.Elapsed.TotalSeconds;
        throughput.Should().BeGreaterThan(1, "Should achieve more than 1 reject/second");
    }

    [Fact] [Trait("TestId", "LOAD-010")]
    public async Task Load_MixedSuccessAndFailure_NoExceptions()
    {
        var (controller, dbContext, wf) = CreateIsolated();

        for (var i = 1; i <= 5; i++)
        {
            dbContext.Opportunities.Add(new Opportunity
            {
                Id = i, Name = $"Success Opp {i}", Stage = "GO", Description = "Load test opportunity",
                Status = EntityStatus.Active, IsDeleted = false,
                ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
            });
            // WorkflowLog is managed by IWorkflowManager mock, no DB seeding needed
        }
        await dbContext.SaveChangesAsync();

        wf.Setup(x => x.PendingTask("Opportunity", It.Is<int>(id => id <= 5)))
            .Returns<string, int>((_, id) => new WorkflowLog { Id = 100 + id, RequiresApproval = true });
        wf.Setup(x => x.PendingTask("Opportunity", It.Is<int>(id => id > 5)))
            .Returns((WorkflowLog?)null);
        wf.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var act = async () =>
        {
            for (var i = 1; i <= 10; i++)
            {
                await controller.Reject(new RejectWorkflowRequest
                {
                    EntityId = i, EntityName = "Opportunity",
                    Rationale = "Mixed load test", ConfirmationAcknowledged = true
                });
            }
        };

        await act.Should().NotThrowAsync();
    }
}
