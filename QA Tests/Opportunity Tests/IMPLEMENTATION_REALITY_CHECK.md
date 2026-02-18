# Opportunity Tests - Implementation Reality Check

**Created:** January 15, 2026  
**Status:** ⚠️ Tests Exist But Don't Compile - Major Refactoring Required

---

## Executive Summary

**Finding:** 484 test method signatures exist across 50 C# test files, BUT they cannot run because they reference **non-existent classes**.

**Root Cause:** Tests were scaffolded with placeholder names that don't match actual codebase implementation.

**Impact:** 
- ❌ Zero tests currently executable  
- ⚠️ Compilation errors prevent test discovery
- 🔧 Major refactoring required before any tests can run

---

## What Exists vs What Tests Expect

### Manager Classes

| **Tests Reference** | **Actually Exists** | **Status** |
|---------------------|---------------------|------------|
| `OpportunityManager` | `UNOPSOpportunityManager` | ❌ Name mismatch |
| `DSTManager` | ❓ Not found | ❌ Missing |
| `DecisionManager` | ❓ Not found | ❌ Missing |
| `OpportunityBudgetManager` | ❓ Not found | ❌ Missing |
| `OpportunityScheduleManager` | ❓ Not found | ❌ Missing |
| `ResourcePlanManager` | ❓ Not found | ❌ Missing |
| `RiskManager` | `UNOPSRiskManager` | ❌ Name mismatch |
| `GlobalIndicesManager` | ❓ Not found | ❌ Missing |

### Model/Request Classes

| **Tests Reference** | **Actually Exists** | **Status** |
|---------------------|---------------------|------------|
| `OpportunityCreateRequest` | `OpportunityRequest` | ❌ Name mismatch |
| `OpportunityUpdateRequest` | `UpdateOpportunityRequest` | ❌ Name mismatch |
| `OpportunityModel` | `OpportunityModel` | ✅ Exists |

### Entity Classes

| **Tests Reference** | **Actually Exists** | **Status** |
|---------------------|---------------------|------------|
| `Domain.Entities.Opportunity` | `Domain.Entities.Opportunity` | ✅ Exists |
| `Country` | `Country` | ✅ Exists |
| `OrganizationUnit` | `OrganizationalUnit` | ❓ Need verification |
| `Currency` | `Currency` | ✅ Exists |

---

## Test File Breakdown

### Manager Tests (133 tests across 9 files)

