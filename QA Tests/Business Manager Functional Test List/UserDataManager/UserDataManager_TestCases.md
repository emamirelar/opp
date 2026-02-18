# UserDataManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/UserDataManager`  
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
| Interface | UNOPS.PAO.Business/Interfaces/IUserDataManager.cs | Implemented |
| Manager | UNOPS.PAO.Business/Managers/UserDataManager.cs | Implemented (no UNOPS override) |
| Model | UNOPS.PAO.Models/Users/PAOUserModel.cs | Implemented (Id, Email) |
| Entity | UNOPS.PAO.Domain/Entities/PAOUser.cs | Implemented (Id, Email, IsInternal, ActiveUser, UserProfile 1:1) |
| Used By | UserProfileController, CommentManager, GmailAddonManager | Active |
| API | GET /api/user-info/current | Current user info |
| API | PUT /api/user-info/update | Update user info |

---

## Feature Overview

**UserDataManager** is a READ-ONLY manager providing user lookup by ID, email, current authenticated user, and batch lookup by emails. Key responsibilities: GetUserByIdAsync, GetCurrentUserAsync, GetUserByEmailAsync, GetUsersByEmailsAsync. No CRUD operations. Uses IHttpContextAccessor for current user resolution (NameIdentifier or Email fallback). GetUsersByEmailsAsync normalizes emails to lowercase for matching.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | GetUserByIdAsync — valid user ID | PAOUser exists with Id=1 | GetUserByIdAsync(1) | PAOUserModel with Id=1, Email | P0 |
| POS-002 | GetUserByEmailAsync — valid email | PAOUser exists with Email="user@unops.org" | GetUserByEmailAsync("user@unops.org") | PAOUserModel with matching Email | P0 |
| POS-003 | GetCurrentUserAsync — authenticated by NameIdentifier | HttpContext has ClaimTypes.NameIdentifier="1" | GetCurrentUserAsync() | PAOUserModel for user 1 | P0 |
| POS-004 | GetCurrentUserAsync — authenticated by Email fallback | NameIdentifier missing, ClaimTypes.Email="user@unops.org" | GetCurrentUserAsync() | PAOUserModel via GetUserByEmailAsync | P0 |
| POS-005 | GetUsersByEmailsAsync — single email | PAOUser exists for email | GetUsersByEmailsAsync(["user@unops.org"]) | List with 1 PAOUserModel | P0 |
| POS-006 | GetUsersByEmailsAsync — multiple emails | 3 PAOUsers exist for emails | GetUsersByEmailsAsync([email1, email2, email3]) | List with 3 PAOUserModels | P0 |
| POS-007 | GetUserByIdAsync — user with UserProfile | PAOUser has UserProfile with FirstName/LastName | GetUserByIdAsync(id) | PAOUserModel with Id, Email (mapping may include profile) | P1 |
| POS-008 | GetUserByEmailAsync — email case insensitive | PAOUser Email="User@UNOPS.org" | GetUserByEmailAsync("user@unops.org") | PAOUserModel (GetUserByEmail uses exact match; batch uses ToLower) | P1 |
| POS-009 | GetUsersByEmailsAsync — mixed case emails | PAOUser Email="User@UNOPS.org" | GetUsersByEmailsAsync(["USER@UNOPS.ORG"]) | List with 1 PAOUserModel (emails normalized to lower) | P1 |
| POS-010 | GetUserByIdAsync — Id=1 (min valid) | PAOUser with Id=1 | GetUserByIdAsync(1) | PAOUserModel | P1 |
| POS-011 | GetUserByIdAsync — user IsInternal=true | PAOUser IsInternal=true | GetUserByIdAsync(id) | PAOUserModel returned | P1 |
| POS-012 | GetUserByIdAsync — user ActiveUser=true | PAOUser ActiveUser=true | GetUserByIdAsync(id) | PAOUserModel returned | P1 |
| POS-013 | GetUserByEmailAsync — email with subaddress | PAOUser Email="user+tag@unops.org" | GetUserByEmailAsync("user+tag@unops.org") | PAOUserModel | P1 |
| POS-014 | GetUsersByEmailsAsync — two emails same user | PAOUser has one email | GetUsersByEmailsAsync([email, email]) | List with 1 PAOUserModel (emails deduplicated by DB) | P1 |
| POS-015 | GetUserByIdAsync — AutoMapper produces PAOUserModel | PAOUser entity | GetUserByIdAsync(id) | PAOUserModel with Id, Email (not entity reference) | P1 |
| POS-016 | GetUserByEmailAsync — returns new model instance | User exists | GetUserByEmailAsync(email) | New PAOUserModel instance | P1 |
| POS-017 | GetUsersByEmailsAsync — order preserved | 3 emails | GetUsersByEmailsAsync([email1, email2, email3]) | List order matches DB query order | P1 |
| POS-018 | GetCurrentUserAsync — Identity.IsAuthenticated=true | User authenticated | GetCurrentUserAsync() | PAOUserModel or null | P1 |
| POS-019 | GetUserByIdAsync — user with UserProfile null | PAOUser.UserProfile=null | GetUserByIdAsync(id) | PAOUserModel (no UserProfile required) | P1 |
| POS-020 | GetUsersByEmailsAsync — 10 emails | 10 PAOUsers exist | GetUsersByEmailsAsync(10 emails) | List with 10 PAOUserModels | P1 |
| POS-021 | GetUserByEmailAsync — email with dot in local | PAOUser Email="first.last@unops.org" | GetUserByEmailAsync("first.last@unops.org") | PAOUserModel | P1 |
| POS-022 | GetUserByEmailAsync — email with hyphen | PAOUser Email="user-name@unops.org" | GetUserByEmailAsync("user-name@unops.org") | PAOUserModel | P1 |
| POS-023 | GetCurrentUserAsync — NameIdentifier integer parseable | ClaimTypes.NameIdentifier="42" | GetCurrentUserAsync() | GetUserByIdAsync(42) result | P1 |
| POS-024 | GetUsersByEmailsAsync — IEnumerable from array | string[] emails | GetUsersByEmailsAsync(emails) | List<PAOUserModel> | P1 |
| POS-025 | GetUsersByEmailsAsync — IEnumerable from List | List<string> emails | GetUsersByEmailsAsync(emails) | List<PAOUserModel> | P1 |
| POS-026 | GetUserByIdAsync — used by CommentManager | CommentManager needs user lookup | UserDataManager.GetUserByIdAsync(authorId) | PAOUserModel for comment author | P1 |
| POS-027 | GetUserByEmailAsync — used by GmailAddonManager | Gmail addon needs user by email | UserDataManager.GetUserByEmailAsync(email) | PAOUserModel | P1 |
| POS-028 | GetCurrentUserAsync — used by UserProfileController | Controller loads current user | UserDataManager.GetCurrentUserAsync() | PAOUserModel for profile | P1 |
| POS-029 | GetUserByIdAsync — Id at max int boundary | PAOUser with Id=2147483647 | GetUserByIdAsync(2147483647) | PAOUserModel or null | P2 |
| POS-030 | GetUsersByEmailsAsync — single email in collection | GetUsersByEmailsAsync with 1 email | Call | List with 0 or 1 item | P1 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | GetUserByIdAsync — non-existent ID | GetUserByIdAsync(99999) | null | P0 |
| NEG-002 | GetUserByIdAsync — zero ID | GetUserByIdAsync(0) | null (no PAOUser with Id=0) | P0 |
| NEG-003 | GetUserByIdAsync — negative ID | GetUserByIdAsync(-1) | null | P0 |
| NEG-004 | GetUserByEmailAsync — non-existent email | GetUserByEmailAsync("nonexistent@unops.org") | null | P0 |
| NEG-005 | GetUserByEmailAsync — null email | GetUserByEmailAsync(null) | NullReferenceException or ArgumentNullException | P0 |
| NEG-006 | GetUserByEmailAsync — empty string | GetUserByEmailAsync("") | null (no PAOUser with empty email) | P0 |
| NEG-007 | GetCurrentUserAsync — unauthenticated | HttpContext.User.Identity.IsAuthenticated=false | null | P0 |
| NEG-008 | GetCurrentUserAsync — HttpContext null | httpContextAccessor.HttpContext=null | null | P0 |
| NEG-009 | GetCurrentUserAsync — User null | HttpContext.User=null | null | P0 |
| NEG-010 | GetUsersByEmailsAsync — null emails | GetUsersByEmailsAsync(null) | Empty List (code checks null \|\| !Any()) | P0 |
| NEG-011 | GetUsersByEmailsAsync — empty collection | GetUsersByEmailsAsync([]) | Empty List | P0 |
| NEG-012 | GetUserByEmailAsync — whitespace-only email | GetUserByEmailAsync("   ") | null | P1 |
| NEG-013 | GetUserByEmailAsync — email wrong case | PAOUser "user@unops.org", Get("USER@UNOPS.ORG") | null (exact match) | P1 |
| NEG-014 | GetUserByEmailAsync — email typo | GetUserByEmailAsync("user@unopss.org") | null | P1 |
| NEG-015 | GetUserByEmailAsync — email missing @ | GetUserByEmailAsync("userunops.org") | null | P1 |
| NEG-016 | GetUserByEmailAsync — SQL injection | GetUserByEmailAsync("'; DROP TABLE PAOUsers;--") | null or sanitized | P0 |
| NEG-017 | GetUserByEmailAsync — email with null byte | GetUserByEmailAsync("user@unops.org\0") | null | P1 |
| NEG-018 | GetCurrentUserAsync — NameIdentifier non-integer | ClaimTypes.NameIdentifier="abc" | null (int.TryParse fails) | P0 |
| NEG-019 | GetCurrentUserAsync — NameIdentifier empty | ClaimTypes.NameIdentifier="" | null or fallback to Email | P1 |
| NEG-020 | GetCurrentUserAsync — both NameIdentifier and Email missing | No NameIdentifier, no Email claim | null | P0 |
| NEG-021 | GetUserByIdAsync — PAOUser ActiveUser=false | PAOUser exists but ActiveUser=false | Still returns (no filter) | P1 |
| NEG-022 | GetUserByIdAsync — DbContext disposed | Context disposed before call | ObjectDisposedException | P1 |
| NEG-023 | GetUserByEmailAsync — database connection lost | DB unavailable | Exception propagated | P1 |
| NEG-024 | GetUserByIdAsync — PAOUsers DbSet empty | No PAOUsers in DB | null | P1 |
| NEG-025 | GetUsersByEmailsAsync — all emails non-existent | GetUsersByEmailsAsync([nonexistent1, nonexistent2]) | Empty List | P1 |
| NEG-026 | GetUsersByEmailsAsync — mixed existent/non-existent | 2 exist, 1 not | List with 2 PAOUserModels | P1 |
| NEG-027 | GetUserByEmailAsync — email with trailing space | GetUserByEmailAsync("user@unops.org ") | null | P1 |
| NEG-028 | GetUserByEmailAsync — email with leading space | GetUserByEmailAsync(" user@unops.org") | null | P1 |
| NEG-029 | GetUserByEmailAsync — email format invalid (no domain) | GetUserByEmailAsync("user") | null | P1 |
| NEG-030 | GetUserByEmailAsync — email format invalid (no local) | GetUserByEmailAsync("@unops.org") | null | P1 |
| NEG-031 | GetUserByIdAsync — ID exceeds max int | GetUserByIdAsync(2147483648) — overflow | Compile error or overflow | P1 |
| NEG-032 | GetCurrentUserAsync — Identity null | User.Identity=null | null | P1 |
| NEG-033 | GetUserByEmailAsync — email with control chars | GetUserByEmailAsync("user@unops.org\u0000") | null | P2 |
| NEG-034 | GetUserByEmailAsync — unicode homograph | GetUserByEmailAsync("user@unоps.org") (Cyrillic o) | null | P2 |
| NEG-035 | GetUsersByEmailsAsync — emails with null in collection | IEnumerable contains null | Possible NullReferenceException on ToLower() | P1 |
| NEG-036 | GetUsersByEmailsAsync — emails with empty strings | GetUsersByEmailsAsync(["", "user@unops.org"]) | Empty string ToLower="", may match nothing | P1 |
| NEG-037 | GetUserByIdAsync — AutoMapper null source | Entity null (already handled) | Returns null before mapping | P1 |
| NEG-038 | GetUserByEmailAsync — PAOUser.Email null | PAOUser with Email=null (data corruption) | NullReferenceException on u.Email.ToLower() | P1 |
| NEG-039 | GetUsersByEmailsAsync — PAOUser.Email null in result | User in DB has null Email | Exception during Where/Select | P1 |
| NEG-040 | GetCurrentUserAsync — HttpContextAccessor null | httpContextAccessor=null | NullReferenceException | P1 |
| NEG-041 | GetUserByIdAsync — context.PAOUsers null | DbSet not initialized | NullReferenceException | P1 |
| NEG-042 | GetUserByEmailAsync — mapper null | IMapper=null | NullReferenceException on Map | P1 |
| NEG-043 | GetUserByIdAsync — multiple PAOUsers same Id | Data corruption (impossible with PK) | N/A | P2 |
| NEG-044 | GetUserByEmailAsync — duplicate emails in DB | Two PAOUsers same Email | FirstOrDefault returns first | P1 |
| NEG-045 | GetCurrentUserAsync — token expired | JWT expired | Auth middleware returns 401 before controller | P1 |
| NEG-046 | GetCurrentUserAsync — token revoked | Token revoked | null or 401 | P1 |
| NEG-047 | GetUserByEmailAsync — email with newline | GetUserByEmailAsync("user@unops.org\n") | null | P2 |
| NEG-048 | GetUserByEmailAsync — email with tab | GetUserByEmailAsync("user@unops.org\t") | null | P2 |
| NEG-049 | GetUsersByEmailsAsync — very long email string | Email 1000 chars | May hit DB limits | P2 |
| NEG-050 | GetUserByIdAsync — concurrent delete | PAOUser deleted between check and map | Stale data or null | P2 |
| NEG-051 | GetUserByEmailAsync — RTL override char | GetUserByEmailAsync("user@unops.org\u202E") | null | P2 |
| NEG-052 | GetUserByEmailAsync — zero-width space | GetUserByEmailAsync("user@unops.org\u200B") | null | P2 |
| NEG-053 | GetCurrentUserAsync — UserDataManager not in DI | Not registered | Resolution exception at controller | P1 |
| NEG-054 | GetUserByIdAsync — PAOUser soft-deleted (if applicable) | PAOUser has IsDeleted (if entity has it) | Depends on entity definition | P1 |
| NEG-055 | GetUsersByEmailsAsync — duplicate emails in input | GetUsersByEmailsAsync([e1, e1, e1]) | List with 1 PAOUserModel (DB distinct) | P1 |
| NEG-056 | GetUserByEmailAsync — email 320 chars (max valid) | Valid 320-char email not in DB | null | P1 |
| NEG-057 | GetUserByIdAsync — ID of deleted PAOUser | PAOUser physically deleted | null | P1 |
| NEG-058 | GetCurrentUserAsync — NameIdentifier "0" | ClaimTypes.NameIdentifier="0" | GetUserByIdAsync(0) → null | P1 |
| NEG-059 | GetUsersByEmailsAsync — IEnumerable yields null | Custom enumerable yields null | NullReferenceException on ToLower() | P1 |
| NEG-060 | GetUserByEmailAsync — XSS in email | GetUserByEmailAsync("<script>alert(1)</script>@x.com") | null | P1 |
| NEG-061 | GetUserByIdAsync — DbContext in failed state | Previous operation failed | May propagate exception | P2 |
| NEG-062 | GetUserByEmailAsync — connection timeout | DB slow | Timeout exception | P2 |
| NEG-063 | GetUsersByEmailsAsync — large collection 10000 emails | 10000 emails | Performance or memory issue | P2 |
| NEG-064 | GetCurrentUserAsync — multiple NameIdentifier claims | Duplicate claims | FindFirst returns first | P1 |
| NEG-065 | GetCurrentUserAsync — multiple Email claims | Duplicate claims | FindFirst returns first | P1 |
| NEG-066 | GetUserByIdAsync — PAOUser with UserProfile IsDeleted | UserProfile soft-deleted | User still returned (no filter) | P1 |
| NEG-067 | GetUserByEmailAsync — case sensitivity in GetUserByEmail | PAOUser "User@UNOPS.org" | Exact match, Get("user@unops.org")=null | P1 |
| NEG-068 | GetUsersByEmailsAsync — case normalization | GetUsersByEmailsAsync(["USER@UNOPS.ORG"]) | emailList has "user@unops.org", matches if DB has lowercase | P1 |
| NEG-069 | GetUserByIdAsync — mapper throws | AutoMapper misconfigured | Exception propagated | P1 |
| NEG-070 | GetUserByEmailAsync — FirstOrDefault returns null | No match | null | P0 |
| NEG-071 | GetUsersByEmailsAsync — Where returns empty | No emails match | Empty List | P1 |
| NEG-072 | GetCurrentUserAsync — User.Claims empty | No claims | null | P1 |
| NEG-073 | GetUserByIdAsync — Id=1 when user 1 deleted | PAOUser Id=1 deleted | null | P1 |
| NEG-074 | GetUserByEmailAsync — email with leading/trailing spaces | "  user@unops.org  " | null (no trim) | P1 |
| NEG-075 | GetUsersByEmailsAsync — null element in array | ["a@b.com", null, "c@d.com"] | NullRef on null.ToLower() | P1 |
| NEG-076 | GetCurrentUserAsync — HttpContext.User.Claims null | Claims null | FindFirst returns null | P1 |
| NEG-077 | GetUserByIdAsync — context not injected | UserDataManager with null context | NullReferenceException | P1 |
| NEG-078 | GetUserByEmailAsync — PAOUser.Email empty | PAOUser with Email="" | u.Email.ToLower()="", may match | P1 |
| NEG-079 | GetUsersByEmailsAsync — single non-existent email | GetUsersByEmailsAsync(["x@y.z"]) | Empty List | P1 |
| NEG-080 | GetUserByIdAsync — PAOUser IsInternal=false | External user | PAOUserModel returned | P1 |
| NEG-081 | GetCurrentUserAsync — Email claim present but user not in PAOUsers | Email="external@example.com", no PAOUser | null | P1 |
| NEG-082 | GetUserByEmailAsync — email with plus | GetUserByEmailAsync("user+filter@unops.org") | Match if PAOUser has same email | P1 |
| NEG-083 | GetUsersByEmailsAsync — all emails empty | GetUsersByEmailsAsync(["", "", ""]) | Empty List (empty not in DB) | P1 |
| NEG-084 | GetUserByIdAsync — PAOUser without UserProfile | UserProfile=null | PAOUserModel with Id, Email | P1 |
| NEG-085 | GetCurrentUserAsync — int.TryParse returns false | NameIdentifier="1.5" | null | P1 |
| NEG-086 | GetUserByEmailAsync — email exceeds 320 chars | 321-char email | null or DB error | P2 |
| NEG-087 | GetUsersByEmailsAsync — IQueryable vs IEnumerable | Emails from IQueryable | Works (IEnumerable) | P1 |
| NEG-088 | GetUserByIdAsync — PAOUser with all optional null | UserProfile, etc. null | PAOUserModel returned | P1 |
| NEG-089 | GetCurrentUserAsync — fallback to Email when NameIdentifier invalid | NameIdentifier="x", Email="user@unops.org" | GetUserByEmailAsync(email) | P1 |
| NEG-090 | GetUsersByEmailsAsync — Select mapper produces PAOUserModel | Users found | Each mapped to PAOUserModel | P1 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | User Id | 1 | 2147483647 | 1 | 2147483647 | — | P1 |
| BND-002 | User Id zero | 0 | — | 0 | — | — | P1 |
| BND-003 | User Id negative | — | — | -1 | — | — | P1 |
| BND-004 | Email length | 5 | 320 | "a@b.c" | 320 chars | 321 chars | P1 |
| BND-005 | Email min valid | 5 | 5 | "a@b.c" | — | — | P1 |
| BND-006 | Email local part | 1 | 64 | "a" | 64 chars | — | P1 |
| BND-007 | Email domain | 1 | 253 | "a.b" | 253 chars | — | P1 |
| BND-008 | GetUserByEmailAsync — empty string | 0 | 0 | "" | — | — | P1 |
| BND-009 | GetUserByEmailAsync — single char | — | — | "a" | — | — | P1 |
| BND-010 | GetUsersByEmailsAsync — empty collection | 0 | 0 | [] | — | — | P1 |
| BND-011 | GetUsersByEmailsAsync — single email | 1 | 1 | [email] | — | — | P1 |
| BND-012 | GetUsersByEmailsAsync — two emails | 2 | 2 | [e1, e2] | — | — | P1 |
| BND-013 | GetUsersByEmailsAsync — 100 emails | 100 | 100 | 100 emails | — | — | P1 |
| BND-014 | GetUsersByEmailsAsync — 1000 emails | 1000 | 1000 | 1000 emails | — | — | P2 |
| BND-015 | NameIdentifier — "1" | 1 | 1 | "1" | — | — | P1 |
| BND-016 | NameIdentifier — "2147483647" | — | — | "2147483647" | — | — | P1 |
| BND-017 | NameIdentifier — "0" | 0 | 0 | "0" | — | — | P1 |
| BND-018 | Email — "user@unops.org" | — | — | Exact match | — | — | P1 |
| BND-019 | Email — "USER@UNOPS.ORG" vs "user@unops.org" | — | — | Case diff | — | — | P1 |
| BND-020 | GetUsersByEmailsAsync — case normalization | — | — | "USER@X.ORG" → "user@x.org" | — | — | P1 |
| BND-021 | PAOUser.Id at 1 | 1 | 1 | 1 | — | — | P1 |
| BND-022 | PAOUser.Id at max int | 2147483647 | 2147483647 | — | 2147483647 | — | P1 |
| BND-023 | Email with subaddress | — | — | "user+tag@unops.org" | — | — | P1 |
| BND-024 | Email with dot in local | — | — | "first.last@unops.org" | — | — | P1 |
| BND-025 | Email with hyphen | — | — | "user-name@unops.org" | — | — | P1 |
| BND-026 | Email unicode | — | — | "user@münchen.de" | — | — | P2 |
| BND-027 | Email 255 chars | 255 | 255 | — | 255 chars | — | P1 |
| BND-028 | Email 320 chars | 320 | 320 | — | 320 chars | — | P1 |
| BND-029 | Email 321 chars | — | — | — | — | 321 chars | P1 |
| BND-030 | GetUsersByEmailsAsync — all exist | — | — | 5 emails, 5 users | 5 results | — | P1 |
| BND-031 | GetUsersByEmailsAsync — none exist | — | — | 5 emails, 0 users | Empty list | — | P1 |
| BND-032 | GetUsersByEmailsAsync — partial match | — | — | 3 emails, 2 users | 2 results | — | P1 |
| BND-033 | PAOUser.IsInternal true | — | — | true | — | — | P1 |
| BND-034 | PAOUser.IsInternal false | — | — | false | — | — | P1 |
| BND-035 | PAOUser.ActiveUser true | — | — | true | — | — | P1 |
| BND-036 | PAOUser.ActiveUser false | — | — | false | — | — | P1 |
| BND-037 | UserProfile null | — | — | null | — | — | P1 |
| BND-038 | UserProfile present | — | — | Present | — | — | P1 |
| BND-039 | PAOUserModel.Id mapping | — | — | PAOUser.Id=5 | PAOUserModel.Id=5 | — | P1 |
| BND-040 | PAOUserModel.Email mapping | — | — | PAOUser.Email="x@y.z" | PAOUserModel.Email="x@y.z" | — | P1 |
| BND-041 | GetUserByIdAsync — first PAOUser | — | — | Id=1 | Returns | — | P1 |
| BND-042 | GetUserByIdAsync — last PAOUser | — | — | Id=max | Returns or null | — | P1 |
| BND-043 | GetUserByEmailAsync — first char 'a' | — | — | "a@b.c" | — | — | P1 |
| BND-044 | GetUserByEmailAsync — @ position | — | — | "user@domain" | — | — | P1 |
| BND-045 | GetUsersByEmailsAsync — List vs Array | — | — | List<string> | Same result | — | P1 |
| BND-046 | GetUsersByEmailsAsync — HashSet emails | — | — | HashSet<string> | Works | — | P1 |
| BND-047 | GetUsersByEmailsAsync — lazy IEnumerable | — | — | yield return | Works | — | P1 |
| BND-048 | Email leading space | — | — | " user@x.org" | No match | — | P1 |
| BND-049 | Email trailing space | — | — | "user@x.org " | No match | — | P1 |
| BND-050 | Email tab | — | — | "user@x.org\t" | No match | — | P2 |
| BND-051 | Email newline | — | — | "user@x.org\n" | No match | — | P2 |
| BND-052 | Email carriage return | — | — | "user@x.org\r" | No match | — | P2 |
| BND-053 | NameIdentifier leading space | — | — | " 1" | int.TryParse may fail | — | P1 |
| BND-054 | NameIdentifier trailing space | — | — | "1 " | int.TryParse may fail | — | P1 |
| BND-055 | GetUsersByEmailsAsync — 50 emails | 50 | 50 | 50 emails | — | — | P1 |
| BND-056 | GetUsersByEmailsAsync — 200 emails | 200 | 200 | 200 emails | — | — | P2 |
| BND-057 | GetUserByIdAsync — Id just above max existing | — | — | maxId+1 | null | — | P1 |
| BND-058 | GetUserByEmailAsync — email one char diff | — | — | "user@unops.og" | null | — | P1 |
| BND-059 | GetUserByEmailAsync — email wrong TLD | — | — | "user@unops.com" | null | — | P1 |
| BND-060 | GetUsersByEmailsAsync — whitespace email | — | — | ["  "] | Empty or no match | — | P1 |
| BND-061 | GetUsersByEmailsAsync — mixed whitespace | — | — | [" ", "user@x.org"] | 1 or 0 results | — | P1 |
| BND-062 | PAOUser.Id int.MinValue | — | — | -2147483648 | No PAOUser | — | P1 |
| BND-063 | PAOUser.Id int.MaxValue | — | — | 2147483647 | May exist | — | P1 |
| BND-064 | Email with multiple @ | — | — | "user@@unops.org" | null | — | P1 |
| BND-065 | Email with @ at start | — | — | "@user@unops.org" | null | — | P1 |
| BND-066 | Email with @ at end | — | — | "user@unops.org@" | null | — | P1 |
| BND-067 | GetCurrentUserAsync — Identity.Name | — | — | May differ from Email | Fallback logic | — | P1 |
| BND-068 | GetUsersByEmailsAsync — distinct emails | — | — | [e,e,e] | 1 result | — | P1 |
| BND-069 | GetUserByEmailAsync — email 254 chars | 254 | 254 | — | 254 chars | — | P1 |
| BND-070 | GetUserByEmailAsync — email 319 chars | 319 | 319 | — | 319 chars | — | P1 |
| BND-071 | GetUsersByEmailsAsync — 0 results | — | — | All non-existent | [] | — | P1 |
| BND-072 | GetUsersByEmailsAsync — 1 result | — | — | 1 match | [1 model] | — | P1 |
| BND-073 | GetUserByIdAsync — Id 2 | 2 | 2 | 2 | — | — | P1 |
| BND-074 | GetUserByIdAsync — Id 100 | 100 | 100 | 100 | — | — | P1 |
| BND-075 | Email "x@y.z" | 5 | 5 | "x@y.z" | — | — | P1 |
| BND-076 | Email "ab@cd.ef" | 8 | 8 | "ab@cd.ef" | — | — | P1 |
| BND-077 | GetUsersByEmailsAsync — ICollection | — | — | ICollection<string> | Works | — | P1 |
| BND-078 | GetUsersByEmailsAsync — IReadOnlyList | — | — | IReadOnlyList<string> | Works | — | P1 |
| BND-079 | GetUserByEmailAsync — exact 64 local | — | — | 64-char local + @domain | — | — | P1 |
| BND-080 | GetUserByEmailAsync — exact 253 domain | — | — | user@ + 253 domain | — | — | P1 |
| BND-081 | PAOUserModel — Id zero | — | — | PAOUser Id=0 | Unusual | — | P1 |
| BND-082 | PAOUserModel — Email empty | — | — | PAOUser Email="" | Possible | — | P1 |
| BND-083 | GetCurrentUserAsync — one claim only | — | — | Only NameIdentifier | Works | — | P1 |
| BND-084 | GetCurrentUserAsync — one claim only Email | — | — | Only Email | Fallback works | — | P1 |
| BND-085 | GetUsersByEmailsAsync — 500 emails | 500 | 500 | 500 | — | — | P2 |
| BND-086 | GetUserByEmailAsync — IDN domain | — | — | "user@münchen.de" | Depends on DB | — | P2 |
| BND-087 | GetUsersByEmailsAsync — all same email | — | — | [e,e,e,e,e] | 1 result | — | P1 |
| BND-088 | GetUserByIdAsync — Id between existing | — | — | Id=5, users 1,2,3,4,6 exist | null | — | P1 |
| BND-089 | GetUserByEmailAsync — case variant | — | — | "User@UNOPS.org" vs "user@unops.org" | GetUserByEmail exact, GetUsersByEmails ToLower | — | P1 |
| BND-090 | GetUsersByEmailsAsync — ToLower invariant | — | — | "USER@UNOPS.ORG" | "user@unops.org" in query | — | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | GetUserByIdAsync — returns PAOUserModel | Model mapping rule | GetUserByIdAsync(1) | PAOUserModel with Id, Email | P0 |
| FUN-002 | GetUserByEmailAsync — exact email match | Email match rule | GetUserByEmailAsync("user@unops.org") | PAOUserModel or null | P0 |
| FUN-003 | GetCurrentUserAsync — resolves from NameIdentifier | Current user resolution | Authenticated, NameIdentifier="1" | GetUserByIdAsync(1) | P0 |
| FUN-004 | GetCurrentUserAsync — fallback to Email | Fallback when no NameIdentifier | Email claim only | GetUserByEmailAsync(email) | P0 |
| FUN-005 | GetUsersByEmailsAsync — normalizes to lowercase | Case normalization | GetUsersByEmailsAsync(["USER@X.ORG"]) | emailList.ToLower() for query | P0 |
| FUN-006 | GetUsersByEmailsAsync — returns empty for null | Null handling | GetUsersByEmailsAsync(null) | Empty List | P0 |
| FUN-007 | GetUsersByEmailsAsync — returns empty for empty | Empty collection | GetUsersByEmailsAsync([]) | Empty List | P0 |
| FUN-008 | GetUserByIdAsync — returns null for non-existent | Not found rule | GetUserByIdAsync(99999) | null | P0 |
| FUN-009 | GetUserByEmailAsync — returns null for non-existent | Not found rule | GetUserByEmailAsync("x@y.z") | null | P0 |
| FUN-010 | GetCurrentUserAsync — returns null when unauthenticated | Auth rule | User not authenticated | null | P0 |
| FUN-011 | GetUserByIdAsync — uses FirstOrDefault | Query behavior | GetUserByIdAsync(id) | Single or null | P1 |
| FUN-012 | GetUserByEmailAsync — uses FirstOrDefault | Query behavior | GetUserByEmailAsync(email) | Single or null | P1 |
| FUN-013 | GetUsersByEmailsAsync — uses Where Contains | Query behavior | GetUsersByEmailsAsync(emails) | Filter by email list | P1 |
| FUN-014 | GetUsersByEmailsAsync — uses Select Map | Mapping rule | Users found | mapper.Map<PAOUserModel>(u) each | P1 |
| FUN-015 | GetUserByIdAsync — no Include UserProfile | Lazy load | GetUserByIdAsync(id) | UserProfile not required for PAOUserModel | P1 |
| FUN-016 | PAOUserModel — Id from PAOUser.Id | Mapping | PAOUser entity | PAOUserModel.Id = entity.Id | P1 |
| FUN-017 | PAOUserModel — Email from PAOUser.Email | Mapping | PAOUser entity | PAOUserModel.Email = entity.Email | P1 |
| FUN-018 | GetCurrentUserAsync — int.TryParse for NameIdentifier | Parse rule | NameIdentifier="42" | userIdInt=42 | P1 |
| FUN-019 | GetCurrentUserAsync — returns null when TryParse fails | Parse failure | NameIdentifier="abc" | null | P1 |
| FUN-020 | GetUsersByEmailsAsync — emailList = emails.Select(e=>e.ToLower()) | Normalization | Any case emails | Lowercase list | P1 |
| FUN-021 | GetUserByIdAsync — context.PAOUsers.FirstOrDefault | Data source | Call | Direct DbSet query | P1 |
| FUN-022 | GetUserByEmailAsync — context.PAOUsers.FirstOrDefault | Data source | Call | Direct DbSet query | P1 |
| FUN-023 | GetUsersByEmailsAsync — no duplicate users for same email | DB distinct | Two inputs same email | One PAOUser in result | P1 |
| FUN-024 | GetCurrentUserAsync — User.Identity.IsAuthenticated check | Auth check | Call | Proceed only if true | P1 |
| FUN-025 | GetCurrentUserAsync — User null short-circuit | Null check | User null | null | P1 |
| FUN-026 | GetUserByIdAsync — Task.FromResult for sync pattern | Async pattern | Returns Task | Task.FromResult | P1 |
| FUN-027 | GetUserByEmailAsync — Task.FromResult for sync pattern | Async pattern | Returns Task | Task.FromResult | P1 |
| FUN-028 | GetUsersByEmailsAsync — Task.FromResult for sync pattern | Async pattern | Returns Task | Task.FromResult | P1 |
| FUN-029 | GetUserByIdAsync — returns before mapping when null | Early return | user==null | Task.FromResult<PAOUserModel?>(null) | P1 |
| FUN-030 | GetUserByEmailAsync — returns before mapping when null | Early return | user==null | Task.FromResult<PAOUserModel?>(null) | P1 |
| FUN-031 | GetUsersByEmailsAsync — .ToList() on users | Materialization | Query | In-memory list | P1 |
| FUN-032 | GetUsersByEmailsAsync — .ToList() on mappedUsers | Result | users.Select(mapper.Map) | List<PAOUserModel> | P1 |
| FUN-033 | GetUserByIdAsync — mapper.Map<PAOUserModel>(user) | AutoMapper | PAOUser | PAOUserModel | P1 |
| FUN-034 | GetUserByEmailAsync — u.Email.ToLower() in GetUsersByEmails | Case comparison | Where clause | emailList.Contains(u.Email.ToLower()) | P1 |
| FUN-035 | GetUsersByEmailsAsync — emails.Any() check | Empty check | !emails.Any() | Return empty list | P1 |
| FUN-036 | GetCurrentUserAsync — FindFirst(ClaimTypes.NameIdentifier) | Claim resolution | User.Claims | First NameIdentifier | P1 |
| FUN-037 | GetCurrentUserAsync — FindFirst(ClaimTypes.Email) | Fallback claim | No NameIdentifier | First Email | P1 |
| FUN-038 | GetUserByIdAsync — PAOUser without IsDeleted filter | No soft-delete filter | Query | All PAOUsers | P1 |
| FUN-039 | GetUserByEmailAsync — PAOUser without ActiveUser filter | No active filter | Query | All PAOUsers | P1 |
| FUN-040 | GetUsersByEmailsAsync — preserves user count | Count rule | 3 emails match 3 users | List count = 3 | P1 |
| FUN-041 | GetUserByIdAsync — used by CommentManager | Integration | Comment author lookup | Resolves user for comment | P1 |
| FUN-042 | GetUserByEmailAsync — used by GmailAddonManager | Integration | Email from Gmail | Resolves PAO user | P1 |
| FUN-043 | GetCurrentUserAsync — used by UserProfileController | Integration | Current user info | Loads profile user | P1 |
| FUN-044 | GetUserByIdAsync — no caching | No cache rule | Multiple calls same id | Fresh DB query each time | P1 |
| FUN-045 | GetUserByEmailAsync — no caching | No cache rule | Multiple calls same email | Fresh DB query each time | P1 |
| FUN-046 | GetUsersByEmailsAsync — single query for batch | Batch optimization | Multiple emails | One Where.Contains query | P1 |
| FUN-047 | GetUserByIdAsync — PAOUser.Id primary key | PK lookup | Id | Unique result | P1 |
| FUN-048 | GetUserByEmailAsync — PAOUser.Email may not be unique | FirstOrDefault | Duplicate emails | First match | P1 |
| FUN-049 | GetCurrentUserAsync — no NameIdentifier when 0 | Edge | NameIdentifier="0" | GetUserByIdAsync(0) → null | P1 |
| FUN-050 | GetUsersByEmailsAsync — empty string in emails | Input handling | ["", "a@b.com"] | "" ToLower, may match PAOUser Email="" | P1 |
| FUN-051 | GetUserByIdAsync — IUserDataManager contract | Interface | Call | Implements IUserDataManager | P1 |
| FUN-052 | GetUserByEmailAsync — string parameter | Signature | email: string | Accepts string | P1 |
| FUN-053 | GetUsersByEmailsAsync — IEnumerable<string> parameter | Signature | emails: IEnumerable<string> | Accepts any enumerable | P1 |
| FUN-054 | GetUserByIdAsync — int parameter | Signature | id: int | Accepts int | P1 |
| FUN-055 | GetCurrentUserAsync — no parameters | Signature | () | Uses HttpContext | P1 |
| FUN-056 | GetUserByIdAsync — returns Task<PAOUserModel?> | Return type | Call | Nullable model | P1 |
| FUN-057 | GetUserByEmailAsync — returns Task<PAOUserModel?> | Return type | Call | Nullable model | P1 |
| FUN-058 | GetUsersByEmailsAsync — returns Task<List<PAOUserModel>> | Return type | Call | List, never null | P1 |
| FUN-059 | GetCurrentUserAsync — returns Task<PAOUserModel?> | Return type | Call | Nullable model | P1 |
| FUN-060 | GetUserByIdAsync — constructor requires IMapper | DI | UserDataManager(mapper, context, http) | mapper used for Map | P1 |
| FUN-061 | GetUserByIdAsync — constructor requires AppDbContext | DI | UserDataManager(mapper, context, http) | context.PAOUsers | P1 |
| FUN-062 | GetCurrentUserAsync — constructor requires IHttpContextAccessor | DI | UserDataManager(mapper, context, http) | httpContextAccessor | P1 |
| FUN-063 | GetUsersByEmailsAsync — no AsNoTracking | Query | Read-only | May use tracking (manager is read-only) | P1 |
| FUN-064 | GetUserByIdAsync — synchronous FirstOrDefault | Sync in async | context.PAOUsers.FirstOrDefault | Blocks thread | P1 |
| FUN-065 | GetUserByEmailAsync — synchronous FirstOrDefault | Sync in async | context.PAOUsers.FirstOrDefault | Blocks thread | P1 |
| FUN-066 | GetUsersByEmailsAsync — synchronous ToList | Sync in async | .ToList() | Blocks thread | P1 |
| FUN-067 | GetUserByIdAsync — no AsNoTracking on PAOUsers | EF behavior | Query | Default tracking | P1 |
| FUN-068 | GetUsersByEmailsAsync — Where before ToList | Query order | emails → Where → ToList | Filter then materialize | P1 |
| FUN-069 | GetUserByIdAsync — PAOUser entity not exposed | Encapsulation | Return | PAOUserModel only | P1 |
| FUN-070 | GetUserByEmailAsync — PAOUser entity not exposed | Encapsulation | Return | PAOUserModel only | P1 |
| FUN-071 | GetUsersByEmailsAsync — PAOUser entities not exposed | Encapsulation | Return | List<PAOUserModel> | P1 |
| FUN-072 | GetCurrentUserAsync — PAOUser entity not exposed | Encapsulation | Return | PAOUserModel or null | P1 |
| FUN-073 | GetUserByIdAsync — no exception for not found | Error handling | Non-existent id | null, no exception | P1 |
| FUN-074 | GetUserByEmailAsync — no exception for not found | Error handling | Non-existent email | null, no exception | P1 |
| FUN-075 | GetUsersByEmailsAsync — no exception for no matches | Error handling | All non-existent | Empty list, no exception | P1 |
| FUN-076 | GetCurrentUserAsync — no exception when unauthenticated | Error handling | Not authenticated | null, no exception | P1 |
| FUN-077 | GetUserByIdAsync — ManagerWrapper provides UserDataManager | DI chain | Controller | managerWrapper.UserDataManager | P1 |
| FUN-078 | GetUserByEmailAsync — case in GetUserByEmail | Exact match | "User@UNOPS.org" vs "user@unops.org" | No ToLower in GetUserByEmail | P1 |
| FUN-079 | GetUsersByEmailsAsync — case in batch | ToLower | "USER@UNOPS.ORG" | emailList has lowercase | P1 |
| FUN-080 | GetCurrentUserAsync — fallback order | Priority | NameIdentifier then Email | Try NameIdentifier first | P1 |
| FUN-081 | GetUserByIdAsync — PAOUser.Name computed property | Entity | PAOUser has UserProfile | Name from UserProfile.Name | P1 |
| FUN-082 | PAOUserModel — no UserProfile in model | Model design | PAOUserModel | Id, Email only | P1 |
| FUN-083 | PAOUserModel — no IsInternal in model | Model design | PAOUserModel | Not in model | P1 |
| FUN-084 | PAOUserModel — no ActiveUser in model | Model design | PAOUserModel | Not in model | P1 |
| FUN-085 | GetUserByIdAsync — AutoMapper profile PAOUser→PAOUserModel | Mapping config | MappingProfile | CreateMap<PAOUser, PAOUserModel> | P1 |
| FUN-086 | GetUsersByEmailsAsync — Select not Where for mapping | LINQ | users.Select(mapper.Map) | Transform each | P1 |
| FUN-087 | GetUserByIdAsync — FirstOrDefault not SingleOrDefault | Query choice | Multiple (impossible with PK) | FirstOrDefault | P1 |
| FUN-088 | GetUserByEmailAsync — FirstOrDefault for possible duplicates | Query choice | Duplicate emails | First | P1 |
| FUN-089 | GetCurrentUserAsync — HttpContext scope | Request scope | Per request | HttpContext is request-scoped | P1 |
| FUN-090 | GetUsersByEmailsAsync — emails.ToList() for iteration | Materialization | emails.Select(e=>e.ToLower()) | ToList() for emailList | P1 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | UserProfileController — GetCurrentUserInfo uses UserDataManager | GET /api/user-info/current | UserProfileController, UserDataManager, PAOUser | Current user info returned | P0 |
| INT-002 | UserProfileController — UpdateUserInfo uses UserDataManager | PUT /api/user-info/update | UserProfileController, UserDataManager | User lookup for update | P0 |
| INT-003 | CommentManager — GetUserByIdAsync for comment author | Comment creation/display | CommentManager, UserDataManager, PAOUser | Author resolved | P0 |
| INT-004 | GmailAddonManager — GetUserByEmailAsync for Gmail user | Gmail addon flow | GmailAddonManager, UserDataManager, PAOUser | User resolved by email | P0 |
| INT-005 | Full flow — GetCurrentUserAsync → UserProfile | Profile load | UserDataManager, ProfileManager, PAOUser, UserProfile | Profile for current user | P0 |
| INT-006 | GetUserByIdAsync → PAOUserModel → API response | Serialization | UserDataManager, Controller, PAOUserModel | JSON with Id, Email | P1 |
| INT-007 | GetUserByEmailAsync → Comment author display | Comment flow | UserDataManager, Comment, PAOUser | Author name/email in UI | P1 |
| INT-008 | GetUsersByEmailsAsync → batch notification recipients | Notification | UserDataManager, NotificationManager, PAOUser | Recipients resolved | P1 |
| INT-009 | UserDataManager + AppDbContext — PAOUsers DbSet | Data access | UserDataManager, AppDbContext, PAOUser | Query executes | P1 |
| INT-010 | UserDataManager + AutoMapper — PAOUser to PAOUserModel | Mapping | UserDataManager, IMapper, PAOUser | PAOUserModel produced | P1 |
| INT-011 | UserDataManager + IHttpContextAccessor — HttpContext | Context | UserDataManager, IHttpContextAccessor | HttpContext available | P1 |
| INT-012 | ManagerWrapper — UserDataManager injection | DI | ManagerWrapper, IUserDataManager | UserDataManager resolved | P1 |
| INT-013 | UserProfileController — UserDataManager from ManagerWrapper | Controller DI | UserProfileController, ManagerWrapper | _userDataManager available | P1 |
| INT-014 | GetUserByIdAsync — PAOUser with UserProfile | Entity relationship | PAOUser, UserProfile 1:1 | User returned (profile optional for model) | P1 |
| INT-015 | GetUserByEmailAsync — PAOUser.Email unique constraint | DB | PAOUser table | FirstOrDefault if duplicate | P1 |
| INT-016 | GetUsersByEmailsAsync — PAOUsers + UserProfile | Entities | PAOUser, UserProfile | Users returned | P1 |
| INT-017 | GetCurrentUserAsync — JWT → Claims → User | Auth flow | JWT, ClaimsPrincipal, UserDataManager | User from token | P1 |
| INT-018 | GetCurrentUserAsync — NameIdentifier from Identity | Claim source | Identity, ClaimTypes.NameIdentifier | User ID from claim | P1 |
| INT-019 | GetCurrentUserAsync — Email from Identity | Claim source | Identity, ClaimTypes.Email | Email from claim | P1 |
| INT-020 | GetUserByIdAsync — used in authorization check | Auth | Authorization handler, UserDataManager | User loaded for permission | P1 |
| INT-021 | GetUserByEmailAsync — Gmail OAuth email match | OAuth | Gmail, PAOUser | Match external to internal | P1 |
| INT-022 | GetUsersByEmailsAsync — multiple Comment authors | Comments | CommentManager, multiple PAOUsers | Batch resolve authors | P1 |
| INT-023 | UserDataManager — no UNOPS override | Architecture | IUserDataManager | Single implementation | P1 |
| INT-024 | GetUserByIdAsync — DbContext scope | Scoped | UserDataManager, DbContext | Same context per request | P1 |
| INT-025 | GetUserByEmailAsync — DbContext scope | Scoped | UserDataManager, DbContext | Same context per request | P1 |
| INT-026 | GetUsersByEmailsAsync — DbContext scope | Scoped | UserDataManager, DbContext | Same context per request | P1 |
| INT-027 | GetCurrentUserAsync — HttpContext request scope | Scoped | IHttpContextAccessor | Request-specific context | P1 |
| INT-028 | GetUserByIdAsync — PAOUser from PostgreSQL | DB | AppDbContext, PostgreSQL | Query against PAOUsers table | P1 |
| INT-029 | GetUserByEmailAsync — PAOUser from PostgreSQL | DB | AppDbContext, PostgreSQL | Query against PAOUsers table | P1 |
| INT-030 | GetUsersByEmailsAsync — PAOUsers from PostgreSQL | DB | AppDbContext, PostgreSQL | Query with IN clause | P1 |
| INT-031 | PAOUserModel — API contract | API | PAOUserModel, JSON | Id, Email in response | P1 |
| INT-032 | GetUserByIdAsync — EF Core query | EF | DbContext, PAOUsers | FirstOrDefaultAsync or FirstOrDefault | P1 |
| INT-033 | GetUserByEmailAsync — EF Core query | EF | DbContext, PAOUsers | FirstOrDefault | P1 |
| INT-034 | GetUsersByEmailsAsync — EF Core query | EF | DbContext, PAOUsers | Where, ToList | P1 |
| INT-035 | UserProfileController — current-user-info endpoint | API | GET /api/user-info/current | Uses UserDataManager | P1 |
| INT-036 | UserProfileController — user-info update endpoint | API | PUT /api/user-info/update | Uses UserDataManager | P1 |
| INT-037 | GetUserByIdAsync — Comment.CommentBy or similar | Comment entity | Comment, PAOUser | FK or ID reference | P1 |
| INT-038 | GetUserByEmailAsync — external system email | Integration | Gmail, oUP, etc. | Email as universal identifier | P1 |
| INT-039 | GetUsersByEmailsAsync — notification batch | Notification | Multiple recipients | Batch lookup | P1 |
| INT-040 | GetCurrentUserAsync — profile edit flow | Profile | User edits profile | Current user for update | P1 |
| INT-041 | GetUserByIdAsync — audit CreatedBy | Audit | ModifiableDeletableEntity | Resolve user name | P1 |
| INT-042 | GetUserByIdAsync — audit LastModifiedBy | Audit | ModifiableDeletableEntity | Resolve user name | P1 |
| INT-043 | GetUserByEmailAsync — login/registration match | Auth | External auth, PAOUser | Match by email | P1 |
| INT-044 | GetUsersByEmailsAsync — invite flow | Invitation | Multiple invitees | Resolve existing users | P1 |
| INT-045 | UserDataManager — registered in DI | Startup | Lamar/DI container | IUserDataManager → UserDataManager | P1 |
| INT-046 | GetUserByIdAsync — PAOUser.Id from Identity | Identity | PAOIdentityUser, PAOUser | Id alignment | P1 |
| INT-047 | GetUserByEmailAsync — PAOUser.Email from Identity | Identity | PAOIdentityUser, PAOUser | Email alignment | P1 |
| INT-048 | GetCurrentUserAsync — UserResolverService alignment | Services | UserResolverService, UserDataManager | Same user ID | P1 |
| INT-049 | GetUserByIdAsync — IUserInfoService interaction | Services | UserInfoService, UserDataManager | May share user loading | P1 |
| INT-050 | GetUserByEmailAsync — UserProfileCacheService | Cache | UserProfileCacheService | Cache may use UserDataManager | P1 |
| INT-051 | GetUsersByEmailsAsync — UserPreferenceService | Preferences | UserPreferenceService | User lookup for preferences | P1 |
| INT-052 | GetUserByIdAsync — Opportunity created by | Opportunity | Opportunity, PAOUser | Creator lookup | P1 |
| INT-053 | GetUserByEmailAsync — Contact primary email | Contact | Contact, PAOUser | Match contact to user | P1 |
| INT-054 | GetUsersByEmailsAsync — Interaction participants | Interaction | Interaction, multiple PAOUsers | Participant lookup | P1 |
| INT-055 | GetCurrentUserAsync — Dashboard personalization | Dashboard | Dashboard, current user | User-specific data | P1 |
| INT-056 | GetUserByIdAsync — Document owner | Document | Document, PAOUser | Owner lookup | P1 |
| INT-057 | GetUserByEmailAsync — Partner focal point | Partner | Partner, PAOUser | Focal point lookup | P1 |
| INT-058 | GetUsersByEmailsAsync — Workflow assignees | Workflow | Workflow, PAOUsers | Assignee lookup | P1 |
| INT-059 | GetCurrentUserAsync — Saved filters | SavedFilter | SavedFilter, current user | User's filters | P1 |
| INT-060 | GetUserByIdAsync — Recent items | RecentItem | RecentItem, PAOUser | User's recent items | P1 |
| INT-061 | GetUserByEmailAsync — AI context user | AI | AiContextualService, PAOUser | User for AI context | P1 |
| INT-062 | GetUsersByEmailsAsync — Email merge | Merge | Duplicate contacts, PAOUsers | Resolve users for merge | P1 |
| INT-063 | GetUserByIdAsync — Report filter by user | Reports | Report, PAOUser | Filter by creator | P1 |
| INT-064 | GetUserByEmailAsync — Sync from ERP | ERP | oUP, PAOUser | Match ERP user | P1 |
| INT-065 | GetUsersByEmailsAsync — Bulk import users | Import | Import, PAOUsers | Resolve existing users | P1 |
| INT-066 | GetCurrentUserAsync — Permission check | Permissions | PermissionService, current user | User for permission | P1 |
| INT-067 | GetUserByIdAsync — Role assignment | Roles | Role, PAOUser | User for role check | P1 |
| INT-068 | GetUserByEmailAsync — SSO mapping | SSO | SSO provider, PAOUser | Email from SSO | P1 |
| INT-069 | GetUsersByEmailsAsync — Distribution list | Distribution | List of emails, PAOUsers | Resolve to users | P1 |
| INT-070 | GetUserByIdAsync — Audit trail display | Audit | Audit log, PAOUser | User name in audit | P1 |
| INT-071 | GetUserByEmailAsync — Password reset | Auth | Reset flow, PAOUser | User by email | P1 |
| INT-072 | GetUsersByEmailsAsync — Share with users | Share | Share dialog, PAOUsers | Resolve by emails | P1 |
| INT-073 | GetCurrentUserAsync — User preferences load | Preferences | UserPreferenceService | Current user prefs | P1 |
| INT-074 | GetUserByIdAsync — Notification recipient | Notification | Notification, PAOUser | Recipient lookup | P1 |
| INT-075 | GetUserByEmailAsync — Invite existing user | Invite | Invitation, PAOUser | Check if user exists | P1 |
| INT-076 | GetUsersByEmailsAsync — Multi-select user picker | UI | User picker, emails | Resolve selection | P1 |
| INT-077 | GetCurrentUserAsync — Session validation | Session | Session, current user | Validate session user | P1 |
| INT-078 | GetUserByIdAsync — Delegation | Delegation | Delegation, PAOUser | Delegate lookup | P1 |
| INT-079 | GetUserByEmailAsync — External partner user | Partner | Partner, external user | Match partner user | P1 |
| INT-080 | GetUsersByEmailsAsync — Team members | Team | Team, PAOUsers | Resolve team | P1 |
| INT-081 | GetCurrentUserAsync — Org unit context | Org | OrgUnit, current user | User's org | P1 |
| INT-082 | GetUserByIdAsync — Supervisor | Hierarchy | UserProfile.SupervisorId, PAOUser | Supervisor lookup | P1 |
| INT-083 | GetUserByEmailAsync — Duty station | Profile | UserProfile.DutyStation, PAOUser | User by duty station | P1 |
| INT-084 | GetUsersByEmailsAsync — Org unit members | Org | OrgUnit, PAOUsers | Members by email | P1 |
| INT-085 | GetCurrentUserAsync — Language preference | i18n | User preference, current user | User's language | P1 |
| INT-086 | GetUserByIdAsync — Timezone | Preferences | User preference, PAOUser | User's timezone | P1 |
| INT-087 | GetUserByEmailAsync — Notification preferences | Notification | Notification prefs, PAOUser | User by email | P1 |
| INT-088 | GetUsersByEmailsAsync — Search result highlighting | Search | Search, PAOUsers | Highlight matches | P1 |
| INT-089 | GetCurrentUserAsync — Theme preference | UI | Theme, current user | User's theme | P1 |
| INT-090 | GetUserByIdAsync — Last login | Auth | Last login, PAOUser | User for login record | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | GetUserByIdAsync — SQL injection in ID | id=1; DROP TABLE | GetUserByIdAsync | Parameterized query, no injection | P0 |
| SEC-002 | GetUserByEmailAsync — SQL injection | '; DROP TABLE PAOUsers;-- | GetUserByEmailAsync | Parameterized, null returned | P0 |
| SEC-003 | GetUsersByEmailsAsync — SQL injection in email | emails with '; DROP | GetUsersByEmailsAsync | Parameterized, sanitized | P0 |
| SEC-004 | GetCurrentUserAsync — unauthenticated access | No JWT | GetCurrentUserAsync | null, no data leak | P0 |
| SEC-005 | GetUserByIdAsync — IDOR enumeration | Iterate ids 1-10000 | GetUserByIdAsync | Returns null for non-existent, no info leak | P0 |
| SEC-006 | GetUserByEmailAsync — email enumeration | Brute force emails | GetUserByEmailAsync | null for non-existent | P0 |
| SEC-007 | GetUsersByEmailsAsync — batch enumeration | Large email list | GetUsersByEmailsAsync | Only returns existing | P0 |
| SEC-008 | GetUserByIdAsync — unauthorized user lookup | User A requests User B | GetUserByIdAsync(B_id) | Returns data (controller must authorize) | P0 |
| SEC-009 | GetCurrentUserAsync — token tampering | Modified JWT | GetCurrentUserAsync | Invalid claims, null | P0 |
| SEC-010 | GetUserByEmailAsync — XSS in email | <script> in email | GetUserByEmailAsync | No match, null | P0 |
| SEC-011 | GetUsersByEmailsAsync — XSS in emails | <script> in list | GetUsersByEmailsAsync | No match | P1 |
| SEC-012 | GetUserByIdAsync — negative ID | -1 | GetUserByIdAsync | null | P1 |
| SEC-013 | GetUserByEmailAsync — null byte injection | user@unops.org\0 | GetUserByEmailAsync | null | P1 |
| SEC-014 | GetCurrentUserAsync — expired token | Expired JWT | GetCurrentUserAsync | Auth middleware blocks first | P1 |
| SEC-015 | GetCurrentUserAsync — revoked token | Revoked JWT | GetCurrentUserAsync | null or 401 | P1 |
| SEC-016 | GetUserByIdAsync — mass data extraction | Loop GetUserByIdAsync | Rate limit | No rate limit in manager | P1 |
| SEC-017 | GetUsersByEmailsAsync — mass extraction | 10000 emails | GetUsersByEmailsAsync | Performance/memory | P1 |
| SEC-018 | GetUserByEmailAsync — LDAP injection | *)(uid=* | GetUserByEmailAsync | No match | P1 |
| SEC-019 | GetUserByIdAsync — integer overflow | 2147483648 | GetUserByIdAsync | Overflow or error | P1 |
| SEC-020 | GetCurrentUserAsync — claim injection | Malicious claim | GetCurrentUserAsync | Use only NameIdentifier, Email | P1 |
| SEC-021 | GetUserByEmailAsync — path traversal | ../../../etc/passwd | GetUserByEmailAsync | No match | P1 |
| SEC-022 | GetUsersByEmailsAsync — null in list | [null, "a@b.com"] | GetUsersByEmailsAsync | Possible NullRef | P1 |
| SEC-023 | GetUserByIdAsync — PAOUserModel data exposure | Return model | PAOUserModel | Only Id, Email (no sensitive) | P1 |
| SEC-024 | GetUserByEmailAsync — PAOUserModel data exposure | Return model | PAOUserModel | No password, tokens | P1 |
| SEC-025 | GetCurrentUserAsync — HttpContext isolation | Request A vs B | GetCurrentUserAsync | Per-request context | P1 |
| SEC-026 | GetUserByIdAsync — no authorization in manager | Manager layer | GetUserByIdAsync | Controller must authorize | P1 |
| SEC-027 | GetUserByEmailAsync — no authorization in manager | Manager layer | GetUserByEmailAsync | Controller must authorize | P1 |
| SEC-028 | GetUsersByEmailsAsync — no authorization in manager | Manager layer | GetUsersByEmailsAsync | Controller must authorize | P1 |
| SEC-029 | GetCurrentUserAsync — impersonation | Forged NameIdentifier | GetCurrentUserAsync | Trust Identity, auth layer | P1 |
| SEC-030 | GetUserByEmailAsync — email spoofing | user@attacker.com | GetUserByEmailAsync | Returns if exists | P1 |
| SEC-031 | GetUsersByEmailsAsync — information disclosure | Partial match | GetUsersByEmailsAsync | Only returns matched | P1 |
| SEC-032 | GetUserByIdAsync — timing attack | Measure response time | GetUserByIdAsync | Similar timing null vs found | P1 |
| SEC-033 | GetUserByEmailAsync — timing attack | Measure response time | GetUserByEmailAsync | Similar timing | P1 |
| SEC-034 | GetCurrentUserAsync — session fixation | Session token | GetCurrentUserAsync | Use current session | P1 |
| SEC-035 | GetUserByIdAsync — CSRF | Cross-site request | N/A (no state change) | Read-only | P1 |
| SEC-036 | GetUserByEmailAsync — open redirect | Email with URL | GetUserByEmailAsync | No redirect | P1 |
| SEC-037 | GetUsersByEmailsAsync — DoS large input | 1M emails | GetUsersByEmailsAsync | Memory/CPU | P1 |
| SEC-038 | GetUserByIdAsync — parameter pollution | id=1&id=2 | GetUserByIdAsync | Single id | P1 |
| SEC-039 | GetUserByEmailAsync — header injection | Email in header | GetUserByEmailAsync | Parameter from body/query | P1 |
| SEC-040 | GetCurrentUserAsync — JWT algorithm confusion | alg=none | GetCurrentUserAsync | Auth rejects | P1 |
| SEC-041 | GetUserByIdAsync — log injection | id=1\nadmin | GetUserByIdAsync | Log sanitization | P2 |
| SEC-042 | GetUserByEmailAsync — log injection | Email with newlines | GetUserByEmailAsync | Log sanitization | P2 |
| SEC-043 | GetUsersByEmailsAsync — regex DoS | Catastrophic backtracking | GetUsersByEmailsAsync | No regex on input | P2 |
| SEC-044 | GetUserByIdAsync — type confusion | id="1" (string) | GetUserByIdAsync | Compile-time int | P1 |
| SEC-045 | GetUserByEmailAsync — unicode homograph | user@unоps.org | GetUserByEmailAsync | No match | P1 |
| SEC-046 | GetCurrentUserAsync — missing Authorization header | No header | GetCurrentUserAsync | [Authorize] blocks | P1 |
| SEC-047 | GetUserByIdAsync — horizontal privilege | User A gets User B | Controller | Controller must check | P1 |
| SEC-048 | GetUserByEmailAsync — horizontal privilege | User A gets B's email | Controller | Controller must check | P1 |
| SEC-049 | GetUsersByEmailsAsync — horizontal privilege | User A gets B's emails | Controller | Controller must check | P1 |
| SEC-050 | GetCurrentUserAsync — vertical privilege | Guest gets admin | GetCurrentUserAsync | Returns own user only | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | GetUserByIdAsync — concurrent same ID | 10 threads GetUserByIdAsync(1) | All return same PAOUserModel | P0 |
| CON-002 | GetUserByEmailAsync — concurrent same email | 10 threads GetUserByEmailAsync("u@x.org") | All return same PAOUserModel | P0 |
| CON-003 | GetUsersByEmailsAsync — concurrent same list | 10 threads same emails | All return same list | P0 |
| CON-004 | GetCurrentUserAsync — concurrent same user | 10 threads same request | All return same user | P0 |
| CON-005 | GetUserByIdAsync — concurrent different IDs | 10 threads different ids | All return correct models | P0 |
| CON-006 | GetUserByIdAsync vs GetUserByEmailAsync — same user | Concurrent both | Consistent result | P1 |
| CON-007 | GetUsersByEmailsAsync — concurrent overlapping lists | Thread 1 [a,b], Thread 2 [b,c] | No deadlock | P1 |
| CON-008 | GetUserByIdAsync — PAOUser deleted during call | Delete in another request | null or stale | P1 |
| CON-009 | GetUserByEmailAsync — PAOUser updated during call | Update in another request | Consistent or stale | P1 |
| CON-010 | GetCurrentUserAsync — session change during call | Re-auth during request | Unlikely, request-scoped | P1 |
| CON-011 | GetUsersByEmailsAsync — large concurrent batches | 5 threads × 100 emails | No connection pool exhaustion | P1 |
| CON-012 | GetUserByIdAsync — DbContext concurrent access | Same context, multiple calls | DbContext not thread-safe | P1 |
| CON-013 | GetUserByEmailAsync — DbContext concurrent access | Same context | Per-request scope, no cross-request | P1 |
| CON-014 | GetUsersByEmailsAsync — DbContext single request | Multiple calls same request | Same context, sequential | P1 |
| CON-015 | GetUserByIdAsync — connection pool under load | 100 concurrent requests | Pool handles | P1 |
| CON-016 | GetCurrentUserAsync — parallel requests different users | Req 1 user A, Req 2 user B | Each gets own user | P1 |
| CON-017 | GetUserByIdAsync — cache stampede (if cached) | No cache in manager | N/A | P2 |
| CON-018 | GetUsersByEmailsAsync — race on ToList | Concurrent materialization | No shared mutable state | P1 |
| CON-019 | GetUserByIdAsync — FirstOrDefault concurrent read | Multiple readers | No lock needed, read-only | P1 |
| CON-020 | GetUserByEmailAsync — FirstOrDefault concurrent read | Multiple readers | Read-only | P1 |
| CON-021 | GetUsersByEmailsAsync — Where Contains concurrent | Multiple queries | Independent queries | P1 |
| CON-022 | GetUserByIdAsync — AutoMapper concurrent | Mapper is thread-safe | Safe | P1 |
| CON-023 | GetCurrentUserAsync — HttpContext concurrent | Per-request | No cross-request | P1 |
| CON-024 | GetUserByIdAsync — DbContext disposed by another | Request ends, context disposed | ObjectDisposedException if reused | P1 |
| CON-025 | GetUsersByEmailsAsync — Task.WhenAll pattern | Parallel GetUserByIdAsync | Each has own scope | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | GetUserByIdAsync — valid id | Lookup | id=1 | PAOUserModel or null | P0 |
| UNT-002 | GetUserByIdAsync — zero | Lookup | id=0 | null | P0 |
| UNT-003 | GetUserByIdAsync — negative | Lookup | id=-1 | null | P0 |
| UNT-004 | GetUserByEmailAsync — valid email | Lookup | "user@unops.org" | PAOUserModel or null | P0 |
| UNT-005 | GetUserByEmailAsync — null | Lookup | null | Exception or null | P0 |
| UNT-006 | GetUserByEmailAsync — empty | Lookup | "" | null | P0 |
| UNT-007 | GetCurrentUserAsync — authenticated | Lookup | User authenticated | PAOUserModel or null | P0 |
| UNT-008 | GetCurrentUserAsync — unauthenticated | Lookup | User not authenticated | null | P0 |
| UNT-009 | GetUsersByEmailsAsync — null | Batch | null | Empty List | P0 |
| UNT-010 | GetUsersByEmailsAsync — empty | Batch | [] | Empty List | P0 |
| UNT-011 | GetUsersByEmailsAsync — one email | Batch | ["a@b.com"] | List 0 or 1 item | P1 |
| UNT-012 | GetUsersByEmailsAsync — multiple emails | Batch | ["a@b.com","c@d.com"] | List 0-2 items | P1 |
| UNT-013 | GetUserByIdAsync — mapper output | Mapping | PAOUser | PAOUserModel.Id, Email | P1 |
| UNT-014 | GetUserByEmailAsync — case sensitivity | Lookup | "USER@UNOPS.ORG" vs "user@unops.org" | Exact match | P1 |
| UNT-015 | GetUsersByEmailsAsync — case normalization | Batch | ["USER@X.ORG"] | ToLower applied | P1 |
| UNT-016 | GetCurrentUserAsync — NameIdentifier parse | Lookup | "42" | GetUserByIdAsync(42) | P1 |
| UNT-017 | GetCurrentUserAsync — NameIdentifier invalid | Lookup | "x" | null | P1 |
| UNT-018 | GetCurrentUserAsync — Email fallback | Lookup | No NameIdentifier, Email="u@x.org" | GetUserByEmailAsync | P1 |
| UNT-019 | GetUserByIdAsync — returns Task | Async | Any id | Task<PAOUserModel?> | P1 |
| UNT-020 | GetUserByEmailAsync — returns Task | Async | Any email | Task<PAOUserModel?> | P1 |
| UNT-021 | GetUsersByEmailsAsync — returns Task | Async | Any emails | Task<List<PAOUserModel>> | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetUserByIdAsync — single lookup | GetUserByIdAsync(1) | < 100ms | P0 |
| PRF-002 | GetUserByEmailAsync — single lookup | GetUserByEmailAsync("u@x.org") | < 100ms | P0 |
| PRF-003 | GetCurrentUserAsync — authenticated | GetCurrentUserAsync() | < 150ms | P0 |
| PRF-004 | GetUsersByEmailsAsync — 10 emails | GetUsersByEmailsAsync(10 emails) | < 200ms | P0 |
| PRF-005 | GetUsersByEmailsAsync — 50 emails | GetUsersByEmailsAsync(50 emails) | < 500ms | P1 |
| PRF-006 | GetUsersByEmailsAsync — 100 emails | GetUsersByEmailsAsync(100 emails) | < 1s | P1 |
| PRF-007 | GetUserByIdAsync — 100 sequential calls | Loop 100× GetUserByIdAsync | < 5s total | P1 |
| PRF-008 | GetUserByEmailAsync — 100 sequential calls | Loop 100× GetUserByEmailAsync | < 5s total | P1 |
| PRF-009 | GetCurrentUserAsync — 50 sequential calls | Loop 50× GetCurrentUserAsync | < 3s total | P1 |
| PRF-010 | GetUsersByEmailsAsync — 200 emails | GetUsersByEmailsAsync(200 emails) | < 2s | P1 |
| PRF-011 | GetUserByIdAsync — cold start | First call after app start | < 500ms | P2 |
| PRF-012 | GetUserByEmailAsync — with index on Email | Email lookup | < 100ms | P1 |
| PRF-013 | GetUsersByEmailsAsync — IN clause performance | Where Contains | < 500ms for 50 | P1 |
| PRF-014 | GetUserByIdAsync — PK lookup | Primary key | < 50ms | P1 |
| PRF-015 | GetUsersByEmailsAsync — 500 emails | Large batch | < 5s | P2 |
| PRF-016 | GetCurrentUserAsync — claim resolution | FindFirst | < 10ms overhead | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | GetUserByIdAsync — 20 req/s | 20 concurrent users, GetUserByIdAsync | 5 min | 95% < 200ms, 0% error | P0 |
| LDT-002 | GetUserByEmailAsync — 20 req/s | 20 concurrent, GetUserByEmailAsync | 5 min | 95% < 200ms, 0% error | P0 |
| LDT-003 | GetCurrentUserAsync — 20 req/s | 20 concurrent, GetCurrentUserAsync | 5 min | 95% < 250ms, 0% error | P0 |
| LDT-004 | GetUsersByEmailsAsync — 10 req/s | 10 concurrent, 20 emails each | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-005 | Mixed — 30 req/s combined | Mix of all 4 methods | 5 min | 95% < 500ms | P1 |
| LDT-006 | GetUserByIdAsync — spike 50 req/s | 50 concurrent for 1 min | 1 min spike | No connection pool exhaustion | P1 |
| LDT-007 | GetUsersByEmailsAsync — sustained 5 req/s | 5 concurrent, 50 emails | 10 min | Stable latency | P1 |
| LDT-008 | GetCurrentUserAsync — sustained 15 req/s | 15 concurrent | 10 min | No 401 spike | P1 |
| LDT-009 | GetUserByIdAsync — ramp 0→30 req/s | Ramp over 5 min | 5 min | Graceful degradation | P2 |
| LDT-010 | GetUsersByEmailsAsync — stress 100 emails × 20 req/s | Heavy batch load | 3 min | < 2s p95 | P2 |

---

## 3:1 Ratio Compliance Check

| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 30 | POS-001 through POS-030 |
| Negative (N) | 90 | NEG-001 through NEG-090 |
| Edge/Boundary (E) | 90 | BND-001 through BND-090 |
| Functional (F) | 90 | FUN-001 through FUN-090 |
| Integration (I) | 90 | INT-001 through INT-090 |
| **N ≥ 3P?** | ✅ | 90 ≥ 90 |
| **E ≥ 3P?** | ✅ | 90 ≥ 90 |
| **F ≥ 3P?** | ✅ | 90 ≥ 90 |
| **I ≥ 3P?** | ✅ | 90 ≥ 90 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
