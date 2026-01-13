# UserDataManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `UserDataManager`  
**Location**: `UNOPS.PAO.Business/Managers/UserDataManager.cs`  
**Purpose**: User data retrieval and authentication context management.

---

## Functional Test Cases (20+)
- F001: Get user by ID - exists
- F002: Get user by ID - not found
- F003: Get user by email - exists
- F004: Get user by email - case insensitive
- F005: Get user by email - not found
- F006: Get current user - authenticated
- F007: Get current user - not authenticated
- F008: Get current user - from NameIdentifier claim
- F009: Get current user - from Email claim fallback
- F010: Get current user - invalid user ID format
- F011: Get users by emails - multiple users
- F012: Get users by emails - empty list
- F013: Get users by emails - null input
- F014: Get users by emails - case insensitive matching
- F015: Get users by emails - partial matches (some found, some not)
- F016: Get users by emails - duplicate emails in request
- F017: Get user - with profile data
- F018: Get current user - expired session
- F019: Get users by emails - large batch (100 emails)
- F020: User ID parsing - non-integer claim value

---

## Performance Test Cases (10)
- P001: Get user by ID - response time < 100ms
- P002: Get user by email - response time < 150ms (with index)
- P003: Get current user - response time < 50ms (cached claims)
- P004: Get users by emails - bulk lookup < 500ms for 100 users
- P005: Concurrent user lookups - 50 threads < 200ms each
- P006: Get current user - HTTP context access < 20ms
- P007: Email matching - case-insensitive performance < 200ms on 10K users
- P008: Batch user retrieval - throughput > 200 users/second
- P009: User by ID - cache effectiveness test
- P010: Claims parsing - performance < 5ms

---

## Concurrency Test Cases (10)
- C001: Concurrent get user by ID - same user, 20 threads
- C002: Concurrent get current user - multiple sessions
- C003: Concurrent email lookups - different emails
- C004: Concurrent email lookups - same email
- C005: Get users by emails - concurrent batch requests
- C006: Current user lookup - session expiry race condition
- C007: Concurrent user queries during user update
- C008: HTTP context access - thread safety
- C009: Claims reading - concurrent access
- C010: Bulk email lookup - concurrent large batches

---

## Edge Cases (5)
- E001: Claim with null/empty NameIdentifier
- E002: Email claim with special characters
- E003: User ID claim not parseable as integer
- E004: HTTP context is null
- E005: User email with + addressing (gmail style)

