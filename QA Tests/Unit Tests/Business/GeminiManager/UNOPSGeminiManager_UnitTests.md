# UNOPSGeminiManager - Unit Test Cases

**Manager**: `UNOPSGeminiManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UNOPSGeminiManager` with focus on:
- AI prompt execution
- Response parsing and validation
- Token management
- Error handling for AI service failures
- Context building
- Rate limiting

**Total Test Cases**: 30+

---

## 1. Execute Prompt Tests

### TC-GM-001: Execute Simple Prompt
**Test**: `ExecutePrompt_Should_ReturnResponse_When_ValidPromptProvided`

### TC-GM-002: Execute Prompt with Context
**Test**: `ExecutePrompt_Should_IncludeContext_When_ContextProvided`

### TC-GM-003: Execute Prompt with Temperature
**Test**: `ExecutePrompt_Should_RespectTemperature_When_TemperatureSet`

### TC-GM-004: Execute Prompt - API Failure
**Test**: `ExecutePrompt_Should_ThrowException_When_APIFails`

### TC-GM-005: Execute Prompt - Invalid API Key
**Test**: `ExecutePrompt_Should_ThrowException_When_APIKeyInvalid`

---

## 2. Response Parsing Tests

### TC-GM-006: Parse JSON Response
**Test**: `ParseResponse_Should_ParseJSON_When_ValidJSONReturned`

### TC-GM-007: Parse Text Response
**Test**: `ParseResponse_Should_ReturnText_When_PlainTextReturned`

### TC-GM-008: Parse Markdown Response
**Test**: `ParseResponse_Should_PreserveFormatting_When_MarkdownReturned`

### TC-GM-009: Parse Malformed Response
**Test**: `ParseResponse_Should_HandleMalformed_When_InvalidJSONReturned`

### TC-GM-010: Extract Code Blocks
**Test**: `ParseResponse_Should_ExtractCode_When_CodeBlocksPresent`

---

## 3. Token Management Tests

### TC-GM-011: Count Input Tokens
**Test**: `CountTokens_Should_ReturnCount_When_TextProvided`

### TC-GM-012: Respect Token Limit
**Test**: `ExecutePrompt_Should_ThrowException_When_TokenLimitExceeded`

### TC-GM-013: Truncate Long Input
**Test**: `TruncateInput_Should_LimitTokens_When_InputTooLong`

### TC-GM-014: Calculate Total Tokens
**Test**: `CalculateTotalTokens_Should_SumInputAndOutput_When_ResponseReceived`

### TC-GM-015: Track Token Usage
**Test**: `TrackTokenUsage_Should_RecordUsage_When_PromptExecuted`

---

## 4. Context Building Tests

### TC-GM-016: Build Context from Entity
**Test**: `BuildContext_Should_IncludeEntityData_When_EntityProvided`

### TC-GM-017: Build Context from Multiple Entities
**Test**: `BuildContext_Should_CombineData_When_MultipleEntitiesProvided`

### TC-GM-018: Build Context with History
**Test**: `BuildContext_Should_IncludeHistory_When_ConversationHistoryProvided`

### TC-GM-019: Limit Context Size
**Test**: `BuildContext_Should_Truncate_When_ContextExceedsLimit`

### TC-GM-020: Build Context with Metadata
**Test**: `BuildContext_Should_IncludeMetadata_When_MetadataProvided`

---

## 5. Rate Limiting Tests

### TC-GM-021: Respect Rate Limit
**Test**: `ExecutePrompt_Should_Delay_When_RateLimitApproached`

### TC-GM-022: Handle Rate Limit Error
**Test**: `ExecutePrompt_Should_Retry_When_RateLimitErrorReceived`

### TC-GM-023: Track Request Count
**Test**: `TrackRequests_Should_IncrementCounter_When_PromptExecuted`

---

## 6. Error Handling Tests

### TC-GM-024: Handle Network Timeout
**Test**: `ExecutePrompt_Should_ThrowException_When_NetworkTimeout`

### TC-GM-025: Handle Service Unavailable
**Test**: `ExecutePrompt_Should_ThrowException_When_ServiceUnavailable`

### TC-GM-026: Handle Invalid Response
**Test**: `ExecutePrompt_Should_ThrowException_When_ResponseInvalid`

### TC-GM-027: Retry on Transient Failure
**Test**: `ExecutePrompt_Should_Retry_When_TransientErrorOccurs`

---

## 7. Prompt Template Tests

### TC-GM-028: Load Prompt Template
**Test**: `LoadTemplate_Should_ReturnTemplate_When_TemplateExists`

### TC-GM-029: Replace Template Variables
**Test**: `ApplyTemplate_Should_ReplaceVariables_When_VariablesProvided`

### TC-GM-030: Validate Template
**Test**: `ValidateTemplate_Should_ThrowException_When_TemplateInvalid`

---

## Coverage Goals
**Overall**: 30+ tests, 80%+ coverage

