# Opportunity Tests - Advanced Coverage

**Component:** Advanced Test Scenarios  
**Test Count:** 120+  
**Priority:** P1-P2  
**Created:** January 13, 2026

---

## Overview

Comprehensive coverage of negative tests, integration tests, boundary/limits tests, and edge cases for all Opportunity features.

---

## Test Categories

| Category | Test Count | Priority | Status |
|----------|------------|----------|--------|
| Negative Tests | 45 | P1 | ✅ New |
| Integration Tests | 35 | P1 | ✅ New |
| Boundary & Limits Tests | 25 | P1 | ✅ New |
| Edge Cases | 15 | P2 | ✅ New |
| **Total** | **120** | | |

---

## 1. Negative Tests (45 tests)

### 1.1 Opportunity Manager Negative Tests (10 tests)

#### TC-OPP-NEG-OM-001: Create Opportunity with SQL Injection Attempt
**Priority:** P1  
**Category:** Security - Negative

**Test Steps:**
1. Attempt to create opportunity with Name: `"Robert'; DROP TABLE Opportunities;--"`
2. Verify SQL injection prevented

**Expected Results:**
- Input sanitized
- No SQL injection executed
- BusinessException or validation error
- Opportunity not created
- Security audit log entry

---

#### TC-OPP-NEG-OM-002: Create Opportunity with XSS Script in Description
**Priority:** P1  
**Category:** Security - Negative

**Test Steps:**
1. Attempt to create opportunity with Description: `<script>alert('XSS')</script>`
2. Verify XSS prevented

**Expected Results:**
- Script tags escaped or removed
- Content sanitized
- No script execution
- Safe storage in database

---

#### TC-OPP-NEG-OM-003: Update Opportunity with Negative Budget
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Attempt to update EstimatedValue = -1000000
2. Verify rejection

**Expected Results:**
- BusinessException thrown
- Error: "Budget must be positive"
- Value not updated
- Audit trail shows attempt

---

#### TC-OPP-NEG-OM-004: Create Opportunity with Extremely Long Name
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Create opportunity with Name = 5000 characters
2. Verify validation

**Expected Results:**
- Validation error if exceeds max length (e.g., 500 chars)
- Clear error message
- Name truncation not allowed
- Must meet length requirements

---

#### TC-OPP-NEG-OM-005: Delete Non-Existent Opportunity
**Priority:** P1  
**Category:** Error Handling - Negative

**Test Steps:**
1. Call DeleteAsync(99999999)
2. Verify graceful handling

**Expected Results:**
- KeyNotFoundException thrown
- Clear error message
- No database corruption
- Proper error code returned

---

#### TC-OPP-NEG-OM-006: Update Opportunity with Invalid Status Enum
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Attempt to set Status = "InvalidStatus"
2. Verify rejection

**Expected Results:**
- BusinessException thrown
- Error lists valid statuses
- Status unchanged
- Enum validation enforced

---

#### TC-OPP-NEG-OM-007: Create Opportunity with NULL Required Fields
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Create opportunity with Name = null, EstimatedValue = null
2. Verify null handling

**Expected Results:**
- ArgumentNullException or BusinessException
- Each null field identified
- Clear error messages
- No partial creation

---

#### TC-OPP-NEG-OM-008: Convert Draft Opportunity to Project
**Priority:** P1  
**Category:** Business Rule - Negative

**Test Steps:**
1. Attempt to convert opportunity with Status = "Draft"
2. Verify rejection

**Expected Results:**
- BusinessException: "Must be approved first"
- Conversion blocked
- Opportunity status unchanged
- No project entity created

---

#### TC-OPP-NEG-OM-009: Circular Partnership Reference
**Priority:** P2  
**Category:** Data Integrity - Negative

**Test Steps:**
1. Create Partner A linked to Opportunity 1
2. Attempt to link Opportunity 1 to Partner A with parent reference creating circular dependency
3. Verify prevention

**Expected Results:**
- Circular reference detected
- BusinessException thrown
- Relationship not created
- Data integrity maintained

---

#### TC-OPP-NEG-OM-010: Update Opportunity After Soft Delete
**Priority:** P1  
**Category:** Business Rule - Negative

**Test Steps:**
1. Soft delete opportunity (IsDeleted = true)
2. Attempt to update opportunity
3. Verify blocked

**Expected Results:**
- BusinessException: "Cannot modify deleted opportunity"
- Update rejected
- Data unchanged
- Must undelete first

---

### 1.2 DST Manager Negative Tests (10 tests)

#### TC-OPP-NEG-DST-001: Generate Profile for Non-Existent Opportunity
**Priority:** P1  
**Category:** Error Handling - Negative

**Test Steps:**
1. Call GenerateDSTProfileAsync(999999)
2. Verify error handling

**Expected Results:**
- KeyNotFoundException thrown
- Clear error message
- No partial profile created
- No database corruption

---

#### TC-OPP-NEG-DST-002: Generate Profile with All Parameters Missing
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Opportunity has no country, budget, partners, deliverables
2. Attempt profile generation
3. Verify handling

