# Business Logic Tests

## Overview

This folder contains test documentation and C# implementations for business logic validation across the UNOPS Opportunity+ system. These tests focus on business rules, workflows, and complex logic rather than simple CRUD operations.

**Total Test Cases**: ~400+
**Status**: ✅ Complete - All tests converted to C#

---

## Test Files

### Documentation

| File | Description | Test Count |
|------|-------------|------------|
| PartnerManager_BusinessLogic_TestCases.md | Partner approval, ERP integration, org unit relationships | 80+ |
| ContactManager_BusinessLogic_TestCases.md | Contact-Partner relationships, deduplication, merging | 60+ |
| InteractionManager_BusinessLogic_TestCases.md | AI integration, calendar sync, timeline | 50+ |
| DocumentManager_BusinessLogic_TestCases.md | Storage providers, text extraction, OCR | 50+ |
| OrganizationHierarchyManager_BusinessLogic_TestCases.md | Hierarchy management, user access | 40+ |
| DataImportFixes_TestCases.md | Import validation and data fixes | 25+ |
| PartnerErpDimValueFix_TestCases.md | ERP dim value conflict resolution | 20+ |

### C# Tests

Located in: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/BusinessLogic/`

| File | Description | Test Count |
|------|-------------|------------|
| PartnerBusinessLogicTests.cs | All partner-related business logic | 400+ |

---

## Key Business Rules Tested

### P0 - Critical Priority

1. **Partner Approval Workflow**
   - ERP Dim Value assignment (unique, valid range)
   - Reserved range handling (8000-9999)
   - Approval status transitions
   - Required fields validation

2. **Organization Unit Relationships**
   - Only OrgUnit type allowed (not Country/Region)
   - User visibility filtering
   - Cascade on partner delete

3. **Contact-Partner Relationships**
   - Inheritance of org units
   - Primary contact management
   - Merge operations

### P1 - High Priority

1. **Partner Tree Integration**
   - Category/Group assignment
   - Hierarchy path calculation
   - Filter by category/group

2. **AI Integration**
   - Transcription from audio
   - Summary generation
   - Sentiment analysis
   - Keyword extraction

3. **Document Storage**
   - Multiple provider support (GCS, Google Drive)
   - Signed URL generation
   - Access control

### P2 - Medium Priority

1. **Smart Search**
   - Cross-entity search
   - Relevance ranking
   - Org unit filtering

2. **Import/Export**
   - CSV/Excel support
   - Validation errors
   - Duplicate handling

---

## Running Tests

```powershell
# Run all business logic tests
dotnet test --filter "Namespace~BusinessLogic"

# Run by priority
dotnet test --filter "FullyQualifiedName~TC_PM_BL_P0"  # P0 Critical
dotnet test --filter "FullyQualifiedName~TC_PM_BL_P1"  # P1 High
dotnet test --filter "FullyQualifiedName~TC_PM_BL_P2"  # P2 Medium
```

---

*Last Updated: December 19, 2025*
