# DSTManager Test Cases

**Manager:** `DSTManager` (Decision Support Tool Manager)  
**Entity:** `DSTProfile`, `DSTRecommendation`  
**Test Count:** 45+  
**Priority:** P0-P1 (Critical/High)  
**Created:** January 13, 2026

---

## Overview

Test cases for the Decision Support Tool (DST) which analyzes opportunities for feasibility, complexity, risk, and strategic alignment. The DST performs systematic evaluation across 9 parameters and generates actionable recommendations.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Profile Generation | 8 | P0 |
| Nine Parameter Evaluation | 18 | P0 |
| Recommendations | 10 | P1 |
| Similar Projects | 5 | P1 |
| Reports | 4 | P1 |

---

## 1. Profile Generation (P0)

### TC-OPP-DST-F-001: Generate Complete DST Profile
**Priority:** P0  
**Category:** Functional

**Description:**  
Generate complete DST profile for an opportunity.

**Preconditions:**
- Opportunity exists with required data
- Country profile data available
- Historical project data exists

**Test Steps:**
1. Create opportunity with full details
2. Call `GenerateDSTProfileAsync(opportunityId)`
3. Verify profile generated

**Expected Results:**
- `DSTProfile` entity created
- All 9 parameters evaluated
- Complexity score calculated (0-10)
- Risk assessment completed
- Profile report generated

**Test Data:**
```json
{
  "opportunityId": 123,
  "generateDate": "2026-01-13",
  "analysisDepth": "Comprehensive"
}
```

---

### TC-OPP-DST-F-002: Regenerate Existing Profile
**Priority:** P1  
**Category:** Functional

**Description:**  
Regenerate DST profile when opportunity data changes.

**Test Steps:**
1. Generate initial profile
2. Update opportunity budget significantly
3. Call `RegenerateDSTProfileAsync(opportunityId)`
4. Verify profile updated

**Expected Results:**
- New profile version created
- Old profile archived
- Changes highlighted in report
- Version history maintained

---

### TC-OPP-DST-F-003: Generate Profile with Missing Data
**Priority:** P0  
**Category:** Validation

**Description:**  
Handle profile generation when opportunity data incomplete.

**Test Steps:**
1. Create opportunity missing budget data
2. Attempt to generate profile
3. Verify partial profile with warnings

**Expected Results:**
- Profile generated with available data
- Missing data flagged clearly
- Completeness score calculated
- User prompted to complete data

---

### TC-OPP-DST-F-004: Calculate Complexity Score
**Priority:** P0  
**Category:** Calculation

**Description:**  
Verify complexity score calculation is accurate.

**Test Steps:**
1. Create simple opportunity (single country, standard scope)
2. Generate profile
3. Verify low complexity score (2-4)
4. Create complex opportunity (multi-country, innovative approach)
5. Generate profile
6. Verify high complexity score (7-9)

**Expected Results:**
- Complexity score range: 0-10
- Factors considered:
  - Geographic spread
  - Technical complexity
  - Stakeholder count
  - Risk factors
  - Innovation level
- Score consistent with factors

---

### TC-OPP-DST-F-005: Analyze Delivery Risk
**Priority:** P0  
**Category:** Analysis

**Description:**  
Assess delivery risk based on country context and past performance.

**Test Steps:**
1. Create opportunity in fragile state (e.g., conflict zone)
2. Generate profile
3. Verify risk assessment

**Expected Results:**
- Risk level: Low/Medium/High/Very High
- Risk factors identified:
  - Country stability index
  - Corruption perception index
  - Past performance in region
  - Security situation
- Mitigation recommendations

---

### TC-OPP-DST-F-006: Assess Organizational Capabilities
**Priority:** P1  
**Category:** Analysis

**Description:**  
Evaluate UNOPS capability to deliver opportunity.

**Test Steps:**
1. Create opportunity requiring specialized expertise
2. Generate profile
3. Verify capability assessment

**Expected Results:**
- Technical capabilities evaluated
- Staff expertise assessed
- Equipment/resources checked
- Gaps identified
- Training needs flagged

---

### TC-OPP-DST-F-007: Profile Performance Benchmarking
**Priority:** P2  
**Category:** Performance

**Description:**  
Verify DST profile generation completes in acceptable time.

**Test Steps:**
1. Generate profile for complex opportunity
2. Measure execution time
3. Verify acceptable performance

**Expected Results:**
- Profile generated in < 15 seconds
- Progress indicators shown
- Can run async without blocking
- Results cached appropriately

