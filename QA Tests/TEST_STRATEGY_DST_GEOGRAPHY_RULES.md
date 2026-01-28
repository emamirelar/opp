# Test Strategy: DST, Geography Management & Rules Engine

**Date**: 2026-01-27  
**Purpose**: Comprehensive test plan for Decision Support Tool (DST), Geography Management, and Rules Engine features  
**Status**: Implementation Analysis Complete, Test Plans Ready

---

## 📊 Executive Summary

This document provides comprehensive test plans for three major feature areas based on PRD analysis and codebase investigation:

| Feature | Production Status | Test Status | Priority | Estimated Tests |
|---------|------------------|-------------|----------|----------------|
| **DST (Decision Support Tool)** | ✅ **IMPLEMENTED** | 🟡 **Partial** (scaffolded only) | 🔴 **CRITICAL** | **80-120 tests** |
| **Geography Management** | ❌ **NOT IMPLEMENTED** | 🟡 **Scaffolded** | 🟠 **HIGH** | **35-50 tests** |
| **Rules Engine** | ❌ **NOT IMPLEMENTED** | ❌ **None** | 🟠 **HIGH** | **80-120 tests** |

---

## 🎯 1. Decision Support Tool (DST) - Test Suite

### **Implementation Status: ✅ PRODUCTION READY**

**Evidence of Implementation**:
- ✅ `UNOPS.PAO.Models/DSTRecommendationModel.cs` - Complete data models
- ✅ `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs` - DST recommendation logic (3600+ lines)
- ✅ `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs` - DST API endpoints
- ✅ `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSRiskManager.cs` - Risk integration

**Production Endpoints**:
```
GET  /api/opportunity/{id}/dst-recommendations
GET  /api/opportunity/{id}/dst-risks
POST /api/opportunity/{id}/dst-risks
PUT  /api/opportunity/{id}/dst-risks/{riskId}
DELETE /api/opportunity/{id}/dst-risks/{riskId}
```

**Scaffolded Tests (Archive)**: 90+ tests exist but need migration

---

### **DST Test Plan: 80-120 Tests**

#### **Backend Tests (50-70 tests)**

##### **1.1 DST Recommendation Generation (15-20 tests)**

**File**: `QA Tests/Integration Tests/DST/DSTRecommendationTests.cs`

```csharp
// TC-DST-REC-001: Generate recommendations with complete opportunity data
[Fact]
public async Task GetDSTRecommendations_CompleteOpportunity_ReturnsRelevantRisks()

// TC-DST-REC-002: Handle missing opportunity context gracefully
[Fact]
public async Task GetDSTRecommendations_MinimalData_ReturnsBasicRecommendations()

// TC-DST-REC-003: Extract keywords from opportunity description
[Fact]
public async Task GetDSTRecommendations_ContextualKeywords_ExtractsRelevantTerms()

// TC-DST-REC-004: Vector store integration - find similar project risks
[Fact]
public async Task GetDSTRecommendations_VectorStore_FindsSimilarProjectRisks()

// TC-DST-REC-005: Predefined high risks from oUP EAC checklist
[Fact]
public async Task GetDSTRecommendations_PreDefinedRisks_IncludesEACChecklist()

// TC-DST-REC-006: Confidence level thresholds (strongly recommended >= 80)
[Fact]
public async Task GetDSTRecommendations_ConfidenceFiltering_OnlyHighConfidence()

// TC-DST-REC-007: Dismiss and exclude recommendations
[Fact]
public async Task GetDSTRecommendations_DismissedIds_ExcludesFromResults()

// TC-DST-REC-008: Force refresh bypasses cache
[Fact]
public async Task GetDSTRecommendations_ForceRefresh_RegeneratesResults()

// TC-DST-REC-009: High Risk Guidance document integration
[Fact]
public async Task GetDSTRecommendations_WithGuidanceDoc_EnhancesRecommendations()

// TC-DST-REC-010: Pagination and maxResults parameter
[Fact]
public async Task GetDSTRecommendations_MaxResults_ReturnsTopN()

// TC-DST-REC-011: Execution time performance tracking
[Fact]
public async Task GetDSTRecommendations_PerformanceMetrics_UnderThreshold()

// TC-DST-REC-012: Source type differentiation (PREDEFINED_HIGH_RISK vs SIMILAR_PROJECT)
[Fact]
public async Task GetDSTRecommendations_SourceTypes_CorrectlyLabeled()

// TC-DST-REC-013: StableIdentifier generation for dismiss persistence
[Fact]
public async Task DSTRecommendation_StableIdentifier_UniqueAndPersistent()

// TC-DST-REC-014: oUP Question ID mapping for predefined risks
[Fact]
public async Task GetDSTRecommendations_OupQuestionId_MapsCorrectly()

// TC-DST-REC-015: Empty recommendations when no relevant risks found
[Fact]
public async Task GetDSTRecommendations_NoMatches_ReturnsEmptyList()
```

---

##### **1.2 DST Risk Management (12-15 tests)**

**File**: `QA Tests/Integration Tests/DST/DSTRiskManagementTests.cs`

