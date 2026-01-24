# PR #671 - Interactive Verification Session
**Date**: January 23, 2026  
**Status**: 🟡 IN PROGRESS

---

## 📋 **Verification Checklist**

Follow these steps in order. Mark each step as you complete it.

---

## **STEP 1: Database Migration Verification** ⏱️ 2 minutes

### **Option A: Using pgAdmin or Database GUI**

1. Open your PostgreSQL database client (pgAdmin, DBeaver, etc.)
2. Connect to your Opportunity+ database
3. Run these queries:

#### **Query 1: Check Migration History**
```sql
-- Verify migration was applied
SELECT 
    "MigrationId",
    "ProductVersion"
FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20260122185435_SetDefaultStageForOpportunity'
ORDER BY "MigrationId" DESC;
```

**✅ Expected Result**: 1 row returned with MigrationId = `20260122185435_SetDefaultStageForOpportunity`

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

---

#### **Query 2: Verify No NULL/Empty Stage Values**
```sql
-- This should return 0 (zero)
SELECT COUNT(*) as "ProblematicRecords"
FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';
```

**✅ Expected Result**: `ProblematicRecords = 0`

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

---

#### **Query 3: Stage Distribution**
```sql
-- See how opportunities are distributed across stages
SELECT 
    "Stage", 
    COUNT(*) as "Count"
FROM public."Opportunities"
GROUP BY "Stage"
ORDER BY "Count" DESC;
```

**✅ Expected Result**: 
- All records have valid Stage values
- Most should be "IDENTIFY & PROFILE" (default)
- May also see "GO" and "NO GO"

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

---

#### **Query 4: Sample Legacy Records**
```sql
-- Check some legacy opportunities (created before the fix)
SELECT 
    "Id",
    "Name",
    "Stage",
    "CreatedDate",
    "Status"
FROM public."Opportunities"
WHERE "CreatedDate" < '2026-01-22'
ORDER BY "Id" DESC
LIMIT 10;
```

**✅ Expected Result**: All have valid Stage values (likely "IDENTIFY & PROFILE")

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

---

### **Option B: Using Command Line (psql)**

```bash
# Connect to database (update with your connection details)
psql -h localhost -U postgres -d opportunityplus

# Then paste each query above
```

---

### **Option C: Using .NET Entity Framework**

```bash
# Check migrations list
cd "c:\Users\Leonardc\git\opportunityplus"
dotnet ef migrations list --project UNOPS.PAO.UNOPSDataAccess --startup-project UNOPS.PAO.Presentation

# Look for: 20260122185435_SetDefaultStageForOpportunity
```

---

### **Database Verification Results:**

**Migration Applied**: [ ] YES  [ ] NO  [ ] UNKNOWN

**NULL/Empty Stages Found**: [ ] 0 (PASS)  [ ] > 0 (FAIL)  [ ] NOT CHECKED

**All Stages Valid**: [ ] YES  [ ] NO  [ ] NOT CHECKED

---

## **STEP 2: Application Smoke Test** ⏱️ 5 minutes

### **Prerequisites**
- [ ] Application is running (dev/local/QA environment)
- [ ] You have login credentials
- [ ] Browser developer tools ready (F12)

---

### **Test 1: Opportunity List View** ⏱️ 1 min

**Steps:**
1. Log into the application
2. Navigate to: **Opportunities** menu or `/opportunities` URL
3. Wait for page to load

**Verification Points:**
- [ ] ✅ Page loads without errors (no error messages)
- [ ] ✅ Opportunity list displays
- [ ] ✅ Stage column visible
- [ ] ✅ Stage values show "IDENTIFY & PROFILE", "GO", or "NO GO"
- [ ] ✅ No console errors (check browser F12 console)
- [ ] ✅ No "WorkflowStage" errors in console

**Browser Console Check:**
```
Press F12 → Console Tab
Look for any red errors
```

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

**If FAIL - Record Details:**
- Error Message: ___________________________________
- Console Errors: __________________________________
- Screenshot Saved: [ ] YES  [ ] NO

---

### **Test 2: Opportunity Detail View** ⏱️ 1 min

