/**
 * @fileoverview Related Docs & Search xUnit collection definition.
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.RelatedDocsAndSearch;

[CollectionDefinition("Related Docs And Search Integration")]
public class RelatedDocsAndSearchIntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
