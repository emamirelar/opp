namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using System.Security.Claims;
using UNOPS.PAO.Models.PartnerTrees;

public interface IPartnerTreeManager
{
    Task<PartnerTreeModel> CreatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model);

    Task<IEnumerable<PartnerTreeModel>> GetPartnerTreesAsync(ClaimsPrincipal user, string sortBy = "Name", bool ascending = true);

    Task<PartnerTreeModel?> GetPartnerTreeAsync(ClaimsPrincipal user, int id);

    IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees();

    Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id);

    Task<PartnerTreeModel?> UpdatePartnerTreeAsync(ClaimsPrincipal user, PartnerTreeDataModel model);

    Task DeletePartnerTreeAsync(ClaimsPrincipal user, int id);
    
    Task<IEnumerable<object>> GetCategoryAndGroupStructureAsync(ClaimsPrincipal user);
}