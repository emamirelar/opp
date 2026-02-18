# SystemAdminManager — Comprehensive Test Cases

**Component:** `SystemAdminController`, `SystemAdminManager`, `UNOPSSystemAdminManager`, `GenericSeedRunner`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (N≥3P, E≥3P, F≥3P, I≥3P)

---

## Implementation Status

| Component | Path | Status |
|-----------|------|--------|
| ISystemAdminManager | `UNOPS.PAO.Business/Interfaces/ISystemAdminManager.cs` | ✅ Implemented |
| SystemAdminManager (Base) | `UNOPS.PAO.Business/Managers/SystemAdminManager.cs` | ✅ Implemented (no-op base) |
| UNOPSSystemAdminManager | `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSSystemAdminManager.cs` | ✅ Implemented |
| SystemAdminController | `UNOPS.PAO.Presentation/Controllers/Admin/SystemAdminController.cs` | ✅ Implemented |
| GenericSeedRunner | `UNOPS.PAO.UNOPSDataAccess/Seed/GenericSeedRunner.cs` | ✅ Implemented |

### API Endpoints: `/api/system-admin`

| Method | Route | Permission |
|--------|-------|------------|
| GET | `/api/system-admin/endpoints` | CanRunMigrations |
| GET | `/api/system-admin/auth-debug` | Authenticated |
| GET | `/api/system-admin/migrations/run` | CanRunMigrations |
| GET | `/api/system-admin/seeding/run` | CanRunSeedings |
| GET | `/api/system-admin/seeding/run/{name}` | CanRunSeedings |
| GET | `/api/system-admin/seed-scripts/truncate` | CanRunSeedings |
| GET | `/api/system-admin/seed-scripts/delete/{name}` | CanRunSeedings |
| GET | `/api/system-admin/output-embeddings/generate` | CanRunSeedings |
| POST | `/api/system-admin/clean-up-users` | CanRunSeedings |

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30=90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30=90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30=90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30=90 | ✅ |
| 6 | Security Tests | §6 | 50 | ≥50 | ✅ |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Mandatory Ratio Compliance Checks

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| N ≥ 3P | Negative ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## §1 Positive Tests (30)

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Migrations (5)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-001 | RunMigrations applies pending migrations | Pending migrations exist | GET /api/system-admin/migrations/run | 200 OK, MigrateAsync completes, schema updated | P0 |
| POS-002 | RunMigrations with no pending migrations | Database up to date | GET /api/system-admin/migrations/run | 200 OK, no changes applied | P0 |
| POS-003 | RunMigrations uses correct DbContext (UNOPS override) | IsUNOPSOverride=true | GET migrations/run | UNOPSAppDbContext.Database.MigrateAsync called | P0 |
| POS-004 | RunMigrations uses base DbContext (non-UNOPS) | IsUNOPSOverride=false | GET migrations/run | AppDbContext.Database.MigrateAsync called | P1 |
| POS-005 | Migrations endpoint returns success message | Any state | GET migrations/run | Response includes completion status | P1 |

### Seeding (10)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-006 | RunSeeding executes all configured seeds | Seed scripts configured | GET /api/system-admin/seeding/run | GenericSeedRunner.ExecuteConfiguredSeedsAsync completes, SeedScript records created | P0 |
| POS-007 | RunSeeding skips already-executed seeds | Some seeds previously run | GET seeding/run | Only new/changed seeds execute | P0 |
| POS-008 | RunSpecificSeeder runs named seeder | Seeder "RoleSeeder" exists | GET /api/system-admin/seeding/run/RoleSeeder | GenericSeedRunner.ExecuteSpecificSeederAsync("RoleSeeder") called | P0 |
| POS-009 | RunSpecificSeeder records SeedScript entry | Valid seeder name | GET seeding/run/TestSeeder | SeedScript row created with ScriptName, LastExecutedDate | P0 |
| POS-010 | RunSeeding on base manager is no-op | IsUNOPSOverride=false | GET seeding/run | Returns success, no seeding executed | P1 |
| POS-011 | Seeding updates FileHash for changed scripts | Script content changed | GET seeding/run | FileHash updated in SeedScript table | P1 |
| POS-012 | Seeding respects ExecutionOrder | Multiple seeds with order | GET seeding/run | Seeds execute in ExecutionOrder | P1 |
| POS-013 | Seeding handles empty seed list | No configured seeds | GET seeding/run | Returns success, no operations | P1 |
| POS-014 | RunSpecificSeeder with exact case-sensitive name | "StateMachineStageChangeSeeder" | GET seeding/run/StateMachineStageChangeSeeder | Correct seeder found and executed | P1 |
| POS-015 | Seeding populates Description field | Seeder with description | GET seeding/run | SeedScript.Description populated | P2 |

### SeedScript Management (5)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-016 | TruncateSeedScripts clears all records | SeedScripts table has data | GET /api/system-admin/seed-scripts/truncate | Table truncated, identity restarted | P0 |
| POS-017 | DeleteSeedScript removes specific record | SeedScript "RoleSeeder" exists | GET /api/system-admin/seed-scripts/delete/RoleSeeder | Record removed from table | P0 |
| POS-018 | DeleteSeedScript by exact ScriptName match | Multiple records | GET seed-scripts/delete/TestSeeder | Only matching record deleted | P0 |
| POS-019 | TruncateSeedScripts on empty table | Table already empty | GET seed-scripts/truncate | Success, no error | P1 |
| POS-020 | DeleteSeedScript saves changes to DB | Valid script name | GET seed-scripts/delete/X | SaveChangesAsync called after Remove | P1 |

### System Admin Utilities (5)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-021 | GetEndpoints lists available admin endpoints | Admin user | GET /api/system-admin/endpoints | Returns list of system admin endpoints | P0 |
| POS-022 | AuthDebug returns current user auth info | Authenticated user | GET /api/system-admin/auth-debug | Returns user ID, email, roles, claims, permissions | P0 |
| POS-023 | GenerateOutputEmbeddings creates embeddings | Output entities exist | GET /api/system-admin/output-embeddings/generate | OutputEmbeddingSeeder executes, embeddings generated | P0 |
| POS-024 | CleanUpUsers executes cleanup SQL | AspNetUsers table has orphans | POST /api/system-admin/clean-up-users | SQL script executes, orphaned users cleaned | P0 |
| POS-025 | AuthDebug shows all permission flags | User with multiple permissions | GET auth-debug | All permission flags visible (CanRunMigrations, CanRunSeedings, etc.) | P1 |

### Configuration & Diagnostics (5)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-026 | Endpoints response includes correct route paths | Admin | GET endpoints | Each endpoint shows method + route pattern | P1 |
| POS-027 | AuthDebug returns email from claims | User with email claim | GET auth-debug | Email matches authenticated user | P1 |
| POS-028 | AuthDebug returns IsInternal flag | Internal user | GET auth-debug | IsInternal=true shown | P2 |
| POS-029 | Migration status after successful run | Migrations applied | GET migrations/run then check | DB schema version advanced | P1 |
| POS-030 | System admin controller inherits BaseController | Architecture check | Code review | Inherits BaseController with logger, auth, user resolver | P2 |

---

