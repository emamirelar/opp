# OpportunityManager Test Cases

**Manager:** `OpportunityManager`  
**Entity:** `Opportunity`  
**Test Count:** 50+  
**Priority:** P0 (Critical)  
**Created:** January 13, 2026

---

## Overview

Test cases for core Opportunity entity management including CRUD operations, lifecycle management, AI-powered suggestions, and conversion to Project/Programme/Portfolio.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| CRUD Operations | 10 | P0 |
| Status Lifecycle | 8 | P0 |
| AI Suggestions | 12 | P1 |
| Conversion | 6 | P0 |
| Validation | 8 | P0 |
| Permissions | 6 | P1 |

---

## 1. CRUD Operations (P0)

### TC-OPP-OM-F-001: Create Opportunity with Required Fields
**Priority:** P0  
**Category:** Functional

**Description:**  
Create a new opportunity with all required P3M entity properties.

**Preconditions:**
- User has create opportunity permission
- Required lookup data exists (countries, org units)

**Test Steps:**
1. Call `CreateOpportunityAsync()` with required fields:
   - Name
   - Description
   - OpportunityType (Project/Programme/Portfolio)
   - EstimatedValue
   - CurrencyId
   - PrimaryCountryId
   - ResponsibleOrgUnitId
2. Verify opportunity created
3. Verify audit fields populated
4. Verify default status is "Draft"

**Expected Results:**
- Opportunity created successfully
- ID assigned
- CreatedBy = current user ID
- CreatedDate = current timestamp
- Status = "Draft"
- All required fields populated

**Test Data:**
```json
{
  "name": "Water Infrastructure Initiative - South Asia",
  "description": "Multi-country water infrastructure project",
  "opportunityType": "Project",
  "estimatedValue": 2500000.00,
  "currencyId": 1, // USD
  "primaryCountryId": 18, // Bangladesh
  "responsibleOrgUnitId": 45
}
```

---

### TC-OPP-OM-F-002: Create Opportunity Missing Required Fields
**Priority:** P0  
**Category:** Validation

**Description:**  
Verify validation when creating opportunity without required fields.

**Test Steps:**
1. Call `CreateOpportunityAsync()` with missing Name
2. Verify `BusinessException` thrown
3. Repeat for each required field

**Expected Results:**
- `BusinessException` with clear message
- No opportunity created
- Error message indicates missing field

---

### TC-OPP-OM-F-003: Get Opportunity by ID
**Priority:** P0  
**Category:** Functional

**Description:**  
Retrieve opportunity details by ID.

**Test Steps:**
1. Create test opportunity
2. Call `GetByIdAsync(opportunityId)` with valid ID
3. Verify all fields returned correctly

**Expected Results:**
- Opportunity returned with all properties
- Navigation properties loaded if requested
- Permissions calculated correctly

---

### TC-OPP-OM-F-004: Get Opportunity by ID - Not Found
**Priority:** P0  
**Category:** Validation

**Description:**  
Handle request for non-existent opportunity.

**Test Steps:**
1. Call `GetByIdAsync(999999)` with non-existent ID
2. Verify appropriate response

**Expected Results:**
- Returns null or throws `KeyNotFoundException`
- Error message clear and actionable

---

### TC-OPP-OM-F-005: Update Opportunity Basic Fields
**Priority:** P0  
**Category:** Functional

**Description:**  
Update opportunity basic information.

**Test Steps:**
1. Create test opportunity
2. Update Name, Description, EstimatedValue
3. Call `UpdateAsync(opportunity)`
4. Verify changes saved

**Expected Results:**
- Opportunity updated successfully
- LastModifiedBy = current user
- LastModifiedDate updated
- Version number incremented

---

### TC-OPP-OM-F-006: Update Opportunity - Concurrency Check
**Priority:** P1  
**Category:** Concurrency

**Description:**  
Verify concurrency handling when multiple users update same opportunity.

**Test Steps:**
1. User A retrieves opportunity
2. User B retrieves same opportunity
3. User A updates and saves
4. User B attempts to save changes
5. Verify concurrency conflict detected

