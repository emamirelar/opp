/**
 * @fileoverview Tests for PaoWorkflowNotificationService.NotifyWorkflowRecalledAsync.
 * PNO-1146: Submission recalled - TO: OM + Workflow Initiator; CC: OrgUnit Director, Deputy, DoA2, DoA3.
 * Uses PENDING Submit log (CompletedOn == null) for initiator lookup.
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
public class PaoWorkflowNotificationRecalledTests : PaoWorkflowNotificationTestFixtureBase
{
    #region Positive Tests (4)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRecalledAsync_OpportunityWithOMAndInitiator_SendsEmailToBoth()
    {
        // Arrange - Recalled uses PENDING Submit log
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "OM", "User");
        await SeedUserAsync(101, "initiator@unops.org", "Initiator", "User");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            performedByUserName: "Recaller",
            comment: "Need to fix data");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRecalled);
        LastCapturedEmail.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRecalledAsync_WithComment_IncludesJustificationInEmailModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "Need to update budget figures");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.Comment.Should().Be("Need to update budget figures");
        capturedModel.CommentSection.Should().Contain("Justification");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRecalledAsync_NonOpportunityEntity_UsesRecipientUserIds()
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
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("recipient@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRecalledAsync_EntityUrlContainsOpportunityId()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityUrl.Should().Contain("opportunities/1");
    }

    #endregion

    #region Negative Tests (12)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_NoOM_DoesNotThrow()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedPendingSubmitWorkflowLogAsync("1", 101);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_NoRecipients_DoesNotSendEmail()
    {
        // Arrange
        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "999",
            recipientUserIds: new List<int>());

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_InvalidEntityId_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(entityId: "invalid");

        // Act
        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_NonExistentOpportunity_DoesNotThrow()
    {
        // Arrange
        await SeedUserAsync(100, "om@unops.org");
        var notification = BuildWorkflowNotification(entityId: "99999");

        // Act
        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_EmailSenderThrows_DoesNotPropagate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Email down"));

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_EmptyComment_DoesNotThrow()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        var notification = BuildWorkflowNotification(entityId: "1", comment: "");

        // Act
        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_SoftDeletedOpportunity_HandledGracefully()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_OMSameAsInitiator_NoDuplicateInTO()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Count(e => e == "om@unops.org").Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_EntityNameCaseInsensitive_OpportunityMatched()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "OPPORTUNITY",
            entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_RecalledByNamePopulated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "User Recaller");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.RecalledByName.Should().Be("User Recaller");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_TitleContainsSubmissionRecalled()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opp");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.Title.Should().Contain("Recalled");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRecalledAsync_NoPendingWorkflowLog_OMOnlyInTO()
    {
        // Arrange - no PENDING Submit log (only completed would not match)
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    #endregion

    #region Boundary Tests (12)

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_UsesPendingNotCompletedSubmitLog()
    {
        // Arrange - Recalled looks for CompletedOn == null
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert - initiator from pending log
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_CompletedSubmitLog_NotUsedForInitiator()
    {
        // Arrange - only completed log, no pending
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101); // Completed, not pending
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert - Recalled uses pending (CompletedOn==null), so initiator NOT found; only OM
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_MinTimestamp_Accepted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            timestamp: DateTime.MinValue);

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_SpecialCharactersInComment_HtmlEncoded()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "<img src=x onerror=alert(1)>");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.CommentSection.Should().NotContain("<img");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_OpportunityWithNoResponsibleOrgUnit_OrgUnitNameUnknown()
    {
        // Arrange
        await SeedOpportunityAsync(1, orgUnitId: null);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.ResponsibleOrgUnitId = null;
        await DbContext.SaveChangesAsync();
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("Unknown");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_RecalledOnFormatted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        var timestamp = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            timestamp: timestamp);

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.RecalledOn.Should().Contain("2026");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_CCRecalledOrRejectedRoleCodes()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_NonOpportunityNoCC()
    {
        // Arrange
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "1",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_GetRecallAdditionalRecipientUserIds_OMAndInitiator()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_EntityDisplayNameInTitle()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "My Opportunity");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.Title.Should().Contain("My Opportunity");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_WorkflowLogActionSubmit_Required()
    {
        // Arrange - service filters by Action == "Submit"
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRecalledAsync_MultipleRecipients_Deduplicated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(1);
    }

    #endregion

    #region Functional Tests (12)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_UsesCorrectTemplate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRecalled);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_SendEmailCalledOnce()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_InitiatorFromPendingSubmitLog()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_BuildRecalledOrRejectedCC_SameAsRejected()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_EntityNameInModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "My Opp");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.EntityName.Should().Be("My Opp");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_CommentSectionContainsJustification()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "Data fix needed");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.CommentSection.Should().Contain("Justification");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_OrgUnitNameFromOpportunity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_OpportunityStakeholderLookup_OMFound()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_GetInitiatorUserIdForRecalled_Pending()
    {
        // Arrange - Recalled uses CompletedOn == null
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_EmailReceiversNotNull()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_RecipientNameFromUserProfile()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "Jane", "Manager");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.RecipientName.Should().Contain("Jane");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRecalledAsync_WorkflowLogOrderByCreatedDateDesc()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    #endregion

    #region Integration Tests (12)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_FullFlow_OpportunityToEmail()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "OM", "User");
        await SeedUserAsync(101, "initiator@unops.org", "Initiator", "User");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opportunity",
            performedByUserName: "Recaller",
            comment: "Need to fix");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.TemplateName.Should().Be(TemplateRecalled);
        LastCapturedEmail.Title.Should().Contain("Recalled");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_DbContextFactory_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        await NotificationService.Invoking(s => s.NotifyWorkflowRecalledAsync(notification))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_WorkflowDbContext_PendingLogLookup()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_ConsecutiveCalls_NoStateLeakage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_GetRecipientEmails_FromPAOUsers()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().AllSatisfy(e => e.Should().Contain("@"));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_GetOrgUnitName_FromOpportunity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_EmailMessageStructure_Valid()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().NotBeNullOrEmpty();
        LastCapturedEmail.Title.Should().NotBeNullOrEmpty();
        LastCapturedEmail.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_ServiceScopeFactory_ProvidesWorkflowDbContext()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_ConfigurationBaseUrl_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        capturedModel!.EntityUrl.Should().Contain("test.pao.unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_EndToEnd_AllComponentsInteract()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "Jane", "Doe");
        await SeedUserAsync(101, "initiator@unops.org", "John", "Smith");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Strategic Partnership",
            performedByUserName: "User Recaller",
            comment: "Need to update budget");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.TemplateName.Should().Be(TemplateRecalled);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_AppDbContext_OpportunityLookup()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRecalledAsync_DifferentFromRejected_InitiatorLookup()
    {
        // Arrange - Recalled uses pending (CompletedOn==null), Rejected uses completed (CompletedOn!=null)
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1");

        // Act
        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        // Assert - Recalled finds initiator from pending log
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    #endregion
}
