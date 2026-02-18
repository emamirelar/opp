# 🔧 **REMAINING TEST FIXES GUIDE**

**Date:** January 13, 2026  
**Status:** 42 of 138 errors fixed (30.4% complete)  
**Remaining:** 96 errors across 10+ files  
**Estimated Time:** 5-7 hours

---

## 📊 **REMAINING ERRORS BY CATEGORY**

### **1. OrganizationUnitType Enum (15+ errors)**
**Files Affected:**
- `DataIntegrityTests.cs`
- `OrganizationHierarchyLookupServiceTests.cs`
- `ValuesManagerTests.cs`
- `OrganizationHierarchyManagerFullTests.cs`

**Problem:**
```csharp
// These enum values DO NOT exist:
OrganizationUnitType.HeadQuarter  // ❌ Removed
OrganizationUnitType.Country      // ❌ Never existed

// Valid enum values are:
OrganizationUnitType.Office   // ✅ Use this
OrganizationUnitType.Region   // ✅ Use this
OrganizationUnitType.Hub      // ✅ Use this
OrganizationUnitType.OrgUnit  // ✅ Use this
```

**Fix Pattern:**
```csharp
// FIND & REPLACE:
OrganizationUnitType.HeadQuarter → OrganizationUnitType.Office
OrganizationUnitType.Country     → OrganizationUnitType.Region
```

**Locations:**
- `EdgeCases/DataIntegrityTests.cs:59`
- `Services/OrganizationHierarchyLookupServiceTests.cs:46, 70-73, 117, 140, 151`
- `Managers/ValuesManagerTests.cs:265`
- `Managers/OrganizationHierarchyManagerFullTests.cs:41, 44`

---

### **2. UserProfile Entity Changes (17+ errors)**
**Files Affected:**
- `UserDataManagerFullTests.cs`
- `ProfileManagerFullTests.cs`

**Problems:**

#### **A. `Name` Property is Read-Only**
```csharp
// ERROR: Name is computed from FirstName + LastName
profile.Name = "John Doe";  // ❌ Cannot set

// FIX: Don't set Name directly
profile.FirstName = "John";
profile.LastName = "Doe";
// Name is automatically computed
```

#### **B. `Email` Property Renamed**
```csharp
// OLD property name:
profile.Email = "test@example.com";  // ❌ Doesn't exist

// NEW property name:
profile.UserEmail = "test@example.com";  // ✅ Correct
```

#### **C. Missing `UserProfiles` DbSet**
```csharp
// ERROR: AppDbContext doesn't have UserProfiles DbSet
_context.UserProfiles.Add(profile);  // ❌ Doesn't exist

// POSSIBLE FIXES:
// Option 1: DbSet was renamed
_context.PAOUserProfiles.Add(profile);  // Check if this exists

// Option 2: DbSet needs to be added to AppDbContext
// Add to AppDbContext.cs:
public DbSet<UserProfile> UserProfiles { get; set; }

// Option 3: Tests should use a different context
// Check if should use UNOPSAppDbContext instead
```

**Locations:**
- `Managers/UserDataManagerFullTests.cs:53-54, 60, 120, 128, 132`
- `Managers/ProfileManagerFullTests.cs:51-52, 58, 67, 75, 83, 107, 115, 121`

---

### **3. Partner Entity Changes (15+ errors)**
**Files Affected:**
- `PartnerTreeManagerFullTests.cs`

**Problem:**
```csharp
// Property REMOVED from Partner entity
partner.ParentPartnerId = 1;  // ❌ No longer exists

// FIX: Remove all references to ParentPartnerId
// Partner hierarchy is now handled differently
// (Likely through PartnerGroup navigation property)
```

**Action Required:**
1. Check how partner hierarchy is now implemented
2. Remove all `ParentPartnerId` references from tests
3. Update tests to use new hierarchy approach (if applicable)
4. OR mark these tests as obsolete if functionality removed

**Locations:**
- `Managers/PartnerTreeManagerFullTests.cs:41-46, 58, 68, 77, 85, 115, 119, 126, 129, 155`

---

### **4. Country Entity - Missing Required Property (6 errors)**
**File Affected:**
- `CountryServiceTests.cs`

**Problem:**
```csharp
// ERROR: Iso2Code is REQUIRED but not initialized
new Country {
    Name = "Test Country",
    Code = "TC"  // ❌ Wrong property name
};

// FIX: Always initialize Iso2Code (required)
new Country {
    Name = "Test Country",
    Iso2Code = "TC"  // ✅ Correct and required
};
```

**Locations:**
- `Services/CountryServiceTests.cs:510, 513, 533, 536, 539, 542`

---