**Expected Results:**
- Profile generated with warnings
- All parameters marked "Insufficient Data"
- Completeness score = 0%
- User prompted to complete data

---

#### TC-OPP-NEG-DST-003: Calculate Complexity with Corrupted Data
**Priority:** P1  
**Category:** Error Handling - Negative

**Test Steps:**
1. Opportunity has invalid numeric values (NaN, Infinity)
2. Attempt complexity calculation
3. Verify error handling

**Expected Results:**
- Invalid values detected
- BusinessException thrown
- Calculation aborted
- Error logged with details

---

#### TC-OPP-NEG-DST-004: Accept Recommendation for Wrong Opportunity
**Priority:** P1  
**Category:** Authorization - Negative

**Test Steps:**
1. Recommendation belongs to Opportunity A
2. User attempts to accept for Opportunity B
3. Verify rejection

**Expected Results:**
- BusinessException: "Recommendation mismatch"
- Action blocked
- Audit log entry
- Data integrity maintained

---

#### TC-OPP-NEG-DST-005: Generate Profile During Concurrent Update
**Priority:** P1  
**Category:** Concurrency - Negative

**Test Steps:**
1. Start DST profile generation
2. Simultaneously update opportunity budget
3. Verify handling

**Expected Results:**
- Optimistic concurrency check
- Profile uses consistent snapshot
- Or regeneration triggered
- No data corruption

---

#### TC-OPP-NEG-DST-006: Reject Recommendation Without Reason
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Call RejectRecommendationAsync(id, reason: null)
2. Verify validation

**Expected Results:**
- BusinessException: "Reason required"
- Rejection blocked
- Status unchanged
- Minimum reason length enforced

---

#### TC-OPP-NEG-DST-007: Parameter Score Outside Valid Range
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Attempt to set parameter score = 150 (max is 100)
2. Verify validation

**Expected Results:**
- ArgumentOutOfRangeException
- Score must be 0-100
- Value not saved
- Clear error message

---

#### TC-OPP-NEG-DST-008: AI Service Timeout
**Priority:** P1  
**Category:** External Dependency - Negative

**Test Steps:**
1. Mock AI service with 60 second delay
2. Generate profile
3. Verify timeout handling

**Expected Results:**
- Timeout after 30 seconds
- Graceful degradation
- Fallback to historical averages
- User notified
- Retry option provided

---

#### TC-OPP-NEG-DST-009: Similar Projects Query with No Historical Data
**Priority:** P2  
**Category:** Edge Case - Negative

**Test Steps:**
1. New system with no historical opportunities
2. Search for similar projects
3. Verify handling

**Expected Results:**
- Empty result set returned
- User message: "No historical data available"
- No error thrown
- Graceful handling

---

#### TC-OPP-NEG-DST-010: Profile Report Generation with Missing Templates
**Priority:** P1  
**Category:** Configuration - Negative

**Test Steps:**
1. Delete/corrupt PDF templates
2. Attempt report generation
3. Verify error handling

**Expected Results:**
- FileNotFoundException or ConfigurationException
- Clear error message
- Admin notified
- Fallback to basic format

---

### 1.3 Decision Manager Negative Tests (10 tests)

#### TC-OPP-NEG-DEC-001: Record Decision Without Assembled Package
**Priority:** P1  
**Category:** Business Rule - Negative

**Test Steps:**
1. Attempt to record Go decision without decision package
2. Verify rejection

**Expected Results:**
- BusinessException: "Decision package not assembled"
- Decision blocked
- No authorization granted
- Clear process guidance

---

#### TC-OPP-NEG-DEC-002: Decision by User with Expired DOA
**Priority:** P1  
**Category:** Authorization - Negative

**Test Steps:**
1. User's DOA authority expired yesterday
2. Attempt to make decision
3. Verify rejection

**Expected Results:**
- UnauthorizedAccessException
- Error: "DOA authority expired"
- Decision not recorded
- Must renew authority

---

#### TC-OPP-NEG-DEC-003: Delegate to Lower Authority
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. DOA2 ($5M) attempts to delegate to DOA4 ($100K)
2. Verify rejection

**Expected Results:**
- BusinessException: "Cannot delegate to lower authority"
- Delegation blocked
- Clear error message
- Can only delegate equal or up

---

#### TC-OPP-NEG-DEC-004: Authorize Budget Twice
**Priority:** P1  
**Category:** Data Integrity - Negative

**Test Steps:**
1. Authorize budget for opportunity
2. Attempt to authorize again
3. Verify idempotency

**Expected Results:**
- Second authorization ignored or error
- No duplicate authorization
- Audit trail shows attempt
- Data integrity maintained

---

#### TC-OPP-NEG-DEC-005: Revoke Authorization Without Reason
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Attempt RevokeAuthorizationAsync(id, reason: null)
2. Verify validation

**Expected Results:**
- BusinessException: "Justification required"
- Revocation blocked
- Authorization unchanged
- Audit requirement enforced

