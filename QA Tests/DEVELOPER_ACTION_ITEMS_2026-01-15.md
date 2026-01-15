# Developer Action Items - January 15, 2026

**Generated**: January 15, 2026, 11:45 AM  
**Context**: Post workflow optimization test execution  
**Priority**: CRITICAL, HIGH, MEDIUM, LOW

---

## 🚨 **CRITICAL PRIORITY - BLOCKING ISSUES**

### **1. Fix Build Timeout Issue in Test Execution** ⚠️⚠️⚠️
**Priority**: 🔴 **CRITICAL**  
**Blocking**: Business.Tests & Integration Tests execution  
**Effort**: 2-4 hours  
**Owner**: DevOps / Build Engineer

**Problem:**
```
Command timed out after 180 seconds during build/execution
Error: CS2012: Cannot open 'UNOPS.PAO.Utilities.dll' for writing
Reason: File is being used by another process
```

**Root Cause:**
- Multiple MSBuild processes competing for same DLL files
- File lock contention during parallel builds
- Previous build processes not releasing locks properly
- Long build times due to solution-wide dependencies

**Impact:**
- ❌ Cannot run Business.Tests (2,135 tests)
- ❌ Cannot run Integration Tests (1,365 tests)
- ❌ Cannot verify CI/CD workflow changes
- ❌ PR checks may fail with timeouts

**Solution Options:**

**Option A: Pre-Build with No-Build Test Flag** (Recommended)
```bash
# Step 1: Clean build
dotnet build-server shutdown
dotnet clean
dotnet build --configuration Release

# Step 2: Run tests without rebuilding
dotnet test --no-build --configuration Release --verbosity quiet
```

**Option B: Sequential Test Execution**
```bash
# Run each test project one at a time
dotnet test "QA Tests/C# Tests/UNOPS.PAO.FastTests/UNOPS.PAO.FastTests.csproj"
# Wait for completion
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj"
# Wait for completion
dotnet test "QA Tests/Integration Tests/UNOPS.PAO.IntegrationTests.csproj"
```

**Option C: Update GitHub Actions Workflow**
```yaml
# In .github/workflows/qa-tests.yml
- name: Build Solution Once
  run: dotnet build --configuration Release --no-incremental

- name: Run Tests (No Build)
  run: dotnet test --no-build --configuration Release
  timeout-minutes: 15
```

**Files to Modify:**
- `.github/workflows/qa-tests.yml`
- Local test execution scripts

**Acceptance Criteria:**
- ✅ Business.Tests execute successfully (2,135 tests)
- ✅ Integration Tests execute successfully (1,365 tests)
- ✅ No build timeouts
- ✅ All tests complete within 10 minutes

---

### **2. Update CI/CD Workflow Timeouts** ⚠️⚠️
**Priority**: 🔴 **CRITICAL**  
**Blocking**: Automated PR checks  
**Effort**: 15 minutes  
**Owner**: DevOps

**Problem:**
- Default GitHub Actions timeout is 6 hours, but jobs may hang
- Test execution timeouts default to 30 seconds (too short for Business.Tests)
- Build process can take 2-5 minutes alone

**Solution:**
```yaml
# .github/workflows/qa-tests.yml

jobs:
  fast-tests:
    timeout-minutes: 10  # Add timeout
    steps:
    - name: Run FastTests
      run: dotnet test "QA Tests/C# Tests/UNOPS.PAO.FastTests/UNOPS.PAO.FastTests.csproj"
      timeout-minutes: 5  # Add step timeout

  business-tests:
    timeout-minutes: 20  # Add timeout
    steps:
    - name: Build Solution
      run: dotnet build --configuration Release
      timeout-minutes: 10
    
    - name: Run Business Tests
      run: dotnet test --no-build --configuration Release
      timeout-minutes: 10
```

**Files to Modify:**
- `.github/workflows/qa-tests.yml`

