using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public class EntityRoleSeeder
{
    public static async Task SeedEntityRolesAsync(UNOPSAppDbContext context)
    {
        await SeedOpportunityRolesAsync(context);
        await SeedOrganizationHierarchyRolesAsync(context);
    }

    /// <summary>
    /// Adds a role if it doesn't exist, or updates it if it does exist (by EntityType and Name)
    /// Returns true if role was added, false if it was updated
    /// </summary>
    private static async Task<bool> AddOrUpdateRoleAsync(UNOPSAppDbContext context, EntityRole role)
    {
        var existingRole = await context.EntityRoles
            .FirstOrDefaultAsync(er => er.EntityType == role.EntityType && er.Name == role.Name);

        if (existingRole != null)
        {
            // Update existing role properties (preserve Id, CreatedDate, CreatedBy)
            existingRole.Description = role.Description;
            existingRole.Type = role.Type;
            existingRole.SubType = role.SubType;
            existingRole.IsInternal = role.IsInternal;
            existingRole.AllowsMultiple = role.AllowsMultiple;
            existingRole.Status = role.Status;
            existingRole.LastModifiedDate = DateTime.UtcNow;
            existingRole.LastModifiedBy = 1; // System user
            
            context.EntityRoles.Update(existingRole);
            return false; // Updated, not added
        }

        await context.EntityRoles.AddAsync(role);
        return true; // Added
    }

    private static async Task SeedOpportunityRolesAsync(UNOPSAppDbContext context)
    {
        var rolesToSeed = new List<EntityRole>
        {
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Opportunity Manager",
                Description = "Primary manager responsible for overall opportunity strategy, stakeholder engagement, and successful delivery",
                IsInternal = true,
                AllowsMultiple = false,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Partnership Lead",
                Description = "Lead responsible for partnership development, relationship management, and collaboration with partners",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Reviewer",
                Description = "Reviewer responsible for quality assurance, compliance checks, and approval workflows",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Internal Stakeholder",
                Description = "Internal UNOPS stakeholder involved in the opportunity",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "External Stakeholder",
                Description = "External stakeholder or partner contact involved in the opportunity",
                IsInternal = false,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Infrastructure",
                Description = "Subject Matter Expert providing infrastructure expertise and guidance",
                Type = "SME",
                SubType = "Service Line",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Project Management",
                Description = "Subject Matter Expert providing project management expertise and guidance",
                Type = "SME",
                SubType = "Service Line",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Human Resources",
                Description = "Subject Matter Expert providing human resources expertise and guidance",
                Type = "SME",
                SubType = "Service Line",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Financial Management",
                Description = "Subject Matter Expert providing financial management expertise and guidance",
                Type = "SME",
                SubType = "Service Line",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Procurement",
                Description = "Subject Matter Expert providing procurement expertise and guidance",
                Type = "SME",
                SubType = "Service Line",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - GESI",
                Description = "Subject Matter Expert providing Gender Equality and Social Inclusion expertise and guidance",
                Type = "SME",
                SubType = "Other",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - HSSE",
                Description = "Subject Matter Expert providing Health, Safety, Social and Environmental expertise and guidance",
                Type = "SME",
                SubType = "Other",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Results Management",
                Description = "Subject Matter Expert providing results management expertise and guidance",
                Type = "SME",
                SubType = "Other",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "Opportunity",
                Name = "SME - Risk Management",
                Description = "Subject Matter Expert providing risk management expertise and guidance",
                Type = "SME",
                SubType = "Other",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            }
        };

        var addedCount = 0;
        var updatedCount = 0;
        foreach (var role in rolesToSeed)
        {
            if (await AddOrUpdateRoleAsync(context, role))
            {
                addedCount++;
            }
            else
            {
                updatedCount++;
            }
        }

        if (addedCount > 0 || updatedCount > 0)
        {
            await context.SaveChangesAsync();
            if (addedCount > 0 && updatedCount > 0)
            {
                Console.WriteLine($"Seeded {addedCount} new and updated {updatedCount} existing EntityRoles for Opportunity entity.");
            }
            else if (addedCount > 0)
            {
                Console.WriteLine($"Seeded {addedCount} new EntityRoles for Opportunity entity.");
            }
            else
            {
                Console.WriteLine($"Updated {updatedCount} existing EntityRoles for Opportunity entity.");
            }
        }
        else
        {
            Console.WriteLine("No EntityRoles for Opportunity were added or updated.");
        }
    }

    private static async Task SeedOrganizationHierarchyRolesAsync(UNOPSAppDbContext context)
    {
        var rolesToSeed = new List<EntityRole>
        {
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Region Director",
                Description = "Director responsible for overseeing the entire region",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Region Deputy Director",
                Description = "Deputy Director supporting the Region Director",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Hub Director",
                Description = "Director responsible for overseeing a hub",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "Hub Deputy Director",
                Description = "Deputy Director supporting the Hub Director",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "OrgUnit Director",
                Description = "Director responsible for overseeing an organizational unit",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "OrgUnit Deputy Director",
                Description = "Deputy Director supporting the OrgUnit Director",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA1",
                Description = "Delegation of Authority Level 1",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA2",
                Description = "Delegation of Authority Level 2",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA3",
                Description = "Delegation of Authority Level 3",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new EntityRole
            {
                EntityType = "OrganizationHierarchy",
                Name = "DoA4",
                Description = "Delegation of Authority Level 4",
                IsInternal = true,
                AllowsMultiple = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            }
        };

        var addedCount = 0;
        var updatedCount = 0;
        foreach (var role in rolesToSeed)
        {
            if (await AddOrUpdateRoleAsync(context, role))
            {
                addedCount++;
            }
            else
            {
                updatedCount++;
            }
        }

        if (addedCount > 0 || updatedCount > 0)
        {
            await context.SaveChangesAsync();
            if (addedCount > 0 && updatedCount > 0)
            {
                Console.WriteLine($"Seeded {addedCount} new and updated {updatedCount} existing EntityRoles for OrganizationHierarchy entity.");
            }
            else if (addedCount > 0)
            {
                Console.WriteLine($"Seeded {addedCount} new EntityRoles for OrganizationHierarchy entity.");
            }
            else
            {
                Console.WriteLine($"Updated {updatedCount} existing EntityRoles for OrganizationHierarchy entity.");
            }
        }
        else
        {
            Console.WriteLine("No EntityRoles for OrganizationHierarchy were added or updated.");
        }
    }
}

