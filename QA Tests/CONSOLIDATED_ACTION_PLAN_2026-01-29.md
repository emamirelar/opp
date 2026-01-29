# Consolidated Action Plan - All Recommended Next Steps

**Date**: January 29, 2026  
**Purpose**: Consolidate all "Recommended Next Steps" across QA Tests documentation  
**Sources**: Defect Lists, Test Strategy Enhancements, C# Implementation Analysis, Red Flags Analysis

---

## 🚨 CRITICAL PRIORITY ACTIONS (BLOCKING TESTS)

### 1. DEF-005 Phase 2: Create Manager Classes (URGENT - 4-6 hours)

**Status**: ✅ Phase 1 Complete (Models created), 🔥 Phase 2 URGENT (Managers needed)  
**Impact**: Blocks 1,800 integration tests (47% of marathon suite)  
**Owner**: Development Team

**Action Required**:
Create 9 Manager classes in `UNOPS.PAO.Business/Managers/`:

1. `ContactAnalyticsManager.cs` - Contact analytics operations
2. `LiaisonOfficeManager.cs` - Liaison office operations  
3. `OrganizationHierarchyManager.cs` - Organization hierarchy operations
4. `PermissionManager.cs` - Permission CRUD operations
5. `RoleManager.cs` - Role CRUD operations
6. `UserProfileManager.cs` - User profile operations
7. `SystemAdminManager.cs` - System administration operations
8. `UserManagementManager.cs` - User management operations
9. `EntityConfigurationManager.cs` - Entity configuration operations

**For Each Manager**:
- Create class with `AppDbContext` dependency
- Add stub methods returning default/empty data
- Add to `ManagerWrapper` constructor
- Add to `IManagerWrapper` interface

**Validation**:
```bash
cd "QA Tests/Integration Tests"
dotnet build  # Should compile all 3,820 tests
dotnet test   # Will show which methods tests expect
```

**Expected Result**: 1,800 tests compile and can execute (will fail initially, guiding implementation)

**Next Steps After Phase 2**:
- Phase 3 (20-40 hours): Implement actual business logic
- Phase 4 (10-20 hours): Iterate based on test failures to achieve 90%+ pass rate

---

### 2. DEF-001: Fix Route Permission Guard (HIGH PRIORITY - 2-4 hours)

**Status**: Open - Blocking 29 Playwright tests  
**Impact**: 27.6% of Phase 1A tests blocked, prevents 100% pass rate  
**Owner**: Development Team (Frontend)

**Problem**: `routePermissionGuard` blocks access to detail pages even with valid permissions

**Affected Routes**:
- `/partnerships/partners/{id}`
- `/partnerships/contacts/{id}`
- `/partnerships/interactions/{id}`
- `/opportunities/{id}`

**Action Required**:
1. Review `UNOPS.PAO.ClientApp/src/app/core/guards/route-permission.guard.ts`
2. Verify guard correctly handles permissions from `/api/permissions/check/partnerships/*`
3. Ensure guard properly reads authenticated user claims from `/user/claims`
4. Test with both real backend and API mocks

**Validation**:
```bash
cd "QA Tests/Playwright Tests"
npx playwright test partner-item-basic.spec.ts contact-item-basic.spec.ts interaction-item-basic.spec.ts opportunity-item-basic.spec.ts
```

**Expected Result**: All 105 Phase 1A tests pass (100% pass rate)

**ROI**: 2-4 hours work → 29 tests unlocked, +4% UI coverage, 72.4% → 100% pass rate

---

### 3. DEF-004: Fix AdvancedSearchService for Tests (MEDIUM PRIORITY - 4-6 hours)

**Status**: Open - Blocking 53 Partner integration tests  
**Impact**: 100% of Partner controller tests fail with HTTP 500  
**Owner**: Development Team (Backend)

**Problem**: AdvancedSearchService uses PostgreSQL-specific SQL incompatible with in-memory test database

**Recommended Solution** (Option A):
Use real PostgreSQL test database instead of in-memory database

**Action Required**:
1. Update `PAOWebApplicationFactory.cs` to use PostgreSQL test database
2. Configure test database connection string
3. Run database migrations on test database
4. Update test configuration

