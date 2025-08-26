using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class DocumentTypeSeeder
    {
        public static async Task SeedDocumentTypesAsync(UNOPSAppDbContext context)
        {
            if (await context.DocumentTypes.AnyAsync())
            {
                return;
            }

            var documentTypes = new List<DocumentType>
            {
                new DocumentType
                {
                    EntityType = "Partner",
                    Name = "Guidance",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new DocumentType
                {
                    EntityType = "Partner",
                    Name = "Partnership Agreement",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new DocumentType
                {
                    EntityType = "Partner",
                    Name = "Standard Template",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new DocumentType
                {
                    EntityType = "Contact",
                    Name = "Other",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new DocumentType
                {
                    EntityType = "Interaction",
                    Name = "Meeting Note",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                },
                new DocumentType
                {
                    EntityType = "PartnerTree",
                    Name = "Other",
                    Status = (EntityStatus)1,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedBy = 0
                }
            };

            await context.DocumentTypes.AddRangeAsync(documentTypes);
            await context.SaveChangesAsync();
        }
    }
}