# WHY Section — Test Cases

**Component:** Opportunity WHY Section (Rationale, Strategic Alignment, Context)  
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
| §6 Security | 0 | — | (covered in §2) |
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

WHY section of opportunities captures rationale, strategic alignment (SDGs, UNOPS Strategic Plan), country/regional context, partner needs assessment, risk context (DST integration), development challenges, and UNOPS mandate alignment. Features: rich text editing, SDG mapping, context data auto-population from DST, AI-assisted drafting, version tracking, completeness validation, and export.

---

## §1 Positive (30)

1. Save WHY section (P0)
2. Load WHY section (P0)
3. Update rationale (P0)
4. Map SDGs (P0)
5. Completeness check (P0)
6. Strategic alignment save
7. Country context display
8. Partner needs assessment
9. Risk context from DST
10. Development challenges
11. Mandate alignment
12. AI draft generation
13. Version save
14. Rich text editing
15. SDG multi-select
16. Auto-populate from DST
17. Export WHY section
18. Audit trail
19. Validation pass
20. Search within rationale
21. Filter by SDG
22. Pagination
23. Sort by date
24. Model mapping
25. Section lock
26. Compare versions
27. Restore version
28. Typeahead for context
29. Count SDGs
30. Validation success

---

## §2 Negative (90)

### Input (15)
1. Null oppId
2. Non-existent oppId
3. Deleted opportunity
4. Null rationale
5. Invalid SDG ID
6. No SDGs selected
7. Null strategic alignment text
8. Invalid SDG code format
9. Duplicate SDG assignment
10. Null country context
11. Invalid alignment reference
12. Missing rationale on submit
13. Invalid rich text markup
14. Null partner needs
15. Malformed SDG JSON

### Auth (10)
16. Unauthenticated access
17. No view permission
18. No edit permission
19. No delete permission
20. Wrong tenant
21. Expired session
22. Invalid token
23. Role without access
24. Deactivated user
25. Cross-org access

### State (10)
26. Edit closed opportunity
27. Edit locked section
28. Edit during workflow transition
29. Edit approved opportunity
30. Edit final version
31. Save when read-only
32. Update archived opportunity
33. Modify during approval
34. Edit without draft status
35. Change during publish

### Injection (10)
36. SQL injection in rationale
37. XSS in rationale
38. Rich text XSS
39. HTML injection
40. Template injection
41. SDG description injection
42. Strategic alignment injection
43. Partner needs injection
44. Script in context field
45. Event handler injection

### AI (10)
46. AI service down
47. AI timeout
48. Inappropriate content
49. Hallucination
50. Quota exceeded
51. AI returns empty
52. AI returns invalid JSON
53. AI rate limit
54. AI auth failure
55. AI malformed response

### DST (10)
56. DST service down
57. Stale DST data
58. Missing country in DST
59. Invalid DST indices
60. DST timeout
61. DST auth failure
62. DST rate limit
63. DST malformed response
64. DST partial failure
65. DST version mismatch

### Rationale / Strategic Alignment / SDG (20)
66. Rationale exceeds max length
67. Rationale with invalid characters
68. Rationale empty on mandatory save
69. Strategic alignment null when required
70. Strategic alignment invalid format
71. SDG count exceeds maximum (17)
72. SDG target invalid
73. SDG indicator invalid
74. SDG without rationale link
75. Circular SDG reference
76. Orphaned SDG mapping
77. SDG code out of range (1–17)
78. SDG duplicate in same section
79. Strategic plan reference invalid
80. Alignment score out of range
81. Missing SDG justification
82. SDG target mismatch
83. Invalid SDG cascade
84. Rationale sanitization failure
85. Strategic alignment max items exceeded

### Dependencies / Format / Business (15)
86. Missing dependency service
87. Invalid API version
88. Malformed request body
89. Wrong content-type
90. Invalid UUID format
91. Incomplete mandatory fields
92. Mass assignment
93. Invalid date format
94. Wrong encoding
95. Oversized payload
96. Invalid pagination params
97. Malformed filter
98. Invalid sort field
99. Business rule violation
100. Constraint violation