**Expected Results:**
- Concurrency exception thrown for User B
- User B notified of conflict
- No data loss

---

### TC-OPP-OM-F-007: Delete Opportunity (Soft Delete)
**Priority:** P0  
**Category:** Functional

**Description:**  
Soft delete an opportunity.

**Test Steps:**
1. Create test opportunity
2. Call `DeleteAsync(opportunityId)`
3. Verify soft delete applied

**Expected Results:**
- IsDeleted = true
- DeletedBy = current user
- DeletedDate = current timestamp
- Opportunity not returned in normal queries
- Data still in database (soft delete)

---

### TC-OPP-OM-F-008: Get All Opportunities with Pagination
**Priority:** P1  
**Category:** Functional

**Description:**  
Retrieve opportunities with pagination.

**Test Steps:**
1. Create 25 test opportunities
2. Call `GetAllAsync(page: 1, pageSize: 10)`
3. Verify pagination works correctly

**Expected Results:**
- First 10 opportunities returned
- Total count = 25
- Page metadata correct
- Opportunities in expected order

---

### TC-OPP-OM-F-009: Get Opportunities by Status
**Priority:** P1  
**Category:** Functional

**Description:**  
Filter opportunities by status.

**Test Steps:**
1. Create opportunities with various statuses
2. Call `GetByStatusAsync("Active")`
3. Verify only active opportunities returned

**Expected Results:**
- Only opportunities with Status = "Active" returned
- Count matches expected
- Ordering applied correctly

---

### TC-OPP-OM-F-010: Get Opportunities by Org Unit
**Priority:** P1  
**Category:** Functional

**Description:**  
Filter opportunities by responsible organizational unit.

**Test Steps:**
1. Create opportunities for different org units
2. Call `GetByOrgUnitAsync(orgUnitId)`
3. Verify filtering correct

**Expected Results:**
- Only opportunities for specified org unit returned
- Includes child org unit opportunities if requested
- Permissions applied correctly

---

## 2. Status Lifecycle (P0)

### TC-OPP-OM-L-001: Transition from Draft to Active
**Priority:** P0  
**Category:** Lifecycle

**Description:**  
Move opportunity from Draft to Active status.

**Test Steps:**
1. Create opportunity (status = Draft)
2. Complete required fields
3. Call `UpdateStatusAsync(opportunityId, "Active")`
4. Verify status changed

**Expected Results:**
- Status = "Active"
- ActivatedDate set
- ActivatedBy = current user
- Status history record created

---

### TC-OPP-OM-L-002: Transition to On Hold
**Priority:** P0  
**Category:** Lifecycle

**Description:**  
Put opportunity on hold.

**Test Steps:**
1. Create active opportunity
2. Call `UpdateStatusAsync(opportunityId, "OnHold", reason)`
3. Verify status and reason recorded

**Expected Results:**
- Status = "OnHold"
- OnHoldReason populated
- OnHoldDate set
- Notification sent to stakeholders

---

### TC-OPP-OM-L-003: Transition to Closed
**Priority:** P0  
**Category:** Lifecycle

**Description:**  
Close an opportunity.

**Test Steps:**
1. Create active opportunity
2. Call `UpdateStatusAsync(opportunityId, "Closed", reason)`
3. Verify closure recorded

**Expected Results:**
- Status = "Closed"
- ClosedReason populated
- ClosedDate set
- ClosedBy = current user
- Cannot be modified (unless recovered)

---

### TC-OPP-OM-L-004: Recover Closed Opportunity
**Priority:** P1  
**Category:** Lifecycle

**Description:**  
Reactivate a closed opportunity.

**Test Steps:**
1. Create closed opportunity
2. Call `RecoverOpportunityAsync(opportunityId, justification)`
3. Verify recovery successful

**Expected Results:**
- Status = "Active" or previous status
- RecoveryDate set
- RecoveredBy = current user
- RecoveryJustification recorded
- Can be modified again

---

### TC-OPP-OM-L-005: Invalid Status Transition
**Priority:** P0  
**Category:** Validation

**Description:**  
Prevent invalid status transitions.

