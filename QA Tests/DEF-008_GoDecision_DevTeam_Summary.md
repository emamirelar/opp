# DEF-008: Go Decision Feature Implementation Guide

**For:** Development Team  
**From:** QA Team  
**Date:** February 2, 2026  
**Priority:** P1 - High  

---

## Executive Summary

The "Send Opportunity for Go Decision" feature requires implementation to match the PRD requirements. QA has created **102 comprehensive test cases** that serve as executable acceptance criteria. Currently, **96% of tests are blocked** because the feature is not fully implemented.

---

## Current State vs. Required State

### What's Implemented Now (4 validations)

```csharp
// OpportunityStageRequirements.cs - Current
✅ Name
✅ Description  
✅ ResponsibleOrgUnitId
✅ InitiativeBudgetUSD (optional)
```

### What's Required by PRD (20+ validations)

```csharp
// Required for GO stage transition
❌ Context & Challenges (text, required)
❌ UNOPS Strategic Mission(s) (array, minLength=1)
❌ Expected Impact (text, required)
❌ Expected Outcomes (text, required)
❌ SDG Alignment (array, minLength=1)
❌ Funding Partner with amount/currency (array, minLength=1)
❌ Client Partner (array, minLength=1)
❌ Products & Services (array, minLength=1)
❌ Countries of Implementation (array, minLength=1)
❌ Target Signing Date (date, required)
❌ Implementation Start Date (date, required)
❌ Implementation End Date (date, required)
❌ Opportunity Manager role (validated server-side)
❌ Proposed Initiative Type (selection, required)
❌ DoA Level 2 holder exists (server-side check)
❌ Opportunity Statement generated (boolean check)
❌ UNCooperation Framework Outcome(s) (array, minLength=1)
❌ Estimated Beneficiaries OR acknowledgement (conditional)
❌ High Risk Acknowledgement (boolean, required)
```

---

## Implementation Priority Order

### Phase 1: Field Validations (Est. 8-12 hours)
**File:** `UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirements.cs`

Add the 16+ missing field validations to `GetGoStageRequirements()`:

```csharp
private static List<FieldRequirement> GetGoStageRequirements()
{
    return new List<FieldRequirement>
    {
        // Existing fields...
        
        // ADD THESE:
        new FieldRequirement
        {
            FieldName = "ContextAndChallenges",
            DisplayName = "Context & Challenges",
            IsRequired = true,
            ErrorMessage = "Context & Challenges is required before moving to GO stage."
        },
        new FieldRequirement
        {
            FieldName = "StrategicMissions",
            DisplayName = "UNOPS Strategic Mission(s)",
            IsRequired = true,
            ValidationRule = FieldValidationRule.NotEmpty,
            ErrorMessage = "At least one UNOPS Strategic Mission must be selected."
        },
        // ... (see full list in PRD)
    };
}
```

### Phase 2: DoA Level 2 Lookup (Est. 4-6 hours)
**New Method Required**

```csharp
// Query EntityUserRole for DoA2 approvers
public async Task<List<User>> GetDoA2ApproversAsync(int orgUnitId)
{
    return await context.EntityUserRoles
        .Where(r => r.Code == "DoA2_OrganizationHierarchy" 
                 && r.OrganizationUnitId == orgUnitId
                 && !r.IsDeleted)
        .Select(r => r.User)
        .ToListAsync();
}

// Block submission if no DoA2 found
public async Task<ValidationResult> ValidateDoA2ExistsAsync(int opportunityId)
{
    var opportunity = await GetOpportunityAsync(opportunityId);
    var doA2Holders = await GetDoA2ApproversAsync(opportunity.ResponsibleOrgUnitId);
    
    if (!doA2Holders.Any())
    {
        return ValidationResult.Failure("No DoA Level 2 holder found for this organization unit.");
    }
    return ValidationResult.Success();
}
```

### Phase 3: Warnings & Acknowledgments (Est. 6-8 hours)

1. **Non-OM Submitter Warning**
   - Check if submitter has "Opportunity Manager" role for this opportunity
   - If not, return warning (not blocker): "You are not the Opportunity Manager for this record"

