# Cloud Scheduler Deployment — Comprehensive Test Cases

**Component:** Google Cloud Scheduler — External Data Service Sync Jobs  
**Scripts:** `ExternalDataService/deploy/setup-cloud-scheduler.sh`, `setup-cloud-scheduler.ps1`  
**Config:** `ExternalDataService/deploy/cloud-scheduler-jobs.yaml`  
**Pipeline:** `deployments/CI-CD/Jenkinsfile-eds`, `jenkins-config-eds.yaml`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

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

### Scheduled Jobs (15 jobs)

| # | Job Name | Schedule | Endpoint |
|---|----------|----------|----------|
| 1 | `pao-external-data-users-daily` | 0 2 * * * | `/api/sync/execute/users` |
| 2 | `pao-external-data-user-profiles-daily` | 15 2 * * * | `/api/sync/execute/user-profiles` |
| 3 | `pao-external-data-user-roles-daily` | 30 2 * * * | `/api/sync/execute/user-roles` |
| 4 | `pao-external-data-countries-daily` | 45 2 * * * | `/api/sync/execute/countries` |
| 5 | `pao-external-data-currencies-daily` | 0 3 * * * | `/api/sync/execute/currencies` |
| 6 | `pao-external-data-engagements-daily` | 15 3 * * * | `/api/sync/execute/engagements` |
| 7 | `pao-external-data-engagement-partners-daily` | 30 3 * * * | `/api/sync/execute/engagement-partners` |
| 8 | `pao-external-data-partner-agreements-daily` | 45 3 * * * | `/api/sync/execute/partner-agreements` |
| 9 | `pao-external-data-org-hierarchies-daily` | 0 4 * * * | `/api/sync/execute/organization-hierarchies` |
| 10 | `pao-external-data-entity-user-roles-doa-daily` | 15 4 * * * | `/api/sync/execute/entity-user-roles-doa` |
| 11 | `pao-external-data-entity-user-roles-mgmt-daily` | 25 4 * * * | `/api/sync/execute/entity-user-roles-mgmt` |
| 12 | `pao-external-data-exchange-rates-daily` | 35 4 * * * | `/api/sync/execute/exchange-rates` |
| 13 | `pao-external-data-weekly-full-sync` | 0 1 * * 1 | `/api/sync/execute-all` |
| 14 | `pao-external-data-health-check` | */15 * * * * | `/health` |
| 15 | `pao-external-data-config-status` | 0 */4 * * * | `/api/sync/status` |

### Architecture

- **Cloud Scheduler** → HTTP POST → EDS (External Data Service) sync endpoints
- **Auth:** OIDC token via `cloud-scheduler-invoker` service account
- **Timezone:** `Europe/Copenhagen`
- **Region mapping:** `europe-west4` → `europe-west3` (scheduler limitation)
- **Jenkins:** Runs `setup-cloud-scheduler.sh` during deploy stage

### Environments

| Environment | Service URL | Invoker SA |
|-------------|------------|------------|
| Production | `https://eds.opportunityplus.unops.org` | `cloud-scheduler-invoker@unops-opportunityplus-prod.iam` |
| Test | `https://eds.opportunityplus.test.unops.org` | `...test.iam` |
| QA | `https://eds.opportunityplus.qa.unops.org` | `...qa.iam` |
| Dev | `https://eds.opportunityplus.dev.unops.org` | `...dev.iam` |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Script Execution (POS-001–010)

POS-001: `setup-cloud-scheduler.sh` executes without errors on Linux.  
POS-002: `setup-cloud-scheduler.ps1` executes without errors on Windows.  
POS-003: Script enables `cloudscheduler.googleapis.com` API.  
POS-004: Script creates `cloud-scheduler-invoker` service account.  
POS-005: Script grants Cloud Run invoker IAM role to service account.  
POS-006: Script creates all 15 scheduled jobs from YAML config.  
POS-007: Script deletes existing jobs before re-creating (idempotent).  
POS-008: Script maps `europe-west4` → `europe-west3` for scheduler region.  
POS-009: Script reads `OAuthClientId` from appsettings.  
POS-010: Script completes and lists all created jobs via `gcloud scheduler jobs list`.

### Job Configuration (POS-011–020)

POS-011: Each job has correct cron schedule per YAML.  
POS-012: Each job targets correct EDS endpoint.  
POS-013: Each job uses OIDC authentication.  
POS-014: Each job uses correct service account.  
POS-015: Each job uses correct `audience` (OAuth client ID).  
POS-016: Each job timezone set to `Europe/Copenhagen`.  
POS-017: Health check job runs every 15 minutes.  
POS-018: Config status job runs every 4 hours.  
POS-019: Weekly full sync runs Monday at 01:00.  
POS-020: Daily sync jobs staggered (15-minute intervals starting 02:00).