**Test Steps:**
1. Create opportunity with status = "Closed"
2. Attempt to set status = "Draft"
3. Verify transition rejected

**Expected Results:**
- `BusinessException` thrown
- Error message explains invalid transition
- Status unchanged

---

### TC-OPP-OM-L-006: Status Transition Without Permission
**Priority:** P1  
**Category:** Security

**Description:**  
Verify permission check for status transitions.

**Test Steps:**
1. Create opportunity
2. User without permission attempts status change
3. Verify access denied

**Expected Results:**
- `UnauthorizedAccessException` thrown
- Status unchanged
- Audit log records attempt

---

### TC-OPP-OM-L-007: Status Transition Requires Justification
**Priority:** P1  
**Category:** Validation

**Description:**  
Enforce justification for certain status transitions.

**Test Steps:**
1. Create active opportunity
2. Attempt to close without reason
3. Verify validation error

**Expected Results:**
- `BusinessException` thrown
- Error indicates reason required
- Status unchanged

---

### TC-OPP-OM-L-008: Status History Tracking
**Priority:** P1  
**Category:** Audit

**Description:**  
Verify all status changes are tracked.

**Test Steps:**
1. Create opportunity (Draft)
2. Change to Active
3. Change to OnHold
4. Change to Active
5. Query status history

**Expected Results:**
- All 4 status changes recorded
- Each record has timestamp, user, reason
- History in chronological order

---

## 3. AI-Powered Suggestions (P1)

### TC-OPP-OM-AI-001: Suggest SDGs Based on Deliverables
**Priority:** P1  
**Category:** AI Integration

**Description:**  
AI suggests relevant SDGs based on opportunity deliverables.

**Test Steps:**
1. Create opportunity with deliverables:
   - "Clean water infrastructure"
   - "Sanitation systems"
2. Call `SuggestSDGsAsync(opportunityId)`
3. Verify suggestions returned

**Expected Results:**
- SDG 6 (Clean Water and Sanitation) suggested
- SDG 3 (Good Health and Well-being) suggested
- Confidence scores provided
- Justification for each suggestion

---

### TC-OPP-OM-AI-002: Suggest UN Cooperation Framework Outcomes
**Priority:** P1  
**Category:** AI Integration

**Description:**  
AI suggests applicable UN Cooperation Framework outcomes by country.

**Test Steps:**
1. Create opportunity in Bangladesh
2. Call `SuggestUNCFOutcomesAsync(opportunityId)`
3. Verify country-specific outcomes suggested

**Expected Results:**
- Relevant UNCF outcomes for Bangladesh returned
- Alignment rationale provided
- User can accept or reject suggestions

---

### TC-OPP-OM-AI-003: Accept AI Suggestion
**Priority:** P1  
**Category:** AI Integration

**Description:**  
User accepts an AI suggestion and data is updated.

**Test Steps:**
1. Get AI suggestions for opportunity
2. Call `AcceptSuggestionAsync(opportunityId, suggestionId)`
3. Verify opportunity updated

**Expected Results:**
- Suggested data applied to opportunity
- Suggestion marked as accepted
- LastModifiedBy/Date updated
- User notified of change

---

### TC-OPP-OM-AI-004: Reject AI Suggestion
**Priority:** P1  
**Category:** AI Integration

**Description:**  
User rejects an AI suggestion.

**Test Steps:**
1. Get AI suggestions for opportunity
2. Call `RejectSuggestionAsync(opportunityId, suggestionId, reason)`
3. Verify rejection recorded

**Expected Results:**
- Opportunity data unchanged
- Suggestion marked as rejected
- Rejection reason saved
- AI learns from feedback (future enhancement)

---

### TC-OPP-OM-AI-005: Suggest Related Opportunities
**Priority:** P2  
**Category:** AI Integration

**Description:**  
AI suggests similar past opportunities for reference.

**Test Steps:**
1. Create opportunity with specific characteristics
2. Call `SuggestRelatedOpportunitiesAsync(opportunityId)`
3. Verify relevant opportunities returned

