# ValuesManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `ValuesManager`  
**Location**: `UNOPS.PAO.Business/Managers/ValuesManager.cs`  
**Purpose**: Provides access to lookup values including currencies, countries, partners, contacts, users, and organization units.

---

## Functional Test Cases (20+ Cases)

### TC-VM-F001-020: Core Operations
- F001: Get currencies - returns all currencies
- F002: Get currencies - maps to CurrencyModel correctly
- F003: Get eligible entities - returns all eligible entities
- F004: Get eligible entities - maps to EligibleEntityModel
- F005: Get countries - returns all countries
- F006: Get countries - maps to CountryModel correctly
- F007: Get partners - returns IQueryable of partners
- F008: Get partners - maps to PartnerValueModel
- F009: Get partners for filtering - returns raw Partner entities
- F010: Get organization units - filters by OrgUnit type
- F011: Get organization units - maps to OrganizationHierarchyModel
- F012: Get contacts - returns all contacts
- F013: Get contacts - maps to ContactValueModel
- F014: Get users - returns all users
- F015: Get users - maps to UserValueModel
- F016: Get users paged - pagination works correctly
- F017: Get users paged - search term filtering
- F018: Get users paged - active only filtering
- F019: Get users paged - selected user IDs inclusion
- F020: Search users async - returns limited results

---

## Performance Test Cases (10 Cases)

### TC-VM-P001: Get Currencies - Response Time
**Performance Criteria**: < 100ms for all currencies

### TC-VM-P002: Get Countries - Response Time
**Performance Criteria**: < 150ms for all countries

### TC-VM-P003: Get Partners - Response Time
**Performance Criteria**: < 500ms for partner query

### TC-VM-P004: Get Users - Response Time
**Performance Criteria**: < 300ms for all users

### TC-VM-P005: Get Users Paged - Response Time
**Performance Criteria**: < 200ms per page

### TC-VM-P006: Search Users Async - Response Time
**Performance Criteria**: < 150ms for autocomplete

### TC-VM-P007: Get Contacts - Large Dataset
**Performance Criteria**: < 1000ms for 5000 contacts

### TC-VM-P008: Get Organization Units - Response Time
**Performance Criteria**: < 200ms with type filter

### TC-VM-P009: Get Liaison Offices - Response Time
**Performance Criteria**: < 100ms for offices

### TC-VM-P010: Concurrent Value Lookups
**Performance Criteria**: 20 concurrent calls < 400ms each

---

## Concurrency Test Cases (10 Cases)

### TC-VM-C001: Concurrent Get Currencies
**Scenario**: 20 threads fetching currencies

### TC-VM-C002: Concurrent Get Countries
**Scenario**: 20 threads fetching countries

### TC-VM-C003: Concurrent Get Partners
**Scenario**: 15 threads querying partners

### TC-VM-C004: Concurrent Get Users
**Scenario**: 15 threads fetching users

### TC-VM-C005: Concurrent Paged User Queries
**Scenario**: 10 threads requesting different pages

### TC-VM-C006: Concurrent User Search
**Scenario**: 20 threads searching simultaneously

### TC-VM-C007: Mixed Value Type Requests
**Scenario**: Different value types requested concurrently

### TC-VM-C008: Repository Concurrent Access
**Scenario**: ValuesRepository thread safety

### TC-VM-C009: AutoMapper Concurrent Usage
**Scenario**: Mapper operations under load

### TC-VM-C010: High Load Lookup Scenario
**Scenario**: 100 concurrent value lookups

---

## Edge Cases (5 Cases)

### TC-VM-E001: Search Users with Special Characters
### TC-VM-E002: Pagination Beyond Available Data
### TC-VM-E003: Empty Search Term Returns Default Results
### TC-VM-E004: Selected User IDs Include Deleted Users
### TC-VM-E005: Very Large Page Size Request