2. **Country-Org Unit Warning**
   - Query `OrganizationUnitRelationship` for country-org unit match
   - If mismatch, return warning with org unit name

3. **Acknowledgment Statement**
   - Add required field: `GoDecisionAcknowledgement` (boolean)
   - Must be true before submission

### Phase 4: Custom Workflow Behavior (Est. 8-12 hours)

1. **Rejection → NO GO**
   - Override default rejection behavior
   - Instead of reverting to previous stage, set stage to "NO GO"
   - Require rejection reason

2. **CANCELLED Stage**
   - Add "CANCELLED" to stage machine
   - Implement Cancel action (from IDENTIFY & PROFILE only)
   - Implement Reopen action (from CANCELLED → IDENTIFY & PROFILE)

3. **OM Recall**
   - Any OM can recall, not just submitter
   - Returns opportunity to IDENTIFY & PROFILE

### Phase 5: Email Notifications (Est. 6-8 hours)
**Location:** `UNOPS.PAO.Business/EmailTemplates/`

Create templates with exact wording from PRD:

| Event | Template | Recipients |
|-------|----------|------------|
| Submission | `GoDecision_Submitted.html` | DoA2, OIC |
| Approval | `GoDecision_Approved.html` | OM, Collaborators, Stakeholders |
| Rejection | `GoDecision_Rejected.html` | OM |
| Recall | `GoDecision_Recalled.html` | DoA2 |

---

## Test Cases Reference

**Location:** `QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_PRD_TestCases.md`

| Category | Tests | Priority |
|----------|-------|----------|
| DoA Level 2 Approver Lookup | 6 | P0 |
| Mandatory Field Validation | 12 | P0 |
| Non-OM Submitter Warning | 4 | P1 |
| Country-Org Unit Warning | 5 | P1 |
| OM Recall Capability | 5 | P0 |
| Opportunity Statement Regeneration | 3 | P1 |
| Email Notifications | 6 | P1 |
| Custom Rejection → NO GO | 5 | P0 |
| Reopen from NO GO | 4 | P1 |
| Cancel Opportunity | 5 | P0 |
| Reopen from CANCELLED | 4 | P1 |
| Roles & Permissions | 9 | P0-P1 |
| Visibility & Workflow Lock | 5 | P0-P1 |
| Additional Validations | 5 | P0-P1 |
| **TOTAL** | **102** | - |

---

## Acceptance Criteria (Summary)

For each test case, QA will verify:

1. ✅ All 20+ mandatory fields validated before GO stage
2. ✅ DoA2 lookup returns correct approvers from EntityUserRole
3. ✅ Warnings display for non-OM submitter and country-org mismatch
4. ✅ Rejection transitions to NO GO (not previous stage)
5. ✅ CANCELLED stage works with cancel/reopen actions
6. ✅ Email notifications sent with correct wording and recipients
7. ✅ OM role transfer works (OM → Collaborator)
8. ✅ OIC receives notifications
9. ✅ Acknowledgment statement is mandatory

---

## Estimated Total Effort

| Phase | Effort | Dependencies |
|-------|--------|--------------|
| Phase 1: Field Validations | 8-12 hours | None |
| Phase 2: DoA2 Lookup | 4-6 hours | Phase 1 |
| Phase 3: Warnings | 6-8 hours | Phase 1 |
| Phase 4: Workflow | 8-12 hours | Phase 1-3 |
| Phase 5: Notifications | 6-8 hours | Phase 4 |
| **Total** | **32-46 hours** | - |

---

## How to Use Test Cases

1. **Before coding:** Review test cases for expected behavior
2. **During coding:** Test cases define exact validation rules
3. **After coding:** QA runs test cases to verify implementation
4. **Iteration:** Failed tests guide fixes

---

## Contact

**QA Lead:** QA Team  
**Defect ID:** DEF-008  
**Test Case Document:** `GoNoGoDecision_PRD_TestCases.md`  
**Execution Report:** `GoNoGoDecision_TestExecution_Report.md`

---

*This document serves as the implementation guide. Test cases serve as executable specifications.*
