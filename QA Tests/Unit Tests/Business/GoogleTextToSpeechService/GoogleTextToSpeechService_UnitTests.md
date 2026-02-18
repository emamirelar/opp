# GoogleTextToSpeechService - Unit Test Cases

**Manager**: `GoogleTextToSpeechService`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/GoogleTextToSpeechService.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Text-to-speech conversion, Voice selection, Audio format handling, Language support

**Total Test Cases**: 12+

---

## Test Categories

### 1. Speech Synthesis (4 tests)
- TC-TTS-001: Convert text to speech
- TC-TTS-002: Select voice type
- TC-TTS-003: Set speaking rate
- TC-TTS-004: Set pitch

### 2. Language Support (3 tests)
- TC-TTS-005: Synthesize English
- TC-TTS-006: Synthesize French
- TC-TTS-007: Synthesize Spanish

### 3. Audio Format (2 tests)
- TC-TTS-008: Generate MP3
- TC-TTS-009: Generate WAV

### 4. Error Handling (3 tests)
- TC-TTS-010: Handle invalid text
- TC-TTS-011: Handle API error
- TC-TTS-012: Handle quota exceeded

**Coverage**: 70%+

