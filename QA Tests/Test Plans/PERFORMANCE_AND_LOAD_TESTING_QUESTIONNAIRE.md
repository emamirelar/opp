# Performance & Load Testing Questionnaire

**Project:** UNOPS Opportunity+ Partnership Management System  
**Status:** Requirements Gathering  
**Created:** February 17, 2026  
**Owner:** QA Team  
**Audience:** Development Team, Architecture Team, Operations Team, Product Team

---

## Purpose

This questionnaire captures the information QA needs from stakeholders before implementing performance and load tests. The answers will directly inform test design, threshold configuration, and tooling decisions.

> **Note:** Security testing is out of scope for QA and will be handled by a separate team. This document covers **Performance** and **Load Testing** only.

---

## Team Assignment Summary

Each section and subsection is tagged with the team(s) best positioned to answer. Use this table to quickly find your sections.

| Team | Sections to Complete | Questions |
|------|---------------------|-----------|
| **Development** | A1 (co-own), A2 (co-own), A3 (own), A4 (co-own), A5 (co-own), B4 (co-own), D (co-own), E Phase 1 (co-own), E Phase 3 (co-own) | Q1-18 (co-own), Q19-23 (co-own), Q24-29 (own), Q30-35 (co-own), Q36-39 (co-own), Q54-57 (co-own), Q72-75 (co-own) |
| **Architecture** | A1 (co-own), A2 (co-own), A4 (co-own), B2 (co-own), B3 (co-own), B4 (co-own), B5 (co-own), C1 (co-own), D (co-own), E Phase 2 (co-own), E Phase 3 (co-own), E Q76-77 (co-own) | Q1-18 (co-own), Q19-23 (co-own), Q30-35 (co-own), Q47-53 (co-own), Q54-57 (co-own), Q58-62 (co-own), Q63-67 (co-own), Q72-75 (co-own), Q76-77 |
| **Operations** | A5 (co-own), B1 (co-own), B2 (co-own), B3 (co-own), C1 (co-own), C2 (co-own), E Phase 2 (co-own), E Phase 3 (co-own), E Q78 (co-own) | Q36-39 (co-own), Q40-46 (co-own), Q47-53 (co-own), Q63-71 (co-own), Q78 |
| **Product** | A1 (co-own), B1 (co-own), B5 (co-own), E Q76-78 (co-own) | Q1-18 (co-own), Q40-46 (co-own), Q58-62 (co-own), Q76-78 |
| **QA** | D (co-own), E Phase 1 (co-own), E Phase 2 (co-own), E Phase 3 (co-own) | Q72-75 (co-own), Phases 1-3 execution |
| **All Teams** | C2 | Q68-71 |

---

## How to Respond

- Fill in answers inline (replace `__________` blanks, check boxes, add notes)
- If you don't know an answer, write **"Unknown"** -- partial answers are still valuable
- Flag any questions that need a follow-up meeting with **"[MEETING NEEDED]"**
- If a question isn't applicable, write **"N/A"** with a brief reason
- **Return by:** `[DATE TBD]`

---

## Section A: Performance Testing

This section establishes acceptable response times, identifies known bottlenecks, and defines the performance baseline QA will test against.

---

### A1. Application Response Time Targets (SLAs)

> *QA needs defined thresholds to write pass/fail assertions. If no SLA exists today, write "No SLA" and QA will propose a baseline after initial profiling.*

**For: Product / Architecture / Development**