### Jenkins Pipeline (POS-021–030)

POS-021: Jenkins pipeline stage "Setup Cloud Scheduler Jobs" executes.  
POS-022: Pipeline reads `scheduler_enabled: true` from config.  
POS-023: Pipeline parses `cloud-scheduler-jobs.yaml` correctly.  
POS-024: Pipeline builds `SCHEDULER_COMMANDS` environment variable.  
POS-025: Pipeline invokes `setup-cloud-scheduler.sh` with correct arguments.  
POS-026: Pipeline uses `GOOGLE_APPLICATION_CREDENTIALS`.  
POS-027: Pipeline completes scheduler stage without errors.  
POS-028: Pipeline works for all 4 environments (prod, test, qa, dev).  
POS-029: Pipeline uses correct project ID per environment.  
POS-030: Pipeline uses correct region per environment.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Script Failures (NEG-001–020)

NEG-001: Script run without `project-id` argument → Error with usage message.  
NEG-002: Script run without `environment` argument → Error.  
NEG-003: Script run without `region` argument → Error.  
NEG-004: Script run with invalid project-id → `gcloud` error.  
NEG-005: Script run with invalid environment → Service URL malformed.  
NEG-006: Script run with invalid region → Region mapping fails.  
NEG-007: Script run without `gcloud` installed → Command not found.  
NEG-008: Script run without authentication → `gcloud` auth error.  
NEG-009: Script run with expired credentials → Auth token error.  
NEG-010: Script run with insufficient IAM permissions → Permission denied.  
NEG-011: Script run when Cloud Scheduler API disabled → API error.  
NEG-012: Script run when service account already exists → Handled (idempotent).  
NEG-013: Script run when jobs already exist → Deletes and recreates.  
NEG-014: Script interrupted mid-execution → Partial jobs created.  
NEG-015: Script run with malformed YAML config → YAML parse error.  
NEG-016: Script run with empty YAML config → No jobs created.  
NEG-017: Script run with missing YAML file → File not found error.  
NEG-018: Script run with read-only YAML → Permission error.  
NEG-019: OAuthClientId missing from appsettings → Script uses empty audience.  
NEG-020: OAuthClientId in wrong format → OIDC auth will fail at runtime.

### Job Configuration Failures (NEG-021–040)

NEG-021: Cron expression invalid → Job creation fails.  
NEG-022: Endpoint URL malformed → Job creation fails or HTTP error at runtime.  
NEG-023: Timezone invalid → Job creation fails.  
NEG-024: Service account doesn't exist → Job creation fails.  
NEG-025: Service account lacks invoker role → Job runs but EDS returns 403.  
NEG-026: Audience (OAuth client ID) incorrect → OIDC token rejected.  
NEG-027: Service URL incorrect → HTTP 404 at runtime.  
NEG-028: HTTP method wrong (GET instead of POST) → Endpoint may reject.  
NEG-029: Job name contains invalid characters → Creation fails.  
NEG-030: Job name too long → Creation fails.  
NEG-031: Duplicate job names → Conflict error.  
NEG-032: Job with 0-second retry → Invalid configuration.  
NEG-033: Job with negative retry count → Invalid.  
NEG-034: Job body empty when endpoint expects payload → Sync fails.  
NEG-035: Job body malformed JSON → Endpoint returns 400.  
NEG-036: Job timeout too short → Sync fails mid-execution.  
NEG-037: Job timeout too long → Resource waste.  
NEG-038: Health check endpoint returns non-200 → Job marked failed.  
NEG-039: Sync endpoint returns 500 → Job retried.  
NEG-040: Sync endpoint returns 401 → OIDC token issue.

### Pipeline Failures (NEG-041–055)

NEG-041: `scheduler_enabled: false` → Scheduler stage skipped.  
NEG-042: `scheduler_enabled` missing from config → Stage behavior undefined.  
NEG-043: YAML parse failure in Jenkinsfile → Stage fails.  
NEG-044: `GOOGLE_APPLICATION_CREDENTIALS` not set → Auth fails.  
NEG-045: `GOOGLE_APPLICATION_CREDENTIALS` points to wrong file → Auth fails.  
NEG-046: Credentials file expired → Auth fails.  
NEG-047: Jenkins agent without `gcloud` → Command not found.  
NEG-048: Jenkins agent without `yq` (YAML parser) → Parse fails.  
NEG-049: Network error from Jenkins to GCP → Timeout.  
NEG-050: GCP API rate limit hit → Retry needed.  
NEG-051: Jenkins pipeline timeout → Scheduler stage incomplete.  
NEG-052: Concurrent Jenkins runs for same environment → Race condition.  
NEG-053: Jenkins runs for different environments simultaneously → Independent.  
NEG-054: Jenkinsfile syntax error → Pipeline fails.  
NEG-055: jenkins-config-eds.yaml syntax error → Config load fails.

