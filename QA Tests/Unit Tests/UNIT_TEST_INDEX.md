# Unit Test Cases - Complete Index

**Project**: UNOPS Opportunity+ System  
**Purpose**: Prevent defects like PNO-686, PNO-680, PNO-677, PNO-676  
**Total Planned Tests**: 300+  
**Target Coverage**: 75%+ overall, 90%+ for critical components

---

## 📊 Executive Summary

### Current Status: Phase 1 Initiated

| Metric | Status | Progress |
|--------|--------|----------|
| **Specifications Created** | 1 of 13 | 8% |
| **Test Cases Specified** | 40 of 300+ | 13% |
| **Tests Implemented** | 0 of 300+ | 0% |
| **Coverage Achieved** | 0% | 0% |

### Immediate Next Steps

1. ✅ **DONE**: PartnerManager spec created (40+ tests)
2. 📝 **NEXT**: Implement PartnerManager tests (2 days)
3. 📝 **NEXT**: Create ContactManager spec (1 day)
4. 📝 **NEXT**: Create InteractionManager spec (1 day)

---

## 📁 File Structure

```
Test Cases/Unit Tests/
├── README.md                                    ← Main overview
├── UNIT_TEST_INDEX.md                          ← This file
│
└── Business/
    ├── README.md                                ← Business layer overview
    │
    ├── PartnerManager/
    │   └── UNOPSPartnerManager_UnitTests.md    ✅ COMPLETE (40+ tests)
    │
    ├── ContactManager/
    │   └── UNOPSContactManager_UnitTests.md    📝 PENDING (35+ tests)
    │
    ├── InteractionManager/
    │   └── UNOPSInteractionManager_UnitTests.md 📝 PENDING (30+ tests)
    │
    ├── DocumentManager/
    │   └── UNOPSDocumentManager_UnitTests.md   📝 PENDING (25+ tests)
    │
    ├── GeminiManager/
    │   └── UNOPSGeminiManager_UnitTests.md     📝 PENDING (30+ tests)
    │
    ├── NotificationManager/
    │   └── NotificationManager_UnitTests.md     📝 PENDING (20+ tests)
    │
    ├── WorkflowManager/
    │   └── WorkflowManager_UnitTests.md         📝 PENDING (15+ tests)
    │
    ├── UserDataManager/
    │   └── UserDataManager_UnitTests.md         📝 PENDING (15+ tests)
    │
    ├── SystemAdminManager/
    │   └── UNOPSSystemAdminManager_UnitTests.md 📝 PENDING (20+ tests)
    │
    ├── EntityConfigurationManager/
    │   └── UNOPSEntityConfigurationManager_UnitTests.md 📝 PENDING (15+ tests)
    │
    ├── PartnerTreeManager/
    │   └── UNOPSPartnerTreeManager_UnitTests.md 📝 PENDING (15+ tests)
    │
    ├── GoogleDriveDocumentManager/
    │   └── GoogleDriveDocumentManager_UnitTests.md 📝 PENDING (20+ tests)
    │
    └── AiContextualService/
        └── AiContextualService_UnitTests.md     📝 PENDING (25+ tests)
```

---

## 📋 Manager Test Specifications

### ✅ Completed Specifications

#### 1. UNOPSPartnerManager (CRITICAL - PNO-686)
**File**: `Business/PartnerManager/UNOPSPartnerManager_UnitTests.md`

**Test Categories**:
1. GetNextErpDimValueAsync Tests (10 tests) - **CRITICAL for PNO-686**
   - Normal sequence generation
   - Reserved range exclusion (8000-9999)
   - Empty database scenario
   - Boundary value testing
   - Null value handling
   - Deleted partners inclusion
   - Performance testing

2. ApprovePartnerAsync Tests (6 tests)
   - Successful approval flow
   - ErpDimValue assignment
   - Permission validation
   - Audit trail recording

3. UnapprovePartnerAsync Tests (2 tests)
   - Successful unapproval
   - Validation checks

4. CreatePartnerAsync Tests (3 tests)
   - Successful creation
   - Validation rules
   - Duplicate detection

5. UpdatePartnerAsync Tests (3 tests)
   - Field updates
   - Permission checks
   - Approved partner handling

