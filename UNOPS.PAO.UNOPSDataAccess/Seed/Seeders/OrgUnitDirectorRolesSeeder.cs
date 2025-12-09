using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds EntityUserRole records linking OrganizationHierarchy entities to Users via EntityRoles.
/// Generated from OrgUnit_Director_DeputyDirector_etc - ImportFile.csv
/// </summary>
public class OrgUnitDirectorRolesSeeder
{
    public static async Task SeedOrgUnitDirectorRolesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("Starting OrgUnitDirectorRolesSeeder...");
        
        // Get all EntityRoles for OrganizationHierarchy
        var entityRoles = await context.EntityRoles
            .Where(er => er.EntityType == "OrganizationHierarchy" && !er.IsDeleted)
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
        
        // Process each org unit and its role assignments

        // B0002
        if (codeToOrgUnitId.TryGetValue("B0002", out var orgUnit_B0002Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0002_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B0002_Region_DirectorId))
            {
                var key_B0002_Region_Director = $"{orgUnit_B0002Id}_{role_B0002_Region_DirectorId}_{user_B0002_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0002_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0002Id} - {user_B0002_Region_DirectorId}",
                        EntityId = orgUnit_B0002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0002_Region_DirectorId,
                        UserId = user_B0002_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: jorge.moreiradasilva@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0002_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jorge.moreiradasilva@unops.org", out var user_B0002_OrgUnit_DirectorId))
            {
                var key_B0002_OrgUnit_Director = $"{orgUnit_B0002Id}_{role_B0002_OrgUnit_DirectorId}_{user_B0002_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0002_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0002Id} - {user_B0002_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0002_OrgUnit_DirectorId,
                        UserId = user_B0002_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jorge.moreiradasilva@unops.org")) missingUsers.Add("jorge.moreiradasilva@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0002");
        }

        // B0004
        if (codeToOrgUnitId.TryGetValue("B0004", out var orgUnit_B0004Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0004_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0004_Region_DirectorId))
            {
                var key_B0004_Region_Director = $"{orgUnit_B0004Id}_{role_B0004_Region_DirectorId}_{user_B0004_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0004_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0004Id} - {user_B0004_Region_DirectorId}",
                        EntityId = orgUnit_B0004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0004_Region_DirectorId,
                        UserId = user_B0004_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: raady@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0004_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("raady@unops.org", out var user_B0004_OrgUnit_DirectorId))
            {
                var key_B0004_OrgUnit_Director = $"{orgUnit_B0004Id}_{role_B0004_OrgUnit_DirectorId}_{user_B0004_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0004_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0004Id} - {user_B0004_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0004_OrgUnit_DirectorId,
                        UserId = user_B0004_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("raady@unops.org")) missingUsers.Add("raady@unops.org");
            }
            // OrgUnit Deputy Director: svene@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0004_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("svene@unops.org", out var user_B0004_OrgUnit_Deputy_DirectorId))
            {
                var key_B0004_OrgUnit_Deputy_Director = $"{orgUnit_B0004Id}_{role_B0004_OrgUnit_Deputy_DirectorId}_{user_B0004_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0004_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0004Id} - {user_B0004_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0004_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0004_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("svene@unops.org")) missingUsers.Add("svene@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0004");
        }

