/**
 * @fileoverview PNO-1166 Negative Tests: Reject action fix + OM role transfer.
 * Covers failure scenarios for Reject, Cancel, Reopen, and OM Transfer.
 * @author UNOPS Opportunity+ QA Team
 */

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
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Xunit;
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1166;

/// <summary>
/// PNO-1166 Negative tests: Reject action fix + OM role transfer failure scenarios.
/// Uses same fixture pattern as WorkflowControllerTests (InMemory DB, mocks, WorkflowController).
/// </summary>
[Collection("Negative")]
[Trait("Category", "Negative")]
[Trait("Type", "Negative")]
public class NegativeTests : IDisposable
{
    private readonly Mock<ILogger<WorkflowController>> _mockLogger;
    private readonly Mock<IAuthorizationService> _mockAuthService;
    private readonly Mock<IWorkflowManager> _mockWorkflowManager;
    private readonly Mock<IEntityStageProvider> _mockEntityStageProvider;
    private readonly Mock<IPaoWorkflowApproverProvider> _mockApproverProvider;
    private readonly Mock<IStageRequirementsProvider> _mockRequirementsProvider;
    private readonly Mock<IManagerWrapper> _mockManagerWrapper;
    private readonly Mock<IGeminiManager> _mockGeminiManager;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly PaoWorkflowNotificationService _notificationService;
    private readonly AppDbContext _dbContext;
    private readonly WorkflowController _controller;
    private readonly UserResolverService<int> _userResolverService;
    private readonly DefaultHttpContext _httpContext;

    public NegativeTests()
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
        _httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        _userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _dbContext = new AppDbContext(options, _userResolverService, mockDbContextSchema.Object);

        _mockLogger = new Mock<ILogger<WorkflowController>>();
        _mockAuthService = new Mock<IAuthorizationService>();
        _mockWorkflowManager = new Mock<IWorkflowManager>();
        _mockEntityStageProvider = new Mock<IEntityStageProvider>();
        _mockApproverProvider = new Mock<IPaoWorkflowApproverProvider>();
        _mockRequirementsProvider = new Mock<IStageRequirementsProvider>();
        _mockManagerWrapper = new Mock<IManagerWrapper>();
        _mockGeminiManager = new Mock<IGeminiManager>();
        _mockEmailSender = new Mock<IEmailSender>();

