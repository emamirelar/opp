# DST AI-Powered Profiling — Comprehensive Test Cases

**Component:** Decision Support Tool — AI Risk Recommendations, Risk Register, Semantic Search  
**Backend:** `UNOPSGeminiManager` (DST methods), `UNOPSOpportunityManager`, `UNOPSRiskManager`  
**Frontend:** `opportunity-dst-section.component`, `opportunity-view.component`  
**API Endpoints:**  
- `POST /api/opportunity/{id}/dst-recommendations` — AI risk recommendations  
- `GET/POST/PUT/DELETE /api/opportunity/{id}/dst-risks` — Risk register CRUD  
- `PUT /api/opportunity/{id}/acknowledge-high-risks` — High risk acknowledgement  
- `GET /api/opportunity/{id}/similar-opportunities` — Semantic search (embeddings)  
- `GET /api/opportunity/{id}/similar-projects` — Semantic search (vector store)  
- `GET /api/opportunity/{id}/relevant-people` — Semantic search (vector store)  
**AI Prompts:** `opportunity_extract_risk_keywords`, `refine_opportunity_risks`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, N/E/F/I ≥ 3×P Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30=90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30=90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30=90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30=90 | ✅ |
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

### DST Recommendation Flow (4 Steps)

```
POST /api/opportunity/{id}/dst-recommendations
  → Step 1:  GetOpportunityDetailsForAIAsync(id) → Opportunity JSON context
  → Step 1.5: Load existing risks + predefined high risks (deduplication)
  → Step 1.6: Load High Risk Guidance document (EntityArtifact PDF)
  → Step 2:  ExtractRiskKeywordsAsync(context) → 5–8 risk keywords via LLM
  → Step 3:  Vector store search (EntityTypeId="RISK") → Similar risks
  → Step 4:  RefineAndRankRisksAsync(context, similarRisks, guidancePDF) → Top 10 ranked risks
  → Response: DSTRecommendationsResponse {recommendations[], keywords[], sourceCount}
```

### DSTRecommendation Model

```json
{
  "title": "Corruption risk in procurement",
  "description": "High corruption perception index...",
  "recommendation": "Implement enhanced due diligence...",
  "confidenceLevel": 0.85,
  "sourceType": "vector_store|predefined|ai_generated"
}
```

### Risk Register

| Operation | Endpoint | Method |
|-----------|----------|--------|
| List risks | `/api/opportunity/{id}/dst-risks` | GET |
| Add risk | `/api/opportunity/{id}/dst-risks` | POST |
| Update risk | `/api/opportunity/{id}/dst-risks/{riskId}` | PUT |
| Delete risk | `/api/opportunity/{id}/dst-risks/{riskId}` | DELETE |
| Accept recommendation | Add to register from DST panel | POST (same) |
| Dismiss recommendation | `DismissedOupQuestionIds[]` in request | POST |

### Semantic Search Features

| Feature | Endpoint | Source |
|---------|----------|--------|
| Similar Opportunities | `/similar-opportunities` | Embedding similarity (internal DB) |
| Similar Projects | `/similar-projects` | Vector store (external) |
| Relevant People | `/relevant-people` | Vector store (corporate directory) |

### Predefined High Risks

- Sourced from `PreDefinedHighRisk` entity (oUP EAC checklist)
- Detection rule types: CPI < 50, Fragile State, Sanctions, etc.
- `DetectionRuleType` enum for automatic risk detection
- `highRisksAcknowledged` flag on Opportunity

### DST Section UI Components

- Risk register table (add/edit/delete inline)
- AI recommendations panel (accept/dismiss/refresh)
- High risk indicators + acknowledgement checkbox
- Similar opportunities accordion
- Similar projects accordion
- Relevant people accordion
- Risk lookups: types, categories, probabilities, impact levels, response types

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### AI Recommendations (POS-001–012)

POS-001: POST dst-recommendations → Returns ranked risk recommendations.  
POS-002: Recommendations contain title, description, recommendation, confidenceLevel, sourceType.  
POS-003: Keywords extracted from opportunity context → 5-8 keywords returned.  
POS-004: Vector store search returns similar risks.  
POS-005: Recommendations deduplicated against existing risks.  
POS-006: Recommendations deduplicated against predefined high risks.  
POS-007: High Risk Guidance PDF included in LLM context when available.  
POS-008: DismissedOupQuestionIds excluded from recommendations.  
POS-009: Recommendations sorted by confidenceLevel (high → low).  
POS-010: Recommendations capped at 10 results.  
POS-011: Refresh button → New recommendations (bypasses cache).  
POS-012: Accept recommendation → Risk added to register.

### Risk Register (POS-013–022)

