using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds EntityUserRole records for DoA (Delegation of Authority) roles linking 
/// OrganizationHierarchy entities to Users.
/// Generated from OrgUnit_DoA - ImportFile.csv
/// </summary>
public class OrgUnitDOARolesSeeder
{
    public static async Task SeedOrgUnitDOARolesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("Starting OrgUnitDOARolesSeeder...");
        
        // Get all DoA EntityRoles for OrganizationHierarchy
        var entityRoles = await context.EntityRoles
            .Where(er => er.EntityType == "OrganizationHierarchy" 
                && (er.Name == "DoA1" || er.Name == "DoA2" || er.Name == "DoA3" || er.Name == "DoA4")
                && !er.IsDeleted)
            .ToListAsync();
        
        var roleNameToId = entityRoles.ToDictionary(r => r.Name, r => r.Id);
        
        // Get all users by email (case-insensitive)
        var allUsers = await context.PAOUsers
            .Where(u => u.Email != null)
            .Select(u => new { u.Id, Email = u.Email!.ToLower() })
            .ToListAsync();
        
        var emailToUserId = allUsers
            .GroupBy(u => u.Email)
            .ToDictionary(g => g.Key, g => g.First().Id);
        
        // Get all OrgUnit type OrganizationHierarchy records by Code
        var orgUnits = await context.OrganizationHierarchies
            .Where(oh => oh.Type == OrganizationUnitType.OrgUnit && !oh.IsDeleted)
            .Select(oh => new { oh.Id, oh.Code })
            .ToListAsync();
        
        var codeToOrgUnitId = orgUnits.ToDictionary(o => o.Code, o => o.Id);
        
        // Get existing EntityUserRoles to avoid duplicates
        var existingRoles = await context.EntityUserRoles
            .Where(eur => eur.EntityType == "OrganizationHierarchy" && !eur.IsDeleted)
            .Select(eur => new { eur.EntityId, eur.EntityRoleId, eur.UserId })
            .ToListAsync();
        
        var existingRoleKeys = existingRoles
            .Select(r => $"{r.EntityId}_{r.EntityRoleId}_{r.UserId}")
            .ToHashSet();
        
        var rolesToAdd = new List<EntityUserRole>();
        var skippedCount = 0;
        var missingOrgUnits = new HashSet<string>();
        var missingUsers = new HashSet<string>();
        var missingRoles = new HashSet<string>();
        
        // Process DoA role assignments