---

#### TC-OPP-NEG-DEC-006: Decision on Already Decided Opportunity
**Priority:** P1  
**Category:** Business Rule - Negative

**Test Steps:**
1. Opportunity has Go decision from yesterday
2. Attempt new No-Go decision
3. Verify handling

**Expected Results:**
- BusinessException: "Already decided"
- New decision blocked
- Or marked as "Decision Update" with justification
- Original decision preserved

---

#### TC-OPP-NEG-DEC-007: Condition Fulfillment by Unauthorized User
**Priority:** P1  
**Category:** Authorization - Negative

**Test Steps:**
1. Mark condition as complete by user not assigned
2. Verify rejection

**Expected Results:**
- UnauthorizedAccessException
- Only assigned user can mark complete
- Status unchanged
- Audit log entry

---

#### TC-OPP-NEG-DEC-008: Escalate to Non-Existent User
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Call EscalateDecisionAsync with userId = 999999
2. Verify validation

**Expected Results:**
- BusinessException: "User not found"
- Escalation blocked
- Valid users suggested
- Clear error message

---

#### TC-OPP-NEG-DEC-009: Decision Package with Expired Documents
**Priority:** P2  
**Category:** Business Rule - Negative

**Test Steps:**
1. DST profile generated 90 days ago
2. Attempt to assemble decision package
3. Verify validation

**Expected Results:**
- Warning: "DST profile may be outdated"
- Option to regenerate
- Can proceed with acknowledgment
- Timestamp warnings

---

#### TC-OPP-NEG-DEC-010: Conditional Go with Empty Conditions Array
**Priority:** P1  
**Category:** Validation - Negative

**Test Steps:**
1. Record "Go with Conditions" but conditions = []
2. Verify validation

**Expected Results:**
- BusinessException: "Must specify conditions"
- Decision blocked
- At least 1 condition required
- Clear error message

---

### 1.4 Budget Manager Negative Tests (5 tests)

#### TC-OPP-NEG-BUD-001: Generate Budget with Zero Deliverables
**Priority:** P1  
**Category:** Validation - Negative

**Expected Results:**
- BusinessException: "No deliverables defined"
- Budget generation blocked
- User prompted to add deliverables
- Cannot proceed without scope

---

#### TC-OPP-NEG-BUD-002: Apply Fee Percentage > 100%
**Priority:** P1  
**Category:** Validation - Negative

**Expected Results:**
- BusinessException: "Fee cannot exceed 100%"
- Invalid fee rejected
- Reasonable range enforced (0-50%)
- Clear validation message

---

#### TC-OPP-NEG-BUD-003: Budget with Mismatched Currency Totals
**Priority:** P1  
**Category:** Data Integrity - Negative

**Expected Results:**
- Validation error
- Currency mismatch detected
- Must convert or reconcile
- Cannot save inconsistent data

---

#### TC-OPP-NEG-BUD-004: Update Budget After Authorization
**Priority:** P1  
**Category:** Business Rule - Negative

**Expected Results:**
- BusinessException: "Budget is authorized"
- Update blocked
- Must revoke authorization first
- Or create amendment request

---

#### TC-OPP-NEG-BUD-005: Budget Exceeds Agreement Cap
**Priority:** P1  
**Category:** Business Rule - Negative

**Expected Results:**
- BusinessException with agreement cap
- Budget blocked
- Must adjust or seek amendment
- Clear comparison shown

---

### 1.5 Document Extraction Negative Tests (5 tests)

#### TC-OPP-NEG-DOC-001: Upload Malicious PDF with Embedded Scripts
**Priority:** P1  
**Category:** Security - Negative

**Expected Results:**
- Malicious content detected
- Upload rejected
- Security scan triggered
- Incident logged

---

#### TC-OPP-NEG-DOC-002: Extract from Completely Corrupted PDF
**Priority:** P1  
**Category:** Error Handling - Negative

**Expected Results:**
- Extraction fails gracefully
- Clear error message
- User can re-upload
- No system crash

---

#### TC-OPP-NEG-DOC-003: AI Service Returns Malformed JSON
**Priority:** P1  
**Category:** External Dependency - Negative

**Expected Results:**
- JSON parsing exception handled
- Retry attempted
- Fallback to manual entry
- Error logged for investigation

---

#### TC-OPP-NEG-DOC-004: Upload Exceeds File Size Limit
**Priority:** P1  
**Category:** Validation - Negative

**Expected Results:**
- File rejected at upload
- Clear size limit shown (e.g., 10MB)
- Suggestion to compress
- No partial upload

---

#### TC-OPP-NEG-DOC-005: Extract with Unsupported Language
**Priority:** P2  
**Category:** Feature Limitation - Negative

**Expected Results:**
- Language detection
- Warning: "Translation not supported"
- Option to proceed without extraction
- Or request translation service

---

### 1.6 Workflow Negative Tests (5 tests)

#### TC-OPP-NEG-WF-001: Trigger Invalid Workflow Transition
**Priority:** P1  
**Category:** Business Rule - Negative