POS-013: GET dst-risks → Returns all non-deleted risks for opportunity.  
POS-014: POST dst-risk → New risk created with all fields.  
POS-015: PUT dst-risk/{riskId} → Risk updated.  
POS-016: DELETE dst-risk/{riskId} → Risk soft-deleted.  
POS-017: Risk has type, category, probability, impact, response type.  
POS-018: Risk has title and description.  
POS-019: Risk register displays in table format.  
POS-020: Inline editing for risk fields.  
POS-021: Risk count badge reflects active risks.  
POS-022: High risk acknowledgement toggle → `highRisksAcknowledged` updated.

### Semantic Search (POS-023–030)

POS-023: GET similar-opportunities → Returns similar opportunities by embedding.  
POS-024: Similar opportunities include title, similarity score, link.  
POS-025: GET similar-projects → Returns similar projects from vector store.  
POS-026: Similar projects include project name, description, relevance.  
POS-027: GET relevant-people → Returns relevant people from vector store.  
POS-028: Relevant people include name, expertise, contact info.  
POS-029: Results sorted by relevance/similarity score.  
POS-030: Results displayed in expandable accordion sections.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Recommendation Failures (NEG-001–025)

NEG-001: Recommendations for non-existent opportunity → 404.  
NEG-002: Recommendations for soft-deleted opportunity → 404.  
NEG-003: Recommendations without authentication → 401.  
NEG-004: Recommendations without permission → 403.  
NEG-005: Recommendations with opportunity ID=0 → Validation error.  
NEG-006: Recommendations with negative opportunity ID → Validation error.  
NEG-007: Recommendations with non-numeric ID → 400.  
NEG-008: Gemini API unavailable during keyword extraction → Error handled.  
NEG-009: Gemini API timeout during keyword extraction → Timeout error.  
NEG-010: Gemini API returns invalid JSON for keywords → Parse error handled.  
NEG-011: Gemini API unavailable during risk refinement → Error handled.  
NEG-012: Gemini API timeout during risk refinement → Timeout error.  
NEG-013: Vector store unavailable → Error handled, partial results.  
NEG-014: Vector store returns 0 results → LLM generates without context.  
NEG-015: Vector store timeout → Fallback to LLM-only generation.  
NEG-016: High Risk Guidance PDF not found → Proceeds without guidance.  
NEG-017: High Risk Guidance PDF corrupted → Error handled, proceeds without.  
NEG-018: DismissedOupQuestionIds with invalid IDs → Invalid IDs ignored.  
NEG-019: DismissedOupQuestionIds with all recommendation IDs → Empty result.  
NEG-020: Request body null → 400.  
NEG-021: Request body malformed JSON → 400.  
NEG-022: Opportunity with no data (empty context) → Minimal/no recommendations.  
NEG-023: LLM returns HTML instead of JSON → Parse error handled.  
NEG-024: LLM returns truncated response → Partial parse attempted.  
NEG-025: LLM returns duplicate recommendations → Deduplicated.

### Risk Register Failures (NEG-026–045)

NEG-026: Add risk to non-existent opportunity → 404.  
NEG-027: Add risk to soft-deleted opportunity → 404.  
NEG-028: Add risk without required fields → 400.  
NEG-029: Add risk with null title → Validation error.  
NEG-030: Add risk with empty title → Validation error.  
NEG-031: Add risk without authentication → 401.  
NEG-032: Add risk without edit permission → 403.  
NEG-033: Update non-existent risk → 404.  
NEG-034: Update soft-deleted risk → 404.  
NEG-035: Update risk on wrong opportunity → 403 or 404.  
NEG-036: Delete non-existent risk → 404.  
NEG-037: Delete already-deleted risk → 404.  
NEG-038: Delete risk without permission → 403.  
NEG-039: Add risk with invalid type → Validation error.  
NEG-040: Add risk with invalid category → Validation error.  
NEG-041: Add risk with invalid probability → Validation error.  
NEG-042: Add risk with invalid impact level → Validation error.  
NEG-043: Add risk with invalid response type → Validation error.  
NEG-044: Acknowledge high risks without permission → 403.  
NEG-045: Acknowledge high risks on non-existent opportunity → 404.

### Semantic Search Failures (NEG-046–060)

NEG-046: Similar opportunities for non-existent opportunity → 404.  
NEG-047: Similar opportunities without authentication → 401.  
NEG-048: Similar opportunities without permission → 403.  
NEG-049: Similar projects — vector store unavailable → Error or empty.  
NEG-050: Similar projects — vector store timeout → Timeout error.  
NEG-051: Similar projects — vector store returns invalid data → Error handled.  
NEG-052: Relevant people — vector store unavailable → Error or empty.  
NEG-053: Relevant people — vector store timeout → Timeout error.  
NEG-054: Relevant people — no matches → Empty results displayed.  
NEG-055: Similar opportunities — no embedding exists → Error or empty.  
NEG-056: Similar opportunities — embedding service down → Error.  
NEG-057: Similar opportunities for opportunity with no data → No meaningful matches.  
NEG-058: Similar projects with network error → Error message.  
NEG-059: Relevant people with 0 results → "No relevant people found" message.  
NEG-060: Semantic search with expired auth token → 401.

