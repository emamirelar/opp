# .NET Backend Codebase Analysis & Restructuring Recommendations

**Date**: January 15, 2025  
**Project**: UNOPS Opportunity Plus Backend  
**Technology**: .NET 9.0, C#, ASP.NET Core, Entity Framework Core  
**Architecture**: Clean Architecture / Onion Architecture

---

## 📊 Executive Summary

This document provides a comprehensive analysis of the .NET backend codebase structure and identifies critical organizational issues that impact maintainability, scalability, and developer productivity. The backend follows a layered architecture pattern but suffers from several structural inconsistencies, duplicate layers, temporary files in the root, and minimal test coverage.

**Key Findings**:
- 🔴 **CRITICAL**: Minimal test coverage across the application (only integration tests)
- ❌ Duplicate architecture layers (PAO vs UNOPS prefixes)
- ❌ Temporary test files and scripts scattered in root directory
- ❌ Inconsistent naming conventions (Grants vs PAO)
- ❌ 262 migration files in UNOPSDataAccess (migration bloat)
- ❌ Massive Models project (107 files) lacking organization
- ❌ Mixed responsibilities in some projects
- ⚠️ CSV and SQL files mixed with production code

---

## 🔍 Current Structure Overview

### Solution Structure

```
UNOPS.PAO.sln
├── Core Projects (✅ Base Architecture)
│   ├── UNOPS.PAO.Domain          # Entities, Enums, Specifications
│   ├── UNOPS.PAO.DataAccess      # EF Core, DbContext, Migrations
│   ├── UNOPS.PAO.Business        # Business Logic, Managers, Services
│   ├── UNOPS.PAO.Presentation    # Controllers, API Layer
│   └── UNOPS.PAO.Server          # Web Host, Startup, Middleware
│
├── Supporting Projects
│   ├── UNOPS.PAO.Models          # DTOs, Request/Response Models
│   ├── UNOPS.PAO.Identity        # Authentication/Authorization
│   ├── UNOPS.PAO.Utilities       # Utility classes
│   ├── UNOPS.PAO.MailSender      # Email services
│   └── UNOPS.PAO.GoogleServices  # Google API integrations
│
├── Overrides Layer (⚠️ PROBLEM: Duplicate Architecture)
│   ├── UNOPS.PAO.UNOPSDomain     # Extended domain models
│   ├── UNOPS.PAO.UNOPSDataAccess # Extended data access (262 migrations!)
│   ├── UNOPS.PAO.UNOPSBusiness   # Extended business logic
│   ├── UNOPS.PAO.UNOPSIdentity   # Extended identity
│   └── UNOPS.PAO.UNOPSPresentation # Extended controllers
│
├── Testing (⚠️ PROBLEM: Minimal Coverage)
│   ├── UNOPS.PAO.IntegrationTests # Only integration tests exist
│   └── TestSpecification          # Test utility project
│
└── Root Clutter (❌ PROBLEM: Temporary Files)
    ├── test_*.py (5 files)        # Temporary Python test files
    ├── update_liaison_office_ids.py
    ├── generate_pubsub_embedding_messages.sql
    ├── Dockerfile.* (4 files)
    ├── Jenkinsfile* (2 files)
    ├── package-lock.json          # ❌ Why is this here?
    └── Multiple service folders
```

---

## 🚨 Critical Issues Identified

### 1. Duplicate Architecture Layers (PAO vs UNOPS)

**Issue**: The solution has two parallel architecture implementations

```
Core Architecture:              Override Architecture:
├── UNOPS.PAO.Domain           ├── UNOPS.PAO.UNOPSDomain
├── UNOPS.PAO.DataAccess       ├── UNOPS.PAO.UNOPSDataAccess (262 migrations!)
├── UNOPS.PAO.Business         ├── UNOPS.PAO.UNOPSBusiness
├── UNOPS.PAO.Identity         ├── UNOPS.PAO.UNOPSIdentity
└── UNOPS.PAO.Presentation     └── UNOPS.PAO.UNOPSPresentation
```

**Problems**:
- Confusing for developers - which layer to use?
- Code duplication and maintenance overhead
- Unclear boundaries and responsibilities
- "Overrides" folder name doesn't explain purpose
- Double the migration files to manage
- Increases build time unnecessarily

**Impact**: 🔴 High - Architectural confusion, technical debt

**Root Cause**: Likely created to extend base functionality for UNOPS-specific features, but naming and organization are poor.

---

### 2. Migration File Bloat

**Issue**: 262 migration files in `UNOPS.PAO.UNOPSDataAccess/Migrations/`

```
UNOPSDataAccess/Migrations/
├── 20250113103619_Init.cs
├── 20250131102141_DocumentRelationships.cs
├── 20250131112130_SimplerDocumentRelationships.cs
├── 20250131122458_RethinkDocumentRelationships.cs
├── ... (258 more migration files)
└── AppDb/ (more migrations)
```

**Problems**:
- Extremely slow migration discovery
- Hard to understand database evolution
- Likely contains redundant/reverted changes
- Makes rollbacks complex
- Increases repository size
- Developer confusion about current schema state

**Impact**: 🔴 High - Performance, maintainability, onboarding

**Solution**: Squash migrations periodically (see Migration Plan section)

---

### 3. Temporary Files in Root Directory

**Issue**: Multiple temporary files cluttering the root

```
Root Directory:
├── test_corrected_scoring.py          # ❌ Temporary test file
├── test_endpoint_selection.py         # ❌ Temporary test file
├── test_new_scoring_format.py         # ❌ Temporary test file
├── test_perfect_scoring.py            # ❌ Temporary test file
├── test_scoring_standalone.py         # ❌ Temporary test file
├── update_liaison_office_ids.py       # ❌ One-off script
├── generate_pubsub_embedding_messages.sql # ❌ Script file
└── package-lock.json                  # ❌ Wrong location
```

**Problems**:
- Unprofessional repository appearance
- Confuses developers about what's production code
- May contain outdated logic or test code
- Pollutes root directory
- Not version controlled properly
- Hard to know what's safe to delete

**Impact**: 🟡 Medium - Code organization, professionalism

**Solution**: 
- Move to `Scripts/Temp/` or `Tools/Deprecated/`
- Or delete if no longer needed
- Add to `.gitignore` if truly temporary

---

### 4. Oversized Models Project

**Issue**: `UNOPS.PAO.Models` contains 107 model files in flat structure

```
UNOPS.PAO.Models/ (107 files!)
├── AiChatRequest.cs
├── AiChatSessionModel.cs
├── AiPromptModel.cs
├── ContactModel.cs
├── ContactRequest.cs
├── DocumentModel.cs
├── InteractionModel.cs
├── PartnerModel.cs
├── ... (99 more files in flat structure)
└── Workflow/
    └── (6 workflow files)
```

**Problems**:
- Nearly 107 files in one directory
- Hard to find specific models
- No logical grouping by feature/domain
- Mix of DTOs, requests, responses, and view models
- Unclear naming patterns
- Long build times for project

**Impact**: 🟡 Medium - Developer productivity, navigation

**Solution**: Organize by feature area (see Models Reorganization section)

---

### 5. Missing Unit Tests

**Issue**: No unit test projects - only integration tests

```
Current Test Structure:
└── UNOPS.PAO.IntegrationTests/
    ├── Controllers/            (API integration tests)
    ├── UnitTests/              (⚠️ Misleading name - still integration)
    ├── Infrastructure/
    └── TestData/

Missing:
├── UNOPS.PAO.Domain.Tests      ❌ Should test specifications, entities
├── UNOPS.PAO.Business.Tests    ❌ Should test managers, services
├── UNOPS.PAO.Utilities.Tests   ❌ Should test utility classes
└── UNOPS.PAO.Presentation.Tests ❌ Should test controllers (unit)
```

**Problems**:
- **No fast feedback loop** - Integration tests are slow (minutes)
- **Can't test business logic in isolation** - Always need database
- **Hard to debug failures** - Integration tests involve many layers
- **No code coverage metrics** - Can't measure test quality
- **Difficult to practice TDD** - Integration tests are too heavy
- **Brittle tests** - Changes to DB schema break everything
- **Can't test edge cases easily** - Need complex test data setup

**Impact**: 🔴 **CRITICAL** - Quality, velocity, confidence

**What Should Exist**:
```
Recommended Test Structure:
├── Unit Tests (Fast - milliseconds)
│   ├── UNOPS.PAO.Domain.Tests
│   │   ├── Entities/
│   │   ├── Specifications/
│   │   └── Enums/
│   ├── UNOPS.PAO.Business.Tests
│   │   ├── Managers/
│   │   ├── Services/
│   │   └── Extensions/
│   └── UNOPS.PAO.Presentation.Tests
│       └── Controllers/
│
└── Integration Tests (Slow - seconds)
    └── UNOPS.PAO.IntegrationTests
        ├── API/
        └── Database/
```

---

### 6. Inconsistent Naming Conventions

**Issue**: Mix of naming patterns across the solution

**Examples**:

