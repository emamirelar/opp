namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using System.Security.Claims;

public interface IPartnerTreeManager
{
    Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model);

    IEnumerable<PartnerTreeModel> GetPartnerTreesAsync(int userId, string sortBy = "Name", bool ascending = true);

    Task<PartnerTreeModel?> GetPartnerTree(int userId, int id);

    IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees();

    Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id);

    Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model);

    Task DeletePartnerTreeAsync(int userId, int id);
    
    IEnumerable<object> GetCategoryAndGroupStructure(int userId);

    // New secure methods with ClaimsPrincipal for row-level security
    
    /// <summary>
    /// Creates a partner tree with entity-level access check
    /// </summary>
    Task<PartnerTreeModel> CreatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model);
    
    /// <summary>
    /// Gets partner trees with row-level filtering and entity permissions applied
    /// </summary>
    Task<IEnumerable<PartnerTreeModel>> GetPartnerTreesAsync(ClaimsPrincipal user, string sortBy = "Name", bool ascending = true);
    
    /// <summary>
    /// Gets a specific partner tree with entity-level access check and permissions
    /// </summary>
    Task<PartnerTreeModel?> GetPartnerTreeAsync(ClaimsPrincipal user, int id);
    
    /// <summary>
    /// Updates a partner tree with entity-level access check
    /// </summary>
    Task<PartnerTreeModel?> UpdatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model);
    
    /// <summary>
    /// Deletes a partner tree with entity-level access check
    /// </summary>
    Task DeletePartnerTreeAsync(ClaimsPrincipal user, int id);
    
    /// <summary>
    /// Gets category and group structure with row-level filtering applied
    /// </summary>
    Task<IEnumerable<object>> GetCategoryAndGroupStructureAsync(ClaimsPrincipal user);
}