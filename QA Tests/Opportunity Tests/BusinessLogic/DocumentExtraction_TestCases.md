# Document Extraction — Test Cases

**Component:** Opportunity Document Extraction (AI-powered text/data extraction)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

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

**Ratio Checks:**
- N≥3P: 90≥90 ✅ PASS
- E≥3P: 90≥90 ✅ PASS
- F≥3P: 90≥90 ✅ PASS
- I≥3P: 90≥90 ✅ PASS

---

## Feature Overview

AI-powered document text extraction for opportunity-related documents (PDFs, Word, images, scanned docs). Features: OCR processing, text extraction, structured data extraction (tables, key-value pairs), multi-language support, confidence scoring, batch processing, extraction templates, field mapping to opportunity sections, manual correction interface, extraction history, re-extraction, supported format detection, and audit trail.

---

## §1 Positive Tests — 30

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

---

## §2 Negative Tests — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| NEG-001 | Null file input | 400 Bad Request | P0 |
| NEG-002 | Empty file (0 bytes) | 400 or empty result | P0 |
| NEG-003 | Wrong file type (.exe) | 400 Unsupported format | P0 |
| NEG-004 | Corrupt PDF | Extraction fails gracefully | P0 |
| NEG-005 | Corrupt image | Extraction fails gracefully | P0 |
| NEG-006 | Password-protected PDF | 400 or prompt for password | P0 |
| NEG-007 | Encrypted document | 400 Access denied | P0 |
| NEG-008 | DRM-protected file | 400 Not extractable | P0 |
| NEG-009 | Non-existent docId | 404 Not found | P0 |
| NEG-010 | Deleted document | 404 Not found | P0 |
| NEG-011 | Unauthenticated request | 401 Unauthorized | P0 |
| NEG-012 | Expired token | 401 Unauthorized | P0 |
| NEG-013 | Invalid API key | 401 Unauthorized | P0 |
| NEG-014 | Missing permission | 403 Forbidden | P0 |
| NEG-015 | Wrong opportunity scope | 403 Forbidden | P0 |
| NEG-016 | Cross-tenant doc access | 403 Forbidden | P0 |
| NEG-017 | Revoked user access | 401 Unauthorized | P0 |
| NEG-018 | Session expired | 401 Unauthorized | P0 |
| NEG-019 | Insufficient role | 403 Forbidden | P0 |
| NEG-020 | Anonymous extraction | 401 Unauthorized | P0 |
| NEG-021 | Blurry image | Low confidence or fail | P1 |
| NEG-022 | Handwritten text | Unsupported or low confidence | P1 |
| NEG-023 | Rotated 180° image | May fail or low confidence | P1 |
| NEG-024 | Very low DPI (<72) | Poor extraction or fail | P1 |
| NEG-025 | 0 text pages | Empty result | P1 |
| NEG-026 | Unsupported language | Fallback or fail | P1 |
| NEG-027 | Mixed orientation | Partial fail or warning | P1 |
| NEG-028 | Torn scan | Partial extraction | P1 |
| NEG-029 | Dark image | Low confidence or fail | P1 |
| NEG-030 | Overexposed image | Low confidence or fail | P1 |
| NEG-031 | SQL injection in filename | Sanitized or rejected | P0 |
| NEG-032 | XSS in document content | Escaped in output | P0 |
| NEG-033 | Path traversal in docId | 400 Rejected | P0 |
| NEG-034 | Command injection | Rejected or sanitized | P0 |
| NEG-035 | Template injection | Sanitized or rejected | P0 |
| NEG-036 | EICAR test file | Blocked by AV | P0 |
| NEG-037 | AI service down | 503 or graceful degradation | P0 |
| NEG-038 | OCR service timeout | Timeout error | P0 |
| NEG-039 | Storage failure | 500 or retry | P0 |
| NEG-040 | Quota exceeded | 429 or quota error | P0 |
| NEG-041 | Rate limit exceeded | 429 Too Many Requests | P0 |
| NEG-042 | Memory OOM | 500 or graceful fail | P0 |
| NEG-043 | .txt as PDF | Format mismatch error | P1 |
| NEG-044 | .html as Word | Format mismatch or parse fail | P1 |
| NEG-045 | Empty PDF (no pages) | 400 or empty result | P1 |
| NEG-046 | Corrupt Word header | Extraction fails | P1 |
| NEG-047 | Unsupported Excel version | 400 Unsupported | P1 |
| NEG-048 | BMP (unsupported) | 400 Unsupported format | P1 |
| NEG-049 | TIFF multi-page edge | May fail or partial | P1 |
| NEG-050 | ODF (unsupported) | 400 Unsupported | P1 |
| NEG-051 | RTF with embedded binary | Partial extraction | P1 |
| NEG-052 | Malformed JSON metadata | Ignored or error | P1 |
| NEG-053 | Invalid opportunity ID | 400 Bad Request | P0 |
| NEG-054 | Negative docId | 400 Bad Request | P0 |
| NEG-055 | Zero docId | 400 Bad Request | P0 |
| NEG-056 | Null template ID | 400 or default template | P1 |
| NEG-057 | Invalid field mapping | 400 or validation error | P1 |
| NEG-058 | Mass-assign readonly field | Ignored or 400 | P1 |
| NEG-059 | Mass-assign internal field | Ignored or 403 | P1 |
| NEG-060 | Concurrent delete during extract | 409 Conflict | P1 |
| NEG-061 | Extract soft-deleted doc | 404 Not found | P1 |
| NEG-062 | Malformed multipart | 400 Bad Request | P0 |
| NEG-063 | Missing Content-Type | 400 Bad Request | P0 |
| NEG-064 | Oversized filename | 400 or truncated | P1 |
| NEG-065 | Null batch items | 400 Bad Request | P0 |
| NEG-066 | Empty batch array | 400 Bad Request | P0 |
| NEG-067 | Duplicate docId in batch | 400 or deduplicated | P1 |
| NEG-068 | Invalid confidence threshold | 400 Bad Request | P1 |
| NEG-069 | Negative page range | 400 Bad Request | P0 |
| NEG-070 | Page range beyond doc | 400 or clamped | P1 |
| NEG-071 | Invalid language code | 400 or default | P1 |
| NEG-072 | Null extraction options | 400 or defaults | P1 |
| NEG-073 | Invalid export format | 400 Unsupported | P1 |
| NEG-074 | Cancel already-completed | 400 or no-op | P1 |
| NEG-075 | Re-extract non-existent | 404 Not found | P1 |
| NEG-076 | Template with invalid regex | 400 Bad Request | P1 |
| NEG-077 | Field mapping to deleted field | 400 or ignored | P1 |
| NEG-078 | Extract during maintenance | 503 Service Unavailable | P0 |
| NEG-079 | Invalid encoding in PDF | Extraction fails | P1 |
| NEG-080 | Binary-only document | Empty text result | P1 |
| NEG-081 | Nested password in PDF | Partial or fail | P1 |
| NEG-082 | Malformed image header | 400 Bad Request | P0 |
| NEG-083 | Truncated file upload | 400 or partial | P1 |
| NEG-084 | Invalid checksum | 400 Bad Request | P0 |
| NEG-085 | Expired document link | 404 or 410 Gone | P1 |
| NEG-086 | Cross-origin doc reference | 403 Forbidden | P0 |
| NEG-087 | Invalid batch size (>max) | 400 Bad Request | P0 |
| NEG-088 | Null document content | 400 Bad Request | P0 |
| NEG-089 | Wrong MIME type | 400 Mismatch | P1 |
| NEG-090 | Circular template ref | 400 or error | P1 |

