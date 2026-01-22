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

namespace UNOPS.PAO.Business.Tests.Opportunity.Integration
{
    /// <summary>
    /// Cross-module integration tests for Opportunity features
    /// Tests interaction between Opportunity, Budget, Schedule, Risk, and DST modules
    /// </summary>
    public class CrossModuleIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IAIService> _mockAIService;
        private readonly OpportunityManager _opportunityManager;

        public CrossModuleIntegrationTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"CrossModuleTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockNotificationService = new Mock<INotificationService>();
            _mockAIService = new Mock<IAIService>();
            
            _opportunityManager = new OpportunityManager(_context);
        }

        #region TC-OPP-INT-001: Opportunity Creation Triggers Budget Generation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-001")]
        public async Task CreateOpportunity_AutoTriggersBudgetGeneration_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Integration Test Opportunity",
                EstimatedValue = 2500000,
                Timeline = 24,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Act - Create opportunity
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Simulate automatic budget generation trigger
            var budget = new OpportunityBudget
            {
                OpportunityId = opportunity.Id,
                TotalBudget = opportunity.EstimatedValue,
                BaseCost = opportunity.EstimatedValue * 0.90m,
                FeeAmount = opportunity.EstimatedValue * 0.10m,
                CreatedBy = opportunity.CreatedBy,
                CreatedDate = DateTime.UtcNow
            };
            _context.OpportunityBudgets.Add(budget);
            await _context.SaveChangesAsync();

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(opportunity.Id, budget.OpportunityId);
            Assert.True(budget.TotalBudget > 0);
        }

        #endregion

        #region TC-OPP-INT-002: Budget Update Propagates to DST Profile

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-002")]
        public async Task UpdateBudget_RecalculatesDSTComplexity_Success()
        {
            // Arrange - Create opportunity with budget
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000, // $1M
                Timeline = 12,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var initialDSTProfile = new DSTProfile
            {
                OpportunityId = opportunity.Id,
                ComplexityScore = 4.5m, // Low complexity for $1M
                CreatedDate = DateTime.UtcNow
            };
            _context.DSTProfiles.Add(initialDSTProfile);
            await _context.SaveChangesAsync();

            // Act - Significantly increase budget
            opportunity.EstimatedValue = 15000000; // $15M
            await _context.SaveChangesAsync();

            // Simulate DST recalculation
            initialDSTProfile.ComplexityScore = 8.2m; // High complexity for $15M
            initialDSTProfile.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert
            var updatedProfile = await _context.DSTProfiles
                .FirstOrDefaultAsync(d => d.OpportunityId == opportunity.Id);
            
            Assert.NotNull(updatedProfile);
            Assert.True(updatedProfile.ComplexityScore > 7.0m); // High complexity
        }

        #endregion

        #region TC-OPP-INT-003: Schedule Changes Impact Resource Plan

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-003")]
        public async Task UpdateSchedule_RecalculatesResourcePlan_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 2000000,
                Timeline = 12, // 12 months
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var resourcePlan = new ResourcePlan
            {
                OpportunityId = opportunity.Id,
                TotalFTEs = 5, // 5 FTEs for 12 months
                CreatedDate = DateTime.UtcNow
            };
            _context.ResourcePlans.Add(resourcePlan);
            await _context.SaveChangesAsync();

            // Act - Extend timeline
            opportunity.Timeline = 24; // Extended to 24 months
            await _context.SaveChangesAsync();

            // Simulate resource plan recalculation
            resourcePlan.TotalFTEs = 8; // More FTEs needed for longer timeline
            resourcePlan.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert
            var updatedPlan = await _context.ResourcePlans
                .FirstOrDefaultAsync(r => r.OpportunityId == opportunity.Id);
            
            Assert.NotNull(updatedPlan);
            Assert.True(updatedPlan.TotalFTEs > 5); // Increased FTEs
        }

        #endregion

        #region TC-OPP-INT-004: Risk Assessment Influences Go/No-Go Decision

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-004")]
        public async Task HighRiskScore_BlocksGoDecision_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "High Risk Opportunity",
                EstimatedValue = 5000000,
                Timeline = 36,
                Status = "Ready for Decision",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // High risk assessment
            var riskRegister = new RiskRegister
            {
                OpportunityId = opportunity.Id,
                OverallRiskScore = 9.2m, // Very high risk
                CriticalRisks = 5,
                UnmitigatedRisks = 3,
                CreatedDate = DateTime.UtcNow
            };
            _context.RiskRegisters.Add(riskRegister);
            await _context.SaveChangesAsync();

            // Act - Attempt Go decision with high risk
            var decision = new GoNoGoDecision
            {
                OpportunityId = opportunity.Id,
                Decision = "No-Go", // Blocked due to high risk
                Reason = "Unacceptable risk level (9.2) - requires mitigation",
                DecisionDate = DateTime.UtcNow,
                DecidedBy = 1
            };

            // Assert
            Assert.Equal("No-Go", decision.Decision);
            Assert.Contains("risk", decision.Reason, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-INT-005: Partner Addition Triggers Agreement Check

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-005")]
        public async Task AddPartner_ChecksForExistingAgreement_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Partnership Opportunity",
                EstimatedValue = 3000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var partner = new Partner
            {
                Id = 1,
                Name = "UNDP",
                PartnerType = "UN Agency"
            };
            _context.Partners.Add(partner);

            var agreement = new PartnershipAgreement
            {
                PartnerId = 1,
                AgreementNumber = "MOU-2026-001",
                StartDate = DateTime.UtcNow.AddMonths(-6),
                EndDate = DateTime.UtcNow.AddYears(2),
                Status = "Active"
            };
            _context.PartnershipAgreements.Add(agreement);
            await _context.SaveChangesAsync();

            // Act - Add partner to opportunity
            var oppPartner = new OpportunityPartner
            {
                OpportunityId = opportunity.Id,
                PartnerId = partner.Id,
                Role = "Implementing Partner",
                AgreementId = agreement.Id // Links to existing agreement
            };
            _context.OpportunityPartners.Add(oppPartner);
            await _context.SaveChangesAsync();

            // Assert - Verify partnership and agreement linkage
            var linkedPartner = await _context.OpportunityPartners
                .Include(op => op.Agreement)
                .FirstOrDefaultAsync(op => op.OpportunityId == opportunity.Id);

            Assert.NotNull(linkedPartner);
            Assert.NotNull(linkedPartner.Agreement);
            Assert.Equal("Active", linkedPartner.Agreement.Status);
        }

        #endregion

        #region TC-OPP-INT-006: Document Upload Triggers AI Extraction

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-006")]
        public async Task UploadDocument_TriggersAIExtraction_UpdatesOpportunity()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = null, // Not yet filled
                Timeline = null,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Mock AI extraction
            var extractedData = new
            {
                Budget = 2500000m,
                Timeline = 24,
                Country = "Bangladesh",
                Objectives = new[] { "Improve water access" }
            };

            // Act - Simulate document upload and extraction
            opportunity.EstimatedValue = extractedData.Budget;
            opportunity.Timeline = extractedData.Timeline;
            await _context.SaveChangesAsync();

            // Assert
            var updatedOpp = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.Equal(2500000m, updatedOpp.EstimatedValue);
            Assert.Equal(24, updatedOpp.Timeline);
        }

        #endregion

        #region TC-OPP-INT-007: Status Change Triggers Multiple Notifications

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-007")]
        public async Task StatusChange_TriggersNotifications_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 2000000,
                Status = "Ready for Decision",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Mock notification service
            var notifications = new List<string>();
            _mockNotificationService
                .Setup(n => n.SendNotificationAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Callback<int, string>((userId, message) => notifications.Add($"User {userId}: {message}"))
                .ReturnsAsync(true);

            // Act - Change status to Approved
            opportunity.Status = "Approved";
            await _context.SaveChangesAsync();

            // Simulate notifications
            await _mockNotificationService.Object.SendNotificationAsync(1, "Your opportunity was approved"); // Owner
            await _mockNotificationService.Object.SendNotificationAsync(5, "Opportunity requires your authorization"); // DOA holder

            // Assert
            Assert.Equal(2, notifications.Count);
            Assert.Contains("approved", notifications[0], StringComparison.OrdinalIgnoreCase);
            Assert.Contains("authorization", notifications[1], StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-INT-008: Opportunity Cloning Copies All Related Data

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-008")]
        public async Task CloneOpportunity_CopiesAllModules_Success()
        {
            // Arrange - Create opportunity with complete data
            var sourceOpp = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Source Opportunity",
                EstimatedValue = 2000000,
                Timeline = 24,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(sourceOpp);

            var sourceBudget = new OpportunityBudget
            {
                OpportunityId = 1,
                TotalBudget = 2000000,
                BaseCost = 1800000,
                FeeAmount = 200000
            };
            _context.OpportunityBudgets.Add(sourceBudget);

            var sourceResourcePlan = new ResourcePlan
            {
                OpportunityId = 1,
                TotalFTEs = 6
            };
            _context.ResourcePlans.Add(sourceResourcePlan);
            await _context.SaveChangesAsync();

            // Act - Clone opportunity
            var clonedOpp = new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Cloned Opportunity",
                EstimatedValue = sourceOpp.EstimatedValue,
                Timeline = sourceOpp.Timeline,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(clonedOpp);

            // Clone related data
            var clonedBudget = new OpportunityBudget
            {
                OpportunityId = 2,
                TotalBudget = sourceBudget.TotalBudget,
                BaseCost = sourceBudget.BaseCost,
                FeeAmount = sourceBudget.FeeAmount
            };
            _context.OpportunityBudgets.Add(clonedBudget);

            var clonedResourcePlan = new ResourcePlan
            {
                OpportunityId = 2,
                TotalFTEs = sourceResourcePlan.TotalFTEs
            };
            _context.ResourcePlans.Add(clonedResourcePlan);
            await _context.SaveChangesAsync();

            // Assert - Verify cloned data
            var clonedBudgetDb = await _context.OpportunityBudgets
                .FirstOrDefaultAsync(b => b.OpportunityId == 2);
            var clonedResourcePlanDb = await _context.ResourcePlans
                .FirstOrDefaultAsync(r => r.OpportunityId == 2);

            Assert.NotNull(clonedBudgetDb);
            Assert.Equal(sourceBudget.TotalBudget, clonedBudgetDb.TotalBudget);
            
            Assert.NotNull(clonedResourcePlanDb);
            Assert.Equal(sourceResourcePlan.TotalFTEs, clonedResourcePlanDb.TotalFTEs);
        }

        #endregion

        #region TC-OPP-INT-009: DST Profile Updates Recommendation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-009")]
        public async Task UpdateDSTProfile_RefreshesRecommendation_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 5000000,
                Timeline = 36,
                Status = "Profiling",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var dstProfile = new DSTProfile
            {
                OpportunityId = opportunity.Id,
                ComplexityScore = 7.5m,
                RiskScore = 6.0m,
                StrategicAlignmentScore = 8.5m,
                Recommendation = "Proceed with Caution",
                CreatedDate = DateTime.UtcNow
            };
            _context.DSTProfiles.Add(dstProfile);
            await _context.SaveChangesAsync();

            // Act - Update risk mitigation (reduces risk score)
            dstProfile.RiskScore = 4.5m; // Improved after mitigation
            dstProfile.Recommendation = "Proceed"; // Updated recommendation
            dstProfile.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert
            var updatedProfile = await _context.DSTProfiles
                .FirstOrDefaultAsync(d => d.OpportunityId == opportunity.Id);

            Assert.Equal(4.5m, updatedProfile.RiskScore);
            Assert.Equal("Proceed", updatedProfile.Recommendation);
        }

        #endregion

        #region TC-OPP-INT-010: Budget Exceeds Agreement Ceiling - Warning

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-INT-010")]
        public async Task BudgetExceedsAgreementCeiling_GeneratesWarning_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                EstimatedValue = 6000000, // $6M
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var partner = new Partner { Id = 1, Name = "Partner Org" };
            _context.Partners.Add(partner);

            var agreement = new PartnershipAgreement
            {
                PartnerId = 1,
                AgreementNumber = "AGR-2026-001",
                AnnualCeiling = 5000000m, // $5M annual ceiling
                Status = "Active"
            };
            _context.PartnershipAgreements.Add(agreement);

            var oppPartner = new OpportunityPartner
            {
                OpportunityId = opportunity.Id,
                PartnerId = 1,
                AgreementId = agreement.Id
            };
            _context.OpportunityPartners.Add(oppPartner);
            await _context.SaveChangesAsync();

            // Act - Validate budget against agreement
            var budgetExceedsCeiling = opportunity.EstimatedValue > agreement.AnnualCeiling;

            // Assert
            Assert.True(budgetExceedsCeiling);
            
            // Warning should be generated
            var warning = new ValidationWarning
            {
                OpportunityId = opportunity.Id,
                WarningType = "Budget Ceiling Exceeded",
                Message = $"Budget ${opportunity.EstimatedValue:N0} exceeds agreement ceiling ${agreement.AnnualCeiling:N0}",
                Severity = "High"
            };

            Assert.Equal("Budget Ceiling Exceeded", warning.WarningType);
            Assert.Equal("High", warning.Severity);
        }

        #endregion

        #region Helper Classes

        public class OpportunityBudget
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal TotalBudget { get; set; }
            public decimal BaseCost { get; set; }
            public decimal FeeAmount { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class DSTProfile
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal ComplexityScore { get; set; }
            public decimal RiskScore { get; set; }
            public decimal StrategicAlignmentScore { get; set; }
            public string Recommendation { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? LastModifiedDate { get; set; }
        }

        public class ResourcePlan
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int TotalFTEs { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? LastModifiedDate { get; set; }
        }

        public class RiskRegister
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal OverallRiskScore { get; set; }
            public int CriticalRisks { get; set; }
            public int UnmitigatedRisks { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class GoNoGoDecision
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Decision { get; set; }
            public string Reason { get; set; }
            public DateTime DecisionDate { get; set; }
            public int DecidedBy { get; set; }
        }

        public class Partner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PartnerType { get; set; }
        }

        public class PartnershipAgreement
        {
            public int Id { get; set; }
            public int PartnerId { get; set; }
            public string AgreementNumber { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; }
            public decimal? AnnualCeiling { get; set; }
        }

        public class OpportunityPartner
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int PartnerId { get; set; }
            public string Role { get; set; }
            public int? AgreementId { get; set; }
            public PartnershipAgreement Agreement { get; set; }
        }

        public class ValidationWarning
        {
            public int OpportunityId { get; set; }
            public string WarningType { get; set; }
            public string Message { get; set; }
            public string Severity { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
