using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.Models;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

public class SimplePartnerControllerTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SimplePartnerControllerTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        // Set up authentication header for test
        _client.DefaultRequestHeaders.Add("X-Goog-IAP-JWT-Assertion", "test-jwt-token");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", TestAuthenticationHandler.TestUserEmail);
    }

    [Fact]
    public async Task GetAll_WithoutParameters_Should_ReturnOkResult()
    {
        // Act
        var response = await _client.GetAsync("/api/partner");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_Should_ReturnEmptyPaginatedResult()
    {
        // Arrange - Make sure database is clean
        await CleanDatabaseAsync();

        // Act
        var response = await _client.GetAsync("/api/partner");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeEmpty();
        
        // The response should be a valid JSON that can be deserialized
        var result = System.Text.Json.JsonSerializer.Deserialize<PaginationResponse<PartnerModel>>(content, 
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        
        result.Should().NotBeNull();
        result!.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WithSampleData_Should_ReturnPartnersCount()
    {
        // Arrange
        await CleanDatabaseAsync();
        await SeedSamplePartnersAsync(3);

        // Act
        var response = await _client.GetAsync("/api/partner");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<PaginationResponse<PartnerModel>>(content, 
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        
        result.Should().NotBeNull();
        result!.Records.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAll_WithPagination_Should_RespectPageSize()
    {
        // Arrange
        await CleanDatabaseAsync();
        await SeedSamplePartnersAsync(5);

        // Act
        var response = await _client.GetAsync("/api/partner?pageSize=2&pageIndex=1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<PaginationResponse<PartnerModel>>(content, 
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        
        result.Should().NotBeNull();
        result!.Records.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageSize.Should().Be(2);
        result.PageIndex.Should().Be(1);
    }

    // Helper methods
    private async Task CleanDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
        
        dbContext.Partners.RemoveRange(dbContext.Partners);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedSamplePartnersAsync(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(count);

        dbContext.Partners.AddRange(partners);
        await dbContext.SaveChangesAsync();
    }
}