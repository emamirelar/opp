using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Utilities.Interfaces;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models.OrganizationUnits;

namespace UNOPS.PAO.Business.Repositories;
public class ValuesRepository
{
    AppDbContext context;

    public ValuesRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IEnumerable<Currency> GetCurrencies() => context.Currencies.Where(x => x.Status == EntityStatus.Active);

    public IEnumerable<EligibleEntity> GetEligibleEntities() => context.EligibleEntities.Where(x => x.Status == EntityStatus.Active);

    public IEnumerable<Country> GetCountries() => context.Countries.Where(x => x.Status == EntityStatus.Active);

    public IQueryable<Partner> GetPartners()
        => context.Partners
            .Where(x => !x.IsDeleted);
    
    // Get flat list of organization units by type
    public IEnumerable<OrganizationHierarchy> GetOrganizationsByType(OrganizationUnitType type)
        => context.OrganizationHierarchies
            .Where(x => !x.IsDeleted && x.Type == type && x.Status == EntityStatus.Active)
            .OrderBy(x => x.Name);

    // Get complete hierarchy starting from root (where ParentId is null)
    public async Task<IEnumerable<OrganizationHierarchyTreeModel>> GetOrganizationHierarchy()
    {
        var allUnits = await context.OrganizationHierarchies
            .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var rootUnits = allUnits.Where(x => x.ParentId == null).ToList();
        var result = new List<OrganizationHierarchyTreeModel>();

        foreach (var root in rootUnits)
        {
            var treeModel = new OrganizationHierarchyTreeModel
            {
                Data = new OrganizationHierarchyDataModel
                {
                    Id = root.Id,
                    Code = root.Code,
                    Name = root.Name,
                    Type = root.Type,
                    Description = root.Description,
                    ParentId = root.ParentId,
                    Children = BuildChildren(root.Id, allUnits)
                }
            };
            result.Add(treeModel);
        }

        return result;
    }

    private List<OrganizationHierarchyDataModel> BuildChildren(int parentId, List<OrganizationHierarchy> allUnits)
    {
        var children = allUnits.Where(x => x.ParentId == parentId).ToList();
        var result = new List<OrganizationHierarchyDataModel>();

        foreach (var child in children)
        {
            result.Add(new OrganizationHierarchyDataModel
            {
                Id = child.Id,
                Code = child.Code,
                Name = child.Name,
                Type = child.Type,
                Description = child.Description,
                ParentId = child.ParentId,
                Children = BuildChildren(child.Id, allUnits)
            });
        }

        return result;
    }

    // Get hierarchy for a specific organization unit and its descendants
    public async Task<OrganizationHierarchy> GetOrganizationHierarchyById(int id)
        => await context.OrganizationHierarchies
            .Where(x => !x.IsDeleted && x.Id == id)
            .Include(x => x.Children)
            .ThenInclude(child => child.Children)
            .FirstOrDefaultAsync();

