# AI Prompt / Insights Refresh — Comprehensive Test Cases

**Component:** Opportunity Insights Generation, AI Prompt Management, Cache Refresh  
**Backend:** `UNOPSGeminiManager.GenerateOpportunityInsightsAsync`, `AiContextualService`, `UNOPSAiPromptManager`  
**Frontend:** `opportunity-view.component` (insights loading), `opportunity-analysis-section.component` (display)  
**API:** `GET /api/opportunity/{id}/insights?forceRefresh=true`  
**Prompts:** `AiPrompts.sql` — `opportunity_generate_insights`, `opportunity_statement`, `opportunity_document_transcribe`  
**AI Provider:** Google Vertex AI (Gemini `gemini-2.5-flash-lite`)  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | ≥30 | ✅ |
| 2 | Negative Tests | §2 | 90 | ≥90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | ≥90 | ✅ |
| 4 | Functional Tests | §4 | 90 | ≥90 | ✅ |
| 5 | Integration Tests | §5 | 90 | ≥90 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

**Ratio Compliance:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- E ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

### Insights Flow

```
User opens opportunity → _loadInsights()
  → GET /api/opportunity/{id}/insights
  → GenerateOpportunityInsightsAsync()
  → GetOpportunityDetailsForAIAsync() → JSON context
  → AiContextualService.FetchResultFromGemini(prompt, context)
  → ProcessPlaceholders() + CallGeminiApi()
  → Parse response: insights[], suggestions[]
  → Frontend signals: allInsights, allSuggestions
  → Analysis section renders insights + suggestions
```

### Refresh Triggers

| Trigger | Action | Cache Bypass |
|---------|--------|-------------|
| Initial page load | `_loadInsights()` | No (uses cache) |
| Section save | `sectionSaveTrigger` → 3s delay → `_loadInsights(true)` | Yes |
| Manual refresh button | `refreshRequested` → `handleInsightsRefresh()` | Yes |

### AI Prompt System

| Concept | Detail |
|---------|--------|
| **Entity** | `AiPrompt` — `Type`, `SystemInstructions`, `UserPrompt`, `UseCache`, `CacheInvalidationMinutes` |
| **Placeholders** | `{promptData}`, `{id}`, `{name}`, `{description}`, `{status}` |
| **Cache** | `IAiPromptCacheService` — keyed by `Type + entityId` |
| **Admin UI** | `ai-prompt.component.ts` — CRUD for prompts |

### Insights Response Model

```json
{
  "insights": [
    { "title": "...", "description": "...", "type": "info|warning|success", "priority": "high|medium|low" }
  ],
  "suggestions": [
    { "title": "...", "description": "...", "actionTarget": "WHAT|WHY|WHO|TEAM|WHERE|WHEN" }
  ],
  "analysisConfidence": 0.85,
  "analysisTimestamp": "2026-02-17T..."
}
```

### Other AI Features

| Feature | Prompt Type | Purpose |
|---------|-------------|---------|
| Opportunity Statement | `opportunity_statement` | Generate markdown statement |
| Statement Validation | `opportunity_statement_validation` | Validate alignment |
| Document Transcribe | `opportunity_document_transcribe` | Extract document content |
| AI Panel (Partner/Contact) | Various | Contextual AI content |
| AI Chat | Chat prompts | Interactive assistant |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: ≥30** | ✅ COMPLIANT

### Insights Generation (POS-001–012)

POS-001: Load opportunity → Insights fetched and displayed in analysis section.  
POS-002: Insights response contains 3-7 insight items.  
POS-003: Insights response contains 3-7 suggestion items.  
POS-004: Each insight has title, description, type, priority.  
POS-005: Each suggestion has title, description, actionTarget.  
POS-006: Insight types are valid: `info`, `warning`, `success`.  
POS-007: Insight priorities are valid: `high`, `medium`, `low`.  
POS-008: Suggestion actionTargets are valid: `WHAT`, `WHY`, `WHO`, `TEAM`, `WHERE`, `WHEN`.  
POS-009: `analysisConfidence` is a decimal between 0 and 1.  
POS-010: `analysisTimestamp` is a valid ISO timestamp.  
POS-011: Insights rendered with correct icons per type.  
POS-012: Suggestions rendered with actionTarget buttons.

### Cache Behavior (POS-013–020)

POS-013: First load → Fetches from Gemini, caches result.  
POS-014: Second load → Returns cached result (faster).  
POS-015: `forceRefresh=true` → Bypasses cache, fetches fresh from Gemini.  
POS-016: Cache key = `opportunity_generate_insights + entityId`.  
POS-017: Cache invalidation after `CacheInvalidationMinutes` → Fresh fetch.  
POS-018: Prompt change → Cache invalidated for all entities using that prompt.  
POS-019: Cached response identical to original Gemini response.  
POS-020: Cache entry created with correct TTL.