**Expected Results:**
- Similar opportunities by country, sector, partner
- Similarity score provided
- Links to opportunity details
- Lessons learned highlighted

---

### TC-OPP-OM-AI-006: Suggest Partners
**Priority:** P2  
**Category:** AI Integration

**Description:**  
AI suggests potential partners based on opportunity details.

**Test Steps:**
1. Create opportunity with specific scope
2. Call `SuggestPartnersAsync(opportunityId)`
3. Verify relevant partners suggested

**Expected Results:**
- Partners with relevant experience suggested
- Past performance data included
- Geographic relevance considered
- Capacity assessment provided

---

### TC-OPP-OM-AI-007: Suggest Budget Range
**Priority:** P2  
**Category:** AI Integration

**Description:**  
AI suggests realistic budget range based on similar opportunities.

**Test Steps:**
1. Create opportunity with deliverables
2. Call `SuggestBudgetRangeAsync(opportunityId)`
3. Verify budget estimates provided

**Expected Results:**
- Minimum and maximum budget suggested
- Based on historical data
- Breakdown by cost category
- Confidence interval provided

---

### TC-OPP-OM-AI-008: Suggest Timeline
**Priority:** P2  
**Category:** AI Integration

**Description:**  
AI suggests realistic timeline based on complexity.

**Test Steps:**
1. Create opportunity with scope defined
2. Call `SuggestTimelineAsync(opportunityId)`
3. Verify timeline estimates provided

**Expected Results:**
- Start and end dates suggested
- Phase durations estimated
- Critical path identified
- Risk-adjusted timeline provided

---

### TC-OPP-OM-AI-009: Batch AI Suggestions
**Priority:** P2  
**Category:** AI Integration

**Description:**  
Get all AI suggestions for opportunity in single call.

**Test Steps:**
1. Create opportunity
2. Call `GetAllSuggestionsAsync(opportunityId)`
3. Verify comprehensive suggestions returned

**Expected Results:**
- SDGs, partners, budget, timeline all suggested
- Grouped by category
- Priority ordering applied
- Performance optimized (single AI call)

---

### TC-OPP-OM-AI-010: Suggestion History
**Priority:** P2  
**Category:** Audit

**Description:**  
Track history of AI suggestions and user actions.

**Test Steps:**
1. Create opportunity
2. Get suggestions multiple times
3. Accept some, reject others
4. Query suggestion history

**Expected Results:**
- All suggestions recorded
- User actions tracked (accept/reject/ignore)
- Timestamps for each action
- Can replay decision process

---

### TC-OPP-OM-AI-011: Disable AI Suggestions
**Priority:** P2  
**Category:** Configuration

**Description:**  
User can disable AI suggestions for specific opportunity.

**Test Steps:**
1. Create opportunity
2. Set AIAssistanceEnabled = false
3. Attempt to get suggestions
4. Verify suggestions not generated

**Expected Results:**
- AI suggestions skipped
- User notified AI is disabled
- Can re-enable at any time
- Manual data entry required

---

### TC-OPP-OM-AI-012: AI Suggestion Performance
**Priority:** P2  
**Category:** Performance

**Description:**  
Verify AI suggestions generated within acceptable time.

**Test Steps:**
1. Create complex opportunity
2. Measure time for `GetAllSuggestionsAsync()`
3. Verify performance acceptable

**Expected Results:**
- Suggestions returned in < 5 seconds
- Cached where appropriate
- Async processing doesn't block UI
- Progress indicators shown

---

## 4. Conversion to Project/Programme/Portfolio (P0)

### TC-OPP-OM-C-001: Convert Opportunity to Project
**Priority:** P0  
**Category:** Conversion

**Description:**  
Convert approved opportunity to project entity.

**Test Steps:**
1. Create opportunity with status = "Approved"
2. Call `ConvertToProjectAsync(opportunityId, projectDetails)`
3. Verify project created

**Expected Results:**
- New Project entity created
- All opportunity data copied
- Opportunity linked to project
- Opportunity status = "Converted"
- Project status = "Planning"

---

### TC-OPP-OM-C-002: Convert Opportunity to Programme
**Priority:** P0  
**Category:** Conversion

