# Analytics Controllers Test Cases

**Controllers**: 
- `UNOPS.PAO.Presentation/Controllers/Partners/PartnerAnalyticsController.cs`
- `UNOPS.PAO.Presentation/Controllers/Contacts/ContactAnalyticsController.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 50 (25 per controller)  

---

## Overview

The Analytics Controllers provide analytics and reporting:
- Partner analytics and trends
- Contact activity metrics
- Historical data analysis
- Export capabilities

---

# PartnerAnalyticsController Tests

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Summary Statistics | 8 | P0 |
| Trend Analysis | 7 | P1 |
| Filtering & Grouping | 6 | P1 |
| Export | 4 | P2 |

---

### TC-PA-001: Get partner count summary
**Description**: Get total partner counts by status  
**Test Steps**:
1. Call `GET /api/partners/analytics/summary`
**Expected Result**: Counts by status (Active, Pending, Inactive)

### TC-PA-002: Get partner growth trend
**Description**: Get partner growth over time  
**Test Steps**:
1. Call `GET /api/partners/analytics/trends/growth?period=monthly`
**Expected Result**: Monthly growth data

### TC-PA-003: Get partners by region
**Description**: Partner distribution by region  
**Test Steps**:
1. Call `GET /api/partners/analytics/by-region`
**Expected Result**: Counts per region

### TC-PA-004: Get partners by type
**Description**: Partner distribution by type  
**Test Steps**:
1. Call `GET /api/partners/analytics/by-type`
**Expected Result**: Counts per partner type

### TC-PA-005: Get top partners by interactions
**Description**: Most active partners  
**Test Steps**:
1. Call `GET /api/partners/analytics/top-by-interactions?limit=10`
**Expected Result**: Top 10 partners

### TC-PA-006: Get partner activity heatmap
**Description**: Activity levels over time  
**Test Steps**:
1. Call `GET /api/partners/analytics/activity-heatmap`
**Expected Result**: Heatmap data structure

### TC-PA-007: Get new partners this period
**Description**: New partners in date range  
**Test Steps**:
1. Call with date range params
**Expected Result**: New partners count and list

### TC-PA-008: Get partner conversion rates
**Description**: Approval conversion metrics  
**Test Steps**:
1. Call `GET /api/partners/analytics/conversion-rates`
**Expected Result**: Conversion percentages

### TC-PA-009: Filter analytics by org unit
**Description**: Analytics scoped to org unit  
**Test Steps**:
1. Call with orgUnitId filter
**Expected Result**: Org unit scoped data

### TC-PA-010: Filter analytics by date range
**Description**: Analytics for specific period  
**Test Steps**:
1. Call with startDate and endDate
**Expected Result**: Period-specific data

### TC-PA-011: Compare periods
**Description**: Compare two time periods  
**Test Steps**:
1. Call compare endpoint with two periods
**Expected Result**: Period comparison data

### TC-PA-012: Export analytics to CSV
**Description**: Export analytics data  
**Test Steps**:
1. Call `GET /api/partners/analytics/export?format=csv`
**Expected Result**: CSV file download

### TC-PA-013: Export analytics to Excel
**Description**: Export to Excel format  
**Test Steps**:
1. Call with format=xlsx
**Expected Result**: Excel file download

---

# ContactAnalyticsController Tests

### TC-CA-001: Get contact count summary
**Description**: Total contact counts  
**Test Steps**:
1. Call `GET /api/contacts/analytics/summary`
**Expected Result**: Contact totals

### TC-CA-002: Get contact growth trend
**Description**: Contact growth over time  
**Test Steps**:
1. Call with period parameter
**Expected Result**: Growth trend data

### TC-CA-003: Get contacts by partner
**Description**: Contact distribution by partner  
**Test Steps**:
1. Call `GET /api/contacts/analytics/by-partner`
**Expected Result**: Counts per partner

### TC-CA-004: Get contact activity metrics
**Description**: Contact interaction activity  
**Test Steps**:
1. Call activity endpoint
**Expected Result**: Activity metrics

### TC-CA-005: Get most contacted
**Description**: Contacts with most interactions  
**Test Steps**:
1. Call with limit
**Expected Result**: Top contacts

### TC-CA-006: Get contact roles distribution
**Description**: Contacts by role/type  
**Test Steps**:
1. Call roles endpoint
**Expected Result**: Role distribution

### TC-CA-007: Get new contacts this period
**Description**: New contacts in date range  
**Test Steps**:
1. Call with date params
**Expected Result**: New contact data

### TC-CA-008: Get inactive contacts
**Description**: Contacts with no recent activity  
**Test Steps**:
1. Call `GET /api/contacts/analytics/inactive?days=90`
**Expected Result**: Inactive contacts list

### TC-CA-009: Filter analytics by partner
**Description**: Analytics for specific partner  
**Test Steps**:
1. Call with partnerId filter
**Expected Result**: Partner-scoped data

### TC-CA-010: Contact engagement score
**Description**: Engagement scoring for contacts  
**Test Steps**:
1. Call engagement endpoint
**Expected Result**: Scored contact list

### TC-CA-011: Export contact analytics
**Description**: Export contact analytics data  
**Test Steps**:
1. Call export endpoint
**Expected Result**: Export file

### TC-CA-012: Analytics with no data
**Description**: Handle empty data gracefully  
**Test Steps**:
1. Query for empty org unit
**Expected Result**: Zero counts, no errors

---

## Authorization Tests

### TC-ANA-A001: Analytics requires authentication
**Expected Result**: 401 for unauthenticated

### TC-ANA-A002: Org unit filter applied
**Expected Result**: User sees only permitted data

### TC-ANA-A003: Export requires permission
**Expected Result**: 403 without export permission

---

## Performance Tests

### TC-ANA-P001: Summary analytics < 1s
**Performance Criteria**: < 1 second

### TC-ANA-P002: Trend analysis < 2s
**Performance Criteria**: < 2 seconds for 1 year

### TC-ANA-P003: Export < 5s
**Performance Criteria**: < 5 seconds for 10K records

---

**Last Updated**: December 18, 2025  
**C# Test Files**: 
- `QA Tests/Integration Tests/Controllers/PartnerAnalyticsControllerTests.cs`
- `QA Tests/Integration Tests/Controllers/ContactAnalyticsControllerTests.cs`