### Refresh Triggers (POS-021–027)

POS-021: Section save → Insights refresh after 3-second delay.  
POS-022: Manual refresh button click → Insights refresh immediately.  
POS-023: Loading indicator shown during refresh.  
POS-024: `insightsRefreshingPending` signal → UI shows pending state.  
POS-025: Refresh completes → Updated insights displayed.  
POS-026: Refresh completes → Loading indicator hidden.  
POS-027: Multiple section saves → Single refresh (debounced by 3s delay).

### AI Prompt Admin (POS-028–030)

POS-028: Admin UI lists all AI prompts.  
POS-029: Admin UI allows editing SystemInstructions.  
POS-030: Admin UI allows editing UserPrompt.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: ≥90** | ✅ COMPLIANT

### Insights Generation Failures (NEG-001–020)

NEG-001: Insights for non-existent opportunity → 404.  
NEG-002: Insights for soft-deleted opportunity → 404.  
NEG-003: Insights without authentication → 401.  
NEG-004: Insights without permission → 403.  
NEG-005: Insights with opportunity ID=0 → Validation error.  
NEG-006: Insights with negative opportunity ID → Validation error.  
NEG-007: Insights with non-numeric opportunity ID → 400.  
NEG-008: Insights API called with POST instead of GET → 405.  
NEG-009: Insights with malformed query parameter → Ignored or error.  
NEG-010: Insights with `forceRefresh=invalid` → Parsed as false or error.  
NEG-011: Gemini API unavailable → Error response returned.  
NEG-012: Gemini API timeout → Timeout error.  
NEG-013: Gemini API rate limit → 429 error.  
NEG-014: Gemini API returns invalid JSON → Parse error handled.  
NEG-015: Gemini API returns empty response → Error or empty insights.  
NEG-016: Gemini API returns malformed insights → Parse error handled.  
NEG-017: Gemini API returns HTML instead of JSON → Parse error.  
NEG-018: Gemini API returns truncated response → Parse error.  
NEG-019: Gemini model not found → 404 from Vertex AI.  
NEG-020: Gemini project/location incorrect → Auth error.

### Prompt Failures (NEG-021–040)

NEG-021: Prompt not found in database → Error (no prompt for type).  
NEG-022: Prompt with null SystemInstructions → Sent without system instruction.  
NEG-023: Prompt with empty SystemInstructions → Same.  
NEG-024: Prompt with null UserPrompt → Error or empty prompt.  
NEG-025: Prompt with empty UserPrompt → Error.  
NEG-026: Prompt with invalid placeholder name → Placeholder not replaced.  
NEG-027: Prompt with `{promptData}` but no data → Empty string substituted.  
NEG-028: Prompt with unmatched braces `{unclosed` → Literal text.  
NEG-029: Prompt with SQL injection in placeholder → Not executed (read-only).  
NEG-030: Prompt with XSS in template → Escaped in response.  
NEG-031: Prompt exceeding Gemini token limit → Truncated or error.  
NEG-032: Prompt with binary content → Error.  
NEG-033: Prompt deletion while in use → Error on next generation.  
NEG-034: Prompt type mismatch → Wrong prompt used.  
NEG-035: Prompt with UseCache=false → Never cached.  
NEG-036: Prompt with CacheInvalidationMinutes=0 → Immediate invalidation.  
NEG-037: Prompt with CacheInvalidationMinutes=-1 → Invalid, behavior undefined.  
NEG-038: Prompt update without admin permission → 403.  
NEG-039: Prompt create with duplicate Type → Unique violation.  
NEG-040: Prompt with very long SystemInstructions → Accepted (text type).

### Cache Failures (NEG-041–055)

NEG-041: Cache service unavailable → Fallback to direct Gemini call.  
NEG-042: Cache corruption → Stale data returned.  
NEG-043: Cache key collision → Wrong data returned.  
NEG-044: Cache entry expired → Fresh fetch triggered.  
NEG-045: Cache invalidation fails → Stale data persists.  
NEG-046: Cache write fails → Response still returned (not cached).  
NEG-047: Cache read timeout → Fallback to Gemini.  
NEG-048: Cache with maximum entries → Eviction policy applied.  
NEG-049: Cache memory pressure → Entries evicted.  
NEG-050: Concurrent cache read + invalidation → One operation wins.  
NEG-051: Cache bypass (`forceRefresh=true`) always hits Gemini.  
NEG-052: Cache bypass with Gemini failure → Error (no cached fallback).  
NEG-053: Cache key with special characters → Handled.  
NEG-054: Cache entry with very large response → Stored (memory impact).  
NEG-055: Cache across app restart → Lost (in-memory) or persisted (depends on impl).