**Description:**  
Convert approved opportunity to programme entity.

**Test Steps:**
1. Create opportunity designated as Programme
2. Call `ConvertToProgrammeAsync(opportunityId)`
3. Verify programme created

**Expected Results:**
- New Programme entity created
- Opportunity data migrated
- Linkage maintained
- Can add child projects

---

### TC-OPP-OM-C-003: Convert to Portfolio
**Priority:** P1  
**Category:** Conversion

**Description:**  
Convert opportunity to portfolio.

**Test Steps:**
1. Create opportunity of type Portfolio
2. Call `ConvertToPortfolioAsync(opportunityId)`
3. Verify portfolio created

**Expected Results:**
- Portfolio entity created
- Hierarchy support enabled
- Programmes can be added
- Governance structure defined

---

### TC-OPP-OM-C-004: Prevent Conversion of Non-Approved Opportunity
**Priority:** P0  
**Category:** Validation

**Description:**  
Cannot convert opportunity that hasn't been approved.

**Test Steps:**
1. Create opportunity with status = "Draft"
2. Attempt to convert to project
3. Verify validation error

**Expected Results:**
- `BusinessException` thrown
- Error indicates approval required
- Opportunity unchanged
- Project not created

---

### TC-OPP-OM-C-005: Prevent Duplicate Conversion
**Priority:** P0  
**Category:** Validation

**Description:**  
Cannot convert already-converted opportunity.

**Test Steps:**
1. Create and convert opportunity to project
2. Attempt to convert again
3. Verify error

**Expected Results:**
- `BusinessException` thrown
- Error indicates already converted
- Reference to existing project provided

---

### TC-OPP-OM-C-006: Conversion Data Validation
**Priority:** P0  
**Category:** Validation

**Description:**  
Verify opportunity has required data before conversion.

**Test Steps:**
1. Create opportunity missing budget data
2. Attempt conversion
3. Verify validation catches missing data

**Expected Results:**
- Validation checks run before conversion
- Clear list of missing data provided
- User can complete and retry
- No partial conversion

---

## 5. Validation (P0)

### TC-OPP-OM-V-001: Validate Estimated Value Range
**Priority:** P0  
**Category:** Validation

**Description:**  
Verify estimated value is within reasonable range.

**Test Steps:**
1. Attempt to create opportunity with EstimatedValue = -100
2. Verify validation error
3. Attempt with EstimatedValue = 0
4. Verify validation error

**Expected Results:**
- Negative values rejected
- Zero values rejected
- Clear error message
- Minimum/maximum thresholds enforced

---

### TC-OPP-OM-V-002: Validate Country Exists
**Priority:** P0  
**Category:** Validation

**Description:**  
Verify primary country ID references valid country.

**Test Steps:**
1. Attempt to create opportunity with invalid CountryId
2. Verify foreign key validation

**Expected Results:**
- `BusinessException` thrown
- Error indicates invalid country
- List of valid countries suggested

---

### TC-OPP-OM-V-003: Validate Org Unit Exists
**Priority:** P0  
**Category:** Validation

**Description:**  
Verify responsible org unit ID is valid.

**Test Steps:**
1. Attempt to create with invalid OrgUnitId
2. Verify validation error

**Expected Results:**
- Foreign key validation enforced
- Error message clear
- User can select valid org unit

---

### TC-OPP-OM-V-004: Validate Timeline Logical
**Priority:** P1  
**Category:** Validation

**Description:**  
Verify start date before end date.

**Test Steps:**
1. Set StartDate = 2025-12-01
2. Set EndDate = 2025-01-01
3. Attempt to save
4. Verify validation error

**Expected Results:**
- `BusinessException` thrown
- Error indicates date logic issue
- Dates not saved

---

### TC-OPP-OM-V-005: Validate Currency Conversion
**Priority:** P1  
**Category:** Validation

**Description:**  
Verify currency conversions are valid.

**Test Steps:**
1. Create opportunity with EUR currency
2. Verify EstimatedValue stored correctly
3. Request value in USD
4. Verify conversion applied

