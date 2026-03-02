# DbContextFactory Concurrency — Comprehensive Test Cases

**Component:** `IDbContextFactory<UNOPSAppDbContext>` + `IDbContextFactory<AppDbContext>` — Parallel DB Access Pattern  
**Primary Usage:** `UNOPSOpportunityManager.GetOpportunityDetailsForAIAsync` (10 parallel queries)  
**Also Used By:** Workflow Adapters, PubSubPullService, GeminiManager, PartnerManager, InteractionManager  
**Registration:** `Startup.cs` — `AddDbContextFactory<UNOPSAppDbContext>`, `AddDbContextFactory<AppDbContext>`, `AddDbContextFactory<PAOIdentityDbContext>`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30=90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30=90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30=90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30=90 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Ratio Compliance Checks

| Check | Formula | Actual | Required | Status |
|-------|---------|--------|----------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Feature Overview

The system uses `IDbContextFactory` to create **separate, disposable DbContext instances** for parallel database queries. This avoids the core limitation that a single `DbContext` is **not thread-safe**.

### Pattern

```csharp
var task1 = Task.Run(async () => {
    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
    return await ctx.Set<Entity>()
        .AsNoTracking()
        .Where(e => e.ParentId == id && !e.IsDeleted)
        .ToListAsync();
});

var task2 = Task.Run(async () => {
    await using var ctx = await _dbContextFactory.CreateDbContextAsync();
    return await ctx.Set<OtherEntity>()
        .AsNoTracking()
        .Where(e => e.ParentId == id && !e.IsDeleted)
        .ToListAsync();
});

await Task.WhenAll(task1, task2);
```

### Key Invariants

1. **Each parallel task creates its own DbContext** (via factory)
2. **Each DbContext is disposed** after use (`await using`)
3. **All parallel queries use `.AsNoTracking()`** (read-only)
4. **All queries filter `!IsDeleted`** (soft-delete)
5. **Main entity query runs first** (not parallelized)
6. **Task.WhenAll** waits for all parallel queries
7. **Connection pool** must support concurrent connections (MinPoolSize=10, MaxPoolSize=100)

### Registration

```csharp
// Startup.cs
services.AddDbContextFactory<UNOPSAppDbContext>(options => ...);
services.AddDbContextFactory<AppDbContext>(options => ...);
services.AddDbContextFactory<PAOIdentityDbContext>(options => ...);
```

### Consumers

| Consumer | DbContext Type | Purpose |
|----------|---------------|---------|
| `UNOPSOpportunityManager` | `UNOPSAppDbContext` | 10 parallel collection queries for AI |
| `UNOPSGeminiManager` | `UNOPSAppDbContext` | AI data retrieval |
| `UNOPSPartnerManager` | `UNOPSAppDbContext` | Partner data (optional) |
| `UNOPSInteractionManager` | `UNOPSAppDbContext` | Interaction data (optional) |
| `PaoWorkflowApproverProvider` | `AppDbContext` | Workflow approver lookup |
| `PaoWorkflowUserContext` | `AppDbContext` | Workflow user context |
| `PaoWorkflowNotificationService` | `AppDbContext` | Workflow notifications |
| `PaoEntityStageProvider` | `AppDbContext` | Entity stage lookup |
| `PubSubPullService` | `UNOPSAppDbContext` | Pub/Sub message processing |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| **Factory creates isolated contexts** | POS-001–005, FUN-001–010, UNT-001–005 |
| **Contexts disposed after use** | POS-006–010, NEG-001–010, FUN-011–015 |
| **AsNoTracking on all factory queries** | POS-011–015, FUN-016–020, UNT-006–010 |
| **Soft-delete filter on all queries** | POS-016–020, NEG-021–030, UNT-011–015 |
| **Connection pool management** | BND-001–020, CON-001–015, PRF-001–010 |
| **Thread safety** | CON-001–025, NEG-031–050 |
| **Error handling** | NEG-051–070, INT-031–050 |
| **Domain-specific scenarios** | NEG-071–090, BND-071–090, FUN-051–090, INT-051–090 |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Factory Context Creation (POS-001–010)

POS-001: `CreateDbContextAsync()` returns a new `UNOPSAppDbContext` instance.  
POS-002: Each `CreateDbContextAsync()` call returns a DIFFERENT instance.  
POS-003: Factory-created context connects to same database as DI-scoped context.  
POS-004: Factory-created context has same model/schema as DI-scoped context.  
POS-005: Factory-created `AppDbContext` is independent from `UNOPSAppDbContext`.  
POS-006: Context created via factory is disposable (`await using`).  
POS-007: Context disposed after `Task.Run` completes → No connection leak.  
POS-008: 10 contexts created in parallel → All succeed.  
POS-009: Factory resolves correctly from DI container.  
POS-010: Factory registered as expected service lifetime (Singleton).

### Parallel Query Execution (POS-011–020)

