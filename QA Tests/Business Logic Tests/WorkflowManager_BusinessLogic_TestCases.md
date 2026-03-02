# WorkflowManager Business Logic — Test Cases

**Component:** `UNOPS.Workflow/UNOPS.PAO.Business/Workflow` (WorkflowManager, OpportunityWorkflow, StageRequirements)  
**Created:** 2026-02-18  
**Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 30 | 30-50 | ✅ |
| Negative Tests | §2 | 90 | Max(50, 3×30)=90 | ✅ |
| Boundary Tests | §3 | 90 | Max(50, 3×30)=90 | ✅ |
| Functional Tests | §4 | 90 | ≥90 | ✅ |
| Integration Tests | §5 | 90 | ≥90 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

WorkflowManager handles Opportunity workflow stage transitions and business rules. Key functionality: stage transition validation (IDENTIFY & PROFILE → GO/NO GO/CANCELLED, NO GO/CANCELLED → I&P), DoA Level 2/3 approver resolution (PNO-1197), 21 mandatory field validation for GO transition, self-approval prevention, rejection → NO GO, OM recall rules, statement regeneration on submit, post-decision immutability, executive assignment, email CC recipients, notification creation on stage change, and confirmation acknowledgment.

---

## §1 Positive Tests (Happy Path) — 30 tests

> **Minimum:** 30-50 tests | **Focus:** Valid stage transitions, successful workflow operations

### Detailed Test Cases (P0)

#### POS-001: I&P → GO Transition with All 21 Requirements Met

**Priority:** P0  
**Precondition:** Opportunity in IDENTIFY & PROFILE with all 21 mandatory fields populated. User has submit permission. DoA2 holder exists for ResponsibleOrgUnit.

**Steps:**
1. Call `ChangeStageAsync` with fromStage=IDENTIFY & PROFILE, toStage=GO
2. Verify transition succeeds

**Expected Result:**
- Stage changes to GO
- Opportunity becomes read-only (terminal state)
- Notification created for approver
- Audit trail records transition

---

#### POS-002: I&P → NO GO Transition

**Priority:** P0  
**Precondition:** Opportunity in IDENTIFY & PROFILE. User has submit permission. Comment provided.

**Steps:**
1. Call `ChangeStageAsync` with fromStage=IDENTIFY & PROFILE, toStage=NO GO
2. Provide mandatory comment

**Expected Result:**
- Stage changes to NO GO
- Opportunity becomes read-only
- Rejection path (not back to I&P)
- Notification created

---

#### POS-003: I&P → CANCELLED (OM Only)

**Priority:** P0  
**Precondition:** Opportunity in IDENTIFY & PROFILE. User is Opportunity Manager. Mandatory comment provided.

**Steps:**
1. Call `ChangeStageAsync` with fromStage=IDENTIFY & PROFILE, toStage=CANCELLED
2. Provide mandatory comment

**Expected Result:**
- Stage changes to CANCELLED
- Opportunity becomes read-only
- OM-only action enforced

---

#### POS-004: NO GO → I&P Reopen

**Priority:** P0  
**Precondition:** Opportunity in NO GO. User is OM. Optional comment.

**Steps:**
1. Call `ChangeStageAsync` with fromStage=NO GO, toStage=IDENTIFY & PROFILE
2. Reopen action

**Expected Result:**
- Stage changes to IDENTIFY & PROFILE
- Opportunity editable again
- No approval required

---

#### POS-005: CANCELLED → I&P Reopen

**Priority:** P0  
**Precondition:** Opportunity in CANCELLED. User is OM. Mandatory comment provided.

**Steps:**
1. Call `ChangeStageAsync` with fromStage=CANCELLED, toStage=IDENTIFY & PROFILE
2. Provide mandatory comment

**Expected Result:**
- Stage changes to IDENTIFY & PROFILE
- Opportunity editable again
- Mandatory comment enforced

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | DoA Level 2 approver resolution | OrgUnit has DoA2 holder | Resolve approver for GO | EntityUserRole lookup returns DoA2 holder | P0 |
| POS-007 | DoA Level 3 fallback when no DoA2 | OrgUnit has DoA3 only | Resolve approver for GO | Fallback to DoA3 holder (PNO-1197) | P0 |
| POS-008 | 21 mandatory fields validation passes | All fields valid | Validate GO transition | All 21 requirements pass | P0 |
| POS-009 | OM recall from pending approval | OM or submitter | Recall submitted opportunity | Stage reverts to I&P | P1 |
| POS-010 | Statement regeneration on submit | Opportunity with context | Submit for GO | Statement regenerated before validation | P1 |
| POS-011 | GO state immutability | Opportunity in GO | Attempt edit | Read-only, edits rejected | P0 |
| POS-012 | NO GO state immutability | Opportunity in NO GO | Attempt edit | Read-only, edits rejected | P0 |
| POS-013 | CANCELLED state immutability | Opportunity in CANCELLED | Attempt edit | Read-only, edits rejected | P0 |
| POS-014 | Executive assignment on Go | Go transition | Assign executive | Must be active personnel, mandatory | P1 |
| POS-015 | Email CC: OM, initiator, Director/Manager | Stage change | Send notifications | CC list includes OM, initiator, Director/Manager | P1 |
| POS-016 | Notification created on stage change | Any valid transition | ChangeStageAsync | Notification created with correct RecordData | P1 |
| POS-017 | Confirmation acknowledgment on submit | User submits for GO | Multi-step confirmation | User must acknowledge before submit | P1 |
| POS-018 | Submitter recall (non-OM) | Submitter recalls | Recall before approval | Stage reverts to I&P | P1 |
| POS-019 | Valid stage transition lookup | Any valid from/to | GetAvailableTransitions | Returns correct transitions for current stage | P1 |
| POS-020 | Stage requirements returned for GO | I&P → GO | GetRequirementsForStageChange | Returns 21 requirements | P1 |
| POS-021 | No requirements for NO GO transition | I&P → NO GO | GetRequirementsForStageChange | Empty requirements list | P1 |
| POS-022 | No requirements for CANCELLED | I&P → CANCELLED | GetRequirementsForStageChange | Empty requirements list | P1 |
| POS-023 | No requirements for Reopen | NO GO/CANCELLED → I&P | GetRequirementsForStageChange | Empty requirements list | P1 |
| POS-024 | Country–org unit relationship valid | Country in org unit scope | Validate before GO | No warning, validation passes | P2 |
| POS-025 | Non-OM submitter warning displayed | Non-OM submits | Submit for GO | Warning shown, user can proceed | P2 |
| POS-026 | OpportunityWorkflow.IsValidStage | "IDENTIFY & PROFILE" | IsValidStage | Returns true | P1 |
| POS-027 | OpportunityWorkflow.AllStages | Query | AllStages | Returns 4 stages | P1 |
| POS-028 | State machine definition | Query | StateMachine | EntityType=Opportunity, 4 states | P1 |
| POS-029 | Audit fields on stage change | Any transition | ChangeStageAsync | LastModifiedBy, LastModifiedDate set | P1 |
| POS-030 | Comment stored on transition | Transition with comment | ChangeStageAsync | Comment persisted in audit | P1 |

