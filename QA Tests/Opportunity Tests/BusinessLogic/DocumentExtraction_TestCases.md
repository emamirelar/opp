# DocumentExtraction Test Cases

**Component:** AI Document Extraction Business Logic  
**Test Count:** 30+  
**Priority:** P1-P2 (High/Medium)  
**Created:** January 13, 2026

---

## Overview

Test cases for AI-powered document upload, analysis, structured data extraction, field mapping, and verification workflows.

---

## Test Categories

| Category | Test Count | Priority |
|----------|------------|----------|
| Document Upload | 6 | P1 |
| AI Extraction | 10 | P1 |
| Field Mapping | 8 | P1 |
| Verification | 6 | P1 |

---

## 1. Document Upload (P1)

### TC-OPP-DOC-UP-001: Upload PDF Concept Note
**Priority:** P1  
**Test Steps:**
1. Upload PDF concept note
2. Verify file accepted and stored
3. Trigger extraction

**Expected Results:**
- File validated (PDF format, < 10MB)
- Stored in document repository
- Extraction job queued
- User notified of processing

---

### TC-OPP-DOC-UP-002: Upload Word Document
**Priority:** P1  
**Test Steps:**
1. Upload .docx file
2. Convert and extract

**Expected Results:**
- .docx accepted
- Converted to PDF internally
- Text extraction successful
- Formatting preserved where possible

---

### TC-OPP-DOC-UP-003: Upload Multiple Documents
**Priority:** P1  
**Test Steps:**
1. Upload concept note, budget, schedule
2. Process all documents
3. Merge extracted data

**Expected Results:**
- All documents processed
- Data merged intelligently
- Conflicts flagged for resolution
- Comprehensive dataset created

---

### TC-OPP-DOC-UP-004: Invalid File Type Rejection
**Priority:** P1  
**Test Steps:**
1. Attempt to upload .exe file
2. Verify rejection

**Expected Results:**
- File type validation
- Clear error message
- Allowed types listed
- No security risk

---

### TC-OPP-DOC-UP-005: Oversized File Handling
**Priority:** P2  
**Test Steps:**
1. Upload 50MB PDF
2. Verify size limit enforced

**Expected Results:**
- Size limit: 10MB
- Rejection with clear message
- Suggestion to compress or split
- Alternative upload method offered

---

### TC-OPP-DOC-UP-006: OCR for Scanned Documents
**Priority:** P2  
**Test Steps:**
1. Upload scanned PDF (image-based)
2. Perform OCR
3. Extract text

**Expected Results:**
- OCR triggered automatically
- Text extracted with confidence scores
- Low-confidence areas flagged
- Manual review prompted

---

## 2. AI Extraction (P1)

### TC-OPP-DOC-EXT-001: Extract Opportunity Title
**Priority:** P1  
**Test Steps:**
1. Document contains: "Project Title: Clean Water Initiative"
2. AI extracts title

**Expected Results:**
- Field: OpportunityName
- Value: "Clean Water Initiative"
- Confidence: >90%
- Source page/paragraph noted

---

### TC-OPP-DOC-EXT-002: Extract Budget Information
**Priority:** P1  
**Test Steps:**
1. Document states: "Total budget: USD 2.5 million"
2. Extract budget

**Expected Results:**
- Field: EstimatedValue
- Value: 2,500,000
- Currency: USD
- Confidence: >85%

---

### TC-OPP-DOC-EXT-003: Extract Timeline/Dates
**Priority:** P1  
**Test Steps:**
1. Document: "Implementation: Jan 2027 - Dec 2028"
2. Extract dates

**Expected Results:**
- StartDate: 2027-01-01
- EndDate: 2028-12-31
- Duration: 24 months
- Confidence: >85%

---

### TC-OPP-DOC-EXT-004: Extract Geographic Information
**Priority:** P1  
**Test Steps:**
1. Document mentions: "Bangladesh, Nepal, and Myanmar"
2. Extract countries

**Expected Results:**
- Countries: [BD, NP, MM]
- Mapped to country IDs
- Primary country inferred if possible
- Multi-country flag set

