/**
 * @fileoverview Opportunity WHY Section — Impact & Strategic Alignment Specification
 * Consolidated requirements from PNO-692, PNO-817, PNO-886
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Specification and requirement traceability for WHY - Impact &amp; Strategic Alignment section.
///
/// JIRA Tickets:
/// - PNO-692: WHY Section core feature (AC1–AC10)
/// - PNO-817: SDG details fixes (SDG 8, 15.3, Main/Cross-cutting naming, default, optional targets)
/// - PNO-886: UAT consolidated feedback (section header, Expected Impact, UNOPS Missions, etc.)
///
/// Requirements validated:
/// - AC1: Section "WHY - Impact &amp; Strategic Alignment" exists
/// - AC2: Context and challenge(s) subsection with description field
/// - AC3: Partner Results Framework upload with partner association (tag document against partner)
/// - AC4: SDG Alignment: 1 Main + N cross-cutting SDGs mandatory before Go
/// - AC5: For each SDG: optional selection of targets/indicators (or opt out once per opportunity)
/// - AC6: UNSDCF alignment per implementation country; alert if no countries
/// - AC7: At least 1 UNSDCF Outcome required per country before Go
/// - AC8: UNSDCF Outcomes aligned with SDGs indicated
/// - AC9: UNSDCF version change handling (inactive framework notification)
/// - AC10: Humanitarian/Peace/Security framework per country
/// - PNO-817: SDG 8 present; SDG 15.3 references 2030 not 2020; Main/Cross-cutting naming; default Main; optional targets
/// - PNO-886: Section header "Why - Impact &amp; Strategic Alignment"; Expected Impact field; UNOPS Strategic Missions
/// </summary>
public static class OpportunityWhySectionSpec
{
    /// <summary>Section identifier for WHY - Impact &amp; Strategic Alignment</summary>
    public const string SectionId = "why";

    /// <summary>Max length for Context/Challenges field</summary>
    public const int ChallengesMaxLength = 1020;

    /// <summary>Max length for Expected Impact (PNO-886)</summary>
    public const int ExpectedImpactMaxLength = 510;

    /// <summary>Max length for Expected Outcomes</summary>
    public const int ExpectedOutcomesMaxLength = 510;

    /// <summary>Max length for Expected Beneficiaries (PNO-886: 1000 chars)</summary>
    public const int ExpectedBeneficiariesMaxLength = 1000;

    /// <summary>SDG classification: Main (formerly Primary)</summary>
    public const string SdgClassificationMain = "Main";

    /// <summary>SDG classification: Cross-cutting (formerly Secondary)</summary>
    public const string SdgClassificationCrossCutting = "Cross-cutting";

    /// <summary>SDG 8 must be present in dropdown (PNO-817)</summary>
    public const string Sdg8Id = "8";

    /// <summary>SDG 15.3 target year (PNO-817: 2030 not 2020)</summary>
    public const int Sdg153TargetYear = 2030;
}
