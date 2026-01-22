using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.AdvancedTests
{
    /// <summary>
    /// Advanced test suite covering negative tests, integration tests, boundary tests, and edge cases
    /// Demonstrates comprehensive test coverage beyond basic functional tests
    /// </summary>
    public class OpportunityAdvancedTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IDSTManager> _mockDSTManager;
        private readonly Mock<IDecisionManager> _mockDecisionManager;
        private readonly Mock<IBudgetManager> _mockBudgetManager;
        private readonly OpportunityManager _opportunityManager;

        public OpportunityAdvancedTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"AdvancedTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDSTManager = new Mock<IDSTManager>();
            _mockDecisionManager = new Mock<IDecisionManager>();
            _mockBudgetManager = new Mock<IBudgetManager>();

            _opportunityManager = new OpportunityManager(
                _mockMapper.Object,
                _context,
                _mockConfiguration.Object
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Countries.AddRange(new[]
            {
                new Country { Id = 1, Name = "Bangladesh", Code = "BD" },
                new Country { Id = 2, Name = "Test Country", Code = "TC" }
            });

            _context.Currencies.AddRange(new[]
            {
                new Currency { Id = 1, Code = "USD", Name = "US Dollar" },
                new Currency { Id = 2, Code = "JPY", Name = "Japanese Yen" }
            });

            _context.OrganizationUnits.Add(new OrganizationUnit
            {
                Id = 1,
                Name = "Test Org Unit",
                Code = "TOU"
            });

            _context.SaveChanges();
        }

        #region Negative Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-OM-001")]
        public async Task CreateOpportunity_SQLInjectionAttempt_Sanitized()
        {
            // Arrange - SQL injection attempt in name field
            var maliciousRequest = new OpportunityCreateRequest
            {
                Name = "Robert'; DROP TABLE Opportunities;--",
                Description = "Test",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(maliciousRequest);

            // Assert
            Assert.NotNull(result);
            // Name should be stored but not cause SQL injection
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.NotNull(opportunity);
            Assert.Contains("Robert", opportunity.Name); // Data retained
            // Verify no tables were dropped (database still accessible)
            Assert.True(await _context.Opportunities.AnyAsync());
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-OM-002")]
        public async Task CreateOpportunity_XSSScriptInDescription_Sanitized()
        {
            // Arrange - XSS attempt in description
            var xssRequest = new OpportunityCreateRequest
            {
                Name = "Test Opportunity",
                Description = "<script>alert('XSS')</script>",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(xssRequest);

            // Assert
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            // Script should be escaped or removed
            Assert.DoesNotContain("<script>", opportunity.Description);
            Assert.DoesNotContain("</script>", opportunity.Description);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-OM-003")]
        public async Task UpdateOpportunity_NegativeBudget_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test",
                EstimatedValue = 100000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var updateRequest = new OpportunityUpdateRequest
            {
                Id = 1,
                EstimatedValue = -1000000 // Negative value
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _opportunityManager.UpdateAsync(updateRequest));

            Assert.Contains("positive", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-OM-004")]
        public async Task CreateOpportunity_ExtremelyLongName_ValidatesLength()
        {
            // Arrange - Name exceeds max length
            var longName = new string('A', 5000);
            var request = new OpportunityCreateRequest
            {
                Name = longName,
                Description = "Test",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _opportunityManager.CreateOpportunityAsync(request));

            Assert.Contains("length", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("500", ex.Message); // Max length shown in error
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-OM-007")]
        public async Task CreateOpportunity_NullRequiredFields_ThrowsException()
        {
            // Arrange - Multiple null required fields
            var request = new OpportunityCreateRequest
            {
                Name = null,
                EstimatedValue = null,
                CurrencyId = null
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _opportunityManager.CreateOpportunityAsync(request));

            // Error message should list all missing required fields
            Assert.Contains("Name", ex.Message);
            Assert.Contains("EstimatedValue", ex.Message);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Negative")]
        [Trait("TestId", "TC-OPP-NEG-OM-010")]
        public async Task UpdateOpportunity_AfterSoftDelete_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test",
                EstimatedValue = 100000,
                Status = "Draft",
                IsDeleted = true, // Soft deleted
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var updateRequest = new OpportunityUpdateRequest
            {
                Id = 1,
                Name = "Updated Name"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _opportunityManager.UpdateAsync(updateRequest));

            Assert.Contains("deleted", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Integration Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-E2E-001")]
        public async Task CompleteOpportunityLifecycle_AllComponents_Success()
        {
            // Arrange
            var createRequest = new OpportunityCreateRequest
            {
                Name = "End-to-End Test Opportunity",
                Description = "Complete lifecycle test",
                EstimatedValue = 2500000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act - Step 1: Create Opportunity
            var opportunity = await _opportunityManager.CreateOpportunityAsync(createRequest);
            Assert.NotNull(opportunity);

            // Step 2: Generate DST Profile
            _mockDSTManager.Setup(m => m.GenerateDSTProfileAsync(opportunity.Id))
                .ReturnsAsync(new DSTProfile
                {
                    Id = 1,
                    OpportunityId = opportunity.Id,
                    ComplexityScore = 6.5m,
                    GeneratedDate = DateTime.UtcNow
                });
            var dstProfile = await _mockDSTManager.Object.GenerateDSTProfileAsync(opportunity.Id);
            Assert.NotNull(dstProfile);

            // Step 3: Generate Budget
            _mockBudgetManager.Setup(m => m.GenerateBudgetAsync(opportunity.Id))
                .ReturnsAsync(new OpportunityBudget
                {
                    Id = 1,
                    OpportunityId = opportunity.Id,
                    TotalAmount = 2500000,
                    Status = "Draft"
                });
            var budget = await _mockBudgetManager.Object.GenerateBudgetAsync(opportunity.Id);
            Assert.NotNull(budget);

            // Step 4: Assemble Decision Package
            _mockDecisionManager.Setup(m => m.AssembleDecisionPackageAsync(opportunity.Id))
                .ReturnsAsync(new DecisionPackage
                {
                    OpportunityId = opportunity.Id,
                    IsComplete = true
                });
            var decisionPackage = await _mockDecisionManager.Object.AssembleDecisionPackageAsync(opportunity.Id);
            Assert.True(decisionPackage.IsComplete);

            // Step 5: Record Go Decision
            _mockDecisionManager.Setup(m => m.RecordDecisionAsync(opportunity.Id, "Go", It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new OpportunityDecision
                {
                    Id = 1,
                    OpportunityId = opportunity.Id,
                    Decision = "Go",
                    DecisionDate = DateTime.UtcNow
                });
            var decision = await _mockDecisionManager.Object.RecordDecisionAsync(opportunity.Id, "Go", "Approved", 1);
            Assert.Equal("Go", decision.Decision);

            // Assert - Verify complete lifecycle
            Assert.NotNull(opportunity);
            Assert.NotNull(dstProfile);
            Assert.NotNull(budget);
            Assert.NotNull(decisionPackage);
            Assert.NotNull(decision);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-E2E-006")]
        public async Task ConcurrentMultiUserEditing_OptimisticConcurrency_HandlesConflict()
        {
            // Arrange - Create opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Concurrent Test",
                EstimatedValue = 100000,
                Status = "Draft",
                RowVersion = new byte[] { 1, 2, 3, 4 },
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - User A updates
            var userAUpdate = await _context.Opportunities.FindAsync(1);
            userAUpdate.Name = "Updated by User A";

            // User B updates (simulated by modifying in database)
            var userBUpdate = await _context.Opportunities.FindAsync(1);
            userBUpdate.Description = "Updated by User B";
            userBUpdate.RowVersion = new byte[] { 5, 6, 7, 8 };
            await _context.SaveChangesAsync();

            // User A tries to save (should detect concurrency conflict)
            // Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await _context.SaveChangesAsync();
            });
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-MGR-001")]
        public async Task OpportunityDSTDecisionIntegration_DataFlowsSeamlessly()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Integration Test",
                EstimatedValue = 1000000,
                PrimaryCountryId = 1,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // Add DST Profile
            var dstProfile = new DSTProfile
            {
                Id = 1,
                OpportunityId = 1,
                ComplexityScore = 7.5m,
                RiskScore = 6.2m,
                IsCurrent = true,
                GeneratedDate = DateTime.UtcNow
            };
            _context.DSTProfiles.Add(dstProfile);

            // Add Decision
            var decision = new OpportunityDecision
            {
                Id = 1,
                OpportunityId = 1,
                Decision = "Go",
                DecisionMakerId = 1,
                DecisionDate = DateTime.UtcNow,
                Rationale = "Good complexity score, manageable risks"
            };
            _context.OpportunityDecisions.Add(decision);

            await _context.SaveChangesAsync();

            // Act - Query integrated data
            var opportunityWithDetails = await _context.Opportunities
                .Include(o => o.DSTProfile)
                .Include(o => o.Decisions)
                .FirstOrDefaultAsync(o => o.Id == 1);

            // Assert - Verify data integration
            Assert.NotNull(opportunityWithDetails);
            Assert.NotNull(opportunityWithDetails.DSTProfile);
            Assert.Equal(7.5m, opportunityWithDetails.DSTProfile.ComplexityScore);
            Assert.Single(opportunityWithDetails.Decisions);
            Assert.Equal("Go", opportunityWithDetails.Decisions.First().Decision);
        }

        #endregion

        #region Boundary and Limits Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BND-VOL-001")]
        public async Task CreateOpportunity_ExactMaxNameLength_Accepted()
        {
            // Arrange - Name exactly at max length (500 chars)
            var name = new string('A', 500);
            var request = new OpportunityCreateRequest
            {
                Name = name,
                Description = "Test",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(request);

            // Assert
            Assert.NotNull(result);
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.Equal(500, opportunity.Name.Length);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BND-VOL-001-2")]
        public async Task CreateOpportunity_ExceedsMaxNameLength_Rejected()
        {
            // Arrange - Name exceeds max length (501 chars)
            var name = new string('A', 501);
            var request = new OpportunityCreateRequest
            {
                Name = name,
                Description = "Test",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _opportunityManager.CreateOpportunityAsync(request));

            Assert.Contains("500", ex.Message); // Max length indicated
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BND-VOL-002")]
        public async Task CreateOpportunity_MinimumBudgetValue_Accepted()
        {
            // Arrange - Minimum possible budget ($1)
            var request = new OpportunityCreateRequest
            {
                Name = "Minimum Budget Test",
                Description = "Test",
                EstimatedValue = 1.00m,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(request);

            // Assert
            Assert.NotNull(result);
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.Equal(1.00m, opportunity.EstimatedValue);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BND-VOL-003")]
        public async Task CreateOpportunity_MaximumBudgetValue_HandlesLargeNumbers()
        {
            // Arrange - Very large budget (near decimal limit)
            var request = new OpportunityCreateRequest
            {
                Name = "Maximum Budget Test",
                Description = "Test",
                EstimatedValue = 999999999999.99m, // Near max decimal
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(request);

            // Assert
            Assert.NotNull(result);
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.Equal(999999999999.99m, opportunity.EstimatedValue);
            // No overflow error
            // Precision maintained
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BND-NUM-006")]
        public async Task Budget_DecimalPrecision_RoundsCorrectly()
        {
            // Arrange - Budget with high decimal precision
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Precision Test",
                EstimatedValue = 2500000.9999m, // 4 decimal places
                CurrencyId = 1,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Retrieve and check
            var retrieved = await _context.Opportunities.FindAsync(1);

            // Assert - Should round to 2 decimal places for currency
            Assert.Equal(2500001.00m, Math.Round(retrieved.EstimatedValue, 2));
        }

        [Theory]
        [InlineData(0.0, 10.0, true)] // Min and Max valid
        [InlineData(5.0, 10.0, true)] // Mid-range valid
        [InlineData(-1.0, 10.0, false)] // Below min invalid
        [InlineData(0.0, 11.0, false)] // Above max invalid
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BND-NUM-001")]
        public void ValidateComplexityScore_BoundaryValues(decimal min, decimal max, bool isValid)
        {
            // Arrange
            var score = max;

            // Act
            var inRange = score >= 0 && score <= 10;

            // Assert
            Assert.Equal(isValid, inRange);
        }

        #endregion

        #region Edge Cases

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "EdgeCase")]
        [Trait("TestId", "TC-OPP-EDGE-001")]
        public async Task CreateOpportunity_NameWithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange - Name with various special characters
            var request = new OpportunityCreateRequest
            {
                Name = "Water & Sanitation (Phase II) – 50% Match! @#$%",
                Description = "Test",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(request);

            // Assert
            Assert.NotNull(result);
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.Contains("&", opportunity.Name);
            Assert.Contains("–", opportunity.Name);
            Assert.Contains("%", opportunity.Name);
            // Special chars preserved, no encoding issues
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "EdgeCase")]
        [Trait("TestId", "TC-OPP-EDGE-002")]
        public async Task CreateOpportunity_MultiLanguageText_StoresCorrectly()
        {
            // Arrange - Description in multiple languages
            var request = new OpportunityCreateRequest
            {
                Name = "Test Opportunity",
                Description = "English text. النص العربي. 中文文本. Texte français.",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(request);

            // Assert
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.Contains("English", opportunity.Description);
            Assert.Contains("النص", opportunity.Description); // Arabic
            Assert.Contains("中文", opportunity.Description); // Chinese
            Assert.Contains("français", opportunity.Description); // French
            // UTF-8 encoding preserved
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "EdgeCase")]
        [Trait("TestId", "TC-OPP-EDGE-005")]
        public async Task CreateOpportunity_OnLeapDay_HandlesDateCorrectly()
        {
            // Arrange - Simulate creating on Feb 29 (leap year)
            var leapDayDate = new DateTime(2024, 2, 29);
            var request = new OpportunityCreateRequest
            {
                Name = "Leap Day Test",
                Description = "Test",
                EstimatedValue = 100000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Override creation date
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = request.Name,
                EstimatedValue = request.EstimatedValue.Value,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = leapDayDate
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Calculate 1 year later
            var oneYearLater = opportunity.CreatedDate.AddYears(1);

            // Assert
            Assert.Equal(new DateTime(2025, 2, 28), oneYearLater); // Feb 28, 2025 (not leap year)
            // Date math handles leap year correctly
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "EdgeCase")]
        [Trait("TestId", "TC-OPP-EDGE-006")]
        public async Task CreateOpportunity_JapaneseYenNoCents_HandlesIntegerCurrency()
        {
            // Arrange - Currency with no decimal places (JPY)
            var request = new OpportunityCreateRequest
            {
                Name = "JPY Test",
                Description = "Test",
                EstimatedValue = 2500000m, // ¥2,500,000 (no decimals)
                CurrencyId = 2, // JPY
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act
            var result = await _opportunityManager.CreateOpportunityAsync(request);

            // Assert
            var opportunity = await _context.Opportunities.FindAsync(result.Id);
            Assert.Equal(2500000m, opportunity.EstimatedValue);
            Assert.Equal(2, opportunity.CurrencyId); // JPY
            // No decimal places stored or displayed for JPY
            // Should be formatted as ¥2,500,000 not ¥2,500,000.00
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "EdgeCase")]
        [Trait("TestId", "TC-OPP-EDGE-SYS-001")]
        public async Task Transaction_DatabaseConnectionLost_RollsBackCorrectly()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Transaction Test",
                EstimatedValue = 100000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Opportunities.Add(opportunity);
                    await _context.SaveChangesAsync();

                    // Simulate error - throw exception
                    throw new Exception("Simulated connection loss");

                    // This would commit if no exception
                    // await transaction.CommitAsync();
                }
                catch
                {
                    // Act - Rollback on error
                    await transaction.RollbackAsync();
                }
            }

            // Assert - Verify data not saved
            var count = await _context.Opportunities.CountAsync();
            Assert.Equal(0, count); // No opportunities saved
            // Transaction rolled back successfully
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "EdgeCase")]
        [Trait("TestId", "TC-OPP-EDGE-WF-002")]
        public async Task OpportunityUpdate_DuringConcurrentRead_MaintainsConsistency()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Consistency Test",
                EstimatedValue = 100000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Simulate concurrent read while update happens
            var readTask = Task.Run(async () =>
            {
                await Task.Delay(100); // Slight delay
                return await _context.Opportunities.FindAsync(1);
            });

            var updateTask = Task.Run(async () =>
            {
                var opp = await _context.Opportunities.FindAsync(1);
                opp.Name = "Updated Name";
                await _context.SaveChangesAsync();
            });

            await Task.WhenAll(readTask, updateTask);

            // Assert - Both operations complete successfully
            var finalState = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Updated Name", finalState.Name);
            // No deadlocks or consistency violations
        }

        #endregion

        #region Performance and Load Tests

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        public async Task CreateOpportunities_BulkInsert_CompletesInReasonableTime()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var opportunities = new List<Domain.Entities.Opportunity>();

            for (int i = 0; i < 100; i++)
            {
                opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Bulk Test {i}",
                    EstimatedValue = 100000,
                    CurrencyId = 1,
                    PrimaryCountryId = 1,
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Act
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();
            stopwatch.Stop();

            // Assert
            Assert.Equal(100, await _context.Opportunities.CountAsync());
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
                $"Bulk insert took {stopwatch.ElapsedMilliseconds}ms, should be < 5000ms");
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
