# Business Logic Test Cases — Master Index

**Component:** `UNOPS.PAO.Business/Managers/*` (All Business Managers)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

Business logic test case index: master list of all business logic test cases across managers, cross-references, overall compliance summary.

---

## Index of Business Logic Test Case Files

| # | File | Manager/Feature | Total Tests | 3:1 Ratio | Compliance |
|---|------|-----------------|-------------|-----------|------------|
| 1 | [ContactManager_BusinessLogic_TestCases.md](./ContactManager_BusinessLogic_TestCases.md) | ContactManager | 397 | ✅ PASS | ✅ |
| 2 | [DocumentManager_BusinessLogic_TestCases.md](./DocumentManager_BusinessLogic_TestCases.md) | DocumentManager | 397 | ✅ PASS | ✅ |
| 3 | [InteractionManager_BusinessLogic_TestCases.md](./InteractionManager_BusinessLogic_TestCases.md) | InteractionManager | 397 | ✅ PASS | ✅ |
| 4 | [OrganizationHierarchyManager_BusinessLogic_TestCases.md](./OrganizationHierarchyManager_BusinessLogic_TestCases.md) | OrganizationHierarchyManager | 397 | ✅ PASS | ✅ |
| 5 | [PartnerManager_BusinessLogic_TestCases.md](./PartnerManager_BusinessLogic_TestCases.md) | PartnerManager | 397 | ✅ PASS | ✅ |
| 6 | [DataImportFixes_BusinessLogic_TestCases.md](./DataImportFixes_BusinessLogic_TestCases.md) | Data Import Fixes | 397 | ✅ PASS | ✅ |
| 7 | [PartnerErpDimValueFix_BusinessLogic_TestCases.md](./PartnerErpDimValueFix_BusinessLogic_TestCases.md) | Partner ErpDimValue Fix | 397 | ✅ PASS | ✅ |

---

## Overall Compliance Summary

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| **Files Indexed** | 7 | 7 | ✅ |
| **Tests per File** | 397 | 397 | ✅ |
| **Total Tests (Indexed)** | 2,779 | 2,779 | ✅ |
| **3:1 Ratio per File** | PASS | PASS | ✅ |
| **Category Coverage** | All 10 | All 10 | ✅ |

---

## Category Breakdown (Per File)

Each indexed file contains exactly:

| Category | Count | Description |
|----------|-------|-------------|
| §1 Positive | 35 | Happy path, valid inputs, successful operations |
| §2 Negative | 70 | Invalid inputs, unauthorized access, error conditions |
| §3 Boundary | 70 | String/numeric/date/collection boundaries, Unicode |
| §4 Functional | 50 | Workflow, validation, constraint, audit rules |
| §5 Integration | 50 | CRUD, search, filter, pagination, relationships, errors |
| §6 Security | 50 | Injection, access control, IDOR, mass assignment, auth |
| §7 Concurrency | 25 | Race conditions, concurrent updates, deadlocks |
| §8 Unit | 21 | Validation, formatting, calculations, status logic |
| §9 Performance | 16 | Single ops, bulk, search, concurrent, memory |
| §10 Load | 10 | Sustained, spike, stress, recovery |

---

## Cross-References

### Manager Dependencies

| Manager | Depends On | Related Test Files |
|---------|------------|-------------------|
| ContactManager | PartnerManager | PartnerManager_BusinessLogic_TestCases.md |
| DocumentManager | ContactManager, PartnerManager | ContactManager, PartnerManager |
| InteractionManager | ContactManager, PartnerManager | ContactManager, PartnerManager |
| OrganizationHierarchyManager | - | - |
| PartnerManager | OrganizationHierarchyManager | OrganizationHierarchyManager |
| DataImportFixes | Multiple | All managers |
| PartnerErpDimValueFix | PartnerManager | PartnerManager |

### Shared Test Patterns

- **Soft Delete (IsDeleted):** All managers — filter in queries, soft delete on delete
- **Audit Trail:** All managers — CreatedBy, CreatedDate, LastModifiedBy, LastModifiedDate
- **Permission Checks:** All managers — entity-level and operation-level
- **OrgUnit Scope:** ContactManager, PartnerManager, InteractionManager — scoped queries

---

## Test Execution Prioritization

| Priority | Focus | Recommended Order |
|----------|-------|-------------------|
| **P0** | Critical business rules | PartnerManager → ContactManager → DocumentManager |
| **P1** | High-impact scenarios | InteractionManager → OrganizationHierarchyManager |
| **P2** | Fixes and enhancements | DataImportFixes → PartnerErpDimValueFix |

---

## Traceability Matrix (Index Level)

| Business Area | Test Files Covering |
|---------------|---------------------|
| Partner CRUD & Approval | PartnerManager, PartnerErpDimValueFix |
| Contact Management | ContactManager |
| Document Management | DocumentManager |
| Interaction Tracking | InteractionManager |
| Organization Hierarchy | OrganizationHierarchyManager |
| Data Import | DataImportFixes |
| ERP Integration | PartnerErpDimValueFix |

---

## Maintenance Notes

- **Add new file:** When creating a new business logic test file, add to index table with 397 tests, 3:1 PASS
- **Update existing:** Ensure any updates maintain 397 total and 3:1 ratio
- **Retire file:** Remove from index, update cross-references

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