## §2 Negative Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Permission Denied (20)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | RunMigrations without CanRunMigrations permission | User lacks CanRunMigrations | 403 Forbidden | P0 |
| NEG-002 | RunSeeding without CanRunSeedings permission | User lacks CanRunSeedings | 403 Forbidden | P0 |
| NEG-003 | RunSpecificSeeder without CanRunSeedings | Regular user | 403 Forbidden | P0 |
| NEG-004 | TruncateSeedScripts without CanRunSeedings | Regular user | 403 Forbidden | P0 |
| NEG-005 | DeleteSeedScript without CanRunSeedings | Regular user | 403 Forbidden | P0 |
| NEG-006 | GenerateEmbeddings without CanRunSeedings | Regular user | 403 Forbidden | P0 |
| NEG-007 | CleanUpUsers without CanRunSeedings | Regular user | 403 Forbidden | P0 |
| NEG-008 | GetEndpoints without CanRunMigrations | Regular user | 403 Forbidden | P0 |
| NEG-009 | Unauthenticated access to migrations | No auth header | 401 Unauthorized | P0 |
| NEG-010 | Unauthenticated access to seeding | No auth header | 401 Unauthorized | P0 |
| NEG-011 | Unauthenticated access to auth-debug | No auth header | 401 Unauthorized | P0 |
| NEG-012 | Unauthenticated access to endpoints list | No auth header | 401 Unauthorized | P0 |
| NEG-013 | Unauthenticated access to truncate | No auth header | 401 Unauthorized | P0 |
| NEG-014 | Unauthenticated access to delete seed script | No auth header | 401 Unauthorized | P0 |
| NEG-015 | Unauthenticated generate embeddings | No auth header | 401 Unauthorized | P0 |
| NEG-016 | Unauthenticated clean up users | No auth header | 401 Unauthorized | P0 |
| NEG-017 | Expired token on migration endpoint | Expired JWT | 401 Unauthorized | P1 |
| NEG-018 | Invalid token format on seeding | Malformed token | 401 Unauthorized | P1 |
| NEG-019 | User with CanRunSeedings but not CanRunMigrations on migrations | Partial permissions | 403 Forbidden | P1 |
| NEG-020 | User with CanRunMigrations but not CanRunSeedings on seeding | Partial permissions | 403 Forbidden | P1 |

### Invalid Seeder Operations (20)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-021 | RunSpecificSeeder with non-existent seeder name | Name="NonExistentSeeder" | Error: seeder not found or graceful handling | P0 |
| NEG-022 | RunSpecificSeeder with empty name | Name="" | 404 or route mismatch | P0 |
| NEG-023 | RunSpecificSeeder with null name | Name=null | Route mismatch or error | P1 |
| NEG-024 | RunSpecificSeeder with special characters | Name="<script>alert(1)</script>" | No injection, seeder not found | P0 |
| NEG-025 | RunSpecificSeeder with SQL injection attempt | Name="'; DROP TABLE SeedScripts--" | No SQL execution, seeder not found | P0 |
| NEG-026 | RunSpecificSeeder with very long name (5000 chars) | Name=string(5000) | Handled gracefully (seeder not found or truncation) | P1 |
| NEG-027 | RunSpecificSeeder with path traversal | Name="../../etc/passwd" | No file access, seeder not found | P1 |
| NEG-028 | DeleteSeedScript with non-existent script name | Name="DoesNotExist" | Error or graceful handling (record not found) | P0 |
| NEG-029 | DeleteSeedScript with empty name | Name="" | 404 or error | P1 |
| NEG-030 | DeleteSeedScript with special characters | Name="drop_table" | Only exact match lookup, no injection | P0 |
| NEG-031 | RunSeeding when database connection fails | DB unavailable | 500 Internal Server Error with appropriate message | P0 |
| NEG-032 | RunMigrations when database connection fails | DB unavailable | 500 Internal Server Error | P0 |
| NEG-033 | RunSpecificSeeder when seeder throws exception | Seeder has bug | 500 with error details (dev), generic error (prod) | P0 |
| NEG-034 | RunSeeding when SeedScript table doesn't exist | Missing table | Error handled, appropriate message | P1 |
| NEG-035 | TruncateSeedScripts when DB connection drops mid-operation | Connection interruption | Transaction rollback or error | P1 |
| NEG-036 | DeleteSeedScript with URL-encoded special characters | Name="seed%20script" | Proper URL decoding, record not found | P2 |
| NEG-037 | RunSpecificSeeder with whitespace-only name | Name="   " | Seeder not found | P1 |
| NEG-038 | RunSeeding when GenericSeedRunner is not configured | No seed configuration | Graceful handling, no-op or empty result | P1 |
| NEG-039 | GenerateEmbeddings with no Output entities | Empty Outputs table | Graceful handling, no embeddings generated | P1 |
| NEG-040 | CleanUpUsers when SQL script fails | Invalid SQL or constraint violation | 500 with error, no partial cleanup | P1 |

### Database & Migration Failures (20)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-041 | RunMigrations with conflicting migration state | Pending migration conflicts with schema | MigrateAsync throws, 500 returned | P0 |
| NEG-042 | RunMigrations with corrupted migrations history | __EFMigrationsHistory table corrupted | Error with clear message | P0 |
| NEG-043 | RunMigrations when already running | Concurrent migration attempt | Second call waits or fails gracefully | P0 |
| NEG-044 | RunSeeding with duplicate ScriptName | Unique constraint on ScriptName | Error handled, existing record preserved | P1 |
| NEG-045 | RunSeeding when seed script throws BusinessException | Business rule violation in seeder | Exception propagated with message | P1 |
| NEG-046 | TruncateSeedScripts with foreign key references | FK constraint blocking truncate | SQL error, table not truncated | P1 |
| NEG-047 | RunMigrations with read-only database | DB permissions restricted | MigrateAsync fails, appropriate error | P1 |
| NEG-048 | DeleteSeedScript for already-deleted record | Record removed between find and delete | Concurrency error handled | P2 |
| NEG-049 | RunSeeding with timeout | Long-running seed | Timeout error, partial state documented | P1 |
| NEG-050 | RunMigrations with incompatible provider | Wrong DB provider | Migration fails with provider error | P2 |
| NEG-051 | CleanUpUsers with locked rows | Concurrent user session | SQL waits or times out | P2 |
| NEG-052 | GenerateEmbeddings with API failure | Embedding API unavailable | Error logged, operation fails gracefully | P1 |
| NEG-053 | RunSpecificSeeder when seeder modifies read-only entity | Constraint violation | Transaction rolled back | P2 |
| NEG-054 | RunMigrations with insufficient disk space | Disk full | Migration fails, error reported | P2 |
| NEG-055 | RunSeeding when ExecutionOrder has gaps | Order: 1, 5, 10 (missing 2-4) | Seeds still execute in available order | P2 |
| NEG-056 | DeleteSeedScript with concurrent truncate | Truncate during delete | One operation wins, consistent state | P2 |
| NEG-057 | RunMigrations with wrong connection string | Invalid credentials | Connection error, 500 | P1 |
| NEG-058 | RunSeeding with circular seed dependencies | SeedA depends on SeedB depends on SeedA | Deadlock or cycle detection | P2 |
| NEG-059 | AuthDebug when user claims are malformed | Missing NameIdentifier claim | Graceful handling, partial info returned | P1 |
| NEG-060 | GetEndpoints when reflection fails | Internal error | 500 or fallback response | P2 |