POS-011: `GetOpportunityDetailsForAIAsync` → 10 parallel tasks complete.  
POS-012: All 10 parallel queries return correct data for given OpportunityId.  
POS-013: Parallel queries use `.AsNoTracking()` → No change tracking overhead.  
POS-014: Parallel queries filter `!IsDeleted` → Soft-deleted records excluded.  
POS-015: `Task.WhenAll` waits for all 10 tasks → All results available.  
POS-016: FundingPartners loaded correctly in parallel.  
POS-017: ClientPartners loaded correctly in parallel.  
POS-018: Stakeholders loaded correctly in parallel.  
POS-019: Collaborators loaded correctly in parallel.  
POS-020: Missions loaded correctly in parallel.

### Data Correctness (POS-021–030)

POS-021: Parallel query results match sequential query results.  
POS-022: Include() navigation properties loaded correctly in parallel contexts.  
POS-023: ThenInclude() chains work correctly in factory contexts.  
POS-024: Where() filters applied correctly in parallel contexts.  
POS-025: Parallel results assigned back to parent entity correctly.  
POS-026: AI summary built from parallel data is complete and accurate.  
POS-027: Conditional tasks (SDGTargets, UNCFIndicators) execute when IDs exist.  
POS-028: Conditional tasks skipped when no IDs exist (`Task.FromResult`).  
POS-029: Wave 2 dependent queries use Wave 1 results correctly.  
POS-030: All collections non-null after parallel load (empty list if no data).

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Context Lifecycle Violations (NEG-001–015)

NEG-001: Use factory context after disposal → `ObjectDisposedException`.  
NEG-002: Share factory context between two parallel tasks → `ConcurrencyDetector` failure.  
NEG-003: Use DI-scoped context in `Task.Run` → Not thread-safe, may corrupt.  
NEG-004: Create context but forget `await using` → Connection leak.  
NEG-005: Create context but never dispose → Connection pool exhaustion.  
NEG-006: Double-dispose factory context → No error (idempotent).  
NEG-007: Access context.ChangeTracker after disposal → Exception.  
NEG-008: Call SaveChanges on factory-created AsNoTracking context → No effect.  
NEG-009: Modify entity loaded via factory context → Changes not tracked.  
NEG-010: Call factory.CreateDbContext (sync) in async path → Works but suboptimal.  
NEG-011: Factory returns null → Should never happen; throw if it does.  
NEG-012: Factory throws during creation → Task.Run propagates exception.  
NEG-013: Factory context with wrong connection string → Connection error.  
NEG-014: Factory context created after application shutdown → ObjectDisposedException.  
NEG-015: Factory context used with Transaction scope → Not supported in parallel.

### Thread Safety Violations (NEG-016–030)

NEG-016: Shared DI context across `Task.WhenAll` → `InvalidOperationException`.  
NEG-017: Two tasks writing to same context → Concurrency detector error.  
NEG-018: Two tasks reading from same context → May succeed but unreliable.  
NEG-019: Task modifying context while another reads → Race condition.  
NEG-020: Factory context accessed from multiple threads → Safe only if not shared.  
NEG-021: Parallel tasks with tracked entities → Change tracker not thread-safe.  
NEG-022: Parallel tasks calling SaveChanges → Multiple saves on shared context fail.  
NEG-023: Task.Run without factory (using scoped context) → Detected failure.  
NEG-024: Nested Task.Run sharing outer context → ConcurrencyDetector.  
NEG-025: ConfigureAwait(false) with factory context → Works correctly.  
NEG-026: Deadlock: Task.Run → .Result on async factory call → Potential deadlock.  
NEG-027: Task cancellation during factory context use → Context disposed.  
NEG-028: ThreadPool starvation → Factory creation queued.  
NEG-029: Recursive factory context creation → N contexts, N connections.  
NEG-030: Factory context in sync-over-async pattern → May deadlock.

### Missing Data Guards (NEG-031–045)

NEG-031: Parallel query without `!IsDeleted` filter → Returns soft-deleted records.  
NEG-032: Parallel query without `.AsNoTracking()` → Change tracking overhead.  
NEG-033: Parallel query without `Where(e => e.ParentId == id)` → Wrong data.  
NEG-034: OpportunityId = 0 in parallel query → No data, but no error.  
NEG-035: OpportunityId = negative in parallel query → No data, but no error.  
NEG-036: OpportunityId for soft-deleted opportunity → Parallel queries return empty.  
NEG-037: Non-existent OpportunityId → All parallel queries return empty lists.  
NEG-038: Null result from factory task → Handled (not NullReferenceException).  
NEG-039: Exception in one parallel task → Task.WhenAll aggregates exceptions.  
NEG-040: Timeout in one parallel task → Other tasks may succeed.  
NEG-041: One task returns 0 results, others return data → Handled correctly.  
NEG-042: Include on non-existent navigation property → Compile-time error.  
NEG-043: ThenInclude on null navigation → No error, empty collection.  
NEG-044: Factory context with incorrect schema → Query errors.  
NEG-045: Factory context with pending migration → Schema mismatch.

### Connection Pool Failures (NEG-046–060)

