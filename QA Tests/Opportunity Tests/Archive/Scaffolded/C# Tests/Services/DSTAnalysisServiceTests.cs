using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSBusiness.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Services
{
    public class DSTAnalysisServiceTests
    {
        private readonly Mock<IAIService> _mockAIService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly DSTAnalysisService _service;

        public DSTAnalysisServiceTests()
        {
            _mockAIService = new Mock<IAIService>();
            _mockCacheService = new Mock<ICacheService>();
            _service = new DSTAnalysisService(_mockAIService.Object, _mockCacheService.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-001")]
        public async Task AnalyzeComplexity_LargeScope_HighScore()
        {
            var opportunityData = new { Budget = 15000000m, Deliverables = 25, Timeline = 60 };
            _mockAIService.Setup(ai => ai.AnalyzeComplexityAsync(It.IsAny<object>())).ReturnsAsync(8.5m);

            var score = await _service.AnalyzeComplexityAsync(opportunityData);

            Assert.InRange(score, 7.5m, 9.5m);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-002")]
        public async Task GenerateRecommendations_HighRisk_CriticalRecommendations()
        {
            var profileData = new { RiskScore = 8.5m, ComplexityScore = 7.8m };
            var recommendations = new List<string> { "Hire security specialist", "Enhanced risk monitoring" };
            _mockAIService.Setup(ai => ai.GenerateRecommendationsAsync(It.IsAny<object>())).ReturnsAsync(recommendations);

            var result = await _service.GenerateRecommendationsAsync(profileData);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Contains("security"));
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-003")]
        public async Task FindSimilar_CachedResults_ReturnFromCache()
        {
            // Arrange
            var opportunityId = 1;
            var cachedResults = new List<SimilarProjectModel>
            {
                new SimilarProjectModel { Id = 10, SimilarityScore = 0.85m },
                new SimilarProjectModel { Id = 12, SimilarityScore = 0.78m }
            };

            _mockCacheService.Setup(c => c.GetAsync<List<SimilarProjectModel>>($"similar_{opportunityId}"))
                .ReturnsAsync(cachedResults);

            // Act
            var result = await _service.FindSimilarProjectsAsync(opportunityId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.SimilarityScore >= 0.75m));
            
            // Cache hit, no AI call
            _mockAIService.Verify(ai => ai.FindSimilarAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-004")]
        public async Task AssessContextualRisk_FragileState_HighRiskScore()
        {
            // Arrange
            var contextData = new
            {
                CountryMVI = 15m, // Very fragile
                CountryHDI = 0.45m, // Low development
                RegionalConflict = true,
                HistoricalProjectFailures = 3
            };

            _mockAIService.Setup(ai => ai.AssessContextualRiskAsync(It.IsAny<object>()))
                .ReturnsAsync(8.2m);

            // Act
            var riskScore = await _service.AssessContextualRiskAsync(contextData);

            // Assert
            Assert.InRange(riskScore, 7.0m, 9.0m); // High risk
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-005")]
        public async Task EvaluateStrategicAlignment_PerfectMatch_HighScore()
        {
            // Arrange
            var opportunityData = new
            {
                Sector = "Infrastructure",
                SDGs = new[] { "SDG 6", "SDG 11", "SDG 13" },
                CountryPriorities = new[] { "Climate resilience", "Urban development" },
                UNOPSCoreCompetencies = new[] { "Infrastructure", "Project management" }
            };

            _mockAIService.Setup(ai => ai.EvaluateStrategicAlignmentAsync(It.IsAny<object>()))
                .ReturnsAsync(9.1m);

            // Act
            var alignmentScore = await _service.EvaluateStrategicAlignmentAsync(opportunityData);

            // Assert
            Assert.InRange(alignmentScore, 8.5m, 10.0m); // High alignment
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-006")]
        public async Task GenerateComprehensiveDSTReport_AllDimensions_CompleteAnalysis()
        {
            // Arrange
            var opportunityId = 1;
            
            _mockAIService.Setup(ai => ai.AnalyzeComplexityAsync(It.IsAny<object>())).ReturnsAsync(7.5m);
            _mockAIService.Setup(ai => ai.AssessContextualRiskAsync(It.IsAny<object>())).ReturnsAsync(6.2m);
            _mockAIService.Setup(ai => ai.EvaluateStrategicAlignmentAsync(It.IsAny<object>())).ReturnsAsync(8.8m);
            _mockAIService.Setup(ai => ai.EvaluatePartnerCapacityAsync(It.IsAny<object>())).ReturnsAsync(7.0m);
            _mockAIService.Setup(ai => ai.EvaluateFeasibilityAsync(It.IsAny<object>())).ReturnsAsync(7.5m);

            // Act
            var report = await _service.GenerateComprehensiveDSTReportAsync(opportunityId);

            // Assert
            Assert.NotNull(report);
            Assert.Equal(7.5m, report.ComplexityScore);
            Assert.Equal(6.2m, report.RiskScore);
            Assert.Equal(8.8m, report.StrategicAlignmentScore);
            Assert.Equal(7.0m, report.PartnerCapacityScore);
            Assert.Equal(7.5m, report.FeasibilityScore);
            
            // Overall recommendation generated
            Assert.NotNull(report.OverallRecommendation);
            Assert.Contains("Proceed", report.OverallRecommendation);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-007")]
        public async Task CompareDSTProfiles_TwoOpportunities_HighlightsDifferences()
        {
            // Arrange
            var opportunityId1 = 1;
            var opportunityId2 = 2;

            var profile1 = new DSTProfileComparison
            {
                OpportunityId = 1,
                ComplexityScore = 7.5m,
                RiskScore = 6.0m
            };

            var profile2 = new DSTProfileComparison
            {
                OpportunityId = 2,
                ComplexityScore = 5.2m,
                RiskScore = 4.5m
            };

            // Act
            var comparison = await _service.CompareDSTProfilesAsync(opportunityId1, opportunityId2);

            // Assert
            Assert.NotNull(comparison);
            Assert.True(comparison.ComplexityDifference > 2.0m);
            Assert.True(comparison.RiskDifference > 1.0m);
            Assert.Equal("Opportunity 1 is more complex and higher risk", comparison.Summary);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-DSTA-SVC-F-008")]
        public async Task BatchAnalyzeOpportunities_MultipleOpportunities_ProcessesInParallel()
        {
            // Arrange
            var opportunityIds = new[] { 1, 2, 3, 4, 5 };

            _mockAIService.Setup(ai => ai.AnalyzeComplexityAsync(It.IsAny<object>()))
                .ReturnsAsync(7.5m);

            // Act
            var results = await _service.BatchAnalyzeOpportunitiesAsync(opportunityIds);

            // Assert
            Assert.Equal(5, results.Count);
            Assert.All(results, r => Assert.True(r.ComplexityScore > 0));
            
            // Verify AI called for each opportunity
            _mockAIService.Verify(ai => ai.AnalyzeComplexityAsync(It.IsAny<object>()), Times.Exactly(5));
        }

        public class SimilarProjectModel
        {
            public int Id { get; set; }
            public decimal SimilarityScore { get; set; }
        }

        public class ComprehensiveDSTReport
        {
            public decimal ComplexityScore { get; set; }
            public decimal RiskScore { get; set; }
            public decimal StrategicAlignmentScore { get; set; }
            public decimal PartnerCapacityScore { get; set; }
            public decimal FeasibilityScore { get; set; }
            public string OverallRecommendation { get; set; }
        }

        public class DSTProfileComparison
        {
            public int OpportunityId { get; set; }
            public decimal ComplexityScore { get; set; }
            public decimal RiskScore { get; set; }
        }

        public class ProfileComparisonResult
        {
            public decimal ComplexityDifference { get; set; }
            public decimal RiskDifference { get; set; }
            public string Summary { get; set; }
        }

        public class BatchAnalysisResult
        {
            public int OpportunityId { get; set; }
            public decimal ComplexityScore { get; set; }
        }
    }
}
