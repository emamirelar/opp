# Data Integrity Test Cases

**Priority**: P0 - Critical  
**Total Test Cases**: 30  

---

## Overview

Data integrity tests covering:
- Referential integrity (FK constraints)
- Orphaned records
- Circular references
- Cascade behaviors
- Soft delete consistency
- Data consistency across operations

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Referential Integrity | 10 | P0 |
| Orphan Prevention | 6 | P0 |
| Cascade Behaviors | 6 | P1 |
| Soft Delete | 5 | P0 |
| Data Consistency | 3 | P1 |

---

## Referential Integrity Tests

### TC-DI-001: FK constraint on create
**Description**: Can't reference non-existent parent  
**Test Steps**:
1. Create contact with invalid partnerId
**Expected Result**: FK violation error

### TC-DI-002: FK constraint on delete
**Description**: Can't delete referenced parent  
**Test Steps**:
1. Delete partner with contacts
**Expected Result**: Constraint error or cascade

### TC-DI-003: Valid FK accepted
**Description**: Valid reference works  
**Test Steps**:
1. Create contact with valid partnerId
**Expected Result**: Contact created

### TC-DI-004: Nullable FK allowed
**Description**: Optional relationship  
**Test Steps**:
1. Create entity without optional FK
**Expected Result**: Created with null FK

### TC-DI-005: Self-referencing FK
**Description**: Parent/child same table  
**Test Steps**:
1. Create org unit with parent
**Expected Result**: Valid hierarchy

### TC-DI-006: Circular reference prevented
**Description**: A parent of B, B parent of A  
**Test Steps**:
1. Set A.parentId = B
2. Set B.parentId = A
**Expected Result**: Validation error

### TC-DI-007: Deep hierarchy limit
**Description**: Max nesting depth  
**Test Steps**:
1. Create 100-level hierarchy
**Expected Result**: Depth limit enforced

### TC-DI-008: Multi-FK consistency
**Description**: Entity with multiple FKs  
**Test Steps**:
1. All FKs must be valid
**Expected Result**: All validated

### TC-DI-009: Junction table integrity
**Description**: Many-to-many valid  
**Test Steps**:
1. Create partner-category link
**Expected Result**: Both IDs must exist

### TC-DI-010: Composite key integrity
**Description**: Composite FK validation  
**Test Steps**:
1. Reference composite key
**Expected Result**: All parts valid

---

## Orphan Prevention Tests

### TC-DI-011: Delete removes children
**Description**: Cascade delete  
**Test Steps**:
1. Delete partner
2. Check contacts
**Expected Result**: Contacts deleted or orphan prevented

### TC-DI-012: Soft delete children
**Description**: Soft delete cascades  
**Test Steps**:
1. Soft delete partner
2. Check contacts
**Expected Result**: Contacts soft deleted

### TC-DI-013: Orphan detection query
**Description**: Find orphaned records  
**Test Steps**:
1. Run orphan detection
**Expected Result**: Report any orphans

### TC-DI-014: Orphan cleanup job
**Description**: Scheduled cleanup  
**Test Steps**:
1. Create orphan manually
2. Run cleanup job
**Expected Result**: Orphan removed

### TC-DI-015: Prevent new orphan on update
**Description**: Update removes parent ref  
**Test Steps**:
1. Update to remove required FK
**Expected Result**: Validation error

### TC-DI-016: Reparent instead of orphan
**Description**: Move to different parent  
**Test Steps**:
1. Change parentId to valid parent
**Expected Result**: Reparented

---

## Cascade Behavior Tests

### TC-DI-017: Cascade delete works
**Description**: ON DELETE CASCADE  
**Test Steps**:
1. Delete parent
2. Check children
**Expected Result**: Children deleted

### TC-DI-018: Cascade update works
**Description**: ON UPDATE CASCADE  
**Test Steps**:
1. Update parent key
2. Check child FKs
**Expected Result**: Child FKs updated

### TC-DI-019: Set null on delete
**Description**: ON DELETE SET NULL  
**Test Steps**:
1. Delete referenced entity
2. Check referencing entity
**Expected Result**: FK set to null

### TC-DI-020: Restrict delete
**Description**: ON DELETE RESTRICT  
**Test Steps**:
1. Delete referenced entity
**Expected Result**: Delete blocked

### TC-DI-021: No action
**Description**: ON DELETE NO ACTION  
**Test Steps**:
1. Delete with children
**Expected Result**: FK error

### TC-DI-022: Multi-level cascade
**Description**: Grandchildren affected  
**Test Steps**:
1. Delete grandparent
2. Check all levels
**Expected Result**: All cascaded

---

## Soft Delete Tests

### TC-DI-023: Soft delete sets flag
**Description**: IsDeleted = true  
**Test Steps**:
1. Delete entity
2. Check flag
**Expected Result**: IsDeleted = true

### TC-DI-024: Soft deleted excluded from queries
**Description**: Default filter  
**Test Steps**:
1. Soft delete entity
2. Query without filter
**Expected Result**: Not returned

### TC-DI-025: Include deleted option
**Description**: Can query deleted  
**Test Steps**:
1. Query with includeDeleted
**Expected Result**: Deleted returned

### TC-DI-026: Restore soft deleted
**Description**: Undelete  
**Test Steps**:
1. Soft delete
2. Restore
**Expected Result**: IsDeleted = false

### TC-DI-027: Hard delete option
**Description**: Permanent removal  
**Test Steps**:
1. Hard delete
**Expected Result**: Record removed

---

## Data Consistency Tests

### TC-DI-028: Computed field accuracy
**Description**: Derived values correct  
**Test Steps**:
1. Update source fields
2. Check computed
**Expected Result**: Computed updated

### TC-DI-029: Aggregate consistency
**Description**: Count/sum fields  
**Test Steps**:
1. Add/remove children
2. Check parent count
**Expected Result**: Count accurate

### TC-DI-030: Denormalized data sync
**Description**: Cached/copied data  
**Test Steps**:
1. Update source
2. Check copies
**Expected Result**: All in sync

---

**Last Updated**: December 18, 2025