### Runtime Execution Failures (NEG-056–070)

NEG-056: EDS service down → Scheduled job returns 503.  
NEG-057: EDS service restarting → Job returns 503 or timeout.  
NEG-058: EDS database down → Sync endpoint returns 500.  
NEG-059: External data source unavailable → Sync returns partial error.  
NEG-060: OIDC token expired during job execution → 401 error.  
NEG-061: Job retries exceed max attempts → Job fails permanently.  
NEG-062: Job runs during EDS deployment → May fail or use old version.  
NEG-063: Network partition between scheduler and EDS → Timeout.  
NEG-064: DNS resolution failure → Connection error.  
NEG-065: SSL certificate expired on EDS → Connection refused.  
NEG-066: Cloud Scheduler service outage → No jobs triggered.  
NEG-067: Overlapping job executions → Same endpoint called twice.  
NEG-068: Health check fails → Indicates EDS is unhealthy.  
NEG-069: Config status returns degraded → Warning but not actionable.  
NEG-070: Full sync on Monday overlaps with daily syncs → Both run independently.

### Extended Negative (NEG-071–090)

NEG-071: Script run with invalid timezone format → Job creation fails.  
NEG-072: Script run with wrong script path → Execution fails.  
NEG-073: Job creation with duplicate endpoint in same schedule → May conflict.  
NEG-074: OAuthClientId contains forbidden characters → Token request fails.  
NEG-075: Cloud Run service not deployed → Invoker role binding fails.  
NEG-076: GCP project in suspended state → API calls fail.  
NEG-077: Service account key deleted → OIDC token generation fails.  
NEG-078: Cloud Scheduler quota exceeded → Job creation fails.  
NEG-079: Script run with symbolic link to YAML → Broken link error.  
NEG-080: Jenkins workspace path contains spaces → Script path error.  
NEG-081: gcloud config project set to wrong project → Jobs created in wrong project.  
NEG-082: Job body exceeds Cloud Scheduler payload limit → Creation fails.  
NEG-083: Endpoint URL contains invalid characters → URL parse error.  
NEG-084: Service account without project access → IAM binding fails.  
NEG-085: Cloud Run service in different region → Invoker binding may fail.  
NEG-086: Script run with no internet connectivity → gcloud API errors.  
NEG-087: Jenkins pipeline with wrong branch → Deploys wrong config.  
NEG-088: YAML file with wrong encoding (UTF-16) → Parse error.  
NEG-089: Job creation with legacy HTTP target (not HTTP) → Incompatible.  
NEG-090: Script run with conflicting environment variables → Undefined behavior.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Cron Schedule Boundaries (BND-001–020)

BND-001: `0 2 * * *` → Fires at exactly 02:00 Copenhagen time.  
BND-002: `*/15 * * * *` → Fires at :00, :15, :30, :45 every hour.  
BND-003: `0 1 * * 1` → Fires Monday 01:00 only.  
BND-004: `0 */4 * * *` → Fires at 00:00, 04:00, 08:00, 12:00, 16:00, 20:00.  
BND-005: DST spring forward (02:00 → 03:00) → Job at 02:00 skipped or adjusted.  
BND-006: DST fall back (03:00 → 02:00) → Job at 02:00 fires once or twice.  
BND-007: Midnight boundary → Jobs crossing midnight handled.  
BND-008: Year boundary (Dec 31 → Jan 1) → Jobs continue.  
BND-009: Leap year Feb 29 → Jobs fire correctly.  
BND-010: Month boundary (Jan 31 → Feb 1) → Jobs continue.  
BND-011: Consecutive daily jobs staggered by 15 min → No overlap.  
BND-012: 10-minute gap (DOA vs MGMT roles) → No overlap.  
BND-013: Fastest schedule (*/15 health check) → 96 executions/day.  
BND-014: Slowest schedule (weekly full sync) → 1 execution/week.  
BND-015: All daily syncs complete before weekly sync (Mon 01:00 vs dailies 02:00-04:35).  
BND-016: Weekly sync duration < 1 hour → Completes before dailies start.  
BND-017: Cron expression with 5 fields → Valid (minute hour dom month dow).  
BND-018: Cron expression with 6 fields → Invalid for Cloud Scheduler.  
BND-019: Cron expression with invalid minute (60) → Rejected.  
BND-020: Cron expression with invalid hour (25) → Rejected.

