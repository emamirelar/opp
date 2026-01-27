# DSTProfiler Test Cases

**Component:** DST Profiler Business Logic  
**Test Count:** 40+  
**Priority:** P0-P1 (Critical/High)  
**Created:** January 13, 2026

---

## Overview

Test cases for Decision Support Tool profiling algorithms, parameter calculation logic, scoring methodologies, and recommendation generation.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Profiling Algorithms | 12 | P0 |
| Parameter Calculations | 18 | P0 |
| Scoring Methodology | 6 | P0 |
| Recommendation Logic | 4 | P1 |

---

## 1. Profiling Algorithms (P0)

### TC-OPP-DST-ALG-001: Complexity Score Calculation
**Priority:** P0  
**Test Steps:**
1. Input opportunity with known characteristics
2. Calculate complexity score
3. Verify algorithm correctness

**Expected Results:**
- Score range: 0-10
- Factors weighted correctly:
  - Geographic spread (20%)
  - Technical complexity (25%)
  - Stakeholder count (15%)
  - Innovation level (20%)
  - Risk factors (20%)
- Score = weighted sum

**Test Data:**
```csharp
var opportunity = new {
    GeographicSpread = 3, // Multi-country
    TechnicalComplexity = 8, // High
    StakeholderCount = 15,
    InnovationLevel = 6,
    RiskScore = 7
};
// Expected: (3*0.2) + (8*0.25) + (5*0.15) + (6*0.2) + (7*0.2) = 6.15
```

---

### TC-OPP-DST-ALG-002: Strategic Alignment Scoring
**Priority:** P0  
**Test Steps:**
1. Opportunity aligned with UNOPS priorities
2. Calculate alignment score
3. Verify methodology

**Expected Results:**
- SDG alignment (40 points)
- Regional strategy alignment (30 points)
- Thematic alignment (30 points)
- Total: 0-100 scale
- Bonus for multiple SDG contribution

---

### TC-OPP-DST-ALG-003: Risk Assessment Algorithm
**Priority:** P0  
**Test Steps:**
1. Input country risk factors
2. Input project-specific risks
3. Calculate overall risk score

**Expected Results:**
- Country risk (40% weight)
- Operational risk (30% weight)
- Financial risk (20% weight)
- Reputational risk (10% weight)
- Risk level: Low/Medium/High/Very High
- Thresholds: <3=Low, 3-5=Medium, 5-7=High, >7=Very High

---

### TC-OPP-DST-ALG-004: Feasibility Score Calculation
**Priority:** P0  
**Test Steps:**
1. Assess technical feasibility
2. Assess financial feasibility
3. Assess operational feasibility
4. Combine into overall score

**Expected Results:**
- Each dimension scored 0-10
- Overall = average of three
- Red flags if any dimension < 4
- Recommendations for improvement

---

### TC-OPP-DST-ALG-005: Partner Capacity Assessment
**Priority:** P0  
**Test Steps:**
1. Analyze partner past performance
2. Assess financial capacity
3. Evaluate technical expertise
4. Calculate capacity score

**Expected Results:**
- Past performance (40%)
- Financial health (30%)
- Technical expertise (20%)
- Management capacity (10%)
- Score: 0-100
- Minimum threshold: 60 for approval

---

### TC-OPP-DST-ALG-006: Timeline Realism Check
**Priority:** P1  
**Test Steps:**
1. Compare proposed timeline to historical data
2. Calculate realism score

**Expected Results:**
- Historical average ± 20% = realistic
- >20% faster = optimistic (flag)
- >20% slower = conservative (note)
- Seasonal factors considered
- Complexity-adjusted estimates

---

### TC-OPP-DST-ALG-007: Budget Adequacy Analysis
**Priority:** P1  
**Test Steps:**
1. Compare budget to similar opportunities
2. Calculate adequacy score

**Expected Results:**
- Within 80-120% of similar = adequate
- <80% = underfunded (warning)
- >120% = overfunded (review)
- Adjustments for inflation
- Regional cost variations

---

### TC-OPP-DST-ALG-008: Resource Availability Check
**Priority:** P1  
**Test Steps:**
1. Match required skills to available personnel
2. Calculate availability score

**Expected Results:**
- All critical roles covered = 100%
- Missing critical role = major flag
- Missing nice-to-have = minor note
- Timeline conflict detection
- Alternative resource suggestions

---

