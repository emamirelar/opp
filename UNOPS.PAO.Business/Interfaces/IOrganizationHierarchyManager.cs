using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.OrganizationUnits;

namespace UNOPS.PAO.Business.Interfaces;

public interface IOrganizationHierarchyManager
{
    Task<IEnumerable<OrganizationHierarchyTreeModel>> GetOrganizationHierarchy();
    Task<IEnumerable<OrganizationHierarchyPrimeModel>> GetOrganizationHierarchyPrime();
    Task<OrganizationHierarchyModel> GetOrganizationHierarchyById(int id);
    IEnumerable<OrganizationHierarchyModel> GetOrganizationsByType(OrganizationUnitType type);
    IEnumerable<OrganizationHierarchyModel> GetAllOrganizations();
} 