### Environment Boundaries (BND-021–035)

BND-021: Prod environment → Service URL `https://eds.opportunityplus.unops.org`.  
BND-022: Test environment → Service URL `https://eds.opportunityplus.test.unops.org`.  
BND-023: QA environment → Service URL `https://eds.opportunityplus.qa.unops.org`.  
BND-024: Dev environment → Service URL `https://eds.opportunityplus.dev.unops.org`.  
BND-025: Unknown environment → Script handles gracefully or errors.  
BND-026: Region `europe-west4` → Mapped to `europe-west3`.  
BND-027: Region `europe-west3` → Used directly (no mapping).  
BND-028: Region `us-central1` → Used directly.  
BND-029: Region not in mapping → Used as-is.  
BND-030: Project ID at max length → gcloud handles.  
BND-031: Project ID with hyphens → Valid GCP project ID.  
BND-032: Project ID with underscores → Invalid GCP project ID.  
BND-033: Service account name at max length → Created.  
BND-034: Service account already exists → Script handles (no duplicate).  
BND-035: IAM binding already exists → Script handles (idempotent).

### Job Count Boundaries (BND-036–050)

BND-036: 0 jobs in YAML → No jobs created.  
BND-037: 1 job in YAML → 1 job created.  
BND-038: 15 jobs in YAML (current) → All created.  
BND-039: 50 jobs in YAML → All created (within quota).  
BND-040: 500 jobs (approaching quota limit) → May hit GCP quota.  
BND-041: Job name = 1 character → Too short, may fail.  
BND-042: Job name = 500 characters → Too long, may fail.  
BND-043: Job name with special characters → May fail.  
BND-044: Job description = empty → Accepted.  
BND-045: Job description = max length → Accepted.  
BND-046: Endpoint path at max URL length → May fail.  
BND-047: Request body at max size → May fail.  
BND-048: Retry config: 0 retries → No retry on failure.  
BND-049: Retry config: 5 retries → 5 retry attempts.  
BND-050: Retry config: max retries → GCP limit respected.

### YAML Config Boundaries (BND-051–070)

BND-051: YAML with all required fields → Parsed correctly.  
BND-052: YAML with missing optional fields → Defaults applied.  
BND-053: YAML with extra unknown fields → Ignored.  
BND-054: YAML with empty job entry → Skipped or error.  
BND-055: YAML with duplicate job names → Last wins or error.  
BND-056: YAML with Unicode in description → Handled.  
BND-057: YAML with inline comments → Parsed correctly.  
BND-058: YAML with multiline strings → Parsed correctly.  
BND-059: YAML file at 0 bytes → Empty, no jobs.  
BND-060: YAML file at 1MB → Large but parseable.  
BND-061: YAML with tab indentation → May fail (YAML uses spaces).  
BND-062: YAML with mixed indentation → Parse error.  
BND-063: YAML with Windows line endings (CRLF) → Handled.  
BND-064: YAML with Unix line endings (LF) → Handled.  
BND-065: `appsettings.Development.json` → OAuthClientId read.  
BND-066: `appsettings.Production.json` → OAuthClientId read.  
BND-067: appsettings file missing → Script falls back or errors.  
BND-068: OAuthClientId empty in appsettings → Empty audience.  
BND-069: OAuthClientId with special characters → URL-encoded in token request.  
BND-070: Multiple appsettings files for same env → First found used.

### Extended Boundary (BND-071–090)

BND-071: Job name exactly at max GCP limit → Accepted or rejected.  
BND-072: Cron expression with minute 59 → Last minute of hour.  
BND-073: Cron expression with hour 23 → Last hour of day.  
BND-074: Cron expression with dow 7 (Sunday) → Valid.  
BND-075: Cron expression with dom 31 → Last day of month.  
BND-076: Cron expression with month 12 → December.  
BND-077: Endpoint path at max length → Accepted.  
BND-078: Request body at 0 bytes → Empty body accepted.  
BND-079: Request body at max Cloud Scheduler size → Accepted.  
BND-080: Timeout at 0 seconds → Invalid.  
BND-081: Timeout at max allowed → Accepted.  
BND-082: Retry count at 0 → No retries.  
BND-083: Retry count at 5 → Standard retries.  
BND-084: Retry count at 10 → May hit GCP limit.  
BND-085: Retry interval at minimum → 1 second.  
BND-086: Retry interval at maximum → 86400 seconds.  
BND-087: OAuthClientId at min length → Valid.  
BND-088: OAuthClientId at max length → Valid.  
BND-089: Project ID at min length → Valid.  
BND-090: Environment string at max length → Valid.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Script Logic (FUN-001–015)