### Frontend Failures (NEG-056–070)

NEG-056: Insights API returns empty array → "No insights" state displayed.  
NEG-057: Insights API returns null → Handled gracefully.  
NEG-058: Insights API network error → Error state in analysis section.  
NEG-059: Insights API timeout → Loading indicator replaced with error.  
NEG-060: Refresh button clicked during loading → Debounced or queued.  
NEG-061: Section save during insights refresh → Refresh requeued.  
NEG-062: Navigate away during insights load → Request cancelled.  
NEG-063: Insights with unknown `type` value → Default icon used.  
NEG-064: Insights with unknown `priority` value → Default styling.  
NEG-065: Suggestions with unknown `actionTarget` → Default button or hidden.  
NEG-066: Very large insights response → Scrollable section.  
NEG-067: Insights with HTML in title → Escaped (no XSS).  
NEG-068: Insights with HTML in description → Escaped.  
NEG-069: `loadingInsights` signal stuck true → Timeout recovery.  
NEG-070: `allInsights` signal null → Section shows empty state.

### AI Prompt / Insights / Cache / Gemini Failures (NEG-071–090)

NEG-071: AI prompt missing `opportunity_generate_insights` type → Error on insights load.  
NEG-072: AI prompt with corrupted SystemInstructions encoding → Error or fallback.  
NEG-073: Insights generation with malformed opportunity context JSON → Parse error.  
NEG-074: Gemini API returns 500 Internal Server Error → Error propagated to client.  
NEG-075: Gemini API returns 503 Service Unavailable → Retry or error.  
NEG-076: Gemini API returns 401 Unauthorized → Auth error propagated.  
NEG-077: Gemini API returns 403 Forbidden → Permission error propagated.  
NEG-078: Gemini API returns 400 Bad Request (invalid payload) → Validation error.  
NEG-079: Gemini API returns response with null insights array → Handled gracefully.  
NEG-080: Gemini API returns response with null suggestions array → Handled gracefully.  
NEG-081: Cache service throws exception on read → Fallback to Gemini.  
NEG-082: Cache service throws exception on write → Response returned, not cached.  
NEG-083: Cache key generation with null entityId → Validation error.  
NEG-084: Cache key generation with null prompt type → Validation error.  
NEG-085: Insights refresh during Gemini rate limit → 429, user notified.  
NEG-086: Multiple rapid force-refresh requests → Throttled or queued.  
NEG-087: AI prompt deleted while cached insights exist → Stale cache or error on next use.  
NEG-088: Vertex AI project quota exceeded → Quota error returned.  
NEG-089: Vertex AI region unreachable → Network error.  
NEG-090: Insights request with expired auth token → 401, token refresh attempted.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: ≥90** | ✅ COMPLIANT

### Insights Count Boundaries (BND-001–015)

BND-001: 0 insights returned → Empty insights section.  
BND-002: 1 insight returned → Single insight displayed.  
BND-003: 3 insights (minimum expected) → All displayed.  
BND-004: 7 insights (maximum expected) → All displayed.  
BND-005: 10 insights (above expected) → All displayed.  
BND-006: 50 insights (unexpected) → All displayed, scrollable.  
BND-007: 0 suggestions returned → Empty suggestions section.  
BND-008: 1 suggestion returned → Single suggestion displayed.  
BND-009: 3 suggestions (minimum expected) → All displayed.  
BND-010: 7 suggestions (maximum expected) → All displayed.  
BND-011: 10 suggestions (above expected) → All displayed.  
BND-012: `analysisConfidence = 0.0` → Displayed as 0%.  
BND-013: `analysisConfidence = 1.0` → Displayed as 100%.  
BND-014: `analysisConfidence = 0.5` → Displayed as 50%.  
BND-015: `analysisConfidence = null` → Handled (default or hidden).

### Content Boundaries (BND-016–035)

