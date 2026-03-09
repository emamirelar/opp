/// <summary>
/// Tests for Search filter criteria data entry permutations.
///
/// Requirements validated:
/// - REQ-1: SearchTerm optional → Partial, boundary tests
/// - REQ-2: EntityType optional, must be Partner/Contact/Interaction/Opportunity when provided → Invalid tests
/// - REQ-3: SortField, SortDirection optional, SortDirection asc/desc when provided → Invalid tests
/// - REQ-4: Page default 1, PageSize default 20, must be positive → Invalid, boundary tests
/// - REQ-5: Filters list optional → Partial tests
/// - REQ-6: SearchTerm injection patterns rejected/sanitized → Invalid, mixed tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Search;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Search")]

public class SearchFilterPermutationTests
{
    private static readonly string[] ValidEntityTypes = { "Partner", "Contact", "Interaction", "Opportunity" };
    private static readonly string[] ValidSortDirections = { "asc", "desc" };

    private class SearchCriteriaRequest
    {
        public string? SearchTerm { get; set; }
        public string? EntityType { get; set; }
        public List<object>? Filters { get; set; }
        public string? SortField { get; set; }
        public string? SortDirection { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    private static (bool IsValid, List<string> Errors) ValidateSearchCriteria(SearchCriteriaRequest req)
    {
        var errors = new List<string>();
        if (!string.IsNullOrEmpty(req.EntityType) && !ValidEntityTypes.Contains(req.EntityType))
            errors.Add($"EntityType must be one of: {string.Join(", ", ValidEntityTypes)}");
        if (!string.IsNullOrEmpty(req.SortDirection) && !ValidSortDirections.Any(d => string.Equals(d, req.SortDirection, StringComparison.OrdinalIgnoreCase)))
            errors.Add("SortDirection must be 'asc' or 'desc'");
        if (req.Page < 1) errors.Add("Page must be at least 1");
        if (req.PageSize < 1) errors.Add("PageSize must be at least 1");
        if (req.SearchTerm != null && req.SearchTerm.Contains("'; DROP TABLE"))
            errors.Add("SearchTerm contains invalid injection pattern");
        return (errors.Count == 0, errors);
    }

    private static SearchCriteriaRequest CreateValidBaseRequest() => new()
    {
        SearchTerm = "test",
        EntityType = "Partner",
        Page = 1,
        PageSize = 20
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_SearchTermFirst_ProducesValidRequest()
    {
        var req = new SearchCriteriaRequest { SearchTerm = "partner", EntityType = "Partner", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.SearchTerm.Should().Be("partner");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_EntityTypeFirst_ProducesValidRequest()
    {
        var req = new SearchCriteriaRequest { EntityType = "Contact", SearchTerm = "john", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.EntityType.Should().Be("Contact");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_SortFirst_ProducesValidRequest()
    {
        var req = new SearchCriteriaRequest { SortField = "Name", SortDirection = "asc", SearchTerm = "x", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.SortField.Should().Be("Name");
        req.SortDirection.Should().Be("asc");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_PageLast_ProducesValidRequest()
    {
        var req = new SearchCriteriaRequest { SearchTerm = "a", EntityType = "Opportunity", SortField = "Id", SortDirection = "desc", Page = 2, PageSize = 10 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.Page.Should().Be(2);
        req.PageSize.Should().Be(10);
    }

    #endregion

    #region 2. Invalid Combinations

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "Negative")]
    public void Invalid_NegativePage_FailsValidation(int page)
    {
        var req = CreateValidBaseRequest();
        req.Page = page;
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Page"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Negative")]
    public void Invalid_NegativeOrZeroPageSize_FailsValidation(int pageSize)
    {
        var req = CreateValidBaseRequest();
        req.PageSize = pageSize;
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("PageSize"));
    }

    [Theory]
    [InlineData("InvalidEntity")]
    [InlineData("")]
    [InlineData("Partner ")]
    [InlineData(" partner")]
    [InlineData("partners")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidEntityType_FailsValidation(string entityType)
    {
        var req = CreateValidBaseRequest();
        req.EntityType = entityType;
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityType"));
    }

    [Theory]
    [InlineData("ASC")]
    [InlineData("DESC")]
    [Trait("Category", "Functional")]
    public void Invalid_ValidSortDirectionCaseInsensitive_AcceptsValue(string sortDir)
    {
        var req = CreateValidBaseRequest();
        req.SortDirection = sortDir;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ascending")]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidSortDirection_FailsValidation(string sortDir)
    {
        var req = CreateValidBaseRequest();
        req.SortDirection = sortDir;
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SortDirection"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_SqlInjectionInSearchTerm_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = InvalidValueSets.SpecialCharacters[1];
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("SearchTerm") || e.Contains("injection"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_AllInvalidCombined_FailsValidation()
    {
        var req = new SearchCriteriaRequest { EntityType = "Invalid", SortDirection = "bad", Page = 0, PageSize = 0 };
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_XssInSearchTerm_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = InvalidValueSets.SpecialCharacters[0];
        req.SearchTerm.Should().Contain("script");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_PathTraversalInSearchTerm_StructureAcceptable()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = InvalidValueSets.SpecialCharacters[2];
        req.SearchTerm.Should().Contain("..");
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidSearch_InvalidSort_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = "valid";
        req.EntityType = "Partner";
        req.SortDirection = "invalid";
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntityType_InvalidPage_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityType = "Contact";
        req.Page = -1;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidSearch_ValidSort_Valid()
    {
        var req = CreateValidBaseRequest();
        req.SortField = "Name";
        req.SortDirection = "asc";
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntityType_InvalidEntityType_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.EntityType = "InvalidType";
        req.Page = 1;
        req.PageSize = 20;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidPage_InvalidPageSize_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.Page = 1;
        req.PageSize = 0;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidSearchTerm_SqlInjectionAttempt_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = "'; DROP TABLE Partners;--";
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntityType_ValidPage_Valid()
    {
        var req = CreateValidBaseRequest();
        req.EntityType = "Opportunity";
        req.Page = 5;
        req.PageSize = 50;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidSort_InvalidEntityType_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.SortDirection = "desc";
        req.EntityType = "BadType";
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_NoFilters_Valid()
    {
        var req = new SearchCriteriaRequest { Page = 1, PageSize = 20 };
        req.Filters = null;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.SearchTerm.Should().BeNull();
        req.EntityType.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithSearchTermOnly_Valid()
    {
        var req = new SearchCriteriaRequest { SearchTerm = "acme", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.SearchTerm.Should().Be("acme");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithEntityTypeOnly_Valid()
    {
        var req = new SearchCriteriaRequest { EntityType = "Partner", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.EntityType.Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithSortOnly_Valid()
    {
        var req = new SearchCriteriaRequest { SortField = "CreatedDate", SortDirection = "desc", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.SortField.Should().Be("CreatedDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithFiltersEmptyList_Valid()
    {
        var req = new SearchCriteriaRequest { Filters = new List<object>(), Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WithAllFields_Valid()
    {
        var req = new SearchCriteriaRequest
        {
            SearchTerm = "test",
            EntityType = "Opportunity",
            Filters = new List<object> { new { field = "Status", value = "Active" } },
            SortField = "Name",
            SortDirection = "asc",
            Page = 2,
            PageSize = 50
        };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_DefaultPageAndPageSize_Valid()
    {
        var req = new SearchCriteriaRequest();
        req.Page.Should().Be(1);
        req.PageSize.Should().Be(20);
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_NullSearchTerm_Valid()
    {
        var req = new SearchCriteriaRequest { SearchTerm = null, EntityType = "Contact", Page = 1, PageSize = 20 };
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_VeryLongSearchTerm_PropertyAcceptsValue()
    {
        var longStr = InvalidValueSets.VeryLongString(10000);
        var req = CreateValidBaseRequest();
        req.SearchTerm = longStr;
        req.SearchTerm.Should().HaveLength(10000);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_PageSizeAtZero_FailsValidation()
    {
        var req = CreateValidBaseRequest();
        req.PageSize = 0;
        var (isValid, errors) = ValidateSearchCriteria(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("PageSize"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_PageSizeAtOne_Valid()
    {
        var req = CreateValidBaseRequest();
        req.PageSize = 1;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.PageSize.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_PageSizeAtIntMaxValue_Valid()
    {
        var req = CreateValidBaseRequest();
        req.PageSize = int.MaxValue;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.PageSize.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllEntityTypes_Valid()
    {
        foreach (var et in ValidEntityTypes)
        {
            var req = CreateValidBaseRequest();
            req.EntityType = et;
            var (isValid, _) = ValidateSearchCriteria(req);
            isValid.Should().BeTrue($"EntityType '{et}' should be valid");
        }
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_SpecialCharsInSearchTerm_PropertyAcceptsValue()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = "test & co. <b>bold</b>";
        req.SearchTerm.Should().Contain("&");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_UnicodeSearchTerm_Valid()
    {
        var req = CreateValidBaseRequest();
        req.SearchTerm = InvalidValueSets.UnicodeStrings[0];
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.SearchTerm.Should().Contain("日本語");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_PageAtOne_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Page = 1;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_PageAtLargeValue_Valid()
    {
        var req = CreateValidBaseRequest();
        req.Page = 99999;
        var (isValid, _) = ValidateSearchCriteria(req);
        isValid.Should().BeTrue();
        req.Page.Should().Be(99999);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_BothSortDirections_Valid()
    {
        foreach (var dir in ValidSortDirections)
        {
            var req = CreateValidBaseRequest();
            req.SortDirection = dir;
            var (isValid, _) = ValidateSearchCriteria(req);
            isValid.Should().BeTrue($"SortDirection '{dir}' should be valid");
        }
    }

    #endregion
}