```csharp
// TC-DST-RISK-001: Create risk from DST recommendation
[Fact]
public async Task AddDSTRisk_FromRecommendation_CreatesRiskRecord()

// TC-DST-RISK-002: Pre-populate risk fields from recommendation
[Fact]
public async Task AddDSTRisk_PreDefinedRisk_PopulatesCategory()

// TC-DST-RISK-003: Update DST-sourced risk
[Fact]
public async Task UpdateDSTRisk_ExistingRisk_UpdatesFields()

// TC-DST-RISK-004: Delete DST risk
[Fact]
public async Task DeleteDSTRisk_ValidId_RemovesRisk()

// TC-DST-RISK-005: Get all DST risks for opportunity
[Fact]
public async Task GetDSTRisks_OpportunityId_ReturnsAllRisks()

// TC-DST-RISK-006: Track risk source (DST recommendation vs manual)
[Fact]
public async Task DSTRisk_SourceTracking_IdentifiesDSTOrigin()

// TC-DST-RISK-007: Prevent duplicate DST recommendations
[Fact]
public async Task AddDSTRisk_DuplicateRecommendation_PreventsDuplicate()

// TC-DST-RISK-008: Risk category linking for predefined risks
[Fact]
public async Task AddDSTRisk_PreDefinedCategory_LinksCorrectly()

// TC-DST-RISK-009: Bulk add risks from multiple recommendations
[Fact]
public async Task AddDSTRisks_BulkCreate_CreatesMultiple()

// TC-DST-RISK-010: Authorization checks for DST risk operations
[Fact]
public async Task DSTRiskOperations_UnauthorizedUser_ReturnsForbidden()

// TC-DST-RISK-011: Validation errors return proper status codes
[Fact]
public async Task AddDSTRisk_InvalidData_Returns400()

// TC-DST-RISK-012: Audit trail for DST risk creation
[Fact]
public async Task AddDSTRisk_AuditLog_TracksCreation()
```

---

##### **1.3 DST Keyword Extraction & Vector Store (10-12 tests)**

**File**: `QA Tests/Integration Tests/DST/DSTKeywordExtractionTests.cs`

```csharp
// TC-DST-KWD-001: Extract keywords from opportunity title
[Fact]
public async Task ExtractKeywords_OpportunityTitle_IdentifiesKeyTerms()

// TC-DST-KWD-002: Extract keywords from description
[Fact]
public async Task ExtractKeywords_Description_ExtractsRiskIndicators()

// TC-DST-KWD-003: Extract keywords from deliverables
[Fact]
public async Task ExtractKeywords_Deliverables_IdentifiesThemes()

// TC-DST-KWD-004: Combine keywords from multiple sources
[Fact]
public async Task ExtractKeywords_MultipleSources_AggregatesUnique()

// TC-DST-KWD-005: Handle empty or minimal context
[Fact]
public async Task ExtractKeywords_MinimalContext_ReturnsBasicTerms()

// TC-DST-KWD-006: Vector store query construction
[Fact]
public async Task BuildVectorQuery_Keywords_FormatsCorrectly()

// TC-DST-KWD-007: Vector store similarity threshold
[Fact]
public async Task VectorStoreSearch_SimilarityScore_FiltersLowScores()

// TC-DST-KWD-008: Vector store returns relevant documents
[Fact]
public async Task VectorStoreSearch_RelevantRisks_ReturnsMatches()

// TC-DST-KWD-009: Handle vector store timeout gracefully
[Fact]
public async Task VectorStoreSearch_Timeout_ReturnsGracefulFallback()

// TC-DST-KWD-010: LLM refinement of vector store results
[Fact]
public async Task RefineResults_LLMRanking_ImproveRelevance()
```

---

##### **1.4 DST AI Integration (8-10 tests)**

**File**: `QA Tests/Integration Tests/DST/DSTAIIntegrationTests.cs`

```csharp
// TC-DST-AI-001: AI prompt construction for risk analysis
[Fact]
public async Task BuildAIPrompt_OpportunityContext_ProperlyFormatted()

// TC-DST-AI-002: AI response parsing and validation
[Fact]
public async Task ParseAIResponse_ValidJSON_ParsesCorrectly()

// TC-DST-AI-003: AI confidence scoring
[Fact]
public async Task AIRiskAnalysis_ConfidenceScores_WithinValidRange()

// TC-DST-AI-004: Handle AI service unavailability
[Fact]
public async Task DSTRecommendations_AIUnavailable_FallbackToPreDefined()

// TC-DST-AI-005: Token limit handling for large contexts
[Fact]
public async Task AIPrompt_LargeContext_TruncatesAppropriately()

// TC-DST-AI-006: Multi-turn conversation for clarification
[Fact]
public async Task AIAnalysis_FollowUpQuestions_RefinesResults()

// TC-DST-AI-007: AI hallucination detection
[Fact]
public async Task AIResponse_InvalidRecommendation_Filtered()

// TC-DST-AI-008: AI response caching for performance
[Fact]
public async Task AIAnalysis_CachedResults_ImprovePerformance()
```

---

##### **1.5 DST Performance & Edge Cases (5-8 tests)**

**File**: `QA Tests/Integration Tests/DST/DSTPerformanceTests.cs`

```csharp
// TC-DST-PERF-001: Large opportunity context processing time
[Fact]
public async Task DSTRecommendations_LargeContext_UnderPerformanceThreshold()

// TC-DST-PERF-002: Concurrent recommendation requests
[Fact]
public async Task DSTRecommendations_ConcurrentRequests_HandlesLoad()

// TC-DST-PERF-003: Cache effectiveness metrics
[Fact]
public async Task DSTCache_HitRate_MeetsTarget()

// TC-DST-PERF-004: Memory usage for large result sets
[Fact]
public async Task DSTRecommendations_LargeResults_MemoryEfficient()

// TC-DST-PERF-005: Database query optimization
[Fact]
public async Task DSTQueries_ExecutionPlan_Optimized()
```

---

#### **Integration Tests (15-20 tests)**

**File**: `QA Tests/Integration Tests/DST/DSTEndToEndTests.cs`

