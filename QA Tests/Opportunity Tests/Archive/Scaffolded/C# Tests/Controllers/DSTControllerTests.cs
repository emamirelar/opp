using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    /// <summary>
    /// Tests for DSTController API endpoints
    /// Based on DSTController_TestCases.md (10+ tests)
    /// </summary>
    public class DSTControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DSTController _controller;

        public DSTControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _mockMapper = new Mock<IMapper>();

            _controller = new DSTController(
                _mockManagerWrapper.Object,
                _mockMapper.Object
            );
        }

        #region TC-OPP-DST-CTRL-F-001: POST - Generate DST Profile

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DST-CTRL-F-001")]
        public async Task GenerateDSTProfile_ValidOpportunity_ReturnsOkWithProfile()
        {
            // Arrange
            var opportunityId = 1;
            var profile = new DSTProfileModel
            {
                Id = 1,
                OpportunityId = opportunityId,
                ComplexityScore = 7.2m,
                RiskScore = 6.5m,
                GeneratedDate = DateTime.UtcNow
            };

            _mockManagerWrapper.Setup(m => m.DSTManager.GenerateDSTProfileAsync(opportunityId))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.GenerateProfile(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProfile = Assert.IsType<DSTProfileModel>(okResult.Value);
            Assert.Equal(opportunityId, returnedProfile.OpportunityId);
            Assert.InRange(returnedProfile.ComplexityScore, 0m, 10m);
        }

        #endregion

        #region TC-OPP-DST-CTRL-F-002: GET - Get DST Profile

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DST-CTRL-F-002")]
        public async Task GetDSTProfile_ValidOpportunity_ReturnsProfile()
        {
            // Arrange
            var opportunityId = 1;
            var profile = new DSTProfileModel
            {
                Id = 1,
                OpportunityId = opportunityId,
                ComplexityScore = 6.8m,
                IsCurrent = true
            };

            _mockManagerWrapper.Setup(m => m.DSTManager.GetCurrentProfileAsync(opportunityId))
                .ReturnsAsync(profile);

            // Act
            var result = await _controller.GetProfile(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProfile = Assert.IsType<DSTProfileModel>(okResult.Value);
            Assert.True(returnedProfile.IsCurrent);
        }

        #endregion

        #region TC-OPP-DST-CTRL-F-003: POST - Regenerate DST Profile

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DST-CTRL-F-003")]
        public async Task RegenerateDSTProfile_ExistingProfile_CreatesNewVersion()
        {
            // Arrange
            var opportunityId = 1;
            var newProfile = new DSTProfileModel
            {
                Id = 2,
                OpportunityId = opportunityId,
                ComplexityScore = 7.5m,
                Version = 2,
                GeneratedDate = DateTime.UtcNow
            };

            _mockManagerWrapper.Setup(m => m.DSTManager.RegenerateDSTProfileAsync(opportunityId))
                .ReturnsAsync(newProfile);

            // Act
            var result = await _controller.RegenerateProfile(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProfile = Assert.IsType<DSTProfileModel>(okResult.Value);
            Assert.Equal(2, returnedProfile.Version); // New version
        }

        #endregion

        #region TC-OPP-DST-CTRL-F-004: GET - Get Recommendations

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DST-CTRL-F-004")]
        public async Task GetRecommendations_ForOpportunity_ReturnsList()
        {
            // Arrange
            var opportunityId = 1;
            var recommendations = new System.Collections.Generic.List<DSTRecommendationModel>
            {
                new DSTRecommendationModel 
                { 
                    Id = 1, 
                    Recommendation = "Hire infrastructure specialist",
                    Priority = "High",
                    Status = "Pending"
                },
                new DSTRecommendationModel 
                { 
                    Id = 2, 
                    Recommendation = "Conduct security assessment",
                    Priority = "Critical",
                    Status = "Pending"
                }
            };

            _mockManagerWrapper.Setup(m => m.DSTManager.GetRecommendationsAsync(opportunityId))
                .ReturnsAsync(recommendations);

            // Act
            var result = await _controller.GetRecommendations(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRecs = Assert.IsAssignableFrom<System.Collections.Generic.List<DSTRecommendationModel>>(okResult.Value);
            Assert.Equal(2, returnedRecs.Count);
            Assert.Contains(returnedRecs, r => r.Priority == "Critical");
        }

        #endregion

        #region TC-OPP-DST-CTRL-F-005: POST - Accept Recommendation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DST-CTRL-F-005")]
        public async Task AcceptRecommendation_ValidRecommendation_ReturnsOk()
        {
            // Arrange
            var recommendationId = 1;
            var acceptRequest = new RecommendationActionRequest
            {
                Action = "Accept",
                Notes = "Agreed - will hire specialist"
            };

            _mockManagerWrapper.Setup(m => m.DSTManager.AcceptRecommendationAsync(recommendationId, acceptRequest.Notes))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AcceptRecommendation(recommendationId, acceptRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        #endregion

        #region TC-OPP-DST-CTRL-F-006: GET - Search Similar Opportunities

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DST-CTRL-F-006")]
        public async Task SearchSimilarOpportunities_ValidCriteria_ReturnsSimilarList()
        {
            // Arrange
            var opportunityId = 1;
            var similarOpportunities = new System.Collections.Generic.List<SimilarOpportunityModel>
            {
                new SimilarOpportunityModel 
                { 
                    Id = 10, 
                    Name = "Similar Project 1",
                    SimilarityScore = 0.85m
                },
                new SimilarOpportunityModel 
                { 
                    Id = 11, 
                    Name = "Similar Project 2",
                    SimilarityScore = 0.78m
                }
            };

            _mockManagerWrapper.Setup(m => m.DSTManager.FindSimilarOpportunitiesAsync(opportunityId))
                .ReturnsAsync(similarOpportunities);

            // Act
            var result = await _controller.GetSimilar(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<System.Collections.Generic.List<SimilarOpportunityModel>>(okResult.Value);
            Assert.Equal(2, returnedList.Count);
            Assert.All(returnedList, o => Assert.True(o.SimilarityScore > 0.5m));
            
            // Sorted by similarity descending
            Assert.True(returnedList[0].SimilarityScore >= returnedList[1].SimilarityScore);
        }

        #endregion

        #region Helper Classes

        public class DSTProfileModel
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal ComplexityScore { get; set; }
            public decimal RiskScore { get; set; }
            public int Version { get; set; }
            public bool IsCurrent { get; set; }
            public DateTime GeneratedDate { get; set; }
        }

        public class DSTRecommendationModel
        {
            public int Id { get; set; }
            public string Recommendation { get; set; }
            public string Priority { get; set; }
            public string Status { get; set; }
        }

        public class RecommendationActionRequest
        {
            public string Action { get; set; } // Accept, Reject
            public string Notes { get; set; }
        }

        public class SimilarOpportunityModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal SimilarityScore { get; set; }
        }

        #endregion
    }
}
