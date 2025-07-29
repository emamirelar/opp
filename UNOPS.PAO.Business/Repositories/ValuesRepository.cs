using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Interfaces;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDomain.Entities;

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
            .Where(x => x.Status.Equals("Active") && !x.IsDeleted);
    
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
        => context.Contacts.Where(x => !x.IsDeleted);

    public IEnumerable<GrantUser> GetUsers()
        => context.GrantUsers;

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
}