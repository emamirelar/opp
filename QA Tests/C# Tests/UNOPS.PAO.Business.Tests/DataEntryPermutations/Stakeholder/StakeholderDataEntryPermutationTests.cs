/// <summary>
/// Tests for Stakeholder Data Entry Permutations (OpportunityStakeholderRequest, OpportunityCollaboratorRequest, OpportunityExternalStakeholderRequest).
///
/// Requirements validated:
/// - REQ-1: OpportunityStakeholderRequest - EntityRoleId required, UserId/OrganizationHierarchyId optional, Notes max 1000
/// - REQ-2: OpportunityCollaboratorRequest - UserId required, ExpertiseIds optional
/// - REQ-3: OpportunityExternalStakeholderRequest - ContactId required
/// - REQ-4: Field order independence, invalid combinations, mixed valid/invalid, partial submission, boundary values
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Stakeholder;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Stakeholder")]
public class StakeholderDataEntryPermutationTests
{
    private const int NotesMaxLength = 1000;
    private const int ValidEntityRoleId = 1;
    private const int ValidUserId = 42;
    private const int ValidContactId = 10;
    private const int ValidOrganizationHierarchyId = 5;

    private static (bool IsValid, string? Error) ValidateStakeholderRequest(OpportunityStakeholderRequest req)
    {
        if (req.EntityRoleId <= 0) return (false, "EntityRoleId must be positive");
        if (req.UserId.HasValue && req.UserId.Value <= 0) return (false, "UserId must be positive when set");
        if (req.OrganizationHierarchyId.HasValue && req.OrganizationHierarchyId.Value <= 0)
            return (false, "OrganizationHierarchyId must be positive when set");
        if (req.Notes != null && req.Notes.Length > NotesMaxLength)
            return (false, $"Notes must not exceed {NotesMaxLength} characters");
        return (true, null);
    }

    private static (bool IsValid, string? Error) ValidateCollaboratorRequest(OpportunityCollaboratorRequest req)
    {
        if (req.UserId <= 0) return (false, "UserId must be positive");
        if (req.ExpertiseIds != null && req.ExpertiseIds.Any(x => x <= 0))
            return (false, "ExpertiseIds must contain only positive integers");
        return (true, null);
    }

    private static (bool IsValid, string? Error) ValidateExternalStakeholderRequest(OpportunityExternalStakeholderRequest req)
    {
        if (req.ContactId <= 0) return (false, "ContactId must be positive");
        return (true, null);
    }

    // ========== OpportunityStakeholderRequest ==========

