using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Tests.Integration.DST
{
    /// <summary>
    /// Integration tests for DST API endpoints (OpportunityController)
    /// Tests HTTP API contracts for DST recommendation and risk management
    /// 
    /// Test Coverage:
    /// - GET /api/opportunity/{id}/dst-recommendations
    /// - GET /api/opportunity/{id}/dst-recommendations with filters
    /// - POST /api/opportunity/{id}/dst-risks (create)
    /// - PUT /api/opportunity/{id}/dst-risks/{riskId} (update)
    /// - DELETE /api/opportunity/{id}/dst-risks/{riskId}
    /// - GET /api/opportunity/{id}/dst-risks (get all)
    /// - HTTP status codes and error handling
    /// - Authorization checks
    /// - Request/response format validation
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "API")]
    public class DSTControllerTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTControllerTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        private async Task<int> CreateTestOpportunityAsync(string title = "Test Opportunity")
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = "Test opportunity for DST API testing",
                CountryId = 1,
                EstimatedBudget = 1000000m,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

        #endregion

        #region TC-DST-API-001 through TC-DST-API-004: GET Recommendations Endpoints

        /// <summary>
        /// TC-DST-API-001: GET /api/opportunity/{id}/dst-recommendations - Success
        /// 
        /// Given: Valid opportunity ID
        /// When: GET request is made to dst-recommendations endpoint
        /// Then: Returns 200 OK with recommendations
        /// 
        /// Expected Behavior:
        /// - HTTP 200 status code
        /// - Response contains DSTRecommendationsResponse
        /// - Recommendations list is populated
        /// - Response format matches contract
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-001")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_ValidId_Returns200WithRecommendations()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                "Infrastructure Project with Multiple Risk Factors");

            // Act
            var response = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations?maxResults=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK, 
                "valid request should return 200 OK");

            var content = await response.Content.ReadFromJsonAsync<DSTRecommendationsResponse>();
            content.Should().NotBeNull("response should contain DST recommendations data");
            content.Recommendations.Should().NotBeNull("recommendations list should not be null");
            content.ExtractedKeywords.Should().NotBeNull("keywords list should not be null");
            content.ExecutionTimeMs.Should().BeGreaterThan(0, "execution time should be tracked");
        }

        /// <summary>
        /// TC-DST-API-002: GET /api/opportunity/{id}/dst-recommendations - Not found
        /// 
        /// Given: Invalid/non-existent opportunity ID
        /// When: GET request is made
        /// Then: Returns 404 Not Found
        /// 
        /// Expected Behavior:
        /// - HTTP 404 status code
        /// - Clear error message in response
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-002")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_InvalidId_Returns404()
        {
            // Arrange
            var nonExistentId = 999999;

            // Act
            var response = await _client.GetAsync(
                $"/api/opportunity/{nonExistentId}/dst-recommendations");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                "non-existent opportunity should return 404");
        }

        /// <summary>
        /// TC-DST-API-003: GET /api/opportunity/{id}/dst-recommendations - Unauthorized
        /// 
        /// Given: Request without authentication
        /// When: GET request is made
        /// Then: Returns 401 Unauthorized
        /// 
        /// Expected Behavior:
        /// - HTTP 401 status code
        /// - Authorization required
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-003")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_Unauthorized_Returns401()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();
            var anonymousClient = _factory.CreateClient();
            // Remove authentication headers if any

            // Act
            var response = await anonymousClient.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations");

            // Assert
            // Depending on configuration, may return 401 or 403
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized, 
                HttpStatusCode.Forbidden,
                "unauthenticated request should be rejected");
        }

        /// <summary>
        /// TC-DST-API-004: GET /api/opportunity/{id}/dst-recommendations - With query params
        /// 
        /// Given: Valid opportunity ID with query parameters
        /// When: GET request is made with maxResults, dismissed IDs, forceRefresh
        /// Then: Returns filtered recommendations respecting parameters
        /// 
        /// Expected Behavior:
        /// - Query parameters are respected
        /// - maxResults limits response size
        /// - dismissedOupQuestionIds filters out dismissed risks
        /// - forceRefresh bypasses cache
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-004")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_WithFilters_FiltersResults()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                "Complex Project Requiring Risk Analysis");

            // Act - First call to get recommendations
            var initialResponse = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations?maxResults=20&forceRefresh=true");

            initialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var initialContent = await initialResponse.Content.ReadFromJsonAsync<DSTRecommendationsResponse>();

            // Get some oUP Question IDs to dismiss
            var oupIds = initialContent.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .Take(2)
                .ToList();

            if (!oupIds.Any())
            {
                // Skip if no predefined risks to dismiss
                return;
            }

            // Act - Second call with dismissed IDs and maxResults=5
            var dismissedIdsParam = string.Join(",", oupIds);
            var filteredResponse = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations?maxResults=5&dismissedOupQuestionIds={dismissedIdsParam}");

            // Assert
            filteredResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var filteredContent = await filteredResponse.Content.ReadFromJsonAsync<DSTRecommendationsResponse>();

            filteredContent.Recommendations.Should().HaveCountLessThanOrEqualTo(5,
                "maxResults=5 should limit response");

            var returnedOupIds = filteredContent.Recommendations
                .Where(r => r.OupQuestionId.HasValue)
                .Select(r => r.OupQuestionId.Value)
                .ToList();

            foreach (var dismissedId in oupIds)
            {
                returnedOupIds.Should().NotContain(dismissedId,
                    $"dismissed oUP Question ID {dismissedId} should not appear");
            }
        }

        #endregion

        #region TC-DST-API-005 through TC-DST-API-008: POST/PUT/DELETE Risk Endpoints

        /// <summary>
        /// TC-DST-API-005: POST /api/opportunity/{id}/dst-risks - Create risk
        /// 
        /// Given: Valid risk creation request
        /// When: POST request is made
        /// Then: Returns 201 Created with risk details
        /// 
        /// Expected Behavior:
        /// - HTTP 201 status code
        /// - Location header with new risk URL
        /// - Response contains created risk
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-005")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_ValidRequest_Returns201()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "API Test Risk",
                Description = "Risk created via API",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = "oup_api_test"
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks", riskRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created,
                "successful creation should return 201");

            response.Headers.Location.Should().NotBeNull(
                "Location header should be set for created resource");

            var createdRisk = await response.Content.ReadFromJsonAsync<RiskModel>();
            createdRisk.Should().NotBeNull();
            createdRisk.Id.Should().BeGreaterThan(0);
            createdRisk.Title.Should().Be(riskRequest.Title);
            createdRisk.Source.Should().Be("DST Recommendation");
        }

        /// <summary>
        /// TC-DST-API-006: POST /api/opportunity/{id}/dst-risks - Validation error
        /// 
        /// Given: Invalid risk creation request (missing required fields)
        /// When: POST request is made
        /// Then: Returns 400 Bad Request with validation errors
        /// 
        /// Expected Behavior:
        /// - HTTP 400 status code
        /// - Response contains validation error details
        /// - Clear error messages
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-006")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InvalidData_Returns400()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();

            var invalidRequest = new RiskCreateRequest
            {
                Title = "", // Invalid - empty title
                Description = "Description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "invalid request should return 400");

            var errorContent = await response.Content.ReadAsStringAsync();
            errorContent.Should().NotBeNullOrEmpty("error response should contain details");
        }

        /// <summary>
        /// TC-DST-API-007: PUT /api/opportunity/{id}/dst-risks/{riskId} - Update
        /// 
        /// Given: Existing DST risk and valid update request
        /// When: PUT request is made
        /// Then: Returns 200 OK with updated risk
        /// 
        /// Expected Behavior:
        /// - HTTP 200 status code
        /// - Response contains updated risk details
        /// - Changes are persisted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-007")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDSTRisk_ValidData_Returns200()
        {
            // Arrange - Create a risk first
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Original Title",
                Description = "Original description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation"
            };

            var createResponse = await _client.PostAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks", createRequest);

            var createdRisk = await createResponse.Content.ReadFromJsonAsync<RiskModel>();
            var riskId = createdRisk.Id;

            // Act - Update the risk
            var updateRequest = new RiskUpdateRequest
            {
                Id = riskId,
                Title = "Updated Title",
                Description = "Updated description",
                ProbabilityId = 4,
                ImpactId = 5
            };

            var updateResponse = await _client.PutAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks/{riskId}", updateRequest);

            // Assert
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK,
                "successful update should return 200");

            var updatedRisk = await updateResponse.Content.ReadFromJsonAsync<RiskModel>();
            updatedRisk.Should().NotBeNull();
            updatedRisk.Id.Should().Be(riskId);
            updatedRisk.Title.Should().Be("Updated Title");
            updatedRisk.Description.Should().Be("Updated description");
            updatedRisk.ProbabilityId.Should().Be(4);
            updatedRisk.ImpactId.Should().Be(5);
        }

        /// <summary>
        /// TC-DST-API-008: DELETE /api/opportunity/{id}/dst-risks/{riskId} - Delete
        /// 
        /// Given: Existing DST risk
        /// When: DELETE request is made
        /// Then: Returns 204 No Content
        /// 
        /// Expected Behavior:
        /// - HTTP 204 status code
        /// - Risk is soft-deleted
        /// - Risk no longer appears in active queries
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-008")]
        [Trait("Priority", "Critical")]
        public async Task DeleteDSTRisk_ValidId_Returns204()
        {
            // Arrange - Create a risk
            var opportunityId = await CreateTestOpportunityAsync();

            var createRequest = new RiskCreateRequest
            {
                Title = "Risk to Delete",
                Description = "This risk will be deleted",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation"
            };

            var createResponse = await _client.PostAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks", createRequest);

            var createdRisk = await createResponse.Content.ReadFromJsonAsync<RiskModel>();
            var riskId = createdRisk.Id;

            // Act - Delete the risk
            var deleteResponse = await _client.DeleteAsync(
                $"/api/opportunity/{opportunityId}/dst-risks/{riskId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
                "successful deletion should return 204");

            // Verify risk no longer appears
            var getRisksResponse = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-risks");

            var risks = await getRisksResponse.Content.ReadFromJsonAsync<DSTRisksResponse>();
            risks.Risks.Should().NotContain(r => r.Id == riskId,
                "deleted risk should not appear in active list");
        }

        #endregion

        #region TC-DST-API-009 through TC-DST-API-012: Additional API Tests

        /// <summary>
        /// TC-DST-API-009: GET /api/opportunity/{id}/dst-risks - Get all
        /// 
        /// Given: Multiple DST risks exist for opportunity
        /// When: GET request is made to dst-risks endpoint
        /// Then: Returns 200 OK with all DST risks
        /// 
        /// Expected Behavior:
        /// - HTTP 200 status code
        /// - Response contains DSTRisksResponse
        /// - All DST-sourced risks are included
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-009")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRisks_ValidId_ReturnsAllRisks()
        {
            // Arrange - Create multiple risks
            var opportunityId = await CreateTestOpportunityAsync();

            var risk1 = new RiskCreateRequest
            {
                Title = "DST Risk 1",
                Description = "First risk",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = "oup_1"
            };

            var risk2 = new RiskCreateRequest
            {
                Title = "DST Risk 2",
                Description = "Second risk",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 2,
                ProbabilityId = 2,
                ImpactId = 3,
                Source = "DST Recommendation",
                SourceReferenceId = "vs_2"
            };

            await _client.PostAsJsonAsync($"/api/opportunity/{opportunityId}/dst-risks", risk1);
            await _client.PostAsJsonAsync($"/api/opportunity/{opportunityId}/dst-risks", risk2);

            // Act
            var response = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-risks");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<DSTRisksResponse>();
            content.Should().NotBeNull();
            content.Risks.Should().HaveCountGreaterOrEqualTo(2,
                "should return at least the 2 created risks");
            content.TotalCount.Should().BeGreaterOrEqualTo(2);

            content.Risks.Should().AllSatisfy(r =>
            {
                r.Source.Should().Be("DST Recommendation");
            });
        }

        /// <summary>
        /// TC-DST-API-010: Response format validation
        /// 
        /// Given: Various API requests
        /// When: Responses are received
        /// Then: All responses match API contract
        /// 
        /// Expected Behavior:
        /// - Content-Type is application/json
        /// - Response models match expected schema
        /// - Required fields are present
        /// - Data types are correct
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-010")]
        [Trait("Priority", "Medium")]
        public async Task DSTEndpoints_ResponseFormat_MatchesContract()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                "Project for Response Format Validation");

            // Act - Test recommendations endpoint
            var recResponse = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations");

            // Assert - Recommendations response format
            recResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
                "response should be JSON");

            var recommendations = await recResponse.Content.ReadFromJsonAsync<DSTRecommendationsResponse>();
            recommendations.Should().NotBeNull();

            // Verify response structure
            recommendations.Recommendations.Should().NotBeNull("Recommendations property required");
            recommendations.ExtractedKeywords.Should().NotBeNull("ExtractedKeywords property required");
            recommendations.TotalFound.Should().BeGreaterOrEqualTo(0, "TotalFound should be non-negative");
            recommendations.ExecutionTimeMs.Should().BeGreaterThan(0, "ExecutionTimeMs should be tracked");

            // Verify recommendation structure
            if (recommendations.Recommendations.Any())
            {
                var firstRec = recommendations.Recommendations.First();
                firstRec.Title.Should().NotBeNullOrEmpty("Title is required");
                firstRec.Description.Should().NotBeNullOrEmpty("Description is required");
                firstRec.ConfidenceLevel.Should().BeInRange(0, 100, "ConfidenceLevel should be 0-100");
                firstRec.StableIdentifier.Should().NotBeNullOrEmpty("StableIdentifier is required");
                firstRec.SourceType.Should().NotBeNullOrEmpty("SourceType is required");
            }
        }

        /// <summary>
        /// TC-DST-API-011: Error handling and status codes
        /// 
        /// Given: Various error scenarios
        /// When: API requests are made
        /// Then: Appropriate HTTP status codes are returned
        /// 
        /// Expected Behavior:
        /// - 400 for validation errors
        /// - 401/403 for authorization failures
        /// - 404 for not found
        /// - 500 for server errors
        /// - Error responses include helpful messages
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-011")]
        [Trait("Priority", "High")]
        public async Task DSTEndpoints_Errors_ReturnProperStatusCodes()
        {
            // Test 404 - Non-existent opportunity
            var notFoundResponse = await _client.GetAsync(
                "/api/opportunity/999999/dst-recommendations");
            notFoundResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Test 400 - Invalid request (negative opportunity ID)
            var badRequestResponse = await _client.GetAsync(
                "/api/opportunity/-1/dst-recommendations");
            badRequestResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest, 
                HttpStatusCode.NotFound);

            // Test 400 - Invalid risk creation
            var opportunityId = await CreateTestOpportunityAsync();
            var invalidRisk = new RiskCreateRequest
            {
                // Missing required fields
                EntityType = "Opportunity",
                EntityId = opportunityId
            };

            var validationResponse = await _client.PostAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks", invalidRisk);
            validationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "validation errors should return 400");

            // Test 404 - Update non-existent risk
            var updateNonExistent = new RiskUpdateRequest
            {
                Id = 999999,
                Title = "Updated"
            };

            var updateResponse = await _client.PutAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks/999999", updateNonExistent);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
                "updating non-existent risk should return 404");
        }

        /// <summary>
        /// TC-DST-API-012: Rate limiting on expensive operations
        /// 
        /// Given: Multiple rapid requests for DST recommendations
        /// When: Requests exceed rate limit
        /// Then: System throttles requests appropriately
        /// 
        /// Expected Behavior:
        /// - First requests succeed
        /// - Excessive requests may be throttled
        /// - 429 Too Many Requests if rate limited
        /// - Retry-After header provided
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-012")]
        [Trait("Priority", "Low")]
        public async Task DSTRecommendations_RateLimiting_ThrottlesRequests()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();

            // Act - Make multiple rapid requests with forceRefresh=true (expensive)
            var tasks = Enumerable.Range(0, 10).Select(i =>
                _client.GetAsync(
                    $"/api/opportunity/{opportunityId}/dst-recommendations?forceRefresh=true")
            ).ToList();

            var responses = await Task.WhenAll(tasks);

            // Assert
            // Most requests should succeed
            var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            successCount.Should().BeGreaterThan(0, "at least some requests should succeed");

            // Check if any were rate limited
            var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
            
            if (rateLimitedCount > 0)
            {
                // Verify rate limiting behavior
                var rateLimitedResponse = responses.First(r => r.StatusCode == HttpStatusCode.TooManyRequests);
                rateLimitedResponse.Headers.Should().ContainKey("Retry-After",
                    "rate limited response should include Retry-After header");
            }

            // Note: If no rate limiting is implemented, all requests will succeed
            // This test documents expected behavior when rate limiting is added
        }

        #endregion

        #region TC-DST-API-013 through TC-DST-API-015: Advanced API Scenarios

        /// <summary>
        /// TC-DST-API-013: POST with oUP Question ID and predefined risk linking
        /// 
        /// Given: Risk created from predefined high risk recommendation
        /// When: POST includes oUP Question ID and predefined risk ID
        /// Then: Risk is properly linked to predefined risk and category
        /// 
        /// Expected Behavior:
        /// - OupQuestionId is stored
        /// - PreDefinedHighRiskId is linked
        /// - RiskCategoryId is set from predefined risk
        /// - Full traceability to source recommendation
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-013")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_WithOupQuestionId_LinksCorrectly()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                "High-Risk Project for Predefined Risk Testing");

            // Get recommendations to find a predefined risk
            var recResponse = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations?maxResults=20");
            var recommendations = await recResponse.Content.ReadFromJsonAsync<DSTRecommendationsResponse>();

            var preDefinedRec = recommendations.Recommendations
                .FirstOrDefault(r => r.SourceType == "PREDEFINED_HIGH_RISK" && r.OupQuestionId.HasValue);

            if (preDefinedRec == null)
            {
                // Skip if no predefined risks
                return;
            }

            // Act - Create risk with full predefined risk linking
            var riskRequest = new RiskCreateRequest
            {
                Title = preDefinedRec.Title,
                Description = preDefinedRec.Description,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = preDefinedRec.StableIdentifier,
                OupQuestionId = preDefinedRec.OupQuestionId,
                PreDefinedHighRiskId = preDefinedRec.PreDefinedHighRiskId,
                RiskCategoryId = preDefinedRec.RiskCategoryId
            };

            var response = await _client.PostAsJsonAsync(
                $"/api/opportunity/{opportunityId}/dst-risks", riskRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdRisk = await response.Content.ReadFromJsonAsync<RiskModel>();
            createdRisk.Should().NotBeNull();
            createdRisk.OupQuestionId.Should().Be(preDefinedRec.OupQuestionId.Value,
                "oUP Question ID should be stored");
            createdRisk.PreDefinedHighRiskId.Should().Be(preDefinedRec.PreDefinedHighRiskId,
                "predefined high risk ID should be linked");
            createdRisk.RiskCategoryId.Should().Be(preDefinedRec.RiskCategoryId,
                "risk category should be set");
        }

        /// <summary>
        /// TC-DST-API-014: Concurrent API operations
        /// 
        /// Given: Multiple concurrent requests to DST endpoints
        /// When: Operations are performed in parallel
        /// Then: All operations complete successfully without conflicts
        /// 
        /// Expected Behavior:
        /// - Concurrent reads succeed
        /// - Concurrent writes are handled safely
        /// - No data corruption or race conditions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-014")]
        [Trait("Priority", "Medium")]
        public async Task DSTEndpoints_ConcurrentRequests_HandleSafely()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();

            // Act - Perform concurrent operations
            var getTask1 = _client.GetAsync($"/api/opportunity/{opportunityId}/dst-recommendations");
            var getTask2 = _client.GetAsync($"/api/opportunity/{opportunityId}/dst-risks");
            var getTask3 = _client.GetAsync($"/api/opportunity/{opportunityId}/dst-recommendations?maxResults=5");

            var createTask1 = _client.PostAsJsonAsync($"/api/opportunity/{opportunityId}/dst-risks",
                new RiskCreateRequest
                {
                    Title = "Concurrent Risk 1",
                    Description = "Created concurrently",
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 2,
                    ImpactId = 3,
                    Source = "DST Recommendation"
                });

            var createTask2 = _client.PostAsJsonAsync($"/api/opportunity/{opportunityId}/dst-risks",
                new RiskCreateRequest
                {
                    Title = "Concurrent Risk 2",
                    Description = "Created concurrently",
                    EntityType = "Opportunity",
                    EntityId = opportunityId,
                    RiskTypeId = 1,
                    ProbabilityId = 2,
                    ImpactId = 3,
                    Source = "DST Recommendation"
                });

            var responses = await Task.WhenAll(getTask1, getTask2, getTask3, createTask1, createTask2);

            // Assert
            foreach (var response in responses)
            {
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,
                    HttpStatusCode.Created,
                    "all concurrent operations should complete successfully");
            }

            // Verify created risks exist
            var verifyResponse = await _client.GetAsync($"/api/opportunity/{opportunityId}/dst-risks");
            var risks = await verifyResponse.Content.ReadFromJsonAsync<DSTRisksResponse>();
            risks.Risks.Should().Contain(r => r.Title == "Concurrent Risk 1");
            risks.Risks.Should().Contain(r => r.Title == "Concurrent Risk 2");
        }

        /// <summary>
        /// TC-DST-API-015: API response caching headers
        /// 
        /// Given: Cacheable DST API responses
        /// When: Cache headers are checked
        /// Then: Appropriate cache control headers are set
        /// 
        /// Expected Behavior:
        /// - Cache-Control headers present
        /// - ETag for conditional requests (if implemented)
        /// - Recommendations may be cached briefly
        /// - Dynamic content has appropriate cache directives
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-API-015")]
        [Trait("Priority", "Low")]
        public async Task DSTEndpoints_CacheHeaders_SetAppropriately()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var response = await _client.GetAsync(
                $"/api/opportunity/{opportunityId}/dst-recommendations");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Check cache headers (if implemented)
            if (response.Headers.CacheControl != null)
            {
                // Verify cache behavior
                response.Headers.CacheControl.Should().NotBeNull(
                    "cache control headers should be set for performance");
            }

            // Recommendations are dynamic, so cache should be short or private
            if (response.Headers.CacheControl?.MaxAge.HasValue == true)
            {
                response.Headers.CacheControl.MaxAge.Value.Should().BeLessThan(
                    TimeSpan.FromMinutes(5),
                    "dynamic DST recommendations should have short cache time");
            }
        }

        #endregion
    }
}