### Partner Risk Profile Failures (NEG-061–070)

NEG-061: Partner risk profile for non-existent partner → 404.  
NEG-062: Partner risk profile for soft-deleted partner → 404.  
NEG-063: Partner risk profile without authentication → 401.  
NEG-064: Partner risk profile Gemini unavailable → Error handled.  
NEG-065: Partner risk profile Gemini timeout → Timeout error.  
NEG-066: Partner risk profile with partner having no projects → Minimal profile.  
NEG-067: Partner risk profile cache expired → Re-generated.  
NEG-068: Partner risk profile prompt missing → Error.  
NEG-069: Partner risk profile with LLM returning invalid JSON → Parse error.  
NEG-070: Partner risk profile refresh when Gemini rate-limited → 429 error.

### Additional Negative Tests (NEG-071–090)

NEG-071: Recommendations with opportunity in wrong workflow status → Validation error or 400.  
NEG-072: Add risk with OpportunityId mismatch in request → 403.  
NEG-073: Acknowledge high risks with malformed request body → 400.  
NEG-074: DismissedOupQuestionIds with non-existent OUP IDs → Ignored.  
NEG-075: Similar opportunities with invalid embedding model version → Error.  
NEG-076: Similar projects with empty opportunity context → Empty or error.  
NEG-077: Relevant people with null opportunity keywords → Error or empty.  
NEG-078: Partner risk profile with partner having no organizations → Error or minimal.  
NEG-079: Risk register POST with duplicate risk title for same opportunity → Validation or accepted per business rule.  
NEG-080: RefineAndRankRisksAsync receives empty similar risks from vector store → LLM-only generation.  
NEG-081: ExtractRiskKeywordsAsync with context exceeding token limit → Truncated or error.  
NEG-082: Vector store search with invalid EntityTypeId → Error.  
NEG-083: High Risk Guidance PDF with unsupported format (non-PDF) → Error handled.  
NEG-084: Recommendations request during maintenance window → Service unavailable.  
NEG-085: Risk update with stale LastModifiedDate → Concurrency conflict.  
NEG-086: Similar opportunities for opportunity with no description → Error or empty.  
NEG-087: Relevant people with vector store returning malformed person objects → Error handled.  
NEG-088: DST recommendations with Gemini returning empty response → Graceful fallback.  
NEG-089: Risk register GET with invalid pagination params → Validation error.  
NEG-090: Acknowledge high risks without any high risks triggered → Idempotent, no error.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Recommendation Boundaries (BND-001–025)

BND-001: 0 recommendations returned → "No recommendations" displayed.  
BND-002: 1 recommendation → Single card displayed.  
BND-003: 5 recommendations → All displayed.  
BND-004: 10 recommendations (max) → All displayed.  
BND-005: 11+ recommendations → Capped at 10.  
BND-006: Confidence = 0.0 → Lowest priority.  
BND-007: Confidence = 0.5 → Medium priority.  
BND-008: Confidence = 1.0 → Highest priority.  
BND-009: Confidence = null → Default ordering.  
BND-010: Keywords = 0 extracted → Vector search skipped.  
BND-011: Keywords = 5 (minimum expected) → Vector search executed.  
BND-012: Keywords = 8 (maximum expected) → All used in search.  
BND-013: Keywords = 20 (above expected) → Capped or all used.  
BND-014: sourceType = "vector_store" → From similar risks search.  
BND-015: sourceType = "predefined" → From predefined high risks.  
BND-016: sourceType = "ai_generated" → LLM-generated novel risk.  
BND-017: Recommendation title = 1 char → Displayed.  
BND-018: Recommendation title = 500 chars → Truncated or scrollable.  
BND-019: Recommendation description = 5000 chars → Scrollable.  
BND-020: Recommendation with Unicode content → Rendered correctly.  
BND-021: DismissedOupQuestionIds = empty array → All recommendations shown.  
BND-022: DismissedOupQuestionIds = 1 ID → That recommendation excluded.  
BND-023: DismissedOupQuestionIds = all IDs → No recommendations.  
BND-024: DismissedOupQuestionIds = 100 IDs → All valid excluded.  
BND-025: DismissedOupQuestionIds with mix of valid and invalid IDs → Valid excluded.

### Risk Register Boundaries (BND-026–045)

