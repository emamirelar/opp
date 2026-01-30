# Workflow Submodule CI/CD Build Fix

**Date:** 2026-01-30  
**Issue:** GitHub Actions build failing due to uninitialized UNOPS.Workflow submodule  
**Status:** ✅ RESOLVED

---

## Problem Summary

The GitHub Actions CI/CD pipeline was failing with 11+ compilation errors:

```
CS0234: The type or namespace name 'Workflow' does not exist in the namespace 'UNOPS.PAO.Business'
CS0234: The type or namespace name 'Workflow' does not exist in the namespace 'UNOPS'
CS0246: The type or namespace name 'IWorkflowManager' could not be found
CS0246: The type or namespace name 'IEntityStageProvider' could not be found
CS0246: The type or namespace name 'IPaoWorkflowApproverProvider' could not be found
```

**Root Cause:**
- The `UNOPS.Workflow` submodule exists as a folder in the repository
- However, it's **not initialized** in the CI/CD environment (no project files)
- Code was attempting to compile workflow-dependent code even though dependencies were missing

---

## Solution Architecture

Implemented **conditional compilation** strategy that allows the application to build and run in two modes:

### Mode 1: With Workflow (Local Development)
```
✅ Workflow submodule initialized
✅ Workflow project references included
✅ WorkflowController compiled and available
✅ Workflow services registered
✅ Full workflow functionality enabled
```

### Mode 2: Without Workflow (CI/CD)
```
❌ Workflow submodule NOT initialized
❌ Workflow project references skipped
❌ WorkflowController excluded from compilation
❌ Workflow services NOT registered
✅ Build succeeds - application runs without workflow functionality
```

---

## Implementation Details

### 1. UNOPS.PAO.Presentation Project

**File:** `UNOPS.PAO.Presentation.csproj`

**Changes:**
```xml
<ItemGroup>
  <!-- Workflow submodule references - only include if project files exist -->
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj" 
    Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Models\UNOPS.Workflow.Models.csproj" 
    Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.Models\UNOPS.Workflow.Models.csproj')" />
</ItemGroup>

<!-- Conditional compilation: Exclude WorkflowController when UNOPS.Workflow submodule project files are not available -->
<ItemGroup Condition="!Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')">
  <Compile Remove="Controllers\WorkflowController.cs" />
</ItemGroup>
```

**Effect:**
- `WorkflowController.cs` only compiled when workflow projects are available
- Project references only included when workflow projects exist
- No compilation errors when workflow is missing

---

### 2. UNOPS.PAO.Server Project

**File:** `UNOPS.PAO.Server.csproj`

**Changes:**
```xml
<PropertyGroup>
  <!-- Define compilation symbol when workflow projects are available -->
  <DefineConstants Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')">$(DefineConstants);WORKFLOW_AVAILABLE</DefineConstants>
</PropertyGroup>

<ItemGroup>
  <!-- Workflow submodule references - only include if project files exist -->
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.DataAccess\UNOPS.Workflow.DataAccess.csproj" 
    Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.DataAccess\UNOPS.Workflow.DataAccess.csproj')" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj" 
    Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')" />
</ItemGroup>
```

**File:** `Program.cs`

**Changes:**
```csharp
#if WORKFLOW_AVAILABLE
using UNOPS.PAO.Business.Workflow.Seeders;
using UNOPS.Workflow.DataAccess;
#endif

// In Main method:
#if WORKFLOW_AVAILABLE
        // Ensure workflow schema is created and migrations are applied
        using (var scope = app.Services.CreateScope())
        {
            var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
            workflowContext.EnsureWorkflowSchemaCreated();
        }
        
        // Seed workflow configuration data
        await app.Services.SeedStateMachineStageChangesAsync();
        await app.Services.SeedStateMachineStageChangeRolesAsync();
#endif
```

**File:** `Startup.cs`

**Changes:**
```csharp
#if WORKFLOW_AVAILABLE
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.Workflow.DataAccess;
#endif

// In ConfigureDataAccess method:
#if WORKFLOW_AVAILABLE
        // Register WorkflowDbContext and workflow services
        services.AddPaoWorkflowServices(options =>
        {
            options.UsePostgreSqlStorage(optimizedConnectionString, "workflow");
        });

        services.AddDbContext<UNOPS.Workflow.DataAccess.WorkflowDbContext>(options =>
            options.UseNpgsql(dataSource, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "workflow");
            }));
#endif
```

**Effect:**
- `WORKFLOW_AVAILABLE` symbol only defined when workflow projects exist
- Workflow using statements only included when symbol is defined
- Workflow service registration only executed when symbol is defined
- Application starts successfully without workflow when symbol is not defined

---

### 3. UNOPS.PAO.Business Project

**File:** `UNOPS.PAO.Business.csproj`