    // Get flat list of all active organization units
    public IEnumerable<OrganizationHierarchy> GetAllOrganizations()
        => context.OrganizationHierarchies
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name);
    

    public IEnumerable<Contact> GetContacts()
        => context.Contacts.Where(x => !x.IsDeleted)
                   .Include(x => x.Partner);

    public IEnumerable<PAOUser> GetUsers()
        => context.PAOUsers;

    // Optimized user loading with pagination and search
    public async Task<(IEnumerable<PAOUser> Users, int TotalCount)> GetUsersPagedAsync(
        int pageIndex = 0, 
        int pageSize = 50, 
        string? searchTerm = null,
        bool activeOnly = true,
        int[]? selectedUserIds = null)
    {
        var allUsers = new List<PAOUser>();
        
        // First, get selected users if any are provided
        if (selectedUserIds != null && selectedUserIds.Length > 0)
        {
            var selectedUsers = await context.PAOUsers
                .Include(u => u.UserProfile)
                .Where(u => selectedUserIds.Contains(u.Id) && 
                           (!activeOnly || u.UserProfile == null || !u.UserProfile.IsDeleted))
                .ToListAsync();
            
            allUsers.AddRange(selectedUsers);
        }

        var query = context.PAOUsers
            .Include(u => u.UserProfile)
            .Where(u => !activeOnly || !u.UserProfile!.IsDeleted); // Filter active users if requested

        // Exclude already selected users from the main query
        if (selectedUserIds != null && selectedUserIds.Length > 0)
        {
            query = query.Where(u => !selectedUserIds.Contains(u.Id));
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(searchLower) ||
                (u.UserProfile != null && (
                    (u.UserProfile.FirstName != null && u.UserProfile.FirstName.ToLower().Contains(searchLower)) ||
                    (u.UserProfile.LastName != null && u.UserProfile.LastName.ToLower().Contains(searchLower)) ||
                    (u.UserProfile.Position != null && u.UserProfile.Position.ToLower().Contains(searchLower)) ||
                    (u.UserProfile.OrgUnit != null && u.UserProfile.OrgUnit.ToLower().Contains(searchLower))
                ))
            );
        }

        // Order by name for consistent results
        query = query.OrderBy(u => u.UserProfile != null ? u.UserProfile.FirstName ?? "" : "")
                    .ThenBy(u => u.UserProfile != null ? u.UserProfile.LastName ?? "" : "")
                    .ThenBy(u => u.Email);

        var totalCount = await query.CountAsync();
        var pagedUsers = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        allUsers.AddRange(pagedUsers);

        // Sort final result: selected users first, then paged results
        var selectedIds = selectedUserIds ?? Array.Empty<int>();
        var finalUsers = allUsers
            .OrderBy(u => selectedIds.Contains(u.Id) ? 0 : 1) // Selected users first
            .ThenBy(u => u.UserProfile?.FirstName ?? "")
            .ThenBy(u => u.UserProfile?.LastName ?? "")
            .ThenBy(u => u.Email);

        // Total count includes selected users plus the count of searchable users
        var finalTotalCount = (selectedUserIds?.Length ?? 0) + totalCount;

        return (finalUsers, finalTotalCount);
    }

    // Quick search for autocomplete - returns first 20 matches plus any selected users
    public async Task<IEnumerable<PAOUser>> SearchUsersAsync(string? searchTerm, int maxResults = 20, int[]? selectedUserIds = null)
    {
        var allUsers = new List<PAOUser>();

        // First, get selected users if any are provided
        if (selectedUserIds != null && selectedUserIds.Length > 0)
        {
            var selectedUsers = await context.PAOUsers
                .Include(u => u.UserProfile)
                .Where(u => selectedUserIds.Contains(u.Id) && u.UserProfile != null && !u.UserProfile.IsDeleted)
                .ToListAsync();
            
            allUsers.AddRange(selectedUsers);
        }

        // Then get search results if search term is provided
        if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length >= 2)
        {
            var searchLower = searchTerm.ToLower();
            
            // Exclude already selected users from search results to avoid duplicates
            var excludeIds = selectedUserIds ?? Array.Empty<int>();
            
            var searchUsers = await context.PAOUsers
                .Include(u => u.UserProfile)
                .Where(u => u.UserProfile != null && !u.UserProfile.IsDeleted &&
                    !excludeIds.Contains(u.Id) &&
                    (u.Email.ToLower().Contains(searchLower) ||
                    (u.UserProfile.FirstName != null && u.UserProfile.FirstName.ToLower().Contains(searchLower)) ||
                    (u.UserProfile.LastName != null && u.UserProfile.LastName.ToLower().Contains(searchLower)) ||
                    (u.UserProfile.Position != null && u.UserProfile.Position.ToLower().Contains(searchLower))))
                .OrderBy(u => u.UserProfile.FirstName ?? "")
                .ThenBy(u => u.UserProfile.LastName ?? "")
                .Take(maxResults)
                .ToListAsync();

            allUsers.AddRange(searchUsers);
        }

        // Sort the final result: selected users first, then search results
        var selectedIds = selectedUserIds ?? Array.Empty<int>();
        return allUsers
            .OrderBy(u => selectedIds.Contains(u.Id) ? 0 : 1) // Selected users first
            .ThenBy(u => u.UserProfile?.FirstName ?? "")
            .ThenBy(u => u.UserProfile?.LastName ?? "");
    }

    public IEnumerable<LiaisonOffice> GetLiaisonOffices() 
        => context.LiaisonOffices.Where(x => x.IsActive && !x.IsDeleted);

    // Get organization hierarchy optimized for PrimeNG organization chart
    public async Task<IEnumerable<OrganizationHierarchyPrimeModel>> GetOrganizationHierarchyPrime()
    {
        var allUnits = await context.OrganizationHierarchies
            .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active && (x.Type == 0 || x.ParentId != null))
            .OrderBy(x => x.Name)
            .ToListAsync();

        var rootUnits = allUnits.Where(x => x.ParentId == null).ToList();
        var result = new List<OrganizationHierarchyPrimeModel>();

        foreach (var root in rootUnits)
        {
            var primeModel = new OrganizationHierarchyPrimeModel
            {
                Expanded = true,
                Type = "person",
                Data = new OrganizationHierarchyPrimeDataModel
                {
                    Id = root.Id,
                    Code = root.Code ?? "N/A",
                    Name = root.Name ?? "Unnamed",
                    Type = root.Type,
                    Description = root.Description ?? "No description available",
                    ParentId = root.ParentId
                },
                Children = BuildPrimeChildren(root.Id, allUnits)
            };
            result.Add(primeModel);
        }

        return result;
    }
    
    private List<OrganizationHierarchyPrimeModel> BuildPrimeChildren(int parentId, List<OrganizationHierarchy> allUnits)
    {
        var children = allUnits.Where(x => x.ParentId == parentId).ToList();
        var result = new List<OrganizationHierarchyPrimeModel>();

        foreach (var child in children)
        {
            result.Add(new OrganizationHierarchyPrimeModel
            {
                Expanded = false, // Children start collapsed
                Type = "person",
                Data = new OrganizationHierarchyPrimeDataModel
                {
                    Id = child.Id,
                    Code = child.Code ?? "N/A",
                    Name = child.Name ?? "Unnamed",
                    Type = child.Type,
                    Description = child.Description ?? "No description available",
                    ParentId = child.ParentId
                },
                Children = BuildPrimeChildren(child.Id, allUnits)
            });
        }

        return result;
    }   

    public IEnumerable<ProposedInitiativeType> GetProposedInitiativeTypes()
        => context.ProposedInitiativeTypes.Where(x => x.Status == EntityStatus.Active);

    public IEnumerable<Output> GetOutputs()
        => context.Outputs
            .Include(x => x.Unit)
            .Include(x => x.ProjectCategory)
            .Where(x => x.Status == EntityStatus.Active);
}