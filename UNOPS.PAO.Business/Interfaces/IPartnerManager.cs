namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public interface IPartnerManager
{
    Task<PartnerModel> CreatePartnerAsync(PartnerRequest model);

    IEnumerable<PartnerModel> GetPartners(int userId);

    Task<PartnerModel?> GetPartner(int userId, int id);

    //IEnumerable<ExternalPartnerModel> GetPostedPartners();

    //Task<ExternalPartnerModel?> GetPostedPartner(int id);

    Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model);

    Task DeletePartnerAsync(int userId, int id);
}