```csharp
// ❌ Inconsistent Project Names
UNOPS.PAO.Business     vs    UNOPS.PAO.UNOPSBusiness
UNOPS.Grants.Business  vs    UNOPS.PAO.Business

// ❌ Inconsistent File Names in bin/
UNOPS.Grants.Business.deps.json
UNOPS.Grants.Business.dll
UNOPS.PAO.Business.deps.json
UNOPS.PAO.Business.dll

// ❌ "Grants" vs "PAO" confusion
// Some projects have both naming patterns
```

**Problems**:
- Developers confused about naming standards
- Hard to search/find files
- Suggests incomplete refactoring
- Mixed concerns (Grants vs PAO)
- Professional code should be consistent

**Impact**: 🟡 Medium - Developer experience, maintainability

**Root Cause**: Likely rebranding from "Grants" to "PAO" was incomplete

---

### 7. Scripts and Data Files Mixed with Code

**Issue**: SQL scripts, CSV files, and Python scripts scattered in DataAccess projects

```
UNOPS.PAO.UNOPSDataAccess/
├── Scripts/
│   ├── 001_organization_units_insert.sql
│   ├── 001_partner_categories_insert.sql
│   ├── CSV/
│   │   ├── Contacts_20251002.csv
│   │   ├── Events_20251002.csv
│   │   ├── Partner tree export.csv
│   │   └── ... (10+ CSV files)
│   ├── Python/
│   │   ├── partner_seeder_v2.py
│   │   ├── contact_seeder.py
│   │   └── ... (11 Python files)
│   └── 2025-08-27/          # ❌ Date-based folders
│       └── ... (many files)
```

**Problems**:
- CSV files in source control (should be in data repos or external)
- Python scripts in .NET project (should be separate)
- Date-based folders suggest ad-hoc organization
- Data seeding mixed with application code
- Hard to know what's currently used vs obsolete

**Impact**: 🟡 Medium - Repository size, organization

**Solution**: Move to dedicated `/Database` or `/Tools` folders

---

### 8. Unclear Project Purposes

**Issue**: Some projects have ambiguous or overlapping purposes

#### `TestSpecification` Project
```
TestSpecification/
├── Program.cs
└── TestSpecification.csproj
```
- ❌ Only 2 files
- ❌ Unclear purpose (not a test project?)
- ❌ Empty or minimal implementation

#### `UNOPS.PAO.Web` Project
```
UNOPS.PAO.Web/
└── 2 TypeScript files
```
- ❌ Only 2 TypeScript files
- ❌ Why is this separate from ClientApp?
- ❌ Seems incomplete or abandoned

#### `ExternalDataService/` and `AIService/` Folders
- Located at solution root
- Not .csproj projects
- Contain Python code (FastAPI)
- Should be separate repositories or solution folders

**Impact**: 🟢 Low - Works but architecturally confusing

---

### 9. Managers vs Services Inconsistency

**Issue**: Unclear distinction between Managers and Services

```
UNOPS.PAO.Business/
├── Managers/               (18 files)
│   ├── ContactManager.cs
│   ├── PartnerManager.cs
│   └── InteractionManager.cs
├── Services/               (6 files)
│   ├── CountryService.cs
│   ├── SavedFilterService.cs
│   └── OrganizationHierarchyService.cs
└── Repositories/           (4 files)
    └── ValuesRepository.cs

UNOPS.PAO.UNOPSBusiness/
├── Managers/               (18 files)
│   ├── UNOPSContactManager.cs
│   ├── UNOPSPartnerManager.cs
│   └── BaseEngagementManager.cs
└── Services/               (18 files)
    ├── DashboardService.cs
    ├── PermissionService.cs
    └── AdvancedSearchService.cs
```

**Problems**:
- No clear rule for Manager vs Service
- Both seem to contain business logic
- Developers don't know which to use
- Possible duplication of concepts
- Inconsistent between PAO and UNOPS projects

**Common Patterns**:
- **Managers**: Typically orchestrate business operations, interact with repositories
- **Services**: Typically provide specific functionality, utilities, or integrations
- **Repositories**: Data access abstraction

**Impact**: 🟡 Medium - Confusion, inconsistency

**Recommendation**: 
- **Managers** = Aggregate root operations (Contact, Partner, Interaction)
- **Services** = Cross-cutting concerns or infrastructure (Search, Permissions, Caching)
- **Repositories** = Data access only

---

### 10. Presentation Layer Organization

**Issue**: Controllers are flat - no organization by feature

```
UNOPS.PAO.Presentation/Controllers/ (35 controllers!)
├── ContactController.cs
├── PartnerController.cs
├── InteractionController.cs
├── DocumentController.cs
├── LinkController.cs
├── GeminiController.cs
├── ... (29 more controllers)
└── (No folder organization)
```

**Problems**:
- 35+ controllers in one directory
- Hard to find specific controller
- No logical grouping
- All controllers exposed at project level
- Difficult to navigate

**Impact**: 🟢 Low - Works but harder to navigate

**Solution**: Organize by feature area (see Presentation Reorganization section)

---

## ✅ What's Working Well

### Good Architectural Decisions

✅ **Clean Architecture Pattern**
- Clear separation of concerns
- Domain at center, dependencies point inward
- Infrastructure isolated

✅ **Specification Pattern**
- Excellent use of specifications for queries
- Reusable query logic
- `ContactSpecifications/`, `PartnerSpecifications/`, `InteractionSpecifications/`

✅ **AutoMapper Integration**
- Mapping profiles organized by feature
- `MappingProfile.cs` in multiple projects

✅ **Integration Tests Exist**
- Good starting point for testing
- Tests for authorization, RBAC, controllers
- Uses `WebApplicationFactory` pattern

✅ **Role-Based Access Control (RBAC)**
- Custom authorization handlers
- Permission-based authorization
- Context-aware permissions

✅ **Database Seed Infrastructure**
- `GenericSeedRunner.cs`
- Seed scripts organized
- Configuration-driven seeding

✅ **API Documentation**
- Swagger/OpenAPI configured
- Controller documentation
- Helper classes for API organization

✅ **Entity Framework Core**
- Code-first migrations
- Proper DbContext setup
- Multi-tenant schema support

---

## 📋 Recommended Solution Structure

### Complete Reorganized Structure

