using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

public interface IOpportunityManager
{
    Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model);
    Task<OpportunityModel?> GetOpportunityAsync(int id);
    Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync();
    Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model);
    Task<bool> DeleteOpportunityAsync(int id);
}