---

### TC-OPP-DST-F-008: Profile Version Control
**Priority:** P1  
**Category:** Audit

**Description:**  
Maintain version history of DST profiles.

**Test Steps:**
1. Generate initial profile (v1)
2. Update opportunity
3. Regenerate profile (v2)
4. Update again
5. Regenerate profile (v3)
6. Query profile history

**Expected Results:**
- All 3 versions preserved
- Timestamps recorded
- User who generated each version tracked
- Can compare versions
- Can view historical profile

---

## 2. Nine Parameter Evaluation (P0)

### TC-OPP-DST-P-001: Evaluate Strategic Alignment
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Assess opportunity alignment with UNOPS strategic priorities.

**Test Steps:**
1. Create opportunity aligned with UNOPS strategic plan
2. Generate profile
3. Verify strategic alignment evaluation

**Expected Results:**
- Alignment score: 0-100%
- Strategic priorities mapped:
  - SDG alignment
  - UNOPS focus areas
  - Regional priorities
  - Thematic areas
- Narrative summary generated
- Recommendations for improvement

---

### TC-OPP-DST-P-002: Evaluate Partners and Stakeholders
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Analyze partner relationships and stakeholder landscape.

**Test Steps:**
1. Create opportunity with multiple partners
2. Generate profile
3. Verify partner/stakeholder analysis

**Expected Results:**
- Partner profiles assessed:
  - Past performance
  - Financial capacity
  - Technical expertise
  - Reputation
- Stakeholder map generated
- Risks identified (e.g., conflicts of interest)
- Due diligence status checked

---

### TC-OPP-DST-P-003: Evaluate Physical Implementation
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Assess physical implementation feasibility.

**Test Steps:**
1. Create infrastructure opportunity
2. Generate profile
3. Verify implementation analysis

**Expected Results:**
- Site accessibility assessed
- Logistics challenges identified
- Equipment availability checked
- Environmental factors considered
- Permits and approvals flagged

---

### TC-OPP-DST-P-004: Evaluate Context
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Analyze country/regional context for delivery.

**Test Steps:**
1. Create opportunity in specific country
2. Generate profile
3. Verify context evaluation

**Expected Results:**
- Political stability assessed
- Economic conditions analyzed
- Social factors considered
- Legal framework reviewed
- Cultural considerations noted

---

### TC-OPP-DST-P-005: Evaluate Scope and Scale
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Assess opportunity scope and scale appropriateness.

**Test Steps:**
1. Create large-scale opportunity ($5M+)
2. Generate profile
3. Verify scope/scale analysis

**Expected Results:**
- Scale appropriateness assessed
- Scope creep risks identified
- Deliverables validated
- Phasing recommended if needed
- Resource requirements estimated

---

### TC-OPP-DST-P-006: Evaluate Timeframe
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Analyze timeline realism and achievability.

**Test Steps:**
1. Create opportunity with ambitious timeline
2. Generate profile
3. Verify timeframe evaluation

**Expected Results:**
- Timeline realism assessed
- Critical path identified
- Seasonal factors considered
- Dependencies highlighted
- Alternative timelines suggested

---

### TC-OPP-DST-P-007: Evaluate Budget and Resourcing
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Assess budget adequacy and resource requirements.

**Test Steps:**
1. Create opportunity with defined budget
2. Generate profile
3. Verify budget/resource analysis

**Expected Results:**
- Budget adequacy assessed
- Cost estimates compared to similar projects
- Resource availability checked
- Personnel requirements identified
- Funding gaps highlighted

---

### TC-OPP-DST-P-008: Evaluate Outcome and Impact
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Analyze expected outcomes and impact potential.

**Test Steps:**
1. Create opportunity with defined outcomes
2. Generate profile
3. Verify outcome/impact assessment

**Expected Results:**
- Impact potential scored
- Beneficiary analysis completed
- Sustainability assessed
- M&E framework suggested
- Theory of change validated

---

### TC-OPP-DST-P-009: Evaluate Safeguards, Ethics, and Legal
**Priority:** P0  
**Category:** Parameter Evaluation

**Description:**  
Assess compliance with safeguards and legal requirements.

**Test Steps:**
1. Create opportunity requiring environmental assessment
2. Generate profile
3. Verify safeguards evaluation

**Expected Results:**
- Environmental safeguards assessed
- Social safeguards checked
- Ethical considerations reviewed
- Legal compliance validated
- Required assessments identified

---

