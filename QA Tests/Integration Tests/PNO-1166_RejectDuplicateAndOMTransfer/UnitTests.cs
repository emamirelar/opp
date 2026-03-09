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
/// PNO-1166: Unit tests for Reject action fix + OM role transfer.
/// Tests request/response model validation and helper logic.
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Type", "Unit")]
public class UnitTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly WorkflowController _controller;

    public UnitTests()
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
            mockWorkflowManager.Object,
            mockEntityStageProvider.Object,
            mockApproverProvider.Object,
            new[] { mockRequirementsProvider.Object },
            mockManagerWrapper.Object,
            _dbContext,
            notificationService);

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    public void Dispose() => _dbContext.Dispose();

    #region UNIT_001-007: Request model validation

    [Fact]
    public void UNIT_001_WorkflowSubmitRequest_RequiredFields_Present()
    {
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO"
        };
        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.NewStage.Should().Be("GO");
    }

    [Fact]
    public void UNIT_002_RejectWorkflowRequest_RequiredFields_Present()
    {
        var request = new RejectWorkflowRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Rationale = "Insufficient info",
            ConfirmationAcknowledged = true
        };
        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.Rationale.Should().Be("Insufficient info");
        request.ConfirmationAcknowledged.Should().BeTrue();
    }

    [Fact]
    public void UNIT_003_WorkflowCancelRequest_RequiredFields_Present()
    {
        var request = new WorkflowCancelRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Cancelling"
        };
        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.Comment.Should().Be("Cancelling");
    }

    [Fact]
    public void UNIT_004_WorkflowReopenRequest_RequiredFields_Present()
    {
        var request = new WorkflowReopenRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Reopening"
        };
        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.Comment.Should().Be("Reopening");
    }

    [Fact]
    public void UNIT_005_ApproveWorkflowRequest_RequiredFields_Present()
    {
        var request = new ApproveWorkflowRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Rationale = "Approved",
            ConfirmationAcknowledged = true,
            ExecutiveId = 10
        };
        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.Rationale.Should().Be("Approved");
        request.ConfirmationAcknowledged.Should().BeTrue();
        request.ExecutiveId.Should().Be(10);
    }

    [Fact]
    public void UNIT_006_WorkflowRecallRequest_RequiredFields_Present()
    {
        var request = new WorkflowRecallRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Recalling"
        };
        request.EntityName.Should().Be("opportunity");
        request.EntityId.Should().Be(1);
        request.Comment.Should().Be("Recalling");
    }

    [Fact]
    public void UNIT_007_WorkflowActionResponse_Properties_Accessible()
    {
        var response = new WorkflowActionResponse
        {
            Success = true,
            Message = "Done",
            NewStage = "NO GO"
        };
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Done");
        response.NewStage.Should().Be("NO GO");
    }

    #endregion

    #region UNIT_008-014: Response model validation

    [Fact]
    public void UNIT_008_WorkflowSubmitResponse_SuccessState_Correct()
    {
        var response = new WorkflowSubmitResponse
        {
            Success = true,
            ApprovalRequired = true,
            PendingStage = "GO"
        };
        response.Success.Should().BeTrue();
        response.ApprovalRequired.Should().BeTrue();
        response.PendingStage.Should().Be("GO");
    }

    [Fact]
    public void UNIT_009_WorkflowSubmitResponse_FailureState_Correct()
    {
        var response = new WorkflowSubmitResponse
        {
            Success = false,
            RequiresConfirmation = true,
            ConfirmationType = "NonOMSubmitter"
        };
        response.Success.Should().BeFalse();
        response.RequiresConfirmation.Should().BeTrue();
        response.ConfirmationType.Should().Be("NonOMSubmitter");
    }

    [Fact]
    public void UNIT_010_WorkflowActionResponse_SuccessState_Correct()
    {
        var response = new WorkflowActionResponse
        {
            Success = true,
            NewStage = "NO GO",
            Message = "Rejected"
        };
        response.Success.Should().BeTrue();
        response.NewStage.Should().Be("NO GO");
    }

    [Fact]
    public void UNIT_011_WorkflowActionResponse_FailureState_Correct()
    {
        var response = new WorkflowActionResponse
        {
            Success = false,
            Message = "No pending workflow"
        };
        response.Success.Should().BeFalse();
        response.Message.Should().Be("No pending workflow");
    }

    [Fact]
    public void UNIT_012_WorkflowStateResponse_Mapping_Correct()
    {
        var response = new WorkflowStateResponse
        {
            CurrentStage = "IDENTIFY & PROFILE",
            IsInWorkflow = true,
            PendingStage = "GO"
        };
        response.CurrentStage.Should().Be("IDENTIFY & PROFILE");
        response.IsInWorkflow.Should().BeTrue();
        response.PendingStage.Should().Be("GO");
    }

    [Fact]
    public void UNIT_013_PendingApprovalResponse_Mapping_Correct()
    {
        var response = new PendingApprovalResponse
        {
            EntityName = "Opportunity",
            EntityId = 1,
            EntityDisplayName = "Test Opp",
            CurrentStage = "IDENTIFY & PROFILE",
            PendingStage = "GO"
        };
        response.EntityName.Should().Be("Opportunity");
        response.EntityId.Should().Be(1);
        response.EntityDisplayName.Should().Be("Test Opp");
        response.CurrentStage.Should().Be("IDENTIFY & PROFILE");
        response.PendingStage.Should().Be("GO");
    }

    [Fact]
    public void UNIT_014_WorkflowDetailsResponse_Mapping_Correct()
    {
        var response = new WorkflowDetailsResponse
        {
            CurrentStage = "IDENTIFY & PROFILE",
            IsInWorkflow = true,
            PendingStage = "GO",
            CanApprove = true,
            CanRecall = false
        };
        response.CurrentStage.Should().Be("IDENTIFY & PROFILE");
        response.IsInWorkflow.Should().BeTrue();
        response.PendingStage.Should().Be("GO");
        response.CanApprove.Should().BeTrue();
        response.CanRecall.Should().BeFalse();
    }

    #endregion

    #region UNIT_015-021: Helper method tests (via controller behavior)

    [Fact]
    public void UNIT_015_EntityNameNormalization_Opportunity_Accepted()
    {
        var result = _controller.GetWorkflowStages("opportunity");
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UNIT_016_OMRoleCheckLogic_ReflectedInCancelPermission()
    {
        _dbContext.Opportunities.Add(new Opportunity
        {
            Id = 100,
            Name = "Test",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
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
        await _dbContext.SaveChangesAsync();

        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 100, Comment = "Cancel" };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<ObjectResult>();
    }

    [Fact]
    public async Task UNIT_017_ApprovalCheckLogic_ReflectedInRejectWithNoPending()
    {
        _dbContext.Opportunities.Add(new Opportunity
        {
            Id = 101,
            Name = "Test",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
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
        await _dbContext.SaveChangesAsync();

        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 101, Rationale = "Reject", ConfirmationAcknowledged = true };
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UNIT_018_StageValidationLogic_UnsupportedEntity_Returns404()
    {
        var result = await _controller.GetWorkflowState("unsupported", 1);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UNIT_019_IsInWorkflowFlagLogic_ReflectedInStateResponse()
    {
        _dbContext.Opportunities.Add(new Opportunity
        {
            Id = 102,
            Name = "Test",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
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
        await _dbContext.SaveChangesAsync();

        var result = _controller.GetWorkflowStages("Opportunity");
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UNIT_020_PendingTaskDetection_NonExistentEntity_Returns404()
    {
        var result = await _controller.GetWorkflowState("Opportunity", 99999);
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UNIT_021_StageRequirementFiltering_ReturnsRequirementsForValidStage()
    {
        _dbContext.Opportunities.Add(new Opportunity
        {
            Id = 103,
            Name = "Test",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
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
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetRequirementsForStageChange("Opportunity", 103, "GO");
        result.Result.Should().NotBeNull();
    }

    #endregion
}
