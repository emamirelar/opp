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
        Console.WriteLine("🔄 Seeding Risk Categories...");

        // Seed Level 1
        await UpsertLevel1CategoriesAsync(context);

        // Seed Level 2 (needs Level 1 IDs)
        var level1Lookup = await context.RiskCategories
            .Where(c => c.Level == 1)
            .ToDictionaryAsync(c => c.ShortCode, c => c.Id);
        await UpsertLevel2CategoriesAsync(context, level1Lookup);

        // Seed Level 3 (needs Level 2 IDs)
        var level2Lookup = await context.RiskCategories
            .Where(c => c.Level == 2)
            .ToDictionaryAsync(c => c.ShortCode, c => c.Id);
        await UpsertLevel3CategoriesAsync(context, level2Lookup);

        Console.WriteLine("✅ Risk Categories seeding completed.");
    }

    private static async Task UpsertLevel1CategoriesAsync(UNOPSAppDbContext context)
    {
        var categories = new[]
        {
            ("UPC1_FINANCE", "FINANCE", "Finance", 1),
            ("UPC1_PARTNERS_STAKEHOLDERS", "PARTNERS_STAKEHOLDERS", "Partners & stakeholders", 2),
            ("UPC1_PEOPLE", "PEOPLE", "People", 3),
            ("UPC1_PROCESS_OPERATIONS", "PROCESS_OPERATIONS", "Process / Operations", 4)
        };

        foreach (var (code, shortCode, name, order) in categories)
        {
            await UpsertCategoryAsync(context, code, shortCode, name, 1, null, null, order);
        }
    }

    private static async Task UpsertLevel2CategoriesAsync(UNOPSAppDbContext context, Dictionary<string, int> level1Lookup)
    {
        var categories = new[]
        {
            // FINANCE
            ("UPC2_CONTRIBUTIONS", "CONTRIBUTIONS", "Contributions", "FINANCE", 1),
            ("UPC2_CURR_EXCHANGE_RATE", "CURR_EXCHANGE_RATE", "Currency and exchange rate", "FINANCE", 2),
            ("UPC2_EXPENDITURE", "EXPENDITURE", "Expenditure", "FINANCE", 3),
            ("UPC2_ICT", "ICT", "ICT", "FINANCE", 4),
            ("UPC2_REPORTING_DATA", "REPORTING_DATA", "Reporting and data", "FINANCE", 5),
            ("UPC2_TREASURY", "TREASURY", "Treasury", "FINANCE", 6),
            ("UPC2_OTHER_FINANCE_RISKS", "OTHER_FINANCE_RISKS", "Other finance risks", "FINANCE", 7),
            // PARTNERS_STAKEHOLDERS
            ("UPC2_GEOP_ECON", "GEOP_ECON", "Geopolitical / economic context", "PARTNERS_STAKEHOLDERS", 1),
            ("UPC2_LEGAL_COMP", "LEGAL_COMP", "Legal and compliance", "PARTNERS_STAKEHOLDERS", 2),
            ("UPC2_PART_FUND", "PART_FUND", "Partnership & funding landscape", "PARTNERS_STAKEHOLDERS", 3),
            ("UPC2_REL_SATISF", "REL_SATISF", "Relations & satisfaction", "PARTNERS_STAKEHOLDERS", 4),
            ("UPC2_OTHER_PRT_RI", "OTHER_PRT_RI", "Other partners & stakeholders risks", "PARTNERS_STAKEHOLDERS", 5),
            // PEOPLE
            ("UPC2_CAPABILITIES_PERF_MGMGT", "CAPABILITIES_PERF_MGMGT", "Capabilities and performance management", "PEOPLE", 1),
            ("UPC2_RECRUITMENT_RETENTION", "RECRUITMENT_RETENTION", "Recruitment & retention", "PEOPLE", 2),
            ("UPC2_SAFETY_SECURITY", "SAFETY_SECURITY", "Safety and security", "PEOPLE", 3),
            ("UPC2_OTHER_PEOPLE_RISKS", "OTHER_PEOPLE_RISKS", "Other people risks", "PEOPLE", 4),
            // PROCESS_OPERATIONS
            ("UPC2_FRAUD_ETHICS", "FRAUD_ETHICS", "Fraud and ethics", "PROCESS_OPERATIONS", 1),
            ("UPC2_HSSE_OFFICES", "HSSE_OFFICES", "HSSE - Offices & connected workplaces", "PROCESS_OPERATIONS", 2),
            ("UPC2_HSSE_SITES", "HSSE_SITES", "HSSE - Sites & connected workplaces", "PROCESS_OPERATIONS", 3),
            ("UPC2_INFRASTRUCTURE_OPERATIONS", "INFRASTRUCTURE_OPERATIONS", "Infrastructure operations", "PROCESS_OPERATIONS", 4),
            ("UPC2_NON_PROCUREMENT_FUND_DISB", "NON_PROCUREMENT_FUND_DISB", "Non procurement fund disbursement", "PROCESS_OPERATIONS", 5),
            ("UPC2_ORGANISATIONAL_SETTING", "ORGANISATIONAL_SETTING", "Organisational setting", "PROCESS_OPERATIONS", 6),
            ("UPC2_PROCUREMENT", "PROCUREMENT", "Procurement", "PROCESS_OPERATIONS", 7),
            ("UPC2_PROJECT_MANAGEMENT_DESI", "PROJECT_MANAGEMENT_DESI", "Project Management & Design", "PROCESS_OPERATIONS", 8),
            ("UPC2_PROJECT_SITE_OPERATIONS", "PROJECT_SITE_OPERATIONS", "Project site operations", "PROCESS_OPERATIONS", 9),
            ("UPC2_SUSTAINABILITY", "SUSTAINABILITY", "Sustainability", "PROCESS_OPERATIONS", 10),
            ("UPC2_OTHER_OPERATIONS_RISKS", "OTHER_OPERATIONS_RISKS", "Other operations risks", "PROCESS_OPERATIONS", 11)
        };

        foreach (var (code, shortCode, name, parentShortCode, order) in categories)
        {
            await UpsertCategoryAsync(context, code, shortCode, name, 2, parentShortCode, level1Lookup[parentShortCode], order);
        }
    }

    private static async Task UpsertLevel3CategoriesAsync(UNOPSAppDbContext context, Dictionary<string, int> level2Lookup)
    {
        var categories = new[]
        {
            // CONTRIBUTIONS
            ("UPC3_ENG_COST_PRICE", "ENG_COST_PRICE", "Engagement costing and pricing", "CONTRIBUTIONS", 1),
            ("UPC3_ADVANCE_FINANCING", "ADVANCE_FINANCING", "Advance financing", "CONTRIBUTIONS", 2),
            ("UPC3_TIMELY_RECEIPT_OF_FUNDS", "TIMELY_RECEIPT_OF_FUNDS", "Timely receipt of funds", "CONTRIBUTIONS", 3),
            // CURR_EXCHANGE_RATE
            ("UPC3_EXCHANGE_RATE_FOR_CONTRIB", "EXCHANGE_RATE_FOR_CONTRIB", "Exchange rate for contributions", "CURR_EXCHANGE_RATE", 1),
            ("UPC3_EXCHANGE_RATE_FOR_PAYMENT", "EXCHANGE_RATE_FOR_PAYMENT", "Exchange rate for payments", "CURR_EXCHANGE_RATE", 2),
            ("UPC3_INFLATION_OR_DEFLATION", "INFLATION_OR_DEFLATION", "Inflation or deflation", "CURR_EXCHANGE_RATE", 3),
            // EXPENDITURE
            ("UPC3_OVR_INELIGB_PRJT_EXP_CONT", "OVR_INELIGB_PRJT_EXP_CONT", "Over/ineligible project expenditure and contingencies", "EXPENDITURE", 1),
            ("UPC3_OVER_DUPLICATE_PAYMENTS", "OVER_DUPLICATE_PAYMENTS", "Over/duplicate payments", "EXPENDITURE", 2),
            ("UPC3_PRE_PAYMENTS", "PRE_PAYMENTS", "Pre-payments", "EXPENDITURE", 3),
            ("UPC3_PAYMENT_INFRASTRUCTURE", "PAYMENT_INFRASTRUCTURE", "Payment infrastructure", "EXPENDITURE", 4),
            // ICT
            ("UPC3_ICT_TOOLS", "ICT_TOOLS", "ICT tools", "ICT", 1),
            // REPORTING_DATA
            ("UPC3_ACCNTNG_STDS_FIN_STMTNS", "ACCNTNG_STDS_FIN_STMTNS", "Accounting standards and financial statements", "REPORTING_DATA", 1),
            ("UPC3_ENG_PRJT_REPORT_TO_PTNR", "ENG_PRJT_REPORT_TO_PTNR", "Engagement/project reporting to partners", "REPORTING_DATA", 2),
            // TREASURY
            ("UPC3_BANK_CREDIT_DEFAULT", "BANK_CREDIT_DEFAULT", "Bank credit default", "TREASURY", 1),
            ("UPC3_LIQUIDITY_INVESTMENT", "LIQUIDITY_INVESTMENT", "Liquidity and investment management", "TREASURY", 2),
            // OTHER_FINANCE_RISKS
            ("UPC3_OTHER_FINANCE_RISKS", "OTHER_FINANCE_RISKS_L3", "Other finance risks", "OTHER_FINANCE_RISKS", 1),
            // GEOP_ECON
            ("UPC3_REGIONAL_LOCAL_INSTABILIT", "REGIONAL_LOCAL_INSTABILIT", "Regional/local instability and security", "GEOP_ECON", 1),
            ("UPC3_BUSINESS_ENV_TRAD_BARR", "BUSINESS_ENV_TRAD_BARR", "Business environment and trade barriers", "GEOP_ECON", 2),
            // LEGAL_COMP
            ("UPC3_LEGAL_REGUL_FRWRK_OP", "LEGAL_REGUL_FRWRK_OP", "Legal and regulatory framework to operate", "LEGAL_COMP", 1),
            ("UPC3_CONTRACT_COMMIT_LIABILITY", "CONTRACT_COMMIT_LIABILITY", "Contractual commitments and liabilities", "LEGAL_COMP", 2),
            ("UPC3_CLAIMS_INSURANCES_GRIEVAN", "CLAIMS_INSURANCES_GRIEVAN", "Claims, insurances and grievances", "LEGAL_COMP", 3),
            ("UPC3_COMPL_UN_VAL", "COMPL_UN_VAL", "Compliance with UN and/or UNOPS values, principles, policies and procedures", "LEGAL_COMP", 4),
            // PART_FUND
            ("UPC3_ENGAGEMENT_PIPELINE", "ENGAGEMENT_PIPELINE", "Engagement pipeline", "PART_FUND", 1),
            ("UPC3_CHG_PTNR_LANDSCAP", "CHG_PTNR_LANDSCAP", "Changing partner landscape", "PART_FUND", 2),
            ("UPC3_DEP_FEW_PARTNR_LOC_ENG", "DEP_FEW_PARTNR_LOC_ENG", "Dependency on few partners/locations/engagements", "PART_FUND", 3),
            ("UPC3_POS_COMPET_LAND_VAL_PROP", "POS_COMPET_LAND_VAL_PROP", "Positioning in the competitive landscape and value proposition", "PART_FUND", 4),
            // REL_SATISF
            ("UPC3_RELATIONSHIP_MANAGEMENT", "RELATIONSHIP_MANAGEMENT", "Relationship management", "REL_SATISF", 1),
            ("UPC3_CHG_PTNR_REQ_PRIO", "CHG_PTNR_REQ_PRIO", "Changing partner requirements and priorities", "REL_SATISF", 2),
            ("UPC3_BUYIN_FROM_KEY_STAKEHOLD", "BUYIN_FROM_KEY_STAKEHOLD", "Buy-in from key stakeholders", "REL_SATISF", 3),
            ("UPC3_INTER_INTRA_GVT_REL", "INTER_INTRA_GVT_REL", "Inter and intra governmental relations", "REL_SATISF", 4),
            ("UPC3_REP_ALGN_MAND_VAL", "REP_ALGN_MAND_VAL", "Reputation and alignment with mandate and values", "REL_SATISF", 5),
            ("UPC3_MEDIA_REL_NEG_PUB", "MEDIA_REL_NEG_PUB", "Media relations and negative publicity", "REL_SATISF", 6),
            ("UPC3_STAKEHOLDER_SATISF", "STAKEHOLDER_SATISF", "Stakeholder satisfaction", "REL_SATISF", 7),
            // OTHER_PRT_RI
            ("UPC3_OTHER_PTNR_STAKEHLD_RISK", "OTHER_PTNR_STAKEHLD_RISK", "Other partners & stakeholders risks", "OTHER_PRT_RI", 1),
            // CAPABILITIES_PERF_MGMGT
            ("UPC3_TECHNICAL_EXPERTISE", "TECHNICAL_EXPERTISE", "Technical expertise", "CAPABILITIES_PERF_MGMGT", 1),
            ("UPC3_PERFORMANCE_MANAGEMENT", "PERFORMANCE_MANAGEMENT", "Performance management", "CAPABILITIES_PERF_MGMGT", 2),
            ("UPC3_LEARN_TRAIN_KNLG_MGMT", "LEARN_TRAIN_KNLG_MGMT", "Learning, training and knowledge management", "CAPABILITIES_PERF_MGMGT", 3),
            // RECRUITMENT_RETENTION
            ("UPC3_ATTRC_AVLB_RET_QUAL_PERS", "ATTRC_AVLB_RET_QUAL_PERS", "Attraction, availability and retention of qualified personnel", "RECRUITMENT_RETENTION", 1),
            ("UPC3_DIVERSITY_GEND_GEND_MAINS", "DIVERSITY_GEND_GEND_MAINS", "Diversity, gender and gender mainstreaming", "RECRUITMENT_RETENTION", 2),
            // SAFETY_SECURITY
            ("UPC3_SECUR_POLICIES_AND_REQ", "SECUR_POLICIES_AND_REQ", "Security policies and requirements", "SAFETY_SECURITY", 1),
            // OTHER_PEOPLE_RISKS
            ("UPC3_OTHER_PEOPLE_RISKS", "OTHER_PEOPLE_RISKS_L3", "Other people risks", "OTHER_PEOPLE_RISKS", 1),
            // FRAUD_ETHICS
            ("UPC3_CYBERSEC_DATA_PROTECT", "CYBERSEC_DATA_PROTECT", "Cybersecurity and data protection", "FRAUD_ETHICS", 1),
            ("UPC3_FRAUD_UNETHC_BHV_OPS_PERS", "FRAUD_UNETHC_BHV_OPS_PERS", "Fraudulent or unethical behavior by UNOPS personnel", "FRAUD_ETHICS", 2),
            ("UPC3_FRAUD_UNETHC_BHV_OPS_PTNR", "FRAUD_UNETHC_BHV_OPS_PTNR", "Fraudulent or unethical behavior by UNOPS supplier or implementing partner", "FRAUD_ETHICS", 3),
            ("UPC3_SEX_EXPL_HARAS_ABUS_AUTHR", "SEX_EXPL_HARAS_ABUS_AUTHR", "Sexual exploitation, harassment, bullying and abuse of authority", "FRAUD_ETHICS", 4),
            // HSSE_OFFICES
            ("UPC3_GEN_HOUSEKEEPG_HZRD_CTRL", "GEN_HOUSEKEEPG_HZRD_CTRL", "General housekeeping and hazard control", "HSSE_OFFICES", 1),
            ("UPC3_WELFARE_FACILITIES", "WELFARE_FACILITIES", "Welfare Facilities", "HSSE_OFFICES", 2),
            ("UPC3_O_EMERGENCY_ARRANGEMENTS", "O_EMERGENCY_ARRANGEMENTS", "Office emergency arrangements and response", "HSSE_OFFICES", 3),
            ("UPC3_FUEL_MANAGEMENT", "FUEL_MANAGEMENT", "Management of fuel/oils/chemicals", "HSSE_OFFICES", 4),
            ("UPC3_ELECTRICAL_SAFETY", "ELECTRICAL_SAFETY", "Electrical Safety", "HSSE_OFFICES", 5),
            ("UPC3_O_TRAFFIC", "O_TRAFFIC", "Office Traffic Management", "HSSE_OFFICES", 6),
            ("UPC3_SLIPS_TRIPS_MANHANDLING", "SLIPS_TRIPS_MANHANDLING", "Slips, Trips and Manual Handling", "HSSE_OFFICES", 7),
            ("UPC3_ERGONOMICS", "ERGONOMICS", "Ergonomics", "HSSE_OFFICES", 8),
            ("UPC3_PSYCHOSOCIAL_WELLBEING", "PSYCHOSOCIAL_WELLBEING", "Psychosocial Wellbeing", "HSSE_OFFICES", 9),
            ("UPC3_O_WASTE_MANAGEMENT", "O_WASTE_MANAGEMENT", "Office Waste Management and Segregation", "HSSE_OFFICES", 10),
            ("UPC3_GHG_REDUCTIONS", "GHG_REDUCTIONS", "GHG Emission Reductions", "HSSE_OFFICES", 11),
            ("UPC3_COMMUNITY_INTERFACE", "COMMUNITY_INTERFACE", "Community Interface", "HSSE_OFFICES", 12),
            ("UPC3_ACCOMMODATION", "ACCOMMODATION", "Accommodation Standards", "HSSE_OFFICES", 13),
            ("UPC3_O_OTHER", "O_OTHER", "Office Other", "HSSE_OFFICES", 14),
            // HSSE_SITES
            ("UPC3_GENERAL_SITE_LAYOUT", "GENERAL_SITE_LAYOUT", "General Site Layout and welfare (incl. housekeeping)", "HSSE_SITES", 1),
            ("UPC3_EMERGENCY_ARRANGEMENTS", "EMERGENCY_ARRANGEMENTS", "Site emergency arrangements and response", "HSSE_SITES", 2),
            ("UPC3_WORK_AT_HEIGHT", "WORK_AT_HEIGHT", "Work At Height", "HSSE_SITES", 3),
            ("UPC3_EQUIPMENT", "EQUIPMENT", "Equipment/Portable tools/Electrical appliances", "HSSE_SITES", 4),
            ("UPC3_EXCAVATIONS", "EXCAVATIONS", "Excavations", "HSSE_SITES", 5),
            ("UPC3_PPE", "PPE", "Personal Protection Equipment (PPE)", "HSSE_SITES", 6),
            ("UPC3_UNDEROVER_SERVICES", "UNDEROVER_SERVICES", "Underground and Overhead Services", "HSSE_SITES", 7),
            ("UPC3_HAZARDOUS_MATERIALS", "HAZARDOUS_MATERIALS", "Hazardous Materials", "HSSE_SITES", 8),
            ("UPC3_TRAFFIC", "TRAFFIC", "Site Traffic Management", "HSSE_SITES", 9),
            ("UPC3_MOBILE_PLANT_EQUIPMENT", "MOBILE_PLANT_EQUIPMENT", "Mobile Plant Equipment", "HSSE_SITES", 10),
            ("UPC3_RAMS", "RAMS", "Risk Assessment and Method Statement (RAMS)", "HSSE_SITES", 11),
            ("UPC3_LIFTING_TOOLS_APPLIANCES", "LIFTING_TOOLS_APPLIANCES", "Lifting Appliances and Equipment", "HSSE_SITES", 12),
            ("UPC3_WASTE_MANAGEMENT", "WASTE_MANAGEMENT", "Site Waste Management and Segregation", "HSSE_SITES", 13),
            ("UPC3_FUEL_STORAGE", "FUEL_STORAGE", "Fuel/Oil/Chemical Storage", "HSSE_SITES", 14),
            ("UPC3_DRAINAGE_SPILLAGE", "DRAINAGE_SPILLAGE", "Drainage, Dewatering, Spillage Control", "HSSE_SITES", 15),
            ("UPC3_ECOLOGY_ARCHAEOLOGY", "ECOLOGY_ARCHAEOLOGY", "Ecology Archaeology and Heritage", "HSSE_SITES", 16),
            ("UPC3_DUST_MUD", "DUST_MUD", "Dust and Mud", "HSSE_SITES", 17),
            ("UPC3_ODOUR_EMISSIONS", "ODOUR_EMISSIONS", "Odour and Air Emissions", "HSSE_SITES", 18),
            ("UPC3_NOISE_VIBRATION", "NOISE_VIBRATION", "Noise and Vibration", "HSSE_SITES", 19),
            ("UPC3_LABOUR_REL_COMMUN_INT", "LABOUR_REL_COMMUN_INT", "Labour Relations And Community Interface", "HSSE_SITES", 20),
            ("UPC3_VIOLENCE_ABUSE_PREVENTION", "VIOLENCE_ABUSE_PREVENTION", "Prevention of Gender Based Violence, Sexual Exploitation, Abuse and Harassment", "HSSE_SITES", 21),
            ("UPC3_OTHER", "SITE_OTHER", "Site Other", "HSSE_SITES", 22),
            // INFRASTRUCTURE_OPERATIONS
            ("UPC3_SITE_INVESTIG_INFRA_DESIG", "SITE_INVESTIG_INFRA_DESIG", "Site investigations and infrastructure design", "INFRASTRUCTURE_OPERATIONS", 1),
            ("UPC3_CONSTRUCTION_SUPERVISION", "CONSTRUCTION_SUPERVISION", "Construction supervision", "INFRASTRUCTURE_OPERATIONS", 2),
            ("UPC3_IMPL_WORKS_CONTRACTS", "IMPL_WORKS_CONTRACTS", "Implementation of works contracts", "INFRASTRUCTURE_OPERATIONS", 3),
            // NON_PROCUREMENT_FUND_DISB
            ("UPC3_GRNT_OTR_NON_PROC_STRAT", "GRNT_OTR_NON_PROC_STRAT", "Grant and other non procurement fund disbursement - planning and strategy", "NON_PROCUREMENT_FUND_DISB", 1),
            ("UPC3_GRNT_OTR_NON_PROC_PRC_MGT", "GRNT_OTR_NON_PROC_PRC_MGT", "Grant and other non procurement fund disbursement - process management", "NON_PROCUREMENT_FUND_DISB", 2),
            ("UPC3_GSA_UN2UN_PERF_MGMT", "GSA_UN2UN_PERF_MGMT", "GSA, UN2UN and other non-procurement agreement to transfer funds - performance management", "NON_PROCUREMENT_FUND_DISB", 3),
            // ORGANISATIONAL_SETTING
            ("UPC3_GOV_ROLE_RESP", "GOV_ROLE_RESP", "Governance and roles/responsibilities", "ORGANISATIONAL_SETTING", 1),
            ("UPC3_PROCESS_EFCTIV_INT_CONTRL", "PROCESS_EFCTIV_INT_CONTRL", "Process effectiveness and internal controls", "ORGANISATIONAL_SETTING", 2),
            ("UPC3_STRAT_PLAN_RES_ALLOC", "STRAT_PLAN_RES_ALLOC", "Strategic planning and resource allocation", "ORGANISATIONAL_SETTING", 3),
            // PROCUREMENT
            ("UPC3_PROCUR_PLAN_STRAT", "PROCUR_PLAN_STRAT", "Procurement planning and strategy", "PROCUREMENT", 1),
            ("UPC3_PROCUR_PROCESS_MGMT", "PROCUR_PROCESS_MGMT", "Procurement process management", "PROCUREMENT", 2),
            ("UPC3_AVBL_SERV_PROV_QUAL_SUP", "AVBL_SERV_PROV_QUAL_SUP", "Availability of service providers, quality supplies and equipment", "PROCUREMENT", 3),
            ("UPC3_SUP_CNTRCT_PERF_MGMT", "SUP_CNTRCT_PERF_MGMT", "Supplier contract and performance management", "PROCUREMENT", 4),
            // PROJECT_MANAGEMENT_DESI
            ("UPC3_PROJECT_PLANNING", "PROJECT_PLANNING", "Project planning", "PROJECT_MANAGEMENT_DESI", 1),
            ("UPC3_PROJECT_CONTROL", "PROJECT_CONTROL", "Project control", "PROJECT_MANAGEMENT_DESI", 2),
            ("UPC3_PROJECT_DELIVERY", "PROJECT_DELIVERY", "Project delivery", "PROJECT_MANAGEMENT_DESI", 3),
            ("UPC3_PROJECT_CLOSURE", "PROJECT_CLOSURE", "Project closure", "PROJECT_MANAGEMENT_DESI", 4),
            ("UPC3_PRJT_COMPLEXT_INTERDEPEND", "PRJT_COMPLEXT_INTERDEPEND", "Project complexity and interdependencies", "PROJECT_MANAGEMENT_DESI", 5),
            ("UPC3_BENEF_DEF_REAL", "BENEF_DEF_REAL", "Benefits definition and realisation", "PROJECT_MANAGEMENT_DESI", 6),
            ("UPC3_BENEFICIARY_SELECT_TARGET", "BENEFICIARY_SELECT_TARGET", "Beneficiary selection and targeting", "PROJECT_MANAGEMENT_DESI", 7),
            ("UPC3_MONITORING_EVALUATION", "MONITORING_EVALUATION", "Monitoring & Evaluation", "PROJECT_MANAGEMENT_DESI", 8),
            ("UPC3_TECHNO_COMPLEXIT_CHALNG", "TECHNO_COMPLEXIT_CHALNG", "Technology complexity or challenges", "PROJECT_MANAGEMENT_DESI", 9),
            // PROJECT_SITE_OPERATIONS
            ("UPC3_ACCESS_TO_PROJECT_SITE", "ACCESS_TO_PROJECT_SITE", "Access to project site", "PROJECT_SITE_OPERATIONS", 1),
            ("UPC3_AVAILABILITY_BASIC_INFRA", "AVAILABILITY_BASIC_INFRA", "Availability of basic infrastructure", "PROJECT_SITE_OPERATIONS", 2),
            ("UPC3_AVAILABILITY_NAT_RES", "AVAILABILITY_NAT_RES", "Availability of natural resources", "PROJECT_SITE_OPERATIONS", 3),
            ("UPC3_OWN_USE_PROJECT_SITE", "OWN_USE_PROJECT_SITE", "Ownership and continued use of project site", "PROJECT_SITE_OPERATIONS", 4),
            ("UPC3_SITE_LOGIST_PLAN_STRAT", "SITE_LOGIST_PLAN_STRAT", "Site logistics planning and strategy", "PROJECT_SITE_OPERATIONS", 5),
            // SUSTAINABILITY
            ("UPC3_ENV_CLIMATE_CHANGE", "ENV_CLIMATE_CHANGE", "Environment and climate change", "SUSTAINABILITY", 1),
            ("UPC3_SOCIAL_AND_CULTURE", "SOCIAL_AND_CULTURE", "Social and culture", "SUSTAINABILITY", 2),
            ("UPC3_ECONOMIC_DEVELOPMENT", "ECONOMIC_DEVELOPMENT", "Economic development", "SUSTAINABILITY", 3),
            ("UPC3_DRR_RESILIENCE", "DRR_RESILIENCE", "Disaster risk reduction (DRR) and resilience", "SUSTAINABILITY", 4),
            // OTHER_OPERATIONS_RISKS
            ("UPC3_OTHER_PROCESS_OPS_RISKS", "OTHER_PROCESS_OPS_RISKS", "Other process/operations risks", "OTHER_OPERATIONS_RISKS", 1)
        };

        foreach (var (code, shortCode, name, parentShortCode, order) in categories)
        {
            await UpsertCategoryAsync(context, code, shortCode, name, 3, parentShortCode, level2Lookup[parentShortCode], order);
        }
    }

    private static async Task UpsertCategoryAsync(
        UNOPSAppDbContext context,
        string code,
        string shortCode,
        string name,
        int level,
        string? parentShortCode,
        int? parentCategoryId,
        int displayOrder)
    {
        var existing = await context.RiskCategories.FirstOrDefaultAsync(c => c.ShortCode == shortCode);

        if (existing != null)
        {
            // Update
            existing.Code = code;
            existing.Name = name;
            existing.Level = level;
            existing.ParentShortCode = parentShortCode;
            existing.ParentCategoryId = parentCategoryId;
            existing.DisplayOrder = displayOrder;
            existing.LastModifiedDate = DateTime.UtcNow;
        }
        else
        {
            // Insert
            context.RiskCategories.Add(new RiskCategory
            {
                Code = code,
                ShortCode = shortCode,
                Name = name,
                Level = level,
                ParentShortCode = parentShortCode,
                ParentCategoryId = parentCategoryId,
                DisplayOrder = displayOrder,
                Status = EntityStatus.Active,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }
}
