# Opportunity+ to oUP Integration - QA Validation Checklist

## Quick Reference Checklist

Use this checklist during integration testing to validate all aspects of the Opportunity+ to oUP sync.

---

## Pre-Test Setup Checklist

- [ ] Access to Opportunity+ test environment confirmed
- [ ] Access to oUP test environment confirmed
- [ ] Test user accounts have appropriate permissions
- [ ] Email inbox access for notification testing
- [ ] Test data prepared (partners, countries, SDGs, risks)

---

## Sync Trigger Validation

- [ ] Save in Opportunity+ triggers sync
- [ ] Sync occurs within 1-5 minutes
- [ ] New opportunity creates new engagement
- [ ] Existing opportunity updates existing engagement
- [ ] No duplicate engagements created

---

## Key Information Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Opportunity Name | Engagement Name | ☐ |
| Description | Engagement Description | ☐ |
| Proposed Budget | NOT MAPPED | ☐ |

---

## Products and Services Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Delivery Modality | Engagement Name | ☐ |
| Products & Services | Project Category (derived) | ☐ |

---

## Impact & Strategic Alignment Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Context & Challenges | Engagement Justification | ☐ |
| SDG Alignment | SDG Contributions | ☐ |
| UN Cooperation Framework | UN Cooperation Framework | ☐ |
| Impact | NOT MAPPED | ☐ |
| Outcome(s) | NOT MAPPED | ☐ |
| Direct Beneficiaries | NOT MAPPED | ☐ |
| Indirect Beneficiaries | NOT MAPPED | ☐ |
| UNOPS Strategic Missions | NOT MAPPED | ☐ |
| Organization Unit Strategy | NOT MAPPED | ☐ |

---

## Partners & Stakeholders Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Total Budget (USD) | Amounts → Estimated Amount | ☐ |
| Currency | USD (default) | ☐ |
| Exchange Rate | 1 (default) | ☐ |
| Funding Partners | Partners → Funding Source | ☐ |
| Client Partners | Partners → Client | ☐ |
| External Stakeholders | NOT MAPPED | ☐ |
| Other External Stakeholders | NOT MAPPED | ☐ |
| Additional Notes | NOT MAPPED | ☐ |

---

## Geographic Implementation Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Implementation Countries | Countries of Implementation | ☐ |

---

## Timeline Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Target Signing Date | Estimated Signing Date | ☐ |
| Implementation Start Date | Implementation Start Date | ☐ |
| Target Delivery Date | Implementation End Date | ☐ |
| Work breakdown structure | NOT MAPPED | ☐ |

---

## Team & Stakeholders Field Mapping

| Opp+ Field | oUP Field | Verified |
|------------|-----------|:--------:|
| Opportunity Manager | Business Developer | ☐ |
| Opportunity Collaborators | Engagement Team (contributors) | ☐ |
| Org Unit Responsible | Organisational Unit | ☐ |
| DOA2 | Engagement Authority DoA2 | ☐ |
| DOA3 | Engagement Authority DoA3 | ☐ |
| Proposed Initiative Type | NOT MAPPED | ☐ |
| Other Internal Stakeholders | NOT MAPPED | ☐ |

---

## High-Risk Mapping Validation

When risk is tagged as "Organizational High Risk", verify survey question answered "Yes" and risk created in Risk Register.

| oUP S.No. | Risk Type | Verified |
|-----------|-----------|:--------:|
| 1.1.1 | No Host Country Agreement | ☐ |
| 1.2.1 | High-Risk Security Issues / Armed Conflict | ☐ |
| 1.3.1 | New Funding Source or Client | ☐ |
| 1.4.1 | Scope Outside UNOPS Mandate | ☐ |
| 1.4.2 | Support to Non-UN Security Forces | ☐ |
| 1.4.3 | Conflict of Interest | ☐ |
| 1.4.4 | Reputational Risk | ☐ |
| 1.4.5 | Pre-selection by Government with CPI < 50 | ☐ |
| 1.4.6 | Pay Agent Services to Third Parties | ☐ |
| 2.1.1 | Negative SDG Impact | ☐ |
| 2.2.1 | Grants to For-Profit Entities | ☐ |
| 2.3.1 | IT Security and Privacy Risks | ☐ |
| 3.1.1 | Engagement Exceeds $100 Million | ☐ |
| 3.1.2 | Pricing Policy Deviation | ☐ |
| 3.2.1 | Currency Exchange Risk | ☐ |
| 3.3.1 | Implementation Before/After Legal Agreement | ☐ |
| 4.1.1 | Other Undefined High Risks | ☐ |

