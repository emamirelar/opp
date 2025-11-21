using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Seeds LiaisonOffices with proper insert/update/delete logic
    /// </summary>
    public static class LiaisonOfficeSeeder
    {
        public static async Task SeedLiaisonOfficesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 Seeding Liaison Offices...");

            var liaisonOfficesToSeed = new List<(string Code, string Name, bool IsActive)>
            {
                ("a0bQx000000jsXKIAY", "Other Partners", true),
                ("a0bQx000000jsXLIAY", "Northern Europe Liaison Office", true),
                ("a0bQx000000jsXMIAY", "Washington Liaison Office", true),
                ("a0bQx000000jsXNIAY", "Tokyo Liaison Office", true),
                ("a0bQx000000jsXOIAY", "Manila Liaison Office", true),
                ("a0bQx000000jsXPIAY", "Gulf Countries Liaison Office", true),
                ("a0bQx000000jsXQIAY", "Brussels Liaison Office", true),
                ("a0bQx000000jsXRIAY", "New York Liaison Office", true),
                ("a0bQx000000jsXSIAY", "Geneva Liaison Office", true),
                ("a0bQx000000jsXTIAY", "Nairobi Liaison Office", true),
                ("a0bQx000000sTEjIAM", "Bangkok Liaison Office", true),
                ("a0bQx00000BPmpVIAT", "Rome Liaison Office", true),
                ("a0bQx00000CT3YIIA1", "Other PLG Managed Partners", true)
            };

            // Get existing liaison offices from database
            var existingLiaisonOffices = await context.LiaisonOffices.ToListAsync();

            var liaisonOfficeCodesToKeep = liaisonOfficesToSeed.Select(lo => lo.Code).ToHashSet();

            // Insert or Update liaison offices
            foreach (var (code, name, isActive) in liaisonOfficesToSeed)
            {
                var existingLiaisonOffice = existingLiaisonOffices.FirstOrDefault(lo => lo.Code == code);

                if (existingLiaisonOffice == null)
                {
                    // Insert new liaison office
                    var newLiaisonOffice = new LiaisonOffice
                    {
                        Code = code,
                        Name = name,
                        IsActive = isActive,
                        Status = EntityStatus.Active,
                        CreatedBy = 0,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        LastModifiedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedBy = 0
                    };
                    
                    context.LiaisonOffices.Add(newLiaisonOffice);
                    Console.WriteLine($"  ✅ Inserted liaison office: {name}");
                }
                else
                {
                    // Update if any properties changed
                    bool hasChanges = false;

                    if (existingLiaisonOffice.Name != name)
                    {
                        existingLiaisonOffice.Name = name;
                        hasChanges = true;
                    }

                    if (existingLiaisonOffice.IsActive != isActive)
                    {
                        existingLiaisonOffice.IsActive = isActive;
                        hasChanges = true;
                    }

                    if (existingLiaisonOffice.IsDeleted)
                    {
                        existingLiaisonOffice.IsDeleted = false;
                        hasChanges = true;
                    }

                    if (existingLiaisonOffice.Status != EntityStatus.Active)
                    {
                        existingLiaisonOffice.Status = EntityStatus.Active;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingLiaisonOffice.LastModifiedBy = 0;
                        existingLiaisonOffice.LastModifiedDate = DateTime.UtcNow;
                        Console.WriteLine($"  🔄 Updated liaison office: {name}");
                    }
                    else
                    {
                        Console.WriteLine($"  ⏭️  Skipped liaison office (unchanged): {name}");
                    }
                }
            }

            // Delete liaison offices that are no longer in the seed list
            var liaisonOfficesToDelete = existingLiaisonOffices
                .Where(lo => !liaisonOfficeCodesToKeep.Contains(lo.Code))
                .ToList();

            foreach (var liaisonOfficeToDelete in liaisonOfficesToDelete)
            {
                context.LiaisonOffices.Remove(liaisonOfficeToDelete);
                Console.WriteLine($"  🗑️  Deleted liaison office: {liaisonOfficeToDelete.Name}");
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Liaison Offices seeding completed\n");
        }
    }
}