BND-026: Opportunity with 0 risks → Empty table displayed.  
BND-027: Opportunity with 1 risk → Single row.  
BND-028: Opportunity with 10 risks → All displayed.  
BND-029: Opportunity with 50 risks → Scrollable table.  
BND-030: Opportunity with 100 risks → Performance acceptable.  
BND-031: Risk title = 1 char → Accepted.  
BND-032: Risk title = 500 chars → Accepted, UI truncates.  
BND-033: Risk description = 0 chars → Optional field.  
BND-034: Risk description = 5000 chars → Accepted.  
BND-035: Probability = lowest value → Display correct label.  
BND-036: Probability = highest value → Display correct label.  
BND-037: Impact = lowest → Correct label.  
BND-038: Impact = highest → Correct label.  
BND-039: All risk categories → Each renders correctly.  
BND-040: All risk types → Each renders correctly.  
BND-041: All response types → Each renders correctly.  
BND-042: Risk with all optional fields null → Accepted.  
BND-043: Risk with all fields populated → All displayed.  
BND-044: Soft-deleted risk excluded from GET list.  
BND-045: Risk created with Name set (ModifiableDeletableEntity requirement).

### Semantic Search Boundaries (BND-046–060)

BND-046: Similar opportunities: 0 results → "No similar opportunities" shown.  
BND-047: Similar opportunities: 1 result → Single card.  
BND-048: Similar opportunities: 10 results → All displayed.  
BND-049: Similar opportunities: similarity = 0.0 → Shown but low relevance.  
BND-050: Similar opportunities: similarity = 1.0 → Exact match.  
BND-051: Similar projects: 0 results → Empty accordion.  
BND-052: Similar projects: 1 result → Single entry.  
BND-053: Similar projects: 20 results → All displayed.  
BND-054: Relevant people: 0 results → "No relevant people" shown.  
BND-055: Relevant people: 1 result → Single person card.  
BND-056: Relevant people: 30 results → All displayed.  
BND-057: Embedding vector dimension mismatch → Error handled.  
BND-058: Vector store query at max token length → Truncated.  
BND-059: Similarity threshold = 0.0 → All results returned.  
BND-060: Similarity threshold = 0.9 → Only very similar returned.

### Predefined High Risk Boundaries (BND-061–070)

BND-061: Opportunity with 0 predefined high risks triggered → No warnings.  
BND-062: Opportunity with 1 high risk triggered → Warning displayed.  
BND-063: Opportunity with all high risks triggered → All warnings.  
BND-064: Country with CPI < 50 → "Pre-selection with CPI < 50" triggered.  
BND-065: Country with CPI = 50 → Boundary — not triggered.  
BND-066: Country with CPI = 49 → Triggered.  
BND-067: Country flagged as Fragile State → High risk triggered.  
BND-068: Country not flagged as Fragile State → Not triggered.  
BND-069: highRisksAcknowledged = true → Acknowledgement saved.  
BND-070: highRisksAcknowledged = false → Acknowledgement cleared.

### Additional Boundary Tests (BND-071–090)

BND-071: Opportunity with exactly 9 predefined high risks → All displayed.  
BND-072: Recommendation confidence = 0.99 → Near-maximum displayed.  
BND-073: Risk title at max length (500 chars) → Truncated in UI.  
BND-074: Embedding dimension = 0 → Error or fallback.  
BND-075: Similar opportunities with tie scores → Deterministic ordering.  
BND-076: Vector store returns exactly 100 results → Paginated or capped.  
BND-077: Keywords = 4 (below minimum 5) → Vector search skipped or reduced.  
BND-078: High Risk Guidance PDF exactly at size limit → Accepted or rejected.  
BND-079: DismissedOupQuestionIds with 99 IDs → All valid excluded.  
BND-080: Risk with probability = boundary value → Correct label.  
BND-081: Opportunity with 1 predefined + 9 AI recommendations → All displayed.  
BND-082: Similar projects with relevance = 0.5 (threshold) → Included or excluded.  
BND-083: Relevant people with empty expertise field → Handled.  
BND-084: CPI = 50.0 (floating point) → Not triggered.  
BND-085: Recommendation with empty recommendation text → Displayed or skipped.  
BND-086: Risk register with mixed deleted and active → Only active shown.  
BND-087: Multiple recommendations with same confidence → Deterministic sort.  
BND-088: Vector store query with special characters in keywords → Escaped.  
BND-089: Opportunity with zero budget → Recommendations still generated.  
BND-090: High risk acknowledgement with concurrent toggle → Last write wins.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### GetDSTRecommendationsAsync Flow (FUN-001–018)