FUN-001: Script accepts 3 required args: project-id, environment, region.  
FUN-002: Script accepts 2 optional args: oauth-client-id, config-file-path.  
FUN-003: Script prints usage when called without arguments.  
FUN-004: Script sets `gcloud config project` to provided project-id.  
FUN-005: Script enables Cloud Scheduler API via `gcloud services enable`.  
FUN-006: Script creates service account `cloud-scheduler-invoker`.  
FUN-007: Script grants `roles/run.invoker` to service account on Cloud Run service.  
FUN-008: Script reads YAML config and creates jobs.  
FUN-009: Script uses `SCHEDULER_COMMANDS` from Jenkins if available.  
FUN-010: Script falls back to hardcoded jobs if `SCHEDULER_COMMANDS` empty.  
FUN-011: Script deletes existing job before creating (describe → delete → create).  
FUN-012: Script handles "job not found" on describe (first deploy).  
FUN-013: Script maps scheduler region for `europe-west4`.  
FUN-014: Script reads OAuthClientId from appsettings JSON.  
FUN-015: Script outputs final job list via `gcloud scheduler jobs list`.

### gcloud Commands (FUN-016–030)

FUN-016: `gcloud scheduler jobs create http` with correct name.  
FUN-017: `gcloud scheduler jobs create http` with correct `--uri`.  
FUN-018: `gcloud scheduler jobs create http` with correct `--schedule`.  
FUN-019: `gcloud scheduler jobs create http` with correct `--time-zone`.  
FUN-020: `gcloud scheduler jobs create http` with `--http-method=POST`.  
FUN-021: `gcloud scheduler jobs create http` with OIDC token config.  
FUN-022: `gcloud scheduler jobs create http` with correct service account email.  
FUN-023: `gcloud scheduler jobs create http` with correct audience.  
FUN-024: `gcloud scheduler jobs create http` with correct `--location`.  
FUN-025: `gcloud scheduler jobs describe` checks existing job.  
FUN-026: `gcloud scheduler jobs delete` removes old job.  
FUN-027: `gcloud scheduler jobs list` shows all created jobs.  
FUN-028: `gcloud iam service-accounts create` creates invoker SA.  
FUN-029: `gcloud run services add-iam-policy-binding` grants access.  
FUN-030: `gcloud services enable cloudscheduler.googleapis.com` enables API.

### Jenkins Pipeline Logic (FUN-031–040)

FUN-031: Pipeline checks `SCHEDULER_ENABLED == true`.  
FUN-032: Pipeline skips scheduler stage when `SCHEDULER_ENABLED != true`.  
FUN-033: Pipeline reads YAML file from workspace.  
FUN-034: Pipeline parses each job entry from YAML.  
FUN-035: Pipeline builds gcloud command string per job.  
FUN-036: Pipeline exports `SCHEDULER_COMMANDS` for script.  
FUN-037: Pipeline passes correct project-id per environment.  
FUN-038: Pipeline passes correct region per environment.  
FUN-039: Pipeline uses correct credentials per environment.  
FUN-040: Pipeline logs scheduler setup output.

### Sync Endpoint Rules (FUN-041–050)

FUN-041: Each sync endpoint returns 200 on success.  
FUN-042: Each sync endpoint processes the requested data type.  
FUN-043: Health check returns 200 with uptime info.  
FUN-044: Config status returns current sync configuration.  
FUN-045: Full sync (`/api/sync/execute-all`) triggers all individual syncs.  
FUN-046: Sync endpoints accept POST with OIDC bearer token.  
FUN-047: Sync endpoints reject requests without valid token.  
FUN-048: Sync endpoints log execution start and completion.  
FUN-049: Sync endpoints handle partial data gracefully.  
FUN-050: Sync endpoints return error details on failure.

### Extended Functional (FUN-051–090)

