namespace UNOPS.PAO.DataAccess.Context;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

public class PAOIdentityDbContext : IdentityDbContext<PAOIdentityUser, PAOIdentityRole, int>
{
    private readonly IServiceProvider _serviceProvider;
    
    public PAOIdentityDbContext(DbContextOptions<PAOIdentityDbContext> options, IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Ensure SecurityStamp is set for all users (new and modified)
        var userEntries = ChangeTracker.Entries<PAOIdentityUser>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in userEntries)
        {
            if (string.IsNullOrEmpty(entry.Entity.SecurityStamp))
            {
                entry.Entity.SecurityStamp = Guid.NewGuid().ToString();
            }
        }

        // Capture new users before saving
        var addedPaoUsers = ChangeTracker.Entries<PAOIdentityUser>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        // First save the PAOIdentityUser entities
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Then create related entities after the users are saved
        if (addedPaoUsers.Any())
        {
            await CreateRelatedEntitiesAsync(addedPaoUsers);
        }
        
        return result;
    }

    public override int SaveChanges()
    {
        // Ensure SecurityStamp is set for all users (new and modified)
        var userEntries = ChangeTracker.Entries<PAOIdentityUser>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in userEntries)
        {
            if (string.IsNullOrEmpty(entry.Entity.SecurityStamp))
            {
                entry.Entity.SecurityStamp = Guid.NewGuid().ToString();
            }
        }

        // Capture new users before saving
        var addedPaoUsers = ChangeTracker.Entries<PAOIdentityUser>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        // First save the PAOIdentityUser entities
        var result = base.SaveChanges();
        
        // Then create related entities after the users are saved
        if (addedPaoUsers.Any())
        {
            CreateRelatedEntities(addedPaoUsers);
        }
        
        return result;
    }

    private void CreateRelatedEntities(List<PAOIdentityUser> savedUsers)
    {
        // Get AppDbContext to create related entities
        using var scope = _serviceProvider.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var paoUser in savedUsers)
        {
            // Check if UserProfile already exists
            var existingProfile = appDbContext.Set<UserProfile>()
                .FirstOrDefault(up => up.UserId == paoUser.Id);

            if (existingProfile == null)
            {
                // Create UserProfile automatically with default values
                var firstName = !string.IsNullOrEmpty(paoUser.Email) ? paoUser.Email.Split('@')[0] : $"User{paoUser.Id}";
                var userProfile = new UserProfile
                {
                    UserId = paoUser.Id,
                    FirstName = firstName,
                    LastName = ""
                };

                appDbContext.Set<UserProfile>().Add(userProfile);
                
                // Save UserProfile first to ensure it exists before creating UserPreference
                appDbContext.SaveChanges();
            }

            // Check if UserPreference already exists
            var existingPreference = appDbContext.Set<UserPreference>()
                .FirstOrDefault(up => up.UserId == paoUser.Id);

            if (existingPreference == null)
            {
                // Get user's default org unit ID from UserProfile using email (proper way)
                int? defaultOrgUnitId = null;
                if (!string.IsNullOrEmpty(paoUser.Email))
                {
                    var userInfoForOrgUnit = appDbContext.Set<UserProfile>()
                        .FirstOrDefault(ui => (ui.UserEmail ?? "").ToLower() == paoUser.Email.ToLower());
                
                if (userInfoForOrgUnit?.OrgUnit != null)
                {
                    var orgUnit = appDbContext.Set<OrganizationHierarchy>()
                        .FirstOrDefault(oh => oh.Code == userInfoForOrgUnit!.OrgUnit && oh.Type == UNOPS.PAO.Domain.Enums.OrganizationUnitType.OrgUnit);
                    defaultOrgUnitId = orgUnit?.Id;
                }
                }

                // Create UserPreference automatically with default values
                var userPreference = new UserPreference
                {
                    UserId = paoUser.Id,
                    Name = $"UserPreferences_{paoUser.Id}",
                    Status = EntityStatus.Active,
                    CreatedBy = paoUser.Id,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = paoUser.Id,
                    IsDeleted = false,
                    DeletedBy = 0,
                    GlobalFilters = new GlobalFilters 
                    { 
                        OrgUnitId = defaultOrgUnitId  // Set to user's default org unit from UserProfile
                    }
                };

                appDbContext.Set<UserPreference>().Add(userPreference);
            }
        }

        // Save the related entities in AppDbContext (synchronously)
        appDbContext.SaveChanges();
    }

    // Keep the async version for backwards compatibility and async contexts
    private async Task CreateRelatedEntitiesAsync(List<PAOIdentityUser> savedUsers)
    {
        // Get AppDbContext to create related entities
        using var scope = _serviceProvider.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var paoUser in savedUsers)
        {
            // Check if UserProfile already exists
            var existingProfile = await appDbContext.Set<UserProfile>()
                .FirstOrDefaultAsync(up => up.UserId == paoUser.Id);

            if (existingProfile == null)
            {
                // Create UserProfile automatically with default values
                var firstName = !string.IsNullOrEmpty(paoUser.Email) ? paoUser.Email.Split('@')[0] : $"User{paoUser.Id}";
                var userProfile = new UserProfile
                {
                    UserId = paoUser.Id,
                    FirstName = firstName,
                    LastName = "",
                };

                appDbContext.Set<UserProfile>().Add(userProfile);
                
                // Save UserProfile first to ensure it exists before creating UserPreference
                await appDbContext.SaveChangesAsync();
            }

            // Check if UserPreference already exists
            var existingPreference = await appDbContext.Set<UserPreference>()
                .FirstOrDefaultAsync(up => up.UserId == paoUser.Id);

            if (existingPreference == null)
            {
                // Get user's default org unit ID from UserProfile using email (proper way)
                int? defaultOrgUnitId = null;
                if (!string.IsNullOrEmpty(paoUser.Email))
                {
                    var userInfoForOrgUnit = await appDbContext.Set<UserProfile>()
                        .FirstOrDefaultAsync(ui => (ui.UserEmail ?? "").ToLower() == paoUser.Email.ToLower());
                
                if (userInfoForOrgUnit?.OrgUnit != null)
                {
                    var orgUnit = await appDbContext.Set<OrganizationHierarchy>()
                        .FirstOrDefaultAsync(oh => oh.Code == userInfoForOrgUnit!.OrgUnit && oh.Type == UNOPS.PAO.Domain.Enums.OrganizationUnitType.OrgUnit);
                    defaultOrgUnitId = orgUnit?.Id;
                }
                }

                // Create UserPreference automatically with default values
                var userPreference = new UserPreference
                {
                    UserId = paoUser.Id,
                    Name = $"UserPreferences_{paoUser.Id}",
                    Status = EntityStatus.Active,
                    CreatedBy = paoUser.Id,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = paoUser.Id,
                    IsDeleted = false,
                    DeletedBy = 0,
                    GlobalFilters = new GlobalFilters 
                    { 
                        OrgUnitId = defaultOrgUnitId  // Set to user's default org unit from UserProfile
                    }
                };

                appDbContext.Set<UserPreference>().Add(userPreference);
            }
        }

        // Save the related entities in AppDbContext
        await appDbContext.SaveChangesAsync();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<PAOIdentityUser>().Ignore(x => x.GoogleSignIn);
    }
}
