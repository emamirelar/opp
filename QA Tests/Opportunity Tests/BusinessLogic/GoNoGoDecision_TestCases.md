# Go/No-Go Decision (Legacy) — Test Cases

**Component:** Opportunity Go/No-Go Decision Process (Legacy file — see PNO-969 for authoritative)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio  
**Note:** The authoritative Go/No-Go test cases are in `PNO-969_GoDecision_TestCases.md`. This file covers supplementary decision workflow scenarios.

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

Supplementary decision workflow scenarios beyond the core Go/No-Go (PNO-969): decision criteria management, scoring matrix, decision committee, voting/quorum, decision rationale capture, historical decisions, templates, escalation, delegation, conditional decisions, decision impact analysis, re-decision after material change, and decision expiry.

---

## §1 Positive — 35

POS-001–005 (P0): Create decision criteria, score opportunity against criteria, record committee vote, generate decision rationale, link decision to stage change.
POS-006–035 (P1/P2): Criteria CRUD, weight assignment, scoring matrix, quorum check, vote recording, delegation, escalation, conditional approval, expiry setup, historical query, template management, impact analysis, re-decision trigger, notification, audit, export, search, filter, pagination, sort, model mapping, typeahead, count, bulk operations, criteria categories.

## §2 Negative — 70

NEG-001–010: Input (null, empty, invalid criteria, non-existent opp, deleted opp, invalid score, negative weight, weights>100, null committee, empty vote).
NEG-011–020: Auth (10 tests).
NEG-021–030: State (vote on closed, approve without quorum, re-decide without change, expired decision, duplicate vote, modify locked, delete final, score incomplete criteria, escalate without reason, delegate to self).
NEG-031–040: SQL/XSS/injection (10).
NEG-041–050: Dependencies (10).
NEG-051–060: Format/ID (10).
NEG-061–070: Business rules (insufficient votes, conflicting decisions, missing mandatory criteria, circular escalation, max re-decisions, expired committee, invalid delegation chain, quorum changed mid-vote, criteria removed mid-scoring, mass assignment).

## §3 Boundary — 70

BND-001–070: Criteria count (0/1/10/50/51), score range (0.0/0.5/1.0/min/max), weights (0-100%), votes (0/1/quorum-1/quorum/quorum+1), committee size (1/3/10/50), rationale length (0/1/1000/4000/4001), decision history count, date boundaries, Unicode, pagination, concurrent, comparison, search terms, delegation depth.

## §4 Functional — 50 | §5 Integration — 50 | §6 Security — 50

**§4:** Scoring workflow (15), committee management (10), voting process (10), rationale capture (10), audit (5).
**§5:** Opportunity linking (10), notification (10), workflow (10), export (10), committee service (10).
**§6:** Injection (10), access control (10), IDOR (10), voting security (10), data privacy (10).

## §7–§10

**§7 (25):** Concurrent votes, score + vote, modify criteria during voting, quorum race, etc.
**§8 (21):** Score calculation (5), quorum check (3), weight validation (5), status (5), formatting (3).
**§9 (16):** Score (<200ms), vote (<200ms), list (<300ms), PDF (<3s), history (<500ms), memory tests.
**§10 (10):** 50 concurrent votes, spike, sustained, recovery tests.

---

**Status:** Ready for Execution
