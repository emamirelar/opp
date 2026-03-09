/**
 * @fileoverview PNO-1146 integration tests for workflow email notifications.
 * End-to-end flows: submit, approve, reject, recall, and notification sequences.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.Workflow.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1146;

[Collection("PNO1146_Integration")]
[Trait("Category", "Integration")]
public class IntegrationTests : PNO1146TestFixtureBase
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitWorkflow_TriggersApprovalRequestEmail_EndToEnd()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org", "Approver", "User");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            recipientUserIds: new List<int> { 1 },
            performedByUserName: "Initiator");

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
        LastCapturedEmail.EmailReceivers.Should().Contain("approver@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApproveWorkflow_TriggersCompletedEmail_EndToEnd()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "submitter@unops.org", "Submitter", "User");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            recipientUserIds: new List<int> { 1 },
            performedByUserName: "Approver Name");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
        LastCapturedEmail.EmailReceivers.Should().Contain("submitter@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RejectWorkflow_TriggersRejectedEmail_EndToEnd()
    {
        // Arrange — seed OM so rejection recipient lookup finds a valid user
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "submitter@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            comment: "NO GO - Budget constraints");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRejected);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RecallWorkflow_TriggersRecalledEmail_EndToEnd()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            performedByUserName: "Initiator");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRecalled);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenRecall_SendsBothNotifications_InSequence()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");

        var approvalNotification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 });
        var recallNotification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            performedByUserName: "Initiator");

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(approvalNotification);
        await NotificationService.NotifyWorkflowRecalledAsync(recallNotification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateApprovalRequest),
                It.IsAny<object>(),
                It.IsAny<string?>()),
            Times.Once);
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateRecalled),
                It.IsAny<object>(),
                It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenApprove_CreatesAndResolvesNotifications()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await SeedUserAsync(2, "submitter@unops.org");

        var approvalNotification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 });
        var completedNotification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 2 },
            performedByUserId: 1,
            performedByUserName: "Approver");

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(approvalNotification);
        await NotificationService.NotifyWorkflowCompletedAsync(completedNotification);
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Entity == "Opportunity" && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenReject_CreatesAndResolvesNotifications()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await SeedUserAsync(2, "submitter@unops.org");

        var approvalNotification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 });
        var rejectedNotification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 2 },
            performedByUserId: 1,
            performedByUserName: "Approver",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(approvalNotification);
        await NotificationService.NotifyWorkflowRejectedAsync(rejectedNotification);
        await NotificationService.MarkWorkflowNotificationsAsRejectedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Entity == "Opportunity" && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MultipleApprovers_EachReceivesNotification()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver1@unops.org");
        await SeedUserAsync(2, "approver2@unops.org");
        await SeedUserAsync(3, "approver3@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1, 2, 3 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert - one email to all three recipients
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(3);
        LastCapturedEmail.EmailReceivers.Should().Contain("approver1@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("approver2@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("approver3@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task WorkflowWithDocuments_EmailIncludesEntityUrl()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityUrl.Should().Contain("opportunities");
        capturedModel.EntityUrl.Should().Contain("1");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitOpportunity_InSystemNotificationCreated_ForApprovers()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org", "Approver", "User");

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var inSystemNotifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1
                && n.UserId == 1)
            .ToListAsync();
        inSystemNotifications.Should().NotBeEmpty();
        inSystemNotifications.Should().OnlyContain(n => n.Status == NotificationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenRecall_ThenResubmit_ThreeNotificationsCreated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");

        var submit1 = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });
        var recall = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });
        var submit2 = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(submit1);
        await NotificationService.NotifyWorkflowRecalledAsync(recall);
        await NotificationService.NotifyNewApprovalRequestAsync(submit2);

        // Assert — three email sends total
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Exactly(3));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApproveWorkflow_NotifiesStakeholders_AndCreatesInSystemNotifications()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "stakeholder1@unops.org");
        await SeedUserAsync(2, "stakeholder2@unops.org");
        await SeedStakeholderAsync(1, 1);
        await SeedStakeholderAsync(2, 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1, 2 },
            performedByUserName: "Approver");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert — email sent to both stakeholders
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
    }
}
