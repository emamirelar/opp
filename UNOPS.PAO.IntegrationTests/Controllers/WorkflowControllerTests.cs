using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Unit tests for WorkflowController.
/// Tests API endpoints for workflow operations.
/// </summary>
public class WorkflowControllerTests : IDisposable
{
    private readonly Mock<ILogger<WorkflowController>> _mockLogger;
    private readonly Mock<IAuthorizationService> _mockAuthService;
    private readonly Mock<IWorkflowManager> _mockWorkflowManager;
    private readonly Mock<IEntityStageProvider> _mockEntityStageProvider;
    private readonly Mock<IPaoWorkflowApproverProvider> _mockApproverProvider;
    private readonly AppDbContext _dbContext;
    private readonly WorkflowController _controller;
    private readonly UserResolverService<int> _userResolverService;

    public WorkflowControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        
        // Setup authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        _userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _dbContext = new AppDbContext(options, _userResolverService, mockDbContextSchema.Object);

        // Setup mocks
        _mockLogger = new Mock<ILogger<WorkflowController>>();
        _mockAuthService = new Mock<IAuthorizationService>();
        _mockWorkflowManager = new Mock<IWorkflowManager>();
        _mockEntityStageProvider = new Mock<IEntityStageProvider>();
        _mockApproverProvider = new Mock<IPaoWorkflowApproverProvider>();

        // Create controller
        _controller = new WorkflowController(
            _mockLogger.Object,
            _mockAuthService.Object,
            _userResolverService,
            _mockWorkflowManager.Object,
            _mockEntityStageProvider.Object,
            _mockApproverProvider.Object,
            _dbContext);

        // Set HttpContext on controller
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region GetWorkflowStages Tests

    [Fact]
    public void GetWorkflowStages_ForOpportunity_ReturnsStageList()
    {
        // Act
        var result = _controller.GetWorkflowStages("opportunity");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stages = okResult.Value as IEnumerable<WorkflowStageConfigResponse>;
        stages.Should().NotBeNull();
        stages.Should().HaveCount(3);
    }

