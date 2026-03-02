# E2E Test Scenarios - UNOPS Opportunity+ System

## Overview

This document provides a comprehensive mapping of all product requirements to end-to-end test scenarios. Each scenario maps to a specific Playwright spec file for execution.

**Last Updated:** 2026-02-13
**Total Scenarios:** 215+
**Coverage Target:** All user-facing features and workflows

---

## Requirement Sources

| # | Source Document | Module |
|---|----------------|--------|
| R1 | `tasks/workflow-submodule-integration/workflow-submodule-integration-prd.md` | Workflow & Approvals |
| R2 | `tasks/opportunity-ux/Opportunity Epics.md` | Opportunity Management |
| R3 | `docs/product-service-search-enhancement/product-service-search-enhancement.md` | Product & Service Search |
| R4 | `tasks/app-assistance-and-transcribe-feature/EXECUTIVE-SUMMARY.md` | AI Assistance & Transcribe |
| R5 | `docs/Development/crm-enhancement-implementation.md` | CRM Enhancement |
| R6 | `QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_PRD_TestCases.md` | Go/No-Go Decision |
| R7 | `docs/Security/README_RBAC_Implementation.md` | Security & Access Control |
| R8 | `docs/OAuth/` | Authentication |

---

## 1. Authentication & Authorization

**Playwright Spec:** `login.spec.ts`, `role-access-control.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| AUTH-001 | Login page displays email and password fields | R8 | High | Active |
| AUTH-002 | Successful login redirects to home/dashboard | R8 | Critical | Skipped (needs backend) |
| AUTH-003 | Invalid credentials show error message | R8 | High | Skipped (needs backend) |
| AUTH-004 | Required field validation on login form | R8 | High | Skipped (needs backend) |
| AUTH-005 | Password visibility toggle works | R8 | Medium | Skipped (needs backend) |
| AUTH-006 | Logout clears session and redirects to login | R8 | High | Active |
| AUTH-007 | Unauthenticated user redirected to login | R8 | Critical | Active |
| AUTH-008 | Administrator sees all sidebar menu items | R7 | Critical | Active |
| AUTH-009 | General User sees restricted sidebar items | R7 | Critical | Active |
| AUTH-010 | Viewer cannot see create/edit/delete buttons | R7 | Critical | Active |
| AUTH-011 | Role-based access for partners page | R7 | High | Active |
| AUTH-012 | Role-based access for contacts page | R7 | High | Active |
| AUTH-013 | Role-based access for interactions page | R7 | High | Active |
| AUTH-014 | Role-based access for opportunities page | R7 | High | Active |
| AUTH-015 | Role-based access for admin pages | R7 | High | Active |

---

## 2. Home & Dashboard

**Playwright Spec:** `home.spec.ts`, `dashboard.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| HOME-001 | Home page loads with dashboard | - | High | Active |
| HOME-002 | Dashboard displays widgets/panels | - | High | Active |
| HOME-003 | Welcome message displayed | - | Medium | Active |
| HOME-004 | Quick actions visible and functional | - | High | Active |
| HOME-005 | Recent activity section displays data | - | Medium | Active |
| HOME-006 | Dashboard refresh loads updated data | - | Medium | Active |
| HOME-007 | Dashboard is responsive on mobile | - | High | Active |
| HOME-008 | Empty state handled gracefully | - | Medium | Active |
| HOME-009 | Announcement banner displays when present | - | Low | Active |

---

## 3. Navigation & Layout

