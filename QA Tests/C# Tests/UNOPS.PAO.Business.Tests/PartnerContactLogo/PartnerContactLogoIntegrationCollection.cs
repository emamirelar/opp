/**
 * @fileoverview Partner/Contact/Logo test collection definition.
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.PartnerContactLogo;

[CollectionDefinition("Partner Contact Logo Integration")]
public class PartnerContactLogoIntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
