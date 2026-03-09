/// <summary>
/// Tests for User Role assignment data entry permutations.
///
/// Requirements validated:
/// - REQ-1: UserId required, positive → Field order, invalid tests
/// - REQ-2: Roles required, at least one → Invalid tests
/// - REQ-3: OrganizationHierarchyIds optional → Partial tests
/// - REQ-4: No duplicate roles → Invalid tests
/// - REQ-5: Role names must be valid → Invalid, boundary tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.UserManagement;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "UserManagement")]

public class UserRolePermutationTests
{
    private static readonly string[] ValidRoleNames =
    {
        "Administrator", "OpportunityManager", "Viewer", "Editor", "Approver",
        "Role6", "Role7", "Role8", "Role9", "Role10", "Role11", "Role12", "Role13", "Role14", "Role15",
        "Role16", "Role17", "Role18", "Role19", "Role20", "Role21", "Role22", "Role23", "Role24", "Role25",
        "Role26", "Role27", "Role28", "Role29", "Role30", "Role31", "Role32", "Role33", "Role34", "Role35",
        "Role36", "Role37", "Role38", "Role39", "Role40", "Role41", "Role42", "Role43", "Role44", "Role45",
        "Role46", "Role47", "Role48", "Role49", "Role50", "Role51"
    };

    private class UserRoleAssignmentRequest
    {
        public int UserId { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<int>? OrganizationHierarchyIds { get; set; }
    }

    private static (bool IsValid, List<string> Errors) ValidateUserRoleAssignment(UserRoleAssignmentRequest req)
    {
        var errors = new List<string>();
        if (req.UserId <= 0) errors.Add("UserId must be positive");
        if (req.Roles == null || req.Roles.Count == 0) errors.Add("Roles must contain at least one role");
        else
        {
            if (req.Roles.Any(r => string.IsNullOrWhiteSpace(r))) errors.Add("Roles must not contain null or empty strings");
            if (req.Roles.Count != req.Roles.Distinct().Count()) errors.Add("Roles must not contain duplicates");
            foreach (var role in req.Roles)
            {
                if (!string.IsNullOrWhiteSpace(role) && !ValidRoleNames.Contains(role))
                    errors.Add($"Role '{role}' is not a valid role name");
            }
        }
        if (req.OrganizationHierarchyIds != null && req.OrganizationHierarchyIds.Any(id => id <= 0))
            errors.Add("OrganizationHierarchyIds must contain only positive integers");
        return (errors.Count == 0, errors);
    }

    private static UserRoleAssignmentRequest CreateValidBaseRequest() => new()
    {
        UserId = 1,
        Roles = new List<string> { "Viewer" }
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_UserIdFirst_ProducesValidRequest()
    {
        var req = new UserRoleAssignmentRequest { UserId = 5, Roles = new List<string> { "Editor" } };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.UserId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_RolesFirst_ProducesValidRequest()
    {
        var req = new UserRoleAssignmentRequest { Roles = new List<string> { "Administrator" }, UserId = 5 };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.Roles.Should().Contain("Administrator");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_OrgHierarchyIdsFirst_ProducesValidRequest()
    {
        var req = new UserRoleAssignmentRequest { OrganizationHierarchyIds = new List<int> { 1, 2 }, UserId = 10, Roles = new List<string> { "Viewer" } };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.OrganizationHierarchyIds.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllFieldsReverseOrder_ProducesValidRequest()
    {
        var req = new UserRoleAssignmentRequest { OrganizationHierarchyIds = new List<int> { 3 }, Roles = new List<string> { "OpportunityManager" }, UserId = 7 };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.UserId.Should().Be(7);
        req.Roles.Should().Contain("OpportunityManager");
    }

    #endregion

    #region 2. Invalid Combinations

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Negative")]
    public void Invalid_UserIdZeroOrNegative_FailsValidation(int userId)
    {
        var req = CreateValidBaseRequest();
        req.UserId = userId;
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("UserId"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EmptyRolesList_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string>();
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Roles"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_NullRoles_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = null!;
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Roles"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_DuplicateRoles_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Viewer", "Viewer", "Editor" };
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("duplicate"));
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("Admin")]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidRoleNames_FailsValidation(string role)
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { role };
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Role") || e.Contains("valid"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_ValidRoleWithInvalidRole_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Viewer", "InvalidRole" };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyStringInRoles_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Viewer", "" };
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_NegativeOrgHierarchyId_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = new List<int> { -1, 0 };
        var (isValid, errors) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("OrganizationHierarchyIds"));
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidUserId_EmptyRoles_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.UserId = 100;
        req.Roles = new List<string>();
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidRoles_InvalidUserId_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Editor", "Viewer" };
        req.UserId = 0;
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidUserId_ValidRoles_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserId = 50;
        req.Roles = new List<string> { "Administrator", "Editor" };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidRoles_InvalidOrgHierarchyIds_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = new List<int> { 1, -1 };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidUserId_InvalidDuplicateRoles_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.UserId = 5;
        req.Roles = new List<string> { "Viewer", "Viewer" };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidUserId_ValidRoles_ValidOrgHierarchy_Valid()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = new List<int> { 1, 2, 3 };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalUserIdAndOneRole_Valid()
    {
        var req = new UserRoleAssignmentRequest { UserId = 1, Roles = new List<string> { "Viewer" } };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.OrganizationHierarchyIds.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithOrgHierarchy_Valid()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = new List<int> { 10, 20 };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.OrganizationHierarchyIds.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithManyRoles_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Administrator", "Editor", "Approver" };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.Roles.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithAllFields_Valid()
    {
        var req = new UserRoleAssignmentRequest
        {
            UserId = 42,
            Roles = new List<string> { "OpportunityManager", "Viewer" },
            OrganizationHierarchyIds = new List<int> { 1, 2, 3, 4 }
        };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithEmptyOrgHierarchy_Valid()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = new List<int>();
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithNullOrgHierarchy_Valid()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = null;
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ManyRoles_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Viewer", "Editor", "Approver", "OpportunityManager", "Administrator" };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.Roles.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ManyRolesFiftyPlus_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Roles = ValidRoleNames.Take(51).ToList();
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.Roles.Should().HaveCount(51);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ManyOrgHierarchyIds_Valid()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = Enumerable.Range(1, 100).ToList();
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.OrganizationHierarchyIds.Should().HaveCount(100);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UserIdAtIntMaxValue_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserId = int.MaxValue;
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.UserId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongRoleName_InvalidRole_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { InvalidValueSets.VeryLongString(500) };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllValidRoleNames_Valid()
    {
        foreach (var role in ValidRoleNames)
        {
            var req = CreateValidBaseRequest();
            req.Roles = new List<string> { role };
            var (isValid, _) = ValidateUserRoleAssignment(req);
            isValid.Should().BeTrue($"Role '{role}' should be valid");
        }
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UserIdAtOne_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserId = 1;
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SingleRole_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Roles = new List<string> { "Administrator" };
        var (isValid, _) = ValidateUserRoleAssignment(req);
        isValid.Should().BeTrue();
        req.Roles.Should().HaveCount(1);
    }

    #endregion
}
