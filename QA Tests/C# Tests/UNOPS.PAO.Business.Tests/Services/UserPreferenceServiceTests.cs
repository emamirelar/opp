/**
 * @fileoverview Comprehensive tests for UserPreferenceService.
 * Tests GetDefaultOrgUnitIdAsync, UpdateDefaultOrgUnitAsync, GetUserPreferencesAsync,
 * UpdateUserPreferencesAsync, GetGlobalFiltersAsync, UpdateGlobalFiltersAsync, ResetGlobalFiltersAsync.
 * Uses InMemory database with UserResolverService.
 *
 * Ratio: P=2, N=6+, E=6+, F=6+, I=6+
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Mock-based tests for UserPreferenceService using InMemory database.
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class UserPreferenceServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly UserPreferenceService _service;
    private readonly UserResolverService<int> _userResolver;
    private readonly string _dbName;
    private readonly IDbContextTransaction? _transaction;

    public UserPreferenceServiceTests()
    {
        _dbName = $"UserPref_{Guid.NewGuid():N}";
        var options = TestEnvironment.CreateUNOPSDbContextOptions(_dbName);
        _userResolver = new UserResolverService<int>("test@example.com");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");
        _context = TestDbContextFactory.CreateUNOPS(options, _userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(_context);

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = _context.Database.BeginTransaction();
            _context.Set<UNOPS.PAO.Domain.Entities.UserPreference>().RemoveRange(
                _context.Set<UNOPS.PAO.Domain.Entities.UserPreference>());
            _context.SaveChanges();
        }

        _service = new UserPreferenceService(_context, _userResolver);
    }

    private static int UniqueId() => Random.Shared.Next(100_000, 999_999);

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }

    #region Positive (2)

    [Fact]
    public async Task GetDefaultOrgUnitIdAsync_WithPreference_ReturnsOrgUnitIdFromPreferences()
    {
        // Arrange
        var userId = UniqueId();
        var orgUnitId = 42;
        await SeedUserPreference(userId, orgUnitId);

        // Act
        var result = await _service.GetDefaultOrgUnitIdAsync(userId);

        // Assert
        result.Should().Be(orgUnitId);
    }

    [Fact]
    public async Task GetGlobalFiltersAsync_WithPreference_ReturnsFiltersWithOrgUnitName()
    {
        // Arrange
        var userId = UniqueId();
        var orgUnitId = await SeedOrganizationHierarchyAndGetId("Test Org Unit");
        await SeedUserPreference(userId, orgUnitId);

        // Act
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(orgUnitId);
        result.OrgUnitName.Should().Be("Test Org Unit");
    }

    #endregion

    #region Negative (6+)

    [Fact]
    public async Task GetDefaultOrgUnitIdAsync_UserWithNoPreferences_ReturnsNull()
    {
        // Arrange - no UserPreference, no UserProfile with matching email
        var userId = UniqueId();

        // Act
        var result = await _service.GetDefaultOrgUnitIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserPreferencesAsync_InvalidUserIdString_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserPreferencesAsync("not-a-number");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_InvalidUserId_DoesNothing()
    {
        // Arrange
        var prefs = new UserPreference { UserId = 0, GlobalFilters = new GlobalFilters { OrgUnitId = 5 } };
        var countBefore = await _context.UserPreferences.CountAsync();

        // Act
        await _service.UpdateUserPreferencesAsync("invalid", prefs);

        // Assert - invalid userId should not create any new preference
        var countAfter = await _context.UserPreferences.CountAsync();
        countAfter.Should().Be(countBefore);
    }

    [Fact]
    public async Task GetGlobalFiltersAsync_InvalidUserId_ReturnsEmptyFilters()
    {
        // Act
        var result = await _service.GetGlobalFiltersAsync("abc");

        // Assert
        result.Should().NotBeNull();
        result.OrgUnitId.Should().BeNull();
        result.OrgUnitName.Should().BeNull();
    }

    [Fact]
    public async Task UpdateGlobalFiltersAsync_InvalidUserId_DoesNothing()
    {
        // Arrange
        var filters = new GlobalFilters { OrgUnitId = 7 };
        var countBefore = await _context.UserPreferences.CountAsync();

        // Act
        await _service.UpdateGlobalFiltersAsync("xyz", filters);

        // Assert - invalid userId should not create any new preference
        var countAfter = await _context.UserPreferences.CountAsync();
        countAfter.Should().Be(countBefore);
    }

    [Fact]
    public async Task ResetGlobalFiltersAsync_UserWithNoPreferences_DoesNothing()
    {
        // Arrange - no preference for user
        var userId = UniqueId();

        // Act
        await _service.ResetGlobalFiltersAsync(userId.ToString());

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref.Should().BeNull();
    }

    [Fact]
    public async Task GetUserPreferencesAsync_EmptyUserId_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserPreferencesAsync("");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Edge/Boundary (6+)

    [Fact]
    [Trait("Defect", "DEF-224")]
    public async Task GetDefaultOrgUnitIdAsync_FallsBackToUserProfile_WhenNoPreference()
    {
        // Arrange - UserProfile with OrgUnit, no UserPreference
        var userId = UniqueId();
        var orgUnitCode = "FALLBACK_ORG";
        var orgUnitId = await SeedOrganizationHierarchyAndGetId(orgUnitCode);
        await SeedUserProfileWithOrgUnit(userId, orgUnitCode);

        // Act
        var result = await _service.GetDefaultOrgUnitIdAsync(userId);

        // Assert - falls back to UserProfile's OrgUnit lookup
        result.Should().Be(orgUnitId);
    }

    [Fact]
    public async Task UpdateDefaultOrgUnitAsync_CreatesNewPreference_WhenNoneExists()
    {
        // Arrange
        var userId = UniqueId();
        var orgUnitId = 15;
        await SeedUserProfile(userId);

        // Act
        await _service.UpdateDefaultOrgUnitAsync(userId, orgUnitId);

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref.Should().NotBeNull();
        pref!.GlobalFilters!.OrgUnitId.Should().Be(orgUnitId);
    }

    [Fact]
    [Trait("Defect", "DEF-223")]
    public async Task UpdateDefaultOrgUnitAsync_AutoCreatesUserProfile_WhenMissing()
    {
        // Arrange - no UserProfile for user, but AspNetUser must exist for FK
        var userId = UniqueId();
        var orgUnitId = 20;
        await EnsureAspNetUserAsync(userId);

        // Act
        await _service.UpdateDefaultOrgUnitAsync(userId, orgUnitId);

        // Assert - UserProfile was auto-created
        var profile = await _context.UserProfile.FirstOrDefaultAsync(up => up.UserId == userId);
        profile.Should().NotBeNull();
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_CreatesNew_WhenNoneExists()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserProfile(userId);
        var prefs = new UserPreference { GlobalFilters = new GlobalFilters { OrgUnitId = 25 } };

        // Act
        await _service.UpdateUserPreferencesAsync(userId.ToString(), prefs);

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref.Should().NotBeNull();
        pref!.GlobalFilters!.OrgUnitId.Should().Be(25);
    }

    [Fact]
    public async Task ResetGlobalFiltersAsync_SetsOrgUnitIdToNull()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 30);

        // Act
        await _service.ResetGlobalFiltersAsync(userId.ToString());

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref.Should().NotBeNull();
        pref!.GlobalFilters!.OrgUnitId.Should().BeNull();
    }

    [Fact]
    public async Task GetGlobalFiltersAsync_PopulatesOrgUnitName_FromOrganizationHierarchy()
    {
        // Arrange
        var userId = UniqueId();
        var orgUnitId = await SeedOrganizationHierarchyAndGetId("Hierarchy Populated Name");
        await SeedUserPreference(userId, orgUnitId);

        // Act
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.OrgUnitName.Should().Be("Hierarchy Populated Name");
    }

    [Fact]
    public async Task GetGlobalFiltersAsync_OrgUnitNotFound_ReturnsNullOrgUnitName()
    {
        // Arrange - preference references non-existent org unit
        var userId = UniqueId();
        await SeedUserProfile(userId);
        var pref = new UserPreference
        {
            UserId = userId,
            Name = $"UserPreferences_{userId}",
            GlobalFilters = new GlobalFilters { OrgUnitId = 99999 }
        };
        _context.UserPreferences.Add(pref);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.OrgUnitId.Should().Be(99999);
        result.OrgUnitName.Should().BeNull();
    }

    #endregion

    #region Functional (6+)

    [Fact]
    public async Task UpdateDefaultOrgUnitAsync_UpdatesExistingPreference()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 40);

        // Act
        await _service.UpdateDefaultOrgUnitAsync(userId, 41);

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref!.GlobalFilters!.OrgUnitId.Should().Be(41);
    }

    [Fact]
    public async Task UpdateGlobalFiltersAsync_UpdatesExistingFilters()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 42);
        var newFilters = new GlobalFilters { OrgUnitId = 43, RelatedToMe = true };

        // Act
        await _service.UpdateGlobalFiltersAsync(userId.ToString(), newFilters);

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref!.GlobalFilters!.OrgUnitId.Should().Be(43);
        pref.GlobalFilters.RelatedToMe.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_MarksGlobalFilterJsonAsModified()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 44);
        var updatedPrefs = new UserPreference
        {
            GlobalFilters = new GlobalFilters { OrgUnitId = 45 },
            AdditionalSettingsJson = "{\"key\":\"value\"}"
        };

        // Act
        await _service.UpdateUserPreferencesAsync(userId.ToString(), updatedPrefs);

        // Assert - verify the update persisted
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref!.GlobalFilters!.OrgUnitId.Should().Be(45);
        pref.AdditionalSettingsJson.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public async Task GetUserPreferencesAsync_ParsesStringUserIdCorrectly()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 46);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId.ToString());

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.GlobalFilters!.OrgUnitId.Should().Be(46);
    }

    [Fact]
    public async Task ResetGlobalFiltersAsync_PreservesOtherPreferenceData()
    {
        // Arrange - preference with AdditionalSettingsJson
        var userId = UniqueId();
        await SeedUserProfile(userId);
        var pref = new UserPreference
        {
            UserId = userId,
            Name = $"UserPreferences_{userId}",
            GlobalFilters = new GlobalFilters { OrgUnitId = 47 },
            AdditionalSettingsJson = "{\"preserved\":true}"
        };
        _context.UserPreferences.Add(pref);
        await _context.SaveChangesAsync();

        // Act
        await _service.ResetGlobalFiltersAsync(userId.ToString());

        // Assert - OrgUnitId reset, but AdditionalSettingsJson preserved (Reset only touches GlobalFilters)
        var after = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        after!.GlobalFilters!.OrgUnitId.Should().BeNull();
        // Note: ResetGlobalFiltersAsync replaces entire GlobalFilters - AdditionalSettingsJson is on UserPreference
        // The current implementation only sets GlobalFilters = new GlobalFilters { OrgUnitId = null }
        // So AdditionalSettingsJson on the entity is NOT touched - it stays. Good.
        after.AdditionalSettingsJson.Should().Be("{\"preserved\":true}");
    }

    [Fact]
    public async Task GetGlobalFiltersAsync_HandlesNullGlobalFilters()
    {
        // Arrange - UserPreference with null/empty GlobalFilterJson
        var userId = UniqueId();
        await SeedUserProfile(userId);
        var pref = new UserPreference
        {
            UserId = userId,
            Name = $"UserPreferences_{userId}",
            GlobalFilterJson = null
        };
        _context.UserPreferences.Add(pref);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.Should().NotBeNull();
        result.OrgUnitId.Should().BeNull();
        result.OrgUnitName.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_WithNullOrgUnitId_ShowsEverything()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 48);
        var prefs = new UserPreference { GlobalFilters = new GlobalFilters { OrgUnitId = null } };

        // Act
        await _service.UpdateUserPreferencesAsync(userId.ToString(), prefs);

        // Assert
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);
        pref!.GlobalFilters!.OrgUnitId.Should().BeNull();
    }

    #endregion

    #region Integration (6+)

    [Fact]
    public async Task FullCreateGetUpdateGet_Flow()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserProfile(userId);

        // Act 1: Create
        await _service.UpdateGlobalFiltersAsync(userId.ToString(), new GlobalFilters { OrgUnitId = 80 });

        // Act 2: Get
        var first = await _service.GetGlobalFiltersAsync(userId.ToString());
        first.OrgUnitId.Should().Be(80);

        // Act 3: Update
        await _service.UpdateGlobalFiltersAsync(userId.ToString(), new GlobalFilters { OrgUnitId = 81 });

        // Act 4: Get again
        var second = await _service.GetGlobalFiltersAsync(userId.ToString());
        second.OrgUnitId.Should().Be(81);
    }

    [Fact]
    public async Task CreatePreference_UpdateOrgUnit_Verify()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserProfile(userId);

        // Act
        await _service.UpdateDefaultOrgUnitAsync(userId, 82);
        var result = await _service.GetDefaultOrgUnitIdAsync(userId);

        // Assert
        result.Should().Be(82);
    }

    [Fact]
    public async Task SetGlobalFilters_GetGlobalFilters_VerifyOrgUnitName()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserProfile(userId);
        var orgUnitId = await SeedOrganizationHierarchyAndGetId("Integration Org Name");
        await _service.UpdateGlobalFiltersAsync(userId.ToString(), new GlobalFilters { OrgUnitId = orgUnitId });

        // Act
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.OrgUnitId.Should().Be(orgUnitId);
        result.OrgUnitName.Should().Be("Integration Org Name");
    }

    [Fact]
    public async Task ResetGlobalFilters_VerifyNullOrgUnitId()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 84);

        // Act
        await _service.ResetGlobalFiltersAsync(userId.ToString());
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.OrgUnitId.Should().BeNull();
    }

    [Fact]
    public async Task MultipleUsers_SeparatePreferences()
    {
        // Arrange
        var userId1 = UniqueId();
        var userId2 = UniqueId();
        var userId3 = UniqueId();
        await SeedUserPreference(userId1, 91);
        await SeedUserPreference(userId2, 92);
        await SeedUserPreference(userId3, 93);

        // Act
        var r1 = await _service.GetDefaultOrgUnitIdAsync(userId1);
        var r2 = await _service.GetDefaultOrgUnitIdAsync(userId2);
        var r3 = await _service.GetDefaultOrgUnitIdAsync(userId3);

        // Assert
        r1.Should().Be(91);
        r2.Should().Be(92);
        r3.Should().Be(93);
    }

    [Fact]
    public async Task UpdatePreferences_WithNullOrgUnitId_ShowEverything()
    {
        // Arrange
        var userId = UniqueId();
        await SeedUserPreference(userId, 85);

        // Act - user chooses to see everything (null org unit)
        await _service.UpdateGlobalFiltersAsync(userId.ToString(), new GlobalFilters { OrgUnitId = null });
        var result = await _service.GetGlobalFiltersAsync(userId.ToString());

        // Assert
        result.OrgUnitId.Should().BeNull();
    }

    #endregion

    #region Helpers

    private async Task EnsureAspNetUserAsync(int userId)
    {
        if (!TestEnvironment.UsePostgreSQL)
            return;

        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"AspNetUsers\" (\"Id\", \"Email\", \"NormalizedEmail\", \"UserName\", \"NormalizedUserName\", " +
            "\"EmailConfirmed\", \"PasswordHash\", \"SecurityStamp\", \"ConcurrencyStamp\", " +
            "\"PhoneNumberConfirmed\", \"TwoFactorEnabled\", \"LockoutEnabled\", \"AccessFailedCount\", \"IsInternal\") " +
            "SELECT {0}, {1}, {2}, {1}, {2}, " +
            "true, 'x', 'x', 'x', false, false, true, 0, true " +
            "WHERE NOT EXISTS (SELECT 1 FROM \"AspNetUsers\" WHERE \"Id\" = {0})",
            userId, $"testuser_{userId}@test.local", $"TESTUSER_{userId}@TEST.LOCAL");
    }

    private async Task SeedUserPreference(int userId, int orgUnitId)
    {
        await SeedUserProfile(userId);
        var pref = new UserPreference
        {
            UserId = userId,
            Name = $"UserPreferences_{userId}",
            GlobalFilters = new GlobalFilters { OrgUnitId = orgUnitId }
        };
        _context.UserPreferences.Add(pref);
        await _context.SaveChangesAsync();
    }

    private async Task SeedUserProfile(int userId)
    {
        await EnsureAspNetUserAsync(userId);
        if (TestEnvironment.UsePostgreSQL)
        {
            var now = DateTime.UtcNow;
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"UserProfile\" (\"Id\", \"UserId\", \"FirstName\", \"LastName\", \"Name\", \"UserEmail\", " +
                "\"Status\", \"CreatedBy\", \"CreatedDate\", \"LastModifiedBy\", \"LastModifiedDate\", " +
                "\"IsDeleted\", \"DeletedBy\", \"WorkflowStatus\") " +
                "SELECT (SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"UserProfile\"), " +
                "{0}, 'Test', 'User', {1}, {2}, 1, {0}, {3}, 0, {3}, false, 0, 0 " +
                "WHERE NOT EXISTS (SELECT 1 FROM \"UserProfile\" WHERE \"UserId\" = {0})",
                userId, $"Test User {userId}", $"testuser_{userId}@test.local", now);
        }
        else
        {
            var profile = new UserProfile
            {
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                UserEmail = $"testuser_{userId}@test.local",
                Status = EntityStatus.Active,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = userId,
                IsDeleted = false
            };
            _context.UserProfile.Add(profile);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedUserProfileWithOrgUnit(int userId, string orgUnitCode)
    {
        await EnsureAspNetUserAsync(userId);
        if (TestEnvironment.UsePostgreSQL)
        {
            var now = DateTime.UtcNow;
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO \"UserProfile\" (\"Id\", \"UserId\", \"FirstName\", \"LastName\", \"Name\", \"UserEmail\", " +
                "\"OrgUnit\", \"Status\", \"CreatedBy\", \"CreatedDate\", \"LastModifiedBy\", \"LastModifiedDate\", " +
                "\"IsDeleted\", \"DeletedBy\", \"WorkflowStatus\") " +
                "SELECT (SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"UserProfile\"), " +
                "{0}, 'Test', 'User', {1}, {2}, {3}, 1, {0}, {4}, 0, {4}, false, 0, 0 " +
                "WHERE NOT EXISTS (SELECT 1 FROM \"UserProfile\" WHERE \"UserId\" = {0})",
                userId, $"Test User {userId}", $"testuser_{userId}@test.local", orgUnitCode, now);
        }
        else
        {
            var profile = new UserProfile
            {
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                UserEmail = $"testuser_{userId}@test.local",
                OrgUnit = orgUnitCode,
                Status = EntityStatus.Active,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = userId,
                IsDeleted = false
            };
            _context.UserProfile.Add(profile);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds OrganizationHierarchy and returns its Id (avoids explicit Id when EF auto-generates).
    /// </summary>
    private async Task<int> SeedOrganizationHierarchyAndGetId(string name)
    {
        var code = name.Replace(" ", "_");
        var org = new OrganizationHierarchy
        {
            Code = code,
            Name = name,
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(org);
        await _context.SaveChangesAsync();
        return org.Id;
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | GetDefaultOrgUnitIdAsync_WithPreference_ReturnsOrgUnitIdFromPreferences, GetGlobalFiltersAsync_WithPreference_ReturnsFiltersWithOrgUnitName |
| Negative (N) | 7 | GetDefaultOrgUnitIdAsync_UserWithNoPreferences_ReturnsNull, GetUserPreferencesAsync_InvalidUserIdString_ReturnsNull, UpdateUserPreferencesAsync_InvalidUserId_DoesNothing, GetGlobalFiltersAsync_InvalidUserId_ReturnsEmptyFilters, UpdateGlobalFiltersAsync_InvalidUserId_DoesNothing, ResetGlobalFiltersAsync_UserWithNoPreferences_DoesNothing, GetUserPreferencesAsync_EmptyUserId_ReturnsNull |
| Edge/Boundary (E) | 7 | GetDefaultOrgUnitIdAsync_FallsBackToUserProfile_WhenNoPreference, UpdateDefaultOrgUnitAsync_CreatesNewPreference_WhenNoneExists, UpdateDefaultOrgUnitAsync_AutoCreatesUserProfile_WhenMissing, UpdateUserPreferencesAsync_CreatesNew_WhenNoneExists, ResetGlobalFiltersAsync_SetsOrgUnitIdToNull, GetGlobalFiltersAsync_PopulatesOrgUnitName_FromOrganizationHierarchy, GetGlobalFiltersAsync_OrgUnitNotFound_ReturnsNullOrgUnitName |
| Functional (F) | 7 | UpdateDefaultOrgUnitAsync_UpdatesExistingPreference, UpdateGlobalFiltersAsync_UpdatesExistingFilters, UpdateUserPreferencesAsync_MarksGlobalFilterJsonAsModified, GetUserPreferencesAsync_ParsesStringUserIdCorrectly, ResetGlobalFiltersAsync_PreservesOtherPreferenceData, GetGlobalFiltersAsync_HandlesNullGlobalFilters, UpdateUserPreferencesAsync_WithNullOrgUnitId_ShowsEverything |
| Integration (I) | 6 | FullCreateGetUpdateGet_Flow, CreatePreference_UpdateOrgUnit_Verify, SetGlobalFilters_GetGlobalFilters_VerifyOrgUnitName, ResetGlobalFilters_VerifyNullOrgUnitId, MultipleUsers_SeparatePreferences, UpdatePreferences_WithNullOrgUnitId_ShowEverything |
| **N ≥ 3P?** | ✅ | 7 >= 6 |
| **E ≥ 3P?** | ✅ | 7 >= 6 |
| **F ≥ 3P?** | ✅ | 7 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
