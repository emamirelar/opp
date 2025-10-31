using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;

namespace UNOPS.PAO.Business.Interfaces;

public interface IOpportunityManager
{
    Task<OpportunityModel> CreateOpportunityAsync(OpportunityRequest model);
    Task<OpportunityModel?> GetOpportunityAsync(int id);
    Task<IEnumerable<OpportunityModel>> GetAllOpportunitiesAsync();
    Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model);
    Task<OpportunityModel> UpdateWhatSectionAsync(int id, WhatSectionRequest request);
    Task<OpportunityModel> UpdateWhySectionAsync(int id, WhySectionRequest request);
    Task<OpportunityModel> UpdateWhoSectionAsync(int id, WhoSectionRequest request);
    Task<OpportunityModel> UpdateWhereSectionAsync(int id, WhereSectionRequest request);
    Task<bool> DeleteOpportunityAsync(int id);
}

