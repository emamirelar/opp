# ProfileManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/ProfileManager`  
**Created:** 2026-02-18 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Security (SEC) | 50 | 50 | ✅ |
| §7 Concurrency (CON) | 25 | 25 | ✅ |
| §8 Unit (UNT) | 21 | 21 | ✅ |
| §9 Performance (PRF) | 16 | 16 | ✅ |
| §10 Load (LDT) | 10 | 10 | ✅ |
| **TOTAL** | **462** | **462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result | Formula |
|-------|--------|---------|
| N≥3P? | ✅ PASS | 90 ≥ 3×30 (90) |
| E≥3P? | ✅ PASS | 90 ≥ 3×30 (90) |
| F≥3P? | ✅ PASS | 90 ≥ 3×30 (90) |
| I≥3P? | ✅ PASS | 90 ≥ 3×30 (90) |

---

## Implementation Status

| File | Path | Status |
|------|------|--------|
| Manager | UNOPS.PAO.Business/Managers/ProfileManager.cs | Implemented (no UNOPS override) |
| Controller | UNOPS.PAO.Presentation/Controllers/Users/UserProfileController.cs | Implemented |
| Model | UNOPS.PAO.Models/Users/ProfileModel.cs | Implemented (Email, FirstName, LastName) |
| Entity | UNOPS.PAO.Domain/Entities/UserProfile.cs | Implemented (inherits ModifiableDeletableEntity) |
| Entity | UNOPS.PAO.Domain/Entities/PAOUser.cs | Implemented (UserProfile 1:1 navigation) |
| Authorization | UNOPS.PAO.Presentation/ContextPermissionHandlers/ProfileAuthorizationHandler.cs | Implemented (Operations.Read only) |
| API Route | POST /api/profile | Active |
| API Route | GET /api/profile | Commented out in code |

---

## Feature Overview

