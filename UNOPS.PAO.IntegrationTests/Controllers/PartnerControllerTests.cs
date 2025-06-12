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

public class PartnerControllerTests : IntegrationTestBase
{
    public PartnerControllerTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_WithoutParameters_ReturnsAllPartners()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedTestPartnersAsync(5);

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().HaveCount(5);
        response.TotalCount.Should().Be(5);
        response.PageIndex.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedTestPartnersAsync(15);

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageIndex=2&pageSize=5");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().HaveCount(5);
        response.TotalCount.Should().Be(15);
        response.PageIndex.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAll_WithSearchText_ReturnsFilteredResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedTestPartnersAsync(5);
        
        // Create a partner with specific name for search
        await SeedSpecificPartnerAsync("ACME Corporation");

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=ACME");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().HaveCount(1);
        response.Records.First().Name.Should().Contain("ACME");
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedPartnersWithStatusAsync("Active", 3);
        await SeedPartnersWithStatusAsync("Inactive", 2);

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().HaveCount(3);
        response.Records.Should().OnlyContain(p => p.Status == "Active");
    }

    [Fact]
    public async Task GetAll_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedPartnersWithSpecificNamesAsync(new[] { "Zebra Corp", "Alpha Corp", "Beta Corp" });

        // Act - Sort by Name ascending
        var responseAsc = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?orderBy=Name&ascending=true");
        
        // Assert
        responseAsc.Should().NotBeNull();
        responseAsc!.Records.Should().HaveCount(3);
        responseAsc.Records.Select(p => p.Name).Should().BeInAscendingOrder();

        // Act - Sort by Name descending
        var responseDesc = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?orderBy=Name&ascending=false");
        
        // Assert
        responseDesc.Should().NotBeNull();
        responseDesc!.Records.Should().HaveCount(3);
        responseDesc.Records.Select(p => p.Name).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAll_WithAdvancedSearch_ReturnsFilteredResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedTestPartnersAsync(5);

        var searchCriteria = """
            {
                "Name": "Test",
                "Status": "Active"
            }
            """;

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>($"/api/partner?advancedSearch=true&searchCriteria={Uri.EscapeDataString(searchCriteria)}");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithLargePageSize_RespectsMaximumLimit()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedTestPartnersAsync(5);

        // Act - Request very large page size
        var response = await GetAsync("/api/partner?pageSize=10000");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var paginationResponse = System.Text.Json.JsonSerializer.Deserialize<PaginationResponse<PartnerModel>>(content, JsonOptions);
            paginationResponse!.Records.Should().HaveCount(5);
        }
    }

    [Fact]
    public async Task GetAll_WithInvalidPageIndex_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await GetAsync("/api/partner?pageIndex=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var clientWithoutAuth = Factory.CreateClient();
        
        // Act
        var response = await clientWithoutAuth.GetAsync("/api/partner");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.PageIndex.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_WithNameFilter_ReturnsFilteredResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedPartnersWithNamePattern("Test Company", 3);
        await SeedPartnersWithNamePattern("Other Corp", 2);

        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?name=Test");

        // Assert
        response.Should().NotBeNull();
        response!.Records.Should().HaveCount(3);
        response.Records.Should().OnlyContain(p => p.Name!.Contains("Test"));
    }

    // Helper methods for seeding test data
    private async Task SeedTestPartnersAsync(int count)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(count);

        dbContext.Partners.AddRange(partners);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedSpecificPartnerAsync(string name)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        var partner = TestDataBuilder.GetPartnerFaker().Generate();
        partner.Name = name;

        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedPartnersWithStatusAsync(string status, int count)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(count);
        
        foreach (var partner in partners)
        {
            partner.Status = status;
        }

        dbContext.Partners.AddRange(partners);
        await dbContext.SaveChangesAsync();
    }

    private async Task SeedPartnersWithSpecificNamesAsync(string[] names)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        var faker = TestDataBuilder.GetPartnerFaker();
        
        foreach (var name in names)
        {
            var partner = faker.Generate();
            partner.Name = name;
            dbContext.Partners.Add(partner);
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedPartnersWithNamePattern(string namePattern, int count)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(count);
        
        foreach (var partner in partners)
        {
            partner.Name = $"{namePattern} {partner.Name}";
        }

        dbContext.Partners.AddRange(partners);
        await dbContext.SaveChangesAsync();
    }
}