```
UNOPS.PAO.sln
│
├── src/
│   ├── Core/                           # Core business logic (no dependencies)
│   │   ├── UNOPS.PAO.Domain
│   │   │   ├── Entities/
│   │   │   │   ├── Partners/
│   │   │   │   │   ├── Partner.cs
│   │   │   │   │   ├── PartnerCategory.cs
│   │   │   │   │   └── PartnerGroup.cs
│   │   │   │   ├── Contacts/
│   │   │   │   │   ├── Contact.cs
│   │   │   │   │   └── ContactValue.cs
│   │   │   │   ├── Interactions/
│   │   │   │   │   ├── Interaction.cs
│   │   │   │   │   ├── InteractionContact.cs
│   │   │   │   │   ├── InteractionPartner.cs
│   │   │   │   │   └── InteractionUser.cs
│   │   │   │   ├── Documents/
│   │   │   │   │   ├── Document.cs
│   │   │   │   │   ├── DocumentType.cs
│   │   │   │   │   └── DocumentRelationship.cs
│   │   │   │   ├── OrganizationUnits/
│   │   │   │   │   ├── OrganizationHierarchy.cs
│   │   │   │   │   ├── LiaisonOffice.cs
│   │   │   │   │   └── OrganizationUnitRelationship.cs
│   │   │   │   ├── Identity/
│   │   │   │   │   ├── PAOUser.cs
│   │   │   │   │   ├── UserProfile.cs
│   │   │   │   │   └── UserPreference.cs
│   │   │   │   ├── Shared/
│   │   │   │   │   ├── BaseBusinessEntity.cs
│   │   │   │   │   ├── Country.cs
│   │   │   │   │   ├── Currency.cs
│   │   │   │   │   └── Link.cs
│   │   │   │   └── AI/
│   │   │   │       ├── AiChatSession.cs
│   │   │   │       ├── AiPrompt.cs
│   │   │   │       └── EntityEmbeddings.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   ├── EntityStatus.cs
│   │   │   │   ├── InteractionType.cs
│   │   │   │   ├── PartnerApprovalStatus.cs
│   │   │   │   ├── NotificationType.cs
│   │   │   │   └── ... (other enums)
│   │   │   │
│   │   │   ├── Specifications/
│   │   │   │   ├── Base/
│   │   │   │   │   ├── ISpecification.cs
│   │   │   │   │   ├── BaseSpecification.cs
│   │   │   │   │   └── BaseCompositeSpecification.cs
│   │   │   │   ├── ContactSpecifications/
│   │   │   │   │   ├── ContactByOrgUnitHierarchySpec.cs
│   │   │   │   │   └── ContactCompositeSpec.cs
│   │   │   │   ├── PartnerSpecifications/
│   │   │   │   │   ├── PartnerByStatusSpec.cs
│   │   │   │   │   ├── PartnerByOrgUnitSpec.cs
│   │   │   │   │   └── ... (15 specifications)
│   │   │   │   └── InteractionSpecifications/
│   │   │   │       ├── InteractionByDateRangeSpec.cs
│   │   │   │       ├── InteractionByTypeSpec.cs
│   │   │   │       └── ... (other specs)
│   │   │   │
│   │   │   ├── DTOs/
│   │   │   │   ├── AdvancedSearchDTO.cs
│   │   │   │   └── OrganizationHierarchyTreeDto.cs
│   │   │   │
│   │   │   └── Interfaces/
│   │   │       ├── IDeletable.cs
│   │   │       └── IStatusEntity.cs
│   │   │
│   │   └── UNOPS.PAO.Domain.Tests/            # ⬅️ UNIT TESTS
│   │       ├── Entities/
│   │       │   ├── PartnerTests.cs
│   │       │   ├── ContactTests.cs
│   │       │   └── InteractionTests.cs
│   │       ├── Specifications/
│   │       │   ├── ContactSpecificationTests.cs
│   │       │   ├── PartnerSpecificationTests.cs
│   │       │   └── InteractionSpecificationTests.cs
│   │       └── Enums/
│   │           └── EnumTests.cs
│   │
│   ├── Application/                    # Application business logic
│   │   ├── UNOPS.PAO.Business
│   │   │   ├── Interfaces/
│   │   │   │   ├── Partners/
│   │   │   │   │   ├── IPartnerManager.cs
│   │   │   │   │   └── IPartnerTreeManager.cs
│   │   │   │   ├── Contacts/
│   │   │   │   │   └── IContactManager.cs
│   │   │   │   ├── Interactions/
│   │   │   │   │   └── IInteractionManager.cs
│   │   │   │   ├── Documents/
│   │   │   │   │   ├── IDocumentManager.cs
│   │   │   │   │   └── IDocumentTypeManager.cs
│   │   │   │   ├── Admin/
│   │   │   │   │   ├── ISystemAdminManager.cs
│   │   │   │   │   └── IUserManagementManager.cs
│   │   │   │   ├── AI/
│   │   │   │   │   ├── IGeminiManager.cs
│   │   │   │   │   └── IAiPromptManager.cs
│   │   │   │   └── Integrations/
│   │   │   │       └── IGmailAddonManager.cs
│   │   │   │
│   │   │   ├── Managers/               # Aggregate root operations
│   │   │   │   ├── Partners/
│   │   │   │   │   ├── PartnerManager.cs
│   │   │   │   │   └── PartnerTreeManager.cs
│   │   │   │   ├── Contacts/
│   │   │   │   │   └── ContactManager.cs
│   │   │   │   ├── Interactions/
│   │   │   │   │   └── InteractionManager.cs
│   │   │   │   ├── Documents/
│   │   │   │   │   ├── DocumentManager.cs
│   │   │   │   │   └── DocumentTypeManager.cs
│   │   │   │   ├── Admin/
│   │   │   │   │   ├── SystemAdminManager.cs
│   │   │   │   │   ├── UserDataManager.cs
│   │   │   │   │   └── ProfileManager.cs
│   │   │   │   ├── AI/
│   │   │   │   │   ├── GeminiManager.cs
│   │   │   │   │   └── NotificationManager.cs
│   │   │   │   ├── Integrations/
│   │   │   │   │   └── GmailAddonManager.cs
│   │   │   │   └── Shared/
│   │   │   │       ├── ManagerWrapper.cs
│   │   │   │       └── WorkflowManager.cs
│   │   │   │
│   │   │   ├── Services/               # Cross-cutting services
│   │   │   │   ├── OrganizationUnits/
│   │   │   │   │   ├── OrganizationHierarchyService.cs
│   │   │   │   │   ├── OrganizationHierarchyLookupService.cs
│   │   │   │   │   ├── LiaisonOfficeService.cs
│   │   │   │   │   └── LiaisonOfficeLookupService.cs
│   │   │   │   ├── Location/
│   │   │   │   │   └── CountryService.cs
│   │   │   │   └── Filters/
│   │   │   │       └── SavedFilterService.cs
│   │   │   │
│   │   │   ├── Extensions/
│   │   │   │   ├── ContactExtensions.cs
│   │   │   │   ├── PartnerExtensions.cs
│   │   │   │   └── InteractionExtensions.cs
│   │   │   │
│   │   │   ├── Mapping/
│   │   │   │   ├── MappingProfile.cs
│   │   │   │   ├── CountryMappingProfile.cs
│   │   │   │   ├── LiaisonOfficeMappingProfile.cs
│   │   │   │   ├── OrganizationHierarchyMappingProfile.cs
│   │   │   │   ├── PartnerCategoryMappingProfile.cs
│   │   │   │   └── SavedFilterMappingProfile.cs
│   │   │   │
│   │   │   └── EmailTemplates/
│   │   │       └── DueDiligenceExpiryNotification.html
│   │   │
│   │   └── UNOPS.PAO.Business.Tests/          # ⬅️ UNIT TESTS
│   │       ├── Managers/
│   │       │   ├── ContactManagerTests.cs
│   │       │   ├── PartnerManagerTests.cs
│   │       │   └── InteractionManagerTests.cs
│   │       ├── Services/
│   │       │   ├── OrganizationHierarchyServiceTests.cs
│   │       │   └── SavedFilterServiceTests.cs
│   │       └── Extensions/
│   │           └── ContactExtensionsTests.cs
│   │
│   ├── Infrastructure/                 # External concerns
│   │   ├── UNOPS.PAO.DataAccess
│   │   │   ├── Context/
│   │   │   │   ├── AppDbContext.cs
│   │   │   │   ├── PAOIdentityDbContext.cs
│   │   │   │   ├── AuditableDbContext.cs
│   │   │   │   ├── DbContextSchema.cs
│   │   │   │   └── DbSchemaAwareModelCacheKeyFactory.cs
│   │   │   │
│   │   │   ├── Migrations/             # ⬅️ Keep only recent
│   │   │   │   ├── AppDb/
│   │   │   │   │   ├── <Recent migration files>
│   │   │   │   │   └── AppDbContextModelSnapshot.cs
│   │   │   │   └── PAOIdentityDbContextModelSnapshot.cs
│   │   │   │
│   │   │   ├── Configuration/          # EF Core entity configurations
│   │   │   │   ├── PartnerConfiguration.cs
│   │   │   │   ├── ContactConfiguration.cs
│   │   │   │   └── ... (fluent API configs)
│   │   │   │
│   │   │   ├── Repositories/           # Optional: Generic repositories
│   │   │   │   └── ... (if using repository pattern)
│   │   │   │
│   │   │   └── Services/
│   │   │       ├── UserInfoService.cs
│   │   │       └── UserResolverService.cs
│   │   │
│   │   ├── UNOPS.PAO.Identity
│   │   │   ├── Models/
│   │   │   │   └── ... (Identity models)
│   │   │   └── Services/
│   │   │       └── ... (Identity services)
│   │   │
│   │   ├── UNOPS.PAO.GoogleServices
│   │   │   ├── Services/
│   │   │   │   ├── GoogleDriveService.cs
│   │   │   │   ├── GoogleStorageService.cs
│   │   │   │   └── GoogleTextToSpeechService.cs
│   │   │   └── ... (Google integrations)
│   │   │
│   │   ├── UNOPS.PAO.MailSender
│   │   │   └── ... (Email infrastructure)
│   │   │
│   │   └── UNOPS.PAO.Utilities
│   │       └── ... (Shared utilities)
│   │
│   ├── Presentation/                   # API Layer
│   │   ├── UNOPS.PAO.Presentation
│   │   │   ├── Controllers/
│   │   │   │   ├── Partners/
│   │   │   │   │   ├── PartnerController.cs
│   │   │   │   │   ├── PartnerTreeController.cs
│   │   │   │   │   ├── PartnerCategoryController.cs
│   │   │   │   │   ├── PartnerGroupController.cs
│   │   │   │   │   └── PartnerAnalyticsController.cs
│   │   │   │   ├── Contacts/
│   │   │   │   │   ├── ContactController.cs
│   │   │   │   │   └── ContactAnalyticsController.cs
│   │   │   │   ├── Interactions/
│   │   │   │   │   └── InteractionController.cs
│   │   │   │   ├── Documents/
│   │   │   │   │   ├── DocumentController.cs
│   │   │   │   │   ├── DocumentTypeController.cs
│   │   │   │   │   └── LinkController.cs
│   │   │   │   ├── OrganizationUnits/
│   │   │   │   │   ├── OrganizationHierarchyController.cs
│   │   │   │   │   ├── OrganizationHierarchyLookupController.cs
│   │   │   │   │   ├── LiaisonOfficeController.cs
│   │   │   │   │   └── LiaisonOfficeLookupController.cs
│   │   │   │   ├── Admin/
│   │   │   │   │   ├── SystemAdminController.cs
│   │   │   │   │   ├── UserManagementController.cs
│   │   │   │   │   ├── PermissionController.cs
│   │   │   │   │   └── EntityConfigurationController.cs
│   │   │   │   ├── AI/
│   │   │   │   │   ├── GeminiController.cs
│   │   │   │   │   └── NotificationController.cs
│   │   │   │   ├── Integrations/
│   │   │   │   │   └── GmailAddonController.cs
│   │   │   │   ├── Location/
│   │   │   │   │   └── CountryController.cs
│   │   │   │   ├── Dashboard/
│   │   │   │   │   └── DashboardController.cs
│   │   │   │   ├── User/
│   │   │   │   │   ├── UserProfileController.cs
│   │   │   │   │   └── UserPreferenceController.cs
│   │   │   │   └── Shared/
│   │   │   │       ├── BaseController.cs
│   │   │   │       ├── ConfigurationController.cs
│   │   │   │       ├── GlobalController.cs
│   │   │   │       ├── SavedFilterController.cs
│   │   │   │       └── ValuesController.cs
│   │   │   │
│   │   │   ├── Filters/
│   │   │   │   └── ValidateModelStateAttribute.cs
│   │   │   │
│   │   │   ├── Security/
│   │   │   │   ├── PermissionAuthorizeAttribute.cs
│   │   │   │   ├── Operations.cs
│   │   │   │   └── EntityPermissionHelper.cs
│   │   │   │
│   │   │   ├── ContextPermissionHandlers/
│   │   │   │   ├── IAuthorizationHandlerWrapper.cs
│   │   │   │   ├── AuthorizationHandlerWrapper.cs
│   │   │   │   ├── ContactAuthorizationHandler.cs
│   │   │   │   ├── PartnerTreeAuthorizationHandler.cs
│   │   │   │   └── ProfileAuthorizationHandler.cs
│   │   │   │
│   │   │   └── Helpers/
│   │   │       ├── APIDictionary.cs
│   │   │       ├── EntityTypes.cs
│   │   │       ├── AdvancedSearchHelper.cs
│   │   │       ├── SearchControllerHelper.cs
│   │   │       └── SecureSearchControllerHelper.cs
│   │   │
│   │   └── UNOPS.PAO.Presentation.Tests/      # ⬅️ UNIT TESTS
│   │       └── Controllers/
│   │           ├── PartnerControllerTests.cs
│   │           ├── ContactControllerTests.cs
│   │           └── InteractionControllerTests.cs
│   │
│   ├── Models/                         # DTOs, Requests, Responses
│   │   └── UNOPS.PAO.Models
│   │       ├── Partners/
│   │       │   ├── PartnerModel.cs
│   │       │   ├── PartnerRequest.cs
│   │       │   ├── PartnerFilterRequest.cs
│   │       │   ├── UpdatePartnerRequest.cs
│   │       │   ├── PartnerValueModel.cs
│   │       │   ├── PartnerTreeModel.cs
│   │       │   ├── PartnerTreeRequest.cs
│   │       │   ├── UpdatePartnerTreeRequest.cs
│   │       │   ├── ExternalPartnerTreeModel.cs
│   │       │   ├── PartnerCategoryModel.cs
│   │       │   └── PartnerGroupModel.cs
│   │       │
│   │       ├── Contacts/
│   │       │   ├── ContactModel.cs
│   │       │   ├── ContactRequest.cs
│   │       │   ├── ContactFilterRequest.cs
│   │       │   ├── UpdateContactRequest.cs
│   │       │   ├── ContactValueModel.cs
│   │       │   ├── ExternalContactModel.cs
│   │       │   └── GmailCreateContactsRequest.cs
│   │       │
│   │       ├── Interactions/
│   │       │   ├── InteractionModel.cs
│   │       │   ├── InteractionRequest.cs
│   │       │   ├── InteractionFilterRequest.cs
│   │       │   ├── UpdateInteractionRequest.cs
│   │       │   ├── ExternalInteractionModel.cs
│   │       │   └── GmailInteractionRequest.cs
│   │       │
│   │       ├── Documents/
│   │       │   ├── DocumentModel.cs
│   │       │   ├── DocumentBaseModel.cs
│   │       │   ├── DocumentBaseCreateModel.cs
│   │       │   ├── DocumentUploadModel.cs
│   │       │   ├── UpdateDocumentRequest.cs
│   │       │   ├── DocumentLinkModel.cs
│   │       │   ├── DocumentTypeModel.cs
│   │       │   ├── DocumentTypeRequestParameters.cs
│   │       │   └── LinkModels.cs
│   │       │
│   │       ├── OrganizationUnits/
│   │       │   ├── OrganizationHierarchyModel.cs
│   │       │   ├── OrganizationHierarchyLookupModel.cs
│   │       │   ├── OrganizationHierarchyPrimeModel.cs
│   │       │   ├── OrganizationUnitModel.cs
│   │       │   ├── OrganizationUnitRelationshipModel.cs
│   │       │   ├── OrgUnitRecentUpdatesResponse.cs
│   │       │   ├── LiaisonOfficeModel.cs
│   │       │   └── LiaisonOfficeLookupModel.cs
│   │       │
│   │       ├── AI/
│   │       │   ├── AiChatRequest.cs
│   │       │   ├── AiChatSessionModel.cs
│   │       │   ├── AiPromptModel.cs
│   │       │   ├── AiPromptResponseModel.cs
│   │       │   ├── GeminiAssistantRequest.cs
│   │       │   ├── GeminiSessionRequest.cs
│   │       │   ├── GeminiUserSessionsRequest.cs
│   │       │   ├── GeminiAccessibilityRequest.cs
│   │       │   ├── GeminiFileRequest.cs
│   │       │   ├── GeminiProcessDataRequest.cs
│   │       │   ├── SessionResponse.cs
│   │       │   ├── SessionUpdateRequest.cs
│   │       │   ├── SessionWithChats.cs
│   │       │   ├── TestPromptRequest.cs
│   │       │   ├── TestPromptResponse.cs
│   │       │   ├── AnalyseFileRequest.cs
│   │       │   ├── EntityDetectionResult.cs
│   │       │   └── EntityEmbeddingsModel.cs
│   │       │
│   │       ├── Integrations/
│   │       │   ├── GmailRelatedRecordsRequest.cs
│   │       │   ├── GmailRelatedRecordsResponse.cs
│   │       │   ├── GmailCreateRecordsResult.cs
│   │       │   └── GenerateGoogleDocRequest.cs
│   │       │
│   │       ├── Admin/
│   │       │   ├── UserManagementModels.cs
│   │       │   ├── UsersPagedRequest.cs
│   │       │   └── EntityConfiguration/
│   │       │       └── CreateEntityConfigurationRequest.cs
│   │       │
│   │       ├── Location/
│   │       │   ├── CountryModel.cs
│   │       │   └── CurrencyModel.cs
│   │       │
│   │       ├── Notifications/
│   │       │   ├── NotificationModel.cs
│   │       │   └── NotificationFilterModel.cs
│   │       │
│   │       ├── User/
│   │       │   ├── PAOUserModel.cs
│   │       │   ├── ProfileModel.cs
│   │       │   └── UserValueModel.cs
│   │       │
│   │       ├── Search/
│   │       │   ├── GlobalSearchModels.cs
│   │       │   ├── SearchCriteria.cs
│   │       │   ├── SearchFilter.cs
│   │       │   └── SearchFieldInfo.cs
│   │       │
│   │       ├── Filters/
│   │       │   ├── SavedFilterModels.cs
│   │       │   └── TypeaheadInput.cs
│   │       │
│   │       ├── Shared/
│   │       │   ├── BaseEngagementModel.cs
│   │       │   ├── PaginationRequest.cs
│   │       │   ├── PaginationResponse.cs
│   │       │   ├── RequestParameters.cs
│   │       │   ├── SpecificationPaginationRequest.cs
│   │       │   ├── ConfigurationResponse.cs
│   │       │   ├── EntityTagModel.cs
│   │       │   ├── EntityPermissionsModel.cs
│   │       │   ├── EligibleEntityModel.cs
│   │       │   ├── ExtensibleModel.cs
│   │       │   ├── ErrorDetails.cs
│   │       │   ├── RecentUpdateModel.cs
│   │       │   ├── BulkUploadRequest.cs
│   │       │   ├── ApplicationTypeModel.cs
│   │       │   └── ApplicantModel.cs
│   │       │
│   │       ├── Workflow/
│   │       │   ├── StateMachine.cs
│   │       │   ├── State.cs
│   │       │   ├── StateAction.cs
│   │       │   ├── Facing.cs
│   │       │   ├── WorkflowStateModel.cs
│   │       │   ├── WorkflowStageModel.cs
│   │       │   └── WorkflowActionModel.cs
│   │       │
│   │       ├── Audit/
│   │       │   ├── IModifiableEntityModel.cs
│   │       │   └── ModifiableEntityModel.cs
│   │       │
│   │       └── Converters/
│   │           └── StringOrStringArrayConverter.cs
│   │
│   └── WebHost/                        # Entry point
│       └── UNOPS.PAO.Server
│           ├── Program.cs
│           ├── Startup.cs
│           ├── appsettings.json
│           ├── appsettings.Development.json
│           ├── appsettings.Production.json
│           ├── appsettings.QA.json
│           ├── appsettings.Test.json
│           │
│           ├── Infrastructure/
│           │   ├── GlobalExceptionHandler.cs
│           │   ├── AuthenticationLoggingMiddleware.cs
│           │   ├── DevelopmentLoginPageMiddleware.cs
│           │   └── Security/
│           │       ├── PAOAuthorizationService.cs
│           │       ├── PermissionHandler.cs
│           │       ├── PermissionRequirement.cs
│           │       ├── PermissionPolicyProvider.cs
│           │       └── PermissionAuthorizeAttribute.cs
│           │
│           └── Middleware/
│               └── ValidationMiddleware.cs
│
├── tests/                              # ⬅️ All tests organized here
│   ├── Unit/
│   │   ├── UNOPS.PAO.Domain.Tests/
│   │   ├── UNOPS.PAO.Business.Tests/
│   │   ├── UNOPS.PAO.Presentation.Tests/
│   │   └── UNOPS.PAO.Utilities.Tests/
│   │
│   ├── Integration/
│   │   └── UNOPS.PAO.IntegrationTests/
│   │       ├── API/
│   │       │   ├── Controllers/
│   │       │   │   ├── PartnerControllerTests.cs
│   │       │   │   ├── ContactControllerTests.cs
│   │       │   │   └── InteractionControllerTests.cs
│   │       │   └── Authorization/
│   │       │       ├── PartnerControllerOrgUnitTests.cs
│   │       │       ├── ContactControllerOrgUnitTests.cs
│   │       │       └── InteractionControllerOrgUnitTests.cs
│   │       │
│   │       ├── Database/
│   │       │   ├── PartnerRepositoryTests.cs
│   │       │   └── ContactRepositoryTests.cs
│   │       │
│   │       ├── Infrastructure/
│   │       │   ├── IntegrationTestBase.cs
│   │       │   ├── PAOWebApplicationFactory.cs
│   │       │   ├── TestAuthHandler.cs
│   │       │   ├── TestOrgUnitHierarchyService.cs
│   │       │   └── TestPermissionService.cs
│   │       │
│   │       └── TestData/
│   │           ├── TestDataBuilder.cs
│   │           └── TestDataSeeder.cs
│   │
│   └── E2E/                            # Optional: End-to-end API tests
│       └── UNOPS.PAO.E2ETests/
│           └── ... (API workflow tests)
│
├── database/                           # ⬅️ Database-related files
│   ├── Migrations/
│   │   └── ... (migration scripts for reference)
│   │
│   ├── Seed/
│   │   ├── Scripts/
│   │   │   ├── AiPrompts.sql
│   │   │   ├── AspNetUsers.sql
│   │   │   ├── EntityManager.sql
│   │   │   ├── EntityPermissions.sql
│   │   │   ├── OrganizationUnits.sql
│   │   │   └── UserProfiles.sql
│   │   │
│   │   ├── Data/                       # ⬅️ CSV data files
│   │   │   ├── Contacts_Import.csv
│   │   │   ├── Partners_Import.csv
│   │   │   ├── Interactions_Import.csv
│   │   │   └── OrgUnits_Import.csv
│   │   │
│   │   └── Seeders/                    # C# seeder classes
│   │       ├── GenericSeedRunner.cs
│   │       ├── SeedExtensions.cs
│   │       └── ... (seeder classes)
│   │
│   └── Scripts/                        # ⬅️ Ad-hoc SQL scripts
│       ├── Analysis/
│       │   ├── Partner_Interactions_Summary.sql
│       │   ├── Search_Records.sql
│       │   └── Detect_Duplicate_Records.sql
│       │
│       ├── Maintenance/
│       │   ├── fix-gmail-propertyfilter.sql
│       │   └── remove-orgunit-filters.sql
│       │
│       └── Setup/
│           ├── seed-entities.sql
│           ├── seed-roles.sql
│           └── seed-liaison-offices.sql
│
├── tools/                              # ⬅️ Development tools and utilities
│   ├── DataImport/
│   │   └── Python/                     # Python data import scripts
│   │       ├── partner_seeder_v2.py
│   │       ├── contact_seeder.py
│   │       ├── interaction_seeder.py
│   │       └── ... (other import scripts)
│   │
│   ├── ToolsJsonGenerator/             # Frontend tool generation
│   │   └── ... (existing tools)
│   │
│   └── Deprecated/                     # ⬅️ Move old scripts here
│       ├── test_corrected_scoring.py
│       ├── test_endpoint_selection.py
│       └── ... (temporary test files)
│
├── docs/                               # ⬅️ Documentation
│   ├── Architecture/
│   │   ├── BACKEND_CODEBASE_ANALYSIS.md
│   │   ├── BACKEND_TESTING_GUIDE.md
│   │   └── CleanArchitecture.md
│   │
│   ├── Security/
│   │   ├── IAP-Authentication-Guide.md
│   │   ├── Role-Based-Access-Control.md
│   │   └── SecurityMeasures.md
│   │
│   ├── Development/
│   │   ├── GettingStarted.md
│   │   ├── CodingStandards.md
│   │   └── DeploymentGuide.md
│   │
│   └── API/
│       └── ... (API documentation)
│
├── deployments/                        # ⬅️ Deployment configurations
│   ├── Docker/
│   │   ├── Dockerfile.dev
│   │   ├── Dockerfile.prod
│   │   ├── Dockerfile.qa
│   │   └── Dockerfile.test
│   │
│   └── CI-CD/
│       ├── Jenkinsfile
│       ├── Jenkinsfile-eds
│       ├── jenkins-config.yaml
│       └── jenkins-config-eds.yaml
│
└── scripts/                            # ⬅️ Automation scripts
    ├── build.sh
    ├── test.sh
    ├── deploy.sh
    └── ... (build/deploy scripts)
```

