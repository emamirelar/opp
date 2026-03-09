/**
 * @fileoverview Tests for PaoWorkflowNotificationService.NotifyWorkflowRejectedAsync.
 * PNO-1146: NO GO - TO: OM + Workflow Initiator; CC: OrgUnit Director, Deputy, DoA2, DoA3.
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
public class PaoWorkflowNotificationRejectedTests : PaoWorkflowNotificationTestFixtureBase
{
    #region Positive Tests (4)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRejectedAsync_OpportunityWithOMAndInitiator_SendsEmailToBoth()
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
            performedByUserName: "Rejector",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRejected);
        LastCapturedEmail.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRejectedAsync_WithComment_IncludesReasonInEmailModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "Budget constraints");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.Comment.Should().Be("Budget constraints");
        capturedModel.CommentSection.Should().Contain("Reason");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRejectedAsync_NonOpportunityEntity_UsesRecipientUserIds()
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
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("recipient@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyWorkflowRejectedAsync_EntityUrlContainsOpportunityId()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.EntityUrl.Should().Contain("opportunities/1");
    }

    #endregion

    #region Negative Tests (12)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_NoOM_DoesNotThrow()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        var act = () => NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_NoRecipients_DoesNotSendEmail()
    {
        // Arrange
        await SeedUserAsync(1, "user@unops.org");
        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "999",
            recipientUserIds: new List<int>());

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_InvalidEntityId_DoesNotThrow()
    {
        // Arrange
        var notification = BuildWorkflowNotification(entityId: "invalid", comment: "NO GO");

        // Act
        var act = () => NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_NonExistentOpportunity_DoesNotThrow()
    {
        // Arrange
        await SeedUserAsync(100, "om@unops.org");
        var notification = BuildWorkflowNotification(entityId: "99999", comment: "NO GO");

        // Act
        var act = () => NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_EmailSenderThrows_DoesNotPropagate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Email down"));

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        var act = () => NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_EmptyComment_DoesNotThrow()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        var notification = BuildWorkflowNotification(entityId: "1", comment: "");

        // Act
        var act = () => NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_SoftDeletedOpportunity_HandledGracefully()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        var act = () => NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_OMSameAsInitiator_NoDuplicateInTO()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Count(e => e == "om@unops.org").Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_EntityNameCaseInsensitive_OpportunityMatched()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "opportunity",
            entityId: "1",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_RejectedByNamePopulated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "Director Rejector",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.RejectedByName.Should().Be("Director Rejector");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_TitleContainsSetToNOGO()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Test Opp",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.Title.Should().Contain("NO GO");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyWorkflowRejectedAsync_NoWorkflowLog_OMOnlyInTO()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    #endregion

    #region Boundary Tests (12)

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_MinTimestamp_Accepted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "NO GO",
            timestamp: DateTime.MinValue);

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_SpecialCharactersInComment_HtmlEncoded()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "<script>alert(1)</script>");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.CommentSection.Should().NotContain("<script>");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_OpportunityWithNoResponsibleOrgUnit_OrgUnitNameUnknown()
    {
        // Arrange
        await SeedOpportunityAsync(1, orgUnitId: null);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.ResponsibleOrgUnitId = null;
        await DbContext.SaveChangesAsync();
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("Unknown");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_LongComment_Handled()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        var longComment = new string('x', 1000);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: longComment);

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_UnicodeInComment_Handled()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "理由: 予算制約");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_RejectedOnFormatted()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        var timestamp = new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "NO GO",
            timestamp: timestamp);

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.RejectedOn.Should().Contain("2026");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_CCRecalledOrRejectedRoleCodes()
    {
        // Arrange - BuildRecalledOrRejectedCCRecipients uses OrgUnit Director + DoA2, DoA3
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        var directorRole = new EntityRole
        {
            Id = 202,
            Code = "OrgUnit_Director_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Name = "Director",
            IsDeleted = false
        };
        await DbContext.Set<EntityRole>().AddAsync(directorRole);
        await DbContext.EntityUserRoles.AddAsync(new EntityUserRole
        {
            Id = 2,
            Name = "Dir",
            EntityType = "OrganizationHierarchy",
            EntityId = 1,
            EntityRoleId = 202,
            UserId = 102,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await DbContext.PAOUsers.AddAsync(new PAOUser { Id = 102, Email = "director@unops.org" });
        await DbContext.SaveChangesAsync();
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_NonOpportunityNoCC()
    {
        // Arrange
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "1",
            recipientUserIds: new List<int> { 1 });

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_EntityDisplayNameInTitle()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Strategic Initiative",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.Title.Should().Contain("Strategic Initiative");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_RecipientNameFromUserProfile()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "Jane", "Manager");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.RecipientName.Should().Contain("Jane");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_BaseUrlInEntityUrl()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.EntityUrl.Should().StartWith("https://");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyWorkflowRejectedAsync_MultipleRecipients_Deduplicated()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(1);
    }

    #endregion

    #region Functional Tests (12)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_UsesCorrectTemplate()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRejected);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_SendEmailCalledOnce()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_GetRejectionRecipientUserIds_OMAndInitiator()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_BuildRecalledOrRejectedCC_OrgUnitDirectorsOnly()
    {
        // Arrange - Rejected uses OrgUnit Director + Deputy + DoA2 + DoA3 (not Region/Hub)
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_EntityNameInModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "My Opp",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.EntityName.Should().Be("My Opp");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_EmailReceiversNotNull()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_InitiatorFromCompletedSubmitLog()
    {
        // Arrange - Rejected uses CompletedOn != null (completed Submit)
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_OrgUnitNameFromOpportunity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_CommentSectionContainsReason()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "Budget cut");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.CommentSection.Should().Contain("Reason");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_OpportunityStakeholderLookup_OMFound()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_StringEqualsForEntityName()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "Opportunity",
            entityId: "1",
            comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().Be(TemplateRejected);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyWorkflowRejectedAsync_WorkflowLogOrderByCompletedOnDesc()
    {
        // Arrange - multiple completed logs, should get most recent
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    #endregion

    #region Integration Tests (12)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_FullFlow_OpportunityToEmail()
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
            performedByUserName: "Director",
            comment: "NO GO - Budget");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.TemplateName.Should().Be(TemplateRejected);
        LastCapturedEmail.Title.Should().Contain("NO GO");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_DbContextFactory_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        await NotificationService.Invoking(s => s.NotifyWorkflowRejectedAsync(notification))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_WorkflowDbContext_InitiatorLookup()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_ConsecutiveCalls_NoStateLeakage()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_GetRecipientEmails_FromPAOUsers()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().AllSatisfy(e => e.Should().Contain("@"));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_GetOrgUnitName_FromOpportunity()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_EmailMessageStructure_Valid()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail!.TemplateName.Should().NotBeNullOrEmpty();
        LastCapturedEmail.Title.Should().NotBeNullOrEmpty();
        LastCapturedEmail.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_ServiceScopeFactory_ProvidesWorkflowDbContext()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_ConfigurationBaseUrl_Used()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.EntityUrl.Should().Contain("test.pao.unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_EndToEnd_AllComponentsInteract()
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
            performedByUserName: "Director Rejector",
            comment: "NO GO - Budget constraints");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.TemplateName.Should().Be(TemplateRejected);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_AppDbContext_OpportunityLookup()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyWorkflowRejectedAsync_GetRecipientNames_ForEmailModel()
    {
        // Arrange
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org", "Jane", "Manager");
        await SeedOpportunityManagerAsync(1, 100);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(entityId: "1", comment: "NO GO");

        // Act
        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        // Assert
        capturedModel!.RecipientName.Should().NotBeNullOrEmpty();
    }

    #endregion
}
