/**
 * @fileoverview PNO-1146 functional tests for workflow email notifications.
 * Business rules: correct templates, entity URL, approver name, in-system notifications.
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

[Collection("PNO1146_Functional")]
[Trait("Category", "Functional")]
public class FunctionalTests : PNO1146TestFixtureBase
{
    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_UsesCorrectTemplate_WorkflowApprovalRequestHtml()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CompletedNotification_UsesCorrectTemplate_WorkflowCompletedHtml()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task RejectedNotification_UsesCorrectTemplate_WorkflowRejectedHtml()
    {
        // Arrange — seed OM so rejection recipient lookup finds a valid user
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRejected);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task RecalledNotification_UsesCorrectTemplate_WorkflowRecalledHtml()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRecalled);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_IncludesEntityUrl_InEmailBody()
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

        // Assert — URL is {baseUrl}/partnerships/opportunities/{entityId}
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityUrl.Should().Contain("/partnerships/opportunities/1");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_IncludesApproverName_InEmailBody()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org", "Approver", "User");
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
        capturedModel!.ApproverName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CompletedNotification_IncludesOpportunityName_InEmailBody()
    {
        // Arrange
        await SeedOpportunityAsync(1, "My Test Opportunity");
        await SeedUserAsync(1, "user@unops.org");
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityDisplayName: "My Test Opportunity",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityName.Should().Be("My Test Opportunity");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task RejectedNotification_IncludesRejectionComment_InEmailBody()
    {
        // Arrange — seed OM so rejection recipient lookup finds a valid user
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            comment: "Project no longer aligned",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.Comment.Should().Be("Project no longer aligned");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_CreatesInSystemNotification_ForEachRecipient()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user1@unops.org");
        await SeedUserAsync(2, "user2@unops.org");

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1, 2 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert - notifications created via DbContext
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkNotificationsAsDone_UpdatesNotificationStatus()
    {
        // Arrange - create approval notification
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var updated = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        updated.Should().OnlyContain(n => n.Status == NotificationStatus.Done && n.IsRead);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkNotificationsAsRejected_UpdatesDecisionMessage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRejectedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var updated = await context.Notifications
            .Where(n => n.Entity == "Opportunity" && n.EntityId == 1)
            .ToListAsync();
        updated.Should().NotBeEmpty();
        updated.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkNotificationsAsRecalled_UpdatesNotificationStatus()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 });
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRecalledAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var updated = await context.Notifications
            .Where(n => n.Entity == "Opportunity" && n.EntityId == 1)
            .ToListAsync();
        updated.Should().NotBeEmpty();
        updated.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }
}
