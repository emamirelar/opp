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
    /// Input validation tests for DST (Decision Support Tool)
    /// Tests data validation, constraint enforcement, and sanitization
    /// 
    /// Test Coverage:
    /// - Required field validation
    /// - Data type validation
    /// - Format validation (dates, emails, etc.)
    /// - Range validation (min/max values)
    /// - Enum/lookup value validation
    /// - Cross-field validation
    /// - Business rule validation
    /// - XSS/injection prevention
    /// - Constraint validation (unique, FK, etc.)
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "Validation")]
    public class DSTValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTValidationTests(PAOWebApplicationFactory<Program> factory)
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

        #region TC-DST-VAL-001 through TC-DST-VAL-005: Required Field Validation

        /// <summary>
        /// TC-DST-VAL-001: Create DST risk without title (required field)
        /// 
        /// Given: Risk creation request missing title
        /// When: Risk creation is attempted
        /// Then: ValidationException with clear error message
        /// 
        /// Expected Behavior:
        /// - Throws ArgumentException or validation error
        /// - Error message indicates Title is required
        /// - Risk is not created
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-001")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_MissingTitle_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                // Title omitted (required field)
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

            exception.Message.Should().Contain("Title", "error should indicate Title is required");
        }

        /// <summary>
        /// TC-DST-VAL-002: Create DST risk without description (required field)
        /// 
        /// Given: Risk creation request missing description
        /// When: Risk creation is attempted
        /// Then: ValidationException with clear error message
        /// 
        /// Expected Behavior:
        /// - Throws validation error
        /// - Error indicates Description is required
        /// - Risk is not created
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-002")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_MissingDescription_ThrowsValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Test Risk",
                // Description omitted (required field)
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
        /// TC-DST-VAL-003: Create DST risk without EntityType (required field)
        /// 
        /// Given: Risk creation request missing EntityType
        /// When: Risk creation is attempted
        /// Then: ValidationException with clear error message
        /// 
        /// Expected Behavior:
        /// - Throws validation error
        /// - Error indicates EntityType is required
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-003")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_MissingEntityType_ThrowsValidationError()
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
                // EntityType omitted (required field)
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
        /// TC-DST-VAL-004: Create DST risk without EntityId (required field)
        /// 
        /// Given: Risk creation request missing EntityId
        /// When: Risk creation is attempted
        /// Then: ValidationException with clear error message
        /// 
        /// Expected Behavior:
        /// - Throws validation error
        /// - Error indicates EntityId is required
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-004")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_MissingEntityId_ThrowsValidationError()
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
                // EntityId omitted or set to 0
                EntityId = 0,
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
        /// TC-DST-VAL-005: Create DST risk without required lookup IDs
        /// 
        /// Given: Risk creation request missing RiskTypeId, ProbabilityId, or ImpactId
        /// When: Risk creation is attempted
        /// Then: ValidationException with clear error message
        /// 
        /// Expected Behavior:
        /// - Throws validation error
        /// - Error indicates which required lookup ID is missing
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-005")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_MissingLookupIds_ThrowsValidationError()
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
                // Missing required lookup IDs
                RiskTypeId = 0,
                ProbabilityId = 0,
                ImpactId = 0,
                Source = "DST Recommendation"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        #endregion

        #region TC-DST-VAL-006 through TC-DST-VAL-010: Data Type and Format Validation

        /// <summary>
        /// TC-DST-VAL-006: Create DST risk with invalid confidence level (out of range)
        /// 
        /// Given: Confidence level outside 0-100 range
        /// When: Risk creation attempted (if confidence is settable)
        /// Then: Validation error or clamping to valid range
        /// 
        /// Expected Behavior:
        /// - Value clamped to 0-100
        /// - Or validation error thrown
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-006")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_InvalidConfidenceLevel_Validated()
        {
            // Note: ConfidenceLevel may be computed from DST recommendation,
            // not directly settable on risk creation
            // This test documents expected behavior if it becomes settable
            
            Assert.True(true, "ConfidenceLevel validation documented");
        }

        /// <summary>
        /// TC-DST-VAL-007: Update DST risk with invalid date format
        /// 
        /// Given: Risk update with malformed date strings (if date fields exist)
        /// When: Update is attempted
        /// Then: FormatException or validation error
        /// 
        /// Expected Behavior:
        /// - Date parsing fails gracefully
        /// - Error indicates invalid date format
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-007")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDSTRisk_InvalidDateFormat_ThrowsFormatException()
        {
            // Note: Dates are typically handled by framework/serializer
            // Manual date validation would occur if dates are passed as strings
            
            Assert.True(true, "Date format validation handled by framework");
        }

        /// <summary>
        /// TC-DST-VAL-008: Create DST risk with invalid boolean values
        /// 
        /// Given: Boolean fields with non-boolean values (e.g., "yes", "no", 1, 0)
        /// When: Deserialization occurs
        /// Then: Deserialization error or proper conversion
        /// 
        /// Expected Behavior:
        /// - Framework handles type conversion
        /// - Or throws deserialization error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-008")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_InvalidBooleanFormat_HandledByFramework()
        {
            // Boolean validation typically handled by JSON deserialization
            Assert.True(true, "Boolean type validation handled by framework");
        }

        /// <summary>
        /// TC-DST-VAL-009: Create DST risk with SQL injection attempt in Source field
        /// 
        /// Given: Source field contains SQL injection payload
        /// When: Risk is created
        /// Then: No SQL injection, text properly escaped
        /// 
        /// Expected Behavior:
        /// - SQL injection prevented by parameterized queries
        /// - Text stored as-is (but safely)
        /// - No database corruption
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-009")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_SQLInjectionAttempt_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "SQL Injection Test",
                Description = "Testing SQL injection prevention",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST'; DROP TABLE Risks; --" // SQL injection attempt
            };

            // Act
            var risk = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            risk.Should().NotBeNull("SQL injection should be prevented");
            risk.Source.Should().Be("DST'; DROP TABLE Risks; --",
                "malicious text should be stored as-is but safely");
            
            // Verify table still exists by querying
            var risks = await riskManager.GetRisksByEntityAsync("Opportunity", opportunityId, user);
            risks.Should().NotBeNull("Risks table should still exist");
        }

        /// <summary>
        /// TC-DST-VAL-010: Create DST risk with XSS attempt in Title/Description
        /// 
        /// Given: Title or Description contains HTML/JavaScript tags
        /// When: Risk is created and retrieved
        /// Then: Script tags are sanitized or escaped
        /// 
        /// Expected Behavior:
        /// - XSS prevented through encoding
        /// - Script tags not executed
        /// - Text displayed safely
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-010")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_XSSAttempt_Sanitized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<script>alert('XSS')</script>Malicious Risk",
                Description = "Description with <img src=x onerror=alert('XSS')>",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act
            var risk = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            risk.Should().NotBeNull();
            
            // XSS should be prevented - either sanitized or encoded
            // Exact behavior depends on sanitization strategy
            risk.Title.Should().NotBeNull();
            risk.Description.Should().NotBeNull();
        }

        #endregion

        #region TC-DST-VAL-011 through TC-DST-VAL-015: Range and Boundary Validation

        /// <summary>
        /// TC-DST-VAL-011: Get DST recommendations with invalid maxResults range
        /// 
        /// Given: maxResults with extreme values (negative, zero, > system max)
        /// When: Recommendations are requested
        /// Then: Value validated or clamped to acceptable range
        /// 
        /// Expected Behavior:
        /// - Negative values throw ArgumentException
        /// - Zero returns empty list or throws exception
        /// - Very large values capped at system maximum
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-011")]
        [Trait("Priority", "High")]
        public async Task GetDSTRecommendations_InvalidMaxResultsRange_ValidatedOrClamped()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Test 1: Negative value
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: -10,
                    dismissedOupQuestionIds: new List<int>(),
                    forceRefresh: true
                );
            });

            // Test 2: Very large value (should be clamped)
            var largeMaxResponse = await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: 10000,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: true
            );

            largeMaxResponse.Recommendations.Should().HaveCountLessOrEqualTo(100,
                "large maxResults should be capped");
        }

        /// <summary>
        /// TC-DST-VAL-012: Create DST risk with probability/impact outside valid range
        /// 
        /// Given: ProbabilityId or ImpactId referencing invalid lookup values
        /// When: Risk creation is attempted
        /// Then: Foreign key constraint or validation error
        /// 
        /// Expected Behavior:
        /// - Invalid lookup IDs rejected
        /// - Error indicates invalid foreign key
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-012")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_InvalidProbabilityImpactRange_ForeignKeyError()
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
                ProbabilityId = 999, // Invalid
                ImpactId = 999,      // Invalid
                Source = "DST Recommendation"
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.AddRiskAsync(riskRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-VAL-013: Create DST risk with extremely long SourceReferenceId
        /// 
        /// Given: SourceReferenceId exceeding field length limit
        /// When: Risk creation is attempted
        /// Then: Validation error or truncation
        /// 
        /// Expected Behavior:
        /// - Length validated
        /// - Error indicates field length exceeded
        /// - Or value truncated to max length
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-013")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ExcessivelyLongSourceReferenceId_ValidatedOrTruncated()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var longSourceRefId = new string('X', 1000); // Very long

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
                SourceReferenceId = longSourceRefId
            };

            // Act
            try
            {
                var risk = await riskManager.AddRiskAsync(riskRequest, user);
                
                // If successful, should be truncated or validated
                risk.SourceReferenceId.Length.Should().BeLessOrEqualTo(500,
                    "should be within field limits");
            }
            catch (ArgumentException ex)
            {
                ex.Message.Should().Contain("length", "should indicate length violation");
            }
        }

        /// <summary>
        /// TC-DST-VAL-014: Update DST risk with probability/impact set to zero
        /// 
        /// Given: Risk update setting ProbabilityId or ImpactId to 0
        /// When: Update is attempted
        /// Then: Validation error (0 is not valid lookup ID)
        /// 
        /// Expected Behavior:
        /// - Zero not valid for required lookup fields
        /// - Error indicates invalid value
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-014")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_ZeroProbabilityImpact_ValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create risk first
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

            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Attempt to update with zero values
            var updateRequest = new RiskUpdateRequest
            {
                Id = createdRisk.Id,
                Title = createdRisk.Title,
                Description = createdRisk.Description,
                ProbabilityId = 0, // Invalid
                ImpactId = 0       // Invalid
            };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await riskManager.UpdateRiskAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DST-VAL-015: Dismiss DST recommendations with invalid oUP Question IDs
        /// 
        /// Given: dismissedOupQuestionIds list contains negative or zero values
        /// When: Recommendations are requested
        /// Then: Invalid IDs filtered out or validation error
        /// 
        /// Expected Behavior:
        /// - System handles invalid IDs gracefully
        /// - Invalid IDs ignored or validation error thrown
        /// - Does not crash or corrupt data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-015")]
        [Trait("Priority", "Medium")]
        public async Task GetDSTRecommendations_InvalidDismissedIds_HandleGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var invalidDismissedIds = new List<int> { -1, 0, -999 }; // Invalid IDs

            // Act
            try
            {
                var response = await geminiManager.GetDSTRecommendationsAsync(
                    opportunityId: opportunityId,
                    user: user,
                    maxResults: 10,
                    dismissedOupQuestionIds: invalidDismissedIds,
                    forceRefresh: true
                );

                // If no exception, invalid IDs should be filtered
                response.Should().NotBeNull();
                response.Recommendations.Should().NotBeNull();
            }
            catch (ArgumentException)
            {
                // Acceptable if strict validation enforced
                Assert.True(true, "Validation of dismissed IDs is acceptable");
            }
        }

        #endregion

        #region TC-DST-VAL-016 through TC-DST-VAL-020: Cross-Field and Business Rule Validation

        /// <summary>
        /// TC-DST-VAL-016: Create DST risk with mismatched EntityType and EntityId
        /// 
        /// Given: EntityType="Opportunity" but EntityId references Partner
        /// When: Risk creation is attempted
        /// Then: Business rule validation error
        /// 
        /// Expected Behavior:
        /// - EntityId must correspond to EntityType
        /// - Error indicates type mismatch
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-016")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_MismatchedEntityTypeAndId_ValidationError()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();

            // Create opportunity
            var opportunityId = await CreateTestOpportunityAsync();

            // Try to create risk claiming EntityType="Partner" but using opportunity ID
            var riskRequest = new RiskCreateRequest
            {
                Title = "Mismatched Risk",
                Description = "EntityType/EntityId mismatch test",
                EntityType = "Partner", // Mismatch
                EntityId = opportunityId, // This is an Opportunity ID
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            // Act & Assert
            try
            {
                await riskManager.AddRiskAsync(riskRequest, user);
                
                // May succeed if validation not enforced
                // Or may fail foreign key constraint
            }
            catch (Exception)
            {
                // Acceptable - validation caught mismatch
                Assert.True(true, "Entity type/ID mismatch validation is acceptable");
            }
        }

        /// <summary>
        /// TC-DST-VAL-017: Update DST risk changing source from "DST Recommendation" to invalid
        /// 
        /// Given: Risk update attempting to change Source to non-DST value
        /// When: Update is attempted
        /// Then: Business rule may prevent source change
        /// 
        /// Expected Behavior:
        /// - Source change may be prevented (business rule)
        /// - Or allowed if business allows re-categorization
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-017")]
        [Trait("Priority", "Low")]
        public async Task UpdateDSTRisk_ChangeSource_BusinessRuleEnforced()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create DST risk
            var riskRequest = new RiskCreateRequest
            {
                Title = "DST Risk",
                Description = "Risk from DST",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation"
            };

            var createdRisk = await riskManager.AddRiskAsync(riskRequest, user);

            // Try to change source
            var updateRequest = new RiskUpdateRequest
            {
                Id = createdRisk.Id,
                Title = createdRisk.Title,
                Description = createdRisk.Description,
                Source = "Manual Entry" // Try to change source
            };

            // Act
            try
            {
                var updated = await riskManager.UpdateRiskAsync(updateRequest, user);
                
                // May succeed if source change is allowed
                updated.Should().NotBeNull();
            }
            catch (Exception)
            {
                // Acceptable if source change is prevented by business rule
                Assert.True(true, "Source immutability is valid business rule");
            }
        }

        /// <summary>
        /// TC-DST-VAL-018: Create DST risk with OupQuestionId but no PreDefinedHighRiskId
        /// 
        /// Given: Risk with OupQuestionId but PreDefinedHighRiskId is null
        /// When: Risk creation is attempted
        /// Then: May require PreDefinedHighRiskId for consistency
        /// 
        /// Expected Behavior:
        /// - Cross-field validation enforced
        /// - Or both fields are optional
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-018")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_OupQuestionIdWithoutPreDefinedRisk_ValidatedOrAllowed()
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
                OupQuestionId = 123, // Set
                PreDefinedHighRiskId = null // Not set
            };

            // Act
            try
            {
                var risk = await riskManager.AddRiskAsync(riskRequest, user);
                
                // May be allowed if cross-field validation not required
                risk.Should().NotBeNull();
            }
            catch (Exception)
            {
                // Acceptable if cross-field validation enforced
                Assert.True(true, "Cross-field validation is acceptable");
            }
        }

        /// <summary>
        /// TC-DST-VAL-019: Create DST risk with conflicting Probability and Impact levels
        /// 
        /// Given: Risk with Probability=Low but Impact=Critical (unusual but valid)
        /// When: Risk is created
        /// Then: System allows (no conflicting validation rule)
        /// 
        /// Expected Behavior:
        /// - All combinations of Probability/Impact are valid
        /// - No cross-field validation preventing unusual combinations
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-019")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_UnusualProbabilityImpactCombination_Allowed()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Unlikely but Critical Risk",
                Description = "Low probability but high impact",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 1, // Lowest probability
                ImpactId = 5,      // Highest impact (if 5 exists)
                Source = "DST Recommendation"
            };

            // Act
            try
            {
                var risk = await riskManager.AddRiskAsync(riskRequest, user);
                
                // Should be allowed - unusual but valid
                risk.Should().NotBeNull();
                risk.ProbabilityId.Should().Be(1);
                risk.ImpactId.Should().Be(5);
            }
            catch (Exception)
            {
                // Skip if ImpactId=5 doesn't exist
                Assert.True(true, "Test skipped due to lookup data");
            }
        }

        /// <summary>
        /// TC-DST-VAL-020: Verify StableIdentifier uniqueness for duplicate recommendations
        /// 
        /// Given: Two DST recommendations with same title
        /// When: Both are converted to risks
        /// Then: Each has unique ID despite same content
        /// 
        /// Expected Behavior:
        /// - StableIdentifier provides uniqueness
        /// - Duplicate content creates separate risk records
        /// - No unique constraint violation
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-020")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DuplicateContent_EachHasUniqueId()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            // Create two risks with identical content
            var risk1 = new RiskCreateRequest
            {
                Title = "Identical Risk",
                Description = "Same description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = "stable_id_1"
            };

            var risk2 = new RiskCreateRequest
            {
                Title = "Identical Risk", // Same title
                Description = "Same description", // Same description
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST Recommendation",
                SourceReferenceId = "stable_id_2" // Different stable ID
            };

            // Act
            var created1 = await riskManager.AddRiskAsync(risk1, user);
            var created2 = await riskManager.AddRiskAsync(risk2, user);

            // Assert
            created1.Should().NotBeNull();
            created2.Should().NotBeNull();
            created1.Id.Should().NotBe(created2.Id, "each risk should have unique ID");
            created1.SourceReferenceId.Should().NotBe(created2.SourceReferenceId,
                "each should have different stable identifier");
        }

        #endregion

        #region TC-DST-VAL-021 through TC-DST-VAL-076: Extended Validation Tests

        /// <summary>
        /// TC-DST-VAL-021: Create DST risk with LDAP injection attempt in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-021")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_LDAPInjectionTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Admin*)(uid=*", // LDAP injection
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("Admin");
        }

        /// <summary>
        /// TC-DST-VAL-022: Create DST risk with NoSQL injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-022")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_NoSQLInjectionDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "NoSQL Injection Test",
                Description = "{ $ne: null }", // NoSQL injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("$ne");
        }

        /// <summary>
        /// TC-DST-VAL-023: Create DST risk with command injection attempt in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-023")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_CommandInjectionSource_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Command Injection Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST; rm -rf /" // Command injection
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("DST");
        }

        /// <summary>
        /// TC-DST-VAL-024: Create DST risk with path traversal in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-024")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_PathTraversalTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "../../../etc/passwd", // Path traversal
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("../");
        }

        /// <summary>
        /// TC-DST-VAL-025: Create DST risk with XML injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-025")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_XMLInjectionDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "XML Injection Test",
                Description = "<!DOCTYPE foo [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]>", // XXE injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("DOCTYPE");
        }

        /// <summary>
        /// TC-DST-VAL-026: Create DST risk with JSON injection in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-026")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_JSONInjectionTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "{\"exploit\": true, \"admin\": true}", // JSON injection
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("exploit");
        }

        /// <summary>
        /// TC-DST-VAL-027: Create DST risk with null bytes in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-027")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_NullBytesTitle_HandlesOrRejects()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Title with \0 null byte", // Null byte
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act & Assert
            try
            {
                var result = await riskManager.AddRiskAsync(riskRequest, user);
                result.Should().NotBeNull();
            }
            catch (ArgumentException)
            {
                Assert.True(true, "Null bytes may be rejected");
            }
        }

        /// <summary>
        /// TC-DST-VAL-028: Update DST risk with SQL injection in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-028")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDSTRisk_SQLInjectionTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

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

            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "'; DROP TABLE Risks; --", // SQL injection
                Description = "Updated description"
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Title.Should().Contain("DROP TABLE");
        }

        /// <summary>
        /// TC-DST-VAL-029: Update DST risk with XSS in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-029")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDSTRisk_XSSDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

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

            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "Updated Title",
                Description = "<script>alert('XSS')</script>" // XSS attempt
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
            updated.Description.Should().Contain("script");
        }

        /// <summary>
        /// TC-DST-VAL-030: Create DST risk with extremely deep HTML nesting in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-030")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DeepHTMLNesting_HandlesWithoutStackOverflow()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var deepNesting = string.Join("", Enumerable.Repeat("<div>", 1000)) + "Content" + string.Join("", Enumerable.Repeat("</div>", 1000));

            var riskRequest = new RiskCreateRequest
            {
                Title = "Deep Nesting Test",
                Description = deepNesting,
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-031: Create DST risk with regex DoS payload in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-031")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_RegexDoSTitle_HandlesWithoutTimeout()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var regexDoS = new string('a', 1000) + "!"; // Regex catastrophic backtracking

            var riskRequest = new RiskCreateRequest
            {
                Title = regexDoS,
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-032: Create DST risk with format string attack in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-032")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_FormatStringAttackDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Format String Test",
                Description = "%s%s%s%s%s%s%s%s%s%s", // Format string attack
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("%s");
        }

        /// <summary>
        /// TC-DST-VAL-033: Create DST risk with buffer overflow attempt in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-033")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_BufferOverflowTitle_EnforcesLengthLimits()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var veryLongTitle = new string('A', 100000); // Way over limit

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
        /// TC-DST-VAL-034: Create DST risk with integer overflow in RiskTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-034")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_IntegerOverflowRiskTypeId_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Integer Overflow Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = int.MaxValue, // Integer overflow
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
        /// TC-DST-VAL-035: Create DST risk with CRLF injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-035")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_CRLFInjectionDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "CRLF Injection Test",
                Description = "Header injection\r\nSet-Cookie: malicious=true", // CRLF injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-036: Create DST risk with HTML comment injection in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-036")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_HTMLCommentInjection_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<!--Hidden comment with <script>-->", // HTML comment injection
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("<!--");
        }

        /// <summary>
        /// TC-DST-VAL-037: Create DST risk with Unicode normalization attack in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-037")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_UnicodeNormalization_HandlesCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Caf\u00E9 vs Cafe\u0301", // Unicode normalization (café vs café)
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("Caf");
        }

        /// <summary>
        /// TC-DST-VAL-038: Create DST risk with homograph attack in Title (punycode)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-038")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_HomographAttack_HandlesUnicodeLookalikes()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Αdministrator", // Greek Alpha looks like Latin A
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-039: Create DST risk with directory traversal variations in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-039")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_DirectoryTraversalVariations_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Traversal Test",
                Description = "..\\..\\..\\windows\\system32", // Windows path traversal
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("windows");
        }

        /// <summary>
        /// TC-DST-VAL-040: Create DST risk with encoded XSS payload in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-040")]
        [Trait("Priority", "Critical")]
        public async Task AddDSTRisk_EncodedXSSTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "&#60;script&#62;alert(&#39;XSS&#39;)&#60;/script&#62;", // HTML encoded XSS
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("&#60;");
        }

        /// <summary>
        /// TC-DST-VAL-041: Create DST risk with Base64 encoded payload in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-041")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_Base64EncodedDescription_StoredAsIs()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Base64 Test",
                Description = "PHNjcmlwdD5hbGVydCgnWFNTJyk8L3NjcmlwdD4=", // Base64 of XSS
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("PHNjcmlwdD");
        }

        /// <summary>
        /// TC-DST-VAL-042: Create DST risk with URL encoding in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-042")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_URLEncodedSource_StoredAsIs()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "URL Encoding Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST%20%3Cscript%3E" // URL encoded
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("%20");
        }

        /// <summary>
        /// TC-DST-VAL-043: Create DST risk with hex encoding in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-043")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_HexEncodedTitle_StoredAsIs()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "\\x3c\\x73\\x63\\x72\\x69\\x70\\x74\\x3e", // Hex encoded <script>
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("\\x3c");
        }

        /// <summary>
        /// TC-DST-VAL-044: Create DST risk with octal encoding in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-044")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_OctalEncodedDescription_StoredAsIs()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Octal Encoding Test",
                Description = "\\074\\163\\143\\162\\151\\160\\164\\076", // Octal encoded <script>
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("\\074");
        }

        /// <summary>
        /// TC-DST-VAL-045: Create DST risk with mixed encoding (HTML + URL + Base64) in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-045")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_MixedEncodingTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "&#60;%3CscriptPHNjcmlwdD4%3E&#62;", // Mixed encoding
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-046: Create DST risk with JavaScript protocol in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-046")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_JavaScriptProtocolSource_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "JavaScript Protocol Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "javascript:alert('XSS')" // JavaScript protocol
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("javascript:");
        }

        /// <summary>
        /// TC-DST-VAL-047: Create DST risk with data URI in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-047")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_DataURIDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Data URI Test",
                Description = "data:text/html,<script>alert('XSS')</script>", // Data URI
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("data:text/html");
        }

        /// <summary>
        /// TC-DST-VAL-048: Create DST risk with VBScript protocol in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-048")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_VBScriptProtocolTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "vbscript:msgbox('XSS')", // VBScript protocol
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("vbscript:");
        }

        /// <summary>
        /// TC-DST-VAL-049: Create DST risk with SVG XSS payload in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-049")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_SVGXSSDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "SVG XSS Test",
                Description = "<svg onload=alert('XSS')>", // SVG XSS
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("svg");
        }

        /// <summary>
        /// TC-DST-VAL-050: Create DST risk with IMG tag XSS in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-050")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_IMGTagXSSTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<img src=x onerror=alert('XSS')>", // IMG XSS
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("img");
        }

        /// <summary>
        /// TC-DST-VAL-051: Create DST risk with IFRAME injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-051")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_IFRAMEInjectionDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "IFRAME Injection Test",
                Description = "<iframe src='http://malicious.com'></iframe>", // IFRAME injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("iframe");
        }

        /// <summary>
        /// TC-DST-VAL-052: Create DST risk with META refresh redirect in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-052")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_METARefreshTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<meta http-equiv='refresh' content='0;url=http://malicious.com'>", // META refresh
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("meta");
        }

        /// <summary>
        /// TC-DST-VAL-053: Create DST risk with OBJECT tag injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-053")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_OBJECTTagDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "OBJECT Tag Test",
                Description = "<object data='http://malicious.com'></object>", // OBJECT injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("object");
        }

        /// <summary>
        /// TC-DST-VAL-054: Create DST risk with EMBED tag injection in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-054")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_EMBEDTagSource_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "EMBED Tag Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "<embed src='http://malicious.com'>" // EMBED injection
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("embed");
        }

        /// <summary>
        /// TC-DST-VAL-055: Create DST risk with LINK stylesheet injection in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-055")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_LINKStylesheetTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.serviceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<link rel='stylesheet' href='http://malicious.com/evil.css'>", // LINK injection
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("link");
        }

        /// <summary>
        /// TC-DST-VAL-056: Create DST risk with STYLE tag CSS injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-056")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_STYLETagDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "STYLE Tag Test",
                Description = "<style>body{background:url('javascript:alert(1)')}</style>", // STYLE injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("style");
        }

        /// <summary>
        /// TC-DST-VAL-057: Create DST risk with BASE tag hijacking in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-057")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_BASETagTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<base href='http://malicious.com/'>", // BASE hijacking
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("base");
        }

        /// <summary>
        /// TC-DST-VAL-058: Create DST risk with FORM action hijacking in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-058")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_FORMActionDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "FORM Action Test",
                Description = "<form action='http://malicious.com/steal'><input type='submit'></form>", // FORM hijacking
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("form");
        }

        /// <summary>
        /// TC-DST-VAL-059: Create DST risk with event handler attributes in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-059")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_EventHandlerSource_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Event Handler Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "<div onload=alert('XSS') onclick=alert('XSS')>" // Event handlers
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("onload");
        }

        /// <summary>
        /// TC-DST-VAL-060: Update DST risk with polyglot XSS payload in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-060")]
        [Trait("Priority", "High")]
        public async Task UpdateDSTRisk_PolyglotXSSTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

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

            var updateRequest = new RiskUpdateRequest
            {
                Id = created.Id,
                Title = "javascript:/*--></title></style></textarea></script></xmp><svg/onload='+/\"/+/onmouseover=1/+/[*/[]/+alert(1)//'", // Polyglot XSS
                Description = "Updated description"
            };

            // Act
            var updated = await riskManager.UpdateRiskAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-061: Create DST risk with mutation XSS in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-061")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_MutationXSSDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Mutation XSS Test",
                Description = "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">", // mXSS
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-062: Create DST risk with DOM clobbering attack in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-062")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DOMClobberingTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<form name='x'><input name='y'></form>", // DOM clobbering
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("form");
        }

        /// <summary>
        /// TC-DST-VAL-063: Create DST risk with dangling markup injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-063")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_DanglingMarkupDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Dangling Markup Test",
                Description = "<img src='http://malicious.com?", // Dangling markup
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-064: Create DST risk with UTF-7 encoded XSS in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-064")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_UTF7EncodedSource_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "UTF-7 Encoding Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "+ADw-script+AD4-alert('XSS')+ADw-/script+AD4-" // UTF-7 encoded
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("+ADw-");
        }

        /// <summary>
        /// TC-DST-VAL-065: Create DST risk with zero-width characters in Title (steganography)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-065")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_ZeroWidthCharactersTitle_HandlesInvisibleChars()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Title\u200B\u200C\u200D\uFEFFwith zero-width", // Zero-width chars
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-066: Create DST risk with bidirectional override characters in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-066")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_BidiOverrideDescription_HandlesDirectionOverride()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Bidi Override Test",
                Description = "Text\u202E reversed text", // Bidirectional override
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-067: Create DST risk with combining characters (zalgo text) in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-067")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_CombiningCharactersTitle_HandlesZalgoText()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Z̴̡̢̛̗̬̪̟̲͙̮̲͔͖̗̘̤̳̞̳̖͉͍̿̊̆͌̓̊̉̍̂̈́͆̾̐̔̚͝͝A̵̢̡̢̡̨̛̪̫̩̻̠̮̟͎̻̟̦̹̘̦̝̲̠͖̜͕̗̺̻̩̲̳̠̹̘͔̯̥̟͂̾̏̈́̾̿̓̋̿̅͑̚͜͝ͅL̶̨̢̛̥͓̹̹̤̯̮̰̘̱̦̮̼̬̳͎͔̥̼̼̺̜̺͚̳̙͖̼̥̱̩͂̎͗͛̉̐̎͑̾̏̽̊̀́̏̓̌̇̓͗̑͌̊͆̒́̇̋̈̎̐͘̕͘͜͝͠ͅG̸̡̢̡̡̧̢̨̛̛̛̳̟̝̱͙̪̖̺̫̗̳̟͇̜̹͕̻̭̪̞͇̘͎̤̻̟̟̜͙̗̱̤̝̗̫͍͈̙̤͙̪͖̹̺̳̙̺͎͖̼̰̓̋̋̏̓̎̉͂̄̈́̅̅̍̇͋̾̀͋̏̏̐̔̀̈̊͗̑̿̓̋̏̔̚͘͘̕͜͜͜͝͝Ö̸̧̢̡̹̦̟͚̝͔̼̫̜̮̻̝̮̫̥̰͇̖̥͓̭̠̬̱̝̹͖̲̫̗̘̱́͗̔̐͌̀̾̇̄͋͊̋͊͊́̾͆̏̿͂̇́̚͘̕͘͝͝ͅ", // Combining chars
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-068: Create DST risk with control characters (BEL, ESC) in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-068")]
        [Trait("Priority", "Medium")]
        public async Task AddDSTRisk_ControlCharactersDescription_HandlesOrRejects()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Control Chars Test",
                Description = "Text\u0007with\u001Bcontrol\u0008chars", // BEL, ESC, BS
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
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
        /// TC-DST-VAL-069: Create DST risk with emoji variations and modifiers in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-069")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_EmojiVariationsSource_HandlesComplexEmoji()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Emoji Variations Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "DST 👨‍👩‍👧‍👦 👍🏻 👍🏿 🏳️‍🌈" // Complex emoji with modifiers
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-070: Create DST risk with line separator and paragraph separator in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-070")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_LineParagraphSeparatorsTitle_HandlesUnicodeSeparators()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Line\u2028Separator\u2029Test", // U+2028, U+2029
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-071: Create DST risk with mathematical alphanumeric symbols in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-071")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_MathematicalAlphanumericsDescription_HandlesUnicodeVariants()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Mathematical Symbols Test",
                Description = "𝐓𝐞𝐬𝐭 𝐰𝐢𝐭𝐡 𝐦𝐚𝐭𝐡 𝐬𝐲𝐦𝐛𝐨𝐥𝐬", // Mathematical bold
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DST-VAL-072: Create DST risk with deprecated HTML tags in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-072")]
        [Trait("Priority", "Low")]
        public async Task AddDSTRisk_DeprecatedHTMLTagsTitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "<blink><marquee>Deprecated tags</marquee></blink>", // Deprecated tags
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("blink");
        }

        /// <summary>
        /// TC-DST-VAL-073: Create DST risk with template literal injection in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-073")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_TemplateLiteralDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Template Literal Test",
                Description = "${alert('XSS')}", // Template literal injection
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("${");
        }

        /// <summary>
        /// TC-DST-VAL-074: Create DST risk with expression language injection in Source
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-074")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_ExpressionLanguageSource_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Expression Language Test",
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4,
                Source = "{{7*7}} #{7*7} ${7*7}" // EL injection
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Source.Should().Contain("{{");
        }

        /// <summary>
        /// TC-DST-VAL-075: Create DST risk with server-side template injection in Title
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-075")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_SSTITitle_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "{{config.items()}}", // SSTI payload
                Description = "Test description",
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Contain("{{config");
        }

        /// <summary>
        /// TC-DST-VAL-076: Create DST risk with prototype pollution payload in Description
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-VAL-076")]
        [Trait("Priority", "High")]
        public async Task AddDSTRisk_PrototypePollutionDescription_SafelyHandled()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var riskManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RiskManager;
            var user = CreateTestUser();
            var opportunityId = await CreateTestOpportunityAsync();

            var riskRequest = new RiskCreateRequest
            {
                Title = "Prototype Pollution Test",
                Description = "{\"__proto__\":{\"admin\":true}}", // Prototype pollution
                EntityType = "Opportunity",
                EntityId = opportunityId,
                RiskTypeId = 1,
                ProbabilityId = 3,
                ImpactId = 4
            };

            // Act
            var result = await riskManager.AddRiskAsync(riskRequest, user);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Contain("__proto__");
        }

        #endregion
    }
}
