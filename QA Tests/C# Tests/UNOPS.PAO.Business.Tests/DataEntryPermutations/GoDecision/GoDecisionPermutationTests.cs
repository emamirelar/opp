/// <summary>
/// Tests for Go Decision workflow request model data entry permutations.
///
/// Requirements validated:
/// - REQ-1: WorkflowSubmitRequest (EntityName, EntityId, NewStage required; optional Comment, booleans, AdditionalRemarks) → Field order, pairwise, partial, boundary
/// - REQ-2: ApproveWorkflowRequest (EntityName, EntityId, Rationale required; ConfirmationAcknowledged, ExecutiveId) → All test types
/// - REQ-3: RejectWorkflowRequest (EntityName, EntityId, Rationale required; ConfirmationAcknowledged) → All test types
/// - REQ-4: WorkflowRecallRequest (EntityName, EntityId required; optional Comment) → All test types
/// - REQ-5: WorkflowCancelRequest (EntityName, EntityId, Comment required) → All test types
/// - REQ-6: WorkflowReopenRequest (EntityName, EntityId required; optional Comment) → All test types
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Models.Workflow;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.GoDecision;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "GoDecision")]
public class GoDecisionPermutationTests
{
    private const int VeryLongLength = 10000;

    #region Validation Helpers

    private static (bool IsValid, List<string> Errors) ValidateWorkflowSubmitRequest(WorkflowSubmitRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        if (req.EntityId <= 0) errors.Add("EntityId must be positive");
        if (string.IsNullOrWhiteSpace(req.NewStage)) errors.Add("NewStage is required");
        return (errors.Count == 0, errors);
    }

    private static (bool IsValid, List<string> Errors) ValidateApproveWorkflowRequest(ApproveWorkflowRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        if (req.EntityId <= 0) errors.Add("EntityId must be positive");
        if (string.IsNullOrWhiteSpace(req.Rationale)) errors.Add("Rationale is required");
        if (!req.ConfirmationAcknowledged) errors.Add("ConfirmationAcknowledged must be true");
        if (req.ExecutiveId <= 0) errors.Add("ExecutiveId must be positive for Opportunity");
        return (errors.Count == 0, errors);
    }

    private static (bool IsValid, List<string> Errors) ValidateRejectWorkflowRequest(RejectWorkflowRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        if (req.EntityId <= 0) errors.Add("EntityId must be positive");
        if (string.IsNullOrWhiteSpace(req.Rationale)) errors.Add("Rationale is required");
        if (!req.ConfirmationAcknowledged) errors.Add("ConfirmationAcknowledged must be true");
        return (errors.Count == 0, errors);
    }

    private static (bool IsValid, List<string> Errors) ValidateWorkflowRecallRequest(WorkflowRecallRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        if (req.EntityId <= 0) errors.Add("EntityId must be positive");
        return (errors.Count == 0, errors);
    }

    private static (bool IsValid, List<string> Errors) ValidateWorkflowCancelRequest(WorkflowCancelRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        if (req.EntityId <= 0) errors.Add("EntityId must be positive");
        if (string.IsNullOrWhiteSpace(req.Comment)) errors.Add("Comment is required");
        return (errors.Count == 0, errors);
    }

    private static (bool IsValid, List<string> Errors) ValidateWorkflowReopenRequest(WorkflowReopenRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        if (req.EntityId <= 0) errors.Add("EntityId must be positive");
        return (errors.Count == 0, errors);
    }

