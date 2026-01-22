using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Managers
{
    /// <summary>
    /// Tests for GlobalIndicesManager
    /// Based on GlobalIndicesManager_TestCases.md (15+ tests)
    /// </summary>
    public class GlobalIndicesManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GlobalIndicesManager _manager;

        public GlobalIndicesManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"IndicesTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();

            _manager = new GlobalIndicesManager(_mockMapper.Object, _context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed 5 test countries
            for (int i = 1; i <= 5; i++)
            {
                _context.Countries.Add(new Country
                {
                    Id = i,
                    Name = $"Country {i}",
                    Code = $"C{i}"
                });
            }

            _context.SaveChanges();
        }

        #region TC-OPP-GI-F-001: Upload Global Indices for All Countries

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GI-F-001")]
        public async Task UploadGlobalIndices_AllCountries_Success()
        {
            // Arrange - Prepare index data for 5 countries
            var indexData = new List<GlobalIndexUpload>
            {
                new GlobalIndexUpload { CountryId = 1, IndexType = "MVI", Value = 35m, Year = 2026 },
                new GlobalIndexUpload { CountryId = 2, IndexType = "MVI", Value = 28m, Year = 2026 },
                new GlobalIndexUpload { CountryId = 3, IndexType = "MVI", Value = 42m, Year = 2026 },
                new GlobalIndexUpload { CountryId = 4, IndexType = "MVI", Value = 18m, Year = 2026 },
                new GlobalIndexUpload { CountryId = 5, IndexType = "MVI", Value = 51m, Year = 2026 }
            };

            // Act
            var result = await _manager.UploadGlobalIndicesAsync(indexData, uploadedBy: 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.RecordsProcessed);
            Assert.Empty(result.Errors);
            
            // Verify data stored
            var storedIndices = await _context.GlobalIndices
                .Where(gi => gi.Year == 2026 && gi.IndexType == "MVI")
                .ToListAsync();
            
            Assert.Equal(5, storedIndices.Count);
            Assert.All(storedIndices, gi => Assert.Equal(2026, gi.Year));
        }

        #endregion

        #region TC-OPP-GI-F-002: Update Existing Indices (Replace)

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GI-F-002")]
        public async Task UpdateGlobalIndices_ReplaceExisting_Success()
        {
            // Arrange - Upload 2025 data first
            var indices2025 = new List<GlobalIndex>
            {
                new GlobalIndex { Id = 1, CountryId = 1, IndexType = "FSI", Value = 92m, Year = 2025, IsCurrent = true }
            };
            _context.GlobalIndices.AddRange(indices2025);
            await _context.SaveChangesAsync();

            // Act - Upload 2026 data (should replace as current)
            var indices2026 = new List<GlobalIndexUpload>
            {
                new GlobalIndexUpload { CountryId = 1, IndexType = "FSI", Value = 88m, Year = 2026 }
            };
            var result = await _manager.UploadGlobalIndicesAsync(indices2026, uploadedBy: 1);

            // Assert
            Assert.True(result.Success);
            
            // 2025 data archived (not current)
            var historical = await _context.GlobalIndices.FirstAsync(gi => gi.Year == 2025);
            Assert.False(historical.IsCurrent);
            
            // 2026 data is current
            var current = await _context.GlobalIndices.FirstAsync(gi => gi.Year == 2026);
            Assert.True(current.IsCurrent);
            Assert.Equal(88m, current.Value);
        }

        #endregion

        #region TC-OPP-GI-F-003: Query Historical "As-At" Data

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GI-F-003")]
        public async Task QueryHistoricalIndices_AsAtDate_ReturnsCorrectVersion()
        {
            // Arrange - Upload multiple years of data
            _context.GlobalIndices.AddRange(new[]
            {
                new GlobalIndex { CountryId = 1, IndexType = "MVI", Value = 35m, Year = 2024, IsCurrent = false },
                new GlobalIndex { CountryId = 1, IndexType = "MVI", Value = 32m, Year = 2025, IsCurrent = false },
                new GlobalIndex { CountryId = 1, IndexType = "MVI", Value = 30m, Year = 2026, IsCurrent = true }
            });
            await _context.SaveChangesAsync();

            // Act - Query "as-at" 2025
            var asAt2025 = await _manager.GetIndicesAsAtAsync(countryId: 1, asAtYear: 2025);

            // Assert
            Assert.NotNull(asAt2025);
            var mvi2025 = asAt2025.First(i => i.IndexType == "MVI");
            Assert.Equal(32m, mvi2025.Value); // 2025 value, not 2026
        }

        #endregion

        #region TC-OPP-GI-F-004: Trigger DST Updates on Index Change

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GI-F-004")]
        public async Task UploadIndices_IdentifiesAffectedOpportunities_Success()
        {
            // Arrange
            // Create opportunity in Country 1
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                PrimaryCountryId = 1,
                Status = "Profiling", // Active development
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            
            // Old index value
            _context.GlobalIndices.Add(new GlobalIndex
            {
                CountryId = 1,
                IndexType = "FSI",
                Value = 75m,
                Year = 2025,
                IsCurrent = true
            });
            await _context.SaveChangesAsync();

            // Act - Upload new index value (significant change)
            var newIndices = new List<GlobalIndexUpload>
            {
                new GlobalIndexUpload { CountryId = 1, IndexType = "FSI", Value = 95m, Year = 2026 } // +20 points
            };
            var result = await _manager.UploadGlobalIndicesAsync(newIndices, uploadedBy: 1);

            // Assert - Affected opportunities identified
            Assert.True(result.Success);
            Assert.Single(result.AffectedOpportunities);
            Assert.Equal(1, result.AffectedOpportunities.First());
            
            // Notification would be sent to opportunity manager
            Assert.Contains("significant change", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-GI-V-001: Validate Index Value Range

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GI-V-001")]
        public async Task UploadIndices_ValueOutOfRange_ThrowsException()
        {
            // Arrange - MVI typically 0-100 range
            var invalidIndices = new List<GlobalIndexUpload>
            {
                new GlobalIndexUpload { CountryId = 1, IndexType = "MVI", Value = 150m, Year = 2026 } // Out of range
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.UploadGlobalIndicesAsync(invalidIndices, uploadedBy: 1));

            Assert.Contains("range", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("0-100", ex.Message);
        }

        #endregion

        #region Helper Classes

        public class GlobalIndex
        {
            public int Id { get; set; }
            public int CountryId { get; set; }
            public string IndexType { get; set; } // MVI, FSI, CPI, etc.
            public decimal Value { get; set; }
            public int Year { get; set; }
            public bool IsCurrent { get; set; }
            public int UploadedBy { get; set; }
            public DateTime UploadedDate { get; set; }
        }

        public class GlobalIndexUpload
        {
            public int CountryId { get; set; }
            public string IndexType { get; set; }
            public decimal Value { get; set; }
            public int Year { get; set; }
        }

        public class GlobalIndexUploadResult
        {
            public bool Success { get; set; }
            public int RecordsProcessed { get; set; }
            public List<string> Errors { get; set; }
            public List<int> AffectedOpportunities { get; set; }
            public string Message { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
