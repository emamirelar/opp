# Performance, Security, and Load Testing Questionnaire

**Status:** Requirements Gathering  
**Created:** February 16, 2026  
**Owner:** QA Team  
**Audience:** Development Team, Architecture Team, Security Team, Operations Team

---

## How to Use This Document

This questionnaire captures the specific information QA needs before implementing performance, security, and load tests. Questions are organized by test category and targeted at the team best positioned to answer.

**Instructions for respondents:**
- Fill in answers inline (replace blanks, check boxes, add notes)
- If you don't know, write "Unknown" -- partial answers are valuable
- Flag any questions that need a follow-up meeting
- Return by: **[DATE TBD]**

---

## A. Performance Testing

### A1. Dynamic LINQ & Row Filter Performance

> *Context: The `GenericRowFilterService` evaluates Dynamic LINQ expressions via `PermissionService.EvaluateFilterOnEntity<T>()` on every data access request for RBAC row filtering. Advanced search also uses Dynamic LINQ for dynamic ordering. No performance baselines exist for this code path.*

**For: Development / Architecture**

1. What is the acceptable overhead for row-filter evaluation per query?
   - Answer: __________ ms

2. For a search across 10,000+ partners with multi-column sort and RBAC filtering, what is the acceptable total response time?
   - Answer: __________ ms

3. Has anyone profiled the `PermissionService.EvaluateFilterOnEntity<T>()` method under realistic data volumes?
   - [ ] Yes -- Results: __________
   - [ ] No

4. Are there known scenarios where row filter evaluation becomes a bottleneck?
   - Answer: __________

---

### A2. EF Core & PostgreSQL

> *Context: PostgreSQL connection pool is configured at `MinPoolSize=10, MaxPoolSize=100, CommandTimeout=60s`. The `DbContextFactory` is used for parallel queries. The `GetOpportunityDetailsForAIAsync` method was previously optimized from 310s to 32-63s by splitting queries and adding `AsNoTracking()`.*

**For: Development**

5. Has the connection pool (`MaxPoolSize=100`) ever been exhausted under load? What monitoring exists for pool saturation?
   - [ ] Yes, exhaustion observed -- Details: __________
   - [ ] No known exhaustion
   - [ ] No monitoring in place
   - Monitoring tools in use: __________

6. Are there known queries that approach the 60-second `CommandTimeout`? Which manager methods are the slowest?
   - Answer: __________

7. Besides `GetOpportunityDetailsForAIAsync`, are there other manager methods with similar Cartesian product or N+1 issues that haven't been optimized yet?
   - [ ] Yes -- Methods: __________
   - [ ] No, all critical paths optimized
   - [ ] Unknown

8. How many manager methods currently have 5+ `Include()` chains that haven't been split into separate queries?
   - Answer: __________

---

### A3. AI Integration Latency

> *Context: AI calls go through `UNOPSGeminiManager` to Vertex AI (Gemini). Embeddings use `text-embedding-005` via `AiContextualService`. PubSub handles async embedding creation.*

**For: Development / Architecture**

9. What is the current P95 latency for Vertex AI (Gemini) calls? Is there a timeout configured?
   - P95 latency: __________ ms
   - Timeout: __________ seconds / [ ] No timeout configured

10. How many embeddings are generated per user session on average?
    - Answer: __________

11. What is the acceptable PubSub queue depth and processing delay for async embedding creation?
    - Max queue depth: __________
    - Max acceptable delay: __________

12. Is there circuit-breaking or graceful degradation if Vertex AI becomes slow or unavailable?
    - [ ] Yes -- Mechanism: __________
    - [ ] No

---

### A4. Document Operations (GCS)

> *Context: Documents are stored in Google Cloud Storage with signed URLs. Upload path is `{entityType}/{entityId}/{guid}_{filename}`. PDF-only validation exists for GCS uploads.*

**For: Development / Operations**

13. What is the largest document uploaded to GCS in production? What is the P95 document size?
    - Largest: __________ MB
    - P95 size: __________ MB

14. Are signed URLs cached or generated per-request? What is the expiration time?
    - [ ] Cached -- TTL: __________
    - [ ] Generated per-request
    - Expiration: __________

15. How many documents exist per entity on average and at maximum?
    - Average: __________
    - Maximum: __________

---

### A5. Cloud Run & Infrastructure

> *Context: Application is deployed on Google Cloud Run with IAP in front. Cold starts may affect performance.*