### TC-OPP-DST-P-010: All Parameters - Balanced Opportunity
**Priority:** P0  
**Category:** Integration

**Description:**  
Verify all 9 parameters evaluated correctly for balanced opportunity.

**Test Steps:**
1. Create well-balanced opportunity (all data complete)
2. Generate profile
3. Verify comprehensive evaluation

**Expected Results:**
- All 9 parameters scored
- Overall complexity score: 4-6 (medium)
- No critical issues flagged
- Recommendations balanced
- Ready for Go decision

---

### TC-OPP-DST-P-011: All Parameters - High Risk Opportunity
**Priority:** P1  
**Category:** Integration

**Description:**  
Verify appropriate scoring for high-risk opportunity.

**Test Steps:**
1. Create opportunity with multiple risk factors
2. Generate profile
3. Verify risk properly reflected

**Expected Results:**
- Multiple parameters flagged red
- Overall complexity score: 8-10 (high)
- Critical issues clearly highlighted
- Significant mitigation needed
- May recommend No-Go

---

### TC-OPP-DST-P-012: Parameter Weighting
**Priority:** P1  
**Category:** Configuration

**Description:**  
Verify parameter weighting can be adjusted.

**Test Steps:**
1. Set custom weights for parameters
2. Generate profile
3. Verify weights applied correctly

**Expected Results:**
- Custom weights used in scoring
- More important parameters weighted higher
- Overall score reflects weighting
- Weighting rationale documented

---

### TC-OPP-DST-P-013: Parameter Missing Data Handling
**Priority:** P1  
**Category:** Validation

**Description:**  
Handle missing data for specific parameters gracefully.

**Test Steps:**
1. Create opportunity missing context data
2. Generate profile
3. Verify handling

**Expected Results:**
- Parameter flagged as incomplete
- Score based on available data
- Missing data clearly indicated
- User prompted to complete

---

### TC-OPP-DST-P-014: Parameter Narrative Generation
**Priority:** P1  
**Category:** Reporting

**Description:**  
Verify each parameter generates narrative summary.

**Test Steps:**
1. Generate profile
2. Verify narrative for each parameter

**Expected Results:**
- Human-readable summary for each parameter
- Key points highlighted
- Supporting data referenced
- AI-generated but reviewed for accuracy

---

### TC-OPP-DST-P-015: Parameter Score Thresholds
**Priority:** P1  
**Category:** Validation

**Description:**  
Verify parameter scores trigger appropriate alerts.

**Test Steps:**
1. Parameter score < 40% (red threshold)
2. Verify critical alert generated

**Expected Results:**
- Alert levels:
  - Red: < 40% (critical)
  - Yellow: 40-60% (caution)
  - Green: > 60% (good)
- Alerts visible in profile
- Decision makers notified

---

### TC-OPP-DST-P-016: Historical Parameter Comparison
**Priority:** P2  
**Category:** Analysis

**Description:**  
Compare parameter scores to historical opportunities.

**Test Steps:**
1. Generate profile
2. Compare to similar opportunities
3. Verify benchmarking

**Expected Results:**
- Historical range provided for each parameter
- Current opportunity plotted on range
- Outliers identified
- Trends visible

---

### TC-OPP-DST-P-017: Parameter Interdependencies
**Priority:** P2  
**Category:** Analysis

**Description:**  
Identify interdependencies between parameters.

**Test Steps:**
1. Generate profile
2. Analyze parameter relationships
3. Verify dependencies highlighted

**Expected Results:**
- Dependencies mapped (e.g., scope affects budget)
- Cascading impacts identified
- Optimization suggestions provided

---

### TC-OPP-DST-P-018: Parameter Override with Justification
**Priority:** P2  
**Category:** Configuration

**Description:**  
Allow expert override of parameter scores with justification.

**Test Steps:**
1. Generate profile (automated score = 65%)
2. Expert overrides to 80% with justification
3. Verify override recorded

**Expected Results:**
- Override applied
- Justification required and recorded
- Original score preserved
- Override visible in audit trail

---

## 3. Recommendations (P1)

### TC-OPP-DST-R-001: Generate Risk Recommendations
**Priority:** P1  
**Category:** Recommendations

**Description:**  
DST suggests risks to add to draft risk register.

**Test Steps:**
1. Generate profile
2. Query `GetRiskRecommendationsAsync(profileId)`
3. Verify risks suggested

**Expected Results:**
- Relevant risks identified from:
  - Country context
  - Similar projects
  - Complexity factors
  - Historical issues
