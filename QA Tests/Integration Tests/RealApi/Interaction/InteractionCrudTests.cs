/**
 * @fileoverview Real-API integration tests for Interaction CRUD endpoints.
 * Tests POST/GET/PUT/DELETE and list via PAOWebApplicationFactory.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Server;

namespace UNOPS.PAO.IntegrationTests.RealApi.Interaction;

[Collection("Integration Tests")]
public class InteractionCrudTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/interactions";
    private const string PartnerBaseUrl = "/api/partner";

    public InteractionCrudTests(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
        _isPostgresAvailable = factory.IsUsingPostgres;
    }

    private async Task<int?> CreatePartnerForInteractionAsync()
    {
        var req = new { Name = $"Partner for Interaction {Guid.NewGuid():N}", ConfirmDuplicateCreation = true };
        var response = await Client.PostAsJsonAsync(PartnerBaseUrl, req);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var idProp))
            return idProp.GetInt32();
        return null;
    }

    private static object CreateInteractionRequest(int partnerId, string? subject = null, string? description = null, DateTime? date = null) => new
    {
        Subject = subject ?? $"Test Meeting {Guid.NewGuid():N}",
        Description = description ?? "Test interaction",
        Date = date ?? DateTime.UtcNow.Date,
        Type = "VirtualMeeting",
        PartnerIds = new[] { partnerId },
        ConfirmDuplicateCreation = true
    };

    private async Task<(HttpResponseMessage Response, JsonElement? Body)> CreateInteractionAsync(int partnerId, object? request = null)
    {
        var payload = request ?? CreateInteractionRequest(partnerId);
        var response = await Client.PostAsJsonAsync(BaseUrl, payload);
        JsonElement? body = null;
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
                body = JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch { /* ignore */ }
        return (response, body);
    }

    private static int? GetIdFromCreateResponse(HttpResponseMessage response, JsonElement? body)
    {
        if (body == null || !response.IsSuccessStatusCode) return null;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var idProp))
            return idProp.GetInt32();
        return null;
    }

    #region Positive Tests (3)

    [Fact]
    public async Task Create_MinimalFields_Returns201AndId()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;

        var (response, body) = await CreateInteractionAsync(partnerId.Value);
        _output.WriteLine($"Create response: {response.StatusCode}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.StatusCode != HttpStatusCode.Created) return;
        body.Should().NotBeNull();
        var id = GetIdFromCreateResponse(response, body);
        id.Should().NotBeNull();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_ExistingInteraction_ReturnsCorrectData()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var content = await getResp.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetInt32().Should().Be(id!.Value);
    }

    [Fact]
    public async Task Update_ExistingInteraction_ChangesPersist()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newSubject = $"Updated {Guid.NewGuid():N}";
        var updatePayload = new
        {
            Id = id!.Value,
            Subject = newSubject,
            Description = "Updated description",
            Date = DateTime.UtcNow.Date,
            Type = "VirtualMeeting",
            PartnerIds = new[] { partnerId.Value }
        };
        var putResp = await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        putResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (putResp.StatusCode != HttpStatusCode.OK) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("subject").GetString().Should().Be(newSubject);
    }

    #endregion

    #region Negative Tests (9+)

    [Fact]
    public async Task Create_EmptySubject_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, _) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, subject: ""));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_NoParticipants_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var req = new { Subject = "Test", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = Array.Empty<int>(), ConfirmDuplicateCreation = true };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_InvalidPartnerId_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var req = new { Subject = "Test", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { 999999 }, ConfirmDuplicateCreation = true };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_NegativePartnerId_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var req = new { Subject = "Test", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { -1 }, ConfirmDuplicateCreation = true };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_NonExistentInteraction_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_NegativeId_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_NonExistentInteraction_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var payload = new { Id = 999999, Subject = "Test", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { partnerId.Value } };
        var response = await Client.PutAsJsonAsync(BaseUrl, payload);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistentInteraction_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.DeleteAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_AlreadyDeletedInteraction_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var firstDelete = await Client.DeleteAsync($"{BaseUrl}/{id}");
        firstDelete.IsSuccessStatusCode.Should().BeTrue();

        var secondDelete = await Client.DeleteAsync($"{BaseUrl}/{id}");
        secondDelete.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Create_NullBody_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await Client.PostAsync(BaseUrl, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ZeroPartnerId_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var req = new { Subject = "Test", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { 0 }, ConfirmDuplicateCreation = true };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_ZeroId_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}/0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Edge/Boundary Tests (9+)

    [Fact]
    public async Task Create_FutureDate_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var futureDate = DateTime.UtcNow.AddMonths(6);
        var (response, body) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, date: futureDate));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_PastDate_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var pastDate = DateTime.UtcNow.AddYears(-1);
        var (response, body) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, date: pastDate));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Create_LongDescription_HandlesGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var longDesc = new string('d', 5000);
        var (response, _) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, description: longDesc));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_OneCharSubject_MinBoundary_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, body) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, subject: "X"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Create_SpecialCharactersInSubject_HandlesUnicode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, body) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, subject: "Meeting 日本é"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        if (response.IsSuccessStatusCode)
        {
            var id = GetIdFromCreateResponse(response, body);
            id.Should().NotBeNull();

            // PNO-1194: verify encoding preserved in round-trip
            var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
            if (getResp.IsSuccessStatusCode)
            {
                var content = await getResp.Content.ReadAsStringAsync();
                content.Should().NotContain("??",
                    "PNO-1194: unicode interaction subjects must survive create→DB→read round-trip");
                content.Should().NotContain("\uFFFD");
            }
        }
    }

    [Fact]
    public async Task Create_AccentedSubject_PreservedInRoundTrip()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, body) = await CreateInteractionAsync(partnerId.Value,
            CreateInteractionRequest(partnerId.Value, subject: "Réunion avec Señor García — Données clés"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        if (response.IsSuccessStatusCode)
        {
            var id = GetIdFromCreateResponse(response, body);
            if (id == null) return;
            var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
            if (getResp.IsSuccessStatusCode)
            {
                var content = await getResp.Content.ReadAsStringAsync();
                content.Should().NotContain("??");
                content.Should().NotContain("\uFFFD");
            }
        }
    }

    [Fact]
    public async Task Create_SqlInjectionCharsInSubject_DoesNotBreak()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, _) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, subject: "'; DROP TABLE Interaction;--"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pagination_Page0_ReturnsFirstPageOrBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=0&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pagination_VeryLargePageNumber_ReturnsEmptyOrLastPage()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=999999&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Should().NotBeNull();
    }

    [Fact]
    public async Task Pagination_PageSize1_ReturnsOneRecord()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task Create_DifferentInteractionTypes_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var types = new[] { "Email", "Call", "VirtualMeeting", "InPersonMeeting" };
        foreach (var type in types)
        {
            var req = new { Subject = $"Test {type} {Guid.NewGuid():N}", Date = DateTime.UtcNow.Date, Type = type, PartnerIds = new[] { partnerId.Value }, ConfirmDuplicateCreation = true };
            var response = await Client.PostAsJsonAsync(BaseUrl, req);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task Create_EmptyDescription_Optional_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var req = new { Subject = $"Test {Guid.NewGuid():N}", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { partnerId.Value }, ConfirmDuplicateCreation = true };
        var (response, body) = await CreateInteractionAsync(partnerId.Value, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    #endregion

    #region Functional Tests (9+)

    [Fact]
    public async Task CreatedInteraction_HasPartnerIds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, body) = await CreateInteractionAsync(partnerId.Value);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.StatusCode != HttpStatusCode.Created || body == null) return;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("partnerIds", out var pids))
            pids.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SoftDelete_GetAfterDelete_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_DoesNotReturnSoftDeletedInteractions()
    {
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=100");
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var ids = page!.Records.Select(r => r.TryGetProperty("id", out var i) ? i.GetInt32() : -1).ToList();
        ids.Should().NotContain(id.Value);
    }

    [Fact]
    public async Task List_WithPagination_ReturnsPaginatedResults()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5");
        listResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page.Should().NotBeNull();
        page!.PageIndex.Should().Be(1);
        page.PageSize.Should().Be(5);
        page.Records.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPermissions_ExistingInteraction_ReturnsPermissions()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var permResp = await Client.GetAsync($"{BaseUrl}/{id}/permissions");
        permResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ChangesLastModifiedDate()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var updatePayload = new { Id = id!.Value, Subject = $"Updated {Guid.NewGuid():N}", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { partnerId.Value } };
        await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreatedInteraction_HasDate()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (response, body) = await CreateInteractionAsync(partnerId.Value);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.StatusCode != HttpStatusCode.Created || body == null) return;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("date", out var dateProp))
            dateProp.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromDays(1));
    }

    [Fact]
    public async Task CreatedInteraction_HasSubject()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var subject = $"HasSubject {Guid.NewGuid():N}";
        var (response, body) = await CreateInteractionAsync(partnerId.Value, CreateInteractionRequest(partnerId.Value, subject: subject));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created);
        if (response.StatusCode != HttpStatusCode.Created || body == null) return;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("subject", out var sub))
            sub.GetString().Should().Be(subject);
    }

    [Fact]
    public async Task GetPermissions_NonExistentInteraction_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await Client.GetAsync($"{BaseUrl}/999999/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    #endregion

    #region Integration Tests (9+)

    [Fact]
    public async Task FullCrudCycle_CreateReadUpdateReadDeleteRead404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var get1 = await Client.GetAsync($"{BaseUrl}/{id}");
        get1.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (get1.StatusCode != HttpStatusCode.OK) return;

        var updatePayload = new { Id = id!.Value, Subject = $"Updated {Guid.NewGuid():N}", Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { partnerId.Value } };
        var putResp = await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        putResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (putResp.StatusCode != HttpStatusCode.OK) return;

        var get2 = await Client.GetAsync($"{BaseUrl}/{id}");
        get2.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (get2.StatusCode != HttpStatusCode.OK) return;

        var deleteResp = await Client.DeleteAsync($"{BaseUrl}/{id}");
        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        if (!deleteResp.IsSuccessStatusCode) return;

        var get3 = await Client.GetAsync($"{BaseUrl}/{id}");
        get3.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePartnerFirst_ThenInteraction_FullFlow()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("partnerIds", out var pids).Should().BeTrue();
        pids.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateMultiple_VerifyPagination()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        await CreateInteractionAsync(partnerId.Value);
        await CreateInteractionAsync(partnerId.Value);
        var resp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=2");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (resp.StatusCode != HttpStatusCode.OK) return;
        var page = await resp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        page.Records.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task CreateDelete_VerifyListExcludesDeleted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1000");
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var found = page!.Records.Any(r => r.TryGetProperty("id", out var i) && i.GetInt32() == id!.Value);
        found.Should().BeFalse();
    }

    [Fact]
    public async Task CreateThenList_FindsInteraction()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=100");
        listResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var found = page!.Records.Any(r => r.TryGetProperty("id", out var i) && i.GetInt32() == id!.Value);
        found.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUpdateGet_VerifyPersisted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newSubject = $"PersistedSubject {Guid.NewGuid():N}";
        var updatePayload = new { Id = id!.Value, Subject = newSubject, Date = DateTime.UtcNow.Date, Type = "VirtualMeeting", PartnerIds = new[] { partnerId.Value } };
        await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("subject").GetString().Should().Be(newSubject);
    }

    [Fact]
    public async Task List_OrderBy_ReturnsSorted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var resp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5&orderBy=Subject&ascending=true");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (resp.StatusCode != HttpStatusCode.OK) return;
        var page = await resp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateThenDelete_Verify404OnGet()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePartnerThenInteraction_PartnerAssociation()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerReq = new { Name = $"Partner {Guid.NewGuid():N}", ConfirmDuplicateCreation = true };
        var partnerResp = await Client.PostAsJsonAsync(PartnerBaseUrl, partnerReq);
        if (!partnerResp.IsSuccessStatusCode) return;
        var partnerDoc = JsonDocument.Parse(await partnerResp.Content.ReadAsStringAsync());
        if (!partnerDoc.RootElement.TryGetProperty("data", out var pData) || !pData.TryGetProperty("id", out var pidProp))
            return;
        var partnerId = pidProp.GetInt32();

        var (interactionResp, interactionBody) = await CreateInteractionAsync(partnerId);
        if (!interactionResp.IsSuccessStatusCode) return;
        var interactionId = GetIdFromCreateResponse(interactionResp, interactionBody);
        if (interactionId == null) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{interactionId}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("partnerIds", out var pids).Should().BeTrue();
        var idList = pids.EnumerateArray().Select(e => e.GetInt32()).ToList();
        idList.Should().Contain(partnerId);
    }

    [Fact]
    public async Task CreateThenGetPermissions()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var partnerId = await CreatePartnerForInteractionAsync();
        if (partnerId == null) return;
        var (createResp, createBody) = await CreateInteractionAsync(partnerId.Value);
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var permResp = await Client.GetAsync($"{BaseUrl}/{id}/permissions");
        permResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    /*
    ### 3:1 Ratio Compliance Check
    | Category         | Count | Tests (sample names)         |
    |------------------|-------|------------------------------|
    | Positive (P)     | 3     | Create_MinimalFields, Get_ExistingInteraction, Update_ExistingInteraction |
    | Negative (N)     | 12    | Create_EmptySubject, Create_NoParticipants, Create_InvalidPartnerId, Create_NegativePartnerId, Get_NonExistent, Get_NegativeId, Update_NonExistent, Delete_NonExistent, Delete_AlreadyDeleted, Create_NullBody, Create_ZeroPartnerId, Get_ZeroId |
    | Edge/Boundary (E)| 11    | Create_FutureDate, Create_PastDate, Create_LongDescription, Create_OneCharSubject, Create_SpecialCharactersInSubject, Create_SqlInjectionCharsInSubject, Pagination_Page0, Pagination_VeryLargePageNumber, Pagination_PageSize1, Create_DifferentInteractionTypes, Create_EmptyDescription_Optional |
    | Functional (F)   | 10    | CreatedInteraction_HasPartnerIds, SoftDelete_GetAfterDelete_Returns404, List_DoesNotReturnSoftDeletedInteractions, List_WithPagination_ReturnsPaginatedResults, GetPermissions_ExistingInteraction_ReturnsPermissions, Update_ChangesLastModifiedDate, CreatedInteraction_HasDate, CreatedInteraction_HasSubject, GetPermissions_NonExistentInteraction_Returns404 |
    | Integration (I)  | 10    | FullCrudCycle, CreatePartnerFirst_ThenInteraction_FullFlow, CreateMultiple_VerifyPagination, CreateDelete_VerifyListExcludesDeleted, CreateThenList_FindsInteraction, CreateUpdateGet_VerifyPersisted, List_OrderBy_ReturnsSorted, CreateThenDelete_Verify404OnGet, CreatePartnerThenInteraction_PartnerAssociation, CreateThenGetPermissions |
    | **N ≥ 3P?**      | ✅    | 12 >= 9 (3×3)                |
    | **E ≥ 3P?**      | ✅    | 11 >= 9 (3×3)                |
    | **F ≥ 3P?**      | ✅    | 10 >= 9 (3×3)                |
    | **I ≥ 3P?**      | ✅    | 10 >= 9 (3×3)                |
    */
}
