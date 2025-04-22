using Microsoft.AspNetCore.Http;

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

    PaginationResponse<PartnerModel> GetPartners(int userId, PaginationRequest request);

    Task<PartnerModel?> GetPartner(int userId, int id);

    //IEnumerable<ExternalPartnerModel> GetPostedPartners();

    //Task<ExternalPartnerModel?> GetPostedPartner(int id);

    Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model);

    Task DeletePartnerAsync(int userId, int id);
    Task<PartnerModel?> GetPartnerAsync(int id);
    Task<string?> UpdatePartnerLogoAsync(int partnerId, IFormFile file);
}