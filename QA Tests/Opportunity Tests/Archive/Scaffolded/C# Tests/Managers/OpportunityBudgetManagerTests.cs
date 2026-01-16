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

namespace UNOPS.PAO.Business.Tests.Opportunity.Managers
{
    /// <summary>
    /// Comprehensive tests for OpportunityBudgetManager
    /// Covers budget generation, fee calculations, phasing, and validations
    /// Based on OpportunityBudgetManager_TestCases.md (20+ tests)
    /// </summary>
    public class OpportunityBudgetManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly OpportunityBudgetManager _manager;

        public OpportunityBudgetManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"BudgetTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();

            _manager = new OpportunityBudgetManager(
                _mockMapper.Object,
                _context,
                _mockConfiguration.Object
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed opportunities
            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity
                {
                    Id = 1,
                    Name = "Infrastructure Project",
                    EstimatedValue = 2500000,
                    Timeline = 24, // months
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new Domain.Entities.Opportunity
                {
                    Id = 2,
                    Name = "Education Programme",
                    EstimatedValue = 5000000,
                    Timeline = 36,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                }
            });

            // Seed deliverables
            _context.OpportunityDeliverables.AddRange(new[]
            {
                new OpportunityDeliverable { Id = 1, OpportunityId = 1, Description = "Deliverable 1", EstimatedCost = 500000 },
                new OpportunityDeliverable { Id = 2, OpportunityId = 1, Description = "Deliverable 2", EstimatedCost = 800000 },
                new OpportunityDeliverable { Id = 3, OpportunityId = 1, Description = "Deliverable 3", EstimatedCost = 1200000 }
            });