        // B0005
        if (codeToOrgUnitId.TryGetValue("B0005", out var orgUnit_B0005Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0005_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0005_Region_DirectorId))
            {
                var key_B0005_Region_Director = $"{orgUnit_B0005Id}_{role_B0005_Region_DirectorId}_{user_B0005_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0005_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0005Id} - {user_B0005_Region_DirectorId}",
                        EntityId = orgUnit_B0005Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0005_Region_DirectorId,
                        UserId = user_B0005_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: raady@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0005_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("raady@unops.org", out var user_B0005_OrgUnit_DirectorId))
            {
                var key_B0005_OrgUnit_Director = $"{orgUnit_B0005Id}_{role_B0005_OrgUnit_DirectorId}_{user_B0005_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0005_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0005Id} - {user_B0005_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0005Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0005_OrgUnit_DirectorId,
                        UserId = user_B0005_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("raady@unops.org")) missingUsers.Add("raady@unops.org");
            }
            // OrgUnit Deputy Director: sabinek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0005_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("sabinek@unops.org", out var user_B0005_OrgUnit_Deputy_DirectorId))
            {
                var key_B0005_OrgUnit_Deputy_Director = $"{orgUnit_B0005Id}_{role_B0005_OrgUnit_Deputy_DirectorId}_{user_B0005_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0005_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0005Id} - {user_B0005_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0005Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0005_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0005_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("sabinek@unops.org")) missingUsers.Add("sabinek@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0005");
        }

        // B0006
        if (codeToOrgUnitId.TryGetValue("B0006", out var orgUnit_B0006Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0006_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B0006_Region_DirectorId))
            {
                var key_B0006_Region_Director = $"{orgUnit_B0006Id}_{role_B0006_Region_DirectorId}_{user_B0006_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0006_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0006Id} - {user_B0006_Region_DirectorId}",
                        EntityId = orgUnit_B0006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0006_Region_DirectorId,
                        UserId = user_B0006_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: thomasl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0006_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("thomasl@unops.org", out var user_B0006_OrgUnit_DirectorId))
            {
                var key_B0006_OrgUnit_Director = $"{orgUnit_B0006Id}_{role_B0006_OrgUnit_DirectorId}_{user_B0006_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0006_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0006Id} - {user_B0006_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0006_OrgUnit_DirectorId,
                        UserId = user_B0006_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("thomasl@unops.org")) missingUsers.Add("thomasl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0006");
        }

        // B0009
        if (codeToOrgUnitId.TryGetValue("B0009", out var orgUnit_B0009Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0009_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0009_Region_DirectorId))
            {
                var key_B0009_Region_Director = $"{orgUnit_B0009Id}_{role_B0009_Region_DirectorId}_{user_B0009_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0009_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0009Id} - {user_B0009_Region_DirectorId}",
                        EntityId = orgUnit_B0009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0009_Region_DirectorId,
                        UserId = user_B0009_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: tushard@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0009_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tushard@unops.org", out var user_B0009_OrgUnit_DirectorId))
            {
                var key_B0009_OrgUnit_Director = $"{orgUnit_B0009Id}_{role_B0009_OrgUnit_DirectorId}_{user_B0009_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0009_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0009Id} - {user_B0009_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0009_OrgUnit_DirectorId,
                        UserId = user_B0009_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tushard@unops.org")) missingUsers.Add("tushard@unops.org");
            }
            // OrgUnit Deputy Director: issamam@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0009_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("issamam@unops.org", out var user_B0009_OrgUnit_Deputy_DirectorId))
            {
                var key_B0009_OrgUnit_Deputy_Director = $"{orgUnit_B0009Id}_{role_B0009_OrgUnit_Deputy_DirectorId}_{user_B0009_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0009_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0009Id} - {user_B0009_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0009_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0009_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("issamam@unops.org")) missingUsers.Add("issamam@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0009");
        }

        // B0012
        if (codeToOrgUnitId.TryGetValue("B0012", out var orgUnit_B0012Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0012_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0012_Region_DirectorId))
            {
                var key_B0012_Region_Director = $"{orgUnit_B0012Id}_{role_B0012_Region_DirectorId}_{user_B0012_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0012_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0012Id} - {user_B0012_Region_DirectorId}",
                        EntityId = orgUnit_B0012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0012_Region_DirectorId,
                        UserId = user_B0012_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: anneclaireh@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0012_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("anneclaireh@unops.org", out var user_B0012_OrgUnit_DirectorId))
            {
                var key_B0012_OrgUnit_Director = $"{orgUnit_B0012Id}_{role_B0012_OrgUnit_DirectorId}_{user_B0012_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0012_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0012Id} - {user_B0012_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0012_OrgUnit_DirectorId,
                        UserId = user_B0012_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("anneclaireh@unops.org")) missingUsers.Add("anneclaireh@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0012");
        }

        // B0036
        if (codeToOrgUnitId.TryGetValue("B0036", out var orgUnit_B0036Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0036_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B0036_Region_DirectorId))
            {
                var key_B0036_Region_Director = $"{orgUnit_B0036Id}_{role_B0036_Region_DirectorId}_{user_B0036_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0036_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0036Id} - {user_B0036_Region_DirectorId}",
                        EntityId = orgUnit_B0036Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0036_Region_DirectorId,
                        UserId = user_B0036_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: thomasl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0036_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("thomasl@unops.org", out var user_B0036_OrgUnit_DirectorId))
            {
                var key_B0036_OrgUnit_Director = $"{orgUnit_B0036Id}_{role_B0036_OrgUnit_DirectorId}_{user_B0036_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0036_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0036Id} - {user_B0036_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0036Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0036_OrgUnit_DirectorId,
                        UserId = user_B0036_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("thomasl@unops.org")) missingUsers.Add("thomasl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0036");
        }

        // B0037
        if (codeToOrgUnitId.TryGetValue("B0037", out var orgUnit_B0037Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0037_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B0037_Region_DirectorId))
            {
                var key_B0037_Region_Director = $"{orgUnit_B0037Id}_{role_B0037_Region_DirectorId}_{user_B0037_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0037_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0037Id} - {user_B0037_Region_DirectorId}",
                        EntityId = orgUnit_B0037Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0037_Region_DirectorId,
                        UserId = user_B0037_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: thomasl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0037_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("thomasl@unops.org", out var user_B0037_OrgUnit_DirectorId))
            {
                var key_B0037_OrgUnit_Director = $"{orgUnit_B0037Id}_{role_B0037_OrgUnit_DirectorId}_{user_B0037_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0037_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0037Id} - {user_B0037_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0037Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0037_OrgUnit_DirectorId,
                        UserId = user_B0037_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("thomasl@unops.org")) missingUsers.Add("thomasl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0037");
        }

        // B0038
        if (codeToOrgUnitId.TryGetValue("B0038", out var orgUnit_B0038Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0038_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B0038_Region_DirectorId))
            {
                var key_B0038_Region_Director = $"{orgUnit_B0038Id}_{role_B0038_Region_DirectorId}_{user_B0038_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0038_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0038Id} - {user_B0038_Region_DirectorId}",
                        EntityId = orgUnit_B0038Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0038_Region_DirectorId,
                        UserId = user_B0038_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: thomasl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0038_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("thomasl@unops.org", out var user_B0038_OrgUnit_DirectorId))
            {
                var key_B0038_OrgUnit_Director = $"{orgUnit_B0038Id}_{role_B0038_OrgUnit_DirectorId}_{user_B0038_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0038_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0038Id} - {user_B0038_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0038Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0038_OrgUnit_DirectorId,
                        UserId = user_B0038_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("thomasl@unops.org")) missingUsers.Add("thomasl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0038");
        }

        // B0047
        if (codeToOrgUnitId.TryGetValue("B0047", out var orgUnit_B0047Id))
        {
            // Region Director: kirstined@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0047_Region_DirectorId) &&
                emailToUserId.TryGetValue("kirstined@unops.org", out var user_B0047_Region_DirectorId))
            {
                var key_B0047_Region_Director = $"{orgUnit_B0047Id}_{role_B0047_Region_DirectorId}_{user_B0047_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0047_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0047Id} - {user_B0047_Region_DirectorId}",
                        EntityId = orgUnit_B0047Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0047_Region_DirectorId,
                        UserId = user_B0047_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("kirstined@unops.org")) missingUsers.Add("kirstined@unops.org");
            }
            // OrgUnit Director: freyavg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0047_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("freyavg@unops.org", out var user_B0047_OrgUnit_DirectorId))
            {
                var key_B0047_OrgUnit_Director = $"{orgUnit_B0047Id}_{role_B0047_OrgUnit_DirectorId}_{user_B0047_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0047_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0047Id} - {user_B0047_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0047Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0047_OrgUnit_DirectorId,
                        UserId = user_B0047_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("freyavg@unops.org")) missingUsers.Add("freyavg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0047");
        }

        // B0048
        if (codeToOrgUnitId.TryGetValue("B0048", out var orgUnit_B0048Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0048_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B0048_Region_DirectorId))
            {
                var key_B0048_Region_Director = $"{orgUnit_B0048Id}_{role_B0048_Region_DirectorId}_{user_B0048_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0048_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0048Id} - {user_B0048_Region_DirectorId}",
                        EntityId = orgUnit_B0048Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0048_Region_DirectorId,
                        UserId = user_B0048_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: peterb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0048_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("peterb@unops.org", out var user_B0048_OrgUnit_DirectorId))
            {
                var key_B0048_OrgUnit_Director = $"{orgUnit_B0048Id}_{role_B0048_OrgUnit_DirectorId}_{user_B0048_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0048_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0048Id} - {user_B0048_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0048Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0048_OrgUnit_DirectorId,
                        UserId = user_B0048_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("peterb@unops.org")) missingUsers.Add("peterb@unops.org");
            }
            // OrgUnit Deputy Director: cilliano@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0048_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("cilliano@unops.org", out var user_B0048_OrgUnit_Deputy_DirectorId))
            {
                var key_B0048_OrgUnit_Deputy_Director = $"{orgUnit_B0048Id}_{role_B0048_OrgUnit_Deputy_DirectorId}_{user_B0048_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0048_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0048Id} - {user_B0048_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0048Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0048_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0048_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("cilliano@unops.org")) missingUsers.Add("cilliano@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0048");
        }

        // B0049
        if (codeToOrgUnitId.TryGetValue("B0049", out var orgUnit_B0049Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0049_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0049_Region_DirectorId))
            {
                var key_B0049_Region_Director = $"{orgUnit_B0049Id}_{role_B0049_Region_DirectorId}_{user_B0049_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0049_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0049Id} - {user_B0049_Region_DirectorId}",
                        EntityId = orgUnit_B0049Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0049_Region_DirectorId,
                        UserId = user_B0049_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: raady@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0049_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("raady@unops.org", out var user_B0049_OrgUnit_DirectorId))
            {
                var key_B0049_OrgUnit_Director = $"{orgUnit_B0049Id}_{role_B0049_OrgUnit_DirectorId}_{user_B0049_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0049_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0049Id} - {user_B0049_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0049Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0049_OrgUnit_DirectorId,
                        UserId = user_B0049_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("raady@unops.org")) missingUsers.Add("raady@unops.org");
            }
            // OrgUnit Deputy Director: davidc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0049_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("davidc@unops.org", out var user_B0049_OrgUnit_Deputy_DirectorId))
            {
                var key_B0049_OrgUnit_Deputy_Director = $"{orgUnit_B0049Id}_{role_B0049_OrgUnit_Deputy_DirectorId}_{user_B0049_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0049_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0049Id} - {user_B0049_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0049Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0049_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0049_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("davidc@unops.org")) missingUsers.Add("davidc@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0049");
        }

        // B0050
        if (codeToOrgUnitId.TryGetValue("B0050", out var orgUnit_B0050Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0050_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B0050_Region_DirectorId))
            {
                var key_B0050_Region_Director = $"{orgUnit_B0050Id}_{role_B0050_Region_DirectorId}_{user_B0050_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0050_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_Region_DirectorId}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_Region_DirectorId,
                        UserId = user_B0050_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B0050_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B0050_Region_Deputy_DirectorId))
            {
                var key_B0050_Region_Deputy_Director = $"{orgUnit_B0050Id}_{role_B0050_Region_Deputy_DirectorId}_{user_B0050_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0050_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_Region_Deputy_DirectorId,
                        UserId = user_B0050_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // OrgUnit Director: timl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0050_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B0050_OrgUnit_DirectorId))
            {
                var key_B0050_OrgUnit_Director = $"{orgUnit_B0050Id}_{role_B0050_OrgUnit_DirectorId}_{user_B0050_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0050_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_OrgUnit_DirectorId,
                        UserId = user_B0050_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // OrgUnit Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0050_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B0050_OrgUnit_Deputy_DirectorId))
            {
                var key_B0050_OrgUnit_Deputy_Director = $"{orgUnit_B0050Id}_{role_B0050_OrgUnit_Deputy_DirectorId}_{user_B0050_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0050_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0050Id} - {user_B0050_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0050Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0050_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0050_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0050");
        }

        // B0051
        if (codeToOrgUnitId.TryGetValue("B0051", out var orgUnit_B0051Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0051_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B0051_Region_DirectorId))
            {
                var key_B0051_Region_Director = $"{orgUnit_B0051Id}_{role_B0051_Region_DirectorId}_{user_B0051_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0051_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0051Id} - {user_B0051_Region_DirectorId}",
                        EntityId = orgUnit_B0051Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0051_Region_DirectorId,
                        UserId = user_B0051_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B0051_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B0051_Region_Deputy_DirectorId))
            {
                var key_B0051_Region_Deputy_Director = $"{orgUnit_B0051Id}_{role_B0051_Region_Deputy_DirectorId}_{user_B0051_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0051_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B0051Id} - {user_B0051_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B0051Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0051_Region_Deputy_DirectorId,
                        UserId = user_B0051_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // OrgUnit Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0051_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B0051_OrgUnit_DirectorId))
            {
                var key_B0051_OrgUnit_Director = $"{orgUnit_B0051Id}_{role_B0051_OrgUnit_DirectorId}_{user_B0051_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0051_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0051Id} - {user_B0051_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0051Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0051_OrgUnit_DirectorId,
                        UserId = user_B0051_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // OrgUnit Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0051_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B0051_OrgUnit_Deputy_DirectorId))
            {
                var key_B0051_OrgUnit_Deputy_Director = $"{orgUnit_B0051Id}_{role_B0051_OrgUnit_Deputy_DirectorId}_{user_B0051_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0051_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0051Id} - {user_B0051_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0051Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0051_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0051_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0051");
        }

        // B0053
        if (codeToOrgUnitId.TryGetValue("B0053", out var orgUnit_B0053Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0053_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B0053_Region_DirectorId))
            {
                var key_B0053_Region_Director = $"{orgUnit_B0053Id}_{role_B0053_Region_DirectorId}_{user_B0053_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0053_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0053Id} - {user_B0053_Region_DirectorId}",
                        EntityId = orgUnit_B0053Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0053_Region_DirectorId,
                        UserId = user_B0053_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // OrgUnit Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0053_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B0053_OrgUnit_DirectorId))
            {
                var key_B0053_OrgUnit_Director = $"{orgUnit_B0053Id}_{role_B0053_OrgUnit_DirectorId}_{user_B0053_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0053_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0053Id} - {user_B0053_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0053Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0053_OrgUnit_DirectorId,
                        UserId = user_B0053_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0053");
        }

        // B0054
        if (codeToOrgUnitId.TryGetValue("B0054", out var orgUnit_B0054Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0054_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B0054_Region_DirectorId))
            {
                var key_B0054_Region_Director = $"{orgUnit_B0054Id}_{role_B0054_Region_DirectorId}_{user_B0054_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0054_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_Region_DirectorId}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_Region_DirectorId,
                        UserId = user_B0054_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B0054_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B0054_Region_Deputy_DirectorId))
            {
                var key_B0054_Region_Deputy_Director = $"{orgUnit_B0054Id}_{role_B0054_Region_Deputy_DirectorId}_{user_B0054_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0054_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_Region_Deputy_DirectorId,
                        UserId = user_B0054_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: fabriziof@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0054_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("fabriziof@unops.org", out var user_B0054_OrgUnit_DirectorId))
            {
                var key_B0054_OrgUnit_Director = $"{orgUnit_B0054Id}_{role_B0054_OrgUnit_DirectorId}_{user_B0054_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0054_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_OrgUnit_DirectorId,
                        UserId = user_B0054_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("fabriziof@unops.org")) missingUsers.Add("fabriziof@unops.org");
            }
            // OrgUnit Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0054_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B0054_OrgUnit_Deputy_DirectorId))
            {
                var key_B0054_OrgUnit_Deputy_Director = $"{orgUnit_B0054Id}_{role_B0054_OrgUnit_Deputy_DirectorId}_{user_B0054_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0054_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0054Id} - {user_B0054_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0054Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0054_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0054_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0054");
        }

        // B0058
        if (codeToOrgUnitId.TryGetValue("B0058", out var orgUnit_B0058Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0058_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0058_Region_DirectorId))
            {
                var key_B0058_Region_Director = $"{orgUnit_B0058Id}_{role_B0058_Region_DirectorId}_{user_B0058_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0058_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0058Id} - {user_B0058_Region_DirectorId}",
                        EntityId = orgUnit_B0058Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0058_Region_DirectorId,
                        UserId = user_B0058_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: raady@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0058_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("raady@unops.org", out var user_B0058_OrgUnit_DirectorId))
            {
                var key_B0058_OrgUnit_Director = $"{orgUnit_B0058Id}_{role_B0058_OrgUnit_DirectorId}_{user_B0058_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0058_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0058Id} - {user_B0058_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0058Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0058_OrgUnit_DirectorId,
                        UserId = user_B0058_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("raady@unops.org")) missingUsers.Add("raady@unops.org");
            }
            // OrgUnit Deputy Director: jesperla@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0058_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jesperla@unops.org", out var user_B0058_OrgUnit_Deputy_DirectorId))
            {
                var key_B0058_OrgUnit_Deputy_Director = $"{orgUnit_B0058Id}_{role_B0058_OrgUnit_Deputy_DirectorId}_{user_B0058_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0058_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0058Id} - {user_B0058_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0058Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0058_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0058_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jesperla@unops.org")) missingUsers.Add("jesperla@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0058");
        }

        // B0059
        if (codeToOrgUnitId.TryGetValue("B0059", out var orgUnit_B0059Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0059_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0059_Region_DirectorId))
            {
                var key_B0059_Region_Director = $"{orgUnit_B0059Id}_{role_B0059_Region_DirectorId}_{user_B0059_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0059_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0059Id} - {user_B0059_Region_DirectorId}",
                        EntityId = orgUnit_B0059Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0059_Region_DirectorId,
                        UserId = user_B0059_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: stevenc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0059_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("stevenc@unops.org", out var user_B0059_OrgUnit_DirectorId))
            {
                var key_B0059_OrgUnit_Director = $"{orgUnit_B0059Id}_{role_B0059_OrgUnit_DirectorId}_{user_B0059_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0059_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0059Id} - {user_B0059_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0059Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0059_OrgUnit_DirectorId,
                        UserId = user_B0059_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("stevenc@unops.org")) missingUsers.Add("stevenc@unops.org");
            }
            // OrgUnit Deputy Director: besnikej@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B0059_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("besnikej@unops.org", out var user_B0059_OrgUnit_Deputy_DirectorId))
            {
                var key_B0059_OrgUnit_Deputy_Director = $"{orgUnit_B0059Id}_{role_B0059_OrgUnit_Deputy_DirectorId}_{user_B0059_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0059_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B0059Id} - {user_B0059_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B0059Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0059_OrgUnit_Deputy_DirectorId,
                        UserId = user_B0059_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("besnikej@unops.org")) missingUsers.Add("besnikej@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0059");
        }

        // B0063
        if (codeToOrgUnitId.TryGetValue("B0063", out var orgUnit_B0063Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0063_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0063_Region_DirectorId))
            {
                var key_B0063_Region_Director = $"{orgUnit_B0063Id}_{role_B0063_Region_DirectorId}_{user_B0063_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0063_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0063Id} - {user_B0063_Region_DirectorId}",
                        EntityId = orgUnit_B0063Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0063_Region_DirectorId,
                        UserId = user_B0063_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: yngvilf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0063_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("yngvilf@unops.org", out var user_B0063_OrgUnit_DirectorId))
            {
                var key_B0063_OrgUnit_Director = $"{orgUnit_B0063Id}_{role_B0063_OrgUnit_DirectorId}_{user_B0063_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0063_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0063Id} - {user_B0063_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0063Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0063_OrgUnit_DirectorId,
                        UserId = user_B0063_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("yngvilf@unops.org")) missingUsers.Add("yngvilf@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0063");
        }

        // B0064
        if (codeToOrgUnitId.TryGetValue("B0064", out var orgUnit_B0064Id))
        {
            // Region Director: sonjalk@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0064_Region_DirectorId) &&
                emailToUserId.TryGetValue("sonjalk@unops.org", out var user_B0064_Region_DirectorId))
            {
                var key_B0064_Region_Director = $"{orgUnit_B0064Id}_{role_B0064_Region_DirectorId}_{user_B0064_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0064_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0064Id} - {user_B0064_Region_DirectorId}",
                        EntityId = orgUnit_B0064Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0064_Region_DirectorId,
                        UserId = user_B0064_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sonjalk@unops.org")) missingUsers.Add("sonjalk@unops.org");
            }
            // OrgUnit Director: orenng@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0064_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("orenng@unops.org", out var user_B0064_OrgUnit_DirectorId))
            {
                var key_B0064_OrgUnit_Director = $"{orgUnit_B0064Id}_{role_B0064_OrgUnit_DirectorId}_{user_B0064_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0064_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0064Id} - {user_B0064_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0064Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0064_OrgUnit_DirectorId,
                        UserId = user_B0064_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("orenng@unops.org")) missingUsers.Add("orenng@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0064");
        }

        // B0201
        if (codeToOrgUnitId.TryGetValue("B0201", out var orgUnit_B0201Id))
        {
            // Region Director: abdould@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B0201_Region_DirectorId) &&
                emailToUserId.TryGetValue("abdould@unops.org", out var user_B0201_Region_DirectorId))
            {
                var key_B0201_Region_Director = $"{orgUnit_B0201Id}_{role_B0201_Region_DirectorId}_{user_B0201_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0201_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B0201Id} - {user_B0201_Region_DirectorId}",
                        EntityId = orgUnit_B0201Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0201_Region_DirectorId,
                        UserId = user_B0201_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("abdould@unops.org")) missingUsers.Add("abdould@unops.org");
            }
            // OrgUnit Director: berkanmv@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B0201_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("berkanmv@unops.org", out var user_B0201_OrgUnit_DirectorId))
            {
                var key_B0201_OrgUnit_Director = $"{orgUnit_B0201Id}_{role_B0201_OrgUnit_DirectorId}_{user_B0201_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B0201_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B0201Id} - {user_B0201_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B0201Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B0201_OrgUnit_DirectorId,
                        UserId = user_B0201_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("berkanmv@unops.org")) missingUsers.Add("berkanmv@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B0201");
        }

        // B3603
        if (codeToOrgUnitId.TryGetValue("B3603", out var orgUnit_B3603Id))
        {
            // Region Director: hilaryb@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B3603_Region_DirectorId) &&
                emailToUserId.TryGetValue("hilaryb@unops.org", out var user_B3603_Region_DirectorId))
            {
                var key_B3603_Region_Director = $"{orgUnit_B3603Id}_{role_B3603_Region_DirectorId}_{user_B3603_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B3603_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B3603Id} - {user_B3603_Region_DirectorId}",
                        EntityId = orgUnit_B3603Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B3603_Region_DirectorId,
                        UserId = user_B3603_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("hilaryb@unops.org")) missingUsers.Add("hilaryb@unops.org");
            }
            // OrgUnit Director: karls@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B3603_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("karls@unops.org", out var user_B3603_OrgUnit_DirectorId))
            {
                var key_B3603_OrgUnit_Director = $"{orgUnit_B3603Id}_{role_B3603_OrgUnit_DirectorId}_{user_B3603_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B3603_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B3603Id} - {user_B3603_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B3603Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B3603_OrgUnit_DirectorId,
                        UserId = user_B3603_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("karls@unops.org")) missingUsers.Add("karls@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B3603");
        }

        // B5001
        if (codeToOrgUnitId.TryGetValue("B5001", out var orgUnit_B5001Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5001_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5001_Region_DirectorId))
            {
                var key_B5001_Region_Director = $"{orgUnit_B5001Id}_{role_B5001_Region_DirectorId}_{user_B5001_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5001_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_Region_DirectorId}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_Region_DirectorId,
                        UserId = user_B5001_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5001_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5001_Hub_DirectorId))
            {
                var key_B5001_Hub_Director = $"{orgUnit_B5001Id}_{role_B5001_Hub_DirectorId}_{user_B5001_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5001_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_Hub_DirectorId}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_Hub_DirectorId,
                        UserId = user_B5001_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5001_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5001_Hub_Deputy_DirectorId))
            {
                var key_B5001_Hub_Deputy_Director = $"{orgUnit_B5001Id}_{role_B5001_Hub_Deputy_DirectorId}_{user_B5001_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5001_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_Hub_Deputy_DirectorId,
                        UserId = user_B5001_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5001_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5001_OrgUnit_DirectorId))
            {
                var key_B5001_OrgUnit_Director = $"{orgUnit_B5001Id}_{role_B5001_OrgUnit_DirectorId}_{user_B5001_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5001_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5001Id} - {user_B5001_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5001Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5001_OrgUnit_DirectorId,
                        UserId = user_B5001_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5001");
        }

        // B5002
        if (codeToOrgUnitId.TryGetValue("B5002", out var orgUnit_B5002Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5002_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5002_Region_DirectorId))
            {
                var key_B5002_Region_Director = $"{orgUnit_B5002Id}_{role_B5002_Region_DirectorId}_{user_B5002_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5002_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_Region_DirectorId}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_Region_DirectorId,
                        UserId = user_B5002_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: andrewk@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5002_Hub_DirectorId) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5002_Hub_DirectorId))
            {
                var key_B5002_Hub_Director = $"{orgUnit_B5002Id}_{role_B5002_Hub_DirectorId}_{user_B5002_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5002_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_Hub_DirectorId}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_Hub_DirectorId,
                        UserId = user_B5002_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // OrgUnit Director: andrewk@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5002_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5002_OrgUnit_DirectorId))
            {
                var key_B5002_OrgUnit_Director = $"{orgUnit_B5002Id}_{role_B5002_OrgUnit_DirectorId}_{user_B5002_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5002_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_OrgUnit_DirectorId,
                        UserId = user_B5002_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // OrgUnit Deputy Director: katrinl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5002_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("katrinl@unops.org", out var user_B5002_OrgUnit_Deputy_DirectorId))
            {
                var key_B5002_OrgUnit_Deputy_Director = $"{orgUnit_B5002Id}_{role_B5002_OrgUnit_Deputy_DirectorId}_{user_B5002_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5002_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5002Id} - {user_B5002_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5002Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5002_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5002_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("katrinl@unops.org")) missingUsers.Add("katrinl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5002");
        }

        // B5003
        if (codeToOrgUnitId.TryGetValue("B5003", out var orgUnit_B5003Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5003_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5003_Region_DirectorId))
            {
                var key_B5003_Region_Director = $"{orgUnit_B5003Id}_{role_B5003_Region_DirectorId}_{user_B5003_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5003_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_Region_DirectorId}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_Region_DirectorId,
                        UserId = user_B5003_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5003_Hub_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5003_Hub_DirectorId))
            {
                var key_B5003_Hub_Director = $"{orgUnit_B5003Id}_{role_B5003_Hub_DirectorId}_{user_B5003_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5003_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_Hub_DirectorId}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_Hub_DirectorId,
                        UserId = user_B5003_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // Hub Deputy Director: robertgodin@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5003_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5003_Hub_Deputy_DirectorId))
            {
                var key_B5003_Hub_Deputy_Director = $"{orgUnit_B5003Id}_{role_B5003_Hub_Deputy_DirectorId}_{user_B5003_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5003_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_Hub_Deputy_DirectorId,
                        UserId = user_B5003_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // OrgUnit Director: edrissr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5003_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("edrissr@unops.org", out var user_B5003_OrgUnit_DirectorId))
            {
                var key_B5003_OrgUnit_Director = $"{orgUnit_B5003Id}_{role_B5003_OrgUnit_DirectorId}_{user_B5003_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5003_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5003Id} - {user_B5003_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5003Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5003_OrgUnit_DirectorId,
                        UserId = user_B5003_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("edrissr@unops.org")) missingUsers.Add("edrissr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5003");
        }

        // B5004
        if (codeToOrgUnitId.TryGetValue("B5004", out var orgUnit_B5004Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5004_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5004_Region_DirectorId))
            {
                var key_B5004_Region_Director = $"{orgUnit_B5004Id}_{role_B5004_Region_DirectorId}_{user_B5004_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5004_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_Region_DirectorId}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_Region_DirectorId,
                        UserId = user_B5004_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5004_Hub_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5004_Hub_DirectorId))
            {
                var key_B5004_Hub_Director = $"{orgUnit_B5004Id}_{role_B5004_Hub_DirectorId}_{user_B5004_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5004_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_Hub_DirectorId}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_Hub_DirectorId,
                        UserId = user_B5004_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // Hub Deputy Director: robertgodin@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5004_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5004_Hub_Deputy_DirectorId))
            {
                var key_B5004_Hub_Deputy_Director = $"{orgUnit_B5004Id}_{role_B5004_Hub_Deputy_DirectorId}_{user_B5004_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5004_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_Hub_Deputy_DirectorId,
                        UserId = user_B5004_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // OrgUnit Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5004_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5004_OrgUnit_DirectorId))
            {
                var key_B5004_OrgUnit_Director = $"{orgUnit_B5004Id}_{role_B5004_OrgUnit_DirectorId}_{user_B5004_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5004_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_OrgUnit_DirectorId,
                        UserId = user_B5004_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // OrgUnit Deputy Director: isabellav@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5004_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("isabellav@unops.org", out var user_B5004_OrgUnit_Deputy_DirectorId))
            {
                var key_B5004_OrgUnit_Deputy_Director = $"{orgUnit_B5004Id}_{role_B5004_OrgUnit_Deputy_DirectorId}_{user_B5004_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5004_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5004Id} - {user_B5004_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5004Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5004_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5004_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("isabellav@unops.org")) missingUsers.Add("isabellav@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5004");
        }

        // B5006
        if (codeToOrgUnitId.TryGetValue("B5006", out var orgUnit_B5006Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5006_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5006_Region_DirectorId))
            {
                var key_B5006_Region_Director = $"{orgUnit_B5006Id}_{role_B5006_Region_DirectorId}_{user_B5006_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5006_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_Region_DirectorId}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_Region_DirectorId,
                        UserId = user_B5006_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5006_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5006_Region_Deputy_DirectorId))
            {
                var key_B5006_Region_Deputy_Director = $"{orgUnit_B5006Id}_{role_B5006_Region_Deputy_DirectorId}_{user_B5006_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5006_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_Region_Deputy_DirectorId,
                        UserId = user_B5006_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5006_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5006_Hub_DirectorId))
            {
                var key_B5006_Hub_Director = $"{orgUnit_B5006Id}_{role_B5006_Hub_DirectorId}_{user_B5006_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5006_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_Hub_DirectorId}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_Hub_DirectorId,
                        UserId = user_B5006_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5006_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5006_Hub_Deputy_DirectorId))
            {
                var key_B5006_Hub_Deputy_Director = $"{orgUnit_B5006Id}_{role_B5006_Hub_Deputy_DirectorId}_{user_B5006_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5006_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_Hub_Deputy_DirectorId,
                        UserId = user_B5006_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5006_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5006_OrgUnit_DirectorId))
            {
                var key_B5006_OrgUnit_Director = $"{orgUnit_B5006Id}_{role_B5006_OrgUnit_DirectorId}_{user_B5006_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5006_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_OrgUnit_DirectorId,
                        UserId = user_B5006_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5006_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5006_OrgUnit_Deputy_DirectorId))
            {
                var key_B5006_OrgUnit_Deputy_Director = $"{orgUnit_B5006Id}_{role_B5006_OrgUnit_Deputy_DirectorId}_{user_B5006_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5006_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5006Id} - {user_B5006_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5006Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5006_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5006_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5006");
        }

        // B5007
        if (codeToOrgUnitId.TryGetValue("B5007", out var orgUnit_B5007Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5007_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5007_Region_DirectorId))
            {
                var key_B5007_Region_Director = $"{orgUnit_B5007Id}_{role_B5007_Region_DirectorId}_{user_B5007_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5007_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_Region_DirectorId}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_Region_DirectorId,
                        UserId = user_B5007_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: andrewk@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5007_Hub_DirectorId) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5007_Hub_DirectorId))
            {
                var key_B5007_Hub_Director = $"{orgUnit_B5007Id}_{role_B5007_Hub_DirectorId}_{user_B5007_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5007_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_Hub_DirectorId}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_Hub_DirectorId,
                        UserId = user_B5007_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
            // OrgUnit Director: andrewk@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5007_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("andrewk@unops.org", out var user_B5007_OrgUnit_DirectorId))
            {
                var key_B5007_OrgUnit_Director = $"{orgUnit_B5007Id}_{role_B5007_OrgUnit_DirectorId}_{user_B5007_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5007_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5007Id} - {user_B5007_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5007Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5007_OrgUnit_DirectorId,
                        UserId = user_B5007_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("andrewk@unops.org")) missingUsers.Add("andrewk@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5007");
        }

        // B5009
        if (codeToOrgUnitId.TryGetValue("B5009", out var orgUnit_B5009Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5009_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5009_Region_DirectorId))
            {
                var key_B5009_Region_Director = $"{orgUnit_B5009Id}_{role_B5009_Region_DirectorId}_{user_B5009_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5009_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5009Id} - {user_B5009_Region_DirectorId}",
                        EntityId = orgUnit_B5009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5009_Region_DirectorId,
                        UserId = user_B5009_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // OrgUnit Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5009_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5009_OrgUnit_DirectorId))
            {
                var key_B5009_OrgUnit_Director = $"{orgUnit_B5009Id}_{role_B5009_OrgUnit_DirectorId}_{user_B5009_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5009_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5009Id} - {user_B5009_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5009_OrgUnit_DirectorId,
                        UserId = user_B5009_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // OrgUnit Deputy Director: robertgodin@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5009_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5009_OrgUnit_Deputy_DirectorId))
            {
                var key_B5009_OrgUnit_Deputy_Director = $"{orgUnit_B5009Id}_{role_B5009_OrgUnit_Deputy_DirectorId}_{user_B5009_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5009_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5009Id} - {user_B5009_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5009Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5009_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5009_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5009");
        }

        // B5010
        if (codeToOrgUnitId.TryGetValue("B5010", out var orgUnit_B5010Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5010_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5010_Region_DirectorId))
            {
                var key_B5010_Region_Director = $"{orgUnit_B5010Id}_{role_B5010_Region_DirectorId}_{user_B5010_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5010_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_Region_DirectorId}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_Region_DirectorId,
                        UserId = user_B5010_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5010_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5010_Region_Deputy_DirectorId))
            {
                var key_B5010_Region_Deputy_Director = $"{orgUnit_B5010Id}_{role_B5010_Region_Deputy_DirectorId}_{user_B5010_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5010_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_Region_Deputy_DirectorId,
                        UserId = user_B5010_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // Hub Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5010_Hub_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5010_Hub_DirectorId))
            {
                var key_B5010_Hub_Director = $"{orgUnit_B5010Id}_{role_B5010_Hub_DirectorId}_{user_B5010_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5010_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_Hub_DirectorId}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_Hub_DirectorId,
                        UserId = user_B5010_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // OrgUnit Director: simonettas@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5010_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5010_OrgUnit_DirectorId))
            {
                var key_B5010_OrgUnit_Director = $"{orgUnit_B5010Id}_{role_B5010_OrgUnit_DirectorId}_{user_B5010_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5010_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_OrgUnit_DirectorId,
                        UserId = user_B5010_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
            // OrgUnit Deputy Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5010_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5010_OrgUnit_Deputy_DirectorId))
            {
                var key_B5010_OrgUnit_Deputy_Director = $"{orgUnit_B5010Id}_{role_B5010_OrgUnit_Deputy_DirectorId}_{user_B5010_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5010_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5010Id} - {user_B5010_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5010Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5010_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5010_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5010");
        }

        // B5011
        if (codeToOrgUnitId.TryGetValue("B5011", out var orgUnit_B5011Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5011_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5011_Region_DirectorId))
            {
                var key_B5011_Region_Director = $"{orgUnit_B5011Id}_{role_B5011_Region_DirectorId}_{user_B5011_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5011_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_Region_DirectorId}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_Region_DirectorId,
                        UserId = user_B5011_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5011_Hub_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5011_Hub_DirectorId))
            {
                var key_B5011_Hub_Director = $"{orgUnit_B5011Id}_{role_B5011_Hub_DirectorId}_{user_B5011_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5011_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_Hub_DirectorId}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_Hub_DirectorId,
                        UserId = user_B5011_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // Hub Deputy Director: robertgodin@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5011_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5011_Hub_Deputy_DirectorId))
            {
                var key_B5011_Hub_Deputy_Director = $"{orgUnit_B5011Id}_{role_B5011_Hub_Deputy_DirectorId}_{user_B5011_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5011_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_Hub_Deputy_DirectorId,
                        UserId = user_B5011_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // OrgUnit Director: nielsg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5011_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nielsg@unops.org", out var user_B5011_OrgUnit_DirectorId))
            {
                var key_B5011_OrgUnit_Director = $"{orgUnit_B5011Id}_{role_B5011_OrgUnit_DirectorId}_{user_B5011_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5011_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5011Id} - {user_B5011_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5011Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5011_OrgUnit_DirectorId,
                        UserId = user_B5011_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nielsg@unops.org")) missingUsers.Add("nielsg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5011");
        }

        // B5012
        if (codeToOrgUnitId.TryGetValue("B5012", out var orgUnit_B5012Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5012_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5012_Region_DirectorId))
            {
                var key_B5012_Region_Director = $"{orgUnit_B5012Id}_{role_B5012_Region_DirectorId}_{user_B5012_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5012_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_Region_DirectorId}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_Region_DirectorId,
                        UserId = user_B5012_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5012_Hub_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5012_Hub_DirectorId))
            {
                var key_B5012_Hub_Director = $"{orgUnit_B5012Id}_{role_B5012_Hub_DirectorId}_{user_B5012_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5012_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_Hub_DirectorId}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_Hub_DirectorId,
                        UserId = user_B5012_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // Hub Deputy Director: robertgodin@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5012_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("robertgodin@unops.org", out var user_B5012_Hub_Deputy_DirectorId))
            {
                var key_B5012_Hub_Deputy_Director = $"{orgUnit_B5012Id}_{role_B5012_Hub_Deputy_DirectorId}_{user_B5012_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5012_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_Hub_Deputy_DirectorId,
                        UserId = user_B5012_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("robertgodin@unops.org")) missingUsers.Add("robertgodin@unops.org");
            }
            // OrgUnit Director: dionyssiag@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5012_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("dionyssiag@unops.org", out var user_B5012_OrgUnit_DirectorId))
            {
                var key_B5012_OrgUnit_Director = $"{orgUnit_B5012Id}_{role_B5012_OrgUnit_DirectorId}_{user_B5012_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5012_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_OrgUnit_DirectorId,
                        UserId = user_B5012_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("dionyssiag@unops.org")) missingUsers.Add("dionyssiag@unops.org");
            }
            // OrgUnit Deputy Director: isabellav@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5012_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("isabellav@unops.org", out var user_B5012_OrgUnit_Deputy_DirectorId))
            {
                var key_B5012_OrgUnit_Deputy_Director = $"{orgUnit_B5012Id}_{role_B5012_OrgUnit_Deputy_DirectorId}_{user_B5012_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5012_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5012Id} - {user_B5012_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5012Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5012_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5012_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("isabellav@unops.org")) missingUsers.Add("isabellav@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5012");
        }

        // B5014
        if (codeToOrgUnitId.TryGetValue("B5014", out var orgUnit_B5014Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5014_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5014_Region_DirectorId))
            {
                var key_B5014_Region_Director = $"{orgUnit_B5014Id}_{role_B5014_Region_DirectorId}_{user_B5014_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5014_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_Region_DirectorId}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_Region_DirectorId,
                        UserId = user_B5014_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5014_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5014_Hub_DirectorId))
            {
                var key_B5014_Hub_Director = $"{orgUnit_B5014Id}_{role_B5014_Hub_DirectorId}_{user_B5014_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5014_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_Hub_DirectorId}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_Hub_DirectorId,
                        UserId = user_B5014_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5014_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5014_Hub_Deputy_DirectorId))
            {
                var key_B5014_Hub_Deputy_Director = $"{orgUnit_B5014Id}_{role_B5014_Hub_Deputy_DirectorId}_{user_B5014_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5014_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_Hub_Deputy_DirectorId,
                        UserId = user_B5014_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5014_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5014_OrgUnit_DirectorId))
            {
                var key_B5014_OrgUnit_Director = $"{orgUnit_B5014Id}_{role_B5014_OrgUnit_DirectorId}_{user_B5014_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5014_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_OrgUnit_DirectorId,
                        UserId = user_B5014_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5014_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5014_OrgUnit_Deputy_DirectorId))
            {
                var key_B5014_OrgUnit_Deputy_Director = $"{orgUnit_B5014Id}_{role_B5014_OrgUnit_Deputy_DirectorId}_{user_B5014_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5014_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5014Id} - {user_B5014_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5014Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5014_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5014_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5014");
        }

        // B5015
        if (codeToOrgUnitId.TryGetValue("B5015", out var orgUnit_B5015Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5015_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5015_Region_DirectorId))
            {
                var key_B5015_Region_Director = $"{orgUnit_B5015Id}_{role_B5015_Region_DirectorId}_{user_B5015_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5015_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_Region_DirectorId}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_Region_DirectorId,
                        UserId = user_B5015_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5015_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5015_Hub_DirectorId))
            {
                var key_B5015_Hub_Director = $"{orgUnit_B5015Id}_{role_B5015_Hub_DirectorId}_{user_B5015_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5015_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_Hub_DirectorId}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_Hub_DirectorId,
                        UserId = user_B5015_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5015_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5015_Hub_Deputy_DirectorId))
            {
                var key_B5015_Hub_Deputy_Director = $"{orgUnit_B5015Id}_{role_B5015_Hub_Deputy_DirectorId}_{user_B5015_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5015_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_Hub_Deputy_DirectorId,
                        UserId = user_B5015_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5015_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5015_OrgUnit_DirectorId))
            {
                var key_B5015_OrgUnit_Director = $"{orgUnit_B5015Id}_{role_B5015_OrgUnit_DirectorId}_{user_B5015_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5015_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_OrgUnit_DirectorId,
                        UserId = user_B5015_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5015_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5015_OrgUnit_Deputy_DirectorId))
            {
                var key_B5015_OrgUnit_Deputy_Director = $"{orgUnit_B5015Id}_{role_B5015_OrgUnit_Deputy_DirectorId}_{user_B5015_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5015_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5015Id} - {user_B5015_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5015Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5015_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5015_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5015");
        }

        // B5016
        if (codeToOrgUnitId.TryGetValue("B5016", out var orgUnit_B5016Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5016_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5016_Region_DirectorId))
            {
                var key_B5016_Region_Director = $"{orgUnit_B5016Id}_{role_B5016_Region_DirectorId}_{user_B5016_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5016_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_Region_DirectorId}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_Region_DirectorId,
                        UserId = user_B5016_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5016_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5016_Hub_DirectorId))
            {
                var key_B5016_Hub_Director = $"{orgUnit_B5016Id}_{role_B5016_Hub_DirectorId}_{user_B5016_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5016_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_Hub_DirectorId}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_Hub_DirectorId,
                        UserId = user_B5016_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5016_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5016_Hub_Deputy_DirectorId))
            {
                var key_B5016_Hub_Deputy_Director = $"{orgUnit_B5016Id}_{role_B5016_Hub_Deputy_DirectorId}_{user_B5016_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5016_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_Hub_Deputy_DirectorId,
                        UserId = user_B5016_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5016_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5016_OrgUnit_DirectorId))
            {
                var key_B5016_OrgUnit_Director = $"{orgUnit_B5016Id}_{role_B5016_OrgUnit_DirectorId}_{user_B5016_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5016_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_OrgUnit_DirectorId,
                        UserId = user_B5016_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5016_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5016_OrgUnit_Deputy_DirectorId))
            {
                var key_B5016_OrgUnit_Deputy_Director = $"{orgUnit_B5016Id}_{role_B5016_OrgUnit_Deputy_DirectorId}_{user_B5016_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5016_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5016Id} - {user_B5016_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5016Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5016_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5016_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5016");
        }

        // B5017
        if (codeToOrgUnitId.TryGetValue("B5017", out var orgUnit_B5017Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5017_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5017_Region_DirectorId))
            {
                var key_B5017_Region_Director = $"{orgUnit_B5017Id}_{role_B5017_Region_DirectorId}_{user_B5017_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5017_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_Region_DirectorId}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_Region_DirectorId,
                        UserId = user_B5017_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5017_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5017_Hub_DirectorId))
            {
                var key_B5017_Hub_Director = $"{orgUnit_B5017Id}_{role_B5017_Hub_DirectorId}_{user_B5017_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5017_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_Hub_DirectorId}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_Hub_DirectorId,
                        UserId = user_B5017_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5017_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5017_Hub_Deputy_DirectorId))
            {
                var key_B5017_Hub_Deputy_Director = $"{orgUnit_B5017Id}_{role_B5017_Hub_Deputy_DirectorId}_{user_B5017_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5017_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_Hub_Deputy_DirectorId,
                        UserId = user_B5017_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5017_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5017_OrgUnit_DirectorId))
            {
                var key_B5017_OrgUnit_Director = $"{orgUnit_B5017Id}_{role_B5017_OrgUnit_DirectorId}_{user_B5017_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5017_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_OrgUnit_DirectorId,
                        UserId = user_B5017_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5017_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5017_OrgUnit_Deputy_DirectorId))
            {
                var key_B5017_OrgUnit_Deputy_Director = $"{orgUnit_B5017Id}_{role_B5017_OrgUnit_Deputy_DirectorId}_{user_B5017_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5017_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5017Id} - {user_B5017_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5017Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5017_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5017_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5017");
        }

        // B5018
        if (codeToOrgUnitId.TryGetValue("B5018", out var orgUnit_B5018Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5018_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5018_Region_DirectorId))
            {
                var key_B5018_Region_Director = $"{orgUnit_B5018Id}_{role_B5018_Region_DirectorId}_{user_B5018_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5018_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_Region_DirectorId}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_Region_DirectorId,
                        UserId = user_B5018_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5018_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5018_Hub_DirectorId))
            {
                var key_B5018_Hub_Director = $"{orgUnit_B5018Id}_{role_B5018_Hub_DirectorId}_{user_B5018_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5018_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_Hub_DirectorId}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_Hub_DirectorId,
                        UserId = user_B5018_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5018_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5018_Hub_Deputy_DirectorId))
            {
                var key_B5018_Hub_Deputy_Director = $"{orgUnit_B5018Id}_{role_B5018_Hub_Deputy_DirectorId}_{user_B5018_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5018_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_Hub_Deputy_DirectorId,
                        UserId = user_B5018_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5018_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5018_OrgUnit_DirectorId))
            {
                var key_B5018_OrgUnit_Director = $"{orgUnit_B5018Id}_{role_B5018_OrgUnit_DirectorId}_{user_B5018_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5018_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_OrgUnit_DirectorId,
                        UserId = user_B5018_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5018_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5018_OrgUnit_Deputy_DirectorId))
            {
                var key_B5018_OrgUnit_Deputy_Director = $"{orgUnit_B5018Id}_{role_B5018_OrgUnit_Deputy_DirectorId}_{user_B5018_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5018_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5018Id} - {user_B5018_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5018Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5018_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5018_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5018");
        }

        // B5019
        if (codeToOrgUnitId.TryGetValue("B5019", out var orgUnit_B5019Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5019_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5019_Region_DirectorId))
            {
                var key_B5019_Region_Director = $"{orgUnit_B5019Id}_{role_B5019_Region_DirectorId}_{user_B5019_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5019_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_Region_DirectorId}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_Region_DirectorId,
                        UserId = user_B5019_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5019_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5019_Hub_DirectorId))
            {
                var key_B5019_Hub_Director = $"{orgUnit_B5019Id}_{role_B5019_Hub_DirectorId}_{user_B5019_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5019_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_Hub_DirectorId}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_Hub_DirectorId,
                        UserId = user_B5019_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5019_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5019_Hub_Deputy_DirectorId))
            {
                var key_B5019_Hub_Deputy_Director = $"{orgUnit_B5019Id}_{role_B5019_Hub_Deputy_DirectorId}_{user_B5019_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5019_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_Hub_Deputy_DirectorId,
                        UserId = user_B5019_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5019_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5019_OrgUnit_DirectorId))
            {
                var key_B5019_OrgUnit_Director = $"{orgUnit_B5019Id}_{role_B5019_OrgUnit_DirectorId}_{user_B5019_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5019_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_OrgUnit_DirectorId,
                        UserId = user_B5019_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5019_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5019_OrgUnit_Deputy_DirectorId))
            {
                var key_B5019_OrgUnit_Deputy_Director = $"{orgUnit_B5019Id}_{role_B5019_OrgUnit_Deputy_DirectorId}_{user_B5019_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5019_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5019Id} - {user_B5019_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5019Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5019_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5019_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5019");
        }

        // B5020
        if (codeToOrgUnitId.TryGetValue("B5020", out var orgUnit_B5020Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5020_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5020_Region_DirectorId))
            {
                var key_B5020_Region_Director = $"{orgUnit_B5020Id}_{role_B5020_Region_DirectorId}_{user_B5020_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5020_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_Region_DirectorId}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_Region_DirectorId,
                        UserId = user_B5020_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5020_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5020_Hub_DirectorId))
            {
                var key_B5020_Hub_Director = $"{orgUnit_B5020Id}_{role_B5020_Hub_DirectorId}_{user_B5020_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5020_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_Hub_DirectorId}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_Hub_DirectorId,
                        UserId = user_B5020_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5020_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5020_Hub_Deputy_DirectorId))
            {
                var key_B5020_Hub_Deputy_Director = $"{orgUnit_B5020Id}_{role_B5020_Hub_Deputy_DirectorId}_{user_B5020_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5020_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_Hub_Deputy_DirectorId,
                        UserId = user_B5020_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5020_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5020_OrgUnit_DirectorId))
            {
                var key_B5020_OrgUnit_Director = $"{orgUnit_B5020Id}_{role_B5020_OrgUnit_DirectorId}_{user_B5020_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5020_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_OrgUnit_DirectorId,
                        UserId = user_B5020_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5020_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5020_OrgUnit_Deputy_DirectorId))
            {
                var key_B5020_OrgUnit_Deputy_Director = $"{orgUnit_B5020Id}_{role_B5020_OrgUnit_Deputy_DirectorId}_{user_B5020_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5020_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5020Id} - {user_B5020_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5020Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5020_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5020_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5020");
        }

        // B5022
        if (codeToOrgUnitId.TryGetValue("B5022", out var orgUnit_B5022Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5022_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5022_Region_DirectorId))
            {
                var key_B5022_Region_Director = $"{orgUnit_B5022Id}_{role_B5022_Region_DirectorId}_{user_B5022_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5022_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_Region_DirectorId}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_Region_DirectorId,
                        UserId = user_B5022_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5022_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5022_Hub_DirectorId))
            {
                var key_B5022_Hub_Director = $"{orgUnit_B5022Id}_{role_B5022_Hub_DirectorId}_{user_B5022_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5022_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_Hub_DirectorId}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_Hub_DirectorId,
                        UserId = user_B5022_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5022_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5022_Hub_Deputy_DirectorId))
            {
                var key_B5022_Hub_Deputy_Director = $"{orgUnit_B5022Id}_{role_B5022_Hub_Deputy_DirectorId}_{user_B5022_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5022_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_Hub_Deputy_DirectorId,
                        UserId = user_B5022_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5022_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5022_OrgUnit_DirectorId))
            {
                var key_B5022_OrgUnit_Director = $"{orgUnit_B5022Id}_{role_B5022_OrgUnit_DirectorId}_{user_B5022_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5022_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_OrgUnit_DirectorId,
                        UserId = user_B5022_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5022_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5022_OrgUnit_Deputy_DirectorId))
            {
                var key_B5022_OrgUnit_Deputy_Director = $"{orgUnit_B5022Id}_{role_B5022_OrgUnit_Deputy_DirectorId}_{user_B5022_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5022_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5022Id} - {user_B5022_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5022Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5022_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5022_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5022");
        }

        // B5023
        if (codeToOrgUnitId.TryGetValue("B5023", out var orgUnit_B5023Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5023_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5023_Region_DirectorId))
            {
                var key_B5023_Region_Director = $"{orgUnit_B5023Id}_{role_B5023_Region_DirectorId}_{user_B5023_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5023_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_Region_DirectorId}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_Region_DirectorId,
                        UserId = user_B5023_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5023_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5023_Hub_DirectorId))
            {
                var key_B5023_Hub_Director = $"{orgUnit_B5023Id}_{role_B5023_Hub_DirectorId}_{user_B5023_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5023_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_Hub_DirectorId}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_Hub_DirectorId,
                        UserId = user_B5023_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5023_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5023_Hub_Deputy_DirectorId))
            {
                var key_B5023_Hub_Deputy_Director = $"{orgUnit_B5023Id}_{role_B5023_Hub_Deputy_DirectorId}_{user_B5023_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5023_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_Hub_Deputy_DirectorId,
                        UserId = user_B5023_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5023_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5023_OrgUnit_DirectorId))
            {
                var key_B5023_OrgUnit_Director = $"{orgUnit_B5023Id}_{role_B5023_OrgUnit_DirectorId}_{user_B5023_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5023_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_OrgUnit_DirectorId,
                        UserId = user_B5023_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5023_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5023_OrgUnit_Deputy_DirectorId))
            {
                var key_B5023_OrgUnit_Deputy_Director = $"{orgUnit_B5023Id}_{role_B5023_OrgUnit_Deputy_DirectorId}_{user_B5023_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5023_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5023Id} - {user_B5023_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5023Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5023_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5023_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5023");
        }

        // B5024
        if (codeToOrgUnitId.TryGetValue("B5024", out var orgUnit_B5024Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5024_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5024_Region_DirectorId))
            {
                var key_B5024_Region_Director = $"{orgUnit_B5024Id}_{role_B5024_Region_DirectorId}_{user_B5024_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5024_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_Region_DirectorId}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_Region_DirectorId,
                        UserId = user_B5024_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5024_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5024_Hub_DirectorId))
            {
                var key_B5024_Hub_Director = $"{orgUnit_B5024Id}_{role_B5024_Hub_DirectorId}_{user_B5024_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5024_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_Hub_DirectorId}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_Hub_DirectorId,
                        UserId = user_B5024_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5024_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5024_Hub_Deputy_DirectorId))
            {
                var key_B5024_Hub_Deputy_Director = $"{orgUnit_B5024Id}_{role_B5024_Hub_Deputy_DirectorId}_{user_B5024_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5024_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_Hub_Deputy_DirectorId,
                        UserId = user_B5024_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5024_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5024_OrgUnit_DirectorId))
            {
                var key_B5024_OrgUnit_Director = $"{orgUnit_B5024Id}_{role_B5024_OrgUnit_DirectorId}_{user_B5024_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5024_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_OrgUnit_DirectorId,
                        UserId = user_B5024_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5024_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5024_OrgUnit_Deputy_DirectorId))
            {
                var key_B5024_OrgUnit_Deputy_Director = $"{orgUnit_B5024Id}_{role_B5024_OrgUnit_Deputy_DirectorId}_{user_B5024_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5024_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5024Id} - {user_B5024_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5024Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5024_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5024_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5024");
        }

        // B5025
        if (codeToOrgUnitId.TryGetValue("B5025", out var orgUnit_B5025Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5025_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5025_Region_DirectorId))
            {
                var key_B5025_Region_Director = $"{orgUnit_B5025Id}_{role_B5025_Region_DirectorId}_{user_B5025_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5025_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_Region_DirectorId}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_Region_DirectorId,
                        UserId = user_B5025_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5025_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5025_Hub_DirectorId))
            {
                var key_B5025_Hub_Director = $"{orgUnit_B5025Id}_{role_B5025_Hub_DirectorId}_{user_B5025_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5025_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_Hub_DirectorId}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_Hub_DirectorId,
                        UserId = user_B5025_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5025_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5025_Hub_Deputy_DirectorId))
            {
                var key_B5025_Hub_Deputy_Director = $"{orgUnit_B5025Id}_{role_B5025_Hub_Deputy_DirectorId}_{user_B5025_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5025_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_Hub_Deputy_DirectorId,
                        UserId = user_B5025_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5025_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5025_OrgUnit_DirectorId))
            {
                var key_B5025_OrgUnit_Director = $"{orgUnit_B5025Id}_{role_B5025_OrgUnit_DirectorId}_{user_B5025_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5025_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_OrgUnit_DirectorId,
                        UserId = user_B5025_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5025_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5025_OrgUnit_Deputy_DirectorId))
            {
                var key_B5025_OrgUnit_Deputy_Director = $"{orgUnit_B5025Id}_{role_B5025_OrgUnit_Deputy_DirectorId}_{user_B5025_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5025_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5025Id} - {user_B5025_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5025Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5025_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5025_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5025");
        }

        // B5026
        if (codeToOrgUnitId.TryGetValue("B5026", out var orgUnit_B5026Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5026_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5026_Region_DirectorId))
            {
                var key_B5026_Region_Director = $"{orgUnit_B5026Id}_{role_B5026_Region_DirectorId}_{user_B5026_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5026_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_Region_DirectorId}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_Region_DirectorId,
                        UserId = user_B5026_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5026_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5026_Hub_DirectorId))
            {
                var key_B5026_Hub_Director = $"{orgUnit_B5026Id}_{role_B5026_Hub_DirectorId}_{user_B5026_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5026_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_Hub_DirectorId}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_Hub_DirectorId,
                        UserId = user_B5026_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5026_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5026_Hub_Deputy_DirectorId))
            {
                var key_B5026_Hub_Deputy_Director = $"{orgUnit_B5026Id}_{role_B5026_Hub_Deputy_DirectorId}_{user_B5026_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5026_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_Hub_Deputy_DirectorId,
                        UserId = user_B5026_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5026_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5026_OrgUnit_DirectorId))
            {
                var key_B5026_OrgUnit_Director = $"{orgUnit_B5026Id}_{role_B5026_OrgUnit_DirectorId}_{user_B5026_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5026_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_OrgUnit_DirectorId,
                        UserId = user_B5026_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5026_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5026_OrgUnit_Deputy_DirectorId))
            {
                var key_B5026_OrgUnit_Deputy_Director = $"{orgUnit_B5026Id}_{role_B5026_OrgUnit_Deputy_DirectorId}_{user_B5026_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5026_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5026Id} - {user_B5026_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5026Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5026_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5026_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5026");
        }

        // B5027
        if (codeToOrgUnitId.TryGetValue("B5027", out var orgUnit_B5027Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5027_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5027_Region_DirectorId))
            {
                var key_B5027_Region_Director = $"{orgUnit_B5027Id}_{role_B5027_Region_DirectorId}_{user_B5027_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5027_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_Region_DirectorId}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_Region_DirectorId,
                        UserId = user_B5027_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5027_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5027_Hub_DirectorId))
            {
                var key_B5027_Hub_Director = $"{orgUnit_B5027Id}_{role_B5027_Hub_DirectorId}_{user_B5027_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5027_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_Hub_DirectorId}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_Hub_DirectorId,
                        UserId = user_B5027_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5027_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5027_Hub_Deputy_DirectorId))
            {
                var key_B5027_Hub_Deputy_Director = $"{orgUnit_B5027Id}_{role_B5027_Hub_Deputy_DirectorId}_{user_B5027_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5027_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_Hub_Deputy_DirectorId,
                        UserId = user_B5027_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5027_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5027_OrgUnit_DirectorId))
            {
                var key_B5027_OrgUnit_Director = $"{orgUnit_B5027Id}_{role_B5027_OrgUnit_DirectorId}_{user_B5027_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5027_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_OrgUnit_DirectorId,
                        UserId = user_B5027_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5027_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5027_OrgUnit_Deputy_DirectorId))
            {
                var key_B5027_OrgUnit_Deputy_Director = $"{orgUnit_B5027Id}_{role_B5027_OrgUnit_Deputy_DirectorId}_{user_B5027_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5027_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5027Id} - {user_B5027_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5027Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5027_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5027_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5027");
        }

        // B5028
        if (codeToOrgUnitId.TryGetValue("B5028", out var orgUnit_B5028Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5028_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5028_Region_DirectorId))
            {
                var key_B5028_Region_Director = $"{orgUnit_B5028Id}_{role_B5028_Region_DirectorId}_{user_B5028_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5028_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_Region_DirectorId}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_Region_DirectorId,
                        UserId = user_B5028_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5028_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5028_Hub_DirectorId))
            {
                var key_B5028_Hub_Director = $"{orgUnit_B5028Id}_{role_B5028_Hub_DirectorId}_{user_B5028_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5028_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_Hub_DirectorId}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_Hub_DirectorId,
                        UserId = user_B5028_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5028_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5028_Hub_Deputy_DirectorId))
            {
                var key_B5028_Hub_Deputy_Director = $"{orgUnit_B5028Id}_{role_B5028_Hub_Deputy_DirectorId}_{user_B5028_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5028_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_Hub_Deputy_DirectorId,
                        UserId = user_B5028_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5028_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5028_OrgUnit_DirectorId))
            {
                var key_B5028_OrgUnit_Director = $"{orgUnit_B5028Id}_{role_B5028_OrgUnit_DirectorId}_{user_B5028_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5028_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_OrgUnit_DirectorId,
                        UserId = user_B5028_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5028_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5028_OrgUnit_Deputy_DirectorId))
            {
                var key_B5028_OrgUnit_Deputy_Director = $"{orgUnit_B5028Id}_{role_B5028_OrgUnit_Deputy_DirectorId}_{user_B5028_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5028_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5028Id} - {user_B5028_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5028Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5028_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5028_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5028");
        }

        // B5029
        if (codeToOrgUnitId.TryGetValue("B5029", out var orgUnit_B5029Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5029_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5029_Region_DirectorId))
            {
                var key_B5029_Region_Director = $"{orgUnit_B5029Id}_{role_B5029_Region_DirectorId}_{user_B5029_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5029_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_Region_DirectorId}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_Region_DirectorId,
                        UserId = user_B5029_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5029_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5029_Hub_DirectorId))
            {
                var key_B5029_Hub_Director = $"{orgUnit_B5029Id}_{role_B5029_Hub_DirectorId}_{user_B5029_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5029_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_Hub_DirectorId}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_Hub_DirectorId,
                        UserId = user_B5029_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5029_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5029_Hub_Deputy_DirectorId))
            {
                var key_B5029_Hub_Deputy_Director = $"{orgUnit_B5029Id}_{role_B5029_Hub_Deputy_DirectorId}_{user_B5029_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5029_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_Hub_Deputy_DirectorId,
                        UserId = user_B5029_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5029_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5029_OrgUnit_DirectorId))
            {
                var key_B5029_OrgUnit_Director = $"{orgUnit_B5029Id}_{role_B5029_OrgUnit_DirectorId}_{user_B5029_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5029_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_OrgUnit_DirectorId,
                        UserId = user_B5029_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5029_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5029_OrgUnit_Deputy_DirectorId))
            {
                var key_B5029_OrgUnit_Deputy_Director = $"{orgUnit_B5029Id}_{role_B5029_OrgUnit_Deputy_DirectorId}_{user_B5029_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5029_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5029Id} - {user_B5029_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5029Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5029_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5029_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5029");
        }

        // B5031
        if (codeToOrgUnitId.TryGetValue("B5031", out var orgUnit_B5031Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5031_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5031_Region_DirectorId))
            {
                var key_B5031_Region_Director = $"{orgUnit_B5031Id}_{role_B5031_Region_DirectorId}_{user_B5031_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5031_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_Region_DirectorId}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_Region_DirectorId,
                        UserId = user_B5031_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5031_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5031_Hub_DirectorId))
            {
                var key_B5031_Hub_Director = $"{orgUnit_B5031Id}_{role_B5031_Hub_DirectorId}_{user_B5031_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5031_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_Hub_DirectorId}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_Hub_DirectorId,
                        UserId = user_B5031_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5031_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5031_Hub_Deputy_DirectorId))
            {
                var key_B5031_Hub_Deputy_Director = $"{orgUnit_B5031Id}_{role_B5031_Hub_Deputy_DirectorId}_{user_B5031_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5031_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_Hub_Deputy_DirectorId,
                        UserId = user_B5031_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5031_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5031_OrgUnit_DirectorId))
            {
                var key_B5031_OrgUnit_Director = $"{orgUnit_B5031Id}_{role_B5031_OrgUnit_DirectorId}_{user_B5031_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5031_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_OrgUnit_DirectorId,
                        UserId = user_B5031_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5031_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5031_OrgUnit_Deputy_DirectorId))
            {
                var key_B5031_OrgUnit_Deputy_Director = $"{orgUnit_B5031Id}_{role_B5031_OrgUnit_Deputy_DirectorId}_{user_B5031_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5031_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5031Id} - {user_B5031_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5031Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5031_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5031_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5031");
        }

        // B5032
        if (codeToOrgUnitId.TryGetValue("B5032", out var orgUnit_B5032Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5032_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5032_Region_DirectorId))
            {
                var key_B5032_Region_Director = $"{orgUnit_B5032Id}_{role_B5032_Region_DirectorId}_{user_B5032_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5032_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_Region_DirectorId}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_Region_DirectorId,
                        UserId = user_B5032_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5032_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5032_Hub_DirectorId))
            {
                var key_B5032_Hub_Director = $"{orgUnit_B5032Id}_{role_B5032_Hub_DirectorId}_{user_B5032_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5032_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_Hub_DirectorId}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_Hub_DirectorId,
                        UserId = user_B5032_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5032_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5032_Hub_Deputy_DirectorId))
            {
                var key_B5032_Hub_Deputy_Director = $"{orgUnit_B5032Id}_{role_B5032_Hub_Deputy_DirectorId}_{user_B5032_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5032_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_Hub_Deputy_DirectorId,
                        UserId = user_B5032_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5032_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5032_OrgUnit_DirectorId))
            {
                var key_B5032_OrgUnit_Director = $"{orgUnit_B5032Id}_{role_B5032_OrgUnit_DirectorId}_{user_B5032_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5032_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_OrgUnit_DirectorId,
                        UserId = user_B5032_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5032_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5032_OrgUnit_Deputy_DirectorId))
            {
                var key_B5032_OrgUnit_Deputy_Director = $"{orgUnit_B5032Id}_{role_B5032_OrgUnit_Deputy_DirectorId}_{user_B5032_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5032_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5032Id} - {user_B5032_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5032Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5032_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5032_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5032");
        }

        // B5033
        if (codeToOrgUnitId.TryGetValue("B5033", out var orgUnit_B5033Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5033_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5033_Region_DirectorId))
            {
                var key_B5033_Region_Director = $"{orgUnit_B5033Id}_{role_B5033_Region_DirectorId}_{user_B5033_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5033_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_Region_DirectorId}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_Region_DirectorId,
                        UserId = user_B5033_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5033_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5033_Hub_DirectorId))
            {
                var key_B5033_Hub_Director = $"{orgUnit_B5033Id}_{role_B5033_Hub_DirectorId}_{user_B5033_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5033_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_Hub_DirectorId}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_Hub_DirectorId,
                        UserId = user_B5033_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5033_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5033_Hub_Deputy_DirectorId))
            {
                var key_B5033_Hub_Deputy_Director = $"{orgUnit_B5033Id}_{role_B5033_Hub_Deputy_DirectorId}_{user_B5033_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5033_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_Hub_Deputy_DirectorId,
                        UserId = user_B5033_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5033_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5033_OrgUnit_DirectorId))
            {
                var key_B5033_OrgUnit_Director = $"{orgUnit_B5033Id}_{role_B5033_OrgUnit_DirectorId}_{user_B5033_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5033_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_OrgUnit_DirectorId,
                        UserId = user_B5033_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
            // OrgUnit Deputy Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5033_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5033_OrgUnit_Deputy_DirectorId))
            {
                var key_B5033_OrgUnit_Deputy_Director = $"{orgUnit_B5033Id}_{role_B5033_OrgUnit_Deputy_DirectorId}_{user_B5033_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5033_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5033Id} - {user_B5033_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5033Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5033_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5033_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5033");
        }

        // B5034
        if (codeToOrgUnitId.TryGetValue("B5034", out var orgUnit_B5034Id))
        {
            // Region Director: emiliep@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5034_Region_DirectorId) &&
                emailToUserId.TryGetValue("emiliep@unops.org", out var user_B5034_Region_DirectorId))
            {
                var key_B5034_Region_Director = $"{orgUnit_B5034Id}_{role_B5034_Region_DirectorId}_{user_B5034_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5034_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_Region_DirectorId}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_Region_DirectorId,
                        UserId = user_B5034_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("emiliep@unops.org")) missingUsers.Add("emiliep@unops.org");
            }
            // Hub Director: amiro@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5034_Hub_DirectorId) &&
                emailToUserId.TryGetValue("amiro@unops.org", out var user_B5034_Hub_DirectorId))
            {
                var key_B5034_Hub_Director = $"{orgUnit_B5034Id}_{role_B5034_Hub_DirectorId}_{user_B5034_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5034_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_Hub_DirectorId}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_Hub_DirectorId,
                        UserId = user_B5034_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("amiro@unops.org")) missingUsers.Add("amiro@unops.org");
            }
            // Hub Deputy Director: salmanh@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5034_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("salmanh@unops.org", out var user_B5034_Hub_Deputy_DirectorId))
            {
                var key_B5034_Hub_Deputy_Director = $"{orgUnit_B5034Id}_{role_B5034_Hub_Deputy_DirectorId}_{user_B5034_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5034_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_Hub_Deputy_DirectorId,
                        UserId = user_B5034_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("salmanh@unops.org")) missingUsers.Add("salmanh@unops.org");
            }
            // OrgUnit Director: jeanguyl@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5034_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jeanguyl@unops.org", out var user_B5034_OrgUnit_DirectorId))
            {
                var key_B5034_OrgUnit_Director = $"{orgUnit_B5034Id}_{role_B5034_OrgUnit_DirectorId}_{user_B5034_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5034_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_OrgUnit_DirectorId,
                        UserId = user_B5034_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jeanguyl@unops.org")) missingUsers.Add("jeanguyl@unops.org");
            }
            // OrgUnit Deputy Director: tammyb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5034_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("tammyb@unops.org", out var user_B5034_OrgUnit_Deputy_DirectorId))
            {
                var key_B5034_OrgUnit_Deputy_Director = $"{orgUnit_B5034Id}_{role_B5034_OrgUnit_Deputy_DirectorId}_{user_B5034_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5034_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5034Id} - {user_B5034_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5034Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5034_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5034_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("tammyb@unops.org")) missingUsers.Add("tammyb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5034");
        }

        // B5101
        if (codeToOrgUnitId.TryGetValue("B5101", out var orgUnit_B5101Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5101_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5101_Region_DirectorId))
            {
                var key_B5101_Region_Director = $"{orgUnit_B5101Id}_{role_B5101_Region_DirectorId}_{user_B5101_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5101_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_Region_DirectorId}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_Region_DirectorId,
                        UserId = user_B5101_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5101_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5101_Region_Deputy_DirectorId))
            {
                var key_B5101_Region_Deputy_Director = $"{orgUnit_B5101Id}_{role_B5101_Region_Deputy_DirectorId}_{user_B5101_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5101_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_Region_Deputy_DirectorId,
                        UserId = user_B5101_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // OrgUnit Director: katyw@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5101_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("katyw@unops.org", out var user_B5101_OrgUnit_DirectorId))
            {
                var key_B5101_OrgUnit_Director = $"{orgUnit_B5101Id}_{role_B5101_OrgUnit_DirectorId}_{user_B5101_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5101_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_OrgUnit_DirectorId,
                        UserId = user_B5101_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("katyw@unops.org")) missingUsers.Add("katyw@unops.org");
            }
            // OrgUnit Deputy Director: mihaelas@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5101_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mihaelas@unops.org", out var user_B5101_OrgUnit_Deputy_DirectorId))
            {
                var key_B5101_OrgUnit_Deputy_Director = $"{orgUnit_B5101Id}_{role_B5101_OrgUnit_Deputy_DirectorId}_{user_B5101_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5101_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5101Id} - {user_B5101_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5101Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5101_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5101_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mihaelas@unops.org")) missingUsers.Add("mihaelas@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5101");
        }

        // B5104
        if (codeToOrgUnitId.TryGetValue("B5104", out var orgUnit_B5104Id))
        {
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5104_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5104_Region_DirectorId))
            {
                var key_B5104_Region_Director = $"{orgUnit_B5104Id}_{role_B5104_Region_DirectorId}_{user_B5104_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5104_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5104Id} - {user_B5104_Region_DirectorId}",
                        EntityId = orgUnit_B5104Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5104_Region_DirectorId,
                        UserId = user_B5104_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // Hub Director: usmana@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5104_Hub_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5104_Hub_DirectorId))
            {
                var key_B5104_Hub_Director = $"{orgUnit_B5104Id}_{role_B5104_Hub_DirectorId}_{user_B5104_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5104_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5104Id} - {user_B5104_Hub_DirectorId}",
                        EntityId = orgUnit_B5104Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5104_Hub_DirectorId,
                        UserId = user_B5104_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // OrgUnit Director: usmana@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5104_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5104_OrgUnit_DirectorId))
            {
                var key_B5104_OrgUnit_Director = $"{orgUnit_B5104Id}_{role_B5104_OrgUnit_DirectorId}_{user_B5104_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5104_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5104Id} - {user_B5104_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5104Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5104_OrgUnit_DirectorId,
                        UserId = user_B5104_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5104");
        }

        // B5106
        if (codeToOrgUnitId.TryGetValue("B5106", out var orgUnit_B5106Id))
        {
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5106_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5106_Region_DirectorId))
            {
                var key_B5106_Region_Director = $"{orgUnit_B5106Id}_{role_B5106_Region_DirectorId}_{user_B5106_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5106_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5106Id} - {user_B5106_Region_DirectorId}",
                        EntityId = orgUnit_B5106Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5106_Region_DirectorId,
                        UserId = user_B5106_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // OrgUnit Director: karunah@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5106_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("karunah@unops.org", out var user_B5106_OrgUnit_DirectorId))
            {
                var key_B5106_OrgUnit_Director = $"{orgUnit_B5106Id}_{role_B5106_OrgUnit_DirectorId}_{user_B5106_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5106_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5106Id} - {user_B5106_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5106Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5106_OrgUnit_DirectorId,
                        UserId = user_B5106_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("karunah@unops.org")) missingUsers.Add("karunah@unops.org");
            }
            // OrgUnit Deputy Director: sophien@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5106_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("sophien@unops.org", out var user_B5106_OrgUnit_Deputy_DirectorId))
            {
                var key_B5106_OrgUnit_Deputy_Director = $"{orgUnit_B5106Id}_{role_B5106_OrgUnit_Deputy_DirectorId}_{user_B5106_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5106_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5106Id} - {user_B5106_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5106Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5106_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5106_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
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
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5107_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5107_Region_DirectorId))
            {
                var key_B5107_Region_Director = $"{orgUnit_B5107Id}_{role_B5107_Region_DirectorId}_{user_B5107_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5107_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_Region_DirectorId}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_Region_DirectorId,
                        UserId = user_B5107_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5107_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5107_Region_Deputy_DirectorId))
            {
                var key_B5107_Region_Deputy_Director = $"{orgUnit_B5107Id}_{role_B5107_Region_Deputy_DirectorId}_{user_B5107_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5107_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_Region_Deputy_DirectorId,
                        UserId = user_B5107_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // Hub Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5107_Hub_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5107_Hub_DirectorId))
            {
                var key_B5107_Hub_Director = $"{orgUnit_B5107Id}_{role_B5107_Hub_DirectorId}_{user_B5107_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5107_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_Hub_DirectorId}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_Hub_DirectorId,
                        UserId = user_B5107_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // OrgUnit Director: sabinek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5107_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("sabinek@unops.org", out var user_B5107_OrgUnit_DirectorId))
            {
                var key_B5107_OrgUnit_Director = $"{orgUnit_B5107Id}_{role_B5107_OrgUnit_DirectorId}_{user_B5107_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5107_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_OrgUnit_DirectorId,
                        UserId = user_B5107_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("sabinek@unops.org")) missingUsers.Add("sabinek@unops.org");
            }
            // OrgUnit Deputy Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5107_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5107_OrgUnit_Deputy_DirectorId))
            {
                var key_B5107_OrgUnit_Deputy_Director = $"{orgUnit_B5107Id}_{role_B5107_OrgUnit_Deputy_DirectorId}_{user_B5107_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5107_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5107Id} - {user_B5107_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5107Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5107_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5107_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5107");
        }

        // B5109
        if (codeToOrgUnitId.TryGetValue("B5109", out var orgUnit_B5109Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5109_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5109_Region_DirectorId))
            {
                var key_B5109_Region_Director = $"{orgUnit_B5109Id}_{role_B5109_Region_DirectorId}_{user_B5109_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5109_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_Region_DirectorId}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_Region_DirectorId,
                        UserId = user_B5109_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5109_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5109_Region_Deputy_DirectorId))
            {
                var key_B5109_Region_Deputy_Director = $"{orgUnit_B5109Id}_{role_B5109_Region_Deputy_DirectorId}_{user_B5109_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5109_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_Region_Deputy_DirectorId,
                        UserId = user_B5109_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // OrgUnit Director: michelat@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5109_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("michelat@unops.org", out var user_B5109_OrgUnit_DirectorId))
            {
                var key_B5109_OrgUnit_Director = $"{orgUnit_B5109Id}_{role_B5109_OrgUnit_DirectorId}_{user_B5109_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5109_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_OrgUnit_DirectorId,
                        UserId = user_B5109_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("michelat@unops.org")) missingUsers.Add("michelat@unops.org");
            }
            // OrgUnit Deputy Director: daliborkak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5109_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("daliborkak@unops.org", out var user_B5109_OrgUnit_Deputy_DirectorId))
            {
                var key_B5109_OrgUnit_Deputy_Director = $"{orgUnit_B5109Id}_{role_B5109_OrgUnit_Deputy_DirectorId}_{user_B5109_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5109_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5109Id} - {user_B5109_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5109Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5109_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5109_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("daliborkak@unops.org")) missingUsers.Add("daliborkak@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5109");
        }

        // B5110
        if (codeToOrgUnitId.TryGetValue("B5110", out var orgUnit_B5110Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5110_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5110_Region_DirectorId))
            {
                var key_B5110_Region_Director = $"{orgUnit_B5110Id}_{role_B5110_Region_DirectorId}_{user_B5110_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5110_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_Region_DirectorId}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_Region_DirectorId,
                        UserId = user_B5110_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5110_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5110_Region_Deputy_DirectorId))
            {
                var key_B5110_Region_Deputy_Director = $"{orgUnit_B5110Id}_{role_B5110_Region_Deputy_DirectorId}_{user_B5110_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5110_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_Region_Deputy_DirectorId,
                        UserId = user_B5110_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // OrgUnit Director: michelat@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5110_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("michelat@unops.org", out var user_B5110_OrgUnit_DirectorId))
            {
                var key_B5110_OrgUnit_Director = $"{orgUnit_B5110Id}_{role_B5110_OrgUnit_DirectorId}_{user_B5110_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5110_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5110Id} - {user_B5110_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5110Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5110_OrgUnit_DirectorId,
                        UserId = user_B5110_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("michelat@unops.org")) missingUsers.Add("michelat@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5110");
        }

        // B5116
        if (codeToOrgUnitId.TryGetValue("B5116", out var orgUnit_B5116Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5116_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5116_Region_DirectorId))
            {
                var key_B5116_Region_Director = $"{orgUnit_B5116Id}_{role_B5116_Region_DirectorId}_{user_B5116_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5116_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_Region_DirectorId}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_Region_DirectorId,
                        UserId = user_B5116_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5116_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5116_Region_Deputy_DirectorId))
            {
                var key_B5116_Region_Deputy_Director = $"{orgUnit_B5116Id}_{role_B5116_Region_Deputy_DirectorId}_{user_B5116_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5116_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_Region_Deputy_DirectorId,
                        UserId = user_B5116_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // OrgUnit Director: massimodi@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5116_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("massimodi@unops.org", out var user_B5116_OrgUnit_DirectorId))
            {
                var key_B5116_OrgUnit_Director = $"{orgUnit_B5116Id}_{role_B5116_OrgUnit_DirectorId}_{user_B5116_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5116_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_OrgUnit_DirectorId,
                        UserId = user_B5116_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("massimodi@unops.org")) missingUsers.Add("massimodi@unops.org");
            }
            // OrgUnit Deputy Director: marysiaz@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5116_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("marysiaz@unops.org", out var user_B5116_OrgUnit_Deputy_DirectorId))
            {
                var key_B5116_OrgUnit_Deputy_Director = $"{orgUnit_B5116Id}_{role_B5116_OrgUnit_Deputy_DirectorId}_{user_B5116_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5116_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5116Id} - {user_B5116_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5116Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5116_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5116_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("marysiaz@unops.org")) missingUsers.Add("marysiaz@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5116");
        }

        // B5120
        if (codeToOrgUnitId.TryGetValue("B5120", out var orgUnit_B5120Id))
        {
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5120_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5120_Region_DirectorId))
            {
                var key_B5120_Region_Director = $"{orgUnit_B5120Id}_{role_B5120_Region_DirectorId}_{user_B5120_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5120_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5120Id} - {user_B5120_Region_DirectorId}",
                        EntityId = orgUnit_B5120Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5120_Region_DirectorId,
                        UserId = user_B5120_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // OrgUnit Director: banak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5120_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5120_OrgUnit_DirectorId))
            {
                var key_B5120_OrgUnit_Director = $"{orgUnit_B5120Id}_{role_B5120_OrgUnit_DirectorId}_{user_B5120_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5120_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5120Id} - {user_B5120_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5120Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5120_OrgUnit_DirectorId,
                        UserId = user_B5120_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
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
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5121_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5121_Region_DirectorId))
            {
                var key_B5121_Region_Director = $"{orgUnit_B5121Id}_{role_B5121_Region_DirectorId}_{user_B5121_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5121_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5121Id} - {user_B5121_Region_DirectorId}",
                        EntityId = orgUnit_B5121Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5121_Region_DirectorId,
                        UserId = user_B5121_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // Hub Director: usmana@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5121_Hub_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5121_Hub_DirectorId))
            {
                var key_B5121_Hub_Director = $"{orgUnit_B5121Id}_{role_B5121_Hub_DirectorId}_{user_B5121_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5121_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5121Id} - {user_B5121_Hub_DirectorId}",
                        EntityId = orgUnit_B5121Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5121_Hub_DirectorId,
                        UserId = user_B5121_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // OrgUnit Director: usmana@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5121_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5121_OrgUnit_DirectorId))
            {
                var key_B5121_OrgUnit_Director = $"{orgUnit_B5121Id}_{role_B5121_OrgUnit_DirectorId}_{user_B5121_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5121_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5121Id} - {user_B5121_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5121Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5121_OrgUnit_DirectorId,
                        UserId = user_B5121_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
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
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5122_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5122_Region_DirectorId))
            {
                var key_B5122_Region_Director = $"{orgUnit_B5122Id}_{role_B5122_Region_DirectorId}_{user_B5122_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5122_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5122Id} - {user_B5122_Region_DirectorId}",
                        EntityId = orgUnit_B5122Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5122_Region_DirectorId,
                        UserId = user_B5122_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // Hub Director: usmana@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5122_Hub_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5122_Hub_DirectorId))
            {
                var key_B5122_Hub_Director = $"{orgUnit_B5122Id}_{role_B5122_Hub_DirectorId}_{user_B5122_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5122_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5122Id} - {user_B5122_Hub_DirectorId}",
                        EntityId = orgUnit_B5122Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5122_Hub_DirectorId,
                        UserId = user_B5122_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // OrgUnit Director: usmana@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5122_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5122_OrgUnit_DirectorId))
            {
                var key_B5122_OrgUnit_Director = $"{orgUnit_B5122Id}_{role_B5122_OrgUnit_DirectorId}_{user_B5122_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5122_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5122Id} - {user_B5122_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5122Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5122_OrgUnit_DirectorId,
                        UserId = user_B5122_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
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
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5123_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5123_Region_DirectorId))
            {
                var key_B5123_Region_Director = $"{orgUnit_B5123Id}_{role_B5123_Region_DirectorId}_{user_B5123_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5123_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5123Id} - {user_B5123_Region_DirectorId}",
                        EntityId = orgUnit_B5123Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5123_Region_DirectorId,
                        UserId = user_B5123_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // Hub Director: usmana@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5123_Hub_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5123_Hub_DirectorId))
            {
                var key_B5123_Hub_Director = $"{orgUnit_B5123Id}_{role_B5123_Hub_DirectorId}_{user_B5123_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5123_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5123Id} - {user_B5123_Hub_DirectorId}",
                        EntityId = orgUnit_B5123Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5123_Hub_DirectorId,
                        UserId = user_B5123_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // OrgUnit Director: usmana@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5123_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5123_OrgUnit_DirectorId))
            {
                var key_B5123_OrgUnit_Director = $"{orgUnit_B5123Id}_{role_B5123_OrgUnit_DirectorId}_{user_B5123_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5123_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5123Id} - {user_B5123_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5123Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5123_OrgUnit_DirectorId,
                        UserId = user_B5123_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
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
            // Region Director: banak@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5124_Region_DirectorId) &&
                emailToUserId.TryGetValue("banak@unops.org", out var user_B5124_Region_DirectorId))
            {
                var key_B5124_Region_Director = $"{orgUnit_B5124Id}_{role_B5124_Region_DirectorId}_{user_B5124_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5124_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5124Id} - {user_B5124_Region_DirectorId}",
                        EntityId = orgUnit_B5124Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5124_Region_DirectorId,
                        UserId = user_B5124_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("banak@unops.org")) missingUsers.Add("banak@unops.org");
            }
            // Hub Director: usmana@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5124_Hub_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5124_Hub_DirectorId))
            {
                var key_B5124_Hub_Director = $"{orgUnit_B5124Id}_{role_B5124_Hub_DirectorId}_{user_B5124_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5124_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5124Id} - {user_B5124_Hub_DirectorId}",
                        EntityId = orgUnit_B5124Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5124_Hub_DirectorId,
                        UserId = user_B5124_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // OrgUnit Director: usmana@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5124_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("usmana@unops.org", out var user_B5124_OrgUnit_DirectorId))
            {
                var key_B5124_OrgUnit_Director = $"{orgUnit_B5124Id}_{role_B5124_OrgUnit_DirectorId}_{user_B5124_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5124_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5124Id} - {user_B5124_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5124Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5124_OrgUnit_DirectorId,
                        UserId = user_B5124_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("usmana@unops.org")) missingUsers.Add("usmana@unops.org");
            }
            // OrgUnit Deputy Director: fayyazfr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5124_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fayyazfr@unops.org", out var user_B5124_OrgUnit_Deputy_DirectorId))
            {
                var key_B5124_OrgUnit_Deputy_Director = $"{orgUnit_B5124Id}_{role_B5124_OrgUnit_Deputy_DirectorId}_{user_B5124_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5124_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5124Id} - {user_B5124_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5124Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5124_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5124_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("fayyazfr@unops.org")) missingUsers.Add("fayyazfr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5124");
        }

        // B5130
        if (codeToOrgUnitId.TryGetValue("B5130", out var orgUnit_B5130Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5130_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5130_Region_DirectorId))
            {
                var key_B5130_Region_Director = $"{orgUnit_B5130Id}_{role_B5130_Region_DirectorId}_{user_B5130_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5130_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_Region_DirectorId}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_Region_DirectorId,
                        UserId = user_B5130_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5130_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5130_Region_Deputy_DirectorId))
            {
                var key_B5130_Region_Deputy_Director = $"{orgUnit_B5130Id}_{role_B5130_Region_Deputy_DirectorId}_{user_B5130_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5130_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_Region_Deputy_DirectorId,
                        UserId = user_B5130_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // Hub Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5130_Hub_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5130_Hub_DirectorId))
            {
                var key_B5130_Hub_Director = $"{orgUnit_B5130Id}_{role_B5130_Hub_DirectorId}_{user_B5130_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5130_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_Hub_DirectorId}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_Hub_DirectorId,
                        UserId = user_B5130_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // OrgUnit Director: simonettas@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5130_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5130_OrgUnit_DirectorId))
            {
                var key_B5130_OrgUnit_Director = $"{orgUnit_B5130Id}_{role_B5130_OrgUnit_DirectorId}_{user_B5130_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5130_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_OrgUnit_DirectorId,
                        UserId = user_B5130_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
            // OrgUnit Deputy Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5130_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5130_OrgUnit_Deputy_DirectorId))
            {
                var key_B5130_OrgUnit_Deputy_Director = $"{orgUnit_B5130Id}_{role_B5130_OrgUnit_Deputy_DirectorId}_{user_B5130_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5130_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5130Id} - {user_B5130_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5130Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5130_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5130_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5130");
        }

        // B5131
        if (codeToOrgUnitId.TryGetValue("B5131", out var orgUnit_B5131Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5131_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5131_Region_DirectorId))
            {
                var key_B5131_Region_Director = $"{orgUnit_B5131Id}_{role_B5131_Region_DirectorId}_{user_B5131_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5131_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_Region_DirectorId}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_Region_DirectorId,
                        UserId = user_B5131_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5131_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5131_Region_Deputy_DirectorId))
            {
                var key_B5131_Region_Deputy_Director = $"{orgUnit_B5131Id}_{role_B5131_Region_Deputy_DirectorId}_{user_B5131_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5131_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_Region_Deputy_DirectorId,
                        UserId = user_B5131_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // Hub Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5131_Hub_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5131_Hub_DirectorId))
            {
                var key_B5131_Hub_Director = $"{orgUnit_B5131Id}_{role_B5131_Hub_DirectorId}_{user_B5131_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5131_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_Hub_DirectorId}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_Hub_DirectorId,
                        UserId = user_B5131_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // OrgUnit Director: sabinek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5131_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("sabinek@unops.org", out var user_B5131_OrgUnit_DirectorId))
            {
                var key_B5131_OrgUnit_Director = $"{orgUnit_B5131Id}_{role_B5131_OrgUnit_DirectorId}_{user_B5131_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5131_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_OrgUnit_DirectorId,
                        UserId = user_B5131_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("sabinek@unops.org")) missingUsers.Add("sabinek@unops.org");
            }
            // OrgUnit Deputy Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5131_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5131_OrgUnit_Deputy_DirectorId))
            {
                var key_B5131_OrgUnit_Deputy_Director = $"{orgUnit_B5131Id}_{role_B5131_OrgUnit_Deputy_DirectorId}_{user_B5131_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5131_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5131Id} - {user_B5131_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5131Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5131_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5131_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5131");
        }

        // B5132
        if (codeToOrgUnitId.TryGetValue("B5132", out var orgUnit_B5132Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5132_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5132_Region_DirectorId))
            {
                var key_B5132_Region_Director = $"{orgUnit_B5132Id}_{role_B5132_Region_DirectorId}_{user_B5132_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5132_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_Region_DirectorId}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_Region_DirectorId,
                        UserId = user_B5132_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5132_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5132_Region_Deputy_DirectorId))
            {
                var key_B5132_Region_Deputy_Director = $"{orgUnit_B5132Id}_{role_B5132_Region_Deputy_DirectorId}_{user_B5132_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5132_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_Region_Deputy_DirectorId,
                        UserId = user_B5132_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // Hub Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5132_Hub_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5132_Hub_DirectorId))
            {
                var key_B5132_Hub_Director = $"{orgUnit_B5132Id}_{role_B5132_Hub_DirectorId}_{user_B5132_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5132_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_Hub_DirectorId}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_Hub_DirectorId,
                        UserId = user_B5132_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // OrgUnit Director: simonettas@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5132_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5132_OrgUnit_DirectorId))
            {
                var key_B5132_OrgUnit_Director = $"{orgUnit_B5132Id}_{role_B5132_OrgUnit_DirectorId}_{user_B5132_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5132_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_OrgUnit_DirectorId,
                        UserId = user_B5132_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
            // OrgUnit Deputy Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5132_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5132_OrgUnit_Deputy_DirectorId))
            {
                var key_B5132_OrgUnit_Deputy_Director = $"{orgUnit_B5132Id}_{role_B5132_OrgUnit_Deputy_DirectorId}_{user_B5132_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5132_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5132Id} - {user_B5132_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5132Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5132_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5132_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5132");
        }

        // B5133
        if (codeToOrgUnitId.TryGetValue("B5133", out var orgUnit_B5133Id))
        {
            // Region Director: timl@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5133_Region_DirectorId) &&
                emailToUserId.TryGetValue("timl@unops.org", out var user_B5133_Region_DirectorId))
            {
                var key_B5133_Region_Director = $"{orgUnit_B5133Id}_{role_B5133_Region_DirectorId}_{user_B5133_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5133_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_Region_DirectorId}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_Region_DirectorId,
                        UserId = user_B5133_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("timl@unops.org")) missingUsers.Add("timl@unops.org");
            }
            // Region Deputy Director: christaa@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5133_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("christaa@unops.org", out var user_B5133_Region_Deputy_DirectorId))
            {
                var key_B5133_Region_Deputy_Director = $"{orgUnit_B5133Id}_{role_B5133_Region_Deputy_DirectorId}_{user_B5133_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5133_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_Region_Deputy_DirectorId,
                        UserId = user_B5133_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("christaa@unops.org")) missingUsers.Add("christaa@unops.org");
            }
            // Hub Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5133_Hub_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5133_Hub_DirectorId))
            {
                var key_B5133_Hub_Director = $"{orgUnit_B5133Id}_{role_B5133_Hub_DirectorId}_{user_B5133_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5133_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_Hub_DirectorId}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_Hub_DirectorId,
                        UserId = user_B5133_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
            // OrgUnit Director: simonettas@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5133_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("simonettas@unops.org", out var user_B5133_OrgUnit_DirectorId))
            {
                var key_B5133_OrgUnit_Director = $"{orgUnit_B5133Id}_{role_B5133_OrgUnit_DirectorId}_{user_B5133_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5133_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_OrgUnit_DirectorId,
                        UserId = user_B5133_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("simonettas@unops.org")) missingUsers.Add("simonettas@unops.org");
            }
            // OrgUnit Deputy Director: gurelg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5133_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("gurelg@unops.org", out var user_B5133_OrgUnit_Deputy_DirectorId))
            {
                var key_B5133_OrgUnit_Deputy_Director = $"{orgUnit_B5133Id}_{role_B5133_OrgUnit_Deputy_DirectorId}_{user_B5133_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5133_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5133Id} - {user_B5133_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5133Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5133_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5133_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("gurelg@unops.org")) missingUsers.Add("gurelg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5133");
        }

        // B5301
        if (codeToOrgUnitId.TryGetValue("B5301", out var orgUnit_B5301Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5301_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5301_Region_DirectorId))
            {
                var key_B5301_Region_Director = $"{orgUnit_B5301Id}_{role_B5301_Region_DirectorId}_{user_B5301_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5301_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_Region_DirectorId}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_Region_DirectorId,
                        UserId = user_B5301_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5301_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5301_Hub_DirectorId))
            {
                var key_B5301_Hub_Director = $"{orgUnit_B5301Id}_{role_B5301_Hub_DirectorId}_{user_B5301_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5301_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_Hub_DirectorId}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_Hub_DirectorId,
                        UserId = user_B5301_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5301_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5301_Hub_Deputy_DirectorId))
            {
                var key_B5301_Hub_Deputy_Director = $"{orgUnit_B5301Id}_{role_B5301_Hub_Deputy_DirectorId}_{user_B5301_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5301_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_Hub_Deputy_DirectorId,
                        UserId = user_B5301_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5301_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5301_OrgUnit_DirectorId))
            {
                var key_B5301_OrgUnit_Director = $"{orgUnit_B5301Id}_{role_B5301_OrgUnit_DirectorId}_{user_B5301_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5301_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_OrgUnit_DirectorId,
                        UserId = user_B5301_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5301_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5301_OrgUnit_Deputy_DirectorId))
            {
                var key_B5301_OrgUnit_Deputy_Director = $"{orgUnit_B5301Id}_{role_B5301_OrgUnit_Deputy_DirectorId}_{user_B5301_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5301_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5301Id} - {user_B5301_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5301Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5301_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5301_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5301");
        }

        // B5302
        if (codeToOrgUnitId.TryGetValue("B5302", out var orgUnit_B5302Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5302_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5302_Region_DirectorId))
            {
                var key_B5302_Region_Director = $"{orgUnit_B5302Id}_{role_B5302_Region_DirectorId}_{user_B5302_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5302_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_Region_DirectorId}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_Region_DirectorId,
                        UserId = user_B5302_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5302_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5302_Hub_DirectorId))
            {
                var key_B5302_Hub_Director = $"{orgUnit_B5302Id}_{role_B5302_Hub_DirectorId}_{user_B5302_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5302_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_Hub_DirectorId}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_Hub_DirectorId,
                        UserId = user_B5302_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5302_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5302_OrgUnit_DirectorId))
            {
                var key_B5302_OrgUnit_Director = $"{orgUnit_B5302Id}_{role_B5302_OrgUnit_DirectorId}_{user_B5302_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5302_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_OrgUnit_DirectorId,
                        UserId = user_B5302_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5302_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5302_OrgUnit_Deputy_DirectorId))
            {
                var key_B5302_OrgUnit_Deputy_Director = $"{orgUnit_B5302Id}_{role_B5302_OrgUnit_Deputy_DirectorId}_{user_B5302_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5302_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5302Id} - {user_B5302_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5302Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5302_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5302_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5302");
        }

        // B5303
        if (codeToOrgUnitId.TryGetValue("B5303", out var orgUnit_B5303Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5303_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5303_Region_DirectorId))
            {
                var key_B5303_Region_Director = $"{orgUnit_B5303Id}_{role_B5303_Region_DirectorId}_{user_B5303_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5303_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5303Id} - {user_B5303_Region_DirectorId}",
                        EntityId = orgUnit_B5303Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5303_Region_DirectorId,
                        UserId = user_B5303_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5303_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5303_Hub_DirectorId))
            {
                var key_B5303_Hub_Director = $"{orgUnit_B5303Id}_{role_B5303_Hub_DirectorId}_{user_B5303_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5303_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5303Id} - {user_B5303_Hub_DirectorId}",
                        EntityId = orgUnit_B5303Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5303_Hub_DirectorId,
                        UserId = user_B5303_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5303_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5303_OrgUnit_DirectorId))
            {
                var key_B5303_OrgUnit_Director = $"{orgUnit_B5303Id}_{role_B5303_OrgUnit_DirectorId}_{user_B5303_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5303_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5303Id} - {user_B5303_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5303Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5303_OrgUnit_DirectorId,
                        UserId = user_B5303_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5303");
        }

        // B5304
        if (codeToOrgUnitId.TryGetValue("B5304", out var orgUnit_B5304Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5304_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5304_Region_DirectorId))
            {
                var key_B5304_Region_Director = $"{orgUnit_B5304Id}_{role_B5304_Region_DirectorId}_{user_B5304_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5304_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_Region_DirectorId}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_Region_DirectorId,
                        UserId = user_B5304_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5304_Hub_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5304_Hub_DirectorId))
            {
                var key_B5304_Hub_Director = $"{orgUnit_B5304Id}_{role_B5304_Hub_DirectorId}_{user_B5304_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5304_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_Hub_DirectorId}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_Hub_DirectorId,
                        UserId = user_B5304_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // Hub Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5304_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5304_Hub_Deputy_DirectorId))
            {
                var key_B5304_Hub_Deputy_Director = $"{orgUnit_B5304Id}_{role_B5304_Hub_Deputy_DirectorId}_{user_B5304_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5304_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_Hub_Deputy_DirectorId,
                        UserId = user_B5304_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // OrgUnit Director: petronellah@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5304_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("petronellah@unops.org", out var user_B5304_OrgUnit_DirectorId))
            {
                var key_B5304_OrgUnit_Director = $"{orgUnit_B5304Id}_{role_B5304_OrgUnit_DirectorId}_{user_B5304_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5304_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_OrgUnit_DirectorId,
                        UserId = user_B5304_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("petronellah@unops.org")) missingUsers.Add("petronellah@unops.org");
            }
            // OrgUnit Deputy Director: georgeic@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5304_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("georgeic@unops.org", out var user_B5304_OrgUnit_Deputy_DirectorId))
            {
                var key_B5304_OrgUnit_Deputy_Director = $"{orgUnit_B5304Id}_{role_B5304_OrgUnit_Deputy_DirectorId}_{user_B5304_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5304_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5304Id} - {user_B5304_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5304Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5304_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5304_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("georgeic@unops.org")) missingUsers.Add("georgeic@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5304");
        }

        // B5305
        if (codeToOrgUnitId.TryGetValue("B5305", out var orgUnit_B5305Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5305_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5305_Region_DirectorId))
            {
                var key_B5305_Region_Director = $"{orgUnit_B5305Id}_{role_B5305_Region_DirectorId}_{user_B5305_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5305_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_Region_DirectorId}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_Region_DirectorId,
                        UserId = user_B5305_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5305_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5305_Hub_DirectorId))
            {
                var key_B5305_Hub_Director = $"{orgUnit_B5305Id}_{role_B5305_Hub_DirectorId}_{user_B5305_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5305_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_Hub_DirectorId}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_Hub_DirectorId,
                        UserId = user_B5305_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5305_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5305_OrgUnit_DirectorId))
            {
                var key_B5305_OrgUnit_Director = $"{orgUnit_B5305Id}_{role_B5305_OrgUnit_DirectorId}_{user_B5305_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5305_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_OrgUnit_DirectorId,
                        UserId = user_B5305_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5305_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5305_OrgUnit_Deputy_DirectorId))
            {
                var key_B5305_OrgUnit_Deputy_Director = $"{orgUnit_B5305Id}_{role_B5305_OrgUnit_Deputy_DirectorId}_{user_B5305_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5305_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5305Id} - {user_B5305_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5305Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5305_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5305_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5305");
        }

        // B5306
        if (codeToOrgUnitId.TryGetValue("B5306", out var orgUnit_B5306Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5306_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5306_Region_DirectorId))
            {
                var key_B5306_Region_Director = $"{orgUnit_B5306Id}_{role_B5306_Region_DirectorId}_{user_B5306_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5306_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_Region_DirectorId}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_Region_DirectorId,
                        UserId = user_B5306_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5306_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5306_Hub_DirectorId))
            {
                var key_B5306_Hub_Director = $"{orgUnit_B5306Id}_{role_B5306_Hub_DirectorId}_{user_B5306_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5306_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_Hub_DirectorId}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_Hub_DirectorId,
                        UserId = user_B5306_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5306_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5306_Hub_Deputy_DirectorId))
            {
                var key_B5306_Hub_Deputy_Director = $"{orgUnit_B5306Id}_{role_B5306_Hub_Deputy_DirectorId}_{user_B5306_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5306_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_Hub_Deputy_DirectorId,
                        UserId = user_B5306_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5306_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5306_OrgUnit_DirectorId))
            {
                var key_B5306_OrgUnit_Director = $"{orgUnit_B5306Id}_{role_B5306_OrgUnit_DirectorId}_{user_B5306_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5306_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_OrgUnit_DirectorId,
                        UserId = user_B5306_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5306_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5306_OrgUnit_Deputy_DirectorId))
            {
                var key_B5306_OrgUnit_Deputy_Director = $"{orgUnit_B5306Id}_{role_B5306_OrgUnit_Deputy_DirectorId}_{user_B5306_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5306_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5306Id} - {user_B5306_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5306Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5306_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5306_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5306");
        }

        // B5308
        if (codeToOrgUnitId.TryGetValue("B5308", out var orgUnit_B5308Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5308_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5308_Region_DirectorId))
            {
                var key_B5308_Region_Director = $"{orgUnit_B5308Id}_{role_B5308_Region_DirectorId}_{user_B5308_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5308_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_Region_DirectorId}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_Region_DirectorId,
                        UserId = user_B5308_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5308_Hub_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5308_Hub_DirectorId))
            {
                var key_B5308_Hub_Director = $"{orgUnit_B5308Id}_{role_B5308_Hub_DirectorId}_{user_B5308_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5308_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_Hub_DirectorId}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_Hub_DirectorId,
                        UserId = user_B5308_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // Hub Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5308_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5308_Hub_Deputy_DirectorId))
            {
                var key_B5308_Hub_Deputy_Director = $"{orgUnit_B5308Id}_{role_B5308_Hub_Deputy_DirectorId}_{user_B5308_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5308_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_Hub_Deputy_DirectorId,
                        UserId = user_B5308_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // OrgUnit Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5308_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5308_OrgUnit_DirectorId))
            {
                var key_B5308_OrgUnit_Director = $"{orgUnit_B5308Id}_{role_B5308_OrgUnit_DirectorId}_{user_B5308_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5308_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_OrgUnit_DirectorId,
                        UserId = user_B5308_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // OrgUnit Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5308_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5308_OrgUnit_Deputy_DirectorId))
            {
                var key_B5308_OrgUnit_Deputy_Director = $"{orgUnit_B5308Id}_{role_B5308_OrgUnit_Deputy_DirectorId}_{user_B5308_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5308_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5308Id} - {user_B5308_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5308Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5308_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5308_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5308");
        }

        // B5309
        if (codeToOrgUnitId.TryGetValue("B5309", out var orgUnit_B5309Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5309_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5309_Region_DirectorId))
            {
                var key_B5309_Region_Director = $"{orgUnit_B5309Id}_{role_B5309_Region_DirectorId}_{user_B5309_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5309_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_Region_DirectorId}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_Region_DirectorId,
                        UserId = user_B5309_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5309_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5309_Hub_DirectorId))
            {
                var key_B5309_Hub_Director = $"{orgUnit_B5309Id}_{role_B5309_Hub_DirectorId}_{user_B5309_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5309_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_Hub_DirectorId}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_Hub_DirectorId,
                        UserId = user_B5309_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5309_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5309_OrgUnit_DirectorId))
            {
                var key_B5309_OrgUnit_Director = $"{orgUnit_B5309Id}_{role_B5309_OrgUnit_DirectorId}_{user_B5309_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5309_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_OrgUnit_DirectorId,
                        UserId = user_B5309_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5309_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5309_OrgUnit_Deputy_DirectorId))
            {
                var key_B5309_OrgUnit_Deputy_Director = $"{orgUnit_B5309Id}_{role_B5309_OrgUnit_Deputy_DirectorId}_{user_B5309_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5309_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5309Id} - {user_B5309_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5309Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5309_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5309_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5309");
        }

        // B5310
        if (codeToOrgUnitId.TryGetValue("B5310", out var orgUnit_B5310Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5310_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5310_Region_DirectorId))
            {
                var key_B5310_Region_Director = $"{orgUnit_B5310Id}_{role_B5310_Region_DirectorId}_{user_B5310_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5310_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_Region_DirectorId}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_Region_DirectorId,
                        UserId = user_B5310_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5310_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5310_Hub_DirectorId))
            {
                var key_B5310_Hub_Director = $"{orgUnit_B5310Id}_{role_B5310_Hub_DirectorId}_{user_B5310_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5310_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_Hub_DirectorId}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_Hub_DirectorId,
                        UserId = user_B5310_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: michaeld@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5310_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("michaeld@unops.org", out var user_B5310_OrgUnit_DirectorId))
            {
                var key_B5310_OrgUnit_Director = $"{orgUnit_B5310Id}_{role_B5310_OrgUnit_DirectorId}_{user_B5310_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5310_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_OrgUnit_DirectorId,
                        UserId = user_B5310_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("michaeld@unops.org")) missingUsers.Add("michaeld@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5310_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5310_OrgUnit_Deputy_DirectorId))
            {
                var key_B5310_OrgUnit_Deputy_Director = $"{orgUnit_B5310Id}_{role_B5310_OrgUnit_Deputy_DirectorId}_{user_B5310_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5310_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5310Id} - {user_B5310_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5310Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5310_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5310_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5310");
        }

        // B5311
        if (codeToOrgUnitId.TryGetValue("B5311", out var orgUnit_B5311Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5311_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5311_Region_DirectorId))
            {
                var key_B5311_Region_Director = $"{orgUnit_B5311Id}_{role_B5311_Region_DirectorId}_{user_B5311_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5311_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_Region_DirectorId}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_Region_DirectorId,
                        UserId = user_B5311_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // OrgUnit Director: alaan@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5311_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("alaan@unops.org", out var user_B5311_OrgUnit_DirectorId))
            {
                var key_B5311_OrgUnit_Director = $"{orgUnit_B5311Id}_{role_B5311_OrgUnit_DirectorId}_{user_B5311_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5311_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_OrgUnit_DirectorId,
                        UserId = user_B5311_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("alaan@unops.org")) missingUsers.Add("alaan@unops.org");
            }
            // OrgUnit Deputy Director: venelinr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5311_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("venelinr@unops.org", out var user_B5311_OrgUnit_Deputy_DirectorId))
            {
                var key_B5311_OrgUnit_Deputy_Director = $"{orgUnit_B5311Id}_{role_B5311_OrgUnit_Deputy_DirectorId}_{user_B5311_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5311_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5311Id} - {user_B5311_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5311Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5311_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5311_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("venelinr@unops.org")) missingUsers.Add("venelinr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5311");
        }

        // B5312
        if (codeToOrgUnitId.TryGetValue("B5312", out var orgUnit_B5312Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5312_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5312_Region_DirectorId))
            {
                var key_B5312_Region_Director = $"{orgUnit_B5312Id}_{role_B5312_Region_DirectorId}_{user_B5312_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5312_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_Region_DirectorId}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_Region_DirectorId,
                        UserId = user_B5312_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5312_Hub_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5312_Hub_DirectorId))
            {
                var key_B5312_Hub_Director = $"{orgUnit_B5312Id}_{role_B5312_Hub_DirectorId}_{user_B5312_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5312_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_Hub_DirectorId}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_Hub_DirectorId,
                        UserId = user_B5312_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // Hub Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5312_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5312_Hub_Deputy_DirectorId))
            {
                var key_B5312_Hub_Deputy_Director = $"{orgUnit_B5312Id}_{role_B5312_Hub_Deputy_DirectorId}_{user_B5312_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5312_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_Hub_Deputy_DirectorId,
                        UserId = user_B5312_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // OrgUnit Director: munierm@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5312_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("munierm@unops.org", out var user_B5312_OrgUnit_DirectorId))
            {
                var key_B5312_OrgUnit_Director = $"{orgUnit_B5312Id}_{role_B5312_OrgUnit_DirectorId}_{user_B5312_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5312_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_OrgUnit_DirectorId,
                        UserId = user_B5312_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("munierm@unops.org")) missingUsers.Add("munierm@unops.org");
            }
            // OrgUnit Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5312_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5312_OrgUnit_Deputy_DirectorId))
            {
                var key_B5312_OrgUnit_Deputy_Director = $"{orgUnit_B5312Id}_{role_B5312_OrgUnit_Deputy_DirectorId}_{user_B5312_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5312_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5312Id} - {user_B5312_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5312Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5312_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5312_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5312");
        }

        // B5313
        if (codeToOrgUnitId.TryGetValue("B5313", out var orgUnit_B5313Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5313_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5313_Region_DirectorId))
            {
                var key_B5313_Region_Director = $"{orgUnit_B5313Id}_{role_B5313_Region_DirectorId}_{user_B5313_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5313_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_Region_DirectorId}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_Region_DirectorId,
                        UserId = user_B5313_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5313_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5313_Hub_DirectorId))
            {
                var key_B5313_Hub_Director = $"{orgUnit_B5313Id}_{role_B5313_Hub_DirectorId}_{user_B5313_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5313_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_Hub_DirectorId}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_Hub_DirectorId,
                        UserId = user_B5313_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5313_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5313_Hub_Deputy_DirectorId))
            {
                var key_B5313_Hub_Deputy_Director = $"{orgUnit_B5313Id}_{role_B5313_Hub_Deputy_DirectorId}_{user_B5313_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5313_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_Hub_Deputy_DirectorId,
                        UserId = user_B5313_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5313_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5313_OrgUnit_DirectorId))
            {
                var key_B5313_OrgUnit_Director = $"{orgUnit_B5313Id}_{role_B5313_OrgUnit_DirectorId}_{user_B5313_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5313_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_OrgUnit_DirectorId,
                        UserId = user_B5313_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5313_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5313_OrgUnit_Deputy_DirectorId))
            {
                var key_B5313_OrgUnit_Deputy_Director = $"{orgUnit_B5313Id}_{role_B5313_OrgUnit_Deputy_DirectorId}_{user_B5313_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5313_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5313Id} - {user_B5313_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5313Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5313_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5313_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5313");
        }

        // B5314
        if (codeToOrgUnitId.TryGetValue("B5314", out var orgUnit_B5314Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5314_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5314_Region_DirectorId))
            {
                var key_B5314_Region_Director = $"{orgUnit_B5314Id}_{role_B5314_Region_DirectorId}_{user_B5314_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5314_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_Region_DirectorId}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_Region_DirectorId,
                        UserId = user_B5314_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5314_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5314_Hub_DirectorId))
            {
                var key_B5314_Hub_Director = $"{orgUnit_B5314Id}_{role_B5314_Hub_DirectorId}_{user_B5314_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5314_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_Hub_DirectorId}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_Hub_DirectorId,
                        UserId = user_B5314_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: lindag@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5314_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("lindag@unops.org", out var user_B5314_OrgUnit_DirectorId))
            {
                var key_B5314_OrgUnit_Director = $"{orgUnit_B5314Id}_{role_B5314_OrgUnit_DirectorId}_{user_B5314_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5314_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_OrgUnit_DirectorId,
                        UserId = user_B5314_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("lindag@unops.org")) missingUsers.Add("lindag@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5314_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5314_OrgUnit_Deputy_DirectorId))
            {
                var key_B5314_OrgUnit_Deputy_Director = $"{orgUnit_B5314Id}_{role_B5314_OrgUnit_Deputy_DirectorId}_{user_B5314_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5314_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5314Id} - {user_B5314_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5314Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5314_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5314_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5314");
        }

        // B5315
        if (codeToOrgUnitId.TryGetValue("B5315", out var orgUnit_B5315Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5315_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5315_Region_DirectorId))
            {
                var key_B5315_Region_Director = $"{orgUnit_B5315Id}_{role_B5315_Region_DirectorId}_{user_B5315_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5315_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_Region_DirectorId}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_Region_DirectorId,
                        UserId = user_B5315_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5315_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5315_Hub_DirectorId))
            {
                var key_B5315_Hub_Director = $"{orgUnit_B5315Id}_{role_B5315_Hub_DirectorId}_{user_B5315_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5315_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_Hub_DirectorId}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_Hub_DirectorId,
                        UserId = user_B5315_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5315_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5315_OrgUnit_DirectorId))
            {
                var key_B5315_OrgUnit_Director = $"{orgUnit_B5315Id}_{role_B5315_OrgUnit_DirectorId}_{user_B5315_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5315_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_OrgUnit_DirectorId,
                        UserId = user_B5315_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5315_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5315_OrgUnit_Deputy_DirectorId))
            {
                var key_B5315_OrgUnit_Deputy_Director = $"{orgUnit_B5315Id}_{role_B5315_OrgUnit_Deputy_DirectorId}_{user_B5315_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5315_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5315Id} - {user_B5315_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5315Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5315_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5315_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5315");
        }

        // B5316
        if (codeToOrgUnitId.TryGetValue("B5316", out var orgUnit_B5316Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5316_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5316_Region_DirectorId))
            {
                var key_B5316_Region_Director = $"{orgUnit_B5316Id}_{role_B5316_Region_DirectorId}_{user_B5316_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5316_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_Region_DirectorId}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_Region_DirectorId,
                        UserId = user_B5316_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5316_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5316_Hub_DirectorId))
            {
                var key_B5316_Hub_Director = $"{orgUnit_B5316Id}_{role_B5316_Hub_DirectorId}_{user_B5316_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5316_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_Hub_DirectorId}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_Hub_DirectorId,
                        UserId = user_B5316_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: hazelgn@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5316_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("hazelgn@unops.org", out var user_B5316_OrgUnit_DirectorId))
            {
                var key_B5316_OrgUnit_Director = $"{orgUnit_B5316Id}_{role_B5316_OrgUnit_DirectorId}_{user_B5316_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5316_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_OrgUnit_DirectorId,
                        UserId = user_B5316_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("hazelgn@unops.org")) missingUsers.Add("hazelgn@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5316_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5316_OrgUnit_Deputy_DirectorId))
            {
                var key_B5316_OrgUnit_Deputy_Director = $"{orgUnit_B5316Id}_{role_B5316_OrgUnit_Deputy_DirectorId}_{user_B5316_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5316_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5316Id} - {user_B5316_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5316Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5316_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5316_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5316");
        }

        // B5317
        if (codeToOrgUnitId.TryGetValue("B5317", out var orgUnit_B5317Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5317_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5317_Region_DirectorId))
            {
                var key_B5317_Region_Director = $"{orgUnit_B5317Id}_{role_B5317_Region_DirectorId}_{user_B5317_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5317_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_Region_DirectorId}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_Region_DirectorId,
                        UserId = user_B5317_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5317_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5317_Hub_DirectorId))
            {
                var key_B5317_Hub_Director = $"{orgUnit_B5317Id}_{role_B5317_Hub_DirectorId}_{user_B5317_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5317_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_Hub_DirectorId}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_Hub_DirectorId,
                        UserId = user_B5317_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: josho@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5317_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("josho@unops.org", out var user_B5317_OrgUnit_DirectorId))
            {
                var key_B5317_OrgUnit_Director = $"{orgUnit_B5317Id}_{role_B5317_OrgUnit_DirectorId}_{user_B5317_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5317_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_OrgUnit_DirectorId,
                        UserId = user_B5317_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("josho@unops.org")) missingUsers.Add("josho@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5317_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5317_OrgUnit_Deputy_DirectorId))
            {
                var key_B5317_OrgUnit_Deputy_Director = $"{orgUnit_B5317Id}_{role_B5317_OrgUnit_Deputy_DirectorId}_{user_B5317_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5317_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5317Id} - {user_B5317_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5317Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5317_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5317_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5317");
        }

        // B5318
        if (codeToOrgUnitId.TryGetValue("B5318", out var orgUnit_B5318Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5318_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5318_Region_DirectorId))
            {
                var key_B5318_Region_Director = $"{orgUnit_B5318Id}_{role_B5318_Region_DirectorId}_{user_B5318_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5318_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_Region_DirectorId}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_Region_DirectorId,
                        UserId = user_B5318_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5318_Hub_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5318_Hub_DirectorId))
            {
                var key_B5318_Hub_Director = $"{orgUnit_B5318Id}_{role_B5318_Hub_DirectorId}_{user_B5318_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5318_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_Hub_DirectorId}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_Hub_DirectorId,
                        UserId = user_B5318_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // Hub Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5318_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5318_Hub_Deputy_DirectorId))
            {
                var key_B5318_Hub_Deputy_Director = $"{orgUnit_B5318Id}_{role_B5318_Hub_Deputy_DirectorId}_{user_B5318_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5318_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_Hub_Deputy_DirectorId,
                        UserId = user_B5318_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // OrgUnit Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5318_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5318_OrgUnit_DirectorId))
            {
                var key_B5318_OrgUnit_Director = $"{orgUnit_B5318Id}_{role_B5318_OrgUnit_DirectorId}_{user_B5318_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5318_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_OrgUnit_DirectorId,
                        UserId = user_B5318_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // OrgUnit Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5318_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5318_OrgUnit_Deputy_DirectorId))
            {
                var key_B5318_OrgUnit_Deputy_Director = $"{orgUnit_B5318Id}_{role_B5318_OrgUnit_Deputy_DirectorId}_{user_B5318_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5318_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5318Id} - {user_B5318_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5318Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5318_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5318_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5318");
        }

        // B5320
        if (codeToOrgUnitId.TryGetValue("B5320", out var orgUnit_B5320Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5320_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5320_Region_DirectorId))
            {
                var key_B5320_Region_Director = $"{orgUnit_B5320Id}_{role_B5320_Region_DirectorId}_{user_B5320_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5320_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_Region_DirectorId}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_Region_DirectorId,
                        UserId = user_B5320_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5320_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5320_Hub_DirectorId))
            {
                var key_B5320_Hub_Director = $"{orgUnit_B5320Id}_{role_B5320_Hub_DirectorId}_{user_B5320_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5320_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_Hub_DirectorId}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_Hub_DirectorId,
                        UserId = user_B5320_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: adamada@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5320_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("adamada@unops.org", out var user_B5320_OrgUnit_DirectorId))
            {
                var key_B5320_OrgUnit_Director = $"{orgUnit_B5320Id}_{role_B5320_OrgUnit_DirectorId}_{user_B5320_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5320_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_OrgUnit_DirectorId,
                        UserId = user_B5320_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("adamada@unops.org")) missingUsers.Add("adamada@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5320_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5320_OrgUnit_Deputy_DirectorId))
            {
                var key_B5320_OrgUnit_Deputy_Director = $"{orgUnit_B5320Id}_{role_B5320_OrgUnit_Deputy_DirectorId}_{user_B5320_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5320_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5320Id} - {user_B5320_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5320Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5320_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5320_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5320");
        }

        // B5321
        if (codeToOrgUnitId.TryGetValue("B5321", out var orgUnit_B5321Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5321_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5321_Region_DirectorId))
            {
                var key_B5321_Region_Director = $"{orgUnit_B5321Id}_{role_B5321_Region_DirectorId}_{user_B5321_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5321_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_Region_DirectorId}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_Region_DirectorId,
                        UserId = user_B5321_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5321_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5321_Hub_DirectorId))
            {
                var key_B5321_Hub_Director = $"{orgUnit_B5321Id}_{role_B5321_Hub_DirectorId}_{user_B5321_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5321_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_Hub_DirectorId}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_Hub_DirectorId,
                        UserId = user_B5321_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: hubertd@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5321_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("hubertd@unops.org", out var user_B5321_OrgUnit_DirectorId))
            {
                var key_B5321_OrgUnit_Director = $"{orgUnit_B5321Id}_{role_B5321_OrgUnit_DirectorId}_{user_B5321_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5321_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_OrgUnit_DirectorId,
                        UserId = user_B5321_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("hubertd@unops.org")) missingUsers.Add("hubertd@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5321_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5321_OrgUnit_Deputy_DirectorId))
            {
                var key_B5321_OrgUnit_Deputy_Director = $"{orgUnit_B5321Id}_{role_B5321_OrgUnit_Deputy_DirectorId}_{user_B5321_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5321_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5321Id} - {user_B5321_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5321Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5321_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5321_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5321");
        }

        // B5322
        if (codeToOrgUnitId.TryGetValue("B5322", out var orgUnit_B5322Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5322_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5322_Region_DirectorId))
            {
                var key_B5322_Region_Director = $"{orgUnit_B5322Id}_{role_B5322_Region_DirectorId}_{user_B5322_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5322_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_Region_DirectorId}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_Region_DirectorId,
                        UserId = user_B5322_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5322_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5322_Hub_DirectorId))
            {
                var key_B5322_Hub_Director = $"{orgUnit_B5322Id}_{role_B5322_Hub_DirectorId}_{user_B5322_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5322_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_Hub_DirectorId}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_Hub_DirectorId,
                        UserId = user_B5322_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: genevievel@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5322_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("genevievel@unops.org", out var user_B5322_OrgUnit_DirectorId))
            {
                var key_B5322_OrgUnit_Director = $"{orgUnit_B5322Id}_{role_B5322_OrgUnit_DirectorId}_{user_B5322_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5322_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_OrgUnit_DirectorId,
                        UserId = user_B5322_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("genevievel@unops.org")) missingUsers.Add("genevievel@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5322_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5322_OrgUnit_Deputy_DirectorId))
            {
                var key_B5322_OrgUnit_Deputy_Director = $"{orgUnit_B5322Id}_{role_B5322_OrgUnit_Deputy_DirectorId}_{user_B5322_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5322_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5322Id} - {user_B5322_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5322Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5322_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5322_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5322");
        }

        // B5323
        if (codeToOrgUnitId.TryGetValue("B5323", out var orgUnit_B5323Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5323_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5323_Region_DirectorId))
            {
                var key_B5323_Region_Director = $"{orgUnit_B5323Id}_{role_B5323_Region_DirectorId}_{user_B5323_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5323_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5323Id} - {user_B5323_Region_DirectorId}",
                        EntityId = orgUnit_B5323Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5323_Region_DirectorId,
                        UserId = user_B5323_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5323_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5323_Hub_DirectorId))
            {
                var key_B5323_Hub_Director = $"{orgUnit_B5323Id}_{role_B5323_Hub_DirectorId}_{user_B5323_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5323_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5323Id} - {user_B5323_Hub_DirectorId}",
                        EntityId = orgUnit_B5323Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5323_Hub_DirectorId,
                        UserId = user_B5323_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5323_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5323_OrgUnit_DirectorId))
            {
                var key_B5323_OrgUnit_Director = $"{orgUnit_B5323Id}_{role_B5323_OrgUnit_DirectorId}_{user_B5323_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5323_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5323Id} - {user_B5323_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5323Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5323_OrgUnit_DirectorId,
                        UserId = user_B5323_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5323");
        }

        // B5324
        if (codeToOrgUnitId.TryGetValue("B5324", out var orgUnit_B5324Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5324_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5324_Region_DirectorId))
            {
                var key_B5324_Region_Director = $"{orgUnit_B5324Id}_{role_B5324_Region_DirectorId}_{user_B5324_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5324_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_Region_DirectorId}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_Region_DirectorId,
                        UserId = user_B5324_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5324_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5324_Hub_DirectorId))
            {
                var key_B5324_Hub_Director = $"{orgUnit_B5324Id}_{role_B5324_Hub_DirectorId}_{user_B5324_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5324_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_Hub_DirectorId}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_Hub_DirectorId,
                        UserId = user_B5324_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5324_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5324_Hub_Deputy_DirectorId))
            {
                var key_B5324_Hub_Deputy_Director = $"{orgUnit_B5324Id}_{role_B5324_Hub_Deputy_DirectorId}_{user_B5324_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5324_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_Hub_Deputy_DirectorId,
                        UserId = user_B5324_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5324_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5324_OrgUnit_DirectorId))
            {
                var key_B5324_OrgUnit_Director = $"{orgUnit_B5324Id}_{role_B5324_OrgUnit_DirectorId}_{user_B5324_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5324_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_OrgUnit_DirectorId,
                        UserId = user_B5324_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5324_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5324_OrgUnit_Deputy_DirectorId))
            {
                var key_B5324_OrgUnit_Deputy_Director = $"{orgUnit_B5324Id}_{role_B5324_OrgUnit_Deputy_DirectorId}_{user_B5324_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5324_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5324Id} - {user_B5324_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5324Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5324_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5324_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5324");
        }

        // B5325
        if (codeToOrgUnitId.TryGetValue("B5325", out var orgUnit_B5325Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5325_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5325_Region_DirectorId))
            {
                var key_B5325_Region_Director = $"{orgUnit_B5325Id}_{role_B5325_Region_DirectorId}_{user_B5325_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5325_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_Region_DirectorId}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_Region_DirectorId,
                        UserId = user_B5325_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5325_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5325_Hub_DirectorId))
            {
                var key_B5325_Hub_Director = $"{orgUnit_B5325Id}_{role_B5325_Hub_DirectorId}_{user_B5325_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5325_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_Hub_DirectorId}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_Hub_DirectorId,
                        UserId = user_B5325_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5325_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5325_Hub_Deputy_DirectorId))
            {
                var key_B5325_Hub_Deputy_Director = $"{orgUnit_B5325Id}_{role_B5325_Hub_Deputy_DirectorId}_{user_B5325_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5325_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_Hub_Deputy_DirectorId,
                        UserId = user_B5325_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5325_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5325_OrgUnit_DirectorId))
            {
                var key_B5325_OrgUnit_Director = $"{orgUnit_B5325Id}_{role_B5325_OrgUnit_DirectorId}_{user_B5325_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5325_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_OrgUnit_DirectorId,
                        UserId = user_B5325_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5325_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5325_OrgUnit_Deputy_DirectorId))
            {
                var key_B5325_OrgUnit_Deputy_Director = $"{orgUnit_B5325Id}_{role_B5325_OrgUnit_Deputy_DirectorId}_{user_B5325_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5325_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5325Id} - {user_B5325_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5325Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5325_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5325_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5325");
        }

        // B5327
        if (codeToOrgUnitId.TryGetValue("B5327", out var orgUnit_B5327Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5327_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5327_Region_DirectorId))
            {
                var key_B5327_Region_Director = $"{orgUnit_B5327Id}_{role_B5327_Region_DirectorId}_{user_B5327_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5327_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_Region_DirectorId}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_Region_DirectorId,
                        UserId = user_B5327_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5327_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5327_Hub_DirectorId))
            {
                var key_B5327_Hub_Director = $"{orgUnit_B5327Id}_{role_B5327_Hub_DirectorId}_{user_B5327_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5327_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_Hub_DirectorId}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_Hub_DirectorId,
                        UserId = user_B5327_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: sofiag@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5327_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("sofiag@unops.org", out var user_B5327_OrgUnit_DirectorId))
            {
                var key_B5327_OrgUnit_Director = $"{orgUnit_B5327Id}_{role_B5327_OrgUnit_DirectorId}_{user_B5327_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5327_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_OrgUnit_DirectorId,
                        UserId = user_B5327_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("sofiag@unops.org")) missingUsers.Add("sofiag@unops.org");
            }
            // OrgUnit Deputy Director: sharonle@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5327_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5327_OrgUnit_Deputy_DirectorId))
            {
                var key_B5327_OrgUnit_Deputy_Director = $"{orgUnit_B5327Id}_{role_B5327_OrgUnit_Deputy_DirectorId}_{user_B5327_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5327_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5327Id} - {user_B5327_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5327Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5327_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5327_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5327");
        }

        // B5328
        if (codeToOrgUnitId.TryGetValue("B5328", out var orgUnit_B5328Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5328_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5328_Region_DirectorId))
            {
                var key_B5328_Region_Director = $"{orgUnit_B5328Id}_{role_B5328_Region_DirectorId}_{user_B5328_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5328_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_Region_DirectorId}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_Region_DirectorId,
                        UserId = user_B5328_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5328_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5328_Hub_DirectorId))
            {
                var key_B5328_Hub_Director = $"{orgUnit_B5328Id}_{role_B5328_Hub_DirectorId}_{user_B5328_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5328_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_Hub_DirectorId}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_Hub_DirectorId,
                        UserId = user_B5328_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5328_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5328_Hub_Deputy_DirectorId))
            {
                var key_B5328_Hub_Deputy_Director = $"{orgUnit_B5328Id}_{role_B5328_Hub_Deputy_DirectorId}_{user_B5328_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5328_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_Hub_Deputy_DirectorId,
                        UserId = user_B5328_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5328_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5328_OrgUnit_DirectorId))
            {
                var key_B5328_OrgUnit_Director = $"{orgUnit_B5328Id}_{role_B5328_OrgUnit_DirectorId}_{user_B5328_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5328_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_OrgUnit_DirectorId,
                        UserId = user_B5328_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5328_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5328_OrgUnit_Deputy_DirectorId))
            {
                var key_B5328_OrgUnit_Deputy_Director = $"{orgUnit_B5328Id}_{role_B5328_OrgUnit_Deputy_DirectorId}_{user_B5328_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5328_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5328Id} - {user_B5328_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5328Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5328_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5328_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5328");
        }

        // B5329
        if (codeToOrgUnitId.TryGetValue("B5329", out var orgUnit_B5329Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5329_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5329_Region_DirectorId))
            {
                var key_B5329_Region_Director = $"{orgUnit_B5329Id}_{role_B5329_Region_DirectorId}_{user_B5329_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5329_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_Region_DirectorId}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_Region_DirectorId,
                        UserId = user_B5329_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5329_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5329_Hub_DirectorId))
            {
                var key_B5329_Hub_Director = $"{orgUnit_B5329Id}_{role_B5329_Hub_DirectorId}_{user_B5329_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5329_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_Hub_DirectorId}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_Hub_DirectorId,
                        UserId = user_B5329_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: clementm@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5329_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("clementm@unops.org", out var user_B5329_OrgUnit_DirectorId))
            {
                var key_B5329_OrgUnit_Director = $"{orgUnit_B5329Id}_{role_B5329_OrgUnit_DirectorId}_{user_B5329_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5329_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_OrgUnit_DirectorId,
                        UserId = user_B5329_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("clementm@unops.org")) missingUsers.Add("clementm@unops.org");
            }
            // OrgUnit Deputy Director: sharonle@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5329_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5329_OrgUnit_Deputy_DirectorId))
            {
                var key_B5329_OrgUnit_Deputy_Director = $"{orgUnit_B5329Id}_{role_B5329_OrgUnit_Deputy_DirectorId}_{user_B5329_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5329_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5329Id} - {user_B5329_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5329Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5329_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5329_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5329");
        }

        // B5330
        if (codeToOrgUnitId.TryGetValue("B5330", out var orgUnit_B5330Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5330_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5330_Region_DirectorId))
            {
                var key_B5330_Region_Director = $"{orgUnit_B5330Id}_{role_B5330_Region_DirectorId}_{user_B5330_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5330_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_Region_DirectorId}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_Region_DirectorId,
                        UserId = user_B5330_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5330_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5330_Hub_DirectorId))
            {
                var key_B5330_Hub_Director = $"{orgUnit_B5330Id}_{role_B5330_Hub_DirectorId}_{user_B5330_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5330_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_Hub_DirectorId}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_Hub_DirectorId,
                        UserId = user_B5330_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: tidjaniw@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5330_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("tidjaniw@unops.org", out var user_B5330_OrgUnit_DirectorId))
            {
                var key_B5330_OrgUnit_Director = $"{orgUnit_B5330Id}_{role_B5330_OrgUnit_DirectorId}_{user_B5330_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5330_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_OrgUnit_DirectorId,
                        UserId = user_B5330_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("tidjaniw@unops.org")) missingUsers.Add("tidjaniw@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5330_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5330_OrgUnit_Deputy_DirectorId))
            {
                var key_B5330_OrgUnit_Deputy_Director = $"{orgUnit_B5330Id}_{role_B5330_OrgUnit_Deputy_DirectorId}_{user_B5330_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5330_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5330Id} - {user_B5330_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5330Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5330_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5330_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5330");
        }

        // B5331
        if (codeToOrgUnitId.TryGetValue("B5331", out var orgUnit_B5331Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5331_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5331_Region_DirectorId))
            {
                var key_B5331_Region_Director = $"{orgUnit_B5331Id}_{role_B5331_Region_DirectorId}_{user_B5331_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5331_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5331Id} - {user_B5331_Region_DirectorId}",
                        EntityId = orgUnit_B5331Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5331_Region_DirectorId,
                        UserId = user_B5331_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5331_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5331_Hub_DirectorId))
            {
                var key_B5331_Hub_Director = $"{orgUnit_B5331Id}_{role_B5331_Hub_DirectorId}_{user_B5331_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5331_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5331Id} - {user_B5331_Hub_DirectorId}",
                        EntityId = orgUnit_B5331Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5331_Hub_DirectorId,
                        UserId = user_B5331_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5331_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5331_OrgUnit_DirectorId))
            {
                var key_B5331_OrgUnit_Director = $"{orgUnit_B5331Id}_{role_B5331_OrgUnit_DirectorId}_{user_B5331_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5331_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5331Id} - {user_B5331_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5331Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5331_OrgUnit_DirectorId,
                        UserId = user_B5331_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5331");
        }

        // B5332
        if (codeToOrgUnitId.TryGetValue("B5332", out var orgUnit_B5332Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5332_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5332_Region_DirectorId))
            {
                var key_B5332_Region_Director = $"{orgUnit_B5332Id}_{role_B5332_Region_DirectorId}_{user_B5332_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5332_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_Region_DirectorId}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_Region_DirectorId,
                        UserId = user_B5332_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5332_Hub_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5332_Hub_DirectorId))
            {
                var key_B5332_Hub_Director = $"{orgUnit_B5332Id}_{role_B5332_Hub_DirectorId}_{user_B5332_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5332_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_Hub_DirectorId}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_Hub_DirectorId,
                        UserId = user_B5332_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Director: mariasg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5332_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("mariasg@unops.org", out var user_B5332_OrgUnit_DirectorId))
            {
                var key_B5332_OrgUnit_Director = $"{orgUnit_B5332Id}_{role_B5332_OrgUnit_DirectorId}_{user_B5332_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5332_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_OrgUnit_DirectorId,
                        UserId = user_B5332_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("mariasg@unops.org")) missingUsers.Add("mariasg@unops.org");
            }
            // OrgUnit Deputy Director: george@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5332_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("george@unops.org", out var user_B5332_OrgUnit_Deputy_DirectorId))
            {
                var key_B5332_OrgUnit_Deputy_Director = $"{orgUnit_B5332Id}_{role_B5332_OrgUnit_Deputy_DirectorId}_{user_B5332_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5332_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5332Id} - {user_B5332_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5332Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5332_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5332_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("george@unops.org")) missingUsers.Add("george@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5332");
        }

        // B5333
        if (codeToOrgUnitId.TryGetValue("B5333", out var orgUnit_B5333Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5333_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5333_Region_DirectorId))
            {
                var key_B5333_Region_Director = $"{orgUnit_B5333Id}_{role_B5333_Region_DirectorId}_{user_B5333_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5333_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_Region_DirectorId}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_Region_DirectorId,
                        UserId = user_B5333_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5333_Hub_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5333_Hub_DirectorId))
            {
                var key_B5333_Hub_Director = $"{orgUnit_B5333Id}_{role_B5333_Hub_DirectorId}_{user_B5333_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5333_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_Hub_DirectorId}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_Hub_DirectorId,
                        UserId = user_B5333_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // Hub Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5333_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5333_Hub_Deputy_DirectorId))
            {
                var key_B5333_Hub_Deputy_Director = $"{orgUnit_B5333Id}_{role_B5333_Hub_Deputy_DirectorId}_{user_B5333_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5333_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_Hub_Deputy_DirectorId,
                        UserId = user_B5333_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
            // OrgUnit Director: workneshg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5333_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("workneshg@unops.org", out var user_B5333_OrgUnit_DirectorId))
            {
                var key_B5333_OrgUnit_Director = $"{orgUnit_B5333Id}_{role_B5333_OrgUnit_DirectorId}_{user_B5333_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5333_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_OrgUnit_DirectorId,
                        UserId = user_B5333_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("workneshg@unops.org")) missingUsers.Add("workneshg@unops.org");
            }
            // OrgUnit Deputy Director: irenek@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5333_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("irenek@unops.org", out var user_B5333_OrgUnit_Deputy_DirectorId))
            {
                var key_B5333_OrgUnit_Deputy_Director = $"{orgUnit_B5333Id}_{role_B5333_OrgUnit_Deputy_DirectorId}_{user_B5333_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5333_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5333Id} - {user_B5333_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5333Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5333_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5333_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("irenek@unops.org")) missingUsers.Add("irenek@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5333");
        }

        // B5334
        if (codeToOrgUnitId.TryGetValue("B5334", out var orgUnit_B5334Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5334_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5334_Region_DirectorId))
            {
                var key_B5334_Region_Director = $"{orgUnit_B5334Id}_{role_B5334_Region_DirectorId}_{user_B5334_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5334_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5334Id} - {user_B5334_Region_DirectorId}",
                        EntityId = orgUnit_B5334Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5334_Region_DirectorId,
                        UserId = user_B5334_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5334_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5334_Hub_DirectorId))
            {
                var key_B5334_Hub_Director = $"{orgUnit_B5334Id}_{role_B5334_Hub_DirectorId}_{user_B5334_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5334_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5334Id} - {user_B5334_Hub_DirectorId}",
                        EntityId = orgUnit_B5334Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5334_Hub_DirectorId,
                        UserId = user_B5334_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5334_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5334_OrgUnit_DirectorId))
            {
                var key_B5334_OrgUnit_Director = $"{orgUnit_B5334Id}_{role_B5334_OrgUnit_DirectorId}_{user_B5334_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5334_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5334Id} - {user_B5334_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5334Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5334_OrgUnit_DirectorId,
                        UserId = user_B5334_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5334");
        }

        // B5335
        if (codeToOrgUnitId.TryGetValue("B5335", out var orgUnit_B5335Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5335_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5335_Region_DirectorId))
            {
                var key_B5335_Region_Director = $"{orgUnit_B5335Id}_{role_B5335_Region_DirectorId}_{user_B5335_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5335_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_Region_DirectorId}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_Region_DirectorId,
                        UserId = user_B5335_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5335_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5335_Hub_DirectorId))
            {
                var key_B5335_Hub_Director = $"{orgUnit_B5335Id}_{role_B5335_Hub_DirectorId}_{user_B5335_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5335_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_Hub_DirectorId}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_Hub_DirectorId,
                        UserId = user_B5335_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5335_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5335_OrgUnit_DirectorId))
            {
                var key_B5335_OrgUnit_Director = $"{orgUnit_B5335Id}_{role_B5335_OrgUnit_DirectorId}_{user_B5335_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5335_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_OrgUnit_DirectorId,
                        UserId = user_B5335_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Deputy Director: sharonle@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5335_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("sharonle@unops.org", out var user_B5335_OrgUnit_Deputy_DirectorId))
            {
                var key_B5335_OrgUnit_Deputy_Director = $"{orgUnit_B5335Id}_{role_B5335_OrgUnit_Deputy_DirectorId}_{user_B5335_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5335_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5335Id} - {user_B5335_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5335Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5335_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5335_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("sharonle@unops.org")) missingUsers.Add("sharonle@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5335");
        }

        // B5336
        if (codeToOrgUnitId.TryGetValue("B5336", out var orgUnit_B5336Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5336_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5336_Region_DirectorId))
            {
                var key_B5336_Region_Director = $"{orgUnit_B5336Id}_{role_B5336_Region_DirectorId}_{user_B5336_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5336_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5336Id} - {user_B5336_Region_DirectorId}",
                        EntityId = orgUnit_B5336Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5336_Region_DirectorId,
                        UserId = user_B5336_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: rainerf@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5336_Hub_DirectorId) &&
                emailToUserId.TryGetValue("rainerf@unops.org", out var user_B5336_Hub_DirectorId))
            {
                var key_B5336_Hub_Director = $"{orgUnit_B5336Id}_{role_B5336_Hub_DirectorId}_{user_B5336_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5336_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5336Id} - {user_B5336_Hub_DirectorId}",
                        EntityId = orgUnit_B5336Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5336_Hub_DirectorId,
                        UserId = user_B5336_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("rainerf@unops.org")) missingUsers.Add("rainerf@unops.org");
            }
            // OrgUnit Director: sayedf@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5336_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("sayedf@unops.org", out var user_B5336_OrgUnit_DirectorId))
            {
                var key_B5336_OrgUnit_Director = $"{orgUnit_B5336Id}_{role_B5336_OrgUnit_DirectorId}_{user_B5336_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5336_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5336Id} - {user_B5336_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5336Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5336_OrgUnit_DirectorId,
                        UserId = user_B5336_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("sayedf@unops.org")) missingUsers.Add("sayedf@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5336");
        }

        // B5337
        if (codeToOrgUnitId.TryGetValue("B5337", out var orgUnit_B5337Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5337_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5337_Region_DirectorId))
            {
                var key_B5337_Region_Director = $"{orgUnit_B5337Id}_{role_B5337_Region_DirectorId}_{user_B5337_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5337_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_Region_DirectorId}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_Region_DirectorId,
                        UserId = user_B5337_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5337_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5337_Hub_DirectorId))
            {
                var key_B5337_Hub_Director = $"{orgUnit_B5337Id}_{role_B5337_Hub_DirectorId}_{user_B5337_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5337_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_Hub_DirectorId}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_Hub_DirectorId,
                        UserId = user_B5337_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5337_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5337_Hub_Deputy_DirectorId))
            {
                var key_B5337_Hub_Deputy_Director = $"{orgUnit_B5337Id}_{role_B5337_Hub_Deputy_DirectorId}_{user_B5337_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5337_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_Hub_Deputy_DirectorId,
                        UserId = user_B5337_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5337_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5337_OrgUnit_DirectorId))
            {
                var key_B5337_OrgUnit_Director = $"{orgUnit_B5337Id}_{role_B5337_OrgUnit_DirectorId}_{user_B5337_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5337_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_OrgUnit_DirectorId,
                        UserId = user_B5337_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: nezhad@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5337_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nezhad@unops.org", out var user_B5337_OrgUnit_Deputy_DirectorId))
            {
                var key_B5337_OrgUnit_Deputy_Director = $"{orgUnit_B5337Id}_{role_B5337_OrgUnit_Deputy_DirectorId}_{user_B5337_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5337_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5337Id} - {user_B5337_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5337Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5337_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5337_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("nezhad@unops.org")) missingUsers.Add("nezhad@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5337");
        }

        // B5338
        if (codeToOrgUnitId.TryGetValue("B5338", out var orgUnit_B5338Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5338_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5338_Region_DirectorId))
            {
                var key_B5338_Region_Director = $"{orgUnit_B5338Id}_{role_B5338_Region_DirectorId}_{user_B5338_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5338_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_Region_DirectorId}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_Region_DirectorId,
                        UserId = user_B5338_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Hub Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5338_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5338_Hub_DirectorId))
            {
                var key_B5338_Hub_Director = $"{orgUnit_B5338Id}_{role_B5338_Hub_DirectorId}_{user_B5338_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5338_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_Hub_DirectorId}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_Hub_DirectorId,
                        UserId = user_B5338_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // Hub Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5338_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5338_Hub_Deputy_DirectorId))
            {
                var key_B5338_Hub_Deputy_Director = $"{orgUnit_B5338Id}_{role_B5338_Hub_Deputy_DirectorId}_{user_B5338_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5338_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_Hub_Deputy_DirectorId,
                        UserId = user_B5338_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
            // OrgUnit Director: nathaliea@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5338_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nathaliea@unops.org", out var user_B5338_OrgUnit_DirectorId))
            {
                var key_B5338_OrgUnit_Director = $"{orgUnit_B5338Id}_{role_B5338_OrgUnit_DirectorId}_{user_B5338_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5338_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_OrgUnit_DirectorId,
                        UserId = user_B5338_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nathaliea@unops.org")) missingUsers.Add("nathaliea@unops.org");
            }
            // OrgUnit Deputy Director: fredericfr@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5338_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("fredericfr@unops.org", out var user_B5338_OrgUnit_Deputy_DirectorId))
            {
                var key_B5338_OrgUnit_Deputy_Director = $"{orgUnit_B5338Id}_{role_B5338_OrgUnit_Deputy_DirectorId}_{user_B5338_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5338_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5338Id} - {user_B5338_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5338Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5338_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5338_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("fredericfr@unops.org")) missingUsers.Add("fredericfr@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5338");
        }

        // B5401
        if (codeToOrgUnitId.TryGetValue("B5401", out var orgUnit_B5401Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5401_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5401_Region_DirectorId))
            {
                var key_B5401_Region_Director = $"{orgUnit_B5401Id}_{role_B5401_Region_DirectorId}_{user_B5401_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5401_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_Region_DirectorId}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_Region_DirectorId,
                        UserId = user_B5401_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5401_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5401_Region_Deputy_DirectorId))
            {
                var key_B5401_Region_Deputy_Director = $"{orgUnit_B5401Id}_{role_B5401_Region_Deputy_DirectorId}_{user_B5401_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5401_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_Region_Deputy_DirectorId,
                        UserId = user_B5401_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5401_Hub_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5401_Hub_DirectorId))
            {
                var key_B5401_Hub_Director = $"{orgUnit_B5401Id}_{role_B5401_Hub_DirectorId}_{user_B5401_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5401_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_Hub_DirectorId}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_Hub_DirectorId,
                        UserId = user_B5401_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // Hub Deputy Director: catherinew@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5401_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("catherinew@unops.org", out var user_B5401_Hub_Deputy_DirectorId))
            {
                var key_B5401_Hub_Deputy_Director = $"{orgUnit_B5401Id}_{role_B5401_Hub_Deputy_DirectorId}_{user_B5401_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5401_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_Hub_Deputy_DirectorId,
                        UserId = user_B5401_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("catherinew@unops.org")) missingUsers.Add("catherinew@unops.org");
            }
            // OrgUnit Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5401_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5401_OrgUnit_DirectorId))
            {
                var key_B5401_OrgUnit_Director = $"{orgUnit_B5401Id}_{role_B5401_OrgUnit_DirectorId}_{user_B5401_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5401_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_OrgUnit_DirectorId,
                        UserId = user_B5401_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // OrgUnit Deputy Director: catherinew@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5401_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("catherinew@unops.org", out var user_B5401_OrgUnit_Deputy_DirectorId))
            {
                var key_B5401_OrgUnit_Deputy_Director = $"{orgUnit_B5401Id}_{role_B5401_OrgUnit_Deputy_DirectorId}_{user_B5401_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5401_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5401Id} - {user_B5401_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5401Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5401_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5401_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("catherinew@unops.org")) missingUsers.Add("catherinew@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5401");
        }

        // B5405
        if (codeToOrgUnitId.TryGetValue("B5405", out var orgUnit_B5405Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5405_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5405_Region_DirectorId))
            {
                var key_B5405_Region_Director = $"{orgUnit_B5405Id}_{role_B5405_Region_DirectorId}_{user_B5405_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5405_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_Region_DirectorId}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_Region_DirectorId,
                        UserId = user_B5405_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5405_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5405_Region_Deputy_DirectorId))
            {
                var key_B5405_Region_Deputy_Director = $"{orgUnit_B5405Id}_{role_B5405_Region_Deputy_DirectorId}_{user_B5405_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5405_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_Region_Deputy_DirectorId,
                        UserId = user_B5405_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: andreaca@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5405_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("andreaca@unops.org", out var user_B5405_OrgUnit_DirectorId))
            {
                var key_B5405_OrgUnit_Director = $"{orgUnit_B5405Id}_{role_B5405_OrgUnit_DirectorId}_{user_B5405_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5405_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5405Id} - {user_B5405_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5405Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5405_OrgUnit_DirectorId,
                        UserId = user_B5405_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("andreaca@unops.org")) missingUsers.Add("andreaca@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5405");
        }

        // B5406
        if (codeToOrgUnitId.TryGetValue("B5406", out var orgUnit_B5406Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5406_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5406_Region_DirectorId))
            {
                var key_B5406_Region_Director = $"{orgUnit_B5406Id}_{role_B5406_Region_DirectorId}_{user_B5406_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5406_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_Region_DirectorId}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_Region_DirectorId,
                        UserId = user_B5406_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5406_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5406_Region_Deputy_DirectorId))
            {
                var key_B5406_Region_Deputy_Director = $"{orgUnit_B5406Id}_{role_B5406_Region_Deputy_DirectorId}_{user_B5406_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5406_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_Region_Deputy_DirectorId,
                        UserId = user_B5406_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: dabagaid@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5406_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("dabagaid@unops.org", out var user_B5406_OrgUnit_DirectorId))
            {
                var key_B5406_OrgUnit_Director = $"{orgUnit_B5406Id}_{role_B5406_OrgUnit_DirectorId}_{user_B5406_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5406_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_OrgUnit_DirectorId,
                        UserId = user_B5406_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("dabagaid@unops.org")) missingUsers.Add("dabagaid@unops.org");
            }
            // OrgUnit Deputy Director: nubarg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5406_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nubarg@unops.org", out var user_B5406_OrgUnit_Deputy_DirectorId))
            {
                var key_B5406_OrgUnit_Deputy_Director = $"{orgUnit_B5406Id}_{role_B5406_OrgUnit_Deputy_DirectorId}_{user_B5406_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5406_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5406Id} - {user_B5406_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5406Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5406_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5406_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("nubarg@unops.org")) missingUsers.Add("nubarg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5406");
        }

        // B5407
        if (codeToOrgUnitId.TryGetValue("B5407", out var orgUnit_B5407Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5407_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5407_Region_DirectorId))
            {
                var key_B5407_Region_Director = $"{orgUnit_B5407Id}_{role_B5407_Region_DirectorId}_{user_B5407_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5407_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_Region_DirectorId}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_Region_DirectorId,
                        UserId = user_B5407_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5407_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5407_Region_Deputy_DirectorId))
            {
                var key_B5407_Region_Deputy_Director = $"{orgUnit_B5407Id}_{role_B5407_Region_Deputy_DirectorId}_{user_B5407_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5407_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_Region_Deputy_DirectorId,
                        UserId = user_B5407_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: espositon@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5407_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("espositon@unops.org", out var user_B5407_OrgUnit_DirectorId))
            {
                var key_B5407_OrgUnit_Director = $"{orgUnit_B5407Id}_{role_B5407_OrgUnit_DirectorId}_{user_B5407_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5407_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_OrgUnit_DirectorId,
                        UserId = user_B5407_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("espositon@unops.org")) missingUsers.Add("espositon@unops.org");
            }
            // OrgUnit Deputy Director: davidme@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5407_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("davidme@unops.org", out var user_B5407_OrgUnit_Deputy_DirectorId))
            {
                var key_B5407_OrgUnit_Deputy_Director = $"{orgUnit_B5407Id}_{role_B5407_OrgUnit_Deputy_DirectorId}_{user_B5407_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5407_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5407Id} - {user_B5407_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5407Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5407_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5407_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("davidme@unops.org")) missingUsers.Add("davidme@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5407");
        }

        // B5408
        if (codeToOrgUnitId.TryGetValue("B5408", out var orgUnit_B5408Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5408_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5408_Region_DirectorId))
            {
                var key_B5408_Region_Director = $"{orgUnit_B5408Id}_{role_B5408_Region_DirectorId}_{user_B5408_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5408_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_Region_DirectorId}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_Region_DirectorId,
                        UserId = user_B5408_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5408_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5408_Region_Deputy_DirectorId))
            {
                var key_B5408_Region_Deputy_Director = $"{orgUnit_B5408Id}_{role_B5408_Region_Deputy_DirectorId}_{user_B5408_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5408_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_Region_Deputy_DirectorId,
                        UserId = user_B5408_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: marialk@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5408_Hub_DirectorId) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5408_Hub_DirectorId))
            {
                var key_B5408_Hub_Director = $"{orgUnit_B5408Id}_{role_B5408_Hub_DirectorId}_{user_B5408_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5408_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_Hub_DirectorId}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_Hub_DirectorId,
                        UserId = user_B5408_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("marialk@unops.org")) missingUsers.Add("marialk@unops.org");
            }
            // OrgUnit Director: marialk@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5408_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5408_OrgUnit_DirectorId))
            {
                var key_B5408_OrgUnit_Director = $"{orgUnit_B5408Id}_{role_B5408_OrgUnit_DirectorId}_{user_B5408_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5408_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5408Id} - {user_B5408_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5408Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5408_OrgUnit_DirectorId,
                        UserId = user_B5408_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
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
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5410_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5410_Region_DirectorId))
            {
                var key_B5410_Region_Director = $"{orgUnit_B5410Id}_{role_B5410_Region_DirectorId}_{user_B5410_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5410_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_Region_DirectorId}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_Region_DirectorId,
                        UserId = user_B5410_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5410_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5410_Region_Deputy_DirectorId))
            {
                var key_B5410_Region_Deputy_Director = $"{orgUnit_B5410Id}_{role_B5410_Region_Deputy_DirectorId}_{user_B5410_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5410_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_Region_Deputy_DirectorId,
                        UserId = user_B5410_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: nickg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5410_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nickg@unops.org", out var user_B5410_Hub_DirectorId))
            {
                var key_B5410_Hub_Director = $"{orgUnit_B5410Id}_{role_B5410_Hub_DirectorId}_{user_B5410_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5410_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_Hub_DirectorId}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_Hub_DirectorId,
                        UserId = user_B5410_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nickg@unops.org")) missingUsers.Add("nickg@unops.org");
            }
            // OrgUnit Director: nickg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5410_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nickg@unops.org", out var user_B5410_OrgUnit_DirectorId))
            {
                var key_B5410_OrgUnit_Director = $"{orgUnit_B5410Id}_{role_B5410_OrgUnit_DirectorId}_{user_B5410_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5410_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5410Id} - {user_B5410_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5410Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5410_OrgUnit_DirectorId,
                        UserId = user_B5410_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nickg@unops.org")) missingUsers.Add("nickg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5410");
        }

        // B5411
        if (codeToOrgUnitId.TryGetValue("B5411", out var orgUnit_B5411Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5411_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5411_Region_DirectorId))
            {
                var key_B5411_Region_Director = $"{orgUnit_B5411Id}_{role_B5411_Region_DirectorId}_{user_B5411_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5411_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_Region_DirectorId}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_Region_DirectorId,
                        UserId = user_B5411_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5411_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5411_Region_Deputy_DirectorId))
            {
                var key_B5411_Region_Deputy_Director = $"{orgUnit_B5411Id}_{role_B5411_Region_Deputy_DirectorId}_{user_B5411_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5411_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_Region_Deputy_DirectorId,
                        UserId = user_B5411_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5411_Hub_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5411_Hub_DirectorId))
            {
                var key_B5411_Hub_Director = $"{orgUnit_B5411Id}_{role_B5411_Hub_DirectorId}_{user_B5411_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5411_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_Hub_DirectorId}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_Hub_DirectorId,
                        UserId = user_B5411_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // Hub Deputy Director: catherinew@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5411_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("catherinew@unops.org", out var user_B5411_Hub_Deputy_DirectorId))
            {
                var key_B5411_Hub_Deputy_Director = $"{orgUnit_B5411Id}_{role_B5411_Hub_Deputy_DirectorId}_{user_B5411_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5411_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_Hub_Deputy_DirectorId,
                        UserId = user_B5411_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("catherinew@unops.org")) missingUsers.Add("catherinew@unops.org");
            }
            // OrgUnit Director: robertoc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5411_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("robertoc@unops.org", out var user_B5411_OrgUnit_DirectorId))
            {
                var key_B5411_OrgUnit_Director = $"{orgUnit_B5411Id}_{role_B5411_OrgUnit_DirectorId}_{user_B5411_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5411_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_OrgUnit_DirectorId,
                        UserId = user_B5411_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("robertoc@unops.org")) missingUsers.Add("robertoc@unops.org");
            }
            // OrgUnit Deputy Director: nathaliet@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5411_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("nathaliet@unops.org", out var user_B5411_OrgUnit_Deputy_DirectorId))
            {
                var key_B5411_OrgUnit_Deputy_Director = $"{orgUnit_B5411Id}_{role_B5411_OrgUnit_Deputy_DirectorId}_{user_B5411_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5411_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5411Id} - {user_B5411_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5411Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5411_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5411_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("nathaliet@unops.org")) missingUsers.Add("nathaliet@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5411");
        }

        // B5412
        if (codeToOrgUnitId.TryGetValue("B5412", out var orgUnit_B5412Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5412_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5412_Region_DirectorId))
            {
                var key_B5412_Region_Director = $"{orgUnit_B5412Id}_{role_B5412_Region_DirectorId}_{user_B5412_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5412_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_Region_DirectorId}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_Region_DirectorId,
                        UserId = user_B5412_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5412_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5412_Region_Deputy_DirectorId))
            {
                var key_B5412_Region_Deputy_Director = $"{orgUnit_B5412Id}_{role_B5412_Region_Deputy_DirectorId}_{user_B5412_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5412_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_Region_Deputy_DirectorId,
                        UserId = user_B5412_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: claudiav@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5412_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("claudiav@unops.org", out var user_B5412_OrgUnit_DirectorId))
            {
                var key_B5412_OrgUnit_Director = $"{orgUnit_B5412Id}_{role_B5412_OrgUnit_DirectorId}_{user_B5412_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5412_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_OrgUnit_DirectorId,
                        UserId = user_B5412_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("claudiav@unops.org")) missingUsers.Add("claudiav@unops.org");
            }
            // OrgUnit Deputy Director: soledadb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5412_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("soledadb@unops.org", out var user_B5412_OrgUnit_Deputy_DirectorId))
            {
                var key_B5412_OrgUnit_Deputy_Director = $"{orgUnit_B5412Id}_{role_B5412_OrgUnit_Deputy_DirectorId}_{user_B5412_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5412_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5412Id} - {user_B5412_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5412Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5412_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5412_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("soledadb@unops.org")) missingUsers.Add("soledadb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5412");
        }

        // B5414
        if (codeToOrgUnitId.TryGetValue("B5414", out var orgUnit_B5414Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5414_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5414_Region_DirectorId))
            {
                var key_B5414_Region_Director = $"{orgUnit_B5414Id}_{role_B5414_Region_DirectorId}_{user_B5414_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5414_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_Region_DirectorId}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_Region_DirectorId,
                        UserId = user_B5414_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5414_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5414_Region_Deputy_DirectorId))
            {
                var key_B5414_Region_Deputy_Director = $"{orgUnit_B5414Id}_{role_B5414_Region_Deputy_DirectorId}_{user_B5414_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5414_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_Region_Deputy_DirectorId,
                        UserId = user_B5414_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5414_Hub_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5414_Hub_DirectorId))
            {
                var key_B5414_Hub_Director = $"{orgUnit_B5414Id}_{role_B5414_Hub_DirectorId}_{user_B5414_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5414_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_Hub_DirectorId}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_Hub_DirectorId,
                        UserId = user_B5414_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // Hub Deputy Director: catherinew@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5414_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("catherinew@unops.org", out var user_B5414_Hub_Deputy_DirectorId))
            {
                var key_B5414_Hub_Deputy_Director = $"{orgUnit_B5414Id}_{role_B5414_Hub_Deputy_DirectorId}_{user_B5414_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5414_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_Hub_Deputy_DirectorId,
                        UserId = user_B5414_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("catherinew@unops.org")) missingUsers.Add("catherinew@unops.org");
            }
            // OrgUnit Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5414_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5414_OrgUnit_DirectorId))
            {
                var key_B5414_OrgUnit_Director = $"{orgUnit_B5414Id}_{role_B5414_OrgUnit_DirectorId}_{user_B5414_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5414_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_OrgUnit_DirectorId,
                        UserId = user_B5414_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // OrgUnit Deputy Director: ignaciol@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5414_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("ignaciol@unops.org", out var user_B5414_OrgUnit_Deputy_DirectorId))
            {
                var key_B5414_OrgUnit_Deputy_Director = $"{orgUnit_B5414Id}_{role_B5414_OrgUnit_Deputy_DirectorId}_{user_B5414_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5414_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5414Id} - {user_B5414_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5414Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5414_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5414_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("ignaciol@unops.org")) missingUsers.Add("ignaciol@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5414");
        }

        // B5416
        if (codeToOrgUnitId.TryGetValue("B5416", out var orgUnit_B5416Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5416_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5416_Region_DirectorId))
            {
                var key_B5416_Region_Director = $"{orgUnit_B5416Id}_{role_B5416_Region_DirectorId}_{user_B5416_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5416_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_Region_DirectorId}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_Region_DirectorId,
                        UserId = user_B5416_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5416_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5416_Region_Deputy_DirectorId))
            {
                var key_B5416_Region_Deputy_Director = $"{orgUnit_B5416Id}_{role_B5416_Region_Deputy_DirectorId}_{user_B5416_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5416_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_Region_Deputy_DirectorId,
                        UserId = user_B5416_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: marialk@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5416_Hub_DirectorId) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5416_Hub_DirectorId))
            {
                var key_B5416_Hub_Director = $"{orgUnit_B5416Id}_{role_B5416_Hub_DirectorId}_{user_B5416_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5416_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_Hub_DirectorId}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_Hub_DirectorId,
                        UserId = user_B5416_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("marialk@unops.org")) missingUsers.Add("marialk@unops.org");
            }
            // OrgUnit Director: davidme@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5416_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("davidme@unops.org", out var user_B5416_OrgUnit_DirectorId))
            {
                var key_B5416_OrgUnit_Director = $"{orgUnit_B5416Id}_{role_B5416_OrgUnit_DirectorId}_{user_B5416_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5416_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_OrgUnit_DirectorId,
                        UserId = user_B5416_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("davidme@unops.org")) missingUsers.Add("davidme@unops.org");
            }
            // OrgUnit Deputy Director: williamsg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5416_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("williamsg@unops.org", out var user_B5416_OrgUnit_Deputy_DirectorId))
            {
                var key_B5416_OrgUnit_Deputy_Director = $"{orgUnit_B5416Id}_{role_B5416_OrgUnit_Deputy_DirectorId}_{user_B5416_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5416_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5416Id} - {user_B5416_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5416Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5416_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5416_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("williamsg@unops.org")) missingUsers.Add("williamsg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5416");
        }

        // B5417
        if (codeToOrgUnitId.TryGetValue("B5417", out var orgUnit_B5417Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5417_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5417_Region_DirectorId))
            {
                var key_B5417_Region_Director = $"{orgUnit_B5417Id}_{role_B5417_Region_DirectorId}_{user_B5417_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5417_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_Region_DirectorId}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_Region_DirectorId,
                        UserId = user_B5417_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5417_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5417_Region_Deputy_DirectorId))
            {
                var key_B5417_Region_Deputy_Director = $"{orgUnit_B5417Id}_{role_B5417_Region_Deputy_DirectorId}_{user_B5417_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5417_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_Region_Deputy_DirectorId,
                        UserId = user_B5417_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: claudiav@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5417_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("claudiav@unops.org", out var user_B5417_OrgUnit_DirectorId))
            {
                var key_B5417_OrgUnit_Director = $"{orgUnit_B5417Id}_{role_B5417_OrgUnit_DirectorId}_{user_B5417_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5417_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_OrgUnit_DirectorId,
                        UserId = user_B5417_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("claudiav@unops.org")) missingUsers.Add("claudiav@unops.org");
            }
            // OrgUnit Deputy Director: soledadb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5417_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("soledadb@unops.org", out var user_B5417_OrgUnit_Deputy_DirectorId))
            {
                var key_B5417_OrgUnit_Deputy_Director = $"{orgUnit_B5417Id}_{role_B5417_OrgUnit_Deputy_DirectorId}_{user_B5417_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5417_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5417Id} - {user_B5417_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5417Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5417_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5417_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("soledadb@unops.org")) missingUsers.Add("soledadb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5417");
        }

        // B5421
        if (codeToOrgUnitId.TryGetValue("B5421", out var orgUnit_B5421Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5421_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5421_Region_DirectorId))
            {
                var key_B5421_Region_Director = $"{orgUnit_B5421Id}_{role_B5421_Region_DirectorId}_{user_B5421_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5421_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_Region_DirectorId}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_Region_DirectorId,
                        UserId = user_B5421_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5421_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5421_Region_Deputy_DirectorId))
            {
                var key_B5421_Region_Deputy_Director = $"{orgUnit_B5421Id}_{role_B5421_Region_Deputy_DirectorId}_{user_B5421_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5421_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_Region_Deputy_DirectorId,
                        UserId = user_B5421_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5421_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5421_OrgUnit_DirectorId))
            {
                var key_B5421_OrgUnit_Director = $"{orgUnit_B5421Id}_{role_B5421_OrgUnit_DirectorId}_{user_B5421_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5421_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_OrgUnit_DirectorId,
                        UserId = user_B5421_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Deputy Director: jorgeb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5421_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("jorgeb@unops.org", out var user_B5421_OrgUnit_Deputy_DirectorId))
            {
                var key_B5421_OrgUnit_Deputy_Director = $"{orgUnit_B5421Id}_{role_B5421_OrgUnit_Deputy_DirectorId}_{user_B5421_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5421_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5421Id} - {user_B5421_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5421Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5421_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5421_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("jorgeb@unops.org")) missingUsers.Add("jorgeb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5421");
        }

        // B5422
        if (codeToOrgUnitId.TryGetValue("B5422", out var orgUnit_B5422Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5422_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5422_Region_DirectorId))
            {
                var key_B5422_Region_Director = $"{orgUnit_B5422Id}_{role_B5422_Region_DirectorId}_{user_B5422_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5422_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_Region_DirectorId}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_Region_DirectorId,
                        UserId = user_B5422_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5422_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5422_Region_Deputy_DirectorId))
            {
                var key_B5422_Region_Deputy_Director = $"{orgUnit_B5422Id}_{role_B5422_Region_Deputy_DirectorId}_{user_B5422_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5422_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_Region_Deputy_DirectorId,
                        UserId = user_B5422_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5422_Hub_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5422_Hub_DirectorId))
            {
                var key_B5422_Hub_Director = $"{orgUnit_B5422Id}_{role_B5422_Hub_DirectorId}_{user_B5422_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5422_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_Hub_DirectorId}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_Hub_DirectorId,
                        UserId = user_B5422_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // Hub Deputy Director: catherinew@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5422_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("catherinew@unops.org", out var user_B5422_Hub_Deputy_DirectorId))
            {
                var key_B5422_Hub_Deputy_Director = $"{orgUnit_B5422Id}_{role_B5422_Hub_Deputy_DirectorId}_{user_B5422_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5422_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_Hub_Deputy_DirectorId,
                        UserId = user_B5422_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("catherinew@unops.org")) missingUsers.Add("catherinew@unops.org");
            }
            // OrgUnit Director: fernandoc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5422_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("fernandoc@unops.org", out var user_B5422_OrgUnit_DirectorId))
            {
                var key_B5422_OrgUnit_Director = $"{orgUnit_B5422Id}_{role_B5422_OrgUnit_DirectorId}_{user_B5422_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5422_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_OrgUnit_DirectorId,
                        UserId = user_B5422_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("fernandoc@unops.org")) missingUsers.Add("fernandoc@unops.org");
            }
            // OrgUnit Deputy Director: catherinew@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5422_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("catherinew@unops.org", out var user_B5422_OrgUnit_Deputy_DirectorId))
            {
                var key_B5422_OrgUnit_Deputy_Director = $"{orgUnit_B5422Id}_{role_B5422_OrgUnit_Deputy_DirectorId}_{user_B5422_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5422_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5422Id} - {user_B5422_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5422Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5422_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5422_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("catherinew@unops.org")) missingUsers.Add("catherinew@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5422");
        }

        // B5423
        if (codeToOrgUnitId.TryGetValue("B5423", out var orgUnit_B5423Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5423_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5423_Region_DirectorId))
            {
                var key_B5423_Region_Director = $"{orgUnit_B5423Id}_{role_B5423_Region_DirectorId}_{user_B5423_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5423_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_Region_DirectorId}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_Region_DirectorId,
                        UserId = user_B5423_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5423_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5423_Region_Deputy_DirectorId))
            {
                var key_B5423_Region_Deputy_Director = $"{orgUnit_B5423Id}_{role_B5423_Region_Deputy_DirectorId}_{user_B5423_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5423_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_Region_Deputy_DirectorId,
                        UserId = user_B5423_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: marialk@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5423_Hub_DirectorId) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5423_Hub_DirectorId))
            {
                var key_B5423_Hub_Director = $"{orgUnit_B5423Id}_{role_B5423_Hub_DirectorId}_{user_B5423_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5423_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_Hub_DirectorId}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_Hub_DirectorId,
                        UserId = user_B5423_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("marialk@unops.org")) missingUsers.Add("marialk@unops.org");
            }
            // OrgUnit Director: marialk@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5423_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("marialk@unops.org", out var user_B5423_OrgUnit_DirectorId))
            {
                var key_B5423_OrgUnit_Director = $"{orgUnit_B5423Id}_{role_B5423_OrgUnit_DirectorId}_{user_B5423_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5423_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5423Id} - {user_B5423_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5423Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5423_OrgUnit_DirectorId,
                        UserId = user_B5423_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("marialk@unops.org")) missingUsers.Add("marialk@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5423");
        }

        // B5424
        if (codeToOrgUnitId.TryGetValue("B5424", out var orgUnit_B5424Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5424_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5424_Region_DirectorId))
            {
                var key_B5424_Region_Director = $"{orgUnit_B5424Id}_{role_B5424_Region_DirectorId}_{user_B5424_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5424_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_Region_DirectorId}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_Region_DirectorId,
                        UserId = user_B5424_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5424_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5424_Region_Deputy_DirectorId))
            {
                var key_B5424_Region_Deputy_Director = $"{orgUnit_B5424Id}_{role_B5424_Region_Deputy_DirectorId}_{user_B5424_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5424_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_Region_Deputy_DirectorId,
                        UserId = user_B5424_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // Hub Director: nickg@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5424_Hub_DirectorId) &&
                emailToUserId.TryGetValue("nickg@unops.org", out var user_B5424_Hub_DirectorId))
            {
                var key_B5424_Hub_Director = $"{orgUnit_B5424Id}_{role_B5424_Hub_DirectorId}_{user_B5424_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5424_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_Hub_DirectorId}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_Hub_DirectorId,
                        UserId = user_B5424_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("nickg@unops.org")) missingUsers.Add("nickg@unops.org");
            }
            // OrgUnit Director: nickg@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5424_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("nickg@unops.org", out var user_B5424_OrgUnit_DirectorId))
            {
                var key_B5424_OrgUnit_Director = $"{orgUnit_B5424Id}_{role_B5424_OrgUnit_DirectorId}_{user_B5424_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5424_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5424Id} - {user_B5424_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5424Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5424_OrgUnit_DirectorId,
                        UserId = user_B5424_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("nickg@unops.org")) missingUsers.Add("nickg@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5424");
        }

        // B5425
        if (codeToOrgUnitId.TryGetValue("B5425", out var orgUnit_B5425Id))
        {
            // Region Director: dalilag@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5425_Region_DirectorId) &&
                emailToUserId.TryGetValue("dalilag@unops.org", out var user_B5425_Region_DirectorId))
            {
                var key_B5425_Region_Director = $"{orgUnit_B5425Id}_{role_B5425_Region_DirectorId}_{user_B5425_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5425_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_Region_DirectorId}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_Region_DirectorId,
                        UserId = user_B5425_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("dalilag@unops.org")) missingUsers.Add("dalilag@unops.org");
            }
            // Region Deputy Director: giuseppem@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5425_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("giuseppem@unops.org", out var user_B5425_Region_Deputy_DirectorId))
            {
                var key_B5425_Region_Deputy_Director = $"{orgUnit_B5425Id}_{role_B5425_Region_Deputy_DirectorId}_{user_B5425_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5425_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_Region_Deputy_DirectorId,
                        UserId = user_B5425_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("giuseppem@unops.org")) missingUsers.Add("giuseppem@unops.org");
            }
            // OrgUnit Director: claudiav@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5425_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("claudiav@unops.org", out var user_B5425_OrgUnit_DirectorId))
            {
                var key_B5425_OrgUnit_Director = $"{orgUnit_B5425Id}_{role_B5425_OrgUnit_DirectorId}_{user_B5425_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5425_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_OrgUnit_DirectorId,
                        UserId = user_B5425_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("claudiav@unops.org")) missingUsers.Add("claudiav@unops.org");
            }
            // OrgUnit Deputy Director: soledadb@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5425_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("soledadb@unops.org", out var user_B5425_OrgUnit_Deputy_DirectorId))
            {
                var key_B5425_OrgUnit_Deputy_Director = $"{orgUnit_B5425Id}_{role_B5425_OrgUnit_Deputy_DirectorId}_{user_B5425_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5425_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5425Id} - {user_B5425_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5425Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5425_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5425_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("soledadb@unops.org")) missingUsers.Add("soledadb@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5425");
        }

        // B5502
        if (codeToOrgUnitId.TryGetValue("B5502", out var orgUnit_B5502Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5502_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5502_Region_DirectorId))
            {
                var key_B5502_Region_Director = $"{orgUnit_B5502Id}_{role_B5502_Region_DirectorId}_{user_B5502_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5502_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_Region_DirectorId}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_Region_DirectorId,
                        UserId = user_B5502_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5502_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5502_Region_Deputy_DirectorId))
            {
                var key_B5502_Region_Deputy_Director = $"{orgUnit_B5502Id}_{role_B5502_Region_Deputy_DirectorId}_{user_B5502_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5502_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_Region_Deputy_DirectorId,
                        UserId = user_B5502_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5502_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5502_Hub_DirectorId))
            {
                var key_B5502_Hub_Director = $"{orgUnit_B5502Id}_{role_B5502_Hub_DirectorId}_{user_B5502_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5502_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_Hub_DirectorId}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_Hub_DirectorId,
                        UserId = user_B5502_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5502_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5502_Hub_Deputy_DirectorId))
            {
                var key_B5502_Hub_Deputy_Director = $"{orgUnit_B5502Id}_{role_B5502_Hub_Deputy_DirectorId}_{user_B5502_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5502_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_Hub_Deputy_DirectorId,
                        UserId = user_B5502_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5502_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5502_OrgUnit_DirectorId))
            {
                var key_B5502_OrgUnit_Director = $"{orgUnit_B5502Id}_{role_B5502_OrgUnit_DirectorId}_{user_B5502_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5502_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_OrgUnit_DirectorId,
                        UserId = user_B5502_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5502_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5502_OrgUnit_Deputy_DirectorId))
            {
                var key_B5502_OrgUnit_Deputy_Director = $"{orgUnit_B5502Id}_{role_B5502_OrgUnit_Deputy_DirectorId}_{user_B5502_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5502_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5502Id} - {user_B5502_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5502Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5502_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5502_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5502");
        }

        // B5503
        if (codeToOrgUnitId.TryGetValue("B5503", out var orgUnit_B5503Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5503_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5503_Region_DirectorId))
            {
                var key_B5503_Region_Director = $"{orgUnit_B5503Id}_{role_B5503_Region_DirectorId}_{user_B5503_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5503_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_Region_DirectorId}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_Region_DirectorId,
                        UserId = user_B5503_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5503_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5503_Region_Deputy_DirectorId))
            {
                var key_B5503_Region_Deputy_Director = $"{orgUnit_B5503Id}_{role_B5503_Region_Deputy_DirectorId}_{user_B5503_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5503_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_Region_Deputy_DirectorId,
                        UserId = user_B5503_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5503_Hub_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5503_Hub_DirectorId))
            {
                var key_B5503_Hub_Director = $"{orgUnit_B5503Id}_{role_B5503_Hub_DirectorId}_{user_B5503_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5503_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_Hub_DirectorId}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_Hub_DirectorId,
                        UserId = user_B5503_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5503_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5503_OrgUnit_DirectorId))
            {
                var key_B5503_OrgUnit_Director = $"{orgUnit_B5503Id}_{role_B5503_OrgUnit_DirectorId}_{user_B5503_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5503_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5503Id} - {user_B5503_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5503Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5503_OrgUnit_DirectorId,
                        UserId = user_B5503_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5503");
        }

        // B5505
        if (codeToOrgUnitId.TryGetValue("B5505", out var orgUnit_B5505Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5505_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5505_Region_DirectorId))
            {
                var key_B5505_Region_Director = $"{orgUnit_B5505Id}_{role_B5505_Region_DirectorId}_{user_B5505_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5505_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_Region_DirectorId}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_Region_DirectorId,
                        UserId = user_B5505_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5505_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5505_Region_Deputy_DirectorId))
            {
                var key_B5505_Region_Deputy_Director = $"{orgUnit_B5505Id}_{role_B5505_Region_Deputy_DirectorId}_{user_B5505_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5505_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_Region_Deputy_DirectorId,
                        UserId = user_B5505_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5505_Hub_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5505_Hub_DirectorId))
            {
                var key_B5505_Hub_Director = $"{orgUnit_B5505Id}_{role_B5505_Hub_DirectorId}_{user_B5505_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5505_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_Hub_DirectorId}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_Hub_DirectorId,
                        UserId = user_B5505_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5505_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5505_OrgUnit_DirectorId))
            {
                var key_B5505_OrgUnit_Director = $"{orgUnit_B5505Id}_{role_B5505_OrgUnit_DirectorId}_{user_B5505_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5505_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_OrgUnit_DirectorId,
                        UserId = user_B5505_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Deputy Director: pathmalathap@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5505_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("pathmalathap@unops.org", out var user_B5505_OrgUnit_Deputy_DirectorId))
            {
                var key_B5505_OrgUnit_Deputy_Director = $"{orgUnit_B5505Id}_{role_B5505_OrgUnit_Deputy_DirectorId}_{user_B5505_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5505_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5505Id} - {user_B5505_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5505Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5505_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5505_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("pathmalathap@unops.org")) missingUsers.Add("pathmalathap@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5505");
        }

        // B5506
        if (codeToOrgUnitId.TryGetValue("B5506", out var orgUnit_B5506Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5506_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5506_Region_DirectorId))
            {
                var key_B5506_Region_Director = $"{orgUnit_B5506Id}_{role_B5506_Region_DirectorId}_{user_B5506_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5506_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_Region_DirectorId}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_Region_DirectorId,
                        UserId = user_B5506_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5506_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5506_Region_Deputy_DirectorId))
            {
                var key_B5506_Region_Deputy_Director = $"{orgUnit_B5506Id}_{role_B5506_Region_Deputy_DirectorId}_{user_B5506_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5506_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_Region_Deputy_DirectorId,
                        UserId = user_B5506_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // OrgUnit Director: sarane@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5506_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("sarane@unops.org", out var user_B5506_OrgUnit_DirectorId))
            {
                var key_B5506_OrgUnit_Director = $"{orgUnit_B5506Id}_{role_B5506_OrgUnit_DirectorId}_{user_B5506_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5506_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_OrgUnit_DirectorId,
                        UserId = user_B5506_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("sarane@unops.org")) missingUsers.Add("sarane@unops.org");
            }
            // OrgUnit Deputy Director: akikok@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5506_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("akikok@unops.org", out var user_B5506_OrgUnit_Deputy_DirectorId))
            {
                var key_B5506_OrgUnit_Deputy_Director = $"{orgUnit_B5506Id}_{role_B5506_OrgUnit_Deputy_DirectorId}_{user_B5506_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5506_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5506Id} - {user_B5506_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5506Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5506_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5506_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("akikok@unops.org")) missingUsers.Add("akikok@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5506");
        }

        // B5507
        if (codeToOrgUnitId.TryGetValue("B5507", out var orgUnit_B5507Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5507_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5507_Region_DirectorId))
            {
                var key_B5507_Region_Director = $"{orgUnit_B5507Id}_{role_B5507_Region_DirectorId}_{user_B5507_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5507_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_Region_DirectorId}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_Region_DirectorId,
                        UserId = user_B5507_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5507_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5507_Region_Deputy_DirectorId))
            {
                var key_B5507_Region_Deputy_Director = $"{orgUnit_B5507Id}_{role_B5507_Region_Deputy_DirectorId}_{user_B5507_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5507_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_Region_Deputy_DirectorId,
                        UserId = user_B5507_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5507_Hub_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5507_Hub_DirectorId))
            {
                var key_B5507_Hub_Director = $"{orgUnit_B5507Id}_{role_B5507_Hub_DirectorId}_{user_B5507_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5507_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_Hub_DirectorId}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_Hub_DirectorId,
                        UserId = user_B5507_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Director: jennifera@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5507_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("jennifera@unops.org", out var user_B5507_OrgUnit_DirectorId))
            {
                var key_B5507_OrgUnit_Director = $"{orgUnit_B5507Id}_{role_B5507_OrgUnit_DirectorId}_{user_B5507_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5507_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_OrgUnit_DirectorId,
                        UserId = user_B5507_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("jennifera@unops.org")) missingUsers.Add("jennifera@unops.org");
            }
            // OrgUnit Deputy Director: naumana@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5507_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("naumana@unops.org", out var user_B5507_OrgUnit_Deputy_DirectorId))
            {
                var key_B5507_OrgUnit_Deputy_Director = $"{orgUnit_B5507Id}_{role_B5507_OrgUnit_Deputy_DirectorId}_{user_B5507_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5507_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5507Id} - {user_B5507_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5507Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5507_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5507_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("naumana@unops.org")) missingUsers.Add("naumana@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5507");
        }

        // B5510
        if (codeToOrgUnitId.TryGetValue("B5510", out var orgUnit_B5510Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5510_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5510_Region_DirectorId))
            {
                var key_B5510_Region_Director = $"{orgUnit_B5510Id}_{role_B5510_Region_DirectorId}_{user_B5510_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5510_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_Region_DirectorId}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_Region_DirectorId,
                        UserId = user_B5510_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5510_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5510_Region_Deputy_DirectorId))
            {
                var key_B5510_Region_Deputy_Director = $"{orgUnit_B5510Id}_{role_B5510_Region_Deputy_DirectorId}_{user_B5510_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5510_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_Region_Deputy_DirectorId,
                        UserId = user_B5510_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5510_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5510_Hub_DirectorId))
            {
                var key_B5510_Hub_Director = $"{orgUnit_B5510Id}_{role_B5510_Hub_DirectorId}_{user_B5510_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5510_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_Hub_DirectorId}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_Hub_DirectorId,
                        UserId = user_B5510_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5510_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5510_Hub_Deputy_DirectorId))
            {
                var key_B5510_Hub_Deputy_Director = $"{orgUnit_B5510Id}_{role_B5510_Hub_Deputy_DirectorId}_{user_B5510_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5510_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_Hub_Deputy_DirectorId,
                        UserId = user_B5510_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5510_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5510_OrgUnit_DirectorId))
            {
                var key_B5510_OrgUnit_Director = $"{orgUnit_B5510Id}_{role_B5510_OrgUnit_DirectorId}_{user_B5510_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5510_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_OrgUnit_DirectorId,
                        UserId = user_B5510_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5510_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5510_OrgUnit_Deputy_DirectorId))
            {
                var key_B5510_OrgUnit_Deputy_Director = $"{orgUnit_B5510Id}_{role_B5510_OrgUnit_Deputy_DirectorId}_{user_B5510_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5510_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5510Id} - {user_B5510_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5510Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5510_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5510_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5510");
        }

        // B5511
        if (codeToOrgUnitId.TryGetValue("B5511", out var orgUnit_B5511Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5511_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5511_Region_DirectorId))
            {
                var key_B5511_Region_Director = $"{orgUnit_B5511Id}_{role_B5511_Region_DirectorId}_{user_B5511_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5511_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_Region_DirectorId}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_Region_DirectorId,
                        UserId = user_B5511_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5511_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5511_Region_Deputy_DirectorId))
            {
                var key_B5511_Region_Deputy_Director = $"{orgUnit_B5511Id}_{role_B5511_Region_Deputy_DirectorId}_{user_B5511_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5511_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_Region_Deputy_DirectorId,
                        UserId = user_B5511_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5511_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5511_Hub_DirectorId))
            {
                var key_B5511_Hub_Director = $"{orgUnit_B5511Id}_{role_B5511_Hub_DirectorId}_{user_B5511_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5511_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_Hub_DirectorId}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_Hub_DirectorId,
                        UserId = user_B5511_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5511_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5511_Hub_Deputy_DirectorId))
            {
                var key_B5511_Hub_Deputy_Director = $"{orgUnit_B5511Id}_{role_B5511_Hub_Deputy_DirectorId}_{user_B5511_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5511_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_Hub_Deputy_DirectorId,
                        UserId = user_B5511_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5511_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5511_OrgUnit_DirectorId))
            {
                var key_B5511_OrgUnit_Director = $"{orgUnit_B5511Id}_{role_B5511_OrgUnit_DirectorId}_{user_B5511_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5511_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_OrgUnit_DirectorId,
                        UserId = user_B5511_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5511_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5511_OrgUnit_Deputy_DirectorId))
            {
                var key_B5511_OrgUnit_Deputy_Director = $"{orgUnit_B5511Id}_{role_B5511_OrgUnit_Deputy_DirectorId}_{user_B5511_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5511_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5511Id} - {user_B5511_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5511Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5511_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5511_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5511");
        }

        // B5512
        if (codeToOrgUnitId.TryGetValue("B5512", out var orgUnit_B5512Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5512_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5512_Region_DirectorId))
            {
                var key_B5512_Region_Director = $"{orgUnit_B5512Id}_{role_B5512_Region_DirectorId}_{user_B5512_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5512_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_Region_DirectorId}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_Region_DirectorId,
                        UserId = user_B5512_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5512_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5512_Region_Deputy_DirectorId))
            {
                var key_B5512_Region_Deputy_Director = $"{orgUnit_B5512Id}_{role_B5512_Region_Deputy_DirectorId}_{user_B5512_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5512_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_Region_Deputy_DirectorId,
                        UserId = user_B5512_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5512_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5512_Hub_DirectorId))
            {
                var key_B5512_Hub_Director = $"{orgUnit_B5512Id}_{role_B5512_Hub_DirectorId}_{user_B5512_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5512_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_Hub_DirectorId}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_Hub_DirectorId,
                        UserId = user_B5512_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5512_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5512_Hub_Deputy_DirectorId))
            {
                var key_B5512_Hub_Deputy_Director = $"{orgUnit_B5512Id}_{role_B5512_Hub_Deputy_DirectorId}_{user_B5512_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5512_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_Hub_Deputy_DirectorId,
                        UserId = user_B5512_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5512_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5512_OrgUnit_DirectorId))
            {
                var key_B5512_OrgUnit_Director = $"{orgUnit_B5512Id}_{role_B5512_OrgUnit_DirectorId}_{user_B5512_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5512_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_OrgUnit_DirectorId,
                        UserId = user_B5512_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5512_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5512_OrgUnit_Deputy_DirectorId))
            {
                var key_B5512_OrgUnit_Deputy_Director = $"{orgUnit_B5512Id}_{role_B5512_OrgUnit_Deputy_DirectorId}_{user_B5512_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5512_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5512Id} - {user_B5512_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5512Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5512_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5512_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5512");
        }

        // B5514
        if (codeToOrgUnitId.TryGetValue("B5514", out var orgUnit_B5514Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5514_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5514_Region_DirectorId))
            {
                var key_B5514_Region_Director = $"{orgUnit_B5514Id}_{role_B5514_Region_DirectorId}_{user_B5514_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5514_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_Region_DirectorId}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_Region_DirectorId,
                        UserId = user_B5514_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5514_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5514_Region_Deputy_DirectorId))
            {
                var key_B5514_Region_Deputy_Director = $"{orgUnit_B5514Id}_{role_B5514_Region_Deputy_DirectorId}_{user_B5514_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5514_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_Region_Deputy_DirectorId,
                        UserId = user_B5514_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5514_Hub_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5514_Hub_DirectorId))
            {
                var key_B5514_Hub_Director = $"{orgUnit_B5514Id}_{role_B5514_Hub_DirectorId}_{user_B5514_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5514_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_Hub_DirectorId}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_Hub_DirectorId,
                        UserId = user_B5514_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5514_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5514_OrgUnit_DirectorId))
            {
                var key_B5514_OrgUnit_Director = $"{orgUnit_B5514Id}_{role_B5514_OrgUnit_DirectorId}_{user_B5514_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5514_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5514Id} - {user_B5514_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5514Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5514_OrgUnit_DirectorId,
                        UserId = user_B5514_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5514");
        }

        // B5516
        if (codeToOrgUnitId.TryGetValue("B5516", out var orgUnit_B5516Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5516_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5516_Region_DirectorId))
            {
                var key_B5516_Region_Director = $"{orgUnit_B5516Id}_{role_B5516_Region_DirectorId}_{user_B5516_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5516_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_Region_DirectorId}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_Region_DirectorId,
                        UserId = user_B5516_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5516_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5516_Region_Deputy_DirectorId))
            {
                var key_B5516_Region_Deputy_Director = $"{orgUnit_B5516Id}_{role_B5516_Region_Deputy_DirectorId}_{user_B5516_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5516_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_Region_Deputy_DirectorId,
                        UserId = user_B5516_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5516_Hub_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5516_Hub_DirectorId))
            {
                var key_B5516_Hub_Director = $"{orgUnit_B5516Id}_{role_B5516_Hub_DirectorId}_{user_B5516_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5516_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_Hub_DirectorId}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_Hub_DirectorId,
                        UserId = user_B5516_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Director: karkik@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5516_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("karkik@unops.org", out var user_B5516_OrgUnit_DirectorId))
            {
                var key_B5516_OrgUnit_Director = $"{orgUnit_B5516Id}_{role_B5516_OrgUnit_DirectorId}_{user_B5516_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5516_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_OrgUnit_DirectorId,
                        UserId = user_B5516_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("karkik@unops.org")) missingUsers.Add("karkik@unops.org");
            }
            // OrgUnit Deputy Director: krishnak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5516_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("krishnak@unops.org", out var user_B5516_OrgUnit_Deputy_DirectorId))
            {
                var key_B5516_OrgUnit_Deputy_Director = $"{orgUnit_B5516Id}_{role_B5516_OrgUnit_Deputy_DirectorId}_{user_B5516_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5516_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5516Id} - {user_B5516_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5516Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5516_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5516_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("krishnak@unops.org")) missingUsers.Add("krishnak@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5516");
        }

        // B5517
        if (codeToOrgUnitId.TryGetValue("B5517", out var orgUnit_B5517Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5517_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5517_Region_DirectorId))
            {
                var key_B5517_Region_Director = $"{orgUnit_B5517Id}_{role_B5517_Region_DirectorId}_{user_B5517_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5517_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_Region_DirectorId}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_Region_DirectorId,
                        UserId = user_B5517_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5517_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5517_Region_Deputy_DirectorId))
            {
                var key_B5517_Region_Deputy_Director = $"{orgUnit_B5517Id}_{role_B5517_Region_Deputy_DirectorId}_{user_B5517_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5517_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_Region_Deputy_DirectorId,
                        UserId = user_B5517_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5517_Hub_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5517_Hub_DirectorId))
            {
                var key_B5517_Hub_Director = $"{orgUnit_B5517Id}_{role_B5517_Hub_DirectorId}_{user_B5517_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5517_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_Hub_DirectorId}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_Hub_DirectorId,
                        UserId = user_B5517_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
            // OrgUnit Director: charlesc@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5517_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("charlesc@unops.org", out var user_B5517_OrgUnit_DirectorId))
            {
                var key_B5517_OrgUnit_Director = $"{orgUnit_B5517Id}_{role_B5517_OrgUnit_DirectorId}_{user_B5517_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5517_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5517Id} - {user_B5517_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5517Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5517_OrgUnit_DirectorId,
                        UserId = user_B5517_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("charlesc@unops.org")) missingUsers.Add("charlesc@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5517");
        }

        // B5518
        if (codeToOrgUnitId.TryGetValue("B5518", out var orgUnit_B5518Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5518_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5518_Region_DirectorId))
            {
                var key_B5518_Region_Director = $"{orgUnit_B5518Id}_{role_B5518_Region_DirectorId}_{user_B5518_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5518_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_Region_DirectorId}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_Region_DirectorId,
                        UserId = user_B5518_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5518_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5518_Region_Deputy_DirectorId))
            {
                var key_B5518_Region_Deputy_Director = $"{orgUnit_B5518Id}_{role_B5518_Region_Deputy_DirectorId}_{user_B5518_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5518_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_Region_Deputy_DirectorId,
                        UserId = user_B5518_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // OrgUnit Director: attilam@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5518_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("attilam@unops.org", out var user_B5518_OrgUnit_DirectorId))
            {
                var key_B5518_OrgUnit_Director = $"{orgUnit_B5518Id}_{role_B5518_OrgUnit_DirectorId}_{user_B5518_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5518_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_OrgUnit_DirectorId,
                        UserId = user_B5518_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("attilam@unops.org")) missingUsers.Add("attilam@unops.org");
            }
            // OrgUnit Deputy Director: frederics@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5518_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("frederics@unops.org", out var user_B5518_OrgUnit_Deputy_DirectorId))
            {
                var key_B5518_OrgUnit_Deputy_Director = $"{orgUnit_B5518Id}_{role_B5518_OrgUnit_Deputy_DirectorId}_{user_B5518_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5518_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5518Id} - {user_B5518_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5518Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5518_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5518_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("frederics@unops.org")) missingUsers.Add("frederics@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5518");
        }

        // B5519
        if (codeToOrgUnitId.TryGetValue("B5519", out var orgUnit_B5519Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5519_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5519_Region_DirectorId))
            {
                var key_B5519_Region_Director = $"{orgUnit_B5519Id}_{role_B5519_Region_DirectorId}_{user_B5519_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5519_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_Region_DirectorId}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_Region_DirectorId,
                        UserId = user_B5519_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5519_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5519_Region_Deputy_DirectorId))
            {
                var key_B5519_Region_Deputy_Director = $"{orgUnit_B5519Id}_{role_B5519_Region_Deputy_DirectorId}_{user_B5519_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5519_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_Region_Deputy_DirectorId,
                        UserId = user_B5519_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5519_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5519_Hub_DirectorId))
            {
                var key_B5519_Hub_Director = $"{orgUnit_B5519Id}_{role_B5519_Hub_DirectorId}_{user_B5519_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5519_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_Hub_DirectorId}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_Hub_DirectorId,
                        UserId = user_B5519_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5519_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5519_Hub_Deputy_DirectorId))
            {
                var key_B5519_Hub_Deputy_Director = $"{orgUnit_B5519Id}_{role_B5519_Hub_Deputy_DirectorId}_{user_B5519_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5519_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_Hub_Deputy_DirectorId,
                        UserId = user_B5519_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5519_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5519_OrgUnit_DirectorId))
            {
                var key_B5519_OrgUnit_Director = $"{orgUnit_B5519Id}_{role_B5519_OrgUnit_DirectorId}_{user_B5519_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5519_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_OrgUnit_DirectorId,
                        UserId = user_B5519_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5519_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5519_OrgUnit_Deputy_DirectorId))
            {
                var key_B5519_OrgUnit_Deputy_Director = $"{orgUnit_B5519Id}_{role_B5519_OrgUnit_Deputy_DirectorId}_{user_B5519_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5519_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5519Id} - {user_B5519_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5519Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5519_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5519_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5519");
        }

        // B5520
        if (codeToOrgUnitId.TryGetValue("B5520", out var orgUnit_B5520Id))
        {
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5520_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5520_OrgUnit_DirectorId))
            {
                var key_B5520_OrgUnit_Director = $"{orgUnit_B5520Id}_{role_B5520_OrgUnit_DirectorId}_{user_B5520_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5520_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5520Id} - {user_B5520_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5520Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5520_OrgUnit_DirectorId,
                        UserId = user_B5520_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
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
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5521_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5521_Region_DirectorId))
            {
                var key_B5521_Region_Director = $"{orgUnit_B5521Id}_{role_B5521_Region_DirectorId}_{user_B5521_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5521_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_Region_DirectorId}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_Region_DirectorId,
                        UserId = user_B5521_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5521_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5521_Region_Deputy_DirectorId))
            {
                var key_B5521_Region_Deputy_Director = $"{orgUnit_B5521Id}_{role_B5521_Region_Deputy_DirectorId}_{user_B5521_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5521_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_Region_Deputy_DirectorId,
                        UserId = user_B5521_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5521_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5521_Hub_DirectorId))
            {
                var key_B5521_Hub_Director = $"{orgUnit_B5521Id}_{role_B5521_Hub_DirectorId}_{user_B5521_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5521_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_Hub_DirectorId}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_Hub_DirectorId,
                        UserId = user_B5521_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5521_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5521_Hub_Deputy_DirectorId))
            {
                var key_B5521_Hub_Deputy_Director = $"{orgUnit_B5521Id}_{role_B5521_Hub_Deputy_DirectorId}_{user_B5521_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5521_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_Hub_Deputy_DirectorId,
                        UserId = user_B5521_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5521_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5521_OrgUnit_DirectorId))
            {
                var key_B5521_OrgUnit_Director = $"{orgUnit_B5521Id}_{role_B5521_OrgUnit_DirectorId}_{user_B5521_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5521_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_OrgUnit_DirectorId,
                        UserId = user_B5521_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5521_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5521_OrgUnit_Deputy_DirectorId))
            {
                var key_B5521_OrgUnit_Deputy_Director = $"{orgUnit_B5521Id}_{role_B5521_OrgUnit_Deputy_DirectorId}_{user_B5521_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5521_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5521Id} - {user_B5521_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5521Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5521_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5521_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5521");
        }

        // B5523
        if (codeToOrgUnitId.TryGetValue("B5523", out var orgUnit_B5523Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5523_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5523_Region_DirectorId))
            {
                var key_B5523_Region_Director = $"{orgUnit_B5523Id}_{role_B5523_Region_DirectorId}_{user_B5523_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5523_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_Region_DirectorId}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_Region_DirectorId,
                        UserId = user_B5523_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5523_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5523_Region_Deputy_DirectorId))
            {
                var key_B5523_Region_Deputy_Director = $"{orgUnit_B5523Id}_{role_B5523_Region_Deputy_DirectorId}_{user_B5523_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5523_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_Region_Deputy_DirectorId,
                        UserId = user_B5523_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5523_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5523_Hub_DirectorId))
            {
                var key_B5523_Hub_Director = $"{orgUnit_B5523Id}_{role_B5523_Hub_DirectorId}_{user_B5523_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5523_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_Hub_DirectorId}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_Hub_DirectorId,
                        UserId = user_B5523_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5523_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5523_Hub_Deputy_DirectorId))
            {
                var key_B5523_Hub_Deputy_Director = $"{orgUnit_B5523Id}_{role_B5523_Hub_Deputy_DirectorId}_{user_B5523_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5523_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_Hub_Deputy_DirectorId,
                        UserId = user_B5523_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5523_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5523_OrgUnit_DirectorId))
            {
                var key_B5523_OrgUnit_Director = $"{orgUnit_B5523Id}_{role_B5523_OrgUnit_DirectorId}_{user_B5523_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5523_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_OrgUnit_DirectorId,
                        UserId = user_B5523_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5523_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5523_OrgUnit_Deputy_DirectorId))
            {
                var key_B5523_OrgUnit_Deputy_Director = $"{orgUnit_B5523Id}_{role_B5523_OrgUnit_Deputy_DirectorId}_{user_B5523_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5523_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5523Id} - {user_B5523_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5523Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5523_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5523_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
        }
        else
        {
            missingOrgUnits.Add("B5523");
        }

        // B5524
        if (codeToOrgUnitId.TryGetValue("B5524", out var orgUnit_B5524Id))
        {
            // Region Director: sanjaym@unops.org
            if (roleNameToId.TryGetValue("Region Director", out var role_B5524_Region_DirectorId) &&
                emailToUserId.TryGetValue("sanjaym@unops.org", out var user_B5524_Region_DirectorId))
            {
                var key_B5524_Region_Director = $"{orgUnit_B5524Id}_{role_B5524_Region_DirectorId}_{user_B5524_Region_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5524_Region_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Director - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_Region_DirectorId}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_Region_DirectorId,
                        UserId = user_B5524_Region_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Director")) missingRoles.Add("Region Director");
                if (!emailToUserId.ContainsKey("sanjaym@unops.org")) missingUsers.Add("sanjaym@unops.org");
            }
            // Region Deputy Director: mikaelc@unops.org
            if (roleNameToId.TryGetValue("Region Deputy Director", out var role_B5524_Region_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mikaelc@unops.org", out var user_B5524_Region_Deputy_DirectorId))
            {
                var key_B5524_Region_Deputy_Director = $"{orgUnit_B5524Id}_{role_B5524_Region_Deputy_DirectorId}_{user_B5524_Region_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5524_Region_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Region Deputy Director - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_Region_Deputy_DirectorId}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_Region_Deputy_DirectorId,
                        UserId = user_B5524_Region_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Region Deputy Director")) missingRoles.Add("Region Deputy Director");
                if (!emailToUserId.ContainsKey("mikaelc@unops.org")) missingUsers.Add("mikaelc@unops.org");
            }
            // Hub Director: saminak@unops.org
            if (roleNameToId.TryGetValue("Hub Director", out var role_B5524_Hub_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5524_Hub_DirectorId))
            {
                var key_B5524_Hub_Director = $"{orgUnit_B5524Id}_{role_B5524_Hub_DirectorId}_{user_B5524_Hub_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5524_Hub_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Director - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_Hub_DirectorId}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_Hub_DirectorId,
                        UserId = user_B5524_Hub_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Director")) missingRoles.Add("Hub Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // Hub Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("Hub Deputy Director", out var role_B5524_Hub_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5524_Hub_Deputy_DirectorId))
            {
                var key_B5524_Hub_Deputy_Director = $"{orgUnit_B5524Id}_{role_B5524_Hub_Deputy_DirectorId}_{user_B5524_Hub_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5524_Hub_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"Hub Deputy Director - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_Hub_Deputy_DirectorId}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_Hub_Deputy_DirectorId,
                        UserId = user_B5524_Hub_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("Hub Deputy Director")) missingRoles.Add("Hub Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
            }
            // OrgUnit Director: saminak@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Director", out var role_B5524_OrgUnit_DirectorId) &&
                emailToUserId.TryGetValue("saminak@unops.org", out var user_B5524_OrgUnit_DirectorId))
            {
                var key_B5524_OrgUnit_Director = $"{orgUnit_B5524Id}_{role_B5524_OrgUnit_DirectorId}_{user_B5524_OrgUnit_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5524_OrgUnit_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Director - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_OrgUnit_DirectorId}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_OrgUnit_DirectorId,
                        UserId = user_B5524_OrgUnit_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Director")) missingRoles.Add("OrgUnit Director");
                if (!emailToUserId.ContainsKey("saminak@unops.org")) missingUsers.Add("saminak@unops.org");
            }
            // OrgUnit Deputy Director: mariapa@unops.org
            if (roleNameToId.TryGetValue("OrgUnit Deputy Director", out var role_B5524_OrgUnit_Deputy_DirectorId) &&
                emailToUserId.TryGetValue("mariapa@unops.org", out var user_B5524_OrgUnit_Deputy_DirectorId))
            {
                var key_B5524_OrgUnit_Deputy_Director = $"{orgUnit_B5524Id}_{role_B5524_OrgUnit_Deputy_DirectorId}_{user_B5524_OrgUnit_Deputy_DirectorId}";
                if (!existingRoleKeys.Contains(key_B5524_OrgUnit_Deputy_Director))
                {
                    rolesToAdd.Add(new EntityUserRole
                    {
                        Name = $"OrgUnit Deputy Director - OrganizationHierarchy - {orgUnit_B5524Id} - {user_B5524_OrgUnit_Deputy_DirectorId}",
                        EntityId = orgUnit_B5524Id,
                        EntityType = "OrganizationHierarchy",
                        EntityRoleId = role_B5524_OrgUnit_Deputy_DirectorId,
                        UserId = user_B5524_OrgUnit_Deputy_DirectorId,
                        Status = EntityStatus.Active,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = 1
                    });
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                if (!roleNameToId.ContainsKey("OrgUnit Deputy Director")) missingRoles.Add("OrgUnit Deputy Director");
                if (!emailToUserId.ContainsKey("mariapa@unops.org")) missingUsers.Add("mariapa@unops.org");
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
            Console.WriteLine($"Added {rolesToAdd.Count} new EntityUserRole records for OrganizationHierarchy.");
        }
        else
        {
            Console.WriteLine("No new EntityUserRole records to add.");
        }
        
        if (skippedCount > 0)
        {
            Console.WriteLine($"Skipped {skippedCount} existing EntityUserRole records.");
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
        
        Console.WriteLine("OrgUnitDirectorRolesSeeder completed.");
    }
}
