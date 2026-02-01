using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models.Requirements;

namespace UNOPS.PAO.Business.Workflow.StageRequirements;

/// <summary>
/// Provides stage requirements for Opportunity workflow transitions.
/// Implements IStageRequirementsProvider from the workflow submodule.
/// 
/// Requirements are defined for the IDENTIFY & PROFILE → GO transition,
/// which includes 21 mandatory field validations per PRD FR-2.1.
/// </summary>
public class OpportunityStageRequirementsProvider : IStageRequirementsProvider
{
    /// <summary>
    /// The entity name this provider handles.
    /// </summary>
    public IEnumerable<string> EntityNames => ["Opportunity"];

    /// <summary>
    /// Gets the requirements for a specific stage transition.
    /// </summary>
    /// <param name="currentStage">The current stage of the opportunity</param>
    /// <param name="nextStage">The target stage for the transition</param>
    /// <returns>List of requirements that must be met for the stage change</returns>
    public List<StageRequirement> GetRequirementsForStageChange(string currentStage, string nextStage)
    {
        // Only return requirements for IDENTIFY & PROFILE → GO transition
        if (currentStage == OpportunityWorkflow.Stages.IdentifyAndProfile &&
            nextStage == OpportunityWorkflow.Stages.Go)
        {
            return GetGoTransitionRequirements();
        }

        // No requirements for other transitions (NO GO, CANCELLED, Reopen)
        return new List<StageRequirement>();
    }

