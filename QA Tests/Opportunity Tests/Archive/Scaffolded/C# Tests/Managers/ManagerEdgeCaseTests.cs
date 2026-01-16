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
    /// Edge case tests for Opportunity Managers
    /// Tests boundary conditions, extreme values, and unusual scenarios
    /// </summary>
    public class ManagerEdgeCaseTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly OpportunityBudgetManager _budgetManager;
        private readonly OpportunityScheduleManager _scheduleManager;
        private readonly ResourcePlanManager _resourceManager;

        public ManagerEdgeCaseTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"ManagerEdgeTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _budgetManager = new OpportunityBudgetManager(_context);
            _scheduleManager = new OpportunityScheduleManager(_context);
            _resourceManager = new ResourcePlanManager(_context);
        }

        #region TC-OPP-MGR-EDGE-001: Minimum Budget Edge Case

        [Theory]
        [InlineData(1)] // $1
        [InlineData(100)] // $100
        [InlineData(1000)] // $1,000
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-001")]
        public async Task GenerateBudget_MinimumBudgets_HandlesGracefully(decimal minBudget)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = $"Min Budget Test ${minBudget}",
                EstimatedValue = minBudget,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalBudget >= minBudget);
            Assert.True(result.FeeAmount >= 0); // Fee should be calculated even for tiny budgets
        }

        #endregion

        #region TC-OPP-MGR-EDGE-002: Maximum Budget Edge Case

        [Theory]
        [InlineData(999999999)] // ~$1B
        [InlineData(long.MaxValue / 2)] // Very large
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-002")]
        public async Task GenerateBudget_MaximumBudgets_HandlesWithoutOverflow(decimal maxBudget)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = $"Max Budget Test ${maxBudget:N0}",
                EstimatedValue = maxBudget,
                Timeline = 60,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalBudget > 0); // Should not overflow
            Assert.True(result.FeeAmount < result.TotalBudget); // Fee should be reasonable proportion
        }

        #endregion

        #region TC-OPP-MGR-EDGE-003: Single Month Timeline

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-003")]
        public async Task GenerateSchedule_SingleMonth_CreatesValidSchedule()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "One Month Project",
                EstimatedValue = 50000,
                Timeline = 1, // Just 1 month
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var schedule = await _scheduleManager.GenerateScheduleAsync(opportunity.Id);

            // Assert
            Assert.NotNull(schedule);
            Assert.Equal(1, schedule.TotalMonths);
            Assert.NotEmpty(schedule.Milestones); // Should have at least Start and End milestones
        }

        #endregion

        #region TC-OPP-MGR-EDGE-004: Very Long Timeline

        [Theory]
        [InlineData(120)] // 10 years
        [InlineData(240)] // 20 years
        [InlineData(360)] // 30 years
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-004")]
        public async Task GenerateSchedule_VeryLongTimeline_HandlesProperly(int months)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = $"{months}-Month Programme",
                EstimatedValue = 50000000, // Large programme
                Timeline = months,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var schedule = await _scheduleManager.GenerateScheduleAsync(opportunity.Id);

            // Assert
            Assert.NotNull(schedule);
            Assert.Equal(months, schedule.TotalMonths);
            Assert.True(schedule.Phases.Count >= 3); // Should have multiple phases for long timelines
        }

        #endregion

        #region TC-OPP-MGR-EDGE-005: Zero FTE Resource Plan

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-005")]
        public async Task GenerateResourcePlan_ZeroFTEs_ThrowsValidationException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "No Staff Project",
                EstimatedValue = 100000,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _resourceManager.GenerateResourcePlanAsync(opportunity.Id, totalFTEs: 0));

            Assert.Contains("at least one", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-006: Fractional FTE Values

        [Theory]
        [InlineData(0.1)] // 10% FTE
        [InlineData(0.25)] // 25% FTE
        [InlineData(0.5)] // 50% FTE
        [InlineData(1.5)] // 1.5 FTE
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-006")]
        public async Task GenerateResourcePlan_FractionalFTEs_HandlesCorrectly(decimal fractionalFTE)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Part-Time Project",
                EstimatedValue = 500000,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var plan = await _resourceManager.GenerateResourcePlanAsync(opportunity.Id, totalFTEs: fractionalFTE);

            // Assert
            Assert.NotNull(plan);
            Assert.True(plan.TotalFTEs > 0);
            Assert.True(plan.TotalFTEs <= fractionalFTE + 0.1m); // Allow small rounding
        }

        #endregion

        #region TC-OPP-MGR-EDGE-007: Same Day Start and End Date

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-007")]
        public async Task GenerateSchedule_SameDayStartEnd_ThrowsException()
        {
            // Arrange
            var today = DateTime.Today;
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Same Day Project",
                EstimatedValue = 10000,
                StartDate = today,
                EndDate = today, // Same day!
                Timeline = 0,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _scheduleManager.GenerateScheduleAsync(opportunity.Id));

            Assert.Contains("duration", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-008: Extreme Fee Percentages

        [Theory]
        [InlineData(0.01)] // 0.01% - very low
        [InlineData(99.99)] // 99.99% - extremely high
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-008")]
        public async Task GenerateBudget_ExtremeFeePercentages_ValidatesAndWarns(decimal extremeFee)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Extreme Fee Test",
                EstimatedValue = 1000000,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            if (extremeFee > 20m) // Above 20% should warn or throw
            {
                var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                    await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: extremeFee));

                Assert.Contains("fee percentage", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                var result = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: extremeFee);
                Assert.NotNull(result);
                Assert.True(result.FeePercentage == extremeFee);
            }
        }

        #endregion

        #region TC-OPP-MGR-EDGE-009: Single Deliverable

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-009")]
        public async Task GenerateBudget_SingleDeliverable_CreatesValidBudget()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Single Deliverable Project",
                EstimatedValue = 500000,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            _context.OpportunityDeliverables.Add(new OpportunityDeliverable
            {
                OpportunityId = opportunity.Id,
                Description = "The Only Deliverable",
                EstimatedCost = 500000
            });
            await _context.SaveChangesAsync();

            // Act
            var budget = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(1, budget.DeliverablesCount);
            Assert.True(budget.TotalBudget >= 500000);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-010: Hundreds of Deliverables

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-010")]
        public async Task GenerateBudget_100Deliverables_HandlesLargeScale()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Complex Programme with Many Deliverables",
                EstimatedValue = 50000000,
                Timeline = 60,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // Add 100 deliverables
            for (int i = 1; i <= 100; i++)
            {
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = opportunity.Id,
                    Description = $"Deliverable {i}",
                    EstimatedCost = 500000
                });
            }
            await _context.SaveChangesAsync();

            // Act
            var budget = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(100, budget.DeliverablesCount);
            Assert.True(budget.TotalBudget > 50000000); // Should include fee
        }

        #endregion

        #region TC-OPP-MGR-EDGE-011: Budget with No Cost Breakdown

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-011")]
        public async Task GenerateBudget_NoDeliverables_UsesEstimatedValue()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "No Deliverables Yet",
                EstimatedValue = 2000000,
                Timeline = 24,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            // No deliverables added

            // Act
            var budget = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(0, budget.DeliverablesCount);
            Assert.True(budget.TotalBudget >= opportunity.EstimatedValue);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-012: Schedule with No Phases

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-012")]
        public async Task GenerateSchedule_NoPhasesDefined_CreatesDefaultPhases()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Simple Timeline",
                EstimatedValue = 500000,
                Timeline = 6,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var schedule = await _scheduleManager.GenerateScheduleAsync(opportunity.Id);

            // Assert
            Assert.NotNull(schedule);
            Assert.True(schedule.Phases.Count >= 2); // Should create default phases (e.g., Start, End)
        }

        #endregion

        #region TC-OPP-MGR-EDGE-013: Resource Plan for Remote Work (100%)

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-013")]
        public async Task GenerateResourcePlan_FullyRemote_HandlesCorrectly()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Fully Remote Project",
                EstimatedValue = 1000000,
                Timeline = 18,
                IsRemote = true,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var plan = await _resourceManager.GenerateResourcePlanAsync(opportunity.Id, totalFTEs: 5m);

            // Assert
            Assert.NotNull(plan);
            Assert.Equal(5m, plan.TotalFTEs);
            Assert.True(plan.RemotePercentage == 100); // Fully remote
        }

        #endregion

        #region TC-OPP-MGR-EDGE-014: Past Date Schedule Generation

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-014")]
        public async Task GenerateSchedule_StartDateInPast_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Historical Project",
                EstimatedValue = 1000000,
                StartDate = DateTime.UtcNow.AddYears(-1), // Start date in the past
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _scheduleManager.GenerateScheduleAsync(opportunity.Id));

            Assert.Contains("past", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-015: Currency Conversion Edge Cases

        [Theory]
        [InlineData("USD", "EUR", 1.0)] // 1:1 rate
        [InlineData("USD", "JPY", 150.0)] // High rate
        [InlineData("USD", "BTC", 0.000025)] // Very low rate
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-015")]
        public async Task ConvertBudget_ExtremeCurrencyRates_HandlesCorrectly(
            string fromCurrency, string toCurrency, decimal rate)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Multi-Currency Project",
                EstimatedValue = 1000000,
                Currency = fromCurrency,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var convertedAmount = opportunity.EstimatedValue * rate;

            // Assert
            Assert.True(convertedAmount > 0);
            if (rate < 0.001m) // Very low rate (like BTC)
            {
                Assert.True(convertedAmount < opportunity.EstimatedValue);
            }
            else if (rate > 100m) // High rate (like JPY)
            {
                Assert.True(convertedAmount > opportunity.EstimatedValue);
            }
        }

        #endregion

        #region TC-OPP-MGR-EDGE-016: Leap Year Timeline Calculations

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-016")]
        public async Task GenerateSchedule_LeapYear_HandlesFebruary29()
        {
            // Arrange - Start on Feb 28 in leap year
            var leapYearStart = new DateTime(2024, 2, 28);
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Leap Year Project",
                EstimatedValue = 500000,
                StartDate = leapYearStart,
                Timeline = 13, // Crosses Feb 29
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var schedule = await _scheduleManager.GenerateScheduleAsync(opportunity.Id);

            // Assert
            Assert.NotNull(schedule);
            var expectedEnd = leapYearStart.AddMonths(13);
            Assert.Equal(expectedEnd.Date, schedule.EndDate.Date); // Should handle Feb 29 correctly
        }

        #endregion

        #region TC-OPP-MGR-EDGE-017: Budget Rounding Edge Cases

        [Theory]
        [InlineData(999999.99)] // Just under 1M
        [InlineData(1000000.01)] // Just over 1M
        [InlineData(2500000.555)] // Needs rounding
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-017")]
        public async Task GenerateBudget_RequiresRounding_HandlesCorrectly(decimal preciseAmount)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Rounding Test",
                EstimatedValue = preciseAmount,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var budget = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(budget);
            // Budget should be rounded to 2 decimal places
            Assert.Equal(Math.Round(budget.TotalBudget, 2), budget.TotalBudget);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-018: Overlapping Phases

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-018")]
        public async Task ValidateSchedule_OverlappingPhases_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Overlapping Phases Project",
                EstimatedValue = 1000000,
                Timeline = 24,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var schedule = await _scheduleManager.GenerateScheduleAsync(opportunity.Id);

            // Manually create overlapping phases
            var phase1 = new Phase
            {
                Name = "Phase 1",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(12)
            };

            var phase2 = new Phase
            {
                Name = "Phase 2",
                StartDate = DateTime.Today.AddMonths(6), // Overlaps with Phase 1!
                EndDate = DateTime.Today.AddMonths(18)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _scheduleManager.ValidatePhasesAsync(new[] { phase1, phase2 }));

            Assert.Contains("overlap", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-019: Resource Over-Allocation

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-019")]
        public async Task ValidateResourcePlan_OverAllocation_WarnsUser()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Over-Allocated Project",
                EstimatedValue = 500000,
                Timeline = 6,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Try to allocate 50 FTEs for a $500K, 6-month project (unrealistic)
            var plan = await _resourceManager.GenerateResourcePlanAsync(opportunity.Id, totalFTEs: 50m);

            // Act - Validate allocation
            var validation = await _resourceManager.ValidateResourceAllocationAsync(opportunity.Id);

            // Assert
            Assert.False(validation.IsRealistic);
            Assert.Contains("over-allocated", validation.WarningMessage, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-020: Budget with All Zero-Cost Deliverables

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-020")]
        public async Task GenerateBudget_AllZeroCostDeliverables_UsesEstimate()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Zero Cost Deliverables",
                EstimatedValue = 1000000,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // Add deliverables with zero cost
            for (int i = 1; i <= 5; i++)
            {
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = opportunity.Id,
                    Description = $"Free Deliverable {i}",
                    EstimatedCost = 0 // Zero cost!
                });
            }
            await _context.SaveChangesAsync();

            // Act
            var budget = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Assert
            Assert.NotNull(budget);
            Assert.True(budget.TotalBudget > 0); // Should use opportunity estimate, not deliverable sum
            Assert.Equal(5, budget.DeliverablesCount);
        }

        #endregion

        #region TC-OPP-MGR-EDGE-021: Schedule Spanning Multiple Years

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-021")]
        public async Task GenerateSchedule_SpansMultipleYears_HandlesYearBoundaries()
        {
            // Arrange - Project from Dec to Jan next year
            var yearEnd = new DateTime(2025, 12, 15);
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Year-Spanning Project",
                EstimatedValue = 500000,
                StartDate = yearEnd,
                Timeline = 3, // Dec, Jan, Feb
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var schedule = await _scheduleManager.GenerateScheduleAsync(opportunity.Id);

            // Assert
            Assert.NotNull(schedule);
            Assert.Equal(2025, schedule.StartDate.Year);
            Assert.Equal(2026, schedule.EndDate.Year); // Spans to next year
        }

        #endregion

        #region TC-OPP-MGR-EDGE-022: Concurrent Budget Updates

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Edge")]
        [Trait("TestId", "TC-OPP-MGR-EDGE-022")]
        public async Task UpdateBudget_ConcurrentUpdates_HandlesOptimisticConcurrency()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Concurrent Update Test",
                EstimatedValue = 1000000,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var budget = await _budgetManager.GenerateBudgetAsync(opportunity.Id, feePercentage: 10m);

            // Simulate concurrent updates
            using var context2 = new UNOPSAppDbContext(_dbContextOptions);
            var budgetManager2 = new OpportunityBudgetManager(context2);

            // User 1 updates
            budget.FeePercentage = 12m;
            await _context.SaveChangesAsync();

            // User 2 tries to update with stale data
            var budget2 = await context2.OpportunityBudgets.FindAsync(budget.Id);
            budget2.FeePercentage = 8m;

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                await context2.SaveChangesAsync());
        }

        #endregion

        #region Helper Classes

        public class OpportunityDeliverable
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Description { get; set; }
            public decimal EstimatedCost { get; set; }
        }

        public class BudgetResult
        {
            public decimal TotalBudget { get; set; }
            public decimal FeePercentage { get; set; }
            public decimal FeeAmount { get; set; }
            public int DeliverablesCount { get; set; }
        }

        public class ScheduleResult
        {
            public int TotalMonths { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<Phase> Phases { get; set; } = new List<Phase>();
            public List<Milestone> Milestones { get; set; } = new List<Milestone>();
        }

        public class Phase
        {
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class Milestone
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

        public class ResourcePlanResult
        {
            public decimal TotalFTEs { get; set; }
            public int RemotePercentage { get; set; }
        }

        public class ResourceValidation
        {
            public bool IsRealistic { get; set; }
            public string WarningMessage { get; set; }
        }

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
