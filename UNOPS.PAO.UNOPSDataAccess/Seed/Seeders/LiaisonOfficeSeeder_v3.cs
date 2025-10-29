using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Seeds LiaisonOffices with proper insert/update/delete logic
    /// </summary>
    public static class LiaisonOfficeSeeder_v3
    {
        public static async Task SeedLiaisonOfficesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 V3 Seeding Liaison Offices...");

            var liaisonOfficesToSeed = new List<(string Code, string Name, bool IsActive)>
            {
                ("a0bQx00000CT3YIIA1", "Other PLG Partners", true)
            };

            // Get existing liaison offices from database
            var existingLiaisonOffices = await context.LiaisonOffices.ToListAsync();

            // Insert or Update liaison offices
            foreach (var (code, name, isActive) in liaisonOfficesToSeed)
            {
                var existingLiaisonOffice = existingLiaisonOffices.FirstOrDefault(lo => lo.Name == name || lo.Code == code);

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
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Liaison Offices v3 seeding completed\n");
        }
    }
}

