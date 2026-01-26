using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeder for CollaboratorExpertise lookup table.
/// Seeds the expertise types that collaborators can be assigned to indicate their capacity on an opportunity.
/// </summary>
public static class CollaboratorExpertiseSeeder
{
    public static async Task SeedCollaboratorExpertisesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Collaborator Expertises...");

        var expertisesToSeed = GetExpertisesToSeed();
        var existingExpertises = await context.Set<CollaboratorExpertise>().ToListAsync();

        foreach (var expertiseData in expertisesToSeed)
        {
            var existingExpertise = existingExpertises.FirstOrDefault(e => e.Code == expertiseData.Code);

            if (existingExpertise == null)
            {
                context.Set<CollaboratorExpertise>().Add(expertiseData);
                Console.WriteLine($"  ✅ Inserted Collaborator Expertise: {expertiseData.Code} - {expertiseData.Name}");
            }
            else
            {
                bool hasChanges = false;

                if (existingExpertise.Name != expertiseData.Name)
                {
                    existingExpertise.Name = expertiseData.Name;
                    hasChanges = true;
                }

                if (existingExpertise.Description != expertiseData.Description)
                {
                    existingExpertise.Description = expertiseData.Description;
                    hasChanges = true;
                }

                if (existingExpertise.DisplayOrder != expertiseData.DisplayOrder)
                {
                    existingExpertise.DisplayOrder = expertiseData.DisplayOrder;
                    hasChanges = true;
                }

                if (existingExpertise.Status != expertiseData.Status)
                {
                    existingExpertise.Status = expertiseData.Status;
                    hasChanges = true;
                }

                if (existingExpertise.IsDeleted)
                {
                    existingExpertise.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated Collaborator Expertise: {expertiseData.Code} - {expertiseData.Name}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped Collaborator Expertise (unchanged): {expertiseData.Code} - {expertiseData.Name}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Collaborator Expertises seeding completed\n");
    }

    private static List<CollaboratorExpertise> GetExpertisesToSeed()
    {
        return new List<CollaboratorExpertise>
        {
            new CollaboratorExpertise
            {
                Code = "GEN_OPP_DEV",
                Name = "General Opportunity Development",
                Description = "General expertise in opportunity development and management",
                DisplayOrder = 1,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "FIN_MGMT",
                Name = "Financial Management",
                Description = "Expertise in financial management, budgeting, and financial planning",
                DisplayOrder = 2,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "HR",
                Name = "Human Resources",
                Description = "Expertise in human resources management and personnel",
                DisplayOrder = 3,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "INFRA",
                Name = "Infrastructure",
                Description = "Expertise in infrastructure development and management",
                DisplayOrder = 4,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "PROC",
                Name = "Procurement",
                Description = "Expertise in procurement processes and supply chain management",
                DisplayOrder = 5,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "PROJ_MGMT",
                Name = "Project Management",
                Description = "Expertise in project management methodologies and practices",
                DisplayOrder = 6,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "GESI",
                Name = "Gender Equality & Social Inclusion (GESI)",
                Description = "Expertise in gender equality and social inclusion practices",
                DisplayOrder = 7,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "HSSE",
                Name = "HSSE",
                Description = "Expertise in Health, Safety, Security, and Environment",
                DisplayOrder = 8,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "RESULTS_MGMT",
                Name = "Results Management",
                Description = "Expertise in results-based management and monitoring",
                DisplayOrder = 9,
                Status = EntityStatus.Active,
                IsDeleted = false
            },
            new CollaboratorExpertise
            {
                Code = "RISK_MGMT",
                Name = "Risk Management",
                Description = "Expertise in risk identification, assessment, and mitigation",
                DisplayOrder = 10,
                Status = EntityStatus.Active,
                IsDeleted = false
            }
        };
    }
}