6. Delete/Archive Tests (3 tests)
   - Soft delete
   - Archive workflow
   - Permission validation

7. GetPartnerByIdAsync Tests (3 tests)
   - Successful retrieval
   - Not found scenarios
   - Deleted partner handling

8. GetPartnersAsync Tests (4 tests)
   - Listing and pagination
   - Filtering
   - Search functionality
   - RBAC filtering

9. Permission/RBAC Tests (2 tests)
   - Org unit filtering
   - Admin access

10. Validation Tests (2 tests)
    - Boolean field handling
    - Special flag validation

11. Concurrent Access Tests (1 test)
    - Concurrent approvals

12. Mapping Tests (1 test)
    - Entity to model mapping

**Total**: 40 tests  
**Priority**: CRITICAL  
**Status**: ✅ Specification Complete  
**Target Coverage**: 90%+

---

### 📝 Pending Specifications

#### 2. UNOPSContactManager (HIGH - PNO-676, PNO-677)
**Planned Tests**: 35+  
**Focus Areas**:
- CRUD operations (8 tests)
- **Duplicate detection** (10 tests) - PNO-676
  - Create with duplicate check
  - Update duplicate status
  - Import workflow duplicate detection
  - Inline edit re-validation
- **Advanced search** (8 tests) - PNO-677
  - Name equals/contains
  - Boolean field search
  - Date field search
  - Related entity search
- Validation (5 tests)
- Permissions (4 tests)

**Priority**: HIGH  
**Target Coverage**: 85%+

---

#### 3. UNOPSInteractionManager (HIGH)
**Planned Tests**: 30+  
**Focus Areas**:
- CRUD operations (8 tests)
- Partner relationship validation (6 tests)
- Contact relationship validation (6 tests)
- Interaction type handling (4 tests)
- Date validation (3 tests)
- Permissions (3 tests)

**Priority**: HIGH  
**Target Coverage**: 85%+

---

#### 4. UNOPSGeminiManager (HIGH)
**Planned Tests**: 30+  
**Focus Areas**:
- AI prompt execution (8 tests)
- Error handling (6 tests)
- Response parsing (5 tests)
- Token management (4 tests)
- Context building (4 tests)
- Rate limiting (3 tests)

**Priority**: HIGH  
**Target Coverage**: 80%+

---

#### 5. UNOPSDocumentManager (MEDIUM)
**Planned Tests**: 25+  
**Focus Areas**:
- File upload (6 tests)
- File download (4 tests)
- File validation (6 tests)
- Storage management (5 tests)
- Permissions (4 tests)

**Priority**: MEDIUM  
**Target Coverage**: 80%+

---

#### 6. GoogleDriveDocumentManager (MEDIUM - PNO-680 Related)
**Planned Tests**: 20+  
**Focus Areas**:
- Drive upload (5 tests)
- Drive download (3 tests)
- **Configuration validation** (4 tests) - PNO-680 related
- **Error handling** (4 tests) - External service failures
- Authentication (4 tests)

**Priority**: MEDIUM  
**Target Coverage**: 75%+

---

#### 7. SystemAdminManager (MEDIUM)
**Planned Tests**: 20+  
**Focus Areas**:
- Seeding operations (6 tests)
- Configuration management (5 tests)
- System diagnostics (4 tests)
- User management (5 tests)

**Priority**: MEDIUM  
**Target Coverage**: 75%+

---

#### 8. NotificationManager (MEDIUM)
**Planned Tests**: 20+  
**Focus Areas**:
- Notification creation (6 tests)
- Notification delivery (5 tests)
- Template processing (4 tests)
- Recipient management (5 tests)

**Priority**: MEDIUM  
**Target Coverage**: 75%+

---

#### 9. WorkflowManager (MEDIUM)
**Planned Tests**: 15+  
**Focus Areas**:
- State transitions (6 tests)
- Validation rules (4 tests)
- Audit tracking (3 tests)
- Permissions (2 tests)

**Priority**: MEDIUM  
**Target Coverage**: 75%+

---

#### 10. UserDataManager (MEDIUM)
**Planned Tests**: 15+  
**Focus Areas**:
- User preferences (5 tests)
- Profile management (4 tests)
- Settings persistence (3 tests)
- Validation (3 tests)

