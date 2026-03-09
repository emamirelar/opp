/**
 * @fileoverview Admin Access Validation xUnit collection definition.
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.AdminAccessValidation;

[CollectionDefinition("Admin Access Validation Integration")]
public class AdminAccessValidationIntegrationCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}