**Acceptance Criteria:**
- ✅ Jobs timeout appropriately (don't hang forever)
- ✅ Sufficient time for builds (10 minutes)
- ✅ Sufficient time for tests (10 minutes per suite)

---

## 🔴 **HIGH PRIORITY - CODE QUALITY ISSUES**

### **3. Fix Null Safety Warnings (CS86xx Series)** 🔴
**Priority**: 🔴 **HIGH**  
**Impact**: Runtime null reference exceptions  
**Effort**: 8-16 hours  
**Owner**: Backend Team Lead

**Problem:**
- **High volume of null safety warnings** across multiple files
- Not enforcing nullable reference type contracts
- Potential runtime null reference exceptions

**Warning Types:**
```csharp
CS8600: Converting null literal or possible null value to non-nullable type
CS8602: Dereference of a possibly null reference
CS8603: Possible null reference return
CS8604: Possible null reference argument
CS8618: Non-nullable field must contain a non-null value when exiting constructor
CS8765: Nullability of type doesn't match overridden member
CS8766: Nullability of reference types doesn't match
```

**Files Affected (Priority Order):**

1. **Critical - Data Access Layer:**
   - `UNOPS.PAO.DataAccess/Context/AppDbContext.cs` (20+ warnings)
   - `UNOPS.PAO.DataAccess/Context/AuditableDbContext.cs` (15+ warnings)
   - `UNOPS.PAO.DataAccess/Services/UserInfoService.cs` (5+ warnings)
   - `UNOPS.PAO.DataAccess/Services/UserResolverService.cs`

2. **Important - Domain Layer:**
   - `UNOPS.PAO.Domain/Specifications/PartnerSpecifications/*.cs` (30+ warnings)
   - `UNOPS.PAO.Domain/Specifications/ContactSpecifications/*.cs` (15+ warnings)
   - `UNOPS.PAO.Domain/Specifications/InteractionSpecifications/*.cs` (15+ warnings)

3. **Moderate - Utilities:**
   - `UNOPS.PAO.Utilities/Helpers/Enumeration.cs` (5+ warnings)
   - `UNOPS.PAO.Utilities/Helpers/Extensions.cs` (8+ warnings)
   - `UNOPS.PAO.Utilities/Helpers/QueryExtensions.cs` (4+ warnings)
   - `UNOPS.PAO.Utilities/Helpers/SpecificationEvaluator.cs`

4. **Lower - Identity/Auth:**
   - `UNOPS.PAO.UNOPSIdentity/Authentication/*.cs` (25+ warnings)

**Solution Approach:**

**Phase 1: Enable Enforcement** (30 min)
```xml
<!-- Add to Directory.Build.props or each .csproj -->
<PropertyGroup>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <!-- Start with just null safety warnings -->
  <WarningsAsErrors>CS8600,CS8602,CS8603,CS8604,CS8618,CS8765,CS8766</WarningsAsErrors>
</PropertyGroup>
```

**Phase 2: Fix Critical Issues** (4-6 hours)
```csharp
// Example fixes:

// Before (CS8618):
public class AppDbContext : AuditableDbContext<int, int>
{
  private readonly UserResolverService<int> _userResolverService;
}

// After:
public class AppDbContext : AuditableDbContext<int, int>
{
  private readonly UserResolverService<int>? _userResolverService;
  // OR
  private readonly UserResolverService<int> _userResolverService = null!;
}

// Before (CS8602):
var name = user.Profile.Name;  // Dereference of possibly null reference

// After:
var name = user.Profile?.Name ?? "Unknown";
// OR
if (user.Profile == null)
  throw new InvalidOperationException("Profile cannot be null");
var name = user.Profile.Name;

// Before (CS8604):
void ProcessUser(User user)
{
  SaveToDatabase(user.Name);  // Possible null argument
}

// After:
void ProcessUser(User user)
{
  ArgumentNullException.ThrowIfNull(user);
  ArgumentException.ThrowIfNullOrEmpty(user.Name);
  SaveToDatabase(user.Name);
}

// Before (CS8603):
public string? GetUserName(int id)
{
  return null;  // Possible null reference return
}

// After:
[return: NotNullIfNotNull(nameof(id))]
public string? GetUserName(int id)
{
  return _users.TryGetValue(id, out var user) ? user.Name : null;
}
```

**Phase 3: Fix Domain/Utilities** (3-5 hours)
- Update specification classes
- Add null checks in query extensions
- Fix helper method contracts

**Phase 4: Fix Identity/Auth** (2-3 hours)
- Update authentication handlers
- Fix IAP middleware
- Add proper null handling

**Acceptance Criteria:**
- ✅ Zero CS86xx warnings in build output
- ✅ All nullable reference types properly annotated
- ✅ Proper null checks at boundaries
- ✅ No runtime null reference exceptions

---

### **4. Fix GetHashCode Override** 🔴
**Priority**: 🔴 **HIGH**  
**Impact**: Incorrect behavior in hash-based collections  
**Effort**: 15 minutes  
**Owner**: Backend Developer

**Problem:**
```csharp
CS0659: 'Enumeration<T>' overrides Object.Equals(object o) but does not override Object.GetHashCode()
```

**File:**
- `UNOPS.PAO.Utilities/Helpers/Enumeration.cs`

**Why This Matters:**
- Breaks `Dictionary<Enumeration<T>, TValue>` behavior
- Breaks `HashSet<Enumeration<T>>` behavior
- Violates .NET equality contract
- Causes incorrect lookups and comparisons

**Solution:**
```csharp
// File: UNOPS.PAO.Utilities/Helpers/Enumeration.cs

public abstract class Enumeration<T> : IComparable
{
  public string Name { get; private set; }
  public T Value { get; private set; }

  // Existing Equals implementation
  public override bool Equals(object? obj)
  {
    if (obj is not Enumeration<T> otherValue)
      return false;

    var typeMatches = GetType() == obj.GetType();
    var valueMatches = Name?.Equals(otherValue.Name, StringComparison.Ordinal) ?? false;

    return typeMatches && valueMatches;
  }

  // ADD THIS METHOD:
  public override int GetHashCode()
  {
    return HashCode.Combine(GetType(), Name);
  }

  // Alternative if targeting older .NET:
  // public override int GetHashCode()
  // {
  //   return (GetType(), Name).GetHashCode();
  // }
}
```

**Acceptance Criteria:**
- ✅ CS0659 warning resolved
- ✅ GetHashCode implemented correctly
- ✅ Consistent with Equals implementation
- ✅ Test hash-based collections work correctly

---

### **5. Update Obsolete API Usage (ISystemClock)** 🔴
**Priority**: 🔴 **HIGH**  
**Impact**: Future .NET compatibility  
**Effort**: 1-2 hours  
**Owner**: Identity Team

**Problem:**
```csharp
CS0618: 'ISystemClock' is obsolete: 'Use TimeProvider instead.'
```

**Files:**
- `UNOPS.PAO.UNOPSIdentity/Authentication/IAPAuthenticationHandler.cs`

**Solution:**
```csharp
// Before:
using Microsoft.AspNetCore.Authentication;

public class IAPAuthenticationHandler : AuthenticationHandler<IAPAuthenticationOptions>
{
  private readonly ISystemClock _clock;  // ❌ Obsolete

  public IAPAuthenticationHandler(
    IOptionsMonitor<IAPAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock)  // ❌ Obsolete
    : base(options, logger, encoder, clock)
  {
    _clock = clock;
  }

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var now = _clock.UtcNow;  // ❌ Obsolete
    // ... rest of implementation
  }
}

// After:
using Microsoft.AspNetCore.Authentication;

public class IAPAuthenticationHandler : AuthenticationHandler<IAPAuthenticationOptions>
{
  private readonly TimeProvider _timeProvider;  // ✅ Modern API

  public IAPAuthenticationHandler(
    IOptionsMonitor<IAPAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TimeProvider timeProvider)  // ✅ Modern API
    : base(options, logger, encoder)
  {
    _timeProvider = timeProvider;
  }

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var now = _timeProvider.GetUtcNow();  // ✅ Modern API
    // ... rest of implementation
  }
}

// Update DI registration in Startup.cs or Program.cs:
services.AddSingleton(TimeProvider.System);
```

**Acceptance Criteria:**
- ✅ CS0618 warning resolved
- ✅ All ISystemClock references removed
- ✅ TimeProvider properly injected
- ✅ Authentication still works correctly

---

## 🟡 **MEDIUM PRIORITY - CODE CLEANUP**

### **6. Remove Unused Fields and Variables** 🟡
**Priority**: 🟡 **MEDIUM**  
**Impact**: Code cleanliness, reduced confusion  
**Effort**: 1-2 hours  
**Owner**: Backend Team

**Problem:**
```csharp
CS0169: The field 'AppDbContext._userResolverService' is never used
CS0169: The field 'UserResolverService<TUserId>._userId' is never used
CS0168: The variable 'ex' is declared but never used
CS0219: The variable 'jwtVerified' is assigned but its value is never used
```

**Files:**
- `UNOPS.PAO.DataAccess/Context/AppDbContext.cs`
- `UNOPS.PAO.DataAccess/Services/UserResolverService.cs`
- `UNOPS.PAO.UNOPSIdentity/Authentication/IAPAuthenticationHandler.cs`

**Solution:**
```csharp
// Option 1: Remove if truly unused
// Delete the field/variable entirely

// Option 2: Suppress warning if intentionally unused
#pragma warning disable CS0169  // Field is never used
private readonly UserResolverService<int> _userResolverService;
#pragma warning restore CS0169

// Option 3: Use the field (if it should be used)
// Add logic that actually uses the field

// For unused exception variables:
// Before:
try { /* code */ }
catch (Exception ex) {  // ❌ 'ex' never used
  logger.LogError("Error occurred");
}

// After:
try { /* code */ }
catch (Exception ex) {  // ✅ Used
  logger.LogError(ex, "Error occurred");
}
// OR
try { /* code */ }
catch (Exception) {  // ✅ Don't declare if not needed
  logger.LogError("Error occurred");
}
```

**Acceptance Criteria:**
- ✅ CS0169, CS0168, CS0219 warnings resolved
- ✅ No unused fields in codebase
- ✅ Exception variables properly used or omitted

---

### **7. Fix Name Hiding Warnings** 🟡
**Priority**: 🟡 **MEDIUM**  
**Impact**: Code clarity  
**Effort**: 30 minutes  
**Owner**: Backend Developer

**Problem:**
```csharp
CS0108: 'ArtifactDataType.Name' hides inherited member 'ModifiableDeletableEntity<int, int>.Name'. Use the new keyword if hiding was intended.
CS0108: 'ArtifactType.Name' hides inherited member
CS0108: 'EntityArtifact.Name' hides inherited member
```

**Files:**
- `UNOPS.PAO.Domain/Entities/ArtifactDataType.cs`
- `UNOPS.PAO.Domain/Entities/ArtifactType.cs`
- `UNOPS.PAO.Domain/Entities/EntityArtifact.cs`

**Solution:**
```csharp
// If hiding is intentional:
public class ArtifactDataType : ModifiableDeletableEntity<int, int>
{
  public new string Name { get; set; }  // ✅ Explicitly hide base property
}

// Better: Avoid hiding if possible
public class ArtifactDataType : ModifiableDeletableEntity<int, int>
{
  public string TypeName { get; set; }  // ✅ Different name, no hiding
}
```

**Acceptance Criteria:**
- ✅ CS0108 warnings resolved
- ✅ Clear inheritance hierarchy
- ✅ No ambiguous property names

---

### **8. Fix Type Parameter Shadowing** 🟡
**Priority**: 🟡 **MEDIUM**  
**Impact**: Code clarity  
**Effort**: 15 minutes  
**Owner**: Backend Developer

**Problem:**
```csharp
CS0693: Type parameter 'T' has the same name as the type parameter from outer type 'Enumeration<T>'
```

**File:**
- `UNOPS.PAO.Utilities/Helpers/Enumeration.cs`

**Solution:**
```csharp
// Before:
public abstract class Enumeration<T>
{
  public static IEnumerable<T> GetAll<T>() where T : Enumeration<T>  // ❌ T shadows outer T
  {
    // ...
  }
}

// After:
public abstract class Enumeration<T>
{
  public static IEnumerable<TEnum> GetAll<TEnum>() where TEnum : Enumeration<T>  // ✅ Different name
  {
    // ...
  }
}
```

**Acceptance Criteria:**
- ✅ CS0693 warning resolved
- ✅ No type parameter name conflicts
- ✅ Clear generic parameter naming

---

## 🟢 **LOW PRIORITY - OPTIMIZATION & POLISH**

### **9. Reduce Solution Build Time** 🟢
**Priority**: 🟢 **LOW**  
**Impact**: Developer productivity  
**Effort**: 4-8 hours  
**Owner**: Architecture Team

**Problem:**
- Solution-wide builds take 2-5 minutes
- Test execution requires full solution rebuild
- File lock contention during parallel builds

**Investigation Areas:**
1. Project dependency graph
2. Unnecessary project references
3. Large projects that could be split
4. Build configuration optimization

**Potential Solutions:**
- Split large projects into smaller modules
- Use NuGet packages for shared code
- Enable incremental builds properly
- Use build caching
- Optimize project references

**Acceptance Criteria:**
- ✅ Build time < 2 minutes for full rebuild
- ✅ Incremental builds < 30 seconds
- ✅ No file lock contention

---

### **10. Add Build Performance Monitoring** 🟢
**Priority**: 🟢 **LOW**  
**Impact**: Visibility into build performance  
**Effort**: 2-3 hours  
**Owner**: DevOps

**Solution:**
```bash
# Add build timing to GitHub Actions
- name: Build with Timing
  run: |
    $start = Get-Date
    dotnet build --configuration Release
    $duration = (Get-Date) - $start
    Write-Host "Build completed in $($duration.TotalSeconds) seconds"
    echo "BUILD_DURATION=$($duration.TotalSeconds)" >> $env:GITHUB_ENV

# Track build metrics over time
- name: Report Build Metrics
  run: |
    echo "Build Duration: ${{ env.BUILD_DURATION }}s" >> $GITHUB_STEP_SUMMARY
```

---

### **11. Add Pre-Commit Hooks** 🟢
**Priority**: 🟢 **LOW**  
**Impact**: Code quality gates  
**Effort**: 1-2 hours  
**Owner**: DevOps

**Solution:**
```bash
# .git/hooks/pre-commit
#!/bin/sh

echo "Running pre-commit checks..."

# Check for build errors
dotnet build --no-incremental
if [ $? -ne 0 ]; then
  echo "❌ Build failed. Commit aborted."
  exit 1
fi

# Run fast tests
dotnet test "QA Tests/C# Tests/UNOPS.PAO.FastTests/UNOPS.PAO.FastTests.csproj" --no-build
if [ $? -ne 0 ]; then
  echo "❌ Tests failed. Commit aborted."
  exit 1
fi

echo "✅ Pre-commit checks passed"
exit 0
```

---

## 📊 **SUMMARY BY PRIORITY**

### **🔴 CRITICAL (Must Fix Immediately):**
1. ✅ Fix build timeout issue (2-4 hours)
2. ✅ Update CI/CD workflow timeouts (15 minutes)

**Total Critical Effort**: ~2-4 hours  
**Blocking**: Test execution and CI/CD

---

### **🔴 HIGH (Fix This Sprint):**
3. ✅ Fix null safety warnings (8-16 hours)
4. ✅ Fix GetHashCode override (15 minutes)
5. ✅ Update obsolete API usage (1-2 hours)

**Total High Effort**: ~10-18 hours  
**Impact**: Code quality, runtime safety, future compatibility

---

### **🟡 MEDIUM (Fix Next Sprint):**
6. ✅ Remove unused fields/variables (1-2 hours)
7. ✅ Fix name hiding warnings (30 minutes)
8. ✅ Fix type parameter shadowing (15 minutes)

**Total Medium Effort**: ~2-3 hours  
**Impact**: Code cleanliness, maintainability

---

### **🟢 LOW (Nice to Have):**
9. ✅ Reduce solution build time (4-8 hours)
10. ✅ Add build performance monitoring (2-3 hours)
11. ✅ Add pre-commit hooks (1-2 hours)

**Total Low Effort**: ~7-13 hours  
**Impact**: Developer experience, productivity

---

## 📋 **SPRINT PLANNING RECOMMENDATION**

### **Sprint 1 (Current Sprint):**
**Focus**: Unblock test execution + Critical code quality

1. Fix build timeout issue (2-4 hours) - **CRITICAL**
2. Update CI/CD timeouts (15 min) - **CRITICAL**
3. Fix null safety warnings - Phase 1 & 2 (4-6 hours) - **HIGH**
4. Fix GetHashCode (15 min) - **HIGH**
5. Update obsolete APIs (1-2 hours) - **HIGH**

**Total**: ~8-13 hours  
**Outcome**: Tests running, critical warnings resolved

---

### **Sprint 2 (Next Sprint):**
**Focus**: Complete code quality + Cleanup

6. Fix null safety - Phase 3 & 4 (5-8 hours) - **HIGH**
7. Remove unused code (1-2 hours) - **MEDIUM**
8. Fix name hiding (30 min) - **MEDIUM**
9. Fix type parameter shadowing (15 min) - **MEDIUM**

**Total**: ~7-11 hours  
**Outcome**: Zero warnings, clean codebase

---

### **Sprint 3+ (Future Sprints):**
**Focus**: Performance & Developer Experience

10. Reduce build time (4-8 hours) - **LOW**
11. Add build monitoring (2-3 hours) - **LOW**
12. Add pre-commit hooks (1-2 hours) - **LOW**

**Total**: ~7-13 hours  
**Outcome**: Faster builds, better DX

---

## ✅ **ACCEPTANCE CRITERIA FOR COMPLETION**

### **Definition of Done:**
- ✅ All tests execute successfully without timeouts
- ✅ Zero compilation warnings (clean build)
- ✅ No unused code (fields, variables, methods)
- ✅ All nullable reference types properly annotated
- ✅ No obsolete API usage
- ✅ CI/CD pipeline runs reliably
- ✅ Build time < 2 minutes (full rebuild)
- ✅ Test execution < 5 minutes (all suites)

---

**Document Status**: ✅ **READY FOR REVIEW**  
**Next Update**: After critical issues resolved  
**Owner**: Development Team Lead

---

*Generated: January 15, 2026, 11:45 AM*  
*Location: QA Tests/DEVELOPER_ACTION_ITEMS_2026-01-15.md*