---

## §3 Boundary Tests — 90

| ID | Test | Input | Expected | Pr |
|----|------|-------|----------|----|
| BND-001 | Min file size | 1 KB | Extracts successfully | P1 |
| BND-002 | Small file | 1 MB | Extracts successfully | P1 |
| BND-003 | Medium file | 10 MB | Extracts successfully | P1 |
| BND-004 | Large file | 50 MB | Extracts or timeout | P1 |
| BND-005 | Max file size | 100 MB | Extracts or limit error | P1 |
| BND-006 | Over max file size | 101 MB | 400 or rejected | P0 |
| BND-007 | Exactly 0 bytes | 0 B | Empty or 400 | P1 |
| BND-008 | Single page | 1 page | Full extraction | P0 |
| BND-009 | 10 pages | 10 pages | All extracted | P1 |
| BND-010 | 100 pages | 100 pages | All extracted | P1 |
| BND-011 | 500 pages | 500 pages | All or timeout | P1 |
| BND-012 | Over max pages | 501 pages | Rejected or partial | P0 |
| BND-013 | Zero pages | 0 pages | Empty or error | P1 |
| BND-014 | Min DPI | 72 DPI | Low quality acceptable | P1 |
| BND-015 | Standard DPI | 150 DPI | Good extraction | P1 |
| BND-016 | High DPI | 300 DPI | High quality | P1 |
| BND-017 | Very high DPI | 600 DPI | Excellent quality | P1 |
| BND-018 | Max DPI | 1200 DPI | Best or resource limit | P1 |
| BND-019 | Below min DPI | 50 DPI | Poor or fail | P1 |
| BND-020 | Confidence 0.0 | Score 0 | No confidence | P1 |
| BND-021 | Confidence 0.5 | Score 0.5 | Medium confidence | P1 |
| BND-022 | Confidence 0.8 | Score 0.8 | High confidence | P1 |
| BND-023 | Confidence 0.95 | Score 0.95 | Very high | P1 |
| BND-024 | Confidence 1.0 | Score 1.0 | Perfect | P1 |
| BND-025 | Text length 1 char | 1 char | Extracted | P1 |
| BND-026 | Text length 1000 | 1000 chars | Extracted | P1 |
| BND-027 | Text length 10000 | 10000 chars | Extracted | P1 |
| BND-028 | Text length 100000 | 100000 chars | Extracted or truncated | P1 |
| BND-029 | Empty text | 0 chars | Empty result | P1 |
| BND-030 | Max text length | At limit | Truncated or full | P1 |
| BND-031 | Single language | 1 lang | Extracted | P1 |
| BND-032 | Two languages | 2 langs | Both extracted | P1 |
| BND-033 | Five languages | 5 langs | All extracted | P1 |
| BND-034 | Zero languages | 0 | Auto-detect or error | P1 |
| BND-035 | Batch size 1 | 1 doc | Processes | P1 |
| BND-036 | Batch size 5 | 5 docs | All processed | P1 |
| BND-037 | Batch size 10 | 10 docs | All processed | P1 |
| BND-038 | Batch size 50 | 50 docs | All or limit | P1 |
| BND-039 | Batch size 51 | 51 docs | Rejected or partial | P0 |
| BND-040 | Batch size 0 | 0 docs | 400 Bad Request | P0 |
| BND-041 | Fast extraction | <1s | Completes | P1 |
| BND-042 | Medium extraction | 1–5s | Completes | P1 |
| BND-043 | Slow extraction | 5–30s | Completes or timeout | P1 |
| BND-044 | Timeout boundary | At timeout | Timeout or complete | P1 |
| BND-045 | Field count 0 | 0 fields | Empty mapping | P1 |
| BND-046 | Field count 1 | 1 field | Mapped | P1 |
| BND-047 | Field count 10 | 10 fields | All mapped | P1 |
| BND-048 | Field count 50 | 50 fields | All or limit | P1 |
| BND-049 | Table 1×1 | 1 cell | Extracted | P1 |
| BND-050 | Table 100×100 | 10000 cells | Extracted or limit | P1 |
| BND-051 | Unicode BMP | Basic multilingual | Correct encoding | P1 |
| BND-052 | Unicode SMP | Supplementary | Correct encoding | P1 |
| BND-053 | RTL text | Arabic/Hebrew | Correct order | P1 |
| BND-054 | Emoji in text | Emoji chars | Preserved or stripped | P1 |
| BND-055 | Pagination first page | Page 1 | Correct | P1 |
| BND-056 | Pagination last page | Last page | Correct | P1 |
| BND-057 | Pagination middle | Middle page | Correct | P1 |
| BND-058 | Pagination out of range | Page 999 | Error or empty | P1 |
| BND-059 | Concurrent 2 | 2 concurrent | Both complete | P1 |
| BND-060 | Concurrent 10 | 10 concurrent | All complete | P1 |
| BND-061 | Concurrent at limit | At max | All or queue | P1 |
| BND-062 | Concurrent over limit | Over max | Rejected or queued | P0 |
| BND-063 | PDF min version | Old PDF | May work | P1 |
| BND-064 | PDF max version | New PDF | Works | P1 |
| BND-065 | Word .doc | Legacy format | Extracted | P1 |
| BND-066 | Word .docx | Modern format | Extracted | P1 |
| BND-067 | Image min dimensions | 1×1 px | Fail or empty | P1 |
| BND-068 | Image max dimensions | At limit | Extracted or limit | P1 |
| BND-069 | Filename min length | 1 char | Works | P1 |
| BND-070 | Filename max length | At limit | Works or truncated | P1 |
| BND-071 | Template 0 rules | Empty template | Default behavior | P1 |
| BND-072 | Template max rules | At limit | All applied | P1 |
| BND-073 | Extraction ID min | ID 1 | Works | P1 |
| BND-074 | Extraction ID max | Max int | Works | P1 |
| BND-075 | Timestamp epoch | 0 | Handled | P1 |
| BND-076 | Timestamp future | Future date | Handled | P1 |
| BND-077 | Decimal precision | Many decimals | Rounded or full | P1 |
| BND-078 | Currency zero | 0.00 | Extracted | P1 |
| BND-079 | Currency large | Very large | Extracted | P1 |
| BND-080 | Date format edge | Unusual format | Parsed or fail | P1 |
| BND-081 | Timezone boundary | UTC±12 | Correct | P1 |
| BND-082 | Line length 1 | 1 char line | Extracted | P1 |
| BND-083 | Line length max | At limit | Extracted | P1 |
| BND-084 | Paragraph count 0 | 0 paragraphs | Empty | P1 |
| BND-085 | Paragraph count max | At limit | All extracted | P1 |
| BND-086 | Key-value empty key | Empty key | Handled | P1 |
| BND-087 | Key-value empty value | Empty value | Handled | P1 |
| BND-088 | Nested table depth | Deep nesting | Extracted or limit | P1 |
| BND-089 | Merge cell boundary | Complex table | Correct structure | P1 |
| BND-090 | Header/footer size | Large header | Excluded correctly | P1 |