---

## 📋 Migration Plan

### Phase 1: Cleanup and Organization (Low Risk)

**Estimated Time**: 1-2 days

#### 1.1 Clean Up Root Directory

```bash
# Create organized folders
mkdir -p tools/Deprecated
mkdir -p database/Scripts/Temp
mkdir -p docs/Architecture

# Move temporary test files
mv test_*.py tools/Deprecated/
mv update_liaison_office_ids.py tools/Deprecated/
mv generate_pubsub_embedding_messages.sql database/Scripts/Temp/

# Remove unnecessary files
rm package-lock.json  # Shouldn't be in .NET project root
```

#### 1.2 Organize Database Scripts

```bash
# Create database folder structure
mkdir -p database/Scripts/{Analysis,Maintenance,Setup}
mkdir -p database/Seed/Data

# Move scripts from UNOPS.PAO.UNOPSDataAccess/Scripts/
mv UNOPS.PAO.UNOPSDataAccess/Scripts/CSV/* database/Seed/Data/
mv UNOPS.PAO.UNOPSDataAccess/Scripts/Python/* tools/DataImport/Python/
mv UNOPS.PAO.UNOPSDataAccess/Scripts/2025-*/ tools/DataImport/Archives/

# Move SQL scripts to appropriate categories
mv UNOPS.PAO.UNOPSDataAccess/Scripts/*_Summary.sql database/Scripts/Analysis/
mv UNOPS.PAO.UNOPSDataAccess/Scripts/seed-*.sql database/Scripts/Setup/
mv UNOPS.PAO.UNOPSDataAccess/Scripts/fix-*.sql database/Scripts/Maintenance/
```