BND-016: Insight title = 1 character → Displayed.  
BND-017: Insight title = 200 characters → Displayed, truncated if needed.  
BND-018: Insight description = 1 character → Displayed.  
BND-019: Insight description = 2000 characters → Scrollable.  
BND-020: Suggestion title = 1 character → Displayed.  
BND-021: Suggestion title = 200 characters → Displayed, truncated if needed.  
BND-022: Suggestion description = 2000 characters → Scrollable.  
BND-023: Insight with Unicode content (Arabic, Chinese) → Rendered correctly.  
BND-024: Insight with emoji → Rendered correctly.  
BND-025: Insight with markdown in description → Rendered or escaped.  
BND-026: Suggestion with newlines → Preserved.  
BND-027: Suggestion actionTarget = "WHAT" → Navigates to What section.  
BND-028: Suggestion actionTarget = "WHY" → Navigates to Why section.  
BND-029: Suggestion actionTarget = "WHO" → Navigates to Who section.  
BND-030: Suggestion actionTarget = "TEAM" → Navigates to Team section.  
BND-031: Suggestion actionTarget = "WHERE" → Navigates to Where section.  
BND-032: Suggestion actionTarget = "WHEN" → Navigates to When section.  
BND-033: Insight type = "info" → Blue/info icon.  
BND-034: Insight type = "warning" → Yellow/warning icon.  
BND-035: Insight type = "success" → Green/success icon.

### Cache Boundaries (BND-036–050)

BND-036: Cache TTL = 1 minute → Expires after 60 seconds.  
BND-037: Cache TTL = 60 minutes → Expires after 1 hour.  
BND-038: Cache TTL = 1440 minutes (1 day) → Expires after 24 hours.  
BND-039: Cache hit on second load → Response time < 50ms.  
BND-040: Cache miss (first load) → Full Gemini round trip.  
BND-041: Cache miss (expired) → Full Gemini round trip.  
BND-042: Cache invalidation on prompt update → Next call refreshes.  
BND-043: Cache for opportunity A → Independent from opportunity B.  
BND-044: Cache with 0 entries → First call creates entry.  
BND-045: Cache with 100 entries → 101st entry created or evicts oldest.  
BND-046: Cache with 1000 entries → Memory impact measured.  
BND-047: Cache entry size = 1KB → Minimal impact.  
BND-048: Cache entry size = 1MB → Significant impact.  
BND-049: Cache key with entityId=1 → Unique entry.  
BND-050: Cache key with entityId=MAX_INT → Unique entry.

### Prompt Boundaries (BND-051–065)

BND-051: SystemInstructions = 100 characters → Short prompt.  
BND-052: SystemInstructions = 10,000 characters → Long prompt.  
BND-053: UserPrompt = 100 characters → Short prompt.  
BND-054: UserPrompt = 10,000 characters → Long prompt.  
BND-055: Combined prompt + context < Gemini token limit → Accepted.  
BND-056: Combined prompt + context > Gemini token limit → Truncated or error.  
BND-057: `{promptData}` replaced with 1KB JSON → Short context.  
BND-058: `{promptData}` replaced with 100KB JSON → Large context.  
BND-059: `{promptData}` replaced with 1MB JSON → Very large context.  
BND-060: Opportunity with 0 partners → Minimal context JSON.  
BND-061: Opportunity with 100 partners → Large context JSON.  
BND-062: Opportunity with all sections populated → Maximum context.  
BND-063: Opportunity with only name populated → Minimal context.  
BND-064: Multiple placeholders in one prompt → All replaced.  
BND-065: Placeholder value with curly braces → Escaped correctly.

### Refresh Timing Boundaries (BND-066–070)

BND-066: Section save → 3s delay → Refresh triggers.  
BND-067: Two section saves within 3s → Single refresh (debounced).  
BND-068: Three section saves at 1s intervals → Single refresh after last.  
BND-069: Manual refresh during auto-refresh delay → Manual takes precedence.  
BND-070: Refresh during page navigation → Cancelled or ignored.

### Token Limits, Cache Boundaries, Prompt Length (BND-071–090)

BND-071: Prompt at exactly Gemini input token limit (e.g., 32K) → Accepted.  
BND-072: Prompt at 1 token over Gemini input limit → Truncated or rejected.  
BND-073: Context JSON at 50% of token budget → Accepted.  
BND-074: Context JSON at 99% of token budget → Accepted with minimal prompt.  
BND-075: Cache TTL = 0 minutes → Immediate expiration (edge case).  
BND-076: Cache TTL = 1 second → Expires after 1 second.  
BND-077: Cache TTL = MAX_INT minutes → Long-lived entry.  
BND-078: Cache entry count at eviction threshold → Eviction triggered.  
BND-079: Cache entry count just below eviction threshold → No eviction.  
BND-080: Prompt length = 0 characters (empty UserPrompt) → Error.  
BND-081: Prompt length = 1 character → Minimal prompt sent.  
BND-082: Prompt length = 100,000 characters → Truncated or error.  
BND-083: SystemInstructions + UserPrompt combined at limit → Accepted.  
BND-084: Placeholder replacement results in prompt at limit → Accepted or truncated.  
BND-085: Insight array length = 0 → Empty section.  
BND-086: Insight array length = 100 → All displayed or paginated.  
BND-087: Suggestion array length = 100 → All displayed or paginated.  
BND-088: analysisConfidence = 0.9999 → Displayed correctly.  
BND-089: analysisConfidence = 0.0001 → Displayed correctly.  
BND-090: Debounce delay = 0ms (if configurable) → Immediate refresh.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: ≥90** | ✅ COMPLIANT

