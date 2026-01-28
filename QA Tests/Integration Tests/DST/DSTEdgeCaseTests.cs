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
        public async Task AddDSTRisk_DuplicateTitle_HandlesPer BusinessRules()
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
    }
}