### **5. One Remaining Country Reference (1 error)**
**File Affected:**
- `ValuesManagerTests.cs`

**Problem:**
```csharp
// Still using old Code property
.FirstOrDefaultAsync(c => c.Code == "XX" && !c.IsDeleted);

// FIX:
.FirstOrDefaultAsync(c => c.Iso2Code == "XX" && !c.IsDeleted);
```

**Location:**
- `Managers/ValuesManagerTests.cs:143`

---

## 🛠️ **DETAILED FIX INSTRUCTIONS**

### **File 1: OrganizationHierarchyLookupServiceTests.cs**

**Errors to Fix:** 9 errors

```csharp
// Line 46 - FIND:
Type = OrganizationUnitType.HeadQuarter

// REPLACE WITH:
Type = OrganizationUnitType.Office

// Lines 70-73 - FIND (4 occurrences):
Type = OrganizationUnitType.Country

// REPLACE WITH:
Type = OrganizationUnitType.Region

// Line 117 - FIND:
Type = OrganizationUnitType.Country

// REPLACE WITH:
Type = OrganizationUnitType.Region

// Line 140 - FIND:
Type = OrganizationUnitType.HeadQuarter

// REPLACE WITH:
Type = OrganizationUnitType.Office

// Line 151 - FIND:
Type = OrganizationUnitType.HeadQuarter

// REPLACE WITH:
Type = OrganizationUnitType.Office
```

---

### **File 2: UserDataManagerFullTests.cs**

**Errors to Fix:** 5 errors

```csharp
// Lines 53-54 - FIND:
Name = "Test User",
Email = "test@example.com"

// REPLACE WITH:
FirstName = "Test",
LastName = "User",
UserEmail = "test@example.com"

// Lines 60, 120, 128, 132 - FIND:
_context.UserProfiles

// OPTION 1 - If DbSet exists with different name:
_context.PAOUserProfiles  // or whatever the actual name is

// OPTION 2 - If DbSet missing, add to AppDbContext:
// In AppDbContext.cs, add:
public DbSet<UserProfile> UserProfiles { get; set; }
```

---

### **File 3: ProfileManagerFullTests.cs**

**Errors to Fix:** 10 errors

```csharp
// Lines 51-52 - FIND:
Name = "Test User",
Email = "test@example.com"

// REPLACE WITH:
FirstName = "Test",
LastName = "User",
UserEmail = "test@example.com"

// Lines 58, 67, 75, 83, 107, 115, 121 - FIND:
_context.UserProfiles

// REPLACE WITH:
_context.PAOUserProfiles  // (or add DbSet to AppDbContext)
```

---

### **File 4: PartnerTreeManagerFullTests.cs**

**Errors to Fix:** 15 errors

**Option A - Remove ParentPartnerId (Recommended):**
```csharp
// Lines 41-46 - FIND:
ParentPartnerId = someValue

// DELETE these lines entirely
// Partner hierarchy is no longer managed this way
```

**Option B - Update to New Hierarchy Approach:**
```csharp
// Research how partner hierarchy is now implemented
// Likely through PartnerGroupId navigation property
// Update tests accordingly
```

**Option C - Mark Tests as Obsolete:**
```csharp
[Fact]
[Trait("Status", "Obsolete")]  // Mark as obsolete
public async Task TestPartnerHierarchy()
{
    // Test partner parent-child relationships
    // This functionality may have been removed
}
```

---

### **File 5: CountryServiceTests.cs**

**Errors to Fix:** 6 errors

```csharp
// Lines 510, 533, 539 - ADD required Iso2Code:
new Country
{
    Name = "Test Country",
    Iso2Code = "TC"  // ADD THIS LINE - Required property
}

// Lines 513, 536, 542 - RENAME property:
Code = "TC"

// CHANGE TO:
Iso2Code = "TC"
```

---

### **File 6: DataIntegrityTests.cs**

**Errors to Fix:** 1 error

```csharp
// Line 59 - FIND:
Type = OrganizationUnitType.Country

// REPLACE WITH:
Type = OrganizationUnitType.Region
```

---

### **File 7: OrganizationHierarchyManagerFullTests.cs**

**Errors to Fix:** 2 errors

```csharp
// Line 41 - FIND:
Type = OrganizationUnitType.HeadQuarter

// REPLACE WITH:
Type = OrganizationUnitType.Office

// Line 44 - FIND:
Type = OrganizationUnitType.Country

// REPLACE WITH:
Type = OrganizationUnitType.Region
```

---

### **File 8: ValuesManagerTests.cs**

**Errors to Fix:** 2 errors (1 already partially fixed)

