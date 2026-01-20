# UNOPSEntityConfigurationManager - Unit Test Cases

**Manager**: `UNOPSEntityConfigurationManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSEntityConfigurationManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UNOPSEntityConfigurationManager` with focus on:
- Entity field configuration
- Dynamic field management
- Validation rules
- Entity metadata
- Configuration persistence

**Total Test Cases**: 15+

---

## 1. Entity Configuration Tests

### TC-EC-001: Get Entity Configuration
**Test**: `GetEntityConfig_Should_ReturnConfig_When_EntityExists`

### TC-EC-002: Update Entity Configuration
**Test**: `UpdateEntityConfig_Should_SaveChanges_When_ValidConfigProvided`

### TC-EC-003: Create New Entity Configuration
**Test**: `CreateEntityConfig_Should_AddConfig_When_NewEntityProvided`

### TC-EC-004: Delete Entity Configuration
**Test**: `DeleteEntityConfig_Should_RemoveConfig_When_EntityConfigExists`

### TC-EC-005: List All Entity Configurations
**Test**: `GetAllEntityConfigs_Should_ReturnAll_When_ConfigsExist`

---

## 2. Field Configuration Tests

### TC-EC-006: Add Dynamic Field
**Test**: `AddField_Should_CreateField_When_ValidFieldProvided`

### TC-EC-007: Update Field Configuration
**Test**: `UpdateField_Should_ModifyField_When_FieldExists`

### TC-EC-008: Remove Dynamic Field
**Test**: `RemoveField_Should_DeleteField_When_FieldExists`

### TC-EC-009: Get Field Configuration
**Test**: `GetFieldConfig_Should_ReturnConfig_When_FieldExists`

### TC-EC-010: Validate Field Type
**Test**: `ValidateField_Should_ThrowException_When_InvalidFieldType`

---

## 3. Validation Rules Tests

### TC-EC-011: Add Validation Rule
**Test**: `AddValidationRule_Should_CreateRule_When_ValidRuleProvided`

### TC-EC-012: Update Validation Rule
**Test**: `UpdateValidationRule_Should_ModifyRule_When_RuleExists`

### TC-EC-013: Remove Validation Rule
**Test**: `RemoveValidationRule_Should_DeleteRule_When_RuleExists`

### TC-EC-014: Apply Validation Rules
**Test**: `ApplyValidation_Should_ValidateData_When_RulesExist`

### TC-EC-015: Validation Rule Inheritance
**Test**: `ApplyValidation_Should_InheritRules_When_EntityHasParent`

---

## Coverage Goals
**Overall**: 15+ tests, 75%+ coverage