### Insights API (FUN-001–015)

FUN-001: `GET /api/opportunity/{id}/insights` returns insights and suggestions.  
FUN-002: `forceRefresh=true` bypasses cache.  
FUN-003: `forceRefresh=false` (default) uses cache.  
FUN-004: Response JSON structure matches `OpportunityInsightsResponse` model.  
FUN-005: Insights sorted by priority (high → medium → low).  
FUN-006: Suggestions sorted by actionTarget relevance.  
FUN-007: API returns 200 on success.  
FUN-008: API returns correct Content-Type (`application/json`).  
FUN-009: API logs insights generation request.  
FUN-010: API logs Gemini response time.  
FUN-011: API handles partial Gemini response → Returns what's available.  
FUN-012: API sanitizes Gemini output → No XSS in response.  
FUN-013: API respects opportunity permissions → Only authorized users.  
FUN-014: API includes `analysisConfidence` in response.  
FUN-015: API includes `analysisTimestamp` in response.

### Prompt Processing (FUN-016–030)

FUN-016: `ProcessPlaceholders` replaces `{promptData}` with JSON context.  
FUN-017: `ProcessPlaceholders` replaces `{id}` with opportunity ID.  
FUN-018: `ProcessPlaceholders` replaces `{name}` with opportunity name.  
FUN-019: `ProcessPlaceholders` handles missing placeholders → No replacement.  
FUN-020: `ProcessPlaceholders` handles null context → Empty string.  
FUN-021: `CallGeminiApi` sends `system_instruction` to Vertex AI.  
FUN-022: `CallGeminiApi` sends user prompt as content.  
FUN-023: `CallGeminiApi` uses correct Vertex AI URL.  
FUN-024: `CallGeminiApi` uses correct model (`gemini-2.5-flash-lite`).  
FUN-025: `CallGeminiApi` includes `GenerationConfig` from prompt entity.  
FUN-026: `CallGeminiApi` includes `SafetySettings` from prompt entity.  
FUN-027: `CallGeminiApi` uses project, location from prompt entity.  
FUN-028: `AiPromptCacheService` stores response with correct key.  
FUN-029: `AiPromptCacheService` retrieves cached response by key.  
FUN-030: `AiPromptCacheService` invalidates entries when prompt updated.

### Frontend Display (FUN-031–042)

FUN-031: `allInsights` signal populated from API response.  
FUN-032: `allSuggestions` signal populated from API response.  
FUN-033: `loadingInsights` signal true during fetch, false after.  
FUN-034: `insightsRefreshingPending` signal true during debounce delay.  
FUN-035: Analysis section receives insights as input signal.  
FUN-036: Analysis section receives suggestions as input signal.  
FUN-037: Analysis section refresh button emits `refreshRequested`.  
FUN-038: Parent handles `refreshRequested` → Calls `_loadInsights(true)`.  
FUN-039: Insights rendered with correct icon per type.  
FUN-040: Suggestions rendered with action target buttons.  
FUN-041: Action target button navigates to correct section.  
FUN-042: Loading skeleton shown during insights load.

### Prompt Admin (FUN-043–050)

FUN-043: Admin lists all prompts from `AiPrompt` table.  
FUN-044: Admin edits SystemInstructions → Saved to DB.  
FUN-045: Admin edits UserPrompt → Saved to DB.  
FUN-046: Admin toggles UseCache → Saved to DB.  
FUN-047: Admin sets CacheInvalidationMinutes → Saved to DB.  
FUN-048: Prompt update triggers cache invalidation.  
FUN-049: Prompt CRUD uses `UNOPSAiPromptManager`.  
FUN-050: Prompt admin restricted to admin users.

### Prompt Processing Rules, Cache Behavior, Analysis Display (FUN-051–090)

