# RiskManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/RiskManager`  
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

Risk management for opportunities: CRUD risks, risk categories (Strategic/Operational/Financial/Legal/Reputational), probability/impact scoring (1-5 matrix), risk heat map, mitigation plans, risk owners, monitoring, residual risk calculation, risk register, and reporting.

---

## §1–§10

**§1 (35):** CRUD risks + scoring + categories + mitigation + heat map + register + reporting (35 tests).
**§2 (70):** Input (10), Auth (10), State (10), injection (10), dependencies (10), format (10), business (invalid score, probability×impact calc, duplicate risk, orphan mitigation, max risks, circular reference, invalid category, missing owner, score out of range, mass assignment).
**§3 (70):** Probability (1–5), impact (1–5), score (1–25), risk count (0–100+), mitigation count per risk, description lengths, concurrent, Unicode, pagination, heat map data points, category distributions, residual vs inherent, date ranges, owner count.
**§4 (50):** Score calculation (15), mitigation tracking (10), heat map (10), categorization (10), audit (5).
**§5 (50):** Opportunity (10), notification (10), DST (10), export (10), reporting (10).
**§6 (50):** Injection (10), auth (10), IDOR (10), data exposure (10), scoring manipulation (10).
**§7 (25):** Concurrent risk CRUD, score updates, mitigation + risk, bulk operations, owner changes.
**§8 (21):** Score calc (5), probability×impact (5), residual calc (3), validation (5), formatting (3).
**§9 (16):** CRUD (<200ms), heat map (<500ms), register (<500ms), export (<3s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large registers, recovery.

---

**Status:** Ready for Execution