    #endregion

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowSubmitRequest_EntityNameFirst_ProducesValidRequest()
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 1, NewStage = "GO" };
        var (isValid, _) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeTrue();
        req.EntityName.Should().Be("opportunity");
        req.EntityId.Should().Be(1);
        req.NewStage.Should().Be("GO");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowSubmitRequest_EntityIdFirst_ProducesValidRequest()
    {
        var req = new WorkflowSubmitRequest { EntityId = 42, EntityName = "opportunity", NewStage = "GO" };
        var (isValid, _) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeTrue();
        req.EntityId.Should().Be(42);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowSubmitRequest_NewStageLast_ProducesValidRequest()
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 1, NewStage = "IDENTIFY_AND_PROFILE" };
        var (isValid, _) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeTrue();
        req.NewStage.Should().Be("IDENTIFY_AND_PROFILE");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowSubmitRequest_OptionalFieldsInterleaved_PropertiesMatch()
    {
        var req = new WorkflowSubmitRequest
        {
            Comment = "C",
            EntityName = "opportunity",
            ConfirmedNonOMSubmission = true,
            EntityId = 1,
            NewStage = "GO",
            ConfirmedOrgUnitWarning = false,
            AcknowledgedStatement = true,
            AdditionalRemarks = "Remarks"
        };
        req.Comment.Should().Be("C");
        req.ConfirmedNonOMSubmission.Should().BeTrue();
        req.AcknowledgedStatement.Should().BeTrue();
        req.AdditionalRemarks.Should().Be("Remarks");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ApproveWorkflowRequest_FieldsSetInDifferentOrders_PropertiesMatch()
    {
        var req1 = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Approved", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        var req2 = new ApproveWorkflowRequest { ExecutiveId = 10, Rationale = "Approved", EntityId = 1, ConfirmationAcknowledged = true, EntityName = "opportunity" };
        req1.EntityName.Should().Be(req2.EntityName);
        req1.EntityId.Should().Be(req2.EntityId);
        req1.Rationale.Should().Be(req2.Rationale);
        req1.ExecutiveId.Should().Be(req2.ExecutiveId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_RejectWorkflowRequest_FieldsSetInDifferentOrders_PropertiesMatch()
    {
        var req1 = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Rejected", ConfirmationAcknowledged = true };
        var req2 = new RejectWorkflowRequest { Rationale = "Rejected", EntityId = 1, EntityName = "opportunity", ConfirmationAcknowledged = true };
        req1.EntityName.Should().Be(req2.EntityName);
        req1.Rationale.Should().Be(req2.Rationale);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowRecallRequest_FieldsSetInDifferentOrders_PropertiesMatch()
    {
        var req1 = new WorkflowRecallRequest { EntityName = "opportunity", EntityId = 1, Comment = "Recall" };
        var req2 = new WorkflowRecallRequest { Comment = "Recall", EntityId = 1, EntityName = "opportunity" };
        req1.EntityName.Should().Be(req2.EntityName);
        req1.Comment.Should().Be(req2.Comment);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowCancelRequest_FieldsSetInDifferentOrders_PropertiesMatch()
    {
        var req1 = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 1, Comment = "Cancelling" };
        var req2 = new WorkflowCancelRequest { Comment = "Cancelling", EntityId = 1, EntityName = "opportunity" };
        req1.EntityName.Should().Be(req2.EntityName);
        req1.Comment.Should().Be(req2.Comment);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WorkflowReopenRequest_FieldsSetInDifferentOrders_PropertiesMatch()
    {
        var req1 = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 1, Comment = "Reopening" };
        var req2 = new WorkflowReopenRequest { Comment = "Reopening", EntityId = 1, EntityName = "opportunity" };
        req1.EntityName.Should().Be(req2.EntityName);
        req1.Comment.Should().Be(req2.Comment);
    }

    #endregion

    #region 2. Pairwise / Invalid Combinations

    public static IEnumerable<object[]> InvalidEntityNameValues()
    {
        yield return new object[] { null! };
        yield return new object[] { "" };
        yield return new object[] { "   " };
        yield return new object[] { "InvalidEntity" };
        yield return new object[] { InvalidValueSets.VeryLongString(VeryLongLength) };
    }

    public static IEnumerable<object[]> InvalidEntityIdValues()
    {
        yield return new object[] { 0 };
        yield return new object[] { -1 };
        yield return new object[] { int.MaxValue };
    }

    public static IEnumerable<object[]> InvalidNewStageValues()
    {
        yield return new object[] { null! };
        yield return new object[] { "" };
        yield return new object[] { "INVALID_STAGE" };
        yield return new object[] { InvalidValueSets.VeryLongString(VeryLongLength) };
    }

    public static IEnumerable<object[]> InvalidRationaleValues()
    {
        yield return new object[] { null! };
        yield return new object[] { "" };
        yield return new object[] { "   " };
        yield return new object[] { InvalidValueSets.VeryLongString(VeryLongLength) };
    }

    public static IEnumerable<object[]> InvalidCommentValues()
    {
        yield return new object[] { null! };
        yield return new object[] { "" };
        yield return new object[] { "   " };
        yield return new object[] { InvalidValueSets.VeryLongString(VeryLongLength) };
    }

    public static IEnumerable<object[]> InvalidExecutiveIdValues()
    {
        yield return new object[] { 0 };
        yield return new object[] { -1 };
    }

    [Theory]
    [MemberData(nameof(InvalidEntityNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowSubmitRequest_InvalidEntityName_FailsValidation(string? entityName)
    {
        var req = new WorkflowSubmitRequest { EntityName = entityName ?? "", EntityId = 1, NewStage = "GO" };
        var (isValid, errors) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName"));
    }

    [Theory]
    [MemberData(nameof(InvalidEntityIdValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowSubmitRequest_InvalidEntityId_FailsValidation(int entityId)
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = entityId, NewStage = "GO" };
        var (isValid, errors) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityId"));
    }

    [Theory]
    [MemberData(nameof(InvalidNewStageValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowSubmitRequest_InvalidNewStage_FailsValidation(string? newStage)
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 1, NewStage = newStage ?? "" };
        var (isValid, errors) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("NewStage"));
    }

    [Theory]
    [MemberData(nameof(InvalidEntityNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_ApproveWorkflowRequest_InvalidEntityName_FailsValidation(string? entityName)
    {
        var req = new ApproveWorkflowRequest { EntityName = entityName ?? "", EntityId = 1, Rationale = "OK", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        var (isValid, errors) = ValidateApproveWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName"));
    }

    [Theory]
    [MemberData(nameof(InvalidRationaleValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_ApproveWorkflowRequest_InvalidRationale_FailsValidation(string? rationale)
    {
        var req = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = rationale ?? "", ConfirmationAcknowledged = true, ExecutiveId = 10 };
        var (isValid, errors) = ValidateApproveWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Rationale"));
    }

    [Theory]
    [MemberData(nameof(InvalidExecutiveIdValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_ApproveWorkflowRequest_InvalidExecutiveId_FailsValidation(int executiveId)
    {
        var req = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "OK", ConfirmationAcknowledged = true, ExecutiveId = executiveId };
        var (isValid, errors) = ValidateApproveWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ExecutiveId"));
    }

    [Theory]
    [MemberData(nameof(InvalidEntityNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_RejectWorkflowRequest_InvalidEntityName_FailsValidation(string? entityName)
    {
        var req = new RejectWorkflowRequest { EntityName = entityName ?? "", EntityId = 1, Rationale = "No", ConfirmationAcknowledged = true };
        var (isValid, errors) = ValidateRejectWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName"));
    }

    [Theory]
    [MemberData(nameof(InvalidRationaleValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_RejectWorkflowRequest_InvalidRationale_FailsValidation(string? rationale)
    {
        var req = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = rationale ?? "", ConfirmationAcknowledged = true };
        var (isValid, errors) = ValidateRejectWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Rationale"));
    }

    [Theory]
    [MemberData(nameof(InvalidEntityNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowRecallRequest_InvalidEntityName_FailsValidation(string? entityName)
    {
        var req = new WorkflowRecallRequest { EntityName = entityName ?? "", EntityId = 1 };
        var (isValid, errors) = ValidateWorkflowRecallRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName"));
    }

    [Theory]
    [MemberData(nameof(InvalidEntityNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowCancelRequest_InvalidEntityName_FailsValidation(string? entityName)
    {
        var req = new WorkflowCancelRequest { EntityName = entityName ?? "", EntityId = 1, Comment = "Cancel" };
        var (isValid, errors) = ValidateWorkflowCancelRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName"));
    }

    [Theory]
    [MemberData(nameof(InvalidCommentValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowCancelRequest_InvalidComment_FailsValidation(string? comment)
    {
        var req = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 1, Comment = comment ?? "" };
        var (isValid, errors) = ValidateWorkflowCancelRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Comment"));
    }

    [Theory]
    [MemberData(nameof(InvalidEntityNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_WorkflowReopenRequest_InvalidEntityName_FailsValidation(string? entityName)
    {
        var req = new WorkflowReopenRequest { EntityName = entityName ?? "", EntityId = 1 };
        var (isValid, errors) = ValidateWorkflowReopenRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName"));
    }

    [Theory]
    [MemberData(nameof(SubmitBooleanPermutations))]
    [Trait("Category", "Functional")]
    public void Pairwise_WorkflowSubmitRequest_BooleanFlags_PropertiesReflectValues(bool confirmedNonOM, bool confirmedOrgUnit, bool acknowledged)
    {
        var req = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            ConfirmedNonOMSubmission = confirmedNonOM,
            ConfirmedOrgUnitWarning = confirmedOrgUnit,
            AcknowledgedStatement = acknowledged
        };
        req.ConfirmedNonOMSubmission.Should().Be(confirmedNonOM);
        req.ConfirmedOrgUnitWarning.Should().Be(confirmedOrgUnit);
        req.AcknowledgedStatement.Should().Be(acknowledged);
    }

    public static IEnumerable<object[]> SubmitBooleanPermutations()
    {
        foreach (var perm in InvalidValueSets.BooleanPermutations(3))
            yield return new object[] { perm[0], perm[1], perm[2] };
    }

    #endregion

    #region 3. Mixed Valid / Invalid

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WorkflowSubmitRequest_ValidEntityNameInvalidEntityIdInvalidNewStage_FailsValidation()
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 0, NewStage = "" };
        var (isValid, errors) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeFalse();
        errors.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ApproveWorkflowRequest_ValidRationaleConfirmationAcknowledgedFalse_FailsValidation()
    {
        var req = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Valid rationale", ConfirmationAcknowledged = false, ExecutiveId = 10 };
        var (isValid, errors) = ValidateApproveWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ConfirmationAcknowledged"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ApproveWorkflowRequest_ValidRequestExecutiveIdZero_FailsValidation()
    {
        var req = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "OK", ConfirmationAcknowledged = true, ExecutiveId = 0 };
        var (isValid, errors) = ValidateApproveWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ExecutiveId"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WorkflowCancelRequest_ValidEntityNameValidEntityIdEmptyComment_FailsValidation()
    {
        var req = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 1, Comment = "" };
        var (isValid, errors) = ValidateWorkflowCancelRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Comment"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_RejectWorkflowRequest_ValidRationaleConfirmationAcknowledgedFalse_FailsValidation()
    {
        var req = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "Valid", ConfirmationAcknowledged = false };
        var (isValid, errors) = ValidateRejectWorkflowRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ConfirmationAcknowledged"));
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WorkflowSubmitRequest_MinimalRequiredFieldsOnly_Valid()
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = 1, NewStage = "GO" };
        var (isValid, _) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeTrue();
        req.Comment.Should().BeNull();
        req.AdditionalRemarks.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WorkflowSubmitRequest_AllOptionalFieldsPopulated_Valid()
    {
        var req = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            Comment = "Comment",
            ConfirmedNonOMSubmission = true,
            ConfirmedOrgUnitWarning = true,
            AcknowledgedStatement = true,
            AdditionalRemarks = "Remarks"
        };
        var (isValid, _) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeTrue();
        req.Comment.Should().Be("Comment");
        req.AdditionalRemarks.Should().Be("Remarks");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_ApproveWorkflowRequest_WithoutExecutiveId_FailsValidation()
    {
        var req = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = "OK", ConfirmationAcknowledged = true, ExecutiveId = 0 };
        var (isValid, _) = ValidateApproveWorkflowRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Partial_WorkflowCancelRequest_WithoutComment_FailsValidation()
    {
        var req = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 1, Comment = "" };
        var (isValid, errors) = ValidateWorkflowCancelRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Comment"));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WorkflowRecallRequest_MinimalRequiredOnly_Valid()
    {
        var req = new WorkflowRecallRequest { EntityName = "opportunity", EntityId = 1 };
        var (isValid, _) = ValidateWorkflowRecallRequest(req);
        isValid.Should().BeTrue();
        req.Comment.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WorkflowReopenRequest_WithOptionalComment_Valid()
    {
        var req = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 1, Comment = "Reopening from NO GO" };
        var (isValid, _) = ValidateWorkflowReopenRequest(req);
        isValid.Should().BeTrue();
        req.Comment.Should().Be("Reopening from NO GO");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WorkflowReopenRequest_WithoutComment_Valid()
    {
        var req = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 1 };
        var (isValid, _) = ValidateWorkflowReopenRequest(req);
        isValid.Should().BeTrue();
        req.Comment.Should().BeNull();
    }

    #endregion

    #region 5. Boundary Values

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowSubmitRequest_VeryLongEntityName_FailsValidation()
    {
        var longName = InvalidValueSets.VeryLongString(VeryLongLength);
        var req = new WorkflowSubmitRequest { EntityName = longName, EntityId = 1, NewStage = "GO" };
        var (isValid, errors) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeFalse();
        req.EntityName.Should().HaveLength(VeryLongLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowSubmitRequest_EntityIdMaxValue_FailsValidation()
    {
        var req = new WorkflowSubmitRequest { EntityName = "opportunity", EntityId = int.MaxValue, NewStage = "GO" };
        var (isValid, errors) = ValidateWorkflowSubmitRequest(req);
        isValid.Should().BeFalse();
        req.EntityId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowSubmitRequest_VeryLongRationaleCommentAdditionalRemarks_PropertiesAcceptValues()
    {
        var longStr = InvalidValueSets.VeryLongString(VeryLongLength);
        var req = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            Comment = longStr,
            AdditionalRemarks = longStr
        };
        req.Comment.Should().HaveLength(VeryLongLength);
        req.AdditionalRemarks.Should().HaveLength(VeryLongLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ApproveWorkflowRequest_VeryLongRationale_PropertyAcceptsValue()
    {
        var longRationale = InvalidValueSets.VeryLongString(VeryLongLength);
        var req = new ApproveWorkflowRequest { EntityName = "opportunity", EntityId = 1, Rationale = longRationale, ConfirmationAcknowledged = true, ExecutiveId = 10 };
        req.Rationale.Should().HaveLength(VeryLongLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowCancelRequest_VeryLongComment_PropertyAcceptsValue()
    {
        var longComment = InvalidValueSets.VeryLongString(VeryLongLength);
        var req = new WorkflowCancelRequest { EntityName = "opportunity", EntityId = 1, Comment = longComment };
        req.Comment.Should().HaveLength(VeryLongLength);
        var (isValid, _) = ValidateWorkflowCancelRequest(req);
        isValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(SubmitBooleanPermutations))]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowSubmitRequest_AllBooleanCombinations_PropertiesReflectValues(bool a, bool b, bool c)
    {
        var req = new WorkflowSubmitRequest
        {
            EntityName = "opportunity",
            EntityId = 1,
            NewStage = "GO",
            ConfirmedNonOMSubmission = a,
            ConfirmedOrgUnitWarning = b,
            AcknowledgedStatement = c
        };
        req.ConfirmedNonOMSubmission.Should().Be(a);
        req.ConfirmedOrgUnitWarning.Should().Be(b);
        req.AcknowledgedStatement.Should().Be(c);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_RejectWorkflowRequest_EntityIdMaxValue_FailsValidation()
    {
        var req = new RejectWorkflowRequest { EntityName = "opportunity", EntityId = int.MaxValue, Rationale = "No", ConfirmationAcknowledged = true };
        var (isValid, _) = ValidateRejectWorkflowRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowRecallRequest_EntityIdOne_Valid()
    {
        var req = new WorkflowRecallRequest { EntityName = "opportunity", EntityId = 1 };
        var (isValid, _) = ValidateWorkflowRecallRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WorkflowReopenRequest_OptionalCommentNull_Valid()
    {
        var req = new WorkflowReopenRequest { EntityName = "opportunity", EntityId = 1, Comment = null };
        var (isValid, _) = ValidateWorkflowReopenRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion
}