**Expected Results:**
- Invalid transition detected
- StateTransitionException thrown
- Valid transitions shown
- State unchanged

---

#### TC-OPP-NEG-WF-002: Submit Without Required Prerequisites
**Priority:** P1  
**Category:** Validation - Negative

**Expected Results:**
- Prerequisite check fails
- Submission blocked
- Missing items listed
- Clear guidance provided

---

#### TC-OPP-NEG-WF-003: Escalate Without Valid Reason
**Priority:** P1  
**Category:** Validation - Negative

**Expected Results:**
- Reason required and validated
- Minimum length enforced
- Escalation blocked
- Clear requirements

---

#### TC-OPP-NEG-WF-004: Approve with Circular Delegation
**Priority:** P2  
**Category:** Logic Error - Negative

**Expected Results:**
- Circular delegation detected
- BusinessException thrown
- Delegation chain validated
- Cannot approve own submission

---

#### TC-OPP-NEG-WF-005: Notification Send Failure
**Priority:** P1  
**Category:** External Dependency - Negative

**Expected Results:**
- Email service failure handled
- Workflow not blocked
- Notification queued for retry
- In-app notification as fallback

---

## 2. Integration Tests (35 tests)

### 2.1 End-to-End Integration Tests (15 tests)

#### TC-OPP-INT-E2E-001: Complete Opportunity Lifecycle
**Priority:** P0  
**Category:** Integration

**Test Steps:**
1. Create opportunity
2. Upload documents and extract data
3. Generate DST profile
4. Create budget, schedule, resource plan
5. Assemble decision package
6. Record Go decision
7. Authorize budget and personnel
8. Convert to project

**Expected Results:**
- All steps complete successfully
- Data flows correctly between components
- No data loss
- Audit trail complete
- Performance acceptable (<5 minutes)
- Project created with all opportunity data

---

#### TC-OPP-INT-E2E-002: Opportunity with Multi-Country Geography
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Create opportunity spanning 3 countries
2. Each country has different indices
3. Generate DST profile
4. Verify context analysis uses all countries

**Expected Results:**
- All countries considered in DST
- Highest risk country drives overall risk
- Geography correctly reflected in budget
- Resource plan accounts for travel
- Reporting aggregates correctly

---

#### TC-OPP-INT-E2E-003: Partnership Agreement Integration Flow
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Upload partnership agreement
2. Extract terms (geography, fee %)
3. Create opportunity linked to agreement
4. Verify pre-population
5. Generate budget with agreement fee
6. Validate against agreement terms

**Expected Results:**
- Agreement terms applied correctly
- Fee from agreement used (8% not 10% default)
- Geography validated
- Scope checked
- No validation errors

---

#### TC-OPP-INT-E2E-004: AI-Assisted Opportunity Creation
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Upload concept note PDF
2. AI extracts all fields
3. User reviews and accepts
4. AI suggests SDGs
5. Generate DST profile
6. AI recommends personnel

**Expected Results:**
- 80%+ fields auto-populated
- SDG suggestions accurate
- DST incorporates extracted data
- Personnel recommendations relevant
- Significant time saving

---

#### TC-OPP-INT-E2E-005: Rejected Decision Recovery Flow
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Submit opportunity for decision
2. DOA holder rejects with feedback
3. Opportunity manager updates
4. Regenerates DST
5. Resubmits for decision
6. Approved

**Expected Results:**
- All feedback addressed
- Version history maintained
- DST shows improvements
- Second submission cleaner
- Approval recorded

---

#### TC-OPP-INT-E2E-006: Concurrent Multi-User Collaboration
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. User A edits opportunity details
2. User B works on budget
3. User C generates DST profile
4. All save simultaneously

**Expected Results:**
- No data loss
- Optimistic concurrency handled
- Users notified of conflicts
- Can merge changes
- Audit trail accurate

---

#### TC-OPP-INT-E2E-007: Global Indices Update Cascade
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Upload new MVI data for all countries
2. Existing opportunities in those countries
3. Trigger DST regeneration
4. Verify updates cascade

**Expected Results:**
- All country records updated
- Existing DST profiles flagged as outdated
- Option to regenerate
- Historical "as-at" views preserved
- No data corruption

---

#### TC-OPP-INT-E2E-008: Budget-Schedule-Resource Alignment
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Generate budget (personnel + non-personnel)
2. Generate schedule with phases
3. Generate resource plan
4. Verify alignment

**Expected Results:**
- Personnel costs match resource plan FTEs
- Schedule phases align with budget phasing
- Spend rate realistic
- No contradictions
- Integrated view available

---

#### TC-OPP-INT-E2E-009: Risk Register Integration
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. DST recommends 5 risks
2. Accept 3 risks to register
3. Add 2 manual risks
4. DST references in Go decision
5. Risks transfer to project

**Expected Results:**
- All risks in single register
- Source tracked (DST vs manual)
- Referenced in decision package
- Transfer to project seamless
- Historical context preserved