---

## §4 Functional Tests — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| FUN-001 | OCR pipeline init | Pipeline starts | P0 |
| FUN-002 | OCR pipeline preprocess | Image preprocessed | P0 |
| FUN-003 | OCR pipeline recognize | Text recognized | P0 |
| FUN-004 | OCR pipeline postprocess | Output cleaned | P0 |
| FUN-005 | OCR pipeline end-to-end | Full OCR flow | P0 |
| FUN-006 | Text extraction PDF | Text from PDF | P0 |
| FUN-007 | Text extraction Word | Text from Word | P0 |
| FUN-008 | Text extraction Excel | Text from Excel | P0 |
| FUN-009 | Text extraction image | Text from image | P0 |
| FUN-010 | Text extraction preserve order | Order preserved | P1 |
| FUN-011 | Table parsing structure | Table structure | P0 |
| FUN-012 | Table parsing cells | Cell values | P0 |
| FUN-013 | Table parsing headers | Headers detected | P1 |
| FUN-014 | Table parsing merged cells | Merged handled | P1 |
| FUN-015 | Table parsing multi-table | Multiple tables | P1 |
| FUN-016 | Entity recognition names | Names extracted | P1 |
| FUN-017 | Entity recognition orgs | Orgs extracted | P1 |
| FUN-018 | Entity recognition dates | Dates extracted | P1 |
| FUN-019 | Entity recognition amounts | Amounts extracted | P1 |
| FUN-020 | Entity recognition mixed | All entity types | P1 |
| FUN-021 | Field mapping apply | Fields mapped | P0 |
| FUN-022 | Field mapping override | Manual override | P1 |
| FUN-023 | Field mapping validation | Invalid rejected | P1 |
| FUN-024 | Field mapping partial | Partial mapping | P1 |
| FUN-025 | Field mapping template | Template-based | P1 |
| FUN-026 | Confidence calculation | Score computed | P0 |
| FUN-027 | Confidence per-field | Per-field score | P1 |
| FUN-028 | Confidence aggregate | Overall score | P1 |
| FUN-029 | Template matching exact | Exact match | P1 |
| FUN-030 | Template matching fuzzy | Fuzzy match | P1 |
| FUN-031 | Template matching regex | Regex match | P1 |
| FUN-032 | Batch orchestration | Batch runs | P0 |
| FUN-033 | Batch partial failure | Partial results | P1 |
| FUN-034 | Batch ordering | Order preserved | P1 |
| FUN-035 | Error handling invalid input | Graceful error | P0 |
| FUN-036 | Error handling service down | Retry or fail | P0 |
| FUN-037 | Retry logic transient | Retries on fail | P1 |
| FUN-038 | Retry logic max attempts | Stops at max | P1 |
| FUN-039 | Cancel processing | Cancelled | P1 |
| FUN-040 | Validation rules required | Required enforced | P1 |
| FUN-041 | Validation rules format | Format validated | P1 |
| FUN-042 | Format detection auto | Auto-detected | P0 |
| FUN-043 | Format detection explicit | Explicit respected | P1 |
| FUN-044 | Metadata parsing title | Title extracted | P1 |
| FUN-045 | Metadata parsing author | Author extracted | P1 |
| FUN-046 | Metadata parsing date | Date extracted | P1 |
| FUN-047 | Audit extraction start | Start logged | P1 |
| FUN-048 | Audit extraction complete | Complete logged | P1 |
| FUN-049 | Audit extraction fail | Fail logged | P1 |
| FUN-050 | Audit user attribution | User logged | P1 |
| FUN-051 | Re-extraction overwrite | Previous overwritten | P1 |
| FUN-052 | Re-extraction history | History preserved | P1 |
| FUN-053 | Manual correction save | Corrections saved | P1 |
| FUN-054 | Manual correction validation | Validated on save | P1 |
| FUN-055 | Export CSV format | CSV correct | P1 |
| FUN-056 | Export JSON format | JSON correct | P1 |
| FUN-057 | Preview before commit | Preview shown | P1 |
| FUN-058 | Preview diff | Diff shown | P2 |
| FUN-059 | Supported formats list | List returned | P1 |
| FUN-060 | Header/footer exclusion | Excluded from body | P1 |
| FUN-061 | Watermark exclusion | Excluded | P1 |
| FUN-062 | Paragraph structure | Paragraphs preserved | P1 |
| FUN-063 | Page break handling | Breaks handled | P1 |
| FUN-064 | Column layout | Columns detected | P1 |
| FUN-065 | List detection | Lists detected | P1 |
| FUN-066 | Footnote handling | Footnotes handled | P1 |
| FUN-067 | Hyperlink extraction | Links extracted | P1 |
| FUN-068 | Image alt text | Alt text used | P1 |
| FUN-069 | Multi-column table | Multi-col parsed | P1 |
| FUN-070 | Nested list | Nested parsed | P1 |
| FUN-071 | Abbreviation expansion | Expansion optional | P2 |
| FUN-072 | Unit conversion | Units handled | P2 |
| FUN-073 | Currency normalization | Normalized | P1 |
| FUN-074 | Date normalization | Normalized | P1 |
| FUN-075 | Number formatting | Format preserved | P1 |
| FUN-076 | Whitespace normalization | Normalized | P1 |
| FUN-077 | Encoding detection | Encoding detected | P1 |
| FUN-078 | Language detection | Language detected | P1 |
| FUN-079 | Duplicate detection | Duplicates handled | P1 |
| FUN-080 | Redaction handling | Redacted excluded | P2 |
| FUN-081 | Version comparison | Version diff | P2 |
| FUN-082 | Template priority | Priority applied | P1 |
| FUN-083 | Template fallback | Fallback used | P1 |
| FUN-084 | Field type coercion | Type coerced | P1 |
| FUN-085 | Null field handling | Nulls handled | P1 |
| FUN-086 | Empty string handling | Empties handled | P1 |
| FUN-087 | Trim whitespace | Trimmed | P1 |
| FUN-088 | Case sensitivity | Case preserved | P1 |
| FUN-089 | Sort order | Order preserved | P1 |
| FUN-090 | Pagination extraction | Paginated result | P1 |