| # | Operation | Acceptable Response Time | Current Measured Time (if known) |
|---|-----------|--------------------------|----------------------------------|
| 1 | Partner list page load (default view, <100 results) | __________ ms | __________ ms |
| 2 | Partner search with filters + RBAC row filtering | __________ ms | __________ ms |
| 3 | Advanced search with Dynamic LINQ + multi-column sort | __________ ms | __________ ms |
| 4 | Opportunity detail page (all sections, all related data) | __________ seconds | __________ seconds |
| 5 | Opportunity list page load (default view) | __________ ms | __________ ms |
| 6 | Contact list page load | __________ ms | __________ ms |
| 7 | Dashboard page load (all widgets) | __________ seconds | __________ seconds |
| 8 | Document upload (10 MB PDF to GCS) | __________ seconds | __________ seconds |
| 9 | Document download (signed URL generation) | __________ ms | __________ ms |
| 10 | AI chat response (time to first token) | __________ seconds | __________ seconds |
| 11 | AI opportunity statement generation | __________ seconds | __________ seconds |
| 12 | Workflow submit (with all 21 requirement validations) | __________ seconds | __________ seconds |
| 13 | Workflow approve/reject action | __________ seconds | __________ seconds |
| 14 | Bulk import throughput | __________ records/second | __________ records/second |
| 15 | Bulk export throughput | __________ records/second | __________ records/second |
| 16 | User login (IAP → application session) | __________ seconds | __________ seconds |

**Additional questions:**

17. Are there operations not listed above that have known performance concerns?
    - Answer: __________

18. Should API response times and UI page load times have different SLAs?
    - [ ] Yes -- API: __________ ms / UI: __________ ms
    - [ ] No, same threshold for both
    - [ ] Unsure

---

### A2. Dynamic LINQ & Row Filter Performance

> *Context: The `GenericRowFilterService` evaluates Dynamic LINQ expressions via `PermissionService.EvaluateFilterOnEntity<T>()` on every data access request for RBAC row filtering. Advanced search also uses Dynamic LINQ for dynamic ordering. No performance baselines currently exist for this code path.*

**For: Development / Architecture**

19. What is the acceptable overhead for row-filter evaluation per query?
    - Answer: __________ ms

20. For a search across 10,000+ partners with multi-column sort and RBAC filtering, what is the acceptable total response time?
    - Answer: __________ ms

21. Has anyone profiled the `PermissionService.EvaluateFilterOnEntity<T>()` method under realistic data volumes?
    - [ ] Yes -- Results: __________
    - [ ] No

22. Are there known scenarios where row filter evaluation becomes a bottleneck?
    - Answer: __________

23. How many distinct RBAC filter expressions are typically active at once per user?
    - Answer: __________

---

### A3. EF Core & PostgreSQL Performance

> *Context: PostgreSQL connection pool is configured at `MinPoolSize=10, MaxPoolSize=100, CommandTimeout=60s`. The `DbContextFactory` is used for parallel queries. The `GetOpportunityDetailsForAIAsync` method was previously optimized from 310s to 32-63s by splitting queries and adding `AsNoTracking()`.*

**For: Development**

24. Has the connection pool (`MaxPoolSize=100`) ever been exhausted under load? What monitoring exists for pool saturation?
    - [ ] Yes, exhaustion observed -- Details: __________
    - [ ] No known exhaustion
    - [ ] No monitoring in place
    - Monitoring tools in use: __________

25. Are there known queries that approach the 60-second `CommandTimeout`? Which manager methods are the slowest?
    - Answer: __________

26. Besides `GetOpportunityDetailsForAIAsync`, are there other manager methods with similar Cartesian product or N+1 issues that haven't been optimized yet?
    - [ ] Yes -- Methods: __________
    - [ ] No, all critical paths optimized
    - [ ] Unknown

27. How many manager methods currently have 5+ `Include()` chains that haven't been split into separate queries?
    - Answer: __________

28. Is `AsNoTracking()` consistently applied to all read-only queries across the codebase?
    - [ ] Yes, all read-only queries use AsNoTracking
    - [ ] Mostly, but some may be missing
    - [ ] Unknown -- needs audit

29. Are there any known deadlock or lock contention issues with PostgreSQL under concurrent writes?
    - Answer: __________

---

### A4. AI Integration Latency

> *Context: AI calls go through `UNOPSGeminiManager` to Vertex AI (Gemini). Embeddings use `text-embedding-005` via `AiContextualService`. PubSub handles async embedding creation.*

