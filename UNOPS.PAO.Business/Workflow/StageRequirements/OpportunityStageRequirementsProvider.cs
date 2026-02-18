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
    /// Order matches UI display order (Overview → What → Why → Who → Where → When → Statement → Team).
    /// </summary>
    private static List<StageRequirement> GetGoTransitionRequirements()
    {
        return new List<StageRequirement>
        {
            // ============================================
            // SECTION: OVERVIEW
            // ============================================

            // 1. Opportunity Name
            new PaoStageRequirement
            {
                Name = "name",
                Description = "message.requirements.opportunity.nameRequired",
                FieldName = "name",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 2. Description
            new PaoStageRequirement
            {
                Name = "description",
                Description = "message.requirements.opportunity.descriptionRequired",
                FieldName = "description",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 3. Proposed Budget (Initiative Budget USD)
            new PaoStageRequirement
            {
                Name = "initiativeBudgetUSD",
                Description = "message.requirements.opportunity.budgetRequired",
                FieldName = "initiativeBudgetUSD",
                FieldType = FieldTypes.Number,
                Validation = new RequirementValidation { Required = true, GreaterThan = 0 }
            },

            // ============================================
            // SECTION: WHAT (Products & Services)
            // ============================================

            // 4. Products & Services (Deliverables)
            new PaoStageRequirement
            {
                Name = "deliverables",
                Description = "message.requirements.opportunity.productsRequired",
                FieldName = "deliverables",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // ============================================
            // SECTION: WHY (Impact & Alignment)
            // ============================================

            // 5. Context & Challenges
            new PaoStageRequirement
            {
                Name = "challenges",
                Description = "message.requirements.opportunity.challengesRequired",
                FieldName = "challenges",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 6. Expected Impact
            new PaoStageRequirement
            {
                Name = "expectedImpact",
                Description = "message.requirements.opportunity.impactRequired",
                FieldName = "expectedImpact",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 7. Expected Outcomes
            new PaoStageRequirement
            {
                Name = "expectedOutcomes",
                Description = "message.requirements.opportunity.outcomesRequired",
                FieldName = "expectedOutcomes",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // 8. Beneficiaries (Conditional validation)
            // Either BeneficiariesToBeDetermined == true OR (EstimatedDirectBeneficiaries > 0 AND EstimatedIndirectBeneficiaries >= 0)
            new PaoStageRequirement
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

            // 9. SDG Alignment
            new PaoStageRequirement
            {
                Name = "sdgs",
                Description = "message.requirements.opportunity.sdgRequired",
                FieldName = "sdgs",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 10. Strategic Missions (UNOPS Missions)
            // Required: At least one mission selected, UNLESS "Not Applicable" flag is checked
            new PaoStageRequirement
            {
                Name = "unopsMissions",
                Description = "message.requirements.opportunity.missionsRequired",
                FieldName = "unopsMissions",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation
                {
                    Required = true,
                    MinLength = 1,
                    // Only require missions when "Not Applicable" is false
                    Conditional = new ConditionalValidation
                    {
                        Field = "unopsMissionsNotApplicable",
                        Value = false
                    }
                }
            },

            // ============================================
            // SECTION: WHO (Partners & People)
            // ============================================

            // 11. Funding Partners
            new PaoStageRequirement
            {
                Name = "fundingPartners",
                Description = "message.requirements.opportunity.fundingPartnerRequired",
                FieldName = "fundingPartners",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // 12. Client Partners
            new PaoStageRequirement
            {
                Name = "clientPartners",
                Description = "message.requirements.opportunity.clientPartnerRequired",
                FieldName = "clientPartners",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // ============================================
            // SECTION: WHERE (Geographic Implementation)
            // ============================================

            // 13. Countries of Implementation
            new PaoStageRequirement
            {
                Name = "countries",
                Description = "message.requirements.opportunity.countriesRequired",
                FieldName = "countries",
                FieldType = FieldTypes.Array,
                Validation = new RequirementValidation { Required = true, MinLength = 1 }
            },

            // ============================================
            // SECTION: WHEN (Timeline & Key Dates)
            // ============================================

            // 14. Target Signing Date
            new PaoStageRequirement
            {
                Name = "targetSigningDate",
                Description = "message.requirements.opportunity.signingDateRequired",
                FieldName = "targetSigningDate",
                FieldType = FieldTypes.Date,
                Validation = new RequirementValidation { Required = true }
            },

            // 15. Implementation Start Date
            new PaoStageRequirement
            {
                Name = "implementationStartDate",
                Description = "message.requirements.opportunity.startDateRequired",
                FieldName = "implementationStartDate",
                FieldType = FieldTypes.Date,
                Validation = new RequirementValidation { Required = true }
            },

            // 16. Implementation End Date (Target Delivery Date)
            new PaoStageRequirement
            {
                Name = "targetDeliveryDate",
                Description = "message.requirements.opportunity.endDateRequired",
                FieldName = "targetDeliveryDate",
                FieldType = FieldTypes.Date,
                Validation = new RequirementValidation { Required = true }
            },

            // ============================================
            // SECTION: STATEMENT
            // ============================================

            // 17. Opportunity Statement
            new PaoStageRequirement
            {
                Name = "opportunityStatementMarkdown",
                Description = "message.requirements.opportunity.statementRequired",
                FieldName = "opportunityStatementMarkdown",
                FieldType = FieldTypes.Text,
                Validation = new RequirementValidation { Required = true }
            },

            // ============================================
            // SECTION: TEAM (UNOPS Team & Stakeholders)
            // ============================================

            // 18. Opportunity Manager (Role-based validation)
            // At least one stakeholder with "Opportunity Manager" role
            new PaoStageRequirement
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

            // 19. Responsible Org Unit
            new PaoStageRequirement
            {
                Name = "responsibleOrgUnitId",
                Description = "message.requirements.opportunity.orgUnitRequired",
                FieldName = "responsibleOrgUnitId",
                FieldType = FieldTypes.Select,
                Validation = new RequirementValidation { Required = true }
            },

            // 20. Proposed Initiative Type
            new PaoStageRequirement
            {
                Name = "proposedInitiativeTypeId",
                Description = "message.requirements.opportunity.initiativeTypeRequired",
                FieldName = "proposedInitiativeTypeId",
                FieldType = FieldTypes.Select,
                Validation = new RequirementValidation { Required = true }
            },

            // 21. DoA Holder (Server-side only validation)
            // Checks if ResponsibleOrgUnit has DoA2 or DoA3 holders assigned; DoA3 used when no DoA2 exists
            new PaoStageRequirement
            {
                Name = "doaHolders",
                Description = "message.requirements.opportunity.doaHolderRequired",
                FieldName = "doaHolders",
                FieldType = "doaValidation",
                OnlyServerSideEvaluation = true,
                CustomValidatorConfig = new Dictionary<string, object>
                {
                    ["validatorName"] = "DoAHolderValidator",
                    ["entityRoleCodes"] = new[] { "DoA2_OrganizationHierarchy", "DoA3_OrganizationHierarchy" },
                    ["lookupField"] = "responsibleOrgUnitId"
                }
            }
        };
    }
}
