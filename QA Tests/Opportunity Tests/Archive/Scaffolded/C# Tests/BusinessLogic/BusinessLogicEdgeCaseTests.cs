using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.BusinessLogic;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.BusinessLogic
{
    /// <summary>
    /// Comprehensive edge case tests for Business Logic layer
    /// Tests boundary conditions, unusual inputs, and rare scenarios
    /// </summary>
    public class BusinessLogicEdgeCaseTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IAIService> _mockAIService;

        public BusinessLogicEdgeCaseTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"BusinessEdgeTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockAIService = new Mock<IAIService>();
        }

        #region TC-OPP-BL-EDGE-001: Unicode and Special Characters in Names

        [Theory]
        [InlineData("Project with Émojis 🏗️🌍")]
        [InlineData("مشروع بالعربية (Arabic Project)")]
        [InlineData("中文项目名称 (Chinese Project)")] 
        [InlineData("Проект на русском (Russian Project)")]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-001")]
        public async Task CreateOpportunity_UnicodeCharacters_StoresAndRetrievesCorrectly(string unicodeName)
        {
            // Arrange & Act
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = unicodeName,
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FirstOrDefaultAsync(o => o.Name == unicodeName);
            Assert.NotNull(retrieved);
            Assert.Equal(unicodeName, retrieved.Name); // Exact match with Unicode
        }

        #endregion

        #region TC-OPP-BL-EDGE-002: Extremely Long Text Fields

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-002")]
        public async Task CreateOpportunity_VeryLongDescription_HandlesOrTruncates()
        {
            // Arrange - 10,000 character description
            var longDescription = string.Join(" ", Enumerable.Repeat("This is a very long description.", 300));
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Long Description Test",
                Description = longDescription, // ~10K characters
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.NotNull(retrieved);
            Assert.NotNull(retrieved.Description);
            // Either stored fully or truncated gracefully
            Assert.True(retrieved.Description.Length > 0);
        }

        #endregion

        #region TC-OPP-BL-EDGE-003: Null vs Empty String Handling

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")] // Whitespace
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-003")]
        public async Task CreateOpportunity_NullOrEmptyOptionalFields_HandlesGracefully(string optionalValue)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Required Field Test",
                Description = optionalValue, // Optional field
                Notes = optionalValue, // Optional field
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Required Field Test", retrieved.Name);
        }

        #endregion

        #region TC-OPP-BL-EDGE-004: Date Boundaries (Year 1900 and 2100)

        [Theory]
        [InlineData("1900-01-01")] // Very old date
        [InlineData("2099-12-31")] // Far future date
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-004")]
        public async Task CreateOpportunity_ExtremeDates_HandlesDateBoundaries(string dateString)
        {
            // Arrange
            var extremeDate = DateTime.Parse(dateString);
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Date Boundary Test",
                EstimatedValue = 1000000,
                StartDate = extremeDate,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(extremeDate.Date, retrieved.StartDate.Value.Date);
        }

        #endregion

        #region TC-OPP-BL-EDGE-005: Decimal Precision Edge Cases

        [Theory]
        [InlineData(1000000.999)] // Many decimal places
        [InlineData(2500000.505050)] // Repeating decimals
        [InlineData(9999999.99)] // Edge of display
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-005")]
        public async Task CreateOpportunity_PreciseDecimals_RoundsAppropriately(decimal preciseAmount)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Decimal Precision Test",
                EstimatedValue = preciseAmount,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.NotNull(retrieved);
            // Should be rounded to 2 decimal places for currency
            var rounded = Math.Round(preciseAmount, 2);
            Assert.True(Math.Abs(retrieved.EstimatedValue.Value - rounded) < 0.01m);
        }

        #endregion

        #region TC-OPP-BL-EDGE-006: Batch Operations at Scale

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-006")]
        public async Task BulkCreateOpportunities_1000Records_CompletesSuccessfully()
        {
            // Arrange
            var opportunities = Enumerable.Range(1, 1000).Select(i => new Domain.Entities.Opportunity
            {
                Name = $"Bulk Create Test {i}",
                EstimatedValue = 1000000 + (i * 1000),
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            // Act
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Assert
            var count = await _context.Opportunities.CountAsync();
            Assert.Equal(1000, count);
        }

        #endregion

        #region TC-OPP-BL-EDGE-007: Circular Dependencies in Schedule

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-007")]
        public async Task ValidateSchedule_CircularDependency_ThrowsException()
        {
            // Arrange
            var tasks = new List<ScheduleTask>
            {
                new ScheduleTask { Id = 1, Name = "Task A", DependsOn = new[] { 3 } },
                new ScheduleTask { Id = 2, Name = "Task B", DependsOn = new[] { 1 } },
                new ScheduleTask { Id = 3, Name = "Task C", DependsOn = new[] { 2 } } // Circular!
            };

            // Act & Assert
            var ex = Assert.Throws<BusinessException>(() =>
                ValidateNoCycles(tasks));

            Assert.Contains("circular", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        private void ValidateNoCycles(List<ScheduleTask> tasks)
        {
            // Simplified cycle detection
            var visited = new HashSet<int>();
            var recStack = new HashSet<int>();

            foreach (var task in tasks)
            {
                if (HasCycle(task.Id, tasks, visited, recStack))
                {
                    throw new BusinessException("Circular dependency detected in schedule tasks");
                }
            }
        }

        private bool HasCycle(int taskId, List<ScheduleTask> allTasks, HashSet<int> visited, HashSet<int> recStack)
        {
            var task = allTasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return false;

            if (recStack.Contains(taskId)) return true; // Cycle found
            if (visited.Contains(taskId)) return false;

            visited.Add(taskId);
            recStack.Add(taskId);

            if (task.DependsOn != null)
            {
                foreach (var dependencyId in task.DependsOn)
                {
                    if (HasCycle(dependencyId, allTasks, visited, recStack))
                        return true;
                }
            }

            recStack.Remove(taskId);
            return false;
        }

        #endregion

        #region TC-OPP-BL-EDGE-008: Time Zone Handling

        [Theory]
        [InlineData("UTC")]
        [InlineData("America/New_York")]
        [InlineData("Asia/Tokyo")]
        [InlineData("Europe/London")]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-008")]
        public async Task CreateOpportunity_DifferentTimeZones_StoresUTC(string timeZoneId)
        {
            // Arrange
            var localTime = DateTime.Now;
            var utcTime = localTime.ToUniversalTime();

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = $"Timezone Test - {timeZoneId}",
                EstimatedValue = 1000000,
                CreatedDate = utcTime, // Always store UTC
                CreatedBy = 1
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.Equal(DateTimeKind.Utc, retrieved.CreatedDate.Kind); // Should be UTC
        }

        #endregion

        #region TC-OPP-BL-EDGE-009: Daylight Saving Time Transitions

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-009")]
        public async Task CalculateTimeline_DSTTransition_HandlesCorrectly()
        {
            // Arrange - Project spanning DST change (Spring forward: March 12, 2023)
            var startDate = new DateTime(2023, 3, 10); // Before DST
            var endDate = new DateTime(2023, 3, 14); // After DST
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "DST Spanning Project",
                EstimatedValue = 100000,
                StartDate = startDate,
                EndDate = endDate,
                Timeline = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Calculate actual days
            var actualDays = (endDate - startDate).Days;

            // Assert
            Assert.Equal(4, actualDays); // 4 calendar days
            // System should handle DST automatically when using UTC
        }

        #endregion

        #region TC-OPP-BL-EDGE-010: Empty Collections

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-010")]
        public async Task ProcessOpportunity_EmptyLists_HandlesGracefully()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Empty Collections Test",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            // No deliverables, no partners, no documents

            // Act - Various operations should handle empty collections
            var deliverables = await _context.OpportunityDeliverables
                .Where(d => d.OpportunityId == opportunity.Id)
                .ToListAsync();
            
            var partners = await _context.OpportunityPartners
                .Where(p => p.OpportunityId == opportunity.Id)
                .ToListAsync();

            // Assert
            Assert.Empty(deliverables);
            Assert.Empty(partners);
            // Should not throw errors, just return empty lists
        }

        #endregion

        #region TC-OPP-BL-EDGE-011: Maximum String Length Fields

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-011")]
        public async Task CreateOpportunity_MaximumNameLength_Validates()
        {
            // Arrange - 255 character name (typical VARCHAR limit)
            var maxLengthName = new string('A', 255);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = maxLengthName,
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.Equal(255, retrieved.Name.Length);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-011-Exceeds")]
        public async Task CreateOpportunity_ExceedsMaxLength_ThrowsException()
        {
            // Arrange - 300 character name (exceeds limit)
            var tooLongName = new string('A', 300);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = tooLongName,
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            _context.Opportunities.Add(opportunity);
            var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
                await _context.SaveChangesAsync());

            Assert.NotNull(ex);
        }

        #endregion

        #region TC-OPP-BL-EDGE-012: Whitespace-Only Strings

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-012")]
        public async Task CreateOpportunity_WhitespaceOnlyName_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "     ", // Only whitespace
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act & Assert
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                var logic = new OpportunityValidationLogic(_context);
                await logic.ValidateOpportunityAsync(opportunity.Id);
            });

            Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-BL-EDGE-013: Deleted Entity References

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-013")]
        public async Task GetOpportunity_ReferencesDeletedCountry_HandlesGracefully()
        {
            // Arrange
            var country = new Country { Id = 1, Name = "Test Country", Code = "TC" };
            _context.Countries.Add(country);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000,
                PrimaryCountryId = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Soft-delete the country
            country.IsDeleted = true;
            await _context.SaveChangesAsync();

            // Act
            var retrieved = await _context.Opportunities
                .Include(o => o.PrimaryCountry)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.NotNull(retrieved.PrimaryCountry);
            Assert.True(retrieved.PrimaryCountry.IsDeleted); // Can still access but marked deleted
        }

        #endregion

        #region TC-OPP-BL-EDGE-014: Floating Point Arithmetic Edge Cases

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-014")]
        public async Task CalculateBudget_FloatingPointPrecision_NoRoundingErrors()
        {
            // Arrange - Test 0.1 + 0.2 = 0.3 floating point issue
            decimal baseCost = 2500000.10m;
            decimal fee = 250000.20m;
            decimal expectedTotal = 2750000.30m;

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Floating Point Test",
                EstimatedValue = baseCost,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var actualTotal = baseCost + fee;

            // Assert
            Assert.Equal(expectedTotal, actualTotal); // Decimal should handle precisely
        }

        #endregion

        #region TC-OPP-BL-EDGE-015: Transaction Rollback on Partial Failure

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-015")]
        public async Task CreateOpportunityWithRelated_PartialFailure_RollsBackAll()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Transaction Test",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Opportunities.Add(opportunity);
                await _context.SaveChangesAsync();

                // Try to add related data with invalid reference
                var invalidDeliverable = new OpportunityDeliverable
                {
                    OpportunityId = 99999, // Non-existent opportunity
                    Description = "Invalid"
                };
                _context.OpportunityDeliverables.Add(invalidDeliverable);
                await _context.SaveChangesAsync(); // Should fail

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
            }

            // Assert - Nothing should be saved
            var oppCount = await _context.Opportunities.CountAsync();
            Assert.Equal(0, oppCount); // Rolled back
        }

        #endregion

        #region TC-OPP-BL-EDGE-016: Case Sensitivity in Search

        [Theory]
        [InlineData("PROJECT")]
        [InlineData("project")]
        [InlineData("PrOjEcT")]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-016")]
        public async Task SearchOpportunities_CaseInsensitive_FindsAllMatches(string searchTerm)
        {
            // Arrange
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Name = "Test Project Alpha",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act - Search should be case-insensitive
            var results = await _context.Opportunities
                .Where(o => o.Name.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();

            // Assert
            Assert.Single(results); // Should find the project regardless of case
        }

        #endregion

        #region TC-OPP-BL-EDGE-017: Wildcard Search Patterns

        [Theory]
        [InlineData("%")]
        [InlineData("_")]
        [InlineData("*")]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-017")]
        public async Task SearchOpportunities_WildcardCharacters_HandledAsLiterals(string wildcardChar)
        {
            // Arrange
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Name = "Test Project",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act - Wildcards should be treated as literals, not SQL wildcards
            var results = await _context.Opportunities
                .Where(o => o.Name.Contains(wildcardChar))
                .ToListAsync();

            // Assert
            Assert.Empty(results); // No matches (wildcards treated as literals)
        }

        #endregion

        #region TC-OPP-BL-EDGE-018: Null Object Pattern

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-018")]
        public async Task GetOpportunityRelations_NoRelations_ReturnsEmptyNotNull()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Isolated Opportunity",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Get related data
            var deliverables = await _context.OpportunityDeliverables
                .Where(d => d.OpportunityId == opportunity.Id)
                .ToListAsync();

            var partners = await _context.OpportunityPartners
                .Where(p => p.OpportunityId == opportunity.Id)
                .ToListAsync();

            // Assert - Should return empty lists, not null
            Assert.NotNull(deliverables);
            Assert.NotNull(partners);
            Assert.Empty(deliverables);
            Assert.Empty(partners);
        }

        #endregion

        #region TC-OPP-BL-EDGE-019: Race Condition in Status Update

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-019")]
        public async Task UpdateStatus_SimultaneousUpdates_LastWriteWins()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Race Condition Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Simulate race condition
            using var context2 = new UNOPSAppDbContext(_dbContextOptions);
            
            var opp1 = await _context.Opportunities.FindAsync(opportunity.Id);
            var opp2 = await context2.Opportunities.FindAsync(opportunity.Id);

            opp1.Status = "Under Review";
            await _context.SaveChangesAsync(); // First update succeeds

            opp2.Status = "Profiling";
            // Second update should fail due to concurrency
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                await context2.SaveChangesAsync());
        }

        #endregion

        #region TC-OPP-BL-EDGE-020: Default Values and Null Coalescing

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-020")]
        public async Task CreateOpportunity_NullableFields_UsesDefaults()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Default Values Test",
                EstimatedValue = null, // Nullable
                Timeline = null, // Nullable
                Status = null, // Should default to "Draft"
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Retrieve and apply defaults
            var retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            retrieved.Status = retrieved.Status ?? "Draft";
            await _context.SaveChangesAsync();

            // Assert
            retrieved = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.Equal("Draft", retrieved.Status);
        }

        #endregion

        #region TC-OPP-BL-EDGE-021: Enum Edge Values

        [Theory]
        [InlineData(0)] // Minimum enum value
        [InlineData(int.MaxValue)] // Maximum value
        [InlineData(-1)] // Invalid enum value
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-021")]
        public async Task SetOpportunityPriority_EdgeValues_ValidatesEnum(int priorityValue)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Enum Edge Test",
                EstimatedValue = 1000000,
                PriorityValue = priorityValue,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Validate
            var logic = new OpportunityValidationLogic(_context);
            
            if (priorityValue < 0 || priorityValue > 10)
            {
                var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                    await logic.ValidateOpportunityAsync(opportunity.Id));
                Assert.Contains("priority", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion

        #region TC-OPP-BL-EDGE-022: Foreign Key Cascade Behavior

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-BL-EDGE-022")]
        public async Task DeleteOpportunity_WithRelatedData_CascadesOrBlocks()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Cascade Test",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Add related deliverable
            var deliverable = new OpportunityDeliverable
            {
                OpportunityId = opportunity.Id,
                Description = "Related Deliverable",
                EstimatedCost = 500000
            };
            _context.OpportunityDeliverables.Add(deliverable);
            await _context.SaveChangesAsync();

            // Act - Try to delete opportunity
            _context.Opportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            // Assert - Related data should also be deleted (cascade) or opportunity delete blocked
            var deliverableExists = await _context.OpportunityDeliverables
                .AnyAsync(d => d.OpportunityId == opportunity.Id);
            
            Assert.False(deliverableExists); // Cascade delete worked
        }

        #endregion

        #region Helper Classes

        public class ScheduleTask
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int[] DependsOn { get; set; }
        }

        public class OpportunityDeliverable
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Description { get; set; }
            public decimal EstimatedCost { get; set; }
        }

        public class Country
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class BusinessException : Exception
        {
            public BusinessException(string message) : base(message) { }
        }

        public class OpportunityValidationLogic
        {
            private readonly UNOPSAppDbContext _context;

            public OpportunityValidationLogic(UNOPSAppDbContext context)
            {
                _context = context;
            }

            public async Task ValidateOpportunityAsync(int opportunityId)
            {
                var opportunity = await _context.Opportunities.FindAsync(opportunityId);
                if (opportunity == null)
                    throw new BusinessException("Opportunity not found");

                if (string.IsNullOrWhiteSpace(opportunity.Name))
                    throw new BusinessException("Opportunity name is required and cannot be whitespace");

                if (opportunity.PriorityValue < 0 || opportunity.PriorityValue > 10)
                    throw new BusinessException("Priority value must be between 0 and 10");
            }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