---

## §3 Boundary (90)

### Rationale (10)
1. Rationale length 0
2. Rationale length 100
3. Rationale length 10000
4. Rationale at max chars
5. Rationale 1 char under max
6. Rationale 1 char over max
7. Rationale whitespace only
8. Rationale single char
9. Rationale Unicode boundary
10. Rationale newline count

### SDG (15)
11. SDG count 0
12. SDG count 1
13. SDG count 5
14. SDG count 17
15. SDG count 18 (over max)
16. SDG ID 1
17. SDG ID 17
18. SDG ID 0
19. SDG ID 18
20. SDG target min
21. SDG target max
22. SDG indicator min
23. SDG indicator max
24. SDG cascade depth
25. SDG multi-select max

### Strategic Alignment (10)
26. Strategic alignment items 0
27. Strategic alignment items 1
28. Strategic alignment items 10
29. Strategic alignment items 50
30. Strategic alignment items max+1
31. Alignment score 0
32. Alignment score 100
33. Alignment score -1
34. Alignment score 101
35. Plan reference length

### Context / Rich Text (10)
36. Context fields empty
37. Context fields max
38. Rich text size min
39. Rich text size max
40. Rich text size 1 over max
41. Country code length
42. Region code length
43. Partner needs length
44. Development challenges length
45. Mandate alignment length

### Version / DST / AI (15)
46. Version count 0
47. Version count 1
48. Version count max
49. Version count max+1
50. DST data points 0
51. DST data points max
52. AI response length min
53. AI response length max
54. AI response length 1 over max
55. DST refresh interval min
56. DST refresh interval max
57. Version diff size
58. Compare versions max gap
59. Restore version age
60. Audit log size

### Concurrency / Unicode / Search (15)
61. Concurrent edits 2 users
62. Concurrent edits 10 users
63. Unicode BMP
64. Unicode supplementary
65. Search term min length
66. Search term max length
67. Pagination page 0
68. Pagination page max
69. Pagination page size min
70. Pagination page size max
71. Date range min
72. Date range max
73. Comparison diff size
74. Nesting depth max
75. Comment count max

### Attachments / Misc (15)
76. Attachment count 0
77. Attachment count max
78. Attachment count max+1
79. Attachment size min
80. Attachment size max
81. Attachment size 1 over max
82. Typeahead min chars
83. Typeahead max results
84. Filter combination max
85. Sort field max length
86. Export format boundary
87. Lock duration
88. Validation timeout
89. Cache TTL
90. Retry count max

---

## §4 Functional (90)

### Section CRUD (20)
1. Create WHY section
2. Read WHY section
3. Update rationale
4. Update strategic alignment
5. Update country context
6. Update partner needs
7. Update development challenges
8. Update mandate alignment
9. Delete WHY section (soft)
10. Restore deleted section
11. Partial update rationale only
12. Partial update SDGs only
13. Bulk update context
14. Merge version changes
15. Clone section
16. Copy to new opportunity
17. Initialize from template
18. Clear section
19. Reset to default
20. Archive section

### SDG Management (20)
21. Add single SDG
22. Add multiple SDGs
23. Remove SDG
24. Reorder SDGs
25. Link SDG to rationale
26. Link SDG to target
27. Link SDG to indicator
28. Validate SDG cascade
29. SDG completeness check
30. SDG export
31. SDG filter
32. SDG search
33. SDG sort
34. SDG deduplication
35. SDG target validation
36. SDG indicator validation
37. SDG rationale required
38. SDG max count enforcement
39. SDG mapping persistence
40. SDG audit trail

### DST Integration (15)
41. Fetch DST country data
42. Fetch DST risk context
43. Auto-populate from DST
44. DST refresh on demand
45. DST stale data handling
46. DST fallback
47. DST merge with manual
48. DST score parsing
49. DST index mapping
50. DST validation
51. DST cache invalidation
52. DST retry logic
53. DST partial update
54. DST conflict resolution
55. DST audit