### TC-OPP-DST-ALG-009: Context Analysis Algorithm
**Priority:** P0  
**Test Steps:**
1. Analyze country political stability
2. Economic conditions
3. Social factors
4. Combine into context score

**Expected Results:**
- Multiple indices integrated:
  - Political Stability Index
  - Economic Growth Rate
  - Corruption Perception Index
  - Fragility Index
- Context score: 0-100
- Lower score = more challenging

---

### TC-OPP-DST-ALG-010: Impact Potential Calculation
**Priority:** P1  
**Test Steps:**
1. Assess beneficiary reach
2. Assess outcome sustainability
3. Calculate impact potential

**Expected Results:**
- Direct beneficiaries count
- Indirect beneficiaries estimate
- Long-term sustainability score
- SDG contribution quantified
- Impact score: 0-10

---

### TC-OPP-DST-ALG-011: Similar Project Matching
**Priority:** P1  
**Test Steps:**
1. Input opportunity characteristics
2. Find similar past opportunities
3. Calculate similarity scores

**Expected Results:**
- Vector similarity algorithm
- Factors: geography, sector, budget, partners
- Similarity threshold: >60% to show
- Top 5 matches returned
- Lessons learned extracted

---

### TC-OPP-DST-ALG-012: Overall Recommendation Logic
**Priority:** P0  
**Test Steps:**
1. All parameters scored
2. Generate Go/No-Go recommendation
3. Verify logic

**Expected Results:**
- Go if:
  - Complexity < 8
  - Risk < 7
  - Feasibility > 6
  - Strategic alignment > 70%
- No-Go if any critical threshold exceeded
- Conditional Go if borderline
- Confidence level provided

---

## 2. Parameter Calculations (P0)

### TC-OPP-DST-CALC-001: Parameter 1 - Strategic Alignment
**Priority:** P0  
**Test Steps:**
1. Calculate SDG alignment score
2. Calculate regional strategy score
3. Calculate thematic alignment
4. Combine into parameter score

**Expected Results:**
```csharp
// SDG alignment
int sdgCount = opportunity.SDGs.Count;
decimal sdgScore = Math.Min(sdgCount * 20, 40); // Max 40 points

// Regional strategy
bool alignsWithRegionalStrategy = CheckRegionalAlignment(opportunity);
decimal regionalScore = alignsWithRegionalStrategy ? 30 : 0;

// Thematic alignment
int thematicMatches = CountThematicMatches(opportunity);
decimal thematicScore = Math.Min(thematicMatches * 10, 30);

decimal totalScore = sdgScore + regionalScore + thematicScore;
// Score: 0-100
```

---

### TC-OPP-DST-CALC-002: Parameter 2 - Partners and Stakeholders
**Priority:** P0  
**Test Steps:**
1. Count and categorize stakeholders
2. Assess partner relationships
3. Calculate parameter score

**Expected Results:**
- Partner past performance (40 points)
- Stakeholder complexity (30 points)
- Due diligence status (30 points)
- Flags for high-risk partners
- Score: 0-100

---

### TC-OPP-DST-CALC-003: Parameter 3 - Physical Implementation
**Priority:** P0  
**Test Steps:**
1. Assess site accessibility
2. Logistics complexity
3. Equipment availability

**Expected Results:**
- Accessibility score (35 points)
- Logistics score (35 points)
- Equipment score (30 points)
- Red flags for remote/difficult sites
- Score: 0-100

---

### TC-OPP-DST-CALC-004: Parameter 4 - Context
**Priority:** P0  
**Test Steps:**
1. Pull country indices
2. Assess operating environment
3. Calculate context score

**Expected Results:**
```csharp
decimal contextScore = 0;

// Political stability (30%)
contextScore += country.PoliticalStabilityIndex * 0.3m;

// Economic conditions (25%)
contextScore += country.EconomicIndex * 0.25m;

// Security (25%)
contextScore += country.SecurityIndex * 0.25m;

// Legal framework (20%)
contextScore += country.LegalIndex * 0.2m;

// Lower score = more challenging context
```

---

### TC-OPP-DST-CALC-005: Parameter 5 - Scope and Scale
**Priority:** P0  
**Test Steps:**
1. Assess scope definition
2. Evaluate scale appropriateness
3. Calculate parameter score

**Expected Results:**
- Scope clarity (40 points)
- Scale appropriateness (30 points)
- Deliverables well-defined (30 points)
- Flags for scope creep risk
- Score: 0-100