**Playwright Spec:** `navigation-tabs.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| NAV-001 | Desktop tabs display all navigation items | - | High | Active |
| NAV-002 | Active tab highlighted correctly | - | Medium | Active |
| NAV-003 | Tab click navigates to correct page | - | High | Active |
| NAV-004 | Mobile shows dropdown navigation | - | High | Active |
| NAV-005 | Mobile dropdown selection navigates correctly | - | High | Active |
| NAV-006 | Desktop tabs hidden on mobile | - | Medium | Active |
| NAV-007 | Mobile dropdown hidden on desktop | - | Medium | Active |
| NAV-008 | Disabled tabs are non-interactive | - | Low | Active |
| NAV-009 | Sidebar navigation links work correctly | - | High | Active |
| NAV-010 | Breadcrumbs display correct hierarchy | - | Medium | Active |

---

## 4. Partners

**Playwright Specs:** `partners.spec.ts`, `partner-item.spec.ts`, `partner-features.spec.ts`

### 4.1 Partner List

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| PTR-001 | Partners page displays header | - | High | Active |
| PTR-002 | New Partner button visible with permission | - | Critical | Active |
| PTR-003 | Export button visible with permission | - | High | Active |
| PTR-004 | Import button visible with permission | - | High | Active |
| PTR-005 | Partner listview renders cards | - | High | Active |
| PTR-006 | Search functionality filters partners | - | High | Active |
| PTR-007 | Click card navigates to partner detail | - | High | Active |
| PTR-008 | Empty state displays correctly | - | Medium | Active |
| PTR-009 | New Partner dialog opens | - | Critical | Skipped (QA-008) |
| PTR-010 | Partner list responsive on mobile | - | High | Active |

### 4.2 Partner Detail

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| PTR-011 | Partner detail page header displays | - | High | Active |
| PTR-012 | Partner name displayed correctly | - | High | Active |
| PTR-013 | Partner information panel visible | - | High | Active |
| PTR-014 | Edit button visible with permission | - | Critical | Active |
| PTR-015 | Delete button visible with permission | - | Critical | Active |
| PTR-016 | Edit dialog opens on click | - | High | Conditional |
| PTR-017 | Delete confirmation dialog opens | - | High | Conditional |
| PTR-018 | Contacts tab/section visible | - | High | Active |
| PTR-019 | Interactions section visible | - | High | Active |
| PTR-020 | Documents section visible with upload | - | High | Active |
| PTR-021 | Links section visible with add capability | - | High | Active |
| PTR-022 | Activity timeline displays events | - | Medium | Active |
| PTR-023 | Back navigation to partners list | - | High | Conditional |
| PTR-024 | Page responsive on mobile | - | High | Active |

### 4.3 Partner Features

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| PTR-025 | Partner Ecosystem view accessible (PNO-150) | R5 | High | Active |
| PTR-026 | Partner hierarchy navigation | R5 | High | Active |
| PTR-027 | Partner search in ecosystem | R5 | High | Active |
| PTR-028 | Partner Intelligence section (PNO-108) | R5 | High | Active |
| PTR-029 | AI insights available on partner | R4 | Medium | Active |
| PTR-030 | Take a Tour feature works (PNO-446) | - | Medium | Active |

### 4.4 Partner CRM Panels (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| PTR-031 | Engagements related panel displays | R5 | High | **NEW** |
| PTR-032 | Projects related panel displays | R5 | High | **NEW** |
| PTR-033 | Focal Points related panel displays | R5 | High | **NEW** |
| PTR-034 | Interaction Overview chart displays | R5 | Medium | **NEW** |
| PTR-035 | Panel expand to full view | R5 | Medium | **NEW** |
| PTR-036 | Add item from related panel | R5 | High | **NEW** |
| PTR-037 | Navigate to detail from panel item | R5 | High | **NEW** |
| PTR-038 | Mobile: related info in sidebar | R5 | High | **NEW** |
| PTR-039 | Responsive layout (2/3 + 1/3) | R5 | High | **NEW** |

---

## 5. Contacts

**Playwright Specs:** `contacts.spec.ts`, `contact-item.spec.ts`

### 5.1 Contact List

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| CON-001 | Contacts page displays header | - | High | Active |
| CON-002 | New Contact button visible with permission | - | Critical | Active |
| CON-003 | Business Card Scanner button visible | - | High | Active |
| CON-004 | Export/Import buttons visible | - | High | Active |
| CON-005 | Contact listview renders cards | - | High | Active |
| CON-006 | Search filters contacts | - | High | Active |
| CON-007 | Click card navigates to contact detail | - | High | Active |
| CON-008 | New Contact dialog opens | - | Critical | Skipped (QA-008) |
| CON-009 | No permission: New Contact hidden | - | Critical | Active |
| CON-010 | Contact list responsive on mobile | - | High | Active |

### 5.2 Contact Detail

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| CON-011 | Contact detail header displays | - | High | Active |
| CON-012 | Contact name and email displayed | - | High | Active |
| CON-013 | Partner association displayed | - | High | Active |
| CON-014 | Edit/delete buttons per permission | - | Critical | Active |
| CON-015 | Documents section with upload | - | High | Active |
| CON-016 | Links section with add capability | - | High | Active |
| CON-017 | Interactions section visible | - | High | Active |
| CON-018 | Page responsive on mobile | - | High | Active |

### 5.3 Contact CRM Panels (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| CON-019 | Recent Interactions panel displays | R5 | High | **NEW** |
| CON-020 | Involved Engagements panel displays | R5 | High | **NEW** |
| CON-021 | Navigate to interaction from panel | R5 | High | **NEW** |

---

## 6. Interactions

**Playwright Specs:** `interactions.spec.ts`, `interaction-item.spec.ts`

### 6.1 Interaction List

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| INT-001 | Interactions page displays header | - | High | Active |
| INT-002 | New Interaction button visible | - | Critical | Active |
| INT-003 | Create Opportunity button visible | - | High | Active |
| INT-004 | Export/Import buttons visible | - | High | Active |
| INT-005 | Interaction listview renders | - | High | Active |
| INT-006 | Search filters interactions | - | High | Active |
| INT-007 | Click navigates to interaction detail | - | High | Active |
| INT-008 | New Interaction dialog opens | - | Critical | Skipped (QA-008) |
| INT-009 | Create Opportunity dialog opens | - | High | Skipped (QA-008) |

### 6.2 Interaction Detail

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| INT-010 | Interaction detail header displays | - | High | Active |
| INT-011 | Type and date displayed | - | High | Active |
| INT-012 | Description section visible | - | High | Active |
| INT-013 | Participants/contacts listed | - | High | Active |
| INT-014 | Edit/delete buttons per permission | - | Critical | Active |
| INT-015 | Documents section visible | - | High | Active |
| INT-016 | Create Opportunity from interaction | - | High | Active |

---

## 7. Opportunities

**Playwright Specs:** `opportunities.spec.ts`, `opportunity-item.spec.ts`, `opportunity-creation.spec.ts`, `opportunity-sections.spec.ts`

### 7.1 Opportunity List

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-001 | Opportunities page displays header | - | High | Active |
| OPP-002 | New Opportunity button visible | - | Critical | Active |
| OPP-003 | Export button visible | - | High | Active |
| OPP-004 | Opportunity listview renders | - | High | Active |
| OPP-005 | Search filters opportunities | - | High | Active |
| OPP-006 | Click navigates to opportunity detail | - | High | Active |
| OPP-007 | New Opportunity dialog opens | - | Critical | Skipped (QA-008) |

### 7.2 Opportunity Creation

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-008 | Create from Partners (PNO-687) | R2 | Critical | Active |
| OPP-009 | Create from Interactions (PNO-688) | R2 | Critical | Active |
| OPP-010 | Create from Opportunity page (PNO-689) | R2 | Critical | Active |
| OPP-011 | Mandatory name field validation | R2 | High | Active |
| OPP-012 | Max length validation (256 chars) | R2 | High | Active |
| OPP-013 | General user cannot create | R7 | Critical | Active |

### 7.3 Opportunity Detail

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-014 | Opportunity detail header | - | High | Active |
| OPP-015 | Title and stage displayed | - | High | Active |
| OPP-016 | Metadata row (ID, manager, org unit) | - | High | Active |
| OPP-017 | Overview section visible | - | High | Active |
| OPP-018 | Workflow toolbar visible | - | High | Active |
| OPP-019 | Back navigation to list | - | High | Conditional |
| OPP-020 | Page responsive on mobile | - | High | Active |

### 7.4 Opportunity Sections - Team (PNO-979)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-021 | Team section layout displays | R2 | High | Active |
| OPP-022 | Opportunity Manager displayed | R2 | High | Active |
| OPP-023 | Collaborators list displayed | R2 | High | Active |
| OPP-024 | Org unit displayed | R2 | High | Active |
| OPP-025 | Country displayed | R2 | High | Active |
| OPP-026 | Permission-based team management | R7 | High | Active |

### 7.5 Opportunity Sections - WHY (PNO-692/938)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-027 | SDG section displays | R2 | High | Active |
| OPP-028 | Beneficiaries section displays | R2 | High | Active |
| OPP-029 | UN Framework section displays | R2 | High | Active |
| OPP-030 | Risk checklist displays | R2 | High | Active |
| OPP-031 | AI context awareness for WHY | R4 | Medium | Active |

### 7.6 Opportunity Sections - WHAT (PNO-700)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-032 | Scope section displays | R2 | High | Active |
| OPP-033 | Deliverables section displays | R2 | High | Active |
| OPP-034 | Initiative type displays | R2 | High | Active |
| OPP-035 | AI matching for products/services | R4 | Medium | Active |
| OPP-036 | Grant support section | R2 | Medium | Active |

### 7.7 Opportunity Sections - Budget & Schedule (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-037 | Budget section displays | R2 | High | **NEW** |
| OPP-038 | Budget values editable | R2 | High | **NEW** |
| OPP-039 | Budget total calculated correctly | R2 | High | **NEW** |
| OPP-040 | Schedule section displays | R2 | High | **NEW** |
| OPP-041 | Schedule milestones displayed | R2 | Medium | **NEW** |
| OPP-042 | WBS structure displayed | R2 | Medium | **NEW** |
| OPP-043 | AI-generated budget suggestions | R4 | Medium | **NEW** |
| OPP-044 | AI-generated schedule suggestions | R4 | Medium | **NEW** |

### 7.8 Opportunity Sections - Risk Register (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-045 | Risk register section displays | R2 | High | **NEW** |
| OPP-046 | Add risk manually | R2 | High | **NEW** |
| OPP-047 | Risk details: category, likelihood, impact | R2 | High | **NEW** |
| OPP-048 | AI-suggested risks displayed | R4 | Medium | **NEW** |
| OPP-049 | Accept/reject AI risk suggestion | R4 | Medium | **NEW** |
| OPP-050 | Risk list sortable/filterable | R2 | Medium | **NEW** |

### 7.9 Opportunity Sections - DST Profiling (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-051 | DST section displays | R2 | High | **NEW** |
| OPP-052 | DST analysis trigger | R2 | High | **NEW** |
| OPP-053 | DST results displayed | R2 | High | **NEW** |
| OPP-054 | Profile report generation | R2 | Medium | **NEW** |
| OPP-055 | Suggested risks from DST | R2 | Medium | **NEW** |
| OPP-056 | Suggested issues from DST | R2 | Medium | **NEW** |
| OPP-057 | Lessons learned from DST | R2 | Medium | **NEW** |

### 7.10 Opportunity Statement (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-058 | Opportunity Statement section displays | R2, R6 | High | **NEW** |
| OPP-059 | Generate statement from data | R2 | High | **NEW** |
| OPP-060 | Statement editable by OM | R2 | High | **NEW** |
| OPP-061 | AI-improved statement | R4 | Medium | **NEW** |
| OPP-062 | Statement required before Go submission | R6 | Critical | **NEW** |
| OPP-063 | Concept Note generation | R2 | Medium | **NEW** |

### 7.11 Opportunity Documents & Related Items (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OPP-064 | Documents section displays | R2 | High | Active |
| OPP-065 | Upload document to opportunity | R2 | High | **NEW** |
| OPP-066 | Related entities section | R2 | High | Active |
| OPP-067 | Collaboration section | R2 | High | **NEW** |
| OPP-068 | Analysis section | R2 | Medium | **NEW** |

---

## 8. Workflow & Go/No-Go Decision

**Playwright Specs:** `workflow.spec.ts`, `go-decision.spec.ts`

### 8.1 Workflow Display

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| WF-001 | Workflow component visible on opportunity | R1 | Critical | Scaffolded |
| WF-002 | Current stage displayed in stepper | R1 | Critical | Scaffolded |
| WF-003 | Available actions shown per permissions | R1 | Critical | Scaffolded |
| WF-004 | "In Workflow" indicator when pending | R1, R6 | High | Scaffolded |
| WF-005 | Stage history table displays | R1 | High | Scaffolded |
| WF-006 | Stage history shows user, timestamp, action | R1 | High | Scaffolded |
| WF-007 | Approvers tab visible when in workflow | R1 | High | Scaffolded |

### 8.2 Workflow Actions

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| WF-008 | OM Submit for Go - approval initiated | R1, R6 | Critical | Scaffolded |
| WF-009 | OM Submit for No Go - approval initiated | R1, R6 | Critical | Scaffolded |
| WF-010 | DOA Approve - stage changes to GO | R1, R6 | Critical | Scaffolded |
| WF-011 | DOA Reject - stage unchanged, reset | R1, R6 | Critical | Scaffolded |
| WF-012 | OM Recall - returns to I&P | R1, R6 | High | Scaffolded |
| WF-013 | OM Cancel - CANCELLED/Closed | R6 | High | Scaffolded |
| WF-014 | OM Reopen from Cancelled | R6 | High | Scaffolded |
| WF-015 | OM Reopen from No-Go | R1 | High | Scaffolded |
| WF-016 | Comment required for Submit/Reject/Recall | R1, R6 | High | Scaffolded |
| WF-017 | GO stage is final (no transitions) | R1 | High | Scaffolded |

### 8.3 Workflow Permissions

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| WF-018 | Only OM can submit/cancel/reopen | R1, R6 | Critical | Scaffolded |
| WF-019 | Only DOA can approve/reject | R1, R6 | Critical | Scaffolded |
| WF-020 | Collaborator cannot submit/cancel | R6 | Critical | Scaffolded |
| WF-021 | Read-only during approval workflow | R1, R6 | High | Scaffolded |

### 8.4 Go Decision Validation

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| WF-022 | Mandatory fields complete - Submit enabled | R6 | Critical | Scaffolded |
| WF-023 | Mandatory fields missing - Submit disabled | R6 | Critical | Scaffolded |
| WF-024 | Statement required before submission | R6 | Critical | Scaffolded |
| WF-025 | DOA correctly identified | R6 | High | Scaffolded |
| WF-026 | Pre-submission checklist enforced | R2, R6 | High | **NEW** |

### 8.5 Go Decision E2E Workflows

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| WF-027 | Full path: Submit → Approve → GO | R1, R6 | Critical | Env-gated |
| WF-028 | Full path: Submit → Reject → NO GO | R1, R6 | Critical | Env-gated |
| WF-029 | Cancel → Reopen cycle | R6 | High | Env-gated |
| WF-030 | Status Active after Go | R1, R6 | High | Env-gated |
| WF-031 | Status Closed after No Go/Cancel | R1, R6 | High | Env-gated |

---

## 9. Product & Service Search

**Playwright Spec:** `product-service-search.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| PSS-001 | Add Product/Service dialog opens | R3 | High | Scaffolded |
| PSS-002 | Search mode: text search (min 2 chars) | R3 | High | Scaffolded |
| PSS-003 | Results grouped by level (L0-L4) | R3 | High | Scaffolded |
| PSS-004 | Select item from search results | R3 | High | Scaffolded |
| PSS-005 | Browse mode: cascading dropdowns | R3 | High | Scaffolded |
| PSS-006 | Switch between Search and Browse | R3 | High | Scaffolded |
| PSS-007 | Breadcrumb path in results | R3 | Medium | Scaffolded |
| PSS-008 | "Has Sub-levels" badge displayed | R3 | Medium | Scaffolded |
| PSS-009 | "Most Specific Level" badge displayed | R3 | Medium | Scaffolded |
| PSS-010 | Selection at any hierarchy level | R3 | High | Scaffolded |
| PSS-011 | Keyboard navigation and accessibility | R3 | High | Scaffolded |
| PSS-012 | Scenario: known item search | R3 | High | Scaffolded |