| File | Test Count | Status |
|------|------------|--------|
| `OpportunityManagerTests.cs` | 30 | ❌ References OpportunityManager (wrong name) |
| `DSTManagerTests.cs` | 18 | ❌ References DSTManager (doesn't exist) |
| `DecisionManagerTests.cs` | 15 | ❌ References DecisionManager (doesn't exist) |
| `OpportunityBudgetManagerTests.cs` | 24 | ❌ References OpportunityBudgetManager (doesn't exist) |
| `OpportunityScheduleManagerTests.cs` | 7 | ❌ References OpportunityScheduleManagerTests (doesn't exist) |
| `ResourcePlanManagerTests.cs` | 6 | ❌ References ResourcePlanManager (doesn't exist) |
| `RiskManagerTests.cs` | 6 | ❌ References RiskManager (should be UNOPSRiskManager) |
| `GlobalIndicesManagerTests.cs` | 5 | ❌ References GlobalIndicesManager (doesn't exist) |
| `ManagerEdgeCaseTests.cs` | 22 | ❌ Multiple manager references |

### Business Logic Tests (124 tests across multiple files)

❌ **All reference non-existent classes**

### Controller Tests (49 tests across multiple files)

❌ **All reference non-existent classes**

### E2E Tests (68 tests across multiple files)

❌ **All reference non-existent classes**

### Services Tests (28 tests across multiple files)

❌ **All reference non-existent classes**

### Other Test Categories (82+ tests)

❌ **All have similar issues**

---

## Actual Codebase Analysis

### What Actually Exists

**Managers in `UNOPS.PAO.UNOPSBusiness\Managers`:**
- ✅ `UNOPSOpportunityManager.cs` - Main opportunity management
- ✅ `UNOPSRiskManager.cs` - Risk management  
- ✅ `UNOPSPartnerManager.cs` - Partner management
- ✅ `UNOPSContactManager.cs` - Contact management
- ✅ `UNOPSInteractionManager.cs` - Interaction management
- ✅ `CommonEntitiesManager.cs` - Common entities
- ✅ `UNOPSGeminiManager.cs` - AI/Gemini integration
- ❌ No separate DST, Decision, Budget, Schedule, ResourcePlan managers found

**Key Methods in `UNOPSOpportunityManager`:**
```csharp
- CreateOpportunityAsync(OpportunityRequest model)
- GetOpportunityAsync(int id)
- GetOpportunityAsync(ClaimsPrincipal user, int id)
- UpdateOpportunityAsync(UpdateOpportunityRequest model)
- DeleteOpportunityAsync(int id)
- GetAllOpportunitiesAsync()
- UpdateOverviewSectionAsync(int id, OverviewSectionRequest request)
- UpdateWhatSectionAsync(int id, WhatSectionRequest request)
- UpdateWhySectionAsync(int id, WhySectionRequest request)
- UpdateWhoSectionAsync(int id, WhoSectionRequest request)
- UpdateWhereSectionAsync(int id, WhereSectionRequest request)
- UpdateWhenSectionAsync(int id, WhenSectionRequest request)
- UpdateTeamSectionAsync(int id, TeamSectionRequest request)
- ApplyAiChangesAsync(int id, ApplyOpportunityAiChangesRequest request)
- CreateOpportunityFromProposalAsync(...)
- GetOpportunityDetailsForAIAsync(int id)
- AssignCreatorAsOpportunityManagerAsync(int opportunityId, int userId)
- GetOpportunitiesByPartnerIdAsync(int partnerId)
```

**Missing Functionality:**
- ❌ No separate DST (Decision Support Tool) manager - functionality may be integrated elsewhere
- ❌ No separate Decision Manager - decisions may be in workflow
- ❌ No separate Budget/Schedule/ResourcePlan managers - may be sections within Opportunity
- ❌ No Global Indices Manager found

---

## Refactoring Required

### Option 1: Full Refactoring (Estimated 40-60 hours)

**Scope:**
1. Map all test references to actual class names (9 hours)
2. Fix all 484 test methods to use correct classes (25 hours)
3. Handle missing functionality:
   - Either stub out missing managers (5 hours)
   - Or consolidate tests into existing managers (10 hours)
4. Fix model/request type references (5 hours)
5. Compile and fix errors (10 hours)
6. Run and fix failing tests (15 hours)

**Result:**
- 400+ tests working (some may need to be skipped for missing features)
- Comprehensive coverage
- Significant time investment

### Option 2: Fresh Start with Working Tests (Estimated 10-15 hours)

**Scope:**
1. Delete scaffolded tests that reference non-existent classes
2. Create NEW tests that match actual codebase:
   - `UNOPSOpportunityManagerTests` - 50 tests for actual methods
   - `UNOPSRiskManagerTests` - 20 tests for risk management
   - Integration tests for actual workflows
3. Use actual model names (`OpportunityRequest`, `UpdateOpportunityRequest`)
4. Test actual functionality, not imagined functionality

**Result:**
- 70-100 working, meaningful tests
- All tests compile and run
- Tests match actual system behavior
- Faster implementation
- Better quality

### Option 3: Hybrid Approach (Estimated 20-30 hours)

**Scope:**
1. Fix high-priority `OpportunityManagerTests.cs` to use `UNOPSOpportunityManager` (3 hours)
2. Keep the 30 existing tests, update references
3. Create new tests for missing managers/functionality (5 hours)
4. Skip/delete tests for non-existent features (2 hours)
5. Compile and validate (3 hours)
6. Implement integration tests for actual workflows (7 hours)

**Result:**
- 100-150 working tests
- Mix of refactored and new
- Covers actual functionality
- Reasonable time investment

---

## Recommendation

### 🎯 **Recommended: Option 2 - Fresh Start with Working Tests**

**Why:**
1. **Quality over Quantity**: 100 working tests > 400 broken tests
2. **Test Actual Behavior**: Tests match reality, not assumptions
3. **Faster Results**: 10-15 hours vs 40-60 hours
4. **Maintainable**: Clean codebase without legacy baggage
5. **Production Ready**: All tests compile, run, and pass

**Implementation Priority:**
1. **P0 (Critical) - 50 tests:**
   - `UNOPSOpportunityManagerTests`
     - CreateOpportunity (5 tests)
     - GetOpportunity (5 tests)
     - UpdateOpportunity (10 tests)
     - DeleteOpportunity (3 tests)
     - Section Updates (15 tests)
     - AI Integration (5 tests)
     - Permissions (7 tests)

2. **P1 (High) - 30 tests:**
   - Integration tests for opportunity workflows
   - Risk management tests
   - Partner/Opportunity relationship tests

3. **P2 (Medium) - 20 tests:**
   - Advanced AI features
   - Complex validations
   - Performance tests

**Deliverable:**
- 100 working, documented tests
- All compile and pass
- Test real system behavior
- Ready for CI/CD pipeline

---

## Next Steps

**If Option 2 (Recommended):**
1. Archive existing scaffolded tests to `QA Tests/Opportunity Tests/Archive/Scaffolded/`
2. Create new `UNOPSOpportunityManagerTests.cs` with 50 working tests
3. Add integration tests for actual workflows
4. All tests compile, run, pass in ~10-15 hours

**If Option 1 (Full Refactoring):**
1. Begin systematic refactoring of all 484 tests
2. Map placeholder names to actual classes
3. Handle missing functionality (stub or skip)
4. Expect 40-60 hours to complete

**If Option 3 (Hybrid):**
1. Refactor `OpportunityManagerTests.cs` only
2. Create new tests for missing areas
3. Mix of refactored + new tests
4. Expect 20-30 hours to complete

---

## User Decision Required

**Question:** Which approach do you want me to take?

- **A)** Fresh start with 100 working tests (10-15 hours) ⭐ **RECOMMENDED**
- **B)** Full refactoring of all 484 tests (40-60 hours)
- **C)** Hybrid approach (20-30 hours)

**Factors to Consider:**
- **Timeline**: How quickly do you need working tests?
- **Coverage**: Do you need 100 quality tests or 400 tests?
- **Maintenance**: Do you want clean code or refactored legacy?
- **Reality**: Test actual features vs imagined features?

---

**Status:** ⏸️ Awaiting user decision on implementation approach

**Created:** January 15, 2026  
**Last Updated:** January 15, 2026
