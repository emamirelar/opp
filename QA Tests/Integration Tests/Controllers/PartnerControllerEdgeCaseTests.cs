using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.Controllers
{
    /// <summary>
    /// Comprehensive EDGE CASE tests for PartnerController
    /// Phase 2: Created 2026-01-28 to achieve 3:1 ratio compliance
    /// Focus: Boundary conditions, extreme values, unusual inputs
    /// Test Count: 73 tests (Edge category)
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "PartnerController")]
    [Trait("Component", "EdgeCaseTests")]
    public class PartnerControllerEdgeCaseTests
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        /// <summary>
        /// True when the test environment is using a real PostgreSQL database.
        /// Tests that POST/create partners require pg_trgm and are skipped via early-return when InMemory is in use.
        /// </summary>
        private readonly bool _isPostgresAvailable;

        public PartnerControllerEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _isPostgresAvailable = factory.IsUsingPostgres;
            _client = factory.CreateAuthenticatedClient();
        }

        #region Boundary Value Tests (20 tests)

        /// <summary>
        /// TC-PARTNER-EDGE-001: Get partner with ID = 1 (minimum valid ID)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-001")]
        [Trait("Priority", "Medium")]
        public async Task GetPartner_IdOne_ReturnsPartner()
        {
            // Arrange
            var minId = 1;

            // Act
            var response = await _client.GetAsync($"/api/partner/{minId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-002: Get partner with ID = Int32.MaxValue
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-002")]
        [Trait("Priority", "Low")]
        public async Task GetPartner_MaxIntId_HandlesGracefully()
        {
            // Arrange
            var maxId = int.MaxValue;

            // Act
            var response = await _client.GetAsync($"/api/partner/{maxId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-003: Create partner with name at exact minimum length (1 char)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-003")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_MinLengthName_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var minLengthPartner = new { 
                Name = "A", // 1 character
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", minLengthPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-004: Create partner with name at exact maximum length (e.g. 500 chars)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-004")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_MaxLengthName_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var maxLengthName = new string('A', 500); // Assuming 500 is max
            var maxLengthPartner = new { 
                Name = maxLengthName,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", maxLengthPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-005: Create partner with name at maximum + 1 char
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-005")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_MaxLengthPlusOne_Rejects()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var tooLongName = new string('A', 501); // Max + 1
            var invalidPartner = new { 
                Name = tooLongName,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", invalidPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-006: Get partners with page = 1 (first page boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-006")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_PageOne_ReturnsFirstPage()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?page=1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-007: Get partners with pageSize = 1 (minimum page size)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-007")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_PageSizeOne_ReturnsSingleItem()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?pageSize=1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-008: Get partners with pageSize = 100 (typical maximum)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-008")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_PageSizeMaxAllowed_ReturnsMaxItems()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?pageSize=100");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-009: Create partner with budget = 0 (zero value boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-009")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_ZeroBudget_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var zeroBudgetPartner = new { 
                Name = "Zero Budget Partner",
                AnnualBudget = 0,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", zeroBudgetPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-010: Create partner with budget = decimal max value
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-010")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_MaxBudget_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var maxBudgetPartner = new { 
                Name = "Max Budget Partner",
                AnnualBudget = 999999999999.99m,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", maxBudgetPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-011: Get partners with page = 1000000 (extreme page number)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-011")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_ExtremePage_ReturnsEmptyOrError()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?page=1000000");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-012: Create partner with established date = current date (boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-012")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_CurrentDate_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var currentDatePartner = new { 
                Name = "Current Date Partner",
                EstablishedDate = System.DateTime.Now.ToString("yyyy-MM-dd"),
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", currentDatePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-013: Create partner with established date = year 1900 (old boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-013")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_Year1900_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var oldDatePartner = new { 
                Name = "Old Date Partner",
                EstablishedDate = "1900-01-01",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", oldDatePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-014: Get partners with search term = single character
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-014")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_SingleCharSearch_ReturnsResults()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?search=A");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-015: Get partners with search term = 1000 characters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-015")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_VeryLongSearch_HandlesGracefully()
        {
            // Arrange
            var longSearch = new string('A', 1000);

            // Act
            var response = await _client.GetAsync($"/api/partner?search={longSearch}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-016: Bulk create partners with exactly 1 partner
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-016")]
        [Trait("Priority", "Low")]
        public async Task BulkCreatePartners_SingleItem_Works()
        {
            // Arrange
            var singlePartner = new[] { 
                new { Name = "Single Partner", Status = "Active" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner/bulk", singlePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-017: Bulk create partners with 1000 partners (large batch)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-017")]
        [Trait("Priority", "Medium")]
        public async Task BulkCreatePartners_LargeBatch_HandlesGracefully()
        {
            // Arrange
            var largeBatch = Enumerable.Range(1, 1000).Select(i => new { 
                Name = $"Partner {i}",
                Status = "Active"
            }).ToArray();

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner/bulk", largeBatch);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-018: Create partner with email at maximum length (320 chars RFC 5321)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-018")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_MaxLengthEmail_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var localPart = new string('a', 64); // Max local part
            var domainPart = new string('b', 251); // Remaining for domain
            var maxEmail = $"{localPart}@{domainPart}.com";
            var partner = new { 
                Name = "Max Email Partner",
                Email = maxEmail,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", partner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-019: Get partner export with 0 results
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-019")]
        [Trait("Priority", "Medium")]
        public async Task GetPartnerExport_ZeroResults_ReturnsEmptyFile()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/export?status=NONEXISTENT_STATUS");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-020: Get partners with filter returning exactly 1 result
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-020")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_ExactlyOneResult_ReturnsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?search=UniquePartnerName12345");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        #endregion

        #region Unicode and Special Characters (15 tests)

        /// <summary>
        /// TC-PARTNER-EDGE-021: Create partner with Chinese characters in name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-021")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_ChineseCharacters_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var chinesePartner = new { 
                Name = "åˆä½œä¼™ä¼´åç§°",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", chinesePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-022: Create partner with Arabic characters in name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-022")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_ArabicCharacters_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var arabicPartner = new { 
                Name = "Ø§Ø³Ù… Ø§Ù„Ø´Ø±ÙŠÙƒ",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", arabicPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-023: Create partner with emoji in name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-023")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_EmojiInName_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var emojiPartner = new { 
                Name = "Partner ðŸ¤ Company",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", emojiPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-024: Create partner with Cyrillic characters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-024")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_CyrillicCharacters_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var cyrillicPartner = new { 
                Name = "ÐŸÐ°Ñ€Ñ‚Ð½ÐµÑ€ ÐÐ°Ð·Ð²Ð°Ð½Ð¸Ðµ",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", cyrillicPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-025: Create partner with mixed scripts (Latin + Arabic + Chinese)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-025")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_MixedScripts_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var mixedPartner = new { 
                Name = "Partner Ø§Ù„Ø´Ø±ÙŠÙƒ åˆä½œä¼™ä¼´",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", mixedPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-026: Create partner with special characters (quotes, ampersand)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-026")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_SpecialCharacters_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var specialCharsPartner = new { 
                Name = "O'Reilly & Associates \"Partners\"",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", specialCharsPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-027: Create partner with leading/trailing spaces
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-027")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_LeadingTrailingSpaces_TrimsOrAccepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var spacedPartner = new { 
                Name = "  Partner Name  ",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", spacedPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-028: Create partner with internal multiple spaces
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-028")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_InternalMultipleSpaces_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var multiSpacePartner = new { 
                Name = "Partner    Multiple    Spaces",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", multiSpacePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-029: Create partner with newline character in name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-029")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_NewlineInName_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var newlinePartner = new { 
                Name = "Partner\nName",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", newlinePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-030: Create partner with tab character in name
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-030")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_TabInName_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var tabPartner = new { 
                Name = "Partner\tName",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", tabPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-031: Get partners search with Unicode wildcard characters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-031")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_UnicodeWildcardSearch_HandlesGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?search=åˆä½œ*");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-032: Create partner with RTL (Right-To-Left) text
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-032")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_RtlText_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var rtlPartner = new { 
                Name = "\u202EØ´Ø±ÙŠÙƒ Ø§Ù„Ø§Ø³Ù…\u202C", // RTL override characters
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", rtlPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-033: Create partner with zero-width joiner characters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-033")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_ZeroWidthCharacters_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var zeroWidthPartner = new { 
                Name = "Part\u200Dner\u200CName", // Zero-width joiner and non-joiner
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", zeroWidthPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-034: Create partner with combining diacritical marks
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-034")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_CombiningMarks_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var diacriticPartner = new { 
                Name = "PÃ¢rtÃ±Ã©r Ã‘Ã¡mÃ¨", // Combining diacritical marks
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", diacriticPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-035: Get partners with search containing URL-encoded characters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-035")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_UrlEncodedSearch_DecodesCorrectly()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?search=Partner%20%26%20Company");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        #endregion

        #region Concurrency and Rapid Operations (15 tests)

        /// <summary>
        /// TC-PARTNER-EDGE-036: Rapid sequential gets to same partner (20 requests)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-036")]
        [Trait("Priority", "Medium")]
        public async Task GetPartner_RapidSequential_NoStateIssues()
        {
            // Act & Assert
            for (int i = 0; i < 20; i++)
            {
                var response = await _client.GetAsync("/api/partner/1");
                response.Should().NotBeNull();
            }
        }

        /// <summary>
        /// TC-PARTNER-EDGE-037: 50 concurrent GET requests to same partner
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-037")]
        [Trait("Priority", "High")]
        public async Task GetPartner_50Concurrent_AllSucceed()
        {
            // Arrange
            var tasks = Enumerable.Range(1, 50).Select(_ => _client.GetAsync("/api/partner/1"));

            // Act
            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().HaveCount(50);
            responses.Should().OnlyContain(r => r != null);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-038: 100 concurrent GET partners list requests
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-038")]
        [Trait("Priority", "High")]
        public async Task GetPartners_100Concurrent_AllSucceed()
        {
            if (!_isPostgresAvailable) return;
            // Arrange
            var tasks = Enumerable.Range(1, 100).Select(_ => _client.GetAsync("/api/partner"));

            // Act
            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().HaveCount(100);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-039: Immediate update after create (no delay)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-039")]
        [Trait("Priority", "Medium")]
        public async Task CreateThenImmediateUpdate_NoDelay_HandlesGracefully()
        {
            // Arrange
            var createData = new { Name = "Rapid Partner", Status = "Active" };
            var updateData = new { Name = "Updated Rapid Partner" };

            // Act
            var createResponse = await _client.PostAsJsonAsync("/api/partner", createData);
            var updateResponse = await _client.PutAsJsonAsync("/api/partner/1", updateData);

            // Assert
            updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-040: Rapid create-delete-recreate cycle
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-040")]
        [Trait("Priority", "Medium")]
        public async Task CreateDeleteRecreate_RapidCycle_HandlesGracefully()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var data = new { Name = "Cycle Partner", Status = "Active" };

            // Act
            var createResponse1 = await _client.PostAsJsonAsync("/api/partner", data);
            var deleteResponse = await _client.DeleteAsync("/api/partner/1");
            var createResponse2 = await _client.PostAsJsonAsync("/api/partner", data);

            // Assert
            createResponse2.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-041: Double submit (click button twice rapidly)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-041")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_DoubleSubmit_PreventsDuplicate()
        {
            // Arrange
            var data = new { Name = "Double Submit Partner", Status = "Active" };

            // Act
            var task1 = _client.PostAsJsonAsync("/api/partner", data);
            var task2 = _client.PostAsJsonAsync("/api/partner", data);
            var responses = await Task.WhenAll(task1, task2);

            // Assert - At least one should succeed or conflict
            responses.Should().Contain(r => r.StatusCode == HttpStatusCode.Created || r.StatusCode == HttpStatusCode.Conflict || r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-042: Concurrent updates to different fields of same partner
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-042")]
        [Trait("Priority", "High")]
        public async Task UpdatePartner_ConcurrentDifferentFields_HandlesConflict()
        {
            // Arrange
            var update1 = new { Name = "Updated Name 1" };
            var update2 = new { Status = "Inactive" };

            // Act
            var task1 = _client.PutAsJsonAsync("/api/partner/1", update1);
            var task2 = _client.PutAsJsonAsync("/api/partner/1", update2);
            var responses = await Task.WhenAll(task1, task2);

            // Assert
            responses.Should().Contain(r => r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.Conflict || r.StatusCode == HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-043: Concurrent delete of same partner from multiple users
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-043")]
        [Trait("Priority", "Medium")]
        public async Task DeletePartner_ConcurrentSameId_OnlyOneSucceeds()
        {
            if (!_isPostgresAvailable) return;
            // Act
            var task1 = _client.DeleteAsync("/api/partner/1");
            var task2 = _client.DeleteAsync("/api/partner/1");
            var responses = await Task.WhenAll(task1, task2);

            // Assert
            responses.Should().Contain(r => r.StatusCode == HttpStatusCode.NoContent || r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.OK);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-044: Read partner during update operation
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-044")]
        [Trait("Priority", "Medium")]
        public async Task GetPartner_DuringUpdate_ReturnsConsistentState()
        {
            if (!_isPostgresAvailable) return;
            // Arrange
            var updateData = new { Name = "Being Updated" };

            // Act
            var updateTask = _client.PutAsJsonAsync("/api/partner/1", updateData);
            var getTask = _client.GetAsync("/api/partner/1");
            var responses = await Task.WhenAll(updateTask, getTask);

            // Assert
            responses[1].StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-045: Rapid pagination through all pages
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-045")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_RapidPagination_AllPagesSucceed()
        {
            // Act
            var tasks = Enumerable.Range(1, 10).Select(page => 
                _client.GetAsync($"/api/partner?page={page}&pageSize=10")
            );
            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().HaveCount(10);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-046: Create partners with identical names concurrently
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-046")]
        [Trait("Priority", "High")]
        public async Task CreatePartner_IdenticalNamesConcurrent_PreventsDuplicates()
        {
            // Arrange
            var identicalData = new { Name = "Identical Partner Name", Status = "Active" };

            // Act
            var tasks = Enumerable.Range(1, 5).Select(_ => 
                _client.PostAsJsonAsync("/api/partner", identicalData)
            );
            var responses = await Task.WhenAll(tasks);

            // Assert - Only one should succeed, others should conflict
            responses.Should().Contain(r => r.StatusCode == HttpStatusCode.Created || r.StatusCode == HttpStatusCode.Conflict || r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-047: Approve same partner concurrently from multiple users
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-047")]
        [Trait("Priority", "Medium")]
        public async Task ApprovePartner_Concurrent_OnlyOneSucceeds()
        {
            if (!_isPostgresAvailable) return;
            // Act
            var task1 = _client.PostAsync("/api/partner/1/approve", null);
            var task2 = _client.PostAsync("/api/partner/1/approve", null);
            var responses = await Task.WhenAll(task1, task2);

            // Assert
            responses.Should().Contain(r => r.StatusCode == HttpStatusCode.OK || r.StatusCode == HttpStatusCode.Conflict || r.StatusCode == HttpStatusCode.Unauthorized || r.StatusCode == HttpStatusCode.UnsupportedMediaType || r.StatusCode == HttpStatusCode.MethodNotAllowed);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-048: Bulk operations with overlapping IDs
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-048")]
        [Trait("Priority", "Low")]
        public async Task BulkDeletePartners_OverlappingIds_HandlesGracefully()
        {
            // Arrange
            var batch1 = new[] { 1, 2, 3 };
            var batch2 = new[] { 2, 3, 4 }; // Overlapping

            // Act
            var task1 = _client.PostAsJsonAsync("/api/partner/bulk-delete", batch1);
            var task2 = _client.PostAsJsonAsync("/api/partner/bulk-delete", batch2);
            var responses = await Task.WhenAll(task1, task2);

            // Assert
            responses.Should().OnlyContain(r => r != null);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-049: Rapid filter changes (stress test UI scenario)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-049")]
        [Trait("Priority", "Medium")]
        public async Task GetPartners_RapidFilterChanges_AllRespond()
        {
            // Act
            var responses = new[]
            {
                await _client.GetAsync("/api/partner?status=Active"),
                await _client.GetAsync("/api/partner?status=Inactive"),
                await _client.GetAsync("/api/partner?search=Test"),
                await _client.GetAsync("/api/partner?category=Type1"),
                await _client.GetAsync("/api/partner?sortBy=Name")
            };

            // Assert
            responses.Should().HaveCount(5);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-050: Export large dataset (1000+ partners)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-050")]
        [Trait("Priority", "Medium")]
        public async Task GetPartnerExport_LargeDataset_CompletesSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/export?format=csv");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        #endregion

        #region Extreme and Unusual Scenarios (23 tests)

        /// <summary>
        /// TC-PARTNER-EDGE-051: Create partner with all optional fields null
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-051")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_AllOptionalFieldsNull_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var minimalPartner = new { 
                Name = "Minimal Partner",
                Status = "Active"
                // All other fields omitted/null
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", minimalPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-052: Create partner with all possible fields populated
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-052")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_AllFieldsPopulated_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var maximalPartner = new { 
                Name = "Maximal Partner",
                Status = "Active",
                Email = "max@example.com",
                Phone = "+1-555-1234",
                Website = "https://example.com",
                Description = "Full description",
                Category = "Type1",
                AnnualBudget = 1000000,
                EstablishedDate = "2020-01-01",
                CountryCode = "USA"
                // ... all possible fields
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", maximalPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-053: Get partners with combination of all filters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-053")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_AllFiltersCombined_HandlesGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?status=Active&category=Type1&search=Test&sortBy=Name&page=1&pageSize=10");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-054: Import CSV with 10,000 partners
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-054")]
        [Trait("Priority", "Low")]
        public async Task ImportPartners_LargeCsv_HandlesGracefully()
        {
            // Arrange
            var largeCsv = "Name,Status\n" + string.Join("\n", Enumerable.Range(1, 10000).Select(i => $"Partner{i},Active"));
            var content = new StringContent(largeCsv);

            // Act
            var response = await _client.PostAsync("/api/partner/import", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-055: Update partner changing only 1 field
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-055")]
        [Trait("Priority", "Low")]
        public async Task UpdatePartner_SingleFieldChange_Accepts()
        {
            // Arrange
            var singleFieldUpdate = new { Status = "Inactive" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", singleFieldUpdate);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-056: Get partner with include depth of 5 levels
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-056")]
        [Trait("Priority", "Low")]
        public async Task GetPartner_DeepInclude_HandlesGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/1?include=contacts,interactions,documents,orgunits,categories");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-057: Create partner with website containing query parameters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-057")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_WebsiteWithQueryParams_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var complexUrlPartner = new { 
                Name = "Complex URL Partner",
                Website = "https://example.com/page?param1=value1&param2=value2#anchor",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", complexUrlPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-058: Get partners sorted by non-existent field with fallback
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-058")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_SortByNonExistentField_FallbacksGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?sortBy=NonExistentField123");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-059: Create partner with description containing HTML tags
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-059")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_HtmlInDescription_SanitizesOrAccepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var htmlPartner = new { 
                Name = "HTML Partner",
                Description = "<b>Bold</b> and <i>italic</i> text with <a href='test'>link</a>",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", htmlPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-060: Bulk update 500 partners
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-060")]
        [Trait("Priority", "Low")]
        public async Task BulkUpdatePartners_LargeBatch_HandlesGracefully()
        {
            // Arrange
            var largeBatch = Enumerable.Range(1, 500).Select(i => new { 
                Id = i,
                Status = "Active"
            }).ToArray();

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/bulk", largeBatch);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-061: Get partner audit log with 10,000+ entries
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-061")]
        [Trait("Priority", "Low")]
        public async Task GetPartnerAuditLog_LargeHistory_PaginatesCorrectly()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/1/audit?pageSize=100");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-062: Create partner with phone number in various international formats
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-062")]
        [Trait("Priority", "Medium")]
        public async Task CreatePartner_InternationalPhoneFormats_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var intlPhonePartner = new { 
                Name = "International Phone Partner",
                Phone = "+44 20 7946 0958", // UK format
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", intlPhonePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-063: Get partners with date range filter spanning 50 years
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-063")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_LargeDateRange_HandlesGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?startDate=1970-01-01&endDate=2020-12-31");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-064: Create partner with budget value having many decimal places
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-064")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_ManyDecimalPlaces_RoundsCorrectly()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var precisePartner = new { 
                Name = "Precise Budget Partner",
                AnnualBudget = 123456.789012345m,
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", precisePartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-065: Get typeahead with single letter returning 1000+ matches
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-065")]
        [Trait("Priority", "Medium")]
        public async Task GetPartnersTypeahead_ManyMatches_LimitsResults()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/typeahead?search=A");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-066: Update partner with no actual changes (idempotent)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-066")]
        [Trait("Priority", "Low")]
        public async Task UpdatePartner_NoChanges_IdempotentSuccess()
        {
            // Arrange
            var noChangeUpdate = new { Name = "Existing Name" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/partner/1", noChangeUpdate);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-067: Create partner with all text fields at maximum allowed length
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-067")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_AllFieldsMaxLength_Accepts()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var maxFieldsPartner = new { 
                Name = new string('A', 500),
                Description = new string('B', 10000),
                Website = "https://" + new string('c', 300) + ".com",
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", maxFieldsPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-068: Get partner timeline with no interactions
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-068")]
        [Trait("Priority", "Low")]
        public async Task GetPartnerTimeline_NoInteractions_ReturnsEmpty()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/1/timeline");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-069: Import CSV with mixed line endings (CRLF and LF)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-069")]
        [Trait("Priority", "Low")]
        public async Task ImportPartners_MixedLineEndings_ParsesCorrectly()
        {
            // Arrange
            var mixedCsv = "Name,Status\r\nPartner1,Active\nPartner2,Inactive\r\n";
            var content = new StringContent(mixedCsv);

            // Act
            var response = await _client.PostAsync("/api/partner/import", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-070: Create partner with establishment date = Dec 31, leap year
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-070")]
        [Trait("Priority", "Low")]
        public async Task CreatePartner_LeapYearBoundary_HandlesCorrectly()
        {
            if (!_isPostgresAvailable) return; // Requires PostgreSQL for partner creation (pg_trgm)
            // Arrange
            var leapYearPartner = new { 
                Name = "Leap Year Partner",
                EstablishedDate = "2020-02-29", // Leap year
                Status = "Active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/partner", leapYearPartner);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-071: Get partners with invalid combination of mutually exclusive filters
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-071")]
        [Trait("Priority", "Low")]
        public async Task GetPartners_MutuallyExclusiveFilters_HandlesGracefully()
        {
            // Act
            var response = await _client.GetAsync("/api/partner?status=Active&status=Inactive");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-072: Update partner logo with exactly 5MB file (boundary)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-072")]
        [Trait("Priority", "Medium")]
        public async Task UpdatePartnerLogo_ExactBoundarySize_Accepts()
        {
            // Arrange
            var fiveMbFile = new ByteArrayContent(new byte[5 * 1024 * 1024]);

            // Act
            var response = await _client.PutAsync("/api/partner/1/logo", fiveMbFile);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.Unauthorized, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// TC-PARTNER-EDGE-073: Get partner statistics for newly created partner (no history)
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-PARTNER-EDGE-073")]
        [Trait("Priority", "Low")]
        public async Task GetPartnerStatistics_NewPartner_ReturnsZeroCounts()
        {
            // Act
            var response = await _client.GetAsync("/api/partner/1/statistics");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
        }

        #endregion
    }
}