FUN-051: Script constructs full service URL from base URL and endpoint path.  
FUN-052: Script validates project-id format before gcloud calls.  
FUN-053: Script validates environment against allowed values.  
FUN-054: Script validates region format before API calls.  
FUN-055: Script exits with non-zero on any gcloud failure.  
FUN-056: Script preserves existing jobs not in YAML when using incremental mode.  
FUN-057: gcloud command includes `--attempt-deadline` when specified.  
FUN-058: gcloud command includes `--retry-count` when specified.  
FUN-059: gcloud command includes `--message-body` when endpoint expects payload.  
FUN-060: Jenkins parses `jenkins-config-eds.yaml` for environment-specific values.  
FUN-061: Jenkins resolves `EDS_SERVICE_URL` per environment.  
FUN-062: Jenkins resolves `GCP_PROJECT_ID` per environment.  
FUN-063: Jenkins resolves `GCP_REGION` per environment.  
FUN-064: Jenkins sets `SCHEDULER_ENABLED` from config.  
FUN-065: Jenkins passes workspace path to setup script.  
FUN-066: Sync endpoint `/api/sync/execute/users` maps to users data source.  
FUN-067: Sync endpoint `/api/sync/execute/countries` maps to countries data source.  
FUN-068: Sync endpoint `/api/sync/execute-all` invokes all sync endpoints in order.  
FUN-069: Health endpoint `/health` returns JSON with status field.  
FUN-070: Config status endpoint returns last sync timestamps per data type.  
FUN-071: OIDC token audience matches EDS OAuth client ID.  
FUN-072: OIDC token includes correct `iss` claim.  
FUN-073: OIDC token includes correct `sub` (service account).  
FUN-074: EDS validates OIDC token signature.  
FUN-075: EDS validates OIDC token expiration.  
FUN-076: Region mapping applies only to scheduler location, not Cloud Run.  
FUN-077: Service account email format: `{name}@{project}.iam.gserviceaccount.com`.  
FUN-078: Job name format: `pao-external-data-{data-type}-{frequency}`.  
FUN-079: Cron schedule format: 5 fields (minute hour dom month dow).  
FUN-080: Timezone format: IANA (e.g., `Europe/Copenhagen`).  
FUN-081: YAML job entry requires: name, schedule, endpoint.  
FUN-082: YAML job entry optional: description, timeout, retry.  
FUN-083: Script reads config file path from argument or default.  
FUN-084: Script reads appsettings path relative to EDS project root.  
FUN-085: Jenkins workspace contains `ExternalDataService/deploy/` path.  
FUN-086: Jenkins workspace contains `cloud-scheduler-jobs.yaml`.  
FUN-087: gcloud `--location` uses mapped region when applicable.  
FUN-088: gcloud `--oidc-service-account-email` uses invoker SA.  
FUN-089: gcloud `--oidc-token-audience` uses OAuth client ID.  
FUN-090: Script idempotent: running twice produces same job set.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### Deploy + Verify (INT-001–015)

INT-001: Run setup script → All 15 jobs visible in Cloud Scheduler console.  
INT-002: Run setup script → Health check job fires within 15 minutes.  
INT-003: Run setup script → Service account exists with correct roles.  
INT-004: Run setup script twice → Idempotent (same result).  
INT-005: Deploy new EDS version → Scheduler jobs still point to correct URL.  
INT-006: Deploy to dev → Dev scheduler jobs active.  
INT-007: Deploy to qa → QA scheduler jobs active.  
INT-008: Deploy to test → Test scheduler jobs active.  
INT-009: Deploy to prod → Prod scheduler jobs active.  
INT-010: Jenkins pipeline success → All jobs created.  
INT-011: Jenkins pipeline failure → Partial jobs may exist (manual cleanup).  
INT-012: Job created → Manual trigger → Sync completes.  
INT-013: Job created → Wait for schedule → Auto-triggers.  
INT-014: Job triggers → EDS endpoint receives request.  
INT-015: Job triggers → OIDC token valid → Request authenticated.

### Sync Data Flow (INT-016–030)

INT-016: Users sync → User data updated in PAO database.  
INT-017: User profiles sync → Profile data updated.  
INT-018: User roles sync → Role assignments updated.  
INT-019: Countries sync → Country list updated.  
INT-020: Currencies sync → Currency list updated.  
INT-021: Engagements sync → BaseEngagement data updated.  
INT-022: Engagement partners sync → Partner links updated.  
INT-023: Partner agreements sync → Agreement data updated.  
INT-024: Org hierarchies sync → Org unit tree updated.  
INT-025: Entity user roles (DOA) sync → DOA roles updated.  
INT-026: Entity user roles (MGMT) sync → Management roles updated.  
INT-027: Exchange rates sync → Rate data updated.  
INT-028: Weekly full sync → All data types refreshed.  
INT-029: Health check → Returns current service status.  
INT-030: Config status → Returns sync configuration.

### Cross-Environment (INT-031–040)

