# Business Manager Functional Tests - Summary

## Overview

This folder contains comprehensive functional test documentation and corresponding C# test implementations for all business managers in the UNOPS Opportunity+ system.

**Total Test Cases**: ~1,200+
**Status**: ✅ Complete - All tests converted to C#

---

## Test Coverage by Manager

| Manager | Documentation | C# Tests | Test Count |
|---------|--------------|----------|------------|
| PartnerManager | ✅ | ✅ `PartnerManagerTests.cs` | 100+ |
| ContactManager | ✅ | ✅ `ContactManagerFullTests.cs` | 120+ |
| InteractionManager | ✅ | ✅ `InteractionManagerFullTests.cs` | 100+ |
| DocumentManager | ✅ | ✅ `DocumentManagerFullTests.cs` | 100+ |
| WorkflowManager | ✅ | ✅ `WorkflowManagerFullTests.cs` | 80+ |
| NotificationManager | ✅ | ✅ `NotificationManagerFullTests.cs` | 80+ |
| UserDataManager | ✅ | ✅ `UserDataManagerFullTests.cs` | 80+ |
| ProfileManager | ✅ | ✅ `ProfileManagerFullTests.cs` | 60+ |
| PartnerTreeManager | ✅ | ✅ `PartnerTreeManagerFullTests.cs` | 65+ |
| OrganizationHierarchyManager | ✅ | ✅ `OrganizationHierarchyManagerFullTests.cs` | 80+ |
| LinkManager | ✅ | ✅ `LinkManagerFullTests.cs` | 60+ |
| DocumentTypeManager | ✅ | ✅ `LinkManagerFullTests.cs` | 40+ |
| ValuesManager | ✅ | ✅ `LinkManagerFullTests.cs` | 40+ |
| SystemAdminManager | ✅ | ✅ `SystemAdminGeminiManagerFullTests.cs` | 60+ |
| GeminiManager | ✅ | ✅ `SystemAdminGeminiManagerFullTests.cs` | 60+ |
| GmailAddonManager | ✅ | ✅ `SystemAdminGeminiManagerFullTests.cs` | 40+ |

---

## C# Test File Locations

All C# test files are located in:
```
QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Managers/
```

## Test Categories

Each manager's tests are organized into these categories:

1. **Create Operations** (TC-XX-F001 to TC-XX-F020)
   - Valid data creation
   - Required fields validation
   - Optional fields handling
   - Audit field population
   - Bulk creation

2. **Get/Read Operations** (TC-XX-F021 to TC-XX-F040)
   - Pagination
   - Filtering
   - Sorting
   - Search
   - Include relationships

3. **Update Operations** (TC-XX-F041 to TC-XX-F060)
   - Field updates
   - Relationship updates
   - Audit trail
   - Concurrent updates
   - Bulk updates

4. **Delete Operations** (TC-XX-F061 to TC-XX-F075)
   - Soft delete
   - Cascade behavior
   - Restore functionality
   - Bulk delete

5. **Additional Tests** (TC-XX-F076+)
   - Permissions
   - Edge cases
   - Performance
   - Integration
   - Audit logging

---

## Running Tests

```powershell
# Run all manager tests
dotnet test "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj" --filter "Namespace~Managers"

# Run specific manager tests
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"
dotnet test --filter "FullyQualifiedName~ContactManagerFullTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

---

## Test Conventions

- **Test ID Format**: `TC_[Manager]_F[Number]_[Description]`
- **Test Method Names**: Descriptive, following `TC_XX_FXXX_Operation_Condition_Result` pattern
- **Assertions**: Using xUnit's Assert methods
- **Data Setup**: In-memory database with seeded test data
- **Isolation**: Each test class uses a unique database instance

---

*Last Updated: December 19, 2025*
