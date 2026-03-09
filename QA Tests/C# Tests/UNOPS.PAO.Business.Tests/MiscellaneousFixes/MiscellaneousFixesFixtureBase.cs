/// <summary>
/// Base fixture for Miscellaneous Fixes tests (PNO-805, PNO-801).
/// Reuses OpportunityAIFeaturesFixtureBase for UNOPSOpportunityManager and seeded data.
/// </summary>

using UNOPS.PAO.Business.Tests.OpportunityAIFeatures;

namespace UNOPS.PAO.Business.Tests.MiscellaneousFixes;

public abstract class MiscellaneousFixesFixtureBase : OpportunityAIFeaturesFixtureBase
{
    // Inherits all setup from OpportunityAIFeaturesFixtureBase:
    // - Manager (UNOPSOpportunityManager)
    // - PaoUserId, PartnerId, CurrencyId, CountryId, OrgHierarchyId, ProposedInitiativeTypeId
    // - CreateTestOpportunityAsync helper
}
