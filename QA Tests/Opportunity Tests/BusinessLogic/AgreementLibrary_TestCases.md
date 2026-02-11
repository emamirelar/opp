# Agreement Library — Test Cases

**Component:** Opportunity Agreement Library  
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

Partnership agreement templates, versioning, linking to opportunities, clause management, approval workflows, document generation, PDF export, digital signatures, compliance tracking, agreement types (MOU, LOA, Framework), status lifecycle, amendment tracking, expiry notifications, and audit trail.

---

## §1 Positive Tests — 35 tests

| ID | Test Name | Steps | Expected | Pr |
|----|-----------|-------|----------|----|
| POS-001 | Create agreement from template | Select template → CreateAgreement | Agreement created with template fields | P0 |
| POS-002 | Link agreement to opportunity | LinkAgreement(oppId, agrId) | Linked, visible on opp | P0 |
| POS-003 | Generate PDF from agreement | GeneratePDF(agrId) | Valid PDF with all clauses | P0 |
| POS-004 | Update agreement clauses | UpdateClauses(agrId, clauses) | Clauses saved, version incremented | P0 |
| POS-005 | Submit agreement for approval | SubmitForApproval(agrId) | Status=PendingApproval | P0 |
| POS-006 | Approve agreement | Approve(agrId) | Status=Approved, audit logged | P1 |
| POS-007 | Reject agreement | Reject(agrId, reason) | Status=Rejected, reason stored | P1 |
| POS-008 | Create MOU type | Create(type=MOU) | MOU agreement created | P1 |
| POS-009 | Create LOA type | Create(type=LOA) | LOA agreement created | P1 |
| POS-010 | Create Framework type | Create(type=Framework) | Framework created | P1 |
| POS-011 | Add amendment | AddAmendment(agrId, text) | Amendment linked | P1 |
| POS-012 | Version history | GetVersions(agrId) | All versions listed | P1 |
| POS-013 | Search agreements | Search("partnership") | Matching found | P1 |
| POS-014 | Filter by type | Filter(type=MOU) | Only MOUs | P1 |
| POS-015 | Filter by status | Filter(status=Active) | Only active | P1 |
| POS-016 | Paginate agreements | GetPaginated(page=1) | Paginated results | P1 |
| POS-017 | Sort by date | Sort(date, desc) | Newest first | P2 |
| POS-018 | Sort by name | Sort(name, asc) | Alphabetical | P2 |
| POS-019 | Get agreement detail | GetById(agrId) | All fields returned | P1 |
| POS-020 | Soft delete | Delete(agrId) | IsDeleted=true | P1 |
| POS-021 | Expiry notification | Agreement near expiry | Notification sent | P1 |
| POS-022 | Clone agreement | Clone(agrId) | New agreement, same content | P1 |
| POS-023 | Add digital signature | Sign(agrId, userId) | Signature recorded | P1 |
| POS-024 | Multiple signatories | Sign by 3 users | All signatures | P1 |
| POS-025 | Agreement timeline | GetTimeline(agrId) | Events in order | P2 |
| POS-026 | Export to Word | ExportWord(agrId) | Valid .docx | P2 |
| POS-027 | Audit trail | GetAudit(agrId) | Full history | P2 |
| POS-028 | Map to model | mapper.Map | All fields | P2 |
| POS-029 | Get templates list | GetTemplates | Available templates | P2 |
| POS-030 | Template preview | PreviewTemplate(tplId) | Preview rendered | P2 |
| POS-031 | Renew agreement | Renew(agrId) | New agreement linked to old | P1 |
| POS-032 | Compliance check | CheckCompliance(agrId) | Compliance status | P2 |
| POS-033 | Agreement count per opp | GetCount(oppId) | Non-deleted count | P2 |
| POS-034 | Bulk export | ExportAll(oppId) | Zip of PDFs | P2 |
| POS-035 | Agreement typeahead | Typeahead("part") | Matching names | P2 |

