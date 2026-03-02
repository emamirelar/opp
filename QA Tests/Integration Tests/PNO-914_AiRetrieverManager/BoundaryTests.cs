/**
 * @fileoverview PNO-914 AiRetrieverManager boundary tests.
 * Edge cases: settings configuration, URL formats, timeout values.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Models.AI;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.AiRetriever;

[Collection("PNO914AiRetriever_Collection")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-AiRetriever")]
public class AiRetrieverBoundaryTests : AiRetrieverTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-001")]
    public void CreateManager_MinimumTimeout_Initializes()
    {
        Settings.Timeout = 1;
        var manager = CreateManager();

        manager.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-002")]
    public void CreateManager_LargeTimeout_Initializes()
    {
        Settings.Timeout = 3600;
        var manager = CreateManager();

        manager.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-003")]
    public async Task SearchVectorStoreAsync_EmptyQuery_ThrowsOnAuthOrValidation()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "", MaxResults = 5 };

        var act = () => manager.SearchVectorStoreAsync(request);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-004")]
    public void VectorStoreSearchRequest_MaxResultsZero_CanBeConstructed()
    {
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = 0 };

        request.MaxResults.Should().Be(0);
        request.Query.Should().Be("test");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-005")]
    public void VectorStoreSearchRequest_VeryLongQuery_CanBeConstructed()
    {
        var longQuery = new string('x', 10000);
        var request = new VectorStoreSearchRequest { Query = longQuery, MaxResults = 5 };

        request.Query.Should().HaveLength(10000);
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-006")]
    public void VectorStoreSearchRequest_UnicodeQuery_CanBeConstructed()
    {
        var request = new VectorStoreSearchRequest { Query = "日本語 テスト 🎉", MaxResults = 5 };

        request.Query.Should().Contain("日本語");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-007")]
    public async Task SearchVectorStoreAsync_MaxResultsNegative_ThrowsOnAuthOrValidation()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test", MaxResults = -1 };

        var act = () => manager.SearchVectorStoreAsync(request);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-008")]
    public void CreateManager_BaseUrlWithTrailingSlash_Initializes()
    {
        Settings.BaseUrl = "https://api.test.unops.org/";
        var manager = CreateManager();

        manager.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-BND-009")]
    public void CreateManager_BaseUrlWithoutTrailingSlash_Initializes()
    {
        Settings.BaseUrl = "https://api.test.unops.org";
        var manager = CreateManager();

        manager.Should().NotBeNull();
    }
}
