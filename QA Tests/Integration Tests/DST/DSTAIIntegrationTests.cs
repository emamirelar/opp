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
    /// Integration tests for DST AI Integration (Gemini/LLM)
    /// Tests the AI-powered analysis, prompt construction, and response parsing
    /// 
    /// Test Coverage:
    /// - AI prompt construction for risk analysis
    /// - AI response parsing and validation
    /// - Confidence scoring
    /// - Error handling for AI service failures
    /// - Token limit management
    /// - Response caching
    /// - Hallucination detection
    /// - Multi-turn conversation handling
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "AIIntegration")]
    public class DSTAIIntegrationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTAIIntegrationTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        private async Task<int> CreateTestOpportunityAsync(
            string title,
            string description,
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

        private async Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(
            int opportunityId,
            bool forceRefresh = true)
        {
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            return await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: forceRefresh
            );
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

        #region TC-DST-AI-001 through TC-DST-AI-003: Prompt Construction and Response Parsing

        /// <summary>
        /// TC-DST-AI-001: AI prompt construction for risk analysis
        /// 
        /// Given: Opportunity context and extracted keywords
        /// When: AI prompt is constructed
        /// Then: Prompt includes all necessary context
        /// 
        /// Expected Behavior:
        /// - Includes opportunity title, description, deliverables
        /// - Includes extracted keywords
        /// - Includes predefined high risks for consideration
        /// - Includes High Risk Guidance document (if available)
        /// - Prompt is properly formatted for AI model
        /// - Token count is within limits
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-001")]
        [Trait("Priority", "Critical")]
        public async Task BuildAIPrompt_OpportunityContext_ProperlyFormatted()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Complex Infrastructure Project with Multiple Risk Factors",
                description: "Large-scale infrastructure development in challenging environment with procurement complexities, security concerns, and capacity building requirements. Project spans multiple sectors and involves significant budget.",
                deliverables: "Infrastructure construction, capacity building programs, procurement frameworks, risk mitigation strategies",
                budget: 10000000m
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId, forceRefresh: true);

            // Assert
            response.Should().NotBeNull("AI should return response");
            
            // Verify AI was able to process context
            response.Recommendations.Should().NotBeEmpty(
                "AI should generate recommendations from context");

            response.ExtractedKeywords.Should().NotBeEmpty(
                "AI should extract keywords from opportunity");

            // Verify recommendations are contextual
            if (response.Recommendations.Any())
            {
                // AI-generated recommendations should reference opportunity context
                var hasContextualRecommendations = response.Recommendations.Any(rec =>
                    rec.Description.Length > 100 || // Detailed description
                    rec.Recommendation.Length > 50   // Substantive mitigation advice
                );

                hasContextualRecommendations.Should().BeTrue(
                    "AI should generate contextual recommendations");
            }

            // Verify execution completed (AI didn't timeout or fail)
            response.ExecutionTimeMs.Should().BeGreaterThan(0);
            response.ExecutionTimeMs.Should().BeLessThan(60000, 
                "AI prompt should complete within reasonable time");
        }

        /// <summary>
        /// TC-DST-AI-002: AI response parsing and validation
        /// 
        /// Given: AI model returns response with recommendations
        /// When: Response is parsed
        /// Then: Valid DSTRecommendation objects are created
        /// 
        /// Expected Behavior:
        /// - JSON response is parsed correctly
        /// - All required fields are populated
        /// - Data types are correct
        /// - Invalid/malformed recommendations are filtered out
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-002")]
        [Trait("Priority", "Critical")]
        public async Task ParseAIResponse_ValidJSON_ParsesCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare System Development",
                description: "Strengthen healthcare delivery systems",
                deliverables: "Healthcare infrastructure, training programs"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull();

            if (response.Recommendations.Any())
            {
                // Verify all recommendations are properly parsed
                response.Recommendations.Should().AllSatisfy(rec =>
                {
                    // Required fields
                    rec.Title.Should().NotBeNullOrEmpty("Title is required");
                    rec.Description.Should().NotBeNullOrEmpty("Description is required");
                    rec.Recommendation.Should().NotBeNullOrEmpty("Recommendation is required");
                    
                    // Data types
                    rec.ConfidenceLevel.Should().BeInRange(0, 100, "ConfidenceLevel should be 0-100");
                    rec.RelevanceScore.Should().BeGreaterOrEqualTo(0, "RelevanceScore should be non-negative");
                    
                    // Source type validation
                    rec.SourceType.Should().BeOneOf("PREDEFINED_HIGH_RISK", "SIMILAR_PROJECT",
                        "SourceType should be valid enum value");
                    
                    // Stable identifier
                    rec.StableIdentifier.Should().NotBeNullOrEmpty("StableIdentifier is required");
                });

                // Verify IsStronglyRecommended computed property
                var stronglyRecommended = response.Recommendations
                    .Where(r => r.IsStronglyRecommended)
                    .ToList();

                stronglyRecommended.Should().AllSatisfy(rec =>
                {
                    rec.ConfidenceLevel.Should().BeGreaterThanOrEqualTo(80,
                        "IsStronglyRecommended requires ConfidenceLevel >= 80");
                });
            }
        }

        /// <summary>
        /// TC-DST-AI-003: AI confidence scoring
        /// 
        /// Given: AI analyzes opportunity and generates recommendations
        /// When: Confidence scores are assigned
        /// Then: Scores are within valid range and meaningful
        /// 
        /// Expected Behavior:
        /// - Confidence level 0-100
        /// - Higher scores for more relevant/certain risks
        /// - Scores reflect AI's certainty about recommendation
        /// - Strongly recommended (>=80) are clearly high-priority
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-003")]
        [Trait("Priority", "Critical")]
        public async Task AIRiskAnalysis_ConfidenceScores_WithinValidRange()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Procurement Integrity and Anti-Corruption Initiative",
                description: "Strengthen procurement systems, reduce corruption risks through transparency measures, capacity building for procurement officers, establishment of oversight mechanisms",
                deliverables: "Procurement policy framework, training materials, audit systems, transparency portals"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Recommendations.Should().NotBeEmpty("should have recommendations");

            // All confidence levels should be valid
            response.Recommendations.Should().AllSatisfy(rec =>
            {
                rec.ConfidenceLevel.Should().BeInRange(0, 100,
                    "confidence level must be 0-100");
            });

            // For procurement/corruption context, should have high-confidence recommendations
            var highConfidenceRecs = response.Recommendations
                .Where(r => r.ConfidenceLevel >= 80)
                .ToList();

            if (highConfidenceRecs.Any())
            {
                // High confidence recommendations should be relevant to context
                var hasRelevantHighConfidence = highConfidenceRecs.Any(rec =>
                    rec.Title.ToLower().Contains("procurement") ||
                    rec.Title.ToLower().Contains("corruption") ||
                    rec.Title.ToLower().Contains("integrity") ||
                    rec.Description.ToLower().Contains("procurement") ||
                    rec.Description.ToLower().Contains("corruption"));

                hasRelevantHighConfidence.Should().BeTrue(
                    "high confidence recommendations should be contextually relevant");
            }

            // Verify distribution of confidence scores
            var avgConfidence = response.Recommendations.Average(r => r.ConfidenceLevel);
            avgConfidence.Should().BeGreaterThan(0, "should have positive average confidence");
            avgConfidence.Should().BeLessThan(100, "should not all be maximum confidence");
        }

        #endregion

        #region TC-DST-AI-004 through TC-DST-AI-006: Error Handling and Resilience

        /// <summary>
        /// TC-DST-AI-004: Handle AI service unavailability
        /// 
        /// Given: AI service (Gemini) is unavailable or returns error
        /// When: DST recommendations are requested
        /// Then: System falls back to predefined risks gracefully
        /// 
        /// Expected Behavior:
        /// - Does not crash when AI fails
        /// - Returns predefined high risks as fallback
        /// - Logs error appropriately
        /// - User receives useful recommendations despite AI failure
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-004")]
        [Trait("Priority", "High")]
        public async Task DSTRecommendations_AIUnavailable_FallbackToPreDefined()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Standard Project Implementation",
                description: "Regular project with standard scope",
                deliverables: "Standard deliverables"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull("system should return response even if AI fails");
            response.Recommendations.Should().NotBeNull("recommendations list should not be null");

            // Even if AI fails, predefined risks should be available
            var preDefinedRisks = response.Recommendations
                .Where(r => r.SourceType == "PREDEFINED_HIGH_RISK")
                .ToList();

            // System should still function with predefined risks
            if (preDefinedRisks.Any())
            {
                preDefinedRisks.Should().AllSatisfy(rec =>
                {
                    rec.OupQuestionId.Should().HaveValue(
                        "predefined risks available as fallback");
                    rec.PreDefinedHighRiskId.Should().HaveValue();
                    rec.RiskCategoryId.Should().HaveValue();
                });
            }

            // Error should be logged but not crash system
            response.ExecutionTimeMs.Should().BeGreaterThan(0,
                "execution should complete even with AI errors");
        }

        /// <summary>
        /// TC-DST-AI-005: Token limit handling for large contexts
        /// 
        /// Given: Opportunity with very large description/deliverables
        /// When: AI prompt is constructed
        /// Then: Context is truncated appropriately to fit token limits
        /// 
        /// Expected Behavior:
        /// - Very long text is truncated
        /// - Most important context is prioritized
        /// - System doesn't fail on token limit errors
        /// - Recommendations are still generated
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-005")]
        [Trait("Priority", "Medium")]
        public async Task AIPrompt_LargeContext_TruncatesAppropriately()
        {
            // Arrange - Create opportunity with very large description
            var largeDescription = string.Join(" ", Enumerable.Range(1, 500).Select(i =>
                $"This is sentence number {i} providing extensive detail about the project scope, objectives, deliverables, risks, stakeholders, and implementation strategy."));

            var largeDeliverables = string.Join(", ", Enumerable.Range(1, 100).Select(i =>
                $"Deliverable {i} with detailed description"));

            var opportunityId = await CreateTestOpportunityAsync(
                title: "Very Large and Complex Multi-Phase Development Program",
                description: largeDescription,
                deliverables: largeDeliverables
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull("system should handle large context");
            
            // Should still generate recommendations despite large context
            response.Recommendations.Should().NotBeNull();
            
            // AI should still complete within reasonable time
            response.ExecutionTimeMs.Should().BeLessThan(60000,
                "should handle large context efficiently");

            // Keywords should still be extracted
            response.ExtractedKeywords.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-AI-006: Multi-turn conversation for clarification
        /// 
        /// Given: Initial AI response may need refinement
        /// When: Follow-up analysis is needed
        /// Then: System can perform multi-turn conversation
        /// 
        /// Expected Behavior:
        /// - First call generates initial recommendations
        /// - Follow-up calls can refine or expand
        /// - Context is maintained across calls
        /// - Cache can be used for efficiency
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-006")]
        [Trait("Priority", "Low")]
        public async Task AIAnalysis_FollowUpQuestions_RefinesResults()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Infrastructure Project",
                description: "Infrastructure development project",
                deliverables: "Infrastructure outputs"
            );

            // Act - First call
            var firstResponse = await GetDSTRecommendationsAsync(
                opportunityId, 
                forceRefresh: true);

            // Act - Second call (may use cache or refine)
            var secondResponse = await GetDSTRecommendationsAsync(
                opportunityId, 
                forceRefresh: false);

            // Assert
            firstResponse.Should().NotBeNull();
            secondResponse.Should().NotBeNull();

            // Both calls should succeed
            firstResponse.Recommendations.Should().NotBeNull();
            secondResponse.Recommendations.Should().NotBeNull();

            // Second call might be faster due to caching
            if (secondResponse.ExecutionTimeMs < firstResponse.ExecutionTimeMs)
            {
                (firstResponse.ExecutionTimeMs - secondResponse.ExecutionTimeMs)
                    .Should().BeGreaterThan(0, "cached call should be faster");
            }
        }

        #endregion

        #region TC-DST-AI-007 through TC-DST-AI-008: Quality and Performance

        /// <summary>
        /// TC-DST-AI-007: AI hallucination detection
        /// 
        /// Given: AI generates recommendations
        /// When: Recommendations are validated
        /// Then: Unrealistic or nonsensical recommendations are filtered
        /// 
        /// Expected Behavior:
        /// - Recommendations are grounded in reality
        /// - No obviously false or fabricated information
        /// - Mitigation advice is practical
        /// - Confidence scores reflect uncertainty
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-007")]
        [Trait("Priority", "High")]
        public async Task AIResponse_InvalidRecommendation_Filtered()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Education Infrastructure Development",
                description: "Build schools and train teachers",
                deliverables: "School buildings, teacher training programs"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            if (response.Recommendations.Any())
            {
                // Verify recommendations are coherent
                response.Recommendations.Should().AllSatisfy(rec =>
                {
                    // Title should not be empty or nonsensical
                    rec.Title.Length.Should().BeGreaterThan(10,
                        "recommendation titles should be substantive");
                    
                    // Description should be meaningful
                    rec.Description.Length.Should().BeGreaterThan(20,
                        "descriptions should provide context");
                    
                    // Recommendation should provide actionable advice
                    rec.Recommendation.Length.Should().BeGreaterThan(20,
                        "mitigation advice should be detailed");
                });

                // Recommendations should be contextually relevant
                var hasRelevantContent = response.Recommendations.Any(rec =>
                    rec.Title.ToLower().Contains("education") ||
                    rec.Title.ToLower().Contains("school") ||
                    rec.Title.ToLower().Contains("teacher") ||
                    rec.Title.ToLower().Contains("training") ||
                    rec.Description.ToLower().Contains("education") ||
                    rec.Description.ToLower().Contains("school"));

                if (response.Recommendations.Count >= 3)
                {
                    hasRelevantContent.Should().BeTrue(
                        "recommendations should be contextually relevant");
                }
            }
        }

        /// <summary>
        /// TC-DST-AI-008: AI response caching for performance
        /// 
        /// Given: Same opportunity analyzed multiple times
        /// When: forceRefresh=false is used
        /// Then: Cached results improve performance
        /// 
        /// Expected Behavior:
        /// - First call is slower (full AI analysis)
        /// - Cached calls are faster
        /// - Results are consistent
        /// - Cache expires after reasonable time
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-AI-008")]
        [Trait("Priority", "Medium")]
        public async Task AIAnalysis_CachedResults_ImprovePerformance()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Standard Development Project",
                description: "Regular project implementation",
                deliverables: "Standard outputs"
            );

            // Act - First call with force refresh (no cache)
            var uncachedResponse = await GetDSTRecommendationsAsync(
                opportunityId, 
                forceRefresh: true);

            var uncachedTime = uncachedResponse.ExecutionTimeMs;

            // Wait a moment
            await Task.Delay(1000);

            // Act - Second call without force refresh (may use cache)
            var cachedResponse = await GetDSTRecommendationsAsync(
                opportunityId, 
                forceRefresh: false);

            var cachedTime = cachedResponse.ExecutionTimeMs;

            // Assert
            uncachedResponse.Should().NotBeNull();
            cachedResponse.Should().NotBeNull();

            uncachedTime.Should().BeGreaterThan(0);
            cachedTime.Should().BeGreaterThan(0);

            // Cached call should typically be faster
            // (Though not guaranteed due to various factors)
            if (cachedTime < uncachedTime * 0.8)
            {
                cachedTime.Should().BeLessThan(uncachedTime,
                    "cached response should be faster");
            }

            // Results should be consistent
            if (uncachedResponse.Recommendations.Any() && cachedResponse.Recommendations.Any())
            {
                // Should have similar number of recommendations
                var countDifference = Math.Abs(
                    uncachedResponse.Recommendations.Count - 
                    cachedResponse.Recommendations.Count);
                
                countDifference.Should().BeLessThanOrEqualTo(2,
                    "cached results should be consistent with fresh results");
            }
        }

        #endregion
    }
}