NEG-046: Connection pool exhausted → Factory CreateDbContextAsync waits or throws.  
NEG-047: MaxPoolSize=1 with 10 parallel tasks → 9 tasks wait.  
NEG-048: All pool connections in use by other requests → Factory queued.  
NEG-049: Connection timeout during factory creation → TimeoutException.  
NEG-050: Database server down → All factory tasks fail.  
NEG-051: Network partition during parallel queries → Tasks fail with timeout.  
NEG-052: SSL certificate expired → Connection error for factory contexts.  
NEG-053: Incorrect password in connection string → Authentication error.  
NEG-054: Connection string missing → Configuration error at startup.  
NEG-055: MaxPoolSize < number of parallel tasks → Tasks wait for pool.  
NEG-056: Pool connection idle timeout → Stale connection recycled.  
NEG-057: Pool connection broken → Factory creates new connection.  
NEG-058: MinPoolSize = 0 → Cold start for every request.  
NEG-059: Multiplexing disabled → Each task holds exclusive connection.  
NEG-060: Database connection limit reached → Factory returns error.

### Error Propagation (NEG-061–070)

NEG-061: Exception in task1 → Task.WhenAll throws AggregateException.  
NEG-062: Exceptions in multiple tasks → All exceptions in AggregateException.  
NEG-063: Task cancellation via CancellationToken → OperationCanceledException.  
NEG-064: Unhandled exception in Task.Run → Observed via await.  
NEG-065: Exception after partial data load → No partial data returned.  
NEG-066: DbUpdateException in factory context → Propagated to caller.  
NEG-067: SqlException in factory context → Propagated.  
NEG-068: NpgsqlException (PostgreSQL-specific) → Propagated.  
NEG-069: Stack trace preserved through Task.Run → Debuggable.  
NEG-070: Error logging includes which parallel task failed → Identifiable.

### Domain-Specific Negative Scenarios (NEG-071–090)

NEG-071: Factory context created with wrong DbContext type (AppDbContext vs UNOPSAppDbContext) → Type mismatch.  
NEG-072: Factory context reused across multiple parallel operations → Shared state corruption.  
NEG-073: Parallel tasks with different CancellationToken sources → Inconsistent cancellation.  
NEG-074: Factory not registered in DI → Resolution fails at runtime.  
NEG-075: Factory context with disposed underlying connection → InvalidOperationException.  
NEG-076: Parallel query with connection closed mid-query → NpgsqlException.  
NEG-077: Factory context creation during garbage collection → May block or fail.  
NEG-078: Factory context with disposed service provider → ObjectDisposedException.  
NEG-079: Factory context creation in fire-and-forget Task → Orphaned context, connection leak.  
NEG-080: Factory context with invalid DbContextOptions → Configuration error.  
NEG-081: OpportunityId from soft-deleted parent entity → Orphaned FK, empty or error.  
NEG-082: Parallel query on FundingPartner with deleted Partner → Include returns null.  
NEG-083: Parallel query on Stakeholder with deleted User → Include returns null.  
NEG-084: Factory context creation timeout during high load → TimeoutException.  
NEG-085: Factory context with wrong schema (public vs custom) → Query returns wrong data.  
NEG-086: Parallel tasks with shared DbContextOptions snapshot → Options disposed early.  
NEG-087: Factory context creation during database failover → Transient failure.  
NEG-088: Factory context with read-only user credentials → Write attempt fails.  
NEG-089: Parallel query on Geography with soft-deleted parent → Filter mismatch.  
NEG-090: Factory context creation after DbContextOptions disposed → ObjectDisposedException.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Parallel Task Count Boundaries (BND-001–015)

BND-001: 0 parallel tasks (only main query) → Works (no parallelism).  
BND-002: 1 parallel task → Works, minimal overhead.  
BND-003: 5 parallel tasks → Works, moderate concurrency.  
BND-004: 10 parallel tasks (current max) → Works, designed capacity.  
BND-005: 20 parallel tasks → Works if pool allows, higher memory.  
BND-006: 50 parallel tasks → Connection pool pressure.  
BND-007: 100 parallel tasks → Pool exhaustion likely.  
BND-008: Parallel tasks = MaxPoolSize → All connections used.  
BND-009: Parallel tasks = MaxPoolSize + 1 → One task waits.  
BND-010: Parallel tasks = MaxPoolSize × 2 → Many tasks wait.  
BND-011: Task.WhenAll with 0 tasks → Completes immediately.  
BND-012: Task.WhenAll with 1 completed + 9 pending → Waits for all.  
BND-013: Task.WhenAll with all tasks already completed → Returns immediately.  
BND-014: Task.WhenAll timeout not exceeded → All complete.  
BND-015: Task.WhenAll with one very slow task → Waits for slowest.

### Connection Pool Boundaries (BND-016–030)

