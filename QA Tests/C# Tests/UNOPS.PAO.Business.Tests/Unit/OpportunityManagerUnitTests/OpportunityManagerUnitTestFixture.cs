/**
 * @fileoverview Fixture for OpportunityManager unit tests.
 * Reuses OpportunityManagerTestFixtureBase for DB setup and seed helpers.
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerUnitTests;

using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

/// <summary>
/// Fixture for OpportunityManagerUnitTests.
/// Inherits from OpportunityManagerTestFixtureBase to reuse DB setup and seed helpers.
/// Provides TestableOpportunityManager for testing protected immutability helpers.
/// Exposes Manager, Context, and seed methods for tests using IClassFixture.
/// </summary>
public class OpportunityManagerUnitTestFixture : OpportunityManagerTestFixtureBase
{
    private TestableOpportunityManager? _testableManager;

    /// <summary>
    /// Gets the OpportunityManager for tests.
    /// </summary>
    public OpportunityManager Manager => base.Manager;

    /// <summary>
    /// Gets the UNOPSAppDbContext for tests.
    /// </summary>
    public new UNOPSAppDbContext Context => base.Context;

    /// <summary>
    /// Gets a TestableOpportunityManager that exposes protected methods for unit testing.
    /// </summary>
    public TestableOpportunityManager TestableManager =>
        _testableManager ??= new TestableOpportunityManager(Mapper, Context);

    /// <summary>
    /// Gets an Opportunity entity by ID directly from the context (bypasses IsDeleted filter).
    /// Used to verify soft delete behavior.
    /// </summary>
    public async Task<OpportunityEntity?> GetOpportunityEntityDirectlyAsync(int id)
    {
        return await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Seeds a minimal opportunity. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedOpportunityAsync(string? stage = "IDENTIFY & PROFILE", bool isDeleted = false) =>
        base.SeedOpportunityAsync(stage, isDeleted);

    /// <summary>
    /// Seeds an immutable opportunity. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedImmutableOpportunityAsync(string stage = "GO") =>
        base.SeedImmutableOpportunityAsync(stage);

    /// <summary>
    /// Seeds a currency. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedCurrencyAsync(string code = "USD") =>
        base.SeedCurrencyAsync(code);

    /// <summary>
    /// Seeds a partner. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedPartnerAsync() =>
        base.SeedPartnerAsync();

    /// <summary>
    /// Seeds an org unit. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedOrgUnitAsync() =>
        base.SeedOrgUnitAsync();

    /// <summary>
    /// Seeds an initiative type. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedInitiativeTypeAsync() =>
        base.SeedInitiativeTypeAsync();

    /// <summary>
    /// Seeds an SDG. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedSDGAsync(string sdgId = "1", string name = "No Poverty") =>
        base.SeedSDGAsync(sdgId, name);

    /// <summary>
    /// Seeds a country. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedCountryAsync(string iso2 = "XX", string name = "Test Country") =>
        base.SeedCountryAsync(iso2, name);

    /// <summary>
    /// Seeds an output. Public wrapper for tests.
    /// </summary>
    public new Task<int> SeedOutputAsync() =>
        base.SeedOutputAsync();
}