```csharp
// Line 143 - FIND:
.FirstOrDefaultAsync(c => c.Code == "XX" && !c.IsDeleted)

// REPLACE WITH:
.FirstOrDefaultAsync(c => c.Iso2Code == "XX" && !c.IsDeleted)

// Line 265 - FIND:
Type = OrganizationUnitType.HeadQuarter

// REPLACE WITH:
Type = OrganizationUnitType.Office
```

---

## ⚡ **QUICK FIX SCRIPT (PowerShell)**

```powershell
# Run this in QA Tests/C# Tests/UNOPS.PAO.Business.Tests directory

# Fix OrganizationUnitType.HeadQuarter
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_.FullName) -replace 'OrganizationUnitType\.HeadQuarter', 'OrganizationUnitType.Office' | 
    Set-Content $_.FullName
}

# Fix OrganizationUnitType.Country
Get-ChildItem -Recurse -Include *.cs | ForEach-Object {
    (Get-Content $_.FullName) -replace 'OrganizationUnitType\.Country', 'OrganizationUnitType.Region' | 
    Set-Content $_.FullName
}

# Fix Country.Code (be careful - only in test context)
# This one needs manual review to avoid false positives
```

---

## 📊 **PRIORITY ORDER**

### **High Priority (Quick Wins):**
1. ✅ OrganizationUnitType fixes (15 errors) - **30 minutes**
   - Simple find/replace across multiple files
2. ✅ Country.Code → Iso2Code (7 errors) - **15 minutes**
   - Simple property rename
3. ✅ UserProfile.Email → UserEmail (4 errors) - **10 minutes**
   - Simple property rename

### **Medium Priority (Requires Research):**
4. ⚠️ AppDbContext.UserProfiles (13 errors) - **1-2 hours**
   - Need to check if DbSet exists with different name
   - Or add DbSet to AppDbContext
5. ⚠️ UserProfile.Name read-only (4 errors) - **30 minutes**
   - Change to set FirstName/LastName instead

### **Low Priority (Requires Design Decision):**
6. 🤔 Partner.ParentPartnerId (15 errors) - **2-3 hours**
   - Need to understand new partner hierarchy design
   - May require test redesign or marking as obsolete

---

## 📈 **ESTIMATED COMPLETION TIME**

| Task | Errors | Time | Difficulty |
|------|--------|------|------------|
| OrganizationUnitType enum | 15 | 30 min | Easy |
| Country property names | 7 | 15 min | Easy |
| UserProfile.UserEmail | 4 | 10 min | Easy |
| UserProfile.Name read-only | 4 | 30 min | Medium |
| UserProfiles DbSet | 13 | 1-2 hrs | Medium |
| Partner.ParentPartnerId | 15 | 2-3 hrs | Hard |
| **TOTAL** | **96** | **5-7 hrs** | - |

---

## ✅ **NEXT SESSION ACTION PLAN**

1. **Start with Quick Wins (1 hour):**
   - Fix OrganizationUnitType (15 errors)
   - Fix Country properties (7 errors)
   - Fix UserEmail (4 errors)
   - **Total: 26 errors fixed**

2. **Medium Priority (2 hours):**
   - Fix UserProfile.Name (4 errors)
   - Research and fix UserProfiles DbSet (13 errors)
   - **Total: 17 more errors fixed**

3. **Partner Hierarchy Decision (2-3 hours):**
   - Consult with dev team on Partner hierarchy
   - Either update tests or mark as obsolete
   - **Total: 15 more errors fixed**

4. **Final Verification:**
   - Build and verify 0 errors
   - Run tests
   - Document any runtime issues

---

## 🎯 **RECOMMENDATION**

**Option 1: Continue Yourself (Recommended)**
- Use this guide to fix remaining errors
- Start with Quick Wins (1 hour)
- Then tackle Medium Priority (2 hours)
- Total: 3-4 hours to finish

**Option 2: Share with Dev Team**
- Create ticket with this guide
- Dev team fixes their domain (User/Partner changes)
- You focus on simple find/replace fixes
- Parallel work, done in 1-2 days

**Option 3: Automated Script**
- Run PowerShell script for enum fixes
- Manually fix UserProfile issues
- Consult team on Partner hierarchy
- Total: 4-5 hours

---

**Files to modify:**
1. DataIntegrityTests.cs
2. OrganizationHierarchyLookupServiceTests.cs  
3. ValuesManagerTests.cs
4. UserDataManagerFullTests.cs
5. ProfileManagerFullTests.cs
6. PartnerTreeManagerFullTests.cs
7. CountryServiceTests.cs
8. OrganizationHierarchyManagerFullTests.cs

---

*Fix Guide | January 13, 2026 | 96 Errors Remaining*
