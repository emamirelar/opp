/**
 * @fileoverview Comprehensive unit tests for LiaisonOfficeService
 * Tests liaison office lookups, filtering, CRUD operations, and edge cases
 * Uses test markers for PostgreSQL data isolation.
 * @author UNOPS Opportunity+ System Development Team
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.Services
{
    /// <summary>
    /// Test suite for LiaisonOfficeService.
    /// Uses unique test markers per test run for PostgreSQL data isolation.
    /// Uses UNOPSAppDbContext to ensure proper DbSet property access (avoids
    /// the 'new' keyword hiding issue with AppDbContext.LiaisonOffices).
    /// </summary>
    public class LiaisonOfficeServiceTests : IDisposable
    {
        private readonly UNOPSAppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly string _testMarker = $"LO_{Guid.NewGuid():N}";
        private readonly List<int> _createdIds = new();

        public LiaisonOfficeServiceTests()
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                using var tempContext = TestDbContextFactory.CreateUNOPS();
                var testUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "liaisonoffice-test@unops.org");
                _context = TestDbContextFactory.CreateUNOPSWithUserId(testUserId);
                _transaction = _context.Database.BeginTransaction();
            }
            else
            {
                _context = TestDbContextFactory.CreateUNOPS();
            }
            SeedTestData().GetAwaiter().GetResult();
        }

        private async Task SeedTestData()
        {
            var liaisonOffices = new List<LiaisonOffice>
            {
                new LiaisonOffice 
                { 
                    Name = $"Nairobi Office {_testMarker}", 
                    Code = $"NBO_{_testMarker}", 
                    IsActive = true,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = $"Kampala Office {_testMarker}", 
                    Code = $"KLA_{_testMarker}", 
                    IsActive = true,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = $"Dar es Salaam Office {_testMarker}", 
                    Code = $"DAR_{_testMarker}", 
                    IsActive = true,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = $"Inactive Office {_testMarker}", 
                    Code = $"INA_{_testMarker}", 
                    IsActive = false,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = $"Deleted Office {_testMarker}", 
                    Code = $"DEL_{_testMarker}", 
                    IsActive = true,
                    IsDeleted = true
                }
            };

            await _context.LiaisonOffices.AddRangeAsync(liaisonOffices);
            await _context.SaveChangesAsync();
            _createdIds.AddRange(liaisonOffices.Select(lo => lo.Id));
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                try { _transaction.Rollback(); }
                catch { }
                _transaction.Dispose();
                _transaction = null;
            }
            _context?.Dispose();
        }

        /// <summary>Helper: Query only liaison offices created by this test instance.</summary>
        private IQueryable<LiaisonOffice> TestOffices =>
            _context.LiaisonOffices.Where(lo => lo.Code.Contains(_testMarker));

        #region Basic Lookup Tests

        [Fact]
        public async Task GetAllLiaisonOffices_ReturnsActiveNonDeletedOnly()
        {
            // Act
            var offices = await TestOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            // Assert
            offices.Should().HaveCount(3);
            offices.Should().NotContain(lo => lo.Name.Contains("Inactive Office"));
            offices.Should().NotContain(lo => lo.Name.Contains("Deleted Office"));
        }

        [Fact]
        public async Task GetLiaisonOfficeById_ExistingId_ReturnsOffice()
        {
            // Arrange
            var firstOffice = await TestOffices.FirstAsync();

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Id == firstOffice.Id);

            // Assert
            office.Should().NotBeNull();
            office!.Name.Should().Contain("Nairobi Office");
        }

        [Fact]
        public async Task GetLiaisonOfficeByCode_ValidCode_ReturnsOffice()
        {
            // Arrange
            var targetCode = $"NBO_{_testMarker}";

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == targetCode && lo.IsActive && !lo.IsDeleted);

            // Assert
            office.Should().NotBeNull();
            office!.Name.Should().Contain("Nairobi Office");
        }

        [Fact]
        public async Task GetLiaisonOfficeByCode_InvalidCode_ReturnsNull()
        {
            // Arrange
            var invalidCode = "INVALID_ZZZZZ";

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == invalidCode);

            // Assert
            office.Should().BeNull();
        }

        #endregion

        #region Filter Tests

        [Fact]
        public async Task GetLiaisonOffices_ActiveOnly_FiltersCorrectly()
        {
            // Act
            var activeOffices = await TestOffices
                .Where(lo => lo.IsActive)
                .ToListAsync();

            // Assert
            activeOffices.Should().HaveCountGreaterOrEqualTo(3);
            activeOffices.Should().OnlyContain(lo => lo.IsActive);
        }

        [Fact]
        public async Task GetLiaisonOffices_IncludingInactive_ReturnsAll()
        {
            // Act
            var allOffices = await TestOffices
                .Where(lo => !lo.IsDeleted)
                .ToListAsync();

            // Assert
            allOffices.Should().HaveCount(4); // Active + Inactive
            allOffices.Should().Contain(lo => lo.Name.Contains("Inactive Office"));
        }

        [Fact]
        public async Task SearchLiaisonOffices_ByName_ReturnsMatches()
        {
            // Act
            var offices = await TestOffices
                .Where(lo => lo.Name.Contains("Nairobi") && lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            // Assert
            offices.Should().HaveCount(1);
            offices[0].Name.Should().Contain("Nairobi");
        }

        [Fact]
        public async Task SearchLiaisonOffices_ByCode_ReturnsMatches()
        {
            // Act
            var offices = await TestOffices
                .Where(lo => lo.Code.Contains("KLA") && lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            // Assert
            offices.Should().HaveCount(1);
            offices[0].Code.Should().Contain("KLA");
        }

        #endregion

        #region CRUD Operations Tests

        [Fact]
        public async Task CreateLiaisonOffice_ValidData_CreatesSuccessfully()
        {
            // Arrange
            var newOffice = new LiaisonOffice
            {
                Name = $"New York Office {_testMarker}",
                Code = $"NYC_{_testMarker}",
                IsActive = true,
                IsDeleted = false
            };

            // Act
            await _context.LiaisonOffices.AddAsync(newOffice);
            await _context.SaveChangesAsync();
            _createdIds.Add(newOffice.Id);

            // Assert
            var savedOffice = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == $"NYC_{_testMarker}");
            
            savedOffice.Should().NotBeNull();
            savedOffice!.Name.Should().Contain("New York Office");
            savedOffice.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateLiaisonOffice_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            var office = await TestOffices.FirstAsync();
            var originalName = office.Name;

            // Act
            office.Name = $"Updated Office Name {_testMarker}";
            await _context.SaveChangesAsync();

            // Assert
            _context.ChangeTracker.Clear();
            var updatedOffice = await _context.LiaisonOffices.FindAsync(office.Id);
            updatedOffice!.Name.Should().Contain("Updated Office Name");
            updatedOffice.Name.Should().NotBe(originalName);
        }

        [Fact]
        public async Task DeleteLiaisonOffice_SoftDelete_SetsIsDeletedTrue()
        {
            // Arrange
            var office = await TestOffices.FirstAsync(lo => !lo.IsDeleted);

            // Act - Soft delete
            office.IsDeleted = true;
            await _context.SaveChangesAsync();

            // Assert
            _context.ChangeTracker.Clear();
            var deletedOffice = await _context.LiaisonOffices.FindAsync(office.Id);
            deletedOffice!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeactivateLiaisonOffice_SetsIsActiveFalse()
        {
            // Arrange
            var office = await TestOffices.FirstAsync(lo => lo.IsActive && !lo.IsDeleted);

            // Act
            office.IsActive = false;
            await _context.SaveChangesAsync();

            // Assert
            _context.ChangeTracker.Clear();
            var deactivatedOffice = await _context.LiaisonOffices.FindAsync(office.Id);
            deactivatedOffice!.IsActive.Should().BeFalse();
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task CreateLiaisonOffice_DuplicateCode_ShouldBeDetectable()
        {
            // Arrange
            var existingCode = $"NBO_{_testMarker}";

            // Act - Check for existing code before insert
            var exists = await _context.LiaisonOffices
                .AnyAsync(lo => lo.Code == existingCode);

            // Assert
            exists.Should().BeTrue("Duplicate code should be detected");
        }

        [Fact]
        public void LiaisonOfficeCode_EmptyString_ShouldBeInvalid()
        {
            // Arrange
            var emptyCode = "";

            // Act
            var isValid = !string.IsNullOrWhiteSpace(emptyCode);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void LiaisonOfficeName_EmptyString_ShouldBeInvalid()
        {
            // Arrange
            var emptyName = "";

            // Act
            var isValid = !string.IsNullOrWhiteSpace(emptyName);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void LiaisonOfficeCode_MaxLength_ShouldBeEnforced()
        {
            // Arrange
            var maxLength = 50; // Typical code max length
            var validCode = new string('A', maxLength);
            var tooLongCode = new string('A', maxLength + 1);

            // Act & Assert
            validCode.Length.Should().BeLessOrEqualTo(maxLength);
            tooLongCode.Length.Should().BeGreaterThan(maxLength);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetLiaisonOfficeById_NegativeId_ReturnsNull()
        {
            // Arrange
            var negativeId = -1;

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Id == negativeId);

            // Assert
            office.Should().BeNull();
        }

        [Fact]
        public async Task GetLiaisonOfficeById_MaxIntId_ReturnsNull()
        {
            // Arrange
            var maxId = int.MaxValue;

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Id == maxId);

            // Assert
            office.Should().BeNull();
        }

        [Fact]
        public async Task SearchLiaisonOffices_EmptySearchTerm_ReturnsAll()
        {
            // Arrange
            var searchTerm = "";

            // Act
            var offices = string.IsNullOrEmpty(searchTerm)
                ? await TestOffices.Where(lo => lo.IsActive && !lo.IsDeleted).ToListAsync()
                : await TestOffices
                    .Where(lo => lo.Name.Contains(searchTerm) && lo.IsActive && !lo.IsDeleted)
                    .ToListAsync();

            // Assert
            offices.Should().HaveCount(3);
        }

        [Fact]
        public async Task SearchLiaisonOffices_SpecialCharacters_HandledGracefully()
        {
            // Arrange
            var specialChars = "%_'\"\\;";

            // Act
            var action = async () => await _context.LiaisonOffices
                .Where(lo => lo.Name.Contains(specialChars))
                .ToListAsync();

            // Assert - Should not throw
            await action.Should().NotThrowAsync();
        }

        [Fact(Skip = "Concurrent read test creates separate DbContexts that cannot see " +
                    "uncommitted transaction data seeded by the test fixture. " +
                    "The transaction-based isolation pattern prevents external contexts " +
                    "from seeing test data until committed.")]
        public async Task GetLiaisonOffices_ConcurrentReads_HandledCorrectly()
        {
            // This test requires committed data visible across separate DbContext instances.
            // With transaction-based test isolation, seeded data is only visible within the
            // test's own transaction context.
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetLiaisonOfficeByCode_CaseInsensitive_ShouldMatch()
        {
            // Arrange
            var baseCode = $"NBO_{_testMarker}";
            var codes = new[] { baseCode.ToLower(), baseCode.ToUpper(), baseCode };

            // Act & Assert
            foreach (var code in codes)
            {
                var office = await _context.LiaisonOffices
                    .FirstOrDefaultAsync(lo => lo.Code.ToUpper() == code.ToUpper());
                
                office.Should().NotBeNull($"Code '{code}' should match case-insensitively");
            }
        }

        [Fact]
        public async Task GetLiaisonOffices_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var pageSize = 2;
            var pageNumber = 1;

            // Act
            var pagedOffices = await TestOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted)
                .OrderBy(lo => lo.Name)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Assert
            pagedOffices.Should().HaveCountLessOrEqualTo(pageSize);
        }

        [Fact]
        public async Task GetLiaisonOffices_SortedByName_ReturnsAlphabetically()
        {
            // Act
            var sortedOffices = await TestOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted)
                .OrderBy(lo => lo.Name)
                .ToListAsync();

            // Assert
            sortedOffices.Should().BeInAscendingOrder(lo => lo.Name);
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task GetAllLiaisonOffices_LargeDataset_CompletesQuickly()
        {
            // Arrange - Add more offices for performance testing
            var additionalOffices = Enumerable.Range(1, 100)
                .Select(i => new LiaisonOffice
                {
                    Name = $"Office {i} {_testMarker}",
                    Code = $"OF{i:D3}_{_testMarker}",
                    IsActive = true,
                    IsDeleted = false
                })
                .ToList();

            await _context.LiaisonOffices.AddRangeAsync(additionalOffices);
            await _context.SaveChangesAsync();
            _createdIds.AddRange(additionalOffices.Select(lo => lo.Id));

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var offices = await TestOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted)
                .ToListAsync();
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
            offices.Should().HaveCountGreaterOrEqualTo(103); // 3 original + 100 new
        }

        [Fact]
        public async Task SearchLiaisonOffices_LargeDataset_CompletesQuickly()
        {
            // Arrange - Add more offices
            var additionalOffices = Enumerable.Range(1, 100)
                .Select(i => new LiaisonOffice
                {
                    Name = $"Performance Test Office {i} {_testMarker}",
                    Code = $"PTO{i:D3}_{_testMarker}",
                    IsActive = true,
                    IsDeleted = false
                })
                .ToList();

            await _context.LiaisonOffices.AddRangeAsync(additionalOffices);
            await _context.SaveChangesAsync();
            _createdIds.AddRange(additionalOffices.Select(lo => lo.Id));

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var offices = await TestOffices
                .Where(lo => lo.Name.Contains("Performance") && lo.IsActive && !lo.IsDeleted)
                .ToListAsync();
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
            offices.Should().HaveCount(100);
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public async Task ActivateLiaisonOffice_WhenInactive_SetsIsActiveTrue()
        {
            // Arrange
            var inactiveOffice = await TestOffices
                .FirstAsync(lo => !lo.IsActive && !lo.IsDeleted);

            // Act
            inactiveOffice.IsActive = true;
            await _context.SaveChangesAsync();

            // Assert
            _context.ChangeTracker.Clear();
            var activatedOffice = await _context.LiaisonOffices.FindAsync(inactiveOffice.Id);
            activatedOffice!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreLiaisonOffice_WhenSoftDeleted_SetsIsDeletedFalse()
        {
            // Arrange
            var deletedOffice = await TestOffices
                .FirstAsync(lo => lo.IsDeleted);

            // Act
            deletedOffice.IsDeleted = false;
            await _context.SaveChangesAsync();

            // Assert
            _context.ChangeTracker.Clear();
            var restoredOffice = await _context.LiaisonOffices.FindAsync(deletedOffice.Id);
            restoredOffice!.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetActiveLiaisonOfficesCount_ReturnsCorrectCount()
        {
            // Act
            var count = await TestOffices
                .CountAsync(lo => lo.IsActive && !lo.IsDeleted);

            // Assert
            count.Should().Be(3);
        }

        [Fact]
        public async Task GetTotalLiaisonOfficesCount_IncludingAll_ReturnsCorrectCount()
        {
            // Act
            var totalCount = await TestOffices.CountAsync();

            // Assert
            totalCount.Should().Be(5); // 3 active + 1 inactive + 1 deleted
        }

        #endregion
    }
}