**For: Development / Architecture**

30. What is the current P95 latency for Vertex AI (Gemini) calls? Is there a timeout configured?
    - P95 latency: __________ ms
    - Timeout: __________ seconds / [ ] No timeout configured

31. How many embeddings are generated per user session on average?
    - Answer: __________

32. What is the acceptable PubSub queue depth and processing delay for async embedding creation?
    - Max queue depth: __________
    - Max acceptable delay: __________ seconds

33. Is there circuit-breaking or graceful degradation if Vertex AI becomes slow or unavailable?
    - [ ] Yes -- Mechanism: __________
    - [ ] No -- what happens to the user? __________

34. What is the acceptable time for the AI to generate an opportunity statement?
    - Answer: __________ seconds

35. Is Vertex AI usage metered/billed per call? Are there cost concerns with performance testing at volume?
    - [ ] Yes, billed per call -- Estimated cost per 1000 calls: __________
    - [ ] No cost concerns
    - [ ] Unsure

---

### A5. Document Operations (GCS)

> *Context: Documents are stored in Google Cloud Storage with signed URLs. Upload path is `{entityType}/{entityId}/{guid}_{filename}`. PDF-only validation exists for GCS uploads.*

**For: Development / Operations**

36. What is the largest document uploaded to GCS in production? What is the P95 document size?
    - Largest: __________ MB
    - P95 size: __________ MB

37. Are signed URLs cached or generated per-request? What is the expiration time?
    - [ ] Cached -- TTL: __________
    - [ ] Generated per-request
    - Expiration: __________

38. How many documents exist per entity on average and at maximum?
    - Average: __________
    - Maximum: __________

39. Is there a file size limit enforced at the application level?
    - [ ] Yes -- Limit: __________ MB
    - [ ] No

---

## Section B: Load Testing

This section establishes expected traffic patterns, capacity targets, and infrastructure constraints that determine how QA designs load test scenarios.

---

### B1. User Load & Traffic Profile

**For: Operations / Product / Architecture**

40. How many concurrent users do you expect?

    | Scenario | Concurrent Users |
    |----------|-----------------|
    | Normal operations | __________ |
    | Peak hours | __________ |
    | Maximum capacity target | __________ |

41. What is the peak usage time/period?
    - Answer: __________

42. What is the current production user count?
    - Active users (monthly): __________
    - Registered users (total): __________

43. What is the expected annual growth rate?
    - Users: __________% per year
    - Data volume: __________% per year

44. What percentage of requests are read vs. write in production?
    - Read: __________% / Write: __________%
    - [ ] Unknown -- can we check access logs?

45. What are the top 5-10 most-called API endpoints by volume?

    | Rank | Endpoint | Estimated % of Traffic |
    |------|----------|----------------------|
    | 1 | __________ | __________ |
    | 2 | __________ | __________ |
    | 3 | __________ | __________ |
    | 4 | __________ | __________ |
    | 5 | __________ | __________ |
    | 6 | __________ | __________ |
    | 7 | __________ | __________ |
    | 8 | __________ | __________ |
    | 9 | __________ | __________ |
    | 10 | __________ | __________ |
    | | [ ] Unknown -- can we check access logs? | |

46. Are there known "thundering herd" scenarios (e.g., all users logging in at 9 AM, batch notifications, report generation)?
    - Answer: __________

---

### B2. Cloud Run & Infrastructure

> *Context: Application is deployed on Google Cloud Run with IAP in front. Cold starts may affect performance.*

**For: Operations / Architecture**

47. What is the Cloud Run autoscaling configuration?

    | Setting | Value |
    |---------|-------|
    | Min instances | __________ |
    | Max instances | __________ |
    | Concurrency per instance | __________ |
    | CPU allocation | __________ vCPUs |
    | Memory per instance | __________ GB |

