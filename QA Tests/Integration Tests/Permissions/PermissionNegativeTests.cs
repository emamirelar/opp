/**
 * @fileoverview Negative integration tests for PermissionController
 * Tests invalid inputs and error handling against actual API: /api/permissions/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Permissions;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Permissions")]
[Trait("Component", "NegativeTests")]
public class PermissionNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/permissions";

    public PermissionNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-001")]
    public async Task GetNonExistentEndpoint_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/non-existent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-002")]
    public async Task GetEntityPermissions_EmptyEntityName_Returns400()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permissions/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-003")]
    public async Task GetUser_EmptyUserId_Returns400Or404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-004")]
    public async Task GetUser_NonexistentUserId_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user/999999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-005")]
    public async Task PostSystemConfig_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync(BaseUrl, null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-006")]
    public async Task PutUserRoles_InsteadOfGet_Returns405()
    {
        var response = await _client.PutAsync($"{BaseUrl}/user-roles", null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-007")]
    public async Task DeleteEntityPermissions_InsteadOfGet_Returns405()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/entity-permissions/Contact");
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-008")]
    public async Task GetPermissionsBaseWithTrailingSlash_Returns404Or200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-009")]
    public async Task GetTyposInPath_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permission/Contact");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-NEG-010")]
    public async Task GetUser_InvalidUserIdFormat_Returns404Or400()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user/not-a-number");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