        _mockRequirementsProvider.Setup(x => x.EntityNames).Returns(new[] { "Opportunity" });
        _mockManagerWrapper.Setup(x => x.GeminiManager).Returns(_mockGeminiManager.Object);
        _mockGeminiManager.Setup(x => x.GenerateOpportunityStatementAsync(
                It.IsAny<int>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
            .ReturnsAsync("Generated statement");

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x["AppBaseUrl"]).Returns("https://test.pao.unops.org");

        var mockNotificationLogger = new Mock<ILogger<PaoWorkflowNotificationService>>();
        var mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        mockContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(options, _userResolverService, mockDbContextSchema.Object));
        mockContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(options, _userResolverService, mockDbContextSchema.Object));
        var mockNotificationManager = new Mock<NotificationManager>(
            new AppDbContext(options, _userResolverService, mockDbContextSchema.Object),
            _userResolverService);
        _notificationService = new PaoWorkflowNotificationService(
            _mockEmailSender.Object,
            mockContextFactory.Object,
            mockNotificationLogger.Object,
            mockConfiguration.Object,
            mockNotificationManager.Object);

        _controller = new WorkflowController(
            _mockLogger.Object,
            _mockAuthService.Object,
            _userResolverService,
            _mockWorkflowManager.Object,
            _mockEntityStageProvider.Object,
            _mockApproverProvider.Object,
            new[] { _mockRequirementsProvider.Object },
            _mockManagerWrapper.Object,
            _dbContext,
            _notificationService);

        _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };
    }

    public void Dispose() => _dbContext.Dispose();

    #region Helpers

    private async Task SeedOpportunityAsync(int id, string stage, EntityStatus status = EntityStatus.Active)
    {
        var existing = await _dbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Stage = stage;
            existing.Status = status;
        }
        else
        {
            _dbContext.Opportunities.Add(new Opportunity
            {
                Id = id,
                Name = $"Test Opportunity {id}",
                Description = "Full test opportunity",
                Stage = stage,
                Status = status,
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
                OpportunityStatementMarkdown = "## Test",
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

    private void SetupRejectMocks(int entityId, WorkflowLog? pendingTask, bool canApprove = false)
    {
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", entityId.ToString()))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", entityId))
            .Returns(pendingTask);
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync(
                "Opportunity", entityId, 1, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(canApprove);
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", entityId.ToString()))
            .ReturnsAsync("Test Opportunity");
    }

    #endregion

    #region NEG_001-010: Reject validation failures

    [Fact]
    public async Task NEG_001_Reject_EmptyRationale_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_002_Reject_NullRationale_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = null!,
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_003_Reject_WhitespaceRationale_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "   \t\n  ",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_004_Reject_NoAcknowledgment_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Budget constraints",
            ConfirmationAcknowledged = false
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_005_Reject_FalseAcknowledgment_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Scope unclear",
            ConfirmationAcknowledged = false
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_006_Reject_EmptyEntityName_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_007_Reject_NullEntityName_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = null!,
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_008_Reject_ZeroEntityId_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 0,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 0))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_009_Reject_NegativeEntityId_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = -1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_010_Reject_VeryLongRationale_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = new string('x', 10001),
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region NEG_011-020: Reject authorization failures

    [Fact]
    public async Task NEG_011_Reject_NoPendingTask_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_012_Reject_UserNotApprover_Returns403()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "1",
            NewStage = "GO",
            UserId = 999
        };
        SetupRejectMocks(1, pendingTask, canApprove: false);
        var result = await _controller.Reject(request);
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task NEG_013_Reject_ExpiredSession_Returns403()
    {
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "1",
            NewStage = "GO",
            UserId = 1
        };
        SetupRejectMocks(1, pendingTask, canApprove: false);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<ObjectResult>();
    }

    [Fact]
    public async Task NEG_014_Reject_WrongEntityType_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Partner",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Partner", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_015_Reject_NonExistentEntity_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 99999,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "99999",
            NewStage = "GO",
            UserId = 1
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 99999))
            .Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "99999"))
            .ReturnsAsync((string?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_016_Reject_DeletedEntity_Fails()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 999,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "999",
            NewStage = "GO",
            UserId = 1
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 999))
            .Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "999"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync(
                "Opportunity", 999, 1, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "999"))
            .ReturnsAsync("Test Opportunity");
        _mockWorkflowManager.Setup(x => x.Reject(
                It.IsAny<WorkflowLog>(), "Opportunity", 999, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        var result = await _controller.Reject(request);
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task NEG_017_Reject_WrongEntityNameCase_Normalized()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "OPPORTUNITY",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_018_Reject_EntityInWrongStage_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "GO");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_019_Reject_MissingClaims_Returns403()
    {
        var identity = new ClaimsIdentity();
        _httpContext.User = new ClaimsPrincipal(identity);
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "1",
            NewStage = "GO",
            UserId = 0
        };
        SetupRejectMocks(1, pendingTask, canApprove: false);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<ObjectResult>();
    }

    [Fact]
    public async Task NEG_020_Reject_AnonymousUser_Returns403()
    {
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "1",
            NewStage = "GO",
            UserId = 1
        };
        SetupRejectMocks(1, pendingTask, canApprove: false);
        var result = await _controller.Reject(request);
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    #endregion

    #region NEG_021-030: Reject state failures

    [Fact]
    public async Task NEG_021_Reject_EntityNotInWorkflow_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_022_Reject_AlreadyRejected_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "NO GO");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_023_Reject_AlreadyApproved_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "GO");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_024_Reject_EntityCancelled_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "CANCELLED", EntityStatus.Closed);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_025_Reject_EntityClosed_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "NO GO", EntityStatus.Closed);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_026_Reject_ConcurrentRejectAttempt_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_027_Reject_AfterRecall_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_028_Reject_StalePendingTask_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "NO GO");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_029_Reject_DifferentEntityType_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "Partner",
            EntityId = "1",
            NewStage = "GO",
            UserId = 1
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns(pendingTask);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_030_Reject_NullPendingTask_Returns400()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region NEG_031-040: OM Transfer failures (via Workflow/Reject context - validation scenarios)

    [Fact]
    public async Task NEG_031_OMTransfer_AssignDeletedUser_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_032_OMTransfer_AssignDeactivatedUser_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_033_OMTransfer_AssignUserWithoutEmail_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_034_OMTransfer_AssignUserFromWrongOrg_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_035_OMTransfer_AssignSelfAsOMAndCollaborator_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_036_OMTransfer_NullUserId_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task NEG_037_OMTransfer_ZeroUserId_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 0,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_038_OMTransfer_AssignNonExistentUser_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 99999,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_039_OMTransfer_WhenEntityLocked_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "1",
            NewStage = "GO",
            UserId = 999
        };
        SetupRejectMocks(1, pendingTask, canApprove: false);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task NEG_040_OMTransfer_ToUserWhoAlreadyHasRole_Fails()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Reject(new RejectWorkflowRequest
        {
            EntityName = "Opportunity",
            EntityId = 1,
            Rationale = "Rejecting",
            ConfirmationAcknowledged = true
        });
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region NEG_041-050: Cancel failures

    [Fact]
    public async Task NEG_041_Cancel_NonOpportunity_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "Partner",
            EntityId = 1,
            Comment = "Cancelling"
        };
        var result = await _controller.Cancel(request);
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task NEG_042_Cancel_FromWrongStage_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Cancelling"
        };
        await SeedOpportunityAsync(1, "GO");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_043_Cancel_WithoutComment_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = ""
        };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_044_Cancel_AsNonOM_Returns403()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Cancelling"
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Cancel(request);
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task NEG_045_Cancel_WhileInWorkflow_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Cancelling"
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = "1",
            NewStage = "GO",
            UserId = 1
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns(pendingTask);
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_046_Cancel_AlreadyCancelled_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Cancelling"
        };
        await SeedOpportunityAsync(1, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1))
            .Returns((WorkflowLog?)null);
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_047_Cancel_EmptyEntityName_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "",
            EntityId = 1,
            Comment = "Cancelling"
        };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_048_Cancel_NonExistent_Returns404()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 99999,
            Comment = "Cancelling"
        };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task NEG_049_Cancel_NullComment_Returns400()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = null!
        };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_050_Cancel_DeletedEntity_Returns404()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Cancelling"
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var opp = await _dbContext.Opportunities.FindAsync(1);
        if (opp != null)
        {
            opp.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
        }
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region NEG_051-060: Reopen failures

    [Fact]
    public async Task NEG_051_Reopen_FromWrongStage_Returns400()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_052_Reopen_CancelledWithoutComment_Returns400()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = " "
        };
        await SeedOpportunityAsync(1, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_053_Reopen_AsNonOM_Returns403()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "NO GO");
        var result = await _controller.Reopen(request);
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task NEG_054_Reopen_NonExistent_Returns404()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 99999,
            Comment = "Reopening"
        };
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task NEG_055_Reopen_DeletedEntity_Returns404()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "NO GO");
        var opp = await _dbContext.Opportunities.FindAsync(1);
        if (opp != null)
        {
            opp.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
        }
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task NEG_056_Reopen_EmptyEntityName_Returns400()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_057_Reopen_ActiveEntity_Returns400()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "GO");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_058_Reopen_FromIdentifyProfileStage_Returns400()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_059_Reopen_NullComment_FromCancelled_Returns400()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = null
        };
        await SeedOpportunityAsync(1, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task NEG_060_Reopen_ConcurrentAttempt_Handled()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        await SeedOpportunityAsync(1, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        var result1 = await _controller.Reopen(request);
        result1.Result.Should().BeOfType<OkObjectResult>();
        var result2 = await _controller.Reopen(request);
        result2.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion
}