---

## 10. AI Assistant & Transcribe

**Playwright Spec:** `ai-assistant.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| AI-001 | AI panel toggle visibility | R4 | High | Scaffolded |
| AI-002 | Send prompt and receive response | R4 | High | Scaffolded |
| AI-003 | Content rendering (markdown) | R4 | High | Scaffolded |
| AI-004 | Session management | R4 | Medium | Scaffolded |
| AI-005 | Context awareness per section | R4 | High | Scaffolded |
| AI-006 | Quick actions (Explain, Required) | R4 | High | Scaffolded |
| AI-007 | Document upload for autofill | R4 | High | Scaffolded |
| AI-008 | Preview and accept autofill | R4 | High | Scaffolded |
| AI-009 | "Improve this statement" action | R4 | Medium | Scaffolded |
| AI-010 | Transcribe file upload | R4 | High | Scaffolded |
| AI-011 | AI guidance display per section | R4 | Medium | Scaffolded |
| AI-012 | Error handling for AI failures | R4 | Medium | Scaffolded |
| AI-013 | Accessibility (keyboard, ARIA) | R4 | High | Scaffolded |

---

## 11. Document Management

**Playwright Spec:** `document-management.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| DOC-001 | Document list displays on entity | - | High | Scaffolded |
| DOC-002 | Upload document with file picker | - | High | Scaffolded |
| DOC-003 | Download document | - | High | Scaffolded |
| DOC-004 | Delete document | - | High | Scaffolded |
| DOC-005 | Google Drive integration | - | Medium | Scaffolded |
| DOC-006 | Add link to document | - | High | Scaffolded |
| DOC-007 | Cross-entity document view | - | Medium | Scaffolded |
| DOC-008 | Error handling for upload failures | - | Medium | Scaffolded |
| DOC-009 | Document accessibility | - | Medium | Scaffolded |