---

### TC-OPP-DOC-EXT-005: Extract Partner Information
**Priority:** P1  
**Test Steps:**
1. Document lists: "In partnership with World Bank"
2. Extract partner

**Expected Results:**
- Partner name extracted
- Matched to existing partner if possible
- New partner flagged for creation
- Relationship type inferred

---

### TC-OPP-DOC-EXT-006: Extract Deliverables
**Priority:** P1  
**Test Steps:**
1. Document lists key deliverables
2. Extract as structured list

**Expected Results:**
- Deliverables extracted as list
- Each with name and description
- Quantities if mentioned
- Linked to WBS if available

---

### TC-OPP-DOC-EXT-007: Extract SDG References
**Priority:** P1  
**Test Steps:**
1. Document mentions "SDG 6" and "clean water"
2. Extract SDG alignment

**Expected Results:**
- SDG 6 identified explicitly
- Additional SDGs inferred from content
- Confidence scores per SDG
- Justification provided

---

### TC-OPP-DOC-EXT-008: Extract Risk Mentions
**Priority:** P1  
**Test Steps:**
1. Document discusses implementation risks
2. Extract as risks

**Expected Results:**
- Risks extracted as list
- Categorized (operational, financial, etc.)
- Severity estimated
- Added to draft risk register

---

### TC-OPP-DOC-EXT-009: Handle Ambiguous Information
**Priority:** P1  
**Test Steps:**
1. Document vague: "Budget approximately 2-3 million"
2. Extract with uncertainty

**Expected Results:**
- Range captured: 2M-3M
- Midpoint used as estimate
- Uncertainty flagged
- Manual verification requested

---

### TC-OPP-DOC-EXT-010: Multi-Language Document Support
**Priority:** P2  
**Test Steps:**
1. Upload French concept note
2. Translate and extract

**Expected Results:**
- Language detected
- Translation to English
- Extraction from translated text
- Original preserved

---

## 3. Field Mapping (P1)

### TC-OPP-DOC-MAP-001: Map Extracted Data to Opportunity Fields
**Priority:** P1  
**Test Steps:**
1. Extraction complete
2. Map to opportunity schema

**Expected Results:**
```csharp
Mapping:
  "Project Title" → Opportunity.Name
  "Total Budget" → Opportunity.EstimatedValue
  "Start Date" → Opportunity.StartDate
  "Countries" → Opportunity.Countries
  "Partners" → Opportunity.Partners
  "Deliverables" → Opportunity.Deliverables
```

---

### TC-OPP-DOC-MAP-002: Handle Missing Fields
**Priority:** P1  
**Test Steps:**
1. Document missing budget information
2. Flag for manual entry

**Expected Results:**
- Missing fields identified
- Marked as "Not Found"
- User prompted to enter manually
- AI suggests similar field value

---

### TC-OPP-DOC-MAP-003: Resolve Field Conflicts
**Priority:** P1  
**Test Steps:**
1. Multiple documents with different budgets
2. Resolve conflict

**Expected Results:**
- All values shown with sources
- Most recent/authoritative selected
- User can choose which to use
- Conflict resolution logged

---

### TC-OPP-DOC-MAP-004: Validate Extracted Data Types
**Priority:** P1  
**Test Steps:**
1. Extracted value doesn't match field type
2. Verify validation catches

**Expected Results:**
- Type checking enforced
- Invalid values flagged
- Conversion attempted if possible
- User correction requested

---

### TC-OPP-DOC-MAP-005: Confidence Threshold Filtering
**Priority:** P1  
**Test Steps:**
1. Low confidence extraction (<70%)
2. Require manual verification

**Expected Results:**
- Low confidence values highlighted
- Not auto-populated
- Shown as suggestions only
- User confirms or corrects

---

### TC-OPP-DOC-MAP-006: Partial Field Matching
**Priority:** P2  
**Test Steps:**
1. Document uses non-standard terminology
2. Match to standard fields

**Expected Results:**
- Fuzzy matching applied
- "Total cost" → EstimatedValue
- "Timeline" → StartDate/EndDate
- Confidence adjusted for match quality

