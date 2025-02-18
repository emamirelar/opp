using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Workflow;

public class ProposalManager : IProposalManager
{
    private IMapper mapper;

    private DataRepository<Proposal> proposalRepository;
    private DataRepository<FundingOpportunity> fundingOpportunityRepository;

    private IWorkflowManager workflowManager;

    public ProposalManager(IMapper mapper, AppDbContext context, IWorkflowManager workflowManager)
    {
        this.mapper = mapper;
        proposalRepository = new DataRepository<Proposal>(context);
        fundingOpportunityRepository = new DataRepository<FundingOpportunity>(context);

        this.workflowManager = workflowManager;
    }

    private ExternalProposalModel EntityToExternalProposalModel(Proposal proposal)
    {
        ExternalProposalModel result = mapper.Map<ExternalProposalModel>(proposal);

        var wfState = workflowManager.GetWorkflowState(ProposalWorkflow.StateMachine, proposal.Stage, Facing.External);

        if (wfState == null)
        {
            result.Stage = string.Empty;
        }
        else
        {
            result.Stage = wfState.Stage;
        }

        return result;
    }

    public async Task<ProposalModel> CreateProposalAsync(int applicantId, ProposalRequest model)
    {
        var opp = await fundingOpportunityRepository.GetByIdAsync(model.FundingOpportunityId);

        if (opp == null)
        {
            throw new BusinessException("A valid funding opportunity is required");
        }

        if (opp.SingleSubmition)
        {
            var existingProposals = proposalRepository
                .GetAll()
                .Where(x => x.FundingOpportunityId == model.FundingOpportunityId && x.ApplicantId == applicantId)
                .Count();

            if (existingProposals > 0)
            {
                throw new BusinessException("This funding opportunity does not allows the submission of multiple proposals");
            }
        }

        var entity = mapper.Map<Proposal>(model);

        entity.ApplicantId = applicantId;

        await proposalRepository.AddAsync(entity);

        entity.Name = $"{entity.Id} - {opp.Name}";

        await proposalRepository.UpdateAsync(entity);

        return mapper.Map<ProposalModel>(entity);
    }

    public IEnumerable<ExternalProposalModel> GetApplicantProposals(int userId)
    {
        return proposalRepository
            .GetAll(["FundingOpportunity"])
            .ToList()
            .Select(EntityToExternalProposalModel);
    }

    public async Task<ExternalProposalModel?> GetApplicantProposalByIdAsync(int userId, int id)
    {
        var item = await proposalRepository.GetByIdAsync(id);
        if (item == null)
        {
            return default;
        }

        return EntityToExternalProposalModel(item);
    }

    public IEnumerable<InternalProposalModel> GetProposals()
    {
        return proposalRepository
            .GetAll(["Applicant", "FundingOpportunity"])
            .Select(x => new InternalProposalModel()
            {
                Id = x.Id,
                FundingOpportunityId = x.FundingOpportunity.Id,
                FundingOpportunityName = x.FundingOpportunity.Name,
                Applicant = new ApplicantModel()
                {
                    Id = x.Applicant.Id,
                    Name = x.Applicant.Name,
                    Email = x.Applicant.Email
                },
                EligibilityCriteriaMet = x.EligibilityCriteriaMet,
                EligibilityEntityMet = x.EligibilityEntityMet,
                SubmissionDate = x.SubmissionDate,
                Stage = x.Stage
            });
    }

    public IEnumerable<InternalProposalModel> GetFundingOpportunityProposals(int fundingOpportunityId)
    {
        // TODO: get stage from workflow?
        return proposalRepository
            .GetAll(["Applicant", "FundingOpportunity"])
            .Where(x => x.FundingOpportunityId == fundingOpportunityId)
            .Select(x => new InternalProposalModel()
            {
                Id = x.Id,
                FundingOpportunityId = x.FundingOpportunity.Id,
                FundingOpportunityName = x.FundingOpportunity.Name,
                Applicant = new ApplicantModel()
                {
                    Id = x.Applicant.Id,
                    Name = x.Applicant.Name,
                    Email = x.Applicant.Email
                },
                EligibilityCriteriaMet = x.EligibilityCriteriaMet,
                EligibilityEntityMet = x.EligibilityEntityMet,
                SubmissionDate = x.SubmissionDate,
                Stage = x.Stage
            });
    }

    public async Task<ProposalModel?> GetFundingOpportunityProposalByIdAsync(int id)
    {
        var item = await proposalRepository.GetByIdAsync(id, ["Applicant", "FundingOpportunity", "Documents"]);

        if (item == null)
        {
            return default;
        }

        var docs = await proposalRepository.GetDocumentsForEntityAsync(item.Id, DocumentParentEntityType.Proposal);
        item.Documents = docs.ToList();
        return mapper.Map<ProposalModel>(item);
    }

    public async Task<ProposalModel> UpdateProposalAsync(int userId, UpdateProposalRequest req)
    {
        var entity = await proposalRepository.GetByIdAsync(req.Id, ["Applicant"]);

        if (entity == null)
        {
            throw new BusinessException($"Proposal {req.Id} does not exist.");
        }

        entity.EligibilityCriteriaMet = req.EligibilityCriteriaMet;
        entity.EligibilityEntityMet = req.EligibilityEntityMet;

        await proposalRepository.UpdateAsync(entity);

        return mapper.Map<ProposalModel>(entity);
    }

    public async Task<string?> GetProposalStage(int id)
    {
        var item = await proposalRepository.GetByIdAsync(id);

        if (item == null)
        {
            return null;
        }

        return item.Stage;
    }

    public async Task<ProposalModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await proposalRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        if (newStage == "Submitted" && (!entity.EligibilityCriteriaMet || !entity.EligibilityEntityMet))
        {
            return default;
        }

        if (newStage == "Submitted")
        {
            entity.SubmissionDate = DateTime.UtcNow;
        }

        entity.Stage = newStage;

        await proposalRepository.UpdateAsync(entity);

        return mapper.Map<ProposalModel>(entity);
    }
}