48. What is the measured cold start time for a new Cloud Run instance?
    - Answer: __________ seconds

49. Is there a minimum instance count configured to avoid cold starts?
    - [ ] Yes -- Count: __________
    - [ ] No

---

### B3. Database Scaling Under Load

> *Context: Cloud Run autoscales instances. Each instance has a PostgreSQL connection pool of `MaxPoolSize=100`. Multiple instances could collectively exceed Cloud SQL connection limits.*

**For: Operations / Architecture**

50. What is the Cloud SQL maximum connection limit?
    - Answer: __________

51. Is PgBouncer or Cloud SQL Auth Proxy connection pooling in use?
    - [ ] PgBouncer -- Configuration: __________
    - [ ] Cloud SQL Auth Proxy
    - [ ] Neither -- each Cloud Run instance manages its own pool
    - Max connections across all instances: __________

52. With multiple Cloud Run instances each using `MaxPoolSize=100`, what prevents the total connections from exceeding the Cloud SQL limit?
    - Answer: __________

53. What is the Cloud SQL instance tier and resource allocation?
    - Tier: __________
    - vCPUs: __________
    - Memory: __________ GB
    - Storage: __________ GB
    - [ ] Unknown

---

### B4. External Dependencies Under Load

**For: Architecture / Development**

54. When the system is under load and Vertex AI calls slow down or fail, what happens?
    - [ ] Circuit breaker / graceful degradation -- Mechanism: __________
    - [ ] Requests queue and eventually timeout
    - [ ] Errors propagate to the user
    - [ ] Unsure

55. Are there quotas or rate limits on the GCS bucket that could bottleneck under load?
    - [ ] Yes -- Limits: __________
    - [ ] No
    - [ ] Unsure

56. What is the PubSub message throughput limit, and what is the maximum acceptable backlog?
    - Throughput: __________ messages/second
    - Max backlog: __________

57. Are there any other external services (email, SSO, third-party APIs) that could become bottlenecks?
    - Answer: __________

---

### B5. Load Test Acceptance Criteria

**For: Architecture / Product**

58. What response time is acceptable under load?

    | Condition | P50 Response Time | P95 Response Time | P99 Response Time |
    |-----------|-------------------|-------------------|-------------------|
    | Normal load (__________ concurrent users) | __________ ms | __________ ms | __________ ms |
    | Peak load (__________ concurrent users) | __________ ms | __________ ms | __________ ms |
    | Stress load (__________ concurrent users) | __________ ms | __________ ms | __________ ms |

59. What error rate is acceptable under load?
    - [ ] < 0.1% (high reliability)
    - [ ] < 0.5%
    - [ ] < 1%
    - [ ] Other: __________%

60. What is the maximum acceptable downtime per month?
    - [ ] 99.99% uptime (< 4.3 minutes/month)
    - [ ] 99.9% uptime (< 43.8 minutes/month)
    - [ ] 99.5% uptime (< 3.6 hours/month)
    - [ ] Other: __________

61. What is the Recovery Time Objective (RTO) after a failure?
    - Answer: __________

62. What is the Recovery Point Objective (RPO) for data loss?
    - Answer: __________

---

## Section C: Test Environment & Execution

This section covers the practical logistics of where and how performance and load tests will be executed.

---

### C1. Test Environment

**For: Operations / Architecture**

63. Do you have a dedicated performance test environment separate from QA/staging?
    - [ ] Yes -- Environment: __________
    - [ ] No -- Which environment should we use? __________

64. Can we use production-like data volumes for testing?
    - [ ] Yes
    - [ ] No -- Reason: __________

65. Approximate production data volumes (so we can seed the test environment):

    | Entity | Approximate Count |
    |--------|-------------------|
    | Partners | __________ |
    | Opportunities | __________ |
    | Contacts | __________ |
    | Interactions | __________ |
    | Documents | __________ |
    | Users | __________ |
    | Workflow history entries | __________ |

