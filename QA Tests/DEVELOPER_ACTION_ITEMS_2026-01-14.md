# Developer Action Items - January 14, 2026

## 🚨 **DEFECTS REQUIRING DEVELOPER ATTENTION**

Based on comprehensive JIRA defect analysis and test execution results, here are the issues that need developer fixes:

---

## **CATEGORY 1: PRODUCTION BUGS (34 JIRA Defects)**

### **Status Breakdown:**
- **To Do**: 18 bugs (53%) - Not yet started
- **Ready for QA**: 12 bugs (35%) - Fixes implemented, awaiting testing
- **Ready for Dev**: 3 bugs (9%) - Ready to be picked up
- **Ready for UAT**: 1 bug (3%) - Awaiting user acceptance testing

---

## 🔴 **CRITICAL ISSUES (High Priority)**

### **1. AI Suggestion Issues** (11 bugs - 32% of defects)

#### **PNO-929: Wrong AI suggestions in Team section** 🔴 **HIGH PRIORITY**
**Problem**: AI suggests stakeholders/roles that are already assigned
**Impact**: Confuses users, reduces AI trust
**Root Cause**: AI doesn't validate current state before making suggestions
**Fix Required**: Add context validation before generating suggestions
```csharp
// NEEDS FIX: AI should check existing team before suggesting
public async Task<AIInsights> GetTeamInsights(int opportunityId)
{
    var opportunity = await GetWithTeamAsync(opportunityId);
    var suggestions = await aiService.GenerateSuggestions(opportunity);
    
    // ❌ MISSING: Filter out already-assigned stakeholders
    // ✅ ADD THIS:
    var filteredSuggestions = suggestions
        .Where(s => !IsAlreadyAssigned(s, opportunity.Stakeholders))
        .ToList();
    
    return new AIInsights { Suggestions = filteredSuggestions };
}
```

#### **PNO-900: AI Budget Information displayed incorrectly under WHEN section** 🔴 **HIGH PRIORITY**
**Problem**: Budget suggestions appear in wrong UI section
**Impact**: Data confusion, wrong information location
**Fix Required**: Correct AI suggestion categorization
**File**: Likely in AI suggestion routing/display logic

#### **PNO-860: Create Opportunity using AI assistant - Error** 🔴 **HIGH PRIORITY**
**Problem**: AI assistant fails during opportunity creation
**Impact**: Feature completely broken
**Fix Required**: Debug and fix AI-assisted creation flow

#### **PNO-773: AI Assistant unable to search Opportunities by Name or Description** 🔴 **HIGH PRIORITY**
**Problem**: Core search functionality broken
**Impact**: Users cannot find opportunities via AI
**Fix Required**: Fix AI search integration

---

### **2. Data Synchronization Issues** (6 bugs - 18% of defects)

#### **PNO-912: STATEMENT section - Info omitted or wrong** 🔴 **CRITICAL**
**Problems**:
- Opportunity Manager not appearing in statement
- Target signing date off by 1 day (Dec 12 → Dec 11)
- Delivery date off by 1 day (May 15 → May 14)

**Root Cause**: Timezone handling or date formatting issues
**Fix Required**: Ensure date consistency across all views
```csharp
// NEEDS FIX: Date formatting/timezone issue
public async Task<StatementDto> GenerateStatement(int opportunityId)
{
    var opportunity = await GetAsync(opportunityId);
    
    // ❌ LIKELY ISSUE: Date being converted with timezone offset
    // ✅ ENSURE: Use .Date property or UTC consistently
    return new StatementDto
    {
        TargetSigningDate = opportunity.TargetSigningDate.Date, // ✅ Use .Date
        DeliveryDate = opportunity.DeliveryDate.Date, // ✅ Use .Date
        OpportunityManager = opportunity.Manager?.Name ?? "Not Assigned"
    };
}
```

#### **PNO-933: Mass import of Contacts - Org unit mapping missing** 🔴 **HIGH PRIORITY**
**Problem**: Org unit not mapped during import
**Impact**: Data integrity issues
**Fix Required**: Add org unit mapping to import logic