INT-031: Dev scheduler → Only hits dev EDS.  
INT-032: QA scheduler → Only hits QA EDS.  
INT-033: Test scheduler → Only hits test EDS.  
INT-034: Prod scheduler → Only hits prod EDS.  
INT-035: Environment isolation → Dev jobs don't affect prod.  
INT-036: Credential isolation → Dev SA can't access prod.  
INT-037: URL isolation → Each env has unique service URL.  
INT-038: Concurrent deploys across environments → Independent.  
INT-039: Config differences between environments → Correct per env.  
INT-040: OAuthClientId different per environment → Correct per env.

### Error Recovery (INT-041–050)

INT-041: EDS down during scheduled job → Job marked failed, retried.  
INT-042: EDS recovers → Next scheduled execution succeeds.  
INT-043: Scheduler service down → Jobs queue and fire when restored.  
INT-044: Network error → Job retried per retry config.  
INT-045: Auth error → Job failed, investigated.  
INT-046: Partial sync failure → EDS logs error, next sync retries.  
INT-047: Database error during sync → EDS returns 500, job retried.  
INT-048: External API rate limit → Sync fails, retried later.  
INT-049: Cloud Scheduler quota exceeded → Jobs cannot be created.  
INT-050: Manual job deletion → Re-deploy recreates.

### Extended Integration (INT-051–090)