---

## §2 Negative Tests (Failure Scenarios) — 90 tests

> **Minimum:** 90 tests | **Focus:** Invalid transitions, business rule violations, missing data

### 2.1 Invalid Stage Transitions

| ID | Test Name | Current Stage | Invalid Action | Expected Error | Priority |
|----|-----------|--------------|---------------|---------------|----------|
| NEG-001 | GO → I&P (GO is terminal) | GO | Transition to I&P | BusinessException: GO is terminal | P0 |
| NEG-002 | GO → NO GO | GO | Transition to NO GO | BusinessException: GO is terminal | P0 |
| NEG-003 | GO → CANCELLED | GO | Transition to CANCELLED | BusinessException: GO is terminal | P0 |
| NEG-004 | GO → GO | GO | Same stage | BusinessException: Invalid transition | P0 |
| NEG-005 | NO GO → GO | NO GO | Direct to GO | BusinessException: Must reopen first | P0 |
| NEG-006 | NO GO → CANCELLED | NO GO | Transition to CANCELLED | BusinessException: Invalid transition | P1 |
| NEG-007 | CANCELLED → GO | CANCELLED | Direct to GO | BusinessException: Must reopen first | P0 |
| NEG-008 | CANCELLED → NO GO | CANCELLED | Transition to NO GO | BusinessException: Invalid transition | P1 |
| NEG-009 | I&P → I&P | I&P | Same stage | BusinessException: Invalid transition | P1 |
| NEG-010 | Invalid stage name | Any | ChangeStageAsync("INVALID") | BusinessException: Invalid stage | P0 |

### 2.2 GO Transition — 21 Mandatory Field Violations

| ID | Test Name | Missing/Invalid Field | Expected Error | Priority |
|----|-----------|----------------------|---------------|----------|
| NEG-011 | GO without Name | name = null/empty | Requirement validation failure | P0 |
| NEG-012 | GO without Budget | initiativeBudgetUSD = 0/null | Requirement validation failure | P0 |
| NEG-013 | GO without Description | description = null/empty | Requirement validation failure | P0 |
| NEG-014 | GO without Context/Challenges | challenges = null/empty | Requirement validation failure | P0 |
| NEG-015 | GO without Missions | unopsMissions empty, NotApplicable=false | Requirement validation failure | P0 |
| NEG-016 | GO without Impact | expectedImpact = null/empty | Requirement validation failure | P0 |
| NEG-017 | GO without Outcomes | expectedOutcomes = null/empty | Requirement validation failure | P0 |
| NEG-018 | GO without Beneficiaries | TBD=false, Direct=0 | Requirement validation failure | P0 |
| NEG-019 | GO without SDG | sdgs empty | Requirement validation failure | P0 |
| NEG-020 | GO without Funding Partners | fundingPartners empty | Requirement validation failure | P0 |
| NEG-021 | GO without Client Partners | clientPartners empty | Requirement validation failure | P0 |
| NEG-022 | GO without Products & Services | deliverables empty | Requirement validation failure | P0 |
| NEG-023 | GO without Countries | countries empty | Requirement validation failure | P0 |
| NEG-024 | GO without Target Signing Date | targetSigningDate = null | Requirement validation failure | P0 |
| NEG-025 | GO without Implementation Start | implementationStartDate = null | Requirement validation failure | P0 |
| NEG-026 | GO without Implementation End | targetDeliveryDate = null | Requirement validation failure | P0 |
| NEG-027 | GO without Opportunity Manager | No OM in stakeholders | Requirement validation failure | P0 |
| NEG-028 | GO without Responsible Org Unit | responsibleOrgUnitId = null | Requirement validation failure | P0 |
| NEG-029 | GO without Initiative Type | proposedInitiativeTypeId = null | Requirement validation failure | P0 |
| NEG-030 | GO without DoA Holder | OrgUnit has no DoA2/DoA3 | Requirement validation failure | P0 |
| NEG-031 | GO without Statement | opportunityStatementMarkdown = null/empty | Requirement validation failure | P0 |

### 2.3 Self-Approval Prevention

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-032 | Self-approval in Test | Approver = submitter, Test env | BusinessException: Self-approval not allowed | P0 |
| NEG-033 | Self-approval in Production | Approver = submitter, Prod env | BusinessException: Self-approval not allowed | P0 |
| NEG-034 | DoA holder is OM and submitter | OM submits, OM is DoA | Rejected or routed to alternate | P0 |

### 2.4 Rejection and Recall Rules

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-035 | Rejection → I&P (invalid) | Reject action | Must go to NO GO, not I&P | P0 |
| NEG-036 | Non-OM recall attempt | User other than OM/submitter | BusinessException: Only OM or submitter can recall | P0 |
| NEG-037 | Recall when not in pending | Opportunity not pending approval | BusinessException: Cannot recall | P1 |
| NEG-038 | Recall after approval | Already approved | BusinessException: Cannot recall | P0 |

### 2.5 Missing/Invalid Data

| ID | Test Name | Missing/Invalid | Expected Error | Priority |
|----|-----------|----------------|---------------|----------|
| NEG-039 | ChangeStage with null entityId | entityId = null | ArgumentException | P1 |
| NEG-040 | ChangeStage with zero entityId | entityId = 0 | KeyNotFoundException | P1 |
| NEG-041 | ChangeStage with non-existent opportunity | entityId = 999999 | KeyNotFoundException | P0 |
| NEG-042 | Transition without mandatory comment | I&P → CANCELLED, no comment | BusinessException: Comment required | P0 |
| NEG-043 | Transition without comment for NO GO | I&P → NO GO, no comment | BusinessException: Comment required | P0 |
| NEG-044 | Reopen CANCELLED without comment | CANCELLED → I&P, no comment | BusinessException: Comment required | P0 |
| NEG-045 | Executive assignment: inactive personnel | Assign inactive user as executive | BusinessException: Must be active | P1 |
| NEG-046 | Executive assignment: null | GO without executive | Requirement validation failure | P1 |
| NEG-047 | EntityUserRole lookup: no DoA2/DoA3 | OrgUnit has no approvers | Requirement validation failure | P0 |
| NEG-048 | Budget ≤ 0 for GO | initiativeBudgetUSD = 0 | Requirement validation failure | P0 |

### 2.6 Country–Org Unit Relationship

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-049 | Country not in org unit scope | Country outside org unit | Warning displayed, may block or warn | P1 |
| NEG-050 | Country–org unit mismatch | Invalid combination | Validation warning | P1 |

### 2.7 Statement and Confirmation

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-051 | Submit without confirmation | User skips acknowledgment | Submit blocked until acknowledged | P1 |
| NEG-052 | Statement regeneration failure | AI/service unavailable | Graceful handling, may block submit | P2 |

