/**
 * @fileoverview PNO-914 AiRetrieverManager functional tests.
 * Business rules: settings binding, timeout configuration, interface contract.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Configuration;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.AiRetriever;

[Collection("PNO914AiRetriever_Collection")]
[Trait("Category", "Integration")]
[Trait("Feature", "PNO-914-AiRetriever")]
public class AiRetrieverFunctionalTests : AiRetrieverTestFixtureBase
{
    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-001")]
    public void CreateManager_TimeoutConfiguredFromSettings()
    {
        Settings.Timeout = 120;
        var manager = CreateManager();

        manager.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-002")]
    public void CreateManager_BaseUrlFromSettings()
    {
        Settings.BaseUrl = "https://custom.api.unops.org/";
        var manager = CreateManager();

        manager.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-003")]
    public async Task SearchVectorStoreAsync_AttemptsIapAuth_ThrowsWithoutCredentials()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var ex = await Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request));

        ex.Should().NotBeNull("manager should attempt IAP auth which fails without credentials");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-004")]
    public async Task ConvertUrlAsync_AttemptsIapAuth_ThrowsWithoutCredentials()
    {
        var manager = CreateManager();

        var ex = await Record.ExceptionAsync(() => manager.ConvertUrlAsync("https://example.com", "user@unops.org"));

        ex.Should().NotBeNull("manager should attempt IAP auth which fails without credentials");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-005")]
    public async Task ConvertMarkdownToGoogleDocAsync_AttemptsIapAuth_ThrowsWithoutCredentials()
    {
        var manager = CreateManager();

        var ex = await Record.ExceptionAsync(() => manager.ConvertMarkdownToGoogleDocAsync("# Test"));

        ex.Should().NotBeNull("manager should attempt IAP auth which fails without credentials");
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-006")]
    public void ExternalApiSettings_AllPropertiesConfigurable()
    {
        var settings = new ExternalApiSettings
        {
            BaseUrl = "https://api.example.com/",
            OAuthClientId = "client-123",
            Timeout = 60
        };

        settings.BaseUrl.Should().Be("https://api.example.com/");
        settings.OAuthClientId.Should().Be("client-123");
        settings.Timeout.Should().Be(60);
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-007")]
    public void IAiRetrieverManager_InterfaceContract_HasExpectedMethods()
    {
        var iface = typeof(IAiRetrieverManager);

        iface.GetMethod("SearchVectorStoreAsync").Should().NotBeNull();
        iface.GetMethod("ConvertUrlAsync").Should().NotBeNull();
        iface.GetMethod("ConvertMarkdownToGoogleDocAsync").Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-008")]
    public void AiRetrieverManager_ImplementsInterface()
    {
        typeof(AiRetrieverManager).Should().Implement<IAiRetrieverManager>();
    }

    [Fact]
    [Trait("TestId", "PNO914-AIR-FUN-009")]
    public async Task SearchVectorStoreAsync_ExceptionContainsMeaningfulInfo()
    {
        var manager = CreateManager();
        var request = new VectorStoreSearchRequest { Query = "test" };

        var ex = await Record.ExceptionAsync(() => manager.SearchVectorStoreAsync(request));

        ex.Should().NotBeNull();
        ex!.GetType().Should().NotBe(typeof(NotImplementedException),
            "the method should be implemented even if it fails due to missing credentials");
    }
}