### Input Validation & Edge Cases (30)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-061 | POST to GET-only migration endpoint | POST /api/system-admin/migrations/run | 405 Method Not Allowed | P0 |
| NEG-062 | PUT to seeding endpoint | PUT /api/system-admin/seeding/run | 405 Method Not Allowed | P1 |
| NEG-063 | DELETE to seed-scripts/truncate | DELETE method | 405 Method Not Allowed | P1 |
| NEG-064 | GET to clean-up-users (requires POST) | GET /api/system-admin/clean-up-users | 405 Method Not Allowed | P0 |
| NEG-065 | Request with extremely large headers | 1MB header | 431 Request Header Fields Too Large or 400 | P2 |
| NEG-066 | RunSpecificSeeder with Unicode name | Name="シーダー" | Seeder not found (ASCII names only) | P2 |
| NEG-067 | RunSpecificSeeder with dots in name | Name="seed.runner.v2" | Seeder not found or found if naming allows | P2 |
| NEG-068 | DeleteSeedScript with forward slashes | Name="path/to/script" | No path traversal, record not found | P1 |
| NEG-069 | Multiple rapid migration calls | 10 calls in 1 second | Only one executes, others queue or fail | P1 |
| NEG-070 | CleanUpUsers with empty POST body | No body | Executes (no body required) or 400 | P1 |
| NEG-071 | CleanUpUsers with malformed JSON body | Invalid JSON | 400 Bad Request | P1 |
| NEG-072 | RunSeeding after TruncateSeedScripts | Scripts cleared, then seed | All seeds re-execute (no tracking records) | P0 |
| NEG-073 | DeleteSeedScript then RunSeeding | Script deleted then seeding | Deleted seeder re-runs (no record blocking it) | P1 |
| NEG-074 | RunMigrations on fresh database | No existing schema | All migrations applied from start | P0 |
| NEG-075 | AuthDebug for external (non-internal) user | IsInternal=false | IsInternal flag correctly shown as false | P1 |
| NEG-076 | RunSeeding with no UNOPS override active | Base manager (no-op) | Returns success, no seeding occurs | P1 |
| NEG-077 | RunSpecificSeeder with no UNOPS override | Base manager | Returns success, no-op | P1 |
| NEG-078 | TruncateSeedScripts with no UNOPS override | Base manager | No-op | P1 |
| NEG-079 | DeleteSeedScript with no UNOPS override | Base manager | No-op | P1 |
| NEG-080 | Concurrent TruncateSeedScripts calls | Two simultaneous truncate | One succeeds, other is no-op or waits | P2 |
| NEG-081 | RunMigrations returns proper error on partial failure | Migration 3 of 5 fails | First 2 rolled back (if transactional) or error state documented | P1 |
| NEG-082 | RunSeeding with seeder that creates millions of rows | Very large seed | Timeout or memory limit reached | P2 |
| NEG-083 | GenerateEmbeddings with extremely large Output content | 10MB+ output text | Memory handled, chunking or error | P2 |
| NEG-084 | CleanUpUsers when no orphaned users exist | Clean AspNetUsers table | Success, 0 rows affected | P1 |
| NEG-085 | RunSpecificSeeder with case-insensitive name match | Name="roleseeder" vs "RoleSeeder" | Exact match or case-insensitive depending on implementation | P2 |
| NEG-086 | AuthDebug returns empty permissions for new user | New user, no roles | Empty permission list | P1 |
| NEG-087 | Request to non-existent system-admin sub-route | GET /api/system-admin/nonexistent | 404 Not Found | P1 |
| NEG-088 | RunMigrations with very long timeout | Slow migration | Eventually completes or times out gracefully | P2 |
| NEG-089 | GetEndpoints on base controller without UNOPS endpoints | Non-UNOPS mode | Only base endpoints listed | P2 |
| NEG-090 | RunSeeding called twice rapidly | Double-click scenario | Second call either queues or returns "already running" | P1 |

---

## §3 Boundary/Edge Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Seeder Name Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-001 | Seeder name with exactly 1 character | Name="A" | Seeder lookup attempted | P1 |
| BND-002 | Seeder name with max reasonable length (255 chars) | Name=string(255) | Handled without truncation | P1 |
| BND-003 | Seeder name with spaces | Name="Role Seeder" | Seeder not found (no spaces in class names) | P1 |
| BND-004 | Seeder name matching partial class name | Name="Role" vs "RoleSeeder" | Exact match only | P1 |
| BND-005 | Seeder name with trailing whitespace | Name="RoleSeeder " | Trimmed or exact match | P2 |
| BND-006 | Seeder name with leading whitespace | Name=" RoleSeeder" | Trimmed or not found | P2 |
| BND-007 | ScriptName in SeedScripts at max column length | Max varchar length | Stored without truncation | P2 |
| BND-008 | SeedScript.Description at null | No description | Null stored, no error | P2 |
| BND-009 | SeedScript.ExecutionOrder = 0 | Zero order | Executes first or handled | P2 |
| BND-010 | SeedScript.ExecutionOrder = int.MaxValue | Maximum order | Executes last | P2 |
| BND-011 | SeedScript.ExecutionOrder negative | Negative order | Handled (executes before 0?) | P2 |
| BND-012 | FileHash comparison: same content | No changes | Seed skipped (hash matches) | P1 |
| BND-013 | FileHash comparison: single byte changed | Minimal change | Hash differs, seed re-executes | P1 |
| BND-014 | FileHash for empty seed script | Zero-length script | Hash computed, record created | P2 |
| BND-015 | Multiple seeders with same ExecutionOrder | Duplicate order values | All execute (order may be non-deterministic) | P2 |

### Migration State Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-016 | Zero pending migrations | Fresh state | MigrateAsync no-ops | P0 |
| BND-017 | Exactly 1 pending migration | Single new migration | Applied successfully | P0 |
| BND-018 | 50+ pending migrations | Many accumulated | All applied in order | P1 |
| BND-019 | Migration with Up() only (no Down()) | Irreversible migration | Applied, cannot rollback | P1 |
| BND-020 | Migration creating index on large table | Performance-sensitive | Completes within timeout | P2 |
| BND-021 | Migration altering column type | Schema change | Applied, data preserved or migrated | P1 |
| BND-022 | Migration adding NOT NULL column with default | Schema change | Applied, existing rows get default value | P1 |
| BND-023 | Migration dropping table | Destructive migration | Applied (if Down exists for rollback) | P2 |
| BND-024 | First migration on empty database | Bootstrap | Schema created from scratch | P0 |
| BND-025 | __EFMigrationsHistory empty but schema exists | Manual schema creation | Migrations skip or detect conflict | P2 |
| BND-026 | __EFMigrationsHistory has future migration entry | Time travel scenario | Current migration may skip or error | P2 |
| BND-027 | Database has migrations from different model | Model mismatch | Error on migration or warning | P1 |
| BND-028 | UNOPSAppDbContext includes all base AppDbContext entities | Inheritance | All base entities available in UNOPS context | P0 |
| BND-029 | Base SystemAdminManager.RunMigrations uses AppDbContext | Non-UNOPS | AppDbContext.Database.MigrateAsync used | P0 |
| BND-030 | UNOPS SystemAdminManager.RunMigrations uses UNOPSAppDbContext | UNOPS mode | UNOPSAppDbContext.Database.MigrateAsync used | P0 |

### SeedScript Table Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-031 | TRUNCATE restarts identity counter | After truncate | Next insert gets Id=1 | P0 |
| BND-032 | TRUNCATE on table with 1000+ rows | Large table | All rows removed, fast execution | P1 |
| BND-033 | Delete last remaining SeedScript | Only 1 record | Table empty after delete | P1 |
| BND-034 | Delete first of many SeedScripts | First record | Others unaffected | P1 |
| BND-035 | SeedScript.LastExecutedDate precision | Timestamp comparison | DateTime stored with full precision | P2 |
| BND-036 | SeedScript with very long ScriptName | 500+ char name | Column handles or truncates | P2 |
| BND-037 | SeedScript.FileHash with SHA-256 format | Standard hash | 64-char hex string stored | P1 |
| BND-038 | SeedScript.FileHash null | No hash computed | Null allowed or default | P2 |
| BND-039 | Truncate followed by immediate seeding | No time gap | All seeds re-execute | P0 |
| BND-040 | Delete specific script followed by targeted reseed | Delete then run specific | Deleted script re-runs | P0 |
| BND-041 | SeedScript with ScriptType distinctions | Different types | Correctly categorized | P2 |
| BND-042 | SeedScript table concurrent read+write | Read during seed | Consistent reads | P1 |
| BND-043 | TRUNCATE TABLE syntax for PostgreSQL | Raw SQL | "TRUNCATE TABLE public.\"SeedScripts\" RESTART IDENTITY" executes | P0 |
| BND-044 | Delete uses Entity Framework Remove+Save pattern | ORM operation | context.Remove(entity) + SaveChangesAsync() | P0 |
| BND-045 | SeedScript lookup by ScriptName is case-sensitive | PostgreSQL default | Exact match or CI collation | P1 |