---

## 12. Admin Features

**Playwright Specs:** `admin-features.spec.ts`, `admin-entity-config.spec.ts`, `admin-translation-workbench.spec.ts`, `user-management.spec.ts`

### 12.1 Entity Configuration

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| ADM-001 | Entity Manager page loads | - | High | Scaffolded |
| ADM-002 | Entity configuration editing | - | High | Scaffolded |
| ADM-003 | Entity Artifact CRUD | - | High | Scaffolded |
| ADM-004 | Bulk artifact update | - | Medium | Scaffolded |

### 12.2 Translation Workbench

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| ADM-005 | Translation page loads | - | High | Scaffolded |
| ADM-006 | Translation table displays | - | High | Scaffolded |
| ADM-007 | Search/filter translations | - | High | Scaffolded |
| ADM-008 | Edit translation inline | - | High | Scaffolded |
| ADM-009 | Language switching (en/fr/es/pt) | - | High | Scaffolded |

### 12.3 User Management

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| ADM-010 | User list displays | - | High | Scaffolded |
| ADM-011 | Search/filter users | - | High | Scaffolded |
| ADM-012 | User CRUD operations | - | High | Scaffolded |
| ADM-013 | Role assignment | - | Critical | Scaffolded |
| ADM-014 | Permission matrix display | - | High | Scaffolded |