---

#### TC-OPP-INT-E2E-010: External System Integration
**Priority:** P2  
**Category:** Integration

**Test Steps:**
1. Opportunity approved
2. Sync to ERP system
3. Create project in PM tool
4. Notify finance system
5. Update reporting dashboard

**Expected Results:**
- All systems notified
- Data mapping correct
- Async processing
- Retry on failures
- Complete integration

---

#### TC-OPP-INT-E2E-011: Mobile Access During Development
**Priority:** P2  
**Category:** Integration

**Test Steps:**
1. User starts opportunity on desktop
2. Reviews DST on mobile
3. Approves decision on tablet
4. Verify cross-device sync

**Expected Results:**
- Data syncs across devices
- UI responsive
- No data loss
- Session management works
- Optimized for mobile

---

#### TC-OPP-INT-E2E-012: Bulk Opportunity Import
**Priority:** P2  
**Category:** Integration

**Test Steps:**
1. Import 100 opportunities from Excel
2. Trigger batch DST generation
3. Verify all created correctly

**Expected Results:**
- All opportunities imported
- Validation errors reported
- Batch processing efficient
- Progress tracking
- Rollback on critical errors

---

#### TC-OPP-INT-E2E-013: Report Generation Across Components
**Priority:** P2  
**Category:** Integration

**Test Steps:**
1. Generate comprehensive opportunity report
2. Include: details, DST, budget, schedule, risks, decisions
3. Verify data completeness

**Expected Results:**
- All sections populated
- Data consistent
- Charts render correctly
- PDF <5MB
- Generation <30 seconds

---

#### TC-OPP-INT-E2E-014: Audit Trail Across All Components
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. Complete opportunity lifecycle
2. Query complete audit trail
3. Verify all actions logged

**Expected Results:**
- Every action recorded
- Who, what, when, where captured
- Chronological order
- Can reconstruct entire history
- Suitable for compliance

---

#### TC-OPP-INT-E2E-015: Disaster Recovery Scenario
**Priority:** P2  
**Category:** Integration

**Test Steps:**
1. Opportunity at decision stage
2. Simulate system crash
3. System recovers
4. Verify data integrity

**Expected Results:**
- No data loss
- In-flight transactions rolled back
- Can resume workflow
- Users notified
- Audit trail accurate

---

### 2.2 Cross-Manager Integration Tests (10 tests)

#### TC-OPP-INT-MGR-001: Opportunity-DST-Decision Integration
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. OpportunityManager creates opportunity
2. DSTManager generates profile
3. DecisionManager uses profile in decision package

**Expected Results:**
- Seamless data flow
- No manual copying
- Profile automatically included
- Updates reflected immediately

---

#### TC-OPP-INT-MGR-002: Budget-Schedule Coordination
**Priority:** P1  
**Category:** Integration

**Test Steps:**
1. BudgetManager generates budget with phases
2. ScheduleManager generates schedule
3. Verify phases align

**Expected Results:**
- Phase dates match
- Budget phasing realistic
- Changes propagate
- Integrated timeline view

---

#### TC-OPP-INT-MGR-003: Resource-Budget Integration
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- Personnel costs calculated from resource plan
- FTE rates applied correctly
- Total personnel budget matches
- Changes sync both ways

---

#### TC-OPP-INT-MGR-004: Risk-DST Integration
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- DST-identified risks flow to RiskManager
- Risk register feeds back to DST
- Risk scores influence complexity
- Mitigation tracked

---

#### TC-OPP-INT-MGR-005: Agreement-Budget Integration
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- Agreement fee % applied to budget
- Geography validated
- Value caps enforced
- Utilization tracked

---

#### TC-OPP-INT-MGR-006: Document-Opportunity Integration
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- Extracted data populates opportunity
- Documents linked
- Version control maintained
- Can re-extract on document update

---

#### TC-OPP-INT-MGR-007: Workflow-Decision Integration
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- Workflow triggers decision package assembly
- Decision updates workflow state
- Approvals tracked
- State transitions logged

---

#### TC-OPP-INT-MGR-008: GlobalIndices-DST Integration
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- DST uses latest indices
- Country data influences context parameter
- Updates trigger profile refresh option
- Historical comparison available

---

#### TC-OPP-INT-MGR-009: Opportunity-Project Conversion
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- All opportunity data transferred
- Budget becomes project budget
- Schedule becomes project schedule
- Team assignments preserved
- Linkage maintained

---

#### TC-OPP-INT-MGR-010: Multi-Manager Transaction
**Priority:** P1  
**Category:** Integration

**Expected Results:**
- All changes committed or rolled back
- No partial updates
- Data consistency maintained
- Performance acceptable

---

### 2.3 External Service Integration Tests (10 tests)

#### TC-OPP-INT-EXT-001: Gemini AI Service Integration
**Priority:** P1  
**Category:** External Integration

**Expected Results:**
- API calls successful
- Rate limiting respected
- Responses parsed correctly
- Timeout handling
- Fallback on failure

