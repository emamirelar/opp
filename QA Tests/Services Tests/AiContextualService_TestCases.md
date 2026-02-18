# AiContextualService Test Cases

**Service**: `UNOPS.PAO.UNOPSBusiness/Managers/AiContextualService.cs`  
**Priority**: P1 - High  
**Total Test Cases**: 25  

---

## Overview

The AiContextualService handles AI-powered contextual responses:
- Contextual prompt generation
- AI response processing
- Parameter substitution
- Caching for performance
- Rate limiting

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Response Generation | 8 | P0 |
| Parameter Handling | 6 | P1 |
| Caching | 5 | P1 |
| Error Handling | 6 | P0 |

---

## P0 - Critical Tests

### TC-ACS-001: Generate contextual response with valid prompt
**Description**: Generate AI response for valid prompt  
**Test Steps**:
1. Call `GetContextualResponseAsync(prompt, parameters)`
2. Verify response returned
**Expected Result**: Non-empty AI response

### TC-ACS-002: Handle empty prompt gracefully
**Description**: Empty prompt should fail with clear error  
**Test Steps**:
1. Call with empty prompt
2. Verify error
**Expected Result**: ArgumentException

### TC-ACS-003: Parameter substitution in prompt
**Description**: Parameters replaced in prompt template  
**Test Steps**:
1. Provide prompt with `{partnerName}` placeholder
2. Provide parameters `{ "partnerName": "ACME Corp" }`
3. Verify substitution
**Expected Result**: Prompt contains "ACME Corp"

### TC-ACS-004: Missing required parameter fails
**Description**: Missing required parameter throws error  
**Test Steps**:
1. Provide prompt with placeholder
2. Omit parameter
3. Verify error
**Expected Result**: ArgumentException

### TC-ACS-005: Handle AI service timeout
**Description**: Timeout handled gracefully  
**Test Steps**:
1. Mock timeout response
2. Call service
3. Verify timeout error
**Expected Result**: TimeoutException with retry guidance

### TC-ACS-006: Handle AI service unavailable
**Description**: Service unavailable handled  
**Test Steps**:
1. Mock 503 response
2. Call service
3. Verify error
**Expected Result**: ServiceUnavailableException

### TC-ACS-007: Rate limiting enforced
**Description**: Rate limits prevent abuse  
**Test Steps**:
1. Make many rapid requests
2. Verify rate limit response
**Expected Result**: Rate limit error after threshold

### TC-ACS-008: Response parsing handles malformed JSON
**Description**: Malformed AI response handled  
**Test Steps**:
1. Mock malformed response
2. Call service
3. Verify graceful handling
**Expected Result**: Parsing error with fallback

---

## P1 - High Priority Tests

### TC-ACS-009: Multiple parameter substitution
**Description**: Multiple parameters replaced correctly  
**Test Steps**:
1. Provide prompt with multiple placeholders
2. Provide all parameters
3. Verify all substituted
**Expected Result**: All placeholders replaced

### TC-ACS-010: Optional parameter handling
**Description**: Optional parameters use defaults  
**Test Steps**:
1. Provide prompt with optional placeholder
2. Omit optional parameter
3. Verify default used
**Expected Result**: Default value in prompt

### TC-ACS-011: Special characters in parameters
**Description**: Special characters escaped properly  
**Test Steps**:
1. Provide parameter with special chars
2. Verify proper handling
**Expected Result**: No injection vulnerabilities

### TC-ACS-012: Long prompt handling
**Description**: Long prompts truncated appropriately  
**Test Steps**:
1. Provide very long prompt
2. Verify handling
**Expected Result**: Truncated or error

### TC-ACS-013: Cached response returned
**Description**: Cached responses used when available  
**Test Steps**:
1. Make request
2. Make identical request
3. Verify cache hit
**Expected Result**: Second request faster

### TC-ACS-014: Cache invalidation works
**Description**: Cache invalidated on relevant changes  
**Test Steps**:
1. Cache response
2. Trigger invalidation
3. Verify fresh response
**Expected Result**: New response generated

### TC-ACS-015: Cache key generation
**Description**: Unique cache keys for different requests  
**Test Steps**:
1. Make two different requests
2. Verify separate cache entries
**Expected Result**: Different cache keys

### TC-ACS-016: Concurrent requests handled
**Description**: Concurrent requests don't conflict  
**Test Steps**:
1. Make 10 concurrent requests
2. Verify all complete
**Expected Result**: All requests succeed

### TC-ACS-017: Response filtering for PII
**Description**: PII filtered from responses  
**Test Steps**:
1. Generate response with potential PII
2. Verify filtering
**Expected Result**: PII redacted

---

## Performance Tests

### TC-ACS-P001: Response time within threshold
**Performance Criteria**: < 3000ms for response

### TC-ACS-P002: Batch requests performance
**Performance Criteria**: 10 requests < 15 seconds

### TC-ACS-P003: Cache hit performance
**Performance Criteria**: < 50ms for cached response

---

**Last Updated**: December 18, 2025  
**C# Test File**: `QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Services/AiContextualServiceTests.cs`