    [Fact]
    public void GetWorkflowStages_ForUnsupportedEntity_Returns404()
    {
        // Act
        var result = _controller.GetWorkflowStages("unsupported");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Theory]
    [InlineData("Opportunity")]
    [InlineData("OPPORTUNITY")]
    [InlineData("opportunity")]
    public void GetWorkflowStages_CaseInsensitive_ReturnsStageList(string entityName)
    {
        // Act
        var result = _controller.GetWorkflowStages(entityName);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    #endregion

    #region GetWorkflowState Tests

    [Fact]
    public async Task GetWorkflowState_WithValidOpportunity_ReturnsState()
    {
        // Arrange
        var entityId = 1;
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", entityId))
            .Returns((WorkflowLogModel?)null);
        _mockWorkflowManager.Setup(x => x.WorkflowStateByStage(
                It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal))
            .Returns(new WorkflowStateModel { Stage = "IDENTIFY & PROFILE" });
        _mockWorkflowManager.Setup(x => x.NextActions(
                "opportunity", It.IsAny<WorkflowStateModel>(), Facing.Internal))
            .Returns(new List<WorkflowStateActionModel>());

        // Act
        var result = await _controller.GetWorkflowState("opportunity", entityId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var state = okResult.Value as WorkflowStateResponse;
        state.Should().NotBeNull();
        state!.CurrentStage.Should().Be("IDENTIFY & PROFILE");
        state.IsInWorkflow.Should().BeFalse();
    }

    [Fact]
    public async Task GetWorkflowState_WithNonExistentEntity_Returns404()
    {
        // Arrange
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "999"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetWorkflowState("opportunity", 999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWorkflowState_WithUnsupportedEntity_Returns404()
    {
        // Act
        var result = await _controller.GetWorkflowState("unsupported", 1);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWorkflowState_WithPendingWorkflow_ReturnsInWorkflowTrue()
    {
        // Arrange
        var entityId = 1;
        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = entityId,
            NewStage = "GO",
            CreatedBy = 1,
            CreatedOn = DateTime.UtcNow
        };

        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", entityId))
            .Returns(pendingTask);
        _mockWorkflowManager.Setup(x => x.WorkflowStateByStage(
                It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal))
            .Returns(new WorkflowStateModel { Stage = "IDENTIFY & PROFILE" });

        // Act
        var result = await _controller.GetWorkflowState("opportunity", entityId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var state = okResult.Value as WorkflowStateResponse;
        state.Should().NotBeNull();
        state!.IsInWorkflow.Should().BeTrue();
        state.PendingStage.Should().Be("GO");
    }

    #endregion

    #region GetWorkflowDetails Tests

    [Fact]
    public async Task GetWorkflowDetails_WithValidOpportunity_ReturnsDetails()
    {
        // Arrange
        var entityId = 1;
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", entityId))
            .Returns((WorkflowLogModel?)null);

        // Act
        var result = await _controller.GetWorkflowDetails("opportunity", entityId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var details = okResult.Value as WorkflowDetailsResponse;
        details.Should().NotBeNull();
        details!.CurrentStage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task GetWorkflowDetails_WithNonExistentEntity_Returns404()
    {
        // Arrange
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "999"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetWorkflowDetails("opportunity", 999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Submit Tests

    [Fact]
    public async Task Submit_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            Comment = "Submitting for approval"
        };

        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("opportunity", "1"))
            .ReturnsAsync("Test Opportunity");
        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns((WorkflowLogModel?)null);
        _mockWorkflowManager.Setup(x => x.WorkflowStateByStage(
                It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal))
            .Returns(new WorkflowStateModel { Stage = "IDENTIFY & PROFILE" });
        _mockWorkflowManager.Setup(x => x.NextActions(
                "opportunity", It.IsAny<WorkflowStateModel>(), Facing.Internal))
            .Returns(new List<WorkflowStateActionModel>
            {
                new WorkflowStateActionModel 
                { 
                    TargetStage = "GO",
                    CommentRequired = false,
                    CommentOptional = true
                }
            });
        _mockWorkflowManager.Setup(x => x.ApprovalNeeded("opportunity", "IDENTIFY & PROFILE", "GO"))
            .Returns(true);
        _mockWorkflowManager.Setup(x => x.Initiate(
                It.IsAny<UNOPS.Workflow.Models.WorkflowActionModel>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Submit(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.ApprovalRequired.Should().BeTrue();
    }

    [Fact]
    public async Task Submit_WithEntityAlreadyInWorkflow_Returns400()
    {
        // Arrange
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO"
        };

        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO"
        };

        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns(pendingTask);

        // Act
        var result = await _controller.Submit(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Submit_WithInvalidTransition_Returns400()
    {
        // Arrange
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "INVALID_STAGE"
        };

        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns((WorkflowLogModel?)null);
        _mockWorkflowManager.Setup(x => x.WorkflowStateByStage(
                It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal))
            .Returns(new WorkflowStateModel { Stage = "IDENTIFY & PROFILE" });
        _mockWorkflowManager.Setup(x => x.NextActions(
                "opportunity", It.IsAny<WorkflowStateModel>(), Facing.Internal))
            .Returns(new List<WorkflowStateActionModel>()); // No valid actions

        // Act
        var result = await _controller.Submit(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Submit_WithNonExistentEntity_Returns404()
    {
        // Arrange
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 999,
            NewStage = "GO"
        };

        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "999"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Submit(request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Approve Tests

    [Fact]
    public async Task Approve_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new WorkflowActionRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Approved"
        };

        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            OldStage = "IDENTIFY & PROFILE"
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("opportunity", "1"))
            .ReturnsAsync("Test Opportunity");
        _mockEntityStageProvider.Setup(x => x.UpdateStageAsync("opportunity", "1", "GO", It.IsAny<int>()))
            .ReturnsAsync(true);
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync(
                "opportunity", 1, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO"))
            .ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Approve(
                pendingTask, "opportunity", 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("GO");

        // Act
        var result = await _controller.Approve(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Approve_WithNoPendingWorkflow_Returns400()
    {
        // Arrange
        var request = new WorkflowActionRequest
        {
            EntityName = "opportunity",
            EntityId = 1
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns((WorkflowLogModel?)null);

        // Act
        var result = await _controller.Approve(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Approve_WithUnauthorizedUser_Returns403()
    {
        // Arrange
        var request = new WorkflowActionRequest
        {
            EntityName = "opportunity",
            EntityId = 1
        };

        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO"
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync(
                "opportunity", 1, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Approve(request);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    #endregion

    #region Reject Tests

    [Fact]
    public async Task Reject_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new WorkflowActionRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Rejecting due to missing information"
        };

        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO"
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("opportunity", "1"))
            .ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("opportunity", "1"))
            .ReturnsAsync("Test Opportunity");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync(
                "opportunity", 1, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO"))
            .ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(
                pendingTask, "opportunity", 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Reject(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Reject_WithoutComment_Returns400()
    {
        // Arrange
        var request = new WorkflowActionRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = null // Comment is required for reject
        };

        // Act
        var result = await _controller.Reject(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reject_WithNoPendingWorkflow_Returns400()
    {
        // Arrange
        var request = new WorkflowActionRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Rejecting"
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns((WorkflowLogModel?)null);

        // Act
        var result = await _controller.Reject(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Recall Tests

    [Fact]
    public async Task Recall_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new WorkflowRecallRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            Comment = "Recalling for updates"
        };

        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            CreatedBy = 1 // Same as current user
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("opportunity", "1"))
            .ReturnsAsync("Test Opportunity");
        _mockWorkflowManager.Setup(x => x.Recall(
                pendingTask, "opportunity", 1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Recall(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Recall_WithNoPendingWorkflow_Returns400()
    {
        // Arrange
        var request = new WorkflowRecallRequest
        {
            EntityName = "opportunity",
            EntityId = 1
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns((WorkflowLogModel?)null);

        // Act
        var result = await _controller.Recall(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Recall_WithDifferentUser_Returns403()
    {
        // Arrange
        var request = new WorkflowRecallRequest
        {
            EntityName = "opportunity",
            EntityId = 1
        };

        var pendingTask = new WorkflowLogModel
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            CreatedBy = 999 // Different user
        };

        _mockWorkflowManager.Setup(x => x.PendingTask("opportunity", 1))
            .Returns(pendingTask);

        // Act
        var result = await _controller.Recall(request);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    #endregion

    #region GetWorkflowHistory Tests

    [Fact]
    public async Task GetWorkflowHistory_WithValidOpportunity_ReturnsHistory()
    {
        // Arrange
        var entityId = 1;
        var historyEntries = new List<WorkflowHistoryModel>
        {
            new WorkflowHistoryModel
            {
                OldStage = "IDENTIFY & PROFILE",
                NewStage = "GO",
                Action = "Approved",
                CompletedBy = 1,
                CompletedOn = DateTime.UtcNow,
                Comment = "Approved"
            }
        };

        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "1"))
            .ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.GetWorkflowHistory(
                It.IsAny<StateMachine>(), "opportunity", entityId))
            .Returns(historyEntries);

        // Act
        var result = await _controller.GetWorkflowHistory("opportunity", entityId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var history = okResult.Value as List<WorkflowHistoryResponse>;
        history.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWorkflowHistory_WithNonExistentEntity_Returns404()
    {
        // Arrange
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("opportunity", "999"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetWorkflowHistory("opportunity", 999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetWorkflowHistory_WithUnsupportedEntity_Returns404()
    {
        // Act
        var result = await _controller.GetWorkflowHistory("unsupported", 1);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
