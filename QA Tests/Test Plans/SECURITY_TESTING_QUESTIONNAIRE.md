# Security Testing Questionnaire

**Project:** UNOPS Opportunity+ Partnership Management System  
**Status:** Requirements Gathering  
**Created:** February 16, 2026  
**Updated:** February 17, 2026  
**Owner:** External Security Team  
**Audience:** Development Team, Architecture Team, Security Team, Operations Team

---

## Purpose

This questionnaire captures the information needed from stakeholders before implementing security tests for the UNOPS Opportunity+ system. The answers will directly inform test scope, attack simulations, compliance requirements, and tooling decisions.

> **Note:** Performance and Load Testing are handled separately by the QA Team (see `PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md`). This document covers **Security Testing** only.

---

## Scope Disclaimer

**Security Testing is out of scope for the QA Testing Team.**

Security testing for the UNOPS Opportunity+ system will **not** be performed by the project QA team. It will instead be handled in various ways by the **Infrastructure** and **Security** teams as necessary, which may include:

- **Internal security reviews** conducted by the Infrastructure / DevOps team (e.g., IAP configuration, CORS hardening, network policies, Cloud Run ingress rules)
- **Automated security scanning** integrated into CI/CD pipelines by the Infrastructure team (e.g., dependency vulnerability scanning, secret detection)
- **Dedicated security assessments** performed by a separate Security team or external firm (e.g., penetration testing, OWASP Top 10 coverage, SAST/DAST)
- **Compliance audits** coordinated by Security and Management as required by UN/UNOPS standards

The QA Team has prepared this questionnaire based on its analysis of the codebase and architecture to assist the responsible teams in planning their security testing activities. The QA Team remains available to coordinate on shared test environments and provide technical context as needed.

---

## Team Assignment Summary

Each section is tagged with the team(s) best positioned to answer. Use this table to quickly find your sections.

| Team | Sections to Complete | Questions |
|------|---------------------|-----------|
| **Security** | A1 (co-own), A5 (co-own), A7 (own), B (co-own), C (co-own) | Q1-5 (co-own), Q21-25 (co-own), Q26-30 (own), Q31-39 (co-own), Q40-41 |
| **Development** | A1 (co-own), A2 (co-own), A3 (co-own), A4 (co-own), A5 (co-own), A6 (co-own), B (co-own) | Q1-5, Q6-10, Q11-13, Q14-18, Q21-25 (co-own), Q19-20, Q31-39 (co-own) |
| **Architecture** | A4 (co-own), B (co-own), C (co-own) | Q14-18 (co-own), Q31-39 (co-own), Q40-41 (co-own) |
| **Operations** | A2 (co-own), C (co-own) | Q6-10 (co-own), Q40-41 (co-own) |
| **Management** | A7 (co-own) | Q26-30 |
| **All Stakeholders** | C, D | Q40-41, Q42-44 |

---

## How to Respond

- Fill in answers inline (replace `__________` blanks, check boxes, add notes)
- If you don't know an answer, write **"Unknown"** -- partial answers are still valuable
- Flag any questions that need a follow-up meeting with **"[MEETING NEEDED]"**
- If a question isn't applicable, write **"N/A"** with a brief reason
- **Return by:** `[DATE TBD]`

---

## Section A: Security Testing Areas

This section covers the key attack surfaces and security concerns identified in the UNOPS Opportunity+ system.

---

### A1. Dynamic LINQ Injection (Highest Risk)

> *Context: `GenericRowFilterService` has 6 security layers documented in `docs/Security/SecurityMeasures.md`: input validation (1000 char limit, 10 nesting levels), parameter processing, expression filtering, property whitelist, execution config (`AllowNewToEvaluateAnyType = false`), and runtime validation. This is the highest-risk attack surface because it evaluates user-influenced expressions.*

**For: Development / Security**

1. Have the 6 security layers in `GenericRowFilterService` ever been tested with adversarial inputs (fuzzing)?
    - [ ] Yes -- When: __________ Results: __________
    - [ ] No

2. How were the limits chosen (1000 char max, 10 nesting levels)? Were they based on threat modeling?
    - [ ] Threat modeling -- Details: __________
    - [ ] Based on industry standards
    - [ ] Arbitrary / best guess

3. Who maintains the entity property and LINQ method whitelist? What is the review process when new entries are added?
    - Maintainer: __________
    - Review process: __________

