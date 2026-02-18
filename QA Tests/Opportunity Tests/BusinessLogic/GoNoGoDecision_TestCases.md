# Go/No-Go Decision (Legacy) — Test Cases

**Component:** Opportunity Go/No-Go Decision Process (Legacy file — see PNO-969 for authoritative)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio  
**Note:** The authoritative Go/No-Go test cases are in `PNO-969_GoDecision_TestCases.md`. This file covers supplementary decision workflow scenarios.

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- E ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

Supplementary decision workflow scenarios beyond the core Go/No-Go (PNO-969): decision criteria management, scoring matrix, decision committee, voting/quorum, decision rationale capture, historical decisions, templates, escalation, delegation, conditional decisions, decision impact analysis, re-decision after material change, and decision expiry.

---

## §1 Positive — 30

POS-001–005 (P0): Create decision criteria, score opportunity against criteria, record committee vote, generate decision rationale, link decision to stage change.
POS-006–030 (P1/P2): Criteria CRUD, weight assignment, scoring matrix, quorum check, vote recording, delegation, escalation, conditional approval, expiry setup, historical query, template management, impact analysis, re-decision trigger, notification, audit, export, search, filter, pagination, sort, model mapping, typeahead, count, bulk operations, criteria categories.

## §2 Negative — 90

NEG-001–010: Input (null, empty, invalid criteria, non-existent opp, deleted opp, invalid score, negative weight, weights>100, null committee, empty vote).
NEG-011–020: Auth (10 tests).
NEG-021–030: State (vote on closed, approve without quorum, re-decide without change, expired decision, duplicate vote, modify locked, delete final, score incomplete criteria, escalate without reason, delegate to self).
NEG-031–040: SQL/XSS/injection (10).
NEG-041–050: Dependencies (10).
NEG-051–060: Format/ID (10).
NEG-061–070: Business rules (insufficient votes, conflicting decisions, missing mandatory criteria, circular escalation, max re-decisions, expired committee, invalid delegation chain, quorum changed mid-vote, criteria removed mid-scoring, mass assignment).
NEG-071–080: API/Contract (invalid payload, wrong content-type, malformed JSON, missing required fields, extra unknown fields, wrong HTTP method, version mismatch, schema violation, empty body, oversized payload).
NEG-081–090: Time/Expiry (stale token, expired session, clock skew, past decision date, future cutoff, timezone mismatch, concurrent expiry check, race on expiry, invalid date format, null timestamp).

## §3 Boundary — 90

BND-001–070: Criteria count (0/1/10/50/51), score range (0.0/0.5/1.0/min/max), weights (0-100%), votes (0/1/quorum-1/quorum/quorum+1), committee size (1/3/10/50), rationale length (0/1/1000/4000/4001), decision history count, date boundaries, Unicode, pagination, concurrent, comparison, search terms, delegation depth.
BND-071–080: Weight precision (99.99%, 0.01%, 50.00%), score precision (0.001, 0.999), committee min/max (0, 51, 100), vote threshold edge (exactly quorum, quorum minus one).
BND-081–090: Rationale min/max chars, criteria name length, delegation chain depth (1/5/10), pagination offset/limit edges, sort field boundaries, filter combination limits, bulk batch size (1/100/101), date range extremes, concurrent request ordering.

## §4 Functional — 90

**Core (40):** Scoring workflow (15), committee management (10), voting process (10), rationale capture (5).
**Extended (30):** Audit trail (10), decision history (10), template apply (5), escalation flow (5).
**Advanced (20):** Delegation chain (5), conditional approval (5), re-decision trigger (5), impact analysis (5).

## §5 Integration — 90

**Opportunity & Workflow (30):** Opportunity linking (10), stage change integration (10), workflow status sync (10).
**External (30):** Notification service (10), export service (10), committee service (10).
**Data & API (30):** Database persistence (10), API contract (10), event/message bus (10).

## §7–§10

**§7 Concurrency (25):** Concurrent votes, score + vote, modify criteria during voting, quorum race, lock contention, optimistic concurrency, vote overwrite, criteria update race, delegation race, committee change during vote, etc.

**§8 Unit (21):** Score calculation (5), quorum check (3), weight validation (5), status (5), formatting (3).

**§9 Performance (16):** Score (<200ms), vote (<200ms), list (<300ms), PDF (<3s), history (<500ms), memory tests.

**§10 Load (10):** 50 concurrent votes, spike, sustained, recovery tests.

---

**Status:** Ready for Execution
