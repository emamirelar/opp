using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers
{
    /// <summary>
    /// Tests for PartnerLiaisonOffice management — CANCELLED.
    /// 
    /// Per developer clarification (Anusha, 2026-03-04):
    /// LiaisonOffice does NOT have a dedicated manager. LiaisonOffice is not a managed entity
    /// in Opp+; it can only be selected as part of a Partner (via LiaisonOfficeId FK).
    /// 
    /// DEF-013 closed as Won't Fix — no PartnerLiaisonOfficeManager is needed.
    /// QA-044 closed — 9 tests cancelled (not blocked).
    /// 
    /// LiaisonOffice data is served through:
    /// - ValuesManager.GetLiaisonOffices() (lookup/dropdown)
    /// - LiaisonOfficeController GET/search endpoints
    /// - Partner.LiaisonOfficeId FK selection
    /// </summary>
    public class PartnerLiaisonOfficeManagerTests
    {
        [Fact(Skip = "CANCELLED: LiaisonOffice does not have a dedicated manager by design — selected as part of Partner only (DEF-013 closed Won't Fix)")]
        public void TestSuite_Cancelled_LiaisonOfficeNotManagedEntity()
        {
            // All 9 original tests cancelled per developer clarification.
            // LiaisonOffice is a lookup entity, not a managed entity.
            // See ValuesManagerPerformanceTests and LiaisonOfficeControllerTests for actual coverage.
        }
    }
}

