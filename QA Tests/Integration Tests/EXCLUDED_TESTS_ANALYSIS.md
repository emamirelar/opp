# Excluded Integration Test Files — Detailed Analysis

**Date:** 2026-02-16  
**Reference:** DEF-007 (Updated 2026-02-16)  
**Total Excluded:** 58 files

---

## 1. Compile Remove Entries from .csproj

### Category C: External Dependency (1 file)

| File | Reason |
|------|--------|
| `Controllers\WorkflowControllerTests.cs` | Depends on UNOPS.Workflow (separate repository; project references commented out in csproj) |

---

### Category A: Manager Not in IManagerWrapper (31 files)

| Block | Files | Missing Manager |
|-------|-------|-----------------|
| **Dashboard/** | `DashboardEdgeCaseTests.cs`, `DashboardNegativeTests.cs`, `DashboardSecurityTests.cs`, `DashboardValidationTests.cs` | `IManagerWrapper.DashboardManager` |
| **PartnerAnalytics/** | `AnalyticsEdgeCaseTests.cs`, `AnalyticsNegativeTests.cs`, `AnalyticsSecurityTests.cs`, `AnalyticsValidationTests.cs` | `IManagerWrapper.PartnerAnalyticsManager` |
| **ContactAnalytics/** | `ContactAnalyticsEdgeCaseTests.cs`, `ContactAnalyticsNegativeTests.cs`, `ContactAnalyticsSecurityTests.cs`, `ContactAnalyticsValidationTests.cs` | `IManagerWrapper.ContactAnalyticsManager` |
| **OrgHierarchy/** | `OrgHierarchyEdgeCaseTests.cs`, `OrgHierarchyNegativeTests.cs`, `OrgHierarchySecurityTests.cs`, `OrgHierarchyValidationTests.cs` | `IManagerWrapper.OrganizationManager` |
| **UserProfile/** | `UserProfileEdgeCaseTests.cs`, `UserProfileNegativeTests.cs`, `UserProfileSecurityTests.cs` | `IManagerWrapper.UserProfileManager` |
| **Roles/** | `RoleEdgeCaseTests.cs`, `RoleNegativeTests.cs`, `RoleSecurityTests.cs`, `RoleValidationTests.cs` | Custom RoleManager (only ASP.NET Identity RoleManager exists) |
| **Permissions/** | `PermissionEdgeCaseTests.cs`, `PermissionNegativeTests.cs`, `PermissionSecurityTests.cs`, `PermissionValidationTests.cs` | `IManagerWrapper.PermissionManager` |
| **LiaisonOffice/** | `LiaisonOfficeEdgeCaseTests.cs`, `LiaisonOfficeNegativeTests.cs`, `LiaisonOfficeSecurityTests.cs`, `LiaisonOfficeValidationTests.cs` | `IManagerWrapper.LiaisonOfficeManager` (LiaisonOfficeService exists but not exposed as manager) |

---

### Category B: Manager Exists but API Differs (19 files)

| Block | Files | Manager | API Mismatch |
|-------|-------|---------|--------------|
| **SystemAdmin/** | 4 files | `ISystemAdminManager` | Tests call GetSystemSettingsAsync, UpdateSystemSettingsAsync, GetAuditLogsAsync, ExecuteMaintenanceTaskAsync — not implemented |
| **PartnerTree/** | 4 files | `IPartnerManager` (tests use PartnerManager) | Tests call AddChildPartnerAsync, MovePartnerAsync, GetPartnerChildrenAsync — not on IPartnerManager or IPartnerTreeManager |
| **Documents/** | 3 files | `IDocumentManager` | Tests call UploadDocumentAsync(byte[], string, string, int, ClaimsPrincipal) and DeleteDocumentAsync(int, ClaimsPrincipal) — different signatures |
| **UserManagement/** | 4 files | `IUserManagementManager` | Tests use UserManager with CreateUserAsync(CreateUserRequest, user), AssignRolesAsync — ASP.NET Identity has different API |
| **EntityConfiguration/** | 4 files | `IUNOPSEntityConfigurationManager` (in UNOPSManagerWrapper) | Tests reference EntityConfigManager (not in IManagerWrapper), use CreateEntityConfigRequest, AddFieldRequest — models don't exist |

---

### Category D: Controller Endpoint Issues (7 files)

| File | Issue |
|------|-------|
| `Controllers\NotificationControllerTests.cs` | Tests hit /api/notifications/{id}, /api/notifications/send, /api/notifications/send-bulk, etc. Real controller only has GET /api/notifications and PUT /api/notifications/{id}/read |
| `Controllers\DashboardControllerTests.cs` | Tests hit /api/dashboard/summary, /api/dashboard/partners/stats, /api/dashboard/widgets/*. Real controller uses /api/dashboard/my-partners, /api/dashboard/content |
| `Controllers\CommonEntitiesControllerTests.cs` | Tests hit /api/common/* endpoints. Real CommonEntitiesController has NO action methods |
| `Controllers\EntityConfigurationControllerTests.cs` | Tests use /api/admin/entity-configuration/*. Real routes differ |
| `Controllers\ConfigurationControllerTests.cs` | Tests use /api/configuration/public, /api/configuration/version. Real controller only has GET /api/configuration |
| `Controllers\ContactAnalyticsControllerTests.cs` | Tests use /api/contacts/analytics/*. Real routes are /api/contact-analytics/* |
| `Controllers\PartnerAnalyticsControllerTests.cs` | Tests use /api/partners/analytics/*. Real routes are /api/partner/analytics/* |

---

## 2. Representative File Analysis by Category

### Category A: DashboardEdgeCaseTests.cs

**Infrastructure expectations:**
- `PAOWebApplicationFactory<Program>` as IClassFixture
- `scope.ServiceProvider.GetRequiredService<IManagerWrapper>().DashboardManager`
- `ClaimsPrincipal` with `ClaimTypes.NameIdentifier`, `ClaimTypes.Name`, `ClaimTypes.Role`

**Methods expected on IDashboardManager (does not exist):**
| Method | Signature (expected) |
|--------|----------------------|
| GetRecentActivitiesAsync | `(ClaimsPrincipal user, int count)` |
| GetRecentActivitiesAsync | `(ClaimsPrincipal user, string entityType, int count)` |
| GetMetricsAsync | `(ClaimsPrincipal user, DateTime start, DateTime end)` |
| GetDashboardDataAsync | `(ClaimsPrincipal user)` |
| RefreshDashboardAsync | `(ClaimsPrincipal user)` |
| GetDashboardStatsAsync | `(ClaimsPrincipal user)` |

**No stub managers** — tests expect real `IManagerWrapper` from DI. `DashboardManager` property does not exist on `IManagerWrapper`.

---

### Category B: SystemAdminEdgeCaseTests.cs

**Methods expected on ISystemAdminManager:**
| Expected | Actual ISystemAdminManager |
|----------|----------------------------|
| `GetSystemSettingsAsync(ClaimsPrincipal)` | ❌ Not implemented |
| `UpdateSystemSettingsAsync(UpdateSystemSettingsRequest, ClaimsPrincipal)` | ❌ Not implemented |
| `GetAuditLogsAsync(DateTime, DateTime, ClaimsPrincipal, int? limit, int? offset, int? userId, string? action)` | ❌ Not implemented |
| `ClearCacheAsync(ClaimsPrincipal)` | ❌ Not implemented |
| `GetSystemHealthAsync(ClaimsPrincipal)` | ❌ Not implemented |
| `GetActiveUsersAsync(int? limit, ClaimsPrincipal)` | ❌ Not implemented |
| `BackupDatabaseAsync(ClaimsPrincipal)` | ❌ Not implemented |
| `GetSystemLogsAsync(string type, ClaimsPrincipal, string? filter, string? orderBy)` | ❌ Not implemented |
| `PurgeOldDataAsync(DateTime beforeDate, ClaimsPrincipal)` | ❌ Not implemented |
| `GetSystemMetricsAsync(ClaimsPrincipal, int? timeRangeMinutes)` | ❌ Not implemented |
| `ExecuteMaintenanceTaskAsync(string task, ClaimsPrincipal)` | ❌ Not implemented |
| `RunDatabaseOptimizationAsync(ClaimsPrincipal)` | ❌ Not implemented |
| `RestartServiceAsync(string serviceName, ClaimsPrincipal)` | ❌ Not implemented |

**Actual ISystemAdminManager methods:**
- `RunMigrations()`
- `RunSeeding()`
- `RunSpecificSeeder(string seederName)`
- `TruncateSeedScripts()`
- `DeleteSeedScript(string scriptName)`

**Models expected:** `UpdateSystemSettingsRequest` (from `UNOPS.PAO.Models.Admin`) — may or may not exist; tests use properties: MaintenanceMode, SessionTimeoutMinutes, MaxUploadSizeMB, SMTPServer, FeatureFlags.

---

### Category B: DocumentEdgeCaseTests.cs

**Methods expected on IDocumentManager:**
| Expected | Actual IDocumentManager / UNOPSDocumentManager |
|----------|-----------------------------------------------|
| `UploadDocumentAsync(byte[] fileData, string fileName, string entityType, int entityId, ClaimsPrincipal user)` | ❌ IDocumentManager has no UploadDocumentAsync. UNOPSDocumentManager has `UploadDocumentAsync(DocumentUploadModel model, string entityFolderId)` |
| `GetDocumentByIdAsync(int id, ClaimsPrincipal user)` | `GetDocumentByIdAsync(int documentId)` — no user param |
| `GetDocumentsByEntityAsync(string entityType, int entityId, ClaimsPrincipal user)` | `GetDocumentsByEntityAsync(string entityName, int entityId)` — no user param |
| `UpdateDocumentAsync(UpdateDocumentRequest request, ClaimsPrincipal user)` | `UpdateDocumentAsync(UpdateDocumentRequest request)` — no user param |
| `DeleteDocumentAsync(int id, ClaimsPrincipal user)` | ❌ Not on IDocumentManager at all |

---

### Category B: PartnerTreeEdgeCaseTests.cs

**Note:** Tests use `PartnerManager`, not `PartnerTreeManager`. `IPartnerTreeManager` manages Partner *Trees* (groupings), not parent-child hierarchy of partners.

**Methods expected on IPartnerManager (from tests):**
| Expected | Actual IPartnerManager |
|----------|------------------------|
| `GetPartnerTreeAsync(int id, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerTreeAsync(int id, ClaimsPrincipal user, int? maxDepth)` | ❌ Not on IPartnerManager |
| `AddChildPartnerAsync(int parentId, int childId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerChildrenAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerParentAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerAncestorsAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerDescendantsAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `RemoveChildPartnerAsync(int parentId, int childId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerLevelAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerSiblingsAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `MovePartnerAsync(int partnerId, int? newParentId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |
| `GetPartnerPathAsync(int partnerId, ClaimsPrincipal user)` | ❌ Not on IPartnerManager |

**IPartnerTreeManager** has: CreatePartnerTreeAsync, GetPartnerTreesAsync, GetPartnerTreeAsync (returns PartnerTreeModel for a tree/category, not partner hierarchy).

---

### Category B: EntityConfigEdgeCaseTests.cs

**Tests reference:** `IManagerWrapper.EntityConfigManager` — **does not exist**. `IUNOPSEntityConfigurationManager` exists in UNOPSManagerWrapper as `EntityConfigurationManager`, but:
- Not on base `IManagerWrapper` (only on UNOPS override)
- Different property name: `EntityConfigurationManager` vs `EntityConfigManager`
- Different models: `CreateEntityConfigRequest`, `AddFieldRequest` vs `CreateEntityConfigurationRequest`, `CreateEntityFieldRequest`

---

## 3. Test Infrastructure

### Infrastructure Folder Contents

| File | Purpose |
|------|---------|
| `PAOWebApplicationFactory.cs` | WebApplicationFactory with in-memory DB, TestAuthHandler, mock services |
| `IntegrationTestBase.cs` | Base class with auth headers, HttpClient, GetAsync/PostAsync helpers |
| `TestAuthHandler.cs` | Authentication handler for test requests |
| `TestPermissionService.cs` | Test implementation of IPermissionService |
| `TestOrgUnitHierarchyService.cs` | Test implementation of IOrgUnitHierarchyService |
| `MockServices/MockUserInfoService.cs` | Mock user info |
| `MockServices/MockGoogleCredential.cs` | Mock Google credential for AI |
| `MockServices/MockCacheServices.cs` | Mock cache services |
| `MockServices/MockAiContextualService.cs` | Mock AI contextual service |

**No StubManagerWrapper or mock manager files.** Tests resolve `IManagerWrapper` from DI and call real managers.

### PAOWebApplicationFactory Key Configuration

- In-memory databases for UNOPSAppDbContext, AppDbContext, PAOIdentityDbContext
- TestAuthHandler for authentication
- TestOrgUnitHierarchyService, TestPermissionService replace production services
- Mock services for Google Credential, UserProfileCache, ScreenContextCache, GeoTimeCache, UserInfoService, AiContextualService
- TestDataSeeder for basic data
- No manager replacement — uses real ManagerWrapper and managers

---

## 4. Category B: Specific Compilation Errors

### SystemAdminEdgeCaseTests.cs

```
error CS1061: 'ISystemAdminManager' does not contain a definition for 'GetSystemSettingsAsync' and no accessible extension method 'GetSystemSettingsAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'UpdateSystemSettingsAsync' and no accessible extension method 'UpdateSystemSettingsAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'GetAuditLogsAsync' and no accessible extension method 'GetAuditLogsAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'ClearCacheAsync' and no accessible extension method 'ClearCacheAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'GetSystemHealthAsync' and no accessible extension method 'GetSystemHealthAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'GetActiveUsersAsync' and no accessible extension method 'GetActiveUsersAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'BackupDatabaseAsync' and no accessible extension method 'BackupDatabaseAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'GetSystemLogsAsync' and no accessible extension method 'GetSystemLogsAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'PurgeOldDataAsync' and no accessible extension method 'PurgeOldDataAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'GetSystemMetricsAsync' and no accessible extension method 'GetSystemMetricsAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'ExecuteMaintenanceTaskAsync' and no accessible extension method 'ExecuteMaintenanceTaskAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'RunDatabaseOptimizationAsync' and no accessible extension method 'RunDatabaseOptimizationAsync' accepting a first argument of type 'ISystemAdminManager' could be found
error CS1061: 'ISystemAdminManager' does not contain a definition for 'RestartServiceAsync' and no accessible extension method 'RestartServiceAsync' accepting a first argument of type 'ISystemAdminManager' could be found
```

### DocumentEdgeCaseTests.cs

```
error CS1061: 'IDocumentManager' does not contain a definition for 'UploadDocumentAsync' and no accessible extension method 'UploadDocumentAsync' accepting a first argument of type 'IDocumentManager' could be found (are you missing a using directive or an assembly reference?)
error CS1061: 'IDocumentManager' does not contain a definition for 'DeleteDocumentAsync' and no accessible extension method 'DeleteDocumentAsync' accepting a first argument of type 'IDocumentManager' could be found
error CS1503: Argument 2: cannot convert from 'System.Security.Claims.ClaimsPrincipal' to 'int' (GetDocumentByIdAsync - wrong parameter order/type)
error CS1503: Argument 2: cannot convert from 'System.Security.Claims.ClaimsPrincipal' to 'int' (GetDocumentsByEntityAsync - wrong parameter order/type)
error CS1503: Argument 2: cannot convert from 'System.Security.Claims.ClaimsPrincipal' to 'int' (UpdateDocumentAsync - wrong parameter order/type)
```

### PartnerTreeEdgeCaseTests.cs

```
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerTreeAsync' and no accessible extension method 'GetPartnerTreeAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'AddChildPartnerAsync' and no accessible extension method 'AddChildPartnerAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerChildrenAsync' and no accessible extension method 'GetPartnerChildrenAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerParentAsync' and no accessible extension method 'GetPartnerParentAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerAncestorsAsync' and no accessible extension method 'GetPartnerAncestorsAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerDescendantsAsync' and no accessible extension method 'GetPartnerDescendantsAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'RemoveChildPartnerAsync' and no accessible extension method 'RemoveChildPartnerAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerLevelAsync' and no accessible extension method 'GetPartnerLevelAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerSiblingsAsync' and no accessible extension method 'GetPartnerSiblingsAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'MovePartnerAsync' and no accessible extension method 'MovePartnerAsync' accepting a first argument of type 'IPartnerManager' could be found
error CS1061: 'IPartnerManager' does not contain a definition for 'GetPartnerPathAsync' and no accessible extension method 'GetPartnerPathAsync' accepting a first argument of type 'IPartnerManager' could be found
```

---

## 5. Category D: Controller Endpoint Analysis

### NotificationControllerTests.cs

| Test Hits | Real Controller |
|-----------|-----------------|
| GET /api/notifications/999999 | ❌ No GET by ID — only GET /api/notifications (list) |
| POST /api/notifications/send | ❌ Not implemented |
| POST /api/notifications/send-bulk | ❌ Not implemented |
| GET /api/notifications/user/-1 | ❌ Not implemented |
| PUT /api/notifications/999999/read | ✅ PUT /api/notifications/{notificationId}/read exists |
| DELETE /api/notifications/999999 | ❌ Not implemented |
| PUT /api/notifications/user/-1/read-all | ❌ Not implemented |
| GET /api/notifications/count/unread | ❌ Not implemented |
| GET /api/notifications/preferences | ❌ Not implemented |
| PUT /api/notifications/preferences | ❌ Not implemented |

**Real NotificationController routes:**
- GET /api/notifications (list with optional unreadOnly filter)
- PUT /api/notifications/{notificationId}/read
- PUT /api/notifications/{notificationId}/update

---

### DashboardControllerTests.cs

| Test Hits | Real Controller |
|-----------|-----------------|
| GET /api/dashboard/summary | ❌ Not implemented |
| GET /api/dashboard/partners/stats | ❌ Not implemented |
| GET /api/dashboard/contacts/stats | ❌ Not implemented |
| GET /api/dashboard/interactions/stats | ❌ Not implemented |
| GET /api/dashboard/kpis | ❌ Not implemented |
| GET /api/dashboard/activity/recent | ❌ Not implemented |
| GET /api/dashboard/widgets/{name} | ❌ Not implemented |
| GET /api/dashboard/deadlines/upcoming | ❌ Not implemented |
| GET /api/dashboard/widgets/layout | ❌ Not implemented |
| POST /api/dashboard/widgets/layout | ❌ Not implemented |
| POST /api/dashboard/refresh | ❌ Not implemented |
| GET /api/dashboard/export/pdf | ❌ Not implemented |

**Real DashboardController routes:**
- GET /api/dashboard/my-partners
- GET /api/dashboard/my-contacts
- GET /api/dashboard/my-draft-partners
- GET /api/dashboard/my-draft-contacts
- GET /api/dashboard/my-interactions
- GET /api/dashboard/my-draft-interactions
- GET /api/dashboard/my-opportunities
- GET /api/dashboard/my-draft-opportunities
- GET /api/dashboard/org-unit-recent-updates
- GET /api/dashboard/content

**Models expected by tests (may not exist):** DashboardSummaryModel, PartnerStatisticsModel, ContactStatisticsModel, InteractionStatisticsModel, KPIMetricsModel, ActivityFeedModel, PaginatedResult<T>, TrendDataModel, BreakdownModel, PendingApprovalsModel, RecentItemModel, WidgetDataModel, WidgetListModel, WidgetLayoutModel, DeadlineItemModel.

---

### CommonEntitiesControllerTests.cs

| Test Hits | Real Controller |
|-----------|-----------------|
| GET /api/common/partner-statuses | ❌ CommonEntitiesController has **no action methods** — empty controller |
| GET /api/common/contact-statuses | ❌ |
| GET /api/common/interaction-types | ❌ |
| GET /api/common/document-types | ❌ |
| GET /api/common/workflow-statuses | ❌ |
| GET /api/common/statuses/{entityType} | ❌ |
| GET /api/common/partner-types | ❌ |
| GET /api/common/currencies | ❌ |
| GET /api/common/languages | ❌ |
| GET /api/common/timezones | ❌ |
| GET /api/common/countries | ❌ |
| GET /api/common/regions | ❌ |
| GET /api/common/date-formats | ❌ |
| GET /api/common/number-formats | ❌ |
| GET /api/common/all | ❌ |

**Real CommonEntitiesController:** Constructor only; no HTTP actions. All tests would receive 404.

---

## 6. Summary: Delta to Fix

### Category A (31 files)
- **Action:** Create and register new managers in IManagerWrapper/ManagerWrapper.
- **Managers needed:** DashboardManager, PartnerAnalyticsManager, ContactAnalyticsManager, OrganizationManager, UserProfileManager, PermissionManager, LiaisonOfficeManager. Custom RoleManager for application roles (distinct from ASP.NET Identity).

### Category B (19 files)
- **SystemAdmin:** Either implement the expected methods on ISystemAdminManager or rewrite tests to use RunMigrations, RunSeeding, etc.
- **PartnerTree:** Tests target partner hierarchy (parent/child). Either add hierarchy methods to IPartnerManager or create a dedicated IPartnerHierarchyManager. IPartnerTreeManager is for partner trees/categories, not hierarchy.
- **Documents:** Add UploadDocumentAsync and DeleteDocumentAsync to IDocumentManager with test-compatible signatures, or add adapter/wrapper. Align GetDocumentByIdAsync, GetDocumentsByEntityAsync, UpdateDocumentAsync to include user parameter if required.
- **UserManagement:** Align tests with ASP.NET Identity UserManager API or introduce IUserManagementManager that wraps it with the expected methods.
- **EntityConfiguration:** Expose EntityConfigurationManager on base IManagerWrapper (or ensure UNOPS tests use UNOPSManagerWrapper), and map CreateEntityConfigRequest/AddFieldRequest to CreateEntityConfigurationRequest/CreateEntityFieldRequest.

### Category C (1 file)
- **Action:** Add UNOPS.Workflow project references or move WorkflowControllerTests to the Workflow repo.

### Category D (7 files)
- **NotificationController:** Implement send, send-bulk, get-by-id, delete, user-specific, count, preferences endpoints, or rewrite tests to match current API.
- **DashboardController:** Implement summary, stats, widgets, activity, deadlines, layout, refresh, export endpoints, or rewrite tests to use my-partners, content, etc.
- **CommonEntitiesController:** Implement all /api/common/* actions.
- **EntityConfigurationController, ConfigurationController, ContactAnalyticsController, PartnerAnalyticsController:** Align routes and models with actual controllers.