**Priority**: MEDIUM  
**Target Coverage**: 70%+

---

#### 11. EntityConfigurationManager (MEDIUM)
**Planned Tests**: 15+  
**Focus Areas**:
- Field configuration (6 tests)
- Entity metadata (4 tests)
- Validation rules (3 tests)
- Dynamic fields (2 tests)

**Priority**: MEDIUM  
**Target Coverage**: 75%+

---

#### 12. PartnerTreeManager (MEDIUM)
**Planned Tests**: 15+  
**Focus Areas**:
- Hierarchy management (6 tests)
- Parent-child relationships (4 tests)
- Tree operations (3 tests)
- Validation (2 tests)

**Priority**: MEDIUM  
**Target Coverage**: 70%+

---

#### 13. AiContextualService (MEDIUM)
**Planned Tests**: 25+  
**Focus Areas**:
- Similarity search (8 tests)
- Embedding generation (6 tests)
- Context retrieval (5 tests)
- Vector operations (4 tests)
- Cache management (2 tests)

**Priority**: MEDIUM  
**Target Coverage**: 75%+

---

## 🎯 Defect Prevention Mapping

Each test specification directly addresses production defects:

### PNO-686: Partner Code Generation
**Tests**: TC-PM-001 through TC-PM-010 (PartnerManager)  
**Coverage**: 10 tests specifically for ErpDimValue generation  
**Focus**: Reserved range (8000-9999), boundary values, edge cases

**Key Tests**:
- ✅ TC-PM-002: Skip reserved range
- ✅ TC-PM-003: Empty database
- ✅ TC-PM-004-006: Boundary values
- ✅ TC-PM-008: Include deleted partners

---

### PNO-680: Export Configuration Failure
**Tests**: GoogleDriveDocumentManager tests (planned)  
**Coverage**: 4 configuration validation tests + 4 error handling tests  
**Focus**: Startup validation, external service errors

**Key Tests**:
- 📝 Configuration validation on startup
- 📝 Missing credentials handling
- 📝 External service unavailable
- 📝 Authentication failures

---

### PNO-677: Advanced Search Field Issues
**Tests**: ContactManager search tests (planned)  
**Coverage**: 8 advanced search tests  
**Focus**: Boolean, date, text, related entity fields

**Key Tests**:
- 📝 Boolean field search (pooledFund, keyGlobalPartner)
- 📝 Date field search (approvalDate)
- 📝 Text field equals vs. contains
- 📝 Related entity search (liaisonOffice.name)

---

### PNO-676: Duplicate Detection
**Tests**: ContactManager duplicate detection tests (planned)  
**Coverage**: 10 duplicate detection tests  
**Focus**: State management, inline edits, re-validation

**Key Tests**:
- 📝 Initial duplicate detection
- 📝 Edit triggers re-validation
- 📝 UI state updates after detection
- 📝 Import workflow integration

---

## 📅 Implementation Timeline

### Week 1-2: Phase 1 (Critical)
**Goal**: Prevent PNO-686 recurrence

- [x] Week 1 Day 1-2: Create PartnerManager spec (✅ DONE)
- [ ] Week 1 Day 3-4: Implement PartnerManager tests
- [ ] Week 1 Day 5: Create ContactManager spec
- [ ] Week 2 Day 1-3: Implement ContactManager tests
- [ ] Week 2 Day 4: Create InteractionManager spec
- [ ] Week 2 Day 5: Implement InteractionManager tests

**Deliverables**:
- 3 manager test suites (100+ tests total)
- 30% overall coverage
- Critical defect prevention in place

---

### Week 3-4: Phase 2 (High Priority)
**Goal**: Cover core business logic

- [ ] Week 3: GeminiManager, DocumentManager specs & implementation
- [ ] Week 4: GoogleDriveDocumentManager spec & implementation

**Deliverables**:
- 3 additional manager test suites (75+ tests)
- 60% overall coverage
- AI and document management tested

---

### Week 5-6: Phase 3 (Remaining)
**Goal**: Complete coverage

- [ ] Week 5: 4 remaining manager specs & implementation
- [ ] Week 6: Final 3 managers & cleanup

**Deliverables**:
- All 13 managers tested (300+ tests)
- 75%+ overall coverage
- All quality gates passing

