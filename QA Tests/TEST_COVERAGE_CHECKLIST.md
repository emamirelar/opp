# Unit Test Coverage - Quick Checklist

**Use this checklist to track your test implementation progress.**

---

## 🔴 **Phase 1: Critical Gaps (Weeks 1-4)**

**Goal**: Address P0 critical gaps  
**Target**: Add ~750 tests, reach 55% coverage

### **A. Controller Tests (Priority P0)**

#### **Create Test Project:**
- [ ] Create `UNOPS.PAO.Presentation.Tests` project
- [ ] Add xUnit + Moq dependencies
- [ ] Create `ControllerTestBase.cs`
- [ ] Set up test data factories
- [ ] Configure CI/CD integration

#### **P0 Controllers (Must Test First):**
- [ ] **PartnerController** (~44 tests)
  - [ ] Authorization tests (8)
  - [ ] CRUD operations (15)
  - [ ] Validation tests (8)
  - [ ] Error handling (8)
  - [ ] Permission checks (5)

- [ ] **ContactController** (~44 tests)
  - [ ] Authorization tests (8)
  - [ ] CRUD operations (15)
  - [ ] Validation tests (8)
  - [ ] Error handling (8)
  - [ ] Permission checks (5)

- [ ] **InteractionController** (~44 tests)
  - [ ] Authorization tests (8)
  - [ ] CRUD operations (15)
  - [ ] Validation tests (8)
  - [ ] Error handling (8)
  - [ ] Permission checks (5)

- [ ] **OpportunityController** (~44 tests)
  - [ ] Authorization tests (8)
  - [ ] CRUD operations (15)
  - [ ] Validation tests (8)
  - [ ] Error handling (8)
  - [ ] Permission checks (5)

**Subtotal P0 Controllers**: ~176 tests

---

### **B. Missing Manager FullTests (Priority P0-P1)**

- [ ] **AuditLogManagerFullTests** (~50 tests) - P0 CRITICAL
  - [ ] CRUD operations (15)
  - [ ] Audit trail logic (15)
  - [ ] Change tracking (10)
  - [ ] Edge cases (5)
  - [ ] Soft delete tests (5)

- [ ] **CommentManagerFullTests** (~50 tests) - P0
  - [ ] CRUD operations (15)
  - [ ] Comment threading (10)
  - [ ] Notification integration (10)
  - [ ] Edge cases (10)
  - [ ] Soft delete tests (5)

- [ ] **EntityArtifactManagerFullTests** (~50 tests) - P1
  - [ ] CRUD operations (15)
  - [ ] Metadata management (15)
  - [ ] File handling (10)
  - [ ] Edge cases (5)
  - [ ] Soft delete tests (5)

- [ ] **AiRetrieverManagerFullTests** (~50 tests) - P1
  - [ ] Document retrieval (15)
  - [ ] AI integration (15)
  - [ ] Search functionality (10)
  - [ ] Edge cases (5)
  - [ ] Soft delete tests (5)

**Subtotal Missing Managers**: ~200 tests

---

**Phase 1 Total**: ~376 tests  
**Expected Coverage**: 40% → 55%

---

## 🟡 **Phase 2: Important Gaps (Weeks 5-8)**

**Goal**: Complete all controller and manager tests  
**Target**: Add ~1,000 tests, reach 75% coverage

### **C. Remaining Controllers (Priority P1-P2)**

#### **P1 Controllers:**
- [ ] **DocumentController** (~44 tests)
- [ ] **UserManagementController** (~44 tests)
- [ ] **WorkflowController** (~44 tests)
- [ ] **SystemAdminController** (~44 tests)
- [ ] **GeminiController** (~44 tests)
- [ ] **NotificationController** (~44 tests)
- [ ] **LinkController** (~44 tests)
- [ ] **OrganizationHierarchyController** (~44 tests)

**Subtotal P1 Controllers**: ~352 tests

#### **P2 Controllers (Remaining 25):**
- [ ] Admin Controllers (4 remaining)
- [ ] User Controllers (2 remaining)
- [ ] Document Controllers (1 remaining)
- [ ] Location Controllers (1 remaining)
- [ ] Shared Controllers (5 remaining)
- [ ] Partner Tree Controllers (3 remaining)
- [ ] Liaison Office Controllers (2 remaining)
- [ ] Dashboard Controller (1 remaining)
- [ ] Other Controllers (6 remaining)

**Subtotal P2 Controllers**: ~1,100 tests

---

### **D. Upgrade Basic Manager Tests to FullTests**

- [ ] **PartnerManagerFullTests** (~50 tests)
  - [ ] Upgrade from basic tests
  - [ ] Add edge cases
  - [ ] Add business logic tests
  - [ ] Add soft delete tests

