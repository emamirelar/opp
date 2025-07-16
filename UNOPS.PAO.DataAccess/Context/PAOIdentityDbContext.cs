namespace UNOPS.PAO.DataAccess.Context;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

public class PAOIdentityDbContext : IdentityDbContext<PAOIdentityUser, PAOIdentityRole, int>
{
    private readonly IServiceProvider _serviceProvider;
    
    public PAOIdentityDbContext(DbContextOptions options, IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
            CreateRelatedEntitiesAsync(addedPaoUsers).GetAwaiter().GetResult();
        }
        
        return result;
    }

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
                var userProfile = new UserProfile
                {
                    UserId = paoUser.Id,
                    FirstName = paoUser.Email.Split('@')[0], // Extract name from email
                    LastName = "",
                    Name = paoUser.Email.Split('@')[0], // Set the Name field as well
                    Status = EntityStatus.Active,
                    CreatedBy = paoUser.Id,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = paoUser.Id,
                    IsDeleted = false,
                    DeletedBy = 0
                };

                appDbContext.Set<UserProfile>().Add(userProfile);
            }

            // Check if UserPreference already exists
            var existingPreference = await appDbContext.Set<UserPreference>()
                .FirstOrDefaultAsync(up => up.UserId == paoUser.Id);

            if (existingPreference == null)
            {
                // Get user's default org unit ID from UserInfo using email (proper way)
                int? defaultOrgUnitId = null;
                var userInfoForOrgUnit = await appDbContext.Set<UserInfo>()
                    .FirstOrDefaultAsync(ui => ui.UserEmail.ToLower() == paoUser.Email.ToLower());
                
                if (userInfoForOrgUnit?.OrgUnit != null)
                {
                    var orgUnit = await appDbContext.Set<OrganizationHierarchy>()
                        .FirstOrDefaultAsync(oh => oh.Code == userInfoForOrgUnit.OrgUnit && oh.Type == UNOPS.PAO.Domain.Enums.OrganizationUnitType.OrgUnit);
                    defaultOrgUnitId = orgUnit?.Id;
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
                        OrgUnitId = defaultOrgUnitId  // Set to user's default org unit from UserInfo
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
