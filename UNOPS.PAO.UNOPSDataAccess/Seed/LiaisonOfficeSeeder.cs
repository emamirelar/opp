using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class LiaisonOfficeSeeder
    {
        public static async Task SeedLiaisonOfficesAsync(UNOPSAppDbContext context)
        {
            if (await context.LiaisonOffices.AnyAsync())
            {
                return;
            }

            var liaisonOffices = new List<LiaisonOffice>
            {
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXKIAY",
                    Name = "Other Partners",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXLIAY",
                    Name = "Northern Europe Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXMIAY",
                    Name = "Washington Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXNIAY",
                    Name = "Tokyo Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXOIAY",
                    Name = "Manila Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXPIAY",
                    Name = "Gulf Countries Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXQIAY",
                    Name = "Brussels Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXRIAY",
                    Name = "New York Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXSIAY",
                    Name = "Geneva Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000jsXTIAY",
                    Name = "Nairobi Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx000000sTEjIAM",
                    Name = "Bangkok Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx00000BPmpVIAT",
                    Name = "Rome Liaison Office",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new LiaisonOffice
                {
                    Code = "a0bQx00000CT3YIIA1",
                    Name = "Other PLG Managed Partners",
                    IsActive = true,
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }
            };

            await context.LiaisonOffices.AddRangeAsync(liaisonOffices);
            await context.SaveChangesAsync();
        }
    }
}