---

#### TC-OPP-INT-EXT-002: Email Notification Service
**Priority:** P1  
**Category:** External Integration

**Expected Results:**
- Emails sent correctly
- Templates applied
- Links work
- Bounce handling
- Queue management

---

#### TC-OPP-INT-EXT-003: Document Storage (GCS)
**Priority:** P1  
**Category:** External Integration

**Expected Results:**
- Files uploaded successfully
- Proper folder structure
- Access control
- Versioning
- Retrieval works

---

#### TC-OPP-INT-EXT-004: ERP System Integration
**Priority:** P2  
**Category:** External Integration

**Expected Results:**
- Data sync successful
- Mapping correct
- Async processing
- Error handling
- Retry mechanism

---

#### TC-OPP-INT-EXT-005: Authentication Service
**Priority:** P1  
**Category:** External Integration

**Expected Results:**
- SSO works
- Tokens validated
- Permissions retrieved
- Session management
- Logout handled

---

#### TC-OPP-INT-EXT-006: Currency Exchange Service
**Priority:** P2  
**Category:** External Integration

**Expected Results:**
- Exchange rates retrieved
- Conversion accurate
- Historical rates available
- Fallback rates
- Cache strategy

---

#### TC-OPP-INT-EXT-007: Reporting Dashboard
**Priority:** P2  
**Category:** External Integration

**Expected Results:**
- Metrics updated
- Charts render
- Drill-down works
- Real-time updates
- Performance acceptable

---

#### TC-OPP-INT-EXT-008: OCR Service for Scanned Documents
**Priority:** P2  
**Category:** External Integration

**Expected Results:**
- Text extracted accurately
- Confidence scores provided
- Multiple languages supported
- Error handling
- Quality validation

---

#### TC-OPP-INT-EXT-009: Country Profile Data Service
**Priority:** P2  
**Category:** External Integration

**Expected Results:**
- Indices retrieved
- Data validated
- Updates processed
- Historical tracking
- Source attribution

---

#### TC-OPP-INT-EXT-010: Backup and Archive Service
**Priority:** P2  
**Category:** External Integration

**Expected Results:**
- Automated backups
- Point-in-time recovery
- Archive compliance
- Restore tested
- Encryption enforced

---

## 3. Boundary and Limits Tests (25 tests)

### 3.1 Data Volume Limits (10 tests)

#### TC-OPP-BND-VOL-001: Maximum Name Length
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Create opportunity with Name = exactly 500 characters
2. Verify accepted
3. Try 501 characters
4. Verify rejected

**Expected Results:**
- 500 chars: Accepted
- 501 chars: Validation error
- Clear limit communicated
- Truncation not allowed

---

#### TC-OPP-BND-VOL-002: Minimum Budget Value
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Create opportunity with EstimatedValue = $1
2. Verify accepted (edge case)
3. Try $0
4. Verify rejected

**Expected Results:**
- $1: Accepted (minimum viable)
- $0: Rejected (must be positive)
- Clear minimum communicated

---

#### TC-OPP-BND-VOL-003: Maximum Budget Value
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Create opportunity with EstimatedValue = $999,999,999,999
2. Verify handling
3. Check data type limits

**Expected Results:**
- Decimal precision maintained
- No overflow errors
- Display formatting correct
- Database storage adequate

---

#### TC-OPP-BND-VOL-004: Maximum Deliverables Count
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Create opportunity with 100 deliverables
2. Verify performance
3. Try 101
4. Check if limit enforced

**Expected Results:**
- 100: Accepted (if no limit)
- Or clear limit enforced (e.g., 50)
- Performance acceptable
- UI usable

---

#### TC-OPP-BND-VOL-005: Maximum Partners Count
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Add 20 partners to opportunity
2. Verify handling
3. Check performance

**Expected Results:**
- All partners stored correctly
- Display handles pagination
- Performance acceptable
- Reasonable limit enforced

---

#### TC-OPP-BND-VOL-006: Maximum Document Upload Size
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Upload 9.9MB PDF (under 10MB limit)
2. Verify success
3. Upload 10.1MB PDF
4. Verify rejection

**Expected Results:**
- 9.9MB: Accepted
- 10.1MB: Rejected
- Clear limit shown
- Suggestion to compress

---

#### TC-OPP-BND-VOL-007: Maximum Countries Count
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Opportunity spanning 50 countries
2. Verify handling
3. DST performance

**Expected Results:**
- All countries stored
- DST analyzes all efficiently
- Performance acceptable
- UI handles many countries

---

#### TC-OPP-BND-VOL-008: Maximum Risk Register Entries
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Add 500 risks to register
2. Verify performance

**Expected Results:**
- All risks stored
- Pagination works
- Search/filter efficient
- Report generation viable

---

#### TC-OPP-BND-VOL-009: Maximum Timeline Duration
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Create opportunity with 20-year duration
2. Verify schedule generation
3. Check calculations

**Expected Results:**
- Long duration handled
- Schedule generates correctly
- Budget phasing reasonable
- No calculation errors