### 12.4 Partner Tree (Admin)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| ADM-015 | Partner tree displays hierarchy | - | High | Scaffolded |
| ADM-016 | Tree node navigation | - | High | Scaffolded |
| ADM-017 | Node CRUD operations | - | High | Scaffolded |
| ADM-018 | Tree search functionality | - | High | Scaffolded |
| ADM-019 | Detail panel for selected node | - | High | Scaffolded |

### 12.5 Entity Artifacts (NEW)

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| ADM-020 | Entity artifacts page loads | - | High | **NEW** |
| ADM-021 | List artifacts by entity type | - | High | **NEW** |
| ADM-022 | Create new artifact | - | High | **NEW** |
| ADM-023 | Edit artifact properties | - | High | **NEW** |
| ADM-024 | Delete artifact | - | High | **NEW** |
| ADM-025 | Bulk update artifacts | - | Medium | **NEW** |

---

## 13. Comments

**Playwright Spec:** `comments.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| CMT-001 | Comment list displays on entity | - | High | Scaffolded |
| CMT-002 | Add new comment | - | High | Scaffolded |
| CMT-003 | Edit own comment | - | High | Scaffolded |
| CMT-004 | Delete own comment | - | High | Scaffolded |
| CMT-005 | Comments on partners | - | High | Scaffolded |
| CMT-006 | Comments on opportunities | - | High | Scaffolded |
| CMT-007 | Comment validation (empty, max length) | - | Medium | Scaffolded |

---

## 14. Import/Export

**Playwright Spec:** `import-export.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| IMP-001 | CSV import flow | - | High | Scaffolded |
| IMP-002 | Google Sheets import | - | Medium | Scaffolded |
| IMP-003 | Manual data entry | - | High | Scaffolded |
| IMP-004 | Duplicate detection | - | High | Scaffolded |
| IMP-005 | Export to CSV | - | High | Scaffolded |
| IMP-006 | Error handling for invalid data | - | High | Scaffolded |

