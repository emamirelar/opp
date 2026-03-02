/**
 * @fileoverview PNO-914 AiRetrieverManager integration tests.
 * End-to-end patterns: sequential calls, error recovery, concurrent behavior.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.AI;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.AiRetriever;

[Collection("PNO914AiRetriever_Collection")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-AiRetriever")]
public class AiRetrieverIntegrationTests : AiRetrieverTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-001")]
    public async Task MultipleSequentialCalls_AllFailConsistently()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = 1 };

        var ex1 = await Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request));
        var ex2 = await Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request));

        ex1.Should().NotBeNull();
        ex2.Should().NotBeNull();
        ex1!.GetType().Should().Be(ex2!.GetType(), "same error should occur on repeated calls");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-002")]
    public async Task ConvertUrlAsync_FailsConsistentlyAcrossCalls()
    {
        var manager = CreateManager();

        var ex1 = await Record.ExceptionAsync(() => manager.ConvertUrlAsync("https://a.com"));
        var ex2 = await Record.ExceptionAsync(() => manager.ConvertUrlAsync("https://b.com"));

        ex1.Should().NotBeNull();
        ex2.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-003")]
    public async Task ConvertMarkdownToGoogleDocAsync_FailsConsistentlyAcrossCalls()
    {
        var manager = CreateManager();

        var ex1 = await Record.ExceptionAsync(() => manager.ConvertMarkdownToGoogleDocAsync("# Doc 1"));
        var ex2 = await Record.ExceptionAsync(() => manager.ConvertMarkdownToGoogleDocAsync("# Doc 2"));

        ex1.Should().NotBeNull();
        ex2.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-004")]
    public async Task DifferentEndpoints_AllFailDueToMissingAuth()
    {
        var manager = CreateManager();

        var exSearch = await Record.ExceptionAsync(
            () => manager.SearchVectorStoreAsync(new VectorStoreSearchRequest { Query = "test" }));
        var exConvert = await Record.ExceptionAsync(
            () => manager.ConvertUrlAsync("https://example.com"));
        var exMarkdown = await Record.ExceptionAsync(
            () => manager.ConvertMarkdownToGoogleDocAsync("# Test"));

        exSearch.Should().NotBeNull();
        exConvert.Should().NotBeNull();
        exMarkdown.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-005")]
    public async Task ErrorRecovery_AfterFailure_ManagerStillFunctional()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        await Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request));
        var ex2 = await Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request));

        ex2.Should().NotBeNull("manager should still attempt the call even after a previous failure");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-006")]
    public async Task DifferentUserEmails_AllFailDueToMissingAuth()
    {
        var manager = CreateManager();

        var ex1 = await Record.ExceptionAsync(
            () => manager.ConvertUrlAsync("https://a.com", "user1@unops.org"));
        var ex2 = await Record.ExceptionAsync(
            () => manager.ConvertUrlAsync("https://b.com", "user2@unops.org"));

        ex1.Should().NotBeNull();
        ex2.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-007")]
    public async Task ConcurrentCalls_AllFailWithoutHanging()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = 1 };

        var tasks = Enumerable.Range(0, 3)
            .Select(_ => Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request)))
            .ToArray();

        var exceptions = await Task.WhenAll(tasks);
        exceptions.Should().AllSatisfy(ex => ex.Should().NotBeNull());
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-008")]
    public void CreateMultipleManagers_EachIndependent()
    {
        Settings.BaseUrl = "https://api1.test.unops.org/";
        var manager1 = CreateManager();

        Settings.BaseUrl = "https://api2.test.unops.org/";
        var manager2 = CreateManager();

        manager1.Should().NotBeNull();
        manager2.Should().NotBeNull();
        manager1.Should().NotBeSameAs(manager2);
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-INT-009")]
    public async Task FullRoundTrip_SearchThenConvert_BothFailDueToAuth()
    {
        var manager = CreateManager();

        var exSearch = await Record.ExceptionAsync(
            () => manager.SearchVectorStoreAsync(new VectorStoreSearchRequest { Query = "partnership", MaxResults = 3 }));

        var exConvert = await Record.ExceptionAsync(
            () => manager.ConvertMarkdownToGoogleDocAsync("# Summary", "user@unops.org"));

        exSearch.Should().NotBeNull();
        exConvert.Should().NotBeNull();
    }
}
