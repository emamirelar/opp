using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for PaoWorkflowNotificationService CC recipient functionality.
/// Tests the email CC recipient logic for workflow approval request notifications.
/// Per PRD "The Go Decision" US-9, CC recipients include:
/// - Opportunity Manager (from stakeholders)
/// - Workflow initiator (if different from OM)
/// - Director/Manager of responsible org unit
/// </summary>
public class PaoWorkflowNotificationServiceCCTests : IDisposable
{
    private readonly AppDbContext _appDbContext;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<PaoWorkflowNotificationService>> _mockLogger;
    private readonly Mock<NotificationManager> _mockNotificationManager;
    private readonly Mock<IDbContextFactory<AppDbContext>> _mockContextFactory;
    private readonly PaoWorkflowNotificationService _notificationService;

    public PaoWorkflowNotificationServiceCCTests()
    {
        // Setup in-memory database for AppDbContext
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _appDbContext = new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object);

        // Setup DbContextFactory mock to return the in-memory context
        _mockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        _mockContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object));
        _mockContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object));

        // Setup mocks
        _mockEmailSender = new Mock<IEmailSender>();
        _mockLogger = new Mock<ILogger<PaoWorkflowNotificationService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["AppConfig:BaseUrl"]).Returns("https://test.unops.org");

        // NotificationManager requires AppDbContext + UserResolverService constructor args
        _mockNotificationManager = new Mock<NotificationManager>(_appDbContext, userResolverService);

        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        _notificationService = new PaoWorkflowNotificationService(
            _mockEmailSender.Object,
            _mockContextFactory.Object,
            mockServiceScopeFactory.Object,
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockNotificationManager.Object);
    }

    public void Dispose()
    {
        _appDbContext.Dispose();
    }

    #region Test Data Setup

    private async Task SeedTestDataAsync()
    {
        // Create users
        var omUser = new PAOUser
        {
            Id = 100,
            Email = "om@unops.org"
        };
        var initiatorUser = new PAOUser
        {
            Id = 101,
            Email = "initiator@unops.org"
        };
        var directorUser = new PAOUser
        {
            Id = 102,
            Email = "director@unops.org"
        };
        var approverUser = new PAOUser
        {
            Id = 103,
            Email = "approver@unops.org"
        };

        await _appDbContext.PAOUsers.AddRangeAsync(omUser, initiatorUser, directorUser, approverUser);

        // Create org unit
        var orgUnit = new OrganizationHierarchy
        {
            Id = 1,
            Name = "Test Org Unit",
            Description = "Test Org Unit Description",
            Code = "TEST-OU",
            IsDeleted = false
        };
        await _appDbContext.OrganizationHierarchies.AddAsync(orgUnit);

        // Create Opportunity Manager entity role
        var omRole = new EntityRole
        {
            Id = 1,
            EntityType = "Opportunity",
            Name = "Opportunity Manager",
            Code = "Opportunity_Manager_Opportunity",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await _appDbContext.EntityRoles.AddAsync(omRole);

        // Create Director entity role for OrgUnit
        var directorRole = new EntityRole
        {
            Id = 2,
            EntityType = "OrganizationHierarchy",
            Name = "OrgUnit Director",
            Code = "OrgUnit_Director_OrganizationHierarchy",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await _appDbContext.EntityRoles.AddAsync(directorRole);

        // Create opportunity
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test opportunity for CC tests",
            ResponsibleOrgUnitId = 1,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await _appDbContext.Opportunities.AddAsync(opportunity);

        // Create OM stakeholder
        var omStakeholder = new OpportunityStakeholder
        {
            Id = 1,
            OpportunityId = 1,
            UserId = 100,
            EntityRoleId = 1, // OM role
            IsInternal = true
        };
        await _appDbContext.OpportunityStakeholders.AddAsync(omStakeholder);

        // Create Director EntityUserRole
        var directorUserRole = new EntityUserRole
        {
            Id = 1,
            Name = "Director Assignment",
            EntityType = "OrganizationHierarchy",
            EntityId = 1,
            EntityRoleId = 2,
            UserId = 102,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await _appDbContext.EntityUserRoles.AddAsync(directorUserRole);

        await _appDbContext.SaveChangesAsync();
    }

    #endregion

    #region NotifyNewApprovalRequestAsync Tests

    [Fact]
    public async Task NotifyNewApprovalRequestAsync_SendsEmailWithCCRecipients()
    {
        // Arrange
        await SeedTestDataAsync();

        var notification = new WorkflowNotification
        {
            EntityName = "Opportunity",
            EntityId = "1",
            EntityDisplayName = "Test Opportunity",
            RecipientUserIds = new List<int> { 103 }, // Approver
            PerformedByUserId = 101, // Initiator (different from OM)
            PerformedByUserName = "Initiator User",
            Timestamp = DateTime.UtcNow,
            Comment = "Please review"
        };

        EmailMessage? capturedEmailMessage = null;
        _mockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((msg, model, url) => capturedEmailMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        await _notificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        capturedEmailMessage.Should().NotBeNull();
        capturedEmailMessage!.EmailReceivers.Should().Contain("approver@unops.org");
        
        // CC should include OM, initiator, and director
        capturedEmailMessage.CcReceivers.Should().NotBeNull();
        capturedEmailMessage.CcReceivers.Should().Contain("om@unops.org", "Opportunity Manager should be in CC");
        capturedEmailMessage.CcReceivers.Should().Contain("initiator@unops.org", "Initiator should be in CC");
        capturedEmailMessage.CcReceivers.Should().Contain("director@unops.org", "Director should be in CC");
    }

    [Fact]
    public async Task NotifyNewApprovalRequestAsync_DoesNotDuplicateCCWhenInitiatorIsOM()
    {
        // Arrange
        await SeedTestDataAsync();

        var notification = new WorkflowNotification
        {
            EntityName = "Opportunity",
            EntityId = "1",
            EntityDisplayName = "Test Opportunity",
            RecipientUserIds = new List<int> { 103 }, // Approver
            PerformedByUserId = 100, // Initiator is same as OM
            PerformedByUserName = "OM User",
            Timestamp = DateTime.UtcNow,
            Comment = "Please review"
        };

        EmailMessage? capturedEmailMessage = null;
        _mockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((msg, model, url) => capturedEmailMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        await _notificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        capturedEmailMessage.Should().NotBeNull();
        
        // OM email should only appear once (no duplicate)
        var omEmailCount = capturedEmailMessage!.CcReceivers.Count(e => e.Equals("om@unops.org", StringComparison.OrdinalIgnoreCase));
        omEmailCount.Should().Be(1, "OM email should not be duplicated when initiator is OM");
    }

    [Fact]
    public async Task NotifyNewApprovalRequestAsync_ReturnsEmptyCCForNonOpportunity()
    {
        // Arrange
        await SeedTestDataAsync();

        var notification = new WorkflowNotification
        {
            EntityName = "Partner", // Not Opportunity
            EntityId = "1",
            EntityDisplayName = "Test Partner",
            RecipientUserIds = new List<int> { 103 },
            PerformedByUserId = 101,
            PerformedByUserName = "Initiator User",
            Timestamp = DateTime.UtcNow,
            Comment = "Please review"
        };

        EmailMessage? capturedEmailMessage = null;
        _mockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((msg, model, url) => capturedEmailMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        await _notificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        capturedEmailMessage.Should().NotBeNull();
        capturedEmailMessage!.CcReceivers.Should().BeEmpty("Non-Opportunity entities should not have CC recipients");
    }

    [Fact]
    public async Task NotifyNewApprovalRequestAsync_HandlesNoOMStakeholder()
    {
        // Arrange
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        
        using var context = new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object);

        // Add minimal test data without OM stakeholder
        var approverUser = new PAOUser { Id = 103, Email = "approver@unops.org" };
        await context.PAOUsers.AddAsync(approverUser);

        var orgUnit = new OrganizationHierarchy { Id = 1, Name = "Test Org", Description = "Test Org", Code = "TEST", IsDeleted = false };
        await context.OrganizationHierarchies.AddAsync(orgUnit);

        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test opportunity for no-OM test",
            ResponsibleOrgUnitId = 1,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await context.Opportunities.AddAsync(opportunity);
        await context.SaveChangesAsync();

        var localContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        localContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object));
        localContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(appOptions, userResolverService, mockDbContextSchema.Object));

        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        var service = new PaoWorkflowNotificationService(
            _mockEmailSender.Object, localContextFactory.Object, mockServiceScopeFactory.Object,
            _mockLogger.Object, _mockConfiguration.Object, _mockNotificationManager.Object);

        var notification = new WorkflowNotification
        {
            EntityName = "Opportunity",
            EntityId = "1",
            EntityDisplayName = "Test Opportunity",
            RecipientUserIds = new List<int> { 103 },
            PerformedByUserId = 0, // No initiator
            PerformedByUserName = "Unknown",
            Timestamp = DateTime.UtcNow,
            Comment = "Please review"
        };

        EmailMessage? capturedEmailMessage = null;
        _mockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((msg, model, url) => capturedEmailMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        await service.NotifyNewApprovalRequestAsync(notification);

        // Assert - Should not fail even with no OM, just empty CC
        capturedEmailMessage.Should().NotBeNull();
        capturedEmailMessage!.CcReceivers.Should().NotBeNull();
        // CC may be empty or contain only Director (no OM in this test data)
    }

    #endregion

    #region EmailMessage CcReceivers Tests

    [Fact]
    public void EmailMessage_CcReceiversDefaultsToEmptyArray()
    {
        // Arrange & Act
        var emailMessage = new EmailMessage
        {
            TemplateName = "Test.html",
            Title = "Test Email"
        };

        // Assert
        emailMessage.CcReceivers.Should().NotBeNull();
        emailMessage.CcReceivers.Should().BeEmpty();
    }

    [Fact]
    public void EmailMessage_CcReceiversCanBeSet()
    {
        // Arrange & Act
        var ccList = new[] { "cc1@test.com", "cc2@test.com" };
        var emailMessage = new EmailMessage
        {
            TemplateName = "Test.html",
            Title = "Test Email",
            CcReceivers = ccList
        };

        // Assert
        emailMessage.CcReceivers.Should().BeEquivalentTo(ccList);
    }

    #endregion
}