4. Has anyone verified that `AllowNewToEvaluateAnyType = false` actually blocks type instantiation in all code paths (not just the main filter path)?
    - [ ] Yes -- Verified by: __________
    - [ ] No

5. Can we run active fuzzing/injection tests against the Dynamic LINQ filter endpoints?
    - [ ] Yes, go ahead
    - [ ] Yes, but only in [environment]: __________
    - [ ] No -- Reason: __________

---

### A2. IAP Authentication

> *Context: Production uses Google Cloud IAP with JWT verification via `IAPVerificationMiddleware`. Development uses `DevelopmentIAPAuthHandler` with a `DevIAPAuth` cookie. Headers `X-Goog-Authenticated-User-Email` and `X-Goog-IAP-JWT-Assertion` carry identity.*

**For: Development / Operations / Security**

6. Is the `DevelopmentIAPAuthHandler` / `DevelopmentLoginPageMiddleware` code excluded from production builds, or is it present but disabled by environment?
    - [ ] Excluded from production build (conditional compilation)
    - [ ] Present but disabled by environment config
    - [ ] Unsure
    - What mechanism prevents it from activating in production? __________

7. Can the Cloud Run service be accessed directly via its `.run.app` URL, bypassing IAP?
    - [ ] No -- Ingress restricted to internal + IAP only
    - [ ] Yes -- it is publicly accessible (compensating controls: __________)
    - [ ] Unsure

8. What happens if Google's JWKS endpoint (for IAP JWT verification) is unreachable?
    - [ ] Cached keys used -- Cache TTL: __________
    - [ ] Requests fail (deny by default)
    - [ ] Requests pass through (fail-open)
    - [ ] Unsure

9. Is the `X-Goog-Authenticated-User-Email` header validated to ensure it can only come from IAP (not from a direct request)?
    - [ ] Yes -- Validation mechanism: __________
    - [ ] No
    - [ ] Unsure

10. Are there any other authentication bypass risks identified?
    - Answer: __________

---

### A3. CORS Configuration

> *Context: CORS is currently configured as `AllowAnyOrigin(), AllowAnyHeader(), AllowAnyMethod()` in `Program.cs`/`Startup.cs`.*

**For: Development / Security**

11. Is the `AllowAll` CORS policy intentional for production?
    - [ ] Yes -- Reason: __________
    - [ ] No -- it should be restricted to: __________
    - [ ] Only for development -- production has different config at: __________

12. If intentional, what compensating controls prevent cross-origin abuse (e.g., IAP restricts access anyway)?
    - Answer: __________

13. Has the CORS configuration been reviewed by a security team?
    - [ ] Yes -- When: __________ Findings: __________
    - [ ] No

---

### A4. Rate Limiting

> *Context: `RateLimitingTests.cs` exists but is excluded from the build. No app-level rate limiting middleware was found in Startup/Program.*

**For: Development / Architecture**

14. Was rate limiting implemented and then removed, or was it never completed?
    - [ ] Implemented then removed -- Reason: __________
    - [ ] Never completed -- Priority: __________
    - [ ] Handled at infrastructure level (IAP / Cloud Run / load balancer)
    - Details: __________

15. Without app-level rate limiting, what prevents abuse of expensive endpoints (AI chat, advanced search, bulk operations)?
    - Answer: __________

16. Are there rate limits at the IAP or Cloud Run level?
    - [ ] Yes -- Limits: __________
    - [ ] No
    - [ ] Unsure

17. What is the risk assessment for rate limiting gaps?
    - [ ] High -- critical endpoints exposed
    - [ ] Medium -- infrastructure mitigates most risk
    - [ ] Low -- user base is internal/trusted
    - Notes: __________

18. Is there a plan or timeline to implement app-level rate limiting?
    - [ ] Yes -- Timeline: __________
    - [ ] No
    - [ ] Under consideration

---

### A5. Authorization & RBAC Bypass

> *Context: Row-level security uses `@currentUserId` and `@userOrgUnit` parameters in Dynamic LINQ expressions. Property filters use JSON whitelists. `EntityPermissions` table controls CRUD access per role.*

**For: Development / Security**