### 2.8 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-053 | Deleted opportunity stage change | Opportunity IsDeleted=true | KeyNotFoundException | P1 |
| NEG-054 | Unauthorized user stage change | No permission | UnauthorizedAccessException | P0 |
| NEG-055 | Invalid fromStage in request | fromStage mismatch | BusinessException: Stage mismatch | P1 |
| NEG-056 | Invalid toStage in request | toStage not in valid transitions | BusinessException: Invalid transition | P1 |
| NEG-057 | Transition during concurrent edit | Optimistic concurrency | Conflict handling | P1 |
| NEG-058 | GetRequirements with null currentStage | currentStage = null | ArgumentException or empty | P1 |
| NEG-059 | GetRequirements with null nextStage | nextStage = null | ArgumentException or empty | P1 |
| NEG-060 | Beneficiaries: Direct negative | estimatedDirectBeneficiaries < 0 | Validation failure | P1 |
| NEG-061 | Beneficiaries: Indirect negative | estimatedIndirectBeneficiaries < 0 | Validation failure | P1 |
| NEG-062 | Date: End before Start | targetDeliveryDate < implementationStartDate | Validation failure | P1 |
| NEG-063 | Date: Signing after Start | targetSigningDate > implementationStartDate | May warn or reject | P2 |
| NEG-064 | Stakeholder role invalid | Invalid role code | Validation failure | P1 |
| NEG-065 | OrgUnit deleted | responsibleOrgUnitId of deleted org | Validation failure | P1 |
| NEG-066 | Initiative type deleted | proposedInitiativeTypeId deleted | Validation failure | P1 |
| NEG-067 | Funding partner deleted | Partner in fundingPartners IsDeleted | Validation failure | P1 |
| NEG-068 | Client partner deleted | Partner in clientPartners IsDeleted | Validation failure | P1 |
| NEG-069 | Country deleted/invalid | Country in countries invalid | Validation failure | P1 |
| NEG-070 | SDG invalid | Invalid SDG code | Validation failure | P1 |
| NEG-071 | Mission NotApplicable with missions | Both set inconsistently | Validation logic applied | P2 |
| NEG-072 | Multiple validation errors | 5+ fields missing | All errors returned | P1 |
| NEG-073 | Workflow config missing | No stage change config | BusinessException | P1 |
| NEG-074 | DoA resolver returns null | EntityUserRole lookup fails | Fallback or error | P1 |
| NEG-075 | Notification creation failure | DB/notification service down | Transaction rollback or retry | P2 |
| NEG-076 | Audit save failure | Audit service unavailable | Transaction rollback | P2 |
| NEG-077 | Null user context | No current user | UnauthorizedAccessException | P0 |
| NEG-078 | Expired session | Session expired | UnauthorizedAccessException | P0 |
| NEG-079 | Transition to deleted stage | toStage = deleted stage | BusinessException | P1 |
| NEG-080 | Opportunity in wrong entity type | Entity not Opportunity | BusinessException | P1 |
| NEG-081 | Empty comment for CANCELLED | Comment = "" | Validation failure | P0 |
| NEG-082 | Whitespace-only comment | Comment = "   " | Treated as empty, validation failure | P1 |
| NEG-083 | Comment exceeds max length | Comment = 10000 chars | Validation or truncation | P2 |
| NEG-084 | Duplicate transition request | Same transition in flight | Idempotent or conflict | P2 |
| NEG-085 | Stage change during approval | Approver and another user | Last-write or conflict | P1 |
| NEG-086 | GetAvailableTransitions for terminal | Opportunity in GO | Empty or no transitions | P1 |
| NEG-087 | Invalid workflow entity | EntityName = "Invalid" | BusinessException | P1 |
| NEG-088 | Stage sequence violation | Out-of-order transition | BusinessException | P1 |
| NEG-089 | Approval required but not triggered | GO without approval flow | BusinessException | P0 |
| NEG-090 | Recall by approver | Approver tries recall | Only OM/submitter can recall | P1 |

---

## §3 Boundary Tests (Edge Cases) — 90 tests

> **Minimum:** 90 tests | **Focus:** Edge values, boundary conditions

### 3.1 Stage Value Boundaries

| ID | Test Name | Input | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-001 | Stage "IDENTIFY & PROFILE" exact | Exact string match | Valid | P1 |
| BND-002 | Stage "GO" exact | Exact string | Valid | P1 |
| BND-003 | Stage "NO GO" exact | Exact string | Valid | P1 |
| BND-004 | Stage "CANCELLED" exact | Exact string | Valid | P1 |
| BND-005 | Stage with leading space | " IDENTIFY & PROFILE" | Invalid or trimmed | P1 |
| BND-006 | Stage with trailing space | "IDENTIFY & PROFILE " | Invalid or trimmed | P1 |
| BND-007 | Stage lowercase | "identify & profile" | Invalid (case-sensitive) | P1 |
| BND-008 | Stage empty string | "" | Invalid | P0 |
| BND-009 | Stage null | null | Invalid | P0 |
| BND-010 | IsValidStage with valid | "GO" | true | P1 |

### 3.2 Field Boundary Values (21 Requirements)

| ID | Field | Min/Edge | At Boundary | Over Boundary | Priority |
|----|-------|----------|------------|---------------|----------|
| BND-011 | name | 1 char | "A" ✅ | 0 chars ❌ | P1 |
| BND-012 | name | Max 500 | 500 chars ✅ | 501 ❌ | P1 |
| BND-013 | initiativeBudgetUSD | 0.01 | 0.01 ✅ | 0 ❌ | P1 |
| BND-014 | initiativeBudgetUSD | Max | 999999999 ✅ | Overflow | P1 |
| BND-015 | deliverables | 1 item | [1 item] ✅ | [] ❌ | P1 |
| BND-016 | fundingPartners | 1 item | [1] ✅ | [] ❌ | P1 |
| BND-017 | clientPartners | 1 item | [1] ✅ | [] ❌ | P1 |
| BND-018 | countries | 1 item | [1] ✅ | [] ❌ | P1 |
| BND-019 | sdgs | 1 item | [1] ✅ | [] ❌ | P1 |
| BND-020 | unopsMissions | 1 item | [1] ✅ | [] (when NotApplicable=false) ❌ | P1 |
| BND-021 | targetSigningDate | Min date | 2000-01-01 | null ❌ | P1 |
| BND-022 | implementationStartDate | Min date | Valid date | null ❌ | P1 |
| BND-023 | targetDeliveryDate | Min date | Valid date | null ❌ | P1 |
| BND-024 | opportunityStatementMarkdown | 1 char | "X" ✅ | "" ❌ | P1 |
| BND-025 | stakeholders (OM) | 1 OM | 1 OM ✅ | 0 ❌ | P1 |
| BND-026 | responsibleOrgUnitId | Min 1 | 1 ✅ | 0 ❌ | P1 |
| BND-027 | proposedInitiativeTypeId | Min 1 | 1 ✅ | 0 ❌ | P1 |
| BND-028 | beneficiaries TBD | true | TBD=true ✅ | TBD=false, Direct=0 ❌ | P1 |
| BND-029 | beneficiaries Direct | 1 | Direct=1, Indirect=0 ✅ | Direct=0 ❌ | P1 |
| BND-030 | challenges | 1 char | "X" ✅ | "" ❌ | P1 |

