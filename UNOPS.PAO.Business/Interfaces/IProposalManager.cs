namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Models;

public interface IProposalManager
{
    Task<ProposalModel> CreateProposalAsync(int applicantId, ProposalRequest model);


    IEnumerable<ExternalProposalModel> GetApplicantProposals(int userId);
    Task<ExternalProposalModel?> GetApplicantProposalByIdAsync(int userId, int id);

    IEnumerable<InternalProposalModel> GetProposals();
    IEnumerable<InternalProposalModel> GetFundingOpportunityProposals(int fundingOpportunityId);

    Task<ProposalModel?> GetFundingOpportunityProposalByIdAsync(int id);

    Task<ProposalModel> UpdateProposalAsync(int userId, UpdateProposalRequest req);
    Task<ProposalModel?> UpdateStage(int userId, int id, string newStage);

    Task<string?> GetProposalStage(int id);
}