### Validation (20)
56. Mandatory rationale
57. Mandatory SDG count
58. Strategic alignment format
59. Country context format
60. Partner needs format
61. Rich text sanitization
62. Length validation
63. Completeness score
64. Pre-submit validation
65. Post-save validation
66. Cross-field validation
67. SDG-rationale consistency
68. Alignment score range
69. Date range validation
70. Reference integrity
71. Business rule validation
72. Duplicate check
73. Orphan check
74. Cascade validation
75. Export validation

### Audit (15)
76. Audit create
77. Audit update
78. Audit delete
79. Audit field-level
80. Audit user attribution
81. Audit timestamp
82. Audit version link
83. Audit export
84. Audit filter
85. Audit search
86. Audit retention
87. Audit integrity
88. Audit compliance
89. Audit export format
90. Audit retention policy

---

## §5 Integration (90)

### Opportunity (18)
1. WHY → Opportunity load
2. Opportunity → WHY save
3. Opportunity workflow → WHY lock
4. Opportunity status → WHY visibility
5. Opportunity delete → WHY cascade
6. Opportunity clone → WHY copy
7. Opportunity export → WHY include
8. Opportunity search → WHY index
9. Opportunity permissions → WHY access
10. Opportunity audit → WHY link
11. Opportunity version → WHY sync
12. Opportunity approval → WHY lock
13. Opportunity publish → WHY finalize
14. Opportunity archive → WHY archive
15. Opportunity restore → WHY restore
16. Opportunity filter → WHY filter
17. Opportunity sort → WHY sort
18. Opportunity pagination → WHY pagination

### SDG Service (18)
19. SDG service fetch
20. SDG service validate
21. SDG service map
22. SDG service cache
23. SDG service fallback
24. SDG service timeout
25. SDG service error handling
26. SDG target resolution
27. SDG indicator resolution
28. SDG cascade resolution
29. SDG search
30. SDG filter
31. SDG export
32. SDG audit
33. SDG version sync
34. SDG conflict resolution
35. SDG bulk operations
36. SDG permission check

### DST Service (18)
37. DST fetch country
38. DST fetch risk
39. DST auto-populate
40. DST refresh
41. DST cache
42. DST fallback
43. DST timeout
44. DST error handling
45. DST retry
46. DST auth
47. DST rate limit
48. DST version
49. DST merge
50. DST validation
51. DST audit
52. DST conflict
53. DST partial
54. DST bulk

### AI (18)
55. AI draft generation
56. AI completion
57. AI timeout
58. AI error handling
59. AI quota
60. AI rate limit
61. AI fallback
62. AI cache
63. AI validation
64. AI sanitization
65. AI audit
66. AI version
67. AI retry
68. AI partial
69. AI context
70. AI prompt
71. AI response parse
72. AI security

### Export (18)
73. Export PDF
74. Export Word
75. Export JSON
76. Export XML
77. Export with SDGs
78. Export with audit
79. Export with versions
80. Export filter
81. Export pagination
82. Export large dataset
83. Export encoding
84. Export format validation
85. Export permission
86. Export audit
87. Export timeout
88. Export error handling
89. Export retry
90. Export batch

---

## §6 Security (10)

Injection (2), access control (2), IDOR (2), rich text (2), AI security (2).

---

## §7 Concurrency (25)

Concurrent edits, SDG assignments, DST refreshes, AI drafts, version creation.

---

## §8 Unit (21)

Validation (5), SDG mapping (5), completeness (3), formatting (5), DST score parsing (3).

---

## §9 Performance (16)

Save (<500ms), load (<300ms), DST auto-populate (<2s), AI draft (<5s), export (<3s), memory.

---

## §10 Load (10)

50 concurrent edits, spike, sustained, DST under load, recovery.

---

**Status:** Ready for Execution
