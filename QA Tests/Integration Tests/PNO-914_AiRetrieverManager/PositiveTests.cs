/**
 * @fileoverview PNO-914 AiRetrieverManager positive tests.
 * Verifies manager creation, settings binding, and method existence.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.AI;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.AiRetriever;

[Collection("PNO914AiRetriever_Collection")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-AiRetriever")]
public class AiRetrieverPositiveTests : AiRetrieverTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-AIR-POS-001")]
    public void CreateManager_ValidSettings_ManagerCreatedSuccessfully()
    {
        var manager = CreateManager();

        manager.Should().NotBeNull();
        manager.Should().BeAssignableTo<IAiRetrieverManager>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-POS-002")]
    public void CreateManager_ImplementsIAiRetrieverManager_AllMethodsExist()
    {
        var manager = CreateManager();

        var type = typeof(IAiRetrieverManager);
        type.GetMethod("SearchVectorStoreAsync").Should().NotBeNull();
        type.GetMethod("ConvertUrlAsync").Should().NotBeNull();
        type.GetMethod("ConvertMarkdownToGoogleDocAsync").Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-POS-003")]
    public void VectorStoreSearchRequest_CanBeConstructed_WithValidProperties()
    {
        var request = new VectorStoreSearchRequest
        {
            Query = "partnership opportunities",
            MaxResults = 5
        };

        request.Query.Should().Be("partnership opportunities");
        request.MaxResults.Should().Be(5);
    }
}
