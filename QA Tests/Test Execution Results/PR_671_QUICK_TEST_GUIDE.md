# PR #671 - Quick Testing Guide (5-Minute Verification)

**PR**: Fix for opportunity screen not loading  
**Commit**: 887f9279  
**Date**: January 23, 2026

---

## 🚀 **Quick Start - 5 Minute Test Plan**

### **Test 1: Opportunity List (1 min)**
1. Navigate to `/opportunities` or Opportunities menu
2. ✅ Page loads without errors
3. ✅ Stage column shows values ("IDENTIFY & PROFILE", "GO", or "NO GO")

### **Test 2: Opportunity Detail (1 min)**
1. Click any opportunity from the list
2. ✅ Detail page loads
3. ✅ All tabs work (Overview, Budget, Documents, etc.)
4. ✅ Related data displays (Partners, Stakeholders, etc.)

### **Test 3: Create Opportunity (2 min)**
1. Click "New Opportunity"
2. Fill required fields and save
3. ✅ Saves successfully
4. ✅ New opportunity has Stage = "IDENTIFY & PROFILE"

### **Test 4: Database Check (1 min)**
```sql
-- Should return 0 rows (no NULL/empty Stage values)
SELECT COUNT(*) FROM public."Opportunities" 
WHERE "Stage" IS NULL OR "Stage" = '';
```

---

## ✅ **Pass Criteria**

All 4 tests above pass = **APPROVED FOR PRODUCTION**

---

## ❌ **If Tests Fail**

Contact development team immediately with:
1. Which test failed
2. Error message (screenshot)
3. Browser console errors
4. Opportunity ID (if applicable)

---

## 📝 **What Was Fixed**

- **Bug**: Opportunity screen not loading
- **Cause**: Code trying to load deleted `WorkflowStage` navigation property
- **Fix**: Removed invalid includes, added migration for legacy data
- **Files Changed**: 3 manager/service files + 1 database migration

---

## 🔗 **Full Documentation**

See: `PR_671_VERIFICATION_RESULTS_2026-01-23.md` for complete details
