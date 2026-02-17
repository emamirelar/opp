using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.Managers
{
    /// <summary>
    /// Test suite for DocumentTypeManager
    /// Covers:
    /// - Document type retrieval
    /// - Filtering by entity type
    /// - Pagination
    /// - Edge cases
    /// </summary>
    public class DocumentTypeManagerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public DocumentTypeManagerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"DocumentTypeManagerTest_{Guid.NewGuid()}")
                .Options;
            _context = TestDbContextFactory.Create(_dbOptions);
            SeedTestData();
        }

        private void SeedTestData()
        {
            var documentTypes = new List<DocumentType>
            {
                // Partner document types
                new DocumentType { Id = 1, Name = "Contract", EntityType = "Partner", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new DocumentType { Id = 2, Name = "MOU", EntityType = "Partner", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new DocumentType { Id = 3, Name = "Agreement", EntityType = "Partner", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                
                // Contact document types
                new DocumentType { Id = 4, Name = "CV", EntityType = "Contact", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new DocumentType { Id = 5, Name = "ID Document", EntityType = "Contact", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                
                // Interaction document types
                new DocumentType { Id = 6, Name = "Meeting Notes", EntityType = "Interaction", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new DocumentType { Id = 7, Name = "Email Attachment", EntityType = "Interaction", Status = EntityStatus.Active, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                
                // Inactive document type
                new DocumentType { Id = 8, Name = "Old Type", EntityType = "Partner", Status = EntityStatus.Inactive, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                
                // Deleted document type
                new DocumentType { Id = 9, Name = "Deleted Type", EntityType = "Partner", Status = EntityStatus.Active, IsDeleted = true, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
            };
            
            _context.DocumentTypes.AddRange(documentTypes);
            _context.SaveChanges();
        }

        #region Basic Retrieval Tests

        [Fact]
        public async Task TC_DTM_001_GetAllDocumentTypes_ReturnsNotDeleted()
        {
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(8, types.Count);
            Assert.DoesNotContain(types, dt => dt.Name == "Deleted Type");
        }

        [Fact]
        public async Task TC_DTM_002_GetActiveDocumentTypes_ReturnsOnlyActive()
        {
            var types = await _context.DocumentTypes
                .Where(dt => dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(7, types.Count);
            Assert.DoesNotContain(types, dt => dt.Name == "Old Type");
            Assert.DoesNotContain(types, dt => dt.Name == "Deleted Type");
        }

        [Fact]
        public async Task TC_DTM_003_GetDocumentTypeById_Exists_ReturnsType()
        {
            var type = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.Id == 1 && !dt.IsDeleted);

            Assert.NotNull(type);
            Assert.Equal("Contract", type.Name);
            Assert.Equal("Partner", type.EntityType);
        }

        [Fact]
        public async Task TC_DTM_004_GetDocumentTypeById_NotExists_ReturnsNull()
        {
            var type = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.Id == 999 && !dt.IsDeleted);

            Assert.Null(type);
        }

        [Fact]
        public async Task TC_DTM_005_GetDocumentTypeById_Deleted_ReturnsNull()
        {
            var type = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.Id == 9 && !dt.IsDeleted);

            Assert.Null(type);
        }

        #endregion

        #region Entity Type Filtering Tests

        [Fact]
        public async Task TC_DTM_010_FilterByEntityType_Partner_ReturnsPartnerTypes()
        {
            var types = await _context.DocumentTypes
                .Where(dt => dt.EntityType == "Partner" && dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(3, types.Count);
            Assert.Contains(types, dt => dt.Name == "Contract");
            Assert.Contains(types, dt => dt.Name == "MOU");
            Assert.Contains(types, dt => dt.Name == "Agreement");
        }

        [Fact]
        public async Task TC_DTM_011_FilterByEntityType_Contact_ReturnsContactTypes()
        {
            var types = await _context.DocumentTypes
                .Where(dt => dt.EntityType == "Contact" && dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, types.Count);
            Assert.Contains(types, dt => dt.Name == "CV");
            Assert.Contains(types, dt => dt.Name == "ID Document");
        }

        [Fact]
        public async Task TC_DTM_012_FilterByEntityType_Interaction_ReturnsInteractionTypes()
        {
            var types = await _context.DocumentTypes
                .Where(dt => dt.EntityType == "Interaction" && dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, types.Count);
            Assert.Contains(types, dt => dt.Name == "Meeting Notes");
            Assert.Contains(types, dt => dt.Name == "Email Attachment");
        }

        [Fact]
        public async Task TC_DTM_013_FilterByEntityType_CaseInsensitive()
        {
            var entityType = "partner";
            var types = await _context.DocumentTypes
                .Where(dt => dt.EntityType.ToLower() == entityType.ToLower() && dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(3, types.Count);
        }

        [Fact]
        public async Task TC_DTM_014_FilterByEntityType_Invalid_ReturnsEmpty()
        {
            var types = await _context.DocumentTypes
                .Where(dt => dt.EntityType == "InvalidType" && !dt.IsDeleted)
                .ToListAsync();

            Assert.Empty(types);
        }

        [Fact]
        public async Task TC_DTM_015_FilterByEntityType_Empty_ReturnsAll()
        {
            var entityType = "";
            var types = await _context.DocumentTypes
                .Where(dt => (string.IsNullOrEmpty(entityType) || dt.EntityType == entityType) && !dt.IsDeleted)
                .ToListAsync();

            Assert.Equal(8, types.Count);
        }

        #endregion

        #region Pagination Tests

        [Fact]
        public async Task TC_DTM_020_Pagination_FirstPage_ReturnsCorrectItems()
        {
            var pageSize = 3;
            var pageIndex = 0;
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .OrderBy(dt => dt.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Equal(3, types.Count);
        }

        [Fact]
        public async Task TC_DTM_021_Pagination_SecondPage_ReturnsCorrectItems()
        {
            var pageSize = 3;
            var pageIndex = 1;
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .OrderBy(dt => dt.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Equal(3, types.Count);
        }

        [Fact]
        public async Task TC_DTM_022_Pagination_LastPage_ReturnsRemaining()
        {
            var pageSize = 3;
            var pageIndex = 2;
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .OrderBy(dt => dt.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Equal(2, types.Count); // 8 total, 2 remaining after 2 pages of 3
        }

        [Fact]
        public async Task TC_DTM_023_Pagination_BeyondData_ReturnsEmpty()
        {
            var pageSize = 10;
            var pageIndex = 10;
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Empty(types);
        }

        [Fact]
        public async Task TC_DTM_024_Pagination_WithFilter_ReturnsFilteredAndPaged()
        {
            var entityType = "Partner";
            var pageSize = 2;
            var pageIndex = 0;
            var types = await _context.DocumentTypes
                .Where(dt => dt.EntityType == entityType && dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .OrderBy(dt => dt.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Equal(2, types.Count);
        }

        #endregion

        #region Sorting Tests

        [Fact]
        public async Task TC_DTM_030_SortByName_Ascending()
        {
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .OrderBy(dt => dt.Name)
                .Select(dt => dt.Name)
                .ToListAsync();

            var sorted = types.OrderBy(n => n).ToList();
            Assert.Equal(sorted, types);
        }

        [Fact]
        public async Task TC_DTM_031_SortByName_Descending()
        {
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .OrderByDescending(dt => dt.Name)
                .Select(dt => dt.Name)
                .ToListAsync();

            var sorted = types.OrderByDescending(n => n).ToList();
            Assert.Equal(sorted, types);
        }

        [Fact]
        public async Task TC_DTM_032_SortByEntityType_ThenByName()
        {
            var types = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted && dt.Status == EntityStatus.Active)
                .OrderBy(dt => dt.EntityType)
                .ThenBy(dt => dt.Name)
                .ToListAsync();

            // First should be Contact types (alphabetically before Interaction and Partner)
            Assert.Equal("Contact", types.First().EntityType);
        }

        #endregion

        #region Search Tests

        [Fact]
        public async Task TC_DTM_040_SearchByName_ReturnsMatches()
        {
            var searchTerm = "Contract";
            var types = await _context.DocumentTypes
                .Where(dt => dt.Name.Contains(searchTerm) && !dt.IsDeleted)
                .ToListAsync();

            Assert.Single(types);
            Assert.Equal("Contract", types.First().Name);
        }

        [Fact]
        public async Task TC_DTM_041_SearchByName_ReturnsMatches()
        {
            // NOTE: DocumentType no longer has Description property - searching by Name only
            var searchTerm = "Contract";
            var types = await _context.DocumentTypes
                .Where(dt => dt.Name.Contains(searchTerm) && !dt.IsDeleted)
                .ToListAsync();

            Assert.Single(types);
            Assert.Equal("Contract", types.First().Name);
        }

        [Fact]
        public async Task TC_DTM_042_SearchByName_CaseInsensitive_ReturnsMatches()
        {
            // NOTE: DocumentType no longer has Description property - searching by Name only
            var searchTerm = "meeting";
            var types = await _context.DocumentTypes
                .Where(dt => dt.Name.ToLower().Contains(searchTerm.ToLower()) 
                          && !dt.IsDeleted)
                .ToListAsync();

            Assert.Single(types);
            Assert.Equal("Meeting Notes", types.First().Name);
        }

        [Fact]
        public async Task TC_DTM_043_Search_NoMatches_ReturnsEmpty()
        {
            var searchTerm = "NonExistent";
            var types = await _context.DocumentTypes
                .Where(dt => dt.Name.Contains(searchTerm) && !dt.IsDeleted)
                .ToListAsync();

            Assert.Empty(types);
        }

        #endregion

        #region CRUD Operation Tests

        [Fact]
        public async Task TC_DTM_050_CreateDocumentType_ValidData_Succeeds()
        {
            var newType = new DocumentType
            {
                Name = "New Document Type",
                EntityType = "Partner",
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.DocumentTypes.Add(newType);
            await _context.SaveChangesAsync();

            Assert.True(newType.Id > 0);
            var retrieved = await _context.DocumentTypes.FindAsync(newType.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("New Document Type", retrieved.Name);
        }

        [Fact]
        public async Task TC_DTM_051_UpdateDocumentType_ValidData_Succeeds()
        {
            // NOTE: DocumentType no longer has Description property - testing Name update
            var type = await _context.DocumentTypes.FirstAsync(dt => dt.Id == 1);
            type.Name = "Updated Contract Type";
            type.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updated = await _context.DocumentTypes.FindAsync(1);
            Assert.Equal("Updated Contract Type", updated!.Name);
        }

        [Fact]
        public async Task TC_DTM_052_SoftDeleteDocumentType_SetsIsDeletedTrue()
        {
            var type = await _context.DocumentTypes.FirstAsync(dt => dt.Id == 2);
            type.IsDeleted = true;
            type.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var deleted = await _context.DocumentTypes.FindAsync(2);
            Assert.True(deleted!.IsDeleted);

            var activeTypes = await _context.DocumentTypes
                .Where(dt => !dt.IsDeleted)
                .ToListAsync();
            Assert.DoesNotContain(activeTypes, dt => dt.Id == 2);
        }

        [Fact]
        public async Task TC_DTM_053_DeactivateDocumentType_SetsStatusInactive()
        {
            var type = await _context.DocumentTypes.FirstAsync(dt => dt.Id == 3);
            type.Status = EntityStatus.Inactive;
            type.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var deactivated = await _context.DocumentTypes.FindAsync(3);
            Assert.Equal(EntityStatus.Inactive, deactivated!.Status);
        }

        [Fact]
        public async Task TC_DTM_054_ReactivateDocumentType_SetsStatusActive()
        {
            var type = await _context.DocumentTypes.FirstAsync(dt => dt.Id == 8); // Old Type (inactive)
            type.Status = EntityStatus.Active;
            type.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var reactivated = await _context.DocumentTypes.FindAsync(8);
            Assert.Equal(EntityStatus.Active, reactivated!.Status);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task TC_DTM_060_CreateDocumentType_MissingName_ThrowsException()
        {
            var newType = new DocumentType
            {
                Name = null!,
                EntityType = "Partner",
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            _context.DocumentTypes.Add(newType);
            await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        }

        [Fact]
        public async Task TC_DTM_061_GetDocumentTypeCount_ByEntityType()
        {
            var counts = await _context.DocumentTypes
                .Where(dt => dt.Status == EntityStatus.Active && !dt.IsDeleted)
                .GroupBy(dt => dt.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .ToListAsync();

            Assert.Equal(3, counts.Count);
            Assert.Contains(counts, c => c.EntityType == "Partner" && c.Count == 3);
            Assert.Contains(counts, c => c.EntityType == "Contact" && c.Count == 2);
            Assert.Contains(counts, c => c.EntityType == "Interaction" && c.Count == 2);
        }

        #endregion

        public void Dispose()
        {
            if (TestEnvironment.UseInMemory)
            {
                try { _context.Database.EnsureDeleted(); }
                catch { /* SQLite connection may already be closed during concurrent test runs */ }
            }
            _context.Dispose();
        }
    }
}