BND-016: MinPoolSize = 0 → Pool starts empty.  
BND-017: MinPoolSize = 10 → 10 warm connections.  
BND-018: MinPoolSize = MaxPoolSize → Fixed pool size.  
BND-019: MaxPoolSize = 10 → Limit at 10 concurrent contexts.  
BND-020: MaxPoolSize = 100 (configured) → 100 concurrent contexts.  
BND-021: MaxPoolSize = 1000 → Very high concurrency capacity.  
BND-022: Connection idle for 300s → Recycled by pool.  
BND-023: Connection used for 1ms → Returned to pool.  
BND-024: Connection used for 30s → Still valid.  
BND-025: Connection used for 600s → May timeout.  
BND-026: Pool at 0 connections → Next request creates new.  
BND-027: Pool at MaxPoolSize → Next request waits.  
BND-028: Pool connection just returned → Immediately reusable.  
BND-029: Pool connection returned after error → Validated before reuse.  
BND-030: Multiplexing enabled → Multiple commands per connection.

### Data Volume Boundaries (BND-031–050)

BND-031: Opportunity with 0 items in all collections → All tasks return empty.  
BND-032: Opportunity with 1 item per collection → 10 tasks, 10 rows total.  
BND-033: Opportunity with 100 items per collection → 10 tasks, 1000 rows.  
BND-034: Opportunity with 1000 items per collection → 10 tasks, 10K rows.  
BND-035: Opportunity with 10K items in one collection → 1 heavy task, 9 light.  
BND-036: Each collection has different sizes (1, 10, 100, 1000, ...) → All load.  
BND-037: Collection with large navigation properties (nested includes) → Memory.  
BND-038: Collection with large text fields → Network bandwidth.  
BND-039: Collection with null FK references → Handled (include returns null).  
BND-040: Collection with circular references (prevented by AsNoTracking) → Safe.  
BND-041: Result set at 1 row → Minimal memory.  
BND-042: Result set at 10K rows → Moderate memory.  
BND-043: Result set at 100K rows → High memory, possible timeout.  
BND-044: Result set at 1M rows → Very high memory, likely timeout.  
BND-045: Empty database → All tasks return empty lists.  
BND-046: Single record database → Only relevant tasks return data.  
BND-047: All records soft-deleted → All tasks return empty (filtered).  
BND-048: Mix of active and deleted records → Only active returned.  
BND-049: Records added during parallel queries → Snapshot isolation.  
BND-050: Records deleted during parallel queries → Snapshot isolation.

### Timing Boundaries (BND-051–060)

BND-051: All 10 tasks complete in < 100ms → Minimal overhead.  
BND-052: One task takes 10s, others < 100ms → Total ≈ 10s (slowest).  
BND-053: All tasks take 5s each → Total ≈ 5s (parallel), not 50s.  
BND-054: Task creation overhead < 1ms per task.  
BND-055: Factory CreateDbContextAsync < 5ms per context.  
BND-056: Context disposal < 1ms per context.  
BND-057: Connection acquisition from warm pool < 10ms.  
BND-058: Connection return to pool < 1ms.  
BND-059: Task.WhenAll overhead < 1ms.  
BND-060: Total parallel execution < sequential execution time.

### Memory Boundaries (BND-061–070)

BND-061: 10 concurrent contexts → 10× DbContext memory.  
BND-062: Each context ≈ small footprint with AsNoTracking.  
BND-063: Context disposal frees memory → GC collects.  
BND-064: 100 sequential factory operations → Memory stable (no leak).  
BND-065: 1000 sequential factory operations → Memory stable.  
BND-066: Factory context with large result set → Memory proportional to data.  
BND-067: Factory context with empty result set → Minimal memory.  
BND-068: Parallel result aggregation → Memory = sum of all results.  
BND-069: Peak memory during Task.WhenAll → All results in memory simultaneously.  
BND-070: Memory after Task.WhenAll completes → Only aggregated results retained.

### Domain-Specific Boundary Scenarios (BND-071–090)

BND-071: OpportunityId = int.MaxValue → Query executes, no match or overflow edge.  
BND-072: OpportunityId = int.MinValue → Query executes, no match.  
BND-073: OpportunityId = 1 (first valid ID) → Boundary of valid range.  
BND-074: Opportunity with exactly 10 FundingPartners (typical max) → All load.  
BND-075: Opportunity with 0 SDGTargets, 0 UNCFIndicators → Conditional tasks skipped.  
BND-076: Opportunity with 1 SDGTarget, 1 UNCFIndicator → Wave 2 executes.  
BND-077: Partner.Name at max length (255 chars) in parallel Include → No truncation.  
BND-078: Stakeholder collection with 1000 items → Pagination or full load boundary.  
BND-079: Geography with null ParentId (root node) → Include returns null.  
BND-080: DateTime.MinValue in CreatedDate filter → Query boundary.  
BND-081: DateTime.MaxValue in LastModifiedDate filter → Query boundary.  
BND-082: Parallel tasks = MinPoolSize exactly → All use warm connections.  
BND-083: Factory creation at exact connection pool limit → Last slot.  
BND-084: Command timeout = 30s (default) → Long query boundary.  
BND-085: Command timeout = 1s → Short query may timeout.  
BND-086: Connection lifetime = 0 (no limit) → No forced recycle.  
BND-087: Thread pool at saturation → Factory creation queued.  
BND-088: GC pressure during Task.WhenAll → No allocation failure.  
BND-089: Nested Include depth = 5 (max typical) → All levels load.  
BND-090: Empty string in optional filter parameter → Handled as empty.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Factory Registration & Resolution (FUN-001–010)

