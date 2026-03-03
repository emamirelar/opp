/**
 * @fileoverview Mock-based tests for PartnerCategoryService.
 * Tests GetPartnerCategoriesAsync, SearchPartnerCategoriesAsync, GetPartnerCategoryByIdAsync,
 * GetPartnerCategoryByCodeAsync, and InvalidateCache using InMemory database.
 *
 * Ratio: P=2, N=6+, E=6+, F=6+, I=6+
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Mock-based tests for PartnerCategoryService using InMemory database.
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class PartnerCategoryServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly PartnerCategoryService _service;
    private readonly IMemoryCache _memoryCache;
    private readonly string _dbName;

    public PartnerCategoryServiceTests()
    {
        _dbName = $"PartnerCategory_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        _context = TestDbContextFactory.CreateUNOPS(options);
        TestEnvironment.EnsureCleanDatabase(_context);

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var partnerTreeRepo = new DataRepository<UNOPSPartnerTree>(_context);
        var partnerRepo = new DataRepository<UNOPSPartner>(_context);

        _service = new PartnerCategoryService(partnerTreeRepo, partnerRepo, _context, _memoryCache);
        SeedPartnerTrees();
    }

    private void SeedPartnerTrees()
    {
        // Level_1 not in specialCategoryCodes = Category (e.g., NGO, PRIVATE)
        var cat1 = new UNOPSPartnerTree
        {
            Name = "NGO Category",
            Description = "NGO partners",
            Code = "NGO",
            Type = "Level_1",
            Parent = null,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        var cat2 = new UNOPSPartnerTree
        {
            Name = "Private Sector",
            Description = "Private sector",
            Code = "PRIVATE",
            Type = "Level_1",
            Parent = null,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        // Level_2 under MULTILATERAL = Category
        var cat3 = new UNOPSPartnerTree
        {
            Name = "UN Agency",
            Description = "UN agencies",
            Code = "UN_AGENCY",
            Type = "Level_2",
            Parent = "MULTILATERAL",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().AddRange(cat1, cat2, cat3);
        _context.SaveChanges();
    }

    public void Dispose() => _context?.Dispose();

    #region Positive Tests (P=2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerCategoriesAsync_ValidRequest_ReturnsCategories()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false,
            IncludePartnerGroups = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Records.Should().OnlyContain(c => c.Name != null && c.Code != null);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerCategoryByIdAsync_ValidId_ReturnsCategory()
    {
        var categories = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false,
            IncludePartnerGroups = false
        });
        var firstId = categories.Records.First().Id;

        var result = await _service.GetPartnerCategoryByIdAsync(firstId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(firstId);
        result.Name.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Negative Tests (N=6)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerCategoryByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _service.GetPartnerCategoryByIdAsync(999999);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerCategoryByCodeAsync_NonExistentCode_ReturnsNull()
    {
        var result = await _service.GetPartnerCategoryByCodeAsync("NONEXISTENT_CODE_XYZ");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerCategoryByCodeAsync_NullCode_ReturnsNull()
    {
        var result = await _service.GetPartnerCategoryByCodeAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task SearchPartnerCategoriesAsync_NoMatchingSearchTerm_ReturnsEmpty()
    {
        var request = new PartnerCategorySearchRequest
        {
            SearchTerm = "ZZZ_NO_MATCH_XYZ_123",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerCategoriesAsync_FilterByNonExistentType_ReturnsEmpty()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            Type = "Level_99",
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerCategoryByIdAsync_ZeroId_ReturnsNull()
    {
        var result = await _service.GetPartnerCategoryByIdAsync(0);

        result.Should().BeNull();
    }

    #endregion

    #region Edge/Boundary Tests (E=6)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerCategoriesAsync_PageSizeOne_ReturnsSingleRecord()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 1,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerCategoriesAsync_PageIndexBeyondData_ReturnsEmptyRecords()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 999,
            PageSize = 10,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task SearchPartnerCategoriesAsync_EmptySearchTerm_ReturnsAllCategories()
    {
        var request = new PartnerCategorySearchRequest
        {
            SearchTerm = "",
            PageIndex = 1,
            PageSize = 100
        };

        var result = await _service.SearchPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerCategoryByCodeAsync_MatchesPartnerCategoryCode()
    {
        var categories = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });
        var firstCode = categories.Records.First().Code;

        var result = await _service.GetPartnerCategoryByCodeAsync(firstCode);

        result.Should().NotBeNull();
        result!.Code.Should().Be(firstCode);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task InvalidateCache_AfterGet_CategoriesStillRetrievable()
    {
        var before = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });
        _service.InvalidateCache();
        var after = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });

        after.Should().NotBeNull();
        after.Records.Should().HaveCount(before.Records.Count);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerCategoriesAsync_IncludePartnerGroups_PopulatesPartnerGroups()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = true,
            IncludePartnerGroups = true
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
        foreach (var cat in result.Records)
        {
            cat.PartnerGroups.Should().NotBeNull();
        }
    }

    #endregion

    #region Functional Tests (F=6)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerCategoriesAsync_FilterByName_ReturnsMatchingCategories()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            Name = "NGO",
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(c => c.Name.Contains("NGO", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task SearchPartnerCategoriesAsync_SearchByName_ReturnsMatching()
    {
        var request = new PartnerCategorySearchRequest
        {
            SearchTerm = "Private",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(c =>
            c.Name.Contains("Private", StringComparison.OrdinalIgnoreCase) ||
            c.Description.Contains("Private", StringComparison.OrdinalIgnoreCase) ||
            c.Code.Contains("Private", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerCategoriesAsync_SortByNameAscending_ReturnsOrdered()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            OrderBy = "Name",
            Ascending = true,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        var names = result.Records.Select(c => c.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerCategoriesAsync_SortByCodeDescending_ReturnsOrdered()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            OrderBy = "Code",
            Ascending = false,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        var codes = result.Records.Select(c => c.Code).ToList();
        codes.Should().BeInDescendingOrder();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerCategoryByIdAsync_PopulatesPartnerCounts()
    {
        var categories = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = true
        });
        var firstId = categories.Records.First().Id;

        var result = await _service.GetPartnerCategoryByIdAsync(firstId);

        result.Should().NotBeNull();
        result!.TotalPartnerCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerCategoriesAsync_ExcludesDeletedPartnerTrees()
    {
        var deletedTree = new UNOPSPartnerTree
        {
            Name = "Deleted Cat",
            Description = "Deleted",
            Code = "DELETED_CAT",
            Type = "Level_1",
            Parent = null,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(deletedTree);
        await _context.SaveChangesAsync();
        _service.InvalidateCache();

        var result = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });

        result.Records.Should().NotContain(c => c.Code == "DELETED_CAT");
    }

    #endregion

    #region Integration Tests (I=6)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetCategories_Search_GetById_GetByCode()
    {
        var list = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = true,
            IncludePartnerGroups = true
        });
        var first = list.Records.First();

        var byId = await _service.GetPartnerCategoryByIdAsync(first.Id);
        var byCode = await _service.GetPartnerCategoryByCodeAsync(first.Code);

        byId.Should().NotBeNull();
        byCode.Should().NotBeNull();
        byId!.Id.Should().Be(byCode!.Id);
        byId.Name.Should().Be(byCode.Name);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Cache_GetCategories_Invalidate_RefetchesFromDb()
    {
        var first = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });
        _service.InvalidateCache();

        var second = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });

        second.Should().NotBeNull();
        second.Records.Should().HaveCount(first.Records.Count);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Pagination_TotalPagesCalculatedCorrectly()
    {
        var request = new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 2,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerCategoriesAsync(request);

        result.Should().NotBeNull();
        result.TotalPages.Should().Be((int)Math.Ceiling((double)result.TotalCount / result.PageSize));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchAndFilter_Combined_ReturnsConsistentResults()
    {
        var searchResult = await _service.SearchPartnerCategoriesAsync(new PartnerCategorySearchRequest
        {
            SearchTerm = "NGO",
            PageIndex = 1,
            PageSize = 10
        });
        var filterResult = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            Name = "NGO",
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });

        searchResult.Records.Should().NotBeEmpty();
        filterResult.Records.Should().NotBeEmpty();
        searchResult.Records.Select(c => c.Id).Should().BeEquivalentTo(filterResult.Records.Select(c => c.Id));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Level1AndLevel2Categories_BothIncludedInResults()
    {
        var result = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });

        var level1 = result.Records.Where(c => c.Type == "Level_1").ToList();
        var level2 = result.Records.Where(c => c.Type == "Level_2").ToList();

        level1.Should().NotBeEmpty();
        level2.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SpecialCategoryCodes_ExcludedFromLevel1Categories()
    {
        var multilateral = new UNOPSPartnerTree
        {
            Name = "Multilateral",
            Description = "Multilateral orgs",
            Code = "MULTILATERAL",
            Type = "Level_1",
            Parent = null,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(multilateral);
        await _context.SaveChangesAsync();
        _service.InvalidateCache();

        var result = await _service.GetPartnerCategoriesAsync(new PartnerCategoryFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });

        result.Records.Should().NotContain(c => c.Code == "MULTILATERAL");
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | GetPartnerCategoriesAsync_ValidRequest_ReturnsCategories, GetPartnerCategoryByIdAsync_ValidId_ReturnsCategory |
| Negative (N) | 6 | GetPartnerCategoryByIdAsync_NonExistentId_ReturnsNull, GetPartnerCategoryByCodeAsync_NonExistentCode_ReturnsNull, GetPartnerCategoryByCodeAsync_NullCode_ReturnsNull, SearchPartnerCategoriesAsync_NoMatchingSearchTerm_ReturnsEmpty, GetPartnerCategoriesAsync_FilterByNonExistentType_ReturnsEmpty, GetPartnerCategoryByIdAsync_ZeroId_ReturnsNull |
| Edge/Boundary (E) | 6 | GetPartnerCategoriesAsync_PageSizeOne_ReturnsSingleRecord, GetPartnerCategoriesAsync_PageIndexBeyondData_ReturnsEmptyRecords, SearchPartnerCategoriesAsync_EmptySearchTerm_ReturnsAllCategories, GetPartnerCategoryByCodeAsync_MatchesPartnerCategoryCode, InvalidateCache_AfterGet_CategoriesStillRetrievable, GetPartnerCategoriesAsync_IncludePartnerGroups_PopulatesPartnerGroups |
| Functional (F) | 6 | GetPartnerCategoriesAsync_FilterByName_ReturnsMatchingCategories, SearchPartnerCategoriesAsync_SearchByName_ReturnsMatching, GetPartnerCategoriesAsync_SortByNameAscending_ReturnsOrdered, GetPartnerCategoriesAsync_SortByCodeDescending_ReturnsOrdered, GetPartnerCategoryByIdAsync_PopulatesPartnerCounts, GetPartnerCategoriesAsync_ExcludesDeletedPartnerTrees |
| Integration (I) | 6 | FullFlow_GetCategories_Search_GetById_GetByCode, Cache_GetCategories_Invalidate_RefetchesFromDb, Pagination_TotalPagesCalculatedCorrectly, SearchAndFilter_Combined_ReturnsConsistentResults, Level1AndLevel2Categories_BothIncludedInResults, SpecialCategoryCodes_ExcludedFromLevel1Categories |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
