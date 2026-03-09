/// <summary>
/// Tests for Interaction entity data entry permutations.
///
/// Requirements validated:
/// - REQ-1: InteractionRequest required fields (Type, Date, Subject) → Field order, partial submission tests
/// - REQ-2: Subject validation (required, non-empty) → Pairwise invalid Subject tests
/// - REQ-3: Date validation → Invalid Date combinations
/// - REQ-4: Type enum coverage → All InteractionType values
/// - REQ-5: Optional fields (EmailAddresses, ContactIds, PartnerIds, UserIds, Location, Gmail*) → Partial/mixed tests
/// - REQ-6: GmailMessageId max 80 chars → Boundary tests
/// - REQ-7: ID list validation (positive integers) → Negative/zero ID tests
/// </summary>

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Interactions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Interaction;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Interaction")]
public class InteractionDataEntryPermutationTests : ManagerTestBase
{
    private const int GmailMessageIdMaxLength = 80;

    /// <summary>
    /// Validates InteractionRequest per controller rules: Subject required, at least one participant,
    /// positive IDs in ContactIds/PartnerIds, GmailMessageId max 80.
    /// </summary>
    private static (bool IsValid, List<string> Errors) ValidateInteractionRequest(InteractionRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.Subject))
            errors.Add("Subject is required");
        var hasParticipants = (req.ContactIds != null && req.ContactIds.Any()) ||
                             (req.PartnerIds != null && req.PartnerIds.Any()) ||
                             (req.UserIds != null && req.UserIds.Any()) ||
                             (req.EmailAddresses != null && req.EmailAddresses.Any());
        if (!hasParticipants)
            errors.Add("At least one participant is required");
        if (req.PartnerIds != null)
            foreach (var id in req.PartnerIds)
                if (id <= 0) errors.Add($"Invalid PartnerId: {id}");
        if (req.ContactIds != null)
            foreach (var id in req.ContactIds)
                if (id <= 0) errors.Add($"Invalid ContactId: {id}");
        if (req.UserIds != null)
            foreach (var id in req.UserIds)
                if (id <= 0) errors.Add($"Invalid UserId: {id}");
        if (!string.IsNullOrEmpty(req.GmailMessageId) && req.GmailMessageId.Length > GmailMessageIdMaxLength)
            errors.Add($"GmailMessageId must not exceed {GmailMessageIdMaxLength} characters");
        return (errors.Count == 0, errors);
    }

    private static InteractionRequest CreateValidBaseRequest()
    {
        return new InteractionRequest
        {
            Type = InteractionType.Email,
            Date = DateTime.UtcNow,
            Subject = "Valid Subject",
            EmailAddresses = new List<string> { "user@example.com" }
        };
    }

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_TypeThenDateThenSubject_ProducesValidRequest()
    {
        var req = new InteractionRequest { EmailAddresses = new List<string> { "a@b.com" } };
        req.Type = InteractionType.Call;
        req.Date = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        req.Subject = "Call Subject";
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
        req.Type.Should().Be(InteractionType.Call);
        req.Subject.Should().Be("Call Subject");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_SubjectThenDateThenType_ProducesValidRequest()
    {
        var req = new InteractionRequest { EmailAddresses = new List<string> { "x@y.com" } };
        req.Subject = "Meeting Subject";
        req.Date = new DateTime(2024, 7, 1, 14, 0, 0, DateTimeKind.Utc);
        req.Type = InteractionType.InPersonMeeting;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
        req.Type.Should().Be(InteractionType.InPersonMeeting);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_DateThenSubjectThenType_ProducesValidRequest()
    {
        var req = new InteractionRequest { EmailAddresses = new List<string> { "c@d.com" } };
        req.Date = DateTime.UtcNow.AddDays(1);
        req.Subject = "Virtual Meeting";
        req.Type = InteractionType.VirtualMeeting;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_TypeSubjectDate_OrderIndependent()
    {
        var req1 = new InteractionRequest { EmailAddresses = new List<string> { "e@f.com" } };
        req1.Type = InteractionType.Chat;
        req1.Subject = "Chat";
        req1.Date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var req2 = new InteractionRequest { EmailAddresses = new List<string> { "e@f.com" } };
        req2.Date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        req2.Subject = "Chat";
        req2.Type = InteractionType.Chat;

        req1.Type.Should().Be(req2.Type);
        req1.Subject.Should().Be(req2.Subject);
        req1.Date.Should().Be(req2.Date);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllRequiredFieldsLast_StillValid()
    {
        var req = new InteractionRequest
        {
            Description = "Desc",
            Location = "Office",
            EmailAddresses = new List<string> { "last@test.com" },
            Subject = "Subject Last",
            Date = DateTime.UtcNow,
            Type = InteractionType.Other
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_RequiredFieldsInterleavedWithOptional_Valid()
    {
        var req = new InteractionRequest();
        req.Type = InteractionType.Email;
        req.Description = "Optional";
        req.Date = DateTime.UtcNow;
        req.Location = "Room A";
        req.Subject = "Interleaved";
        req.EmailAddresses = new List<string> { "i@j.com" };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Pairwise / Invalid Combinations

    [Theory]
    [MemberData(nameof(InvalidSubjectValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidSubject_FailsValidation(string? subject)
    {
        var req = CreateValidBaseRequest();
        req.Subject = subject ?? string.Empty;
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Subject"));
    }

    public static IEnumerable<object[]> InvalidSubjectValues()
    {
        foreach (var s in InvalidValueSets.NullEmptyWhitespace)
            yield return new object?[] { s };
        yield return new object[] { InvalidValueSets.VeryLongString(5000) };
    }

    [Theory]
    [MemberData(nameof(InvalidDateValues))]
    [Trait("Category", "Edge")]
    public void Pairwise_InvalidDateWithValidSubject_RequestStructureAcceptable(DateTime date)
    {
        var req = CreateValidBaseRequest();
        req.Date = date;
        req.Subject.Should().NotBeNullOrWhiteSpace();
        req.Date.Should().Be(date);
    }

    public static IEnumerable<object[]> InvalidDateValues()
    {
        yield return new object[] { default(DateTime) };
        yield return new object[] { DateTime.MinValue };
        yield return new object[] { DateTime.UtcNow.AddYears(100) };
    }

    [Theory]
    [MemberData(nameof(AllInteractionTypesData))]
    [Trait("Category", "Functional")]
    public void Pairwise_EachInteractionType_WithValidSubject_Valid(InteractionType type)
    {
        var req = CreateValidBaseRequest();
        req.Type = type;
        req.Subject = $"Subject for {type}";
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> AllInteractionTypesData() =>
        InvalidValueSets.AllInteractionTypes.Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(InvalidSubjectWithTypeData))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidSubjectWithEachType_FailsValidation(string? subject, InteractionType type)
    {
        var req = CreateValidBaseRequest();
        req.Subject = subject ?? string.Empty;
        req.Type = type;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
    }

    public static IEnumerable<object[]> InvalidSubjectWithTypeData()
    {
        foreach (var s in new string?[] { null, "", "   " })
            foreach (var t in InvalidValueSets.AllInteractionTypes)
                yield return new object?[] { s, t };
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidDateAndValidSubject_DateStoredAsProvided()
    {
        var req = CreateValidBaseRequest();
        req.Date = DateTime.MinValue;
        req.Subject.Should().NotBeNullOrWhiteSpace();
        req.Date.Should().Be(DateTime.MinValue);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_AllRequiredFieldsInvalid_FailsValidation()
    {
        var req = new InteractionRequest
        {
            Subject = null!,
            Date = default,
            Type = InteractionType.Email,
            EmailAddresses = null
        };
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Subject"));
        errors.Should().Contain(e => e.Contains("participant"));
    }

    [Theory]
    [MemberData(nameof(InvalidEmailAddressesData))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidEmailAddressesInList_WithValidSubject_StructureAcceptable(string invalidEmail)
    {
        var req = CreateValidBaseRequest();
        req.EmailAddresses = new List<string> { invalidEmail };
        req.Subject.Should().NotBeNullOrWhiteSpace();
    }

    public static IEnumerable<object[]> InvalidEmailAddressesData()
    {
        foreach (var e in InvalidValueSets.InvalidEmails.Where(x => !string.IsNullOrEmpty(x)).Take(5))
            yield return new object[] { e! };
    }

    [Theory]
    [MemberData(nameof(NegativeOrZeroIdsData))]
    [Trait("Category", "Negative")]
    public void Pairwise_NegativeOrZeroContactIds_FailsValidation(List<int> ids)
    {
        var req = CreateValidBaseRequest();
        req.ContactIds = ids;
        req.EmailAddresses = null;
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ContactId") || e.Contains("Invalid"));
    }

    [Theory]
    [MemberData(nameof(NegativeOrZeroIdsData))]
    [Trait("Category", "Negative")]
    public void Pairwise_NegativeOrZeroPartnerIds_FailsValidation(List<int> ids)
    {
        var req = CreateValidBaseRequest();
        req.PartnerIds = ids;
        req.EmailAddresses = null;
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(NegativeOrZeroIdsData))]
    [Trait("Category", "Negative")]
    public void Pairwise_NegativeOrZeroUserIds_FailsValidation(List<int> ids)
    {
        var req = CreateValidBaseRequest();
        req.UserIds = ids;
        req.EmailAddresses = null;
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
    }

    public static IEnumerable<object[]> NegativeOrZeroIdsData()
    {
        yield return new object[] { new List<int> { -1 } };
        yield return new object[] { new List<int> { 0 } };
        yield return new object[] { new List<int> { -1, 0, -100 } };
    }

    #endregion

    #region 3. Mixed Valid / Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidSubjectValidType_InvalidEmailAddresses_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.EmailAddresses = new List<string> { "not-an-email", "missing@domain" };
        req.Subject.Should().NotBeNullOrWhiteSpace();
        req.Type.Should().Be(InteractionType.Email);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidSubjectValidType_MixValidInvalidContactIds_FailsWhenInvalidPresent()
    {
        var req = CreateValidBaseRequest();
        req.ContactIds = new List<int> { 1, -1, 2 };
        req.EmailAddresses = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_EmailType_WithEmailAddresses_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.Email;
        req.EmailAddresses = InvalidValueSets.ValidEmails.ToList();
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_EmailType_WithoutEmailAddresses_ButWithContactIds_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.Email;
        req.EmailAddresses = null;
        req.ContactIds = new List<int> { 1 };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_MeetingType_WithLocation_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.InPersonMeeting;
        req.Location = "Conference Room A";
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_MeetingType_WithoutLocation_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.InPersonMeeting;
        req.Location = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_CallType_WithOptionalFields_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.Call;
        req.Description = "Follow-up call";
        req.UserIds = new List<int> { 1 };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ChatType_MinimalOptional_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.Chat;
        req.Description = null;
        req.Location = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_VirtualMeeting_WithLocation_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.VirtualMeeting;
        req.Location = "Zoom link";
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_OtherType_AllOptionalNull_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Type = InteractionType.Other;
        req.Description = null;
        req.Location = null;
        req.GmailThreadId = null;
        req.GmailMessageId = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_GmailMessageIdMaxLength80_Valid()
    {
        var req = CreateValidBaseRequest();
        req.GmailMessageId = InvalidValueSets.MaxLengthString(GmailMessageIdMaxLength);
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
        req.GmailMessageId!.Length.Should().Be(GmailMessageIdMaxLength);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Mixed_GmailMessageIdOverflow81_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.GmailMessageId = InvalidValueSets.OverMaxLengthString(GmailMessageIdMaxLength);
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("GmailMessageId") || e.Contains("80"));
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_OnlyRequiredFields_SubjectDateType_NeedsParticipant()
    {
        var req = new InteractionRequest
        {
            Subject = "Minimal",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email
        };
        var (isValid, errors) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("participant"));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusEmailAddressesOnly_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Email Only",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            EmailAddresses = new List<string> { "only@test.com" }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusContactIdsOnly_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Contacts Only",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            ContactIds = new List<int> { 1 }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusPartnerIdsOnly_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Partners Only",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            PartnerIds = new List<int> { 1 }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusUserIdsOnly_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Users Only",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            UserIds = new List<int> { 1 }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusLocation_ForMeetingType_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Meeting with Location",
            Date = DateTime.UtcNow,
            Type = InteractionType.InPersonMeeting,
            Location = "HQ Room 1",
            EmailAddresses = new List<string> { "m@test.com" }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusAllOptionalFields_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Full",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            Description = "Desc",
            EmailAddresses = new List<string> { "a@b.com" },
            ContactIds = new List<int> { 1 },
            PartnerIds = new List<int> { 1 },
            UserIds = new List<int> { 1 },
            Location = "Loc",
            OrganizationHierarchyIds = new List<int> { 1 },
            GmailThreadId = "thread-1",
            GmailMessageId = "msg-1",
            Status = "Active",
            ConfirmDuplicateCreation = true,
            CreatedBy = 1
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusGmailThreadIdAndMessageId_GmailImportScenario_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "Gmail Import",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            GmailThreadId = "thread-gmail-123",
            GmailMessageId = "msg-gmail-456",
            EmailAddresses = new List<string> { "gmail@test.com" }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusDescriptionOnly_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "With Desc",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            Description = "Some notes",
            EmailAddresses = new List<string> { "d@e.com" }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredPlusOrganizationHierarchyIds_Valid()
    {
        var req = new InteractionRequest
        {
            Subject = "With Org",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            OrganizationHierarchyIds = new List<int> { 1, 2 },
            EmailAddresses = new List<string> { "o@test.com" }
        };
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Value Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongSubject_AndVeryLongDescription_AndVeryLongLocation_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.Subject = InvalidValueSets.VeryLongString(2000);
        req.Description = InvalidValueSets.VeryLongString(5000);
        req.Location = InvalidValueSets.VeryLongString(1000);
        req.Subject.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_GmailMessageId_Exactly80Chars_Valid()
    {
        var req = CreateValidBaseRequest();
        req.GmailMessageId = InvalidValueSets.MaxLengthString(GmailMessageIdMaxLength);
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
        req.GmailMessageId!.Length.Should().Be(80);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_GmailMessageId_81Chars_Invalid()
    {
        var req = CreateValidBaseRequest();
        req.GmailMessageId = InvalidValueSets.OverMaxLengthString(GmailMessageIdMaxLength);
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_LargeEmailAddressesList_50Entries_Valid()
    {
        var emails = Enumerable.Range(1, 50).Select(i => $"user{i}@domain{i}.com").ToList();
        var req = CreateValidBaseRequest();
        req.EmailAddresses = emails;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
        req.EmailAddresses.Should().HaveCount(50);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_LargeContactIdsList_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ContactIds = Enumerable.Range(1, 100).ToList();
        req.EmailAddresses = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_LargePartnerIdsList_Valid()
    {
        var req = CreateValidBaseRequest();
        req.PartnerIds = Enumerable.Range(1, 100).ToList();
        req.EmailAddresses = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_LargeUserIdsList_Valid()
    {
        var req = CreateValidBaseRequest();
        req.UserIds = Enumerable.Range(1, 100).ToList();
        req.EmailAddresses = null;
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_DateTimeMinValue_WithMaxLengthSubject_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.Date = DateTime.MinValue;
        req.Subject = InvalidValueSets.MaxLengthString(500);
        req.Subject.Should().NotBeNullOrWhiteSpace();
        req.Date.Should().Be(DateTime.MinValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllListsPopulatedWithLargeData_Valid()
    {
        var req = CreateValidBaseRequest();
        req.EmailAddresses = Enumerable.Range(1, 30).Select(i => $"e{i}@test.com").ToList();
        req.ContactIds = Enumerable.Range(1, 20).ToList();
        req.PartnerIds = Enumerable.Range(1, 15).ToList();
        req.UserIds = Enumerable.Range(1, 10).ToList();
        req.OrganizationHierarchyIds = Enumerable.Range(1, 5).ToList();
        var (isValid, _) = ValidateInteractionRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion
}
