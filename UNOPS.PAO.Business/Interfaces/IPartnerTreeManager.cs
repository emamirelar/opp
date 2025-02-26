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
    Task<PartnerTreeModel> CreatePartnerTreeAsync(PartnerTreeRequest model);

    IEnumerable<PartnerTreeModel> GetPartnerTrees(int userId);

    Task<PartnerTreeModel?> GetPartnerTree(int userId, int id);

    IEnumerable<ExternalPartnerTreeModel> GetPostedPartnerTrees();

    Task<ExternalPartnerTreeModel?> GetPostedPartnerTree(int id);

    Task<PartnerTreeModel?> UpdatePartnerTreeAsync(int userId, UpdatePartnerTreeRequest model);

    Task DeletePartnerTreeAsync(int userId, int id);
}