---

## Email Notification Validation

### New Engagement Email

- [ ] Email sent from: noreply@unops.org
- [ ] Subject: "Engagement Created from Opportunity+ - [number]"
- [ ] Received by Project Executive
- [ ] Received by DoA2
- [ ] Received by Business Developer
- [ ] Contains Opportunity ID
- [ ] Contains Opportunity Name
- [ ] Contains Engagement Number
- [ ] Contains Stage: Pre-Engagement
- [ ] Contains oUP link
- [ ] Contains Opportunity+ link

### Update Email

- [ ] Subject: "Engagement Updated from Opportunity+ - [number]"
- [ ] Received by all recipients
- [ ] Indicates update (not creation)
- [ ] Links functional

---

## Deep Linking Validation

### Go to oUP Button (Opportunity+)

- [ ] Button appears after successful sync
- [ ] Located next to status tag
- [ ] Navigates to correct engagement
- [ ] URL format: `https://projects.unops.org/?route=uenb/<base_eng>/engagement/overview`

**Note:** Only testable in production environment

### View in Opportunity+ Button (oUP)

- [ ] Button appears in engagement footer
- [ ] Navigates to correct opportunity
- [ ] URL format: `https://opportunityplus.unops.org/#/partnerships/opportunities/<opp_id>`

---

## Idempotency Validation

- [ ] Multiple saves = single engagement
- [ ] Rapid saves handled correctly
- [ ] No duplicate data in shadow tables
- [ ] Final state reflects last save

---

## Shadow Table Validation

Verify data inserted into correct tables:

| Table | Data Type | Verified |
|-------|-----------|:--------:|
| eppm.aunops_opportunity | Main opportunity | ☐ |
| eppm.aunops_opportunity_country | Countries | ☐ |
| eppm.aunops_opportunity_funding_partner | Funding Partners | ☐ |
| eppm.aunops_opportunity_client_partner | Client Partners | ☐ |
| eppm.aunops_opportunity_stakeholder | Stakeholders | ☐ |
| eppm.aunops_opportunity_deliverable | Deliverables | ☐ |
| eppm.aunops_opportunity_sdg | SDGs | ☐ |
| eppm.aunops_opportunity_sdg_target | SDG Targets | ☐ |
| eppm.aunops_opportunity_sdg_indicator | SDG Indicators | ☐ |
| eppm.aunops_opportunity_collaborator | Collaborators | ☐ |
| eppm.aunops_opportunity_uncf_outcome | UNCF Outcomes | ☐ |
| eppm.aunops_opportunity_uncf_indicator | UNCF Indicators | ☐ |
| eppm.aunops_opportunity_risk | Risks | ☐ |

---

## oUP Engagement Validation

| Field | Expected Value | Verified |
|-------|----------------|:--------:|
| Stage | Pre-Engagement | ☐ |
| Output sub-group | Blank | ☐ |
| Programme/Portfolio | "Not Applicable" | ☐ |
| Project Category | Derived from Service line | ☐ |
| HoSS OiC | Autogenerated | ☐ |

---

## Test Run Sign-Off

| Attribute | Value |
|-----------|-------|
| **Tester Name** | |
| **Test Date** | |
| **Environment** | |
| **Overall Result** | ☐ PASS / ☐ FAIL |
| **Notes** | |

---

## Quick Links

- **Stored Procedure:** `ou-erp/Database/Stored Procedures/dbo.aunops_Integration_OpportunityPlus_Opportunity_Synchronize.sql`
- **Full Test Cases:** [OpportunityPlus_oUP_Integration_TestCases.md](./OpportunityPlus_oUP_Integration_TestCases.md)
- **Integration Rule:** `.cursor/rules/opportunity-oup-integration.mdc`
