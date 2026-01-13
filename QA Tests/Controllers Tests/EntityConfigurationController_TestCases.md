# EntityConfigurationController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Admin/EntityConfigurationController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 30  

---

## Overview

The EntityConfigurationController manages dynamic entity configuration:
- Entity field definitions
- Validation rules
- Display configurations
- Custom field management

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Configuration CRUD | 10 | P0 |
| Field Management | 8 | P0 |
| Validation Rules | 7 | P1 |
| Authorization | 5 | P0 |

---

## P0 - Critical Tests

### TC-EC-001: Get entity configuration
**Description**: Get configuration for entity type  
**Test Steps**:
1. Call `GET /api/admin/entity-config/{entityType}`
**Expected Result**: Complete configuration

### TC-EC-002: Get all entity configurations
**Description**: List all entity configurations  
**Test Steps**:
1. Call `GET /api/admin/entity-config`
**Expected Result**: All configurations

### TC-EC-003: Update entity configuration
**Description**: Update entity configuration  
**Test Steps**:
1. Call `PUT /api/admin/entity-config/{entityType}`
**Expected Result**: Configuration updated

### TC-EC-004: Get field definitions
**Description**: Get fields for entity type  
**Test Steps**:
1. Call `GET /api/admin/entity-config/{entityType}/fields`
**Expected Result**: Field definitions

### TC-EC-005: Add custom field
**Description**: Add custom field to entity  
**Test Steps**:
1. Call `POST /api/admin/entity-config/{entityType}/fields`
**Expected Result**: Field added

### TC-EC-006: Update field definition
**Description**: Update field properties  
**Test Steps**:
1. Call `PUT /api/admin/entity-config/{entityType}/fields/{fieldId}`
**Expected Result**: Field updated

### TC-EC-007: Delete custom field
**Description**: Remove custom field  
**Test Steps**:
1. Call `DELETE /api/admin/entity-config/{entityType}/fields/{fieldId}`
**Expected Result**: Field removed

### TC-EC-008: Cannot delete system field
**Description**: System fields protected  
**Test Steps**:
1. Attempt delete system field
**Expected Result**: 400 Bad Request

### TC-EC-009: Get field by ID
**Description**: Get specific field  
**Test Steps**:
1. Call `GET /api/admin/entity-config/{entityType}/fields/{fieldId}`
**Expected Result**: Field details

### TC-EC-010: Reorder fields
**Description**: Change field display order  
**Test Steps**:
1. Call `PUT /api/admin/entity-config/{entityType}/fields/order`
**Expected Result**: Order updated

---

## P1 - High Priority Tests

### TC-EC-011: Get validation rules
**Description**: Get validation rules for field  
**Test Steps**:
1. Call `GET /api/admin/entity-config/{entityType}/fields/{fieldId}/validations`
**Expected Result**: Validation rules

### TC-EC-012: Add validation rule
**Description**: Add validation to field  
**Test Steps**:
1. Call `POST /api/admin/entity-config/{entityType}/fields/{fieldId}/validations`
**Expected Result**: Rule added

### TC-EC-013: Update validation rule
**Description**: Update existing rule  
**Test Steps**:
1. Call `PUT` validation endpoint
**Expected Result**: Rule updated

### TC-EC-014: Delete validation rule
**Description**: Remove validation rule  
**Test Steps**:
1. Call `DELETE` validation endpoint
**Expected Result**: Rule removed

### TC-EC-015: Get display configuration
**Description**: Get UI display settings  
**Test Steps**:
1. Call display config endpoint
**Expected Result**: Display settings

### TC-EC-016: Update display configuration
**Description**: Update UI settings  
**Test Steps**:
1. Call `PUT` display config
**Expected Result**: Settings updated

### TC-EC-017: Clone entity configuration
**Description**: Clone config to new entity  
**Test Steps**:
1. Call clone endpoint
**Expected Result**: Configuration cloned

### TC-EC-018: Export configuration
**Description**: Export config to JSON  
**Test Steps**:
1. Call export endpoint
**Expected Result**: JSON export

### TC-EC-019: Import configuration
**Description**: Import config from JSON  
**Test Steps**:
1. Call `POST` import endpoint
**Expected Result**: Configuration imported

### TC-EC-020: Get configuration history
**Description**: View configuration changes  
**Test Steps**:
1. Call history endpoint
**Expected Result**: Change history

---

## Authorization Tests

### TC-EC-A001: Non-admin cannot access
**Expected Result**: 403 Forbidden

### TC-EC-A002: Admin can manage
**Expected Result**: 200 OK

### TC-EC-A003: View-only access
**Expected Result**: Can read, cannot modify

### TC-EC-A004: Audit trail created
**Expected Result**: Changes logged

### TC-EC-A005: Prevent breaking changes
**Expected Result**: Destructive changes require confirmation

---

## Validation Tests

### TC-EC-V001: Field name uniqueness
**Description**: Field names must be unique

### TC-EC-V002: Field type validation
**Description**: Valid field types only

### TC-EC-V003: Validation rule syntax
**Description**: Valid rule expressions

### TC-EC-V004: Required field constraint
**Description**: Required fields cannot be deleted

### TC-EC-V005: Reference integrity
**Description**: Cannot delete referenced fields

---

## Performance Tests

### TC-EC-P001: Get config < 100ms
**Performance Criteria**: < 100ms

### TC-EC-P002: Update config < 200ms
**Performance Criteria**: < 200ms

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/EntityConfigurationControllerTests.cs`