FUN-001: `IDbContextFactory<UNOPSAppDbContext>` registered in DI.  
FUN-002: `IDbContextFactory<AppDbContext>` registered in DI.  
FUN-003: `IDbContextFactory<PAOIdentityDbContext>` registered in DI.  
FUN-004: Factory configured with Npgsql provider.  
FUN-005: Factory configured with `DbSchemaAwareModelCacheKeyFactory`.  
FUN-006: Factory resolved in `UNOPSManagerWrapper` constructor.  
FUN-007: Factory passed to `UNOPSOpportunityManager` via constructor.  
FUN-008: Factory optional in `UNOPSPartnerManager` (nullable).  
FUN-009: Factory optional in `UNOPSInteractionManager` (nullable).  
FUN-010: Factory resolved by workflow adapter constructors.

### Context Isolation (FUN-011–020)

FUN-011: Factory context is independent from request-scoped context.  
FUN-012: Changes in factory context NOT visible to request-scoped context.  
FUN-013: Changes in request-scoped context NOT visible to factory context.  
FUN-014: Factory context has no tracked entities initially.  
FUN-015: Factory context ChangeTracker is empty with AsNoTracking.  
FUN-016: Two factory contexts created in parallel are independent.  
FUN-017: Disposing one factory context doesn't affect another.  
FUN-018: Factory context disposal returns connection to pool.  
FUN-019: Factory context uses separate connection from request context.  
FUN-020: Factory context schema/model matches request context.

### Query Correctness (FUN-021–035)

FUN-021: Factory context query results match request context query results.  
FUN-022: Factory context `.Include()` loads navigation properties.  
FUN-023: Factory context `.ThenInclude()` loads nested properties.  
FUN-024: Factory context `.Where()` filter applies correctly.  
FUN-025: Factory context `.AsNoTracking()` prevents change tracking.  
FUN-026: Factory context `.ToListAsync()` materializes results.  
FUN-027: Factory context `.FirstOrDefaultAsync()` returns single entity.  
FUN-028: Factory context `.CountAsync()` returns correct count.  
FUN-029: Factory context `.AnyAsync()` returns correct boolean.  
FUN-030: Factory context `.Select()` projection works correctly.  
FUN-031: Factory context with LINQ `.GroupBy()` → Correct grouping.  
FUN-032: Factory context with `.OrderBy()` → Correct ordering.  
FUN-033: Factory context with `.Skip().Take()` → Correct pagination.  
FUN-034: Factory context generates correct SQL.  
FUN-035: Factory context respects global query filters (if any).

### Test Infrastructure (FUN-036–050)

FUN-036: `TestDbContextFactory` creates contexts for unit tests.  
FUN-037: `TestDbContextFactory.CreateDbContext()` returns InMemory context.  
FUN-038: `TestDbContextFactory.CreateDbContextAsync()` returns InMemory context.  
FUN-039: Mock `IDbContextFactory` in unit tests → Returns mock context.  
FUN-040: `PAOWebApplicationFactory` registers factory for integration tests.  
FUN-041: Integration test factory uses InMemory provider.  
FUN-042: Each test gets isolated factory context.  
FUN-043: Factory mock supports `Setup(f => f.CreateDbContextAsync(...))`.  
FUN-044: Factory mock verifies `CreateDbContextAsync` call count.  
FUN-045: Parallel test execution → Each test has own factory.  
FUN-046: Factory context in test → Shares InMemory database (same options).  
FUN-047: Factory context in test → Can seed data before parallel queries.  
FUN-048: Factory context in test → Can verify data after parallel queries.  
FUN-049: Factory context in test → Disposal tracked for leak detection.  
FUN-050: Factory mock supports cancellation token forwarding.

### Domain-Specific Functional Scenarios (FUN-051–090)

