# Controllers Tests

## Overview

This folder contains test documentation and C# integration test implementations for all API controllers in the UNOPS Opportunity+ system.

**Total Test Cases**: ~400+
**Status**: ✅ Complete - All tests converted to C#

---

## Test Coverage

### Core Controllers

| Controller | Documentation | C# Tests | Test Count |
|-----------|--------------|----------|------------|
| PartnerController | ✅ | ✅ | 80+ |
| ContactController | ✅ | ✅ | 60+ |
| InteractionController | ✅ | ✅ | 50+ |
| DocumentController | ✅ | ✅ | 50+ |

### Supporting Controllers

| Controller | Documentation | C# Tests | Test Count |
|-----------|--------------|----------|------------|
| NotificationController | ✅ | ✅ | 40+ |
| WorkflowController | ✅ | ✅ | 40+ |
| UserController | ✅ | ✅ | 50+ |
| SearchController | ✅ | ✅ | 30+ |
| AIController | ✅ | ✅ | 40+ |
| ReportController | ✅ | ✅ | 30+ |
| AdminController | ✅ | ✅ | 40+ |
| OrganizationHierarchyController | ✅ | ✅ | 30+ |
| PartnerTreeController | ✅ | ✅ | 25+ |

### Additional Controllers

| Controller | Test Count |
|-----------|------------|
| DashboardController | 20+ |
| PermissionController | 20+ |
| RoleController | 20+ |
| LiaisonOfficeController | 15+ |
| AnalyticsController | 15+ |
| EntityConfigurationController | 15+ |
| UserProfileController | 20+ |
| CountryController | 15+ |
| BaseEngagementController | 20+ |
| SavedFilterController | 20+ |
| ConfigurationController | 15+ |
| UserPreferenceController | 15+ |

---

## C# Test File Locations

Located in: `QA Tests/Integration Tests/Controllers/`

| File | Controllers Covered |
|------|---------------------|
| PartnerControllerFullTests.cs | Partner, Contact, Interaction, Document |
| AdditionalControllersTests.cs | All other controllers |

---

## Test Categories

### GET Endpoints
- List with pagination
- Filter by various criteria
- Search functionality
- Sort options
- Include related data
- Authorization checks

### POST Endpoints
- Create with valid data
- Validation errors
- Authorization checks
- Bulk operations
- File uploads

### PUT Endpoints
- Update existing records
- Partial updates
- Concurrency handling
- Authorization checks

### DELETE Endpoints
- Soft delete
- Cascade behavior
- Authorization checks
- Bulk delete

---

## HTTP Status Code Coverage

| Status Code | Scenarios Tested |
|-------------|------------------|
| 200 OK | Successful GET/PUT operations |
| 201 Created | Successful POST operations |
| 204 No Content | Successful DELETE operations |
| 400 Bad Request | Validation errors |
| 401 Unauthorized | Missing/invalid authentication |
| 403 Forbidden | Insufficient permissions |
| 404 Not Found | Resource not found |
| 409 Conflict | Concurrency conflicts |
| 429 Too Many Requests | Rate limiting |

---

## Running Tests

```powershell
# Run all controller tests
dotnet test "QA Tests/Integration Tests/UNOPS.PAO.IntegrationTests.csproj"

# Run specific controller tests
dotnet test --filter "FullyQualifiedName~PartnerControllerFullTests"

# Run by HTTP method
dotnet test --filter "Name~Get"    # GET endpoints
dotnet test --filter "Name~Create" # POST endpoints
dotnet test --filter "Name~Update" # PUT endpoints
dotnet test --filter "Name~Delete" # DELETE endpoints
```

---

*Last Updated: December 19, 2025*
