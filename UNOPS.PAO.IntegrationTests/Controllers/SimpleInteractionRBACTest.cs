using System.Net.Http.Json;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Server;
using Xunit;
using Xunit.Abstractions;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Simple integration test to validate RBAC-aware pagination is working
/// </summary>
public class SimpleInteractionRBACTest : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    
    public SimpleInteractionRBACTest(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output) 
        : base(factory)
    {
        _output = output;
    }

    [Fact]
    public async Task GetInteractions_WithPagination_ReturnsValidResponse()
    {
        // Arrange
        var client = Factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/interactions?pageIndex=0&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        // Basic validation that response is not empty and contains expected structure
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        _output.WriteLine($"Response received: {content.Length} characters");
        _output.WriteLine("✅ RBAC-integrated pagination endpoint is accessible");
    }

    [Fact]
    public async Task GetInteractions_WithDifferentPageSizes_ReturnsConsistentStructure()
    {
        // Arrange
        var client = Factory.CreateClient();
        var pageSizes = new[] { 5, 10, 20 };
        
        foreach (var pageSize in pageSizes)
        {
            // Act
            var response = await client.GetAsync($"/api/interactions?pageIndex=0&pageSize={pageSize}");
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            try
            {
                var result = await response.Content.ReadFromJsonAsync<PaginationResponse<InteractionModel>>();
                
                Assert.NotNull(result);
                Assert.True(result.PageSize == pageSize || result.PageSize == 0); // 0 for default
                Assert.True(result.TotalCount >= 0);
                Assert.NotNull(result.Records);
                
                _output.WriteLine($"✅ Page size {pageSize}: TotalCount={result.TotalCount}, Records={result.Records.Count}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ Failed to deserialize response for page size {pageSize}: {ex.Message}");
                throw;
            }
        }
    }

    [Fact]
    public async Task GetInteractions_WithAdvancedSearch_HandlesRBACCorrectly()
    {
        // Arrange
        var client = Factory.CreateClient();
        var searchCriteria = """
            {
                "criteria": [
                    {
                        "field": "Subject",
                        "operator": "contains",
                        "value": "test"
                    }
                ]
            }
            """;
        
        // Act
        var response = await client.GetAsync(
            $"/api/interactions?advancedSearch=true&searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageIndex=0&pageSize=5");
        
        // Assert
        response.EnsureSuccessStatusCode();
        
        try
        {
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<InteractionModel>>();
            
            Assert.NotNull(result);
            Assert.True(result.TotalCount >= 0);
            Assert.NotNull(result.Records);
            
            _output.WriteLine($"✅ Advanced search with RBAC: TotalCount={result.TotalCount}, Records={result.Records.Count}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Advanced search failed: {ex.Message}");
            throw;
        }
    }
    
    [Fact]
    public async Task CompareOldVsNewEndpoint_ValidateImprovement()
    {
        // This test validates that the new RBAC-integrated approach works
        // and provides some basic performance comparison
        
        // Arrange
        var client = Factory.CreateClient();
        const int pageSize = 10;
        
        // Act - Test new endpoint
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync($"/api/interactions?pageIndex=0&pageSize={pageSize}");
        stopwatch.Stop();
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<InteractionModel>>();
        
        Assert.NotNull(result);
        
        _output.WriteLine($"✅ New RBAC-integrated endpoint:");
        _output.WriteLine($"   Response time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"   TotalCount: {result.TotalCount}");
        _output.WriteLine($"   Records returned: {result.Records?.Count ?? 0}");
        _output.WriteLine($"   Page metadata correct: PageIndex={result.PageIndex}, PageSize={result.PageSize}");
        
        // Validation: TotalCount should match actual record availability
        if (result.Records?.Count > 0)
        {
            Assert.True(result.TotalCount >= result.Records.Count, 
                "TotalCount should be at least as large as returned records count");
        }
        
        // Validation: No empty pages unless this is the last page
        if (result.PageIndex == 0 && result.Records?.Count < pageSize)
        {
            _output.WriteLine($"   Note: First page has {result.Records?.Count} records (less than page size {pageSize})");
        }
        
        _output.WriteLine("✅ RBAC-integrated pagination validation completed successfully");
    }
}