- [ ] **OpportunityManagerFullTests** (~50 tests)
  - [ ] Upgrade from integration tests
  - [ ] Add unit-level tests
  - [ ] Add validation tests

- [ ] **ValuesManagerFullTests** (~50 tests)
  - [ ] Upgrade from basic tests
  - [ ] Add lookup tests
  - [ ] Add caching tests

- [ ] **GmailAddonManagerFullTests** (~50 tests)
  - [ ] Upgrade from basic tests
  - [ ] Add Gmail integration tests
  - [ ] Add authorization tests

- [ ] **DocumentTypeManagerFullTests** (~50 tests)
  - [ ] Upgrade from basic tests
  - [ ] Add type management tests
  - [ ] Add validation tests

- [ ] **GeminiManagerFullTests** (~50 tests)
  - [ ] Create dedicated tests (separate from SystemAdmin)
  - [ ] Add AI integration tests
  - [ ] Add prompt management tests

**Subtotal Manager Upgrades**: ~300 tests

---

### **E. UNOPS Override Testing**

- [ ] Create `UNOPS.PAO.UNOPSBusiness.Tests` project
- [ ] Set up UNOPS-specific test infrastructure
- [ ] Add UNOPS override manager tests (~15 files, ~600 tests)
- [ ] Add UNOPS entity validation tests
- [ ] Add UNOPS business rule tests

**Subtotal UNOPS Tests**: ~600 tests

---

### **F. Integration Test Expansion**

- [ ] Partner-Contact-Interaction workflow (10 tests)
- [ ] Document management end-to-end (8 tests)
- [ ] Opportunity creation full flow (12 tests)
- [ ] User management workflows (10 tests)
- [ ] Notification delivery chain (8 tests)
- [ ] AI document processing pipeline (10 tests)
- [ ] Workflow state transitions (12 tests)
- [ ] Cross-module integration (20 tests)
- [ ] Security integration (10 tests)
- [ ] Performance integration (10 tests)

**Subtotal Integration Tests**: ~110 tests

---

**Phase 2 Total**: ~2,462 tests (cumulative: ~2,838 tests)  
**Expected Coverage**: 55% → 75%

---

## 🟢 **Phase 3: Enhancement (Weeks 9-12)**

**Goal**: Add validation, performance, and advanced tests  
**Target**: Add ~400 tests, reach 85% coverage

### **G. Validation Layer Testing**

- [ ] Model validation attributes (20 tests)
- [ ] Custom validators (15 tests)
- [ ] Business rule validation (20 tests)
- [ ] Cross-field validation (15 tests)
- [ ] Async validation (10 tests)
- [ ] Conditional validation (10 tests)
- [ ] Error message validation (10 tests)

**Subtotal Validation Tests**: ~100 tests

---

### **H. Performance & Load Testing**

- [ ] Create `UNOPS.PAO.PerformanceTests` project
- [ ] Database query performance (15 tests)
- [ ] API endpoint response times (15 tests)
- [ ] Bulk operation performance (10 tests)
- [ ] Complex query performance (10 tests)
- [ ] Concurrent user simulation (10 tests)
- [ ] Memory usage tests (5 tests)
- [ ] CPU usage tests (5 tests)
- [ ] Database connection pool tests (5 tests)
- [ ] Cache performance tests (5 tests)

**Subtotal Performance Tests**: ~80 tests

---

### **I. Advanced Integration Scenarios**

- [ ] Multi-user collaboration scenarios (10 tests)
- [ ] Complex workflow scenarios (10 tests)
- [ ] Data migration scenarios (8 tests)
- [ ] Bulk import/export scenarios (10 tests)
- [ ] External integration scenarios (10 tests)
- [ ] Real-time notification scenarios (8 tests)
- [ ] AI processing scenarios (10 tests)
- [ ] Search and filtering scenarios (10 tests)

**Subtotal Advanced Integration**: ~76 tests

---

### **J. CI/CD & Test Infrastructure**

- [ ] Set up test coverage reporting (SonarQube/Coverlet)
- [ ] Configure CI/CD for all test projects
- [ ] Add pre-commit test hooks
- [ ] Set up test result dashboards
- [ ] Configure test parallelization
- [ ] Add performance test gates
- [ ] Set up nightly full test runs
- [ ] Configure test failure notifications

---

**Phase 3 Total**: ~256 tests (cumulative: ~3,094 tests)  
**Expected Coverage**: 75% → 85%

---

## 📊 **Progress Tracking**

### **Current Status:**

| Phase | Target Tests | Completed | Remaining | % Complete |
|-------|-------------|-----------|-----------|------------|
| **Phase 1** | 376 | 0 | 376 | 0% |
| **Phase 2** | 2,462 | 0 | 2,462 | 0% |
| **Phase 3** | 256 | 0 | 256 | 0% |
| **TOTAL** | **3,094** | **0** | **3,094** | **0%** |