- Risk severity estimated
- Mitigation strategies suggested

---

### TC-OPP-DST-R-002: Generate Personnel Recommendations
**Priority:** P1  
**Category:** Recommendations

**Description:**  
DST flags required personnel specializations.

**Test Steps:**
1. Generate profile for infrastructure opportunity
2. Query personnel recommendations
3. Verify roles suggested

**Expected Results:**
- Required roles identified (e.g., gender advisor, environmental specialist)
- Justification provided
- Availability checked
- Alternative options suggested

---

### TC-OPP-DST-R-003: Suggest Initiative Structure
**Priority:** P1  
**Category:** Recommendations

**Description:**  
DST recommends Project/Programme/Portfolio structure.

**Test Steps:**
1. Generate profile for multi-country opportunity
2. Query structure recommendation
3. Verify appropriate structure suggested

**Expected Results:**
- Structure recommended based on:
  - Scope complexity
  - Geographic spread
  - Budget size
  - Duration
- Rationale provided
- Governance implications explained

---

### TC-OPP-DST-R-004: Suggest Process Triggers
**Priority:** P1  
**Category:** Recommendations

**Description:**  
DST identifies required processes to initiate.

**Test Steps:**
1. Generate profile
2. Query process recommendations
3. Verify applicable processes identified

**Expected Results:**
- Required processes flagged:
  - Environmental assessment
  - Social impact assessment
  - Procurement planning
  - Stakeholder consultation
- Timing recommendations
- Resource requirements

---

### TC-OPP-DST-R-005: Suggest Similar Opportunities for Learning
**Priority:** P1  
**Category:** Recommendations

**Description:**  
DST recommends reviewing similar past opportunities.

**Test Steps:**
1. Generate profile
2. Query similar opportunities
3. Verify relevant matches

**Expected Results:**
- 3-5 similar opportunities identified
- Similarity score provided (0-100%)
- Lessons learned highlighted
- Contact information for key staff

---

### TC-OPP-DST-R-006: Accept Recommendation
**Priority:** P1  
**Category:** User Action

**Description:**  
User marks recommendation as actioned.

**Test Steps:**
1. Get recommendations
2. Call `AcceptRecommendationAsync(recommendationId, action)`
3. Verify status updated

**Expected Results:**
- Recommendation marked "Actioned"
- Action details recorded
- Timestamp and user captured
- Visible in profile

---

### TC-OPP-DST-R-007: Reject Recommendation with Reason
**Priority:** P1  
**Category:** User Action

**Description:**  
User rejects recommendation with justification.

**Test Steps:**
1. Get recommendations
2. Call `RejectRecommendationAsync(recommendationId, reason)`
3. Verify rejection recorded

**Expected Results:**
- Recommendation marked "Rejected"
- Reason captured
- Expert review flagged if critical
- Audit trail maintained

---

### TC-OPP-DST-R-008: Mark Recommendation as Considered
**Priority:** P2  
**Category:** User Action

**Description:**  
User acknowledges recommendation without action.

**Test Steps:**
1. Get recommendations
2. Mark as "Considered but not actioned"
3. Verify status

**Expected Results:**
- Status = "Considered"
- Timestamp recorded
- Can revisit later
- Counted as reviewed

---

### TC-OPP-DST-R-009: Priority Ranking of Recommendations
**Priority:** P1  
**Category:** Reporting

**Description:**  
Recommendations ranked by priority/criticality.

**Test Steps:**
1. Generate profile with multiple recommendations
2. Query recommendations
3. Verify priority ordering

**Expected Results:**
- Critical recommendations first
- Priority levels:
  - Critical (must do)
  - Important (should do)
  - Nice to have (could do)
- Color coding applied

---

### TC-OPP-DST-R-010: Recommendation Impact Analysis
**Priority:** P2  
**Category:** Analysis

**Description:**  
Show impact of implementing recommendations.

**Test Steps:**
1. Generate profile
2. For each recommendation, show impact on:
   - Risk reduction
   - Complexity score
   - Success probability
3. Verify impact displayed

**Expected Results:**
- Impact quantified where possible
- Before/after comparison
- ROI of implementing recommendation
- Helps prioritization

---

## 4. Similar Projects (P1)

### TC-OPP-DST-S-001: Find Similar by Geography
**Priority:** P1  
**Category:** Similarity

**Description:**  
Find similar opportunities in same geographic region.