```csharp
// TC-DST-E2E-001: Complete DST workflow - recommendation to risk creation
[Fact]
public async Task DSTWorkflow_RecommendationToRisk_CompleteFlow()

// TC-DST-E2E-002: Multi-user collaboration on DST risks
[Fact]
public async Task DSTRisks_MultipleUsers_ConcurrentAccess()

// TC-DST-E2E-003: DST integration with opportunity workflow
[Fact]
public async Task DSTAnalysis_OpportunityStage_TriggersCorrectly()

// TC-DST-E2E-004: DST recommendations across opportunity lifecycle
[Fact]
public async Task DSTRecommendations_OpportunityProgression_UpdatesRelevance()

// TC-DST-E2E-005: Historical DST analysis tracking
[Fact]
public async Task DSTHistory_MultipleAnalyses_TracksChanges()

// TC-DST-E2E-006: DST risk export functionality
[Fact]
public async Task DSTRisks_Export_FormatsCorrectly()

// TC-DST-E2E-007: DST recommendations in decision reports
[Fact]
public async Task DSTRecommendations_DecisionReport_IncludesInsights()

// TC-DST-E2E-008: DST integration with partner risk profiles
[Fact]
public async Task DSTAnalysis_PartnerRisks_IncorporatesPartnerData()

// TC-DST-E2E-009: DST country context integration
[Fact]
public async Task DSTRecommendations_CountryRisk_IncludesGeography()

// TC-DST-E2E-010: DST recommendations for similar opportunities
[Fact]
public async Task DSTLearning_SimilarOpportunities_SharesKnowledge()
```

---

#### **API/Controller Tests (15-20 tests)**

**File**: `QA Tests/Integration Tests/Controllers/DSTControllerTests.cs`

```csharp
// TC-DST-API-001: GET /api/opportunity/{id}/dst-recommendations - Success
[Fact]
public async Task GetDSTRecommendations_ValidId_Returns200WithRecommendations()

// TC-DST-API-002: GET /api/opportunity/{id}/dst-recommendations - Not found
[Fact]
public async Task GetDSTRecommendations_InvalidId_Returns404()

// TC-DST-API-003: GET /api/opportunity/{id}/dst-recommendations - Unauthorized
[Fact]
public async Task GetDSTRecommendations_Unauthorized_Returns401()

// TC-DST-API-004: GET /api/opportunity/{id}/dst-recommendations - With query params
[Fact]
public async Task GetDSTRecommendations_WithFilters_FiltersResults()

// TC-DST-API-005: POST /api/opportunity/{id}/dst-risks - Create risk
[Fact]
public async Task AddDSTRisk_ValidRequest_Returns201()

// TC-DST-API-006: POST /api/opportunity/{id}/dst-risks - Validation error
[Fact]
public async Task AddDSTRisk_InvalidData_Returns400()

// TC-DST-API-007: PUT /api/opportunity/{id}/dst-risks/{riskId} - Update
[Fact]
public async Task UpdateDSTRisk_ValidData_Returns200()

// TC-DST-API-008: DELETE /api/opportunity/{id}/dst-risks/{riskId} - Delete
[Fact]
public async Task DeleteDSTRisk_ValidId_Returns204()

// TC-DST-API-009: GET /api/opportunity/{id}/dst-risks - Get all
[Fact]
public async Task GetDSTRisks_ValidId_ReturnsAllRisks()

// TC-DST-API-010: Response format validation
[Fact]
public async Task DSTEndpoints_ResponseFormat_MatchesContract()

// TC-DST-API-011: Error handling and status codes
[Fact]
public async Task DSTEndpoints_Errors_ReturnProperStatusCodes()

// TC-DST-API-012: Rate limiting on expensive operations
[Fact]
public async Task DSTRecommendations_RateLimiting_ThrottlesRequests()
```

---

### **DST Test Coverage Summary**

| Test Category | Tests | Priority | Effort |
|--------------|-------|----------|--------|
| Recommendation Generation | 15-20 | 🔴 Critical | 2-3 days |
| Risk Management | 12-15 | 🔴 Critical | 1-2 days |
| Keyword Extraction & Vector Store | 10-12 | 🟠 High | 1-2 days |
| AI Integration | 8-10 | 🟠 High | 1-2 days |
| Performance & Edge Cases | 5-8 | 🟡 Medium | 1 day |
| End-to-End Integration | 15-20 | 🔴 Critical | 2-3 days |
| API/Controller Tests | 15-20 | 🔴 Critical | 1-2 days |
| **TOTAL** | **80-105** | | **9-15 days** |

---

## 🌍 2. Geography Management & Global Indices - Test Suite

### **Implementation Status: ❌ NOT IMPLEMENTED IN PRODUCTION**

**Evidence**:
- ❌ No `GlobalIndicesManager` found in `UNOPS.PAO.Business`
- ❌ No `GlobalIndex` or `CountryProfile` entities in `UNOPS.PAO.Domain`
- ✅ **Scaffolded tests exist**: `GlobalIndicesManagerTests.cs` (15+ tests)
- ✅ **PRD documented**: Opportunity Epics rows 8-9, 19

**Design Intent** (from PRD):
- Country profiles with organizational unit ownership
- Host Country Agreement tracking
- UN Cooperation Framework integration
- Global indices (Fragile State, Corruption, MVI scores, etc.)
- Periodic index uploads for all 193 countries
- Historical "as-at" data retrieval

---

### **Geography Test Plan: 35-50 Tests**

**⚠️ PREREQUISITE: Production implementation required before tests can be executed**

#### **Backend Tests (20-30 tests)**

##### **2.1 Country Profile Management (8-10 tests)**

**File**: `QA Tests/Integration Tests/Geography/CountryProfileTests.cs`

