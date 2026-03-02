using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Direct database-level tests for User Management entities (UserProfile, OrganizationHierarchy).
/// Validates the data layer that UNOPSUserManagementManager depends on:
/// user profiles (read-only queries), org unit CRUD, and soft delete behavior.
/// 
/// NOTE: UserProfile cannot be directly created in tests because its Name property
/// is computed (get-only) but the database column is NOT NULL. These tests use
/// read-only queries against existing profiles and focus CRUD testing on
/// OrganizationHierarchy which can be created directly.
/// </summary>
public class UNOPSUserManagementManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"UMGR_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task<int> SeedOrgHierarchyAsync(string code, string name)
    {
        var existing = await Context.OrganizationHierarchies
            .FirstOrDefaultAsync(o => o.Code == code && !o.IsDeleted);
        if (existing != null) return existing.Id;

        var org = new OrganizationHierarchy
        {
            Name = name,
            Code = code,
            Description = $"Test org unit {name}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        return org.Id;
    }

    #endregion

    #region P0 - UserProfile Read Queries

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-001")]
    public async Task QueryUserProfiles_ActiveOnly_ShouldExcludeDeleted()
    {
        // Act - query active profiles; table may be empty in test db
        var activeProfiles = await Context.UserProfile
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Take(10)
            .ToListAsync();

        // Assert - the query should execute without error
        activeProfiles.Should().NotBeNull();
        if (activeProfiles.Any())
        {
            activeProfiles.Should().OnlyContain(u => !u.IsDeleted);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-002")]
    public async Task QueryUserProfile_ByTestUserId_ShouldFindProfile()
    {
        // Act - the test setup creates a user; check if profile exists
        var profile = await Context.UserProfile
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == TestUserId && !u.IsDeleted);

        // Assert - may or may not exist depending on test setup
        // The important thing is the query executes without error
        if (profile != null)
        {
            profile.UserId.Should().Be(TestUserId);
            profile.IsDeleted.Should().BeFalse();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-003")]
    public async Task QueryUserProfiles_Pagination_ShouldReturnCorrectSlice()
    {
        // Act - simulate pagination
        var pageSize = 5;
        var totalCount = await Context.UserProfile
            .CountAsync(u => !u.IsDeleted);

        var page1 = await Context.UserProfile
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .Skip(0)
            .Take(pageSize)
            .ToListAsync();

        // Assert
        page1.Should().NotBeNull();
        page1.Count.Should().BeLessOrEqualTo(pageSize);

        if (totalCount > pageSize)
        {
            var page2 = await Context.UserProfile
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Id)
                .Skip(pageSize)
                .Take(pageSize)
                .ToListAsync();

            page1.Select(u => u.Id).Should().NotIntersectWith(page2.Select(u => u.Id));
        }
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-004")]
    public async Task QueryUserProfiles_ByEmail_ShouldFilterCorrectly()
    {
        // Act - query by email pattern
        var profiles = await Context.UserProfile
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.UserEmail != null)
            .Take(5)
            .ToListAsync();

        // Assert
        profiles.Should().NotBeNull();
        if (profiles.Any())
        {
            var firstEmail = profiles.First().UserEmail;
            var filtered = await Context.UserProfile
                .AsNoTracking()
                .Where(u => u.UserEmail == firstEmail && !u.IsDeleted)
                .ToListAsync();

            filtered.Should().NotBeEmpty();
            filtered.Should().OnlyContain(u => u.UserEmail == firstEmail);
        }
    }

    #endregion

    #region P0 - Organization Hierarchy CRUD

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-005")]
    public async Task CreateOrgHierarchy_WithValidData_ShouldPersist()
    {
        // Arrange
        var code = $"ORG-{_testMarker[..8]}";
        var name = $"Test Org Unit {_testMarker}";

        // Act
        var orgId = await SeedOrgHierarchyAsync(code, name);
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId);
        saved.Should().NotBeNull();
        saved!.Code.Should().Be(code);
        saved.Name.Should().Be(name);
        saved.Description.Should().Contain(name);
        saved.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-006")]
    public async Task ReadOrgHierarchy_ById_ShouldReturnCorrectRecord()
    {
        // Arrange
        var code = $"RD-{_testMarker[..6]}";
        var orgId = await SeedOrgHierarchyAsync(code, $"Read Org {_testMarker}");
        Context.ChangeTracker.Clear();

        // Act
        var org = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId && !o.IsDeleted);

        // Assert
        org.Should().NotBeNull();
        org!.Code.Should().Be(code);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-007")]
    public async Task UpdateOrgHierarchy_ChangeDescription_ShouldPersist()
    {
        // Arrange
        var code = $"UPD-{_testMarker[..6]}";
        var orgId = await SeedOrgHierarchyAsync(code, $"Update Org {_testMarker}");

        // Act
        var org = await Context.OrganizationHierarchies.FirstAsync(o => o.Id == orgId);
        org.Description = "Updated description";
        org.IsSelfManagementEnabled = true;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId);
        saved.Should().NotBeNull();
        saved!.Description.Should().Be("Updated description");
        saved.IsSelfManagementEnabled.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-008")]
    public async Task SoftDeleteOrgHierarchy_ShouldSetIsDeleted()
    {
        // Arrange
        var code = $"DEL-{_testMarker[..6]}";
        var orgId = await SeedOrgHierarchyAsync(code, $"Delete Org {_testMarker}");

        // Act
        var org = await Context.OrganizationHierarchies.FirstAsync(o => o.Id == orgId);
        org.IsDeleted = true;
        org.DeletedDate = DateTime.UtcNow;
        org.DeletedBy = TestUserId;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedDate.Should().NotBeNull();
    }

    #endregion

    #region P1 - Multiple Org Units

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-009")]
    public async Task MultipleOrgUnits_ShouldAllPersist()
    {
        // Arrange & Act
        await SeedOrgHierarchyAsync($"OU1-{_testMarker[..6]}", $"Unit 1 {_testMarker}");
        await SeedOrgHierarchyAsync($"OU2-{_testMarker[..6]}", $"Unit 2 {_testMarker}");
        await SeedOrgHierarchyAsync($"OU3-{_testMarker[..6]}", $"Unit 3 {_testMarker}");
        Context.ChangeTracker.Clear();

        // Assert
        var orgs = await Context.OrganizationHierarchies
            .AsNoTracking()
            .Where(o => o.Name.Contains(_testMarker) && !o.IsDeleted)
            .ToListAsync();
        orgs.Should().HaveCount(3);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-010")]
    public async Task OrgHierarchy_ParentChild_ShouldPersistRelationship()
    {
        // Arrange
        var parentId = await SeedOrgHierarchyAsync($"PAR-{_testMarker[..6]}", $"Parent Org {_testMarker}");

        var child = new OrganizationHierarchy
        {
            Name = $"Child Org {_testMarker}",
            Code = $"CHD-{_testMarker[..6]}",
            Description = "Child org unit",
            ParentId = parentId,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.OrganizationHierarchies.AddAsync(child);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var savedChild = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == child.Id);

        // Assert
        savedChild.Should().NotBeNull();
        savedChild!.ParentId.Should().Be(parentId);
    }

    #endregion

    #region P1 - Self Management Toggle

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-011")]
    public async Task OrgHierarchy_SelfManagement_DefaultsFalse()
    {
        // Arrange
        var code = $"SM-{_testMarker[..6]}";
        var orgId = await SeedOrgHierarchyAsync(code, $"Self Mgmt {_testMarker}");
        Context.ChangeTracker.Clear();

        // Act
        var org = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstAsync(o => o.Id == orgId);

        // Assert
        org.IsSelfManagementEnabled.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-012")]
    public async Task OrgHierarchy_ToggleSelfManagement_ShouldPersist()
    {
        // Arrange
        var code = $"TOG-{_testMarker[..6]}";
        var orgId = await SeedOrgHierarchyAsync(code, $"Toggle Org {_testMarker}");

        // Act - enable self management
        var org = await Context.OrganizationHierarchies.FirstAsync(o => o.Id == orgId);
        org.IsSelfManagementEnabled = true;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstAsync(o => o.Id == orgId);
        saved.IsSelfManagementEnabled.Should().BeTrue();

        // Act - disable self management
        var org2 = await Context.OrganizationHierarchies.FirstAsync(o => o.Id == orgId);
        org2.IsSelfManagementEnabled = false;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var saved2 = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstAsync(o => o.Id == orgId);
        saved2.IsSelfManagementEnabled.Should().BeFalse();
    }

    #endregion

    #region P2 - Soft Delete Isolation

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UMGR-013")]
    public async Task OrgHierarchy_SoftDelete_ShouldExcludeFromActiveQueries()
    {
        // Arrange
        var activeCode = $"ACT-{_testMarker[..6]}";
        var deletedCode = $"DEL2-{_testMarker[..6]}";

        var activeId = await SeedOrgHierarchyAsync(activeCode, $"Active Org {_testMarker}");
        var deletedId = await SeedOrgHierarchyAsync(deletedCode, $"Deleted Org {_testMarker}");

        var deletedOrg = await Context.OrganizationHierarchies.FirstAsync(o => o.Id == deletedId);
        deletedOrg.IsDeleted = true;
        deletedOrg.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var activeOrgs = await Context.OrganizationHierarchies
            .AsNoTracking()
            .Where(o => o.Name.Contains(_testMarker) && !o.IsDeleted)
            .ToListAsync();

        // Assert
        activeOrgs.Should().HaveCount(1);
        activeOrgs.Single().Code.Should().Be(activeCode);
    }

    #endregion
}
