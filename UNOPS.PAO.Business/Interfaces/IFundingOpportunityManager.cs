namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public interface IFundingOpportunityManager
{
    Task<FundingOpportunityModel> CreateFundingOpportunityAsync(FundingOpportunityRequest model);

    IEnumerable<FundingOpportunityModel> GetFundingOpportunities(int userId);

    Task<FundingOpportunityModel?> GetFundingOpportunity(int userId, int id);
    Task<string?> GetFundingOpportunityStage(int id);

    IEnumerable<ExternalFundingOpportunityModel> GetPostedFundingOpportunities();

    Task<ExternalFundingOpportunityModel?> GetPostedFundingOpportunity(int id);

    Task<FundingOpportunityModel?> UpdateFundingOpportunityAsync(int userId, UpdateFundingOpportunityRequest model);
    Task<FundingOpportunityModel?> UpdateStage(int userId, int id, string newStage);

    Task DeleteFundingOpportunityAsync(int userId, int id);
}