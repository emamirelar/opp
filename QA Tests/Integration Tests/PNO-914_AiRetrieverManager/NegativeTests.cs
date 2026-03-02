/**
 * @fileoverview PNO-914 AiRetrieverManager negative tests.
 * Invalid input, null requests, and expected error handling.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.AI;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.AiRetriever;

[Collection("PNO914AiRetriever_Collection")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-AiRetriever")]
public class AiRetrieverNegativeTests : AiRetrieverTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-001")]
    public async Task SearchVectorStoreAsync_NullRequest_Throws()
    {
        var manager = CreateManager();

        var act = () => manager.SearchVectorStoreAsync(null!);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-002")]
    public async Task ConvertUrlAsync_EmptyUrl_ThrowsDueToInfrastructure()
    {
        var manager = CreateManager();

        var act = () => manager.ConvertUrlAsync("");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-003")]
    public async Task ConvertMarkdownToGoogleDocAsync_NullMarkdown_Throws()
    {
        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync(null!);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-004")]
    public async Task SearchVectorStoreAsync_WithoutCloudCredentials_ThrowsOnAuth()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var act = () => manager.SearchVectorStoreAsync(request);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-005")]
    public async Task ConvertUrlAsync_WithoutCloudCredentials_ThrowsOnAuth()
    {
        var manager = CreateManager();

        var act = () => manager.ConvertUrlAsync("https://example.com");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-006")]
    public async Task ConvertMarkdownToGoogleDocAsync_WithoutCloudCredentials_ThrowsOnAuth()
    {
        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-007")]
    public async Task SearchVectorStoreAsync_WithUserEmail_StillThrowsWithoutCredentials()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var act = () => manager.SearchVectorStoreAsync(request, "user@unops.org");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-008")]
    public async Task ConvertUrlAsync_WithUserEmail_StillThrowsWithoutCredentials()
    {
        var manager = CreateManager();

        var act = () => manager.ConvertUrlAsync("https://example.com", "user@unops.org");

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-NEG-009")]
    public async Task ConvertMarkdownToGoogleDocAsync_WithUserEmail_StillThrowsWithoutCredentials()
    {
        var manager = CreateManager();

        var act = () => manager.ConvertMarkdownToGoogleDocAsync("# Test", "user@unops.org");

        await act.Should().ThrowAsync<Exception>();
    }
}