19. Are the `@currentUserId` and `@userOrgUnit` parameters in row filters sourced exclusively from the authenticated IAP identity, or could they be influenced by request parameters?
    - [ ] Sourced from IAP identity only
    - [ ] Could be influenced by request -- Details: __________

20. Can `EntityPermissions` records be modified at runtime by administrators? If so, what audit trail exists?
    - [ ] Yes, modifiable at runtime -- Audit: __________
    - [ ] No, configuration-only

21. Are there test scenarios for permission escalation (e.g., `PARTNER_USER` attempting `ORG_UNIT_ADMIN` operations)?
    - [ ] Yes -- Test location: __________
    - [ ] No

22. Has horizontal privilege escalation been tested (e.g., User A accessing User B's data)?
    - [ ] Yes -- Results: __________
    - [ ] No

23. Are there any known gaps in the RBAC implementation?
    - Answer: __________

---

### A6. File Upload Security

> *Context: Document upload validates "PDF only" for GCS uploads. Storage uses signed URLs.*

**For: Development / Security**

24. Does upload validation check file content (magic bytes), or only the file extension?
    - [ ] Magic byte validation
    - [ ] Extension only
    - [ ] Both
    - [ ] Unsure

25. Is there antivirus or malware scanning on uploaded files before they reach GCS?
    - [ ] Yes -- Tool: __________
    - [ ] No

26. Can signed URLs for GCS objects be shared with unauthorized users (i.e., are they bearer tokens)?
    - [ ] Yes, anyone with the URL can access the file
    - [ ] No, additional auth is required
    - Signed URL expiration: __________

27. Are there file size limits enforced at the application level?
    - [ ] Yes -- Limit: __________ MB
    - [ ] No

28. Is there content-type validation on the server side (not just client-side)?
    - [ ] Yes
    - [ ] No
    - [ ] Unsure

---

### A7. Security Compliance & Scope

**For: Security / Management**

29. What security standards must this system meet?
    - [ ] ISO 27001
    - [ ] SOC 2
    - [ ] GDPR
    - [ ] UN/UNOPS-specific standards -- Name: __________
    - [ ] Other: __________

30. Do we need external (third-party) penetration testing certification?
    - [ ] Yes -- Frequency: __________
    - [ ] No
    - [ ] Unsure -- who decides? __________

31. Have there been previous security audits?
    - [ ] Yes -- When: __________ Findings: __________
    - [ ] No

32. Can we perform active attack simulations (attempted exploitation), or should tests be passive (detection only)?
    - [ ] Active testing approved
    - [ ] Active testing approved in [environment] only: __________
    - [ ] Passive only
    - Are there off-limits attack vectors? __________

33. What is the vulnerability disclosure process? Who should be notified of security findings?
    - Process: __________
    - Notify: __________

---

## Section B: Tooling Decisions

> *Context: The application is built with .NET 9, Angular 19, PostgreSQL, Google Cloud Run, and IAP. These questions help the Security Team decide what tools to use.*

**For: Architecture / Development / Security**

34. For automated security scanning, what level is needed?
    - [ ] C# xUnit tests only (test specific injection/bypass scenarios)
    - [ ] OWASP ZAP in CI (automated crawl + scan)
    - [ ] Manual Burp Suite testing (periodic)
    - [ ] All of the above
    - [ ] Other: __________

35. For dependency vulnerability scanning, what do we need?
    - [ ] `dotnet list package --vulnerable` in CI (free, built-in)
    - [ ] Snyk (comprehensive, includes transitive deps)
    - [ ] GitHub Dependabot (already available)
    - [ ] Other: __________

36. Do compliance requirements mandate any specific tools or certifications?
    - [ ] Yes -- Required: __________
    - [ ] No
    - [ ] Unsure

37. Is there a SAST (Static Application Security Testing) tool already in use?
    - [ ] Yes -- Tool: __________
    - [ ] No
    - [ ] Under evaluation

38. Is there a DAST (Dynamic Application Security Testing) tool already in use?
    - [ ] Yes -- Tool: __________
    - [ ] No
    - [ ] Under evaluation

39. What is the budget for security tooling (if any)?
    - [ ] Use only free/open-source tools
    - [ ] Budget available: __________
    - [ ] Need to discuss

---

### What Can Be Tested Now vs. What Needs Additional Tools

**For: Security (informational -- no response needed)**

| Test Type | Can Do Now (Existing Infrastructure) | Needs Additional Tools |
|-----------|--------------------------------------|----------------------|
| Dynamic LINQ injection tests | ✅ xUnit tests with adversarial payloads | -- |
| IAP header spoofing tests | ✅ xUnit integration tests | -- |
| RBAC bypass / permission escalation | ✅ xUnit tests per role + entity | -- |
| XSS / unauthorized navigation | ✅ Playwright spec files | -- |
| CORS misconfiguration testing | ✅ xUnit / Playwright | -- |
| File upload validation bypass | ✅ xUnit with crafted files | -- |
| Dependency vulnerability scan | ✅ `dotnet list package --vulnerable` | Snyk for deep transitive scanning |
| Automated vulnerability crawling | ❌ | OWASP ZAP |
| Manual penetration testing | ❌ | Burp Suite or external firm |
| SAST (static code analysis) | ❌ | SonarQube, Semgrep, or Snyk Code |
| Secret detection in codebase | ❌ | GitLeaks, TruffleHog, or GitHub Secret Scanning |

---

## Section C: Prioritization

> *Based on the system architecture and identified attack surfaces, the following phased approach is recommended. Please confirm or adjust.*

**For: All Stakeholders**

### Proposed Phase 1 -- Critical Path (Highest Risk)

**For: Security / Development**

- [ ] Dynamic LINQ injection testing (fuzzing and adversarial payloads)
- [ ] IAP authentication bypass testing (header spoofing, JWT manipulation)
- [ ] RBAC / row-level security bypass testing (privilege escalation, horizontal access)
- [ ] CORS policy review and hardening
- [ ] Dependency vulnerability scanning (known CVEs)

### Proposed Phase 2 -- Extended Coverage

**For: Security / Development / Architecture**

- [ ] Rate limiting assessment and recommendations
- [ ] File upload security (content validation, malware scanning)
- [ ] Signed URL security review (expiration, scope)
- [ ] SAST integration in CI pipeline
- [ ] Secret detection in codebase and CI

### Proposed Phase 3 -- Comprehensive

**For: Security / External Firm (if required)**

- [ ] Full OWASP Top 10 coverage
- [ ] External (third-party) penetration testing (if compliance requires)
- [ ] DAST integration (OWASP ZAP or equivalent)
- [ ] Security incident response plan validation
- [ ] Compliance audit preparation

**For: Security / Architecture**

40. Do you agree with this prioritization?
    - [ ] Yes
    - [ ] No -- Adjustments: __________

41. Are there other high-risk security areas not listed above?
    - Answer: __________

---

## Section D: Timeline & Coordination

**For: All Stakeholders**

42. What is the target timeline for each security testing phase?
    - Phase 1: __________
    - Phase 2: __________
    - Phase 3: __________

43. How should the Security Team coordinate with the QA Team (who handles Performance & Load testing)?
    - [ ] Joint test environment -- share access
    - [ ] Separate environments
    - [ ] Regular sync meetings -- Frequency: __________
    - [ ] Other: __________

44. Who is the primary contact for the Security Team?
    - Name: __________
    - Email: __________
    - Role: __________

---

## Response Tracking

| Section | Target Respondent | Date Sent | Date Received | Status |
|---------|-------------------|-----------|---------------|--------|
| A1. Dynamic LINQ Injection | Development / Security | | | Pending |
| A2. IAP Authentication | Development / Operations / Security | | | Pending |
| A3. CORS Configuration | Development / Security | | | Pending |
| A4. Rate Limiting | Development / Architecture | | | Pending |
| A5. Authorization & RBAC Bypass | Development / Security | | | Pending |
| A6. File Upload Security | Development / Security | | | Pending |
| A7. Security Compliance & Scope | Security / Management | | | Pending |
| B. Tooling Decisions | Architecture / Development / Security | | | Pending |
| C. Prioritization | All Stakeholders | | | Pending |
| D. Timeline & Coordination | All Stakeholders | | | Pending |

---

**Please return completed sections to:** Security Team / QA Team  
**Deadline:** `[TBD]`  
**Questions or clarifications:** Contact Security Team at `[EMAIL/CHANNEL TBD]`

---

*This questionnaire was prepared based on analysis of the UNOPS Opportunity+ codebase, infrastructure configuration, and identified attack surfaces. Performance and Load Testing are managed separately by the QA Team (see `PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md`).*