```csharp
// TC-GEO-PROF-001: Create country profile with all required fields
[Fact]
public async Task CreateCountryProfile_CompleteData_Success()

// TC-GEO-PROF-002: Update country profile - organizational unit assignment
[Fact]
public async Task UpdateCountryProfile_OrgUnitAssignment_UpdatesCorrectly()

// TC-GEO-PROF-003: Host Country Agreement tracking
[Fact]
public async Task CountryProfile_HostCountryAgreement_TracksStatus()

// TC-GEO-PROF-004: UN Cooperation Framework integration
[Fact]
public async Task CountryProfile_UNCF_StoresFrameworkDetails()

// TC-GEO-PROF-005: Get country profile by ID
[Fact]
public async Task GetCountryProfile_ValidId_ReturnsProfile()

// TC-GEO-PROF-006: Get country profile by country code
[Fact]
public async Task GetCountryProfile_CountryCode_ReturnsProfile()

// TC-GEO-PROF-007: List all country profiles with pagination
[Fact]
public async Task GetCountryProfiles_Pagination_ReturnsPagedResults()

// TC-GEO-PROF-008: Filter country profiles by organizational unit
[Fact]
public async Task GetCountryProfiles_OrgUnitFilter_FiltersCorrectly()
```

---

##### **2.2 Global Indices Management (12-15 tests)**

**File**: `QA Tests/Integration Tests/Geography/GlobalIndicesTests.cs`

```csharp
// TC-GEO-IDX-001: Upload global indices for all countries
[Fact]
public async Task UploadGlobalIndices_193Countries_Success()

// TC-GEO-IDX-002: Update existing indices - new year data
[Fact]
public async Task UpdateGlobalIndices_NewYear_ReplacesCurrentData()

// TC-GEO-IDX-003: Historical data retrieval - "as-at" date
[Fact]
public async Task GetGlobalIndices_HistoricalDate_ReturnsAsAtData()

// TC-GEO-IDX-004: Current indices retrieval
[Fact]
public async Task GetCurrentIndices_CountryId_ReturnsLatestData()

// TC-GEO-IDX-005: Indices trend analysis over time
[Fact]
public async Task GetIndicesTrend_TimeRange_ReturnsTimeSeries()

// TC-GEO-IDX-006: Fragile State Index tracking
[Fact]
public async Task GlobalIndices_FragileStateIndex_TracksAccurately()

// TC-GEO-IDX-007: Corruption Index tracking
[Fact]
public async Task GlobalIndices_CorruptionIndex_TracksAccurately()

// TC-GEO-IDX-008: MVI (Multidimensional Vulnerability Index) tracking
[Fact]
public async Task GlobalIndices_MVIScore_TracksAccurately()

// TC-GEO-IDX-009: Bulk index upload validation
[Fact]
public async Task UploadGlobalIndices_ValidationErrors_RejectsInvalidData()

// TC-GEO-IDX-010: Index data completeness check
[Fact]
public async Task GlobalIndices_DataCompleteness_ValidatesAllCountries()

// TC-GEO-IDX-011: Index versioning and audit trail
[Fact]
public async Task GlobalIndices_Versioning_TracksChanges()

// TC-GEO-IDX-012: DST integration - trigger on index update
[Fact]
public async Task GlobalIndices_Update_TriggersDSTRefresh()
```

---

##### **2.3 Business Rules Based on Geography (5-8 tests)**

**File**: `QA Tests/Integration Tests/Geography/GeographyBusinessRulesTests.cs`

```csharp
// TC-GEO-RULE-001: Fragile state triggers high-risk flag
[Fact]
public async Task OpportunityInFragileState_RiskFlag_AutomaticallySet()

// TC-GEO-RULE-002: Corruption index above threshold triggers approval requirement
[Fact]
public async Task OpportunityHighCorruption_AdditionalApproval_Required()

// TC-GEO-RULE-003: MVI score influences risk assessment
[Fact]
public async Task OpportunityMVIScore_RiskAssessment_Adjusted()

// TC-GEO-RULE-004: Host Country Agreement missing triggers warning
[Fact]
public async Task OpportunityNoHCA_Warning_Displayed()

// TC-GEO-RULE-005: UNCF alignment validation
[Fact]
public async Task OpportunityUNCF_AlignmentCheck_ValidatesFramework()

// TC-GEO-RULE-006: Geography-based reporting filters
[Fact]
public async Task GeographyReporting_IndexFilters_CorrectlyApplies()
```

---

#### **Integration Tests (10-12 tests)**

**File**: `QA Tests/Integration Tests/Geography/GeographyIntegrationTests.cs`

```csharp
// TC-GEO-INT-001: Country profile creation cascades to opportunity validation
[Fact]
public async Task CountryProfile_OpportunityValidation_IntegratesCorrectly()

// TC-GEO-INT-002: Index update triggers DST re-analysis
[Fact]
public async Task IndexUpdate_DSTAnalysis_RefreshesRecommendations()

// TC-GEO-INT-003: Organizational unit assignment affects user permissions
[Fact]
public async Task OrgUnitAssignment_UserPermissions_AppliesCorrectly()

// TC-GEO-INT-004: Historical data for closed opportunities
[Fact]
public async Task HistoricalIndices_ClosedOpportunities_UsesAsAtData()

// TC-GEO-INT-005: Geography data in opportunity reports
[Fact]
public async Task OpportunityReport_GeographyData_IncludesIndices()

// TC-GEO-INT-006: Multi-country opportunities aggregate indices
[Fact]
public async Task MultiCountryOpportunity_IndicesAggregation_AveragesCorrectly()

// TC-GEO-INT-007: Index-based opportunity filtering
[Fact]
public async Task OpportunityFilter_ByIndices_FiltersCorrectly()

// TC-GEO-INT-008: Geography data export for analytics
[Fact]
public async Task GeographyData_Export_FormatsForAnalytics()
```

---

#### **API/Controller Tests (5-8 tests)**

**File**: `QA Tests/Integration Tests/Controllers/GeographyControllerTests.cs`

