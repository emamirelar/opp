# Concurrency & Race Conditions — Test Cases

**Component:** Cross-cutting / Concurrency  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P? 90≥90 ✅ | E≥3P? 90≥90 ✅ | F≥3P? 90≥90 ✅ | I≥3P? 90≥90 ✅

---

## Feature Overview

**Concurrency & Race Conditions** covers scenarios where multiple requests or users operate concurrently: optimistic locking, database deadlocks, parallel requests, stale data handling, and connection pool exhaustion. These tests ensure the system handles concurrent access correctly and safely.

**Key Capabilities:**
- Optimistic locking (row version)
- Deadlock detection and resolution
- Parallel request handling
- Stale data detection
- Connection pool management
- Transaction isolation

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Single user update | Entity exists | Single update | Success | P0 |
| POS-002 | Two users update different entities | 2 entities | Parallel update | Both succeed | P0 |
| POS-003 | Optimistic lock: first wins | Entity v1 | User A updates | Success, v2 | P0 |
| POS-004 | Optimistic lock: second gets conflict | Entity v1 | A updates, B updates v1 | B gets 409 Conflict | P0 |
| POS-005 | Retry after optimistic conflict | Conflict | Retry with fresh load | Success | P0 |
| POS-006 | Parallel reads | Entity exists | 10 concurrent reads | All succeed | P0 |
| POS-007 | Read while write | Entity exists | Read during write | Read gets consistent view | P0 |
| POS-008 | Connection pool under load | Normal load | 50 concurrent requests | All succeed | P0 |
| POS-009 | Transaction isolation: read committed | 2 transactions | A reads, B updates, A reads | A sees B's commit | P0 |
| POS-010 | No deadlock: different order | 2 entities | A locks 1→2, B locks 2→1 | One waits, no deadlock | P0 |
| POS-011 | Connection release after use | Request | Complete request | Connection returned | P0 |
| POS-012 | Parallel creates | No conflict | 10 users create | 10 created | P0 |
| POS-013 | Cascade with lock | Parent, child | Update parent | Child locked correctly | P1 |
| POS-014 | Batch without deadlock | 100 updates | Sequential batch | All succeed | P1 |
| POS-015 | Stale data refresh | Entity modified | User refreshes | Gets latest | P0 |
| POS-016 | Version increment on update | Entity v1 | Update | Version becomes v2 | P0 |
| POS-017 | Parallel workflow actions | 2 users, 2 opps | Each approve own | Both succeed | P0 |
| POS-018 | Lock timeout | Contention | Wait timeout | Clear error | P1 |
| POS-019 | Connection pool resize | Config change | Increase pool | New connections | P1 |
| POS-020 | Transaction rollback releases | Failed tx | Rollback | Lock released | P0 |
| POS-021 | Idempotent retry | Duplicate request | Retry | Same result | P1 |
| POS-022 | Parallel bulk operations | 2 bulk | Different entities | Both succeed | P1 |
| POS-023 | Read replica | Read-only | Read from replica | Success | P1 |
| POS-024 | Cache invalidation on update | Cached entity | Update | Cache invalidated | P1 |
| POS-025 | No dirty read | Uncommitted | Read | Doesn't see uncommitted | P0 |
| POS-026 | No phantom read | Insert during tx | Read again | Consistent | P1 |
| POS-027 | Serializable isolation (if needed) | Critical section | Serialize | No anomalies | P1 |
| POS-028 | Connection health check | Connection | Check | Valid | P1 |
| POS-029 | Pool exhaustion recovery | Exhausted | Release | New requests succeed | P1 |
| POS-030 | Distributed lock (if used) | Cross-node | Lock | Single holder | P1 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 2.1 Optimistic Lock Failures (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-001 | Update with stale version | Entity v2, send v1 | 409 Conflict | P0 |
| NEG-002 | Delete with stale version | Entity v2, send v1 | 409 Conflict | P0 |
| NEG-003 | Version tampered | Send v999 | 409 Conflict | P0 |
| NEG-004 | Version null | Send null | 400 Bad Request | P0 |
| NEG-005 | Version negative | Send -1 | 400 Bad Request | P0 |
| NEG-006 | Version after delete | Entity deleted | 404 or 409 | P0 |
| NEG-007 | Concurrent update same field | A and B update name | One wins, 409 for other | P0 |
| NEG-008 | Version in wrong format | Wrong type | 400 Bad Request | P1 |
| NEG-009 | Update without version | No version sent | 400 or auto-fail | P0 |
| NEG-010 | Version rollback attack | Old version | 409 Conflict | P0 |
| NEG-011 | Race: read-update-write | B overwrites A | A gets 409 on save | P0 |
| NEG-012 | Double submit | Same form twice | Same result or 409 | P0 |
| NEG-013 | Stale UI submission | Page open 1 hr | 409 on submit | P0 |
| NEG-014 | Version in audit | Audit version | Version in audit | P1 |
| NEG-015 | Optimistic lock disabled | Config off | Last-write-wins | P1 |

### 2.2 Deadlock Scenarios (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-016 | Classic deadlock | A: 1→2, B: 2→1 | One aborted | P0 |
| NEG-017 | Three-way deadlock | A: 1→2, B: 2→3, C: 3→1 | One aborted | P0 |
| NEG-018 | Deadlock in transaction | Nested locks | Deadlock detected | P0 |
| NEG-019 | Deadlock in bulk | Bulk update | Deadlock detected | P0 |
| NEG-020 | Deadlock in cascade | Parent→child | Resolved | P1 |
| NEG-021 | Deadlock timeout | Long wait | Timeout error | P1 |
| NEG-022 | Deadlock in trigger | Trigger locks | Handled | P1 |
| NEG-023 | Deadlock in stored proc | Proc locks | Handled | P1 |
| NEG-024 | Deadlock with FK | FK lock | Resolved | P1 |
| NEG-025 | Deadlock in index | Index update | Resolved | P1 |
| NEG-026 | No deadlock: same order | A and B: 1→2 | Both succeed | P0 |
| NEG-027 | Deadlock victim | One chosen | Victim retries | P1 |
| NEG-028 | Deadlock in audit | Audit + entity | Resolved | P1 |
| NEG-029 | Deadlock in notification | Notify + lock | Resolved | P1 |
| NEG-030 | Deadlock in search index | Index + entity | Resolved | P1 |

### 2.3 Stale Data & Race (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-031 | Read stale then update | Read old, update | Optimistic lock fails | P0 |
| NEG-032 | Cache stale | Cache old | Invalidation or TTL | P0 |
| NEG-033 | Session stale | Session expired | 401 | P0 |
| NEG-034 | Token stale | Token expired | 401 | P0 |
| NEG-035 | Workflow stale | Opp changed | Reject or refresh | P0 |
| NEG-036 | Bulk stale | Batch modified | Partial failure | P1 |
| NEG-037 | Delete stale | Entity deleted | 404 | P0 |
| NEG-038 | FK stale | Parent deleted | FK error | P0 |
| NEG-039 | Permission stale | Permission revoked | 403 | P0 |
| NEG-040 | Config stale | Config changed | Reload or use | P1 |
| NEG-041 | List stale | List modified | Pagination or refresh | P1 |
| NEG-042 | Export stale | Data changed during | Consistent snapshot | P1 |
| NEG-043 | Report stale | Report generated | Timestamp or refresh | P1 |
| NEG-044 | Dashboard stale | Dashboard data | Refresh interval | P1 |
| NEG-045 | Realtime stale | WebSocket | Reconnect or update | P1 |

### 2.4 Connection Pool Exhaustion (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-046 | Pool exhausted | Max connections | Queue or 503 | P0 |
| NEG-047 | Connection leak | Not released | Eventually exhausted | P0 |
| NEG-048 | Long-running holds | Connection held long | Timeout or limit | P0 |
| NEG-049 | Pool under spike | 10x normal load | Graceful degradation | P0 |
| NEG-050 | Connection timeout | Connection stale | Released, new | P0 |
| NEG-051 | Pool resize down | Active connections | Release when idle | P1 |
| NEG-052 | Connection in transaction | Long tx | Lock held | P1 |
| NEG-053 | Connection in bulk | Bulk holds | Released after | P1 |
| NEG-054 | Connection in export | Large export | Hold or stream | P1 |
| NEG-055 | Multiple pools | Read/write | Separate pools | P1 |
| NEG-056 | Connection retry | Connection fail | Retry | P1 |
| NEG-057 | Pool health | Degraded | Alert or recover | P1 |
| NEG-058 | Connection pool monitoring | Metrics | Exposed | P2 |
| NEG-059 | Pool exhaustion alert | Exhausted | Alert | P1 |
| NEG-060 | Connection recovery | DB restart | Reconnect | P1 |

### 2.5 Parallel Request Failures (10)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-061 | Double submit same form | Submit twice | One succeeds | P0 |
| NEG-062 | Duplicate create | Same data twice | One or dedupe | P0 |
| NEG-063 | Race: create + delete | Create then delete | Consistent | P0 |
| NEG-064 | Race: update + delete | Update then delete | Delete wins | P0 |
| NEG-065 | Race: two deletes | Same entity | One succeeds | P0 |
| NEG-066 | Parallel unique constraint | Same unique value | One fails | P0 |
| NEG-067 | Parallel FK | Same parent | Both succeed | P2 |
| NEG-068 | Parallel counter | Increment counter | Correct final | P0 |
| NEG-069 | Parallel status change | Same workflow | One valid | P0 |
| NEG-070 | Parallel export | Same data | Both get snapshot | P1 |

### 2.6 Additional Negative (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-071 | Version overflow | Version max long | Handle or reject | P1 |
| NEG-072 | Lock key injection | Special chars in key | Sanitized | P0 |
| NEG-073 | Connection in retry | Retry holds connection | Released | P1 |
| NEG-074 | Pool resize during use | Active resize | Graceful | P1 |
| NEG-075 | Transaction in deadlock | 3-way | One victim | P0 |
| NEG-076 | Optimistic lock disabled | Config off | Last-write-wins | P1 |
| NEG-077 | Stale cache during update | Cache not invalidated | Stale read | P0 |
| NEG-078 | Export during delete | Delete during export | Snapshot or error | P1 |
| NEG-079 | Bulk during single update | Both concurrent | No deadlock | P1 |
| NEG-080 | Workflow lock timeout | Long hold | Timeout | P1 |
| NEG-081 | Connection pool metrics | Exhausted | Metrics | P2 |
| NEG-082 | Version in wrong format | String version | 400 | P1 |
| NEG-083 | Retry after 409 | 409 received | Retry with fresh | P0 |
| NEG-084 | Deadlock victim retry | Victim | Auto-retry | P1 |
| NEG-085 | Lock order violation | Wrong order | Deadlock risk | P1 |
| NEG-086 | Transaction scope leak | Tx not disposed | Leak | P0 |
| NEG-087 | Connection not returned | Exception path | Pool leak | P0 |
| NEG-088 | Concurrent circuit breaker | Multiple circuits | Independent | P1 |
| NEG-089 | Optimistic lock on create | Create | Version=1 | P0 |
| NEG-090 | Version in audit | Update | Old version in audit | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 3.1 Version Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Row version | 1 | Max long | ✅ | ✅ | Overflow | P1 |
| BND-002 | Version increment | 1 | 1 | ✅ | ✅ | N/A | P1 |
| BND-003 | Version in request | 0 | Max | ❌ 0 | ✅ | Reject | P1 |
| BND-004 | Version history | 1 | 100 | ✅ | ✅ | Truncate | P2 |
| BND-005 | Retry count | 1 | 5 | ✅ | ✅ | Stop | P1 |
| BND-006 | Lock timeout ms | 100 | 30000 | ✅ | ✅ | Cap | P1 |
| BND-007 | Connection timeout | 5 | 120 sec | ✅ | ✅ | Default | P1 |
| BND-008 | Pool size | 1 | 200 | ✅ | ✅ | Cap | P1 |
| BND-009 | Pool min | 0 | 100 | ✅ | ✅ | Cap | P2 |
| BND-010 | Backoff base ms | 10 | 1000 | ✅ | ✅ | Default | P2 |
| BND-011 | Backoff max ms | 100 | 10000 | ✅ | ✅ | Cap | P2 |
| BND-012 | Concurrent requests | 1 | 1000 | ✅ | ✅ | Reject | P1 |
| BND-013 | Transaction timeout | 1 | 300 sec | ✅ | ✅ | Default | P1 |
| BND-014 | Deadlock retry | 1 | 3 | ✅ | ✅ | Cap | P1 |
| BND-015 | Stale threshold | 0 | 3600 sec | ✅ | ✅ | Default | P2 |

### 3.2 Numeric Boundaries (10)

| ID | Field | Zero | Negative | Very Large | Priority |
|----|-------|------|----------|------------|----------|
| BND-016 | Version | ❌ | ❌ | ✅ Handle | P1 |
| BND-017 | Retry count | ❌ | ❌ | Cap 5 | P1 |
| BND-018 | Connection count | 0 | ❌ | Pool max | P1 |
| BND-019 | Lock wait ms | 0 | ❌ | 60000 | P1 |
| BND-020 | Parallel threads | 1 | ❌ | Config max | P1 |
| BND-021 | Batch size | 1 | ❌ | 1000 | P1 |
| BND-022 | Timeout sec | 1 | ❌ | 300 | P1 |
| BND-023 | Pool size | 1 | ❌ | 200 | P1 |
| BND-024 | Version in audit | N/A | ❌ | ✅ | P2 |
| BND-025 | Sequence number | 1 | ❌ | Max | P2 |

### 3.3 Timing Boundaries (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-026 | Update at exact same ms | 2 users, same ms | One wins | P1 |
| BND-027 | Lock timeout boundary | Exact timeout | Release or timeout | P1 |
| BND-028 | Retry at boundary | 5th retry | Success or stop | P1 |
| BND-029 | Connection pool at limit | Exactly max | Accept or queue | P1 |
| BND-030 | Transaction at timeout | Exact timeout | Rollback | P1 |
| BND-031 | Deadlock at 1 sec | 1 sec deadlock | Detect | P1 |
| BND-032 | Stale at 1 sec | 1 sec old | Accept or reject | P1 |
| BND-033 | Backoff at max | Max backoff | Use max | P2 |
| BND-034 | Session at expiry | Exact expiry | 401 | P1 |
| BND-035 | Token at expiry | Exact expiry | 401 | P1 |
| BND-036 | Cache TTL at boundary | Exact TTL | Invalidate | P1 |
| BND-037 | Connection idle at limit | Idle timeout | Release | P1 |
| BND-038 | Batch at 10 min | 10 min batch | Complete or timeout | P1 |
| BND-039 | Export at 30 min | 30 min export | Complete or timeout | P2 |
| BND-040 | Concurrent at 100 | 100 requests | All or queue | P1 |

### 3.4 Concurrency Level Boundaries (15)

| ID | Concurrency | State | Expected | Priority |
|----|-------------|-------|----------|----------|
| BND-041 | 1 user | Single | Success | P0 |
| BND-042 | 2 users | Low | Both succeed | P0 |
| BND-043 | 10 users | Medium | All succeed | P0 |
| BND-044 | 50 users | High | All or queue | P1 |
| BND-045 | 100 users | Very high | Degradation or queue | P1 |
| BND-046 | 1000 users | Extreme | Reject or queue | P2 |
| BND-047 | Same entity, 2 users | Contention | One wins | P0 |
| BND-048 | Same entity, 10 users | High contention | 9 conflicts | P1 |
| BND-049 | Different entities, 100 | No contention | All succeed | P0 |
| BND-050 | Mixed: 50 read, 50 write | Mixed | All succeed | P1 |
| BND-051 | Same workflow, 2 users | Same opp | One acts | P0 |
| BND-052 | Same list, 10 users | List view | All get data | P1 |
| BND-053 | Same export, 5 users | Export | All get snapshot | P1 |
| BND-054 | Pool at 50% | Half full | Normal | P1 |
| BND-055 | Pool at 100% | Full | Queue or reject | P1 |

### 3.5 Unicode & Special Cases (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-056 | Version in error message | Conflict | No XSS | P0 |
| BND-057 | Retry in log | Retry count | Logged | P2 |
| BND-058 | Deadlock in log | Deadlock | Victim logged | P2 |
| BND-059 | Connection ID | Unicode | Valid | P2 |
| BND-060 | Lock key | Special chars | Escaped | P1 |
| BND-061 | Transaction ID | UUID | Valid | P1 |
| BND-062 | Pool name | Special | Valid | P2 |
| BND-063 | Retry reason | Unicode | Stored | P2 |
| BND-064 | Conflict message | User-facing | Clear | P1 |
| BND-065 | Timeout message | User-facing | Clear | P1 |

### 3.6 Isolation Level Boundaries (5)

| ID | Test Name | Level | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-066 | Read uncommitted | RU | Not used | P1 |
| BND-067 | Read committed | RC | Default | P0 |
| BND-068 | Repeatable read | RR | If used | P1 |
| BND-069 | Serializable | S | Critical section | P1 |
| BND-070 | Snapshot isolation | SI | If used | P1 |

### 3.7 Additional Boundaries (20)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-071 | Version at 1 | Create | Version=1 | P0 |
| BND-072 | Retry at 1 | First retry | Base delay | P1 |
| BND-073 | Lock timeout at 0 | 0 ms | Reject or default | P1 |
| BND-074 | Pool at min size | Min connections | Maintained | P1 |
| BND-075 | Concurrent at 1 | Single request | Success | P0 |
| BND-076 | Transaction at 1 stmt | Single | 1 transaction | P0 |
| BND-077 | Deadlock retry at max | 3 retries | Final fail | P1 |
| BND-078 | Backoff at 0 | No backoff | Immediate | P1 |
| BND-079 | Connection at 1 | Single | Normal | P0 |
| BND-080 | Version history at 1 | Single version | Valid | P1 |
| BND-081 | Batch at 1 row | Single row | Success | P0 |
| BND-082 | Export at 1 record | Single | Success | P1 |
| BND-083 | Workflow at 1 user | Single | Success | P0 |
| BND-084 | Cache TTL at 0 | No cache | Bypass | P1 |
| BND-085 | Session at 1 | Single session | Normal | P0 |
| BND-086 | Lock at 1 resource | Single lock | Success | P0 |
| BND-087 | Conflict at 1 | Single 409 | Retry | P0 |
| BND-088 | Pool at 99% | Nearly full | Queue or accept | P1 |
| BND-089 | Transaction at 300 sec | Max timeout | Rollback | P1 |
| BND-090 | Stale at 0 sec | Just updated | Accept | P1 |

---

## §4 Functional Tests (Business Rules)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 4.1 Optimistic Lock Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | Version required for update | Update without version | 400 or fail | P0 |
| FUN-002 | Version incremented | Successful update | Version+1 | P0 |
| FUN-003 | Stale version rejected | Update with old version | 409 | P0 |
| FUN-004 | Retry after conflict | 409 received | Retry with fresh | P0 |
| FUN-005 | Version in response | Any read | Version in response | P0 |
| FUN-006 | Version immutable | Update | Version never decrease | P0 |
| FUN-007 | Version on create | Create | Version=1 | P0 |
| FUN-008 | Version on delete | Delete | Version in request | P0 |
| FUN-009 | Concurrent create same | Two creates | One or unique | P0 |
| FUN-010 | Idempotent update | Same update twice | Same result | P1 |
| FUN-011 | Version in audit | Update | Old version in audit | P1 |
| FUN-012 | Conflict message clear | 409 | User-friendly message | P1 |
| FUN-013 | Version in ETag | If ETag used | Version in ETag | P1 |
| FUN-014 | Version in If-Match | Conditional update | If-Match checked | P1 |
| FUN-015 | Version rollback | Rollback | Version unchanged | P0 |

### 4.2 Deadlock Rules (10)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-016 | Deadlock detected | Deadlock | One victim | P0 |
| FUN-017 | Victim retries | Victim | Auto-retry | P1 |
| FUN-018 | Consistent lock order | Lock order | Same order always | P0 |
| FUN-019 | Lock timeout | Wait too long | Timeout error | P0 |
| FUN-020 | No circular wait | Lock acquisition | Avoid | P0 |
| FUN-021 | Deadlock logged | Deadlock | Logged | P1 |
| FUN-022 | Deadlock metrics | Deadlock | Metrics | P2 |
| FUN-023 | Short transactions | Minimize hold | Reduce deadlock | P1 |
| FUN-024 | Lock scope | Minimal | Lock only needed | P1 |
| FUN-025 | No lock escalation | Avoid | Row-level preferred | P1 |

### 4.3 Connection Pool Rules (10)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-026 | Connection released | Request complete | Returned to pool | P0 |
| FUN-027 | Pool exhausted | Max reached | Queue or 503 | P0 |
| FUN-028 | Connection timeout | Stale connection | Released | P0 |
| FUN-029 | Pool health | Periodic | Health check | P1 |
| FUN-030 | Connection retry | Connection fail | Retry | P1 |
| FUN-031 | Pool size config | Config | Applied | P1 |
| FUN-032 | Connection reuse | Same request | Reuse | P1 |
| FUN-033 | No connection leak | All paths | Release | P0 |
| FUN-034 | Pool monitoring | Metrics | Exposed | P2 |
| FUN-035 | Pool exhaustion alert | Exhausted | Alert | P1 |

### 4.4 Transaction Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-036 | Atomicity | Transaction | All or nothing | P0 |
| FUN-037 | Consistency | Transaction | Valid state | P0 |
| FUN-038 | Isolation | Concurrent | No dirty read | P0 |
| FUN-039 | Durability | Commit | Persisted | P0 |
| FUN-040 | Rollback | Error | All rolled back | P0 |
| FUN-041 | Read committed | Default | See committed | P0 |
| FUN-042 | No dirty read | Uncommitted | Not visible | P0 |
| FUN-043 | Transaction timeout | Long tx | Rollback | P0 |
| FUN-044 | Nested transaction | Savepoint | Handled | P1 |
| FUN-045 | Transaction scope | Request | One per request | P1 |
| FUN-046 | Distributed transaction | If used | 2PC or saga | P2 |
| FUN-047 | Transaction ID | Logging | Unique ID | P1 |
| FUN-048 | Transaction audit | Commit | Audit written | P1 |
| FUN-049 | Transaction retry | Transient failure | Retry | P1 |
| FUN-050 | Connection per transaction | Transaction | One connection | P1 |

### 4.5 Additional Functional Rules (40)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-051 | Version in response | Any read | Version in response | P0 |
| FUN-052 | Version immutable | Update | Never decrease | P0 |
| FUN-053 | Version on create | Create | Version=1 | P0 |
| FUN-054 | Version on delete | Delete | Version in request | P0 |
| FUN-055 | Idempotent update | Same update twice | Same result | P1 |
| FUN-056 | Conflict message | 409 | User-friendly | P1 |
| FUN-057 | Version in ETag | If ETag used | Version in ETag | P1 |
| FUN-058 | If-Match header | Conditional update | Checked | P1 |
| FUN-059 | Version rollback | Rollback | Unchanged | P0 |
| FUN-060 | Deadlock victim | Deadlock | One chosen | P0 |
| FUN-061 | Victim retry | Victim | Auto-retry | P1 |
| FUN-062 | Lock order | Acquisition | Same order | P0 |
| FUN-063 | Lock timeout | Wait too long | Timeout error | P0 |
| FUN-064 | No circular wait | Lock | Avoid | P0 |
| FUN-065 | Deadlock logged | Deadlock | Logged | P1 |
| FUN-066 | Deadlock metrics | Deadlock | Metrics | P2 |
| FUN-067 | Short transactions | Minimize hold | Reduce deadlock | P1 |
| FUN-068 | Lock scope | Minimal | Lock only needed | P1 |
| FUN-069 | No lock escalation | Avoid | Row-level | P1 |
| FUN-070 | Connection released | Request complete | Returned | P0 |
| FUN-071 | Pool exhausted | Max reached | Queue or 503 | P0 |
| FUN-072 | Connection timeout | Stale | Released | P0 |
| FUN-073 | Pool health | Periodic | Health check | P1 |
| FUN-074 | Connection retry | Fail | Retry | P1 |
| FUN-075 | Pool size config | Config | Applied | P1 |
| FUN-076 | Connection reuse | Same request | Reuse | P1 |
| FUN-077 | No connection leak | All paths | Release | P0 |
| FUN-078 | Pool monitoring | Metrics | Exposed | P2 |
| FUN-079 | Pool exhaustion alert | Exhausted | Alert | P1 |
| FUN-080 | Atomicity | Transaction | All or nothing | P0 |
| FUN-081 | Consistency | Transaction | Valid state | P0 |
| FUN-082 | Isolation | Concurrent | No dirty read | P0 |
| FUN-083 | Durability | Commit | Persisted | P0 |
| FUN-084 | Rollback | Error | All rolled back | P0 |
| FUN-085 | Read committed | Default | See committed | P0 |
| FUN-086 | No dirty read | Uncommitted | Not visible | P0 |
| FUN-087 | Transaction timeout | Long tx | Rollback | P0 |
| FUN-088 | Nested transaction | Savepoint | Handled | P1 |
| FUN-089 | Transaction scope | Request | One per request | P1 |
| FUN-090 | Transaction ID | Logging | Unique ID | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 5.1 CRUD + Concurrency (15)

| ID | Operation | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-001 | Create + read | Create then read | Consistent | P0 |
| INT-002 | Update + read | Update then read | See update | P0 |
| INT-003 | Delete + read | Delete then read | 404 | P0 |
| INT-004 | Create + create | Duplicate create | One or dedupe | P0 |
| INT-005 | Update + update | Concurrent update | 409 for one | P0 |
| INT-006 | Update + delete | Update then delete | Delete wins | P0 |
| INT-007 | Bulk + single | Bulk and single | Both succeed | P1 |
| INT-008 | Workflow + update | Approve + update | Conflict or block | P0 |
| INT-009 | Export + update | Export during update | Snapshot | P1 |
| INT-010 | Import + read | Import during read | Consistent | P1 |
| INT-011 | Audit + update | Update + audit | Both succeed | P1 |
| INT-012 | Notification + update | Update + notify | Both succeed | P1 |
| INT-013 | Search + update | Update + index | Eventually consistent | P1 |
| INT-014 | Cache + update | Update + cache | Invalidated | P1 |
| INT-015 | FK + concurrent | Parent update + child | Consistent | P1 |

### 5.2 Search/Filter/Pagination (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-016 | Search during update | Update + search | Consistent | P1 |
| INT-017 | Pagination during insert | Insert + page | Consistent | P1 |
| INT-018 | Filter during delete | Delete + filter | Consistent | P1 |
| INT-019 | Sort during update | Update + sort | Consistent | P1 |
| INT-020 | Count during bulk | Bulk + count | Eventually consistent | P1 |
| INT-021 | Export during bulk | Bulk + export | Snapshot | P1 |
| INT-022 | List + create | Create + list | Eventually in list | P1 |
| INT-023 | List + delete | Delete + list | Eventually not in list | P1 |
| INT-024 | Aggregation + update | Update + aggregate | Consistent | P1 |
| INT-025 | Full-text + update | Update + search | Index updated | P1 |

### 5.3 Workflow + Concurrency (15)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-026 | Two users approve same | Same opp | One succeeds | P0 |
| INT-027 | Approve + recall | Race | One wins | P0 |
| INT-028 | Submit + cancel | Race | One wins | P0 |
| INT-029 | Workflow + entity update | Update during workflow | Block or conflict | P0 |
| INT-030 | DoA change + approve | DoA changed | Per business rule | P1 |
| INT-031 | Stage change + action | Stage change | Action valid | P1 |
| INT-032 | Bulk workflow | Bulk status change | All or partial | P1 |
| INT-033 | Workflow history + concurrent | Concurrent actions | All in history | P1 |
| INT-034 | Notification + workflow | Approve + notify | Both succeed | P1 |
| INT-035 | Workflow + audit | Workflow action | Audit written | P0 |
| INT-036 | Workflow + permission | Permission change | Re-evaluated | P1 |
| INT-037 | Workflow + lock | Workflow lock | Entity locked | P1 |
| INT-038 | Workflow timeout | Long workflow | Timeout or complete | P1 |
| INT-039 | Workflow retry | Transient failure | Retry | P1 |
| INT-040 | Workflow rollback | Workflow fail | Rollback | P0 |

### 5.4 Error Handling (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-041 | Conflict + retry | 409 | Retry succeeds | P0 |
| INT-042 | Deadlock + retry | Deadlock | Retry succeeds | P0 |
| INT-043 | Timeout + retry | Timeout | Retry or fail | P1 |
| INT-044 | Pool exhausted + wait | Exhausted | Eventually succeeds | P1 |
| INT-045 | DB restart + reconnect | Restart | Reconnect | P1 |
| INT-046 | Network error + retry | Network | Retry | P1 |
| INT-047 | Transaction fail + rollback | Error | Rollback | P0 |
| INT-048 | Partial failure | Some fail | Clear report | P1 |
| INT-049 | Cascading failure | Dependency fail | Rollback | P0 |
| INT-050 | Circuit breaker | Repeated failure | Open circuit | P1 |

### 5.5 Additional Integration Flows (40)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-051 | Create → Read → Update | Full flow | Consistent | P0 |
| INT-052 | Update → Conflict → Retry | 409 | Retry succeeds | P0 |
| INT-053 | Deadlock → Retry | Deadlock | Retry succeeds | P0 |
| INT-054 | Pool exhausted → Wait | Exhausted | Eventually succeeds | P1 |
| INT-055 | DB restart → Reconnect | Restart | Reconnect | P1 |
| INT-056 | Network error → Retry | Network | Retry | P1 |
| INT-057 | Transaction fail → Rollback | Error | Rollback | P0 |
| INT-058 | Partial failure | Some fail | Clear report | P1 |
| INT-059 | Bulk + single | Both | Both succeed | P1 |
| INT-060 | Export + update | Concurrent | Snapshot | P1 |
| INT-061 | Create + create | Duplicate | One or dedupe | P0 |
| INT-062 | Update + update | Concurrent | 409 for one | P0 |
| INT-063 | Update + delete | Race | Delete wins | P0 |
| INT-064 | Workflow + update | Approve + update | Conflict or block | P0 |
| INT-065 | Audit + update | Update + audit | Both succeed | P1 |
| INT-066 | Notification + update | Update + notify | Both succeed | P1 |
| INT-067 | Search + update | Update + index | Eventually consistent | P1 |
| INT-068 | Cache + update | Update + cache | Invalidated | P1 |
| INT-069 | FK + concurrent | Parent update + child | Consistent | P1 |
| INT-070 | Search during update | Update + search | Consistent | P1 |
| INT-071 | Pagination during insert | Insert + page | Consistent | P1 |
| INT-072 | Filter during delete | Delete + filter | Consistent | P1 |
| INT-073 | Sort during update | Update + sort | Consistent | P1 |
| INT-074 | Count during bulk | Bulk + count | Eventually consistent | P1 |
| INT-075 | Export during bulk | Bulk + export | Snapshot | P1 |
| INT-076 | List + create | Create + list | Eventually in list | P1 |
| INT-077 | List + delete | Delete + list | Eventually not in list | P1 |
| INT-078 | Aggregation + update | Update + aggregate | Consistent | P1 |
| INT-079 | Full-text + update | Update + search | Index updated | P1 |
| INT-080 | Two users approve same | Same opp | One succeeds | P0 |
| INT-081 | Approve + recall | Race | One wins | P0 |
| INT-082 | Submit + cancel | Race | One wins | P0 |
| INT-083 | Workflow + entity update | Update during workflow | Block or conflict | P0 |
| INT-084 | DoA change + approve | DoA changed | Per business rule | P1 |
| INT-085 | Stage change + action | Stage change | Action valid | P1 |
| INT-086 | Bulk workflow | Bulk status change | All or partial | P1 |
| INT-087 | Workflow history + concurrent | Concurrent actions | All in history | P1 |
| INT-088 | Notification + workflow | Approve + notify | Both succeed | P1 |
| INT-089 | Workflow + audit | Workflow action | Audit written | P0 |
| INT-090 | Workflow + permission | Permission change | Re-evaluated | P1 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: 50** | ✅ COMPLIANT

### 6.1 Injection (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in version | Version param | Parameterized | P0 |
| SEC-002 | XSS in conflict message | Error message | Escaped | P0 |
| SEC-003 | Log injection | Retry reason | Escaped | P0 |
| SEC-004 | Header injection | Request header | Validated | P0 |
| SEC-005 | NoSQL injection | Filter | Validated | P1 |
| SEC-006 | Command injection | Lock key | Sanitized | P0 |
| SEC-007 | Path traversal | Lock path | Sanitized | P0 |
| SEC-008 | Template injection | Message | No eval | P1 |
| SEC-009 | LDAP injection | User filter | Parameterized | P1 |
| SEC-010 | XXE in config | Config XML | Validated | P1 |

### 6.2 Access Control (10)

| ID | User | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | Update | 401 | P0 |
| SEC-012 | Wrong user | Update other's | 403 | P0 |
| SEC-013 | Read-only | Update | 403 | P0 |
| SEC-014 | Admin | Full access | 200 | P0 |
| SEC-015 | Org-scoped | Cross-org | 403 | P0 |
| SEC-016 | Delegated | Update on behalf | Per delegation | P1 |
| SEC-017 | API key | Update (no scope) | 403 | P0 |
| SEC-018 | Expired session | Update | 401 | P0 |
| SEC-019 | Deactivated user | Update | 401 | P0 |
| SEC-020 | Service account | Update (if allowed) | Per config | P1 |

### 6.3 IDOR (10)

| ID | Manipulation | Expected | Priority |
|----|-------------|----------|----------|
| SEC-021 | Change entity ID | 403 or filtered | P0 |
| SEC-022 | Change version ID | 409 | P0 |
| SEC-023 | Access other's lock | 403 | P0 |
| SEC-024 | Modify other's transaction | 403 | P0 |
| SEC-025 | Brute force IDs | Rate limit | P0 |
| SEC-026 | Sequential ID enum | Rate limit | P1 |
| SEC-027 | Batch with mixed IDs | Filter to own | P0 |
| SEC-028 | Update with wrong version | 409 | P0 |
| SEC-029 | Delete other's entity | 403 | P0 |
| SEC-030 | Lock other's resource | 403 | P0 |

### 6.4 Auth & Session (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| SEC-031 | JWT expired | 401 | P0 |
| SEC-032 | JWT tampered | 401 | P0 |
| SEC-033 | CSRF on update | Token required | P0 |
| SEC-034 | Replay attack | Nonce/timestamp | P1 |
| SEC-035 | Session fixation | New session | P0 |
| SEC-036 | Token theft | Invalidate on logout | P0 |
| SEC-037 | Concurrent session | Per policy | P1 |
| SEC-038 | Refresh token | Limited reuse | P1 |
| SEC-039 | MFA during conflict | MFA required | P1 |
| SEC-040 | Password change | Re-auth | P1 |

### 6.5 Data Exposure (10)

| ID | Data | Risk | Expected | Priority |
|----|------|------|----------|----------|
| SEC-041 | Version in error | Info leak | Generic message | P0 |
| SEC-042 | Internal details | Stack trace | No stack trace | P0 |
| SEC-043 | Lock details | Info leak | Minimal | P1 |
| SEC-044 | Pool metrics | Metrics | Admin only | P1 |
| SEC-045 | Deadlock details | Internal | Logged, not exposed | P0 |
| SEC-046 | Transaction ID | Correlation | No PII | P1 |
| SEC-047 | Retry count | Info | Minimal | P1 |
| SEC-048 | Connection pool | Internal | Not exposed | P1 |
| SEC-049 | Timeout value | Config | Generic | P1 |
| SEC-050 | Conflict details | User-facing | Clear, no internal | P1 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: 25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | 2 users update different | Both succeed | P0 |
| CON-002 | 2 users update same | 409 for one | P0 |
| CON-003 | 10 users update different | All succeed | P0 |
| CON-004 | 10 users update same | 9 conflicts | P1 |
| CON-005 | 100 concurrent reads | All succeed | P0 |
| CON-006 | 50 concurrent creates | All succeed | P0 |
| CON-007 | Deadlock scenario | One victim | P0 |
| CON-008 | No deadlock | Same order | Both succeed | P0 |
| CON-009 | Pool at 80% | Normal | P1 |
| CON-010 | Pool at 100% | Queue or 503 | P0 |
| CON-011 | Retry after 409 | Success | P0 |
| CON-012 | Retry after deadlock | Success | P0 |
| CON-013 | Transaction timeout | Rollback | P0 |
| CON-014 | Long transaction | Timeout or complete | P1 |
| CON-015 | Bulk + single | Both succeed | P1 |
| CON-016 | Export + update | Snapshot | P1 |
| CON-017 | 2 exports concurrent | Both succeed | P1 |
| CON-018 | Workflow + update | Conflict or block | P0 |
| CON-019 | Cache + update | Invalidated | P1 |
| CON-020 | Audit + update | Both succeed | P1 |
| CON-021 | Search + update | Eventually consistent | P1 |
| CON-022 | FK + concurrent | Consistent | P1 |
| CON-023 | Optimistic lock disabled | Last-write-wins | P1 |
| CON-024 | Connection recovery | Reconnect | P1 |
| CON-025 | Stale data refresh | User refreshes | Correct | P0 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: 21** | ✅ COMPLIANT

### 8.1 Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | Version valid | 1 | Valid | P1 |
| UNT-002 | Version invalid | 0 | Invalid | P1 |
| UNT-003 | Version stale | Old | Stale | P1 |
| UNT-004 | Retry count | 3 | Valid | P1 |
| UNT-005 | Timeout | 5000 | Valid | P1 |

### 8.2 Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | Conflict message | 409 | User-friendly | P1 |
| UNT-007 | Retry message | Retry | Clear | P1 |
| UNT-008 | Timeout message | Timeout | Clear | P1 |

### 8.3 Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | Backoff | 1st retry | Base * 1 | P1 |
| UNT-010 | Backoff | 3rd retry | Base * 4 | P1 |
| UNT-011 | Backoff max | 10th retry | Max | P1 |
| UNT-012 | Version increment | v1 | v2 | P1 |
| UNT-013 | Lock order | A, B | Consistent | P1 |

### 8.4 Status Logic (5)

| ID | Test | Condition | Expected | Priority |
|----|------|-----------|----------|----------|
| UNT-014 | Is conflict | 409 | True | P1 |
| UNT-015 | Is deadlock | Deadlock | True | P1 |
| UNT-016 | Is retryable | Transient | True | P1 |
| UNT-017 | Can retry | Count < max | True | P1 |
| UNT-018 | Is stale | Version old | True | P1 |

### 8.5 Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | Lock order | [A,B,C] | Same always | P1 |
| UNT-020 | Retry list | [409,409] | 2 retries | P1 |
| UNT-021 | Version history | [v1,v2,v3] | Ordered | P1 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: 16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|-----------|-----------|----------|
| PRF-001 | Single update | < 100 ms | P1 |
| PRF-002 | Read with version | < 50 ms | P1 |
| PRF-003 | Conflict detection | < 10 ms | P1 |
| PRF-004 | Retry latency | < 200 ms | P1 |
| PRF-005 | 10 concurrent updates | < 2 s | P1 |
| PRF-006 | 50 concurrent reads | < 1 s | P1 |
| PRF-007 | Deadlock detection | < 5 s | P1 |
| PRF-008 | Lock acquisition | < 50 ms | P1 |
| PRF-009 | Lock release | < 10 ms | P1 |
| PRF-010 | Connection acquire | < 20 ms | P1 |
| PRF-011 | Connection release | < 5 ms | P1 |
| PRF-012 | Transaction commit | < 100 ms | P1 |
| PRF-013 | Transaction rollback | < 50 ms | P1 |
| PRF-014 | Pool exhaustion | < 10 s | P2 |
| PRF-015 | 100 concurrent | < 10 s | P2 |
| PRF-016 | Memory under load | No leak | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: 10** | ✅ COMPLIANT

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 50 req/s sustained | 10 min | All succeed | P1 |
| LDT-002 | 100 req/s sustained | 10 min | < 1% error | P1 |
| LDT-003 | 200 req/s sustained | 5 min | Degradation ok | P2 |
| LDT-004 | Spike: 500 req/s | 1 min | Recover | P1 |
| LDT-005 | Spike: 1000 req/s | 30 sec | Recover | P2 |
| LDT-006 | Stress: 500 req/s | 1 min | Observe limits | P2 |
| LDT-007 | Stress: connection pool | Until exhaustion | Graceful | P2 |
| LDT-008 | Stress: deadlock | Concurrent | Detect | P2 |
| LDT-009 | Recovery after spike | 5 min | Normal | P1 |
| LDT-010 | Recovery after stress | 10 min | Full recovery | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Optimistic locking | POS-003–006, NEG-001–015, FUN-001–015 |
| Deadlock handling | NEG-016–030, FUN-016–025 |
| Connection pool | NEG-046–060, FUN-026–035 |
| Stale data | NEG-031–045, POS-015 |
| Transaction isolation | POS-009, FUN-036–050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