**Steps:**
1. From the opportunity list, click on ANY opportunity
2. Wait for detail page to load
3. Check all sections

**Verification Points:**
- [ ] ✅ Detail page loads completely
- [ ] ✅ Opportunity name displays
- [ ] ✅ Stage field visible and populated
- [ ] ✅ "Overview" tab works
- [ ] ✅ "Budget" tab works (if exists)
- [ ] ✅ "Documents" tab works (if exists)
- [ ] ✅ Related entities display:
  - [ ] Responsible Org Unit
  - [ ] Partners (Funding/Client)
  - [ ] Stakeholders
  - [ ] Deliverables
  - [ ] Countries
  - [ ] SDGs
- [ ] ✅ No console errors

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

**If FAIL - Record Details:**
- Opportunity ID: ___________________________________
- Which section failed: ____________________________
- Error Message: ___________________________________

---

### **Test 3: Create New Opportunity** ⏱️ 2 min

**Steps:**
1. Click "New Opportunity" or "+ Opportunity" button
2. Fill in REQUIRED fields:
   - Name: "Test PR #671 Verification"
   - Description: "Testing opportunity screen fix"
   - Responsible Org Unit: (select any)
   - Initiative Type: (select any)
3. Click "Save" or "Create"
4. View the newly created opportunity

**Verification Points:**
- [ ] ✅ Create form opens
- [ ] ✅ Form fields are editable
- [ ] ✅ Save succeeds (no errors)
- [ ] ✅ Redirected to opportunity detail page
- [ ] ✅ New opportunity displays correctly
- [ ] ✅ Stage = "IDENTIFY & PROFILE" (default)
- [ ] ✅ Can navigate back to list
- [ ] ✅ New opportunity appears in list

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

**New Opportunity ID**: ___________________________________

**If FAIL - Record Details:**
- At which step: ___________________________________
- Error Message: ___________________________________

---

### **Test 4: Legacy Opportunity Check** ⏱️ 1 min

**Steps:**
1. Find an opportunity created BEFORE January 22, 2026
   - Look for older opportunities in the list
   - Check "Created Date" column
2. Open 2-3 legacy opportunities
3. Verify they load correctly

**Verification Points:**
- [ ] ✅ Legacy opportunities identified
- [ ] ✅ Legacy opportunity #1 loads: ID ___________
- [ ] ✅ Legacy opportunity #2 loads: ID ___________
- [ ] ✅ Legacy opportunity #3 loads: ID ___________
- [ ] ✅ All have Stage = "IDENTIFY & PROFILE"
- [ ] ✅ All related data displays correctly
- [ ] ✅ No errors when loading

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN

**If FAIL - Record Details:**
- Legacy Opportunity ID: ____________________________
- Issue: ___________________________________________

---

### **Test 5: Search/Filter** ⏱️ 1 min (Optional)

**Steps:**
1. Use search or filter functionality
2. Try filtering by Stage (if available)
3. Try text search

**Verification Points:**
- [ ] ✅ Search/filter works
- [ ] ✅ Results load correctly
- [ ] ✅ Stage values display in results
- [ ] ✅ No errors

**Status**: [ ] PASS  [ ] FAIL  [ ] NOT RUN  [ ] SKIPPED

---

## **STEP 3: Results Summary**

### **Database Verification**
- Migration Applied: [ ] ✅ YES  [ ] ❌ NO
- No NULL Stages: [ ] ✅ YES  [ ] ❌ NO
- Database Status: [ ] ✅ PASS  [ ] ❌ FAIL

### **Application Testing**
- Opportunity List: [ ] ✅ PASS  [ ] ❌ FAIL
- Opportunity Detail: [ ] ✅ PASS  [ ] ❌ FAIL
- Create Opportunity: [ ] ✅ PASS  [ ] ❌ FAIL
- Legacy Data: [ ] ✅ PASS  [ ] ❌ FAIL
- Search/Filter: [ ] ✅ PASS  [ ] ❌ FAIL  [ ] SKIPPED

### **Overall Assessment**

**Total Tests**: _____ / _____ Passed