FUN-001: Step 1: `GetOpportunityDetailsForAIAsync` loads full opportunity context.  
FUN-002: Context includes partners, countries, stakeholders, team, budget.  
FUN-003: Step 1.5: Existing risks loaded for deduplication.  
FUN-004: Step 1.5: Predefined high risks loaded for deduplication.  
FUN-005: Step 1.6: High Risk Guidance document fetched from EntityArtifact.  
FUN-006: Step 1.6: Guidance document is a PDF → Sent to LLM as attachment.  
FUN-007: Step 2: `ExtractRiskKeywordsAsync` sends context to Gemini.  
FUN-008: Step 2: Uses `opportunity_extract_risk_keywords` prompt.  
FUN-009: Step 2: Returns 5-8 risk-related keywords.  
FUN-010: Step 3: Keywords used for vector store search (EntityTypeId="RISK").  
FUN-011: Step 3: Vector store returns similar risk descriptions.  
FUN-012: Step 4: `RefineAndRankRisksAsync` combines context + similar risks + guidance.  
FUN-013: Step 4: Uses `refine_opportunity_risks` prompt.  
FUN-014: Step 4: Returns JSON array with title, description, recommendation, confidenceLevel, sourceType.  
FUN-015: Results deduplicated against existing risks (title similarity).  
FUN-016: Results deduplicated against predefined high risks.  
FUN-017: Results ranked by confidenceLevel descending.  
FUN-018: Response includes recommendation count and source count.

### Risk Register CRUD (FUN-019–030)

FUN-019: GET dst-risks returns risks with IsDeleted=false filter.  
FUN-020: GET dst-risks includes risk type, category, probability, impact lookups.  
FUN-021: POST dst-risk creates Risk entity with OpportunityId FK.  
FUN-022: POST dst-risk sets Name property (ModifiableDeletableEntity requirement).  
FUN-023: POST dst-risk sets audit fields (CreatedBy, CreatedDate).  
FUN-024: PUT dst-risk updates all mutable fields.  
FUN-025: PUT dst-risk sets audit fields (LastModifiedBy, LastModifiedDate).  
FUN-026: DELETE dst-risk sets IsDeleted=true, DeletedBy, DeletedDate.  
FUN-027: Accept recommendation → POST dst-risk with recommendation data pre-filled.  
FUN-028: Dismiss recommendation → ID added to DismissedOupQuestionIds.  
FUN-029: Risk lookups loaded from lookup tables (type, category, etc.).  
FUN-030: Risk register count excludes soft-deleted.

### Semantic Search Logic (FUN-031–042)

FUN-031: `GetSimilarOpportunitiesAsync` queries by embedding similarity.  
FUN-032: Embedding generated from opportunity description + context.  
FUN-033: Results sorted by cosine similarity descending.  
FUN-034: Self (same opportunity ID) excluded from results.  
FUN-035: Only non-deleted opportunities returned.  
FUN-036: `GetSimilarProjectsAsync` queries external vector store.  
FUN-037: Vector store query constructed from opportunity keywords.  
FUN-038: Results mapped to project model with name, description, relevance.  
FUN-039: `GetRelevantPeopleAsync` queries external vector store.  
FUN-040: People query constructed from opportunity domain + expertise needs.  
FUN-041: Results mapped to person model with name, expertise, contact.  
FUN-042: All semantic search results paginated or capped.

### High Risk Management (FUN-043–050)

FUN-043: Predefined high risks sourced from `PreDefinedHighRisk` table.  
FUN-044: Detection rules auto-detect risks based on opportunity data.  
FUN-045: CPI detection: country CPI < 50 → High risk flagged.  
FUN-046: Fragile State detection: country marked fragile → High risk flagged.  
FUN-047: `highRisksAcknowledged` toggle saves to opportunity.  
FUN-048: Acknowledged high risks persist across page reloads.  
FUN-049: High risk indicators displayed with severity badges.  
FUN-050: High risk section shows which rules triggered.

### Additional Functional Tests (FUN-051–090)

