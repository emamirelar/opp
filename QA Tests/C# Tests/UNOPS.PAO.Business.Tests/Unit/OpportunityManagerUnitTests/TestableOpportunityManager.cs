/**
 * @fileoverview Testable subclass of OpportunityManager that exposes protected methods for unit testing.
 * Used to test IsOpportunityImmutable and ThrowIfImmutable logic in isolation.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerUnitTests;

/// <summary>
/// Testable OpportunityManager that exposes protected immutability helpers for unit testing.
/// </summary>
public class TestableOpportunityManager : OpportunityManager
{
    public TestableOpportunityManager(IMapper mapper, AppDbContext context)
        : base(mapper, context)
    {
    }

    /// <summary>
    /// Exposes IsOpportunityImmutable(Opportunity) for unit testing.
    /// </summary>
    public bool IsOpportunityImmutablePublic(UNOPS.PAO.Domain.Entities.Opportunity? opportunity) => IsOpportunityImmutable(opportunity);

    /// <summary>
    /// Exposes IsOpportunityImmutable(string) for unit testing.
    /// </summary>
    public bool IsOpportunityImmutablePublic(string? stage) => IsOpportunityImmutable(stage);

    /// <summary>
    /// Exposes ThrowIfImmutable for unit testing.
    /// </summary>
    public void ThrowIfImmutablePublic(UNOPS.PAO.Domain.Entities.Opportunity opportunity) => ThrowIfImmutable(opportunity);
}
