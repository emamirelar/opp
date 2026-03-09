# ✅ **DEVELOPER IMPLEMENTATION CHECKLIST**

**Generated:** January 13, 2026  
**Last Reviewed:** March 9, 2026  
**Test Suite:** Originally 605 Opportunity Tests — now significantly expanded (~10,040 total C# test methods + 1,629 Playwright tests)  
**Status:** Historical reference — most items completed. See [ACTION_ITEMS.md](ACTION_ITEMS.md) for current open items.

> **Note:** This checklist was created during the initial Opportunity feature build-out. Many items have been implemented. It is preserved for reference but the active tracking document is now [ACTION_ITEMS.md](ACTION_ITEMS.md).

---

## 🔴 **PHASE 1: FOUNDATION (BLOCKING - Do First!)**

### **Create Missing Namespaces:**
- [ ] `UNOPS.PAO.Models.Opportunity` - DTO models namespace
- [ ] `UNOPS.PAO.UNOPSBusiness.BusinessLogic` - Business logic namespace
- [ ] `UNOPS.PAO.UNOPSPresentation.Controllers` - Opportunity controllers

### **Fix/Verify Database Context:**
- [ ] Verify `UNOPSAppDbContext` exists or rename in tests
- [ ] Add Opportunity-related DbSets:
  - [ ] `DbSet<Opportunity> Opportunities`
  - [ ] `DbSet<OpportunityBudget> OpportunityBudgets`
  - [ ] `DbSet<OpportunitySchedule> OpportunitySchedules`
  - [ ] `DbSet<ResourcePlan> ResourcePlans`
  - [ ] `DbSet<RiskRegister> RiskRegisters`
  - [ ] `DbSet<DSTProfile> DSTProfiles`
  - [ ] `DbSet<GoNoGoDecision> GoNoGoDecisions`
  - [ ] `DbSet<PartnershipAgreement> PartnershipAgreements`
  - [ ] `DbSet<OpportunityPartner> OpportunityPartners`
  - [ ] `DbSet<OpportunityDocument> OpportunityDocuments`

---

## 🟠 **PHASE 2: SERVICE INTERFACES (CRITICAL)**

### **Create Core Service Interfaces:**
- [ ] `IAIService` - AI/ML functionality (narrative, extraction, OCR)
- [ ] `INotificationService` - User notifications
- [ ] `ICacheService` - Caching and search indexing
- [ ] `IDocumentStorageService` - Document upload/download
- [ ] `IPermissionService` - DOA and access validation
- [ ] `IExternalSystemService` - ERP, PM Tool, HR integration
- [ ] `IHttpContextAccessor` - HTTP context access (or use existing)
- [ ] `IConfiguration` - Configuration access (or use existing)
- [ ] `IAuthorizationService` - Authorization (or use existing)

---

## 🟡 **PHASE 3: MODEL/DTO CLASSES (HIGH PRIORITY)**

### **Opportunity Models (in UNOPS.PAO.Models.Opportunity):**
- [ ] `OpportunityModel` - Main DTO
- [ ] `OpportunityCreateRequest` - Create request
- [ ] `OpportunityUpdateRequest` - Update request
- [ ] `OpportunityFilterRequest` - List filtering

### **Budget Models:**
- [ ] `BudgetModel`
- [ ] `BudgetCreateRequest`
- [ ] `BudgetUpdateRequest`
- [ ] `SpendRateResponse`
- [ ] `CostCategoriesResponse`
- [ ] `FeeStructureUpdateRequest`
- [ ] `BudgetComparisonResponse`

### **Schedule Models:**
- [ ] `ScheduleModel`
- [ ] `PhaseModel`
- [ ] `MilestoneModel`
- [ ] `ScheduleUpdateRequest`
- [ ] `WBSResponse`, `WBSNode`
- [ ] `GanttChartResponse`, `GanttTask`
- [ ] `CriticalPathResponse`, `TaskModel`

### **Resource Models:**
- [ ] `ResourcePlanModel`
- [ ] `PhaseResources`
- [ ] `RoleRequirement`, `RoleAvailability`
- [ ] `ResourcePlanUpdateRequest`
- [ ] `PersonnelBudgetResponse`

### **DST Models:**
- [ ] `DSTProfileModel`
- [ ] `DSTParameterScores`
- [ ] `DSTRecommendationModel`
- [ ] `ComprehensiveDSTReport`
- [ ] `DSTProfileComparison`

### **Decision Models:**
- [ ] `GoNoGoDecisionModel`
- [ ] `DecisionPackageModel`
- [ ] `DecisionCreateRequest`

### **Risk Models:**
- [ ] `RiskRegisterModel`
- [ ] `RiskModel`
- [ ] `RiskMitigationPlan`

### **Agreement Models:**
- [ ] `PartnershipAgreementModel`
- [ ] `AgreementCreateRequest`
- [ ] `ExtractedTerms`

### **Index Models:**
- [ ] `GlobalIndexModel`
- [ ] `IndexTrendResponse`, `TrendDataPoint`

### **Additional Models:**
- [ ] All response/request models referenced in tests (50+ total)

---

## 🟢 **PHASE 4: MANAGERS (8 CLASSES)**

### **In UNOPS.PAO.UNOPSBusiness.Managers:**
- [ ] `OpportunityManager` - CRUD operations (tests: 10)
- [ ] `OpportunityBudgetManager` - Budget generation/management (tests: 20)
- [ ] `OpportunityScheduleManager` - Schedule/WBS/Gantt (tests: 18)
- [ ] `ResourcePlanManager` - Resource allocation (tests: 15)
- [ ] `RiskManager` - Risk assessment/mitigation (tests: 22)
- [ ] `GlobalIndicesManager` - Global indices (tests: 18)
- [ ] `DSTManager` - Decision Support Tool (tests: 25)
- [ ] `DecisionManager` - Go/No-Go decisions (tests: 20)

**Total Manager Tests:** 170 tests waiting

---

## 🔵 **PHASE 5: BUSINESS LOGIC (7 CLASSES)**

### **In UNOPS.PAO.UNOPSBusiness.BusinessLogic:**
- [ ] `OpportunityStatementLogic` - Statement generation (tests: 20)
- [ ] `DocumentExtractionLogic` - AI extraction (tests: 9)
- [ ] `DSTProfilerLogic` - DST profiling (tests: 13)
- [ ] `GoNoGoDecisionLogic` - Decision workflow (tests: 25)
- [ ] `AgreementLibraryLogic` - Agreement storage (tests: 20)
- [ ] `OpportunityWorkflowLogic` - Status workflows (tests: 10)
- [ ] `OpportunityValidationLogic` - Validation rules (tests: various)

**Total Business Logic Tests:** 150+ tests waiting

---

## 🟣 **PHASE 6: CONTROLLERS (8 CLASSES)**

### **In UNOPS.PAO.UNOPSPresentation.Controllers:**
- [ ] `OpportunityController` - Main CRUD API (tests: 12)
- [ ] `OpportunityBudgetController` - Budget API (tests: 8)
- [ ] `OpportunityScheduleController` - Schedule API (tests: 8)
- [ ] `ResourcePlanController` - Resource API (tests: 5)
- [ ] `GlobalIndicesController` - Indices API (tests: 5)
- [ ] `DSTController` - DST API (tests: 5)
- [ ] `DecisionController` - Decision API (tests: 4)
- [ ] `PartnershipAgreementController` - Agreement API (tests: 2)

**Total Controller Tests:** 60 tests waiting

---

## ⚪ **PHASE 7: SERVICES (3 CLASSES)**

### **In UNOPS.PAO.UNOPSBusiness.Services:**
- [ ] `OpportunityService` - Orchestration (tests: 12)
- [ ] `DSTAnalysisService` - Advanced DST (tests: 8)
- [ ] `AgreementService` - Agreement validation (tests: 10)

**Total Service Tests:** 30 tests waiting

---

## 📊 **PROGRESS TRACKING**

### **Check Your Progress:**

```bash
# Build and see what's left
dotnet build UNOPS.PAO.Business.Tests.csproj

# Run tests by priority
dotnet test --filter "Category=P0"  # Critical (230 tests)
dotnet test --filter "Category=P1"  # High (285 tests)
dotnet test --filter "Category=P2"  # Medium (90 tests)

# Run all Opportunity tests
dotnet test --filter "FullyQualifiedName~Opportunity"

# See pass rate
dotnet test --logger "console;verbosity=normal"
```

### **Milestone Tracking:**
- [ ] **Milestone 1:** Tests compile (0 errors)
- [ ] **Milestone 2:** P0 tests pass (230 tests)
- [ ] **Milestone 3:** P1 tests pass (285 tests)
- [ ] **Milestone 4:** ALL tests pass (605 tests) 🎯

---

## 🎯 **QUICK WINS**

### **Start Here (Easiest First):**

1. **Create Namespaces** (30 min)
   ```csharp
   // Create folders and add namespace declarations
   namespace UNOPS.PAO.Models.Opportunity { }
   namespace UNOPS.PAO.UNOPSBusiness.BusinessLogic { }
   ```

2. **Create Model Stubs** (2-3 hours)
   ```csharp
   public class OpportunityModel
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public decimal? EstimatedValue { get; set; }
       // Add properties as needed to satisfy tests
   }
   ```

3. **Create Manager Stubs** (4-6 hours)
   ```csharp
   public class OpportunityManager
   {
       private readonly UNOPSAppDbContext _context;
       
       public OpportunityManager(UNOPSAppDbContext context)
       {
           _context = context;
       }
       
       // Add methods as needed to satisfy tests
       public async Task<Opportunity> GetByIdAsync(int id)
       {
           return await _context.Opportunities.FindAsync(id);
       }
   }
   ```

**Result:** Tests compile! Now implement real logic to make them pass.

---

## 📚 **REFERENCE DOCUMENTS**

- **Complete Error Analysis:** `DEVELOPER_IMPLEMENTATION_REQUIRED_2026-01-13.md`
- **Build Output:** `COMPLETE_BUILD_OUTPUT_2026-01-13.txt`
- **Test Suite Status:** `TEST_SUITE_STATUS_AND_NEXT_STEPS.md`
- **Push Decision:** `PUSH_DECISION_QUICK_REFERENCE.md` (this file)

---

## 🏆 **BOTTOM LINE**

**Test Suite:** ✅ 10,040 C# test methods + 1,629 Playwright tests  
**Defect-Exposing Tests:** ~240 tests tagged with `[Trait("Defect", "DEF-XXX")]`  
**Test Data Infrastructure:** ✅ TestEntityBuilder fluent API, Bogus fake data, JSON fixtures  
**Implementation:** ⏳ ONGOING — many opportunity features implemented, remaining items tracked in ACTION_ITEMS.md  
**CI/CD:** ✅ 11-job pipeline operational

**Commands to verify:**
```bash
# Run smoke gate
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests" --filter "Category=Smoke"

# Run all non-defect tests
dotnet test --filter "Defect!~DEF"

# Run Playwright smoke
npx playwright test login.spec.ts home.spec.ts dashboard.spec.ts partners.spec.ts opportunities.spec.ts interactions.spec.ts
```

---

*Tests are specifications. This checklist is preserved as historical reference. See [ACTION_ITEMS.md](ACTION_ITEMS.md) for current tracking.*
