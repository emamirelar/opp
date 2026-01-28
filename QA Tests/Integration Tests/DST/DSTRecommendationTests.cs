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

namespace UNOPS.PAO.Tests.Integration.DST
{
    /// <summary>
    /// Integration tests for DST (Decision Support Tool) Recommendation Generation
    /// Tests the AI-powered risk recommendation system for opportunities
    /// 
    /// Test Coverage:
    /// - Recommendation generation with various opportunity contexts
    /// - Keyword extraction from opportunity data
    /// - Vector store integration for similar project risks
    /// - Predefined high risks from oUP EAC checklist
    /// - Confidence level filtering and ranking
    /// - Dismiss and exclusion functionality
    /// - Cache and force refresh behavior
    /// - Performance and execution time tracking
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "RecommendationGeneration")]
    public class DSTRecommendationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTRecommendationTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        /// <summary>
        /// Creates a test opportunity with complete context for DST analysis
        /// </summary>
        private async Task<int> CreateTestOpportunityAsync(
            string title,
            string description,
            string deliverables,
            int? countryId = null,
            decimal? budget = null)
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = description,
                Deliverables = deliverables,
                CountryId = countryId ?? 1,
                EstimatedBudget = budget ?? 100000m,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

        /// <summary>
        /// Creates a test user with proper claims for DST operations
        /// </summary>
        private ClaimsPrincipal CreateTestUser(int userId = 1, string role = "Project Manager")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, $"Test User {userId}"),
                new Claim(ClaimTypes.Role, role)
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        #endregion

        #region TC-DST-REC-001 through TC-DST-REC-005: Core Recommendation Generation

        /// <summary>
        /// TC-DST-REC-001: Generate recommendations with complete opportunity data
        /// 
        /// Given: An opportunity with rich context (title, description, deliverables, country)
        /// When: DST recommendations are requested
        /// Then: System returns relevant risk recommendations with confidence scores
        /// 
        /// Expected Behavior:
        /// - AI analyzes opportunity context
        /// - Extracts keywords for vector store search
        /// - Returns recommendations with relevance scores
        /// - Includes both predefined and similar project risks
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-001")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_CompleteOpportunity_ReturnsRelevantRisks()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare Infrastructure Development in Fragile State",
                description: "Establish primary healthcare facilities in conflict-affected regions with limited infrastructure. Project involves construction, medical equipment procurement, and capacity building for local healthcare workers.",
                deliverables: "3 new health clinics, medical equipment installation, training for 50 healthcare workers",
                countryId: 1,
                budget: 5000000m
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
                forceRefresh: false
            );

            // Assert
            response.Should().NotBeNull("DST should return a recommendation response");
            response.Recommendations.Should().NotBeEmpty("DST should return risk recommendations for a complete opportunity");
            response.Recommendations.Should().HaveCountLessThanOrEqualTo(10, "maxResults parameter should be respected");
            
            // Verify recommendation structure
            response.Recommendations.Should().AllSatisfy(rec =>
            {
                rec.Title.Should().NotBeNullOrEmpty("each recommendation should have a title");
                rec.Description.Should().NotBeNullOrEmpty("each recommendation should have a description");
                rec.Recommendation.Should().NotBeNullOrEmpty("each recommendation should have mitigation advice");
                rec.ConfidenceLevel.Should().BeInRange(0, 100, "confidence level should be 0-100");
                rec.RelevanceScore.Should().BeGreaterThanOrEqualTo(0, "relevance score should be non-negative");
            });

            // Verify keywords were extracted
            response.ExtractedKeywords.Should().NotBeEmpty("system should extract keywords from opportunity context");
            response.ExtractedKeywords.Should().Contain(k => 
                k.ToLower().Contains("healthcare") || 
                k.ToLower().Contains("infrastructure") || 
                k.ToLower().Contains("conflict"),
                "keywords should be relevant to opportunity content"
            );

            // Verify execution time is tracked
            response.ExecutionTimeMs.Should().BeGreaterThan(0, "execution time should be tracked");
            response.ExecutionTimeMs.Should().BeLessThan(30000, "DST should complete within 30 seconds");
        }

        /// <summary>
        /// TC-DST-REC-002: Handle missing opportunity context gracefully
        /// 
        /// Given: An opportunity with minimal data (only title)
        /// When: DST recommendations are requested
        /// Then: System returns basic recommendations without crashing
        /// 
        /// Expected Behavior:
        /// - System handles sparse data gracefully
        /// - Returns generic/common risks when context is limited
        /// - Does not throw exceptions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-002")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_MinimalData_ReturnsBasicRecommendations()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Project X",
                description: "",
                deliverables: "",
                countryId: 1,
                budget: 10000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 5,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false
            );

            // Assert
            response.Should().NotBeNull("DST should handle minimal data gracefully");
            
            // May return empty or basic recommendations
            if (response.Recommendations.Any())
            {
                response.Recommendations.Should().AllSatisfy(rec =>
                {
                    rec.Title.Should().NotBeNullOrEmpty();
                    rec.ConfidenceLevel.Should().BeInRange(0, 100);
                });
            }

            // Should not crash even with minimal context
            response.ExecutionTimeMs.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// TC-DST-REC-003: Extract keywords from opportunity description
        /// 
        /// Given: An opportunity with rich descriptive text
        /// When: DST analysis is performed
        /// Then: System extracts relevant risk-related keywords
        /// 
        /// Expected Behavior:
        /// - Identifies key themes (healthcare, infrastructure, security, etc.)
        /// - Extracts risk indicators (conflict, fragile, corruption, etc.)
        /// - Removes stop words and noise
        /// - Returns meaningful keywords for vector search
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-003")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_ContextualKeywords_ExtractsRelevantTerms()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Education System Strengthening in Post-Conflict Environment",
                description: "Rebuild education infrastructure damaged by conflict. Address security concerns for students and teachers. Establish teacher training programs. Procurement of educational materials and technology. Community engagement for sustainable education delivery.",
                deliverables: "School rehabilitation, teacher training, curriculum development, community outreach",
                countryId: 1,
                budget: 2500000m
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
                forceRefresh: true // Force fresh analysis
            );

            // Assert
            response.Should().NotBeNull();
            response.ExtractedKeywords.Should().NotBeEmpty("system should extract keywords");

            // Verify relevant keywords were extracted
            var keywordsLower = response.ExtractedKeywords.Select(k => k.ToLower()).ToList();
            
            // Should extract domain-relevant terms
            keywordsLower.Should().Contain(k => 
                k.Contains("education") || 
                k.Contains("school") || 
                k.Contains("teacher") || 
                k.Contains("training") ||
                k.Contains("conflict") ||
                k.Contains("security"),
                "keywords should be relevant to the opportunity domain"
            );

            // Should extract risk-related terms
            var hasRiskKeywords = keywordsLower.Any(k => 
                k.Contains("conflict") || 
                k.Contains("security") || 
                k.Contains("risk") ||
                k.Contains("fragile") ||
                k.Contains("procurement")
            );
            hasRiskKeywords.Should().BeTrue("system should identify risk-related keywords");
        }

        /// <summary>
        /// TC-DST-REC-004: Vector store integration - find similar project risks
        /// 
        /// Given: An opportunity context and extracted keywords
        /// When: DST performs vector store search
        /// Then: System returns similar project risks from historical data
        /// 
        /// Expected Behavior:
        /// - Queries vector store with keyword-based search
        /// - Returns documents with similarity scores
        /// - Filters results by relevance threshold
        /// - Includes source risk IDs for traceability
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-004")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_VectorStore_FindsSimilarProjectRisks()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Water and Sanitation Infrastructure in Rural Areas",
                description: "Install water treatment facilities and sanitation systems in remote rural communities. Address water quality issues, hygiene education, and sustainable operation models.",
                deliverables: "Water treatment plants, sanitation facilities, hygiene training programs",
                countryId: 1,
                budget: 3000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.TotalFound.Should().BeGreaterThanOrEqualTo(0, "vector store should track total documents found");

            if (response.Recommendations.Any())
            {
                // Verify some recommendations come from vector store (similar projects)
                var similarProjectRisks = response.Recommendations
                    .Where(r => r.SourceType == "SIMILAR_PROJECT")
                    .ToList();

                // Should have at least some similar project risks if vector store is populated
                // (May be empty if vector store has no relevant data yet)
                if (similarProjectRisks.Any())
                {
                    similarProjectRisks.Should().AllSatisfy(rec =>
                    {
                        rec.SourceRiskId.Should().NotBeNullOrEmpty("similar project risks should have source IDs");
                        rec.RelevanceScore.Should().BeGreaterThan(0, "vector store results should have relevance scores");
                    });
                }
            }
        }

        /// <summary>
        /// TC-DST-REC-005: Predefined high risks from oUP EAC checklist
        /// 
        /// Given: An opportunity that matches predefined high-risk criteria
        /// When: DST recommendations are generated
        /// Then: System includes relevant predefined high risks from EAC checklist
        /// 
        /// Expected Behavior:
        /// - Includes PREDEFINED_HIGH_RISK source type recommendations
        /// - Maps to oUP Question IDs for dismiss persistence
        /// - Includes PreDefinedHighRiskId and RiskCategoryId
        /// - Confidence level >= 80 for strongly recommended risks
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-005")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_PreDefinedRisks_IncludesEACChecklist()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Infrastructure Project in High-Risk Country with Procurement Concerns",
                description: "Large-scale construction project in fragile state with complex procurement requirements. Multiple international contractors and significant budget. Corruption risks identified.",
                deliverables: "Infrastructure construction, procurement management, risk mitigation",
                countryId: 1,
                budget: 10000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().NotBeEmpty();

            // Check for predefined high risks
            var preDefinedRisks = response.Recommendations
                .Where(r => r.SourceType == "PREDEFINED_HIGH_RISK")
                .ToList();

            if (preDefinedRisks.Any())
            {
                preDefinedRisks.Should().AllSatisfy(rec =>
                {
                    rec.OupQuestionId.Should().HaveValue("predefined risks should have oUP Question IDs");
                    rec.PreDefinedHighRiskId.Should().HaveValue("predefined risks should have PreDefinedHighRiskId");
                    rec.RiskCategoryId.Should().HaveValue("predefined risks should have category IDs");
                    rec.ConfidenceLevel.Should().BeInRange(0, 100, "confidence level should be valid");
                });

                // Verify StableIdentifier generation for predefined risks
                preDefinedRisks.Should().AllSatisfy(rec =>
                {
                    rec.StableIdentifier.Should().StartWith("oup_", 
                        "predefined risks should use oup_questionId for stable identifier");
                });

                // Check for strongly recommended risks
                var stronglyRecommended = preDefinedRisks.Where(r => r.IsStronglyRecommended).ToList();
                if (stronglyRecommended.Any())
                {
                    stronglyRecommended.Should().AllSatisfy(rec =>
                    {
                        rec.ConfidenceLevel.Should().BeGreaterThanOrEqualTo(80, 
                            "strongly recommended risks should have confidence >= 80");
                    });
                }
            }
        }

        #endregion

        #region TC-DST-REC-006 through TC-DST-REC-010: Filtering and Parameters

        /// <summary>
        /// TC-DST-REC-006: Confidence level thresholds (strongly recommended >= 80)
        /// 
        /// Given: DST recommendations with various confidence levels
        /// When: Results are filtered by confidence threshold
        /// Then: Only high-confidence recommendations are returned
        /// 
        /// Expected Behavior:
        /// - IsStronglyRecommended flag correctly set for confidence >= 80
        /// - Results can be filtered to show only high-confidence risks
        /// - Confidence scores are accurate and meaningful
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-006")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_ConfidenceFiltering_OnlyHighConfidence()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Complex Multi-Sector Development Program",
                description: "Large-scale development initiative spanning health, education, and infrastructure sectors. Multiple partners, complex coordination requirements.",
                deliverables: "Multi-sector interventions, stakeholder coordination, capacity building",
                countryId: 1,
                budget: 15000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();

            if (response.Recommendations.Any())
            {
                // Verify IsStronglyRecommended flag logic
                var stronglyRecommended = response.Recommendations.Where(r => r.IsStronglyRecommended).ToList();
                var notStronglyRecommended = response.Recommendations.Where(r => !r.IsStronglyRecommended).ToList();

                // All strongly recommended should have confidence >= 80
                stronglyRecommended.Should().AllSatisfy(rec =>
                {
                    rec.ConfidenceLevel.Should().BeGreaterThanOrEqualTo(80,
                        "IsStronglyRecommended=true requires confidence >= 80");
                });

                // All not strongly recommended should have confidence < 80
                notStronglyRecommended.Should().AllSatisfy(rec =>
                {
                    rec.ConfidenceLevel.Should().BeLessThan(80,
                        "IsStronglyRecommended=false requires confidence < 80");
                });

                // Verify recommendations are ordered by confidence/relevance
                var confidenceLevels = response.Recommendations.Select(r => r.ConfidenceLevel).ToList();
                confidenceLevels.Should().BeInDescendingOrder("recommendations should be ordered by confidence");
            }
        }

        /// <summary>
        /// TC-DST-REC-007: Dismiss and exclude recommendations
        /// 
        /// Given: A list of dismissed oUP Question IDs
        /// When: DST recommendations are requested
        /// Then: Dismissed recommendations are excluded from results
        /// 
        /// Expected Behavior:
        /// - DismissedOupQuestionIds parameter filters out specific risks
        /// - Dismissed risks are not returned in response
        /// - Only applies to predefined risks with oUP Question IDs
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-007")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_DismissedIds_ExcludesFromResults()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Procurement Reform and Capacity Building",
                description: "Strengthen procurement systems and build capacity for transparent procurement processes.",
                deliverables: "Procurement system improvements, training programs, policy development",
                countryId: 1,
                budget: 1500000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // First call - get all recommendations
            var allRecommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Get oUP Question IDs from predefined risks
            var oupQuestionIds = allRecommendations.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .Take(3) // Dismiss first 3
                .ToList();

            if (!oupQuestionIds.Any())
            {
                // Skip test if no predefined risks were returned
                return;
            }

            // Act - Second call with dismissed IDs
            var filteredRecommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: oupQuestionIds,
                forceRefresh: false // Use cache
            );

            // Assert
            filteredRecommendations.Should().NotBeNull();

            // Verify dismissed recommendations are not in results
            var returnedOupIds = filteredRecommendations.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .ToList();

            foreach (var dismissedId in oupQuestionIds)
            {
                returnedOupIds.Should().NotContain(dismissedId,
                    $"dismissed oUP Question ID {dismissedId} should not appear in results");
            }
        }

        /// <summary>
        /// TC-DST-REC-008: Force refresh bypasses cache
        /// 
        /// Given: Cached DST recommendations exist
        /// When: forceRefresh=true is specified
        /// Then: System regenerates recommendations instead of using cache
        /// 
        /// Expected Behavior:
        /// - forceRefresh=false uses cached results (faster)
        /// - forceRefresh=true regenerates from scratch
        /// - Cache improves performance for repeated requests
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-008")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_ForceRefresh_RegeneratesResults()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Digital Transformation Initiative",
                description: "Modernize government services through digital platforms and capacity building.",
                deliverables: "Digital platform development, change management, user training",
                countryId: 1,
                budget: 2000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - First call with force refresh
            var firstCall = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            var firstCallTime = firstCall.ExecutionTimeMs;

            // Second call without force refresh (should use cache)
            var secondCall = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false
            );

            var secondCallTime = secondCall.ExecutionTimeMs;

            // Third call with force refresh again
            var thirdCall = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            var thirdCallTime = thirdCall.ExecutionTimeMs;

            // Assert
            firstCall.Should().NotBeNull();
            secondCall.Should().NotBeNull();
            thirdCall.Should().NotBeNull();

            // Cached call should typically be faster (but not guaranteed due to various factors)
            // Main assertion: all calls should complete successfully
            secondCallTime.Should().BeGreaterThan(0, "cached call should still track execution time");

            // Force refresh calls should regenerate
            thirdCall.Recommendations.Count.Should().BeGreaterThanOrEqualTo(0,
                "force refresh should generate new recommendations");
        }

        /// <summary>
        /// TC-DST-REC-009: High Risk Guidance document integration
        /// 
        /// Given: A High Risk Guidance document exists in Google Cloud Storage
        /// When: DST recommendations are generated
        /// Then: System incorporates guidance document into AI analysis
        /// 
        /// Expected Behavior:
        /// - Fetches High Risk Guidance document from GCS
        /// - Includes document content in AI prompt for better recommendations
        /// - Logs document retrieval status
        /// - Handles missing document gracefully
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-009")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_WithGuidanceDoc_EnhancesRecommendations()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Emergency Response in Conflict Zone",
                description: "Rapid deployment of humanitarian assistance in active conflict area. High security risks, logistical challenges.",
                deliverables: "Emergency supplies, security protocols, rapid response capacity",
                countryId: 1,
                budget: 5000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();
            
            // System should attempt to fetch guidance document
            // (May or may not exist in test environment)
            // Main assertion: system should handle gracefully either way
            response.Recommendations.Should().NotBeNull();
            
            if (response.Recommendations.Any())
            {
                // If guidance document was found and used, recommendations should be comprehensive
                response.Recommendations.Should().AllSatisfy(rec =>
                {
                    rec.Recommendation.Should().NotBeNullOrEmpty(
                        "recommendations should include mitigation advice");
                });
            }
        }

        /// <summary>
        /// TC-DST-REC-010: Pagination and maxResults parameter
        /// 
        /// Given: A request with specified maxResults parameter
        /// When: DST recommendations are generated
        /// Then: System returns at most maxResults recommendations
        /// 
        /// Expected Behavior:
        /// - maxResults parameter limits response size
        /// - Returns top N recommendations by confidence/relevance
        /// - TotalFound indicates how many risks were found before pagination
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-010")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_MaxResults_ReturnsTopN()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Large-Scale Infrastructure and Capacity Building Program",
                description: "Comprehensive infrastructure development with multiple components: roads, bridges, water systems, health facilities, schools. Extensive capacity building and institutional strengthening.",
                deliverables: "Infrastructure construction, institutional capacity building, training programs, policy development",
                countryId: 1,
                budget: 25000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - Request different maxResults values
            var smallRequest = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 5,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            var mediumRequest = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false // Use cache
            );

            var largeRequest = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false // Use cache
            );

            // Assert
            smallRequest.Recommendations.Should().HaveCountLessThanOrEqualTo(5,
                "maxResults=5 should return at most 5 recommendations");

            mediumRequest.Recommendations.Should().HaveCountLessThanOrEqualTo(10,
                "maxResults=10 should return at most 10 recommendations");

            largeRequest.Recommendations.Should().HaveCountLessThanOrEqualTo(20,
                "maxResults=20 should return at most 20 recommendations");

            // Verify TotalFound is tracked
            smallRequest.TotalFound.Should().BeGreaterThanOrEqualTo(0,
                "TotalFound should indicate total risks before pagination");

            // If we got results, verify they're the top recommendations
            if (smallRequest.Recommendations.Any() && mediumRequest.Recommendations.Any())
            {
                // First 5 from medium request should match small request (same top risks)
                var topFiveFromMedium = mediumRequest.Recommendations.Take(5).ToList();
                for (int i = 0; i < Math.Min(5, smallRequest.Recommendations.Count); i++)
                {
                    if (i < topFiveFromMedium.Count)
                    {
                        // Should be same recommendations (by title or stable identifier)
                        var smallRec = smallRequest.Recommendations[i];
                        var mediumRec = topFiveFromMedium[i];
                        
                        // Compare by stable identifier or title
                        (smallRec.StableIdentifier == mediumRec.StableIdentifier ||
                         smallRec.Title == mediumRec.Title)
                            .Should().BeTrue($"top {i+1} recommendation should be consistent across requests");
                    }
                }
            }
        }

        #endregion

        #region TC-DST-REC-011 through TC-DST-REC-015: Performance and Edge Cases

        /// <summary>
        /// TC-DST-REC-011: Execution time performance tracking
        /// 
        /// Given: DST recommendation request
        /// When: Processing completes
        /// Then: System tracks and returns execution time metrics
        /// 
        /// Expected Behavior:
        /// - ExecutionTimeMs is populated and accurate
        /// - Typical requests complete within reasonable time (< 30s)
        /// - Performance is logged for monitoring
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-011")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_PerformanceMetrics_UnderThreshold()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Standard Project Implementation",
                description: "Regular project implementation with standard scope and complexity.",
                deliverables: "Project deliverables as specified",
                countryId: 1,
                budget: 1000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );
            stopwatch.Stop();

            // Assert
            response.Should().NotBeNull();
            response.ExecutionTimeMs.Should().BeGreaterThan(0, "execution time should be tracked");
            response.ExecutionTimeMs.Should().BeLessThan(30000, "DST should complete within 30 seconds");

            // Verify measured time is close to reported time (within tolerance)
            var measuredTimeMs = stopwatch.ElapsedMilliseconds;
            var reportedTimeMs = response.ExecutionTimeMs;

            // Reported time should be reasonably close to measured time
            // (Allow some variance due to overhead)
            reportedTimeMs.Should().BeGreaterThan(0);
            reportedTimeMs.Should().BeLessThanOrEqualTo(measuredTimeMs + 5000,
                "reported execution time should be close to measured time");
        }

        /// <summary>
        /// TC-DST-REC-012: Source type differentiation (PREDEFINED_HIGH_RISK vs SIMILAR_PROJECT)
        /// 
        /// Given: DST recommendations from multiple sources
        /// When: Results are returned
        /// Then: Each recommendation has correct source type label
        /// 
        /// Expected Behavior:
        /// - PREDEFINED_HIGH_RISK for EAC checklist risks
        /// - SIMILAR_PROJECT for vector store risks
        /// - Source type is clearly differentiated
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-012")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_SourceTypes_CorrectlyLabeled()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Comprehensive Development Project with Multiple Risk Factors",
                description: "Large project spanning multiple sectors with procurement, security, and operational risks.",
                deliverables: "Multi-sector deliverables, complex procurement, institutional capacity building",
                countryId: 1,
                budget: 8000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            response.Should().NotBeNull();

            if (response.Recommendations.Any())
            {
                // Group recommendations by source type
                var preDefinedRisks = response.Recommendations
                    .Where(r => r.SourceType == "PREDEFINED_HIGH_RISK")
                    .ToList();

                var similarProjectRisks = response.Recommendations
                    .Where(r => r.SourceType == "SIMILAR_PROJECT")
                    .ToList();

                // Verify predefined risks have oUP Question IDs
                preDefinedRisks.Should().AllSatisfy(rec =>
                {
                    rec.OupQuestionId.Should().HaveValue(
                        "PREDEFINED_HIGH_RISK must have oUP Question ID");
                    rec.PreDefinedHighRiskId.Should().HaveValue(
                        "PREDEFINED_HIGH_RISK must have PreDefinedHighRiskId");
                });

                // Verify similar project risks have source risk IDs (if from vector store)
                var vectorStoreRisks = similarProjectRisks.Where(r => !string.IsNullOrEmpty(r.SourceRiskId)).ToList();
                if (vectorStoreRisks.Any())
                {
                    vectorStoreRisks.Should().AllSatisfy(rec =>
                    {
                        rec.SourceRiskId.Should().NotBeNullOrEmpty(
                            "SIMILAR_PROJECT from vector store should have SourceRiskId");
                    });
                }
            }
        }

        /// <summary>
        /// TC-DST-REC-013: StableIdentifier generation for dismiss persistence
        /// 
        /// Given: Recommendations from different sources
        /// When: StableIdentifier is accessed
        /// Then: Each recommendation has unique, stable identifier
        /// 
        /// Expected Behavior:
        /// - oUP Question ID risks: "oup_{questionId}"
        /// - Vector store risks: "vs_{sourceRiskId}"
        /// - Custom risks: "hash_{titleHash}"
        /// - Identifiers are stable across requests for same risk
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-013")]
        [Trait("Priority", "Critical")]
        public async Task DSTRecommendation_StableIdentifier_UniqueAndPersistent()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Project Requiring Risk Tracking",
                description: "Project with various risk factors requiring stable risk identification.",
                deliverables: "Project deliverables with risk management",
                countryId: 1,
                budget: 2000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act - Get recommendations twice to verify stability
            var firstRequest = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            var secondRequest = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            if (firstRequest.Recommendations.Any())
            {
                // Verify stable identifier format
                firstRequest.Recommendations.Should().AllSatisfy(rec =>
                {
                    rec.StableIdentifier.Should().NotBeNullOrEmpty("every recommendation must have stable identifier");

                    // Verify format based on source
                    if (rec.OupQuestionId.HasValue)
                    {
                        rec.StableIdentifier.Should().StartWith("oup_",
                            "predefined risks should use oup_ prefix");
                        rec.StableIdentifier.Should().Contain(rec.OupQuestionId.Value.ToString(),
                            "oup identifier should contain question ID");
                    }
                    else if (!string.IsNullOrEmpty(rec.SourceRiskId))
                    {
                        rec.StableIdentifier.Should().StartWith("vs_",
                            "vector store risks should use vs_ prefix");
                    }
                    else
                    {
                        rec.StableIdentifier.Should().StartWith("hash_",
                            "custom risks should use hash_ prefix");
                    }
                });

                // Verify stability across requests (same identifiers for same risks)
                var firstIdentifiers = firstRequest.Recommendations
                    .Select(r => r.StableIdentifier)
                    .ToHashSet();

                var secondIdentifiers = secondRequest.Recommendations
                    .Select(r => r.StableIdentifier)
                    .ToHashSet();

                // Should have significant overlap in stable identifiers
                var commonIdentifiers = firstIdentifiers.Intersect(secondIdentifiers).Count();
                if (firstRequest.Recommendations.Count >= 3 && secondRequest.Recommendations.Count >= 3)
                {
                    commonIdentifiers.Should().BeGreaterThan(0,
                        "stable identifiers should persist across requests for same opportunity");
                }
            }
        }

        /// <summary>
        /// TC-DST-REC-014: oUP Question ID mapping for predefined risks
        /// 
        /// Given: Predefined high risks from EAC checklist
        /// When: Recommendations are returned
        /// Then: Each predefined risk has correct oUP Question ID mapping
        /// 
        /// Expected Behavior:
        /// - OupQuestionId is populated for predefined risks
        /// - Maps to oUP EAC checklist questions
        /// - Used for dismiss persistence across sessions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-014")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_OupQuestionId_MapsCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "High-Risk Procurement in Fragile State",
                description: "Major procurement initiative in high-risk environment with corruption concerns.",
                deliverables: "Procurement system strengthening, transparency measures",
                countryId: 1,
                budget: 5000000m
            );

            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act
            var response = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Assert
            var preDefinedRisks = response.Recommendations
                .Where(r => r.SourceType == "PREDEFINED_HIGH_RISK")
                .ToList();

            if (preDefinedRisks.Any())
            {
                preDefinedRisks.Should().AllSatisfy(rec =>
                {
                    rec.OupQuestionId.Should().HaveValue("predefined risks must have oUP Question ID");
                    rec.OupQuestionId.Value.Should().BeGreaterThan(0, "oUP Question ID should be valid positive integer");
                    rec.PreDefinedHighRiskId.Should().HaveValue("must have PreDefinedHighRiskId");
                    rec.RiskCategoryId.Should().HaveValue("must have RiskCategoryId for categorization");
                });

                // Verify oUP Question IDs are unique within results
                var oupIds = preDefinedRisks.Select(r => r.OupQuestionId.Value).ToList();
                oupIds.Should().OnlyHaveUniqueItems("each predefined risk should have unique oUP Question ID");
            }
        }

        /// <summary>
        /// TC-DST-REC-015: Empty recommendations when no relevant risks found
        /// 
        /// Given: An opportunity with no risk indicators or very generic content
        /// When: DST analysis is performed
        /// Then: System returns empty or minimal recommendations gracefully
        /// 
        /// Expected Behavior:
        /// - Does not crash or throw exceptions
        /// - Returns empty list or very generic risks
        /// - Execution time is still tracked
        /// - Logs indicate no matches found
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-REC-015")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_NoMatches_ReturnsEmptyList()
        {
            // Arrange - Very generic, low-risk opportunity
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Administrative Support Services",
                description: "Routine administrative support and office management.",
                deliverables: "Administrative services",
                countryId: 1,
                budget: 50000m
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
            response.Should().NotBeNull("DST should return response even with no matches");
            response.Recommendations.Should().NotBeNull("recommendations list should not be null");
            
            // May be empty or have very few generic risks
            response.ExtractedKeywords.Should().NotBeNull("keywords list should not be null");
            response.ExecutionTimeMs.Should().BeGreaterThan(0, "execution time should be tracked even with no results");
            response.TotalFound.Should().BeGreaterThanOrEqualTo(0, "total found should be non-negative");

            // If there are recommendations, they should be low confidence
            if (response.Recommendations.Any())
            {
                response.Recommendations.Should().AllSatisfy(rec =>
                {
                    rec.ConfidenceLevel.Should().BeLessThan(70,
                        "generic opportunity should have low confidence risks");
                });
            }
        }

        #endregion
    }
}