### AuthDebug & Endpoints Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-046 | AuthDebug with user having 0 permissions | New user | Empty permissions list | P0 |
| BND-047 | AuthDebug with user having all permissions | Super admin | All permissions listed | P0 |
| BND-048 | AuthDebug with user having 1 permission | Single permission | Exactly 1 permission shown | P1 |
| BND-049 | AuthDebug returns NameIdentifier claim | Standard claim | User ID shown | P0 |
| BND-050 | AuthDebug returns Email claim | Email claim present | Email address shown | P0 |
| BND-051 | AuthDebug with missing email claim | No email claim | Graceful handling (null or empty) | P1 |
| BND-052 | AuthDebug with multiple role claims | User in 3 roles | All roles listed | P1 |
| BND-053 | Endpoints list includes all 9 endpoints | Complete list | 9 endpoint entries returned | P0 |
| BND-054 | Endpoints list shows HTTP method for each | GET/POST | Correct method per endpoint | P1 |
| BND-055 | Endpoints list shows route template | URL patterns | Full route with parameters shown | P1 |
| BND-056 | AuthDebug response includes IsInternal flag | Any user | IsInternal field present | P1 |
| BND-057 | AuthDebug for user with expired but valid token | Token at expiry boundary | Response returned or 401 | P2 |
| BND-058 | Endpoints list consistent across calls | Multiple requests | Same endpoint list returned | P1 |
| BND-059 | AuthDebug with custom claims | Non-standard claims | Custom claims visible in response | P2 |
| BND-060 | AuthDebug does not cache between users | Different users | Each user sees own auth info | P0 |

### Embeddings & Cleanup Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-061 | GenerateEmbeddings with 0 Output entities | Empty table | No embeddings generated, success | P0 |
| BND-062 | GenerateEmbeddings with 1 Output entity | Minimal data | 1 embedding generated | P0 |
| BND-063 | GenerateEmbeddings with 100 Output entities | Moderate data | All embeddings generated | P1 |
| BND-064 | GenerateEmbeddings with 1000+ Output entities | Large dataset | Batched or all generated | P1 |
| BND-065 | GenerateEmbeddings with empty Output.Content | No text to embed | Null or zero-vector embedding | P1 |
| BND-066 | GenerateEmbeddings with very long content (50K chars) | Large text | Chunked or truncated for embedding | P2 |
| BND-067 | CleanUpUsers with 0 orphaned records | Clean state | 0 rows affected | P0 |
| BND-068 | CleanUpUsers with 1 orphaned user | Minimal cleanup | 1 row cleaned | P0 |
| BND-069 | CleanUpUsers with 100 orphaned users | Moderate cleanup | All cleaned | P1 |
| BND-070 | CleanUpUsers preserves active users | Active + orphaned mix | Only orphans removed, active preserved | P0 |
| BND-071 | CleanUpUsers SQL script handles NULL email | User with no email | Handled in WHERE clause | P1 |
| BND-072 | CleanUpUsers SQL uses correct table schema | PostgreSQL schema | Correct schema.table reference | P1 |
| BND-073 | Embeddings idempotent (re-running same data) | Second run | Existing embeddings updated or skipped | P1 |
| BND-074 | CleanUpUsers within transaction | Transactional cleanup | All-or-nothing cleanup | P1 |
| BND-075 | GenerateEmbeddings timeout handling | API slow | Timeout after configured duration | P2 |

### Override Pattern Boundaries (15)

| ID | Test Name | Boundary Condition | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| BND-076 | Base SystemAdminManager.RunSeeding is no-op | Non-UNOPS | Method returns without action | P0 |
| BND-077 | Base SystemAdminManager.RunSpecificSeeder is no-op | Non-UNOPS | Method returns without action | P0 |
| BND-078 | Base SystemAdminManager.TruncateSeedScripts is no-op | Non-UNOPS | No SQL executed | P0 |
| BND-079 | Base SystemAdminManager.DeleteSeedScript is no-op | Non-UNOPS | No record deleted | P0 |
| BND-080 | UNOPS override registered in UNOPSManagerWrapper | DI container | UNOPSSystemAdminManager resolved | P0 |
| BND-081 | Base manager registered in ManagerWrapper | DI container | SystemAdminManager resolved | P0 |
| BND-082 | IsUNOPSOverride=true selects UNOPS manager | Runtime config | UNOPS implementation used | P0 |
| BND-083 | IsUNOPSOverride=false selects base manager | Runtime config | Base implementation used | P0 |
| BND-084 | Manager implements ISystemAdminManager interface | Contract | All 5 methods implemented | P0 |
| BND-085 | UNOPS manager calls GenericSeedRunner (not base) | Override | GenericSeedRunner.ExecuteConfiguredSeedsAsync called | P0 |
| BND-086 | UNOPS manager uses UNOPSAppDbContext for SeedScript | Data access | Correct context for entity access | P0 |
| BND-087 | Base manager uses AppDbContext for migrations | Data access | AppDbContext.Database.MigrateAsync | P0 |
| BND-088 | Both managers constructor-inject IMapper and DbContext | DI | Dependencies resolved correctly | P1 |
| BND-089 | Controller resolves correct manager via IManagerWrapper | Runtime | SystemAdminManager or UNOPSSystemAdminManager based on config | P0 |
| BND-090 | Switching override at runtime (if supported) | Config change | Correct manager used after switch | P2 |

---

## §4 Functional Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Migration Functional Flow (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-001 | Full migration lifecycle: pending → applied → up-to-date | Start with pending migrations | After run: no pending, schema matches model | P0 |
| FUN-002 | Migration creates new table | AddNewEntityMigration | Table exists in database after migration | P0 |
| FUN-003 | Migration adds column to existing table | AddColumnMigration | Column exists, existing data preserved | P0 |
| FUN-004 | Migration creates index | AddIndexMigration | Index exists in pg_indexes | P1 |
| FUN-005 | Migration seeds initial data | Data migration | Rows inserted by migration | P1 |
| FUN-006 | Migration error rolls back partial changes | Migration 2 of 3 fails | Schema stays at pre-migration state (if transactional) | P0 |
| FUN-007 | Migrations history updated after successful run | Apply migrations | __EFMigrationsHistory has new entries | P0 |
| FUN-008 | MigrateAsync handles PendingModelChangesWarning | Warning configured to be ignored | Migration proceeds despite warning | P1 |
| FUN-009 | Migration uses correct schema ("public") | PostgreSQL default schema | Tables created in "public" schema | P0 |
| FUN-010 | Migration handles schema "public" setting from UNOPSAppDbContext | DbContext config | HasDefaultSchema("public") applied | P1 |
| FUN-011 | Migration creates SeedScripts table | First UNOPS migration | public."SeedScripts" table created | P0 |
| FUN-012 | Migration connection uses correct PostgreSQL provider | Provider config | UseNpgsql configured | P0 |
| FUN-013 | Migration respects connection pool settings | Pool config | Connections reused, not exhausted | P1 |
| FUN-014 | Migration handles long-running DDL statements | ALTER TABLE on large table | Completes within reasonable timeout | P2 |
| FUN-015 | Migration output logged | Logger injected | Migration steps visible in logs | P1 |

### Seeding Functional Flow (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-016 | Full seeding lifecycle: configure → execute → verify | First run | All configured seeds execute in order | P0 |
| FUN-017 | Seeding creates SeedScript tracking records | First seed | SeedScript row with ScriptName, FileHash, LastExecutedDate | P0 |
| FUN-018 | Seeding skips unchanged scripts (hash match) | Re-run without changes | Scripts not re-executed, "skipped" logged | P0 |
| FUN-019 | Seeding re-runs changed scripts (hash mismatch) | Script content modified | Script re-executes, FileHash updated | P0 |
| FUN-020 | Seeding respects ExecutionOrder | Seeds with order 1, 2, 3 | Execute in order 1→2→3 | P0 |
| FUN-021 | RunSpecificSeeder finds seeder by exact name | Valid name | Correct seeder class instantiated and run | P0 |
| FUN-022 | Seeding creates required lookup data | RoleSeeder, EntityTypeSeeder | Roles, entity types present in DB | P0 |
| FUN-023 | Seeding creates StateMachineStageChanges | StageChangeSeeder | Workflow transitions seeded | P0 |
| FUN-024 | Seeding creates StateMachineStageChangeRoles | RoleSeeder | Role permissions for transitions seeded | P0 |
| FUN-025 | Seeding preserves existing data (additive) | Data already exists | New data added, existing not duplicated | P0 |
| FUN-026 | Seeding handles DbUpdateException for duplicates | Duplicate key | Caught and logged, seeding continues | P1 |
| FUN-027 | Seeding transaction per script | Script 2 fails | Script 1 changes preserved, script 2 rolled back | P1 |
| FUN-028 | Specific seeder can be re-run after delete of tracking record | Delete then re-seed | Seeder executes again | P0 |
| FUN-029 | GenericSeedRunner discovers seeders by convention | Assembly scanning | All ISeed implementations found | P1 |
| FUN-030 | Seeding log shows execution time per script | Performance logging | Duration logged for each seed | P2 |