FUN-051: `ProcessPlaceholders` replaces `{description}` when present in prompt.  
FUN-052: `ProcessPlaceholders` replaces `{status}` when present in prompt.  
FUN-053: `ProcessPlaceholders` preserves literal `{{` as escaped brace.  
FUN-054: `ProcessPlaceholders` handles nested JSON in `{promptData}`.  
FUN-055: `ProcessPlaceholders` trims whitespace around placeholder values.  
FUN-056: Cache stores response with TTL from prompt's CacheInvalidationMinutes.  
FUN-057: Cache returns same object reference for repeated reads (if applicable).  
FUN-058: Cache invalidation clears only entries for that prompt type.  
FUN-059: Cache does not store failed Gemini responses.  
FUN-060: Cache key includes prompt type and entityId only (no extra params).  
FUN-061: Analysis section displays insights in priority order.  
FUN-062: Analysis section displays suggestions grouped by actionTarget.  
FUN-063: Analysis section shows confidence percentage when analysisConfidence present.  
FUN-064: Analysis section shows timestamp when analysisTimestamp present.  
FUN-065: Analysis section empty state when no insights.  
FUN-066: Analysis section empty state when no suggestions.  
FUN-067: Analysis section handles mixed insight types in single response.  
FUN-068: Analysis section handles mixed suggestion actionTargets.  
FUN-069: Prompt with UseCache=true → Response cached.  
FUN-070: Prompt with UseCache=false → Response never cached.  
FUN-071: Multiple placeholders in UserPrompt → All replaced in order.  
FUN-072: Placeholder `{id}` with numeric ID → String representation used.  
FUN-073: Placeholder `{name}` with null → Empty string or "Unknown".  
FUN-074: Gemini response with extra fields → Extra fields ignored.  
FUN-075: Gemini response with missing optional fields → Defaults applied.  
FUN-076: Section save triggers debounced refresh (3s).  
FUN-077: Manual refresh bypasses debounce.  
FUN-078: Refresh during loading → Single in-flight request.  
FUN-079: Insights displayed after navigation to opportunity.  
FUN-080: Insights cleared when navigating away (if applicable).  
FUN-081: Prompt type `opportunity_generate_insights` used for insights.  
FUN-082: Prompt type `opportunity_statement` independent of insights.  
FUN-083: Prompt type `opportunity_document_transcribe` independent of insights.  
FUN-084: Admin prompt list shows Type, UseCache, CacheInvalidationMinutes.  
FUN-085: Admin prompt edit form validates required fields.  
FUN-086: Admin prompt create requires unique Type.  
FUN-087: Cache hit returns within expected latency (< 100ms).  
FUN-088: Cache miss triggers full Gemini flow.  
FUN-089: forceRefresh in URL overrides default cache behavior.  
FUN-090: Insights API idempotent for same opportunity + forceRefresh=false.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: ≥90** | ✅ COMPLIANT

### Insights End-to-End (INT-001–015)

INT-001: Load opportunity → Insights displayed in analysis section.  
INT-002: Insights reflect current opportunity data.  
INT-003: Change opportunity data → Save → Insights refresh after 3s → Updated.  
INT-004: Manual refresh → New insights from Gemini.  
INT-005: Cached insights → Second load instant.  
INT-006: Force refresh → Cache bypassed, fresh data.  
INT-007: Insights on Draft opportunity → Generated.  
INT-008: Insights on Active opportunity → Generated.  
INT-009: Insights on Closed (NO GO) opportunity → Generated (read-only).  
INT-010: Insights on GO opportunity → Generated (read-only).  
INT-011: Insights include budget analysis → Reflects current budget.  
INT-012: Insights include stakeholder analysis → Reflects current team.  
INT-013: Insights include risk assessment → Based on opportunity data.  
INT-014: Suggestions reference correct sections (WHAT, WHY, etc.).  
INT-015: Action target buttons navigate to correct section.

### Cross-Feature (INT-016–030)

INT-016: Insights + Statement section → Both use Gemini, independent.  
INT-017: Insights + Document transcribe → Both use Gemini, independent.  
INT-018: Insights + AI Panel (partner) → Both use Gemini, independent.  
INT-019: Insights + AI Chat → Different prompt types, no conflict.  
INT-020: Insights after submit (InWorkflow) → Still viewable.  
INT-021: Insights after approval (GO) → Still viewable.  
INT-022: Insights after rejection (NO GO) → Still viewable.  
INT-023: Insights on opportunity with all data → Rich insights.  
INT-024: Insights on opportunity with minimal data → Simpler insights.  
INT-025: Insights with new prompt template → Reflects template changes.  
INT-026: Prompt admin update → Next insights call uses new prompt.  
INT-027: Prompt admin update → Cache invalidated for that prompt type.  
INT-028: Multiple users viewing same opportunity → Same cached insights.  
INT-029: Different opportunities → Different insights (different cache keys).  
INT-030: Insights in different languages → Prompt language-agnostic.

### Error Recovery (INT-031–040)