```csharp
// TC-GEO-API-001: GET /api/geography/countries - Get all countries
[Fact]
public async Task GetCountries_Success_Returns200()

// TC-GEO-API-002: GET /api/geography/countries/{id} - Get country profile
[Fact]
public async Task GetCountryProfile_ValidId_Returns200()

// TC-GEO-API-003: POST /api/geography/indices/upload - Upload indices
[Fact]
public async Task UploadIndices_ValidData_Returns201()

// TC-GEO-API-004: GET /api/geography/indices/current/{countryId} - Get current indices
[Fact]
public async Task GetCurrentIndices_ValidId_ReturnsLatestData()

// TC-GEO-API-005: GET /api/geography/indices/historical/{countryId} - Get historical indices
[Fact]
public async Task GetHistoricalIndices_AsAtDate_ReturnsHistoricalData()

// TC-GEO-API-006: Authorization on admin endpoints
[Fact]
public async Task UploadIndices_Unauthorized_Returns403()
```

---

### **Geography Test Coverage Summary**

| Test Category | Tests | Priority | Effort | Implementation Dependency |
|--------------|-------|----------|--------|---------------------------|
| Country Profile Management | 8-10 | 🟠 High | 1-2 days | Backend implementation required |
| Global Indices Management | 12-15 | 🔴 Critical | 2-3 days | Backend implementation required |
| Business Rules | 5-8 | 🟡 Medium | 1 day | Backend implementation required |
| Integration Tests | 10-12 | 🟠 High | 1-2 days | Backend implementation required |
| API/Controller Tests | 5-8 | 🟡 Medium | 1 day | Backend implementation required |
| **TOTAL** | **40-53** | | **6-9 days** | **⚠️ BLOCKED** - Requires production implementation |

---

## ⚙️ 3. Rules Engine - Test Suite

### **Implementation Status: ❌ NOT IMPLEMENTED IN PRODUCTION**

**Evidence**:
- ❌ No `RulesEngine`, `RuleEvaluator`, or `RuleOrchestrator` classes found
- ✅ **Comprehensive design doc**: `docs/Architecture/Rules-Engine-Design.md` (1,798 lines)
- ❌ No scaffolded tests exist

**Design Intent** (from Architecture Doc):
- Generic, configurable rules engine for any entity/output type
- Database-driven rules (no code changes needed)
- Multiple rule evaluators:
  - Artifact Rule Evaluator
  - Relationship Rule Evaluator
  - AI Rule Evaluator
  - People/Org Rule Evaluator
  - Calculated Metrics Evaluator
- Weighted scoring system
- Historical tracking and audit trail
- Use cases:
  - Opportunity Risk calculation
  - Partner Risk Score
  - Country Risk Assessment
  - Compliance checks

---

### **Rules Engine Test Plan: 80-120 Tests**

**⚠️ PREREQUISITE: Production implementation required before tests can be executed**

#### **Backend Tests (50-70 tests)**

##### **3.1 Rule Engine Core (10-15 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/RuleEngineOrchestratorTests.cs`

```csharp
// TC-RULES-CORE-001: Load active rules for output type
[Fact]
public async Task LoadRules_OutputType_ReturnsActiveRules()

// TC-RULES-CORE-002: Execute rules in priority order
[Fact]
public async Task ExecuteRules_Priority_ExecutesInOrder()

// TC-RULES-CORE-003: Calculate weighted scores across rules
[Fact]
public async Task CalculateScore_WeightedRules_AggregatesCorrectly()

// TC-RULES-CORE-004: Store rule execution results
[Fact]
public async Task ExecuteRules_StoreResults_CreatesExecutionRecord()

// TC-RULES-CORE-005: Rule execution history tracking
[Fact]
public async Task RuleExecution_History_TracksAllExecutions()

// TC-RULES-CORE-006: Handle rule evaluation errors gracefully
[Fact]
public async Task RuleEvaluation_Error_ContinuesWithOtherRules()

// TC-RULES-CORE-007: Rule caching for performance
[Fact]
public async Task LoadRules_Cache_ImprovesPerformance()

// TC-RULES-CORE-008: Concurrent rule execution
[Fact]
public async Task ExecuteRules_Concurrent_HandlesParallel()

// TC-RULES-CORE-009: Rule activation/deactivation
[Fact]
public async Task RuleActivation_ActiveFlag_ControlsExecution()

// TC-RULES-CORE-010: Rule versioning and audit trail
[Fact]
public async Task RuleVersioning_Changes_TrackedInHistory()
```

---

##### **3.2 Artifact Rule Evaluator (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/ArtifactRuleEvaluatorTests.cs`

```csharp
// TC-RULES-ART-001: Evaluate artifact value against threshold
[Fact]
public async Task EvaluateArtifact_ValueThreshold_CorrectlyEvaluates()

// TC-RULES-ART-002: Query EntityArtifact by type and entity
[Fact]
public async Task QueryArtifact_ByType_RetrievesCorrectArtifact()

// TC-RULES-ART-003: String comparison rules
[Fact]
public async Task EvaluateArtifact_StringValue_MatchesCondition()

// TC-RULES-ART-004: Numeric comparison rules
[Fact]
public async Task EvaluateArtifact_NumericValue_ComparesCorrectly()

// TC-RULES-ART-005: Boolean artifact rules
[Fact]
public async Task EvaluateArtifact_BooleanValue_EvaluatesCorrectly()

// TC-RULES-ART-006: Date comparison rules
[Fact]
public async Task EvaluateArtifact_DateValue_ComparesCorrectly()

// TC-RULES-ART-007: Missing artifact handling
[Fact]
public async Task EvaluateArtifact_NotFound_ReturnsDefaultScore()

// TC-RULES-ART-008: Multiple artifact conditions (AND/OR logic)
[Fact]
public async Task EvaluateArtifacts_MultipleConditions_AppliesLogic()
```

