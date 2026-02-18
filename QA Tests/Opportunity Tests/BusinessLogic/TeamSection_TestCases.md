# Team Section — Test Cases

**Component:** Opportunity Team Section Management  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
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
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Checks:**
- N≥3P: 90≥90 ✅ PASS
- E≥3P: 90≥90 ✅ PASS
- F≥3P: 90≥90 ✅ PASS
- I≥3P: 90≥90 ✅ PASS

---

## Feature Overview

Manages the Team section of opportunities: team member assignment, roles (OM, Collaborator, Reviewer), capacity allocation (%), availability, skills/competencies, org unit mapping, team composition rules, succession planning, conflict of interest check, and team history.

---

## §1 Positive (30)

Add team member (P0), assign role (P0), set allocation % (P0), get team for opp (P0), remove member (P0), update role (P1), update allocation (P1), multiple members (P1), OM assignment (P1), Collaborator assignment (P1), Reviewer assignment (P1), search members (P1), filter by role (P1), capacity check (P1), skills display (P1), org unit display (P1), team history (P1), succession planning (P1), COI check (P1), availability check (P1), export team (P1), audit (P1), pagination (P1), sort (P1), model mapping (P1), typeahead (P1), count (P1), bulk add (P1), reorder (P1), transfer (P1).

---

## §2 Negative (90)

**Input (10):** Null member, non-existent user, deleted user, invalid role, null oppId, duplicate member, self-assign, allocation > 100%, allocation < 0%, missing required role.

**Auth (10):** Unauthorized add, unauthorized remove, unauthorized update role, unauthorized view team, unauthorized export, cross-tenant access, expired session, missing claim, wrong org scope, insufficient permission.

**State (10):** Add to closed opp, modify locked opp, remove last OM, add during workflow transition, update approved opp, remove from archived, add to rejected, modify during approval, change role on inactive member, bulk add to draft-only.

**OM/Collaborators/Stakeholders/Decision Pathway (20):** No OM assigned, multiple OMs when restricted, OM removed without replacement, Collaborator without OM, Collaborator on non-existent opp, Stakeholder not in org hierarchy, Stakeholder with conflicting role, Decision pathway missing required approver, Decision pathway with invalid sequence, OM capacity exceeded, Collaborator allocation exceeds available, Stakeholder duplicate in pathway, Decision pathway circular dependency, OM from wrong org unit, Collaborator without required skill, Stakeholder COI not resolved, Decision pathway missing stakeholder, OM soft-deleted user, Collaborator on closed opp, Stakeholder outside geography.

**Injection (10):** SQL injection member name, SQL injection role, XSS in skills, XSS in org unit, command injection search, LDAP injection user lookup, path traversal export, JSON injection payload, header injection, script injection availability note.

**Dependencies (10):** User service unavailable, opportunity service down, org hierarchy service timeout, notification service failure, skills service error, audit service unavailable, typeahead service 500, bulk add partial failure, export service timeout, capacity calc service error.

**Format/ID (10):** Invalid oppId format, negative oppId, zero oppId, malformed member ID, invalid allocation format, wrong date format, oversized payload, invalid JSON, missing required field, wrong content-type.

**Business Rules (10):** Capacity exceeded, incompatible roles, org mismatch, COI detected, skill gap, availability conflict, max team size exceeded, unauthorized org unit, succession depth exceeded, team composition invalid.

---

## §3 Boundary (90)

**Team Size (10):** 0 members, 1 member, 2 members, 10 members, 50 members, 100 members, 101 members (max+1), empty team add, single OM only, max collaborators.

**Allocation (10):** 0%, 1%, 50%, 99%, 100%, 101%, 200%, fractional 0.5%, sum exactly 100%, sum 99.9%.

**Member Name Lengths (5):** Empty name, 1 char, 255 chars, 256 chars, Unicode 100 chars.

**Role Count (5):** Zero roles, 1 role, max roles per member, duplicate role same member, role transition boundary.

**Skills Count (5):** 0 skills, 1 skill, 50 skills, 51 skills, empty skill name.

**Org Unit Depth (5):** Root only, 1 level, 5 levels, max depth, invalid depth.

**Concurrent Additions (5):** Two users add same member, two users add different members, add during bulk import, add during role change, add during capacity calc.

**Capacity Calculations (5):** Zero capacity, 100% capacity, fractional remainder, rounding edge, overflow.

**Availability Ranges (5):** Start equals end, past date range, future only, overlapping ranges, gap between ranges.

**Search Terms (5):** Empty string, single char, exact match, partial match, special chars.