#### **PNO-763: Missing Partners** 🔴 **HIGH PRIORITY**
**Problem**: Partners not appearing in system
**Impact**: Data completeness issues
**Fix Required**: Debug partner loading/filtering logic

---

### **3. Team/User Management Issues** (9 bugs - 26% of defects)

#### **PNO-931: OiCs, HoSS, HoPs not listed as internal stakeholders** 🔴 **HIGH PRIORITY**
**Problem**: Expected default team members not appearing
**Impact**: Users must manually add default team members
**Fix Required**: Auto-populate default stakeholders based on org unit
```csharp
// NEEDS FIX: Add automatic team assignment
public async Task<Opportunity> CreateAsync(OpportunityRequest request)
{
    var opportunity = mapper.Map<Opportunity>(request);
    await repository.AddAsync(opportunity);
    
    // ✅ ADD THIS: Auto-assign default team members
    await AssignDefaultTeamMembers(opportunity);
    
    return opportunity;
}

private async Task AssignDefaultTeamMembers(Opportunity opportunity)
{
    var orgUnit = await orgUnitService.GetAsync(opportunity.OrgUnitId);
    
    // Add OiC, HoSS, HoP from org unit hierarchy
    if (orgUnit.OfficerInCharge != null)
        opportunity.AddStakeholder(orgUnit.OfficerInCharge, StakeholderRole.OiC);
    
    if (orgUnit.HeadOfSupportServices != null)
        opportunity.AddStakeholder(orgUnit.HeadOfSupportServices, StakeholderRole.HoSS);
        
    if (orgUnit.HeadOfPractice != null)
        opportunity.AddStakeholder(orgUnit.HeadOfPractice, StakeholderRole.HoP);
}
```

#### **PNO-934: Wrong Opportunity Manager when creating from Concept note** 🔴 **HIGH PRIORITY**
**Problem**: System assigns wrong user instead of creator
**Impact**: Incorrect ownership assignment
**Fix Required**: Ensure creator is always set to current user
```csharp
// NEEDS FIX: Creator assignment
public async Task<Opportunity> CreateFromDocumentAsync(IFormFile file)
{
    var extractedData = await aiService.ExtractFromDocument(file);
    var opportunity = mapper.Map<Opportunity>(extractedData);
    
    // ✅ MUST SET: Current user as creator/manager
    opportunity.CreatedBy = currentUserId;
    opportunity.OpportunityManagerId = currentUserId; // ✅ Always current user
    
    await repository.AddAsync(opportunity);
    return opportunity;
}
```

#### **PNO-960: User with 'ENGREVADMIN' role unable to add/edit Programmes** 🔴 **HIGH PRIORITY**
**Problem**: Permission/authorization issue
**Impact**: Users cannot perform allowed actions
**Fix Required**: Add Programme CRUD permissions for ENGREVADMIN role
**File**: Permission configuration / authorization policies

---

### **4. UI/Dialog State Management** (4 bugs)

#### **PNO-964: Search boxes retain previous values when reopening dialogs** 🟠 **HIGH**
**Problem**: State not cleared between dialog opens, text overlaps icons
**Impact**: Confusing UX, incorrect pre-filled values
**Fix Required**: Reset form/search state on dialog open
```typescript
// NEEDS FIX: Angular component (Frontend)
openDialog(): void {
  // ✅ ADD THIS: Clear search state
  this.searchControl.setValue('');
  this.selectedFilters = [];
  
  this.dialogVisible = true;
}

closeDialog(): void {
  this.dialogVisible = false;
  
  // ✅ ADD THIS: Clear state on close too
  this.searchControl.setValue('');
  this.selectedFilters = [];
}
```

#### **PNO-935: WHERE section - Search by region confusion** 🟠 **MEDIUM**
**Problem**: Inconsistent region/continent naming
**Impact**: Search confusion
**Fix Required**: Standardize region naming

---