**For: Operations / Architecture**

16. What is the Cloud Run autoscaling configuration?
    - Min instances: __________
    - Max instances: __________
    - Concurrency per instance: __________

17. What is the measured cold start time for a new Cloud Run instance?
    - Answer: __________ seconds

18. Is there a minimum instance count configured to avoid cold starts?
    - [ ] Yes -- Count: __________
    - [ ] No

19. Do you have a dedicated performance test environment separate from QA/staging?
    - [ ] Yes -- Environment: __________
    - [ ] No -- Which environment should we use? __________

20. Can we use production-like data volumes for testing?
    - [ ] Yes
    - [ ] No -- Reason: __________
    - Approximate production data size:
      - Partners: __________
      - Opportunities: __________
      - Documents: __________
      - Users: __________

---

### A6. Performance SLAs

> *Please define acceptable response times. If no SLA exists, write "No SLA -- suggest one" and QA will propose baselines after initial testing.*

**For: Architecture / Product**

| Operation | Acceptable Response Time |
|---|---|
| Partner search (simple, <100 results) | __________ ms |
| Partner search (complex, filters + RBAC) | __________ ms |
| Advanced search with Dynamic LINQ | __________ ms |
| Opportunity detail page (all related data) | __________ seconds |
| Document upload (10 MB PDF) | __________ seconds |
| Document download (signed URL generation) | __________ ms |
| AI chat response (first token) | __________ seconds |
| Bulk import throughput | __________ records/second |
| Bulk export throughput | __________ records/second |
| Dashboard page load | __________ seconds |

---

## B. Security Testing

### B1. Dynamic LINQ Injection (Highest Risk)

> *Context: `GenericRowFilterService` has 6 security layers documented in `docs/Security/SecurityMeasures.md`: input validation (1000 char limit, 10 nesting levels), parameter processing, expression filtering, property whitelist, execution config (`AllowNewToEvaluateAnyType = false`), and runtime validation. This is the highest-risk attack surface because it evaluates user-influenced expressions.*

**For: Development / Security**

21. Have the 6 security layers in `GenericRowFilterService` ever been tested with adversarial inputs (fuzzing)?
    - [ ] Yes -- When: __________ Results: __________
    - [ ] No

22. How were the limits chosen (1000 char max, 10 nesting levels)? Were they based on threat modeling?
    - [ ] Threat modeling -- Details: __________
    - [ ] Based on industry standards
    - [ ] Arbitrary / best guess

23. Who maintains the entity property and LINQ method whitelist? What is the review process when new entries are added?
    - Maintainer: __________
    - Review process: __________

24. Has anyone verified that `AllowNewToEvaluateAnyType = false` actually blocks type instantiation in all code paths (not just the main filter path)?
    - [ ] Yes -- Verified by: __________
    - [ ] No

25. Can we run active fuzzing/injection tests against the Dynamic LINQ filter endpoints?
    - [ ] Yes, go ahead
    - [ ] Yes, but only in [environment]: __________
    - [ ] No -- Reason: __________

---

### B2. IAP Authentication

> *Context: Production uses Google Cloud IAP with JWT verification via `IAPVerificationMiddleware`. Development uses `DevelopmentIAPAuthHandler` with a `DevIAPAuth` cookie. Headers `X-Goog-Authenticated-User-Email` and `X-Goog-IAP-JWT-Assertion` carry identity.*

**For: Development / Operations / Security**

26. Is the `DevelopmentIAPAuthHandler` / `DevelopmentLoginPageMiddleware` code excluded from production builds, or is it present but disabled by environment?
    - [ ] Excluded from production build (conditional compilation)
    - [ ] Present but disabled by environment config
    - [ ] Unsure
    - What mechanism prevents it from activating in production? __________

27. Can the Cloud Run service be accessed directly via its `.run.app` URL, bypassing IAP?
    - [ ] No -- Ingress restricted to internal + IAP only
    - [ ] Yes -- it is publicly accessible (compensating controls: __________)
    - [ ] Unsure

28. What happens if Google's JWKS endpoint (for IAP JWT verification) is unreachable?
    - [ ] Cached keys used -- Cache TTL: __________
    - [ ] Requests fail (deny by default)
    - [ ] Requests pass through (fail-open)
    - [ ] Unsure

29. Is the `X-Goog-Authenticated-User-Email` header validated to ensure it can only come from IAP (not from a direct request)?
    - [ ] Yes -- Validation mechanism: __________
    - [ ] No
    - [ ] Unsure