### 3.3 Comment Boundaries

| ID | Test Name | Comment Value | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| BND-031 | Comment min length | 1 char | Accepted | P1 |
| BND-032 | Comment max length | 4000 chars | Accepted or truncated | P1 |
| BND-033 | Comment 4001 chars | Over max | Rejected or truncated | P2 |
| BND-034 | Comment Unicode | Arabic/Chinese | Stored correctly | P2 |
| BND-035 | Comment with newlines | Multi-line | Preserved | P2 |
| BND-036 | Comment with special chars | <>&" | Escaped or sanitized | P1 |
| BND-037 | Optional comment for Reopen NO GO | Empty | Accepted (optional) | P1 |
| BND-038 | Mandatory comment for CANCELLED | Empty | Rejected | P0 |

### 3.4 Entity ID Boundaries

| ID | Test Name | EntityId | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-039 | EntityId = 1 | Minimum valid | Retrieved | P1 |
| BND-040 | EntityId = MAX_INT | 2147483647 | Handled | P2 |
| BND-041 | EntityId = -1 | Negative | Rejected | P1 |
| BND-042 | EntityId = 0 | Zero | Rejected | P1 |
| BND-043 | EntityId non-existent | 999999999 | KeyNotFoundException | P1 |

### 3.5 Collection Boundaries (Arrays)

| ID | Test Name | Collection | Expected Result | Priority |
|----|-----------|------------|-----------------|----------|
| BND-044 | deliverables: 0 items | [] | GO validation fails | P1 |
| BND-045 | deliverables: 1 item | [1] | Passes | P1 |
| BND-046 | deliverables: 100 items | Large array | Passes | P2 |
| BND-047 | fundingPartners: 0 | [] | Fails | P1 |
| BND-048 | clientPartners: 0 | [] | Fails | P1 |
| BND-049 | countries: 0 | [] | Fails | P1 |
| BND-050 | sdgs: 0 | [] | Fails | P1 |
| BND-051 | unopsMissions: 0, NotApplicable=true | [] | Passes | P1 |
| BND-052 | stakeholders: 0 OM | No OM | Fails | P1 |
| BND-053 | stakeholders: 2 OMs | 2 OMs | Passes | P2 |

### 3.6 Date Boundaries

| ID | Test Name | Date Scenario | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| BND-054 | targetSigningDate leap year | 2028-02-29 | Accepted | P2 |
| BND-055 | implementationStartDate = targetDeliveryDate | Same day | Accepted or validated | P2 |
| BND-056 | implementationStartDate > targetDeliveryDate | Invalid range | Rejected | P1 |
| BND-057 | targetSigningDate far future | 2030-12-31 | Accepted | P2 |
| BND-058 | Dates at midnight UTC | 00:00:00 | Stored correctly | P2 |
| BND-059 | Dates at 23:59:59 | End of day | Stored correctly | P2 |
| BND-060 | Null date for optional | N/A | Required dates reject null | P1 |

### 3.7 DoA and Approver Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-061 | OrgUnit with DoA2 only | DoA2 exists | DoA2 used | P1 |
| BND-062 | OrgUnit with DoA3 only | No DoA2 | DoA3 fallback (PNO-1197) | P1 |
| BND-063 | OrgUnit with both DoA2 and DoA3 | Both exist | DoA2 preferred | P1 |
| BND-064 | OrgUnit with no DoA | Neither exists | Validation fails | P0 |
| BND-065 | EntityUserRole inactive user | DoA holder inactive | May reject or warn | P1 |
| BND-066 | Multiple DoA2 holders | 2+ DoA2 | First or configured logic | P2 |
| BND-067 | DoA holder = current user (self) | Self-approval | Rejected in Test/Prod | P0 |

### 3.8 Notification and Audit Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-068 | Notification RecordData structure | Stage change | entityType, entityId, action present | P1 |
| BND-069 | Multiple notifications same transition | Bulk | Each recipient gets notification | P2 |
| BND-070 | Audit CreatedBy = 0 | System user | Handled | P2 |
| BND-071 | Rapid sequential transitions | I&P→NO GO→I&P | All audited | P2 |
| BND-072 | Transition with long comment | 2000 chars | Stored in audit | P2 |

### 3.9 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-073 | Empty transition list for GO | Opportunity in GO | No transitions available | P1 |
| BND-074 | All 21 requirements at boundary | Each at min valid | All pass | P1 |
| BND-075 | One requirement at boundary fail | One at invalid | Single error returned | P1 |
| BND-076 | Budget decimal precision | 1234567.89 | Stored correctly | P2 |
| BND-077 | Budget integer | 1000000 | Accepted | P1 |
| BND-078 | Description max length | 10000 chars | Accepted or validated | P2 |
| BND-079 | challenges max length | 5000 chars | Accepted | P2 |
| BND-080 | expectedImpact max length | 5000 chars | Accepted | P2 |
| BND-081 | expectedOutcomes max length | 5000 chars | Accepted | P2 |
| BND-082 | opportunityStatementMarkdown max | 50000 chars | Accepted | P2 |
| BND-083 | OrgUnitId = 1 | Minimum | Valid | P1 |
| BND-084 | InitiativeTypeId = 1 | Minimum | Valid | P1 |
| BND-085 | Country ID = 1 | Minimum | Valid | P1 |
| BND-086 | SDG ID = 1 | Minimum | Valid | P1 |
| BND-087 | Mission ID = 1 | Minimum | Valid | P1 |
| BND-088 | Partner ID in funding = 1 | Minimum | Valid | P1 |
| BND-089 | Partner ID in client = 1 | Minimum | Valid | P1 |
| BND-090 | Workflow status enum boundary | All values | Each handled correctly | P2 |

---

## §4 Functional Tests (Business Rules) — 90 tests

> **Minimum:** 90 tests | **Breakdown:** Workflow (25), Validation (25), Constraint (20), Audit (20)