### SeedScript Management Functional (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-031 | Truncate removes all records and resets identity | 10 records → truncate | Table empty, next Id=1 | P0 |
| FUN-032 | Delete removes single record by ScriptName | 3 records, delete middle | 2 records remain, correct one removed | P0 |
| FUN-033 | Delete uses FindFirst by ScriptName | Lookup | FirstOrDefault on ScriptName field | P0 |
| FUN-034 | Delete calls context.Remove then SaveChanges | ORM pattern | Entity tracked as Deleted, then persisted | P0 |
| FUN-035 | Truncate uses raw SQL execution | ExecuteSqlRawAsync | TRUNCATE TABLE statement sent to PostgreSQL | P0 |
| FUN-036 | Truncate includes RESTART IDENTITY | SQL syntax | Identity sequence reset to 1 | P0 |
| FUN-037 | Delete-then-seed forces re-execution of specific seeder | Targeted reset | Only deleted seeder re-runs, others skip | P0 |
| FUN-038 | Truncate-then-seed forces re-execution of all seeders | Full reset | All seeders re-run as if first time | P0 |
| FUN-039 | SeedScript table schema is "public" | PostgreSQL | public."SeedScripts" is correct table reference | P1 |
| FUN-040 | SeedScript.ScriptName matches seeder class name | Convention | Exact class name stored (e.g., "RoleSeeder") | P1 |
| FUN-041 | SeedScript.ScriptType distinguishes seed types | If applicable | Type field properly categorized | P2 |
| FUN-042 | SeedScript inherits BaseBusinessEntity | Entity hierarchy | Has Id, IsDeleted, audit fields | P1 |
| FUN-043 | Multiple deletes in sequence | Delete A, Delete B, Delete C | All three removed correctly | P1 |
| FUN-044 | Truncate on table with cascade constraints | If FK exists | Cascade or error documented | P2 |
| FUN-045 | SeedScript.LastExecutedDate updated on re-execution | Re-run | Timestamp updated to current UTC | P1 |

### AuthDebug Functional (10)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-046 | AuthDebug returns all claims from token | IAP token | All standard + custom claims listed | P0 |
| FUN-047 | AuthDebug returns user's permission names | Permission check | CanRunMigrations, CanRunSeedings, etc. listed | P0 |
| FUN-048 | AuthDebug returns user roles | Role claims | All assigned roles shown | P0 |
| FUN-049 | AuthDebug returns NameIdentifier as user ID | Standard claim | User ID correctly parsed from token | P0 |
| FUN-050 | AuthDebug returns IsInternal status | Internal/external user | Correctly reflects user type | P1 |
| FUN-051 | AuthDebug for admin vs regular user | Different roles | Different permission sets shown | P1 |
| FUN-052 | AuthDebug does not modify any data | Read-only operation | No database writes | P0 |
| FUN-053 | AuthDebug response is JSON formatted | Content type | application/json with proper structure | P1 |
| FUN-054 | AuthDebug works with IAP authentication scheme | Auth scheme | [Authorize(AuthenticationSchemes = "IAP")] respected | P0 |
| FUN-055 | AuthDebug with proxy/forwarded headers | Behind load balancer | Correct user resolved despite forwarding | P2 |

### Embedding & Cleanup Functional (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-056 | GenerateEmbeddings processes all Output entities | 10 outputs | 10 embeddings created | P0 |
| FUN-057 | GenerateEmbeddings uses OutputEmbeddingSeeder | Internal | Seeder class invoked correctly | P0 |
| FUN-058 | GenerateEmbeddings stores vectors in database | Embedding storage | Vector data persisted | P0 |
| FUN-059 | GenerateEmbeddings handles duplicate runs | Idempotent | Existing embeddings updated or skipped | P1 |
| FUN-060 | GenerateEmbeddings uses correct model/API | AI service | Proper embedding model called | P1 |
| FUN-061 | CleanUpUsers executes predefined SQL script | Script execution | Raw SQL runs against AspNetUsers | P0 |
| FUN-062 | CleanUpUsers removes only orphaned records | Active + orphaned | Active users untouched | P0 |
| FUN-063 | CleanUpUsers returns affected row count | SQL result | Number of cleaned records returned | P1 |
| FUN-064 | CleanUpUsers handles concurrent user sessions | Active sessions | Does not remove currently logged-in users | P0 |
| FUN-065 | CleanUpUsers preserves FK integrity | Related records exist | No FK violations | P0 |
| FUN-066 | Embedding vector dimensions correct | AI model spec | Consistent dimensions for all embeddings | P1 |
| FUN-067 | CleanUpUsers SQL is PostgreSQL compatible | SQL dialect | No SQL Server-specific syntax | P1 |
| FUN-068 | GenerateEmbeddings logs progress | Logger | Each entity processing logged | P2 |
| FUN-069 | CleanUpUsers logs cleaned records | Logger | Count and details logged | P1 |
| FUN-070 | Both operations require POST/GET respectively | HTTP methods | Correct method enforced | P0 |

### Controller & Routing Functional (20)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| FUN-071 | All 9 endpoints accessible under /api/system-admin | Route resolution | Each endpoint returns 200 (with auth) | P0 |
| FUN-072 | Controller uses [Authorize(AuthenticationSchemes = "IAP")] | Auth attribute | IAP scheme enforced | P0 |
| FUN-073 | PermissionAuthorize attribute on migration endpoints | Permission check | CanRunMigrations checked | P0 |
| FUN-074 | PermissionAuthorize attribute on seeding endpoints | Permission check | CanRunSeedings checked | P0 |
| FUN-075 | Controller inherits BaseController | Architecture | Logger, AuthService, UserResolver available | P0 |
| FUN-076 | Controller receives ISystemAdminManager via DI | Constructor injection | Correct manager injected | P0 |
| FUN-077 | Route parameter {name} correctly bound for specific seeder | URL parameter | Name parameter received by action method | P0 |
| FUN-078 | Route parameter {name} correctly bound for delete script | URL parameter | Name parameter received | P0 |
| FUN-079 | APIDictionary.SystemAdmin = "/api/system-admin" | Constant | Route prefix correct | P0 |
| FUN-080 | All endpoints return appropriate status codes | HTTP standards | 200, 400, 401, 403, 404, 500 as appropriate | P0 |
| FUN-081 | Error responses use ProblemDetails format | Error handling | Structured error response | P1 |
| FUN-082 | Successful operations return success flag/message | Response format | Confirmation of action taken | P0 |
| FUN-083 | Migration endpoint uses GET method | HTTP convention | GET for idempotent migration check+apply | P1 |
| FUN-084 | Seeding endpoints use GET method | HTTP convention | GET for trigger | P1 |
| FUN-085 | CleanUpUsers uses POST method | HTTP convention | POST for state-changing operation | P1 |
| FUN-086 | Content negotiation returns JSON | Accept header | application/json responses | P1 |
| FUN-087 | Controller actions are async | Architecture | All Task-returning methods | P1 |
| FUN-088 | Controller logs operations | ILogger<SystemAdminController> | Operations logged at appropriate levels | P1 |
| FUN-089 | Controller handles exceptions via GlobalExceptionHandler | Exception pipeline | Unhandled exceptions caught and formatted | P0 |
| FUN-090 | CORS allows system admin requests from allowed origins | CORS config | Admin UI origin allowed | P2 |

---