---

##### **3.3 Relationship Rule Evaluator (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/RelationshipRuleEvaluatorTests.cs`

```csharp
// TC-RULES-REL-001: Count related entities
[Fact]
public async Task EvaluateRelationship_EntityCount_CountsCorrectly()

// TC-RULES-REL-002: Relationship existence check
[Fact]
public async Task EvaluateRelationship_Exists_ReturnsTrueIfExists()

// TC-RULES-REL-003: Relationship diversity metrics
[Fact]
public async Task EvaluateRelationship_Diversity_CalculatesMetric()

// TC-RULES-REL-004: One-to-many relationship rules
[Fact]
public async Task EvaluateRelationship_OneToMany_EvaluatesCorrectly()

// TC-RULES-REL-005: Many-to-many relationship rules
[Fact]
public async Task EvaluateRelationship_ManyToMany_EvaluatesCorrectly()

// TC-RULES-REL-006: Related entity property aggregation
[Fact]
public async Task EvaluateRelationship_Aggregation_SumsValues()

// TC-RULES-REL-007: Relationship quality checks
[Fact]
public async Task EvaluateRelationship_Quality_AssessesStrength()

// TC-RULES-REL-008: Cross-entity relationship rules
[Fact]
public async Task EvaluateRelationship_CrossEntity_NavigatesCorrectly()
```

---

##### **3.4 AI Rule Evaluator (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/AIRuleEvaluatorTests.cs`

```csharp
// TC-RULES-AI-001: Call AI service for similarity analysis
[Fact]
public async Task EvaluateAI_SimilarityAnalysis_CallsAIService()

// TC-RULES-AI-002: Parse AI response for rule evaluation
[Fact]
public async Task EvaluateAI_Response_ParsesCorrectly()

// TC-RULES-AI-003: AI confidence score integration
[Fact]
public async Task EvaluateAI_ConfidenceScore_IncorporatesIntoRule()

// TC-RULES-AI-004: Similarity detection for similar projects
[Fact]
public async Task EvaluateAI_SimilarProjects_DetectsRelevance()

// TC-RULES-AI-005: Sentiment analysis integration
[Fact]
public async Task EvaluateAI_SentimentAnalysis_EvaluatesCorrectly()

// TC-RULES-AI-006: Handle AI service unavailability
[Fact]
public async Task EvaluateAI_ServiceDown_UsesDefaultScore()

// TC-RULES-AI-007: AI response caching
[Fact]
public async Task EvaluateAI_Cache_ReducesAPIcalls()

// TC-RULES-AI-008: AI token limit handling
[Fact]
public async Task EvaluateAI_LargeInput_TruncatesContext()
```

---

##### **3.5 People/Org Rule Evaluator (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/PeopleOrgRuleEvaluatorTests.cs`

```csharp
// TC-RULES-ORG-001: User expertise evaluation
[Fact]
public async Task EvaluateOrg_UserExpertise_AssessesSkills()

// TC-RULES-ORG-002: Organizational unit capabilities
[Fact]
public async Task EvaluateOrg_OrgUnitCapability_ChecksCompetency()

// TC-RULES-ORG-003: Team composition rules
[Fact]
public async Task EvaluateOrg_TeamComposition_ValidatesRoles()

// TC-RULES-ORG-004: Geographic expertise rules
[Fact]
public async Task EvaluateOrg_GeographicExpertise_ChecksCountryExperience()

// TC-RULES-ORG-005: Organizational hierarchy rules
[Fact]
public async Task EvaluateOrg_Hierarchy_NavigatesCorrectly()

// TC-RULES-ORG-006: Role-based capability rules
[Fact]
public async Task EvaluateOrg_RoleCapability_ChecksPermissions()

// TC-RULES-ORG-007: Resource availability rules
[Fact]
public async Task EvaluateOrg_ResourceAvailability_ChecksCapacity()

// TC-RULES-ORG-008: Multi-office coordination rules
[Fact]
public async Task EvaluateOrg_MultiOffice_AssessesCoordination()
```

---

##### **3.6 Calculated Metrics Evaluator (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/CalculatedMetricsEvaluatorTests.cs`

```csharp
// TC-RULES-CALC-001: Budget variance calculation
[Fact]
public async Task EvaluateMetric_BudgetVariance_CalculatesCorrectly()

// TC-RULES-CALC-002: Output level distribution
[Fact]
public async Task EvaluateMetric_OutputDistribution_CalculatesPercentages()

// TC-RULES-CALC-003: Timeline metrics
[Fact]
public async Task EvaluateMetric_TimelineMetrics_CalculatesDuration()

// TC-RULES-CALC-004: Complexity score calculation
[Fact]
public async Task EvaluateMetric_ComplexityScore_AggregatesFactors()

// TC-RULES-CALC-005: Risk exposure calculation
[Fact]
public async Task EvaluateMetric_RiskExposure_WeightsRisks()

// TC-RULES-CALC-006: Performance indicators
[Fact]
public async Task EvaluateMetric_PerformanceKPIs_CalculatesMetrics()

// TC-RULES-CALC-007: Aggregation across entities
[Fact]
public async Task EvaluateMetric_Aggregation_SumsAcrossEntities()

// TC-RULES-CALC-008: Derived metric calculations
[Fact]
public async Task EvaluateMetric_DerivedMetric_CalculatesFromBase()
```

---

#### **Use Case Tests (20-30 tests)**

##### **3.7 Opportunity Risk Calculation (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/OpportunityRiskUseCaseTests.cs`