---

### TC-OPP-DOC-MAP-007: Nested Field Extraction
**Priority:** P2  
**Test Steps:**
1. Extract complex nested structures
2. Map to related entities

**Expected Results:**
- Budget breakdown → BudgetLines
- Deliverables with sub-tasks → WBS
- Partner roles → PartnerAssignments
- Relationships preserved

---

### TC-OPP-DOC-MAP-008: Custom Field Mapping
**Priority:** P2  
**Test Steps:**
1. User defines custom field mappings
2. Apply to extraction

**Expected Results:**
- Custom mappings saved
- Applied to future extractions
- Can override defaults
- Organization-specific logic

---

## 4. Verification (P1)

### TC-OPP-DOC-VER-001: User Review Interface
**Priority:** P1  
**Test Steps:**
1. Extraction complete
2. Present to user for review

**Expected Results:**
- Side-by-side: document vs extracted data
- Color coding: Green (high conf), Yellow (medium), Red (low)
- Click to edit values
- Mark as verified when complete

---

### TC-OPP-DOC-VER-002: Accept Extracted Value
**Priority:** P1  
**Test Steps:**
1. User reviews and accepts suggestion
2. Value committed to opportunity

**Expected Results:**
- Value saved
- Marked as "Verified"
- Timestamp and user recorded
- Confidence score preserved

---

### TC-OPP-DOC-VER-003: Correct Extracted Value
**Priority:** P1  
**Test Steps:**
1. AI extracted incorrect value
2. User corrects

**Expected Results:**
- Correction saved
- Original extraction logged
- AI feedback loop (learning)
- Audit trail maintained

---

### TC-OPP-DOC-VER-004: Reject Extraction
**Priority:** P1  
**Test Steps:**
1. Extracted value completely wrong
2. User rejects and enters manually

**Expected Results:**
- Extraction marked "Rejected"
- User-entered value used
- AI learns from rejection
- Pattern analysis for improvement

---

### TC-OPP-DOC-VER-005: Bulk Verification
**Priority:** P1  
**Test Steps:**
1. Accept multiple fields at once
2. Verify batch operation

**Expected Results:**
- Select multiple fields
- "Accept all high confidence" option
- Batch save operation
- Individual audit trails

---

### TC-OPP-DOC-VER-006: Re-Extract After Document Update
**Priority:** P2  
**Test Steps:**
1. Updated document uploaded
2. Re-run extraction
3. Show changes

**Expected Results:**
- Delta extraction (only changes)
- Highlight what changed
- Preserve verified fields
- Update only new/modified data

---

## 5. Integration Tests

### TC-OPP-DOC-INT-001: End-to-End Extraction Flow
**Priority:** P1  
**Test Steps:**
1. Upload document
2. AI extraction
3. Mapping
4. Verification
5. Create opportunity

**Expected Results:**
- Complete flow in <5 minutes
- Opportunity created with 80%+ fields populated
- Only requires verification, not full data entry
- Significant time savings

---

### TC-OPP-DOC-INT-002: Multiple Document Consolidation
**Priority:** P1  
**Test Steps:**
1. Concept note has narrative
2. Budget spreadsheet has financials
3. Timeline doc has schedule
4. Merge all

**Expected Results:**
- Best data from each source
- No duplication
- Conflicts resolved
- Comprehensive opportunity profile

---

### TC-OPP-DOC-INT-003: Extraction Performance
**Priority:** P2  
**Test Steps:**
1. 10-page document
2. Measure extraction time

**Expected Results:**
- Extraction completes in <60 seconds
- Progress indicator shown
- Can continue other work
- Notification when complete

---

## Summary

**Total Test Cases:** 30+  
**High (P1):** 24  
**Medium (P2):** 8

**Execution Time:** ~8-10 minutes  
**Dependencies:** Gemini AI, Document storage, Opportunity entity

---

**Last Updated:** January 13, 2026  
**C# Test Class:** `DocumentExtractionTests.cs`  
**Status:** ✅ Ready for Implementation
