# Additional End-to-End Test Scenarios for Opportunity Features

**Created:** January 13, 2026  
**Purpose:** Comprehensive positive and negative E2E scenarios covering complex business workflows  
**Total Scenarios:** 40+ (20 Positive, 20 Negative)

---

## Overview

This document provides **additional End-to-End test scenarios** beyond the 15 E2E tests already created in `ADVANCED_TEST_COVERAGE.md`. These scenarios cover more complex, realistic business workflows and failure scenarios based on PRD requirements.

---

## 🟢 POSITIVE END-TO-END SCENARIOS (20 scenarios)

### Category 1: Multi-Stakeholder Collaboration Workflows (5 scenarios)

#### TC-OPP-E2E-POS-001: Multi-Regional Opportunity Coordination
**Priority:** P1  
**Category:** E2E Positive - Collaboration

**Business Scenario:**
Large infrastructure opportunity spanning 3 countries (Bangladesh, Nepal, Pakistan) requires coordination between regional offices, HQ advisors, and local teams.

**Test Flow:**
1. **Setup:**
   - Bangladesh office creates initial opportunity ($5M, multi-country)
   - Links to existing partnership agreement with Asian Development Bank
   
2. **Regional Coordination:**
   - Nepal regional office adds country-specific deliverables
   - Pakistan office adds local partner organizations
   - Each region contributes to country-specific risk assessments
   
3. **HQ Review:**
   - Infrastructure advisor reviews technical specifications
   - Procurement advisor reviews partner due diligence
   - Legal advisor reviews partnership agreement compliance
   - All add comments and recommendations
   
4. **DST Profile Generation:**
   - System aggregates data from all 3 countries
   - Generates unified DST profile considering:
     - All country contexts (fragile state indicators, corruption indices)
     - Combined complexity score
     - Multi-regional implementation risks
   
5. **Budget Consolidation:**
   - System generates consolidated budget
   - Each country's costs segregated
   - Travel and coordination costs calculated
   - Currency conversions handled (USD, NPR, PKR, BDT)
   
6. **Decision Package Assembly:**
   - All regional inputs consolidated
   - Multi-regional approval workflow initiated
   - DOA2 reviews (higher authority due to multi-country)
   
7. **Go Decision:**
   - DOA2 approves with regional implementation conditions
   - Budget authorized for all 3 countries
   - Regional coordinators assigned
   
8. **Programme Conversion:**
   - Opportunity converted to Programme (multi-country)
   - 3 child projects created (one per country)
   - All data flows to projects
   - Regional teams notified

**Expected Results:**
- ✅ All 3 regional offices collaborate seamlessly
- ✅ Data from all regions aggregated correctly
- ✅ DST considers all country contexts
- ✅ Budget consolidation accurate across currencies
- ✅ Multi-level approvals tracked
- ✅ Programme and 3 projects created
- ✅ Complete audit trail across all regions
- ✅ Notifications to all stakeholders
- ✅ Execution time: <10 minutes

**Validation Points:**
- No data loss during multi-user editing
- Currency conversions accurate
- All regional risks captured
- Approval workflow follows multi-regional rules
- Programme structure correct

---

#### TC-OPP-E2E-POS-002: Real-Time Collaborative Editing
**Priority:** P1  
**Category:** E2E Positive - Collaboration

**Business Scenario:**
Opportunity Manager, Budget Specialist, and Technical Advisor work simultaneously on opportunity development during a 2-hour workshop.

**Test Flow:**
1. **Simultaneous Editing (3 users):**
   - User A (Opportunity Manager): Updates deliverables and timeline
   - User B (Budget Specialist): Creates draft budget line items
   - User C (Technical Advisor): Adds technical specifications
   
2. **Real-Time Sync:**
   - All users see each other's changes within 2 seconds
   - Optimistic locking prevents conflicting edits
   - Version control tracks all changes
   
3. **Conflict Resolution:**
   - User A and User C both try to update the same description field
   - System detects conflict
   - Users shown each other's changes
   - Manual merge facilitated
   
4. **Collaborative DST Generation:**
   - User A triggers DST profile generation
   - All users see DST progress in real-time
   - DST results immediately visible to all
   
5. **Collaborative Review:**
   - All users review DST recommendations together
   - Accept/reject decisions tracked by user
   - Comments and rationale captured
   
6. **Final Package Assembly:**
   - User A initiates submission for decision
   - System validates all sections complete
   - All contributors notified
   - Decision package assembled with attribution

**Expected Results:**
- ✅ All 3 users work simultaneously without blocking
- ✅ Changes visible to all within 2 seconds
- ✅ Conflicts detected and resolved
- ✅ All edits tracked with user attribution
- ✅ DST generation doesn't block other work
- ✅ Collaborative decision-making captured
- ✅ Final package includes all contributions

---

#### TC-OPP-E2E-POS-003: Delegated Decision Workflow with Escalation
**Priority:** P1  
**Category:** E2E Positive - Workflow

**Business Scenario:**
DOA holder is unavailable, delegates to deputy, who escalates due to complexity.

**Test Flow:**
1. **Opportunity Submission:**
   - Opportunity Manager submits $2M opportunity for decision
   - Default DOA3 holder is on leave (system knows this)
   
2. **Automatic Delegation:**
   - System checks DOA3 availability
   - Detects "On Leave" status (until next week)
   - Automatically delegates to designated deputy DOA3
   - Deputy notified via email and in-app
   
3. **Deputy Review:**
   - Deputy reviews opportunity package
   - Identifies complexity beyond comfort level
   - Decides to escalate to DOA2 for guidance
   
4. **Manual Escalation:**
   - Deputy escalates with justification: "Multi-country, high-risk context"
   - DOA2 notified
   - Original DOA3 copied on escalation
   
5. **DOA2 Review:**
   - DOA2 reviews complete package
   - Adds additional conditions:
     - "Require Infrastructure advisor sign-off"
     - "Monthly progress reports to HQ"
   
6. **Conditional Go Decision:**
   - DOA2 records "Go with Conditions"
   - Conditions tracked as checklist items
   - Opportunity Manager notified
   
7. **Condition Fulfillment:**
   - Infrastructure advisor reviews and approves
   - Sign-off attached to opportunity
   - Reporting schedule set up
   
8. **Authorization:**
   - Budget authorized by DOA2
   - Personnel authorized
   - Project conversion enabled

**Expected Results:**
- ✅ Automatic delegation to deputy when holder unavailable
- ✅ Manual escalation tracked with justification
- ✅ Conditions clearly defined and tracked
- ✅ All stakeholders notified at each step
- ✅ Complete audit trail of delegation chain
- ✅ Conditions must be met before final authorization
- ✅ Original DOA holder can review upon return

---

#### TC-OPP-E2E-POS-004: Partnership Agreement Triggers Opportunity Creation
**Priority:** P2  
**Category:** E2E Positive - Integration

**Business Scenario:**
New partnership agreement uploaded triggers creation of pre-populated opportunity.

**Test Flow:**
1. **Agreement Upload:**
   - Partnerships team uploads MOU with Government of Ethiopia
   - Document: 45-page partnership agreement PDF
   
2. **AI Extraction:**
   - System extracts key terms:
     - Geography: Ethiopia (primary), Kenya (secondary)
     - Scope: Water infrastructure and sanitation
     - Financial terms: 8% fee, $10M ceiling
     - Duration: 3 years
     - SDGs: 6 (Water), 11 (Sustainable Cities)
   
3. **Agreement Validation:**
   - Partnerships manager reviews extracted data
   - Corrects scope details
   - Confirms all key terms
   - Agreement marked "Active" with 3-year validity
   
4. **Opportunity Auto-Creation:**
   - System suggests creating opportunity
   - "Would you like to create an opportunity based on this agreement?"
   - User clicks "Yes, create opportunity"
   
5. **Pre-Population:**
   - New opportunity created with:
     - Name: "Water Infrastructure - Ethiopia"
     - Geography: Ethiopia (primary), Kenya (secondary)
     - Partners: Government of Ethiopia
     - Fee structure: 8%
     - Budget ceiling: $10M
     - Timeline: 3-year duration
     - SDGs: 6, 11 (pre-selected)
     - Partnership agreement linked
   
6. **Opportunity Refinement:**
   - Opportunity Manager adds specific deliverables
   - Refines budget and timeline
   - Uploads concept note from partner
   
7. **Validation Against Agreement:**
   - DST generation checks:
     - Geography matches agreement
     - Budget within $10M ceiling
     - Fee at 8% per agreement
   - All validations pass
   
8. **Decision and Conversion:**
   - Go decision recorded
   - Project created
   - Agreement automatically linked to project

**Expected Results:**
- ✅ AI extracts agreement terms accurately (>85%)
- ✅ Opportunity pre-populated with agreement data
- ✅ 60-70% of fields auto-filled
- ✅ Budget and geography validated against agreement
- ✅ Agreement linkage maintained through conversion
- ✅ Saves 2-3 hours of manual data entry

---

#### TC-OPP-E2E-POS-005: Portfolio Aggregation from Multiple Opportunities
**Priority:** P2  
**Category:** E2E Positive - Portfolio Management

**Business Scenario:**
Regional office recognizes 4 related water opportunities should be managed as portfolio.

**Test Flow:**
1. **Existing Opportunities:**
   - 4 opportunities in various stages:
     - Water Supply - Urban Areas ($3M, Profiling stage)
     - Sanitation - Rural Areas ($2M, Decision stage)
     - Water Treatment Facilities ($4M, Draft)
     - Community Hygiene Programs ($1M, Profiling)
   
2. **Portfolio Recognition:**
   - Regional Manager identifies common themes:
     - All water/sanitation sector
     - Same country (Tanzania)
     - Same 5-year timeframe
     - Overlapping beneficiaries
     - Shared infrastructure
   - Decides to create portfolio
   
3. **Portfolio Creation:**
   - System creates new Portfolio entity
   - Name: "Tanzania Water & Sanitation Programme"
   - Total value: $10M aggregated
   - Timeline: 5 years
   - Governance structure defined
   
4. **Opportunity Linking:**
   - All 4 opportunities linked to portfolio
   - Relationships: "Child of Portfolio"
   - Each maintains individual workflow status
   
5. **Portfolio-Level Analysis:**
   - Aggregated DST profile generated:
     - Combined complexity score
     - Shared risks identified
     - Synergy opportunities highlighted
   - Portfolio-level budget created
   - Master schedule with dependencies
   
6. **Portfolio Governance:**
   - Portfolio Board assigned
   - Portfolio Manager appointed
   - Monthly steering committee schedule
   
7. **Child Opportunity Progression:**
   - Each opportunity progresses individually
   - Portfolio Manager can view all statuses
   - Consolidated reporting available
   
8. **Portfolio Go Decision:**
   - DOA1 reviews portfolio (higher authority)
   - Approves portfolio approach
   - Authorizes $10M portfolio budget
   
9. **Conversion:**
   - Portfolio converted to Programme
   - 4 opportunities → 4 Projects (children)
   - Portfolio governance structure maintained

**Expected Results:**
- ✅ Portfolio created from existing opportunities
- ✅ Aggregated budget: $10M accurate
- ✅ DST identifies synergies between projects
- ✅ Shared risks consolidated
- ✅ Individual opportunity workflows maintained
- ✅ Portfolio-level governance established
- ✅ Programme and 4 child projects created
- ✅ Consolidated reporting available

---

### Category 2: Advanced AI and Document Processing (4 scenarios)

#### TC-OPP-E2E-POS-006: AI-Driven Opportunity Discovery from Multiple Documents
**Priority:** P1  
**Category:** E2E Positive - AI Integration

**Business Scenario:**
Partner sends 5 documents (emails, concept note, feasibility study, budget estimate, MOU) - system synthesizes into opportunity.

**Test Flow:**
1. **Bulk Document Upload:**
   - Opportunity Manager uploads 5 documents:
     - Email thread (15 messages, PDF)
     - Concept note (8 pages, Word)
     - Feasibility study (60 pages, PDF)
     - Budget estimate (Excel)
     - Draft MOU (20 pages, PDF)
   
2. **Multi-Document AI Processing:**
   - System processes all documents in parallel
   - Extraction time: ~2-3 minutes for all 5
   - AI identifies:
     - **From email thread:**
       - Timeline discussions: "Target start Q2 2026"
       - Partner contact details
       - Key stakeholder names
     - **From concept note:**
       - Project objective and outcomes
       - Beneficiary demographics
       - Geographic scope: 3 districts
     - **From feasibility study:**
       - Risk analysis
       - Implementation challenges
       - Technical requirements
       - Similar project benchmarks
     - **From budget:**
       - Line item costs
       - Personnel requirements
       - Equipment needs
       - Total: $2.8M
     - **From MOU:**
       - Partner commitments
       - UNOPS role definition
       - Governance structure
   
3. **Data Synthesis:**
   - AI synthesizes data from all 5 sources
   - Resolves conflicts:
     - Budget in concept note ($2.5M) vs budget file ($2.8M)
     - System flags discrepancy for review
     - User selects $2.8M (more detailed)
   - Cross-validates information
   