**Test Steps:**
1. Create opportunity in Bangladesh
2. Call `FindSimilarOpportunitiesAsync(opportunityId, "geography")`
3. Verify similar opportunities returned

**Expected Results:**
- Opportunities in Bangladesh or South Asia
- Similarity score based on geography
- Other matching criteria highlighted
- Up to 10 results

---

### TC-OPP-DST-S-002: Find Similar by Sector
**Priority:** P1  
**Category:** Similarity

**Description:**  
Find similar opportunities in same sector.

**Test Steps:**
1. Create water infrastructure opportunity
2. Find similar by sector
3. Verify relevant matches

**Expected Results:**
- Opportunities in water/infrastructure sector
- Scope similarity considered
- Technical approach compared
- Lessons learned highlighted

---

### TC-OPP-DST-S-003: Find Similar by Partner
**Priority:** P1  
**Category:** Similarity

**Description:**  
Find opportunities with same partners.

**Test Steps:**
1. Create opportunity with World Bank as partner
2. Find similar opportunities
3. Verify partner-based matches

**Expected Results:**
- Opportunities with World Bank
- Partnership model compared
- Historical performance data
- Contact information for relationship managers

---

### TC-OPP-DST-S-004: Similarity Scoring Algorithm
**Priority:** P2  
**Category:** Algorithm

**Description:**  
Verify similarity score calculation is accurate.

**Test Steps:**
1. Create opportunity with known characteristics
2. Find similar opportunities
3. Verify similarity scores make sense

**Expected Results:**
- Score range: 0-100%
- Factors weighted:
  - Geography (30%)
  - Sector (25%)
  - Budget size (15%)
  - Partners (15%)
  - Complexity (15%)
- Threshold: > 60% shown as similar

---

### TC-OPP-DST-S-005: Similar Project Lessons Learned
**Priority:** P1  
**Category:** Knowledge Management

**Description:**  
Extract lessons learned from similar projects.

**Test Steps:**
1. Find similar opportunities
2. Query lessons learned
3. Verify relevant lessons extracted

**Expected Results:**
- Lessons categorized:
  - What worked well
  - What didn't work
  - What to avoid
  - What to replicate
- Actionable insights
- Linked to specific projects

---

## 5. Reports (P1)

### TC-OPP-DST-REP-001: Generate Profile Summary Report
**Priority:** P1  
**Category:** Reporting

**Description:**  
Generate comprehensive DST profile report.

**Test Steps:**
1. Generate DST profile
2. Call `GenerateProfileReportAsync(profileId)`
3. Verify report content

**Expected Results:**
- PDF report generated
- Sections:
  - Executive summary
  - 9 parameter evaluations
  - Recommendations
  - Risk assessment
  - Similar projects
- Professional formatting
- UNOPS branding

---

### TC-OPP-DST-REP-002: Generate Executive Summary
**Priority:** P1  
**Category:** Reporting

**Description:**  
Generate 1-page executive summary for decision makers.

**Test Steps:**
1. Generate profile
2. Call `GenerateExecutiveSummaryAsync(profileId)`
3. Verify summary concise

**Expected Results:**
- 1-page summary
- Key findings highlighted
- Go/No-Go recommendation
- Critical issues flagged
- Next steps outlined

---

### TC-OPP-DST-REP-003: Export Profile Data
**Priority:** P2  
**Category:** Reporting

**Description:**  
Export DST profile data in various formats.

**Test Steps:**
1. Generate profile
2. Export as JSON, PDF, Excel
3. Verify data integrity

**Expected Results:**
- All formats supported
- Data complete in each format
- Formatting appropriate
- Can be imported back

---

### TC-OPP-DST-REP-004: Profile Comparison Report
**Priority:** P2  
**Category:** Reporting

**Description:**  
Compare profiles of multiple opportunities.

**Test Steps:**
1. Generate profiles for 3 opportunities
2. Call `CompareProfilesAsync([id1, id2, id3])`
3. Verify comparison report

**Expected Results:**
- Side-by-side comparison
- Parameter scores compared
- Differences highlighted
- Ranking by complexity/risk
- Helps portfolio prioritization

---

## Summary

**Total Test Cases:** 45+  
**Critical (P0):** 20  
**High (P1):** 20  
**Medium (P2):** 10

**Execution Time:** ~15-20 minutes for full suite  
**Dependencies:** Opportunity, Country, Historical projects  
**AI Integration:** Heavy use of Gemini for analysis

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `DSTManagerTests.cs`  
**Status:** ✅ Ready for Implementation
