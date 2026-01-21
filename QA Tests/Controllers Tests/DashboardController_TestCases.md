# DashboardController Test Cases

**Controller**: `UNOPS.PAO.Presentation/Controllers/Dashboard/DashboardController.cs`  
**Priority**: P0 - Critical  
**Total Test Cases**: 30  

---

## Overview

The DashboardController provides dashboard data and analytics for the Opportunity+ system:
- Partner statistics and trends
- Contact activity summaries
- Interaction metrics
- Recent activity feeds
- Key performance indicators (KPIs)

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Dashboard Statistics | 10 | P0 |
| Activity Feeds | 6 | P1 |
| Analytics Endpoints | 8 | P1 |
| Authorization | 4 | P0 |
| Performance | 2 | P1 |

---

## P0 - Critical Tests

### TC-DASH-001: Get dashboard summary - authenticated user
**Description**: Retrieve dashboard summary for authenticated user  
**Preconditions**: 
- User authenticated
- User has dashboard access permission
**Test Steps**:
1. Authenticate as valid user
2. Call `GET /api/dashboard/summary`
3. Verify summary data returned
**Expected Result**: Dashboard summary with partner count, contact count, interaction count

### TC-DASH-002: Get dashboard summary - unauthorized user
**Description**: Unauthenticated request should fail  
**Preconditions**: No authentication token  
**Test Steps**:
1. Call `GET /api/dashboard/summary` without auth
2. Verify 401 response
**Expected Result**: 401 Unauthorized

### TC-DASH-003: Get partner statistics
**Description**: Retrieve partner-related statistics  
**Preconditions**: User authenticated with partner read permission  
**Test Steps**:
1. Create test partners with various statuses
2. Call `GET /api/dashboard/partners/stats`
3. Verify statistics accuracy
**Expected Result**: Returns counts by status, new partners this month, etc.

### TC-DASH-004: Get contact statistics
**Description**: Retrieve contact-related statistics  
**Preconditions**: User authenticated with contact read permission  
**Test Steps**:
1. Create test contacts
2. Call `GET /api/dashboard/contacts/stats`
3. Verify statistics
**Expected Result**: Returns total contacts, active contacts, new this month

### TC-DASH-005: Get interaction statistics
**Description**: Retrieve interaction-related statistics  
**Preconditions**: User authenticated with interaction read permission  
**Test Steps**:
1. Create test interactions
2. Call `GET /api/dashboard/interactions/stats`
3. Verify statistics
**Expected Result**: Returns interaction counts by type, recent activity

### TC-DASH-006: Dashboard respects org unit filter
**Description**: Dashboard data filtered by user's org unit  
**Preconditions**: 
- User with org unit assignment
- Data exists across multiple org units
**Test Steps**:
1. Create data in user's org unit and other org units
2. Call dashboard summary
3. Verify only user's org unit data returned
**Expected Result**: Only data from user's accessible org units included

### TC-DASH-007: Dashboard with no data
**Description**: Dashboard handles empty data gracefully  
**Preconditions**: User has no accessible data  
**Test Steps**:
1. Authenticate as user with no data access
2. Call dashboard summary
3. Verify zero counts returned
**Expected Result**: Returns zeros, no errors

### TC-DASH-008: Get KPI metrics
**Description**: Retrieve key performance indicators  
**Preconditions**: User authenticated  
**Test Steps**:
1. Call `GET /api/dashboard/kpis`
2. Verify KPI data structure
**Expected Result**: Returns KPIs with current values and trends

### TC-DASH-009: Dashboard date range filter
**Description**: Filter dashboard data by date range  
**Preconditions**: Historical data exists  
**Test Steps**:
1. Create data across multiple months
2. Call `GET /api/dashboard/summary?startDate=2025-01-01&endDate=2025-01-31`
3. Verify only January data included
**Expected Result**: Data filtered to specified date range

### TC-DASH-010: Dashboard with invalid date range
**Description**: Handle invalid date range gracefully  
**Preconditions**: None  
**Test Steps**:
1. Call `GET /api/dashboard/summary?startDate=2025-12-31&endDate=2025-01-01`
2. Verify appropriate error
**Expected Result**: 400 Bad Request with validation message

---

## P1 - High Priority Tests

