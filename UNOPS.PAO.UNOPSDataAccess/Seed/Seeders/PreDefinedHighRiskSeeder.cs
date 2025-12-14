using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds PreDefinedHighRisks from oUP High Risk Checklist (EAC questions)
/// Data sourced from risk_questions.csv - 17 items from the ticket
/// </summary>
public class PreDefinedHighRiskSeeder
{
    public static async Task SeedPreDefinedHighRisksAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding PreDefined High Risks (from oUP EAC questions)...");

        if (await context.PreDefinedHighRisks.AnyAsync())
        {
            Console.WriteLine("  ⏭️  PreDefinedHighRisks already exist. Skipping seed.");
            return;
        }

        // Get Risk Category lookup for FK references (Level 3 categories)
        var categoryLookup = await context.RiskCategories
            .Where(c => c.Level == 3)
            .ToDictionaryAsync(c => c.ShortCode, c => c.Id);

        // High Risk Checklist items from risk_questions.csv
        var highRisks = new List<PreDefinedHighRisk>
        {
            // 1.1.1 - Legal and regulatory framework
            new PreDefinedHighRisk
            {
                Name = "No Host Country Agreement",
                CategoryCode = "LEGAL_REGUL_FRWRK_OP",
                Level1 = 1,
                Level2Code = "1.1",
                OupQuestionId = 476,
                Code = "1.1.1",
                DisplayCode = "1.1.1",
                Description = "In the territory(ies) of operation, there is 1) no UNOPS-specific host country agreement; 2) no exchange of letters providing that a UNDP Standard Basic Assistance Agreement (SBAA) or other United Nations host country agreement applies mutatis mutandis to UNOPS; or 3) no Status of Forces Agreement (SOFA) or Status of Mission Agreement (SOMA) between the United Nations and the host country that fully covers UNOPS and its activities.",
                ShortTitle = "No Host Country Agreement",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 1,
                RiskCategoryId = categoryLookup.GetValueOrDefault("LEGAL_REGUL_FRWRK_OP"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.2.1 - Security issues
            new PreDefinedHighRisk
            {
                Name = "High-Risk Security Issues",
                CategoryCode = "REGIONAL_LOCAL_INSTABILIT",
                Level1 = 1,
                Level2Code = "1.2",
                OupQuestionId = 415,
                Code = "1.2.1",
                DisplayCode = "1.2.1",
                Description = "High-risk security issues exist, for which a United Nations Security Management System (UNSMS) Programme Criticality Assessment requires the approval of the UNOPS Executive Director, or the security measures that are in place are not in line with, or may not be sufficiently mitigated by, the in-country UNSMS.",
                ShortTitle = "High-Risk Security Issues / Armed Conflict",
                IsAutoDetectable = true,
                DetectionRuleType = "COUNTRY_FRAGILE",
                DisplayOrder = 2,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REGIONAL_LOCAL_INSTABILIT"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.3.1 - New funding source or client
            new PreDefinedHighRisk
            {
                Name = "New Funding Source or Client",
                CategoryCode = "RELATIONSHIP_MANAGEMENT",
                Level1 = 1,
                Level2Code = "1.3",
                OupQuestionId = 92,
                Code = "1.3.1",
                DisplayCode = "1.3.1",
                Description = "The funding source or client is new.",
                ShortTitle = "New Funding Source or Client",
                IsAutoDetectable = true,
                DetectionRuleType = "PARTNER_DRAFT",
                DisplayOrder = 3,
                RiskCategoryId = categoryLookup.GetValueOrDefault("RELATIONSHIP_MANAGEMENT"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.4.1 - Scope outside mandate
            new PreDefinedHighRisk
            {
                Name = "Scope Outside UNOPS Mandate",
                CategoryCode = "REP_ALGN_MAND_VAL",
                Level1 = 1,
                Level2Code = "1.4",
                OupQuestionId = 93,
                Code = "1.4.1",
                DisplayCode = "1.4.1",
                Description = "The scope is not aligned with or is outside the UNOPS mandate.",
                ShortTitle = "Scope Outside UNOPS Mandate",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 4,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REP_ALGN_MAND_VAL"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.4.2 - Non-UN security forces
            new PreDefinedHighRisk
            {
                Name = "Support to Non-UN Security Forces",
                CategoryCode = "REP_ALGN_MAND_VAL",
                Level1 = 1,
                Level2Code = "1.4",
                OupQuestionId = 94,
                Code = "1.4.2",
                DisplayCode = "1.4.2",
                Description = "The project involves providing support to non-United Nations security forces.",
                ShortTitle = "Support to Non-UN Security Forces",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 5,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REP_ALGN_MAND_VAL"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.4.3 - Conflict of interest
            new PreDefinedHighRisk
            {
                Name = "Conflict of Interest",
                CategoryCode = "REP_ALGN_MAND_VAL",
                Level1 = 1,
                Level2Code = "1.4",
                OupQuestionId = 477,
                Code = "1.4.3",
                DisplayCode = "1.4.3",
                Description = "There is a conflict of interest or the risk of a real or perceived conflict of interest, and there is no internal or other relevant stakeholder clearance.",
                ShortTitle = "Conflict of Interest",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 6,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REP_ALGN_MAND_VAL"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.4.4 - Reputational risk
            new PreDefinedHighRisk
            {
                Name = "Reputational Risk",
                CategoryCode = "REP_ALGN_MAND_VAL",
                Level1 = 1,
                Level2Code = "1.4",
                OupQuestionId = 478,
                Code = "1.4.4",
                DisplayCode = "1.4.4",
                Description = "There is a reputational risk from real or perceived concerns of potential serious negative reputational exposure for UNOPS, and/or the project poses significant ethical concerns.",
                ShortTitle = "Reputational Risk",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 7,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REP_ALGN_MAND_VAL"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.4.5 - CPI below 50
            new PreDefinedHighRisk
            {
                Name = "Pre-selection by Government with CPI < 50",
                CategoryCode = "REP_ALGN_MAND_VAL",
                Level1 = 1,
                Level2Code = "1.4",
                OupQuestionId = 479,
                Code = "1.4.5",
                DisplayCode = "1.4.5",
                Description = "The project involves pre-selection by governments with a Corruption Perception Index (CPI) score below 50.",
                ShortTitle = "Pre-selection by Government with CPI < 50",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 8,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REP_ALGN_MAND_VAL"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 1.4.6 - Pay agent services
            new PreDefinedHighRisk
            {
                Name = "Pay Agent Services to Third Parties",
                CategoryCode = "REP_ALGN_MAND_VAL",
                Level1 = 1,
                Level2Code = "1.4",
                OupQuestionId = 515,
                Code = "1.4.6",
                DisplayCode = "1.4.6",
                Description = "The project(s) involve providing pay agent services to third parties.",
                ShortTitle = "Pay Agent Services to Third Parties",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 9,
                RiskCategoryId = categoryLookup.GetValueOrDefault("REP_ALGN_MAND_VAL"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 2.1.1 - SDG impact
            // Note: Mapped to ENV_CLIMATE_CHANGE as the original category "SCL_CLTR_ENV_CLMT_ECO" doesn't exist
            // This covers the sustainability/environmental dimensions mentioned in the description
            new PreDefinedHighRisk
            {
                Name = "Negative SDG Impact",
                CategoryCode = "ENV_CLIMATE_CHANGE",
                Level1 = 2,
                Level2Code = "2.1",
                OupQuestionId = 481,
                Code = "2.1.1",
                DisplayCode = "2.1.1",
                Description = "There is a risk of significant negative impact(s) within the social, environmental and/or economic dimensions of the Sustainable Development Goals, which has been identified as part of the Social and Environmental Screening process.",
                ShortTitle = "Negative SDG Impact (Social/Environmental/Economic)",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 10,
                RiskCategoryId = categoryLookup.GetValueOrDefault("ENV_CLIMATE_CHANGE"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 2.2.1 / 2.4.1 - Grants to for-profit
            new PreDefinedHighRisk
            {
                Name = "Grants to For-Profit Entities",
                CategoryCode = "GRNT_OTR_NON_PROC_STRAT",
                Level1 = 2,
                Level2Code = "2.2",
                OupQuestionId = 413,
                Code = "2.2.1",
                DisplayCode = "2.4.1",
                Description = "The project involves providing grants to for-profit entities or individuals.",
                ShortTitle = "Grants to For-Profit Entities or Individuals",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 11,
                RiskCategoryId = categoryLookup.GetValueOrDefault("GRNT_OTR_NON_PROC_STRAT"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 2.3.1 / 2.5.1 - IT security
            new PreDefinedHighRisk
            {
                Name = "IT Security and Privacy Risks",
                CategoryCode = "CYBERSEC_DATA_PROTECT",
                Level1 = 2,
                Level2Code = "2.3",
                OupQuestionId = 138,
                Code = "2.3.1",
                DisplayCode = "2.5.1",
                Description = "The project involves the creation or use of information technology (IT) products with information security and privacy risks that may compromise the reputation and/or security of UNOPS and/or its affiliates.",
                ShortTitle = "IT Security and Privacy Risks",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 12,
                RiskCategoryId = categoryLookup.GetValueOrDefault("CYBERSEC_DATA_PROTECT"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 3.1.1 - Exceeds $100M
            new PreDefinedHighRisk
            {
                Name = "Engagement Exceeds $100 Million",
                CategoryCode = "ENG_COST_PRICE",
                Level1 = 3,
                Level2Code = "3.1",
                OupQuestionId = 513,
                Code = "3.1.1",
                DisplayCode = "3.1.1",
                Description = "The annual value of engagement addition signed under an individual engagement exceeds US$100 million.",
                ShortTitle = "Engagement Exceeds $100 Million",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 13,
                RiskCategoryId = categoryLookup.GetValueOrDefault("ENG_COST_PRICE"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 3.1.2 - Pricing policy deviation
            new PreDefinedHighRisk
            {
                Name = "Pricing Policy Deviation",
                CategoryCode = "ENG_COST_PRICE",
                Level1 = 3,
                Level2Code = "3.1",
                OupQuestionId = 514,
                Code = "3.1.2",
                DisplayCode = "3.1.2",
                Description = "The engagement: 1) is subject to the Pricing Policy and deviates from the fee-setting; 2) is subject to a previous delegated authority or Chief Financial Officer decision and deviates from the fee agreed to in this decision; 3) is subject to a memorandum of understanding (MoU), hosting agreement, or programmatic agreement that prescribes the fee and deviates from this fee; and/or 4) is subject to an engagement amendment and deviates from the original fee setting as a percentage over direct cost.",
                ShortTitle = "Pricing Policy Deviation",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 14,
                RiskCategoryId = categoryLookup.GetValueOrDefault("ENG_COST_PRICE"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 3.2.1 - Currency exchange risk
            new PreDefinedHighRisk
            {
                Name = "Currency Exchange Risk",
                CategoryCode = "EXCHANGE_RATE_FOR_CONTRIB",
                Level1 = 3,
                Level2Code = "3.2",
                OupQuestionId = 101,
                Code = "3.2.1",
                DisplayCode = "3.2.1",
                Description = "The engagement exposes UNOPS to significant currency exchange risk.",
                ShortTitle = "Currency Exchange Risk",
                IsAutoDetectable = true,
                DetectionRuleType = "NON_USD_CURRENCY",
                DisplayOrder = 15,
                RiskCategoryId = categoryLookup.GetValueOrDefault("EXCHANGE_RATE_FOR_CONTRIB"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 3.3.1 - Implementation timing
            new PreDefinedHighRisk
            {
                Name = "Implementation Before/After Legal Agreement",
                CategoryCode = "OVR_INELIGB_PRJT_EXP_CONT",
                Level1 = 3,
                Level2Code = "3.3",
                OupQuestionId = 376,
                Code = "3.3.1",
                DisplayCode = "3.3.1",
                Description = "The implementation of the project begins before signing the legal agreement or continues after the end date identified in the legal agreement before signing an amendment to the legal agreement.",
                ShortTitle = "Implementation Before/After Legal Agreement",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 16,
                RiskCategoryId = categoryLookup.GetValueOrDefault("OVR_INELIGB_PRJT_EXP_CONT"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            },

            // 4.1.1 - Other high risks
            new PreDefinedHighRisk
            {
                Name = "Other Undefined High Risks",
                CategoryCode = "OTHER_PROCESS_OPS_RISKS",
                Level1 = 4,
                Level2Code = "4.1",
                OupQuestionId = 103,
                Code = "4.1.1",
                DisplayCode = "4.1.1",
                Description = "There are other high risks that have not been defined in this list of high risks, which may result in significant and/or organization-wide consequences.",
                ShortTitle = "Other Undefined High Risks",
                IsAutoDetectable = false,
                DetectionRuleType = null,
                DisplayOrder = 17,
                RiskCategoryId = categoryLookup.GetValueOrDefault("OTHER_PROCESS_OPS_RISKS"),
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            }
        };

        await context.PreDefinedHighRisks.AddRangeAsync(highRisks);
        await context.SaveChangesAsync();

        Console.WriteLine($"  ✅ Seeded {highRisks.Count} PreDefined High Risks");
        Console.WriteLine("✅ PreDefined High Risks seeding completed\n");
    }
}