### **5. Performance Issues** (2 bugs)

#### **PNO-924: Persistent Server 'Error 429 - Too Many Requests'** 🔴 **HIGH PRIORITY**
**Problem**: Rate limiting triggered during normal operations
**Impact**: System unusable at times
**Fix Required**: 
1. Review and increase rate limits
2. Add request throttling on frontend
3. Implement retry logic with exponential backoff
```csharp
// NEEDS FIX: Rate limiting configuration
public void ConfigureServices(IServiceCollection services)
{
    services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? "anonymous",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100, // ✅ INCREASE: Was too low
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                });
        });
    });
}
```

#### **PNO-925: Loading stuck at 90%** 🟠 **MEDIUM**
**Problem**: Loading indicator doesn't complete
**Impact**: Poor UX, appears broken
**Fix Required**: Fix loading state management

---

### **6. UI Form Issues** (3 bugs)

#### **PNO-913: WHEN section - deadline notes label moves incorrectly** 🟡 **MEDIUM**
**Problem**: Floating label behavior issue
**Impact**: Visual glitch
**Fix Required**: Fix floating label CSS/animation

#### **PNO-963: New mandatory field blocks Adjustment submission** 🟠 **HIGH**
**Problem**: Required field validation blocking workflow
**Impact**: Users cannot submit valid adjustments
**Fix Required**: Review validation logic, make field optional or pre-populate

#### **PNO-148: Logo not displaying correctly** 🟢 **LOW**
**Problem**: Image rendering issues
**Impact**: Visual/branding issue
**Fix Required**: Fix image loading/rendering

---

## ⚠️ **TEST INFRASTRUCTURE ISSUES (6 failing tests)**

### **IntegrationTests Failures** (0.2% of tests)

All 6 failing tests are in `UNOPSPartnerManagerTests.cs`:

1. `GetPartnersWithSpecificationAsync_WithoutOrgUnitId_ReturnsAllPermittedPartners`
2. `TestDataPersistence_VerifyPartnersAreSavedCorrectly`
3. `GetPartnersWithSpecificationAsync_WhenHierarchyServiceNotAvailable_LogsWarningAndSkipsOrgUnitFilter`
4. `GetPartnersWithSpecificationAsync_WithPagination_ReturnsCorrectPage`
5. `GetPartnersWithSpecificationAsync_WithOrgUnitId_FiltersPartnersByOrgUnitHierarchy`
6. `TestSimpleGetPartnersWithSpecification_ReturnsData`

**Impact**: Low (tests only, not production code)
**Root Cause**: Likely test data setup or mock service configuration
**Fix Required**: Review test data seeding and UNOPSPartnerManager test setup

---

## 📋 **MISSING TEST COVERAGE (72 tests needed)**

Based on JIRA analysis, **72 new tests are recommended** to prevent future defects:

### **Critical Gap 1: Cross-Section Data Consistency** (15 tests) 🔴
**Problem**: Data entered in one section doesn't match another section
**Tests Needed**: Verify date consistency, field synchronization across views
**Priority**: CRITICAL

### **Critical Gap 2: AI Context Awareness** (12 tests) 🔴
**Problem**: AI doesn't validate state before suggesting
**Tests Needed**: Verify AI checks existing data before suggesting changes
**Priority**: CRITICAL

### **Critical Gap 3: Dialog State Management** (8 tests) 🔴
**Problem**: Forms/searches retain state between opens
**Tests Needed**: Verify dialogs reset properly
**Priority**: HIGH

### **Critical Gap 4: Document Upload Creator Assignment** (10 tests) 🔴
**Problem**: Wrong user assigned when creating from documents
**Tests Needed**: Verify current user is always the creator
**Priority**: HIGH

### **Gap 5: Role-Based Permissions** (8 tests) 🟠
**Problem**: Some role/permission combinations not tested
**Tests Needed**: Comprehensive permission matrix tests
**Priority**: HIGH

