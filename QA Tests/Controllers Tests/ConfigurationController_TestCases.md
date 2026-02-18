# ConfigurationController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Configuration/ConfigurationController.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 15  

---

## Overview

The ConfigurationController manages application configuration:
- System settings retrieval
- Feature flags
- Application metadata

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Settings Retrieval | 6 | P0 |
| Feature Flags | 4 | P1 |
| Admin Settings | 5 | P0 |

---

## P0 - Critical Tests

### TC-CFG-001: Get public configuration
**Description**: Get public app settings  
**Test Steps**:
1. Call `GET /api/configuration/public`
**Expected Result**: Public settings

### TC-CFG-002: Get authenticated configuration
**Description**: Get user-specific settings  
**Test Steps**:
1. Call `GET /api/configuration` as authenticated
**Expected Result**: Full settings

### TC-CFG-003: Get application version
**Description**: Get app version info  
**Test Steps**:
1. Call `GET /api/configuration/version`
**Expected Result**: Version info

### TC-CFG-004: Get supported languages
**Description**: List languages  
**Test Steps**:
1. Call `GET /api/configuration/languages`
**Expected Result**: Language list

### TC-CFG-005: Get environment info
**Description**: Get environment name  
**Test Steps**:
1. Call `GET /api/configuration/environment`
**Expected Result**: Environment (dev/test/prod)

### TC-CFG-006: Admin get all settings
**Description**: Admin retrieves all  
**Test Steps**:
1. Call `GET /api/admin/configuration` as admin
**Expected Result**: All settings

---

## P1 - High Priority Tests

### TC-CFG-007: Get feature flags
**Description**: List feature flags  
**Test Steps**:
1. Call `GET /api/configuration/features`
**Expected Result**: Feature flags

### TC-CFG-008: Check specific feature
**Description**: Check if feature enabled  
**Test Steps**:
1. Call `GET /api/configuration/features/{featureKey}`
**Expected Result**: Boolean response

### TC-CFG-009: Update feature flag (admin)
**Description**: Toggle feature  
**Test Steps**:
1. Call `PUT /api/admin/configuration/features/{key}`
**Expected Result**: Feature updated

### TC-CFG-010: Feature flags affect behavior
**Description**: Disabled feature blocks access  
**Test Steps**:
1. Disable feature
2. Try access feature
**Expected Result**: Feature unavailable

### TC-CFG-011: Update system setting (admin)
**Description**: Modify setting  
**Test Steps**:
1. Call `PUT /api/admin/configuration/{key}`
**Expected Result**: Setting updated

### TC-CFG-012: Validate setting value
**Description**: Invalid value rejected  
**Test Steps**:
1. Set invalid value
**Expected Result**: Validation error

### TC-CFG-013: Get setting history
**Description**: Audit trail  
**Test Steps**:
1. Call `GET /api/admin/configuration/{key}/history`
**Expected Result**: Change history

### TC-CFG-014: Cache invalidation
**Description**: Settings refresh  
**Test Steps**:
1. Update setting
2. Verify new value used
**Expected Result**: New value active

### TC-CFG-015: Get maintenance status
**Description**: Check maintenance mode  
**Test Steps**:
1. Call `GET /api/configuration/maintenance`
**Expected Result**: Maintenance status

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/ConfigurationControllerTests.cs`

