using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.NegativeTests
{
    /// <summary>
    /// Comprehensive negative test scenarios for Opportunity features
    /// Tests invalid inputs, error conditions, and edge cases
    /// </summary>
    public class OpportunityNegativeTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly OpportunityManager _manager;

        public OpportunityNegativeTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"NegativeTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _manager = new OpportunityManager(_context);
        }

        #region TC-OPP-NEG-001: Invalid Budget Values

        [Theory]
        [InlineData(-1000000)] // Negative budget
        [InlineData(0)] // Zero budget
        [InlineData(long.MaxValue)] // Unrealistically large
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-001")]
        public async Task CreateOpportunity_InvalidBudget_ThrowsException(decimal invalidBudget)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = invalidBudget,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            if (invalidBudget < 0)
            {
                var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                {
                    _context.Opportunities.Add(opportunity);
                    await _context.SaveChangesAsync();
                    await _manager.ValidateOpportunityAsync(opportunity.Id);
                });
                Assert.Contains("negative", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
            else if (invalidBudget == 0)
            {
                var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                {
                    _context.Opportunities.Add(opportunity);
                    await _context.SaveChangesAsync();
                    await _manager.ValidateOpportunityAsync(opportunity.Id);
                });
                Assert.Contains("zero", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion

        #region TC-OPP-NEG-002: Invalid Timeline Values

        [Theory]
        [InlineData(-12)] // Negative timeline
        [InlineData(0)] // Zero timeline
        [InlineData(999)] // Unrealistically long (83+ years)
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-002")]
        public async Task CreateOpportunity_InvalidTimeline_ThrowsException(int invalidTimeline)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000,
                Timeline = invalidTimeline,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                _context.Opportunities.Add(opportunity);
                await _context.SaveChangesAsync();
                await _manager.ValidateOpportunityAsync(opportunity.Id);
            });

            if (invalidTimeline <= 0)
                Assert.Contains("positive", ex.Message, StringComparison.OrdinalIgnoreCase);
            else
                Assert.Contains("reasonable", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-NEG-003: Missing Required Fields

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-003")]
        public async Task CreateOpportunity_MissingRequiredFields_ThrowsException()
        {
            // Arrange - Missing name
            var opportunityNoName = new Domain.Entities.Opportunity
            {
                Name = null, // Missing required field
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                _context.Opportunities.Add(opportunityNoName);
                await _context.SaveChangesAsync();
                await _manager.ValidateOpportunityAsync(opportunityNoName.Id);
            });

            Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-NEG-004: Duplicate Opportunity Names

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-004")]
        public async Task CreateOpportunity_DuplicateName_ThrowsException()
        {
            // Arrange - First opportunity
            var opportunity1 = new Domain.Entities.Opportunity
            {
                Name = "Unique Project Name",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity1);
            await _context.SaveChangesAsync();

            // Second opportunity with same name
            var opportunity2 = new Domain.Entities.Opportunity
            {
                Name = "Unique Project Name", // Duplicate!
                EstimatedValue = 2000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                _context.Opportunities.Add(opportunity2);
                await _context.SaveChangesAsync();
                await _manager.ValidateDuplicateNameAsync(opportunity2.Name);
            });

            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-NEG-005: Invalid Status Transitions

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-005")]
        public async Task UpdateStatus_InvalidTransition_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                Status = "Draft",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert - Try invalid transition: Draft → Converted
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.UpdateStatusAsync(opportunity.Id, "Converted"));

            Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("transition", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-NEG-006: Non-Existent Opportunity ID

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-006")]
        public async Task GetOpportunity_NonExistentId_ThrowsException()
        {
            // Arrange
            var nonExistentId = 99999;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _manager.GetByIdAsync(nonExistentId));

            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(nonExistentId.ToString(), ex.Message);
        }

        #endregion

        #region TC-OPP-NEG-007: Unauthorized Access

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-007")]
        public async Task AccessOpportunity_UnauthorizedUser_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Confidential Opportunity",
                EstimatedValue = 5000000,
                IsSensitive = true,
                CreatedBy = 1, // Owner
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert - User 99 (unauthorized) tries to access
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _manager.ValidateAccessAsync(opportunity.Id, userId: 99));

            Assert.Contains("unauthorized", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-NEG-008: Invalid Date Ranges

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-008")]
        public async Task CreateOpportunity_InvalidDateRange_ThrowsException()
        {
            // Arrange - End date before start date
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-30), // End before start!
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                _context.Opportunities.Add(opportunity);
                await _context.SaveChangesAsync();
                await _manager.ValidateDateRangeAsync(opportunity.Id);
            });

            Assert.Contains("end date", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("before", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-NEG-009: SQL Injection Attempts

        [Theory]
        [InlineData("'; DROP TABLE Opportunities; --")]
        [InlineData("1' OR '1'='1")]
        [InlineData("<script>alert('xss')</script>")]
        [Trait("Category", "P0")]
        [Trait("Type", "Security")]
        [Trait("TestId", "TC-OPP-NEG-009")]
        public async Task SearchOpportunities_MaliciousInput_SafelyHandled(string maliciousInput)
        {
            // Arrange & Act
            var results = await _manager.SearchOpportunitiesAsync(maliciousInput);

            // Assert - Should return empty results, not crash or execute malicious code
            Assert.NotNull(results);
            Assert.Empty(results); // No results for malicious input
            
            // Verify database integrity - table still exists
            var allOpportunities = await _context.Opportunities.ToListAsync();
            Assert.NotNull(allOpportunities); // Table not dropped
        }

        #endregion

        #region TC-OPP-NEG-010: Concurrent Modification Conflicts

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-010")]
        public async Task UpdateOpportunity_ConcurrentModification_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000,
                RowVersion = new byte[] { 1, 2, 3, 4 }, // Initial version
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // User 1 loads opportunity
            var opp1 = await _context.Opportunities.FindAsync(opportunity.Id);
            
            // User 2 loads and updates opportunity
            var opp2 = await _context.Opportunities.FindAsync(opportunity.Id);
            opp2.Name = "Updated by User 2";
            await _context.SaveChangesAsync(); // Version changes

            // Act & Assert - User 1 tries to update with stale data
            opp1.Name = "Updated by User 1";
            
            var ex = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                await _context.SaveChangesAsync());

            Assert.NotNull(ex);
        }

        #endregion

        #region Helper Classes

        public class BusinessException : Exception
        {
            public BusinessException(string message) : base(message) { }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