## §5 Integration Tests (90)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### API Endpoint Integration (20)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-001 | GET /api/system-admin/endpoints returns JSON array | Full HTTP | 200 OK, JSON array of endpoint descriptions | P0 |
| INT-002 | GET /api/system-admin/auth-debug returns auth info | Full HTTP | 200 OK, JSON with claims, permissions, roles | P0 |
| INT-003 | GET /api/system-admin/migrations/run executes migrations | Full HTTP + DB | 200 OK, database schema updated | P0 |
| INT-004 | GET /api/system-admin/seeding/run executes seeds | Full HTTP + DB | 200 OK, seed data created | P0 |
| INT-005 | GET /api/system-admin/seeding/run/{name} runs specific seeder | Full HTTP + DB | Named seeder executes | P0 |
| INT-006 | GET /api/system-admin/seed-scripts/truncate clears table | Full HTTP + DB | SeedScripts table empty | P0 |
| INT-007 | GET /api/system-admin/seed-scripts/delete/{name} removes record | Full HTTP + DB | Specific record removed | P0 |
| INT-008 | GET /api/system-admin/output-embeddings/generate creates embeddings | Full HTTP + AI | Embeddings generated and stored | P0 |
| INT-009 | POST /api/system-admin/clean-up-users runs cleanup | Full HTTP + DB | Orphaned users removed | P0 |
| INT-010 | All endpoints require IAP authentication | No auth | All return 401 | P0 |
| INT-011 | Migration endpoint with CanRunMigrations permission | Authorized user | 200 OK | P0 |
| INT-012 | Seeding endpoint with CanRunSeedings permission | Authorized user | 200 OK | P0 |
| INT-013 | Migration endpoint without permission | Unauthorized | 403 Forbidden | P0 |
| INT-014 | Seeding endpoint without permission | Unauthorized | 403 Forbidden | P0 |
| INT-015 | Endpoint list includes correct routes | Response parsing | Each route is valid and reachable | P1 |
| INT-016 | Auth debug returns current user's data | Identity check | User ID matches authenticated user | P1 |
| INT-017 | Migration followed by seeding | Sequential operations | Both complete successfully | P0 |
| INT-018 | Truncate followed by seeding | Reset + reseed | All seeds re-execute | P0 |
| INT-019 | Delete followed by specific reseed | Targeted reset | Only deleted seeder runs | P0 |
| INT-020 | Full admin workflow: migrate → seed → verify | End-to-end | System fully initialized | P0 |

### Database Integration (20)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-021 | MigrateAsync creates __EFMigrationsHistory entries | EF Core | New rows in migrations table | P0 |
| INT-022 | MigrateAsync creates new tables per migration | Schema check | pg_tables shows new tables | P0 |
| INT-023 | SeedScripts table created by migration | First run | public."SeedScripts" exists | P0 |
| INT-024 | SeedScript records created by seeding | Post-seed | SELECT from SeedScripts returns rows | P0 |
| INT-025 | TRUNCATE TABLE executes on PostgreSQL | Raw SQL | Table empty, identity reset | P0 |
| INT-026 | DELETE uses EF Core Remove pattern | ORM | Entity state tracked, SaveChanges persists | P0 |
| INT-027 | CleanUpUsers SQL executes on PostgreSQL | Raw SQL | No syntax errors, rows affected | P0 |
| INT-028 | Connection pooling supports admin operations | Pool config | No connection exhaustion during operations | P1 |
| INT-029 | Multiple admin operations share same transaction scope | If applicable | Consistent DB state | P1 |
| INT-030 | Database schema matches EF Core model after migration | Model validation | No model changes warning | P1 |
| INT-031 | Seeding creates roles in correct tables | Role data | EntityRoles, etc. populated | P0 |
| INT-032 | Seeding creates workflow stage changes | Workflow data | StateMachineStageChanges populated | P0 |
| INT-033 | Seeding creates workflow role permissions | Workflow data | StateMachineStageChangeRoles populated | P0 |
| INT-034 | EF Core configures PostgreSQL provider | Connection | UseNpgsql configured and connected | P0 |
| INT-035 | Admin operations respect connection string from configuration | IConfiguration | Correct database targeted | P0 |
| INT-036 | Multiplexing enabled in connection string | Pool config | NpgsqlConnectionStringBuilder.Multiplexing=true | P1 |
| INT-037 | MinPoolSize configured for warm connections | Pool config | MinPoolSize >= 10 | P1 |
| INT-038 | MaxPoolSize allows admin operations | Pool config | MaxPoolSize >= 100 | P1 |
| INT-039 | UNOPSAppDbContext inherits AppDbContext | Context hierarchy | All base entities accessible | P0 |
| INT-040 | UNOPSAppDbContext.OnModelCreating configures UNOPS entities | Model config | SeedScript entity configured | P0 |

### DI & Service Resolution Integration (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-041 | ISystemAdminManager resolved from DI container | Service provider | Instance returned | P0 |
| INT-042 | Correct manager resolved based on IsUNOPSOverride | Config flag | UNOPS or base manager | P0 |
| INT-043 | SystemAdminController receives ISystemAdminManager | Constructor injection | Not null | P0 |
| INT-044 | ManagerWrapper exposes SystemAdminManager | Facade | managerWrapper.SystemAdminManager accessible | P0 |
| INT-045 | UNOPSManagerWrapper overrides SystemAdminManager | Override | UNOPS implementation returned | P0 |
| INT-046 | GenericSeedRunner resolved in UNOPS manager | DI | Runner instance available | P0 |
| INT-047 | OutputEmbeddingSeeder resolved in controller | DI | Seeder instance available | P1 |
| INT-048 | ILogger<SystemAdminController> injected | DI | Logger functional | P1 |
| INT-049 | IAuthorizationService injected in controller | DI | Auth service available | P0 |
| INT-050 | UserResolverService<int> injected in controller | DI | User resolver available | P0 |
| INT-051 | DbContext lifetime is Scoped | DI scope | New context per request | P0 |
| INT-052 | SystemAdminManager constructor receives AppDbContext | DI | Context injected | P0 |
| INT-053 | UNOPSSystemAdminManager receives UNOPSAppDbContext | DI | UNOPS context injected | P0 |
| INT-054 | GenericSeedRunner receives IServiceProvider | DI | Service provider for seed instantiation | P1 |
| INT-055 | All admin service registrations are Scoped | Lifetime | Not Singleton (DB context is scoped) | P1 |

### Cross-System Integration (20)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-056 | Migration + Seeding produces fully functional system | Bootstrap | All entities, roles, permissions, workflows seeded | P0 |
| INT-057 | Seeded roles used by authorization handlers | Auth check | PermissionAuthorize works with seeded roles | P0 |
| INT-058 | Seeded workflow stages used by WorkflowController | Workflow check | Stage transitions available after seeding | P0 |
| INT-059 | Seeded entity types used by EntityConfigurationManager | Entity config | Entity types available after seeding | P0 |
| INT-060 | CleanUpUsers doesn't affect seeded system users | System users | System/service accounts preserved | P0 |
| INT-061 | GenerateEmbeddings integrates with AI service | AI endpoint | Embedding API called correctly | P1 |
| INT-062 | Admin operations visible in application logs | Logging | Structured log entries for all operations | P1 |
| INT-063 | Admin operations don't interfere with concurrent app usage | Isolation | App continues serving while admin runs | P0 |
| INT-064 | Migration doesn't lock tables for extended periods | Lock behavior | Short-lived locks only | P1 |
| INT-065 | Seeding doesn't duplicate existing data | Idempotent | Upsert or skip patterns used | P0 |
| INT-066 | Admin endpoint health after migration | Post-migration | All app endpoints still functional | P0 |
| INT-067 | Admin endpoint health after seeding | Post-seeding | All app endpoints functional with seed data | P0 |
| INT-068 | TRUNCATE doesn't affect non-SeedScript tables | Isolation | Only SeedScripts table truncated | P0 |
| INT-069 | DELETE doesn't cascade to related entities | FK safety | Only SeedScript record removed | P0 |
| INT-070 | Admin operations work with connection string from appsettings | Config | Correct environment DB targeted | P0 |
| INT-071 | Admin operations work behind IAP proxy | Network | Authentication passes through proxy | P1 |
| INT-072 | Admin operations respect CORS policy | CORS | Admin UI origin allowed | P2 |
| INT-073 | Admin operations timeout gracefully | Long-running | No hung connections | P1 |
| INT-074 | Multiple sequential admin operations don't exhaust connections | Connection pool | Pool healthy after 10+ operations | P1 |
| INT-075 | Admin operations preserve audit trail | Audit | Operations traceable in logs | P1 |

