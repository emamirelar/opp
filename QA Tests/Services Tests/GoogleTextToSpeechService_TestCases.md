# GoogleTextToSpeechService Test Cases

**Service**: `UNOPS.PAO.UNOPSBusiness/Managers/GoogleTextToSpeechService.cs`  
**Priority**: P2 - Medium  
**Total Test Cases**: 15  

---

## Overview

The GoogleTextToSpeechService converts text to audio:
- Text-to-speech conversion
- Voice selection
- Audio format options
- Language support

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Speech Generation | 6 | P0 |
| Voice Options | 4 | P1 |
| Error Handling | 3 | P1 |
| Performance | 2 | P2 |

---

## P0 - Critical Tests

### TC-TTS-001: Convert text to speech
**Description**: Basic TTS conversion  
**Test Steps**:
1. Call `SynthesizeSpeechAsync(text, voice, format)`
2. Verify audio returned
**Expected Result**: Audio bytes returned

### TC-TTS-002: Convert with default voice
**Description**: Use default voice  
**Test Steps**:
1. Call without voice parameter
**Expected Result**: Audio with default voice

### TC-TTS-003: Convert to MP3 format
**Description**: Output as MP3  
**Test Steps**:
1. Specify format = "MP3"
**Expected Result**: Valid MP3 audio

### TC-TTS-004: Convert to WAV format
**Description**: Output as WAV  
**Test Steps**:
1. Specify format = "LINEAR16"
**Expected Result**: Valid WAV audio

### TC-TTS-005: Handle empty text
**Description**: Empty text fails gracefully  
**Test Steps**:
1. Call with empty text
**Expected Result**: ArgumentException

### TC-TTS-006: Handle long text
**Description**: Long text handled  
**Test Steps**:
1. Provide text > 5000 chars
**Expected Result**: Text split or error

---

## P1 - High Priority Tests

### TC-TTS-007: Select specific voice
**Description**: Use specific voice  
**Test Steps**:
1. Specify voice name
**Expected Result**: Audio in specified voice

### TC-TTS-008: Set speaking rate
**Description**: Adjust speech speed  
**Test Steps**:
1. Set speakingRate = 1.5
**Expected Result**: Faster speech

### TC-TTS-009: Set pitch
**Description**: Adjust voice pitch  
**Test Steps**:
1. Set pitch = 2.0
**Expected Result**: Higher pitch

### TC-TTS-010: Get available voices
**Description**: List all voices  
**Test Steps**:
1. Call `GetVoicesAsync(language)`
**Expected Result**: Voice list

### TC-TTS-011: Handle unsupported language
**Description**: Invalid language error  
**Test Steps**:
1. Specify invalid language
**Expected Result**: Clear error

### TC-TTS-012: Handle service unavailable
**Description**: Service error handled  
**Test Steps**:
1. Mock service down
**Expected Result**: ServiceException

### TC-TTS-013: Handle quota exceeded
**Description**: Quota error handled  
**Test Steps**:
1. Mock quota exceeded
**Expected Result**: QuotaException

---

## Performance Tests

### TC-TTS-P001: Short text < 2s
**Performance Criteria**: 100 words < 2 seconds

### TC-TTS-P002: Long text < 10s
**Performance Criteria**: 1000 words < 10 seconds

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/GoogleTextToSpeechServiceTests.cs`