66. Is there an anonymized production database snapshot available for load testing?
    - [ ] Yes -- How to access: __________
    - [ ] No, but can be created
    - [ ] No, and cannot be created -- Reason: __________

67. Can load tests generate traffic against the Cloud Run deployment, or should they target a local/container-based deployment?
    - [ ] Against Cloud Run (preferred for realistic results)
    - [ ] Local/container only
    - [ ] Both
    - Constraints: __________

---

### C2. Test Execution Schedule & Process

**For: All Teams**

68. When should performance/load tests run?
    - [ ] Before each release to staging/production
    - [ ] Weekly (scheduled)
    - [ ] Nightly in CI/CD
    - [ ] On-demand only
    - [ ] Other: __________

69. Should performance/load test failures block deployments?
    - [ ] Yes, always
    - [ ] Only for critical regressions (> __________% degradation from baseline)
    - [ ] No, advisory only

70. Who should be notified of performance/load test results?

    | Scenario | Notify |
    |----------|--------|
    | Test passes (no regression) | __________ |
    | Minor degradation detected | __________ |
    | Critical failure / SLA breach | __________ |

71. What is the test data refresh strategy?
    - [ ] Generate synthetic data for each run
    - [ ] Use anonymized production snapshot (refreshed periodically)
    - [ ] Use static test dataset
    - [ ] Other: __________

---

## Section D: Tooling Decisions

> *Context: We currently have xUnit with `Stopwatch`-based performance measurements, Playwright for E2E, BenchmarkDotNet available as a dependency, and a CI pipeline in GitHub Actions. These questions help decide what additional tools, if any, we need for production-grade performance and load testing.*

**For: Architecture / Development / QA**

72. For C# micro-benchmarks (individual method performance), which approach should we use?
    - [ ] Continue with `Stopwatch`-based xUnit tests (simpler, good enough for regression detection)
    - [ ] Adopt BenchmarkDotNet (statistical rigor, warmup, memory diagnostics, better for baselines)
    - [ ] Both (Stopwatch for CI, BenchmarkDotNet for detailed profiling)
    - [ ] Other: __________