**Final Recommendation:**
- [ ] ✅ **APPROVED FOR PRODUCTION** (All critical tests passed)
- [ ] ⚠️ **APPROVED WITH NOTES** (Minor issues, not blocking)
- [ ] ❌ **REJECTED** (Critical failures, needs fix)

---

## **STEP 4: Issue Reporting** (If tests failed)

**If any test FAILED, provide these details:**

### **Environment Information**
- Environment: [ ] Local Dev  [ ] DEV  [ ] QA  [ ] Other: ___________
- Database: PostgreSQL version: ___________
- Browser: ___________
- Application Version/Branch: ___________

### **Failure Details**
```
Test Name: _______________________________________
Step Failed: ______________________________________
Error Message: ____________________________________
____________________________________________
____________________________________________

Stack Trace (if available):
____________________________________________
____________________________________________
____________________________________________

Console Errors:
____________________________________________
____________________________________________
```

### **Screenshots**
- [ ] Error message screenshot attached
- [ ] Browser console screenshot attached
- [ ] Network tab screenshot attached (if API error)

### **Database Query Results** (if database issue)
```sql
-- Paste problematic query results here
```

---

## **STEP 5: Next Actions**

### **If All Tests PASS:**
- [x] Mark PR as verified
- [ ] Notify team: "PR #671 verified and ready for deployment"
- [ ] Schedule production deployment
- [ ] Monitor production after deployment

### **If Tests FAIL:**
- [ ] Create bug report with details above
- [ ] Notify development team immediately
- [ ] Block production deployment
- [ ] Tag PR #671 for review

---

## **Quick Reference: SQL Verification Script**

Copy/paste this entire script into your database client:

```sql
-- ============================================
-- PR #671 Database Verification Script
-- ============================================

-- Test 1: Migration Applied
SELECT 'Test 1: Migration Applied' as "Test";
SELECT 
    CASE 
        WHEN COUNT(*) = 1 THEN '✅ PASS: Migration applied'
        ELSE '❌ FAIL: Migration not found'
    END as "Result"
FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20260122185435_SetDefaultStageForOpportunity';

-- Test 2: No NULL/Empty Stages
SELECT 'Test 2: NULL/Empty Stage Check' as "Test";
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✅ PASS: No NULL/empty stages'
        ELSE '❌ FAIL: ' || COUNT(*) || ' records have NULL/empty Stage'
    END as "Result"
FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';

-- Test 3: Stage Distribution
SELECT 'Test 3: Stage Distribution' as "Test";
SELECT 
    "Stage", 
    COUNT(*) as "Count",
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER(), 2) as "Percentage"
FROM public."Opportunities"
GROUP BY "Stage"
ORDER BY "Count" DESC;

-- Test 4: Sample Records
SELECT 'Test 4: Sample Opportunities' as "Test";
SELECT 
    "Id",
    LEFT("Name", 50) as "Name",
    "Stage",
    "Status",
    "CreatedDate"
FROM public."Opportunities"
ORDER BY "Id" DESC
LIMIT 10;

-- Test 5: Legacy Records Check
SELECT 'Test 5: Legacy Records (Before Jan 22)' as "Test";
SELECT 
    COUNT(*) as "TotalLegacyRecords",
    COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) as "WithValidStage",
    COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) as "WithNULLStage"
FROM public."Opportunities"
WHERE "CreatedDate" < '2026-01-22';

-- Summary
SELECT 'VERIFICATION SUMMARY' as "Test";
SELECT 
    COUNT(*) as "TotalOpportunities",
    COUNT(CASE WHEN "Stage" IS NOT NULL AND "Stage" != '' THEN 1 END) as "ValidStage",
    COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) as "InvalidStage",
    CASE 
        WHEN COUNT(CASE WHEN "Stage" IS NULL OR "Stage" = '' THEN 1 END) = 0 
        THEN '✅ ALL TESTS PASS'
        ELSE '❌ TESTS FAILED'
    END as "OverallResult"
FROM public."Opportunities";
```

---

**Verification Performed By**: ___________________________________  
**Date**: ___________________________________  
**Time Started**: ___________________________________  
**Time Completed**: ___________________________________  
**Duration**: ___________ minutes

**Signature**: ___________________________________