INT-051: Full Jenkins deploy → EDS build → Cloud Run deploy → Scheduler setup → All succeed.  
INT-052: Scheduler setup after Cloud Run deploy → Jobs point to new revision.  
INT-053: Scheduler setup before Cloud Run deploy → Jobs created, first run may 404.  
INT-054: Change YAML job order → Script creates jobs in YAML order.  
INT-055: Add new job to YAML → Re-run script creates new job.  
INT-056: Remove job from YAML → Re-run script deletes job.  
INT-057: Modify job schedule in YAML → Re-run script updates job.  
INT-058: Modify job endpoint in YAML → Re-run script updates job.  
INT-059: OAuthClientId change in appsettings → Re-run script uses new audience.  
INT-060: Service account recreated → IAM binding re-applied, jobs still work.  
INT-061: Cloud Run service recreated → Invoker binding still valid.  
INT-062: GCP project migrated → Script run with new project-id succeeds.  
INT-063: Region change in config → Script uses new region, jobs recreated.  
INT-064: Environment variable override in Jenkins → Script uses override.  
INT-065: Jenkins deploy with `scheduler_enabled: false` → No scheduler stage, EDS deployed.  
INT-066: Jenkins deploy with `scheduler_enabled: true` → Scheduler stage runs.  
INT-067: Manual gcloud job trigger → Same behavior as scheduled trigger.  
INT-068: Pause job in Cloud Scheduler → No triggers until resumed.  
INT-069: Resume paused job → Next scheduled run fires.  
INT-070: Disable job via gcloud → Job not triggered.  
INT-071: Re-enable job → Job triggers on next schedule.  
INT-072: Users sync → PAO User table updated.  
INT-073: Countries sync → PAO Country table updated.  
INT-074: Currencies sync → PAO Currency table updated.  
INT-075: Exchange rates sync → PAO ExchangeRate table updated.  
INT-076: Full sync → All sync endpoints invoked in sequence.  
INT-077: Health check during EDS startup → Returns 503 until ready.  
INT-078: Health check when EDS ready → Returns 200.  
INT-079: Config status when no syncs run → Returns empty or default timestamps.  
INT-080: Config status after sync → Returns last sync timestamp.  
INT-081: OIDC token from Cloud Scheduler → EDS accepts and processes.  
INT-082: OIDC token from different SA → EDS rejects 401.  
INT-083: Request without Authorization header → EDS rejects 401.  
INT-084: Request with invalid Bearer token → EDS rejects 401.  
INT-085: Request with expired token → EDS rejects 401.  
INT-086: Dev job triggers dev EDS → Prod EDS unaffected.  
INT-087: Prod job triggers prod EDS → Dev EDS unaffected.  
INT-088: Same job name in different projects → Independent.  
INT-089: Scheduler in region A, EDS in region B → Cross-region HTTP works.  
INT-090: End-to-end: Deploy → Wait for schedule → Verify data in PAO DB.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two scheduler jobs fire simultaneously → EDS handles both.  
CON-002: Health check and sync job fire simultaneously → Both succeed.  
CON-003: Two daily sync jobs overlap → Both endpoints independent.  
CON-004: Weekly full sync and daily sync overlap → Both run.  
CON-005: Script run from two terminals → Second run re-creates jobs.  
CON-006: Jenkins deploy during active scheduler job → Job may fail, next succeeds.  
CON-007: Concurrent Jenkins deploys for same env → Race condition.  
CON-008: Concurrent Jenkins deploys for different envs → Independent.  
CON-009: Scheduler job during EDS deployment → Job may fail, retried.  
CON-010: Multiple health checks in parallel → All return 200.  
CON-011: gcloud API rate limit during job creation → Queued or retried.  
CON-012: Service account creation + IAM binding race → Eventual consistency.  
CON-013: Job delete + recreate atomicity → Brief gap with no job.  
CON-014: Concurrent sync requests to same endpoint → EDS handles.  
CON-015: DB lock during sync → Sync waits or retries.  
CON-016: Multiple scheduler regions → Jobs independent per region.  
CON-017: GCP project-level quota shared → Jobs compete for quota.  
CON-018: OIDC token refresh during job execution → Token valid for duration.  
CON-019: Scheduler retry during concurrent job execution → May run twice.  
CON-020: Script + Jenkins both creating jobs → Last write wins.  
CON-021: YAML config update during script execution → Reads config at start.  
CON-022: Appsettings change during script execution → Reads at start.  
CON-023: Concurrent full sync and individual sync → Both database operations.  
CON-024: Connection pool under sync load → EDS pool manages.  
CON-025: Scheduler service failover → Jobs migrated to new scheduler instance.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: YAML parser extracts job name correctly.  
UNT-002: YAML parser extracts schedule correctly.  
UNT-003: YAML parser extracts endpoint correctly.  
UNT-004: YAML parser extracts timezone correctly.  
UNT-005: YAML parser handles missing optional fields.  
UNT-006: Region mapping `europe-west4` → `europe-west3`.  
UNT-007: Region mapping for non-mapped region → Uses original.  
UNT-008: Service URL construction per environment → Correct.  
UNT-009: Service account email construction → Correct format.  
UNT-010: OAuthClientId extraction from appsettings JSON.  
UNT-011: gcloud command construction for job creation → Correct flags.  
UNT-012: gcloud command for service account creation → Correct flags.  
UNT-013: gcloud command for IAM binding → Correct flags.  
UNT-014: Jenkins YAML parse → Extracts all 15 jobs.  
UNT-015: Jenkins command builder → Correct gcloud string.  
UNT-016: Cron expression `0 2 * * *` → Valid.  
UNT-017: Cron expression `*/15 * * * *` → Valid.  
UNT-018: Cron expression `0 1 * * 1` → Valid (Monday).  
UNT-019: Cron expression `0 */4 * * *` → Valid (every 4 hours).  
UNT-020: Environment string normalization (Dev/dev/DEV) → Consistent.  
UNT-021: Script usage message contains all required arguments.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Setup script execution < 2 minutes for all 15 jobs.  
PRF-002: Individual job creation < 5 seconds via gcloud.  
PRF-003: Job deletion < 3 seconds via gcloud.  
PRF-004: Health check endpoint response < 500ms.  
PRF-005: Sync endpoint response < 30 seconds per data type.  
PRF-006: Full sync endpoint < 10 minutes.  
PRF-007: OIDC token acquisition < 2 seconds.  
PRF-008: Jenkins scheduler stage < 5 minutes total.  
PRF-009: YAML parsing < 1 second.  
PRF-010: gcloud jobs list < 5 seconds.  
PRF-011: Health check 96 times/day → Negligible EDS load.  
PRF-012: 12 daily syncs in 2.5-hour window → All complete in window.  
PRF-013: Weekly full sync < 1 hour.  
PRF-014: Scheduler overhead per job < 100ms.  
PRF-015: Network round trip scheduler→EDS < 200ms.  
PRF-016: Concurrent daily syncs don't exceed EDS capacity.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: All 12 daily syncs in 2.5-hour window → All complete.  
LDT-002: Weekly full sync + daily syncs → Both complete without conflict.  
LDT-003: Health check 96/day for 30 days → All return 200.  
LDT-004: Deploy + scheduler setup under CI load → Completes within timeout.  
LDT-005: 50 concurrent scheduler jobs across environments → All succeed.  
LDT-006: EDS under sync load → Response times acceptable.  
LDT-007: Database under sync load → Connection pool handles.  
LDT-008: Recovery after EDS crash → Next scheduled jobs succeed.  
LDT-009: Recovery after database crash → Syncs resume.  
LDT-010: Recovery after Cloud Scheduler outage → Jobs fire when restored.

---

## Status: Ready for Implementation