**Expected Results:**
- Original currency preserved
- Conversions accurate
- Exchange rate source documented
- Historical rates maintained

---

### TC-OPP-OM-V-006: Validate Deliverables Required
**Priority:** P1  
**Category:** Validation

**Description:**  
Opportunity must have at least one deliverable.

**Test Steps:**
1. Create opportunity without deliverables
2. Attempt to set status = Active
3. Verify validation error

**Expected Results:**
- Validation prevents activation
- Error indicates deliverables required
- User can add and retry

---

### TC-OPP-OM-V-007: Validate Stakeholders
**Priority:** P2  
**Category:** Validation

**Description:**  
Verify stakeholder assignments are valid.

**Test Steps:**
1. Add stakeholder with invalid role
2. Verify validation error

**Expected Results:**
- Role must be from valid list
- Person must exist
- No duplicate stakeholders

---

### TC-OPP-OM-V-008: Validate Name Uniqueness
**Priority:** P2  
**Category:** Validation

**Description:**  
Opportunity names should be unique within org unit.

**Test Steps:**
1. Create opportunity "Project A"
2. Attempt to create another "Project A" in same org unit
3. Verify warning (not hard error)

**Expected Results:**
- Warning displayed
- User can override if intentional
- Search finds potential duplicates

---

## 6. Permissions (P1)

### TC-OPP-OM-P-001: Create Permission Check
**Priority:** P1  
**Category:** Security

**Description:**  
Verify user has permission to create opportunity.

**Test Steps:**
1. User without create permission attempts to create
2. Verify access denied

**Expected Results:**
- `UnauthorizedAccessException` thrown
- No opportunity created
- Audit log records attempt

---

### TC-OPP-OM-P-002: Update Permission Check
**Priority:** P1  
**Category:** Security

**Description:**  
Verify user has permission to update opportunity.

**Test Steps:**
1. User without update permission attempts update
2. Verify access denied

**Expected Results:**
- Update rejected
- Error message clear
- Opportunity unchanged

---

### TC-OPP-OM-P-003: Delete Permission Check
**Priority:** P1  
**Category:** Security

**Description:**  
Verify user has permission to delete opportunity.

**Test Steps:**
1. User without delete permission attempts delete
2. Verify access denied

**Expected Results:**
- Delete rejected
- Opportunity not marked deleted
- Security audit recorded

---

### TC-OPP-OM-P-004: View Permission with Row-Level Security
**Priority:** P1  
**Category:** Security

**Description:**  
Verify row-level security filters opportunities by org unit.

**Test Steps:**
1. Create opportunities in different org units
2. User with access to OrgUnit A only
3. Call `GetAllAsync()`
4. Verify only OrgUnit A opportunities returned

**Expected Results:**
- Row-level filtering applied
- Only authorized opportunities visible
- Count reflects filtered results

---

### TC-OPP-OM-P-006: Permission Inheritance
**Priority:** P2  
**Category:** Security

**Description:**  
Verify permissions inherited from org unit hierarchy.

**Test Steps:**
1. User has permission at parent org unit
2. Query opportunities in child org unit
3. Verify access granted

**Expected Results:**
- Hierarchical permissions respected
- Child org unit opportunities accessible
- Appropriate permission level applied

---

### TC-OPP-OM-P-007: Delegate Permissions Temporarily
**Priority:** P2  
**Category:** Security

**Description:**  
Temporarily delegate opportunity access to another user.

**Test Steps:**
1. Owner delegates access to colleague
2. Colleague can view/edit
3. Delegation expires after timeframe
4. Verify access revoked

**Expected Results:**
- Temporary access granted
- Expiration enforced
- Audit trail of delegation
- Can revoke early if needed

---

## Summary

**Total Test Cases:** 50+  
**Critical (P0):** 28  
**High (P1):** 18  
**Medium (P2):** 12

**Execution Time:** ~10-15 minutes for full suite  
**Dependencies:** Country, OrgUnit, Currency, User entities  
**Test Data:** Requires seed data for lookups

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `OpportunityManagerTests.cs`  
**Status:** ✅ Ready for Implementation
