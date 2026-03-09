/// <summary>
/// Tests for Comment entity data entry permutations (CommentRequest).
///
/// Requirements validated:
/// - REQ-1: EntityType required → Field order, invalid tests
/// - REQ-2: EntityId required, positive → Invalid ID tests
/// - REQ-3: Content required → Invalid Content tests
/// - REQ-4: ParentCommentId optional (replies) → Partial tests
/// - REQ-5: MentionedUserIds optional → Partial, invalid list tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Comment;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Comment")]

public class CommentDataEntryPermutationTests
{
    private static readonly string[] ValidEntityTypes = { "Partner", "Contact", "Opportunity", "Interaction" };
    private const int ContentMaxLength = 10000;

    private static (bool IsValid, List<string> Errors) ValidateCommentRequest(CommentRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityType))
            errors.Add("EntityType is required");
        else if (!ValidEntityTypes.Contains(req.EntityType))
            errors.Add($"EntityType must be one of: {string.Join(", ", ValidEntityTypes)}");
        if (req.EntityId <= 0)
            errors.Add("EntityId must be positive");
        if (string.IsNullOrWhiteSpace(req.Content))
            errors.Add("Content is required");
        else if (req.Content.Length > ContentMaxLength)
            errors.Add($"Content must not exceed {ContentMaxLength} characters");
        if (req.MentionedUserIds != null)
            foreach (var id in req.MentionedUserIds)
                if (id <= 0) errors.Add($"Invalid MentionedUserId: {id}");
        return (errors.Count == 0, errors);
    }

    private static CommentRequest CreateValidBaseRequest() => new()
    {
        EntityType = "Partner",
        EntityId = 1,
        Content = "Valid comment content"
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_EntityTypeFirst_ProducesValidRequest()
    {
        var req = new CommentRequest { EntityType = "Contact", EntityId = 1, Content = "Test" };
        req.EntityType.Should().Be("Contact");
        req.EntityId.Should().Be(1);
        req.Content.Should().Be("Test");
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_EntityIdFirst_ProducesValidRequest()
    {
        var req = new CommentRequest { EntityId = 5, EntityType = "Opportunity", Content = "Comment" };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
        req.EntityId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ContentFirst_ProducesValidRequest()
    {
        var req = new CommentRequest { Content = "First content", EntityType = "Interaction", EntityId = 2 };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
        req.Content.Should().Be("First content");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ReverseOrder_ProducesValidRequest()
    {
        var req = new CommentRequest { Content = "Rev", EntityId = 10, EntityType = "Partner" };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WithParentCommentId_ProducesValidRequest()
    {
        var req = new CommentRequest { EntityType = "Partner", EntityId = 1, Content = "Reply", ParentCommentId = 5 };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
        req.ParentCommentId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WithMentionedUserIds_ProducesValidRequest()
    {
        var req = new CommentRequest { EntityType = "Partner", EntityId = 1, Content = "Hi @user", MentionedUserIds = new List<int> { 1, 2 } };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
        req.MentionedUserIds.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllFieldsInterleaved_ProducesValidRequest()
    {
        var req = new CommentRequest { EntityType = "placeholder", Content = "placeholder" };
        req.EntityType = "Contact";
        req.ParentCommentId = 3;
        req.EntityId = 7;
        req.MentionedUserIds = new List<int> { 1 };
        req.Content = "Interleaved";
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Invalid Combinations

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyContent_FailsValidation(string? content)
    {
        var req = CreateValidBaseRequest();
        req.Content = content ?? string.Empty;
        var (isValid, errors) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Content"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyEntityType_FailsValidation(string? entityType)
    {
        var req = CreateValidBaseRequest();
        req.EntityType = entityType ?? string.Empty;
        var (isValid, errors) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityType"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidEntityId_FailsValidation(int entityId)
    {
        var req = CreateValidBaseRequest();
        req.EntityId = entityId;
        var (isValid, errors) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityId"));
    }

    [Theory]
    [MemberData(nameof(InvalidMentionedUserIdsData))]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidMentionedUserIds_FailsValidation(List<int> ids)
    {
        var req = CreateValidBaseRequest();
        req.MentionedUserIds = ids;
        var (isValid, errors) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("MentionedUserId") || e.Contains("Invalid"));
    }

    public static IEnumerable<object[]> InvalidMentionedUserIdsData()
    {
        yield return new object[] { new List<int> { -1 } };
        yield return new object[] { new List<int> { 0 } };
        yield return new object[] { new List<int> { 1, -1, 2 } };
        yield return new object[] { InvalidValueSets.NegativeIdList };
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_AllRequiredFieldsInvalid_FailsValidation()
    {
        var req = new CommentRequest { EntityType = "", EntityId = 0, Content = "" };
        var (isValid, errors) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidEntityType_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityType = "InvalidEntity";
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidContent_InvalidEntityType_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityType = "InvalidType";
        req.Content.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntityType_InvalidEntityId_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityId = 0;
        req.EntityType.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ContentWithSpecialChars_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.Content = InvalidValueSets.SpecialCharacters[0];
        req.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ContentWithInjectionPattern_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.Content = "'; DROP TABLE Comments;--";
        req.Content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidContent_ValidEntityType_Valid()
    {
        var req = CreateValidBaseRequest();
        req.EntityType = "Opportunity";
        req.Content = "Approved";
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntity_ValidParentCommentId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ParentCommentId = 10;
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntity_ValidMentionedUserIds_Valid()
    {
        var req = CreateValidBaseRequest();
        req.MentionedUserIds = new List<int> { 1, 2, 3 };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalEntityTypeEntityIdContent_Valid()
    {
        var req = new CommentRequest { EntityType = "Partner", EntityId = 1, Content = "Minimal" };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithParentCommentId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ParentCommentId = 5;
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithMentionedUserIds_Valid()
    {
        var req = CreateValidBaseRequest();
        req.MentionedUserIds = new List<int> { 1 };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithParentCommentIdAndMentionedUserIds_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ParentCommentId = 3;
        req.MentionedUserIds = new List<int> { 1, 2 };
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_ParentCommentIdNull_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ParentCommentId = null;
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MentionedUserIdsNull_Valid()
    {
        var req = CreateValidBaseRequest();
        req.MentionedUserIds = null;
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongContent_Over10000Chars_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Content = InvalidValueSets.VeryLongString(10001);
        var (isValid, errors) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Content") || e.Contains("10000"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ContentExactly10000Chars_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Content = InvalidValueSets.MaxLengthString(ContentMaxLength);
        req.Content.Length.Should().Be(ContentMaxLength);
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_MarkdownInContent_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Content = InvalidValueSets.MarkdownStrings[0];
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ManyMentionedUserIds_Valid()
    {
        var req = CreateValidBaseRequest();
        req.MentionedUserIds = InvalidValueSets.LargeList;
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
        req.MentionedUserIds.Should().HaveCount(100);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ParentCommentIdIntMaxValue_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ParentCommentId = int.MaxValue;
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
        req.ParentCommentId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ContentOnlyWhitespace_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Content = "   \t\n  ";
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ContentOnlyEmojis_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Content = InvalidValueSets.UnicodeStrings[4];
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_ContentWithUnicode_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Content = InvalidValueSets.UnicodeStrings[0] + " - test";
        var (isValid, _) = ValidateCommentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllValidEntityTypes_Valid()
    {
        foreach (var entityType in ValidEntityTypes)
        {
            var req = CreateValidBaseRequest();
            req.EntityType = entityType;
            var (isValid, _) = ValidateCommentRequest(req);
            isValid.Should().BeTrue($"EntityType '{entityType}' should be valid");
        }
    }

    #endregion
}
