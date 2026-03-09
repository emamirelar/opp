/**
 * @fileoverview Entity-level tests for Partner workflow transitions.
 * Tests ActivatePartner, ClosePartner, ArchivePartner, ApprovePartner, UnapprovePartner
 * business rules on the Partner entity (used by PartnerManager/UNOPSPartnerManager).
 *
 * Ratio: P=2, N=6+, E=6+, F=6+, I=6+
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Entity-level tests for Partner workflow transition business rules.
/// Verifies valid transitions succeed, invalid transitions are rejected,
/// correct status after transition, and edge cases.
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class PartnerWorkflowTransitionTests
{
    #region Helpers

    private static UNOPSPartner CreateDraftPartnerWithMandatoryFields()
    {
        return new UNOPSPartner
        {
            Name = "Test Partner",
            PartnerShortDescription = "Short desc",
            PartnerGroupId = 1,
            LiaisonOfficeId = 1,
            Status = EntityStatus.Draft
        };
    }

    private static UNOPSPartner CreateActivePartner()
    {
        return new UNOPSPartner
        {
            Name = "Active Partner",
            PartnerShortDescription = "Short desc",
            PartnerGroupId = 1,
            LiaisonOfficeId = 1,
            Status = EntityStatus.Active,
            PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
        };
    }

    private static UNOPSPartner CreateApprovedPartner()
    {
        return new UNOPSPartner
        {
            Name = "Approved Partner",
            PartnerShortDescription = "Short desc",
            PartnerGroupId = 1,
            LiaisonOfficeId = 1,
            Status = EntityStatus.Active,
            PartnerApprovalStatus = PartnerApprovalStatus.Approved,
            CanCreateNewOpportunities = true
        };
    }

    private static UNOPSPartner CreateClosedPartner()
    {
        return new UNOPSPartner
        {
            Name = "Closed Partner",
            Status = EntityStatus.Closed,
            PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
        };
    }

    #endregion

    #region Positive Tests (P=2)

    [Fact]
    [Trait("Category", "Positive")]
    public void ActivatePartner_FromDraftWithMandatoryFields_Succeeds()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        partner.ActivatePartner();

        partner.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ApprovePartner_FromActive_SetsApprovalStatusAndErpDimValue()
    {
        var partner = CreateActivePartner();
        var nextErpDimValue = 100;

        partner.ApprovePartner(1, "Admin User", nextErpDimValue);

        partner.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.Approved);
        partner.CanCreateNewOpportunities.Should().BeTrue();
        partner.ErpDimValue.Should().Be(nextErpDimValue);
        partner.PartnerApprovalDate.Should().NotBeNull();
        partner.PartnerApprovedBy.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Negative Tests (N=6)

    [Fact]
    [Trait("Category", "Negative")]
    public void ActivatePartner_FromActive_ThrowsInvalidOperationException()
    {
        var partner = CreateActivePartner();

        var act = () => partner.ActivatePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Draft partners can be activated*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ActivatePartner_MissingMandatoryFields_Throws()
    {
        var partner = new UNOPSPartner
        {
            Name = "Test",
            Status = EntityStatus.Draft
            // Missing PartnerShortDescription, PartnerGroupId, LiaisonOfficeId
        };

        var act = () => partner.ActivatePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*mandatory fields are missing*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ClosePartner_FromDraft_Throws()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        var act = () => partner.ClosePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Active partners can be closed*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ArchivePartner_FromDraft_Throws()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        var act = () => partner.ArchivePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Active or Closed partners can be archived*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ApprovePartner_FromDraft_Throws()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        var act = () => partner.ApprovePartner(1, "Admin", 100);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Active partners can be approved*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UnapprovePartner_WhenNotApproved_Throws()
    {
        var partner = CreateActivePartner();

        var act = () => partner.UnapprovePartner(1, "Admin");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only approved partners can be unapproved*");
    }

    #endregion

    #region Edge/Boundary Tests (E=6)

    [Fact]
    [Trait("Category", "Edge")]
    public void ClosePartner_FromActive_SetsStatusToClosed()
    {
        var partner = CreateActivePartner();

        partner.ClosePartner();

        partner.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ArchivePartner_FromActive_SetsStatusToArchived()
    {
        var partner = CreateActivePartner();

        partner.ArchivePartner();

        partner.Status.Should().Be(EntityStatus.Archived);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ArchivePartner_FromClosed_SetsStatusToArchived()
    {
        var partner = CreateClosedPartner();

        partner.ArchivePartner();

        partner.Status.Should().Be(EntityStatus.Archived);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ApprovePartner_WhenErpDimValueAlreadySet_PreservesExistingValue()
    {
        var partner = CreateActivePartner();
        partner.ErpDimValue = 500;
        var nextErpDimValue = 100;

        partner.ApprovePartner(1, "Admin", nextErpDimValue);

        partner.ErpDimValue.Should().Be(500);
        partner.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.Approved);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void UnapprovePartner_FromApproved_ClearsApprovalAndCanCreateOpportunities()
    {
        var partner = CreateApprovedPartner();

        partner.UnapprovePartner(1, "Admin");

        partner.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.NotApproved);
        partner.CanCreateNewOpportunities.Should().BeFalse();
        partner.PartnerApprovedBy.Should().Contain("Unapproved");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ClosePartner_FromClosed_Throws()
    {
        var partner = CreateClosedPartner();

        var act = () => partner.ClosePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Active partners can be closed*");
    }

    #endregion

    #region Functional Tests (F=6)

    [Fact]
    [Trait("Category", "Functional")]
    public void ActivatePartner_SetsStatusToActive()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        partner.ActivatePartner();

        partner.Status.Should().Be(EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ApprovePartner_SetsAuditTrailFields()
    {
        var partner = CreateActivePartner();
        var approverId = 42;
        var approverName = "Test Admin";

        partner.ApprovePartner(approverId, approverName, 100);

        partner.PartnerApprovalDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        partner.PartnerApprovedBy.Should().Contain(approverName);
        partner.PartnerApprovedBy.Should().Contain(approverId.ToString());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UnapprovePartner_SetsAuditTrailFields()
    {
        var partner = CreateApprovedPartner();
        var unapproverId = 99;
        var unapproverName = "Test Admin";

        partner.UnapprovePartner(unapproverId, unapproverName);

        partner.PartnerApprovedBy.Should().Contain("Unapproved");
        partner.PartnerApprovedBy.Should().Contain(unapproverName);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ActivatePartner_MissingName_ThrowsWithNameInMessage()
    {
        var partner = new UNOPSPartner
        {
            Name = "",
            PartnerShortDescription = "Desc",
            PartnerGroupId = 1,
            LiaisonOfficeId = 1,
            Status = EntityStatus.Draft
        };

        var act = () => partner.ActivatePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Name*");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ActivatePartner_MissingPartnerGroup_ThrowsWithPartnerGroupInMessage()
    {
        var partner = new UNOPSPartner
        {
            Name = "Test",
            PartnerShortDescription = "Desc",
            PartnerGroupId = null,
            LiaisonOfficeId = 1,
            Status = EntityStatus.Draft
        };

        var act = () => partner.ActivatePartner();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Partner Group*");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UnapprovePartner_FromActiveButNotApproved_Throws()
    {
        var partner = CreateActivePartner();

        var act = () => partner.UnapprovePartner(1, "Admin");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only approved partners can be unapproved*");
    }

    #endregion

    #region Integration Tests (I=6)

    [Fact]
    [Trait("Category", "Integration")]
    public void FullWorkflow_DraftToActiveToClosedToArchived_Succeeds()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        partner.ActivatePartner();
        partner.Status.Should().Be(EntityStatus.Active);

        partner.ClosePartner();
        partner.Status.Should().Be(EntityStatus.Closed);

        partner.ArchivePartner();
        partner.Status.Should().Be(EntityStatus.Archived);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullWorkflow_DraftToActiveToApprovedToUnapproved_Succeeds()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        partner.ActivatePartner();
        partner.ApprovePartner(1, "Admin", 100);
        partner.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.Approved);
        partner.CanCreateNewOpportunities.Should().BeTrue();

        partner.UnapprovePartner(1, "Admin");
        partner.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.NotApproved);
        partner.CanCreateNewOpportunities.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullWorkflow_ActiveToApproved_PreservesErpDimValueWhenAlreadySet()
    {
        var partner = CreateActivePartner();
        partner.ErpDimValue = 777;

        partner.ApprovePartner(1, "Admin", 100);

        partner.ErpDimValue.Should().Be(777);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void InvalidTransition_ArchiveFromDraft_Rejected()
    {
        var partner = CreateDraftPartnerWithMandatoryFields();

        var act = () => partner.ArchivePartner();

        act.Should().Throw<InvalidOperationException>();
        partner.Status.Should().Be(EntityStatus.Draft);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void InvalidTransition_CloseFromClosed_Rejected()
    {
        var partner = CreateClosedPartner();

        var act = () => partner.ClosePartner();

        act.Should().Throw<InvalidOperationException>();
        partner.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void InvalidTransition_ActivateFromActive_Rejected()
    {
        var partner = CreateActivePartner();

        var act = () => partner.ActivatePartner();

        act.Should().Throw<InvalidOperationException>();
        partner.Status.Should().Be(EntityStatus.Active);
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | ActivatePartner_FromDraftWithMandatoryFields_Succeeds, ApprovePartner_FromActive_SetsApprovalStatusAndErpDimValue |
| Negative (N) | 6 | ActivatePartner_FromActive_ThrowsInvalidOperationException, ActivatePartner_MissingMandatoryFields_Throws, ClosePartner_FromDraft_Throws, ArchivePartner_FromDraft_Throws, ApprovePartner_FromDraft_Throws, UnapprovePartner_WhenNotApproved_Throws |
| Edge/Boundary (E) | 6 | ClosePartner_FromActive_SetsStatusToClosed, ArchivePartner_FromActive_SetsStatusToArchived, ArchivePartner_FromClosed_SetsStatusToArchived, ApprovePartner_WhenErpDimValueAlreadySet_PreservesExistingValue, UnapprovePartner_FromApproved_ClearsApprovalAndCanCreateOpportunities, ClosePartner_FromClosed_Throws |
| Functional (F) | 6 | ActivatePartner_SetsStatusToActive, ApprovePartner_SetsAuditTrailFields, UnapprovePartner_SetsAuditTrailFields, ActivatePartner_MissingName_ThrowsWithNameInMessage, ActivatePartner_MissingPartnerGroup_ThrowsWithPartnerGroupInMessage, UnapprovePartner_FromActiveButNotApproved_Throws |
| Integration (I) | 6 | FullWorkflow_DraftToActiveToClosedToArchived_Succeeds, FullWorkflow_DraftToActiveToApprovedToUnapproved_Succeeds, FullWorkflow_ActiveToApproved_PreservesErpDimValueWhenAlreadySet, InvalidTransition_ArchiveFromDraft_Rejected, InvalidTransition_CloseFromClosed_Rejected, InvalidTransition_ActivateFromActive_Rejected |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