---

### B3. CORS Configuration

> *Context: CORS is currently configured as `AllowAnyOrigin(), AllowAnyHeader(), AllowAnyMethod()` in `Program.cs`/`Startup.cs`.*

**For: Development / Security**

30. Is the `AllowAll` CORS policy intentional for production?
    - [ ] Yes -- Reason: __________
    - [ ] No -- it should be restricted to: __________
    - [ ] Only for development -- production has different config at: __________

31. If intentional, what compensating controls prevent cross-origin abuse (e.g., IAP restricts access anyway)?
    - Answer: __________

---

### B4. Rate Limiting

> *Context: `RateLimitingTests.cs` exists but is excluded from the build. No app-level rate limiting middleware was found in Startup/Program.*

**For: Development / Architecture**

32. Was rate limiting implemented and then removed, or was it never completed?
    - [ ] Implemented then removed -- Reason: __________
    - [ ] Never completed -- Priority: __________
    - [ ] Handled at infrastructure level (IAP / Cloud Run / load balancer)
    - Details: __________

33. Without app-level rate limiting, what prevents abuse of expensive endpoints (AI chat, advanced search, bulk operations)?
    - Answer: __________

34. Are there rate limits at the IAP or Cloud Run level?
    - [ ] Yes -- Limits: __________
    - [ ] No
    - [ ] Unsure

---

### B5. Authorization & RBAC Bypass

> *Context: Row-level security uses `@currentUserId` and `@userOrgUnit` parameters in Dynamic LINQ expressions. Property filters use JSON whitelists. `EntityPermissions` table controls CRUD access per role.*

**For: Development / Security**

35. Are the `@currentUserId` and `@userOrgUnit` parameters in row filters sourced exclusively from the authenticated IAP identity, or could they be influenced by request parameters?
    - [ ] Sourced from IAP identity only
    - [ ] Could be influenced by request -- Details: __________

36. Can `EntityPermissions` records be modified at runtime by administrators? If so, what audit trail exists?
    - [ ] Yes, modifiable at runtime -- Audit: __________
    - [ ] No, configuration-only

37. Are there test scenarios for permission escalation (e.g., `PARTNER_USER` attempting `ORG_UNIT_ADMIN` operations)?
    - [ ] Yes -- Test location: __________
    - [ ] No

---

### B6. File Upload Security

> *Context: Document upload validates "PDF only" for GCS uploads. Storage uses signed URLs.*

**For: Development / Security**

38. Does upload validation check file content (magic bytes), or only the file extension?
    - [ ] Magic byte validation
    - [ ] Extension only
    - [ ] Both
    - [ ] Unsure

39. Is there antivirus or malware scanning on uploaded files before they reach GCS?
    - [ ] Yes -- Tool: __________
    - [ ] No

40. Can signed URLs for GCS objects be shared with unauthorized users (i.e., are they bearer tokens)?
    - [ ] Yes, anyone with the URL can access the file
    - [ ] No, additional auth is required
    - Signed URL expiration: __________

---

### B7. Security Compliance & Scope

**For: Security / Management**

41. What security standards must this system meet?
    - [ ] ISO 27001
    - [ ] SOC 2
    - [ ] GDPR
    - [ ] UN/UNOPS-specific standards -- Name: __________
    - [ ] Other: __________

42. Do we need external (third-party) penetration testing certification?
    - [ ] Yes -- Frequency: __________
    - [ ] No
    - [ ] Unsure -- who decides? __________

43. Have there been previous security audits?
    - [ ] Yes -- When: __________ Findings: __________
    - [ ] No

44. Can we perform active attack simulations (attempted exploitation), or should tests be passive (detection only)?
    - [ ] Active testing approved
    - [ ] Active testing approved in [environment] only: __________
    - [ ] Passive only
    - Are there off-limits attack vectors? __________

45. What is the vulnerability disclosure process? Who should be notified of security findings?
    - Process: __________
    - Notify: __________

---

## C. Load Testing

### C1. Cloud Run Scaling Under Load

> *Context: Cloud Run autoscales instances. Each instance has a PostgreSQL connection pool of `MaxPoolSize=100`. Multiple instances could collectively exceed Cloud SQL connection limits.*

**For: Operations / Architecture**

46. What is the Cloud SQL maximum connection limit?
    - Answer: __________