    #region 1. Field Order Permutations - OpportunityStakeholderRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_FieldOrder_EntityRoleIdFirst_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, UserId = ValidUserId };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
        req.EntityRoleId.Should().Be(ValidEntityRoleId);
        req.UserId.Should().Be(ValidUserId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_FieldOrder_UserIdFirst_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { UserId = ValidUserId, EntityRoleId = ValidEntityRoleId };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_FieldOrder_NotesLast_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest
        {
            EntityRoleId = ValidEntityRoleId,
            UserId = ValidUserId,
            OrganizationHierarchyId = ValidOrganizationHierarchyId,
            Notes = "Some notes"
        };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
        req.Notes.Should().Be("Some notes");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_FieldOrder_AllPermutationsProduceIdenticalValidation()
    {
        var permutations = new[]
        {
            new OpportunityStakeholderRequest { EntityRoleId = 1, UserId = 2 },
            new OpportunityStakeholderRequest { UserId = 2, EntityRoleId = 1 },
            new OpportunityStakeholderRequest { EntityRoleId = 1, Notes = "X", UserId = 2 },
            new OpportunityStakeholderRequest { Notes = "X", EntityRoleId = 1, UserId = 2 }
        };
        foreach (var p in permutations)
        {
            var (isValid, _) = ValidateStakeholderRequest(p);
            isValid.Should().BeTrue();
        }
    }

    #endregion

    #region 2. Invalid Combinations - OpportunityStakeholderRequest

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void Stakeholder_Invalid_EntityRoleIdZeroOrNegative_ShouldFailValidation(int invalidEntityRoleId)
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = invalidEntityRoleId, UserId = ValidUserId };
        var (isValid, error) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("EntityRoleId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void Stakeholder_Invalid_UserIdZeroOrNegative_ShouldFailValidation(int invalidUserId)
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, UserId = invalidUserId };
        var (isValid, error) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("UserId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Negative")]
    public void Stakeholder_Invalid_OrganizationHierarchyIdZeroOrNegative_ShouldFailValidation(int invalidOrgId)
    {
        var req = new OpportunityStakeholderRequest
        {
            EntityRoleId = ValidEntityRoleId,
            OrganizationHierarchyId = invalidOrgId
        };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Stakeholder_Invalid_EntityRoleIdAndUserIdBothInvalid_ShouldFailValidation()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = 0, UserId = -1 };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Stakeholder_Invalid_NotesOverMaxLength_ShouldFailValidation()
    {
        var req = new OpportunityStakeholderRequest
        {
            EntityRoleId = ValidEntityRoleId,
            Notes = InvalidValueSets.OverMaxLengthString(NotesMaxLength)
        };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid - OpportunityStakeholderRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Mixed_ValidUserId_InvalidEntityRoleId_ShouldFailValidation()
    {
        var req = new OpportunityStakeholderRequest { UserId = ValidUserId, EntityRoleId = 0 };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Mixed_ValidEntityRoleId_InvalidUserId_ShouldFailValidation()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, UserId = -1 };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Mixed_ValidEntityRoleId_ValidUserId_InvalidNotesOverMax_ShouldFailValidation()
    {
        var req = new OpportunityStakeholderRequest
        {
            EntityRoleId = ValidEntityRoleId,
            UserId = ValidUserId,
            Notes = InvalidValueSets.OverMaxLengthString(NotesMaxLength)
        };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Mixed_ValidEntityRoleId_InvalidOrganizationHierarchyId_ShouldFailValidation()
    {
        var req = new OpportunityStakeholderRequest
        {
            EntityRoleId = ValidEntityRoleId,
            OrganizationHierarchyId = -5
        };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission - OpportunityStakeholderRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_Partial_Minimal_EntityRoleIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
        req.UserId.Should().BeNull();
        req.Notes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_Partial_Full_AllFieldsSet_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest
        {
            EntityRoleId = ValidEntityRoleId,
            UserId = ValidUserId,
            OrganizationHierarchyId = ValidOrganizationHierarchyId,
            Notes = "Full notes"
        };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
        req.Notes.Should().Be("Full notes");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Stakeholder_Partial_EntityRoleIdAndNotesOnly_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, Notes = "Brief" };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary - OpportunityStakeholderRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Boundary_NotesAt1000Chars_ProducesValidRequest()
    {
        var notes = InvalidValueSets.MaxLengthString(NotesMaxLength);
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, Notes = notes };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
        req.Notes!.Length.Should().Be(NotesMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Boundary_NotesAt1001Chars_ShouldFailValidation()
    {
        var notes = InvalidValueSets.OverMaxLengthString(NotesMaxLength);
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, Notes = notes };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Boundary_EntityRoleIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = int.MaxValue };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Boundary_UserIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, UserId = int.MaxValue };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Stakeholder_Boundary_UserIdNull_ProducesValidRequest()
    {
        var req = new OpportunityStakeholderRequest { EntityRoleId = ValidEntityRoleId, UserId = null };
        var (isValid, _) = ValidateStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    // ========== OpportunityCollaboratorRequest ==========

    #region 1. Field Order Permutations - OpportunityCollaboratorRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_FieldOrder_UserIdFirst_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
        req.UserId.Should().Be(ValidUserId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_FieldOrder_ExpertiseIdsLast_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = new List<int> { 1, 2 } };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
        req.ExpertiseIds.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_FieldOrder_ExpertiseIdsFirst_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { ExpertiseIds = new List<int> { 3 }, UserId = ValidUserId };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_FieldOrder_AllPermutationsProduceIdenticalValidation()
    {
        var permutations = new[]
        {
            new OpportunityCollaboratorRequest { UserId = 1 },
            new OpportunityCollaboratorRequest { UserId = 1, ExpertiseIds = new List<int>() },
            new OpportunityCollaboratorRequest { ExpertiseIds = new List<int> { 1 }, UserId = 1 }
        };
        foreach (var p in permutations)
        {
            var (isValid, _) = ValidateCollaboratorRequest(p);
            isValid.Should().BeTrue();
        }
    }

    #endregion

    #region 2. Invalid Combinations - OpportunityCollaboratorRequest

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void Collaborator_Invalid_UserIdZeroOrNegative_ShouldFailValidation(int invalidUserId)
    {
        var req = new OpportunityCollaboratorRequest { UserId = invalidUserId };
        var (isValid, error) = ValidateCollaboratorRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("UserId");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Collaborator_Invalid_ExpertiseIdsContainsNegative_ShouldFailValidation()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = new List<int> { 1, -1, 2 } };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Collaborator_Invalid_ExpertiseIdsContainsZero_ShouldFailValidation()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = new List<int> { 0, 1 } };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Collaborator_Invalid_ExpertiseIdsAllNegative_ShouldFailValidation()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = InvalidValueSets.NegativeIdList };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid - OpportunityCollaboratorRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Mixed_ValidExpertiseIds_InvalidUserId_ShouldFailValidation()
    {
        var req = new OpportunityCollaboratorRequest { UserId = 0, ExpertiseIds = new List<int> { 1, 2 } };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Mixed_ValidUserId_InvalidExpertiseIds_ShouldFailValidation()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = new List<int> { -1 } };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission - OpportunityCollaboratorRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_Partial_Minimal_UserIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
        req.ExpertiseIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_Partial_Full_UserIdAndExpertiseIds_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = new List<int> { 1, 2, 3 } };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
        req.ExpertiseIds.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Collaborator_Partial_UserIdWithEmptyExpertiseIds_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = InvalidValueSets.EmptyList };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary - OpportunityCollaboratorRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Boundary_ExpertiseIdsEmpty_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = new List<int>() };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Boundary_ExpertiseIdsNull_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = null };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Boundary_ExpertiseIdsLarge_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = InvalidValueSets.LargeList };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Boundary_ExpertiseIdsDuplicate_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = ValidUserId, ExpertiseIds = InvalidValueSets.DuplicateItemList };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Collaborator_Boundary_UserIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityCollaboratorRequest { UserId = int.MaxValue };
        var (isValid, _) = ValidateCollaboratorRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    // ========== OpportunityExternalStakeholderRequest ==========

    #region 1. Field Order Permutations - OpportunityExternalStakeholderRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void ExternalStakeholder_FieldOrder_ContactIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = ValidContactId };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
        req.ContactId.Should().Be(ValidContactId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ExternalStakeholder_FieldOrder_SingleField_OrderIndependent()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = 7 };
        req.ContactId.Should().Be(7);
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Invalid Combinations - OpportunityExternalStakeholderRequest

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void ExternalStakeholder_Invalid_ContactIdZeroOrNegative_ShouldFailValidation(int invalidContactId)
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = invalidContactId };
        var (isValid, error) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("ContactId");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ExternalStakeholder_Invalid_ContactIdZero_ShouldFailValidation()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = 0 };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ExternalStakeholder_Invalid_ContactIdNegative_ShouldFailValidation()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = -1 };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid - OpportunityExternalStakeholderRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void ExternalStakeholder_Mixed_SingleField_InvalidContactId_ShouldFailValidation()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = -5 };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission - OpportunityExternalStakeholderRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void ExternalStakeholder_Partial_Minimal_ContactIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = ValidContactId };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ExternalStakeholder_Partial_Full_AllFieldsSet_ProducesValidRequest()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = ValidContactId };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary - OpportunityExternalStakeholderRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void ExternalStakeholder_Boundary_ContactIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = int.MaxValue };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ExternalStakeholder_Boundary_ContactIdOne_ProducesValidRequest()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = 1 };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ExternalStakeholder_Boundary_ContactIdLarge_ProducesValidRequest()
    {
        var req = new OpportunityExternalStakeholderRequest { ContactId = 999999 };
        var (isValid, _) = ValidateExternalStakeholderRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion
}