**Alternative** (Option B - 2-4 hours):
Mock AdvancedSearchService for tests (faster but doesn't test real search)

**Validation**:
```bash
cd "QA Tests/Integration Tests"
dotnet test --filter "FullyQualifiedName~Partner"
```

**Expected Result**: All 53 Partner tests return HTTP 200 OK, pass rate 92.2% → 96.0%

---

## 🔶 HIGH PRIORITY ACTIONS (COVERAGE IMPROVEMENTS)

### 4. DEF-002 & DEF-003: Add data-testid Attributes (6-12 hours total)

**Status**: Open - Blocks 50-90 Playwright tests (Phase 1B)  
**Impact**: Cannot test specific fields, form validation, button actions  
**Owner**: Development Team (Frontend)

**Components Needing Attributes** (12 files):

**Detail Pages (4 components)**:
- `partner-view.component.html`
- `contact-view.component.html`  
- `interaction-view.component.html`
- `opportunity-view.component.html`

**Create Forms (4 components)**:
- `new-partner.component.html`
- `new-contact.component.html`
- `new-interaction.component.html`
- `create-opportunity.component.html`

**Edit/Delete Forms (4 components)**:
- Partner, Contact, Interaction, Opportunity edit/delete dialogs

**Reference Documents**:
- `Playwright Tests/DATA_TESTID_GUIDE.md` - Complete developer guide
- `Playwright Tests/DATA_TESTID_CHECKLIST.md` - Printable checklist

**Estimated Effort**: 30-60 minutes per component × 12 = 6-12 hours

**Expected Result**: Enables 50-90 Phase 1B tests, achieves 75% UI coverage goal

---

### 5. Red Flags Phase 2: Create 293 New Tests (15-20 hours)

**Status**: Phase 1 Complete (reclassification), Phase 2 Remaining  
**Impact**: Achieve full 3:1 ratio compliance (currently 2.55:1, need 3:1)  
**Owner**: QA Team

**Required Tests**:
- ~147 Negative tests
- ~146 Edge case tests

**Focus Areas**:
1. **PartnerControllerFullTests.cs** (116 positive → needs ~150 more neg/edge)
2. **TranslationControllerTests.cs** (82 positive → needs ~100 more neg/edge)
3. **ImportControllerTests.cs** (41 positive → needs ~50 more neg/edge)

**Use Templates**:
- "Three C's" framework (Crashes, Corruption, Compliance)
- Domain-specific edge cases (financial, temporal, workflow)
- Combinatorial testing for complex forms

**Validation**:
```bash
cd "QA Tests/Integration Tests"
dotnet test
# Calculate ratio: (Negative + Edge) ≥ 3 × Positive
```

**Expected Result**: (Negative + Edge) ≥ 3 × Positive, full 3:1 compliance ✅

---

## 📋 MEDIUM PRIORITY ACTIONS (EXPAND COVERAGE)

### 6. C# Implementation Phase 1: Controller Tests (3-4 weeks)

**Status**: Not started  
**Impact**: 520 API endpoint tests, full HTTP contract validation  
**Owner**: QA Team

**Implementation Order**:

**Week 1-2: Core Controllers**
- DashboardController (45 tests)
- ConfigurationController (25 tests)
- EntityConfigurationController (40 tests)
- CommonEntitiesController (30 tests)

**Week 3-4: Specialized Controllers**
- AnalyticsController (40 tests)
- RoleController (35 tests)
- PermissionController (35 tests)
- UserProfileController (30 tests)

**Templates Available**:
- See existing controller tests in `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Controllers/`
- Follow pattern: `{ControllerName}ControllerTests.cs`
- Use WebApplicationFactory for API testing

**Expected Deliverable**: 520+ API endpoint tests, 100% controller coverage

---

### 7. C# Implementation Phase 2: Business Logic Tests (2-3 weeks)

**Status**: 8 files remaining  
**Impact**: 422 business rule tests, validation logic coverage  
**Owner**: QA Team

**Remaining Files**:
1. `ContactManager_BusinessLogic_TestCases.md` (45 tests)
2. `DocumentManager_BusinessLogic_TestCases.md` (52 tests)
3. `InteractionManager_BusinessLogic_TestCases.md` (48 tests)
4. `PartnerManager_BusinessLogic_TestCases.md` (56 tests)
5. `OrganizationHierarchyManager_BusinessLogic_TestCases.md` (35 tests)
6. `DataImportFixes_TestCases.md` (28 tests)
7-14. Opportunity Business Logic (6 files, ~158 tests)

**Templates Available**:
- Location: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/BusinessLogic/`
- Pattern: `{Feature}BusinessLogicTests.cs`
- Use in-memory database for isolated testing

**Expected Deliverable**: 422+ business rule tests, validation logic coverage

---

### 8. C# Implementation Phase 3: Unit Tests (4-5 weeks)

**Status**: 27 files remaining  
**Impact**: 1,020 unit tests, isolated component validation  
**Owner**: QA Team

**Approach**:
- Group by feature area (5-6 managers per week)
- Focus on mocking external dependencies
- Use Moq for dependency injection
- Isolate each component for testing

**Reference**:
- See `Unit Tests/Business/` folder for specifications
- Pattern: `{ManagerName}UnitTests.cs`
- Location: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/`

**Expected Deliverable**: 1,020+ unit tests, 100% manager isolation coverage

---

### 9. C# Implementation Phase 4: Edge Cases & Security (1-2 weeks)

**Status**: 4 files remaining  
**Impact**: 135 edge case/security tests  
**Owner**: QA Team

**Remaining Files**:
1. `AuditTrail_TestCases.md` (30 tests)
2. `BulkOperations_TestCases.md` (35 tests)
3. `DataIntegrity_TestCases.md` (40 tests)
4. `ErrorRecovery_Resilience_TestCases.md` (30 tests)

**Templates Available**:
- Location: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/EdgeCases/`
- Use patterns from comprehensive test strategy
- Focus on boundary testing and attack prevention

**Expected Deliverable**: 135+ edge case tests, comprehensive robustness validation

---

## 🔵 LOW PRIORITY / INFORMATIONAL

### 10. DEF-006: Monitor .NET 9 PipeWriter Issue (2-4 hours)

**Status**: Open - Test infrastructure issue  
**Impact**: Intermittent test failures, not production blocker  
**Owner**: QA Team

**Action**: Monitor .NET 9 GitHub issues for resolution, implement workaround when available

---

### 11. Test Strategy Enhancements (COMPLETE ✅)

**Status**: Complete - No action needed  
**Documentation**: All enhancements documented and implemented

**Completed**:
- ✅ 3:1 ratio refinement
- ✅ "Three C's" framework (Crashes, Corruption, Compliance)
- ✅ Domain-specific edge cases (financial, temporal, workflow)
- ✅ Combinatorial testing framework
- ✅ 1,316 lines added to comprehensive test strategy

---

## 📊 Priority Matrix

| Action | Priority | Effort | Impact | Owner | Status |
|--------|----------|--------|--------|-------|--------|
| **DEF-005 Phase 2** | 🔥 CRITICAL | 4-6h | 1,800 tests | Dev | URGENT |
| **DEF-001 Fix** | 🔴 HIGH | 2-4h | 29 tests | Dev | Open |
| **DEF-004 Fix** | 🔴 HIGH | 4-6h | 53 tests | Dev | Open |
| **DEF-002/003** | 🟠 HIGH | 6-12h | 50-90 tests | Dev | Open |
| **Red Flags Phase 2** | 🟠 HIGH | 15-20h | 3:1 compliance | QA | Open |
| **C# Phase 1** | 🟡 MEDIUM | 3-4wk | 520 tests | QA | Not started |
| **C# Phase 2** | 🟡 MEDIUM | 2-3wk | 422 tests | QA | Not started |
| **C# Phase 3** | 🟡 MEDIUM | 4-5wk | 1,020 tests | QA | Not started |
| **C# Phase 4** | 🟡 MEDIUM | 1-2wk | 135 tests | QA | Not started |
| **DEF-006** | 🔵 LOW | 2-4h | Monitoring | QA | Open |

---

## 🎯 Recommended Execution Order

### Sprint 1 (IMMEDIATE - 1-2 weeks):
1. ✅ **DEF-005 Phase 2** (4-6 hours) - URGENT: Unblocks 1,800 tests
2. ✅ **DEF-001** (2-4 hours) - Quick win: 100% Playwright pass rate
3. ✅ **DEF-004** (4-6 hours) - Unblocks 53 Partner tests

**Deliverable**: 1,879 tests unblocked, critical infrastructure complete

### Sprint 2 (2-3 weeks):
4. **DEF-002/003** (6-12 hours) - Enables Phase 1B Playwright tests
5. **Red Flags Phase 2** (15-20 hours) - Achieves 3:1 ratio compliance

**Deliverable**: 50-90 UI tests enabled, 3:1 compliance achieved

### Sprint 3-6 (3-4 months):
6. **C# Phases 1-4** (10-14 weeks) - Complete backend test coverage

**Deliverable**: 2,097 new C# tests, 95%+ backend coverage

---

## 📈 Expected Outcomes

### After Sprint 1 (Critical Actions):
- ✅ 1,879 tests unblocked (DEF-001, DEF-004, DEF-005)
- ✅ 100% Playwright Phase 1A pass rate
- ✅ 96.0% integration test pass rate
- ✅ All 3,820 marathon tests compile and execute

### After Sprint 2 (High Priority):
- ✅ 75% UI test coverage (Phase 1B complete)
- ✅ 3:1 ratio compliance (Red Flags cleared)
- ✅ 50% UI coverage milestone achieved

### After Sprint 3-6 (C# Implementation):
- ✅ 95%+ backend coverage
- ✅ 2,097 new C# tests
- ✅ 100% controller coverage
- ✅ 100% business logic coverage
- ✅ 100% unit test coverage

---

## 🚀 Getting Started

### For Development Team:

**Immediate Actions**:
1. Review DEF-005 Phase 2 requirements (9 managers)
2. Review DEF-001 route guard issue
3. Review DEF-004 AdvancedSearchService issue
4. Prioritize based on team capacity

**Resources**:
- Defect List: `QA Tests/Defect List for Developers.md`
- Test Expectations: `QA Tests/Integration Tests/` folders
- UI Requirements: `QA Tests/Playwright Tests/DATA_TESTID_GUIDE.md`

### For QA Team:

**Immediate Actions**:
1. Continue Red Flags Phase 2 (293 tests remaining)
2. Plan C# Implementation Phase 1 (Controller tests)
3. Document test creation patterns

**Resources**:
- Test Strategy: `.cursor/rules/comprehensive-test-strategy.mdc`
- Templates: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/`
- Analysis: `QA Tests/C# IMPLEMENTATION_ANALYSIS.md`

---

## 📁 Key Documents Referenced

1. `Defect List for Developers.md` - 6 open defects (DEF-001 to DEF-006)
2. `RED_FLAGS_RECLASSIFICATION_ANALYSIS_2026-01-28.md` - Phase 1 complete, Phase 2 plan
3. `C# IMPLEMENTATION_ANALYSIS.md` - Complete roadmap for C# test expansion
4. `COMPREHENSIVE_TEST_STRATEGY_ENHANCEMENTS_SUMMARY_2026-01-28.md` - Strategy updates
5. `3-1_RATIO_UPDATE_2026-01-28.md` - Updated 3:1 ratio rules

---

## ✅ Summary

**Total Actionable Items**: 11 (1 critical, 4 high priority, 5 medium priority, 1 low priority)

**Estimated Total Effort**:
- Critical/High Priority: 33-48 hours (Sprint 1-2)
- Medium Priority: 10-14 weeks (Sprint 3-6)
- Low Priority: 2-4 hours (monitoring)

**Expected Results**:
- 1,879 tests unblocked immediately
- 3:1 ratio compliance achieved
- 95%+ backend coverage in 3-4 months
- Production-ready test suite

**Next Steps**: Development team to prioritize DEF-005 Phase 2, DEF-001, and DEF-004 in Sprint 1

---

**Created**: January 29, 2026  
**Status**: Active - Ready for execution  
**Review**: Update as actions complete