---

### TC-OPP-DST-CALC-006: Parameter 6 - Timeframe
**Priority:** P0  
**Test Steps:**
1. Compare to historical data
2. Assess complexity-adjusted timeline
3. Calculate realism score

**Expected Results:**
- Historical comparison (50 points)
- Complexity adjustment (30 points)
- Seasonal factors (20 points)
- Timeline risk assessment
- Score: 0-100

---

### TC-OPP-DST-CALC-007: Parameter 7 - Budget and Resourcing
**Priority:** P0  
**Test Steps:**
1. Compare to similar opportunities
2. Assess resource availability
3. Calculate adequacy score

**Expected Results:**
- Budget adequacy (50 points)
- Resource availability (30 points)
- Cost estimation confidence (20 points)
- Underfunding flags
- Score: 0-100

---

### TC-OPP-DST-CALC-008: Parameter 8 - Outcome and Impact
**Priority:** P0  
**Test Steps:**
1. Calculate beneficiary reach
2. Assess sustainability
3. Calculate impact score

**Expected Results:**
- Beneficiary reach (40 points)
- Outcome sustainability (35 points)
- M&E framework (25 points)
- Impact potential rating
- Score: 0-100

---

### TC-OPP-DST-CALC-009: Parameter 9 - Safeguards, Ethics, Legal
**Priority:** P0  
**Test Steps:**
1. Check environmental safeguards
2. Assess ethical considerations
3. Verify legal compliance

**Expected Results:**
- Environmental compliance (35 points)
- Social safeguards (35 points)
- Legal compliance (30 points)
- Required assessments identified
- Score: 0-100

---

### TC-OPP-DST-CALC-010: Parameter Interdependencies
**Priority:** P1  
**Test Steps:**
1. Identify parameter dependencies
2. Adjust scores for interdependencies

**Expected Results:**
- Low budget impacts timeline
- High complexity impacts resourcing
- Poor context impacts risk
- Dependencies documented
- Cascading adjustments applied

---

### TC-OPP-DST-CALC-011: Parameter Weighting Override
**Priority:** P2  
**Test Steps:**
1. Apply custom parameter weights
2. Recalculate overall score

**Expected Results:**
- Custom weights sum to 100%
- Scores recalculated correctly
- Override documented
- Rationale captured

---

### TC-OPP-DST-CALC-012: Parameter Threshold Alerts
**Priority:** P1  
**Test Steps:**
1. Parameter below threshold
2. Verify alert triggered

**Expected Results:**
- Red: <40% (critical)
- Yellow: 40-60% (caution)
- Green: >60% (good)
- Alerts visible in profile
- Decision makers notified

---

### TC-OPP-DST-CALC-013: Historical Data Integration
**Priority:** P1  
**Test Steps:**
1. Pull historical data for comparison
2. Benchmark current opportunity

**Expected Results:**
- Historical averages calculated
- Current vs historical comparison
- Percentile ranking
- Trend analysis

---

### TC-OPP-DST-CALC-014: Country-Specific Adjustments
**Priority:** P1  
**Test Steps:**
1. Apply country-specific factors
2. Adjust scores accordingly

**Expected Results:**
- Cost of living adjustments
- Risk premiums applied
- Local capacity factored
- Context-appropriate scoring

---

### TC-OPP-DST-CALC-015: Sector-Specific Calculations
**Priority:** P1  
**Test Steps:**
1. Infrastructure opportunity
2. Apply sector-specific logic

**Expected Results:**
- Sector benchmarks used
- Specialized parameters considered
- Industry standards applied
- Sector expertise weighted

---

### TC-OPP-DST-CALC-016: Multi-Country Complexity
**Priority:** P1  
**Test Steps:**
1. Opportunity spans 3 countries
2. Calculate complexity adjustment

**Expected Results:**
- Base complexity per country
- Coordination overhead added
- Harmonization challenges noted
- Governance complexity increased

---

### TC-OPP-DST-CALC-017: Innovation Premium
**Priority:** P2  
**Test Steps:**
1. Innovative approach identified
2. Calculate innovation adjustment

**Expected Results:**
- Innovation adds complexity
- Also adds impact potential
- Risk adjusted upward
- Learning opportunity noted

---

### TC-OPP-DST-CALC-018: Emergency/Rapid Response Adjustment
**Priority:** P2  
**Test Steps:**
1. Emergency context identified
2. Apply rapid response logic

