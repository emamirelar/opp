# GlobalController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Shared/GlobalController.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 15  

---

## Overview

The GlobalController provides global/cross-cutting functionality:
- Global search across entities
- System health checks
- Application metadata
- Session management

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Global Search | 6 | P1 |
| Health/Status | 4 | P0 |
| Metadata | 3 | P2 |
| Authorization | 2 | P0 |

---

## P0 - Critical Tests

### TC-GC-001: Health check
**Description**: Application health  
**Test Steps**:
1. Call `GET /api/health`
**Expected Result**: Health status

### TC-GC-002: Readiness check
**Description**: Ready to serve  
**Test Steps**:
1. Call `GET /api/health/ready`
**Expected Result**: Readiness status

### TC-GC-003: Liveness check
**Description**: Application alive  
**Test Steps**:
1. Call `GET /api/health/live`
**Expected Result**: Alive status

### TC-GC-004: Database connectivity
**Description**: DB connection check  
**Test Steps**:
1. Call `GET /api/health/db`
**Expected Result**: DB status

---

## P1 - High Priority Tests

### TC-GC-005: Global search
**Description**: Search across entities  
**Test Steps**:
1. Call `GET /api/search?q=UNOPS`
**Expected Result**: Results from all entities

### TC-GC-006: Global search - partners
**Description**: Search returns partners  
**Test Steps**:
1. Search for partner name
**Expected Result**: Partner results

### TC-GC-007: Global search - contacts
**Description**: Search returns contacts  
**Test Steps**:
1. Search for contact name
**Expected Result**: Contact results

### TC-GC-008: Global search - interactions
**Description**: Search returns interactions  
**Test Steps**:
1. Search for interaction subject
**Expected Result**: Interaction results

### TC-GC-009: Global search - pagination
**Description**: Paginated results  
**Test Steps**:
1. Search with page params
**Expected Result**: Paginated response

### TC-GC-010: Global search - entity filter
**Description**: Filter by entity type  
**Test Steps**:
1. Call with entityTypes=Partner,Contact
**Expected Result**: Filtered results

---

## P2 - Medium Priority Tests

### TC-GC-011: Get application version
**Description**: App version info  
**Test Steps**:
1. Call `GET /api/version`
**Expected Result**: Version details

### TC-GC-012: Get system info
**Description**: System metadata  
**Test Steps**:
1. Call `GET /api/system-info`
**Expected Result**: System info

### TC-GC-013: Get current time
**Description**: Server time  
**Test Steps**:
1. Call `GET /api/time`
**Expected Result**: UTC timestamp

---

## Authorization Tests

### TC-GC-A001: Health endpoints public
**Expected Result**: 200 without auth

### TC-GC-A002: Search requires auth
**Expected Result**: 401 without auth

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/GlobalControllerTests.cs`