        // B0004
        if (codeToOrgUnitId.TryGetValue("B0004", out var orgUnit_B0004Id))
        {
            // DoA3: liliann@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0004_DoA3_3690Id) &&
                emailToUserId.TryGetValue("liliann@unops.org", out var user_B0004_DoA3_3690Id))
            {
                var key_B0004_DoA3_3690 = $"{orgUnit_B0004Id}_{role_B0004_DoA3_3690Id}_{user_B0004_DoA3_3690Id}";
                if (!existingRoleKeys.Contains(key_B0004_DoA3_3690))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0004Id} - {user_B0004_DoA3_3690Id}",
                        EntityId = orgUnit_B0004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0004_DoA3_3690Id,
                        UserId = user_B0004_DoA3_3690Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0004_DoA3_3690); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("liliann@unops.org")) missingUsers.Add("liliann@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0004");
        }

        // B0012
        if (codeToOrgUnitId.TryGetValue("B0012", out var orgUnit_B0012Id))
        {
            // DoA3: anneclaireh@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0012_DoA3_4532Id) &&
                emailToUserId.TryGetValue("anneclaireh@unops.org", out var user_B0012_DoA3_4532Id))
            {
                var key_B0012_DoA3_4532 = $"{orgUnit_B0012Id}_{role_B0012_DoA3_4532Id}_{user_B0012_DoA3_4532Id}";
                if (!existingRoleKeys.Contains(key_B0012_DoA3_4532))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0012Id} - {user_B0012_DoA3_4532Id}",
                        EntityId = orgUnit_B0012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0012_DoA3_4532Id,
                        UserId = user_B0012_DoA3_4532Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0012_DoA3_4532); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("anneclaireh@unops.org")) missingUsers.Add("anneclaireh@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0012");
        }

        // B0050
        if (codeToOrgUnitId.TryGetValue("B0050", out var orgUnit_B0050Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0050_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B0050_DoA3_6407Id))
            {
                var key_B0050_DoA3_6407 = $"{orgUnit_B0050Id}_{role_B0050_DoA3_6407Id}_{user_B0050_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B0050_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_DoA3_6407Id}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_DoA3_6407Id,
                        UserId = user_B0050_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0050_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B0050_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B0050_DoA2_6407Id))
            {
                var key_B0050_DoA2_6407 = $"{orgUnit_B0050Id}_{role_B0050_DoA2_6407Id}_{user_B0050_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B0050_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_DoA2_6407Id}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_DoA2_6407Id,
                        UserId = user_B0050_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0050_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0050_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B0050_DoA3_3632Id))
            {
                var key_B0050_DoA3_3632 = $"{orgUnit_B0050Id}_{role_B0050_DoA3_3632Id}_{user_B0050_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B0050_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_DoA3_3632Id}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_DoA3_3632Id,
                        UserId = user_B0050_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0050_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0050");
        }

        // B0051
        if (codeToOrgUnitId.TryGetValue("B0051", out var orgUnit_B0051Id))
        {
            // DoA1: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B0051_DoA1_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B0051_DoA1_3918Id))
            {
                var key_B0051_DoA1_3918 = $"{orgUnit_B0051Id}_{role_B0051_DoA1_3918Id}_{user_B0051_DoA1_3918Id}";
                if (!existingRoleKeys.Contains(key_B0051_DoA1_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B0051Id} - {user_B0051_DoA1_3918Id}",
                        EntityId = orgUnit_B0051Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0051_DoA1_3918Id,
                        UserId = user_B0051_DoA1_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0051_DoA1_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0051_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B0051_DoA3_3918Id))
            {
                var key_B0051_DoA3_3918 = $"{orgUnit_B0051Id}_{role_B0051_DoA3_3918Id}_{user_B0051_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B0051_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0051Id} - {user_B0051_DoA3_3918Id}",
                        EntityId = orgUnit_B0051Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0051_DoA3_3918Id,
                        UserId = user_B0051_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0051_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0051");
        }

        // B0053
        if (codeToOrgUnitId.TryGetValue("B0053", out var orgUnit_B0053Id))
        {
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0053_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B0053_DoA3_8676Id))
            {
                var key_B0053_DoA3_8676 = $"{orgUnit_B0053Id}_{role_B0053_DoA3_8676Id}_{user_B0053_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B0053_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0053Id} - {user_B0053_DoA3_8676Id}",
                        EntityId = orgUnit_B0053Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0053_DoA3_8676Id,
                        UserId = user_B0053_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0053_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0053");
        }

        // B0054
        if (codeToOrgUnitId.TryGetValue("B0054", out var orgUnit_B0054Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0054_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B0054_DoA3_120Id))
            {
                var key_B0054_DoA3_120 = $"{orgUnit_B0054Id}_{role_B0054_DoA3_120Id}_{user_B0054_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B0054_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_DoA3_120Id}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_DoA3_120Id,
                        UserId = user_B0054_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0054_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0054_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B0054_DoA3_8818Id))
            {
                var key_B0054_DoA3_8818 = $"{orgUnit_B0054Id}_{role_B0054_DoA3_8818Id}_{user_B0054_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B0054_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_DoA3_8818Id}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_DoA3_8818Id,
                        UserId = user_B0054_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0054_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B0054_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B0054_DoA3_9564Id))
            {
                var key_B0054_DoA3_9564 = $"{orgUnit_B0054Id}_{role_B0054_DoA3_9564Id}_{user_B0054_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B0054_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_DoA3_9564Id}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_DoA3_9564Id,
                        UserId = user_B0054_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B0054_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0054");
        }

        // B5001
        if (codeToOrgUnitId.TryGetValue("B5001", out var orgUnit_B5001Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5001_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5001_DoA3_8767Id))
            {
                var key_B5001_DoA3_8767 = $"{orgUnit_B5001Id}_{role_B5001_DoA3_8767Id}_{user_B5001_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA3_8767Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA3_8767Id,
                        UserId = user_B5001_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5001_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5001_DoA2_402Id))
            {
                var key_B5001_DoA2_402 = $"{orgUnit_B5001Id}_{role_B5001_DoA2_402Id}_{user_B5001_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA2_402Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA2_402Id,
                        UserId = user_B5001_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5001_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5001_DoA2_4134Id))
            {
                var key_B5001_DoA2_4134 = $"{orgUnit_B5001Id}_{role_B5001_DoA2_4134Id}_{user_B5001_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA2_4134Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA2_4134Id,
                        UserId = user_B5001_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5001_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5001_DoA1_2171Id))
            {
                var key_B5001_DoA1_2171 = $"{orgUnit_B5001Id}_{role_B5001_DoA1_2171Id}_{user_B5001_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA1_2171Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA1_2171Id,
                        UserId = user_B5001_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5001_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5001_DoA1_307Id))
            {
                var key_B5001_DoA1_307 = $"{orgUnit_B5001Id}_{role_B5001_DoA1_307Id}_{user_B5001_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA1_307Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA1_307Id,
                        UserId = user_B5001_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5001_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5001_DoA2_2171Id))
            {
                var key_B5001_DoA2_2171 = $"{orgUnit_B5001Id}_{role_B5001_DoA2_2171Id}_{user_B5001_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA2_2171Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA2_2171Id,
                        UserId = user_B5001_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5001_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5001_DoA2_307Id))
            {
                var key_B5001_DoA2_307 = $"{orgUnit_B5001Id}_{role_B5001_DoA2_307Id}_{user_B5001_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA2_307Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA2_307Id,
                        UserId = user_B5001_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5001_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5001_DoA3_2942Id))
            {
                var key_B5001_DoA3_2942 = $"{orgUnit_B5001Id}_{role_B5001_DoA3_2942Id}_{user_B5001_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5001_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_DoA3_2942Id}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_DoA3_2942Id,
                        UserId = user_B5001_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5001_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5001");
        }

        // B5002
        if (codeToOrgUnitId.TryGetValue("B5002", out var orgUnit_B5002Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5002_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5002_DoA2_6407Id))
            {
                var key_B5002_DoA2_6407 = $"{orgUnit_B5002Id}_{role_B5002_DoA2_6407Id}_{user_B5002_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA2_6407Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA2_6407Id,
                        UserId = user_B5002_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5002_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5002_DoA3_8767Id))
            {
                var key_B5002_DoA3_8767 = $"{orgUnit_B5002Id}_{role_B5002_DoA3_8767Id}_{user_B5002_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA3_8767Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA3_8767Id,
                        UserId = user_B5002_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5002_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5002_DoA1_9154Id))
            {
                var key_B5002_DoA1_9154 = $"{orgUnit_B5002Id}_{role_B5002_DoA1_9154Id}_{user_B5002_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA1_9154Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA1_9154Id,
                        UserId = user_B5002_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5002_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5002_DoA2_729Id))
            {
                var key_B5002_DoA2_729 = $"{orgUnit_B5002Id}_{role_B5002_DoA2_729Id}_{user_B5002_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA2_729Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA2_729Id,
                        UserId = user_B5002_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5002_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5002_DoA1_175Id))
            {
                var key_B5002_DoA1_175 = $"{orgUnit_B5002Id}_{role_B5002_DoA1_175Id}_{user_B5002_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA1_175Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA1_175Id,
                        UserId = user_B5002_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA1: katrinl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5002_DoA1_6416Id) &&
                emailToUserId.TryGetValue("katrinl@unops.org", out var user_B5002_DoA1_6416Id))
            {
                var key_B5002_DoA1_6416 = $"{orgUnit_B5002Id}_{role_B5002_DoA1_6416Id}_{user_B5002_DoA1_6416Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA1_6416))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA1_6416Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA1_6416Id,
                        UserId = user_B5002_DoA1_6416Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA1_6416); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("katrinl@unops.org")) missingUsers.Add("katrinl@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5002_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5002_DoA3_2942Id))
            {
                var key_B5002_DoA3_2942 = $"{orgUnit_B5002Id}_{role_B5002_DoA3_2942Id}_{user_B5002_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5002_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_DoA3_2942Id}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_DoA3_2942Id,
                        UserId = user_B5002_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5002_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5002");
        }

        // B5003
        if (codeToOrgUnitId.TryGetValue("B5003", out var orgUnit_B5003Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5003_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5003_DoA3_8767Id))
            {
                var key_B5003_DoA3_8767 = $"{orgUnit_B5003Id}_{role_B5003_DoA3_8767Id}_{user_B5003_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5003_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_DoA3_8767Id}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_DoA3_8767Id,
                        UserId = user_B5003_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5003_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA1: adas@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5003_DoA1_5061Id) &&
                emailToUserId.TryGetValue("adas@unops.org", out var user_B5003_DoA1_5061Id))
            {
                var key_B5003_DoA1_5061 = $"{orgUnit_B5003Id}_{role_B5003_DoA1_5061Id}_{user_B5003_DoA1_5061Id}";
                if (!existingRoleKeys.Contains(key_B5003_DoA1_5061))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_DoA1_5061Id}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_DoA1_5061Id,
                        UserId = user_B5003_DoA1_5061Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5003_DoA1_5061); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("adas@unops.org")) missingUsers.Add("adas@unops.org");
            }
            // DoA1: edrissr@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5003_DoA1_8018Id) &&
                emailToUserId.TryGetValue("edrissr@unops.org", out var user_B5003_DoA1_8018Id))
            {
                var key_B5003_DoA1_8018 = $"{orgUnit_B5003Id}_{role_B5003_DoA1_8018Id}_{user_B5003_DoA1_8018Id}";
                if (!existingRoleKeys.Contains(key_B5003_DoA1_8018))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_DoA1_8018Id}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_DoA1_8018Id,
                        UserId = user_B5003_DoA1_8018Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5003_DoA1_8018); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("edrissr@unops.org")) missingUsers.Add("edrissr@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5003_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5003_DoA3_2942Id))
            {
                var key_B5003_DoA3_2942 = $"{orgUnit_B5003Id}_{role_B5003_DoA3_2942Id}_{user_B5003_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5003_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_DoA3_2942Id}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_DoA3_2942Id,
                        UserId = user_B5003_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5003_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5003");
        }

        // B5004
        if (codeToOrgUnitId.TryGetValue("B5004", out var orgUnit_B5004Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5004_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5004_DoA3_8767Id))
            {
                var key_B5004_DoA3_8767 = $"{orgUnit_B5004Id}_{role_B5004_DoA3_8767Id}_{user_B5004_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5004_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_DoA3_8767Id}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_DoA3_8767Id,
                        UserId = user_B5004_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5004_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA1: isabellav@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5004_DoA1_4215Id) &&
                emailToUserId.TryGetValue("isabellav@unops.org", out var user_B5004_DoA1_4215Id))
            {
                var key_B5004_DoA1_4215 = $"{orgUnit_B5004Id}_{role_B5004_DoA1_4215Id}_{user_B5004_DoA1_4215Id}";
                if (!existingRoleKeys.Contains(key_B5004_DoA1_4215))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_DoA1_4215Id}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_DoA1_4215Id,
                        UserId = user_B5004_DoA1_4215Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5004_DoA1_4215); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("isabellav@unops.org")) missingUsers.Add("isabellav@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5004_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5004_DoA3_2942Id))
            {
                var key_B5004_DoA3_2942 = $"{orgUnit_B5004Id}_{role_B5004_DoA3_2942Id}_{user_B5004_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5004_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_DoA3_2942Id}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_DoA3_2942Id,
                        UserId = user_B5004_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5004_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5004");
        }

        // B5006
        if (codeToOrgUnitId.TryGetValue("B5006", out var orgUnit_B5006Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5006_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5006_DoA1_7955Id))
            {
                var key_B5006_DoA1_7955 = $"{orgUnit_B5006Id}_{role_B5006_DoA1_7955Id}_{user_B5006_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5006_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_DoA1_7955Id}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_DoA1_7955Id,
                        UserId = user_B5006_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5006_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5006_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5006_DoA1_5331Id))
            {
                var key_B5006_DoA1_5331 = $"{orgUnit_B5006Id}_{role_B5006_DoA1_5331Id}_{user_B5006_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5006_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_DoA1_5331Id}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_DoA1_5331Id,
                        UserId = user_B5006_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5006_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5006_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5006_DoA2_1626Id))
            {
                var key_B5006_DoA2_1626 = $"{orgUnit_B5006Id}_{role_B5006_DoA2_1626Id}_{user_B5006_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5006_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_DoA2_1626Id}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_DoA2_1626Id,
                        UserId = user_B5006_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5006_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5006_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5006_DoA3_3918Id))
            {
                var key_B5006_DoA3_3918 = $"{orgUnit_B5006Id}_{role_B5006_DoA3_3918Id}_{user_B5006_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5006_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_DoA3_3918Id}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_DoA3_3918Id,
                        UserId = user_B5006_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5006_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5006");
        }

        // B5007
        if (codeToOrgUnitId.TryGetValue("B5007", out var orgUnit_B5007Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5007_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5007_DoA2_6407Id))
            {
                var key_B5007_DoA2_6407 = $"{orgUnit_B5007Id}_{role_B5007_DoA2_6407Id}_{user_B5007_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5007_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_DoA2_6407Id}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_DoA2_6407Id,
                        UserId = user_B5007_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5007_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5007_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5007_DoA2_729Id))
            {
                var key_B5007_DoA2_729 = $"{orgUnit_B5007Id}_{role_B5007_DoA2_729Id}_{user_B5007_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5007_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_DoA2_729Id}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_DoA2_729Id,
                        UserId = user_B5007_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5007_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5007_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5007_DoA1_9154Id))
            {
                var key_B5007_DoA1_9154 = $"{orgUnit_B5007Id}_{role_B5007_DoA1_9154Id}_{user_B5007_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5007_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_DoA1_9154Id}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_DoA1_9154Id,
                        UserId = user_B5007_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5007_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5007_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5007_DoA3_8767Id))
            {
                var key_B5007_DoA3_8767 = $"{orgUnit_B5007Id}_{role_B5007_DoA3_8767Id}_{user_B5007_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5007_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_DoA3_8767Id}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_DoA3_8767Id,
                        UserId = user_B5007_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5007_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5007_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5007_DoA3_2942Id))
            {
                var key_B5007_DoA3_2942 = $"{orgUnit_B5007Id}_{role_B5007_DoA3_2942Id}_{user_B5007_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5007_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_DoA3_2942Id}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_DoA3_2942Id,
                        UserId = user_B5007_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5007_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5007");
        }

        // B5009
        if (codeToOrgUnitId.TryGetValue("B5009", out var orgUnit_B5009Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5009_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5009_DoA3_8767Id))
            {
                var key_B5009_DoA3_8767 = $"{orgUnit_B5009Id}_{role_B5009_DoA3_8767Id}_{user_B5009_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5009_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5009Id} - {user_B5009_DoA3_8767Id}",
                        EntityId = orgUnit_B5009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5009_DoA3_8767Id,
                        UserId = user_B5009_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5009_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5009_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5009_DoA3_2942Id))
            {
                var key_B5009_DoA3_2942 = $"{orgUnit_B5009Id}_{role_B5009_DoA3_2942Id}_{user_B5009_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5009_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5009Id} - {user_B5009_DoA3_2942Id}",
                        EntityId = orgUnit_B5009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5009_DoA3_2942Id,
                        UserId = user_B5009_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5009_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5009");
        }

        // B5010
        if (codeToOrgUnitId.TryGetValue("B5010", out var orgUnit_B5010Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5010_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5010_DoA3_6407Id))
            {
                var key_B5010_DoA3_6407 = $"{orgUnit_B5010Id}_{role_B5010_DoA3_6407Id}_{user_B5010_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5010_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_DoA3_6407Id}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_DoA3_6407Id,
                        UserId = user_B5010_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5010_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5010_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5010_DoA2_6407Id))
            {
                var key_B5010_DoA2_6407 = $"{orgUnit_B5010Id}_{role_B5010_DoA2_6407Id}_{user_B5010_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5010_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_DoA2_6407Id}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_DoA2_6407Id,
                        UserId = user_B5010_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5010_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5010_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5010_DoA1_175Id))
            {
                var key_B5010_DoA1_175 = $"{orgUnit_B5010Id}_{role_B5010_DoA1_175Id}_{user_B5010_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5010_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_DoA1_175Id}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_DoA1_175Id,
                        UserId = user_B5010_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5010_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5010_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5010_DoA3_3632Id))
            {
                var key_B5010_DoA3_3632 = $"{orgUnit_B5010Id}_{role_B5010_DoA3_3632Id}_{user_B5010_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5010_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_DoA3_3632Id}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_DoA3_3632Id,
                        UserId = user_B5010_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5010_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: peteron@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5010_DoA1_6267Id) &&
                emailToUserId.TryGetValue("peteron@unops.org", out var user_B5010_DoA1_6267Id))
            {
                var key_B5010_DoA1_6267 = $"{orgUnit_B5010Id}_{role_B5010_DoA1_6267Id}_{user_B5010_DoA1_6267Id}";
                if (!existingRoleKeys.Contains(key_B5010_DoA1_6267))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_DoA1_6267Id}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_DoA1_6267Id,
                        UserId = user_B5010_DoA1_6267Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5010_DoA1_6267); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("peteron@unops.org")) missingUsers.Add("peteron@unops.org");
            }
            // DoA2: simonettas@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5010_DoA2_6238Id) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5010_DoA2_6238Id))
            {
                var key_B5010_DoA2_6238 = $"{orgUnit_B5010Id}_{role_B5010_DoA2_6238Id}_{user_B5010_DoA2_6238Id}";
                if (!existingRoleKeys.Contains(key_B5010_DoA2_6238))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_DoA2_6238Id}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_DoA2_6238Id,
                        UserId = user_B5010_DoA2_6238Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5010_DoA2_6238); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5010");
        }

        // B5011
        if (codeToOrgUnitId.TryGetValue("B5011", out var orgUnit_B5011Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5011_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5011_DoA3_8767Id))
            {
                var key_B5011_DoA3_8767 = $"{orgUnit_B5011Id}_{role_B5011_DoA3_8767Id}_{user_B5011_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5011_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_DoA3_8767Id}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_DoA3_8767Id,
                        UserId = user_B5011_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5011_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA1: nielsg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5011_DoA1_3414Id) &&
                emailToUserId.TryGetValue("nielsg@unops.org", out var user_B5011_DoA1_3414Id))
            {
                var key_B5011_DoA1_3414 = $"{orgUnit_B5011Id}_{role_B5011_DoA1_3414Id}_{user_B5011_DoA1_3414Id}";
                if (!existingRoleKeys.Contains(key_B5011_DoA1_3414))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_DoA1_3414Id}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_DoA1_3414Id,
                        UserId = user_B5011_DoA1_3414Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5011_DoA1_3414); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("nielsg@unops.org")) missingUsers.Add("nielsg@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5011_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5011_DoA3_2942Id))
            {
                var key_B5011_DoA3_2942 = $"{orgUnit_B5011Id}_{role_B5011_DoA3_2942Id}_{user_B5011_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5011_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_DoA3_2942Id}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_DoA3_2942Id,
                        UserId = user_B5011_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5011_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5011");
        }

        // B5012
        if (codeToOrgUnitId.TryGetValue("B5012", out var orgUnit_B5012Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5012_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5012_DoA3_8767Id))
            {
                var key_B5012_DoA3_8767 = $"{orgUnit_B5012Id}_{role_B5012_DoA3_8767Id}_{user_B5012_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5012_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_DoA3_8767Id}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_DoA3_8767Id,
                        UserId = user_B5012_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5012_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5012_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5012_DoA3_2942Id))
            {
                var key_B5012_DoA3_2942 = $"{orgUnit_B5012Id}_{role_B5012_DoA3_2942Id}_{user_B5012_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5012_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_DoA3_2942Id}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_DoA3_2942Id,
                        UserId = user_B5012_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5012_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5012");
        }

        // B5014
        if (codeToOrgUnitId.TryGetValue("B5014", out var orgUnit_B5014Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5014_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5014_DoA3_8767Id))
            {
                var key_B5014_DoA3_8767 = $"{orgUnit_B5014Id}_{role_B5014_DoA3_8767Id}_{user_B5014_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA3_8767Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA3_8767Id,
                        UserId = user_B5014_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5014_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5014_DoA2_402Id))
            {
                var key_B5014_DoA2_402 = $"{orgUnit_B5014Id}_{role_B5014_DoA2_402Id}_{user_B5014_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA2_402Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA2_402Id,
                        UserId = user_B5014_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5014_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5014_DoA2_4134Id))
            {
                var key_B5014_DoA2_4134 = $"{orgUnit_B5014Id}_{role_B5014_DoA2_4134Id}_{user_B5014_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA2_4134Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA2_4134Id,
                        UserId = user_B5014_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5014_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5014_DoA1_2171Id))
            {
                var key_B5014_DoA1_2171 = $"{orgUnit_B5014Id}_{role_B5014_DoA1_2171Id}_{user_B5014_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA1_2171Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA1_2171Id,
                        UserId = user_B5014_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5014_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5014_DoA1_307Id))
            {
                var key_B5014_DoA1_307 = $"{orgUnit_B5014Id}_{role_B5014_DoA1_307Id}_{user_B5014_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA1_307Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA1_307Id,
                        UserId = user_B5014_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5014_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5014_DoA2_2171Id))
            {
                var key_B5014_DoA2_2171 = $"{orgUnit_B5014Id}_{role_B5014_DoA2_2171Id}_{user_B5014_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA2_2171Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA2_2171Id,
                        UserId = user_B5014_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5014_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5014_DoA2_307Id))
            {
                var key_B5014_DoA2_307 = $"{orgUnit_B5014Id}_{role_B5014_DoA2_307Id}_{user_B5014_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA2_307Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA2_307Id,
                        UserId = user_B5014_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5014_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5014_DoA3_2942Id))
            {
                var key_B5014_DoA3_2942 = $"{orgUnit_B5014Id}_{role_B5014_DoA3_2942Id}_{user_B5014_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5014_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_DoA3_2942Id}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_DoA3_2942Id,
                        UserId = user_B5014_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5014_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5014");
        }

        // B5015
        if (codeToOrgUnitId.TryGetValue("B5015", out var orgUnit_B5015Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5015_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5015_DoA3_8767Id))
            {
                var key_B5015_DoA3_8767 = $"{orgUnit_B5015Id}_{role_B5015_DoA3_8767Id}_{user_B5015_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA3_8767Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA3_8767Id,
                        UserId = user_B5015_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5015_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5015_DoA2_402Id))
            {
                var key_B5015_DoA2_402 = $"{orgUnit_B5015Id}_{role_B5015_DoA2_402Id}_{user_B5015_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA2_402Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA2_402Id,
                        UserId = user_B5015_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5015_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5015_DoA2_4134Id))
            {
                var key_B5015_DoA2_4134 = $"{orgUnit_B5015Id}_{role_B5015_DoA2_4134Id}_{user_B5015_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA2_4134Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA2_4134Id,
                        UserId = user_B5015_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5015_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5015_DoA1_2171Id))
            {
                var key_B5015_DoA1_2171 = $"{orgUnit_B5015Id}_{role_B5015_DoA1_2171Id}_{user_B5015_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA1_2171Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA1_2171Id,
                        UserId = user_B5015_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5015_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5015_DoA1_307Id))
            {
                var key_B5015_DoA1_307 = $"{orgUnit_B5015Id}_{role_B5015_DoA1_307Id}_{user_B5015_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA1_307Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA1_307Id,
                        UserId = user_B5015_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5015_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5015_DoA2_2171Id))
            {
                var key_B5015_DoA2_2171 = $"{orgUnit_B5015Id}_{role_B5015_DoA2_2171Id}_{user_B5015_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA2_2171Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA2_2171Id,
                        UserId = user_B5015_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5015_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5015_DoA2_307Id))
            {
                var key_B5015_DoA2_307 = $"{orgUnit_B5015Id}_{role_B5015_DoA2_307Id}_{user_B5015_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA2_307Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA2_307Id,
                        UserId = user_B5015_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5015_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5015_DoA3_2942Id))
            {
                var key_B5015_DoA3_2942 = $"{orgUnit_B5015Id}_{role_B5015_DoA3_2942Id}_{user_B5015_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA3_2942Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA3_2942Id,
                        UserId = user_B5015_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // DoA1: petruss@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5015_DoA1_6427Id) &&
                emailToUserId.TryGetValue("petruss@unops.org", out var user_B5015_DoA1_6427Id))
            {
                var key_B5015_DoA1_6427 = $"{orgUnit_B5015Id}_{role_B5015_DoA1_6427Id}_{user_B5015_DoA1_6427Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA1_6427))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA1_6427Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA1_6427Id,
                        UserId = user_B5015_DoA1_6427Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA1_6427); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("petruss@unops.org")) missingUsers.Add("petruss@unops.org");
            }
            // DoA1: grahammi@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5015_DoA1_4330Id) &&
                emailToUserId.TryGetValue("grahammi@unops.org", out var user_B5015_DoA1_4330Id))
            {
                var key_B5015_DoA1_4330 = $"{orgUnit_B5015Id}_{role_B5015_DoA1_4330Id}_{user_B5015_DoA1_4330Id}";
                if (!existingRoleKeys.Contains(key_B5015_DoA1_4330))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_DoA1_4330Id}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_DoA1_4330Id,
                        UserId = user_B5015_DoA1_4330Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5015_DoA1_4330); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("grahammi@unops.org")) missingUsers.Add("grahammi@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5015");
        }

        // B5016
        if (codeToOrgUnitId.TryGetValue("B5016", out var orgUnit_B5016Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5016_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5016_DoA3_8767Id))
            {
                var key_B5016_DoA3_8767 = $"{orgUnit_B5016Id}_{role_B5016_DoA3_8767Id}_{user_B5016_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA3_8767Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA3_8767Id,
                        UserId = user_B5016_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5016_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5016_DoA2_402Id))
            {
                var key_B5016_DoA2_402 = $"{orgUnit_B5016Id}_{role_B5016_DoA2_402Id}_{user_B5016_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA2_402Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA2_402Id,
                        UserId = user_B5016_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5016_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5016_DoA2_4134Id))
            {
                var key_B5016_DoA2_4134 = $"{orgUnit_B5016Id}_{role_B5016_DoA2_4134Id}_{user_B5016_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA2_4134Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA2_4134Id,
                        UserId = user_B5016_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5016_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5016_DoA1_2171Id))
            {
                var key_B5016_DoA1_2171 = $"{orgUnit_B5016Id}_{role_B5016_DoA1_2171Id}_{user_B5016_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA1_2171Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA1_2171Id,
                        UserId = user_B5016_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5016_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5016_DoA1_307Id))
            {
                var key_B5016_DoA1_307 = $"{orgUnit_B5016Id}_{role_B5016_DoA1_307Id}_{user_B5016_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA1_307Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA1_307Id,
                        UserId = user_B5016_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5016_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5016_DoA2_2171Id))
            {
                var key_B5016_DoA2_2171 = $"{orgUnit_B5016Id}_{role_B5016_DoA2_2171Id}_{user_B5016_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA2_2171Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA2_2171Id,
                        UserId = user_B5016_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5016_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5016_DoA2_307Id))
            {
                var key_B5016_DoA2_307 = $"{orgUnit_B5016Id}_{role_B5016_DoA2_307Id}_{user_B5016_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA2_307Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA2_307Id,
                        UserId = user_B5016_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5016_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5016_DoA3_2942Id))
            {
                var key_B5016_DoA3_2942 = $"{orgUnit_B5016Id}_{role_B5016_DoA3_2942Id}_{user_B5016_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5016_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_DoA3_2942Id}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_DoA3_2942Id,
                        UserId = user_B5016_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5016_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5016");
        }

        // B5017
        if (codeToOrgUnitId.TryGetValue("B5017", out var orgUnit_B5017Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5017_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5017_DoA3_8767Id))
            {
                var key_B5017_DoA3_8767 = $"{orgUnit_B5017Id}_{role_B5017_DoA3_8767Id}_{user_B5017_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA3_8767Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA3_8767Id,
                        UserId = user_B5017_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5017_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5017_DoA2_402Id))
            {
                var key_B5017_DoA2_402 = $"{orgUnit_B5017Id}_{role_B5017_DoA2_402Id}_{user_B5017_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA2_402Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA2_402Id,
                        UserId = user_B5017_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5017_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5017_DoA2_4134Id))
            {
                var key_B5017_DoA2_4134 = $"{orgUnit_B5017Id}_{role_B5017_DoA2_4134Id}_{user_B5017_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA2_4134Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA2_4134Id,
                        UserId = user_B5017_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5017_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5017_DoA1_2171Id))
            {
                var key_B5017_DoA1_2171 = $"{orgUnit_B5017Id}_{role_B5017_DoA1_2171Id}_{user_B5017_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA1_2171Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA1_2171Id,
                        UserId = user_B5017_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5017_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5017_DoA1_307Id))
            {
                var key_B5017_DoA1_307 = $"{orgUnit_B5017Id}_{role_B5017_DoA1_307Id}_{user_B5017_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA1_307Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA1_307Id,
                        UserId = user_B5017_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5017_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5017_DoA2_2171Id))
            {
                var key_B5017_DoA2_2171 = $"{orgUnit_B5017Id}_{role_B5017_DoA2_2171Id}_{user_B5017_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA2_2171Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA2_2171Id,
                        UserId = user_B5017_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5017_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5017_DoA2_307Id))
            {
                var key_B5017_DoA2_307 = $"{orgUnit_B5017Id}_{role_B5017_DoA2_307Id}_{user_B5017_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA2_307Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA2_307Id,
                        UserId = user_B5017_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5017_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5017_DoA3_2942Id))
            {
                var key_B5017_DoA3_2942 = $"{orgUnit_B5017Id}_{role_B5017_DoA3_2942Id}_{user_B5017_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5017_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_DoA3_2942Id}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_DoA3_2942Id,
                        UserId = user_B5017_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5017_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5017");
        }

        // B5018
        if (codeToOrgUnitId.TryGetValue("B5018", out var orgUnit_B5018Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5018_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5018_DoA3_8767Id))
            {
                var key_B5018_DoA3_8767 = $"{orgUnit_B5018Id}_{role_B5018_DoA3_8767Id}_{user_B5018_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA3_8767Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA3_8767Id,
                        UserId = user_B5018_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5018_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5018_DoA2_402Id))
            {
                var key_B5018_DoA2_402 = $"{orgUnit_B5018Id}_{role_B5018_DoA2_402Id}_{user_B5018_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA2_402Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA2_402Id,
                        UserId = user_B5018_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5018_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5018_DoA2_4134Id))
            {
                var key_B5018_DoA2_4134 = $"{orgUnit_B5018Id}_{role_B5018_DoA2_4134Id}_{user_B5018_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA2_4134Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA2_4134Id,
                        UserId = user_B5018_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5018_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5018_DoA1_2171Id))
            {
                var key_B5018_DoA1_2171 = $"{orgUnit_B5018Id}_{role_B5018_DoA1_2171Id}_{user_B5018_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA1_2171Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA1_2171Id,
                        UserId = user_B5018_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5018_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5018_DoA1_307Id))
            {
                var key_B5018_DoA1_307 = $"{orgUnit_B5018Id}_{role_B5018_DoA1_307Id}_{user_B5018_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA1_307Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA1_307Id,
                        UserId = user_B5018_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5018_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5018_DoA2_2171Id))
            {
                var key_B5018_DoA2_2171 = $"{orgUnit_B5018Id}_{role_B5018_DoA2_2171Id}_{user_B5018_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA2_2171Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA2_2171Id,
                        UserId = user_B5018_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5018_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5018_DoA2_307Id))
            {
                var key_B5018_DoA2_307 = $"{orgUnit_B5018Id}_{role_B5018_DoA2_307Id}_{user_B5018_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA2_307Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA2_307Id,
                        UserId = user_B5018_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5018_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5018_DoA3_2942Id))
            {
                var key_B5018_DoA3_2942 = $"{orgUnit_B5018Id}_{role_B5018_DoA3_2942Id}_{user_B5018_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA3_2942Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA3_2942Id,
                        UserId = user_B5018_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // DoA1: borysp@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5018_DoA1_8190Id) &&
                emailToUserId.TryGetValue("borysp@unops.org", out var user_B5018_DoA1_8190Id))
            {
                var key_B5018_DoA1_8190 = $"{orgUnit_B5018Id}_{role_B5018_DoA1_8190Id}_{user_B5018_DoA1_8190Id}";
                if (!existingRoleKeys.Contains(key_B5018_DoA1_8190))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_DoA1_8190Id}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_DoA1_8190Id,
                        UserId = user_B5018_DoA1_8190Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5018_DoA1_8190); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("borysp@unops.org")) missingUsers.Add("borysp@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5018");
        }

        // B5019
        if (codeToOrgUnitId.TryGetValue("B5019", out var orgUnit_B5019Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5019_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5019_DoA3_8767Id))
            {
                var key_B5019_DoA3_8767 = $"{orgUnit_B5019Id}_{role_B5019_DoA3_8767Id}_{user_B5019_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA3_8767Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA3_8767Id,
                        UserId = user_B5019_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5019_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5019_DoA2_402Id))
            {
                var key_B5019_DoA2_402 = $"{orgUnit_B5019Id}_{role_B5019_DoA2_402Id}_{user_B5019_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA2_402Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA2_402Id,
                        UserId = user_B5019_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5019_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5019_DoA2_4134Id))
            {
                var key_B5019_DoA2_4134 = $"{orgUnit_B5019Id}_{role_B5019_DoA2_4134Id}_{user_B5019_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA2_4134Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA2_4134Id,
                        UserId = user_B5019_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5019_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5019_DoA1_2171Id))
            {
                var key_B5019_DoA1_2171 = $"{orgUnit_B5019Id}_{role_B5019_DoA1_2171Id}_{user_B5019_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA1_2171Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA1_2171Id,
                        UserId = user_B5019_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5019_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5019_DoA1_307Id))
            {
                var key_B5019_DoA1_307 = $"{orgUnit_B5019Id}_{role_B5019_DoA1_307Id}_{user_B5019_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA1_307Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA1_307Id,
                        UserId = user_B5019_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA1: gorant@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5019_DoA1_7143Id) &&
                emailToUserId.TryGetValue("gorant@unops.org", out var user_B5019_DoA1_7143Id))
            {
                var key_B5019_DoA1_7143 = $"{orgUnit_B5019Id}_{role_B5019_DoA1_7143Id}_{user_B5019_DoA1_7143Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA1_7143))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA1_7143Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA1_7143Id,
                        UserId = user_B5019_DoA1_7143Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA1_7143); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gorant@unops.org")) missingUsers.Add("gorant@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5019_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5019_DoA2_2171Id))
            {
                var key_B5019_DoA2_2171 = $"{orgUnit_B5019Id}_{role_B5019_DoA2_2171Id}_{user_B5019_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA2_2171Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA2_2171Id,
                        UserId = user_B5019_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5019_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5019_DoA2_307Id))
            {
                var key_B5019_DoA2_307 = $"{orgUnit_B5019Id}_{role_B5019_DoA2_307Id}_{user_B5019_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA2_307Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA2_307Id,
                        UserId = user_B5019_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5019_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5019_DoA3_2942Id))
            {
                var key_B5019_DoA3_2942 = $"{orgUnit_B5019Id}_{role_B5019_DoA3_2942Id}_{user_B5019_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5019_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_DoA3_2942Id}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_DoA3_2942Id,
                        UserId = user_B5019_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5019_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5019");
        }

        // B5020
        if (codeToOrgUnitId.TryGetValue("B5020", out var orgUnit_B5020Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5020_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5020_DoA3_8767Id))
            {
                var key_B5020_DoA3_8767 = $"{orgUnit_B5020Id}_{role_B5020_DoA3_8767Id}_{user_B5020_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA3_8767Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA3_8767Id,
                        UserId = user_B5020_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5020_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5020_DoA2_402Id))
            {
                var key_B5020_DoA2_402 = $"{orgUnit_B5020Id}_{role_B5020_DoA2_402Id}_{user_B5020_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA2_402Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA2_402Id,
                        UserId = user_B5020_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5020_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5020_DoA2_4134Id))
            {
                var key_B5020_DoA2_4134 = $"{orgUnit_B5020Id}_{role_B5020_DoA2_4134Id}_{user_B5020_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA2_4134Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA2_4134Id,
                        UserId = user_B5020_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5020_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5020_DoA1_2171Id))
            {
                var key_B5020_DoA1_2171 = $"{orgUnit_B5020Id}_{role_B5020_DoA1_2171Id}_{user_B5020_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA1_2171Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA1_2171Id,
                        UserId = user_B5020_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5020_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5020_DoA1_307Id))
            {
                var key_B5020_DoA1_307 = $"{orgUnit_B5020Id}_{role_B5020_DoA1_307Id}_{user_B5020_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA1_307Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA1_307Id,
                        UserId = user_B5020_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5020_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5020_DoA2_2171Id))
            {
                var key_B5020_DoA2_2171 = $"{orgUnit_B5020Id}_{role_B5020_DoA2_2171Id}_{user_B5020_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA2_2171Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA2_2171Id,
                        UserId = user_B5020_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5020_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5020_DoA2_307Id))
            {
                var key_B5020_DoA2_307 = $"{orgUnit_B5020Id}_{role_B5020_DoA2_307Id}_{user_B5020_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA2_307Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA2_307Id,
                        UserId = user_B5020_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5020_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5020_DoA3_2942Id))
            {
                var key_B5020_DoA3_2942 = $"{orgUnit_B5020Id}_{role_B5020_DoA3_2942Id}_{user_B5020_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5020_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_DoA3_2942Id}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_DoA3_2942Id,
                        UserId = user_B5020_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5020_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5020");
        }

        // B5021
        if (codeToOrgUnitId.TryGetValue("B5021", out var orgUnit_B5021Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5021_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5021_DoA3_8767Id))
            {
                var key_B5021_DoA3_8767 = $"{orgUnit_B5021Id}_{role_B5021_DoA3_8767Id}_{user_B5021_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA3_8767Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA3_8767Id,
                        UserId = user_B5021_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5021_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5021_DoA2_402Id))
            {
                var key_B5021_DoA2_402 = $"{orgUnit_B5021Id}_{role_B5021_DoA2_402Id}_{user_B5021_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA2_402Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA2_402Id,
                        UserId = user_B5021_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5021_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5021_DoA2_4134Id))
            {
                var key_B5021_DoA2_4134 = $"{orgUnit_B5021Id}_{role_B5021_DoA2_4134Id}_{user_B5021_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA2_4134Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA2_4134Id,
                        UserId = user_B5021_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5021_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5021_DoA1_2171Id))
            {
                var key_B5021_DoA1_2171 = $"{orgUnit_B5021Id}_{role_B5021_DoA1_2171Id}_{user_B5021_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA1_2171Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA1_2171Id,
                        UserId = user_B5021_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5021_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5021_DoA1_307Id))
            {
                var key_B5021_DoA1_307 = $"{orgUnit_B5021Id}_{role_B5021_DoA1_307Id}_{user_B5021_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA1_307Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA1_307Id,
                        UserId = user_B5021_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5021_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5021_DoA2_2171Id))
            {
                var key_B5021_DoA2_2171 = $"{orgUnit_B5021Id}_{role_B5021_DoA2_2171Id}_{user_B5021_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA2_2171Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA2_2171Id,
                        UserId = user_B5021_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5021_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5021_DoA2_307Id))
            {
                var key_B5021_DoA2_307 = $"{orgUnit_B5021Id}_{role_B5021_DoA2_307Id}_{user_B5021_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5021_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5021Id} - {user_B5021_DoA2_307Id}",
                        EntityId = orgUnit_B5021Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5021_DoA2_307Id,
                        UserId = user_B5021_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5021_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5021");
        }

        // B5022
        if (codeToOrgUnitId.TryGetValue("B5022", out var orgUnit_B5022Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5022_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5022_DoA3_8767Id))
            {
                var key_B5022_DoA3_8767 = $"{orgUnit_B5022Id}_{role_B5022_DoA3_8767Id}_{user_B5022_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA3_8767Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA3_8767Id,
                        UserId = user_B5022_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5022_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5022_DoA2_402Id))
            {
                var key_B5022_DoA2_402 = $"{orgUnit_B5022Id}_{role_B5022_DoA2_402Id}_{user_B5022_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA2_402Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA2_402Id,
                        UserId = user_B5022_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5022_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5022_DoA2_4134Id))
            {
                var key_B5022_DoA2_4134 = $"{orgUnit_B5022Id}_{role_B5022_DoA2_4134Id}_{user_B5022_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA2_4134Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA2_4134Id,
                        UserId = user_B5022_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5022_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5022_DoA1_2171Id))
            {
                var key_B5022_DoA1_2171 = $"{orgUnit_B5022Id}_{role_B5022_DoA1_2171Id}_{user_B5022_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA1_2171Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA1_2171Id,
                        UserId = user_B5022_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5022_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5022_DoA1_307Id))
            {
                var key_B5022_DoA1_307 = $"{orgUnit_B5022Id}_{role_B5022_DoA1_307Id}_{user_B5022_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA1_307Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA1_307Id,
                        UserId = user_B5022_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5022_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5022_DoA2_2171Id))
            {
                var key_B5022_DoA2_2171 = $"{orgUnit_B5022Id}_{role_B5022_DoA2_2171Id}_{user_B5022_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA2_2171Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA2_2171Id,
                        UserId = user_B5022_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5022_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5022_DoA2_307Id))
            {
                var key_B5022_DoA2_307 = $"{orgUnit_B5022Id}_{role_B5022_DoA2_307Id}_{user_B5022_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA2_307Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA2_307Id,
                        UserId = user_B5022_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5022_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5022_DoA3_2942Id))
            {
                var key_B5022_DoA3_2942 = $"{orgUnit_B5022Id}_{role_B5022_DoA3_2942Id}_{user_B5022_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5022_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_DoA3_2942Id}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_DoA3_2942Id,
                        UserId = user_B5022_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5022_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5022");
        }

        // B5023
        if (codeToOrgUnitId.TryGetValue("B5023", out var orgUnit_B5023Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5023_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5023_DoA3_8767Id))
            {
                var key_B5023_DoA3_8767 = $"{orgUnit_B5023Id}_{role_B5023_DoA3_8767Id}_{user_B5023_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA3_8767Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA3_8767Id,
                        UserId = user_B5023_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5023_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5023_DoA2_402Id))
            {
                var key_B5023_DoA2_402 = $"{orgUnit_B5023Id}_{role_B5023_DoA2_402Id}_{user_B5023_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA2_402Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA2_402Id,
                        UserId = user_B5023_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5023_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5023_DoA2_4134Id))
            {
                var key_B5023_DoA2_4134 = $"{orgUnit_B5023Id}_{role_B5023_DoA2_4134Id}_{user_B5023_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA2_4134Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA2_4134Id,
                        UserId = user_B5023_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5023_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5023_DoA1_2171Id))
            {
                var key_B5023_DoA1_2171 = $"{orgUnit_B5023Id}_{role_B5023_DoA1_2171Id}_{user_B5023_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA1_2171Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA1_2171Id,
                        UserId = user_B5023_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5023_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5023_DoA1_307Id))
            {
                var key_B5023_DoA1_307 = $"{orgUnit_B5023Id}_{role_B5023_DoA1_307Id}_{user_B5023_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA1_307Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA1_307Id,
                        UserId = user_B5023_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5023_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5023_DoA2_2171Id))
            {
                var key_B5023_DoA2_2171 = $"{orgUnit_B5023Id}_{role_B5023_DoA2_2171Id}_{user_B5023_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA2_2171Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA2_2171Id,
                        UserId = user_B5023_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5023_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5023_DoA2_307Id))
            {
                var key_B5023_DoA2_307 = $"{orgUnit_B5023Id}_{role_B5023_DoA2_307Id}_{user_B5023_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA2_307Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA2_307Id,
                        UserId = user_B5023_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA1: lillianf@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5023_DoA1_6265Id) &&
                emailToUserId.TryGetValue("lillianf@unops.org", out var user_B5023_DoA1_6265Id))
            {
                var key_B5023_DoA1_6265 = $"{orgUnit_B5023Id}_{role_B5023_DoA1_6265Id}_{user_B5023_DoA1_6265Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA1_6265))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA1_6265Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA1_6265Id,
                        UserId = user_B5023_DoA1_6265Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA1_6265); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("lillianf@unops.org")) missingUsers.Add("lillianf@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5023_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5023_DoA3_2942Id))
            {
                var key_B5023_DoA3_2942 = $"{orgUnit_B5023Id}_{role_B5023_DoA3_2942Id}_{user_B5023_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5023_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_DoA3_2942Id}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_DoA3_2942Id,
                        UserId = user_B5023_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5023_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5023");
        }

        // B5024
        if (codeToOrgUnitId.TryGetValue("B5024", out var orgUnit_B5024Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5024_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5024_DoA3_8767Id))
            {
                var key_B5024_DoA3_8767 = $"{orgUnit_B5024Id}_{role_B5024_DoA3_8767Id}_{user_B5024_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA3_8767Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA3_8767Id,
                        UserId = user_B5024_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5024_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5024_DoA2_402Id))
            {
                var key_B5024_DoA2_402 = $"{orgUnit_B5024Id}_{role_B5024_DoA2_402Id}_{user_B5024_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA2_402Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA2_402Id,
                        UserId = user_B5024_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5024_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5024_DoA2_4134Id))
            {
                var key_B5024_DoA2_4134 = $"{orgUnit_B5024Id}_{role_B5024_DoA2_4134Id}_{user_B5024_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA2_4134Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA2_4134Id,
                        UserId = user_B5024_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5024_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5024_DoA1_2171Id))
            {
                var key_B5024_DoA1_2171 = $"{orgUnit_B5024Id}_{role_B5024_DoA1_2171Id}_{user_B5024_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA1_2171Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA1_2171Id,
                        UserId = user_B5024_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5024_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5024_DoA1_307Id))
            {
                var key_B5024_DoA1_307 = $"{orgUnit_B5024Id}_{role_B5024_DoA1_307Id}_{user_B5024_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA1_307Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA1_307Id,
                        UserId = user_B5024_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5024_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5024_DoA2_2171Id))
            {
                var key_B5024_DoA2_2171 = $"{orgUnit_B5024Id}_{role_B5024_DoA2_2171Id}_{user_B5024_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA2_2171Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA2_2171Id,
                        UserId = user_B5024_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5024_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5024_DoA2_307Id))
            {
                var key_B5024_DoA2_307 = $"{orgUnit_B5024Id}_{role_B5024_DoA2_307Id}_{user_B5024_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA2_307Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA2_307Id,
                        UserId = user_B5024_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5024_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5024_DoA3_2942Id))
            {
                var key_B5024_DoA3_2942 = $"{orgUnit_B5024Id}_{role_B5024_DoA3_2942Id}_{user_B5024_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA3_2942Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA3_2942Id,
                        UserId = user_B5024_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // DoA1: artyomh@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5024_DoA1_1405Id) &&
                emailToUserId.TryGetValue("artyomh@unops.org", out var user_B5024_DoA1_1405Id))
            {
                var key_B5024_DoA1_1405 = $"{orgUnit_B5024Id}_{role_B5024_DoA1_1405Id}_{user_B5024_DoA1_1405Id}";
                if (!existingRoleKeys.Contains(key_B5024_DoA1_1405))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_DoA1_1405Id}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_DoA1_1405Id,
                        UserId = user_B5024_DoA1_1405Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5024_DoA1_1405); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("artyomh@unops.org")) missingUsers.Add("artyomh@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5024");
        }

        // B5025
        if (codeToOrgUnitId.TryGetValue("B5025", out var orgUnit_B5025Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5025_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5025_DoA3_8767Id))
            {
                var key_B5025_DoA3_8767 = $"{orgUnit_B5025Id}_{role_B5025_DoA3_8767Id}_{user_B5025_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA3_8767Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA3_8767Id,
                        UserId = user_B5025_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5025_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5025_DoA2_402Id))
            {
                var key_B5025_DoA2_402 = $"{orgUnit_B5025Id}_{role_B5025_DoA2_402Id}_{user_B5025_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA2_402Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA2_402Id,
                        UserId = user_B5025_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5025_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5025_DoA2_4134Id))
            {
                var key_B5025_DoA2_4134 = $"{orgUnit_B5025Id}_{role_B5025_DoA2_4134Id}_{user_B5025_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA2_4134Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA2_4134Id,
                        UserId = user_B5025_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5025_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5025_DoA1_2171Id))
            {
                var key_B5025_DoA1_2171 = $"{orgUnit_B5025Id}_{role_B5025_DoA1_2171Id}_{user_B5025_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA1_2171Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA1_2171Id,
                        UserId = user_B5025_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5025_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5025_DoA1_307Id))
            {
                var key_B5025_DoA1_307 = $"{orgUnit_B5025Id}_{role_B5025_DoA1_307Id}_{user_B5025_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA1_307Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA1_307Id,
                        UserId = user_B5025_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5025_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5025_DoA2_2171Id))
            {
                var key_B5025_DoA2_2171 = $"{orgUnit_B5025Id}_{role_B5025_DoA2_2171Id}_{user_B5025_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA2_2171Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA2_2171Id,
                        UserId = user_B5025_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5025_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5025_DoA2_307Id))
            {
                var key_B5025_DoA2_307 = $"{orgUnit_B5025Id}_{role_B5025_DoA2_307Id}_{user_B5025_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA2_307Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA2_307Id,
                        UserId = user_B5025_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5025_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5025_DoA3_2942Id))
            {
                var key_B5025_DoA3_2942 = $"{orgUnit_B5025Id}_{role_B5025_DoA3_2942Id}_{user_B5025_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA3_2942Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA3_2942Id,
                        UserId = user_B5025_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // DoA1: brunob@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5025_DoA1_5139Id) &&
                emailToUserId.TryGetValue("brunob@unops.org", out var user_B5025_DoA1_5139Id))
            {
                var key_B5025_DoA1_5139 = $"{orgUnit_B5025Id}_{role_B5025_DoA1_5139Id}_{user_B5025_DoA1_5139Id}";
                if (!existingRoleKeys.Contains(key_B5025_DoA1_5139))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_DoA1_5139Id}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_DoA1_5139Id,
                        UserId = user_B5025_DoA1_5139Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5025_DoA1_5139); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("brunob@unops.org")) missingUsers.Add("brunob@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5025");
        }

        // B5026
        if (codeToOrgUnitId.TryGetValue("B5026", out var orgUnit_B5026Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5026_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5026_DoA3_8767Id))
            {
                var key_B5026_DoA3_8767 = $"{orgUnit_B5026Id}_{role_B5026_DoA3_8767Id}_{user_B5026_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA3_8767Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA3_8767Id,
                        UserId = user_B5026_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5026_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5026_DoA2_402Id))
            {
                var key_B5026_DoA2_402 = $"{orgUnit_B5026Id}_{role_B5026_DoA2_402Id}_{user_B5026_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA2_402Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA2_402Id,
                        UserId = user_B5026_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5026_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5026_DoA2_4134Id))
            {
                var key_B5026_DoA2_4134 = $"{orgUnit_B5026Id}_{role_B5026_DoA2_4134Id}_{user_B5026_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA2_4134Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA2_4134Id,
                        UserId = user_B5026_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5026_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5026_DoA1_2171Id))
            {
                var key_B5026_DoA1_2171 = $"{orgUnit_B5026Id}_{role_B5026_DoA1_2171Id}_{user_B5026_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA1_2171Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA1_2171Id,
                        UserId = user_B5026_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5026_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5026_DoA1_307Id))
            {
                var key_B5026_DoA1_307 = $"{orgUnit_B5026Id}_{role_B5026_DoA1_307Id}_{user_B5026_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA1_307Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA1_307Id,
                        UserId = user_B5026_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5026_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5026_DoA2_2171Id))
            {
                var key_B5026_DoA2_2171 = $"{orgUnit_B5026Id}_{role_B5026_DoA2_2171Id}_{user_B5026_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA2_2171Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA2_2171Id,
                        UserId = user_B5026_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5026_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5026_DoA2_307Id))
            {
                var key_B5026_DoA2_307 = $"{orgUnit_B5026Id}_{role_B5026_DoA2_307Id}_{user_B5026_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA2_307Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA2_307Id,
                        UserId = user_B5026_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5026_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5026_DoA3_2942Id))
            {
                var key_B5026_DoA3_2942 = $"{orgUnit_B5026Id}_{role_B5026_DoA3_2942Id}_{user_B5026_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5026_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_DoA3_2942Id}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_DoA3_2942Id,
                        UserId = user_B5026_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5026_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5026");
        }

        // B5027
        if (codeToOrgUnitId.TryGetValue("B5027", out var orgUnit_B5027Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5027_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5027_DoA3_8767Id))
            {
                var key_B5027_DoA3_8767 = $"{orgUnit_B5027Id}_{role_B5027_DoA3_8767Id}_{user_B5027_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA3_8767Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA3_8767Id,
                        UserId = user_B5027_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5027_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5027_DoA2_402Id))
            {
                var key_B5027_DoA2_402 = $"{orgUnit_B5027Id}_{role_B5027_DoA2_402Id}_{user_B5027_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA2_402Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA2_402Id,
                        UserId = user_B5027_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5027_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5027_DoA2_4134Id))
            {
                var key_B5027_DoA2_4134 = $"{orgUnit_B5027Id}_{role_B5027_DoA2_4134Id}_{user_B5027_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA2_4134Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA2_4134Id,
                        UserId = user_B5027_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5027_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5027_DoA1_2171Id))
            {
                var key_B5027_DoA1_2171 = $"{orgUnit_B5027Id}_{role_B5027_DoA1_2171Id}_{user_B5027_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA1_2171Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA1_2171Id,
                        UserId = user_B5027_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5027_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5027_DoA1_307Id))
            {
                var key_B5027_DoA1_307 = $"{orgUnit_B5027Id}_{role_B5027_DoA1_307Id}_{user_B5027_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA1_307Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA1_307Id,
                        UserId = user_B5027_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5027_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5027_DoA2_2171Id))
            {
                var key_B5027_DoA2_2171 = $"{orgUnit_B5027Id}_{role_B5027_DoA2_2171Id}_{user_B5027_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA2_2171Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA2_2171Id,
                        UserId = user_B5027_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5027_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5027_DoA2_307Id))
            {
                var key_B5027_DoA2_307 = $"{orgUnit_B5027Id}_{role_B5027_DoA2_307Id}_{user_B5027_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA2_307Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA2_307Id,
                        UserId = user_B5027_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5027_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5027_DoA3_2942Id))
            {
                var key_B5027_DoA3_2942 = $"{orgUnit_B5027Id}_{role_B5027_DoA3_2942Id}_{user_B5027_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA3_2942Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA3_2942Id,
                        UserId = user_B5027_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // DoA1: dorothyd@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5027_DoA1_4047Id) &&
                emailToUserId.TryGetValue("dorothyd@unops.org", out var user_B5027_DoA1_4047Id))
            {
                var key_B5027_DoA1_4047 = $"{orgUnit_B5027Id}_{role_B5027_DoA1_4047Id}_{user_B5027_DoA1_4047Id}";
                if (!existingRoleKeys.Contains(key_B5027_DoA1_4047))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_DoA1_4047Id}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_DoA1_4047Id,
                        UserId = user_B5027_DoA1_4047Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5027_DoA1_4047); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("dorothyd@unops.org")) missingUsers.Add("dorothyd@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5027");
        }

        // B5028
        if (codeToOrgUnitId.TryGetValue("B5028", out var orgUnit_B5028Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5028_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5028_DoA3_8767Id))
            {
                var key_B5028_DoA3_8767 = $"{orgUnit_B5028Id}_{role_B5028_DoA3_8767Id}_{user_B5028_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA3_8767Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA3_8767Id,
                        UserId = user_B5028_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5028_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5028_DoA2_402Id))
            {
                var key_B5028_DoA2_402 = $"{orgUnit_B5028Id}_{role_B5028_DoA2_402Id}_{user_B5028_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA2_402Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA2_402Id,
                        UserId = user_B5028_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5028_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5028_DoA2_4134Id))
            {
                var key_B5028_DoA2_4134 = $"{orgUnit_B5028Id}_{role_B5028_DoA2_4134Id}_{user_B5028_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA2_4134Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA2_4134Id,
                        UserId = user_B5028_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5028_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5028_DoA1_2171Id))
            {
                var key_B5028_DoA1_2171 = $"{orgUnit_B5028Id}_{role_B5028_DoA1_2171Id}_{user_B5028_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA1_2171Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA1_2171Id,
                        UserId = user_B5028_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5028_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5028_DoA1_307Id))
            {
                var key_B5028_DoA1_307 = $"{orgUnit_B5028Id}_{role_B5028_DoA1_307Id}_{user_B5028_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA1_307Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA1_307Id,
                        UserId = user_B5028_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5028_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5028_DoA2_2171Id))
            {
                var key_B5028_DoA2_2171 = $"{orgUnit_B5028Id}_{role_B5028_DoA2_2171Id}_{user_B5028_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA2_2171Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA2_2171Id,
                        UserId = user_B5028_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5028_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5028_DoA2_307Id))
            {
                var key_B5028_DoA2_307 = $"{orgUnit_B5028Id}_{role_B5028_DoA2_307Id}_{user_B5028_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA2_307Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA2_307Id,
                        UserId = user_B5028_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5028_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5028_DoA3_2942Id))
            {
                var key_B5028_DoA3_2942 = $"{orgUnit_B5028Id}_{role_B5028_DoA3_2942Id}_{user_B5028_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5028_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_DoA3_2942Id}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_DoA3_2942Id,
                        UserId = user_B5028_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5028_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5028");
        }

        // B5029
        if (codeToOrgUnitId.TryGetValue("B5029", out var orgUnit_B5029Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5029_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5029_DoA3_8767Id))
            {
                var key_B5029_DoA3_8767 = $"{orgUnit_B5029Id}_{role_B5029_DoA3_8767Id}_{user_B5029_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA3_8767Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA3_8767Id,
                        UserId = user_B5029_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5029_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5029_DoA2_402Id))
            {
                var key_B5029_DoA2_402 = $"{orgUnit_B5029Id}_{role_B5029_DoA2_402Id}_{user_B5029_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA2_402Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA2_402Id,
                        UserId = user_B5029_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5029_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5029_DoA2_4134Id))
            {
                var key_B5029_DoA2_4134 = $"{orgUnit_B5029Id}_{role_B5029_DoA2_4134Id}_{user_B5029_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA2_4134Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA2_4134Id,
                        UserId = user_B5029_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5029_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5029_DoA1_2171Id))
            {
                var key_B5029_DoA1_2171 = $"{orgUnit_B5029Id}_{role_B5029_DoA1_2171Id}_{user_B5029_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA1_2171Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA1_2171Id,
                        UserId = user_B5029_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5029_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5029_DoA1_307Id))
            {
                var key_B5029_DoA1_307 = $"{orgUnit_B5029Id}_{role_B5029_DoA1_307Id}_{user_B5029_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA1_307Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA1_307Id,
                        UserId = user_B5029_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5029_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5029_DoA2_2171Id))
            {
                var key_B5029_DoA2_2171 = $"{orgUnit_B5029Id}_{role_B5029_DoA2_2171Id}_{user_B5029_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA2_2171Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA2_2171Id,
                        UserId = user_B5029_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5029_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5029_DoA2_307Id))
            {
                var key_B5029_DoA2_307 = $"{orgUnit_B5029Id}_{role_B5029_DoA2_307Id}_{user_B5029_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA2_307Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA2_307Id,
                        UserId = user_B5029_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA1: catalinavm@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5029_DoA1_6842Id) &&
                emailToUserId.TryGetValue("catalinavm@unops.org", out var user_B5029_DoA1_6842Id))
            {
                var key_B5029_DoA1_6842 = $"{orgUnit_B5029Id}_{role_B5029_DoA1_6842Id}_{user_B5029_DoA1_6842Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA1_6842))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA1_6842Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA1_6842Id,
                        UserId = user_B5029_DoA1_6842Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA1_6842); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("catalinavm@unops.org")) missingUsers.Add("catalinavm@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5029_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5029_DoA3_2942Id))
            {
                var key_B5029_DoA3_2942 = $"{orgUnit_B5029Id}_{role_B5029_DoA3_2942Id}_{user_B5029_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5029_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_DoA3_2942Id}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_DoA3_2942Id,
                        UserId = user_B5029_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5029_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5029");
        }

        // B5030
        if (codeToOrgUnitId.TryGetValue("B5030", out var orgUnit_B5030Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5030_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5030_DoA3_8767Id))
            {
                var key_B5030_DoA3_8767 = $"{orgUnit_B5030Id}_{role_B5030_DoA3_8767Id}_{user_B5030_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA3_8767Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA3_8767Id,
                        UserId = user_B5030_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5030_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5030_DoA2_402Id))
            {
                var key_B5030_DoA2_402 = $"{orgUnit_B5030Id}_{role_B5030_DoA2_402Id}_{user_B5030_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA2_402Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA2_402Id,
                        UserId = user_B5030_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5030_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5030_DoA2_4134Id))
            {
                var key_B5030_DoA2_4134 = $"{orgUnit_B5030Id}_{role_B5030_DoA2_4134Id}_{user_B5030_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA2_4134Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA2_4134Id,
                        UserId = user_B5030_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5030_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5030_DoA1_2171Id))
            {
                var key_B5030_DoA1_2171 = $"{orgUnit_B5030Id}_{role_B5030_DoA1_2171Id}_{user_B5030_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA1_2171Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA1_2171Id,
                        UserId = user_B5030_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5030_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5030_DoA1_307Id))
            {
                var key_B5030_DoA1_307 = $"{orgUnit_B5030Id}_{role_B5030_DoA1_307Id}_{user_B5030_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA1_307Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA1_307Id,
                        UserId = user_B5030_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5030_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5030_DoA2_2171Id))
            {
                var key_B5030_DoA2_2171 = $"{orgUnit_B5030Id}_{role_B5030_DoA2_2171Id}_{user_B5030_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA2_2171Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA2_2171Id,
                        UserId = user_B5030_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5030_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5030_DoA2_307Id))
            {
                var key_B5030_DoA2_307 = $"{orgUnit_B5030Id}_{role_B5030_DoA2_307Id}_{user_B5030_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5030_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5030Id} - {user_B5030_DoA2_307Id}",
                        EntityId = orgUnit_B5030Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5030_DoA2_307Id,
                        UserId = user_B5030_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5030_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5030");
        }

        // B5031
        if (codeToOrgUnitId.TryGetValue("B5031", out var orgUnit_B5031Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5031_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5031_DoA3_8767Id))
            {
                var key_B5031_DoA3_8767 = $"{orgUnit_B5031Id}_{role_B5031_DoA3_8767Id}_{user_B5031_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA3_8767Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA3_8767Id,
                        UserId = user_B5031_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5031_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5031_DoA2_402Id))
            {
                var key_B5031_DoA2_402 = $"{orgUnit_B5031Id}_{role_B5031_DoA2_402Id}_{user_B5031_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA2_402Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA2_402Id,
                        UserId = user_B5031_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5031_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5031_DoA2_4134Id))
            {
                var key_B5031_DoA2_4134 = $"{orgUnit_B5031Id}_{role_B5031_DoA2_4134Id}_{user_B5031_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA2_4134Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA2_4134Id,
                        UserId = user_B5031_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5031_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5031_DoA1_2171Id))
            {
                var key_B5031_DoA1_2171 = $"{orgUnit_B5031Id}_{role_B5031_DoA1_2171Id}_{user_B5031_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA1_2171Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA1_2171Id,
                        UserId = user_B5031_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5031_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5031_DoA1_307Id))
            {
                var key_B5031_DoA1_307 = $"{orgUnit_B5031Id}_{role_B5031_DoA1_307Id}_{user_B5031_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA1_307Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA1_307Id,
                        UserId = user_B5031_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5031_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5031_DoA2_2171Id))
            {
                var key_B5031_DoA2_2171 = $"{orgUnit_B5031Id}_{role_B5031_DoA2_2171Id}_{user_B5031_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA2_2171Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA2_2171Id,
                        UserId = user_B5031_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5031_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5031_DoA2_307Id))
            {
                var key_B5031_DoA2_307 = $"{orgUnit_B5031Id}_{role_B5031_DoA2_307Id}_{user_B5031_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA2_307Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA2_307Id,
                        UserId = user_B5031_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5031_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5031_DoA3_2942Id))
            {
                var key_B5031_DoA3_2942 = $"{orgUnit_B5031Id}_{role_B5031_DoA3_2942Id}_{user_B5031_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5031_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_DoA3_2942Id}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_DoA3_2942Id,
                        UserId = user_B5031_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5031_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5031");
        }

        // B5032
        if (codeToOrgUnitId.TryGetValue("B5032", out var orgUnit_B5032Id))
        {
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5032_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5032_DoA3_8767Id))
            {
                var key_B5032_DoA3_8767 = $"{orgUnit_B5032Id}_{role_B5032_DoA3_8767Id}_{user_B5032_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA3_8767Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA3_8767Id,
                        UserId = user_B5032_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5032_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5032_DoA2_402Id))
            {
                var key_B5032_DoA2_402 = $"{orgUnit_B5032Id}_{role_B5032_DoA2_402Id}_{user_B5032_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA2_402Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA2_402Id,
                        UserId = user_B5032_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA2: amiro@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5032_DoA2_4134Id) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5032_DoA2_4134Id))
            {
                var key_B5032_DoA2_4134 = $"{orgUnit_B5032Id}_{role_B5032_DoA2_4134Id}_{user_B5032_DoA2_4134Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA2_4134))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA2_4134Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA2_4134Id,
                        UserId = user_B5032_DoA2_4134Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA2_4134); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5032_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5032_DoA1_2171Id))
            {
                var key_B5032_DoA1_2171 = $"{orgUnit_B5032Id}_{role_B5032_DoA1_2171Id}_{user_B5032_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA1_2171Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA1_2171Id,
                        UserId = user_B5032_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5032_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5032_DoA1_307Id))
            {
                var key_B5032_DoA1_307 = $"{orgUnit_B5032Id}_{role_B5032_DoA1_307Id}_{user_B5032_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA1_307Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA1_307Id,
                        UserId = user_B5032_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5032_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5032_DoA2_2171Id))
            {
                var key_B5032_DoA2_2171 = $"{orgUnit_B5032Id}_{role_B5032_DoA2_2171Id}_{user_B5032_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA2_2171Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA2_2171Id,
                        UserId = user_B5032_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5032_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5032_DoA2_307Id))
            {
                var key_B5032_DoA2_307 = $"{orgUnit_B5032Id}_{role_B5032_DoA2_307Id}_{user_B5032_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA2_307Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA2_307Id,
                        UserId = user_B5032_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5032_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5032_DoA3_2942Id))
            {
                var key_B5032_DoA3_2942 = $"{orgUnit_B5032Id}_{role_B5032_DoA3_2942Id}_{user_B5032_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5032_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_DoA3_2942Id}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_DoA3_2942Id,
                        UserId = user_B5032_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5032_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5032");
        }

        // B5033
        if (codeToOrgUnitId.TryGetValue("B5033", out var orgUnit_B5033Id))
        {
            // DoA2: salmanh@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5033_DoA2_402Id) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5033_DoA2_402Id))
            {
                var key_B5033_DoA2_402 = $"{orgUnit_B5033Id}_{role_B5033_DoA2_402Id}_{user_B5033_DoA2_402Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA2_402))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA2_402Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA2_402Id,
                        UserId = user_B5033_DoA2_402Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA2_402); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // DoA3: robertgodin@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5033_DoA3_8767Id) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5033_DoA3_8767Id))
            {
                var key_B5033_DoA3_8767 = $"{orgUnit_B5033Id}_{role_B5033_DoA3_8767Id}_{user_B5033_DoA3_8767Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA3_8767))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA3_8767Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA3_8767Id,
                        UserId = user_B5033_DoA3_8767Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA3_8767); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5033_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5033_DoA1_2171Id))
            {
                var key_B5033_DoA1_2171 = $"{orgUnit_B5033Id}_{role_B5033_DoA1_2171Id}_{user_B5033_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA1_2171Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA1_2171Id,
                        UserId = user_B5033_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5033_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5033_DoA1_307Id))
            {
                var key_B5033_DoA1_307 = $"{orgUnit_B5033Id}_{role_B5033_DoA1_307Id}_{user_B5033_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA1_307Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA1_307Id,
                        UserId = user_B5033_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA1: teresabt@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5033_DoA1_7876Id) &&
                emailToUserId.TryGetValue("teresabt@unops.org", out var user_B5033_DoA1_7876Id))
            {
                var key_B5033_DoA1_7876 = $"{orgUnit_B5033Id}_{role_B5033_DoA1_7876Id}_{user_B5033_DoA1_7876Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA1_7876))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA1_7876Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA1_7876Id,
                        UserId = user_B5033_DoA1_7876Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA1_7876); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("teresabt@unops.org")) missingUsers.Add("teresabt@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5033_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5033_DoA2_2171Id))
            {
                var key_B5033_DoA2_2171 = $"{orgUnit_B5033Id}_{role_B5033_DoA2_2171Id}_{user_B5033_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA2_2171Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA2_2171Id,
                        UserId = user_B5033_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5033_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5033_DoA2_307Id))
            {
                var key_B5033_DoA2_307 = $"{orgUnit_B5033Id}_{role_B5033_DoA2_307Id}_{user_B5033_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA2_307Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA2_307Id,
                        UserId = user_B5033_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5033_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5033_DoA3_2942Id))
            {
                var key_B5033_DoA3_2942 = $"{orgUnit_B5033Id}_{role_B5033_DoA3_2942Id}_{user_B5033_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5033_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_DoA3_2942Id}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_DoA3_2942Id,
                        UserId = user_B5033_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5033_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5033");
        }

        // B5034
        if (codeToOrgUnitId.TryGetValue("B5034", out var orgUnit_B5034Id))
        {
            // DoA1: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5034_DoA1_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5034_DoA1_2171Id))
            {
                var key_B5034_DoA1_2171 = $"{orgUnit_B5034Id}_{role_B5034_DoA1_2171Id}_{user_B5034_DoA1_2171Id}";
                if (!existingRoleKeys.Contains(key_B5034_DoA1_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_DoA1_2171Id}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_DoA1_2171Id,
                        UserId = user_B5034_DoA1_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5034_DoA1_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA1: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5034_DoA1_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5034_DoA1_307Id))
            {
                var key_B5034_DoA1_307 = $"{orgUnit_B5034Id}_{role_B5034_DoA1_307Id}_{user_B5034_DoA1_307Id}";
                if (!existingRoleKeys.Contains(key_B5034_DoA1_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_DoA1_307Id}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_DoA1_307Id,
                        UserId = user_B5034_DoA1_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5034_DoA1_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA2: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5034_DoA2_2171Id) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5034_DoA2_2171Id))
            {
                var key_B5034_DoA2_2171 = $"{orgUnit_B5034Id}_{role_B5034_DoA2_2171Id}_{user_B5034_DoA2_2171Id}";
                if (!existingRoleKeys.Contains(key_B5034_DoA2_2171))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_DoA2_2171Id}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_DoA2_2171Id,
                        UserId = user_B5034_DoA2_2171Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5034_DoA2_2171); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // DoA2: tammyb@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5034_DoA2_307Id) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5034_DoA2_307Id))
            {
                var key_B5034_DoA2_307 = $"{orgUnit_B5034Id}_{role_B5034_DoA2_307Id}_{user_B5034_DoA2_307Id}";
                if (!existingRoleKeys.Contains(key_B5034_DoA2_307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_DoA2_307Id}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_DoA2_307Id,
                        UserId = user_B5034_DoA2_307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5034_DoA2_307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // DoA3: emiliep@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5034_DoA3_2942Id) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5034_DoA3_2942Id))
            {
                var key_B5034_DoA3_2942 = $"{orgUnit_B5034Id}_{role_B5034_DoA3_2942Id}_{user_B5034_DoA3_2942Id}";
                if (!existingRoleKeys.Contains(key_B5034_DoA3_2942))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_DoA3_2942Id}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_DoA3_2942Id,
                        UserId = user_B5034_DoA3_2942Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5034_DoA3_2942); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5034");
        }

        // B5101
        if (codeToOrgUnitId.TryGetValue("B5101", out var orgUnit_B5101Id))
        {
            // DoA1: azusac@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5101_DoA1_2997Id) &&
                emailToUserId.TryGetValue("azusac@unops.org", out var user_B5101_DoA1_2997Id))
            {
                var key_B5101_DoA1_2997 = $"{orgUnit_B5101Id}_{role_B5101_DoA1_2997Id}_{user_B5101_DoA1_2997Id}";
                if (!existingRoleKeys.Contains(key_B5101_DoA1_2997))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_DoA1_2997Id}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_DoA1_2997Id,
                        UserId = user_B5101_DoA1_2997Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5101_DoA1_2997); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("azusac@unops.org")) missingUsers.Add("azusac@unops.org");
            }
            // DoA2: katyw@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5101_DoA2_8499Id) &&
                emailToUserId.TryGetValue("katyw@unops.org", out var user_B5101_DoA2_8499Id))
            {
                var key_B5101_DoA2_8499 = $"{orgUnit_B5101Id}_{role_B5101_DoA2_8499Id}_{user_B5101_DoA2_8499Id}";
                if (!existingRoleKeys.Contains(key_B5101_DoA2_8499))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_DoA2_8499Id}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_DoA2_8499Id,
                        UserId = user_B5101_DoA2_8499Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5101_DoA2_8499); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("katyw@unops.org")) missingUsers.Add("katyw@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5101_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5101_DoA3_3918Id))
            {
                var key_B5101_DoA3_3918 = $"{orgUnit_B5101Id}_{role_B5101_DoA3_3918Id}_{user_B5101_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5101_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_DoA3_3918Id}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_DoA3_3918Id,
                        UserId = user_B5101_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5101_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5101");
        }

        // B5104
        if (codeToOrgUnitId.TryGetValue("B5104", out var orgUnit_B5104Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5104_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5104_DoA3_2452Id))
            {
                var key_B5104_DoA3_2452 = $"{orgUnit_B5104Id}_{role_B5104_DoA3_2452Id}_{user_B5104_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5104_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5104Id} - {user_B5104_DoA3_2452Id}",
                        EntityId = orgUnit_B5104Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5104_DoA3_2452Id,
                        UserId = user_B5104_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5104_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // DoA2: usmana@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5104_DoA2_6095Id) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5104_DoA2_6095Id))
            {
                var key_B5104_DoA2_6095 = $"{orgUnit_B5104Id}_{role_B5104_DoA2_6095Id}_{user_B5104_DoA2_6095Id}";
                if (!existingRoleKeys.Contains(key_B5104_DoA2_6095))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5104Id} - {user_B5104_DoA2_6095Id}",
                        EntityId = orgUnit_B5104Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5104_DoA2_6095Id,
                        UserId = user_B5104_DoA2_6095Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5104_DoA2_6095); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // DoA1: anastaciar@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5104_DoA1_8090Id) &&
                emailToUserId.TryGetValue("anastaciar@unops.org", out var user_B5104_DoA1_8090Id))
            {
                var key_B5104_DoA1_8090 = $"{orgUnit_B5104Id}_{role_B5104_DoA1_8090Id}_{user_B5104_DoA1_8090Id}";
                if (!existingRoleKeys.Contains(key_B5104_DoA1_8090))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5104Id} - {user_B5104_DoA1_8090Id}",
                        EntityId = orgUnit_B5104Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5104_DoA1_8090Id,
                        UserId = user_B5104_DoA1_8090Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5104_DoA1_8090); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("anastaciar@unops.org")) missingUsers.Add("anastaciar@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5104");
        }

        // B5106
        if (codeToOrgUnitId.TryGetValue("B5106", out var orgUnit_B5106Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5106_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5106_DoA3_2452Id))
            {
                var key_B5106_DoA3_2452 = $"{orgUnit_B5106Id}_{role_B5106_DoA3_2452Id}_{user_B5106_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5106_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5106Id} - {user_B5106_DoA3_2452Id}",
                        EntityId = orgUnit_B5106Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5106_DoA3_2452Id,
                        UserId = user_B5106_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5106_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // DoA2: karunah@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5106_DoA2_5521Id) &&
                emailToUserId.TryGetValue("karunah@unops.org", out var user_B5106_DoA2_5521Id))
            {
                var key_B5106_DoA2_5521 = $"{orgUnit_B5106Id}_{role_B5106_DoA2_5521Id}_{user_B5106_DoA2_5521Id}";
                if (!existingRoleKeys.Contains(key_B5106_DoA2_5521))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5106Id} - {user_B5106_DoA2_5521Id}",
                        EntityId = orgUnit_B5106Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5106_DoA2_5521Id,
                        UserId = user_B5106_DoA2_5521Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5106_DoA2_5521); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("karunah@unops.org")) missingUsers.Add("karunah@unops.org");
            }
            // DoA1: sophien@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5106_DoA1_7915Id) &&
                emailToUserId.TryGetValue("sophien@unops.org", out var user_B5106_DoA1_7915Id))
            {
                var key_B5106_DoA1_7915 = $"{orgUnit_B5106Id}_{role_B5106_DoA1_7915Id}_{user_B5106_DoA1_7915Id}";
                if (!existingRoleKeys.Contains(key_B5106_DoA1_7915))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5106Id} - {user_B5106_DoA1_7915Id}",
                        EntityId = orgUnit_B5106Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5106_DoA1_7915Id,
                        UserId = user_B5106_DoA1_7915Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5106_DoA1_7915); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sophien@unops.org")) missingUsers.Add("sophien@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5106");
        }

        // B5107
        if (codeToOrgUnitId.TryGetValue("B5107", out var orgUnit_B5107Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5107_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5107_DoA3_6407Id))
            {
                var key_B5107_DoA3_6407 = $"{orgUnit_B5107Id}_{role_B5107_DoA3_6407Id}_{user_B5107_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA3_6407Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA3_6407Id,
                        UserId = user_B5107_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5107_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5107_DoA2_6407Id))
            {
                var key_B5107_DoA2_6407 = $"{orgUnit_B5107Id}_{role_B5107_DoA2_6407Id}_{user_B5107_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA2_6407Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA2_6407Id,
                        UserId = user_B5107_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA1: sabinek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5107_DoA1_4029Id) &&
                emailToUserId.TryGetValue("sabinek@unops.org", out var user_B5107_DoA1_4029Id))
            {
                var key_B5107_DoA1_4029 = $"{orgUnit_B5107Id}_{role_B5107_DoA1_4029Id}_{user_B5107_DoA1_4029Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA1_4029))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA1_4029Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA1_4029Id,
                        UserId = user_B5107_DoA1_4029Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA1_4029); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sabinek@unops.org")) missingUsers.Add("sabinek@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5107_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5107_DoA1_175Id))
            {
                var key_B5107_DoA1_175 = $"{orgUnit_B5107Id}_{role_B5107_DoA1_175Id}_{user_B5107_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA1_175Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA1_175Id,
                        UserId = user_B5107_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5107_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5107_DoA3_3632Id))
            {
                var key_B5107_DoA3_3632 = $"{orgUnit_B5107Id}_{role_B5107_DoA3_3632Id}_{user_B5107_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA3_3632Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA3_3632Id,
                        UserId = user_B5107_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: peteron@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5107_DoA1_6267Id) &&
                emailToUserId.TryGetValue("peteron@unops.org", out var user_B5107_DoA1_6267Id))
            {
                var key_B5107_DoA1_6267 = $"{orgUnit_B5107Id}_{role_B5107_DoA1_6267Id}_{user_B5107_DoA1_6267Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA1_6267))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA1_6267Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA1_6267Id,
                        UserId = user_B5107_DoA1_6267Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA1_6267); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("peteron@unops.org")) missingUsers.Add("peteron@unops.org");
            }
            // DoA2: simonettas@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5107_DoA2_6238Id) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5107_DoA2_6238Id))
            {
                var key_B5107_DoA2_6238 = $"{orgUnit_B5107Id}_{role_B5107_DoA2_6238Id}_{user_B5107_DoA2_6238Id}";
                if (!existingRoleKeys.Contains(key_B5107_DoA2_6238))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_DoA2_6238Id}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_DoA2_6238Id,
                        UserId = user_B5107_DoA2_6238Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5107_DoA2_6238); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5107");
        }

        // B5109
        if (codeToOrgUnitId.TryGetValue("B5109", out var orgUnit_B5109Id))
        {
            // DoA2: michelat@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5109_DoA2_1259Id) &&
                emailToUserId.TryGetValue("michelat@unops.org", out var user_B5109_DoA2_1259Id))
            {
                var key_B5109_DoA2_1259 = $"{orgUnit_B5109Id}_{role_B5109_DoA2_1259Id}_{user_B5109_DoA2_1259Id}";
                if (!existingRoleKeys.Contains(key_B5109_DoA2_1259))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_DoA2_1259Id}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_DoA2_1259Id,
                        UserId = user_B5109_DoA2_1259Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5109_DoA2_1259); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("michelat@unops.org")) missingUsers.Add("michelat@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5109_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5109_DoA3_6407Id))
            {
                var key_B5109_DoA3_6407 = $"{orgUnit_B5109Id}_{role_B5109_DoA3_6407Id}_{user_B5109_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5109_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_DoA3_6407Id}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_DoA3_6407Id,
                        UserId = user_B5109_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5109_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5109_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5109_DoA2_6407Id))
            {
                var key_B5109_DoA2_6407 = $"{orgUnit_B5109Id}_{role_B5109_DoA2_6407Id}_{user_B5109_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5109_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_DoA2_6407Id}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_DoA2_6407Id,
                        UserId = user_B5109_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5109_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5109_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5109_DoA3_3632Id))
            {
                var key_B5109_DoA3_3632 = $"{orgUnit_B5109Id}_{role_B5109_DoA3_3632Id}_{user_B5109_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5109_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_DoA3_3632Id}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_DoA3_3632Id,
                        UserId = user_B5109_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5109_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: daliborkak@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5109_DoA1_5247Id) &&
                emailToUserId.TryGetValue("daliborkak@unops.org", out var user_B5109_DoA1_5247Id))
            {
                var key_B5109_DoA1_5247 = $"{orgUnit_B5109Id}_{role_B5109_DoA1_5247Id}_{user_B5109_DoA1_5247Id}";
                if (!existingRoleKeys.Contains(key_B5109_DoA1_5247))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_DoA1_5247Id}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_DoA1_5247Id,
                        UserId = user_B5109_DoA1_5247Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5109_DoA1_5247); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("daliborkak@unops.org")) missingUsers.Add("daliborkak@unops.org");
            }
            // DoA1: markov@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5109_DoA1_381Id) &&
                emailToUserId.TryGetValue("markov@unops.org", out var user_B5109_DoA1_381Id))
            {
                var key_B5109_DoA1_381 = $"{orgUnit_B5109Id}_{role_B5109_DoA1_381Id}_{user_B5109_DoA1_381Id}";
                if (!existingRoleKeys.Contains(key_B5109_DoA1_381))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_DoA1_381Id}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_DoA1_381Id,
                        UserId = user_B5109_DoA1_381Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5109_DoA1_381); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("markov@unops.org")) missingUsers.Add("markov@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5109");
        }

        // B5110
        if (codeToOrgUnitId.TryGetValue("B5110", out var orgUnit_B5110Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5110_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5110_DoA2_6407Id))
            {
                var key_B5110_DoA2_6407 = $"{orgUnit_B5110Id}_{role_B5110_DoA2_6407Id}_{user_B5110_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5110_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_DoA2_6407Id}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_DoA2_6407Id,
                        UserId = user_B5110_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5110_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5110_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5110_DoA3_6407Id))
            {
                var key_B5110_DoA3_6407 = $"{orgUnit_B5110Id}_{role_B5110_DoA3_6407Id}_{user_B5110_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5110_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_DoA3_6407Id}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_DoA3_6407Id,
                        UserId = user_B5110_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5110_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5110_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5110_DoA3_3632Id))
            {
                var key_B5110_DoA3_3632 = $"{orgUnit_B5110Id}_{role_B5110_DoA3_3632Id}_{user_B5110_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5110_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_DoA3_3632Id}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_DoA3_3632Id,
                        UserId = user_B5110_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5110_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA2: michelat@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5110_DoA2_1259Id) &&
                emailToUserId.TryGetValue("michelat@unops.org", out var user_B5110_DoA2_1259Id))
            {
                var key_B5110_DoA2_1259 = $"{orgUnit_B5110Id}_{role_B5110_DoA2_1259Id}_{user_B5110_DoA2_1259Id}";
                if (!existingRoleKeys.Contains(key_B5110_DoA2_1259))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_DoA2_1259Id}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_DoA2_1259Id,
                        UserId = user_B5110_DoA2_1259Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5110_DoA2_1259); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("michelat@unops.org")) missingUsers.Add("michelat@unops.org");
            }
            // DoA1: daliborkak@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5110_DoA1_5247Id) &&
                emailToUserId.TryGetValue("daliborkak@unops.org", out var user_B5110_DoA1_5247Id))
            {
                var key_B5110_DoA1_5247 = $"{orgUnit_B5110Id}_{role_B5110_DoA1_5247Id}_{user_B5110_DoA1_5247Id}";
                if (!existingRoleKeys.Contains(key_B5110_DoA1_5247))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_DoA1_5247Id}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_DoA1_5247Id,
                        UserId = user_B5110_DoA1_5247Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5110_DoA1_5247); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("daliborkak@unops.org")) missingUsers.Add("daliborkak@unops.org");
            }
            // DoA1: markov@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5110_DoA1_381Id) &&
                emailToUserId.TryGetValue("markov@unops.org", out var user_B5110_DoA1_381Id))
            {
                var key_B5110_DoA1_381 = $"{orgUnit_B5110Id}_{role_B5110_DoA1_381Id}_{user_B5110_DoA1_381Id}";
                if (!existingRoleKeys.Contains(key_B5110_DoA1_381))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_DoA1_381Id}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_DoA1_381Id,
                        UserId = user_B5110_DoA1_381Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5110_DoA1_381); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("markov@unops.org")) missingUsers.Add("markov@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5110");
        }

        // B5114
        if (codeToOrgUnitId.TryGetValue("B5114", out var orgUnit_B5114Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5114_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5114_DoA2_6407Id))
            {
                var key_B5114_DoA2_6407 = $"{orgUnit_B5114Id}_{role_B5114_DoA2_6407Id}_{user_B5114_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5114_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5114Id} - {user_B5114_DoA2_6407Id}",
                        EntityId = orgUnit_B5114Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5114_DoA2_6407Id,
                        UserId = user_B5114_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5114_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5114_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5114_DoA3_6407Id))
            {
                var key_B5114_DoA3_6407 = $"{orgUnit_B5114Id}_{role_B5114_DoA3_6407Id}_{user_B5114_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5114_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5114Id} - {user_B5114_DoA3_6407Id}",
                        EntityId = orgUnit_B5114Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5114_DoA3_6407Id,
                        UserId = user_B5114_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5114_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5114_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5114_DoA2_729Id))
            {
                var key_B5114_DoA2_729 = $"{orgUnit_B5114Id}_{role_B5114_DoA2_729Id}_{user_B5114_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5114_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5114Id} - {user_B5114_DoA2_729Id}",
                        EntityId = orgUnit_B5114Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5114_DoA2_729Id,
                        UserId = user_B5114_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5114_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5114_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5114_DoA1_9154Id))
            {
                var key_B5114_DoA1_9154 = $"{orgUnit_B5114Id}_{role_B5114_DoA1_9154Id}_{user_B5114_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5114_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5114Id} - {user_B5114_DoA1_9154Id}",
                        EntityId = orgUnit_B5114Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5114_DoA1_9154Id,
                        UserId = user_B5114_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5114_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5114_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5114_DoA3_3632Id))
            {
                var key_B5114_DoA3_3632 = $"{orgUnit_B5114Id}_{role_B5114_DoA3_3632Id}_{user_B5114_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5114_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5114Id} - {user_B5114_DoA3_3632Id}",
                        EntityId = orgUnit_B5114Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5114_DoA3_3632Id,
                        UserId = user_B5114_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5114_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5114");
        }

        // B5115
        if (codeToOrgUnitId.TryGetValue("B5115", out var orgUnit_B5115Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5115_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5115_DoA3_6407Id))
            {
                var key_B5115_DoA3_6407 = $"{orgUnit_B5115Id}_{role_B5115_DoA3_6407Id}_{user_B5115_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5115_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5115Id} - {user_B5115_DoA3_6407Id}",
                        EntityId = orgUnit_B5115Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5115_DoA3_6407Id,
                        UserId = user_B5115_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5115_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5115_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5115_DoA2_6407Id))
            {
                var key_B5115_DoA2_6407 = $"{orgUnit_B5115Id}_{role_B5115_DoA2_6407Id}_{user_B5115_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5115_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5115Id} - {user_B5115_DoA2_6407Id}",
                        EntityId = orgUnit_B5115Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5115_DoA2_6407Id,
                        UserId = user_B5115_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5115_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5115_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5115_DoA2_729Id))
            {
                var key_B5115_DoA2_729 = $"{orgUnit_B5115Id}_{role_B5115_DoA2_729Id}_{user_B5115_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5115_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5115Id} - {user_B5115_DoA2_729Id}",
                        EntityId = orgUnit_B5115Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5115_DoA2_729Id,
                        UserId = user_B5115_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5115_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5115_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5115_DoA1_9154Id))
            {
                var key_B5115_DoA1_9154 = $"{orgUnit_B5115Id}_{role_B5115_DoA1_9154Id}_{user_B5115_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5115_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5115Id} - {user_B5115_DoA1_9154Id}",
                        EntityId = orgUnit_B5115Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5115_DoA1_9154Id,
                        UserId = user_B5115_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5115_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5115_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5115_DoA3_3632Id))
            {
                var key_B5115_DoA3_3632 = $"{orgUnit_B5115Id}_{role_B5115_DoA3_3632Id}_{user_B5115_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5115_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5115Id} - {user_B5115_DoA3_3632Id}",
                        EntityId = orgUnit_B5115Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5115_DoA3_3632Id,
                        UserId = user_B5115_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5115_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5115");
        }

        // B5116
        if (codeToOrgUnitId.TryGetValue("B5116", out var orgUnit_B5116Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5116_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5116_DoA3_6407Id))
            {
                var key_B5116_DoA3_6407 = $"{orgUnit_B5116Id}_{role_B5116_DoA3_6407Id}_{user_B5116_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5116_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_DoA3_6407Id}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_DoA3_6407Id,
                        UserId = user_B5116_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5116_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5116_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5116_DoA2_6407Id))
            {
                var key_B5116_DoA2_6407 = $"{orgUnit_B5116Id}_{role_B5116_DoA2_6407Id}_{user_B5116_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5116_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_DoA2_6407Id}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_DoA2_6407Id,
                        UserId = user_B5116_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5116_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5116_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5116_DoA3_3632Id))
            {
                var key_B5116_DoA3_3632 = $"{orgUnit_B5116Id}_{role_B5116_DoA3_3632Id}_{user_B5116_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5116_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_DoA3_3632Id}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_DoA3_3632Id,
                        UserId = user_B5116_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5116_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA2: massimodi@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5116_DoA2_4204Id) &&
                emailToUserId.TryGetValue("massimodi@unops.org", out var user_B5116_DoA2_4204Id))
            {
                var key_B5116_DoA2_4204 = $"{orgUnit_B5116Id}_{role_B5116_DoA2_4204Id}_{user_B5116_DoA2_4204Id}";
                if (!existingRoleKeys.Contains(key_B5116_DoA2_4204))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_DoA2_4204Id}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_DoA2_4204Id,
                        UserId = user_B5116_DoA2_4204Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5116_DoA2_4204); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("massimodi@unops.org")) missingUsers.Add("massimodi@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5116");
        }

        // B5118
        if (codeToOrgUnitId.TryGetValue("B5118", out var orgUnit_B5118Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5118_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5118_DoA3_6407Id))
            {
                var key_B5118_DoA3_6407 = $"{orgUnit_B5118Id}_{role_B5118_DoA3_6407Id}_{user_B5118_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5118_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5118Id} - {user_B5118_DoA3_6407Id}",
                        EntityId = orgUnit_B5118Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5118_DoA3_6407Id,
                        UserId = user_B5118_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5118_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5118_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5118_DoA2_6407Id))
            {
                var key_B5118_DoA2_6407 = $"{orgUnit_B5118Id}_{role_B5118_DoA2_6407Id}_{user_B5118_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5118_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5118Id} - {user_B5118_DoA2_6407Id}",
                        EntityId = orgUnit_B5118Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5118_DoA2_6407Id,
                        UserId = user_B5118_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5118_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5118_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5118_DoA2_729Id))
            {
                var key_B5118_DoA2_729 = $"{orgUnit_B5118Id}_{role_B5118_DoA2_729Id}_{user_B5118_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5118_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5118Id} - {user_B5118_DoA2_729Id}",
                        EntityId = orgUnit_B5118Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5118_DoA2_729Id,
                        UserId = user_B5118_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5118_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5118_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5118_DoA1_9154Id))
            {
                var key_B5118_DoA1_9154 = $"{orgUnit_B5118Id}_{role_B5118_DoA1_9154Id}_{user_B5118_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5118_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5118Id} - {user_B5118_DoA1_9154Id}",
                        EntityId = orgUnit_B5118Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5118_DoA1_9154Id,
                        UserId = user_B5118_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5118_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5118_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5118_DoA3_3632Id))
            {
                var key_B5118_DoA3_3632 = $"{orgUnit_B5118Id}_{role_B5118_DoA3_3632Id}_{user_B5118_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5118_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5118Id} - {user_B5118_DoA3_3632Id}",
                        EntityId = orgUnit_B5118Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5118_DoA3_3632Id,
                        UserId = user_B5118_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5118_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5118");
        }

        // B5120
        if (codeToOrgUnitId.TryGetValue("B5120", out var orgUnit_B5120Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5120_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5120_DoA3_2452Id))
            {
                var key_B5120_DoA3_2452 = $"{orgUnit_B5120Id}_{role_B5120_DoA3_2452Id}_{user_B5120_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5120_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5120Id} - {user_B5120_DoA3_2452Id}",
                        EntityId = orgUnit_B5120Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5120_DoA3_2452Id,
                        UserId = user_B5120_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5120_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5120");
        }

        // B5121
        if (codeToOrgUnitId.TryGetValue("B5121", out var orgUnit_B5121Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5121_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5121_DoA3_2452Id))
            {
                var key_B5121_DoA3_2452 = $"{orgUnit_B5121Id}_{role_B5121_DoA3_2452Id}_{user_B5121_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5121_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5121Id} - {user_B5121_DoA3_2452Id}",
                        EntityId = orgUnit_B5121Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5121_DoA3_2452Id,
                        UserId = user_B5121_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5121_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // DoA2: usmana@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5121_DoA2_6095Id) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5121_DoA2_6095Id))
            {
                var key_B5121_DoA2_6095 = $"{orgUnit_B5121Id}_{role_B5121_DoA2_6095Id}_{user_B5121_DoA2_6095Id}";
                if (!existingRoleKeys.Contains(key_B5121_DoA2_6095))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5121Id} - {user_B5121_DoA2_6095Id}",
                        EntityId = orgUnit_B5121Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5121_DoA2_6095Id,
                        UserId = user_B5121_DoA2_6095Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5121_DoA2_6095); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5121");
        }

        // B5122
        if (codeToOrgUnitId.TryGetValue("B5122", out var orgUnit_B5122Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5122_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5122_DoA3_2452Id))
            {
                var key_B5122_DoA3_2452 = $"{orgUnit_B5122Id}_{role_B5122_DoA3_2452Id}_{user_B5122_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5122_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5122Id} - {user_B5122_DoA3_2452Id}",
                        EntityId = orgUnit_B5122Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5122_DoA3_2452Id,
                        UserId = user_B5122_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5122_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // DoA2: usmana@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5122_DoA2_6095Id) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5122_DoA2_6095Id))
            {
                var key_B5122_DoA2_6095 = $"{orgUnit_B5122Id}_{role_B5122_DoA2_6095Id}_{user_B5122_DoA2_6095Id}";
                if (!existingRoleKeys.Contains(key_B5122_DoA2_6095))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5122Id} - {user_B5122_DoA2_6095Id}",
                        EntityId = orgUnit_B5122Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5122_DoA2_6095Id,
                        UserId = user_B5122_DoA2_6095Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5122_DoA2_6095); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5122");
        }

        // B5123
        if (codeToOrgUnitId.TryGetValue("B5123", out var orgUnit_B5123Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5123_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5123_DoA3_2452Id))
            {
                var key_B5123_DoA3_2452 = $"{orgUnit_B5123Id}_{role_B5123_DoA3_2452Id}_{user_B5123_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5123_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5123Id} - {user_B5123_DoA3_2452Id}",
                        EntityId = orgUnit_B5123Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5123_DoA3_2452Id,
                        UserId = user_B5123_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5123_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // DoA2: usmana@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5123_DoA2_6095Id) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5123_DoA2_6095Id))
            {
                var key_B5123_DoA2_6095 = $"{orgUnit_B5123Id}_{role_B5123_DoA2_6095Id}_{user_B5123_DoA2_6095Id}";
                if (!existingRoleKeys.Contains(key_B5123_DoA2_6095))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5123Id} - {user_B5123_DoA2_6095Id}",
                        EntityId = orgUnit_B5123Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5123_DoA2_6095Id,
                        UserId = user_B5123_DoA2_6095Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5123_DoA2_6095); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5123");
        }

        // B5124
        if (codeToOrgUnitId.TryGetValue("B5124", out var orgUnit_B5124Id))
        {
            // DoA3: banak@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5124_DoA3_2452Id) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5124_DoA3_2452Id))
            {
                var key_B5124_DoA3_2452 = $"{orgUnit_B5124Id}_{role_B5124_DoA3_2452Id}_{user_B5124_DoA3_2452Id}";
                if (!existingRoleKeys.Contains(key_B5124_DoA3_2452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5124Id} - {user_B5124_DoA3_2452Id}",
                        EntityId = orgUnit_B5124Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5124_DoA3_2452Id,
                        UserId = user_B5124_DoA3_2452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5124_DoA3_2452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // DoA2: usmana@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5124_DoA2_6095Id) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5124_DoA2_6095Id))
            {
                var key_B5124_DoA2_6095 = $"{orgUnit_B5124Id}_{role_B5124_DoA2_6095Id}_{user_B5124_DoA2_6095Id}";
                if (!existingRoleKeys.Contains(key_B5124_DoA2_6095))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5124Id} - {user_B5124_DoA2_6095Id}",
                        EntityId = orgUnit_B5124Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5124_DoA2_6095Id,
                        UserId = user_B5124_DoA2_6095Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5124_DoA2_6095); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5124");
        }

        // B5130
        if (codeToOrgUnitId.TryGetValue("B5130", out var orgUnit_B5130Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5130_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5130_DoA3_6407Id))
            {
                var key_B5130_DoA3_6407 = $"{orgUnit_B5130Id}_{role_B5130_DoA3_6407Id}_{user_B5130_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5130_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_DoA3_6407Id}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_DoA3_6407Id,
                        UserId = user_B5130_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5130_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5130_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5130_DoA2_6407Id))
            {
                var key_B5130_DoA2_6407 = $"{orgUnit_B5130Id}_{role_B5130_DoA2_6407Id}_{user_B5130_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5130_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_DoA2_6407Id}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_DoA2_6407Id,
                        UserId = user_B5130_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5130_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5130_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5130_DoA1_175Id))
            {
                var key_B5130_DoA1_175 = $"{orgUnit_B5130Id}_{role_B5130_DoA1_175Id}_{user_B5130_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5130_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_DoA1_175Id}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_DoA1_175Id,
                        UserId = user_B5130_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5130_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5130_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5130_DoA3_3632Id))
            {
                var key_B5130_DoA3_3632 = $"{orgUnit_B5130Id}_{role_B5130_DoA3_3632Id}_{user_B5130_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5130_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_DoA3_3632Id}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_DoA3_3632Id,
                        UserId = user_B5130_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5130_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: peteron@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5130_DoA1_6267Id) &&
                emailToUserId.TryGetValue("peteron@unops.org", out var user_B5130_DoA1_6267Id))
            {
                var key_B5130_DoA1_6267 = $"{orgUnit_B5130Id}_{role_B5130_DoA1_6267Id}_{user_B5130_DoA1_6267Id}";
                if (!existingRoleKeys.Contains(key_B5130_DoA1_6267))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_DoA1_6267Id}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_DoA1_6267Id,
                        UserId = user_B5130_DoA1_6267Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5130_DoA1_6267); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("peteron@unops.org")) missingUsers.Add("peteron@unops.org");
            }
            // DoA2: simonettas@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5130_DoA2_6238Id) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5130_DoA2_6238Id))
            {
                var key_B5130_DoA2_6238 = $"{orgUnit_B5130Id}_{role_B5130_DoA2_6238Id}_{user_B5130_DoA2_6238Id}";
                if (!existingRoleKeys.Contains(key_B5130_DoA2_6238))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_DoA2_6238Id}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_DoA2_6238Id,
                        UserId = user_B5130_DoA2_6238Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5130_DoA2_6238); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5130");
        }

        // B5131
        if (codeToOrgUnitId.TryGetValue("B5131", out var orgUnit_B5131Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5131_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5131_DoA2_6407Id))
            {
                var key_B5131_DoA2_6407 = $"{orgUnit_B5131Id}_{role_B5131_DoA2_6407Id}_{user_B5131_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA2_6407Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA2_6407Id,
                        UserId = user_B5131_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5131_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5131_DoA3_6407Id))
            {
                var key_B5131_DoA3_6407 = $"{orgUnit_B5131Id}_{role_B5131_DoA3_6407Id}_{user_B5131_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA3_6407Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA3_6407Id,
                        UserId = user_B5131_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA1: sabinek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5131_DoA1_4029Id) &&
                emailToUserId.TryGetValue("sabinek@unops.org", out var user_B5131_DoA1_4029Id))
            {
                var key_B5131_DoA1_4029 = $"{orgUnit_B5131Id}_{role_B5131_DoA1_4029Id}_{user_B5131_DoA1_4029Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA1_4029))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA1_4029Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA1_4029Id,
                        UserId = user_B5131_DoA1_4029Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA1_4029); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sabinek@unops.org")) missingUsers.Add("sabinek@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5131_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5131_DoA1_175Id))
            {
                var key_B5131_DoA1_175 = $"{orgUnit_B5131Id}_{role_B5131_DoA1_175Id}_{user_B5131_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA1_175Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA1_175Id,
                        UserId = user_B5131_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5131_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5131_DoA3_3632Id))
            {
                var key_B5131_DoA3_3632 = $"{orgUnit_B5131Id}_{role_B5131_DoA3_3632Id}_{user_B5131_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA3_3632Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA3_3632Id,
                        UserId = user_B5131_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: peteron@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5131_DoA1_6267Id) &&
                emailToUserId.TryGetValue("peteron@unops.org", out var user_B5131_DoA1_6267Id))
            {
                var key_B5131_DoA1_6267 = $"{orgUnit_B5131Id}_{role_B5131_DoA1_6267Id}_{user_B5131_DoA1_6267Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA1_6267))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA1_6267Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA1_6267Id,
                        UserId = user_B5131_DoA1_6267Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA1_6267); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("peteron@unops.org")) missingUsers.Add("peteron@unops.org");
            }
            // DoA2: simonettas@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5131_DoA2_6238Id) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5131_DoA2_6238Id))
            {
                var key_B5131_DoA2_6238 = $"{orgUnit_B5131Id}_{role_B5131_DoA2_6238Id}_{user_B5131_DoA2_6238Id}";
                if (!existingRoleKeys.Contains(key_B5131_DoA2_6238))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_DoA2_6238Id}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_DoA2_6238Id,
                        UserId = user_B5131_DoA2_6238Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5131_DoA2_6238); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5131");
        }

        // B5132
        if (codeToOrgUnitId.TryGetValue("B5132", out var orgUnit_B5132Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5132_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5132_DoA2_6407Id))
            {
                var key_B5132_DoA2_6407 = $"{orgUnit_B5132Id}_{role_B5132_DoA2_6407Id}_{user_B5132_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5132_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_DoA2_6407Id}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_DoA2_6407Id,
                        UserId = user_B5132_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5132_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5132_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5132_DoA3_6407Id))
            {
                var key_B5132_DoA3_6407 = $"{orgUnit_B5132Id}_{role_B5132_DoA3_6407Id}_{user_B5132_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5132_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_DoA3_6407Id}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_DoA3_6407Id,
                        UserId = user_B5132_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5132_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5132_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5132_DoA1_175Id))
            {
                var key_B5132_DoA1_175 = $"{orgUnit_B5132Id}_{role_B5132_DoA1_175Id}_{user_B5132_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5132_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_DoA1_175Id}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_DoA1_175Id,
                        UserId = user_B5132_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5132_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5132_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5132_DoA3_3632Id))
            {
                var key_B5132_DoA3_3632 = $"{orgUnit_B5132Id}_{role_B5132_DoA3_3632Id}_{user_B5132_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5132_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_DoA3_3632Id}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_DoA3_3632Id,
                        UserId = user_B5132_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5132_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: peteron@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5132_DoA1_6267Id) &&
                emailToUserId.TryGetValue("peteron@unops.org", out var user_B5132_DoA1_6267Id))
            {
                var key_B5132_DoA1_6267 = $"{orgUnit_B5132Id}_{role_B5132_DoA1_6267Id}_{user_B5132_DoA1_6267Id}";
                if (!existingRoleKeys.Contains(key_B5132_DoA1_6267))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_DoA1_6267Id}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_DoA1_6267Id,
                        UserId = user_B5132_DoA1_6267Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5132_DoA1_6267); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("peteron@unops.org")) missingUsers.Add("peteron@unops.org");
            }
            // DoA2: simonettas@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5132_DoA2_6238Id) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5132_DoA2_6238Id))
            {
                var key_B5132_DoA2_6238 = $"{orgUnit_B5132Id}_{role_B5132_DoA2_6238Id}_{user_B5132_DoA2_6238Id}";
                if (!existingRoleKeys.Contains(key_B5132_DoA2_6238))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_DoA2_6238Id}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_DoA2_6238Id,
                        UserId = user_B5132_DoA2_6238Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5132_DoA2_6238); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5132");
        }

        // B5133
        if (codeToOrgUnitId.TryGetValue("B5133", out var orgUnit_B5133Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5133_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5133_DoA3_6407Id))
            {
                var key_B5133_DoA3_6407 = $"{orgUnit_B5133Id}_{role_B5133_DoA3_6407Id}_{user_B5133_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5133_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_DoA3_6407Id}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_DoA3_6407Id,
                        UserId = user_B5133_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5133_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA1: gurelg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5133_DoA1_175Id) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5133_DoA1_175Id))
            {
                var key_B5133_DoA1_175 = $"{orgUnit_B5133Id}_{role_B5133_DoA1_175Id}_{user_B5133_DoA1_175Id}";
                if (!existingRoleKeys.Contains(key_B5133_DoA1_175))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_DoA1_175Id}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_DoA1_175Id,
                        UserId = user_B5133_DoA1_175Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5133_DoA1_175); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5133_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5133_DoA3_3632Id))
            {
                var key_B5133_DoA3_3632 = $"{orgUnit_B5133Id}_{role_B5133_DoA3_3632Id}_{user_B5133_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5133_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_DoA3_3632Id}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_DoA3_3632Id,
                        UserId = user_B5133_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5133_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // DoA1: peteron@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5133_DoA1_6267Id) &&
                emailToUserId.TryGetValue("peteron@unops.org", out var user_B5133_DoA1_6267Id))
            {
                var key_B5133_DoA1_6267 = $"{orgUnit_B5133Id}_{role_B5133_DoA1_6267Id}_{user_B5133_DoA1_6267Id}";
                if (!existingRoleKeys.Contains(key_B5133_DoA1_6267))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_DoA1_6267Id}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_DoA1_6267Id,
                        UserId = user_B5133_DoA1_6267Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5133_DoA1_6267); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("peteron@unops.org")) missingUsers.Add("peteron@unops.org");
            }
            // DoA2: simonettas@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5133_DoA2_6238Id) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5133_DoA2_6238Id))
            {
                var key_B5133_DoA2_6238 = $"{orgUnit_B5133Id}_{role_B5133_DoA2_6238Id}_{user_B5133_DoA2_6238Id}";
                if (!existingRoleKeys.Contains(key_B5133_DoA2_6238))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_DoA2_6238Id}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_DoA2_6238Id,
                        UserId = user_B5133_DoA2_6238Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5133_DoA2_6238); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
            // DoA1: elainec@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5133_DoA1_6003Id) &&
                emailToUserId.TryGetValue("elainec@unops.org", out var user_B5133_DoA1_6003Id))
            {
                var key_B5133_DoA1_6003 = $"{orgUnit_B5133Id}_{role_B5133_DoA1_6003Id}_{user_B5133_DoA1_6003Id}";
                if (!existingRoleKeys.Contains(key_B5133_DoA1_6003))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_DoA1_6003Id}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_DoA1_6003Id,
                        UserId = user_B5133_DoA1_6003Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5133_DoA1_6003); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("elainec@unops.org")) missingUsers.Add("elainec@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5133");
        }

        // B5141
        if (codeToOrgUnitId.TryGetValue("B5141", out var orgUnit_B5141Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5141_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5141_DoA2_6407Id))
            {
                var key_B5141_DoA2_6407 = $"{orgUnit_B5141Id}_{role_B5141_DoA2_6407Id}_{user_B5141_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5141_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5141Id} - {user_B5141_DoA2_6407Id}",
                        EntityId = orgUnit_B5141Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5141_DoA2_6407Id,
                        UserId = user_B5141_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5141_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5141_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5141_DoA3_6407Id))
            {
                var key_B5141_DoA3_6407 = $"{orgUnit_B5141Id}_{role_B5141_DoA3_6407Id}_{user_B5141_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5141_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5141Id} - {user_B5141_DoA3_6407Id}",
                        EntityId = orgUnit_B5141Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5141_DoA3_6407Id,
                        UserId = user_B5141_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5141_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5141_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5141_DoA2_729Id))
            {
                var key_B5141_DoA2_729 = $"{orgUnit_B5141Id}_{role_B5141_DoA2_729Id}_{user_B5141_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5141_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5141Id} - {user_B5141_DoA2_729Id}",
                        EntityId = orgUnit_B5141Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5141_DoA2_729Id,
                        UserId = user_B5141_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5141_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5141_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5141_DoA1_9154Id))
            {
                var key_B5141_DoA1_9154 = $"{orgUnit_B5141Id}_{role_B5141_DoA1_9154Id}_{user_B5141_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5141_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5141Id} - {user_B5141_DoA1_9154Id}",
                        EntityId = orgUnit_B5141Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5141_DoA1_9154Id,
                        UserId = user_B5141_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5141_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5141_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5141_DoA3_3632Id))
            {
                var key_B5141_DoA3_3632 = $"{orgUnit_B5141Id}_{role_B5141_DoA3_3632Id}_{user_B5141_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5141_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5141Id} - {user_B5141_DoA3_3632Id}",
                        EntityId = orgUnit_B5141Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5141_DoA3_3632Id,
                        UserId = user_B5141_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5141_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5141");
        }

        // B5143
        if (codeToOrgUnitId.TryGetValue("B5143", out var orgUnit_B5143Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5143_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5143_DoA3_6407Id))
            {
                var key_B5143_DoA3_6407 = $"{orgUnit_B5143Id}_{role_B5143_DoA3_6407Id}_{user_B5143_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5143_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5143Id} - {user_B5143_DoA3_6407Id}",
                        EntityId = orgUnit_B5143Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5143_DoA3_6407Id,
                        UserId = user_B5143_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5143_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5143_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5143_DoA2_6407Id))
            {
                var key_B5143_DoA2_6407 = $"{orgUnit_B5143Id}_{role_B5143_DoA2_6407Id}_{user_B5143_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5143_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5143Id} - {user_B5143_DoA2_6407Id}",
                        EntityId = orgUnit_B5143Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5143_DoA2_6407Id,
                        UserId = user_B5143_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5143_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5143_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5143_DoA2_729Id))
            {
                var key_B5143_DoA2_729 = $"{orgUnit_B5143Id}_{role_B5143_DoA2_729Id}_{user_B5143_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5143_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5143Id} - {user_B5143_DoA2_729Id}",
                        EntityId = orgUnit_B5143Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5143_DoA2_729Id,
                        UserId = user_B5143_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5143_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5143_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5143_DoA1_9154Id))
            {
                var key_B5143_DoA1_9154 = $"{orgUnit_B5143Id}_{role_B5143_DoA1_9154Id}_{user_B5143_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5143_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5143Id} - {user_B5143_DoA1_9154Id}",
                        EntityId = orgUnit_B5143Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5143_DoA1_9154Id,
                        UserId = user_B5143_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5143_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5143_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5143_DoA3_3632Id))
            {
                var key_B5143_DoA3_3632 = $"{orgUnit_B5143Id}_{role_B5143_DoA3_3632Id}_{user_B5143_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5143_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5143Id} - {user_B5143_DoA3_3632Id}",
                        EntityId = orgUnit_B5143Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5143_DoA3_3632Id,
                        UserId = user_B5143_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5143_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5143");
        }

        // B5145
        if (codeToOrgUnitId.TryGetValue("B5145", out var orgUnit_B5145Id))
        {
            // DoA2: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5145_DoA2_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5145_DoA2_6407Id))
            {
                var key_B5145_DoA2_6407 = $"{orgUnit_B5145Id}_{role_B5145_DoA2_6407Id}_{user_B5145_DoA2_6407Id}";
                if (!existingRoleKeys.Contains(key_B5145_DoA2_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5145Id} - {user_B5145_DoA2_6407Id}",
                        EntityId = orgUnit_B5145Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5145_DoA2_6407Id,
                        UserId = user_B5145_DoA2_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5145_DoA2_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5145_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5145_DoA3_6407Id))
            {
                var key_B5145_DoA3_6407 = $"{orgUnit_B5145Id}_{role_B5145_DoA3_6407Id}_{user_B5145_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5145_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5145Id} - {user_B5145_DoA3_6407Id}",
                        EntityId = orgUnit_B5145Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5145_DoA3_6407Id,
                        UserId = user_B5145_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5145_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5145_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5145_DoA2_729Id))
            {
                var key_B5145_DoA2_729 = $"{orgUnit_B5145Id}_{role_B5145_DoA2_729Id}_{user_B5145_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5145_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5145Id} - {user_B5145_DoA2_729Id}",
                        EntityId = orgUnit_B5145Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5145_DoA2_729Id,
                        UserId = user_B5145_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5145_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5145_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5145_DoA1_9154Id))
            {
                var key_B5145_DoA1_9154 = $"{orgUnit_B5145Id}_{role_B5145_DoA1_9154Id}_{user_B5145_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5145_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5145Id} - {user_B5145_DoA1_9154Id}",
                        EntityId = orgUnit_B5145Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5145_DoA1_9154Id,
                        UserId = user_B5145_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5145_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5145_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5145_DoA3_3632Id))
            {
                var key_B5145_DoA3_3632 = $"{orgUnit_B5145Id}_{role_B5145_DoA3_3632Id}_{user_B5145_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5145_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5145Id} - {user_B5145_DoA3_3632Id}",
                        EntityId = orgUnit_B5145Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5145_DoA3_3632Id,
                        UserId = user_B5145_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5145_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5145");
        }

        // B5146
        if (codeToOrgUnitId.TryGetValue("B5146", out var orgUnit_B5146Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5146_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5146_DoA3_6407Id))
            {
                var key_B5146_DoA3_6407 = $"{orgUnit_B5146Id}_{role_B5146_DoA3_6407Id}_{user_B5146_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5146_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5146Id} - {user_B5146_DoA3_6407Id}",
                        EntityId = orgUnit_B5146Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5146_DoA3_6407Id,
                        UserId = user_B5146_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5146_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5146_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5146_DoA2_729Id))
            {
                var key_B5146_DoA2_729 = $"{orgUnit_B5146Id}_{role_B5146_DoA2_729Id}_{user_B5146_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5146_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5146Id} - {user_B5146_DoA2_729Id}",
                        EntityId = orgUnit_B5146Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5146_DoA2_729Id,
                        UserId = user_B5146_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5146_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5146_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5146_DoA1_9154Id))
            {
                var key_B5146_DoA1_9154 = $"{orgUnit_B5146Id}_{role_B5146_DoA1_9154Id}_{user_B5146_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5146_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5146Id} - {user_B5146_DoA1_9154Id}",
                        EntityId = orgUnit_B5146Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5146_DoA1_9154Id,
                        UserId = user_B5146_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5146_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5146_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5146_DoA3_3632Id))
            {
                var key_B5146_DoA3_3632 = $"{orgUnit_B5146Id}_{role_B5146_DoA3_3632Id}_{user_B5146_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5146_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5146Id} - {user_B5146_DoA3_3632Id}",
                        EntityId = orgUnit_B5146Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5146_DoA3_3632Id,
                        UserId = user_B5146_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5146_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5146");
        }

        // B5147
        if (codeToOrgUnitId.TryGetValue("B5147", out var orgUnit_B5147Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5147_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5147_DoA3_6407Id))
            {
                var key_B5147_DoA3_6407 = $"{orgUnit_B5147Id}_{role_B5147_DoA3_6407Id}_{user_B5147_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5147_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5147Id} - {user_B5147_DoA3_6407Id}",
                        EntityId = orgUnit_B5147Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5147_DoA3_6407Id,
                        UserId = user_B5147_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5147_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5147_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5147_DoA2_729Id))
            {
                var key_B5147_DoA2_729 = $"{orgUnit_B5147Id}_{role_B5147_DoA2_729Id}_{user_B5147_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5147_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5147Id} - {user_B5147_DoA2_729Id}",
                        EntityId = orgUnit_B5147Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5147_DoA2_729Id,
                        UserId = user_B5147_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5147_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5147_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5147_DoA1_9154Id))
            {
                var key_B5147_DoA1_9154 = $"{orgUnit_B5147Id}_{role_B5147_DoA1_9154Id}_{user_B5147_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5147_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5147Id} - {user_B5147_DoA1_9154Id}",
                        EntityId = orgUnit_B5147Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5147_DoA1_9154Id,
                        UserId = user_B5147_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5147_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5147_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5147_DoA3_3632Id))
            {
                var key_B5147_DoA3_3632 = $"{orgUnit_B5147Id}_{role_B5147_DoA3_3632Id}_{user_B5147_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5147_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5147Id} - {user_B5147_DoA3_3632Id}",
                        EntityId = orgUnit_B5147Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5147_DoA3_3632Id,
                        UserId = user_B5147_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5147_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5147");
        }

        // B5148
        if (codeToOrgUnitId.TryGetValue("B5148", out var orgUnit_B5148Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5148_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5148_DoA3_6407Id))
            {
                var key_B5148_DoA3_6407 = $"{orgUnit_B5148Id}_{role_B5148_DoA3_6407Id}_{user_B5148_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5148_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5148Id} - {user_B5148_DoA3_6407Id}",
                        EntityId = orgUnit_B5148Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5148_DoA3_6407Id,
                        UserId = user_B5148_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5148_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5148_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5148_DoA2_729Id))
            {
                var key_B5148_DoA2_729 = $"{orgUnit_B5148Id}_{role_B5148_DoA2_729Id}_{user_B5148_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5148_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5148Id} - {user_B5148_DoA2_729Id}",
                        EntityId = orgUnit_B5148Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5148_DoA2_729Id,
                        UserId = user_B5148_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5148_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5148_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5148_DoA1_9154Id))
            {
                var key_B5148_DoA1_9154 = $"{orgUnit_B5148Id}_{role_B5148_DoA1_9154Id}_{user_B5148_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5148_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5148Id} - {user_B5148_DoA1_9154Id}",
                        EntityId = orgUnit_B5148Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5148_DoA1_9154Id,
                        UserId = user_B5148_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5148_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5148_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5148_DoA3_3632Id))
            {
                var key_B5148_DoA3_3632 = $"{orgUnit_B5148Id}_{role_B5148_DoA3_3632Id}_{user_B5148_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5148_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5148Id} - {user_B5148_DoA3_3632Id}",
                        EntityId = orgUnit_B5148Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5148_DoA3_3632Id,
                        UserId = user_B5148_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5148_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5148");
        }

        // B5149
        if (codeToOrgUnitId.TryGetValue("B5149", out var orgUnit_B5149Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5149_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5149_DoA3_6407Id))
            {
                var key_B5149_DoA3_6407 = $"{orgUnit_B5149Id}_{role_B5149_DoA3_6407Id}_{user_B5149_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5149_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5149Id} - {user_B5149_DoA3_6407Id}",
                        EntityId = orgUnit_B5149Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5149_DoA3_6407Id,
                        UserId = user_B5149_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5149_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5149_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5149_DoA2_729Id))
            {
                var key_B5149_DoA2_729 = $"{orgUnit_B5149Id}_{role_B5149_DoA2_729Id}_{user_B5149_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5149_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5149Id} - {user_B5149_DoA2_729Id}",
                        EntityId = orgUnit_B5149Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5149_DoA2_729Id,
                        UserId = user_B5149_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5149_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5149_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5149_DoA1_9154Id))
            {
                var key_B5149_DoA1_9154 = $"{orgUnit_B5149Id}_{role_B5149_DoA1_9154Id}_{user_B5149_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5149_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5149Id} - {user_B5149_DoA1_9154Id}",
                        EntityId = orgUnit_B5149Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5149_DoA1_9154Id,
                        UserId = user_B5149_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5149_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5149_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5149_DoA3_3632Id))
            {
                var key_B5149_DoA3_3632 = $"{orgUnit_B5149Id}_{role_B5149_DoA3_3632Id}_{user_B5149_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5149_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5149Id} - {user_B5149_DoA3_3632Id}",
                        EntityId = orgUnit_B5149Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5149_DoA3_3632Id,
                        UserId = user_B5149_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5149_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5149");
        }

        // B5150
        if (codeToOrgUnitId.TryGetValue("B5150", out var orgUnit_B5150Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5150_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5150_DoA3_6407Id))
            {
                var key_B5150_DoA3_6407 = $"{orgUnit_B5150Id}_{role_B5150_DoA3_6407Id}_{user_B5150_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5150_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5150Id} - {user_B5150_DoA3_6407Id}",
                        EntityId = orgUnit_B5150Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5150_DoA3_6407Id,
                        UserId = user_B5150_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5150_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5150_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5150_DoA2_729Id))
            {
                var key_B5150_DoA2_729 = $"{orgUnit_B5150Id}_{role_B5150_DoA2_729Id}_{user_B5150_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5150_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5150Id} - {user_B5150_DoA2_729Id}",
                        EntityId = orgUnit_B5150Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5150_DoA2_729Id,
                        UserId = user_B5150_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5150_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5150_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5150_DoA1_9154Id))
            {
                var key_B5150_DoA1_9154 = $"{orgUnit_B5150Id}_{role_B5150_DoA1_9154Id}_{user_B5150_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5150_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5150Id} - {user_B5150_DoA1_9154Id}",
                        EntityId = orgUnit_B5150Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5150_DoA1_9154Id,
                        UserId = user_B5150_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5150_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5150_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5150_DoA3_3632Id))
            {
                var key_B5150_DoA3_3632 = $"{orgUnit_B5150Id}_{role_B5150_DoA3_3632Id}_{user_B5150_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5150_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5150Id} - {user_B5150_DoA3_3632Id}",
                        EntityId = orgUnit_B5150Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5150_DoA3_3632Id,
                        UserId = user_B5150_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5150_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5150");
        }

        // B5151
        if (codeToOrgUnitId.TryGetValue("B5151", out var orgUnit_B5151Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5151_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5151_DoA3_6407Id))
            {
                var key_B5151_DoA3_6407 = $"{orgUnit_B5151Id}_{role_B5151_DoA3_6407Id}_{user_B5151_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5151_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5151Id} - {user_B5151_DoA3_6407Id}",
                        EntityId = orgUnit_B5151Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5151_DoA3_6407Id,
                        UserId = user_B5151_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5151_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5151_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5151_DoA2_729Id))
            {
                var key_B5151_DoA2_729 = $"{orgUnit_B5151Id}_{role_B5151_DoA2_729Id}_{user_B5151_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5151_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5151Id} - {user_B5151_DoA2_729Id}",
                        EntityId = orgUnit_B5151Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5151_DoA2_729Id,
                        UserId = user_B5151_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5151_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5151_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5151_DoA1_9154Id))
            {
                var key_B5151_DoA1_9154 = $"{orgUnit_B5151Id}_{role_B5151_DoA1_9154Id}_{user_B5151_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5151_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5151Id} - {user_B5151_DoA1_9154Id}",
                        EntityId = orgUnit_B5151Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5151_DoA1_9154Id,
                        UserId = user_B5151_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5151_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5151_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5151_DoA3_3632Id))
            {
                var key_B5151_DoA3_3632 = $"{orgUnit_B5151Id}_{role_B5151_DoA3_3632Id}_{user_B5151_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5151_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5151Id} - {user_B5151_DoA3_3632Id}",
                        EntityId = orgUnit_B5151Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5151_DoA3_3632Id,
                        UserId = user_B5151_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5151_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5151");
        }

        // B5152
        if (codeToOrgUnitId.TryGetValue("B5152", out var orgUnit_B5152Id))
        {
            // DoA3: christaa@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5152_DoA3_6407Id) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5152_DoA3_6407Id))
            {
                var key_B5152_DoA3_6407 = $"{orgUnit_B5152Id}_{role_B5152_DoA3_6407Id}_{user_B5152_DoA3_6407Id}";
                if (!existingRoleKeys.Contains(key_B5152_DoA3_6407))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5152Id} - {user_B5152_DoA3_6407Id}",
                        EntityId = orgUnit_B5152Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5152_DoA3_6407Id,
                        UserId = user_B5152_DoA3_6407Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5152_DoA3_6407); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // DoA2: andrewk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5152_DoA2_729Id) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5152_DoA2_729Id))
            {
                var key_B5152_DoA2_729 = $"{orgUnit_B5152Id}_{role_B5152_DoA2_729Id}_{user_B5152_DoA2_729Id}";
                if (!existingRoleKeys.Contains(key_B5152_DoA2_729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5152Id} - {user_B5152_DoA2_729Id}",
                        EntityId = orgUnit_B5152Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5152_DoA2_729Id,
                        UserId = user_B5152_DoA2_729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5152_DoA2_729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // DoA1: jean-francoisl@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5152_DoA1_9154Id) &&
                emailToUserId.TryGetValue("jean-francoisl@unops.org", out var user_B5152_DoA1_9154Id))
            {
                var key_B5152_DoA1_9154 = $"{orgUnit_B5152Id}_{role_B5152_DoA1_9154Id}_{user_B5152_DoA1_9154Id}";
                if (!existingRoleKeys.Contains(key_B5152_DoA1_9154))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5152Id} - {user_B5152_DoA1_9154Id}",
                        EntityId = orgUnit_B5152Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5152_DoA1_9154Id,
                        UserId = user_B5152_DoA1_9154Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5152_DoA1_9154); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jean-francoisl@unops.org")) missingUsers.Add("jean-francoisl@unops.org");
            }
            // DoA3: timl@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5152_DoA3_3632Id) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5152_DoA3_3632Id))
            {
                var key_B5152_DoA3_3632 = $"{orgUnit_B5152Id}_{role_B5152_DoA3_3632Id}_{user_B5152_DoA3_3632Id}";
                if (!existingRoleKeys.Contains(key_B5152_DoA3_3632))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5152Id} - {user_B5152_DoA3_3632Id}",
                        EntityId = orgUnit_B5152Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5152_DoA3_3632Id,
                        UserId = user_B5152_DoA3_3632Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5152_DoA3_3632); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5152");
        }

        // B5301
        if (codeToOrgUnitId.TryGetValue("B5301", out var orgUnit_B5301Id))
        {
            // DoA2: tatianaw@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5301_DoA2_6288Id) &&
                emailToUserId.TryGetValue("tatianaw@unops.org", out var user_B5301_DoA2_6288Id))
            {
                var key_B5301_DoA2_6288 = $"{orgUnit_B5301Id}_{role_B5301_DoA2_6288Id}_{user_B5301_DoA2_6288Id}";
                if (!existingRoleKeys.Contains(key_B5301_DoA2_6288))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_DoA2_6288Id}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_DoA2_6288Id,
                        UserId = user_B5301_DoA2_6288Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5301_DoA2_6288); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tatianaw@unops.org")) missingUsers.Add("tatianaw@unops.org");
            }
            // DoA1: fredericfr@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5301_DoA1_4542Id) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5301_DoA1_4542Id))
            {
                var key_B5301_DoA1_4542 = $"{orgUnit_B5301Id}_{role_B5301_DoA1_4542Id}_{user_B5301_DoA1_4542Id}";
                if (!existingRoleKeys.Contains(key_B5301_DoA1_4542))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_DoA1_4542Id}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_DoA1_4542Id,
                        UserId = user_B5301_DoA1_4542Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5301_DoA1_4542); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5301_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5301_DoA2_8434Id))
            {
                var key_B5301_DoA2_8434 = $"{orgUnit_B5301Id}_{role_B5301_DoA2_8434Id}_{user_B5301_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5301_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_DoA2_8434Id}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_DoA2_8434Id,
                        UserId = user_B5301_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5301_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5301_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5301_DoA3_8676Id))
            {
                var key_B5301_DoA3_8676 = $"{orgUnit_B5301Id}_{role_B5301_DoA3_8676Id}_{user_B5301_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5301_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_DoA3_8676Id}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_DoA3_8676Id,
                        UserId = user_B5301_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5301_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5301");
        }

        // B5302
        if (codeToOrgUnitId.TryGetValue("B5302", out var orgUnit_B5302Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5302_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5302_DoA1_5418Id))
            {
                var key_B5302_DoA1_5418 = $"{orgUnit_B5302Id}_{role_B5302_DoA1_5418Id}_{user_B5302_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5302_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_DoA1_5418Id}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_DoA1_5418Id,
                        UserId = user_B5302_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5302_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5302_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5302_DoA2_1135Id))
            {
                var key_B5302_DoA2_1135 = $"{orgUnit_B5302Id}_{role_B5302_DoA2_1135Id}_{user_B5302_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5302_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_DoA2_1135Id}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_DoA2_1135Id,
                        UserId = user_B5302_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5302_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5302_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5302_DoA1_6789Id))
            {
                var key_B5302_DoA1_6789 = $"{orgUnit_B5302Id}_{role_B5302_DoA1_6789Id}_{user_B5302_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5302_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_DoA1_6789Id}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_DoA1_6789Id,
                        UserId = user_B5302_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5302_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5302_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5302_DoA3_8676Id))
            {
                var key_B5302_DoA3_8676 = $"{orgUnit_B5302Id}_{role_B5302_DoA3_8676Id}_{user_B5302_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5302_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_DoA3_8676Id}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_DoA3_8676Id,
                        UserId = user_B5302_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5302_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5302");
        }

        // B5303
        if (codeToOrgUnitId.TryGetValue("B5303", out var orgUnit_B5303Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5303_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5303_DoA1_1570Id))
            {
                var key_B5303_DoA1_1570 = $"{orgUnit_B5303Id}_{role_B5303_DoA1_1570Id}_{user_B5303_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5303_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5303Id} - {user_B5303_DoA1_1570Id}",
                        EntityId = orgUnit_B5303Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5303_DoA1_1570Id,
                        UserId = user_B5303_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5303_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5303_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5303_DoA2_195Id))
            {
                var key_B5303_DoA2_195 = $"{orgUnit_B5303Id}_{role_B5303_DoA2_195Id}_{user_B5303_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5303_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5303Id} - {user_B5303_DoA2_195Id}",
                        EntityId = orgUnit_B5303Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5303_DoA2_195Id,
                        UserId = user_B5303_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5303_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5303_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5303_DoA3_8676Id))
            {
                var key_B5303_DoA3_8676 = $"{orgUnit_B5303Id}_{role_B5303_DoA3_8676Id}_{user_B5303_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5303_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5303Id} - {user_B5303_DoA3_8676Id}",
                        EntityId = orgUnit_B5303Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5303_DoA3_8676Id,
                        UserId = user_B5303_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5303_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5303");
        }

        // B5304
        if (codeToOrgUnitId.TryGetValue("B5304", out var orgUnit_B5304Id))
        {
            // DoA1: sonjav@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5304_DoA1_4170Id) &&
                emailToUserId.TryGetValue("sonjav@unops.org", out var user_B5304_DoA1_4170Id))
            {
                var key_B5304_DoA1_4170 = $"{orgUnit_B5304Id}_{role_B5304_DoA1_4170Id}_{user_B5304_DoA1_4170Id}";
                if (!existingRoleKeys.Contains(key_B5304_DoA1_4170))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_DoA1_4170Id}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_DoA1_4170Id,
                        UserId = user_B5304_DoA1_4170Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5304_DoA1_4170); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sonjav@unops.org")) missingUsers.Add("sonjav@unops.org");
            }
            // DoA1: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5304_DoA1_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5304_DoA1_1631Id))
            {
                var key_B5304_DoA1_1631 = $"{orgUnit_B5304Id}_{role_B5304_DoA1_1631Id}_{user_B5304_DoA1_1631Id}";
                if (!existingRoleKeys.Contains(key_B5304_DoA1_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_DoA1_1631Id}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_DoA1_1631Id,
                        UserId = user_B5304_DoA1_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5304_DoA1_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA2: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5304_DoA2_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5304_DoA2_1631Id))
            {
                var key_B5304_DoA2_1631 = $"{orgUnit_B5304Id}_{role_B5304_DoA2_1631Id}_{user_B5304_DoA2_1631Id}";
                if (!existingRoleKeys.Contains(key_B5304_DoA2_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_DoA2_1631Id}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_DoA2_1631Id,
                        UserId = user_B5304_DoA2_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5304_DoA2_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5304_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5304_DoA3_8676Id))
            {
                var key_B5304_DoA3_8676 = $"{orgUnit_B5304Id}_{role_B5304_DoA3_8676Id}_{user_B5304_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5304_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_DoA3_8676Id}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_DoA3_8676Id,
                        UserId = user_B5304_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5304_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5304");
        }

        // B5305
        if (codeToOrgUnitId.TryGetValue("B5305", out var orgUnit_B5305Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5305_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5305_DoA1_5418Id))
            {
                var key_B5305_DoA1_5418 = $"{orgUnit_B5305Id}_{role_B5305_DoA1_5418Id}_{user_B5305_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5305_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_DoA1_5418Id}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_DoA1_5418Id,
                        UserId = user_B5305_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5305_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5305_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5305_DoA2_1135Id))
            {
                var key_B5305_DoA2_1135 = $"{orgUnit_B5305Id}_{role_B5305_DoA2_1135Id}_{user_B5305_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5305_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_DoA2_1135Id}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_DoA2_1135Id,
                        UserId = user_B5305_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5305_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5305_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5305_DoA1_6789Id))
            {
                var key_B5305_DoA1_6789 = $"{orgUnit_B5305Id}_{role_B5305_DoA1_6789Id}_{user_B5305_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5305_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_DoA1_6789Id}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_DoA1_6789Id,
                        UserId = user_B5305_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5305_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5305_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5305_DoA3_8676Id))
            {
                var key_B5305_DoA3_8676 = $"{orgUnit_B5305Id}_{role_B5305_DoA3_8676Id}_{user_B5305_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5305_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_DoA3_8676Id}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_DoA3_8676Id,
                        UserId = user_B5305_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5305_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5305");
        }

        // B5306
        if (codeToOrgUnitId.TryGetValue("B5306", out var orgUnit_B5306Id))
        {
            // DoA1: boureimat@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5306_DoA1_8437Id) &&
                emailToUserId.TryGetValue("boureimat@unops.org", out var user_B5306_DoA1_8437Id))
            {
                var key_B5306_DoA1_8437 = $"{orgUnit_B5306Id}_{role_B5306_DoA1_8437Id}_{user_B5306_DoA1_8437Id}";
                if (!existingRoleKeys.Contains(key_B5306_DoA1_8437))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_DoA1_8437Id}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_DoA1_8437Id,
                        UserId = user_B5306_DoA1_8437Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5306_DoA1_8437); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("boureimat@unops.org")) missingUsers.Add("boureimat@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5306_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5306_DoA2_8434Id))
            {
                var key_B5306_DoA2_8434 = $"{orgUnit_B5306Id}_{role_B5306_DoA2_8434Id}_{user_B5306_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5306_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_DoA2_8434Id}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_DoA2_8434Id,
                        UserId = user_B5306_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5306_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5306_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5306_DoA3_8676Id))
            {
                var key_B5306_DoA3_8676 = $"{orgUnit_B5306Id}_{role_B5306_DoA3_8676Id}_{user_B5306_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5306_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_DoA3_8676Id}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_DoA3_8676Id,
                        UserId = user_B5306_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5306_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5306");
        }

        // B5308
        if (codeToOrgUnitId.TryGetValue("B5308", out var orgUnit_B5308Id))
        {
            // DoA1: sonjav@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5308_DoA1_4170Id) &&
                emailToUserId.TryGetValue("sonjav@unops.org", out var user_B5308_DoA1_4170Id))
            {
                var key_B5308_DoA1_4170 = $"{orgUnit_B5308Id}_{role_B5308_DoA1_4170Id}_{user_B5308_DoA1_4170Id}";
                if (!existingRoleKeys.Contains(key_B5308_DoA1_4170))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_DoA1_4170Id}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_DoA1_4170Id,
                        UserId = user_B5308_DoA1_4170Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5308_DoA1_4170); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sonjav@unops.org")) missingUsers.Add("sonjav@unops.org");
            }
            // DoA1: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5308_DoA1_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5308_DoA1_1631Id))
            {
                var key_B5308_DoA1_1631 = $"{orgUnit_B5308Id}_{role_B5308_DoA1_1631Id}_{user_B5308_DoA1_1631Id}";
                if (!existingRoleKeys.Contains(key_B5308_DoA1_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_DoA1_1631Id}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_DoA1_1631Id,
                        UserId = user_B5308_DoA1_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5308_DoA1_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA2: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5308_DoA2_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5308_DoA2_1631Id))
            {
                var key_B5308_DoA2_1631 = $"{orgUnit_B5308Id}_{role_B5308_DoA2_1631Id}_{user_B5308_DoA2_1631Id}";
                if (!existingRoleKeys.Contains(key_B5308_DoA2_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_DoA2_1631Id}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_DoA2_1631Id,
                        UserId = user_B5308_DoA2_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5308_DoA2_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5308_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5308_DoA3_8676Id))
            {
                var key_B5308_DoA3_8676 = $"{orgUnit_B5308Id}_{role_B5308_DoA3_8676Id}_{user_B5308_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5308_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_DoA3_8676Id}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_DoA3_8676Id,
                        UserId = user_B5308_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5308_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5308");
        }

        // B5309
        if (codeToOrgUnitId.TryGetValue("B5309", out var orgUnit_B5309Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5309_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5309_DoA1_5418Id))
            {
                var key_B5309_DoA1_5418 = $"{orgUnit_B5309Id}_{role_B5309_DoA1_5418Id}_{user_B5309_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5309_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_DoA1_5418Id}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_DoA1_5418Id,
                        UserId = user_B5309_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5309_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5309_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5309_DoA2_1135Id))
            {
                var key_B5309_DoA2_1135 = $"{orgUnit_B5309Id}_{role_B5309_DoA2_1135Id}_{user_B5309_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5309_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_DoA2_1135Id}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_DoA2_1135Id,
                        UserId = user_B5309_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5309_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5309_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5309_DoA1_6789Id))
            {
                var key_B5309_DoA1_6789 = $"{orgUnit_B5309Id}_{role_B5309_DoA1_6789Id}_{user_B5309_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5309_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_DoA1_6789Id}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_DoA1_6789Id,
                        UserId = user_B5309_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5309_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5309_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5309_DoA3_8676Id))
            {
                var key_B5309_DoA3_8676 = $"{orgUnit_B5309Id}_{role_B5309_DoA3_8676Id}_{user_B5309_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5309_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_DoA3_8676Id}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_DoA3_8676Id,
                        UserId = user_B5309_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5309_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5309");
        }

        // B5310
        if (codeToOrgUnitId.TryGetValue("B5310", out var orgUnit_B5310Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5310_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5310_DoA1_5418Id))
            {
                var key_B5310_DoA1_5418 = $"{orgUnit_B5310Id}_{role_B5310_DoA1_5418Id}_{user_B5310_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5310_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_DoA1_5418Id}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_DoA1_5418Id,
                        UserId = user_B5310_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5310_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5310_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5310_DoA2_1135Id))
            {
                var key_B5310_DoA2_1135 = $"{orgUnit_B5310Id}_{role_B5310_DoA2_1135Id}_{user_B5310_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5310_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_DoA2_1135Id}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_DoA2_1135Id,
                        UserId = user_B5310_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5310_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5310_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5310_DoA1_6789Id))
            {
                var key_B5310_DoA1_6789 = $"{orgUnit_B5310Id}_{role_B5310_DoA1_6789Id}_{user_B5310_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5310_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_DoA1_6789Id}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_DoA1_6789Id,
                        UserId = user_B5310_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5310_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5310_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5310_DoA3_8676Id))
            {
                var key_B5310_DoA3_8676 = $"{orgUnit_B5310Id}_{role_B5310_DoA3_8676Id}_{user_B5310_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5310_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_DoA3_8676Id}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_DoA3_8676Id,
                        UserId = user_B5310_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5310_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5310");
        }

        // B5311
        if (codeToOrgUnitId.TryGetValue("B5311", out var orgUnit_B5311Id))
        {
            // DoA1: venelinr@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5311_DoA1_330Id) &&
                emailToUserId.TryGetValue("venelinr@unops.org", out var user_B5311_DoA1_330Id))
            {
                var key_B5311_DoA1_330 = $"{orgUnit_B5311Id}_{role_B5311_DoA1_330Id}_{user_B5311_DoA1_330Id}";
                if (!existingRoleKeys.Contains(key_B5311_DoA1_330))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_DoA1_330Id}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_DoA1_330Id,
                        UserId = user_B5311_DoA1_330Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5311_DoA1_330); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("venelinr@unops.org")) missingUsers.Add("venelinr@unops.org");
            }
            // DoA1: alaa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5311_DoA1_5032Id) &&
                emailToUserId.TryGetValue("alaa@unops.org", out var user_B5311_DoA1_5032Id))
            {
                var key_B5311_DoA1_5032 = $"{orgUnit_B5311Id}_{role_B5311_DoA1_5032Id}_{user_B5311_DoA1_5032Id}";
                if (!existingRoleKeys.Contains(key_B5311_DoA1_5032))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_DoA1_5032Id}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_DoA1_5032Id,
                        UserId = user_B5311_DoA1_5032Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5311_DoA1_5032); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alaa@unops.org")) missingUsers.Add("alaa@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5311_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5311_DoA3_8676Id))
            {
                var key_B5311_DoA3_8676 = $"{orgUnit_B5311Id}_{role_B5311_DoA3_8676Id}_{user_B5311_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5311_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_DoA3_8676Id}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_DoA3_8676Id,
                        UserId = user_B5311_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5311_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // DoA2: alaan@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5311_DoA2_5939Id) &&
                emailToUserId.TryGetValue("alaan@unops.org", out var user_B5311_DoA2_5939Id))
            {
                var key_B5311_DoA2_5939 = $"{orgUnit_B5311Id}_{role_B5311_DoA2_5939Id}_{user_B5311_DoA2_5939Id}";
                if (!existingRoleKeys.Contains(key_B5311_DoA2_5939))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_DoA2_5939Id}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_DoA2_5939Id,
                        UserId = user_B5311_DoA2_5939Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5311_DoA2_5939); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("alaan@unops.org")) missingUsers.Add("alaan@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5311");
        }

        // B5312
        if (codeToOrgUnitId.TryGetValue("B5312", out var orgUnit_B5312Id))
        {
            // DoA1: sonjav@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5312_DoA1_4170Id) &&
                emailToUserId.TryGetValue("sonjav@unops.org", out var user_B5312_DoA1_4170Id))
            {
                var key_B5312_DoA1_4170 = $"{orgUnit_B5312Id}_{role_B5312_DoA1_4170Id}_{user_B5312_DoA1_4170Id}";
                if (!existingRoleKeys.Contains(key_B5312_DoA1_4170))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_DoA1_4170Id}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_DoA1_4170Id,
                        UserId = user_B5312_DoA1_4170Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5312_DoA1_4170); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sonjav@unops.org")) missingUsers.Add("sonjav@unops.org");
            }
            // DoA1: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5312_DoA1_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5312_DoA1_1631Id))
            {
                var key_B5312_DoA1_1631 = $"{orgUnit_B5312Id}_{role_B5312_DoA1_1631Id}_{user_B5312_DoA1_1631Id}";
                if (!existingRoleKeys.Contains(key_B5312_DoA1_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_DoA1_1631Id}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_DoA1_1631Id,
                        UserId = user_B5312_DoA1_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5312_DoA1_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA2: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5312_DoA2_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5312_DoA2_1631Id))
            {
                var key_B5312_DoA2_1631 = $"{orgUnit_B5312Id}_{role_B5312_DoA2_1631Id}_{user_B5312_DoA2_1631Id}";
                if (!existingRoleKeys.Contains(key_B5312_DoA2_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_DoA2_1631Id}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_DoA2_1631Id,
                        UserId = user_B5312_DoA2_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5312_DoA2_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5312_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5312_DoA3_8676Id))
            {
                var key_B5312_DoA3_8676 = $"{orgUnit_B5312Id}_{role_B5312_DoA3_8676Id}_{user_B5312_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5312_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_DoA3_8676Id}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_DoA3_8676Id,
                        UserId = user_B5312_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5312_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5312");
        }

        // B5313
        if (codeToOrgUnitId.TryGetValue("B5313", out var orgUnit_B5313Id))
        {
            // DoA2: tatianaw@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5313_DoA2_6288Id) &&
                emailToUserId.TryGetValue("tatianaw@unops.org", out var user_B5313_DoA2_6288Id))
            {
                var key_B5313_DoA2_6288 = $"{orgUnit_B5313Id}_{role_B5313_DoA2_6288Id}_{user_B5313_DoA2_6288Id}";
                if (!existingRoleKeys.Contains(key_B5313_DoA2_6288))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_DoA2_6288Id}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_DoA2_6288Id,
                        UserId = user_B5313_DoA2_6288Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5313_DoA2_6288); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tatianaw@unops.org")) missingUsers.Add("tatianaw@unops.org");
            }
            // DoA1: fredericfr@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5313_DoA1_4542Id) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5313_DoA1_4542Id))
            {
                var key_B5313_DoA1_4542 = $"{orgUnit_B5313Id}_{role_B5313_DoA1_4542Id}_{user_B5313_DoA1_4542Id}";
                if (!existingRoleKeys.Contains(key_B5313_DoA1_4542))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_DoA1_4542Id}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_DoA1_4542Id,
                        UserId = user_B5313_DoA1_4542Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5313_DoA1_4542); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5313_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5313_DoA2_8434Id))
            {
                var key_B5313_DoA2_8434 = $"{orgUnit_B5313Id}_{role_B5313_DoA2_8434Id}_{user_B5313_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5313_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_DoA2_8434Id}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_DoA2_8434Id,
                        UserId = user_B5313_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5313_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5313_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5313_DoA3_8676Id))
            {
                var key_B5313_DoA3_8676 = $"{orgUnit_B5313Id}_{role_B5313_DoA3_8676Id}_{user_B5313_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5313_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_DoA3_8676Id}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_DoA3_8676Id,
                        UserId = user_B5313_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5313_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5313");
        }

        // B5314
        if (codeToOrgUnitId.TryGetValue("B5314", out var orgUnit_B5314Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5314_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5314_DoA1_5418Id))
            {
                var key_B5314_DoA1_5418 = $"{orgUnit_B5314Id}_{role_B5314_DoA1_5418Id}_{user_B5314_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5314_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_DoA1_5418Id}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_DoA1_5418Id,
                        UserId = user_B5314_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5314_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5314_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5314_DoA2_1135Id))
            {
                var key_B5314_DoA2_1135 = $"{orgUnit_B5314Id}_{role_B5314_DoA2_1135Id}_{user_B5314_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5314_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_DoA2_1135Id}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_DoA2_1135Id,
                        UserId = user_B5314_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5314_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5314_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5314_DoA1_6789Id))
            {
                var key_B5314_DoA1_6789 = $"{orgUnit_B5314Id}_{role_B5314_DoA1_6789Id}_{user_B5314_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5314_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_DoA1_6789Id}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_DoA1_6789Id,
                        UserId = user_B5314_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5314_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5314_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5314_DoA3_8676Id))
            {
                var key_B5314_DoA3_8676 = $"{orgUnit_B5314Id}_{role_B5314_DoA3_8676Id}_{user_B5314_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5314_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_DoA3_8676Id}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_DoA3_8676Id,
                        UserId = user_B5314_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5314_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5314");
        }

        // B5315
        if (codeToOrgUnitId.TryGetValue("B5315", out var orgUnit_B5315Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5315_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5315_DoA1_5418Id))
            {
                var key_B5315_DoA1_5418 = $"{orgUnit_B5315Id}_{role_B5315_DoA1_5418Id}_{user_B5315_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5315_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_DoA1_5418Id}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_DoA1_5418Id,
                        UserId = user_B5315_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5315_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5315_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5315_DoA2_1135Id))
            {
                var key_B5315_DoA2_1135 = $"{orgUnit_B5315Id}_{role_B5315_DoA2_1135Id}_{user_B5315_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5315_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_DoA2_1135Id}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_DoA2_1135Id,
                        UserId = user_B5315_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5315_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5315_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5315_DoA1_6789Id))
            {
                var key_B5315_DoA1_6789 = $"{orgUnit_B5315Id}_{role_B5315_DoA1_6789Id}_{user_B5315_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5315_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_DoA1_6789Id}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_DoA1_6789Id,
                        UserId = user_B5315_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5315_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5315_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5315_DoA3_8676Id))
            {
                var key_B5315_DoA3_8676 = $"{orgUnit_B5315Id}_{role_B5315_DoA3_8676Id}_{user_B5315_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5315_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_DoA3_8676Id}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_DoA3_8676Id,
                        UserId = user_B5315_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5315_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5315");
        }

        // B5316
        if (codeToOrgUnitId.TryGetValue("B5316", out var orgUnit_B5316Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5316_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5316_DoA1_5418Id))
            {
                var key_B5316_DoA1_5418 = $"{orgUnit_B5316Id}_{role_B5316_DoA1_5418Id}_{user_B5316_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5316_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_DoA1_5418Id}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_DoA1_5418Id,
                        UserId = user_B5316_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5316_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5316_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5316_DoA2_1135Id))
            {
                var key_B5316_DoA2_1135 = $"{orgUnit_B5316Id}_{role_B5316_DoA2_1135Id}_{user_B5316_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5316_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_DoA2_1135Id}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_DoA2_1135Id,
                        UserId = user_B5316_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5316_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5316_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5316_DoA1_6789Id))
            {
                var key_B5316_DoA1_6789 = $"{orgUnit_B5316Id}_{role_B5316_DoA1_6789Id}_{user_B5316_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5316_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_DoA1_6789Id}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_DoA1_6789Id,
                        UserId = user_B5316_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5316_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5316_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5316_DoA3_8676Id))
            {
                var key_B5316_DoA3_8676 = $"{orgUnit_B5316Id}_{role_B5316_DoA3_8676Id}_{user_B5316_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5316_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_DoA3_8676Id}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_DoA3_8676Id,
                        UserId = user_B5316_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5316_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5316");
        }

        // B5317
        if (codeToOrgUnitId.TryGetValue("B5317", out var orgUnit_B5317Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5317_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5317_DoA1_5418Id))
            {
                var key_B5317_DoA1_5418 = $"{orgUnit_B5317Id}_{role_B5317_DoA1_5418Id}_{user_B5317_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5317_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_DoA1_5418Id}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_DoA1_5418Id,
                        UserId = user_B5317_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5317_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5317_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5317_DoA2_1135Id))
            {
                var key_B5317_DoA2_1135 = $"{orgUnit_B5317Id}_{role_B5317_DoA2_1135Id}_{user_B5317_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5317_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_DoA2_1135Id}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_DoA2_1135Id,
                        UserId = user_B5317_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5317_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5317_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5317_DoA1_6789Id))
            {
                var key_B5317_DoA1_6789 = $"{orgUnit_B5317Id}_{role_B5317_DoA1_6789Id}_{user_B5317_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5317_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_DoA1_6789Id}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_DoA1_6789Id,
                        UserId = user_B5317_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5317_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5317_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5317_DoA3_8676Id))
            {
                var key_B5317_DoA3_8676 = $"{orgUnit_B5317Id}_{role_B5317_DoA3_8676Id}_{user_B5317_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5317_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_DoA3_8676Id}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_DoA3_8676Id,
                        UserId = user_B5317_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5317_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5317");
        }

        // B5318
        if (codeToOrgUnitId.TryGetValue("B5318", out var orgUnit_B5318Id))
        {
            // DoA1: sonjav@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5318_DoA1_4170Id) &&
                emailToUserId.TryGetValue("sonjav@unops.org", out var user_B5318_DoA1_4170Id))
            {
                var key_B5318_DoA1_4170 = $"{orgUnit_B5318Id}_{role_B5318_DoA1_4170Id}_{user_B5318_DoA1_4170Id}";
                if (!existingRoleKeys.Contains(key_B5318_DoA1_4170))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_DoA1_4170Id}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_DoA1_4170Id,
                        UserId = user_B5318_DoA1_4170Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5318_DoA1_4170); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sonjav@unops.org")) missingUsers.Add("sonjav@unops.org");
            }
            // DoA1: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5318_DoA1_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5318_DoA1_1631Id))
            {
                var key_B5318_DoA1_1631 = $"{orgUnit_B5318Id}_{role_B5318_DoA1_1631Id}_{user_B5318_DoA1_1631Id}";
                if (!existingRoleKeys.Contains(key_B5318_DoA1_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_DoA1_1631Id}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_DoA1_1631Id,
                        UserId = user_B5318_DoA1_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5318_DoA1_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA2: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5318_DoA2_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5318_DoA2_1631Id))
            {
                var key_B5318_DoA2_1631 = $"{orgUnit_B5318Id}_{role_B5318_DoA2_1631Id}_{user_B5318_DoA2_1631Id}";
                if (!existingRoleKeys.Contains(key_B5318_DoA2_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_DoA2_1631Id}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_DoA2_1631Id,
                        UserId = user_B5318_DoA2_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5318_DoA2_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5318_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5318_DoA3_8676Id))
            {
                var key_B5318_DoA3_8676 = $"{orgUnit_B5318Id}_{role_B5318_DoA3_8676Id}_{user_B5318_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5318_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_DoA3_8676Id}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_DoA3_8676Id,
                        UserId = user_B5318_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5318_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5318");
        }

        // B5320
        if (codeToOrgUnitId.TryGetValue("B5320", out var orgUnit_B5320Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5320_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5320_DoA1_5418Id))
            {
                var key_B5320_DoA1_5418 = $"{orgUnit_B5320Id}_{role_B5320_DoA1_5418Id}_{user_B5320_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5320_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_DoA1_5418Id}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_DoA1_5418Id,
                        UserId = user_B5320_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5320_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5320_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5320_DoA2_1135Id))
            {
                var key_B5320_DoA2_1135 = $"{orgUnit_B5320Id}_{role_B5320_DoA2_1135Id}_{user_B5320_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5320_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_DoA2_1135Id}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_DoA2_1135Id,
                        UserId = user_B5320_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5320_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5320_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5320_DoA1_6789Id))
            {
                var key_B5320_DoA1_6789 = $"{orgUnit_B5320Id}_{role_B5320_DoA1_6789Id}_{user_B5320_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5320_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_DoA1_6789Id}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_DoA1_6789Id,
                        UserId = user_B5320_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5320_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5320_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5320_DoA3_8676Id))
            {
                var key_B5320_DoA3_8676 = $"{orgUnit_B5320Id}_{role_B5320_DoA3_8676Id}_{user_B5320_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5320_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_DoA3_8676Id}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_DoA3_8676Id,
                        UserId = user_B5320_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5320_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5320");
        }

        // B5321
        if (codeToOrgUnitId.TryGetValue("B5321", out var orgUnit_B5321Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5321_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5321_DoA1_5418Id))
            {
                var key_B5321_DoA1_5418 = $"{orgUnit_B5321Id}_{role_B5321_DoA1_5418Id}_{user_B5321_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5321_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_DoA1_5418Id}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_DoA1_5418Id,
                        UserId = user_B5321_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5321_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5321_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5321_DoA2_1135Id))
            {
                var key_B5321_DoA2_1135 = $"{orgUnit_B5321Id}_{role_B5321_DoA2_1135Id}_{user_B5321_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5321_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_DoA2_1135Id}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_DoA2_1135Id,
                        UserId = user_B5321_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5321_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5321_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5321_DoA1_6789Id))
            {
                var key_B5321_DoA1_6789 = $"{orgUnit_B5321Id}_{role_B5321_DoA1_6789Id}_{user_B5321_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5321_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_DoA1_6789Id}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_DoA1_6789Id,
                        UserId = user_B5321_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5321_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5321_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5321_DoA3_8676Id))
            {
                var key_B5321_DoA3_8676 = $"{orgUnit_B5321Id}_{role_B5321_DoA3_8676Id}_{user_B5321_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5321_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_DoA3_8676Id}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_DoA3_8676Id,
                        UserId = user_B5321_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5321_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5321");
        }

        // B5322
        if (codeToOrgUnitId.TryGetValue("B5322", out var orgUnit_B5322Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5322_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5322_DoA1_5418Id))
            {
                var key_B5322_DoA1_5418 = $"{orgUnit_B5322Id}_{role_B5322_DoA1_5418Id}_{user_B5322_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5322_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_DoA1_5418Id}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_DoA1_5418Id,
                        UserId = user_B5322_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5322_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5322_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5322_DoA2_1135Id))
            {
                var key_B5322_DoA2_1135 = $"{orgUnit_B5322Id}_{role_B5322_DoA2_1135Id}_{user_B5322_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5322_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_DoA2_1135Id}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_DoA2_1135Id,
                        UserId = user_B5322_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5322_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5322_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5322_DoA1_6789Id))
            {
                var key_B5322_DoA1_6789 = $"{orgUnit_B5322Id}_{role_B5322_DoA1_6789Id}_{user_B5322_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5322_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_DoA1_6789Id}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_DoA1_6789Id,
                        UserId = user_B5322_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5322_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5322_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5322_DoA3_8676Id))
            {
                var key_B5322_DoA3_8676 = $"{orgUnit_B5322Id}_{role_B5322_DoA3_8676Id}_{user_B5322_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5322_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_DoA3_8676Id}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_DoA3_8676Id,
                        UserId = user_B5322_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5322_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5322");
        }

        // B5323
        if (codeToOrgUnitId.TryGetValue("B5323", out var orgUnit_B5323Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5323_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5323_DoA1_1570Id))
            {
                var key_B5323_DoA1_1570 = $"{orgUnit_B5323Id}_{role_B5323_DoA1_1570Id}_{user_B5323_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5323_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5323Id} - {user_B5323_DoA1_1570Id}",
                        EntityId = orgUnit_B5323Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5323_DoA1_1570Id,
                        UserId = user_B5323_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5323_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5323_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5323_DoA2_195Id))
            {
                var key_B5323_DoA2_195 = $"{orgUnit_B5323Id}_{role_B5323_DoA2_195Id}_{user_B5323_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5323_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5323Id} - {user_B5323_DoA2_195Id}",
                        EntityId = orgUnit_B5323Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5323_DoA2_195Id,
                        UserId = user_B5323_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5323_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5323_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5323_DoA3_8676Id))
            {
                var key_B5323_DoA3_8676 = $"{orgUnit_B5323Id}_{role_B5323_DoA3_8676Id}_{user_B5323_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5323_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5323Id} - {user_B5323_DoA3_8676Id}",
                        EntityId = orgUnit_B5323Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5323_DoA3_8676Id,
                        UserId = user_B5323_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5323_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5323");
        }

        // B5324
        if (codeToOrgUnitId.TryGetValue("B5324", out var orgUnit_B5324Id))
        {
            // DoA1: boureimat@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5324_DoA1_8437Id) &&
                emailToUserId.TryGetValue("boureimat@unops.org", out var user_B5324_DoA1_8437Id))
            {
                var key_B5324_DoA1_8437 = $"{orgUnit_B5324Id}_{role_B5324_DoA1_8437Id}_{user_B5324_DoA1_8437Id}";
                if (!existingRoleKeys.Contains(key_B5324_DoA1_8437))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_DoA1_8437Id}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_DoA1_8437Id,
                        UserId = user_B5324_DoA1_8437Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5324_DoA1_8437); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("boureimat@unops.org")) missingUsers.Add("boureimat@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5324_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5324_DoA2_8434Id))
            {
                var key_B5324_DoA2_8434 = $"{orgUnit_B5324Id}_{role_B5324_DoA2_8434Id}_{user_B5324_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5324_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_DoA2_8434Id}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_DoA2_8434Id,
                        UserId = user_B5324_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5324_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5324_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5324_DoA3_8676Id))
            {
                var key_B5324_DoA3_8676 = $"{orgUnit_B5324Id}_{role_B5324_DoA3_8676Id}_{user_B5324_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5324_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_DoA3_8676Id}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_DoA3_8676Id,
                        UserId = user_B5324_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5324_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5324");
        }

        // B5325
        if (codeToOrgUnitId.TryGetValue("B5325", out var orgUnit_B5325Id))
        {
            // DoA1: boureimat@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5325_DoA1_8437Id) &&
                emailToUserId.TryGetValue("boureimat@unops.org", out var user_B5325_DoA1_8437Id))
            {
                var key_B5325_DoA1_8437 = $"{orgUnit_B5325Id}_{role_B5325_DoA1_8437Id}_{user_B5325_DoA1_8437Id}";
                if (!existingRoleKeys.Contains(key_B5325_DoA1_8437))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_DoA1_8437Id}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_DoA1_8437Id,
                        UserId = user_B5325_DoA1_8437Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5325_DoA1_8437); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("boureimat@unops.org")) missingUsers.Add("boureimat@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5325_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5325_DoA2_8434Id))
            {
                var key_B5325_DoA2_8434 = $"{orgUnit_B5325Id}_{role_B5325_DoA2_8434Id}_{user_B5325_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5325_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_DoA2_8434Id}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_DoA2_8434Id,
                        UserId = user_B5325_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5325_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5325_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5325_DoA3_8676Id))
            {
                var key_B5325_DoA3_8676 = $"{orgUnit_B5325Id}_{role_B5325_DoA3_8676Id}_{user_B5325_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5325_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_DoA3_8676Id}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_DoA3_8676Id,
                        UserId = user_B5325_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5325_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5325");
        }

        // B5326
        if (codeToOrgUnitId.TryGetValue("B5326", out var orgUnit_B5326Id))
        {
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5326_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5326_DoA3_8676Id))
            {
                var key_B5326_DoA3_8676 = $"{orgUnit_B5326Id}_{role_B5326_DoA3_8676Id}_{user_B5326_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5326_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5326Id} - {user_B5326_DoA3_8676Id}",
                        EntityId = orgUnit_B5326Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5326_DoA3_8676Id,
                        UserId = user_B5326_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5326_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5326");
        }

        // B5327
        if (codeToOrgUnitId.TryGetValue("B5327", out var orgUnit_B5327Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5327_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5327_DoA1_1570Id))
            {
                var key_B5327_DoA1_1570 = $"{orgUnit_B5327Id}_{role_B5327_DoA1_1570Id}_{user_B5327_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5327_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_DoA1_1570Id}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_DoA1_1570Id,
                        UserId = user_B5327_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5327_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5327_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5327_DoA2_195Id))
            {
                var key_B5327_DoA2_195 = $"{orgUnit_B5327Id}_{role_B5327_DoA2_195Id}_{user_B5327_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5327_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_DoA2_195Id}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_DoA2_195Id,
                        UserId = user_B5327_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5327_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5327_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5327_DoA3_8676Id))
            {
                var key_B5327_DoA3_8676 = $"{orgUnit_B5327Id}_{role_B5327_DoA3_8676Id}_{user_B5327_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5327_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_DoA3_8676Id}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_DoA3_8676Id,
                        UserId = user_B5327_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5327_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5327");
        }

        // B5328
        if (codeToOrgUnitId.TryGetValue("B5328", out var orgUnit_B5328Id))
        {
            // DoA2: tatianaw@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5328_DoA2_6288Id) &&
                emailToUserId.TryGetValue("tatianaw@unops.org", out var user_B5328_DoA2_6288Id))
            {
                var key_B5328_DoA2_6288 = $"{orgUnit_B5328Id}_{role_B5328_DoA2_6288Id}_{user_B5328_DoA2_6288Id}";
                if (!existingRoleKeys.Contains(key_B5328_DoA2_6288))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_DoA2_6288Id}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_DoA2_6288Id,
                        UserId = user_B5328_DoA2_6288Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5328_DoA2_6288); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("tatianaw@unops.org")) missingUsers.Add("tatianaw@unops.org");
            }
            // DoA1: fredericfr@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5328_DoA1_4542Id) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5328_DoA1_4542Id))
            {
                var key_B5328_DoA1_4542 = $"{orgUnit_B5328Id}_{role_B5328_DoA1_4542Id}_{user_B5328_DoA1_4542Id}";
                if (!existingRoleKeys.Contains(key_B5328_DoA1_4542))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_DoA1_4542Id}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_DoA1_4542Id,
                        UserId = user_B5328_DoA1_4542Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5328_DoA1_4542); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5328_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5328_DoA2_8434Id))
            {
                var key_B5328_DoA2_8434 = $"{orgUnit_B5328Id}_{role_B5328_DoA2_8434Id}_{user_B5328_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5328_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_DoA2_8434Id}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_DoA2_8434Id,
                        UserId = user_B5328_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5328_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5328_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5328_DoA3_8676Id))
            {
                var key_B5328_DoA3_8676 = $"{orgUnit_B5328Id}_{role_B5328_DoA3_8676Id}_{user_B5328_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5328_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_DoA3_8676Id}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_DoA3_8676Id,
                        UserId = user_B5328_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5328_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5328");
        }

        // B5329
        if (codeToOrgUnitId.TryGetValue("B5329", out var orgUnit_B5329Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5329_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5329_DoA1_1570Id))
            {
                var key_B5329_DoA1_1570 = $"{orgUnit_B5329Id}_{role_B5329_DoA1_1570Id}_{user_B5329_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5329_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_DoA1_1570Id}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_DoA1_1570Id,
                        UserId = user_B5329_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5329_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5329_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5329_DoA2_195Id))
            {
                var key_B5329_DoA2_195 = $"{orgUnit_B5329Id}_{role_B5329_DoA2_195Id}_{user_B5329_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5329_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_DoA2_195Id}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_DoA2_195Id,
                        UserId = user_B5329_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5329_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5329_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5329_DoA3_8676Id))
            {
                var key_B5329_DoA3_8676 = $"{orgUnit_B5329Id}_{role_B5329_DoA3_8676Id}_{user_B5329_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5329_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_DoA3_8676Id}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_DoA3_8676Id,
                        UserId = user_B5329_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5329_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5329");
        }

        // B5330
        if (codeToOrgUnitId.TryGetValue("B5330", out var orgUnit_B5330Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5330_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5330_DoA1_5418Id))
            {
                var key_B5330_DoA1_5418 = $"{orgUnit_B5330Id}_{role_B5330_DoA1_5418Id}_{user_B5330_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5330_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_DoA1_5418Id}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_DoA1_5418Id,
                        UserId = user_B5330_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5330_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5330_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5330_DoA2_1135Id))
            {
                var key_B5330_DoA2_1135 = $"{orgUnit_B5330Id}_{role_B5330_DoA2_1135Id}_{user_B5330_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5330_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_DoA2_1135Id}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_DoA2_1135Id,
                        UserId = user_B5330_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5330_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5330_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5330_DoA1_6789Id))
            {
                var key_B5330_DoA1_6789 = $"{orgUnit_B5330Id}_{role_B5330_DoA1_6789Id}_{user_B5330_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5330_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_DoA1_6789Id}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_DoA1_6789Id,
                        UserId = user_B5330_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5330_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5330_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5330_DoA3_8676Id))
            {
                var key_B5330_DoA3_8676 = $"{orgUnit_B5330Id}_{role_B5330_DoA3_8676Id}_{user_B5330_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5330_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_DoA3_8676Id}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_DoA3_8676Id,
                        UserId = user_B5330_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5330_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5330");
        }

        // B5331
        if (codeToOrgUnitId.TryGetValue("B5331", out var orgUnit_B5331Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5331_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5331_DoA1_1570Id))
            {
                var key_B5331_DoA1_1570 = $"{orgUnit_B5331Id}_{role_B5331_DoA1_1570Id}_{user_B5331_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5331_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5331Id} - {user_B5331_DoA1_1570Id}",
                        EntityId = orgUnit_B5331Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5331_DoA1_1570Id,
                        UserId = user_B5331_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5331_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5331_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5331_DoA2_195Id))
            {
                var key_B5331_DoA2_195 = $"{orgUnit_B5331Id}_{role_B5331_DoA2_195Id}_{user_B5331_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5331_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5331Id} - {user_B5331_DoA2_195Id}",
                        EntityId = orgUnit_B5331Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5331_DoA2_195Id,
                        UserId = user_B5331_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5331_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5331_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5331_DoA3_8676Id))
            {
                var key_B5331_DoA3_8676 = $"{orgUnit_B5331Id}_{role_B5331_DoA3_8676Id}_{user_B5331_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5331_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5331Id} - {user_B5331_DoA3_8676Id}",
                        EntityId = orgUnit_B5331Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5331_DoA3_8676Id,
                        UserId = user_B5331_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5331_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5331");
        }

        // B5332
        if (codeToOrgUnitId.TryGetValue("B5332", out var orgUnit_B5332Id))
        {
            // DoA1: alimatour@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5332_DoA1_5418Id) &&
                emailToUserId.TryGetValue("alimatour@unops.org", out var user_B5332_DoA1_5418Id))
            {
                var key_B5332_DoA1_5418 = $"{orgUnit_B5332Id}_{role_B5332_DoA1_5418Id}_{user_B5332_DoA1_5418Id}";
                if (!existingRoleKeys.Contains(key_B5332_DoA1_5418))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_DoA1_5418Id}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_DoA1_5418Id,
                        UserId = user_B5332_DoA1_5418Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5332_DoA1_5418); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("alimatour@unops.org")) missingUsers.Add("alimatour@unops.org");
            }
            // DoA2: mariasg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5332_DoA2_1135Id) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5332_DoA2_1135Id))
            {
                var key_B5332_DoA2_1135 = $"{orgUnit_B5332Id}_{role_B5332_DoA2_1135Id}_{user_B5332_DoA2_1135Id}";
                if (!existingRoleKeys.Contains(key_B5332_DoA2_1135))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_DoA2_1135Id}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_DoA2_1135Id,
                        UserId = user_B5332_DoA2_1135Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5332_DoA2_1135); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // DoA1: george@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5332_DoA1_6789Id) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5332_DoA1_6789Id))
            {
                var key_B5332_DoA1_6789 = $"{orgUnit_B5332Id}_{role_B5332_DoA1_6789Id}_{user_B5332_DoA1_6789Id}";
                if (!existingRoleKeys.Contains(key_B5332_DoA1_6789))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_DoA1_6789Id}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_DoA1_6789Id,
                        UserId = user_B5332_DoA1_6789Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5332_DoA1_6789); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5332_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5332_DoA3_8676Id))
            {
                var key_B5332_DoA3_8676 = $"{orgUnit_B5332Id}_{role_B5332_DoA3_8676Id}_{user_B5332_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5332_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_DoA3_8676Id}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_DoA3_8676Id,
                        UserId = user_B5332_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5332_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5332");
        }

        // B5333
        if (codeToOrgUnitId.TryGetValue("B5333", out var orgUnit_B5333Id))
        {
            // DoA1: sonjav@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5333_DoA1_4170Id) &&
                emailToUserId.TryGetValue("sonjav@unops.org", out var user_B5333_DoA1_4170Id))
            {
                var key_B5333_DoA1_4170 = $"{orgUnit_B5333Id}_{role_B5333_DoA1_4170Id}_{user_B5333_DoA1_4170Id}";
                if (!existingRoleKeys.Contains(key_B5333_DoA1_4170))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_DoA1_4170Id}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_DoA1_4170Id,
                        UserId = user_B5333_DoA1_4170Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5333_DoA1_4170); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sonjav@unops.org")) missingUsers.Add("sonjav@unops.org");
            }
            // DoA1: irenek@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5333_DoA1_1631Id) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5333_DoA1_1631Id))
            {
                var key_B5333_DoA1_1631 = $"{orgUnit_B5333Id}_{role_B5333_DoA1_1631Id}_{user_B5333_DoA1_1631Id}";
                if (!existingRoleKeys.Contains(key_B5333_DoA1_1631))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_DoA1_1631Id}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_DoA1_1631Id,
                        UserId = user_B5333_DoA1_1631Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5333_DoA1_1631); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5333_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5333_DoA3_8676Id))
            {
                var key_B5333_DoA3_8676 = $"{orgUnit_B5333Id}_{role_B5333_DoA3_8676Id}_{user_B5333_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5333_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_DoA3_8676Id}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_DoA3_8676Id,
                        UserId = user_B5333_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5333_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5333");
        }

        // B5334
        if (codeToOrgUnitId.TryGetValue("B5334", out var orgUnit_B5334Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5334_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5334_DoA1_1570Id))
            {
                var key_B5334_DoA1_1570 = $"{orgUnit_B5334Id}_{role_B5334_DoA1_1570Id}_{user_B5334_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5334_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5334Id} - {user_B5334_DoA1_1570Id}",
                        EntityId = orgUnit_B5334Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5334_DoA1_1570Id,
                        UserId = user_B5334_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5334_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5334_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5334_DoA2_195Id))
            {
                var key_B5334_DoA2_195 = $"{orgUnit_B5334Id}_{role_B5334_DoA2_195Id}_{user_B5334_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5334_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5334Id} - {user_B5334_DoA2_195Id}",
                        EntityId = orgUnit_B5334Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5334_DoA2_195Id,
                        UserId = user_B5334_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5334_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5334_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5334_DoA3_8676Id))
            {
                var key_B5334_DoA3_8676 = $"{orgUnit_B5334Id}_{role_B5334_DoA3_8676Id}_{user_B5334_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5334_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5334Id} - {user_B5334_DoA3_8676Id}",
                        EntityId = orgUnit_B5334Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5334_DoA3_8676Id,
                        UserId = user_B5334_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5334_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5334");
        }

        // B5335
        if (codeToOrgUnitId.TryGetValue("B5335", out var orgUnit_B5335Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5335_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5335_DoA1_1570Id))
            {
                var key_B5335_DoA1_1570 = $"{orgUnit_B5335Id}_{role_B5335_DoA1_1570Id}_{user_B5335_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5335_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_DoA1_1570Id}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_DoA1_1570Id,
                        UserId = user_B5335_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5335_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5335_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5335_DoA2_195Id))
            {
                var key_B5335_DoA2_195 = $"{orgUnit_B5335Id}_{role_B5335_DoA2_195Id}_{user_B5335_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5335_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_DoA2_195Id}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_DoA2_195Id,
                        UserId = user_B5335_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5335_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5335_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5335_DoA3_8676Id))
            {
                var key_B5335_DoA3_8676 = $"{orgUnit_B5335Id}_{role_B5335_DoA3_8676Id}_{user_B5335_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5335_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_DoA3_8676Id}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_DoA3_8676Id,
                        UserId = user_B5335_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5335_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5335");
        }

        // B5336
        if (codeToOrgUnitId.TryGetValue("B5336", out var orgUnit_B5336Id))
        {
            // DoA1: sharonle@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5336_DoA1_1570Id) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5336_DoA1_1570Id))
            {
                var key_B5336_DoA1_1570 = $"{orgUnit_B5336Id}_{role_B5336_DoA1_1570Id}_{user_B5336_DoA1_1570Id}";
                if (!existingRoleKeys.Contains(key_B5336_DoA1_1570))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5336Id} - {user_B5336_DoA1_1570Id}",
                        EntityId = orgUnit_B5336Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5336_DoA1_1570Id,
                        UserId = user_B5336_DoA1_1570Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5336_DoA1_1570); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
            // DoA2: rainerf@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5336_DoA2_195Id) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5336_DoA2_195Id))
            {
                var key_B5336_DoA2_195 = $"{orgUnit_B5336Id}_{role_B5336_DoA2_195Id}_{user_B5336_DoA2_195Id}";
                if (!existingRoleKeys.Contains(key_B5336_DoA2_195))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5336Id} - {user_B5336_DoA2_195Id}",
                        EntityId = orgUnit_B5336Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5336_DoA2_195Id,
                        UserId = user_B5336_DoA2_195Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5336_DoA2_195); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5336_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5336_DoA3_8676Id))
            {
                var key_B5336_DoA3_8676 = $"{orgUnit_B5336Id}_{role_B5336_DoA3_8676Id}_{user_B5336_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5336_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5336Id} - {user_B5336_DoA3_8676Id}",
                        EntityId = orgUnit_B5336Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5336_DoA3_8676Id,
                        UserId = user_B5336_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5336_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5336");
        }

        // B5337
        if (codeToOrgUnitId.TryGetValue("B5337", out var orgUnit_B5337Id))
        {
            // DoA1: boureimat@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5337_DoA1_8437Id) &&
                emailToUserId.TryGetValue("boureimat@unops.org", out var user_B5337_DoA1_8437Id))
            {
                var key_B5337_DoA1_8437 = $"{orgUnit_B5337Id}_{role_B5337_DoA1_8437Id}_{user_B5337_DoA1_8437Id}";
                if (!existingRoleKeys.Contains(key_B5337_DoA1_8437))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_DoA1_8437Id}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_DoA1_8437Id,
                        UserId = user_B5337_DoA1_8437Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5337_DoA1_8437); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("boureimat@unops.org")) missingUsers.Add("boureimat@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5337_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5337_DoA2_8434Id))
            {
                var key_B5337_DoA2_8434 = $"{orgUnit_B5337Id}_{role_B5337_DoA2_8434Id}_{user_B5337_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5337_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_DoA2_8434Id}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_DoA2_8434Id,
                        UserId = user_B5337_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5337_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5337_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5337_DoA3_8676Id))
            {
                var key_B5337_DoA3_8676 = $"{orgUnit_B5337Id}_{role_B5337_DoA3_8676Id}_{user_B5337_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5337_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_DoA3_8676Id}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_DoA3_8676Id,
                        UserId = user_B5337_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5337_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5337");
        }

        // B5338
        if (codeToOrgUnitId.TryGetValue("B5338", out var orgUnit_B5338Id))
        {
            // DoA1: fredericfr@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5338_DoA1_4542Id) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5338_DoA1_4542Id))
            {
                var key_B5338_DoA1_4542 = $"{orgUnit_B5338Id}_{role_B5338_DoA1_4542Id}_{user_B5338_DoA1_4542Id}";
                if (!existingRoleKeys.Contains(key_B5338_DoA1_4542))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_DoA1_4542Id}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_DoA1_4542Id,
                        UserId = user_B5338_DoA1_4542Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5338_DoA1_4542); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // DoA2: nathaliea@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5338_DoA2_8434Id) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5338_DoA2_8434Id))
            {
                var key_B5338_DoA2_8434 = $"{orgUnit_B5338Id}_{role_B5338_DoA2_8434Id}_{user_B5338_DoA2_8434Id}";
                if (!existingRoleKeys.Contains(key_B5338_DoA2_8434))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_DoA2_8434Id}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_DoA2_8434Id,
                        UserId = user_B5338_DoA2_8434Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5338_DoA2_8434); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // DoA3: workneshg@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5338_DoA3_8676Id) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5338_DoA3_8676Id))
            {
                var key_B5338_DoA3_8676 = $"{orgUnit_B5338Id}_{role_B5338_DoA3_8676Id}_{user_B5338_DoA3_8676Id}";
                if (!existingRoleKeys.Contains(key_B5338_DoA3_8676))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_DoA3_8676Id}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_DoA3_8676Id,
                        UserId = user_B5338_DoA3_8676Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5338_DoA3_8676); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5338");
        }

        // B5401
        if (codeToOrgUnitId.TryGetValue("B5401", out var orgUnit_B5401Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5401_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5401_DoA3_120Id))
            {
                var key_B5401_DoA3_120 = $"{orgUnit_B5401Id}_{role_B5401_DoA3_120Id}_{user_B5401_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5401_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_DoA3_120Id}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_DoA3_120Id,
                        UserId = user_B5401_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5401_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5401_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5401_DoA3_8818Id))
            {
                var key_B5401_DoA3_8818 = $"{orgUnit_B5401Id}_{role_B5401_DoA3_8818Id}_{user_B5401_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5401_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_DoA3_8818Id}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_DoA3_8818Id,
                        UserId = user_B5401_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5401_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: fernandoc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5401_DoA2_4689Id) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5401_DoA2_4689Id))
            {
                var key_B5401_DoA2_4689 = $"{orgUnit_B5401Id}_{role_B5401_DoA2_4689Id}_{user_B5401_DoA2_4689Id}";
                if (!existingRoleKeys.Contains(key_B5401_DoA2_4689))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_DoA2_4689Id}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_DoA2_4689Id,
                        UserId = user_B5401_DoA2_4689Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5401_DoA2_4689); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5401_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5401_DoA3_9564Id))
            {
                var key_B5401_DoA3_9564 = $"{orgUnit_B5401Id}_{role_B5401_DoA3_9564Id}_{user_B5401_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5401_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_DoA3_9564Id}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_DoA3_9564Id,
                        UserId = user_B5401_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5401_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5401");
        }

        // B5405
        if (codeToOrgUnitId.TryGetValue("B5405", out var orgUnit_B5405Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5405_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5405_DoA3_120Id))
            {
                var key_B5405_DoA3_120 = $"{orgUnit_B5405Id}_{role_B5405_DoA3_120Id}_{user_B5405_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5405_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_DoA3_120Id}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_DoA3_120Id,
                        UserId = user_B5405_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5405_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA1: andreaca@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5405_DoA1_4996Id) &&
                emailToUserId.TryGetValue("andreaca@unops.org", out var user_B5405_DoA1_4996Id))
            {
                var key_B5405_DoA1_4996 = $"{orgUnit_B5405Id}_{role_B5405_DoA1_4996Id}_{user_B5405_DoA1_4996Id}";
                if (!existingRoleKeys.Contains(key_B5405_DoA1_4996))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_DoA1_4996Id}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_DoA1_4996Id,
                        UserId = user_B5405_DoA1_4996Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5405_DoA1_4996); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("andreaca@unops.org")) missingUsers.Add("andreaca@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5405_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5405_DoA3_8818Id))
            {
                var key_B5405_DoA3_8818 = $"{orgUnit_B5405Id}_{role_B5405_DoA3_8818Id}_{user_B5405_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5405_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_DoA3_8818Id}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_DoA3_8818Id,
                        UserId = user_B5405_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5405_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: andreaca@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5405_DoA2_4996Id) &&
                emailToUserId.TryGetValue("andreaca@unops.org", out var user_B5405_DoA2_4996Id))
            {
                var key_B5405_DoA2_4996 = $"{orgUnit_B5405Id}_{role_B5405_DoA2_4996Id}_{user_B5405_DoA2_4996Id}";
                if (!existingRoleKeys.Contains(key_B5405_DoA2_4996))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_DoA2_4996Id}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_DoA2_4996Id,
                        UserId = user_B5405_DoA2_4996Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5405_DoA2_4996); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("andreaca@unops.org")) missingUsers.Add("andreaca@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5405_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5405_DoA3_9564Id))
            {
                var key_B5405_DoA3_9564 = $"{orgUnit_B5405Id}_{role_B5405_DoA3_9564Id}_{user_B5405_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5405_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_DoA3_9564Id}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_DoA3_9564Id,
                        UserId = user_B5405_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5405_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5405");
        }

        // B5406
        if (codeToOrgUnitId.TryGetValue("B5406", out var orgUnit_B5406Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5406_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5406_DoA3_120Id))
            {
                var key_B5406_DoA3_120 = $"{orgUnit_B5406Id}_{role_B5406_DoA3_120Id}_{user_B5406_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5406_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_DoA3_120Id}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_DoA3_120Id,
                        UserId = user_B5406_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5406_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA2: dabagaid@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5406_DoA2_6588Id) &&
                emailToUserId.TryGetValue("dabagaid@unops.org", out var user_B5406_DoA2_6588Id))
            {
                var key_B5406_DoA2_6588 = $"{orgUnit_B5406Id}_{role_B5406_DoA2_6588Id}_{user_B5406_DoA2_6588Id}";
                if (!existingRoleKeys.Contains(key_B5406_DoA2_6588))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_DoA2_6588Id}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_DoA2_6588Id,
                        UserId = user_B5406_DoA2_6588Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5406_DoA2_6588); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("dabagaid@unops.org")) missingUsers.Add("dabagaid@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5406_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5406_DoA3_8818Id))
            {
                var key_B5406_DoA3_8818 = $"{orgUnit_B5406Id}_{role_B5406_DoA3_8818Id}_{user_B5406_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5406_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_DoA3_8818Id}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_DoA3_8818Id,
                        UserId = user_B5406_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5406_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA1: sorayaf@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5406_DoA1_3673Id) &&
                emailToUserId.TryGetValue("sorayaf@unops.org", out var user_B5406_DoA1_3673Id))
            {
                var key_B5406_DoA1_3673 = $"{orgUnit_B5406Id}_{role_B5406_DoA1_3673Id}_{user_B5406_DoA1_3673Id}";
                if (!existingRoleKeys.Contains(key_B5406_DoA1_3673))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_DoA1_3673Id}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_DoA1_3673Id,
                        UserId = user_B5406_DoA1_3673Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5406_DoA1_3673); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sorayaf@unops.org")) missingUsers.Add("sorayaf@unops.org");
            }
            // DoA1: nubarg@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5406_DoA1_1953Id) &&
                emailToUserId.TryGetValue("nubarg@unops.org", out var user_B5406_DoA1_1953Id))
            {
                var key_B5406_DoA1_1953 = $"{orgUnit_B5406Id}_{role_B5406_DoA1_1953Id}_{user_B5406_DoA1_1953Id}";
                if (!existingRoleKeys.Contains(key_B5406_DoA1_1953))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_DoA1_1953Id}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_DoA1_1953Id,
                        UserId = user_B5406_DoA1_1953Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5406_DoA1_1953); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("nubarg@unops.org")) missingUsers.Add("nubarg@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5406_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5406_DoA3_9564Id))
            {
                var key_B5406_DoA3_9564 = $"{orgUnit_B5406Id}_{role_B5406_DoA3_9564Id}_{user_B5406_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5406_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_DoA3_9564Id}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_DoA3_9564Id,
                        UserId = user_B5406_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5406_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5406");
        }

        // B5407
        if (codeToOrgUnitId.TryGetValue("B5407", out var orgUnit_B5407Id))
        {
            // DoA1: espositon@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5407_DoA1_9833Id) &&
                emailToUserId.TryGetValue("espositon@unops.org", out var user_B5407_DoA1_9833Id))
            {
                var key_B5407_DoA1_9833 = $"{orgUnit_B5407Id}_{role_B5407_DoA1_9833Id}_{user_B5407_DoA1_9833Id}";
                if (!existingRoleKeys.Contains(key_B5407_DoA1_9833))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_DoA1_9833Id}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_DoA1_9833Id,
                        UserId = user_B5407_DoA1_9833Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5407_DoA1_9833); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("espositon@unops.org")) missingUsers.Add("espositon@unops.org");
            }
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5407_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5407_DoA3_120Id))
            {
                var key_B5407_DoA3_120 = $"{orgUnit_B5407Id}_{role_B5407_DoA3_120Id}_{user_B5407_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5407_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_DoA3_120Id}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_DoA3_120Id,
                        UserId = user_B5407_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5407_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5407_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5407_DoA3_8818Id))
            {
                var key_B5407_DoA3_8818 = $"{orgUnit_B5407Id}_{role_B5407_DoA3_8818Id}_{user_B5407_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5407_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_DoA3_8818Id}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_DoA3_8818Id,
                        UserId = user_B5407_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5407_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5407_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5407_DoA3_9564Id))
            {
                var key_B5407_DoA3_9564 = $"{orgUnit_B5407Id}_{role_B5407_DoA3_9564Id}_{user_B5407_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5407_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_DoA3_9564Id}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_DoA3_9564Id,
                        UserId = user_B5407_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5407_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // DoA1: leyres@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5407_DoA1_1349Id) &&
                emailToUserId.TryGetValue("leyres@unops.org", out var user_B5407_DoA1_1349Id))
            {
                var key_B5407_DoA1_1349 = $"{orgUnit_B5407Id}_{role_B5407_DoA1_1349Id}_{user_B5407_DoA1_1349Id}";
                if (!existingRoleKeys.Contains(key_B5407_DoA1_1349))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_DoA1_1349Id}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_DoA1_1349Id,
                        UserId = user_B5407_DoA1_1349Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5407_DoA1_1349); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("leyres@unops.org")) missingUsers.Add("leyres@unops.org");
            }
            // DoA2: leyres@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5407_DoA2_1349Id) &&
                emailToUserId.TryGetValue("leyres@unops.org", out var user_B5407_DoA2_1349Id))
            {
                var key_B5407_DoA2_1349 = $"{orgUnit_B5407Id}_{role_B5407_DoA2_1349Id}_{user_B5407_DoA2_1349Id}";
                if (!existingRoleKeys.Contains(key_B5407_DoA2_1349))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_DoA2_1349Id}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_DoA2_1349Id,
                        UserId = user_B5407_DoA2_1349Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5407_DoA2_1349); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("leyres@unops.org")) missingUsers.Add("leyres@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5407");
        }

        // B5408
        if (codeToOrgUnitId.TryGetValue("B5408", out var orgUnit_B5408Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5408_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5408_DoA3_120Id))
            {
                var key_B5408_DoA3_120 = $"{orgUnit_B5408Id}_{role_B5408_DoA3_120Id}_{user_B5408_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA3_120Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA3_120Id,
                        UserId = user_B5408_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA2: alexandrak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5408_DoA2_3729Id) &&
                emailToUserId.TryGetValue("alexandrak@unops.org", out var user_B5408_DoA2_3729Id))
            {
                var key_B5408_DoA2_3729 = $"{orgUnit_B5408Id}_{role_B5408_DoA2_3729Id}_{user_B5408_DoA2_3729Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA2_3729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA2_3729Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA2_3729Id,
                        UserId = user_B5408_DoA2_3729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA2_3729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("alexandrak@unops.org")) missingUsers.Add("alexandrak@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5408_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5408_DoA3_8818Id))
            {
                var key_B5408_DoA3_8818 = $"{orgUnit_B5408Id}_{role_B5408_DoA3_8818Id}_{user_B5408_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA3_8818Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA3_8818Id,
                        UserId = user_B5408_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA1: melidasuzanap@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5408_DoA1_619Id) &&
                emailToUserId.TryGetValue("melidasuzanap@unops.org", out var user_B5408_DoA1_619Id))
            {
                var key_B5408_DoA1_619 = $"{orgUnit_B5408Id}_{role_B5408_DoA1_619Id}_{user_B5408_DoA1_619Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA1_619))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA1_619Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA1_619Id,
                        UserId = user_B5408_DoA1_619Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA1_619); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("melidasuzanap@unops.org")) missingUsers.Add("melidasuzanap@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5408_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5408_DoA3_9564Id))
            {
                var key_B5408_DoA3_9564 = $"{orgUnit_B5408Id}_{role_B5408_DoA3_9564Id}_{user_B5408_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA3_9564Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA3_9564Id,
                        UserId = user_B5408_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // DoA1: marialk@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5408_DoA1_1703Id) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5408_DoA1_1703Id))
            {
                var key_B5408_DoA1_1703 = $"{orgUnit_B5408Id}_{role_B5408_DoA1_1703Id}_{user_B5408_DoA1_1703Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA1_1703))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA1_1703Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA1_1703Id,
                        UserId = user_B5408_DoA1_1703Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA1_1703); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("marialk@unops.org")) missingUsers.Add("marialk@unops.org");
            }
            // DoA2: marialk@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5408_DoA2_1703Id) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5408_DoA2_1703Id))
            {
                var key_B5408_DoA2_1703 = $"{orgUnit_B5408Id}_{role_B5408_DoA2_1703Id}_{user_B5408_DoA2_1703Id}";
                if (!existingRoleKeys.Contains(key_B5408_DoA2_1703))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_DoA2_1703Id}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_DoA2_1703Id,
                        UserId = user_B5408_DoA2_1703Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5408_DoA2_1703); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("marialk@unops.org")) missingUsers.Add("marialk@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5408");
        }

        // B5410
        if (codeToOrgUnitId.TryGetValue("B5410", out var orgUnit_B5410Id))
        {
            // DoA2: monicasi@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5410_DoA2_3284Id) &&
                emailToUserId.TryGetValue("monicasi@unops.org", out var user_B5410_DoA2_3284Id))
            {
                var key_B5410_DoA2_3284 = $"{orgUnit_B5410Id}_{role_B5410_DoA2_3284Id}_{user_B5410_DoA2_3284Id}";
                if (!existingRoleKeys.Contains(key_B5410_DoA2_3284))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_DoA2_3284Id}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_DoA2_3284Id,
                        UserId = user_B5410_DoA2_3284Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5410_DoA2_3284); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("monicasi@unops.org")) missingUsers.Add("monicasi@unops.org");
            }
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5410_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5410_DoA3_120Id))
            {
                var key_B5410_DoA3_120 = $"{orgUnit_B5410Id}_{role_B5410_DoA3_120Id}_{user_B5410_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5410_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_DoA3_120Id}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_DoA3_120Id,
                        UserId = user_B5410_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5410_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5410_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5410_DoA3_8818Id))
            {
                var key_B5410_DoA3_8818 = $"{orgUnit_B5410Id}_{role_B5410_DoA3_8818Id}_{user_B5410_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5410_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_DoA3_8818Id}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_DoA3_8818Id,
                        UserId = user_B5410_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5410_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: nickg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5410_DoA2_92Id) &&
                emailToUserId.TryGetValue("nickg@unops.org", out var user_B5410_DoA2_92Id))
            {
                var key_B5410_DoA2_92 = $"{orgUnit_B5410Id}_{role_B5410_DoA2_92Id}_{user_B5410_DoA2_92Id}";
                if (!existingRoleKeys.Contains(key_B5410_DoA2_92))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_DoA2_92Id}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_DoA2_92Id,
                        UserId = user_B5410_DoA2_92Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5410_DoA2_92); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nickg@unops.org")) missingUsers.Add("nickg@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5410_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5410_DoA3_9564Id))
            {
                var key_B5410_DoA3_9564 = $"{orgUnit_B5410Id}_{role_B5410_DoA3_9564Id}_{user_B5410_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5410_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_DoA3_9564Id}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_DoA3_9564Id,
                        UserId = user_B5410_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5410_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5410");
        }

        // B5411
        if (codeToOrgUnitId.TryGetValue("B5411", out var orgUnit_B5411Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5411_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5411_DoA3_120Id))
            {
                var key_B5411_DoA3_120 = $"{orgUnit_B5411Id}_{role_B5411_DoA3_120Id}_{user_B5411_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5411_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_DoA3_120Id}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_DoA3_120Id,
                        UserId = user_B5411_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5411_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA1: robertoc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5411_DoA1_5222Id) &&
                emailToUserId.TryGetValue("robertoc@unops.org", out var user_B5411_DoA1_5222Id))
            {
                var key_B5411_DoA1_5222 = $"{orgUnit_B5411Id}_{role_B5411_DoA1_5222Id}_{user_B5411_DoA1_5222Id}";
                if (!existingRoleKeys.Contains(key_B5411_DoA1_5222))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_DoA1_5222Id}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_DoA1_5222Id,
                        UserId = user_B5411_DoA1_5222Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5411_DoA1_5222); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("robertoc@unops.org")) missingUsers.Add("robertoc@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5411_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5411_DoA3_8818Id))
            {
                var key_B5411_DoA3_8818 = $"{orgUnit_B5411Id}_{role_B5411_DoA3_8818Id}_{user_B5411_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5411_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_DoA3_8818Id}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_DoA3_8818Id,
                        UserId = user_B5411_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5411_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: fernandoc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5411_DoA2_4689Id) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5411_DoA2_4689Id))
            {
                var key_B5411_DoA2_4689 = $"{orgUnit_B5411Id}_{role_B5411_DoA2_4689Id}_{user_B5411_DoA2_4689Id}";
                if (!existingRoleKeys.Contains(key_B5411_DoA2_4689))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_DoA2_4689Id}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_DoA2_4689Id,
                        UserId = user_B5411_DoA2_4689Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5411_DoA2_4689); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5411_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5411_DoA3_9564Id))
            {
                var key_B5411_DoA3_9564 = $"{orgUnit_B5411Id}_{role_B5411_DoA3_9564Id}_{user_B5411_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5411_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_DoA3_9564Id}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_DoA3_9564Id,
                        UserId = user_B5411_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5411_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5411");
        }

        // B5412
        if (codeToOrgUnitId.TryGetValue("B5412", out var orgUnit_B5412Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5412_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5412_DoA3_120Id))
            {
                var key_B5412_DoA3_120 = $"{orgUnit_B5412Id}_{role_B5412_DoA3_120Id}_{user_B5412_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5412_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_DoA3_120Id}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_DoA3_120Id,
                        UserId = user_B5412_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5412_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA2: claudiav@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5412_DoA2_8648Id) &&
                emailToUserId.TryGetValue("claudiav@unops.org", out var user_B5412_DoA2_8648Id))
            {
                var key_B5412_DoA2_8648 = $"{orgUnit_B5412Id}_{role_B5412_DoA2_8648Id}_{user_B5412_DoA2_8648Id}";
                if (!existingRoleKeys.Contains(key_B5412_DoA2_8648))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_DoA2_8648Id}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_DoA2_8648Id,
                        UserId = user_B5412_DoA2_8648Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5412_DoA2_8648); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("claudiav@unops.org")) missingUsers.Add("claudiav@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5412_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5412_DoA3_8818Id))
            {
                var key_B5412_DoA3_8818 = $"{orgUnit_B5412Id}_{role_B5412_DoA3_8818Id}_{user_B5412_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5412_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_DoA3_8818Id}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_DoA3_8818Id,
                        UserId = user_B5412_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5412_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5412_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5412_DoA3_9564Id))
            {
                var key_B5412_DoA3_9564 = $"{orgUnit_B5412Id}_{role_B5412_DoA3_9564Id}_{user_B5412_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5412_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_DoA3_9564Id}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_DoA3_9564Id,
                        UserId = user_B5412_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5412_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5412");
        }

        // B5414
        if (codeToOrgUnitId.TryGetValue("B5414", out var orgUnit_B5414Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5414_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5414_DoA3_120Id))
            {
                var key_B5414_DoA3_120 = $"{orgUnit_B5414Id}_{role_B5414_DoA3_120Id}_{user_B5414_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5414_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_DoA3_120Id}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_DoA3_120Id,
                        UserId = user_B5414_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5414_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5414_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5414_DoA3_8818Id))
            {
                var key_B5414_DoA3_8818 = $"{orgUnit_B5414Id}_{role_B5414_DoA3_8818Id}_{user_B5414_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5414_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_DoA3_8818Id}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_DoA3_8818Id,
                        UserId = user_B5414_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5414_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: fernandoc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5414_DoA2_4689Id) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5414_DoA2_4689Id))
            {
                var key_B5414_DoA2_4689 = $"{orgUnit_B5414Id}_{role_B5414_DoA2_4689Id}_{user_B5414_DoA2_4689Id}";
                if (!existingRoleKeys.Contains(key_B5414_DoA2_4689))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_DoA2_4689Id}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_DoA2_4689Id,
                        UserId = user_B5414_DoA2_4689Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5414_DoA2_4689); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5414_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5414_DoA3_9564Id))
            {
                var key_B5414_DoA3_9564 = $"{orgUnit_B5414Id}_{role_B5414_DoA3_9564Id}_{user_B5414_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5414_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_DoA3_9564Id}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_DoA3_9564Id,
                        UserId = user_B5414_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5414_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5414");
        }

        // B5416
        if (codeToOrgUnitId.TryGetValue("B5416", out var orgUnit_B5416Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5416_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5416_DoA3_120Id))
            {
                var key_B5416_DoA3_120 = $"{orgUnit_B5416Id}_{role_B5416_DoA3_120Id}_{user_B5416_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA3_120Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA3_120Id,
                        UserId = user_B5416_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA2: alexandrak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5416_DoA2_3729Id) &&
                emailToUserId.TryGetValue("alexandrak@unops.org", out var user_B5416_DoA2_3729Id))
            {
                var key_B5416_DoA2_3729 = $"{orgUnit_B5416Id}_{role_B5416_DoA2_3729Id}_{user_B5416_DoA2_3729Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA2_3729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA2_3729Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA2_3729Id,
                        UserId = user_B5416_DoA2_3729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA2_3729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("alexandrak@unops.org")) missingUsers.Add("alexandrak@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5416_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5416_DoA3_8818Id))
            {
                var key_B5416_DoA3_8818 = $"{orgUnit_B5416Id}_{role_B5416_DoA3_8818Id}_{user_B5416_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA3_8818Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA3_8818Id,
                        UserId = user_B5416_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA1: melidasuzanap@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5416_DoA1_619Id) &&
                emailToUserId.TryGetValue("melidasuzanap@unops.org", out var user_B5416_DoA1_619Id))
            {
                var key_B5416_DoA1_619 = $"{orgUnit_B5416Id}_{role_B5416_DoA1_619Id}_{user_B5416_DoA1_619Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA1_619))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA1_619Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA1_619Id,
                        UserId = user_B5416_DoA1_619Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA1_619); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("melidasuzanap@unops.org")) missingUsers.Add("melidasuzanap@unops.org");
            }
            // DoA1: davidme@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5416_DoA1_4307Id) &&
                emailToUserId.TryGetValue("davidme@unops.org", out var user_B5416_DoA1_4307Id))
            {
                var key_B5416_DoA1_4307 = $"{orgUnit_B5416Id}_{role_B5416_DoA1_4307Id}_{user_B5416_DoA1_4307Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA1_4307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA1_4307Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA1_4307Id,
                        UserId = user_B5416_DoA1_4307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA1_4307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("davidme@unops.org")) missingUsers.Add("davidme@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5416_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5416_DoA3_9564Id))
            {
                var key_B5416_DoA3_9564 = $"{orgUnit_B5416Id}_{role_B5416_DoA3_9564Id}_{user_B5416_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA3_9564Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA3_9564Id,
                        UserId = user_B5416_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // DoA2: davidme@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5416_DoA2_4307Id) &&
                emailToUserId.TryGetValue("davidme@unops.org", out var user_B5416_DoA2_4307Id))
            {
                var key_B5416_DoA2_4307 = $"{orgUnit_B5416Id}_{role_B5416_DoA2_4307Id}_{user_B5416_DoA2_4307Id}";
                if (!existingRoleKeys.Contains(key_B5416_DoA2_4307))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_DoA2_4307Id}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_DoA2_4307Id,
                        UserId = user_B5416_DoA2_4307Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5416_DoA2_4307); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("davidme@unops.org")) missingUsers.Add("davidme@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5416");
        }

        // B5417
        if (codeToOrgUnitId.TryGetValue("B5417", out var orgUnit_B5417Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5417_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5417_DoA3_120Id))
            {
                var key_B5417_DoA3_120 = $"{orgUnit_B5417Id}_{role_B5417_DoA3_120Id}_{user_B5417_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5417_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_DoA3_120Id}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_DoA3_120Id,
                        UserId = user_B5417_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5417_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA2: claudiav@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5417_DoA2_8648Id) &&
                emailToUserId.TryGetValue("claudiav@unops.org", out var user_B5417_DoA2_8648Id))
            {
                var key_B5417_DoA2_8648 = $"{orgUnit_B5417Id}_{role_B5417_DoA2_8648Id}_{user_B5417_DoA2_8648Id}";
                if (!existingRoleKeys.Contains(key_B5417_DoA2_8648))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_DoA2_8648Id}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_DoA2_8648Id,
                        UserId = user_B5417_DoA2_8648Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5417_DoA2_8648); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("claudiav@unops.org")) missingUsers.Add("claudiav@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5417_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5417_DoA3_8818Id))
            {
                var key_B5417_DoA3_8818 = $"{orgUnit_B5417Id}_{role_B5417_DoA3_8818Id}_{user_B5417_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5417_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_DoA3_8818Id}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_DoA3_8818Id,
                        UserId = user_B5417_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5417_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5417_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5417_DoA3_9564Id))
            {
                var key_B5417_DoA3_9564 = $"{orgUnit_B5417Id}_{role_B5417_DoA3_9564Id}_{user_B5417_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5417_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_DoA3_9564Id}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_DoA3_9564Id,
                        UserId = user_B5417_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5417_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5417");
        }

        // B5418
        if (codeToOrgUnitId.TryGetValue("B5418", out var orgUnit_B5418Id))
        {
            // DoA3: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5418_DoA3_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5418_DoA3_120Id))
            {
                var key_B5418_DoA3_120 = $"{orgUnit_B5418Id}_{role_B5418_DoA3_120Id}_{user_B5418_DoA3_120Id}";
                if (!existingRoleKeys.Contains(key_B5418_DoA3_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5418Id} - {user_B5418_DoA3_120Id}",
                        EntityId = orgUnit_B5418Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5418_DoA3_120Id,
                        UserId = user_B5418_DoA3_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5418_DoA3_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA2: alexandrak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5418_DoA2_3729Id) &&
                emailToUserId.TryGetValue("alexandrak@unops.org", out var user_B5418_DoA2_3729Id))
            {
                var key_B5418_DoA2_3729 = $"{orgUnit_B5418Id}_{role_B5418_DoA2_3729Id}_{user_B5418_DoA2_3729Id}";
                if (!existingRoleKeys.Contains(key_B5418_DoA2_3729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5418Id} - {user_B5418_DoA2_3729Id}",
                        EntityId = orgUnit_B5418Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5418_DoA2_3729Id,
                        UserId = user_B5418_DoA2_3729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5418_DoA2_3729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("alexandrak@unops.org")) missingUsers.Add("alexandrak@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5418_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5418_DoA3_8818Id))
            {
                var key_B5418_DoA3_8818 = $"{orgUnit_B5418Id}_{role_B5418_DoA3_8818Id}_{user_B5418_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5418_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5418Id} - {user_B5418_DoA3_8818Id}",
                        EntityId = orgUnit_B5418Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5418_DoA3_8818Id,
                        UserId = user_B5418_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5418_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA1: melidasuzanap@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5418_DoA1_619Id) &&
                emailToUserId.TryGetValue("melidasuzanap@unops.org", out var user_B5418_DoA1_619Id))
            {
                var key_B5418_DoA1_619 = $"{orgUnit_B5418Id}_{role_B5418_DoA1_619Id}_{user_B5418_DoA1_619Id}";
                if (!existingRoleKeys.Contains(key_B5418_DoA1_619))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5418Id} - {user_B5418_DoA1_619Id}",
                        EntityId = orgUnit_B5418Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5418_DoA1_619Id,
                        UserId = user_B5418_DoA1_619Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5418_DoA1_619); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("melidasuzanap@unops.org")) missingUsers.Add("melidasuzanap@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5418_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5418_DoA3_9564Id))
            {
                var key_B5418_DoA3_9564 = $"{orgUnit_B5418Id}_{role_B5418_DoA3_9564Id}_{user_B5418_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5418_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5418Id} - {user_B5418_DoA3_9564Id}",
                        EntityId = orgUnit_B5418Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5418_DoA3_9564Id,
                        UserId = user_B5418_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5418_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5418");
        }

        // B5419
        if (codeToOrgUnitId.TryGetValue("B5419", out var orgUnit_B5419Id))
        {
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5419_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5419_DoA3_8818Id))
            {
                var key_B5419_DoA3_8818 = $"{orgUnit_B5419Id}_{role_B5419_DoA3_8818Id}_{user_B5419_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5419_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5419Id} - {user_B5419_DoA3_8818Id}",
                        EntityId = orgUnit_B5419Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5419_DoA3_8818Id,
                        UserId = user_B5419_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5419_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5419_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5419_DoA3_9564Id))
            {
                var key_B5419_DoA3_9564 = $"{orgUnit_B5419Id}_{role_B5419_DoA3_9564Id}_{user_B5419_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5419_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5419Id} - {user_B5419_DoA3_9564Id}",
                        EntityId = orgUnit_B5419Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5419_DoA3_9564Id,
                        UserId = user_B5419_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5419_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5419");
        }

        // B5421
        if (codeToOrgUnitId.TryGetValue("B5421", out var orgUnit_B5421Id))
        {
            // DoA2: giuseppem@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5421_DoA2_120Id) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5421_DoA2_120Id))
            {
                var key_B5421_DoA2_120 = $"{orgUnit_B5421Id}_{role_B5421_DoA2_120Id}_{user_B5421_DoA2_120Id}";
                if (!existingRoleKeys.Contains(key_B5421_DoA2_120))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_DoA2_120Id}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_DoA2_120Id,
                        UserId = user_B5421_DoA2_120Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5421_DoA2_120); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5421_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5421_DoA3_8818Id))
            {
                var key_B5421_DoA3_8818 = $"{orgUnit_B5421Id}_{role_B5421_DoA3_8818Id}_{user_B5421_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5421_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_DoA3_8818Id}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_DoA3_8818Id,
                        UserId = user_B5421_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5421_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA1: jorgeb@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5421_DoA1_9450Id) &&
                emailToUserId.TryGetValue("jorgeb@unops.org", out var user_B5421_DoA1_9450Id))
            {
                var key_B5421_DoA1_9450 = $"{orgUnit_B5421Id}_{role_B5421_DoA1_9450Id}_{user_B5421_DoA1_9450Id}";
                if (!existingRoleKeys.Contains(key_B5421_DoA1_9450))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_DoA1_9450Id}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_DoA1_9450Id,
                        UserId = user_B5421_DoA1_9450Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5421_DoA1_9450); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jorgeb@unops.org")) missingUsers.Add("jorgeb@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5421_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5421_DoA3_9564Id))
            {
                var key_B5421_DoA3_9564 = $"{orgUnit_B5421Id}_{role_B5421_DoA3_9564Id}_{user_B5421_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5421_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_DoA3_9564Id}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_DoA3_9564Id,
                        UserId = user_B5421_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5421_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5421");
        }

        // B5422
        if (codeToOrgUnitId.TryGetValue("B5422", out var orgUnit_B5422Id))
        {
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5422_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5422_DoA3_8818Id))
            {
                var key_B5422_DoA3_8818 = $"{orgUnit_B5422Id}_{role_B5422_DoA3_8818Id}_{user_B5422_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5422_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_DoA3_8818Id}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_DoA3_8818Id,
                        UserId = user_B5422_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5422_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: fernandoc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5422_DoA2_4689Id) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5422_DoA2_4689Id))
            {
                var key_B5422_DoA2_4689 = $"{orgUnit_B5422Id}_{role_B5422_DoA2_4689Id}_{user_B5422_DoA2_4689Id}";
                if (!existingRoleKeys.Contains(key_B5422_DoA2_4689))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_DoA2_4689Id}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_DoA2_4689Id,
                        UserId = user_B5422_DoA2_4689Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5422_DoA2_4689); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5422_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5422_DoA3_9564Id))
            {
                var key_B5422_DoA3_9564 = $"{orgUnit_B5422Id}_{role_B5422_DoA3_9564Id}_{user_B5422_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5422_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_DoA3_9564Id}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_DoA3_9564Id,
                        UserId = user_B5422_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5422_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5422");
        }

        // B5423
        if (codeToOrgUnitId.TryGetValue("B5423", out var orgUnit_B5423Id))
        {
            // DoA2: alexandrak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5423_DoA2_3729Id) &&
                emailToUserId.TryGetValue("alexandrak@unops.org", out var user_B5423_DoA2_3729Id))
            {
                var key_B5423_DoA2_3729 = $"{orgUnit_B5423Id}_{role_B5423_DoA2_3729Id}_{user_B5423_DoA2_3729Id}";
                if (!existingRoleKeys.Contains(key_B5423_DoA2_3729))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_DoA2_3729Id}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_DoA2_3729Id,
                        UserId = user_B5423_DoA2_3729Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5423_DoA2_3729); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("alexandrak@unops.org")) missingUsers.Add("alexandrak@unops.org");
            }
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5423_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5423_DoA3_8818Id))
            {
                var key_B5423_DoA3_8818 = $"{orgUnit_B5423Id}_{role_B5423_DoA3_8818Id}_{user_B5423_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5423_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_DoA3_8818Id}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_DoA3_8818Id,
                        UserId = user_B5423_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5423_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA1: melidasuzanap@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5423_DoA1_619Id) &&
                emailToUserId.TryGetValue("melidasuzanap@unops.org", out var user_B5423_DoA1_619Id))
            {
                var key_B5423_DoA1_619 = $"{orgUnit_B5423Id}_{role_B5423_DoA1_619Id}_{user_B5423_DoA1_619Id}";
                if (!existingRoleKeys.Contains(key_B5423_DoA1_619))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_DoA1_619Id}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_DoA1_619Id,
                        UserId = user_B5423_DoA1_619Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5423_DoA1_619); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("melidasuzanap@unops.org")) missingUsers.Add("melidasuzanap@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5423_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5423_DoA3_9564Id))
            {
                var key_B5423_DoA3_9564 = $"{orgUnit_B5423Id}_{role_B5423_DoA3_9564Id}_{user_B5423_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5423_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_DoA3_9564Id}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_DoA3_9564Id,
                        UserId = user_B5423_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5423_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5423");
        }

        // B5424
        if (codeToOrgUnitId.TryGetValue("B5424", out var orgUnit_B5424Id))
        {
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5424_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5424_DoA3_8818Id))
            {
                var key_B5424_DoA3_8818 = $"{orgUnit_B5424Id}_{role_B5424_DoA3_8818Id}_{user_B5424_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5424_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_DoA3_8818Id}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_DoA3_8818Id,
                        UserId = user_B5424_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5424_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: nickg@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5424_DoA2_92Id) &&
                emailToUserId.TryGetValue("nickg@unops.org", out var user_B5424_DoA2_92Id))
            {
                var key_B5424_DoA2_92 = $"{orgUnit_B5424Id}_{role_B5424_DoA2_92Id}_{user_B5424_DoA2_92Id}";
                if (!existingRoleKeys.Contains(key_B5424_DoA2_92))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_DoA2_92Id}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_DoA2_92Id,
                        UserId = user_B5424_DoA2_92Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5424_DoA2_92); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("nickg@unops.org")) missingUsers.Add("nickg@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5424_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5424_DoA3_9564Id))
            {
                var key_B5424_DoA3_9564 = $"{orgUnit_B5424Id}_{role_B5424_DoA3_9564Id}_{user_B5424_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5424_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_DoA3_9564Id}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_DoA3_9564Id,
                        UserId = user_B5424_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5424_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5424");
        }

        // B5425
        if (codeToOrgUnitId.TryGetValue("B5425", out var orgUnit_B5425Id))
        {
            // DoA3: paolab@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5425_DoA3_8818Id) &&
                emailToUserId.TryGetValue("paolab@unops.org", out var user_B5425_DoA3_8818Id))
            {
                var key_B5425_DoA3_8818 = $"{orgUnit_B5425Id}_{role_B5425_DoA3_8818Id}_{user_B5425_DoA3_8818Id}";
                if (!existingRoleKeys.Contains(key_B5425_DoA3_8818))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_DoA3_8818Id}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_DoA3_8818Id,
                        UserId = user_B5425_DoA3_8818Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5425_DoA3_8818); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("paolab@unops.org")) missingUsers.Add("paolab@unops.org");
            }
            // DoA2: claudiav@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5425_DoA2_8648Id) &&
                emailToUserId.TryGetValue("claudiav@unops.org", out var user_B5425_DoA2_8648Id))
            {
                var key_B5425_DoA2_8648 = $"{orgUnit_B5425Id}_{role_B5425_DoA2_8648Id}_{user_B5425_DoA2_8648Id}";
                if (!existingRoleKeys.Contains(key_B5425_DoA2_8648))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_DoA2_8648Id}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_DoA2_8648Id,
                        UserId = user_B5425_DoA2_8648Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5425_DoA2_8648); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("claudiav@unops.org")) missingUsers.Add("claudiav@unops.org");
            }
            // DoA3: dalilag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5425_DoA3_9564Id) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5425_DoA3_9564Id))
            {
                var key_B5425_DoA3_9564 = $"{orgUnit_B5425Id}_{role_B5425_DoA3_9564Id}_{user_B5425_DoA3_9564Id}";
                if (!existingRoleKeys.Contains(key_B5425_DoA3_9564))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_DoA3_9564Id}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_DoA3_9564Id,
                        UserId = user_B5425_DoA3_9564Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5425_DoA3_9564); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5425");
        }

        // B5502
        if (codeToOrgUnitId.TryGetValue("B5502", out var orgUnit_B5502Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5502_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5502_DoA1_7955Id))
            {
                var key_B5502_DoA1_7955 = $"{orgUnit_B5502Id}_{role_B5502_DoA1_7955Id}_{user_B5502_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5502_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_DoA1_7955Id}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_DoA1_7955Id,
                        UserId = user_B5502_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5502_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5502_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5502_DoA1_5331Id))
            {
                var key_B5502_DoA1_5331 = $"{orgUnit_B5502Id}_{role_B5502_DoA1_5331Id}_{user_B5502_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5502_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_DoA1_5331Id}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_DoA1_5331Id,
                        UserId = user_B5502_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5502_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5502_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5502_DoA2_1626Id))
            {
                var key_B5502_DoA2_1626 = $"{orgUnit_B5502Id}_{role_B5502_DoA2_1626Id}_{user_B5502_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5502_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_DoA2_1626Id}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_DoA2_1626Id,
                        UserId = user_B5502_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5502_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5502_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5502_DoA3_3918Id))
            {
                var key_B5502_DoA3_3918 = $"{orgUnit_B5502Id}_{role_B5502_DoA3_3918Id}_{user_B5502_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5502_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_DoA3_3918Id}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_DoA3_3918Id,
                        UserId = user_B5502_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5502_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5502");
        }

        // B5503
        if (codeToOrgUnitId.TryGetValue("B5503", out var orgUnit_B5503Id))
        {
            // DoA1: vinodm@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5503_DoA1_8809Id) &&
                emailToUserId.TryGetValue("vinodm@unops.org", out var user_B5503_DoA1_8809Id))
            {
                var key_B5503_DoA1_8809 = $"{orgUnit_B5503Id}_{role_B5503_DoA1_8809Id}_{user_B5503_DoA1_8809Id}";
                if (!existingRoleKeys.Contains(key_B5503_DoA1_8809))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_DoA1_8809Id}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_DoA1_8809Id,
                        UserId = user_B5503_DoA1_8809Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5503_DoA1_8809); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("vinodm@unops.org")) missingUsers.Add("vinodm@unops.org");
            }
            // DoA1: makir@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5503_DoA1_7016Id) &&
                emailToUserId.TryGetValue("makir@unops.org", out var user_B5503_DoA1_7016Id))
            {
                var key_B5503_DoA1_7016 = $"{orgUnit_B5503Id}_{role_B5503_DoA1_7016Id}_{user_B5503_DoA1_7016Id}";
                if (!existingRoleKeys.Contains(key_B5503_DoA1_7016))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_DoA1_7016Id}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_DoA1_7016Id,
                        UserId = user_B5503_DoA1_7016Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5503_DoA1_7016); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("makir@unops.org")) missingUsers.Add("makir@unops.org");
            }
            // DoA2: charlesc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5503_DoA2_8485Id) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5503_DoA2_8485Id))
            {
                var key_B5503_DoA2_8485 = $"{orgUnit_B5503Id}_{role_B5503_DoA2_8485Id}_{user_B5503_DoA2_8485Id}";
                if (!existingRoleKeys.Contains(key_B5503_DoA2_8485))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_DoA2_8485Id}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_DoA2_8485Id,
                        UserId = user_B5503_DoA2_8485Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5503_DoA2_8485); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5503_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5503_DoA3_3918Id))
            {
                var key_B5503_DoA3_3918 = $"{orgUnit_B5503Id}_{role_B5503_DoA3_3918Id}_{user_B5503_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5503_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_DoA3_3918Id}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_DoA3_3918Id,
                        UserId = user_B5503_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5503_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5503");
        }

        // B5505
        if (codeToOrgUnitId.TryGetValue("B5505", out var orgUnit_B5505Id))
        {
            // DoA1: makir@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5505_DoA1_7016Id) &&
                emailToUserId.TryGetValue("makir@unops.org", out var user_B5505_DoA1_7016Id))
            {
                var key_B5505_DoA1_7016 = $"{orgUnit_B5505Id}_{role_B5505_DoA1_7016Id}_{user_B5505_DoA1_7016Id}";
                if (!existingRoleKeys.Contains(key_B5505_DoA1_7016))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_DoA1_7016Id}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_DoA1_7016Id,
                        UserId = user_B5505_DoA1_7016Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5505_DoA1_7016); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("makir@unops.org")) missingUsers.Add("makir@unops.org");
            }
            // DoA2: charlesc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5505_DoA2_8485Id) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5505_DoA2_8485Id))
            {
                var key_B5505_DoA2_8485 = $"{orgUnit_B5505Id}_{role_B5505_DoA2_8485Id}_{user_B5505_DoA2_8485Id}";
                if (!existingRoleKeys.Contains(key_B5505_DoA2_8485))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_DoA2_8485Id}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_DoA2_8485Id,
                        UserId = user_B5505_DoA2_8485Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5505_DoA2_8485); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // DoA1: aleksandrar@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5505_DoA1_9919Id) &&
                emailToUserId.TryGetValue("aleksandrar@unops.org", out var user_B5505_DoA1_9919Id))
            {
                var key_B5505_DoA1_9919 = $"{orgUnit_B5505Id}_{role_B5505_DoA1_9919Id}_{user_B5505_DoA1_9919Id}";
                if (!existingRoleKeys.Contains(key_B5505_DoA1_9919))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_DoA1_9919Id}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_DoA1_9919Id,
                        UserId = user_B5505_DoA1_9919Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5505_DoA1_9919); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("aleksandrar@unops.org")) missingUsers.Add("aleksandrar@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5505_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5505_DoA3_3918Id))
            {
                var key_B5505_DoA3_3918 = $"{orgUnit_B5505Id}_{role_B5505_DoA3_3918Id}_{user_B5505_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5505_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_DoA3_3918Id}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_DoA3_3918Id,
                        UserId = user_B5505_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5505_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5505");
        }

        // B5506
        if (codeToOrgUnitId.TryGetValue("B5506", out var orgUnit_B5506Id))
        {
            // DoA1: akikok@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5506_DoA1_5203Id) &&
                emailToUserId.TryGetValue("akikok@unops.org", out var user_B5506_DoA1_5203Id))
            {
                var key_B5506_DoA1_5203 = $"{orgUnit_B5506Id}_{role_B5506_DoA1_5203Id}_{user_B5506_DoA1_5203Id}";
                if (!existingRoleKeys.Contains(key_B5506_DoA1_5203))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_DoA1_5203Id}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_DoA1_5203Id,
                        UserId = user_B5506_DoA1_5203Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5506_DoA1_5203); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("akikok@unops.org")) missingUsers.Add("akikok@unops.org");
            }
            // DoA1: tua@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5506_DoA1_8974Id) &&
                emailToUserId.TryGetValue("tua@unops.org", out var user_B5506_DoA1_8974Id))
            {
                var key_B5506_DoA1_8974 = $"{orgUnit_B5506Id}_{role_B5506_DoA1_8974Id}_{user_B5506_DoA1_8974Id}";
                if (!existingRoleKeys.Contains(key_B5506_DoA1_8974))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_DoA1_8974Id}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_DoA1_8974Id,
                        UserId = user_B5506_DoA1_8974Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5506_DoA1_8974); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tua@unops.org")) missingUsers.Add("tua@unops.org");
            }
            // DoA2: akikok@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5506_DoA2_5203Id) &&
                emailToUserId.TryGetValue("akikok@unops.org", out var user_B5506_DoA2_5203Id))
            {
                var key_B5506_DoA2_5203 = $"{orgUnit_B5506Id}_{role_B5506_DoA2_5203Id}_{user_B5506_DoA2_5203Id}";
                if (!existingRoleKeys.Contains(key_B5506_DoA2_5203))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_DoA2_5203Id}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_DoA2_5203Id,
                        UserId = user_B5506_DoA2_5203Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5506_DoA2_5203); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("akikok@unops.org")) missingUsers.Add("akikok@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5506_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5506_DoA3_3918Id))
            {
                var key_B5506_DoA3_3918 = $"{orgUnit_B5506Id}_{role_B5506_DoA3_3918Id}_{user_B5506_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5506_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_DoA3_3918Id}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_DoA3_3918Id,
                        UserId = user_B5506_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5506_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5506");
        }

        // B5507
        if (codeToOrgUnitId.TryGetValue("B5507", out var orgUnit_B5507Id))
        {
            // DoA1: jennifera@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5507_DoA1_2417Id) &&
                emailToUserId.TryGetValue("jennifera@unops.org", out var user_B5507_DoA1_2417Id))
            {
                var key_B5507_DoA1_2417 = $"{orgUnit_B5507Id}_{role_B5507_DoA1_2417Id}_{user_B5507_DoA1_2417Id}";
                if (!existingRoleKeys.Contains(key_B5507_DoA1_2417))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_DoA1_2417Id}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_DoA1_2417Id,
                        UserId = user_B5507_DoA1_2417Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5507_DoA1_2417); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("jennifera@unops.org")) missingUsers.Add("jennifera@unops.org");
            }
            // DoA1: makir@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5507_DoA1_7016Id) &&
                emailToUserId.TryGetValue("makir@unops.org", out var user_B5507_DoA1_7016Id))
            {
                var key_B5507_DoA1_7016 = $"{orgUnit_B5507Id}_{role_B5507_DoA1_7016Id}_{user_B5507_DoA1_7016Id}";
                if (!existingRoleKeys.Contains(key_B5507_DoA1_7016))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_DoA1_7016Id}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_DoA1_7016Id,
                        UserId = user_B5507_DoA1_7016Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5507_DoA1_7016); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("makir@unops.org")) missingUsers.Add("makir@unops.org");
            }
            // DoA2: charlesc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5507_DoA2_8485Id) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5507_DoA2_8485Id))
            {
                var key_B5507_DoA2_8485 = $"{orgUnit_B5507Id}_{role_B5507_DoA2_8485Id}_{user_B5507_DoA2_8485Id}";
                if (!existingRoleKeys.Contains(key_B5507_DoA2_8485))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_DoA2_8485Id}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_DoA2_8485Id,
                        UserId = user_B5507_DoA2_8485Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5507_DoA2_8485); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5507_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5507_DoA3_3918Id))
            {
                var key_B5507_DoA3_3918 = $"{orgUnit_B5507Id}_{role_B5507_DoA3_3918Id}_{user_B5507_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5507_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_DoA3_3918Id}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_DoA3_3918Id,
                        UserId = user_B5507_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5507_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5507");
        }

        // B5510
        if (codeToOrgUnitId.TryGetValue("B5510", out var orgUnit_B5510Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5510_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5510_DoA1_7955Id))
            {
                var key_B5510_DoA1_7955 = $"{orgUnit_B5510Id}_{role_B5510_DoA1_7955Id}_{user_B5510_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5510_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_DoA1_7955Id}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_DoA1_7955Id,
                        UserId = user_B5510_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5510_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5510_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5510_DoA1_5331Id))
            {
                var key_B5510_DoA1_5331 = $"{orgUnit_B5510Id}_{role_B5510_DoA1_5331Id}_{user_B5510_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5510_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_DoA1_5331Id}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_DoA1_5331Id,
                        UserId = user_B5510_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5510_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA1: edak@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5510_DoA1_6601Id) &&
                emailToUserId.TryGetValue("edak@unops.org", out var user_B5510_DoA1_6601Id))
            {
                var key_B5510_DoA1_6601 = $"{orgUnit_B5510Id}_{role_B5510_DoA1_6601Id}_{user_B5510_DoA1_6601Id}";
                if (!existingRoleKeys.Contains(key_B5510_DoA1_6601))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_DoA1_6601Id}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_DoA1_6601Id,
                        UserId = user_B5510_DoA1_6601Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5510_DoA1_6601); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("edak@unops.org")) missingUsers.Add("edak@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5510_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5510_DoA2_1626Id))
            {
                var key_B5510_DoA2_1626 = $"{orgUnit_B5510Id}_{role_B5510_DoA2_1626Id}_{user_B5510_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5510_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_DoA2_1626Id}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_DoA2_1626Id,
                        UserId = user_B5510_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5510_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5510_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5510_DoA3_3918Id))
            {
                var key_B5510_DoA3_3918 = $"{orgUnit_B5510Id}_{role_B5510_DoA3_3918Id}_{user_B5510_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5510_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_DoA3_3918Id}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_DoA3_3918Id,
                        UserId = user_B5510_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5510_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5510");
        }

        // B5511
        if (codeToOrgUnitId.TryGetValue("B5511", out var orgUnit_B5511Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5511_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5511_DoA1_7955Id))
            {
                var key_B5511_DoA1_7955 = $"{orgUnit_B5511Id}_{role_B5511_DoA1_7955Id}_{user_B5511_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5511_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_DoA1_7955Id}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_DoA1_7955Id,
                        UserId = user_B5511_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5511_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5511_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5511_DoA1_5331Id))
            {
                var key_B5511_DoA1_5331 = $"{orgUnit_B5511Id}_{role_B5511_DoA1_5331Id}_{user_B5511_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5511_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_DoA1_5331Id}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_DoA1_5331Id,
                        UserId = user_B5511_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5511_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5511_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5511_DoA2_1626Id))
            {
                var key_B5511_DoA2_1626 = $"{orgUnit_B5511Id}_{role_B5511_DoA2_1626Id}_{user_B5511_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5511_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_DoA2_1626Id}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_DoA2_1626Id,
                        UserId = user_B5511_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5511_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5511_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5511_DoA3_3918Id))
            {
                var key_B5511_DoA3_3918 = $"{orgUnit_B5511Id}_{role_B5511_DoA3_3918Id}_{user_B5511_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5511_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_DoA3_3918Id}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_DoA3_3918Id,
                        UserId = user_B5511_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5511_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5511");
        }

        // B5512
        if (codeToOrgUnitId.TryGetValue("B5512", out var orgUnit_B5512Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5512_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5512_DoA1_7955Id))
            {
                var key_B5512_DoA1_7955 = $"{orgUnit_B5512Id}_{role_B5512_DoA1_7955Id}_{user_B5512_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5512_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_DoA1_7955Id}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_DoA1_7955Id,
                        UserId = user_B5512_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5512_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5512_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5512_DoA1_5331Id))
            {
                var key_B5512_DoA1_5331 = $"{orgUnit_B5512Id}_{role_B5512_DoA1_5331Id}_{user_B5512_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5512_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_DoA1_5331Id}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_DoA1_5331Id,
                        UserId = user_B5512_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5512_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA1: sirpaj@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5512_DoA1_5932Id) &&
                emailToUserId.TryGetValue("sirpaj@unops.org", out var user_B5512_DoA1_5932Id))
            {
                var key_B5512_DoA1_5932 = $"{orgUnit_B5512Id}_{role_B5512_DoA1_5932Id}_{user_B5512_DoA1_5932Id}";
                if (!existingRoleKeys.Contains(key_B5512_DoA1_5932))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_DoA1_5932Id}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_DoA1_5932Id,
                        UserId = user_B5512_DoA1_5932Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5512_DoA1_5932); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sirpaj@unops.org")) missingUsers.Add("sirpaj@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5512_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5512_DoA2_1626Id))
            {
                var key_B5512_DoA2_1626 = $"{orgUnit_B5512Id}_{role_B5512_DoA2_1626Id}_{user_B5512_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5512_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_DoA2_1626Id}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_DoA2_1626Id,
                        UserId = user_B5512_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5512_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5512_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5512_DoA3_3918Id))
            {
                var key_B5512_DoA3_3918 = $"{orgUnit_B5512Id}_{role_B5512_DoA3_3918Id}_{user_B5512_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5512_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_DoA3_3918Id}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_DoA3_3918Id,
                        UserId = user_B5512_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5512_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5512");
        }

        // B5514
        if (codeToOrgUnitId.TryGetValue("B5514", out var orgUnit_B5514Id))
        {
            // DoA1: sudhirm@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5514_DoA1_3497Id) &&
                emailToUserId.TryGetValue("sudhirm@unops.org", out var user_B5514_DoA1_3497Id))
            {
                var key_B5514_DoA1_3497 = $"{orgUnit_B5514Id}_{role_B5514_DoA1_3497Id}_{user_B5514_DoA1_3497Id}";
                if (!existingRoleKeys.Contains(key_B5514_DoA1_3497))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_DoA1_3497Id}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_DoA1_3497Id,
                        UserId = user_B5514_DoA1_3497Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5514_DoA1_3497); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sudhirm@unops.org")) missingUsers.Add("sudhirm@unops.org");
            }
            // DoA1: makir@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5514_DoA1_7016Id) &&
                emailToUserId.TryGetValue("makir@unops.org", out var user_B5514_DoA1_7016Id))
            {
                var key_B5514_DoA1_7016 = $"{orgUnit_B5514Id}_{role_B5514_DoA1_7016Id}_{user_B5514_DoA1_7016Id}";
                if (!existingRoleKeys.Contains(key_B5514_DoA1_7016))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_DoA1_7016Id}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_DoA1_7016Id,
                        UserId = user_B5514_DoA1_7016Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5514_DoA1_7016); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("makir@unops.org")) missingUsers.Add("makir@unops.org");
            }
            // DoA2: charlesc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5514_DoA2_8485Id) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5514_DoA2_8485Id))
            {
                var key_B5514_DoA2_8485 = $"{orgUnit_B5514Id}_{role_B5514_DoA2_8485Id}_{user_B5514_DoA2_8485Id}";
                if (!existingRoleKeys.Contains(key_B5514_DoA2_8485))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_DoA2_8485Id}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_DoA2_8485Id,
                        UserId = user_B5514_DoA2_8485Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5514_DoA2_8485); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5514_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5514_DoA3_3918Id))
            {
                var key_B5514_DoA3_3918 = $"{orgUnit_B5514Id}_{role_B5514_DoA3_3918Id}_{user_B5514_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5514_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_DoA3_3918Id}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_DoA3_3918Id,
                        UserId = user_B5514_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5514_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5514");
        }

        // B5516
        if (codeToOrgUnitId.TryGetValue("B5516", out var orgUnit_B5516Id))
        {
            // DoA1: makir@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5516_DoA1_7016Id) &&
                emailToUserId.TryGetValue("makir@unops.org", out var user_B5516_DoA1_7016Id))
            {
                var key_B5516_DoA1_7016 = $"{orgUnit_B5516Id}_{role_B5516_DoA1_7016Id}_{user_B5516_DoA1_7016Id}";
                if (!existingRoleKeys.Contains(key_B5516_DoA1_7016))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_DoA1_7016Id}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_DoA1_7016Id,
                        UserId = user_B5516_DoA1_7016Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5516_DoA1_7016); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("makir@unops.org")) missingUsers.Add("makir@unops.org");
            }
            // DoA2: charlesc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5516_DoA2_8485Id) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5516_DoA2_8485Id))
            {
                var key_B5516_DoA2_8485 = $"{orgUnit_B5516Id}_{role_B5516_DoA2_8485Id}_{user_B5516_DoA2_8485Id}";
                if (!existingRoleKeys.Contains(key_B5516_DoA2_8485))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_DoA2_8485Id}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_DoA2_8485Id,
                        UserId = user_B5516_DoA2_8485Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5516_DoA2_8485); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // DoA1: karkik@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5516_DoA1_5713Id) &&
                emailToUserId.TryGetValue("karkik@unops.org", out var user_B5516_DoA1_5713Id))
            {
                var key_B5516_DoA1_5713 = $"{orgUnit_B5516Id}_{role_B5516_DoA1_5713Id}_{user_B5516_DoA1_5713Id}";
                if (!existingRoleKeys.Contains(key_B5516_DoA1_5713))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_DoA1_5713Id}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_DoA1_5713Id,
                        UserId = user_B5516_DoA1_5713Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5516_DoA1_5713); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("karkik@unops.org")) missingUsers.Add("karkik@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5516_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5516_DoA3_3918Id))
            {
                var key_B5516_DoA3_3918 = $"{orgUnit_B5516Id}_{role_B5516_DoA3_3918Id}_{user_B5516_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5516_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_DoA3_3918Id}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_DoA3_3918Id,
                        UserId = user_B5516_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5516_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5516");
        }

        // B5517
        if (codeToOrgUnitId.TryGetValue("B5517", out var orgUnit_B5517Id))
        {
            // DoA1: makir@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5517_DoA1_7016Id) &&
                emailToUserId.TryGetValue("makir@unops.org", out var user_B5517_DoA1_7016Id))
            {
                var key_B5517_DoA1_7016 = $"{orgUnit_B5517Id}_{role_B5517_DoA1_7016Id}_{user_B5517_DoA1_7016Id}";
                if (!existingRoleKeys.Contains(key_B5517_DoA1_7016))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_DoA1_7016Id}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_DoA1_7016Id,
                        UserId = user_B5517_DoA1_7016Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5517_DoA1_7016); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("makir@unops.org")) missingUsers.Add("makir@unops.org");
            }
            // DoA2: charlesc@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5517_DoA2_8485Id) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5517_DoA2_8485Id))
            {
                var key_B5517_DoA2_8485 = $"{orgUnit_B5517Id}_{role_B5517_DoA2_8485Id}_{user_B5517_DoA2_8485Id}";
                if (!existingRoleKeys.Contains(key_B5517_DoA2_8485))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_DoA2_8485Id}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_DoA2_8485Id,
                        UserId = user_B5517_DoA2_8485Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5517_DoA2_8485); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5517_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5517_DoA3_3918Id))
            {
                var key_B5517_DoA3_3918 = $"{orgUnit_B5517Id}_{role_B5517_DoA3_3918Id}_{user_B5517_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5517_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_DoA3_3918Id}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_DoA3_3918Id,
                        UserId = user_B5517_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5517_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5517");
        }

        // B5518
        if (codeToOrgUnitId.TryGetValue("B5518", out var orgUnit_B5518Id))
        {
            // DoA1: frederics@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5518_DoA1_4266Id) &&
                emailToUserId.TryGetValue("frederics@unops.org", out var user_B5518_DoA1_4266Id))
            {
                var key_B5518_DoA1_4266 = $"{orgUnit_B5518Id}_{role_B5518_DoA1_4266Id}_{user_B5518_DoA1_4266Id}";
                if (!existingRoleKeys.Contains(key_B5518_DoA1_4266))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_DoA1_4266Id}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_DoA1_4266Id,
                        UserId = user_B5518_DoA1_4266Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5518_DoA1_4266); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("frederics@unops.org")) missingUsers.Add("frederics@unops.org");
            }
            // DoA2: attilam@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5518_DoA2_1452Id) &&
                emailToUserId.TryGetValue("attilam@unops.org", out var user_B5518_DoA2_1452Id))
            {
                var key_B5518_DoA2_1452 = $"{orgUnit_B5518Id}_{role_B5518_DoA2_1452Id}_{user_B5518_DoA2_1452Id}";
                if (!existingRoleKeys.Contains(key_B5518_DoA2_1452))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_DoA2_1452Id}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_DoA2_1452Id,
                        UserId = user_B5518_DoA2_1452Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5518_DoA2_1452); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("attilam@unops.org")) missingUsers.Add("attilam@unops.org");
            }
            // DoA2: frederics@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5518_DoA2_4266Id) &&
                emailToUserId.TryGetValue("frederics@unops.org", out var user_B5518_DoA2_4266Id))
            {
                var key_B5518_DoA2_4266 = $"{orgUnit_B5518Id}_{role_B5518_DoA2_4266Id}_{user_B5518_DoA2_4266Id}";
                if (!existingRoleKeys.Contains(key_B5518_DoA2_4266))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_DoA2_4266Id}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_DoA2_4266Id,
                        UserId = user_B5518_DoA2_4266Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5518_DoA2_4266); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("frederics@unops.org")) missingUsers.Add("frederics@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5518_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5518_DoA3_3918Id))
            {
                var key_B5518_DoA3_3918 = $"{orgUnit_B5518Id}_{role_B5518_DoA3_3918Id}_{user_B5518_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5518_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_DoA3_3918Id}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_DoA3_3918Id,
                        UserId = user_B5518_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5518_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5518");
        }

        // B5519
        if (codeToOrgUnitId.TryGetValue("B5519", out var orgUnit_B5519Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5519_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5519_DoA1_7955Id))
            {
                var key_B5519_DoA1_7955 = $"{orgUnit_B5519Id}_{role_B5519_DoA1_7955Id}_{user_B5519_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5519_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_DoA1_7955Id}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_DoA1_7955Id,
                        UserId = user_B5519_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5519_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5519_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5519_DoA1_5331Id))
            {
                var key_B5519_DoA1_5331 = $"{orgUnit_B5519Id}_{role_B5519_DoA1_5331Id}_{user_B5519_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5519_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_DoA1_5331Id}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_DoA1_5331Id,
                        UserId = user_B5519_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5519_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5519_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5519_DoA2_1626Id))
            {
                var key_B5519_DoA2_1626 = $"{orgUnit_B5519Id}_{role_B5519_DoA2_1626Id}_{user_B5519_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5519_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_DoA2_1626Id}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_DoA2_1626Id,
                        UserId = user_B5519_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5519_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5519_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5519_DoA3_3918Id))
            {
                var key_B5519_DoA3_3918 = $"{orgUnit_B5519Id}_{role_B5519_DoA3_3918Id}_{user_B5519_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5519_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_DoA3_3918Id}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_DoA3_3918Id,
                        UserId = user_B5519_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5519_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5519");
        }

        // B5520
        if (codeToOrgUnitId.TryGetValue("B5520", out var orgUnit_B5520Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5520_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5520_DoA1_7955Id))
            {
                var key_B5520_DoA1_7955 = $"{orgUnit_B5520Id}_{role_B5520_DoA1_7955Id}_{user_B5520_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5520_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5520Id} - {user_B5520_DoA1_7955Id}",
                        EntityId = orgUnit_B5520Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5520_DoA1_7955Id,
                        UserId = user_B5520_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5520_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5520_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5520_DoA1_5331Id))
            {
                var key_B5520_DoA1_5331 = $"{orgUnit_B5520Id}_{role_B5520_DoA1_5331Id}_{user_B5520_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5520_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5520Id} - {user_B5520_DoA1_5331Id}",
                        EntityId = orgUnit_B5520Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5520_DoA1_5331Id,
                        UserId = user_B5520_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5520_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA1: edak@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5520_DoA1_6601Id) &&
                emailToUserId.TryGetValue("edak@unops.org", out var user_B5520_DoA1_6601Id))
            {
                var key_B5520_DoA1_6601 = $"{orgUnit_B5520Id}_{role_B5520_DoA1_6601Id}_{user_B5520_DoA1_6601Id}";
                if (!existingRoleKeys.Contains(key_B5520_DoA1_6601))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5520Id} - {user_B5520_DoA1_6601Id}",
                        EntityId = orgUnit_B5520Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5520_DoA1_6601Id,
                        UserId = user_B5520_DoA1_6601Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5520_DoA1_6601); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("edak@unops.org")) missingUsers.Add("edak@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5520_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5520_DoA2_1626Id))
            {
                var key_B5520_DoA2_1626 = $"{orgUnit_B5520Id}_{role_B5520_DoA2_1626Id}_{user_B5520_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5520_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5520Id} - {user_B5520_DoA2_1626Id}",
                        EntityId = orgUnit_B5520Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5520_DoA2_1626Id,
                        UserId = user_B5520_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5520_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5520");
        }

        // B5521
        if (codeToOrgUnitId.TryGetValue("B5521", out var orgUnit_B5521Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5521_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5521_DoA1_7955Id))
            {
                var key_B5521_DoA1_7955 = $"{orgUnit_B5521Id}_{role_B5521_DoA1_7955Id}_{user_B5521_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5521_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_DoA1_7955Id}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_DoA1_7955Id,
                        UserId = user_B5521_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5521_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5521_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5521_DoA1_5331Id))
            {
                var key_B5521_DoA1_5331 = $"{orgUnit_B5521Id}_{role_B5521_DoA1_5331Id}_{user_B5521_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5521_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_DoA1_5331Id}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_DoA1_5331Id,
                        UserId = user_B5521_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5521_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA1: tokumitsuk@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5521_DoA1_3650Id) &&
                emailToUserId.TryGetValue("tokumitsuk@unops.org", out var user_B5521_DoA1_3650Id))
            {
                var key_B5521_DoA1_3650 = $"{orgUnit_B5521Id}_{role_B5521_DoA1_3650Id}_{user_B5521_DoA1_3650Id}";
                if (!existingRoleKeys.Contains(key_B5521_DoA1_3650))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_DoA1_3650Id}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_DoA1_3650Id,
                        UserId = user_B5521_DoA1_3650Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5521_DoA1_3650); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("tokumitsuk@unops.org")) missingUsers.Add("tokumitsuk@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5521_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5521_DoA2_1626Id))
            {
                var key_B5521_DoA2_1626 = $"{orgUnit_B5521Id}_{role_B5521_DoA2_1626Id}_{user_B5521_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5521_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_DoA2_1626Id}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_DoA2_1626Id,
                        UserId = user_B5521_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5521_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5521_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5521_DoA3_3918Id))
            {
                var key_B5521_DoA3_3918 = $"{orgUnit_B5521Id}_{role_B5521_DoA3_3918Id}_{user_B5521_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5521_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_DoA3_3918Id}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_DoA3_3918Id,
                        UserId = user_B5521_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5521_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5521");
        }

        // B5523
        if (codeToOrgUnitId.TryGetValue("B5523", out var orgUnit_B5523Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5523_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5523_DoA1_7955Id))
            {
                var key_B5523_DoA1_7955 = $"{orgUnit_B5523Id}_{role_B5523_DoA1_7955Id}_{user_B5523_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5523_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_DoA1_7955Id}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_DoA1_7955Id,
                        UserId = user_B5523_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5523_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5523_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5523_DoA1_5331Id))
            {
                var key_B5523_DoA1_5331 = $"{orgUnit_B5523Id}_{role_B5523_DoA1_5331Id}_{user_B5523_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5523_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_DoA1_5331Id}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_DoA1_5331Id,
                        UserId = user_B5523_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5523_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5523_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5523_DoA3_3918Id))
            {
                var key_B5523_DoA3_3918 = $"{orgUnit_B5523Id}_{role_B5523_DoA3_3918Id}_{user_B5523_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5523_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_DoA3_3918Id}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_DoA3_3918Id,
                        UserId = user_B5523_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5523_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5523");
        }

        // B5524
        if (codeToOrgUnitId.TryGetValue("B5524", out var orgUnit_B5524Id))
        {
            // DoA1: keithc@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5524_DoA1_7955Id) &&
                emailToUserId.TryGetValue("keithc@unops.org", out var user_B5524_DoA1_7955Id))
            {
                var key_B5524_DoA1_7955 = $"{orgUnit_B5524Id}_{role_B5524_DoA1_7955Id}_{user_B5524_DoA1_7955Id}";
                if (!existingRoleKeys.Contains(key_B5524_DoA1_7955))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_DoA1_7955Id}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_DoA1_7955Id,
                        UserId = user_B5524_DoA1_7955Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5524_DoA1_7955); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("keithc@unops.org")) missingUsers.Add("keithc@unops.org");
            }
            // DoA1: mariapa@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5524_DoA1_5331Id) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5524_DoA1_5331Id))
            {
                var key_B5524_DoA1_5331 = $"{orgUnit_B5524Id}_{role_B5524_DoA1_5331Id}_{user_B5524_DoA1_5331Id}";
                if (!existingRoleKeys.Contains(key_B5524_DoA1_5331))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_DoA1_5331Id}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_DoA1_5331Id,
                        UserId = user_B5524_DoA1_5331Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5524_DoA1_5331); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // DoA1: sirpaj@unops.org
            if (roleNameToId.TryGetValue("DoA1", out var role_B5524_DoA1_5932Id) &&
                emailToUserId.TryGetValue("sirpaj@unops.org", out var user_B5524_DoA1_5932Id))
            {
                var key_B5524_DoA1_5932 = $"{orgUnit_B5524Id}_{role_B5524_DoA1_5932Id}_{user_B5524_DoA1_5932Id}";
                if (!existingRoleKeys.Contains(key_B5524_DoA1_5932))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA1 - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_DoA1_5932Id}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_DoA1_5932Id,
                        UserId = user_B5524_DoA1_5932Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5524_DoA1_5932); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA1")) missingRoles.Add("DoA1");
                if (!emailToUserId.ContainsKey("sirpaj@unops.org")) missingUsers.Add("sirpaj@unops.org");
            }
            // DoA2: saminak@unops.org
            if (roleNameToId.TryGetValue("DoA2", out var role_B5524_DoA2_1626Id) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5524_DoA2_1626Id))
            {
                var key_B5524_DoA2_1626 = $"{orgUnit_B5524Id}_{role_B5524_DoA2_1626Id}_{user_B5524_DoA2_1626Id}";
                if (!existingRoleKeys.Contains(key_B5524_DoA2_1626))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA2 - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_DoA2_1626Id}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_DoA2_1626Id,
                        UserId = user_B5524_DoA2_1626Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5524_DoA2_1626); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA2")) missingRoles.Add("DoA2");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // DoA3: clarag@unops.org
            if (roleNameToId.TryGetValue("DoA3", out var role_B5524_DoA3_3918Id) &&
                emailToUserId.TryGetValue("clarag@unops.org", out var user_B5524_DoA3_3918Id))
            {
                var key_B5524_DoA3_3918 = $"{orgUnit_B5524Id}_{role_B5524_DoA3_3918Id}_{user_B5524_DoA3_3918Id}";
                if (!existingRoleKeys.Contains(key_B5524_DoA3_3918))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"DoA3 - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_DoA3_3918Id}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_DoA3_3918Id,
                        UserId = user_B5524_DoA3_3918Id,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                    existingRoleKeys.Add(key_B5524_DoA3_3918); // Prevent duplicates within this run
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("DoA3")) missingRoles.Add("DoA3");
                if (!emailToUserId.ContainsKey("clarag@unops.org")) missingUsers.Add("clarag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5524");
        }

        // Add all new roles
        if (rolesToAdd.Any())
        {
            await context.EntityUserRoles.AddRangeAsync(rolesToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"Added {rolesToAdd.Count} new DoA EntityUserRole records for OrganizationHierarchy.");
        }
        else
        {
            Console.WriteLine("No new DoA EntityUserRole records to add.");
        }
        
        if (skippedCount > 0)
        {
            Console.WriteLine($"Skipped {skippedCount} existing DoA EntityUserRole records.");
        }
        
        if (missingOrgUnits.Any())
        {
            Console.WriteLine($"Warning: Could not find OrgUnit codes: {string.Join(", ", missingOrgUnits)}");
        }
        
        if (missingUsers.Any())
        {
            Console.WriteLine($"Warning: Could not find users with emails: {string.Join(", ", missingUsers)}");
        }
        
        if (missingRoles.Any())
        {
            Console.WriteLine($"Warning: Could not find EntityRoles: {string.Join(", ", missingRoles)}");
        }
        
        Console.WriteLine("OrgUnitDOARolesSeeder completed.");
    }
}
