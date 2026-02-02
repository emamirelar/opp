# QA Credentials Request

**Request Date:** February 2, 2026  
**Requested By:** QA Team  
**Priority:** High  
**Related Issue:** QA-014, QA-015

---

## Summary

QA has created **34 automated integration tests** for the Opportunity+ to oUP (oneUNOPS Projects) integration. These tests are currently **BLOCKED** pending access credentials.

---

## Credentials Required

### 1. oUP Test Environment Access

| Credential | Purpose | Example Format |
|------------|---------|----------------|
| `OUP_BASE_URL` | oUP test environment URL | `https://projects-test.unops.org` |
| `OUP_API_URL` | oUP API endpoint | `https://api-projects-test.unops.org` |
| `OUP_USERNAME` | Test user login | `qa-test-user@unops.org` |
| `OUP_PASSWORD` | Test user password | `********` |

**Required Permissions for Test User:**
- View engagements
- Create engagements (for verification)
- Access EAC Survey
- Access Risk Register
- Navigate to engagement details

### 2. Email Testing Access

| Credential | Purpose | Example Format |
|------------|---------|----------------|
| `EMAIL_HOST` | SMTP/IMAP server | `mail.unops.org` or `imap.gmail.com` |
| `EMAIL_PORT` | Mail server port | `993` (IMAP) or `587` (SMTP) |
| `EMAIL_USERNAME` | Email account | `qa-notifications@unops.org` |
| `EMAIL_PASSWORD` | Email credentials | `********` |
| `EMAIL_TLS` | TLS enabled | `true` |

### 3. Test User Email Accounts

| Account | Role | Purpose |
|---------|------|---------|
| `OPP_MANAGER_EMAIL` | Opportunity Manager | Receives workflow notifications |
| `DOA2_EMAIL` | DoA Level 2 Approver | Receives approval requests |
| `BD_EMAIL` | Business Developer | Receives engagement notifications |

**Note:** These can be shared test accounts or aliases that QA can access to verify email content.

### 4. Optional: Pub/Sub Monitoring

| Credential | Purpose |
|------------|---------|
| `PUBSUB_PROJECT_ID` | Google Cloud project ID |
| `PUBSUB_SUBSCRIPTION_NAME` | Subscription for monitoring |

---

## Why These Are Needed

### Tests Blocked (34 total)

| Category | Tests | Blocking Credential |
|----------|-------|---------------------|
| Integration Flow | 4 | oUP access |
| Field Mapping | 8 | oUP access |
| High-Risk Mapping | 4 | oUP access |
| Email Notifications | 4 | Email access |
| Deep Linking | 2 | oUP access (1 prod-only) |
| Idempotency | 3 | oUP access |
| Error Handling | 3 | oUP access |
| Edge Cases | 4 | oUP access |
| **TOTAL** | **34** | - |

### Business Value

- **Integration validation:** Ensure data flows correctly from Opportunity+ to oUP
- **Field mapping verification:** Confirm all 30+ fields map correctly
- **High-risk compliance:** Verify risk checklist items create EAC Survey and Risk Register entries
- **Notification testing:** Ensure PE, DoA2, and BD receive correct emails
- **Regression prevention:** Automated tests catch integration breaks early

---

## Security Considerations

1. **Test environment only:** No production credentials needed
2. **Read-mostly operations:** Tests primarily verify data, minimal writes
3. **Isolated test data:** Tests use dedicated test opportunities
4. **Credential storage:** Will use `.env` file (gitignored) or CI secrets

---

## Requested Actions

### IT Team

- [ ] Provide oUP test environment URL
- [ ] Create QA test user account in oUP test environment
- [ ] Grant required permissions to test user
- [ ] Provide API endpoint URL

### Email Admin

- [ ] Create or designate shared QA email account
- [ ] Provide IMAP/SMTP credentials
- [ ] Ensure account can receive Opportunity+ notifications

### DevOps

- [ ] Create test accounts for OPP_MANAGER, DOA2, BD roles
- [ ] Configure email aliases if needed
- [ ] (Optional) Provide Pub/Sub monitoring access

---

## Configuration Location

Once credentials are received, they will be added to:

```
QA Tests/Playwright Tests/.env
```

Template already exists at:
```
QA Tests/Playwright Tests/.env.example
```

---

## Contact

**QA Team Contact:** [QA Team]  
**Ticket/Issue:** QA-014  
**Test File:** `oup-integration.spec.ts`

---

## Timeline

| Milestone | Target |
|-----------|--------|
| Credentials received | ASAP |
| Test configuration | Same day |
| First test execution | 1 day after credentials |
| Full suite validation | 2-3 days after credentials |

---

*Once credentials are provided, QA will execute all 34 integration tests and report results within 48 hours.*
