/**
 * @fileoverview PNO-697, PNO-775, PNO-776, PNO-778, PNO-895, PNO-935: Opportunity WHERE Section — Geographic Implementation specification.
 * Defines constants and expected behavior for the WHERE section (implementation countries).
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.OpportunityWhereSection;

/// <summary>
/// Specification for Opportunity WHERE Section (PNO-697 and related tickets).
/// AC1: WHERE section exists on opportunity record.
/// AC2: Identify one or more geographies (countries or region).
/// AC3: UNOPS Org Unit and Org Unit Type per country.
/// AC4: SIDS/Fragile State tags visible.
/// AC5: HCA status indicator per country.
/// AC6: Add/remove geography; multi-select; bulk delete.
/// AC7: UNSDCF notification for WHY section.
/// </summary>
public static class OpportunityWhereSectionSpec
{
    /// <summary>
    /// EntityArtifact type codes for country indicators (PNO-775).
    /// </summary>
    public static class CountryArtifactTypes
    {
        public const string SIDS = "SIDS";
        public const string WorldBankFragileSituation = "World_Bank_Fragile_Situation";
        public const string HostAgreement = "Host_Agreement";
        public const string UNRegion = "UN_Region";
        public const string UNSubRegion = "UN_Sub_Region";
        public const string UNOPSRegion = "UNOPS_Region";
    }

    /// <summary>
    /// OrganizationUnitRelationship entity type for Country.
    /// </summary>
    public const string CountryEntityType = "Country";

    /// <summary>
    /// Maximum length for SpecificAreas field (OpportunityCountry).
    /// </summary>
    public const int SpecificAreasMaxLength = 1000;

    /// <summary>
    /// Maximum length for ContextWarning field (OpportunityCountry).
    /// </summary>
    public const int ContextWarningMaxLength = 500;
}