73. For HTTP load testing (simulating concurrent users), which tool fits best?
    - [ ] **NBomber** (C# native, integrates with existing test stack, xUnit compatible)
    - [ ] **k6** (JavaScript/TypeScript, modern, excellent Cloud Run support, Grafana integration)
    - [ ] **Apache JMeter** (mature, GUI-based, team already has experience)
    - [ ] **Locust** (Python, easy scripting, distributed load)
    - [ ] Other: __________
    - Team familiarity with these tools: __________

74. For performance monitoring during tests, what APM/observability tools are available?
    - [ ] Google Cloud Monitoring / Cloud Trace
    - [ ] Application Insights
    - [ ] Grafana + Prometheus
    - [ ] None currently
    - [ ] Other: __________

75. What is the budget for tooling (if any)?
    - [ ] Use only free/open-source tools
    - [ ] Budget available: __________
    - [ ] Need to discuss

---

### What QA Can Implement Now vs. What Needs Additional Tools

**For: QA (informational -- no response needed)**

| Test Type | Can Do Now (Existing Tools) | Needs Additional Tools |
|-----------|---------------------------|----------------------|
| Individual method response time | ✅ xUnit + `Stopwatch` / `PerformanceTestBase` | BenchmarkDotNet for statistical baselines |
| Page load time (E2E) | ✅ Playwright with performance timing | -- |
| Database query performance | ✅ xUnit + InMemory/PostgreSQL + Stopwatch | -- |
| API endpoint response time | ✅ xUnit + `HttpClient` + Stopwatch | -- |
| Concurrent user simulation | ❌ | NBomber or k6 |
| Sustained load over time | ❌ | NBomber or k6 |
| Spike/burst load | ❌ | NBomber or k6 |
| Cloud Run autoscaling validation | ❌ | NBomber/k6 + Cloud Monitoring |
| Connection pool exhaustion testing | ❌ | NBomber/k6 against live PostgreSQL |
| Memory leak detection (long-running) | Partial (xUnit can measure GC) | APM tool for production-like soak tests |

---

## Section E: Prioritization

> *Based on the system architecture, QA recommends the following phased approach. Please confirm or adjust.*

**For: All Stakeholders**

### Proposed Phase 1 -- Performance Baselines (Immediate)
*Establish measurable baselines using existing xUnit + Playwright tools. No new tooling required.*

**For: Development / QA**

- [ ] Partner search performance under RBAC filtering
- [ ] Opportunity detail page load time (all sections + related data)
- [ ] Dashboard page load time
- [ ] Workflow submit validation performance (21 requirements)
- [ ] AI chat/statement generation response time
- [ ] Document upload/download performance
- [ ] Key manager method execution times (top 10 slowest)

### Proposed Phase 2 -- Load Testing (After Tooling Decision)
*Requires load testing tool (NBomber/k6) and a dedicated or shared test environment.*

**For: Operations / Architecture / QA**

- [ ] Concurrent user load at expected peak
- [ ] Cloud Run autoscaling under gradual load increase
- [ ] Database connection pool behavior under load
- [ ] Read-heavy vs write-heavy traffic mix simulation
- [ ] PubSub/AI queue behavior under sustained load
- [ ] Error rate monitoring under load

### Proposed Phase 3 -- Stress & Endurance (Comprehensive)
*Extended testing for capacity planning and reliability.*

**For: Operations / Architecture / Development / QA**

- [ ] Stress testing beyond expected capacity (find breaking point)
- [ ] Spike testing (sudden traffic surge)
- [ ] Soak/endurance testing (24-hour sustained load)
- [ ] Recovery testing (behavior after load spike subsides)
- [ ] External dependency failure under load (Vertex AI, GCS, PubSub)
- [ ] Data growth impact simulation (2x, 5x, 10x current volume)

**For: Product / Architecture**

76. Do you agree with this prioritization?
    - [ ] Yes
    - [ ] No -- Adjustments: __________

77. Are there other high-risk performance areas not listed above?
    - Answer: __________

**For: Product / Operations**

78. What is the target timeline for each phase?
    - Phase 1: __________
    - Phase 2: __________
    - Phase 3: __________

---

## Response Tracking

| Section | Target Respondent | Date Sent | Date Received | Status |
|---------|-------------------|-----------|---------------|--------|
| A. Performance SLAs (A1) | Product / Architecture | | | Pending |
| A. Dynamic LINQ Performance (A2) | Development / Architecture | | | Pending |
| A. EF Core & PostgreSQL (A3) | Development | | | Pending |
| A. AI Integration Latency (A4) | Development / Architecture | | | Pending |
| A. Document Operations (A5) | Development / Operations | | | Pending |
| B. User Load & Traffic (B1) | Operations / Product | | | Pending |
| B. Cloud Run Infrastructure (B2) | Operations / Architecture | | | Pending |
| B. Database Scaling (B3) | Operations / Architecture | | | Pending |
| B. External Dependencies (B4) | Architecture / Development | | | Pending |
| B. Acceptance Criteria (B5) | Architecture / Product | | | Pending |
| C. Test Environment (C1) | Operations / Architecture | | | Pending |
| C. Execution Schedule (C2) | All Teams | | | Pending |
| D. Tooling Decisions | Architecture / Development / QA | | | Pending |
| E. Prioritization | All Stakeholders | | | Pending |

---

**Please return completed sections to:** QA Team  
**Deadline:** `[TBD]`  
**Questions or clarifications:** Contact QA Team at `[EMAIL/CHANNEL TBD]`

---

*This questionnaire was prepared by the QA Team based on analysis of the UNOPS Opportunity+ codebase, infrastructure configuration, and existing test coverage. Security testing is managed separately by `[SECURITY TEAM NAME TBD]`.*
