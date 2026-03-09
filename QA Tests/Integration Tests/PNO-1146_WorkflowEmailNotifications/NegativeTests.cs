/**
 * @fileoverview PNO-1146 negative tests for workflow email notifications.
 * Invalid input, missing data, and expected failure scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.MailSender;
using UNOPS.Workflow.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1146;

[Collection("PNO1146_Negative")]
[Trait("Category", "Negative")]
public class NegativeTests : PNO1146TestFixtureBase
{
    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyApproval_EmptyRecipientList_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(recipientUserIds: new List<int>());

        // Act
        var act = async () => await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyApproval_NullEntityName_HandlesGracefully()
    {
        // Arrange - null EntityName; service uses it for logging, may still proceed
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(entityName: "");
        notification.EntityName = null!;

        // Act
        var act = async () => await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert - should not throw (service catches exceptions)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyApproval_EmptyEntityUrl_StillSendsEmail()
    {
        // Arrange - EntityUrl comes from config base URL + entityId, not from notification
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(entityId: "1");
        notification.EntityUrl = "";

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert - email uses baseUrl + entityId, not notification.EntityUrl
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyApproval_InvalidUserId_DoesNotCrash()
    {
        // Arrange - recipient has ID that doesn't exist in DB; no email found
        await SeedOpportunityAsync(1);
        var notification = BuildWorkflowNotification(recipientUserIds: new List<int> { 99999 });

        // Act
        var act = async () => await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyCompleted_NullNotification_Throws()
    {
        // Act
        var act = async () => await NotificationService.NotifyWorkflowCompletedAsync(null!);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyRejected_EmptyComment_StillSendsEmail()
    {
        // Arrange — seed OM so rejection recipient lookup finds a valid user
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        var notification = BuildWorkflowNotification(comment: "", recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyRecalled_ZeroPerformedByUserId_HandlesGracefully()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(
            recipientUserIds: new List<int> { 1 },
            performedByUserId: 0,
            performedByUserName: "System");

        // Act
        var act = async () => await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyCompleted_EmptyRecipientList_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(recipientUserIds: new List<int>());

        // Act
        var act = async () => await NotificationService.NotifyWorkflowCompletedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyRejected_EmptyRecipientList_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(recipientUserIds: new List<int>());

        // Act
        var act = async () => await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyRecalled_EmptyRecipientList_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(recipientUserIds: new List<int>());

        // Act
        var act = async () => await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyApproval_UserWithNoEmail_SkipsThatRecipient()
    {
        // Arrange - user exists but has empty email; GetRecipientEmailsAsync filters these out
        await SeedOpportunityAsync(1);
        DbContext.PAOUsers.Add(new UNOPS.PAO.Domain.Entities.PAOUser { Id = 100, Email = string.Empty });
        await DbContext.SaveChangesAsync();

        var notification = BuildWorkflowNotification(recipientUserIds: new List<int> { 100 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert - no recipients with valid email, so no email sent
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyApproval_InvalidEntityId_HandlesGracefully()
    {
        // Arrange
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(entityId: "invalid", recipientUserIds: new List<int> { 1 });

        // Act - service uses entityId for URL and org unit lookup; invalid ID returns "Unknown" org unit
        var act = async () => await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }
}
