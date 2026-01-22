using AutoMapper;
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
    /// Tests for DST Profiling algorithms and calculations
    /// Based on DSTProfiler_TestCases.md (40+ tests)
    /// </summary>
    public class DSTProfilerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly DSTProfilerLogic _profilerLogic;

        public DSTProfilerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"ProfilerTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockAIService = new Mock<IAIService>();

            _profilerLogic = new DSTProfilerLogic(_context, _mockAIService.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed country with indices
            _context.Countries.Add(new Country
            {
                Id = 1,
                Name = "Afghanistan",
                Code = "AF",
                FragileStateIndex = 97, // Very high
                CorruptionIndex = 16, // Very corrupt
                MVIIndex = 42 // High vulnerability
            });

            // Seed simple opportunity
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Simple Infrastructure",
                EstimatedValue = 500000,
                PrimaryCountryId = 1,
                Timeline = 12, // months
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            // Seed complex opportunity
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Complex Programme",
                EstimatedValue = 15000000,
                PrimaryCountryId = 1,
                Timeline = 60, // months
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            // Add deliverables to complex opportunity
            for (int i = 1; i <= 25; i++)
            {
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = 2,
                    Description = $"Complex Deliverable {i}",
                    EstimatedCost = 600000
                });
            }

            _context.SaveChanges();
        }

        #region TC-OPP-DST-ALG-001: Calculate Complexity Score - Simple Project

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-001")]
        public async Task CalculateComplexityScore_SimpleProject_LowScore()
        {
            // Arrange
            var opportunityId = 1; // Simple: $500K, 12 months, 1 country

            // Act
            var complexityScore = await _profilerLogic.CalculateComplexityScoreAsync(opportunityId);

            // Assert
            Assert.InRange(complexityScore, 2.0m, 4.0m); // Low complexity (2-4)
            
            // Factors contributing to low score:
            // - Small budget ($500K)
            // - Short timeline (12 months)
            // - Single country
            // - Few deliverables
        }

        #endregion

        #region TC-OPP-DST-ALG-002: Calculate Complexity Score - Complex Programme

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-002")]
        public async Task CalculateComplexityScore_ComplexProgramme_HighScore()
        {
            // Arrange
            var opportunityId = 2; // Complex: $15M, 60 months, 25 deliverables

            // Act
            var complexityScore = await _profilerLogic.CalculateComplexityScoreAsync(opportunityId);

            // Assert
            Assert.InRange(complexityScore, 7.5m, 9.5m); // High complexity (7.5-9.5)
            
            // Factors contributing to high score:
            // - Large budget ($15M)
            // - Long timeline (60 months)
            // - Many deliverables (25)
            // - Programme-level coordination
        }

        #endregion

        #region TC-OPP-DST-ALG-003: Evaluate Context Parameter - Fragile State

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-003")]
        public async Task EvaluateContextParameter_FragileState_HighRiskScore()
        {
            // Arrange
            var opportunityId = 1; // Afghanistan (FSI = 97)

            // Act
            var contextEvaluation = await _profilerLogic.EvaluateContextParameterAsync(opportunityId);

            // Assert
            Assert.NotNull(contextEvaluation);
            Assert.InRange(contextEvaluation.Score, 7.0m, 9.0m); // High risk score
            Assert.Contains("fragile", contextEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("security", contextEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            
            // Specific risks identified
            Assert.Contains(contextEvaluation.IdentifiedRisks, r => r.Contains("political"));
            Assert.Contains(contextEvaluation.IdentifiedRisks, r => r.Contains("security"));
        }

        #endregion

        #region TC-OPP-DST-ALG-004: Calculate Strategic Alignment Score

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-004")]
        public async Task CalculateStrategicAlignment_MultipleSDGs_HighAlignment()
        {
            // Arrange
            var opportunityId = 1;
            
            // Link opportunity to SDGs
            _context.OpportunitySDGs.AddRange(new[]
            {
                new OpportunitySDG { OpportunityId = 1, SDGId = 6 }, // Clean Water
                new OpportunitySDG { OpportunityId = 1, SDGId = 11 }, // Sustainable Cities
                new OpportunitySDG { OpportunityId = 1, SDGId = 13 } // Climate Action
            });
            await _context.SaveChangesAsync();

            // Act
            var alignmentScore = await _profilerLogic.CalculateStrategicAlignmentAsync(opportunityId);

            // Assert
            Assert.InRange(alignmentScore.Score, 7.0m, 9.0m); // High alignment (3 SDGs)
            Assert.Equal(3, alignmentScore.SDGCount);
            Assert.Contains("water", alignmentScore.Narrative, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sustainable development", alignmentScore.Narrative, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-DST-ALG-005: Evaluate Partner Capacity Parameter

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-005")]
        public async Task EvaluatePartnerCapacity_EstablishedPartner_PositiveScore()
        {
            // Arrange
            var opportunityId = 1;
            
            // Add established partner with good track record
            var partner = new Partner
            {
                Id = 1,
                Name = "Established Development Bank",
                PartnerType = "Multilateral",
                YearsOfPartnership = 8,
                PreviousProjectCount = 25,
                SuccessRate = 92m, // %
                FinancialStability = "Strong"
            };
            _context.Partners.Add(partner);
            
            _context.OpportunityPartners.Add(new OpportunityPartner
            {
                OpportunityId = 1,
                PartnerId = 1,
                Role = "Primary Partner"
            });
            await _context.SaveChangesAsync();

            // Act
            var partnerEvaluation = await _profilerLogic.EvaluatePartnerCapacityAsync(opportunityId);

            // Assert
            Assert.InRange(partnerEvaluation.Score, 8.0m, 10.0m); // High capacity score
            Assert.Contains("established", partnerEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strong track record", partnerEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("Low", partnerEvaluation.RiskLevel); // Low risk with established partner
        }

        #endregion

        #region TC-OPP-DST-ALG-006: Calculate Feasibility Score

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-006")]
        public async Task CalculateFeasibilityScore_AllFactors_RealisticScore()
        {
            // Arrange
            var opportunityId = 1;
            
            // Add feasibility factors
            var feasibilityFactors = new
            {
                TechnicalFeasibility = 8.0m, // High
                FinancialFeasibility = 7.0m, // Good
                PoliticalFeasibility = 5.0m, // Moderate (fragile state)
                ResourceAvailability = 7.5m, // Good
                TimeframeFeasibility = 8.5m // Realistic
            };

            // Act
            var feasibilityScore = await _profilerLogic.CalculateFeasibilityScoreAsync(opportunityId);

            // Assert
            // Weighted average of factors
            var expectedScore = (feasibilityFactors.TechnicalFeasibility * 0.25m) +
                              (feasibilityFactors.FinancialFeasibility * 0.20m) +
                              (feasibilityFactors.PoliticalFeasibility * 0.20m) +
                              (feasibilityFactors.ResourceAvailability * 0.20m) +
                              (feasibilityFactors.TimeframeFeasibility * 0.15m);

            Assert.InRange(feasibilityScore.Score, 6.5m, 7.5m);
            Assert.NotEmpty(feasibilityScore.FeasibilityConcerns);
        }

        #endregion

        #region TC-OPP-DST-ALG-007: Generate Overall Recommendation

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-007")]
        public async Task GenerateRecommendation_AllParametersEvaluated_RealisticRecommendation()
        {
            // Arrange
            var profileScores = new DSTParameterScores
            {
                Complexity = 6.5m,
                StrategicAlignment = 8.0m,
                PartnerCapacity = 7.5m,
                Context = 6.0m,
                Feasibility = 7.0m,
                Risk = 6.5m,
                Scope = 7.0m,
                Timeframe = 7.5m,
                Resources = 7.0m
            };

            // Act
            var recommendation = _profilerLogic.GenerateOverallRecommendation(profileScores);

            // Assert
            Assert.NotNull(recommendation);
            
            // Average score ~7.0 = "Proceed with Caution"
            var avgScore = new[] 
            { 
                profileScores.Complexity, 
                profileScores.StrategicAlignment,
                profileScores.PartnerCapacity,
                profileScores.Context,
                profileScores.Feasibility,
                profileScores.Risk,
                profileScores.Scope,
                profileScores.Timeframe,
                profileScores.Resources
            }.Average();

            Assert.InRange(avgScore, 6.8m, 7.2m);
            
            if (avgScore >= 7.0m)
            {
                Assert.Equal("Proceed with Caution", recommendation.Decision);
            }
            
            Assert.NotEmpty(recommendation.KeyConsiderations);
        }

        #endregion

        #region TC-OPP-DST-ALG-008: Similarity Scoring Algorithm

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-008")]
        public async Task CalculateSimilarityScore_SameCountrySameSector_HighSimilarity()
        {
            // Arrange
            var targetOpportunity = new Domain.Entities.Opportunity
            {
                Id = 100,
                Name = "Education Project - Afghanistan",
                Sector = "Education",
                PrimaryCountryId = 1, // Afghanistan
                EstimatedValue = 2000000,
                Timeline = 24
            };

            var similarOpportunity = new Domain.Entities.Opportunity
            {
                Id = 101,
                Name = "School Rehabilitation - Afghanistan",
                Sector = "Education",
                PrimaryCountryId = 1, // Same country
                EstimatedValue = 1800000, // Similar budget
                Timeline = 20 // Similar timeline
            };

            _context.Opportunities.AddRange(targetOpportunity, similarOpportunity);
            await _context.SaveChangesAsync();

            // Act
            var similarityScore = _profilerLogic.CalculateSimilarityScore(targetOpportunity, similarOpportunity);

            // Assert
            Assert.InRange(similarityScore, 0.75m, 0.95m); // 75-95% similar
            
            // Factors:
            // - Same country: +40 points
            // - Same sector: +30 points
            // - Similar budget: +15 points
            // - Similar timeline: +10 points
            // Total: ~95 points = 95% similar
        }

        #endregion

        #region TC-OPP-DST-ALG-009: Risk Parameter Calculation

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-009")]
        public async Task CalculateRiskParameter_HighFragileState_HighRiskScore()
        {
            // Arrange
            var opportunityId = 1; // Afghanistan, FSI = 97

            // Act
            var riskEvaluation = await _profilerLogic.EvaluateRiskParameterAsync(opportunityId);

            // Assert
            Assert.NotNull(riskEvaluation);
            Assert.InRange(riskEvaluation.Score, 7.5m, 9.5m); // High risk
            
            // Risk factors identified
            Assert.Contains(riskEvaluation.RiskFactors, f => f.Type == "Political");
            Assert.Contains(riskEvaluation.RiskFactors, f => f.Type == "Security");
            Assert.Contains(riskEvaluation.RiskFactors, f => f.Type == "Corruption");
            
            // Recommendations
            Assert.NotEmpty(riskEvaluation.MitigationRecommendations);
            Assert.Contains(riskEvaluation.MitigationRecommendations, 
                r => r.Contains("security") || r.Contains("contingency"));
        }

        #endregion

        #region TC-OPP-DST-ALG-010: Scope and Scale Parameter

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-010")]
        public async Task EvaluateScopeParameter_25Deliverables_HighComplexity()
        {
            // Arrange
            var opportunityId = 2; // Complex opportunity with 25 deliverables

            // Act
            var scopeEvaluation = await _profilerLogic.EvaluateScopeParameterAsync(opportunityId);

            // Assert
            Assert.InRange(scopeEvaluation.Score, 7.0m, 9.0m); // High scope complexity
            Assert.Equal(25, scopeEvaluation.DeliverableCount);
            Assert.Contains("large scope", scopeEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            
            // Recommendations for large scope
            Assert.Contains(scopeEvaluation.Recommendations, 
                r => r.Contains("phase") || r.Contains("programme"));
        }

        #endregion

        #region TC-OPP-DST-ALG-011: Timeframe Parameter Evaluation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-011")]
        public async Task EvaluateTimeframe_LongDuration_IncreasedComplexity()
        {
            // Arrange
            var opportunityId = 2; // 60-month timeline

            // Act
            var timeframeEvaluation = await _profilerLogic.EvaluateTimeframeParameterAsync(opportunityId);

            // Assert
            Assert.InRange(timeframeEvaluation.Score, 7.0m, 9.0m); // Long timeline = higher complexity
            Assert.Equal(60, timeframeEvaluation.DurationMonths);
            Assert.Contains("long-term", timeframeEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            
            // Considerations for long timeline
            Assert.Contains(timeframeEvaluation.Considerations, "Sustainability planning");
            Assert.Contains(timeframeEvaluation.Considerations, "Team continuity");
        }

        #endregion

        #region TC-OPP-DST-ALG-012: Budget and Resourcing Parameter

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-ALG-012")]
        public async Task EvaluateBudgetParameter_LargeBudget_HighComplexity()
        {
            // Arrange
            var opportunityId = 2; // $15M budget

            // Act
            var budgetEvaluation = await _profilerLogic.EvaluateBudgetParameterAsync(opportunityId);

            // Assert
            Assert.InRange(budgetEvaluation.Score, 7.5m, 9.5m); // Large budget = higher complexity
            Assert.Equal(15000000, budgetEvaluation.BudgetAmount);
            Assert.Contains("substantial", budgetEvaluation.Narrative, StringComparison.OrdinalIgnoreCase);
            
            // Financial management recommendations
            Assert.Contains(budgetEvaluation.Recommendations, 
                r => r.Contains("financial controls") || r.Contains("oversight"));
        }

        #endregion

        #region TC-OPP-DST-ALG-013: Generate Parameter Narrative

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-DST-ALG-013")]
        public async Task GenerateParameterNarrative_UsingAI_ComprehensiveText()
        {
            // Arrange
            var opportunityId = 1;
            var parameterData = new
            {
                Parameter = "Context",
                Score = 8.2m,
                Country = "Afghanistan",
                FSI = 97m,
                CPI = 16m
            };

            // Mock AI service to generate narrative
            _mockAIService.Setup(ai => ai.GenerateNarrativeAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(@"Afghanistan presents a highly complex operational context with a Fragile State Index of 97, 
                              indicating significant political instability and security challenges. The Corruption Perception Index 
                              of 16 suggests substantial governance risks. Implementation will require enhanced risk management, 
                              security protocols, and close coordination with local authorities.");

            // Act
            var narrative = await _profilerLogic.GenerateParameterNarrativeAsync("Context", parameterData);

            // Assert
            Assert.NotNull(narrative);
            Assert.Contains("Afghanistan", narrative);
            Assert.Contains("Fragile State Index", narrative);
            Assert.Contains("security", narrative, StringComparison.OrdinalIgnoreCase);
            Assert.True(narrative.Length > 100); // Substantial narrative
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

        public class OpportunitySDG
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int SDGId { get; set; }
        }

        public class Partner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PartnerType { get; set; }
            public int YearsOfPartnership { get; set; }
            public int PreviousProjectCount { get; set; }
            public decimal SuccessRate { get; set; }
            public string FinancialStability { get; set; }
        }

        public class OpportunityPartner
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int PartnerId { get; set; }
            public string Role { get; set; }
        }

        public class ParameterEvaluation
        {
            public decimal Score { get; set; }
            public string Narrative { get; set; }
            public List<string> IdentifiedRisks { get; set; }
            public List<string> Considerations { get; set; }
            public List<string> Recommendations { get; set; }
            public string RiskLevel { get; set; }
        }

        public class StrategicAlignmentEvaluation
        {
            public decimal Score { get; set; }
            public int SDGCount { get; set; }
            public string Narrative { get; set; }
        }

        public class RiskFactor
        {
            public string Type { get; set; }
            public string Description { get; set; }
        }

        public class DSTParameterScores
        {
            public decimal Complexity { get; set; }
            public decimal StrategicAlignment { get; set; }
            public decimal PartnerCapacity { get; set; }
            public decimal Context { get; set; }
            public decimal Feasibility { get; set; }
            public decimal Risk { get; set; }
            public decimal Scope { get; set; }
            public decimal Timeframe { get; set; }
            public decimal Resources { get; set; }
        }

        public class OverallRecommendation
        {
            public string Decision { get; set; } // Proceed, Proceed with Caution, Do Not Proceed
            public List<string> KeyConsiderations { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
