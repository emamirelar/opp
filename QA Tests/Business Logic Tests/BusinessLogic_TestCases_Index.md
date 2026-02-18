# Business Logic Test Cases - Comprehensive Index

## Overview

This index provides a complete reference to all business logic test cases for the UNOPS Opportunity+ System Business Managers. These tests focus on validating actual business rules, workflows, and manager method behaviors.

---

## Test Case Summary by Manager

| Manager | P0 Critical | P1 High | P2 Medium | P3 Low | Total |
|---------|-------------|---------|-----------|--------|-------|
| **PartnerManager** | 15 | 15 | 5 | 3 | **38** |
| **ContactManager** | 12 | 10 | 5 | 3 | **30** |
| **InteractionManager** | 10 | 10 | 5 | 3 | **28** |
| **DocumentManager** | 8 | 8 | 4 | 3 | **23** |
| **OrganizationHierarchyManager** | 8 | 8 | 4 | 4 | **24** |
| **Data Import Fixes (NEW)** | 10 | 8 | 4 | 3 | **25** |
| **Partner ErpDimValue Fix (NEW)** | 7 | 6 | 3 | 3 | **19** |
| **Total** | **70** | **65** | **30** | **22** | **187** |

---

## PartnerManager Test Cases

### P0 - Critical (15 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-PM-BL-P0-001 | Partner Approval - Valid Workflow | Approved partners receive ERP Dim Value |
| TC-PM-BL-P0-002 | ERP Dim Value Uniqueness | Each approved partner has unique value |
| TC-PM-BL-P0-003 | Reserved ERP Range | 8000-9999 range reserved for special partners |
| TC-PM-BL-P0-004 | Partner Unapproval | Remove ERP integration on unapproval |
| TC-PM-BL-P0-005 | Draft to Active Transition | Status lifecycle validation |
| TC-PM-BL-P0-006 | Active to Closed Transition | Closed partners cannot be used |
| TC-PM-BL-P0-007 | OrgUnit Relationship Creation | Only OrgUnit type allowed |
| TC-PM-BL-P0-008 | Differential Update | Efficient relationship updates |
| TC-PM-BL-P0-009 | Soft Delete Verification | Historical data preservation |
| TC-PM-BL-P0-010 | Permission Check | Read vs Update access |
| TC-PM-BL-P0-011 | Partner Group Assignment | Category validation |
| TC-PM-BL-P0-012 | Approval Missing Fields | Required field validation |
| TC-PM-BL-P0-013 | Contact Cascade Behavior | Status affects contacts |
| TC-PM-BL-P0-014 | OrgUnit Filtering | User sees filtered data |
| TC-PM-BL-P0-015 | Logo Upload Validation | File type and size limits |

### P1 - High (15 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-PM-BL-P1-001 | Include Contacts/Interactions | Eager loading |
| TC-PM-BL-P1-002 | Partner Group Filtering | Filter by group |
| TC-PM-BL-P1-003 | Partner Category Filtering | Filter by category |
| TC-PM-BL-P1-004 | Partner Tree Recursion | Child node retrieval |
| TC-PM-BL-P1-005 | Complex Specification Filter | Multiple criteria |
| TC-PM-BL-P1-006 | Smart Search | Cross-entity search |
| TC-PM-BL-P1-007 | Concurrent Modification | Data consistency |
| TC-PM-BL-P1-008 | Sorting Options | OrderBy support |
| TC-PM-BL-P1-009 | Archive Validation | Only closed can archive |
| TC-PM-BL-P1-010 | Audit Fields - Create | CreatedBy/Date |
| TC-PM-BL-P1-011 | Audit Fields - Update | LastModifiedBy/Date |
| TC-PM-BL-P1-012 | Gmail Integration | Partner lookup by email |
| TC-PM-BL-P1-013 | Partner by Name | Exact match lookup |
| TC-PM-BL-P1-014 | Exclude Deleted | Default query filter |
| TC-PM-BL-P1-015 | Total Partner Count | Debug method |

[Full details: PartnerManager_BusinessLogic_TestCases.md](./PartnerManager_BusinessLogic_TestCases.md)

---

## ContactManager Test Cases

### P0 - Critical (12 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-CM-BL-P0-001 | Partner Association | Valid partner reference |
| TC-CM-BL-P0-002 | Required Fields Validation | Email, LastName required |
| TC-CM-BL-P0-003 | Email Format Validation | Valid email format |
| TC-CM-BL-P0-004 | Include Documents | Eager loading |
| TC-CM-BL-P0-005 | Include Interactions | Complete history |
| TC-CM-BL-P0-006 | Change Partner | Reassign contact |
| TC-CM-BL-P0-007 | Non-Existent Update | Graceful handling |
| TC-CM-BL-P0-008 | Soft Delete | Historical preservation |
| TC-CM-BL-P0-009 | Interactions Preserved | Delete doesn't cascade |
| TC-CM-BL-P0-010 | Partner Contacts Filter | Data isolation |
| TC-CM-BL-P0-011 | Permission Check | User claims access |
| TC-CM-BL-P0-012 | Gmail Lookup | Email matching |

