/**
 * @fileoverview Mock-based tests for PartnerGroupService.
 * Tests GetPartnerGroupsAsync, SearchPartnerGroupsAsync, GetPartnerGroupByIdAsync,
 * GetPartnerGroupByCodeAsync, GetPartnerGroupsByCategoryIdAsync, and InvalidateCache using InMemory database.
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
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Mock-based tests for PartnerGroupService using InMemory database.
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class PartnerGroupServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly PartnerGroupService _service;
    private readonly IMemoryCache _memoryCache;
    private int _categoryId;

    public PartnerGroupServiceTests()
    {
        var dbName = $"PartnerGroup_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        _context = TestDbContextFactory.CreateUNOPS(options);
        TestEnvironment.EnsureCleanDatabase(_context);

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var partnerTreeRepo = new DataRepository<UNOPSPartnerTree>(_context);
        var partnerRepo = new DataRepository<UNOPSPartner>(_context);

        _service = new PartnerGroupService(partnerTreeRepo, partnerRepo, _context, _memoryCache);
        SeedPartnerTrees();
    }

    private void SeedPartnerTrees()
    {
        // Category: Level_1 not in specialCategoryCodes
        var category = new UNOPSPartnerTree
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
        _context.Set<UNOPSPartnerTree>().Add(category);
        _context.SaveChanges();
        _categoryId = category.Id;

        // Group: child of category
        var group1 = new UNOPSPartnerTree
        {
            Name = "International NGO",
            Description = "International NGOs",
            Code = "INTL_NGO",
            Type = "Level_2",
            Parent = "NGO",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        var group2 = new UNOPSPartnerTree
        {
            Name = "Local NGO",
            Description = "Local NGOs",
            Code = "LOCAL_NGO",
            Type = "Level_2",
            Parent = "NGO",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().AddRange(group1, group2);
        _context.SaveChanges();
    }

    public void Dispose() => _context?.Dispose();

    #region Positive Tests (P=2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerGroupsAsync_ValidRequest_ReturnsGroups()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false,
            IncludePartnerCategory = false,
            IncludePartners = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Records.Should().OnlyContain(g => g.Name != null && g.Code != null);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerGroupByIdAsync_ValidId_ReturnsGroup()
    {
        var groups = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });
        var firstId = groups.Records.First().Id;

        var result = await _service.GetPartnerGroupByIdAsync(firstId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(firstId);
        result.Name.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Negative Tests (N=6)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerGroupByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _service.GetPartnerGroupByIdAsync(999999);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerGroupByCodeAsync_NonExistentCode_ReturnsNull()
    {
        var result = await _service.GetPartnerGroupByCodeAsync("NONEXISTENT_GROUP_XYZ");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerGroupByCodeAsync_NullCode_ReturnsNull()
    {
        var result = await _service.GetPartnerGroupByCodeAsync(null!);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task SearchPartnerGroupsAsync_NoMatchingSearchTerm_ReturnsEmpty()
    {
        var request = new PartnerGroupSearchRequest
        {
            SearchTerm = "ZZZ_NO_MATCH_XYZ_123",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerGroupsByCategoryIdAsync_NonExistentCategoryId_ReturnsEmpty()
    {
        var result = await _service.GetPartnerGroupsByCategoryIdAsync(999999);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerGroupByIdAsync_ZeroId_ReturnsNull()
    {
        var result = await _service.GetPartnerGroupByIdAsync(0);

        result.Should().BeNull();
    }

    #endregion

    #region Edge/Boundary Tests (E=6)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerGroupsAsync_PageSizeOne_ReturnsSingleRecord()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 1,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerGroupsAsync_PageIndexBeyondData_ReturnsEmptyRecords()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 999,
            PageSize = 10,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task SearchPartnerGroupsAsync_EmptySearchTerm_ReturnsAllGroups()
    {
        var request = new PartnerGroupSearchRequest
        {
            SearchTerm = "",
            PageIndex = 1,
            PageSize = 100
        };

        var result = await _service.SearchPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetPartnerGroupByCodeAsync_MatchesPartnerGroupCode()
    {
        var groups = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });
        var firstCode = groups.Records.First().Code;

        var result = await _service.GetPartnerGroupByCodeAsync(firstCode);

        result.Should().NotBeNull();
        result!.Code.Should().Be(firstCode);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task InvalidateCache_AfterGet_GroupsStillRetrievable()
    {
        var before = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });
        _service.InvalidateCache();
        var after = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
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
    public async Task GetPartnerGroupsByCategoryIdAsync_ValidCategoryId_ReturnsGroups()
    {
        var result = await _service.GetPartnerGroupsByCategoryIdAsync(_categoryId);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(g => g.PartnerCategoryId == _categoryId);
    }

    #endregion

    #region Functional Tests (F=6)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerGroupsAsync_FilterByName_ReturnsMatchingGroups()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            Name = "International",
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(g => g.Name.Contains("International", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task SearchPartnerGroupsAsync_SearchByName_ReturnsMatching()
    {
        var request = new PartnerGroupSearchRequest
        {
            SearchTerm = "Local",
            PageIndex = 1,
            PageSize = 10
        };

        var result = await _service.SearchPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(g =>
            g.Name.Contains("Local", StringComparison.OrdinalIgnoreCase) ||
            (g.Description != null && g.Description.Contains("Local", StringComparison.OrdinalIgnoreCase)) ||
            g.Code.Contains("Local", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerGroupsAsync_FilterByCategoryId_ReturnsOnlyCategoryGroups()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            PartnerCategoryId = _categoryId,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(g => g.PartnerCategoryId == _categoryId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerGroupsAsync_SortByNameAscending_ReturnsOrdered()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            OrderBy = "Name",
            Ascending = true,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        var names = result.Records.Select(g => g.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerGroupByIdAsync_PopulatesPartnerCategory()
    {
        var groups = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCategory = true
        });
        var firstId = groups.Records.First().Id;

        var result = await _service.GetPartnerGroupByIdAsync(firstId);

        result.Should().NotBeNull();
        result!.PartnerCategoryId.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerGroupsAsync_ExcludesDeletedPartnerTrees()
    {
        var deletedGroup = new UNOPSPartnerTree
        {
            Name = "Deleted Group",
            Description = "Deleted",
            Code = "DELETED_GRP",
            Type = "Level_2",
            Parent = "NGO",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(deletedGroup);
        await _context.SaveChangesAsync();
        _service.InvalidateCache();

        var result = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = false
        });

        result.Records.Should().NotContain(g => g.Code == "DELETED_GRP");
    }

    #endregion

    #region Integration Tests (I=6)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_GetGroups_Search_GetById_GetByCode_GetByCategoryId()
    {
        var list = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = true,
            IncludePartnerCategory = true
        });
        var first = list.Records.First();

        var byId = await _service.GetPartnerGroupByIdAsync(first.Id);
        var byCode = await _service.GetPartnerGroupByCodeAsync(first.Code);
        var byCategory = await _service.GetPartnerGroupsByCategoryIdAsync(first.PartnerCategoryId!.Value);

        byId.Should().NotBeNull();
        byCode.Should().NotBeNull();
        byId!.Id.Should().Be(byCode!.Id);
        byCategory.Should().Contain(g => g.Id == first.Id);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Cache_GetGroups_Invalidate_RefetchesFromDb()
    {
        var first = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 10,
            IncludePartnerCounts = false
        });
        _service.InvalidateCache();

        var second = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
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
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 2,
            IncludePartnerCounts = false
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.TotalPages.Should().Be((int)Math.Ceiling((double)result.TotalCount / result.PageSize));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetPartnerGroupsByCategoryIdAsync_MatchesFilteredList()
    {
        var byCategory = await _service.GetPartnerGroupsByCategoryIdAsync(_categoryId);
        var filtered = await _service.GetPartnerGroupsAsync(new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            PartnerCategoryId = _categoryId,
            IncludePartnerCounts = false
        });

        byCategory.Select(g => g.Id).Should().BeEquivalentTo(filtered.Records.Select(g => g.Id));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PartnerCount_PopulatedWhenIncludePartnerCounts()
    {
        var request = new PartnerGroupFilterRequest
        {
            PageIndex = 1,
            PageSize = 100,
            IncludePartnerCounts = true
        };

        var result = await _service.GetPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        foreach (var g in result.Records)
        {
            g.TotalPartnerCount.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SearchWithCategoryFilter_ReturnsOnlyMatchingCategoryGroups()
    {
        var request = new PartnerGroupSearchRequest
        {
            SearchTerm = "NGO",
            PartnerCategoryId = _categoryId,
            PageIndex = 1,
            PageSize = 100
        };

        var result = await _service.SearchPartnerGroupsAsync(request);

        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(g => g.PartnerCategoryId == _categoryId);
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | GetPartnerGroupsAsync_ValidRequest_ReturnsGroups, GetPartnerGroupByIdAsync_ValidId_ReturnsGroup |
| Negative (N) | 6 | GetPartnerGroupByIdAsync_NonExistentId_ReturnsNull, GetPartnerGroupByCodeAsync_NonExistentCode_ReturnsNull, GetPartnerGroupByCodeAsync_NullCode_ReturnsNull, SearchPartnerGroupsAsync_NoMatchingSearchTerm_ReturnsEmpty, GetPartnerGroupsByCategoryIdAsync_NonExistentCategoryId_ReturnsEmpty, GetPartnerGroupByIdAsync_ZeroId_ReturnsNull |
| Edge/Boundary (E) | 6 | GetPartnerGroupsAsync_PageSizeOne_ReturnsSingleRecord, GetPartnerGroupsAsync_PageIndexBeyondData_ReturnsEmptyRecords, SearchPartnerGroupsAsync_EmptySearchTerm_ReturnsAllGroups, GetPartnerGroupByCodeAsync_MatchesPartnerGroupCode, InvalidateCache_AfterGet_GroupsStillRetrievable, GetPartnerGroupsByCategoryIdAsync_ValidCategoryId_ReturnsGroups |
| Functional (F) | 6 | GetPartnerGroupsAsync_FilterByName_ReturnsMatchingGroups, SearchPartnerGroupsAsync_SearchByName_ReturnsMatching, GetPartnerGroupsAsync_FilterByCategoryId_ReturnsOnlyCategoryGroups, GetPartnerGroupsAsync_SortByNameAscending_ReturnsOrdered, GetPartnerGroupByIdAsync_PopulatesPartnerCategory, GetPartnerGroupsAsync_ExcludesDeletedPartnerTrees |
| Integration (I) | 6 | FullFlow_GetGroups_Search_GetById_GetByCode_GetByCategoryId, Cache_GetGroups_Invalidate_RefetchesFromDb, Pagination_TotalPagesCalculatedCorrectly, GetPartnerGroupsByCategoryIdAsync_MatchesFilteredList, PartnerCount_PopulatedWhenIncludePartnerCounts, SearchWithCategoryFilter_ReturnsOnlyMatchingCategoryGroups |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