**ProfileManager** manages user profile retrieval and updates via PAOUser and UserProfile (1:1). Key responsibilities: Get profile by email (sync), Update FirstName/LastName (async), create UserProfile when missing. Uses exact email match; no IsDeleted filter on PAOUser; relies on lazy loading for UserProfile. ProfileManager is injected directly (not in ManagerWrapper).

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Get profile by valid email — user with UserProfile | PAOUser exists with email user@unops.org, UserProfile has FirstName/LastName | ProfileManager.Get("user@unops.org") | ProfileModel with Email, FirstName, LastName | P0 |
| POS-002 | Get profile by valid email — user without UserProfile | PAOUser exists, UserProfile is null | ProfileManager.Get("newuser@unops.org") | ProfileModel with Email, FirstName="", LastName="" | P0 |
| POS-003 | Update profile — existing UserProfile | PAOUser has UserProfile with FirstName="John", LastName="Doe" | Update(ProfileModel{Email, FirstName="Jane", LastName="Smith"}) | FirstName/LastName persisted | P0 |
| POS-004 | Update profile — creates UserProfile when missing | PAOUser exists, UserProfile is null | Update(ProfileModel{Email, FirstName="Alice", LastName="Brown"}) | UserProfile created and persisted | P0 |
| POS-005 | Get profile — FirstName only | UserProfile has FirstName="John", LastName=null | Get(email) | FirstName="John", LastName="" | P1 |
| POS-006 | Get profile — LastName only | UserProfile has FirstName=null, LastName="Doe" | Get(email) | FirstName="", LastName="Doe" | P1 |
| POS-007 | Update profile — FirstName only | Valid profile with FirstName="Marie", LastName=null | Update(profile) | FirstName persisted, LastName unchanged | P1 |
| POS-008 | Update profile — LastName only | Valid profile with FirstName=null, LastName="Curie" | Update(profile) | LastName persisted | P1 |
| POS-009 | Update profile — both names empty | ProfileModel with FirstName="", LastName="" | Update(profile) | Empty strings persisted | P1 |
| POS-010 | Get profile — both names empty | UserProfile has FirstName="", LastName="" | Get(email) | ProfileModel with empty FirstName/LastName | P1 |
| POS-011 | Update profile — single character names | FirstName="A", LastName="B" | Update(profile) | Single chars persisted | P1 |
| POS-012 | Get profile — single character names | UserProfile has FirstName="A", LastName="B" | Get(email) | ProfileModel returned correctly | P1 |
| POS-013 | Update profile — names with spaces | FirstName="Mary Jane", LastName="van der Berg" | Update(profile) | Names with spaces persisted | P1 |
| POS-014 | Get profile — names with hyphens | UserProfile has LastName="O'Brien" | Get(email) | LastName returned correctly | P1 |
| POS-015 | Update profile — unicode names | FirstName="François", LastName="Müller" | Update(profile) | Unicode persisted | P1 |
| POS-016 | Get profile — unicode names | UserProfile has FirstName="José", LastName="García" | Get(email) | Unicode returned correctly | P1 |
| POS-017 | Update profile — long valid names | FirstName/LastName at 255 chars | Update(profile) | Long names persisted | P1 |
| POS-018 | Get profile — email with subaddress | PAOUser Email="user+tag@unops.org" | Get("user+tag@unops.org") | ProfileModel returned | P1 |
| POS-019 | Update profile — email with subaddress | ProfileModel Email="user+tag@unops.org" | Update(profile) | Update succeeds | P1 |
| POS-020 | Get profile — email case-sensitive match | PAOUser Email="User@UNOPS.org" (exact) | Get("User@UNOPS.org") | ProfileModel returned | P1 |
| POS-021 | Update profile — overwrite existing values | UserProfile has FirstName="Old", LastName="Name" | Update with FirstName="New", LastName="Name" | Only FirstName changed | P1 |
| POS-022 | Update profile — SaveChangesAsync completes | Valid profile | Update(profile) | No exception, DB updated | P0 |
| POS-023 | Get profile — returns new ProfileModel instance | User exists | Get(email) | New ProfileModel, not entity reference | P1 |
| POS-024 | Update profile — does not modify Email | ProfileModel with Email | Update(profile) | Email used for lookup only, not persisted to UserProfile | P1 |
| POS-025 | Get profile — Email from input parameter | Get("specific@unops.org") | ProfileModel.Email equals "specific@unops.org" | P1 |
| POS-026 | Update profile — UserProfile.UserId set by EF | New UserProfile created | Update(profile) | UserProfile linked to PAOUser via FK | P1 |
| POS-027 | POST /api/profile — authenticated user | Valid JWT, valid ProfileModel body | POST /api/profile | 200 OK | P0 |
| POS-028 | POST /api/profile — HandleOperationAsync wraps Update | Valid request | POST /api/profile | Success response from HandleOperationAsync | P1 |
| POS-029 | Get profile — PAOUser.Name uses UserProfile.Name | UserProfile has FirstName="John", LastName="Doe" | PAOUser.Name (computed) | "John Doe" | P2 |
| POS-030 | Update profile — UserProfile.Name computed after save | Update FirstName="A", LastName="B" | UserProfile.Name | "A B" | P2 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Get — non-existent email | Get("nonexistent@unops.org") | BusinessException "User profile not found" | P0 |
| NEG-002 | Get — null email | Get(null) | BusinessException or FirstOrDefault returns null → BusinessException | P0 |
| NEG-003 | Get — empty string email | Get("") | BusinessException (no PAOUser with empty email) | P0 |
| NEG-004 | Update — non-existent email | ProfileModel{Email="nonexistent@unops.org"} | BusinessException "User profile not found" | P0 |
| NEG-005 | Update — null profile | Update(null) | NullReferenceException or ArgumentNullException | P0 |
| NEG-006 | Update — profile with null Email | ProfileModel{Email=null, FirstName="A", LastName="B"} | BusinessException (user not found) | P0 |
| NEG-007 | Update — profile with empty Email | ProfileModel{Email="", FirstName="A", LastName="B"} | BusinessException | P0 |
| NEG-008 | Get — whitespace-only email | Get("   ") | BusinessException (no match) | P1 |
| NEG-009 | Get — email with wrong case | PAOUser has "user@unops.org", Get("USER@UNOPS.ORG") | BusinessException (exact match fails) | P1 |
| NEG-010 | Get — email typo | Get("user@unopss.org") | BusinessException | P1 |
| NEG-011 | Get — email missing @ | Get("userunops.org") | BusinessException | P1 |
| NEG-012 | Get — email with trailing space | Get("user@unops.org ") | BusinessException | P1 |
| NEG-013 | Get — SQL injection in email | Get("'; DROP TABLE PAOUsers;--") | Sanitized or no match, BusinessException | P0 |
| NEG-014 | Update — SQL injection in FirstName | FirstName="'; DROP TABLE--" | Sanitized or error | P0 |
| NEG-015 | Update — XSS in FirstName | FirstName="<script>alert(1)</script>" | Sanitized or stored as-is (context-dependent) | P1 |
| NEG-016 | Update — XSS in LastName | LastName="<img src=x onerror=alert(1)>" | Sanitized or stored | P1 |
| NEG-017 | Get — email with null byte | Get("user@unops.org\0") | BusinessException or no match | P1 |
| NEG-018 | Update — profile Email mismatch (cross-user) | Authenticated as A, body Email=B | Update succeeds for B (authorization gap) | P0 |
| NEG-019 | Get — PAOUser deleted (ActiveUser=false) | PAOUser exists but ActiveUser=false | May still return (no ActiveUser filter) | P1 |
| NEG-020 | Get — UserProfile IsDeleted=true | UserProfile soft-deleted | May still return (no IsDeleted filter) | P1 |
| NEG-021 | Update — DbContext disposed | ProfileManager with disposed context | ObjectDisposedException on SaveChangesAsync | P1 |
| NEG-022 | Update — database connection lost | DB unavailable during SaveChangesAsync | DbUpdateException or connection error | P1 |
| NEG-023 | Update — transaction rolled back | Simulate rollback | No partial persist | P1 |
| NEG-024 | Get — PAOUsers DbSet empty | No PAOUsers in DB | BusinessException | P1 |
| NEG-025 | Update — duplicate PAOUser same email | Two PAOUsers with same Email (data integrity issue) | FirstOrDefault returns first, may update wrong user | P1 |
| NEG-026 | Get — UserProfile lazy load disabled | Lazy loading off, no Include | NullReferenceException on user.UserProfile?.FirstName | P1 |
| NEG-027 | Update — UserProfile creation fails (constraint) | UserProfile creation violates FK | DbUpdateException | P1 |
| NEG-028 | Get — email exceeds max length | Get(500-char string) | BusinessException or no match | P2 |
| NEG-029 | Update — FirstName exceeds DB column limit | FirstName 10000 chars | DbUpdateException or truncation | P1 |
| NEG-030 | Update — LastName exceeds DB column limit | LastName 10000 chars | DbUpdateException or truncation | P1 |
| NEG-031 | POST /api/profile — unauthenticated | No JWT | 401 Unauthorized | P0 |
| NEG-032 | POST /api/profile — malformed JSON body | Invalid JSON | 400 Bad Request | P1 |
| NEG-033 | POST /api/profile — missing Content-Type | No application/json header | 415 or 400 | P1 |
| NEG-034 | POST /api/profile — wrong HTTP method | GET /api/profile (commented out) | 404 or method not allowed | P1 |
| NEG-035 | Get — ProfileManager not registered | ProfileManager not in DI | Resolution exception at controller construction | P1 |
| NEG-036 | Update — AppDbContext null | ProfileManager with null context | NullReferenceException | P1 |
| NEG-037 | Get — email with control characters | Get("user@unops.org\u0000") | BusinessException | P2 |
| NEG-038 | Update — profile with control chars in FirstName | FirstName="A\u0000B" | Stored or rejected | P2 |
| NEG-039 | Get — email unicode homograph | Get("user@unоps.org") (Cyrillic o) | No match, BusinessException | P2 |
| NEG-040 | Update — null FirstName | ProfileModel{FirstName=null} | NullReferenceException or persisted as null | P1 |
| NEG-041 | Update — null LastName | ProfileModel{LastName=null} | NullReferenceException or persisted as null | P1 |
| NEG-042 | Get — concurrent delete of PAOUser | PAOUser deleted between Get and use | Stale data or error | P2 |
| NEG-043 | Update — concurrent delete of PAOUser | PAOUser deleted before Update | DbUpdateConcurrencyException or error | P1 |
| NEG-044 | Update — UserProfile creation without UserId | New UserProfile, UserId not set | FK constraint may fail | P1 |
| NEG-045 | Get — multiple PAOUsers same email (data corruption) | Duplicate emails in DB | FirstOrDefault returns first | P1 |
| NEG-046 | POST /api/profile — empty body | POST with {} | May fail validation or Update with null Email | P1 |
| NEG-047 | POST /api/profile — Content-Type text/plain | Wrong content type | 415 Unsupported Media Type | P1 |
| NEG-048 | Get — ProfileAuthorizationHandler not invoked for Get | GET commented out | N/A — endpoint inactive | P2 |
| NEG-049 | Update — ProfileAuthorizationHandler not used for Update | UpdateProfile has no resource auth | Any authenticated user can update any profile | P0 |
| NEG-050 | Get — expired JWT (when GET active) | Expired token | 401 Unauthorized | P1 |
| NEG-051 | Update — revoked token | Token revoked | 401 Unauthorized | P1 |
| NEG-052 | Get — email with leading spaces | Get("  user@unops.org") | No match, BusinessException | P1 |
| NEG-053 | Update — profile Email with leading spaces | Email="  user@unops.org" | No match, BusinessException | P1 |
| NEG-054 | Get — email format invalid (no domain) | Get("user") | BusinessException | P1 |
| NEG-055 | Get — email format invalid (no local) | Get("@unops.org") | BusinessException | P1 |
| NEG-056 | Update — circular reference in JSON | Malformed JSON with circular ref | 400 Bad Request | P2 |
| NEG-057 | Get — PAOUser with orphaned UserProfile | UserProfile.UserId points to deleted PAOUser | Scenario depends on FK config | P2 |
| NEG-058 | Update — readonly DbContext | Context configured read-only | InvalidOperationException on SaveChanges | P2 |
| NEG-059 | Get — DbContext in failed transaction state | Previous operation failed | May propagate exception | P2 |
| NEG-060 | Update — SaveChangesAsync throws | Simulate DB error | Exception propagated | P1 |
| NEG-061 | Get — email with newline | Get("user@unops.org\n") | No match | P2 |
| NEG-062 | Update — FirstName with newline | FirstName="A\nB" | Persisted or rejected | P2 |
| NEG-063 | Get — email with tab | Get("user@unops.org\t") | No match | P2 |
| NEG-064 | Update — LastName with tab | LastName="A\tB" | Persisted | P2 |
| NEG-065 | POST /api/profile — oversized body | Body > 1MB | 413 Payload Too Large or error | P2 |
| NEG-066 | Get — ProfileModel mapping wrong Email | Get returns profile | ProfileModel.Email must match input email | P1 |
| NEG-067 | Update — UserProfile.Name not updated | Update FirstName/LastName | UserProfile.Name computed on read | P1 |
| NEG-068 | Get — UserProfile optional fields not mapped | ProfileModel has only Email, FirstName, LastName | OrgUnit, DutyStation, etc. not in ProfileModel | P1 |
| NEG-069 | Update — UserProfile other fields overwritten | UserProfile has OrgUnit, DutyStation | Update only touches FirstName/LastName | P1 |
| NEG-070 | Get — PAOUser without Include UserProfile | Lazy loading | Extra query or N+1 if in loop | P2 |
| NEG-071 | Update — UserProfile new instance not tracked | user.UserProfile = new UserProfile() | EF tracks when assigned to user | P1 |
| NEG-072 | Get — email with RTL override char | Get("user@unops.org\u202E") | No match | P2 |
| NEG-073 | Update — FirstName with RTL char | FirstName="A\u202EB" | Persisted | P2 |
| NEG-074 | Get — email with zero-width space | Get("user@unops.org\u200B") | No match | P2 |
| NEG-075 | Update — LastName with zero-width space | LastName="Smith\u200B" | Persisted | P2 |
| NEG-076 | POST /api/profile — CORS preflight failure | Origin not allowed | CORS error | P2 |
| NEG-077 | Get — ProfileManager in different scope | Request-scoped vs singleton mismatch | Depends on DI config | P2 |
| NEG-078 | Update — UserProfile ModifiableDeletableEntity fields | New UserProfile created | Name, Status, audit fields have defaults | P1 |
| NEG-079 | Get — UserProfile inherits ModifiableDeletableEntity | UserProfile has IsDeleted, audit fields | Get does not filter IsDeleted | P1 |
| NEG-080 | Update — UserProfile.Name required by base | ModifiableDeletableEntity requires Name | May need Name set for save | P1 |
| NEG-081 | Get — PAOUser.ActiveUser not checked | PAOUser with ActiveUser=false | Still returned | P1 |
| NEG-082 | Update — PAOUser.ActiveUser not checked | Update for inactive user | Update succeeds | P1 |
| NEG-083 | POST /api/profile — rate limit exceeded | Too many requests | 429 Too Many Requests | P2 |
| NEG-084 | Get — HandleOperationAsync not used (GET commented) | GET endpoint | N/A | P2 |
| NEG-085 | Update — HandleOperationAsync catches exception | Update throws | Handled, appropriate status returned | P1 |
| NEG-086 | Get — ProfileAuthorizationHandler always succeeds | Operations.Read | context.Succeed(requirement) | P1 |
| NEG-087 | Update — no [PermissionAuthorize] on UpdateProfile | UpdateProfile action | Only [Authorize] required | P1 |
| NEG-088 | Get — ProfileModel not returned from controller | GET commented | N/A | P2 |
| NEG-089 | Update — ProfileModel from body not validated | No [Required] on Email | Update may receive invalid data | P1 |
| NEG-090 | Get — PAOUser.Id not in ProfileModel | ProfileModel has Email, FirstName, LastName only | No UserId or PAOUser.Id exposed | P1 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | FirstName | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-002 | LastName | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-003 | Email | 5 | 320 | "a@b.c" | 320 chars | 321 chars | P1 |
| BND-004 | Email local part | 1 | 64 | "a" | 64 chars | 65 chars | P1 |
| BND-005 | Email domain | 1 | 253 | "a.b" | 253 chars | 254 chars | P1 |
| BND-006 | FirstName single char | 1 | 1 | "A" | "A" | — | P1 |
| BND-007 | LastName single char | 1 | 1 | "B" | "B" | — | P1 |
| BND-008 | FirstName empty string | 0 | 0 | "" | "" | — | P1 |
| BND-009 | LastName empty string | 0 | 0 | "" | "" | — | P1 |
| BND-010 | FirstName null | — | — | null | null | — | P1 |
| BND-011 | LastName null | — | — | null | null | — | P1 |
| BND-012 | Email null | — | — | null | null | — | P1 |
| BND-013 | Email empty | 0 | 0 | "" | "" | — | P1 |
| BND-014 | FirstName whitespace only | — | — | "   " | "   " | — | P1 |
| BND-015 | LastName whitespace only | — | — | "   " | "   " | — | P1 |
| BND-016 | FirstName leading space | — | — | " John" | — | — | P1 |
| BND-017 | LastName trailing space | — | — | "Doe " | — | — | P1 |
| BND-018 | FirstName 254 chars | 254 | 255 | 254 chars | — | — | P1 |
| BND-019 | LastName 255 chars | 255 | 255 | — | 255 chars | — | P1 |
| BND-020 | FirstName 256 chars | 255 | 255 | — | — | 256 chars | P1 |
| BND-021 | Email "a@b.c" (min valid) | 5 | 5 | "a@b.c" | — | — | P1 |
| BND-022 | Email 320 chars (max) | 320 | 320 | — | 320 chars | — | P1 |
| BND-023 | Email 321 chars | 320 | 320 | — | — | 321 chars | P1 |
| BND-024 | FirstName unicode BMP | — | — | "José" | "日本語" | — | P1 |
| BND-025 | LastName unicode BMP | — | — | "Müller" | "北京" | — | P1 |
| BND-026 | FirstName emoji | — | — | "John😀" | — | — | P2 |
| BND-027 | LastName emoji | — | — | "Doe🎉" | — | — | P2 |
| BND-028 | FirstName hyphen | — | — | "Mary-Jane" | — | — | P1 |
| BND-029 | LastName apostrophe | — | — | "O'Brien" | — | — | P1 |
| BND-030 | FirstName multiple spaces | — | — | "Mary  Jane" | — | — | P1 |
| BND-031 | Email subaddress | — | — | "user+tag@unops.org" | — | — | P1 |
| BND-032 | Email case boundary | — | — | "User@UNOPS.org" vs "user@unops.org" | — | — | P1 |
| BND-033 | PAOUser.Id | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-034 | UserProfile.UserId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-035 | FirstName newline | — | — | "A\nB" | — | — | P2 |
| BND-036 | LastName tab | — | — | "A\tB" | — | — | P2 |
| BND-037 | FirstName carriage return | — | — | "A\rB" | — | — | P2 |
| BND-038 | Email with plus | — | — | "user+filter@unops.org" | — | — | P1 |
| BND-039 | Email with dot in local | — | — | "first.last@unops.org" | — | — | P1 |
| BND-040 | FirstName at 1 char | 1 | 1 | "X" | — | — | P1 |
| BND-041 | LastName at 1 char | 1 | 1 | "Y" | — | — | P1 |
| BND-042 | FirstName at 2 chars | 2 | 2 | "AB" | — | — | P1 |
| BND-043 | ProfileModel all null | — | — | Email=null, FirstName=null, LastName=null | — | — | P1 |
| BND-044 | ProfileModel all empty | — | — | Email="", FirstName="", LastName="" | — | — | P1 |
| BND-045 | Get email at max length | 320 | 320 | — | 320 chars | — | P1 |
| BND-046 | Update FirstName at 255 | 255 | 255 | — | 255 chars | — | P1 |
| BND-047 | Update LastName at 255 | 255 | 255 | — | 255 chars | — | P1 |
| BND-048 | UserProfile.Name computed empty | — | — | FirstName="", LastName="" | Name="" | — | P1 |
| BND-049 | UserProfile.Name computed FirstName only | — | — | FirstName="John", LastName="" | Name="John" | — | P1 |
| BND-050 | UserProfile.Name computed LastName only | — | — | FirstName="", LastName="Doe" | Name="Doe" | — | P1 |
| BND-051 | UserProfile.Name computed both | — | — | FirstName="John", LastName="Doe" | Name="John Doe" | — | P1 |
| BND-052 | PAOUser.Name when UserProfile null | — | — | UserProfile=null | Name="" | — | P1 |
| BND-053 | PAOUser.Name when UserProfile.Name empty | — | — | UserProfile.Name="" | Name="" | — | P1 |
| BND-054 | FirstName 0 chars | 0 | 0 | "" | — | — | P1 |
| BND-055 | LastName 0 chars | 0 | 0 | "" | — | — | P1 |
| BND-056 | Email with dash | — | — | "user-name@unops.org" | — | — | P1 |
| BND-057 | Email with underscore | — | — | "user_name@unops.org" | — | — | P1 |
| BND-058 | FirstName 100 chars | 100 | 100 | 100 chars | — | — | P1 |
| BND-059 | LastName 100 chars | 100 | 100 | 100 chars | — | — | P1 |
| BND-060 | FirstName 200 chars | 200 | 200 | 200 chars | — | — | P1 |
| BND-061 | Get with email at boundary of DB column | — | — | Email length at column max | — | — | P2 |
| BND-062 | Update with FirstName at DB column max | — | — | FirstName at column limit | — | — | P2 |
| BND-063 | Update with LastName at DB column max | — | — | LastName at column limit | — | — | P2 |
| BND-064 | FirstName with mixed unicode | — | — | "François 北京" | — | — | P2 |
| BND-065 | LastName with mixed unicode | — | — | "Müller 日本語" | — | — | P2 |
| BND-066 | Email with internationalized domain | — | — | "user@münchen.de" | — | — | P2 |
| BND-067 | FirstName 255 chars exact | 255 | 255 | — | 255 chars | — | P1 |
| BND-068 | LastName 255 chars exact | 255 | 255 | — | 255 chars | — | P1 |
| BND-069 | FirstName 256 chars over | 255 | 255 | — | — | 256 chars | P1 |
| BND-070 | LastName 256 chars over | 255 | 255 | — | — | 256 chars | P1 |
| BND-071 | Email 319 chars | 319 | 320 | 319 chars | — | — | P1 |
| BND-072 | Email 321 chars over | 320 | 320 | — | — | 321 chars | P1 |
| BND-073 | FirstName with only spaces (10) | — | — | "          " | — | — | P1 |
| BND-074 | LastName with only spaces (10) | — | — | "          " | — | — | P1 |
| BND-075 | Get email exactly matching stored | — | — | Exact match required | — | — | P1 |
| BND-076 | Update profile Email exactly matching PAOUser | — | — | Must match for lookup | — | — | P1 |
| BND-077 | UserProfile.UserId boundary | 1 | 2147483647 | 1 | Max | — | P2 |
| BND-078 | UserProfile.Id (ModifiableDeletableEntity) | 1 | 2147483647 | 1 | Max | — | P2 |
| BND-079 | FirstName with HTML entities | — | — | "&lt;script&gt;" | — | — | P2 |
| BND-080 | LastName with HTML entities | — | — | "&amp;" | — | — | P2 |
| BND-081 | Email with multiple @ | — | — | "user@@unops.org" | — | — | P1 |
| BND-082 | Email with @ at start | — | — | "@unops.org" | — | — | P1 |
| BND-083 | Email with @ at end | — | — | "user@" | — | — | P1 |
| BND-084 | FirstName 50 chars | 50 | 50 | 50 chars | — | — | P1 |
| BND-085 | LastName 50 chars | 50 | 50 | 50 chars | — | — | P1 |
| BND-086 | FirstName 150 chars | 150 | 150 | 150 chars | — | — | P1 |
| BND-087 | LastName 150 chars | 150 | 150 | 150 chars | — | — | P1 |
| BND-088 | FirstName 250 chars | 250 | 255 | 250 chars | — | — | P1 |
| BND-089 | LastName 250 chars | 250 | 255 | 250 chars | — | — | P1 |
| BND-090 | ProfileModel optional vs required | — | — | Email required for Update | FirstName/LastName nullable | — | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Get maps PAOUser to ProfileModel | ProfileModel has Email, FirstName, LastName only | Get(email) | ProfileModel populated from PAOUser + UserProfile | P0 |
| FUN-002 | Get uses email for PAOUser lookup | Lookup by Email | Get(email) | appDbContext.PAOUsers.FirstOrDefault(x => x.Email == email) | P0 |
| FUN-003 | Get returns empty strings when UserProfile null | Null coalescing | user.UserProfile?.FirstName ?? string.Empty | FirstName="", LastName="" | P0 |
| FUN-004 | Update creates UserProfile when null | user.UserProfile == null | Update(profile) | user.UserProfile = new UserProfile() | P0 |
| FUN-005 | Update sets FirstName on UserProfile | profile.FirstName | Update(profile) | user.UserProfile.FirstName = profile.FirstName | P0 |
| FUN-006 | Update sets LastName on UserProfile | profile.LastName | Update(profile) | user.UserProfile.LastName = profile.LastName | P0 |
| FUN-007 | Update uses Email for PAOUser lookup | profile.Email | Update(profile) | appDbContext.PAOUsers.FirstOrDefault(x => x.Email == profile.Email) | P0 |
| FUN-008 | Update calls SaveChangesAsync | Persistence | Update(profile) | await appDbContext.SaveChangesAsync() | P0 |
| FUN-009 | Get throws BusinessException when user null | user == null | Get(nonExistentEmail) | throw new BusinessException("User profile not found") | P0 |
| FUN-010 | Update throws BusinessException when user null | user == null | Update(profileWithBadEmail) | throw new BusinessException("User profile not found") | P0 |
| FUN-011 | ProfileModel.Email from input parameter | Get does not read from entity | Get(email) | ProfileModel.Email = email (input) | P1 |
| FUN-012 | UserProfile not modified except FirstName/LastName | Update scope | Update(profile) | OrgUnit, DutyStation, Position, etc. unchanged | P1 |
| FUN-013 | PAOUser.UserProfile 1:1 relationship | EF configuration | Assign user.UserProfile | Single UserProfile per PAOUser | P1 |
| FUN-014 | UserProfile.UserId links to PAOUser.Id | FK relationship | New UserProfile | UserId = user.Id when saved | P1 |
| FUN-015 | Get does not use Include for UserProfile | Lazy loading | Get(email) | Relies on lazy load for user.UserProfile | P1 |
| FUN-016 | Update does not set UserProfile.UserEmail | ProfileModel has Email | Update(profile) | UserProfile.UserEmail not updated | P1 |
| FUN-017 | Update does not set UserProfile.UserId explicitly | New UserProfile | user.UserProfile = new UserProfile() | EF sets UserId via relationship | P1 |
| FUN-018 | ProfileManager not in ManagerWrapper | DI | Controller constructor | ProfileManager injected directly | P1 |
| FUN-019 | ProfileManager depends only on AppDbContext | Constructor | new ProfileManager(context) | No IManagerWrapper, IMapper | P1 |
| FUN-020 | Get is synchronous | Method signature | Get(string?) | Returns ProfileModel, no async | P1 |
| FUN-021 | Update is asynchronous | Method signature | Update(ProfileModel) | async Task | P1 |
| FUN-022 | UserProfile inherits ModifiableDeletableEntity | Entity base | UserProfile | Has Id, Name, Status, audit, IsDeleted | P1 |
| FUN-023 | UserProfile.Name computed from FirstName+LastName | Computed property | UserProfile.Name | FirstName + " " + LastName, or FirstName/LastName only | P1 |
| FUN-024 | PAOUser.Name uses UserProfile.Name | PAOUser.Name getter | PAOUser.Name | UserProfile?.Name ?? string.Empty | P1 |
| FUN-025 | ProfileModel has no validation attributes | Model | ProfileModel | Email, FirstName, LastName all nullable | P1 |
| FUN-026 | Get uses FirstOrDefault | Query | PAOUsers.FirstOrDefault(x => x.Email == email) | Single or null | P1 |
| FUN-027 | Update uses FirstOrDefault | Query | PAOUsers.FirstOrDefault(x => x.Email == profile.Email) | Single or null | P1 |
| FUN-028 | No IsDeleted filter on PAOUser | Query | Get/Update | PAOUser query has no !x.IsDeleted | P1 |
| FUN-029 | No IsDeleted filter on UserProfile | Query | Get | UserProfile not filtered by IsDeleted | P1 |
| FUN-030 | No ActiveUser filter on PAOUser | Query | Get/Update | PAOUser query has no x.ActiveUser | P1 |
| FUN-031 | POST /api/profile uses [FromBody] | Controller | UpdateProfile([FromBody] ProfileModel profile) | Profile from request body | P1 |
| FUN-032 | POST /api/profile has [Authorize] | Controller | UserProfileController | Requires authenticated user | P1 |
| FUN-033 | GET /api/profile commented out | Controller | Get() method | Endpoint not active | P1 |
| FUN-034 | ProfileAuthorizationHandler checks Operations.Read | Handler | HandleRequirementAsync | requirement == Operations.Read → Succeed | P1 |
| FUN-035 | ProfileAuthorizationHandler always succeeds for Read | Handler | Any ProfileModel | context.Succeed(requirement) | P1 |
| FUN-036 | ProfileAuthorizationHandler does not check Edit | Handler | Operations.Edit (if used) | Not implemented | P1 |
| FUN-037 | UpdateProfile does not authorize resource | Controller | UpdateProfile | No AuthorizeAsync(User, profile, ...) | P1 |
| FUN-038 | HandleOperationAsync wraps Update | Controller | return await HandleOperationAsync(async () => { await _profileManager.Update(profile); }) | Exception handling, status code | P1 |
| FUN-039 | Get (commented) would use HttpContext.User.Identity?.Name | Controller | var email = HttpContext.User.Identity?.Name | Email from claims | P1 |
| FUN-040 | Get (commented) would authorize with Operations.Read | Controller | AuthorizeAsync(User, profile, Operations.Read) | ProfileAuthorizationHandler invoked | P1 |
| FUN-041 | UserProfile has UserPreference navigation | Entity | UserProfile.UserPreference | Optional 1:1 | P2 |
| FUN-042 | UserProfile has OrgUnit, SupervisorId, DutyStation, Position | Entity | UserProfile | Not used by ProfileManager | P1 |
| FUN-043 | ProfileModel maps only 3 fields | Mapping | Get return | Email, FirstName, LastName | P1 |
| FUN-044 | Update touches only 2 entity fields | Update scope | user.UserProfile.FirstName, LastName | No other UserProfile fields | P1 |
| FUN-045 | New UserProfile has default ModifiableDeletableEntity values | Entity creation | new UserProfile() | Name, Status, IsDeleted defaults | P1 |
| FUN-046 | UserProfile.Name required by base (ModifiableDeletableEntity) | Entity | UserProfile | Name may need value for save | P1 |
| FUN-047 | Get returns new object not entity | Return type | Get(email) | return new ProfileModel() { ... } | P1 |
| FUN-048 | Update modifies tracked entity | EF tracking | user.UserProfile.FirstName = ... | Changes tracked, SaveChanges persists | P1 |
| FUN-049 | New UserProfile added to context via assignment | EF | user.UserProfile = new UserProfile() | EF adds when user is tracked | P1 |
| FUN-050 | APIDictionary.Profile = "api/profile" | Route | APIDictionary.Profile | Route constant | P2 |
| FUN-051 | UserProfileController inherits BaseController | Controller | UserProfileController | HandleOperationAsync from base | P1 |
| FUN-052 | ProfileManager implements IApplicationService | Manager | ProfileManager | Marker interface | P2 |
| FUN-053 | Get email exact match (case-sensitive) | Lookup | x.Email == email | No ToLower, no case-insensitive | P1 |
| FUN-054 | Update email exact match (case-sensitive) | Lookup | x.Email == profile.Email | No ToLower | P1 |
| FUN-055 | UserProfile.UserEmail not synced from PAOUser.Email | Data | Update | UserProfile.UserEmail independent | P1 |
| FUN-056 | ProfileManager has no permission checks | Manager | Get, Update | No permission logic in manager | P1 |
| FUN-057 | Controller UpdateProfile has no permission attribute | Controller | [HttpPost(APIDictionary.Profile)] | No [PermissionAuthorize] | P1 |
| FUN-058 | Get (commented) would return profile directly | Controller | return profile | Not Ok(profile), just profile | P1 |
| FUN-059 | Update returns HandleOperationAsync result | Controller | return await HandleOperationAsync(...) | ActionResult from base | P1 |
| FUN-060 | UserProfile table "UserProfile" schema "public" | EF config | modelBuilder.Entity<UserProfile> | ToTable("UserProfile", "public") | P2 |
| FUN-061 | PAOUser has UserProfile optional navigation | Entity | PAOUser.UserProfile | UserProfile? | P1 |
| FUN-062 | UserProfile has UserId FK | Entity | UserProfile.UserId | int, required | P1 |
| FUN-063 | ProfileModel properties nullable | Model | ProfileModel | string? for all | P1 |
| FUN-064 | Get null email passes to FirstOrDefault | Get(null) | x.Email == null | Matches PAOUser with null Email (rare) | P1 |
| FUN-065 | Update null FirstName assigns null | profile.FirstName = null | user.UserProfile.FirstName = null | Null persisted | P1 |
| FUN-066 | Update null LastName assigns null | profile.LastName = null | user.UserProfile.LastName = null | Null persisted | P1 |
| FUN-067 | UserProfile.Name handles null FirstName | FirstName=null, LastName="Doe" | UserProfile.Name | "Doe" | P1 |
| FUN-068 | UserProfile.Name handles null LastName | FirstName="John", LastName=null | UserProfile.Name | "John" | P1 |
| FUN-069 | UserProfile.Name handles both null | FirstName=null, LastName=null | UserProfile.Name | "" | P1 |
| FUN-070 | UserProfile.Name trims space | FirstName="John", LastName="" | UserProfile.Name | "John" (Trim) | P1 |
| FUN-071 | PAOUser required Email | Entity | PAOUser.Email | required string | P1 |
| FUN-072 | PAOUser.Id primary key | Entity | PAOUser.Id | int, PK | P1 |
| FUN-073 | UserProfile.Id from ModifiableDeletableEntity | Entity | UserProfile.Id | int, PK | P1 |
| FUN-074 | ProfileManager stateless | Manager | Get, Update | No instance state between calls | P1 |
| FUN-075 | AppDbContext scoped | DI | ProfileManager | DbContext per request | P1 |
| FUN-076 | Get does not persist | Get | No SaveChanges | Read-only | P1 |
| FUN-077 | Update persists | Update | SaveChangesAsync | Write | P1 |
| FUN-078 | Get can be called multiple times | Idempotent read | Get(email) x3 | Same result each time | P1 |
| FUN-079 | Update is not idempotent | Write | Update(profile1), Update(profile2) | Second overwrites first | P1 |
| FUN-080 | ProfileModel has no Id | Model | ProfileModel | No UserId, no UserProfile.Id | P1 |
| FUN-081 | UserProfile created with default constructor | new UserProfile() | No parameters | All properties default | P1 |
| FUN-082 | Update does not set UserProfile.Name | Name is computed | Update(profile) | Name computed from FirstName+LastName | P1 |
| FUN-083 | Get ProfileModel.Email always from input | Not from entity | Get(email) | ProfileModel.Email = email | P1 |
| FUN-084 | UserProfile.UserEmail separate from ProfileModel.Email | Data | Update | UserProfile.UserEmail not set | P1 |
| FUN-085 | PAOUser.IsInternal not used by ProfileManager | Entity | PAOUser | Ignored | P2 |
| FUN-086 | PAOUser.ActiveUser not used by ProfileManager | Entity | PAOUser | Ignored | P1 |
| FUN-087 | UserProfile.SupervisorId not used by ProfileManager | Entity | UserProfile | Ignored | P2 |
| FUN-088 | UserProfile.DutyStation not used by ProfileManager | Entity | UserProfile | Ignored | P2 |
| FUN-089 | UserProfile.Position not used by ProfileManager | Entity | UserProfile | Ignored | P2 |
| FUN-090 | UserProfile.OrgUnit not used by ProfileManager | Entity | UserProfile | Ignored | P2 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Get — PAOUser to ProfileModel round-trip | Get | PAOUser, UserProfile, ProfileModel | ProfileModel correctly populated | P0 |
| INT-002 | Update — ProfileModel to UserProfile persist | Update | ProfileModel, PAOUser, UserProfile, DbContext | DB updated | P0 |
| INT-003 | Update — UserProfile creation and persist | Update | PAOUser, UserProfile, DbContext | New UserProfile in DB | P0 |
| INT-004 | POST /api/profile — full request flow | HTTP POST | Controller, ProfileManager, DbContext, PAOUser, UserProfile | 200, DB updated | P0 |
| INT-005 | Get — DbContext PAOUsers query | Get | AppDbContext, PAOUsers DbSet | Query executes, returns user | P0 |
| INT-006 | Update — DbContext SaveChangesAsync | Update | AppDbContext | Changes persisted to DB | P0 |
| INT-007 | Get — PAOUser-UserProfile 1:1 load | Get | PAOUser, UserProfile | UserProfile loaded (lazy or include) | P1 |
| INT-008 | Update — PAOUser-UserProfile relationship | Update | PAOUser, UserProfile | Relationship maintained | P1 |
| INT-009 | UserProfile FK to PAOUser | Update | UserProfile.UserId, PAOUser.Id | FK satisfied on insert | P1 |
| INT-010 | ProfileManager — AppDbContext injection | DI | ProfileManager, AppDbContext | Context injected, queries work | P1 |
| INT-011 | UserProfileController — ProfileManager injection | DI | UserProfileController, ProfileManager | Manager injected | P1 |
| INT-012 | UserProfileController — multiple dependencies | DI | ProfileManager, IUserDataManager, IUserInfoService, etc. | All resolved | P1 |
| INT-013 | Get — PAOUsers DbSet from AppDbContext | Get | appDbContext.PAOUsers | DbSet queryable | P1 |
| INT-014 | Update — UserProfile added to DbContext | Update | appDbContext, UserProfile | New entity tracked | P1 |
| INT-015 | Update — existing UserProfile updated in DbContext | Update | appDbContext, UserProfile | Entity state Modified | P1 |
| INT-016 | Get — UserProfile lazy load from PAOUser | Get | PAOUser.UserProfile | Navigation loaded | P1 |
| INT-017 | Update — UserProfile assignment to PAOUser | Update | user.UserProfile = new UserProfile() | EF tracks relationship | P1 |
| INT-018 | ProfileModel — API contract | POST | ProfileModel in body | Serialization/deserialization | P1 |
| INT-019 | HandleOperationAsync — exception handling | Update throws | BaseController.HandleOperationAsync | Appropriate error response | P1 |
| INT-020 | [Authorize] — authentication pipeline | POST without token | ASP.NET Core auth | 401 Unauthorized | P1 |
| INT-021 | ProfileAuthorizationHandler — registration | GET (if active) | IAuthorizationHandler, ProfileModel | Handler invoked | P1 |
| INT-022 | UserProfile — ModifiableDeletableEntity inheritance | UserProfile | Domain, Audit | Base fields present | P1 |
| INT-023 | PAOUser — UserProfile navigation | PAOUser | Domain | 1:1 optional | P1 |
| INT-024 | AppDbContext — PAOUsers DbSet | AppDbContext | DataAccess | DbSet<PAOUser> | P1 |
| INT-025 | AppDbContext — UserProfile DbSet | AppDbContext | DataAccess | DbSet<UserProfile> or via PAOUser | P1 |
| INT-026 | EF — PAOUser-UserProfile configuration | OnModelCreating | EF fluent config | HasOne, WithOne, HasForeignKey | P1 |
| INT-027 | Update — transaction scope | SaveChangesAsync | DbContext | Single transaction | P1 |
| INT-028 | Get — no transaction (read) | Get | DbContext | Read uncommitted or default | P1 |
| INT-029 | ProfileModel — JSON serialization | POST body | System.Text.Json or Newtonsoft | ProfileModel deserialized | P1 |
| INT-030 | UserProfileController — BaseController inheritance | Controller | BaseController | HandleOperationAsync available | P1 |
| INT-031 | Get — multiple PAOUsers (different emails) | Get | Multiple PAOUsers | Correct user returned by email | P1 |
| INT-032 | Update — multiple users, update one | Update | PAOUser A, PAOUser B | Only specified user updated | P1 |
| INT-033 | UserProfile — UserPreference navigation | UserProfile | UserPreference | Optional, not used by ProfileManager | P2 |
| INT-034 | UserInfoService — separate from ProfileManager | UpdateUserInfo | IUserInfoService, UserProfile | Different endpoint, different service | P1 |
| INT-035 | GetUserProfileDetails — uses IUserInfoService | GET CurrentUserInfo | UserProfileController, IUserInfoService | Not ProfileManager.Get | P1 |
| INT-036 | UpdateUserInfo — updates full UserProfile | PUT UserInfoUpdate | UserProfile, IUserInfoService | Different from ProfileManager.Update | P1 |
| INT-037 | ProfileManager — no IUserInfoService | ProfileManager | Dependencies | Only AppDbContext | P1 |
| INT-038 | PAOUser — AspNetUsers link | PAOUser | Identity | May link to AspNetUsers | P2 |
| INT-039 | UserProfile — UserId to PAOUser | UserProfile | PAOUser | FK UserId | P1 |
| INT-040 | Get — ProfileModel not from AutoMapper | Get | ProfileManager | Manual new ProfileModel() | P1 |
| INT-041 | Update — no AutoMapper | Update | ProfileManager | Direct property assignment | P1 |
| INT-042 | ProfileManager — no IMapper | ProfileManager | Dependencies | No mapper injection | P1 |
| INT-043 | POST /api/profile — route resolution | HTTP | Route /api/profile | Controller action matched | P1 |
| INT-044 | POST /api/profile — model binding | [FromBody] | ProfileModel | Model bound from JSON | P1 |
| INT-045 | Get — PAOUser with multiple UserProfiles (invalid) | Data | 1:1 constraint | Only one UserProfile per PAOUser | P1 |
| INT-046 | Update — UserProfile already exists | Update | user.UserProfile != null | No new creation, update existing | P1 |
| INT-047 | Get — PAOUser without UserProfile | Get | UserProfile = null | ProfileModel with empty FirstName/LastName | P1 |
| INT-048 | Update — UserProfile creation, then save | Update | New UserProfile | SaveChangesAsync inserts UserProfile | P1 |
| INT-049 | EF — UserProfile table name | Migration | UserProfile | Table "UserProfile", schema "public" | P2 |
| INT-050 | Get — DbContext scope | Get | Request scope | Same context as Update in same request | P1 |
| INT-051 | Update — DbContext scope | Update | Request scope | Same context as Get in same request | P1 |
| INT-052 | ProfileManager — scoped lifetime | DI | services.AddScoped<ProfileManager> or similar | Per-request instance | P1 |
| INT-053 | Get — PAOUser from PostgreSQL | Get | Npgsql, PostgreSQL | Query executes | P1 |
| INT-054 | Update — UserProfile persist to PostgreSQL | Update | Npgsql, PostgreSQL | Insert/Update in UserProfile table | P1 |
| INT-055 | UserProfile — audit fields (ModifiableDeletableEntity) | Update | CreatedBy, LastModifiedBy, etc. | May be set by AuditableDbContext | P2 |
| INT-056 | Get — no audit fields in ProfileModel | Get | ProfileModel | No CreatedDate, etc. | P1 |
| INT-057 | Update — UserProfile audit fields | Update | UserProfile | LastModifiedBy, LastModifiedDate may update | P2 |
| INT-058 | ProfileModel — DTO for API | API | ProfileModel | Transport object, not entity | P1 |
| INT-059 | UserProfile — entity for persistence | DB | UserProfile | Persisted entity | P1 |
| INT-060 | PAOUser — entity for persistence | DB | PAOUser | Persisted entity | P1 |
| INT-061 | Get — read path: Controller (commented) → Manager → DbContext → DB | Get | Full stack | N/A (GET commented) | P2 |
| INT-062 | Update — write path: Controller → Manager → DbContext → DB | Update | Full stack | End-to-end persist | P0 |
| INT-063 | HandleOperationAsync — success path | Update succeeds | BaseController | 200 OK or configured success | P1 |
| INT-064 | HandleOperationAsync — exception path | Update throws BusinessException | BaseController | 400 or appropriate status | P1 |
| INT-065 | BusinessException — global handler | Update throws | IExceptionHandler | ProblemDetails response | P1 |
| INT-066 | Get — no authorization in manager | Get | ProfileManager | No auth check | P1 |
| INT-067 | Update — no authorization in manager | Update | ProfileManager | No auth check | P1 |
| INT-068 | POST /api/profile — authorization at controller | [Authorize] | Controller | Authentication required | P1 |
| INT-069 | ProfileAuthorizationHandler — ProfileModel resource | Handler | AuthorizationHandler<..., ProfileModel> | ProfileModel as resource | P1 |
| INT-070 | Operations.Read — requirement type | Handler | OperationAuthorizationRequirement | Standard requirement | P1 |
| INT-071 | Get — PAOUser Email unique (business rule) | Data | PAOUser | Email should be unique | P1 |
| INT-072 | Update — lookup by Email | Update | profile.Email | PAOUser found by Email | P1 |
| INT-073 | Get — ProfileModel immutable from client | Get | ProfileModel | Returned, not modified by client in same request | P2 |
| INT-074 | Update — ProfileModel from client | Update | ProfileModel | Client sends, server applies | P1 |
| INT-075 | UserProfile — JsonIgnore on UserPreference | UserProfile | [JsonIgnore] | UserPreference not serialized | P2 |
| INT-076 | ProfileModel — no JsonIgnore | ProfileModel | All properties | All serialized | P1 |
| INT-077 | POST /api/profile — CORS | HTTP | CORS middleware | Allowed origins | P2 |
| INT-078 | POST /api/profile — logging | HandleOperationAsync | ILogger | Exceptions logged | P2 |
| INT-079 | ProfileManager — no logger | ProfileManager | Dependencies | No ILogger | P1 |
| INT-080 | Get — PAOUser ActiveUser not filtered | Get | PAOUser | ActiveUser=false still returned | P1 |
| INT-081 | Update — PAOUser ActiveUser not filtered | Update | PAOUser | Can update inactive user profile | P1 |
| INT-082 | UserProfile — IsDeleted not filtered | Get/Update | UserProfile | Soft-deleted UserProfile may be used | P1 |
| INT-083 | Update — UserProfile Name not explicitly set | Update | user.UserProfile.Name | Computed, not stored in Name column (if ignored) | P2 |
| INT-084 | UserProfile — Name property migration | Migration | IgnoreUserProfileNameProperty | Name may be computed only | P2 |
| INT-085 | Get — ProfileModel used by AI or other consumers | Get | Downstream | Contract stability | P2 |
| INT-086 | Update — called from profile edit UI | Update | Client | UI sends ProfileModel | P1 |
| INT-087 | UserProfileController — consolidated controller | Controller | Profile, UserData, UserInfo | Multiple concerns in one controller | P1 |
| INT-088 | ProfileManager — single responsibility | Manager | Get, Update | Profile only | P1 |
| INT-089 | Get — used by commented GET endpoint | Get | Controller | var profile = _profileManager.Get(email) | P2 |
| INT-090 | Update — used by POST endpoint | Update | Controller | await _profileManager.Update(profile) | P0 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | Update another user's profile — Email in body | Authenticated as A, body Email=B | Update B's profile | No server-side check; update succeeds (vulnerability) | P0 |
| SEC-002 | POST /api/profile unauthenticated | No JWT | Update profile | 401 Unauthorized | P0 |
| SEC-003 | POST /api/profile — expired token | Expired JWT | Update profile | 401 Unauthorized | P0 |
| SEC-004 | POST /api/profile — tampered token | Modified JWT | Update profile | 401 Unauthorized | P0 |
| SEC-005 | POST /api/profile — token with wrong audience | JWT audience mismatch | Update profile | 401 Unauthorized | P1 |
| SEC-006 | SQL injection in Get email | Get("'; DROP TABLE PAOUsers;--") | Database | Parameterized query, no injection | P0 |
| SEC-007 | SQL injection in Update Email | ProfileModel Email with SQL | Database | Parameterized query | P0 |
| SEC-008 | SQL injection in Update FirstName | FirstName with SQL | Database | Parameterized query | P0 |
| SEC-009 | SQL injection in Update LastName | LastName with SQL | Database | Parameterized query | P0 |
| SEC-010 | XSS in FirstName — stored | FirstName="<script>alert(1)</script>" | Client display | Sanitize on output or store as-is | P1 |
| SEC-011 | XSS in LastName — stored | LastName with script | Client display | Sanitize on output | P1 |
| SEC-012 | ProfileAuthorizationHandler — no resource check for Update | Update any profile | Authorization | Update has no resource-level auth | P0 |
| SEC-013 | GET /api/profile — commented, no IDOR test | N/A | GET endpoint | Endpoint inactive | P2 |
| SEC-014 | Update — no verify authenticated user matches Email | Body Email != claims | Authorization | Server does not verify | P0 |
| SEC-015 | POST /api/profile — CSRF | Cross-site request | CSRF token | [ValidateAntiForgeryToken] or SameSite cookie | P1 |
| SEC-016 | Get — information disclosure (if GET active) | Enumerate emails | Get(email) | Would need auth + self-only check | P2 |
| SEC-017 | Update — mass assignment | Extra properties in JSON | ProfileModel | Only Email, FirstName, LastName bound | P1 |
| SEC-018 | Update — prototype pollution | __proto__ in JSON | ProfileModel | Ignored by serializer | P2 |
| SEC-019 | POST /api/profile — oversized payload | Body 10MB | Request size limit | 413 or rejected | P1 |
| SEC-020 | Get — path traversal (N/A) | N/A | Get uses email not path | N/A | P2 |
| SEC-021 | Update — LDAP injection in Email | Email with LDAP chars | Lookup | Exact match, no LDAP | P2 |
| SEC-022 | Update — NoSQL injection (N/A) | N/A | PostgreSQL | N/A | P2 |
| SEC-023 | ProfileManager — no permission service | Manager | Get, Update | No IPermissionService | P1 |
| SEC-024 | Update — privilege escalation | Low-privilege user | Update admin profile | No check, may succeed | P0 |
| SEC-025 | Get — horizontal privilege escalation | User A | Get(B's email) | Manager allows; controller (GET) would need check | P1 |
| SEC-026 | POST /api/profile — HTTP method override | X-HTTP-Method-Override | Bypass | Standard ASP.NET handling | P2 |
| SEC-027 | Update — replay attack | Replay valid request | Idempotency | Same request twice, both succeed | P2 |
| SEC-028 | Get — timing attack on email | Measure response time | Enumerate emails | Constant-time or acceptable | P2 |
| SEC-029 | Update — rate limiting | Many updates | DoS | Rate limit if configured | P2 |
| SEC-030 | ProfileModel — sensitive data in ProfileModel | ProfileModel | Email, FirstName, LastName | No password, token | P1 |
| SEC-031 | UserProfile — sensitive fields not in ProfileModel | UserProfile | OrgUnit, SupervisorId | Not exposed via ProfileManager | P1 |
| SEC-032 | POST /api/profile — HTTPS required | HTTP | Man-in-the-middle | HTTPS enforced in production | P1 |
| SEC-033 | Update — audit trail | Who updated | AuditableDbContext | LastModifiedBy, LastModifiedDate | P2 |
| SEC-034 | Get — audit trail | Who read | N/A | No read audit in ProfileManager | P2 |
| SEC-035 | ProfileAuthorizationHandler — no role check | Handler | Any user | Succeeds for all | P1 |
| SEC-036 | Update — UserProfile creation by attacker | Attacker creates profile for victim | UserProfile | If victim has no profile, attacker could populate | P1 |
| SEC-037 | Get — null email information leak | Get(null) | Exception message | "User profile not found" — minimal leak | P1 |
| SEC-038 | Update — null profile DoS | Update(null) | Server | NullReferenceException, 500 | P1 |
| SEC-039 | POST /api/profile — content-type bypass | application/xml with JSON body | Parser | 400 or parse error | P2 |
| SEC-040 | Update — FirstName with null byte | FirstName="A\0B" | Database/display | Stored or rejected | P2 |
| SEC-041 | Get — email enumeration (if GET active) | Try emails | User existence | Would reveal existence | P2 |
| SEC-042 | Update — email enumeration | Update with non-existent email | BusinessException | "User profile not found" — reveals non-existence | P1 |
| SEC-043 | ProfileManager — no encryption at rest | UserProfile in DB | Storage | DB encryption if configured | P2 |
| SEC-044 | ProfileModel — no encryption in transit | API | Transport | HTTPS | P1 |
| SEC-045 | Update — concurrent update by different users | User A and B update same profile | Last write wins | No optimistic concurrency | P1 |
| SEC-046 | Get — cached profile (if caching) | Cached ProfileModel | Stale data | No caching in ProfileManager | P2 |
| SEC-047 | POST /api/profile — CORS preflight | OPTIONS request | CORS | Proper preflight response | P2 |
| SEC-048 | Update — JWT with wrong issuer | JWT from other tenant | Validation | 401 if issuer validated | P1 |
| SEC-049 | ProfileAuthorizationHandler — registered in DI | Handler | Authorization | Handler registered for ProfileModel | P1 |
| SEC-050 | Update — authorization handler not invoked | UpdateProfile | No resource parameter | AuthorizeAsync not called for Update | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Two Updates same user — sequential | Update A, then Update B | Last write wins | P0 |
| CON-002 | Two Updates same user — parallel | Update A and B concurrently | Last write wins, no exception | P0 |
| CON-003 | Get and Update same user — parallel | Get while Update in progress | Get may return stale or updated data | P1 |
| CON-004 | Update and Get same user — parallel | Update while Get in progress | Get may not see Update | P1 |
| CON-005 | Two Gets same user — parallel | Get(email) x2 concurrently | Both succeed, same result | P0 |
| CON-006 | Update user A, Update user B — parallel | Different users | Both succeed, no conflict | P0 |
| CON-007 | Update creates UserProfile — concurrent with Get | UserProfile null, Update creates, Get reads | Get may see null or new profile | P1 |
| CON-008 | SaveChangesAsync — concurrent from same context | Same ProfileManager instance, two Updates | Sequential (same context) | P1 |
| CON-009 | SaveChangesAsync — concurrent from different requests | Two HTTP requests, two ProfileManager instances | Both may succeed, last write wins | P1 |
| CON-010 | Update — DbContext disposed during SaveChanges | Context disposed mid-save | ObjectDisposedException | P1 |
| CON-011 | Get — PAOUser deleted during Get | Another request deletes PAOUser | Stale data or error | P2 |
| CON-012 | Update — PAOUser deleted during Update | Another request deletes PAOUser | DbUpdateException or FK violation | P1 |
| CON-013 | Update — UserProfile deleted during Update | UserProfile soft-deleted | May update IsDeleted record | P2 |
| CON-014 | Two Updates — FirstName vs LastName | Update1: FirstName, Update2: LastName | Both fields updated, possible interleave | P1 |
| CON-015 | Update — transaction isolation | Update in transaction, another reads | Depends on isolation level | P2 |
| CON-016 | Get — read uncommitted | Get during Update before commit | May see uncommitted data | P2 |
| CON-017 | Update — connection pool exhaustion | Many concurrent Updates | Connection pool limit | P2 |
| CON-018 | Get — connection pool | Many concurrent Gets | Read connections | P2 |
| CON-019 | Update — deadlock | Two Updates, different order | Deadlock detection, retry | P2 |
| CON-020 | Update — optimistic concurrency | UserProfile has RowVersion | No RowVersion in UserProfile | P1 |
| CON-021 | Get — cached DbContext | Same request, Get twice | Same context, same result | P1 |
| CON-022 | Update — DbContext scope per request | Two requests | Separate contexts | P1 |
| CON-023 | Update — UserProfile creation race | Two requests, both create UserProfile | One may fail FK or duplicate | P1 |
| CON-024 | Get — lazy load race | Get, UserProfile loading | Lazy load completes | P1 |
| CON-025 | Update — SaveChangesAsync cancellation | CancellationToken | OperationCanceledException | P2 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Get — valid email returns ProfileModel | Get | email="user@unops.org", user exists | ProfileModel with Email, FirstName, LastName | P0 |
| UNT-002 | Get — non-existent email throws BusinessException | Get | email="none@unops.org" | BusinessException "User profile not found" | P0 |
| UNT-003 | Get — null email throws | Get | email=null | BusinessException | P0 |
| UNT-004 | Update — valid profile persists | Update | ProfileModel{Email, FirstName, LastName} | No exception, SaveChangesAsync called | P0 |
| UNT-005 | Update — non-existent email throws BusinessException | Update | ProfileModel{Email="none@unops.org"} | BusinessException "User profile not found" | P0 |
| UNT-006 | Update — creates UserProfile when null | Update | user.UserProfile=null | user.UserProfile assigned new UserProfile | P0 |
| UNT-007 | Update — does not create UserProfile when exists | Update | user.UserProfile!=null | Existing UserProfile updated | P1 |
| UNT-008 | Get — UserProfile null returns empty strings | Get | user.UserProfile=null | FirstName="", LastName="" | P1 |
| UNT-009 | Get — ProfileModel.Email equals input email | Get | email="x@y.com" | ProfileModel.Email="x@y.com" | P1 |
| UNT-010 | Update — FirstName assigned to UserProfile | Update | profile.FirstName="Jane" | user.UserProfile.FirstName="Jane" | P1 |
| UNT-011 | Update — LastName assigned to UserProfile | Update | profile.LastName="Doe" | user.UserProfile.LastName="Doe" | P1 |
| UNT-012 | Get — uses FirstOrDefault | Get | Mock DbSet | FirstOrDefault invoked with email predicate | P1 |
| UNT-013 | Update — uses FirstOrDefault | Update | Mock DbSet | FirstOrDefault invoked with profile.Email | P1 |
| UNT-014 | Update — SaveChangesAsync invoked | Update | Mock DbContext | SaveChangesAsync called | P1 |
| UNT-015 | Get — returns new ProfileModel instance | Get | Any | Not same as entity | P1 |
| UNT-016 | ProfileManager — constructor accepts AppDbContext | Constructor | AppDbContext | Instance created | P1 |
| UNT-017 | Get — empty string email | Get | email="" | BusinessException | P1 |
| UNT-018 | Update — null FirstName | Update | profile.FirstName=null | user.UserProfile.FirstName=null | P1 |
| UNT-019 | Update — null LastName | Update | profile.LastName=null | user.UserProfile.LastName=null | P1 |
| UNT-020 | Get — case-sensitive email match | Get | PAOUser Email="User@UNOPS.org", Get("user@unops.org") | BusinessException (no match) | P1 |
| UNT-021 | Update — case-sensitive email match | Update | PAOUser Email="User@UNOPS.org", profile.Email="user@unops.org" | BusinessException | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Get — single user by email | Get(email) | < 100ms | P0 |
| PRF-002 | Update — existing UserProfile | Update(profile) | < 200ms | P0 |
| PRF-003 | Update — create new UserProfile | Update(profile) where UserProfile null | < 300ms | P0 |
| PRF-004 | POST /api/profile — full request | HTTP POST | < 500ms | P0 |
| PRF-005 | Get — PAOUsers table 10K rows | Get with 10K PAOUsers | < 100ms (indexed Email) | P1 |
| PRF-006 | Get — UserProfile lazy load | Get with lazy load | +1 query, < 50ms | P1 |
| PRF-007 | Update — SaveChangesAsync | SaveChangesAsync | < 100ms | P1 |
| PRF-008 | Get — no N+1 | Get in loop (if applicable) | No N+1 pattern | P1 |
| PRF-009 | Update — single round-trip | Update | 1 SaveChanges call | P1 |
| PRF-010 | Get — index on PAOUser.Email | Get query | Uses index | P1 |
| PRF-011 | Update — UserProfile insert | New UserProfile | Single INSERT | P1 |
| PRF-012 | Update — UserProfile update | Existing UserProfile | Single UPDATE | P1 |
| PRF-013 | Get — PAOUsers 100K rows | Get with large table | < 200ms | P2 |
| PRF-014 | Update — transaction overhead | Update | Minimal transaction cost | P2 |
| PRF-015 | Get — no Include overhead | Get without Include | No extra join | P1 |
| PRF-016 | ProfileManager — no heavy initialization | Constructor | < 1ms | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | POST /api/profile — 10 req/s | 10 concurrent users | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-002 | POST /api/profile — 20 req/s | 20 concurrent users | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-003 | Get (manager) — 50 req/s | 50 Get calls/s | 5 min | 95% < 100ms | P1 |
| LDT-004 | Update — 20 req/s mixed users | 20 users, different emails | 5 min | 95% < 500ms | P0 |
| LDT-005 | Update — 30 req/s same user | 30 concurrent updates same email | 5 min | Last write wins, no crash | P1 |
| LDT-006 | POST /api/profile — ramp 0–50 req/s | Ramp up | 10 min | No connection pool exhaustion | P1 |
| LDT-007 | Update — 100 sequential | 100 Updates | 1 min | All succeed | P1 |
| LDT-008 | Get — 200 sequential | 200 Gets | 1 min | All succeed | P1 |
| LDT-009 | Mixed Get/Update — 30 req/s | 15 Get, 15 Update/s | 5 min | 95% < 500ms | P1 |
| LDT-010 | POST /api/profile — sustained 50 req/s | 50 req/s | 2 min | No 503, no timeout | P2 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
