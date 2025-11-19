using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Seeds DocumentTypes with proper insert/update/delete logic
    /// </summary>
    public static class DocumentTypeSeeder
    {
        public static async Task SeedDocumentTypesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 Seeding Document Types...");

            var documentTypesToSeed = new List<(string EntityType, string Name)>
            {
                // Partner document types
                ("Partner", "Partner/National strategic plan"),
                ("Partner", "Concept note"),
                ("Partner", "Action plan"),
                ("Partner", "Mission report"),
                ("Partner", "Draft/working document"),
                ("Partner", "Presentation"),
                ("Partner", "Factsheet"),
                ("Partner", "Reports produced by partner"),
                ("Partner", "Other"),
                
                // Contact document types
                ("Contact", "CV/Bio"),
                ("Contact", "Talking points"),
                ("Contact", "Background note"),
                ("Contact", "Other"),
                
                // Interaction document types
                ("Interaction", "Minutes"),
                ("Interaction", "Action plan/next steps"),
                ("Interaction", "Supporting materials"),
                ("Interaction", "Other"),
                
                // PartnerTree document types
                ("PartnerTree", "Other"),
                
                // Opportunity document types
                ("Opportunity", "Concept Note"),
                ("Opportunity", "Proposal"),
                ("Opportunity", "Other")
            };

            // Get existing document types from database
            var existingDocumentTypes = await context.DocumentTypes.ToListAsync();

            var documentTypeKeys = documentTypesToSeed
                .Select(dt => $"{dt.EntityType}|{dt.Name}")
                .ToHashSet();

            // Insert or Update document types
            foreach (var (entityType, name) in documentTypesToSeed)
            {
                var existingDocType = existingDocumentTypes
                    .FirstOrDefault(dt => dt.EntityType == entityType && dt.Name == name);

                if (existingDocType == null)
                {
                    // Insert new document type
                    var newDocType = new DocumentType
                    {
                        EntityType = entityType,
                        Name = name,
                        Status = EntityStatus.Active,
                        CreatedBy = 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    };
                    
                    context.DocumentTypes.Add(newDocType);
                    Console.WriteLine($"  ✅ Inserted document type: {entityType} - {name}");
                }
                else
                {
                    // Update if needed (in this case, just mark as modified)
                    existingDocType.LastModifiedBy = 0;
                    existingDocType.LastModifiedDate = DateTime.UtcNow;
                    existingDocType.IsDeleted = false;
                    existingDocType.Status = EntityStatus.Active;
                    Console.WriteLine($"  ⏭️  Verified document type: {entityType} - {name}");
                }
            }

            // Delete document types that are no longer in the seed list
            var documentTypesToDelete = existingDocumentTypes
                .Where(dt => !documentTypeKeys.Contains($"{dt.EntityType}|{dt.Name}"))
                .ToList();

            foreach (var docTypeToDelete in documentTypesToDelete)
            {
                context.DocumentTypes.Remove(docTypeToDelete);
                Console.WriteLine($"  🗑️  Deleted document type: {docTypeToDelete.EntityType} - {docTypeToDelete.Name}");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Document Types seeding completed\n");
        }
    }
}

