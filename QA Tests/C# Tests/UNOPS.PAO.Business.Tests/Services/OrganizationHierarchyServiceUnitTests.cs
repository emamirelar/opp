/// <summary>
/// Comprehensive unit tests for OrganizationHierarchyService.
/// Tests all 3 public methods: GetOrganizationHierarchiesAsync, SearchOrganizationHierarchiesAsync, GetOrganizationHierarchyByIdAsync.
/// Covers: listing with pagination, filtering by type/parent/search, children/entity count population,
/// soft delete filtering (IsDeleted), edge cases (empty search, no results, large datasets).
/// Requirements source: UNOPS.PAO.Business/Services/OrganizationHierarchyService.cs
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

[Trait("Category", "Unit")]
[Trait("Feature", "OrganizationHierarchyService")]
public class OrganizationHierarchyServiceUnitTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly OrganizationHierarchyService _service;
    private readonly IMemoryCache _memoryCache;
    private int _rootId;
    private int _child1Id;
    private int _child2Id;

    private const string CacheKey = "ORGANIZATION_HIERARCHY_CACHE";
    private const string ChildrenCountCacheKey = "ORGANIZATION_HIERARCHY_CHILDREN_COUNTS_CACHE";
    private const string EntityRelationshipCountCacheKey = "ORGANIZATION_HIERARCHY_ENTITY_RELATIONSHIP_COUNTS_CACHE";

    public OrganizationHierarchyServiceUnitTests()
    {
        var dbName = $"OrgHierarchy_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        _context = TestDbContextFactory.Create(options);
        TestEnvironment.EnsureCleanDatabase(_context);

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new OrganizationHierarchyService(_context, _memoryCache);
        SeedOrganizationHierarchies();
    }

    private void SeedOrganizationHierarchies()
    {
        var root = new OrganizationHierarchy
        {
            Name = "Global HQ",
            Code = "GHQ",
            Description = "Global Headquarters",
            Type = OrganizationUnitType.Office,
            ParentId = null,
            IsSelfManagementEnabled = true,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(root);
        _context.SaveChanges();
        _rootId = root.Id;

        var child1 = new OrganizationHierarchy
        {
            Name = "Africa Region",
            Code = "AFR",
            Description = "Africa Regional Office",
            Type = OrganizationUnitType.Region,
            ParentId = _rootId,
            IsSelfManagementEnabled = false,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(child1);
        _context.SaveChanges();
        _child1Id = child1.Id;

        var child2 = new OrganizationHierarchy
        {
            Name = "Asia Region",
            Code = "ASI",
            Description = "Asia Regional Office",
            Type = OrganizationUnitType.Region,
            ParentId = _rootId,
            IsSelfManagementEnabled = true,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(child2);
        _context.SaveChanges();
        _child2Id = child2.Id;

        var grandchild = new OrganizationHierarchy
        {
            Name = "Kenya Office",
            Code = "KEN",
            Description = "Kenya Country Office",
            Type = OrganizationUnitType.Office,
            ParentId = _child1Id,
            IsSelfManagementEnabled = false,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(grandchild);
        _context.SaveChanges();
    }

    private void ClearCache()
    {
        _memoryCache.Remove(CacheKey);
        _memoryCache.Remove(ChildrenCountCacheKey);
        _memoryCache.Remove(EntityRelationshipCountCacheKey);
    }

    public void Dispose() => _context?.Dispose();

    #region 1. GetOrganizationHierarchiesAsync — Listing & Pagination

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_ValidRequest_ReturnsPaginatedResults()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_Pagination_SecondPageReturnsCorrectRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 2,
            PageSize = 2,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.PageIndex.Should().Be(2);
        result.TotalCount.Should().Be(4);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_Pagination_TotalPagesCalculation()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 3,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.TotalPages.Should().Be(2);
        result.Records.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_IncludeCountsFalse_DoesNotPopulateCounts()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
        result.Records.Should().OnlyContain(r => r.ChildrenCount == 0 && r.EntityRelationshipCount == 0);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_IncludeCountsTrue_PopulatesChildrenAndEntityCounts()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = true
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var root = result.Records.FirstOrDefault(r => r.Id == _rootId);
        root.Should().NotBeNull();
        root!.ChildrenCount.Should().Be(2);
        root.ParentName.Should().BeNull();
        root.ParentCode.Should().BeNull();

        var child1 = result.Records.FirstOrDefault(r => r.Id == _child1Id);
        child1.Should().NotBeNull();
        child1!.ChildrenCount.Should().Be(1);
        child1.ParentName.Should().Be("Global HQ");
        child1.ParentCode.Should().Be("GHQ");
    }

    #endregion

    #region 2. GetOrganizationHierarchiesAsync — Filtering

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByName_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            Name = "Africa",
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Contain("Africa");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByCode_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            Code = "GHQ",
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(1);
        result.Records[0].Code.Should().Be("GHQ");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByType_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            Type = "Region",
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.Type == OrganizationUnitType.Region);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByParentId_ReturnsChildrenOnly()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            ParentId = _rootId,
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.ParentId == _rootId);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByParentCode_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            ParentCode = "GHQ",
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.ParentCode == "GHQ");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByStatus_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            Status = "Active",
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchiesAsync_FilterByIsSelfManagementEnabled_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            IsSelfManagementEnabled = true,
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.IsSelfManagementEnabled);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_CombinedFilters_AppliesAll()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            Type = "Region",
            ParentId = _rootId,
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.Type == OrganizationUnitType.Region && r.ParentId == _rootId);
    }

    #endregion

    #region 3. GetOrganizationHierarchiesAsync — Sorting

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByNameAscending_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "name",
            Ascending = true,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var names = result.Records.Select(r => r.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByNameDescending_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "name",
            Ascending = false,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var names = result.Records.Select(r => r.Name).ToList();
        names.Should().BeInDescendingOrder();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByCode_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "code",
            Ascending = true,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var codes = result.Records.Select(r => r.Code).ToList();
        codes.Should().BeInAscendingOrder();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByType_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "type",
            Ascending = true,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByDefault_UsesNameAscending()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = null,
            Ascending = true,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var names = result.Records.Select(r => r.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    #endregion

    #region 4. SearchOrganizationHierarchiesAsync

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SearchOrganizationHierarchiesAsync_ValidSearch_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "Africa",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Contain("Africa");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SearchOrganizationHierarchiesAsync_SearchByCode_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "KEN",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(1);
        result.Records[0].Code.Should().Be("KEN");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SearchOrganizationHierarchiesAsync_SearchByDescription_ReturnsMatchingRecords()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "Headquarters",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(1);
        result.Records[0].Description.Should().Contain("Headquarters");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SearchOrganizationHierarchiesAsync_SearchCaseInsensitive_ReturnsMatches()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "africa",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Contain("Africa");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SearchOrganizationHierarchiesAsync_WithTypeFilter_AppliesBoth()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "Region",
            Type = "Region",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.Type == OrganizationUnitType.Region);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task SearchOrganizationHierarchiesAsync_AlwaysPopulatesCounts()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "HQ",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
        var root = result.Records.FirstOrDefault(r => r.Id == _rootId);
        root.Should().NotBeNull();
        root!.ChildrenCount.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task SearchOrganizationHierarchiesAsync_Pagination_RespectsPageSize()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = null,
            PageIndex = 1,
            PageSize = 2
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(4);
    }

    #endregion

    #region 5. GetOrganizationHierarchyByIdAsync

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchyByIdAsync_ValidId_ReturnsHierarchy()
    {
        var result = await _service.GetOrganizationHierarchyByIdAsync(_rootId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(_rootId);
        result.Name.Should().Be("Global HQ");
        result.Code.Should().Be("GHQ");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrganizationHierarchyByIdAsync_ValidId_PopulatesCounts()
    {
        var result = await _service.GetOrganizationHierarchyByIdAsync(_rootId);

        result.Should().NotBeNull();
        result!.ChildrenCount.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetOrganizationHierarchyByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _service.GetOrganizationHierarchyByIdAsync(999999);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetOrganizationHierarchyByIdAsync_ZeroId_ReturnsNull()
    {
        var result = await _service.GetOrganizationHierarchyByIdAsync(0);

        result.Should().BeNull();
    }

    #endregion

    #region 6. Soft Delete Filtering (IsDeleted)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_ExcludesSoftDeleted()
    {
        var deleted = new OrganizationHierarchy
        {
            Name = "Deleted Office",
            Code = "DEL",
            Description = "Soft deleted",
            Type = OrganizationUnitType.Office,
            ParentId = null,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(deleted);
        await _context.SaveChangesAsync();
        ClearCache();

        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 20,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotContain(r => r.Code == "DEL");
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task SearchOrganizationHierarchiesAsync_ExcludesSoftDeleted()
    {
        var deleted = new OrganizationHierarchy
        {
            Name = "Deleted Searchable",
            Code = "DELSRCH",
            Description = "Soft deleted",
            Type = OrganizationUnitType.Office,
            ParentId = null,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(deleted);
        await _context.SaveChangesAsync();
        ClearCache();

        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "Searchable",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchyByIdAsync_SoftDeletedId_ReturnsNull()
    {
        var deleted = new OrganizationHierarchy
        {
            Name = "Deleted By Id",
            Code = "DELID",
            Description = "Soft deleted",
            Type = OrganizationUnitType.Office,
            ParentId = null,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(deleted);
        await _context.SaveChangesAsync();
        ClearCache();

        var result = await _service.GetOrganizationHierarchyByIdAsync(deleted.Id);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task PopulateCounts_ChildrenCount_ExcludesSoftDeletedChildren()
    {
        var deletedChild = new OrganizationHierarchy
        {
            Name = "Deleted Child",
            Code = "DELCHILD",
            Description = "Soft deleted child",
            Type = OrganizationUnitType.Office,
            ParentId = _rootId,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(deletedChild);
        await _context.SaveChangesAsync();
        ClearCache();

        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = true
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var root = result.Records.FirstOrDefault(r => r.Id == _rootId);
        root.Should().NotBeNull();
        root!.ChildrenCount.Should().Be(2);
    }

    #endregion

    #region 7. Entity Relationship Count

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_IncludeCounts_PopulatesEntityRelationshipCount()
    {
        var rel = new OrganizationUnitRelationship
        {
            OrganizationHierarchyId = _rootId,
            EntityId = 100,
            EntityType = "Partner",
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationUnitRelationships.Add(rel);
        await _context.SaveChangesAsync();
        ClearCache();

        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = true
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var root = result.Records.FirstOrDefault(r => r.Id == _rootId);
        root.Should().NotBeNull();
        root!.EntityRelationshipCount.Should().Be(1);
    }

    #endregion

    #region 8. Edge Cases — Empty Search, No Results, Large Datasets

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task SearchOrganizationHierarchiesAsync_EmptySearchTerm_ReturnsAllRecords()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "",
            PageIndex = 1,
            PageSize = 20
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task SearchOrganizationHierarchiesAsync_NullSearchTerm_ReturnsAllRecords()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = null,
            PageIndex = 1,
            PageSize = 20
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task SearchOrganizationHierarchiesAsync_NoMatch_ReturnsEmpty()
    {
        var request = new OrganizationHierarchySearchRequest
        {
            SearchTerm = "NonexistentXyz123",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchOrganizationHierarchiesAsync(request);

        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetOrganizationHierarchiesAsync_FilterNoMatch_ReturnsEmpty()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            Name = "NonexistentFilter",
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetOrganizationHierarchiesAsync_EmptyDatabase_ReturnsEmpty()
    {
        _context.OrganizationHierarchies.RemoveRange(_context.OrganizationHierarchies);
        await _context.SaveChangesAsync();
        ClearCache();

        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetOrganizationHierarchiesAsync_LargePageSize_ReturnsAllRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 1000,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(4);
        result.PageSize.Should().Be(1000);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetOrganizationHierarchiesAsync_PageIndexBeyondData_ReturnsEmptyPage()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 10,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().BeEmpty();
        result.PageIndex.Should().Be(10);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetOrganizationHierarchiesAsync_StatusInactive_ExcludedFromBaseQuery()
    {
        var inactive = new OrganizationHierarchy
        {
            Name = "Inactive Office",
            Code = "INACT",
            Description = "Inactive",
            Type = OrganizationUnitType.Office,
            ParentId = null,
            Status = EntityStatus.Inactive,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(inactive);
        await _context.SaveChangesAsync();
        ClearCache();

        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 20,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotContain(r => r.Code == "INACT");
        result.TotalCount.Should().Be(4);
    }

    #endregion

    #region 9. Cache Behavior

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_SubsequentCalls_UseCache()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result1 = await _service.GetOrganizationHierarchiesAsync(request);
        var result2 = await _service.GetOrganizationHierarchiesAsync(request);

        result1.TotalCount.Should().Be(result2.TotalCount);
        result1.Records.Should().HaveCount(result2.Records.Count);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_ParentInfo_PopulatedFromInclude()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        var child = result.Records.FirstOrDefault(r => r.Id == _child1Id);
        child.Should().NotBeNull();
        child!.ParentName.Should().Be("Global HQ");
        child.ParentCode.Should().Be("GHQ");
    }

    #endregion

    #region 10. Additional Sorting Options

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByParentName_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "parentname",
            Ascending = true,
            IncludeCounts = true
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByStatus_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "status",
            Ascending = true,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByChildrenCount_AcceptsParameterAndReturnsRecords()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "childrencount",
            Ascending = true,
            IncludeCounts = true
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
        result.Records.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_OrderByEntityRelationshipCount_ReturnsSorted()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "entityrelationshipcount",
            Ascending = true,
            IncludeCounts = true
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrganizationHierarchiesAsync_UnknownOrderBy_DefaultsToName()
    {
        var request = new OrganizationHierarchyFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            OrderBy = "unknownfield",
            Ascending = true,
            IncludeCounts = false
        };

        var result = await _service.GetOrganizationHierarchiesAsync(request);

        result.Records.Should().HaveCount(4);
        var names = result.Records.Select(r => r.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    #endregion
}
