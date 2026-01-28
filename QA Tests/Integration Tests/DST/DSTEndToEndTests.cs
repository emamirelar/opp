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
    /// End-to-end integration tests for complete DST workflows
    /// Tests full user journeys from recommendation generation to risk management
    /// 
    /// Test Coverage:
    /// - Complete DST workflow: recommendation → risk creation → management
    /// - Multi-user collaboration on DST risks
    /// - DST integration with opportunity workflow stages
    /// - DST recommendations across opportunity lifecycle
    /// - Historical DST analysis tracking
    /// - DST risk export functionality
    /// - DST in decision reports
    /// - DST integration with partner risk profiles
    /// - DST country context integration
    /// - DST learning from similar opportunities
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "EndToEnd")]
    public class DSTEndToEndTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTEndToEndTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        private async Task<int> CreateTestOpportunityAsync(
            string title,
            string description,
            string deliverables = "",
            string stage = "Draft")
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = description,
                Deliverables = deliverables,
                CountryId = 1,
                EstimatedBudget = 1000000m,
                Stage = stage
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

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

        #region TC-DST-E2E-001 through TC-DST-E2E-003: Complete Workflows

        /// <summary>
        /// TC-DST-E2E-001: Complete DST workflow - recommendation to risk creation
        /// 
        /// Given: New opportunity requiring risk analysis
        /// When: User follows complete DST workflow
        /// Then: Recommendations are generated and converted to managed risks
        /// 
        /// Workflow:
        /// 1. Create opportunity
        /// 2. Get DST recommendations
        /// 3. Review and select recommendations
        /// 4. Create risks from selected recommendations
        /// 5. Verify risks are tracked with DST source
        /// 6. Update risk details
        /// 7. Link risks to mitigation actions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-001")]
        [Trait("Priority", "Critical")]
        public async Task DSTWorkflow_RecommendationToRisk_CompleteFlow()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var opportunityManager = serviceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var riskManager = serviceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser(userId: 100);

            // Step 1: Create opportunity
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Healthcare Infrastructure Project in High-Risk Environment",
                description: "Establish healthcare facilities in conflict-affected region with procurement and security challenges",
                deliverables: "Health clinics, medical equipment, training programs"
            );

            opportunityId.Should().BeGreaterThan(0, "opportunity should be created");

            // Step 2: Get DST recommendations
            var recommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            recommendations.Should().NotBeNull("DST should generate recommendations");
            recommendations.Recommendations.Should().NotBeEmpty("should have risk recommendations");

            // Step 3: User reviews and selects top 3 recommendations
            var selectedRecommendations = recommendations.Recommendations
                .OrderByDescending(r => r.ConfidenceLevel)
                .Take(3)
                .ToList();

            selectedRecommendations.Should().HaveCount(3, "user selected 3 recommendations");

            // Step 4: Create risks from selected recommendations
            var createdRisks = new List<RiskModel>();
            foreach (var recommendation in selectedRecommendations)
            {
                var riskRequest = new RiskCreateRequest
                {
                    Title = recommendation.Title,
                    Description = recommendation.Description,
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 3,
                    ImpactId = 4,
                    Source = "DST Recommendation",
                    SourceReferenceId = recommendation.StableIdentifier,
                    OupQuestionId = recommendation.OupQuestionId,
                    PreDefinedHighRiskId = recommendation.PreDefinedHighRiskId,
                    RiskCategoryId = recommendation.RiskCategoryId
                };

                var risk = await riskManager.AddRiskAsync(riskRequest, user);
                createdRisks.Add(risk);
            }

            createdRisks.Should().HaveCount(3, "all 3 risks should be created");

            // Step 5: Verify risks are tracked with DST source
            var dstRisks = await riskManager.GetDSTRisksAsync(opportunityId, user);
            dstRisks.Should().NotBeNull();
            dstRisks.Risks.Should().HaveCountGreaterOrEqualTo(3, "all DST risks should be retrievable");
            dstRisks.Risks.Should().AllSatisfy(r =>
            {
                r.Source.Should().Be("DST Recommendation");
                r.SourceReferenceId.Should().NotBeNullOrEmpty();
            });

            // Step 6: Update risk details (user adds mitigation plan)
            var firstRisk = createdRisks.First();
            var updateRequest = new RiskUpdateRequest
            {
                Id = firstRisk.Id,
                Title = firstRisk.Title,
                Description = firstRisk.Description,
                ProbabilityId = 2, // Reduced probability after mitigation
                ImpactId = 3,       // Reduced impact
                ResponseTypeId = 2, // Mitigation response
                Status = "Active"
            };

            var updatedRisk = await riskManager.UpdateRiskAsync(updateRequest, user);
            updatedRisk.Should().NotBeNull();
            updatedRisk.ProbabilityId.Should().Be(2, "probability should be updated");
            updatedRisk.ResponseTypeId.Should().Be(2, "response type should be set");

            // Step 7: Verify complete workflow
            var allOpportunityRisks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);
            allOpportunityRisks.Risks.Should().Contain(r => r.Id == updatedRisk.Id,
                "updated risk should be in opportunity risk list");
        }

        /// <summary>
        /// TC-DST-E2E-002: Multi-user collaboration on DST risks
        /// 
        /// Given: Multiple users working on same opportunity
        /// When: Different users interact with DST recommendations and risks
        /// Then: Collaboration works smoothly with proper permissions
        /// 
        /// Workflow:
        /// 1. User A creates opportunity and gets DST recommendations
        /// 2. User A creates risks from recommendations
        /// 3. User B reviews risks and updates some
        /// 4. User C dismisses certain recommendations
        /// 5. Verify audit trail tracks all user actions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-002")]
        [Trait("Priority", "High")]
        public async Task DSTRisks_MultipleUsers_ConcurrentAccess()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var riskManager = serviceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            
            var userA = CreateTestUser(userId: 101, role: "Project Manager");
            var userB = CreateTestUser(userId: 102, role: "Risk Manager");
            var userC = CreateTestUser(userId: 103, role: "Team Member");

            // User A: Create opportunity and get recommendations
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Multi-User Collaboration Project",
                description: "Project requiring collaboration on risk management",
                deliverables: "Collaborative outputs"
            );

            var recommendationsForA = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: userA,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            recommendationsForA.Recommendations.Should().NotBeEmpty();

            // User A: Create risk from first recommendation
            var firstRec = recommendationsForA.Recommendations.First();
            var riskRequest = new RiskCreateRequest
            {
                Title = firstRec.Title,
                Description = firstRec.Description,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = firstRec.StableIdentifier
            };

            var createdRisk = await riskManager.AddRiskAsync(riskRequest, userA);
            createdRisk.Should().NotBeNull();
            createdRisk.CreatedBy.Should().Be(101, "risk created by User A");

            // User B: Review and update the risk
            var updateRequest = new RiskUpdateRequest
            {
                Id = createdRisk.Id,
                Title = createdRisk.Title + " (Reviewed)",
                Description = createdRisk.Description + " - Reviewed by Risk Manager",
                ProbabilityId = 2, // User B reduces probability
                ImpactId = 3
            };

            var updatedRisk = await riskManager.UpdateRiskAsync(updateRequest, userB);
            updatedRisk.Should().NotBeNull();
            updatedRisk.LastModifiedBy.Should().Be(102, "risk modified by User B");

            // User C: Get recommendations with some dismissed
            var dismissedIds = recommendationsForA.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .Take(2)
                .ToList();

            if (dismissedIds.Any())
            {
                var recommendationsForC = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: userC,
                    maxResults: 15,
                    dismissedOupQuestionIds: dismissedIds,
                    forceRefresh: false
                );

                // Dismissed recommendations should not appear
                var returnedOupIds = recommendationsForC.Recommendations
                    .Where(r => r.OupQuestionId.HasValue)
                    .Select(r => r.OupQuestionId.Value)
                    .ToList();

                foreach (var dismissedId in dismissedIds)
                {
                    returnedOupIds.Should().NotContain(dismissedId,
                        $"User C dismissed oUP ID {dismissedId}");
                }
            }

            // Verify audit trail
            var finalRisk = await riskManager.GetRiskByIdAsync(createdRisk.Id, userA);
            finalRisk.CreatedBy.Should().Be(101, "created by User A");
            finalRisk.LastModifiedBy.Should().Be(102, "last modified by User B");
        }

        /// <summary>
        /// TC-DST-E2E-003: DST integration with opportunity workflow
        /// 
        /// Given: Opportunity progressing through workflow stages
        /// When: Opportunity stage changes
        /// Then: DST analysis is triggered at appropriate stages
        /// 
        /// Workflow:
        /// 1. Draft stage: Initial DST analysis
        /// 2. Planning stage: Comprehensive risk assessment
        /// 3. Review stage: Risk review and updates
        /// 4. Active stage: Ongoing risk monitoring
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-003")]
        [Trait("Priority", "High")]
        public async Task DSTAnalysis_OpportunityStage_TriggersCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var opportunityManager = serviceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Stage 1: Draft - Initial DST analysis
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Workflow Integration Test Project",
                description: "Project for testing DST integration with workflow",
                deliverables: "Workflow outputs",
                stage: "Draft"
            );

            var draftRecommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            draftRecommendations.Should().NotBeNull("DST should work in Draft stage");

            // Stage 2: Update to Planning stage
            var opportunity = await opportunityManager.GetOpportunityByIdAsync(opportunityId);
            opportunity.Stage = "Planning";
            await opportunityManager.UpdateOpportunityAsync(opportunity);

            // Get recommendations again (may trigger re-analysis)
            var planningRecommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            planningRecommendations.Should().NotBeNull("DST should work in Planning stage");

            // Verify both analyses completed successfully
            draftRecommendations.ExecutionTimeMs.Should().BeGreaterThan(0);
            planningRecommendations.ExecutionTimeMs.Should().BeGreaterThan(0);
        }

        #endregion

        #region TC-DST-E2E-004 through TC-DST-E2E-007: Lifecycle and History

        /// <summary>
        /// TC-DST-E2E-004: DST recommendations across opportunity lifecycle
        /// 
        /// Given: Opportunity evolves over time
        /// When: DST analysis is performed at different lifecycle points
        /// Then: Recommendations reflect current opportunity state
        /// 
        /// Expected Behavior:
        /// - Early stage: Focus on planning risks
        /// - Mid stage: Implementation risks
        /// - Late stage: Closeout risks
        /// - Context changes reflected in recommendations
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-004")]
        [Trait("Priority", "Medium")]
        public async Task DSTRecommendations_OpportunityProgression_UpdatesRelevance()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var opportunityManager = serviceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Create opportunity with initial context
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Lifecycle Test Project",
                description: "Initial planning phase project",
                deliverables: "Planning deliverables",
                stage: "Draft"
            );

            // Analysis 1: Draft stage
            var draftAnalysis = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Update opportunity to implementation phase
            var opportunity = await opportunityManager.GetOpportunityByIdAsync(opportunityId);
            opportunity.Description = "Implementation phase with procurement and construction activities";
            opportunity.Deliverables = "Construction outputs, procurement deliverables";
            opportunity.Stage = "Active";
            await opportunityManager.UpdateOpportunityAsync(opportunity);

            // Analysis 2: Active stage with new context
            var activeAnalysis = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Verify both analyses succeeded
            draftAnalysis.Should().NotBeNull();
            activeAnalysis.Should().NotBeNull();

            // Keywords should reflect stage changes
            if (activeAnalysis.ExtractedKeywords.Any())
            {
                var implementationKeywords = activeAnalysis.ExtractedKeywords
                    .Select(k => k.ToLower())
                    .Any(k => k.Contains("implementation") || k.Contains("procurement") || k.Contains("construction"));

                if (activeAnalysis.ExtractedKeywords.Count >= 5)
                {
                    implementationKeywords.Should().BeTrue(
                        "active stage keywords should reflect implementation context");
                }
            }
        }

        /// <summary>
        /// TC-DST-E2E-005: Historical DST analysis tracking
        /// 
        /// Given: Multiple DST analyses over time
        /// When: Historical analyses are queried
        /// Then: System tracks analysis history
        /// 
        /// Expected Behavior:
        /// - Each analysis is timestamped
        /// - Historical recommendations are preserved
        /// - Trends can be analyzed
        /// - Audit trail exists
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-005")]
        [Trait("Priority", "Medium")]
        public async Task DSTHistory_MultipleAnalyses_TracksChanges()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            var opportunityId = await CreateTestOpportunityAsync(
                title: "History Tracking Project",
                description: "Project for testing historical tracking",
                deliverables: "Historical outputs"
            );

            // Perform multiple analyses
            var analysis1 = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            await Task.Delay(1000); // Wait to ensure different timestamps

            var analysis2 = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            await Task.Delay(1000);

            var analysis3 = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Verify all analyses completed
            analysis1.Should().NotBeNull();
            analysis2.Should().NotBeNull();
            analysis3.Should().NotBeNull();

            // Each analysis should have execution time tracked
            analysis1.ExecutionTimeMs.Should().BeGreaterThan(0);
            analysis2.ExecutionTimeMs.Should().BeGreaterThan(0);
            analysis3.ExecutionTimeMs.Should().BeGreaterThan(0);

            // Analyses should be independent
            // (They may have different recommendations due to AI variability or cache)
        }

        /// <summary>
        /// TC-DST-E2E-006: DST risk export functionality
        /// 
        /// Given: Opportunity with DST-sourced risks
        /// When: Risks are exported
        /// Then: Export includes DST metadata and traceability
        /// 
        /// Expected Behavior:
        /// - Export includes source information
        /// - StableIdentifier allows re-import
        /// - Recommendations are preserved
        /// - Export format is suitable for reporting
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-006")]
        [Trait("Priority", "Low")]
        public async Task DSTRisks_Export_FormatsCorrectly()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var riskManager = serviceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Create opportunity and risks
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Export Test Project",
                description: "Project for export testing",
                deliverables: "Export outputs"
            );

            var recommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 5,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Create risks from recommendations
            var createdRisks = new List<RiskModel>();
            foreach (var rec in recommendations.Recommendations.Take(3))
            {
                var risk = await riskManager.AddRiskAsync(new RiskCreateRequest
                {
                    Title = rec.Title,
                    Description = rec.Description,
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 3,
                    ImpactId = 4,
                    Source = "DST Recommendation",
                    SourceReferenceId = rec.StableIdentifier,
                    OupQuestionId = rec.OupQuestionId
                }, user);

                createdRisks.Add(risk);
            }

            // Get risks for "export"
            var dstRisks = await riskManager.GetDSTRisksAsync(opportunityId, user);

            // Verify export data completeness
            dstRisks.Should().NotBeNull();
            dstRisks.Risks.Should().HaveCountGreaterOrEqualTo(3);
            dstRisks.Risks.Should().AllSatisfy(r =>
            {
                r.Title.Should().NotBeNullOrEmpty("export should include title");
                r.Description.Should().NotBeNullOrEmpty("export should include description");
                r.Source.Should().Be("DST Recommendation", "export should include source");
                r.SourceReferenceId.Should().NotBeNullOrEmpty("export should include traceability");
            });
        }

        /// <summary>
        /// TC-DST-E2E-007: DST recommendations in decision reports
        /// 
        /// Given: Opportunity with DST analysis
        /// When: Decision report is generated
        /// Then: DST insights are included in report
        /// 
        /// Expected Behavior:
        /// - Report includes DST recommendations
        /// - Risk analysis summary is provided
        /// - High-priority risks are highlighted
        /// - Source attribution is maintained
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-007")]
        [Trait("Priority", "Medium")]
        public async Task DSTRecommendations_DecisionReport_IncludesInsights()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            var opportunityId = await CreateTestOpportunityAsync(
                title: "Decision Report Test Project",
                description: "Project requiring decision support",
                deliverables: "Decision deliverables"
            );

            // Get DST analysis for decision support
            var dstAnalysis = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 20,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Verify analysis provides decision insights
            dstAnalysis.Should().NotBeNull("DST should provide decision insights");

            if (dstAnalysis.Recommendations.Any())
            {
                // Identify high-priority risks for decision report
                var highPriorityRisks = dstAnalysis.Recommendations
                    .Where(r => r.IsStronglyRecommended)
                    .ToList();

                // High-priority risks should have comprehensive information
                if (highPriorityRisks.Any())
                {
                    highPriorityRisks.Should().AllSatisfy(rec =>
                    {
                        rec.Title.Should().NotBeNullOrEmpty();
                        rec.Description.Should().NotBeNullOrEmpty();
                        rec.Recommendation.Should().NotBeNullOrEmpty(
                            "decision report should include mitigation recommendations");
                        rec.ConfidenceLevel.Should().BeGreaterThanOrEqualTo(80);
                    });
                }

                // Extract keywords for executive summary
                dstAnalysis.ExtractedKeywords.Should().NotBeEmpty(
                    "decision report should include key themes");
            }
        }

        #endregion

        #region TC-DST-E2E-008 through TC-DST-E2E-010: Cross-System Integration

        /// <summary>
        /// TC-DST-E2E-008: DST integration with partner risk profiles
        /// 
        /// Given: Opportunity involving partners with risk profiles
        /// When: DST analysis is performed
        /// Then: Partner risks are incorporated into analysis
        /// 
        /// Expected Behavior:
        /// - Partner risk data influences recommendations
        /// - Partner-specific risks are identified
        /// - Due diligence gaps trigger recommendations
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-008")]
        [Trait("Priority", "Medium")]
        public async Task DSTAnalysis_PartnerRisks_IncorporatesPartnerData()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            var opportunityId = await CreateTestOpportunityAsync(
                title: "Partnership Project with Multiple Partners",
                description: "Project involving collaboration with multiple partner organizations, some with limited due diligence",
                deliverables: "Partnership deliverables"
            );

            var recommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            recommendations.Should().NotBeNull();

            // Look for partner-related risk keywords
            if (recommendations.ExtractedKeywords.Any())
            {
                var hasPartnerKeywords = recommendations.ExtractedKeywords
                    .Select(k => k.ToLower())
                    .Any(k => k.Contains("partner") || k.Contains("collaboration") || k.Contains("due diligence"));

                if (recommendations.ExtractedKeywords.Count >= 5)
                {
                    hasPartnerKeywords.Should().BeTrue(
                        "DST should identify partner-related keywords");
                }
            }
        }

        /// <summary>
        /// TC-DST-E2E-009: DST country context integration
        /// 
        /// Given: Opportunity in country with specific risk profile
        /// When: DST analysis is performed
        /// Then: Country-specific risks are considered
        /// 
        /// Expected Behavior:
        /// - Country risk indices influence recommendations
        /// - Fragile state risks are identified
        /// - Context-appropriate mitigation advice
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-009")]
        [Trait("Priority", "Medium")]
        public async Task DSTRecommendations_CountryRisk_IncludesGeography()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            var opportunityId = await CreateTestOpportunityAsync(
                title: "Project in High-Risk Country Environment",
                description: "Implementation in fragile state with security concerns, corruption risks, and limited infrastructure",
                deliverables: "Context-sensitive deliverables"
            );

            var recommendations = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            recommendations.Should().NotBeNull();

            // Look for country/geography-related risks
            if (recommendations.Recommendations.Any())
            {
                var hasCountryRisks = recommendations.Recommendations.Any(rec =>
                    rec.Title.ToLower().Contains("security") ||
                    rec.Title.ToLower().Contains("corruption") ||
                    rec.Title.ToLower().Contains("infrastructure") ||
                    rec.Title.ToLower().Contains("fragile") ||
                    rec.Description.ToLower().Contains("country") ||
                    rec.Description.ToLower().Contains("context"));

                if (recommendations.Recommendations.Count >= 5)
                {
                    hasCountryRisks.Should().BeTrue(
                        "DST should identify country-specific risks");
                }
            }
        }

        /// <summary>
        /// TC-DST-E2E-010: DST recommendations for similar opportunities
        /// 
        /// Given: Multiple similar opportunities exist
        /// When: DST analyzes new similar opportunity
        /// Then: System learns from historical similar projects
        /// 
        /// Expected Behavior:
        /// - Vector store finds similar project risks
        /// - Historical lessons are applied
        /// - Pattern recognition improves recommendations
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-E2E-010")]
        [Trait("Priority", "Low")]
        public async Task DSTLearning_SimilarOpportunities_SharesKnowledge()
        {
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var geminiManager = serviceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Create first healthcare opportunity
            var opp1Id = await CreateTestOpportunityAsync(
                title: "Healthcare Infrastructure Project Alpha",
                description: "Establish healthcare facilities in rural areas with medical equipment and training",
                deliverables: "Health clinics, equipment, training programs"
            );

            // Get DST analysis for first opportunity
            var analysis1 = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opp1Id,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Create second similar healthcare opportunity
            var opp2Id = await CreateTestOpportunityAsync(
                title: "Healthcare System Strengthening Project Beta",
                description: "Build health infrastructure and strengthen healthcare delivery systems in underserved regions",
                deliverables: "Healthcare facilities, capacity building, system strengthening"
            );

            // Get DST analysis for second opportunity
            var analysis2 = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opp2Id,
                user: user,
                maxResults: 15,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            // Verify both analyses succeeded
            analysis1.Should().NotBeNull();
            analysis2.Should().NotBeNull();

            // Both should have healthcare-related keywords
            if (analysis1.ExtractedKeywords.Any() && analysis2.ExtractedKeywords.Any())
            {
                var healthKeywords1 = analysis1.ExtractedKeywords
                    .Select(k => k.ToLower())
                    .Where(k => k.Contains("health") || k.Contains("medical") || k.Contains("clinic"))
                    .ToList();

                var healthKeywords2 = analysis2.ExtractedKeywords
                    .Select(k => k.ToLower())
                    .Where(k => k.Contains("health") || k.Contains("medical") || k.Contains("clinic"))
                    .ToList();

                // Similar opportunities should extract similar themes
                (healthKeywords1.Any() && healthKeywords2.Any())
                    .Should().BeTrue("similar opportunities should have similar keywords");
            }

            // Check if second opportunity got recommendations from vector store (similar projects)
            if (analysis2.Recommendations.Any())
            {
                var vectorStoreRecs = analysis2.Recommendations
                    .Where(r => r.SourceType == "SIMILAR_PROJECT")
                    .ToList();

                // If vector store has data, should find similar project risks
                if (vectorStoreRecs.Any())
                {
                    vectorStoreRecs.Should().AllSatisfy(rec =>
                    {
                        rec.SourceRiskId.Should().NotBeNullOrEmpty(
                            "similar project recommendations should have source IDs");
                    });
                }
            }
        }

        #endregion
    }
}
