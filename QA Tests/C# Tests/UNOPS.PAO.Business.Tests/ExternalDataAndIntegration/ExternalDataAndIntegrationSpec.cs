/**
 * @fileoverview Specification for External Data Service & Gmail Integration test suite
 * Covers PNO-1164 (External Data Service) and PNO-1169 (Gmail Addon Test Environment).
 *
 * Requirements validated:
 * - PNO-1164: EDS scheduled jobs, master data sync (user info, entity roles, org units)
 * - PNO-1164: Country data, SDG data, UNSDCF data from ValuesManager
 * - PNO-1169: Gmail addon test environment, email-to-opportunity flow
 *
 * @author QA Team
 * @since 2026-03-09
 */

namespace UNOPS.PAO.Business.Tests.ExternalDataAndIntegration;

/// <summary>
/// Specification constants for External Data & Gmail Integration tests.
/// PNO-1164: External Data Service | PNO-1169: Gmail Addon Test Environment
/// </summary>
public static class ExternalDataAndIntegrationSpec
{
    /// <summary>PNO-1164: EDS fetches and caches external reference data</summary>
    public const string EDS_CacheExternalData = "EDS_CacheExternalData";

    /// <summary>PNO-1164: Country data (SIDS, Fragile State, HCA) from external source</summary>
    public const string EDS_CountryIndicators = "EDS_CountryIndicators";

    /// <summary>PNO-1164: SDG goals, targets, indicators from external source</summary>
    public const string EDS_SDGData = "EDS_SDGData";

    /// <summary>PNO-1164: UNSDCF frameworks, outcomes from external source</summary>
    public const string EDS_UNSDCFData = "EDS_UNSDCFData";

    /// <summary>PNO-1169: Gmail addon test environment setup</summary>
    public const string Gmail_TestEnvironment = "Gmail_TestEnvironment";

    /// <summary>PNO-1169: Email-to-opportunity flow</summary>
    public const string Gmail_EmailToOpportunity = "Gmail_EmailToOpportunity";

    /// <summary>Data synchronization and caching</summary>
    public const string DataSyncCaching = "DataSyncCaching";

    /// <summary>Country artifact types for SIDS/Fragile (PNO-775)</summary>
    public static class CountryArtifactTypes
    {
        public const string SIDS = "SIDS";
        public const string WorldBankFragileSituation = "World_Bank_Fragile_Situation";
        public const string HCA = "HCA";
    }
}