### **Gap 6: Default Team Assignment** (6 tests) 🟠
**Problem**: Default stakeholders not auto-assigned
**Tests Needed**: Verify OiC/HoSS/HoP auto-population
**Priority**: HIGH

### **Other Gaps**: (13 tests) 🟡
- Rate limiting tests (6)
- Floating label behavior (4)
- Image loading tests (3)

---

## 📊 **SUMMARY & PRIORITIES**

### **Developer Action Required:**

| Priority | Issues | Description | Impact |
|----------|--------|-------------|---------|
| 🔴 **CRITICAL** | 13 bugs | AI, data sync, team assignment | Production broken |
| 🟠 **HIGH** | 15 bugs | Permissions, UI state, search | Major UX issues |
| 🟡 **MEDIUM** | 4 bugs | UI polish, labels | Minor issues |
| 🟢 **LOW** | 2 bugs | Visual issues | Low impact |

### **Test Action Required:**

| Priority | Tests Needed | Category | Prevents |
|----------|--------------|----------|----------|
| 🔴 **CRITICAL** | 37 tests | Data sync, AI, uploads | 20+ bugs/release |
| 🟠 **HIGH** | 22 tests | State, permissions, teams | 10+ bugs/release |
| 🟡 **MEDIUM** | 13 tests | Performance, UI | 5+ bugs/release |

---

## 🎯 **RECOMMENDED ACTION PLAN**

### **Week 1: Critical Bugs**
1. ✅ Fix PNO-912 (Date sync issues)
2. ✅ Fix PNO-929 (AI wrong suggestions)
3. ✅ Fix PNO-931 (Missing default team members)
4. ✅ Fix PNO-934 (Wrong creator from documents)
5. ✅ Fix PNO-924 (Rate limiting 429 errors)

### **Week 2: High Priority Bugs + Critical Tests**
1. ✅ Fix PNO-960 (ENGREVADMIN permissions)
2. ✅ Fix PNO-964 (Dialog state retention)
3. ✅ Fix PNO-933 (Import org unit mapping)
4. ✅ Add 15 cross-section data consistency tests
5. ✅ Add 12 AI context awareness tests

### **Week 3: Medium Priority + Remaining Tests**
1. ✅ Fix remaining medium/low priority bugs
2. ✅ Add remaining 45 tests
3. ✅ Fix 6 failing IntegrationTests
4. ✅ Code review and testing

---

## 📈 **EXPECTED OUTCOMES**

### **After Fixes:**
- ✅ Zero critical production bugs
- ✅ 99.8% → 100% test pass rate
- ✅ +72 new tests (defect prevention)
- ✅ Estimated 20-25 bugs prevented per release
- ✅ Improved AI reliability
- ✅ Better data consistency
- ✅ Enhanced user experience

### **Quality Metrics:**
- **Current**: 3,593 tests, 99.8% pass rate, 34 production bugs
- **Target**: 3,665 tests, 100% pass rate, 0 critical bugs

---

## 📞 **NEXT STEPS**

1. **Developers**: Review and prioritize JIRA bugs (especially 13 critical issues)
2. **QA Team**: Create 72 missing test cases (see JIRA_DEFECT_ANALYSIS document)
3. **Dev Lead**: Assign bugs to team members
4. **QA Lead**: Set up test creation sprint
5. **Product Owner**: Review and approve priorities

---

## 📄 **RELATED DOCUMENTS**

- `QA Tests/Test Execution Results/JIRA_DEFECT_ANALYSIS_AND_TEST_GAPS_2026-01-13.md` - Full defect analysis
- `QA Tests/COMPREHENSIVE_TEST_EXECUTION_REPORT_2026-01-14.md` - Current test status
- `QA Tests/TEST_DASHBOARD_2026-01-13.md` - Test dashboard

---

**Status**: ✅ **READY FOR ACTION**  
**Priority**: 🔴 **HIGH - 13 Critical Bugs Need Immediate Attention**  
**Estimated Effort**: 3 weeks for all fixes + tests  

*This document provides a comprehensive action plan for developers based on production defect analysis and test execution results.*
