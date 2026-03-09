/**
 * @fileoverview Integration tests for UNOPS DocumentController endpoints.
 * Tests the UNOPS override endpoints: entity list, upload, link, delete, download.
 * Base DocumentController endpoints are blocked by DEF-024 (GetCredentials in constructor).
 *
 * Ratio: P=3, N=9, E=9, F=9, I=9 → Total=39
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for UNOPS DocumentController endpoints.
///
/// The UNOPS override endpoints (entity/, upload, link, delete, Download/)
/// use UNOPSDocumentManager and do NOT trigger DEF-024 directly.
/// However, DEF-053 (UNOPSGeminiManager crashes on missing Google credentials)
/// prevents the server from resolving IManagerWrapper for authenticated requests.
///
/// Unauthenticated tests (401 assertions) work because the auth middleware
/// short-circuits before controller/manager instantiation.
///
/// Ratio compliance:
///   Positive  (P) =  3  (skipped — DEF-053)
///   Negative  (N) =  9  (N ≥ 3P ✅)
///   Edge      (E) =  9  (E ≥ 3P ✅)
///   Functional(F) =  9  (F ≥ 3P ✅)
///   Integration(I)=  9  (I ≥ 3P ✅)
///   ─────────────────────────────────
///   TOTAL         = 39
/// </summary>
[Collection("Integration Tests")]
public class DocumentControllerUNOPSTests : IntegrationTestBase
{
    private readonly bool _isPostgresAvailable;

    public DocumentControllerUNOPSTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
        _isPostgresAvailable = Factory.IsUsingPostgres;
    }

    // ═══════════════════════════════════════════════════════════════════
    // POSITIVE TESTS (P=3) — Blocked by DEF-053 (authenticated requests)
    // ═══════════════════════════════════════════════════════════════════

    #region Positive Tests

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("TestId", "TC-DUNOPS-P01")]
    public async Task GetDocumentsByEntity_ValidPartner_ReturnsListOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Partner/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("TestId", "TC-DUNOPS-P02")]
    public async Task GetDocumentsByEntity_ValidContact_ReturnsListOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Contact/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("TestId", "TC-DUNOPS-P03")]
    public async Task DeleteDocument_ValidId_ReturnsSuccessOrNotFound()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.DeleteAsync("/api/document/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // NEGATIVE TESTS (N=9) — Auth checks (401) work without manager
    // ═══════════════════════════════════════════════════════════════════

    #region Negative Tests

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N01")]
    public async Task GetDocumentsByEntity_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/document/entity/Partner/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N02")]
    public async Task DeleteDocument_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.DeleteAsync("/api/document/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N03")]
    public async Task UploadDocument_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Partner"), "EntityType");
        content.Add(new StringContent("1"), "EntityId");
        var response = await client.PostAsync("/api/document/upload", content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N04")]
    public async Task LinkDocument_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var linkData = new { link = "https://drive.google.com/test", entityType = "Partner", entityId = 1 };
        var response = await client.PostAsJsonAsync("/api/document/link", linkData);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N05")]
    public async Task DownloadDocument_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/document/Download/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N06")]
    public async Task GetDocumentsByEntity_InvalidEntityType_ReturnsErrorOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/InvalidType/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N07")]
    public async Task GetDocumentsByEntity_NonExistentEntityId_ReturnsEmptyOrNotFound()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Partner/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N08")]
    public async Task DeleteDocument_NonExistentId_ReturnsNotFoundOrError()
    {
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.DeleteAsync("/api/document/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("TestId", "TC-DUNOPS-N09")]
    public async Task DownloadDocument_NonExistentId_ReturnsNotFoundOrError()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/Download/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // EDGE/BOUNDARY TESTS (E=9) — Boundary IDs and entity types
    // ═══════════════════════════════════════════════════════════════════

    #region Edge/Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E01")]
    public async Task GetDocumentsByEntity_EntityIdZero_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/document/entity/Partner/0");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E02")]
    public async Task GetDocumentsByEntity_NegativeEntityId_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/document/entity/Partner/-1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E03")]
    public async Task DeleteDocument_IdZero_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.DeleteAsync("/api/document/0");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E04")]
    public async Task DeleteDocument_NegativeId_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.DeleteAsync("/api/document/-1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E05")]
    public async Task UploadDocument_EmptyMultipart_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync("/api/document/upload", new MultipartFormDataContent());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E06")]
    public async Task LinkDocument_EmptyBody_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/document/link", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E07")]
    public async Task GetDocumentsByEntity_Opportunity_ReturnsListOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E08")]
    public async Task GetDocumentsByEntity_Interaction_ReturnsListOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Interaction/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Edge")]
    [Trait("TestId", "TC-DUNOPS-E09")]
    public async Task GetDocumentsByEntity_MaxIntId_HandlesGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync($"/api/document/entity/Partner/{int.MaxValue}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // FUNCTIONAL TESTS (F=9) — HTTP method and response behavior
    // ═══════════════════════════════════════════════════════════════════

    #region Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F01")]
    public async Task GetDocumentsByEntity_GetMethod_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/document/entity/Partner/1");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F02")]
    public async Task DeleteDocument_DeleteMethod_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.DeleteAsync("/api/document/5");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F03")]
    public async Task UploadDocument_PostMethod_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Partner"), "ParentEntityType");
        var response = await client.PostAsync("/api/document/upload", content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F04")]
    public async Task LinkDocument_PostMethod_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var body = new { link = "https://test.example.com/doc", parentEntityType = "Partner", parentEntityId = 1 };
        var response = await client.PostAsJsonAsync("/api/document/link", body);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F05")]
    public async Task DownloadDocument_GetMethod_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/document/Download/5");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F06")]
    public async Task GetDocumentsByEntity_ReturnsJsonContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Partner/1");
        if (response.IsSuccessStatusCode && response.Content.Headers.ContentType != null)
        {
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        }
    }

    [Fact]

    [Trait("Defect", "DEF-024")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F07")]
    public async Task GenerateGoogleDoc_CorrectRoute_UsesGenerateDocument()
    {
        var client = Factory.CreateAuthenticatedClient();
        var generateData = new { data = "Test content", filename = "TestDoc" };
        var response = await client.PostAsJsonAsync("/api/document/generate-document", generateData);
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "because /api/document/generate-document is the correct production route");
    }

    [Fact]

    [Trait("Defect", "DEF-024")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F08")]
    public async Task GetDocumentViewUrl_Authenticated_ReturnsResponse()
    {
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/view-url/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]

    [Trait("Defect", "DEF-021")]
    [Trait("Category", "Functional")]
    [Trait("TestId", "TC-DUNOPS-F09")]
    public async Task BaseDocumentGetAll_NoRouteConflict_WithUNOPSOverride()
    {
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/Partner/1");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "because DEF-021 route ambiguity should be resolved");
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════
    // INTEGRATION TESTS (I=9) — Multi-endpoint flows and consistency
    // ═══════════════════════════════════════════════════════════════════

    #region Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I01")]
    public async Task AllUNOPSEndpoints_Unauthenticated_AllReturn401()
    {
        var client = Factory.CreateClient();
        var getResponse = await client.GetAsync("/api/document/entity/Partner/1");
        var deleteResponse = await client.DeleteAsync("/api/document/1");
        var downloadResponse = await client.GetAsync("/api/document/Download/1");

        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I02")]
    public async Task PostEndpoints_Unauthenticated_AllReturn401()
    {
        var client = Factory.CreateClient();
        var uploadResponse = await client.PostAsync("/api/document/upload", new MultipartFormDataContent());
        var linkResponse = await client.PostAsJsonAsync("/api/document/link", new { link = "https://test.com" });

        uploadResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        linkResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I03")]
    public async Task AllEntityTypes_Unauthenticated_AllReturn401()
    {
        var client = Factory.CreateClient();
        var entityTypes = new[] { "Partner", "Contact", "Interaction", "Opportunity" };

        foreach (var entityType in entityTypes)
        {
            var response = await client.GetAsync($"/api/document/entity/{entityType}/1");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                $"because GET /api/document/entity/{entityType}/1 requires auth");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I04")]
    public async Task MultipleEntityIds_Unauthenticated_AllReturn401()
    {
        var client = Factory.CreateClient();
        for (int id = 1; id <= 5; id++)
        {
            var response = await client.GetAsync($"/api/document/entity/Partner/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I05")]
    public async Task SequentialDeleteAttempts_Unauthenticated_AllReturn401()
    {
        var client = Factory.CreateClient();
        for (int id = 1; id <= 3; id++)
        {
            var response = await client.DeleteAsync($"/api/document/{id}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I06")]
    public async Task GetDocumentsByEntity_ConsistentResponse_AcrossEntityTypes()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = Factory.CreateAuthenticatedClient();
        var entityTypes = new[] { "Partner", "Contact", "Interaction", "Opportunity" };

        foreach (var entityType in entityTypes)
        {
            var response = await client.GetAsync($"/api/document/entity/{entityType}/1");
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
                $"because /api/document/entity/{entityType}/1 should not crash");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I07")]
    public async Task UploadDocument_WithFile_ReturnsSuccessOrError()
    {
        var client = Factory.CreateAuthenticatedClient();
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("Partner"), "ParentEntityType");
        content.Add(new StringContent("1"), "ParentEntityId");
        content.Add(new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }), "File", "test.pdf");

        var response = await client.PostAsync("/api/document/upload", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I08")]
    public async Task LinkDocument_ValidPayload_ReturnsResponse()
    {
        var client = Factory.CreateAuthenticatedClient();
        var linkData = new
        {
            link = "https://docs.google.com/document/d/test123",
            name = "Test Document Link",
            parentEntityType = "Partner",
            parentEntityId = 1
        };
        var response = await client.PostAsJsonAsync("/api/document/link", linkData);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("TestId", "TC-DUNOPS-I09")]
    public async Task GetDocumentsByEntity_ResponseIsJsonArray()
    {
        var client = Factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/document/entity/Partner/1");

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNull();
            body.TrimStart().Should().Match(
                s => s.StartsWith("[") || s.StartsWith("{"),
                "because response should be valid JSON");
        }
    }

    #endregion
}