### P1 - High (10 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-CM-BL-P1-001 | Pagination | Page size/index |
| TC-CM-BL-P1-002 | Email Domain Filter | Specification pattern |
| TC-CM-BL-P1-003 | Posted Contacts | Active only |
| TC-CM-BL-P1-004 | Email Exact Match | Case handling |
| TC-CM-BL-P1-005 | Audit Fields | Change tracking |
| TC-CM-BL-P1-006 | Profile Picture Upload | File handling |
| TC-CM-BL-P1-007 | Partner Suggestions | Unmatched emails |
| TC-CM-BL-P1-008 | Search Fields Metadata | Dynamic search |
| TC-CM-BL-P1-009 | Eligible Entities | External visibility |
| TC-CM-BL-P1-010 | Duplicate Email | Multi-partner contacts |

[Full details: ContactManager_BusinessLogic_TestCases.md](./ContactManager_BusinessLogic_TestCases.md)

---

## InteractionManager Test Cases

### P0 - Critical (10 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-IM-BL-P0-001 | Valid Meeting Creation | Required fields |
| TC-IM-BL-P0-002 | Type Validation | Valid InteractionType |
| TC-IM-BL-P0-003 | Contact Association | Link to contacts |
| TC-IM-BL-P0-004 | Include Related Data | Eager loading |
| TC-IM-BL-P0-005 | Change Type | Mutable type |
| TC-IM-BL-P0-006 | Soft Delete | Audit preservation |
| TC-IM-BL-P0-007 | Date Range Handling | Meeting duration |
| TC-IM-BL-P0-008 | Gmail Create Email | Add-on integration |
| TC-IM-BL-P0-009 | Permission Check | User access |
| TC-IM-BL-P0-010 | Filter by Contact | Contact history |

### P1 - High (10 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-IM-BL-P1-001 | Date Range Search | Filter by dates |
| TC-IM-BL-P1-002 | Type Search | Filter by type |
| TC-IM-BL-P1-003 | Pagination | Large result sets |
| TC-IM-BL-P1-004 | Add Contacts | Multi-person tracking |
| TC-IM-BL-P1-005 | Remove Contacts | Data correction |
| TC-IM-BL-P1-006 | Long Description | Meeting notes |
| TC-IM-BL-P1-007 | Audit Fields | Change tracking |
| TC-IM-BL-P1-008 | Recent Interactions | Partner view |
| TC-IM-BL-P1-009 | Gmail Deduplication | Prevent duplicates |
| TC-IM-BL-P1-010 | Status Lifecycle | State management |

[Full details: InteractionManager_BusinessLogic_TestCases.md](./InteractionManager_BusinessLogic_TestCases.md)

---

## DocumentManager Test Cases

### P0 - Critical (8 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-DM-BL-P0-001 | Valid File Upload | Storage integration |
| TC-DM-BL-P0-002 | Invalid File Type | Security validation |
| TC-DM-BL-P0-003 | Size Limit | Storage management |
| TC-DM-BL-P0-004 | Signed URL Generation | Secure access |
| TC-DM-BL-P0-005 | Partner Association | Organization docs |
| TC-DM-BL-P0-006 | Contact Association | Contact docs |
| TC-DM-BL-P0-007 | Soft Delete | Recovery capability |
| TC-DM-BL-P0-008 | Include Link | Document access |

### P1 - High (8 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-DM-BL-P1-001 | Type Assignment | Classification |
| TC-DM-BL-P1-002 | List by Entity | Complete view |
| TC-DM-BL-P1-003 | Metadata Update | Info correction |
| TC-DM-BL-P1-004 | Document Replace | Version management |
| TC-DM-BL-P1-005 | Pagination | Performance |
| TC-DM-BL-P1-006 | Multiple Associations | Flexible linking |
| TC-DM-BL-P1-007 | Name Search | Discovery |
| TC-DM-BL-P1-008 | Audit Fields | Accountability |

[Full details: DocumentManager_BusinessLogic_TestCases.md](./DocumentManager_BusinessLogic_TestCases.md)

---

## OrganizationHierarchyManager Test Cases

### P0 - Critical (8 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-OHM-BL-P0-001 | Create OrgUnit | Hierarchy structure |
| TC-OHM-BL-P0-002 | Code Uniqueness | Data integrity |
| TC-OHM-BL-P0-003 | Type Filtering | Hierarchy navigation |
| TC-OHM-BL-P0-004 | Direct Children | Immediate children |
| TC-OHM-BL-P0-005 | Recursive Descendants | All levels |
| TC-OHM-BL-P0-006 | Ancestor Path | Root traversal |
| TC-OHM-BL-P0-007 | Change Parent | Reorganization |
| TC-OHM-BL-P0-008 | OrgUnit Only Relationships | Model integrity |

### P1 - High (8 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-OHM-BL-P1-001 | Search by Code | Quick lookup |
| TC-OHM-BL-P1-002 | Search by Name | User convenience |
| TC-OHM-BL-P1-003 | Parent Type Rules | Hierarchy validation |
| TC-OHM-BL-P1-004 | Root Units | Top-level structure |
| TC-OHM-BL-P1-005 | Delete with Children | Cascade protection |
| TC-OHM-BL-P1-006 | Flat vs Hierarchical | List formats |
| TC-OHM-BL-P1-007 | User Permission Filtering | Multi-tenant |
| TC-OHM-BL-P1-008 | Audit Fields | Accountability |

