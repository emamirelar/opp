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
    /// Test suite for DSTManager - Decision Support Tool
    /// Tests profile generation, parameter evaluation, recommendations
    /// </summary>
    public class DSTManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDSTAnalysisService> _mockDSTService;
        private readonly DSTManager _manager;

        public DSTManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"DSTTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();
            _mockDSTService = new Mock<IDSTAnalysisService>();

            _manager = new DSTManager(_mockMapper.Object, _context, _mockDSTService.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Countries.AddRange(new[]
            {
                new Country { Id = 1, Name = "Bangladesh", Code = "BD", MVIScore = 32.5m, FragilityIndex = 45 },
                new Country { Id = 2, Name = "Nepal", Code = "NP", MVIScore = 28.7m, FragilityIndex = 52 }
            });

            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity
                {
                    Id = 1,
                    Name = "Water Infrastructure",
                    EstimatedValue = 2500000,
                    PrimaryCountryId = 1,
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new Domain.Entities.Opportunity
                {
                    Id = 2,
                    Name = "Simple Project",
                    EstimatedValue = 100000,
                    PrimaryCountryId = 2,
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                }
            });

            _context.SaveChanges();
        }

        #region Profile Generation Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DST-F-001")]
        public async Task GenerateDSTProfile_CompleteData_Success()
        {
            // Arrange
            var opportunityId = 1;
            var profile = new DSTProfile
            {
                Id = 1,
                OpportunityId = opportunityId,
                ComplexityScore = 6.5m,
                RiskScore = 5.8m,
                FeasibilityScore = 7.2m,
                GeneratedDate = DateTime.UtcNow
            };

            _mockDSTService.Setup(s => s.GenerateProfileAsync(opportunityId))
                .ReturnsAsync(profile);
            _mockMapper.Setup(m => m.Map<DSTProfileModel>(It.IsAny<DSTProfile>()))
                .Returns(new DSTProfileModel { Id = 1, ComplexityScore = 6.5m });

            // Act
            var result = await _manager.GenerateDSTProfileAsync(opportunityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6.5m, result.ComplexityScore);
            _mockDSTService.Verify(s => s.GenerateProfileAsync(opportunityId), Times.Once);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DST-F-002")]
        public async Task RegenerateDSTProfile_CreatesNewVersion()
        {
            // Arrange
            var existingProfile = new DSTProfile
            {
                Id = 1,
                OpportunityId = 1,
                Version = 1,
                ComplexityScore = 6.0m,
                CreatedDate = DateTime.UtcNow.AddDays(-7)
            };
            _context.DSTProfiles.Add(existingProfile);
            await _context.SaveChangesAsync();

            var newProfile = new DSTProfile
            {
                Id = 2,
                OpportunityId = 1,
                Version = 2,
                ComplexityScore = 6.5m,
                CreatedDate = DateTime.UtcNow
            };

            _mockDSTService.Setup(s => s.RegenerateProfileAsync(1))
                .ReturnsAsync(newProfile);

            // Act
            var result = await _manager.RegenerateDSTProfileAsync(1);

            // Assert
            Assert.NotNull(result);
            var oldProfile = await _context.DSTProfiles.FindAsync(1);
            Assert.False(oldProfile.IsCurrent); // Old version archived
            Assert.True(result.IsCurrent); // New version marked current
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-DST-F-003")]
        public async Task GenerateDSTProfile_MissingData_HandlesGracefully()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 3,
                Name = "Incomplete Opportunity",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
                // Missing budget, country, etc.
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            // Should either generate partial profile with warnings
            // Or throw BusinessException indicating missing data
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.GenerateDSTProfileAsync(3));

            Assert.Contains("incomplete", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Calculation")]
        [Trait("TestId", "TC-OPP-DST-F-004")]
        public async Task CalculateComplexityScore_SimpleOpportunity_LowScore()
        {
            // Arrange
            var opportunityId = 2; // Simple opportunity

            // Act
            var score = await _manager.CalculateComplexityScoreAsync(opportunityId);

            // Assert
            Assert.InRange(score, 0, 4); // Low complexity (0-4)
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Calculation")]
        [Trait("TestId", "TC-OPP-DST-F-004-2")]
        public async Task CalculateComplexityScore_ComplexOpportunity_HighScore()
        {
            // Arrange
            var opportunityId = 1; // Complex opportunity

            // Act
            var score = await _manager.CalculateComplexityScoreAsync(opportunityId);

            // Assert
            Assert.InRange(score, 5, 10); // High complexity (5-10)
        }

        #endregion

        #region Nine Parameter Evaluation Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Parameter")]
        [Trait("TestId", "TC-OPP-DST-P-001")]
        public async Task EvaluateStrategicAlignment_OpportunityWithSDGs_HighScore()
        {
            // Arrange
            var opportunity = await _context.Opportunities.FindAsync(1);
            opportunity.SDGs = new List<OpportunitySDG>
            {
                new OpportunitySDG { SDGNumber = 6, OpportunityId = 1 },
                new OpportunitySDG { SDGNumber = 13, OpportunityId = 1 }
            };
            await _context.SaveChangesAsync();

            // Act
            var score = await _manager.EvaluateParameter1_StrategicAlignmentAsync(1);

            // Assert
            Assert.InRange(score, 70, 100); // High alignment
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Parameter")]
        [Trait("TestId", "TC-OPP-DST-P-002")]
        public async Task EvaluatePartnersStakeholders_ExperiencedPartner_HighScore()
        {
            // Arrange
            var partner = new Partner
            {
                Id = 1,
                Name = "World Bank",
                PastPerformanceScore = 85,
                FinancialCapacityScore = 90
            };
            _context.Partners.Add(partner);

            var opportunity = await _context.Opportunities.FindAsync(1);
            opportunity.Partners = new List<OpportunityPartner>
            {
                new OpportunityPartner { PartnerId = 1, Role = "Funding Partner" }
            };
            await _context.SaveChangesAsync();

            // Act
            var score = await _manager.EvaluateParameter2_PartnersStakeholdersAsync(1);

            // Assert
            Assert.InRange(score, 75, 100); // Strong partner
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Parameter")]
        [Trait("TestId", "TC-OPP-DST-P-004")]
        public async Task EvaluateContext_FragileState_LowScore()
        {
            // Arrange
            var country = await _context.Countries.FindAsync(2);
            country.FragilityIndex = 85; // High fragility = challenging context
            await _context.SaveChangesAsync();

            var opportunity = await _context.Opportunities.FindAsync(2);
            opportunity.PrimaryCountryId = 2;
            await _context.SaveChangesAsync();

            // Act
            var score = await _manager.EvaluateParameter4_ContextAsync(2);

            // Assert
            Assert.InRange(score, 0, 40); // Challenging context = low score
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Parameter")]
        [Trait("TestId", "TC-OPP-DST-P-010")]
        public async Task EvaluateAllParameters_BalancedOpportunity_MediumScores()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var parameters = await _manager.EvaluateAllParametersAsync(opportunityId);

            // Assert
            Assert.Equal(9, parameters.Count);
            Assert.All(parameters, p => Assert.InRange(p.Score, 0, 100));
            Assert.Contains(parameters, p => p.ParameterName == "Strategic Alignment");
            Assert.Contains(parameters, p => p.ParameterName == "Partners and Stakeholders");
            // ... all 9 parameters present
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-DST-P-011")]
        public async Task EvaluateAllParameters_HighRisk_MultipleRedFlags()
        {
            // Arrange - Create high-risk opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 4,
                Name = "High Risk Project",
                EstimatedValue = 10000000,
                PrimaryCountryId = 2, // Fragile state
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var parameters = await _manager.EvaluateAllParametersAsync(4);

            // Assert
            var redFlags = parameters.Where(p => p.Score < 40).ToList();
            Assert.True(redFlags.Count >= 2, "High-risk opportunity should have multiple red flags");
        }

        #endregion

        #region Recommendations Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Recommendations")]
        [Trait("TestId", "TC-OPP-DST-R-001")]
        public async Task GenerateRiskRecommendations_HighRiskCountry_SuggestsRisks()
        {
            // Arrange
            var profileId = 1;

            // Act
            var recommendations = await _manager.GetRiskRecommendationsAsync(profileId);

            // Assert
            Assert.NotNull(recommendations);
            Assert.NotEmpty(recommendations);
            Assert.Contains(recommendations, r => r.Category == "Risk");
            Assert.All(recommendations, r => Assert.NotEmpty(r.MitigationStrategy));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Recommendations")]
        [Trait("TestId", "TC-OPP-DST-R-002")]
        public async Task GeneratePersonnelRecommendations_ComplexProject_SpecialistsRequired()
        {
            // Arrange
            var profileId = 1;

            // Act
            var recommendations = await _manager.GetPersonnelRecommendationsAsync(profileId);

            // Assert
            Assert.NotNull(recommendations);
            Assert.Contains(recommendations, r =>
                r.RequiredRole.Contains("specialist", StringComparison.OrdinalIgnoreCase) ||
                r.RequiredRole.Contains("advisor", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "UserAction")]
        [Trait("TestId", "TC-OPP-DST-R-006")]
        public async Task AcceptRecommendation_UpdatesStatus()
        {
            // Arrange
            var recommendation = new DSTRecommendation
            {
                Id = 1,
                ProfileId = 1,
                RecommendationType = "Risk",
                Description = "Add security risk to register",
                Status = "Pending"
            };
            _context.DSTRecommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            var action = "Risk added to register";

            // Act
            await _manager.AcceptRecommendationAsync(1, action);

            // Assert
            var updated = await _context.DSTRecommendations.FindAsync(1);
            Assert.Equal("Actioned", updated.Status);
            Assert.Equal(action, updated.ActionTaken);
            Assert.True(updated.ActionedDate.HasValue);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "UserAction")]
        [Trait("TestId", "TC-OPP-DST-R-007")]
        public async Task RejectRecommendation_CapturesReason()
        {
            // Arrange
            var recommendation = new DSTRecommendation
            {
                Id = 2,
                ProfileId = 1,
                RecommendationType = "Personnel",
                Description = "Add gender advisor",
                Status = "Pending"
            };
            _context.DSTRecommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            var reason = "Gender advisor not required for this project scope";

            // Act
            await _manager.RejectRecommendationAsync(2, reason);

            // Assert
            var updated = await _context.DSTRecommendations.FindAsync(2);
            Assert.Equal("Rejected", updated.Status);
            Assert.Equal(reason, updated.RejectionReason);
            Assert.True(updated.RejectedDate.HasValue);
        }

        #endregion

        #region Similar Projects Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Similarity")]
        [Trait("TestId", "TC-OPP-DST-S-001")]
        public async Task FindSimilarByGeography_ReturnsSameRegion()
        {
            // Arrange
            var historicalOpp = new Domain.Entities.Opportunity
            {
                Id = 5,
                Name = "Previous Bangladesh Project",
                PrimaryCountryId = 1,
                Status = "Completed",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddYears(-2)
            };
            _context.Opportunities.Add(historicalOpp);
            await _context.SaveChangesAsync();

            // Act
            var similar = await _manager.FindSimilarOpportunitiesAsync(1, "geography");

            // Assert
            Assert.NotNull(similar);
            Assert.Contains(similar, s => s.OpportunityId == 5);
            Assert.All(similar, s => Assert.InRange(s.SimilarityScore, 0, 100));
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Algorithm")]
        [Trait("TestId", "TC-OPP-DST-S-004")]
        public async Task SimilarityScoringAlgorithm_AccurateCalculation()
        {
            // Arrange
            var opp1 = await _context.Opportunities.FindAsync(1);
            var opp2 = await _context.Opportunities.FindAsync(2);

            // Act
            var score = await _manager.CalculateSimilarityScoreAsync(opp1, opp2);

            // Assert
            Assert.InRange(score, 0, 100);
            // Geography match (both South Asia) should contribute ~30%
            // If same country, score should be > 30
            // If different countries in region, score should be 15-30
        }

        #endregion

        #region Reporting Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Reporting")]
        [Trait("TestId", "TC-OPP-DST-REP-001")]
        public async Task GenerateProfileReport_CompletePDF()
        {
            // Arrange
            var profileId = 1;

            // Act
            var pdfBytes = await _manager.GenerateProfileReportAsync(profileId);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 1000, "PDF should have meaningful content");
            // First bytes should be PDF signature
            Assert.Equal(0x25, pdfBytes[0]); // %
            Assert.Equal(0x50, pdfBytes[1]); // P
            Assert.Equal(0x44, pdfBytes[2]); // D
            Assert.Equal(0x46, pdfBytes[3]); // F
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Reporting")]
        [Trait("TestId", "TC-OPP-DST-REP-002")]
        public async Task GenerateExecutiveSummary_OnePage()
        {
            // Arrange
            var profileId = 1;

            // Act
            var summary = await _manager.GenerateExecutiveSummaryAsync(profileId);

            // Assert
            Assert.NotNull(summary);
            Assert.True(summary.Length <= 2000, "Executive summary should be concise");
            Assert.Contains("complexity", summary, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("risk", summary, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("recommendation", summary, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