#### 1.3 Organize Documentation

```bash
# Move documentation to docs folder
mkdir -p docs/{Architecture,Security,Development,API}

mv Readme/*.md docs/Security/
mv UNOPS.PAO.Documentation/*.md docs/Development/
mv tasks/*.md docs/Development/
mv UNOPS.PAO.UNOPSBusiness/README_RBAC_Implementation.md docs/Security/
mv UNOPS.PAO.Server/CHANGELOG.md docs/Development/
```

#### 1.4 Organize Deployment Files

```bash
# Create deployments folder
mkdir -p deployments/{Docker,CI-CD}

# Move deployment files
mv Dockerfile.* deployments/Docker/
mv Jenkinsfile* deployments/CI-CD/
mv jenkins-config*.yaml deployments/CI-CD/
```

---

### Phase 2: Squash Migrations (Medium Risk)

**Estimated Time**: 4-8 hours

**⚠️ WARNING**: This is a destructive operation. Create a backup first!

#### 2.1 Backup Current Database

```bash
# PostgreSQL example
pg_dump -h localhost -U postgres -d pao_database > backup_$(date +%Y%m%d).sql

# Or SQL Server
# Use SQL Server Management Studio to create a backup
```

#### 2.2 Squash Migrations

```bash
# 1. Remove all migration files
rm -rf UNOPS.PAO.UNOPSDataAccess/Migrations/*

# 2. Create new initial migration
cd UNOPS.PAO.Server
dotnet ef migrations add InitialMigration --project ../UNOPS.PAO.UNOPSDataAccess --context UNOPSAppDbContext

# 3. Verify the new migration
# Check that InitialMigration creates all tables correctly

# 4. Test on a fresh database
dotnet ef database update --project ../UNOPS.PAO.UNOPSDataAccess --context UNOPSAppDbContext
```

