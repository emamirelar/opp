/**
 * @fileoverview PNO-1146 positive tests for workflow email notifications.
 * Happy path scenarios for NotifyNewApprovalRequest, NotifyWorkflowCompleted,
 * NotifyWorkflowRejected, and NotifyWorkflowRecalled.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.MailSender;
using UNOPS.Workflow.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1146;

[Collection("PNO1146_Positive")]
[Trait("Category", "Positive")]
public class PositiveTests : PNO1146TestFixtureBase
{
    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyNewApprovalRequest_SendsEmail_ToRecipients()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org", "Approver", "User");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateApprovalRequest),
                It.IsAny<ApprovalRequestEmailModel>(),
                It.IsAny<string?>()),
            Times.Once);
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("approver@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowCompleted_SendsApprovalEmail_ToStakeholders()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "stakeholder@unops.org", "Stakeholder", "User");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            recipientUserIds: new List<int> { 1 },
            performedByUserName: "Approver Name");

        // Act
        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateCompleted),
                It.IsAny<WorkflowCompletedEmailModel>(),
                It.IsAny<string?>()),
            Times.Once);
        LastCapturedEmail!.EmailReceivers.Should().Contain("stakeholder@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRejected_SendsRejectionEmail_WithComment()
    {
        // Arrange — seed OM so rejection recipient lookup finds a valid user
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "submitter@unops.org", "Submitter", "User");
        await SeedOpportunityManagerAsync(1, 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            comment: "Project no longer aligned with mandate");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateRejected),
                It.IsAny<WorkflowRejectedEmailModel>(),
                It.IsAny<string?>()),
            Times.Once);
        LastCapturedEmail!.EmailReceivers.Should().Contain("submitter@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRecalled_SendsRecallEmail_ToRecipients()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org", "Approver", "User");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            performedByUserName: "Initiator Name");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateRecalled),
                It.IsAny<WorkflowRecalledEmailModel>(),
                It.IsAny<string?>()),
            Times.Once);
        LastCapturedEmail!.EmailReceivers.Should().Contain("approver@unops.org");
    }
}