**Expected Results:**
- Timeline compressed
- Risk tolerance adjusted
- Simplified procedures
- Enhanced monitoring

---

## 3. Scoring Methodology (P0)

### TC-OPP-DST-SCORE-001: Composite Score Calculation
**Priority:** P0  
**Test Steps:**
1. All 9 parameters scored
2. Calculate composite score
3. Verify methodology

**Expected Results:**
```csharp
decimal compositeScore = 0;
compositeScore += param1 * 0.15m; // Strategic alignment
compositeScore += param2 * 0.10m; // Partners
compositeScore += param3 * 0.10m; // Implementation
compositeScore += param4 * 0.15m; // Context
compositeScore += param5 * 0.10m; // Scope
compositeScore += param6 * 0.10m; // Timeframe
compositeScore += param7 * 0.15m; // Budget
compositeScore += param8 * 0.10m; // Impact
compositeScore += param9 * 0.05m; // Safeguards
// Result: 0-100
```

---

### TC-OPP-DST-SCORE-002: Complexity Band Classification
**Priority:** P0  
**Test Steps:**
1. Calculate complexity score
2. Classify into band

**Expected Results:**
- Score 0-3: Low complexity (Band 1)
- Score 3-5: Medium complexity (Band 2)
- Score 5-7: High complexity (Band 3)
- Score 7-10: Very high complexity (Band 4)
- Band determines approval level

---

### TC-OPP-DST-SCORE-003: Risk Category Assignment
**Priority:** P0  
**Test Steps:**
1. Calculate risk score
2. Assign risk category

**Expected Results:**
- Score 0-3: Low risk (Green)
- Score 3-5: Medium risk (Yellow)
- Score 5-7: High risk (Orange)
- Score 7-10: Very high risk (Red)
- Color coding in reports

---

### TC-OPP-DST-SCORE-004: Confidence Intervals
**Priority:** P1  
**Test Steps:**
1. Calculate scores
2. Determine confidence intervals

**Expected Results:**
- High confidence: ±5%
- Medium confidence: ±10%
- Low confidence: ±20%
- Based on data completeness
- Ranges shown in reports

---

### TC-OPP-DST-SCORE-005: Normalized Scoring
**Priority:** P1  
**Test Steps:**
1. Different parameters on different scales
2. Normalize to 0-100

**Expected Results:**
- All parameters normalized
- Consistent scale across profile
- Easy comparison
- Original values preserved

---

### TC-OPP-DST-SCORE-006: Score Validation
**Priority:** P0  
**Test Steps:**
1. Invalid input values
2. Verify validation catches

**Expected Results:**
- Scores must be 0-100
- Negative values rejected
- Values >100 rejected
- NaN/null handled gracefully

---

## 4. Recommendation Logic (P1)

### TC-OPP-DST-REC-001: Generate Risk Recommendations
**Priority:** P1  
**Test Steps:**
1. High-risk parameters identified
2. Generate recommendations

**Expected Results:**
- Specific risks listed
- Mitigation strategies suggested
- Responsibility assigned
- Priority ranking applied

---

### TC-OPP-DST-REC-002: Generate Personnel Recommendations
**Priority:** P1  
**Test Steps:**
1. Complexity and context analyzed
2. Recommend specialized roles

**Expected Results:**
- Required expertise identified
- Justification provided
- Availability checked
- Alternative options suggested

---

### TC-OPP-DST-REC-003: Generate Structure Recommendations
**Priority:** P1  
**Test Steps:**
1. Analyze scope and scale
2. Recommend structure

**Expected Results:**
- Project/Programme/Portfolio suggested
- Rationale clear
- Governance implications outlined
- Resource implications noted

---

### TC-OPP-DST-REC-004: Generate Process Recommendations
**Priority:** P1  
**Test Steps:**
1. Identify required processes
2. Generate recommendations

**Expected Results:**
- Environmental assessment needed
- Social impact assessment required
- Procurement strategy defined
- Timeline for each process

---

## Summary

**Total Test Cases:** 40+  
**Critical (P0):** 28  
**High (P1):** 18  
**Medium (P2):** 4

**Execution Time:** ~12-15 minutes  
**Dependencies:** Opportunity, Country profiles, Historical data

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `DSTProfilerTests.cs`  
**Status:** ✅ Ready for Implementation
