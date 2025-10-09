using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    public static class ProspectAccountsSeeder
    {
        public static async Task SeedProspectAccountsAsync(UNOPSAppDbContext context)
        {
            // Create mapping from PAOUser Email to Id (handle duplicates by taking first, filter out null emails)
            var paoUsers = await context.PAOUsers
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();
            var paoUserEmailMapping = paoUsers
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .GroupBy(u => u.Email.ToLower())
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Create mapping from OrganizationHierarchy Name to Id (handle duplicates by taking first, filter out null names)
            var orgUnits = await context.OrganizationHierarchies
                .Where(oh => oh.Type == Domain.Enums.OrganizationUnitType.OrgUnit)
                .ToListAsync();
            var orgUnitMapping = orgUnits
                .Where(oh => !string.IsNullOrEmpty(oh.Name))
                .GroupBy(oh => oh.Name)
                .ToDictionary(g => g.Key, g => g.First().Id);

            // Process prospect accounts
            var partnersToProcess = new List<(string Name, UNOPSPartner Partner, string? OrgUnit)>
            {
                new ("Ministry of Health, Labour and Welfare (MHLW) Japan", new UNOPSPartner
                {
                    Name = "Ministry of Health, Labour and Welfare (MHLW) Japan",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Ministry of Economy, Trade and Industry (METI) Japan", new UNOPSPartner
                {
                    Name = "Ministry of Economy, Trade and Industry (METI) Japan",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/meti.go.jp",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("AAIC Japan Co., Ltd.", new UNOPSPartner
                {
                    Name = "AAIC Japan Co., Ltd.",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/aa-ic.com",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("FPI - European Commission", new UNOPSPartner
                {
                    Name = "FPI - European Commission",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Ministry of Foreign Affairs of Italy", new UNOPSPartner
                {
                    Name = "Ministry of Foreign Affairs of Italy",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("mariacarmenco@unops.org".ToLower()) ? paoUserEmailMapping["mariacarmenco@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Japan Embassy Conakry", new UNOPSPartner
                {
                    Name = "Japan Embassy Conakry",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("seynaboud@unops.org".ToLower()) ? paoUserEmailMapping["seynaboud@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "AFR, WAMCO, Senegal"),
                new ("Japan Embassy Guinea", new UNOPSPartner
                {
                    Name = "Japan Embassy Guinea",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("seynaboud@unops.org".ToLower()) ? paoUserEmailMapping["seynaboud@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "AFR, WAMCO, Senegal"),
                new ("RCO Mali", new UNOPSPartner
                {
                    Name = "RCO Mali",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("michaeld@unops.org".ToLower()) ? paoUserEmailMapping["michaeld@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "AFR, WAMCO, Mali"),
                new ("Hotel New Otani Tokyo", new UNOPSPartner
                {
                    Name = "Hotel New Otani Tokyo",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/newotani.co.jp",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("NEC Corporation", new UNOPSPartner
                {
                    Name = "NEC Corporation",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/nec.com",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Allm Inc.", new UNOPSPartner
                {
                    Name = "Allm Inc.",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/g.allm.net",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Nomura Research Institute (NRI)", new UNOPSPartner
                {
                    Name = "Nomura Research Institute (NRI)",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/nri.com",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Twinbird Corporation", new UNOPSPartner
                {
                    Name = "Twinbird Corporation",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/twinbird.jp",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Yamaha Motor Co., Ltd.", new UNOPSPartner
                {
                    Name = "Yamaha Motor Co., Ltd.",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/global.yamaha-motor.com",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("yuichis@unops.org".ToLower()) ? paoUserEmailMapping["yuichis@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("AEF Africa-Europe Foundation", new UNOPSPartner
                {
                    Name = "AEF Africa-Europe Foundation",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/africaeuropefoundation.org",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("celiaafricak@unops.org".ToLower()) ? paoUserEmailMapping["celiaafricak@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Human Practice Foundation", new UNOPSPartner
                {
                    Name = "Human Practice Foundation",
                    PartnerShortDescription = null,
                    PartnerLongDescription = "2 Brolæggerstræde, København, 1211.0, Denmark",
                    LogoUrl = "https://logo.clearbit.com/humanpractice.org",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("UN Integrated Strategy for the Sahel (UNISS)", new UNOPSPartner
                {
                    Name = "UN Integrated Strategy for the Sahel (UNISS)",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("abdoulazizs@unops.org".ToLower()) ? paoUserEmailMapping["abdoulazizs@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "AFR, WAMCO, West Africa MCO"),
                new ("Ministry of Climate, Energy and Utilities of Denmark", new UNOPSPartner
                {
                    Name = "Ministry of Climate, Energy and Utilities of Denmark",
                    PartnerShortDescription = null,
                    PartnerLongDescription = "20 Holmens Kanal, København, 1060.0, Denmark",
                    LogoUrl = "https://logo.clearbit.com/en.kefm.dk",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("Secretaría de Relaciones Exteriores y Cooperación Internacional", new UNOPSPartner
                {
                    Name = "Secretaría de Relaciones Exteriores y Cooperación Internacional",
                    PartnerShortDescription = null,
                    PartnerLongDescription = "Bulevar Kuwait, contiguo a la Corte Suprema de Justicia (CSJ)",
                    LogoUrl = "https://logo.clearbit.com/sreci.gob.hn",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("lauragi@unops.org".ToLower()) ? paoUserEmailMapping["lauragi@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "LCR, CEMCO, Honduras"),
                new ("Camara de Comercio de Cortes", new UNOPSPartner
                {
                    Name = "Camara de Comercio de Cortes",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("lauragi@unops.org".ToLower()) ? paoUserEmailMapping["lauragi@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "LCR, CEMCO, Honduras"),
                new ("Carlsberg Group A/S", new UNOPSPartner
                {
                    Name = "Carlsberg Group A/S",
                    PartnerShortDescription = null,
                    PartnerLongDescription = "1 J. C. Jacobsens Gade, København, 1799.0, Denmark",
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("UN in Rome", new UNOPSPartner
                {
                    Name = "UN in Rome",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("British Virgin Islands", new UNOPSPartner
                {
                    Name = "British Virgin Islands",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("williamsg@unops.org".ToLower()) ? paoUserEmailMapping["williamsg@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "LCR, ICMCO, Costa Rica"),
                new ("Comunità Sant'Egidio", new UNOPSPartner
                {
                    Name = "Comunità Sant'Egidio",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = null,
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("martina@unops.org".ToLower()) ? paoUserEmailMapping["martina@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group"),
                new ("IFU - Impact Fund Denmark", new UNOPSPartner
                {
                    Name = "IFU - Impact Fund Denmark",
                    PartnerShortDescription = null,
                    PartnerLongDescription = null,
                    LogoUrl = "https://logo.clearbit.com/impactfund.dk",
                    ErpDimValue = null,
                    Status = (EntityStatus)1,
                    UNSecretariatPartner = false,
                    PooledFund = false,
                    KeyGlobalPartner = false,
                    CanCreateNewOpportunities = false,
                    PartnerApprovalStatus = (PartnerApprovalStatus)0,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DueDiligenceRequired = null,
                    DueDiligenceApproval = null,
                    DueDiligenceApprovalDate = null,
                    DueDiligenceExpiryDate = null,
                    PartnerApprovalDate = null,
                    PartnerApprovedBy = null,
                    PartnerFocalPointUserId = paoUserEmailMapping.ContainsKey("asbjornb@unops.org".ToLower()) ? paoUserEmailMapping["asbjornb@unops.org".ToLower()] : (int?)null,
                    PartnerCategoryId = null,
                    PartnerGroupId = null,
                    LiaisonOfficeId = null,
                    PartnerLevyStatus = null,
                    LevyTreatment = null,
                    ReasonForLevy = null,
                    ReasonForNoNewOpportunity = null,
                    PartnerApprovalReference = null
                }, "DP, PLG, Partnerships and Liaison Group")
            };

            // Begin transaction to ensure atomicity
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Step 1: Process all partners (create or update)
                foreach (var (partnerName, partnerData, _) in partnersToProcess)
                {
                    // Check if partner already exists based on Name where ErpDimValue is null
                    var existingPartner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == partnerName && p.ErpDimValue == null);

                    if (existingPartner != null)
                    {
                        // Update existing partner
                        existingPartner.Name = partnerData.Name;
                        existingPartner.PartnerShortDescription = partnerData.PartnerShortDescription;
                        existingPartner.PartnerLongDescription = partnerData.PartnerLongDescription;
                        existingPartner.LogoUrl = partnerData.LogoUrl;
                        existingPartner.Status = partnerData.Status;
                        existingPartner.UNSecretariatPartner = partnerData.UNSecretariatPartner;
                        existingPartner.PooledFund = partnerData.PooledFund;
                        existingPartner.KeyGlobalPartner = partnerData.KeyGlobalPartner;
                        existingPartner.CanCreateNewOpportunities = partnerData.CanCreateNewOpportunities;
                        existingPartner.PartnerApprovalStatus = partnerData.PartnerApprovalStatus;
                        existingPartner.PartnerFocalPointUserId = partnerData.PartnerFocalPointUserId;
                        existingPartner.DueDiligenceRequired = partnerData.DueDiligenceRequired;
                        existingPartner.DueDiligenceApproval = partnerData.DueDiligenceApproval;
                        existingPartner.DueDiligenceApprovalDate = partnerData.DueDiligenceApprovalDate;
                        existingPartner.DueDiligenceExpiryDate = partnerData.DueDiligenceExpiryDate;
                        existingPartner.PartnerApprovalDate = partnerData.PartnerApprovalDate;
                        existingPartner.PartnerApprovedBy = partnerData.PartnerApprovedBy;
                        existingPartner.PartnerCategoryId = partnerData.PartnerCategoryId;
                        existingPartner.PartnerGroupId = partnerData.PartnerGroupId;
                        existingPartner.LiaisonOfficeId = partnerData.LiaisonOfficeId;
                        existingPartner.PartnerLevyStatus = partnerData.PartnerLevyStatus;
                        existingPartner.LevyTreatment = partnerData.LevyTreatment;
                        existingPartner.ReasonForLevy = partnerData.ReasonForLevy;
                        existingPartner.ReasonForNoNewOpportunity = partnerData.ReasonForNoNewOpportunity;
                        existingPartner.PartnerApprovalReference = partnerData.PartnerApprovalReference;
                        existingPartner.LastModifiedBy = 0;
                        existingPartner.LastModifiedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        // Add new partner to context
                        context.Partners.Add(partnerData);
                    }
                }

                // Save all partners at once
                await context.SaveChangesAsync();

                // Step 2: Process all organization unit relationships in batch
                foreach (var (partnerName, _, orgUnit) in partnersToProcess)
                {
                    // Skip if no org unit specified
                    if (string.IsNullOrWhiteSpace(orgUnit) || !orgUnitMapping.ContainsKey(orgUnit))
                        continue;

                    // Get the partner (now guaranteed to exist with an ID)
                    var partner = await context.Partners
                        .FirstOrDefaultAsync(p => p.Name == partnerName && p.ErpDimValue == null);

                    if (partner == null)
                        continue;

                    var orgHierarchyId = orgUnitMapping[orgUnit];

                    // Check if relationship already exists
                    var existingRelationship = await context.OrganizationUnitRelationships
                        .FirstOrDefaultAsync(r => r.EntityType == nameof(Partner) && 
                                                  r.EntityId == partner.Id && 
                                                  r.OrganizationHierarchyId == orgHierarchyId);

                    if (existingRelationship == null)
                    {
                        // Create new relationship
                        var newRelationship = new OrganizationUnitRelationship
                        {
                            OrganizationHierarchyId = orgHierarchyId,
                            EntityId = partner.Id,
                            EntityType = nameof(Partner),
                            Name = $"Partner-{partner.Id}-{orgHierarchyId}",
                            Status = EntityStatus.Active,
                            CreatedBy = 0,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedBy = 0,
                            LastModifiedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        context.OrganizationUnitRelationships.Add(newRelationship);
                    }
                }

                // Save all organization unit relationships at once
                await context.SaveChangesAsync();

                // Commit transaction if everything succeeded
                await transaction.CommitAsync();

                Console.WriteLine($"Successfully seeded prospect accounts");
            }
            catch (Exception ex)
            {
                // Rollback transaction if any error occurred
                await transaction.RollbackAsync();
                Console.WriteLine($"Error seeding prospect accounts: {ex.Message}");
                throw;
            }
        }
    }
}