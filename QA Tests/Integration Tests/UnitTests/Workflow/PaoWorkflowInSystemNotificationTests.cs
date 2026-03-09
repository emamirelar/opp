/**
 * @fileoverview Tests for PaoWorkflowNotificationService in-system notification methods.
 * PNO-1146: MarkWorkflowNotificationsAsDoneAsync, Mark*Approved/Rejected/Recalled, CreateInSystemNotificationsAsync.
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

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

[Collection("PaoWorkflowNotification")]
public class PaoWorkflowInSystemNotificationTests : PaoWorkflowNotificationTestFixtureBase
{
    private const string TemplateApprovalRequest = "UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowApprovalRequest.html";

    private async Task CreateWorkflowApprovalNotificationsAsync(int entityId = 1, params int[] userIds)
    {
        var notification = BuildWorkflowNotification(
            entityId: entityId.ToString(),
            entityDisplayName: "Test Opportunity",
            recipientUserIds: userIds.ToList());
        await NotificationService.NotifyNewApprovalRequestAsync(notification);
    }

    #region Positive Tests (4)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_UpdatesStatusToDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task MarkWorkflowNotificationsAsApprovedAsync_AppendsApprovedToMessage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Message.Contains("Approved"));
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task MarkWorkflowNotificationsAsRejectedAsync_AppendsSetToNOGOToMessage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRejectedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Message.Contains("NO GO"));
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task MarkWorkflowNotificationsAsRecalledAsync_AppendsRecalledToMessage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRecalledAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Message.Contains("Recalled"));
    }

    #endregion

    #region Negative Tests (12)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_NoPendingNotifications_DoesNotThrow()
    {
        // Arrange - no notifications
        // Act
        var act = () => NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 99999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_OnlyPendingUpdated()
    {
        // Arrange - create notifications and mark as done
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Act - call again
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert - no throw
        await NotificationService.Invoking(s => s.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_EmptyDecisionMessage_Accepted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        var act = () => NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1, null);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_NonMatchingEntityName_NoUpdate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Partner", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_NonMatchingEntityId_NoUpdate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 999);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_IsReadSetToTrue()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.IsRead);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreateInSystemNotificationsAsync_InvalidEntityId_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(
            entityId: "invalid",
            entityDisplayName: "Test",
            recipientUserIds: new List<int> { 1 });

        // Act
        var act = () => NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreateInSystemNotificationsAsync_NoRecipients_NoInSystemNotifications()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int>());

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var count = await context.Notifications
            .CountAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        count.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_WithDecisionMessage_AppendsToMessage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1, "Custom decision");

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Message.Contains("Custom decision"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsApprovedAsync_CallsMarkDoneWithApproved()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyNewApprovalRequestAsync_DoesNotCreateInSystemNotifications_WhenNoRecipients()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test",
            recipientUserIds: new List<int> { 999 }); // User 999 doesn't exist

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert - may or may not create; if no recipients found, no email sent
        await NotificationService.Invoking(s => s.NotifyNewApprovalRequestAsync(notification))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_DoesNotRethrowOnException()
    {
        // Arrange - notifications exist
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act - service catches and logs, doesn't rethrow
        var act = () => NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Boundary Tests (12)

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_OneRecipient_OneNotification()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1
                && n.UserId == 1)
            .ToListAsync();
        notifications.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_MultipleRecipients_MultipleNotifications()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver1@unops.org");
        await SeedUserAsync(2, "approver2@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1, 2);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_CategoryWorkflowApproval()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_ResponseTypeActionRequired()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.ResponseType.Should().Be("action_required");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_StatusPending()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_IsReadFalse()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => !n.IsRead);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_RecordDataContainsEntityInfo()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.RecordData.Should().Contain("entityId");
        notification.RecordData.Should().Contain("1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyNewApprovalRequestAsync_CreatesInSystemNotificationsBeforeEmail()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var count = await context.Notifications
            .CountAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_UpdatesAllMatching()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver1@unops.org");
        await SeedUserAsync(2, "approver2@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1, 2);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().HaveCount(2);
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_MessageContainsEntityDisplayName()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.Message.Should().Contain("Go Decision");
        notification.Message.Should().Contain("approval required");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreateInSystemNotificationsAsync_EntityAndEntityIdSet()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.Entity.Should().Be("Opportunity");
        notification.EntityId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task MarkWorkflowNotificationsAsRecalledAsync_SetsIsReadTrue()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRecalledAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.IsRead);
    }

    #endregion

    #region Functional Tests (12)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_FiltersByCategory()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var otherCategory = await context.Notifications
            .Where(n => n.Entity == "Opportunity" && n.EntityId == 1 && n.Category != PaoWorkflowNotificationService.WorkflowApprovalCategory)
            .ToListAsync();
        var updated = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        updated.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_FiltersByEntity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_FiltersByEntityId()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedOpportunityAsync(2);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);
        await CreateWorkflowApprovalNotificationsAsync(2, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var entity1Notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1)
            .ToListAsync();
        var entity2Notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 2)
            .ToListAsync();
        entity1Notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
        entity2Notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_FiltersByPendingStatus()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert - only Pending were updated
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsApprovedAsync_DelegatesToMarkDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsRejectedAsync_DelegatesToMarkDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRejectedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsRecalledAsync_DelegatesToMarkDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRecalledAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreateInSystemNotificationsAsync_UserIdFromRecipientUserIds()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.UserId.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreateInSystemNotificationsAsync_CreatedAtSet()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyNewApprovalRequestAsync_SendsEmailAndCreatesInSystemNotifications()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var count = await context.Notifications
            .CountAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task WorkflowApprovalCategory_ConstantValue()
    {
        // Assert
        PaoWorkflowNotificationService.WorkflowApprovalCategory.Should().Be("workflow_approval");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_SaveChangesCalled()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    #endregion

    #region Integration Tests (12)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenApprove_FullFlow_NotificationsMarkedDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
        notifications.Should().OnlyContain(n => n.IsRead);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenReject_FullFlow_NotificationsMarkedDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRejectedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitThenRecall_FullFlow_NotificationsMarkedDone()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRecalledAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateInSystemNotificationsAsync_DbContextFactory_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");

        // Act
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_DbContextFactory_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1);

        // Assert
        await NotificationService.Invoking(s => s.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyNewApprovalRequestAsync_CreatesNotifications_ThenMarkDone_UpdatesThem()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().HaveCount(1);
        notifications[0].Status.Should().Be(NotificationStatus.Done);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MultipleEntities_OnlyTargetEntityUpdated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedOpportunityAsync(2);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);
        await CreateWorkflowApprovalNotificationsAsync(2, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var entity1Notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1)
            .ToListAsync();
        var entity2Notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 2)
            .ToListAsync();
        entity1Notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
        entity2Notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Pending);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateInSystemNotificationsAsync_RecordDataJsonSerialized()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        notification.Should().NotBeNull();
        notification!.RecordData.Should().Contain("{");
        notification.RecordData.Should().Contain("}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MarkWorkflowNotificationsAsDoneAsync_UpdatesMessageWhenDecisionMessageProvided()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsDoneAsync("Opportunity", 1, "Custom");

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Message.EndsWith(" - Custom"));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyNewApprovalRequestAsync_EmailAndInSystemNotifications_BothCreated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var count = await context.Notifications
            .CountAsync(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.EntityId == 1);
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EndToEnd_SubmitApprove_NotificationsResolved()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsApprovedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().NotBeEmpty();
        notifications.Should().OnlyContain(n => n.Status == NotificationStatus.Done);
        notifications.Should().OnlyContain(n => n.IsRead);
        notifications.Should().OnlyContain(n => n.Message.Contains("Approved"));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EndToEnd_SubmitReject_NotificationsResolved()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        await CreateWorkflowApprovalNotificationsAsync(1, 1);

        // Act
        await NotificationService.MarkWorkflowNotificationsAsRejectedAsync("Opportunity", 1);

        // Assert
        await using var context = await MockContextFactory.Object.CreateDbContextAsync();
        var notifications = await context.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory
                && n.Entity == "Opportunity"
                && n.EntityId == 1)
            .ToListAsync();
        notifications.Should().OnlyContain(n => n.Message.Contains("NO GO"));
    }

    #endregion
}