            _context.SaveChanges();
        }

        #region TC-OPP-BUD-F-001: Generate High-Level Budget

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-001")]
        public async Task GenerateHighLevelBudget_WithDeliverables_Success()
        {
            // Arrange
            var opportunityId = 1;
            var feePercentage = 10m; // 10% fee

            // Act
            var budget = await _manager.GenerateBudgetAsync(opportunityId, feePercentage);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(opportunityId, budget.OpportunityId);
            
            // Calculate expected total
            var deliverablesCost = 500000 + 800000 + 1200000; // 2,500,000
            var expectedTotal = deliverablesCost * 1.10m; // 2,750,000 with 10% fee
            
            Assert.Equal(deliverablesCost, budget.BaseCost);
            Assert.Equal(feePercentage, budget.FeePercentage);
            Assert.Equal(deliverablesCost * (feePercentage / 100), budget.FeeAmount);
            Assert.Equal(expectedTotal, budget.TotalBudget);
        }

        #endregion

        #region TC-OPP-BUD-F-002: Calculate Fee from Partnership Agreement

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-002")]
        public async Task CalculateFee_FromPartnershipAgreement_UsesAgreementRate()
        {
            // Arrange
            var opportunityId = 1;
            
            // Create partnership agreement with specific fee
            var agreement = new PartnershipAgreement
            {
                Id = 1,
                Name = "MOU with Partner X",
                FeePercentage = 8m, // Agreement specifies 8%
                IsActive = true,
                ValidFrom = DateTime.UtcNow.AddYears(-1),
                ValidUntil = DateTime.UtcNow.AddYears(1)
            };
            _context.PartnershipAgreements.Add(agreement);

            // Link agreement to opportunity
            var opportunityAgreement = new OpportunityAgreement
            {
                OpportunityId = opportunityId,
                AgreementId = agreement.Id
            };
            _context.OpportunityAgreements.Add(opportunityAgreement);
            await _context.SaveChangesAsync();

            // Act - Generate budget (should use agreement fee, not default)
            var budget = await _manager.GenerateBudgetAsync(opportunityId);

            // Assert
            Assert.Equal(8m, budget.FeePercentage); // Uses agreement fee, not default 10%
            
            var deliverablesCost = 2500000m;
            var expectedFee = deliverablesCost * 0.08m; // 8% fee = 200,000
            Assert.Equal(expectedFee, budget.FeeAmount);
        }

        #endregion

        #region TC-OPP-BUD-F-003: Budget Phasing by Timeline

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-003")]
        public async Task GenerateBudgetPhasing_24MonthTimeline_Success()
        {
            // Arrange
            var opportunityId = 1; // 24-month timeline

            // Act
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            var phasing = await _manager.GenerateBudgetPhasingAsync(budget.Id);

            // Assert
            Assert.NotNull(phasing);
            
            // 24 months = 2 years
            Assert.Equal(2, phasing.Count); // 2 year phases
            
            // Typical phasing: Higher spend in implementation phase (Year 2)
            var year1 = phasing.First(p => p.Phase == "Year 1");
            var year2 = phasing.First(p => p.Phase == "Year 2");
            
            Assert.True(year2.Amount > year1.Amount); // More spent in Year 2
            
            // Total phasing equals total budget
            var totalPhased = phasing.Sum(p => p.Amount);
            Assert.Equal(budget.TotalBudget, totalPhased);
        }

        #endregion

        #region TC-OPP-BUD-F-004: Development vs Implementation Cost Segregation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-004")]
        public async Task GenerateBudget_SegregatesDevelopmentAndImplementation_Success()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);

            // Assert - Budget segregated
            Assert.NotNull(budget.DevelopmentCost); // Opportunity development costs
            Assert.NotNull(budget.ImplementationCost); // Project implementation costs
            
            // Development cost typically 3-5% of total
            var expectedDevCostRange = budget.BaseCost * 0.03m; // 3% minimum
            Assert.True(budget.DevelopmentCost >= expectedDevCostRange);
            
            // Implementation is the bulk
            Assert.True(budget.ImplementationCost > budget.DevelopmentCost);
            
            // Total adds up
            var segregatedTotal = budget.DevelopmentCost + budget.ImplementationCost + budget.FeeAmount;
            Assert.Equal(budget.TotalBudget, segregatedTotal);
        }

        #endregion

        #region TC-OPP-BUD-F-005: Personnel vs Non-Personnel Split

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-005")]
        public async Task GenerateBudget_SplitsPersonnelAndNonPersonnel_Success()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            var breakdown = await _manager.GetBudgetBreakdownAsync(budget.Id);

            // Assert
            Assert.NotNull(breakdown);
            Assert.Contains(breakdown, b => b.Category == "Personnel");
            Assert.Contains(breakdown, b => b.Category == "Non-Personnel");
            Assert.Contains(breakdown, b => b.Category == "Fee");
            
            var personnelCost = breakdown.First(b => b.Category == "Personnel").Amount;
            var nonPersonnelCost = breakdown.First(b => b.Category == "Non-Personnel").Amount;
            var feeCost = breakdown.First(b => b.Category == "Fee").Amount;
            
            // Personnel typically 40-60% of base cost
            Assert.True(personnelCost >= budget.BaseCost * 0.35m);
            Assert.True(personnelCost <= budget.BaseCost * 0.65m);
            
            // All categories add up
            var totalBreakdown = personnelCost + nonPersonnelCost + feeCost;
            Assert.Equal(budget.TotalBudget, totalBreakdown);
        }

        #endregion

        #region TC-OPP-BUD-V-001: Validate Budget Against Agreement Ceiling

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-BUD-V-001")]
        public async Task ValidateBudget_ExceedsAgreementCeiling_ThrowsException()
        {
            // Arrange
            var opportunityId = 2; // $5M opportunity
            
            // Partnership agreement with $3M ceiling
            var agreement = new PartnershipAgreement
            {
                Id = 2,
                Name = "MOU with Ceiling",
                BudgetCeiling = 3000000m,
                IsActive = true,
                ValidFrom = DateTime.UtcNow.AddYears(-1),
                ValidUntil = DateTime.UtcNow.AddYears(1)
            };
            _context.PartnershipAgreements.Add(agreement);
            
            var opportunityAgreement = new OpportunityAgreement
            {
                OpportunityId = opportunityId,
                AgreementId = agreement.Id
            };
            _context.OpportunityAgreements.Add(opportunityAgreement);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.GenerateBudgetAsync(opportunityId, 10m));

            Assert.Contains("agreement ceiling", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("$3,000,000", ex.Message);
        }

        #endregion

        #region TC-OPP-BUD-V-002: Zero Deliverables Validation

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-BUD-V-002")]
        public async Task GenerateBudget_NoDeliverables_ThrowsException()
        {
            // Arrange - Opportunity with no deliverables
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 99,
                Name = "No Deliverables Test",
                EstimatedValue = 1000000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.GenerateBudgetAsync(99, 10m));

            Assert.Contains("deliverables", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-BUD-V-003: Fee Percentage Validation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-BUD-V-003")]
        public async Task GenerateBudget_FeeExceeds50Percent_ThrowsException()
        {
            // Arrange
            var opportunityId = 1;

            // Act & Assert - Fee > 50% is unreasonable
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.GenerateBudgetAsync(opportunityId, 75m)); // 75% fee

            Assert.Contains("fee", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("50%", ex.Message);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-BUD-V-003-NegativeFee")]
        public async Task GenerateBudget_NegativeFee_ThrowsException()
        {
            // Arrange
            var opportunityId = 1;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.GenerateBudgetAsync(opportunityId, -5m)); // Negative fee

            Assert.Contains("positive", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-BUD-C-001: Calculate Spend Rate Visualization

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-C-001")]
        public async Task CalculateSpendRate_24Months_ReturnsMonthlyData()
        {
            // Arrange
            var opportunityId = 1; // 24-month timeline
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);

            // Act
            var spendRate = await _manager.CalculateSpendRateAsync(budget.Id);

            // Assert
            Assert.NotNull(spendRate);
            Assert.Equal(24, spendRate.MonthlyData.Count); // 24 months
            
            // S-curve spending pattern
            // Lower in early months, peak in middle, taper at end
            var earlyMonths = spendRate.MonthlyData.Take(6).Sum(m => m.Amount);
            var middleMonths = spendRate.MonthlyData.Skip(6).Take(12).Sum(m => m.Amount);
            var lateMonths = spendRate.MonthlyData.Skip(18).Take(6).Sum(m => m.Amount);
            
            Assert.True(middleMonths > earlyMonths); // Peak spending in middle
            Assert.True(middleMonths > lateMonths); // Peak spending in middle
            
            // Total equals budget
            var totalSpend = spendRate.MonthlyData.Sum(m => m.Amount);
            Assert.Equal(budget.TotalBudget, totalSpend);
        }

        #endregion

        #region TC-OPP-BUD-A-001: Budget Authorization

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Authorization")]
        [Trait("TestId", "TC-OPP-BUD-A-001")]
        public async Task AuthorizeBudget_ValidAuthorization_Success()
        {
            // Arrange
            var opportunityId = 1;
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);

            // Act
            var authorization = await _manager.AuthorizeBudgetAsync(budget.Id, authorizerId: 1);

            // Assert
            Assert.NotNull(authorization);
            Assert.Equal(budget.Id, authorization.BudgetId);
            Assert.Equal(1, authorization.AuthorizedBy);
            Assert.NotNull(authorization.AuthorizedDate);
            
            // Budget status updated
            var authorizedBudget = await _context.OpportunityBudgets.FindAsync(budget.Id);
            Assert.Equal("Authorized", authorizedBudget.Status);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Authorization")]
        [Trait("TestId", "TC-OPP-BUD-A-002")]
        public async Task AuthorizeBudget_AlreadyAuthorized_ThrowsException()
        {
            // Arrange
            var opportunityId = 1;
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            await _manager.AuthorizeBudgetAsync(budget.Id, authorizerId: 1);

            // Act & Assert - Second authorization should fail
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.AuthorizeBudgetAsync(budget.Id, authorizerId: 2));

            Assert.Contains("already authorized", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-BUD-E-001 to E-010: Edge Cases and Boundary Tests

        [Theory]
        [InlineData(1000)] // Very small budget
        [InlineData(100000000)] // Very large budget - $100M
        [InlineData(2500000)] // Medium budget
        [Trait("Category", "P2")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BUD-E-001")]
        public async Task GenerateBudget_VariousBudgetSizes_Success(decimal totalValue)
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 100,
                Name = $"Test {totalValue}",
                EstimatedValue = totalValue,
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            
            _context.OpportunityDeliverables.Add(new OpportunityDeliverable
            {
                OpportunityId = 100,
                Description = "Main Deliverable",
                EstimatedCost = totalValue * 0.90m // 90% of total
            });
            await _context.SaveChangesAsync();
            
            // Act
            var budget = await _manager.GenerateBudgetAsync(100, 10m);
            
            // Assert
            Assert.NotNull(budget);
            Assert.True(budget.TotalBudget > 0);
            Assert.True(budget.TotalBudget >= totalValue * 0.9m); // At least base cost
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BUD-E-002")]
        public async Task GenerateBudget_SingleDeliverable_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 101,
                Name = "Single Deliverable Project",
                EstimatedValue = 500000,
                Timeline = 6,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            
            _context.OpportunityDeliverables.Add(new OpportunityDeliverable
            {
                OpportunityId = 101,
                Description = "Only Deliverable",
                EstimatedCost = 500000
            });
            await _context.SaveChangesAsync();
            
            // Act
            var budget = await _manager.GenerateBudgetAsync(101, 10m);
            
            // Assert
            Assert.NotNull(budget);
            Assert.Single(await _context.OpportunityDeliverables.Where(d => d.OpportunityId == 101).ToListAsync());
            Assert.Equal(500000m * 1.10m, budget.TotalBudget); // 500K + 10% fee
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BUD-E-003")]
        public async Task GenerateBudget_ManyDeliverables_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 102,
                Name = "Many Deliverables Project",
                EstimatedValue = 10000000,
                Timeline = 48,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            
            // Add 50 deliverables
            for (int i = 1; i <= 50; i++)
            {
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = 102,
                    Description = $"Deliverable {i}",
                    EstimatedCost = 200000 // 200K each
                });
            }
            await _context.SaveChangesAsync();
            
            // Act
            var budget = await _manager.GenerateBudgetAsync(102, 10m);
            
            // Assert
            Assert.NotNull(budget);
            
            var totalDeliverables = 50 * 200000m; // 10M
            Assert.Equal(totalDeliverables, budget.BaseCost);
            Assert.Equal(50, await _context.OpportunityDeliverables.CountAsync(d => d.OpportunityId == 102));
        }

        [Theory]
        [InlineData(1)] // 1 month - very short
        [InlineData(6)] // 6 months
        [InlineData(24)] // 2 years
        [InlineData(60)] // 5 years - very long
        [Trait("Category", "P2")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-BUD-E-004")]
        public async Task GenerateBudgetPhasing_VariousTimelines_Success(int months)
        {
            // Arrange
            var opportunityId = 103 + months; // Unique ID
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = opportunityId,
                Name = $"Project {months} months",
                EstimatedValue = 1000000,
                Timeline = months,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            
            _context.OpportunityDeliverables.Add(new OpportunityDeliverable
            {
                OpportunityId = opportunityId,
                Description = "Deliverable",
                EstimatedCost = 1000000
            });
            await _context.SaveChangesAsync();
            
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            
            // Act
            var phasing = await _manager.GenerateBudgetPhasingAsync(budget.Id);
            
            // Assert
            Assert.NotNull(phasing);
            Assert.NotEmpty(phasing);
            
            // Phases should cover the timeline
            var expectedPhases = Math.Max(1, months / 12); // At least 1 phase, typically annual
            Assert.True(phasing.Count >= expectedPhases);
            
            // Total phasing equals budget
            var totalPhased = phasing.Sum(p => p.Amount);
            Assert.Equal(budget.TotalBudget, totalPhased);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-BUD-I-001")]
        public async Task UpdateBudget_RecalculatesTotal_Success()
        {
            // Arrange
            var opportunityId = 1;
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            var originalTotal = budget.TotalBudget;
            
            // Add a new deliverable
            _context.OpportunityDeliverables.Add(new OpportunityDeliverable
            {
                OpportunityId = opportunityId,
                Description = "Additional Deliverable",
                EstimatedCost = 500000
            });
            await _context.SaveChangesAsync();
            
            // Act - Regenerate budget
            var updatedBudget = await _manager.RegenerateBudgetAsync(budget.Id);
            
            // Assert
            Assert.NotNull(updatedBudget);
            Assert.True(updatedBudget.TotalBudget > originalTotal);
            Assert.Equal(originalTotal + (500000m * 1.10m), updatedBudget.TotalBudget);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-BUD-I-002")]
        public async Task CompareBudgetVersions_MultipleVersions_ShowsDifferences()
        {
            // Arrange
            var opportunityId = 1;
            
            // Version 1
            var budget_v1 = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            
            // Modify deliverables
            var deliverable = await _context.OpportunityDeliverables.FirstAsync(d => d.OpportunityId == opportunityId);
            deliverable.EstimatedCost += 200000; // Increase cost
            await _context.SaveChangesAsync();
            
            // Version 2
            var budget_v2 = await _manager.RegenerateBudgetAsync(budget_v1.Id);
            
            // Act
            var comparison = await _manager.CompareBudgetVersionsAsync(budget_v1.Id, budget_v2.Id);
            
            // Assert
            Assert.NotNull(comparison);
            Assert.NotEqual(comparison.Version1Total, comparison.Version2Total);
            Assert.True(comparison.Version2Total > comparison.Version1Total);
            Assert.Equal(220000m, comparison.Difference); // 200K * 1.10 (with fee)
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-006")]
        public async Task GenerateBudget_WithContingency_IncludesReserve()
        {
            // Arrange
            var opportunityId = 1;
            var contingencyPercentage = 5m; // 5% contingency reserve
            
            // Act
            var budget = await _manager.GenerateBudgetWithContingencyAsync(opportunityId, 10m, contingencyPercentage);
            
            // Assert
            Assert.NotNull(budget);
            Assert.NotNull(budget.ContingencyReserve);
            
            var expectedContingency = budget.BaseCost * (contingencyPercentage / 100);
            Assert.Equal(expectedContingency, budget.ContingencyReserve);
            
            // Total includes contingency
            var expectedTotal = budget.BaseCost + budget.FeeAmount + budget.ContingencyReserve.Value;
            Assert.Equal(expectedTotal, budget.TotalBudget);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-007")]
        public async Task ExportBudget_ToExcel_GeneratesFile()
        {
            // Arrange
            var opportunityId = 1;
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            
            // Act
            var exportResult = await _manager.ExportBudgetToExcelAsync(budget.Id);
            
            // Assert
            Assert.NotNull(exportResult);
            Assert.NotNull(exportResult.FileBytes);
            Assert.True(exportResult.FileBytes.Length > 0);
            Assert.Contains(".xlsx", exportResult.FileName);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportResult.ContentType);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-008")]
        public async Task ImportBudget_FromExternalSource_PopulatesFields()
        {
            // Arrange
            var opportunityId = 1;
            var externalBudgetData = new ExternalBudgetData
            {
                TotalBudget = 3000000m,
                FeePercentage = 8m,
                Breakdown = new Dictionary<string, decimal>
                {
                    { "Personnel", 1500000m },
                    { "Equipment", 800000m },
                    { "Travel", 450000m },
                    { "Fee", 250000m }
                }
            };
            
            // Act
            var budget = await _manager.ImportBudgetFromExternalSourceAsync(opportunityId, externalBudgetData);
            
            // Assert
            Assert.NotNull(budget);
            Assert.Equal(3000000m, budget.TotalBudget);
            Assert.Equal(8m, budget.FeePercentage);
            
            var breakdown = await _manager.GetBudgetBreakdownAsync(budget.Id);
            Assert.Equal(4, breakdown.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-BUD-V-004")]
        public async Task ValidateBudgetTotals_DeliverablesMismatch_FlagsWarning()
        {
            // Arrange
            var opportunityId = 1;
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            
            // Opportunity estimated value doesn't match deliverables total
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            opportunity.EstimatedValue = 5000000; // Different from deliverables total (2.5M)
            await _context.SaveChangesAsync();
            
            // Act
            var validation = await _manager.ValidateBudgetConsistencyAsync(budget.Id);
            
            // Assert
            Assert.False(validation.IsConsistent);
            Assert.Contains(validation.Warnings, w => w.Contains("estimated value"));
            Assert.Contains(validation.Warnings, w => w.Contains("deliverables"));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-BUD-P-001")]
        public async Task GenerateBudgets_ForMultipleOpportunities_CompleteInReasonableTime()
        {
            // Arrange - Create 10 opportunities
            var opportunities = new List<Domain.Entities.Opportunity>();
            for (int i = 200; i < 210; i++)
            {
                var opp = new Domain.Entities.Opportunity
                {
                    Id = i,
                    Name = $"Perf Test Opp {i}",
                    EstimatedValue = 1000000 + (i * 10000),
                    Timeline = 12,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                };
                opportunities.Add(opp);
                
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = i,
                    Description = "Deliverable",
                    EstimatedCost = 900000 + (i * 9000)
                });
            }
            
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();
            
            // Act - Generate budgets
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            foreach (var opp in opportunities)
            {
                await _manager.GenerateBudgetAsync(opp.Id, 10m);
            }
            
            stopwatch.Stop();
            
            // Assert - Should complete in under 5 seconds
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
                $"Budget generation took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-BUD-F-009")]
        public async Task GenerateBudget_WithCurrencyConversion_ConvertsCorrectly()
        {
            // Arrange
            var opportunityId = 1;
            var targetCurrency = "EUR";
            var exchangeRate = 0.85m; // 1 USD = 0.85 EUR
            
            var budget = await _manager.GenerateBudgetAsync(opportunityId, 10m);
            
            // Act
            var convertedBudget = await _manager.ConvertBudgetCurrencyAsync(budget.Id, targetCurrency, exchangeRate);
            
            // Assert
            Assert.NotNull(convertedBudget);
            Assert.Equal(targetCurrency, convertedBudget.Currency);
            Assert.Equal(budget.TotalBudget * exchangeRate, convertedBudget.TotalBudget);
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

        public class PartnershipAgreement
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal? FeePercentage { get; set; }
            public decimal? BudgetCeiling { get; set; }
            public bool IsActive { get; set; }
            public DateTime ValidFrom { get; set; }
            public DateTime ValidUntil { get; set; }
        }

        public class OpportunityAgreement
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int AgreementId { get; set; }
        }

        public class BudgetBreakdown
        {
            public string Category { get; set; }
            public decimal Amount { get; set; }
        }

        public class BudgetPhasing
        {
            public string Phase { get; set; }
            public decimal Amount { get; set; }
        }

        public class SpendRateData
        {
            public List<MonthlySpend> MonthlyData { get; set; }
        }

        public class MonthlySpend
        {
            public int Month { get; set; }
            public decimal Amount { get; set; }
        }

        public class BudgetComparisonResult
        {
            public decimal Version1Total { get; set; }
            public decimal Version2Total { get; set; }
            public decimal Difference { get; set; }
        }

        public class ExternalBudgetData
        {
            public decimal TotalBudget { get; set; }
            public decimal FeePercentage { get; set; }
            public Dictionary<string, decimal> Breakdown { get; set; }
        }

        public class BudgetValidationResult
        {
            public bool IsConsistent { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public class ExportResult
        {
            public byte[] FileBytes { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
        }

        public class ConvertedBudget
        {
            public string Currency { get; set; }
            public decimal TotalBudget { get; set; }
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
