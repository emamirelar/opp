/**
 * @fileoverview Opportunity Creation xUnit collection definition.
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityCreation;

[CollectionDefinition("Opportunity Creation Integration")]
public class OpportunityCreationIntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
