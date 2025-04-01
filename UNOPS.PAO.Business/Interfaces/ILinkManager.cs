using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

public interface ILinkManager
{
    Task<LinkModel> CreateLinkAsync(LinkRequest model);
    IEnumerable<LinkModel> GetLinks();
    Task<LinkModel?> GetLink(int id);
    Task<LinkModel?> UpdateLinkAsync(UpdateLinkRequest model);
    Task DeleteLinkAsync(int id);
    Task<PaginationResponse<LinkModel>> GetEntityLinks(LinkEntityType entity, int entityId, PaginationRequest request);
} 