4. **Opportunity Creation:**
   - System creates opportunity with 85% of fields populated
   - Only 15% require manual input:
     - Org unit selection (user's office)
     - Currency selection (USD confirmed)
     - Few missing technical details
   
5. **Confidence Scores:**
   - Each field shows confidence score:
     - Budget: 95% confidence
     - Timeline: 88% confidence
     - Geography: 98% confidence
     - Deliverables: 75% confidence (needs review)
   - Low confidence fields highlighted for review
   
6. **Human Review:**
   - Opportunity Manager reviews all extracted data
   - Corrects deliverables (low confidence)
   - Validates budget discrepancy resolution
   - Accepts all other fields
   
7. **SDG and UNCF Suggestions:**
   - AI suggests SDGs based on content: 1, 6, 11
   - Suggests relevant UNCF outcomes for Tanzania
   - User accepts suggestions
   
8. **Workflow Continuation:**
   - Opportunity immediately ready for DST profiling
   - No need to manually enter data
   - Time saved: 4-5 hours vs manual entry

**Expected Results:**
- ✅ All 5 documents processed successfully
- ✅ 85% of fields auto-populated
- ✅ Conflicts detected and resolved
- ✅ Confidence scores guide review
- ✅ Cross-document validation works
- ✅ Time savings: 4-5 hours
- ✅ Data accuracy >95% after review
- ✅ Source documents linked to opportunity

**Validation Points:**
- Budget conflict resolution mechanism
- Cross-document data validation
- Confidence score accuracy
- User review workflow efficiency

---

#### TC-OPP-E2E-POS-007: Historical Data Migration with DST Benchmarking
**Priority:** P2  
**Category:** E2E Positive - Data Migration

**Business Scenario:**
UNOPS migrates 200 historical opportunities from legacy system to use for DST benchmarking.

**Test Flow:**
1. **Bulk Import Preparation:**
   - Export 200 opportunities from legacy system (oUP)
   - CSV file with 50 columns
   - Date range: 2020-2025 (5 years of data)
   
2. **Data Mapping:**
   - System provides mapping UI
   - Legacy fields mapped to new schema:
     - "Engagement Name" → "Opportunity Name"
     - "Total Amount" → "Estimated Value"
     - "Country" → "Primary Country"
     - 47 more mappings
   
3. **Validation Rules:**
   - System validates all 200 rows:
     - 185 opportunities: No errors
     - 12 opportunities: Missing required fields (flagged)
     - 3 opportunities: Invalid country codes (flagged)
   - User corrects 15 errors
   
4. **Batch Import:**
   - 200 opportunities imported
   - Processing time: ~5 minutes
   - All marked as "Historical" (not active)
   
5. **Retroactive DST Generation:**
   - System generates DST profiles for all 200
   - Uses historical country data ("as-at" dates)
   - Complexity scores calculated
   - Risk assessments done
   - Processing time: ~10 minutes (parallel processing)
   
6. **Historical Benchmarking:**
   - New opportunity created in Education sector, Afghanistan
   - DST profile generated
   
7. **Similar Project Matching:**
   - System searches 200 historical opportunities
   - Finds 8 similar opportunities:
     - 5 in Afghanistan
     - 3 in Education sector
     - 2 in both Afghanistan + Education
   - Similarity scores: 65% to 85%
   
8. **Lessons Learned Extraction:**
   - Historical opportunities had outcomes documented
   - System shows lessons from similar projects:
     - "Security challenges in Kandahar region"
     - "Local partner capacity building critical"
     - "6-month planning phase recommended"
   
9. **Risk Recommendations:**
   - Risks from historical projects recommended:
     - "Political instability" (occurred in 3 of 8)
     - "Currency fluctuations" (occurred in 5 of 8)
     - "Supply chain delays" (occurred in 4 of 8)
   
10. **Enhanced Decision Making:**
    - New opportunity benefits from 5 years of historical data
    - DST recommendations more accurate
    - Decision maker has better context

**Expected Results:**
- ✅ 200 historical opportunities imported successfully
- ✅ Validation catches 15 errors (7.5% error rate)
- ✅ DST profiles generated for all historical data
- ✅ Similar project matching works across historical data
- ✅ Lessons learned surfaced for new opportunities
- ✅ Historical risks inform new opportunity assessment
- ✅ Decision quality improved by historical context
- ✅ Import time: ~15 minutes total

---

#### TC-OPP-E2E-POS-008: Opportunity Cloning and Template Management
**Priority:** P2  
**Category:** E2E Positive - Efficiency

**Business Scenario:**
Organization frequently works on similar infrastructure opportunities - uses templates and cloning to speed up creation.

**Test Flow:**
1. **Template Creation:**
   - Completed high-quality opportunity: "Standard Road Rehabilitation"
   - Opportunity Manager selects "Save as Template"
   - Template includes:
     - Standard deliverables (13 items)
     - Default budget structure (8 categories)
     - Common risks (10 pre-defined)
     - Resource roles (PM, Engineers, Admin)
     - Timeline structure (4 phases)
   - Template named: "Road Infrastructure Template"
   - Marked as "Global Template" (available to all offices)
   
2. **Template Library:**
   - 5 templates available:
     - Road Infrastructure
     - Water Supply Systems
     - School Construction
     - Health Facility Rehabilitation
     - Community Capacity Building
   - Each with ratings and usage counts
   
3. **New Opportunity from Template:**
   - User creates opportunity: "Highway Rehabilitation - Region 5"
   - Selects "Road Infrastructure Template"
   - System copies structure:
     - 13 deliverables copied
     - Budget categories copied (values empty)
     - 10 risks copied (status: Draft)
     - Resource roles copied
     - Timeline copied (dates empty)
   
4. **Customization:**
   - User customizes for specific project:
     - Updates geography: Region 5 → 3 specific districts
     - Adjusts 4 deliverables for local context
     - Fills in budget values: $4.2M
     - Adds 2 region-specific risks
     - Sets timeline dates
   - Time to create: 30 minutes (vs 3 hours from scratch)
   
5. **Cloning Existing Opportunity:**
   - Different scenario: User has similar opportunity
   - "Highway Rehabilitation - Region 3" (completed)
   - User selects "Clone Opportunity"
   - System creates duplicate with suffix: "- Copy"
   - All data cloned:
     - Geography: Region 3 copied (will change)
     - Budget: $4.5M copied (will adjust)
     - Partners: Same partners copied
     - Documents: Links copied (not files)
   
6. **Clone Modification:**
   - User updates cloned opportunity:
     - Geography: Region 3 → Region 7
     - Budget: $4.5M → $5.2M (inflation)
     - Timeline: Shifted 6 months forward
     - Same partners confirmed
     - Documents relevant, kept
   - Time to create: 20 minutes
   
7. **Template Analytics:**
   - System tracks template usage:
     - "Road Infrastructure Template": Used 47 times
     - Average time savings: 2.5 hours per use
     - Total time saved: 117.5 hours
     - Success rate: 85% (opportunities reach Go decision)
   
8. **Template Updates:**
   - Best practices identified from 47 uses
   - Template updated with:
     - New risk: "Equipment procurement delays"
     - Additional deliverable: "Environmental impact assessment"
     - Updated budget category: "COVID-19 safety measures"
   - All future uses benefit from improvements

**Expected Results:**
- ✅ Templates save 2-3 hours per opportunity creation
- ✅ Cloning saves 2.5-3 hours per opportunity
- ✅ Template library organized and searchable
- ✅ Templates updated based on collective learning
- ✅ Consistency improved across similar opportunities
- ✅ 85% success rate for templated opportunities
- ✅ Analytics track template effectiveness

---

#### TC-OPP-E2E-POS-009: AI-Assisted Narrative Generation for Concept Note
**Priority:** P2  
**Category:** E2E Positive - AI Content Creation

**Business Scenario:**
Opportunity Manager uses AI to generate first draft of concept note and opportunity statement from structured data.

**Test Flow:**
1. **Structured Data Entry:**
   - Opportunity fully profiled with structured data:
     - Geography, budget, timeline, deliverables
     - Partners, stakeholders, risks
     - DST profile complete (9 parameters)
   
2. **AI Narrative Request:**
   - User clicks "Generate Concept Note"
   - System prompts: "Select sections to generate"
   - User selects:
     - Executive Summary
     - Background and Context
     - Project Objectives
     - Deliverables and Outputs
     - Implementation Approach
     - Risk Management
     - Budget Summary
   
3. **AI Generation:**
   - AI generates narrative from structured data:
     - **Executive Summary:** 200 words summarizing opportunity
     - **Background:** 300 words on country context (from country profile)
     - **Objectives:** SMART objectives from deliverables
     - **Deliverables:** Narrative description of 13 deliverables
     - **Approach:** Implementation phases from schedule
     - **Risk Management:** Narrative from risk register
     - **Budget:** Summary with cost breakdown
   - Generation time: 30 seconds
   
4. **Human Review and Edit:**
   - Opportunity Manager reviews generated content:
     - 85% accurate and acceptable
     - Makes edits:
       - Adds partner-specific language
       - Adjusts tone for government audience
       - Adds 2 policy references
       - Enhances objectives section
   - Editing time: 45 minutes (vs 4 hours from scratch)
   
5. **Iterative Refinement:**
   - User requests regeneration of "Background" section
   - Provides guidance: "Emphasize climate resilience"
   - AI regenerates with climate focus
   - User accepts updated version
   
6. **Multiple Versions:**
   - User generates 2 versions:
     - **Version A:** Technical, for UNOPS HQ review
     - **Version B:** Simplified, for government partner
   - Same data, different tone and detail level
   
7. **Opportunity Statement Generation:**
   - Similar process for internal Opportunity Statement
   - Includes DST insights, recommendations
   - Formatted per UNOPS template
   - Generation time: 20 seconds
   
8. **Version Control:**
   - All versions tracked
   - User can revert to any previous version
   - AI-generated vs human-edited sections marked
   
9. **Final Approval:**
   - Documents attached to opportunity
   - Included in decision package
   - Time saved: 3-4 hours total

**Expected Results:**
- ✅ AI generates 85% usable content
- ✅ Multiple versions for different audiences
- ✅ Iterative refinement works well
- ✅ Time savings: 3-4 hours per document
- ✅ Quality suitable for initial draft
- ✅ Human oversight ensures accuracy
- ✅ Version control maintained

---

### Category 3: Emergency and Fast-Track Scenarios (3 scenarios)

#### TC-OPP-E2E-POS-010: Emergency Fast-Track Approval
**Priority:** P1  
**Category:** E2E Positive - Emergency Process

**Business Scenario:**
Natural disaster in Nepal requires emergency humanitarian response - normal approval process must be expedited.

**Test Flow:**
1. **Emergency Declaration:**
   - Earthquake in Nepal (magnitude 7.2)
   - Regional Director declares "Emergency Response"
   - System creates emergency opportunity category
   
2. **Rapid Opportunity Creation:**
   - Opportunity Manager creates: "Nepal Earthquake Emergency Response"
   - Flagged as "Emergency" status
   - Triggers simplified workflow:
     - Reduced required fields (only critical info)
     - Expedited review process
     - Pre-approved budget ceiling: $500K (no approval needed)
     - 24-hour decision deadline
   
3. **Streamlined DST:**
   - DST profile generated with reduced parameters
   - Focuses on immediate risks only:
     - Safety risks
     - Access challenges
     - Partner capacity
   - Skip non-critical analysis (strategic alignment, long-term impact)
   - Generation time: 2 minutes (vs 10 minutes normal)
   
4. **Parallel Processing:**
   - Multiple activities occur simultaneously:
     - Budget estimated (rough order of magnitude)
     - Partners contacted for rapid mobilization
     - Equipment/supplies procurement initiated
     - Personnel identified and on standby
   
5. **Expedited Decision:**
   - Emergency authority: Regional Director (DOA2)
   - Normal process: DOA3 → DOA2 → DOA1
   - Emergency process: DOA2 directly
   - Decision time: 4 hours (vs 5-7 days normal)
   
6. **Conditional Go with Rolling Authorization:**
   - DOA2 approves emergency response
   - Initial authorization: $500K (immediate)
   - Conditions:
     - Full proposal within 2 weeks
     - Monthly justification for continued funding
     - Regular situation reports
   
7. **Rapid Mobilization:**
   - Project created immediately (no conversion delay)
   - Resources deployed within 24 hours
   - Procurement expedited
   - Normal compliance requirements waived temporarily
   
8. **Post-Emergency Transition:**
   - After 30 days, situation stabilized
   - Full opportunity developed retrospectively
   - Complete DST profile generated
   - Full approval process completed
   - Emergency project transitioned to regular project
   
9. **Audit Trail:**
   - All emergency decisions documented
   - Justifications captured
   - Exception approvals logged
   - Post-emergency review conducted

**Expected Results:**
- ✅ Emergency opportunity created in <1 hour
- ✅ Decision made in 4 hours (vs 5-7 days)
- ✅ $500K authorized immediately
- ✅ Resources deployed within 24 hours
- ✅ Simplified compliance maintained
- ✅ Full audit trail preserved
- ✅ Post-emergency transition to regular process
- ✅ Exception handling documented

**Validation Points:**
- Emergency triggers properly activated
- Approval workflow simplified appropriately
- Audit requirements still met
- Transition back to normal process works

---

#### TC-OPP-E2E-POS-011: Opportunity Amendment After Go Decision
**Priority:** P1  
**Category:** E2E Positive - Change Management

**Business Scenario:**
Opportunity receives Go decision, then major scope change requested by partner before project starts.

**Test Flow:**
1. **Initial Approval:**
   - Opportunity: "Urban Water Supply System" ($3M, 18 months)
   - Go decision recorded 2 weeks ago
   - Budget authorized
   - Project conversion pending (waiting for signed agreement)
   
2. **Change Request:**
   - Partner requests major scope change:
     - Add 2 additional districts (+$1.2M)
     - Extend timeline to 24 months
     - Add new partner organization
   - Change request submitted via system
   
3. **Amendment Initiation:**
   - Opportunity Manager creates "Amendment Request"
   - Amendment type: "Major Change" (triggers re-approval)
   - Original opportunity locked (no edits)
   - Amendment record created:
     - Links to original opportunity
     - Tracks changes:
       - Budget: $3M → $4.2M (+40%)
       - Geography: 3 districts → 5 districts
       - Timeline: 18 → 24 months
       - Partners: 1 → 2
   
4. **Impact Analysis:**
   - System performs impact analysis:
     - Budget increase: 40% (exceeds 25% threshold → requires new approval)
     - New geography: Requires DST update
     - Additional partner: Requires due diligence
     - Timeline extension: Minor impact
   - Assessment: "Major Amendment - Re-approval Required"
   
5. **DST Re-Profile:**
   - DST regenerated for 5 districts (not just 3)
   - New complexity score: 7.2 (was 6.1)
   - Additional risks identified:
     - "Coordination complexity increased"
     - "New districts have higher security risk"
   - Budget-complexity alignment checked
   
6. **Partner Due Diligence:**
   - New partner: Local NGO
   - Due diligence initiated
   - Checks:
     - Financial stability: ✅ Pass
     - Reputation: ✅ Good
     - Capacity assessment: ✅ Adequate
     - UNOPS blacklist: ✅ Clear
   - Due diligence time: 3 days
   
7. **Amended Budget:**
   - Budget updated to $4.2M
   - Additional $1.2M breakdown:
     - New district infrastructure: $800K
     - Partner organization costs: $200K
     - Coordination overhead: $150K
     - Contingency: $50K
   - Fee % maintained at 8%
   
8. **Re-Approval Process:**
   - Amendment requires DOA2 (not DOA3) due to 40% increase
   - Amendment package assembled:
     - Original opportunity + amendment
     - Updated DST profile
     - Partner due diligence report
     - Revised budget
     - Change justification from partner
   
9. **DOA2 Review:**
   - DOA2 reviews amendment
   - Questions:
     - "Why 40% increase?"
     - "Can new partner manage additional scope?"
   - Opportunity Manager provides answers
   - DOA2 satisfied
   
10. **Amended Go Decision:**
    - DOA2 approves amendment
    - New authorization: $4.2M
    - Conditions:
      - "Partner capacity monitored closely"
      - "Phase implementation (3 districts first, then 2)"
    - Amendment recorded in audit trail
    
11. **Opportunity Update:**
    - Original opportunity updated with amendment
    - Version history maintained:
      - Version 1.0: Original ($3M, 3 districts)
      - Version 2.0: Amended ($4.2M, 5 districts)
    - All stakeholders notified of changes
    
12. **Project Conversion:**
    - Project created with amended scope
    - Amendment history transferred to project
    - Partners notified of final scope

**Expected Results:**
- ✅ Amendment process clear and auditable
- ✅ Impact analysis automatic
- ✅ Appropriate re-approval triggered (DOA2 not DOA3)
- ✅ DST updated for new scope
- ✅ Partner due diligence completed
- ✅ Version history maintained
- ✅ All changes tracked in audit trail
- ✅ Project created with final amended scope
- ✅ Amendment time: 1 week (vs 4-6 weeks for new opportunity)

---

#### TC-OPP-E2E-POS-012: Same-Day Fast-Track Opportunity (Compressed Timeline)
**Priority:** P2  
**Category:** E2E Positive - Speed

**Business Scenario:**
Urgent funding opportunity requires proposal by end of day - system enables same-day opportunity development and approval.

**Test Flow:**
1. **Morning: Urgent Request (9 AM):**
   - Partner calls: "UN funding opportunity closes today at 5 PM"
   - Quick assessment: Fits UNOPS mandate
   - Decision: Pursue opportunity
   - Time available: 8 hours
   
2. **Rapid Creation (9:15 AM):**
   - Opportunity Manager creates opportunity
   - Uses similar opportunity as template (cloning)
   - Initial data entry: 30 minutes
   
3. **Parallel Processing (9:45 AM):**
   - Multiple team members work simultaneously:
     - **User A:** Refines deliverables and scope
     - **User B:** Develops rough budget
     - **User C:** Uploads partner agreement and concept note
     - **User D:** Coordinates with technical advisors
   
4. **AI-Accelerated Processing (10:00 AM):**
   - Uploaded documents processed by AI
   - 70% of fields auto-populated
   - Team reviews and confirms
   - Time saved: 2 hours
   
5. **Express DST (10:30 AM):**
   - DST profile generated
   - Focus on critical parameters only
   - Generation time: 3 minutes (parallel processing)
   - High-level risk assessment
   
6. **Compressed Review (11:00 AM):**
   - Technical advisor reviews (30 minutes)
   - Legal advisor quick review (30 minutes)
   - Both provide rapid feedback
   
7. **Budget Finalization (12:00 PM):**
   - Budget specialist finalizes numbers
   - Quick validation against partner requirements
   - Budget: $1.8M
   
8. **Express Decision Package (12:30 PM):**
   - Decision package auto-assembled
   - Quality check: All required sections present
   - Submitted for DOA3 review
   
9. **Accelerated Approval (1:00 PM):**
   - DOA3 reviews package (video call with team)
   - Questions answered in real-time
   - Decision made: "Go"
   - Conditions: "Full proposal within 2 weeks for detailed review"
   - Time: 1 hour (vs 3-5 days normal)
   
10. **Concept Note Generation (2:00 PM):**
    - AI generates concept note from structured data
    - Team reviews and refines
    - Formatted per partner's template
    - Time: 1.5 hours
    
11. **Final Quality Check (3:30 PM):**
    - Opportunity Manager reviews complete package
    - All sections validated
    - Partner requirements checklist completed
    
12. **Submission (4:30 PM):**
    - Concept note submitted to partner
    - 30 minutes before deadline
    - Opportunity marked "Submitted to Partner"

**Expected Results:**
- ✅ Complete opportunity developed in 8 hours
- ✅ All quality checks completed
- ✅ Decision obtained same day
- ✅ Concept note submitted on time
- ✅ Team collaboration efficient
- ✅ AI acceleration significant
- ✅ Compressed approval workflow appropriate
- ✅ Audit trail complete

**Success Factors:**
- Template/cloning capability
- AI document processing
- Parallel team collaboration
- Express DST option
- Accelerated approval pathway
- AI narrative generation

---

### Category 4: Data Integrity and Validation (3 scenarios)

#### TC-OPP-E2E-POS-013: Cross-System Data Synchronization
**Priority:** P1  
**Category:** E2E Positive - Integration

**Business Scenario:**
Opportunity data synchronized across multiple UNOPS systems (Opportunity+, ERP, PM Tool, HR System).

**Test Flow:**
1. **Opportunity Creation in Opportunity+:**
   - Create opportunity: "Education Infrastructure Programme"
   - Budget: $5M, Timeline: 3 years, Staff: 12 FTEs
   
2. **Go Decision:**
   - DOA2 approves
   - Budget authorized: $5M
   - Personnel authorized: 12 FTEs
   
3. **Project Conversion:**
   - Opportunity converted to Project
   - Triggers synchronization to external systems
   
4. **ERP System Sync:**
   - Project data pushed to ERP (OneUNOPS)
   - Created in ERP:
     - Project code assigned: PRJ-2026-0847
     - Budget structure created
     - Chart of accounts set up
     - $5M budget allocated
   - Sync validation: ✅ Success
   - Sync time: 2 minutes
   
5. **PM Tool Sync (MS Project Online):**
   - Project structure pushed to PM tool
   - Created:
     - Project workspace
     - High-level schedule (from opportunity schedule)
     - Milestones imported
     - Deliverables as work packages
   - Sync validation: ✅ Success
   - Sync time: 1 minute
   
6. **HR System Sync:**
   - Personnel requirements pushed to HR system
   - Created:
     - 12 position requests
     - Job descriptions (from resource plan)
     - Budget allocation per position
     - Recruitment workflow initiated
   - Sync validation: ✅ Success
   - Sync time: 30 seconds
   
7. **Document Repository Sync (SharePoint):**
   - All opportunity documents moved to project folder
   - Folder structure created:
     - /Project Documents/
       - /Opportunity Phase/
         - Concept Note
         - DST Profile
         - Decision Package
         - Partnership Agreement
       - /Planning/
       - /Implementation/
   - Sync validation: ✅ Success
   
8. **Bidirectional Updates:**
   - Project Manager updates schedule in PM Tool
   - Changes synchronized back to Opportunity+
   - Budget adjustments in ERP
   - Reflected in Opportunity+ project view
   
9. **Data Consistency Checks:**
   - System runs daily consistency checks:
     - Budget in Opportunity+ = Budget in ERP ✅
     - Timeline in Opportunity+ = Timeline in PM Tool ✅
     - Staff count in Opportunity+ = Positions in HR ✅
   - No discrepancies found
   
10. **Reporting Consolidation:**
    - Manager runs report across all systems
    - Consolidated view shows:
      - Financial data from ERP
      - Schedule status from PM Tool
      - Staffing status from HR
      - Risk data from Opportunity+
    - All data current (last sync: 2 minutes ago)

**Expected Results:**
- ✅ Data synchronized to 4 external systems
- ✅ All syncs successful (<5 minutes total)
- ✅ No data loss or corruption
- ✅ Bidirectional sync works
- ✅ Consistency checks pass
- ✅ Consolidated reporting available
- ✅ Real-time data across systems

---

#### TC-OPP-E2E-POS-014: Global Indices Update Cascade
**Priority:** P1  
**Category:** E2E Positive - Data Management

**Business Scenario:**
Annual global indices update (MVI, Fragile States, Corruption) cascades to all opportunities and triggers DST updates.

**Test Flow:**
1. **Annual Data Upload (January 2026):**
   - HQ uploads new global indices:
     - Multidimensional Vulnerability Index (MVI) - 2025 data
     - Fragile States Index (FSI) - 2025 data
     - Corruption Perceptions Index (CPI) - 2025 data
     - 193 countries updated
   
2. **Data Validation:**
   - System validates uploaded data:
     - All 193 countries present ✅
     - No missing values ✅
     - Value ranges valid ✅
     - Year = 2025 ✅
   - Validation passes
   
3. **Historical Archival:**
   - 2024 data archived
   - Marked as "Historical - 2024"
   - Still available for "as-at" queries
   - 2025 data becomes "Current"
   
4. **Impact Analysis:**
   - System identifies affected opportunities:
     - 47 opportunities in "Profiling" or "Decision" stage
     - Countries with significant index changes:
       - Afghanistan: FSI increased (+5 points → more fragile)
       - Bangladesh: MVI improved (-3 points → less vulnerable)
       - Somalia: CPI declined (+2 points → more corrupt)
   - 12 opportunities significantly affected
   
5. **Automatic Notifications:**
   - Opportunity Managers notified:
     - "Country index data updated for your opportunity"
     - "DST profile may be affected - review recommended"
     - "Changes: FSI +5, MVI -2, CPI +1"
   - 12 managers receive notifications
   
6. **DST Profile Updates:**
   - **Opportunity A (Afghanistan):**
     - Old DST: FSI = 92, Complexity = 7.5
     - New DST: FSI = 97, Complexity = 8.2
     - Risk score increased: 6.8 → 7.5
     - New risk recommendation: "Fragile context - enhanced security measures"
   
   - **Opportunity B (Bangladesh):**
     - Old DST: MVI = 28, Complexity = 6.2
     - New DST: MVI = 25, Complexity = 5.9
     - Risk score decreased: 6.5 → 6.1
     - Recommendation: "Improved conditions - lower contingency acceptable"
   
7. **Decision Package Impact:**
   - **Opportunity C (Somalia):**
     - Decision package assembled last week
     - Waiting for DOA2 review
     - CPI change triggers alert:
       - "Country indices updated since package assembly"
       - "DST profile outdated - regeneration recommended"
     - Opportunity Manager regenerates DST
     - Updated package sent to DOA2
   
8. **Historical Comparison:**
   - Reports generated showing year-over-year changes:
     - Countries improving: 42
     - Countries declining: 38
     - No change: 113
   - UNOPS portfolio risk analysis updated
   
9. **Business Rules Application:**
   - Opportunities in countries with FSI > 90 (very fragile):
     - Automatic risk: "Fragile state context"
     - Budget contingency minimum: 15% (was 10%)
     - Security assessment required
   - 8 opportunities affected by new business rules
   
10. **Audit Trail:**
    - All index changes logged
    - DST regenerations tracked
    - Business rule applications recorded
    - Opportunity Manager actions documented

**Expected Results:**
- ✅ 193 countries updated successfully
- ✅ Historical data preserved
- ✅ 47 opportunities identified as affected
- ✅ 12 significant impacts flagged
- ✅ Managers notified automatically
- ✅ DST profiles updated with new data
- ✅ Decision packages refreshed where needed
- ✅ Business rules applied automatically
- ✅ Complete audit trail maintained

---

#### TC-OPP-E2E-POS-015: Opportunity Lifecycle Audit and Compliance Report
**Priority:** P2  
**Category:** E2E Positive - Compliance

**Business Scenario:**
Internal audit requires complete lifecycle audit trail for opportunity from creation to project conversion.

**Test Flow:**
1. **Audit Request:**
   - Internal Audit requests full audit trail
   - Opportunity: "Health Systems Strengthening - Uganda"
   - Created: January 15, 2026
   - Converted to Project: March 20, 2026
   - Audit scope: All activities, decisions, changes
   
2. **Automated Audit Report Generation:**
   - System generates comprehensive audit report
   - Sections:
     - Creation and Initial Data Entry
     - Document Uploads and AI Extraction
     - DST Profile Generation and Review
     - Budget Development
     - Decision Package Assembly
     - Approval Workflow
     - Decision Recording
     - Authorization
     - Project Conversion
   
3. **Audit Trail Details:**
   
   **Creation (Jan 15, 9:23 AM):**
   - Created by: John Smith (Opportunity Manager)
   - IP Address: 10.45.123.67
   - Initial data: Name, Country, Estimated Value
   - Time: 14 minutes
   
   **Document Upload (Jan 15, 9:37 AM - 9:52 AM):**
   - 3 documents uploaded:
     - Concept Note (9:37 AM, John Smith)
     - Partner MOU (9:45 AM, John Smith)
     - Budget Estimate (9:52 AM, Jane Doe, Budget Specialist)
   - AI extraction: 9:53 AM - 9:57 AM (4 minutes)
   - Review: 10:00 AM - 10:15 AM (John Smith)
   - Accepted: 87% of extracted data, 13% corrected
   
   **DST Profile (Jan 16, 2:00 PM):**
   - Generated by: System (triggered by John Smith)
   - Complexity Score: 6.8
   - Risk Score: 6.2
   - Recommendations: 12 generated
   - Review: Jan 17, 9:00 AM (John Smith)
   - Actions: 8 recommendations accepted, 4 rejected with reasons
   
   **Budget Development (Jan 18-22):**
   - 47 edits by Jane Doe (Budget Specialist)
   - 12 edits by John Smith (Opportunity Manager)
   - Version history: 8 versions
   - Final budget: $2.4M (approved Jan 22, 4:00 PM)
   
   **Decision Package (Jan 25, 10:00 AM):**
   - Assembled by: John Smith
   - Package components:
     - Opportunity details ✅
     - DST profile ✅
     - Budget ✅
     - Schedule ✅
     - Risk register ✅
     - Partnership agreement ✅
   - Completeness check: Passed
   - Submitted to: DOA3 holder (Sarah Johnson)
   
   **Approval Workflow (Jan 26 - Feb 5):**
   - Submitted: Jan 26, 10:15 AM
   - Technical Advisor review: Jan 28, 2:00 PM (Mark Chen)
     - Comments: "Approve with minor budget adjustment"
   - Budget adjustment: Jan 29, 9:00 AM (John Smith + Jane Doe)
     - Change: $2.4M → $2.35M (-2%)
     - Reason: "Technical advisor recommended efficiency"
   - Legal Advisor review: Jan 30, 11:00 AM (Lisa Wong)
     - Comments: "Partnership agreement compliant - approve"
   - DOA3 review: Feb 5, 3:00 PM (Sarah Johnson)
     - Comments: "Well-prepared package - Go decision"
   
   **Go Decision (Feb 5, 3:30 PM):**
   - Decision: Go
   - Decision maker: Sarah Johnson (DOA3)
   - Rationale: "Strong alignment with UNOPS strategy, low risk, capable partner"
   - Conditions: None
   - Authorization: Budget $2.35M, Personnel 8 FTEs
   
   **Authorization (Feb 6, 9:00 AM):**
   - Budget authorized: Feb 6, 9:05 AM (Sarah Johnson)
   - Personnel authorized: Feb 6, 9:07 AM (Sarah Johnson)
   - Procurement authorization: Feb 6, 9:10 AM (Sarah Johnson)
   
   **Project Conversion (Feb 10, 10:00 AM):**
   - Converted by: John Smith
   - Project created: PRJ-2026-0215
   - All data transferred: ✅
   - ERP sync: ✅ (Feb 10, 10:02 AM)
   - PM Tool sync: ✅ (Feb 10, 10:03 AM)
   - HR System sync: ✅ (Feb 10, 10:04 AM)
   
4. **Access Log:**
   - Total accesses: 247
   - Users: 12 different users
   - Most frequent: John Smith (128 accesses)
   - External access: 0 (all internal)
   - Failed access attempts: 2 (both by unauthorized user, blocked)
   
5. **Data Integrity Checks:**
   - Budget consistency: $2.35M (all stages)
   - Country: Uganda (no changes)
   - Timeline: 36 months (no changes)
   - Partners: 2 (consistent throughout)
   - No unexplained data changes ✅
   
6. **Compliance Verification:**
   - All required approvals obtained ✅
   - DST profile generated and reviewed ✅
   - Budget within DOA3 authority ($2.35M < $5M limit) ✅
   - Partnership agreement validated ✅
   - Risk register maintained ✅
   - Decision rationale documented ✅
   - All conditions met (none specified) ✅
   
7. **Audit Report Findings:**
   - **No compliance issues identified**
   - **No unauthorized access**
   - **No data integrity violations**
   - **All approvals properly documented**
   - **Audit trail complete**
   - **Estimated manual audit time:** 8-10 hours
   - **Automated report generation time:** 2 minutes

**Expected Results:**
- ✅ Complete audit trail available
- ✅ All actions logged with user, timestamp, IP
- ✅ No compliance issues found
- ✅ Data integrity verified
- ✅ Approval workflow properly followed
- ✅ Report generated automatically (2 minutes)
- ✅ Audit-ready documentation
- ✅ Time saved: 8-10 hours vs manual audit

---

### Category 5: Programme and Portfolio Management (5 scenarios)

#### TC-OPP-E2E-POS-016: Opportunity to Programme Conversion with Multiple Components
**Priority:** P1  
**Category:** E2E Positive - Programme Management

**Business Scenario:**
Large multi-component opportunity approved as Programme with 4 distinct projects.

**Test Flow:**
1. **Large Opportunity Creation:**
   - Name: "Integrated Rural Development Programme - Mozambique"
   - Budget: $15M over 5 years
   - Scope: 4 main components:
     - Component 1: Agricultural Development ($5M)
     - Component 2: Market Infrastructure ($4M)
     - Component 3: Water & Sanitation ($3M)
     - Component 4: Community Capacity Building ($3M)
   
2. **Component-Based DST:**
   - DST analyses each component:
     - Agriculture: Complexity 7.5, Risk 6.8
     - Market Infrastructure: Complexity 6.2, Risk 5.5
     - Water & Sanitation: Complexity 6.8, Risk 6.2
     - Capacity Building: Complexity 5.1, Risk 4.8
   - Overall programme: Complexity 6.9, Risk 6.1
   
3. **Budget by Component:**
   - Budget specialist develops 4 component budgets
   - Each component has:
     - Personnel costs
     - Non-personnel costs
     - Fee structure (8%)
     - Contingency (10-15%)
   - Total: $15M
   
4. **Schedule by Component:**
   - Component 1-2: Years 1-3 (parallel)
   - Component 3: Years 2-4 (follows Component 1)
   - Component 4: Years 1-5 (throughout)
   - Dependencies mapped
   
5. **Go Decision (DOA1 Required):**
   - Size requires DOA1 (>$10M)
   - Decision package includes:
     - Overall programme logic
     - 4 component summaries
     - Integrated DST profile
     - Total budget $15M
     - 5-year timeline
   - DOA1 approves as Programme
   
6. **Programme Conversion:**
   - System recognizes: 4 components → Programme
   - Creates:
     - **1 Programme** entity (parent)
     - **4 Project** entities (children)
   - Structure:
     ```
     Programme: Integrated Rural Development ($15M)
     ├── Project 1: Agricultural Development ($5M)
     ├── Project 2: Market Infrastructure ($4M)
     ├── Project 3: Water & Sanitation ($3M)
     └── Project 4: Capacity Building ($3M)
     ```
   
7. **Budget Allocation:**
   - $15M programme budget split:
     - Projects: $14.2M (allocated to 4 projects)
     - Programme management: $500K (coordination)
     - Contingency: $300K (programme-level)
   
8. **Governance Structure:**
   - Programme Board established
   - Programme Manager assigned
   - 4 Project Managers assigned
   - Monthly steering committee
   - Quarterly board reviews
   
9. **Cross-Project Dependencies:**
   - Dependencies mapped:
     - Project 3 (Water) depends on Project 1 (Agriculture) completion
     - Project 4 (Capacity) supports all projects
   - System tracks dependencies
   - Alerts if dependencies at risk
   
10. **Consolidated Reporting:**
    - Programme-level dashboard shows:
      - Overall budget utilization: 0% (just started)
      - 4 project statuses: All "Planning"
      - Risk register: Programme-level + 4 project-level
      - Timeline: 5-year Gantt chart with all projects
    - Individual project dashboards also available

**Expected Results:**
- ✅ Programme created with 4 child projects
- ✅ Budget split correctly ($15M → 4 projects + management)
- ✅ Schedule dependencies mapped
- ✅ Governance structure established
- ✅ DST profile reflects programme complexity
- ✅ Consolidated reporting available
- ✅ Individual project autonomy maintained

---

#### TC-OPP-E2E-POS-017: Opportunity Progression Through All Lifecycle Stages
**Priority:** P0  
**Category:** E2E Positive - Complete Lifecycle

**Business Scenario:**
Opportunity progresses through every stage from creation to project implementation with all intermediate steps.

**Test Flow:**
1. **Stage 1: Draft (Day 1):**
   - Opportunity created: "School Rehabilitation - Ghana"
   - Status: Draft
   - Basic information entered
   - Saved for later completion
   
2. **Stage 2: Data Collection (Days 2-5):**
   - Multiple documents uploaded
   - Site visit photos added
   - Partner communications attached
   - Budget estimates collected
   - Technical specifications gathered
   
3. **Stage 3: Profiling (Days 6-10):**
   - All data organized
   - DST profile generated
   - Country context analyzed
   - Risks identified (12 risks)
   - Recommendations reviewed (8 accepted, 2 rejected)
   - Status: Profiling
   
4. **Stage 4: Budget Development (Days 11-15):**
   - Budget specialist develops detailed budget
   - 45 budget line items
   - $1.2M total
   - Fee structure: 10%
   - Contingency: 12%
   - Budget reviewed by 3 stakeholders
   - Status: Budget Development
   
5. **Stage 5: Schedule Development (Days 16-18):**
   - Work breakdown structure created
   - 34 work packages
   - 8 milestones
   - 18-month timeline
   - Critical path identified
   - Status: Schedule Development
   
6. **Stage 6: Risk Assessment (Days 19-21):**
   - Risk register finalized
   - 12 risks assessed
   - Mitigation plans for high risks
   - Risk owners assigned
   - Status: Risk Assessment
   
7. **Stage 7: Quality Review (Days 22-25):**
   - Technical advisor review
   - Legal advisor review
   - Procurement advisor review
   - Finance review
   - All feedback incorporated
   - Status: Quality Review
   
8. **Stage 8: Decision Package Assembly (Day 26):**
   - All components assembled
   - Completeness check: 100%
   - Opportunity Statement finalized
   - Concept Note finalized
   - Status: Ready for Decision
   
9. **Stage 9: Decision Workflow (Days 27-32):**
   - Submitted to DOA3
   - DOA3 reviews (2 days)
   - Questions asked and answered
   - DOA3 requests minor budget adjustment
   - Adjustment made
   - Status: Under Review
   
10. **Stage 10: Go Decision (Day 33):**
    - DOA3 approves
    - Decision: Go
    - Rationale documented
    - Status: Approved
    
11. **Stage 11: Authorization (Days 34-35):**
    - Budget authorized: $1.2M
    - Personnel authorized: 6 FTEs
    - Procurement authorized
    - Status: Authorized
    
12. **Stage 12: Pre-Conversion Preparation (Days 36-40):**
    - Project Manager designated
    - Team assembled
    - Project workspace set up
    - ERP account prepared
    - Status: Conversion Pending
    
13. **Stage 13: Project Conversion (Day 41):**
    - Opportunity converted to Project
    - Project code assigned: PRJ-2026-0654
    - All data transferred
    - Status: Converted (opportunity record archived)
    
14. **Stage 14: Project Initiation (Days 42-50):**
    - Detailed project plan developed
    - Kickoff meeting held
    - Partners engaged
    - Procurement initiated
    - Project Status: Initiated
    
15. **Stage 15: Implementation (Day 51+):**
    - Project implementation begins
    - Regular progress tracking
    - Monthly reporting
    - Project Status: Implementation

**Expected Results:**
- ✅ All 15 stages completed successfully
- ✅ Each stage properly documented
- ✅ Status transitions logical
- ✅ Complete audit trail
- ✅ No stages skipped
- ✅ Time: 51 days from creation to implementation
- ✅ All stakeholders engaged at appropriate stages
- ✅ Data integrity maintained throughout
- ✅ Smooth transition to project

**Validation Points:**
- Status field updated at each stage
- Required activities completed before moving forward
- Approvals obtained where needed
- Audit trail shows complete history

---

#### TC-OPP-E2E-POS-018: Bulk Opportunity Processing and Batch Decision
**Priority:** P2  
**Category:** E2E Positive - Batch Processing

**Business Scenario:**
Regional office has 15 small opportunities ($50K-$200K each) - processes as batch for efficiency.

**Test Flow:**
1. **Batch Creation:**
   - 15 opportunities created over 2 weeks
   - All similar: School minor renovations
   - Same country: Tanzania
   - Budget range: $50K-$200K
   - All use same template
   
2. **Batch DST Generation:**
   - Select all 15 opportunities
   - Trigger batch DST generation
   - System processes in parallel
   - Generation time: 5 minutes for all 15 (vs 150 minutes sequential)
   - All profiles complete
   
3. **Batch Quality Review:**
   - Technical advisor reviews all 15 together
   - Identifies common issues:
     - 3 opportunities: Budget slightly high
     - 2 opportunities: Timeline too aggressive
     - All others: Approved
   - Batch feedback given
   
4. **Batch Corrections:**
   - Opportunity Managers correct flagged issues
   - 3 budgets adjusted
   - 2 timelines extended
   - All resubmitted
   
5. **Batch Decision Package:**
   - System creates consolidated decision package
   - Shows all 15 opportunities:
     - Summary table (name, budget, status)
     - Total batch value: $2.1M
     - Consolidated risk assessment
     - Collective rationale
   
6. **Single Decision for Batch:**
   - DOA3 reviews batch package
   - Approves all 15 together
   - Single decision recorded
   - Rationale: "Low-risk, standardized school renovations"
   - Time: 1 hour (vs 15 hours for individual reviews)
   
7. **Batch Authorization:**
   - All 15 budgets authorized together
   - Personnel authorized (shared pool)
   - Total: $2.1M authorized
   
8. **Batch Project Conversion:**
   - All 15 converted to projects
   - Option 1: 15 individual projects
   - Option 2: 1 programme with 15 child projects
   - User selects: Programme approach
   - Programme created: "Tanzania Schools Renovation Programme"
   - 15 projects under programme
   
9. **Shared Resources:**
   - Programme Manager oversees all 15
   - Shared technical advisor
   - Shared procurement team
   - Efficiency gains from scale
   
10. **Consolidated Reporting:**
    - Programme dashboard shows all 15 projects
    - Aggregate statistics
    - Batch progress tracking

**Expected Results:**
- ✅ Batch processing 10x faster than individual
- ✅ DST generation time: 5 min (vs 150 min)
- ✅ Decision time: 1 hour (vs 15 hours)
- ✅ Programme structure creates efficiency
- ✅ Shared resources optimized
- ✅ All 15 opportunities processed successfully

---

#### TC-OPP-E2E-POS-019: Mobile Field Work and Offline Opportunity Management
**Priority:** P2  
**Category:** E2E Positive - Mobile/Offline

**Business Scenario:**
Opportunity Manager works in remote area with limited internet - uses mobile app with offline capability.

**Test Flow:**
1. **Field Preparation (Office - Online):**
   - Download Opportunity+ mobile app
   - Sync opportunity: "Rural Electrification - Remote Region"
   - Download for offline access:
     - Opportunity data
     - DST profile
     - Documents
     - Forms
   
2. **Travel to Field (Offline):**
   - Manager travels to remote village
   - No internet connectivity
   - Mobile app operates in offline mode
   - All synced data available
   
3. **Field Data Collection (Offline):**
   - Conducts site visits (3 villages)
   - Adds field notes for each village
   - Takes 47 photos with mobile camera
   - Records GPS coordinates
   - Updates deliverable descriptions based on actual site conditions
   - All changes stored locally on device
   
4. **Stakeholder Meetings (Offline):**
   - Meets with village leaders
   - Presents opportunity details (from cached data)
   - Collects feedback
   - Adds meeting notes
   - Updates partner information
   
5. **Budget Adjustments (Offline):**
   - Realizes costs are higher than expected (remote location)
   - Updates budget estimates
   - Adds 15% transportation surcharge
   - Budget: $800K → $950K
   - Changes saved locally
   
6. **Risk Assessment (Offline):**
   - Identifies new risks:
     - "Seasonal road inaccessibility (June-August)"
     - "Limited local technical capacity"
   - Adds 2 risks to register
   - Assigns severity scores
   
7. **Return to Office (Online):**
   - Manager returns to office (2 days later)
   - Connects to Wi-Fi
   - App automatically syncs:
     - Field notes
     - 47 photos (compressed upload)
     - Updated deliverables
     - Budget changes
     - New risks
     - GPS coordinates
   - Sync time: 8 minutes
   
8. **Conflict Resolution:**
   - During offline work, colleague made changes (budget notes)
   - System detects conflict
   - Shows both versions
   - Manager merges changes
   - Final version saved
   
9. **Data Validation:**
   - System validates synced data:
     - All photos uploaded ✅
     - Budget changes reasonable ✅
     - Risk register updated ✅
     - GPS coordinates valid ✅
   - No data corruption
   
10. **Workflow Continuation:**
    - Updated opportunity now available to all team
    - DST can be regenerated with new data
    - Budget approved at new amount
    - Field data enriches decision package

**Expected Results:**
- ✅ Offline mode works seamlessly
- ✅ All field data captured
- ✅ Sync successful (no data loss)
- ✅ Conflict resolution clear
- ✅ Photos uploaded (47 files)
- ✅ Field work efficiency improved
- ✅ No internet dependency during field work
- ✅ Data validated post-sync

---

#### TC-OPP-E2E-POS-020: Opportunity Recovery and Reactivation After Long Hold
**Priority:** P2  
**Category:** E2E Positive - Lifecycle Management

**Business Scenario:**
Opportunity placed on hold due to political situation, reactivated 18 months later when conditions improve.

**Test Flow:**
1. **Initial Opportunity (January 2025):**
   - Opportunity: "Infrastructure Development - Country X"
   - Status: Profiling
   - Budget: $4M
   - Progress: 60% complete (DST done, budget in progress)
   
2. **Political Crisis (February 2025):**
   - Country experiences political instability
   - UNOPS suspends operations
   - Opportunity Manager places opportunity "On Hold"
   - Reason: "Political instability - operations suspended"
   - On Hold date: February 15, 2025
   
3. **On Hold Management:**
   - Opportunity frozen at 60% completion
   - All data preserved
   - No further work allowed
   - Status: On Hold
   - Periodic reviews: Every 6 months
   
4. **Review Cycles:**
   - **August 2025 Review:**
     - Political situation still unstable
     - Decision: Remain on hold
     - Next review: February 2026
   
   - **February 2026 Review:**
     - Situation improving but not stable
     - Decision: Remain on hold
     - Next review: August 2026
   
   - **August 2026 Review:**
     - Situation stabilized
     - New government in place
     - UNOPS operations resumed
     - Decision: Reactivate opportunity
   
5. **Reactivation Process (August 2026):**
   - Opportunity Manager initiates reactivation
   - System checks time elapsed: 18 months
   - Triggers validation workflow
   
6. **Data Currency Validation:**
   - System checks all data currency:
     - **Country indices:**
       - MVI: Updated 2 times since hold
       - FSI: Updated 2 times since hold
       - CPI: Updated 2 times since hold
       - **Action:** Refresh required
     - **Partnership agreement:**
       - Valid until: December 2027
       - **Status:** Still valid ✅
     - **Budget estimates:**
       - Last updated: February 2025 (18 months ago)
       - **Action:** Inflation adjustment required
     - **Team availability:**
       - Original PM transferred to different region
       - **Action:** New PM assignment required
   
7. **DST Profile Regeneration:**
   - Old DST: February 2025 (18 months old)
   - System regenerates with current data:
     - Country: Now stable (FSI improved)
     - Complexity score: 7.2 → 6.1 (reduced)
     - Risk score: 8.5 → 6.8 (improved significantly)
   - Recommendations updated
   
8. **Budget Inflation Adjustment:**
   - Original budget: $4M (February 2025)
   - Inflation rate: 8% per year × 1.5 years = 12%
   - Adjusted budget: $4.48M
   - Budget specialist reviews and confirms
   
9. **Team Reassignment:**
   - Original PM transferred (notified)
   - New PM assigned: Lisa Johnson
   - Knowledge transfer:
     - Complete history available
     - Previous PM notes preserved
     - All documents accessible
   
10. **Stakeholder Re-engagement:**
    - Partners contacted after 18 months
    - Confirm continued interest: ✅ Yes
    - Update partner contacts (2 changed)
    - Refresh partnership agreement (still valid)
    
11. **Timeline Adjustment:**
    - Original start: March 2025
    - New start: October 2026
    - Timeline shifted 19 months
    - Dependencies recalculated
    - New end date: March 2028
    
12. **Validation Meeting:**
    - Reactivation review meeting held
    - Attendees: New PM, Budget Specialist, Regional Manager
    - Review:
      - Updated DST: Approved ✅
      - Adjusted budget: Approved ✅
      - New timeline: Approved ✅
      - Partner engagement: Confirmed ✅
    - Decision: Reactivate opportunity
    
13. **Reactivation:**
    - Status changed: On Hold → Active
    - Workflow resumes from 60% point
    - Remaining 40% of work continues
    - All stakeholders notified
    
14. **Updated Decision Package:**
    - Decision package assembled (now 100% complete)
    - Includes reactivation justification
    - Shows before/after comparison (2025 vs 2026)
    - Submitted to DOA3
    
15. **Go Decision (September 2026):**
    - DOA3 reviews reactivated opportunity
    - Approves with updated parameters
    - Budget authorized: $4.48M (adjusted)
    - Project conversion approved

**Expected Results:**
- ✅ 18-month hold period managed correctly
- ✅ All data preserved during hold
- ✅ Systematic reactivation process
- ✅ All validations completed:
  - Country indices refreshed
  - DST regenerated
  - Budget inflation-adjusted
  - Team reassigned
  - Partners re-engaged
- ✅ Complete audit trail of hold period
- ✅ Improved conditions recognized (risk reduction)
- ✅ Opportunity successfully reactivated
- ✅ Time to reactivate: 3 weeks
- ✅ Workflow resumed seamlessly

---

## 🔴 NEGATIVE END-TO-END SCENARIOS (20 scenarios)

### Category 1: System Failure and Recovery (5 scenarios)

#### TC-OPP-E2E-NEG-001: Database Connection Loss During Decision Recording
**Priority:** P0  
**Category:** E2E Negative - System Failure

**Business Scenario:**
Database connection lost during critical Go decision recording - system must handle gracefully.

**Test Flow:**
1. **Decision Recording Initiated:**
   - DOA3 reviews opportunity ($3M)
   - Decides: "Go"
   - Enters rationale
   - Clicks "Record Decision"
   
2. **Database Connection Lost:**
   - Mid-transaction: Database server goes down
   - Connection timeout: 30 seconds
   - Transaction not committed
   
3. **System Response:**
   - Error detected immediately
   - Transaction automatically rolled back
   - No partial data saved
   - User shown error message:
     - "Connection lost. Decision NOT saved. Please try again."
     - Rationale text preserved in browser cache
   
4. **Data Integrity Check:**
   - Verify opportunity status: Still "Pending Decision" ✅
   - Verify decision record: Not created ✅
   - Verify no orphan data ✅
   - Audit log: Shows attempted decision, failure, rollback
   
5. **Database Recovery:**
   - Database server restored (5 minutes)
   - System reconnects automatically
   - User refreshes page
   
6. **Retry Mechanism:**
   - User clicks "Record Decision" again
   - Rationale auto-filled from browser cache
   - Decision recording successful
   - Audit trail shows:
     - First attempt: Failed (connection loss)
     - Second attempt: Success
   
7. **Verification:**
   - Decision recorded correctly ✅
   - No duplicate decisions ✅
   - Status updated: "Approved" ✅
   - Authorization triggered ✅
   - Notifications sent ✅

**Expected Results:**
- ✅ Transaction rolled back automatically
- ✅ No data corruption
- ✅ Clear error message to user
- ✅ User's work (rationale) preserved
- ✅ Retry successful after recovery
- ✅ Audit trail complete
- ✅ No manual cleanup needed

---

#### TC-OPP-E2E-NEG-002: Cascading Failure - AI Service Down During Bulk Processing
**Priority:** P1  
**Category:** E2E Negative - External Dependency Failure

**Business Scenario:**
Gemini AI service down during bulk document processing for 25 opportunities - system handles gracefully.

**Test Flow:**
1. **Bulk Document Upload:**
   - User uploads 25 concept notes (one per opportunity)
   - Triggers batch AI extraction
   - First 5 documents: Processing ✅
   
2. **AI Service Failure:**
   - Gemini API returns 503 Service Unavailable
   - Remaining 20 documents: Processing failed
   - Error: "External AI service temporarily unavailable"
   
3. **Graceful Degradation:**
   - System doesn't crash
   - First 5 documents: Extraction complete, data saved ✅
   - Remaining 20 documents: Queued for retry
   - User notified:
     - "5 of 25 documents processed successfully"
     - "20 documents queued for automatic retry"
     - "You can continue working on processed opportunities"
   
4. **Partial Work Continuation:**
   - User can work on 5 successfully processed opportunities
   - 20 queued opportunities show status: "Processing Queued"
   - No blocking of user workflow
   
5. **Automatic Retry Mechanism:**
   - System retries every 5 minutes (exponential backoff)
   - Attempt 1 (5 min): Still down ❌
   - Attempt 2 (10 min): Still down ❌
   - Attempt 3 (20 min): Service recovered ✅
   
6. **Resume Processing:**
   - AI service back online
   - Queued 20 documents processed
   - Processing time: 8 minutes
   - All 20 successful ✅
   
7. **User Notification:**
   - Email sent: "All 25 documents processed successfully"
   - In-app notification
   - User can now work on all 25 opportunities
   
8. **Audit Trail:**
   - Logs show:
     - 5 processed immediately
     - AI service failure detected
     - 20 queued for retry
     - 3 retry attempts
     - Final success
   - Total time: 28 minutes (vs 10 minutes if no failure)
   
9. **Manual Fallback Option:**
   - If AI service down >1 hour:
     - User notified: "AI service extended downtime"
     - Option provided: "Enter data manually or wait for automatic retry"
     - User can choose to proceed manually

**Expected Results:**
- ✅ System doesn't crash on external service failure
- ✅ Partial work (5 docs) saved and available
- ✅ Automatic retry mechanism works
- ✅ User notified at each stage
- ✅ User can continue working (not blocked)
- ✅ All documents eventually processed
- ✅ Manual fallback available
- ✅ No data loss

---

#### TC-OPP-E2E-NEG-003: Data Corruption Detection and Recovery
**Priority:** P0  
**Category:** E2E Negative - Data Integrity

**Business Scenario:**
Data corruption detected in opportunity record - system identifies, isolates, and recovers.

**Test Flow:**
1. **Normal Operation:**
   - Opportunity: "Education Project" ($2M)
   - Status: Profiling stage
   - Multiple users editing
   
2. **Corruption Event:**
   - Database write error (disk issue)
   - Budget field corrupted: "$2,000,000" → "2#@!000~00"
   - Status field corrupted: "Profiling" → "Prof#ing"
   - Timestamp corrupted
   
3. **Detection:**
   - **Automatic validation:**
     - System's next read detects invalid data
     - Budget field validation fails (non-numeric characters)
     - Status field validation fails (invalid status value)
   - **Corruption flagged:** "Data integrity violation detected"
   
4. **Immediate Actions:**
   - Opportunity record locked (read-only)
   - All users notified: "Opportunity temporarily unavailable due to data issue"
   - Incident logged to system admin
   - Automated alert sent to IT team
   
5. **Isolation:**
   - Corrupted opportunity isolated
   - Other opportunities unaffected
   - No cascading corruption
   
6. **Recovery Attempt:**
   - **Step 1: Check backup:**
     - Last clean backup: 15 minutes ago
     - Backup data: Budget: "$2,000,000" ✅, Status: "Profiling" ✅
   
   - **Step 2: Check transaction log:**
     - Last valid transaction: 10 minutes ago
     - Transaction log intact ✅
   
   - **Step 3: Reconstruct:**
     - Restore fields from backup
     - Replay transactions from log
     - Verify checksums
   
7. **Data Restoration:**
   - Corrupted fields restored:
     - Budget: "$2,000,000" ✅
     - Status: "Profiling" ✅
     - Timestamp: Corrected ✅
   - Validation passed
   
8. **Integrity Verification:**
   - Run comprehensive integrity check
   - All fields validated
   - Relationships checked
   - Checksums verified
   - Result: Opportunity clean ✅
   
9. **Unlock:**
   - Opportunity unlocked
   - Users notified: "Opportunity restored and available"
   - Work can resume
   - Total downtime: 12 minutes
   
10. **Root Cause Analysis:**
    - Disk error identified
    - Faulty disk sector marked
    - Database maintenance scheduled
    
11. **Post-Recovery Audit:**
    - Verify no data lost
    - Check audit trail complete
    - Confirm all user edits preserved
    - Result: Zero data loss ✅

**Expected Results:**
- ✅ Corruption detected automatically
- ✅ Opportunity isolated (no spread)
- ✅ Recovery successful from backup + log
- ✅ Zero data loss
- ✅ Downtime minimal (12 minutes)
- ✅ Users informed throughout
- ✅ Audit trail complete
- ✅ Root cause identified

---

#### TC-OPP-E2E-NEG-004: Network Partition During Multi-User Collaboration
**Priority:** P1  
**Category:** E2E Negative - Network Failure

**Business Scenario:**
Network partition splits users during active collaboration - system handles split-brain scenario.

**Test Flow:**
1. **Active Collaboration:**
   - 3 users working on same opportunity:
     - User A (HQ Office): Budget updates
     - User B (Regional Office): Deliverables
     - User C (Field Office): Risk assessment
   
2. **Network Partition:**
   - Network failure splits users into 2 groups:
     - **Group 1 (Connected to DB):** User A, User B
     - **Group 2 (Isolated):** User C
   - User C loses connection but app continues in degraded mode
   
3. **Group 1 Continues:**
   - User A saves budget: $2.5M
   - User B saves deliverables: 12 items
   - Both saved to database ✅
   
4. **Group 2 (Isolated):**
   - User C working offline (unaware of network partition)
   - Adds 3 new risks
   - Attempts to save
   - Save queued locally (can't reach database)
   - App shows: "Changes saved locally - will sync when connected"
   
5. **Network Restored:**
   - Network partition healed (20 minutes later)
   - User C reconnects
   - App attempts automatic sync
   
6. **Conflict Detection:**
   - System detects conflicting versions:
     - **Server version:** Budget $2.5M, 12 deliverables (from Users A, B)
     - **User C version:** Old budget $2.2M, old deliverables 10, + 3 new risks
   - Conflicts:
     - Budget: Client $2.2M vs Server $2.5M
     - Deliverables: Client 10 vs Server 12
   
7. **Conflict Resolution UI:**
   - User C shown conflict resolution screen:
     ```
     Field: Budget
     Your version (offline): $2.2M
     Current version (server): $2.5M (updated by User A)
     ☐ Keep your version
     ☑ Accept server version
     
     Field: Deliverables
     Your version (offline): 10 items
     Current version (server): 12 items (updated by User B)
     ☐ Keep your version
     ☑ Accept server version
     
     Field: Risk Register
     Your version (offline): 15 risks (you added 3)
     Current version (server): 12 risks
     ☑ Merge (keep your 3 new risks)
     ```
   
8. **User Resolution:**
   - User C selects:
     - Budget: Accept server ($2.5M)
     - Deliverables: Accept server (12 items)
     - Risks: Merge (keep 3 new, total 15)
   - Conflict resolved
   
9. **Final Sync:**
   - Changes synced to server:
     - Budget: $2.5M ✅
     - Deliverables: 12 items ✅
     - Risks: 15 (12 original + 3 new from User C) ✅
   - Version reconciled
   
10. **All Users Updated:**
    - Users A, B see User C's 3 new risks
    - All users on same version
    - Audit trail shows:
      - Network partition event
      - User C offline work
      - Conflict resolution
      - Final merge

**Expected Results:**
- ✅ Network partition handled gracefully
- ✅ User C can work offline
- ✅ No data loss from any user
- ✅ Conflicts detected automatically
- ✅ User-friendly conflict resolution
- ✅ Final state consistent across all users
- ✅ Audit trail complete

---

#### TC-OPP-E2E-NEG-005: System Overload During Peak Usage
**Priority:** P1  
**Category:** E2E Negative - Performance

**Business Scenario:**
End-of-quarter rush - 500 users submit opportunities simultaneously causing system overload.

**Test Flow:**
1. **Peak Load Event:**
   - Last day of quarter (March 31)
   - 500 users submitting opportunities for quarterly targets
   - Simultaneous requests spike: 500 requests/second
   
2. **System Stress:**
   - Database connections: 450/500 (90% capacity)
   - CPU utilization: 92%
   - Memory: 85%
   - Disk I/O: High
   - Response time degrading: 2s → 15s → 45s
   
3. **Threshold Breach:**
   - Response time > 30 seconds
   - System enters "High Load Mode"
   - Automatic actions triggered:
     - Non-critical background jobs paused
     - Cache hit ratio increased
     - Read replicas prioritized
     - Resource allocation optimized
   
4. **Queue Management:**
   - **Critical operations** (immediate processing):
     - Save opportunity data
     - Record decisions
     - User authentication
   - **Non-critical operations** (queued):
     - DST profile generation (queued)
     - Document AI extraction (queued)
     - Report generation (queued)
     - Email notifications (queued)
   
5. **User Notification:**
   - Users shown banner:
     - "System experiencing high load"
     - "Your changes are being saved"
     - "Some features may be slower"
     - "DST and reports queued for processing"
   
6. **Graceful Degradation:**
   - **Still working:**
     - Create/edit opportunities ✅
     - Save data ✅
     - View existing data ✅
     - Record decisions ✅
   - **Temporarily slower:**
     - DST generation: Queued (estimated wait: 15 minutes)
     - Document extraction: Queued
     - Complex reports: Queued
   - **Not affected:**
     - Data integrity ✅
     - Security ✅
     - Audit logging ✅
   
7. **Auto-Scaling:**
   - Cloud infrastructure auto-scales:
     - Additional database read replicas: +3
     - Additional app servers: +5
     - Scaling time: 8 minutes
   
8. **Load Distribution:**
   - New resources online
   - Load distributed across 8 app servers (was 3)
   - Database queries distributed across 5 replicas (was 2)
   - Response time improves: 45s → 12s → 4s
   
9. **Queue Processing:**
   - Once load reduces, queued items processed:
     - 147 DST profiles queued → Processing in parallel
     - 89 document extractions queued → Processing
     - 234 reports queued → Processing
   - Processing time: 25 minutes
   
10. **System Recovery:**
    - Load returns to normal
    - All queued items processed
    - Auto-scaling reverses (resources released)
    - System back to normal state
    - Total peak period: 45 minutes
    
11. **User Impact Assessment:**
    - **No data loss:** 0 opportunities lost ✅
    - **All saves successful:** 100% ✅
    - **Queued items processed:** 100% ✅
    - **Average wait time:** 18 minutes (for queued items)
    - **User satisfaction:** Acceptable (given high load)
    
12. **Post-Mortem:**
    - Incident reviewed
    - Actions:
      - Increase base capacity before next quarter-end
      - Improve queue visibility to users
      - Optimize slow queries
      - Better load prediction

**Expected Results:**
- ✅ System remains operational under extreme load
- ✅ No data loss or corruption
- ✅ Critical functions maintained
- ✅ Graceful degradation for non-critical functions
- ✅ Clear user communication
- ✅ Auto-scaling successful
- ✅ All queued items eventually processed
- ✅ System recovers fully
- ✅ 500 users' work completed (though slower)

---

### Category 2: Authorization and Security Failures (5 scenarios)

#### TC-OPP-E2E-NEG-006: Authorization Revoked Mid-Workflow
**Priority:** P0  
**Category:** E2E Negative - Security

**Business Scenario:**
DOA holder's authority revoked while they're in middle of reviewing and approving an opportunity.

**Test Flow:**
1. **Review Started:**
   - DOA3 (Sarah) starts reviewing $2M opportunity
   - Opens decision package at 2:00 PM
   - Spends 30 minutes reviewing
   
2. **Authority Revocation:**
   - At 2:20 PM: HR updates Sarah's DOA level
   - Sarah moved from DOA3 ($5M limit) to DOA4 ($500K limit)
   - Reason: Organizational restructuring
   - Effective immediately
   
3. **Continued Review:**
   - Sarah continues reviewing (unaware of revocation)
   - At 2:30 PM: Decides to approve
   - Enters rationale
   - Clicks "Record Go Decision"
   
4. **Authorization Check:**
   - System performs real-time authority check
   - Checks Sarah's current DOA level: DOA4 ($500K limit)
   - Opportunity value: $2M
   - **Validation fails:** $2M > $500K
   
5. **Decision Blocked:**
   - System blocks decision
   - Error message shown:
     - "Authorization Insufficient"
     - "Your DOA level was recently updated"
     - "Current limit: $500K"
     - "Opportunity value: $2M"
     - "Requires DOA3 or higher"
     - "Decision not recorded"
   
6. **User Notification:**
   - Sarah notified of DOA change
   - Email from HR with details
   - In-app notification explaining change
   
7. **Automatic Escalation:**
   - System automatically escalates opportunity
   - Identifies new DOA3 holder: Mark Chen
   - Escalation note: "Originally assigned to Sarah Johnson (DOA3), escalated due to authority change (now DOA4)"
   - Mark notified
   
8. **Sarah's Work Preserved:**
   - Sarah's review comments saved (not lost)
   - Rationale preserved as "Draft rationale from Sarah Johnson"
   - Mark can see Sarah's input
   
9. **New Review:**
   - Mark reviews opportunity
   - Sees Sarah's draft rationale
   - Can use or modify
   - Approves opportunity
   - Decision recorded by Mark (DOA3)
   
10. **Audit Trail:**
    - Complete history recorded:
      - Sarah assigned (2:00 PM)
      - Sarah reviewed (2:00-2:30 PM)
      - Sarah's DOA changed (2:20 PM)
      - Decision attempt blocked (2:30 PM)
      - Auto-escalated to Mark (2:31 PM)
      - Mark approved (3:00 PM)

**Expected Results:**
- ✅ Real-time authorization check prevents invalid decision
- ✅ User notified clearly
- ✅ Automatic escalation to correct authority
- ✅ No work lost (comments preserved)
- ✅ Audit trail complete
- ✅ Security maintained
- ✅ Eventually approved by authorized person

---

#### TC-OPP-E2E-NEG-007: Session Hijacking Attempt Detected
**Priority:** P0  
**Category:** E2E Negative - Security

**Business Scenario:**
Attacker attempts to hijack user session to approve unauthorized opportunity.

**Test Flow:**
1. **Legitimate User Session:**
   - DOA2 (Lisa) logs in from office (IP: 10.45.123.45)
   - Reviews opportunities
   - Session token: abc123xyz
   
2. **Session Token Stolen:**
   - Attacker intercepts session token (man-in-the-middle)
   - Attacker attempts to use token from different location (IP: 185.220.101.13 - suspicious)
   
3. **Hijack Attempt:**
   - Attacker uses stolen token to access system
   - Attempts to approve $5M opportunity
   - Different IP address
   - Different user-agent (different browser/device)
   
4. **Anomaly Detection:**
   - System's security monitoring detects:
     - **IP address change:** Office IP → Suspicious foreign IP
     - **Geographic impossibility:** Office (New York) → Foreign country in 2 minutes
     - **User-agent change:** Chrome/Windows → Firefox/Linux
     - **Behavior anomaly:** Sudden high-value approval without review time
   - **Risk score:** Critical
   
5. **Immediate Response:**
   - Session invalidated immediately
   - All tokens for user revoked
   - User logged out on all devices
   - Approval attempt blocked
   - No decision recorded
   
6. **User Notification:**
   - Lisa receives:
     - Email: "Security alert - suspicious activity detected"
     - SMS: "Your session was terminated due to suspicious activity"
     - In-app notification (on re-login)
   
7. **Security Lockout:**
   - Lisa's account temporarily locked (30 minutes)
   - Requires password reset to unlock
   - Two-factor authentication required
   
8. **Incident Logging:**
   - Security incident logged:
     - Timestamp of hijack attempt
     - Suspicious IP address
     - Attempted actions
     - System response
   - Security team notified
   
9. **Investigation:**
   - Security team investigates:
     - Suspicious IP traced to known attack source
     - Network scan initiated
     - Vulnerability assessment
   
10. **User Recovery:**
    - Lisa resets password
    - Enables two-factor authentication
    - Logs in securely
    - Reviews audit log of suspicious activity
    - Confirms: "I did not attempt those actions"
    
11. **Prevented Damage:**
    - No unauthorized approvals ✅
    - No data accessed by attacker ✅
    - No data modified ✅
    - Legitimate user account secured ✅

**Expected Results:**
- ✅ Hijack attempt detected immediately
- ✅ Session terminated before damage
- ✅ User notified promptly
- ✅ Account secured automatically
- ✅ No unauthorized actions completed
- ✅ Full audit trail
- ✅ Incident investigated

---

(Continuing with remaining negative scenarios...)

#### TC-OPP-E2E-NEG-008: Insufficient Resources for Bulk DST Generation
**Priority:** P1  
**Category:** E2E Negative - Resource Exhaustion

**Business Scenario:**
System attempts to generate DST profiles for 200 opportunities simultaneously, causing memory exhaustion.

**Test Flow:**
1. **Bulk Operation Initiated:**
   - Regional Manager selects 200 opportunities
   - Clicks "Generate DST Profiles for All"
   - System starts parallel processing
   
2. **Resource Consumption:**
   - Each DST generation: ~150MB memory
   - 200 simultaneous: ~30GB required
   - Server memory: 32GB total, 28GB available
   - Memory exhaustion approaching
   
3. **Resource Limit Detection:**
   - System monitors resource usage
   - Memory usage: 25GB → 27GB → 28GB → Critical threshold
   - Alert triggered: "Memory threshold exceeded"
   
4. **Automatic Throttling:**
   - System stops accepting new DST requests
   - Currently processing: 180 profiles
   - Queued: 20 profiles
   - Message shown: "Processing capacity reached - remaining items queued"
   
5. **Batch Processing:**
   - First 180 profiles completed
   - Memory released
   - Next batch of 20 starts automatically
   - All 200 complete eventually
   
6. **User Notification:**
   - Progress bar shown: "185 of 200 complete"
   - Estimated time remaining updated
   - No system crash
   - All profiles generated successfully
   
7. **System Recovery:**
   - Memory released after completion
   - System returns to normal
   - No data loss ✅
   - All 200 profiles available ✅

**Expected Results:**
- ✅ Resource exhaustion prevented
- ✅ Automatic throttling engaged
- ✅ Batch processing successful
- ✅ No system crash
- ✅ All 200 profiles eventually generated
- ✅ Clear progress indication
- ✅ System recovers fully

---

#### TC-OPP-E2E-NEG-009: Conflicting Simultaneous Decisions by Different DOA Holders
**Priority:** P1  
**Category:** E2E Negative - Race Condition

**Business Scenario:**
Opportunity incorrectly routed to 2 DOA holders - both approve simultaneously causing conflict.

**Test Flow:**
1. **Routing Error:**
   - Opportunity: $3M
   - Due to system glitch, routed to both:
     - DOA3 holder A (Sarah)
     - DOA3 holder B (Michael)
   - Both receive notification
   
2. **Simultaneous Review:**
   - Both review independently
   - Sarah: Decides "Go" at 2:30:15 PM
   - Michael: Decides "No-Go" at 2:30:17 PM
   - 2-second difference
   
3. **First Decision (Sarah):**
   - Sarah's "Go" decision submitted first
   - System accepts and commits
   - Decision ID: DEC-001
   - Opportunity status: "Approved"
   
4. **Second Decision (Michael):**
   - Michael's "No-Go" submitted 2 seconds later
   - System detects existing decision
   - **Conflict detected**
   
5. **Conflict Resolution:**
   - System blocks Michael's decision
   - Error shown:
     - "Decision Already Recorded"
     - "Sarah Johnson approved this opportunity 2 seconds ago"
     - "Your decision was not saved"
     - "Please contact Sarah or system admin"
   
6. **Notification:**
   - Both users notified of conflict
   - Admin notified of dual-routing error
   - Opportunity Manager notified
   
7. **Resolution Process:**
   - System admin reviews
   - Identifies routing error
   - Consults with both DOA holders
   - Decision: Sarah's "Go" decision stands
   - Michael's opinion noted in audit trail
   
8. **System Fix:**
   - Routing logic bug fixed
   - Prevents future dual-routing
   - Audit trail shows complete history

**Expected Results:**
- ✅ First decision accepted
- ✅ Second decision blocked
- ✅ No conflicting decisions in system
- ✅ Both users notified
- ✅ Audit trail shows attempted conflict
- ✅ Resolution process clear
- ✅ Root cause fixed

---

#### TC-OPP-E2E-NEG-010: Expired Partnership Agreement Used in Opportunity
**Priority:** P1  
**Category:** E2E Negative - Data Validation

**Business Scenario:**
User creates opportunity using partnership agreement that expired yesterday - system detects and prevents.

**Test Flow:**
1. **Opportunity Creation:**
   - User creates opportunity: "Infrastructure Project"
   - Links to partnership agreement: "MOU with Partner X"
   - Agreement validity: January 1, 2024 - December 31, 2025
   - Today's date: January 1, 2026 (expired)
   
2. **Initial Warning:**
   - System detects expired agreement
   - Warning shown:
     - "Partnership agreement expired: December 31, 2025"
     - "Opportunity may not proceed without valid agreement"
     - "Contact partnerships team to renew or find alternative"
   
3. **Continued Development:**
   - User proceeds anyway (warning, not error)
   - Completes opportunity details
   - Generates DST profile
   - Prepares budget
   
4. **Validation at Submission:**
   - User attempts to submit for decision
   - System performs comprehensive validation
   - **Critical validation fails:**
     - "Partnership agreement expired"
     - "Cannot proceed to decision without valid agreement"
   - Submission blocked
   
5. **User Actions:**
   - **Option 1:** Contact partnerships team to renew agreement
   - **Option 2:** Remove expired agreement, find new partner
   - **Option 3:** Request exception approval
   
6. **Exception Process:**
   - User requests exception from Regional Director
   - Justification: "Renewal in progress, expected next week"
   - Regional Director reviews
   - Approves temporary exception (valid 2 weeks)
   
7. **Proceed with Exception:**
   - Opportunity submitted with exception approval
   - Decision package includes:
     - Expired agreement (noted)
     - Exception approval
     - Renewal timeline
   
8. **Decision with Conditions:**
   - DOA reviews
   - Approves with condition: "Valid renewed agreement required before project start"
   - Decision recorded
   
9. **Follow-up:**
   - Partnership team renews agreement (1 week later)
   - New agreement: January 1, 2026 - December 31, 2028
   - Opportunity updated with renewed agreement
   - Condition satisfied
   - Project conversion approved

**Expected Results:**
- ✅ Expired agreement detected
- ✅ Clear warning provided
- ✅ Submission blocked without exception
- ✅ Exception process available
- ✅ Decision includes conditions
- ✅ Follow-up tracked
- ✅ Prevents proceeding with invalid agreement

---

### Category 3: Data Inconsistency and Validation Failures (5 scenarios)

#### TC-OPP-E2E-NEG-011: Budget-DST Misalignment Detected
**Priority:** P1  
**Category:** E2E Negative - Data Consistency

**Business Scenario:**
Opportunity budget is $500K but DST complexity suggests $2M+ project - system flags severe misalignment.

**Test Flow:**
1. **Opportunity Creation:**
   - Name: "National Healthcare Infrastructure Upgrade"
   - Scope: 50 health facilities across 10 regions
   - Timeline: 3 years
   - Budget entered: $500,000
   
2. **DST Profile Generation:**
   - System analyzes scope:
     - 50 facilities = High scope
     - 10 regions = High geographic spread
     - 3 years = Long timeline
     - Healthcare sector = High complexity
   - DST Complexity Score: 8.5 (Very High)
   - Risk Score: 7.2 (High)
   
3. **Budget-Complexity Analysis:**
   - System compares budget to complexity
   - Historical data: Similar projects (complexity 8-9) average $2.5M
   - Benchmark range for complexity 8.5: $2M - $3M
   - Entered budget: $500K
   - **Severe misalignment detected:** -75% below benchmark
   
4. **Automatic Alert:**
   - System generates critical alert:
     - "⚠️ Budget-Complexity Misalignment Detected"
     - "Your budget ($500K) is significantly lower than expected for this complexity level"
     - "Similar projects: $2M - $3M"
     - "Recommendation: Review budget or reduce scope"
   - Alert shown to Opportunity Manager
   
5. **Validation Failure:**
   - Opportunity cannot proceed to decision
   - Status: "Validation Required"
   - Blocking issues:
     - Budget misalignment
     - Feasibility concern
   
6. **Manager Review:**
   - Opportunity Manager investigates
   - Discovers error: Typed "$500K" instead of "$5M"
   - Actual budget: $5,000,000 (not $500,000)
   
7. **Budget Correction:**
   - Manager corrects budget: $500K → $5M
   - DST re-evaluates
   - New alignment check: $5M vs $2M-$3M benchmark
   - **New status:** +67% above benchmark (high but acceptable for 50 facilities)
   - Alert cleared
   
8. **Alternative Scenario (Scope Reduction):**
   - If budget was actually $500K (not a typo):
     - Manager must reduce scope:
       - 50 facilities → 10 facilities
       - 10 regions → 2 regions
     - DST regenerated
     - New complexity: 5.2 (Medium)
     - Benchmark for 5.2: $400K - $700K
     - $500K now aligned ✅
   
9. **Validation Passed:**
   - Budget-complexity aligned
   - Opportunity can proceed
   - Submitted for decision

**Expected Results:**
- ✅ Severe misalignment detected automatically
- ✅ Submission blocked until resolved
- ✅ Clear guidance provided
- ✅ Manager corrects issue
- ✅ Validation passes after correction
- ✅ Prevents unrealistic budgets from approval

---

#### TC-OPP-E2E-NEG-012: Geography-DST Country Data Mismatch
**Priority:** P1  
**Category:** E2E Negative - Data Validation

**Business Scenario:**
Opportunity listed for Country A but document extractions mention Country B - system detects inconsistency.

**Test Flow:**
1. **Opportunity Creation:**
   - User creates opportunity
   - Primary Country: "Tanzania"
   - Uploads 3 documents:
     - Concept note
     - Partner agreement
     - Feasibility study
   
2. **AI Document Extraction:**
   - AI processes documents
   - **Extracted country mentions:**
     - Document 1: "Kenya" (mentioned 15 times)
     - Document 2: "Kenya" (mentioned 8 times)
     - Document 3: "Kenya" (mentioned 22 times)
     - "Tanzania" mentioned: 0 times
   
3. **Geographic Inconsistency Detection:**
   - System compares:
     - Opportunity primary country: Tanzania
     - Document content: Kenya (45 mentions)
   - **Critical mismatch detected**
   
4. **Validation Alert:**
   - System flags opportunity:
     - "⚠️ Geographic Inconsistency"
     - "Primary country: Tanzania"
     - "Documents primarily mention: Kenya (45 times)"
     - "Possible data entry error"
   
5. **User Notification:**
   - Opportunity Manager notified
   - Shown evidence:
     - Document excerpts mentioning Kenya
     - Country mention frequency chart
   
6. **Manager Investigation:**
   - Reviews documents
   - Discovers: Uploaded wrong documents (Kenya project docs)
   - Correct action: Delete wrong docs, upload Tanzania docs
   
7. **Alternative Scenario (Correct Entry Error):**
   - Documents are correct (Kenya project)
   - Primary country was entered wrong (should be Kenya, not Tanzania)
   - Manager corrects: Tanzania → Kenya
   - Inconsistency resolved
   
8. **DST Impact:**
   - DST profile re-generated with correct country (Kenya)
   - Different country indices:
     - Tanzania: MVI 35, FSI 72, CPI 38
     - Kenya: MVI 32, FSI 68, CPI 31
   - Complexity and risk scores adjusted
   - Recommendations updated
   
9. **Validation Passed:**
   - Geographic consistency verified
   - Opportunity can proceed

**Expected Results:**
- ✅ Geographic mismatch detected automatically
- ✅ Evidence provided to user
- ✅ User corrects issue
- ✅ DST updated with correct country
- ✅ Prevents wrong country data in decision-making

---

#### TC-OPP-E2E-NEG-013: Timeline-Budget Phasing Conflict
**Priority:** P1  
**Category:** E2E Negative - Data Consistency

**Business Scenario:**
Budget phasing doesn't align with schedule phases - causing implementation impossibility.

**Test Flow:**
1. **Schedule Created:**
   - Opportunity: "Road Rehabilitation"
   - Timeline: 18 months
   - Phases:
     - Phase 1 (Months 1-6): Design & Planning
     - Phase 2 (Months 7-12): Procurement & Mobilization
     - Phase 3 (Months 13-18): Construction
   
2. **Budget Created:**
   - Total: $3M
   - Budget phasing (by year):
     - Year 1: $200K (7%)
     - Year 2: $2.8M (93%)
   
3. **Phasing Inconsistency Detection:**
   - System analyzes:
     - Phase 1-2 (Months 1-12 = Year 1): Requires equipment procurement, mobilization
     - Budget Year 1: Only $200K (7%)
     - **Critical issue:** Cannot procure equipment and mobilize with only 7% of budget
   
4. **Alert Generated:**
   - "⚠️ Budget-Schedule Misalignment"
   - "Phase 2 (Procurement & Mobilization) in Year 1 requires significant budget"
   - "Year 1 budget: $200K (7%) - Insufficient"
   - "Typical procurement phase: 20-30% of total budget"
   - "Recommendation: Adjust budget phasing"
   
5. **Manager Review:**
   - Reviews phasing
   - Realizes error: Budget entered as fiscal year, schedule as project months
   - Fiscal year ≠ Project year misalignment
   
6. **Correction:**
   - Budget re-phased by project timeline:
     - Months 1-6 (Phase 1): $400K (13%) - Design
     - Months 7-12 (Phase 2): $800K (27%) - Procurement
     - Months 13-18 (Phase 3): $1.8M (60%) - Construction
   - Now aligned with schedule ✅
   
7. **Cash Flow Analysis:**
   - System generates projected cash flow
   - Shows monthly spend rate
   - Validates feasibility
   - All phases adequately funded ✅
   
8. **Validation Passed:**
   - Budget-schedule alignment verified
   - Opportunity can proceed

**Expected Results:**
- ✅ Timeline-budget conflict detected
- ✅ Specific issue identified
- ✅ Recommendation provided
- ✅ User corrects phasing
- ✅ Cash flow feasibility verified
- ✅ Prevents unworkable project plans

---

#### TC-OPP-E2E-NEG-014: Partner Due Diligence Expired During Opportunity Development
**Priority:** P1  
**Category:** E2E Negative - Compliance

**Business Scenario:**
Partner due diligence expires while opportunity is in development - blocks decision.

**Test Flow:**
1. **Opportunity Creation (Jan 1, 2026):**
   - Opportunity: "Education Programme"
   - Partner: "Local NGO X"
   - Partner due diligence: Valid until March 31, 2026
   - Development timeline: 2-3 months
   
2. **Development Progress:**
   - January: Profiling
   - February: Budget and schedule development
   - March: Quality review, decision package prep
   
3. **Submission (April 5, 2026):**
   - Opportunity Manager submits for decision
   - Due diligence: Expired April 1, 2026 (5 days ago)
   
4. **Validation Failure:**
   - System checks partner due diligence
   - Status: Expired
   - **Critical validation failure:**
     - "Partner due diligence expired"
     - "Cannot proceed without valid due diligence"
     - "Contact partnerships team to renew"
   - Submission blocked
   
5. **Manager Action:**
   - Contacts partnerships team
   - Requests expedited due diligence renewal
   
6. **Partnerships Team Review:**
   - Reviews partner status
   - Identifies concern: Partner had financial issues
   - Requires enhanced due diligence
   - Estimated time: 2 weeks
   
7. **Opportunity On Hold:**
   - Opportunity placed "On Hold - Due Diligence"
   - All stakeholders notified
   - Work paused pending due diligence
   
8. **Due Diligence Outcomes:**
   
   **Scenario A (Pass):**
   - Due diligence completed (April 20)
   - Partner cleared: Financial issues resolved
   - Valid until: April 20, 2027
   - Opportunity reactivated
   - Proceeds to decision
   
   **Scenario B (Fail):**
   - Due diligence completed (April 20)
   - Partner fails: Financial instability confirmed
   - Recommendation: Do not proceed
   - Opportunity has 2 options:
     - Find alternative partner
     - Cancel opportunity
   
9. **Alternative Partner:**
   - If Scenario B, find new partner
   - New partner: "Local NGO Y"
   - Due diligence: Valid until 2027
   - Opportunity updated
   - Proceeds to decision

**Expected Results:**
- ✅ Expired due diligence detected
- ✅ Submission blocked
- ✅ Renewal process triggered
- ✅ Opportunity on hold during renewal
- ✅ Pass/fail scenarios handled
- ✅ Alternative partner option available
- ✅ Prevents partnering with unvetted organizations

---

#### TC-OPP-E2E-NEG-015: Document Version Control Conflict
**Priority:** P2  
**Category:** E2E Negative - Data Integrity

**Business Scenario:**
Two users upload different versions of the same document simultaneously - system must resolve.

**Test Flow:**
1. **Initial Document:**
   - Opportunity: "Health Project"
   - Document: "Budget Estimate v1.0" (uploaded Monday)
   
2. **Simultaneous Updates (Friday):**
   - **User A (Finance):**
     - Updates budget offline all week
     - Creates: "Budget Estimate v2.0"
     - Uploads Friday 3:00 PM
   
   - **User B (Opportunity Manager):**
     - Also updates budget (parallel work)
     - Creates: "Budget Estimate v2.0" (different content)
     - Uploads Friday 3:02 PM (2 minutes later)
   
3. **Conflict Detection:**
   - System detects:
     - Same filename: "Budget Estimate v2.0"
     - Same version number: v2.0
     - Different content (checksums don't match)
     - Both claim to be latest version
   - **Version conflict**
   
4. **Conflict Resolution UI:**
   - User B shown conflict screen:
     ```
     Version Conflict Detected
     
     Document: Budget Estimate v2.0
     
     Your version (uploaded 3:02 PM):
     - Size: 245 KB
     - Last modified: Friday 2:45 PM
     - Uploaded by: You (User B)
     
     Existing version (uploaded 3:00 PM):
     - Size: 238 KB
     - Last modified: Friday 2:50 PM
     - Uploaded by: User A (Finance)
     
     Actions:
     ☐ Replace existing version with yours
     ☐ Keep existing version, rename yours as v2.1
     ☐ Download both versions to compare and merge manually
     ☑ Cancel upload
     ```
   
5. **User B Action:**
   - Selects: "Download both to compare"
   - Downloads both versions
   - Opens side-by-side comparison
   
6. **Manual Merge:**
   - User B contacts User A
   - They compare versions:
     - User A: Updated labor costs
     - User B: Updated equipment costs
     - Both changes needed
   - User A merges both changes
   - Creates: "Budget Estimate v2.0 - Final"
   
7. **Upload Final Version:**
   - User A uploads merged version
   - Clearly labeled: "v2.0 - Final (Merged)"
   - Both User A and User B versions archived
   
8. **Version History:**
   - Complete version history:
     - v1.0 (Monday) - Original
     - v2.0 - User A (Friday 3:00 PM) - Archived
     - v2.0 - User B (Friday 3:02 PM) - Archived
     - v2.0 - Final (Friday 3:30 PM) - Current
   - All versions accessible in history
   
9. **Audit Trail:**
   - Shows conflict detection
   - User actions logged
   - Merge process documented
   - Final version approved by both users

**Expected Results:**
- ✅ Version conflict detected
- ✅ Clear conflict resolution options
- ✅ Users can compare versions
- ✅ Manual merge facilitated
- ✅ All versions preserved in history
- ✅ Audit trail complete
- ✅ Prevents one user's work being lost

---

### Category 4: Workflow and Business Rule Violations (5 scenarios)

#### TC-OPP-E2E-NEG-016: Attempt to Convert Opportunity Before All Conditions Met
**Priority:** P1  
**Category:** E2E Negative - Business Rule

**Business Scenario:**
User attempts to convert opportunity to project before all Go decision conditions are satisfied.

**Test Flow:**
1. **Conditional Go Decision:**
   - Opportunity approved with 3 conditions:
     - Condition 1: "Infrastructure advisor sign-off required" - ❌ Pending
     - Condition 2: "Partnership agreement finalized" - ✅ Complete
     - Condition 3: "Environmental assessment approved" - ❌ Pending
   
2. **Premature Conversion Attempt:**
   - Opportunity Manager attempts to convert to project
   - Clicks "Convert to Project"
   
3. **Validation Check:**
   - System checks all conditions
   - **2 of 3 incomplete:**
     - Condition 1: Incomplete
     - Condition 3: Incomplete
   
4. **Conversion Blocked:**
   - Error message:
     - "Cannot Convert to Project"
     - "The following Go decision conditions must be met first:"
     - "❌ Infrastructure advisor sign-off (Pending)"
     - "❌ Environmental assessment approval (Pending)"
     - "Contact responsible parties to complete conditions"
   - Conversion blocked
   
5. **Condition Fulfillment:**
   - Manager contacts Infrastructure advisor
   - Advisor reviews and approves (Condition 1: ✅)
   - Manager submits for environmental assessment
   - Assessment takes 1 week
   - Assessment approved (Condition 3: ✅)
   
6. **All Conditions Met:**
   - System automatically checks
   - All 3 conditions: ✅ Complete
   - Notification sent: "All conditions met - ready for conversion"
   
7. **Successful Conversion:**
   - Manager attempts conversion again
   - Validation passes ✅
   - Project created successfully

**Expected Results:**
- ✅ Premature conversion blocked
- ✅ Specific incomplete conditions listed
- ✅ Clear guidance provided
- ✅ Manager completes conditions
- ✅ Automatic re-validation
- ✅ Conversion successful after conditions met

---

#### TC-OPP-E2E-NEG-017: Circular Dependency in Multi-Opportunity Programme
**Priority:** P2  
**Category:** E2E Negative - Data Integrity

**Business Scenario:**
User creates circular dependencies between opportunities in a programme - system detects and prevents.

**Test Flow:**
1. **Programme with 3 Opportunities:**
   - Opportunity A: "Infrastructure Component"
   - Opportunity B: "Capacity Building Component"
   - Opportunity C: "Monitoring & Evaluation Component"
   
2. **Dependency Setup:**
   - User sets:
     - A depends on B (B must complete before A starts)
     - B depends on C (C must complete before B starts)
     - C depends on A (A must complete before C starts) ← Circular!
   
3. **Circular Dependency Detection:**
   - System analyzes dependency graph
   - Detects cycle: A → B → C → A
   - **Critical error:** Circular dependency
   
4. **Validation Failure:**
   - Error shown:
     - "⚠️ Circular Dependency Detected"
     - "The following opportunities form a circular dependency:"
     - "A depends on B, B depends on C, C depends on A"
     - "This creates an impossible sequence"
     - "Please remove at least one dependency to break the cycle"
   
5. **Visualization:**
   - System shows dependency diagram:
     ```
        A
       ↗ ↓
      C → B
     ```
   - Cycle highlighted in red
   
6. **Resolution:**
   - User removes: "C depends on A"
   - New dependencies:
     - A depends on B
     - B depends on C
     - C has no dependencies (can start first)
   - Valid sequence: C → B → A ✅
   
7. **Validation Passed:**
   - Dependency graph validated
   - Programme can proceed

**Expected Results:**
- ✅ Circular dependency detected
- ✅ Visual representation provided
- ✅ User removes problematic dependency
- ✅ Valid sequence established
- ✅ Programme proceeds with correct dependencies

---

#### TC-OPP-E2E-NEG-018: Attempt to Delete Opportunity Referenced by Active Project
**Priority:** P1  
**Category:** E2E Negative - Data Integrity

**Business Scenario:**
User attempts to delete opportunity that has already been converted to an active project.

**Test Flow:**
1. **Converted Opportunity:**
   - Opportunity: "Education Initiative"
   - Status: "Converted"
   - Project created: PRJ-2026-0123
   - Project status: "Active - Implementation"
   
2. **Delete Attempt:**
   - User attempts to delete opportunity
   - Reason: "Clean up old records"
   
3. **Referential Integrity Check:**
   - System checks if opportunity is referenced
   - Finds: Active project PRJ-2026-0123
   - **Cannot delete:** Active reference exists
   
4. **Delete Blocked:**
   - Error message:
     - "Cannot Delete Opportunity"
     - "This opportunity is linked to active project PRJ-2026-0123"
     - "Cannot delete opportunity with active project reference"
     - "Options:"
     - "1. Archive opportunity (recommended)"
     - "2. Delete project first (not recommended - only if project cancelled)"
   
5. **User Action:**
   - Selects: "Archive opportunity"
   - Opportunity archived (not deleted)
   - Still accessible for historical reference
   - Linkage to project maintained
   - Audit trail preserved

**Expected Results:**
- ✅ Delete blocked due to active reference
- ✅ Clear explanation provided
- ✅ Alternative (archive) offered
- ✅ Data integrity maintained
- ✅ Historical linkage preserved

---

#### TC-OPP-E2E-NEG-019: Workflow State Machine Violation
**Priority:** P1  
**Category:** E2E Negative - Business Rule

**Business Scenario:**
User attempts invalid status transition that violates workflow state machine.

**Test Flow:**
1. **Valid State Transitions:**
   - Draft → Profiling → Decision → Approved → Authorized → Converted
   - Also allowed: Any status → On Hold → Resume to previous status
   
2. **Invalid Transition Attempt:**
   - Current status: "Profiling"
   - User attempts: "Profiling" → "Authorized" (skipping Decision and Approved)
   
3. **State Machine Validation:**
   - System checks allowed transitions from "Profiling"
   - Allowed: Profiling → Decision, or Profiling → On Hold
   - Attempted: Profiling → Authorized ❌ Invalid
   
4. **Transition Blocked:**
   - Error message:
     - "Invalid Status Transition"
     - "Cannot change from 'Profiling' to 'Authorized'"
     - "Required sequence: Profiling → Decision → Approved → Authorized"
     - "Current status: Profiling"
     - "Next allowed statuses: Decision, On Hold"
   
5. **Correct Workflow:**
   - User follows correct sequence:
     - Profiling → Decision (submit for decision)
     - Decision → Approved (DOA approves)
     - Approved → Authorized (budget/personnel authorized)
     - Authorized → Converted (convert to project)
   - All transitions valid ✅

**Expected Results:**
- ✅ Invalid transition blocked
- ✅ State machine enforced
- ✅ Clear guidance on correct sequence
- ✅ User follows proper workflow
- ✅ Data integrity maintained

---

#### TC-OPP-E2E-NEG-020: Mass Status Change Without Proper Authorization
**Priority:** P1  
**Category:** E2E Negative - Security

**Business Scenario:**
User attempts to bulk-change status of 50 opportunities without proper authorization.

**Test Flow:**
1. **Bulk Selection:**
   - User selects 50 opportunities (various owners)
   - Attempts bulk action: "Change Status to Approved"
   
2. **Authorization Check:**
   - System checks user's permissions for each opportunity:
     - User is Opportunity Manager for 10 opportunities
     - User is NOT manager for 40 opportunities
     - DOA authority required to approve
   
3. **Partial Authorization:**
   - **Authorized for:** 10 opportunities (user is manager)
   - **NOT authorized for:** 40 opportunities (different managers)
   
4. **Action Blocked:**
   - Error message:
     - "Insufficient Authorization"
     - "You can modify 10 of 50 selected opportunities"
     - "You are not the Opportunity Manager for the remaining 40"
     - "Bulk action cancelled"
     - "Select only opportunities you manage, or request proper authorization"
   
5. **Audit Log:**
   - Attempted unauthorized bulk action logged
   - Security team notified
   - No changes made to any opportunities ✅
   
6. **Correct Process:**
   - User selects only their 10 opportunities
   - Performs bulk action successfully
   - Other 40 opportunities remain unchanged

**Expected Results:**
- ✅ Unauthorized bulk action blocked
- ✅ No changes to unauthorized opportunities
- ✅ Security maintained
- ✅ Audit logged
- ✅ Clear error message
- ✅ User guided to correct approach

---

## 📊 Summary

### Total Additional E2E Scenarios Created: 40

#### Positive Scenarios: 20
1. Multi-Regional Opportunity Coordination (5 stakeholder groups)
2. Real-Time Collaborative Editing (3 simultaneous users)
3. Delegated Decision Workflow with Escalation
4. Partnership Agreement Triggers Opportunity Creation
5. Portfolio Aggregation from Multiple Opportunities (4 children)
6. AI-Driven Opportunity Discovery from Multiple Documents (5 docs)
7. Historical Data Migration with DST Benchmarking (200 opportunities)
8. Opportunity Cloning and Template Management
9. AI-Assisted Narrative Generation for Concept Note
10. Emergency Fast-Track Approval (24-hour timeline)
11. Opportunity Amendment After Go Decision
12. Same-Day Fast-Track Opportunity (8-hour timeline)
13. Cross-System Data Synchronization (4 systems)
14. Global Indices Update Cascade (193 countries)
15. Opportunity Lifecycle Audit and Compliance Report
16. Opportunity to Programme Conversion (4 components)
17. Opportunity Progression Through All Lifecycle Stages (15 stages)
18. Bulk Opportunity Processing and Batch Decision (15 opportunities)
19. Mobile Field Work and Offline Opportunity Management
20. Opportunity Recovery After 18-Month Hold

#### Negative Scenarios: 20
1. Database Connection Loss During Decision Recording
2. Cascading Failure - AI Service Down During Bulk Processing
3. Data Corruption Detection and Recovery
4. Network Partition During Multi-User Collaboration
5. System Overload During Peak Usage (500 users)
6. Authorization Revoked Mid-Workflow
7. Session Hijacking Attempt Detected
8. Insufficient Resources for Bulk DST Generation (200 profiles)
9. Conflicting Simultaneous Decisions by Different DOA Holders
10. Expired Partnership Agreement Used in Opportunity
11. Budget-DST Misalignment Detected (-75% below benchmark)
12. Geography-DST Country Data Mismatch
13. Timeline-Budget Phasing Conflict
14. Partner Due Diligence Expired During Development
15. Document Version Control Conflict
16. Attempt to Convert Before All Conditions Met
17. Circular Dependency in Multi-Opportunity Programme
18. Attempt to Delete Opportunity Referenced by Active Project
19. Workflow State Machine Violation
20. Mass Status Change Without Proper Authorization

---

## 🎯 Coverage Enhancement

### Original E2E Tests (ADVANCED_TEST_COVERAGE.md): 15
- Complete lifecycle
- Multi-country
- Partnership agreement integration
- AI-assisted creation
- Rejected decision recovery
- Concurrent collaboration
- Global indices cascade
- Budget-Schedule-Resource alignment
- Risk register integration
- External system integration
- Mobile cross-device sync
- Bulk import
- Comprehensive reporting
- Complete audit trail
- Disaster recovery

### Additional E2E Tests (This Document): 40
- **20 Positive:** Complex collaboration, templates, offline, recovery, bulk processing, fast-track, amendments
- **20 Negative:** System failures, security attacks, data corruption, authorization issues, validation failures

### **Total E2E Coverage: 55 comprehensive scenarios**

---

## 📋 Test Execution Recommendations

### Priority Order:
1. **P0 Scenarios (10):** Execute first - critical paths
2. **P1 Scenarios (25):** Execute second - important flows
3. **P2 Scenarios (5):** Execute third - nice-to-have coverage

### Execution Strategy:
- **Positive scenarios:** Validate happy paths and complex workflows work correctly
- **Negative scenarios:** Ensure system fails gracefully and recovers properly
- **Both together:** Comprehensive production-readiness validation

### Estimated Execution Time:
- **Positive scenarios:** ~40-60 minutes each = 20-40 hours total
- **Negative scenarios:** ~30-45 minutes each = 10-15 hours total
- **Total:** 30-55 hours for complete E2E test execution

---

**Status:** ✅ **COMPLETE - 40 Additional E2E Scenarios Documented**  
**Ready for:** Test case refinement and C# implementation

---

**Last Updated:** January 13, 2026  
**Next Steps:** Implement C# test classes for these scenarios
