# ProfileManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `ProfileManager`  
**Location**: `UNOPS.PAO.Business/Managers/ProfileManager.cs`  
**Purpose**: Manages user profile retrieval and updates.

---

## Functional Test Cases (20+ Cases)

### TC-PM-F001-020: Core Operations
- F001: Get profile - existing user by email
- F002: Get profile - user not found throws BusinessException
- F003: Get profile - null email throws BusinessException
- F004: Get profile - returns correct FirstName
- F005: Get profile - returns correct LastName
- F006: Get profile - returns correct Email
- F007: Get profile - user with null UserProfile returns empty strings
- F008: Update profile - existing user
- F009: Update profile - user not found throws BusinessException
- F010: Update profile - creates UserProfile if null
- F011: Update profile - updates FirstName correctly
- F012: Update profile - updates LastName correctly
- F013: Update profile - saves changes to database
- F014: Get profile - case sensitivity of email lookup
- F015: Update profile - null profile model handling
- F016: Update profile - empty FirstName
- F017: Update profile - empty LastName
- F018: Get profile - email with leading/trailing whitespace
- F019: Update profile - preserves other profile data
- F020: Get profile - after update reflects changes

---

## Performance Test Cases (10 Cases)

### TC-PM-P001: Get Profile - Response Time
**Performance Criteria**: < 100ms per lookup

### TC-PM-P002: Update Profile - Response Time
**Performance Criteria**: < 200ms per update

### TC-PM-P003: Get Profile - With UserProfile Join
**Performance Criteria**: < 150ms with navigation property

### TC-PM-P004: Update Profile - SaveChanges Performance
**Performance Criteria**: < 100ms for save operation

### TC-PM-P005: Concurrent Profile Gets
**Performance Criteria**: 20 concurrent reads < 150ms each

### TC-PM-P006: Concurrent Profile Updates
**Performance Criteria**: 10 concurrent updates < 300ms each

### TC-PM-P007: Get Profile - Database Round Trip
**Performance Criteria**: < 50ms for query execution

### TC-PM-P008: Update Profile - Transaction Overhead
**Performance Criteria**: < 50ms transaction overhead

### TC-PM-P009: Email Lookup Performance
**Performance Criteria**: < 30ms with email index

### TC-PM-P010: Profile Model Creation
**Performance Criteria**: < 5ms for object instantiation

---

## Concurrency Test Cases (10 Cases)

### TC-PM-C001: Concurrent Get Profile - Same User
**Scenario**: 10 threads getting same user profile

### TC-PM-C002: Concurrent Get Profile - Different Users
**Scenario**: 20 threads getting different profiles

### TC-PM-C003: Concurrent Update Profile - Same User
**Scenario**: 5 threads updating same profile

### TC-PM-C004: Concurrent Update Profile - Different Users
**Scenario**: 15 threads updating different profiles

### TC-PM-C005: Get During Update
**Scenario**: Reading profile while being updated

### TC-PM-C006: Concurrent UserProfile Creation
**Scenario**: Multiple threads creating UserProfile for same user

### TC-PM-C007: Concurrent SaveChanges
**Scenario**: Multiple threads saving changes simultaneously

### TC-PM-C008: DbContext Thread Safety
**Scenario**: Verify context usage across threads

### TC-PM-C009: High Load Profile Operations
**Scenario**: 50 concurrent get/update operations

### TC-PM-C010: Database Lock Contention
**Scenario**: Test for deadlock scenarios

---

## Edge Cases (5 Cases)

### TC-PM-E001: Email Address with Special Characters
### TC-PM-E002: Very Long FirstName/LastName
### TC-PM-E003: Profile with Unicode Characters
### TC-PM-E004: Empty Email String
### TC-PM-E005: User Deleted During Profile Update

