using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds RiskCategories from oUP category hierarchy (3 levels)
/// Based on category_level_1.csv, category_level_2.csv, category_level_3.csv
/// </summary>
public class RiskCategorySeeder
{
    public static async Task SeedRiskCategoriesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Risk Categories (3-level hierarchy)...");

        if (await context.RiskCategories.AnyAsync())
        {
            Console.WriteLine("  ⏭️  RiskCategories already exist. Skipping seed.");
            return;
        }

        // Level 1 Categories (from category_level_1.csv)
        var level1Categories = new List<RiskCategory>
        {
            new RiskCategory { Code = "UPC1_FINANCE", ShortCode = "FINANCE", Name = "Finance", Level = 1, DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC1_PARTNERS_STAKEHOLDERS", ShortCode = "PARTNERS_STAKEHOLDERS", Name = "Partners & stakeholders", Level = 1, DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC1_PEOPLE", ShortCode = "PEOPLE", Name = "People", Level = 1, DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC1_PROCESS_OPERATIONS", ShortCode = "PROCESS_OPERATIONS", Name = "Process / Operations", Level = 1, DisplayOrder = 4, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow }
        };

        await context.RiskCategories.AddRangeAsync(level1Categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {level1Categories.Count} Level 1 categories");

        // Get Level 1 IDs for FK references
        var level1Lookup = await context.RiskCategories
            .Where(c => c.Level == 1)
            .ToDictionaryAsync(c => c.ShortCode, c => c.Id);

        // Level 2 Categories (from category_level_2.csv)
        var level2Categories = new List<RiskCategory>
        {
            // FINANCE sub-categories
            new RiskCategory { Code = "UPC2_CONTRIBUTIONS", ShortCode = "CONTRIBUTIONS", Name = "Contributions", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_CURR_EXCHANGE_RATE", ShortCode = "CURR_EXCHANGE_RATE", Name = "Currency and exchange rate", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_EXPENDITURE", ShortCode = "EXPENDITURE", Name = "Expenditure", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_ICT", ShortCode = "ICT", Name = "ICT", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 4, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_REPORTING_DATA", ShortCode = "REPORTING_DATA", Name = "Reporting and data", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 5, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_TREASURY", ShortCode = "TREASURY", Name = "Treasury", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 6, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_OTHER_FINANCE_RISKS", ShortCode = "OTHER_FINANCE_RISKS", Name = "Other finance risks", Level = 2, ParentShortCode = "FINANCE", ParentCategoryId = level1Lookup["FINANCE"], DisplayOrder = 7, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // PARTNERS_STAKEHOLDERS sub-categories
            new RiskCategory { Code = "UPC2_GEOP_ECON", ShortCode = "GEOP_ECON", Name = "Geopolitical / economic context", Level = 2, ParentShortCode = "PARTNERS_STAKEHOLDERS", ParentCategoryId = level1Lookup["PARTNERS_STAKEHOLDERS"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_LEGAL_COMP", ShortCode = "LEGAL_COMP", Name = "Legal and compliance", Level = 2, ParentShortCode = "PARTNERS_STAKEHOLDERS", ParentCategoryId = level1Lookup["PARTNERS_STAKEHOLDERS"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_PART_FUND", ShortCode = "PART_FUND", Name = "Partnership & funding landscape", Level = 2, ParentShortCode = "PARTNERS_STAKEHOLDERS", ParentCategoryId = level1Lookup["PARTNERS_STAKEHOLDERS"], DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_REL_SATISF", ShortCode = "REL_SATISF", Name = "Relations & satisfaction", Level = 2, ParentShortCode = "PARTNERS_STAKEHOLDERS", ParentCategoryId = level1Lookup["PARTNERS_STAKEHOLDERS"], DisplayOrder = 4, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_OTHER_PRT_RI", ShortCode = "OTHER_PRT_RI", Name = "Other partners & stakeholders risks", Level = 2, ParentShortCode = "PARTNERS_STAKEHOLDERS", ParentCategoryId = level1Lookup["PARTNERS_STAKEHOLDERS"], DisplayOrder = 5, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // PEOPLE sub-categories
            new RiskCategory { Code = "UPC2_CAPABILITIES_PERF_MGMGT", ShortCode = "CAPABILITIES_PERF_MGMGT", Name = "Capabilities and performance management", Level = 2, ParentShortCode = "PEOPLE", ParentCategoryId = level1Lookup["PEOPLE"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_RECRUITMENT_RETENTION", ShortCode = "RECRUITMENT_RETENTION", Name = "Recruitment & retention", Level = 2, ParentShortCode = "PEOPLE", ParentCategoryId = level1Lookup["PEOPLE"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_SAFETY_SECURITY", ShortCode = "SAFETY_SECURITY", Name = "Safety and security", Level = 2, ParentShortCode = "PEOPLE", ParentCategoryId = level1Lookup["PEOPLE"], DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_OTHER_PEOPLE_RISKS", ShortCode = "OTHER_PEOPLE_RISKS", Name = "Other people risks", Level = 2, ParentShortCode = "PEOPLE", ParentCategoryId = level1Lookup["PEOPLE"], DisplayOrder = 4, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // PROCESS_OPERATIONS sub-categories
            new RiskCategory { Code = "UPC2_FRAUD_ETHICS", ShortCode = "FRAUD_ETHICS", Name = "Fraud and ethics", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_HSSE_OFFICES", ShortCode = "HSSE_OFFICES", Name = "HSSE - Offices & connected workplaces", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_HSSE_SITES", ShortCode = "HSSE_SITES", Name = "HSSE - Sites & connected workplaces", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_INFRASTRUCTURE_OPERATIONS", ShortCode = "INFRASTRUCTURE_OPERATIONS", Name = "Infrastructure operations", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 4, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_NON_PROCUREMENT_FUND_DISB", ShortCode = "NON_PROCUREMENT_FUND_DISB", Name = "Non procurement fund disbursement", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 5, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_ORGANISATIONAL_SETTING", ShortCode = "ORGANISATIONAL_SETTING", Name = "Organisational setting", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 6, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_PROCUREMENT", ShortCode = "PROCUREMENT", Name = "Procurement", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 7, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_PROJECT_MANAGEMENT_DESI", ShortCode = "PROJECT_MANAGEMENT_DESI", Name = "Project Management & Design", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 8, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_PROJECT_SITE_OPERATIONS", ShortCode = "PROJECT_SITE_OPERATIONS", Name = "Project site operations", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 9, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_SUSTAINABILITY", ShortCode = "SUSTAINABILITY", Name = "Sustainability", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 10, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC2_OTHER_OPERATIONS_RISKS", ShortCode = "OTHER_OPERATIONS_RISKS", Name = "Other operations risks", Level = 2, ParentShortCode = "PROCESS_OPERATIONS", ParentCategoryId = level1Lookup["PROCESS_OPERATIONS"], DisplayOrder = 11, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow }
        };

        await context.RiskCategories.AddRangeAsync(level2Categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {level2Categories.Count} Level 2 categories");

        // Get Level 2 IDs for FK references
        var level2Lookup = await context.RiskCategories
            .Where(c => c.Level == 2)
            .ToDictionaryAsync(c => c.ShortCode, c => c.Id);

        // Level 3 Categories (leaf level - selectable by users)
        // Key categories needed for PreDefinedHighRisk mapping
        var level3Categories = new List<RiskCategory>
        {
            // Under CONTRIBUTIONS
            new RiskCategory { Code = "UPC3_ENG_COST_PRICE", ShortCode = "ENG_COST_PRICE", Name = "Engagement costing and pricing", Level = 3, ParentShortCode = "CONTRIBUTIONS", ParentCategoryId = level2Lookup["CONTRIBUTIONS"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_FND_RECEIPT_COLLECTION", ShortCode = "FND_RECEIPT_COLLECTION", Name = "Funding receipt and collection", Level = 3, ParentShortCode = "CONTRIBUTIONS", ParentCategoryId = level2Lookup["CONTRIBUTIONS"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under CURR_EXCHANGE_RATE
            new RiskCategory { Code = "UPC3_EXCHANGE_RATE_FOR_CONTRIB", ShortCode = "EXCHANGE_RATE_FOR_CONTRIB", Name = "Exchange rate for contributions", Level = 3, ParentShortCode = "CURR_EXCHANGE_RATE", ParentCategoryId = level2Lookup["CURR_EXCHANGE_RATE"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_EXCHANGE_RATE_FOR_EXPEND", ShortCode = "EXCHANGE_RATE_FOR_EXPEND", Name = "Exchange rate for expenditure", Level = 3, ParentShortCode = "CURR_EXCHANGE_RATE", ParentCategoryId = level2Lookup["CURR_EXCHANGE_RATE"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under EXPENDITURE
            new RiskCategory { Code = "UPC3_OVR_INELIGB_PRJT_EXP_CONT", ShortCode = "OVR_INELIGB_PRJT_EXP_CONT", Name = "Over/ineligible project expenditure and contingencies", Level = 3, ParentShortCode = "EXPENDITURE", ParentCategoryId = level2Lookup["EXPENDITURE"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under GEOP_ECON
            new RiskCategory { Code = "UPC3_REGIONAL_LOCAL_INSTABILIT", ShortCode = "REGIONAL_LOCAL_INSTABILIT", Name = "Regional/local instability and security", Level = 3, ParentShortCode = "GEOP_ECON", ParentCategoryId = level2Lookup["GEOP_ECON"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_ECONOMIC_MARKET_CONDIT", ShortCode = "ECONOMIC_MARKET_CONDIT", Name = "Economic and market conditions", Level = 3, ParentShortCode = "GEOP_ECON", ParentCategoryId = level2Lookup["GEOP_ECON"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under LEGAL_COMP
            new RiskCategory { Code = "UPC3_LEGAL_REGUL_FRWRK_OP", ShortCode = "LEGAL_REGUL_FRWRK_OP", Name = "Legal and regulatory framework to operate", Level = 3, ParentShortCode = "LEGAL_COMP", ParentCategoryId = level2Lookup["LEGAL_COMP"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_COMPLIANCE_INT_EXT", ShortCode = "COMPLIANCE_INT_EXT", Name = "Compliance with internal and external requirements", Level = 3, ParentShortCode = "LEGAL_COMP", ParentCategoryId = level2Lookup["LEGAL_COMP"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under REL_SATISF
            new RiskCategory { Code = "UPC3_RELATIONSHIP_MANAGEMENT", ShortCode = "RELATIONSHIP_MANAGEMENT", Name = "Relationship management", Level = 3, ParentShortCode = "REL_SATISF", ParentCategoryId = level2Lookup["REL_SATISF"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_REP_ALGN_MAND_VAL", ShortCode = "REP_ALGN_MAND_VAL", Name = "Reputation and alignment with mandate and values", Level = 3, ParentShortCode = "REL_SATISF", ParentCategoryId = level2Lookup["REL_SATISF"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_STAKEHOLDER_SATISF", ShortCode = "STAKEHOLDER_SATISF", Name = "Stakeholder satisfaction", Level = 3, ParentShortCode = "REL_SATISF", ParentCategoryId = level2Lookup["REL_SATISF"], DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under FRAUD_ETHICS
            new RiskCategory { Code = "UPC3_CYBERSEC_DATA_PROTECT", ShortCode = "CYBERSEC_DATA_PROTECT", Name = "Cybersecurity and data protection", Level = 3, ParentShortCode = "FRAUD_ETHICS", ParentCategoryId = level2Lookup["FRAUD_ETHICS"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_FRAUD_CORRUPTION", ShortCode = "FRAUD_CORRUPTION", Name = "Fraud and corruption", Level = 3, ParentShortCode = "FRAUD_ETHICS", ParentCategoryId = level2Lookup["FRAUD_ETHICS"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_ETHICS_CONDUCT", ShortCode = "ETHICS_CONDUCT", Name = "Ethics and conduct", Level = 3, ParentShortCode = "FRAUD_ETHICS", ParentCategoryId = level2Lookup["FRAUD_ETHICS"], DisplayOrder = 3, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under NON_PROCUREMENT_FUND_DISB
            new RiskCategory { Code = "UPC3_GRNT_OTR_NON_PROC_STRAT", ShortCode = "GRNT_OTR_NON_PROC_STRAT", Name = "Grant and other non procurement fund disbursement - planning and strategy", Level = 3, ParentShortCode = "NON_PROCUREMENT_FUND_DISB", ParentCategoryId = level2Lookup["NON_PROCUREMENT_FUND_DISB"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },
            new RiskCategory { Code = "UPC3_GRNT_OTR_NON_PROC_IMPL", ShortCode = "GRNT_OTR_NON_PROC_IMPL", Name = "Grant and other non procurement fund disbursement - implementation", Level = 3, ParentShortCode = "NON_PROCUREMENT_FUND_DISB", ParentCategoryId = level2Lookup["NON_PROCUREMENT_FUND_DISB"], DisplayOrder = 2, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under SUSTAINABILITY
            new RiskCategory { Code = "UPC3_SCL_CLTR_ENV_CLMT_ECO", ShortCode = "SCL_CLTR_ENV_CLMT_ECO", Name = "Social, cultural, environmental, climate and economic", Level = 3, ParentShortCode = "SUSTAINABILITY", ParentCategoryId = level2Lookup["SUSTAINABILITY"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow },

            // Under OTHER_OPERATIONS_RISKS
            new RiskCategory { Code = "UPC3_OTHER_PROCESS_OPS_RISKS", ShortCode = "OTHER_PROCESS_OPS_RISKS", Name = "Other process/operations risks", Level = 3, ParentShortCode = "OTHER_OPERATIONS_RISKS", ParentCategoryId = level2Lookup["OTHER_OPERATIONS_RISKS"], DisplayOrder = 1, Status = EntityStatus.Active, CreatedBy = 0, CreatedDate = DateTime.UtcNow }
        };

        await context.RiskCategories.AddRangeAsync(level3Categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {level3Categories.Count} Level 3 categories (leaf level)");

        Console.WriteLine("✅ Risk Categories seeding completed\n");
    }
}

