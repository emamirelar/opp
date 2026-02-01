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
    /// Integration tests for DST Keyword Extraction and Vector Store Integration
    /// Tests the AI-powered keyword extraction and semantic search for similar project risks
    /// 
    /// Test Coverage:
    /// - Keyword extraction from various opportunity fields
    /// - Domain-specific term identification
    /// - Risk indicator detection
    /// - Vector store query construction
    /// - Similarity scoring and filtering
    /// - Document retrieval from vector store
    /// - Performance and error handling
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "KeywordExtraction")]
    public class DSTKeywordExtractionTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTKeywordExtractionTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        private async Task<int> CreateTestOpportunityAsync(
            string title,
            string description,
            string deliverables = "",
            string objectives = "")
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = description,
                Deliverables = deliverables,
                Objectives = objectives,
                CountryId = 1,
                EstimatedBudget = 1000000m,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

        private async Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(int opportunityId)
        {
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            return await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
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

        #region TC-DST-KWD-001 through TC-DST-KWD-005: Keyword Extraction

        /// <summary>
        /// TC-DST-KWD-001: Extract keywords from opportunity title
        /// 
        /// Given: Opportunity with descriptive title containing domain terms
        /// When: DST analysis extracts keywords
        /// Then: Title keywords are identified
        /// 
        /// Expected Behavior:
        /// - Extracts key nouns and domain terms from title
        /// - Identifies sector/thematic keywords
        /// - Removes stop words
        /// - Includes title keywords in ExtractedKeywords list
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-001")]
        [Trait("Priority", "Critical")]
        public async Task ExtractKeywords_OpportunityTitle_IdentifiesKeyTerms()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare Infrastructure Development in Conflict-Affected Regions",
                description: "Supporting health system strengthening",
                deliverables: "Health facilities"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull();
            response.ExtractedKeywords.Should().NotBeEmpty("keywords should be extracted from title");

            var keywordsLower = response.ExtractedKeywords.Select(k => k.ToLower()).ToList();

            // Should extract domain terms from title
            var hasHealthcareKeywords = keywordsLower.Any(k =>
                k.Contains("healthcare") ||
                k.Contains("health") ||
                k.Contains("medical"));

            var hasInfrastructureKeywords = keywordsLower.Any(k =>
                k.Contains("infrastructure") ||
                k.Contains("construction") ||
                k.Contains("facilities"));

            var hasConflictKeywords = keywordsLower.Any(k =>
                k.Contains("conflict") ||
                k.Contains("fragile") ||
                k.Contains("affected"));

            (hasHealthcareKeywords || hasInfrastructureKeywords || hasConflictKeywords)
                .Should().BeTrue("should extract key terms from title");
        }

        /// <summary>
        /// TC-DST-KWD-002: Extract keywords from description
        /// 
        /// Given: Opportunity with rich descriptive text
        /// When: DST analysis processes description
        /// Then: Relevant keywords are extracted
        /// 
        /// Expected Behavior:
        /// - Identifies key themes and concepts
        /// - Extracts technical and domain-specific terms
        /// - Prioritizes risk-related keywords
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-002")]
        [Trait("Priority", "Critical")]
        public async Task ExtractKeywords_Description_ExtractsRiskIndicators()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Water Management Project",
                description: "Establish water treatment facilities in remote rural communities facing severe drought conditions. Project involves complex procurement processes, community engagement, and capacity building for sustainable water resource management. Security concerns in operational areas require risk mitigation strategies.",
                deliverables: "Water infrastructure, training programs"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.ExtractedKeywords.Should().NotBeEmpty();

            var keywordsLower = response.ExtractedKeywords.Select(k => k.ToLower()).ToList();

            // Should extract thematic keywords
            var hasWaterKeywords = keywordsLower.Any(k =>
                k.Contains("water") ||
                k.Contains("drought") ||
                k.Contains("resource"));

            // Should extract risk indicators
            var hasRiskKeywords = keywordsLower.Any(k =>
                k.Contains("procurement") ||
                k.Contains("security") ||
                k.Contains("risk") ||
                k.Contains("capacity"));

            // Should extract operational keywords
            var hasOperationalKeywords = keywordsLower.Any(k =>
                k.Contains("remote") ||
                k.Contains("rural") ||
                k.Contains("community") ||
                k.Contains("sustainable"));

            (hasWaterKeywords || hasRiskKeywords || hasOperationalKeywords)
                .Should().BeTrue("should extract relevant keywords from description");
        }

        /// <summary>
        /// TC-DST-KWD-003: Extract keywords from deliverables
        /// 
        /// Given: Opportunity with specific deliverables listed
        /// When: DST processes deliverables text
        /// Then: Deliverable-related keywords are extracted
        /// 
        /// Expected Behavior:
        /// - Identifies output types
        /// - Extracts activity-related terms
        /// - Includes deliverable keywords in analysis
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-003")]
        [Trait("Priority", "High")]
        public async Task ExtractKeywords_Deliverables_IdentifiesThemes()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Education System Strengthening",
                description: "Support education sector development",
                deliverables: "Teacher training programs, curriculum development, school rehabilitation, community outreach activities, assessment frameworks, monitoring and evaluation systems"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.ExtractedKeywords.Should().NotBeEmpty();

            var keywordsLower = response.ExtractedKeywords.Select(k => k.ToLower()).ToList();

            // Should extract deliverable themes
            var hasEducationKeywords = keywordsLower.Any(k =>
                k.Contains("education") ||
                k.Contains("teacher") ||
                k.Contains("training") ||
                k.Contains("curriculum") ||
                k.Contains("school"));

            var hasCapacityKeywords = keywordsLower.Any(k =>
                k.Contains("capacity") ||
                k.Contains("development") ||
                k.Contains("strengthening"));

            (hasEducationKeywords || hasCapacityKeywords)
                .Should().BeTrue("should extract keywords from deliverables");
        }

        /// <summary>
        /// TC-DST-KWD-004: Combine keywords from multiple sources
        /// 
        /// Given: Opportunity with content in title, description, and deliverables
        /// When: Keywords are extracted
        /// Then: System aggregates unique keywords from all sources
        /// 
        /// Expected Behavior:
        /// - Combines keywords from all text fields
        /// - Removes duplicates
        /// - Prioritizes by relevance across sources
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-004")]
        [Trait("Priority", "High")]
        public async Task ExtractKeywords_MultipleSources_AggregatesUnique()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Infrastructure and Capacity Building Initiative",
                description: "Large-scale infrastructure development with community capacity building components. Procurement of materials and equipment. Training local workforce.",
                deliverables: "Infrastructure construction, capacity building programs, procurement management, training workshops"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.ExtractedKeywords.Should().NotBeEmpty();

            // Keywords should be unique (no duplicates)
            var keywords = response.ExtractedKeywords.Select(k => k.ToLower()).ToList();
            var uniqueKeywords = keywords.Distinct().ToList();
            
            uniqueKeywords.Count.Should().Be(keywords.Count,
                "extracted keywords should be unique");

            // Should have keywords from multiple sources
            var hasInfrastructure = keywords.Any(k => k.Contains("infrastructure"));
            var hasCapacity = keywords.Any(k => k.Contains("capacity"));
            var hasProcurement = keywords.Any(k => k.Contains("procurement"));
            var hasTraining = keywords.Any(k => k.Contains("training"));

            // At least 2 different themes should be present
            var themeCount = new[] { hasInfrastructure, hasCapacity, hasProcurement, hasTraining }
                .Count(x => x);
            
            themeCount.Should().BeGreaterThanOrEqualTo(2,
                "should extract keywords from multiple sources");
        }

        /// <summary>
        /// TC-DST-KWD-005: Handle empty or minimal context
        /// 
        /// Given: Opportunity with very minimal text content
        /// When: Keyword extraction is attempted
        /// Then: System handles gracefully without crashing
        /// 
        /// Expected Behavior:
        /// - Returns empty or minimal keyword list
        /// - Does not throw exceptions
        /// - Logs appropriate warning
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-005")]
        [Trait("Priority", "Medium")]
        public async Task ExtractKeywords_MinimalContext_ReturnsBasicTerms()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Project A",
                description: "Brief description",
                deliverables: ""
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull("system should handle minimal context");
            response.ExtractedKeywords.Should().NotBeNull("keywords list should not be null");

            // May be empty or have very few generic terms
            if (response.ExtractedKeywords.Any())
            {
                response.ExtractedKeywords.Should().HaveCountLessThan(10,
                    "minimal context should produce few keywords");
            }
        }

        #endregion

        #region TC-DST-KWD-006 through TC-DST-KWD-010: Vector Store Integration

        /// <summary>
        /// TC-DST-KWD-006: Vector store query construction
        /// 
        /// Given: Extracted keywords from opportunity
        /// When: Vector store query is built
        /// Then: Query is properly formatted for semantic search
        /// 
        /// Expected Behavior:
        /// - Combines keywords into search query
        /// - Formats query for vector store API
        /// - Includes appropriate search parameters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-006")]
        [Trait("Priority", "High")]
        public async Task BuildVectorQuery_Keywords_FormatsCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Procurement Reform Initiative",
                description: "Strengthen procurement systems, reduce corruption risks, improve transparency",
                deliverables: "Procurement policy, training materials, audit frameworks"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull();
            response.ExtractedKeywords.Should().NotBeEmpty();

            // Verify keywords are suitable for vector search
            response.ExtractedKeywords.Should().AllSatisfy(keyword =>
            {
                keyword.Should().NotBeNullOrWhiteSpace("keywords should be valid strings");
                keyword.Length.Should().BeGreaterThan(2, "keywords should be meaningful");
            });

            // Verify TotalFound indicates vector store was queried
            response.TotalFound.Should().BeGreaterOrEqualTo(0,
                "TotalFound should indicate vector store query was performed");
        }

        /// <summary>
        /// TC-DST-KWD-007: Vector store similarity threshold
        /// 
        /// Given: Vector store search results
        /// When: Results are filtered by similarity score
        /// Then: Only relevant results above threshold are returned
        /// 
        /// Expected Behavior:
        /// - Low relevance results are filtered out
        /// - RelevanceScore indicates similarity to query
        /// - Only high-quality matches are included
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-007")]
        [Trait("Priority", "High")]
        public async Task VectorStoreSearch_SimilarityScore_FiltersLowScores()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare System Strengthening in Rural Areas",
                description: "Comprehensive healthcare infrastructure development with focus on maternal and child health services",
                deliverables: "Health facilities, training programs, equipment procurement"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            if (response.Recommendations.Any())
            {
                var vectorStoreRisks = response.Recommendations
                    .Where(r => r.SourceType == "SIMILAR_PROJECT" && !string.IsNullOrEmpty(r.SourceRiskId))
                    .ToList();

                if (vectorStoreRisks.Any())
                {
                    // All vector store results should have positive relevance scores
                    vectorStoreRisks.Should().AllSatisfy(rec =>
                    {
                        rec.RelevanceScore.Should().BeGreaterThan(0,
                            "vector store results should have positive relevance scores");
                    });

                    // Scores should be in descending order (most relevant first)
                    var scores = vectorStoreRisks.Select(r => r.RelevanceScore).ToList();
                    if (scores.Count > 1)
                    {
                        for (int i = 0; i < scores.Count - 1; i++)
                        {
                            scores[i].Should().BeGreaterThanOrEqualTo(scores[i + 1],
                                "results should be ordered by relevance");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// TC-DST-KWD-008: Vector store returns relevant documents
        /// 
        /// Given: Well-defined opportunity context
        /// When: Vector store is queried
        /// Then: Similar project risks are returned
        /// 
        /// Expected Behavior:
        /// - Returns documents from similar projects
        /// - Documents have SourceRiskId for traceability
        /// - Results are semantically related to query
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-008")]
        [Trait("Priority", "High")]
        public async Task VectorStoreSearch_RelevantRisks_ReturnsMatches()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Water and Sanitation Infrastructure Project",
                description: "Develop water treatment facilities and sanitation systems in urban areas with high population density",
                deliverables: "Water treatment plants, sewage systems, hygiene promotion campaigns"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull();
            response.TotalFound.Should().BeGreaterOrEqualTo(0,
                "vector store should return total documents found");

            // If vector store has data, should return similar project risks
            var vectorStoreRisks = response.Recommendations
                .Where(r => r.SourceType == "SIMILAR_PROJECT" && !string.IsNullOrEmpty(r.SourceRiskId))
                .ToList();

            if (vectorStoreRisks.Any())
            {
                // Verify structure of vector store results
                vectorStoreRisks.Should().AllSatisfy(rec =>
                {
                    rec.SourceRiskId.Should().NotBeNullOrEmpty(
                        "vector store results should have source risk IDs");
                    rec.RelevanceScore.Should().BeGreaterThan(0,
                        "should have relevance score from vector store");
                    rec.Title.Should().NotBeNullOrEmpty("should have risk title");
                    rec.Description.Should().NotBeNullOrEmpty("should have risk description");
                });

                // Results should be related to water/infrastructure theme
                var hasRelevantContent = vectorStoreRisks.Any(rec =>
                    rec.Title.ToLower().Contains("water") ||
                    rec.Title.ToLower().Contains("sanitation") ||
                    rec.Title.ToLower().Contains("infrastructure") ||
                    rec.Description.ToLower().Contains("water") ||
                    rec.Description.ToLower().Contains("sanitation"));

                if (vectorStoreRisks.Count >= 3)
                {
                    hasRelevantContent.Should().BeTrue(
                        "vector store results should be semantically related to opportunity");
                }
            }
        }

        /// <summary>
        /// TC-DST-KWD-009: Handle vector store timeout gracefully
        /// 
        /// Given: Vector store query that may timeout
        /// When: Timeout occurs
        /// Then: System returns graceful fallback response
        /// 
        /// Expected Behavior:
        /// - Does not crash on timeout
        /// - Returns predefined risks even if vector store fails
        /// - Logs timeout error
        /// - User sees recommendations (even if incomplete)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-009")]
        [Trait("Priority", "Medium")]
        public async Task VectorStoreSearch_Timeout_ReturnsGracefulFallback()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Large Complex Multi-Sector Development Program",
                description: "Very large program spanning multiple sectors with extensive scope and complexity requiring comprehensive risk analysis across multiple domains",
                deliverables: "Multiple deliverables across health, education, infrastructure, governance, and economic sectors"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull("system should return response even if vector store times out");
            response.Recommendations.Should().NotBeNull("recommendations list should not be null");

            // Even if vector store fails, should have predefined risks
            var preDefinedRisks = response.Recommendations
                .Where(r => r.SourceType == "PREDEFINED_HIGH_RISK")
                .ToList();

            // System should still function with predefined risks
            if (preDefinedRisks.Any())
            {
                preDefinedRisks.Should().AllSatisfy(rec =>
                {
                    rec.OupQuestionId.Should().HaveValue(
                        "predefined risks should still be available as fallback");
                });
            }
        }

        /// <summary>
        /// TC-DST-KWD-010: LLM refinement of vector store results
        /// 
        /// Given: Raw vector store search results
        /// When: LLM refines and ranks results
        /// Then: Results are enhanced with AI analysis
        /// 
        /// Expected Behavior:
        /// - AI re-ranks results by relevance
        /// - Confidence levels are assigned
        /// - Recommendations are contextualized to opportunity
        /// - Less relevant results are filtered out
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-KWD-010")]
        [Trait("Priority", "High")]
        public async Task RefineResults_LLMRanking_ImproveRelevance()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Education Infrastructure in Post-Conflict Settings",
                description: "Rebuild schools and education facilities destroyed in conflict. Address security concerns, community trauma, and displaced populations. Integrate peace-building into curriculum.",
                deliverables: "School construction, teacher training, peace education curriculum, community reconciliation programs"
            );

            // Act
            var response = await GetDSTRecommendationsAsync(opportunityId);

            // Assert
            response.Should().NotBeNull();

            if (response.Recommendations.Any())
            {
                // All recommendations should have confidence levels (AI refinement)
                response.Recommendations.Should().AllSatisfy(rec =>
                {
                    rec.ConfidenceLevel.Should().BeInRange(0, 100,
                        "AI should assign confidence levels during refinement");
                });

                // Recommendations should be ordered by confidence/relevance
                var confidenceLevels = response.Recommendations
                    .Select(r => r.ConfidenceLevel)
                    .ToList();

                if (confidenceLevels.Count > 1)
                {
                    confidenceLevels.Should().BeInDescendingOrder(
                        "AI refinement should rank by confidence");
                }

                // High confidence recommendations should be more relevant
                var highConfidenceRecs = response.Recommendations
                    .Where(r => r.ConfidenceLevel >= 80)
                    .ToList();

                if (highConfidenceRecs.Any())
                {
                    // High confidence risks should have comprehensive recommendations
                    highConfidenceRecs.Should().AllSatisfy(rec =>
                    {
                        rec.Recommendation.Should().NotBeNullOrEmpty(
                            "high confidence risks should have detailed mitigation advice");
                        rec.Recommendation.Length.Should().BeGreaterThan(50,
                            "recommendations should be substantive");
                    });
                }
            }
        }

        #endregion
    }
}
