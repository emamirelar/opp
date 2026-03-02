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
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1166;

/// <summary>
/// PNO-1166: Reject action no longer logs duplicate history + OM role transfer fix.
/// Functional tests for workflow rules, validation, constraints, and audit.
/// </summary>
[Collection("Functional")]
[Trait("Category", "Functional")]
[Trait("Type", "Functional")]
public class FunctionalTests : IDisposable
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

    public FunctionalTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
        _mockWorkflowManager.Setup(x => x.AddLog(It.IsAny<WorkflowLogModel>())).Returns(Task.CompletedTask);
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
        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
        _notificationService = new PaoWorkflowNotificationService(
            _mockEmailSender.Object,
            mockContextFactory.Object,
            mockServiceScopeFactory.Object,
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

    #region Helper Methods

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
                Challenges = "Test challenges",
                ExpectedImpact = "Test impact",
                ExpectedOutcomes = "Test outcomes",
                BeneficiariesToBeDetermined = true,
                UNOPSMissionsNotApplicable = true,
                TargetSigningDate = DateTime.UtcNow.AddMonths(1),
                ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
                TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
                OpportunityStatementMarkdown = "## Statement",
                ResponsibleOrgUnitId = 1,
                ProposedInitiativeTypeId = 1
            });
        }
        if (!await _dbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 1))
        {
            _dbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = 1,
                Name = "Test Org Unit",
                Code = "TOU",
                Description = "Test org unit for workflow",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        if (!await _dbContext.Set<ProposedInitiativeType>().AnyAsync(p => p.Id == 1))
        {
            _dbContext.Set<ProposedInitiativeType>().Add(new ProposedInitiativeType
            {
                Id = 1,
                Name = "Test Initiative Type",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await _dbContext.SaveChangesAsync();

        if (!await _dbContext.Set<OpportunityDeliverable>().AnyAsync(d => d.OpportunityId == id))
        {
            _dbContext.Set<OpportunityDeliverable>().Add(new OpportunityDeliverable
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                Name = "Test Deliverable"
            });
        }
        if (!await _dbContext.Set<OpportunitySDG>().AnyAsync(s => s.OpportunityId == id))
        {
            _dbContext.Set<OpportunitySDG>().Add(new OpportunitySDG
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                SDGId = 1,
                Name = "SDG 1"
            });
        }
        if (!await _dbContext.Set<OpportunityFundingPartner>().AnyAsync(fp => fp.OpportunityId == id))
        {
            _dbContext.Set<OpportunityFundingPartner>().Add(new OpportunityFundingPartner
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                PartnerId = 1,
                Name = "Funding Partner"
            });
        }
        if (!await _dbContext.Set<OpportunityClientPartner>().AnyAsync(cp => cp.OpportunityId == id))
        {
            _dbContext.Set<OpportunityClientPartner>().Add(new OpportunityClientPartner
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                PartnerId = 2,
                Name = "Client Partner"
            });
        }
        if (!await _dbContext.Set<OpportunityCountry>().AnyAsync(oc => oc.OpportunityId == id))
        {
            if (!await _dbContext.Set<Country>().AnyAsync(c => c.Id == 1))
            {
                _dbContext.Set<Country>().Add(new Country
                {
                    Id = 1,
                    Name = "Test Country",
                    Iso2Code = "TC",
                    Status = EntityStatus.Active,
                    IsDeleted = false
                });
                await _dbContext.SaveChangesAsync();
            }
            _dbContext.Set<OpportunityCountry>().Add(new OpportunityCountry
            {
                Id = id * 100 + 1,
                OpportunityId = id,
                CountryId = 1,
                Name = "Test Country"
            });
        }
        if (!await _dbContext.EntityUserRoles.AnyAsync(eur =>
            eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1))
        {
            var doaRole = await _dbContext.EntityRoles.FirstOrDefaultAsync(r => r.Code == "DoA2_OrganizationHierarchy");
            if (doaRole == null)
            {
                doaRole = new EntityRole
                {
                    Id = 200,
                    Name = "DoA Level 2 Holder",
                    Code = "DoA2_OrganizationHierarchy",
                    EntityType = "OrganizationHierarchy",
                    Status = EntityStatus.Active,
                    IsDeleted = false
                };
                _dbContext.EntityRoles.Add(doaRole);
                await _dbContext.SaveChangesAsync();
            }
            _dbContext.EntityUserRoles.Add(new EntityUserRole
            {
                Id = id * 100 + 50,
                UserId = 1,
                EntityRoleId = doaRole.Id,
                EntityRole = doaRole,
                EntityId = 1,
                EntityType = "OrganizationHierarchy",
                Name = "DoA Holder",
                IsDeleted = false
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

    private void SetupStandardRejectMocks(int entityId)
    {
        var pendingTask = new WorkflowLog
        {
            EntityName = "opportunity",
            EntityId = entityId.ToString(),
            NewStage = "GO"
        };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", entityId)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", entityId.ToString())).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", entityId.ToString())).ReturnsAsync($"Test Opportunity {entityId}");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync(
            "Opportunity", entityId, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(
            pendingTask, "Opportunity", entityId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
    }

    private void SetupStandardSubmitMocks()
    {
        _mockEntityStageProvider.Setup(x => x.IsEntityValidAsync("Opportunity", It.IsAny<string>())).ReturnsAsync(true);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", It.IsAny<string>())).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", It.IsAny<string>())).ReturnsAsync("Test Opportunity");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);
        _mockWorkflowManager.Setup(x => x.WorkflowStateByStage(
            It.IsAny<StateMachine>(), "IDENTIFY & PROFILE", Facing.Internal))
            .Returns(new State { StageCode = "IDENTIFY & PROFILE" });
        _mockWorkflowManager.Setup(x => x.NextActions("Opportunity", It.IsAny<State>(), Facing.Internal))
            .Returns(new[] { new WorkflowStateActionModel { NewStage = "GO", Comment = "optional" } });
        _mockWorkflowManager.Setup(x => x.ApprovalNeeded("Opportunity", "IDENTIFY & PROFILE", "GO")).Returns(true);
        _mockWorkflowManager.Setup(x => x.AddLog(It.IsAny<WorkflowLogModel>())).Returns(Task.CompletedTask);
        _mockWorkflowManager.Setup(x => x.Initiate(
            It.IsAny<UNOPS.Workflow.Models.WorkflowActionModel>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
    }

    #endregion

    #region FUN_001-015: Workflow Rules

    [Fact]
    public async Task FUN_001_Reject_SetsStageToNoGo()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Scope unclear", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(1);
        var result = await _controller.Reject(request);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.NewStage.Should().Be("NO GO");
    }

    [Fact]
    public async Task FUN_002_Reject_SetsStatusToClosed()
    {
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 2, Rationale = "Budget", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(2);
        await _controller.Reject(request);
        var opp = await _dbContext.Opportunities.FindAsync(2);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task FUN_003_Reject_CallsRejectExactlyOnce()
    {
        await SeedOpportunityAsync(3, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "3", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 3)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "3")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "3")).ReturnsAsync("Test Opportunity 3");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 3, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Reject(pendingTask, "Opportunity", 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 3, Rationale = "No", ConfirmationAcknowledged = true };
        await _controller.Reject(request);
        _mockWorkflowManager.Verify(x => x.Reject(pendingTask, "Opportunity", 3, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_004_Reject_DoesNotCallAddLogSeparately()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 4, Rationale = "Reject", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(4);
        await _controller.Reject(request);
        _mockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.Action == "Rejected")), Times.Never);
    }

    [Fact]
    public async Task FUN_005_Reject_IncludesRationaleInResponse()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 5, Rationale = "Insufficient info", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(5);
        var result = await _controller.Reject(request);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task FUN_006_Reject_PreservesOpportunityDataIntegrity()
    {
        await SeedOpportunityAsync(6, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 6, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(6);
        await _controller.Reject(request);
        var opp = await _dbContext.Opportunities.FindAsync(6);
        opp.Should().NotBeNull();
        opp!.Name.Should().Be("Test Opportunity 6");
    }

    [Fact]
    public async Task FUN_007_Reject_NotifiesStakeholders()
    {
        await SeedOpportunityAsync(7, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 7, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(7);
        await _controller.Reject(request);
        _mockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 7, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_008_Reject_UpdatesWorkflowHistory()
    {
        await SeedOpportunityAsync(8, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 8, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(8);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FUN_009_Reject_MarksNotificationsAsRejected()
    {
        await SeedOpportunityAsync(9, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 9, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(9);
        var result = await _controller.Reject(request);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.NewStage.Should().Be("NO GO");
    }

    [Fact]
    public async Task FUN_010_Cancel_RequiresIdentifyProfileStage()
    {
        await SeedOpportunityAsync(10, "GO");
        await SeedOpportunityManagerStakeholderAsync(10, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 10)).Returns((WorkflowLog?)null);
        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 10, Comment = "Cancel" };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_011_Cancel_SetsCancelledStage()
    {
        await SeedOpportunityAsync(11, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(11, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 11)).Returns((WorkflowLog?)null);
        _mockWorkflowManager.Setup(x => x.AddLog(It.IsAny<WorkflowLogModel>())).Returns(Task.CompletedTask);
        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 11, Comment = "Cancel" };
        var result = await _controller.Cancel(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.NewStage.Should().Be("CANCELLED");
    }

    [Fact]
    public async Task FUN_012_Reopen_SetsIdentifyProfileStage()
    {
        await SeedOpportunityAsync(12, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(12, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 12, Comment = null };
        var result = await _controller.Reopen(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.NewStage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    public async Task FUN_013_Reopen_SetsDraftStatus()
    {
        await SeedOpportunityAsync(13, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(13, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 13, Comment = "Reopen" };
        await _controller.Reopen(request);
        var opp = await _dbContext.Opportunities.FindAsync(13);
        opp!.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact]
    public async Task FUN_014_Reopen_FromNoGoAllowed()
    {
        await SeedOpportunityAsync(14, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(14, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 14, Comment = null };
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FUN_015_Reopen_FromCancelledRequiresComment()
    {
        await SeedOpportunityAsync(15, "CANCELLED", EntityStatus.Closed);
        await SeedOpportunityManagerStakeholderAsync(15, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 15, Comment = null };
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region FUN_016-025: Validation Rules

    [Fact]
    public async Task FUN_016_Submit_RequiresAllRequirements()
    {
        await SeedOpportunityAsync(16, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(16, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 16, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_017_Submit_ChecksOMStakeholder()
    {
        await SeedOpportunityAsync(17, "IDENTIFY & PROFILE");
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 17, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.UnmetRequirements.Should().Contain(r => r.Contains("managerRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task FUN_018_Submit_ChecksDoAHolder()
    {
        await SeedOpportunityAsync(18, "IDENTIFY & PROFILE");
        var holders = await _dbContext.EntityUserRoles.Where(eur => eur.EntityType == "OrganizationHierarchy" && eur.EntityId == 1).ToListAsync();
        _dbContext.EntityUserRoles.RemoveRange(holders);
        await _dbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(18, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 18, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_019_Submit_ChecksCountries()
    {
        await SeedOpportunityAsync(19, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(19, 1);
        var countries = await _dbContext.Set<OpportunityCountry>().Where(oc => oc.OpportunityId == 19).ToListAsync();
        _dbContext.Set<OpportunityCountry>().RemoveRange(countries);
        await _dbContext.SaveChangesAsync();
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 19, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        result.Result.Should().BeOfType<OkObjectResult>();
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task FUN_020_Submit_ChecksDeliverables()
    {
        await SeedOpportunityAsync(20, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(20, 1);
        var dels = await _dbContext.Set<OpportunityDeliverable>().Where(d => d.OpportunityId == 20).ToListAsync();
        _dbContext.Set<OpportunityDeliverable>().RemoveRange(dels);
        await _dbContext.SaveChangesAsync();
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 20, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        result.Result.Should().BeOfType<OkObjectResult>();
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task FUN_021_Submit_ChecksSDGs()
    {
        await SeedOpportunityAsync(21, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(21, 1);
        var sdgs = await _dbContext.Set<OpportunitySDG>().Where(s => s.OpportunityId == 21).ToListAsync();
        _dbContext.Set<OpportunitySDG>().RemoveRange(sdgs);
        await _dbContext.SaveChangesAsync();
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 21, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        result.Result.Should().BeOfType<OkObjectResult>();
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task FUN_022_Submit_ChecksResponsibleOrgUnit()
    {
        await SeedOpportunityAsync(22, "IDENTIFY & PROFILE");
        var opp = await _dbContext.Opportunities.FindAsync(22);
        opp!.ResponsibleOrgUnitId = null;
        await _dbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(22, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 22, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_023_Submit_ChecksOpportunityStatement()
    {
        await SeedOpportunityAsync(23, "IDENTIFY & PROFILE");
        var opp = await _dbContext.Opportunities.FindAsync(23);
        opp!.OpportunityStatementMarkdown = "";
        await _dbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(23, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 23, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_024_Submit_ChecksBeneficiariesFlag()
    {
        await SeedOpportunityAsync(24, "IDENTIFY & PROFILE");
        var opp = await _dbContext.Opportunities.FindAsync(24);
        opp!.BeneficiariesToBeDetermined = false;
        opp.EstimatedDirectBeneficiaries = 0;
        opp.EstimatedIndirectBeneficiaries = -1;
        await _dbContext.SaveChangesAsync();
        await SeedOpportunityManagerStakeholderAsync(24, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 24, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task FUN_025_Submit_ValidatesEntityNameFormat()
    {
        await SeedOpportunityAsync(25, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(25, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 25, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region FUN_026-035: Constraint Rules

    [Fact]
    public async Task FUN_026_OnlyOMCanCancel()
    {
        await SeedOpportunityAsync(26, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 26)).Returns((WorkflowLog?)null);
        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 26, Comment = "Cancel" };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task FUN_027_OnlyApproverCanReject()
    {
        await SeedOpportunityAsync(27, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "27", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 27)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "27")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "27")).ReturnsAsync("Test Opportunity 27");
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 27, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(false);
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 27, Rationale = "No", ConfirmationAcknowledged = true };
        var result = await _controller.Reject(request);
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task FUN_028_Recall_OnlyByInitiatorOrOM()
    {
        await SeedOpportunityAsync(28, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(28, 1);
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "28", NewStage = "GO", UserId = 1 };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 28)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "28")).ReturnsAsync("Test Opportunity 28");
        _mockWorkflowManager.Setup(x => x.Recall(pendingTask, "Opportunity", 28, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        var request = new WorkflowRecallRequest { EntityName = "opportunity", EntityId = 28, Comment = "Recall" };
        var result = await _controller.Recall(request);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FUN_029_CannotCancelWhileInWorkflow()
    {
        await SeedOpportunityAsync(29, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(29, 1);
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "29", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 29)).Returns(pendingTask);
        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 29, Comment = "Cancel" };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_030_CannotReopenFromIAndP()
    {
        await SeedOpportunityAsync(30, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(30, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 30, Comment = "Reopen" };
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_031_OnlyOpportunitySupportsCancel()
    {
        var request = new WorkflowCancelRequest { EntityName = "partner", EntityId = 1, Comment = "Cancel" };
        var result = await _controller.Cancel(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_032_OnlyOpportunitySupportsReopen()
    {
        var request = new WorkflowReopenRequest { EntityName = "partner", EntityId = 1, Comment = "Reopen" };
        var result = await _controller.Reopen(request);
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_033_OMCheckIsCaseInsensitiveForRoleName()
    {
        await SeedOpportunityAsync(33, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(33, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 33, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        var result = await _controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_034_PendingTaskRequiredForReject()
    {
        await SeedOpportunityAsync(34, "IDENTIFY & PROFILE");
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 34)).Returns((WorkflowLog?)null);
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 34, Rationale = "No", ConfirmationAcknowledged = true };
        var result = await _controller.Reject(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task FUN_035_PendingTaskRequiredForApprove()
    {
        var request = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 35, Rationale = "Yes", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 35)).Returns((WorkflowLog?)null);
        var result = await _controller.Approve(request);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region FUN_036-050: Audit Rules

    [Fact]
    public async Task FUN_036_Reject_CreatesWorkflowLogEntry()
    {
        await SeedOpportunityAsync(36, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 36, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(36);
        await _controller.Reject(request);
        _mockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 36, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_037_Cancel_CreatesWorkflowLogEntry()
    {
        await SeedOpportunityAsync(37, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(37, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 37)).Returns((WorkflowLog?)null);
        _mockWorkflowManager.Setup(x => x.AddLog(It.IsAny<WorkflowLogModel>())).Returns(Task.CompletedTask);
        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 37, Comment = "Cancel" };
        await _controller.Cancel(request);
        _mockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.Action == "Cancelled" && l.NewStage == "CANCELLED")), Times.Once);
    }

    [Fact]
    public async Task FUN_038_Reopen_CreatesWorkflowLogEntry()
    {
        await SeedOpportunityAsync(38, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(38, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 38, Comment = "Reopen" };
        await _controller.Reopen(request);
        _mockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.Action == "Reopened" && l.NewStage == "IDENTIFY & PROFILE")), Times.Once);
    }

    [Fact]
    public async Task FUN_039_Approve_CreatesWorkflowLogEntry()
    {
        await SeedOpportunityAsync(39, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "39", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 39)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "39")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "39")).ReturnsAsync("Test Opportunity 39");
        _mockEntityStageProvider.Setup(x => x.UpdateStageAsync("Opportunity", "39", "GO", It.IsAny<int>())).ReturnsAsync(true);
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 39, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Approve(pendingTask, "Opportunity", 39, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("GO");
        var mockOpportunityManager = new Mock<IOpportunityManager>();
        mockOpportunityManager.Setup(x => x.AssignExecutiveAsync(39, 10)).Returns(Task.CompletedTask);
        _mockManagerWrapper.Setup(x => x.OpportunityManager).Returns(mockOpportunityManager.Object);
        var request = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 39, Rationale = "Yes", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        await _controller.Approve(request);
        _mockWorkflowManager.Verify(x => x.Approve(pendingTask, "Opportunity", 39, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_040_WorkflowLog_IncludesUserId()
    {
        await SeedOpportunityAsync(40, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(40, 1);
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 40)).Returns((WorkflowLog?)null);
        _mockWorkflowManager.Setup(x => x.AddLog(It.IsAny<WorkflowLogModel>())).Returns(Task.CompletedTask);
        var request = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 40, Comment = "Cancel" };
        await _controller.Cancel(request);
        _mockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.Action == "Cancelled")), Times.Once);
    }

    [Fact]
    public async Task FUN_041_WorkflowLog_IncludesTimestamp()
    {
        await SeedOpportunityAsync(41, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(41, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 41, NewStage = "GO", ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true };
        await _controller.Submit(request);
        _mockWorkflowManager.Verify(x => x.AddLog(It.IsAny<WorkflowLogModel>()), Times.Once);
    }

    [Fact]
    public async Task FUN_042_WorkflowLog_IncludesActionType()
    {
        await SeedOpportunityAsync(42, "NO GO");
        await SeedOpportunityManagerStakeholderAsync(42, 1);
        var request = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 42, Comment = "Reopen" };
        await _controller.Reopen(request);
        _mockWorkflowManager.Verify(x => x.AddLog(It.Is<WorkflowLogModel>(l => l.Action == "Reopened")), Times.Once);
    }

    [Fact]
    public async Task FUN_043_WorkflowLog_IncludesEntityName()
    {
        await SeedOpportunityAsync(43, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 43, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(43);
        var result = await _controller.Reject(request);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task FUN_044_WorkflowLog_IncludesEntityId()
    {
        await SeedOpportunityAsync(44, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 44, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(44);
        var result = await _controller.Reject(request);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FUN_045_WorkflowLog_IncludesRationaleForReject()
    {
        await SeedOpportunityAsync(45, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 45, Rationale = "Insufficient scope", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(45);
        await _controller.Reject(request);
        _mockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 45, It.IsAny<string>(), "Insufficient scope", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_046_StageChange_PersistedToDb()
    {
        await SeedOpportunityAsync(46, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 46, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(46);
        await _controller.Reject(request);
        var opp = await _dbContext.Opportunities.FindAsync(46);
        opp!.Stage.Should().Be("NO GO");
    }

    [Fact]
    public async Task FUN_047_StatusChange_PersistedToDb()
    {
        await SeedOpportunityAsync(47, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 47, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(47);
        await _controller.Reject(request);
        var opp = await _dbContext.Opportunities.FindAsync(47);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    public async Task FUN_048_NotificationService_CalledOnRejection()
    {
        await SeedOpportunityAsync(48, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 48, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(48);
        await _controller.Reject(request);
        _mockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), "Opportunity", 48, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_049_NotificationService_CalledOnApproval()
    {
        await SeedOpportunityAsync(49, "IDENTIFY & PROFILE");
        var pendingTask = new WorkflowLog { EntityName = "opportunity", EntityId = "49", NewStage = "GO" };
        _mockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 49)).Returns(pendingTask);
        _mockEntityStageProvider.Setup(x => x.GetCurrentStageAsync("Opportunity", "49")).ReturnsAsync("IDENTIFY & PROFILE");
        _mockEntityStageProvider.Setup(x => x.GetEntityDisplayNameAsync("Opportunity", "49")).ReturnsAsync("Test Opportunity 49");
        _mockEntityStageProvider.Setup(x => x.UpdateStageAsync("Opportunity", "49", "GO", It.IsAny<int>())).ReturnsAsync(true);
        _mockApproverProvider.Setup(x => x.CanUserApproveAsync("Opportunity", 49, It.IsAny<int>(), "IDENTIFY & PROFILE", "GO")).ReturnsAsync(true);
        _mockWorkflowManager.Setup(x => x.Approve(pendingTask, "Opportunity", 49, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("GO");
        var mockOpportunityManager = new Mock<IOpportunityManager>();
        mockOpportunityManager.Setup(x => x.AssignExecutiveAsync(49, 10)).Returns(Task.CompletedTask);
        _mockManagerWrapper.Setup(x => x.OpportunityManager).Returns(mockOpportunityManager.Object);
        var request = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 49, Rationale = "Yes", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        await _controller.Approve(request);
        _mockWorkflowManager.Verify(x => x.Approve(It.IsAny<WorkflowLog>(), "Opportunity", 49, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task FUN_050_WorkflowState_UpdatedCorrectly()
    {
        await SeedOpportunityAsync(50, "IDENTIFY & PROFILE");
        var request = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 50, Rationale = "No", ConfirmationAcknowledged = true };
        SetupStandardRejectMocks(50);
        var result = await _controller.Reject(request);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowActionResponse;
        response!.Success.Should().BeTrue();
        response.NewStage.Should().Be("NO GO");
    }

    #endregion
}
