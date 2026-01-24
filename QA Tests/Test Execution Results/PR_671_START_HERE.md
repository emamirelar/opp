# 🚀 PR #671 Verification - START HERE

**PR**: Fix for opportunity screen not loading  
**Commit**: 887f9279  
**Date**: January 23, 2026  
**Status**: 🟡 Ready for Manual Verification

---

## 📋 **Quick Overview**

This PR fixes a critical bug where the Opportunity screen was not loading. The fix:
- ✅ Removed invalid `WorkflowStage` navigation property references
- ✅ Added database migration to fix legacy data
- ✅ Code verification completed (all automated checks passed)

**Your Task**: Run the 5-minute smoke test + database verification

---

## 🎯 **Two Simple Steps**

### **STEP 1: Database Verification** ⏱️ 2 minutes

#### **Option A: Using PowerShell Helper Script (Easiest)**
```powershell
# From project root directory
.\verify-pr-671.ps1
```
This script will display all SQL queries with color-coding and instructions.

#### **Option B: Run SQL Script Directly**
1. Open your PostgreSQL client (pgAdmin, DBeaver, psql, etc.)
2. Connect to OpportunityPlus database
3. Open and run: `verify-pr-671.sql`
4. Review results - should see "✅ ALL TESTS PASS"

#### **Option C: Manual SQL Queries**
Copy/paste these queries one by one:

```sql
-- 1. Check migration applied (expect 1 row)
SELECT * FROM public."__EFMigrationsHistory" 
WHERE "MigrationId" = '20260122185435_SetDefaultStageForOpportunity';

-- 2. Check for NULL stages (expect 0 rows)
SELECT COUNT(*) FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';

-- 3. Stage distribution (all should have valid values)
SELECT "Stage", COUNT(*) as "Count"
FROM public."Opportunities"
GROUP BY "Stage";
```

**✅ Expected Results:**
- Migration exists in history
- Zero NULL/empty Stage values
- All opportunities have valid Stage ("IDENTIFY & PROFILE", "GO", or "NO GO")

---

### **STEP 2: Application Smoke Test** ⏱️ 5 minutes

1. **Start the application** (if not running):
   ```bash
   cd UNOPS.PAO.Presentation
   dotnet run
   ```
   Or press F5 in Visual Studio

2. **Open browser** and log in

3. **Run these 4 quick tests:**

   **Test 1 (1 min)**: Navigate to Opportunities page
   - ✅ Page loads
   - ✅ Stage column shows values

   **Test 2 (1 min)**: Click any opportunity
   - ✅ Detail page loads
   - ✅ All tabs work
   - ✅ Related data displays

   **Test 3 (2 min)**: Create new opportunity
   - ✅ Form opens
   - ✅ Saves successfully
   - ✅ Stage = "IDENTIFY & PROFILE"

   **Test 4 (1 min)**: Open a legacy opportunity (created before Jan 22)
   - ✅ Loads without errors
   - ✅ Has valid Stage value

---

## 📁 **Verification Documents**

| Document | Purpose | When to Use |
|----------|---------|-------------|
| **`PR_671_START_HERE.md`** ⭐ | This file - quick start guide | Read first |
| `PR_671_QUICK_TEST_GUIDE.md` | 5-minute test checklist | During testing |
| `PR_671_INTERACTIVE_VERIFICATION.md` | Detailed verification with checkboxes | For thorough documentation |
| `PR_671_VERIFICATION_RESULTS_2026-01-23.md` | Complete technical analysis | For technical details |
| `verify-pr-671.ps1` | PowerShell helper script | Windows users |
| `verify-pr-671.sql` | SQL verification script | Database clients |

---

## ✅ **Success Criteria**

**Database Tests:**
- ✅ Migration applied
- ✅ Zero NULL/empty Stage values

**Application Tests:**
- ✅ Opportunity list loads
- ✅ Opportunity detail loads
- ✅ Create new opportunity works
- ✅ Legacy opportunities load

**If ALL pass** → ✅ **APPROVED FOR PRODUCTION**

---

## ❌ **If Tests Fail**

1. **Don't panic** - document what failed
2. **Take screenshots** of any errors
3. **Check browser console** (F12) for errors
4. **Record details** in `PR_671_INTERACTIVE_VERIFICATION.md`
5. **Contact development team** with:
   - Which test failed
   - Error message
   - Screenshots
   - Opportunity ID (if applicable)

---

## 🔧 **Troubleshooting**

### **Issue: Database connection failed**
- Check PostgreSQL is running
- Verify connection string in `appsettings.json`
- Ensure you have database access permissions

### **Issue: Application won't start**
```bash
# Try building first
dotnet build

# Check for errors
dotnet restore
```

### **Issue: Migration not found**
```bash
# Check migrations list
dotnet ef migrations list --project UNOPS.PAO.UNOPSDataAccess --startup-project UNOPS.PAO.Presentation

# Apply migrations if needed
dotnet ef database update --project UNOPS.PAO.UNOPSDataAccess --startup-project UNOPS.PAO.Presentation
```

---

## 🎯 **Quick Decision Tree**

```
START
  ↓
Run database verification
  ↓
┌─────────────────┐
│ All DB tests    │ NO → Document failures
│ pass?           │      Contact dev team
└─────────────────┘      STOP
  ↓ YES
Run application smoke test
  ↓
┌─────────────────┐
│ All app tests   │ NO → Document failures
│ pass?           │      Contact dev team
└─────────────────┘      STOP
  ↓ YES
✅ APPROVED FOR PRODUCTION
Deploy to next environment
Monitor for issues
```

---

## ⏱️ **Time Estimates**

- Database verification: 2 minutes
- Application smoke test: 5 minutes
- Documentation: 2 minutes
- **Total: ~10 minutes**

---

## 📞 **Support**

**Questions or Issues?**
- See full technical details: `PR_671_VERIFICATION_RESULTS_2026-01-23.md`
- Check test defects: `DEFECTS_FOR_DEVELOPERS_UPDATED_2026-01-16.md`
- Review PR: https://github.com/UNOPS-ITG/opportunityplus/pull/671

---

## 🚦 **Ready to Start?**

### **Right Now - Choose Your Path:**

**Path A: Quick Verification (Recommended)**
1. Run: `.\verify-pr-671.ps1` (database)
2. Follow: `PR_671_QUICK_TEST_GUIDE.md` (application)
3. Takes: ~7 minutes

**Path B: Thorough Documentation**
1. Open: `PR_671_INTERACTIVE_VERIFICATION.md`
2. Follow all steps and check boxes
3. Takes: ~15 minutes

**Path C: Technical Deep Dive**
1. Read: `PR_671_VERIFICATION_RESULTS_2026-01-23.md`
2. Understand all code changes
3. Run verification
4. Takes: ~30 minutes

---

## ✨ **What This PR Fixed**

**Before**: Opportunity screen throwing errors and not loading  
**Root Cause**: Code trying to load deleted `WorkflowStage` navigation property  
**After**: Opportunity screen loads normally, all data displays correctly

**Files Changed**: 3 (removed invalid includes)  
**Migration Added**: 1 (fixes legacy data)  
**Risk Level**: 🟢 LOW (simple, focused fix)

---

**Ready?** Let's verify PR #671! 🚀

Start with **STEP 1** above or run `.\verify-pr-671.ps1`