[Full details: OrganizationHierarchyManager_BusinessLogic_TestCases.md](./OrganizationHierarchyManager_BusinessLogic_TestCases.md)

---

## 🆕 Data Import Fixes Test Cases (PR #479)

### P0 - Critical (10 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-DIF-BL-P0-001 | Contact Query - UserId -1 | Opportunity+ User info lookup |
| TC-DIF-BL-P0-002 | Contact Query - UserId 0 | Zero excluded from lookup |
| TC-DIF-BL-P0-003 | Contact List - Negative UserId | Bulk query includes -1 |
| TC-DIF-BL-P0-004 | Interaction Query - UserId -1 | Interaction creator lookup |
| TC-DIF-BL-P0-005 | Partner Query - UserId -1 | Partner creator lookup |
| TC-DIF-BL-P0-006 | User Info Lookup - Negative IDs | Repository handles -1 |
| TC-DIF-BL-P0-007 | Audit Fix - Partner CreatedBy | Migration to system user |
| TC-DIF-BL-P0-008 | Audit Fix - Partner LastModifiedBy | Audit field migration |
| TC-DIF-BL-P0-009 | Audit Fix - Interaction CreatedBy | Interaction audit migration |
| TC-DIF-BL-P0-010 | Audit Fix - Interaction LastModifiedBy | Complete audit migration |

### P1 - High (8 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-DIF-BL-P1-001 | Transaction Rollback | Atomicity on error |
| TC-DIF-BL-P1-002 | Missing larsj User | Graceful handling |
| TC-DIF-BL-P1-003 | Specification Query | Advanced search support |
| TC-DIF-BL-P1-004 | Gmail Addon Query | Integration support |
| TC-DIF-BL-P1-005 | Count Verification | Accurate reporting |
| TC-DIF-BL-P1-006 | No Updates Needed | Idempotency |
| TC-DIF-BL-P1-007 | Mixed UserIds Performance | System responsiveness |
| TC-DIF-BL-P1-008 | UserInfo Repository | System user exists |

[Full details: DataImportFixes_TestCases.md](./DataImportFixes_TestCases.md)

---

## 🆕 Partner ErpDimValue Fix Test Cases (PR #477)

### P0 - Critical (7 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-EPF-BL-P0-001 | Fix Partners > 9999 | Correct invalid values |
| TC-EPF-BL-P0-002 | Skip Reserved Range | 8000-9999 protection |
| TC-EPF-BL-P0-003 | Include Soft-Deleted | Uniqueness across all |
| TC-EPF-BL-P0-004 | Sequential Assignment | Order preservation |
| TC-EPF-BL-P0-005 | Uniqueness After Fix | No duplicates |
| TC-EPF-BL-P0-006 | No Partners to Fix | Graceful empty case |
| TC-EPF-BL-P0-007 | LastModifiedBy System | Audit trail |

### P1 - High (6 tests)
| ID | Test Case | Business Rule |
|----|-----------|---------------|
| TC-EPF-BL-P1-001 | Large Batch | Performance |
| TC-EPF-BL-P1-002 | Highest Valid Calc | Range boundary |
| TC-EPF-BL-P1-003 | Empty Database | No valid partners |
| TC-EPF-BL-P1-004 | Reserved Unchanged | Special partner protection |
| TC-EPF-BL-P1-005 | Idempotency | Safe reruns |
| TC-EPF-BL-P1-006 | Console Output | Migration monitoring |

[Full details: PartnerErpDimValueFix_TestCases.md](./PartnerErpDimValueFix_TestCases.md)

---

## Test Execution Guidance

### Priority-Based Execution Order

1. **Phase 1**: Execute all P0 Critical tests (53 tests)
   - These validate core business functionality
   - Any failures here are blockers

2. **Phase 2**: Execute all P1 High tests (51 tests)
   - These validate important business rules
   - Failures should be addressed promptly

3. **Phase 3**: Execute P2 Medium tests (23 tests)
   - Secondary functionality validation
   - May proceed with known issues

4. **Phase 4**: Execute P3 Low tests (16 tests)
   - Edge cases and nice-to-have scenarios
   - Low priority for fixes

### Test Environment Requirements

- **Database**: PostgreSQL test instance
- **Mocking Framework**: Moq for dependencies
- **Test Framework**: xUnit with FluentAssertions
- **In-Memory Database**: For unit tests
- **Test Users**: Multiple roles configured

### Continuous Integration

- Run P0 tests on every commit
- Run P0+P1 tests on pull request
- Run full suite nightly

---

## Related Documentation

- [Unit Tests Implementation](../../tests/UNOPS.PAO.Business.Tests/)
- [Existing QA Test Cases](../Business/)
- [PRD Documentation](../../docs/Development/)
- [Security & RBAC](../../docs/Security/)