---

## 15. oUP Integration

**Playwright Spec:** `oup-integration.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| OUP-001 | Integration flow triggers | - | High | Scaffolded |
| OUP-002 | Field mapping display | - | High | Scaffolded |
| OUP-003 | High-risk mapping indicators | - | Medium | Scaffolded |
| OUP-004 | Deep linking to oUP | - | Medium | Scaffolded |
| OUP-005 | Error handling for integration failures | - | High | Scaffolded |

---

## 16. Search & Filtering

**Playwright Spec:** `search-listviews.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| SRC-001 | Global search box accessible | - | High | Active |
| SRC-002 | Text search returns results | - | High | Active |
| SRC-003 | Partial match works | - | High | Active |
| SRC-004 | Case-insensitive search | - | High | Active |
| SRC-005 | Clear search resets results | - | High | Active |
| SRC-006 | No results state | - | Medium | Active |
| SRC-007 | Filter by status | - | High | Active |
| SRC-008 | Sort ascending/descending | - | High | Active |
| SRC-009 | Pagination controls | - | High | Active |
| SRC-010 | Export to CSV | - | High | Active |

---

## 17. Form Validation

**Playwright Spec:** `form-validation.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| FRM-001 | Required field validation | - | High | Active |
| FRM-002 | Email format validation | - | High | Active |
| FRM-003 | Number field validation | - | High | Active |
| FRM-004 | Date field validation | - | High | Active |
| FRM-005 | Validation errors display | - | High | Active |
| FRM-006 | Submit disabled when invalid | - | High | Active |
| FRM-007 | Errors clear on correction | - | Medium | Active |

---

## 18. Profile & Settings

**Playwright Spec:** `profile-settings.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| PRF-001 | Profile menu accessible | - | High | Scaffolded |
| PRF-002 | Profile dialog displays | - | High | Scaffolded |
| PRF-003 | Language selector works | - | High | Scaffolded |
| PRF-004 | Org unit selector works | - | High | Scaffolded |
| PRF-005 | Settings saved successfully | - | High | Scaffolded |