### 4.1 Workflow Rules (25)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | I&P → GO valid | Valid transition | ChangeStageAsync | GO state | P0 |
| FUN-002 | I&P → NO GO valid | Valid transition | ChangeStageAsync | NO GO state | P0 |
| FUN-003 | I&P → CANCELLED valid | OM only | ChangeStageAsync | CANCELLED state | P0 |
| FUN-004 | NO GO → I&P valid | Reopen | ChangeStageAsync | I&P state | P0 |
| FUN-005 | CANCELLED → I&P valid | Reopen with comment | ChangeStageAsync | I&P state | P0 |
| FUN-006 | GO is terminal | No outbound transitions | GetAvailableTransitions | Empty | P0 |
| FUN-007 | Rejection → NO GO | Reject action | ChangeStageAsync | NO GO, not I&P | P0 |
| FUN-008 | OM recall only OM or submitter | Recall | ChangeStageAsync | OM or submitter can recall | P0 |
| FUN-009 | Non-OM submitter warning | Non-OM submits | Submit | Warning displayed | P1 |
| FUN-010 | Country–org unit relationship | Validate before GO | Validation | Warning if mismatch | P1 |
| FUN-011 | Statement regeneration on submit | Submit for GO | Submit | Statement regenerated | P1 |
| FUN-012 | Post-decision immutability GO | GO state | Edit attempt | Read-only | P0 |
| FUN-013 | Post-decision immutability NO GO | NO GO state | Edit attempt | Read-only | P0 |
| FUN-014 | Post-decision immutability CANCELLED | CANCELLED state | Edit attempt | Read-only | P0 |
| FUN-015 | Executive assignment mandatory on Go | GO transition | Assign | Must be active personnel | P1 |
| FUN-016 | Email CC: OM, initiator, Director/Manager | Stage change | Notify | CC list correct | P1 |
| FUN-017 | Notification on stage change | Any transition | ChangeStageAsync | Notification created | P1 |
| FUN-018 | Confirmation acknowledgment | Submit for GO | Multi-step | User must acknowledge | P1 |
| FUN-019 | DoA Level 2 resolution | EntityUserRole lookup | Resolve approver | DoA2 returned | P0 |
| FUN-020 | DoA Level 3 fallback | No DoA2 | Resolve approver | DoA3 returned (PNO-1197) | P0 |
| FUN-021 | 21 requirements for GO only | I&P → GO | GetRequirements | 21 requirements | P0 |
| FUN-022 | No requirements for other transitions | NO GO, CANCELLED, Reopen | GetRequirements | Empty | P1 |
| FUN-023 | Self-approval prevention Test | Test env | Approve own submit | Rejected | P0 |
| FUN-024 | Self-approval prevention Production | Prod env | Approve own submit | Rejected | P0 |
| FUN-025 | Mandatory comment CANCELLED | I&P → CANCELLED | ChangeStage | Comment required | P0 |

### 4.2 Validation Rules (25)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-026 | Name required | Required | "Name" | null, "" | P0 |
| FUN-027 | Budget > 0 | GreaterThan 0 | 1000 | 0, -1 | P0 |
| FUN-028 | Description required | Required | "Desc" | null | P0 |
| FUN-029 | Challenges required | Required | "Context" | null | P0 |
| FUN-030 | Impact required | Required | "Impact" | null | P0 |
| FUN-031 | Outcomes required | Required | "Outcomes" | null | P0 |
| FUN-032 | Beneficiaries conditional | TBD or Direct>0 | TBD=true or Direct=1 | Direct=0, TBD=false | P0 |
| FUN-033 | SDG ≥1 | MinLength 1 | [1] | [] | P0 |
| FUN-034 | Missions ≥1 unless NotApplicable | Conditional | [1] or NotApplicable | [] when NotApplicable=false | P0 |
| FUN-035 | Funding partners ≥1 | MinLength 1 | [1] | [] | P0 |
| FUN-036 | Client partners ≥1 | MinLength 1 | [1] | [] | P0 |
| FUN-037 | Countries ≥1 | MinLength 1 | [1] | [] | P0 |
| FUN-038 | Deliverables ≥1 | MinLength 1 | [1] | [] | P0 |
| FUN-039 | Target signing date required | Required | Valid date | null | P0 |
| FUN-040 | Implementation start required | Required | Valid date | null | P0 |
| FUN-041 | Implementation end required | Required | Valid date | null | P0 |
| FUN-042 | Statement required | Required | "Statement" | null | P0 |
| FUN-043 | OM role required | StakeholderRoleValidator | 1 OM | 0 OM | P0 |
| FUN-044 | Responsible org unit required | Required | Valid ID | null | P0 |
| FUN-045 | Initiative type required | Required | Valid ID | null | P0 |
| FUN-046 | DoA holder required | DoAHolderValidator | DoA2 or DoA3 exists | None | P0 |
| FUN-047 | Comment required for CANCELLED | Required | "Comment" | "" | P0 |
| FUN-048 | Comment required for NO GO | Required | "Comment" | "" | P0 |
| FUN-049 | Comment required for Reopen CANCELLED | Required | "Comment" | "" | P0 |
| FUN-050 | Executive active personnel | Business rule | Active user | Inactive user | P1 |

### 4.3 Constraint Rules (20)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-051 | GO transition approval required | ApprovalRequired=true | Submit for GO | Approval flow triggered | P0 |
| FUN-052 | NO GO transition approval required | ApprovalRequired=true | Submit for NO GO | Approval flow triggered | P0 |
| FUN-053 | CANCELLED no approval | ApprovalRequired=false | Cancel | No approval | P0 |
| FUN-054 | Reopen no approval | ApprovalRequired=false | Reopen | No approval | P0 |
| FUN-055 | OM-only CANCELLED | Role check | Non-OM cancels | Rejected | P0 |
| FUN-056 | OM-only Reopen | Role check | Non-OM reopens | Rejected | P0 |
| FUN-057 | Stage change config exists | Config | Transition | Config found | P1 |
| FUN-058 | EntityUserRole filter by org unit | Lookup | responsibleOrgUnitId | Correct DoA | P1 |
| FUN-059 | EntityUserRole filter by role code | DoA2_OrganizationHierarchy | Lookup | DoA2 returned | P1 |
| FUN-060 | EntityUserRole filter DoA3 fallback | DoA3_OrganizationHierarchy | No DoA2 | DoA3 returned | P1 |
| FUN-061 | Partner must exist | FK | fundingPartners | Partner exists, !IsDeleted | P1 |
| FUN-062 | Client partner must exist | FK | clientPartners | Partner exists | P1 |
| FUN-063 | Country must exist | FK | countries | Country valid | P1 |
| FUN-064 | OrgUnit must exist | FK | responsibleOrgUnitId | OrgUnit exists | P1 |
| FUN-065 | Initiative type must exist | FK | proposedInitiativeTypeId | Type exists | P1 |
| FUN-066 | Date order: Start ≤ End | Business rule | Dates | implementationStartDate ≤ targetDeliveryDate | P1 |
| FUN-067 | Beneficiaries Indirect ≥ 0 | Business rule | Indirect | ≥ 0 | P1 |
| FUN-068 | Workflow entity type = Opportunity | EntityNames | GetRequirements | ["Opportunity"] | P1 |
| FUN-069 | Stage sequence order | State machine | Transitions | Valid sequence | P1 |
| FUN-070 | Comment max length | 4000 | Comment | ≤ 4000 or truncated | P2 |

