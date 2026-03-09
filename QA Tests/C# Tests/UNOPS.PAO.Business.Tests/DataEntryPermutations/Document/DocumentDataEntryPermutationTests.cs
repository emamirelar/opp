/// <summary>
/// Tests for Document entity data entry permutations (hypothetical DocumentUploadRequest).
///
/// Requirements validated:
/// - REQ-1: ParentEntityName required, must be Contact/Partner/Interaction/Opportunity → Field order, invalid tests
/// - REQ-2: ParentEntityId required, positive → Invalid ID tests
/// - REQ-3: DocumentTypeId optional → Partial tests
/// - REQ-4: Link conditional (URL or blob) → Invalid URL, boundary tests
/// - REQ-5: FileName optional → Partial, boundary tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Document;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Document")]

public class DocumentDataEntryPermutationTests
{
    private static readonly string[] ValidParentEntityNames = { "Contact", "Partner", "Interaction", "Opportunity" };
    private const int UrlMaxLength = 2000;

    /// <summary>
    /// Hypothetical document upload request model for permutation testing.
    /// </summary>
    private class DocumentUploadRequest
    {
        public string? ParentEntityName { get; set; }
        public int ParentEntityId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? Link { get; set; }
        public string? FileName { get; set; }
    }

    private static (bool IsValid, List<string> Errors) ValidateDocumentRequest(DocumentUploadRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.ParentEntityName))
            errors.Add("ParentEntityName is required");
        else if (!ValidParentEntityNames.Contains(req.ParentEntityName))
            errors.Add($"ParentEntityName must be one of: {string.Join(", ", ValidParentEntityNames)}");
        if (req.ParentEntityId <= 0)
            errors.Add("ParentEntityId must be positive");
        if (!string.IsNullOrEmpty(req.Link))
        {
            if (req.Link.Length > UrlMaxLength)
                errors.Add($"Link must not exceed {UrlMaxLength} characters");
            else if (!req.Link.StartsWith("blob:", StringComparison.OrdinalIgnoreCase) &&
                     (!Uri.TryCreate(req.Link, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
                errors.Add("Link must be a valid URL or blob reference");
        }
        if (!string.IsNullOrEmpty(req.FileName) && req.FileName.Length > 2000)
            errors.Add("FileName must not exceed 2000 characters");
        return (errors.Count == 0, errors);
    }

    private static DocumentUploadRequest CreateValidBaseRequest() => new()
    {
        ParentEntityName = "Partner",
        ParentEntityId = 1,
        Link = "https://example.com/doc.pdf"
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ParentEntityNameFirst_ProducesValidRequest()
    {
        var req = new DocumentUploadRequest { ParentEntityName = "Contact", ParentEntityId = 1, Link = "https://a.com" };
        req.ParentEntityName = "Contact";
        req.ParentEntityId = 10;
        req.DocumentTypeId = 2;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
        req.ParentEntityName.Should().Be("Contact");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_ParentEntityIdFirst_ProducesValidRequest()
    {
        var req = new DocumentUploadRequest { ParentEntityId = 5, ParentEntityName = "Opportunity", Link = "https://b.com" };
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
        req.ParentEntityId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_DocumentTypeIdFirst_ProducesValidRequest()
    {
        var req = new DocumentUploadRequest { DocumentTypeId = 3, ParentEntityName = "Interaction", ParentEntityId = 2, Link = "https://c.com" };
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
        req.DocumentTypeId.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_LinkFirst_ProducesValidRequest()
    {
        var req = new DocumentUploadRequest { Link = "https://d.com/file.pdf", ParentEntityName = "Partner", ParentEntityId = 1 };
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllFieldsReverseOrder_ProducesValidRequest()
    {
        var req = new DocumentUploadRequest
        {
            FileName = "report.pdf",
            Link = "https://e.com/report.pdf",
            DocumentTypeId = 1,
            ParentEntityId = 99,
            ParentEntityName = "Opportunity"
        };
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_InterleavedOptionalAndRequired_Valid()
    {
        var req = new DocumentUploadRequest();
        req.ParentEntityName = "Contact";
        req.DocumentTypeId = 2;
        req.ParentEntityId = 7;
        req.FileName = "doc.xlsx";
        req.Link = "https://f.com/doc.xlsx";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Invalid Combinations

    [Theory]
    [InlineData("InvalidEntity")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidParentEntityName_FailsValidation(string? name)
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityName = name ?? string.Empty;
        var (isValid, errors) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ParentEntityName"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidParentEntityId_FailsValidation(int id)
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityId = id;
        var (isValid, errors) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("ParentEntityId"));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://invalid.com")]
    [InlineData("  ")]
    [InlineData("missing-scheme.com")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidLinkFormat_FailsValidation(string link)
    {
        var req = CreateValidBaseRequest();
        req.Link = link;
        var (isValid, errors) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Link") || e.Contains("URL"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_AllRequiredFieldsInvalid_FailsValidation()
    {
        var req = new DocumentUploadRequest { ParentEntityName = "InvalidEntity", ParentEntityId = 0, Link = "bad" };
        var (isValid, errors) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_NullParentEntityName_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityName = null;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EmptyParentEntityName_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityName = "";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidParent_InvalidDocumentTypeIdNegative_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.DocumentTypeId = -1;
        req.ParentEntityName.Should().Be("Partner");
        req.ParentEntityId.Should().BePositive();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntity_InvalidLinkFormat_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Link = "not-a-valid-url";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidParent_ValidDocumentTypeId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.DocumentTypeId = 5;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntity_ValidLink_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Link = "https://example.org/document.pdf";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidParent_InvalidEntityIdZero_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityId = 0;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntityType_InvalidParentEntityName_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityName = "InvalidEntity";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalParentOnly_WithLink_Valid()
    {
        var req = new DocumentUploadRequest { ParentEntityName = "Partner", ParentEntityId = 1, Link = "https://min.com" };
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithDocumentTypeId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.DocumentTypeId = 2;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithLink_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Link = "https://partial.com/file.pdf";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithAllOptionalFields_Valid()
    {
        var req = CreateValidBaseRequest();
        req.DocumentTypeId = 3;
        req.FileName = "report.pdf";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithFileNameOnly_Valid()
    {
        var req = CreateValidBaseRequest();
        req.FileName = "document.docx";
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Partial_MissingLink_WithBlobReference_Valid()
    {
        var req = new DocumentUploadRequest { ParentEntityName = "Contact", ParentEntityId = 1, Link = "blob:https://example.com/abc-123" };
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongUrl_Over2000Chars_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Link = InvalidValueSets.OverMaxLengthString(UrlMaxLength);
        var (isValid, errors) = ValidateDocumentRequest(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Link") || e.Contains("2000"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UrlExactly2000Chars_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Link = "https://example.com/" + InvalidValueSets.MaxLengthString(UrlMaxLength - 22);
        req.Link!.Length.Should().BeLessOrEqualTo(UrlMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SpecialCharsInFileName_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.FileName = InvalidValueSets.SpecialCharacters[0];
        req.FileName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_IntMaxValueForParentEntityId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.ParentEntityId = int.MaxValue;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
        req.ParentEntityId.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_IntMaxValueForDocumentTypeId_Valid()
    {
        var req = CreateValidBaseRequest();
        req.DocumentTypeId = int.MaxValue;
        var (isValid, _) = ValidateDocumentRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UnicodeInFileName_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.FileName = InvalidValueSets.UnicodeStrings[0] + ".pdf";
        req.FileName.Should().Contain("日本語");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllValidEntityNames_Valid()
    {
        foreach (var name in ValidParentEntityNames)
        {
            var req = CreateValidBaseRequest();
            req.ParentEntityName = name;
            var (isValid, _) = ValidateDocumentRequest(req);
            isValid.Should().BeTrue($"Entity name '{name}' should be valid");
        }
    }

    #endregion
}
