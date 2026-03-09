using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers
{
    /// <summary>
    /// Tests for PartnerFocalPoint management — CANCELLED.
    /// 
    /// Per developer clarification (Anusha, 2026-03-04):
    /// FocalPoint does NOT have a dedicated manager. FocalPoint is not a managed entity
    /// in Opp+; it can only be selected as part of a Partner (via PartnerFocalPointUserId FK).
    /// 
    /// DEF-014 closed as Won't Fix — no PartnerFocalPointManager is needed.
    /// QA-045 closed — 12 tests cancelled (not blocked).
    /// 
    /// FocalPoint data is served through:
    /// - Partner.PartnerFocalPointUserId FK selection
    /// - Contact role "Focal Point" (EntityRole, tested in ContactFunctionalTests)
    /// - Partner analytics includeFocalPoint filter
    /// </summary>
    public class PartnerFocalPointManagerTests
    {
        [Fact(Skip = "CANCELLED: FocalPoint does not have a dedicated manager by design — selected as part of Partner only (DEF-014 closed Won't Fix)")]
        public void TestSuite_Cancelled_FocalPointNotManagedEntity()
        {
            // All 12 original tests cancelled per developer clarification.
            // FocalPoint is a user FK on Partner, not a managed entity.
            // See ContactFunctionalTests for "Focal Point" role coverage.
        }
    }
}

