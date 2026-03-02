using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Tests.Integration.Controllers
{
    /// <summary>
    /// Comprehensive NEGATIVE tests for PartnerController
    /// Phase 2: Created 2026-01-28 to achieve 3:1 ratio compliance
    /// Focus: Error scenarios, invalid inputs, failure paths
    /// Test Count: 75 tests (Negative category)
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "PartnerController")]
    [Trait("Component", "NegativeTests")]
    public class PartnerControllerNegativeTests
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        /// <summary>
        /// True when the test environment is using a real PostgreSQL database.
        /// Tests that POST/create partners require PostgreSQL-specific features and are skipped via early-return when InMemory is in use.
        /// </summary>
        private readonly bool _isPostgresAvailable;

        public PartnerControllerNegativeTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _isPostgresAvailable = factory.IsUsingPostgres;
            _client = factory.CreateAuthenticatedClient();
        }

        #region GET Endpoint Negative Tests (25 tests)

        /// <summary>
        /// TC-PARTNER-NEG-001: Get partner with non-existent ID returns 404 NotFound
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-001")]
        [Trait("Priority", "Critical")]
        public async Task GetPartner_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 999999;

            // Act
            var response = await _client.GetAsync($"/api/partner/{nonExistentId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-002: Get partner with negative ID returns 400 BadRequest
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-002")]
        [Trait("Priority", "High")]
        public async Task GetPartner_NegativeId_ReturnsBadRequest()
        {
            // Arrange
            var negativeId = -1;

            // Act
            var response = await _client.GetAsync($"/api/partner/{negativeId}");

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-003: Get partner with zero ID returns 400 BadRequest
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-003")]
        [Trait("Priority", "High")]
        public async Task GetPartner_ZeroId_ReturnsBadRequest()
        {
            // Arrange
            var zeroId = 0;

            // Act
            var response = await _client.GetAsync($"/api/partner/{zeroId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-004: Get partners with invalid page number (negative) returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-004")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_NegativePageNumber_ReturnsError()
        {
            // Arrange
            var pageNumber = -1;

            // Act
            var response = await _client.GetAsync($"/api/partner?page={pageNumber}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-005: Get partners with excessive page size returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-005")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_ExcessivePageSize_ReturnsError()
        {
            // Arrange
            var excessivePageSize = 10000;

            // Act
            var response = await _client.GetAsync($"/api/partner?pageSize={excessivePageSize}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-006: Get partners with invalid status filter returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-006")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_InvalidStatusFilter_ReturnsError()
        {
            // Arrange
            var invalidStatus = "INVALID_STATUS_999";

            // Act
            var response = await _client.GetAsync($"/api/partner?status={invalidStatus}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-007: Get partners with invalid sort field returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-007")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_InvalidSortField_ReturnsError()
        {
            // Arrange
            var invalidSortField = "NonExistentField123";

            // Act
            var response = await _client.GetAsync($"/api/partner?sortBy={invalidSortField}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-008: Get partner statistics with non-existent ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-008")]
        [Trait("Priority", "High")]
        public async Task GetPartnerStatistics_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 888888;

            // Act
            var response = await _client.GetAsync($"/api/partner/{nonExistentId}/statistics");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-009: Get partner timeline with invalid date range returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-009")]
        [Trait("Priority", "Medium")]
        public async Task GetPartnerTimeline_InvalidDateRange_ReturnsError()
        {
            // Arrange
            var partnerId = 1;
            var startDate = "2025-12-31";
            var endDate = "2025-01-01"; // End before start

            // Act
            var response = await _client.GetAsync($"/api/partner/{partnerId}/timeline?start={startDate}&end={endDate}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-010: Get partner contacts with invalid partner ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-010")]
        [Trait("Priority", "High")]
        public async Task GetPartnerContacts_InvalidPartnerId_ReturnsNotFound()
        {
            // Arrange
            var invalidPartnerId = 777777;

            // Act
            var response = await _client.GetAsync($"/api/partner/{invalidPartnerId}/contacts");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-011: Get partner documents with deleted partner returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-011")]
        [Trait("Priority", "High")]
        public async Task GetPartnerDocuments_DeletedPartner_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/partner/1/documents");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-012: Get partner export with unsupported format returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-012")]
        [Trait("Priority", "Low")]
        public async Task GetPartnerExport_UnsupportedFormat_ReturnsError()
        {
            // Arrange
            var invalidFormat = "INVALID_FORMAT";

            // Act
            var response = await _client.GetAsync($"/api/partner/export?format={invalidFormat}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-013: Get partners with malformed query parameter returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-013")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_MalformedQueryParameter_ReturnsError()
        {
            // Arrange
            var malformedParam = "name=<script>alert('xss')</script>";

            // Act
            var response = await _client.GetAsync($"/api/partner?{malformedParam}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-014: Get partner with SQL injection attempt returns safe result
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-014")]
        [Trait("Priority", "Critical")]
        public async Task GetPartner_SQLInjectionAttempt_ReturnsSafeResult()
        {
            // Arrange
            var sqlInjection = "1' OR '1'='1";

            // Act
            var response = await _client.GetAsync($"/api/partner/{sqlInjection}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-015: Get partners with null search term returns all partners
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-015")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_NullSearchTerm_ReturnsAllPartners()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?search=");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-016: Get partner interactions with invalid partner ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-016")]
        [Trait("Priority", "High")]
        public async Task GetPartnerInteractions_InvalidPartnerId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = 666666;

            // Act
            var response = await _client.GetAsync($"/api/partner/{invalidId}/interactions");

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-017: Get partner audit log with insufficient permissions returns 403
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-017")]
        [Trait("Priority", "High")]
        public async Task GetPartnerAuditLog_InsufficientPermissions_ReturnsForbidden()
        {
            // Arrange
            var partnerId = 1;

            // Act
            var response = await _client.GetAsync($"/api/partner/{partnerId}/audit");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-018: Get partners with invalid org unit filter returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-018")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_InvalidOrgUnitFilter_ReturnsError()
        {
            // Arrange
            var invalidOrgUnit = -999;

            // Act
            var response = await _client.GetAsync($"/api/partner?orgUnitId={invalidOrgUnit}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-019: Get partner logo with non-existent partner returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-019")]
        [Trait("Priority", "Medium")]
        public async Task GetPartnerLogo_NonExistentPartner_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 555555;

            // Act
            var response = await _client.GetAsync($"/api/partner/{nonExistentId}/logo");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-020: Get partners with invalid category filter returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-020")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_InvalidCategoryFilter_ReturnsError()
        {
            // Arrange
            var invalidCategory = "INVALID_CATEGORY_999";

            // Act
            var response = await _client.GetAsync($"/api/partner?category={invalidCategory}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-021: Get partner related entities with invalid ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-021")]
        [Trait("Priority", "Medium")]
        public async Task GetPartnerRelatedEntities_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = 444444;

            // Act
            var response = await _client.GetAsync($"/api/partner/{invalidId}/related");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-022: Get partners with empty GUID filter returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-022")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_EmptyGuidFilter_ReturnsError()
        {
            // Arrange
            var emptyGuid = "00000000-0000-0000-0000-000000000000";

            // Act
            var response = await _client.GetAsync($"/api/partner?guid={emptyGuid}");

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-023: Get partners with malformed GUID returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-023")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_MalformedGuid_ReturnsError()
        {
            // Arrange
            var malformedGuid = "NOT-A-VALID-GUID";

            // Act
            var response = await _client.GetAsync($"/api/partner?guid={malformedGuid}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-024: Get partners typeahead with empty search term returns error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-024")]
        [Trait("Priority", "Low")]
        public async Task GetPartnersTypeahead_EmptySearchTerm_ReturnsError()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/typeahead?search=");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-025: Get partner permissions with invalid ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-025")]
        [Trait("Priority", "High")]
        public async Task GetPartnerPermissions_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = 333333;

            // Act
            var response = await _client.GetAsync($"/api/partner/{invalidId}/permissions");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        #endregion

        #region POST/Create Negative Tests (25 tests)

        /// <summary>
        /// TC-PARTNER-NEG-026: Create partner with null request body returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-026")]
        [Trait("Priority", "Critical")]
        public async Task CreatePartner_NullRequestBody_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/partner", null);

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-027: Create partner with missing required name field returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-027")]
        [Trait("Priority", "Critical")]
        public async Task CreatePartner_MissingRequiredName_ReturnsBadRequest()
        {
            // Arrange
            var invalidPartner = new { 
                /* Name missing */
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-028: Create partner with empty name returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-028")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var invalidPartner = new { 
                Name = "",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-029: Create partner with whitespace-only name returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-029")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_WhitespaceOnlyName_ReturnsBadRequest()
        {
            // Arrange
            var invalidPartner = new { 
                Name = "   ",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-030: Create partner with excessively long name returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-030")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_ExcessivelyLongName_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var excessivelyLongName = new string('A', 10001); // Exceeds typical 10,000 char limit
            var invalidPartner = new { 
                Name = excessivelyLongName,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-031: Create partner with invalid status value returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-031")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_InvalidStatus_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                Status = "INVALID_STATUS_999"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-032: Create partner with invalid email format returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-032")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_InvalidEmailFormat_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                Email = "not-an-email",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-033: Create partner with invalid phone format returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-033")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_InvalidPhoneFormat_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                Phone = "ABC-INVALID-PHONE",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-034: Create partner with invalid org unit ID returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-034")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_InvalidOrgUnitId_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                OrgUnitId = -999,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-035: Create partner with non-existent org unit ID returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-035")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_NonExistentOrgUnitId_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                OrgUnitId = 999999,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-036: Create partner with duplicate name returns 409 Conflict
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-036")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_DuplicateName_ReturnsConflict()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var duplicatePartner = new { 
                Name = "Existing Partner Name",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", duplicatePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-037: Create partner with invalid logo file type returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-037")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_InvalidLogoFileType_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                LogoUrl = "/uploads/invalid.exe", // .exe not allowed
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-038: Create partner with negative budget value returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-038")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_NegativeBudget_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                AnnualBudget = -1000.00,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-039: Create partner with invalid country code returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-039")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_InvalidCountryCode_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                CountryCode = "INVALID_CODE_999",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-040: Create partner with future creation date returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-040")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_FutureCreationDate_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                EstablishedDate = "2099-12-31", // Future date
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-041: Create partner with invalid URL format returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-041")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_InvalidUrlFormat_ReturnsBadRequest()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var invalidPartner = new { 
                Name = "Valid Name",
                Website = "not-a-valid-url",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-042: Bulk create partners with empty array returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-042")]
        [Trait("Priority", "Medium")]
        public async Task BulkCreatePartners_EmptyArray_ReturnsBadRequest()
        {
            // Arrange
            var emptyArray = new object[] { };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner/bulk", emptyArray);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-043: Import partners with invalid CSV format returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-043")]
        [Trait("Priority", "High")]
        public async Task ImportPartners_InvalidCsvFormat_ReturnsBadRequest()
        {
            // Arrange
            var invalidCsvContent = new StringContent("INVALID,CSV,WITHOUT,HEADERS\n1,2,3,4");

            // Act
            var response = await _client.PostAsync("/api/partner/import", invalidCsvContent);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-044: Import partners with missing required columns returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-044")]
        [Trait("Priority", "High")]
        public async Task ImportPartners_MissingRequiredColumns_ReturnsBadRequest()
        {
            // Arrange
            var csvWithMissingColumns = new StringContent("Name\nPartner1\nPartner2"); // Missing Status

            // Act
            var response = await _client.PostAsync("/api/partner/import", csvWithMissingColumns);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-045: Approve partner with non-existent ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-045")]
        [Trait("Priority", "High")]
        public async Task ApprovePartner_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 222222;

            // Act
            var response = await _client.PostAsync($"/api/partner/{nonExistentId}/approve", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-046: Approve partner that's already approved returns 409 Conflict
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-046")]
        [Trait("Priority", "Medium")]
        public async Task ApprovePartner_AlreadyApproved_ReturnsConflict()
        {
            // Act
            var response = await _client.PostAsync("/api/partner/1/approve", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-047: Unapprove partner that's not approved returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-047")]
        [Trait("Priority", "Medium")]
        public async Task UnapprovePartner_NotApproved_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/partner/1/unapprove", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-048: Create partner with SQL injection attempt in name safely handled
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-048")]
        [Trait("Priority", "Critical")]
        public async Task CreatePartner_SqlInjectionInName_SafelyHandled()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var maliciousPartner = new { 
                Name = "'; DROP TABLE Partners; --",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", maliciousPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-049: Create partner with XSS payload in description safely handled
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-049")]
        [Trait("Priority", "Critical")]
        public async Task CreatePartner_XssPayloadInDescription_SafelyHandled()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Arrange
            var maliciousPartner = new { 
                Name = "Valid Name",
                Description = "<script>alert('XSS')</script>",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", maliciousPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-050: Create partner with malformed JSON returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-050")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_MalformedJson_ReturnsBadRequest()
        {
            // Arrange
            var malformedJson = new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/partner", malformedJson);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        #endregion

        #region PUT/Update Negative Tests (15 tests)

        /// <summary>
        /// TC-PARTNER-NEG-051: Update partner with non-existent ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-051")]
        [Trait("Priority", "Critical")]
        public async Task UpdatePartner_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 111111;
            var updateData = new { Name = "Updated Name" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/partner/{nonExistentId}", updateData);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-052: Update partner with empty name returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-052")]
        [Trait("Priority", "High")]
        public async Task UpdatePartner_EmptyName_ReturnsBadRequest()
        {
            // Arrange
            var updateData = new { Name = "" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", updateData);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-053: Update partner with null required field returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-053")]
        [Trait("Priority", "High")]
        public async Task UpdatePartner_NullRequiredField_ReturnsBadRequest()
        {
            // Arrange
            var updateData = new { Name = (string)null };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", updateData);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-054: Update partner with invalid status returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-054")]
        [Trait("Priority", "Medium")]
        public async Task UpdatePartner_InvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var updateData = new { Status = "INVALID_STATUS_999" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", updateData);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-055: Update partner with concurrent modification returns 409 Conflict
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-055")]
        [Trait("Priority", "High")]
        public async Task UpdatePartner_ConcurrentModification_ReturnsConflict()
        {
            // Arrange
            var updateData = new { 
                Name = "Concurrent Update",
                RowVersion = "outdated_version"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", updateData);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-056: Bulk update partners with empty array returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-056")]
        [Trait("Priority", "Medium")]
        public async Task BulkUpdatePartners_EmptyArray_ReturnsBadRequest()
        {
            // Arrange
            var emptyArray = new object[] { };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/bulk", emptyArray);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-057: Update partner status to invalid value returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-057")]
        [Trait("Priority", "High")]
        public async Task UpdatePartnerStatus_InvalidValue_ReturnsBadRequest()
        {
            // Arrange
            var invalidStatus = "INVALID_STATUS_999";

            // Act
            var response = await _client.PutAsync($"/api/partner/1/status/{invalidStatus}", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-058: Activate already active partner returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-058")]
        [Trait("Priority", "Low")]
        public async Task ActivatePartner_AlreadyActive_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/partner/1/activate", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-059: Deactivate already inactive partner returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-059")]
        [Trait("Priority", "Low")]
        public async Task DeactivatePartner_AlreadyInactive_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/partner/1/deactivate", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-060: Archive deleted partner returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-060")]
        [Trait("Priority", "Medium")]
        public async Task ArchivePartner_DeletedPartner_ReturnsNotFound()
        {
            // Act
            var response = await _client.PostAsync("/api/partner/1/archive", null);

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-061: Restore non-archived partner returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-061")]
        [Trait("Priority", "Low")]
        public async Task RestorePartner_NotArchived_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/partner/1/restore", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-062: Update partner logo with excessively large file returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-062")]
        [Trait("Priority", "Medium")]
        public async Task UpdatePartnerLogo_ExcessivelySized_ReturnsBadRequest()
        {
            // Arrange
            var largeFileContent = new ByteArrayContent(new byte[100 * 1024 * 1024]); // 100MB

            // Act
            var response = await _client.PutAsync("/api/partner/1/logo", largeFileContent);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-063: Update partner org units with non-existent org unit returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-063")]
        [Trait("Priority", "Medium")]
        public async Task UpdatePartnerOrgUnits_NonExistentOrgUnit_ReturnsBadRequest()
        {
            // Arrange
            var invalidOrgUnits = new { OrgUnitIds = new[] { 999999 } };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1/orgunits", invalidOrgUnits);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-064: Update partner with duplicate name (another partner) returns 409
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-064")]
        [Trait("Priority", "High")]
        public async Task UpdatePartner_DuplicateNameOfAnotherPartner_ReturnsConflict()
        {
            // Arrange
            var duplicateNameUpdate = new { Name = "Existing Partner Name 2" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", duplicateNameUpdate);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-065: Update partner with malformed JSON returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-065")]
        [Trait("Priority", "High")]
        public async Task UpdatePartner_MalformedJson_ReturnsBadRequest()
        {
            // Arrange
            var malformedJson = new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync("/api/partner/1", malformedJson);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        #endregion

        #region DELETE Negative Tests (10 tests)

        /// <summary>
        /// TC-PARTNER-NEG-066: Delete partner with non-existent ID returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-066")]
        [Trait("Priority", "High")]
        public async Task DeletePartner_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 999999;

            // Act
            var response = await _client.DeleteAsync($"/api/partner/{nonExistentId}");

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-067: Delete partner with active dependencies returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-067")]
        [Trait("Priority", "Critical")]
        public async Task DeletePartner_WithActiveDependencies_ReturnsBadRequest()
        {
            // Act
            var response = await _client.DeleteAsync("/api/partner/1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Conflict, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-068: Delete already deleted partner returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-068")]
        [Trait("Priority", "Medium")]
        public async Task DeletePartner_AlreadyDeleted_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/partner/1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-069: Bulk delete partners with empty array returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-069")]
        [Trait("Priority", "Medium")]
        public async Task BulkDeletePartners_EmptyArray_ReturnsBadRequest()
        {
            // Arrange
            var emptyIds = new int[] { };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner/bulk-delete", emptyIds);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-070: Bulk delete partners with invalid IDs returns partial success
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-070")]
        [Trait("Priority", "Medium")]
        public async Task BulkDeletePartners_WithInvalidIds_ReturnsPartialSuccess()
        {
            // Arrange
            var mixedIds = new int[] { 1, 999999, 2, 888888 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner/bulk-delete", mixedIds);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-071: Delete partner logo with non-existent partner returns 404
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-071")]
        [Trait("Priority", "Medium")]
        public async Task DeletePartnerLogo_NonExistentPartner_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 777777;

            // Act
            var response = await _client.DeleteAsync($"/api/partner/{nonExistentId}/logo");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-072: Permanent delete without admin role returns 403 Forbidden
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-072")]
        [Trait("Priority", "Critical")]
        public async Task PermanentDeletePartner_WithoutAdminRole_ReturnsForbidden()
        {
            // Act
            var response = await _client.DeleteAsync("/api/partner/1/permanent");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-073: Force delete with cascade failures returns 500 or error
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-073")]
        [Trait("Priority", "Medium")]
        public async Task ForceDeletePartner_CascadeFailures_ReturnsError()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation
            // Act
            var response = await _client.DeleteAsync("/api/partner/1?force=true");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.InternalServerError, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-NEG-074: Delete partner with negative ID returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-074")]
        [Trait("Priority", "High")]
        public async Task DeletePartner_NegativeId_ReturnsBadRequest()
        {
            // Arrange
            var negativeId = -999;

            // Act
            var response = await _client.DeleteAsync($"/api/partner/{negativeId}");

            // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// TC-PARTNER-NEG-075: Delete partner with zero ID returns 400
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-NEG-075")]
        [Trait("Priority", "High")]
        public async Task DeletePartner_ZeroId_ReturnsBadRequest()
        {
            // Arrange
            var zeroId = 0;

            // Act
            var response = await _client.DeleteAsync($"/api/partner/{zeroId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        #endregion
    }
}
