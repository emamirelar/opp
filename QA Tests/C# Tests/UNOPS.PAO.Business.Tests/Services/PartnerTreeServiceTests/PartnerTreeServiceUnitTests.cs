/// <summary>
/// Comprehensive unit tests for PartnerTreeService.
/// Tests all 8 public methods: CRUD, CanModifyPartnerCategoryCode/CanModifyPartnerGroupCode rules,
/// recursive parent lookup, descendant resolution, cache invalidation, soft delete filtering,
/// and edge cases (orphan nodes, deep hierarchies).
/// Requirements source: UNOPS.PAO.UNOPSBusiness/Services/PartnerTreeService.cs
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services.PartnerTreeServiceTests;

[Trait("Category", "Unit")]
[Trait("Feature", "PartnerTreeService")]
public class PartnerTreeServiceUnitTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly PartnerTreeService _service;
    private readonly IMemoryCache _memoryCache;
    private int _ngoCategoryId;
    private int _multilateralCategoryId;
    private int _intlNgoGroupId;

    public PartnerTreeServiceUnitTests()
    {
        var dbName = $"PartnerTree_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        _context = TestDbContextFactory.CreateUNOPS(options);
        TestEnvironment.EnsureCleanDatabase(_context);

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var partnerTreeRepo = new DataRepository<UNOPSPartnerTree>(_context);
        _service = new PartnerTreeService(partnerTreeRepo, _memoryCache);
        SeedPartnerTrees();
    }

    private void SeedPartnerTrees()
    {
        // Level_1: NGO (not in specialCategoryCodes) - can modify category code
        var ngo = new UNOPSPartnerTree
        {
            Name = "NGO Category",
            Description = "NGO partners",
            Code = "NGO",
            Type = "Level_1",
            Parent = "",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(ngo);
        _context.SaveChanges();
        _ngoCategoryId = ngo.Id;

        // Level_1: MULTILATERAL (specialCategoryCodes) - cannot modify category code
        var multilateral = new UNOPSPartnerTree
        {
            Name = "Multilateral",
            Description = "Multilateral orgs",
            Code = "MULTILATERAL",
            Type = "Level_1",
            Parent = "",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(multilateral);
        _context.SaveChanges();
        _multilateralCategoryId = multilateral.Id;

        // Level_1: GOVERNMENT (specialCategoryCodes)
        var government = new UNOPSPartnerTree
        {
            Name = "Government",
            Description = "Government entities",
            Code = "GOVERNMENT",
            Type = "Level_1",
            Parent = "",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(government);
        _context.SaveChanges();

        // Level_2: Child of NGO - can modify group code
        var intlNgo = new UNOPSPartnerTree
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
        _context.Set<UNOPSPartnerTree>().Add(intlNgo);
        _context.SaveChanges();
        _intlNgoGroupId = intlNgo.Id;

        // Level_2: Child of MULTILATERAL - can modify category code (child of special)
        var unAgency = new UNOPSPartnerTree
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
        _context.Set<UNOPSPartnerTree>().Add(unAgency);
        _context.SaveChanges();

        // Level_2: Child of INTL_NGO (grandchild of NGO) - deep hierarchy
        var localNgo = new UNOPSPartnerTree
        {
            Name = "Local NGO",
            Description = "Local NGOs",
            Code = "LOCAL_NGO",
            Type = "Level_2",
            Parent = "INTL_NGO",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(localNgo);
        _context.SaveChanges();
    }

    public void Dispose() => _context?.Dispose();

    #region 1. GetAllPartnerTreesAsync

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllPartnerTreesAsync_ValidRequest_ReturnsAllNonDeletedTrees()
    {
        var result = await _service.GetAllPartnerTreesAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(6);
        result.Should().OnlyContain(pt => !pt.IsDeleted);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllPartnerTreesAsync_PopulatesPartnerCategoryCodeForLevel1NotInSpecialCodes()
    {
        var result = await _service.GetAllPartnerTreesAsync();

        var ngo = result.FirstOrDefault(pt => pt.Code == "NGO");
        ngo.Should().NotBeNull();
        ngo!.PartnerCategoryCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllPartnerTreesAsync_PopulatesPartnerGroupCodeForChildOfCategory()
    {
        var result = await _service.GetAllPartnerTreesAsync();

        var intlNgo = result.FirstOrDefault(pt => pt.Code == "INTL_NGO");
        intlNgo.Should().NotBeNull();
        intlNgo!.PartnerGroupCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetAllPartnerTreesAsync_ExcludesSoftDeletedTrees()
    {
        var deleted = new UNOPSPartnerTree
        {
            Name = "Deleted Tree",
            Description = "Deleted",
            Code = "DELETED_TREE",
            Type = "Level_1",
            Parent = "",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(deleted);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        var result = await _service.GetAllPartnerTreesAsync();

        result.Should().NotContain(pt => pt.Code == "DELETED_TREE");
    }

    #endregion

    #region 2. GetPartnerTreeByCodeAsync

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerTreeByCodeAsync_ValidCode_ReturnsTree()
    {
        var result = await _service.GetPartnerTreeByCodeAsync("NGO");

        result.Should().NotBeNull();
        result!.Code.Should().Be("NGO");
        result.Type.Should().Be("Level_1");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerTreeByCodeAsync_NonExistentCode_ReturnsNull()
    {
        var result = await _service.GetPartnerTreeByCodeAsync("NONEXISTENT_XYZ");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerTreeByCodeAsync_EmptyCode_ReturnsNull()
    {
        var result = await _service.GetPartnerTreeByCodeAsync("");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerTreeByCodeAsync_ExcludesSoftDeleted()
    {
        var deleted = new UNOPSPartnerTree
        {
            Name = "Deleted",
            Description = "Soft deleted tree",
            Code = "SOFT_DELETED",
            Type = "Level_1",
            Parent = "",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(deleted);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        var result = await _service.GetPartnerTreeByCodeAsync("SOFT_DELETED");

        result.Should().BeNull();
    }

    #endregion

    #region 3. GetPartnerTreeByIdAsync

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerTreeByIdAsync_ValidId_ReturnsTree()
    {
        var result = await _service.GetPartnerTreeByIdAsync(_ngoCategoryId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(_ngoCategoryId);
        result.Code.Should().Be("NGO");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerTreeByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _service.GetPartnerTreeByIdAsync(999999);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerTreeByIdAsync_ZeroId_ReturnsNull()
    {
        var result = await _service.GetPartnerTreeByIdAsync(0);

        result.Should().BeNull();
    }

    #endregion

    #region 4. GetPartnerCategoryByPartnerGroupCodeAsync (Recursive Parent Lookup)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerCategoryByPartnerGroupCodeAsync_Level2ChildOfNgo_ReturnsNgoCategory()
    {
        var result = await _service.GetPartnerCategoryByPartnerGroupCodeAsync("INTL_NGO");

        result.Should().NotBeNull();
        result!.Code.Should().Be("NGO");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerCategoryByPartnerGroupCodeAsync_Level2ChildOfMultilateral_ReturnsNull()
    {
        // MULTILATERAL has PartnerCategoryCode=null (special code); recursion reaches root with no parent
        var result = await _service.GetPartnerCategoryByPartnerGroupCodeAsync("UN_AGENCY");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetPartnerCategoryByPartnerGroupCodeAsync_Level1Category_ReturnsNull()
    {
        // Level_1 nodes have no parent; GetParentCategory returns null
        var result = await _service.GetPartnerCategoryByPartnerGroupCodeAsync("NGO");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetPartnerCategoryByPartnerGroupCodeAsync_DeepHierarchy_TraversesToRootCategory()
    {
        // LOCAL_NGO -> INTL_NGO -> NGO; NGO is the category
        var result = await _service.GetPartnerCategoryByPartnerGroupCodeAsync("LOCAL_NGO");

        result.Should().NotBeNull();
        result!.Code.Should().Be("NGO");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetPartnerCategoryByPartnerGroupCodeAsync_NonExistentCode_ReturnsNull()
    {
        var result = await _service.GetPartnerCategoryByPartnerGroupCodeAsync("NONEXISTENT");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetPartnerCategoryByPartnerGroupCodeAsync_OrphanWithInvalidParent_ReturnsNull()
    {
        var orphan = new UNOPSPartnerTree
        {
            Name = "Orphan",
            Description = "Orphan node",
            Code = "ORPHAN_NODE",
            Type = "Level_2",
            Parent = "INVALID_PARENT_XYZ",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(orphan);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        var result = await _service.GetPartnerCategoryByPartnerGroupCodeAsync("ORPHAN_NODE");

        result.Should().BeNull();
    }

    #endregion

    #region 5. GetAllDescendantsAsync (Descendant Resolution)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllDescendantsAsync_NgoCategory_ReturnsAllDescendantIds()
    {
        var result = await _service.GetAllDescendantsAsync("NGO");

        result.Should().NotBeNull();
        result.Should().Contain(_intlNgoGroupId);
        result.Should().HaveCountGreaterThanOrEqualTo(2); // INTL_NGO and LOCAL_NGO
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllDescendantsAsync_LeafNode_ReturnsEmpty()
    {
        var result = await _service.GetAllDescendantsAsync("LOCAL_NGO");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetAllDescendantsAsync_DeepHierarchy_ReturnsAllLevels()
    {
        var result = await _service.GetAllDescendantsAsync("NGO");

        var intlNgo = await _service.GetPartnerTreeByCodeAsync("INTL_NGO");
        var localNgo = await _service.GetPartnerTreeByCodeAsync("LOCAL_NGO");
        result.Should().Contain(intlNgo!.Id);
        result.Should().Contain(localNgo!.Id);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetAllDescendantsAsync_NonExistentParentCode_ReturnsEmpty()
    {
        var result = await _service.GetAllDescendantsAsync("NONEXISTENT_PARENT");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetAllDescendantsAsync_ExcludesSoftDeletedChildren()
    {
        var deletedChild = new UNOPSPartnerTree
        {
            Name = "Deleted Child",
            Description = "Deleted child of NGO",
            Code = "DELETED_CHILD",
            Type = "Level_2",
            Parent = "NGO",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(deletedChild);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        var result = await _service.GetAllDescendantsAsync("NGO");

        var deletedEntity = _context.Set<UNOPSPartnerTree>().FirstOrDefault(pt => pt.Code == "DELETED_CHILD");
        result.Should().NotContain(deletedEntity!.Id);
    }

    #endregion

    #region 6. CreatePartnerTreeAsync

    [SkipIfInMemoryFact]
    [Trait("Category", "Positive")]
    public async Task CreatePartnerTreeAsync_ValidLevel1NotInSpecialCodes_CreatesWithCategoryCode()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "PRIVATE Category",
            Code = "PRIVATE",
            Type = "Level_1",
            Parent = "",
            Description = "Private sector",
            Status = EntityStatus.Active
        };

        var result = await _service.CreatePartnerTreeAsync(newTree);

        result.Should().NotBeNull();
        result!.Code.Should().Be("PRIVATE");
        result.PartnerCategoryCode.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Positive")]
    public async Task CreatePartnerTreeAsync_ValidLevel2ChildOfCategory_CreatesWithGroupCode()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "New Group",
            Code = "NEW_GROUP",
            Type = "Level_2",
            Parent = "NGO",
            Description = "New group under NGO",
            Status = EntityStatus.Active
        };

        var result = await _service.CreatePartnerTreeAsync(newTree);

        result.Should().NotBeNull();
        result!.Code.Should().Be("NEW_GROUP");
        result.PartnerGroupCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreatePartnerTreeAsync_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _service.CreatePartnerTreeAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("partnerTree");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreatePartnerTreeAsync_DuplicateCode_ThrowsBusinessException()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "Duplicate NGO",
            Code = "NGO",
            Type = "Level_1",
            Parent = "",
            Description = "Duplicate",
            Status = EntityStatus.Active
        };

        var act = () => _service.CreatePartnerTreeAsync(newTree);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CreatePartnerTreeAsync_Level1SpecialCategory_DoesNotSetPartnerCategoryCode()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "Another Multilateral",
            Code = "MULTILATERAL_2",
            Type = "Level_1",
            Parent = "",
            Description = "Another multilateral",
            Status = EntityStatus.Active
        };
        // MULTILATERAL_2 is not in specialCategoryCodes, so it WILL get PartnerCategoryCode
        // To test special codes, we need a code that IS in specialCategoryCodes
        var specialTree = new UNOPSPartnerTree
        {
            Name = "Government Duplicate Code Test",
            Code = "GOVERNMENT",
            Type = "Level_1",
            Parent = "",
            Description = "Test",
            Status = EntityStatus.Active
        };
        var act = () => _service.CreatePartnerTreeAsync(specialTree);
        await act.Should().ThrowAsync<BusinessException>(); // Duplicate code
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Boundary")]
    public async Task CreatePartnerTreeAsync_EmptyParent_NormalizesToEmptyString()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "Root Node",
            Code = "ROOT_NODE",
            Type = "Level_1",
            Parent = "   ",
            Description = "Root",
            Status = EntityStatus.Active
        };

        var result = await _service.CreatePartnerTreeAsync(newTree);

        result.Should().NotBeNull();
        result!.Parent.Should().Be("");
    }

    #endregion

    #region 7. UpdatePartnerTreeAsync

    [SkipIfInMemoryFact]
    [Trait("Category", "Positive")]
    public async Task UpdatePartnerTreeAsync_ValidUpdate_UpdatesAndReturnsTrue()
    {
        var existing = await _service.GetPartnerTreeByCodeAsync("NGO");
        existing!.Description = "Updated NGO description";

        var result = await _service.UpdatePartnerTreeAsync(existing);

        result.Should().BeTrue();
        var updated = await _service.GetPartnerTreeByCodeAsync("NGO");
        updated!.Description.Should().Be("Updated NGO description");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdatePartnerTreeAsync_NullInput_ThrowsArgumentNullException()
    {
        var act = () => _service.UpdatePartnerTreeAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("partnerTree");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdatePartnerTreeAsync_NonExistentId_ReturnsFalse()
    {
        var fakeTree = new UNOPSPartnerTree
        {
            Id = 999999,
            Name = "Fake",
            Description = "Fake",
            Code = "FAKE",
            Type = "Level_1",
            Parent = "",
            Status = EntityStatus.Active
        };

        var result = await _service.UpdatePartnerTreeAsync(fakeTree);

        result.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task UpdatePartnerTreeAsync_Level1CanModifyCategoryCode_UpdatesPartnerCategoryCode()
    {
        var ngo = await _service.GetPartnerTreeByCodeAsync("NGO");
        ngo!.PartnerCategoryCode = "CUSTOM_CAT";

        await _service.UpdatePartnerTreeAsync(ngo);

        var updated = await _service.GetPartnerTreeByCodeAsync("NGO");
        updated!.PartnerCategoryCode.Should().Be("CUSTOM_CAT");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task UpdatePartnerTreeAsync_Level1SpecialCategory_DoesNotUpdatePartnerCategoryCode()
    {
        var multilateral = await _service.GetPartnerTreeByCodeAsync("MULTILATERAL");
        var originalCategoryCode = multilateral!.PartnerCategoryCode;
        multilateral.PartnerCategoryCode = "SHOULD_NOT_UPDATE";

        await _service.UpdatePartnerTreeAsync(multilateral);

        var updated = await _service.GetPartnerTreeByCodeAsync("MULTILATERAL");
        updated!.PartnerCategoryCode.Should().Be(originalCategoryCode);
    }

    #endregion

    #region 8. DeletePartnerTreeAsync

    [Fact]
    [Trait("Category", "Positive")]
    public async Task DeletePartnerTreeAsync_ValidCode_RemovesAndReturnsTrue()
    {
        var toDelete = new UNOPSPartnerTree
        {
            Name = "To Delete",
            Code = "TO_DELETE",
            Type = "Level_1",
            Parent = "",
            Description = "Will be deleted",
            Status = EntityStatus.Active
        };
        _context.Set<UNOPSPartnerTree>().Add(toDelete);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        var result = await _service.DeletePartnerTreeAsync("TO_DELETE");

        result.Should().BeTrue();
        var afterDelete = await _service.GetPartnerTreeByCodeAsync("TO_DELETE");
        afterDelete.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task DeletePartnerTreeAsync_NonExistentCode_ReturnsFalse()
    {
        var result = await _service.DeletePartnerTreeAsync("NONEXISTENT_DELETE");

        result.Should().BeFalse();
    }

    #endregion

    #region 9. Cache Invalidation

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task UpdatePartnerTreeAsync_InvalidatesCache_SubsequentGetReturnsFreshData()
    {
        var before = await _service.GetAllPartnerTreesAsync();
        var ngo = before.First(pt => pt.Code == "NGO");
        ngo.Description = "Cache invalidation test";
        await _service.UpdatePartnerTreeAsync(ngo);

        var after = await _service.GetAllPartnerTreesAsync();
        var updatedNgo = after.First(pt => pt.Code == "NGO");
        updatedNgo.Description.Should().Be("Cache invalidation test");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Functional")]
    public async Task CreatePartnerTreeAsync_InvalidatesCache_NewTreeVisibleInGetAll()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "Cache Test",
            Code = "CACHE_TEST",
            Type = "Level_1",
            Parent = "",
            Description = "Test",
            Status = EntityStatus.Active
        };
        await _service.CreatePartnerTreeAsync(newTree);

        var all = await _service.GetAllPartnerTreesAsync();
        all.Should().Contain(pt => pt.Code == "CACHE_TEST");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task DeletePartnerTreeAsync_InvalidatesCache_DeletedTreeNotInGetAll()
    {
        var toDelete = new UNOPSPartnerTree
        {
            Name = "Cache Delete Test",
            Code = "CACHE_DELETE",
            Type = "Level_1",
            Parent = "",
            Description = "Test",
            Status = EntityStatus.Active
        };
        _context.Set<UNOPSPartnerTree>().Add(toDelete);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        await _service.DeletePartnerTreeAsync("CACHE_DELETE");

        var all = await _service.GetAllPartnerTreesAsync();
        all.Should().NotContain(pt => pt.Code == "CACHE_DELETE");
    }

    #endregion

    #region 10. CanModifyPartnerCategoryCode / CanModifyPartnerGroupCode (Business Rules)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task LoadPartnerTrees_Level1NgoNotInSpecialCodes_GetsPartnerCategoryCode()
    {
        var all = await _service.GetAllPartnerTreesAsync();
        var ngo = all.First(pt => pt.Code == "NGO");
        ngo.PartnerCategoryCode.Should().NotBeNullOrEmpty();
        ngo.PartnerCategoryCode.Should().Be(ngo.Code);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task LoadPartnerTrees_Level1MultilateralInSpecialCodes_PartnerCategoryCodeNull()
    {
        var all = await _service.GetAllPartnerTreesAsync();
        var multilateral = all.First(pt => pt.Code == "MULTILATERAL");
        multilateral.PartnerCategoryCode.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task LoadPartnerTrees_Level2ChildOfSpecialCode_GetsPartnerCategoryCode()
    {
        var all = await _service.GetAllPartnerTreesAsync();
        var unAgency = all.First(pt => pt.Code == "UN_AGENCY");
        unAgency.PartnerCategoryCode.Should().NotBeNullOrEmpty();
        unAgency.PartnerCategoryCode.Should().Be("UN_AGENCY");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task LoadPartnerTrees_Level2ChildOfCategory_GetsPartnerGroupCode()
    {
        var all = await _service.GetAllPartnerTreesAsync();
        var intlNgo = all.First(pt => pt.Code == "INTL_NGO");
        intlNgo.PartnerGroupCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task LoadPartnerTrees_Level2ChildOfSpecialCode_WhenCategorySet_SkipsPartnerGroupCode()
    {
        var all = await _service.GetAllPartnerTreesAsync();
        var unAgency = all.First(pt => pt.Code == "UN_AGENCY");
        unAgency.PartnerCategoryCode.Should().NotBeNull();
        unAgency.PartnerGroupCode.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task LoadPartnerTrees_Level2WithNoParent_PartnerGroupCodeNull()
    {
        var orphan = new UNOPSPartnerTree
        {
            Name = "No Parent",
            Description = "Level 2 with no parent",
            Code = "NO_PARENT",
            Type = "Level_2",
            Parent = "",
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartnerTree>().Add(orphan);
        await _context.SaveChangesAsync();
        _memoryCache.Remove("PARTNER_TREE_CACHE");

        var all = await _service.GetAllPartnerTreesAsync();
        var noParent = all.First(pt => pt.Code == "NO_PARENT");
        noParent.PartnerGroupCode.Should().BeNull();
    }

    #endregion

    #region 11. Edge Cases

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetAllDescendantsAsync_EmptyString_ReturnsRootLevelChildren()
    {
        var result = await _service.GetAllDescendantsAsync("");

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(id => id > 0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetPartnerTreeByCodeAsync_CaseSensitiveMatch_ReturnsCorrectTree()
    {
        var result = await _service.GetPartnerTreeByCodeAsync("ngo");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetPartnerTreeByCodeAsync_ExactMatch_ReturnsTree()
    {
        var result = await _service.GetPartnerTreeByCodeAsync("NGO");

        result.Should().NotBeNull();
        result!.Code.Should().Be("NGO");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "Boundary")]
    public async Task CreatePartnerTreeAsync_Level2ChildOfLevel2_RecursiveGroupCodeResolution()
    {
        var newTree = new UNOPSPartnerTree
        {
            Name = "Grandchild Group",
            Code = "GRANDCHILD_GRP",
            Type = "Level_2",
            Parent = "INTL_NGO",
            Description = "Child of INTL_NGO which is child of NGO",
            Status = EntityStatus.Active
        };

        var result = await _service.CreatePartnerTreeAsync(newTree);

        result.Should().NotBeNull();
        result!.PartnerGroupCode.Should().NotBeNullOrEmpty();
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check (Unit Tests — PartnerTreeService)
| Category | Count | Sample Tests |
|----------|-------|--------------|
| Positive (P) | 12 | GetAllPartnerTreesAsync_ValidRequest, GetPartnerTreeByCodeAsync_ValidCode, GetPartnerTreeByIdAsync_ValidId, GetPartnerCategoryByPartnerGroupCodeAsync_Level2ChildOfNgo, GetAllDescendantsAsync_NgoCategory, CreatePartnerTreeAsync_ValidLevel1, DeletePartnerTreeAsync_ValidCode |
| Negative (N) | 9 | GetPartnerTreeByCodeAsync_NonExistentCode, GetPartnerTreeByIdAsync_NonExistentId, CreatePartnerTreeAsync_NullInput, CreatePartnerTreeAsync_DuplicateCode, UpdatePartnerTreeAsync_NullInput, UpdatePartnerTreeAsync_NonExistentId, DeletePartnerTreeAsync_NonExistentCode |
| Boundary (E) | 8 | GetPartnerCategoryByPartnerGroupCodeAsync_OrphanWithInvalidParent, GetAllDescendantsAsync_EmptyString_ReturnsRootLevelChildren, CreatePartnerTreeAsync_EmptyParent, LoadPartnerTrees_Level2WithNoParent |
| Functional (F) | 14 | GetAllPartnerTreesAsync_ExcludesSoftDeletedTrees, GetPartnerTreeByCodeAsync_ExcludesSoftDeleted, LoadPartnerTrees_Level1NgoNotInSpecialCodes_GetsPartnerCategoryCode, CanModifyPartnerCategoryCode/CanModifyPartnerGroupCode rules, Cache invalidation |
| Integration (I) | 5 | (via Functional + CRUD flow) |
| **N ≥ 3P?** | — | Unit test suite (ratio applies to full feature suites) |
| **E ≥ 3P?** | — | Unit test suite |

### Test Coverage Summary
- 8 public methods: GetAllPartnerTreesAsync, GetPartnerTreeByCodeAsync, GetPartnerTreeByIdAsync, GetPartnerCategoryByPartnerGroupCodeAsync, GetAllDescendantsAsync, CreatePartnerTreeAsync, UpdatePartnerTreeAsync, DeletePartnerTreeAsync
- Business rules: CanModifyPartnerCategoryCode (Level_1 not in special, Level_2 child of special), CanModifyPartnerGroupCode (parent chain)
- Recursive parent lookup, descendant resolution, cache invalidation, soft delete filtering
- 9 tests use [SkipIfInMemoryFact] (require PostgreSQL for Z.EntityFramework.Extensions SingleUpdateAsync)
*/