47. Is PgBouncer or Cloud SQL Auth Proxy connection pooling in use?
    - [ ] PgBouncer
    - [ ] Cloud SQL Auth Proxy
    - [ ] Neither -- each Cloud Run instance manages its own pool
    - Max connections across all instances: __________

48. With multiple Cloud Run instances each using `MaxPoolSize=100`, what prevents the total connections from exceeding the Cloud SQL limit?
    - Answer: __________

---

### C2. User Load & Traffic Profile

**For: Operations / Product / Architecture**

49. How many concurrent users do you expect?
    - Normal operations: __________
    - Peak hours: __________
    - Maximum capacity target: __________

50. What is the peak usage time/period?
    - Answer: __________

51. What is the current production user count?
    - Active users (monthly): __________
    - Registered users (total): __________

52. What is the expected annual growth rate?
    - Users: __________% per year
    - Data volume: __________% per year

53. What percentage of requests are read vs. write in production?
    - Read: __________% / Write: __________%
    - [ ] Unknown -- can we check access logs?

54. What are the top 5-10 most-called API endpoints by volume?
    - 1. __________
    - 2. __________
    - 3. __________
    - 4. __________
    - 5. __________
    - [ ] Unknown -- can we check access logs?

55. Are there known "thundering herd" scenarios (e.g., all users logging in at 9 AM, batch notifications)?
    - Answer: __________

---

### C3. External Dependencies Under Load

**For: Architecture / Development**

56. When the system is under load and Vertex AI calls slow down or fail, what happens?
    - [ ] Circuit breaker / graceful degradation -- Mechanism: __________
    - [ ] Requests queue and eventually timeout
    - [ ] Errors propagate to the user
    - [ ] Unsure

57. Are there quotas or rate limits on the GCS bucket that could bottleneck under load?
    - [ ] Yes -- Limits: __________
    - [ ] No
    - [ ] Unsure

58. What is the PubSub message throughput limit, and what is the maximum acceptable backlog?
    - Throughput: __________ messages/second
    - Max backlog: __________

---

### C4. Acceptance Criteria

**For: Architecture / Product**

59. What response time is acceptable under load?

    | Condition | Acceptable Response Time |
    |---|---|
    | Normal load (__________ concurrent users) | __________ seconds |
    | Peak load (__________ concurrent users) | __________ seconds |

60. What error rate is acceptable under load?
    - [ ] < 0.1%
    - [ ] < 0.5%
    - [ ] < 1%
    - [ ] Other: __________%

61. What is the maximum acceptable downtime?
    - Answer: __________

62. What is the Recovery Time Objective (RTO)?
    - Answer: __________

---

### C5. Test Execution Logistics

**For: All Teams**

63. When should load tests run?
    - [ ] Before each deployment
    - [ ] Weekly
    - [ ] Nightly
    - [ ] On-demand only
    - [ ] Other: __________

64. Should load test failures block deployments?
    - [ ] Yes
    - [ ] No
    - [ ] Only for critical regressions (>X% degradation)

65. Who should be notified of load test failures?
    - Answer: __________

66. What is the test data refresh strategy?
    - [ ] Generate synthetic data for each run
    - [ ] Use anonymized production snapshot
    - [ ] Use static test dataset
    - [ ] Other: __________

---

## D. Tool Selection

> *Context: We already have xUnit + `PerformanceTestBase` with `Stopwatch` measurements, Playwright for E2E, and a CI pipeline in GitHub Actions. The questions below help decide what additional tools, if any, we need.*

**For: Architecture / Development / QA**

67. For C# micro-benchmarks, should we continue with `Stopwatch`-based tests or invest in BenchmarkDotNet for statistical rigor?
    - [ ] Continue with Stopwatch (simpler, good enough)
    - [ ] Adopt BenchmarkDotNet (better for regression detection)
    - [ ] Other: __________