#### 2.3 Update Production Migration History

```sql
-- Production database: Clear migration history and add squashed migration
DELETE FROM __EFMigrationsHistory;
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20250115000000_InitialMigration', '9.0.0');
```

**Benefits**:
- ✅ Reduces 262 migrations to 1-2
- ✅ Faster application startup
- ✅ Easier to understand database schema
- ✅ Smaller repository size

---

### Phase 3: Organize Models Project (Medium Risk)

**Estimated Time**: 2-4 hours

#### 3.1 Create Folder Structure

```bash
cd UNOPS.PAO.Models

mkdir -p Partners Contacts Interactions Documents OrganizationUnits AI \
         Integrations Admin Location Notifications User Search Filters \
         Shared Workflow Audit Converters
```

#### 3.2 Move Files to Folders

```bash
# Partners
mv Partner*.cs Partners/
mv ExternalPartnerTreeModel.cs Partners/

# Contacts
mv Contact*.cs Contacts/
mv ExternalContactModel.cs Contacts/
mv GmailCreateContactsRequest.cs Contacts/

# Interactions
mv Interaction*.cs Interactions/
mv ExternalInteractionModel.cs Interactions/
mv GmailInteractionRequest.cs Interactions/

# Documents
mv Document*.cs Documents/
mv Link*.cs Documents/
mv UpdateDocumentRequest.cs Documents/

# OrganizationUnits
mv Organization*.cs OrganizationUnits/
mv OrgUnit*.cs OrganizationUnits/
mv LiaisonOffice*.cs OrganizationUnits/

# AI
mv Ai*.cs AI/
mv Gemini*.cs AI/
mv Entity*Embeddings*.cs AI/
mv Session*.cs AI/
mv TestPrompt*.cs AI/
mv AnalyseFileRequest.cs AI/
mv EntityDetectionResult.cs AI/

# Integrations
mv Gmail*.cs Integrations/
mv GenerateGoogleDocRequest.cs Integrations/

# Admin
mv UserManagement*.cs Admin/
mv UsersPagedRequest.cs Admin/

# Location
mv Country*.cs Location/
mv Currency*.cs Location/

# Notifications
mv Notification*.cs Notifications/

# User
mv PAOUser*.cs User/
mv Profile*.cs User/
mv UserValue*.cs User/

# Search
mv GlobalSearch*.cs Search/
mv Search*.cs Search/

# Filters
mv SavedFilter*.cs Filters/
mv TypeaheadInput.cs Filters/

# Shared
mv BaseEngagement*.cs Shared/
mv Pagination*.cs Shared/
mv RequestParameters.cs Shared/
mv SpecificationPaginationRequest.cs Shared/
mv Configuration*.cs Shared/
mv Entity*.cs Shared/  # EntityTag, EntityPermissions, etc.
mv Eligible*.cs Shared/
mv Extensible*.cs Shared/
mv Error*.cs Shared/
mv Recent*.cs Shared/
mv BulkUpload*.cs Shared/
mv Application*.cs Shared/
mv Applicant*.cs Shared/

# Workflow - already has folder
# Audit - already has folder
# Converters - already has folder
```

#### 3.3 Update Namespaces

```bash
# Use find and replace in IDE
# Old: namespace UNOPS.PAO.Models;
# New: namespace UNOPS.PAO.Models.Partners; (for Partners folder)
```

---

### Phase 4: Set Up Unit Testing Infrastructure (High Priority)

**Estimated Time**: 1-2 days (setup + initial tests)

#### 4.1 Create Test Projects

```bash
# Create unit test projects
dotnet new xunit -n UNOPS.PAO.Domain.Tests -o tests/Unit/UNOPS.PAO.Domain.Tests
dotnet new xunit -n UNOPS.PAO.Business.Tests -o tests/Unit/UNOPS.PAO.Business.Tests
dotnet new xunit -n UNOPS.PAO.Presentation.Tests -o tests/Unit/UNOPS.PAO.Presentation.Tests
dotnet new xunit -n UNOPS.PAO.Utilities.Tests -o tests/Unit/UNOPS.PAO.Utilities.Tests

# Add test projects to solution
dotnet sln UNOPS.PAO.sln add tests/Unit/UNOPS.PAO.Domain.Tests/UNOPS.PAO.Domain.Tests.csproj
dotnet sln UNOPS.PAO.sln add tests/Unit/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj
dotnet sln UNOPS.PAO.sln add tests/Unit/UNOPS.PAO.Presentation.Tests/UNOPS.PAO.Presentation.Tests.csproj
dotnet sln UNOPS.PAO.sln add tests/Unit/UNOPS.PAO.Utilities.Tests/UNOPS.PAO.Utilities.Tests.csproj

# Add project references
cd tests/Unit/UNOPS.PAO.Domain.Tests
dotnet add reference ../../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj

cd ../UNOPS.PAO.Business.Tests
dotnet add reference ../../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
dotnet add reference ../../../UNOPS.PAO.Domain/UNOPS.PAO.Domain.csproj

cd ../UNOPS.PAO.Presentation.Tests
dotnet add reference ../../../UNOPS.PAO.Presentation/UNOPS.PAO.Presentation.csproj
dotnet add reference ../../../UNOPS.PAO.Business/UNOPS.PAO.Business.csproj
```

#### 4.2 Install Testing Packages

```bash
cd tests/Unit/UNOPS.PAO.Business.Tests

# Core testing packages
dotnet add package Moq                                  # Mocking framework
dotnet add package FluentAssertions                     # Better assertions
dotnet add package AutoFixture                          # Test data generation
dotnet add package AutoFixture.Xunit2                   # AutoFixture + xUnit
dotnet add package AutoFixture.AutoMoq                  # AutoFixture + Moq
dotnet add package coverlet.collector                   # Code coverage
dotnet add package Microsoft.NET.Test.Sdk

# Repeat for other test projects
```

#### 4.3 Create Base Test Classes

Create `tests/Unit/UNOPS.PAO.Business.Tests/TestBase.cs`:

```csharp
using AutoFixture;
using AutoFixture.AutoMoq;

namespace UNOPS.PAO.Business.Tests;

public abstract class TestBase
{
    protected IFixture Fixture { get; }

    protected TestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
}
```

#### 4.4 Write First Unit Tests

See "Testing Standards & Examples" section for detailed examples.

---

### Phase 5: Consolidate Duplicate Layers (High Risk)

**Estimated Time**: 1-2 weeks

**⚠️ WARNING**: This is a major refactoring. Requires careful planning!

#### Option A: Merge UNOPS layers into PAO layers

```bash
# Strategy: Move UNOPS-specific code into PAO projects with feature flags or inheritance

# Example: Merge UNOPSBusiness into Business
# 1. Review all managers/services in UNOPSBusiness
# 2. Identify UNOPS-specific vs general functionality
# 3. Move general functionality to PAO.Business
# 4. Keep UNOPS-specific as derived classes
```

#### Option B: Rename and clarify purpose

```bash
# Strategy: Rename "UNOPS" prefix to indicate purpose

# Rename UNOPSDomain -> PAO.Domain.Extended
# Rename UNOPSBusiness -> PAO.Business.Extended
# Rename UNOPSDataAccess -> PAO.DataAccess.Extended
```

**Recommendation**: Choose Option A for cleaner architecture, Option B for faster migration.

---

### Phase 6: Reorganize Presentation Layer (Low Risk)

**Estimated Time**: 2-4 hours

#### 6.1 Create Controller Folders

```bash
cd UNOPS.PAO.Presentation/Controllers

mkdir -p Partners Contacts Interactions Documents OrganizationUnits \
         Admin AI Integrations Location Dashboard User Shared
```

#### 6.2 Move Controllers

```bash
# Partners
mv Partner*.cs Partners/

# Contacts
mv Contact*.cs Contacts/

# Interactions
mv Interaction*.cs Interactions/

# Documents
mv Document*.cs Documents/
mv Link*.cs Documents/

# OrganizationUnits
mv Organization*.cs OrganizationUnits/
mv LiaisonOffice*.cs OrganizationUnits/

# Admin
mv SystemAdmin*.cs Admin/
mv UserManagement*.cs Admin/
mv Permission*.cs Admin/
mv EntityConfiguration*.cs Admin/

# AI
mv Gemini*.cs AI/
mv Notification*.cs AI/

# Integrations
mv GmailAddon*.cs Integrations/

# Location
mv Country*.cs Location/

# Dashboard
mv Dashboard*.cs Dashboard/

# User
mv UserProfile*.cs User/
mv UserPreference*.cs User/

# Shared
mv BaseController.cs Shared/
mv Configuration*.cs Shared/
mv Global*.cs Shared/
mv SavedFilter*.cs Shared/
mv Values*.cs Shared/
```

---

## 🧪 Testing Standards & Examples

### Unit Testing Standards

#### Domain Tests Example

