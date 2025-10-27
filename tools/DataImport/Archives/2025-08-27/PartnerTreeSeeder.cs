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
                    Code = "BCMCFE001",
                    Name = "Baylor College of Medicine Children’s Foundation Eswatini",
                    Description = "Baylor Foundation Eswatini",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "CC001",
                    Name = "Cygnum Capital Asset Management",
                    Description = "Cygnum Capital",
                    Type = "Level_2",
                    Parent = "PRIVATE_SECTOR",
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
                    Code = "CLINTONHEALTHACCESS",
                    Name = "Clinton Health Access Initiative",
                    Description = "CHAI",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "COG01",
                    Name = "Comité Olimpico Guatemalteco (COG)",
                    Description = "Comité Olimpico Guatemalteco",
                    Type = "Level_2",
                    Parent = "OTHER",
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
                    Code = "DANLUT001",
                    Name = "Danish Lutheran Mission",
                    Description = "Danish Lutheran Milssion",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "DRC001",
                    Name = "DRC",
                    Description = "Danish Refugee Council",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "FEDERACIONREDNICASALUD",
                    Name = "Federación Red NicaSalud",
                    Description = "Federación Red NicaSalud",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "GLE001",
                    Name = "Google LLC",
                    Description = "Google",
                    Type = "Level_2",
                    Parent = "PRIVATE_SECTOR",
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
                    Code = "GLIDE001",
                    Name = "Global Institute For Disease Elimination",
                    Description = "Global Institute For Disease Elimination",
                    Type = "Level_2",
                    Parent = "PRIVATE_SECTOR",
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
                    Code = "GLOBALALLIANCEFORASUSTPLA",
                    Name = "Global Alliance for a Sustainable Planet",
                    Description = "GASP",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "ICS01",
                    Name = "The Climate and Society Institute",
                    Description = "ICS",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "INTMEDCORPS",
                    Name = "International Medical Corps",
                    Description = "International Medical Corps",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "INTMEDCORPUS",
                    Name = "International Medical Corps US",
                    Description = "International Medical Corps US",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "LOUGHUNI1",
                    Name = "Loughborough University",
                    Description = "Loughborough University",
                    Type = "Level_2",
                    Parent = "ACADEMIC_TRAINING_RESEARC",
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
                    Code = "MAI001",
                    Name = "Maisha",
                    Description = "Maisha",
                    Type = "Level_2",
                    Parent = "PRIVATE_SECTOR",
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
                    Code = "MINDFUND",
                    Name = "Minderoo Foundation",
                    Description = "Minderoo Foundation",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "NEH001",
                    Name = "Nehemia",
                    Description = "Nehemia",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "PAR001",
                    Name = "Parexel",
                    Description = "Parexel",
                    Type = "Level_2",
                    Parent = "PRIVATE_SECTOR",
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
                    Code = "PLANINTMALI001",
                    Name = "Plan International Mali",
                    Description = "Plan International Mali",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "PLINTLIB",
                    Name = "Plan International Liberia",
                    Description = "Plan International Liberia",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "PSIMAD",
                    Name = "Population Services International Madagascar",
                    Description = "PSI Madagascar",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "RBF001",
                    Name = "Rockefeller Brothers Fund",
                    Description = "Rockefeller Brothers Fund",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "RFCC1",
                    Name = "RF Catalytic Capital",
                    Description = "RF Catalytic Capital",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "SGFF001",
                    Name = "Sergey Brin Family Foundation",
                    Description = "Sergey Brin Family Foundation",
                    Type = "Level_2",
                    Parent = "FOUNDATION",
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
                    Code = "SRAGROPL",
                    Name = "Sow & Reap Agro Private Limited",
                    Description = "Sow & Reap Agro Pvt Ltd",
                    Type = "Level_2",
                    Parent = "PRIVATE_SECTOR",
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
                    Code = "SU001",
                    Name = "Stellenbosh University",
                    Description = "SU",
                    Type = "Level_2",
                    Parent = "ACADEMIC_TRAINING_RESEARC",
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
                    Code = "SWANITI001",
                    Name = "Swaniti Initiative",
                    Description = "Swaniti Initiative",
                    Type = "Level_2",
                    Parent = "NGO",
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
                    Code = "UCD001",
                    Name = "UC Davis",
                    Description = "UC Davis",
                    Type = "Level_2",
                    Parent = "ACADEMIC_TRAINING_RESEARC",
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
                    Code = "UNISID1",
                    Name = "The University of Sidney",
                    Description = "Sidney University",
                    Type = "Level_2",
                    Parent = "ACADEMIC_TRAINING_RESEARC",
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
                    Code = "FRDREPUZB",
                    Name = "Fund for Reconstruction and Development of the Republic of Uzbekistan",
                    Description = "Fund for Reconstruction and Development of the Republic of Uzbekistan",
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
                    Code = "NATO",
                    Name = "NATO",
                    Description = "NATO",
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
                    Code = "USMBHC001",
                    Name = "USMBHC The United States-Mexico Border Health Commission",
                    Description = "USMBHC The United States-Mexico Border Health Commission",
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
                    Code = "BOAD1",
                    Name = "West African Development Bank",
                    Description = "West African Development Bank",
                    Type = "Level_4",
                    Parent = "REG_OTH_FI",
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
                    Code = "CEF01",
                    Name = "Caixa Economica Federal",
                    Description = "Caixa Economica Federal",
                    Type = "Level_4",
                    Parent = "BRAZIL",
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
                    Code = "CNECR",
                    Name = "Comisión nacional de prevención de riesgos y atención de emergencias",
                    Description = "Comisión nacional de prevención de riesgos y atención de emergencias",
                    Type = "Level_4",
                    Parent = "COSTA_RICA",
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
                    Code = "COMPANHIAPAULISTA",
                    Name = "CPP - Companhia Paulista de Parcerias",
                    Description = "CPP - Companhia Paulista de Parcerias",
                    Type = "Level_4",
                    Parent = "BRAZIL",
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
                    Code = "EC",
                    Name = "EC European Commission",
                    Description = "EC European Commission",
                    Type = "Level_4",
                    Parent = "EU",
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
                    Code = "EPMMOP001",
                    Name = "EPMMOP",
                    Description = "EPMMOP",
                    Type = "Level_4",
                    Parent = "ECUADOR",
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
                    Code = "INCOFER",
                    Name = "Institituto Costarricense de Ferrocarriles",
                    Description = "Institituto Costarricense de Ferrocarriles",
                    Type = "Level_4",
                    Parent = "COSTA_RICA",
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
                    Code = "WATPNG1",
                    Name = "Water PNG",
                    Description = "Water PNG",
                    Type = "Level_4",
                    Parent = "PNG001",
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