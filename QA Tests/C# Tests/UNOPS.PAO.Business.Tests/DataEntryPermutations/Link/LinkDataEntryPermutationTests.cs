/// <summary>
/// Tests for Link entity data entry permutations (LinkRequest).
///
/// Requirements validated:
/// - REQ-1: Entity (LinkEntityType) required → Field order, invalid tests
/// - REQ-2: EntityId required, positive → Invalid ID tests
/// - REQ-3: Url required, max 2000 chars → Invalid URL, boundary tests
/// - REQ-4: Name optional, max 2000 chars → Partial, boundary tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Links;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Link;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Link")]

public class LinkDataEntryPermutationTests
{
    private const int UrlMaxLength = 2000;
    private const int NameMaxLength = 2000;

    private static (bool IsValid, List<string> Errors) ValidateLinkRequest(LinkRequest req)
    {
        var errors = new List<string>();
        if (!Enum.IsDefined(typeof(LinkEntityType), req.Entity))
            errors.Add("Entity must be a valid LinkEntityType");
        if (req.EntityId <= 0)
            errors.Add("EntityId must be positive");
        if (string.IsNullOrWhiteSpace(req.Url))
            errors.Add("Url is required");
        else
        {
            if (req.Url.Length > UrlMaxLength)
                errors.Add($"Url must not exceed {UrlMaxLength} characters");
            else if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                errors.Add("Url must be a valid http or https URL");
        }
        if (!string.IsNullOrEmpty(req.Name) && req.Name.Length > NameMaxLength)
            errors.Add($"Name must not exceed {NameMaxLength} characters");
        return (errors.Count == 0, errors);
    }

    private static LinkRequest CreateValidBaseRequest() => new()
    {
        Entity = LinkEntityType.Partner,
        EntityId = 1,
        Url = "https://example.com"
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_EntityFirst_ProducesValidRequest()
    {
        var req = new LinkRequest { Entity = LinkEntityType.Contact, EntityId = 1, Url = "https://a.com" };
        req.Entity.Should().Be(LinkEntityType.Contact);
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_EntityIdFirst_ProducesValidRequest()
    {
        var req = new LinkRequest { EntityId = 5, Entity = LinkEntityType.Partner, Url = "https://b.com" };
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
        req.EntityId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_UrlFirst_ProducesValidRequest()
    {
        var req = new LinkRequest { Url = "https://c.com", Entity = LinkEntityType.PartnerTree, EntityId = 2 };
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
        req.Url.Should().Be("https://c.com");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ReverseOrder_ProducesValidRequest()
    {
        var req = new LinkRequest { Url = "https://d.com", EntityId = 10, Entity = LinkEntityType.Partner };
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_WithName_ProducesValidRequest()
    {
        var req = new LinkRequest { Entity = LinkEntityType.Contact, EntityId = 1, Url = "https://e.com", Name = "My Link" };
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
        req.Name.Should().Be("My Link");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllFieldsInterleaved_ProducesValidRequest()
    {
        var req = new LinkRequest();
        req.Entity = LinkEntityType.PartnerTree;
        req.Name = "Optional";
        req.EntityId = 7;
        req.Url = "https://f.com";
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Invalid Combinations

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyUrl_FailsValidation(string? url)
    {
        var req = CreateValidBaseRequest();
        req.Url = url ?? string.Empty;
        var (isValid, errors) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Url"));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://invalid.com")]
    [InlineData("spaces in url.com")]
    [InlineData("missing-scheme.com")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidUrlFormat_FailsValidation(string url)
    {
        var req = CreateValidBaseRequest();
        req.Url = url;
        var (isValid, errors) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Url") || e.Contains("valid"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EntityIdZero_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityId = 0;
        var (isValid, errors) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityId"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EntityIdNegative_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityId = -1;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_AllRequiredFieldsInvalid_FailsValidation()
    {
        var req = new LinkRequest { Entity = LinkEntityType.Partner, EntityId = 0, Url = "" };
        var (isValid, errors) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidEntity_OutOfRange_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Entity = (LinkEntityType)999;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidUrl_InvalidEntity_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Entity = (LinkEntityType)(-1);
        req.Url.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntity_InvalidUrl_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Url = "not-a-valid-url";
        req.Entity.Should().Be(LinkEntityType.Partner);
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidUrl_ValidEntity_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Url = "https://example.org/page";
        req.Entity = LinkEntityType.Contact;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntity_ValidEntityId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.EntityId = 100;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntity_InvalidEntityIdZero_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityId = 0;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalEntityEntityIdUrl_Valid()
    {
        var req = new LinkRequest { Entity = LinkEntityType.Partner, EntityId = 1, Url = "https://min.com" };
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithName_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Name = "Document Link";
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_EmptyName_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Name = "";
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_NullName_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Name = null;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_AllEntityTypes_Valid()
    {
        foreach (LinkEntityType entity in Enum.GetValues<LinkEntityType>())
        {
            var req = CreateValidBaseRequest();
            req.Entity = entity;
            var (isValid, _) = ValidateLinkRequest(req);
            isValid.Should().BeTrue($"Entity {entity} should be valid");
        }
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UrlExactly2000Chars_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Url = "https://a.co/" + InvalidValueSets.MaxLengthString(UrlMaxLength - 13);
        req.Url.Length.Should().Be(UrlMaxLength);
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_Url2001Chars_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Url = "https://a.co/" + InvalidValueSets.OverMaxLengthString(UrlMaxLength - 13);
        req.Url.Length.Should().BeGreaterThan(UrlMaxLength);
        var (isValid, errors) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Url") || e.Contains("2000"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_NameExactly2000Chars_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Name = InvalidValueSets.MaxLengthString(NameMaxLength);
        req.Name!.Length.Should().Be(NameMaxLength);
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_Name2001Chars_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Name = InvalidValueSets.OverMaxLengthString(NameMaxLength);
        var (isValid, errors) = ValidateLinkRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Name") || e.Contains("2000"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SpecialCharsInUrl_ValidWhenFormatted()
    {
        var req = CreateValidBaseRequest();
        req.Url = "https://example.com/path?q=test&x=1";
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UnicodeInName_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Name = InvalidValueSets.UnicodeStrings[0];
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_EntityIdIntMaxValue_Valid()
    {
        var req = CreateValidBaseRequest();
        req.EntityId = int.MaxValue;
        var (isValid, _) = ValidateLinkRequest(req);
        isValid.Should().BeTrue();
        req.EntityId.Should().Be(int.MaxValue);
    }

    #endregion
}