```csharp
// tests/Unit/UNOPS.PAO.Domain.Tests/Entities/PartnerTests.cs
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Domain.Tests.Entities;

public class PartnerTests
{
    [Fact]
    public void Partner_Should_Initialize_With_Pending_Status()
    {
        // Arrange & Act
        var partner = new Partner
        {
            Name = "Test Partner",
            Description = "Test Description"
        };

        // Assert
        partner.Status.Should().Be(EntityStatus.Active);
        partner.Name.Should().Be("Test Partner");
    }

    [Theory]
    [InlineData(PartnerApprovalStatus.Approved)]
    [InlineData(PartnerApprovalStatus.Pending)]
    [InlineData(PartnerApprovalStatus.Rejected)]
    public void Partner_Should_Allow_Valid_Approval_Status(PartnerApprovalStatus status)
    {
        // Arrange
        var partner = new Partner { Name = "Test" };

        // Act
        partner.ApprovalStatus = status;

        // Assert
        partner.ApprovalStatus.Should().Be(status);
    }

    [Fact]
    public void Partner_Should_Track_Modifications()
    {
        // Arrange
        var partner = new Partner
        {
            Name = "Original Name",
            ModifiedBy = "user1",
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };

        var originalModifiedDate = partner.ModifiedDate;

        // Act
        partner.Name = "Updated Name";
        partner.ModifiedBy = "user2";
        partner.ModifiedDate = DateTime.UtcNow;

        // Assert
        partner.Name.Should().Be("Updated Name");
        partner.ModifiedBy.Should().Be("user2");
        partner.ModifiedDate.Should().BeAfter(originalModifiedDate);
    }
}
```

#### Specification Tests Example

```csharp
// tests/Unit/UNOPS.PAO.Domain.Tests/Specifications/PartnerSpecificationTests.cs
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using Xunit;

namespace UNOPS.PAO.Domain.Tests.Specifications;

public class PartnerByStatusSpecificationTests
{
    [Fact]
    public void Should_Filter_Partners_By_Active_Status()
    {
        // Arrange
        var partners = new List<Partner>
        {
            new() { Id = 1, Name = "Partner 1", Status = EntityStatus.Active },
            new() { Id = 2, Name = "Partner 2", Status = EntityStatus.Inactive },
            new() { Id = 3, Name = "Partner 3", Status = EntityStatus.Active }
        }.AsQueryable();

        var spec = new PartnerByStatusSpecification(EntityStatus.Active);

        // Act
        var result = partners.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == 1);
        result.Should().Contain(p => p.Id == 3);
        result.Should().NotContain(p => p.Id == 2);
    }

    [Fact]
    public void Should_Return_Empty_When_No_Partners_Match_Status()
    {
        // Arrange
        var partners = new List<Partner>
        {
            new() { Id = 1, Name = "Partner 1", Status = EntityStatus.Active }
        }.AsQueryable();

        var spec = new PartnerByStatusSpecification(EntityStatus.Deleted);

        // Act
        var result = partners.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        result.Should().BeEmpty();
    }
}
```

#### Business Logic Tests Example

```csharp
// tests/Unit/UNOPS.PAO.Business.Tests/Managers/ContactManagerTests.cs
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class ContactManagerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly ContactManager _contactManager;

    public ContactManagerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        
        _mockDbContext = new Mock<AppDbContext>();
        _contactManager = new ContactManager(_mockDbContext.Object);
    }

    [Fact]
    public async Task CreateContact_Should_Add_Contact_To_Database()
    {
        // Arrange
        var contactRequest = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1234567890"
        };

        var mockDbSet = new Mock<DbSet<Contact>>();
        _mockDbContext.Setup(x => x.Contacts).Returns(mockDbSet.Object);

        // Act
        var result = await _contactManager.CreateContactAsync(contactRequest);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        mockDbSet.Verify(x => x.AddAsync(It.IsAny<Contact>(), default), Times.Once);
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateContact_Should_Throw_When_Email_Is_Duplicate()
    {
        // Arrange
        var existingContact = new Contact
        {
            Email = "john.doe@example.com"
        };

        var contactRequest = new ContactRequest
        {
            Email = "john.doe@example.com"
        };

        _mockDbContext.Setup(x => x.Contacts.AnyAsync(
            It.IsAny<Expression<Func<Contact, bool>>>(),
            default
        )).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _contactManager.CreateContactAsync(contactRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*duplicate email*");
    }

    [Fact]
    public async Task GetContact_Should_Return_Contact_When_Exists()
    {
        // Arrange
        var contactId = 123;
        var expectedContact = new Contact
        {
            Id = contactId,
            FirstName = "Jane",
            LastName = "Smith"
        };

        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync(expectedContact);

        // Act
        var result = await _contactManager.GetContactAsync(contactId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contactId);
        result.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task GetContact_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var contactId = 999;
        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync((Contact)null);

        // Act
        var result = await _contactManager.GetContactAsync(contactId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateContact_Should_Modify_Existing_Contact()
    {
        // Arrange
        var contactId = 123;
        var existingContact = new Contact
        {
            Id = contactId,
            FirstName = "John",
            LastName = "Doe",
            Email = "old@example.com"
        };

        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "new@example.com"
        };

        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync(existingContact);

        // Act
        var result = await _contactManager.UpdateContactAsync(contactId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.Email.Should().Be("new@example.com");
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteContact_Should_Mark_As_Deleted()
    {
        // Arrange
        var contactId = 123;
        var contact = new Contact
        {
            Id = contactId,
            FirstName = "John",
            IsDeleted = false
        };

        _mockDbContext.Setup(x => x.Contacts.FindAsync(contactId))
            .ReturnsAsync(contact);

        // Act
        await _contactManager.DeleteContactAsync(contactId);

        // Assert
        contact.IsDeleted.Should().BeTrue();
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
```

#### Controller Tests Example

```csharp
// tests/Unit/UNOPS.PAO.Presentation.Tests/Controllers/ContactControllerTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class ContactControllerTests
{
    private readonly Mock<IContactManager> _mockContactManager;
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _mockContactManager = new Mock<IContactManager>();
        _controller = new ContactController(_mockContactManager.Object);
    }

    [Fact]
    public async Task GetContact_Should_Return_Ok_With_Contact()
    {
        // Arrange
        var contactId = 123;
        var expectedContact = new ContactModel
        {
            Id = contactId,
            FirstName = "John",
            LastName = "Doe"
        };

        _mockContactManager
            .Setup(x => x.GetContactAsync(contactId))
            .ReturnsAsync(expectedContact);

        // Act
        var result = await _controller.GetContact(contactId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var contact = okResult.Value.Should().BeOfType<ContactModel>().Subject;
        contact.Id.Should().Be(contactId);
        contact.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetContact_Should_Return_NotFound_When_Contact_Not_Exists()
    {
        // Arrange
        var contactId = 999;
        _mockContactManager
            .Setup(x => x.GetContactAsync(contactId))
            .ReturnsAsync((ContactModel)null);

        // Act
        var result = await _controller.GetContact(contactId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateContact_Should_Return_Created_With_Location()
    {
        // Arrange
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var createdContact = new ContactModel
        {
            Id = 123,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockContactManager
            .Setup(x => x.CreateContactAsync(request))
            .ReturnsAsync(createdContact);

        // Act
        var result = await _controller.CreateContact(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetContact));
        createdResult.RouteValues["id"].Should().Be(123);
        
        var contact = createdResult.Value.Should().BeOfType<ContactModel>().Subject;
        contact.Id.Should().Be(123);
    }

    [Fact]
    public async Task CreateContact_Should_Return_BadRequest_When_Model_Invalid()
    {
        // Arrange
        var request = new ContactRequest { FirstName = "" }; // Invalid
        _controller.ModelState.AddModelError("FirstName", "Required");

        // Act
        var result = await _controller.CreateContact(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateContact_Should_Return_Ok_With_Updated_Contact()
    {
        // Arrange
        var contactId = 123;
        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Jane",
            LastName = "Doe"
        };

        var updatedContact = new ContactModel
        {
            Id = contactId,
            FirstName = "Jane",
            LastName = "Doe"
        };

        _mockContactManager
            .Setup(x => x.UpdateContactAsync(contactId, updateRequest))
            .ReturnsAsync(updatedContact);

        // Act
        var result = await _controller.UpdateContact(contactId, updateRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var contact = okResult.Value.Should().BeOfType<ContactModel>().Subject;
        contact.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task DeleteContact_Should_Return_NoContent()
    {
        // Arrange
        var contactId = 123;
        _mockContactManager
            .Setup(x => x.DeleteContactAsync(contactId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteContact(contactId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockContactManager.Verify(x => x.DeleteContactAsync(contactId), Times.Once);
    }
}
```

### Code Coverage Standards

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator \
    -reports:"**/coverage.cobertura.xml" \
    -targetdir:"coveragereport" \
    -reporttypes:Html