---

#### TC-OPP-BND-VOL-010: Concurrent Users Limit
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Simulate 1000 concurrent users
2. All accessing opportunities
3. Measure performance

**Expected Results:**
- System remains responsive
- Response time <2 seconds
- No crashes
- Resource usage acceptable
- Queue management if needed

---

### 3.2 Numeric Boundaries (10 tests)

#### TC-OPP-BND-NUM-001: Complexity Score at Boundaries
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Generate profile with score = 0.0
2. Verify: 0.0 (minimum)
3. Generate profile with score = 10.0
4. Verify: 10.0 (maximum)

**Expected Results:**
- Both extremes handled
- Display correct
- Classifications accurate
- No rounding errors

---

#### TC-OPP-BND-NUM-002: Risk Score at Boundaries
**Priority:** P1  
**Category:** Boundary

**Expected Results:**
- 0 = No risk (theoretical)
- 10 = Maximum risk
- Proper categorization
- Color coding correct

---

#### TC-OPP-BND-NUM-003: Fee Percentage Boundaries
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Apply fee = 0%
2. Verify: Fee = $0
3. Apply fee = 50% (max reasonable)
4. Verify calculation

**Expected Results:**
- 0%: $0 fee (allowed)
- 50%: Calculated correctly
- Validation on excessive fees
- Clear limits

---

#### TC-OPP-BND-NUM-004: DOA Authority Limits
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Opportunity = exactly $1,000,000
2. DOA3 limit = $1,000,000
3. Verify authorization

**Expected Results:**
- Exact match: Authorized
- Just over: Escalation required
- Boundary clear
- No off-by-one errors

---

#### TC-OPP-BND-NUM-005: Parameter Score Precision
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Parameter score = 45.678901234
2. Verify decimal handling

**Expected Results:**
- Precision maintained (2-3 decimal places)
- Rounding correct
- Display truncated appropriately
- Database stores full precision

---

#### TC-OPP-BND-NUM-006: Budget Decimal Precision
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. Budget = $2,500,000.9999
2. Verify decimal handling

**Expected Results:**
- Currency precision (2 decimals)
- Rounding applied correctly
- No floating point errors
- Totals accurate

---

#### TC-OPP-BND-NUM-007: Negative Date Boundaries
**Priority:** P1  
**Category:** Boundary

**Test Steps:**
1. StartDate = today
2. EndDate = today (0 duration)
3. Verify handling

**Expected Results:**
- 0 duration: Warning but allowed
- Or minimum 1 day enforced
- Validation clear
- Business rule defined

---

#### TC-OPP-BND-NUM-008: Far Future Dates
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. EndDate = 2099-12-31
2. Verify handling

**Expected Results:**
- Date accepted
- Calculations work
- No year 2038 issues
- Reasonable warning shown

---

#### TC-OPP-BND-NUM-009: Version Number Overflow
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. DST profile regenerated 1000 times
2. Verify version tracking

**Expected Results:**
- Version numbers continue
- No integer overflow
- History maintained
- Performance acceptable

---

#### TC-OPP-BND-NUM-010: Pagination Boundaries
**Priority:** P2  
**Category:** Boundary

**Test Steps:**
1. Page = 1, Size = 1 (minimum)
2. Verify results
3. Page = 100, Size = 100 (large)
4. Verify performance

**Expected Results:**
- Size = 1: Works correctly
- Size = 100: Performance OK
- Reasonable max size enforced
- Clear boundaries

---

### 3.3 Time Boundaries (5 tests)

#### TC-OPP-BND-TIME-001: Same-Day Opportunity Creation and Decision
**Priority:** P2  
**Category:** Boundary

**Expected Results:**
- All steps complete in hours
- Fast-track indicators
- Timestamps accurate
- Audit trail complete

---

#### TC-OPP-BND-TIME-002: Decision Deadline at Midnight
**Priority:** P2  
**Category:** Boundary

**Expected Results:**
- Deadline = today 23:59:59
- After midnight = overdue
- Timezone handling correct
- Notifications timed properly

---

#### TC-OPP-BND-TIME-003: Concurrent Timestamp Conflicts
**Priority:** P1  
**Category:** Boundary

**Expected Results:**
- Microsecond precision
- No timestamp collisions
- Proper ordering maintained
- Race conditions prevented

---

#### TC-OPP-BND-TIME-004: Authorization Expiration at Exact Time
**Priority:** P2  
**Category:** Boundary

**Expected Results:**
- Expires at exact timestamp
- Check runs periodically
- Status updated promptly
- Users notified

---

#### TC-OPP-BND-TIME-005: Historical Data Query Boundaries
**Priority:** P2  
**Category:** Boundary

**Expected Results:**
- Earliest date in system
- Latest date = now
- "As-at" queries work
- No data outside range

---

## 4. Edge Cases (15 tests)

### 4.1 Data Edge Cases (8 tests)

#### TC-OPP-EDGE-001: Opportunity Name with Special Characters
**Priority:** P1  
**Category:** Edge Case