```csharp
// TC-RULES-OPP-001: Complete opportunity risk calculation
[Fact]
public async Task CalculateOpportunityRisk_CompleteData_ReturnsScore()

// TC-RULES-OPP-002: High-risk country triggers high score
[Fact]
public async Task CalculateOpportunityRisk_HighRiskCountry_IncreasesScore()

// TC-RULES-OPP-003: Partner risk influences opportunity risk
[Fact]
public async Task CalculateOpportunityRisk_PartnerRisk_IntegratesScore()

// TC-RULES-OPP-004: Complexity factors aggregate correctly
[Fact]
public async Task CalculateOpportunityRisk_ComplexityFactors_AggregatesScores()

// TC-RULES-OPP-005: Budget size influences risk assessment
[Fact]
public async Task CalculateOpportunityRisk_LargeBudget_AdjustsRisk()

// TC-RULES-OPP-006: Timeline influences risk score
[Fact]
public async Task CalculateOpportunityRisk_LongTimeline_IncreasesRisk()

// TC-RULES-OPP-007: Missing data handling in risk calculation
[Fact]
public async Task CalculateOpportunityRisk_MissingData_UsesDefaults()

// TC-RULES-OPP-008: Historical risk tracking
[Fact]
public async Task OpportunityRisk_History_TracksChangesOverTime()
```

---

##### **3.8 Partner Risk Score (8-10 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/PartnerRiskUseCaseTests.cs`

```csharp
// TC-RULES-PTR-001: Calculate partner risk score
[Fact]
public async Task CalculatePartnerRisk_CompleteProfile_ReturnsScore()

// TC-RULES-PTR-002: Due diligence status influences score
[Fact]
public async Task CalculatePartnerRisk_DueDiligence_AdjustsScore()

// TC-RULES-PTR-003: Partnership history influences score
[Fact]
public async Task CalculatePartnerRisk_PartnershipHistory_ConsidersPerformance()

// TC-RULES-PTR-004: Financial stability assessment
[Fact]
public async Task CalculatePartnerRisk_FinancialStability_EvaluatesHealth()

// TC-RULES-PTR-005: Geographic operating regions risk
[Fact]
public async Task CalculatePartnerRisk_GeographicRegions_AssessesRisk()

// TC-RULES-PTR-006: Partner type influences risk
[Fact]
public async Task CalculatePartnerRisk_PartnerType_AdjustsScore()

// TC-RULES-PTR-007: Capacity assessment
[Fact]
public async Task CalculatePartnerRisk_Capacity_EvaluatesCapability()

// TC-RULES-PTR-008: Aggregate risks across partnerships
[Fact]
public async Task CalculatePartnerRisk_Aggregation_AveragesAcrossProjects()
```

---

##### **3.9 Country Risk Assessment (4-6 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/CountryRiskUseCaseTests.cs`

```csharp
// TC-RULES-CTY-001: Calculate country risk from indices
[Fact]
public async Task CalculateCountryRisk_GlobalIndices_AggregatesScore()

// TC-RULES-CTY-002: Fragile state high weighting
[Fact]
public async Task CalculateCountryRisk_FragileState_HighImpact()

// TC-RULES-CTY-003: Corruption index integration
[Fact]
public async Task CalculateCountryRisk_CorruptionIndex_IntegratesScore()

// TC-RULES-CTY-004: UNOPS operational history influences score
[Fact]
public async Task CalculateCountryRisk_UNOPSHistory_AdjustsScore()

// TC-RULES-CTY-005: Multi-country aggregation
[Fact]
public async Task CalculateCountryRisk_MultiCountry_AggregatesRisks()
```

---

#### **Administration Tests (10-15 tests)**

**File**: `QA Tests/Integration Tests/RulesEngine/RuleAdministrationTests.cs`

```csharp
// TC-RULES-ADMIN-001: Create new rule definition
[Fact]
public async Task CreateRule_ValidDefinition_CreatesRule()

// TC-RULES-ADMIN-002: Update existing rule
[Fact]
public async Task UpdateRule_ValidChanges_UpdatesRule()

// TC-RULES-ADMIN-003: Delete rule (soft delete)
[Fact]
public async Task DeleteRule_ValidId_SoftDeletesRule()

// TC-RULES-ADMIN-004: Configure rule weights
[Fact]
public async Task ConfigureRuleWeight_ValidWeight_UpdatesWeight()

// TC-RULES-ADMIN-005: Define rule conditions
[Fact]
public async Task DefineRuleCondition_ValidCondition_AddsCondition()

// TC-RULES-ADMIN-006: Rule validation before activation
[Fact]
public async Task ValidateRule_BeforeActivation_ChecksCompleteness()

// TC-RULES-ADMIN-007: Rule versioning on updates
[Fact]
public async Task UpdateRule_VersionControl_CreatesNewVersion()

// TC-RULES-ADMIN-008: Bulk rule import/export
[Fact]
public async Task ImportRules_ValidJSON_CreatesMultipleRules()

// TC-RULES-ADMIN-009: Rule template management
[Fact]
public async Task CreateRuleTemplate_ValidTemplate_SavesTemplate()

// TC-RULES-ADMIN-010: Rule testing/simulation
[Fact]
public async Task SimulateRule_TestData_ReturnsExpectedScore()
```

---

### **Rules Engine Test Coverage Summary**

