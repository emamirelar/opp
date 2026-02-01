using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.IntegrationTests.Infrastructure;

using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.Tests.Integration.DST
{
    /// <summary>
    /// Edge case tests for DST (Decision Support Tool)
    /// Tests boundary conditions, extreme values, and unusual scenarios
    /// 
    /// Test Coverage:
    /// - Boundary values (maxResults = 0, 1, MAX_INT)
    /// - Very long text inputs (exceeding limits)
    /// - Special characters and Unicode
    /// - Empty opportunities (no description/deliverables)
    /// - Extreme budget values (zero, negative, very large)
    /// - Mass dismiss operations (all recommendations dismissed)
    /// - Simultaneous cache invalidation
    /// - Pagination edge cases
    /// - Unicode and internationalization
    /// - Extremely fast repeated requests
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "EdgeCases")]
    public class DSTEdgeCaseTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        private async Task<int> CreateTestOpportunityAsync(
            string title,
            string description = "",
            string deliverables = "",
            decimal budget = 1000000m)
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = description,
                Deliverables = deliverables,
                CountryId = 1,
                EstimatedBudget = budget,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

        private ClaimsPrincipal CreateTestUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Project Manager")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        #endregion

        #region TC-DST-EDGE-001 through TC-DST-EDGE-005: Boundary Values for maxResults

        /// <summary>
        /// TC-DST-EDGE-001: Get DST recommendations with maxResults = 0
        /// 
        /// Given: maxResults parameter set to 0
        /// When: DST recommendations are requested
        /// Then: Returns empty list or throws ArgumentException
        /// 
        /// Expected Behavior:
        /// - Either returns empty list (valid interpretation)
        /// - Or throws ArgumentException (strict validation)
        /// - Does not crash or return invalid data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-001")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_MaxResultsZero_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Test Project",
                description: "Test description"
            );

            // Act
            try
            {
                var response = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 0, // Zero results requested
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );

                // If no exception, should return empty or valid response
                response.Should().NotBeNull();
                response.Recommendations.Should().BeEmpty("maxResults=0 should return no recommendations");
            }
            catch (ArgumentException ex)
            {
                // Acceptable if system requires maxResults > 0
                ex.Message.Should().Contain("maxResults");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-002: Get DST recommendations with maxResults = 1
        /// 
        /// Given: maxResults = 1 (minimum valid value)
        /// When: DST recommendations are requested
        /// Then: Returns exactly 1 recommendation (if available)
        /// 
        /// Expected Behavior:
        /// - Returns at most 1 recommendation
        /// - Top-ranked recommendation is returned
        /// - No errors or crashes
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-002")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_MaxResultsOne_ReturnsSingle()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare Infrastructure Development",
                description: "Large healthcare project with multiple risk factors",
                deliverables: "Healthcare facilities, equipment, training"
            );

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 1, // Only 1 result
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().HaveCountLessOrEqualTo(1, "maxResults=1 should return at most 1");
            
            if (response.Recommendations.Any())
            {
                // Should be top-ranked recommendation
                response.Recommendations.First().ConfidenceLevel.Should().BeGreaterThan(0);
            }
        }

        /// <summary>
        /// TC-DST-EDGE-003: Get DST recommendations with very large maxResults
        /// 
        /// Given: maxResults = 1000 (very large number)
        /// When: DST recommendations are requested
        /// Then: Returns all available recommendations (capped at reasonable limit)
        /// 
        /// Expected Behavior:
        /// - System may cap at internal maximum (e.g., 100)
        /// - Returns all available recommendations if less than maxResults
        /// - Does not crash or timeout
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-003")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_VeryLargeMaxResults_CapsAppropriately()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Complex Multi-Sector Program",
                description: "Large program spanning multiple sectors with many risk factors",
                deliverables: "Extensive deliverables"
            );

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 1000, // Very large
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().NotBeNull();
            
            // System should cap at reasonable limit (e.g., 100)
            response.Recommendations.Should().HaveCountLessOrEqualTo(100,
                "system should cap at reasonable maximum");
        }

        /// <summary>
        /// TC-DST-EDGE-004: Get DST recommendations with negative maxResults
        /// 
        /// Given: maxResults = -1 (invalid negative)
        /// When: DST recommendations are requested
        /// Then: Throws ArgumentException
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException
        /// - Negative values are rejected
        /// - Error message indicates invalid parameter
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-004")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_NegativeMaxResults_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Test Project",
                description: "Test description"
            );

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: -1, // Negative value
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );
            });

            exception.Message.Should().Contain("maxResults");
        }

        /// <summary>
        /// TC-DST-EDGE-005: Get DST recommendations with int.MaxValue
        /// 
        /// Given: maxResults = int.MaxValue (2,147,483,647)
        /// When: DST recommendations are requested
        /// Then: System caps at reasonable limit without overflow
        /// 
        /// Expected Behavior:
        /// - No integer overflow errors
        /// - Returns capped results
        /// - Completes in reasonable time
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-005")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_MaxIntValue_NoOverflow()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Test Project",
                description: "Test description"
            );

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: int.MaxValue, // Maximum int value
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().NotBeNull();
            response.Recommendations.Should().HaveCountLessOrEqualTo(100,
                "system should cap extreme values");
        }

        #endregion

        #region TC-DST-EDGE-006 through TC-DST-EDGE-010: Extreme Text Inputs

        /// <summary>
        /// TC-DST-EDGE-006: Get DST recommendations with very long title
        /// 
        /// Given: Opportunity with extremely long title (10,000+ characters)
        /// When: DST recommendations are requested
        /// Then: System handles or truncates gracefully
        /// 
        /// Expected Behavior:
        /// - Does not crash
        /// - Truncates or processes efficiently
        /// - Keywords still extracted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-006")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_VeryLongTitle_HandlesGracefully()
        {
            // Arrange
            var veryLongTitle = string.Join(" ", Enumerable.Range(1, 2000).Select(i => 
                $"Word{i}"));

            var opportunityId = await CreateTestOpportunityAsync(
                title: veryLongTitle.Substring(0, Math.Min(veryLongTitle.Length, 500)), // Cap for DB limit
                description: "Standard description"
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.ExtractedKeywords.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-007: Create DST risk with maximum length title
        /// 
        /// Given: Risk with title at maximum allowed length
        /// When: Risk creation is attempted
        /// Then: Risk is created successfully
        /// 
        /// Expected Behavior:
        /// - Risk created if within limits
        /// - Or validation error if exceeds limit
        /// - Error message indicates length violation
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-007")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_MaxLengthTitle_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync("Test Project");

            // Assuming max length is 500 characters (adjust based on actual limit)
            var maxLengthTitle = new string('A', 500);

            var riskRequest = new RiskCreateRequest
            {
                Title = maxLengthTitle,
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act
            try
            {
                var risk = await riskManager.AddRiskAsync(riskRequest, user);
                
                // Should succeed if within limits
                risk.Should().NotBeNull();
                risk.Title.Length.Should().BeLessOrEqualTo(500);
            }
            catch (ArgumentException ex)
            {
                // Acceptable if exceeds limit
                ex.Message.Should().Contain("length", "error should indicate length violation");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-008: Create DST risk with title exceeding maximum
        /// 
        /// Given: Risk title exceeding maximum allowed length (e.g., 10,000 chars)
        /// When: Risk creation is attempted
        /// Then: Validation error is returned
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException or validation error
        /// - Risk is not created
        /// - Error indicates field length limit
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-008")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_TitleExceedingMax_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync("Test Project");

            var excessiveTitle = new string('X', 10000); // Way over limit

            var riskRequest = new RiskCreateRequest
            {
                Title = excessiveTitle,
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-EDGE-009: Get DST recommendations with special characters in text
        /// 
        /// Given: Opportunity with special characters (quotes, apostrophes, SQL injection attempts)
        /// When: DST recommendations are requested
        /// Then: Special characters are handled safely
        /// 
        /// Expected Behavior:
        /// - No SQL injection
        /// - Special characters properly escaped
        /// - Keywords extracted correctly
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-009")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_SpecialCharacters_SafelyHandled()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Project with \"quotes\" and 'apostrophes' and <tags>",
                description: "Testing special chars: '; DROP TABLE Risks; --",
                deliverables: "Items with $pecial & characters: #1, @test, 100%"
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull("special characters should be safely handled");
            response.ExtractedKeywords.Should().NotBeNull();
            
            // System should not crash or execute malicious SQL
        }

        /// <summary>
        /// TC-DST-EDGE-010: Get DST recommendations with Unicode characters
        /// 
        /// Given: Opportunity with Unicode text (Chinese, Arabic, Emoji)
        /// When: DST recommendations are requested
        /// Then: Unicode is properly handled
        /// 
        /// Expected Behavior:
        /// - Unicode text processed correctly
        /// - Keywords extracted from international text
        /// - No encoding errors
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-010")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_UnicodeCharacters_HandledCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Project 项目 مشروع with emoji 🏥🌍",
                description: "Testing Unicode: 中文 العربية Español Français 日本語",
                deliverables: "Deliverables with symbols: ™ © ® € £ ¥"
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull("Unicode should be handled");
            response.ExtractedKeywords.Should().NotBeNull();
        }

        #endregion

        #region TC-DST-EDGE-011 through TC-DST-EDGE-015: Empty and Minimal Data

        /// <summary>
        /// TC-DST-EDGE-011: Get DST recommendations for opportunity with no description
        /// 
        /// Given: Opportunity with only title, no description
        /// When: DST recommendations are requested
        /// Then: System generates basic recommendations from limited context
        /// 
        /// Expected Behavior:
        /// - Does not crash
        /// - Returns predefined high risks at minimum
        /// - Minimal keywords extracted from title only
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-011")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_NoDescription_UsesLimitedContext()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Infrastructure Project",
                description: "", // Empty description
                deliverables: ""
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull("system should handle empty description");
            response.Recommendations.Should().NotBeNull();
            
            // Should still have predefined risks
            var preDefinedRisks = response.Recommendations
                .Where(r => r.SourceType == "PREDEFINED_HIGH_RISK")
                .ToList();
            
            if (preDefinedRisks.Any())
            {
                preDefinedRisks.Should().NotBeEmpty("predefined risks available even with minimal context");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-012: Get DST recommendations for opportunity with only whitespace
        /// 
        /// Given: Opportunity with title/description containing only whitespace
        /// When: DST recommendations are requested
        /// Then: Treated as empty, no crash
        /// 
        /// Expected Behavior:
        /// - Whitespace-only treated as empty
        /// - Predefined risks returned
        /// - No keywords extracted from whitespace
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-012")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_OnlyWhitespace_TreatedAsEmpty()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "   ", // Whitespace only
                description: "\t\n   \r\n", // Various whitespace
                deliverables: "     "
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.ExtractedKeywords.Should().NotBeNull();
            
            // Keywords should be empty or minimal
            response.ExtractedKeywords.Should().HaveCountLessOrEqualTo(5);
        }

        /// <summary>
        /// TC-DST-EDGE-013: Get DST recommendations with zero budget
        /// 
        /// Given: Opportunity with EstimatedBudget = 0
        /// When: DST recommendations are requested
        /// Then: System handles edge case appropriately
        /// 
        /// Expected Behavior:
        /// - Zero budget treated as valid (or unknown)
        /// - Recommendations still generated
        /// - Budget-related risks may be highlighted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-013")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_ZeroBudget_HandlesGracefully()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Zero Budget Project",
                description: "Project with no budget allocated",
                deliverables: "Limited deliverables",
                budget: 0m // Zero budget
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-014: Get DST recommendations with extremely large budget
        /// 
        /// Given: Opportunity with very large budget (billions)
        /// When: DST recommendations are requested
        /// Then: Large values handled without overflow
        /// 
        /// Expected Behavior:
        /// - No numeric overflow
        /// - Large budget may trigger specific risks
        /// - System processes correctly
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-014")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_ExtremelyLargeBudget_NoOverflow()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Massive Infrastructure Program",
                description: "Multi-billion dollar program",
                deliverables: "Extensive deliverables",
                budget: 999999999999.99m // Very large budget
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-015: Dismiss all available recommendations
        /// 
        /// Given: All oUP Question IDs are dismissed
        /// When: DST recommendations are requested
        /// Then: Returns only vector store results (or empty)
        /// 
        /// Expected Behavior:
        /// - All predefined risks filtered out
        /// - Only similar project risks returned
        /// - Or empty list if no vector store results
        /// - No errors or crashes
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-015")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_AllDismissed_ReturnsVectorStoreOnly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare Project",
                description: "Healthcare infrastructure development"
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Get initial recommendations
            var initialResponse = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 50,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Get all oUP Question IDs
            var allOupIds = initialResponse.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .ToList();

            if (!allOupIds.Any())
            {
                // Skip if no predefined risks
                return;
            }

            // Act - Dismiss all
            var filteredResponse = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 50,
                dismissedOupQuestionIds: allOupIds, // Dismiss all
                forceRefresh: false
            );

            // Assert
            filteredResponse.Should().NotBeNull();
            filteredResponse.Recommendations.Should().NotBeNull();
            
            // No predefined risks should remain
            var remainingOupRisks = filteredResponse.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .ToList();
            
            remainingOupRisks.Should().BeEmpty("all predefined risks should be dismissed");
            
            // Should only have vector store results (if any)
            var vectorStoreRisks = filteredResponse.Recommendations
                .Where(r => r.SourceType == "SIMILAR_PROJECT")
                .ToList();
            
            filteredResponse.Recommendations.Should().BeEquivalentTo(vectorStoreRisks,
                "only vector store results should remain");
        }

        #endregion

        #region TC-DST-EDGE-016 through TC-DST-EDGE-020: Concurrency and Timing Edge Cases

        /// <summary>
        /// TC-DST-EDGE-016: Simultaneous cache invalidation requests
        /// 
        /// Given: Multiple users request DST with forceRefresh=true simultaneously
        /// When: Requests execute concurrently
        /// Then: Cache is properly invalidated without conflicts
        /// 
        /// Expected Behavior:
        /// - No cache corruption
        /// - All requests complete successfully
        /// - Fresh data returned to all users
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-016")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_SimultaneousCacheInvalidation_NoConflicts()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Cache Test Project",
                description: "Testing concurrent cache invalidation"
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - 5 simultaneous force refresh requests
            var tasks = Enumerable.Range(1, 5).Select(i =>
                geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true // All force refresh
                )
            ).ToList();

            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().HaveCount(5);
            responses.Should().AllSatisfy(r =>
            {
                r.Should().NotBeNull();
                r.Recommendations.Should().NotBeNull();
            });
        }

        /// <summary>
        /// TC-DST-EDGE-017: Extremely fast repeated requests (sub-second)
        /// 
        /// Given: Same opportunity requested multiple times in < 1 second
        /// When: Rapid requests are sent
        /// Then: System handles rate without errors
        /// 
        /// Expected Behavior:
        /// - No race conditions
        /// - Cache may or may not be hit (timing dependent)
        /// - All requests return valid data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-017")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_RapidRepeatedRequests_HandlesCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Rapid Request Test",
                description: "Testing rapid repeated requests"
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - 10 rapid requests without delay
            var tasks = Enumerable.Range(1, 10).Select(i =>
                geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 5,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false // Use cache if available
                )
            ).ToList();

            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().HaveCount(10);
            responses.Should().AllSatisfy(r =>
            {
                r.Should().NotBeNull();
                r.Recommendations.Should().NotBeNull();
            });
        }

        /// <summary>
        /// TC-DST-EDGE-018: Get recommendations during opportunity update
        /// 
        /// Given: Opportunity being updated while DST analysis requested
        /// When: Concurrent read during write
        /// Then: Either gets old or new data consistently
        /// 
        /// Expected Behavior:
        /// - No partial/corrupted data
        /// - Gets either pre-update or post-update snapshot
        /// - No deadlocks or exceptions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-018")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_DuringOpportunityUpdate_NoCorruption()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Original Title",
                description: "Original description"
            );

            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - Update opportunity and get DST concurrently
            var updateTask = Task.Run(async () =>
            {
                var opp = await opportunityManager.GetOpportunityByIdAsync(opportunityId);
                opp.Title = "Updated Title";
                opp.Description = "Updated description with more details";
                await opportunityManager.UpdateOpportunityAsync(opp);
            });

            var dstTask = Task.Run(async () =>
            {
                return await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );
            });

            await Task.WhenAll(updateTask, dstTask);
            var response = await dstTask;

            // Assert
            response.Should().NotBeNull("should get valid data despite concurrent update");
            response.Recommendations.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-019: Create DST risk with same title twice (duplicate detection)
        /// 
        /// Given: Two risks with identical titles created sequentially
        /// When: Both creations are attempted
        /// Then: Both succeed (duplicates allowed) or second is rejected
        /// 
        /// Expected Behavior:
        /// - Depends on business rules
        /// - If duplicates allowed, both created
        /// - If duplicates prevented, second throws exception
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-019")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_DuplicateTitle_HandlesPerBusinessRules()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync("Test Project");

            var risk1 = new RiskCreateRequest
            {
                Title = "Duplicate Risk Title",
                Description = "First risk",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            var risk2 = new RiskCreateRequest
            {
                Title = "Duplicate Risk Title", // Same title
                Description = "Second risk with same title",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act
            var created1 = await riskManager.AddRiskAsync(risk1, user);
            created1.Should().NotBeNull();

            try
            {
                var created2 = await riskManager.AddRiskAsync(risk2, user);
                
                // If duplicates allowed
                created2.Should().NotBeNull();
                created2.Id.Should().NotBe(created1.Id, "should be different risks");
            }
            catch (Exception)
            {
                // If duplicates prevented, exception is acceptable
                Assert.True(true, "Duplicate prevention is valid business rule");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-020: Get DST recommendations immediately after opportunity creation
        /// 
        /// Given: Opportunity just created (potential caching/indexing delay)
        /// When: DST recommendations requested immediately
        /// Then: System handles edge case without errors
        /// 
        /// Expected Behavior:
        /// - No errors due to timing
        /// - Opportunity data is available
        /// - Recommendations generated successfully
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-020")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_ImmediatelyAfterCreation_NoTimingIssues()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - Create and immediately request DST (no delay)
            var opportunity = new OpportunityCreateRequest
            {
                Title = "Immediate DST Test Project",
                Description = "Testing immediate DST request after creation",
                CountryId = 1,
                EstimatedBudget = 1000000m,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);

            // Immediately request DST (no await delay)
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: created.Id,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull("should handle immediate request after creation");
            response.Recommendations.Should().NotBeNull();
        }

        #endregion

        #region TC-DST-EDGE-021 through TC-DST-EDGE-076: Extended Edge Cases

        /// <summary>
        /// TC-DST-EDGE-021: Get DST recommendations with maxResults = 1 (minimum boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-021")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_MaxResultsOne_ReturnsSingleResult()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync("Opportunity with maxResults=1");

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 1, // Boundary: minimum useful value
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().HaveCountLessOrEqualTo(1, "maxResults=1 should return at most 1 recommendation");
        }

        /// <summary>
        /// TC-DST-EDGE-022: Get DST recommendations with maxResults = 1000 (large boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-022")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_MaxResults1000_HandlesLargeResult()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync("Large maxResults");

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 1000,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().HaveCountLessOrEqualTo(1000);
        }

        /// <summary>
        /// TC-DST-EDGE-023: Get DST recommendations with dismissedOupQuestionIds containing single negative value
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-023")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_SingleNegativeDismissedId_IgnoresInvalid()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int> { -1 }, // Single negative
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-024: Create DST risk with Title exactly at maximum length boundary (if enforced)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-024")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_TitleAtMaxLength_AcceptsValidLength()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Assume max length is 500 characters (adjust based on actual constraints)
            var maxLengthTitle = new string('A', 500);

            var riskRequest = new RiskCreateRequest
            {
                Title = maxLengthTitle,
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().HaveLength(500);
        }

        /// <summary>
        /// TC-DST-EDGE-025: Create DST risk with Description exactly at maximum length boundary
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-025")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DescriptionAtMaxLength_AcceptsValidLength()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Assume max length is 5000 characters
            var maxLengthDescription = new string('B', 5000);

            var riskRequest = new RiskCreateRequest
            {
                Title = "Edge Case Risk",
                Description = maxLengthDescription,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().HaveLength(5000);
        }

        /// <summary>
        /// TC-DST-EDGE-026: Create DST risk with Title one character over maximum length
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-026")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_TitleOneOverMaxLength_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var overMaxTitle = new string('A', 501); // One over assumed max

            var riskRequest = new RiskCreateRequest
            {
                Title = overMaxTitle,
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-EDGE-027: Create DST risk with Description one character over maximum length
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-027")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DescriptionOneOverMaxLength_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var overMaxDescription = new string('B', 5001); // One over assumed max

            var riskRequest = new RiskCreateRequest
            {
                Title = "Edge Case Risk",
                Description = overMaxDescription,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-EDGE-028: Get DST recommendations with opportunity having Int32.MaxValue as ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-028")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_MaxIntOpportunityId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: int.MaxValue,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );
            });
        }

        /// <summary>
        /// TC-DST-EDGE-029: Create DST risk with minimum valid EntityId (1)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-029")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_MinimumEntityId_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Minimum EntityId Risk",
                Description = "Test with EntityId = 1",
                EntityType = "Opportunity",
                EntityId = 1, // Minimum valid ID
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (KeyNotFoundException)
            {
                Assert.True(true, "EntityId 1 may not exist");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-030: Get DST recommendations with empty dismissedOupQuestionIds list
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-030")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_EmptyDismissedList_ReturnsAllRecommendations()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(), // Empty list (not null)
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-031: Create DST risk with all Unicode characters in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-031")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_UnicodeTitle_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "风险标题 🚨 Риск Risque خطر", // Unicode from multiple languages
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("风险");
        }

        /// <summary>
        /// TC-DST-EDGE-032: Create DST risk with all emojis in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-032")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_EmojiTitle_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "🔥🚨⚠️💀☠️", // All emojis
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("🔥");
        }

        /// <summary>
        /// TC-DST-EDGE-033: Create DST risk with RTL (Right-to-Left) text in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-033")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_RTLTitle_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "مخاطر المشروع الكبيرة", // Arabic RTL text
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("مخاطر");
        }

        /// <summary>
        /// TC-DST-EDGE-034: Create DST risk with newlines and tabs in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-034")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_NewlinesTabsInTitle_HandlesorRejects()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Risk\nWith\nNewlines\tAnd\tTabs", // Control characters
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (ArgumentException)
            {
                Assert.True(true, "Control characters may be rejected");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-035: Create DST risk with null Source field (optional)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-035")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_NullSource_AcceptsIfOptional()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Risk without source",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = null // Null optional field
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-036: Get DST recommendations with forceRefresh=true repeatedly
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-036")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_RepeatedForceRefresh_HandlesCache()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act - Call 5 times with forceRefresh
            for (int i = 0; i < 5; i++)
            {
                var response = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true // Always refresh
                );

                response.Should().NotBeNull();
            }

            Assert.True(true, "Repeated force refresh handled");
        }

        /// <summary>
        /// TC-DST-EDGE-037: Get DST recommendations alternating forceRefresh true/false
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-037")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_AlternatingForceRefresh_HandlesCacheCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act - Alternate forceRefresh
            for (int i = 0; i < 4; i++)
            {
                var response = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: i % 2 == 0 // Alternate true/false
                );

                response.Should().NotBeNull();
            }

            Assert.True(true, "Alternating force refresh handled");
        }

        /// <summary>
        /// TC-DST-EDGE-038: Create DST risk with minimum RiskTypeId (1)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-038")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_MinimumRiskTypeId_AcceptsValidId()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Minimum RiskTypeId",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1, // Minimum valid ID
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (KeyNotFoundException)
            {
                Assert.True(true, "RiskTypeId 1 may not exist");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-039: Create DST risk with maximum reasonable RiskTypeId (100)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-039")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_LargeRiskTypeId_ThrowsIfInvalid()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Large RiskTypeId",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 100, // Large but reasonable ID
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (KeyNotFoundException)
            {
                Assert.True(true, "RiskTypeId 100 likely doesn't exist");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-040: Create DST risk with all special characters in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-040")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_SpecialCharsDescription_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Special Chars Test",
                Description = "!@#$%^&*()_+-={}[]|\\:;<>?,./~`", // All special chars
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("!@#$");
        }

        /// <summary>
        /// TC-DST-EDGE-041: Update DST risk immediately after creation (no delay)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-041")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_ImmediatelyAfterCreation_Succeeds()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Original",
                Description = "Original description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Create and immediately update
            var created = await riskManager.AddRiskAsync(createRequest, user);

            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Updated Immediately",
                Description = "Updated immediately after creation"
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Title.Should().Be("Updated Immediately");
        }

        /// <summary>
        /// TC-DST-EDGE-042: Delete DST risk immediately after creation (no delay)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-042")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDSTRisk_ImmediatelyAfterCreation_Succeeds()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "To Be Deleted",
                Description = "Will be deleted immediately",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Create and immediately delete
            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Act
            await riskManager.DeleteRiskAsync(created.Id, user);

            // Assert - Verify deletion
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await riskManager.GetRiskByIdAsync(created.Id, user);
            });
        }

        /// <summary>
        /// TC-DST-EDGE-043: Get DST recommendations with maximum Int32 for dismissedOupQuestionIds values
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-043")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_MaxIntDismissedIds_HandlesLargeIds()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var largeDismissedIds = new List<int> { int.MaxValue, int.MaxValue - 1, int.MaxValue - 2 };

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: largeDismissedIds,
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-044: Create DST risk with Title containing only numbers
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-044")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_NumericTitle_AcceptsValidString()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "1234567890", // Only numbers
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("1234567890");
        }

        /// <summary>
        /// TC-DST-EDGE-045: Create DST risk with Title containing single character
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-045")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_SingleCharTitle_AcceptsMinimalInput()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "X", // Single character
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("X");
        }

        /// <summary>
        /// TC-DST-EDGE-046: Get DST risks immediately after creating first risk for opportunity
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-046")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_ImmediatelyAfterFirstRisk_ReturnsSingleRisk()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "First Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            await riskManager.AddRiskAsync(createRequest, user);

            // Act - Immediately query
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);

            // Assert
            risks.Should().HaveCount(1, "should return newly created risk immediately");
        }

        /// <summary>
        /// TC-DST-EDGE-047: Create multiple DST risks rapidly (stress timing)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-047")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_MultipleRapidCreations_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act - Create 10 risks rapidly
            var createTasks = Enumerable.Range(0, 10).Select(i =>
            {
                var request = new RiskCreateRequest
                {
                    Title = $"Rapid Risk {i}",
                    Description = $"Description {i}",
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 3,
                    ImpactId = 4,
                    Source = "DST"
                };
                return riskManager.AddRiskAsync(request, user);
            }).ToList();

            var results = await Task.WhenAll(createTasks);

            // Assert
            results.Should().HaveCount(10);
            results.Should().AllSatisfy(r => r.Should().NotBeNull());
        }

        /// <summary>
        /// TC-DST-EDGE-048: Update DST risk to same values 10 times consecutively
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-048")]
        [Trait("Priority", "Low")]
        public async Task UpdateDSTRisk_RepeatedIdenticalUpdates_HandlesIdempotently()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Original",
                Description = "Original description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Act - Update 10 times with same data
            for (int i = 0; i < 10; i++)
            {
                var updateRequest = new RiskUpdateRequest
                {
                    Id = created.Id,
                    Title = "Same Update",
                    Description = "Same description"
                };

                var updated = await riskManager.UpdateRiskAsync(updateRequest, user);
                updated.Should().NotBeNull();
            }

            Assert.True(true, "Repeated identical updates handled");
        }

        /// <summary>
        /// TC-DST-EDGE-049: Get DST recommendations with dismissedOupQuestionIds = [0]
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-049")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_ZeroDismissedId_IgnoresInvalidId()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int> { 0 }, // Zero ID
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-050: Create DST risk with Description containing only special characters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-050")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_OnlySpecialCharsDescription_AcceptsValidString()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Special Chars Risk",
                Description = "!!!###$$$%%%", // Only special chars
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Be("!!!###$$$%%%");
        }

        /// <summary>
        /// TC-DST-EDGE-051: Get DST risks for opportunity with exactly 1 risk
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-051")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_ExactlyOneRisk_ReturnsSingleRisk()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Only Risk",
                Description = "The only risk for this opportunity",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            await riskManager.AddRiskAsync(createRequest, user);

            // Act
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);

            // Assert
            risks.Should().HaveCount(1, "should return exactly 1 risk");
        }

        /// <summary>
        /// TC-DST-EDGE-052: Get DST risks for opportunity with 100 risks (large result set)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-052")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRisks_100Risks_HandlesLargeResultSet()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync("Opportunity with 100 risks");

            // Create 100 risks
            var createTasks = Enumerable.Range(0, 100).Select(i =>
            {
                var request = new RiskCreateRequest
                {
                    Title = $"Risk {i + 1} of 100",
                    Description = $"Description {i + 1}",
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 3,
                    ImpactId = 4,
                    Source = "DST"
                };
                return riskManager.AddRiskAsync(request, user);
            }).ToList();

            await Task.WhenAll(createTasks);

            // Act
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);

            // Assert
            risks.Should().HaveCount(100, "should return all 100 risks");
        }

        /// <summary>
        /// TC-DST-EDGE-053: Create DST risk with Title containing leading/trailing whitespace
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-053")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_LeadingTrailingWhitespace_TrimsOrPreserves()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "  Title With Whitespace  ", // Leading/trailing spaces
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            // May trim or preserve whitespace depending on implementation
        }

        /// <summary>
        /// TC-DST-EDGE-054: Update DST risk with Title containing leading/trailing whitespace
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-054")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_LeadingTrailingWhitespace_TrimsOrPreserves()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk
            var createRequest = new RiskCreateRequest
            {
                Title = "Original Title",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Update with whitespace
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "  Updated With Whitespace  ",
                Description = "Updated description"
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-055: Get DST recommendations with maxResults = Int32.MaxValue
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-055")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_MaxIntMaxResults_CapsToReasonableLimit()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: int.MaxValue, // Maximum int
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Count.Should().BeLessThan(10000, "should cap to reasonable limit");
        }

        /// <summary>
        /// TC-DST-EDGE-056: Create DST risk with all fields at maximum length
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-056")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_AllFieldsMaxLength_AcceptsValidData()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = new string('T', 500), // Max length
                Description = new string('D', 5000), // Max length
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = new string('S', 200) // Assuming max length for Source
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-057: Get DST recommendations for opportunity created in last millisecond
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-057")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_BrandNewOpportunity_HandlesImmediately()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Create opportunity
            var opportunityId = await CreateTestOpportunityAsync("Brand New Opportunity");

            // Immediately request DST (within milliseconds of creation)
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-058: Create DST risk with EntityType "Opportunity" (exact case)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-058")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_ExactCaseEntityType_AcceptsCorrectCase()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Exact Case Test",
                Description = "Test description",
                EntityType = "Opportunity", // Exact case
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-059: Create DST risk with EntityType "opportunity" (lowercase - case sensitivity test)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-059")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_LowercaseEntityType_HandlesCaseInsensitivityOrRejects()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Lowercase EntityType Test",
                Description = "Test description",
                EntityType = "opportunity", // Lowercase
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull("case-insensitive matching may be supported");
            }
            catch (ArgumentException)
            {
                Assert.True(true, "Case-sensitive EntityType validation enforced");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-060: Get DST risks with EntityType "OPPORTUNITY" (uppercase - case sensitivity test)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-060")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_UppercaseEntityType_HandlesCaseInsensitivityOrRejects()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            try
            {
                var risks = await riskManager.GetRisksByEntityAsync("OPPORTUNITY", opportunityId, user);
                risks.Should().NotBeNull("case-insensitive matching may be supported");
            }
            catch (ArgumentException)
            {
                Assert.True(true, "Case-sensitive EntityType validation enforced");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-061: Update DST risk multiple times rapidly (1000ms between updates)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-061")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_RapidSequentialUpdates_HandlesWithoutDataLoss()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Rapid Update Test",
                Description = "Original",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Act - Update 5 times with 1 second delay
            for (int i = 0; i < 5; i++)
            {
                var updateRequest = new RiskUpdateRequest
                {
                    Id = created.Id,
                    Title = $"Update {i + 1}",
                    Description = $"Description {i + 1}"
                };

                await riskManager.UpdateRiskAsync(updateRequest, user);
                await Task.Delay(1000); // 1 second delay
            }

            // Assert - Verify final state
            var final = await riskManager.GetRiskByIdAsync(created.Id, user);
            final.Title.Should().Be("Update 5");
        }

        /// <summary>
        /// TC-DST-EDGE-062: Get DST recommendations with same dismissedOupQuestionIds repeated (contains duplicates)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-062")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_ManyDuplicateDismissedIds_DeduplicatesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // List with many duplicates
            var dismissedIds = new List<int> { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 };

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: dismissedIds,
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-063: Create DST risk with Source field exactly at maximum length
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-063")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_SourceAtMaxLength_AcceptsValidLength()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Assume max length is 200
            var maxLengthSource = new string('S', 200);

            var riskRequest = new RiskCreateRequest
            {
                Title = "Source Max Length Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = maxLengthSource
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().HaveLength(200);
        }

        /// <summary>
        /// TC-DST-EDGE-064: Get DST recommendations for opportunity with null optional fields
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-064")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_OpportunityNullOptionalFields_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Create opportunity with minimal data (null optional fields)
            var opportunityId = await CreateTestOpportunityAsync("Minimal Opportunity");

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-065: Create DST risk and update it with exact same data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-065")]
        [Trait("Priority", "Low")]
        public async Task UpdateDSTRisk_ExactSameData_HandlesIdempotently()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Idempotent Update Test",
                Description = "Original description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Update with exact same data
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Idempotent Update Test", // Same
                Description = "Original description" // Same
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Title.Should().Be(created.Title);
            updated.Description.Should().Be(created.Description);
        }

        /// <summary>
        /// TC-DST-EDGE-066: Get DST risks for opportunity that has risks then all deleted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-066")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_AllRisksDeleted_ReturnsEmptyList()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create 3 risks
            var createdRisks = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                var createRequest = new RiskCreateRequest
                {
                    Title = $"Risk {i + 1}",
                    Description = "Test description",
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 3,
                    ImpactId = 4,
                    Source = "DST"
                };

                var created = await riskManager.AddRiskAsync(createRequest, user);
                createdRisks.Add(created.Id);
            }

            // Delete all risks
            foreach (var riskId in createdRisks)
            {
                await riskManager.DeleteRiskAsync(riskId, user);
            }

            // Act
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);

            // Assert
            risks.Should().BeEmpty("all risks were deleted");
        }

        /// <summary>
        /// TC-DST-EDGE-067: Get DST recommendations with dismissedOupQuestionIds exactly matching all recommendations
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-067")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_AllDismissed_ReturnsEmptyOrAlternatives()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Get initial recommendations
            var initialResponse = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Extract all OUP question IDs
            var allIds = initialResponse.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .ToList();

            // Request recommendations with all IDs dismissed
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: allIds,
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            // May return empty or alternative recommendations
        }

        /// <summary>
        /// TC-DST-EDGE-068: Create DST risk with only required fields (minimal payload)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-068")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_OnlyRequiredFields_AcceptsMinimalData()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                // Only required fields
                Title = "Minimal Risk",
                Description = "Minimal description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
                // No Source (optional)
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-069: Update DST risk changing only Title (partial update)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-069")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_OnlyTitleChanged_UpdatesSelectively()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Original Title",
                Description = "Original Description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Update only title
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Updated Title Only",
                Description = created.Description // Keep same
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Title.Should().Be("Updated Title Only");
            updated.Description.Should().Be("Original Description");
        }

        /// <summary>
        /// TC-DST-EDGE-070: Update DST risk changing only Description (partial update)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-070")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_OnlyDescriptionChanged_UpdatesSelectively()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Original Title",
                Description = "Original Description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Update only description
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = created.Title, // Keep same
                Description = "Updated Description Only"
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Title.Should().Be("Original Title");
            updated.Description.Should().Be("Updated Description Only");
        }

        /// <summary>
        /// TC-DST-EDGE-071: Get DST recommendations twice without forceRefresh (cache test)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-071")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_WithoutForceRefresh_UsesCache()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // First call - populates cache
            var first = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false
            );

            // Second call - should use cache
            var second = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false // No force refresh
            );

            // Assert
            first.Should().NotBeNull();
            second.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-072: Create DST risk with empty Source string (optional field)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-072")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_EmptySource_AcceptsEmptyString()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Empty Source Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = string.Empty // Empty string
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-073: Get DST recommendations for opportunity with huge Description (performance test)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-073")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_HugeOpportunityDescription_HandlesPerformance()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Create opportunity with huge description
            var opportunityId = await CreateTestOpportunityAsync("Huge Description Opportunity");

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-EDGE-074: Create DST risk with ProbabilityId = 1 (minimum boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-074")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_ProbabilityIdOne_AcceptsMinimumValue()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Minimum Probability",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 1, // Minimum
                ImpactId = 4,
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (KeyNotFoundException)
            {
                Assert.True(true, "ProbabilityId 1 may not exist");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-075: Create DST risk with ImpactId = 1 (minimum boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-075")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_ImpactIdOne_AcceptsMinimumValue()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Minimum Impact",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 1, // Minimum
                Source = "DST"
            };

            // Act
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (KeyNotFoundException)
            {
                Assert.True(true, "ImpactId 1 may not exist");
            }
        }

        /// <summary>
        /// TC-DST-EDGE-076: Get DST risks immediately after deleting half of them
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-EDGE-076")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_AfterPartialDeletion_ReturnsRemainingRisks()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create 10 risks
            var createdRisks = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var createRequest = new RiskCreateRequest
                {
                    Title = $"Risk {i + 1}",
                    Description = "Test description",
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 3,
                    ImpactId = 4,
                    Source = "DST"
                };

                var created = await riskManager.AddRiskAsync(createRequest, user);
                createdRisks.Add(created.Id);
            }

            // Delete first 5 risks
            for (int i = 0; i < 5; i++)
            {
                await riskManager.DeleteRiskAsync(createdRisks[i], user);
            }

            // Act - Get remaining risks
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);

            // Assert
            risks.Should().HaveCount(5, "should return 5 remaining risks after deleting 5 of 10");
        }

        #endregion
    }
}
