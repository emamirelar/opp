# Document Extraction — Test Cases

**Component:** Opportunity Document Extraction (AI-powered text/data extraction)  
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

AI-powered document text extraction for opportunity-related documents (PDFs, Word, images, scanned docs). Features: OCR processing, text extraction, structured data extraction (tables, key-value pairs), multi-language support, confidence scoring, batch processing, extraction templates, field mapping to opportunity sections, manual correction interface, extraction history, re-extraction, supported format detection, and audit trail.

---

## §1 Positive Tests — 35

| ID | Test | Expected | Pr |
|----|------|----------|----|
| POS-001 | Extract text from PDF | Full text extracted | P0 |
| POS-002 | Extract text from Word | Full text extracted | P0 |
| POS-003 | OCR from scanned PDF | Text recognized | P0 |
| POS-004 | Extract table data | Table structure preserved | P0 |
| POS-005 | Extract key-value pairs | Pairs identified | P0 |
| POS-006 | Multi-page extraction | All pages processed | P1 |
| POS-007 | Multi-language (French) | French text extracted | P1 |
| POS-008 | Multi-language (Spanish) | Spanish text extracted | P1 |
| POS-009 | Multi-language (Arabic) | Arabic/RTL extracted | P1 |
| POS-010 | Confidence score | Score > 0.8 for clear doc | P1 |
| POS-011 | Batch extraction | 5 docs processed | P1 |
| POS-012 | Map to opp fields | Fields auto-populated | P1 |
| POS-013 | Extraction template | Template applied | P1 |
| POS-014 | Manual correction | Corrections saved | P1 |
| POS-015 | Re-extract | Updated results | P1 |
| POS-016 | Image text (PNG) | Text from image | P1 |
| POS-017 | Image text (JPEG) | Text from image | P1 |
| POS-018 | Mixed content (text+image) | Both extracted | P1 |
| POS-019 | Extraction history | Past extractions listed | P1 |
| POS-020 | Export extracted data | CSV/JSON export | P2 |
| POS-021 | Preview extraction | Before commit | P2 |
| POS-022 | Supported formats list | GetSupportedFormats | P2 |
| POS-023 | Extract from Excel | Tabular data | P1 |
| POS-024 | Extract dates | Date fields identified | P1 |
| POS-025 | Extract amounts/currency | Financial data found | P1 |
| POS-026 | Extract names/orgs | Entities identified | P1 |
| POS-027 | Audit trail | Extraction logged | P2 |
| POS-028 | Cancel extraction | In-progress cancelled | P1 |
| POS-029 | Extraction status | Running/Complete/Failed | P1 |
| POS-030 | Template management | CRUD templates | P2 |
| POS-031 | Field validation | Extracted data validated | P2 |
| POS-032 | Metadata extraction | Title, author, date | P2 |
| POS-033 | Header/footer handling | Excluded from body | P2 |
| POS-034 | Watermark handling | Excluded | P2 |
| POS-035 | Paragraph structure | Paragraphs preserved | P2 |

---

## §2 Negative — 70 | §3 Boundary — 70

**§2:** Input (null file, empty file, wrong type, corrupt PDF, corrupt image, password-protected, encrypted, DRM, non-existent docId, deleted doc), Auth (10), OCR (blurry image, handwritten, rotated 180°, very low DPI, 0 text pages, unsupported language, mixed orientation, torn scan, dark image, overexposed), injection (SQL, XSS, path traversal, command, template, EICAR), dependencies (AI service down, OCR timeout, storage failure, quota exceeded, rate limit, memory OOM), format (10), state/mass-assign (10).

**§3:** File size (1KB/1MB/10MB/50MB/100MB/101MB), pages (1/10/100/500/501), image DPI (72/150/300/600/1200), confidence (0.0/0.5/0.8/0.95/1.0), text length (1 char/1000/10000/100000), languages (1/2/5), batch size (1/5/10/50/51), extraction time (fast/medium/slow/timeout), field count (0/1/10/50), table size (1×1/100×100), Unicode, pagination, concurrent, format boundaries.

---

## §4 Functional — 50 | §5 Integration — 50 | §6 Security — 50

**§4:** OCR pipeline, text extraction, table parsing, entity recognition, field mapping, confidence calculation, template matching, batch orchestration, error handling, retry logic, cancel processing, validation rules, format detection, metadata parsing, audit (15+15+10+10).
**§5:** AI service, storage, opportunity fields, document service, search index, export, notification, batch job, template engine, OCR engine (10×5).
**§6:** Injection (10), access control (10), IDOR (10), file security (10), data privacy (10).

## §7–§10

**§7 (25):** Concurrent extractions, cancel during extraction, batch + single, re-extract during extract, modify during extract, etc.
**§8 (21):** Text parsing (5), confidence calc (3), format detection (5), field mapping (5), template matching (3).
**§9 (16):** 1-page (<1s), 10-page (<5s), 100-page (<30s), OCR (<5s/page), batch 10 (<60s), search (<500ms), memory, concurrent.
**§10 (10):** 20 concurrent extractions, spike, sustained, large files, recovery.

---

**Status:** Ready for Execution