### Error Recovery Integration (15)

| ID | Test Name | Scenario | Verification | Priority |
|----|-----------|----------|--------------|----------|
| INT-076 | Failed migration doesn't corrupt database | Migration error | Schema in previous valid state | P0 |
| INT-077 | Failed seeding doesn't leave partial data | Seed error | Transaction rolled back | P0 |
| INT-078 | Failed truncate leaves data intact | Truncate error | Records still present | P0 |
| INT-079 | Failed delete leaves record intact | Delete error | Record still present | P0 |
| INT-080 | Failed cleanup leaves users intact | SQL error | No users removed | P0 |
| INT-081 | Database reconnection after timeout | Connection drop | Next operation succeeds | P1 |
| INT-082 | Admin endpoint recovery after 500 error | Previous error | Next request succeeds | P1 |
| INT-083 | Seeding retry after partial failure | Re-run | Only failed seeds retry | P1 |
| INT-084 | Migration retry after partial failure | Re-run | Only pending migrations apply | P1 |
| INT-085 | CleanUpUsers idempotent (safe to re-run) | Double run | Second run cleans 0 or same set | P1 |
| INT-086 | Admin operations handle database maintenance mode | DB read-only | Appropriate error returned | P2 |
| INT-087 | Admin operations handle memory pressure | Low memory | Graceful degradation | P2 |
| INT-088 | Admin operations log errors with stack trace | Exception | Full exception details in dev log | P1 |
| INT-089 | Admin operations return ProblemDetails on error | Error format | Structured error response | P1 |
| INT-090 | GlobalExceptionHandler catches admin controller exceptions | Exception pipeline | 500 with formatted error | P0 |

---

## §6 Security Tests (50)

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

| ID | Test Name | Security Concern | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| SEC-001 | Unauthenticated access to any admin endpoint | No auth | 401 Unauthorized | P0 |
| SEC-002 | Regular user access to migrations | No admin permission | 403 Forbidden | P0 |
| SEC-003 | Regular user access to seeding | No admin permission | 403 Forbidden | P0 |
| SEC-004 | Regular user access to truncate | No admin permission | 403 Forbidden | P0 |
| SEC-005 | Regular user access to delete script | No admin permission | 403 Forbidden | P0 |
| SEC-006 | Regular user access to cleanup | No admin permission | 403 Forbidden | P0 |
| SEC-007 | Regular user access to embeddings | No admin permission | 403 Forbidden | P0 |
| SEC-008 | SQL injection in seeder name parameter | Name="'; DROP TABLE--" | No SQL injection, handled safely | P0 |
| SEC-009 | SQL injection in delete script name | Name="1; DELETE FROM--" | No SQL injection | P0 |
| SEC-010 | XSS in seeder name response | Name="<script>alert(1)</script>" | Not rendered as HTML | P0 |
| SEC-011 | Path traversal in seeder name | Name="../../etc/passwd" | No file access | P0 |
| SEC-012 | Command injection in seeder name | Name="; rm -rf /" | No OS command execution | P0 |
| SEC-013 | LDAP injection in seeder name | Name="*)(objectClass=*" | No LDAP query execution | P1 |
| SEC-014 | Auth debug doesn't expose sensitive tokens | Response | No refresh tokens, passwords, or secrets | P0 |
| SEC-015 | Auth debug doesn't expose connection strings | Response | No DB credentials | P0 |
| SEC-016 | Admin endpoints don't expose internal paths | Error messages | No file system paths leaked | P0 |
| SEC-017 | Admin endpoints don't expose stack traces in production | Production | ProblemDetails without stack trace | P0 |
| SEC-018 | CleanUpUsers SQL doesn't accept user input | No injection surface | Predefined SQL only, no parameterization needed | P0 |
| SEC-019 | TruncateSeedScripts SQL is hardcoded | No injection | Fixed SQL string | P0 |
| SEC-020 | Admin operations logged with user identity | Audit | Who did what, when | P0 |
| SEC-021 | Failed auth attempts logged | Security logging | 401/403 logged with IP | P1 |
| SEC-022 | Rate limiting on admin endpoints | DoS prevention | Some form of throttling | P2 |
| SEC-023 | Admin endpoints not exposed in Swagger (if configured) | API docs | Admin routes hidden or restricted | P2 |
| SEC-024 | CORS restricts admin endpoint origins | Cross-origin | Only admin UI origin allowed | P1 |
| SEC-025 | Admin endpoints use HTTPS only | Transport security | HTTP redirected or blocked | P0 |
| SEC-026 | Seeder name URL-decoded safely | URL encoding | %2F, %00, etc. handled | P1 |
| SEC-027 | Null byte injection in seeder name | Name="seed%00er" | No null byte processing | P1 |
| SEC-028 | Unicode normalization attack in names | Homoglyph names | Exact byte match only | P2 |
| SEC-029 | Admin operations don't bypass soft-delete | Data integrity | Deleted records stay deleted | P0 |
| SEC-030 | CleanUpUsers preserves admin accounts | User cleanup | Admin/system accounts not removed | P0 |
| SEC-031 | Migration doesn't expose DB credentials in logs | Sensitive data | Connection string masked | P0 |
| SEC-032 | Seeding doesn't log seed data values | Sensitive data | No passwords or secrets in logs | P0 |
| SEC-033 | Admin operations respect multi-tenancy (if applicable) | Tenant isolation | Operations scoped to tenant | P2 |
| SEC-034 | Concurrent admin operations from different users | Multi-user | Each user's actions attributed correctly | P1 |
| SEC-035 | Token replay attack on admin endpoint | Reused token | Expired token rejected | P1 |
| SEC-036 | Admin endpoints enforce minimum TLS version | TLS 1.2+ | Older TLS rejected | P2 |
| SEC-037 | Response headers don't expose server details | X-Powered-By, Server | Headers removed or generic | P1 |
| SEC-038 | Admin operations maintain referential integrity | DB constraints | FK constraints respected | P0 |
| SEC-039 | Truncate doesn't bypass FK constraints if they exist | FK safety | Error or cascade properly | P1 |
| SEC-040 | Delete uses parameterized query (EF Core) | ORM safety | No raw SQL for delete | P0 |
| SEC-041 | Admin operations protected by anti-forgery (if applicable) | CSRF | POST endpoints protected | P1 |
| SEC-042 | Seeder execution sandbox | Code execution | Seeders can't access arbitrary system resources | P2 |
| SEC-043 | Admin endpoint response size limited | DoS prevention | No unbounded response | P1 |
| SEC-044 | Admin operations don't expose other users' data | Privacy | Only system data, not user records | P0 |
| SEC-045 | Embedding generation doesn't expose internal content | AI privacy | Raw content not in response | P1 |
| SEC-046 | CleanUpUsers logs each removed user | Audit | Traceability for compliance | P1 |
| SEC-047 | Admin API follows OWASP API Security top 10 | Comprehensive | No broken auth, excessive data exposure, etc. | P1 |
| SEC-048 | PermissionAuthorize attribute on all admin endpoints | Authorization | Every endpoint has permission check | P0 |
| SEC-049 | Base controller provides CurrentUserId safely | User context | No spoofing of user ID | P0 |
| SEC-050 | Admin endpoints immune to HTTP request smuggling | Network security | Proper request parsing | P2 |

---

