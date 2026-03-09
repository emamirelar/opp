/// <summary>
/// Comprehensive unit tests for AdvancedSearchService.
/// Tests pagination, filters, entity-specific search, global search, empty query handling,
/// filter validation, ordering, and textBoost parameter.
/// Requirements source: Production code in AdvancedSearchService.cs
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDomain.Entities;
using SearchFilter = UNOPS.PAO.UNOPSBusiness.Services.SearchFilter;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services.AdvancedSearchServiceTests;

[Trait("Category", "Unit")]
[Trait("Feature", "AdvancedSearchService")]
public class AdvancedSearchServiceUnitTests : AdvancedSearchServiceFixture
{
    #region 1. Pagination (PaginationResponse)

    [Fact]
    public async Task SearchAsync_Partners_PaginationReturnsCorrectPageSizeAndOffset()
    {
        SeedPartners(5);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 2,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(2);
        result.PageSize.Should().Be(2);
        result.PageIndex.Should().Be(1);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_Partners_SecondPageReturnsCorrectOffset()
    {
        SeedPartners(5);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 2,
            PageSize = 2,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(2);
        result.PageIndex.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task SearchAsync_Partners_TotalPagesCalculation()
    {
        SeedPartners(5);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 3,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.TotalPages.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task SearchWithFiltersAsync_PaginationRequestApplied()
    {
        SeedPartners(4);
        var pagination = new PaginationRequest { PageIndex = 2, PageSize = 1, OrderBy = "Id", Ascending = true };
        var filters = new List<SearchFilter>();

        var result = await Service.SearchWithFiltersAsync<UNOPSPartner, PartnerModel>(filters, pagination, TestUser);

        result.Records.Should().HaveCount(1);
        result.PageIndex.Should().Be(2);
        result.PageSize.Should().Be(1);
    }

    #endregion

    #region 2. Empty Search Text Handling

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAllMatchingRecords()
    {
        SeedPartners(3);
        var request = new UnifiedSearchRequest
        {
            Query = "",
            Filters = null,
            PageIndex = 1,
            PageSize = 2,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_NullQuery_ReturnsAllMatchingRecords()
    {
        SeedPartners(3);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_WhitespaceQuery_NoTextSearchApplied()
    {
        SeedPartners(2);
        var request = new UnifiedSearchRequest
        {
            Query = "   ",
            Filters = null,
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(2);
    }

    #endregion

    #region 3. Dynamic Filter Application (FilterActive)

    [Fact]
    public async Task SearchAsync_FilterActiveFalse_GlobalFiltersSkipped()
    {
        SeedPartners(2);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 10,
            FilterActive = false
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_FilterActiveTrue_AppliesGlobalFilters()
    {
        SeedPartners(2);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 10,
            FilterActive = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().NotBeNull();
    }

    #endregion

    #region 4. Entity-Specific Search (SearchAsync, SearchWithFiltersAsync)

    [Fact]
    public async Task SearchAsync_Partners_ReturnsPartnerModels()
    {
        SeedPartners(2);
        var request = new UnifiedSearchRequest { Query = null, Filters = null, PageIndex = 1, PageSize = 10 };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().AllBeOfType<PartnerModel>();
        result.Records.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_Contacts_ReturnsContactModels()
    {
        SeedContacts(2);
        var request = new UnifiedSearchRequest { Query = null, Filters = null, PageIndex = 1, PageSize = 10 };

        var result = await Service.SearchAsync<UNOPSContact, ContactModel>(request, TestUser);

        result.Records.Should().AllBeOfType<ContactModel>();
        result.Records.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_Interactions_ReturnsInteractionModels()
    {
        SeedInteractions(2);
        var request = new UnifiedSearchRequest { Query = null, Filters = null, PageIndex = 1, PageSize = 10 };

        var result = await Service.SearchAsync<UNOPSInteraction, InteractionModel>(request, TestUser);

        result.Records.Should().AllBeOfType<InteractionModel>();
        result.Records.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchWithFiltersAsync_Partners_ReturnsPaginatedResults()
    {
        SeedPartners(3);
        var pagination = new PaginationRequest { PageIndex = 1, PageSize = 10 };
        var filters = new List<SearchFilter>();

        var result = await Service.SearchWithFiltersAsync<UNOPSPartner, PartnerModel>(filters, pagination, TestUser);

        result.Records.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    #endregion

    #region 5. Search Result Ordering

    [Fact]
    public async Task SearchAsync_OrderByAscending_ReturnsRecordsInAscendingOrder()
    {
        SeedPartners(3);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Id",
            Ascending = true
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        var ids = result.Records.Select(r => r.Id).ToList();
        ids.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchAsync_OrderByDescending_ReturnsRecordsInDescendingOrder()
    {
        SeedPartners(3);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = null,
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "Id",
            Ascending = false
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        var ids = result.Records.Select(r => r.Id).ToList();
        ids.Should().BeInDescendingOrder();
    }

    #endregion

    #region 6. Global Search Aggregation (SearchAllEntitiesAsync, SearchAllEntitiesModularAsync)

    [Fact]
    public async Task SearchAllEntitiesAsync_ReturnsValidStructure()
    {
        var result = await Service.SearchAllEntitiesAsync("test", 15);

        result.Should().NotBeNull();
        result.Partners.Should().NotBeNull();
        result.Contacts.Should().NotBeNull();
        result.Interactions.Should().NotBeNull();
        result.Opportunities.Should().NotBeNull();
        result.SearchQuery.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAllEntitiesModularAsync_ReturnsValidStructure()
    {
        var result = await Service.SearchAllEntitiesModularAsync("test", 1.0f, 15);

        result.Should().NotBeNull();
        result.Partners.Should().NotBeNull();
        result.Contacts.Should().NotBeNull();
        result.Interactions.Should().NotBeNull();
        result.Opportunities.Should().NotBeNull();
        result.SearchQuery.Should().Be("test");
    }

    [Fact]
    public async Task SearchAllEntitiesModularAsync_RespectsMaxResultsPerEntity()
    {
        var result = await Service.SearchAllEntitiesModularAsync("test", 1.0f, 5);

        result.Partners.Should().HaveCountLessThanOrEqualTo(5);
        result.Contacts.Should().HaveCountLessThanOrEqualTo(5);
        result.Interactions.Should().HaveCountLessThanOrEqualTo(5);
        result.Opportunities.Should().HaveCountLessThanOrEqualTo(5);
    }

    #endregion

    #region 7. Entity-Specific PostgreSQL Search (SearchPartnersAsync, etc.) — SQLite returns empty

    [Fact]
    public async Task SearchPartnersAsync_ReturnsList()
    {
        var result = await Service.SearchPartnersAsync("test");
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<List<GlobalSearchResult>>();
    }

    [Fact]
    public async Task SearchPartnersAsync_AcceptsTextBoostParameter()
    {
        var result = await Service.SearchPartnersAsync("test", textBoost: 2.0f);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchContactsAsync_ReturnsList()
    {
        var result = await Service.SearchContactsAsync("test");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchInteractionsAsync_ReturnsList()
    {
        var result = await Service.SearchInteractionsAsync("test");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchOpportunitiesAsync_ReturnsList()
    {
        var result = await Service.SearchOpportunitiesAsync("test");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchOpportunitiesAsync_AcceptsTextBoostAndSnippetLength()
    {
        var result = await Service.SearchOpportunitiesAsync("test", textBoost: 1.5f, snippetLength: 100);
        result.Should().NotBeNull();
    }

    #endregion

    #region 8. SearchWithQueryAndMetadataAsync — Unsupported Entity Type

    [Fact]
    public async Task SearchWithQueryAndMetadataAsync_UnsupportedEntityType_ThrowsArgumentException()
    {
        var pagination = new PaginationRequest { PageIndex = 1, PageSize = 10 };
        var act = () => Service.SearchWithQueryAndMetadataAsync<object, object>("test", pagination, TestUser);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Unsupported entity type*");
    }

    #endregion

    #region 9. SearchMetadata and SearchQuery in Response

    [Fact]
    public async Task SearchAsync_WithQuery_PopulatesSearchQuery()
    {
        SeedPartners(1);
        var request = new UnifiedSearchRequest
        {
            Query = "Searchable",
            Filters = null,
            PageIndex = 1,
            PageSize = 10
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.SearchQuery.Should().Be("Searchable");
    }

    [Fact]
    public async Task SearchAsync_WithoutQuery_SearchQueryNull()
    {
        SeedPartners(1);
        var request = new UnifiedSearchRequest { Query = null, Filters = null, PageIndex = 1, PageSize = 10 };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.SearchQuery.Should().BeNull();
    }

    #endregion

    #region 10. Structured Filters (equals, contains)

    [Fact]
    public async Task SearchAsync_WithEqualsFilter_AppliesFilter()
    {
        SeedPartners(3);
        var first = Context.Partners.First(p => !p.IsDeleted);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = new List<SearchFilter>
            {
                new() { field = "Name", value = first.Name, @operator = "equals", fieldType = "text" }
            },
            PageIndex = 1,
            PageSize = 10
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Be(first.Name);
    }

    [Fact]
    public async Task SearchAsync_WithNotEqualsFilter_ExcludesMatching()
    {
        SeedPartners(2);
        var first = Context.Partners.First(p => !p.IsDeleted);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = new List<SearchFilter>
            {
                new() { field = "Name", value = first.Name, @operator = "neq", fieldType = "text" }
            },
            PageIndex = 1,
            PageSize = 10
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().NotBe(first.Name);
    }

    [Fact]
    public async Task SearchAsync_FiltersWithEmptyField_Skipped()
    {
        SeedPartners(2);
        var request = new UnifiedSearchRequest
        {
            Query = null,
            Filters = new List<SearchFilter>
            {
                new() { field = "", value = "x", @operator = "equals", fieldType = "text" }
            },
            PageIndex = 1,
            PageSize = 10
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(2);
    }

    #endregion

    #region 11. IsDeleted Filtering (Soft Delete)

    [Fact]
    public async Task SearchAsync_ExcludesSoftDeleted()
    {
        SeedPartners(2);
        var toDelete = Context.Partners.First(p => !p.IsDeleted);
        toDelete.IsDeleted = true;
        toDelete.DeletedDate = DateTime.UtcNow;
        Context.SaveChanges();

        var request = new UnifiedSearchRequest { Query = null, Filters = null, PageIndex = 1, PageSize = 10 };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    #endregion

    #region 12. UnifiedSearchRequest and SearchFilter Model

    [Fact]
    public void UnifiedSearchRequest_DefaultValues()
    {
        var req = new UnifiedSearchRequest();
        req.PageIndex.Should().Be(1);
        req.PageSize.Should().Be(20);
        req.OrderBy.Should().Be("CreatedDate");
        req.Ascending.Should().BeFalse();
        req.FilterActive.Should().BeTrue();
    }

    [Fact]
    public void SearchFilter_DefaultValues()
    {
        var f = new SearchFilter();
        f.@operator.Should().Be("like");
        f.fieldType.Should().Be("text");
        f.logicalOperator.Should().Be("AND");
    }

    #endregion

    #region 13. SearchWithQueryAsync Delegation

    [Fact]
    public async Task SearchWithQueryAsync_DelegatesToSearchWithQueryAndMetadataAsync()
    {
        var pagination = new PaginationRequest { PageIndex = 1, PageSize = 10 };
        var act = () => Service.SearchWithQueryAsync<UNOPSPartner, PartnerModel>("test", pagination, TestUser);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region 14. SearchWithFiltersAsync_NoQuery

    [Fact]
    public async Task SearchWithFiltersAsync_SetsQueryToNull()
    {
        SeedPartners(1);
        var pagination = new PaginationRequest { PageIndex = 1, PageSize = 10 };
        var filters = new List<SearchFilter>();

        var result = await Service.SearchWithFiltersAsync<UNOPSPartner, PartnerModel>(filters, pagination, TestUser);

        result.Records.Should().HaveCount(1);
        result.SearchQuery.Should().BeNull();
    }

    #endregion

    #region 15. Text Search with Contains (InMemory)

    [Fact]
    public async Task SearchAsync_TextQuery_ContainsMatch()
    {
        SeedPartners(3);
        var request = new UnifiedSearchRequest
        {
            Query = "Searchable",
            Filters = null,
            PageIndex = 1,
            PageSize = 10
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().HaveCount(3);
        result.Records.Should().OnlyContain(r => r.Name.Contains("Searchable"));
    }

    [Fact]
    public async Task SearchAsync_TextQuery_NoMatch_ReturnsEmpty()
    {
        SeedPartners(2);
        var request = new UnifiedSearchRequest
        {
            Query = "NonexistentXyz123",
            Filters = null,
            PageIndex = 1,
            PageSize = 10
        };

        var result = await Service.SearchAsync<UNOPSPartner, PartnerModel>(request, TestUser);

        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    #endregion
}
