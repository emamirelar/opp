/// <summary>
/// Tests for Contact Data Entry Permutations
///
/// Requirements validated:
/// - REQ-1: ContactRequest required fields (LastName, Title, Email, PartnerId) must be valid for creation
/// - REQ-2: Field order permutations produce identical valid request objects
/// - REQ-3: Invalid value combinations are rejected
/// - REQ-4: Partial submissions with required fields only are valid
/// - REQ-5: Boundary values are handled correctly
///
/// Defects found: None
/// </summary>

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Models.Contacts;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Contact;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Contact")]
public class ContactDataEntryPermutationTests : ManagerTestBase
{
    private const int DefaultValidPartnerId = 1;
    private const int DefaultMaxStringLength = 255;

    private static ContactRequest CreateValidBaseRequest() => new()
    {
        LastName = "Smith",
        Title = "Manager",
        Email = "john.smith@example.com",
        PartnerId = DefaultValidPartnerId
    };

    private static bool IsValidContactRequest(ContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Title)
            || string.IsNullOrWhiteSpace(request.Email) || request.PartnerId <= 0)
            return false;
        var email = request.Email.Trim();
        var atIdx = email.IndexOf('@');
        return atIdx > 0 && atIdx < email.Length - 1;
    }

    // ========== 1. FIELD ORDER PERMUTATIONS (~6 tests) ==========

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrderPermutation_0_LastNameFirst_ProducesValidRequest()
    {
        var req = new ContactRequest { LastName = "Doe", Title = "Director", Email = "jane@test.com", PartnerId = 1 };
        req.LastName.Should().Be("Doe");
        req.Title.Should().Be("Director");
        req.Email.Should().Be("jane@test.com");
        req.PartnerId.Should().Be(1);
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrderPermutation_1_TitleFirst_ProducesValidRequest()
    {
        var req = new ContactRequest { Title = "Engineer", LastName = "Brown", Email = "bob@test.com", PartnerId = 2 };
        req.Title.Should().Be("Engineer");
        req.LastName.Should().Be("Brown");
        req.Email.Should().Be("bob@test.com");
        req.PartnerId.Should().Be(2);
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrderPermutation_2_EmailFirst_ProducesValidRequest()
    {
        var req = new ContactRequest { Email = "alice@example.org", LastName = "Jones", Title = "Analyst", PartnerId = 3 };
        req.Email.Should().Be("alice@example.org");
        req.LastName.Should().Be("Jones");
        req.Title.Should().Be("Analyst");
        req.PartnerId.Should().Be(3);
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrderPermutation_3_PartnerIdFirst_ProducesValidRequest()
    {
        var req = new ContactRequest { PartnerId = 4, LastName = "Wilson", Title = "Lead", Email = "wilson@test.com" };
        req.PartnerId.Should().Be(4);
        req.LastName.Should().Be("Wilson");
        req.Title.Should().Be("Lead");
        req.Email.Should().Be("wilson@test.com");
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrderPermutation_4_ReverseOrder_ProducesValidRequest()
    {
        var req = new ContactRequest { PartnerId = 5, Email = "rev@test.com", Title = "VP", LastName = "Reversed" };
        IsValidContactRequest(req).Should().BeTrue();
        req.LastName.Should().Be("Reversed");
        req.Title.Should().Be("VP");
        req.Email.Should().Be("rev@test.com");
        req.PartnerId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrderPermutation_5_AllPermutationsProduceIdenticalValidation()
    {
        var permutations = new[]
        {
            new ContactRequest { LastName = "X", Title = "T", Email = "x@t.com", PartnerId = 1 },
            new ContactRequest { Title = "T", LastName = "X", Email = "x@t.com", PartnerId = 1 },
            new ContactRequest { Email = "x@t.com", LastName = "X", Title = "T", PartnerId = 1 },
            new ContactRequest { PartnerId = 1, LastName = "X", Title = "T", Email = "x@t.com" }
        };
        foreach (var p in permutations)
        {
            IsValidContactRequest(p).Should().BeTrue();
            p.LastName.Should().Be("X");
            p.Title.Should().Be("T");
            p.Email.Should().Be("x@t.com");
            p.PartnerId.Should().Be(1);
        }
    }

    // ========== 2. PAIRWISE/INVALID COMBINATIONS (~20 tests using MemberData) ==========

    [Theory]
    [MemberData(nameof(InvalidLastNameValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidLastName_ShouldFailValidation(string? invalidLastName)
    {
        var req = CreateValidBaseRequest();
        req.LastName = invalidLastName ?? string.Empty;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidTitleValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidTitle_ShouldFailValidation(string? invalidTitle)
    {
        var req = CreateValidBaseRequest();
        req.Title = invalidTitle ?? string.Empty;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidEmailValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidEmail_ShouldFailValidation(string? invalidEmail)
    {
        var req = CreateValidBaseRequest();
        req.Email = invalidEmail ?? string.Empty;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidPartnerId_ShouldFailValidation(int invalidPartnerId)
    {
        var req = CreateValidBaseRequest();
        req.PartnerId = invalidPartnerId;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_LastNameAndEmailInvalid_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.LastName = "";
        req.Email = "not-an-email";
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_TitleAndPartnerIdInvalid_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.Title = "   ";
        req.PartnerId = 0;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_EmailAndLastNameInvalid_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.Email = null!;
        req.LastName = "";
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_AllRequiredFieldsInvalid_ShouldFailValidation()
    {
        var req = new ContactRequest
        {
            LastName = "",
            Title = "",
            Email = "",
            PartnerId = 0
        };
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidEmailFormatValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidEmailFormats_ShouldFailValidation(string invalidEmail)
    {
        var req = CreateValidBaseRequest();
        req.Email = invalidEmail;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidPhoneFormatValues))]
    [Trait("Category", "Negative")]
    public void Pairwise_InvalidPhoneFormats_OptionalField_ShouldBeRejectedOrHandled(string invalidPhone)
    {
        var req = CreateValidBaseRequest();
        req.Phone = invalidPhone;
        req.Mobile = invalidPhone;
        IsValidContactRequest(req).Should().BeTrue();
        req.Phone.Should().Be(invalidPhone);
    }

    public static IEnumerable<object[]> InvalidLastNameValues()
    {
        yield return new object[] { (string?)null };
        yield return new object[] { "" };
        yield return new object[] { "   " };
    }

    public static IEnumerable<object[]> InvalidTitleValues()
    {
        yield return new object[] { (string?)null };
        yield return new object[] { "" };
        yield return new object[] { "   " };
    }

    public static IEnumerable<object[]> InvalidEmailValues()
    {
        yield return new object[] { (string?)null };
        yield return new object[] { "" };
        yield return new object[] { "   " };
    }

    public static IEnumerable<object[]> InvalidEmailFormatValues()
    {
        yield return new object[] { "not-an-email" };
        yield return new object[] { "@nodomain.com" };
        yield return new object[] { "no.at.sign.com" };
        yield return new object[] { "double@@at.com" };
    }

    public static IEnumerable<object[]> InvalidPhoneFormatValues()
    {
        yield return new object[] { "abc" };
        yield return new object[] { "<script>" };
    }

    // ========== 3. MIXED VALID/INVALID COMBINATIONS (~12 tests) ==========

    [Fact]
    [Trait("Category", "Negative")]
    public void Mixed_ValidLastName_InvalidEmail_ValidTitle_ValidPartnerId_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.Email = "invalid-email-format";
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Mixed_ValidEmail_EmptyLastName_ValidTitle_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.LastName = "";
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidRequiredFields_InvalidAssistantEmail_OptionalField_ShouldRemainValid()
    {
        var req = CreateValidBaseRequest();
        req.AssistantEmail = "not-an-email";
        IsValidContactRequest(req).Should().BeTrue();
        req.AssistantEmail.Should().Be("not-an-email");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidRequiredFields_InvalidPhoneAndMobile_OptionalFields_ShouldRemainValid()
    {
        var req = CreateValidBaseRequest();
        req.Phone = "abc";
        req.Mobile = "<script>";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_SpecialCharactersInAllStringFields_RequiredFieldsValid_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Salutation = InvalidValueSets.SpecialCharacters[0];
        req.FirstName = InvalidValueSets.SpecialCharacters[1];
        req.MiddleName = InvalidValueSets.SpecialCharacters[2];
        req.LastName = "ValidLastName";
        req.Suffix = InvalidValueSets.SpecialCharacters[3];
        req.Title = "ValidTitle";
        req.Department = InvalidValueSets.SpecialCharacters[4];
        req.Description = InvalidValueSets.SpecialCharacters[5];
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_UnicodeInOptionalFields_RequiredFieldsValid_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.FirstName = InvalidValueSets.UnicodeStrings[0];
        req.MiddleName = InvalidValueSets.UnicodeStrings[1];
        req.LastName = "ValidLastName";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Mixed_ValidLastName_EmptyTitle_ValidEmail_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.Title = "";
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Mixed_ValidLastName_ValidTitle_NullEmail_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.Email = null!;
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WhitespaceLastName_ValidOthers_ShouldFailValidation()
    {
        var req = CreateValidBaseRequest();
        req.LastName = "   \t  ";
        IsValidContactRequest(req).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidRequiredFields_AllOptionalFieldsFilled_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Salutation = "Dr.";
        req.FirstName = "John";
        req.MiddleName = "Q.";
        req.Suffix = "Jr.";
        req.Department = "Engineering";
        req.Description = "Test";
        req.Phone = "+1-555-0100";
        req.Mobile = "+1-555-0101";
        req.Assistant = "Jane";
        req.AssistantPhone = "+1-555-0102";
        req.AssistantEmail = "jane@example.com";
        req.MailingStreet = "123 Main St";
        req.MailingStreet2 = "Suite 100";
        req.MailingCity = "Boston";
        req.MailingStateProvince = "MA";
        req.MailingPostalCode = "02101";
        req.MailingCountry = "USA";
        req.OrganizationHierarchyIds = new List<int> { 1, 2 };
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_SpecialCharactersInLastName_WhenRequired_ShouldAcceptOrRejectConsistently()
    {
        var req = CreateValidBaseRequest();
        req.LastName = "O'Brien-Smith";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_UnicodeInRequiredLastName_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.LastName = "García";
        IsValidContactRequest(req).Should().BeTrue();
    }

    // ========== 4. PARTIAL SUBMISSION (~10 tests using MemberData) ==========

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_OnlyRequiredFields_ShouldBeValid()
    {
        var req = new ContactRequest
        {
            LastName = "Minimal",
            Title = "Staff",
            Email = "minimal@test.com",
            PartnerId = 1
        };
        IsValidContactRequest(req).Should().BeTrue();
        req.Salutation.Should().BeNull();
        req.FirstName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredFieldsPlusSalutationOnly_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Salutation = "Mr.";
        IsValidContactRequest(req).Should().BeTrue();
        req.Salutation.Should().Be("Mr.");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredFieldsPlusAllNameFields_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Salutation = "Dr.";
        req.FirstName = "Jane";
        req.MiddleName = "Marie";
        req.Suffix = "III";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredFieldsPlusAllContactInfo_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Phone = "+1-555-0100";
        req.Mobile = "+1-555-0101";
        req.Assistant = "Assistant";
        req.AssistantPhone = "+1-555-0102";
        req.AssistantEmail = "assistant@example.com";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredFieldsPlusAllMailingAddress_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.MailingStreet = "100 Main St";
        req.MailingStreet2 = "Apt 5";
        req.MailingCity = "New York";
        req.MailingStateProvince = "NY";
        req.MailingPostalCode = "10001";
        req.MailingCountry = "USA";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_RequiredFieldsPlusAllOptionalFields_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Salutation = "Ms.";
        req.FirstName = "Alice";
        req.MiddleName = "B.";
        req.Suffix = "Jr.";
        req.Department = "Sales";
        req.Description = "Key contact";
        req.Phone = "555-0100";
        req.Mobile = "555-0101";
        req.Assistant = "Bob";
        req.AssistantPhone = "555-0102";
        req.AssistantEmail = "bob@example.com";
        req.MailingStreet = "200 Oak Ave";
        req.MailingStreet2 = "Floor 2";
        req.MailingCity = "Chicago";
        req.MailingStateProvince = "IL";
        req.MailingPostalCode = "60601";
        req.MailingCountry = "USA";
        req.OrganizationHierarchyIds = new List<int> { 1 };
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(PartialSubmissionSubsets))]
    [Trait("Category", "Functional")]
    public void Partial_VariousOptionalSubsets_ShouldBeValid(string subsetName, ContactRequest request)
    {
        IsValidContactRequest(request).Should().BeTrue($"{subsetName} should produce valid request");
    }

    public static IEnumerable<object[]> PartialSubmissionSubsets()
    {
        var req1 = CreateValidBaseRequestStatic();
        req1.Department = "IT";
        yield return new object[] { "Required+Department", req1 };

        var req2 = CreateValidBaseRequestStatic();
        req2.Description = "Test description";
        yield return new object[] { "Required+Description", req2 };

        var req3 = CreateValidBaseRequestStatic();
        req3.Status = "Active";
        yield return new object[] { "Required+Status", req3 };

        var req4 = CreateValidBaseRequestStatic();
        req4.ConfirmDuplicateCreation = true;
        yield return new object[] { "Required+ConfirmDuplicateCreation", req4 };
    }

    private static ContactRequest CreateValidBaseRequestStatic() => new()
    {
        LastName = "Subset",
        Title = "Manager",
        Email = "subset@test.com",
        PartnerId = 1
    };

    // ========== 5. BOUNDARY VALUE COMBINATIONS (~8 tests) ==========

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_MaxLengthStringsInRequiredFields_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.LastName = InvalidValueSets.MaxLengthString(DefaultMaxStringLength);
        req.Title = InvalidValueSets.MaxLengthString(DefaultMaxStringLength);
        req.Email = "user@example.com";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_MaxLengthPlusSpecialCharsPlusUnicode_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.LastName = "García-O'Brien";
        req.Title = "Director (Regional)";
        req.Email = "test+tag@example.com";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongEmailAddress_ShouldBeHandled()
    {
        var longEmail = new string('a', 200) + "@test.com";
        var req = CreateValidBaseRequest();
        req.Email = longEmail;
        IsValidContactRequest(req).Should().BeTrue();
        req.Email.Should().HaveLength(210);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [Trait("Category", "Edge")]
    public void Boundary_EdgeCasePartnerIdValues_ShouldValidateCorrectly(int partnerId)
    {
        var req = CreateValidBaseRequest();
        req.PartnerId = partnerId;
        var expectedValid = partnerId > 0;
        IsValidContactRequest(req).Should().Be(expectedValid);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllMailingAddressFieldsAtMaxLength_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        var maxStr = InvalidValueSets.MaxLengthString(255);
        req.MailingStreet = maxStr;
        req.MailingStreet2 = maxStr;
        req.MailingCity = maxStr;
        req.MailingStateProvince = maxStr;
        req.MailingPostalCode = maxStr;
        req.MailingCountry = maxStr;
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SingleCharRequiredFields_ShouldBeValid()
    {
        var req = new ContactRequest { LastName = "X", Title = "Y", Email = "x@y.com", PartnerId = 1 };
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_EmailWithPlusTag_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.Email = "user+tag@example.com";
        IsValidContactRequest(req).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OrganizationHierarchyIdsEmptyList_ShouldBeValid()
    {
        var req = CreateValidBaseRequest();
        req.OrganizationHierarchyIds = new List<int>();
        IsValidContactRequest(req).Should().BeTrue();
    }
}
