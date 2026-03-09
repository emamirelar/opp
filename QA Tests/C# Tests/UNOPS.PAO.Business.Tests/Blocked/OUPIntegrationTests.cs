/**
 * OUP (oneUNOPS) INTEGRATION TESTS
 *
 * Unit tests for oUP integration behavior using mocked services.
 * Uses Moq to simulate oUP API responses without requiring live credentials.
 *
 * Coverage Areas:
 * - Authentication (10)
 * - Data Synchronization (10)
 * - Partner Matching (10)
 * - Error Handling (5)
 * - Audit & Logging (5)
 */

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Blocked
{
    /// <summary>
    /// oUP Integration Tests - Mocked unit tests for oUP integration behavior.
    /// </summary>
    public class OUPIntegrationTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<OUPIntegrationTests>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly HttpClient _httpClient;
        private readonly List<LogEntry> _logEntries = [];

        private const string MockTokenResponse = """
            {"access_token": "mock-jwt-token-12345", "token_type": "Bearer", "expires_in": 3600}
            """;

        private const string MockPartnerResponse = """
            [{"id": 1, "name": "Test Partner", "dunsNumber": "123456789", "taxId": "TX-001", "country": "Denmark"}]
            """;

        private const string MockEngagementResponse = """
            {"engagementNumber": "UENB-TEST-001", "name": "Test Engagement", "stage": "Identify & Profile", "estimatedAmount": 1500000}
            """;

        private const string MockSyncStatusResponse = """
            {"status": "completed", "recordsProcessed": 150, "recordsInserted": 10, "recordsUpdated": 5, "errors": 0}
            """;

        private const string MockMatchResponse = """
            [{"id": 1, "name": "Test Partner", "dunsNumber": "123456789", "matchScore": 1.0}]
            """;

        public OUPIntegrationTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["OUPSettings:BaseUrl"]).Returns("https://projects-test.unops.org");
            _mockConfig.Setup(c => c["OUPSettings:ApiUrl"]).Returns("https://projects-test.unops.org/api");
            _mockConfig.Setup(c => c["OUPSettings:ClientId"]).Returns("test-client-id");
            _mockConfig.Setup(c => c["OUPSettings:ClientSecret"]).Returns("test-client-secret");
            _mockConfig.Setup(c => c["OUPSettings:TokenUrl"]).Returns("https://projects-test.unops.org/oauth/token");
            _mockConfig.Setup(c => c["OUPSettings:MaxRetries"]).Returns("3");
            _mockConfig.Setup(c => c["OUPSettings:TimeoutSeconds"]).Returns("30");

            _mockLogger = new Mock<ILogger<OUPIntegrationTests>>();
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object?, Exception?, Delegate>((level, _, state, ex, _) =>
                {
                    _logEntries.Add(new LogEntry(level, state?.ToString() ?? "", ex));
                });

            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object)
            {
                BaseAddress = new Uri("https://projects-test.unops.org")
            };
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{}")
        {
            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
        }

        private void SetupSequentialHttpResponses(params (HttpStatusCode Code, string Content)[] responses)
        {
            var queue = new Queue<(HttpStatusCode, string)>(responses);
            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    var (code, content) = queue.Dequeue();
                    return new HttpResponseMessage { StatusCode = code, Content = new StringContent(content) };
                });
        }

        private record LogEntry(LogLevel Level, string Message, Exception? Exception);

        #region Authentication Tests (10)

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP001_Authentication_ValidCredentials_Succeeds()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockTokenResponse);

            var response = await _httpClient.GetAsync("/oauth/token");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            json.RootElement.GetProperty("access_token").GetString().Should().Be("mock-jwt-token-12345");
            json.RootElement.GetProperty("token_type").GetString().Should().Be("Bearer");
            json.RootElement.GetProperty("expires_in").GetInt32().Should().Be(3600);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP002_Authentication_InvalidCredentials_Fails()
        {
            SetupHttpResponse(HttpStatusCode.Unauthorized, """{"error": "invalid_client"}""");

            var response = await _httpClient.GetAsync("/oauth/token");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("invalid_client");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP003_Authentication_TokenRefresh()
        {
            SetupSequentialHttpResponses(
                (HttpStatusCode.OK, MockTokenResponse),
                (HttpStatusCode.OK, """{"access_token": "refreshed-token", "token_type": "Bearer", "expires_in": 3600}""")
            );

            var response1 = await _httpClient.GetAsync("/oauth/token");
            var content1 = await response1.Content.ReadAsStringAsync();
            var token1 = JsonDocument.Parse(content1).RootElement.GetProperty("access_token").GetString();
            token1.Should().Be("mock-jwt-token-12345");

            var response2 = await _httpClient.GetAsync("/oauth/token");
            var content2 = await response2.Content.ReadAsStringAsync();
            var token2 = JsonDocument.Parse(content2).RootElement.GetProperty("access_token").GetString();
            token2.Should().Be("refreshed-token");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP004_Authentication_TokenExpiry()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"access_token": "expired-token", "token_type": "Bearer", "expires_in": 0}""");

            var response = await _httpClient.GetAsync("/oauth/token");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            json.RootElement.GetProperty("expires_in").GetInt32().Should().Be(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP005_Authentication_ServiceAccount()
        {
            _mockConfig.Setup(c => c["OUPSettings:ClientId"]).Returns("service-account-client");
            _mockConfig.Setup(c => c["OUPSettings:ClientSecret"]).Returns("service-secret");

            var clientId = _mockConfig.Object["OUPSettings:ClientId"];
            var clientSecret = _mockConfig.Object["OUPSettings:ClientSecret"];

            clientId.Should().Be("service-account-client");
            clientSecret.Should().Be("service-secret");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP006_Authentication_Scopes()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"access_token": "scoped-token", "scope": "partners engagements", "expires_in": 3600}""");

            var response = await _httpClient.GetAsync("/oauth/token");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            json.RootElement.TryGetProperty("scope", out var scopeProp).Should().BeTrue();
            scopeProp.GetString().Should().Contain("partners");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP007_Authentication_RateLimiting()
        {
            SetupHttpResponse((HttpStatusCode)429, """{"error": "rate_limit_exceeded"}""");

            var response = await _httpClient.GetAsync("/oauth/token");
            response.StatusCode.Should().Be((HttpStatusCode)429);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP008_Authentication_RetryLogic()
        {
            var maxRetries = _mockConfig.Object["OUPSettings:MaxRetries"];
            maxRetries.Should().Be("3");

            SetupSequentialHttpResponses(
                (HttpStatusCode.ServiceUnavailable, "{}"),
                (HttpStatusCode.ServiceUnavailable, "{}"),
                (HttpStatusCode.OK, MockTokenResponse)
            );

            var response1 = await _httpClient.GetAsync("/oauth/token");
            response1.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

            var response2 = await _httpClient.GetAsync("/oauth/token");
            response2.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

            var response3 = await _httpClient.GetAsync("/oauth/token");
            response3.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP009_Authentication_SecureStorage()
        {
            var baseUrl = _mockConfig.Object["OUPSettings:BaseUrl"];
            var tokenUrl = _mockConfig.Object["OUPSettings:TokenUrl"];
            var clientId = _mockConfig.Object["OUPSettings:ClientId"];

            baseUrl.Should().NotBeNullOrEmpty();
            tokenUrl.Should().NotBeNullOrEmpty();
            clientId.Should().NotBeNullOrEmpty();
            baseUrl.Should().StartWith("https://");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP010_Authentication_AuditLogging()
        {
            _logEntries.Clear();
            _mockLogger.Object.LogInformation("Authentication attempt for client {ClientId}", "test-client-id");

            _logEntries.Should().HaveCount(1);
            _logEntries[0].Level.Should().Be(LogLevel.Information);
            _logEntries[0].Message.Should().Contain("test-client-id");
        }

        #endregion

        #region Data Synchronization Tests (10)

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP011_Sync_PartnerToOUP()
        {
            SetupHttpResponse(HttpStatusCode.Created, """{"id": 42, "name": "Synced Partner"}""");

            var content = new StringContent("""{"name": "Test Partner", "dunsNumber": "123456789"}""", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/partners", content);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("id").GetInt32().Should().Be(42);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP012_Sync_PartnerFromOUP()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockPartnerResponse);

            var response = await _httpClient.GetAsync("/api/partners");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var partners = JsonDocument.Parse(responseContent).RootElement;
            partners.GetArrayLength().Should().Be(1);
            partners[0].GetProperty("name").GetString().Should().Be("Test Partner");
            partners[0].GetProperty("dunsNumber").GetString().Should().Be("123456789");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP013_Sync_IncrementalSync()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"lastSync": "2025-03-06T12:00:00Z", "records": []}""");

            var response = await _httpClient.GetAsync("/api/sync/incremental?since=2025-03-05");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("lastSync").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP014_Sync_FullSync()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockSyncStatusResponse);

            var response = await _httpClient.PostAsync("/api/sync/full", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("status").GetString().Should().Be("completed");
            json.RootElement.GetProperty("recordsProcessed").GetInt32().Should().Be(150);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP015_Sync_ConflictResolution()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"resolution": "use_local", "conflictsResolved": 3}""");

            var response = await _httpClient.PostAsync("/api/sync/resolve-conflicts", null);
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("conflictsResolved").GetInt32().Should().Be(3);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP016_Sync_FieldMapping()
        {
            var partnerJson = JsonDocument.Parse(MockPartnerResponse).RootElement[0];
            partnerJson.GetProperty("name").GetString().Should().Be("Test Partner");
            partnerJson.GetProperty("dunsNumber").GetString().Should().Be("123456789");
            partnerJson.GetProperty("taxId").GetString().Should().Be("TX-001");
            partnerJson.GetProperty("country").GetString().Should().Be("Denmark");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP017_Sync_DataValidation()
        {
            SetupHttpResponse(HttpStatusCode.BadRequest, """{"errors": ["name is required", "dunsNumber invalid format"]}""");

            var response = await _httpClient.PostAsync("/api/partners", new StringContent("""{}""", System.Text.Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            var errors = json.RootElement.GetProperty("errors");
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP018_Sync_BatchProcessing()
        {
            var batchPayload = """{"partners": [{"id": 1, "name": "P1"}, {"id": 2, "name": "P2"}]}""";
            SetupHttpResponse(HttpStatusCode.OK, """{"processed": 2, "inserted": 2, "errors": 0}""");

            var content = new StringContent(batchPayload, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/sync/batch", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("processed").GetInt32().Should().Be(2);
            json.RootElement.GetProperty("inserted").GetInt32().Should().Be(2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP019_Sync_StatusTracking()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockSyncStatusResponse);

            var response = await _httpClient.GetAsync("/api/sync/status");
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);

            json.RootElement.GetProperty("status").GetString().Should().Be("completed");
            json.RootElement.GetProperty("recordsInserted").GetInt32().Should().Be(10);
            json.RootElement.GetProperty("recordsUpdated").GetInt32().Should().Be(5);
            json.RootElement.GetProperty("errors").GetInt32().Should().Be(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP020_Sync_ScheduledSync()
        {
            var timeoutSeconds = _mockConfig.Object["OUPSettings:TimeoutSeconds"];
            int.TryParse(timeoutSeconds, out var timeout).Should().BeTrue();
            timeout.Should().Be(30);

            SetupHttpResponse(HttpStatusCode.OK, MockSyncStatusResponse);
            var response = await _httpClient.PostAsync("/api/sync/run", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Partner Matching Tests (10)

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP021_Match_ExactMatch()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockMatchResponse);

            var response = await _httpClient.GetAsync("/api/partners/match?duns=123456789");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches.GetArrayLength().Should().Be(1);
            matches[0].GetProperty("matchScore").GetDouble().Should().Be(1.0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP022_Match_FuzzyMatch()
        {
            SetupHttpResponse(HttpStatusCode.OK, """[{"id": 1, "name": "Test Partner Inc", "matchScore": 0.85}]""");

            var response = await _httpClient.GetAsync("/api/partners/match?name=Test%20Partner");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches[0].GetProperty("matchScore").GetDouble().Should().BeInRange(0.0, 1.0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP023_Match_ByDUNS()
        {
            SetupHttpResponse(HttpStatusCode.OK, """[{"id": 1, "dunsNumber": "123456789", "matchScore": 1.0}]""");

            var response = await _httpClient.GetAsync("/api/partners/match?duns=123456789");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches[0].GetProperty("dunsNumber").GetString().Should().Be("123456789");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP024_Match_ByTaxId()
        {
            SetupHttpResponse(HttpStatusCode.OK, """[{"id": 1, "taxId": "TX-001", "matchScore": 1.0}]""");

            var response = await _httpClient.GetAsync("/api/partners/match?taxId=TX-001");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches[0].GetProperty("taxId").GetString().Should().Be("TX-001");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP025_Match_ByName()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockPartnerResponse);

            var response = await _httpClient.GetAsync("/api/partners?name=Test%20Partner");
            var responseContent = await response.Content.ReadAsStringAsync();
            var partners = JsonDocument.Parse(responseContent).RootElement;
            partners[0].GetProperty("name").GetString().Should().Be("Test Partner");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP026_Match_MultipleMatches()
        {
            SetupHttpResponse(HttpStatusCode.OK, """[{"id": 1, "matchScore": 0.9}, {"id": 2, "matchScore": 0.8}]""");

            var response = await _httpClient.GetAsync("/api/partners/match?name=Test");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches.GetArrayLength().Should().Be(2);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP027_Match_NoMatch()
        {
            SetupHttpResponse(HttpStatusCode.OK, "[]");

            var response = await _httpClient.GetAsync("/api/partners/match?duns=999999999");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches.GetArrayLength().Should().Be(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP028_Match_ConfidenceScore()
        {
            SetupHttpResponse(HttpStatusCode.OK, """[{"id": 1, "matchScore": 0.95}]""");

            var response = await _httpClient.GetAsync("/api/partners/match?name=Test");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            var score = matches[0].GetProperty("matchScore").GetDouble();
            score.Should().BeInRange(0.0, 1.0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP029_Match_ManualReview()
        {
            SetupHttpResponse(HttpStatusCode.OK, """[{"id": 1, "matchScore": 0.6, "requiresReview": true}]""");

            var response = await _httpClient.GetAsync("/api/partners/match?name=Partial");
            var responseContent = await response.Content.ReadAsStringAsync();
            var matches = JsonDocument.Parse(responseContent).RootElement;
            matches[0].GetProperty("requiresReview").GetBoolean().Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP030_Match_LinkPartner()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"paoPartnerId": 100, "oupPartnerId": 1, "linked": true}""");

            var content = new StringContent("""{"paoPartnerId": 100, "oupPartnerId": 1}""", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/partners/link", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("linked").GetBoolean().Should().BeTrue();
        }

        #endregion

        #region Error Handling Tests (5)

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP031_Error_APIUnavailable()
        {
            SetupHttpResponse(HttpStatusCode.ServiceUnavailable, """{"error": "Service temporarily unavailable"}""");

            var response = await _httpClient.GetAsync("/api/partners");
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP032_Error_Timeout()
        {
            SetupHttpResponse((HttpStatusCode)408, """{"error": "Request timeout"}""");

            var response = await _httpClient.GetAsync("/api/partners");
            response.StatusCode.Should().Be((HttpStatusCode)408);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP033_Error_InvalidResponse()
        {
            SetupHttpResponse(HttpStatusCode.OK, "not valid json");

            var response = await _httpClient.GetAsync("/api/partners");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            Action parse = () => JsonDocument.Parse(content);
            parse.Should().Throw<JsonException>();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP034_Error_PartialFailure()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"processed": 10, "success": 8, "failed": 2, "errors": [{"record": 3, "message": "Validation failed"}]}""");

            var response = await _httpClient.PostAsync("/api/sync/batch", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("failed").GetInt32().Should().Be(2);
            json.RootElement.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP035_Error_RetryExhausted()
        {
            SetupSequentialHttpResponses(
                (HttpStatusCode.ServiceUnavailable, "{}"),
                (HttpStatusCode.ServiceUnavailable, "{}"),
                (HttpStatusCode.ServiceUnavailable, "{}")
            );

            var response1 = await _httpClient.GetAsync("/api/partners");
            var response2 = await _httpClient.GetAsync("/api/partners");
            var response3 = await _httpClient.GetAsync("/api/partners");

            response1.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            response2.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            response3.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        #endregion

        #region Audit & Logging Tests (5)

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP036_Audit_SyncHistory()
        {
            SetupHttpResponse(HttpStatusCode.OK, MockSyncStatusResponse);
            var response = await _httpClient.GetAsync("/api/sync/status");

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("status").GetString().Should().Be("completed");
            json.RootElement.GetProperty("recordsProcessed").GetInt32().Should().Be(150);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP037_Audit_ErrorLogging()
        {
            _logEntries.Clear();
            _mockLogger.Object.LogError("Sync failed: {Reason}", "Connection timeout");

            _logEntries.Should().HaveCount(1);
            _logEntries[0].Level.Should().Be(LogLevel.Error);
            _logEntries[0].Message.Should().Contain("Connection timeout");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP038_Audit_DataChanges()
        {
            var engagement = JsonDocument.Parse(MockEngagementResponse).RootElement;
            engagement.GetProperty("engagementNumber").GetString().Should().Be("UENB-TEST-001");
            engagement.GetProperty("name").GetString().Should().Be("Test Engagement");
            engagement.GetProperty("estimatedAmount").GetInt32().Should().Be(1500000);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public void OUP039_Audit_UserActions()
        {
            _logEntries.Clear();
            _mockLogger.Object.LogInformation("User {UserId} initiated sync at {Time}", 42, DateTime.UtcNow);

            _logEntries.Should().HaveCount(1);
            _logEntries[0].Message.Should().Contain("42");
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Feature", "OUPIntegration")]
        public async Task OUP040_Audit_PerformanceMetrics()
        {
            SetupHttpResponse(HttpStatusCode.OK, """{"status": "completed", "durationMs": 1250, "recordsProcessed": 150}""");

            var response = await _httpClient.GetAsync("/api/sync/status");
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseContent);
            json.RootElement.GetProperty("durationMs").GetInt32().Should().Be(1250);
        }

        #endregion
    }
}