INT-031: Gemini timeout → Error message in analysis section.  
INT-032: Gemini unavailable → Cached data served (if available).  
INT-033: Gemini rate limit → 429, retry after delay.  
INT-034: Gemini returns partial data → Partial insights displayed.  
INT-035: Cache service failure → Direct Gemini call (no cache).  
INT-036: Database error during prompt lookup → Error handled.  
INT-037: Network error during insights fetch → Frontend shows error state.  
INT-038: Timeout during insights fetch → Loading indicator cleared, error shown.  
INT-039: Retry after Gemini failure → Succeeds on retry.  
INT-040: App restart → Cache cleared, fresh insights on next load.

### Google ADK / Sessions (INT-041–050)

INT-041: Sessions table timestamps (TIMESTAMPTZ) → Correct after migration.  
INT-042: AI Chat sessions with new timestamp type → No issues.  
INT-043: Session creation → `create_time` in UTC.  
INT-044: Session update → `update_time` in UTC.  
INT-045: Session query by time range → TIMESTAMPTZ comparison correct.  
INT-046: Session cleanup by age → TIMESTAMPTZ arithmetic correct.  
INT-047: AI Chat + Insights → Independent systems, no conflict.  
INT-048: AI service health → Vertex AI reachable.  
INT-049: AI service model availability → Model exists and responding.  
INT-050: AI service quota → Within project quota limits.

### End-to-End AI Flows, Cross-Feature Integration (INT-051–090)

