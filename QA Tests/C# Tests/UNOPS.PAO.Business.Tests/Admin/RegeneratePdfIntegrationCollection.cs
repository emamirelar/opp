/**
 * @fileoverview PNO-1166 xUnit collection definition for RegenerateGoOpportunityPdfs tests.
 * Provides PAOWebApplicationFactory fixture within Business.Tests assembly.
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Collection definition for PNO-1166 RegenerateGoOpportunityPdfs integration tests.
/// Ensures fixture is available when running Business.Tests in isolation.
/// </summary>
[CollectionDefinition("PNO-1166 Integration")]
public class PNO1166IntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
