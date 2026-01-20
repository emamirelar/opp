# TextExtractionService - Unit Test Cases

**Manager**: `TextExtractionService`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/TextExtractionService.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Text extraction from documents, OCR, Format detection, Content parsing

**Total Test Cases**: 12+

---

## Test Categories

### 1. Document Extraction (5 tests)
- TC-TE-001: Extract from PDF
- TC-TE-002: Extract from DOCX
- TC-TE-003: Extract from XLSX
- TC-TE-004: Extract from HTML
- TC-TE-005: Extract from plain text

### 2. OCR Operations (3 tests)
- TC-TE-006: OCR image file
- TC-TE-007: OCR scanned PDF
- TC-TE-008: Handle poor quality image

### 3. Content Processing (2 tests)
- TC-TE-009: Clean extracted text
- TC-TE-010: Preserve formatting

### 4. Error Handling (2 tests)
- TC-TE-011: Handle unsupported format
- TC-TE-012: Handle corrupted file

**Coverage**: 70%+