---

## 💻 Developer Quick Start

### 1. Read Your Assigned Spec
```bash
# Example: Partner Manager
cat "Test Cases/Unit Tests/Business/PartnerManager/UNOPSPartnerManager_UnitTests.md"
```

### 2. Create Test Project (if needed)
```bash
cd tests/Unit
dotnet new xunit -n UNOPS.PAO.Business.Tests
dotnet sln add UNOPS.PAO.Business.Tests
cd UNOPS.PAO.Business.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package coverlet.collector
```

### 3. Implement Tests
```csharp
// Example: TC-PM-002 from specification
[Fact]
public async Task GetNextErpDimValue_Should_Skip_ReservedRange_When_ValuesInRange8000To9999Exist()
{
    // Copy Arrange/Act/Assert from specification
    // ...
}
```

### 4. Run Tests
```bash
dotnet test
dotnet test /p:CollectCoverage=true
```

---

## 📈 Success Metrics

### Coverage Targets

| Week | Overall | Critical Managers | Business Layer |
|------|---------|-------------------|----------------|
| **Week 2** | 20% | 80% | 30% |
| **Week 4** | 40% | 90% | 60% |
| **Week 6** | 65% | 90%+ | 80% |
| **Final** | **75%+** | **90%+** | **85%+** |

### Quality Gates

- ✅ All tests passing: **100%**
- ✅ No flaky tests: **0**
- ✅ Test execution time: **< 5 minutes**
- ✅ Coverage decrease: **Not allowed**

---

## 📚 Resources

### Documentation
- [Unit Tests README](./README.md) - Overview and getting started
- [Business Tests README](./Business/README.md) - Business layer details
- [Backend Testing Guide](../../docs/Development/BACKEND_TESTING_GUIDE.md)

### Implementation Guides
- [Implementation Action Plan](../../Test Execution Results/Recommendations/IMPLEMENTATION_ACTION_PLAN.md)
- [Defect Prevention Recommendations](../../Test Execution Results/Recommendations/DEFECT_ANALYSIS_AND_PREVENTION_RECOMMENDATIONS.md)

### Status Reports
- [Implementation Status](../../Test Execution Results/Recommendations/IMPLEMENTATION_STATUS_REPORT.md)
- [Manager Summary](../../Test Execution Results/Recommendations/STATUS_SUMMARY_FOR_MANAGER.md)

---

## ✅ Implementation Checklist

### Setup
- [ ] Create `UNOPS.PAO.Business.Tests` project
- [ ] Install required packages
- [ ] Configure code coverage
- [ ] Create base test classes
- [ ] Set up test data factories

### Phase 1 (Week 1-2)
- [x] PartnerManager spec created
- [ ] PartnerManager tests implemented (40 tests)
- [ ] ContactManager spec created
- [ ] ContactManager tests implemented (35 tests)
- [ ] InteractionManager spec created
- [ ] InteractionManager tests implemented (30 tests)
- [ ] 30% coverage achieved

### Phase 2 (Week 3-4)
- [ ] GeminiManager spec created
- [ ] GeminiManager tests implemented (30 tests)
- [ ] DocumentManager spec created
- [ ] DocumentManager tests implemented (25 tests)
- [ ] GoogleDriveDocumentManager spec created
- [ ] GoogleDriveDocumentManager tests implemented (20 tests)
- [ ] 60% coverage achieved

### Phase 3 (Week 5-6)
- [ ] All remaining specs created (7 managers)
- [ ] All remaining tests implemented (100+ tests)
- [ ] 75%+ coverage achieved
- [ ] All quality gates passing
- [ ] CI/CD integration complete

---

## 🚀 Next Actions

### This Week
1. ✅ **DONE**: PartnerManager spec created
2. **Assign** developer to implement PartnerManager tests (2 days)
3. **Create** ContactManager spec (1 day)
4. **Review** PartnerManager test implementation

### Next Week
1. **Complete** ContactManager tests
2. **Create** InteractionManager spec
3. **Achieve** 30% coverage milestone

---

**Document Created**: January 2025  
**Last Updated**: January 2025  
**Status**: Phase 1 Initiated - PartnerManager Spec Complete  
**Next Review**: After PartnerManager tests implemented

