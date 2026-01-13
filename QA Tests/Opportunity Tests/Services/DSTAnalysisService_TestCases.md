# DSTAnalysisService Test Cases

**Service:** `DSTAnalysisService`  
**Test Count:** 10+  
**Priority:** P1  
**Created:** January 13, 2026

---

## Overview

DST analysis service tests for AI integration, calculation engine, and recommendation generation.

---

## Test Cases

### TC-OPP-DSTSVC-001: Coordinate AI Analysis
**Priority:** P1  
**Test Steps:**
1. Call Gemini API
2. Pass opportunity context
3. Receive analysis
4. Parse and store results

**Expected Results:**
- API call successful
- Rate limiting respected
- Results parsed correctly
- Stored in database

---

### TC-OPP-DSTSVC-002: Calculate Complexity Algorithm
**Priority:** P1  
**Test Steps:**
1. Input all factors
2. Apply weighting
3. Calculate final score
4. Classify into band

**Expected Results:**
- Algorithm deterministic
- Scores reproducible
- Band assignment correct
- Performance < 100ms

---

### TC-OPP-DSTSVC-003: Generate Recommendations
**Priority:** P1  
**Test Steps:**
1. Analyze DST profile
2. Identify issues
3. Generate recommendations
4. Prioritize

**Expected Results:**
- Relevant recommendations
- Prioritized by criticality
- Actionable guidance
- Context-appropriate

---

### TC-OPP-DSTSVC-004: Match Similar Opportunities
**Priority:** P1  
**Test Steps:**
1. Vector similarity calculation
2. Find top matches
3. Extract lessons learned

**Expected Results:**
- Similarity algorithm accurate
- Top 5 matches returned
- Lessons relevant
- Performance acceptable

---

### TC-OPP-DSTSVC-005: Cache Profile Results
**Priority:** P1  
**Test Steps:**
1. Generate profile (expensive)
2. Cache results
3. Retrieve from cache

**Expected Results:**
- Cache hit for repeat requests
- TTL appropriate (24 hours)
- Cache invalidated on data change
- Significant performance gain

---

### TC-OPP-DSTSVC-006: Handle AI Service Failure
**Priority:** P1  
**Test Steps:**
1. Gemini API unavailable
2. Verify fallback

**Expected Results:**
- Graceful degradation
- Fallback to historical averages
- User notified of limitation
- Retry scheduled

---

### TC-OPP-DSTSVC-007: Parameter Calculation Pipeline
**Priority:** P1  
**Test Steps:**
1. Calculate all 9 parameters
2. Verify pipeline efficiency

**Expected Results:**
- Parameters calculated in correct order
- Dependencies resolved
- Parallel processing where possible
- Total time < 5 seconds

---

### TC-OPP-DSTSVC-008: Historical Data Analysis
**Priority:** P2  
**Test Steps:**
1. Query historical opportunities
2. Calculate benchmarks
3. Compare current to historical

**Expected Results:**
- Historical data retrieved efficiently
- Benchmarks calculated correctly
- Comparison meaningful
- Performance good

---

### TC-OPP-DSTSVC-009: Report Generation
**Priority:** P2  
**Test Steps:**
1. Generate PDF report
2. Include charts
3. Format professionally

**Expected Results:**
- PDF generation < 10 seconds
- Charts render correctly
- UNOPS branding applied
- File size reasonable

---

### TC-OPP-DSTSVC-010: Batch Profile Generation
**Priority:** P2  
**Test Steps:**
1. Generate profiles for 10 opportunities
2. Process efficiently

**Expected Results:**
- Batch processing optimized
- Parallel execution
- Progress tracking
- Results available async

---

**Status:** ✅ Ready for Implementation