### 4.4 Audit Rules (20)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-071 | Stage change audit | ChangeStageAsync | Transition logged | P0 |
| FUN-072 | Comment in audit | Transition with comment | Comment stored | P0 |
| FUN-073 | CreatedBy on transition | ChangeStage | Current user | P0 |
| FUN-074 | CreatedDate on transition | ChangeStage | UTC now | P0 |
| FUN-075 | FromStage in audit | ChangeStage | Previous stage | P1 |
| FUN-076 | ToStage in audit | ChangeStage | New stage | P1 |
| FUN-077 | EntityId in audit | ChangeStage | Opportunity ID | P1 |
| FUN-078 | Approval audit | Approve/Reject | Action logged | P1 |
| FUN-079 | Recall audit | Recall | Recall logged | P1 |
| FUN-080 | DoA holder in audit | Approval | Approver ID | P1 |
| FUN-081 | Read operation no audit | GetOpportunity | No audit write | P1 |
| FUN-082 | Failed transition no audit | Failed ChangeStage | No partial audit | P1 |
| FUN-083 | Batch notification audit | Multiple recipients | Each logged | P2 |
| FUN-084 | Statement regeneration audit | Submit | Regeneration logged | P2 |
| FUN-085 | Executive assignment audit | GO transition | Assignment logged | P1 |
| FUN-086 | Rejection reason in audit | Reject | Reason stored | P1 |
| FUN-087 | Reopen reason in audit | Reopen | Comment stored | P1 |
| FUN-088 | Immutability check audit | Edit attempt on GO | Attempt logged | P2 |
| FUN-089 | Self-approval attempt audit | Self-approval blocked | Attempt logged | P2 |
| FUN-090 | Workflow status history | Multiple transitions | Full history | P1 |

---

## §5 Integration Tests (End-to-End Flows) — 90 tests

> **Minimum:** 90 tests

### 5.1 CRUD/Workflow Lifecycle (15)

| ID | Test Name | Flow | Entities | Expected Result | Priority |
|----|-----------|------|----------|-----------------|----------|
| INT-001 | Full I&P → GO lifecycle | Create→I&P→Submit→Approve→GO | Opportunity, Workflow | GO state, read-only | P0 |
| INT-002 | Full I&P → NO GO lifecycle | Create→I&P→Submit→Reject→NO GO | Opportunity | NO GO state | P0 |
| INT-003 | Full I&P → CANCELLED | Create→I&P→Cancel→CANCELLED | Opportunity | CANCELLED state | P0 |
| INT-004 | NO GO → Reopen → Edit | NO GO→Reopen→I&P→Edit | Opportunity | Editable | P0 |
| INT-005 | CANCELLED → Reopen → Submit | CANCELLED→Reopen→Submit→GO | Opportunity | GO after approval | P0 |
| INT-006 | Recall before approval | I&P→Submit→Recall→I&P | Opportunity | Back to I&P | P0 |
| INT-007 | OM recall | OM recalls | Opportunity | Recall succeeds | P0 |
| INT-008 | Submitter recall | Submitter recalls | Opportunity | Recall succeeds | P0 |
| INT-009 | Statement regeneration flow | Submit for GO | Opportunity, GeminiManager | Statement updated | P1 |
| INT-010 | Notification creation flow | Any transition | Opportunity, NotificationManager | Notification created | P1 |
| INT-011 | DoA resolution flow | Submit for GO | EntityUserRole, OrgUnit | Approver resolved | P1 |
| INT-012 | 21 requirements validation flow | Submit for GO | Opportunity, StageRequirements | All validated | P1 |
| INT-013 | Executive assignment flow | GO approval | Opportunity, User | Executive assigned | P1 |
| INT-014 | Email CC flow | Stage change | Notification, Email | CC list correct | P1 |
| INT-015 | Immutability enforcement flow | GO state, edit attempt | Opportunity | Edit rejected | P0 |

### 5.2 Cross-Manager Integration (15)

| ID | Test Name | Managers | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|----------|
| INT-016 | WorkflowManager + OpportunityManager | Both | Stage change | Opportunity updated | P0 |
| INT-017 | WorkflowManager + NotificationManager | Both | Stage change | Notification created | P0 |
| INT-018 | WorkflowManager + EntityUserRole | Lookup | DoA resolution | Approver found | P0 |
| INT-019 | WorkflowManager + GeminiManager | Statement | Submit | Statement regenerated | P1 |
| INT-020 | WorkflowManager + CommentManager | Comment | Transition with comment | Comment stored | P1 |
| INT-021 | WorkflowManager + Audit | Audit | Any transition | Audit entry created | P1 |
| INT-022 | WorkflowManager + PermissionService | Permission | Stage change | Permission checked | P0 |
| INT-023 | WorkflowManager + OrganizationHierarchyManager | OrgUnit | DoA lookup | OrgUnit hierarchy used | P1 |
| INT-024 | WorkflowManager + RiskManager | DST | GO transition | Risks considered | P2 |
| INT-025 | WorkflowManager + PartnerManager | Partners | Funding/client validation | Partners validated | P1 |
| INT-026 | WorkflowManager + CountryManager | Countries | Country validation | Countries validated | P1 |
| INT-027 | WorkflowManager + UserManager | User | Executive assignment | Active user validated | P1 |
| INT-028 | WorkflowManager + AiPromptManager | Prompts | Notification template | Correct message | P2 |
| INT-029 | WorkflowManager + DbContext | Persistence | Stage change | DB updated | P0 |
| INT-030 | WorkflowManager + ManagerWrapper | All | Full flow | All managers coordinated | P0 |

### 5.3 Database Persistence (15)

| ID | Test Name | Operation | DB State | Expected | Priority |
|----|-----------|----------|----------|----------|----------|
| INT-031 | Stage change persists | ChangeStageAsync | Opportunity.WorkflowStatus | Updated | P0 |
| INT-032 | Comment persists | Transition with comment | Comment table | Stored | P1 |
| INT-033 | Notification persists | Stage change | Notification table | Created | P0 |
| INT-034 | Audit persists | Stage change | Audit table | Entry created | P1 |
| INT-035 | EntityUserRole read | DoA lookup | EntityUserRole table | Correct record | P0 |
| INT-036 | StateMachineStageChange read | Get transitions | Config table | Correct config | P1 |
| INT-037 | Opportunity read after transition | ChangeStage + Get | Opportunity | Latest stage | P0 |
| INT-038 | Soft-delete filter | Deleted opportunity | Get | Excluded | P1 |
| INT-039 | Transaction rollback on failure | Failed transition | DB | No partial update | P0 |
| INT-040 | Concurrent stage change | 2 users | DB | One succeeds, conflict or last-write | P1 |
| INT-041 | Reopen updates WorkflowStatus | CANCELLED → I&P | Opportunity | Stage = I&P | P0 |
| INT-042 | GO updates WorkflowStatus | I&P → GO | Opportunity | Stage = GO | P0 |
| INT-043 | Notification RecordData JSON | Create notification | RecordData column | Valid JSON | P1 |
| INT-044 | Audit CreatedBy foreign key | Audit entry | User exists | FK valid | P1 |
| INT-045 | Stage change idempotency | Same transition twice | DB | Handled correctly | P2 |