**Test Steps:**
1. Name = "Water & Sanitation (Phase II) – 50% Match!"
2. Verify handling

**Expected Results:**
- Special chars stored correctly
- Display renders properly
- Search works
- Export handles encoding
- No injection vulnerabilities

---

#### TC-OPP-EDGE-002: Multi-Language Text in Description
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. Description with English, Arabic, Chinese, French
2. Verify storage and display

**Expected Results:**
- UTF-8 encoding correct
- All languages render
- Search works
- PDF export correct
- No character corruption

---

#### TC-OPP-EDGE-003: Opportunity with No Primary Country
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. Global initiative with no specific country
2. Verify DST handling

**Expected Results:**
- Can be marked "Global"
- DST uses regional averages
- Validation allows null country
- Clear handling documented

---

#### TC-OPP-EDGE-004: Partner with Same Name as Another
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. Two partners both named "Development Bank"
2. Link to opportunity
3. Verify disambiguation

**Expected Results:**
- Both partners selectable
- Additional info shown (country, type)
- No confusion
- Correct partner linked

---

#### TC-OPP-EDGE-005: Opportunity Created on Leap Day
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. Create opportunity on Feb 29, 2024
2. Schedule for 1 year
3. Verify date calculation

**Expected Results:**
- EndDate = Feb 28, 2025 (or 29 if leap)
- Date math correct
- No leap year bugs
- Anniversaries handled

---

#### TC-OPP-EDGE-006: Budget with Fractional Currency
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. Currency = JPY (no cents)
2. Budget = ¥2,500,000
3. Verify handling

**Expected Results:**
- No decimal places shown
- Calculations integer-based
- Rounding appropriate
- Display correct

---

#### TC-OPP-EDGE-007: Opportunity with Circular Geography
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. Opportunity in "Disputed Territory"
2. Country assignment unclear
3. Verify handling

**Expected Results:**
- Can be marked "Disputed"
- Regional classification available
- Risk indicators shown
- Clear documentation

---

#### TC-OPP-EDGE-008: DST Profile with All Parameters Equal
**Priority:** P2  
**Category:** Edge Case

**Test Steps:**
1. All 9 parameters = 50
2. Verify profile generation

**Expected Results:**
- Profile generates correctly
- Balanced visualization
- No div-by-zero errors
- Meaningful recommendations

---

### 4.2 Workflow Edge Cases (4 tests)

#### TC-OPP-EDGE-WF-001: Decision Maker Leaves During Review
**Priority:** P1  
**Category:** Edge Case

**Expected Results:**
- User account disabled
- Decision auto-escalates
- New DOA notified
- Process continues
- No orphaned decisions

---

#### TC-OPP-EDGE-WF-002: Opportunity Updated During Decision Review
**Priority:** P1  
**Category:** Edge Case

**Expected Results:**
- DOA sees version being reviewed
- Notification of update
- Can refresh to see latest
- Decision on specific version
- Version tracked

---

#### TC-OPP-EDGE-WF-003: All Approvers on Leave Simultaneously
**Priority:** P2  
**Category:** Edge Case

**Expected Results:**
- Escalation to backup
- Or temporary DOA assigned
- System doesn't block
- Process continuity maintained

---

#### TC-OPP-EDGE-WF-004: Opportunity Recovered After Project Created
**Priority:** P2  
**Category:** Edge Case

**Expected Results:**
- Warning shown
- Project already exists
- Clear conflict resolution
- Data integrity maintained

---

### 4.3 System Edge Cases (3 tests)

#### TC-OPP-EDGE-SYS-001: Database Connection Lost Mid-Transaction
**Priority:** P1  
**Category:** Edge Case

**Expected Results:**
- Transaction rolled back
- User notified
- No data corruption
- Can retry
- Graceful degradation

---

#### TC-OPP-EDGE-SYS-002: Cache and Database Out of Sync
**Priority:** P1  
**Category:** Edge Case

**Expected Results:**
- Cache invalidation works
- Fresh data retrieved
- User sees correct data
- Sync mechanisms function

---

#### TC-OPP-EDGE-SYS-003: Opportunity Accessed After System Upgrade
**Priority:** P1  
**Category:** Edge Case

**Expected Results:**
- Backward compatibility
- Schema migration successful
- Old data accessible
- No feature regression

---

## Summary

**Total Additional Tests:** 120  
**Negative Tests:** 45  
**Integration Tests:** 35  
**Boundary & Limits:** 25  
**Edge Cases:** 15

**Combined with Original:** 445 + 120 = **565 Total Tests**

**Coverage Now Includes:**
- ✅ Functional tests (original)
- ✅ Validation tests (original)
- ✅ **Negative tests (NEW)**
- ✅ **Integration tests (NEW)**
- ✅ **Boundary/limits tests (NEW)**
- ✅ **Edge cases (NEW)**

**Status:** ✅ Comprehensive Coverage Complete

---

**Last Updated:** January 13, 2026  
**Ready for Implementation**