## §7 Concurrency Tests (25)

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Test Name | Concurrent Scenario | Expected Result | Priority |
|----|-----------|-------------------|-----------------|----------|
| CON-001 | Two concurrent migration runs | Both call MigrateAsync | One completes, other waits or no-ops | P0 |
| CON-002 | Concurrent seed + migration | Seed during migration | No deadlock, both complete | P0 |
| CON-003 | Concurrent seeding runs | Two seed calls | No duplicate data, no deadlock | P0 |
| CON-004 | Truncate during active seeding | Truncate while seeder writes | Consistent state (one wins) | P0 |
| CON-005 | Delete during seeding of same script | Delete while seed runs | Consistent state | P1 |
| CON-006 | Concurrent delete + truncate | Both at once | Table empty, no error | P1 |
| CON-007 | Admin operation during normal app usage | Seed while users browse | App unaffected | P0 |
| CON-008 | Multiple auth-debug requests simultaneously | 50 concurrent | All return correct user-specific data | P1 |
| CON-009 | Concurrent cleanup users | Two calls | No double-deletion | P1 |
| CON-010 | Concurrent embedding generation | Two calls | No duplicate embeddings | P1 |
| CON-011 | Migration with concurrent DDL | External DDL during migration | Lock contention handled | P2 |
| CON-012 | Seeding with concurrent DML | App writes during seeding | No FK violations | P1 |
| CON-013 | Truncate with concurrent read | SELECT during TRUNCATE | Reader gets pre or post state | P1 |
| CON-014 | Connection pool under admin load | 20 admin operations | Pool not exhausted | P1 |
| CON-015 | Database connection timeout under load | Heavy concurrent admin | Graceful timeout, retry works | P1 |
| CON-016 | Concurrent specific seeder runs (same name) | Two calls to same seeder | One executes, other skips or waits | P1 |
| CON-017 | Concurrent specific seeder runs (different names) | Two different seeders | Both execute independently | P0 |
| CON-018 | Admin endpoint under DDoS-like load | 100 rapid requests | Rate limiting or graceful handling | P2 |
| CON-019 | Long-running migration with admin API calls | Other admin calls during migration | Other endpoints still responsive | P1 |
| CON-020 | Concurrent auth-debug + seeding | Different operations | No interference | P1 |
| CON-021 | Database failover during admin operation | Primary DB fails | Error or automatic failover | P2 |
| CON-022 | Connection string change during operation | Config reload | Current operation completes, next uses new string | P2 |
| CON-023 | Concurrent endpoint listing | Multiple GET /endpoints | All return same consistent list | P1 |
| CON-024 | Seeding with app startup | Seed during app init | No race condition with DI setup | P1 |
| CON-025 | Admin operations during rolling deployment | Multiple instances | Migration runs on one instance only | P2 |

---

## §8 Unit Tests (21)

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

| ID | Test Name | Unit Under Test | Verification | Priority |
|----|-----------|----------------|--------------|----------|
| UNT-001 | ISystemAdminManager has 5 method signatures | Interface | RunMigrations, RunSeeding, RunSpecificSeeder, TruncateSeedScripts, DeleteSeedScript | P0 |
| UNT-002 | Base SystemAdminManager.RunSeeding is virtual no-op | Method | Returns completed task, no side effects | P0 |
| UNT-003 | Base SystemAdminManager.RunSpecificSeeder is virtual no-op | Method | Returns completed task | P0 |
| UNT-004 | Base SystemAdminManager.TruncateSeedScripts is virtual no-op | Method | Returns completed task | P0 |
| UNT-005 | Base SystemAdminManager.DeleteSeedScript is virtual no-op | Method | Returns completed task | P0 |
| UNT-006 | Base SystemAdminManager.RunMigrations calls MigrateAsync | Method | AppDbContext.Database.MigrateAsync invoked | P0 |
| UNT-007 | SeedScript entity has required properties | Entity | ScriptName, ScriptType, FileHash, LastExecutedDate, ExecutionOrder | P0 |
| UNT-008 | SeedScript inherits BaseBusinessEntity | Hierarchy | Has Id, IsDeleted, audit fields | P0 |
| UNT-009 | APIDictionary.SystemAdmin equals "/api/system-admin" | Constant | Exact string match | P0 |
| UNT-010 | SystemAdminController inherits BaseController | Hierarchy | Has logger, auth, user resolver | P0 |
| UNT-011 | SystemAdminController has 9 action methods | Method count | All endpoints present | P0 |
| UNT-012 | UNOPSSystemAdminManager overrides RunMigrations | Override | Uses UNOPSAppDbContext | P0 |
| UNT-013 | UNOPSSystemAdminManager overrides RunSeeding | Override | Calls GenericSeedRunner | P0 |
| UNT-014 | UNOPSSystemAdminManager overrides RunSpecificSeeder | Override | Calls GenericSeedRunner specific | P0 |
| UNT-015 | UNOPSSystemAdminManager overrides TruncateSeedScripts | Override | Executes TRUNCATE SQL | P0 |
| UNT-016 | UNOPSSystemAdminManager overrides DeleteSeedScript | Override | Finds and removes entity | P0 |
| UNT-017 | TRUNCATE SQL string is correct PostgreSQL syntax | SQL | TRUNCATE TABLE public."SeedScripts" RESTART IDENTITY | P0 |
| UNT-018 | Delete uses FindAsync or FirstOrDefault on ScriptName | ORM | Correct lookup method | P0 |
| UNT-019 | Controller actions have correct HTTP method attributes | Attributes | [HttpGet], [HttpPost] on correct methods | P0 |
| UNT-020 | Controller actions have correct permission attributes | Attributes | CanRunMigrations, CanRunSeedings | P0 |
| UNT-021 | SystemAdminModel class has expected properties | Model | System info, health, DB info | P0 |

---

## §9 Performance Tests (16)

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

| ID | Test Name | Performance Scenario | Target | Priority |
|----|-----------|---------------------|--------|----------|
| PRF-001 | Auth-debug response time | GET auth-debug | < 200ms | P0 |
| PRF-002 | Endpoints listing response time | GET endpoints | < 100ms | P0 |
| PRF-003 | Small migration execution time | 1 simple migration | < 5s | P0 |
| PRF-004 | Seeding execution time (all seeds) | Full seed run | < 60s for typical dataset | P0 |
| PRF-005 | Specific seeder execution time | Single seeder | < 10s | P1 |
| PRF-006 | TruncateSeedScripts execution time | TRUNCATE | < 1s | P1 |
| PRF-007 | DeleteSeedScript execution time | Single delete | < 500ms | P1 |
| PRF-008 | CleanUpUsers execution time | 100 orphaned users | < 5s | P1 |
| PRF-009 | GenerateEmbeddings per entity | 1 embedding | < 2s per entity | P1 |
| PRF-010 | GenerateEmbeddings batch (100 entities) | Bulk embedding | < 120s total | P2 |
| PRF-011 | Migration with large table ALTER | Column add on 100K rows | < 30s | P2 |
| PRF-012 | Database connection acquisition time | Admin request | < 100ms | P1 |
| PRF-013 | Memory usage during large seeding | 50+ seeders | < 500MB peak | P2 |
| PRF-014 | Admin endpoint cold start (first request) | After app restart | < 2s | P1 |
| PRF-015 | Concurrent admin requests latency | 10 simultaneous | < 3s 95th percentile | P2 |
| PRF-016 | Database pool recovery after admin burst | 20 rapid operations | Pool healthy within 5s | P2 |

---

## §10 Load Tests (10)

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

| ID | Test Name | Load Scenario | Target | Priority |
|----|-----------|---------------|--------|----------|
| LDT-001 | 10 concurrent auth-debug requests | Parallel GET | All return 200, < 500ms avg | P0 |
| LDT-002 | 10 concurrent endpoints requests | Parallel GET | All return 200, < 200ms avg | P0 |
| LDT-003 | 5 concurrent seeding requests | Parallel seed | No data corruption, consistent state | P0 |
| LDT-004 | Sequential admin operations (20 requests) | Rapid sequence | All succeed, pool healthy | P1 |
| LDT-005 | Admin operations during normal app load (100 users) | Background admin | App latency not degraded >20% | P1 |
| LDT-006 | Sustained admin monitoring (auth-debug every 5s for 10min) | Long-running | Stable response times, no memory leak | P1 |
| LDT-007 | Large seeding under concurrent reads | Seed while querying | No lock contention >5s | P1 |
| LDT-008 | Migration under app load | Migrate while app serves | App remains responsive | P1 |
| LDT-009 | 50 rapid auth-debug requests (burst) | Spike | No 500 errors, < 2s max | P2 |
| LDT-010 | Admin endpoint stability over 30 minutes | Endurance | No degradation, stable memory | P2 |
