using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Seeds Entities with proper insert/update/delete logic
    /// </summary>
    public static class EntitySeeder
    {
        public static async Task SeedEntitiesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 Seeding Entities...");

            var entitiesToSeed = new List<(string EntityName, string Name, EntityStatus Status, bool IsActive, bool CanManage)>
            {
                ("Contact", "Contact", EntityStatus.Draft, true, true),
                ("Partner", "Partner", EntityStatus.Draft, true, true),
                ("Interaction", "Interaction", EntityStatus.Draft, true, true),
                ("PartnerTree", "PartnerTree", EntityStatus.Draft, true, true),
                ("OrganizationHierarchy", "OrganizationHierarchy", EntityStatus.Draft, true, false),
                ("Opportunity", "Opportunity", EntityStatus.Draft, true, false),
            };

            // Get existing entities from database
            var existingEntities = await context.Entities.ToListAsync();

            var entityNamesToKeep = entitiesToSeed.Select(e => e.EntityName).ToHashSet();

            // Insert or Update entities
            foreach (var (entityName, name, status, isActive, canManage) in entitiesToSeed)
            {
                var existingEntity = existingEntities.FirstOrDefault(e => e.EntityName == entityName);

                if (existingEntity == null)
                {
                    // Insert new entity
                    var newEntity = new Entities
                    {
                        EntityName = entityName,
                        Name = name,
                        Status = status,
                        IsActive = isActive,
                        CanManage = canManage,
                        CreatedBy = 1,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = null,
                        IsDeleted = false,
                        DeletedBy = 0,
                        DeletedDate = null
                    };
                    
                    context.Entities.Add(newEntity);
                    Console.WriteLine($"  ✅ Inserted entity: {entityName}");
                }
                else
                {
                    // Update if any properties changed
                    bool hasChanges = false;

                    if (existingEntity.Name != name)
                    {
                        existingEntity.Name = name;
                        hasChanges = true;
                    }

                    if (existingEntity.Status != status)
                    {
                        existingEntity.Status = status;
                        hasChanges = true;
                    }

                    if (existingEntity.IsActive != isActive)
                    {
                        existingEntity.IsActive = isActive;
                        hasChanges = true;
                    }

                    if (existingEntity.CanManage != canManage)
                    {
                        existingEntity.CanManage = canManage;
                        hasChanges = true;
                    }

                    if (existingEntity.IsDeleted)
                    {
                        existingEntity.IsDeleted = false;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingEntity.LastModifiedBy = 0;
                        existingEntity.LastModifiedDate = DateTime.UtcNow;
                        Console.WriteLine($"  🔄 Updated entity: {entityName}");
                    }
                    else
                    {
                        Console.WriteLine($"  ⏭️  Skipped entity (unchanged): {entityName}");
                    }
                }
            }

            // Delete entities that are no longer in the seed list
            var entitiesToDelete = existingEntities
                .Where(e => !entityNamesToKeep.Contains(e.EntityName))
                .ToList();

            foreach (var entityToDelete in entitiesToDelete)
            {
                context.Entities.Remove(entityToDelete);
                Console.WriteLine($"  🗑️  Deleted entity: {entityToDelete.EntityName}");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Entities seeding completed\n");
        }
    }
}