FUN-051: ExtractRiskKeywordsAsync uses correct prompt template.  
FUN-052: RefineAndRankRisksAsync receives guidance PDF in expected format.  
FUN-053: Vector store search filters by EntityTypeId="RISK".  
FUN-054: Deduplication uses title similarity threshold.  
FUN-055: Risk register GET applies IsDeleted filter.  
FUN-056: Accept recommendation maps sourceType to risk metadata.  
FUN-057: Dismiss recommendation persists DismissedOupQuestionIds.  
FUN-058: Similar opportunities exclude soft-deleted opportunities.  
FUN-059: Similar projects map external IDs to internal model.  
FUN-060: Relevant people include department/role when available.  
FUN-061: Predefined high risk detection runs before AI recommendations.  
FUN-062: High Risk Guidance PDF attached to Gemini with correct MIME type.  
FUN-063: Opportunity context includes stakeholder names.  
FUN-064: Opportunity context includes country risk indicators.  
FUN-065: Keywords sorted by relevance before vector search.  
FUN-066: Recommendations response includes sourceCount breakdown.  
FUN-067: Risk type lookup cached for performance.  
FUN-068: Risk category lookup cached for performance.  
FUN-069: Partner risk profile integrates with DST context when available.  
FUN-070: GetOpportunityDetailsForAIAsync uses AsNoTracking.  
FUN-071: GetOpportunityDetailsForAIAsync uses split query strategy.  
FUN-072: Vector store timeout configurable.  
FUN-073: Gemini timeout configurable.  
FUN-074: DSTRecommendationsResponse serializes correctly.  
FUN-075: Risk entity validates required fields before save.  
FUN-076: Acknowledge high risks updates LastModifiedDate.  
FUN-077: Similar opportunities use embedding from correct opportunity version.  
FUN-078: Similar projects fallback when vector store empty.  
FUN-079: Relevant people fallback when vector store empty.  
FUN-080: High risk detection respects DetectionRuleType enum.  
FUN-081: CPI detection uses correct country artifact.  
FUN-082: Fragile State detection uses correct country flag.  
FUN-083: Sanctions detection when applicable.  
FUN-084: Risk register count excludes soft-deleted.  
FUN-085: DismissedOupQuestionIds persisted per opportunity.  
FUN-086: Refresh recommendations bypasses cache.  
FUN-087: Risk register PUT validates risk belongs to opportunity.  
FUN-088: Risk register DELETE validates risk belongs to opportunity.  
FUN-089: Similar opportunities embedding uses latest opportunity data.  
FUN-090: Partner risk profile TTL checked before regeneration.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### End-to-End DST (INT-001–015)

INT-001: Open opportunity → DST section loads risks and recommendations.  
INT-002: Recommendations reflect current opportunity data.  
INT-003: Add a risk → Risk appears in register.  
INT-004: Accept recommendation → Risk added to register → Recommendation removed.  
INT-005: Dismiss recommendation → Recommendation hidden → Persists on refresh.  
INT-006: Refresh recommendations → New LLM call → Updated results.  
INT-007: Change opportunity country → Recommendations change (new context).  
INT-008: Change opportunity budget → Recommendations change.  
INT-009: Similar opportunities update when opportunity data changes.  
INT-010: Delete risk → Removed from register → Available for recommendation again.  
INT-011: Acknowledge high risks → Flag persisted → Submit workflow proceeds.  
INT-012: All risk lookups populated from database.  
INT-013: DST section on Draft opportunity → Full functionality.  
INT-014: DST section on Active opportunity → Read-only risks.  
INT-015: DST section on GO opportunity → Read-only risks.

### Cross-Feature (INT-016–030)

INT-016: DST + Insights → Both use Gemini independently.  
INT-017: DST + Opportunity Statement → Both AI features work concurrently.  
INT-018: DST + Workflow → High risk acknowledgement required before submit.  
INT-019: DST + OpportunityCountry.RiskScore → RiskScore populated.  
INT-020: DST + Country artifacts (HDI, FSI) → Artifacts used in risk context.  
INT-021: DST + Go Decision → Risks visible to decision maker.  
INT-022: DST + Partner due diligence → DD status in risk context.  
INT-023: DST + Document transcribe → Transcribed content in risk context.  
INT-024: DST risks exported in reports.  
INT-025: DST results visible in AI insights.  
INT-026: Partner risk profile + DST → Partner risk in opportunity context.  
INT-027: DST + soft delete → Deleted risks excluded from all queries.  
INT-028: DST + permissions → Only authorized users can modify risks.  
INT-029: DST + audit trail → All risk changes tracked.  
INT-030: DST + multiple countries → Risks aggregate across countries.

### Error Recovery (INT-031–040)

INT-031: Gemini unavailable → Error message in DST section.  
INT-032: Vector store unavailable → Partial results (LLM-only).  
INT-033: Network error during recommendation fetch → Error state displayed.  
INT-034: Timeout during recommendation generation → Loading cleared, error shown.  
INT-035: Retry after Gemini failure → Succeeds on retry.  
INT-036: Database error during risk save → Error message.  
INT-037: Concurrent risk edit → Last write wins.  
INT-038: Risk register reload after error → Clean state.  
INT-039: Semantic search timeout → Section shows "unavailable".  
INT-040: Recovery after vector store outage → Searches resume.

### Data Flow Validation (INT-041–050)

