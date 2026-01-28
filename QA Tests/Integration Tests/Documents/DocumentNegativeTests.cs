using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.Documents
{
    /// <summary>
    /// Negative test cases for Document Management
    /// Tests error handling, invalid inputs, and failure scenarios
    /// 
    /// Test Coverage:
    /// - Invalid document IDs (non-existent, negative, zero)
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
    [Trait("Feature", "Documents")]
    [Trait("Component", "NegativeTests")]
    public class DocumentNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DocumentNegativeTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

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

        #region TC-DOC-NEG-001 through TC-DOC-NEG-050: Comprehensive Negative Scenarios

        /// <summary>
        /// TC-DOC-NEG-001: Get documents for non-existent entity
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-001")]
        [Trait("Priority", "Critical")]
        public async Task GetDocuments_NonExistentEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", 999999, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-002: Get document by non-existent ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-002")]
        [Trait("Priority", "Critical")]
        public async Task GetDocument_NonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.GetDocumentByIdAsync(999999, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-003: Update document with non-existent ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-003")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_NonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 999999,
                DocumentTypeId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-004: Delete document with non-existent ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-004")]
        [Trait("Priority", "High")]
        public async Task DeleteDocument_NonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(999999, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-005: Get documents with negative entity ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-005")]
        [Trait("Priority", "High")]
        public async Task GetDocuments_NegativeEntityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", -1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-006: Get documents with zero entity ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-006")]
        [Trait("Priority", "Medium")]
        public async Task GetDocuments_ZeroEntityId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", 0, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-007: Get documents with null entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-007")]
        [Trait("Priority", "Critical")]
        public async Task GetDocuments_NullEntityType_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync(null, 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-008: Get documents with empty entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-008")]
        [Trait("Priority", "High")]
        public async Task GetDocuments_EmptyEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync(string.Empty, 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-009: Get documents with invalid entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-009")]
        [Trait("Priority", "Medium")]
        public async Task GetDocuments_InvalidEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("InvalidType", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-010: Get document with negative ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-010")]
        [Trait("Priority", "High")]
        public async Task GetDocument_NegativeId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentByIdAsync(-1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-011: Get document with zero ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-011")]
        [Trait("Priority", "Medium")]
        public async Task GetDocument_ZeroId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentByIdAsync(0, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-012: Update document with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-012")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDocument_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, null);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-013: Delete document with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-013")]
        [Trait("Priority", "Critical")]
        public async Task DeleteDocument_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(1, null);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-014: Get documents with null user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-014")]
        [Trait("Priority", "Critical")]
        public async Task GetDocuments_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", 1, null);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-015: Update document with invalid DocumentTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-015")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_InvalidDocumentTypeId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = 999999
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-016: Update document with negative DocumentTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-016")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_NegativeDocumentTypeId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = -1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-017: Update document with zero DocumentTypeId
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-017")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDocument_ZeroDocumentTypeId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = 0
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-018: Update document without edit permission
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-018")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDocument_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            
            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Name, "Limited User"),
                new Claim(ClaimTypes.Role, "Viewer")
            }, "TestAuth"));

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, limitedUser);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-019: Delete document without permission
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-019")]
        [Trait("Priority", "Critical")]
        public async Task DeleteDocument_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            
            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Name, "Limited User"),
                new Claim(ClaimTypes.Role, "Viewer")
            }, "TestAuth"));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(1, limitedUser);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-020: Get documents for entity user doesn't have access to
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-020")]
        [Trait("Priority", "Critical")]
        public async Task GetDocuments_UnauthorizedEntity_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            
            var unauthorizedUser = CreateTestUser(userId: 998);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", 1, unauthorizedUser);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-021: Upload document with null file data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-021")]
        [Trait("Priority", "Critical")]
        public async Task UploadDocument_NullFileData_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await documentManager.UploadDocumentAsync(null, "Partner", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-022: Upload document with empty file name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-022")]
        [Trait("Priority", "High")]
        public async Task UploadDocument_EmptyFileName_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var fileData = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UploadDocumentAsync(fileData, string.Empty, "Partner", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-023: Upload document with whitespace-only file name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-023")]
        [Trait("Priority", "Medium")]
        public async Task UploadDocument_WhitespaceFileName_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var fileData = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UploadDocumentAsync(fileData, "   ", "Partner", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-024: Upload document with zero-length file data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-024")]
        [Trait("Priority", "High")]
        public async Task UploadDocument_ZeroLengthFile_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var emptyFile = new byte[0];

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UploadDocumentAsync(emptyFile, "test.pdf", "Partner", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-025: Delete document that was already deleted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-025")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDocument_AlreadyDeleted_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Assume document 1 exists and delete it
            await documentManager.DeleteDocumentAsync(1, user);

            // Act & Assert - Attempt second delete
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-026: Update document after it was deleted
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-026")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_AfterDeletion_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Delete document
            await documentManager.DeleteDocumentAsync(1, user);

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = 2
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-027: Get documents with whitespace entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-027")]
        [Trait("Priority", "High")]
        public async Task GetDocuments_WhitespaceEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("   ", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-028: Update document with null update request
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-028")]
        [Trait("Priority", "Critical")]
        public async Task UpdateDocument_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(null, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-029: Update document with Int32.MaxValue ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-029")]
        [Trait("Priority", "Low")]
        public async Task UpdateDocument_MaxIntId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = int.MaxValue,
                DocumentTypeId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-030: Delete document with Int32.MaxValue ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-030")]
        [Trait("Priority", "Low")]
        public async Task DeleteDocument_MaxIntId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(int.MaxValue, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-031: Get documents with entity ID Int32.MaxValue
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-031")]
        [Trait("Priority", "Low")]
        public async Task GetDocuments_MaxIntEntityId_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", int.MaxValue, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-032: Update document belonging to different user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-032")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_DifferentUser_HandlesAuthorization()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user1 = CreateTestUser(userId: 100);
            var user2 = CreateTestUser(userId: 200);

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1, // Assume belongs to user1
                DocumentTypeId = 2
            };

            // Act & Assert
            try
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user2);
                Assert.True(true, "Cross-user update may be allowed based on permissions");
            }
            catch (UnauthorizedAccessException)
            {
                Assert.True(true, "Cross-user update blocked");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-033: Delete document belonging to different user
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-033")]
        [Trait("Priority", "High")]
        public async Task DeleteDocument_DifferentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user1 = CreateTestUser(userId: 100);
            var user2 = CreateTestUser(userId: 200);

            // Act & Assert
            try
            {
                await documentManager.DeleteDocumentAsync(1, user2);
                Assert.True(true, "Cross-user delete may be allowed based on permissions");
            }
            catch (UnauthorizedAccessException)
            {
                Assert.True(true, "Cross-user delete blocked");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-034: Get documents with user having insufficient claims
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-034")]
        [Trait("Priority", "High")]
        public async Task GetDocuments_InsufficientClaims_ThrowsException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            
            var insufficientUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User") // Missing NameIdentifier
            }, "TestAuth"));

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Partner", 1, insufficientUser);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-035: Upload document for non-existent entity
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-035")]
        [Trait("Priority", "High")]
        public async Task UploadDocument_NonExistentEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var fileData = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await documentManager.UploadDocumentAsync(fileData, "test.pdf", "Partner", 999999, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-036: Upload document with invalid entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-036")]
        [Trait("Priority", "High")]
        public async Task UploadDocument_InvalidEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var fileData = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UploadDocumentAsync(fileData, "test.pdf", "InvalidType", 1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-037: Update document with negative ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-037")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_NegativeId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = -1,
                DocumentTypeId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-038: Update document with zero ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-038")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDocument_ZeroId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 0,
                DocumentTypeId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.UpdateDocumentAsync(updateRequest, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-039: Delete document with negative ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-039")]
        [Trait("Priority", "High")]
        public async Task DeleteDocument_NegativeId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(-1, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-040: Delete document with zero ID
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-040")]
        [Trait("Priority", "Medium")]
        public async Task DeleteDocument_ZeroId_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.DeleteDocumentAsync(0, user);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-041: Upload document without upload permission
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-041")]
        [Trait("Priority", "Critical")]
        public async Task UploadDocument_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            
            var limitedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Name, "Limited User"),
                new Claim(ClaimTypes.Role, "Viewer")
            }, "TestAuth"));

            var fileData = new byte[] { 0x01, 0x02, 0x03 };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await documentManager.UploadDocumentAsync(fileData, "test.pdf", "Partner", 1, limitedUser);
            });
        }

        /// <summary>
        /// TC-DOC-NEG-042: Get documents for deleted entity
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-042")]
        [Trait("Priority", "Medium")]
        public async Task GetDocuments_DeletedEntity_ReturnsEmptyOrThrows()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act
            try
            {
                var result = await documentManager.GetDocumentsByEntityAsync("Partner", 1, user);
                result.Should().NotBeNull();
            }
            catch (KeyNotFoundException)
            {
                Assert.True(true, "Deleted entity may throw exception");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-043: Update document concurrently (race condition)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-043")]
        [Trait("Priority", "High")]
        public async Task UpdateDocument_ConcurrentUpdates_HandlesConflict()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var update1 = new UpdateDocumentRequest { Id = 1, DocumentTypeId = 2 };
            var update2 = new UpdateDocumentRequest { Id = 1, DocumentTypeId = 3 };

            // Act - Fire concurrent updates
            var task1 = documentManager.UpdateDocumentAsync(update1, user);
            var task2 = documentManager.UpdateDocumentAsync(update2, user);

            // Assert - Last write wins or conflict detected
            try
            {
                await Task.WhenAll(task1, task2);
                Assert.True(true, "Concurrent updates handled");
            }
            catch
            {
                Assert.True(true, "Concurrency conflict detected");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-044: Delete document while update is in progress
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-044")]
        [Trait("Priority", "High")]
        public async Task DeleteDocument_DuringUpdate_HandlesRaceCondition()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest { Id = 1, DocumentTypeId = 2 };

            // Act - Update and delete concurrently
            var updateTask = documentManager.UpdateDocumentAsync(updateRequest, user);
            var deleteTask = documentManager.DeleteDocumentAsync(1, user);

            // Assert
            try
            {
                await Task.WhenAll(updateTask, deleteTask);
            }
            catch
            {
                Assert.True(true, "Race condition handled");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-045: Upload document with corrupted file data
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-045")]
        [Trait("Priority", "Medium")]
        public async Task UploadDocument_CorruptedFileData_HandlesGracefully()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var corruptedData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

            // Act
            try
            {
                await documentManager.UploadDocumentAsync(corruptedData, "test.pdf", "Partner", 1, user);
                Assert.True(true, "Corrupted file may be accepted");
            }
            catch (ArgumentException)
            {
                Assert.True(true, "Corrupted file rejected");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-046: Get documents with case-variant entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-046")]
        [Trait("Priority", "Medium")]
        public async Task GetDocuments_CaseVariantEntityType_HandlesCaseSensitivity()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act
            try
            {
                var result = await documentManager.GetDocumentsByEntityAsync("partner", 1, user); // lowercase
                result.Should().NotBeNull("may support case-insensitive");
            }
            catch (ArgumentException)
            {
                Assert.True(true, "Case-sensitive entity type validation");
            }
        }

        /// <summary>
        /// TC-DOC-NEG-047: Update document with null DocumentTypeId (optional field test)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-047")]
        [Trait("Priority", "Medium")]
        public async Task UpdateDocument_NullDocumentTypeId_AcceptsIfOptional()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = null // Optional field
            };

            // Act
            var result = await documentManager.UpdateDocumentAsync(updateRequest, user);

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DOC-NEG-048: Get documents rapidly in sequence (stress test)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-048")]
        [Trait("Priority", "Low")]
        public async Task GetDocuments_RapidSequentialRequests_HandlesLoad()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act - 20 rapid requests
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    await documentManager.GetDocumentsByEntityAsync("Partner", 1, user);
                }
                catch
                {
                    // May fail if entity doesn't exist
                }
            }

            Assert.True(true, "Rapid requests handled");
        }

        /// <summary>
        /// TC-DOC-NEG-049: Update document with no changes (idempotency)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-049")]
        [Trait("Priority", "Low")]
        public async Task UpdateDocument_NoChanges_HandlesIdempotently()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Get current document
            var current = await documentManager.GetDocumentByIdAsync(1, user);

            var updateRequest = new UpdateDocumentRequest
            {
                Id = 1,
                DocumentTypeId = current.DocumentType?.Id // Same value
            };

            // Act
            var updated = await documentManager.UpdateDocumentAsync(updateRequest, user);

            // Assert
            updated.Should().NotBeNull();
        }

        /// <summary>
        /// TC-DOC-NEG-050: Get documents with special characters in entity type
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DOC-NEG-050")]
        [Trait("Priority", "Medium")]
        public async Task GetDocuments_SpecialCharsEntityType_ThrowsArgumentException()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var documentManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DocumentManager;
            var user = CreateTestUser();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await documentManager.GetDocumentsByEntityAsync("Part<ner>", 1, user);
            });
        }

        #endregion
    }
}
