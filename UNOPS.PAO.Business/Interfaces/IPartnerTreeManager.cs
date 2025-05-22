namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public interface IPartnerTreeManager
{
    Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeDataModel model);

    IEnumerable<PartnerTreeModel> GetPartnerTrees(int userId, string sortBy = "Name", bool ascending = true);

    Task<PartnerTreeModel?> GetPartnerTree(int userId, int id);

    IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees();

    Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id);

    Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, PartnerTreeDataModel model);

    Task DeletePartnerTreeAsync(int userId, int id);
    
    IEnumerable<object> GetCategoryAndGroupStructure(int userId);
}