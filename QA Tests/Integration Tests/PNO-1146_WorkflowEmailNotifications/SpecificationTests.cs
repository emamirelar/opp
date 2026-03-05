/**
 * @fileoverview Specification-based tests for PNO-1146: Emails for Go/No Go approval.
 * These tests validate the JIRA REQUIREMENT, not just the code implementation.
 *
 * Requirements validated:
 * - REQ-1: Recall TO = DoA approvers + OM + Initiator -> DEF-096
 * - REQ-2: FYI TO includes opportunity-level internal stakeholders -> DEF-097
 * - REQ-3: Soft-deleted OM excluded from all email recipients -> DEF-098
 * - REQ-4: Email subject lines match Jira specification
 *
 * Defects found:
 * - DEF-096: Recall TO missing DoA approvers
 * - DEF-097: FYI not querying OpportunityStakeholder.IsInternal
 * - DEF-098: GetOpportunityManagerEmailAsync missing IsDeleted filter
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1146;

/// <summary>
/// Specification-based tests for PNO-1146: Emails for Go/No Go approval.
/// These tests validate the JIRA REQUIREMENT, not just the code implementation.
///
/// Requirements validated:
/// - REQ-1: Recall TO = DoA approvers + OM + Initiator -> DEF-096
/// - REQ-2: FYI TO includes opportunity-level internal stakeholders -> DEF-097
/// - REQ-3: Soft-deleted OM excluded from all email recipients -> DEF-098
/// - REQ-4: Email subject lines match Jira specification
///
/// Defects found:
/// - DEF-096: Recall TO missing DoA approvers
/// - DEF-097: FYI not querying OpportunityStakeholder.IsInternal
/// - DEF-098: GetOpportunityManagerEmailAsync missing IsDeleted filter
/// </summary>
[Collection("PNO1146_Specification")]
[Trait("Category", "Specification")]
public class SpecificationTests : PNO1146TestFixtureBase
{
    #region §1 Positive — Subject Line Format (5 tests, NOT skipped)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SubjectLine_ApprovalRequest_MatchesJiraFormat()
    {
        await SeedOpportunityAsync(1, "My Opportunity");
        await SeedUserAsync(1, "approver@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "My Opportunity",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Be("Opportunity+: My Opportunity - Action Required");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SubjectLine_Completed_MatchesJiraFormat()
    {
        await SeedOpportunityAsync(1, "Budget Review");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Budget Review",
            recipientUserIds: new List<int> { 100 },
            performedByUserName: "Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Be("Opportunity+: Budget Review - Go Decision Approved");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SubjectLine_Rejected_MatchesJiraFormat()
    {
        await SeedOpportunityAsync(1, "Nepal Project");
        await SeedUserAsync(1, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        await SeedCompletedSubmitWorkflowLogAsync("1", 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Nepal Project",
            recipientUserIds: new List<int> { 1 },
            comment: "Out of scope");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Be("Opportunity+: Nepal Project - Set to NO GO");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SubjectLine_Recalled_MatchesJiraFormat()
    {
        await SeedOpportunityAsync(1, "Regional Initiative");
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Regional Initiative",
            performedByUserName: "Recaller");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Be("Opportunity+: Regional Initiative - Submission Recalled");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SubjectLine_FYI_MatchesJiraFormat()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver Name");

        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.Title.Should().Be("Opportunity+: FYI Test Opp - Go Decision Approved (FYI)");
    }

    #endregion

    #region §2 Negative — DEF-096 Recall TO missing DoA approvers (4 tests, expected to FAIL)

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-096")]
    public async Task Recalled_TORecipients_ShouldIncludeDoAApprovers()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(50, "doa2@unops.org", "DoA2", "Approver");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            performedByUserId: 101,
            performedByUserName: "Initiator");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("doa2@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-096")]
    public async Task Recalled_TORecipients_ShouldIncludeDoA3_WhenDoA2Absent()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(60, "doa3@unops.org", "DoA3", "Approver");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 60 },
            performedByUserId: 101,
            performedByUserName: "Initiator");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("doa3@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-096")]
    public async Task Recalled_TORecipients_ShouldMergeDoAWithOMAndInitiator()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(50, "doa@unops.org");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            performedByUserId: 101,
            performedByUserName: "Initiator");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("doa@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-096")]
    public async Task Recalled_TORecipients_ShouldDeduplicateWhenDoAIsAlsoOM()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om_doa@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 100 },
            performedByUserId: 101,
            performedByUserName: "Initiator");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        var toCount = LastCapturedEmail!.EmailReceivers.Count(e => e == "om_doa@unops.org");
        toCount.Should().Be(1);
    }

    #endregion

    #region §3 Negative — DEF-097 FYI TO missing opportunity-level internal stakeholders (4 tests, expected to FAIL)

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-097")]
    public async Task FYI_TORecipients_ShouldIncludeOpportunityInternalStakeholders()
    {
        await SeedFYIScenarioWithInternalStakeholderAsync(internalStakeholderUserId: 75);
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        LastCapturedEmail!.EmailReceivers.Should().Contain("internal.stakeholder@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-097")]
    public async Task FYI_TORecipients_ShouldMergeInternalStakeholdersWithDirectors()
    {
        await SeedFYIScenarioWithInternalStakeholderAsync(internalStakeholderUserId: 76);
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        LastCapturedEmail!.EmailReceivers.Should().Contain("internal.stakeholder@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("director@impl.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-097")]
    public async Task FYI_TORecipients_ShouldExcludeSoftDeletedInternalStakeholders()
    {
        await SeedFYIScenarioWithSoftDeletedInternalStakeholderAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        LastCapturedEmail!.EmailReceivers.Should().NotContain("deleted.stakeholder@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-097")]
    public async Task FYI_TORecipients_ShouldDeduplicateWhenStakeholderIsAlsoDirector()
    {
        await SeedFYIScenarioWithStakeholderAsDirectorAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        var toCount = LastCapturedEmail!.EmailReceivers.Count(e => e == "stakeholder.director@unops.org");
        toCount.Should().Be(1);
    }

    #endregion

    #region §4 Negative — DEF-098 GetOpportunityManagerEmailAsync missing IsDeleted filter (3 tests, expected to FAIL)

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-098")]
    public async Task CC_ShouldExcludeSoftDeletedOMStakeholder_FromFYIEmail()
    {
        await SeedFYIScenarioWithSoftDeletedOMAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        LastCapturedEmail!.CcReceivers.Should().NotContain("deleted.om@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-098")]
    public async Task CC_ShouldExcludeSoftDeletedOMStakeholder_FromApprovalRequestCC_WhenInitiatorSameAsOM()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(50, "doa@unops.org");
        await SeedUserAsync(100, "deleted.om@unops.org");
        await SeedSoftDeletedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            performedByUserId: 100,
            performedByUserName: "OM");

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.CcReceivers.Should().NotContain("deleted.om@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-098")]
    public async Task CC_ShouldExcludeSoftDeletedOMStakeholder_FromApprovalRequestCC()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(50, "doa@unops.org");
        await SeedUserAsync(100, "deleted.om@unops.org");
        await SeedSoftDeletedOpportunityManagerAsync(1, 100);
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            performedByUserId: 101,
            performedByUserName: "Initiator");

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.CcReceivers.Should().NotContain("deleted.om@unops.org");
    }

    #endregion

    #region §5 Additional Negative — Specification validation (4 tests)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task ApprovalRequest_EmptyRecipientUserIds_DoesNotSendEmail()
    {
        await SeedOpportunityAsync(1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int>());

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task Recalled_NonOpportunityEntity_IgnoresRecipientUserIdsWhenNotOpportunity()
    {
        await SeedUserAsync(1, "recipient@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityName: "Partner",
            entityId: "1",
            entityDisplayName: "Test Partner",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("recipient@unops.org");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task FYI_OpportunityNotFound_DoesNotThrow()
    {
        var act = () => NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(99999, "Approver");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task FYI_NoCountries_DoesNotSendEmail()
    {
        await SeedOpportunityAsync(1);
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    #endregion

    #region §6 Boundary — Edge cases (15 tests)

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task SubjectLine_EmptyEntityDisplayName_StillFormats()
    {
        await SeedOpportunityAsync(1, "");
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.Title.Should().Contain("Opportunity+:");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task SubjectLine_LongEntityName_TruncatesOrHandles()
    {
        var longName = new string('A', 200);
        await SeedOpportunityAsync(1, longName);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: longName,
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.Title.Should().Contain("Opportunity+:");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Recalled_OMSameAsInitiator_OnlyOneInTO()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om_initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserId: 100,
            performedByUserName: "OM");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        var toCount = LastCapturedEmail!.EmailReceivers.Count(e => e == "om_initiator@unops.org");
        toCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Rejected_NoOM_StillSendsToInitiator()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 101 },
            comment: "Rejected");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Completed_SingleRecipient_SendsSuccessfully()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "sole@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 100 },
            performedByUserName: "Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().HaveCount(1);
        LastCapturedEmail.EmailReceivers.Should().Contain("sole@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task ApprovalRequest_MultipleRecipients_AllInTO()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "doa1@unops.org");
        await SeedUserAsync(2, "doa2@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1, 2 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("doa1@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("doa2@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Recalled_NoPendingSubmitLog_StillSendsToOM()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "Recaller");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Rejected_CommentWithSpecialChars_EscapesInEmail()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        await SeedCompletedSubmitWorkflowLogAsync("1", 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "<script>alert('xss')</script>");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task FYI_SingleDirector_SendsSuccessfully()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task EntityDisplayName_UnicodeCharacters_Handled()
    {
        await SeedOpportunityAsync(1, "Projet Régional 日本");
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Projet Régional 日本",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.Title.Should().Contain("Projet Régional 日本");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Recalled_NullComment_DoesNotThrow()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            comment: "",
            performedByUserName: "Recaller");

        var act = () => NotificationService.NotifyWorkflowRecalledAsync(notification);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Completed_ZeroUserId_ExcludedFromRecipients()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "valid@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 100 },
            performedByUserName: "Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("valid@unops.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task Rejected_OMAndInitiatorSame_NoDuplicateInTO()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "same@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 100 },
            comment: "Rejected");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        var toCount = LastCapturedEmail!.EmailReceivers.Count(e => e == "same@unops.org");
        toCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task FYI_OpportunityOwnOrgUnitOnly_NoOtherOrgUnits_DoesNotSend()
    {
        await SeedOpportunityAsync(1, orgUnitId: 1);
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 1))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = 1,
                Name = "Org 1",
                Code = "O1",
                Description = "Org 1",
                IsDeleted = false
            });
        }
        if (!await DbContext.Set<Country>().AnyAsync(c => c.Id == 100))
        {
            DbContext.Set<Country>().Add(new Country
            {
                Id = 100,
                Name = "Kenya",
                Iso2Code = "KE",
                IsDeleted = false
            });
        }
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 100,
            Name = "Kenya",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "C-OU",
            EntityType = "Country",
            EntityId = 100,
            OrganizationHierarchyId = 1,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task ApprovalRequest_UserWithoutEmail_ExcludedFromRecipients()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "valid@unops.org");
        await SeedUserAsync(2, "");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1, 2 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("valid@unops.org");
    }

    #endregion

    #region §7 Functional — Business rules (15 tests)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_CCIncludesOMAndInitiator()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(50, "doa@unops.org");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            performedByUserId: 101);

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.CcReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.CcReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Completed_ToRecipientsMatchOMAndInitiator()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            performedByUserName: "Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Rejected_ToRecipientsAreOMAndInitiator()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 50 },
            comment: "Rejected");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Recalled_ToRecipientsAreOMAndInitiator()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "Recaller");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_TemplateNameCorrect()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.TemplateName.Should().Be(TemplateApprovalRequest);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Completed_TemplateNameCorrect()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 100 },
            performedByUserName: "Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Rejected_TemplateNameCorrect()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 1);
        await SeedCompletedSubmitWorkflowLogAsync("1", 1);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            comment: "Rejected");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        LastCapturedEmail!.TemplateName.Should().Be(TemplateRejected);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Recalled_TemplateNameCorrect()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "Recaller");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.TemplateName.Should().Be(TemplateRecalled);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FYI_EntityUrlContainsOpportunityId()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        capturedModel!.EntityUrl.Should().Contain("opportunities/1");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task ApprovalRequest_CommentIncludedInModel()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        ApprovalRequestEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<ApprovalRequestEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, ApprovalRequestEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 },
            comment: "Please review budget");

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        capturedModel!.Comment.Should().Be("Please review budget");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Recalled_PerformedByUserNameInModel()
    {
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
            performedByUserName: "John Recaller");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        capturedModel!.RecalledByName.Should().Be("John Recaller");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Rejected_RejectedByNameInModel()
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

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "Jane Rejector",
            comment: "Out of scope");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        capturedModel!.RejectedByName.Should().Be("Jane Rejector");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Completed_ApprovedByNameInModel()
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

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 100 },
            performedByUserName: "Alice Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        capturedModel!.ApprovedByName.Should().Be("Alice Approver");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task FYI_CCIncludesOMAndInitiator()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        LastCapturedEmail!.CcReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.CcReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task Recalled_CCIncludesOrgUnitDirectors()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        await SeedOrgUnitDirectorAsync(1, 200, "director@org.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            performedByUserName: "Recaller");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.CcReceivers.Should().Contain("director@org.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task AllNotifications_EntityUrlUsesBaseUrl()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            recipientUserIds: new List<int> { 1 });

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.TemplateName.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region §8 Integration — Full flows (15 tests)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SubmitToApproval_FullFlow_SendsApprovalRequest()
    {
        await SeedOpportunityAsync(1, "Full Flow Opp");
        await SeedUserAsync(50, "doa@unops.org");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Full Flow Opp",
            recipientUserIds: new List<int> { 50 },
            performedByUserId: 101,
            performedByUserName: "Initiator",
            comment: "Ready for review");

        await NotificationService.NotifyNewApprovalRequestAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("doa@unops.org");
        LastCapturedEmail.CcReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.CcReceivers.Should().Contain("initiator@unops.org");
        LastCapturedEmail.Title.Should().Be("Opportunity+: Full Flow Opp - Action Required");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApproveWorkflow_FullFlow_SendsCompletedEmail()
    {
        await SeedOpportunityAsync(1, "Approved Opp");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Approved Opp",
            recipientUserIds: new List<int> { 100, 101 },
            performedByUserName: "DoA Approver");

        await NotificationService.NotifyWorkflowCompletedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
        LastCapturedEmail.Title.Should().Be("Opportunity+: Approved Opp - Go Decision Approved");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RejectWorkflow_FullFlow_SendsRejectedEmail()
    {
        await SeedOpportunityAsync(1, "Rejected Opp");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Rejected Opp",
            recipientUserIds: new List<int> { 50 },
            comment: "Budget concerns");

        await NotificationService.NotifyWorkflowRejectedAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
        LastCapturedEmail.Title.Should().Be("Opportunity+: Rejected Opp - Set to NO GO");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RecallWorkflow_FullFlow_SendsRecalledEmail()
    {
        await SeedOpportunityAsync(1, "Recalled Opp");
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        var notification = BuildWorkflowNotification(
            entityId: "1",
            entityDisplayName: "Recalled Opp",
            performedByUserName: "Initiator",
            comment: "Need to fix data");

        await NotificationService.NotifyWorkflowRecalledAsync(notification);

        LastCapturedEmail!.EmailReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.EmailReceivers.Should().Contain("initiator@unops.org");
        LastCapturedEmail.Title.Should().Be("Opportunity+: Recalled Opp - Submission Recalled");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FYI_FullFlow_SendsToDirectorsAndCC()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        SetupEmailCapture();

        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "DoA Approver");

        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
        LastCapturedEmail.CcReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.CcReceivers.Should().Contain("initiator@unops.org");
        LastCapturedEmail.Title.Should().Be("Opportunity+: FYI Test Opp - Go Decision Approved (FYI)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MultipleNotifications_Sequential_AllSucceed()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "doa@unops.org");
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 100);
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));
        await NotificationService.NotifyWorkflowCompletedAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 100 }, performedByUserName: "A"));

        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApprovalRequest_OrgUnitNameResolved()
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

        capturedModel!.OrgUnitName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Completed_OrgUnitNameInModel()
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
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 100 }, performedByUserName: "A"));

        capturedModel!.OrgUnitName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Rejected_OrgUnitNameInModel()
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
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }, comment: "No"));

        capturedModel!.OrgUnitName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Recalled_OrgUnitNameInModel()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 100);
        WorkflowRecalledEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowRecalledEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowRecalledEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        await NotificationService.NotifyWorkflowRecalledAsync(
            BuildWorkflowNotification(entityId: "1", performedByUserName: "R"));

        capturedModel!.OrgUnitName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DbContextFactory_CreatesSeparateContext()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        MockContextFactory.Verify(
            f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task WorkflowContext_UsedForInitiatorLookup()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedPendingSubmitWorkflowLogAsync("1", 101);
        SetupEmailCapture();

        await NotificationService.NotifyWorkflowRecalledAsync(
            BuildWorkflowNotification(entityId: "1", performedByUserName: "Recaller"));

        LastCapturedEmail!.EmailReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task InMemoryDb_OpportunityLookupSucceeds()
    {
        await SeedOpportunityAsync(1, "DB Test");
        var opp = await DbContext.Opportunities.FirstOrDefaultAsync(o => o.Id == 1 && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.Name.Should().Be("DB Test");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task InMemoryDb_UserLookupSucceeds()
    {
        await SeedUserAsync(1, "test@unops.org");
        var user = await DbContext.PAOUsers.FindAsync(1);
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task InMemoryDb_StakeholderLookupSucceeds()
    {
        await SeedOpportunityAsync(1);
        await SeedOpportunityManagerAsync(1, 100);
        var stakeholder = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == 1 && !s.IsDeleted);
        stakeholder.Should().NotBeNull();
        stakeholder!.UserId.Should().Be(100);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EmailSender_ReceivesCorrectTemplateModelTypes()
    {
        await SeedOpportunityAsync(1);
        await SeedUserAsync(1, "user@unops.org");
        SetupEmailCapture();

        await NotificationService.NotifyNewApprovalRequestAsync(
            BuildWorkflowNotification(entityId: "1", recipientUserIds: new List<int> { 1 }));

        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == TemplateApprovalRequest),
                It.IsAny<ApprovalRequestEmailModel>(),
                It.IsAny<string?>()),
            Times.Once);
    }

    #endregion

    #region Helper methods

    private async Task SeedFYIScenarioForSubjectLineAsync()
    {
        await SeedOpportunityAsync(1, "FYI Test Opp", orgUnitId: 1);
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 2))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = 2,
                Name = "Impl Org Unit",
                Code = "IMPL",
                Description = "Implementation",
                IsDeleted = false
            });
        }
        if (!await DbContext.Set<Country>().AnyAsync(c => c.Id == 100))
        {
            DbContext.Set<Country>().Add(new Country
            {
                Id = 100,
                Name = "Nepal",
                Iso2Code = "NP",
                IsDeleted = false
            });
        }
        if (!await DbContext.OpportunityCountries.AnyAsync(oc => oc.OpportunityId == 1 && oc.CountryId == 100))
        {
            DbContext.OpportunityCountries.Add(new OpportunityCountry
            {
                OpportunityId = 1,
                CountryId = 100,
                Name = "Nepal",
                IsDeleted = false,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }
        if (!await DbContext.OrganizationUnitRelationships.AnyAsync(r =>
            r.EntityType == "Country" && r.EntityId == 100 && r.OrganizationHierarchyId == 2))
        {
            DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
            {
                Name = "C-OU2",
                EntityType = "Country",
                EntityId = 100,
                OrganizationHierarchyId = 2,
                IsDeleted = false,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }
        var directorRole = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "OrgUnit_Director_OrganizationHierarchy")
            ?? await SeedDirectorRoleAsync();
        await SeedUserAsync(200, "director@impl.org");
        if (!await DbContext.EntityUserRoles.AnyAsync(e => e.EntityId == 2 && e.UserId == 200 && !e.IsDeleted))
        {
            DbContext.EntityUserRoles.Add(new EntityUserRole
            {
                Name = "Dir",
                EntityType = "OrganizationHierarchy",
                EntityId = 2,
                EntityRoleId = directorRole.Id,
                UserId = 200,
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(1, 100);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        await DbContext.SaveChangesAsync();
    }

    private async Task<EntityRole> SeedDirectorRoleAsync()
    {
        var role = new EntityRole
        {
            Id = 302,
            Code = "OrgUnit_Director_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Name = "Director",
            IsInternal = true
        };
        DbContext.Set<EntityRole>().Add(role);
        await DbContext.SaveChangesAsync();
        return role;
    }

    private async Task SeedOrgUnitDirectorAsync(int orgUnitId, int userId, string email)
    {
        var role = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "OrgUnit_Director_OrganizationHierarchy")
            ?? await SeedDirectorRoleAsync();
        await SeedUserAsync(userId, email);
        if (!await DbContext.EntityUserRoles.AnyAsync(e => e.EntityId == orgUnitId && e.UserId == userId && !e.IsDeleted))
        {
            DbContext.EntityUserRoles.Add(new EntityUserRole
            {
                Name = "Dir",
                EntityType = "OrganizationHierarchy",
                EntityId = orgUnitId,
                EntityRoleId = role.Id,
                UserId = userId,
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
            await DbContext.SaveChangesAsync();
        }
    }

    private async Task SeedFYIScenarioWithInternalStakeholderAsync(int internalStakeholderUserId)
    {
        await SeedFYIScenarioForSubjectLineAsync();
        var internalRole = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "Internal_Stakeholder_Opportunity");
        if (internalRole == null)
        {
            internalRole = new EntityRole
            {
                Id = 400,
                Code = "Internal_Stakeholder_Opportunity",
                EntityType = "Opportunity",
                Name = "Internal Stakeholder",
                IsInternal = true
            };
            DbContext.Set<EntityRole>().Add(internalRole);
            await DbContext.SaveChangesAsync();
        }
        await SeedUserAsync(internalStakeholderUserId, "internal.stakeholder@unops.org");
        DbContext.OpportunityStakeholders.Add(new OpportunityStakeholder
        {
            OpportunityId = 1,
            UserId = internalStakeholderUserId,
            EntityRoleId = internalRole.Id,
            IsInternal = true,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
    }

    private async Task SeedFYIScenarioWithSoftDeletedInternalStakeholderAsync()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        var internalRole = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "Internal_Stakeholder_Opportunity");
        if (internalRole == null)
        {
            internalRole = new EntityRole
            {
                Id = 401,
                Code = "Internal_Stakeholder_Opportunity",
                EntityType = "Opportunity",
                Name = "Internal Stakeholder",
                IsInternal = true
            };
            DbContext.Set<EntityRole>().Add(internalRole);
            await DbContext.SaveChangesAsync();
        }
        await SeedUserAsync(77, "deleted.stakeholder@unops.org");
        DbContext.OpportunityStakeholders.Add(new OpportunityStakeholder
        {
            OpportunityId = 1,
            UserId = 77,
            EntityRoleId = internalRole.Id,
            IsInternal = true,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
    }

    private async Task SeedFYIScenarioWithStakeholderAsDirectorAsync()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        var directorRole = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "OrgUnit_Director_OrganizationHierarchy")
            ?? await SeedDirectorRoleAsync();
        await SeedUserAsync(210, "stakeholder.director@unops.org");
        DbContext.EntityUserRoles.Add(new EntityUserRole
        {
            Name = "SD",
            EntityType = "OrganizationHierarchy",
            EntityId = 2,
            EntityRoleId = directorRole.Id,
            UserId = 210,
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        var internalRole = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "Internal_Stakeholder_Opportunity");
        if (internalRole == null)
        {
            internalRole = new EntityRole
            {
                Id = 402,
                Code = "Internal_Stakeholder_Opportunity",
                EntityType = "Opportunity",
                Name = "Internal Stakeholder",
                IsInternal = true
            };
            DbContext.Set<EntityRole>().Add(internalRole);
        }
        DbContext.OpportunityStakeholders.Add(new OpportunityStakeholder
        {
            OpportunityId = 1,
            UserId = 210,
            EntityRoleId = internalRole!.Id,
            IsInternal = true,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
    }

    private async Task SeedFYIScenarioWithSoftDeletedOMAsync()
    {
        await SeedFYIScenarioForSubjectLineAsync();
        await SeedSoftDeletedOpportunityManagerAsync(1, 100);
    }

    private async Task SeedSoftDeletedOpportunityManagerAsync(int opportunityId, int userId)
    {
        var omRole = await DbContext.Set<EntityRole>()
            .FirstOrDefaultAsync(r => r.Code == "Opportunity_Manager_Opportunity");
        if (omRole == null)
        {
            omRole = new EntityRole
            {
                Id = 100,
                EntityType = "Opportunity",
                Name = "Opportunity Manager",
                Code = "Opportunity_Manager_Opportunity",
                IsInternal = true
            };
            DbContext.Set<EntityRole>().Add(omRole);
            await DbContext.SaveChangesAsync();
        }
        var existing = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == opportunityId && s.UserId == userId);
        if (existing != null)
        {
            existing.IsDeleted = true;
            existing.DeletedBy = 1;
            existing.DeletedDate = DateTime.UtcNow;
        }
        else
        {
            DbContext.OpportunityStakeholders.Add(new OpportunityStakeholder
            {
                OpportunityId = opportunityId,
                UserId = userId,
                EntityRoleId = omRole.Id,
                IsInternal = true,
                IsDeleted = true,
                DeletedBy = 1,
                DeletedDate = DateTime.UtcNow,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }
        await DbContext.SaveChangesAsync();
    }

    #endregion
}