68. For load testing, which tool fits best?
    - [ ] NBomber (C# native, fits our existing test stack)
    - [ ] k6 (JavaScript, modern, good Cloud Run support)
    - [ ] Apache JMeter (team already has experience)
    - [ ] Other: __________
    - Team familiarity: __________

69. For automated security scanning, what level is needed?
    - [ ] C# xUnit tests only (test specific injection/bypass scenarios)
    - [ ] OWASP ZAP in CI (automated crawl + scan)
    - [ ] Manual Burp Suite testing (periodic)
    - [ ] All of the above
    - [ ] Other: __________

70. For dependency vulnerability scanning, what do we need?
    - [ ] `dotnet list package --vulnerable` in CI (free, built-in)
    - [ ] Snyk (comprehensive, includes transitive deps)
    - [ ] GitHub Dependabot (already available)
    - [ ] Other: __________

71. Do compliance requirements mandate any specific tools or certifications?
    - [ ] Yes -- Required: __________
    - [ ] No
    - [ ] Unsure

---

## E. Prioritization

> *Based on the system architecture, QA recommends the following priority order. Please confirm or adjust.*

**For: All Stakeholders**

**Proposed Phase 1 -- Critical Path (highest risk):**
- Dynamic LINQ injection testing (security)
- IAP authentication bypass testing (security)
- RBAC/row-level security bypass testing (security)
- Partner search performance under RBAC filtering (performance)
- Opportunity detail page load time (performance)
- Concurrent user load at expected peak (load)

**Proposed Phase 2 -- Extended Coverage:**
- CORS policy review and testing (security)
- Rate limiting implementation/testing (security)
- File upload security (security)
- Document upload/download performance (performance)
- AI service latency and timeout testing (performance)
- Bulk operations under load (load)

**Proposed Phase 3 -- Comprehensive:**
- Full OWASP Top 10 coverage (security)
- External penetration testing if required (security)
- All manager method performance baselines (performance)
- Stress testing beyond capacity (load)
- 24-hour soak testing (load)

72. Do you agree with this prioritization?
    - [ ] Yes
    - [ ] No -- Adjustments: __________

73. Are there other high-risk areas not listed above?
    - Answer: __________

---

## F. What Cursor/Claude Can Do vs. External Tools Needed

> *For stakeholder awareness: this section summarizes what QA can implement immediately using our existing infrastructure versus what requires additional tools or services.*

### Implementable Now (C# xUnit + Playwright + Cursor/Claude)

| Test Type | Approach | Infrastructure |
|---|---|---|
| Dynamic LINQ injection tests | xUnit tests with adversarial payloads | Existing `UNOPS.PAO.Business.Tests` |
| IAP header spoofing tests | xUnit integration tests | Existing `UNOPS.PAO.Presentation.Tests` |
| RBAC bypass / permission escalation | xUnit tests per role + entity | Existing `ConcurrencyTestBase` |
| XSS / unauthorized navigation | Playwright spec files | Existing Playwright suite |
| Manager method performance | xUnit with `PerformanceTestBase` | Existing `MeasureAsync()` pattern |
| Concurrent data access | xUnit with `ConcurrencyTestBase` | Existing per-thread DbContext pattern |
| Dependency vulnerability scan | `dotnet list package --vulnerable` | GitHub Actions workflow |

### Requires Additional Tools

| Test Type | Tool | Why It's Needed |
|---|---|---|
| HTTP load generation (concurrent users) | NBomber or k6 | Generating real network traffic from multiple threads against a running server |
| Automated vulnerability crawling | OWASP ZAP | Discovering unknown attack surfaces by crawling the live API |
| Statistical micro-benchmarks | BenchmarkDotNet | Warmup, multiple iterations, memory diagnostics, regression detection |
| Transitive dependency scanning | Snyk or Dependabot | Deep CVE detection across dependency tree |
| Certified penetration testing | External firm | If UN/UNOPS compliance requires third-party certification |

74. Which additional tools should we proceed with?
    - [ ] NBomber (load testing)
    - [ ] k6 (load testing)
    - [ ] OWASP ZAP (security scanning)
    - [ ] BenchmarkDotNet (micro-benchmarks)
    - [ ] Snyk (dependency scanning)
    - [ ] External pen testing firm
    - [ ] None for now -- start with what we can do in xUnit/Playwright
    - [ ] Other: __________

---

## Response Tracking

| Section | Respondent | Date Received | Status |
|---|---|---|---|
| A. Performance (A1-A6) | Development / Architecture | | Pending |
| B. Security (B1-B7) | Development / Security | | Pending |
| C. Load Testing (C1-C5) | Operations / Architecture | | Pending |
| D. Tool Selection | All teams | | Pending |
| E. Prioritization | All stakeholders | | Pending |
| F. Tool Decisions | All stakeholders | | Pending |

**Please return completed sections to: QA Team**  
**Deadline: [TBD]**  
**Questions? Contact: [QA Team contact]**