---

## 19. Cross-Entity Workflows (NEW)

**Playwright Spec:** `cross-entity-workflows.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| CEW-001 | Partner → Contacts tab → Contact detail | - | High | **NEW** |
| CEW-002 | Partner → Opportunities tab → Opportunity detail | - | High | **NEW** |
| CEW-003 | Partner → Interactions tab → Interaction detail | - | High | **NEW** |
| CEW-004 | Contact → Partner link → Partner detail | - | High | **NEW** |
| CEW-005 | Interaction → Create Opportunity → Opportunity detail | R2 | High | **NEW** |
| CEW-006 | Opportunity → Partners section → Partner detail | - | High | **NEW** |
| CEW-007 | Opportunity → Contacts section → Contact detail | - | High | **NEW** |
| CEW-008 | Search → Result click → Entity detail | - | High | **NEW** |
| CEW-009 | Dashboard → Quick action → Create entity | - | High | **NEW** |
| CEW-010 | Sidebar → Navigate between modules | - | High | **NEW** |

---

## 20. Data Persistence & Integrity (NEW)

**Playwright Spec:** `data-persistence.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| DPR-001 | Create partner → Appears in list | - | Critical | **NEW** |
| DPR-002 | Edit partner → Changes saved | - | Critical | **NEW** |
| DPR-003 | Create contact → Appears in list | - | Critical | **NEW** |
| DPR-004 | Edit contact → Changes saved | - | Critical | **NEW** |
| DPR-005 | Create interaction → Appears in list | - | Critical | **NEW** |
| DPR-006 | Create opportunity → Appears in list | - | Critical | **NEW** |
| DPR-007 | Edit opportunity sections → Data persisted | - | Critical | **NEW** |
| DPR-008 | Page refresh retains data | - | High | **NEW** |
| DPR-009 | Concurrent edits handled gracefully | - | High | **NEW** |
| DPR-010 | Unsaved changes warning on navigation | - | Medium | **NEW** |

---

## 21. Accessibility (NEW)