---

## §2 Negative Tests — 70 tests

NEG-001–010: Input validation (null name, null template, non-existent oppId, deleted opp, invalid type, null clauses, blank name, duplicate name, missing required clause, invalid date range).
NEG-011–020: Auth (no auth, no create, no approve, no delete, wrong scope, expired token, tampered JWT, disabled, post-logout, escalation).
NEG-021–030: State (approve draft, reject approved, amend rejected, sign unapproved, delete signed, modify locked, submit incomplete, expire active, renew cancelled, clone deleted).
NEG-031–040: SQL/XSS (SQL name, SQL search, XSS clause, XSS name, path traversal, HTML injection, JSON injection, template injection, LDAP, command).
NEG-041–050: Dependencies (DB timeout, connection lost, PDF service down, email service down, storage failure, constraint violation, mapper missing, concurrent lock, pool exhausted, service unavailable).
NEG-051–060: Format (ID negative, ID zero, ID float, ID string, page=0, pageSize=-1, pageSize>1000, invalid sort, empty search, regex chars).
NEG-061–070: Business (link to wrong entity type, exceed max amendments, invalid signature, expired certificate, invalid template format, circular reference, max file size, invalid PDF, empty export, mass assignment).

---

## §3 Boundary Tests — 70 tests

BND-001–010: String lengths (name 1/200/201, clause 1/10000/10001, description 0/4000/4001, template name 1).
BND-011–020: Counts (0/1/10/100/1000 agreements, 0/1/50 clauses per agreement, 0/1/10 amendments, 0/1/5 signatures).
BND-021–030: Pagination (page 1, last, pageSize 1/1000, exactly page size, +1, total items).
BND-031–040: Dates (today, past, far future, leap year, midnight, year boundary, expiry=today, expiry=tomorrow, expiry yesterday, duration 1 day).
BND-041–050: Unicode (Arabic, Chinese, Cyrillic, French, emoji, mixed script, RTL, long Unicode, special chars, apostrophe).
BND-051–060: File (PDF 1KB, PDF 10MB, PDF 50MB, Word 1KB, template minimal, template maximal, zero attachments, max attachments, image in template, formula in template).
BND-061–070: Version (v1, v2, v100, amendment v1, amendment v50, concurrent version, rollback, restore, export specific version, compare versions).

---

## §4 Functional — 50 | §5 Integration — 50 | §6 Security — 50

**§4:** Template selection, clause management, approval workflow, PDF generation, signature verification, expiry calculation, amendment tracking, version control, compliance rules, notification trigger, audit creation, status transitions (15 workflow + 15 validation + 10 constraints + 10 audit).
**§5:** CRUD lifecycle, opportunity linking, partner association, document storage, email notifications, PDF service, signature service, template engine, search index, export service (10 each × 5 groups).
**§6:** Injection (10), access control (10), IDOR (10), document security (10), signature security (10).

## §7 Concurrency — 25 | §8 Unit — 21 | §9 Performance — 16 | §10 Load — 10

**§7:** Concurrent approval, sign, edit, version, delete, clone, export, amend, search, PDF generation (25 scenarios).
**§8:** Validation (5), formatting (3), calculations (5), state (5), collections (3).
**§9:** Create (<200ms), PDF gen (<2s), search (<500ms), list 100 (<300ms), export (<5s), concurrent 10 (<1s), memory (6 tests).
**§10:** 50 concurrent ops (30min), 100 reads, spike, sustained, recovery (10 tests).

---

## Traceability Matrix

| Rule | Tests |
|------|-------|
| Agreement types | POS-008–010 |
| Approval workflow | POS-005–007, NEG-021–022 |
| PDF generation | POS-003, BND-051–054 |
| Signatures | POS-023–024, NEG-031 |
| Expiry | POS-021, BND-037–039 |

**Status:** Ready for Execution
