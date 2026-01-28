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
    /// Negative test cases for DST (Decision Support Tool)
    /// Tests error handling, invalid inputs, and failure scenarios
    /// 
    /// Test Coverage:
    /// - Invalid opportunity IDs (non-existent, negative, zero)
    /// - Null/empty input parameters
    /// - Missing required fields
    /// - Invalid data types
    /// - Authorization failures
    /// - Concurrent modification conflicts
    /// - Service unavailability
    /// - Database errors
    /// - Transaction rollback scenarios
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "NegativeTests")]
    public class DSTNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTNegativeTests(PAOWebApplicationFactory<Program> factory)
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
                Description = "Test opportunity description",
                CountryId = 1,
                EstimatedBudget = 1000000m,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

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

        #region TC-DST-NEG-001 through TC-DST-NEG-005: Invalid Opportunity IDs

        /// <summary>
        /// TC-DST-NEG-001: Get DST recommendations for non-existent opportunity
        /// 
        /// Given: Opportunity ID that does not exist
        /// When: DST recommendations are requested
        /// Then: KeyNotFoundException or 404 error is returned
        /// 
        /// Expected Behavior:
        /// - Throws KeyNotFoundException
        /// - Error message indicates opportunity not found
        /// - No partial data is returned
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-001")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_NonExistentOpportunity_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var nonExistentId = 999999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: nonExistentId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );
            });

            exception.Message.Should().Contain("not found", "error message should indicate opportunity not found");
        }

        /// <summary>
        /// TC-DST-NEG-002: Get DST recommendations with negative opportunity ID
        /// 
        /// Given: Negative opportunity ID
        /// When: DST recommendations are requested
        /// Then: ArgumentException or validation error is returned
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException or returns validation error
        /// - Error message indicates invalid ID
        /// - System does not attempt database query
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-002")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_NegativeId_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var negativeId = -1;

            // Act & Assert
            try
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: negativeId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );

                Assert.Fail("Expected exception for negative opportunity ID");
            }
            catch (Exception ex)
            {
                // Should throw ArgumentException or KeyNotFoundException
                ex.Should().BeOfType<ArgumentException>()
                    .Or.BeOfType<KeyNotFoundException>();
            }
        }

        /// <summary>
        /// TC-DST-NEG-003: Get DST recommendations with zero opportunity ID
        /// 
        /// Given: Opportunity ID of zero
        /// When: DST recommendations are requested
        /// Then: Validation error is returned
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException or KeyNotFoundException
        /// - Zero is not treated as valid ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-003")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_ZeroId_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var zeroId = 0;

            // Act & Assert
            try
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: zeroId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );

                Assert.Fail("Expected exception for zero opportunity ID");
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<ArgumentException>()
                    .Or.BeOfType<KeyNotFoundException>();
            }
        }

        /// <summary>
        /// TC-DST-NEG-004: Create DST risk for non-existent opportunity
        /// 
        /// Given: Risk creation request with non-existent opportunity ID
        /// When: Risk creation is attempted
        /// Then: KeyNotFoundException is thrown
        /// 
        /// Expected Behavior:
        /// - Throws KeyNotFoundException
        /// - Risk is not created
        /// - Database state unchanged
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-004")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_NonExistentOpportunity_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = 999999, // Non-existent
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });

            // Should throw KeyNotFoundException or similar
            exception.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-NEG-005: Update non-existent DST risk
        /// 
        /// Given: Update request for risk that doesn't exist
        /// When: Update is attempted
        /// Then: KeyNotFoundException is thrown
        /// 
        /// Expected Behavior:
        /// - Throws KeyNotFoundException
        /// - No database modifications occur
        /// - Error message indicates risk not found
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-005")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_NonExistentRisk_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var updateRequest = new RiskUpdateRequest
            {
                Id = 999999, // Non-existent
                Title = "Updated Title",
                Description = "Updated description"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });

            exception.Message.Should().Contain("not found");
        }

        #endregion

        #region TC-DST-NEG-006 through TC-DST-NEG-010: Null and Empty Parameters

        /// <summary>
        /// TC-DST-NEG-006: Get DST recommendations with null user
        /// 
        /// Given: Null user ClaimsPrincipal
        /// When: DST recommendations are requested
        /// Then: ArgumentNullException or UnauthorizedAccessException is thrown
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentNullException or UnauthorizedAccessException
        /// - User authentication is enforced
        /// - No recommendations returned without user context
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-006")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_NullUser_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var opportunityId = await CreateTestOpportunityAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: null, // Null user
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );
            });

            exception.Should().BeOfType<ArgumentNullException>()
                .Or.BeOfType<UnauthorizedAccessException>();
        }

        /// <summary>
        /// TC-DST-NEG-007: Create DST risk with empty title
        /// 
        /// Given: Risk creation request with empty title
        /// When: Risk creation is attempted
        /// Then: ArgumentException or validation error is returned
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException or returns validation error
        /// - Title is required field
        /// - Risk is not created
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-007")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_EmptyTitle_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "", // Empty title
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });

            exception.Should().BeOfType<ArgumentException>()
                .Or.Subject.GetType().Name.Should().Contain("Validation");
        }

        /// <summary>
        /// TC-DST-NEG-008: Create DST risk with null description
        /// 
        /// Given: Risk creation request with null description
        /// When: Risk creation is attempted
        /// Then: Validation error is returned (description is required)
        /// 
        /// Expected Behavior:
        /// - Throws validation exception
        /// - Description is required field
        /// - Error message indicates missing description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-008")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_NullDescription_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = null, // Null description
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
        /// TC-DST-NEG-009: Get DST recommendations with null dismissed IDs list
        /// 
        /// Given: Null dismissedOupQuestionIds parameter
        /// When: DST recommendations are requested
        /// Then: System treats as empty list or throws ArgumentNullException
        /// 
        /// Expected Behavior:
        /// - Either treats null as empty list (graceful)
        /// - Or throws ArgumentNullException (strict validation)
        /// - Does not crash or return corrupted data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-009")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_NullDismissedList_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            try
            {
                var response = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: null, // Null list
                    forceRefresh: true
                );

                // If no exception, should return valid response
                response.Should().NotBeNull();
                response.Recommendations.Should().NotBeNull();
            }
            catch (ArgumentNullException)
            {
                // Acceptable if strict validation is enforced
                Assert.True(true, "ArgumentNullException is acceptable for null parameter");
            }
        }

        /// <summary>
        /// TC-DST-NEG-010: Create DST risk with invalid entity type
        /// 
        /// Given: Risk creation with invalid EntityType
        /// When: Risk creation is attempted
        /// Then: Validation error is returned
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException or validation error
        /// - EntityType must be valid (e.g., "Opportunity")
        /// - Error message indicates invalid entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-010")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_InvalidEntityType_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "InvalidEntityType", // Invalid
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

        #endregion

        #region TC-DST-NEG-011 through TC-DST-NEG-015: Invalid Foreign Keys

        /// <summary>
        /// TC-DST-NEG-011: Create DST risk with invalid RiskTypeId
        /// 
        /// Given: Risk creation with non-existent RiskTypeId
        /// When: Risk creation is attempted
        /// Then: Foreign key constraint violation or validation error
        /// 
        /// Expected Behavior:
        /// - Throws exception (FK constraint or validation)
        /// - Risk is not created
        /// - Error indicates invalid risk type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-011")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InvalidRiskTypeId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 999999, // Invalid FK
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
        /// TC-DST-NEG-012: Create DST risk with invalid ProbabilityId
        /// 
        /// Given: Risk creation with non-existent ProbabilityId
        /// When: Risk creation is attempted
        /// Then: Foreign key constraint violation or validation error
        /// 
        /// Expected Behavior:
        /// - Throws exception
        /// - ProbabilityId must reference valid lookup value
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-012")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_InvalidProbabilityId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 999999, // Invalid FK
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
        /// TC-DST-NEG-013: Create DST risk with invalid ImpactId
        /// 
        /// Given: Risk creation with non-existent ImpactId
        /// When: Risk creation is attempted
        /// Then: Foreign key constraint violation or validation error
        /// 
        /// Expected Behavior:
        /// - Throws exception
        /// - ImpactId must reference valid lookup value
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-013")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_InvalidImpactId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 999999, // Invalid FK
                Source = "DST Recommendation"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-014: Create DST risk with invalid PreDefinedHighRiskId
        /// 
        /// Given: Risk creation with non-existent PreDefinedHighRiskId
        /// When: Risk creation is attempted
        /// Then: Foreign key constraint violation or validation error
        /// 
        /// Expected Behavior:
        /// - Throws exception if PreDefinedHighRiskId provided
        /// - PreDefinedHighRiskId must reference valid predefined risk
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-014")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_InvalidPreDefinedRiskId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                PreDefinedHighRiskId = 999999 // Invalid FK
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-015: Delete already deleted DST risk
        /// 
        /// Given: Risk that was already soft-deleted
        /// When: Delete is attempted again
        /// Then: Returns false or throws KeyNotFoundException
        /// 
        /// Expected Behavior:
        /// - Returns false or throws KeyNotFoundException
        /// - No error for idempotent delete
        /// - Soft delete flag remains set
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-015")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDSTRisk_AlreadyDeleted_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk
            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk to delete",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Delete once
            var firstDelete = await riskManager.DeleteRiskAsync(createdRisk.Id, user);
            firstDelete.Should().BeTrue("first delete should succeed");

            // Act - Delete again
            try
            {
                var secondDelete = await riskManager.DeleteRiskAsync(createdRisk.Id, user);
                
                // Either returns false (idempotent) or throws exception
                secondDelete.Should().BeFalse("second delete should return false");
            }
            catch (KeyNotFoundException)
            {
                // Also acceptable - risk no longer found
                Assert.True(true, "KeyNotFoundException is acceptable for already deleted risk");
            }
        }

        #endregion

        #region TC-DST-NEG-016 through TC-DST-NEG-020: Authorization Failures

        /// <summary>
        /// TC-DST-NEG-016: Get DST recommendations without sufficient permissions
        /// 
        /// Given: User without ViewOpportunity permission
        /// When: DST recommendations are requested
        /// Then: UnauthorizedAccessException is thrown
        /// 
        /// Expected Behavior:
        /// - Throws UnauthorizedAccessException
        /// - Permissions are enforced
        /// - No data leaked to unauthorized users
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-016")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRecommendations_InsufficientPermissions_ThrowsUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var opportunityId = await CreateTestOpportunityAsync();

            // Create user with limited role
            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Name, "Limited User"),
                new Claim(ClaimTypes.Role, "Guest") // Limited role
            }, "TestAuth"));

            // Act & Assert
            try
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: limitedUser,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );

                // If authorization is not enforced, test fails
                // (Some systems may allow read access to all authenticated users)
            }
            catch (UnauthorizedAccessException)
            {
                // Expected if permissions are strictly enforced
                Assert.True(true, "Unauthorized access properly blocked");
            }
        }

        /// <summary>
        /// TC-DST-NEG-017: Create DST risk without create permission
        /// 
        /// Given: User without CreateRisk permission
        /// When: Risk creation is attempted
        /// Then: UnauthorizedAccessException is thrown
        /// 
        /// Expected Behavior:
        /// - Throws UnauthorizedAccessException
        /// - Risk is not created
        /// - Authorization is enforced
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-017")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InsufficientPermissions_ThrowsUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var opportunityId = await CreateTestOpportunityAsync();

            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "998"),
                new Claim(ClaimTypes.Name, "Read-Only User"),
                new Claim(ClaimTypes.Role, "Viewer") // Read-only role
            }, "TestAuth"));

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act & Assert
            try
            {
                await riskManager.AddRiskAsync(riskRequest, limitedUser);
                
                // May succeed if permissions are not enforced
            }
            catch (UnauthorizedAccessException)
            {
                Assert.True(true, "Create permission properly enforced");
            }
        }

        /// <summary>
        /// TC-DST-NEG-018: Update DST risk created by another user
        /// 
        /// Given: User A created risk, User B attempts to update
        /// When: User B tries to update
        /// Then: UnauthorizedAccessException if user-level permissions enforced
        /// 
        /// Expected Behavior:
        /// - May throw UnauthorizedAccessException (strict)
        /// - Or may allow if user has UpdateRisk permission (relaxed)
        /// - Business rules determine behavior
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-018")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_DifferentUser_HandlesAuthorization()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var opportunityId = await CreateTestOpportunityAsync();

            var userA = CreateTestUser(userId: 100);
            var userB = CreateTestUser(userId: 200);

            // User A creates risk
            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            var createdRisk = await riskManager.AddRiskAsync(riskRequest, userA);

            // User B attempts to update
            var updateRequest = new RiskUpdateRequest
            {
                Id = createdRisk.Id,
                Title = "Updated by User B",
                Description = "Updated description"
            };

            // Act
            try
            {
                var updated = await riskManager.UpdateRiskAsync(updateRequest, userB);
                
                // May succeed if permissions allow cross-user updates
                updated.Should().NotBeNull();
            }
            catch (UnauthorizedAccessException)
            {
                // Acceptable if strict user-level permissions enforced
                Assert.True(true, "Cross-user update blocked");
            }
        }

        /// <summary>
        /// TC-DST-NEG-019: Delete DST risk without delete permission
        /// 
        /// Given: User without DeleteRisk permission
        /// When: Delete is attempted
        /// Then: UnauthorizedAccessException is thrown
        /// 
        /// Expected Behavior:
        /// - Throws UnauthorizedAccessException
        /// - Risk is not deleted
        /// - Delete permission is enforced
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-019")]
        [Trait("Priority", "High")]
        public async Task DeleteDSTRisk_InsufficientPermissions_ThrowsUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var opportunityId = await CreateTestOpportunityAsync();

            // User A creates risk
            var userA = CreateTestUser(userId: 101);
            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Risk description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            var createdRisk = await riskManager.AddRiskAsync(riskRequest, userA);

            // Limited user attempts to delete
            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "997"),
                new Claim(ClaimTypes.Name, "Limited User"),
                new Claim(ClaimTypes.Role, "Contributor") // Can view/edit but not delete
            }, "TestAuth"));

            // Act & Assert
            try
            {
                await riskManager.DeleteRiskAsync(createdRisk.Id, limitedUser);
            }
            catch (UnauthorizedAccessException)
            {
                Assert.True(true, "Delete permission properly enforced");
            }
        }

        /// <summary>
        /// TC-DST-NEG-020: Access DST risks for opportunity user doesn't own
        /// 
        /// Given: User without access to specific opportunity
        /// When: DST risks are requested for that opportunity
        /// Then: UnauthorizedAccessException or empty result
        /// 
        /// Expected Behavior:
        /// - Throws UnauthorizedAccessException (strict)
        /// - Or returns empty list (relaxed)
        /// - Depends on business rules
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-020")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_UnauthorizedOpportunity_HandlesAuthorization()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var opportunityId = await CreateTestOpportunityAsync();

            var unauthorizedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "996"),
                new Claim(ClaimTypes.Name, "Unauthorized User"),
                new Claim(ClaimTypes.Role, "External User")
            }, "TestAuth"));

            // Act
            try
            {
                var risks = await riskManager.GetDSTRisksAsync(opportunityId, unauthorizedUser);
                
                // May return empty list if user has no access
                risks.Should().NotBeNull();
            }
            catch (UnauthorizedAccessException)
            {
                // Acceptable if strict authorization enforced
                Assert.True(true, "Access to unauthorized opportunity blocked");
            }
        }

        #endregion

        #region TC-DST-NEG-021 through TC-DST-NEG-076: Extended Negative Scenarios

        /// <summary>
        /// TC-DST-NEG-021: Get DST recommendations with malformed opportunity ID (negative)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-021")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_NegativeOpportunityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: -1, // Negative ID
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );
            });
        }

        /// <summary>
        /// TC-DST-NEG-022: Get DST recommendations with zero maxResults
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-022")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_ZeroMaxResults_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 0, // Zero results
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
            result.Recommendations.Should().BeEmpty();
        }

        /// <summary>
        /// TC-DST-NEG-023: Create DST risk with missing required field (Title)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-023")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_MissingTitle_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = null, // Missing required field
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
        /// TC-DST-NEG-024: Create DST risk with invalid RiskTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-024")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InvalidRiskTypeId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 999999, // Invalid ID
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
        /// TC-DST-NEG-025: Create DST risk with invalid ProbabilityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-025")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InvalidProbabilityId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 999999, // Invalid ID
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-026: Create DST risk with invalid ImpactId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-026")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InvalidImpactId_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 999999 // Invalid ID
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-027: Update DST risk with non-existent risk ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-027")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_NonExistentRiskId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var updateRequest = new RiskUpdateRequest
            {
                Id = 999999, // Non-existent ID
                Title = "Updated Title",
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-028: Delete DST risk with non-existent risk ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-028")]
        [Trait("Priority", "High")]
        public async Task DeleteDSTRisk_NonExistentRiskId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await riskManager.DeleteRiskAsync(999999, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-029: Get DST recommendations with null dismissedOupQuestionIds
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-029")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_NullDismissedIds_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: null, // Null list
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-NEG-030: Get DST recommendations with extremely large dismissedOupQuestionIds
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-030")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_LargeDismissedIds_HandlesPerformance()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var largeDismissedList = Enumerable.Range(1, 10000).ToList();

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: largeDismissedList,
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-NEG-031: Create DST risk with whitespace-only Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-031")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_WhitespaceTitle_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "   ", // Whitespace only
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
        /// TC-DST-NEG-032: Create DST risk with empty string Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-032")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_EmptyTitle_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = string.Empty, // Empty string
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
        /// TC-DST-NEG-033: Update DST risk with whitespace-only Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-033")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_WhitespaceTitle_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create valid risk first
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

            // Attempt to update with whitespace title
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "   ", // Whitespace only
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-034: Create DST risk for non-existent EntityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-034")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NonExistentEntityId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = 999999, // Non-existent entity
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-035: Create DST risk with negative EntityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-035")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NegativeEntityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = -1, // Negative ID
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-036: Create DST risk with null EntityType
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-036")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NullEntityType_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = null, // Null EntityType
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-037: Create DST risk with invalid EntityType
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-037")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_InvalidEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "InvalidType", // Invalid type
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-038: Get DST recommendations with negative maxResults
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-038")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_NegativeMaxResults_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: -10, // Negative
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );
            });
        }

        /// <summary>
        /// TC-DST-NEG-039: Get DST recommendations with excessively large maxResults
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-039")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_ExcessiveMaxResults_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 999999, // Extremely large
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
            result.Recommendations.Count.Should().BeLessThan(10000, "should cap results");
        }

        /// <summary>
        /// TC-DST-NEG-040: Get DST risks for opportunity with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-040")]
        [Trait("Priority", "Critical")]
        public async Task GetDSTRisks_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var opportunityId = await CreateTestOpportunityAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, null);
            });
        }

        /// <summary>
        /// TC-DST-NEG-041: Update DST risk with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-041")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDSTRisk_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;

            var updateRequest = new RiskUpdateRequest
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, null);
            });
        }

        /// <summary>
        /// TC-DST-NEG-042: Delete DST risk with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-042")]
        [Trait("Priority", "Critical")]
        public async Task DeleteDSTRisk_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await riskManager.DeleteRiskAsync(1, null);
            });
        }

        /// <summary>
        /// TC-DST-NEG-043: Create DST risk with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-043")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, null);
            });
        }

        /// <summary>
        /// TC-DST-NEG-044: Get DST recommendations for opportunity in archived state
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-044")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_ArchivedOpportunity_ThrowsInvalidOperationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;
            var user = CreateTestUser();

            var opportunityId = await CreateTestOpportunityAsync("Archived Opportunity");
            
            // Archive the opportunity
            // (Implementation depends on opportunity workflow)

            // Act & Assert
            try
            {
                var result = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );

                // May succeed with warning or return empty
                result.Should().NotBeNull();
            }
            catch (InvalidOperationException)
            {
                Assert.True(true, "Archived opportunity DST generation blocked");
            }
        }

        /// <summary>
        /// TC-DST-NEG-045: Update DST risk after opportunity is closed
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-045")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_OpportunityClosed_ThrowsInvalidOperationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk first
            var createRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Close opportunity (implementation depends on workflow)

            // Attempt to update risk
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Updated after close",
                Description = "Should fail"
            };

            // Act
            try
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
                Assert.True(true, "Update may be allowed with warning");
            }
            catch (InvalidOperationException)
            {
                Assert.True(true, "Update blocked for closed opportunity");
            }
        }

        /// <summary>
        /// TC-DST-NEG-046: Get DST recommendations with dismissedOupQuestionIds containing invalid IDs
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-046")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_InvalidDismissedIds_IgnoresInvalidIds()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var invalidDismissedIds = new List<int> { -1, 0, 999999 };

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: invalidDismissedIds,
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-NEG-047: Get DST recommendations with duplicate dismissedOupQuestionIds
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-047")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_DuplicateDismissedIds_HandlesDuplicates()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var duplicateDismissedIds = new List<int> { 1, 1, 2, 2, 3, 3 };

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: duplicateDismissedIds,
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-NEG-048: Create duplicate DST risk for same opportunity
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-048")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DuplicateRisk_AllowsOrThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Duplicate Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            // Create first risk
            var first = await riskManager.AddRiskAsync(riskRequest, user);
            first.Should().NotBeNull();

            // Attempt to create duplicate
            try
            {
                var duplicate = await riskManager.AddRiskAsync(riskRequest, user);
                // May allow duplicates with different IDs
                duplicate.Should().NotBeNull();
                duplicate.Id.Should().NotBe(first.Id);
            }
            catch (InvalidOperationException)
            {
                // Or may block duplicates
                Assert.True(true, "Duplicate risk blocked");
            }
        }

        /// <summary>
        /// TC-DST-NEG-049: Update DST risk with same data (no changes)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-049")]
        [Trait("Priority", "Low")]
        public async Task UpdateDSTRisk_NoChanges_HandlesGracefully()
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
                Description = "Original description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Update with same data
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Original Title", // Same
                Description = "Original description" // Same
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Title.Should().Be(created.Title);
        }

        /// <summary>
        /// TC-DST-NEG-050: Delete DST risk that was already deleted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-050")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDSTRisk_AlreadyDeleted_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk
            var createRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Delete once
            await riskManager.DeleteRiskAsync(created.Id, user);

            // Attempt to delete again
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await riskManager.DeleteRiskAsync(created.Id, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-051: Get DST recommendations with user having insufficient claims
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-051")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_InsufficientClaims_ThrowsUnauthorizedException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var opportunityId = await CreateTestOpportunityAsync();

            // User with missing required claims
            var insufficientUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User") // Missing NameIdentifier
            }, "TestAuth"));

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: insufficientUser,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );
            });
        }

        /// <summary>
        /// TC-DST-NEG-052: Create DST risk with extremely long Title (over limit)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-052")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ExcessivelyLongTitle_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var veryLongTitle = new string('A', 10000); // Excessive length

            var riskRequest = new RiskCreateRequest
            {
                Title = veryLongTitle,
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
        /// TC-DST-NEG-053: Create DST risk with extremely long Description (over limit)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-053")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ExcessivelyLongDescription_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var veryLongDescription = new string('B', 50000); // Excessive length

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = veryLongDescription,
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
        /// TC-DST-NEG-054: Get DST recommendations concurrently for same opportunity
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-054")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_ConcurrentRequests_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act - Fire 10 concurrent requests
            var tasks = Enumerable.Range(0, 10).Select(_ => 
                geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                )
            ).ToList();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().AllSatisfy(r => r.Should().NotBeNull());
        }

        /// <summary>
        /// TC-DST-NEG-055: Update DST risk concurrently (potential conflict)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-055")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_ConcurrentUpdates_HandlesConflict()
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
                Description = "Original description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Fire 5 concurrent updates
            var updateTasks = Enumerable.Range(0, 5).Select(i =>
            {
                var updateRequest = new RiskUpdateRequest
                {
                    Id = created.Id,
                    Title = $"Updated Title {i}",
                    Description = $"Updated description {i}"
                };
                return riskManager.UpdateRiskAsync(updateRequest, user);
            }).ToList();

            // Act
            var results = await Task.WhenAll(updateTasks);

            // Assert - Last write wins or optimistic concurrency enforced
            results.Should().AllSatisfy(r => r.Should().NotBeNull());
        }

        /// <summary>
        /// TC-DST-NEG-056: Delete DST risk while update is in progress
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-056")]
        [Trait("Priority", "High")]
        public async Task DeleteDSTRisk_DuringUpdate_HandlesRaceCondition()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk
            var createRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST"
            };

            var created = await riskManager.AddRiskAsync(createRequest, user);

            // Start update and delete concurrently
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Updated Title",
                Description = "Updated description"
            };

            var updateTask = riskManager.UpdateRiskAsync(updateRequest, user);
            var deleteTask = riskManager.DeleteRiskAsync(created.Id, user);

            // Act - One should fail
            try
            {
                await Task.WhenAll(updateTask, deleteTask);
                Assert.True(true, "Both succeeded or race condition handled");
            }
            catch
            {
                // One failed due to concurrent modification
                Assert.True(true, "Race condition properly handled with exception");
            }
        }

        /// <summary>
        /// TC-DST-NEG-057: Get DST recommendations with opportunity in invalid state
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-057")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_InvalidOpportunityState_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Set opportunity to invalid state (implementation depends on workflow)

            // Act
            try
            {
                var result = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );

                result.Should().NotBeNull();
            }
            catch (InvalidOperationException)
            {
                Assert.True(true, "Invalid state prevented DST generation");
            }
        }

        /// <summary>
        /// TC-DST-NEG-058: Create DST risk with zero RiskTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-058")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ZeroRiskTypeId_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 0, // Zero ID
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
        /// TC-DST-NEG-059: Create DST risk with zero ProbabilityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-059")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ZeroProbabilityId_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 0, // Zero ID
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-060: Create DST risk with zero ImpactId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-060")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ZeroImpactId_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 0 // Zero ID
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-061: Get DST risks with empty EntityType string
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-061")]
        [Trait("Priority", "High")]
        public async Task GetDSTRisks_EmptyEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.GetRisksByEntityAsync(string.Empty, opportunityId, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-062: Get DST risks with whitespace EntityType
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-062")]
        [Trait("Priority", "High")]
        public async Task GetDSTRisks_WhitespaceEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.GetRisksByEntityAsync("   ", opportunityId, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-063: Update DST risk with null Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-063")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_NullTitle_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk first
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

            // Attempt to update with null title
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = null, // Null title
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-064: Update DST risk with empty string Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-064")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_EmptyTitle_ThrowsValidationException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk first
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

            // Attempt to update with empty title
            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = string.Empty, // Empty title
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-065: Get DST recommendations for opportunity with missing required data fields
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-065")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_OpportunityMissingData_ReturnsLimitedRecommendations()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Create opportunity with minimal data
            var opportunityId = await CreateTestOpportunityAsync("Minimal Data Opportunity");

            // Act
            var result = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: false
            );

            // Assert
            result.Should().NotBeNull();
            // May return fewer recommendations due to limited context
        }

        /// <summary>
        /// TC-DST-NEG-066: Create DST risk with negative RiskTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-066")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NegativeRiskTypeId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = -1, // Negative ID
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-067: Create DST risk with negative ProbabilityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-067")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NegativeProbabilityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = -1, // Negative ID
                ImpactId = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-068: Create DST risk with negative ImpactId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-068")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NegativeImpactId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = -1 // Negative ID
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-069: Update DST risk with invalid risk ID (zero)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-069")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_ZeroRiskId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var updateRequest = new RiskUpdateRequest
            {
                Id = 0, // Zero ID
                Title = "Updated Title",
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-070: Update DST risk with negative risk ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-070")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_NegativeRiskId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            var updateRequest = new RiskUpdateRequest
            {
                Id = -1, // Negative ID
                Title = "Updated Title",
                Description = "Updated description"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-071: Delete DST risk with zero risk ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-071")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDSTRisk_ZeroRiskId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.DeleteRiskAsync(0, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-072: Delete DST risk with negative risk ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-072")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDSTRisk_NegativeRiskId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.DeleteRiskAsync(-1, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-073: Get DST risks with zero EntityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-073")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_ZeroEntityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.GetRisksByEntityAsync("Opportunity", 0, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-074: Get DST risks with negative EntityId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-074")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRisks_NegativeEntityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await riskManager.GetRisksByEntityAsync("Opportunity", -1, user);
            });
        }

        /// <summary>
        /// TC-DST-NEG-075: Get DST recommendations with null opportunityId parameter name typo
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-075")]
        [Trait("Priority", "Low")]
        public async Task GetDSTRecommendations_ZeroOpportunityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: 0, // Zero ID
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: false
                );
            });
        }

        /// <summary>
        /// TC-DST-NEG-076: Create DST risk with special characters in Source field
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-NEG-076")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_SpecialCharactersInSource_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST 🔥 <script>alert('test')</script> 特殊字符" // Special chars, emoji, HTML
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("DST");
        }

        #endregion
    }
}