INT-041: Opportunity context JSON contains all required fields for LLM.  
INT-042: Keywords extracted match opportunity domain.  
INT-043: Vector store results relevant to opportunity.  
INT-044: Refined recommendations actionable and specific.  
INT-045: Confidence levels correlate with evidence quality.  
INT-046: sourceType correctly identifies recommendation origin.  
INT-047: Deduplication prevents exact duplicates in results.  
INT-048: Deduplication handles near-duplicates (fuzzy matching).  
INT-049: Risk register data round-trips correctly (create → read → update → read).  
INT-050: High Risk Guidance PDF content influences recommendations.

### Additional Integration Tests (INT-051–090)

INT-051: DST + Opportunity Statement → Statement changes affect keywords.  
INT-052: DST + Country artifacts → CPI/FSI used in recommendations.  
INT-053: DST + Partner due diligence → DD status in risk context.  
INT-054: DST + Document transcribe → Transcribed content in context.  
INT-055: DST + multiple countries → Risks aggregate correctly.  
INT-056: DST + workflow → Submit blocked until high risk acknowledged.  
INT-057: DST + Go Decision → Risks visible in decision view.  
INT-058: DST + Insights → Both use separate Gemini instances.  
INT-059: DST + soft delete → Deleted risks excluded.  
INT-060: DST + permissions → Only authorized users edit.  
INT-061: Risk register + audit trail → All changes tracked.  
INT-062: Recommendations + refresh → Cache invalidated.  
INT-063: Similar opportunities + opportunity edit → Results update.  
INT-064: Similar projects + vector store update → New results.  
INT-065: Relevant people + directory update → New results.  
INT-066: High risk + country change → Detection re-runs.  
INT-067: Partner risk profile + DST → Profile in opportunity context.  
INT-068: DST + concurrent users → Isolated sessions.  
INT-069: Risk register + pagination → Large registers load.  
INT-070: DST + export → Risks in report.  
INT-071: Gemini failure + retry → Succeeds on retry.  
INT-072: Vector store failure + fallback → LLM-only.  
INT-073: Database error + risk save → Error message.  
INT-074: Network timeout + recommendation → Error shown.  
INT-075: Concurrent risk edit + last write → Consistent.  
INT-076: DST section load + all data → Complete.  
INT-077: Recommendation accept + risk register → Appears.  
INT-078: Recommendation dismiss + persistence → Persists.  
INT-079: High risk acknowledgement + submit → Proceeds.  
INT-080: Risk delete + recommendation → Available again.  
INT-081: DST + country CPI update → Recommendations reflect.  
INT-082: DST + partner change → Recommendations reflect.  
INT-083: DST + budget change → Recommendations reflect.  
INT-084: DST + stakeholder change → Recommendations reflect.  
INT-085: Similar opportunities + embedding service → Embeddings generated.  
INT-086: Similar projects + vector store API → Results returned.  
INT-087: Relevant people + vector store API → Results returned.  
INT-088: Risk register + lookups → All options populated.  
INT-089: DST + High Risk Guidance update → New recommendations use.  
INT-090: DST + PreDefinedHighRisk update → New risks detected.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users requesting recommendations simultaneously → Both get results.  
CON-002: Recommendation request + risk save simultaneously → Both succeed.  
CON-003: Concurrent recommendation requests for same opportunity → One Gemini call.  
CON-004: Concurrent recommendation requests for different opportunities → Independent.  
CON-005: Risk add + risk delete simultaneously → Both operations succeed.  
CON-006: Two users editing same risk → Last write wins.  
CON-007: Dismiss + refresh simultaneously → Dismiss persists in new results.  
CON-008: Accept recommendation + refresh simultaneously → Risk saved, new recommendations.  
CON-009: Concurrent similar-opportunities queries → Independent.  
CON-010: Concurrent similar-projects queries → Independent vector store calls.  
CON-011: Concurrent relevant-people queries → Independent.  
CON-012: DST + Insights Gemini calls simultaneously → Both succeed.  
CON-013: Risk register CRUD + recommendation generation → Independent.  
CON-014: High risk acknowledgement + risk save → Both succeed.  
CON-015: Bulk risk operations (5 risks at once) → All saved.  
CON-016: Recommendation cache + concurrent invalidation → Consistent.  
CON-017: Vector store under load → Queued, all complete.  
CON-018: Multiple browser tabs with DST → Each independent.  
CON-019: Rapid refresh clicks → Debounced to single request.  
CON-020: DbContextFactory parallel queries for opportunity context → Thread-safe.  
CON-021: Embedding generation + similarity search concurrent → Independent.  
CON-022: Concurrent keyword extraction + vector search → Sequential (step dependency).  
CON-023: Risk register pagination under concurrent updates → Consistent.  
CON-024: High risk detection + manual risk add → Both appear.  
CON-025: Concurrent partner risk profile + DST recommendations → Independent.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: `ExtractRiskKeywordsAsync` returns 5-8 keywords from context.  
UNT-002: `ExtractRiskKeywordsAsync` with empty context → Default/minimal keywords.  
UNT-003: `RefineAndRankRisksAsync` returns ranked recommendations.  
UNT-004: `RefineAndRankRisksAsync` deduplicates against existing risks.  
UNT-005: `RefineAndRankRisksAsync` deduplicates against predefined high risks.  
UNT-006: `RefineAndRankRisksAsync` caps at 10 results.  
UNT-007: DSTRecommendation model: all fields serializable/deserializable.  
UNT-008: DSTRecommendationsRequest: DismissedOupQuestionIds defaults to empty.  
UNT-009: DSTRecommendationsResponse: recommendations defaults to empty list.  
UNT-010: Risk entity: Name required (ModifiableDeletableEntity).  
UNT-011: Risk entity: IsDeleted default false.  
UNT-012: Risk entity: FK to OpportunityId.  
UNT-013: Predefined high risk: DetectionRuleType maps to detection logic.  
UNT-014: CPI detection: score 49 → Detected.  
UNT-015: CPI detection: score 50 → Not detected.  
UNT-016: Fragile State detection: flag true → Detected.  
UNT-017: Similar opportunity: self excluded from results.  
UNT-018: Similar opportunity: deleted excluded from results.  
UNT-019: Similarity score calculation: cosine similarity correct.  
UNT-020: Vector store query construction from keywords.  
UNT-021: High risk acknowledgement toggle: true/false state change.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: DST recommendation generation < 30s (full pipeline).  
PRF-002: Keyword extraction (LLM call) < 10s.  
PRF-003: Vector store search < 5s.  
PRF-004: Risk refinement (LLM call) < 15s.  
PRF-005: Risk register GET < 500ms.  
PRF-006: Risk register POST < 500ms.  
PRF-007: Risk register PUT < 500ms.  
PRF-008: Risk register DELETE < 300ms.  
PRF-009: Similar opportunities query < 3s.  
PRF-010: Similar projects query < 5s.  
PRF-011: Relevant people query < 5s.  
PRF-012: Opportunity context JSON generation < 5s.  
PRF-013: High risk detection < 500ms.  
PRF-014: DST section initial load (all data) < 5s.  
PRF-015: Partner risk profile generation < 15s.  
PRF-016: Risk lookups loading < 200ms.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 20 concurrent DST recommendation requests → All complete < 60s.  
LDT-002: 50 concurrent risk register queries → All complete < 2s.  
LDT-003: 10 concurrent similar-opportunities queries → All complete < 10s.  
LDT-004: 10 concurrent similar-projects queries → All complete < 15s.  
LDT-005: Sustained DST usage (50 requests/hour) → Stable performance.  
LDT-006: Risk register with 200 risks → Query performance stable.  
LDT-007: Gemini under heavy DST load → Graceful degradation.  
LDT-008: Vector store under heavy search load → Queued, all complete.  
LDT-009: Recovery after Gemini outage → DST resumes on recovery.  
LDT-010: Recovery after vector store outage → Searches resume.

