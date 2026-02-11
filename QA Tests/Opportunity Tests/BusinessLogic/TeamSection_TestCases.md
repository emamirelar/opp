# Team Section — Test Cases

**Component:** Opportunity Team Section Management  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

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

Manages the Team section of opportunities: team member assignment, roles (OM, Collaborator, Reviewer), capacity allocation (%), availability, skills/competencies, org unit mapping, team composition rules, succession planning, conflict of interest check, and team history.

---

## §1–§10

**§1 Positive (35):** Add team member (P0), assign role (P0), set allocation % (P0), get team for opp (P0), remove member (P0), + 30 P1/P2 (update role, update allocation, multiple members, OM assignment, Collaborator assignment, Reviewer assignment, search members, filter by role, capacity check, skills display, org unit display, team history, succession planning, COI check, availability check, export team, audit, pagination, sort, model mapping, typeahead, count, bulk add, reorder, transfer, team template, clone team, notification, validate composition, completeness check).

**§2 Negative (70):** Input (null member, non-existent user, deleted user, invalid role, null oppId, duplicate member, self-assign, allocation > 100%, allocation < 0%, missing required role), Auth (10), State (add to closed, modify locked, remove last OM, add during workflow, update approved), injection (10), dependencies (10), format/ID (10), business rules (no OM, multiple OMs if restricted, capacity exceeded, incompatible roles, org mismatch, COI detected, skill gap, availability conflict, max team size, unauthorized org).

**§3 Boundary (70):** Team size (0/1/2/10/50/100/101), allocation (0/1/50/99/100/101/200%), member name lengths, role count, skills count, org unit depth, concurrent additions, capacity calculations, availability ranges, search terms, pagination, date ranges, Unicode names, succession depth, COI complexity.

**§4 (50):** Role management (15), allocation tracking (10), composition validation (10), availability (10), audit (5).
**§5 (50):** User service (10), opportunity (10), org hierarchy (10), notification (10), skills service (10).
**§6 (50):** Injection (10), access control (10), IDOR (10), role security (10), data exposure (10).
**§7 (25):** Concurrent add/remove/update, capacity race, role change during workflow, etc.
**§8 (21):** Allocation calc (5), role validation (5), composition rules (3), capacity (5), formatting (3).
**§9 (16):** Add (<200ms), list (<300ms), search (<500ms), capacity calc (<100ms), export (<2s), memory.
**§10 (10):** 50 concurrent team ops, spike, sustained, large teams, recovery.

---

**Status:** Ready for Execution