| Test Category | Tests | Priority | Effort | Implementation Dependency |
|--------------|-------|----------|--------|---------------------------|
| Engine Core | 10-15 | 🔴 Critical | 2-3 days | Backend implementation required |
| Artifact Evaluator | 8-10 | 🔴 Critical | 1-2 days | Backend implementation required |
| Relationship Evaluator | 8-10 | 🔴 Critical | 1-2 days | Backend implementation required |
| AI Evaluator | 8-10 | 🟠 High | 1-2 days | Backend implementation required |
| People/Org Evaluator | 8-10 | 🟠 High | 1-2 days | Backend implementation required |
| Calculated Metrics | 8-10 | 🟠 High | 1-2 days | Backend implementation required |
| Opportunity Risk | 8-10 | 🔴 Critical | 1-2 days | Backend implementation required |
| Partner Risk | 8-10 | 🟠 High | 1-2 days | Backend implementation required |
| Country Risk | 4-6 | 🟡 Medium | 1 day | Backend implementation required |
| Administration | 10-15 | 🟡 Medium | 2 days | Backend implementation required |
| **TOTAL** | **80-110** | | **12-18 days** | **⚠️ BLOCKED** - Requires production implementation |

---

## 📋 Implementation Priorities & Recommendations

### **Immediate Actions** (Week 1-2)

1. **🔴 CRITICAL: DST Test Suite Implementation**
   - **Status**: Production code exists, tests needed immediately
   - **Effort**: 9-15 days
   - **Tests**: 80-105 tests
   - **Value**: High - DST is user-facing feature in production
   - **Action**: Begin immediately with recommendation generation tests

### **Short-Term Actions** (Month 1)

2. **🟠 HIGH: Developer Consultation Required**
   - **Geography Management**: Confirm if implementation is planned
   - **Rules Engine**: Confirm if implementation is planned
   - **Action**: Schedule meeting with development team to:
     - Review PRD priorities
     - Confirm implementation roadmap
     - Determine if tests should wait or if TDD approach desired

### **Test-Driven Development Option**

If development team plans to implement Geography or Rules Engine:
- **Option 1**: Write tests first (TDD approach)
  - Create test specifications
  - Implement failing tests
  - Developers implement features to pass tests
  
- **Option 2**: Wait for implementation
  - Developers build features first
  - QA creates tests after implementation
  - Standard testing approach

---

## 🎯 Execution Strategy

### **Phase 1: DST Tests (IMMEDIATE)**

**Week 1-2**:
- Set up test infrastructure for DST
- Implement recommendation generation tests (15-20)
- Implement risk management tests (12-15)
- Target: 30-40 tests passing

**Week 3**:
- Implement keyword extraction & vector store tests (10-12)
- Implement AI integration tests (8-10)
- Target: 50-60 total tests passing

**Week 4**:
- Implement performance & edge case tests (5-8)
- Implement end-to-end integration tests (15-20)
- Implement API/controller tests (15-20)
- Target: 80-105 total tests passing

---

### **Phase 2: Geography & Rules Engine (PENDING)**

**Awaiting**:
- ✅ Developer confirmation of implementation plans
- ✅ Implementation timeline
- ✅ Decision on TDD vs traditional testing approach

**If TDD Approach Selected**:
- QA creates failing tests as specifications
- Developers implement to pass tests
- Benefits: Clear requirements, automated validation

**If Traditional Approach Selected**:
- Wait for feature implementation
- QA creates tests after implementation
- Benefits: Tests reflect actual implementation

---

## 📈 Success Metrics

### **DST Test Suite**:
- ✅ 80+ tests implemented
- ✅ 95%+ code coverage for DST features
- ✅ All critical user workflows tested
- ✅ Performance benchmarks established
- ✅ Integration with existing test suite

### **Geography Test Suite** (When Implemented):
- ✅ 35-50 tests implemented
- ✅ All global indices tracked
- ✅ Country profile CRUD validated
- ✅ Business rule integration tested

### **Rules Engine Test Suite** (When Implemented):
- ✅ 80+ tests implemented
- ✅ All evaluator types validated
- ✅ Use cases (Opportunity/Partner/Country risk) tested
- ✅ Administration interface validated

---

## 🚀 Next Steps

### **For QA Team (IMMEDIATE)**:
1. ✅ Review this strategy document
2. ✅ Begin DST test implementation (Phase 1)
3. ✅ Set up test data infrastructure for DST
4. ✅ Create first 10-15 DST tests this week

### **For Development Team (URGENT)**:
1. 🔴 **Review Geography Management PRD** - Confirm implementation plans
2. 🔴 **Review Rules Engine Architecture** - Confirm implementation plans
3. 🔴 **Schedule meeting with QA** - Discuss TDD vs traditional approach
4. 🟠 **Provide DST code review** - Help QA understand implementation details
5. 🟠 **Clarify DST vector store integration** - Technical documentation needed

### **For Product/Project Management**:
1. 🟡 **Prioritize Geography vs Rules Engine** - Which should be implemented first?
2. 🟡 **Allocate development resources** - Timeline for Geography/Rules implementation
3. 🟡 **Approve TDD approach** (if desired) - Decision needed for test-first development

---

## 📚 Reference Documents

### **DST References**:
- `UNOPS.PAO.Models/DSTRecommendationModel.cs` - Data models
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs` - Business logic
- `UNOPS.PAO.Presentation/Controllers/OpportunityController.cs` - API endpoints
- `QA Tests/Opportunity Tests/Archive/Scaffolded/C# Tests/Managers/DSTManagerTests.cs` - Scaffolded tests

### **Geography References**:
- `tasks/opportunity-ux/Opportunity Epics.md` - PRD requirements
- `QA Tests/Opportunity Tests/Archive/Scaffolded/C# Tests/Managers/GlobalIndicesManagerTests.cs` - Scaffolded tests

### **Rules Engine References**:
- `docs/Architecture/Rules-Engine-Design.md` - Complete architecture (1,798 lines)
- No scaffolded tests exist
- No production implementation found

---

**Document Status**: ✅ COMPLETE  
**Ready for**: DST test implementation (immediate), Developer consultation (Geography/Rules Engine)  
**Estimated Value**: 195-275 total tests across all three feature areas
