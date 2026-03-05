/**
 * @fileoverview Tests for PNO-1005: Verify Email Notification Content Sent to Decision Maker and OIC.
 * Validates that the rendered email body model contains specific template data like entity display name,
 * entity URL, approver names, rejection comments, and correct template names.
 *
 * Requirements validated:
 * - REQ-1: Email model populated with entity display name, URL, recipient data
 * - REQ-2: Correct template names for approval request, completed, rejected, recalled
 * - REQ-3: TO recipients include decision maker; entity URL uses AppBaseUrl
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.MailSender;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1146;

/// <summary>
/// Tests for PNO-1005: Verify Email Notification Content Sent to Decision Maker and OIC.
/// Validates email model content (entity name, URL, template names) and template text placeholders.
/// </summary>
public class EmailContentTests : PNO1146TestFixtureBase
{
    #region Positive (2 tests)

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Positive")]
    [Trait("TestId", "TC-PNO1005-POS-001")]
    public async Task TC_PNO1005_POS_001_EmailSentToDecisionMaker_HasNonEmptyBodyData()
    {
        await SeedOpportunityAsync(1, "Budget Review");
        await SeedUserAsync(1, "approver@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Budget Review",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        capturedModel.Should().NotBeNull();
        capturedModel!.EntityName.Should().NotBeNullOrEmpty();
        capturedModel.RequestedByName.Should().NotBeNullOrEmpty();
        capturedModel.EntityUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Positive")]
    [Trait("TestId", "TC-PNO1005-POS-002")]
    public async Task TC_PNO1005_POS_002_EmailSent_HasCorrectTemplateNameForApprovalRequest()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
    }

    #endregion

    #region Negative (6 tests)

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-PNO1005-NEG-001")]
    public async Task TC_PNO1005_NEG_001_EmailModel_IncludesEntityDisplayName()
    {
        await SeedOpportunityAsync(1, "Nepal Infrastructure Project");
        await SeedUserAsync(1, "dm@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Nepal Infrastructure Project", recipientUserIds: new List<int> { 1 }));

        capturedModel!.EntityName.Should().Be("Nepal Infrastructure Project");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-PNO1005-NEG-002")]
    public async Task TC_PNO1005_NEG_002_EmailModel_IncludesEntityUrlForNavigation()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        capturedModel!.EntityUrl.Should().Contain("opportunities/1");
        capturedModel.EntityUrl.Should().Contain("partnerships");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-PNO1005-NEG-003")]
    public async Task TC_PNO1005_NEG_003_EmailWithEmptyOpportunityName_StillSends()
    {
        await SeedOpportunityAsync(1, "");
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-PNO1005-NEG-004")]
    public async Task TC_PNO1005_NEG_004_EmailWithVeryLongOpportunityName_StillSends()
    {
        var longName = new string('A', 500);
        await SeedOpportunityAsync(1, longName);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: longName,
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Contain("Action Required");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-PNO1005-NEG-005")]
    public async Task TC_PNO1005_NEG_005_CompletedEmailModel_IncludesApproverName()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyWorkflowCompletedAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 100 }, performedByUserName: "Alice Approver"));

        capturedModel!.ApprovedByName.Should().Be("Alice Approver");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-PNO1005-NEG-006")]
    public async Task TC_PNO1005_NEG_006_RejectedEmailModel_IncludesRejectionComment()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        await SeedCompletedSubmitWorkflowLogAsync("1", 1);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyWorkflowRejectedAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }, comment: "Out of scope for current portfolio"));

        capturedModel!.Comment.Should().Be("Out of scope for current portfolio");
    }

    #endregion

    #region Functional (6 tests)

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-PNO1005-FUNC-001")]
    public async Task TC_PNO1005_FUNC_001_ApprovalRequestEmail_UsesCorrectTemplateName()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "dm@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        LastCapturedEmail!.TemplateName.Should().Be("UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowApprovalRequest.html");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-PNO1005-FUNC-002")]
    public async Task TC_PNO1005_FUNC_002_CompletedEmail_UsesCorrectTemplateName()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowCompletedAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 100 }, performedByUserName: "Approver"));

        LastCapturedEmail!.TemplateName.Should().Be("UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowCompleted.html");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-PNO1005-FUNC-003")]
    public async Task TC_PNO1005_FUNC_003_RejectedEmail_UsesCorrectTemplateName()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        await SeedCompletedSubmitWorkflowLogAsync("1", 1);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowRejectedAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }, comment: "No"));

        LastCapturedEmail!.TemplateName.Should().Be("UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowRejected.html");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-PNO1005-FUNC-004")]
    public async Task TC_PNO1005_FUNC_004_RecalledEmail_UsesCorrectTemplateName()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowRecalledAsync(
            BuildWorkflowNotification(entityId: "1", performedByUserName: "Recaller"));

        LastCapturedEmail!.TemplateName.Should().Be("UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowRecalled.html");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-PNO1005-FUNC-005")]
    public async Task TC_PNO1005_FUNC_005_EmailTORecipients_IncludeDecisionMakerEmail()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "decision.maker@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        LastCapturedEmail!.EmailReceivers.Should().Contain("decision.maker@unops.org");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-PNO1005-FUNC-006")]
    public async Task TC_PNO1005_FUNC_006_Email_IncludesEntityUrlPointingToOpportunityDetailPage()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        capturedModel!.EntityUrl.Should().Contain("/partnerships/opportunities/1");
    }

    #endregion

    #region Edge/Boundary (6 tests)

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-PNO1005-EDGE-001")]
    public async Task TC_PNO1005_EDGE_001_EmailModelData_CapturedViaMockEmailSenderCallback()
    {
        await SeedOpportunityAsync(1, "Test Opp");
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Test Opp", recipientUserIds: new List<int> { 1 }));

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Contain("Test Opp");
        LastCapturedEmail.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-PNO1005-EDGE-002")]
    public async Task TC_PNO1005_EDGE_002_EmailWithSpecialCharactersInOpportunityName_SendsCorrectly()
    {
        var specialName = "Project \"Alpha\" & Beta <test>";
        await SeedOpportunityAsync(1, specialName);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: specialName,
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-PNO1005-EDGE-003")]
    public async Task TC_PNO1005_EDGE_003_EmailWithMultipleRecipients_SendsOncePerBatch()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "dm1@unops.org");
        await SeedUserAsync(2, "dm2@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1, 2 }));

        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
        LastCapturedEmail!.EmailReceivers.Should().HaveCount(2);
        LastCapturedEmail.EmailReceivers.Should().Contain("dm1@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("dm2@unops.org");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-PNO1005-EDGE-004")]
    public async Task TC_PNO1005_EDGE_004_EmailWithNoCCList_SendsSuccessfully()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "dm@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-PNO1005-EDGE-005")]
    public async Task TC_PNO1005_EDGE_005_EmailSubjectForCompletedNotification_MatchesJiraFormat()
    {
        await SeedOpportunityAsync(1, "Budget Review");
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowCompletedAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Budget Review", recipientUserIds: new List<int> { 100 }, performedByUserName: "Approver"));

        LastCapturedEmail!.Title.Should().Be("Opportunity+: Budget Review - Go Decision Approved");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-PNO1005-EDGE-006")]
    public async Task TC_PNO1005_EDGE_006_EmailSubjectForRecalledNotification_MatchesJiraFormat()
    {
        await SeedOpportunityAsync(1, "Regional Initiative");
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowRecalledAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Regional Initiative", performedByUserName: "Recaller"));

        LastCapturedEmail!.Title.Should().Be("Opportunity+: Regional Initiative - Submission Recalled");
    }

    #endregion

    #region Integration (6 tests)

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-PNO1005-INT-001")]
    public async Task TC_PNO1005_INT_001_FullFlow_SeedOpportunityAndUser_SendApprovalRequest_CaptureEmailModel()
    {
        await SeedOpportunityAsync(1, "Full Flow Opp");
        await SeedUserAsync(1, "approver@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Full Flow Opp", recipientUserIds: new List<int> { 1 }));

        capturedModel.Should().NotBeNull();
        capturedModel!.EntityName.Should().Be("Full Flow Opp");
        capturedModel.EntityUrl.Should().Contain("opportunities/1");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-PNO1005-INT-002")]
    public async Task TC_PNO1005_INT_002_FullFlow_SeedOpportunityAndOM_SendCompletedNotification_VerifyTemplate()
    {
        await SeedOpportunityAsync(1, "Completed Opp");
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowCompletedAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Completed Opp", recipientUserIds: new List<int> { 100 }, performedByUserName: "Approver"));

        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-PNO1005-INT-003")]
    public async Task TC_PNO1005_INT_003_FullFlow_SeedOpportunityAndOM_SendRejectedNotification_VerifyCommentInModel()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        await SeedCompletedSubmitWorkflowLogAsync("1", 1);
        WorkflowRejectedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRejectedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRejectedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyWorkflowRejectedAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }, comment: "Rejection rationale"));

        capturedModel!.Comment.Should().Be("Rejection rationale");
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-PNO1005-INT-004")]
    public async Task TC_PNO1005_INT_004_SendingApprovalRequest_CreatesBothInSystemNotificationAndEmail()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        LastCapturedEmail.Should().NotBeNull();
        var inSystemNotifications = await DbContext.Notifications
            .Where(n => n.Category == PaoWorkflowNotificationService.WorkflowApprovalCategory && n.EntityId == 1)
            .ToListAsync();
        inSystemNotifications.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-PNO1005-INT-005")]
    public async Task TC_PNO1005_INT_005_MultipleEmailEvents_SubmitAndApprove_ProduceSeparateEmailsWithDistinctSubjects()
    {
        await SeedOpportunityAsync(1, "Multi-Event Opp");
        await SeedUserAsync(1, "dm@unops.org");
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);

        var capturedEmails = new List<EmailMessage>();
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback<EmailMessage, object, string?>((msg, _, _) => capturedEmails.Add(msg))
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Multi-Event Opp", recipientUserIds: new List<int> { 1 }));
        await NotificationService.NotifyWorkflowCompletedAsync(
            BuildWorkflowNotification(entityId: "1", entityDisplayName: "Multi-Event Opp", recipientUserIds: new List<int> { 100 }, performedByUserName: "Approver"));

        capturedEmails.Should().HaveCount(2);
        capturedEmails.Select(e => e.Title).Distinct().Should().HaveCount(2);
        capturedEmails.Should().Contain(e => e.Title.Contains("Action Required"));
        capturedEmails.Should().Contain(e => e.Title.Contains("Go Decision Approved"));
    }

    [Fact]
    [Trait("JiraRef", "PNO-1005")]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-PNO1005-INT-006")]
    public async Task TC_PNO1005_INT_006_EmailEntityUrl_UsesConfiguredAppBaseUrlFromIConfiguration()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        capturedModel!.EntityUrl.Should().StartWith("https://test.pao.unops.org");
        capturedModel.EntityUrl.Should().Contain("/partnerships/opportunities/1");
    }

    #endregion
}
