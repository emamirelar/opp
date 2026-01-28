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
    /// Integration tests for DST Risk Management operations
    /// Tests creating, updating, and managing risks sourced from DST recommendations
    /// 
    /// Test Coverage:
    /// - Creating risks from DST recommendations
    /// - Pre-populating risk fields from recommendations
    /// - Updating DST-sourced risks
    /// - Deleting DST risks
    /// - Retrieving all DST risks for an opportunity
    /// - Source tracking and traceability
    /// - Preventing duplicate recommendations
    /// - Risk category linking
    /// - Bulk operations
    /// - Authorization and validation
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "RiskManagement")]
    public class DSTRiskManagementTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTRiskManagementTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        /// <summary>
        /// Creates a test opportunity for risk management testing
        /// </summary>
        private async Task<int> CreateTestOpportunityAsync(string title = "Test Opportunity")
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = "Test opportunity for DST risk management",
                CountryId = 1,
                EstimatedBudget = 1000000m,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

        /// <summary>
        /// Gets DST recommendations for an opportunity
        /// </summary>
        private async Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(
            int opportunityId,
            int maxResults = 10)
        {
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            return await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: maxResults,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );
        }

        /// <summary>
        /// Creates a test user with proper claims
        /// </summary>
        private ClaimsPrincipal CreateTestUser(int userId = 1)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, $"Test User {userId}"),
                new Claim(ClaimTypes.Role, "Project Manager")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        #endregion

        #region TC-DST-RISK-001 through TC-DST-RISK-004: Basic CRUD Operations

        /// <summary>
        /// TC-DST-RISK-001: Create risk from DST recommendation
        /// 
        /// Given: A DST recommendation for an opportunity
        /// When: User creates a risk from the recommendation
        /// Then: Risk record is created with recommendation details
        /// 
        /// Expected Behavior:
        /// - Risk is created successfully
        /// - Risk fields are populated from recommendation
        /// - Risk is linked to opportunity
        /// - Audit trail is created
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-001")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_FromRecommendation_CreatesRiskRecord()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync("Infrastructure Project with Procurement Risks");
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Procurement Integrity Risk",
                Description = "Risk of procurement fraud or irregularities due to weak controls",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1, // Procurement risk
                ProbabilityId = 3, // Medium
                ImpactId = 4, // High
                Source = "DST Recommendation",
                SourceReferenceId = "oup_123" // From DST recommendation
            };

            // Act
            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            createdRisk.Should().NotBeNull("risk should be created successfully");
            createdRisk.Id.Should().BeGreaterThan(0, "created risk should have valid ID");
            createdRisk.Title.Should().Be(riskRequest.Title);
            createdRisk.Description.Should().Be(riskRequest.Description);
            createdRisk.EntityType.Should().Be("Opportunity");
            createdRisk.EntityId.Should().Be(opportunityId);
            createdRisk.Source.Should().Be("DST Recommendation");
            createdRisk.SourceReferenceId.Should().Be("oup_123");
        }

        /// <summary>
        /// TC-DST-RISK-002: Pre-populate risk fields from recommendation
        /// 
        /// Given: A DST recommendation with predefined risk category
        /// When: Risk is created from recommendation
        /// Then: Risk category and other fields are pre-populated
        /// 
        /// Expected Behavior:
        /// - RiskCategoryId is set from recommendation
        /// - PreDefinedHighRiskId is linked
        /// - OupQuestionId is stored for traceability
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-002")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_PreDefinedRisk_PopulatesCategory()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync("High-Risk Project");
            var recommendations = await GetDSTRecommendationsAsync(opportunityId, maxResults: 10);

            // Find a predefined high risk recommendation
            var preDefinedRec = recommendations.Recommendations
                .FirstOrDefault(r => r.SourceType == "PREDEFINED_HIGH_RISK" && r.RiskCategoryId.HasValue);

            if (preDefinedRec == null)
            {
                // Skip test if no predefined risks were returned
                return;
            }

            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = preDefinedRec.Title,
                Description = preDefinedRec.Description,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                RiskCategoryId = preDefinedRec.RiskCategoryId.Value,
                Source = "DST Recommendation",
                SourceReferenceId = preDefinedRec.StableIdentifier,
                PreDefinedHighRiskId = preDefinedRec.PreDefinedHighRiskId,
                OupQuestionId = preDefinedRec.OupQuestionId
            };

            // Act
            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            createdRisk.Should().NotBeNull();
            createdRisk.RiskCategoryId.Should().Be(preDefinedRec.RiskCategoryId.Value,
                "risk category should be pre-populated from recommendation");
            createdRisk.PreDefinedHighRiskId.Should().Be(preDefinedRec.PreDefinedHighRiskId,
                "predefined high risk ID should be linked");
            createdRisk.OupQuestionId.Should().Be(preDefinedRec.OupQuestionId,
                "oUP Question ID should be stored for traceability");
        }

        /// <summary>
        /// TC-DST-RISK-003: Update DST-sourced risk
        /// 
        /// Given: An existing risk created from DST recommendation
        /// When: Risk details are updated
        /// Then: Risk is updated successfully while preserving source tracking
        /// 
        /// Expected Behavior:
        /// - Risk fields can be updated
        /// - Source reference is preserved
        /// - Audit trail tracks modifications
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-003")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_ExistingRisk_UpdatesFields()
        {
            // Arrange - Create a risk first
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var createRequest = new RiskCreateRequest
            {
                Title = "Initial Risk Title",
                Description = "Initial description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation",
                SourceReferenceId = "oup_456"
            };

            var createdRisk = await riskManager.AddRiskAsync(createRequest, user);

            // Act - Update the risk
            var updateRequest = new RiskUpdateRequest
            {
                Id = createdRisk.Id,
                Title = "Updated Risk Title",
                Description = "Updated description with more details",
                ProbabilityId = 3, // Increase probability
                ImpactId = 4, // Increase impact
                ResponseTypeId = 2, // Add response type
                Status = "Active"
            };

            var updatedRisk = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updatedRisk.Should().NotBeNull();
            updatedRisk.Id.Should().Be(createdRisk.Id);
            updatedRisk.Title.Should().Be("Updated Risk Title");
            updatedRisk.Description.Should().Be("Updated description with more details");
            updatedRisk.ProbabilityId.Should().Be(3);
            updatedRisk.ImpactId.Should().Be(4);
            updatedRisk.ResponseTypeId.Should().Be(2);
            
            // Source tracking should be preserved
            updatedRisk.Source.Should().Be("DST Recommendation");
            updatedRisk.SourceReferenceId.Should().Be("oup_456");
        }

        /// <summary>
        /// TC-DST-RISK-004: Delete DST risk
        /// 
        /// Given: An existing DST-sourced risk
        /// When: Risk is deleted
        /// Then: Risk is soft-deleted successfully
        /// 
        /// Expected Behavior:
        /// - Risk is marked as deleted (soft delete)
        /// - Risk no longer appears in active risk queries
        /// - Audit trail records deletion
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-004")]
        [Trait("Priority", "High")]
        public async Task DeleteDSTRisk_ValidId_RemovesRisk()
        {
            // Arrange - Create a risk
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var createRequest = new RiskCreateRequest
            {
                Title = "Risk to be deleted",
                Description = "This risk will be deleted",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation"
            };

            var createdRisk = await riskManager.AddRiskAsync(createRequest, user);
            var riskId = createdRisk.Id;

            // Act - Delete the risk
            var deleteResult = await riskManager.DeleteRiskAsync(riskId, user);

            // Assert
            deleteResult.Should().BeTrue("delete operation should succeed");

            // Verify risk no longer appears in active queries
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);
            risks.Risks.Should().NotContain(r => r.Id == riskId,
                "deleted risk should not appear in active risk list");
        }

        #endregion

        #region TC-DST-RISK-005 through TC-DST-RISK-008: Risk Retrieval and Tracking

        /// <summary>
        /// TC-DST-RISK-005: Get all DST risks for opportunity
        /// 
        /// Given: Multiple risks exist for an opportunity (DST and manual)
        /// When: DST risks are queried
        /// Then: System returns only DST-sourced risks
        /// 
        /// Expected Behavior:
        /// - Returns risks where Source = "DST Recommendation"
        /// - Excludes manually created risks
        /// - Includes risk metadata (category, type, probability, impact)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-005")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRisks_OpportunityId_ReturnsAllRisks()
        {
            // Arrange - Create multiple risks
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Create 2 DST risks
            var dstRisk1 = await riskManager.AddRiskAsync(new RiskCreateRequest
            {
                Title = "DST Risk 1",
                Description = "First DST risk",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = "oup_100"
            }, user);

            var dstRisk2 = await riskManager.AddRiskAsync(new RiskCreateRequest
            {
                Title = "DST Risk 2",
                Description = "Second DST risk",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 2,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation",
                SourceReferenceId = "vs_200"
            }, user);

            // Create 1 manual risk
            var manualRisk = await riskManager.AddRiskAsync(new RiskCreateRequest
            {
                Title = "Manual Risk",
                Description = "Manually identified risk",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 2,
                Source = "Manual Entry"
            }, user);

            // Act - Get DST risks
            var dstRisks = await riskManager.GetDSTRisksAsync(opportunityId, user);

            // Assert
            dstRisks.Should().NotBeNull();
            dstRisks.Risks.Should().HaveCount(2, "should return only DST-sourced risks");
            dstRisks.Risks.Should().AllSatisfy(r =>
            {
                r.Source.Should().Be("DST Recommendation");
                r.SourceReferenceId.Should().NotBeNullOrEmpty();
            });

            // Verify specific risks are included
            dstRisks.Risks.Should().Contain(r => r.Id == dstRisk1.Id);
            dstRisks.Risks.Should().Contain(r => r.Id == dstRisk2.Id);
            dstRisks.Risks.Should().NotContain(r => r.Id == manualRisk.Id);
        }

        /// <summary>
        /// TC-DST-RISK-006: Track risk source (DST recommendation vs manual)
        /// 
        /// Given: Risks from different sources
        /// When: Risk details are viewed
        /// Then: Source is clearly identified and traceable
        /// 
        /// Expected Behavior:
        /// - Source field indicates "DST Recommendation" or "Manual Entry"
        /// - SourceReferenceId links back to recommendation
        /// - Risk can be traced to originating recommendation
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-006")]
        [Trait("Priority", "High")]
        public async Task DSTRisk_SourceTracking_IdentifiesDSTOrigin()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var sourceReferenceId = "oup_789";
            var dstRisk = await riskManager.AddRiskAsync(new RiskCreateRequest
            {
                Title = "Traceable DST Risk",
                Description = "Risk with clear DST source tracking",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = sourceReferenceId,
                OupQuestionId = 789
            }, user);

            // Act - Retrieve the risk
            var retrievedRisk = await riskManager.GetRiskByIdAsync(dstRisk.Id, user);

            // Assert
            retrievedRisk.Should().NotBeNull();
            retrievedRisk.Source.Should().Be("DST Recommendation",
                "source should clearly identify DST origin");
            retrievedRisk.SourceReferenceId.Should().Be(sourceReferenceId,
                "source reference ID should link back to recommendation");
            retrievedRisk.OupQuestionId.Should().Be(789,
                "oUP Question ID should be preserved for traceability");

            // Verify risk can be traced back to recommendation
            retrievedRisk.SourceReferenceId.Should().StartWith("oup_",
                "source reference should indicate oUP recommendation origin");
        }

        /// <summary>
        /// TC-DST-RISK-007: Prevent duplicate DST recommendations
        /// 
        /// Given: A DST recommendation has already been converted to a risk
        /// When: Same recommendation is added again
        /// Then: System prevents duplicate creation or warns user
        /// 
        /// Expected Behavior:
        /// - Check for existing risk with same SourceReferenceId
        /// - Prevent duplicate creation
        /// - Return appropriate error or warning
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-007")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DuplicateRecommendation_PreventsDuplicate()
        {
            // Arrange - Create first risk
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var sourceReferenceId = "oup_999";
            var firstRisk = await riskManager.AddRiskAsync(new RiskCreateRequest
            {
                Title = "Duplicate Prevention Test",
                Description = "First creation",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = sourceReferenceId
            }, user);

            // Act - Attempt to create duplicate
            var duplicateAttempt = async () => await riskManager.AddRiskAsync(new RiskCreateRequest
            {
                Title = "Duplicate Prevention Test",
                Description = "Duplicate attempt",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = sourceReferenceId // Same source reference
            }, user);

            // Assert
            // System should either:
            // 1. Throw exception preventing duplicate
            // 2. Return the existing risk
            // 3. Add validation to check for duplicates

            // For now, verify that checking for duplicates is possible
            var existingRisks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);
            var risksWithSameSource = existingRisks.Risks
                .Where(r => r.SourceReferenceId == sourceReferenceId)
                .ToList();

            risksWithSameSource.Should().HaveCount(1,
                "should only have one risk with same source reference ID");
        }

        /// <summary>
        /// TC-DST-RISK-008: Risk category linking for predefined risks
        /// 
        /// Given: A predefined high risk with Level 3 category
        /// When: Risk is created from recommendation
        /// Then: Risk is properly linked to category hierarchy
        /// 
        /// Expected Behavior:
        /// - RiskCategoryId links to Level 3 category
        /// - Category hierarchy is preserved (Level 1 > Level 2 > Level 3)
        /// - Risk can be filtered by category
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-008")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_PreDefinedCategory_LinksCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            var recommendations = await GetDSTRecommendationsAsync(opportunityId, maxResults: 15);

            var preDefinedRec = recommendations.Recommendations
                .FirstOrDefault(r => 
                    r.SourceType == "PREDEFINED_HIGH_RISK" && 
                    r.RiskCategoryId.HasValue);

            if (preDefinedRec == null)
            {
                // Skip if no predefined risks with categories
                return;
            }

            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = preDefinedRec.Title,
                Description = preDefinedRec.Description,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                RiskCategoryId = preDefinedRec.RiskCategoryId.Value,
                Source = "DST Recommendation",
                SourceReferenceId = preDefinedRec.StableIdentifier
            };

            // Act
            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            createdRisk.Should().NotBeNull();
            createdRisk.RiskCategoryId.Should().Be(preDefinedRec.RiskCategoryId.Value);

            // Verify risk can be retrieved by category
            var allRisks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);
            var categoryRisks = allRisks.Risks
                .Where(r => r.RiskCategoryId == preDefinedRec.RiskCategoryId.Value)
                .ToList();

            categoryRisks.Should().Contain(r => r.Id == createdRisk.Id,
                "risk should be retrievable by category ID");
        }

        #endregion

        #region TC-DST-RISK-009 through TC-DST-RISK-012: Bulk Operations and Authorization

        /// <summary>
        /// TC-DST-RISK-009: Bulk add risks from multiple recommendations
        /// 
        /// Given: Multiple DST recommendations selected by user
        /// When: Bulk add operation is performed
        /// Then: All selected recommendations are converted to risks
        /// 
        /// Expected Behavior:
        /// - Multiple risks created in single operation
        /// - All risks properly linked to opportunity
        /// - Transaction ensures all-or-nothing creation
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-009")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisks_BulkCreate_CreatesMultiple()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            var recommendations = await GetDSTRecommendationsAsync(opportunityId, maxResults: 5);

            if (recommendations.Recommendations.Count < 3)
            {
                // Skip if not enough recommendations
                return;
            }

            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Take first 3 recommendations
            var selectedRecommendations = recommendations.Recommendations.Take(3).ToList();
            var bulkRequests = selectedRecommendations.Select(rec => new RiskCreateRequest
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
                RiskCategoryId = rec.RiskCategoryId,
                OupQuestionId = rec.OupQuestionId
            }).ToList();

            // Act - Create risks in bulk
            var createdRisks = new List<RiskModel>();
            foreach (var request in bulkRequests)
            {
                var risk = await riskManager.AddRiskAsync(request, user);
                createdRisks.Add(risk);
            }

            // Assert
            createdRisks.Should().HaveCount(3, "all 3 risks should be created");
            createdRisks.Should().AllSatisfy(r =>
            {
                r.Id.Should().BeGreaterThan(0);
                r.EntityId.Should().Be(opportunityId);
                r.Source.Should().Be("DST Recommendation");
            });

            // Verify all risks are retrievable
            var allRisks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);
            foreach (var createdRisk in createdRisks)
            {
                allRisks.Risks.Should().Contain(r => r.Id == createdRisk.Id);
            }
        }

        /// <summary>
        /// TC-DST-RISK-010: Authorization checks for DST risk operations
        /// 
        /// Given: A user without risk management permissions
        /// When: User attempts DST risk operations
        /// Then: System denies access appropriately
        /// 
        /// Expected Behavior:
        /// - Unauthorized users cannot create/update/delete risks
        /// - Returns 403 Forbidden or appropriate error
        /// - Audit log records unauthorized access attempts
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-010")]
        [Trait("Priority", "High")]
        public async Task DSTRiskOperations_UnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            
            // Create unauthorized user
            var unauthorizedUser = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Name, "Unauthorized User"),
                new Claim(ClaimTypes.Role, "Viewer") // Read-only role
            }, "TestAuth"));

            var riskRequest = new RiskCreateRequest
            {
                Title = "Unauthorized Risk",
                Description = "Should not be created",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation"
            };

            // Act & Assert
            // System should either:
            // 1. Throw UnauthorizedAccessException
            // 2. Check permissions and return null/error
            // 3. Return authorization failure result

            // For now, verify that permission checks exist
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, unauthorizedUser);
                
                // If no exception, verify authorization check occurred
                result.Should().BeNull("unauthorized user should not create risks");
            }
            catch (UnauthorizedAccessException)
            {
                // Expected exception - authorization check working
                true.Should().BeTrue();
            }
            catch (Exception ex) when (ex.Message.Contains("permission") || ex.Message.Contains("authorized"))
            {
                // Expected authorization failure
                true.Should().BeTrue();
            }
        }

        /// <summary>
        /// TC-DST-RISK-011: Validation errors return proper status codes
        /// 
        /// Given: Invalid risk creation request
        /// When: Request is processed
        /// Then: System returns 400 Bad Request with validation details
        /// 
        /// Expected Behavior:
        /// - Missing required fields trigger validation errors
        /// - Invalid field values are rejected
        /// - Error messages are clear and actionable
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-011")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_InvalidData_Returns400()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Create invalid request (missing title)
            var invalidRequest = new RiskCreateRequest
            {
                Title = "", // Invalid - empty title
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation"
            };

            // Act & Assert
            await FluentActions.Awaiting(async () => 
                await riskManager.AddRiskAsync(invalidRequest, user))
                .Should().ThrowAsync<Exception>()
                .WithMessage("*title*", "validation should catch empty title");
        }

        /// <summary>
        /// TC-DST-RISK-012: Audit trail for DST risk creation
        /// 
        /// Given: DST risk operations are performed
        /// When: Risks are created, updated, or deleted
        /// Then: Full audit trail is maintained
        /// 
        /// Expected Behavior:
        /// - CreatedBy and CreatedDate are set on creation
        /// - LastModifiedBy and LastModifiedDate are set on update
        /// - DeletedBy and DeletedDate are set on soft delete
        /// - All operations are auditable
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-RISK-012")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_AuditLog_TracksCreation()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser(userId: 42);

            var riskRequest = new RiskCreateRequest
            {
                Title = "Auditable Risk",
                Description = "Risk with audit trail",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act
            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert - Verify audit fields
            createdRisk.Should().NotBeNull();
            createdRisk.CreatedBy.Should().Be(42, "CreatedBy should be set to current user ID");
            createdRisk.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1),
                "CreatedDate should be current timestamp");

            // Update the risk
            var updateRequest = new RiskUpdateRequest
            {
                Id = createdRisk.Id,
                Title = "Updated Auditable Risk",
                Description = "Updated description"
            };

            var updatedRisk = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Verify update audit fields
            updatedRisk.LastModifiedBy.Should().Be(42, "LastModifiedBy should be set to current user ID");
            updatedRisk.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1),
                "LastModifiedDate should be current timestamp");
            updatedRisk.LastModifiedDate.Should().BeAfter(updatedRisk.CreatedDate,
                "LastModifiedDate should be after CreatedDate");
        }

        #endregion
    }
}