### TC-DASH-011: Get recent activity feed
**Description**: Retrieve recent activity across entities  
**Preconditions**: Various entity activities exist  
**Test Steps**:
1. Create partners, contacts, interactions
2. Call `GET /api/dashboard/activity/recent`
3. Verify activity feed
**Expected Result**: Returns recent activities sorted by date

### TC-DASH-012: Activity feed pagination
**Description**: Paginate activity feed results  
**Preconditions**: Many activities exist  
**Test Steps**:
1. Create 50 activities
2. Call `GET /api/dashboard/activity/recent?page=1&pageSize=10`
3. Verify pagination
**Expected Result**: Returns 10 items with pagination metadata

### TC-DASH-013: Get partner trend data
**Description**: Retrieve partner growth trends  
**Preconditions**: Historical partner data exists  
**Test Steps**:
1. Create partners over multiple months
2. Call `GET /api/dashboard/partners/trends`
3. Verify trend data
**Expected Result**: Monthly partner counts for trend chart

### TC-DASH-014: Get interaction type breakdown
**Description**: Retrieve interaction distribution by type  
**Preconditions**: Interactions of various types exist  
**Test Steps**:
1. Create interactions: 10 meetings, 5 calls, 3 emails
2. Call `GET /api/dashboard/interactions/breakdown`
3. Verify breakdown
**Expected Result**: Returns counts per interaction type

### TC-DASH-015: Get pending approvals count
**Description**: Retrieve count of items pending user approval  
**Preconditions**: User is approver for some items  
**Test Steps**:
1. Create items pending user's approval
2. Call `GET /api/dashboard/pending-approvals`
3. Verify count
**Expected Result**: Correct count of pending approvals

### TC-DASH-016: Get user's recent items
**Description**: Retrieve items recently viewed/edited by user  
**Preconditions**: User has activity history  
**Test Steps**:
1. User views/edits some items
2. Call `GET /api/dashboard/my-recent`
3. Verify recent items
**Expected Result**: Returns user's recently accessed items

### TC-DASH-017: Dashboard widget data
**Description**: Retrieve data for specific dashboard widget  
**Preconditions**: Widget configuration exists  
**Test Steps**:
1. Call `GET /api/dashboard/widgets/partner-status-chart`
2. Verify widget data format
**Expected Result**: Data formatted for chart rendering

### TC-DASH-018: Get upcoming deadlines
**Description**: Retrieve upcoming deadline items  
**Preconditions**: Items with deadlines exist  
**Test Steps**:
1. Create items with future deadlines
2. Call `GET /api/dashboard/deadlines/upcoming`
3. Verify deadline items
**Expected Result**: Items sorted by deadline date

---

## Authorization Tests

### TC-DASH-A001: Admin sees all org unit data
**Description**: Admin user sees data across all org units  
**Preconditions**: User has admin role  
**Test Steps**:
1. Create data across multiple org units
2. Call dashboard as admin
3. Verify all data visible
**Expected Result**: Aggregated data from all org units

### TC-DASH-A002: User without dashboard permission
**Description**: User without dashboard permission denied  
**Preconditions**: User lacks dashboard permission  
**Test Steps**:
1. Authenticate as restricted user
2. Call `GET /api/dashboard/summary`
3. Verify access denied
**Expected Result**: 403 Forbidden

### TC-DASH-A003: Delegation affects dashboard
**Description**: Delegated user sees delegator's data  
**Preconditions**: User has active delegation  
**Test Steps**:
1. Set up delegation
2. Call dashboard as delegate
3. Verify delegator's data visible
**Expected Result**: Dashboard shows delegated access data

### TC-DASH-A004: Role-based widget visibility
**Description**: Widgets shown based on user role  
**Preconditions**: Widgets configured with role requirements  
**Test Steps**:
1. Call dashboard as user with limited role
2. Verify only permitted widgets returned
**Expected Result**: Only role-appropriate widgets included

---

## Performance Tests

### TC-DASH-P001: Dashboard summary performance
**Description**: Dashboard loads within acceptable time  
**Preconditions**: 50,000 partners, 100,000 contacts  
**Performance Criteria**: Complete in < 2 seconds

### TC-DASH-P002: Activity feed performance
**Description**: Activity feed loads quickly  
**Preconditions**: 1,000,000 activity records  
**Performance Criteria**: Complete in < 1 second

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/Integration Tests/Controllers/DashboardControllerTests.cs`