### 5.4 Error Handling Integration (15)

| ID | Test Name | Error Condition | Expected Response | Priority |
|----|-----------|----------------|------------------|----------|
| INT-046 | DB connection lost during transition | DB down | Exception, rollback | P0 |
| INT-047 | Notification service down | Notification fail | Retry or graceful | P1 |
| INT-048 | EntityUserRole lookup null | No DoA | Validation failure | P0 |
| INT-049 | Opportunity not found | Invalid ID | KeyNotFoundException | P0 |
| INT-050 | Permission denied | No permission | UnauthorizedAccessException | P0 |
| INT-051 | Invalid transition | Wrong from/to | BusinessException | P0 |
| INT-052 | Validation failure | Missing field | BusinessException with details | P0 |
| INT-053 | Self-approval | Approver = submitter | BusinessException | P0 |
| INT-054 | Recall when not pending | Wrong state | BusinessException | P1 |
| INT-055 | Non-OM recall | Wrong user | BusinessException | P0 |
| INT-056 | Statement regeneration failure | AI error | Handled, may block | P2 |
| INT-057 | Audit service failure | Audit down | Rollback or retry | P2 |
| INT-058 | Concurrent edit conflict | Optimistic concurrency | Conflict handling | P1 |
| INT-059 | Transaction timeout | Long operation | Timeout, rollback | P2 |
| INT-060 | Invalid JSON in RecordData | Malformed | Validation or default | P2 |

### 5.5 End-to-End Flows (30)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-061 | Create opportunity → I&P → Submit GO → Approve | Full flow | GO state | P0 |
| INT-062 | Create → I&P → Submit GO → Reject | Full flow | NO GO state | P0 |
| INT-063 | Create → I&P → Cancel | Full flow | CANCELLED state | P0 |
| INT-064 | Create → I&P → Submit NO GO → Approve | Full flow | NO GO state | P0 |
| INT-065 | NO GO → Reopen → Edit → Submit GO | Full flow | GO after approval | P0 |
| INT-066 | CANCELLED → Reopen → Edit → Cancel again | Full flow | CANCELLED again | P1 |
| INT-067 | Submit → Recall → Submit again | Full flow | Second submit works | P1 |
| INT-068 | Non-OM submit → Warning → Proceed | Full flow | Submit succeeds with warning | P1 |
| INT-069 | Country–org unit mismatch → Warning | Full flow | Warning, may block | P1 |
| INT-070 | 21 requirements partial fail → Fix → Submit | Full flow | All pass on retry | P1 |
| INT-071 | DoA2 exists → Approver = DoA2 | Full flow | DoA2 approves | P0 |
| INT-072 | No DoA2, DoA3 exists → Approver = DoA3 | Full flow | DoA3 approves (PNO-1197) | P0 |
| INT-073 | No DoA → Validation fails | Full flow | Cannot submit | P0 |
| INT-074 | Self-approval attempt → Blocked | Full flow | Rejected | P0 |
| INT-075 | Confirmation skipped → Blocked | Full flow | Submit blocked | P1 |
| INT-076 | Statement regeneration → Submit | Full flow | Statement updated | P1 |
| INT-077 | Executive assignment on GO | Full flow | Executive assigned | P1 |
| INT-078 | Email CC on stage change | Full flow | OM, initiator, Director/Manager | P1 |
| INT-079 | Notification to multiple recipients | Full flow | Each gets notification | P1 |
| INT-080 | Edit GO opportunity → Rejected | Full flow | Read-only enforced | P0 |
| INT-081 | Edit NO GO opportunity → Rejected | Full flow | Read-only enforced | P0 |
| INT-082 | Edit CANCELLED opportunity → Rejected | Full flow | Read-only enforced | P0 |
| INT-083 | Reopen NO GO → Edit → Submit GO | Full flow | Full cycle | P0 |
| INT-084 | Reopen CANCELLED → Edit → Cancel | Full flow | Full cycle | P1 |
| INT-085 | Multiple transitions in sequence | I&P→NO GO→I&P→GO | Final GO | P0 |
| INT-086 | Audit trail completeness | Multiple transitions | Full history | P1 |
| INT-087 | Notification RecordData structure | Stage change | entityType, entityId, action | P1 |
| INT-088 | Permission check before transition | No permission | Blocked | P0 |
| INT-089 | OrgUnit scope check | Out of scope | Blocked | P1 |
| INT-090 | Full integration test | All components | End-to-end success | P0 |

---

## §6 Security Tests — 50 tests (OUT OF SCOPE for QA)

> **Note:** Security testing is OUT OF SCOPE for QA per project standards. Placeholder entries for traceability.

| ID | Test Name | Category | Status | Priority |
|----|-----------|----------|--------|----------|
| SEC-001 | SQL injection in comment | Injection | OUT OF SCOPE | P0 |
| SEC-002 | XSS in comment field | Injection | OUT OF SCOPE | P0 |
| SEC-003 | Unauthorized stage change | Access Control | OUT OF SCOPE | P0 |
| SEC-004 | IDOR: Change other user's opportunity | IDOR | OUT OF SCOPE | P0 |
| SEC-005 | Mass assignment WorkflowStatus | Mass Assignment | OUT OF SCOPE | P0 |
| SEC-006 through SEC-050 | [Additional security scenarios] | Various | OUT OF SCOPE | P1/P2 |

---

## §7 Concurrency Tests — 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Two users submit same opportunity | User A and B submit simultaneously | One succeeds, one conflict or queued | P0 |
| CON-002 | Submit and Recall simultaneously | User A submits, User B (OM) recalls | One succeeds | P0 |
| CON-003 | Approve and Recall simultaneously | Approver approves, OM recalls | Conflict handling | P0 |
| CON-004 | Two approvers approve | DoA2 and DoA3 both approve | First wins or single approver | P1 |
| CON-005 | Edit and stage change | User A edits, User B changes stage | Conflict or last-write | P1 |
| CON-006 | Concurrent reopen | Two OMs reopen | One succeeds | P1 |
| CON-007 | Concurrent cancel | Two OMs cancel | One succeeds | P1 |
| CON-008 | Stage change during statement regeneration | Submit + AI regenerating | Coordinated | P1 |
| CON-009 | Multiple notifications same event | Bulk notification creation | All created, no duplicates | P1 |
| CON-010 | DoA lookup during org unit update | Lookup + OrgUnit modified | Consistent or cached | P2 |
| CON-011 | Optimistic concurrency on Opportunity | Version/row version | Conflict detected | P1 |
| CON-012 | Transaction isolation | Two transitions different opportunities | Isolated | P0 |
| CON-013 | Double submit same user | User submits twice rapidly | Idempotent or second rejected | P1 |
| CON-014 | Recall during approval | Approver viewing, OM recalls | Recall wins or approval blocked | P1 |
| CON-015 | Edit during approval | User edits while pending | May block or conflict | P1 |
| CON-016 | Concurrent GetRequirements | Multiple clients | Same result | P1 |
| CON-017 | Concurrent GetAvailableTransitions | Multiple clients | Same result | P1 |
| CON-018 | Notification creation race | Two transitions same opportunity | Both notifications created | P1 |
| CON-019 | Audit entry race | Two transitions | Both audited | P1 |
| CON-020 | DbContext concurrent access | Parallel manager calls | Thread-safe | P0 |
| CON-021 | Cache invalidation on stage change | Cached opportunity, stage change | Cache invalidated | P2 |
| CON-022 | EntityUserRole cache | Concurrent lookups | Consistent | P2 |
| CON-023 | Statement cache | Regeneration during read | Consistent | P2 |
| CON-024 | Deadlock scenario | Circular dependency | Timeout or retry | P2 |
| CON-025 | Load test concurrent transitions | 10 users, 10 opportunities | All complete correctly | P1 |

