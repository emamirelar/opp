# AiContextualService - Unit Test Cases

**Manager**: `AiContextualService`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/AiContextualService.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `AiContextualService` with focus on:
- Similarity search
- Embedding generation
- Context retrieval
- Vector operations
- Cache management

**Total Test Cases**: 25+

---

## 1. Embedding Generation Tests

### TC-AI-001: Generate Text Embedding
**Test**: `GenerateEmbedding_Should_ReturnVector_When_ValidTextProvided`

### TC-AI-002: Generate Batch Embeddings
**Test**: `GenerateBatchEmbeddings_Should_ReturnVectors_When_MultipleTextsProvided`

### TC-AI-003: Handle Empty Text
**Test**: `GenerateEmbedding_Should_ThrowException_When_TextEmpty`

### TC-AI-004: Cache Embeddings
**Test**: `GenerateEmbedding_Should_UseCache_When_EmbeddingExists`

### TC-AI-005: Embedding Dimension Validation
**Test**: `GenerateEmbedding_Should_ReturnCorrectDimension_When_VectorGenerated`

---

## 2. Similarity Search Tests

### TC-AI-006: Find Similar Documents
**Test**: `FindSimilar_Should_ReturnMatches_When_QueryProvided`

### TC-AI-007: Filter by Threshold
**Test**: `FindSimilar_Should_FilterByScore_When_ThresholdSet`

### TC-AI-008: Limit Results
**Test**: `FindSimilar_Should_ReturnTopN_When_LimitProvided`

### TC-AI-009: No Matches Found
**Test**: `FindSimilar_Should_ReturnEmpty_When_NoSimilarDocuments`

### TC-AI-010: Sort by Similarity Score
**Test**: `FindSimilar_Should_OrderByScore_When_MultipleMatchesFound`

---

## 3. Context Retrieval Tests

### TC-AI-011: Retrieve Context for Query
**Test**: `RetrieveContext_Should_ReturnRelevant_When_QueryProvided`

### TC-AI-012: Combine Multiple Sources
**Test**: `RetrieveContext_Should_AggregateData_When_MultipleSourcesAvailable`

### TC-AI-013: Rank Context by Relevance
**Test**: `RetrieveContext_Should_OrderByRelevance_When_RankingEnabled`

### TC-AI-014: Filter by Entity Type
**Test**: `RetrieveContext_Should_FilterByType_When_EntityTypeSpecified`

### TC-AI-015: Context Size Limit
**Test**: `RetrieveContext_Should_Truncate_When_ContextExceedsLimit`

---

## 4. Vector Operations Tests

### TC-AI-016: Calculate Cosine Similarity
**Test**: `CalculateSimilarity_Should_ReturnScore_When_TwoVectorsProvided`

### TC-AI-017: Normalize Vector
**Test**: `NormalizeVector_Should_ScaleToUnitLength_When_VectorProvided`

### TC-AI-018: Vector Distance Calculation
**Test**: `CalculateDistance_Should_ReturnDistance_When_VectorsProvided`

### TC-AI-019: Vector Addition
**Test**: `AddVectors_Should_CombineVectors_When_MultipleVectorsProvided`

### TC-AI-020: Vector Dimension Mismatch
**Test**: `CalculateSimilarity_Should_ThrowException_When_DimensionsMismatch`

---

## 5. Cache Management Tests

### TC-AI-021: Store in Cache
**Test**: `CacheEmbedding_Should_SaveToCache_When_EmbeddingGenerated`

### TC-AI-022: Retrieve from Cache
**Test**: `GetCachedEmbedding_Should_ReturnEmbedding_When_CacheHit`

### TC-AI-023: Clear Cache
**Test**: `ClearCache_Should_RemoveAll_When_Called`

### TC-AI-024: Cache Expiration
**Test**: `GetCachedEmbedding_Should_ReturnNull_When_CacheExpired`

### TC-AI-025: Cache Size Limit
**Test**: `CacheEmbedding_Should_EvictOldest_When_CacheFull`

---

## Coverage Goals
**Overall**: 25+ tests, 75%+ coverage

