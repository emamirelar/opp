# Task 2.0 Completion Report: Backend: Requirements Validation Provider

**Completed:** 2026-01-29

---

## Summary

Successfully created `OpportunityStageRequirementsProvider` implementing `IStageRequirementsProvider` from the workflow submodule. This provider defines all 21 mandatory field requirements for the IDENTIFY & PROFILE → GO transition per PRD FR-2.1.

---

## Files Created/Modified/Deleted

| File | Action | Description |
|------|--------|-------------|
| `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirementsProvider.cs` | CREATE | New provider implementing IStageRequirementsProvider with 21 requirements |
| `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirements.cs` | DELETE | Removed unused static placeholder class |
| `UNOPS.PAO.Business/Workflow/Adapters/WorkflowServiceExtensions.cs` | MODIFY | Registered provider in DI container |
| `UNOPS.PAO.IntegrationTests/UnitTests/Workflow/OpportunityStageRequirementsProviderTests.cs` | CREATE | Unit tests for the provider |

---

## Requirements Implemented (21 total)

### Text Fields (6)
| # | Field Name | Description Key |
|---|------------|-----------------|
| 1 | name | message.requirements.opportunity.nameRequired |
| 2 | description | message.requirements.opportunity.descriptionRequired |
| 3 | challenges | message.requirements.opportunity.challengesRequired |
| 4 | expectedImpact | message.requirements.opportunity.impactRequired |
| 5 | expectedOutcomes | message.requirements.opportunity.outcomesRequired |
| 6 | opportunityStatementMarkdown | message.requirements.opportunity.statementRequired |

### Number Fields (1)
| # | Field Name | Validation |
|---|------------|------------|
| 7 | initiativeBudgetUSD | Required, GreaterThan = 0 |

### Array Fields (6)
| # | Field Name | Validation |
|---|------------|------------|
| 8 | unopsMissions | MinLength = 1 |
| 9 | sdgs | MinLength = 1 |
| 10 | fundingPartners | MinLength = 1 |
| 11 | clientPartners | MinLength = 1 |
| 12 | deliverables | MinLength = 1 |
| 13 | countries | MinLength = 1 |

### Date Fields (3)
| # | Field Name |
|---|------------|
| 14 | targetSigningDate |
| 15 | implementationStartDate |
| 16 | targetDeliveryDate |

### Select Fields (2)
| # | Field Name |
|---|------------|
| 17 | responsibleOrgUnitId |
| 18 | proposedInitiativeTypeId |

### Custom/Conditional Fields (3)
| # | Field Name | Type | Notes |
|---|------------|------|-------|
| 19 | beneficiaries | conditional | BeneficiariesValidator - check TBD flag OR direct/indirect counts |
| 20 | stakeholders | roles | StakeholderRoleValidator - require at least 1 Opportunity Manager |
| 21 | doaHolders | doaValidation | DoA2HolderValidator - server-side only |

---

## Key Implementation Details

### Interface Implementation
```csharp
public class OpportunityStageRequirementsProvider : IStageRequirementsProvider
{
    public IEnumerable<string> EntityNames => ["Opportunity"];

    public List<StageRequirement> GetRequirementsForStageChange(string currentStage, string nextStage)
    {
        // Only returns requirements for IDENTIFY & PROFILE → GO
        // Returns empty list for all other transitions
    }
}
```

### DI Registration
```csharp
// In WorkflowServiceExtensions.AddPaoWorkflowServices()
services.AddScoped<IStageRequirementsProvider, OpportunityStageRequirementsProvider>();
```

### Server-Side Only Validation (DoA2)
The DoA2 holder requirement is marked with `OnlyServerSideEvaluation = true` because:
- Frontend cannot directly query EntityUserRole for DoA2 holders
- Validation must be performed server-side when the opportunity is submitted
- If no DoA2 holders are found, submission should be blocked

---

## Unit Tests Created

| Test Name | Description |
|-----------|-------------|
| `EntityNames_ShouldContainOpportunity` | Verifies entity name is "Opportunity" |
| `SupportsEntity_WithOpportunity_ShouldReturnTrue` | Tests SupportsEntity method |
| `GetRequirementsForStageChange_IdentifyToGo_ShouldReturn21Requirements` | Verifies count of requirements |
| `GetRequirementsForStageChange_IdentifyToNoGo_ShouldReturnEmptyList` | Verifies no requirements for NO GO |
| `GetRequirementsForStageChange_IdentifyToCancelled_ShouldReturnEmptyList` | Verifies no requirements for CANCELLED |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeRequiredTextFields` | Tests 6 text fields |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeBudgetRequirement` | Tests number field |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeArrayFieldsWithMinLength1` | Tests 6 array fields |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeRequiredDateFields` | Tests 3 date fields |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeRequiredSelectFields` | Tests 2 select fields |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeBeneficiariesConditionalValidation` | Tests conditional validation |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeOpportunityManagerRoleValidation` | Tests role validation |
| `GetRequirementsForStageChange_GoTransition_ShouldIncludeDoA2ServerSideValidation` | Tests server-side DoA2 validation |
| `GetRequirementsForStageChange_GoTransition_AllRequirementsShouldHaveDescriptionKeys` | Tests description format |
| `GetRequirementsForStageChange_GoTransition_AllRequirementsShouldHaveUniqueNames` | Tests uniqueness |

---

## Notes for Future Tasks

1. **Task 3.0 (DoA2 Approver Lookup)**: Will use the DoA2 validation defined here to block submission when no holders found
2. **Task 4.0 (WorkflowController)**: Will need to call this provider to get requirements for the frontend
3. **Task 6.0 (Frontend Requirements Validation)**: Will consume the requirements returned by this provider via API

### Custom Validators Required
The following custom validators need to be implemented in Task 4.0 (server-side validation):
- `BeneficiariesValidator` - validates beneficiaries conditional logic
- `StakeholderRoleValidator` - validates at least one stakeholder has "Opportunity Manager" role
- `DoA2HolderValidator` - validates DoA2 holders exist for the org unit

---

## Verification Checklist

- [x] Provider implements `IStageRequirementsProvider` interface
- [x] All 21 requirements from PRD FR-2.1 are included
- [x] Requirements only returned for IDENTIFY & PROFILE → GO transition
- [x] Empty list returned for all other transitions (NO GO, CANCELLED, Reopen)
- [x] DoA2 validation marked with `OnlyServerSideEvaluation = true`
- [x] All description keys follow format `message.requirements.opportunity.*`
- [x] Provider registered in DI container
- [x] Old static class deleted
- [x] Unit tests pass