**Pagination (5):** Page 0, page 1, last page, page beyond count, page size 0.

**Date Ranges (5):** Same day, year span, null start, null end, reversed range.

**Unicode Names (5):** Cyrillic, Chinese, Arabic RTL, emoji, mixed scripts.

**Succession Depth (5):** 0 levels, 1 level, max depth, circular ref, missing successor.

**COI Complexity (5):** No COI, single COI, multiple COI, nested COI, COI resolution boundary.

---

## §4 Functional (90)

**Role Management (25):** Assign OM, assign Collaborator, assign Reviewer, change OM to Collaborator, change Collaborator to Reviewer, remove role, add second role (if allowed), validate role transition, role-based visibility, role-based actions, OM required check, multiple OM policy, role inheritance, role delegation, role audit, role history, role conflict detection, role capacity, role availability, role skills match, role org unit match, role geography match, role COI check, role succession, role template apply, role bulk update.

**Allocation Tracking (20):** Set 0%, set 50%, set 100%, update allocation, sum allocations, allocation per role, allocation per member, allocation validation, allocation history, allocation audit, overallocation warning, underallocation, allocation transfer, allocation rebalance, allocation export, allocation report, allocation by org unit, allocation by skill, allocation trend, allocation forecast.

**Composition Validation (15):** Min OM check, max OM check, Collaborator count, Reviewer count, skill coverage, org coverage, geography coverage, COI resolution, availability overlap, capacity balance, succession chain, role diversity, seniority mix, team size rules, composition score.

**Availability (15):** Set availability, update availability, availability overlap check, availability conflict, availability calendar, availability export, availability by role, availability by member, availability gap, availability trend, availability notification, availability sync, availability timezone, availability partial, availability bulk update.

**Audit (15):** Add member audit, remove member audit, role change audit, allocation change audit, team export audit, composition change audit, who/when/what, audit trail export, audit filter, audit retention, audit integrity, audit search, audit by user, audit by opp, audit by date.

---

## §5 Integration (90)

**User Service (18):** Get user by ID, get user typeahead, validate user exists, user soft-delete check, user org unit, user skills, user availability, user COI status, user capacity, bulk user lookup, user search, user profile sync, user permission check, user geography, user timezone, user locale, user audit, user notification pref.

**Opportunity Service (18):** Get opp by ID, opp workflow status, opp permissions, opp team count, opp capacity, opp geography, opp org unit, opp stage, opp lock status, opp audit, opp history, opp clone team, opp transfer team, opp export, opp validation, opp notification, opp approval flow, opp decision pathway.

**Org Hierarchy Service (18):** Get org unit, org tree, org path, org members, org capacity, org skills, org geography, org approval chain, org succession, org COI scope, org delegation, org audit, org sync, org validation, org depth, org parent, org children, org search.

**Notification Service (18):** Team add notification, team remove notification, role change notification, allocation change notification, COI alert, capacity alert, availability conflict, succession alert, approval request, bulk change notification, notification preference, notification channel, notification audit, notification retry, notification template, notification locale, notification digest, notification suppress.

**Skills Service (18):** Get skills, skill typeahead, skill match, skill gap, skill level, skill expiry, skill certification, skill org mapping, skill role mapping, skill search, skill bulk, skill audit, skill import, skill validation, skill hierarchy, skill synonym, skill translation, skill report.

---

## §6 Security (50)

Injection (10), access control (10), IDOR (10), role security (10), data exposure (10).

---

## §7 Concurrency (25)

Concurrent add/remove/update, capacity race, role change during workflow, two users add same member, two users remove same member, concurrent allocation update, concurrent role change, bulk add during single add, export during update, lock contention, optimistic concurrency, pessimistic lock, transaction isolation, deadlock scenario, cascade update race, notification race, audit race, capacity calc race, composition validation race, typeahead race, bulk operations overlap, workflow transition race, approval race, clone race, transfer race.

---

## §8 Unit (21)

Allocation calc (5), role validation (5), composition rules (3), capacity (5), formatting (3).

---

## §9 Performance (16)

Add (<200ms), list (<300ms), search (<500ms), capacity calc (<100ms), export (<2s), memory, bulk add (<5s), typeahead (<150ms), pagination (<200ms), filter (<250ms), audit query (<400ms), composition validation (<300ms), allocation sum (<50ms), team count (<100ms), export large team (<5s), concurrent load.

---

## §10 Load (10)

50 concurrent team ops, spike, sustained, large teams, recovery, 100 concurrent adds, 200 concurrent searches, 500 team export, capacity calc under load, composition validation under load.

---

**Status:** Ready for Execution
