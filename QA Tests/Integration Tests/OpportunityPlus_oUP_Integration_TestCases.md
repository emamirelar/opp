# Opportunity+ to oUP Integration Test Cases

## Document Information

| Attribute | Value |
|-----------|-------|
| **Document Version** | 1.0 |
| **Created Date** | 2026-02-02 |
| **Last Updated** | 2026-02-02 |
| **Author** | QA Team |
| **Integration Source** | Opportunity+ to oUP Integration Documentation |

---

## Table of Contents

1. [Overview](#overview)
2. [Test Environment Prerequisites](#test-environment-prerequisites)
3. [Integration Flow Test Cases](#integration-flow-test-cases)
4. [Field Mapping Test Cases](#field-mapping-test-cases)
5. [High-Risk Checklist Mapping Test Cases](#high-risk-checklist-mapping-test-cases)
6. [Email Notification Test Cases](#email-notification-test-cases)
7. [Deep Linking Test Cases](#deep-linking-test-cases)
8. [Idempotency Test Cases](#idempotency-test-cases)
9. [Error Handling Test Cases](#error-handling-test-cases)
10. [Performance Test Cases](#performance-test-cases)
11. [Edge Cases](#edge-cases)

---

## Overview

This document contains comprehensive test cases for validating the Opportunity+ to oneUNOPS Projects (oUP) integration. The integration synchronizes opportunity data from Opportunity+ to oUP by creating base engagements in the Pre-engagement stage.

### Key Integration Features to Test

- Data synchronization (19 categories)
- Bi-directional deep linking
- Email notifications
- Idempotent processing
- High-risk mapping

---

## Test Environment Prerequisites

### Required Setup

- [ ] Access to Opportunity+ test environment
- [ ] Access to oUP test environment
- [ ] Valid test user accounts with appropriate permissions
- [ ] Access to email inbox for notification testing
- [ ] Access to Pub/Sub monitoring (optional)
- [ ] Database access for verification (optional)

### Test Data Requirements

- [ ] At least 3 different Funding Partners configured
- [ ] At least 3 different Client Partners configured
- [ ] All 17 high-risk types available for selection
- [ ] Multiple countries available for selection
- [ ] Multiple SDGs configured with targets and indicators
- [ ] Test users with valid email addresses

---

## Integration Flow Test Cases

### INT-001: Basic Integration Flow - Create New Engagement

| Attribute | Value |
|-----------|-------|
| **Test ID** | INT-001 |
| **Priority** | Critical |
| **Type** | Positive |

**Preconditions:**
1. User is logged into Opportunity+
2. No existing engagement linked to the test opportunity

**Test Steps:**
1. Create a new opportunity in Opportunity+
2. Fill in all required fields across all sections
3. Save the opportunity
4. Wait 1-5 minutes for Pub/Sub message processing
5. Navigate to oUP
6. Search for the newly created engagement

**Expected Results:**
- [ ] Base engagement is created in oUP
- [ ] Engagement is in Pre-engagement stage
- [ ] All mapped fields contain correct data
- [ ] Email notification is sent to PE, DoA2, and BD

---

### INT-002: Integration Flow - Update Existing Engagement

| Attribute | Value |
|-----------|-------|
| **Test ID** | INT-002 |
| **Priority** | Critical |
| **Type** | Positive |

**Preconditions:**
1. Opportunity exists with linked engagement in oUP
2. User is logged into Opportunity+

**Test Steps:**
1. Open existing opportunity in Opportunity+
2. Modify several fields (name, description, dates)
3. Save the opportunity
4. Wait 1-5 minutes for processing
5. Navigate to oUP and open the linked engagement

**Expected Results:**
- [ ] Existing engagement is updated (not duplicated)
- [ ] Modified fields reflect new values
- [ ] Engagement remains in same stage
- [ ] Update email notification is sent

---

### INT-003: Integration Trigger on Every Save

| Attribute | Value |
|-----------|-------|
| **Test ID** | INT-003 |
| **Priority** | High |
| **Type** | Positive |

**Preconditions:**
1. New opportunity with no linked engagement

**Test Steps:**
1. Create opportunity with minimal data
2. Save (first save)
3. Verify engagement created in oUP
4. Add more data to opportunity
5. Save (second save)
6. Verify engagement updated in oUP
7. Repeat 2-3 more times

**Expected Results:**
- [ ] Each save triggers sync to oUP
- [ ] First save creates engagement
- [ ] Subsequent saves update engagement
- [ ] No duplicate engagements created

**Notes:** This tests the temporary behavior where sync triggers on every save.

---

### INT-004: Message Transport Latency Verification

| Attribute | Value |
|-----------|-------|
| **Test ID** | INT-004 |
| **Priority** | Medium |
| **Type** | Performance |

**Preconditions:**
1. Stopwatch or timer available

**Test Steps:**
1. Note current time
2. Save opportunity in Opportunity+
3. Continuously check oUP for engagement
4. Note time when engagement appears/updates

**Expected Results:**
- [ ] Engagement appears within 1-5 minutes
- [ ] Latency is consistent across multiple tests

---

## Field Mapping Test Cases

### FM-001: Key Information Section Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-001 |
| **Priority** | Critical |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Opportunity Name | "Test Integration Project Alpha 2026" |
| Description | "Detailed description with special chars: & < > \" ' " |
| Proposed Budget | 1,500,000 USD |

**Test Steps:**
1. Create opportunity with above test data
2. Save and wait for sync
3. Verify in oUP engagement

**Expected Results:**
- [ ] Engagement Name = "Test Integration Project Alpha 2026"
- [ ] Engagement Description = exact match including special characters
- [ ] Proposed Budget is NOT mapped (verify not present in oUP)

---

### FM-002: Products and Services Section Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-002 |
| **Priority** | High |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Delivery Modality | "Remote Implementation" |
| Products & Services | Infrastructure - Construction |

**Test Steps:**
1. Create opportunity with Products & Services data
2. Save and wait for sync
3. Verify Project Category in oUP

**Expected Results:**
- [ ] Project Category derived from Service line of Products & Services
- [ ] Delivery Modality reflected in Engagement Name

---

### FM-003: SDG and UN Framework Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-003 |
| **Priority** | High |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Context & Challenges | "Climate change impacts in region..." |
| SDG Alignment | SDG 13 (Climate Action), SDG 7 (Clean Energy) |
| UN Cooperation Framework | UNDAF 2022-2026 |

**Test Steps:**
1. Add SDGs with targets and indicators
2. Add UNCF alignment
3. Add Context & Challenges
4. Save and verify in oUP

**Expected Results:**
- [ ] SDG Contributions populated with SDG 13, SDG 7
- [ ] SDG Targets mapped to `eppm.aunops_opportunity_sdg_target`
- [ ] SDG Indicators mapped to `eppm.aunops_opportunity_sdg_indicator`
- [ ] Context & Challenges → Engagement Justification
- [ ] UN Cooperation Framework populated
- [ ] Impact, Outcomes NOT mapped (verify empty/null)

---

### FM-004: Partners and Budget Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-004 |
| **Priority** | Critical |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Funding Partners | Partner A ($500,000), Partner B ($300,000), Partner C ($200,000) |
| Client Partners | Client X, Client Y |
| Total Budget | 1,000,000 USD (auto-calculated) |

**Test Steps:**
1. Add multiple Funding Partners with amounts
2. Add multiple Client Partners
3. Save and verify in oUP

**Expected Results:**
- [ ] Partners → Funding Source contains all funding partners
- [ ] Partners → Client contains all client partners
- [ ] Amounts → Estimated Amount = 1,000,000
- [ ] Amounts → Currency = USD
- [ ] Amounts → Exchange Rate = 1
- [ ] External Stakeholders NOT mapped

---

### FM-005: Geographic Implementation Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-005 |
| **Priority** | High |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Implementation Countries | Kenya, Ethiopia, Uganda, Tanzania |

**Test Steps:**
1. Select multiple implementation countries
2. Save and verify in oUP

**Expected Results:**
- [ ] Countries of Implementation contains all 4 countries
- [ ] Each country mapped to `eppm.aunops_opportunity_country`

---

### FM-006: Timeline and Dates Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-006 |
| **Priority** | High |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Target Signing Date | 2026-06-15 |
| Implementation Start Date | 2026-07-01 |
| Target Delivery Date | 2028-12-31 |

**Test Steps:**
1. Set all date fields
2. Save and verify in oUP

**Expected Results:**
- [ ] Estimated Signing Date = 2026-06-15
- [ ] Implementation Start Date = 2026-07-01
- [ ] Implementation End Date = 2028-12-31
- [ ] Work breakdown structure NOT mapped

---

### FM-007: Team and Stakeholders Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-007 |
| **Priority** | High |
| **Type** | Positive |

**Test Data:**

| Opp+ Field | Test Value |
|------------|------------|
| Opportunity Manager | john.doe@unops.org |
| Opportunity Collaborators | jane.smith@unops.org, bob.wilson@unops.org |
| Org Unit Responsible | KEHO (Kenya Hub Office) |
| DOA2 | doa2.approver@unops.org |
| DOA3 | doa3.approver@unops.org |

**Test Steps:**
1. Assign Opportunity Manager
2. Add Collaborators
3. Set Organizational Unit
4. Save and verify in oUP

**Expected Results:**
- [ ] Business Developer = john.doe (resolved from email)
- [ ] Project Executive = DOA2 user
- [ ] Project Executive (OiC) = Opportunity Manager
- [ ] Engagement Team includes all collaborators as contributors
- [ ] Organisational Unit = KEHO
- [ ] Engagement Authority DoA2 populated
- [ ] Engagement Authority DoA3 populated

---

### FM-008: Unmapped Fields Verification

| Attribute | Value |
|-----------|-------|
| **Test ID** | FM-008 |
| **Priority** | Medium |
| **Type** | Negative |

**Preconditions:**
1. Opportunity with all fields filled

**Test Steps:**
1. Verify the following fields are NOT synced to oUP:
   - Proposed Budget for Initiative
   - Impact
   - Outcome(s)
   - Estimated number of direct beneficiaries
   - Estimated number of indirect beneficiaries
   - Alignment to UNOPS Strategic Missions
   - Organization Unit Strategy
   - External Stakeholders
   - Other External Stakeholders
   - Additional Notes
   - Work breakdown structure
   - Proposed Initiative Type
   - Other Internal Stakeholders

**Expected Results:**
- [ ] None of the above fields appear in oUP engagement
- [ ] No errors occur during sync due to these fields

---

## High-Risk Checklist Mapping Test Cases

### HR-001: Single High-Risk Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | HR-001 |
| **Priority** | Critical |
| **Type** | Positive |

**Test Data:**
- Risk: "No Host Country Agreement" (tagged as Organizational High Risk)

**Test Steps:**
1. Create opportunity
2. Add risk "No Host Country Agreement"
3. Tag as Organizational High Risk
4. Save and sync
5. Check oUP engagement

**Expected Results:**
- [ ] Survey question 1.1.1 answered "Yes"
- [ ] Risk created in Engagement Risk Register
- [ ] Risk response includes sufficient information

---

### HR-002: Multiple High-Risks Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | HR-002 |
| **Priority** | Critical |
| **Type** | Positive |

**Test Data:**
- Risk 1: "High-Risk Security Issues / Armed Conflict" (1.2.1)
- Risk 2: "New Funding Source or Client" (1.3.1)
- Risk 3: "Reputational Risk" (1.4.4)
- Risk 4: "Engagement Exceeds $100 Million" (3.1.1)

**Test Steps:**
1. Add all 4 high risks
2. Tag all as Organizational High Risk
3. Save and sync
4. Verify in oUP

**Expected Results:**
- [ ] Survey questions 1.2.1, 1.3.1, 1.4.4, 3.1.1 answered "Yes"
- [ ] 4 risks created in Engagement Risk Register
- [ ] Each risk has appropriate risk responses

---

### HR-003: All 17 High-Risk Types Mapping

| Attribute | Value |
|-----------|-------|
| **Test ID** | HR-003 |
| **Priority** | High |
| **Type** | Positive |

**Test Data:**
All 17 predefined high risks:
1. No Host Country Agreement (1.1.1)
2. High-Risk Security Issues / Armed Conflict (1.2.1)
3. New Funding Source or Client (1.3.1)
4. Scope Outside UNOPS Mandate (1.4.1)
5. Support to Non-UN Security Forces (1.4.2)
6. Conflict of Interest (1.4.3)
7. Reputational Risk (1.4.4)
8. Pre-selection by Government with CPI < 50 (1.4.5)
9. Pay Agent Services to Third Parties (1.4.6)
10. Negative SDG Impact (2.1.1)
11. Grants to For-Profit Entities or Individuals (2.2.1)
12. IT Security and Privacy Risks (2.3.1)
13. Engagement Exceeds $100 Million (3.1.1)
14. Pricing Policy Deviation (3.1.2)
15. Currency Exchange Risk (3.2.1)
16. Implementation Before/After Legal Agreement (3.3.1)
17. Other Undefined High Risks (4.1.1)

**Test Steps:**
1. Add all 17 risks to opportunity
2. Tag all as Organizational High Risk
3. Save and sync
4. Verify each in oUP

**Expected Results:**
- [ ] All 17 survey questions answered "Yes"
- [ ] 17 risks in Engagement Risk Register
- [ ] Correct mapping for each risk type

---

### HR-004: Non-High-Risk Not Mapped

| Attribute | Value |
|-----------|-------|
| **Test ID** | HR-004 |
| **Priority** | High |
| **Type** | Negative |

**Test Data:**
- Risk: "Reputational Risk" (NOT tagged as Organizational High Risk)

**Test Steps:**
1. Add risk without high-risk tag
2. Save and sync
3. Verify in oUP

**Expected Results:**
- [ ] Survey question 1.4.4 answered "No" or empty
- [ ] Risk NOT created in Engagement Risk Register
- [ ] Only high-risk tagged items are synced

---

## Email Notification Test Cases

### EN-001: New Engagement Email Notification

| Attribute | Value |
|-----------|-------|
| **Test ID** | EN-001 |
| **Priority** | Critical |
| **Type** | Positive |

**Preconditions:**
1. Fresh opportunity with no linked engagement
2. Access to email for PE, DoA2, and BD

**Test Steps:**
1. Create and save opportunity
2. Wait for sync and email delivery
3. Check email inboxes

**Expected Results:**
- [ ] Email sent from: noreply@unops.org
- [ ] Email received by Project Executive
- [ ] Email received by DoA2
- [ ] Email received by Business Developer
- [ ] Subject: "Engagement Created from Opportunity+ - [base engagement number]"
- [ ] Body contains Opportunity ID
- [ ] Body contains Opportunity Name
- [ ] Body contains Engagement Number
- [ ] Body contains Stage: Pre-Engagement
- [ ] Body contains link to oUP engagement
- [ ] Body contains link to Opportunity+

---

### EN-002: Updated Engagement Email Notification

| Attribute | Value |
|-----------|-------|
| **Test ID** | EN-002 |
| **Priority** | High |
| **Type** | Positive |

**Preconditions:**
1. Opportunity with existing linked engagement

**Test Steps:**
1. Modify opportunity fields
2. Save opportunity
3. Wait for email delivery
4. Check email inboxes

**Expected Results:**
- [ ] Subject: "Engagement Updated from Opportunity+ - [base engagement number]"
- [ ] Email received by all recipients
- [ ] Body indicates update (not creation)
- [ ] Links to both systems included

---

### EN-003: Email Recipient Resolution

| Attribute | Value |
|-----------|-------|
| **Test ID** | EN-003 |
| **Priority** | Medium |
| **Type** | Positive |

**Test Steps:**
1. Verify email addresses resolved from user IDs
2. Confirm all 3 recipient types receive emails

**Expected Results:**
- [ ] PE email correctly resolved
- [ ] DoA2 email correctly resolved
- [ ] BD (Opportunity Manager) email correctly resolved

---

### EN-004: Email Links Validation

| Attribute | Value |
|-----------|-------|
| **Test ID** | EN-004 |
| **Priority** | High |
| **Type** | Positive |

**Test Steps:**
1. Receive notification email
2. Click oUP engagement link
3. Click Opportunity+ link

**Expected Results:**
- [ ] oUP link navigates to correct engagement: `https://projects.unops.org/?route=uenb/<base_eng>/engagement/overview`
- [ ] Opportunity+ link navigates to correct opportunity: `https://opportunityplus.unops.org/#/partnerships/opportunities/<opp_id>`

---

## Deep Linking Test Cases

### DL-001: Go to oUP Button in Opportunity+

| Attribute | Value |
|-----------|-------|
| **Test ID** | DL-001 |
| **Priority** | High |
| **Type** | Positive |

**Preconditions:**
1. Opportunity with successful integration
2. **Production environment only** (not testable in non-prod)

**Test Steps:**
1. Open opportunity in Opportunity+
2. Locate "Go to oUP" button next to status tag
3. Click the button

**Expected Results:**
- [ ] Button visible after successful integration
- [ ] Button navigates to correct oUP engagement
- [ ] Engagement overview page loads

---

### DL-002: View in Opportunity+ Button in oUP

| Attribute | Value |
|-----------|-------|
| **Test ID** | DL-002 |
| **Priority** | High |
| **Type** | Positive |

**Preconditions:**
1. Engagement created from Opportunity+ integration

**Test Steps:**
1. Open engagement in oUP
2. Scroll to footer
3. Locate "View in Opportunity+" button
4. Click the button

**Expected Results:**
- [ ] Button visible in engagement footer
- [ ] Button navigates to correct Opportunity+ page
- [ ] Opportunity details page loads with correct data

---

## Idempotency Test Cases

### ID-001: Multiple Saves Without Duplication

| Attribute | Value |
|-----------|-------|
| **Test ID** | ID-001 |
| **Priority** | Critical |
| **Type** | Positive |

**Test Steps:**
1. Create and save opportunity (creates engagement)
2. Save again without changes
3. Save 5 more times
4. Check oUP

**Expected Results:**
- [ ] Only ONE engagement exists in oUP
- [ ] No duplicate entries in shadow tables
- [ ] No duplicate risks or relationships

---

### ID-002: Rapid Sequential Saves

| Attribute | Value |
|-----------|-------|
| **Test ID** | ID-002 |
| **Priority** | High |
| **Type** | Stress |

**Test Steps:**
1. Save opportunity
2. Immediately save again (within seconds)
3. Repeat 3-4 times rapidly
4. Wait for all syncs to complete
5. Verify in oUP

**Expected Results:**
- [ ] Single engagement (no duplicates)
- [ ] Final data reflects last save
- [ ] No data corruption

---

### ID-003: Concurrent User Updates

| Attribute | Value |
|-----------|-------|
| **Test ID** | ID-003 |
| **Priority** | Medium |
| **Type** | Concurrency |

**Preconditions:**
- Two users with access to same opportunity

**Test Steps:**
1. User A opens opportunity
2. User B opens same opportunity
3. User A saves changes
4. User B saves different changes (before A's sync completes)
5. Verify final state in oUP

**Expected Results:**
- [ ] No duplicate engagements
- [ ] Final data consistent
- [ ] Both changes applied or conflict handled gracefully

---

## Error Handling Test Cases

### EH-001: Invalid User Email Resolution

| Attribute | Value |
|-----------|-------|
| **Test ID** | EH-001 |
| **Priority** | Medium |
| **Type** | Negative |

**Test Data:**
- Opportunity Manager with non-existent email in oUP

**Test Steps:**
1. Assign invalid/non-existent email as Opportunity Manager
2. Save and sync
3. Check oUP and logs

**Expected Results:**
- [ ] Integration handles gracefully (doesn't crash)
- [ ] Error logged for investigation
- [ ] Engagement created with available data
- [ ] Clear error message/notification

---

### EH-002: Large Payload Handling

| Attribute | Value |
|-----------|-------|
| **Test ID** | EH-002 |
| **Priority** | High |
| **Type** | Performance |

**Test Data:**
- Maximum number of countries
- Maximum number of partners
- All SDGs with all targets and indicators
- All high risks
- Long text in all text fields

**Test Steps:**
1. Create opportunity with maximum data
2. Save and sync
3. Verify complete sync

**Expected Results:**
- [ ] Large XML payload transmitted successfully
- [ ] All data synced correctly
- [ ] No timeouts or failures
- [ ] Sync completes within expected timeframe

---

### EH-003: Special Characters in Text Fields

| Attribute | Value |
|-----------|-------|
| **Test ID** | EH-003 |
| **Priority** | Medium |
| **Type** | Edge Case |

**Test Data:**
- Name: `Test & Project <2026> "Special" 'Chars'`
- Description: Contains `& < > " ' \n \t` and Unicode characters

**Test Steps:**
1. Enter special characters in text fields
2. Save and sync
3. Verify in oUP

**Expected Results:**
- [ ] All special characters preserved
- [ ] No XML parsing errors
- [ ] No data corruption
- [ ] Unicode characters handled correctly

---

## Performance Test Cases

### PF-001: Sync Latency Benchmark

| Attribute | Value |
|-----------|-------|
| **Test ID** | PF-001 |
| **Priority** | Medium |
| **Type** | Performance |

**Test Steps:**
1. Record sync times for 10 different opportunities
2. Calculate average, min, max latency

**Expected Results:**
- [ ] Average latency: 1-5 minutes
- [ ] Maximum latency: < 10 minutes
- [ ] Consistent performance

---

### PF-002: Large Data Set Sync Time

| Attribute | Value |
|-----------|-------|
| **Test ID** | PF-002 |
| **Priority** | Medium |
| **Type** | Performance |

**Test Steps:**
1. Create opportunity with maximum data
2. Measure sync completion time
3. Compare to minimal data opportunity

**Expected Results:**
- [ ] Sync completes within acceptable time
- [ ] Performance degradation proportional to data size
- [ ] No timeouts

---

## Edge Cases

### EC-001: Empty Optional Fields

| Attribute | Value |
|-----------|-------|
| **Test ID** | EC-001 |
| **Priority** | Medium |
| **Type** | Edge Case |

**Test Steps:**
1. Create opportunity with only required fields
2. Leave all optional fields empty
3. Save and sync

**Expected Results:**
- [ ] Engagement created successfully
- [ ] Optional fields in oUP are null/empty
- [ ] No errors during sync

---

### EC-002: Maximum Field Lengths

| Attribute | Value |
|-----------|-------|
| **Test ID** | EC-002 |
| **Priority** | Medium |
| **Type** | Edge Case |

**Test Data:**
- Name: Maximum allowed characters
- Description: Maximum allowed characters

**Test Steps:**
1. Enter maximum length text in all fields
2. Save and sync
3. Verify in oUP

**Expected Results:**
- [ ] Data truncated appropriately if needed
- [ ] No errors or crashes
- [ ] Clear indication if truncation occurred

---

### EC-003: Date Edge Cases

| Attribute | Value |
|-----------|-------|
| **Test ID** | EC-003 |
| **Priority** | Low |
| **Type** | Edge Case |

**Test Data:**
- Past dates
- Far future dates (e.g., 2099)
- Dates spanning year boundaries

**Test Steps:**
1. Test various date scenarios
2. Verify date handling in oUP

**Expected Results:**
- [ ] Dates transferred correctly
- [ ] No timezone conversion issues
- [ ] Date formats consistent

---

### EC-004: Currency Handling

| Attribute | Value |
|-----------|-------|
| **Test ID** | EC-004 |
| **Priority** | Medium |
| **Type** | Edge Case |

**Test Data:**
- Very large amounts (e.g., $999,999,999)
- Amounts with cents (e.g., $1,234,567.89)
- Zero amount

**Test Steps:**
1. Test various amount scenarios
2. Verify in oUP

**Expected Results:**
- [ ] Currency always USD
- [ ] Exchange Rate always 1
- [ ] Amounts calculated correctly from funding partners
- [ ] Large amounts handled without overflow

---

## Test Execution Summary Template

### Test Run Information

| Attribute | Value |
|-----------|-------|
| **Test Run ID** | |
| **Executed By** | |
| **Execution Date** | |
| **Environment** | |
| **Build/Version** | |

### Results Summary

| Category | Total | Passed | Failed | Blocked | Not Run |
|----------|-------|--------|--------|---------|---------|
| Integration Flow | 4 | | | | |
| Field Mapping | 8 | | | | |
| High-Risk Mapping | 4 | | | | |
| Email Notification | 4 | | | | |
| Deep Linking | 2 | | | | |
| Idempotency | 3 | | | | |
| Error Handling | 3 | | | | |
| Performance | 2 | | | | |
| Edge Cases | 4 | | | | |
| **TOTAL** | **34** | | | | |

### Defects Found

| Defect ID | Test Case | Severity | Description |
|-----------|-----------|----------|-------------|
| | | | |

---

## Appendix A: Field Mapping Quick Reference

### Mapped Fields

| Section | Opp+ Field | oUP Field |
|---------|------------|-----------|
| Key Info | Opportunity Name | Engagement Name |
| Key Info | Description | Engagement Description |
| Products | Products & Services | Project Category (derived) |
| Impact | Context & Challenges | Engagement Justification |
| Impact | SDG Alignment | SDG Contributions |
| Impact | UN Cooperation Framework | UN Cooperation Framework |
| Partners | Funding Partners | Partners → Funding Source |
| Partners | Client Partners | Partners → Client |
| Partners | Total Budget | Amounts → Estimated Amount |
| Location | Implementation Countries | Countries of Implementation |
| Timeline | Target Signing Date | Estimated Signing Date |
| Timeline | Implementation Start Date | Implementation Start Date |
| Timeline | Target Delivery Date | Implementation End Date |
| Team | Opportunity Manager | Business Developer |
| Team | Opportunity Collaborators | Engagement Team (contributors) |
| Team | Org Unit Responsible | Organisational Unit |
| Team | DOA2 | Engagement Authority DOA2 |
| Team | DOA3 | Engagement Authority DOA3 |

### Unmapped Fields

- Proposed Budget for Initiative
- Impact
- Outcome(s)
- Beneficiary counts (direct/indirect)
- Alignment to UNOPS Strategic Missions
- Organization Unit Strategy
- External Stakeholders
- Other External Stakeholders
- Additional Notes
- Work breakdown structure
- Proposed Initiative Type
- Other Internal Stakeholders

---

## Appendix B: High-Risk Mapping Reference

| oUP S.No. | Opp+ Predefined Risk |
|-----------|---------------------|
| 1.1.1 | No Host Country Agreement |
| 1.2.1 | High-Risk Security Issues / Armed Conflict |
| 1.3.1 | New Funding Source or Client |
| 1.4.1 | Scope Outside UNOPS Mandate |
| 1.4.2 | Support to Non-UN Security Forces |
| 1.4.3 | Conflict of Interest |
| 1.4.4 | Reputational Risk |
| 1.4.5 | Pre-selection by Government with CPI < 50 |
| 1.4.6 | Pay Agent Services to Third Parties |
| 2.1.1 | Negative SDG Impact (Social/Environmental/Economic) |
| 2.2.1 | Grants to For-Profit Entities or Individuals |
| 2.3.1 | IT Security and Privacy Risks |
| 3.1.1 | Engagement Exceeds $100 Million |
| 3.1.2 | Pricing Policy Deviation |
| 3.2.1 | Currency Exchange Risk |
| 3.3.1 | Implementation Before/After Legal Agreement |
| 4.1.1 | Other Undefined High Risks |