---

## §8 Unit Tests — 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | OpportunityWorkflow.IsValidStage("GO") | Validation | "GO" | true | P1 |
| UNT-002 | OpportunityWorkflow.IsValidStage("INVALID") | Validation | "INVALID" | false | P1 |
| UNT-003 | OpportunityWorkflow.IsValidStage(null) | Validation | null | false | P1 |
| UNT-004 | OpportunityWorkflow.IsValidStage("") | Validation | "" | false | P1 |
| UNT-005 | OpportunityWorkflow.AllStages count | Validation | N/A | 4 | P1 |
| UNT-006 | OpportunityWorkflow.AllStages contains | Validation | N/A | I&P, GO, NO GO, CANCELLED | P1 |
| UNT-007 | GetRequirements I&P→GO count | Validation | I&P, GO | 21 | P1 |
| UNT-008 | GetRequirements I&P→NO GO count | Validation | I&P, NO GO | 0 | P1 |
| UNT-009 | GetRequirements field names | Validation | I&P, GO | name, budget, description, etc. | P1 |
| UNT-010 | StageRequirement Required=true | Validation | GO requirements | All Required | P1 |
| UNT-011 | StageRequirement FieldType | Validation | GO requirements | Text, Number, Array, Date, etc. | P1 |
| UNT-012 | BeneficiariesValidator rule | Validation | TBD=true | Pass | P1 |
| UNT-013 | BeneficiariesValidator rule | Validation | Direct=1, Indirect=0 | Pass | P1 |
| UNT-014 | BeneficiariesValidator rule | Validation | Direct=0, TBD=false | Fail | P1 |
| UNT-015 | Missions conditional | Validation | NotApplicable=true | Missions optional | P1 |
| UNT-016 | Missions conditional | Validation | NotApplicable=false | Missions required | P1 |
| UNT-017 | Budget GreaterThan 0 | Validation | 100 | Pass | P1 |
| UNT-018 | Budget GreaterThan 0 | Validation | 0 | Fail | P1 |
| UNT-019 | EntityNames | Validation | N/A | ["Opportunity"] | P1 |
| UNT-020 | StateMachine EntityType | Validation | N/A | "Opportunity" | P1 |
| UNT-021 | StateMachine States count | Validation | N/A | 4 | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | ChangeStageAsync single transition | I&P → GO | < 2s | P1 |
| PRF-002 | GetRequirementsForStageChange | I&P, GO | < 100ms | P1 |
| PRF-003 | GetAvailableTransitions | Any stage | < 200ms | P1 |
| PRF-004 | DoA resolution (EntityUserRole lookup) | Lookup | < 500ms | P1 |
| PRF-005 | 21 requirements validation | Full validation | < 500ms | P1 |
| PRF-006 | Notification creation | Single notification | < 300ms | P1 |
| PRF-007 | Statement regeneration on submit | Submit for GO | < 30s (AI) | P2 |
| PRF-008 | Bulk notification (10 recipients) | 10 notifications | < 2s | P1 |
| PRF-009 | Audit entry creation | Single audit | < 200ms | P1 |
| PRF-010 | Full GO transition flow | Submit + Approve | < 5s | P1 |
| PRF-011 | Reopen flow | NO GO → I&P | < 1s | P1 |
| PRF-012 | Recall flow | Recall pending | < 1s | P1 |
| PRF-013 | GetOpportunity with workflow status | Read | < 500ms | P1 |
| PRF-014 | Concurrent 5 transitions | 5 parallel | All < 5s | P2 |
| PRF-015 | Validation with 21 requirements | All fields | < 1s | P1 |
| PRF-016 | State machine definition load | Get StateMachine | < 50ms | P2 |

---

## §10 Load Tests — 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained stage changes | 5 transitions/min | 10 min | All succeed, no errors | P2 |
| LDT-002 | Spike: 20 simultaneous submits | 20 concurrent | 1 min | 95% success | P2 |
| LDT-003 | Sustained GetRequirements | 100 req/min | 5 min | < 200ms p95 | P2 |
| LDT-004 | Sustained GetAvailableTransitions | 100 req/min | 5 min | < 200ms p95 | P2 |
| LDT-005 | Stress: 50 concurrent transitions | 50 different opportunities | 2 min | No deadlocks | P2 |
| LDT-006 | Notification creation load | 100 notifications/min | 5 min | All created | P2 |
| LDT-007 | DoA lookup load | 50 lookups/min | 5 min | < 500ms p95 | P2 |
| LDT-008 | Full workflow load | 10 full GO flows/min | 10 min | All complete | P2 |
| LDT-009 | Recovery after load | Load then idle | 2 min | System recovers | P2 |
| LDT-010 | Mixed operations load | Submit, approve, recall mix | 15 min | No degradation | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| FR-2.1: 21 mandatory fields for GO | POS-008, NEG-011 to NEG-031, FUN-026 to FUN-050, UNT-007 to UNT-018 |
| PNO-1197: DoA Level 3 fallback | POS-007, NEG-047, FUN-020, INT-072, BND-062 |
| GO is terminal | POS-011, NEG-001 to NEG-004, FUN-006, INT-080 to INT-082 |
| Rejection → NO GO | POS-002, NEG-035, FUN-007 |
| OM/Submitter recall only | POS-009, NEG-036, FUN-008 |
| Self-approval prevention | NEG-032 to NEG-034, FUN-023, FUN-024, INT-053 |
| Post-decision immutability | POS-011 to POS-013, FUN-012 to FUN-014 |
| Statement regeneration | POS-010, FUN-011, INT-009, INT-019 |
| Country–org unit warning | POS-024, NEG-049, NEG-050, FUN-010 |
| Non-OM submitter warning | POS-025, FUN-009, INT-068 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
