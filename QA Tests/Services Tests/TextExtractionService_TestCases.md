# TextExtractionService Test Cases

**Service**: `UNOPS.PAO.UNOPSBusiness/Managers/TextExtractionService.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 20  

---

## Overview

The TextExtractionService extracts text from documents:
- PDF text extraction
- Image OCR (Optical Character Recognition)
- Word document parsing
- Multi-language support

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| PDF Extraction | 6 | P0 |
| Image OCR | 5 | P1 |
| Document Types | 5 | P1 |
| Error Handling | 4 | P1 |

---

## P0 - Critical Tests

### TC-TE-001: Extract text from PDF
**Description**: Extract all text from PDF  
**Test Steps**:
1. Provide PDF file
2. Call `ExtractTextAsync(pdfBytes)`
**Expected Result**: Extracted text

### TC-TE-002: Extract from multi-page PDF
**Description**: Handle multi-page PDFs  
**Test Steps**:
1. Provide 10-page PDF
2. Extract text
**Expected Result**: All pages extracted

### TC-TE-003: Extract from PDF with images
**Description**: PDF with embedded images  
**Test Steps**:
1. Provide PDF with images
2. Extract text
**Expected Result**: Text extracted, images noted

### TC-TE-004: Extract from encrypted PDF
**Description**: Handle encrypted PDFs  
**Test Steps**:
1. Provide password-protected PDF
**Expected Result**: Error or request password

### TC-TE-005: Extract from empty PDF
**Description**: Handle empty PDFs  
**Test Steps**:
1. Provide empty PDF
**Expected Result**: Empty string, no error

### TC-TE-006: Extract from corrupt PDF
**Description**: Handle corrupt files  
**Test Steps**:
1. Provide corrupt PDF
**Expected Result**: Clear error message

---

## P1 - High Priority Tests

### TC-TE-007: OCR from image
**Description**: Extract text from image  
**Test Steps**:
1. Provide image with text
2. Call `ExtractFromImageAsync(imageBytes)`
**Expected Result**: Extracted text

### TC-TE-008: OCR from scanned PDF
**Description**: OCR on scanned PDF  
**Test Steps**:
1. Provide scanned PDF (image-based)
2. Extract with OCR enabled
**Expected Result**: Text from OCR

### TC-TE-009: OCR with language hint
**Description**: Specify language for OCR  
**Test Steps**:
1. Specify language = "fr"
**Expected Result**: Better French recognition

### TC-TE-010: OCR from low quality image
**Description**: Handle poor quality  
**Test Steps**:
1. Provide low-resolution image
**Expected Result**: Best effort extraction

### TC-TE-011: OCR multiple languages
**Description**: Document with multiple languages  
**Test Steps**:
1. Provide multi-language document
**Expected Result**: All languages extracted

### TC-TE-012: Extract from Word document
**Description**: Extract from .docx  
**Test Steps**:
1. Provide Word document
2. Call extraction
**Expected Result**: Text extracted

### TC-TE-013: Extract from Excel
**Description**: Extract from .xlsx  
**Test Steps**:
1. Provide Excel file
2. Extract text
**Expected Result**: Cell contents extracted

### TC-TE-014: Extract from PowerPoint
**Description**: Extract from .pptx  
**Test Steps**:
1. Provide PowerPoint
**Expected Result**: Slide text extracted

### TC-TE-015: Extract from plain text
**Description**: Handle .txt files  
**Test Steps**:
1. Provide text file
**Expected Result**: Content returned

### TC-TE-016: Unsupported format
**Description**: Handle unsupported files  
**Test Steps**:
1. Provide .exe file
**Expected Result**: Unsupported format error

---

## Performance Tests

### TC-TE-P001: PDF 100 pages < 30s
**Performance Criteria**: < 30 seconds

### TC-TE-P002: Image OCR < 5s
**Performance Criteria**: < 5 seconds per image

### TC-TE-P003: Large Word doc < 10s
**Performance Criteria**: < 10 seconds for 100 pages

### TC-TE-P004: Concurrent extractions
**Performance Criteria**: 5 concurrent < 60s

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/TextExtractionServiceTests.cs`

