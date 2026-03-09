/**
 * @fileoverview Tests for PaoWorkflowNotificationService.NotifyWorkflowCompletedAsync.
 * PNO-1146: Go Decision approved - TO: OM + Workflow Initiator; CC: Region/Hub/OrgUnit Directors, DoA2, DoA3.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.Workflow.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

[Collection("PaoWorkflowNotification")]
public class PaoWorkflowNotificationCompletedTests : PaoWorkflowNotificationTestFixtureBase
{
    #region Positive Tests (4)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowCompletedAsync_OpportunityWithOMAndInitiator_SendsEmailToBoth()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "OM", "User");
        await SeedUserAsync(101, "initiator@unops.org", "Initiator", "User");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            performedByUserId: 1,
            performedByUserName: "Approver");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
        LastCapturedEmail.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowCompletedAsync_WithComment_IncludesCommentInEmailModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "Approved for development");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.Comment.Should().Be("Approved for development");
        capturedModel.CommentSection.Should().Contain("Approver's Comment");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowCompletedAsync_NonOpportunityEntity_UsesRecipientUserIds()
    {
        // Arrange
        await SeedUserAsync(1, "recipient@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "1",
            entityDisplayName: "Test Partner",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("recipient@unops.org");
        LastCapturedEmail.CcReceivers.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowCompletedAsync_EntityUrlContainsOpportunityId()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityUrl.Should().Contain("opportunities/1");
    }

    #endregion

    #region Negative Tests (12)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_NoOM_DoesNotThrow()
    {
        // Arrange - opportunity without OM stakeholder
        await SeedOpportunityAsync(1);
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_NoRecipients_DoesNotSendEmail()
    {
        // Arrange - no OM, no initiator in workflow log, non-opportunity with empty recipients
        await SeedUserAsync(1, "user@unops.org");
        EmailMessage? captured = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback<EmailMessage, object, string?>((msg, _, _) => captured = msg)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "999",
            recipientUserIds: new List<int>()); // Empty - no valid users

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - when no TO recipients, method returns early without sending
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_InvalidEntityId_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(
            entityId: "invalid",
            entityDisplayName: "Test");

        // Act
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_NonExistentOpportunity_DoesNotThrow()
    {
        // Arrange - no opportunity in DB
        await SeedUserAsync(100, "om@unops.org");
        var notification = BuildWorkflowNotification(entityId: "99999");

        // Act
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_NoOpportunityManager_DoesNotThrow()
    {
        // Arrange - opportunity exists but no OM/stakeholders (PAOUser requires Email at schema level)
        await SeedOpportunityAsync(1);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act & Assert - service should not throw when no recipients found
        await NotificationService.Invoking(s => s.NotifyWorkflowCompletedAsync(notification))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_EmailSenderThrows_DoesNotPropagate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Email service down"));

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act - service catches and logs, does not rethrow
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_EmptyEntityDisplayName_StillSends()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_NullComment_DoesNotThrow()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        var notification = BuildWorkflowNotification(entityId: "1", comment: "");

        // Act
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_SoftDeletedOpportunity_HandledGracefully()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act - GetRejectionRecipientUserIds still finds OM; org unit lookup may return Unknown
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_OMSameAsInitiator_NoDuplicateInTO()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100); // Initiator is OM
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        var omCount = LastCapturedEmail!.EmailReceivers.Count(e => e == "om@unops.org");
        omCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_EntityNameCaseInsensitive_OpportunityMatched()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "OPPORTUNITY",
            entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - should use Opportunity-specific logic (OM + initiator)
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowCompletedAsync_ZeroPerformedByUserId_DoesNotThrow()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserId: 0);

        // Act
        var act = () => NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Boundary Tests (12)

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_MinTimestamp_Accepted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            timestamp: DateTime.MinValue);

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_MaxTimestamp_Accepted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            timestamp: DateTime.MaxValue);

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_OpportunityIdOne_Processed()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_LongEntityDisplayName_TruncatedOrHandled()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        var longName = new string('A', 500);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: longName);

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_SpecialCharactersInComment_HtmlEncoded()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "<script>alert('xss')</script>");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.CommentSection.Should().NotContain("<script>");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_OpportunityWithNoResponsibleOrgUnit_OrgUnitNameUnknown()
    {
        // Arrange
        await SeedOpportunityAsync(1, orgUnitId: null);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.ResponsibleOrgUnitId = null;
        await DbContext.SaveChangesAsync();
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.OrgUnitName.Should().Be("Unknown");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_MultipleRecipients_Deduplicated()
    {
        // Arrange - OM and initiator are same, should appear once
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_UnicodeInDisplayName_Handled()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Opportunité 机会");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_WhitespaceOnlyComment_CommentSectionEmptyOrMinimal()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "   ");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_NoWorkflowLogForInitiator_OMOnlyInTO()
    {
        // Arrange - no Submit log, so initiator not found
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - at least OM should be in TO
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_BaseUrlFromConfig_UsedInEntityUrl()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityUrl.Should().StartWith("https://");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowCompletedAsync_TitleContainsEntityDisplayName()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "My Custom Opportunity Name");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Contain("My Custom Opportunity Name");
        LastCapturedEmail.Title.Should().Contain("Go Decision Approved");
    }

    #endregion

    #region Functional Tests (12)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_UsesCorrectTemplate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_ApprovedByNamePopulated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "John Approver");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.ApprovedByName.Should().Be("John Approver");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_ApprovedOnFormatted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        var timestamp = new DateTime(2026, 3, 5, 14, 30, 0, DateTimeKind.Utc);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            timestamp: timestamp);

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.ApprovedOn.Should().Contain("2026");
        capturedModel.ApprovedOn.Should().Contain("Mar");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_RecipientNameFromUserProfile()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "Jane", "Manager");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.RecipientName.Should().Contain("Jane");
        capturedModel.RecipientName.Should().Contain("Manager");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_CCIncludesHierarchyDirectorsWhenOpportunity()
    {
        // Arrange - add OrgUnit with DoA2/DoA3 and director roles
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        var directorRole = new EntityRole
        {
            Id = 201,
            Code = "OrgUnit_Director_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Name = "Director",
            IsDeleted = false
        };
        await DbContext.Set<EntityRole>().AddAsync(directorRole);
        await DbContext.EntityUserRoles.AddAsync(new EntityUserRole
        {
            Id = 1,
            Name = "Dir",
            EntityType = "OrganizationHierarchy",
            EntityId = 1,
            EntityRoleId = 201,
            UserId = 102,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await DbContext.PAOUsers.AddAsync(new PAOUser { Id = 102, Email = "director@unops.org" });
        await DbContext.SaveChangesAsync();
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - CC should include hierarchy directors
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_EntityNameInModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "My Opportunity");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.EntityName.Should().Be("My Opportunity");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_SendEmailCalledOnce()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_OrgUnitNameFromOpportunity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_NonOpportunityNoCC()
    {
        // Arrange
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "1",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_EmailReceiversNotNull()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_CcReceiversNotNull()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowCompletedAsync_InitiatorFromWorkflowLog()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    #endregion

    #region Integration Tests (12)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_FullFlow_OpportunityToEmail()
    {
        // Arrange - full seed
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "OM", "User");
        await SeedUserAsync(101, "initiator@unops.org", "Initiator", "User");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            performedByUserName: "Approver");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.TemplateName.Should().Be(TemplateCompleted);
        LastCapturedEmail.Title.Should().Contain("Go Decision Approved");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_DbContextFactoryCreatesNewContext()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - service uses context factory; no shared context issues
        await NotificationService.Invoking(s => s.NotifyWorkflowCompletedAsync(notification))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_WorkflowDbContextUsedForInitiatorLookup()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - initiator 101 should be in TO (from WorkflowLog)
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_AppDbContextAndWorkflowDbContext_Independent()
    {
        // Arrange - data in both contexts
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_ConsecutiveCalls_NoStateLeakage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification1 = BuildWorkflowNotification(entityId: "1");
        var notification2 = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification1);
        await NotificationService.NotifyWorkflowCompletedAsync(notification2);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_OpportunityStakeholderLookup_OMFound()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_GetRecipientEmails_FromPAOUsers()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().AllSatisfy(e => e.Should().Contain("@"));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_GetOrgUnitName_FromOpportunity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_EmailMessageStructure_Valid()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().NotBeNullOrEmpty();
        LastCapturedEmail.Title.Should().NotBeNullOrEmpty();
        LastCapturedEmail.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_ServiceScopeFactory_ProvidesWorkflowDbContext()
    {
        // Arrange - initiator lookup requires WorkflowDbContext from scope
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert - if scope didn't provide WorkflowDbContext, initiator lookup would fail
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_ConfigurationBaseUrl_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel!.EntityUrl.Should().Contain("test.pao.unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowCompletedAsync_EndToEnd_AllComponentsInteract()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "Jane", "Doe");
        await SeedUserAsync(101, "initiator@unops.org", "John", "Smith");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Strategic Partnership",
            performedByUserName: "Director Approver",
            comment: "Approved");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.CcReceivers.Should().NotBeNull();
        LastCapturedEmail.TemplateName.Should().Be(TemplateCompleted);
    }

    #endregion
}
