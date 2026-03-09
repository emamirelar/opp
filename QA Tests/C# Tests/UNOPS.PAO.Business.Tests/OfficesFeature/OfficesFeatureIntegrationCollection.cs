using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

/// <summary>
/// xUnit collection for Offices Feature integration tests (HTTP API).
/// </summary>
[CollectionDefinition("OfficesFeature Integration")]
public class OfficesFeatureIntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