### **Coverage Goals:**

| Milestone | Tests | Coverage % | Status |
|-----------|-------|-----------|---------|
| **Current** | 750 | 40% | ✅ Baseline |
| **Phase 1** | 1,126 | 55% | ⏳ In Progress |
| **Phase 2** | 3,212 | 75% | ⏳ Not Started |
| **Phase 3** | 3,468 | 85% | ⏳ Not Started |
| **Target** | 3,700+ | 90% | 🎯 Goal |

---

## 🎯 **Weekly Goals**

### **Week 1:**
- [ ] Create controller test project
- [ ] Complete PartnerController tests (44 tests)
- [ ] Start ContactController tests

**Target**: +44 tests

---

### **Week 2:**
- [ ] Complete ContactController tests (44 tests)
- [ ] Complete InteractionController tests (44 tests)
- [ ] Start OpportunityController tests

**Target**: +88 tests (cumulative: 132)

---

### **Week 3:**
- [ ] Complete OpportunityController tests (44 tests)
- [ ] Complete AuditLogManagerFullTests (50 tests)
- [ ] Start CommentManagerFullTests

**Target**: +94 tests (cumulative: 226)

---

### **Week 4:**
- [ ] Complete CommentManagerFullTests (50 tests)
- [ ] Complete EntityArtifactManagerFullTests (50 tests)
- [ ] Start AiRetrieverManagerFullTests

**Target**: +100 tests (cumulative: 326)

**Phase 1 Complete**: 376 tests, 55% coverage

---

### **Weeks 5-8: Phase 2**
- [ ] Complete all remaining controllers
- [ ] Upgrade manager tests
- [ ] Create UNOPS test project
- [ ] Expand integration tests

**Target**: +2,462 tests (cumulative: 2,838)

**Phase 2 Complete**: 75% coverage

---

### **Weeks 9-12: Phase 3**
- [ ] Add validation tests
- [ ] Add performance tests
- [ ] Add advanced integration tests
- [ ] Set up CI/CD

**Target**: +256 tests (cumulative: 3,094)

**Phase 3 Complete**: 85% coverage

---

## 📝 **Notes & Tips**

### **Test Writing Standards:**

✅ Follow existing patterns in `UNOPS.PAO.Business.Tests`  
✅ Use `ManagerTestBase` and `ControllerTestBase`  
✅ Always test `IsDeleted` soft delete filtering  
✅ Test authorization at every level  
✅ Use AAA pattern (Arrange, Act, Assert)  
✅ Write clear, descriptive test names  
✅ Add [Fact] for simple tests, [Theory] for parameterized tests  
✅ Mock external dependencies (DbContext, services)  
✅ Use test data factories for consistency

---

### **Test Naming Convention:**

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior

[Fact]
public async Task GetPartnerById_WithValidId_ReturnsPartner() { }

[Fact]
public async Task GetPartnerById_WithInvalidId_ThrowsKeyNotFoundException() { }

[Fact]
public async Task GetPartnerById_WithDeletedPartner_ThrowsKeyNotFoundException() { }

[Fact]
public async Task CreatePartner_WithValidData_CreatesPartner() { }

[Fact]
public async Task CreatePartner_WithInvalidData_ThrowsBusinessException() { }
```

---

### **Critical Test Areas:**

Every manager/controller test suite must include:

1. ✅ **CRUD Operations** - Create, Read, Update, Delete
2. ✅ **Soft Delete** - `IsDeleted` flag filtering
3. ✅ **Authorization** - Permission checks
4. ✅ **Validation** - Business rule enforcement
5. ✅ **Edge Cases** - Null handling, empty collections
6. ✅ **Error Handling** - Exception scenarios
7. ✅ **Concurrency** - Thread safety (where applicable)

---

## 🚀 **Getting Started**

### **Step 1: Review Analysis Documents**
- [ ] Read `UNIT_TEST_COVERAGE_SUMMARY.md`
- [ ] Read `UNIT_TEST_COVERAGE_ANALYSIS.md`
- [ ] Review existing test patterns in `UNOPS.PAO.Business.Tests`

### **Step 2: Set Up Environment**
- [ ] Ensure test database is configured
- [ ] Install required test tools (xUnit, Moq, FluentAssertions)
- [ ] Set up IDE test runner

### **Step 3: Start with Phase 1**
- [ ] Create controller test project
- [ ] Implement PartnerController baseline tests
- [ ] Review and refine test patterns
- [ ] Document lessons learned

### **Step 4: Execute Systematically**
- [ ] Follow weekly goals
- [ ] Track progress in this checklist
- [ ] Review and adjust as needed
- [ ] Celebrate milestones! 🎉

---

**Last Updated**: January 30, 2026  
**Owner**: QA Team & Development Team
