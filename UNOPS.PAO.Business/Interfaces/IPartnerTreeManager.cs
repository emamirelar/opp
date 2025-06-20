using UNOPS.PAO.RBAC.Attributes;

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
    [RBAC("create", Entity = "PartnerTree")]
    Task<PartnerTreeModel> CreatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model);

    [RBAC("read", Entity = "PartnerTree", ApplyRowFiltering = true)]
    Task<IEnumerable<PartnerTreeModel>> GetPartnerTreesAsync(ClaimsPrincipal user, string sortBy = "Name", bool ascending = true);

    [RBAC("read", Entity = "PartnerTree", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task<PartnerTreeModel?> GetPartnerTreeAsync(ClaimsPrincipal user, int id);

    IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees();

    Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id);

    [RBAC("update", Entity = "PartnerTree", RequireEntityAccess = true, EntityIdParameterName = "model.Id")]
    Task<PartnerTreeModel?> UpdatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model);

    [RBAC("delete", Entity = "PartnerTree", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task DeletePartnerTreeAsync(ClaimsPrincipal user, int id);
    
    [RBAC("read", Entity = "PartnerTree", ApplyRowFiltering = true)]
    Task<IEnumerable<object>> GetCategoryAndGroupStructureAsync(ClaimsPrincipal user);
}