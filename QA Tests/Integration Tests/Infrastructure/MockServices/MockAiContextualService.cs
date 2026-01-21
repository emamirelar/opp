using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Managers;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Mock implementation of AiContextualService for testing without Vertex AI
/// </summary>
public class MockAiContextualService
{
    private readonly UNOPSAppDbContext _context;
    private readonly IConfiguration _configuration;

    public MockAiContextualService(IConfiguration configuration, UNOPSAppDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // Mock methods that return empty/default results
    public Task<List<int>> GetSimilarPartnersAsync(string searchText, int limit = 10)
    {
        return Task.FromResult(new List<int>());
    }

    public Task<List<int>> GetSimilarContactsAsync(string searchText, int limit = 10)
    {
        return Task.FromResult(new List<int>());
    }

    public Task<float[]> GetEmbeddingAsync(string text)
    {
        return Task.FromResult(new float[768]); // Return empty embedding vector
    }
}