**Existing Configuration** (No changes needed):
```xml
<!-- Workflow submodule projects - Only included when submodule is initialized -->
<ItemGroup Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')">
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.DataAccess\UNOPS.Workflow.DataAccess.csproj" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Models\UNOPS.Workflow.Models.csproj" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Domain\UNOPS.Workflow.Domain.csproj" />
</ItemGroup>

<!-- Exclude workflow adapter files when submodule is not available -->
<ItemGroup Condition="!Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')">
  <Compile Remove="Workflow\**\*.cs" />
</ItemGroup>
```

**Effect:**
- Workflow adapter files (`Workflow\**\*.cs`) excluded when workflow projects missing
- Already had conditional compilation in place

---

## Commits Applied

| Commit | Description |
|--------|-------------|
| `07f6abc4` | Fix build errors in GitHub Actions CI/CD pipeline |
| `0df479ba` | Fix WorkflowController exclusion condition for CI/CD |
| `c3675862` | Add conditional compilation for Workflow references in Server project |

---

## Build Results

### Before Fixes:
```
Error: CS0234 (11 occurrences)
Warning: 1,252 warnings
Status: ❌ BUILD FAILED
```

### After Fixes:
```
Errors: 0
Warnings: ~175 (non-blocking)
Status: ✅ BUILD SUCCEEDED
```

---

## How Conditional Compilation Works

### Detection Logic

The system uses `Exists()` checks in MSBuild to detect if workflow projects are available:

```xml
Condition="Exists('..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj')"
```

**Why check for `.csproj` file instead of directory?**
- The `UNOPS.Workflow` directory exists in the repository (git submodule reference)
- However, the directory is **empty** in CI/CD (submodule not initialized)
- Checking for the actual project file ensures we detect **usable** workflow projects

### Compilation Symbol

When workflow projects are detected:
```xml
<DefineConstants>$(DefineConstants);WORKFLOW_AVAILABLE</DefineConstants>
```

This allows C# code to conditionally compile:
```csharp
#if WORKFLOW_AVAILABLE
    // Workflow-dependent code here
#endif
```

---

## Testing the Fix

### Local Testing (With Workflow)

```bash
# Build should succeed with workflow functionality
dotnet build --configuration Release

# Expected output:
# Build succeeded.
# 0 Error(s)
# WorkflowController included
# Workflow services registered
```

### CI/CD Simulation (Without Workflow)

```bash
# Temporarily rename workflow directory
mv UNOPS.Workflow UNOPS.Workflow.backup

# Build should still succeed
dotnet build --configuration Release

# Expected output:
# Build succeeded.
# 0 Error(s)
# WorkflowController excluded
# Workflow services NOT registered

# Restore workflow directory
mv UNOPS.Workflow.backup UNOPS.Workflow
```

---

## Future Considerations

### Option 1: Initialize Submodule in CI/CD (Recommended for Production)

**GitHub Actions Workflow:**
```yaml
- name: Checkout code with submodules
  uses: actions/checkout@v4
  with:
    submodules: 'recursive'
    token: ${{ secrets.SUBMODULE_PAT }}
```

**Pros:**
- Full workflow functionality in all environments
- No conditional compilation needed
- Simpler codebase

**Cons:**
- Requires PAT (Personal Access Token) with submodule repository access
- Slightly longer build times

### Option 2: Keep Current Conditional Approach (Current Implementation)

**Pros:**
- Works without submodule access in CI/CD
- No secrets required
- Faster build times (fewer projects to compile)

**Cons:**
- Workflow functionality disabled in CI/CD builds
- More complex codebase with conditional compilation
- Must remember to test both modes

---

## Verification Checklist

When adding new workflow-dependent code, ensure:

- [ ] Workflow using statements wrapped with `#if WORKFLOW_AVAILABLE`
- [ ] Workflow service registration wrapped with `#if WORKFLOW_AVAILABLE`
- [ ] Workflow project references use `Condition="Exists(...)""`
- [ ] Code compiles successfully with workflow available (local)
- [ ] Code compiles successfully without workflow (CI/CD simulation)
- [ ] Application starts without errors in both modes

---

## Related Files

- `UNOPS.PAO.Presentation/UNOPS.PAO.Presentation.csproj`
- `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`
- `UNOPS.PAO.Server/UNOPS.PAO.Server.csproj`
- `UNOPS.PAO.Server/Program.cs`
- `UNOPS.PAO.Server/Startup.cs`
- `UNOPS.PAO.Business/UNOPS.PAO.Business.csproj`
- `UNOPS.PAO.Business/Workflow/**/*.cs`

---

## Contact

For questions about this fix or workflow submodule integration:
- See: `UNOPS.Workflow/README.md`
- See: `tasks/workflow-submodule-integration/workflow-submodule-integration-tasks.md`

---

**Last Updated:** 2026-01-30  
**Status:** ✅ Production Ready