INT-051: Full flow: Open opportunity → Load insights → Display → No errors.  
INT-052: Full flow: Edit What section → Save → 3s delay → Insights refresh.  
INT-053: Full flow: Edit Why section → Save → Insights reflect changes.  
INT-054: Full flow: Edit Team section → Save → Insights reflect team changes.  
INT-055: Full flow: Add document → Transcribe → Insights may reference document.  
INT-056: Full flow: Generate statement → Insights independent, both work.  
INT-057: Full flow: Change workflow status → Insights still viewable.  
INT-058: Full flow: Force refresh → Cache bypassed → Fresh Gemini response.  
INT-059: Cross-feature: Insights + Statement + Document transcribe in one session.  
INT-060: Cross-feature: Insights while AI Panel open on related partner.  
INT-061: Cross-feature: Insights while AI Chat active → No interference.  
INT-062: Cross-feature: Prompt admin update → Insights use new prompt on next load.  
INT-063: Cross-feature: Multiple opportunities open in tabs → Each has own insights.  
INT-064: Cross-feature: Opportunity list → Open detail → Insights load.  
INT-065: Cross-feature: Search result → Open opportunity → Insights load.  
INT-066: Cross-feature: Dashboard link → Opportunity → Insights load.  
INT-067: Cross-feature: Navigation breadcrumb → Opportunity → Insights load.  
INT-068: Cross-feature: Deep link to opportunity → Insights load.  
INT-069: Cross-feature: Insights + permission change → Respects new permissions.  
INT-070: Cross-feature: Insights + opportunity soft-delete → 404 on next load.  
INT-071: End-to-end: New opportunity created → Insights generated on first view.  
INT-072: End-to-end: Opportunity cloned → New insights for cloned entity.  
INT-073: End-to-end: Opportunity with 0 sections → Minimal insights.  
INT-074: End-to-end: Opportunity with all sections → Rich insights.  
INT-075: End-to-end: Insights refresh during Gemini cold start → Completes.  
INT-076: End-to-end: Insights refresh during cache eviction → Fresh fetch.  
INT-077: End-to-end: Two users same opportunity → Shared cache or independent.  
INT-078: End-to-end: User A edits, User B views → User B sees cached until refresh.  
INT-079: End-to-end: Admin updates prompt → All users get new prompt on next call.  
INT-080: End-to-end: Vertex AI region switch → Insights still work.  
INT-081: Integration: Insights API + Opportunity API → Same auth context.  
INT-082: Integration: Insights API + Partner API → Same auth context.  
INT-083: Integration: Insights + Document upload → No race condition.  
INT-084: Integration: Insights + Workflow transition → No conflict.  
INT-085: Integration: Insights + Permission check → Consistent.  
INT-086: Integration: Insights + Audit log → Generation logged.  
INT-087: Integration: Insights + Telemetry → Metrics recorded.  
INT-088: Integration: Insights + Feature flag → Disabled flag hides insights.  
INT-089: Integration: Insights + Multi-tenant → Correct opportunity scope.  
INT-090: Integration: Insights + Localization → UI labels localized.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users requesting insights for same opportunity → One fetches, one caches.  
CON-002: Two users requesting insights for different opportunities → Independent.  
CON-003: Concurrent force refresh for same opportunity → One Gemini call, other waits.  
CON-004: Section save + manual refresh simultaneously → One refresh executes.  
CON-005: Multiple section saves in rapid succession → Debounced to single refresh.  
CON-006: Insights fetch + page navigation → Fetch cancelled.  
CON-007: Concurrent Gemini calls from different features → Independent.  
CON-008: Cache write + concurrent cache read → Consistent data.  
CON-009: Cache invalidation + concurrent cache read → Old or new data (not corrupt).  
CON-010: Prompt update + concurrent insights generation → Old or new prompt used.  
CON-011: Two admin users updating same prompt → Last write wins.  
CON-012: Insights loading + opportunity save → Save completes, insights refresh queued.  
CON-013: Concurrent AI Panel + Insights requests → Both succeed.  
CON-014: Concurrent statement generation + insights → Both succeed.  
CON-015: Gemini API under load → Queued, no data corruption.  
CON-016: Cache eviction during concurrent reads → One read misses, fetches fresh.  
CON-017: Multiple browser tabs loading insights → Each tab fetches independently.  
CON-018: Refresh button rapid clicks → Debounced to single request.  
CON-019: Auto-refresh during manual refresh → One refresh wins.  
CON-020: Insights signal update during render → Signal recomputes correctly.  
CON-021: Analysis section re-render during data update → Smooth transition.  
CON-022: Concurrent prompt CRUD operations → DB handles correctly.  
CON-023: DbContextFactory used for parallel opportunity data load → Independent.  
CON-024: Token refresh during Gemini API call → Call retried with new token.  
CON-025: Concurrent cache entries for different prompt types → Independent.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: `ProcessPlaceholders("{promptData}", jsonData)` → JSON inserted.  
UNT-002: `ProcessPlaceholders("{id}", id)` → ID inserted.  
UNT-003: `ProcessPlaceholders("{name}", name)` → Name inserted.  
UNT-004: `ProcessPlaceholders` with no placeholders → Original returned.  
UNT-005: `ProcessPlaceholders` with unknown placeholder → Not replaced.  
UNT-006: Cache key construction: `Type + "_" + entityId`.  
UNT-007: Cache key uniqueness: different entities → different keys.  
UNT-008: Cache key uniqueness: different prompt types → different keys.  
UNT-009: Insights response parsing → Correct model populated.  
UNT-010: Insights with missing fields → Defaults applied.  
UNT-011: Insights with extra fields → Extra ignored.  
UNT-012: `analysisConfidence` parsed as decimal.  
UNT-013: `analysisTimestamp` parsed as DateTime.  
UNT-014: Insight type validation → `info`, `warning`, `success` valid.  
UNT-015: Insight priority validation → `high`, `medium`, `low` valid.  
UNT-016: Suggestion actionTarget validation → 6 valid values.  
UNT-017: `_loadInsights(false)` → `forceRefresh=false` in API call.  
UNT-018: `_loadInsights(true)` → `forceRefresh=true` in API call.  
UNT-019: `sectionSaveTrigger` effect → 3-second delay.  
UNT-020: `refreshRequested` output → Emits event.  
UNT-021: `loadingInsights` signal → True during fetch, false after.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Insights API (cache hit) < 100ms.  
PRF-002: Insights API (cache miss, Gemini call) < 15s.  
PRF-003: Gemini API round trip < 10s.  
PRF-004: Prompt lookup from DB < 50ms.  
PRF-005: Placeholder processing < 10ms.  
PRF-006: Cache read < 5ms.  
PRF-007: Cache write < 10ms.  
PRF-008: Context JSON generation (`GetOpportunityDetailsForAIAsync`) < 5s.  
PRF-009: Analysis section render < 200ms.  
PRF-010: Insights refresh after section save < 18s (3s delay + 15s Gemini).  
PRF-011: Manual refresh < 15s.  
PRF-012: Memory usage per cached insight < 10KB.  
PRF-013: 100 cached insights → < 1MB total memory.  
PRF-014: Concurrent insights for 10 users → All < 20s.  
PRF-015: Prompt admin page load < 500ms.  
PRF-016: Prompt save < 300ms.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 20 concurrent insights requests → All complete < 30s.  
LDT-002: 50 concurrent force refresh requests → Gemini handles load.  
LDT-003: 100 concurrent cache reads → All < 50ms.  
LDT-004: Sustained insights usage (100/hour) → Stable performance.  
LDT-005: Spike: 30 insights requests in 10 seconds → Queued, all complete.  
LDT-006: Gemini API under heavy load → Graceful degradation.  
LDT-007: Cache with 1000 entries → Read performance stable.  
LDT-008: Recovery after Gemini outage → Cached data served, fresh on recovery.  
LDT-009: Recovery after cache failure → Direct Gemini calls resume.  
LDT-010: Sustained prompt updates (10/hour) → Cache invalidation stable.

---

## Status: Ready for Implementation