---

## §5 Integration Tests — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| INT-001 | AI service extract | AI returns text | P0 |
| INT-002 | AI service timeout | Timeout handled | P0 |
| INT-003 | AI service fallback | Fallback works | P1 |
| INT-004 | AI service auth | Auth passed | P0 |
| INT-005 | AI service rate limit | Rate limit handled | P1 |
| INT-006 | Storage upload doc | Doc stored | P0 |
| INT-007 | Storage download doc | Doc retrieved | P0 |
| INT-008 | Storage delete doc | Doc deleted | P1 |
| INT-009 | Storage quota | Quota checked | P1 |
| INT-010 | Storage path | Path correct | P1 |
| INT-011 | Opportunity fields update | Fields updated | P0 |
| INT-012 | Opportunity fields validation | Validation applied | P1 |
| INT-013 | Opportunity fields conflict | Conflict handled | P1 |
| INT-014 | Opportunity fields audit | Audit trail | P1 |
| INT-015 | Opportunity fields permissions | Permissions checked | P0 |
| INT-016 | Document service get | Doc retrieved | P0 |
| INT-017 | Document service create | Doc created | P0 |
| INT-018 | Document service update | Doc updated | P1 |
| INT-019 | Document service delete | Doc deleted | P1 |
| INT-020 | Document service list | List returned | P1 |
| INT-021 | Search index update | Index updated | P1 |
| INT-022 | Search index query | Search works | P1 |
| INT-023 | Search index delete | Removed from index | P1 |
| INT-024 | Search index partial | Partial update | P1 |
| INT-025 | Search index refresh | Refresh works | P1 |
| INT-026 | Export CSV | CSV exported | P1 |
| INT-027 | Export JSON | JSON exported | P1 |
| INT-028 | Export permissions | Permissions checked | P1 |
| INT-029 | Export large file | Large export works | P1 |
| INT-030 | Export format validation | Format validation | P1 |
| INT-031 | Notification on complete | Notification sent | P1 |
| INT-032 | Notification on fail | Fail notification | P1 |
| INT-033 | Notification batch | Batch notification | P1 |
| INT-034 | Notification preferences | Prefs respected | P1 |
| INT-035 | Notification channels | Channel correct | P1 |
| INT-036 | Batch job queue | Job queued | P0 |
| INT-037 | Batch job status | Status updated | P1 |
| INT-038 | Batch job cancel | Job cancelled | P1 |
| INT-039 | Batch job retry | Retry works | P1 |
| INT-040 | Batch job priority | Priority applied | P1 |
| INT-041 | Template engine load | Template loaded | P1 |
| INT-042 | Template engine apply | Template applied | P1 |
| INT-043 | Template engine validate | Validation works | P1 |
| INT-044 | Template engine cache | Cache used | P1 |
| INT-045 | Template engine version | Version correct | P1 |
| INT-046 | OCR engine init | Engine initialized | P0 |
| INT-047 | OCR engine process | Processing works | P0 |
| INT-048 | OCR engine config | Config applied | P1 |
| INT-049 | OCR engine fallback | Fallback works | P1 |
| INT-050 | OCR engine language | Language set | P1 |
| INT-051 | Auth service token | Token validated | P0 |
| INT-052 | Auth service permissions | Permissions checked | P0 |
| INT-053 | Auth service user | User resolved | P0 |
| INT-054 | Permission service check | Check works | P0 |
| INT-055 | Permission service cache | Cache used | P1 |
| INT-056 | Audit service log | Log written | P1 |
| INT-057 | Audit service query | Query works | P1 |
| INT-058 | Audit service retention | Retention applied | P1 |
| INT-059 | Config service settings | Settings loaded | P1 |
| INT-060 | Config service override | Override works | P1 |
| INT-061 | Opportunity + Document | Full flow | P0 |
| INT-062 | Document + Template | Template applied | P1 |
| INT-063 | Extracted + Opportunity | Fields updated | P0 |
| INT-064 | Batch + AI | Batch to AI | P1 |
| INT-065 | Extracted + Export | Export flow | P1 |
| INT-066 | Extracted + Search | Search indexed | P1 |
| INT-067 | Extracted + Notification | Notified | P1 |
| INT-068 | Template + Field mapping | Mapping applied | P1 |
| INT-069 | OCR + AI | OCR then AI | P1 |
| INT-070 | Storage + Document | Stored + linked | P1 |
| INT-071 | Multi-opportunity | Cross-opp isolation | P0 |
| INT-072 | Multi-user | User isolation | P0 |
| INT-073 | Multi-tenant | Tenant isolation | P0 |
| INT-074 | Extracted + Audit | Audit trail | P1 |
| INT-075 | Extracted + History | History saved | P1 |
| INT-076 | Re-extract + Overwrite | Overwrite works | P1 |
| INT-077 | Cancel + Cleanup | Cleanup on cancel | P1 |
| INT-078 | Retry + State | State preserved | P1 |
| INT-079 | Timeout + Partial | Partial result | P1 |
| INT-080 | Error + Recovery | Recovery works | P1 |
| INT-081 | Database + Extraction | DB persistence | P0 |
| INT-082 | Cache + Extraction | Cache invalidation | P1 |
| INT-083 | Queue + Extraction | Queue processing | P1 |
| INT-084 | API + Client | API contract | P0 |
| INT-085 | Webhook + Extraction | Webhook triggered | P1 |
| INT-086 | File system + Storage | FS integration | P1 |
| INT-087 | Blob + Extraction | Blob storage | P1 |
| INT-088 | CDN + Document | CDN delivery | P1 |
| INT-089 | Logging + Extraction | Logs written | P1 |
| INT-090 | Metrics + Extraction | Metrics recorded | P1 |

---

## §7 Concurrency — 25 | §8 Unit — 21 | §9 Performance — 16 | §10 Load — 10

**§7 (25):** Concurrent extractions (2/5/10/20), cancel during extraction, batch + single overlap, re-extract during extract, modify doc during extract, concurrent template updates, same-doc concurrent, cross-opportunity concurrent, queue contention, lock timeout, deadlock recovery, optimistic concurrency, extraction status race, batch ordering under load, resource pool exhaustion, DbContext concurrency, file handle contention, memory pressure concurrent, retry under concurrent, idempotency, duplicate submission, concurrent export, concurrent cancel, batch reorder, extraction lock.

**§8 (21):** Text parsing (5), confidence calc (3), format detection (5), field mapping (5), template matching (3).

**§9 (16):** 1-page (<1s), 10-page (<5s), 100-page (<30s), OCR (<5s/page), batch 10 (<60s), search (<500ms), memory, concurrent, cold start, warm cache, large file, many fields, complex table, multi-table, batch 50, export large.

**§10 (10):** 20 concurrent extractions, spike load, sustained load, large files, recovery after failure, queue backlog, memory under load, CPU under load, network latency, mixed workload.

---

**Status:** Ready for Execution