    /// <summary>
    /// Gets all mandatory field requirements for the GO transition.
    /// Based on PRD FR-2.1: 21 mandatory fields.
    /// </summary>
    private static List<StageRequirement> GetGoTransitionRequirements()
    {
        return new List<StageRequirement>
        {
            // ============================================
            // Text Fields (Required)
            // ============================================

            // 1. Opportunity Name
            new StageRequirement
            {
                Name = "name",
                Description = "message.requirements.opportunity.nameRequired",
                FieldName = "name",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 2. Description
            new StageRequirement
            {
                Name = "description",
                Description = "message.requirements.opportunity.descriptionRequired",
                FieldName = "description",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 3. Context & Challenges
            new StageRequirement
            {
                Name = "challenges",
                Description = "message.requirements.opportunity.challengesRequired",
                FieldName = "challenges",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 4. Expected Impact
            new StageRequirement
            {
                Name = "expectedImpact",
                Description = "message.requirements.opportunity.impactRequired",
                FieldName = "expectedImpact",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 5. Expected Outcomes
            new StageRequirement
            {
                Name = "expectedOutcomes",
                Description = "message.requirements.opportunity.outcomesRequired",
                FieldName = "expectedOutcomes",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 6. Opportunity Statement
            new StageRequirement
            {
                Name = "opportunityStatementMarkdown",
                Description = "message.requirements.opportunity.statementRequired",
                FieldName = "opportunityStatementMarkdown",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // ============================================
            // Number Fields (Required)
            // ============================================

            // 7. Proposed Budget (Initiative Budget USD)
            new StageRequirement
            {
                Name = "initiativeBudgetUSD",
                Description = "message.requirements.opportunity.budgetRequired",
                FieldName = "initiativeBudgetUSD",
                FieldType = FieldTypes.Number,
                Validation = new RequirementValidation { Required = true, GreaterThan = 0 }
            },

            // ============================================
            // Array Fields (minLength = 1)
            // ============================================

            // 8. Strategic Missions (UNOPS Missions)
            new StageRequirement
            {
                Name = "unopsMissions",
                Description = "message.requirements.opportunity.missionsRequired",
                FieldName = "unopsMissions",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 9. SDG Alignment
            new StageRequirement
            {
                Name = "sdgs",
                Description = "message.requirements.opportunity.sdgRequired",
                FieldName = "sdgs",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 10. Funding Partners
            new StageRequirement
            {
                Name = "fundingPartners",
                Description = "message.requirements.opportunity.fundingPartnerRequired",
                FieldName = "fundingPartners",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 11. Client Partners
            new StageRequirement
            {
                Name = "clientPartners",
                Description = "message.requirements.opportunity.clientPartnerRequired",
                FieldName = "clientPartners",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 12. Products & Services (Deliverables)
            new StageRequirement
            {
                Name = "deliverables",
                Description = "message.requirements.opportunity.productsRequired",
                FieldName = "deliverables",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 13. Countries of Implementation
            new StageRequirement
            {
                Name = "countries",
                Description = "message.requirements.opportunity.countriesRequired",
                FieldName = "countries",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // ============================================
            // Date Fields (Required)
            // ============================================

            // 14. Target Signing Date
            new StageRequirement
            {
                Name = "targetSigningDate",
                Description = "message.requirements.opportunity.signingDateRequired",
                FieldName = "targetSigningDate",
                FieldType = FieldTypes.Date,
                Validation = new RequirementValidation { Required = true }
            },

            // 15. Implementation Start Date
            new StageRequirement
            {
                Name = "implementationStartDate",
                Description = "message.requirements.opportunity.startDateRequired",
                FieldName = "implementationStartDate",
                FieldType = FieldTypes.Date,
                Validation = new RequirementValidation { Required = true }
            },

            // 16. Implementation End Date (Target Delivery Date)
            new StageRequirement
            {
                Name = "targetDeliveryDate",
                Description = "message.requirements.opportunity.endDateRequired",
                FieldName = "targetDeliveryDate",
                FieldType = FieldTypes.Date,
                Validation = new RequirementValidation { Required = true }
            },

            // ============================================
            // Select Fields (Required)
            // ============================================

            // 17. Responsible Org Unit
            new StageRequirement
            {
                Name = "responsibleOrgUnitId",
                Description = "message.requirements.opportunity.orgUnitRequired",
                FieldName = "responsibleOrgUnitId",
                FieldType = FieldTypes.Select,
                Validation = new RequirementValidation { Required = true }
            },

            // 18. Proposed Initiative Type
            new StageRequirement
            {
                Name = "proposedInitiativeTypeId",
                Description = "message.requirements.opportunity.initiativeTypeRequired",
                FieldName = "proposedInitiativeTypeId",
                FieldType = FieldTypes.Select,
                Validation = new RequirementValidation { Required = true }
            },

            // ============================================
            // Custom/Conditional Fields
            // ============================================

            // 19. Beneficiaries (Conditional validation)
            // Either BeneficiariesToBeDetermined == true OR (EstimatedDirectBeneficiaries > 0 AND EstimatedIndirectBeneficiaries >= 0)
            new StageRequirement
            {
                Name = "beneficiaries",
                Description = "message.requirements.opportunity.beneficiariesRequired",
                FieldName = "beneficiaries",
                FieldType = "conditional",
                CustomValidatorConfig = new Dictionary<string, object>
                {
                    ["validatorName"] = "BeneficiariesValidator",
                    ["fields"] = new[] { "beneficiariesToBeDetermined", "estimatedDirectBeneficiaries", "estimatedIndirectBeneficiaries" },
                    ["rule"] = "BeneficiariesToBeDetermined == true OR (EstimatedDirectBeneficiaries > 0 AND EstimatedIndirectBeneficiaries >= 0)"
                }
            },

            // 20. Opportunity Manager (Role-based validation)
            // At least one stakeholder with "Opportunity Manager" role
            new StageRequirement
            {
                Name = "opportunityManager",
                Description = "message.requirements.opportunity.managerRequired",
                FieldName = "stakeholders",
                FieldType = "roles",
                CustomValidatorConfig = new Dictionary<string, object>
                {
                    ["validatorName"] = "StakeholderRoleValidator",
                    ["requiredRole"] = "Opportunity Manager",
                    ["minCount"] = 1
                }
            },

            // 21. DoA Level 2 Holder (Server-side only validation)
            // Checks if ResponsibleOrgUnit has DoA2 holders assigned
            new StageRequirement
            {
                Name = "doaHolders",
                Description = "message.requirements.opportunity.doaHolderRequired",
                FieldName = "doaHolders",
                FieldType = "doaValidation",
                OnlyServerSideEvaluation = true,
                CustomValidatorConfig = new Dictionary<string, object>
                {
                    ["validatorName"] = "DoA2HolderValidator",
                    ["entityRoleCode"] = "DoA2_OrganizationHierarchy",
                    ["lookupField"] = "responsibleOrgUnitId"
                }
            }
        };
    }
}