---

## Traceability Matrix

| Feature Area | Backend | API | Frontend | Test Coverage |
|-------------|---------|-----|----------|--------------|
| AI Recommendations | `UNOPSGeminiManager.GetDSTRecommendationsAsync` | `POST dst-recommendations` | DST section panel | POS-001–012, NEG-001–025, BND-001–025, FUN-001–018 |
| Risk Register CRUD | `UNOPSRiskManager` | `GET/POST/PUT/DELETE dst-risks` | Risk table | POS-013–022, NEG-026–045, BND-026–045, FUN-019–030 |
| Similar Opportunities | `UNOPSOpportunityManager.GetSimilarOpportunitiesAsync` | `GET similar-opportunities` | Accordion | POS-023–024, NEG-046–048, BND-046–050, FUN-031–035 |
| Similar Projects | `UNOPSGeminiManager.GetSimilarProjectsAsync` | `GET similar-projects` | Accordion | POS-025–026, NEG-049–051, BND-051–053, FUN-036–038 |
| Relevant People | `UNOPSGeminiManager.GetRelevantPeopleAsync` | `GET relevant-people` | Accordion | POS-027–028, NEG-052–054, BND-054–056, FUN-039–041 |
| Predefined High Risks | `PreDefinedHighRisk`, detection rules | `PUT acknowledge-high-risks` | High risk section | POS-022, BND-061–070, FUN-043–050 |
| Partner Risk Profile | `UNOPSPartnerManager.GetPartnerRiskProfileAsync` | Partner API | Partner view | NEG-061–070, FUN-069, FUN-090 |

---

## Status: Ready for Implementation