```

**Coverage Requirements**:

| Layer | Minimum Coverage | Target |
|-------|-----------------|--------|
| Domain | 80% | 90%+ |
| Business Logic | 80% | 85%+ |
| Controllers | 70% | 80%+ |
| Services | 80% | 85%+ |
| **Overall** | **75%** | **80%+** |

---

## 🎯 Key Architectural Principles

### 1. Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │  ← Controllers, DTOs
│         (UNOPS.PAO.Presentation)        │
├─────────────────────────────────────────┤
│         Application Layer               │  ← Business Logic, Managers
│         (UNOPS.PAO.Business)            │
├─────────────────────────────────────────┤
│         Domain Layer                    │  ← Entities, Specifications
│         (UNOPS.PAO.Domain)              │  ← No dependencies!
├─────────────────────────────────────────┤
│         Infrastructure Layer            │  ← EF Core, External APIs
│    (UNOPS.PAO.DataAccess, etc.)        │
└─────────────────────────────────────────┘
```

**Dependency Rules**:
- Domain has NO dependencies on other layers
- Business depends on Domain only
- Presentation depends on Business and Domain
- Infrastructure depends on Domain

### 2. Manager vs Service Guidelines

**Managers** (Aggregate Root Operations):
- Orchestrate business operations for an aggregate root
- Example: `PartnerManager`, `ContactManager`, `InteractionManager`
- Methods: CRUD + business operations (Approve, Reject, Archive, etc.)
- Can call multiple repositories and services

**Services** (Cross-Cutting Concerns):
- Provide specific functionality or utilities
- Example: `PermissionService`, `SearchService`, `CacheService`
- Don't own an aggregate root
- Can be used by multiple managers

**Repositories** (Data Access):
- Abstract data access
- CRUD operations only
- No business logic
- Optional (can use DbContext directly with specifications)

### 3. Testing Strategy

```
Testing Pyramid:
       ╱╲
      ╱  ╲      E2E/Integration Tests (Few - Slow)
     ╱────╲     - API integration tests
    ╱      ╲    - Database integration tests
   ╱────────╲
  ╱          ╲  Unit Tests (Many - Fast)
 ╱────────────╲ - Domain tests
╱______________╲ - Business logic tests
                - Controller tests (mocked)
```

**Guidelines**:
- 70% Unit Tests (fast, isolated)
- 20% Integration Tests (API + DB)
- 10% E2E Tests (critical workflows)

---

## ✅ Validation Checklist

After restructuring, verify:

### Architecture
- [ ] **No duplicate layers**: Consolidated PAO/UNOPS projects
- [ ] **Clean dependencies**: Domain has no infrastructure dependencies
- [ ] **Clear boundaries**: Each project has a single responsibility
- [ ] **Consistent naming**: No "Grants" references, consistent conventions
- [ ] **Organized folders**: Controllers, Models, Services organized by feature

### Testing
- [ ] **Unit test projects exist**: Domain, Business, Presentation
- [ ] **Test coverage ≥ 75%**: Measured and enforced
- [ ] **Fast tests**: Unit tests run in < 10 seconds
- [ ] **CI/CD integration**: Tests run automatically
- [ ] **Clear test structure**: Arrange-Act-Assert pattern

### Code Organization
- [ ] **Models organized**: 107 files grouped by feature
- [ ] **Controllers organized**: 35 controllers in folders
- [ ] **Clean root directory**: No temporary files
- [ ] **Database files separate**: Scripts and CSV in /database
- [ ] **Documentation organized**: All docs in /docs

### Migrations
- [ ] **Migration count reduced**: < 10 migration files
- [ ] **Fast startup**: Application starts in < 5 seconds
- [ ] **Clear history**: Migration names are descriptive

### Build & Run
- [ ] **Solution builds**: `dotnet build` succeeds
- [ ] **Tests pass**: `dotnet test` succeeds
- [ ] **Coverage report**: Generated successfully
- [ ] **No warnings**: Clean build output
- [ ] **Documentation updated**: Architecture docs reflect changes

---

## 📈 Success Metrics

### Before Restructuring
```
- Migration Files: 262
- Test Projects: 1 (Integration only)
- Root Temp Files: 12+
- Models Organization: Flat (107 files)
- Controllers Organization: Flat (35 files)
- Test Coverage: ~20%
- Build Time: ~60 seconds
- Test Execution: Integration only (~2-5 min)
```

### After Restructuring
```
- Migration Files: < 10 (squashed)
- Test Projects: 5 (4 unit + 1 integration)
- Root Temp Files: 0 (moved to tools/Deprecated)
- Models Organization: 15 folders by feature
- Controllers Organization: 10 folders by feature
- Test Coverage: > 75%
- Build Time: ~45 seconds (25% faster)
- Test Execution: Unit (< 10 sec) + Integration (~2-5 min)
```

### Measurable KPIs

| Metric | Before | Target | Status |
|--------|--------|--------|--------|
| Code Coverage | ~20% | 75%+ | ⏳ In Progress |
| Migration Files | 262 | < 10 | ⏳ Pending |
| Root Temp Files | 12+ | 0 | ⏳ Pending |
| Test Execution Time (Unit) | N/A | < 10 sec | ⏳ Pending |
| Build Time | ~60s | ~45s | ⏳ Pending |
| Onboarding Time (Developer) | ~2-3 days | ~1 day | ⏳ Pending |

---

## 🚀 Quick Wins (Implement First)

These changes have **low risk** and **high impact**:

### 1. Clean Up Root Directory
**Impact**: ⭐⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 15 minutes

```bash
# Move temporary files
mkdir -p tools/Deprecated
mv test_*.py tools/Deprecated/
mv update_liaison_office_ids.py tools/Deprecated/
rm package-lock.json
```

### 2. Organize Documentation
**Impact**: ⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 20 minutes

```bash
mkdir -p docs/{Architecture,Security,Development}
mv Readme/*.md docs/Security/
mv UNOPS.PAO.Documentation/*.md docs/Development/
mv tasks/*.md docs/Development/
```

### 3. Create Unit Test Projects
**Impact**: ⭐⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 30 minutes

```bash
# Create test projects (see Phase 4 above)
dotnet new xunit -n UNOPS.PAO.Business.Tests
# Add to solution and references
# Write first 5-10 tests for critical business logic
```

### 4. Add Test Coverage Reporting
**Impact**: ⭐⭐⭐⭐  
**Risk**: ⭐  
**Effort**: 15 minutes

```bash
# Add coverlet packages to test projects
dotnet add package coverlet.collector
dotnet add package coverlet.msbuild

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### 5. Organize Models Project
**Impact**: ⭐⭐⭐⭐  
**Risk**: ⭐⭐  
**Effort**: 2 hours

```bash
# Create folders and move files (see Phase 3)
cd UNOPS.PAO.Models
mkdir -p Partners Contacts Interactions Documents
mv Partner*.cs Partners/
mv Contact*.cs Contacts/
# ... etc
```

---

## 📚 References & Best Practices

### .NET Architecture Guides
- [Clean Architecture with ASP.NET Core](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)
- [Domain-Driven Design](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [Specification Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design#repository-and-unit-of-work-patterns)

### Testing Resources
- **xUnit**: [https://xunit.net/](https://xunit.net/) - Primary test framework
- **Moq**: [https://github.com/moq/moq4](https://github.com/moq/moq4) - Mocking framework
- **FluentAssertions**: [https://fluentassertions.com/](https://fluentassertions.com/) - Better assertions
- **AutoFixture**: [https://github.com/AutoFixture/AutoFixture](https://github.com/AutoFixture/AutoFixture) - Test data generation
- **Coverlet**: [https://github.com/coverlet-coverage/coverlet](https://github.com/coverlet-coverage/coverlet) - Code coverage

### Entity Framework Core
- [Migrations Best Practices](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Squashing Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/squashing)

---

## 🤝 Team Guidelines

### For New Features
1. Follow Clean Architecture layers
2. Add unit tests (aim for 80%+ coverage)
3. Use specifications for complex queries
4. Organize by feature, not by type
5. Create integration tests for API endpoints

### For Bug Fixes
1. Write a failing test first (TDD)
2. Fix the bug
3. Verify test passes
4. Check code coverage didn't decrease

### Code Review Checklist
- [ ] **Tests included**: Unit tests for business logic
- [ ] **Coverage maintained**: No decrease in coverage
- [ ] **Architecture followed**: Correct project/folder
- [ ] **Naming consistent**: Follows conventions
- [ ] **Documentation updated**: If architecture changed
- [ ] **No warnings**: Build is clean
- [ ] **Migration included**: If database schema changed

---

## 📞 Support & Questions

For questions about this architecture:
1. Refer to Clean Architecture documentation
2. Check this document's decision matrices
3. Discuss with architecture team lead
4. Document decisions in Architecture Decision Records (ADRs)

---

**Document Version**: 1.0  
**Last Updated**: January 15, 2025  
**Author**: Backend Architecture Analysis  
**Status**: Proposed - Pending Team Review

**Next Steps**:
1. Review this document with team
2. Prioritize quick wins (Phase 1)
3. Set up unit testing infrastructure (Phase 4)
4. Create test coverage baseline
5. Plan migration squashing (Phase 2)
6. Begin incremental restructuring