FUN-051: Factory context loads FundingPartner with Partner Include → Correct join.  
FUN-052: Factory context loads ClientPartner with Partner Include → Correct join.  
FUN-053: Factory context loads Stakeholder with User Include → Correct join.  
FUN-054: Factory context loads Collaborator with User Include → Correct join.  
FUN-055: Factory context loads Mission with Geography Include → Correct join.  
FUN-056: Factory context loads SDGTarget when Opportunity has SDG IDs → Conditional load.  
FUN-057: Factory context loads UNCFIndicator when Opportunity has UNCF IDs → Conditional load.  
FUN-058: Factory context with IsDeleted filter on FundingPartner → Soft-delete respected.  
FUN-059: Factory context with IsDeleted filter on Stakeholder → Soft-delete respected.  
FUN-060: Factory context loads Opportunity.Documents in parallel → Correct collection.  
FUN-061: Factory context loads Opportunity.Risks in parallel → Correct collection.  
FUN-062: Factory context loads Opportunity.Teams in parallel → Correct collection.  
FUN-063: Factory context with WorkflowStatus filter → Correct status filtering.  
FUN-064: Factory context with EntityStatus filter → Correct status filtering.  
FUN-065: Factory context for PaoWorkflowApproverProvider → Approver lookup correct.  
FUN-066: Factory context for PaoWorkflowUserContext → User resolution correct.  
FUN-067: Factory context for PaoEntityStageProvider → Stage lookup correct.  
FUN-068: Factory context for PubSubPullService → Message processing correct.  
FUN-069: Factory context for UNOPSGeminiManager → AI data retrieval correct.  
FUN-070: Factory context with DbSchemaAwareModelCacheKeyFactory → Schema isolation.  
FUN-071: Factory context with CommandTimeout option → Timeout applied.  
FUN-072: Factory context with EnableSensitiveDataLogging → Logging works.  
FUN-073: Factory context with EnableDetailedErrors → Error details available.  
FUN-074: Factory context for read-only operations → No SaveChanges needed.  
FUN-075: Factory context with Npgsql connection resiliency → Retry on transient failure.  
FUN-076: Factory context loads Partner with Contact Include → Nested navigation.  
FUN-077: Factory context loads FundingPartner with Agreement Include → Nested navigation.  
FUN-078: Factory context with composite key entity (OpportunityPartner) → Correct load.  
FUN-079: Factory context with inheritance (ModifiableDeletableEntity) → Correct load.  
FUN-080: Factory context with owned types → Correct load.  
FUN-081: Factory context with shadow properties → Correct load.  
FUN-082: Factory context with value converters (DateTime → UTC) → Correct conversion.  
FUN-083: Factory context with global query filter (IsDeleted) → Filter applied.  
FUN-084: Factory context for AI summary generation → All required data available.  
FUN-085: Factory context for oUP integration deep link → Correct data resolution.  
FUN-086: Factory context with Geography hierarchy → Parent/child correct.  
FUN-087: Factory context with Clearbit Partner data → Cleanup filter applied.  
FUN-088: Factory context with RulesEngine entity lookup → Correct resolution.  
FUN-089: Factory context with DST AI profiling data → Correct load.  
FUN-090: Factory context with TheGoDecision entities → Correct workflow data.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### End-to-End Parallel Query (INT-001–015)

INT-001: `GetOpportunityDetailsForAIAsync` → Returns complete opportunity with all collections.  
INT-002: Parallel queries return same data as sequential queries.  
INT-003: Parallel queries faster than sequential equivalent.  
INT-004: Main entity query runs BEFORE parallel collection queries.  
INT-005: Wave 2 queries run AFTER Wave 1 completes.  
INT-006: All 10 collection types loaded correctly in parallel.  
INT-007: Conditional collections (SDGTargets, UNCFIndicators) loaded when relevant.  
INT-008: Conditional collections skipped when no relevant IDs.  
INT-009: Results assigned to parent entity correctly.  
INT-010: AI summary generated from parallel-loaded data is accurate.  
INT-011: API endpoint using parallel queries returns correct response.  
INT-012: Parallel queries with large opportunity (100+ items per collection).  
INT-013: Parallel queries with minimal opportunity (1 item per collection).  
INT-014: Parallel queries with empty opportunity (0 items per collection).  
INT-015: Parallel queries on soft-deleted opportunity → Main query returns null.

### Workflow Adapter Integration (INT-016–025)

INT-016: Workflow approval → Adapter creates factory context, loads approver, disposes.  
INT-017: Workflow notification → Adapter creates factory context, sends notification, disposes.  
INT-018: Workflow stage change → Adapter creates factory context, updates stage, disposes.  
INT-019: Workflow user context → Adapter resolves user via factory context.  
INT-020: Concurrent workflow operations → Each adapter uses own factory context.  
INT-021: Workflow adapter factory context independent from main request.  
INT-022: Workflow adapter query does not lock main request's transaction.  
INT-023: Multiple workflow adapters running concurrently → No interference.  
INT-024: PubSubPullService → Creates factory context per message.  
INT-025: GeminiManager → Creates factory context for AI data retrieval.

### Error Recovery (INT-026–040)

INT-026: One parallel task fails → Others complete, error reported.  
INT-027: All parallel tasks fail → AggregateException with all failures.  
INT-028: Factory throws → Caller handles exception.  
INT-029: DB timeout in one task → Task fails, others succeed.  
INT-030: Connection error in one task → Task fails, others succeed.  
INT-031: Partial results from failed Task.WhenAll → Handled gracefully.  
INT-032: Retry after parallel failure → New factory contexts created.  
INT-033: Factory context exception → Connection returned to pool (not leaked).  
INT-034: Task cancellation → Context disposed, connection returned.  
INT-035: Application shutdown during parallel queries → Graceful cleanup.  
INT-036: Memory pressure during parallel queries → GC handles.  
INT-037: Thread pool exhaustion → Tasks queued, eventually complete.  
INT-038: Database failover during parallel queries → Reconnection.  
INT-039: Network blip during parallel queries → Retry or fail.  
INT-040: Transaction conflict between parallel task and main context → Handled.

