/**
 * @fileoverview Opportunity UX & Layout xUnit collection definition.
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityUXAndLayout;

[CollectionDefinition("Opportunity UX And Layout Integration")]
public class OpportunityUXAndLayoutIntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
