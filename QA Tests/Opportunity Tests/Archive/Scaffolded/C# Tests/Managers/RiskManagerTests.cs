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
    /// Tests for RiskManager (Opportunity Risk Management)
    /// Based on RiskManager_TestCases.md (15+ tests)
    /// </summary>
    public class RiskManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RiskManager _manager;

        public RiskManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"RiskTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();

            _manager = new RiskManager(_mockMapper.Object, _context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Risk Test Project",
                EstimatedValue = 2000000,
                PrimaryCountryId = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            _context.Countries.Add(new Country
            {
                Id = 1,
                Name = "Test Country",
                Code = "TC",
                FragileStateIndex = 85 // High risk
            });

            _context.SaveChanges();
        }

        #region TC-OPP-RISK-F-001: Create Risk Register Entry

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RISK-F-001")]
        public async Task CreateRisk_WithAllFields_Success()
        {
            // Arrange
            var riskRequest = new RiskCreateRequest
            {
                OpportunityId = 1,
                RiskDescription = "Political instability may delay project start",
                Category = "Political",
                Probability = "High",
                Impact = "High",
                MitigationPlan = "Monitor situation, develop contingency timeline",
                OwnerId = 1
            };

            // Act
            var risk = await _manager.CreateRiskAsync(riskRequest);

            // Assert
            Assert.NotNull(risk);
            Assert.Equal(1, risk.OpportunityId);
            Assert.Equal("Political instability may delay project start", risk.RiskDescription);
            Assert.Equal("High", risk.Probability);
            Assert.Equal("High", risk.Impact);
            Assert.Equal("Critical", risk.RiskLevel); // High prob + High impact = Critical
            Assert.Equal("Open", risk.Status);
        }

        #endregion

        #region TC-OPP-RISK-F-002: Calculate Risk Score

        [Theory]
        [InlineData("Low", "Low", "Low", 1)]
        [InlineData("Medium", "Medium", "Medium", 5)]
        [InlineData("High", "High", "Critical", 9)]
        [InlineData("Low", "High", "Medium", 4)]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RISK-F-002")]
        public void CalculateRiskScore_VariousCombinations_CorrectScores(
            string probability, 
            string impact, 
            string expectedLevel, 
            int expectedScore)
        {
            // Arrange
            var risk = new Risk
            {
                Probability = probability,
                Impact = impact
            };

            // Act
            var riskScore = CalculateRiskScore(probability, impact);
            var riskLevel = CalculateRiskLevel(riskScore);

            // Assert
            Assert.Equal(expectedScore, riskScore);
            Assert.Equal(expectedLevel, riskLevel);
        }

        private int CalculateRiskScore(string probability, string impact)
        {
            var probValue = probability switch
            {
                "Low" => 1,
                "Medium" => 2,
                "High" => 3,
                _ => 0
            };

            var impactValue = impact switch
            {
                "Low" => 1,
                "Medium" => 2,
                "High" => 3,
                _ => 0
            };

            return probValue * impactValue;
        }

        private string CalculateRiskLevel(int score)
        {
            return score switch
            {
                >= 6 => "Critical",
                >= 3 => "Medium",
                _ => "Low"
            };
        }

        #endregion

        #region TC-OPP-RISK-F-003: Identify Risks from DST Recommendations

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-RISK-F-003")]
        public async Task IdentifyRisks_FromDSTRecommendations_Success()
        {
            // Arrange
            var opportunityId = 1;
            
            // Create DST profile with risk recommendations
            var dstProfile = new DSTProfile
            {
                Id = 1,
                OpportunityId = opportunityId,
                ComplexityScore = 7.5m,
                RiskScore = 8.2m,
                CreatedDate = DateTime.UtcNow
            };
            _context.DSTProfiles.Add(dstProfile);

            // DST risk recommendations
            var recommendations = new List<DSTRecommendation>
            {
                new DSTRecommendation 
                { 
                    ProfileId = 1, 
                    Type = "Risk",
                    Recommendation = "High fragile state index - security risks likely",
                    Priority = "High"
                },
                new DSTRecommendation 
                { 
                    ProfileId = 1, 
                    Type = "Risk",
                    Recommendation = "Limited local technical capacity - capacity building needed",
                    Priority = "Medium"
                }
            };
            _context.DSTRecommendations.AddRange(recommendations);
            await _context.SaveChangesAsync();

            // Act - Accept DST risk recommendations and create risk register entries
            var createdRisks = await _manager.CreateRisksFromDSTAsync(dstProfile.Id, acceptedRecommendationIds: new[] { 1, 2 });

            // Assert
            Assert.Equal(2, createdRisks.Count);
            Assert.All(createdRisks, r => Assert.Equal(opportunityId, r.OpportunityId));
            Assert.All(createdRisks, r => Assert.Equal("DST Recommendation", r.Source));
            
            var securityRisk = createdRisks.First(r => r.RiskDescription.Contains("security"));
            Assert.Equal("High", securityRisk.Probability);
        }

        #endregion

        #region TC-OPP-RISK-F-004: Update Risk Status

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RISK-F-004")]
        public async Task UpdateRiskStatus_ApplyMitigation_StatusChanged()
        {
            // Arrange
            var risk = new Risk
            {
                Id = 1,
                OpportunityId = 1,
                RiskDescription = "Test Risk",
                Status = "Open",
                Probability = "High",
                Impact = "High",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Risks.Add(risk);
            await _context.SaveChangesAsync();

            // Act - Apply mitigation plan
            await _manager.UpdateRiskStatusAsync(1, "Mitigated", "Security measures implemented");

            // Assert
            var updatedRisk = await _context.Risks.FindAsync(1);
            Assert.Equal("Mitigated", updatedRisk.Status);
            Assert.NotNull(updatedRisk.MitigationNotes);
            Assert.Contains("Security measures", updatedRisk.MitigationNotes);
        }

        #endregion

        #region TC-OPP-RISK-F-005: Generate Risk Register Report

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RISK-F-005")]
        public async Task GenerateRiskRegisterReport_AllRisks_Success()
        {
            // Arrange
            var opportunityId = 1;
            
            // Create multiple risks
            var risks = new List<Risk>
            {
                new Risk { OpportunityId = 1, RiskDescription = "Risk 1", Probability = "High", Impact = "High", Status = "Open" },
                new Risk { OpportunityId = 1, RiskDescription = "Risk 2", Probability = "Medium", Impact = "Low", Status = "Mitigated" },
                new Risk { OpportunityId = 1, RiskDescription = "Risk 3", Probability = "Low", Impact = "Medium", Status = "Closed" }
            };
            _context.Risks.AddRange(risks);
            await _context.SaveChangesAsync();

            // Act
            var riskReport = await _manager.GenerateRiskRegisterReportAsync(opportunityId);

            // Assert
            Assert.NotNull(riskReport);
            Assert.Equal(3, riskReport.TotalRisks);
            Assert.Equal(1, riskReport.CriticalRisks); // High/High
            Assert.Equal(1, riskReport.OpenRisks);
            Assert.Equal(1, riskReport.MitigatedRisks);
            Assert.Equal(1, riskReport.ClosedRisks);
            
            // Report includes all risks
            Assert.Equal(3, riskReport.Risks.Count);
        }

        #endregion

        #region TC-OPP-RISK-V-001: Validate Risk Category

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-RISK-V-001")]
        public async Task CreateRisk_InvalidCategory_ThrowsException()
        {
            // Arrange
            var invalidRiskRequest = new RiskCreateRequest
            {
                OpportunityId = 1,
                RiskDescription = "Test risk",
                Category = "InvalidCategory", // Not in allowed list
                Probability = "High",
                Impact = "High"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.CreateRiskAsync(invalidRiskRequest));

            Assert.Contains("category", ex.Message, StringComparison.OrdinalIgnoreCase);
            
            // Valid categories: Political, Financial, Technical, Environmental, Social, etc.
        }

        #endregion

        #region Helper Classes

        public class Risk
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string RiskDescription { get; set; }
            public string Category { get; set; }
            public string Probability { get; set; } // Low, Medium, High
            public string Impact { get; set; } // Low, Medium, High
            public string RiskLevel { get; set; } // Low, Medium, Critical
            public string Status { get; set; } // Open, Mitigated, Closed
            public string MitigationPlan { get; set; }
            public string MitigationNotes { get; set; }
            public int? OwnerId { get; set; }
            public string Source { get; set; } // Manual, DST Recommendation, etc.
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class DSTRecommendation
        {
            public int Id { get; set; }
            public int ProfileId { get; set; }
            public string Type { get; set; } // Risk, Personnel, Process
            public string Recommendation { get; set; }
            public string Priority { get; set; }
        }

        public class RiskCreateRequest
        {
            public int OpportunityId { get; set; }
            public string RiskDescription { get; set; }
            public string Category { get; set; }
            public string Probability { get; set; }
            public string Impact { get; set; }
            public string MitigationPlan { get; set; }
            public int? OwnerId { get; set; }
        }

        public class RiskRegisterReport
        {
            public int TotalRisks { get; set; }
            public int CriticalRisks { get; set; }
            public int OpenRisks { get; set; }
            public int MitigatedRisks { get; set; }
            public int ClosedRisks { get; set; }
            public List<Risk> Risks { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