### Database State Verification (INT-041–050)

INT-041: Parallel read does not modify any data → DB unchanged.  
INT-042: Parallel read with AsNoTracking → No change tracker entries.  
INT-043: Factory context does not participate in ambient transaction.  
INT-044: Read-only factory queries → No locks held after completion.  
INT-045: Sequential calls to parallel method → Each call creates fresh contexts.  
INT-046: Parallel queries see committed data only → Read Committed isolation.  
INT-047: Data inserted by main context → Visible to factory contexts (if committed).  
INT-048: Data inserted by one factory context → Not visible to another (not committed).  
INT-049: Connection pool statistics after 100 parallel operations → Stable.  
INT-050: Connection pool health check → All connections valid.

### Domain-Specific Integration Scenarios (INT-051–090)

INT-051: Full AI pipeline: Opportunity → AI summary → Factory context throughout.  
INT-052: Workflow approval flow: Submit → Approver lookup via factory → Approve.  
INT-053: PubSub message: Pull → Factory context → Process → Dispose.  
INT-054: Gemini API call: Request → Factory context for data → Response.  
INT-055: Partner manager with factory: Partner list → Detail via factory.  
INT-056: Interaction manager with factory: Interaction list → Detail via factory.  
INT-057: Concurrent Opportunity + Partner + Interaction managers → All use factory.  
INT-058: API rate limiting with parallel GetOpportunityDetailsForAIAsync → Throttled.  
INT-059: Distributed tracing: Parallel tasks each have span → Trace correct.  
INT-060: Metrics: Factory context creation count → Metric correct.  
INT-061: Health check: DbContextFactory health → Reports healthy.  
INT-062: Startup: Factory registration before first request → Ready.  
INT-063: Graceful shutdown: Pending factory contexts → Disposed.  
INT-064: Migration: Database migration with factory in use → Handled.  
INT-065: Database backup during parallel queries → No blocking.  
INT-066: Database restore during parallel queries → Fail or retry.  
INT-067: Failover: Primary down → Factory reconnects to replica.  
INT-068: Read replica: Factory with read-only connection string → Read-only.  
INT-069: Connection string rotation: New config → Factory uses new string.  
INT-070: Multi-tenant: Factory with tenant-specific schema → Correct schema.  
INT-071: Schema migration: New schema during parallel → Handled.  
INT-072: Index rebuild: Vacuum during parallel → No deadlock.  
INT-073: Statistics update: ANALYZE during parallel → No conflict.  
INT-074: Replication lag: Read from replica with lag → Eventual consistency.  
INT-075: Circuit breaker: DB down → Factory fails fast.  
INT-076: Retry with exponential backoff: Transient failure → Retry succeeds.  
INT-077: Timeout: Long-running query → TimeoutException.  
INT-078: Cancellation propagation: API request cancelled → Context disposed.  
INT-079: Telemetry: OpenTelemetry with parallel → Spans correct.  
INT-080: Audit logging: Parallel read → No audit entries (read-only).  
INT-081: Compliance: Parallel read with audit trail → No write audit.  
INT-082: Data residency: Factory with region-specific connection → Correct region.  
INT-083: Encryption at rest: Factory with encrypted DB → Transparent.  
INT-084: TLS: Factory with SSL connection string → Encrypted.  
INT-085: Authentication: Factory with service account → Auth correct.  
INT-086: Authorization: Factory context with user context → Read permissions.  
INT-087: oUP integration: Deep link resolution with factory → Correct data.  
INT-088: Rules engine: Entity lookup during parallel → Correct resolution.  
INT-089: DST AI profiling: Profiling data load via factory → Correct.  
INT-090: TheGo decision: Workflow decision data via factory → Correct.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: 10 parallel tasks on same opportunity → All return consistent data.  
CON-002: 10 parallel tasks on different opportunities → All independent.  
CON-003: Concurrent `GetOpportunityDetailsForAIAsync` calls for same opportunity → Both succeed.  
CON-004: Concurrent `GetOpportunityDetailsForAIAsync` calls for different opportunities → Independent.  
CON-005: 5 concurrent API requests each triggering 10 parallel tasks → 50 factory contexts.  
CON-006: Factory context creation is thread-safe → No race conditions.  
CON-007: Factory context disposal is thread-safe → No double-free.  
CON-008: Connection pool access is thread-safe → No corruption.  
CON-009: Task.WhenAll with mixed fast/slow tasks → All complete.  
CON-010: Parallel read + concurrent write by another request → Read sees old or new.  
CON-011: Parallel read during SaveChanges by main context → Snapshot isolation.  
CON-012: Two workflow adapters using factory concurrently → Independent.  
CON-013: PubSubPullService processing multiple messages concurrently → Independent contexts.  
CON-014: GeminiManager + OpportunityManager parallel factory use → No interference.  
CON-015: Connection pool under concurrent factory stress → Queuing, no corruption.  
CON-016: Parallel queries during database maintenance → Handled gracefully.  
CON-017: Parallel queries during connection pool resize → Stable.  
CON-018: Race between context creation and disposal → Safe ordering.  
CON-019: Concurrent factory creation from multiple DI scopes → Independent.  
CON-020: Task.Run on ThreadPool during high CPU → Scheduled eventually.  
CON-021: CancellationToken propagated to all parallel tasks → All cancel.  
CON-022: One task cancelled, others continue → Partial results available.  
CON-023: Concurrent parallel operations across different managers → All succeed.  
CON-024: Factory context used after Task.WhenAll → Safe (results materialized).  
CON-025: Connection recycled between two parallel operations → Valid connection.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Factory Behavior