**Playwright Spec:** `accessibility.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| A11Y-001 | Keyboard navigation through main pages | - | High | **NEW** |
| A11Y-002 | Focus management after dialog close | - | High | **NEW** |
| A11Y-003 | ARIA labels on form controls | - | High | **NEW** |
| A11Y-004 | Screen reader announcements | - | Medium | **NEW** |
| A11Y-005 | Color contrast compliance | - | Medium | **NEW** |
| A11Y-006 | Tab order logical on all pages | - | High | **NEW** |

---

## 22. Funding Agreements (NEW)

**Playwright Spec:** `funding-agreements.spec.ts`

| ID | Scenario | Req | Priority | Status |
|----|----------|-----|----------|--------|
| FA-001 | Funding Agreements tab on partner detail | R2 | High | **NEW** |
| FA-002 | Agreements list displays | R2 | High | **NEW** |
| FA-003 | Agreement detail view | R2 | High | **NEW** |
| FA-004 | Add new agreement | R2 | High | **NEW** |
| FA-005 | Link agreement to opportunity | R2 | High | **NEW** |
| FA-006 | Agreement metadata displayed | R2 | Medium | **NEW** |

---

## Summary Statistics

| Category | Total Scenarios | Active | Scaffolded | New | Env-Gated |
|----------|----------------|--------|------------|-----|-----------|
| Authentication & Authorization | 15 | 12 | 3 | 0 | 0 |
| Home & Dashboard | 9 | 9 | 0 | 0 | 0 |
| Navigation & Layout | 10 | 10 | 0 | 0 | 0 |
| Partners | 39 | 24 | 1 | 9 | 0 |
| Contacts | 21 | 18 | 0 | 3 | 0 |
| Interactions | 16 | 14 | 2 | 0 | 0 |
| Opportunities | 68 | 36 | 0 | 30 | 0 |
| Workflow & Go Decision | 31 | 0 | 21 | 1 | 5 |
| Product & Service Search | 12 | 0 | 12 | 0 | 0 |
| AI Assistant & Transcribe | 13 | 0 | 13 | 0 | 0 |
| Document Management | 9 | 0 | 9 | 0 | 0 |
| Admin Features | 25 | 0 | 19 | 6 | 0 |
| Comments | 7 | 0 | 7 | 0 | 0 |
| Import/Export | 6 | 0 | 6 | 0 | 0 |
| oUP Integration | 5 | 0 | 5 | 0 | 0 |
| Search & Filtering | 10 | 10 | 0 | 0 | 0 |
| Form Validation | 7 | 7 | 0 | 0 | 0 |
| Profile & Settings | 5 | 0 | 5 | 0 | 0 |
| Cross-Entity Workflows | 10 | 0 | 0 | 10 | 0 |
| Data Persistence | 10 | 0 | 0 | 10 | 0 |
| Accessibility | 6 | 0 | 0 | 6 | 0 |
| Funding Agreements | 6 | 0 | 0 | 6 | 0 |
| **TOTAL** | **340** | **140** | **103** | **81** | **5** |

---

## Playwright Test File Inventory

| Spec File | Scenarios | Status |
|-----------|-----------|--------|
| `login.spec.ts` | AUTH-001 to AUTH-007 | Partial |
| `role-access-control.spec.ts` | AUTH-008 to AUTH-015 | Active |
| `home.spec.ts` | HOME-001 to HOME-009 | Active |
| `dashboard.spec.ts` | HOME-001 to HOME-008 | Active |
| `navigation-tabs.spec.ts` | NAV-001 to NAV-010 | Active |
| `partners.spec.ts` | PTR-001 to PTR-010 | Active |
| `partner-item.spec.ts` | PTR-011 to PTR-024 | Active |
| `partner-features.spec.ts` | PTR-025 to PTR-030 | Active |
| `crm-related-panels.spec.ts` | PTR-031 to PTR-039, CON-019 to CON-021 | **NEW** |
| `contacts.spec.ts` | CON-001 to CON-010 | Active |
| `contact-item.spec.ts` | CON-011 to CON-018 | Active |
| `interactions.spec.ts` | INT-001 to INT-009 | Active |
| `interaction-item.spec.ts` | INT-010 to INT-016 | Active |
| `opportunities.spec.ts` | OPP-001 to OPP-007 | Active |
| `opportunity-item.spec.ts` | OPP-014 to OPP-020 | Active |
| `opportunity-creation.spec.ts` | OPP-008 to OPP-013 | Active |
| `opportunity-sections.spec.ts` | OPP-021 to OPP-036 | Active |
| `opportunity-budget-schedule.spec.ts` | OPP-037 to OPP-044 | **NEW** |
| `opportunity-risk-register.spec.ts` | OPP-045 to OPP-050 | **NEW** |
| `opportunity-dst.spec.ts` | OPP-051 to OPP-057 | **NEW** |
| `opportunity-statement.spec.ts` | OPP-058 to OPP-063 | **NEW** |
| `workflow.spec.ts` | WF-001 to WF-007 | Scaffolded |
| `go-decision.spec.ts` | WF-008 to WF-031 | Env-gated |
| `product-service-search.spec.ts` | PSS-001 to PSS-012 | Scaffolded |
| `ai-assistant.spec.ts` | AI-001 to AI-013 | Scaffolded |
| `document-management.spec.ts` | DOC-001 to DOC-009 | Scaffolded |
| `admin-features.spec.ts` | ADM-001 to ADM-004 | Scaffolded |
| `admin-entity-config.spec.ts` | ADM-001 to ADM-004 | Scaffolded |
| `admin-translation-workbench.spec.ts` | ADM-005 to ADM-009 | Scaffolded |
| `user-management.spec.ts` | ADM-010 to ADM-014 | Scaffolded |
| `partner-tree.spec.ts` | ADM-015 to ADM-019 | Scaffolded |
| `entity-artifacts.spec.ts` | ADM-020 to ADM-025 | **NEW** |
| `comments.spec.ts` | CMT-001 to CMT-007 | Scaffolded |
| `import-export.spec.ts` | IMP-001 to IMP-006 | Scaffolded |
| `oup-integration.spec.ts` | OUP-001 to OUP-005 | Scaffolded |
| `search-listviews.spec.ts` | SRC-001 to SRC-010 | Active |
| `form-validation.spec.ts` | FRM-001 to FRM-007 | Active |
| `profile-settings.spec.ts` | PRF-001 to PRF-005 | Scaffolded |
| `cross-entity-workflows.spec.ts` | CEW-001 to CEW-010 | **NEW** |
| `data-persistence.spec.ts` | DPR-001 to DPR-010 | **NEW** |
| `accessibility.spec.ts` | A11Y-001 to A11Y-006 | **NEW** |
| `funding-agreements.spec.ts` | FA-001 to FA-006 | **NEW** |
| `jira-requirements.spec.ts` | Various JIRA tickets | Active |
| `seed.spec.ts` | Test data seeding | Active |
