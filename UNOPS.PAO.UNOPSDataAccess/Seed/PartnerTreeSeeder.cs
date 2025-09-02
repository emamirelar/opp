using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class PartnerTreeSeeder
    {
        public static async Task SeedPartnerTreesAsync(UNOPSAppDbContext context)
        {
            if (await context.PartnerTrees.AnyAsync())
            {
                return;
            }

            var partnerTrees = new List<UNOPSPartnerTree>
            {
                new UNOPSPartnerTree
                {
                    Id = 1,
                    Code = "ACADEMIC_TRAINING_RESEARC",
                    Name = "Academic, Training and Research",
                    Description = "Academic, Training and Research",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 2,
                    Code = "FOUNDATION",
                    Name = "Foundation",
                    Description = "Foundation",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 3,
                    Code = "GOVERNMENT",
                    Name = "Government",
                    Description = "Government",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 4,
                    Code = "MULTILATERAL",
                    Name = "Multilateral",
                    Description = "Multilateral",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 5,
                    Code = "NGO",
                    Name = "Non-governmental Organizations",
                    Description = "NGO",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 6,
                    Code = "OTHER",
                    Name = "Other",
                    Description = "Other",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 7,
                    Code = "PPP",
                    Name = "PPP Public Private Partnership",
                    Description = "PPP",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 8,
                    Code = "PRIVATE_SECTOR",
                    Name = "Private Sector",
                    Description = "Private Sector",
                    Type = "Level_1",
                    Parent = "",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 9,
                    Code = "IFI",
                    Name = "IFI International Financial Institutions",
                    Description = "IFI",
                    Type = "Level_2",
                    Parent = "MULTILATERAL",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 10,
                    Code = "MPI",
                    Name = "Multi-partner initiatives",
                    Description = "MPI",
                    Type = "Level_2",
                    Parent = "MULTILATERAL",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 11,
                    Code = "NON_OECD_DAC",
                    Name = "Non-OECD/DAC Government",
                    Description = "Gov: Non-OECD/DAC",
                    Type = "Level_2",
                    Parent = "GOVERNMENT",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 12,
                    Code = "OECD_DAC",
                    Name = "OECD/DAC Government",
                    Description = "Gov: OECD/DAC",
                    Type = "Level_2",
                    Parent = "GOVERNMENT",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 13,
                    Code = "REG_OTH_INGO",
                    Name = "Regional and other Intergovernmental Organizations",
                    Description = "Regional & Other IGO",
                    Type = "Level_2",
                    Parent = "MULTILATERAL",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 14,
                    Code = "UNITED_NATIONS",
                    Name = "United Nations",
                    Description = "UN",
                    Type = "Level_2",
                    Parent = "MULTILATERAL",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 15,
                    Code = "UN_INTER_POOLED_FUND",
                    Name = "United Nations inter-agency pooled funds incl. Joint Programmes",
                    Description = "UN inter-agency pooled funds incl. JPs",
                    Type = "Level_2",
                    Parent = "MULTILATERAL",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 16,
                    Code = "VERTICAL_FUND",
                    Name = "Vertical Fund",
                    Description = "Vertical Fund",
                    Type = "Level_2",
                    Parent = "MULTILATERAL",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 17,
                    Code = "3MDG_MAH",
                    Name = "3MDG/Myanmar Access for Health",
                    Description = "3MDG/Myanmar Access for Health",
                    Type = "Level_3",
                    Parent = "MPI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 18,
                    Code = "ARG01",
                    Name = "Argentina",
                    Description = "Argentina",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 19,
                    Code = "AU",
                    Name = "AU African Union",
                    Description = "AU African Union",
                    Type = "Level_3",
                    Parent = "REG_OTH_INGO",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 20,
                    Code = "AUSTRALIA",
                    Name = "Australia",
                    Description = "Australia",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 21,
                    Code = "AUSTRIA",
                    Name = "Austria",
                    Description = "Austria",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 22,
                    Code = "BANGLADESH",
                    Name = "Bangladesh",
                    Description = "Bangladesh",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 23,
                    Code = "BARBADOS",
                    Name = "Barbados",
                    Description = "Barbados",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 24,
                    Code = "BELGIUM",
                    Name = "Belgium",
                    Description = "Belgium",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 25,
                    Code = "BRAZIL",
                    Name = "Brazil",
                    Description = "Brazil",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 26,
                    Code = "CANADA",
                    Name = "Canada",
                    Description = "Canada",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 27,
                    Code = "CHINA",
                    Name = "China",
                    Description = "China",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 28,
                    Code = "CONVENTION_FRAMEWORK",
                    Name = "United Nations Conventions and Frameworks",
                    Description = "United Nations Conventions and Frameworks",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 29,
                    Code = "COSTA_RICA",
                    Name = "Costa Rica",
                    Description = "Costa Rica",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 30,
                    Code = "DENMARK",
                    Name = "Denmark",
                    Description = "Denmark",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 31,
                    Code = "DEPARTMENT_OFFICE",
                    Name = "United Nations Departments and Offices",
                    Description = "United Nations Departments and Offices",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 32,
                    Code = "ECUADOR",
                    Name = "Ecuador",
                    Description = "Ecuador",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 33,
                    Code = "EIF",
                    Name = "EIF Enhanced Integrated Framework",
                    Description = "EIF Enhanced Integrated Framework",
                    Type = "Level_3",
                    Parent = "MPI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 34,
                    Code = "ESWATINI",
                    Name = "Eswatini",
                    Description = "Eswatini",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 35,
                    Code = "ETHIOPIA",
                    Name = "Ethiopia",
                    Description = "Ethiopia",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 36,
                    Code = "EU",
                    Name = "EU European Union",
                    Description = "EU European Union",
                    Type = "Level_3",
                    Parent = "REG_OTH_INGO",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 37,
                    Code = "FCLP01",
                    Name = "The Forest & Climate Leaders’ Partnership",
                    Description = "The Forest & Climate Leaders’ Partnership",
                    Type = "Level_3",
                    Parent = "MPI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 38,
                    Code = "FDG001",
                    Name = "Fuel Distribution Gaza",
                    Description = "Fuel Distribution Gaza",
                    Type = "Level_3",
                    Parent = "MPI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 39,
                    Code = "FINLAND",
                    Name = "Finland",
                    Description = "Finland",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 40,
                    Code = "FRANCE",
                    Name = "France",
                    Description = "France",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 41,
                    Code = "FUND_PROGRAMME",
                    Name = "United Nations Funds and Programmes",
                    Description = "United Nations Funds and Programmes",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 42,
                    Code = "GAMBIA",
                    Name = "Gambia",
                    Description = "Gambia",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 43,
                    Code = "GCAP01",
                    Name = "Global Climate Action Partnership",
                    Description = "Global Climate Action Partnership",
                    Type = "Level_3",
                    Parent = "MPI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 44,
                    Code = "GERMANY",
                    Name = "Germany",
                    Description = "Germany",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 45,
                    Code = "GFATM",
                    Name = "GFATM Global Fund to Fight Aids, Tuberculosis and Malaria",
                    Description = "GFATM Global Fund to Fight Aids, Tuberculosis and Malaria",
                    Type = "Level_3",
                    Parent = "VERTICAL_FUND",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 46,
                    Code = "GREECE",
                    Name = "Greece",
                    Description = "Greece",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 47,
                    Code = "GUATEMALA",
                    Name = "Guatemala",
                    Description = "Guatemala",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 48,
                    Code = "HONDURAS",
                    Name = "Honduras",
                    Description = "Honduras",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 49,
                    Code = "ICELAND",
                    Name = "Iceland",
                    Description = "Iceland",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 50,
                    Code = "INDIA",
                    Name = "India",
                    Description = "India",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 51,
                    Code = "INDONESIA",
                    Name = "Indonesia",
                    Description = "Indonesia",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 52,
                    Code = "IRELAND",
                    Name = "Ireland",
                    Description = "Ireland",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 53,
                    Code = "ISRAEL",
                    Name = "Israel",
                    Description = "Israel",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 54,
                    Code = "ITA001",
                    Name = "Italy",
                    Description = "Italy",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 55,
                    Code = "JAPAN",
                    Name = "Japan",
                    Description = "Japan",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 56,
                    Code = "KOREA",
                    Name = "South Korea",
                    Description = "South Korea",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 57,
                    Code = "KW001",
                    Name = "Kuwait",
                    Description = "Kuwait",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 58,
                    Code = "LIBYA",
                    Name = "Libya",
                    Description = "Libya",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 59,
                    Code = "LIECHTENSTEIN",
                    Name = "Liechtenstein",
                    Description = "Liechtenstein",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 60,
                    Code = "LUXEMBOURG",
                    Name = "Luxembourg",
                    Description = "Luxembourg",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 61,
                    Code = "MEXICO",
                    Name = "Mexico",
                    Description = "Mexico",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 62,
                    Code = "MOROCCO",
                    Name = "Moroco",
                    Description = "Moroco",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 63,
                    Code = "NETHERLANDS",
                    Name = "Netherlands",
                    Description = "Netherlands",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 64,
                    Code = "NEW_ZEALAND",
                    Name = "New Zealand",
                    Description = "New Zealand",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 65,
                    Code = "NORWAY",
                    Name = "Norway",
                    Description = "Norway",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 66,
                    Code = "OMAN001",
                    Name = "Oman",
                    Description = "Oman",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 67,
                    Code = "OTHER_BODIES",
                    Name = "United Nations Other Bodies",
                    Description = "United Nations Other Bodies",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 68,
                    Code = "OTHER_ENTITIES",
                    Name = "United Nations Other Entities",
                    Description = "United Nations Other Entities",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 69,
                    Code = "PANAMA",
                    Name = "Panama",
                    Description = "Panama",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 70,
                    Code = "PARAGUAY",
                    Name = "Paraguay",
                    Description = "Paraguay",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 71,
                    Code = "PERU",
                    Name = "Peru",
                    Description = "Peru",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 72,
                    Code = "PNG001",
                    Name = "Papua New Guinea",
                    Description = "Papua New Guinea",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 73,
                    Code = "POLAND",
                    Name = "Poland",
                    Description = "Poland",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 74,
                    Code = "PORTUGAL",
                    Name = "Portugal",
                    Description = "Portugal",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 75,
                    Code = "QATAR",
                    Name = "Qatar",
                    Description = "Qatar",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 76,
                    Code = "REG_COMMISSION",
                    Name = "United Nations Regional Commissions",
                    Description = "United Nations Regional Commissions",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 77,
                    Code = "REG_OTH_FI",
                    Name = "Regional and other Financial Insitutions",
                    Description = "Regional and other Financial Insitutions",
                    Type = "Level_3",
                    Parent = "IFI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 78,
                    Code = "RELATED_ORG",
                    Name = "United Nations Related Organizations",
                    Description = "United Nations Related Organizations",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 79,
                    Code = "RESEARCH_TRAINING",
                    Name = "United Nations Research and Training",
                    Description = "United Nations Research and Training",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 80,
                    Code = "SANTNET1",
                    Name = "The Santiago Network",
                    Description = "The Santiago Network",
                    Type = "Level_3",
                    Parent = "MPI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 81,
                    Code = "SAUDI_ARABIA",
                    Name = "Saudi Arabia",
                    Description = "Saudi Arabia",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 82,
                    Code = "SIERRALEONE",
                    Name = "Sierra Leone",
                    Description = "Sierra Leone",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 83,
                    Code = "SOUTH_AFRICA",
                    Name = "South Africa",
                    Description = "South Africa",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 84,
                    Code = "SPAIN",
                    Name = "Spain",
                    Description = "Spain",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 85,
                    Code = "SPECIALIZED_AGENCIES",
                    Name = "United Nations Specialized Agencies",
                    Description = "United Nations Specialized Agencies",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 86,
                    Code = "SUBSIDIARY_ORG",
                    Name = "United Nations Subsidiary Organs",
                    Description = "United Nations Subsidiary Organs",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 87,
                    Code = "SWEDEN",
                    Name = "Sweden",
                    Description = "Sweden",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 88,
                    Code = "SWITZERLAND",
                    Name = "Switzerland",
                    Description = "Switzerland",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 89,
                    Code = "TURKEY",
                    Name = "Türkiye",
                    Description = "Türkiye",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 90,
                    Code = "UAE001",
                    Name = "United Arab Emirates",
                    Description = "United Arab Emirates",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 91,
                    Code = "UK",
                    Name = "UK United Kingdom",
                    Description = "UK United Kingdom",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 92,
                    Code = "UKRAINE",
                    Name = "Ukraine",
                    Description = "Ukraine",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 93,
                    Code = "UN_COORD",
                    Name = "United Nations Coordination Mechanisms",
                    Description = "United Nations Coordination Mechanisms",
                    Type = "Level_3",
                    Parent = "UNITED_NATIONS",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 94,
                    Code = "USA",
                    Name = "USA United States of America",
                    Description = "USA United States of America",
                    Type = "Level_3",
                    Parent = "OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 95,
                    Code = "UZB001",
                    Name = "Uzbekistan",
                    Description = "Uzbekistan",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 96,
                    Code = "WBG",
                    Name = "WBG World Bank Group",
                    Description = "WBG World Bank Group",
                    Type = "Level_3",
                    Parent = "IFI",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 97,
                    Code = "ZAMBIA001",
                    Name = "Zambia",
                    Description = "Zambia",
                    Type = "Level_3",
                    Parent = "NON_OECD_DAC",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 98,
                    Code = "DOS",
                    Name = "UN DOS Department of Operational Support",
                    Description = "UN DOS Department of Operational Support",
                    Type = "Level_4",
                    Parent = "DEPARTMENT_OFFICE",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 99,
                    Code = "DPO",
                    Name = "UN DPO Department of Peace Operations",
                    Description = "UN DPO Department of Peace Operations",
                    Type = "Level_4",
                    Parent = "DEPARTMENT_OFFICE",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 100,
                    Code = "DPPA",
                    Name = "UN DPPA Department of Political and Peacebuilding Affairs",
                    Description = "UN DPPA Department of Political and Peacebuilding Affairs",
                    Type = "Level_4",
                    Parent = "DEPARTMENT_OFFICE",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 101,
                    Code = "EOSG",
                    Name = "UN EOSG Executive Office of the Secretary-General",
                    Description = "UN EOSG Executive Office of the Secretary-General",
                    Type = "Level_4",
                    Parent = "DEPARTMENT_OFFICE",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 102,
                    Code = "UNDP",
                    Name = "UNDP United Nations Development Programme",
                    Description = "UNDP United Nations Development Programme",
                    Type = "Level_4",
                    Parent = "FUND_PROGRAMME",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 103,
                    Code = "UNOPS",
                    Name = "UNOPS United Nations Office for Project Services",
                    Description = "UNOPS United Nations Office for Project Services",
                    Type = "Level_4",
                    Parent = "OTHER_ENTITIES",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new UNOPSPartnerTree
                {
                    Id = 104,
                    Code = "WHO_PAHO",
                    Name = "WHO / PAHO World Health Organization incl. PAHO",
                    Description = "WHO / PAHO World Health Organization incl. PAHO",
                    Type = "Level_4",
                    Parent = "SPECIALIZED_AGENCIES",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }
            };

            await context.PartnerTrees.AddRangeAsync(partnerTrees);
            await context.SaveChangesAsync();
        }
    }
}