UNT-001: `CreateDbContextAsync()` returns non-null context.  
UNT-002: `CreateDbContext()` (sync) returns non-null context.  
UNT-003: Each call returns different instance (reference inequality).  
UNT-004: `CreateDbContextAsync(CancellationToken)` respects cancellation.  
UNT-005: Factory context has correct connection string (from options).

### TestDbContextFactory

UNT-006: `TestDbContextFactory` implements `IDbContextFactory<UNOPSAppDbContext>`.  
UNT-007: `TestDbContextFactory.CreateDbContext()` returns valid context.  
UNT-008: `TestDbContextFactory.CreateDbContextAsync()` returns valid context.  
UNT-009: `TestDbContextFactory` contexts share InMemory database.  
UNT-010: `TestDbContextFactory` contexts are disposable.

### Mock Verification

UNT-011: Mock factory `Setup` for `CreateDbContextAsync` → Returns mock context.  
UNT-012: Mock factory `Verify` for `CreateDbContextAsync` → Called expected times.  
UNT-013: Mock factory with multiple setups → Returns correct context per call.  
UNT-014: Mock factory `.Callback()` → Can track context creation count.  
UNT-015: Mock factory with `Throws()` → Simulates creation failure.

### Parallel Query Logic

UNT-016: Parallel task count matches expected (10 for GetOpportunityDetailsForAIAsync).  
UNT-017: Each task creates own context (verified via mock call count).  
UNT-018: Each task disposes context (verified via mock Dispose tracking).  
UNT-019: Task results correctly assigned to expected collections.  
UNT-020: Conditional tasks respect empty ID lists.  
UNT-021: Wave 2 tasks receive correct IDs from Wave 1.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: `CreateDbContextAsync()` < 5ms per context.  
PRF-002: 10 parallel `CreateDbContextAsync()` < 50ms total.  
PRF-003: `GetOpportunityDetailsForAIAsync` parallel vs sequential → Parallel 60-80% faster.  
PRF-004: Parallel execution time ≈ MAX(task times), not SUM.  
PRF-005: Connection acquisition from warm pool < 1ms.  
PRF-006: Connection acquisition from cold pool < 50ms.  
PRF-007: Context disposal < 1ms per context.  
PRF-008: Memory per factory context ≈ baseline (no change tracking).  
PRF-009: 10 parallel queries on 1K-row collections → Total < 2s.  
PRF-010: 10 parallel queries on 10K-row collections → Total < 10s.  
PRF-011: Connection pool warmup (MinPoolSize=10) at startup < 5s.  
PRF-012: GC pressure from 100 sequential parallel operations → Stable.  
PRF-013: Thread pool utilization during parallel queries → Not saturated.  
PRF-014: Database CPU during 10 parallel queries → < 50% (distributed load).  
PRF-015: Network round trips: parallel = same as sequential (10 each).  
PRF-016: Total AI data loading (GetOpportunityDetailsForAIAsync) < 60s.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 10 concurrent `GetOpportunityDetailsForAIAsync` calls → 100 factory contexts → All succeed.  
LDT-002: 20 concurrent calls → 200 factory contexts → Pool handles.  
LDT-003: 50 concurrent calls → 500 factory contexts → Pool pressure, all eventually succeed.  
LDT-004: Sustained 5 calls/minute for 1 hour → Connection pool stable.  
LDT-005: Spike: 30 calls in 5 seconds → Pool queues, all complete within 30s.  
LDT-006: Connection pool at MaxPoolSize → New requests queue, don't fail.  
LDT-007: Database under load (1000 concurrent queries) → Factory contexts still created.  
LDT-008: Memory under load (100 concurrent parallel operations) → No OOM.  
LDT-009: Recovery after pool exhaustion → Pool returns to normal within 30s.  
LDT-010: Recovery after database restart → Factory contexts reconnect.

---

## Known Issues

| ID | Issue | Status | Reference |
|----|-------|--------|-----------|
| QA-063 | InMemory DbContext not thread-safe for concurrency tests | Workaround Applied | `Defect List for QA.md` |
| — | `OpportunityAdvancedFeaturesTests.cs:484` skipped concurrency test | Open | "DbContext is not thread-safe" |
| — | EF Core InMemory lacks true parallel support | Known Limitation | Use Testcontainers.PostgreSql for real concurrency tests |

---

## Status: Ready for Implementation
