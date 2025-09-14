using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class EntitiesSeeder
    {
        public static async Task SeedEntitiesAsync(UNOPSAppDbContext context)
        {
            if (await context.Entities.AnyAsync())
            {
                return;
            }

            var entities = new List<Entities>
            {
                new Entities
                {
                    EntityName = "Contact",
                    Name = "Contact",
                    Status = 0,
                    IsActive = true,
                    CanManage = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new Entities
                {
                    EntityName = "Partner",
                    Name = "Partner",
                    Status = 0,
                    IsActive = true,
                    CanManage = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new Entities
                {
                    EntityName = "Interaction",
                    Name = "Interaction",
                    Status = 0,
                    IsActive = true,
                    CanManage = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new Entities
                {
                    EntityName = "PartnerTree",
                    Name = "PartnerTree",
                    Status = 0,
                    IsActive = true,
                    CanManage = true,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new Entities
                {
                    EntityName = "OrganizationHierarchy",
                    Name = "OrganizationHierarchy",
                    Status = 0,
                    IsActive = true,
                    CanManage = false,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };

            await context.Entities.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }
    }
}