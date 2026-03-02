using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.AI
{
    /// <summary>
    /// Integration tests for AI Entity Metadata Lookup functionality.
    /// Tests the new get_json_for_entity tool and AI agent's ability to use it.
    /// NOTE: These tests require the AI service to be running and may be skipped in CI/CD.
    /// </summary>
    [Collection("Integration Tests")]
    public class AIEntityMetadataIntegrationTests
    {
        private readonly PAOWebApplicationFactory<Program> _factory;

        public AIEntityMetadataIntegrationTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact(Skip = "Requires AI service running - start with: cd UNOPS.PAO.AIService && uvicorn main:app --reload")]
        public async Task AIAgent_AsksForOpportunityDetails_ProvidesMetadata()
        {
            // Arrange: Create HTTP client
            var client = _factory.CreateAuthenticatedClient();

            // Create a chat request asking about Opportunity entity
            var chatRequest = new
            {
                message = "Tell me about the Opportunity entity and its fields",
                sessionId = Guid.NewGuid().ToString()
            };

            // Act: Send request to AI chat endpoint
            var response = await client.PostAsJsonAsync("/api/ai/chat", chatRequest);

            // Assert: Response should be successful
            response.Should().NotBeNull();
            response.IsSuccessStatusCode.Should().BeTrue("AI chat endpoint should respond successfully");

            // Read response
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty("AI should provide a response");
            
            // Response should mention Opportunity
            content.Should().Contain("Opportunity", "AI should mention the Opportunity entity");
        }

        [Fact(Skip = "Requires AI service running - start with: cd UNOPS.PAO.AIService && uvicorn main:app --reload")]
        public async Task AIAgent_AsksForSpecificEndpoint_ProvidesEndpointDetails()
        {
            // Arrange: Create HTTP client
            var client = _factory.CreateAuthenticatedClient();

            // Create a chat request asking about specific endpoint
            var chatRequest = new
            {
                message = "How do I create an opportunity? What endpoint should I use?",
                sessionId = Guid.NewGuid().ToString()
            };

            // Act: Send request to AI chat endpoint
            var response = await client.PostAsJsonAsync("/api/ai/chat", chatRequest);

            // Assert: Response should be successful
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            
            // AI should provide information about the create endpoint
            content.Should().Contain("create", "AI should mention creating opportunities");
        }

        [Fact(Skip = "Requires AI service running - start with: cd UNOPS.PAO.AIService && uvicorn main:app --reload")]
        public async Task AIAgent_AsksAboutNonExistentEntity_HandlesGracefully()
        {
            // Arrange: Create HTTP client
            var client = _factory.CreateAuthenticatedClient();

            // Create a chat request about non-existent entity
            var chatRequest = new
            {
                message = "Tell me about the NonExistentEntity",
                sessionId = Guid.NewGuid().ToString()
            };

            // Act: Send request to AI chat endpoint
            var response = await client.PostAsJsonAsync("/api/ai/chat", chatRequest);

            // Assert: Response should still be successful (AI handles it gracefully)
            response.IsSuccessStatusCode.Should().BeTrue();

            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty("AI should provide some response");
            
            // AI should indicate the entity doesn't exist or ask for clarification
            (content.Contains("not found") || 
             content.Contains("don't have") || 
             content.Contains("clarify")).Should().BeTrue(
                "AI should indicate entity not found or ask for clarification");
        }
    }
}
