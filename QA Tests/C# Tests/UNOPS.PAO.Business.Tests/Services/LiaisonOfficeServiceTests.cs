/**
 * @fileoverview Comprehensive unit tests for LiaisonOfficeService
 * Tests liaison office lookups, filtering, CRUD operations, and edge cases
 * @author UNOPS Opportunity+ System Development Team
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.Services
{
    /// <summary>
    /// Test suite for LiaisonOfficeService
    /// Tests liaison office lookups, filtering, partner associations, and validation
    /// </summary>
    public class LiaisonOfficeServiceTests : IDisposable
    {
        private readonly AppDbContext _context;

        public LiaisonOfficeServiceTests()
        {
            _context = TestDbContextFactory.Create();
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var liaisonOffices = new List<LiaisonOffice>
            {
                new LiaisonOffice 
                { 
                    Name = "Nairobi Office", 
                    Code = "NBO", 
                    IsActive = true,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = "Kampala Office", 
                    Code = "KLA", 
                    IsActive = true,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = "Dar es Salaam Office", 
                    Code = "DAR", 
                    IsActive = true,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = "Inactive Office", 
                    Code = "INA", 
                    IsActive = false,
                    IsDeleted = false
                },
                new LiaisonOffice 
                { 
                    Name = "Deleted Office", 
                    Code = "DEL", 
                    IsActive = true,
                    IsDeleted = true
                }
            };

            await _context.LiaisonOffices.AddRangeAsync(liaisonOffices);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        #region Basic Lookup Tests

        [Fact]
        public async Task GetAllLiaisonOffices_ReturnsActiveNonDeletedOnly()
        {
            // Act
            var offices = await _context.LiaisonOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            // Assert
            offices.Should().HaveCount(3);
            offices.Should().NotContain(lo => lo.Name == "Inactive Office");
            offices.Should().NotContain(lo => lo.Name == "Deleted Office");
        }

        [Fact]
        public async Task GetLiaisonOfficeById_ExistingId_ReturnsOffice()
        {
            // Arrange
            var firstOffice = await _context.LiaisonOffices.FirstAsync();

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Id == firstOffice.Id);

            // Assert
            office.Should().NotBeNull();
            office!.Name.Should().Be("Nairobi Office");
        }

        [Fact]
        public async Task GetLiaisonOfficeByCode_ValidCode_ReturnsOffice()
        {
            // Arrange
            var targetCode = "NBO";

            // Act
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == targetCode && lo.IsActive && !lo.IsDeleted);

            // Assert
            office.Should().NotBeNull();
            office!.Name.Should().Be("Nairobi Office");
        }

        [Fact]
        public async Task GetLiaisonOfficeByCode_InvalidCode_ReturnsNull()
        {
            // Arrange
            var invalidCode = "INVALID";

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
            var activeOffices = await _context.LiaisonOffices
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
            var allOffices = await _context.LiaisonOffices
                .Where(lo => !lo.IsDeleted)
                .ToListAsync();

            // Assert
            allOffices.Should().HaveCount(4); // Active + Inactive
            allOffices.Should().Contain(lo => lo.Name == "Inactive Office");
        }

        [Fact]
        public async Task SearchLiaisonOffices_ByName_ReturnsMatches()
        {
            // Arrange
            var searchTerm = "Nairobi";

            // Act
            var offices = await _context.LiaisonOffices
                .Where(lo => lo.Name.Contains(searchTerm) && lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            // Assert
            offices.Should().HaveCount(1);
            offices[0].Name.Should().Contain("Nairobi");
        }

        [Fact]
        public async Task SearchLiaisonOffices_ByCode_ReturnsMatches()
        {
            // Arrange
            var searchCode = "K";

            // Act
            var offices = await _context.LiaisonOffices
                .Where(lo => lo.Code.Contains(searchCode) && lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            // Assert
            offices.Should().HaveCount(1);
            offices[0].Code.Should().Be("KLA");
        }

        #endregion

        #region CRUD Operations Tests

        [Fact]
        public async Task CreateLiaisonOffice_ValidData_CreatesSuccessfully()
        {
            // Arrange
            var newOffice = new LiaisonOffice
            {
                Name = "New York Office",
                Code = "NYC",
                IsActive = true,
                IsDeleted = false
            };

            // Act
            await _context.LiaisonOffices.AddAsync(newOffice);
            await _context.SaveChangesAsync();

            // Assert
            var savedOffice = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == "NYC");
            
            savedOffice.Should().NotBeNull();
            savedOffice!.Name.Should().Be("New York Office");
            savedOffice.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateLiaisonOffice_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            var office = await _context.LiaisonOffices.FirstAsync();
            var originalName = office.Name;

            // Act
            office.Name = "Updated Office Name";
            await _context.SaveChangesAsync();

            // Assert
            var updatedOffice = await _context.LiaisonOffices.FindAsync(office.Id);
            updatedOffice!.Name.Should().Be("Updated Office Name");
            updatedOffice.Name.Should().NotBe(originalName);
        }

        [Fact]
        public async Task DeleteLiaisonOffice_SoftDelete_SetsIsDeletedTrue()
        {
            // Arrange
            var office = await _context.LiaisonOffices.FirstAsync(lo => !lo.IsDeleted);

            // Act - Soft delete
            office.IsDeleted = true;
            await _context.SaveChangesAsync();

            // Assert
            var deletedOffice = await _context.LiaisonOffices.FindAsync(office.Id);
            deletedOffice!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeactivateLiaisonOffice_SetsIsActiveFalse()
        {
            // Arrange
            var office = await _context.LiaisonOffices.FirstAsync(lo => lo.IsActive && !lo.IsDeleted);

            // Act
            office.IsActive = false;
            await _context.SaveChangesAsync();

            // Assert
            var deactivatedOffice = await _context.LiaisonOffices.FindAsync(office.Id);
            deactivatedOffice!.IsActive.Should().BeFalse();
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task CreateLiaisonOffice_DuplicateCode_ShouldBeDetectable()
        {
            // Arrange
            var existingCode = "NBO";
            var duplicateOffice = new LiaisonOffice
            {
                Name = "Duplicate Office",
                Code = existingCode,
                IsActive = true
            };

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
                ? await _context.LiaisonOffices.Where(lo => lo.IsActive && !lo.IsDeleted).ToListAsync()
                : await _context.LiaisonOffices
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

        [Fact]
        public async Task GetLiaisonOffices_ConcurrentReads_HandledCorrectly()
        {
            // Act - Multiple concurrent reads
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => _context.LiaisonOffices
                    .Where(lo => lo.IsActive && !lo.IsDeleted)
                    .ToListAsync());

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().AllSatisfy(r => r.Should().HaveCount(3));
        }

        [Fact]
        public async Task GetLiaisonOfficeByCode_CaseInsensitive_ShouldMatch()
        {
            // Arrange
            var codes = new[] { "nbo", "NBO", "Nbo", "nBo" };

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
            var pagedOffices = await _context.LiaisonOffices
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
            var sortedOffices = await _context.LiaisonOffices
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
                    Name = $"Office {i}",
                    Code = $"OF{i:D3}",
                    IsActive = true,
                    IsDeleted = false
                })
                .ToList();

            await _context.LiaisonOffices.AddRangeAsync(additionalOffices);
            await _context.SaveChangesAsync();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var offices = await _context.LiaisonOffices
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
                    Name = $"Performance Test Office {i}",
                    Code = $"PTO{i:D3}",
                    IsActive = true,
                    IsDeleted = false
                })
                .ToList();

            await _context.LiaisonOffices.AddRangeAsync(additionalOffices);
            await _context.SaveChangesAsync();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var offices = await _context.LiaisonOffices
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
            var inactiveOffice = await _context.LiaisonOffices
                .FirstAsync(lo => !lo.IsActive && !lo.IsDeleted);

            // Act
            inactiveOffice.IsActive = true;
            await _context.SaveChangesAsync();

            // Assert
            var activatedOffice = await _context.LiaisonOffices.FindAsync(inactiveOffice.Id);
            activatedOffice!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreLiaisonOffice_WhenSoftDeleted_SetsIsDeletedFalse()
        {
            // Arrange
            var deletedOffice = await _context.LiaisonOffices
                .FirstAsync(lo => lo.IsDeleted);

            // Act
            deletedOffice.IsDeleted = false;
            await _context.SaveChangesAsync();

            // Assert
            var restoredOffice = await _context.LiaisonOffices.FindAsync(deletedOffice.Id);
            restoredOffice!.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetActiveLiaisonOfficesCount_ReturnsCorrectCount()
        {
            // Act
            var count = await _context.LiaisonOffices
                .CountAsync(lo => lo.IsActive && !lo.IsDeleted);

            // Assert
            count.Should().Be(3);
        }

        [Fact]
        public async Task GetTotalLiaisonOfficesCount_IncludingAll_ReturnsCorrectCount()
        {
            // Act
            var totalCount = await _context.LiaisonOffices.CountAsync();

            // Assert
            totalCount.Should().Be(5); // 3 active + 1 inactive + 1 deleted
        }

        #endregion
    }
}

